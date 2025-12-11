using System;
using DG.Tweening;
using UnityEngine;

namespace _Project.Script.UI.Main
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class MainPanel : MonoBehaviour
    {
        public abstract MainMenuPanelType UIType { get; }
        
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float animateTime = 0.5f;
        [SerializeField] protected Ease animateType;

        private Sequence _sequence;

        public void Initialize()
        {
            canvasGroup.alpha = 0f;
            
            foreach (Transform child in transform)
            {
                child.localScale = Vector3.zero;
            }
            
            canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }

        public void Show()
        {
            if (gameObject.activeSelf) return;
            
            gameObject.SetActive(true);
            
            if (_sequence.IsActive()) _sequence.Kill();
            _sequence = DOTween.Sequence();
            
            var fadeTween = canvasGroup.DOFade(1, animateTime).SetEase(animateType);
            _sequence.Join(fadeTween);

            foreach (Transform child in transform)
            {
                var scaleTween = child.DOScale(1f, animateTime).SetEase(animateType);
                _sequence.Join(scaleTween);
            }
            
            
            _sequence.onComplete += () => canvasGroup.interactable = true;
        }

        public void Hide()
        {
            if (gameObject.activeSelf == false) return;
            
            if (_sequence.IsActive()) _sequence.Kill();
            _sequence = DOTween.Sequence();
            
            canvasGroup.interactable = false;
            
            var fadeTween = canvasGroup.DOFade(0, animateTime).SetEase(animateType);
            _sequence.Join(fadeTween);
            
            foreach (Transform child in transform)
            {
                var scaleTween = child.DOScale(0f, animateTime).SetEase(animateType);
                _sequence.Join(scaleTween);
            }
            
            _sequence.onComplete += () => gameObject.SetActive(false);
        }

        protected virtual void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
