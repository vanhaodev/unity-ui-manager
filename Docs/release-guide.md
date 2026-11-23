# Release Guide — UI Manager

Quick runbook to ship a new version. The package lives **once**, as an embedded package at
`Packages/com.vanhaodev.uimanager/`, and users install that folder directly:
`…/unity-ui-manager.git?path=Packages/com.vanhaodev.uimanager#<version>`. There is no export or mirror
step (the old `Exported/` folder is gone; tags up to `1.0.8` still install from it).

## 1. Bump version (X.Y.Z)

| File | What to change |
|------|----------------|
| `Packages/com.vanhaodev.uimanager/package.json` | `version` + `changelogUrl` (`…/commits/X.Y.Z`) |
| `Docs/README.md` | the **two** install URLs ending `#X.Y.Z` (Git URL tab + manifest.json tab) |
| `README.md` (repo root) | the version **badge** and the ui-manager install URL ending `#X.Y.Z` |

**Do NOT touch** `com.vanhaodev.objectpool#…` refs or `com.unity.multiplayer.center` —
those are unrelated dependencies with their own versions.

## 2. Samples

- Source: `Packages/com.vanhaodev.uimanager/Samples~/<Sample>/` — hidden from Unity's importer, so Unity
  never generates `.meta` files there. Every file in it must already carry its `.meta`.
- To work on a sample, import it (Package Manager → UI Manager → Samples → Import). It lands in
  `Assets/Samples/UI Manager/<version>/<Sample>/`, which is **git-ignored**, with the same GUIDs.
- Edit and test there, then copy the changed files **and their `.meta`** (Unity generates those for new
  files) back into `Samples~`. Only `Samples~` is committed.
- `ProjectSettings/EditorBuildSettings.asset` points at the imported K-pop scene; update the version in
  that path when you re-import under a new version.

## 3. Commit + tag

- Commit message: `chore(release): bump version to X.Y.Z`
- **Git tag is created manually by the maintainer.** Tag name = `X.Y.Z`, must match the
  `#X.Y.Z` install URLs and the `changelogUrl`.

## Notes

- Unrelated churn (`ProjectSettings/`, `DOTweenSettings.asset`, `.vscode/`) is not part of a release.
