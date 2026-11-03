"""Read-only structural checks for the physical 3px asset delivery."""
import hashlib
import json
from pathlib import Path
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'art/density3'


def main():
    catalog=json.loads((ART/'catalog.json').read_text(encoding='utf-8'))
    errors=[]
    checked=[]
    for key,row in catalog['assets'].items():
        path=ROOT/row['file']
        if not path.is_file():
            errors.append(f'{key}: missing file')
            continue
        if row['status']=='retained':
            continue
        image=Image.open(path).convert('RGBA')
        data=np.asarray(image)
        px,py=row['grid_phase']
        vertical=np.flatnonzero((np.arange(1,image.width)+px)%3!=0)+1
        horizontal=np.flatnonzero((np.arange(1,image.height)+py)%3!=0)+1
        if np.any(data[:,vertical]!=data[:,vertical-1]) or np.any(data[horizontal]!=data[horizontal-1]):
            errors.append(f'{key}: breaks exact3px grid')
        if hashlib.sha256(path.read_bytes()).hexdigest()!=row['output_sha256']:
            errors.append(f'{key}: stale output hash')
        if list(image.size)!=row['physical_size']:
            errors.append(f'{key}: wrong output size')
        if not set(np.unique(data[:,:,3])).issubset({0,255}):
            errors.append(f'{key}: partial alpha')
        if not np.any(data[:,:,3]):
            errors.append(f'{key}: empty output')
        checked.append(key)
    whole=Image.open(ART/'assets/story/key_whole.png').convert('RGBA')
    merged=Image.new('RGBA',whole.size)
    coverage=np.zeros((whole.height,whole.width),dtype=np.uint8)
    for n in range(1,5):
        shard=Image.open(ART/f'assets/story/key_shard_{n}.png').convert('RGBA')
        coverage+=(np.array(shard)[:,:,3]!=0)
        merged.alpha_composite(shard)
    if coverage.max()>1 or whole.tobytes()!=merged.tobytes():
        errors.append('Key shards overlap or fail exact reconstruction')
    report={'checked':len(checked),'retained':sum(r['status']=='retained' for r in catalog['assets'].values()),
            'errors':errors,'user_approval':catalog.get('user_review','pending')}
    (ART/'validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(report,ensure_ascii=False))
    raise SystemExit(bool(errors))


if __name__=='__main__':
    main()
