using UnityEngine;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// Sample floating text that exists to exercise <see cref="TextFitter"/>: a plain message, no icon,
    /// whose box comes from the string rather than from the prefab. Feed it lines of wildly different
    /// lengths — ScreenHome's <c>Mgs</c> button does exactly that — and watch the box hug the short
    /// ones, stop at Max Width on the long ones, shrink the font from there, and finally ellipsize.
    ///
    /// On the prefab the label carries the fitter and the root carries a <see cref="TextFitterBackground"/>,
    /// so the root tracks whatever width the label lands on. The visible panel is a *child* of the root
    /// stretched a little past it on each side — that overhang is the padding.
    /// </summary>
    public class FloatingTextMessage : FloatingText
    {
        [Tooltip("The fitter on this prefab's label. Set text through it so the box is right in the " +
                 "same frame; leave empty to fall back to a plain, unfitted label.")]
        [SerializeField] private TextFitter _fitter;

        // Slower and taller than the album float: these strings are long enough to be worth reading.
        protected override FloatingTextConfig CreateDefaultConfig() => new()
        {
            RiseDistance = 140f,
            Duration = 1.6f,
            PopFrom = 0.6f,
            PopDuration = 0.15f,
            FadeStartRatio = 0.6f,
        };

        /// <summary>
        /// Routed through the fitter rather than straight at the label. The fitter resizes on the spot,
        /// which matters here: the manager rebuilds the layout and clamps this thing to the screen
        /// immediately after setup, and a box that has not resized yet would be clamped at its old width.
        /// </summary>
        public override void SetText(string message)
        {
            if (_fitter != null) _fitter.SetText(message);
            else base.SetText(message);
        }
    }
}
