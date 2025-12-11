using _Project.Script.EventStruct;
using _Project.Script.Generic;
using _Project.Script.Items;
using _Project.Script.Items.Feature;
using _Project.Script.Manager;
using _Project.Script.UI.GlobalUI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Script.UI.HUD
{
	//Inventory로부터 드래그된 ItemInstance의 ItemData의 ItemCategory가 Weapon혹은 Armor라면 EquipSlot의 Item으로 옮겨지도록
	public class EquipmentSlot : ItemSlot
    {
        private TweenerCore<Vector3, Vector3, VectorOptions> tween;
        private EquipmentType equipmentType;
        public EquipmentType EquipmentType => equipmentType;

        private void OnDestroy()
        {
            EventHub.Instance.UnregisterEvent<ItemEvents.EquipmentChangedEvent>(OnChangedEquipSlot);
        }

        public override void Initialize(HUDController controller, int index)
        {
            base.Initialize(controller, index);
            equipmentType = (EquipmentType)index;
            
            OnChangedEquipSlot(new ItemEvents.EquipmentChangedEvent(equipmentType, null));
            EventHub.Instance.RegisterEvent<ItemEvents.EquipmentChangedEvent>(OnChangedEquipSlot);
        }
        
        private void OnChangedEquipSlot(ItemEvents.EquipmentChangedEvent args)
        {
            if (args.type != equipmentType) return;

            bool isEmpty = args.item == null || args.item.itemData == null || args.item.count <= 0;
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
            if (tween.IsActive()) tween.Kill();
            tween = transform.DOScale(1.2f, 0.1f);
            
            // TODO : 액션 툴팁 표기
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (tween.IsActive()) tween.Kill();
            tween = transform.DOScale(1f, 0.1f);

            // TODO : 액션 툴팁 표기
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (Item == null) return;
                DataManager.Instance.UnequipItem(equipmentType);
            }
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
		}
		public override void OnDrop(PointerEventData eventData)
		{
			var sourceObject = eventData.pointerDrag;

			if (sourceObject == null) return;
			if (!sourceObject.TryGetComponent(out ItemSlot sourceSlot)) return;
			if (sourceSlot.ItemInstance == null) return;

			var item = sourceSlot.ItemInstance;
			var itemData = item.itemData;

            if (!itemData.TryGetFeatureParam(out EquipParam equipParam)) 
            {
                Debug.Log("현재 아이템은 장비 기능이 없음");
                return; 
            }

            if(equipParam.type != equipmentType)
            {
                Debug.Log("장착부위가 올바르지 않음");
                return;
            }
            if(sourceSlot is InventorySlot inventorySlot)
            {
                Debug.Log($"장착시도 + {inventorySlot.SlotIndex}");
                DataManager.Instance.EquipItem(inventorySlot.SlotIndex);
            }

		}
    }
}
