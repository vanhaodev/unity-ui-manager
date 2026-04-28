using System;
using UnityEngine;

namespace vanhaodev.uimanager.effect
{
    public interface IToastAnimation
    {
        void PlayShow(GameObject target, Action onComplete);
        void PlayClose(GameObject target, Action onComplete);
    }
}
