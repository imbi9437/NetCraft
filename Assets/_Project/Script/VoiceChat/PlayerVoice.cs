using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;

namespace _Project.Script.VoiceChat
{
    public class PlayerVoice : MonoBehaviourPun
    {
        [Header("Voice Components")]
        public Recorder voiceRecorder;
        public Speaker voiceSpeaker;

        private bool wasSpeaking;
        private bool wasReceiving; // 다른 사람 목소리 수신 상태 추적용
        private bool wasAudioSourcePlaying; // 로컬 오디오 재생 상태 추적용

        // Awake에서 중앙 목록에 자기 자신을 등록
        void Awake()
        {
            // 컴포넌트 자동 추가
            voiceRecorder = gameObject.GetComponent<Recorder>() ?? gameObject.AddComponent<Recorder>();
            voiceSpeaker = gameObject.GetComponent<Speaker>() ?? gameObject.AddComponent<Speaker>();

            // NetworkManager의 중앙 목록에 자기 자신을 추가
            if (!NetworkManagerVoice.AllPlayerVoices.Contains(this))
            {
                NetworkManagerVoice.AllPlayerVoices.Add(this);
            }
            Debug.Log($"PlayerVoice Awake: 현재 목록에 {NetworkManagerVoice.AllPlayerVoices.Count}명 등록됨");
        }

        void Start()
        {
            // 로컬 플레이어와 원격 플레이어 설정
            if (photonView.IsMine)
            {
                SetupLocalPlayer();
            }
            else
            {
                SetupRemotePlayer();
            }
        }

        // OnDestroy에서 중앙 목록에 자기 자신을 제거
        void OnDestroy()
        {
            if (NetworkManagerVoice.AllPlayerVoices.Contains(this))
            {
                NetworkManagerVoice.AllPlayerVoices.Remove(this);
            }
            Debug.Log($"PlayerVoice OnDestroy: 현재 목록에 {NetworkManagerVoice.AllPlayerVoices.Count}명 남음");
        }

        void Update()
        {
            if (photonView.IsMine)
            {
                // 로컬 플레이어 (나) - 목소리 송신 확인
                bool isCurrentlySpeaking = voiceRecorder.IsCurrentlyTransmitting;
                if (isCurrentlySpeaking != wasSpeaking)
                {
                    wasSpeaking = isCurrentlySpeaking;
                    if (wasSpeaking)
                    {
                        Debug.Log("<color=green>[송신] 🎤 마이크 ON. 음성 전송을 시작합니다.</color>");
                    }
                    else
                    {
                        Debug.Log("<color=yellow>[송신] 🤫 마이크 OFF. 음성 전송을 중단합니다.</color>");
                    }
                }

                // 로컬 플레이어의 AudioSource 출력 상태가 '변경'될 때만 로그를 출력
                if (voiceSpeaker != null && voiceSpeaker.GetComponent<AudioSource>() != null)
                {
                    AudioSource audioSource = voiceSpeaker.GetComponent<AudioSource>();
                    bool isPlaying = audioSource.isPlaying;
                    if (isPlaying != wasAudioSourcePlaying)
                    {
                        if (isPlaying)
                        {
                            Debug.Log($"<color=lime>[출력 확인] ✅ AudioSource 재생 시작. (볼륨: {audioSource.volume}, 음소거: {audioSource.mute})</color>");
                        }
                        else
                        {
                            Debug.Log($"<color=red>[출력 확인] ❌ AudioSource 재생 중단. (볼륨: {audioSource.volume}, 음소거: {audioSource.mute})</color>");
                        }
                        wasAudioSourcePlaying = isPlaying; // 상태 업데이트
                    }
                }
            }
            else
            {
                // 원격 플레이어 (다른 사람) - 목소리 수신 확인
                bool isCurrentlyReceiving = voiceSpeaker.IsPlaying;
                if (isCurrentlyReceiving != wasReceiving)
                {
                    wasReceiving = isCurrentlyReceiving;
                    if (isCurrentlyReceiving)
                    {
                        Debug.Log($"<color=cyan>[수신] 🔊 {photonView.Owner.NickName}님의 음성을 수신하여 재생합니다.</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=orange>[수신] 🎧 {photonView.Owner.NickName}님의 음성 재생이 끝났습니다.</color>");
                    }
                }
            }
        }

        void SetupLocalPlayer()
        {
            // Recorder 설정 (목소리 보내기)
            voiceRecorder.TransmitEnabled = true;
            voiceRecorder.VoiceDetection = false; // 음성 감지 비활성화 (상시 전송)

            // Speaker 활성화 (자기 목소리 듣기) - 임시 테스트용
            voiceSpeaker.enabled = true;

            Debug.Log("=== 로컬 플레이어 음성 설정 완료 (상시 전송 모드, 자기 목소리 듣기 활성화) ===");
        }

        void SetupRemotePlayer()
        {
            // Recorder 비활성화 (다른 사람은 보내기만 함)
            voiceRecorder.TransmitEnabled = false;
            voiceRecorder.enabled = false;

            // Speaker 활성화 (다른 사람 목소리 듣기)
            voiceSpeaker.enabled = true;

            var audioSource = voiceSpeaker.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.spatialBlend = 1.0f; // 3D 사운드
            }

            Debug.Log("=== 원격 플레이어 음성 설정 완료 ===");
        }

        public bool IsSpeaking()
        {
            // IsCurrentlyTransmitting이 더 정확한 현재 상태를 알려줌
            if (photonView.IsMine && voiceRecorder != null)
            {
                return voiceRecorder.IsCurrentlyTransmitting;
            }
            return false;
        }

        public void ToggleMicrophone(bool isOn)
        {
            if (photonView.IsMine && voiceRecorder != null)
            {
                voiceRecorder.TransmitEnabled = isOn;
                Debug.Log($"마이크: {(isOn ? "ON 🎤" : "OFF 🔇")}");
            }
        }

        public void SetSpeakerVolume(float volume)
        {
            // 이 컴포넌트가 원격 플레이어의 것이고, Speaker가 활성화 되어 있을 때만 볼륨 조절
            if (!photonView.IsMine && voiceSpeaker != null && voiceSpeaker.enabled)
            {
                var audioSource = voiceSpeaker.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.volume = Mathf.Clamp01(volume);
                    Debug.Log($"[PlayerVoice] 성공: {gameObject.name}의 볼륨을 {audioSource.volume}으로 설정했습니다.");
                }
                else
                {
                    Debug.LogError($"[PlayerVoice] 실패: {gameObject.name}에서 AudioSource를 찾을 수 없습니다!");
                }
            }
            else
            {
                // 이 로그는 SetSpeakerVolume이 호출되었지만, 조건 때문에 스킵되었을 때 표시됩니다.
                Debug.LogWarning($"[PlayerVoice] 스킵: {gameObject.name}의 볼륨 조절을 건너뜁니다. " +
                                 $"(IsMine: {photonView.IsMine}, Speaker Not Null: {voiceSpeaker != null}, Speaker Enabled: {voiceSpeaker?.enabled})");
            }
        }
    }
}