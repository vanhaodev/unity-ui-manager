namespace Vanhaodev.UIManager
{
    public interface IScreen : IUIElement
    {
        void OnEnter(object data = null);
        void OnExit();
    }
}
