"""Batch-10 review composites; production art and historical studies are read-only.

Run from the repository: python3 art/generated/_workflow/round2_batch10_review.py
The standard contact sheets and report still come from validate.py --sheets.
"""
import json
import sys
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "art/tools"))
import artlib
import scene
import study
import validate

OUT = ROOT / "art/generated/_review"
OUT.mkdir(parents=True, exist_ok=True)
manifest = artlib.load_manifest()
specs = {s["id"]: s for s in manifest["assets"]}
BG = (29, 23, 39, 255)
FG = (236, 230, 245, 255)
GOLD = (242, 193, 78, 255)
font = study._font(18)
small = study._font(14)

def asset(sid):
    return artlib.open_rgba(str(ROOT / specs[sid]["output"]))

def label(draw, xy, text, colour=FG, face=font):
    draw.text(xy, text, fill=colour, font=face)

overview = Image.new("RGBA", (1320, 876), BG)
d = ImageDraw.Draw(overview)
label(d, (20, 12), "C2 / 4X / NO OUTLINES - BATCH 10", GOLD)
label(d, (20, 40), "Four shops at midday. Same approved Hob for comparison; not game captures.", face=small)
metrics = {}
for tier, key in scene.TIERS.items():
    layers = {name: asset(f"{name}.{key}") for name in ("place", "frame", "shelf", "counter", "sheet")}
    layers.update(sky=asset("sky.midday"), figure=asset("figure.hob"),
                  lantern=validate._unlit(asset(f"prop.lantern_{key}")),
                  prop_left=asset("prop.scales" if tier >= 2 else "prop.gavel"),
                  prop_right=asset("prop.witchlight" if tier >= 3 else "prop.lodestone"))
    frame = scene.compose(layers, tier, "midday", specs["figure.hob"]["height"],
                          chest=validate._chest("wood", "rusty"))
    # Only approved C2 items are staged here. Existing round-1 items appear only in standard mock-ups.
    for x, sid in zip((360, 500, 640), ("item.dwarf_gold_crowns", "heartsong.lullaby_box", "curio.monocle")):
        im = asset(sid)
        box = im.getbbox()
        im = im.crop(box)
        im = im.resize((im.width * 2, im.height * 2), Image.Resampling.NEAREST)
        frame.alpha_composite(im, (x - im.width // 2, scene.SHELF_ROW * 4 - im.height))
    for name, at in (("bubble", (scene.UI["bubble"][0], scene.UI["bubble"][1])), ("hud", (0, 0))):
        p = Path(study.UI) / f"{name}.png"
        if p.exists():
            frame.alpha_composite(artlib.open_rgba(str(p)), at)
    frame.save(OUT / f"round2_batch10_{key}_playview.png")
    col, row = (tier - 1) % 2, (tier - 1) // 2
    x, y = 10 + col * 660, 76 + row * 398
    label(d, (x + 10, y), key.upper(), GOLD)
    overview.alpha_composite(frame.resize((640, 360), Image.Resampling.NEAREST), (x, y + 26))
    # Whole-world comparison excludes the separate 2x writing sheet, UI and code chest.
    world = Image.new("RGBA", (320, 180), (0, 0, 0, 255))
    for name in ("sky", "place"):
        world.alpha_composite(layers[name])
    height = specs["figure.hob"]["height"]
    lift = max(0, scene.FIGURE_SHOWS - (height - (scene.FIGURE_FEET - scene.COUNTER_ROW)))
    world.alpha_composite(layers["figure"], (scene.FIGURE_AT[0], scene.FIGURE_AT[1] - lift))
    world.alpha_composite(layers["shelf"], scene.SHELF_AT)
    world.alpha_composite(layers["frame"])
    for lx, ly in scene.LANTERNS:
        world.alpha_composite(layers["lantern"], (lx - layers["lantern"].width // 2, ly))
    world.alpha_composite(layers["counter"], (0, scene.COUNTER_ROW))
    metrics[key] = artlib.calm_metrics(world)
overview.save(OUT / "round2_batch10_shops.png")
(OUT / "round2_batch10_world_metrics.json").write_text(json.dumps(metrics, indent=2) + "\n", encoding="utf-8")

props = [s["id"] for s in manifest["assets"] if s["category"] == "prop" and s["batch"] in (9, 10)]
canvas = Image.new("RGBA", (1000, 780), BG)
d = ImageDraw.Draw(canvas)
label(d, (20, 12), "COUNTER PROPS - NATIVE AND 4X", GOLD)
label(d, (20, 42), "The stall lantern is the approved batch-9s reference. Every other prop is batch 10.", face=small)
for i, sid in enumerate(props):
    x, y = 20 + (i % 3) * 328, 86 + (i // 3) * 170
    label(d, (x, y), sid, face=small)
    im = asset(sid)
    canvas.alpha_composite(im, (x + 16, y + 58))
    canvas.alpha_composite(im.resize((im.width * 4, im.height * 4), Image.Resampling.NEAREST), (x + 112, y + 32))
    label(d, (x + 12, y + 135), "1x", face=small)
    label(d, (x + 120, y + 135), "4x", face=small)
canvas.save(OUT / "round2_batch10_props.png")

canvas = Image.new("RGBA", (1320, 868), BG)
d = ImageDraw.Draw(canvas)
label(d, (20, 12), "STORY SCENES - BATCH 10", GOLD)
label(d, (20, 42), "320x180 native images shown at 2x; individual 4x previews are in round2_batch10_assets/.", face=small)
for i, sid in enumerate(("scene.guild", "scene.bram", "scene.keyhold", "scene.vault")):
    x, y = 10 + (i % 2) * 660, 76 + (i // 2) * 392
    label(d, (x + 10, y), sid, GOLD)
    canvas.alpha_composite(asset(sid).resize((640, 360), Image.Resampling.NEAREST), (x, y + 24))
canvas.save(OUT / "round2_batch10_stories.png")
print(json.dumps({"world_comparisons": metrics, "props": len(props)}, indent=2))
