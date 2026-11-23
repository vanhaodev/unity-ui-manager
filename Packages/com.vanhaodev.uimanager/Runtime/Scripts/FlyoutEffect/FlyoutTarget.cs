using System;
using TMPro;
using UnityEngine;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Attach to a UI field (e.g., coin counter) to register as a flyout target.
    /// Handles the arrival shake. The displayed number is app-driven via
    /// SetValue/SetTargetValue and can be formatted through <see cref="Formatter"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class FlyoutTarget : MonoBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private TMP_Text _amountText;

        [Header("Flyout aim (where icons land)")]
        [Tooltip("Optional explicit landing point. If set, icons fly to this exact transform — " +
                 "drop an empty child where you want them to hit. Overrides the alignment/centre aim. " +
                 "Alignment vs centre and the edge gap are configured globally in UILibrary > FlyoutConfig.")]
        [SerializeField] private RectTransform _aimPoint;

        [Header("Arrival shake (strength/length: UILibrary > FlyoutConfig)")]
        [Tooltip("Off = icons land without shaking anything.")]
        [SerializeField] private bool _shakeOnArrival = true;
        [Tooltip("Rect shaken when an icon lands. Empty = this object. Prefer one no layout group positions (a " +
                 "field's inner content): a layout group re-places its children and fights the shake.")]
        [SerializeField] private RectTransform _shakeRoot;

        private RectTransform _rectTransform;
        private UIManager _manager;
        private long _displayValue;
        private long _targetValue;
        private bool _isLerping;
        private bool _isShaking;
        private float _shakeEndTime;

        public string Key => _key;
        public RectTransform RectTransform => _rectTransform;
        public long CurrentTargetValue => _targetValue;

        /// <summary>Called when each icon arrives. Parameter is value per icon.</summary>
        public event Action<int> OnIconArrived;

        /// <summary>Called when all icons in a batch have arrived.</summary>
        public event Action OnBatchComplete;

        /// <summary>
        /// Optional formatter for the displayed value — supports any style the game needs
        /// (e.g. "123456789", "1.000.000", "1k2", "1b3", "∞"). Null shows the raw number.
        /// </summary>
        public Func<long, string> Formatter;

        private string Format(long value) => Formatter?.Invoke(value) ?? value.ToString();

        /// <summary>
        /// World-space point the flyout icons should fly into. Resolves, in priority order:
        /// an explicit <c>_aimPoint</c>, the amount text (centre, or an alignment-chosen edge when
        /// <see cref="FlyoutConfig.AimByAlignment"/> is on), then the root rect centre. Aiming at the
        /// text keeps icons on the number even when the field is wide.
        /// </summary>
        public Vector3 GetAimWorldPosition(FlyoutConfig config)
        {
            if (_aimPoint != null)
                return _aimPoint.position;

            if (_amountText != null)
            {
                // textBounds is in the text object's local space and tracks the actual glyphs.
                _amountText.ForceMeshUpdate();
                var bounds = _amountText.textBounds;
                if (bounds.size.sqrMagnitude > 0f)
                {
                    var x = bounds.center.x;
                    if (config != null && config.AimByAlignment)
                    {
                        // head = bounds.min.x (start), tail = bounds.max.x (end). Push outward by the
                        // gap so the icon lands just past the digits instead of overlapping them.
                        // Left grows rightward → tail; Right grows leftward → head; Centre → tail.
                        var gap = bounds.size.y * config.AimEdgeGap;
                        x = _amountText.horizontalAlignment == HorizontalAlignmentOptions.Right
                            ? bounds.min.x - gap   // right-align → head, nudged left
                            : bounds.max.x + gap;  // left-align / centre / other → tail, nudged right
                    }
                    return _amountText.rectTransform.TransformPoint(new Vector3(x, bounds.center.y, bounds.center.z));
                }
                return _amountText.rectTransform.position;
            }

            return _rectTransform != null ? _rectTransform.position : transform.position;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
                _rectTransform = gameObject.AddComponent<RectTransform>();
        }

        private void OnDisable()
        {
            AnimationHelper.CancelAndRemove(this);
        }

        private void OnDestroy()
        {
            // Safety net so a destroyed target never lingers as a dead entry in the registry.
            Unregister();
        }

        /// <summary>
        /// Register this target with the given <see cref="UIManager"/> under its <see cref="Key"/>.
        /// Call this yourself (e.g. from a screen's setup) — the library does not auto-discover the
        /// manager. The manager reference is also required for the arrival shake.
        /// </summary>
        public void Register(UIManager manager)
        {
            if (manager == null) return;
            _manager = manager;
            _manager.FlyoutRegistry.Register(_key, this);
        }

        /// <summary>Remove this target from its manager's registry. Safe to call when not registered.</summary>
        public void Unregister()
        {
            if (_manager == null) return;
            _manager.FlyoutRegistry.Unregister(_key);
            _manager = null;
        }

        /// <summary>
        /// Set both display and target value. Call once at init to sync.
        /// </summary>
        public void SetValue(long value)
        {
            _displayValue = value;
            _targetValue = value;
            if (_amountText != null)
                _amountText.text = Format(value);
        }

        /// <summary>
        /// Set only target value, keeping display value unchanged.
        /// This allows lerp to animate from current display to new target.
        /// </summary>
        public void SetTargetValue(long value)
        {
            _targetValue = value;
            if (!_isLerping && _amountText != null)
                LerpValueAsync().Forget();
        }

        /// <summary>
        /// Called by the flyout system when an icon arrives. Triggers the shake only;
        /// the number is owned by the app — drive it via SetTargetValue/SetValue.
        /// </summary>
        internal void NotifyIconArrived(int valuePerIcon)
        {
            OnIconArrived?.Invoke(valuePerIcon);
            ShakeAsync().Forget();
        }

        /// <summary>
        /// Called when all icons in a batch have arrived. Fires the batch event;
        /// the displayed number is driven by the app, not by the flyout.
        /// </summary>
        internal void NotifyBatchComplete()
        {
            OnBatchComplete?.Invoke();
        }

        /// <summary>
        /// The arrival shake with no icon flying in: a gain felt on the counter itself. <paramref name="duration"/>
        /// is the whole shake — one arrival lasts only FlyoutConfig's ShakeDuration, and a flyout feels longer
        /// because each landing icon extends it, so a caller standing in for a flyout passes the span of one.
        /// Shares the arrival loop, so a call during one only extends it.
        /// </summary>
        public void Shake(float duration) => RunShakeAsync(duration).Forget();

        /// <summary>
        /// The shake played when an icon lands. Virtual for custom override.
        /// </summary>
        protected virtual Awaitable ShakeAsync() => RunShakeAsync(Config != null ? Config.ShakeDuration : 0f);

        private FlyoutConfig Config =>
            _manager != null && _manager.Library != null ? _manager.Library.FlyoutConfig : null;

        private async Awaitable RunShakeAsync(float duration)
        {
            var config = Config;
            var root = _shakeRoot != null ? _shakeRoot : _rectTransform;
            if (!_shakeOnArrival || config == null || root == null || duration <= 0f) return;

            _shakeEndTime = Mathf.Max(_shakeEndTime, Time.time + duration);

            // A loop already running picks up the extended end time, so icons landing together lengthen one
            // shake instead of restarting it.
            if (_isShaking) return;
            _isShaking = true;

            // Read now rather than at Start: by the time an icon lands, whatever positions this rect (a layout
            // group) has had its pass, so this is the place it should return to.
            var origin = root.anchoredPosition;

            try
            {
                while (Time.time < _shakeEndTime)
                {
                    root.anchoredPosition = origin + UnityEngine.Random.insideUnitCircle * config.ShakeIntensity;
                    await Awaitable.NextFrameAsync(destroyCancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Destroyed mid-shake: nothing left to put back.
            }
            finally
            {
                if (root != null) root.anchoredPosition = origin;
                _isShaking = false;
            }
        }

        private async Awaitable LerpValueAsync()
        {
            _isLerping = true;

            try
            {
                while (_displayValue != _targetValue)
                {
                    var diff = _targetValue - _displayValue;
                    var step = Math.Max(1L, Math.Abs(diff) / 10);

                    if (diff > 0)
                        _displayValue = Math.Min(_displayValue + step, _targetValue);
                    else
                        _displayValue = Math.Max(_displayValue - step, _targetValue);

                    if (_amountText != null)
                        _amountText.text = Format(_displayValue);

                    await Awaitable.NextFrameAsync();
                }
            }
            finally
            {
                _isLerping = false;
            }
        }
    }
}
