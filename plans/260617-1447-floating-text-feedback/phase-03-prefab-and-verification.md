# Phase 03 — Prefab/Layer Setup + Verification

## Overview
- Priority: Medium
- Status: Not started
- Depends on: phase-01, phase-02
- Editor-side setup notes + compile/manual verification. Mostly guidance (prefab is authored in Unity).

## Layer setup (`_floatingTextLayer`)
- A `RectTransform` child of the UI canvas, **above** popup/toast layers (renders on top).
- Stretch full-rect: anchorMin (0,0), anchorMax (1,1), offsets 0; pivot (0.5,0.5).
- No `Image`/raycast target. Should not block input.

## Default prefab (`FloatingTextDefault`)
- Root: `FloatingTextDefault` + `CanvasGroup` (blocksRaycasts=false, interactable=false).
- Child `TMP_Text` (assigned to `_text`), centered, raycast target OFF.
- Anchor/pivot center (manager sets anchoredPosition).
- Add to `UILibrary._floatingTexts`.

## Verification
1. Compile: open Unity, confirm no errors (or `dotnet build` on the .slnx if it resolves UnityEngine refs — likely Unity-only; rely on Editor compile).
2. Manual: hook a test button →
   `uiManager.ShowFloatingText("Not enough money", btn.transform, Color.red);`
   Expect: red text rises ~80px from button, fades out in ~1s, auto-removed.
3. Spam test: click rapidly → many texts, pooled, no leak, no input blocked.
4. Canvas modes: test ScreenSpaceOverlay (cam=null path) and ScreenSpaceCamera (worldCamera path).

## Todo
- [ ] Author layer + assign `_floatingTextLayer`
- [ ] Author default prefab + register in UILibrary
- [ ] Compile clean in Unity Editor
- [ ] Manual + spam + canvas-mode checks

## Success criteria
- Feature works end-to-end from a single `ShowFloatingText` call.
- No GC spam, no input blocking, correct position across canvas render modes.

## Open questions
- Where exactly should the floating layer sit in the existing canvas hierarchy? (confirm with author)
- Pooling prewarm size — keep 0 (lazy) like toasts? Assumed yes.
