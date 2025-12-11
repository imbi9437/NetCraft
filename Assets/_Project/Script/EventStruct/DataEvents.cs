using _Project.Script.Interface;

namespace _Project.Script.EventStruct
{
    public static class DataEvents
    {
        public struct CompleteInitUserDataEvent : IEvent
        {
            public bool isSuccess;

            public CompleteInitUserDataEvent(bool isSuccess)
            {
                this.isSuccess = isSuccess;
            }
        }
        
        
        
        public struct RequestCreateNewWorldEvent : IEvent
        {
            public string worldName;
            public string password;
            public bool isPublic;
            public int maxPlayers;

            public RequestCreateNewWorldEvent(string worldName, string password, bool isPublic, int maxPlayers)
            {
                this.worldName = worldName;
                this.password = password;
                this.isPublic = isPublic;
                this.maxPlayers = maxPlayers;
            }
        }

        public struct RequestLoadWorldEvent : IEvent
        {
            public string worldName;
            public string password;
            public bool isPublic;
            public int maxPlayers;

            public RequestLoadWorldEvent(string worldName, string password, bool isPublic, int maxPlayers)
            {
                this.worldName = worldName;
                this.password = password;
                this.isPublic = isPublic;
                this.maxPlayers = maxPlayers;
            }
        }



        public struct RequestWorldDataExistEvent : IEvent { }
        public struct SendWorldDataExistEvent : IEvent
        {
            public bool isExist;
            public SendWorldDataExistEvent(bool isExist) => this.isExist = isExist;
        }
    }
}
