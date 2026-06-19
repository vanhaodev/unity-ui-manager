# CLAUDE.md — unity-ui-manager

Project-specific notes for AI agents. (Global rules still apply.)

## Orienting in the codebase

Before adding a feature or fixing a bug, **read [`Docs/codebase-summary.md`](Docs/codebase-summary.md)**
— a terse map of the source layout, how features are wired, and the project's conventions/gotchas
(cross-canvas camera rule, app-owns-numbers, pooling, the `Exported/` mirror). It's there to save you
re-scanning the tree. Keep it updated when structure or conventions change.

## Releasing / exporting a new version

Before bumping the package version or updating the distributed package, **read
[`Docs/release-export-guide.md`](Docs/release-export-guide.md)**. Key points:

- The version lives in **5 places**: `Assets/com.vanhaodev.uimanager/package.json`,
  `Exported/com.vanhaodev.uimanager/package.json` (each: `version` + `changelogUrl`), the
  **two** ui-manager install URLs in `Docs/README.md`, and the root `README.md` (version badge +
  install URL). Do not touch `objectpool` / `multiplayer.center` refs.
- Users install the **`Exported/`** folder (git URL `…?path=Exported/com.vanhaodev.uimanager#<ver>`).
  It is a verbatim mirror of `Assets/com.vanhaodev.uimanager` with **identical GUIDs** — sync by
  copying files + `.meta` directly; path map `Samples` → `Samples~`.
- Git tags are created manually by the maintainer.
