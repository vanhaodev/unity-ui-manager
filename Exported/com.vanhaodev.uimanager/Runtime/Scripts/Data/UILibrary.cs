using System;
using System.Collections.Generic;
using UnityEngine;

namespace vanhaodev.uimanager
{
    [CreateAssetMenu(fileName = "UILibrary", menuName = "UI Manager/Library")]
    public class UILibrary : ScriptableObject
    {
        [SerializeField] private List<BaseScreen> _screens = new();
        [SerializeField] private List<BasePopup> _popups = new();
        [SerializeField] private List<BaseToast> _toasts = new();
        [SerializeField] private List<BaseLoadingBlock> _loadingBlocks = new();
        [SerializeField] private List<FloatingText> _floatingTexts = new();

        [Tooltip("Maximum number of toasts visible on screen at once. Exceeding will auto-dismiss the oldest.")]
        [SerializeField] private int _maxConcurrentToasts = 3;
        [Tooltip("Spacing between stacked toasts (px).")]
        [SerializeField] private float _toastSpacing = 12f;
        [Tooltip("Padding from screen edges. X = horizontal (left/right), Y = vertical (top/bottom).")]
        [SerializeField] private Vector2 _toastPadding = new Vector2(24f, 48f);

        [Tooltip("Delay (s) between floating texts shown from the same source, so they don't overlap.")]
        [SerializeField] private float _floatingShowInterval = 0.2f;
        [Tooltip("Max active floating texts per source. Beyond this, the oldest fades out immediately to avoid lag.")]
        [SerializeField] private int _floatingMaxPerSource = 20;
        [Tooltip("Keep floating texts this far (px) inside the screen edges. X = left/right, Y = top/bottom.")]
        [SerializeField] private Vector2 _floatingScreenPadding = new Vector2(16f, 16f);

        [Tooltip("Default flyout icon prefab (requires Image + CanvasGroup + FlyoutIcon).")]
        [SerializeField] private FlyoutIcon _flyoutIconPrefab;
        [Tooltip("Flyout effect animation and behavior settings.")]
        [SerializeField] private FlyoutConfig _flyoutConfig = new();

        [Tooltip("Effect played at each click/touch point. Leave the prefab null to disable.")]
        [SerializeField] private ClickEffectConfig _clickEffectConfig = new();

        private Dictionary<Type, BaseScreen> _screenCache;
        private Dictionary<Type, BasePopup> _popupCache;
        private Dictionary<Type, BaseToast> _toastCache;
        private Dictionary<Type, BaseLoadingBlock> _loadingBlockCache;
        private Dictionary<Type, FloatingText> _floatingTextCache;

        public int MaxConcurrentToasts => _maxConcurrentToasts;
        public float ToastSpacing => _toastSpacing;
        public Vector2 ToastPadding => _toastPadding;
        public float FloatingShowInterval => _floatingShowInterval;
        public int FloatingMaxPerSource => _floatingMaxPerSource;
        public Vector2 FloatingScreenPadding => _floatingScreenPadding;
        public FlyoutIcon FlyoutIconPrefab => _flyoutIconPrefab;
        public FlyoutConfig FlyoutConfig => _flyoutConfig;
        public ClickEffectConfig ClickEffectConfig => _clickEffectConfig;

        private void OnEnable()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            _screenCache = new Dictionary<Type, BaseScreen>();
            _popupCache = new Dictionary<Type, BasePopup>();
            _toastCache = new Dictionary<Type, BaseToast>();
            _loadingBlockCache = new Dictionary<Type, BaseLoadingBlock>();
            _floatingTextCache = new Dictionary<Type, FloatingText>();

            foreach (var screen in _screens)
                if (screen != null) _screenCache[screen.GetType()] = screen;

            foreach (var popup in _popups)
                if (popup != null) _popupCache[popup.GetType()] = popup;

            foreach (var toast in _toasts)
                if (toast != null) _toastCache[toast.GetType()] = toast;

            foreach (var loadingBlock in _loadingBlocks)
                if (loadingBlock != null) _loadingBlockCache[loadingBlock.GetType()] = loadingBlock;

            foreach (var floatingText in _floatingTexts)
                if (floatingText != null) _floatingTextCache[floatingText.GetType()] = floatingText;
        }

        public T GetScreenPrefab<T>() where T : BaseScreen
        {
            if (_screenCache == null) BuildCache();
            return _screenCache.TryGetValue(typeof(T), out var screen) ? screen as T : null;
        }

        public T GetPopupPrefab<T>() where T : BasePopup
        {
            if (_popupCache == null) BuildCache();
            return _popupCache.TryGetValue(typeof(T), out var popup) ? popup as T : null;
        }

        public T GetToastPrefab<T>() where T : BaseToast
        {
            if (_toastCache == null) BuildCache();
            return _toastCache.TryGetValue(typeof(T), out var toast) ? toast as T : null;
        }

        public T GetLoadingBlockPrefab<T>() where T : BaseLoadingBlock
        {
            if (_loadingBlockCache == null) BuildCache();
            return _loadingBlockCache.TryGetValue(typeof(T), out var block) ? block as T : null;
        }

        public T GetFloatingTextPrefab<T>() where T : FloatingText
        {
            if (_floatingTextCache == null) BuildCache();
            return _floatingTextCache.TryGetValue(typeof(T), out var floatingText) ? floatingText as T : null;
        }

#if UNITY_EDITOR
        // Editor-only snapshot of each prefab's asset GUID, kept in sync while the lists are healthy.
        // If Unity drops a reference (a list slot becomes "None"), the saved GUID lets the inspector's
        // Reload button put the prefab back. GUIDs survive renames/moves; paths would not.
        [SerializeField, HideInInspector] private List<string> _screenGuids = new();
        [SerializeField, HideInInspector] private List<string> _popupGuids = new();
        [SerializeField, HideInInspector] private List<string> _toastGuids = new();
        [SerializeField, HideInInspector] private List<string> _loadingBlockGuids = new();
        [SerializeField, HideInInspector] private List<string> _floatingTextGuids = new();
        [SerializeField, HideInInspector] private string _flyoutIconGuid;
        [SerializeField, HideInInspector] private string _clickEffectGuid;

        /// <summary>True if any prefab list has a missing (null) slot that could be restored.</summary>
        public bool HasMissingPrefabReferences()
            => HasNull(_screens) || HasNull(_popups) || HasNull(_toasts)
               || HasNull(_loadingBlocks) || HasNull(_floatingTexts);

        /// <summary>
        /// Refresh the saved GUID snapshot from the current references. Skips while anything is
        /// missing so a broken state never overwrites good GUIDs. Returns true if the snapshot changed.
        /// </summary>
        public bool SyncPrefabGuids()
        {
            if (HasMissingPrefabReferences()) return false;

            bool changed = false;
            changed |= SyncGuids(_screens, _screenGuids);
            changed |= SyncGuids(_popups, _popupGuids);
            changed |= SyncGuids(_toasts, _toastGuids);
            changed |= SyncGuids(_loadingBlocks, _loadingBlockGuids);
            changed |= SyncGuids(_floatingTexts, _floatingTextGuids);
            changed |= SyncGuid(_flyoutIconPrefab, ref _flyoutIconGuid);
            changed |= SyncGuid(_clickEffectConfig?.Prefab, ref _clickEffectGuid);
            return changed;
        }

        /// <summary>Restore any missing references from the saved GUID snapshot.</summary>
        public void ReloadPrefabReferences()
        {
            FillNulls(_screens, _screenGuids);
            FillNulls(_popups, _popupGuids);
            FillNulls(_toasts, _toastGuids);
            FillNulls(_loadingBlocks, _loadingBlockGuids);
            FillNulls(_floatingTexts, _floatingTextGuids);

            if (_flyoutIconPrefab == null)
                _flyoutIconPrefab = LoadByGuid<FlyoutIcon>(_flyoutIconGuid);
            if (_clickEffectConfig != null && _clickEffectConfig.Prefab == null)
                _clickEffectConfig.Prefab = LoadByGuid<BaseClickEffect>(_clickEffectGuid);

            // Force the type→prefab caches to rebuild from the restored lists.
            _screenCache = null; _popupCache = null; _toastCache = null;
            _loadingBlockCache = null; _floatingTextCache = null;
        }

        private static bool HasNull<T>(List<T> list) where T : UnityEngine.Object
        {
            foreach (var item in list)
                if (item == null) return true;
            return false;
        }

        // Snapshot a list's GUIDs (called only when the list has no nulls).
        private static bool SyncGuids<T>(List<T> list, List<string> guids) where T : UnityEngine.Object
        {
            bool changed = guids.Count != list.Count;
            if (changed) guids.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                var guid = GuidOf(list[i]);
                if (changed) guids.Add(guid);
                else if (guids[i] != guid) { guids[i] = guid; changed = true; }
            }
            return changed;
        }

        private static bool SyncGuid(UnityEngine.Object obj, ref string guid)
        {
            if (obj == null) return false;          // keep the last good GUID if the ref is gone
            var g = GuidOf(obj);
            if (g == guid) return false;
            guid = g;
            return true;
        }

        // Refill only the null slots, by index, so removed entries aren't re-added.
        private static void FillNulls<T>(List<T> list, List<string> guids) where T : UnityEngine.Object
        {
            for (int i = 0; i < list.Count && i < guids.Count; i++)
                if (list[i] == null)
                    list[i] = LoadByGuid<T>(guids[i]);
        }

        private static string GuidOf(UnityEngine.Object obj)
            => obj != null ? UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(obj)) : "";

        private static T LoadByGuid<T>(string guid) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(guid)) return null;
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? null : UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
    }
}
