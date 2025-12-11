using _Project.Script.Interface;

namespace _Project.Script.EventStruct
{
    /// <summary> 포톤 채팅 관련 이벤트 구조체 </summary>
    public static class PhotonChatEvents
    {
        #region Photon Chat Callbacks Events

        /// <summary> 포톤 채팅 서버 연결 이벤트 </summary>
        public struct OnConnectToChatServerEvent : IEvent {}
        
        /// <summary> 포톤 채팅 서버 연결 해제 이벤트 </summary>
        public struct OnDisconnectFromChatServerEvent : IEvent {}

        /// <summary> 포톤 채팅방 구독 이벤트 </summary>
        public struct OnSubscribeToChatRoomEvent : IEvent
        {
            public string[] roomNames;

            public OnSubscribeToChatRoomEvent(params string[] roomNames)
            {
                this.roomNames = roomNames;
            }
        }

        /// <summary> 포톤 채팅방 구독 해지 이벤트 </summary>
        public struct OnUnsubscribeFromChatRoomEvent : IEvent
        {
            public string[] roomNames;
            
            public OnUnsubscribeFromChatRoomEvent(params string[] roomNames)
            {
                this.roomNames = roomNames;
            }
        }

        /// <summary> 포톤 채팅 메세지 수신 이벤트 </summary>
        public struct OnChatMsgReceivedEvent : IEvent
        {
            public string roomName;
            public string senderName;
            public string message;

            public OnChatMsgReceivedEvent(string roomName, string senderName, string message)
            {
                this.roomName = roomName;
                this.senderName = senderName;
                this.message = message;
            }
        }
        
        // 메세지 송신 이벤트는 따로 필요 없음

        #endregion

        #region Photon Chat Request Events

        /// <summary> 포톤 채팅방 구독 요청 이벤트 </summary>
        public struct RequestSubscribeToChatRoomEvent : IEvent
        {
            public string roomName;

            public RequestSubscribeToChatRoomEvent(string roomName)
            {
                this.roomName = roomName;
            }
        }

        /// <summary> 포톤 채팅방 구독 해지 요청 이벤트 </summary>
        public struct RequestUnsubscribeFromChatRoomEvent : IEvent
        {
            public string roomName;
            
            public RequestUnsubscribeFromChatRoomEvent(string roomName)
            {
                this.roomName = roomName;
            }
        }

        /// <summary> 포톤 채팅 메세지 송신 요청 이벤트 </summary>
        public struct RequestSendChatMsgEvent : IEvent
        {
            public int roomIndex;
            public string message;

            public RequestSendChatMsgEvent(int roomIndex, string message)
            {
                this.roomIndex = roomIndex;
                this.message = message;
            }
        }
        
        //메세지 수신 이벤트는 요청이 따로 필요 없음

        #endregion
        
        
    }
}
