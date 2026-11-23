using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// A ScrollRect that can sit inside another one scrolling the other way — e.g. a row's horizontal reward
    /// list inside a vertical list of rows. Unity hands a drag to the innermost ScrollRect only, so a vertical
    /// swipe that starts on the reward list would move nothing. This one looks at which way the drag starts:
    /// along its own axis it scrolls as usual; across it, the whole drag (begin → drag → end) is passed to the
    /// nearest ScrollRect above it. With no parent ScrollRect it is a plain ScrollRect.
    ///
    /// Drop-in: it keeps every ScrollRect field, so swapping the script on a ScrollRect keeps its setup.
    /// </summary>
    public class NestedScrollRect : ScrollRect
    {
        private ScrollRect _parent;
        private bool _routeToParent;

        protected override void Awake()
        {
            base.Awake();
            _parent = transform.parent != null ? transform.parent.GetComponentInParent<ScrollRect>() : null;
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            // Both get ready: which one actually takes the drag is only known once it starts moving.
            _parent?.OnInitializePotentialDrag(eventData);
            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            _routeToParent = _parent != null && IsAcrossMyAxis(eventData.position - eventData.pressPosition);
            if (_routeToParent) _parent.OnBeginDrag(eventData);
            else base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (_routeToParent) _parent.OnDrag(eventData);
            else base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_routeToParent) _parent.OnEndDrag(eventData);
            else base.OnEndDrag(eventData);
            _routeToParent = false;
        }

        // A horizontal-only list hands over mostly-vertical drags, and the other way round. A list scrolling
        // both ways keeps every drag.
        private bool IsAcrossMyAxis(Vector2 move)
        {
            bool mostlyVertical = Mathf.Abs(move.y) > Mathf.Abs(move.x);
            if (horizontal && !vertical) return mostlyVertical;
            if (vertical && !horizontal) return !mostlyVertical;
            return false;
        }
    }
}
