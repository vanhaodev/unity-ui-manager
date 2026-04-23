using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vanhaodev.UIManager
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Layers")]
        [SerializeField] private Transform _screenLayer;
        [SerializeField] private Transform _popupLayer;

        [Header("Library")]
        [SerializeField] private UILibrary _library;

        // Runtime caches
        private readonly Dictionary<string, IScreen> _screenCache = new();
        private readonly Dictionary<string, IPopup> _popupCache = new();

        // State
        private IScreen _currentScreen;
        private readonly List<IPopup> _activePopups = new();

        // Events
        public event Action<IScreen, IScreen> OnScreenChanged;
        public event Action<IPopup> OnPopupOpened;
        public event Action<IPopup> OnPopupClosed;

        // Properties
        public IScreen CurrentScreen => _currentScreen;
        public IReadOnlyList<IPopup> ActivePopups => _activePopups;
        public bool HasActivePopup => _activePopups.Count > 0;
        public IPopup TopPopup => _activePopups.Count > 0 ? _activePopups[^1] : null;

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        #endregion

        #region Screen Methods

        public T ShowScreen<T>(object data = null, Action onComplete = null) where T : BaseScreen
        {
            var screen = GetOrCreateScreen<T>();
            if (screen != null)
                SwitchScreen(screen, data, onComplete);
            return screen;
        }

        public BaseScreen ShowScreen(string screenId, object data = null, Action onComplete = null)
        {
            var screen = GetOrCreateScreen(screenId);
            if (screen != null)
                SwitchScreen(screen, data, onComplete);
            return screen as BaseScreen;
        }

        public T GetScreen<T>() where T : BaseScreen
        {
            var id = typeof(T).Name;
            return _screenCache.TryGetValue(id, out var screen) ? screen as T : null;
        }

        private void SwitchScreen(IScreen newScreen, object data, Action onComplete)
        {
            if (_currentScreen == newScreen)
            {
                onComplete?.Invoke();
                return;
            }

            var oldScreen = _currentScreen;
            _currentScreen = newScreen;

            if (oldScreen != null)
            {
                oldScreen.Hide(() =>
                {
                    ShowNewScreen(newScreen, data, onComplete);
                    OnScreenChanged?.Invoke(oldScreen, newScreen);
                });
            }
            else
            {
                ShowNewScreen(newScreen, data, onComplete);
                OnScreenChanged?.Invoke(null, newScreen);
            }
        }

        private void ShowNewScreen(IScreen screen, object data, Action onComplete)
        {
            if (screen is BaseScreen baseScreen)
            {
                baseScreen.Show(() =>
                {
                    baseScreen.OnEnter(data);
                    onComplete?.Invoke();
                });
            }
            else
            {
                screen.Show(onComplete);
            }
        }

        #endregion

        #region Popup Methods

        public T ShowPopup<T>(object data = null, Action onComplete = null) where T : BasePopup
        {
            var popup = GetOrCreatePopup<T>();
            if (popup != null)
                OpenPopup(popup, onComplete);
            return popup;
        }

        public BasePopup ShowPopup(string popupId, Action onComplete = null)
        {
            var popup = GetOrCreatePopup(popupId);
            if (popup != null)
                OpenPopup(popup, onComplete);
            return popup as BasePopup;
        }

        public T GetPopup<T>() where T : BasePopup
        {
            var id = typeof(T).Name;
            return _popupCache.TryGetValue(id, out var popup) ? popup as T : null;
        }

        public bool IsPopupActive<T>() where T : BasePopup
        {
            return _activePopups.Any(p => p is T);
        }

        private void OpenPopup(IPopup popup, Action onComplete)
        {
            if (_activePopups.Contains(popup))
            {
                onComplete?.Invoke();
                return;
            }

            _activePopups.Add(popup);
            SortPopups();

            popup.Show(() =>
            {
                onComplete?.Invoke();
                OnPopupOpened?.Invoke(popup);
            });
        }

        public void ClosePopup<T>(Action onComplete = null) where T : BasePopup
        {
            var popup = _activePopups.FirstOrDefault(p => p is T);
            if (popup != null)
                ClosePopup(popup, onComplete);
            else
                onComplete?.Invoke();
        }

        public void ClosePopup(IPopup popup, Action onComplete = null)
        {
            if (!_activePopups.Contains(popup))
            {
                onComplete?.Invoke();
                return;
            }

            popup.Hide(() =>
            {
                _activePopups.Remove(popup);
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
                popup.Hide(() =>
                {
                    _activePopups.Remove(popup);
                    OnPopupClosed?.Invoke(popup);

                    remaining--;
                    if (remaining == 0)
                        onComplete?.Invoke();
                });
            }
        }

        private void SortPopups()
        {
            _activePopups.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            for (int i = 0; i < _activePopups.Count; i++)
            {
                if (_activePopups[i] is MonoBehaviour mb)
                    mb.transform.SetSiblingIndex(i);
            }
        }

        #endregion

        #region Factory Methods

        private T GetOrCreateScreen<T>() where T : BaseScreen
        {
            var id = typeof(T).Name;

            if (_screenCache.TryGetValue(id, out var cached))
                return cached as T;

            var prefab = _library?.GetScreenPrefab<T>();
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Screen prefab not found: {id}");
                return null;
            }

            var instance = Instantiate(prefab, _screenLayer);
            instance.gameObject.SetActive(false);
            _screenCache[id] = instance;
            return instance;
        }

        private IScreen GetOrCreateScreen(string id)
        {
            if (_screenCache.TryGetValue(id, out var cached))
                return cached;

            var prefab = _library?.GetScreenPrefab(id);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Screen prefab not found: {id}");
                return null;
            }

            var instance = Instantiate(prefab, _screenLayer);
            instance.gameObject.SetActive(false);
            _screenCache[id] = instance;
            return instance;
        }

        private T GetOrCreatePopup<T>() where T : BasePopup
        {
            var id = typeof(T).Name;

            if (_popupCache.TryGetValue(id, out var cached))
                return cached as T;

            var prefab = _library?.GetPopupPrefab<T>();
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Popup prefab not found: {id}");
                return null;
            }

            var instance = Instantiate(prefab, _popupLayer);
            instance.gameObject.SetActive(false);
            _popupCache[id] = instance;
            return instance;
        }

        private IPopup GetOrCreatePopup(string id)
        {
            if (_popupCache.TryGetValue(id, out var cached))
                return cached;

            var prefab = _library?.GetPopupPrefab(id);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Popup prefab not found: {id}");
                return null;
            }

            var instance = Instantiate(prefab, _popupLayer);
            instance.gameObject.SetActive(false);
            _popupCache[id] = instance;
            return instance;
        }

        #endregion

        #region Utility

        public void ClearCache()
        {
            foreach (var screen in _screenCache.Values)
            {
                if (screen is MonoBehaviour mb && mb != null)
                    Destroy(mb.gameObject);
            }

            foreach (var popup in _popupCache.Values)
            {
                if (popup is MonoBehaviour mb && mb != null)
                    Destroy(mb.gameObject);
            }

            _screenCache.Clear();
            _popupCache.Clear();
            _activePopups.Clear();
            _currentScreen = null;
        }

        public void SetLibrary(UILibrary library)
        {
            _library = library;
        }

        #endregion
    }
}
