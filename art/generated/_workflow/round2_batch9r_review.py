"""Additional C2 anchor review pictures; does not rewrite the historical style study.

Run from the repository: python3 art/generated/_workflow/round2_batch9r_review.py
The standard report/contact sheets still come from art/tools/validate.py --sheets.
"""
import sys
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "art/tools"))
import artlib
import study

review = ROOT / "art/generated/_review"
anchors = review / "round2_batch9r_anchors"
anchors.mkdir(parents=True, exist_ok=True)
manifest = artlib.load_manifest()
specs = {a["id"]: a for a in manifest["assets"]}

world = study.batch9_world()
world.save(review / "round2_batch9r_world.png")
study.mockup(world).save(review / "round2_batch9r_playview.png")
print("C2 layered world:", artlib.calm_metrics(world))

ids = ["prop.lantern_stall", "item.dwarf_gold_crowns", "heartsong.lullaby_box",
       "curio.monocle", "icon.upgrade.lamp", "icon.kind.coins"]
canvas = Image.new("RGBA", (1040, 790), (29, 23, 39, 255))
draw = ImageDraw.Draw(canvas)
font = study._font(18)
draw.text((24, 16), "C2 / 4X - BATCH 9R SMALL ANCHORS", font=font, fill="white")
draw.text((24, 44), "Native size at 1x, 2x and 4x. Lantern uses 4x in the world; inventory objects use 2x.",
          font=study._font(14), fill=(201, 196, 212, 255))
for i, sid in enumerate(ids):
    col, row = i % 2, i // 2
    x, y = 24 + col * 516, 90 + row * 226
    draw.text((x, y), sid, font=font, fill=(242, 193, 78, 255))
    im = artlib.open_rgba(str(ROOT / specs[sid]["output"]))
    for scale, at in ((1, 28), (2, 132), (4, 288)):
        pic = im.resize((im.width * scale, im.height * scale), Image.Resampling.NEAREST)
        canvas.alpha_composite(pic, (x + at, y + 42 + (128 - pic.height) // 2))
        draw.text((x + at, y + 180), f"{scale}x", font=font, fill="white")
canvas.save(review / "round2_batch9r_sprites.png")

for sid in manifest["anchors"]:
    spec = specs[sid]
    im = artlib.open_rgba(str(ROOT / spec["output"]))
    im.resize((im.width * 4, im.height * 4), Image.Resampling.NEAREST).save(
        anchors / (sid.replace(".", "_") + "_4x.png"))
