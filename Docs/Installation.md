# UI Manager Installation Guide

Follow these steps to install **UI Manager** correctly.

---

<!-- ---- STEP 01 ---- -->

<div style="display:flex; gap:12px; margin-bottom:20px;">
  <div style="text-align:center; min-width:30px;">
    <div style="font-weight:bold;">01</div>
    <div style="width:2px; height:100%; background:#2a2f3a; margin:4px auto;"></div>
  </div>

  <div>
    <b>Check Requirements</b> <span style="color:orange;">(Required)</span><br/><br/>
    UI Manager requires <b>Unity 6 or higher</b>.<br/>
  </div>
</div>

---

<!-- ---- STEP 02 ---- -->

<div style="display:flex; gap:12px; margin-bottom:20px;">
  <div style="text-align:center; min-width:30px;">
    <div style="font-weight:bold;">02</div>
    <div style="width:2px; height:100%; background:#2a2f3a; margin:4px auto;"></div>
  </div>

  <div>
    <b>Install DOTween</b> <span style="color:red;">(Required · Download Unity Package)</span><br/><br/>
    DOTween is required for animations.<br/><br/>

<code>Unity → Window → Asset Store</code><br/><br/>

• Search <b>DOTween</b><br/>
• Download & Import into project<br/>
• Ensure version <b>1.2.825 or higher</b><br/><br/>

Link:<br/> <code>https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676</code>

  </div>
</div>

---

<!-- ---- STEP 03 ---- -->

<div style="display:flex; gap:12px; margin-bottom:20px;">
  <div style="text-align:center; min-width:30px;">
    <div style="font-weight:bold;">03</div>
    <div style="width:2px; height:100%; background:#2a2f3a; margin:4px auto;"></div>
  </div>

  <div>
    <b>Install Object Pool</b> <span style="color:limegreen;">(Required · UPM)</span><br/><br/>
    Object Pool is used for efficient resource management.<br/><br/>

<b>Option 1 — Git URL</b><br/> 
- Open Window → Package Manager
- Click + (plus) icon (top left)
- Select "Add package from git URL..."
- Paste the link:

    <json>https://github.com/vanhaodev/unity-object-pool.git?path=Exported/com.vanhaodev.objectpool#1.0.1</json><br/><br/>

<b>Option 2 — manifest.json</b><br/> 
- Open file in Packages/manifest.json
- Paste the link:

    <code>
    "com.vanhaodev.objectpool": "https://github.com/vanhaodev/unity-object-pool.git?path=Exported/com.vanhaodev.objectpool#1.0.1"
    </code>

  </div>
</div>

---

<!-- ---- STEP 04 ---- -->

<div style="display:flex; gap:12px; margin-bottom:20px;">
  <div style="text-align:center; min-width:30px;">
    <div style="font-weight:bold;">04</div>
    <div style="width:2px; height:100%; background:#2a2f3a; margin:4px auto;"></div>
  </div>

  <div>
    <b>Install UI Manager</b> <span style="color:#4ea1ff;">(Final Step)</span><br/><br/>

<b>Option 1 — Git URL</b><br/> 
- Paste the link:

    <json>https://github.com/vanhaodev/unity-ui-manager.git?path=Exported/com.vanhaodev.uimanager#1.0.0</json><br/>

<b>Option 2 — manifest.json</b><br/> 
- Open file in Packages/manifest.json
- Paste the link:

    <code>
    "com.vanhaodev.uimanager": "https://github.com/vanhaodev/unity-ui-manager.git?path=Exported/com.vanhaodev.uimanager#1.0.0"
    </code>

  </div>
</div>

---

<!-- ---- STEP 05 ---- -->

<div style="display:flex; gap:12px; margin-bottom:20px;">
  <div style="text-align:center; min-width:30px;">
    <div style="font-weight:bold;">05</div>
  </div>

  <div>
    <b>Import Sample</b><br/><br/>

• Open <b>Window → Package Manager</b><br/>
• Select <b>UI Manager</b> from the package list<br/>
• Go to the <b>Samples</b> tab<br/>
• Click <b>Import</b> on <b>K-pop Shop</b><br/>

<i>Note: This sample demonstrates the main features provided by UI Manager and helps you understand how to use it.</i>

  </div>
</div>

---
