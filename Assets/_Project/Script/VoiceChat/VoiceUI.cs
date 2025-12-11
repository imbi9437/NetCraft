using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Photon.Voice.Unity;

namespace _Project.Script.VoiceChat
{
    public class VoiceUI : MonoBehaviourPunCallbacks
    {
        [Header("UI References")]
        public Button micToggleButton;
        public List<GameObject> micOnGameObjects;
        public List<GameObject> micOffGameObjects;

        [Header("Individual Volume UI")]
        public GameObject playerVolumeSliderPrefab;
        public Transform sliderContainer;

        private Recorder localRecorder;
        private bool isMicOn = true;

        private readonly Dictionary<int, float> playerVolumes = new Dictionary<int, float>();
        private readonly Dictionary<int, GameObject> playerVolumeUIs = new Dictionary<int, GameObject>();

        public static VoiceUI Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {

            if (micToggleButton != null)
            {
                micToggleButton.onClick.AddListener(ToggleMicrophone);
            }
            // 초기 UI 상태 설정
            UpdateUI();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            // UI가 활성화될 때 플레이어 목록을 다시 채웁니다.
            PopulateInitialPlayers();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (newPlayer.IsLocal) return;

            Debug.Log($"[VoiceUI] 새 플레이어 입장: {newPlayer.NickName}. 플레이어 볼륨 UI를 생성합니다.");
            CreateSliderForPlayer(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (playerVolumeUIs.ContainsKey(otherPlayer.ActorNumber))
            {
                Destroy(playerVolumeUIs[otherPlayer.ActorNumber]);
                playerVolumeUIs.Remove(otherPlayer.ActorNumber);
                playerVolumes.Remove(otherPlayer.ActorNumber);
                Debug.Log($"[VoiceUI] {otherPlayer.NickName}님이 퇴장하여 볼륨 UI를 제거했습니다.");
            }
        }

        void PopulateInitialPlayers()
        {
            if (PhotonNetwork.CurrentRoom == null) return;

            // 기존 UI 클리어
            foreach (var ui in playerVolumeUIs.Values)
            {
                Destroy(ui);
            }
            playerVolumeUIs.Clear();

            // 현재 방의 모든 원격 플레이어에 대해 UI 생성
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (player.IsLocal) continue;
                CreateSliderForPlayer(player);
            }
        }

        void CreateSliderForPlayer(Player player)
        {
            if (player.IsLocal || playerVolumeUIs.ContainsKey(player.ActorNumber)) return;
            if (playerVolumeSliderPrefab == null || sliderContainer == null) return;

            GameObject sliderInstance = Instantiate(playerVolumeSliderPrefab, sliderContainer);
            sliderInstance.SetActive(true);
            playerVolumeUIs[player.ActorNumber] = sliderInstance;

            float initialVolume = PlayerPrefs.GetFloat($"PlayerVolume_{player.ActorNumber}", 0.7f);
            if (!playerVolumes.ContainsKey(player.ActorNumber))
            {
                playerVolumes.Add(player.ActorNumber, initialVolume);
            }

            var sliderScript = sliderInstance.GetComponent<PlayerVolumeSlider>();
            if (sliderScript != null)
            {
                sliderScript.Setup(player, this, initialVolume);
            }

            ApplyVolumeToPlayer(player.ActorNumber, initialVolume);
        }

        public void SetPlayerVolume(int actorNumber, float volume)
        {
            playerVolumes[actorNumber] = volume;
            PlayerPrefs.SetFloat($"PlayerVolume_{actorNumber}", volume);
            ApplyVolumeToPlayer(actorNumber, volume);
        }

        void ApplyVolumeToPlayer(int actorNumber, float volume)
        {
            foreach (var playerVoice in NetworkManagerVoice.AllPlayerVoices)
            {
                if (playerVoice.photonView.Owner.ActorNumber == actorNumber)
                {
                    playerVoice.SetSpeakerVolume(volume);
                    Debug.Log($"[VoiceUI] {actorNumber}번 플레이어({playerVoice.photonView.Owner.NickName})의 볼륨을 {volume}(으)로 설정했습니다.");
                    return;
                }
            }
        }

        void Update()
        {

        }

        void FindLocalRecorder()
        {
            // FindObjectsOfType은 비효율적이므로, Recorder가 준비되면 직접 참조를 설정하는 것이 더 좋습니다.
            // 예를 들어, PlayerVoice 스크립트에서 VoiceUI.Instance.SetLocalRecorder(recorder)를 호출할 수 있습니다.
            foreach (var rec in FindObjectsOfType<Recorder>())
            {
                PhotonView pv = rec.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    localRecorder = rec;
                    Debug.Log("[VoiceUI] 로컬 플레이어의 Recorder를 찾았습니다.");
                    break;
                }
            }
        }

        void ToggleMicrophone()
        {
            if (localRecorder == null)
            {
                FindLocalRecorder();
                if (localRecorder == null)
                {
                    Debug.LogError("[VoiceUI] Local Recorder를 찾을 수 없어 마이크를 토글할 수 없습니다. 플레이어 프리팹에 Recorder와 PhotonView가 올바르게 설정되었는지 확인하세요.");
                    return;
                }
            }

            isMicOn = !isMicOn;
            localRecorder.TransmitEnabled = isMicOn;
            Debug.Log($"[VoiceUI] 마이크 상태 토글: {(isMicOn ? "ON" : "OFF")}");
            UpdateUI();
        }

        /// <summary>
        /// 마이크 상태를 설정 (외부에서 호출 가능)
        /// </summary>
        public void SetMicrophoneState(bool enable)
        {
            if (localRecorder == null)
            {
                FindLocalRecorder();
                if (localRecorder == null)
                {
                    Debug.LogError("[VoiceUI] Local Recorder를 찾을 수 없어 마이크를 제어할 수 없습니다.");
                    return;
                }
            }

            isMicOn = enable;
            localRecorder.TransmitEnabled = isMicOn;
            Debug.Log($"[VoiceUI] 마이크 상태 변경: {(isMicOn ? "Mic ON" : "Mic OFF")}");
            UpdateUI();
        }

        /// <summary>
        /// 현재 마이크 상태 반환
        /// </summary>
        public bool IsMicrophoneEnabled() => isMicOn;

        void UpdateUI()
        {
            Debug.Log($"[VoiceUI] UI 업데이트 시작. isMicOn = {isMicOn}");

            foreach (var go in micOnGameObjects)
            {
                if (go != null) go.SetActive(isMicOn);
            }

            foreach (var go in micOffGameObjects)
            {
                if (go != null) go.SetActive(!isMicOn);
            }

            Debug.Log("[VoiceUI] UI 업데이트 완료.");
        }
    }
}
