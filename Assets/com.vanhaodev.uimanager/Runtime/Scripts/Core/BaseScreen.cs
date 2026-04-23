using System;
using UnityEngine;
using DG.Tweening;

namespace Vanhaodev.UIManager
{
    public abstract class BaseScreen : UIElement, IScreen
    {
        [Header("Screen Settings")]
        [SerializeField] protected bool _fadeInFromRight = false;
        [SerializeField] protected float _slideOffset = 100f;

        protected RectTransform _rectTransform;

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
        }

        public virtual void OnEnter(object data = null) { }

        public virtual void OnExit() { }

        public override void Show(Action onComplete = null)
        {
            KillCurrentTween();
            gameObject.SetActive(true);
            IsVisible = true;

            PlayShowAnimation(() =>
            {
                OnEnter();
                onComplete?.Invoke();
            });
        }

        public override void Hide(Action onComplete = null)
        {
            KillCurrentTween();
            OnExit();

            PlayHideAnimation(() =>
            {
                IsVisible = false;
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        protected override void PlayShowAnimation(Action onComplete)
        {
            _canvasGroup.alpha = 0f;

            if (_fadeInFromRight && _rectTransform != null)
            {
                var startPos = _rectTransform.anchoredPosition;
                _rectTransform.anchoredPosition = new Vector2(startPos.x + _slideOffset, startPos.y);

                DOTween.Sequence()
                    .Join(_canvasGroup.DOFade(1f, _animDuration).SetEase(_showEase))
                    .Join(_rectTransform.DOAnchorPosX(startPos.x, _animDuration).SetEase(_showEase))
                    .SetUpdate(true)
                    .OnComplete(() => onComplete?.Invoke());
            }
            else
            {
                base.PlayShowAnimation(onComplete);
            }
        }

        protected override void PlayHideAnimation(Action onComplete)
        {
            if (_fadeInFromRight && _rectTransform != null)
            {
                var startPos = _rectTransform.anchoredPosition;

                DOTween.Sequence()
                    .Join(_canvasGroup.DOFade(0f, _animDuration * 0.8f).SetEase(_hideEase))
                    .Join(_rectTransform.DOAnchorPosX(startPos.x - _slideOffset, _animDuration * 0.8f).SetEase(_hideEase))
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        _rectTransform.anchoredPosition = startPos;
                        onComplete?.Invoke();
                    });
            }
            else
            {
                base.PlayHideAnimation(onComplete);
            }
        }
    }
}
