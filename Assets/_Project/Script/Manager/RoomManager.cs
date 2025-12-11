using _Project.Script.Generic;
using _Project.Script.Manager;
using _Project.Script.Character.Player;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using _Project.Script.EventStruct;
using _Project.Script.UI;

namespace _Project.Script.Manager
{
    /// <summary>
    /// 리팩토링된 RoomManager - 단일 책임 원칙 적용
    /// 순수 방 생성/참가/나가기/정보 제공만 담당
    /// </summary>
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        [Header("방 설정")]
        [SerializeField] private int maxPlayersPerRoom = 4;
        [SerializeField] private bool isVisible = true;
        [SerializeField] private bool isOpen = true;

        // 헬퍼 클래스들
        private RoomOperationHandler _operationHandler;
        private RoomValidator _validator;
        private RoomStateManager _stateManager;
        private SceneTransitionHandler _sceneHandler;

        // 방 목록 캐싱 (PUN2 방식)
        private Dictionary<string, RoomInfo> _cachedRoomList = new Dictionary<string, RoomInfo>();

        public static RoomManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeHelpers();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 이벤트 구독
            EventHub.Instance.RegisterEvent<PunEvents.OnJoinRoomRequestEvent>(OnJoinRoomRequest);
            EventHub.Instance.RegisterEvent<PunEvents.OnGameStartRequestEvent>(OnGameStartRequest);
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (EventHub.Instance != null)
            {
                EventHub.Instance.UnregisterEvent<PunEvents.OnJoinRoomRequestEvent>(OnJoinRoomRequest);
                EventHub.Instance.UnregisterEvent<PunEvents.OnGameStartRequestEvent>(OnGameStartRequest);
            }

            // 헬퍼 클래스 정리
            _operationHandler?.Dispose();
        }

        /// <summary>
        /// 헬퍼 클래스들 초기화
        /// </summary>
        private void InitializeHelpers()
        {
            _operationHandler = new RoomOperationHandler();
            _validator = new RoomValidator(_cachedRoomList);
            _stateManager = new RoomStateManager();
            _sceneHandler = new SceneTransitionHandler();

            // 상태 변경 이벤트 구독
            _stateManager.OnStateChanged += OnRoomStateChanged;
            _stateManager.OnNetworkStateChanged += OnNetworkStateChanged;
        }
        

        #region 방 참가

        /// <summary>
        /// 공개방 참가
        /// </summary>
        public async UniTask<bool> JoinRoomAsync(string roomName)
        {
            Debug.Log($"[RoomManager] 공개방 참가 시도: {roomName}");

            try
            {
                _operationHandler.StartRoomJoining();
                _stateManager.StartRoomJoining();

                // 마스터 서버 연결 확인
                if (!await EnsureConnectedToMasterServerAsync())
                {
                    _operationHandler.CompleteRoomJoining(false);
                    _stateManager.CompleteRoomJoining(false);
                    return false;
                }

                // 방 참가
                PhotonNetwork.JoinRoom(roomName);

                // 결과 대기
                bool result = await _operationHandler.WaitForRoomJoiningAsync();
                _stateManager.CompleteRoomJoining(result);
                _operationHandler.FinishRoomJoining();

                return result;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RoomManager] 방 참가 중 오류: {ex.Message}");
                _operationHandler.CompleteRoomJoining(false);
                _stateManager.CompleteRoomJoining(false);
                _operationHandler.FinishRoomJoining();
                RaiseRoomJoinFailedEvent($"방 참가 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 비밀방 참가
        /// </summary>
        public async UniTask<bool> JoinRoomAsync(string roomName, string password)
        {
            Debug.Log($"[RoomManager] 비밀방 참가 시도: {roomName}");

            // 일반 방 참가 로직 실행
            return await JoinRoomAsync(roomName);
        }

        #endregion

        #region 게임 시작

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame(string sceneName)
        {
            // 방장만 게임을 시작할 수 있음
            if (!PhotonNetwork.IsMasterClient)
            {
                RaiseGameStartFailedEvent("방장만 게임을 시작할 수 있습니다.");
                return;
            }

            // 방에 있는지 확인
            if (!PhotonNetwork.InRoom)
            {
                RaiseGameStartFailedEvent("방에 입장하지 않은 상태입니다.");
                return;
            }

            Debug.Log($"[RoomManager] 게임 시작: {sceneName}");

            // 씬 전환 처리
            bool success = _sceneHandler.StartGameSceneTransition(sceneName);
            if (!success)
            {
                RaiseGameStartFailedEvent("게임 시작에 실패했습니다.");
            }
        }

        #endregion

        #region 헬퍼 메서드

        /// <summary>
        /// 마스터 서버 연결 확인
        /// </summary>
        private async UniTask<bool> EnsureConnectedToMasterServerAsync()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.NetworkClientState is ClientState.ConnectedToMasterServer or ClientState.JoinedLobby)
            {
                return true;
            }

            Debug.LogWarning("[RoomManager] 마스터 서버 연결 대기");

            try
            {
                _operationHandler.StartConnection();

                if (PhotonNetwork.NetworkClientState != ClientState.ConnectingToMasterServer)
                {
                    PhotonNetwork.ConnectUsingSettings();
                }

                bool result = await _operationHandler.WaitForConnectionAsync();
                return result;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RoomManager] 마스터 서버 연결 실패: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 이벤트 발송

        private void RaiseRoomJoinFailedEvent(string message)
        {
        }

        private void RaiseGameStartFailedEvent(string message)
        {
            // 게임 시작 실패 이벤트 (추후 NetworkEvents에 추가 필요)
            Debug.LogError($"[RoomManager] {message}");
        }

        #endregion

        #region 이벤트 핸들러

        private void OnJoinRoomRequest(PunEvents.OnJoinRoomRequestEvent evt)
        {
            
        }

        private void OnGameStartRequest(PunEvents.OnGameStartRequestEvent evt)
        {
            StartGame(evt.sceneName);
        }

        private void OnRoomStateChanged(RoomStateManager.RoomOperationState oldState, RoomStateManager.RoomOperationState newState, string reason)
        {
            Debug.Log($"[RoomManager] 방 상태 변경: {oldState} → {newState} ({reason})");
        }

        private void OnNetworkStateChanged(ClientState oldState, ClientState newState)
        {
            Debug.Log($"[RoomManager] 네트워크 상태 변경: {oldState} → {newState}");
        }

        #endregion

        #region PUN2 콜백

        public override void OnJoinedRoom()
        {
            Debug.Log($"[RoomManager] 방 참가 성공: {PhotonNetwork.CurrentRoom.Name}");

            // 씬 동기화 활성화
            PhotonNetwork.AutomaticallySyncScene = true;

            _operationHandler.CompleteRoomJoining(true);
            _stateManager.UpdateNetworkState(PhotonNetwork.NetworkClientState);
        }

        public override void OnCreatedRoom()
        {
            Debug.Log($"[RoomManager] 방 생성 성공: {PhotonNetwork.CurrentRoom.Name}");
            _operationHandler.CompleteRoomCreation(true);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[RoomManager] 방 생성 실패: {message}");
            _operationHandler.CompleteRoomCreation(false);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[RoomManager] 방 참가 실패: {message}");
            _operationHandler.CompleteRoomJoining(false);
        }

        public override void OnLeftRoom()
        {
            Debug.Log("[RoomManager] 방 나가기 성공");

            // Ready 상태 초기화
            ResetLocalPlayerReadyState();

            _stateManager.CompleteRoomLeaving();
            _stateManager.UpdateNetworkState(PhotonNetwork.NetworkClientState);

            // 로비 재접속
            SafeReconnectToLobbyAsync().Forget();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"[RoomManager] 플레이어 입장: {newPlayer.NickName}");

        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"[RoomManager] 플레이어 퇴장: {otherPlayer.NickName}");
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps == null || changedProps.Count == 0)
            {
                return;
            }

            Debug.Log($"[RoomManager] 플레이어 {targetPlayer.NickName} 속성 업데이트: {string.Join(", ", changedProps.Keys)}");

            // Ready 상태 변경 처리
            if (changedProps.ContainsKey("Ready"))
            {
                bool isReady = (bool)changedProps["Ready"];

                EventHub.Instance?.RaiseEvent(new PunEvents.OnPlayerReadyChangedEvent
                {
                    playerName = targetPlayer.NickName,
                    actorNumber = targetPlayer.ActorNumber,
                    isReady = isReady
                });
            }

            // 다른 네트워크 매니저들에게 속성 변경 알림 (이벤트 기반)
            NotifyNetworkManagers(targetPlayer, changedProps);

            // 다른 속성들 처리
            var otherProps = new Hashtable();
            foreach (var kvp in changedProps)
            {
                if (kvp.Key.ToString() != "Ready")
                {
                    otherProps[kvp.Key] = kvp.Value;
                }
            }

            if (otherProps.Count > 0)
            {
                EventHub.Instance?.RaiseEvent(new PunEvents.OnPlayerPropertiesUpdateEvent
                {
                    playerName = targetPlayer.NickName,
                    actorNumber = targetPlayer.ActorNumber,
                    propertyName = "Properties",
                    propertyValue = otherProps
                });
            }
        }

        /// <summary>
        /// 다른 네트워크 매니저들에게 속성 변경 알림 (이벤트 기반)
        /// </summary>
        private void NotifyNetworkManagers(Player targetPlayer, Hashtable changedProps)
        {
            // PlayerNetworkHandler는 각 플레이어마다 개별 인스턴스이므로
            // 직접 호출하는 대신 이벤트를 통해 알림
            EventHub.Instance?.RaiseEvent(new PunEvents.OnPlayerPropertiesUpdateEvent
            {
                playerName = targetPlayer.NickName,
                actorNumber = targetPlayer.ActorNumber,
                propertyName = "PlayerProperties",
                propertyValue = changedProps
            });

            // NetworkGameEventManager와 NetworkWorldManager는 Room CustomProperties를 사용하므로
            // Player Properties 변경과는 관련이 없음
        }

        #endregion

        #region 유틸리티

        private void ResetLocalPlayerReadyState()
        {
            try
            {
                if (PhotonNetwork.LocalPlayer != null)
                {
                    var props = new Hashtable();
                    props["Ready"] = false;
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                    Debug.Log("[RoomManager] 로컬 플레이어 Ready 상태 초기화 완료");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RoomManager] Ready 상태 초기화 중 오류: {ex.Message}");
            }
        }

        private async UniTask SafeReconnectToLobbyAsync()
        {
            try
            {
                if (PhotonNetwork.NetworkClientState == ClientState.DisconnectingFromGameServer)
                {
                    await UniTask.WaitUntil(() =>
                        PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer ||
                        PhotonNetwork.NetworkClientState == ClientState.JoinedLobby ||
                        PhotonNetwork.NetworkClientState == ClientState.Disconnected);
                }

                if (PhotonNetwork.IsConnected && PhotonNetwork.InLobby)
                {
                    Debug.Log("[RoomManager] 이미 로비에 접속되어 있음");
                    return;
                }

                if (PhotonNetwork.IsConnected && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                {
                    await UniTask.Delay(500);
                    PhotonNetwork.JoinLobby();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RoomManager] 로비 재접속 중 오류: {ex.Message}");
            }
        }

        #endregion

        #region 공개 API

        public List<RoomInfo> GetCachedRoomList() => new List<RoomInfo>(_cachedRoomList.Values);
        public RoomInfo GetCachedRoomInfo(string roomName) => _cachedRoomList.ContainsKey(roomName) ? _cachedRoomList[roomName] : null;
        public static void LoadGameScene() => SceneTransitionHandler.LoadGameScene();

        #endregion
    }
}