using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Stretches a top and a bottom bar to cover the strips OUTSIDE the safe area (notch / home indicator),
    /// leaving the middle untouched.
    ///
    /// Why bars instead of one full-screen background: on a Screen Space - Overlay canvas a full-screen image
    /// would also cover the world behind the UI (a 3D scene). Two edge bars hide only what bleeds into the unsafe
    /// strips and keep the world visible in between. Portrait only: the side strips a landscape notch produces
    /// are not covered.
    ///
    /// Usage: put this on a full-stretch RectTransform that is a direct child of the Canvas, as a SIBLING of
    /// the SafeArea node (not inside it) — inside, it would be clipped to the very area it must cover. Author
    /// the two bars as children with whatever art they need; only their anchors are driven from here, so
    /// anything parented inside a bar rides along.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaBars : MonoBehaviour
    {
        [SerializeField] private RectTransform _top;
        [SerializeField] private RectTransform _bottom;

        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake() => Refresh();

        // Screen.safeArea can change without a resolution change (rotation, gesture bar toggling), so poll it,
        // but only rewrite the anchors when something actually moved.
        private void Update() => Refresh();

        private void Refresh()
        {
            var safeArea = Screen.safeArea;
            if (safeArea == _lastSafeArea
                && Screen.width == _lastScreenSize.x
                && Screen.height == _lastScreenSize.y)
                return;

            if (Screen.width <= 0 || Screen.height <= 0) return;

            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            // Safe area in normalised screen coordinates — the same space RectTransform anchors live in.
            float yMin = safeArea.yMin / Screen.height;
            float yMax = safeArea.yMax / Screen.height;

            SetBar(_top, new Vector2(0f, yMax), Vector2.one);
            SetBar(_bottom, Vector2.zero, new Vector2(1f, yMin));
        }

        private static void SetBar(RectTransform bar, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (bar == null) return;

            // A zero-height bar (no notch on this edge) is switched off rather than left as a degenerate rect.
            bool visible = anchorMax.y > anchorMin.y;
            bar.gameObject.SetActive(visible);
            if (!visible) return;

            bar.anchorMin = anchorMin;
            bar.anchorMax = anchorMax;
            bar.offsetMin = Vector2.zero;
            bar.offsetMax = Vector2.zero;
        }
    }
}
