using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vanhaodev.UIManager
{
    [CreateAssetMenu(fileName = "UILibrary", menuName = "UI Manager/Library")]
    public class UILibrary : ScriptableObject
    {
        [SerializeField] private List<BaseScreen> _screens = new();
        [SerializeField] private List<BasePopup> _popups = new();

        private Dictionary<Type, BaseScreen> _screenCache;
        private Dictionary<Type, BasePopup> _popupCache;

        private void OnEnable()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            _screenCache = new Dictionary<Type, BaseScreen>();
            _popupCache = new Dictionary<Type, BasePopup>();

            foreach (var screen in _screens)
            {
                if (screen != null)
                    _screenCache[screen.GetType()] = screen;
            }

            foreach (var popup in _popups)
            {
                if (popup != null)
                    _popupCache[popup.GetType()] = popup;
            }
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
    }
}
