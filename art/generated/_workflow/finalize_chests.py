"""Finalize chest seams, shared masks and the exact body in each open chest."""
from pathlib import Path
import argparse
import json
import finish_assets
import artlib
from PIL import Image

ROOT=Path(__file__).resolve().parents[3]
ap=argparse.ArgumentParser()
ap.add_argument("--batch",type=int,required=True)
args=ap.parse_args()
manifest=artlib.load_manifest()
for spec in manifest["assets"]:
    if spec["category"]!="chest" or spec["batch"]!=args.batch:
        continue
    p=ROOT/spec["output"]
    img=Image.open(p).convert("RGBA")
    img=finish_assets.repair_chest(img,spec,manifest)
    img=finish_assets.cap_with_outline(img,spec)
    if spec["part"]=="open":
        body=Image.open(ROOT/f"art/generated/chest/{spec['skin']}_body.png").convert("RGBA")
        img.paste(body,(0,30))
        assert img.crop((0,30,96,60)).tobytes()==body.tobytes()
    img.save(p)
p=ROOT/"art/generated/_workflow/generations.json"
data=json.loads(p.read_text("utf-8"))
for spec in manifest["assets"]:
    if spec["category"]=="chest" and spec["batch"]==args.batch:
        data[spec["id"]]["geometry_repair"]="Wood alpha template, full-width seam rows, mounted lock seats; open chest reuses exact final body pixels."
p.write_text(json.dumps(data,indent=2)+"\n",encoding="utf-8")
