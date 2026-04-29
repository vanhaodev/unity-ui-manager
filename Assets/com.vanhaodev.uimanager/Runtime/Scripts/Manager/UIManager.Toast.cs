using System;
using System.Collections.Generic;
using com.vanhaodev.objectpool;
using DG.Tweening;
using UnityEngine;

namespace vanhaodev.uimanager
{
    public partial class UIManager
    {
        [SerializeField] private Transform _toastLayer;
        // One pool per toast type for reuse
        private readonly Dictionary<Type, ObjectPool<BaseToast>> _toastPools = new();

        // Currently visible toasts in display order (0 = oldest, last = newest)
        private readonly List<BaseToast> _activeToasts = new();

        // Queue of pending requests when concurrent limit reached
        private readonly Queue<PendingToast> _toastQueue = new();

        // Lookup by id for HideToast
        private readonly Dictionary<string, BaseToast> _toastById = new();

        private int _toastIdCounter;

        public IReadOnlyList<BaseToast> ActiveToasts => _activeToasts;
        public event Action<BaseToast> OnToastShown;
        public event Action<BaseToast> OnToastHidden;

        private class PendingToast
        {
            public Type Type;
            public string Id;
            public ToastPosition Position;
            public Action<BaseToast> Setup;
        }

        /// <summary>
        /// Show a toast at the given position. Returns its id (use with HideToast to dismiss early).
        /// If concurrent count exceeds limit, oldest is auto-dismissed and new one takes its place.
        /// </summary>
        public string ShowToast<T>(ToastPosition position = ToastPosition.Bottom, Action<T> onSetup = null) where T : BaseToast
        {
            var id = GenerateToastId();
            var type = typeof(T);

            Action<BaseToast> setupCast = onSetup != null ? (t => onSetup((T)t)) : null;

            ShowToastInternal(type, id, position, setupCast);
            return id;
        }

        /// <summary>
        /// Hide a toast by id immediately (with close animation). Safe to call with stale ids.
        /// </summary>
        public void HideToast(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!_toastById.TryGetValue(id, out var toast)) return;

            HideToastInternal(toast);
        }

        public void HideAllToasts()
        {
            _toastQueue.Clear();
            var copy = _activeToasts.ToArray();
            foreach (var t in copy)
                HideToastInternal(t);
        }

        private void ShowToastInternal(Type type, string id, ToastPosition position, Action<BaseToast> setup)
        {
            int max = _library != null ? _library.MaxConcurrentToasts : 3;
            if (max < 1) max = 1;

            // If at capacity, queue and wait for an active toast to close (auto-close or manual)
            if (_activeToasts.Count >= max)
            {
                _toastQueue.Enqueue(new PendingToast { Type = type, Id = id, Position = position, Setup = setup });
                return;
            }

            var toast = AcquireToast(type);
            if (toast == null) return;

            toast.ToastId = id;
            toast.Position = position;
            ApplyAnchorForPosition(toast.transform as RectTransform, position);
            _toastById[id] = toast;
            _activeToasts.Add(toast);

            setup?.Invoke(toast);
            // Force rebuild so RectTransform.rect.height reflects the freshly-set message
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(toast.transform as RectTransform);
            LayoutToasts(newcomer: toast);

            toast.Show(() => OnToastShown?.Invoke(toast));
        }

        private void HideToastInternal(BaseToast toast)
        {
            if (toast == null) return;
            if (!_activeToasts.Contains(toast)) return;

            var id = toast.ToastId;
            _activeToasts.Remove(toast);
            if (!string.IsNullOrEmpty(id)) _toastById.Remove(id);

            toast.Close(() =>
            {
                ReleaseToast(toast);
                OnToastHidden?.Invoke(toast);
                LayoutToasts();
                ProcessQueue();
            });

            // Re-layout immediately so visible toasts shift while close anim plays
            LayoutToasts();
        }

        private void ProcessQueue()
        {
            int max = _library != null ? _library.MaxConcurrentToasts : 3;
            while (_toastQueue.Count > 0 && _activeToasts.Count < max)
            {
                var p = _toastQueue.Dequeue();
                ShowToastInternal(p.Type, p.Id, p.Position, p.Setup);
            }
        }

        /// <summary>
        /// Stack toasts per position group. Newest (latest added) is closest to its anchored edge,
        /// older toasts are pushed further away. Newcomer snaps to its slot so the show animation
        /// can slide in from outside the screen edge; existing toasts tween smoothly.
        /// </summary>
        private void LayoutToasts(BaseToast newcomer = null)
        {
            float spacing = _library != null ? _library.ToastSpacing : 12f;
            Vector2 padding = _library != null ? _library.ToastPadding : new Vector2(24f, 48f);

            // Group active toasts by position (preserve insertion order)
            var byPos = new Dictionary<ToastPosition, List<BaseToast>>();
            foreach (var t in _activeToasts)
            {
                if (t == null) continue;
                if (!byPos.TryGetValue(t.Position, out var list))
                {
                    list = new List<BaseToast>();
                    byPos[t.Position] = list;
                }
                list.Add(t);
            }

            foreach (var kv in byPos)
                LayoutGroup(kv.Key, kv.Value, padding, spacing, newcomer);
        }

        /// <summary>
        /// Lay out a single position group. Stack direction depends on position:
        /// Bottom*: newest at bottom (y starts at +padding.y, increases).
        /// Top*: newest at top (y starts at -padding.y, decreases).
        /// X is set per-anchor (center=0, left=+padding.x, right=-padding.x relative to pivot).
        /// </summary>
        private void LayoutGroup(ToastPosition pos, List<BaseToast> list, Vector2 padding, float spacing, BaseToast newcomer)
        {
            bool isTop = pos == ToastPosition.Top || pos == ToastPosition.TopLeft || pos == ToastPosition.TopRight;
            bool isCenter = pos == ToastPosition.Center;
            float dir = isTop ? -1f : 1f;
            float startY = isCenter ? 0f : dir * padding.y;
            float x = GetAnchoredX(pos, padding.x);

            float y = startY;
            // Iterate newest -> oldest so newest sits closest to the edge
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var t = list[i];
                var rect = t.transform as RectTransform;
                if (rect == null) continue;

                float h = rect.rect.height;
                var target = new Vector2(x, y);

                if (t == newcomer)
                {
                    rect.anchoredPosition = target;
                }
                else
                {
                    rect.DOKill();
                    rect.DOAnchorPos(target, 0.25f).SetEase(Ease.OutCubic);
                }

                y += dir * (h + spacing);
            }
        }

        private static float GetAnchoredX(ToastPosition pos, float paddingX)
        {
            switch (pos)
            {
                case ToastPosition.TopLeft:
                case ToastPosition.BottomLeft:
                    return paddingX;
                case ToastPosition.TopRight:
                case ToastPosition.BottomRight:
                    return -paddingX;
                default:
                    return 0f;
            }
        }

        private static void ApplyAnchorForPosition(RectTransform rect, ToastPosition pos)
        {
            if (rect == null) return;

            float ax, ay, px, py;
            switch (pos)
            {
                case ToastPosition.Top:        ax = 0.5f; ay = 1f; px = 0.5f; py = 1f; break;
                case ToastPosition.TopLeft:    ax = 0f;   ay = 1f; px = 0f;   py = 1f; break;
                case ToastPosition.TopRight:   ax = 1f;   ay = 1f; px = 1f;   py = 1f; break;
                case ToastPosition.BottomLeft: ax = 0f;   ay = 0f; px = 0f;   py = 0f; break;
                case ToastPosition.BottomRight:ax = 1f;   ay = 0f; px = 1f;   py = 0f; break;
                case ToastPosition.Center:     ax = 0.5f; ay = 0.5f; px = 0.5f; py = 0.5f; break;
                default: /* Bottom */          ax = 0.5f; ay = 0f; px = 0.5f; py = 0f; break;
            }

            rect.anchorMin = new Vector2(ax, ay);
            rect.anchorMax = new Vector2(ax, ay);
            rect.pivot = new Vector2(px, py);
        }

        private BaseToast AcquireToast(Type type)
        {
            if (!_toastPools.TryGetValue(type, out var pool))
            {
                var prefab = GetToastPrefabByType(type);
                if (prefab == null)
                {
                    Debug.LogError($"[UIManager] Toast not found: {type.Name}");
                    return null;
                }

                pool = new ObjectPool<BaseToast>(
                    factory: () =>
                    {
                        var inst = Instantiate(prefab, _toastLayer);
                        inst.gameObject.SetActive(false);
                        inst.Manager = this;
                        return inst;
                    },
                    initialSize: 0,
                    onGet: t =>
                    {
                        if (t == null) return;
                        ResetToastTransform(t);
                        t.gameObject.SetActive(true);
                    },
                    onRelease: t =>
                    {
                        if (t == null) return;
                        ResetToastTransform(t);
                        t.gameObject.SetActive(false);
                        t.transform.SetParent(_toastLayer, false);
                    },
                    onDestroy: t =>
                    {
                        if (t != null) Destroy(t.gameObject);
                    });

                _toastPools[type] = pool;
            }

            return pool.Get();
        }

        private void ReleaseToast(BaseToast toast)
        {
            if (toast == null) return;
            var type = toast.GetType();
            if (_toastPools.TryGetValue(type, out var pool))
                pool.Release(toast);
            else if (toast != null)
                Destroy(toast.gameObject);
        }

        private BaseToast GetToastPrefabByType(Type type)
        {
            if (_library == null) return null;
            // Use reflection through generic helper since UILibrary uses generic getter
            var method = typeof(UILibrary).GetMethod(nameof(UILibrary.GetToastPrefab));
            var generic = method?.MakeGenericMethod(type);
            return generic?.Invoke(_library, null) as BaseToast;
        }

        private string GenerateToastId() => $"toast_{++_toastIdCounter}";

        /// <summary>
        /// Reset transform and CanvasGroup so a pooled toast doesn't carry over
        /// swipe offset / fade alpha / leftover tweens from its previous lifecycle.
        /// </summary>
        private static void ResetToastTransform(BaseToast t)
        {
            var rect = t.transform as RectTransform;
            if (rect != null)
            {
                rect.DOKill();
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }
            var cg = t.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.DOKill();
                cg.alpha = 1f;
            }
        }


        private void ClearToastCache()
        {
            _toastQueue.Clear();
            _toastById.Clear();
            _activeToasts.Clear();
            foreach (var pool in _toastPools.Values)
                pool.Clear(includeActive: true);
            _toastPools.Clear();
        }
    }
}
