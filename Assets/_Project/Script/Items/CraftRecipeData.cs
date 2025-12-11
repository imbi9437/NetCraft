using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Script.Items
{
    public enum CraftTech
    {
        None = 0,
        Science,
        CookTable,
    }
    /// <summary>
    /// 레시피를 실행할 조건추가용 작업대
    /// </summary>
    public enum CraftStationType
    {
        None,
        Campfire,
        Furnace,
        Workbench,
    }
    /// <summary>
    /// 센서를 필요로하는 작업대를 등록하며 필요여부를 반환하는 함수를 가진 Helper
    /// </summary>
	public static class CraftStationHelper
	{
		private static readonly HashSet<CraftStationType> _requireSensor = new()
		{
			CraftStationType.Workbench,//여기에 등록
		};
		public static bool RequiresSensor(this CraftStationType type)
		{
			return _requireSensor.Contains(type);
		}
	}

    /// <summary>
    /// 레시피 해금용
    /// </summary>
    /// PlayerRecipeData에서 해금되는 HashSet Recipe를 갖게하고 Unlock기능을 넣어서 Unlock된 레시피 조건체크를 시키기
    public enum UnlockRecipe
    {
        None,
        BasicsSurvivor,
        amatureSurvivor,
        MasterSurvivor,
    }
	[Serializable]
    public class CraftRecipeElement
    {
        public ItemData item;
        public int count;
    }
	[CreateAssetMenu(fileName = "CraftRecipeData", menuName = "ScriptableObjects/CraftRecipeData")]
	public class CraftRecipeData : ScriptableObject
    {
        public string recipeId;
        public int order;
        
        public CraftTech tech;
        public CraftStationType stationType;
        public UnlockRecipe unlockRequirement;

        public float time;
        public int tier;

        public List<CraftRecipeElement> ingredients;
        public List<CraftRecipeElement> results;

       
    }
}
