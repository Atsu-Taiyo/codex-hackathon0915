# Room 1 and Map art refresh — 2026-09-15

## Sources and saved result

- PR #3 at `49c69a5c95e5098810fe6e93012943f81b12aedc`: all 21 CloudRobot poses and both standalone characters retain their original PNG bytes and asset GUIDs. `CloudRobotArtwork.asset` references them directly for player inclusion.
- PR #2 at `8fa588c79f6ecd0121bc3d65acdbb388a099e209`: existing terrain used for the furnished workshop, terrace, stairs, ramp and shore. Map decorations and materials are saved in `WorldMap.unity` and `Map/IslandArt`.
- Room 1 uses a new Unity-rendered workshop background. The stage, goal thumbnails, hints, collection, clear screen and reverse actions all use the new artwork.
- Map HUD now contains a small Map title, room labels, one Enter button and a short movement hint. Clear status is a checkmark; counters and descriptive panels are removed. Pointer blocking matches the smaller visible controls.
- The robot can step over the kit's 0.25 m stairs; the controller step offset is 0.3 m.

## Verified in Unity 6000.4.9f1

- 23 PNG SHA-256 hashes and canvas dimensions match the PR manifest.
- 72 rule/resource checks passed.
- All 21 action frames and the idle room were captured in Play mode. Forward lift and reverse lift were visually inspected; complete silhouettes fit and transparent areas show the workshop. Replay preserves history and clear state.
- 549 navigation/material assertions passed: movement, workshop arrival, coast collision, climbing the stairs to the terrace, locked Room 2 selection, Map → Room 1 → Map → Room 1, progress/history persistence and exact restoration of the original save.
- No C# compile errors; asset GUID uniqueness and whitespace checked.

This validation covers the local Editor. Existing standalone/WebGL build outputs have not been refreshed by this change.
