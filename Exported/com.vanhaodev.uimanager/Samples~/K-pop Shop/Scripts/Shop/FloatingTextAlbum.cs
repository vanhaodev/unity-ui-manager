using UnityEngine;
using UnityEngine.UI;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// Sample floating text for the K-pop shop: shows "[album cover] album name" rising from the
    /// buy button. Demonstrates a custom <see cref="FloatingText"/> that carries its own data
    /// (icon + name) — the library only knows how to spawn, position, animate and pool it.
    /// </summary>
    /// <remarks>
    /// Used from <c>PopupBuyConfirm.OnConfirmClicked</c>:
    /// <code>
    /// Manager.ShowFloatingText&lt;FloatingTextAlbum&gt;(
    ///     t => t.SetAlbum(item.Name, thumbnail.sprite), confirmButton.transform);
    /// </code>
    /// Requires: the prefab registered in the sample's <c>UILibrary</c> (Floating Texts list),
    /// and <c>UIManager._floatingTextLayer</c> assigned.
    /// </remarks>
    public class FloatingTextAlbum : FloatingText
    {
        [SerializeField] private Image _icon;

        // Hard-coded motion for the album float: rises higher and lingers a touch longer than default.
        protected override FloatingTextConfig CreateDefaultConfig() => new()
        {
            RiseDistance = 120f,
            Duration = 1.2f,
            PopFrom = 0.5f,
            PopDuration = 0.18f,
            FadeStartRatio = 0.55f,
        };

        public void SetAlbum(string albumName, Sprite cover)
        {
            // Reuse the base TMP_Text for the name.
            SetText(albumName);

            if (_icon == null) return;
            _icon.sprite = cover;
            _icon.enabled = cover != null;
        }
    }
}
