using System;
using UnityEngine;

namespace _Project.Script.Items.Feature
{
    [Serializable]
    public class ItemFeatureConfig
    {
        public ItemFeature feature;
        
        [SerializeReference]
        public FeatureParam param;
        
        public void EnsureParam()
        {
            if (feature == null) return;

            if (param == null || (feature.ParamType != null && param.GetType() != feature.ParamType))
            {
                param = feature.CreateDefaultParam();
            }
        }
    }
}
