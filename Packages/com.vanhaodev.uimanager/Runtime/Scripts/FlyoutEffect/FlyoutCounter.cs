using System;
using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Drives a counter's number in step with the flyout icons landing on its <see cref="FlyoutTarget"/>
    /// (gold, gems, ...). A GAIN is held and ticked up as reward icons land (in sync with the animation);
    /// SPENDS / other changes apply immediately. A short timeout snaps held gains that never
    /// receive icons (a gain granted outside a reward animation), so non-flyout gains still show up.
    ///
    /// <para>The app still owns the number: it feeds the real value (<see cref="SetActual"/>) and renders whatever
    /// the counter hands back through its display callback. Call <see cref="Tick"/> every frame.</para>
    /// </summary>
    public class FlyoutCounter
    {
        private readonly FlyoutTarget _target;
        private readonly float _holdTimeout;
        private readonly Action<long, bool> _display; // (value, instant): instant skips any lerp
        private readonly Func<bool> _isSuppressed;     // optional: snap instead of tick

        private long _shown;
        private long _actual;
        private bool _holding;
        private float _holdStartTime;

        /// <param name="display">Renders a value; bool = render instantly (no count-up).</param>
        /// <param name="isSuppressed">When true, values snap and icons don't tick (e.g. unlimited).</param>
        /// <param name="holdTimeout">Snap a held gain after this long without a flyout icon. Must outlast the wait
        /// for the FIRST icon of a batch (burst plus flight in FlyoutConfig), or every gain snaps before the
        /// count-up it is waiting for can start.</param>
        public FlyoutCounter(FlyoutTarget target, Action<long, bool> display, Func<bool> isSuppressed = null,
            float holdTimeout = 1.25f)
        {
            _target = target;
            _holdTimeout = holdTimeout;
            _display = display;
            _isSuppressed = isSuppressed;
        }

        public void Enable()
        {
            if (_target == null) return;
            _target.OnIconArrived += OnIconArrived;
            _target.OnBatchComplete += OnBatchComplete;
        }

        public void Disable()
        {
            if (_target == null) return;
            _target.OnIconArrived -= OnIconArrived;
            _target.OnBatchComplete -= OnBatchComplete;
            _holding = false;
        }

        /// <summary>Feed the current model value. Use <paramref name="initial"/> on first sync to snap.</summary>
        public void SetActual(long value, bool initial = false)
        {
            _actual = value;

            if (initial || (_isSuppressed != null && _isSuppressed()) || value <= _shown)
            {
                Snap();              // initial sync, suppressed (∞), or a spend/decrease
                return;
            }

            // A gain: hold the number; the flyout icons will tick it up to the new value.
            _holding = true;
            _holdStartTime = Time.unscaledTime;
        }

        /// <summary>Call every frame (Update) so held non-flyout gains snap after the timeout.</summary>
        public void Tick()
        {
            if (_holding && Time.unscaledTime - _holdStartTime > _holdTimeout)
                Snap();
        }

        private void OnIconArrived(int value)
        {
            if (_isSuppressed != null && _isSuppressed()) return;

            // Not holding means the full value is already on screen: the hold timed out, a spend
            // landed, or an earlier batch finished. Icons still in the air belong to a total that
            // has been shown, so adding them again would count past it.
            if (!_holding) return;

            // Icons of a batch sum to exactly the gain they carry, but batches can overlap: a second
            // gain arrives while the first is still flying, and its snap already covers both.
            _shown = Math.Min(_shown + value, _actual);
            _holdStartTime = Time.unscaledTime; // keep holding while icons keep landing
            _display(_shown, false);
        }

        private void OnBatchComplete() => Snap();

        private void Snap()
        {
            _holding = false;
            _shown = _actual;
            _display(_shown, true);
        }
    }
}
