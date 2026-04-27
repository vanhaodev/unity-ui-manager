using DG.Tweening;
using UnityEngine;
using vanhaodev.uimanager.events;

namespace vanhaodev.uimanager.effect
{
    [RequireComponent(typeof(UIButton))]
    public class ButtonScaleAnimation : ButtonAnimationBase
    {
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _pressedScale = 0.9f;

        [SerializeField] private float _duration = 0.1f;

        private RectTransform _rect;
        private void Awake()
        {
            _rect = transform as RectTransform;
        }

        public override void OnPointerEnter(GameObject target)
        {
            base.OnPointerEnter(target);
            _rect.DOKill();
            _rect.DOScale(_hoverScale, _duration);
        }

        public override void OnPointerExit(GameObject target)
        {
            base.OnPointerExit(target);
            _rect.DOKill();
            _rect.DOScale(1f, _duration);
        }

        public override void OnPointerDown(GameObject target)
        {
            base.OnPointerDown(target);
            _rect.DOKill();
            _rect.DOScale(_pressedScale, _duration * 0.8f);
        }

        public override void OnPointerUp(GameObject target)
        {
            base.OnPointerUp(target);
            _rect.DOKill();
            _rect.DOScale(_hoverScale, _duration);
        }

        public override void OnClick(GameObject target)
        {
            base.OnClick(target);
            _rect.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        }
    }
}