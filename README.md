<div align="center">

# 🎛️ UI Manager for Unity

**One manager for every piece of your game's UI — screens, popups, toasts, effects — with smooth animations and zero boilerplate.**

[![Unity](https://img.shields.io/badge/Unity-6000.0%2B-black?logo=unity)](https://unity.com/)
[![Version](https://img.shields.io/badge/version-1.0.5-blue)](https://github.com/vanhaodev/unity-ui-manager/releases)
[![Docs](https://img.shields.io/badge/docs-gitbook-brightgreen)](https://vanhaodev.gitbook.io/ui-manager)

</div>

---

Ask the manager for UI **by type** and it handles the rest — instantiating, caching, animating in and out, pooling, and cleanup:

```csharp
ui.ShowScreen<HomeScreen>();        // switch page
ui.ShowPopup<ConfirmPopup>();       // open a dialog
ui.ShowToast<ToastDefault>(ToastPositionType.Top, t => t.SetMessage("Saved!"));
```

No prefab wiring in code, no manual show/hide plumbing. Just call it.

## ✨ Features

| | Feature | What you get |
|---|---|---|
| 🖥️ | **Screens** | Full-screen pages, one at a time, with auto transition in/out. |
| 🪟 | **Popups** | Stacking dialogs, tap-outside-to-close, newest-on-top (or keep one pinned). |
| 🔔 | **Toasts** | Auto-dismissing messages at any edge/corner, swipe to dismiss, queued & stacked. |
| ⏳ | **Loading Block** | "Please wait" overlay that wraps your async work and nests safely. |
| 💬 | **Floating Text** | Pop-and-float labels ("+100", "Miss!") that space themselves and stay on screen. |
| 🪙 | **Flyout Effect** | Coins/gems that burst and fly into a counter — app-driven count-up, any number format. |
| 👆 | **Click Effect** | A ripple at every tap, anywhere on screen — snappy, responsive feel on mobile & PC. |

## 🚀 Why you'll like it

- **Type-driven API** — `ShowScreen<T>()`, `ShowPopup<T>()`… no prefab references in code.
- **Pooled & cached** — toasts, flyout icons and click effects reuse instances; nothing churns the GC.
- **Animations built in** — every element animates in/out; drop in your own with one interface.
- **Cross-platform input** — click effect auto-detects the New Input System *or* legacy Input.
- **Lazy & lightweight** — features you don't use cost nothing.
- **Editor niceties** — tabbed UILibrary inspector + a one-click **Reload** to restore prefab references Unity drops.
- **Extensible** — subclass any base (`BasePopup`, `BaseToast`, `BaseClickEffect`…) for custom looks, particles, or sounds.

## 📦 Install

Requires **Unity 6+**. Add both packages via **Package Manager → Add package from git URL**:

```
https://github.com/vanhaodev/unity-object-pool.git?path=Exported/com.vanhaodev.objectpool#1.0.1
https://github.com/vanhaodev/unity-ui-manager.git?path=Exported/com.vanhaodev.uimanager#1.0.5
```

Then import the **K-pop Shop** sample (Package Manager → UI Manager → Samples) to see every feature in action.

## 📚 Documentation

👉 **[Read the full documentation](https://vanhaodev.gitbook.io/ui-manager)** — setup, usage guides and API for every feature.
