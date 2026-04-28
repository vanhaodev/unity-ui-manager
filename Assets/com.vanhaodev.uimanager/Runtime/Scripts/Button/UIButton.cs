using UnityEngine;
using UnityEngine.EventSystems;

namespace vanhaodev.uimanager.effect
{
    public class UIButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler
    {
        private IButtonAnimation _anim;

        private void Awake()
        {
            _anim = GetComponent<IButtonAnimation>();

            if (_anim == null)
            {
                Debug.LogWarning($"No IButtonAnimation on {name}");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
            => _anim?.OnPointerEnter(gameObject);

        public void OnPointerExit(PointerEventData eventData)
            => _anim?.OnPointerExit(gameObject);

        public void OnPointerDown(PointerEventData eventData)
            => _anim?.OnPointerDown(gameObject);

        public void OnPointerUp(PointerEventData eventData)
            => _anim?.OnPointerUp(gameObject);

        public void OnPointerClick(PointerEventData eventData)
            => _anim?.OnClick(gameObject);
    }
}