# Terrain Kit production source

18 original static terrain meshes. Run from the repository root:

```sh
blender -b --python-exit-code 1 --python ArtSource/Common/TerrainKit/build_terrain.py -- "$PWD"
blender -b --python-exit-code 1 --python ArtSource/Common/TerrainKit/Validation/validate_fbx.py -- "$PWD"
```

The generator overwrites this kit's FBX exports, .blend source, previews and validation reports. It does not modify .meta files or any scene. Requires Blender 4.5.9 LTS; no extra Python packages. Runtime exports: Assets/_Project/Content/Common/TerrainKit/Models. Usage and limitations: docs/TERRAIN_KIT.md.
