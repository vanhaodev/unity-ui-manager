---
description: A little effect at every touch point, for snappy, responsive-feeling input.
---

# Click Effect

Mobile games feel more responsive when a tap leaves a mark — a small ripple right where the finger
landed. **Click Effect** plays a visual at each click/touch point. It sits on the very top layer and
never blocks input, so a button you tap still works while the ripple plays over it.

{% hint style="info" %}
**Plays anywhere, blocks nothing.** Auto mode reads the pointer device directly (not the UI event
system), so the effect fires wherever you press — empty space, backgrounds, **and on top of buttons,
sliders or scroll views** — without ever stealing their input.
{% endhint %}

{% hint style="info" %}
**Costs nothing until used.** If you never enable it or call it, no pool and no objects are created —
zero memory. Everything spins up lazily on the first effect.
{% endhint %}

## Setup

{% stepper %}
{% step %}
### Make the effect prefab

Create a UI GameObject with an **Image** (your ripple sprite), a **CanvasGroup**, and the
**ClickEffectRipple** component, then save it as a prefab. Tweak its scale/fade/duration fields to
taste. (Want particles, a VFX Graph, or a sound instead? See [Custom effects](#custom-effects).)
{% endstep %}

{% step %}
### Assign it in the UILibrary

On your UILibrary, under **Click Effect Config**, drop the prefab into **Prefab**. Leaving it empty
keeps the feature off.
{% endstep %}

{% step %}
### Add the layer

On the **UIManager**, assign **Click Effect Layer** — a full-screen `RectTransform`, placed **on top
of everything else** so effects render above your UI. Don't put a raycast-target graphic on it, so it
never intercepts taps.
{% endstep %}
{% endstepper %}

## Two ways to trigger it

### Let it play on every tap

Tick **Active Feature** in the config and every press (mouse, touch or pen) spawns an effect at that
point, anywhere on screen. **Min Interval** throttles rapid taps (seconds between auto effects;
default `0.1`, set `0` for no limit).

{% hint style="success" %}
Auto mode works with **any input setup** — it detects the New Input System or the legacy Input
Manager automatically (`#if`), so you don't configure anything. A project using the New Input System
needs the `com.unity.inputsystem` package (most already have it); legacy-only projects just work.
{% endhint %}

### Play it yourself

Leave **Active Feature** off and call it from your own code whenever you want a hit:

```csharp
ui.PlayClickEffect(screenPosition);
```

This is handy for "only on confirmed actions" feedback, or when you already handle input yourself.
It needs no input package at all.

## Custom effects

The ripple is just the default. For anything fancier — a particle burst, a VFX Graph, a sound on tap,
or all of them — make a class that inherits from `BaseClickEffect`, do your thing in `Play`, and call
`onComplete` when it finishes so it can be recycled:

```csharp
public class SparkleClick : BaseClickEffect
{
    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private AudioSource _sound;
    private Action _onDone;

    public override void Play(Action onComplete)
    {
        _onDone = onComplete;
        _particles.Play();
        _sound?.Play();
        Invoke(nameof(Done), _particles.main.duration);
    }

    private void Done() => _onDone?.Invoke();
}
```

Put your component on the prefab and assign that prefab in the config — the manager pools and plays
it exactly the same way.

## Handy members

| Member | Description |
| --- | --- |
| `ui.PlayClickEffect(screenPos)` | Play one effect at a screen position (no input package needed). |
| `ClickEffectConfig.Prefab` | The effect prefab (null = feature off). |
| `ClickEffectConfig.ActiveFeature` | Auto-play on every press, anywhere (off by default). |
| `ClickEffectConfig.MinInterval` | Throttle between auto effects (seconds, default 0.4). |
