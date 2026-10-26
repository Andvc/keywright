"""Check cross-asset relationships not covered by isolated PNG validation."""
from pathlib import Path
import json
from PIL import Image

ROOT=Path(__file__).resolve().parents[3]
m=json.loads((ROOT/'art/manifest.json').read_text('utf-8'))
specs={a['id']:a for a in m['assets']}
checks=[]

def asset(sid):
    return Image.open(ROOT/specs[sid]['output']).convert('RGBA')

for skin in ('wood','green','blue','purple','gold','black','crown'):
    assert asset(f'chest.{skin}.open').crop((0,30,96,60)).tobytes()==asset(f'chest.{skin}.body').tobytes(),skin
    if skin!='wood':
        for part in ('lid','body','open'):
            assert asset(f'chest.{skin}.{part}').getchannel('A').tobytes()==asset(f'chest.wood.{part}').getchannel('A').tobytes(),(skin,part)
checks.append('All seven chest assemblies reuse exact body pixels and matching alpha silhouettes.')

whole=asset('story.key_whole')
merged=Image.new('RGBA',whole.size)
occupied=set()
for n in range(1,5):
    shard=asset(f'story.key_shard_{n}')
    points={(x,y) for y in range(shard.height) for x in range(shard.width) if shard.getpixel((x,y))[3]}
    assert points and not (points & occupied),n
    occupied|=points
    merged.alpha_composite(shard)
assert merged.tobytes()==whole.tobytes()
checks.append('Four nonempty key shards are disjoint and reconstruct the whole key exactly.')

for sid in ('fx.coin_spin','fx.sparkle','fx.dust','fx.explosion'):
    spec=specs[sid]
    im=asset(sid)
    fw,fh=spec['frame_size']
    for n in range(spec['frames']):
        assert im.crop((n*fw,0,(n+1)*fw,fh)).getbbox(),(sid,n)
checks.append('Every animation frame has visible content in its equal-size frame cell.')

for a in m['assets']:
    if a['category']=='store':
        p=ROOT/'art/generated/store/final'/Path(a['output']).name
        with Image.open(p) as im:
            assert im.size==tuple(a['deliver_size']),a['id']
checks.append('All nine Steam exports match their required delivery dimensions.')
for n in (64,48,32,16):
    colours={p[:3] for p in asset(f'brand.app_icon_{n}').getdata() if p[3]}
    assert colours & {(160,101,0),(194,144,38),(242,193,78),(251,237,164)},n
checks.append('Each independently drawn app icon retains gold key colours.')

result={'status':'pass','checks':checks}
(ROOT/'art/generated/_workflow/assembly_checks.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
print(json.dumps(result))
