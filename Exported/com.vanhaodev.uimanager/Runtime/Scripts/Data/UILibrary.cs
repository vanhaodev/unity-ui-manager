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

        [Header("Toast Config")]
        [Tooltip("Maximum number of toasts visible on screen at once. Exceeding will auto-dismiss the oldest.")]
        [SerializeField] private int _maxConcurrentToasts = 3;
        [Tooltip("Spacing between stacked toasts (px).")]
        [SerializeField] private float _toastSpacing = 12f;
        [Tooltip("Padding from screen edges. X = horizontal (left/right), Y = vertical (top/bottom).")]
        [SerializeField] private Vector2 _toastPadding = new Vector2(24f, 48f);

        private Dictionary<Type, BaseScreen> _screenCache;
        private Dictionary<Type, BasePopup> _popupCache;
        private Dictionary<Type, BaseToast> _toastCache;
        private Dictionary<Type, BaseLoadingBlock> _loadingBlockCache;

        public int MaxConcurrentToasts => _maxConcurrentToasts;
        public float ToastSpacing => _toastSpacing;
        public Vector2 ToastPadding => _toastPadding;

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

            foreach (var screen in _screens)
                if (screen != null) _screenCache[screen.GetType()] = screen;

            foreach (var popup in _popups)
                if (popup != null) _popupCache[popup.GetType()] = popup;

            foreach (var toast in _toasts)
                if (toast != null) _toastCache[toast.GetType()] = toast;

            foreach (var loadingBlock in _loadingBlocks)
                if (loadingBlock != null) _loadingBlockCache[loadingBlock.GetType()] = loadingBlock;
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
    }
}
