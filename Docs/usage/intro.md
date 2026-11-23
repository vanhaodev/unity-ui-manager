---
description: A boot logo that pops in, holds while your game loads underneath, and leaves.
---

# Intro

The **intro** is the studio or game logo you see when the app starts: it pops in, stays up for a moment
while the game loads behind it, then goes away. Because the game keeps loading while the logo is up,
the intro costs no load time.

{% hint style="info" %}
**Draws above everything.** The intro is its own overlay canvas, not a child of a UIManager layer. It is
sorted by the library's **Intro Sorting Order** (1000 by default), so it sits on top of your UI and any
loading screen. It also survives scene loads and never blocks taps.
{% endhint %}

## Setup

{% stepper %}
{% step %}
### Make the intro prefab

Create a UI GameObject with a **Canvas** (Screen Space - Overlay), a **CanvasScaler**, a **CanvasGroup**
and the **IntroView** component. Add a child for your logo (an Image, a text, anything) and drag it into
IntroView's **Logo** slot. A full-screen background behind it is optional. Save it as a prefab.

Lay the logo out however you like. The intro only animates its **scale** and the group's **alpha**,
so your anchors and sizes stay exactly as you set them.
{% endstep %}

{% step %}
### Add it to your UILibrary

Open your UILibrary, go to the **Intro** tab and drag the prefab into **Intro Prefab**. Set
**Intro Hold Seconds** to how long the logo should stay up.
{% endstep %}

{% step %}
### Play it at boot

Pass your boot work to `PlayIntro`. The work starts right away and runs under the logo, and the intro
leaves once **both** the hold is over and the work is done:

```csharp
await ui.PlayIntro(async () =>
{
    await LoadSave();
    await LoadFirstScene();
});
```

The intro leaves even if the work throws, and the exception is passed on to you.
{% endstep %}
{% endstepper %}

## The manual way — start and end it yourself

When your boot starts the intro in one place and ends it in another, use `ShowIntro`. It returns the
sequence that drives the intro:

```csharp
var intro = ui.ShowIntro();         // on screen, logo hidden (null if the library has no intro)

await intro.PlayAsync();            // logo pops in and holds
// …more boot work…
await intro.HideAsync();            // logo leaves, the intro closes itself
```

## Timing

`Intro Hold Seconds` counts from the moment the logo **starts** to appear, so the pop is part of it. Set
it to the length of your logo sound and the logo leaves when the sound ends.

The intro waits for the frame rate to settle before it pops. The first frames of an app are often slow
(shaders compiling, SDKs starting), and a pop that starts then would skip straight to its end. If the
frames never settle, it gives up after `MaxSteadyWaitSeconds` and pops anyway. The intro runs on
unscaled time, so setting `Time.timeScale` to 0 does not freeze it.

## Your own animation

By default the logo pops in with a little overshoot and fades out. For a different motion, add a
component that implements `IIntroAnimation` to the prefab, next to IntroView. You can write it with
DOTween or any other tween library.

```csharp
public sealed class MyIntroAnimation : MonoBehaviour, IIntroAnimation
{
    public void PlayAppear(IntroView view, Action onLanded, Action onComplete) { /* scale 0 → 1 */ }
    public void PlayIdle(IntroView view) { /* optional: a gentle breathe */ }
    public void StopIdle(IntroView view) { /* back to rest */ }
    public void PlayHide(IntroView view, Action onComplete) { /* take it away */ }
}
```

Call `onLanded` when the logo lands, which is where the bounce sound plays, and call `onComplete` when
the motion has finished.

## Sounds

The intro plays no audio itself, so it always goes through your game's own sound system and respects
the player's volume. Add components that implement `IIntroCueHandler` to the prefab, and each one is
told when something happens:

| Cue | When |
| --- | --- |
| `AppearStart` | The logo starts to appear. |
| `Bounce` | The logo lands. |
| `HideStart` | The intro starts to leave. |

```csharp
public sealed class IntroSounds : MonoBehaviour, IIntroCueHandler
{
    public void OnIntroCue(IntroCue cue)
    {
        if (cue == IntroCue.Bounce) MySoundSystem.Play("logo_sting");
    }
}
```

{% hint style="info" %}
Start the intro **after** your save has loaded, because that is where the player's volume is stored.
Otherwise the logo sound plays at full volume even when the player has muted the game.
{% endhint %}

## Handy members

| Member | Description |
| --- | --- |
| `ui.PlayIntro(work)` | Shows the intro, runs `work` under it, and hides it once both are done. |
| `ui.ShowIntro()` | Puts the intro on screen with the logo hidden and returns its `IntroSequence`. |
| `IntroSequence.PlayAsync()` | Pops the logo in and holds it. Returns with the intro still on screen. |
| `IntroSequence.HideAsync()` | Takes the intro away, then closes it. |
| `IntroSequence.Appeared` | Event raised once the logo has finished appearing. |
| `HoldSeconds` | How long the logo is up, counted from the start of the pop. Starts at the library's value. |
| `MaxSteadyWaitSeconds` / `MaxSteadyWaitBeforeHideSeconds` | Longest time to wait for steady frames before popping in / before hiding. |
