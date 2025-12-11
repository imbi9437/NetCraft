using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Project.Script.Manager;
using Photon.Realtime;
using Photon.Pun;
using Cysharp.Threading.Tasks;
using _Project.Script.EventStruct;
using _Project.Script.UI.Main;

namespace _Project.Script.UI
{
    /// <summary>
    /// 방 내부 패널 관리
    /// 협업 시 방 내부 UI 로직을 분리하여 관리
    /// </summary>
    public class RoomPanel : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Text roomNameText;
        [SerializeField] private Text roomInfoText; // 방 정보 추가 (인원수, 방 타입 등)
        [SerializeField] private Transform playerListContent;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button leaveRoomButton;

        [Header("프리팹")]
        [SerializeField] private GameObject playerEntryPrefab;

        [Header("참조")]
        [SerializeField] private MainUIManager uiManager;

        private List<GameObject> playerEntryList = new List<GameObject>();

        private void Start()
        {
            InitializeUI();
        }

        /// <summary>
        /// UI 초기화 및 이벤트 연결
        /// </summary>
        private void InitializeUI()
        {
            // 이벤트 연결
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameButtonClick);

            if (leaveRoomButton != null)
                leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClick);

            // 이벤트 구독
            EventHub.Instance.RegisterEvent<PunEvents.OnJoinedRoomEvent>(OnJoinedRoom);
            EventHub.Instance.RegisterEvent<PunEvents.OnPlayerLeftEvent>(OnPlayerLeft);

            // 플레이어 속성 업데이트 이벤트 구독
            EventHub.Instance.RegisterEvent<PunEvents.OnPlayerPropertiesUpdateEvent>(OnPlayerPropertiesUpdate);
            EventHub.Instance.RegisterEvent<PunEvents.OnPlayerReadyChangedEvent>(OnPlayerReadyChanged);

            StatusManager.ShowWarning("방에 입장 중...");

            // 이미 방에 입장한 상태라면 현재 방 정보 표시 (비동기)
            if (PhotonNetwork.InRoom)
            {
                Debug.Log("[RoomPanel] 이미 방에 입장한 상태 - 현재 방 정보 표시");
                UpdateCurrentRoomInfoAsync().Forget();
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제 (null 체크)
            if (EventHub.Instance != null)
            {
                EventHub.Instance.UnregisterEvent<PunEvents.OnJoinedRoomEvent>(OnJoinedRoom);
                EventHub.Instance.UnregisterEvent<PunEvents.OnPlayerJoinedEvent>(OnPlayerJoined);
                EventHub.Instance.UnregisterEvent<PunEvents.OnPlayerLeftEvent>(OnPlayerLeft);
                EventHub.Instance.UnregisterEvent<PunEvents.OnPlayerPropertiesUpdateEvent>(OnPlayerPropertiesUpdate);
                EventHub.Instance.UnregisterEvent<PunEvents.OnPlayerReadyChangedEvent>(OnPlayerReadyChanged);
            }
        }

        /// <summary>
        /// 게임 시작 버튼 클릭
        /// </summary>
        private void OnStartGameButtonClick()
        {
            Debug.Log("[RoomPanel] 게임 시작 버튼 클릭");

            // 마스터 클라이언트만 게임 시작 가능
            if (!PhotonNetwork.IsMasterClient)
            {
                StatusManager.ShowError("방장만 게임을 시작할 수 있습니다");
                return;
            }

            // 최소 플레이어 수 확인 (1명 이상)
            var players = PhotonNetwork.PlayerList;
            if (players.Length < 1)
            {
                StatusManager.ShowError("최소 1명의 플레이어가 필요합니다");
                return;
            }

            // 모든 플레이어가 준비 상태인지 확인
            bool allReady = true;
            foreach (var player in players)
            {
                if (player.IsMasterClient) continue; // 방장은 제외

                if (!player.CustomProperties.ContainsKey("Ready") ||
                    !(bool)player.CustomProperties["Ready"])
                {
                    allReady = false;
                    break;
                }
            }

            if (!allReady)
            {
                StatusManager.ShowError("모든 플레이어가 준비 상태여야 합니다");
                return;
            }

            StatusManager.ShowSuccess("게임을 시작합니다...");

            // 게임 시작 로직
            StartGame();
        }

        /// <summary>
        /// 게임 시작 (이벤트 기반)
        /// </summary>
        private void StartGame()
        {
            // 선택된 캐릭터를 UserData에 저장 (추후 확장용)
            SaveSelectedCharacterToUserData();

            // RoomManager를 통해 게임 시작 (이벤트 기반)
            if (EventHub.Instance != null)
            {
                EventHub.Instance.RaiseEvent(new PunEvents.OnGameStartRequestEvent
                {
                    sceneName = "JAEO_InGameTest"
                });
            }
        }

        /// <summary>
        /// 선택된 캐릭터를 UserData에 저장 (추후 확장용)
        /// </summary>
        private void SaveSelectedCharacterToUserData()
        {
            // 로컬 플레이어의 캐릭터 선택 정보 가져오기
            var cp = PhotonNetwork.LocalPlayer.CustomProperties;
            if (cp.ContainsKey("CharSel"))
            {
                int selectedCharacter = (int)cp["CharSel"];
                Debug.Log($"[RoomPanel] 선택된 캐릭터: {selectedCharacter}");
                // 여기에 UserData 업데이트 로직 추가 (추후 확장용)
            }
        }

        /// <summary>
        /// 방 나가기 버튼 클릭
        /// </summary>
        private void OnLeaveRoomButtonClick()
        {
            Debug.Log("[RoomPanel] 방 나가기 버튼 클릭");
            EventHub.Instance.RaiseEvent(new PunEvents.LeaveRoomRequestEvent());
            
            // UI 전환은 OnLeftRoom 이벤트에서 처리
        }

        /// <summary>
        /// 방 참가 완료 이벤트 처리
        /// </summary>
        private void OnJoinedRoom(PunEvents.OnJoinedRoomEvent evt)
        {

        }

        /// <summary>
        /// 플레이어 입장 이벤트 처리
        /// </summary>
        private void OnPlayerJoined(PunEvents.OnPlayerJoinedEvent evt)
        {
        }

        /// <summary>
        /// 플레이어 퇴장 이벤트 처리
        /// </summary>
        private void OnPlayerLeft(PunEvents.OnPlayerLeftEvent evt)
        {
        }

        /// <summary>
        /// 플레이어 목록 업데이트
        /// </summary>
        private void UpdatePlayerList()
        {
            // 기존 플레이어 아이템들 제거
            ClearPlayerList();

            // 현재 방의 플레이어들 가져오기
            var players = PhotonNetwork.PlayerList;

            if (players == null || players.Length == 0)
            {
                if (EventHub.Instance != null)
                {
                    StatusManager.ShowInfo("플레이어가 없습니다");
                }
                return;
            }

            Debug.Log($"[RoomPanel] 플레이어 목록 업데이트: {players.Length}명");

            // 플레이어 아이템들 생성
            foreach (var player in players)
            {
                CreatePlayerItem(player);
            }

            // 게임 시작 버튼 상태 업데이트
            UpdateStartButton();
        }

        /// <summary>
        /// 플레이어 아이템 생성
        /// </summary>
        private void CreatePlayerItem(Player player)
        {
            if (playerEntryPrefab == null || playerListContent == null) return;

            GameObject playerEntry = Instantiate(playerEntryPrefab, playerListContent);
            playerEntryList.Add(playerEntry);

            // PlayerEntry 컴포넌트 설정
            PlayerEntry playerEntryScript = playerEntry.GetComponent<PlayerEntry>();
            if (playerEntryScript != null)
            {
                playerEntryScript.SetupPlayer(player);
            }
        }

        /// <summary>
        /// 기존 플레이어 아이템들 제거
        /// </summary>
        private void ClearPlayerList()
        {
            foreach (var item in playerEntryList)
            {
                if (item != null)
                    Destroy(item);
            }
            playerEntryList.Clear();
        }

        /// <summary>
        /// ActorNumber로 PlayerEntry 찾기 (통일된 플레이어 찾기)
        /// </summary>
        private PlayerEntry FindPlayerEntryByActorNumber(int actorNumber)
        {
            foreach (Transform child in playerListContent)
            {
                PlayerEntry playerEntry = child.GetComponent<PlayerEntry>();
                if (playerEntry != null && playerEntry.PlayerInfo != null &&
                    playerEntry.PlayerInfo.ActorNumber == actorNumber)
                {
                    return playerEntry;
                }
            }
            return null;
        }

        /// <summary>
        /// 플레이어 속성 업데이트 이벤트 처리 (Ready 속성 제외)
        /// </summary>
        private void OnPlayerPropertiesUpdate(PunEvents.OnPlayerPropertiesUpdateEvent evt)
        {
            // Ready 속성 변경은 OnPlayerReadyChangedEvent에서 처리하므로 제외
            if (evt.propertyName == "Ready")
            {
                return;
            }


            // 해당 플레이어의 PlayerEntry 업데이트 (ActorNumber로 통일)
            var playerEntry = FindPlayerEntryByActorNumber(evt.actorNumber);
            if (playerEntry != null)
            {
                playerEntry.OnPlayerPropertiesUpdate();
            }
            else
            {
                Debug.LogWarning($"[RoomPanel] 플레이어를 찾을 수 없음: {evt.playerName} (Actor: {evt.actorNumber})");
            }

            // 게임 시작 버튼 상태 업데이트
            UpdateStartButton();
        }

        /// <summary>
        /// 게임 시작 버튼 상태 업데이트
        /// </summary>
        private void UpdateStartButton()
        {
            if (startGameButton == null) return;

            // 방장만 게임 시작 버튼 활성화
            bool isMasterClient = PhotonNetwork.IsMasterClient;
            startGameButton.gameObject.SetActive(isMasterClient);

            if (isMasterClient)
            {
                // 모든 플레이어가 레디 상태인지 확인
                bool allReady = CheckAllPlayersReady();
                startGameButton.interactable = allReady;

                // 버튼 텍스트 업데이트
                Text buttonText = startGameButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = allReady ? "게임 시작" : "모든 플레이어 준비 대기";
                }
            }
        }

        /// <summary>
        /// 모든 플레이어가 레디 상태인지 확인
        /// </summary>
        private bool CheckAllPlayersReady()
        {
            // 최소 1명의 플레이어 필요
            var players = PhotonNetwork.PlayerList;
            if (players.Length < 1) return false;

            // 모든 플레이어가 레디 상태인지 확인 (방장 제외)
            foreach (var player in players)
            {
                if (player.IsMasterClient) continue; // 방장은 제외

                if (!player.CustomProperties.ContainsKey("Ready") ||
                    !(bool)player.CustomProperties["Ready"])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 플레이어 레디 상태 변경 이벤트 처리 (트래픽 최적화)
        /// </summary>
        private void OnPlayerReadyChanged(PunEvents.OnPlayerReadyChangedEvent evt)
        {
            // 패널이 활성화되어 있지 않으면 처리하지 않음 (트래픽 절약)
            if (!gameObject.activeInHierarchy)
            {
                Debug.Log("[RoomPanel] 패널이 비활성화 상태 - 레디 상태 변경 이벤트 스킵");
                return;
            }

            // 해당 플레이어의 UI 업데이트 (중복 코드 통합)
            var playerEntry = FindPlayerEntryByActorNumber(evt.actorNumber);
            if (playerEntry != null)
            {
                playerEntry.SetReadyState(evt.isReady);
            }
            else
            {
                Debug.LogWarning($"[RoomPanel] 레디 상태 변경할 플레이어를 찾을 수 없음: {evt.playerName} (Actor: {evt.actorNumber})");
                // 전체 플레이어 목록 업데이트는 비용이 크므로, 로그만 남기고 스킵
                // UpdatePlayerList(); // 제거하여 트래픽 절약
            }

            // 게임 시작 버튼 상태 업데이트
            UpdateStartButton();
        }

        /// <summary>
        /// 방 정보 표시 업데이트
        /// </summary>
        private void UpdateRoomDisplay(string roomName, int playerCount, int maxPlayers)
        {
            // 방 이름 표시
            if (roomNameText != null)
            {
                roomNameText.text = $"방 이름: {roomName}";
                Debug.Log($"[RoomPanel] 방 이름 표시: {roomName}");
            }

            // 방 정보 표시 (인원수, 방 타입 등)
            if (roomInfoText != null)
            {
                string roomType = GetRoomType();
                roomInfoText.text = $"인원: {playerCount}/{maxPlayers}명 | {roomType}";
            }
        }

        /// <summary>
        /// 현재 방 정보 업데이트 (이미 방에 입장한 상태에서 호출)
        /// </summary>
        public void UpdateCurrentRoomInfo()
        {
            UpdateCurrentRoomInfoAsync().Forget();
        }

        /// <summary>
        /// 현재 방 정보 업데이트 (비동기)
        /// </summary>
        public async UniTask UpdateCurrentRoomInfoAsync()
        {
            if ((PhotonNetwork.IsConnected && PhotonNetwork.InRoom) && PhotonNetwork.CurrentRoom != null)
            {
                var room = PhotonNetwork.CurrentRoom;
                Debug.Log($"[RoomPanel] 방 정보 업데이트: {room.Name} ({room.PlayerCount}/{room.MaxPlayers})");

                // 1프레임 대기 (UI 렌더링 완료 후 처리)
                await UniTask.Yield(PlayerLoopTiming.Update);

                // 방 정보 표시
                UpdateRoomDisplay(room.Name, room.PlayerCount, room.MaxPlayers);

                // 플레이어 목록 업데이트
                UpdatePlayerList();

                // 게임 시작 버튼 상태 업데이트
                UpdateStartButton();

                StatusManager.ShowInfo($"현재 방: {room.Name} ({room.PlayerCount}/{room.MaxPlayers}명)");
            }
        }

        /// <summary>
        /// 방 타입 가져오기 (방장만 비밀번호 정보 표시)
        /// </summary>
        private string GetRoomType()
        {
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("roomType"))
            {
                string roomType = PhotonNetwork.CurrentRoom.CustomProperties["roomType"].ToString();

                if (roomType == "private")
                {
                    // 방장만 비밀번호 정보를 볼 수 있음
                    if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("password"))
                    {
                        string password = PhotonNetwork.CurrentRoom.CustomProperties["password"].ToString();
                        return $"비밀방 (비밀번호: {password})";
                    }
                    else
                    {
                        return "비밀방";
                    }
                }
                else
                {
                    return "공개방";
                }
            }
            return "공개방";
        }
    }
}