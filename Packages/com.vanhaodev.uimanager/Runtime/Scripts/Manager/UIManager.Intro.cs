using System;
using System.Threading.Tasks;
using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// The boot intro, from the UILibrary's intro prefab. Unlike the other features it is not parented to a layer
    /// or cached: the intro is its own overlay Canvas (sorted by the library's Intro Sorting Order), survives scene
    /// loads, and destroys itself once it has left.
    /// </summary>
    public partial class UIManager
    {
        /// <summary>
        /// Put the library's intro on screen, logo hidden, and hand back the sequence that drives it — for a boot
        /// that starts and ends the intro from different places. Then <c>await intro.PlayAsync()</c> (pop + hold)
        /// and <c>await intro.HideAsync()</c> (leave + close). Null when the library has no intro prefab.
        /// </summary>
        public IntroSequence ShowIntro()
        {
            var prefab = _library != null ? _library.IntroPrefab : null;
            if (prefab == null)
            {
                Debug.LogWarning("[UIManager] No intro prefab in the UILibrary (Intro tab).");
                return null;
            }

            var view = IntroView.Create(prefab, _library.IntroSortingOrder);
            return new IntroSequence(view) { HoldSeconds = _library.IntroHoldSeconds };
        }

        /// <summary>
        /// Play the whole intro around <paramref name="work"/>: the work starts at once and runs under the logo,
        /// and the intro leaves once both the hold is over and the work is done. The intro leaves even if the
        /// work throws; the exception is passed on. No intro prefab → just runs the work.
        /// </summary>
        public async Task PlayIntro(Func<Task> work = null)
        {
            var intro = ShowIntro();
            // Started before the pop, not after: the logo costs no load time.
            var running = work != null ? work() : Task.CompletedTask;

            if (intro == null)
            {
                await running;
                return;
            }

            try
            {
                await intro.PlayAsync();
            }
            catch (OperationCanceledException)
            {
                // The view was destroyed mid-intro: nothing left on screen to take away.
                await running;
                return;
            }

            try
            {
                await running;
            }
            finally
            {
                await HideIntroQuietly(intro);
            }
        }

        // A hide cut short by the view being destroyed is not an error — the intro is gone either way — and must
        // not hide an exception from the work.
        private static async Task HideIntroQuietly(IntroSequence intro)
        {
            try
            {
                await intro.HideAsync();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
