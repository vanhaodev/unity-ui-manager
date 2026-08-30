---
description: Little labels that pop up and float away — "+100", "Miss!", combo counts.
---

# Floating Text

**Floating text** is a small label that appears at a spot, floats up, and fades — great for damage
numbers, "+100" coin gains, "Miss!", combo counters and the like. Spam it and the labels stack
neatly instead of overlapping.

## Show some text

Float a message from a UI element (it follows that element's position):

```csharp
ui.ShowFloatingText("+100", coinButton.transform);
```

…or from a world object, by converting its position to a screen point:

```csharp
ui.ShowFloatingText("Miss!", Camera.main.WorldToScreenPoint(enemy.position));
```

## Custom labels

For your own style (icons, colours, bigger combos), make a class that inherits from `FloatingText`,
design its prefab, and add it to the **Floating Texts** list on your UILibrary. Then show it with a
setup callback:

```csharp
ui.ShowFloatingText<FloatingTextAlbum>(
    t => t.SetAlbum(name, cover),
    spawnButton.transform);
```

The built-in `FloatingTextDefault` is ready to use for plain messages.

## It sizes itself to the message

`FloatingTextDefault` has no fixed width. A two-word message gets a small label; a long one widens,
then wraps and deepens, and only past that does the font shrink and the tail get cut. So you can
throw any string at it without checking the length first.

That comes from a [`TextFitter`](text.md) on the label and a `TextFitterBackground` on the root —
both plain components, so you can build the same behaviour into your own `FloatingText` subclass, or
tune the limits on the prefab. Set the text through the fitter rather than assigning `.text`, so the
label has its real size before the manager clamps it to the screen edges:

```csharp
public override void SetText(string message) => _fitter.SetText(message);
```

## Keeps itself tidy

* **No overlap** — labels from the same source are spaced out in time so they don't pile up.
* **Stays on screen** — labels are nudged to stay inside the screen edges.
* **No lag from spam** — past a per-source limit, the oldest label fades immediately.

Tune all of this on your **UILibrary** (Floating Text Config section):

| Setting | Default | What it does |
| --- | --- | --- |
| `Floating Show Interval` | 0.2 | Delay between labels from the same source (s). |
| `Floating Max Per Source` | 20 | Max active labels per source before the oldest is dropped. |
| `Floating Screen Padding` | 16, 16 | Keep labels this far inside the screen edges (px). |

## Handy members

| Member | Description |
| --- | --- |
| `ui.ShowFloatingText(text, anchor)` | Float a message from a UI element. |
| `ui.ShowFloatingText(text, screenPos)` | Float a message from a screen position. |
| `ui.ShowFloatingText<T>(setup, anchor)` | Float a custom label from a UI element. |
| `ui.ShowFloatingText<T>(setup, screenPos)` | Float a custom label from a screen position. |
