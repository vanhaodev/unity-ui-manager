using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// One tab of a <see cref="UITabGroup"/>: a button showing an icon or a label, a selected and a normal look, and
    /// a notification dot pinned by the UIManager. Subclass it for a tab that carries more (a count, a lock).
    /// </summary>
    public class UITab : MonoBehaviour
    {
        [SerializeField] private Button _btn;
        [SerializeField] private TMP_Text _txtLabel;
        [SerializeField] private Image _imgIcon;
        [SerializeField] private GameObject _selectedState;
        [SerializeField] private GameObject _normalState;
        [Tooltip("Where the notification dot is pinned (UIManager.SetBadge). Empty = the tab itself.")]
        [SerializeField] private RectTransform _badgeTarget;
        [Tooltip("Optional: icon shown when the tab is given neither a label nor an icon, so the row keeps its " +
                 "shape instead of showing an empty box. Empty = nothing shown.")]
        [SerializeField] private Sprite _placeholderIcon;

        public int Index { get; private set; }
        public event Action<int> OnClicked;

        public void Setup(int index, string label = null, Sprite icon = null)
        {
            Index = index;
            SetContent(label, icon);
            if (_btn != null) _btn.onClick.AddListener(() => OnClicked?.Invoke(Index));
        }

        /// <summary>
        /// A tab shows an icon or a label, never both: side by side they make one tab wider than its
        /// neighbours, which reads as the row being broken rather than as two pieces of information.
        /// With neither, it shows the placeholder icon so the row keeps its shape.
        /// </summary>
        public void SetContent(string label, Sprite icon)
        {
            // Nothing to show is still a tab the player can press, so it gets the placeholder rather than an
            // empty box that reads as a broken row.
            if (icon == null && string.IsNullOrEmpty(label))
                icon = _placeholderIcon;

            if (_imgIcon)
            {
                _imgIcon.sprite = icon;
                _imgIcon.gameObject.SetActive(icon != null);
            }

            if (_txtLabel)
            {
                bool showLabel = icon == null && !string.IsNullOrEmpty(label);
                if (showLabel) _txtLabel.text = label;
                _txtLabel.gameObject.SetActive(showLabel);
            }
        }

        public void SetSelected(bool selected)
        {
            if (_selectedState != null) _selectedState.SetActive(selected);
            if (_normalState != null) _normalState.SetActive(!selected);
        }

        // Naming a tab switches it back to text: the caller wants the word read, not an icon it cannot see.
        public void SetLabel(string label) => SetContent(label, null);

        public void SetIcon(Sprite icon) => SetContent(null, icon);

        /// <summary>Where this tab's notification dot is pinned.</summary>
        public RectTransform BadgeTarget => _badgeTarget != null ? _badgeTarget : (RectTransform)transform;

        /// <summary>Show or hide this tab's notification dot through <paramref name="ui"/>.</summary>
        public void SetBadge(UIManager ui, bool visible)
        {
            if (ui != null) ui.SetBadge(BadgeTarget, visible);
        }

        private void OnDestroy()
        {
            if (_btn != null) _btn.onClick.RemoveAllListeners();
        }
    }
}
