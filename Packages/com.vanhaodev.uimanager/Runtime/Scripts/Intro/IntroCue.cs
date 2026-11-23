namespace vanhaodev.uimanager
{
    /// <summary>
    /// The moments an <see cref="IntroSequence"/> reports to its <see cref="IIntroCueHandler"/>s, for the host to
    /// hang sounds (or anything else) on. The library plays no audio itself: what a cue sounds like, and how
    /// loud, is the game's business.
    /// </summary>
    public enum IntroCue
    {
        /// <summary>The logo starts to appear.</summary>
        AppearStart,

        /// <summary>The logo lands — the peak of its pop, reported by the <see cref="IIntroAnimation"/>.</summary>
        Bounce,

        /// <summary>The intro starts to leave.</summary>
        HideStart,
    }
}
