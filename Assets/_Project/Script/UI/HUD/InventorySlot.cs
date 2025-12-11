using _Project.Script.EventStruct;
using _Project.Script.Items;
using _Project.Script.Manager;
using _Project.Script.UI.GlobalUI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Script.UI.HUD
{
    public class InventorySlot : ItemSlot
    {
        private TweenerCore<Vector3, Vector3, VectorOptions> _tween;


		private void OnDestroy()
        {
            EventHub.Instance?.UnregisterEvent<ItemEvents.InventorySlotChangedEvent>(OnChangedItemSlot);
        }

        public override void Initialize(HUDController controller, int index)
        {
            base.Initialize(controller, index);
            

            OnChangedItemSlot(new ItemEvents.InventorySlotChangedEvent(index, null));
            EventHub.Instance.RegisterEvent<ItemEvents.InventorySlotChangedEvent>(OnChangedItemSlot);
        }
        
        private void OnChangedItemSlot(ItemEvents.InventorySlotChangedEvent args)
        {
            if (args.index != Index) return;

            bool isEmpty = args.item == null || args.item.itemData == null || args.item.count <= 0;
            print($"isEmpty is true? {isEmpty == true}");
            iconImage.gameObject.SetActive(!isEmpty);
            itemCount.gameObject.SetActive(!isEmpty);
            
            if (isEmpty) Item = null;
            if (isEmpty) return;
            
            Item = args.item;
            iconImage.sprite = Item.itemData.icon;
            itemCount.text = args.item.GetCountText();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (_tween.IsActive()) _tween.Kill();
            _tween = transform.DOScale(1.2f, 0.1f);
            
            if (Item == null) return;

            //툴팁?
            GlobalUIManager.Instance.ShowPanel(GlobalPanelType.ItemTooltip, Item);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (_tween.IsActive()) _tween.Kill();
            _tween = transform.DOScale(1f, 0.1f);
            
            if (Item == null) return;
            GlobalUIManager.Instance.HidePanel(GlobalPanelType.ItemTooltip);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            // TODO : 아이템 사용 및 장착
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (Item == null) return;
            var dragItem = Item.Clone();
            GlobalUIManager.Instance.ShowPanel(GlobalPanelType.DragItem, dragItem);
        }


		public override void OnEndDrag(PointerEventData eventData)
		{
			GlobalUIManager.Instance.HidePanel(GlobalPanelType.DragItem);

			if(GlobalUIManager.Instance.TryGetPanel(GlobalPanelType.DragItem, out ItemDragPanel dragPanel))
			{
				ItemInstance dragItem = dragPanel.DragItem;
				var dropTarget = eventData.pointerEnter;
				bool droppedOnSlot = dropTarget != null && dropTarget.GetComponentInParent<ItemSlot>() != null;
				if (!droppedOnSlot)
				{
					DataManager.Instance.RemoveItem(dragItem);
					var go = new GameObject(dragItem?.itemData?.itemName ?? "ItemObject");
					var itemObject = go.AddComponent<ItemObject>();
					itemObject.Initialize(dragItem);
				}
			}

		}

        public override void OnDrop(PointerEventData eventData)
        {
            var sourceObject = eventData.pointerDrag;
            
            if (sourceObject == false) return;
            if (sourceObject.TryGetComponent(out ItemSlot slot) == false) return;
            if (ReferenceEquals(slot, this)) return;
            
            if (slot is InventorySlot inventorySlot)
            {
                if (inventorySlot.Index  == Index) return;
                DataManager.Instance.SwapItem(inventorySlot.Index, Index);
            }

            if(slot is EquipmentSlot equipmentSlot)
            {
                if(equipmentSlot.ItemInstance == null) return;
                    DataManager.Instance.UnequipItem(equipmentSlot.EquipmentType);
                return;
            }

            //Station의 아이템을 드래그앤 드롭으로 받기
            if(sourceObject.TryGetComponent(out StoredItemSlot fromStoredSlot))
            {
                var item = fromStoredSlot.slotitem;
                if (item == null) return;

                bool success = DataManager.Instance.TryAddItem(item, out bool isDestroy);
                if (success)
                {
					StoredItemPanelManager.Instance.stationStoredItem.Remove(item);
					StoredItemPanelManager.Instance.RefreshUI();
				}
            }
           
		}
    }
}
