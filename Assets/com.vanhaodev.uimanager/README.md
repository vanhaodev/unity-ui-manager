# UI Manager

Simple and lightweight UI management system for Unity.

## Features

- **Screen Management**: Full-screen views with switch animations
- **Popup Management**: Stackable popups with priority ordering
- **Smooth Animations**: Built-in DOTween animations (customizable)
- **Easy to Use**: Simple API with generic type support
- **Reusable**: Copy to any project

## Installation

1. Copy `com.vanhaodev.uimanager` folder to your `Assets/` directory
2. Make sure DOTween is installed
3. Create a `UILibrary` asset: `Create > Vanhaodev > UI > Library`
4. Add `UIManager` prefab to your scene

## Quick Start

### 1. Create a Screen

```csharp
using Vanhaodev.UIManager;

public class ScreenHome : BaseScreen
{
    public override void OnEnter(object data = null)
    {
        // Called when screen becomes active
    }

    public override void OnExit()
    {
        // Called when leaving this screen
    }
}
```

### 2. Create a Popup

```csharp
using Vanhaodev.UIManager;

public class PopupSettings : BasePopup
{
    public override void OnPopupOpened()
    {
        // Called when popup is fully shown
    }

    public override void OnPopupClosed()
    {
        // Called when popup is closed
    }
}
```

### 3. Usage

```csharp
// Show a screen
UIManager.Instance.ShowScreen<ScreenHome>();

// Show a screen with data
UIManager.Instance.ShowScreen<ScreenGame>(new GameData { level = 1 });

// Show a popup
UIManager.Instance.ShowPopup<PopupSettings>();

// Close popup
UIManager.Instance.ClosePopup<PopupSettings>();
UIManager.Instance.CloseTopPopup();
UIManager.Instance.CloseAllPopups();

// Check state
if (UIManager.Instance.HasActivePopup) { }
if (UIManager.Instance.IsPopupActive<PopupSettings>()) { }
```

## Setup UIManager Prefab

```
UIManager (GameObject)
├── ScreenLayer (empty, for screens)
└── PopupLayer (empty, for popups)
```

Assign:
- `_screenLayer`: ScreenLayer transform
- `_popupLayer`: PopupLayer transform  
- `_library`: Your UILibrary asset

## Popup Priority

Higher priority = renders on top.

```csharp
public class PopupNotice : BasePopup
{
    // Set in Inspector or override
    // _priority = 100 (high priority, always on top)
}
```

## Custom Animations

Override animation methods:

```csharp
public class PopupCustom : BasePopup
{
    protected override void PlayShowAnimation(Action onComplete)
    {
        // Your custom show animation
        _canvasGroup.DOFade(1f, 0.5f).OnComplete(() => onComplete?.Invoke());
    }

    protected override void PlayHideAnimation(Action onComplete)
    {
        // Your custom hide animation
        _canvasGroup.DOFade(0f, 0.3f).OnComplete(() => onComplete?.Invoke());
    }
}
```

## Events

```csharp
UIManager.Instance.OnScreenChanged += (oldScreen, newScreen) => { };
UIManager.Instance.OnPopupOpened += (popup) => { };
UIManager.Instance.OnPopupClosed += (popup) => { };
```

## License

MIT License
