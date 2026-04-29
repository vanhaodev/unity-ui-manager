using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace vanhaodev.uimanager
{
    public enum MarqueeMode
    {
        Loop,
        PingPong
    }

    public class MarqueeText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private MarqueeMode _mode = MarqueeMode.Loop;
        [SerializeField] private float _speed = 150f;
        [SerializeField] private int _delayMs = 1000;

        private RectTransform _container;
        private RectTransform _textRect;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _container = transform as RectTransform;
            if (_text != null)
            {
                _textRect = _text.transform as RectTransform;
                _text.textWrappingMode = TextWrappingModes.NoWrap;
                _text.overflowMode = TextOverflowModes.Overflow;

                _textRect.anchorMin = new Vector2(0f, 0.5f);
                _textRect.anchorMax = new Vector2(0f, 0.5f);
                _textRect.pivot = new Vector2(0f, 0.5f);
            }
        }

        private void OnEnable()
        {
            Play();
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }

        public void Play()
        {
            Stop();
            _cts = new CancellationTokenSource();
            _ = RunMarqueeAsync(_cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void SetText(string content)
        {
            if (_text != null)
                _text.text = content;
            Play();
        }

        private async Task RunMarqueeAsync(CancellationToken ct)
        {
            await Task.Yield();
            if (ct.IsCancellationRequested || _text == null || _textRect == null) return;

            _text.ForceMeshUpdate();

            float containerWidth = _container.rect.width;
            float textWidth = _text.preferredWidth;

            _textRect.sizeDelta = new Vector2(textWidth, _textRect.sizeDelta.y);

            if (textWidth <= containerWidth)
            {
                SetPosition(0f);
                return;
            }

            float overflow = textWidth - containerWidth;

            if (_mode == MarqueeMode.PingPong)
            {
                await RunPingPongAsync(overflow, ct);
            }
            else
            {
                await RunLoopAsync(containerWidth, textWidth, ct);
            }
        }

        private async Task RunPingPongAsync(float overflow, CancellationToken ct)
        {
            float startX = 0f;
            float endX = -overflow;

            SetPosition(startX);
            int direction = -1;

            while (!ct.IsCancellationRequested && this != null && gameObject.activeInHierarchy)
            {
                await Task.Delay(_delayMs, ct);
                if (ct.IsCancellationRequested) break;

                float target = direction < 0 ? endX : startX;
                await MoveAsync(target, direction, ct);
                if (ct.IsCancellationRequested) break;

                direction *= -1;
            }
        }

        private async Task RunLoopAsync(float containerWidth, float textWidth, CancellationToken ct)
        {
            float startX = 0f;
            float exitX = -textWidth;
            float enterX = containerWidth;

            SetPosition(startX);

            // Delay only on first iteration
            await Task.Delay(_delayMs, ct);
            if (ct.IsCancellationRequested) return;

            while (!ct.IsCancellationRequested && this != null && gameObject.activeInHierarchy)
            {
                // Scroll left until completely exit
                await MoveAsync(exitX, -1, ct);
                if (ct.IsCancellationRequested) break;

                // Instantly move to right side (off-screen)
                SetPosition(enterX);

                // Scroll left back to start (continuous, no delay)
                await MoveAsync(startX, -1, ct);
                if (ct.IsCancellationRequested) break;
            }
        }

        private async Task MoveAsync(float target, int direction, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && this != null)
            {
                var pos = _textRect.anchoredPosition;
                pos.x += direction * _speed * Time.deltaTime;

                bool reached = direction < 0 ? pos.x <= target : pos.x >= target;
                if (reached)
                {
                    pos.x = target;
                    _textRect.anchoredPosition = pos;
                    break;
                }

                _textRect.anchoredPosition = pos;
                await Task.Yield();
            }
        }

        private void SetPosition(float x)
        {
            var pos = _textRect.anchoredPosition;
            pos.x = x;
            _textRect.anchoredPosition = pos;
        }
    }
}
