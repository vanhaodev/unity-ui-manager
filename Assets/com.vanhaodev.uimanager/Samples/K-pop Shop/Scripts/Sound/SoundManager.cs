using System.Collections.Generic;
using UnityEngine;

namespace vanhaodev.uimanager.samples.kpopshop
{
    public class SoundManager : MonoBehaviour
    {
        [Header("Sound Database")] [SerializeField]
        private List<SoundData> _sounds = new();

        private Dictionary<string, AudioClip> _soundMap;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();

            BuildMap();
        }

        private void BuildMap()
        {
            _soundMap = new Dictionary<string, AudioClip>();

            foreach (var s in _sounds)
            {
                if (string.IsNullOrEmpty(s.Key) || s.Clip == null)
                    continue;

                if (!_soundMap.ContainsKey(s.Key))
                    _soundMap.Add(s.Key, s.Clip);
            }
        }

        // ===== PLAY =====
        public void Play(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (_soundMap.TryGetValue(key, out var clip))
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        public void PlayLoop(string key, float volume = 1)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (_soundMap.TryGetValue(key, out var clip))
            {
                _audioSource.clip = clip;
                _audioSource.volume = volume;
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }
    }
}