using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// What the intro looks like, authored as a prefab: an overlay Canvas (with the game's CanvasScaler) holding a
    /// logo laid out however the screen needs it. UI only — no timing, no order of events;
    /// <see cref="IntroSequence"/> drives it.
    ///
    /// <para>The logo's size and anchors are the prefab's. The intro only ever animates its SCALE (and the
    /// group's alpha), so a responsive layout stays exactly as authored.</para>
    ///
    /// <para>It can also sit inside another canvas (no Canvas of its own) as a logo some other screen shows and
    /// hides — driven straight through an <see cref="IIntroAnimation"/>, without a sequence.</para>
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class IntroView : MonoBehaviour
    {
        [Tooltip("The logo that pops in. Lay it out freely — only its scale is animated.")]
        [SerializeField] private RectTransform _logo;

        private CanvasGroup _group;

        /// <summary>The logo, at scale 0 until something grows it.</summary>
        public RectTransform Logo => _logo;

        /// <summary>The whole intro, for fading it.</summary>
        public CanvasGroup Group => _group != null ? _group : _group = GetComponent<CanvasGroup>();

        /// <summary>
        /// Put <paramref name="prefab"/> on screen, logo hidden (scale 0).
        /// </summary>
        /// <param name="sortingOrder">Where the intro draws among the game's canvases, overriding the prefab's
        /// (the prefab is its own overlay Canvas when created this way).</param>
        public static IntroView Create(IntroView prefab, int sortingOrder)
        {
            var view = Instantiate(prefab);
            DontDestroyOnLoad(view.gameObject); // a scene change during boot must not cut the intro off mid-pop

            if (view.TryGetComponent(out Canvas canvas)) canvas.sortingOrder = sortingOrder;
            view.Group.interactable = false;
            view.Group.blocksRaycasts = false; // an intro never swallows a tap meant for what is under it
            view.SetLogoScale(0f);
            return view;
        }

        public void SetLogoScale(float scale) => _logo.localScale = Vector3.one * scale;

        public void SetAlpha(float alpha) => Group.alpha = alpha;

        public void Close()
        {
            if (this != null) Destroy(gameObject);
        }
    }
}
