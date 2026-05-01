---
description: Follow these steps to install UI Manager correctly.
---

# Installation

{% stepper %}
{% step %}
### Check requirements

UI Manager requires **Unity 6** or later.
{% endstep %}

{% step %}
### Install DOTween

DOTween handles animations.

1. Open **Window → Asset Store**.
2. Search for **DOTween**.
3. Download and import it.
4. Use version **1.2.825** or later.

DOTween link:

```
https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676
```
{% endstep %}

{% step %}
### Install Object Pool

Object Pool improves resource management.

#### Option 1 — Git URL

1. Open **Window → Package Manager**.
2. Click the **+** icon.
3. Select **Add package from git URL...**.
4. Paste this URL:

```
https://github.com/vanhaodev/unity-object-pool.git?path=Exported/com.vanhaodev.objectpool#1.0.1
```

#### Option 2 — `manifest.json`

1. Open `Packages/manifest.json`.
2. Add this entry:

```json
"com.vanhaodev.objectpool": "https://github.com/vanhaodev/unity-object-pool.git?path=Exported/com.vanhaodev.objectpool#1.0.1"
```
{% endstep %}

{% step %}
### Install UI Manager

#### Option 1 — Git URL

Paste this URL into Package Manager:

```
https://github.com/vanhaodev/unity-ui-manager.git?path=Exported/com.vanhaodev.uimanager#1.0.0
```

#### Option 2 — `manifest.json`

1. Open `Packages/manifest.json`.
2. Add this entry:

```json
"com.vanhaodev.uimanager": "https://github.com/vanhaodev/unity-ui-manager.git?path=Exported/com.vanhaodev.uimanager#1.0.0"
```
{% endstep %}

{% step %}
### Import sample

1. Open **Window → Package Manager**.
2. Select **UI Manager** from the package list.
3. Open the **Samples** tab.
4. Click **Import** on **K-pop Shop**.

{% hint style="info" %}
This sample shows the main UI Manager features and helps you get started faster.
{% endhint %}
{% endstep %}
{% endstepper %}
