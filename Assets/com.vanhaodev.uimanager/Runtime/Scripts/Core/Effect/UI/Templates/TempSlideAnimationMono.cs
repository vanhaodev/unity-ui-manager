using System;
using System.Collections;
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

        public override void PlayShow(GameObject target, Action onComplete)
        {
            base.PlayShow(target, onComplete);
            target.SetActive(true);
            StartCoroutine(CoShow(onComplete));
        }

        private IEnumerator CoShow(Action onComplete)
        {
            yield return null;

            float height = _tfWindow.rect.height;
            float offset = height + 50f;

            _tfWindow.anchoredPosition = new Vector2(0, -offset);

            if (_fadePanel != null)
            {
                var c = _fadePanel.color;
                c.a = 0f;
                _fadePanel.color = c;
                _fadePanel.raycastTarget = true;
            }

            yield return null;

            var seq = DOTween.Sequence();

            if (_fadePanel != null)
                seq.Join(_fadePanel.DOFade(_fadeAlpha, _duration));

            seq.Join(_tfWindow.DOAnchorPosY(0, _duration).SetEase(Ease.OutCubic));

            seq.OnComplete(() => onComplete?.Invoke());
        }

        public override void PlayClose(GameObject target, Action onComplete)
        {
            base.PlayClose(target, onComplete);
            StartCoroutine(CoClose(target, onComplete));
        }

        private IEnumerator CoClose(GameObject target, Action onComplete)
        {
            float height = _tfWindow.rect.height;
            float offset = height + 50f;

            yield return null;

            var seq = DOTween.Sequence();

            if (_fadePanel != null)
                seq.Join(_fadePanel.DOFade(0f, _duration));

            seq.Join(_tfWindow.DOAnchorPosY(-offset, _duration).SetEase(Ease.InCubic));

            seq.OnComplete(() =>
            {
                if (_fadePanel != null)
                    _fadePanel.raycastTarget = false;

                target.SetActive(false);
                onComplete?.Invoke();
            });
        }
    }
}