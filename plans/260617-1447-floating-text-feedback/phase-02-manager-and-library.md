# Phase 02 — Manager Partial + Library Wiring

## Overview
- Priority: High
- Status: Not started
- Depends on: phase-01
- Wire FloatingText into UIManager (pool + API + position conversion) and UILibrary.

## Files to create
- `Runtime/Scripts/Manager/UIManager.FloatingText.cs` — partial class

## Files to modify
- `Runtime/Scripts/Data/UILibrary.cs` — register floating text prefabs
- `Runtime/Scripts/Manager/UIManager.cs` — add `ClearFloatingTextCache()` to `ClearCache()`

## UILibrary changes
- Add `[SerializeField] private List<FloatingText> _floatingTexts = new();`
- Add `_floatingTextCache` dict, populate in `BuildCache()`.
- Add `public T GetFloatingTextPrefab<T>() where T : FloatingText`.

## UIManager.FloatingText.cs
Mirror `UIManager.Toast.cs` structure (simpler — no queue, no id, no concurrent limit):
- `[SerializeField] private Transform _floatingTextLayer;`
- `private readonly Dictionary<Type, ObjectPool<FloatingText>> _floatingTextPools = new();`

### Public API
```csharp
// Default template, Transform anchor (UI element) — convenience, data only
public void ShowFloatingText(string message, Transform anchor)
    => ShowFloatingText<FloatingTextDefault>(t => t.SetText(message), anchor);

// Default template, screen-space point
public void ShowFloatingText(string message, Vector2 screenPosition)
    => ShowFloatingText<FloatingTextDefault>(t => t.SetText(message), screenPosition);

// Custom template, Transform anchor
public void ShowFloatingText<T>(Action<T> onSetup, Transform anchor) where T : FloatingText
    => ShowFloatingTextInternal(onSetup, WorldToScreen(anchor));

// Custom template, screen point
public void ShowFloatingText<T>(Action<T> onSetup, Vector2 screenPosition) where T : FloatingText
    => ShowFloatingTextInternal(onSetup, screenPosition);
```
Return `void` (caller ignores — fire-and-forget). KISS.

### Internal flow — stays generic end-to-end (NO reflection, unlike Toast)
> Toast reflects `Type -> prefab` only because its **queue** erases the generic type.
> FloatingText has no queue, so `<T>` is never lost — resolve the prefab directly:
```csharp
private void ShowFloatingTextInternal<T>(Action<T> onSetup, Vector2 screenPos) where T : FloatingText
{
    var prefab = _library != null ? _library.GetFloatingTextPrefab<T>() : null;   // generic, no reflection
    if (prefab == null) { Debug.LogError($"[UIManager] FloatingText not found: {typeof(T).Name}"); return; }

    var ft = (T)AcquireFloatingText(typeof(T), prefab);   // pool keyed by typeof(T); factory Instantiate(prefab)
    var rect = ft.transform as RectTransform;
    rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = ScreenToLayerLocal(screenPos);

    onSetup?.Invoke(ft);
    ft.Play(() => ReleaseFloatingText(ft));
}
```
- `AcquireFloatingText(Type type, FloatingText prefab)` — same pool pattern as `AcquireToast`,
  but the prefab is passed in (no `GetXxxPrefabByType` reflection helper needed).

### Position conversion (the tricky part)
```csharp
private Vector2 WorldToScreen(Transform anchor)
{
    var layerRect = _floatingTextLayer as RectTransform;
    var canvas = layerRect.GetComponentInParent<Canvas>();
    Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
    return RectTransformUtility.WorldToScreenPoint(cam, anchor.position);
}

private Vector2 ScreenToLayerLocal(Vector2 screenPos)
{
    var layerRect = _floatingTextLayer as RectTransform;
    var canvas = layerRect.GetComponentInParent<Canvas>();
    Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(layerRect, screenPos, cam, out var local);
    return local; // == anchoredPosition when layer is full-rect, item anchored+pivot center
}
```
- Transform overload assumes a **UI/canvas** anchor. For world-space gameplay objects, caller uses the
  screen-point overload with `Camera.main.WorldToScreenPoint(obj.position)`. Document in XML summary.

### Cleanup
- `ClearFloatingTextCache()` — clear pools (`includeActive: true`), clear dict. Call from `UIManager.ClearCache()`.

## Todo
- [ ] UILibrary: list + cache + getter
- [ ] UIManager.FloatingText.cs: layer, pool, acquire/release
- [ ] Public API overloads (Transform + Vector, default + generic)
- [ ] WorldToScreen + ScreenToLayerLocal
- [ ] Hook ClearFloatingTextCache into ClearCache

## Success criteria
- Compiles. Text spawns at button position in ScreenSpaceOverlay and ScreenSpaceCamera canvases.
- Pool reuses instances (no garbage spam when spamming).

## Risks
- ScreenSpaceCamera/World canvas → must pass canvas.worldCamera (handled).
- `_floatingTextLayer` not full-rect/centered → offset. Mitigate via prefab/layer setup notes (phase-03).
