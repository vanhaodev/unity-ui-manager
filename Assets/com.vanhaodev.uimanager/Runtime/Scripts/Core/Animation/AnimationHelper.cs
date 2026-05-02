using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace vanhaodev.uimanager.effect
{
    public static class AnimationHelper
    {
        private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        private static readonly Dictionary<object, CancellationTokenSource> _ctsMap = new();

        public static CancellationToken ResetToken(object key)
        {
            if (_ctsMap.TryGetValue(key, out var old))
            {
                old.Cancel();
                old.Dispose();
            }

            var cts = new CancellationTokenSource();
            _ctsMap[key] = cts;
            return cts.Token;
        }

        public static void CancelAndRemove(object key)
        {
            if (_ctsMap.TryGetValue(key, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                _ctsMap.Remove(key);
            }
        }

        // ── Animators ────────────────────────────────────────────────

        public static async Awaitable AnchoredPos(
            RectTransform rect,
            Vector2 target,
            float duration,
            CancellationToken ct = default)
        {
            Vector2 start = rect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;
                elapsed += Time.deltaTime;
                rect.anchoredPosition =
                    Vector2.LerpUnclamped(start, target, EaseOutCubic(Mathf.Clamp01(elapsed / duration)));
                await Awaitable.NextFrameAsync(ct);
            }

            rect.anchoredPosition = target;
        }

        public static async Awaitable Alpha(
            CanvasGroup cg,
            float target,
            float duration,
            CancellationToken ct = default)
        {
            float start = cg.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.LerpUnclamped(start, target, EaseOutCubic(Mathf.Clamp01(elapsed / duration)));
                await Awaitable.NextFrameAsync(ct);
            }

            cg.alpha = target;
        }
        
        public static async Awaitable ImageAlpha(
            Image image,
            float target,
            float duration,
            CancellationToken ct = default)
        {
            float start = image.color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;
                elapsed += Time.deltaTime;
                var c = image.color;
                c.a = Mathf.LerpUnclamped(start, target, EaseOutCubic(Mathf.Clamp01(elapsed / duration)));
                image.color = c;
                await Awaitable.NextFrameAsync(ct);
            }

            var final = image.color;
            final.a = target;
            image.color = final;
        }

        public static async Awaitable Scale(
            Transform transform,
            Vector3 target,
            float duration,
            CancellationToken ct = default)
        {
            Vector3 start = transform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;
                elapsed += Time.deltaTime;
                transform.localScale =
                    Vector3.LerpUnclamped(start, target, EaseOutCubic(Mathf.Clamp01(elapsed / duration)));
                await Awaitable.NextFrameAsync(ct);
            }

            transform.localScale = target;
        }
    }

    public static class AwaitableExtensions
    {
        public static async void Forget(this Awaitable awaitable)
        {
            try
            {
                await awaitable;
            }
            catch (System.OperationCanceledException)
            {
            }
        }
    }
}