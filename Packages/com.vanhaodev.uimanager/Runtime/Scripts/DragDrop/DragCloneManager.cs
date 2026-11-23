using UnityEngine;
using UnityEngine.UI;
namespace vanhaodev.uimanager
{
    /// <summary>
    /// Manages a drag clone for a specific owner.
    /// Each draggable object should have its own instance.
    /// Clone is auto-destroyed when owner is destroyed.
    /// </summary>
    public class DragCloneManager
    {
        private GameObject _clone;
        private readonly MonoBehaviour _owner;

        public bool HasClone => _clone != null;

        public DragCloneManager(MonoBehaviour owner)
        {
            _owner = owner;
        }

        public void Create(Image sourceIcon, Canvas canvas, Vector2 position)
        {
            Destroy();

            if (sourceIcon == null || canvas == null) return;

            _clone = new GameObject("DragClone");
            _clone.transform.SetParent(canvas.transform);
            _clone.transform.SetAsLastSibling();

            var image = _clone.AddComponent<Image>();
            image.sprite = sourceIcon.sprite;
            image.preserveAspect = sourceIcon.preserveAspect; // keep the icon's aspect so the clone isn't stretched
            image.raycastTarget = false;

            var rect = _clone.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(
                sourceIcon.rectTransform.rect.width,
                sourceIcon.rectTransform.rect.height
            );
            rect.position = position;
        }

        public void Move(Vector2 position)
        {
            if (_clone != null)
                _clone.transform.position = position;
        }

        public void Destroy()
        {
            if (_clone != null)
            {
                Object.Destroy(_clone);
                _clone = null;
            }
        }
    }
}
