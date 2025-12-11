using System;
using System.Collections.Generic;
using _Project.Script.Character.Network;
using _Project.Script.Core;
using UnityEngine;

namespace _Project.Script.Items.Feature
{
    [Serializable]
    public class EdibleParam : FeatureParam
    {
        public List<StatModifier> statModifiers;
    }
    
    [CreateAssetMenu(fileName = "EdibleFeature", menuName = "ScriptableObjects/Item/ItemFeature/Edible")]
    public class EdibleFeature : ItemFeature
    {
        public override Type ParamType => typeof(EdibleParam);
        public override FeatureParam CreateDefaultParam() => new EdibleParam();

        public override FeatureResult CanUse(ItemInstance instance, FeatureParam param)
        {
            var p = param as EdibleParam;
            if (p == null) return FeatureResult.Block;
            if (p.statModifiers == null || p.statModifiers.Count <= 0) return FeatureResult.Block;
            if (instance.count <= 0) return FeatureResult.Block;
            
            return FeatureResult.Ok;
        }
        
        public override void Use(ItemInstance instance, FeatureParam param)
        {
            var p = param as EdibleParam;
            if (p == null || p.statModifiers == null || p.statModifiers.Count <= 0) return;

            foreach (var modifier in p.statModifiers)
            {
                Debug.Log($"아이템 섭취로 인한 스탯 회복 : {modifier.statType}을 {modifier.value}만큼 회복");
                // TODO : 스탯 변화 추가
            }
        }
    }
}
