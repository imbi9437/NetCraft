using System;

namespace _Project.Script.Items.Feature
{
    [Serializable]
    public class PlaceableParam : FeatureParam
    {
        
    }
    
    public class PlaceableFeature : ItemFeature
    {
        public override Type ParamType => typeof(PlaceableParam);
        public override FeatureParam CreateDefaultParam() => new PlaceableParam();
        
        // TODO : 아이템 배치 추가
    }
}
