---
name: unity-scene-safe-edit
description: Safely modify Unity scene files in this project with mandatory backup and Git change checks before any scene mutation.
---

# Unity Scene Safe Edit

Use this skill for any task that modifies the Unity scene, scene hierarchy, GameObjects, components, transforms, or serialized scene references.

## Required preflight

Before modifying a scene:

1. Identify the exact scene file being edited.
2. Create a timestamped copy outside the active scene path as a recovery backup.
3. Check `git status --short` and inspect Git changes for the target scene.
4. If the scene has uncommitted changes, report that to the user before proceeding. Treat those changes as user-owned and preserve them.
5. Do not use `git restore`, `git checkout`, `git reset`, discard, or equivalent operations on the scene unless the user explicitly requests that exact recovery operation and the recovery point is confirmed.

## Editing rules

- Prefer Unity Editor/MCP scene operations for hierarchy changes.
- Do not rewrite a Unity YAML scene wholesale when a targeted editor operation is available.
- Do not delete or recreate existing scene objects when moving or reconfiguring them; preserve their identity and references.
- Do not commit, stage, amend, or push Git changes unless the user explicitly asks.
- Never assume a Git `HEAD` scene is the user's latest scene state.

## Postflight validation

After editing:

- Save only the intended scene.
- Verify the expected objects, names, component references, parent relationships, and positions.
- Check that the target scene still contains the user's pre-existing objects and changes.
- Check Unity console errors when the task involves Unity scene or script references.
- Report the backup location and validation result.

## Recovery behavior

If a scene is accidentally overwritten or objects appear missing:

- Stop making further scene changes.
- Preserve the current file as an additional backup.
- Check Unity Undo, editor autosave/backup files, Git status, reflog, unreachable objects, and OS file-history/previous-version sources.
- Do not claim recovery is complete without verifying the scene contents.
