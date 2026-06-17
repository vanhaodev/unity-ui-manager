# FloatingText Feedback — Plan Overview

## Goal
Add a **FloatingText** feature: ephemeral text that spawns at a UI anchor (e.g. a button)
and floats upward while fading out. Use case: press "Buy" with no money → "Not enough money"
floats up from the button. Fire-and-forget, non-interactive, many at once.

## Why a separate feature (not a Toast)
| | Toast (existing) | FloatingText (new) |
|---|---|---|
| Position | Anchored to screen edge | Spawns at an arbitrary point (the button) |
| Interaction | Draggable / swipe / click-close | None (pure feedback) |
| Lifetime | Has `Id`, queued, manager-tracked | Fire-and-forget, auto-destroys |
| Quantity | ~1 prominent | Many simultaneously |

Reusing `BaseToast`/`UIElement` would drag in drag, swipe, auto-close timer, id tracking,
block-overlay — all unused here. So FloatingText is a standalone `MonoBehaviour`.

## Architecture (mirrors Toast conventions)
- Standalone `FloatingText : MonoBehaviour` (own `Play(...)`, releases itself on finish).
- `ObjectPool<FloatingText>` per type — same `com.vanhaodev.objectpool` as Toast.
- Registered in `UILibrary` (`_floatingTexts` list + generic getter).
- `UIManager.FloatingText.cs` partial — `_floatingTextLayer`, pool, public API, pos conversion.
- Animation via existing `AnimationHelper` (AnchoredPos + CanvasGroup Alpha + Scale) — no new tween code.

## Public API (anchor: support BOTH Transform and screen Vector — user was unsure)
```csharp
// Convenience (default template)
uiManager.ShowFloatingText("Not enough money", buyButton.transform);          // UI anchor
uiManager.ShowFloatingText("-100", screenPos, color: Color.red);              // screen point
// Custom template
uiManager.ShowFloatingText<DamageFloatingText>(t => t.Set(120), buyButton.transform);
```

## Phases
- [ ] [phase-01](phase-01-floating-text-core.md) — `FloatingText` base + config + default template + animation
- [ ] [phase-02](phase-02-manager-and-library.md) — `UIManager.FloatingText.cs` partial, pool, pos conversion, UILibrary wiring
- [ ] [phase-03](phase-03-prefab-and-verification.md) — prefab/layer setup notes, compile check, manual verification

## Key dependencies
- `com.vanhaodev.objectpool` (already used by Toast)
- `AnimationHelper` (effect namespace)
- `RectTransformUtility` for world/screen → layer-local conversion

## Open questions
- Color presets: pass raw `Color` (KISS, chosen) vs a `FloatingTextStyle` enum in UILibrary. Starting with raw `Color?`; can add enum later if needed.
