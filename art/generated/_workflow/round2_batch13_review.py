"""Rebuild batch 13 review sheets from delivered native sprites; no asset edits."""
import json, math, textwrap
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

REPO = Path(__file__).resolve().parents[3]
OUT = REPO / 'art/generated/_review'
manifest = json.loads((REPO / 'art/manifest.json').read_text(encoding='utf-8'))
specs = [s for s in manifest['assets'] if s['batch'] == 13 or s['id'] == 'curio.monocle']
font = ImageFont.load_default(size=15)
small = ImageFont.load_default(size=12)
bg = (38,34,45,255)
ink = (245,233,198,255)

def label(draw, xy, value, width=31):
    draw.multiline_text(xy, '\n'.join(textwrap.wrap(value, width)), font=small, fill=ink, spacing=3)

def sprite(spec):
    return Image.open(REPO / spec['output']).convert('RGBA')

overview = Image.new('RGBA', (1120, 80 + math.ceil(len(specs)/7)*120), bg)
d = ImageDraw.Draw(overview)
d.text((20,16), 'Batch 13: all curios at their actual 2x UI scale', font=font, fill=ink)
d.text((20,42), 'Includes the approved batch-9s monocle anchor. No outlines; nearest-neighbour display.', font=small, fill=ink)
for i,s in enumerate(specs):
    x,y=(i%7)*160,(i//7)*120+80
    im=sprite(s);overview.alpha_composite(im.resize((56,56),Image.Resampling.NEAREST),(x+48,y))
    label(d,(x+8,y+68),s['title'],23)
overview.save(OUT/'round2_batch13_overview_2x.png')

for page,start in enumerate(range(0,len(specs),16),1):
    group=specs[start:start+16]
    im=Image.new('RGBA',(1120,80+math.ceil(len(group)/4)*210),bg);draw=ImageDraw.Draw(im)
    draw.text((20,15),f'Batch 13 / page {page}: actual 2x UI scale (left), 4x inspection (right)',font=font,fill=ink)
    draw.text((20,44),'Approved anchors are marked; images are processed native sprites, not generated source previews.',font=small,fill=ink)
    for i,s in enumerate(group):
        x,y=(i%4)*280+8,(i//4)*210+80
        label(draw,(x,y),s['title']+(' [anchor]' if s['batch']==9 else ''))
        a=sprite(s)
        im.alpha_composite(a.resize((56,56),Image.Resampling.NEAREST),(x+12,y+76))
        im.alpha_composite(a.resize((112,112),Image.Resampling.NEAREST),(x+124,y+44))
        draw.text((x+16,y+182),s['id'],font=small,fill=ink)
    im.save(OUT/f'round2_batch13_objects_{page}.png')
print(f'Rebuilt overview and {math.ceil(len(specs)/16)} detail pages for {len(specs)} objects.')
