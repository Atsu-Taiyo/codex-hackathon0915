# World Map verification — 2026-09-15

Environment: Unity 6000.4.9f1, macOS, a disposable copy of this project and the built standalone player.

## Passed

- Imported 55 additive terrain files from PR #2 at `8fa588c79f6ecd0121bc3d65acdbb388a099e209`. All 18 FBX files contain binary model data, with 18 generated Unity Prefabs.
- 258 Play-mode assertions covering supported materials, movement on the imported mesh colliders, workshop arrival, coast collision, locked Room 2 selection, and a single active audio listener.
- Map → Room 1 → Map → Room 1 navigation; playback blocks early exit; the real goal saves completion, words, collections, and history; re-entry preserves them and does not count the goal preview as a new experiment.
- The final batch verification uses an isolated PlayerPrefs company domain and restores its previous save exactly. Runtime C# files in the tested copy matched the workspace at final validation.
- macOS player build succeeded with both WorldMap and Bootstrap included, in that order. Output: `Builds/EnglishWorld.app`.
- Standalone Map was visually inspected after correcting clipped labels/counters and lighting. Room 1 entry was also observed in the running player.
- No duplicate asset GUIDs. The tracked files changed for Map pass the scoped whitespace check; pre-existing Unity importer metadata was left intact.

The copied Editor logged an internal Unity SearchDatabase initialization exception. Navigation assertions and the subsequent player build still completed successfully; no game-code exception occurred during the final navigation test.

## Remaining scope

Room 2 remains a future placeholder. Click-to-walk moves directly toward the destination and does not route around obstacles. No WebGL build or browser-input verification was performed for this change.
