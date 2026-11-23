using System;
using System.Collections.Generic;
using com.vanhaodev.objectpool;
using UnityEngine;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager
{
    public partial class UIManager
    {
        [SerializeField] private Transform _flyoutEffectLayer;

        private FlyoutRegistry _flyoutRegistry;
        private ObjectPool<FlyoutIcon> _flyoutPool;
        private FlyoutAnimator _flyoutAnimator;
        private bool _flyoutInitialized;
        private readonly Dictionary<string, List<FlyoutIcon>> _activeIconsPerTarget = new();

        /// <summary>Registry for FlyoutTarget components.</summary>
        public FlyoutRegistry FlyoutRegistry => _flyoutRegistry ??= new FlyoutRegistry();

        /// <summary>Animator instance. Replace for custom animations.</summary>
        public FlyoutAnimator FlyoutAnimator
        {
            get => _flyoutAnimator ??= new FlyoutAnimator();
            set => _flyoutAnimator = value;
        }

        /// <summary>
        /// Spawn flying icons from a world position to a registered <see cref="FlyoutTarget"/>
        /// (looked up by <paramref name="targetKey"/>). Icons are purely visual: they make the
        /// target shake on arrival but do NOT change its displayed number.
        /// </summary>
        /// <remarks>
        /// The app owns the number — drive the count-up by subscribing to
        /// <see cref="FlyoutTarget.OnIconArrived"/> and bumping the target by each icon's value as it
        /// lands (via <see cref="FlyoutTarget.SetTargetValue"/>); the number rises in lockstep with
        /// the icons. Use <paramref name="onComplete"/> for side effects only (e.g. persisting the
        /// gain). Set <see cref="FlyoutTarget.Formatter"/> once to control the number style (e.g. "12.6k").
        /// </remarks>
        /// <param name="sourceWorldPos">World-space spawn origin (e.g. the source button position).</param>
        /// <param name="targetKey">Key of the registered FlyoutTarget to fly toward.</param>
        /// <param name="amount">Total value of the batch; split evenly across the spawned icons.</param>
        /// <param name="icon">Sprite used by each flying icon.</param>
        /// <param name="onComplete">Invoked once, after all icons in the batch have arrived.</param>
        public void PlayFlyout(
            Vector3 sourceWorldPos,
            string targetKey,
            int amount,
            Sprite icon,
            Action onComplete = null)
        {
            if (!FlyoutRegistry.TryGet(targetKey, out var target))
            {
                Debug.LogWarning($"[UIManager] FlyoutTarget not found: {targetKey}");
                return;
            }

            PlayFlyoutInternal(sourceWorldPos, target, amount, icon, onComplete);
        }

        /// <summary>
        /// Same as the keyed overload, but flies toward a direct <see cref="FlyoutTarget"/> reference.
        /// See that overload's remarks for the app-driven count-up contract.
        /// </summary>
        public void PlayFlyout(
            Vector3 sourceWorldPos,
            FlyoutTarget target,
            int amount,
            Sprite icon,
            Action onComplete = null)
        {
            if (target == null)
            {
                Debug.LogWarning("[UIManager] FlyoutTarget is null");
                return;
            }

            PlayFlyoutInternal(sourceWorldPos, target, amount, icon, onComplete);
        }

        /// <summary>
        /// Play flyout effect from screen position to registered target.
        /// </summary>
        public void PlayFlyoutFromScreen(
            Vector2 screenPos,
            string targetKey,
            int amount,
            Sprite icon,
            Action onComplete = null)
        {
            if (!FlyoutRegistry.TryGet(targetKey, out var target))
            {
                Debug.LogWarning($"[UIManager] FlyoutTarget not found: {targetKey}");
                return;
            }

            PlayFlyoutInternalFromScreen(screenPos, target, amount, icon, onComplete);
        }

        /// <summary>
        /// Play flyout from a UI element's <see cref="RectTransform"/> (e.g. the purchased item's
        /// icon) toward a registered target. The library resolves the source element's own canvas
        /// camera, so icons always start from where the element sits on screen — correct for Overlay,
        /// Screen-Space Camera, World-Space, and nested/cross-canvas setups. Prefer this over the
        /// screen/world overloads when the source is a UI element: the caller passes a RectTransform
        /// and the library does all the camera math.
        /// </summary>
        public void PlayFlyoutFromRect(
            RectTransform source,
            string targetKey,
            int amount,
            Sprite icon,
            Action onComplete = null)
        {
            if (source == null)
            {
                Debug.LogWarning("[UIManager] Flyout source RectTransform is null");
                return;
            }
            if (!FlyoutRegistry.TryGet(targetKey, out var target))
            {
                Debug.LogWarning($"[UIManager] FlyoutTarget not found: {targetKey}");
                return;
            }

            PlayFlyoutInternalFromScreen(RectToScreen(source), target, amount, icon, onComplete);
        }

        /// <summary>
        /// Same as the keyed overload, but flies toward a direct <see cref="FlyoutTarget"/> reference.
        /// </summary>
        public void PlayFlyoutFromRect(
            RectTransform source,
            FlyoutTarget target,
            int amount,
            Sprite icon,
            Action onComplete = null)
        {
            if (source == null)
            {
                Debug.LogWarning("[UIManager] Flyout source RectTransform is null");
                return;
            }
            if (target == null)
            {
                Debug.LogWarning("[UIManager] FlyoutTarget is null");
                return;
            }

            PlayFlyoutInternalFromScreen(RectToScreen(source), target, amount, icon, onComplete);
        }

        // Screen position of a UI element via its OWN canvas camera (rt.position == pivot world pos).
        private static Vector2 RectToScreen(RectTransform rt)
            => RectTransformUtility.WorldToScreenPoint(CameraOf(rt), rt.position);

        private void PlayFlyoutInternal(
            Vector3 sourceWorldPos,
            FlyoutTarget target,
            int amount,
            Sprite icon,
            Action onComplete)
        {
            var screenPos = WorldToScreenPoint(sourceWorldPos);
            PlayFlyoutInternalFromScreen(screenPos, target, amount, icon, onComplete);
        }

        private void PlayFlyoutInternalFromScreen(
            Vector2 screenPos,
            FlyoutTarget target,
            int amount,
            Sprite icon,
            Action onComplete)
        {
            EnsureFlyoutInitialized();
            if (_flyoutPool == null) return;

            SpawnFlyoutIcons(screenPos, target, amount, icon, onComplete);
        }

        private void EnsureFlyoutInitialized()
        {
            if (_flyoutInitialized) return;
            if (_flyoutEffectLayer == null)
            {
                Debug.LogError("[UIManager] FlyoutLayer not assigned");
                return;
            }

            var config = _library?.FlyoutConfig;
            if (config == null)
            {
                Debug.LogError("[UIManager] FlyoutConfig not set in UILibrary");
                return;
            }

            var prefab = _library?.FlyoutIconPrefab;
            if (prefab == null)
            {
                Debug.LogError("[UIManager] FlyoutIconPrefab not set in UILibrary");
                return;
            }

            _flyoutPool = new ObjectPool<FlyoutIcon>(
                factory: () =>
                {
                    var inst = Instantiate(prefab, _flyoutEffectLayer);
                    inst.gameObject.SetActive(false);
                    return inst;
                },
                initialSize: 0,
                onGet: icon =>
                {
                    if (icon == null) return;
                    icon.ResetState();
                    icon.gameObject.SetActive(true);
                },
                onRelease: icon =>
                {
                    if (icon == null) return;
                    AnimationHelper.CancelAndRemove(icon);
                    icon.gameObject.SetActive(false);
                    icon.transform.SetParent(_flyoutEffectLayer, false);
                },
                onDestroy: icon =>
                {
                    if (icon != null) Destroy(icon.gameObject);
                });

            _flyoutInitialized = true;
        }

        private void SpawnFlyoutIcons(
            Vector2 screenPos,
            FlyoutTarget target,
            int amount,
            Sprite icon,
            Action onComplete)
        {
            var targetKey = target?.Key ?? "";

            // Recall existing icons and combine their remaining value with new amount
            if (!string.IsNullOrEmpty(targetKey) && _activeIconsPerTarget.TryGetValue(targetKey, out var existingIcons))
            {
                // Copy list to avoid collection modified exception
                var iconsToRecall = new List<FlyoutIcon>(existingIcons);
                existingIcons.Clear();

                var recalledValue = 0;
                foreach (var existingIcon in iconsToRecall)
                {
                    if (existingIcon != null && existingIcon.gameObject.activeInHierarchy)
                    {
                        recalledValue += existingIcon.ValuePerIcon;
                        _flyoutPool.Release(existingIcon);
                    }
                }

                // Add recalled value to new amount
                amount += recalledValue;
            }

            var config = _library.FlyoutConfig;

            var sourceLocalPos = ScreenToFlyoutLocal(screenPos);
            var targetLocalPos = GetTargetLocalPos(target, config);

            var iconCount = CalculateIconCount(amount, config);
            if (iconCount <= 0)
            {
                onComplete?.Invoke();
                return;
            }

            // Create or get icon list for this target
            if (!_activeIconsPerTarget.TryGetValue(targetKey, out var iconList))
            {
                iconList = new List<FlyoutIcon>();
                if (!string.IsNullOrEmpty(targetKey))
                    _activeIconsPerTarget[targetKey] = iconList;
            }

            var baseValue = amount / iconCount;
            var remainder = amount % iconCount;
            var completedCount = 0;

            for (int i = 0; i < iconCount; i++)
            {
                var flyoutIcon = _flyoutPool.Get();
                if (flyoutIcon == null) continue;

                // Track this icon
                iconList.Add(flyoutIcon);

                // Distribute remainder across first N icons (1 extra each)
                var valueForThisIcon = baseValue + (i < remainder ? 1 : 0);

                flyoutIcon.SetSprite(icon);
                flyoutIcon.SourcePosition = sourceLocalPos;
                flyoutIcon.Target = target;
                flyoutIcon.ValuePerIcon = valueForThisIcon;
                flyoutIcon.Index = i;
                flyoutIcon.TotalCount = iconCount;

                PlaySingleIconAsync(flyoutIcon, targetLocalPos, i * config.SpawnStagger, () =>
                {
                    completedCount++;

                    // Remove from tracking
                    iconList.Remove(flyoutIcon);

                    if (completedCount >= iconCount)
                    {
                        // Clear tracking list
                        if (!string.IsNullOrEmpty(targetKey))
                            _activeIconsPerTarget.Remove(targetKey);

                        target?.NotifyBatchComplete();
                        onComplete?.Invoke();
                    }
                }).Forget();
            }
        }

        private async Awaitable PlaySingleIconAsync(
            FlyoutIcon icon,
            Vector2 targetPos,
            float delay,
            Action onComplete)
        {
            var ct = AnimationHelper.ResetToken(icon);
            var config = _library.FlyoutConfig;
            var animator = FlyoutAnimator;

            try
            {
                if (delay > 0)
                    await Awaitable.WaitForSecondsAsync(delay, ct);

                await animator.BurstAsync(icon, icon.SourcePosition, config, ct);
                await animator.FlightAsync(icon, targetPos, config, ct);

                icon.OnReachTarget();

                await animator.ImpactAsync(icon, config, ct);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _flyoutPool?.Release(icon);
                onComplete?.Invoke();
            }
        }

        private Vector2 ScreenToFlyoutLocal(Vector2 screenPos)
        {
            var layerRect = _flyoutEffectLayer as RectTransform;
            if (layerRect == null) return screenPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                layerRect, screenPos, GetFlyoutCamera(), out var local);
            return local;
        }

        private Vector2 GetTargetLocalPos(FlyoutTarget target, FlyoutConfig config)
        {
            if (target == null || _flyoutEffectLayer == null) return Vector2.zero;

            var targetRect = target.RectTransform;
            var layerRect = _flyoutEffectLayer as RectTransform;
            if (targetRect == null || layerRect == null) return Vector2.zero;

            // Aim point may be the amount text (centre or alignment-chosen edge) or an explicit
            // landing transform, so icons hit the number even in a wide field — not the rect centre.
            var worldPos = target.GetAimWorldPosition(config);
            // World→screen uses the TARGET's canvas camera (it may live on a different canvas);
            // screen→local uses the FLYOUT layer's camera.
            var screenPos = RectTransformUtility.WorldToScreenPoint(CameraOf(targetRect), worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                layerRect, screenPos, GetFlyoutCamera(), out var local);
            return local;
        }

        // Camera of the flyout layer's own canvas — used for screen→local conversions on this layer.
        private Camera GetFlyoutCamera() => CameraOf(_flyoutEffectLayer as RectTransform);

        // Resolve the render camera of the canvas that CONTAINS <paramref name="rt"/>.
        // Always go through rootCanvas: a nested canvas keeps a serialized renderMode but holds no
        // worldCamera (only the root does), so the nearest Canvas can report Camera mode yet a null
        // camera. Overlay canvases use a null camera; camera/world canvases need their render camera.
        // World→screen for any object must use the camera of ITS canvas, not the flyout layer's.
        private static Camera CameraOf(RectTransform rt)
        {
            var root = rt != null ? rt.GetComponentInParent<Canvas>()?.rootCanvas : null;
            if (root == null || root.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;
            return root.worldCamera;
        }

        private static int CalculateIconCount(int amount, FlyoutConfig config)
        {
            if (amount <= 0) return 0;

            if (amount <= config.DirectCountMax)
                return amount;

            if (amount < 100)
            {
                var baseCount = config.DirectCountMax;
                var extra = (amount - config.DirectCountMax) / 10 * config.ScaledCountPer10;
                return Mathf.Min(baseCount + extra, config.MaxIcons);
            }

            return config.MaxIcons;
        }

        private void ClearFlyoutCache()
        {
            _flyoutPool?.Clear(includeActive: true);
            _flyoutPool = null;
            _flyoutRegistry?.Clear();
            _activeIconsPerTarget.Clear();
            _flyoutInitialized = false;
        }
    }
}
