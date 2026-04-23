using System;
using UnityEngine;
using DG.Tweening;

namespace Vanhaodev.UIManager
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIElement : MonoBehaviour, IUIElement
    {
        [Header("Animation")]
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected float _animDuration = 0.25f;
        [SerializeField] protected Ease _showEase = Ease.OutQuad;
        [SerializeField] protected Ease _hideEase = Ease.InQuad;

        public virtual string Id => GetType().Name;
        public bool IsVisible { get; protected set; }

        protected Tweener _currentTween;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Show(Action onComplete = null)
        {
            KillCurrentTween();
            gameObject.SetActive(true);
            IsVisible = true;
            PlayShowAnimation(onComplete);
        }

        public virtual void Hide(Action onComplete = null)
        {
            KillCurrentTween();
            PlayHideAnimation(() =>
            {
                IsVisible = false;
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        public void ShowImmediate()
        {
            KillCurrentTween();
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            IsVisible = true;
        }

        public void HideImmediate()
        {
            KillCurrentTween();
            _canvasGroup.alpha = 0f;
            IsVisible = false;
            gameObject.SetActive(false);
        }

        protected virtual void PlayShowAnimation(Action onComplete)
        {
            _canvasGroup.alpha = 0f;
            _currentTween = _canvasGroup
                .DOFade(1f, _animDuration)
                .SetEase(_showEase)
                .SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());
        }

        protected virtual void PlayHideAnimation(Action onComplete)
        {
            _currentTween = _canvasGroup
                .DOFade(0f, _animDuration)
                .SetEase(_hideEase)
                .SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());
        }

        protected void KillCurrentTween()
        {
            if (_currentTween != null && _currentTween.IsActive())
            {
                _currentTween.Kill();
                _currentTween = null;
            }
        }

        protected virtual void OnDestroy()
        {
            KillCurrentTween();
        }
    }
}
