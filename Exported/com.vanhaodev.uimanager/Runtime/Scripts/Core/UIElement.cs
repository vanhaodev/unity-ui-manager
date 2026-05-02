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
            OnShowStart();
            OnShowAnimation(() =>
            {
                OnShowEnd();
                onComplete?.Invoke();
            });
        }

        public virtual void Close(Action onComplete = null)
        {
            OnCloseStart();
            OnCloseAnimation(() =>
            {
                IsVisible = false;
                gameObject.SetActive(false);
                OnCloseEnd();
                onComplete?.Invoke();
            });
        }

        public void ShowImmediate()
        {
            gameObject.SetActive(true);
            IsVisible = true;
            SetBlockOverlay(false);
            OnShowStart();
            OnShowEnd();
        }

        public void HideImmediate()
        {
            OnCloseStart();
            IsVisible = false;
            gameObject.SetActive(false);
            OnCloseEnd();
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

            ShowImmediate();
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

            HideImmediate();
            onComplete?.Invoke();
        }

        protected void SetBlockOverlay(bool active)
        {
            if (_blockOverlay != null)
                _blockOverlay.SetActive(active);
        }

        protected virtual void OnShowStart() { }
        protected virtual void OnShowEnd() { }
        protected virtual void OnCloseStart() { }
        protected virtual void OnCloseEnd() { }
    }
}
