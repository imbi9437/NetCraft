using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Project.Script.Items;
using _Project.Script.Manager;
namespace _Project.Script.UI.HUD
{
    public class InventoryUIController : HUDController
    {
        private List<InventorySlot> _slots;

        private int slotIndex = -1;
        private int previousSlotIndex = -1;


        private void Awake()
        {
            _slots = GetComponentsInChildren<InventorySlot>(true).OrderBy(s => s.transform.GetSiblingIndex()).ToList();

            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].Initialize(this, i);
            }
            if (_slots.Count > 0)
            {
                slotIndex = 0;
                HighlightSlot(slotIndex);
            }

        }

        private void Update()
        {
            //마우스 휠로 슬롯바꾸기
            if (Input.mouseScrollDelta.y != 0)
            {
                previousSlotIndex = slotIndex;

                if (Input.mouseScrollDelta.y < 0)
                    slotIndex++;
                else
                    slotIndex--;

                if (slotIndex < 0)
                    slotIndex = _slots.Count - 1;
                else if (slotIndex >= _slots.Count)
                    slotIndex = 0;

                HighlightSlot(slotIndex, previousSlotIndex);
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                DropItemFromSlot(slotIndex);
            }
        }

        private void HighlightSlot(int currentIndex, int previousIndex = -1)
        {
            if (previousIndex >= 0 && previousIndex < _slots.Count)
                _slots[previousIndex].transform.localScale = Vector3.one;

            if (currentIndex >= 0 && currentIndex < _slots.Count)
                _slots[currentIndex].transform.localScale = Vector3.one * 1.2f;
        }

        /// <summary>
        /// 아이템 하나씩 빼기
        /// </summary>
        /// <param name="index"></param>
        private void DropItemFromSlot(int index)
        {
            if (index < 0 || index >= _slots.Count)
                return;

            var slot = _slots[index];
            var item = slot.ItemInstance;

            if (item == null || item.count <= 0)
                return;

            // 1개 분리
            ItemInstance oneItem = item.SplitOne();
            if (oneItem == null || oneItem.count <= 0)
                return;
            DataManager.Instance.RemoveItem(oneItem);
            // 게임 오브젝트 생성
            var go = new GameObject(oneItem.itemData.itemName ?? "DroppedItem");
            var itemObject = go.AddComponent<ItemObject>();
            itemObject.Initialize(oneItem);
        }

    }
}

