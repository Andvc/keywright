"""Render the batch 14 icons and exact key fragments at their actual UI size."""
import json,math,textwrap
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
REPO=Path(__file__).resolve().parents[3];OUT=REPO/'art/generated/_review'
m=json.loads((REPO/'art/manifest.json').read_text())
specs=[s for s in m['assets'] if (s['batch']==14 and s['category']!='story') or s['id'] in ('icon.upgrade.lamp','icon.kind.coins')]
bg=(38,34,45,255);ink=(245,233,198);font=ImageFont.load_default(size=15);small=ImageFont.load_default(size=12)
for page,start in enumerate(range(0,len(specs),18),1):
 group=specs[start:start+18];out=Image.new('RGBA',(1080,80+180*math.ceil(len(group)/6)),bg);d=ImageDraw.Draw(out)
 d.text((20,18),f'Batch 14 / page {page}: actual 2x UI size (left), 4x inspection (right)',font=font,fill=ink)
 for i,s in enumerate(group):
  x,y=i%6*180,i//6*180+80
  d.multiline_text((x+8,y),'\n'.join(textwrap.wrap(s['title'],22)),font=small,fill=ink)
  a=Image.open(REPO/s['output']).convert('RGBA')
  out.alpha_composite(a.resize((a.width*2,a.height*2),Image.Resampling.NEAREST),(x+12,y+72))
  out.alpha_composite(a.resize((a.width*4,a.height*4),Image.Resampling.NEAREST),(x+78,y+48))
  if s['batch']==9:d.text((x+8,y+155),'Approved anchor',font=small,fill=ink)
 out.save(OUT/f'round2_batch14_icons_{page}.png')
story=[s for s in m['assets'] if s['batch']==14 and s['category']=='story']
out=Image.new('RGBA',(1080,620),bg);d=ImageDraw.Draw(out)
d.text((20,18),'Aldric Key: 2x UI size (left), 4x inspection (right). Shards retain exact canvas position.',font=font,fill=ink)
for i,s in enumerate(story):
 y=70+i*106;a=Image.open(REPO/s['output']).convert('RGBA');d.text((20,y),s['id'],font=small,fill=ink)
 out.alpha_composite(a.resize((192,48),Image.Resampling.NEAREST),(220,y+10));out.alpha_composite(a.resize((384,96),Image.Resampling.NEAREST),(560,y))
out.save(OUT/'round2_batch14_key.png')
print(f'Reviewed layout ready: {len(specs)} icons including two anchors; whole key and four fragments.')

