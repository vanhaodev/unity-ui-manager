using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// Helper to load images from URL into an UI Image via UnityWebRequest.
    /// Disables the target Image GameObject if URL is empty or load fails.
    /// </summary>
    public static class ImageLoader
    {
        // Sprite cache so repeated loads of the same URL don't re-download (used by spam tests).
        private static readonly Dictionary<string, Sprite> _spriteCache = new();

        /// <summary>
        /// Loads (and caches) a Sprite from URL. Cached URLs invoke <paramref name="onLoaded"/>
        /// synchronously; otherwise it downloads once. Passes null on empty URL or failure.
        /// </summary>
        public static void LoadSprite(MonoBehaviour host, string url, Action<Sprite> onLoaded)
        {
            if (string.IsNullOrWhiteSpace(url)) { onLoaded?.Invoke(null); return; }
            if (_spriteCache.TryGetValue(url, out var cached)) { onLoaded?.Invoke(cached); return; }
            if (host == null || !host.gameObject.activeInHierarchy) { onLoaded?.Invoke(null); return; }

            host.StartCoroutine(LoadSpriteRoutine(url, onLoaded));
        }

        private static IEnumerator LoadSpriteRoutine(string url, Action<Sprite> onLoaded)
        {
            using var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[ImageLoader] Failed to load '{url}': {request.error}");
                onLoaded?.Invoke(null);
                yield break;
            }

            var texture = DownloadHandlerTexture.GetContent(request);
            var sprite = Sprite.Create(
                texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            _spriteCache[url] = sprite;
            onLoaded?.Invoke(sprite);
        }

        /// <summary>
        /// Loads an image from URL into the target Image component.
        /// Needs a MonoBehaviour host to run the coroutine.
        /// </summary>
        public static void Load(MonoBehaviour host, Image target, string url)
        {
            if (target == null || host == null) return;
            if (!host.gameObject.activeInHierarchy)
            {
                Debug.LogError("[ImageLoader] Host GameObject must be active to load image.");
                return;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                target.gameObject.SetActive(false);
                return;
            }

            host.StartCoroutine(LoadRoutine(target, url));
        }

        private static IEnumerator LoadRoutine(Image target, string url)
        {
            using var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (target == null) yield break;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[ImageLoader] Failed to load '{url}': {request.error}");
                target.gameObject.SetActive(false);
                yield break;
            }

            var texture = DownloadHandlerTexture.GetContent(request);
            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            target.sprite = sprite;
            target.gameObject.SetActive(true);
        }
    }
}
