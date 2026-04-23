using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Vanhaodev.UIManager
{
    public abstract class BasePopup : UIElement, IPopup
    {
        [Header("Popup Settings")]
        [SerializeField] protected Button _backgroundBtn;
        [SerializeField] protected Transform _panel;
        [SerializeField] protected int _priority = 0;
        [SerializeField] protected bool _closeOnBackgroundClick = true;

        [Header("Popup Animation")]
        [SerializeField] protected bool _useScaleAnimation = true;
        [SerializeField] protected float _scaleFrom = 0.8f;

        public int Priority => _priority;
        public bool CloseOnBackgroundClick => _closeOnBackgroundClick;

        protected override void Awake()
        {
            base.Awake();

            if (_backgroundBtn != null && _closeOnBackgroundClick)
            {
                _backgroundBtn.onClick.AddListener(OnBackgroundClicked);
            }
        }

        protected virtual void OnBackgroundClicked()
        {
            UIManager.Instance?.ClosePopup(this);
        }

        public virtual void OnPopupOpened() { }

        public virtual void OnPopupClosed() { }

        public override void Show(Action onComplete = null)
        {
            KillCurrentTween();
            gameObject.SetActive(true);
            IsVisible = true;

            PlayShowAnimation(() =>
            {
                OnPopupOpened();
                onComplete?.Invoke();
            });
        }

        public override void Hide(Action onComplete = null)
        {
            KillCurrentTween();

            PlayHideAnimation(() =>
            {
                OnPopupClosed();
                IsVisible = false;
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        protected override void PlayShowAnimation(Action onComplete)
        {
            _canvasGroup.alpha = 0f;

            if (_useScaleAnimation && _panel != null)
            {
                _panel.localScale = Vector3.one * _scaleFrom;

                DOTween.Sequence()
                    .Join(_canvasGroup.DOFade(1f, _animDuration).SetEase(_showEase))
                    .Join(_panel.DOScale(1f, _animDuration).SetEase(Ease.OutBack))
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
            if (_useScaleAnimation && _panel != null)
            {
                DOTween.Sequence()
                    .Join(_canvasGroup.DOFade(0f, _animDuration * 0.7f).SetEase(_hideEase))
                    .Join(_panel.DOScale(_scaleFrom, _animDuration * 0.7f).SetEase(Ease.InBack))
                    .SetUpdate(true)
                    .OnComplete(() => onComplete?.Invoke());
            }
            else
            {
                base.PlayHideAnimation(onComplete);
            }
        }

        public void SetCloseOnBackgroundClick(bool value)
        {
            _closeOnBackgroundClick = value;
            if (_backgroundBtn != null)
                _backgroundBtn.interactable = value;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_backgroundBtn != null)
                _backgroundBtn.onClick.RemoveListener(OnBackgroundClicked);
        }
    }
}
