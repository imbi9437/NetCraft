using _Project.Script.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using _Project.Script.UI;
using _Project.Script.Character.Player;
using _Project.Script.Data;
using UnityEngine;

namespace _Project.Script.Generic
{
    [Serializable]
    public class PlayerData
    {
        #region PlayerInfoData

        public string playerName;
        public bool isJoined;
        public bool isAlive = true;

        #endregion

        #region PlayerStatsData

        public float maxHp;
        public float maxHunger;
        public float maxSanity;

        public float hp;
        public float hunger;
        public float sanity;
        public float attack;
        public float defense;
        public float speed;

        #endregion

        #region ItemData
        
        public Equipment equippedItems = new();
        public Inventory inventory = new();
        
        #endregion
        
        #region SceneData
        
        public float posX;
        public float posY;
        public float posZ;
        
        public float rotX;
        public float rotY;
        public float rotZ;
        public float rotW;
        
        #endregion

        #region DataConvert

        public static void SetPosition(PlayerData data, Vector3 position)
        {
            data.posX = position.x;
            data.posY = position.y;
            data.posZ = position.z;
        }
        public static Vector3 GetPosition(PlayerData data)
        {
            return new Vector3(data.posX, data.posY, data.posZ);
        }

        public static void SetRotation(PlayerData data, Quaternion rotation)
        {
            data.rotX = rotation.x;
            data.rotY = rotation.y;
            data.rotZ = rotation.z;
            data.rotW = rotation.w;
        }
        public static Quaternion GetRotation(PlayerData data)
        {
            return new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
        }
        
        #endregion
    }
}
