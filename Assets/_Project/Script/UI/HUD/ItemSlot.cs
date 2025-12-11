using System;
using _Project.Script.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Script.UI.HUD
{
    public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDropHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField] protected Image iconImage;
        [SerializeField] protected TMP_Text itemCount;

        protected HUDController Controller;
        protected ItemInstance Item;
        protected int Index;
        //
        public ItemInstance ItemInstance => Item;
        //
        public int SlotIndex => Index;

        public virtual void Initialize(HUDController controller, int index)
        {
            Controller = controller;
            Index = index;
            
        }

        public virtual void OnPointerEnter(PointerEventData eventData) { }  //툴팁 표기
        public virtual void OnPointerExit(PointerEventData eventData) { }   //툴팁 해제
        public virtual void OnPointerClick(PointerEventData eventData) { }  //아이템 사용 혹은 장착/해제
        public virtual void OnDrop(PointerEventData eventData) { }  // 아이템 스왑 혹은 장착/해제
        public virtual void OnBeginDrag(PointerEventData eventData) { }
        public virtual void OnEndDrag(PointerEventData eventData) { }
        public void OnDrag(PointerEventData eventData) { }
        
        
        public virtual void Reset()
        {
            iconImage = transform.Find("ICON").GetComponent<Image>();
            itemCount = transform.Find("Text").GetComponent<TMP_Text>();
        }

        
    }
}
