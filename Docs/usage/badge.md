---
description: Notification dots the manager pins on any UI element — no dot placed in your prefabs.
---

# Notification Dot

A **notification dot** is the little marker that says "something here is waiting for you": a reward to
claim, an unread message, a new item in the shop. While it is shown it wobbles now and then and gently
swells and shrinks, so the eye finds it.

You never place dots in your prefabs. Point the manager at any UI element, and it pins the dot there.

```csharp
var shopButton = (RectTransform)_btnShop.transform;

ui.ShowBadge(shopButton);                 // dot on
ui.HideBadge(shopButton);                 // dot off
ui.SetBadge(shopButton, hasNewItems);     // on or off, for a refresh that recomputes it each time
```

## Setup

{% stepper %}
{% step %}
### Pick the dot

Open your **UILibrary**, go to the **Badge** tab and set **Badge Prefab**. Use the built-in
`AttentionBadgeDefault` (a red circle), or your own prefab: a `UIAttentionBadge` on a RectTransform root,
pivot at its centre, with your dot art on it or on a child.
{% endstep %}

{% step %}
### Choose where it sits

* **Badge Anchor** — where on the target the dot is pinned, in the target's own rect: `(1, 1)` is the
  top-right corner (the default), `(0.5, 0.5)` the centre.
* **Badge Offset** — a nudge from that point. `(-12, -12)` pulls the dot a little inside the corner.

Every dot uses the same anchor and offset, so dots look the same across your whole UI.
{% endstep %}

{% step %}
### Show it

Call `SetBadge` wherever you already refresh that piece of UI:

```csharp
private void Refresh()
{
    ui.SetBadge(_questButtonRect, quests.HasRewardToClaim());
}
```
{% endstep %}
{% endstepper %}

## How it behaves

* **Created once, reused.** The first show creates the dot as a child of the target; later calls reuse
  it. Calling `SetBadge` on every refresh costs nothing, and hiding a target that never had a dot creates
  nothing.
* **On top, out of the layout.** The dot is drawn over the target's other children, and a layout group on
  the target ignores it.
* **Lives with its target.** When the target is destroyed (a list row rebuilt, a screen unloaded), its dot
  goes with it. `ClearCache()` removes every dot.
* **Unscaled time.** The dot keeps moving on a popup opened over a paused game.

{% hint style="warning" %}
A `Mask` or `RectMask2D` on the target clips the dot, because the dot is the target's child. Pin it to an
element that does not mask, such as the button rather than a masked icon inside it.
{% endhint %}

{% hint style="info" %}
**When** a dot shows is your game's call. A rule such as "hide every quest dot once the player has opened
the quest popup this session" belongs in your code; the dot only shows, hides and moves.
{% endhint %}

## Dots inside screens and popups

A row or widget deep inside a screen or popup can find the manager without being handed it:

```csharp
UIManager.Of(this)?.SetBadge(_claimButtonRect, canClaim);
```

`UIManager.Of(component)` returns the manager that owns the screen or popup the component sits in. A screen
or popup gets its manager right after it is created, so this returns null during its very first `OnEnable`.
If you refresh dots there, keep a reference to the manager instead.

Tabs have this built in: `UITabGroup.SetTabBadge(index, visible)` pins the dot on the tab (or on its
**Badge Target**). See [Widgets](widgets.md#tab-group--uitabgroup--uitab).

## The dot's motion

These settings are on the `UIAttentionBadge` component of your badge prefab.

| Setting | Default | What it does |
| --- | --- | --- |
| `Angle` | 15 | Peak tilt of one wobble, in degrees. |
| `Wobble Duration` | 0.5 | How long one wobble lasts. |
| `Rest Duration` | 1.2 | Pause between two wobbles. `0` = wobble non-stop. |
| `Bubble` | on | Off = the dot keeps its size. |
| `Bubble Scale` | 1.15 | Size at the top of a swell (1 = no swell). |
| `Bubble Duration` | 0.45 | One swell or one shrink. |

## Handy members

| Member | Description |
| --- | --- |
| `ui.ShowBadge(target)` | Shows the dot on `target`, creating it on first use. Returns the dot. |
| `ui.HideBadge(target)` | Hides the dot on `target`. Safe when it never had one. |
| `ui.SetBadge(target, visible)` | Shows or hides it in one call. Returns the dot, or null when there is none. |
| `ui.IsBadgeShown(target)` | Whether the dot on `target` is currently shown. |
| `UIManager.Of(component)` | The manager owning the screen or popup `component` sits in. |
