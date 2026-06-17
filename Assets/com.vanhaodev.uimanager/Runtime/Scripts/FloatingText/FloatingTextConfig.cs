namespace vanhaodev.uimanager
{
    /// <summary>
    /// Motion parameters for a <see cref="FloatingText"/>. Not authored in the editor — each concrete
    /// type hard-codes its own defaults via <see cref="FloatingText.CreateDefaultConfig"/>, and callers
    /// may pass an override through <see cref="FloatingText.SetConfig"/> for a single play.
    /// </summary>
    public class FloatingTextConfig
    {
        /// <summary>Distance (px) the text rises over its lifetime.</summary>
        public float RiseDistance = 80f;

        /// <summary>Total lifetime in seconds (rise spans the whole duration).</summary>
        public float Duration = 1f;

        /// <summary>Start scale for the spawn 'pop'. 1 = no pop.</summary>
        public float PopFrom = 0.6f;

        /// <summary>Duration (s) of the spawn 'pop' scale-up.</summary>
        public float PopDuration = 0.15f;

        /// <summary>When fade-out begins, as a fraction of Duration (0.5 = fade in the 2nd half).</summary>
        public float FadeStartRatio = 0.5f;
    }
}
