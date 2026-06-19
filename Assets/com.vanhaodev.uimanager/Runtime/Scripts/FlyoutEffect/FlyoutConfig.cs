using System;
using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Configuration for flyout effect animations. Set in UILibrary.
    /// </summary>
    [Serializable]
    public class FlyoutConfig
    {
        [Header("Icon Count")]
        [Tooltip("1 to this value: show exact count of icons")]
        public int DirectCountMax = 10;

        [Tooltip("Above DirectCountMax: how many extra icons per 10 units")]
        public int ScaledCountPer10 = 2;

        [Tooltip("Maximum icons regardless of amount")]
        public int MaxIcons = 28;

        [Header("Timing")]
        [Tooltip("Duration of burst scatter animation")]
        public float BurstDuration = 0.15f;

        [Tooltip("Duration of flight to target")]
        public float FlightDuration = 0.5f;

        [Tooltip("Duration of impact fade out")]
        public float ImpactDuration = 0.1f;

        [Tooltip("Delay between each icon spawn")]
        public float SpawnStagger = 0.02f;

        [Header("Burst")]
        [Tooltip("Scatter radius from source position")]
        public float BurstRadius = 80f;

        [Tooltip("Randomness factor for burst (0-1)")]
        [Range(0f, 1f)]
        public float BurstRandomness = 0.3f;

        [Header("Flight")]
        [Tooltip("Height of the bezier curve arc")]
        public float FlightCurveHeight = 100f;

        [Tooltip("Randomness factor for flight path (0-1)")]
        [Range(0f, 1f)]
        public float FlightRandomness = 0.2f;

        [Header("Reaction")]
        [Tooltip("Shake intensity when icon hits target")]
        public float ShakeIntensity = 5f;

        [Tooltip("Duration of shake animation")]
        public float ShakeDuration = 0.1f;

        [Header("Target Aim")]
        [Tooltip("Aim icons at a text edge chosen from the amount text's horizontal alignment " +
                 "(Left → tail, Right → head, Centre → tail) instead of the text centre. " +
                 "Off (default): icons fly into the centre of the number.")]
        public bool AimByAlignment = false;

        [Tooltip("When AimByAlignment is on, outward gap from the chosen text edge as a fraction of " +
                 "the text height, so icons land just past the digits instead of overlapping them. " +
                 "0 = exactly on the edge.")]
        public float AimEdgeGap = 0f;
    }
}
