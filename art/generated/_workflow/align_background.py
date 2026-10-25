"""Register selected native shop artwork to the fixed gameplay surfaces."""
from pathlib import Path
import argparse
import json
from PIL import Image

ROOT=Path(__file__).resolve().parents[3]
ap=argparse.ArgumentParser()
ap.add_argument("id")
ap.add_argument("--shelf",type=int,required=True)
ap.add_argument("--counter",type=int,required=True)
ap.add_argument("--shelf-left",type=int)
ap.add_argument("--shelf-right",type=int)
args=ap.parse_args()
manifest=json.loads((ROOT/"art/manifest.json").read_text("utf-8"))
spec=next(a for a in manifest["assets"] if a["id"]==args.id)
path=ROOT/spec["output"]
im=Image.open(path).convert("RGBA")
sx=[0,640] if args.shelf_left is None else [0,args.shelf_left,args.shelf_right,640]
dx=[0,640] if args.shelf_left is None else [0,140,410,640]
sy=[0,args.shelf,args.counter,360]
dy=[0,98,238,360]
mesh=[]
for j in range(len(sy)-1):
    for i in range(len(sx)-1):
        mesh.append(((dx[i],dy[j],dx[i+1],dy[j+1]),
            (sx[i],sy[j],sx[i],sy[j+1],sx[i+1],sy[j+1],sx[i+1],sy[j])))
im.transform(im.size,Image.Transform.MESH,mesh,Image.Resampling.NEAREST).save(path)
p=ROOT/"art/generated/_workflow/generations.json"
data=json.loads(p.read_text("utf-8"))
data[args.id]["registration"]={"source_shelf_y":args.shelf,"source_counter_y":args.counter,
    "target_shelf_y":98,"target_counter_y":238,"source_x":sx,"target_x":dx,"sampling":"nearest"}
p.write_text(json.dumps(data,indent=2)+"\n",encoding="utf-8")
