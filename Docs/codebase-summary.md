# Codebase Summary (for AI agents & maintainers)

Fast map of the package so you can locate code without re-scanning. Not a GitBook page (kept out of
`SUMMARY.md`). Update it when structure/conventions change.

/ **Source of truth:** `Assets/com.vanhaodev.uimanager/`. `Exported/com.vanhaodev.uimanager/` is a
verbatim mirror (identical GUIDs) that users install — only synced at release time, see
[`release-export-guide.md`](release-export-guide.md). **Edit `Assets/`, never `Exported/` by hand.**

## Layout — `Assets/com.vanhaodev.uimanager/Runtime/Scripts/`

| Area | Path | Notes |
| --- | --- | --- |
| Manager (partials) | `Manager/UIManager.*.cs` | One partial per feature: `.Screen`, `.Popup`, `.Toast`, `.LoadingBlock`, `.FloatingText`, `.FlyoutEffect`; core in `UIManager.cs`. |
| Library asset | `Data/UILibrary.cs` | `ScriptableObject` holding prefab lists + per-feature config. Type→prefab lookup via `Get*Prefab<T>()`. |
| Core UI base | `Core/UIElement.cs` → `Core/InteractableUI.cs` | Show/Close lifecycle, `SetAnimation`, `_blockOverlay`, `_btnClose`. |
| Animation | `Core/Animation/` | `IUIAnimation` + `UIAnimationBase` / `UIAnimationMonoBase` (override `PlayShow`/`PlayClose`, call `onComplete`). |
| Screen | `Screen/BaseScreen.cs` | Hooks: `OnEnter`/`OnExit`. |
| Popup | `Popup/BasePopup.cs` | Hooks: `OnPopupOpened`/`OnPopupClosed`; `_backgroundBtn` + `_closeOnBackgroundClick`. |
| Toast | `Toast/` | `BaseToast` (auto-close timer, swipe-dismiss), `ToastPositionType`, `Templates/ToastDefault`. |
| Loading block | `LoadingBlock/` | `BaseLoadingBlock`, `LoadingBlockHandle` (IDisposable), ref-counted. |
| Floating text | `FloatingText/` | `FloatingText`, `FloatingTextConfig`, `Templates/FloatingTextDefault`. |
| Flyout effect | `FlyoutEffect/` | `FlyoutTarget`, `FlyoutIcon`, `FlyoutAnimator`, `FlyoutConfig`, `FlyoutRegistry`. |
| Misc widgets | `Button/UIButton.cs`, `Text/MarqueeText.cs` | Standalone helpers. |
| Sample | `Samples/K-pop Shop/` | Working example of every feature. |

## How a feature is wired

1. Prefab has a component inheriting the feature's base class (`BaseScreen`/`BasePopup`/…).
2. Prefab is added to the matching list on a **UILibrary** asset.
3. `UIManager` (scene) holds the library + a layer `Transform` per feature (draw order).
4. Call `ui.Show*<T>()` → manager looks up the prefab **by type**, instantiates under the layer,
   caches it, runs the animation. No prefab references in code.

Show/close flow: `UIElement.Show/Close` → `OnShowStart`→ animation → `OnShowEnd` (+ `onComplete`).

## Caching & pooling

- Screens/popups/loading blocks: cached per type, reused (`ClearCache()` frees all).
- Popups can stack (multiple active); newest on top by default. `ShowPopup(..., keepSameTypeOnTop: true)`
  slips a new popup below an open same-type one (opt-in; see `TryGetNearestSameType`).
- Toasts & flyout icons: **object-pooled** (`com.vanhaodev.objectpool` dependency).

## Conventions / gotchas

- **App owns numbers.** Flyout icons are visual only; the displayed value is driven by the app via
  `FlyoutTarget.SetTargetValue` + `OnIconArrived`. Don't make the library mutate counters.
- **Cross-canvas camera rule (flyout/floating text).** world→screen must use the camera of the
  object's **own** canvas; screen→local uses the **target layer's** camera. Always resolve via
  `rootCanvas` (nested canvases keep a serialized `renderMode` but null `worldCamera`). See
  `CameraOf` in `UIManager.FlyoutEffect.cs`. ⚠️ `UIManager.FloatingText.cs > GetLayerCamera()` still
  has the pre-fix `GetComponentInParent<Canvas>()` form — fix to `rootCanvas` if touched.
- **Async:** uses Unity 6 `Awaitable` + `AnimationHelper`; cancellation via `AnimationHelper.ResetToken`.
- **No auto-discovery:** `FlyoutTarget` must be `Register(manager)`ed by the app (unregisters on destroy).
- **Versioning:** version lives in 4 places — see [`release-export-guide.md`](release-export-guide.md).

## User-facing docs

GitBook pages in `Docs/usage/` (one per feature) + `Docs/animation.md`. TOC in `Docs/SUMMARY.md`.
Keep those friendly/non-technical; keep this file terse/technical.
