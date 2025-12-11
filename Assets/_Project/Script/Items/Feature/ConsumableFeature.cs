using System;
using UnityEngine;

namespace _Project.Script.Items.Feature
{
    [Serializable]
    public class ConsumableParam : FeatureParam
    {
        public int consumeAmount = 1;
    }
    
    [CreateAssetMenu(fileName = "ConsumableFeature", menuName = "ScriptableObjects/Item/ItemFeature/Consumable")]
    public class ConsumableFeature : ItemFeature
    {
        public override Type ParamType => typeof(ConsumableParam);
        public override FeatureParam CreateDefaultParam() => new ConsumableParam();

        public override FeatureResult CanUse(ItemInstance instance, FeatureParam param)
        {
            if (instance == null) return FeatureResult.Block;
            
            var p = param as ConsumableParam;
            if (p == null) return FeatureResult.Block;
            
            var need = Mathf.Min(1, p.consumeAmount);
            return instance.count >= need ? FeatureResult.Ok : FeatureResult.Block;
        }

        public override void AfterUse(ItemInstance instance, FeatureParam param)
        {
            if (instance == null) return;
            
            var p = param as ConsumableParam;
            if (p == null) return;

            var consume = Mathf.Max(1, p.consumeAmount);
            instance.count = Mathf.Max(0, instance.count - consume);
        }
    }
}
