using System;
using System.Collections.Generic;

namespace _Project.Script.Generic
{
    [Serializable]
    public class WorldData
    {
        public string worldName;
        public int maxPlayers;
        
        public float currentHour = 0;
        
        
        public Dictionary<string, PlayerData> playerData = new();   //uid : 현재 월드의 UID 플레이어의 데이터


        public WorldData()
        {
            
        }

        public WorldData(string worldName, int maxPlayers)
        {
            this.worldName = worldName;
            this.maxPlayers = maxPlayers;
        }
    }
}
