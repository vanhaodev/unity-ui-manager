using System.Collections.Generic;
using UnityEngine;
using vanhaodev.uimanager.events;

namespace vanhaodev.uimanager.samples.kpopshop
{
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
}