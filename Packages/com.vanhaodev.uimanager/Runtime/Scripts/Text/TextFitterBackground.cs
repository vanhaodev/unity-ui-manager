using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// A box that tracks a <see cref="TextFitter"/>'s label, so a string sitting over a busy scene has
    /// something solid behind it and stays readable.
    ///
    /// Lives on the background object and is driven by the fitter: the fitter keeps the list and pushes
    /// the label's size in here every time it resizes. Nothing is read back out, which is what keeps the
    /// box and the label from chasing each other's size.
    ///
    /// The box lands on exactly the label's size — no padding here. The artwork goes on a *child* of it
    /// anchored to stretch, and that child's offsets are the padding: pulled out a few pixels on each
    /// side for a sliced sprite's border, authored by eye in the inspector rather than typed in as
    /// numbers, and free to differ per background.
    ///
    /// Sit it on whichever object should end up the size of the label. The label's *parent* is fine —
    /// that is what the shipped FloatingTextDefault.prefab does, and for a floating text it is the right
    /// call, since UIManager measures the ROOT when it nudges the label inside the screen edges and a
    /// background on a separate sibling would leave that root at its authored size. The one layout to
    /// avoid is a label anchored to *stretch* to the box: the box would resize the label, and that label
    /// would then resize it right back.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class TextFitterBackground : MonoBehaviour
    {
        /// <summary>Which sides of the box follow the label.</summary>
        public enum FitAxis
        {
            Horizontal,
            Vertical,
            Both,
        }

        [Tooltip("Which sides follow the label. The fitter only ever changes the label's width, so " +
                 "Horizontal is the usual pick; the others are for a box that hugs its height too.")]
        [SerializeField] private FitAxis _axis = FitAxis.Horizontal;

        private RectTransform _rect;

        /// <summary>
        /// Takes the label's size on the chosen axis. Called by the fitter holding this background —
        /// a size already in place is skipped, which also breaks the loop if the box does end up
        /// driving its own label.
        /// </summary>
        public void Fit(Vector2 labelSize)
        {
            if (_rect == null) _rect = transform as RectTransform;
            if (_rect == null) return;

            Vector2 current = _rect.rect.size;

            // Compared with a pixel of slack: sizes arrive as fractions and a write that changes
            // nothing still dirties the canvas.
            if ((_axis is FitAxis.Horizontal or FitAxis.Both) && Mathf.Abs(labelSize.x - current.x) > 0.01f)
                _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, labelSize.x);

            if ((_axis is FitAxis.Vertical or FitAxis.Both) && Mathf.Abs(labelSize.y - current.y) > 0.01f)
                _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, labelSize.y);
        }
    }
}
