using System;
using UnityEngine;
using vanhaodev.uimanager.events;

namespace vanhaodev.uimanager.effect
{
    public abstract class LoadingBlockAnimationMonoBase : MonoBehaviour, IUIAnimation
    {
        public string GetPlayShowKey() => "LoadingBlockPlayShow";
        public string GetPlayCloseKey() => "LoadingBlockPlayClose";

        public virtual void PlayShow(GameObject target, Action onComplete)
        {
            Emit(target, GetPlayShowKey());
        }

        public virtual void PlayClose(GameObject target, Action onComplete)
        {
            Emit(target, GetPlayCloseKey());
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
