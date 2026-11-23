using UnityEngine;
using vanhaodev.uimanager;

namespace vanhaodev.uimanager.samples.kpopshop
{
    public class GameUI : MonoBehaviour
    {
        private UIManager _uiManager;

        private async void Start()
        {
            _uiManager = FindFirstObjectByType<UIManager>();
            if (_uiManager == null) return;

            // The home screen opens under the intro, so the shop is ready the moment the logo leaves. A real game
            // passes its boot work instead: await _uiManager.PlayIntro(LoadGameAsync);
            ShowHomeScreen();
            await _uiManager.PlayIntro();
        }

        public void ShowHomeScreen()
        {
            _uiManager?.ShowScreen<ScreenHome>();
        }
    }
}
