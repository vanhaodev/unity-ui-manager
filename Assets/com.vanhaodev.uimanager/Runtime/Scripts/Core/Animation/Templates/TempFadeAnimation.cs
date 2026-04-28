using System;
using DG.Tweening;
using UnityEngine;

namespace vanhaodev.uimanager.effect.templates
{
    //default fade animation
    public class TempFadeAnimation : UIAnimationBase
    {
        private float _duration;

        public TempFadeAnimation(float duration = 0.25f)
        {
            _duration = duration;
        }

        public override void PlayShow(GameObject target, Action onComplete)
        {
            base.PlayShow(target, onComplete);
            var cg = GetOrAddCanvasGroup(target);

            cg.alpha = 0;
            cg.DOKill();

            cg.DOFade(1, _duration)
                .OnComplete(() => onComplete?.Invoke());
        }

        public override void PlayClose(GameObject target, Action onComplete)
        {
            base.PlayClose(target, onComplete);
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
}