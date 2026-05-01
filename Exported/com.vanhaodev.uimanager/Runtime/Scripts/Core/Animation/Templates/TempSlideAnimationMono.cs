using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace vanhaodev.uimanager.effect.templates
{
    public class TempSlideAnimationMono : UIAnimationMonoBase
    {
        [SerializeField] private Image _fadePanel;
        [SerializeField] private RectTransform _tfWindow;
        [SerializeField] private float _fadeAlpha = 0.5f;
        [SerializeField] private float _duration = 0.3f;

        private CanvasGroup _windowCanvasGroup;

        private void Awake()
        {
            if (_tfWindow != null && !_tfWindow.TryGetComponent(out _windowCanvasGroup))
                _windowCanvasGroup = _tfWindow.gameObject.AddComponent<CanvasGroup>();
        }

        public override void PlayShow(GameObject target, Action onComplete)
        {
            base.PlayShow(target, onComplete);

            float height = _tfWindow.rect.height;
            float offset = height + 50f;

            // Set initial state immediately to prevent jitter
            _tfWindow.anchoredPosition = new Vector2(0, -offset);
            if (_windowCanvasGroup != null)
                _windowCanvasGroup.alpha = 0f;

            if (_fadePanel != null)
            {
                var c = _fadePanel.color;
                c.a = 0f;
                _fadePanel.color = c;
                _fadePanel.raycastTarget = true;
            }

            var seq = DOTween.Sequence();

            if (_fadePanel != null)
                seq.Join(_fadePanel.DOFade(_fadeAlpha, _duration));

            seq.Join(_tfWindow.DOAnchorPosY(0, _duration).SetEase(Ease.OutCubic));

            if (_windowCanvasGroup != null)
                seq.Join(_windowCanvasGroup.DOFade(1f, _duration * 0.5f));

            seq.OnComplete(() => onComplete?.Invoke());
        }

        public override void PlayClose(GameObject target, Action onComplete)
        {
            base.PlayClose(target, onComplete);

            float height = _tfWindow.rect.height;
            float offset = height + 50f;

            var seq = DOTween.Sequence();

            if (_fadePanel != null)
                seq.Join(_fadePanel.DOFade(0f, _duration));

            seq.Join(_tfWindow.DOAnchorPosY(-offset, _duration).SetEase(Ease.InCubic));

            if (_windowCanvasGroup != null)
                seq.Join(_windowCanvasGroup.DOFade(0f, _duration));

            seq.OnComplete(() =>
            {
                if (_fadePanel != null)
                    _fadePanel.raycastTarget = false;

                onComplete?.Invoke();
            });
        }
    }
}
