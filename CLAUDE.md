# CLAUDE.md — unity-ui-manager

Project-specific notes for AI agents. (Global rules still apply.)

## Orienting in the codebase

Before adding a feature or fixing a bug, **read [`Docs/codebase-summary.md`](Docs/codebase-summary.md)**
— a terse map of the source layout, how features are wired, and the project's conventions/gotchas
(cross-canvas camera rule, app-owns-numbers, pooling, no new third-party libs). It's there to save you
re-scanning the tree. Keep it updated when structure or conventions change.

## Releasing a new version

Before bumping the package version, **read [`Docs/release-guide.md`](Docs/release-guide.md)**. Key points:

- The package exists **once**: `Packages/com.vanhaodev.uimanager/` (embedded package). Users install it
  straight from git (`…?path=Packages/com.vanhaodev.uimanager#<ver>`). No `Exported/` mirror any more.
- The version lives in **3 files**: `Packages/com.vanhaodev.uimanager/package.json` (`version` +
  `changelogUrl`), the **two** ui-manager install URLs in `Docs/README.md`, and the root `README.md`
  (version badge + install URL). Do not touch `objectpool` / `multiplayer.center` refs.
- Samples: source in `Samples~/` (commit every file with its `.meta`); edit them as the imported,
  git-ignored copy in `Assets/Samples/UI Manager/<ver>/`, then copy changes back.
- Git tags are created manually by the maintainer.
