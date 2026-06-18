using UnityEngine;
using UnityEngine.UI;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Pooled flyout icon element. Spawned, animated, then returned to pool.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Image))]
    public class FlyoutIcon : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Image _image;

        public RectTransform RectTransform => _rectTransform;
        public CanvasGroup CanvasGroup => _canvasGroup;
        public Image Image => _image;

        // Set by FlyoutEffect when spawning
        internal FlyoutTarget Target { get; set; }
        internal int ValuePerIcon { get; set; }
        internal Vector2 SourcePosition { get; set; }
        internal int Index { get; set; }
        internal int TotalCount { get; set; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _image = GetComponent<Image>();

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        /// <summary>
        /// Reset state when retrieved from pool.
        /// </summary>
        public void ResetState()
        {
            _rectTransform.localScale = Vector3.zero; // Start hidden, BurstAsync will animate in
            _rectTransform.localRotation = Quaternion.identity;
            _canvasGroup.alpha = 1f;
            Target = null;
            ValuePerIcon = 0;
            Index = 0;
            TotalCount = 0;
        }

        /// <summary>
        /// Set the icon sprite. Size is preserved from prefab.
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            _image.sprite = sprite;
        }

        /// <summary>
        /// Called when icon reaches target.
        /// </summary>
        internal void OnReachTarget()
        {
            Target?.NotifyIconArrived(ValuePerIcon);
        }
    }
}
