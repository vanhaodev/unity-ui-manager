using System;
using UnityEngine;

namespace vanhaodev.uimanager
{
    public abstract class UIElement : MonoBehaviour
    {
        public virtual string Id => GetType().Name;
        public bool IsVisible { get; protected set; }

        /// <summary>
        /// Reference to the manager that created this element. Set by UIManager on instantiation.
        /// </summary>
        public UIManager Manager { get; internal set; }

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
            onComplete?.Invoke();
        }

        protected virtual void OnCloseAnimation(Action onComplete)
        {
            onComplete?.Invoke();
        }
    }
}
