using UnityEngine;
using vanhaodev.uimanager.events;

namespace vanhaodev.uimanager.effect
{
    public abstract class ButtonAnimationBase : MonoBehaviour, IButtonAnimation
    {
        public virtual void OnPointerEnter(GameObject target)
        {
            Emit(target, GetPointerEnterKey());
        }

        protected virtual string GetPointerEnterKey() => "PointerEnter";

        public virtual void OnPointerExit(GameObject target)
        {
            Emit(target, GetPointerExitKey());
        }

        protected virtual string GetPointerExitKey() => "PointerExit";

        public virtual void OnPointerDown(GameObject target)
        {
            Emit(target, GetPointerDownKey());
        }

        protected virtual string GetPointerDownKey() => "PointerDown";

        public virtual void OnPointerUp(GameObject target)
        {
            Emit(target, GetPointerUpKey());
        }

        protected virtual string GetPointerUpKey() => "PointerUp";

        public virtual void OnClick(GameObject target)
        {
            Emit(target, GetClickKey());
        }

        protected virtual string GetClickKey() => "Click";

        protected virtual void Emit(GameObject target, string key)
        {
            OnUIEvent(new UIEventData
            {
                Key = key,
                Target = target
            });
        }

        public virtual void OnUIEvent(UIEventData e)
        {
            UIEvent.OnEvent(e);
        }
    }
}