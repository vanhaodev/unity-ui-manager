using UnityEngine.UI;

namespace vanhaodev.uimanager
{
    public static class ScrollRectExtensions
    {
        /// <summary>
        /// Back to the first line — for a cached popup that gets new text each open: a long text scrolled to its
        /// end would otherwise open the next one mid-way. Whether it scrolls at all is the prefab's business
        /// (Clamped + FillViewportHeight: a text that fits cannot move).
        /// </summary>
        public static void ScrollToTop(this ScrollRect scroll)
        {
            if (scroll == null) return;
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f;
        }

        /// <summary>
        /// Jump to the end of a scroll view whose content was just replaced (a log opening on its newest line).
        /// Rebuilds the layout first: the content's new height is only known after a layout pass, and setting the
        /// position against the old height lands short of the bottom.
        /// </summary>
        public static void ScrollToBottom(this ScrollRect scroll)
        {
            if (scroll == null) return;
            scroll.StopMovement();
            if (scroll.content != null)
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            scroll.verticalNormalizedPosition = 0f;
        }
    }
}
