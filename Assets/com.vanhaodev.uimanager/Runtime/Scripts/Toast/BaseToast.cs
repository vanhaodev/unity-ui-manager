using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Base toast UI element. Extend this for custom toast styles (info, success, error, action...).
    /// Supports auto-dismiss after duration and swipe-to-dismiss horizontally.
    /// </summary>
    public abstract class BaseToast : InteractableUI, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("Toast - Common")]
        [SerializeField] protected TMP_Text _messageText;
        [SerializeField] protected float _defaultDuration = 2.5f;

        [Header("Toast - Layout")]
        [Tooltip("Inner padding around the message text (X = horizontal, Y = vertical) in canvas units.")]
        [SerializeField] protected Vector2 _contentPadding = new Vector2(40f, 24f);
        [Tooltip("Max width as a ratio of the toast layer width (0..1).")]
        [SerializeField, Range(0.1f, 1f)] protected float _maxWidthRatio = 0.8f;
        [Tooltip("Max height as a ratio of the toast layer height (0..1).")]
        [SerializeField, Range(0.1f, 1f)] protected float _maxHeightRatio = 0.4f;

        [Header("Toast - Swipe")]
        [SerializeField] protected float _swipeDismissThreshold = 150f;

        private IToastAnimation _toastAnimation;
        private RectTransform _rect;
        private float _autoCloseTimer;
        private bool _autoCloseEnabled;
        private Vector2 _dragStartPos;
        private bool _isDragging;

        public string ToastId { get; internal set; }
        public ToastPosition Position { get; internal set; } = ToastPosition.Bottom;
        public bool IsDismissing { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _rect = transform as RectTransform;
        }

        public void SetToastAnimation(IToastAnimation animation) => _toastAnimation = animation;

        public virtual void SetMessage(string message)
        {
            if (_messageText != null) _messageText.text = message;
            ResizeToContent();
        }

        /// <summary>
        /// Auto-fit toast size to message:
        /// 1) Grow width up to maxWidthRatio * layerWidth.
        /// 2) When width capped, grow height up to maxHeightRatio * layerHeight.
        /// 3) When both maxed and content still overflows → ellipsis.
        /// </summary>
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

            _messageText.enableWordWrapping = true;
            _messageText.overflowMode = TextOverflowModes.Overflow;

            // Step 1: unconstrained preferred width
            Vector2 prefSingleLine = _messageText.GetPreferredValues(_messageText.text);
            float targetW = Mathf.Min(prefSingleLine.x + _contentPadding.x, maxW);

            // Step 2: preferred height when constrained to targetW
            float textW = Mathf.Max(1f, targetW - _contentPadding.x);
            Vector2 prefAtW = _messageText.GetPreferredValues(_messageText.text, textW, 0f);
            float targetH = Mathf.Min(prefAtW.y + _contentPadding.y, maxH);

            rect.sizeDelta = new Vector2(targetW, targetH);

            // Step 3: ellipsis if both axes maxed and text still overflows
            bool overflows = prefAtW.y + _contentPadding.y > maxH + 0.5f;
            _messageText.overflowMode = overflows ? TextOverflowModes.Ellipsis : TextOverflowModes.Overflow;
        }

        public void SetAutoCloseDuration(float seconds)
        {
            _defaultDuration = seconds;
        }

        public override void Show(Action onComplete = null)
        {
            gameObject.SetActive(true);
            IsVisible = true;
            IsDismissing = false;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            ResetAutoClose();

            if (_toastAnimation != null)
                _toastAnimation.PlayShow(gameObject, () => onComplete?.Invoke());
            else
                onComplete?.Invoke();
        }

        public override void Close(Action onComplete = null)
        {
            if (IsDismissing) { onComplete?.Invoke(); return; }
            IsDismissing = true;
            _autoCloseEnabled = false;

            if (_toastAnimation != null)
            {
                _toastAnimation.PlayClose(gameObject, () =>
                {
                    IsVisible = false;
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
            }
            else
            {
                IsVisible = false;
                gameObject.SetActive(false);
                onComplete?.Invoke();
            }
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

        // Swipe-to-dismiss
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

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (IsDismissing || _rect == null) return;
            _isDragging = false;
            float dist = Mathf.Abs(_rect.anchoredPosition.x - _dragStartPos.x);

            if (dist >= _swipeDismissThreshold)
            {
                // Swipe out fast then close
                float dir = Mathf.Sign(_rect.anchoredPosition.x - _dragStartPos.x);
                float targetX = _dragStartPos.x + dir * Screen.width;
                _rect.DOAnchorPosX(targetX, 0.15f).SetEase(Ease.OutCubic);
                if (_canvasGroup != null) _canvasGroup.DOFade(0f, 0.15f);
                DOVirtual.DelayedCall(0.15f, () => Manager?.HideToast(ToastId));
            }
            else
            {
                // Snap back
                _rect.DOAnchorPos(_dragStartPos, 0.2f).SetEase(Ease.OutCubic);
                if (_canvasGroup != null) _canvasGroup.DOFade(1f, 0.2f);
            }
        }
    }
}
