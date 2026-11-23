---
description: Notification dots, tabs, scroll helpers, safe-area bars and drag & drop.
---

# Widgets

Small components for your own UI. Only the notification dot goes through the UIManager; the rest are
standalone.

## Notification Dot — `ui.ShowBadge(target)`

The dot that says "something here is waiting for you" (a reward to claim, an unread message). While it is
shown it wobbles now and then and gently swells and shrinks.

You do not place dots in your prefabs. Point the manager at any UI element and it pins the dot there:

```csharp
var shopButton = (RectTransform)_btnShop.transform;

ui.ShowBadge(shopButton);                       // dot on
ui.HideBadge(shopButton);                       // dot off
ui.SetBadge(shopButton, hasNewItems);           // on or off, for a refresh that recomputes it each time
bool shown = ui.IsBadgeShown(shopButton);
```

### Setup

Open your UILibrary, **Badge** tab:

* **Badge Prefab** — the dot. Use the built-in `AttentionBadgeDefault` (a red circle) or your own prefab
  with a `UIAttentionBadge` on a RectTransform root, pivot at its centre.
* **Badge Anchor** — where on the target the dot is pinned, in the target's own rect: `(1, 1)` = top-right
  corner (default), `(0.5, 0.5)` = centre.
* **Badge Offset** — a nudge from that point, e.g. `(-12, -12)` pulls the dot inside the corner.

The first show creates the dot as a child of the target. Later calls reuse it, so calling `SetBadge` on
every refresh costs nothing. The dot is drawn over the target's other children, a layout group on the
target ignores it, and it is destroyed together with its target. A `Mask` on the target clips it, so pin
it to an element without one.

### The dot's motion

These settings are on the `UIAttentionBadge` component of the badge prefab. The motion runs on unscaled
time, so it keeps moving on a popup opened over a paused game.

| Setting | Default | What it does |
| --- | --- | --- |
| `Angle` | 15 | Peak tilt of one wobble, in degrees. |
| `Wobble Duration` | 0.5 | How long one wobble lasts. |
| `Rest Duration` | 1.2 | Pause between two wobbles. `0` = wobble non-stop. |
| `Bubble` | on | Off = the dot keeps its size. |
| `Bubble Scale` | 1.15 | Size at the top of a swell (1 = no swell). |
| `Bubble Duration` | 0.45 | One swell or one shrink. |

{% hint style="info" %}
**When** a dot shows is your game's call. For example, a "seen it this session" rule belongs in your code:
the badge only shows, hides and moves.
{% endhint %}

## Tab Group — `UITabGroup` + `UITab`

A row of tabs where exactly one is selected.

1. Make a tab prefab with a `UITab`: a **Button**, an optional **Label** (TMP) and **Icon** (Image), a
   **Selected State** and a **Normal State** object, and optionally a **Badge Target** — where its
   notification dot is pinned (empty = the tab itself).
2. Put a `UITabGroup` on the row, with the **Container** the tabs are created under and the **Tab Prefab**.

Tab dots go through `ui.SetBadge` like any other. The group finds the UIManager of the screen or popup it
sits in; for a group outside one, set `_tabs.Manager` yourself.

```csharp
_tabs.AddTab(0, "Weapons");
_tabs.AddTab(1, null, armorIcon);
_tabs.OnTabChanged += ShowPage;
_tabs.SelectTab(0);

_tabs.SetTabBadge(1, hasNewArmor);   // the tab's notification dot
```

A tab shows an icon **or** a label, never both, so every tab in the row keeps the same width. Given
neither, it shows its **Placeholder Icon** (leave it empty to show nothing). Subclass `UITab` for a tab
that carries more, such as a count or a lock.

## Scroll helpers

* **`NestedScrollRect`** — a ScrollRect inside another one that scrolls the other way (a horizontal row
  inside a vertical list). A drag along its own axis scrolls it; a drag across it is passed to the
  ScrollRect above. Swap it in for a ScrollRect: every ScrollRect field is kept.
* **`FillViewportHeight`** — put it on a scroll view's Content, next to its ContentSizeFitter. The content
  is never shorter than the viewport, so a short text can sit centred in the visible area while a long one
  still scrolls.
* **`ScrollToTop()` / `ScrollToBottom()`** — extension methods on `ScrollRect`. `ScrollToBottom` rebuilds
  the layout first, so it lands on the real end of content you just replaced.

```csharp
_scroll.ScrollToTop();       // a reused popup opening on fresh text
_scroll.ScrollToBottom();    // a log opening on its newest line
```

## Safe Area Bars — `SafeAreaBars`

Covers the strips **outside** the safe area (the notch and the home indicator) with a top and a bottom bar,
and leaves the middle alone. Useful on an overlay canvas over a 3D scene, where one full-screen background
would hide the scene.

Put it on a full-stretch RectTransform that is a **direct child of the Canvas**, next to your safe-area
node (not inside it, or it gets clipped to the very area it should cover). Give it two child bars with any
art and assign them to **Top** and **Bottom**. A bar with nothing to cover switches itself off. Portrait
only.

## Drag & Drop — `UIDraggable`

Makes a UI element draggable: while dragging, a copy of its icon follows the finger and the original fades.

1. Add `UIDraggable` (it adds a CanvasGroup). **Drag Icon** is the Image copied for the clone (empty =
   the first Image in its children).
2. Optional: **Drag Handle** limits where a drag can start. A drag that starts elsewhere, or while
   `CanDrag` is false, is handed to the ScrollRect it sits in, so lists still scroll.

```csharp
_draggable.OnDragStart += () => ShowDropZones(true);
_draggable.OnDragEnd   += () => ShowDropZones(false);
```

Drop targets are yours: use Unity's `IDropHandler` on them. Call `ForceCleanup()` if the element is
hidden mid-drag.
