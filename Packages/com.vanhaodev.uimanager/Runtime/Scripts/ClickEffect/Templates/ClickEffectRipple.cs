using System;
using UnityEngine;
using UnityEngine.UI;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Default click effect: a sprite that scales up and fades out — a simple "ripple" at the touch
    /// point. Tweak the look on the prefab, or replace it entirely by subclassing
    /// <see cref="BaseClickEffect"/> (e.g. for particles, VFX Graph, or to add a sound).
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Image))]
    public class ClickEffectRipple : BaseClickEffect
    {
        [SerializeField] private float _duration = 0.4f;
        [SerializeField] private float _startScale = 0.3f;
        [SerializeField] private float _endScale = 1f;
        [SerializeField] private bool _fadeOut = true;

        private CanvasGroup _canvasGroup;

        protected override void Awake()
        {
            base.Awake();
            _canvasGroup = GetComponent<CanvasGroup>();
            // The effect is purely visual — never let it eat input from the UI underneath.
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        public override void ResetState()
        {
            RectTransform.localScale = Vector3.one * _startScale;
            _canvasGroup.alpha = 1f;
        }

        public override void Play(Action onComplete) => PlayAsync(onComplete).Forget();

        private async Awaitable PlayAsync(Action onComplete)
        {
            var ct = AnimationHelper.ResetToken(this);
            try
            {
                // Scale and fade run together over the same duration.
                var scale = AnimationHelper.Scale(RectTransform, Vector3.one * _endScale, _duration, ct);
                if (_fadeOut)
                    await AnimationHelper.Alpha(_canvasGroup, 0f, _duration, ct);
                await scale;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                onComplete?.Invoke();
            }
        }
    }
}
