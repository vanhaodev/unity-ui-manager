using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace vanhaodev.uimanager
{
    public abstract class InteractableUI : UIElement
    {
        [SerializeField] protected Button _btnClose;

        protected virtual void Awake()
        {
            _btnClose?.onClick.AddListener(OnCloseClicked);
        }

        protected virtual void OnDestroy()
        {
            _btnClose?.onClick.RemoveListener(OnCloseClicked);
        }

        protected virtual void OnCloseClicked()
        {
            Close();
        }
    }
}
