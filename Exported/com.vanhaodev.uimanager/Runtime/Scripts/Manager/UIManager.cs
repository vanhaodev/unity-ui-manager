using UnityEngine;

namespace vanhaodev.uimanager
{
    public partial class UIManager : MonoBehaviour
    {
        [SerializeField] private UILibrary _library;

        public void ClearCache()
        {
            ClearScreenCache();
            ClearPopupCache();
            ClearToastCache();
            ClearLoadingBlockCache();
        }

        public void SetLibrary(UILibrary library)
        {
            _library = library;
        }
    }
}
