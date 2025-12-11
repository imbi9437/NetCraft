using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Script.Items.Feature;
using UnityEngine;

namespace _Project.Script.Items
{
    [Serializable]
    public class ItemInstance
    {
        public ItemData itemData;
        public int count;
        public float lastUseTime;
        public float durability;

        /// <summary>
        /// InventorySlot에서 EquipSlot으로 데이터 이동시 같은 DataManager내에서 움직이는 것이므로 원본데이터를 그대로 쓰기보단 복사본을 사용하는 편이 안정적으로 보임
        /// </summary>
        /// <returns></returns>
        public ItemInstance Clone()
        {
            return new ItemInstance
            {
                itemData = this.itemData,
                count = this.count,
                lastUseTime = this.lastUseTime,
                durability = this.durability
            };
        }

        /// <summary>
        /// 하나씩 쪼개기
        /// </summary>
        /// <returns></returns>
        public ItemInstance SplitOne()
        {
            return new ItemInstance
            {
                itemData = this.itemData,
                count = 1,
                lastUseTime = this.lastUseTime,
                durability = this.durability
            };
        }


        private IEnumerable<ItemFeatureConfig> GetOrderFeatures()
        {
            if (itemData == null || itemData.features == null) yield break;

            foreach (var config in itemData.features) config?.EnsureParam();

            var ordered = itemData.features.Where(c => c?.feature != null).OrderBy(c => c.feature.order);
            foreach (var config in ordered) yield return config;
        }
        
        /// <summary>
        /// 사용템은 이걸로 사용하는듯?
        /// </summary>
        /// <returns></returns>
        public bool TryUse()
        {
            if (itemData == null) return false;
            
            foreach (var config in GetOrderFeatures())
            {
                var can = config.feature.CanUse(this, config.param);
                if (can == FeatureResult.Block) return false;
            }

            foreach (var config in GetOrderFeatures())
            {
                var can = config.feature.CanUse(this, config.param);
                if (can == FeatureResult.Skip) continue;
                
                config.feature.BeforeUse(this, config.param);
                config.feature.Use(this, config.param);
                config.feature.AfterUse(this, config.param);;
            }
            
            return true;
        }
        
        public bool TryGetFeature<T>(out T feature) where T : ItemFeature
        {
            feature = null;
            return itemData && itemData.TryGetFeature(out feature);
        }
        public bool TryGetFeatureParam<T>(out T param) where T : FeatureParam
        {
            param = null;
            return itemData && itemData.TryGetFeatureParam(out param);
        }

        public int GetMaxStack() => itemData.GetMaxStack();
        public string GetCountText()
        {
            string text = TryGetFeatureParam(out DurabilityParam param) ? $"{durability / param.maxDurability:P0}" : count.ToString();
            return text;
        }
    }
}
