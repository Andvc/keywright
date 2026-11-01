"""Derive four disjoint shards from the approved whole key, preserving exact pixel colours and positions."""
import json,hashlib,sys
from pathlib import Path
from PIL import Image
REPO=Path(__file__).resolve().parents[3]
sys.path.insert(0,str(REPO/'art/tools'))
import artlib,validate
m=artlib.load_manifest()
whole_spec=artlib.asset(m,'story.key_whole');whole=artlib.open_rgba(REPO/whole_spec['output'])
# Modest jagged cuts, centred on the three specified break positions.
steps=[0,0,0,1,1,1,0,0,-1,-1,0,0,1,1,2,2,1,0,0,-1,-1,0,0,0]
pieces=[Image.new('RGBA',whole.size) for _ in range(4)]
for y in range(whole.height):
 # Keep the low tooth tip entirely on the fourth piece, avoiding a detached one-pixel island.
 cuts=[26+steps[y],50+steps[(y+8)%24],74+steps[(y+16)%24] if y<16 else 72]
 for x in range(whole.width):
  n=sum(x>=cut for cut in cuts)
  pieces[n].putpixel((x,y),whole.getpixel((x,y)))
delivered={'story.key_whole':whole,**{f'story.key_shard_{i+1}':im for i,im in enumerate(pieces)}}
merged=Image.new('RGBA',whole.size)
rows=[]
for i,im in enumerate(pieces,1):
 sid=f'story.key_shard_{i}';s=artlib.asset(m,sid);out=REPO/s['output'];out.parent.mkdir(parents=True,exist_ok=True);im.save(out)
 preview=REPO/'art/generated/_review/round2_batch14_assets'/f'story_key_shard_{i}_4x.png'
 preview.parent.mkdir(parents=True,exist_ok=True);im.resize((384,96),Image.Resampling.NEAREST).save(preview)
 errors,warnings=validate.check(s,m,im,delivered)
 assert not errors and not warnings,(sid,errors,warnings)
 rows.append({'id':sid,'candidate':'derived','output':s['output'],'output_sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'native_size':s['size'],'preview_4x':preview.relative_to(REPO).as_posix(),'errors':errors,'warnings':warnings,'calm':artlib.calm_metrics(im),'colours':len({c[:3] for c in im.getdata() if c[3]}),'selection_reason':'Exact disjoint jagged cut from story.key_whole; original colours and canvas positions retained, with no outlines at the breaks.','source':'story.key_whole','source_sha256':hashlib.sha256((REPO/whole_spec['output']).read_bytes()).hexdigest(),'command':['python3','art/generated/_workflow/round2_batch14_derive.py']})
 merged.alpha_composite(im)
assert merged.tobytes()==whole.tobytes()
errors,warnings=validate.check(whole_spec,m,whole,delivered);assert not errors and not warnings,(errors,warnings)
if len(sys.argv)>1:Path(sys.argv[1]).write_text(json.dumps(rows,indent=2),encoding='utf-8')
print('Four non-overlapping shards reconstruct the exact whole key, RGBA bytes included; no outline pixels added.')

