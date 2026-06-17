# CLAUDE.md — unity-ui-manager

Project-specific notes for AI agents. (Global rules still apply.)

## Releasing / exporting a new version

Before bumping the package version or updating the distributed package, **read
[`Docs/release-export-guide.md`](Docs/release-export-guide.md)**. Key points:

- The version lives in **4 places**: `Assets/com.vanhaodev.uimanager/package.json`,
  `Exported/com.vanhaodev.uimanager/package.json` (each: `version` + `changelogUrl`), and the
  **two** ui-manager install URLs in `Docs/README.md`. Do not touch `objectpool` /
  `multiplayer.center` refs.
- Users install the **`Exported/`** folder (git URL `…?path=Exported/com.vanhaodev.uimanager#<ver>`).
  It is a verbatim mirror of `Assets/com.vanhaodev.uimanager` with **identical GUIDs** — sync by
  copying files + `.meta` directly; path map `Samples` → `Samples~`.
- Git tags are created manually by the maintainer.
