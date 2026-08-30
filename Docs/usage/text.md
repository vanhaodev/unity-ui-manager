---
description: Labels that size themselves to their string, and text that moves.
---

# Text

Four small components you drop straight onto a TextMeshPro label. No manager, no UILibrary entry,
no code required to wire them up — they work on any label, inside UI Manager or not. Each is
independent; use one or all four.

## Text Fitter — a box the size of its string

`TextFitter` goes on the label and resizes **that label's box** to whatever string it is holding.
A short word and the box hugs it; a paragraph and the box grows until it runs out of room, and only
then starts giving ground.

It spends width first, then height, and the font last of all:

| The string is… | What happens |
| --- | --- |
| short | The box hugs the text at **Font Size Max**. |
| wider than **Max Width** | The box stops widening — the text wraps and the box **deepens** instead. |
| taller than **Max Height** as well | Out of room: the font shrinks toward **Font Size Min**. |
| still too big | TMP ellipsizes at the minimum size. |

The middle rung is opt-in — tick **Fit Vertical**. With it off the height stays exactly as you
authored it, so there is nowhere for a long string to go and Max Width leads straight to shrinking
the font.

```csharp
_fitter.SetText("Not enough coins");
```

Assigning `label.text` directly works too — the fitter notices and resizes on the next frame.
`SetText` exists for when you need the new size in the **same** frame, which matters if something
reads the size right after (positioning it, clamping it to the screen).

| Setting | What it does |
| --- | --- |
| `Max Width` | Widest the box may get. |
| `Font Size Max` / `Font Size Min` | The range the font is allowed to shrink through. |
| `Fit Vertical` | Let the box spend height before the font gives way. |
| `Max Height` | Tallest the box may get. Only applies with Fit Vertical on. |
| `Ellipsis When Clamped` | Cut the tail with "…" once the font has shrunk as far as it may. |
| `Backgrounds` | Boxes that should grow along with this label — see below. |

{% hint style="info" %}
**Leave auto sizing to the fitter.** It sets the TMP font size settings rather than reading them: a
font resizing itself to fit the box while the box resizes itself to fit the font never settles, so
one of the two has to be in charge. Your **overflow mode** and **word wrapping** are only borrowed —
the fitter snapshots them before its first fit and hands each back as soon as it no longer needs it.
{% endhint %}

## Text Fitter Background — a panel that follows the label

A label over a busy scene needs something solid behind it. Put `TextFitterBackground` on that
something and drag it into the fitter's **Backgrounds** list. Every time the label resizes, the
fitter pushes the new size across. Add as many as you like — a panel, a glow, a ribbon.

**There is no padding setting, on purpose.** The component lands on exactly the label's size; the
artwork goes on a **child** of it, anchored to stretch and pulled out a few pixels on each side.
Those offsets *are* the padding — you drag them in the Scene view instead of typing numbers, and
each background gets its own.

```
FloatingTextMessage   ← TextFitterBackground: tracks the label
├── Panel             ← the sliced sprite, stretched past its parent — the overhang is the padding
└── TxMgs             ← the TMP label + TextFitter
```

| Setting | What it does |
| --- | --- |
| `Axis` | Which sides follow the label: `Horizontal`, `Vertical` or `Both`. |

{% hint style="warning" %}
Put the background on **whichever object should end up the size of the label**. The label's parent
is fine — that is the sketch above, and what the shipped `FloatingTextDefault.prefab` does. The one
layout to avoid is a label *anchored to stretch* to the box: it would resize the box, which resizes
the label, and round they go.
{% endhint %}

## Wave Text — glyphs riding a sine wave

`WaveText` rolls a sine wave through a label's characters, each one a little later than the one
before, so the string ripples. It moves the glyphs in the mesh rather than the transform, which is
what lets the letters travel independently — and it never touches the layout box, so a label that
is centred or fitted stays exactly where you put it.

```csharp
_wave.Play();              // start rippling
_wave.Play("Level up!");   // swap the string and start
_wave.Stop();              // settle the letters back onto the baseline
```

| Setting | Default | What it does |
| --- | --- | --- |
| `Speed` | 1.5 | How fast the wave travels. |
| `Amplitude` | 3 | How far a glyph rides above and below the baseline. |
| `Frequency` | 0.6 | Phase pushed onto each glyph — higher packs more letters per crest. |

{% hint style="info" %}
Always `Stop()` rather than just disabling it: the offsets are baked into the vertices, so halting
mid-wave would freeze the last frame's ripple into the label. `Stop()` rebuilds the mesh once on the
way out.
{% endhint %}

## Marquee Text — a line too long to show at once

`MarqueeText` scrolls a string that does not fit its container, the way a ticker does.

```csharp
_marquee.SetText("A very long track title that will not fit");
_marquee.Play();                       // scroll until stopped
_marquee.Play(3, () => Debug.Log("done"));  // scroll three times, then call back
_marquee.Stop();
```

| Setting | Default | What it does |
| --- | --- | --- |
| `Mode` | `Loop` | `Loop` restarts from the far side; `PingPong` slides back and forth. |
| `Speed` | 150 | Scroll speed in pixels per second. |
| `Delay Ms` | 1000 | Pause at each end before moving off again. |

| Member | Description |
| --- | --- |
| `LoopCount` | How many times it has scrolled so far. |
| `OnLoopComplete` | Fires after each pass, with the running count. |

## Using them together

`FloatingTextDefault` — the built-in floating label — is built out of the first two, so
`ShowFloatingText("…")` already sizes itself to the message. See [Floating Text](floating-text.md).

Wave and fit can share a label: the fitter refits only when the **string** actually changes, so
`WaveText` rebuilding the mesh every frame does not drag a refit along with it.
