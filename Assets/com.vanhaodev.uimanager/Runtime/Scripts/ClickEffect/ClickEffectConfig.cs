using System;
using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Configuration for the click effect. Set in UILibrary. Nothing is created until the effect is
    /// actually played, so leaving the prefab empty (or auto-play off and never calling it) costs no memory.
    /// </summary>
    [Serializable]
    public class ClickEffectConfig
    {
        [Tooltip("Effect spawned at each click/touch point (a BaseClickEffect, e.g. ClickEffectRipple). " +
                 "Leave null to disable the feature entirely.")]
        public BaseClickEffect Prefab;

        [Tooltip("Automatically play the effect on every screen press — mouse, touch or pen, on any " +
                 "platform, anywhere on screen (including over buttons). Off: nothing runs until your " +
                 "app calls PlayClickEffect itself.")]
        public bool AutoPlay = false;

        [Tooltip("Minimum seconds between auto-played effects (throttles rapid taps). 0 = no limit. " +
                 "Does not affect manual PlayClickEffect calls.")]
        public float MinInterval = 0.4f;
    }
}
