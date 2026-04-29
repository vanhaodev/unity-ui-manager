using System;
using UnityEngine;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIElement : MonoBehaviour
    {
        [SerializeField] protected GameObject _blockOverlay;

        protected CanvasGroup _canvasGroup;
        private IUIAnimation _animation;
        public virtual string Id => GetType().Name;
        public bool IsVisible { get; protected set; }
        public bool IsAnimating { get; private set; }

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
            SetBlockOverlay(false);
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
                SetBlockOverlay(true);
                IsAnimating = true;

                _animation.PlayShow(gameObject, () =>
                {
                    IsAnimating = false;
                    SetBlockOverlay(false);
                    onComplete?.Invoke();
                });
                return;
            }

            onComplete?.Invoke();
        }

        protected virtual void OnCloseAnimation(Action onComplete)
        {
            if (_animation != null)
            {
                SetBlockOverlay(true);
                IsAnimating = true;

                _animation.PlayClose(gameObject, () =>
                {
                    IsAnimating = false;
                    SetBlockOverlay(false);
                    onComplete?.Invoke();
                });
                return;
            }

            onComplete?.Invoke();
        }

        protected void SetBlockOverlay(bool active)
        {
            if (_blockOverlay != null)
                _blockOverlay.SetActive(active);
        }
    }
}
