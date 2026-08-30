using TMPro;
using UnityEngine;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Rolls a sine wave through a label's glyphs: each character rides the same wave a little later than the
    /// one before it, so the string appears to ripple. Drives the mesh directly rather than the transform,
    /// which is what lets the letters move independently of each other.
    ///
    /// Lives on the TMP object itself and drives that one label. The wave is purely cosmetic — it never
    /// changes the text's layout box, so a label that is fitted or centred stays where it was put.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class WaveText : MonoBehaviour
    {
        [Header("Wave")]
        [Tooltip("How fast the wave travels. Higher = quicker ripple.")]
        [SerializeField] private float _speed = 1.5f;

        [Tooltip("How far a glyph rides above and below the baseline, in local units.")]
        [SerializeField] private float _amplitude = 3f;

        [Tooltip("Phase pushed onto each successive glyph. Higher = tighter wave, more letters per crest.")]
        [SerializeField] private float _frequency = 0.6f;

        private TMP_Text _text;
        private bool _isPlaying = true;

        private void Awake() => _text = GetComponent<TMP_Text>();

        /// <summary>Start rippling, optionally swapping the string first.</summary>
        public void Play(string content = null)
        {
            if (!string.IsNullOrEmpty(content) && _text != null)
                _text.text = content;

            _isPlaying = true;
        }

        /// <summary>
        /// Stop rippling and settle the glyphs back onto the baseline. The mesh is rebuilt once on the way
        /// out: the offsets are baked into vertices, so simply halting would leave the last frame's wave
        /// frozen into the label.
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying) return;

            _isPlaying = false;
            if (_text != null) _text.ForceMeshUpdate();
        }

        private void Update()
        {
            if (!_isPlaying || _text == null) return;

            // Regenerated every frame on purpose: the offsets below are written straight into the vertex
            // buffer, so each frame has to start from the clean, un-waved mesh or the glyphs would drift.
            _text.ForceMeshUpdate();

            var textInfo = _text.textInfo;
            var meshInfo = textInfo.meshInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue; // spaces carry no quad

                float offsetY = Mathf.Sin(Time.time * _speed + i * _frequency) * _amplitude;
                var offset = new Vector3(0f, offsetY, 0f);

                // Each glyph is one quad: four consecutive vertices, moved together.
                var vertices = meshInfo[charInfo.materialReferenceIndex].vertices;
                int vertexIndex = charInfo.vertexIndex;
                vertices[vertexIndex + 0] += offset;
                vertices[vertexIndex + 1] += offset;
                vertices[vertexIndex + 2] += offset;
                vertices[vertexIndex + 3] += offset;
            }

            // One push per material — a label with a fallback font or a sprite spans more than one mesh.
            for (int i = 0; i < meshInfo.Length; i++)
            {
                meshInfo[i].mesh.vertices = meshInfo[i].vertices;
                _text.UpdateGeometry(meshInfo[i].mesh, i);
            }
        }
    }
}
