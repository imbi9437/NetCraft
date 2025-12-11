using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Project.Script.Manager;
using Photon.Realtime;
using _Project.Script.EventStruct;
using _Project.Script.UI.Main;
using _Project.Script.UI.Main.Element;
using Photon.Pun;
using Photon.Voice.PUN;
using TMPro;
using UnityEngine.Events;


namespace _Project.Script.UI
{
    [Serializable]
    public class PasswordInputPopup
    {
        public GameObject popupObject;
        public TMP_InputField passwordInput;
        public Button confirmButton;
        public Button cancelButton;
    }
    
    public class LobbyPanel : MainPanel
    {
        public override MainMenuPanelType UIType => MainMenuPanelType.Lobby;
        
        [SerializeField] private Transform roomListContent;
        [SerializeField] private Button closeButton;
        [SerializeField] private RoomItem roomItemPrefab;
        [SerializeField] private PasswordInputPopup passwordInputPopup;

        public bool debug;
        
        private Dictionary<string, RoomItem> roomItemDic = new();

        private void OnEnable()
        {
            EnablePanelElement();
            
            EventHub.Instance?.RegisterEvent<PunEvents.OnRoomListUpdateEvent>(RoomListUpdateEvent);
        }

        private void OnDisable()
        {
            DisablePanelElement();
            
            EventHub.Instance?.UnregisterEvent<PunEvents.OnRoomListUpdateEvent>(RoomListUpdateEvent);
        }

        #region UI 초기화 함수

        
        private void EnablePanelElement()
        {
            closeButton.onClick.AddListener(Hide);
            
            passwordInputPopup.passwordInput.SetTextWithoutNotify("");
            passwordInputPopup.cancelButton.onClick.AddListener(() => passwordInputPopup.popupObject.SetActive(false));

            var evt = new PunEvents.OnRoomListUpdateEvent(MultiPlayManager.Instance.GetRoomList());
            RoomListUpdateEvent(evt);
            
            passwordInputPopup.popupObject.SetActive(false);
        }

        private void DisablePanelElement()
        {
            closeButton.onClick.RemoveListener(Hide);

            passwordInputPopup.cancelButton.onClick.RemoveAllListeners();
        }
        
        
        #endregion

        #region EventHub 래퍼 함수

        
        private void RoomListUpdateEvent(PunEvents.OnRoomListUpdateEvent evt) => UpdateRoomList(evt.roomList);

        
        #endregion
        
        private void UpdateRoomList(List<RoomInfo> updatedRooms)
        {
            if (updatedRooms == null || updatedRooms.Count <= 0) return;
            
            foreach (var info in updatedRooms)
            {
                if (info.RemovedFromList) RemoveRoomButton(info);
                else if (roomItemDic.ContainsKey(info.Name)) UpdateRoomButton(info);
                else CreateRoomButton(info);
            }
        }
        private void CreateRoomButton(RoomInfo roomInfo)
        {
            var item = Instantiate(roomItemPrefab, roomListContent, false);
            
            item.UpdateRoomItem(roomInfo, JoinTargetRoom);
            roomItemDic.Add(roomInfo.Name, item);
        }
        private void RemoveRoomButton(RoomInfo roomInfo)
        {
            if (roomItemDic.TryGetValue(roomInfo.Name, out RoomItem item) == false) return;
            
            Destroy(item.gameObject);
            roomItemDic.Remove(roomInfo.Name);
        }
        private void UpdateRoomButton(RoomInfo info)
        {
            if (roomItemDic.TryGetValue(info.Name, out var item) == false) return;
            
            item.UpdateRoomItem(info);
        }


        private void JoinTargetRoom(RoomInfo room)
        {
            if (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer &&
                PhotonNetwork.NetworkClientState != ClientState.JoinedLobby) return;
            if (room.CustomProperties.TryGetValue("roomType", out object isPublic) == false) return;

            if ((bool)isPublic) RequestJoinRoom(room.Name, "");
            else OpenPasswordPopup(() => RequestJoinRoom(room.Name, passwordInputPopup.passwordInput.text));
        }

        private void OpenPasswordPopup(UnityAction callback)
        {
            passwordInputPopup.popupObject.SetActive(true);
            passwordInputPopup.passwordInput.SetTextWithoutNotify("");
            passwordInputPopup.confirmButton.onClick.RemoveAllListeners();
            passwordInputPopup.confirmButton.onClick.AddListener(callback);
        }

        private void RequestJoinRoom(string roomName, string password)
        {
            var evt = new PunEvents.JoinRoomRequestEvent(roomName, password);
            EventHub.Instance.RaiseEvent(evt);
        }
    }
}