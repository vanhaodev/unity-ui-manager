using System;
using UnityEngine;
using vanhaodev.uimanager.events;

namespace vanhaodev.uimanager.effect
{
    public abstract class UIAnimationBase : IUIAnimation
    {
        public string GetPlayShowKey()
        {
            return "PlayShow";
        }

        public virtual void PlayShow(GameObject target, Action onComplete)
        {
            Emit(target, GetPlayShowKey());
        }

        public string GetPlayPlayClose()
        {
            return "PlayClose";
        }

        public virtual void PlayClose(GameObject target, Action onComplete)
        {
            Emit(target, GetPlayPlayClose());
        }

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