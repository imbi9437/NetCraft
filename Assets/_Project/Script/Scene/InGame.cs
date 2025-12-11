using System;
using _Project.Script.EventStruct;
using _Project.Script.Generic;
using _Project.Script.Manager;
using Newtonsoft.Json;
using Photon.Pun;
using UnityEngine;

namespace _Project.Script.Scene
{
    public class InGame : MonoBehaviour
    {
        private PhotonView photonView;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
            PhotonNetwork.IsMessageQueueRunning = true;
        }

        private void Start()
        {
            StartConnectChatRoom();
            RequestRoomOpen();
            
            CreatePlayer();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                if (PhotonNetwork.IsMasterClient == false) return;

                foreach (var playerData in DataManager.Instance.localUserData.worldData.playerData)
                {
                    Debug.Log(JsonConvert.SerializeObject(playerData.Value));
                }
            }
        }

        private void OnDestroy()
        {
            EndConnectChatRoom();
        }


        private void StartConnectChatRoom()
        {
            if (PhotonNetwork.IsConnected == false) return;
            if (PhotonNetwork.InRoom == false) return;
            
            string roomName = PhotonNetwork.CurrentRoom.Name;
            var evt = new PhotonChatEvents.RequestSubscribeToChatRoomEvent(roomName);
            
            EventHub.Instance?.RaiseEvent(evt);
        }
        private void EndConnectChatRoom()
        {
            if (PhotonNetwork.IsConnected == false) return;
            if (PhotonNetwork.InRoom == false) return;
            
            string roomName = PhotonNetwork.CurrentRoom.Name;
            var evt = new PhotonChatEvents.RequestUnsubscribeFromChatRoomEvent(roomName);
            
            EventHub.Instance?.RaiseEvent(evt);
        }
        
        private void RequestRoomOpen()
        {
            if (PhotonNetwork.InRoom == false || PhotonNetwork.IsMasterClient == false) return;
            
            var evt = new PunEvents.ChangeRoomOptionsRequestEvent(true, true);
            EventHub.Instance.RaiseEvent(evt);
        }
        
        
        private void CreatePlayer()
        {
            if (PhotonNetwork.OfflineMode) Instantiate(Resources.Load<GameObject>("Player"));
            else
            {
                var pos = PlayerData.GetPosition(DataManager.Instance.localPlayerData);
                var rot = PlayerData.GetRotation(DataManager.Instance.localPlayerData);
                PhotonNetwork.Instantiate("Player", pos, rot);
            }
        }
    }
}
