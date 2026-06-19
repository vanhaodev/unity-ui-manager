using System;
using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Base class for the visual played at a click/touch point. The manager spawns it from a pool,
    /// positions it, then calls <see cref="Play"/>. Subclass this to do anything — a sprite ripple
    /// (see <c>ClickEffectRipple</c>), a ParticleSystem / VFX Graph burst, a sound, or all of them.
    /// Always invoke the <c>onComplete</c> callback when your effect finishes so the pool can reclaim it.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseClickEffect : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        /// <summary>Reset visual state when taken from the pool (called right before <see cref="Play"/>).</summary>
        public virtual void ResetState() { }

        /// <summary>
        /// Play the effect. Call <paramref name="onComplete"/> exactly once when it ends — the manager
        /// uses it to return this instance to the pool.
        /// </summary>
        public abstract void Play(Action onComplete);
    }
}
