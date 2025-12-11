using System;
using UnityEngine;

namespace _Project.Script.Items.Feature
{
    [Serializable]
    public class StackableParam : FeatureParam
    {
        public int maxStack = 1;
        public bool IsStackable => maxStack > 1;
    }
    
    [CreateAssetMenu(fileName = "StackableFeature", menuName = "ScriptableObjects/Item/ItemFeature/Stackable")]
    public class StackableFeature : ItemFeature
    {
        public override Type ParamType => typeof(StackableParam);
        public override FeatureParam CreateDefaultParam() => new StackableParam();
    }
}
