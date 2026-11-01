"""Review final lock faces at native and actual 2x size, without changing the delivered images."""
from pathlib import Path
from PIL import Image,ImageDraw
R=Path(__file__).resolve().parents[3]
out=Image.new('RGBA',(1060,570),(38,34,45,255));d=ImageDraw.Draw(out)
for i,name in enumerate(('rusty','guild','dwarven')):
    im=Image.open(R/f'art/generated/lockpick/face_{name}.png').convert('RGBA')
    x=i*350+10
    d.text((x,12),name+' | native + actual 2x',fill=(245,233,198))
    out.alpha_composite(im,(x+80,42))
    out.alpha_composite(im.resize((320,320),Image.Resampling.NEAREST),(x,225))
dest=R/'art/generated/_review/round2_batch15_locks.png';out.save(dest);print(dest)
