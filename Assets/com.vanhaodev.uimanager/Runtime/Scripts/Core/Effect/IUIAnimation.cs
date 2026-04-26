using System;
using DG.Tweening;
using UnityEngine;

namespace vanhaodev.uimanager.effect
{
    public interface IUIAnimation
    {
        void PlayShow(GameObject target, Action onComplete);
        void PlayClose(GameObject target, Action onComplete);
    }
    
    //default fade animation
    public class FadeAnimation : IUIAnimation
    {
        private float _duration;

        public FadeAnimation(float duration = 0.25f)
        {
            _duration = duration;
        }

        public void PlayShow(GameObject target, Action onComplete)
        {
            var cg = GetOrAddCanvasGroup(target);

            cg.alpha = 0;
            cg.DOKill();

            cg.DOFade(1, _duration)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void PlayClose(GameObject target, Action onComplete)
        {
            var cg = GetOrAddCanvasGroup(target);

            cg.DOKill();

            cg.DOFade(0, _duration)
                .OnComplete(() => onComplete?.Invoke());
        }

        private CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            if (!go.TryGetComponent(out CanvasGroup cg))
                cg = go.AddComponent<CanvasGroup>();

            return cg;
        }
    }
    
    //default slide animation
    public class SlideAnimation : IUIAnimation
    {
        private float _duration;

        public SlideAnimation(float duration = 0.3f)
        {
            _duration = duration;
        }

        public void PlayShow(GameObject target, Action onComplete)
        {
            var rect = target.transform as RectTransform;

            float height = rect.rect.height;
            float offset = height + 50f;

            rect.anchoredPosition = new Vector2(0, -offset);

            rect.DOAnchorPosY(0, _duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void PlayClose(GameObject target, Action onComplete)
        {
            var rect = target.transform as RectTransform;

            float height = rect.rect.height;
            float offset = height + 50f;

            rect.DOAnchorPosY(-offset, _duration)
                .SetEase(Ease.InCubic)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}