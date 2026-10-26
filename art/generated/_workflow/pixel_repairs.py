"""Small pixel corrections justified by native-size review."""
from pathlib import Path
import argparse
import json
from PIL import Image, ImageDraw
import finish_assets
import artlib
import pixelize

ROOT=Path(__file__).resolve().parents[3]

def fix_tongue(image):
    steel=[(56,61,72),(98,109,128),(147,162,182),(204,217,232)]
    px=image.load()
    for y in range(image.height):
        for x in range(image.width):
            r,g,b,a=px[x,y]
            if a and r>g*1.12 and r>b*1.12 and r>95:
                light=(r+g+b)/3
                c=steel[0 if light<70 else 1 if light<130 else 2 if light<200 else 3]
                px[x,y]=(*c,255)
    return image

def fix_sparkle():
    image=Image.new("RGBA",(36,9))
    d=ImageDraw.Draw(image)
    for n,length in enumerate((0,1,3,1)):
        x,y=n*9+4,4
        d.point((x,y),fill="#ffffff")
        for distance in range(1,length+1):
            c="#ece6f5" if distance<length else "#c9c4d4"
            for dx,dy in ((distance,0),(-distance,0),(0,distance),(0,-distance)):
                d.point((x+dx,y+dy),fill=c)
        if n==2:
            for dx in (-1,1):
                for dy in (-1,1):
                    d.point((x+dx,y+dy),fill="#ece6f5")
    return image

def fix_chain_seat(image,part):
    d=ImageDraw.Draw(image)
    box=(40,34,55,41) if part=="lid" else (38,0,57,13)
    d.rectangle(box,fill="#383d48")
    ends=[((39,32),(56,41)),((56,32),(39,41))] if part=="lid" else [((36,0),(60,13)),((60,0),(36,13))]
    for start,end in ends:
        d.line((*start,*end),fill="#1a1420",width=4)
        for i in range(8):
            t=i/7
            x=round(start[0]+t*(end[0]-start[0]))
            y=round(start[1]+t*(end[1]-start[1]))
            d.rectangle((x-1,y-1,x+1,y+1),fill="#626d80",outline="#1a1420")
            d.point((x-1,y-1),fill="#93a2b6")
    return image

def fix_app_key(image):
    d=ImageDraw.Draw(image)
    if image.width in (64,48):
        gold={
            (201,196,212):(251,237,164), (204,217,232):(251,237,164),
            (155,146,173):(242,193,78), (147,162,182):(242,193,78),
            (118,109,139):(160,101,0), (98,109,128):(160,101,0),
        }
        x0,y0,x1,y1=(26,32,39,54) if image.width==64 else (20,25,29,41)
        for y in range(y0,y1):
            for x in range(x0,x1):
                c=image.getpixel((x,y))
                if c[:3] in gold:
                    image.putpixel((x,y),(*gold[c[:3]],c[3]))
    elif image.width==16:
        d.line((8,7,8,9),fill="#f2c14e")
        for xy,c in [((8,9),"#fbeda4"),((7,10),"#f2c14e"),
                     ((9,10),"#a06500"),((8,11),"#f2c14e"),((8,10),"#1a1420")]:
            d.point(xy,fill=c)
    return image

def fix_errand(image):
    d=ImageDraw.Draw(image)
    for n,(x,y) in enumerate(((7,9),(8,13),(9,17))):
        d.rectangle((x,y,x+2,y+2),fill="#f7ebc6",outline="#6e3d22")
        if n!=1:
            d.line((x+1,y+1,x+3,y-1),fill="#4b2212")
    # A distinct pencil beside the notebook, readable at the final 28px size.
    d.line((25,9,23,20),fill="#1a1420",width=3)
    d.line((25,10,23,18),fill="#f2c14e")
    d.point((25,9),fill="#d98297")
    d.point((23,19),fill="#d2b688")
    return image

ap=argparse.ArgumentParser()
ap.add_argument("kind",choices=["tongue","sparkle","black_chest","app_key","errand"])
args=ap.parse_args()
manifest=artlib.load_manifest()
ids={"tongue":["curio.tongue"],"sparkle":["fx.sparkle"],"black_chest":["chest.black.lid","chest.black.body"],"app_key":["brand.app_icon_64","brand.app_icon_48","brand.app_icon_16"],"errand":["curio.errand"]}[args.kind]
notes={"tongue":"Changed skin-like tongue highlights to steel ramp so the entire charm reads as silver.",
       "sparkle":"Restored exact single-pixel, small-plus, four-pointed-star, small-plus motifs after reduction lost the smallest marks.",
       "black_chest":"Removed the inherited empty mounting-plate appearance and carried crossed iron chain links over the lock seats.",
       "app_key":"Recoloured the 64px and 48px keys to the gold ramp and hand-simplified the 16px key; preserve gold colours in the palette cap.",
       "errand":"Restored three distinct checkboxes, two checked, and a separate pencil after native-size reduction softened these details."}
p=ROOT/"art/generated/_workflow/generations.json"
data=json.loads(p.read_text("utf-8"))
for sid in ids:
    spec=artlib.asset(manifest,sid)
    path=ROOT/spec["output"]
    image=Image.open(path).convert("RGBA")
    if args.kind=="tongue":
        image=fix_tongue(image)
    elif args.kind=="sparkle":
        image=fix_sparkle()
    elif args.kind=="app_key":
        image=fix_app_key(image)
    elif args.kind=="errand":
        image=fix_errand(image)
    else:
        image=fix_chain_seat(image,spec["part"])
    image=finish_assets.cap_with_outline(image,spec)
    if spec.get("outline"):
        image=pixelize.enforce_outline(image,spec)
    image.save(path)
    data[sid]["pixel_repair"]=notes[args.kind]
p.write_text(json.dumps(data,indent=2)+"\n",encoding="utf-8")
