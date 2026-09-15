# Source and provenance

- Two sprite sheets were generated with the built-in OpenAI image_gen tool on 2026-09-15, using the user's supplied pose sheets and the existing CloudRobot appearance/base-color texture as references.
- Original generated PNGs and exact prompts are preserved under `ArtSource/Features/RoomOne/CloudRobotSprites/`.
- This change performs deterministic rectangular crops and transparent padding with Pillow. No character pixels were repainted, rescaled or regenerated for this split.
- Character design/reference attribution follows the existing Room One artwork and CloudRobot source records. No additional stock artwork, downloaded third-party images, FBX data or texture atlas is distributed in this asset folder. The user supplied no separate license grant for the character reference; this file does not assign a new license.
- Scene IDs and action order come from `RoomOneRules.Actions` and `RoomOneRules.IsBoxLift` in this repository. The second action row is mapped to `pull`, although its generated visual pose resembles pushing.

The original image generation previews showed RGB colors from fully transparent pixels. These are invisible under ordinary alpha compositing. Small edge artifacts in the source may remain visible; alpha has been preserved exactly.
