# Release & Export Guide — UI Manager

Quick runbook to ship a new version. The package users install is the **`Exported/`**
folder (via git URL `…?path=Exported/com.vanhaodev.uimanager#<version>`), so it must be
kept in sync with the source under `Assets/com.vanhaodev.uimanager`.

## 1. Bump version (X.Y.Z)

Update the ui-manager version in these places (replace `1.0.1`-style with the new one):

| File | What to change |
|------|----------------|
| `Assets/com.vanhaodev.uimanager/package.json` | `version` + `changelogUrl` (`…/commits/X.Y.Z`) |
| `Exported/com.vanhaodev.uimanager/package.json` | same two fields |
| `Docs/README.md` | the **two** install URLs ending `#X.Y.Z` (Git URL tab + manifest.json tab) |

**Do NOT touch** `com.vanhaodev.objectpool#…` refs or `com.unity.multiplayer.center` —
those are unrelated dependencies with their own versions.

## 2. Sync `Exported/` (mirror of the Assets package)

- `Exported/com.vanhaodev.uimanager` is a **verbatim copy** of `Assets/com.vanhaodev.uimanager`.
- **GUIDs are identical** between the two, so copying files + their `.meta` directly keeps all
  prefab/asset references intact — no re-wiring needed.
- **Path map:** `Assets/.../Samples/K-pop Shop` → `Exported/.../Samples~/K-pop Shop` (note the `~`).
- For every **new** file, copy the file **and** its `.meta`. For **new folders**, also copy the
  folder `.meta` (e.g. `FloatingText.meta` sitting next to the `FloatingText/` folder).
- For **changed** files, copying the `.cs`/asset is enough (`.meta` is unchanged).

Example (run from repo root — edit the file list to match what actually changed):

```bash
SRC="Assets/com.vanhaodev.uimanager"
EXP="Exported/com.vanhaodev.uimanager"

# new runtime folder (+ its folder meta)
cp "$SRC/Runtime/Scripts/<NewFolder>.meta" "$EXP/Runtime/Scripts/"
cp -r "$SRC/Runtime/Scripts/<NewFolder>" "$EXP/Runtime/Scripts/"

# changed runtime files
cp "$SRC/Runtime/Scripts/<dir>/<changed>.cs" "$EXP/Runtime/Scripts/<dir>/"

# samples: Samples -> Samples~  (copy .meta too for new files)
cp "$SRC/Samples/K-pop Shop/<dir>/<file>" "$EXP/Samples~/K-pop Shop/<dir>/"
```

Verify with: `git status --short Exported/` — it should list exactly the synced files.

## 3. Commit + tag

- Commit message: `chore(release): bump version to X.Y.Z and sync exported package`
- **Git tag is created manually by the maintainer.** Tag name = `X.Y.Z`, must match the
  `#X.Y.Z` install URLs and the `changelogUrl`.

## Notes

- `Samples~` (with `~`) is hidden from Unity's importer but its `.meta` files are kept — they're
  imported when a user adds the sample via Package Manager.
- After syncing, the only intended diff in `Exported/` should be the feature files + version bump.
- Unrelated churn (`ProjectSettings/`, `DOTweenSettings.asset`, `.vscode/`) is not part of a release.
