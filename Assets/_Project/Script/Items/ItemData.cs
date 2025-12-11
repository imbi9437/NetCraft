using System.Collections.Generic;
using System.Linq;
using _Project.Script.Items.Feature;
using UnityEngine;

namespace _Project.Script.Items
{
    /// <summary>
    /// 돈스타브 특화 아이템 카테고리
    /// </summary>
    public enum ItemCategory
    {
        Food,       // 음식 (배고픔 회복)
        Tool,       // 도구 (채집 효율)
        Material,   // 재료 (제작용)
        Weapon,     // 무기 (전투용)
        Armor,      // 방어구 (피해 감소)
        Building    // 건축물 (구조물)
    }

    [CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData")]
    public class ItemData : ScriptableObject
    {
        public int uid;
        public string itemName;
        [TextArea(3, 5)] public string description;
        public ItemCategory category; // 돈스타브 특화 카테고리
        public int rarity;

        public GameObject visualPrefab;
        public Sprite icon;

        public List<ItemFeatureConfig> features;

        public bool TryGetFeature<T>(out T feature) where T : ItemFeature
        {
            feature = features.Find(c => c.feature is T)?.feature as T;
            return feature != null;
        }

        public bool TryGetFeatureParam<T>(out T param) where T : FeatureParam
        {
            var config = features.Find(c => c.param is T);
            
            config?.EnsureParam();
            param = config?.param as T;
            
            return param != null;
        }
        
        public int GetMaxStack() => TryGetFeatureParam(out StackableParam param) ? param.maxStack : 1;
    }
}
