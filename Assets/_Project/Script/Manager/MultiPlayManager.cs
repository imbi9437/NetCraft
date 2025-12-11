using System;
using _Project.Script.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using _Project.Script.EventStruct;
using ExitGames.Client.Photon;

namespace _Project.Script.Manager
{
    [DefaultExecutionOrder(-50)]
    public class MultiPlayManager : MonoSingletonPunCallbacks<MultiPlayManager>
    {
        public const int MaxPlayersLimit = 8;
        public const int DefaultMaxPlayers = 4;
        
        private const double PhotonMaxTime = 4294967.295d;        
        
        [SerializeField] private double qualityCheckInterval = 60d;
        private double _lastQualityCheckTime;
        
        private Dictionary<string, RoomInfo> cachedRoom;
        
        private RoomValidator roomValidator;

        #region Unity Message Functions
        
        protected override void Awake()
        {
            base.Awake();
            
            cachedRoom = new Dictionary<string, RoomInfo>();
            roomValidator = new RoomValidator(cachedRoom);

            PhotonNetwork.SendRate = 60;
            
            RegisterEvents();
        }

        private void Update()
        {
            double elapsed = (PhotonNetwork.Time - _lastQualityCheckTime + PhotonMaxTime) % PhotonMaxTime;
            if (elapsed >= qualityCheckInterval) QualityCheck();
        }

        private void OnDestroy()
        {
            UnRegisterEvents();
        }
        
        #endregion

        private void RegisterEvents()
        {
            if (EventHub.Instance == null)
            {
                Debug.Log($"<color=red>[{typeof(MultiPlayManager)}] EventHub가 존재하지 않습니다.</color>");
                return;
            }
            
            EventHub.Instance.RegisterEvent<DataEvents.CompleteInitUserDataEvent>(ConnectToMaster);
            
            
            EventHub.Instance.RegisterEvent<PunEvents.CreateRoomRequestEvent>(RequestCreatRoomEvent);
            EventHub.Instance.RegisterEvent<PunEvents.JoinRoomRequestEvent>(RequestJoinRoomEvent);
            EventHub.Instance.RegisterEvent<PunEvents.LeaveRoomRequestEvent>(RequestLeaveRoomEvent);
            
            EventHub.Instance.RegisterEvent<PunEvents.ChangeRoomOptionsRequestEvent>(RequestChangeRoomOptions);
            
            
            //Data Request
            EventHub.Instance?.RegisterEvent<PunEvents.RequestServerConnectionEvent>(RequestConnectionEvent);
        }
        private void UnRegisterEvents()
        {
            if (EventHub.Instance == null)
            {
                Debug.Log($"<color=red>[{typeof(MultiPlayManager)}] EventHub가 존재하지 않습니다.</color>");
                return;
            }
            
            EventHub.Instance.UnregisterEvent<DataEvents.CompleteInitUserDataEvent>(ConnectToMaster);
            
            EventHub.Instance.UnregisterEvent<PunEvents.CreateRoomRequestEvent>(RequestCreatRoomEvent);
            EventHub.Instance.UnregisterEvent<PunEvents.JoinRoomRequestEvent>(RequestJoinRoomEvent);
            EventHub.Instance.UnregisterEvent<PunEvents.LeaveRoomRequestEvent>(RequestLeaveRoomEvent);
            
            EventHub.Instance.UnregisterEvent<PunEvents.ChangeRoomOptionsRequestEvent>(RequestChangeRoomOptions);
            
            //Data Request
            EventHub.Instance?.RegisterEvent<PunEvents.RequestServerConnectionEvent>(RequestConnectionEvent);
        }

        #region Event Rapper Functions

        private void ConnectToMaster(DataEvents.CompleteInitUserDataEvent evt) => ConnectToMaster();


        private void RequestCreatRoomEvent(PunEvents.CreateRoomRequestEvent evt) => CreateRoom(evt.roomName, evt.password, evt.isPublic, evt.maxPlayers);
        private void RequestJoinRoomEvent(PunEvents.JoinRoomRequestEvent evt) => JoinRoom(evt.roomName, evt.password);
        private void RequestLeaveRoomEvent(PunEvents.LeaveRoomRequestEvent evt) => LeaveRoom();


        private void RequestChangeRoomOptions(PunEvents.ChangeRoomOptionsRequestEvent evt) => ChangeRoomOptions(evt.isOpen, evt.isVisible, evt.maxPlayers);
        
        
        
        //Data Request
        private void RequestConnectionEvent(PunEvents.RequestServerConnectionEvent evt) => SendServerConnection();
        

        #endregion

        #region Connection Functions
        
        private void ConnectToMaster()
        {
            if (PhotonNetwork.IsConnected) return;
            if (PhotonNetwork.NetworkClientState == ClientState.ConnectingToMasterServer) return;

            PhotonNetwork.NickName = DataManager.Instance.NickName;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() {{"uid", DataManager.Instance.UserId}});
            
            PhotonNetwork.ConnectUsingSettings();
        }
        private void CreateRoom(string roomName, string password, bool isPublic, int maxPlayers)
        {
            var validateResult = roomValidator.ValidateRoomCreation(roomName, password, isPublic, maxPlayers);

            if (validateResult.IsValid == false)
            {
                Debug.Log(validateResult.Message);
                return;
            }

            RoomOptions options = new RoomOptions()
            {
                MaxPlayers = maxPlayers,
                IsVisible = false,

                CustomRoomProperties = new()
                {
                    { "roomType", isPublic },
                    { "password", password }
                },
                
                CustomRoomPropertiesForLobby = new [] {"roomType", "password"}, // 원래는 하면 안되는데 시간이 부족함
            };
            
            PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
        }
        private void JoinRoom(string roomName, string password)
        {
            var validateResult = roomValidator.ValidateRoomJoin(roomName, password);

            if (validateResult.IsValid == false)
            {
                Debug.Log(validateResult.Message);
                return;
            }
            
            PhotonNetwork.JoinRoom(roomName);
        }
        private void LeaveRoom()
        {
            if (!PhotonNetwork.InRoom) return;
            
            PhotonNetwork.LeaveRoom();
        }

        #endregion

        #region Send Data Functions

        private void SendServerConnection()
        {
            var evt = new PunEvents.SendServerConnectionEvent(PhotonNetwork.IsConnected);
            EventHub.Instance?.RaiseEvent(evt);
        }

        #endregion
        
        
        public List<RoomInfo> GetRoomList() => cachedRoom.Values.ToList();

        
        private void ChangeRoomOptions(bool isOpen, bool isVisible, int maxPlayerCount = 0)
        {
            if (PhotonNetwork.InRoom == false) return;
            if (PhotonNetwork.IsMasterClient == false) return;
            
            PhotonNetwork.CurrentRoom.IsOpen = isOpen;
            PhotonNetwork.CurrentRoom.IsVisible = isVisible;
            if (maxPlayerCount > 0) PhotonNetwork.CurrentRoom.MaxPlayers = maxPlayerCount;
        }
        private void QualityCheck()
        {
            if (PhotonNetwork.IsConnected == false || PhotonNetwork.InRoom == false) return;
            _lastQualityCheckTime = PhotonNetwork.Time;
            
            var ping = PhotonNetwork.GetPing();
            
            if (ping <= 50) PhotonNetwork.SerializationRate = 30;
            else if (ping <= 100) PhotonNetwork.SerializationRate = 20;
            else PhotonNetwork.SerializationRate = 10;
        }

        #region Photon Callbacks
        
        public override void OnConnectedToMaster()
        {
            EventHub.Instance?.RaiseEvent(new PunEvents.OnConnectEvent());
            PhotonNetwork.JoinLobby();
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            EventHub.Instance?.RaiseEvent(new PunEvents.OnDisconnectEvent(cause));
        }


        public override void OnJoinedLobby()
        {
            EventHub.Instance?.RaiseEvent(new PunEvents.OnJoinedLobbyEvent());
        }
        public override void OnLeftLobby()
        {
            EventHub.Instance?.RaiseEvent(new PunEvents.OnLeftLobbyEvent());
        }


        public override void OnCreatedRoom()
        {
            EventHub.Instance?.RaiseEvent(new PunEvents.OnCreateRoomEvent());
        }
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            EventHub.Instance.RaiseEvent(new PunEvents.OnCreateRoomFailedEvent(returnCode, message));
        }

        
        public override void OnJoinedRoom()
        {
            EventHub.Instance?.RaiseEvent(new PunEvents.OnJoinedRoomEvent());
            
            PhotonNetwork.IsMessageQueueRunning = false;
            SceneController.Instance.ChangeSceneWithLoading("04.InGame");
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            EventHub.Instance?.RaiseEvent(new PunEvents.OnJoinedRoomFailedEvent(returnCode, message));
        }
        public override void OnLeftRoom()
        {
            EventHub.Instance?.RaiseEvent(new PunEvents.OnLeftRoomEvent());
            SceneController.Instance.ChangeSceneWithLoading("02.Main");
        }
        
        
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            EventHub.Instance?.RaiseEvent(new PunEvents.OnPlayerJoinedEvent(newPlayer));
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            EventHub.Instance?.RaiseEvent(new PunEvents.OnPlayerLeftEvent(otherPlayer));
        }


        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (var info in roomList)
            {
                if (info.RemovedFromList) cachedRoom.Remove(info.Name);
                else cachedRoom[info.Name] = info;
            }
            
            EventHub.Instance?.RaiseEvent(new PunEvents.OnRoomListUpdateEvent(roomList));
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.NetworkClientState == ClientState.ConnectedToGameServer)
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        #endregion
    }
}
