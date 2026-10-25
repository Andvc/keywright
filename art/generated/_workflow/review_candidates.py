"""Create paired native-size and enlarged candidate review sheets."""
from pathlib import Path
import argparse
import json

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[3]
ap = argparse.ArgumentParser()
ap.add_argument("--batch", type=int, required=True)
ap.add_argument("--start", type=int, default=0)
ap.add_argument("--count", type=int, default=10)
args = ap.parse_args()
manifest = json.loads((ROOT / "art/manifest.json").read_text("utf-8"))
specs = [s for s in manifest["assets"] if s["batch"] == args.batch][args.start:args.start+args.count]
font = ImageFont.load_default()
zoom = 3 if all(s["category"] == "portrait" for s in specs) else 4
max_w = max(s["size"][0] for s in specs)
max_h = max(s["size"][1] for s in specs)
cell_w = max(230, max_w * zoom + 100)
cell_h = max_h * zoom + 40
sheet = Image.new("RGB", (cell_w*2, cell_h*len(specs)), (33, 27, 45))
draw = ImageDraw.Draw(sheet)
for row, spec in enumerate(specs):
    for c in (1, 2):
        x, y = (c-1)*cell_w, row*cell_h
        path = ROOT / f"art/generated/_review/candidates/{spec['id']}.c{c}.png"
        draw.text((x+8, y+8), f"{spec['id']} - candidate {c}", fill="white", font=font)
        if not path.exists():
            continue
        with Image.open(path) as src:
            src = src.convert("RGBA")
            big = src.resize((src.width*zoom, src.height*zoom), Image.Resampling.NEAREST)
            sheet.paste(big, (x+8,y+30), big)
            sheet.paste(src, (x+16+big.width,y+30), src)
            if spec["category"] == "portrait":
                face = src.crop((19,14,57,52)).resize((76,76), Image.Resampling.NEAREST)
                sheet.paste(face, (x+16+big.width,y+115), face)
out = ROOT / f"art/generated/_review/candidates_batch{args.batch}_{args.start:03d}.png"
out.parent.mkdir(parents=True, exist_ok=True)
sheet.save(out)
print(out.relative_to(ROOT).as_posix())
