using System;
using TMPro;
using UnityEngine;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Attach to a UI field (e.g., coin counter) to register as a flyout target.
    /// Handles shake animation and optional number lerp when icons arrive.
    /// </summary>
    [DisallowMultipleComponent]
    public class FlyoutTarget : MonoBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private TMP_Text _amountText;

        private RectTransform _rectTransform;
        private UIManager _manager;
        private long _displayValue;
        private long _targetValue;
        private bool _isLerping;
        private bool _isShaking;
        private float _shakeEndTime;
        private Vector2 _originalAnchoredPos;

        public string Key => _key;
        public RectTransform RectTransform => _rectTransform;
        public long CurrentTargetValue => _targetValue;

        /// <summary>Called when each icon arrives. Parameter is value per icon.</summary>
        public event Action<int> OnIconArrived;

        /// <summary>Called when all icons in a batch have arrived.</summary>
        public event Action OnBatchComplete;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
                _rectTransform = gameObject.AddComponent<RectTransform>();
        }

        private void Start()
        {
            _originalAnchoredPos = _rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            _manager = FindAnyObjectByType<UIManager>();
            if (_manager != null)
                _manager.FlyoutRegistry.Register(_key, this);
        }

        private void OnDisable()
        {
            if (_manager != null)
                _manager.FlyoutRegistry.Unregister(_key);

            AnimationHelper.CancelAndRemove(this);
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
                _amountText.text = value.ToString();
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
        /// Merge additional amount into current flyout (no new icons).
        /// Used when spam-clicking to avoid visual clutter.
        /// </summary>
        internal void MergeAmount(int amount)
        {
            _targetValue += amount;
            // Lerp continues naturally if already running
            if (!_isLerping && _amountText != null)
                LerpValueAsync().Forget();
        }

        /// <summary>
        /// Called by flyout system when an icon arrives.
        /// Adds value and triggers shake + lerp.
        /// </summary>
        internal void NotifyIconArrived(int valuePerIcon)
        {
            OnIconArrived?.Invoke(valuePerIcon);

            _targetValue += valuePerIcon;
            if (!_isLerping && _amountText != null)
                LerpValueAsync().Forget();

            ShakeAsync().Forget();
        }

        /// <summary>
        /// Called when all icons in a batch have arrived.
        /// Ensures target value is at least expectedTotal (merged amounts may be higher).
        /// </summary>
        internal void NotifyBatchComplete(long expectedTotal)
        {
            // Safety: ensure at least expected value (merges may have added more)
            if (_targetValue < expectedTotal)
                _targetValue = expectedTotal;
            OnBatchComplete?.Invoke();
        }

        /// <summary>
        /// Shake animation. Virtual for custom override.
        /// </summary>
        protected virtual async Awaitable ShakeAsync()
        {
            if (_manager == null) return;

            var config = _manager.Library?.FlyoutConfig;
            if (config == null) return;

            // Extend shake end time
            _shakeEndTime = Time.time + config.ShakeDuration;

            // Already shaking loop running - it will pick up the extended time
            if (_isShaking) return;

            _isShaking = true;

            try
            {
                while (Time.time < _shakeEndTime)
                {
                    var offset = UnityEngine.Random.insideUnitCircle * config.ShakeIntensity;
                    _rectTransform.anchoredPosition = _originalAnchoredPos + offset;
                    await Awaitable.NextFrameAsync();
                }
            }
            finally
            {
                _rectTransform.anchoredPosition = _originalAnchoredPos;
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
                        _amountText.text = _displayValue.ToString();

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
