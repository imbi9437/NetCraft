using _Project.Script.Generic;
using _Project.Script.Manager;
using _Project.Script.EventStruct;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 사운드 재생 관리 시스템
/// 
/// [기능]
/// - AudioMixer 그룹별 사운드 라우팅 (Music, SFX, UI 등)
/// - ObjectPooling을 통한 AudioSource 재사용으로 메모리 최적화
/// - 재생 완료 시 자동 회수 (non-loop 사운드)
/// 
/// [사용법 1: EventHub (권장)]
/// EventHub.Instance.RaiseEvent(new RequestPlaySoundEvent
/// {
///     id = "SFX_Attack",              // 등록된 ID 또는 Resources 경로
///     position = transform.position,
///     volume = 0.8f,
///     spatialBlend = 1f,              // 0=2D, 1=3D
///     mixerGroupName = "SFXVolum"     // AudioMixer 그룹 이름
/// });
/// 
/// [사용법 2: 직접 호출]
/// SoundManager.Instance.Play(new SoundManager.SoundRequest { ... });
/// 
/// [에디터 설정]
/// - Sound Entries: 자주 쓰는 사운드 등록 (id, clip, resourcesPath)
/// - Mixer Groups: AudioMixerGroup들을 Drag & Drop
/// - Default Mixer Group: 기본 출력 그룹 설정
/// - Warmup Count: 미리 생성할 AudioSource 개수 (기본 8)
/// </summary>
public class SoundManager : MonoSingleton<SoundManager>
{
    [System.Serializable]
    private class SoundEntry
    {
        public string id;                 // 사운드 식별자 (외부 파라미터용)
        public AudioClip clip;            // 직접 할당 클립
        public string resourcesPath;      // Resources 경로 (선택)
        public string mixerGroupName;     // 기본 출력 그룹명 (선택)
        public float defaultVolume = 1f;  // 기본 볼륨
        public bool defaultLoop = false;  // 기본 루프 여부
    }

    public struct SoundRequest
    {
        public string id;                 // 등록된 id 또는 Resources 경로 키로 사용
        public AudioClip clip;            // 직접 재생용 (id 미지정 시 사용)
        public Vector3 position;          // 3D 위치
        public Transform parent;          // 붙일 부모(선택)
        public float volume;              // 볼륨(0~1)
        public float pitch;               // 피치
        public float spatialBlend;        // 0=2D, 1=3D
        public bool loop;                 // 루프 여부
        public string mixerGroupName;     // 출력 그룹명
    }

    [Header("Registry")]
    [Tooltip("사운드 등록해주세요")]
    [SerializeField] private List<SoundEntry> soundEntries = new List<SoundEntry>();

    [Header("Mixer Routing")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private List<AudioMixerGroup> mixerGroups = new List<AudioMixerGroup>();
    [SerializeField] private AudioMixerGroup defaultMixerGroup;

    [Header("Pool Parent & Warmup")]
    [SerializeField] private Transform poolParent;
    [SerializeField] private int warmupCount = 8;

    private readonly Dictionary<string, SoundEntry> idToEntry = new Dictionary<string, SoundEntry>();
    private readonly Dictionary<string, AudioMixerGroup> nameToMixer = new Dictionary<string, AudioMixerGroup>();

    protected override void Awake()
    {
        base.Awake();

        idToEntry.Clear();
        foreach (var e in soundEntries)
        {
            if (string.IsNullOrEmpty(e.id)) continue;
            if (idToEntry.ContainsKey(e.id) == false)
                idToEntry.Add(e.id, e);
            else
                idToEntry[e.id] = e;
        }

        nameToMixer.Clear();
        foreach (var g in mixerGroups)
        {
            if (g == null || g.audioMixer == null) continue;
            if (nameToMixer.ContainsKey(g.name) == false)
                nameToMixer.Add(g.name, g);
        }

        // 풀 워밍업 (커스텀 키 방식)
        ObjectPooling.Warmup(
            key: "__AudioSourcePool__",
            factory: CreatePooledAudio,
            count: warmupCount,
            parent: poolParent
        );

        // 이벤트 구독 (단일 진입점 강제)
        EventHub.Instance?.RegisterEvent<RequestPlaySoundEvent>(OnRequestPlaySound);
        EventHub.Instance?.RegisterEvent<RequestChangeVolumeEvent>(OnRequestChangeVolume);

        // AudioMixer 볼륨 초기화 (기본값: 100)
        InitializeDefaultVolumes();
    }

    /// <summary>
    /// AudioMixer의 모든 볼륨을 기본값(100)으로 초기화
    /// 이미 설정된 값이 있으면 유지
    /// </summary>
    private void InitializeDefaultVolumes()
    {
        if (mainMixer == null)
        {
            Debug.LogWarning("[SoundManager] MainMixer가 할당되지 않아 초기화를 건너뜁니다.");
            return;
        }

        // 기본 볼륨 값 (0~100)
        float defaultVolume = 100f;

        // 모든 믹서 그룹 초기화
        string[] mixerParams = { "Master", "MusicVolume", "SFXVolume", "UIVolume", "MicVolume" };

        foreach (string param in mixerParams)
        {
            // PlayerPrefs에서 저장된 값 불러오기 (없으면 기본값 100)
            float savedVolume = PlayerPrefs.GetFloat($"Volume_{param}", defaultVolume);

            // 저장된 값으로 설정
            SetVolume(param, savedVolume);
        }

        Debug.Log("[SoundManager] 볼륨 초기화 완료");
    }

    private void OnDestroy()
    {
        EventHub.Instance?.UnregisterEvent<RequestPlaySoundEvent>(OnRequestPlaySound);
        EventHub.Instance?.UnregisterEvent<RequestChangeVolumeEvent>(OnRequestChangeVolume);
    }

    // 외부 공개 진입점 1개만 유지
    public void Play(SoundRequest request)
    {
        var (clip, volume, loop, mixer) = ResolveRequest(request);
        if (clip == null) return;

        // 풀에서 PooledAudio 가져오기
        var go = ObjectPooling.Get("__AudioSourcePool__", CreatePooledAudio, poolParent);
        var pooledAudio = go.GetComponent<PooledAudio>();
        var audioSource = go.GetComponent<AudioSource>();

        // Transform 설정
        if (request.parent != null)
        {
            go.transform.SetParent(request.parent, false);
            go.transform.localPosition = Vector3.zero;
        }
        else
        {
            go.transform.SetParent(poolParent, false);
            go.transform.position = request.position;
        }

        // AudioMixer 설정 (PooledAudio가 관리하지 않는 부분)
        if (audioSource != null)
        {
            audioSource.outputAudioMixerGroup = mixer;
        }

        // PooledAudio에게 재생 및 생명주기 관리 위임
        if (pooledAudio != null)
        {
            pooledAudio.Setup(clip, volume, request.pitch, request.spatialBlend, loop);
        }
        else
        {
            Debug.LogError("[SoundManager] PooledAudio 컴포넌트를 찾을 수 없습니다!");
            ObjectPooling.Return(go);
        }
    }

    private (AudioClip clip, float volume, bool loop, AudioMixerGroup mixer) ResolveRequest(SoundRequest request)
    {
        AudioClip clip = request.clip;
        float volume = request.volume > 0f ? request.volume : 1f;
        bool loop = request.loop;
        AudioMixerGroup mixer = null;

        if (string.IsNullOrEmpty(request.mixerGroupName) == false)
        {
            nameToMixer.TryGetValue(request.mixerGroupName, out mixer);
        }
        if (mixer == null) mixer = defaultMixerGroup;

        if (clip == null)
        {
            if (string.IsNullOrEmpty(request.id) == false)
            {
                if (idToEntry.TryGetValue(request.id, out var entry))
                {
                    if (entry.clip != null) clip = entry.clip;
                    else if (string.IsNullOrEmpty(entry.resourcesPath) == false)
                        clip = Resources.Load<AudioClip>(entry.resourcesPath);

                    if (request.volume <= 0f) volume = entry.defaultVolume;
                    if (request.loop == false) loop = entry.defaultLoop;
                    if (mixer == null && string.IsNullOrEmpty(entry.mixerGroupName) == false)
                        nameToMixer.TryGetValue(entry.mixerGroupName, out mixer);
                    if (mixer == null) mixer = defaultMixerGroup;
                }
                else
                {
                    // id가 등록되어있지 않으면 id를 Resources 경로로 간주
                    clip = Resources.Load<AudioClip>(request.id);
                }
            }
        }

        return (clip, volume, loop, mixer);
    }

    private GameObject CreatePooledAudio()
    {
        var go = new GameObject("AudioSource (Pooled)");
        if (poolParent != null) go.transform.SetParent(poolParent, false);

        // AudioSource 확보 (있으면 재사용, 없으면 추가)
        if (!go.TryGetComponent<AudioSource>(out var src))
            src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialize = false;

        // PooledAudio 확보 (있으면 재사용, 없으면 추가)
        if (!go.TryGetComponent<PooledAudio>(out var _))
            go.AddComponent<PooledAudio>();

        go.SetActive(false);
        return go;
    }

    private void OnRequestPlaySound(RequestPlaySoundEvent e)
    {
        Play(new SoundRequest
        {
            id = e.id,
            clip = e.clip,
            position = e.position,
            parent = e.parent,
            volume = e.volume,
            pitch = e.pitch,
            spatialBlend = e.spatialBlend,
            loop = e.loop,
            mixerGroupName = e.mixerGroupName
        });
    }

    /// <summary>
    /// 오디오 믹서 볼륨 변경 이벤트 핸들러
    /// </summary>
    private void OnRequestChangeVolume(RequestChangeVolumeEvent e)
    {
        SetVolume(e.mixerGroupName, e.volume);
    }

    /// <summary>
    /// 오디오 믹서의 특정 그룹 볼륨을 설정합니다.
    /// </summary>
    /// <param name="mixerGroupName">믹서 그룹 이름 (MusicVolume, SFXVolume, UIVolume, MicVolume)</param>
    /// <param name="volume">볼륨 값 (0~100)</param>
    public void SetVolume(string mixerGroupName, float volume)
    {
        if (mainMixer == null)
        {
            Debug.LogWarning("[SoundManager] MainMixer가 할당되지 않았습니다!");
            return;
        }

        // 볼륨 값을 0~100 범위로 클램프
        volume = Mathf.Clamp(volume, 0f, 100f);

        // 0~100을 0~1로 변환
        float normalizedVolume = volume / 100f;

        // 선형 볼륨을 데시벨로 변환 (-80dB ~ 0dB)
        // 0.0001 이하는 완전히 음소거 (-80dB)
        float dbVolume = normalizedVolume > 0.0001f ? Mathf.Log10(normalizedVolume) * 20f : -80f;

        // AudioMixer 파라미터 설정
        bool success = mainMixer.SetFloat(mixerGroupName, dbVolume);

        if (success)
        {
            // PlayerPrefs에 저장 (설정 유지용)
            PlayerPrefs.SetFloat($"Volume_{mixerGroupName}", volume);
            PlayerPrefs.Save();

            // 볼륨 변경 이벤트 발생 (0~100 값으로)
            EventHub.Instance?.RaiseEvent(new OnVolumeChangedEvent(mixerGroupName, volume));
        }
        else
        {
            Debug.LogWarning($"[SoundManager] AudioMixer 파라미터 '{mixerGroupName}'를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 오디오 믹서의 특정 그룹 볼륨을 가져옵니다.
    /// </summary>
    /// <param name="mixerGroupName">믹서 그룹 이름</param>
    /// <returns>볼륨 값 (0~100), 실패 시 100 반환</returns>
    public float GetVolume(string mixerGroupName)
    {
        if (mainMixer == null)
        {
            Debug.LogWarning("[SoundManager] MainMixer가 할당되지 않았습니다!");
            return 100f;
        }

        if (mainMixer.GetFloat(mixerGroupName, out float dbVolume))
        {
            // 데시벨을 선형 볼륨으로 변환 (0~1)
            float normalizedVolume = dbVolume <= -80f ? 0f : Mathf.Pow(10f, dbVolume / 20f);

            // 0~1을 0~100으로 변환
            return normalizedVolume * 100f;
        }

        Debug.LogWarning($"[SoundManager] AudioMixer 파라미터 '{mixerGroupName}'를 찾을 수 없습니다!");
        return 100f;
    }
}
