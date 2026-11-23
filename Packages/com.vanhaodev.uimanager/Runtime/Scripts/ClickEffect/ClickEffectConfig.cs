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

        [Tooltip("Tick to auto-play the effect on every screen press — mouse, touch or pen, anywhere " +
                 "(including over buttons). Off (default): nothing auto-plays; you can still trigger " +
                 "it manually via PlayClickEffect.")]
        public bool ActiveFeature = false;

        [Tooltip("Minimum seconds between auto-played effects (throttles rapid taps). 0 = no limit. " +
                 "Does not affect manual PlayClickEffect calls.")]
        public float MinInterval = 0.1f;
    }
}
