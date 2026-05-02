using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager.samples.kpopshop.animation
{
   public class TempLBAnimationDefault : LoadingBlockAnimationMonoBase
    {
        [Header("References")]
        [SerializeField] private TMP_Text _text;

        [Header("Fade Settings")]
        [SerializeField] private float _fadeDuration = 0.25f;

        [Header("Wave Settings")]
        [SerializeField] private float _waveSpeed = 1.5f;
        [SerializeField] private float _waveAmplitude = 3f;
        [SerializeField] private float _waveFrequency = 0.6f;

        private CanvasGroup _canvasGroup;
        private bool _isPlaying;

        private void Awake()
        {
            if (!TryGetComponent(out _canvasGroup))
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public override void PlayShow(GameObject target, Action onComplete)
        {
            base.PlayShow(target, onComplete);

            _canvasGroup.alpha = 0f;
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(1f, _fadeDuration).OnComplete(() =>
            {
                _isPlaying = true;
                onComplete?.Invoke();
            });
        }

        public override void PlayClose(GameObject target, Action onComplete)
        {
            base.PlayClose(target, onComplete);

            _isPlaying = false;
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0f, _fadeDuration).OnComplete(() => onComplete?.Invoke());
        }

        private void Update()
        {
            if (!_isPlaying || _text == null) return;

            _text.ForceMeshUpdate();

            var textInfo = _text.textInfo;
            var meshInfo = textInfo.meshInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                float offsetY = Mathf.Sin(Time.time * _waveSpeed + i * _waveFrequency) * _waveAmplitude;
                var offset = new Vector3(0, offsetY, 0);

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Vector3[] vertices = meshInfo[materialIndex].vertices;
                vertices[vertexIndex + 0] += offset;
                vertices[vertexIndex + 1] += offset;
                vertices[vertexIndex + 2] += offset;
                vertices[vertexIndex + 3] += offset;
            }

            for (int i = 0; i < meshInfo.Length; i++)
            {
                meshInfo[i].mesh.vertices = meshInfo[i].vertices;
                _text.UpdateGeometry(meshInfo[i].mesh, i);
            }
        }
    }
}