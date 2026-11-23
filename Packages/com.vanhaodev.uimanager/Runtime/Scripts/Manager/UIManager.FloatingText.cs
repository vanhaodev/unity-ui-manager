using System;
using System.Collections.Generic;
using com.vanhaodev.objectpool;
using UnityEngine;
using UnityEngine.UI;
using vanhaodev.uimanager.effect;
using vanhaodev.uimanager.template;

namespace vanhaodev.uimanager
{
    public partial class UIManager
    {
        [SerializeField] private Transform _floatingTextLayer;

        // One pool per floating-text type for reuse.
        private readonly Dictionary<Type, ObjectPool<FloatingText>> _floatingTextPools = new();

        // Show requests are queued and released one every UILibrary.FloatingShowInterval seconds. Each
        // text starts at the same spawn point and floats up, so a small stagger keeps them from
        // overlapping. One source PER anchor instance id (or grid cell for raw positions) so floats
        // from different sources run in parallel — only same-source spam is throttled.
        private float FloatingShowInterval => _library != null ? _library.FloatingShowInterval : 0.2f;
        private int FloatingMaxPerSource => _library != null ? _library.FloatingMaxPerSource : 20;

        private class FloatingSource
        {
            public readonly Queue<Action> Pending = new();   // not-yet-shown requests
            public readonly List<FloatingText> Active = new(); // currently playing, oldest first
            public bool Processing;
        }
        private readonly Dictionary<int, FloatingSource> _floatingSources = new();

        /// <summary>
        /// Show the default floating text at a UI anchor (e.g. a button). The text rises and fades out.
        /// </summary>
        public void ShowFloatingText(string message, Transform anchor)
            => ShowFloatingText<FloatingTextDefault>(t => t.SetText(message), anchor);

        /// <summary>Show the default floating text at a screen-space position.</summary>
        public void ShowFloatingText(string message, Vector2 screenPosition)
            => ShowFloatingText<FloatingTextDefault>(t => t.SetText(message), screenPosition);

        /// <summary>
        /// Show a custom floating text type at a UI anchor. Pass data via <paramref name="onSetup"/>.
        /// The anchor is assumed to be a UI/canvas element; for world-space objects use the
        /// screen-position overload with <c>Camera.main.WorldToScreenPoint(obj.position)</c>.
        /// </summary>
        public void ShowFloatingText<T>(Action<T> onSetup, Transform anchor) where T : FloatingText
        {
            if (anchor == null) return;
            // Per-anchor queue: spam from the same anchor staggers; different anchors run in parallel.
            ShowFloatingTextInternal(anchor.GetInstanceID(), onSetup, WorldToScreenPoint(anchor.position));
        }

        /// <summary>Show a custom floating text type at a screen-space position.</summary>
        public void ShowFloatingText<T>(Action<T> onSetup, Vector2 screenPosition) where T : FloatingText
            => ShowFloatingTextInternal(PositionKey(screenPosition), onSetup, screenPosition);

        // Position is captured now (at the anchor); the actual spawn is deferred through the queue.
        private void ShowFloatingTextInternal<T>(int sourceKey, Action<T> onSetup, Vector2 screenPosition) where T : FloatingText
            => EnqueueFloating(sourceKey, () => SpawnFloatingText(sourceKey, onSetup, screenPosition));

        // Group nearby raw positions into one source so the same spot throttles together.
        private static int PositionKey(Vector2 screenPosition)
        {
            const float cell = 48f;
            int x = Mathf.RoundToInt(screenPosition.x / cell);
            int y = Mathf.RoundToInt(screenPosition.y / cell);
            return unchecked(x * 397) ^ y;
        }

        private void SpawnFloatingText<T>(int sourceKey, Action<T> onSetup, Vector2 screenPosition) where T : FloatingText
        {
            // No toast-style queue erasure here — the generic type is preserved, so resolve the
            // prefab via the generic getter instead of reflecting on Type.
            var prefab = _library != null ? _library.GetFloatingTextPrefab<T>() : null;
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] FloatingText not found: {typeof(T).Name}");
                return;
            }

            var floatingText = (T)AcquireFloatingText(typeof(T), prefab);
            if (floatingText == null) return;

            var rect = floatingText.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = ScreenToLayerLocal(screenPosition);
            }
            floatingText.transform.SetAsLastSibling();   // newest renders on top of older ones

            onSetup?.Invoke(floatingText);

            // Now that the content (and size) is set, keep the text inside the screen edges.
            if (rect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                ClampIntoLayer(rect);
            }

            var source = GetOrCreateSource(sourceKey);
            source.Active.Add(floatingText);
            EnforceActiveCap(sourceKey, source);

            floatingText.Play(() => OnFloatingFinished(sourceKey, floatingText));
        }

        // Cap active floats per source: drop the oldest (quick fade) so spam can't pile up and lag.
        private void EnforceActiveCap(int key, FloatingSource source)
        {
            int max = Mathf.Max(1, FloatingMaxPerSource);
            while (source.Active.Count > max)
            {
                var oldest = source.Active[0];
                source.Active.RemoveAt(0);
                if (oldest != null)
                    oldest.FadeOutNow(() => { ReleaseFloatingText(oldest); TryRemoveSource(key); });
                else
                    TryRemoveSource(key);
            }
        }

        private void OnFloatingFinished(int key, FloatingText floatingText)
        {
            ReleaseFloatingText(floatingText);
            if (_floatingSources.TryGetValue(key, out var source))
            {
                source.Active.Remove(floatingText);
                TryRemoveSource(key);
            }
        }

        // ── Show queue (per-source stagger so floats don't overlap) ───────

        private FloatingSource GetOrCreateSource(int key)
        {
            if (!_floatingSources.TryGetValue(key, out var source))
                _floatingSources[key] = source = new FloatingSource();
            return source;
        }

        private void EnqueueFloating(int key, Action show)
        {
            var source = GetOrCreateSource(key);
            source.Pending.Enqueue(show);
            if (!source.Processing) ProcessFloatingQueue(key, source).Forget();
        }

        private async Awaitable ProcessFloatingQueue(int key, FloatingSource source)
        {
            source.Processing = true;
            try
            {
                while (source.Pending.Count > 0)
                {
                    source.Pending.Dequeue()?.Invoke();
                    await Awaitable.WaitForSecondsAsync(FloatingShowInterval, destroyCancellationToken);
                }
            }
            finally
            {
                source.Processing = false;
                TryRemoveSource(key);
            }
        }

        // Drop the source entry once it has nothing pending and nothing active.
        private void TryRemoveSource(int key)
        {
            if (_floatingSources.TryGetValue(key, out var source)
                && !source.Processing && source.Pending.Count == 0 && source.Active.Count == 0)
                _floatingSources.Remove(key);
        }

        // Clamp the spawn position so the whole text stays inside the layer (screen) minus padding.
        // Assumes the layer is a full-screen RectTransform with a centered pivot.
        private void ClampIntoLayer(RectTransform rect)
        {
            if (_floatingTextLayer is not RectTransform layer) return;

            Vector2 pad = _library != null ? _library.FloatingScreenPadding : new Vector2(16f, 16f);
            Vector2 half = layer.rect.size * 0.5f;
            Vector2 textHalf = rect.rect.size * 0.5f;

            var p = rect.anchoredPosition;
            p.x = Mathf.Clamp(p.x, -half.x + textHalf.x + pad.x, half.x - textHalf.x - pad.x);
            p.y = Mathf.Clamp(p.y, -half.y + textHalf.y + pad.y, half.y - textHalf.y - pad.y);
            rect.anchoredPosition = p;
        }

        // ── Position conversion ──────────────────────────────────────────

        private Vector2 WorldToScreenPoint(Vector3 worldPosition)
        {
            return RectTransformUtility.WorldToScreenPoint(GetLayerCamera(), worldPosition);
        }

        private Vector2 ScreenToLayerLocal(Vector2 screenPosition)
        {
            var layerRect = _floatingTextLayer as RectTransform;
            if (layerRect == null) return screenPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                layerRect, screenPosition, GetLayerCamera(), out var local);
            return local;
        }

        // Overlay canvases use a null camera; camera/world canvases need their render camera.
        private Camera GetLayerCamera()
        {
            var canvas = _floatingTextLayer != null
                ? _floatingTextLayer.GetComponentInParent<Canvas>()
                : null;
            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;
            return canvas.worldCamera;
        }

        // ── Pooling (mirrors the toast pool) ─────────────────────────────

        private FloatingText AcquireFloatingText(Type type, FloatingText prefab)
        {
            if (!_floatingTextPools.TryGetValue(type, out var pool))
            {
                pool = new ObjectPool<FloatingText>(
                    factory: () =>
                    {
                        var inst = Instantiate(prefab, _floatingTextLayer);
                        inst.gameObject.SetActive(false);
                        inst.Manager = this;
                        return inst;
                    },
                    initialSize: 0,
                    onGet: ft =>
                    {
                        if (ft == null) return;
                        ft.gameObject.SetActive(true);
                    },
                    onRelease: ft =>
                    {
                        if (ft == null) return;
                        AnimationHelper.ResetToken(ft);
                        ft.gameObject.SetActive(false);
                        ft.transform.SetParent(_floatingTextLayer, false);
                    },
                    onDestroy: ft =>
                    {
                        if (ft != null) Destroy(ft.gameObject);
                    });

                _floatingTextPools[type] = pool;
            }

            return pool.Get();
        }

        private void ReleaseFloatingText(FloatingText floatingText)
        {
            if (floatingText == null) return;
            var type = floatingText.GetType();
            if (_floatingTextPools.TryGetValue(type, out var pool))
                pool.Release(floatingText);
            else
                Destroy(floatingText.gameObject);
        }

        private void ClearFloatingTextCache()
        {
            foreach (var pool in _floatingTextPools.Values)
                pool.Clear(includeActive: true);
            _floatingTextPools.Clear();
            _floatingSources.Clear();
        }
    }
}
