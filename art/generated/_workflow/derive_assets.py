"""Build exact UI recipes and assets derived from approved generated artwork."""
from pathlib import Path
import argparse
import hashlib
import json
import sys
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "art/tools"))
import artlib
import pixelize

def record(spec, image, source):
    dest = ROOT / spec["output"]
    dest.parent.mkdir(parents=True, exist_ok=True)
    image.save(dest)
    p = ROOT / "art/generated/_workflow/generations.json"
    data = json.loads(p.read_text("utf-8"))
    data[spec["id"]] = {"tool": "Pillow", "mode": spec["mode"],
        "source": source, "output": spec["output"],
        "sha256": hashlib.sha256(dest.read_bytes()).hexdigest()}
    p.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")

def ui(spec):
    image = Image.new("RGBA", tuple(spec["size"]))
    d = ImageDraw.Draw(image)
    sid = spec["id"]
    if sid in ("ui.panel", "ui.button", "ui.button_pressed"):
        panel = sid == "ui.panel"
        pressed = sid == "ui.button_pressed"
        border = "#0b0810" if panel else "#020202"
        top = "#3b3150" if panel else ("#828282" if pressed else "#ebebeb")
        bottom = "#1a1420" if panel else ("#ebebeb" if pressed else "#828282")
        fill = "#2b2236" if panel else ("#828282" if pressed else "#bebebe")
        d.rectangle((0,0,11,11), fill=border)
        d.rectangle((1,1,10,10), fill=bottom)
        d.line((1,10,1,1,10,1), fill=top)
        d.rectangle((2,2,9,9), fill=fill)
        for p in ((0,0),(11,0),(0,11),(11,11)):
            image.putpixel(p, (0,0,0,0))
        if panel:
            for p in ((2,2),(9,2),(2,9),(9,9)):
                d.point(p, fill="#c29026")
        else:
            d.point((2,2), fill="#ffffff")
    elif sid == "ui.frame":
        d.rectangle((1,1,10,10), outline="#ffffff")
        for p in ((1,1),(10,1),(1,10),(10,10)):
            image.putpixel(p, (0,0,0,0))
    elif sid == "ui.slip_paper":
        d.rectangle((0,0,23,23), fill="#4b2212")
        d.rectangle((1,1,22,22), fill="#f7ebc6")
        d.rectangle((1,21,22,22), fill="#9d8872")
        for x,y in ((0,0),(1,0),(0,1),(23,0),(23,1),(0,22),(0,23),(23,23),(22,23)):
            image.putpixel((x,y), (0,0,0,0))
        for x,y in ((3,2),(8,1),(17,2),(21,5),(2,8),(1,17),(21,18),(4,20),(18,20)):
            d.line((x,y,min(x+1,22),y),fill="#d2b688")
    elif sid == "ui.bubble":
        d.rounded_rectangle((0,0,23,23), radius=2, fill="#ffffff", outline="#1a1420")
    elif sid == "ui.bubble_tail":
        d.polygon(((5,0),(0,7),(11,7),(6,0)), fill="#ffffff", outline="#1a1420")
        d.line((5,0,6,0), fill="#ffffff")
    return image

def key(manifest):
    whole_spec = artlib.asset(manifest, "story.key_whole")
    original = Image.open(ROOT / whole_spec["output"]).convert("RGBA")
    shards = [Image.new("RGBA", original.size) for _ in range(4)]
    zigzag = [0,0,1,1,0,-1,-1,0,1,2,1,0,-1,0,1,1,0,-1,0,1,0,0,-1,0]
    for y in range(original.height):
        cuts = [26+zigzag[y],50+zigzag[(y+7)%24],74+zigzag[(y+13)%24]]
        for x in range(original.width):
            n = sum(x >= b for b in cuts)
            shards[n].putpixel((x,y), original.getpixel((x,y)))
    composed = Image.new("RGBA", original.size)
    for n, shard in enumerate(shards,1):
        spec = artlib.asset(manifest, f"story.key_shard_{n}")
        shard = pixelize.enforce_outline(shard,spec)
        record(spec,shard,"story.key_whole; disjoint jagged cuts near x26, x50 and x74; break edges outlined.")
        composed.alpha_composite(shard)
    # Recombine the break outlines so the whole and the four pieces match exactly.
    composed.save(ROOT / whole_spec["output"])
    p=ROOT / "art/generated/_workflow/generations.json"
    data=json.loads(p.read_text("utf-8"))
    data["story.key_whole"]["postprocess"]="Recombined outlined shards; exact disjoint RGBA union."
    p.write_text(json.dumps(data,indent=2)+"\n",encoding="utf-8")

def stores(manifest):
    logo=Image.open(ROOT / "art/generated/brand/logo.png").convert("RGBA")
    for spec in manifest["assets"]:
        if spec["category"] != "store":
            continue
        if spec["mode"] == "derive":
            native=Image.new("RGBA",tuple(spec["size"]))
            large=logo.resize((logo.width*2,logo.height*2),Image.Resampling.NEAREST)
            native.alpha_composite(large,((native.width-large.width)//2,(native.height-large.height)//2))
            record(spec,native,"brand.logo at integer 2x, centred on transparent native canvas.")
        else:
            native=Image.open(ROOT / spec["output"]).convert("RGBA")
        size=tuple(spec["deliver_size"])
        final=native.resize(size,Image.Resampling.NEAREST)
        if spec.get("needs_logo") and spec["mode"] != "derive":
            # Use the original logo at an integer size to retain sharp letter pixels.
            usable_w=int(final.width*0.88)
            factor=max(1,usable_w//logo.width)
            if spec["id"]=="store.main_capsule":
                factor=2
            placed=logo.resize((logo.width*factor,logo.height*factor),Image.Resampling.NEAREST)
            x=(final.width-placed.width)//2
            if spec["id"]=="store.small_capsule":
                y=(final.height-placed.height)//2
            elif spec["id"]=="store.library_capsule":
                y=int(final.height*0.73)
            elif spec["id"]=="store.main_capsule":
                x,y=48,36
            else:
                y=int(final.height*0.07)
            final.alpha_composite(placed,(x,y))
        output=ROOT / "art/generated/store/final" / Path(spec["output"]).name
        output.parent.mkdir(parents=True,exist_ok=True)
        final.save(output)

ap=argparse.ArgumentParser()
ap.add_argument("kind",choices=["ui","key","store"])
ap.add_argument("--batch",type=int)
args=ap.parse_args()
manifest=artlib.load_manifest()
if args.kind=="ui":
    for spec in manifest["assets"]:
        if spec["mode"]=="script" and (args.batch is None or spec["batch"]==args.batch):
            record(spec,ui(spec),"Exact manifest UI recipe.")
elif args.kind=="key":
    key(manifest)
else:
    stores(manifest)
