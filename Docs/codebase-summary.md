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
| Floating text | `FloatingText/` | `FloatingText`, `FloatingTextConfig`, `Templates/FloatingTextDefault`. The template sizes itself to the message: root = `TextFitterBackground` (the tracked box, no artwork of its own), `Panel` child = the sliced background stretched a little past it for padding, `TxMgs` child = the label with `TextFitter`. The label uses centred anchors, not stretch — that is what stops the box and the label resizing each other. `UIManager.SpawnFloatingText` already rebuilds the layout and clamps to screen right after setup, so the new size is picked up with no manager change. |
| Flyout effect | `FlyoutEffect/` | `FlyoutTarget`, `FlyoutIcon`, `FlyoutAnimator`, `FlyoutConfig`, `FlyoutRegistry`. |
| Click effect | `ClickEffect/` | `BaseClickEffect` (subclass for particle/VFX/sound), `Templates/ClickEffectRipple`, `ClickEffectConfig`. Manager partial polls pointer in `Update` (`#if ENABLE_INPUT_SYSTEM` / `#elif ENABLE_LEGACY_INPUT_MANAGER`). |
| Misc widgets | `Button/UIButton.cs`, `Text/MarqueeText.cs` | Standalone helpers. |
| Wave text | `Text/WaveText.cs` | Rolls a sine wave through a label's glyphs, each character a little later than the one before. Writes the vertex buffer, not the transform, so letters move independently; regenerates the mesh every frame because the offsets bake in. `Stop()` rebuilds once on the way out, otherwise the last frame's wave freezes into the label. Never touches the layout box. |
| Text fitting | `Text/TextFitter.cs`, `Text/TextFitterBackground.cs` | `TextFitter` sits on a TMP label and sizes its box to the string, spending width then height then the font: hug at Font Size Max → stop at Max Width and wrap, the box deepening → stop at Max Height and shrink the font → ellipsize at the minimum. The middle rung is `_fitVertical` (opt-in); with it off the height stays as authored and Max Width leads straight to shrinking. Auto sizing therefore turns on only when *both* axes are spent (`tooWide && tooTall`) under vertical fit, but on *either* (`||`) without it. It owns the TMP font settings rather than reading them; overflow and word wrapping are only *borrowed* — `Cache()` snapshots both before the first fit and each is handed back once no longer needed. It also refits only when the string actually changed, so a per-frame mesh rebuild (`WaveText` on the same label) or its own box resize does not retrigger it. `TextFitterBackground` sits on a box behind the label and takes the label's size on its chosen axis — the fitter holds the list and pushes size out one way, never reading back. No padding on the background: its artwork is a stretched child whose offsets are the padding. Keep the background a sibling of the label, not its parent. |
| Editor | `Editor/` (own asmdef) | `UILibraryEditor` — tabbed IMGUI inspector (one tab/feature) + a "Reload missing prefab references" button. UILibrary keeps an editor-only GUID snapshot (`#if UNITY_EDITOR`) auto-synced while healthy; Reload refills dropped (`None`) list slots by index. |
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
- Toasts, flyout icons & click effects: **object-pooled** (`com.vanhaodev.objectpool` dependency).
  Click-effect pool is **lazy** — created on first `PlayClickEffect`, so it costs nothing when unused.

## Conventions / gotchas

- **App owns numbers.** Flyout icons are visual only; the displayed value is driven by the app via
  `FlyoutTarget.SetTargetValue` + `OnIconArrived`. Don't make the library mutate counters.
- **Cross-canvas camera rule (flyout/floating text).** world→screen must use the camera of the
  object's **own** canvas; screen→local uses the **target layer's** camera. Always resolve via
  `rootCanvas` (nested canvases keep a serialized `renderMode` but null `worldCamera`). See
  `CameraOf` in `UIManager.FlyoutEffect.cs`. ⚠️ `UIManager.FloatingText.cs > GetLayerCamera()` still
  has the pre-fix `GetComponentInParent<Canvas>()` form — fix to `rootCanvas` if touched.
- **Async:** uses Unity 6 `Awaitable` + `AnimationHelper`; cancellation via `AnimationHelper.ResetToken`.
- **Click effect input:** auto-play reads the pointer **device** directly (bypasses EventSystem) so it
  fires everywhere incl. over buttons without blocking. Backend auto-detected by `#if` — the asmdef
  references `Unity.InputSystem`; legacy-only projects without the package compile fine (one harmless
  unresolved-reference warning) and use the legacy branch.
- **No auto-discovery:** `FlyoutTarget` must be `Register(manager)`ed by the app (unregisters on destroy).
- **Versioning:** version lives in 4 places — see [`release-export-guide.md`](release-export-guide.md).

## User-facing docs

GitBook pages in `Docs/usage/` (one per feature) + `Docs/animation.md`. TOC in `Docs/SUMMARY.md`.
Keep those friendly/non-technical; keep this file terse/technical.
