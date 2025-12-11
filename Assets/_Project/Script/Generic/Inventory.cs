using System;
using _Project.Script.EventStruct;
using _Project.Script.Items;
using _Project.Script.Items.Feature;
using UnityEngine;

namespace _Project.Script.Generic
{
    [Serializable]
    public class Inventory
    {
        public ItemInstance[] itemSlot = new ItemInstance[15];

        private Action<ItemEvents.InventoryChangedEvent> _onInventoryChanged;
        private Action<ItemEvents.InventorySlotChangedEvent> _onInventorySlotChanged;
        
        public bool TryAddItem(ItemData data, int count, out int remain)
        {
            remain = count;
        
            if (data == null || count <= 0) return false;
        
            int maxStack = data.GetMaxStack();

            for (int i = 0; i < itemSlot.Length && remain > 0; i++)
            {
                var slot = itemSlot[i];
                if (slot == null || !ReferenceEquals(slot.itemData, data)) continue;
            
                int canAdd = maxStack - slot.count;
                if (canAdd <= 0) continue;
            
                int add = Math.Min(canAdd, remain);
                slot.count += add;
                remain -= add;
            
                RaiseSlotChangedEvent(i, slot);
            }

            for (int i = 0; i < itemSlot.Length && remain > 0; i++)
            {
                if (IsEmpty(i) == false) continue;
            
                int add = Mathf.Min(maxStack, remain);
                itemSlot[i] = new ItemInstance()
                {
                    itemData = data,
                    count = add
                };
            
                remain -= add;
                RaiseSlotChangedEvent(i, itemSlot[i]);
            }
        
            if (count != remain) RaiseChangedEvent();
            return remain != count;
        }
        
        public void RemoveItem(int itemIndex, int count = -1)
        {
            if (IsValidSlot(itemIndex) == false) return;
        
            var slot = itemSlot[itemIndex];
            if (slot == null) return;

            bool changed = false;
        
            if (count < 0 || count > slot.count)
            {
                itemSlot[itemIndex] = null;
                changed = true;
            
                RaiseSlotChangedEvent(itemIndex, null);
            }
			else if (count > 0)
			{
				slot.count -= count;
				if (slot.count <= 0)
				{
					itemSlot[itemIndex] = null;
					RaiseSlotChangedEvent(itemIndex, null);
				}
				else
				{
					RaiseSlotChangedEvent(itemIndex, slot);
				}
				changed = true;
			}

			if (changed) RaiseChangedEvent();
        }

        public void SwapItem(int fromIndex, int toIndex)
        {
            if (IsValidSlot(fromIndex) == false || IsValidSlot(toIndex) == false) return;

            if (itemSlot[fromIndex] == null) return;
            if (fromIndex == toIndex) return;

            var fromItem = itemSlot[fromIndex];
            var toItem = itemSlot[toIndex];
            bool changed = false;

            if (IsEmpty(toIndex))
            {   //빈 슬롯으로 이동
                itemSlot[fromIndex] = null;
                itemSlot[toIndex] = fromItem;
                changed = true;
            }
            else if (ReferenceEquals(fromItem.itemData, toItem.itemData))
            {   //아이템 머지
                int maxStack = 1;
                bool isStackable = fromItem.TryGetFeatureParam(out StackableParam param);
                if (isStackable) maxStack = param.maxStack;
            
                int count = maxStack - toItem.count;

                if (count >= fromItem.count)
                {
                    toItem.count += fromItem.count;
                    itemSlot[fromIndex] = null;
                    changed = true;
                }
                else if (count > 0)
                {
                    int remain = fromItem.count - count;
                    toItem.count += count;
                    itemSlot[fromIndex].count = remain;
                    changed = true;
                }
            }
            else
            {   //아이템 스왑
                itemSlot[fromIndex] = toItem;
                itemSlot[toIndex] = fromItem;
                changed = true;
            }

            if (!changed) return;
            RaiseSlotChangedEvent(fromIndex, itemSlot[fromIndex]);
            RaiseSlotChangedEvent(toIndex, itemSlot[toIndex]);
            RaiseChangedEvent();
        }

        public bool TakeOutItem(int index, int count, out ItemInstance item)
        {
            item = null;
            if (IsValidSlot(index) == false) return false;
            if (IsEmpty(index)) return false;
        
            int moveCount = count < 0 ? itemSlot[index].count : Mathf.Min(count, itemSlot[index].count);

            var slot = itemSlot[index];
        
            item = new ItemInstance()
            {
                itemData = slot.itemData,
                count = moveCount
            };
        
            slot.count -= moveCount;
            if (slot.count <= 0)
            {
                itemSlot[index] = null;
            }
        
            RaiseSlotChangedEvent(index, slot);
            RaiseChangedEvent();
            return true;
        }

        public bool TryUseItem(int index)
        {
            if (!IsValidSlot(index) || IsEmpty(index)) return false;
        
            var slot = itemSlot[index];
            bool used = slot.TryUse();

            if (used == false) return false;

            bool remove = slot.count <= 0;

            if (remove == false && slot.TryGetFeatureParam(out DurabilityParam param))
            {
                if (slot.durability <= 0f) remove = true;
            }
            
            if (remove) itemSlot[index] = null;
            
            RaiseSlotChangedEvent(index, slot);
            RaiseChangedEvent();
            return true;
        }
    
        #region 조회 / 유틸
    
        public ItemInstance GetItem(int slotIndex) => itemSlot[slotIndex];
        private bool IsEmpty(int slotIndex) => itemSlot[slotIndex] == null || itemSlot[slotIndex].count <= 0;
        private bool IsValidSlot(int slotIndex) => slotIndex >= 0 && slotIndex < itemSlot.Length;
        public int GetItemCount(int slotIndex) => itemSlot[slotIndex]?.count ?? 0;

        #endregion
    
        #region 이벤트 발행 함수
    
        private void RaiseChangedEvent() => _onInventoryChanged?.Invoke(new ItemEvents.InventoryChangedEvent());
        private void RaiseSlotChangedEvent(int index, ItemInstance item) =>
            _onInventorySlotChanged?.Invoke(new ItemEvents.InventorySlotChangedEvent(index, item));

        //UI확인용
        public void RegisterEventListener(Action<ItemEvents.InventoryChangedEvent> onInventoryChanged, Action<ItemEvents.InventorySlotChangedEvent> onInventorySlotChanged)
        {
            _onInventoryChanged += onInventoryChanged;
            _onInventorySlotChanged += onInventorySlotChanged;
        }

        #endregion
    }
}
