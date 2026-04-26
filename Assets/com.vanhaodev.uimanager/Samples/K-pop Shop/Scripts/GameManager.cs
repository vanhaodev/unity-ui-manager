using UnityEngine;
using vanhaodev.uimanager;

namespace vanhaodev.uimanager.samples.kpopshop
{
    public class GameUI : MonoBehaviour
    {
        private UIManager _uiManager;

        private void Start()
        {
            _uiManager = FindObjectOfType<UIManager>();
            ShowHomeScreen();
        }

        public void ShowHomeScreen()
        {
            _uiManager?.ShowScreen<ScreenHome>();
        }
    }
}
