# Phase 01 — FloatingText Core (base + config + template + animation)

## Overview
- Priority: High (foundation)
- Status: Not started
- Build the standalone visual unit and its animation. No manager yet.

## Files to create
- `Runtime/Scripts/FloatingText/FloatingText.cs` — abstract base
- `Runtime/Scripts/FloatingText/FloatingTextConfig.cs` — serializable tuning struct + presets
- `Runtime/Scripts/FloatingText/Templates/FloatingTextDefault.cs` — concrete default

## FloatingText (base)
`public abstract class FloatingText : MonoBehaviour`
- `[RequireComponent(typeof(CanvasGroup))]`
- Fields: `[SerializeField] TMP_Text _text;` `[SerializeField] FloatingTextConfig _config = FloatingTextConfig.Default;`
- Cache `RectTransform _rect`, `CanvasGroup _cg` in `Awake`.
- `internal UIManager Manager { get; set; }` (parity with UIElement, optional).
- `public virtual void SetText(string message)` — data only. Color/font/size are on the prefab's
  TMP_Text (authored in editor). NO runtime style override — different look = different prefab (Toast model).
- `public void Play(Action onComplete)`:
  - Reset: alpha=1, scale=startScale (config.popFrom), anchoredPosition already set by manager.
  - token = `AnimationHelper.ResetToken(this)`.
  - Rise: `AnimationHelper.AnchoredPos(_rect, start + Vector2.up * _config.riseDistance, _config.duration, ct).Forget()`.
  - Pop (optional): `AnimationHelper.Scale(transform, Vector3.one, _config.popDuration, ct).Forget()` when `_config.popFrom != 1`.
  - Fade: start after `duration * fadeStartRatio` delay, then `Alpha(_cg, 0, fadeDuration, ct)`.
  - After full duration: `onComplete?.Invoke()` (manager releases to pool). Use `Awaitable.WaitForSecondsAsync`.
- Non-interactive: ensure no raycast — `_cg.blocksRaycasts = false; _cg.interactable = false;` (prefab too).

## FloatingTextConfig
`[Serializable] public struct FloatingTextConfig` (or class):
- `float riseDistance` (px, default 80)
- `float duration` (default 1.0)
- `float popFrom` (start scale, default 0.6; 1 = no pop)
- `float popDuration` (default 0.15)
- `[Range(0,1)] float fadeStartRatio` (default 0.5 — fade in 2nd half)
- static `Default` preset. Motion authored per-prefab in editor; different look/motion = different prefab.

## FloatingTextDefault
`public class FloatingTextDefault : FloatingText` in `namespace vanhaodev.uimanager.template` — empty body (template marker), matches `ToastDefault`.

## Implementation steps
1. Create `FloatingTextConfig.cs` (under 60 lines).
2. Create `FloatingText.cs` base with Play/SetText (target < 120 lines; split if over).
3. Create `FloatingTextDefault.cs`.
4. Confirm only existing `AnimationHelper` methods are used (AnchoredPos, Alpha, Scale).

## Todo
- [ ] FloatingTextConfig
- [ ] FloatingText base + animation sequence
- [ ] FloatingTextDefault template
- [ ] No new AnimationHelper code needed (verify)

## Success criteria
- Compiles. `Play` rises + fades + auto-finishes, calls onComplete exactly once.
- No raycast blocking (feedback never eats input).

## Risks
- Fade-after-delay race with cancellation token → guard `ct.IsCancellationRequested` before each step.
- Multiple `Play` on a pooled instance → `ResetToken` cancels prior tweens (already the codebase pattern).
