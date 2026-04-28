using UnityEngine;

namespace vanhaodev.uimanager
{
    public partial class UIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _screenLayer;
        [SerializeField] private Transform _popupLayer;
        [SerializeField] private Transform _toastLayer;
        [SerializeField] private UILibrary _library;

        public void ClearCache()
        {
            ClearScreenCache();
            ClearPopupCache();
            ClearToastCache();
        }

        public void SetLibrary(UILibrary library)
        {
            _library = library;
        }
    }
}
