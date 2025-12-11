using System;
using _Project.Script.Items;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Script.UI.GlobalUI
{
    public class ItemTooltipPanel : GlobalPanel
    {
        public override GlobalPanelType UIType => GlobalPanelType.ItemTooltip;

        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text itemCount;
        [SerializeField] private TMP_Text itemDescription;

        public override void Show(object param, Action<object> callback)
        {
            if (param is not ItemInstance itemInstance) return;

            SetItemInfoUI(itemInstance);
            gameObject.SetActive(true);

            if (_sequence.IsActive()) _sequence.Kill();
            _sequence = DOTween.Sequence();

            var fadeTween = canvasGroup.DOFade(1, animateTime).SetEase(animateType);

            _sequence.Join(fadeTween);
        }

        public override void Hide()
        {
            if (_sequence.IsActive()) _sequence.Kill();
            _sequence = DOTween.Sequence();

            var fadeTween = canvasGroup.DOFade(0, animateTime).SetEase(animateType);

            _sequence.Join(fadeTween);
        }

        private void SetItemInfoUI(ItemInstance instance)
        {
            ItemData data = instance.itemData;

            itemIcon.sprite = data.icon;
            itemName.text = data.itemName;
            itemDescription.text = data.description;

            itemCount.text = instance.count.ToString();
        }
    }
}
