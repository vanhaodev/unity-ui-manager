namespace Vanhaodev.UIManager
{
    public interface IPopup : IUIElement
    {
        int Priority { get; }
        bool CloseOnBackgroundClick { get; }
        void OnPopupOpened();
        void OnPopupClosed();
    }
}
