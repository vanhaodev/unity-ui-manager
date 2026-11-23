---
description: Tabs, scroll helpers, safe-area bars and drag & drop.
---

# Widgets

Small standalone components for your own UI. Looking for notification dots? See
[Notification Dot](badge.md).

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
