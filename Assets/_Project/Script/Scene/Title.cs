using System;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using DG.Tweening;
using UnityEngine;

namespace _Project.Script.Scene
{
    public class Title : MonoBehaviour
    {
        [SerializeField] private RectTransform anyKeyPanel;
        [SerializeField] private RectTransform titlePanel;

        private Sequence anyKeyPanelSequence;
        private bool isInputEnabled = false;
        private void Awake()
        {
            EventHub.Instance.RegisterEvent<ManagerInitCompleteEvent>(ShowAnyKeyPanel);

            var anyKeyGroup = anyKeyPanel.GetComponent<CanvasGroup>();
            anyKeyGroup.DOFade(0, 0);

            var titleGroup = titlePanel.GetComponent<CanvasGroup>();
            titleGroup.DOFade(0, 0);

            ShowTitlePanel();
        }

        private void Update()
        {
            if (isInputEnabled && Input.anyKeyDown)
            {
                EventHub.Instance.RaiseEvent(new AnyInputEvent());
            }
        }

        private void OnDestroy()
        {
            EventHub.Instance?.UnregisterEvent<ManagerInitCompleteEvent>(ShowAnyKeyPanel);
        }

        private void ShowTitlePanel()
        {
            var canvasGroup = titlePanel.GetComponent<CanvasGroup>();

            var fadeTween = canvasGroup.DOFade(1, 2f).SetEase(Ease.OutSine).SetDelay(0.2f);

            if (anyKeyPanelSequence.IsActive()) anyKeyPanelSequence.Kill();
            anyKeyPanelSequence = DOTween.Sequence();

            anyKeyPanelSequence.Join(fadeTween);
        }

        private void ShowAnyKeyPanel(ManagerInitCompleteEvent args)
        {
            var canvasGroup = anyKeyPanel.GetComponent<CanvasGroup>();

            var fadeTween = canvasGroup.DOFade(1, 2f).SetEase(Ease.OutSine);
            var moveTween = anyKeyPanel.DOAnchorPos(new Vector2(0, 100f), 2f).SetEase(Ease.OutSine);

            if (anyKeyPanelSequence.IsActive()) anyKeyPanelSequence.Kill();
            anyKeyPanelSequence = DOTween.Sequence();

            anyKeyPanelSequence.Join(fadeTween);
            anyKeyPanelSequence.Join(moveTween);

            anyKeyPanelSequence.OnComplete(RegisterAnyKeyEvent);
            return;

            void RegisterAnyKeyEvent()
            {
                EventHub.Instance.RegisterEvent<AnyInputEvent>(ChangeScene);
                isInputEnabled = true;
            }
        }
        private void ChangeScene(AnyInputEvent args)
        {
            EventHub.Instance.UnregisterEvent<AnyInputEvent>(ChangeScene);
            SceneController.Instance.ChangeScene("02.Main");
        }
    }
}
