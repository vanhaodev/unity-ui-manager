using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager
{
    public abstract class BaseToast : InteractableUI, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("Toast - Common")]
        [SerializeField] protected TMP_Text _messageText;
        [SerializeField] protected float _defaultDuration = 2.5f;

        [Header("Toast - Layout")]
        [SerializeField] protected Vector2 _contentPadding = new Vector2(40f, 24f);
        [SerializeField, Range(0.1f, 1f)] protected float _maxWidthRatio = 0.8f;
        [SerializeField, Range(0.1f, 1f)] protected float _maxHeightRatio = 0.4f;

        [Header("Toast - Swipe")]
        [SerializeField] protected float _swipeDismissThreshold = 150f;

        private RectTransform _rect;
        private float _autoCloseTimer;
        private bool _autoCloseEnabled;
        private Vector2 _dragStartPos;
        private bool _isDragging;

        public string ToastId { get; internal set; }
        public ToastPositionType PositionType { get; internal set; } = ToastPositionType.Bottom;
        public bool IsDismissing { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _rect = transform as RectTransform;
        }

        protected override void OnShowStart()
        {
            IsDismissing = false;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            ResetAutoClose();
        }

        protected override void OnCloseStart()
        {
            IsDismissing = true;
            _autoCloseEnabled = false;
        }

        public override void Close(Action onComplete = null)
        {
            if (IsDismissing) { onComplete?.Invoke(); return; }
            base.Close(onComplete);
        }

        public virtual void SetMessage(string message)
        {
            if (_messageText != null) _messageText.text = message;
            ResizeToContent();
        }

        protected virtual void ResizeToContent()
        {
            if (_messageText == null) return;
            var rect = transform as RectTransform;
            var parent = rect != null ? rect.parent as RectTransform : null;
            if (rect == null || parent == null) return;

            float layerW = parent.rect.width;
            float layerH = parent.rect.height;
            if (layerW <= 0f || layerH <= 0f) return;

            float maxW = layerW * _maxWidthRatio;
            float maxH = layerH * _maxHeightRatio;

            _messageText.textWrappingMode = TextWrappingModes.Normal;
            _messageText.overflowMode = TextOverflowModes.Overflow;

            Vector2 prefSingleLine = _messageText.GetPreferredValues(_messageText.text);
            float targetW = Mathf.Min(prefSingleLine.x + _contentPadding.x, maxW);

            float textW = Mathf.Max(1f, targetW - _contentPadding.x);
            Vector2 prefAtW = _messageText.GetPreferredValues(_messageText.text, textW, 0f);
            float targetH = Mathf.Min(prefAtW.y + _contentPadding.y, maxH);

            rect.sizeDelta = new Vector2(targetW, targetH);

            bool overflows = prefAtW.y + _contentPadding.y > maxH + 0.5f;
            _messageText.overflowMode = overflows ? TextOverflowModes.Ellipsis : TextOverflowModes.Overflow;
        }

        public void SetAutoCloseDuration(float seconds)
        {
            _defaultDuration = seconds;
        }

        protected override void OnCloseClicked()
        {
            Manager?.HideToast(ToastId);
        }

        public void ResetAutoClose()
        {
            _autoCloseTimer = _defaultDuration;
            _autoCloseEnabled = _defaultDuration > 0f;
        }

        protected virtual void Update()
        {
            if (!_autoCloseEnabled || IsDismissing || _isDragging) return;
            _autoCloseTimer -= Time.unscaledDeltaTime;
            if (_autoCloseTimer <= 0f)
            {
                _autoCloseEnabled = false;
                Manager?.HideToast(ToastId);
            }
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (IsDismissing || _rect == null) return;
            _isDragging = true;
            _dragStartPos = _rect.anchoredPosition;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (IsDismissing || _rect == null) return;
            _rect.anchoredPosition += new Vector2(eventData.delta.x, 0f);
            if (_canvasGroup != null)
            {
                float dist = Mathf.Abs(_rect.anchoredPosition.x - _dragStartPos.x);
                _canvasGroup.alpha = Mathf.Clamp01(1f - dist / (_swipeDismissThreshold * 1.5f));
            }
        }

        public virtual async void OnEndDrag(PointerEventData eventData)
        {
            if (IsDismissing || _rect == null) return;
            _isDragging = false;
            float dist = Mathf.Abs(_rect.anchoredPosition.x - _dragStartPos.x);

            var ct = AnimationHelper.ResetToken(this);

            if (dist >= _swipeDismissThreshold)
            {
                float dir = Mathf.Sign(_rect.anchoredPosition.x - _dragStartPos.x);
                var targetPos = new Vector2(_dragStartPos.x + dir * Screen.width, _rect.anchoredPosition.y);

                AnimationHelper.AnchoredPos(_rect, targetPos, 0.15f, ct).Forget();
                if (_canvasGroup != null) AnimationHelper.Alpha(_canvasGroup, 0f, 0.15f, ct).Forget();

                await Awaitable.WaitForSecondsAsync(0.15f, ct);
                if (!ct.IsCancellationRequested) Manager?.HideToast(ToastId);
            }
            else
            {
                AnimationHelper.AnchoredPos(_rect, _dragStartPos, 0.2f, ct).Forget();
                if (_canvasGroup != null) AnimationHelper.Alpha(_canvasGroup, 1f, 0.2f, ct).Forget();
            }
        }
    }
}
