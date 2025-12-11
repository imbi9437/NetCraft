using _Project.Script.Data;
using _Project.Script.EventStruct;
using _Project.Script.Generic;
using _Project.Script.Items;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Photon.Pun;

namespace _Project.Script.Manager
{
    [DefaultExecutionOrder(-90)]
    public partial class DataManager : MonoSingleton<DataManager>
    {
	    private PlayerConfigData playerConfigData;
        public UserData localUserData;
        [SerializeField] public PlayerData localPlayerData = new(); //현재 월드의 내 플레이어 데이터

        #region UserData Getter Property

        public string NickName => localUserData.nickname;
        public string UserId => localUserData.uid;
        
        
        // 월드 데이터
        public string GetWorldName() => localUserData.worldData.worldName;
        public int GetWorldMaxPlayers() => localUserData.worldData.maxPlayers;
        
        
        // 플레이어 데이터
        

        #endregion
        
		protected override void Awake()
        {
            base.Awake();
            playerConfigData = Resources.Load<PlayerConfigData>("PlayerConfig");

            //UserData
            EventHub.Instance.RegisterEvent<FirebaseEvents.FirebaseLoginSuccess>(DownloadUserData);
            EventHub.Instance.RegisterEvent<FirebaseEvents.FirebaseRegisterSuccess>(CreateUserData);
            
            //WorldData
            EventHub.Instance.RegisterEvent<DataEvents.RequestWorldDataExistEvent>(RequestWorldDataExist);
            
            
            EventHub.Instance.RegisterEvent<DataEvents.RequestCreateNewWorldEvent>(CreateNewWorld);
            EventHub.Instance.RegisterEvent<DataEvents.RequestLoadWorldEvent>(LoadWorld);
            
            EventHub.Instance.RegisterEvent<PunEvents.OnCreateRoomEvent>(OnCreatedRoom);
            EventHub.Instance.RegisterEvent<PunEvents.OnLeftRoomEvent>(OnLeaveRoom);
            EventHub.Instance.RegisterEvent<PunEvents.OnPlayerJoinedEvent>(OnPlayerEntered);
            EventHub.Instance.RegisterEvent<PunEvents.OnPlayerLeftEvent>(OnPlayerLeft);
            
            
            localPlayerData.inventory.RegisterEventListener(EventHub.Instance.RaiseEvent, EventHub.Instance.RaiseEvent);
        }

		private void OnDestroy()
        {
	        //UserData
            EventHub.Instance?.UnregisterEvent<FirebaseEvents.FirebaseLoginSuccess>(DownloadUserData);
            EventHub.Instance?.UnregisterEvent<FirebaseEvents.FirebaseRegisterSuccess>(CreateUserData);
            
            //WorldData
            EventHub.Instance?.UnregisterEvent<DataEvents.RequestWorldDataExistEvent>(RequestWorldDataExist);
            
            EventHub.Instance?.UnregisterEvent<DataEvents.RequestCreateNewWorldEvent>(CreateNewWorld);
            EventHub.Instance?.UnregisterEvent<DataEvents.RequestLoadWorldEvent>(LoadWorld);
            
            EventHub.Instance?.UnregisterEvent<PunEvents.OnCreateRoomEvent>(OnCreatedRoom);
            EventHub.Instance?.UnregisterEvent<PunEvents.OnLeftRoomEvent>(OnLeaveRoom);
            EventHub.Instance?.UnregisterEvent<PunEvents.OnPlayerJoinedEvent>(OnPlayerEntered);
            EventHub.Instance?.UnregisterEvent<PunEvents.OnPlayerLeftEvent>(OnPlayerLeft);
        }


        #region User Data Functions

        
		private void DownloadUserData(FirebaseEvents.FirebaseLoginSuccess args)
		{
			FirebaseManager.Instance.DownloadData<UserData>(InitUserData).Forget();
		}
		private void CreateUserData(FirebaseEvents.FirebaseRegisterSuccess args)
        {
            UserData userData = new UserData(args.nickName, args.UserId);
            InitUserData(userData);
            
            FirebaseManager.Instance.UploadData(userData).Forget();
        }
        public void SaveUserData() => FirebaseManager.Instance.UploadData(localUserData).Forget();

        
        private void InitUserData(UserData data)
        {
	        localUserData = data;
	        
	        bool isSuccess = localUserData != null;
	        EventHub.Instance.RaiseEvent(new DataEvents.CompleteInitUserDataEvent(isSuccess));
        }
        
        
        #endregion


        #region World Data Functions

        private void RequestWorldDataExist(DataEvents.RequestWorldDataExistEvent evt) => SendWorldDataExist();
        private void SendWorldDataExist()
        {
	        var worldData = localUserData?.worldData;
	        bool isExist = worldData != null;
	        var evt = new DataEvents.SendWorldDataExistEvent(isExist);
	        EventHub.Instance.RaiseEvent(evt);
        }

        #endregion
        

        private void CreateNewWorld(DataEvents.RequestCreateNewWorldEvent evt)
        {
	        if (localUserData == null) return;
	        
	        localUserData.worldData = new WorldData(evt.worldName, evt.maxPlayers);
	        SaveUserData();

	        var request = new PunEvents.CreateRoomRequestEvent()
	        {
		        roomName = $"{localUserData.nickname}'s World : {evt.worldName}",
		        isPublic = evt.isPublic,
		        maxPlayers = evt.maxPlayers,
		        password = evt.password,
	        };
	        
	        if (PhotonNetwork.IsConnected) EventHub.Instance.RaiseEvent(request);
        }
        private void LoadWorld(DataEvents.RequestLoadWorldEvent evt)
        {
	        if (localPlayerData == null) return;
	        
	        string prevName = localUserData.worldData.worldName;
	        int prevMaxPlayers = localUserData.worldData.maxPlayers;
	        bool isChanged = false;
	        
	        if (prevName != evt.worldName)
	        {
		        isChanged = true;
		        localUserData.worldData.worldName = evt.worldName;
	        }
	        if (prevMaxPlayers != evt.maxPlayers)
	        {
		        isChanged = true;
		        localUserData.worldData.maxPlayers = evt.maxPlayers;
	        }
	        if (isChanged) SaveUserData();

	        var request = new PunEvents.CreateRoomRequestEvent()
	        {
		        roomName = $"{localUserData.nickname}'s World : {evt.worldName}",
		        isPublic = evt.isPublic,
		        maxPlayers = evt.maxPlayers,
		        password = evt.password,
	        };
	        
	        if (PhotonNetwork.IsConnected) EventHub.Instance.RaiseEvent(request);
        }


        private void OnCreatedRoom(PunEvents.OnCreateRoomEvent evt)
        {
	        string uid = localUserData.uid;
	        string nickName = localUserData.nickname;

	        if (localUserData.worldData.playerData.TryGetValue(uid, out var data) == false)
	        {
		        data = playerConfigData.CreatePlayerData(uid, nickName);
		        localUserData.worldData.playerData.Add(uid, data);
	        }
	        data.isJoined = true;
	        localPlayerData = data;
	        
	        SaveUserData();
        }
        private void OnLeaveRoom(PunEvents.OnLeftRoomEvent evt)
        {
	        if (PhotonNetwork.IsMasterClient == false) return;

	        string uid = localUserData.uid;

	        localUserData.worldData.playerData[uid].isJoined = false;
	        
	        SaveUserData();
        }

        
        private void OnPlayerEntered(PunEvents.OnPlayerJoinedEvent evt)
        {
	        if (PhotonNetwork.IsMasterClient == false) return;
	        
	        string uid = evt.player.CustomProperties["uid"].ToString();
	        string nickname = evt.player.NickName;

	        if (localUserData.worldData.playerData.TryGetValue(uid, out var data) == false)
	        {
		        data = playerConfigData.CreatePlayerData(uid, nickname);
		        localUserData.worldData.playerData.Add(uid, data);
	        }

	        localUserData.worldData.playerData[uid].isJoined = true;
	        string json = JsonConvert.SerializeObject(data);

	        PhotonNetwork.GetPhotonView(1).RPC(nameof(GetPlayerData), evt.player, json);
	        SaveUserData();
        }
        private void OnPlayerLeft(PunEvents.OnPlayerLeftEvent evt)
        {
	        if (PhotonNetwork.IsMasterClient == false) return;

	        string uid = evt.player.CustomProperties["uid"].ToString();

	        localUserData.worldData.playerData[uid].isJoined = false;
	        SaveUserData();
        }
        
		[PunRPC]
        private void GetPlayerData(string json, PhotonMessageInfo info)
        {
	        PlayerData data = JsonConvert.DeserializeObject<PlayerData>(json);
	        localPlayerData = data;
        }


        public void ChangePlayerData()
        {
	        Debug.Log("감소");
	        localPlayerData.hunger -= playerConfigData.RemainHunger;
	        localPlayerData.hunger = Mathf.Max(0, localPlayerData.hunger);
        }
        
        
        #region 조합 및 요리

        
        public void Craft(CraftRecipeData recipe, CraftStation currentStation)
        {
			if (recipe == null)
			{
				Debug.LogWarning("레시피가 설정되지 않았습니다.");
				return;
			}

			bool canCraft = CraftingManager.Instance.CanCraft(recipe, currentStation);
			if (!canCraft)
			{
				Debug.Log("재료 부족으로 조합 불가");
				return;
			}

			CraftingManager.Instance.RequestCraft(recipe, currentStation);

		}
		#endregion
    }
}
