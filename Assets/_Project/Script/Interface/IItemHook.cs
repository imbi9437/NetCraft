using _Project.Script.Items;
using _Project.Script.Items.Feature;

namespace _Project.Script.Interface
{
    public interface IItemHook
    {
        public FeatureResult CanUse(ItemInstance instance, FeatureParam param);
        public void BeforeUse(ItemInstance instance, FeatureParam param);
        public void Use(ItemInstance instance, FeatureParam param);
        public void AfterUse(ItemInstance instance, FeatureParam param);
    }
}
