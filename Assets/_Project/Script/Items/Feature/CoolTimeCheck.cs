using System;
using Photon.Pun;
using UnityEngine;

namespace _Project.Script.Items.Feature
{
    [Serializable]
    public class CoolTimeParam : FeatureParam
    {
        public float coolTime = 1f;
    }
    
    [CreateAssetMenu(fileName = "CoolTimeFeature", menuName = "ScriptableObjects/Item/ItemFeature/CoolTimeCheck")]
    public class CoolTimeCheck : ItemFeature
    {
        public override Type ParamType => typeof(CoolTimeParam);
        public override FeatureParam CreateDefaultParam() => new CoolTimeParam();
        
        private const double PhotonTime = 4294967.295d;

        public override FeatureResult CanUse(ItemInstance instance, FeatureParam param)
        {
            if (instance == null) return FeatureResult.Block;

            var p = param as CoolTimeParam ?? (CoolTimeParam)CreateDefaultParam();
            if (p.coolTime <= 0f) return FeatureResult.Ok;
            
            double now = GetNow();
            double last = instance.lastUseTime;
            
            if (last <= 0) return FeatureResult.Ok;
            
            double elapsed = (now - last + PhotonTime) % PhotonTime;
            
            bool ready = elapsed >= p.coolTime;
            return ready ? FeatureResult.Ok : FeatureResult.Block;
        }

        public override void AfterUse(ItemInstance instance, FeatureParam param)
        {
            if (instance == null) return;
            instance.lastUseTime = (float)GetNow();
        }

        private double GetNow()
        {
            #if PHOTON_UNITY_NETWORKING
            return PhotonNetwork.Time;
            #else
            return Time.realtimeSinceStartupAsDouble
            #endif
        }
    }
}
