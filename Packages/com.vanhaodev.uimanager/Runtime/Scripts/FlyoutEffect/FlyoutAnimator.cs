using System.Threading;
using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Animation helpers for flyout effect. Replace Instance for custom animations.
    /// </summary>
    public class FlyoutAnimator
    {
        /// <summary>
        /// Layer 1: Burst — scatter icons from source with pop effect.
        /// </summary>
        public virtual async Awaitable BurstAsync(
            FlyoutIcon icon,
            Vector2 sourcePos,
            FlyoutConfig config,
            CancellationToken ct)
        {
            var rect = icon.RectTransform;
            rect.anchoredPosition = sourcePos;
            rect.localScale = Vector3.zero;

            var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var distance = config.BurstRadius * (1f - config.BurstRandomness * Random.value);
            var targetPos = sourcePos + new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );

            var elapsed = 0f;
            var duration = config.BurstDuration;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                var t = elapsed / duration;
                var eased = EaseOutBack(t);

                rect.anchoredPosition = Vector2.LerpUnclamped(sourcePos, targetPos, eased);

                var scale = t < 0.5f
                    ? Mathf.Lerp(0f, 1.2f, t * 2f)
                    : Mathf.Lerp(1.2f, 1f, (t - 0.5f) * 2f);
                rect.localScale = Vector3.one * scale;

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(ct);
            }

            rect.anchoredPosition = targetPos;
            rect.localScale = Vector3.one;
        }

        /// <summary>
        /// Layer 2: Flight — curve to target like a flock.
        /// </summary>
        public virtual async Awaitable FlightAsync(
            FlyoutIcon icon,
            Vector2 targetPos,
            FlyoutConfig config,
            CancellationToken ct)
        {
            var rect = icon.RectTransform;
            var startPos = rect.anchoredPosition;

            var midPoint = (startPos + targetPos) / 2f;
            var perpendicular = Vector2.Perpendicular((targetPos - startPos).normalized);
            var curveOffset = perpendicular * config.FlightCurveHeight *
                (0.5f + config.FlightRandomness * (Random.value - 0.5f));
            var controlPoint = midPoint + curveOffset;

            var elapsed = 0f;
            var duration = config.FlightDuration;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                var t = elapsed / duration;
                var eased = EaseInOutCubic(t);

                rect.anchoredPosition = QuadraticBezier(startPos, controlPoint, targetPos, eased);

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(ct);
            }

            rect.anchoredPosition = targetPos;
        }

        /// <summary>
        /// Layer 3: Impact — scale down + fade out.
        /// </summary>
        public virtual async Awaitable ImpactAsync(
            FlyoutIcon icon,
            FlyoutConfig config,
            CancellationToken ct)
        {
            var rect = icon.RectTransform;
            var canvasGroup = icon.CanvasGroup;

            var elapsed = 0f;
            var duration = config.ImpactDuration;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                var t = elapsed / duration;
                var eased = EaseOutCubic(t);

                rect.localScale = Vector3.one * (1f - eased * 0.5f);
                canvasGroup.alpha = 1f - eased;

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(ct);
            }

            rect.localScale = Vector3.one * 0.5f;
            canvasGroup.alpha = 0f;
        }

        // === Easing Functions ===

        protected static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        protected static float EaseInOutCubic(float t) =>
            t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

        protected static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        protected static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            var u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }
    }
}
