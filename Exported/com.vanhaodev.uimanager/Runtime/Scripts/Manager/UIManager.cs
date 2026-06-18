using UnityEngine;

namespace vanhaodev.uimanager
{
    public partial class UIManager : MonoBehaviour
    {
        [SerializeField] private UILibrary _library;

        public UILibrary Library => _library;

        public void ClearCache()
        {
            ClearScreenCache();
            ClearPopupCache();
            ClearToastCache();
            ClearLoadingBlockCache();
            ClearFloatingTextCache();
            ClearFlyoutCache();
        }

        public void SetLibrary(UILibrary library)
        {
            _library = library;
        }
    }
}
