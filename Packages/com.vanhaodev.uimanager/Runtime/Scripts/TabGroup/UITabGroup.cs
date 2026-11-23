using System;
using System.Collections.Generic;
using UnityEngine;

namespace vanhaodev.uimanager
{
    public class UITabGroup : MonoBehaviour
    {
        [SerializeField] private Transform _container;
        [SerializeField] private UITab _tabPrefab;

        public event Action<int> OnTabChanged;
        public int CurrentIndex { get; private set; } = -1;

        private readonly List<UITab> _tabs = new();

        public UITab AddTab(int index, string label, Sprite icon = null)
        {
            var tab = Instantiate(_tabPrefab, _container);
            tab.Setup(index, label, icon);
            tab.OnClicked += OnTabClicked;
            tab.SetSelected(false);
            _tabs.Add(tab);
            return tab;
        }

        public void Clear()
        {
            foreach (var tab in _tabs)
            {
                tab.OnClicked -= OnTabClicked;
                Destroy(tab.gameObject);
            }
            _tabs.Clear();
            CurrentIndex = -1;
        }

        private void OnTabClicked(int index)
        {
            SelectTab(index);
        }

        public void SelectTab(int index)
        {
            if (index == CurrentIndex) return;

            CurrentIndex = index;

            foreach (var tab in _tabs)
                tab.SetSelected(tab.Index == index);

            OnTabChanged?.Invoke(index);
        }

        public void SetTabLabel(int index, string label)
        {
            var tab = _tabs.Find(t => t.Index == index);
            tab?.SetLabel(label);
        }

        public void SetTabIcon(int index, Sprite icon)
        {
            var tab = _tabs.Find(t => t.Index == index);
            tab?.SetIcon(icon);
        }

        /// <summary>Show or hide the notification dot on a tab (pinned by the UIManager, UILibrary > Badge).</summary>
        public void SetTabBadge(int index, bool visible)
        {
            var tab = _tabs.Find(t => t.Index == index);
            if (tab != null) tab.SetBadge(Manager, visible);
        }

        /// <summary>
        /// The manager that pins the tabs' notification dots. Left unset, it is the one owning the screen or popup
        /// this group sits in; set it for a group outside any.
        /// </summary>
        public UIManager Manager
        {
            get => _manager != null ? _manager : _manager = UIManager.Of(this);
            set => _manager = value;
        }

        private UIManager _manager;

        private void OnDestroy()
        {
            Clear();
        }
    }
}
