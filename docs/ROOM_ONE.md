# Room 1 MVP

## Run

Open `Assets/_Project/Scenes/Bootstrap.unity` in Unity 6000.4.9f1 and press Play. The existing application entry point creates Room One. No API key or online service is needed. The layout scales to fit a 16:9 canvas and letterboxes other aspect ratios.

1. Watch the three-frame goal. Use the top **▶** to replay it without revealing a sentence or granting progress.
2. Click the robot and box to discover their nouns.
3. Click word cards to place them in the sentence. Click a sentence slot first to choose the replacement position.
4. Drag any available noun or verb card into a sentence slot; drag placed cards onto one another to exchange positions. Drag back to the vocabulary row to remove a card. Each word has one card, and used cards leave an empty tray position. A selected slot also supports left/right arrow keys; Delete or double-click removes it.
5. Hover over a verb for its three-frame hint. Click **▶** in the separate bottom control row inside the builder, or press Enter, to run the sentence.
6. Open **Collection** for discovered scenes and **My experiments** for replayable history. **Map** returns to the playable 3D world map and shows saved Room 1 progress. Select **Enter Room 1** to return. Room 2 is an inactive future placeholder. See [World Map](WORLD_MAP.md).

## Rules and scope

- English, one room, two discoverable nouns, six initially available verbs.
- Noun cards include the article automatically; no Japanese translation is shown.
- Grammar is noun + verb + noun. The interpreter produces a `SentenceMeaning`, separately from rendering.
- The 12 combinations with distinct subject/object cards are accepted; repeated use of the same card is rejected. The six robot-to-box actions and **The box lifts the robot.** use dedicated generated three-frame artwork. The reverse lift gives the box orange arms and legs; its hands raise and support the robot while its feet stay on the ground. Other reverse combinations use a generic three-step actor/action/target illustration.
- All 21 poses use the CloudRobot PNGs from PR #3 with native alpha and preserved canvas proportions. The same artwork appears in the goal, verb hints, collection, clear screen and replays. Unusual combinations use the new standalone robot and box. The reverse lift is saved in experiment history, and does not clear the forward-lift goal or increase the six standard collections.
- Clear requires `robot:lift:box`. Other experiments never clear the room merely because the picture changes.
- Every execution resets the world first. A failed grammar check shows the offending slot and does not run or create history.
- Six standard action scenes form the collection; repeat experiments do not increase its total. All valid player executions remain in history. Replays and goal previews do not change progress.
- Three badges indicate goal completion, both nouns discovered, and all six scenes collected.
- Clear offers Keep exploring and Back to map. No Room 2 gameplay is included.

## Persistence

`RoomOneSave` stores versioned JSON in Unity PlayerPrefs under `Hackathon.RoomOne.Progress.v1`: unlocked/clear status, discovered nouns, global vocabulary, collections, and complete execution history. Saving occurs after discoveries and finished player experiments. Editor and standalone application can have separate platform preference domains. There is no automatic progress deletion on entering a room.

## Code / assets

- `Scripts/Features/RoomOne/Model`: serializable meaning and progress.
- `Service`: grammar and semantic goal/collection rules.
- `Repo`: local save/load.
- `Controller`: discovery, state transitions and three-frame playback.
- `View`: scaled interactive UI, card drag handling, map and collection.
- `Content/Features/RoomOne/CloudRobotSprites`: PR #3's 21 poses, two characters and animation manifest, retaining original PNG bytes and GUIDs.
- `Content/Features/RoomOne/Art/Resources/RoomOne/CloudRobotArtwork.asset`: build-safe direct references to the PR sprites and new background. Earlier artwork remains archived but is no longer used by the room.
- `RoomOne_CloudWorkshop.png`: Unity render of PR #2's workshop floors/walls and shared island furnishings. **Tools > Room One > Connect CloudRobot Artwork** regenerates this background and reconnects the manifest.
- `ArtSource/RoomOne/PROMPTS.md`: exact generation prompts; built-in image_gen used.

## Verification and build

**Tools > Room One > Validate Rules** checks 12 legal meanings and 12 duplicate-card rejections, malformed sentences, exact goal matching, collection/history behavior, JSON round trips and all art resources. **Tools > Room One > Build macOS MVP** builds `Builds/RoomOne.app`.

For local Editor verification, `RoomOneTools` accepts the fixed commands `validate`, `play`, `stop`, `playtest`, `arttest`, `capture`, `build` in `Temp/RoomOne.command`. No arbitrary code or network endpoint is exposed. `playtest` requires Play mode, exercises every animation and save behavior, and restores the previous PlayerPrefs value on completion or destruction. `arttest` captures the idle room and all 21 poses using replay without changing saved progress. Test outputs remain in ignored `Temp/`.

## Room 1 language selection

Entering Room 1 opens an English / Kiswahili selector before the goal preview.
English keeps the existing free three-card builder. Kiswahili fixes the sentence
frame to `Roboti + [verb] + sanduku.` and offers `inasukuma`, `inavuta`, `inainua`,
`inafungua`, `inatikisa`, and `inavunja`. These map to the existing push, pull,
lift, open, shake, and break animations, including hover previews and collection
replays. Reset clears only the verb in Kiswahili. The local English voice bridge
is available in English mode only.

English retains `Hackathon.RoomOne.Progress.v1`; Kiswahili uses the `.sw` suffix
for separate discoveries, history, and completion. Return to the map and enter
again to choose another language. `RoomOnePlaytest.Run` covers both modes and
restores both original saves after testing.
