# Terrain Kit source

Created locally on 2026-09-15 with Blender 4.5.9 LTS and the included deterministic Python generator, ArtSource/Common/TerrainKit/build_terrain.py. All delivered geometry is newly constructed from primitive polygon surfaces. All materials use uniform colors. No external mesh, texture, paid asset or Nintendo game asset is included.

Visual direction: user requested a cheerful rounded 3D platformer terrain kit. Room One workshop colors were referenced from https://github.com/Atsu-Taiyo/codex-hackathon0915 at commit 6322f8a744344b1c7802cfbd2528bc3dcf6980a9 (docs/ROOM_ONE.md, ArtSource/RoomOne/PROMPTS.md and docs/RoomOne/TransparentRoom.png). The existing repository screenshot is used as a reference only; it is not a runtime texture or part of the new geometry.

Each FBX is one individually placeable mesh with named material slots and UVs. Shared source file: ArtSource/Common/TerrainKit/TerrainKit.blend. Texture dependencies: none. Export settings: meters, FBX binary 7400, -Z forward/Y up, applied axis conversion, no animation, cameras or lights.

Intended use: static terrain. Backdrop_* meshes are background-only suggestions of future stages; no gameplay is implemented. Unity menu helper is supplied separately and has not been compiled in Unity in this environment. See docs/TERRAIN_KIT.md and ArtSource/Common/TerrainKit/Validation records for checks and limits.
