using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace vanhaodev.uimanager
{
    public abstract class InteractableUI : UIElement
    {
        [SerializeField] protected Button _btnClose;

        protected override void Awake()
        {
            base.Awake();
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
