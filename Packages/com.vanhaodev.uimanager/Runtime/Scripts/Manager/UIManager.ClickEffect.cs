using com.vanhaodev.objectpool;
using UnityEngine;
using vanhaodev.uimanager.effect;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace vanhaodev.uimanager
{
    public partial class UIManager
    {
        [SerializeField] private Transform _clickEffectLayer;

        private ObjectPool<BaseClickEffect> _clickEffectPool;
        private bool _clickEffectInitialized;
        private float _lastClickEffectTime;

        /// <summary>
        /// Play the click effect at a screen position. Safe to call from your own input handling.
        /// The pool and effect instances are created lazily on the first call, so the feature costs
        /// nothing until it is actually used.
        /// </summary>
        public void PlayClickEffect(Vector2 screenPos)
        {
            if (!EnsureClickEffectInitialized()) return;

            var fx = _clickEffectPool.Get();
            if (fx == null) return;

            PositionAtScreen(fx.RectTransform, screenPos);
            fx.Play(() => _clickEffectPool?.Release(fx));
        }

        // Auto-play on any screen press when enabled. Reads the pointer device directly (not the
        // EventSystem), so it fires anywhere — including over buttons — without blocking the UI.
        private void Update()
        {
            var config = _library != null ? _library.ClickEffectConfig : null;
            if (config == null || config.Prefab == null || !config.ActiveFeature) return;

            DetectClickInput(config);
        }

#if ENABLE_INPUT_SYSTEM
        // New Input System (also the path when Active Input Handling = "Both"): covers mouse (PC),
        // touch (mobile, multi-finger) and pen on every platform.
        private void DetectClickInput(ClickEffectConfig config)
        {
            var touchscreen = Touchscreen.current;
            bool firedTouch = false;
            if (touchscreen != null)
            {
                foreach (var touch in touchscreen.touches)
                {
                    if (touch.press.wasPressedThisFrame && PassThrottle(config))
                    {
                        PlayClickEffect(touch.position.ReadValue());
                        firedTouch = true;
                    }
                }
            }

            // Fall back to the active pointer (mouse / pen / single touch) only if no touch began.
            if (!firedTouch)
            {
                var pointer = Pointer.current;
                if (pointer != null && pointer.press.wasPressedThisFrame && PassThrottle(config))
                    PlayClickEffect(pointer.position.ReadValue());
            }
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        // Legacy Input Manager: mouse + touch.
        private void DetectClickInput(ClickEffectConfig config)
        {
            if (Input.GetMouseButtonDown(0) && PassThrottle(config))
                PlayClickEffect(Input.mousePosition);

            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && PassThrottle(config))
                    PlayClickEffect(touch.position);
            }
        }
#else
        // No input backend enabled — auto-play is unavailable; use PlayClickEffect manually.
        private void DetectClickInput(ClickEffectConfig config) { }
#endif

        private bool PassThrottle(ClickEffectConfig config)
        {
            if (config.MinInterval <= 0f) return true;

            float now = Time.unscaledTime;
            if (now - _lastClickEffectTime < config.MinInterval) return false;

            _lastClickEffectTime = now;
            return true;
        }

        private bool EnsureClickEffectInitialized()
        {
            if (_clickEffectInitialized) return true;

            if (_clickEffectLayer == null)
            {
                Debug.LogError("[UIManager] ClickEffect layer not assigned");
                return false;
            }

            var prefab = _library?.ClickEffectConfig?.Prefab;
            if (prefab == null)
            {
                Debug.LogError("[UIManager] ClickEffect prefab not set in UILibrary");
                return false;
            }

            _clickEffectPool = new ObjectPool<BaseClickEffect>(
                factory: () =>
                {
                    var inst = Instantiate(prefab, _clickEffectLayer);
                    inst.gameObject.SetActive(false);
                    return inst;
                },
                initialSize: 0,
                onGet: fx =>
                {
                    if (fx == null) return;
                    fx.ResetState();
                    fx.gameObject.SetActive(true);
                },
                onRelease: fx =>
                {
                    if (fx == null) return;
                    AnimationHelper.CancelAndRemove(fx);
                    fx.gameObject.SetActive(false);
                    fx.transform.SetParent(_clickEffectLayer, false);
                },
                onDestroy: fx =>
                {
                    if (fx != null) Destroy(fx.gameObject);
                });

            _clickEffectInitialized = true;
            return true;
        }

        // Convert a screen point to a local position on the click effect layer. Uses the layer's own
        // canvas camera (shared CameraOf helper) so it lands correctly under any render mode.
        private void PositionAtScreen(RectTransform rect, Vector2 screenPos)
        {
            var layerRect = _clickEffectLayer as RectTransform;
            if (layerRect == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                layerRect, screenPos, CameraOf(layerRect), out var local);
            rect.anchoredPosition = local;
        }

        private void ClearClickEffectCache()
        {
            _clickEffectPool?.Clear(includeActive: true);
            _clickEffectPool = null;
            _clickEffectInitialized = false;
        }
    }
}
