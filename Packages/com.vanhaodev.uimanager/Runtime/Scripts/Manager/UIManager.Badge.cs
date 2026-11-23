using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Notification dots on any UI element, from the UILibrary's badge prefab. The first show creates the dot as a
    /// child of the target, pinned at the library's Badge Anchor (top-right corner by default); later calls on the
    /// same target reuse it, so toggling every refresh costs nothing. The dot dies with its target.
    /// </summary>
    public partial class UIManager
    {
        private readonly Dictionary<RectTransform, UIAttentionBadge> _badges = new();

        /// <summary>
        /// The manager that owns the screen or popup <paramref name="child"/> sits in — for a row or widget deep
        /// inside one that needs the manager (to show a badge) without being handed it. Null outside any.
        /// </summary>
        public static UIManager Of(Component child)
        {
            if (child == null) return null;
            var owner = child.GetComponentInParent<UIElement>(true);
            return owner != null ? owner.Manager : null;
        }

        /// <summary>Show the dot on <paramref name="target"/>, creating it on first use.</summary>
        public UIAttentionBadge ShowBadge(RectTransform target) => SetBadge(target, true);

        /// <summary>Hide the dot on <paramref name="target"/>. Safe when it never had one.</summary>
        public void HideBadge(RectTransform target) => SetBadge(target, false);

        /// <summary>
        /// Show or hide the dot on <paramref name="target"/> — for a refresh that recomputes "is something
        /// waiting here" each time. Hiding a target that never had a dot creates nothing. Returns the dot, or null
        /// when there is none (no target, hidden before ever shown, or no badge prefab in the library).
        /// </summary>
        public UIAttentionBadge SetBadge(RectTransform target, bool visible)
        {
            if (target == null) return null;

            if (!_badges.TryGetValue(target, out var badge) || badge == null)
            {
                if (!visible) return null;
                badge = CreateBadge(target);
                if (badge == null) return null;
            }

            badge.SetVisible(visible);
            return badge;
        }

        /// <summary>Whether the dot on <paramref name="target"/> is currently shown.</summary>
        public bool IsBadgeShown(RectTransform target)
        {
            return target != null && _badges.TryGetValue(target, out var badge) && badge != null
                   && badge.gameObject.activeSelf;
        }

        private UIAttentionBadge CreateBadge(RectTransform target)
        {
            var prefab = _library != null ? _library.BadgePrefab : null;
            if (prefab == null)
            {
                Debug.LogWarning("[UIManager] No badge prefab in the UILibrary (Badge tab).");
                return null;
            }

            PruneDeadBadges();

            var badge = Instantiate(prefab, target, false);
            if (badge.transform is RectTransform rect)
            {
                rect.anchorMin = _library.BadgeAnchor;
                rect.anchorMax = _library.BadgeAnchor;
                rect.anchoredPosition = _library.BadgeOffset;
            }

            // Drawn over the target's other children, and never laid out by a layout group on the target as if it
            // were one of its items.
            badge.transform.SetAsLastSibling();
            if (!badge.TryGetComponent(out LayoutElement layout))
                layout = badge.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;

            _badges[target] = badge;
            return badge;
        }

        // Targets destroyed since (a row rebuilt, a screen unloaded) took their dot with them; drop their entries.
        private void PruneDeadBadges()
        {
            List<RectTransform> dead = null;
            foreach (var pair in _badges)
            {
                if (pair.Key != null && pair.Value != null) continue;
                (dead ??= new List<RectTransform>()).Add(pair.Key);
            }

            if (dead == null) return;
            foreach (var key in dead) _badges.Remove(key);
        }

        private void ClearBadgeCache()
        {
            foreach (var badge in _badges.Values)
                if (badge != null) Destroy(badge.gameObject);
            _badges.Clear();
        }
    }
}
