using System;
using System.Collections.Generic;
using UnityEngine;

namespace vanhaodev.uimanager
{
    [CreateAssetMenu(fileName = "UILibrary", menuName = "UI Manager/Library")]
    public class UILibrary : ScriptableObject
    {
        [Header("── Prefabs ──")]
        [SerializeField] private List<BaseScreen> _screens = new();
        [SerializeField] private List<BasePopup> _popups = new();
        [SerializeField] private List<BaseToast> _toasts = new();
        [SerializeField] private List<BaseLoadingBlock> _loadingBlocks = new();
        [SerializeField] private List<FloatingText> _floatingTexts = new();

        [Space(8)]
        [Header("── Toast Config ──")]
        [Tooltip("Maximum number of toasts visible on screen at once. Exceeding will auto-dismiss the oldest.")]
        [SerializeField] private int _maxConcurrentToasts = 3;
        [Tooltip("Spacing between stacked toasts (px).")]
        [SerializeField] private float _toastSpacing = 12f;
        [Tooltip("Padding from screen edges. X = horizontal (left/right), Y = vertical (top/bottom).")]
        [SerializeField] private Vector2 _toastPadding = new Vector2(24f, 48f);

        [Space(8)]
        [Header("── Floating Text Config ──")]
        [Tooltip("Delay (s) between floating texts shown from the same source, so they don't overlap.")]
        [SerializeField] private float _floatingShowInterval = 0.2f;
        [Tooltip("Max active floating texts per source. Beyond this, the oldest fades out immediately to avoid lag.")]
        [SerializeField] private int _floatingMaxPerSource = 20;
        [Tooltip("Keep floating texts this far (px) inside the screen edges. X = left/right, Y = top/bottom.")]
        [SerializeField] private Vector2 _floatingScreenPadding = new Vector2(16f, 16f);

        [Space(8)]
        [Header("── Flyout Effect Config ──")]
        [Tooltip("Default flyout icon prefab (requires Image + CanvasGroup + FlyoutIcon).")]
        [SerializeField] private FlyoutIcon _flyoutIconPrefab;
        [Tooltip("Flyout effect animation and behavior settings.")]
        [SerializeField] private FlyoutConfig _flyoutConfig = new();

        [Space(8)]
        [Header("── Click Effect Config ──")]
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
    }
}
