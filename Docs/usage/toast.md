---
description: Short messages that appear, stack, and fade on their own.
---

# Toast

A **toast** is a small message that slides in, waits a moment, then dismisses itself — "Saved!",
"No internet", "+1 life". You can place them at any edge or corner, they stack neatly, and players
can swipe them away.

## Show a toast

The built-in `ToastDefault` covers most needs:

```csharp
ui.ShowToast<ToastDefault>(
    ToastPositionType.Bottom,
    t => t.SetMessage("Saved!"));
```

`ShowToast` returns an **id** you can keep if you want to dismiss it early:

```csharp
string id = ui.ShowToast<ToastDefault>(ToastPositionType.Top, t => t.SetMessage("Connecting…"));
// later:
ui.HideToast(id);
```

## Positions

Pass any `ToastPositionType`:

`Top` · `TopLeft` · `TopRight` · `Center` · `Bottom` · `BottomLeft` · `BottomRight`

Toasts at the same position stack together, newest closest to the edge.

## What players can do

* **Auto-dismiss** — each toast closes itself after its duration (default 2.5s). Set it per toast
  with `t.SetAutoCloseDuration(seconds)`; use `0` to keep it until you hide it yourself.
* **Swipe to dismiss** — drag a toast sideways far enough and it flies off and closes.

## Custom toasts

Need icons, buttons, or a different look? Make a class that inherits from `BaseToast`, design its
prefab, and add it to the **Toasts** list on your UILibrary. Override `SetMessage` (or add your own
setup methods) to fill it in. The toast auto-sizes to its text within sensible max width/height
ratios you can tweak on the prefab.

## Global settings

On your **UILibrary** (Toast Config section):

| Setting | Default | What it does |
| --- | --- | --- |
| `Max Concurrent Toasts` | 3 | How many show at once. Extra ones wait in a queue; if the limit is reached the oldest is pushed out. |
| `Toast Spacing` | 12 | Gap between stacked toasts (px). |
| `Toast Padding` | 24, 48 | Distance from the screen edges (x = sides, y = top/bottom). |

## Handy members

| Member | Description |
| --- | --- |
| `ui.ShowToast<T>(position, setup)` | Show a toast; returns its id. |
| `ui.HideToast(id)` | Dismiss one early by id (safe with stale ids). |
| `ui.HideAllToasts()` | Clear everything, including the queue. |
| `ui.ActiveToasts` | The toasts currently on screen. |
| `ui.OnToastShown` / `ui.OnToastHidden` | Events as toasts appear and leave. |
