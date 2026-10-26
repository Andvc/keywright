"""Review assembled production assets at integer zoom levels."""
from pathlib import Path
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'art/generated/_review'
BG=(43,34,54)
manifest=json.loads((ROOT/'art/manifest.json').read_text('utf-8'))
specs={a['id']:a for a in manifest['assets']}

def asset(sid):
    return Image.open(ROOT/specs[sid]['output']).convert('RGBA')

def paste(canvas,im,xy,scale=1):
    if scale!=1:
        im=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST)
    canvas.paste(im,xy,im)

sheet=Image.new('RGB',(660,7*250),BG)
d=ImageDraw.Draw(sheet)
for row,skin in enumerate(('wood','green','blue','purple','gold','black','crown')):
    closed=Image.new('RGBA',(96,72))
    closed.alpha_composite(asset(f'chest.{skin}.lid'),(0,0))
    closed.alpha_composite(asset(f'chest.{skin}.body'),(0,42))
    d.text((12,row*250+8),skin,fill='white')
    paste(sheet,closed,(12,row*250+27),3)
    paste(sheet,asset(f'chest.{skin}.open'),(342,row*250+27),3)
sheet.save(OUT/'assembled_chests.png')

sheet=Image.new('RGB',(960,260),BG)
d=ImageDraw.Draw(sheet)
for col,lock in enumerate(('rusty','guild','dwarven')):
    closed=Image.new('RGBA',(96,72))
    for part,y in (('lid',0),('body',42)):
        closed.alpha_composite(asset(f'chest.wood.{part}'),(0,y))
        closed.alpha_composite(asset(f'chest.lock.{lock}.{part}'),(0,y))
    d.text((col*320+8,8),lock,fill='white')
    paste(sheet,closed,(col*320+8,30),3)
sheet.save(OUT/'assembled_locks.png')

sheet=Image.new('RGB',(510,5*125),BG)
d=ImageDraw.Draw(sheet)
for row,sid in enumerate(['story.key_whole']+[f'story.key_shard_{n}' for n in range(1,5)]):
    d.text((8,row*125+5),sid,fill='white')
    paste(sheet,asset(sid),(8,row*125+25),4)
    paste(sheet,asset(sid),(405,row*125+25))
sheet.save(OUT/'key_assembly.png')

sheet=Image.new('RGB',(520,820),BG)
d=ImageDraw.Draw(sheet)
for row,n in enumerate((64,48,32,16)):
    im=asset(f'brand.app_icon_{n}')
    d.text((8,row*205+5),f'App icon {n}x{n}',fill='white')
    paste(sheet,im,(8,row*205+25),max(1,176//n))
    paste(sheet,im,(220,row*205+25))
sheet.save(OUT/'app_icons_native.png')

sheet=Image.new('RGB',(1050,460),BG)
d=ImageDraw.Draw(sheet)
y=5
for sid in ('fx.explosion','fx.coin_spin','fx.sparkle','fx.dust'):
    im=asset(sid)
    scale=1 if im.height>32 else 4
    d.text((8,y),sid,fill='white')
    paste(sheet,im,(8,y+18),scale)
    y+=im.height*scale+32
sheet.save(OUT/'animation_strips.png')

print('Assembly review sheets written.')
