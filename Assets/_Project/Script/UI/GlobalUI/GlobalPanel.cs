using System;
using UnityEngine;
using DG.Tweening;

namespace _Project.Script.UI.GlobalUI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class GlobalPanel : MonoBehaviour
    {
        public abstract GlobalPanelType UIType { get; }

        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float animateTime = 0.5f;
        [SerializeField] protected Ease animateType;

        protected Sequence _sequence;

        public virtual void Initialize() => Hide();
        
        public abstract void Show(object param, Action<object> callback);
        public abstract void Hide();


        protected virtual void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
