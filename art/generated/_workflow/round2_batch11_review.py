"""Reproduce batch-11 figure and counter previews; production images are read-only."""
import json
import sys
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / 'art/tools'))
import artlib, scene, study, validate

OUT = ROOT / 'art/generated/_review'
manifest = artlib.load_manifest()
specs = {s['id']: s for s in manifest['assets']}
figures = [s for s in manifest['assets'] if s['category'] == 'figure']
BG = (38, 34, 45, 255)
FG = (245, 233, 198, 255)
font = study._font(16)

def asset(sid):
    return artlib.open_rgba(str(ROOT / specs[sid]['output']))

def text(im, at, value):
    ImageDraw.Draw(im).text(at, value, fill=FG, font=font)

def figure_page(group, path, title):
    out = Image.new('RGBA', (1600, 50 + 680 * ((len(group) + 3) // 4)), BG)
    text(out, (16, 10), title)
    for i, spec in enumerate(group):
        x, y = (i % 4) * 400 + 8, (i // 4) * 680 + 48
        text(out, (x, y), spec['id'] + ' - ' + str(spec['height']) + 'px')
        im = asset(spec['id'])
        out.alpha_composite(im.resize((384, 640), Image.Resampling.NEAREST), (x, y + 28))
    out.save(path)

for page, start in enumerate(range(0, len(figures), 12), 1):
    figure_page(figures[start:start + 12], OUT / f'round2_batch11_figures_{page}.png',
                'C2 / 4X / NO OUTLINES - FIGURES (Hob is the approved anchor)')

def counter(spec, tier=1, time='midday'):
    key = scene.TIERS[tier]
    layers = {name: asset(f'{name}.{key}') for name in ('place', 'frame', 'shelf', 'counter', 'sheet')}
    lantern = asset(f'prop.lantern_{key}')
    layers.update(sky=asset(f'sky.{time}'), figure=asset(spec['id']),
                  lantern=lantern if scene.TIMES[time]['lit'] else validate._unlit(lantern))
    out = scene.compose(layers, tier, time, spec['height'], chest=validate._chest('wood', 'rusty'))
    for name, at in (('bubble', scene.UI['bubble'][:2]), ('hud', (0, 0))):
        p = Path(study.UI) / f'{name}.png'
        if p.exists():
            out.alpha_composite(artlib.open_rgba(str(p)), at)
    return out

for page, start in enumerate(range(0, len(figures), 8), 1):
    group = figures[start:start + 8]
    out = Image.new('RGBA', (1600, 50 + 452 * ((len(group) + 3) // 4)), BG)
    text(out, (16, 10), 'COUNTER CROP AT ACTUAL SCREEN SIZE - reference UI overlay, not live captures')
    for i, spec in enumerate(group):
        x, y = (i % 4) * 400, (i // 4) * 452 + 48
        text(out, (x + 8, y), spec['id'])
        out.alpha_composite(counter(spec).crop((0, 56, 400, 476)), (x, y + 28))
    out.save(OUT / f'round2_batch11_counter_{page}.png')

figure_page([specs[s] for s in ('figure.aldo', 'figure.king', 'figure.hood', 'figure.mara')],
            OUT / 'round2_batch11_reveals.png', 'IDENTITY REVEALS - Aldo / King and Hood / Mara')

for sid, tier, time in (('figure.vexis', 1, 'midday'), ('figure.orrin', 3, 'midday'),
                         ('figure.vane', 3, 'night'), ('figure.nim', 2, 'morning')):
    counter(specs[sid], tier, time).save(OUT / f'round2_batch11_{sid.split(".")[1]}_playview.png')
print('Wrote four figure pages, five counter-crop pages, identity pairs and four full playviews.')
