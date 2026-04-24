using UnityEngine;

namespace Vanhaodev.UIManager
{
    public partial class UIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _screenLayer;
        [SerializeField] private Transform _popupLayer;
        [SerializeField] private UILibrary _library;

        public void ClearCache()
        {
            ClearScreenCache();
            ClearPopupCache();
        }

        public void SetLibrary(UILibrary library)
        {
            _library = library;
        }
    }
}
