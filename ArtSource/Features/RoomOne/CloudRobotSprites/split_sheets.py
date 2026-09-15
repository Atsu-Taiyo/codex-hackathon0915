#!/usr/bin/env python3
"""Reproduce lossless sprite crops, source mapping and the offline preview (Pillow)."""
import base64
import hashlib
import html
import io
import json
from pathlib import Path
import uuid
from PIL import Image, ImageDraw

SOURCE = Path(__file__).resolve().parent
ROOT = SOURCE.parents[3]
TARGET = ROOT / 'Assets/_Project/Content/Features/RoomOne/CloudRobotSprites'
ACTIONS = [
    ('Push', 'push', '押す', ['Ready', 'Push', 'Finish']),
    ('Pull', 'pull', '引く', ['Ready', 'Pull', 'Finish']),
    ('Lift', 'lift', '持ち上げる', ['Ready', 'Raise', 'Success']),
    ('Open', 'open', '開ける', ['Closed', 'OpenFlap', 'Opened']),
    ('Shake', 'shake', '振る', ['Hold', 'ShakeLeft', 'ShakeRight']),
    ('Break', 'break', '壊す', ['Ready', 'Impact', 'Broken']),
]

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def make_meta(path, kind):
    meta = Path(str(path) + '.meta')
    if meta.exists():
        return  # Existing GUIDs must remain stable on regeneration.
    guid = uuid.uuid5(uuid.NAMESPACE_URL, 'codex-hackathon0915/' + path.relative_to(ROOT).as_posix()).hex
    if kind == 'folder':
        body = 'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    elif kind == 'sprite':
        template = (SOURCE / 'sprite-importer-template.txt').read_text()
        body = template.replace('SPRITE_ID', uuid.uuid5(uuid.NAMESPACE_URL, guid + '/sprite').hex)
    else:
        body = 'TextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    meta.write_text('\n'.join(line.rstrip() for line in f'fileFormatVersion: 2\nguid: {guid}\n{body}'.splitlines()) + '\n')

def main():
    TARGET.mkdir(parents=True, exist_ok=True)
    sheets = {key: Image.open(SOURCE / name).convert('RGBA') for key, name in {
        'actions': 'CloudRobot_Actions_Sheet.png', 'box_lift': 'CloudRobot_BoxLiftsRobot_Sheet.png'}.items()}
    sources = {key: {'file': im.filename if getattr(im, 'filename', '') else ('CloudRobot_Actions_Sheet.png' if key == 'actions' else 'CloudRobot_BoxLiftsRobot_Sheet.png'), 'size': list(im.size)} for key, im in sheets.items()}
    for entry in sources.values():
        entry['sha256'] = digest(SOURCE / entry['file'])
    clips = []
    rows = [0, 210, 405, 650, 845, 1040, 1254]
    for row, (name, action, label, stages) in enumerate(ACTIONS):
        clips.append({'id': f'robot:{action}:box', 'folder': 'Robot' + name + 'Box', 'label': 'ロボットが箱を' + label,
                      'sheet': 'actions', 'source_row': row + 1, 'frame_duration_ms': 700,
                      'canvas_size': [418, 245], 'stages': stages,
                      'rects': [[col * 418, rows[row], (col + 1) * 418, rows[row + 1]] for col in range(3)]})
    clips.append({'id': 'box:lift:robot', 'folder': 'BoxLiftRobot', 'label': '箱がロボットを持ち上げる',
                  'sheet': 'box_lift', 'source_row': None, 'frame_duration_ms': 700,
                  'canvas_size': [1024, 561], 'stages': ['Ready', 'Raise', 'Overhead'],
                  'rects': [[0, 0, 1024, 450], [0, 450, 1024, 975], [0, 975, 1024, 1536]]})
    assets = []
    for clip in clips:
        clip['frames'] = []
        for number, (rect, stage) in enumerate(zip(clip.pop('rects'), clip.pop('stages')), 1):
            filename = f"Frames/{clip['folder']}/RoomOne_CloudRobot_{clip['folder']}_{number:02d}_{stage}.png"
            frame = export(sheets[clip['sheet']], rect, clip['canvas_size'], filename)
            frame.update({'order': number, 'stage': stage, 'source': clip['sheet']})
            clip['frames'].append(frame)
            assets.append(frame)
    characters = []
    for name, rect in [('RobotIdle', [65, 20, 235, 210]), ('BoxIdle', [237, 85, 370, 205])]:
        frame = export(sheets['actions'], rect, [rect[2]-rect[0], rect[3]-rect[1]], f'Characters/RoomOne_CloudRobot_{name}.png')
        frame.update({'id': name, 'source': 'actions'})
        characters.append(frame)
        assets.append(frame)
    manifest = {'schema_version': 1, 'feature': 'RoomOne', 'asset_type': '2D pose sequences',
                'source_directory': SOURCE.relative_to(ROOT).as_posix(), 'sources': sources,
                'coordinate_system': 'source_rect_ltrb_px: top-left origin; right/bottom exclusive',
                'placement': 'Native source pixels, no scaling; bottom-aligned transparent padding to a common canvas per sequence.',
                'alpha': 'Source RGBA preserved byte-for-byte inside each crop.',
                'runtime_integration': 'Reference/import assets only; no Animator clips, rig, scene or runtime changes.',
                'clips': clips, 'characters': characters}
    (TARGET / 'animation-manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n')
    preview(clips, characters)
    for path in [TARGET] + sorted(TARGET.rglob('*')):
        if path.suffix == '.meta':
            continue
        make_meta(path, 'folder' if path.is_dir() else 'sprite' if path.suffix == '.png' else 'text')
    report = {'scene_count': len(clips), 'pose_png_count': 21, 'standalone_character_png_count': len(characters),
              'verified': ['Every output crop matches source RGBA bytes at recorded offset.', 'All output PNGs decode as RGBA and have visible pixels.',
                           'All scene frames share the recorded canvas size.', 'Source hashes recorded; native pixels are not rescaled.'],
              'unity_editor_import_tested': False}
    (SOURCE / 'validation.json').write_text(json.dumps(report, indent=2) + '\n')
    print(json.dumps(report, indent=2))

def export(source, rect, canvas_size, filename):
    crop = source.crop(rect)
    offset = [0, canvas_size[1] - crop.height]
    assert 0 <= rect[0] < rect[2] <= source.width and 0 <= rect[1] < rect[3] <= source.height
    assert offset[1] >= 0 and crop.width <= canvas_size[0]
    canvas = Image.new('RGBA', canvas_size)
    canvas.paste(crop, offset)  # No mask: preserve straight RGBA, including original alpha.
    path = TARGET / filename
    path.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(path)
    check = Image.open(path)
    assert check.mode == 'RGBA' and check.getchannel('A').getbbox()
    assert check.crop((offset[0], offset[1], offset[0]+crop.width, offset[1]+crop.height)).tobytes() == crop.tobytes()
    return {'file': filename, 'source_rect_ltrb_px': rect, 'canvas_size': canvas_size, 'paste_offset_px': offset, 'sha256': digest(path)}

def png_url(path):
    # Preview compositing correctly respects alpha; transparent RGB is not displayed.
    im = Image.open(path)
    im.thumbnail((640, 360))
    stream = io.BytesIO()
    im.save(stream, format='PNG')
    return 'data:image/png;base64,' + base64.b64encode(stream.getvalue()).decode()

def preview(clips, characters):
    cards = []
    contact = Image.new('RGB', (1050, 7 * 240), '#eef1f6')
    draw = ImageDraw.Draw(contact)
    preview_data = []
    for row, clip in enumerate(clips):
        images = [png_url(TARGET / frame['file']) for frame in clip['frames']]
        preview_data.append({'id': clip['id'], 'images': images})
        cards.append(f'<article><h2>{html.escape(clip["label"])}</h2><code>{html.escape(clip["id"])}</code><img id="scene-{row}" src="{images[0]}" alt="{html.escape(clip["label"])}"><p class="step" id="step-{row}">01 / 03</p></article>')
        draw.text((18, row * 240 + 8), clip['id'], fill='#182332')
        for col, frame in enumerate(clip['frames']):
            im = Image.open(TARGET / frame['file']); im.thumbnail((330, 200))
            contact.paste(im, (col*350+(350-im.width)//2, row*240+27+(200-im.height)//2), im)
            draw.text((col*350+12, row*240+222), f'{frame["order"]:02d} {frame["stage"]}', fill='#182332')
    contact.save(SOURCE / 'scene-contact-sheet.png')
    document = (SOURCE / 'preview-template.html').read_text()
    document = document.replace('CARDS', ''.join(cards)).replace('DATA', json.dumps(preview_data))
    (SOURCE / 'preview.html').write_text(document)

if __name__ == '__main__':
    main()
