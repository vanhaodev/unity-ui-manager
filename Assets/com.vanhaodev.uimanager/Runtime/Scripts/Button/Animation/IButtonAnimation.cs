using UnityEngine;

namespace vanhaodev.uimanager.effect
{
    public interface IButtonAnimation
    {
        void OnPointerEnter(GameObject target);
        void OnPointerExit(GameObject target);

        void OnPointerDown(GameObject target);
        void OnPointerUp(GameObject target);

        void OnClick(GameObject target);
    }
}