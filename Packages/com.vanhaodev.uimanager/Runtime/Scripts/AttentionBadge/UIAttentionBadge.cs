using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// "Something here is waiting for you" marker — a notification dot (a claimable reward, an unread message):
    /// an icon that wobbles now and then and bubbles (swells and shrinks back, over and over) while it is shown.
    /// Put it on its own GameObject (art = an Image on it or its children) and call <see cref="SetVisible"/> — the
    /// object switches itself on/off, and the motion runs only while it is on. Wobble = rotation, bubble = scale,
    /// so they never fight.
    ///
    /// <para>When a dot should show is the app's call (a quest claimable, a feature not yet opened this
    /// session); the badge only shows, hides and moves.</para>
    ///
    /// <para>Unscaled time: it sits on popups that open over a paused game too.</para>
    /// </summary>
    public class UIAttentionBadge : MonoBehaviour
    {
        // Swings per wobble — how many times the icon tips back and forth before it settles.
        private const float WobbleSwings = 4f;

        [Header("Wobble")]
        [Tooltip("Peak tilt of one wobble, in degrees.")]
        [SerializeField] private float _angle = 15f;
        [Tooltip("How long one wobble lasts.")]
        [SerializeField] private float _wobbleDuration = 0.5f;
        [Tooltip("Rest between two wobbles. 0 = wobble without pause.")]
        [SerializeField] private float _restDuration = 1.2f;

        [Header("Bubble")]
        [Tooltip("Off = no bubble, the badge keeps its size.")]
        [SerializeField] private bool _bubble = true;
        [Tooltip("Size at the top of a swell, relative to the badge's own (1 = no swell).")]
        [SerializeField] private float _bubbleScale = 1.15f;
        [Tooltip("One swell or one shrink. A full bubble is twice this.")]
        [SerializeField] private float _bubbleDuration = 0.45f;

        private float _time;

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }

        private void OnEnable()
        {
            _time = 0f;
            ResetPose();
        }

        private void OnDisable() => ResetPose();

        private void Update()
        {
            _time += Time.unscaledDeltaTime;
            transform.localRotation = Quaternion.Euler(0f, 0f, WobbleAngle(_time));
            transform.localScale = Vector3.one * BubbleScale(_time);
        }

        // A wobble that dies out over its duration (a punch: full tilt at the start, still at the end), then a
        // rest, on repeat.
        private float WobbleAngle(float time)
        {
            if (_wobbleDuration <= 0f) return 0f;

            float cycle = _wobbleDuration + Mathf.Max(0f, _restDuration);
            float t = time % cycle;
            if (t >= _wobbleDuration) return 0f;

            float progress = t / _wobbleDuration;
            return _angle * (1f - progress) * Mathf.Sin(progress * WobbleSwings * 2f * Mathf.PI);
        }

        // 1 → peak → 1, eased in and out, over and over: one swell plus one shrink is a full cosine period.
        private float BubbleScale(float time)
        {
            if (!_bubble || _bubbleDuration <= 0f) return 1f;

            float swell = 0.5f - 0.5f * Mathf.Cos(time / _bubbleDuration * Mathf.PI);
            return Mathf.LerpUnclamped(1f, _bubbleScale, swell);
        }

        private void ResetPose()
        {
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}
