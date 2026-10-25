"""Post-process generated source art with the repository's supplied pixelizer.

Only writes inside art/generated. Raw images are retained outside the checkout.
"""
from pathlib import Path
from collections import Counter
import argparse
import hashlib
import json
import sys

from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "art/tools"))
import artlib
import pixelize


def cap_with_outline(img, spec):
    limit = spec.get("max_colours", 96)
    counts = Counter(c[:3] for c in img.getdata() if c[3])
    if len(counts) <= limit:
        return img
    required = [pixelize.OUTLINE] if spec.get("outline") else []
    keep = required + [c for c, _ in counts.most_common() if c not in required][:limit-len(required)]
    return pixelize.snap(img, keep)


def repair_chest(img, spec, manifest):
    # The seam is a shared full-width surface, not a transparent silhouette.
    w, h = img.size
    if spec["part"] in ("lid", "body"):
        rows = [h-1] if spec["part"] == "lid" else [0, h-1]
        for y in rows:
            solid = [x for x in range(w) if img.getpixel((x, y))[3]]
            if not solid:
                source_y = min((r for r in range(h) if any(img.getpixel((x, r))[3] for x in range(w))), key=lambda r: abs(r-y))
                img.paste(img.crop((0, source_y, w, source_y+1)), (0, y))
                solid = [x for x in range(w) if img.getpixel((x, y))[3]]
            if solid:
                for x in range(w):
                    if not img.getpixel((x, y))[3]:
                        nearest_x = min(solid, key=lambda sx: abs(sx-x))
                        img.putpixel((x, y), img.getpixel((nearest_x, y)))
        if spec["skin"] != "black":
            x0, y0, x1, y1 = spec["geometry"][spec["part"]]["lock_seat"]
            for y in range(y0, y1+1):
                for x in range(x0, x1+1):
                    if not img.getpixel((x, y))[3]:
                        img.putpixel((x, y), (43, 34, 54, 255))
    if spec["skin"] != "wood":
        ref = ROOT / f"art/generated/chest/wood_{spec['part']}.png"
        template = Image.open(ref).convert("RGBA")
        # Every skin keeps exactly the template silhouette.
        img.putalpha(template.getchannel("A"))
    if spec["part"] == "open":
        body = ROOT / f"art/generated/chest/{spec['skin']}_body.png"
        if body.exists():
            img.paste(Image.open(body).convert("RGBA"), (0, 30))
    return pixelize.enforce_outline(img, spec)


def align_shop1_source(img):
    """Register landmarks in the generated source before reduction to native pixels.

    The model placed the shelf at y90 and counter at y222 on a 640x360
    equivalent. Move these to the game's fixed rows, including the awning.
    The shelf is also registered to x140-410. This affects source composition;
    no finished sprite is rotated or resampled.
    """
    w, h = img.size
    sx = [0, 455 / 1672 * w, 1174 / 1672 * w, w]
    dx = [0, 140 / 640 * w, 410 / 640 * w, w]
    sy = [0, 93 / 941 * h, 235 / 941 * h, 580 / 941 * h, h]
    dy = [0, 44 / 360 * h, 98 / 360 * h, 238 / 360 * h, h]
    mesh = []
    for j in range(len(sy)-1):
        for i in range(len(sx)-1):
            box = tuple(round(v) for v in (dx[i], dy[j], dx[i+1], dy[j+1]))
            quad = (sx[i], sy[j], sx[i], sy[j+1], sx[i+1], sy[j+1], sx[i+1], sy[j])
            mesh.append((box, quad))
    return img.transform((w, h), Image.Transform.MESH, mesh, Image.Resampling.NEAREST)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--id", required=True)
    ap.add_argument("--raw", required=True)
    ap.add_argument("--mode", choices=["grid", "area"])
    ap.add_argument("--block", type=float)
    ap.add_argument("--note", default="")
    args = ap.parse_args()
    manifest = artlib.load_manifest()
    spec = artlib.asset(manifest, args.id)
    raw_path = Path(args.raw)
    img = Image.open(raw_path).convert("RGBA")
    if args.id == "bg.shop1":
        img = align_shop1_source(img)
    if spec["category"] == "chest":
        # Register the generated part to its exact part envelope before sampling.
        # The image model supplies a decorative object, not an engine-ready canvas.
        bounds = img.getchannel("A").point(lambda a: 255 if a >= 128 else 0).getbbox()
        if bounds:
            img = img.crop(bounds).resize((spec["size"][0]*16, spec["size"][1]*16), Image.Resampling.NEAREST)
    process = pixelize.process_sheet if spec.get("frames") else pixelize.process
    result = process(img, spec, manifest, mode=args.mode, block=args.block)
    if spec["category"] == "chest":
        result = repair_chest(result, spec, manifest)
    result = cap_with_outline(result, spec)
    if spec.get("outline"):
        result = pixelize.enforce_outline(result, spec)
    path = ROOT / spec["output"]
    path.parent.mkdir(parents=True, exist_ok=True)
    result.save(path)
    preview = ROOT / "art/generated/_review/anchors" / (args.id + "_4x.png")
    if spec["batch"] == 0:
        preview.parent.mkdir(parents=True, exist_ok=True)
        result.resize((result.width*4, result.height*4), Image.Resampling.NEAREST).save(preview)
    rec_path = ROOT / "art/generated/_workflow/generations.json"
    records = json.loads(rec_path.read_text("utf-8")) if rec_path.exists() else {}
    records[args.id] = {
        "tool": "built-in image_gen",
        "requested_model": "gpt-image-2",
        "prompt_source": "art/manifest.json: " + args.id,
        "raw_sha256": hashlib.sha256(raw_path.read_bytes()).hexdigest(),
        "output": spec["output"],
        "mode": args.mode or spec["mode"],
        "note": args.note,
    }
    rec_path.write_text(json.dumps(records, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")
    print(args.id + " -> " + spec["output"])


if __name__ == "__main__":
    main()
