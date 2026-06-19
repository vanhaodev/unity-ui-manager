---
description: How UI Manager is put together, and the one-time setup every feature shares.
---

# UI

UI Manager gives you a single place to show and hide your game's interface — screens, popups,
toasts, loading blocks and effects — without wiring each one up by hand. You ask the manager for a
piece of UI by its type, and it takes care of creating it, caching it, animating it in and out, and
cleaning it up.

## The two pieces you set up once

{% stepper %}
{% step %}
### UILibrary — your list of prefabs

Create one via **Assets → Create → UI Manager → Library**. It's a single asset that holds **all**
your UI prefabs, grouped by kind:

* **Screens** — full pages (Home, Shop, Settings…).
* **Popups** — dialogs that open on top of a screen.
* **Toasts** — small messages that appear and fade.
* **Loading Blocks** — the "please wait" overlay.
* **Floating Texts** — little labels that pop and float (e.g. "+100").

Drag each prefab into the matching list. UI Manager finds the right prefab by its **type**, so you
never reference prefabs from code.
{% endstep %}

{% step %}
### UIManager — the thing you call

Put a **UIManager** in your scene and assign your **UILibrary** to it. The UIManager also has a
**layer** slot for each kind of UI (Screen Layer, Popup Layer, Toast Layer, etc.) — these are just
`Transform`s on your canvas that decide the draw order. Higher in the canvas = drawn on top, so a
typical order is: screens at the bottom, then popups, toasts, loading block, and effects on top.
{% endstep %}
{% endstepper %}

## How you use it

From anywhere in your game, grab the manager and ask for UI by type:

```csharp
var ui = FindFirstObjectByType<UIManager>();

ui.ShowScreen<HomeScreen>();        // switch to a page
ui.ShowPopup<ConfirmPopup>();       // open a dialog
ui.ShowToast<ToastDefault>();       // show a message
```

That's the whole idea — **one call per piece of UI**. Each feature below builds on this:

* [Screen](screen.md) — full pages, one shown at a time.
* [Popup](popup.md) — stacking dialogs over the current screen.
* [Toast](toast.md) — short auto-dismissing messages.
* [Loading Block](loading-block.md) — block input while something runs.
* [Floating Text](floating-text.md) — pop-and-float labels.
* [Flyout Effect](flyout.md) — icons that fly into a counter.

{% hint style="info" %}
**Prefabs are reused, not re-created.** When you show a screen or popup the first time, UI Manager
instantiates it and keeps it cached, so the next show is instant. Call `ClearCache()` on the manager
(e.g. between scenes) if you want to free everything.
{% endhint %}

## Refreshing what's on screen

Every screen, popup and toast can override `Refresh()`. Put your "redraw from current data" code
there, and call it whenever your data changes:

```csharp
public override void Refresh()
{
    _coinLabel.text = wallet.Coins.ToString();
}
```

This keeps a clean split: your game changes the data, then asks the UI to redraw — the UI never owns
the data itself.
