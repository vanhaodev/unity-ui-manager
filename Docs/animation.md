---
description: How screens, popups and toasts animate in and out — and how to add your own.
---

# Animation

Every piece of UI (screen, popup, toast, loading block) can play an **open** animation when it
appears and a **close** animation when it leaves. UI Manager waits for these to finish before moving
on, so transitions never overlap or get cut off.

If a piece of UI has **no animation set, it just snaps on and off** instantly — animations are
entirely optional.

## Give a UI an animation

Call `SetAnimation` with any animation object, usually in `Awake` or right after you create the UI:

```csharp
public class HomeScreen : BaseScreen
{
    protected override void Awake()
    {
        base.Awake();
        SetAnimation(new SlideAnimation());
    }
}
```

## Write your own animation

An animation just needs to do two things: play the **show**, play the **close**, and call
`onComplete` when each one finishes. Inherit from `UIAnimationBase` and override the two methods:

```csharp
public class SlideAnimation : UIAnimationBase
{
    public override void PlayShow(GameObject target, Action onComplete)
    {
        // tween target in (move/fade/scale)…
        onComplete?.Invoke();   // call this when the tween ends
    }

    public override void PlayClose(GameObject target, Action onComplete)
    {
        // tween target out…
        onComplete?.Invoke();
    }
}
```

{% hint style="warning" %}
Always call `onComplete` at the end of both methods. UI Manager uses it to know the animation is
done — if you forget, the UI can get stuck mid-transition.
{% endhint %}

### Prefer a component?

If your animation needs Inspector fields or lives on the prefab, inherit from
`UIAnimationMonoBase` instead — it's the same two methods, but as a `MonoBehaviour` you can attach
and configure on the GameObject, then pass it to `SetAnimation`.

## While an animation plays

During an open or close animation, UI Manager turns on a **block overlay** so the player can't tap
through a half-finished transition. You can wire this up by assigning a full-screen GameObject to the
`_blockOverlay` field on the prefab; it's toggled for you automatically.

`IsAnimating` on the UI element tells you whether a transition is currently running.
