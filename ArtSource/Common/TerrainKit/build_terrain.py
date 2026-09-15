"""Run with Blender 4.5: blender -b --python build_terrain.py -- /absolute/repository/root"""
import bpy, bmesh, json, math, sys
from pathlib import Path
from mathutils import Vector

ROOT = Path(sys.argv[sys.argv.index('--') + 1]).resolve()
MODELS = ROOT / 'Assets/_Project/Content/Common/TerrainKit/Models'
for p in (MODELS, ROOT/'docs/TerrainKit', ROOT/'ArtSource/Common/TerrainKit/Validation'): p.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1.0
COLORS = {'Grass':'86BD45','GrassEdge':'639A38','Sand':'E8C889','Soil':'C6955C',
          'SoilLight':'D8AE72','SoilDeep':'AE7B49','Ice':'C5E9E6','IceRock':'87ACA9',
          'Amber':'CB8863','AmberLight':'DEA579','Studio':'F3EEE4','Ink':'374A48',
          'Cream':'FFFAEB','Honey':'DCAD6B','HoneyLight':'E9C28B','WoodEdge':'BD8C50'}
def lin(v): return v/12.92 if v <= .04045 else ((v+.055)/1.055)**2.4
MATS = {}
for key, hx in COLORS.items():
    c = tuple(lin(int(hx[i:i+2],16)/255) for i in (0,2,4)) + (1,)
    m = bpy.data.materials.new('Terrain_'+key)
    m.diffuse_color=c; m.use_nodes=True
    bsdf=m.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value=c
    bsdf.inputs['Roughness'].default_value=.8
    MATS[key]=m

def finish(name, verts, faces, ids, bevel=.025):
    mesh=bpy.data.meshes.new(name+'_Mesh'); mesh.from_pydata(verts, [], faces); mesh.update()
    ob=bpy.data.objects.new(name,mesh); scene.collection.objects.link(ob)
    for m in MATS.values(): mesh.materials.append(m)
    keys=list(MATS)
    for f, key in zip(mesh.polygons,ids): f.material_index=keys.index(key)
    bm=bmesh.new(); bm.from_mesh(mesh)
    bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
    bm.to_mesh(mesh); bm.free()
    bpy.ops.object.select_all(action='DESELECT'); ob.select_set(True); bpy.context.view_layer.objects.active=ob
    if bevel:
        mod=ob.modifiers.new('Soft edges','BEVEL'); mod.width=bevel; mod.segments=3
        mod.affect='EDGES'; mod.limit_method='ANGLE'; mod.angle_limit=.4
        bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=1.15,island_margin=.025)
    bpy.ops.object.mode_set(mode='OBJECT')
    # Actual material colors, geometry and UVs are exported; no external textures required.
    ob['purpose']='background-only' if name.startswith('Backdrop_') else 'modular terrain'
    ob['units']='meters'; ob['grid']=2.0
    return ob

def rect(w=2,d=2): return [(-w/2,-d/2),(w/2,-d/2),(w/2,d/2),(-w/2,d/2)]
def rounded(w,d,r=.35):
    out=[]
    for cx,cy,a in [(w/2-r,-d/2+r,-90),(w/2-r,d/2-r,0),(-w/2+r,d/2-r,90),(-w/2+r,-d/2+r,180)]:
        for j in range(7):
            t=math.radians(a+90*j/6); out.append((cx+r*math.cos(t),cy+r*math.sin(t)))
    return out

def prism(name,poly,depth=.5,top='Grass',rise=None,bevel=.025):
    n=len(poly); verts=[]
    side='GrassEdge' if top=='Grass' else top
    for level in range(4):
        for x,y in poly:
            h=rise(x,y) if rise else 0
            z=[-depth, -depth+(h+depth)*.4, h-min(.12,(h+depth)*.2), h][level]
            verts.append((x,y,z))
    faces=[tuple(reversed(range(n)))]; ids=['SoilDeep']
    for level,key in enumerate(['Soil','SoilLight',side]):
        for j in range(n):
            k=(j+1)%n; faces.append((level*n+j,level*n+k,(level+1)*n+k,(level+1)*n+j)); ids.append(key)
    faces.append(tuple(range(3*n,4*n))); ids.append(top)
    return finish(name,verts,faces,ids,bevel)

def steps(name):
    # Extruded stair profile: four 0.5 m treads, each rises 0.25 m.
    profile=[(-1,-.5),(1,-.5),(1,1),(.5,1),(.5,.75),(0,.75),(0,.5),(-.5,.5),(-.5,.25),(-1,.25)]
    n=len(profile); verts=[(x,y,z) for x in [-1,1] for y,z in profile]
    faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]; ids=['SoilLight','SoilLight']
    for i in range(n):
        j=(i+1)%n; faces.append((i,j,j+n,i+n))
        ids.append('Grass' if abs(profile[i][1]-profile[j][1])<1e-6 and profile[i][1]>0 else 'SoilLight')
    return finish(name,verts,faces,ids,.018)

assets=[]
assets.append(prism('Terrain_GrassTile_2x2',rect()))
assets.append(prism('Terrain_GrassFloor_4x4',rect(4,4)))
assets.append(prism('Terrain_GrassCliff_2x2',rect(),depth=2))
# One rounded exposed corner; remaining edges stay on the 2 m construction grid.
poly=[(-1,-1),(1,-1),(1,.4)]
for j in range(1,9):
    a=math.pi/2*j/8; poly.append((.4+.6*math.cos(a),.4+.6*math.sin(a)))
poly.append((-1,1))
assets.append(prism('Terrain_GrassOuterCorner_2x2',poly))
assets.append(prism('Terrain_GrassInnerCorner_2x2',[(-1,-1),(1,-1),(1,0),(0,0),(0,1),(-1,1)]))
assets.append(prism('Terrain_GrassRamp_2x4_Rise1',rect(2,4),rise=lambda x,y:(y+2)/4))
assets.append(steps('Terrain_GrassSteps_2x2_Rise1'))
assets.append(prism('Terrain_SandTile_2x2',rect(),top='Sand'))
assets.append(prism('Terrain_SandRamp_2x4_Rise1',rect(2,4),top='Sand',rise=lambda x,y:(y+2)/4))
assets.append(prism('Terrain_ShoreSlope_2x2_Drop05',rect(),depth=.8,top='Sand',rise=lambda x,y:-(y+1)/4))
assets.append(prism('Terrain_GrassRound_4m',rounded(4,4,1.9),depth=1))
assets.append(prism('Terrain_SandCliff_2x2',rect(),depth=2,top='Sand'))

def join_parts(name,parts):
    bpy.ops.object.select_all(action='DESELECT')
    for ob in parts: ob.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]; bpy.ops.object.join()
    ob=parts[0]; ob.name=name
    scene.cursor.location=(0,0,0); bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    ob['purpose']='background-only' if name.startswith('Backdrop_') else 'modular terrain'; return ob

def uniform(ob,key):
    for face in ob.data.polygons: face.material_index=list(MATS).index(key)
    return ob

# Workshop structural surfaces grounded in the Room One artwork added during production.
boards=[]
for i in range(8):
    board=uniform(prism('FloorBoard',rect(2,.25),depth=.3,top='Honey',bevel=.006),
                  'Honey' if i%2==0 else 'HoneyLight')
    board.location.y=-.875+i*.25; boards.append(board)
assets.append(join_parts('Terrain_WorkshopFloor_2x2',boards))

def wall_piece(name,poly):
    # One closed solid, including the skirting color band: no coplanar overlap.
    n=len(poly); verts=[(x,y,z) for z in [0,.14,2.5] for x,y in poly]
    faces=[tuple(reversed(range(n))),tuple(range(2*n,3*n))]; ids=['Honey','Cream']
    for layer,key in enumerate(['Honey','Cream']):
        for j in range(n):
            k=(j+1)%n
            faces.append((layer*n+j,layer*n+k,(layer+1)*n+k,(layer+1)*n+j)); ids.append(key)
    return finish(name,verts,faces,ids,.008)

assets.append(wall_piece('Terrain_WorkshopWall_2m',rect(2,.2)))
assets.append(wall_piece('Terrain_WorkshopCorner_2m',[(-1,-1),(-.8,-1),(-.8,.8),(1,.8),(1,1),(-1,1)]))

def mesa_part(name,w,d,h,x,y,top):
    ob=prism(name,rounded(w,d,min(w,d)*.24),depth=h,top=top,bevel=.05)
    ob.location=(x,y,h)
    for slot in ob.material_slots:
        if slot.material.name in ['Terrain_Soil','Terrain_SoilDeep','Terrain_SoilLight']:
            slot.material=MATS['IceRock' if top=='Ice' else 'Amber']
    return ob

assets.append(join_parts('Backdrop_AmberMesas',[
    mesa_part('MesaA',3,2.8,2.3,-1.2,0,'Sand'),mesa_part('MesaB',2.3,2.2,3.4,1.1,.3,'Sand')]))
assets.append(join_parts('Backdrop_IceTerraces',[
    mesa_part('IceA',4,3,1.4,0,0,'Ice'),mesa_part('IceB',2.8,2.1,2.5,.5,.35,'Ice'),
    mesa_part('IceC',1.6,1.3,3.5,.9,.55,'Ice')]))
# Natural canyon opening, not a gate or gameplay mechanism.
parts=[mesa_part('ArchL',1.3,2,2.6,-1.8,0,'Sand'),mesa_part('ArchR',1.3,2,2.6,1.8,0,'Sand')]
verts=[]; seg=16
for y in [-.8,.8]:
    for radius in [1.2,2.45]:
        for i in range(seg+1):
            a=math.pi*i/seg; verts.append((radius*math.cos(a),y,2.0+radius*math.sin(a)*.65))
n=seg+1; faces=[]; ids=[]
for i in range(seg):
    for f,key in [((i,i+1,n+i+1,n+i),'AmberLight'),((2*n+i,3*n+i,3*n+i+1,2*n+i+1),'AmberLight'),
                  ((i,2*n+i,2*n+i+1,i+1),'Amber'),((n+i,n+i+1,3*n+i+1,3*n+i),'Sand')]:
        faces.append(f); ids.append(key)
faces.extend([(0,n,3*n,2*n),(seg,2*n+seg,3*n+seg,n+seg)]); ids.extend(['Amber','Amber'])
parts.append(finish('ArchSpan',verts,faces,ids,.06))
assets.append(join_parts('Backdrop_CanyonArch',parts))

report=[]
for i,ob in enumerate(assets):
    # Export every object from its functional pivot, before catalog positioning.
    bpy.ops.object.select_all(action='DESELECT'); ob.select_set(True); bpy.context.view_layer.objects.active=ob
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    ob.data.calc_loop_triangles()
    bm=bmesh.new(); bm.from_mesh(ob.data)
    bad=sum(1 for e in bm.edges if not e.is_manifold)
    zero=sum(1 for f in bm.faces if f.calc_area()<1e-10)
    volume=bm.calc_volume(signed=True); bm.free()
    assert bad==0 and zero==0 and volume>0, (ob.name,bad,zero,volume)
    assert all(math.isfinite(c) for v in ob.data.vertices for c in v.co)
    dims=list(ob.dimensions)
    report.append({'name':ob.name,'vertices':len(ob.data.vertices),'triangles':len(ob.data.loop_triangles),
                   'blender_dimensions_xyz_m':dims,'unity_dimensions_xyz_m':[dims[0],dims[2],dims[1]],
                   'non_manifold_edges':bad,'zero_area_faces':zero,'signed_volume':volume,
                   'pivot':'base at zero' if ob.name.startswith('Backdrop_') or 'Wall' in ob.name or 'WorkshopCorner' in ob.name else 'surface zero; incline uses low end, shore uses high end',
                   'collider':not ob.name.startswith('Backdrop_'),'materials':sorted({p.material_index for p in ob.data.polygons})})
    bpy.ops.export_scene.fbx(filepath=str(MODELS/(ob.name+'.fbx')),use_selection=True,object_types={'MESH'},
        apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',
        bake_space_transform=True,use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,
        bake_anim=False,path_mode='AUTO')
    ob.hide_render=True

(ROOT/'ArtSource/Common/TerrainKit/Validation/mesh-report.json').write_text(json.dumps({'blender_version':bpy.app.version_string,
    'asset_count':len(report),'assets':report,'unity_editor_test':'NOT RUN: Unity Editor unavailable'},indent=2))
(ROOT/'ArtSource/Common/TerrainKit/palette.json').write_text(json.dumps(COLORS,indent=2))

# Editable asset library, each object positioned on a shelf, no assembled room/map.
for i,ob in enumerate(assets):
    ob.location=((i%4)*6,(i//4)*7,0); ob.hide_render=False
    ob['export_pivot_note']='Set location to 0,0,0 before individual export; geometry is already pivot-relative.'
scene.world.color=(.6,.6,.6)
scene.render.engine='CYCLES'; scene.cycles.samples=24
scene.cycles.use_denoising=True
scene.render.resolution_x=1800; scene.render.resolution_y=1350; scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
scene.view_settings.look='AgX - Medium High Contrast'
scene.view_settings.exposure=.7
scene.world.use_nodes=True; scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.75,.81,.85,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.65
bpy.ops.object.camera_add(location=(20,-25,32)); cam=bpy.context.object; cam.name='CatalogCamera'
scene.camera=cam; cam.data.type='ORTHO'; cam.data.lens=50
def aim(ob,at): ob.rotation_euler=(Vector(at)-ob.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.light_add(type='AREA',location=(-3,-5,18)); light=bpy.context.object
light.name='StudioKey'; light.data.energy=2400; light.data.shape='DISK'; light.data.size=12; aim(light,(6,6,0))
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-2.04)); floor=bpy.context.object
floor.name='PreviewOnly_StudioFloor'; floor.data.materials.append(MATS['Studio'])

def render_group(indices,filename,cols):
    for ob in assets: ob.hide_render=True
    chosen=[assets[i] for i in indices]
    # Object-only sheets with consistent ground plane; normalize large background silhouettes for presentation.
    for j,ob in enumerate(chosen):
        ob.hide_render=False; ob.location=((j%cols)*6,(j//cols)*6.5,0)
        bottom=min(v.co.z for v in ob.data.vertices); ob.location.z=-bottom
    rows=math.ceil(len(chosen)/cols); center=((cols-1)*3,(rows-1)*3.25,.4)
    floor.location.z=-.06
    cam.location=Vector(center)+Vector((11,-16,23)); aim(cam,center)
    scene.render.resolution_y=1350 if len(indices)>4 else 850
    bpy.context.view_layer.update()
    right=cam.rotation_euler.to_matrix() @ Vector((1,0,0))
    up=cam.rotation_euler.to_matrix() @ Vector((0,1,0))
    points=[ob.matrix_world @ v.co for ob in chosen for v in ob.data.vertices]
    xs=[(p-cam.location).dot(right) for p in points]
    ys=[(p-cam.location).dot(up) for p in points]
    cam.location += right*((min(xs)+max(xs))/2)+up*((min(ys)+max(ys))/2)
    aspect=scene.render.resolution_x/scene.render.resolution_y
    cam.data.ortho_scale=max(max(xs)-min(xs),(max(ys)-min(ys))*aspect)*1.18
    scene.render.filepath=str(ROOT/'docs/TerrainKit'/filename)
    bpy.ops.render.render(write_still=True)

render_group(list(range(12)),'Terrain_Parts.png',4)
render_group([12,13,14],'Room1_Workshop_Parts.png',3)
render_group([15,16,17],'Later_Stage_Backdrops.png',3)
for i,ob in enumerate(assets):
    ob.hide_render=False; ob.location=((i%4)*6,(i//4)*7,0)
floor.hide_render=True; floor.hide_viewport=True
cam.location=(24,-25,35); aim(cam,(9,10,0)); cam.data.ortho_scale=32
scene.render.resolution_y=1350
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            area.spaces.active.region_3d.view_location=(9,10,0)
            area.spaces.active.region_3d.view_distance=38
            area.spaces.active.shading.type='MATERIAL'
bpy.ops.object.select_all(action='DESELECT')
for ob in assets: ob.select_set(True)
bpy.context.view_layer.objects.active=assets[0]
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Common/TerrainKit/TerrainKit.blend'))
print('TERRAIN_BUILD_COMPLETE',len(assets),flush=True)
