using System;
using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// The motion an intro gets when its prefab carries no <see cref="IIntroAnimation"/>: a curve pop in, a fade
    /// out. Plain Unity, so the library works bare. Unscaled time, like the rest of the intro.
    /// </summary>
    public sealed class DefaultIntroAnimation : IIntroAnimation
    {
        private const float AppearSeconds = 0.55f;
        private const float HideSeconds = 0.25f;

        // Up from nothing, past full size, a small settle back, then rest. The overshoot's peak is where the
        // logo lands.
        private const float PeakTime = 0.6f;

        private static readonly AnimationCurve PopCurve = new(
            new Keyframe(0f, 0f, 0f, 3f),
            new Keyframe(PeakTime, 1.12f, 0f, 0f),
            new Keyframe(0.82f, 0.96f, 0f, 0f),
            new Keyframe(1f, 1f, 0f, 0f));

        public void PlayAppear(IntroView view, Action onLanded, Action onComplete)
        {
            bool landed = false;
            TweenAsync(view, AppearSeconds, t =>
            {
                view.SetLogoScale(PopCurve.Evaluate(t));
                if (landed || t < PeakTime) return;

                landed = true;
                onLanded?.Invoke();
            }, onComplete);
        }

        // The bare default holds still.
        public void PlayIdle(IntroView view) { }

        public void StopIdle(IntroView view) { }

        public void PlayHide(IntroView view, Action onComplete)
        {
            TweenAsync(view, HideSeconds, t => view.SetAlpha(1f - t), onComplete);
        }

        // Steps 0→1 over the duration, ending exactly on 1 however the frames fall. Stops quietly if the view is
        // destroyed: the sequence waiting on it is cancelled by the same destroy.
        private static async void TweenAsync(IntroView view, float seconds, Action<float> step, Action onComplete)
        {
            var token = view.destroyCancellationToken;
            try
            {
                for (float elapsed = 0f; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
                {
                    step(elapsed / seconds);
                    await Awaitable.NextFrameAsync(token);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            step(1f);
            onComplete?.Invoke();
        }
    }
}
