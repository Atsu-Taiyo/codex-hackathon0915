# Room 1 verification — 2026-09-15

Environment: Unity 6000.4.9f1, macOS, the repository's running Editor and a locally built standalone player.

## Passed

- 68 rule/resource checks: all 24 supported meanings, bad grammar and missing cards, exact semantic clear condition, reverse-lift routing, duplicate collection prevention, complete history, replay/preview exclusions, JSON round trips, five image resources and the transparent-sprite shader.
- 40 Play-mode checks: discovery and persistence, card exchanges, all six standard actions, dedicated box-lifts-robot execution and saving, concurrent-run prevention, final-frame completion, first-clear overlay, unusual subject/object order, replay and saved progress. Reverse lifting completes without clearing the forward goal. Verification restores the saved PlayerPrefs value captured at the start of that run.
- Art Play-mode test: idle room and all three dedicated reverse-lift frames captured and visually inspected. The box's orange hands support the robot and its feet remain visible. The character image's former cream rectangle and extraction haze are absent in rendered output. Replay leaves clear/history state unchanged. Screenshots: `docs/RoomOne/TransparentRoom.png` and `docs/RoomOne/BoxLiftsRobot.png`.
- macOS build: `Succeeded`, 0 errors. Deliverable: `Builds/RoomOne.app`.
- Standalone player launch and goal playback inspected. Default window size is 1280 × 720. Initial standalone progress had undiscovered nouns and no discovered scenes. Robot and box discovery were observed. The newly created standalone QA progress was backed up under `Temp/` and reset after verifying it contained only those two test discoveries and no experiments or clear state; the deliverable opens at the first-time experience. Editor progress was preserved.
- UI rendering inspected: three goal images at top, reference-derived robot and orange box, sentence cards and RUN inside the same white widget, vocabulary, map and collection links.
- Transparent robot cutout: RGBA PNG, alpha range 0–255, 1,143,736 fully transparent pixels.
- Standard animation atlas: RGBA, 961,659 fully transparent pixels. Box-lift atlas: RGBA, 1,325,347 fully transparent pixels. Both use native alpha with a 0.96 cutout threshold to suppress low-opacity extraction haze while preserving the opaque character silhouettes.
- `git diff --check` passed.

## Limit of manual input verification

The native automation tool visually moved its pointer, but Unity repeatedly received exactly the same mouse position for different requested clicks. This was observed in a temporary input trace, then that trace was removed. Keyboard execution responded correctly. Consequently, **end-to-end mouse dragging and every modal button have not been manually verified**. The shared card exchange logic and gameplay/persistence paths passed the Play-mode checks. No claim of a complete manual mouse walkthrough is made.

The runtime maps render-pixel pointer coordinates explicitly into the scaled design canvas, including letterbox offsets and history scroll coordinates. Full desktop mouse testing with normal physical input remains the next acceptance check.

No commits, PRs, remote publication, or Room 2 gameplay were created.
