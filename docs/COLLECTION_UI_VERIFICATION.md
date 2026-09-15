# Collection UI verification — 2026-09-15

The Room 01 collection uses one shared header, understated scene/experiment tabs, a three-by-two image grid, and a flat chronological experiment list. Discovered tiles are single native IMGUI replay buttons. Undiscovered tiles expose neither action names nor replay targets. History uses native clipped scrolling, renders only visible rows, and keeps newest experiments first. Collection-specific scrollbar resources are released when the room is destroyed.

## Verified

- Unity 6000.4.9f1 compiled the updated runtime code.
- Actual Game-view captures at 2560 × 1440 inspected: all six discoveries, three discoveries with three locked tiles, empty experiment history, and a 24-entry history with the slim scrollbar.
- 56 existing rule/resource checks passed.
- 49 existing Play-mode checks passed at 14:14:32 JST, covering all six actions, reverse lifting, replay without duplicate history, and progress persistence.
- The saved Editor progress was compared byte-for-byte with the pre-test save and matched after verification (10 original experiments).
- Collection changes passed whitespace checks. Other pre-existing work was preserved.
- Temporary Editor probe code was removed after verification.

Screenshots: [Scenes](RoomOne/Collection.png), [Experiments](RoomOne/CollectionExperiments.png). The screenshots use temporary in-memory fixtures; the saved collection was restored.

## Limits

The native automation tool returned `noWindowsAvailable` for coordinate clicks in Unity. New tile/tab/close clicks and scrollbar dragging have not been verified with physical mouse input. The visual states were opened through a temporary Editor probe; the existing Play-mode checks verify the underlying replay and persistence behavior, not button hit testing.

Changes are in the Unity project. Existing standalone player bundles were not rebuilt.
