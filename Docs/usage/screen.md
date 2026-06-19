---
description: Full-screen pages, with only one shown at a time.
---

# Screen

A **screen** is a full page of your game — Home, Shop, Settings, and so on. UI Manager shows one
screen at a time: opening a new screen automatically closes the current one (playing its close
animation first, then the new screen's open animation).

## Make a screen

{% stepper %}
{% step %}
### Create the script

Make a class that inherits from `BaseScreen` and build your page on the prefab as usual.

```csharp
public class HomeScreen : BaseScreen
{
    [SerializeField] private TMP_Text _coinLabel;

    public override void OnEnter()   // called right before the screen shows
    {
        Refresh();
    }

    public override void Refresh()   // redraw from your data
    {
        _coinLabel.text = Wallet.Coins.ToString();
    }

    public override void OnExit() { }  // called when the screen starts closing
}
```
{% endstep %}

{% step %}
### Register the prefab

Add the screen prefab to the **Screens** list on your UILibrary. That's all the wiring needed — no
manual references.
{% endstep %}
{% endstepper %}

## Show and close

```csharp
ui.ShowScreen<HomeScreen>();                 // switch to Home
ui.ShowScreen<ShopScreen>(s => s.SetTab(0)); // configure it before it appears
ui.CloseScreen();                            // close the current screen
```

`ShowScreen<T>` returns the screen instance, and the optional setup callback runs **before** the
open animation — the perfect place to pass in data so the page is ready when it appears.

## Lifecycle hooks

Override only what you need:

| Hook | When it runs |
| --- | --- |
| `OnEnter()` | Just before the screen shows. Good for loading/refreshing data. |
| `OnExit()` | When the screen starts closing. |
| `Refresh()` | Whenever you call it to redraw from data. |

{% hint style="info" %}
Add a **Close button** by assigning it to the `_btnClose` field on the prefab — UI Manager hooks it
up to close the screen for you.
{% endhint %}

## Handy members

| Member | Description |
| --- | --- |
| `ui.ShowScreen<T>(setup, onComplete)` | Show a screen (closing the current one). |
| `ui.CloseScreen(onComplete)` | Close whatever screen is showing. |
| `ui.GetScreen<T>()` | Get a cached screen instance (or `null`). |
| `ui.CurrentScreen` | The screen currently shown. |
| `ui.OnScreenChanged` | Event `(from, to)` fired whenever the screen switches. |
