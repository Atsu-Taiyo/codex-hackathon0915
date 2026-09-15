"""Reimport every exported FBX into an empty Blender scene and compare geometry."""
import bpy, json, sys, math
from pathlib import Path
from mathutils import Vector
root=Path(sys.argv[sys.argv.index('--')+1]).resolve()
source=json.loads((root/'ArtSource/Common/TerrainKit/Validation/mesh-report.json').read_text())
results=[]
for expected in source['assets']:
    bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
    path=root/'Assets/_Project/Content/Common/TerrainKit/Models'/(expected['name']+'.fbx')
    assert path.read_bytes().startswith(b'Kaydara FBX Binary')
    bpy.ops.import_scene.fbx(filepath=str(path),use_anim=False)
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    assert len(meshes)==1, (path.name,len(meshes))
    ob=meshes[0]; ob.data.calc_loop_triangles()
    points=[ob.matrix_world@v.co for v in ob.data.vertices]
    lo=[min(v[i] for v in points) for i in range(3)]
    hi=[max(v[i] for v in points) for i in range(3)]
    dims=[hi[i]-lo[i] for i in range(3)]
    error=max(abs(a-b) for a,b in zip(dims,expected['blender_dimensions_xyz_m']))
    assert error<.0001,(path.name,dims,expected['blender_dimensions_xyz_m'])
    assert len(ob.data.loop_triangles)==expected['triangles']
    used=sorted({ob.data.materials[p.material_index].name.split('.')[0] for p in ob.data.polygons})
    assert all(n.startswith('Terrain_') for n in used)
    assert ob.data.uv_layers.active is not None
    assert ob.matrix_world.translation.length<.0001,(path.name,list(ob.matrix_world.translation))
    results.append({'name':expected['name'],'fbx_bytes':path.stat().st_size,
                    'dimension_error_m':error,'triangles':len(ob.data.loop_triangles),
                    'uv_present':True,'origin_at_zero':True,'used_materials':used})
out={'passed':len(results),'total':source['asset_count'],'method':'FBX reimport in Blender '+bpy.app.version_string,
     'checks':['binary header','one mesh per FBX','dimensions within 0.1 mm','triangle count preserved','UVs','material names','origin'],
     'unity_editor_test':'Not run. Unity Editor is not installed. Helper C# not compiled in Unity.', 'results':results}
(root/'ArtSource/Common/TerrainKit/Validation/fbx-roundtrip.json').write_text(json.dumps(out,indent=2))
print('FBX_ROUNDTRIP_OK',len(results))
