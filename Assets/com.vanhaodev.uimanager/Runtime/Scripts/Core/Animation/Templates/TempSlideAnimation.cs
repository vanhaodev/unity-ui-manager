using System;
using DG.Tweening;
using UnityEngine;

namespace vanhaodev.uimanager.effect.templates
{
    public class TempSlideAnimation : UIAnimationBase
    {
        private readonly float _duration;

        public TempSlideAnimation(float duration = 0.3f)
        {
            _duration = duration;
        }

        public override void PlayShow(GameObject target, Action onComplete)
        {
            base.PlayShow(target, onComplete);

            var rect = target.transform as RectTransform;
            var cg = GetOrAddCanvasGroup(target);

            float height = rect.rect.height;
            float offset = height + 50f;

            // Set initial state immediately to prevent jitter
            rect.anchoredPosition = new Vector2(0, -offset);
            cg.alpha = 0f;

            var seq = DOTween.Sequence();
            seq.Join(rect.DOAnchorPosY(0, _duration).SetEase(Ease.OutCubic));
            seq.Join(cg.DOFade(1f, _duration * 0.5f));
            seq.OnComplete(() => onComplete?.Invoke());
        }

        public override void PlayClose(GameObject target, Action onComplete)
        {
            base.PlayClose(target, onComplete);

            var rect = target.transform as RectTransform;
            var cg = GetOrAddCanvasGroup(target);

            float height = rect.rect.height;
            float offset = height + 50f;

            var seq = DOTween.Sequence();
            seq.Join(rect.DOAnchorPosY(-offset, _duration).SetEase(Ease.InCubic));
            seq.Join(cg.DOFade(0f, _duration));
            seq.OnComplete(() => onComplete?.Invoke());
        }

        private CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            if (!go.TryGetComponent(out CanvasGroup cg))
                cg = go.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
