---
description: Fly icons from a source position to a UI counter, with an app-driven count-up.
---

# Flyout Effect

The flyout effect spawns icons (coins, gems, stars…) that burst from a source position and fly
to a registered target on your UI — for example, a coin counter. On arrival each icon makes the
target **shake**.

{% hint style="warning" %}
**The flyout icons are purely visual. They do NOT change the displayed number.** Your app owns the
number and drives the count-up itself (see [Driving the number](#driving-the-number)). This keeps a
single source of truth and avoids double-counting when your game already updates currency through
its own events.
{% endhint %}

## Setup

{% stepper %}
{% step %}
### Configure the UILibrary

On your **UILibrary** asset, set:

* **Flyout Config** — animation/timing settings (see [Config reference](#config-reference)).
* **Flyout Icon Prefab** — the prefab used for each flying icon (a `FlyoutIcon` component).
{% endstep %}

{% step %}
### Assign the flyout layer

On the **UIManager**, assign **Flyout Effect Layer** — the `Transform` (a `RectTransform` on your
canvas) under which flying icons are spawned. Put it above your UI so icons render on top.
{% endstep %}

{% step %}
### Add a FlyoutTarget

Add a `FlyoutTarget` component to the UI field that should receive icons (e.g. the coin counter).

* **Key** — a unique string used to look the target up (e.g. `"money"`).
* **Amount Text** — the `TMP_Text` that shows the number (optional; leave empty for a pure shake target).
{% endstep %}

{% step %}
### Register the target

The library does **not** auto-discover the manager. Register each target yourself, then unregister
when it goes away:

```csharp
[SerializeField] private FlyoutTarget _moneyTarget;
private UIManager _uiManager;

private void OnEnable()
{
    _uiManager = FindFirstObjectByType<UIManager>();
    _moneyTarget.Register(_uiManager);   // registers under its Key
}

private void OnDisable()
{
    _moneyTarget.Unregister();
}
```

{% hint style="info" %}
`FlyoutTarget` also unregisters itself on `OnDestroy` as a safety net, so a destroyed target never
lingers in the registry. If you forget to register, `PlayFlyout` logs `FlyoutTarget not found` and
the shake is skipped.
{% endhint %}
{% endstep %}
{% endstepper %}

## Playing the effect

When the source is a **UI element** (a shop item icon, a button…), use `PlayFlyoutFromRect` and pass
that element's `RectTransform`. The library figures out where it sits on screen for you, so icons
start from the element no matter how your canvas is set up. `amount` is the total value of the batch;
it is split across the spawned icons.

```csharp
_uiManager.PlayFlyoutFromRect(
    source: (RectTransform)itemIcon.transform,   // the icon the player tapped
    targetKey: "money",
    amount: 1000,
    icon: coinSprite,
    onComplete: () => { /* runs once after all icons land */ });
```

{% hint style="success" %}
Prefer `PlayFlyoutFromRect` for UI sources — it just works with Overlay, Screen-Space Camera,
World-Space, and nested canvases. You don't have to compute any screen position yourself.
{% endhint %}

Other ways to start the effect:

* `PlayFlyout(sourceWorldPos, …)` — start from a **world position** (e.g. an enemy in the scene).
* `PlayFlyoutFromScreen(screenPos, …)` — start from a **screen position** you already have.

Each one has an overload that takes a direct `FlyoutTarget` reference instead of a `targetKey`.

## Driving the number

Because icons are visual only, you decide **when** and **how** the number counts up. Initialize once,
then push the new total whenever your value changes.

```csharp
// Init once (no animation):
_moneyTarget.SetValue(currentTotal);

// On every change (gain AND spend) — animates a smooth count-up to the new total:
_moneyTarget.SetTargetValue(newTotal);
```

`SetTargetValue` reuses the built-in lerp, so the number animates from its current display to the
new total.

The number ticks up **as each icon lands**, rising in lockstep with the coins. Subscribe to
`OnIconArrived` and bump the target by each icon's value:

```csharp
private void OnEnable() => _moneyTarget.OnIconArrived += OnCoinArrived;
private void OnDisable() => _moneyTarget.OnIconArrived -= OnCoinArrived;

private void OnCoinArrived(int valuePerIcon)
{
    _moneyTarget.SetTargetValue(_moneyTarget.CurrentTargetValue + valuePerIcon);
}
```

Use `onComplete` only for **side effects** that should run once the batch finishes — e.g. saving the
new balance to your data. The displayed number is still driven by `OnIconArrived` above, so the two
stay in sync. `OnBatchComplete` is also available if you just need a "batch done" callback.

## Formatting the number

Set `Formatter` once to control how the value is displayed — any style your game needs
(`123456789`, `1.000.000`, `12.6k`, `1b3`, `∞`…). When `null`, the raw number is shown.

```csharp
_moneyTarget.Formatter = value =>
{
    if (value < 1_000) return value.ToString();
    if (value < 1_000_000) return (value / 1000d).ToString("0.0") + "k"; // 12600 -> "12.6k"
    return (value / 1_000_000d).ToString("0.0") + "m";
};
```

The formatter is applied on every count-up frame, so abbreviated values animate too
(`…11.8k → 12.6k`).

## Where icons land

By default icons fly into the **centre of the number**. That already follows the text, so even a wide
field (a long label next to the value) gets hit correctly.

If you'd rather have icons land at the **edge** of the number, turn on **Aim By Alignment** in your
Flyout Config. The landing spot then follows the text's own alignment:

| Text alignment | Icons land at |
| --- | --- |
| Left | the **end** of the number (right side) |
| Right | the **start** of the number (left side) |
| Center | the **end** of the number (right side) |

Use **Aim Edge Gap** to push the landing spot a little further out so icons sit just past the digits
instead of on top of them (it scales with the text size; `0` lands right on the edge).

{% hint style="info" %}
Need a pixel-perfect spot? Drop an empty child where you want icons to hit and assign it to the
**Aim Point** field on the `FlyoutTarget`. It overrides the centre/alignment behaviour.
{% endhint %}

## API reference

### FlyoutTarget

| Member | Description |
| --- | --- |
| `Register(UIManager manager)` | Register under `Key`; stores the manager (also needed for the shake). |
| `Unregister()` | Remove from the registry. Safe to call when not registered. |
| `SetValue(long value)` | Set display and target instantly (no animation). Use to initialize. |
| `SetTargetValue(long value)` | Set the target and animate the count-up from the current display. |
| `Formatter` | `Func<long, string>` used to format the displayed value; `null` → raw `ToString()`. |
| `CurrentTargetValue` | The current target value. |
| `OnIconArrived` | `event Action<int>` — fired per icon arrival with its value. |
| `OnBatchComplete` | `event Action` — fired once when all icons in a batch arrive. |
| `Key` | The lookup key for this target. |

### Config reference

Set on the **Flyout Config** in your UILibrary.

| Field | Default | Description |
| --- | --- | --- |
| `DirectCountMax` | 10 | For amounts 1..this, show an exact icon count. |
| `ScaledCountPer10` | 2 | Above `DirectCountMax`, extra icons per 10 units. |
| `MaxIcons` | 28 | Hard cap on icons regardless of amount. |
| `BurstDuration` | 0.15 | Duration of the initial scatter burst. |
| `FlightDuration` | 0.5 | Duration of the flight to the target. |
| `ImpactDuration` | 0.1 | Duration of the impact fade-out. |
| `SpawnStagger` | 0.02 | Delay between each icon spawn. |
| `BurstRadius` | 80 | Scatter radius from the source. |
| `BurstRandomness` | 0.3 | Randomness of the burst (0–1). |
| `FlightCurveHeight` | 100 | Height of the bezier flight arc. |
| `FlightRandomness` | 0.2 | Randomness of the flight path (0–1). |
| `ShakeIntensity` | 5 | Shake strength on impact. |
| `ShakeDuration` | 0.1 | Shake duration on impact. |
| `AimByAlignment` | off | Land icons at the number's edge (per text alignment) instead of its centre. |
| `AimEdgeGap` | 0 | When aiming by alignment, gap past the edge (as a fraction of text height). |
