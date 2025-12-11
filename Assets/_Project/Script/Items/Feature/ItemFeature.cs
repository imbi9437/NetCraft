using System;
using _Project.Script.Interface;
using UnityEngine;

namespace _Project.Script.Items.Feature
{
    public enum FeatureResult
    {
        Ok,
        Skip,
        Block,
    }
    
    public abstract class FeatureParam { }
    
    public abstract class ItemFeature : ScriptableObject, IItemHook
    {
        public int order = 0;
        
        public virtual Type ParamType => typeof(FeatureParam);
        public virtual FeatureParam CreateDefaultParam() => null;
        
        public virtual FeatureResult CanUse(ItemInstance instance, FeatureParam param) => FeatureResult.Ok;

        public virtual void BeforeUse(ItemInstance instance, FeatureParam param) { }
        public virtual void Use(ItemInstance instance, FeatureParam param) { }
        public virtual void AfterUse(ItemInstance instance, FeatureParam param) { }
    }
}
