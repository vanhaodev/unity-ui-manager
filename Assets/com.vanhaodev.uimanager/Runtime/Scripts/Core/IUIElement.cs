using System;

namespace Vanhaodev.UIManager
{
    public interface IUIElement
    {
        string Id { get; }
        bool IsVisible { get; }
        void Show(Action onComplete = null);
        void Hide(Action onComplete = null);
    }
}
