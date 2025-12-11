using System.Collections.Generic;
using _Project.Script.EventStruct;
using _Project.Script.Generic;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Chat.Demo;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using AuthenticationValues = Photon.Chat.AuthenticationValues;
using EVT = _Project.Script.EventStruct.PhotonChatEvents;
using FirebaseEVT = _Project.Script.EventStruct.FirebaseEvents;

namespace _Project.Script.Manager
{
    public class ChatManager : MonoSingleton<ChatManager>, IChatClientListener
    {
        private ChatClient client;
    
        private List<string> channels;
    
        #region Unity Message Functions
    
        protected override void Awake()
        {
            base.Awake();
        
            client = new ChatClient(this);
            channels = new List<string>();
            
            EventHub.Instance?.RegisterEvent<DataEvents.CompleteInitUserDataEvent>(RequestConnectToServer);
            EventHub.Instance?.RegisterEvent<FirebaseEVT.FirebaseLogoutEvent>(RequestDisConnectToServer);
            
            
            EventHub.Instance?.RegisterEvent<EVT.RequestSubscribeToChatRoomEvent>(RequestSubscribeChannel);
            EventHub.Instance?.RegisterEvent<EVT.RequestUnsubscribeFromChatRoomEvent>(RequestUnSubscribeChannel);
            EventHub.Instance?.RegisterEvent<EVT.RequestSendChatMsgEvent>(RequestSendMessage);
        }
    
        private void Update()
        {
            client.Service();
        }

        private void OnDestroy()
        {
            EventHub.Instance?.UnregisterEvent<DataEvents.CompleteInitUserDataEvent>(RequestConnectToServer);
            EventHub.Instance?.UnregisterEvent<FirebaseEVT.FirebaseLogoutEvent>(RequestDisConnectToServer);
            
            
            EventHub.Instance?.UnregisterEvent<EVT.RequestSubscribeToChatRoomEvent>(RequestSubscribeChannel);
            EventHub.Instance?.UnregisterEvent<EVT.RequestUnsubscribeFromChatRoomEvent>(RequestUnSubscribeChannel);
            EventHub.Instance?.UnregisterEvent<EVT.RequestSendChatMsgEvent>(RequestSendMessage);
        }
    
        #endregion
    
        #region 이벤트 래퍼 함수
    
        
        private void RequestConnectToServer(DataEvents.CompleteInitUserDataEvent evt) => ConnectToChatServer();
        private void RequestDisConnectToServer(FirebaseEVT.FirebaseLogoutEvent evt) => DisconnectFromChatServer();
        
        
        private void RequestSubscribeChannel(EVT.RequestSubscribeToChatRoomEvent evt) => ConnectToChatRoom(evt.roomName);
        private void RequestUnSubscribeChannel(EVT.RequestUnsubscribeFromChatRoomEvent evt) => DisconnectFromChatRoom(evt.roomName);
        private void RequestSendMessage(EVT.RequestSendChatMsgEvent evt) => SendChatMsg(evt.roomIndex, evt.message);
    
        
        #endregion

        #region 채팅 서버 연결

    
        private void ConnectToChatServer()
        {
            string nickname = DataManager.Instance.NickName;
            client.AuthValues = new AuthenticationValues(nickname);
            AppSettings appSettings = PhotonNetwork.PhotonServerSettings.AppSettings;
            ChatAppSettings chatSetting = appSettings.GetChatSettings();
            client.ConnectUsingSettings(chatSetting);
        }
        private void DisconnectFromChatServer()
        {
            client.Disconnect();
        }

    
        private void ConnectToChatRoom(string roomName)
        {
            if (client.TryGetChannel(roomName, out var channel)) return;
            client.Subscribe(new[] { roomName });
        }
        private void DisconnectFromChatRoom(string roomName)
        {
            if (client.TryGetChannel(roomName, out var channel) == false) return;
            client.Unsubscribe(new[] { roomName });
        }

    
        #endregion

        private void SendChatMsg(int channelIndex, string message)
        {
            int index = Mathf.Clamp(channelIndex, 0, channels.Count - 1);
            var channel = channels[index];
        
            client.PublishMessage(channel, message);
        }
    
        #region PhotonChatCallbacks
    

        public void OnDisconnected()
        {
            EventHub.Instance?.RaiseEvent(new EVT.OnDisconnectFromChatServerEvent());
        }
        public void OnConnected()
        {
            EventHub.Instance?.RaiseEvent(new EVT.OnConnectToChatServerEvent());
        }
    
    
        public void OnSubscribed(string[] channels, bool[] results)
        {
            for (int i = 0; i < channels.Length; i++)
            {
                if (results[i] == false) continue;
                if (this.channels.Contains(channels[i])) continue;
                
                this.channels.Add(channels[i]);

                string message = $"<color=green>{client.AuthValues.UserId}이 입장하였습니다</color>";
                SendChatMsg(i,message);
                
                EventHub.Instance?.RaiseEvent(new EVT.OnSubscribeToChatRoomEvent(channels[i]));
            }
        }
        public void OnUnsubscribed(string[] channels)
        {
            for (int i = 0; i < channels.Length; i++)
            {
                if (this.channels.Contains(channels[i]) == false) continue;

                string message = $"<color=red>{PhotonNetwork.LocalPlayer.NickName}이 퇴장하였습니다</color>";
                SendChatMsg(i, message);
                
                this.channels.Remove(channels[i]);
                
                EventHub.Instance?.RaiseEvent(new EVT.OnUnsubscribeFromChatRoomEvent(channels[i]));
            }
        }

    
        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            for (int i = 0; i < senders.Length; i++)
            {
                var evt = new EVT.OnChatMsgReceivedEvent(channelName, senders[i], messages[i].ToString());
                EventHub.Instance.RaiseEvent(evt);
            }
        }
        public void OnPrivateMessage(string sender, object message, string channelName)
        {
        
        }

    
        public void OnUserSubscribed(string channel, string user)
        {
        }
        public void OnUserUnsubscribed(string channel, string user)
        {
        }
    
    
    
    
        public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
        {
        }
        public void OnChatStateChange(ChatState state)
        {

        }
        public void DebugReturn(DebugLevel level, string message)
        {
        
        }
    
        #endregion
    }
}
