# Room 1 image generation

Tool: built-in image_gen (development-time asset creation only).
Reference: robot image supplied by the user; no external license was supplied. Confirm redistribution rights before public release.
Atlas: 3 columns x 6 rows. Rows: push, pull, lift, open, shake, break. Each action has exactly three frames. Generated at 1254 x 1254, no runtime API dependency.
Background: generated workshop environment.
Original robot reference is retained unchanged; in-game fallback uses the generated transparent cutout below.

## Atlas prompt

Use case: identity-preserve. Asset type: Unity 2D animation atlas. Create ONE exact uniform 3-column by 6-row sprite sheet, 18 panels, no gutters, no borders, no text or UI. Each panel is wide 2:1, total canvas square, ideally 3072x3072. Use the EXACT blue pixel-art robot design in attached reference: scalloped round blue head, navy rectangular screen, cyan >_ face, tiny blue body and limbs, >_ chest. Ignore black backdrop and app buttons in reference. Keep this identity in every panel, pixel-art style. Every panel shows same pale warm ivory background and thin floor shadow, robot left and orange cubic cardboard box right, both entire objects visible. Identical camera and scale every frame. Each ROW is a THREE-FRAME animation: column 1 anticipation / column 2 action / column 3 result. Row1 PUSH: robot next to box; robot pushes right; box and robot moved right. Row2 PULL: robot grasps box left edge; robot leans left pulling; both moved left. Row3 LIFT: robot beside box on ground; robot raises box; robot holds box high overhead exposing a bright yellow star on ground underneath. Row4 OPEN: robot reaches lid; lid partly opens; lid wide open with empty dark interior. Row5 SHAKE: robot grasps box; box tilted left with motion marks; box tilted right with motion marks. Row6 BREAK: robot raises hand; hits box with burst; collapsed orange cardboard pieces. No star in other rows. Generous empty margin, consistent robot size about 55% panel height. The robot is the supplied pixel-art character, never a white smooth 3D robot. Output asset image.

## Background prompt

Use case: stylized-concept. Asset type: 16:9 Unity game room backdrop. Create a cozy minimalist pastel workshop, straight-on camera, pale warm ivory wall (#fffaeb) and light honey wood floor at bottom 18%, bright window and green plant only at far left edge, slim gray-blue bookshelf with a few books and plant only at far right edge, tiny warm pendant at upper right. Very large nearly flat warm ivory EMPTY center covering 70% of width for game sprites overlay. Style is soft premium 3D toy diorama inspired by children's educational puzzle game UI, calm blue and cream color palette, diffused daylight, subtle soft shadows. No characters, no robots, no boxes, no text, no UI, no watermarks. Environment only.

## Robot cutout prompt

Use case: background-extraction. Create a production Unity sprite from the supplied image. Preserve exactly the supplied small blue pixel-art robot: scalloped blue round head, dark navy rectangular face with cyan >_ symbol, little blue body with >_ chest, arms and feet. Remove the entire black background and ALL application buttons/interface below the robot. Entire robot visible, centered, unchanged pose and design, no additions. Output a genuinely transparent PNG with alpha, isolated robot only, no floor, no shadow, no writing other than its existing cyan face/chest symbols. Use crisp pixel-art edges and generous transparent padding.

Cutout QA: RGBA 1266x1242; alpha 0..255; 1,143,736 fully transparent pixels. Runtime uses the transparent cutout; original reference remains unmodified.

## Transparent animation update and box lifting robot

Tool: built-in image_gen. Original opaque atlas backed up as RoomOne_Actions_OpaqueOriginal.png.
The standard 18 frames are now RGBA. Dedicated reverse-lift strip uses the user's approved orange arms and legs. Runtime PixelCutout shader clips residual low-alpha extraction pixels; generated files are retained without programmatic bitmap edits.

### Standard atlas background removal
Use case: background-extraction. Edit this Unity animation sprite sheet. Remove ALL pale cream background, floor fill and opaque background rectangles from the ENTIRE sheet, making these pixels genuinely transparent alpha=0. Keep all 18 robot-and-box drawings, pixel-art colors, motion marks and star, and preserve the precise 3 columns by 6 rows layout and relative positions. Same square canvas layout, 18 equally sized 2:1 cells. Each cell is a complete isolated pair of characters/props; preserve every complete object and every pose. No added labels, no borders, no checkerboard pattern, no solid backdrop, no scenery. Transparent PNG with real alpha channel, crisp pixel-art edges without cream halos. Preserve original >_ blue scalloped robot design and orange cardboard box. Preserve row order: push pull lift open shake break; each row has exactly 3 frames.

### Final box-with-limbs strip
Transparent background sprite sheet for Unity, genuine alpha PNG. THREE equal panels stacked vertically: ONE COLUMN x THREE ROWS, canvas 1024x1536, each cell 1024x512. Create 'The box lifts the robot.' Match the blue scalloped pixel-art robot in the reference, and orange cardboard cube. The BOX has TWO small ORANGE ARMS with mitt hands and TWO stubby ORANGE LEGS with feet. NO face on box, no other characters. TOP cell y0..511: living orange box stands left of blue robot, arms reaching toward robot, box's two feet firmly on invisible ground. MIDDLE cell y512..1023: orange box bends its arms upward, both hands holding robot around waist, robot lifted a little with feet off ground. BOTTOM cell y1024..1535: orange box's two arms fully raised carrying robot HIGH ABOVE the box's head, hands visibly contacting and supporting robot's feet or hips. Box stays on ground on its two legs, robot passively raised with little feet dangling and arms spread. EXACT same camera scale and reference robot in every cell. Robot never lifts box. Each complete group must fit wholly within own cell with at least 40 pixels transparent margin. Keep total group height <=420px so top of robot never clips. Fixed invisible floor baseline at relative y460 of each cell. Crisp pixel-art, blue scalloped robot head, navy screen cyan >_ face and cyan >_ chest, orange box distinct little arms and legs. TRANSPARENT BACKGROUND ONLY, no floor, no shadows, no glow, no checkerboard, no text, no stars, no borders.

### Box-with-limbs background removal
背景透過してください。青いロボットと、手足が生えたオレンジ色の箱の3つのポーズはそのまま保ち、背景の市松模様だけを除去してください。本物の透明背景のRGBA PNGで出力。Make background transparent with real alpha. Preserve the image exactly; only remove checkerboard background.
