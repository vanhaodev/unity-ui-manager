using UnityEngine;
using UnityEngine.UI;

namespace vanhaodev.uimanager
{
    public abstract class BasePopup : InteractableUI
    {
        [SerializeField] protected Button _backgroundBtn;
        [SerializeField] protected bool _closeOnBackgroundClick = true;

        public bool CloseOnBackgroundClick => _closeOnBackgroundClick;

        protected override void Awake()
        {
            base.Awake();
            if (_backgroundBtn != null && _closeOnBackgroundClick)
                _backgroundBtn.onClick.AddListener(OnBackgroundClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_backgroundBtn != null)
                _backgroundBtn.onClick.RemoveListener(OnBackgroundClicked);
        }

        protected override void OnCloseClicked()
        {
            Manager?.ClosePopup(this);
        }

        protected virtual void OnBackgroundClicked()
        {
            Manager?.ClosePopup(this);
        }

        protected override void OnShowEnd()
        {
            OnPopupOpened();
        }

        protected override void OnCloseEnd()
        {
            OnPopupClosed();
        }

        public virtual void OnPopupOpened() { }

        public virtual void OnPopupClosed() { }
    }
}
