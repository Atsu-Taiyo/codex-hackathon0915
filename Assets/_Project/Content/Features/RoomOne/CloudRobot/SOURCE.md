# Source and usage conditions

- Character direction: supplied by the project contributor; blue cloud-shaped robot, navy screen, cyan terminal `>_` face, white terminal chest mark.
- Reference render: OpenAI image generation, edited to adopt the terminal face on 2026-09-15. Input reference and exact edit prompt are in `ArtSource/Features/RoomOne/CloudRobot/`.
- Mesh and textures: generated with Tripo HD v2.5 on 2026-09-15, free plan. Provider: Tripo / Holymolly Ltd.
- [Generated model](https://studio.tripo3d.ai/ja/workspace/generate/b73a8397-3def-4c9a-bdee-5d54b2b9e67e).
- Export: normal Tripo FBX export, Blender preset, 4K textures. Mesh/texture bytes are unchanged from that export. The FBX was renamed to `CloudRobot_Terminal.fbx`; its texture filenames and relative directory were preserved.
- [Free plan: public models, non-commercial use](https://www.tripo3d.ai/pricing).
- [Tripo Terms, section 5.2.1](https://www.tripo3d.ai/terms): Tripo retains the rights to free-user inputs and outputs. This repository does not grant a separate open-source or commercial license to these model/texture files.
- [Provider's commercial-use guidance](https://www.tripo3d.ai/help/privacy-policy/how-to-use-tripo-models-commercially).

The Unity helper assigns the base-color texture and a plastic-like material, normalizes model height, and creates a prefab. No mesh simplification, rigging, animation, or texture repainting was performed. The original normal/metallic/roughness/RM maps are supplied but not assigned by the helper.
