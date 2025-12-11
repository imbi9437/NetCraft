using _Project.Script.Items;
using _Project.Script.UI.GlobalUI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Script.UI.HUD
{
	public class StoredItemSlot : ItemSlot
	{
		private TweenerCore<Vector3, Vector3, VectorOptions> _tween;
		public ItemInstance slotitem;
		/// <summary>
		/// 아이템 데이터를 받아서 슬롯에 적용
		/// </summary>
		public void SetItem(ItemInstance item)
		{
			Item = item;
			slotitem = Item;
			bool isEmpty = item == null || item.itemData == null || item.count <= 0;
			iconImage.gameObject.SetActive(!isEmpty);
			itemCount.gameObject.SetActive(!isEmpty);

			if (isEmpty)
			{
				iconImage.sprite = null;
				itemCount.text = "";
			}
			else
			{
				iconImage.sprite = item.itemData.icon;
				itemCount.text = item.count.ToString();
			}
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (_tween.IsActive()) _tween.Kill();
			_tween = transform.DOScale(1.2f, 0.1f);

			if (Item == null) return;

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
			// 저장 아이템은 클릭으로 사용되지 않음
		}

		public override void OnBeginDrag(PointerEventData eventData)
		{
			if(Item == null) return;
			print($"저장소 드래그시작");
			GlobalUIManager.Instance.ShowPanel(GlobalPanelType.DragItem, Item);
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			print($"저장소 드래그 끝");
			GlobalUIManager.Instance.HidePanel(GlobalPanelType.DragItem);
		}

		public override void OnDrop(PointerEventData eventData)
		{
			// 저장 아이템은 드롭 불가
		}
	}
}
