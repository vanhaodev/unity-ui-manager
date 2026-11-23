using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace vanhaodev.uimanager
{
    /// <summary>
    /// Add this component to make any UI element draggable.
    /// Optional: assign _dragHandle to limit drag initiation to a specific area.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIDraggable : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _dragIcon;
        [SerializeField] private RectTransform _dragHandle;
        [SerializeField] private float _dragAlpha = 0.3f;

        public bool IsDragging { get; private set; }
        public bool CanDrag { get; set; } = true;

        public event Action OnDragStart;
        public event Action OnDragEnd;

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private DragCloneManager _clone;
        private float _originalAlpha;
        private ScrollRect _parentScroll;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _clone = new DragCloneManager(this);
            _parentScroll = GetComponentInParent<ScrollRect>();

            if (_dragIcon == null)
                _dragIcon = GetComponentInChildren<Image>();
        }

        private void OnDestroy()
        {
            _clone?.Destroy();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            bool onHandle = IsPointerOnHandle(eventData);

            if (!CanDrag || _dragIcon == null || !onHandle)
            {
                // Transfer drag ownership to ScrollRect
                if (_parentScroll != null)
                {
                    eventData.pointerDrag = _parentScroll.gameObject;
                    ExecuteEvents.Execute(_parentScroll.gameObject, eventData, ExecuteEvents.beginDragHandler);
                }
                return;
            }

            IsDragging = true;
            _originalAlpha = _canvasGroup.alpha;
            _clone.Create(_dragIcon, _canvas, eventData.position);
            _canvasGroup.alpha = _dragAlpha;
            OnDragStart?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging) return;
            _clone.Move(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDragging) return;

            IsDragging = false;
            _clone.Destroy();
            _canvasGroup.alpha = _originalAlpha;
            OnDragEnd?.Invoke();
        }

        private bool IsPointerOnHandle(PointerEventData eventData)
        {
            if (_dragHandle == null) return true;

            Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera;

            return RectTransformUtility.RectangleContainsScreenPoint(
                _dragHandle, eventData.pressPosition, cam);
        }

        public void ForceCleanup()
        {
            if (!IsDragging) return;
            IsDragging = false;
            _clone?.Destroy();
            _canvasGroup.alpha = _originalAlpha;
        }
    }
}
