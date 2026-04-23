using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vanhaodev.UIManager
{
    [CreateAssetMenu(fileName = "UILibrary", menuName = "Vanhaodev/UI/Library")]
    public class UILibrary : ScriptableObject
    {
        [Serializable]
        public class ScreenEntry
        {
            public string Id;
            public BaseScreen Prefab;
        }

        [Serializable]
        public class PopupEntry
        {
            public string Id;
            public BasePopup Prefab;
        }

        [Header("Screens")]
        [SerializeField] private List<ScreenEntry> _screens = new();

        [Header("Popups")]
        [SerializeField] private List<PopupEntry> _popups = new();

        // Runtime caches
        private Dictionary<string, ScreenEntry> _screenDict;
        private Dictionary<string, PopupEntry> _popupDict;
        private Dictionary<Type, ScreenEntry> _screenTypeDict;
        private Dictionary<Type, PopupEntry> _popupTypeDict;

        private void OnEnable()
        {
            BuildCaches();
        }

        private void BuildCaches()
        {
            _screenDict = new Dictionary<string, ScreenEntry>();
            _popupDict = new Dictionary<string, PopupEntry>();
            _screenTypeDict = new Dictionary<Type, ScreenEntry>();
            _popupTypeDict = new Dictionary<Type, PopupEntry>();

            foreach (var entry in _screens)
            {
                if (entry.Prefab == null) continue;

                if (!string.IsNullOrEmpty(entry.Id))
                    _screenDict[entry.Id] = entry;

                var type = entry.Prefab.GetType();
                _screenTypeDict[type] = entry;
            }

            foreach (var entry in _popups)
            {
                if (entry.Prefab == null) continue;

                if (!string.IsNullOrEmpty(entry.Id))
                    _popupDict[entry.Id] = entry;

                var type = entry.Prefab.GetType();
                _popupTypeDict[type] = entry;
            }
        }

        #region Screen Getters

        public T GetScreenPrefab<T>() where T : BaseScreen
        {
            if (_screenTypeDict == null) BuildCaches();

            var type = typeof(T);
            if (_screenTypeDict.TryGetValue(type, out var entry))
                return entry.Prefab as T;

            // Fallback: search in list
            var found = _screens.FirstOrDefault(s => s.Prefab is T);
            return found?.Prefab as T;
        }

        public BaseScreen GetScreenPrefab(string id)
        {
            if (_screenDict == null) BuildCaches();

            if (_screenDict.TryGetValue(id, out var entry))
                return entry.Prefab;

            // Fallback
            return _screens.FirstOrDefault(s => s.Id == id)?.Prefab;
        }

        public IEnumerable<string> GetAllScreenIds()
        {
            return _screens.Where(s => !string.IsNullOrEmpty(s.Id)).Select(s => s.Id);
        }

        #endregion

        #region Popup Getters

        public T GetPopupPrefab<T>() where T : BasePopup
        {
            if (_popupTypeDict == null) BuildCaches();

            var type = typeof(T);
            if (_popupTypeDict.TryGetValue(type, out var entry))
                return entry.Prefab as T;

            // Fallback
            var found = _popups.FirstOrDefault(p => p.Prefab is T);
            return found?.Prefab as T;
        }

        public BasePopup GetPopupPrefab(string id)
        {
            if (_popupDict == null) BuildCaches();

            if (_popupDict.TryGetValue(id, out var entry))
                return entry.Prefab;

            // Fallback
            return _popups.FirstOrDefault(p => p.Id == id)?.Prefab;
        }

        public IEnumerable<string> GetAllPopupIds()
        {
            return _popups.Where(p => !string.IsNullOrEmpty(p.Id)).Select(p => p.Id);
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        public void AddScreen(string id, BaseScreen prefab)
        {
            _screens.Add(new ScreenEntry { Id = id, Prefab = prefab });
            BuildCaches();
        }

        public void AddPopup(string id, BasePopup prefab)
        {
            _popups.Add(new PopupEntry { Id = id, Prefab = prefab });
            BuildCaches();
        }

        public void RemoveScreen(string id)
        {
            _screens.RemoveAll(s => s.Id == id);
            BuildCaches();
        }

        public void RemovePopup(string id)
        {
            _popups.RemoveAll(p => p.Id == id);
            BuildCaches();
        }

        [ContextMenu("Auto-fill IDs from Prefab Names")]
        private void AutoFillIds()
        {
            foreach (var entry in _screens)
            {
                if (entry.Prefab != null && string.IsNullOrEmpty(entry.Id))
                    entry.Id = entry.Prefab.GetType().Name;
            }

            foreach (var entry in _popups)
            {
                if (entry.Prefab != null && string.IsNullOrEmpty(entry.Id))
                    entry.Id = entry.Prefab.GetType().Name;
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        #endregion
    }
}
