using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Keeps a label's box the size of the string inside it, and owns what happens when the string
    /// stops fitting.
    ///
    /// Lives on the TMP object itself and drives that one label. The string spends width first, then
    /// height, and only when neither is left does the font give way:
    ///   short string   — the box hugs the text at Font Size Max
    ///   too wide       — the box stops at Max Width, the string wraps, and the box deepens to follow
    ///   too tall as well— the box stops at Max Height and the font shrinks toward Font Size Min
    ///   still too big  — TMP ellipsizes at the minimum size
    ///
    /// The middle rung is Fit Vertical, and it is opt-in. With it off the height stays as authored and
    /// there is nowhere for a long string to go, so Max Width leads straight to shrinking the font.
    ///
    /// The font settings are set from here rather than read: with the box and the font both adjusting
    /// to each other, one of them has to be in charge, and splitting that between a component and the
    /// TMP inspector is what makes the pair chase each other. Fit Vertical borrows word wrapping for the
    /// same reason — a label authored to never wrap has no way to spend the height it is offered — and
    /// hands it back the moment it is switched off, the way the ellipsis is handed back.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextFitter : MonoBehaviour
    {
        [Tooltip("Widest the box may get. The font starts shrinking here.")]
        [SerializeField] private float _maxWidth = 200f;

        [Header("Font size range")]
        [SerializeField] private float _fontSizeMax = 36f;
        [SerializeField] private float _fontSizeMin = 18f;

        [Header("Vertical fit")]
        [Tooltip("Let the box spend height once Max Width is used up: the string wraps and the box deepens "
                 + "to follow it. Off, the height stays as authored and a long string goes straight to "
                 + "shrinking the font. Takes over the label's word wrapping while on.")]
        [SerializeField] private bool _fitVertical;

        [Tooltip("Tallest the box may get. Only once this AND Max Width are both used up does the font "
                 + "start shrinking.")]
        [SerializeField] private float _maxHeight = 200f;

        [Tooltip("Ellipsize once the font has shrunk as far as it may and the string still does not fit.")]
        [SerializeField] private bool _ellipsisWhenClamped = true;

        [Header("Backgrounds")]
        [Tooltip("Boxes drawn behind this label that grow with it — a panel, a glow, a ribbon. Each lands "
                 + "on the label's size; its own artwork stretches out from there for padding. Put each on "
                 + "whichever object should end up the size of the label — the label's parent is fine as "
                 + "long as the label is not anchored to stretch to it.")]
        [SerializeField] private List<TextFitterBackground> _backgrounds = new();

        private TextMeshProUGUI _text;
        private RectTransform _rect;
        private bool _applying;

        // The modes the label was authored with, remembered before this component ever writes one. Both
        // are switched only while the string needs them switched, and these are what they go back to.
        private TextOverflowModes _authoredOverflow;
        private TextWrappingModes _authoredWrapping;
        private bool _authoredModesKnown;
        private bool _fitQueued;

        // The string the box was last sized around, so a mesh rebuild that did not change it is ignored.
        private string _lastFittedText;

        public float MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                Fit();
            }
        }

        private void OnEnable()
        {
            Cache();
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(HandleTextChanged);
            Fit();
        }

        private void OnDisable() => TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(HandleTextChanged);

#if UNITY_EDITOR
        private void OnValidate()
        {
            Cache();

            // Queued, not fitted here: writing the rect raises OnRectTransformDimensionsChange through
            // SendMessage, which Unity forbids from inside OnValidate and answers with a console warning
            // on every inspector edit. The deferral lands the same write just after OnValidate returns.
            QueueFit();
        }
#endif

        /// <summary>Sets the string and resizes in one go.</summary>
        public void SetText(string value)
        {
            Cache();
            _text.text = value;

            // Applied straight away rather than queued: a caller that sets text and then reads the size
            // in the same frame should see the new one.
            Fit();
        }

        /// <summary>
        /// Fits on the next frame. TMP raises its change event from inside its own mesh generation, and a
        /// size written during that pass is sometimes swallowed by it — which is why an edit occasionally
        /// needed Fit Now to take. Waiting a frame puts the write safely after TMP is done.
        /// </summary>
        public void QueueFit()
        {
            _fitQueued = true;

#if UNITY_EDITOR
            // Nothing ticks reliably outside play mode, so the editor gets its own deferred call.
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.delayCall += FitDeferred;
#endif
        }

        private void Update()
        {
            if (_fitQueued) Fit();
        }

#if UNITY_EDITOR
        private void FitDeferred()
        {
            // The object can be gone by the time the editor gets back to us.
            if (this == null || !_fitQueued) return;
            Fit();
        }
#endif

        [ContextMenu("Fit Now")]
        public void Fit()
        {
            _fitQueued = false;
            Cache();
            if (_text == null || _rect == null) return;

            _lastFittedText = _text.text;

            // Every write below dirties the text and raises TEXT_CHANGED_EVENT, which would come straight
            // back in here.
            _applying = true;

            // Measured at full size with auto sizing out of the way, so the reading describes the string
            // rather than the box it is currently squeezed into.
            _text.enableAutoSizing = false;
            _text.fontSizeMin = _fontSizeMin;
            _text.fontSizeMax = _fontSizeMax;
            _text.fontSize = _fontSizeMax;

            // No padding of our own: the box lands on the string, and room around it comes from the
            // label's own TMP margins or from whatever artwork stretches out behind it.
            Vector2 natural = _text.GetPreferredValues(_text.text);

            bool tooWide = _maxWidth > 0f && natural.x > _maxWidth;

            // Rounded up with a pixel to spare: TMP lays glyphs out in fractions, and a box cut to the
            // exact preferred width drops the last one over the edge.
            float width = tooWide ? _maxWidth : Mathf.Ceil(natural.x) + 1f;
            float height = _rect.rect.height;
            bool tooTall;

            if (_fitVertical)
            {
                // Width is spent, so the string is allowed to spend height instead. Wrapping has to be on
                // for that to mean anything, and the second reading is taken at the width just settled on
                // — the first one described a single line and says nothing about how deep it wraps.
                _text.textWrappingMode = TextWrappingModes.Normal;

                float wantedHeight = _text.GetPreferredValues(_text.text, width, 0f).y;

                tooTall = _maxHeight > 0f && wantedHeight > _maxHeight;
                height = tooTall ? _maxHeight : Mathf.Ceil(wantedHeight) + 1f;
            }
            else
            {
                // Wrapping handed back: this component only borrows it while Fit Vertical is on, and a
                // label switched back would otherwise be left wrapping for the rest of its life.
                _text.textWrappingMode = _authoredWrapping;

                // Height counts as being out of room too. The box keeps whatever height it was given, so a
                // line taller than that is cut by TMP exactly like an over-long one — which is how ellipsis
                // showed up well before Max Width was ever reached.
                tooTall = height > 0f && natural.y > height;
            }

            // Both axes have to be spent before the font gives way. With Fit Vertical that is the whole
            // ladder: widen, then wrap and deepen, and only with nowhere left to grow does auto sizing
            // shrink the font. Without it the height absorbs nothing, so either axis alone is the end
            // of the road.
            bool clamped = _fitVertical ? tooWide && tooTall : tooWide || tooTall;

            if (clamped)
            {
                // Out of room: hand the box's limit to TMP and let it shrink the font between the two
                // sizes, then ellipsize if even the minimum does not fit.
                _text.enableAutoSizing = true;
                if (_ellipsisWhenClamped) _text.overflowMode = TextOverflowModes.Ellipsis;
            }
            else
            {
                // Fits, so nothing should be cut — including by an ellipsis left over from a longer
                // string this label carried a moment ago.
                _text.overflowMode = _authoredOverflow;
            }

            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            if (_fitVertical) _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            _applying = false;

            // Handed the size the label ended up with rather than the string's natural one: a clamped
            // label is smaller than it asked for, and the box has to wrap what is actually drawn.
            FitBackgrounds(new Vector2(width, height));

            NotifyParentLayout();
        }

        // Pushed out, never read back — the label is the one in charge of the pair's size.
        private void FitBackgrounds(Vector2 labelSize)
        {
            for (int i = 0; i < _backgrounds.Count; i++)
                if (_backgrounds[i] != null)
                    _backgrounds[i].Fit(labelSize);
        }

        // A row that sizes itself around this label measured the old width; without this the parent only
        // catches up on the next thing that happens to dirty it.
        private void NotifyParentLayout()
        {
            if (transform.parent is not RectTransform parent) return;

            if (parent.TryGetComponent(out ContentSizeFitter _) || parent.TryGetComponent(out LayoutGroup _))
                LayoutRebuilder.MarkLayoutForRebuild(parent);
        }

        private void Cache()
        {
            if (_text == null) TryGetComponent(out _text);
            if (_rect == null) _rect = transform as RectTransform;

            // Read once, before the first fit can overwrite them.
            if (!_authoredModesKnown && _text != null)
            {
                _authoredOverflow = _text.overflowMode == TextOverflowModes.Ellipsis
                    ? TextOverflowModes.Overflow // an ellipsis already in place says nothing about intent
                    : _text.overflowMode;
                _authoredWrapping = _text.textWrappingMode;
                _authoredModesKnown = true;
            }
        }

        // TMP raises this for every text object; only ours matters, and not while we are the ones writing.
        private void HandleTextChanged(Object changed)
        {
            if (_applying || changed != _text) return;

            // It fires on every mesh rebuild, not only when the string changed. Something animating the
            // glyphs — WaveText, say — rebuilds the mesh every frame, and refitting a string that is still
            // the same string on every one of those frames buys nothing. Resizing the box rebuilds it too,
            // so without this the fit would also chase its own tail.
            if (_text.text == _lastFittedText) return;

            QueueFit();
        }
    }
}
