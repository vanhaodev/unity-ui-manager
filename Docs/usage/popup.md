---
description: Dialogs that stack on top of the current screen.
---

# Popup

A **popup** is a dialog that opens over your current screen — a confirm box, a reward window, a
settings panel. Unlike screens, **many popups can be open at once**: they stack, and the newest one
sits on top.

## Make a popup

{% stepper %}
{% step %}
### Create the script

Inherit from `BasePopup`.

```csharp
public class ConfirmPopup : BasePopup
{
    [SerializeField] private TMP_Text _message;
    [SerializeField] private Button _yesButton;

    public void Setup(string message, Action onYes)
    {
        _message.text = message;
        _yesButton.onClick.AddListener(() => { onYes(); Manager.ClosePopup(this); });
    }

    public override void OnPopupOpened() { }  // after the open animation finishes
    public override void OnPopupClosed() { }  // after the close animation finishes
}
```
{% endstep %}

{% step %}
### Register the prefab

Add the popup prefab to the **Popups** list on your UILibrary.
{% endstep %}
{% endstepper %}

## Show and close

```csharp
ui.ShowPopup<ConfirmPopup>(p => p.Setup("Buy this item?", Buy));

ui.ClosePopup<ConfirmPopup>();  // close the first popup of this type
ui.CloseTopPopup();             // close the topmost popup
ui.CloseAllPopups();            // close everything
```

The setup callback runs **before** the open animation, so the popup is filled in by the time the
player sees it.

## Closing by tapping outside

Popups have a background button built in. On the prefab:

* Assign the full-screen background to the **`_backgroundBtn`** field.
* Tick **`_closeOnBackgroundClick`** (on by default) to let players dismiss by tapping outside.

You can also assign a **Close button** to `_btnClose`, just like screens.

## Lifecycle hooks

| Hook | When it runs |
| --- | --- |
| `OnPopupOpened()` | After the open animation finishes. |
| `OnPopupClosed()` | After the close animation finishes. |
| `Refresh()` | Whenever you call it to redraw from data. |

## Handy members

| Member | Description |
| --- | --- |
| `ui.ShowPopup<T>(setup, onComplete)` | Open a popup on top of the stack. |
| `ui.ClosePopup<T>()` / `ClosePopup(popup)` | Close a specific popup. |
| `ui.CloseTopPopup()` | Close the topmost popup. |
| `ui.CloseAllPopups()` / `CloseAllPopups<T>()` | Close all popups (optionally of one type). |
| `ui.IsPopupActive<T>()` | Is a popup of this type open? |
| `ui.TopPopup` / `ui.HasActivePopup` | The topmost popup / whether any is open. |
| `ui.OnPopupOpened` / `ui.OnPopupClosed` | Events fired as popups open and close. |

{% hint style="info" %}
Open the **same** popup type twice and the older one stays visually on top, so a freshly opened
dialog never hides one you were already reading.
{% endhint %}
