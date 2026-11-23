using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace vanhaodev.uimanager
{
    public partial class UIManager
    {
        [SerializeField] private Transform _popupLayer;
        private readonly Dictionary<Type, BasePopup> _cachedPopups = new();
        private readonly List<BasePopup> _activePopups = new();

        public IReadOnlyList<BasePopup> ActivePopups => _activePopups;
        public bool HasActivePopup => _activePopups.Count > 0;
        public BasePopup TopPopup => _activePopups.Count > 0 ? _activePopups[^1] : null;
        public event Action<BasePopup> OnPopupOpened;
        public event Action<BasePopup> OnPopupClosed;

        /// <summary>
        /// Open a popup on top of the stack. By default the newest popup sits on top (standard modal
        /// stacking). Set <paramref name="keepSameTypeOnTop"/> for the special case where an already-open
        /// popup of the SAME type should stay visually on top — the new one slips in just below it, so a
        /// repeat-triggered dialog doesn't cover the one the player is already reading.
        /// </summary>
        public T ShowPopup<T>(Action<T> onSetup = null, Action onComplete = null, bool keepSameTypeOnTop = false) where T : BasePopup
        {
            var popup = GetOrCreatePopup<T>();
            if (popup == null) return null;

            onSetup?.Invoke(popup);
            ShowPopupInternal(popup, onComplete, keepSameTypeOnTop);
            return popup;
        }

        private void ShowPopupInternal(BasePopup popup, Action onComplete, bool keepSameTypeOnTop)
        {
            _activePopups.Add(popup);

            if (keepSameTypeOnTop && TryGetNearestSameType(popup, out var sameType))
            {
                // Slip the new popup right below the existing same-type popup so the older one
                // stays visually on top.
                popup.transform.SetSiblingIndex(sameType.transform.GetSiblingIndex());
            }
            else
            {
                // Default: newest popup goes on top.
                popup.transform.SetAsLastSibling();
            }

            popup.Show(() =>
            {
                onComplete?.Invoke();
                OnPopupOpened?.Invoke(popup);
            });
        }

        // Find the topmost other active popup of the same type as <paramref name="popup"/>.
        private bool TryGetNearestSameType(BasePopup popup, out BasePopup sameType)
        {
            var type = popup.GetType();
            for (int i = _activePopups.Count - 1; i >= 0; i--)
            {
                var current = _activePopups[i];
                if (current == popup) continue;            // skip the one we just added
                if (current.GetType() == type)
                {
                    sameType = current;
                    return true;
                }
            }

            sameType = null;
            return false;
        }

        public void ClosePopup<T>(Action onComplete = null) where T : BasePopup
        {
            var popup = _activePopups.FirstOrDefault(p => p is T);
            if (popup != null)
                ClosePopup(popup, onComplete);
            else
                onComplete?.Invoke();
        }

        public void ClosePopup(BasePopup popup, Action onComplete = null)
        {
            if (!_activePopups.Contains(popup))
            {
                onComplete?.Invoke();
                return;
            }

            popup.Close(() =>
            {
                _activePopups.Remove(popup);

                var type = popup.GetType();
                bool hasSameTypeActive = _activePopups.Any(p => p.GetType() == type);

                if (hasSameTypeActive)
                {
                    // Another same type still active → destroy this one
                    if (_cachedPopups.TryGetValue(type, out var cached) && cached == popup)
                        _cachedPopups.Remove(type);
                    Destroy(popup.gameObject);
                }
                else
                {
                    // No same type active → keep as cached
                    _cachedPopups[type] = popup;
                }

                onComplete?.Invoke();
                OnPopupClosed?.Invoke(popup);
            });
        }

        public void CloseTopPopup(Action onComplete = null)
        {
            if (_activePopups.Count > 0)
                ClosePopup(_activePopups[^1], onComplete);
            else
                onComplete?.Invoke();
        }

        public void CloseAllPopups(Action onComplete = null)
        {
            if (_activePopups.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int remaining = _activePopups.Count;
            var popupsToClose = _activePopups.ToList();

            foreach (var popup in popupsToClose)
            {
                popup.Close(() =>
                {
                    _activePopups.Remove(popup);
                    OnPopupClosed?.Invoke(popup);
                    remaining--;
                    if (remaining == 0)
                        onComplete?.Invoke();
                });
            }
        }

        public void CloseAllPopups<T>(Action onComplete = null) where T : BasePopup
        {
            var popupsToClose = _activePopups.Where(p => p is T).ToList();
            if (popupsToClose.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int remaining = popupsToClose.Count;
            foreach (var popup in popupsToClose)
            {
                popup.Close(() =>
                {
                    _activePopups.Remove(popup);
                    OnPopupClosed?.Invoke(popup);
                    remaining--;
                    if (remaining == 0)
                        onComplete?.Invoke();
                });
            }
        }

        public bool IsPopupActive<T>() where T : BasePopup
        {
            return _activePopups.Any(p => p is T);
        }

        public int GetActivePopupCount<T>() where T : BasePopup
        {
            return _activePopups.Count(p => p is T);
        }

        private T GetOrCreatePopup<T>() where T : BasePopup
        {
            var type = typeof(T);

            // Reuse cached if not active
            if (_cachedPopups.TryGetValue(type, out var cached) && !_activePopups.Contains(cached))
                return cached as T;

            var prefab = _library?.GetPopupPrefab<T>();
            if (prefab == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] Popup not found: {type.Name}");
                return null;
            }

            var instance = Instantiate(prefab, _popupLayer);
            instance.gameObject.SetActive(false);
            instance.Manager = this;

            // Cache if no existing cache for this type
            if (!_cachedPopups.ContainsKey(type))
                _cachedPopups[type] = instance;

            return instance;
        }

        private void ClearPopupCache()
        {
            foreach (var popup in _cachedPopups.Values)
                if (popup != null) Destroy(popup.gameObject);

            _cachedPopups.Clear();
            _activePopups.Clear();
        }
    }
}
