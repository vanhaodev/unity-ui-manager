using System;
using System.Threading;
using TMPro;
using UnityEngine;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Ephemeral feedback text that spawns at a point, floats upward and fades out, then
    /// auto-returns to its pool. Fire-and-forget and non-interactive — unlike <see cref="UIElement"/>
    /// it has no show/hide lifecycle. Subclass + author a prefab for each concrete look (the prefab
    /// IS the style); pass data via a typed setter and <c>ShowFloatingText&lt;T&gt;</c>.
    ///
    /// Motion is NOT set in the editor: each concrete type hard-codes its own defaults via
    /// <see cref="CreateDefaultConfig"/>. Override for a single play with <see cref="SetConfig"/> —
    /// the config auto-resets to the type default after the play finishes.
    /// </summary>
    /// <example>
    /// 1) Define a concrete type + author its prefab, then register the prefab in <c>UILibrary</c>:
    /// <code>
    /// public class DamageFloatingText : FloatingText
    /// {
    ///     [SerializeField] private Image _icon;
    ///
    ///     // Hard-coded motion for this type (no editor setup).
    ///     protected override FloatingTextConfig CreateDefaultConfig() => new()
    ///     {
    ///         RiseDistance = 120f, Duration = 1.2f, PopFrom = 0.5f,
    ///     };
    ///
    ///     // Pass data of this type — the prefab defines the look.
    ///     public void SetDamage(int amount) => SetText(amount.ToString());
    /// }
    /// </code>
    ///
    /// 2) Show it from logic, anchored to a UI element (floats up from that element's center):
    /// <code>
    /// // Simplest — built-in default template, just a message:
    /// uiManager.ShowFloatingText("Not enough money", buyButton.transform);
    ///
    /// // Custom type — pass data via the setup callback:
    /// uiManager.ShowFloatingText&lt;DamageFloatingText&gt;(t => t.SetDamage(120), enemy.transform);
    ///
    /// // Override motion for THIS play only (auto-resets to the type default afterwards).
    /// // Tip: keep a Dictionary&lt;string, FloatingTextConfig&gt; of presets and pass one in:
    /// uiManager.ShowFloatingText&lt;DamageFloatingText&gt;(t =>
    /// {
    ///     t.SetDamage(999);
    ///     t.SetConfig(critConfig);   // e.g. new FloatingTextConfig { RiseDistance = 200f, Duration = 1.6f }
    /// }, enemy.transform);
    ///
    /// // World-space object instead of a UI anchor: convert the position yourself.
    /// uiManager.ShowFloatingText("+100", (Vector2)Camera.main.WorldToScreenPoint(enemy.position));
    /// </code>
    /// </example>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class FloatingText : MonoBehaviour
    {
        [SerializeField] protected TMP_Text _text;

        protected RectTransform _rect;
        protected CanvasGroup _canvasGroup;

        private FloatingTextConfig _defaultConfig;   // cached per-type default
        private FloatingTextConfig _config;          // active config for the current/next play

        public UIManager Manager { get; internal set; }

        protected virtual void Awake()
        {
            _rect = transform as RectTransform;
            if (!TryGetComponent(out _canvasGroup))
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Pure feedback never eats input.
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            _defaultConfig = CreateDefaultConfig();
            _config = _defaultConfig;
        }

        /// <summary>
        /// The type's hard-coded motion defaults. Override per subclass to define its own feel
        /// (e.g. crit floats higher and longer). No editor setup required.
        /// </summary>
        protected virtual FloatingTextConfig CreateDefaultConfig() => new();

        /// <summary>Set the displayed text. Color/font/size come from the prefab, not from code.</summary>
        public virtual void SetText(string message)
        {
            if (_text != null) _text.text = message;
        }

        /// <summary>
        /// Override the motion for the NEXT play only. Resets back to the type default automatically
        /// once the play completes. Pass null to keep the default.
        /// </summary>
        public void SetConfig(FloatingTextConfig config)
        {
            if (config != null) _config = config;
        }

        /// <summary>
        /// Play the rise + fade animation once. <paramref name="onComplete"/> fires exactly once
        /// when finished (the manager uses it to release this instance back to the pool).
        /// </summary>
        public void Play(Action onComplete)
        {
            PlayRoutine(onComplete).Forget();
        }

        private async Awaitable PlayRoutine(Action onComplete)
        {
            var cfg = _config;
            var ct = AnimationHelper.ResetToken(this);

            try
            {
                // Reset visual state (instance may be pooled/reused).
                _canvasGroup.alpha = 1f;
                Vector2 start = _rect.anchoredPosition;
                transform.localScale = Vector3.one * cfg.PopFrom;

                // Spawn pop.
                if (!Mathf.Approximately(cfg.PopFrom, 1f))
                    AnimationHelper.Scale(transform, Vector3.one, cfg.PopDuration, ct).Forget();

                // Float up by default; if there isn't room above to rise (too close to the top edge),
                // float down instead so the text stays on screen.
                float dir = 1f;
                if (_rect.parent is RectTransform parent)
                {
                    float roomAbove = parent.rect.height * 0.5f - (start.y + _rect.rect.height * 0.5f);
                    if (roomAbove < cfg.RiseDistance) dir = -1f;
                }

                // Rise (or fall) over the full lifetime.
                var target = start + Vector2.up * (cfg.RiseDistance * dir);
                AnimationHelper.AnchoredPos(_rect, target, cfg.Duration, ct).Forget();

                // Hook for subclasses to add extra motion (e.g. crit shake).
                OnPlay(ct);

                // Fade out during the tail of the lifetime.
                float fadeStart = cfg.Duration * cfg.FadeStartRatio;
                float fadeDuration = Mathf.Max(0.01f, cfg.Duration - fadeStart);

                if (fadeStart > 0f)
                {
                    await Awaitable.WaitForSecondsAsync(fadeStart, ct);
                    if (ct.IsCancellationRequested) return;
                }

                AnimationHelper.Alpha(_canvasGroup, 0f, fadeDuration, ct).Forget();
                await Awaitable.WaitForSecondsAsync(fadeDuration, ct);
                if (ct.IsCancellationRequested) return;

                onComplete?.Invoke();
            }
            finally
            {
                // Always drop any per-play override back to the type default.
                _config = _defaultConfig;
            }
        }

        /// <summary>
        /// Cut the current float short with a quick fade-out, then invoke <paramref name="onComplete"/>.
        /// Used when a source exceeds its active cap, to drop the oldest without an abrupt pop.
        /// </summary>
        public void FadeOutNow(Action onComplete)
        {
            FadeOutRoutine(onComplete).Forget();
        }

        private async Awaitable FadeOutRoutine(Action onComplete)
        {
            const float fade = 0.12f;
            var ct = AnimationHelper.ResetToken(this);   // cancels the in-progress Play

            try
            {
                AnimationHelper.Alpha(_canvasGroup, 0f, fade, ct).Forget();
                await Awaitable.WaitForSecondsAsync(fade, ct);
                if (ct.IsCancellationRequested) return;
                onComplete?.Invoke();
            }
            finally
            {
                _config = _defaultConfig;
            }
        }

        /// <summary>Override to launch additional tweens when the float starts (default: none).</summary>
        protected virtual void OnPlay(CancellationToken ct) { }
    }
}
