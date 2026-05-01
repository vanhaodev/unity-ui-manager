namespace vanhaodev.uimanager
{
    public abstract class BaseScreen : InteractableUI
    {
        protected override void OnCloseClicked()
        {
            Manager?.CloseScreen();
        }

        protected override void OnCloseStart()
        {
            OnExit();
        }

        public virtual void OnEnter() { }

        public virtual void OnExit() { }
    }
}
