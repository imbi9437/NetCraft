using System;
using System.Collections.Generic;
using _Project.Script.Core;
using _Project.Script.Generic;
using UnityEngine;
using _Project.Script.Character.Network;
using _Project.Script.Manager;

namespace _Project.Script.Items.Feature
{
    [Serializable]
    public class EquipParam : FeatureParam
    {
        public EquipmentType type;
        public List<StatModifier> modifiers;
    }
    
    [CreateAssetMenu(fileName = "EquipFeature",menuName = "ScriptableObjects/Item/ItemFeature/Equip")]
    public class EquipFeature : ItemFeature
    {
        public override Type ParamType => typeof(EquipParam);
        public override FeatureParam CreateDefaultParam() => new EquipParam();

		//TODO : 장착시 스탯 변화 추가

		/// <summary>
		/// 장비장착시 이걸 사용해서 능력치를 적용시킴
		/// </summary>
		/// <param name="item"></param>
		/// <param name="param"></param>
		public override void Use(ItemInstance item, FeatureParam param) => Apply(item, param, true);

		/// <summary>
		/// 장비 해제시 이걸 호출시켜 능력치를 적용시킴
		/// </summary>
		/// <param name="item"></param>
		/// <param name="param"></param>
		public void Remove(ItemInstance item, FeatureParam param) => Apply(item, param, false);

		/// <summary>
		/// ItemInstance와 FeatureParma을 매개변수로 받아 아이템의 modifier를 가져와서 ApplyStatModifiers함수에 값을 넣어줌
		/// </summary>
		/// <param name="item"></param>
		/// <param name="param"></param>
		/// <param name="isApply"></param>
		public void Apply(ItemInstance item, FeatureParam param, bool isApply)
		{
			if (item == null || param is not EquipParam equipParam) return;

			var player = DataManager.Instance.localPlayerData;
			if (player == null) return;

			ApplyStatModifiers(player, equipParam.modifiers, isApply);
		}


		/// <summary>
		/// 장비 스탯 적용 함수
		/// 장착된 장비의 List<StatModifier>로부터 Stat을 받아오고 매개변수 isApplying에 따라 장비에 지정된 value를 true면 +(장착) false면 -(해제)로 적용되게 구성
		/// </summary>
		/// <param name="playerData"></param>
		/// <param name="modifiers"></param>
		/// <param name="isApplying"></param>
		private void ApplyStatModifiers(PlayerData playerData, List<StatModifier> modifiers, bool isApplying)
		{
			foreach (var modifier in modifiers)//장비의 모든 StatModifier를 가져옴
			{
				float baseValue = 0f;//playerData.GetStat(modifier.statType);//매개변수로 받은 플레이어 데이터(localPlayerData)의 현재 스탯을 가져옴
				float newValue = baseValue; //임의로 지정된 새 스탯에 넣고

				switch (modifier.modifierType)//타입에 따라 적용방식을 다르게함
				{
					case StatModifierType.Flat: //단순더하기 빼기
						newValue += isApplying ? modifier.value : -modifier.value;
						break;

					case StatModifierType.Percent: //%
						float factor = isApplying ? (1 + modifier.value) : (1 / (1 + modifier.value));
						newValue *= factor;
						break;

					case StatModifierType.Override:

						break;
				}

				//playerData.SetStat(modifier.statType, newValue);//새 스탯을 localPlayerData에 넣음

				Debug.Log($"[{(isApplying ? "Apply" : "Remove")}] {modifier.statType}: {baseValue} → {newValue} ({modifier.modifierType})");
			}
		}
	}


}
