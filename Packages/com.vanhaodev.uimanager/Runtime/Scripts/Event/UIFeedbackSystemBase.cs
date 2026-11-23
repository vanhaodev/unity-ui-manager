using UnityEngine;

namespace vanhaodev.uimanager.events
{
    public abstract class UIFeedbackSystemBase : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            UIEvent.OnEvent += HandleInternal;
        }

        protected virtual void OnDisable()
        {
            UIEvent.OnEvent -= HandleInternal;
        }

        private void HandleInternal(UIEventData data)
        {
            OnUIEvent(data);
        }

        protected abstract void OnUIEvent(UIEventData data);
    }
}
/* EXAMPLE
public class UIFeedbackSystem : UIFeedbackSystemBase
{
    protected override void OnUIEvent(UIEventData data)
    {
        var soundMng = FindFirstObjectByType<SoundManager>();
        switch (data.Key)
        {
            case "PointerEnter":
            case "PointerExit":
            case "PointerDown":
            case "PointerUp":
            case "Click":
            case "PlayShow":
            case "PlayClose":
            {
                soundMng.Play(data.Key);
                break;
            }
        }
    }
}
 */