"""Read-only binary FBX integrity and geometry check (stdlib only)."""
import array
import json
import math
import struct
import sys
import zlib
from pathlib import Path

path = Path(sys.argv[1])
data = path.read_bytes()
assert data[:23] == b'Kaydara FBX Binary  \x00\x1a\x00'
version = struct.unpack_from('<I', data, 23)[0]
header = '<QQQB' if version >= 7500 else '<IIIB'
header_size = struct.calcsize(header)

def node(pos):
    end, count, size, name_size = struct.unpack_from(header, data, pos)
    if end == 0:
        return None, pos + header_size
    assert pos < end <= len(data)
    pos += header_size
    name = data[pos:pos + name_size].decode()
    pos += name_size
    prop_end = pos + size
    props = []
    for _ in range(count):
        typ = chr(data[pos]); pos += 1
        if typ in 'YCLFDI':
            fmt = {'Y':'h','C':'?','L':'q','F':'f','D':'d','I':'i'}[typ]
            value = struct.unpack_from('<' + fmt, data, pos)[0]
            pos += struct.calcsize(fmt)
        elif typ in 'SR':
            n = struct.unpack_from('<I', data, pos)[0]; pos += 4
            value = data[pos:pos+n]; pos += n
            if typ == 'S': value = value.decode('utf-8', errors='replace')
        elif typ in 'fdlibc':
            n, enc, length = struct.unpack_from('<III', data, pos); pos += 12
            raw = data[pos:pos+length]; pos += length
            assert enc in (0, 1)
            if enc: raw = zlib.decompress(raw)
            fmt = {'f':'f','d':'d','l':'q','i':'i','b':'b','c':'B'}[typ]
            value = array.array(fmt); value.frombytes(raw)
            if sys.byteorder != 'little': value.byteswap()
            assert len(value) == n
        else:
            raise ValueError(typ)
        props.append(value)
    assert pos == prop_end
    children = []
    while pos < end:
        child, pos = node(pos)
        if child is None: break
        children.append(child)
    assert pos == end
    return {'name':name, 'props':props, 'children':children}, end

nodes = []
pos = 27
while pos < len(data):
    item, pos = node(pos)
    if item is None: break
    nodes.append(item)

def all_nodes(items):
    for item in items:
        yield item
        yield from all_nodes(item['children'])

flat = list(all_nodes(nodes))
geometries = []
for item in flat:
    if item['name'] != 'Geometry': continue
    children = {c['name']:c for c in item['children']}
    if 'Vertices' not in children: continue
    vertices = children['Vertices']['props'][0]
    indices = children['PolygonVertexIndex']['props'][0]
    assert len(vertices) % 3 == 0
    assert all(math.isfinite(v) for v in vertices)
    assert all(0 <= (i if i >= 0 else -i-1) < len(vertices)//3 for i in indices)
    polys = sum(i < 0 for i in indices)
    assert polys > 0 and indices[-1] < 0
    geometries.append({'vertices':len(vertices)//3, 'polygons':polys,
        'polygon_indices':len(indices),
        'bounds_min':[min(vertices[a::3]) for a in range(3)],
        'bounds_max':[max(vertices[a::3]) for a in range(3)],
        'has_uv':'LayerElementUV' in children,
        'has_normals':'LayerElementNormal' in children})
assert geometries and all(g['has_uv'] and g['has_normals'] for g in geometries)
result = {'file':path.name, 'bytes':len(data), 'fbx_version':version,
    'geometries':geometries,
    'materials':sum(i['name']=='Material' for i in flat),
    'deformers':sum(i['name']=='Deformer' for i in flat),
    'texture_references':[i['props'] for i in flat if i['name'] in ('RelativeFilename','FileName')],
    'result':'PASS: binary structure, finite vertices, valid indices, UVs and normals'}
print(json.dumps(result, indent=2))
