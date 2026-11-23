using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Put on a scroll view's Content (next to its ContentSizeFitter): the content is never shorter than the
    /// viewport, so a short text can sit centred in the visible area (the layout group's child alignment) instead
    /// of hugging the top, while a long one still grows past it and scrolls.
    ///
    /// A layout element rather than a script that sets a height: it only reports a minimum, and the fitter that
    /// already sizes the content takes max(min, preferred) — no second writer fighting over the same rect.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class FillViewportHeight : UIBehaviour, ILayoutElement
    {
        [Tooltip("Area the content must at least fill — the ScrollRect's Viewport. Filled in with the parent when " +
                 "the component is added (or the slot is left empty), so it reads right in the Inspector.")]
        [SerializeField] private RectTransform _viewport;

        private RectTransform Viewport => _viewport != null ? _viewport : transform.parent as RectTransform;

#if UNITY_EDITOR
        // Added from the Inspector's menu / Add Component: point at the parent straight away.
        protected override void Reset()
        {
            base.Reset();
            _viewport = transform.parent as RectTransform;
        }
#endif

        public float minHeight => Viewport != null ? Viewport.rect.height : 0f;

        // Above the layout group (priority 0) so this minimum wins; every other value stays the group's.
        public int layoutPriority => 1;
        public float minWidth => -1f;
        public float preferredWidth => -1f;
        public float flexibleWidth => -1f;
        public float preferredHeight => -1f;
        public float flexibleHeight => -1f;

        public void CalculateLayoutInputHorizontal() { }
        public void CalculateLayoutInputVertical() { }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            // Not via SetDirty: the component is already inactive here, and the content must shrink back.
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (_viewport == null) _viewport = transform.parent as RectTransform;
            SetDirty();
        }
#endif

        private void SetDirty()
        {
            if (IsActive()) LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }
    }
}
