using System;
using DG.Tweening;
using UnityEngine;

namespace vanhaodev.uimanager.effect.templates
{
    /// <summary>
    /// Default toast slide-up from bottom animation.
    /// </summary>
    public class TempToastSlideAnimation : ToastAnimationBase
    {
        private readonly float _duration;

        public TempToastSlideAnimation(float duration = 0.3f)
        {
            _duration = duration;
        }

        public override void PlayShow(GameObject target, Action onComplete)
        {
            base.PlayShow(target, onComplete);
            var rect = target.transform as RectTransform;
            if (rect == null) { onComplete?.Invoke(); return; }

            // Layout has set rest position; slide in from outside the anchored edge.
            // pivot.y >= 0.5 → top-anchored → slide down from above; else slide up from below.
            float targetY = rect.anchoredPosition.y;
            float height = rect.rect.height;
            float offset = height + 50f;
            float startY = rect.pivot.y >= 0.5f ? targetY + offset : targetY - offset;

            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, startY);

            rect.DOKill();
            rect.DOAnchorPosY(targetY, _duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }

        public override void PlayClose(GameObject target, Action onComplete)
        {
            base.PlayClose(target, onComplete);
            var rect = target.transform as RectTransform;
            if (rect == null) { onComplete?.Invoke(); return; }

            var cg = target.GetComponent<CanvasGroup>();
            float fromY = rect.anchoredPosition.y;
            float dy = rect.pivot.y >= 0.5f ? 30f : -30f; // drift outward
            rect.DOKill();

            var seq = DOTween.Sequence();
            if (cg != null) seq.Join(cg.DOFade(0f, _duration));
            seq.Join(rect.DOAnchorPosY(fromY + dy, _duration).SetEase(Ease.InCubic));
            seq.OnComplete(() => onComplete?.Invoke());
        }
    }
}
