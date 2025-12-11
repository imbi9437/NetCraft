using System;
using _Project.Script.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Script.UI.GlobalUI
{
    public class ConfirmPopupPanel : GlobalPanel
    {
        public override GlobalPanelType UIType => GlobalPanelType.ConfirmPopup;

        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text description;
        [SerializeField] private Button confirmButton;

        private Action _confirmAction;

        public override void Initialize()
        {
            base.Initialize();

            confirmButton.onClick.AddListener(OnConfirmButtonClick);
        }

        public override void Show(object param, Action<object> callback)
        {
            if (param is not PopupParam popupParam) return;

            gameObject.SetActive(true);
            SetPopupUI(popupParam);

            // 레이아웃 강제 갱신 (텍스트 변경 후 즉시 크기 조정)
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

            if (_sequence.IsActive()) _sequence.Kill();
            _sequence = DOTween.Sequence();

            var fadeTween = canvasGroup.DOFade(1, 0.1f).SetEase(Ease.OutBack);

            _sequence.Join(fadeTween);
        }

        public override void Hide()
        {
            if (_sequence.IsActive()) _sequence.Kill();
            _sequence = DOTween.Sequence();

            var fadeTween = canvasGroup.DOFade(0, 0.1f).SetEase(Ease.OutBack);

            _sequence.Join(fadeTween);
            _sequence.onComplete += () => gameObject.SetActive(false);
        }

        private void SetPopupUI(PopupParam param)
        {
            title.text = param.title;
            description.text = param.message;

            _confirmAction = param.confirm;
        }

        private void OnConfirmButtonClick()
        {
            _confirmAction?.Invoke();
            Hide();
        }
    }
}
