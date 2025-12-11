using System;
using UnityEngine;

namespace _Project.Script.Items.Feature
{
    [Serializable]
    public class DurabilityParam : FeatureParam
    {
        public float maxDurability = 100f;
        public float costPerUse = 1f;
    }
    
    public class DurabilityFeature : ItemFeature
    {
        public override Type ParamType => typeof(DurabilityParam);
        public override FeatureParam CreateDefaultParam() => new DurabilityParam();

        public override FeatureResult CanUse(ItemInstance instance, FeatureParam param)
        {
            var p = param as DurabilityParam;
            if (p == null) return FeatureResult.Ok;
            return instance.durability > 0 ? FeatureResult.Ok : FeatureResult.Block;
        }

        public override void AfterUse(ItemInstance instance, FeatureParam param)
        {
            var p = param as DurabilityParam;
            if (p == null) return;
            instance.durability -= Mathf.Max(0f, instance.durability - Mathf.Max(0, p.costPerUse));
        }
    }
}
