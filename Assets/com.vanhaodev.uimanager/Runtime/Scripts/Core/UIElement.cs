using System;
using DG.Tweening;
using UnityEngine;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIElement : MonoBehaviour
    {
        protected CanvasGroup _canvasGroup;
        private IUIAnimation _animation;
        public virtual string Id => GetType().Name;
        public bool IsVisible { get; protected set; }

        /// <summary>
        /// Reference to the manager that created this element. Set by UIManager on instantiation.
        /// </summary>
        public UIManager Manager { get; internal set; }

        protected virtual void Awake()
        {
            if (!TryGetComponent(out _canvasGroup))
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void SetAnimation(IUIAnimation animation)
        {
            _animation = animation;
        }

        public virtual void Show(Action onComplete = null)
        {
            gameObject.SetActive(true);
            IsVisible = true;
            OnShowAnimation(() => onComplete?.Invoke());
        }

        public virtual void Close(Action onComplete = null)
        {
            OnCloseAnimation(() =>
            {
                IsVisible = false;
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        public void ShowImmediate()
        {
            gameObject.SetActive(true);
            IsVisible = true;
        }

        public void HideImmediate()
        {
            IsVisible = false;
            gameObject.SetActive(false);
        }

        protected virtual void OnShowAnimation(Action onComplete)
        {
            if (_animation != null)
            {
                _animation.PlayShow(gameObject, onComplete);
                return;
            }

            onComplete?.Invoke();
        }

        protected virtual void OnCloseAnimation(Action onComplete)
        {
            if (_animation != null)
            {
                _animation.PlayClose(gameObject, onComplete);
                return;
            }

            onComplete?.Invoke();
        }
    }
}