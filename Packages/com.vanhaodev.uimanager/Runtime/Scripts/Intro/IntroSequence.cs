using System;
using System.Threading;
using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// The intro's logic: the logo pops in, holds, and the intro leaves. Drives an <see cref="IntroView"/> and
    /// owns nothing visual itself.
    ///
    /// <para>Usually made by <see cref="UIManager.ShowIntro"/> from the UILibrary's intro prefab, but it needs
    /// nothing from the manager: it can drive any <see cref="IntroView"/>. What a game wants to own comes from
    /// components on the intro prefab, found the way UIManager finds its animations:</para>
    /// <list type="bullet">
    /// <item><see cref="IIntroAnimation"/> — the motion (DOTween or anything else). None →
    /// <see cref="DefaultIntroAnimation"/>.</item>
    /// <item><see cref="IIntroCueHandler"/> — the moments worth a sound.</item>
    /// </list>
    ///
    /// <para>Runs on unscaled time: the intro plays during boot, and must not stall if anything pauses time.</para>
    /// </summary>
    public sealed class IntroSequence
    {
        private readonly IIntroAnimation _animation;
        private readonly IIntroCueHandler[] _cueHandlers;

        public IntroSequence(IntroView view)
        {
            View = view;
            _animation = view.GetComponent<IIntroAnimation>() ?? new DefaultIntroAnimation();
            _cueHandlers = view.GetComponents<IIntroCueHandler>();
        }

        public IntroView View { get; }

        /// <summary>
        /// How long the logo is up, counted from the moment it starts to appear — the pop is inside it, not before
        /// it, so a hold matched to a sound's length ends with the sound. <see cref="PlayAsync"/> completes then
        /// (or once the pop has finished, if the pop is the longer of the two).
        /// </summary>
        public float HoldSeconds { get; set; } = 0.5f;

        /// <summary>
        /// The pop waits for this many frames in a row no longer than <see cref="SteadyFrameSeconds"/>, so it
        /// does not start inside the long first frames of an app (shader warm-up, SDKs starting) and jump
        /// straight to its end. 0 starts at once.
        /// </summary>
        public int SteadyFramesNeeded { get; set; } = 3;

        public float SteadyFrameSeconds { get; set; } = 0.05f;

        /// <summary>Gives up waiting for steady frames after this long and pops anyway.</summary>
        public float MaxSteadyWaitSeconds { get; set; } = 2f;

        /// <summary>
        /// The same wait before the intro leaves, kept short: by then the logo is on screen, so every second of it
        /// is a second the player watches a logo that should already be gone.
        /// </summary>
        public float MaxSteadyWaitBeforeHideSeconds { get; set; } = 0.5f;

        /// <summary>Raised once the logo has finished appearing, before the hold.</summary>
        public event Action Appeared;

        /// <summary>Pop the logo in and hold it. Completes at the end of the hold, with the intro still up.</summary>
        public async Awaitable PlayAsync(CancellationToken cancellationToken = default)
        {
            using var linked = Link(cancellationToken);
            var token = linked.Token;

            View.SetLogoScale(0f);
            // The logo is still at scale 0 here, so this wait is never seen.
            await WaitForSteadyFramesAsync(MaxSteadyWaitSeconds, token);

            // Wall-clock, not summed frame times: a hold that runs across stalled frames must still end on time.
            float holdEnd = Time.realtimeSinceStartup + HoldSeconds;

            RaiseCue(IntroCue.AppearStart);
            await RunAsync(done => _animation.PlayAppear(View, () => RaiseCue(IntroCue.Bounce), done), token);
            Appeared?.Invoke();
            _animation.PlayIdle(View); // runs through the rest of the hold, and on until the hide stops it

            while (Time.realtimeSinceStartup < holdEnd)
                await Awaitable.NextFrameAsync(token);
        }

        /// <summary>Take the intro away, then close the view.</summary>
        public async Awaitable HideAsync(CancellationToken cancellationToken = default)
        {
            using (var linked = Link(cancellationToken))
            {
                // The intro usually leaves while the next scene is still loading under it, which is exactly when
                // frames stall; an exit started inside a stall is gone before it is seen.
                await WaitForSteadyFramesAsync(MaxSteadyWaitBeforeHideSeconds, linked.Token);

                _animation.StopIdle(View);
                RaiseCue(IntroCue.HideStart);
                await RunAsync(done => _animation.PlayHide(View, done), linked.Token);
            }

            View.Close();
        }

        private async Awaitable WaitForSteadyFramesAsync(float maxSeconds, CancellationToken token)
        {
            float giveUpAt = Time.realtimeSinceStartup + maxSeconds;
            int steady = 0;
            while (steady < SteadyFramesNeeded && Time.realtimeSinceStartup < giveUpAt)
            {
                await Awaitable.NextFrameAsync(token);
                steady = Time.unscaledDeltaTime <= SteadyFrameSeconds ? steady + 1 : 0;
            }
        }

        private void RaiseCue(IntroCue cue)
        {
            foreach (var handler in _cueHandlers)
                handler.OnIntroCue(cue);
        }

        // Turns a callback animation into something to await. Cancelled with the token, so an animation that
        // dies with its view (and never calls back) does not leave the sequence hanging.
        private static async Awaitable RunAsync(Action<Action> play, CancellationToken token)
        {
            var source = new AwaitableCompletionSource();
            using (token.Register(() => source.TrySetCanceled()))
            {
                play(() => source.TrySetResult());
                await source.Awaitable;
            }
        }

        // Also cancelled by the view being destroyed, so nothing keeps animating an object that is gone.
        private CancellationTokenSource Link(CancellationToken cancellationToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                View.destroyCancellationToken);
        }
    }
}
