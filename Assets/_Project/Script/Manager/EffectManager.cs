using _Project.Script.Generic;
using _Project.Script.Manager;
using _Project.Script.EventStruct;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시각 이펙트(VFX) 재생 관리 시스템
/// 
/// [기능]
/// - 파티클/이펙트 GameObject 재생 및 관리
/// - ObjectPooling을 통한 이펙트 오브젝트 재사용으로 메모리 최적화
/// - duration 기반 자동 회수 시스템
/// 
/// [사용법 1: EventHub (권장)]
/// EventHub.Instance.RaiseEvent(new RequestPlayEffectEvent
/// {
///     id = "VFX_Hit",                 // 등록된 ID 또는 Resources 경로
///     position = hitPosition,
///     rotation = Quaternion.identity,
///     duration = 1.5f,                // 자동 회수까지 대기 시간
///     worldSpace = true,              // 월드/로컬 좌표
///     scale = Vector3.one
/// });
/// 
/// [사용법 2: 직접 호출]
/// EffectManager.Instance.Play(new EffectManager.EffectRequest { ... });
/// 
/// [에디터 설정]
/// - Effect Entries: 자주 쓰는 이펙트 등록 (id, prefab, resourcesPath)
/// - Warmup Count: 미리 생성할 이펙트 컨테이너 개수 (기본 16)
/// 
/// [특징]
/// - 프리팹 스왑: 하나의 풀에서 여러 이펙트 재생 가능
/// - Transform 설정: position, rotation, scale, parent 지원
/// </summary>
public class EffectManager : MonoSingleton<EffectManager>
{
    [System.Serializable]
    private class EffectEntry
    {
        public string id;                 // 이펙트 식별자
        public GameObject prefab;         // 직접 할당 프리팹
        public string resourcesPath;      // Resources 경로 (선택)
        public float defaultDuration = 1f;// 기본 재생 시간
    }

    public struct EffectRequest
    {
        public string id;                 // 등록된 id 또는 Resources 경로 키
        public GameObject prefab;         // 직접 재생용 프리팹
        public Vector3 position;          // 생성 위치
        public Quaternion rotation;       // 회전
        public Transform parent;          // 붙일 부모(선택)
        public float duration;            // 자동 회수 대기 시간(<=0이면 entry 기본값)
        public bool worldSpace;           // true면 월드 좌표 고정
        public Vector3 scale;             // 스케일 지정(선택)
    }

    [Header("Registry")]
    [SerializeField] private List<EffectEntry> effectEntries = new List<EffectEntry>();

    [Header("Pool Parent & Warmup")]
    [SerializeField] private Transform poolParent;
    [SerializeField] private int warmupCount = 16;

    private readonly Dictionary<string, EffectEntry> idToEntry = new Dictionary<string, EffectEntry>();

    protected override void Awake()
    {
        base.Awake();

        idToEntry.Clear();
        foreach (var e in effectEntries)
        {
            if (string.IsNullOrEmpty(e.id)) continue;
            if (idToEntry.ContainsKey(e.id) == false)
                idToEntry.Add(e.id, e);
            else
                idToEntry[e.id] = e;
        }

        // 워밍업: 대표 키 1개 사용, 최초 팩토리에서 dummy cube로 만들어졌다가 첫 요청 시 실제 프리팹으로 교체됨
        ObjectPooling.Warmup("__EffectPool__", CreatePooledEffect, warmupCount, poolParent);

        EventHub.Instance?.RegisterEvent<RequestPlayEffectEvent>(OnRequestPlayEffect);
    }

    private void OnDestroy()
    {
        EventHub.Instance?.UnregisterEvent<RequestPlayEffectEvent>(OnRequestPlayEffect);
    }

    // 외부 공개 진입점 1개만 유지
    public void Play(EffectRequest request)
    {
        var (prefab, duration) = ResolveRequest(request);
        if (prefab == null) return;

        // 풀에서 PooledEffect 가져오기
        var go = ObjectPooling.Get("__EffectPool__", CreatePooledEffect, poolParent);
        var pooledEffect = go.GetComponent<PooledEffect>();

        if (pooledEffect != null)
        {
            // PooledEffect에게 설정 및 생명주기 관리 위임
            pooledEffect.Setup(
                prefab,
                request.position,
                request.rotation,
                request.parent,
                duration > 0f ? duration : 1f,
                request.worldSpace,
                request.scale
            );
        }
        else
        {
            Debug.LogError("[EffectManager] PooledEffect 컴포넌트를 찾을 수 없습니다!");
            ObjectPooling.Return(go);
        }
    }

    private (GameObject prefab, float duration) ResolveRequest(EffectRequest request)
    {
        GameObject prefab = request.prefab;
        float duration = request.duration;

        if (prefab == null)
        {
            if (string.IsNullOrEmpty(request.id) == false)
            {
                if (idToEntry.TryGetValue(request.id, out var entry))
                {
                    if (entry.prefab != null) prefab = entry.prefab;
                    else if (string.IsNullOrEmpty(entry.resourcesPath) == false)
                        prefab = Resources.Load<GameObject>(entry.resourcesPath);
                    if (duration <= 0f) duration = entry.defaultDuration;
                }
                else
                {
                    // id를 Resources 경로로 간주
                    prefab = Resources.Load<GameObject>(request.id);
                }
            }
        }

        if (duration <= 0f) duration = 1f;
        return (prefab, duration);
    }

    private GameObject CreatePooledEffect()
    {
        var go = new GameObject("Effect (Pooled)");
        if (poolParent != null) go.transform.SetParent(poolParent, false);

        // PooledEffect 확보 (있으면 재사용, 없으면 추가)
        if (!go.TryGetComponent<PooledEffect>(out var _))
            go.AddComponent<PooledEffect>();

        go.SetActive(false);
        return go;
    }

    private void OnRequestPlayEffect(RequestPlayEffectEvent e)
    {
        Play(new EffectRequest
        {
            id = e.id,
            prefab = e.prefab,
            position = e.position,
            rotation = e.rotation,
            parent = e.parent,
            duration = e.duration,
            worldSpace = e.worldSpace,
            scale = e.scale
        });
    }
}
