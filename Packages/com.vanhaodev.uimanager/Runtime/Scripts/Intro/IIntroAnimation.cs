using System;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// How the intro moves. Put a component implementing this on the intro prefab, beside its
    /// <see cref="IntroView"/>, and <see cref="IntroSequence"/> finds and plays it — so the motion can use any
    /// tween library without this one depending on it. No component → <see cref="DefaultIntroAnimation"/>.
    /// </summary>
    public interface IIntroAnimation
    {
        /// <summary>
        /// Grow the logo in from scale 0 to 1. Call <paramref name="onLanded"/> where it lands (the bounce sound
        /// plays there), then <paramref name="onComplete"/> once it has settled.
        /// </summary>
        void PlayAppear(IntroView view, Action onLanded, Action onComplete);

        /// <summary>
        /// Keep the settled logo alive while it holds (a breathe, a wobble). Runs until <see cref="StopIdle"/>;
        /// nothing waits on it.
        /// </summary>
        void PlayIdle(IntroView view);

        /// <summary>Stop the idle motion and put the logo back at rest, ready for <see cref="PlayHide"/>.</summary>
        void StopIdle(IntroView view);

        /// <summary>Take the intro away, then call <paramref name="onComplete"/>. The view closes after.</summary>
        void PlayHide(IntroView view, Action onComplete);
    }
}
