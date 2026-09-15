# Room One artwork

- `RoomOne_Robot_Reference.png`: original robot reference supplied by the user, retained unchanged.
- `RoomOne_Robot_Cutout.png`: image_gen background extraction from the original reference, used in unusual-sentence illustrations. RGBA PNG, 1266 × 1242, alpha range 0–255 and 1,143,736 fully transparent pixels verified. App controls are excluded.
- `RoomOne_Actions_Atlas.png`: built-in image_gen output based on that reference, updated to RGBA background transparency on 2026-09-15. 1254 × 1254 PNG; 3 columns × 6 rows, read top to bottom: push, pull, lift, open, shake, break. 961,659 fully transparent pixels. All standard actions and hover hints use exactly 3 discrete frames. The earlier opaque version is preserved in ArtSource/RoomOne.
- `RoomOne_BoxLiftsRobot_Atlas.png`: built-in image_gen output, 1024 × 1536 RGBA PNG with 1,325,347 fully transparent pixels. Three vertical poses show the user-approved orange box with arms and legs lifting the blue robot. Measured row boundaries are 0, 500, 1000, 1536 pixels to retain the complete final pose.
- `RoomOne_PixelCutout.shader`: renders the generated sprites using their native alpha, rejecting extraction haze below 0.96 opacity. No opaque fill is drawn behind the sprites; PNG artwork is not modified by this shader. Dark outlines inside the characters remain visible.
- `RoomOne_Workshop_Background.png`: built-in image_gen output, 1672 × 941 PNG.
- Prompts and creation notes: `ArtSource/RoomOne/PROMPTS.md`.
- The user supplied no separate license for the robot reference. No stock artwork or downloaded third-party art was introduced.

PNG files are imported without mipmaps or compression. The pixel-art atlas and reference use point filtering. The background uses bilinear filtering. Images are loaded from the feature-local Resources folder and require no runtime generation service.
