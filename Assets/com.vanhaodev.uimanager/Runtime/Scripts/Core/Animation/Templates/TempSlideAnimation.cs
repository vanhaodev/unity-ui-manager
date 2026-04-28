using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace vanhaodev.uimanager.effect.templates
{
    //default slide animation
    public class TempSlideAnimation : UIAnimationBase
    {
        private float _duration;
        public TempSlideAnimation(float duration = 0.3f)
        {
            _duration = duration;
        }

        public override void PlayShow(GameObject target, Action onComplete)
        {
            base.PlayShow(target, onComplete);
            var rect = target.transform as RectTransform;

            float height = rect.rect.height;
            float offset = height + 50f;

            rect.anchoredPosition = new Vector2(0, -offset);

            rect.DOAnchorPosY(0, _duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }

        public override void PlayClose(GameObject target, Action onComplete)
        {
            base.PlayClose(target, onComplete);
            var rect = target.transform as RectTransform;

            float height = rect.rect.height;
            float offset = height + 50f;

            rect.DOAnchorPosY(-offset, _duration)
                .SetEase(Ease.InCubic)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}