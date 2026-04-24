using System;

namespace Vanhaodev.UIManager
{
    public abstract class BaseScreen : InteractableUI
    {
        protected override void OnCloseClicked()
        {
            Manager?.CloseScreen();
        }

        public virtual void OnEnter(object data = null) { }

        public virtual void OnExit() { }

        public override void Show(Action onComplete = null)
        {
            gameObject.SetActive(true);
            IsVisible = true;
            OnShowAnimation(() => onComplete?.Invoke());
        }

        public override void Close(Action onComplete = null)
        {
            OnExit();
            OnCloseAnimation(() =>
            {
                IsVisible = false;
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }
    }
}
