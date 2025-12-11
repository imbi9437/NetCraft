using System;
using _Project.Script.Items;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Script.UI.GlobalUI
{
    public class ItemDragPanel : GlobalPanel
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text count;
        private ItemInstance _dragItem;
        public ItemInstance DragItem => _dragItem;
        private void Update() => SetPosition();

        public override GlobalPanelType UIType => GlobalPanelType.DragItem;
        public override void Show(object param, Action<object> callback)
        {
            if (param is not ItemInstance item) return;
            _dragItem = item;
            gameObject.SetActive(true);

            itemIcon.sprite = _dragItem.itemData.icon;
            count.text = _dragItem.GetCountText();
			canvasGroup.blocksRaycasts = false;
			// 레이아웃 강제 갱신 (텍스트 변경 후 즉시 크기 조정)
			Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

            if (_sequence.IsActive()) _sequence.Kill();
            _sequence = DOTween.Sequence();

            var tween = canvasGroup.DOFade(1, animateTime).SetEase(animateType);

            _sequence.Join(tween);
        }

        public override void Hide()
        {
            if (_sequence.IsActive()) _sequence.Kill();
            _sequence = DOTween.Sequence();

            var fadeTween = canvasGroup.DOFade(0, animateTime).SetEase(animateType);

            _sequence.Join(fadeTween);
            _sequence.onComplete += () => gameObject.SetActive(false);
        }

        private void SetPosition()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            var pos = mouse.position.ReadValue();
            var rect = (RectTransform)transform;

            rect.position = pos;
        }


    }
}
