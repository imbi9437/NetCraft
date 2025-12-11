using _Project.Script.Generic;
using System.Collections.Generic;
using UnityEngine;
using static _Project.Script.EventStruct.EnvironmentEvents;

namespace _Project.Script.Manager
{
    public partial class DataManager
    {
        /// <summary> 스탯감소 </summary>
        public void PlayersStatEnvironmentEffect(float hungerDecreaseAmount, float thirstDecreaseAmount)
        {
            if(localUserData == null || localUserData.worldData == null || localUserData.worldData.playerData == null)
            {
                Debug.LogWarning("Local user data or world data is not initialized.");
                return;
            }
            Dictionary<string, PlayerData> playerDict = localUserData.worldData.playerData;
            foreach (var kvp in playerDict)
            {
                var playerData = kvp.Value;
                if (!playerData.isJoined) continue;

                playerData.hunger -= hungerDecreaseAmount;

            }
            EventHub.Instance.RaiseEvent( new OnStatDecreaseEvent(hungerDecreaseAmount, thirstDecreaseAmount));
        }

        public void UpdateWorldEnvironmentTime(float currentHour)
        {
            if (localUserData == null || localUserData.worldData == null)
            {
                return;
            }
            localUserData.worldData.currentHour = currentHour;
        }
    }
}
