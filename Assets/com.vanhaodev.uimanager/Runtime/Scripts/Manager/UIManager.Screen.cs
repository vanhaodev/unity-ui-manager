using System;
using System.Collections.Generic;

namespace Vanhaodev.UIManager
{
    public partial class UIManager
    {
        private readonly Dictionary<Type, BaseScreen> _screenCache = new();
        private BaseScreen _currentScreen;

        public BaseScreen CurrentScreen => _currentScreen;
        public event Action<BaseScreen, BaseScreen> OnScreenChanged;

        public T ShowScreen<T>(object data = null, Action onComplete = null) where T : BaseScreen
        {
            var screen = GetOrCreateScreen<T>();
            if (screen != null)
                ShowScreenInternal(screen, data, onComplete);
            return screen;
        }

        private void ShowScreenInternal(BaseScreen screen, object data, Action onComplete)
        {
            if (_currentScreen == screen)
            {
                onComplete?.Invoke();
                return;
            }

            var oldScreen = _currentScreen;
            _currentScreen = screen;

            if (oldScreen != null)
            {
                oldScreen.Close(() =>
                {
                    screen.OnEnter(data);
                    screen.Show(onComplete);
                    OnScreenChanged?.Invoke(oldScreen, screen);
                });
            }
            else
            {
                screen.OnEnter(data);
                screen.Show(onComplete);
                OnScreenChanged?.Invoke(null, screen);
            }
        }

        public void CloseScreen(Action onComplete = null)
        {
            if (_currentScreen == null)
            {
                onComplete?.Invoke();
                return;
            }

            var oldScreen = _currentScreen;
            _currentScreen = null;
            oldScreen.Close(() =>
            {
                onComplete?.Invoke();
                OnScreenChanged?.Invoke(oldScreen, null);
            });
        }

        public T GetScreen<T>() where T : BaseScreen
        {
            return _screenCache.TryGetValue(typeof(T), out var screen) ? screen as T : null;
        }

        private T GetOrCreateScreen<T>() where T : BaseScreen
        {
            var type = typeof(T);
            if (_screenCache.TryGetValue(type, out var cached))
                return cached as T;

            var prefab = _library?.GetScreenPrefab<T>();
            if (prefab == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] Screen not found: {type.Name}");
                return null;
            }

            var instance = Instantiate(prefab, _screenLayer);
            instance.gameObject.SetActive(false);
            instance.Manager = this;
            _screenCache[type] = instance;
            return instance;
        }

        private void ClearScreenCache()
        {
            foreach (var screen in _screenCache.Values)
                if (screen != null) Destroy(screen.gameObject);
            _screenCache.Clear();
            _currentScreen = null;
        }
    }
}
