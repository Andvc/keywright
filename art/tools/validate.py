"""
Checks every delivered asset against its spec in art/manifest.json, writes art/generated/report.json, and (with
--sheets) contact sheets and in-game mock-ups under art/generated/_review/.

  python3 art/tools/validate.py                 # everything that's been delivered; missing ones are listed
  python3 art/tools/validate.py --only portrait # ids starting with this
  python3 art/tools/validate.py --sheets        # also the review pictures
  python3 art/tools/validate.py --strict        # missing P1 assets count as failures

Exit code 1 if anything delivered fails (or, with --strict, a P1 asset is missing). Needs only Pillow.
"""
import argparse
import json
import math
import os
import sys
from collections import Counter

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import artlib  # noqa: E402

REVIEW = artlib.path("art/generated/_review")


def check(spec, manifest, img, delivered):
    """A list of failures and a list of warnings for one asset."""
    errors, warnings = [], []
    w, h = spec["size"]
    if img.size != (w, h):
        errors.append(f"size {img.width}x{img.height}, want {w}x{h}")
        return errors, warnings
    alpha = img.getchannel("A")
    values = set(artlib.pixels(alpha))
    if not values <= {0, 255}:
        errors.append(f"alpha isn't binary ({len(values - {0, 255})} soft values)")
    opaque = spec["background"] == "opaque"
    mask = artlib.opaque_mask(img)
    if opaque and len(mask) != w * h:
        errors.append(f"{w * h - len(mask)} transparent pixels in an opaque asset")
    if not opaque and not mask:
        errors.append("empty")
        return errors, warnings

    px = img.load()
    allowed = artlib.allowed_colours(spec, manifest)
    used = Counter(px[x, y][:3] for (x, y) in mask)
    stray = [c for c in used if c not in allowed]
    if stray:
        errors.append(f"{len(stray)} colours not in the palette (e.g. {', '.join(artlib.rgb_hex(c) for c in stray[:4])})")
    cap = spec.get("max_colours")
    if cap and len(used) > cap:
        errors.append(f"{len(used)} colours, at most {cap}")
    # Round 2: quiet enough to look at for hours (art/ART_DIRECTION.md, "Quiet first").
    if spec.get("calm"):
        m = artlib.calm_metrics(img)
        words = {"specks": "lone specks", "edges": "edges between neighbours", "busy": "brightness step"}
        for k, limit in spec["calm"].items():
            if m[k] > limit:
                shown = f"{m[k]:.3f}" if k != "busy" else f"{m[k]:.1f}"
                errors.append(f"too busy: {words[k]} {shown}, at most {limit} (draw it simpler)")

    if not opaque:
        m = spec.get("margin", 0) or 0
        box = artlib.bbox(mask)
        if m and (box[0] < m or box[1] < m or box[2] > w - 1 - m or box[3] > h - 1 - m):
            errors.append(f"content {box} reaches into the {m}px margin")
        if spec.get("outline"):
            edges = artlib.silhouette_edges(mask, img.size, artlib.outline_borders(spec))
            bad = [p for p in edges if px[p][:3] not in {artlib.hex_rgb(c) for c in manifest["outline_colours_allowed"]}]
            if bad:
                errors.append(f"{len(bad)} silhouette pixels aren't the outline colour (e.g. at {bad[0]})")
        if spec.get("baseline") is not None and box[3] != spec["baseline"]:
            errors.append(f"lowest pixel on row {box[3]}, want the baseline {spec['baseline']}")
        if spec["category"] in ("item", "heartsong", "curio", "icon_upgrade", "icon_kind", "emblem"):
            cx = (box[0] + box[2]) / 2
            if abs(cx - (w - 1) / 2) > 1:
                errors.append(f"not centred left-to-right (content centre x {cx}, canvas {(w - 1) / 2})")
            if spec["category"] in ("curio", "icon_upgrade", "icon_kind", "emblem"):
                cy = (box[1] + box[3]) / 2
                if abs(cy - (h - 1) / 2) > 1:
                    errors.append(f"not centred top-to-bottom (content centre y {cy})")
        fill = (box[2] - box[0] + 1) / max(1, w - 2 * m)
        if spec["category"] in ("curio", "icon_upgrade", "icon_kind") and fill < 0.6:
            warnings.append(f"content is only {fill:.0%} of the width: small for an icon")

    cat = spec["category"]
    if cat == "lock_face":
        cx, cy = spec["geometry"]["centre"]
        r0, r1 = spec["geometry"]["track_radius"]
        dark = {artlib.hex_rgb(c) for c in ("#0b0810", "#1a1420", "#2b2236")}
        ring = [(x, y) for y in range(h) for x in range(w) if r0 + 1 <= math.hypot(x - cx, y - cy) <= r1 - 1]
        bad = [p for p in ring if p not in mask or px[p][:3] not in dark]
        if len(bad) > len(ring) * 0.02:
            errors.append(f"the ring track (radius {r0}-{r1}) must be plain dark night1-3 ({len(bad)} pixels aren't)")
    if cat == "fx":
        n, (fw, fh) = spec["frames"], spec["frame_size"]
        for i in range(n):
            if not any(fw * i <= x < fw * (i + 1) for (x, _) in mask) and i < n - 1:
                errors.append(f"frame {i + 1} is empty")
    if cat == "story" and spec["id"].startswith("story.key_shard_"):
        for other in ("story.key_shard_1", "story.key_shard_2", "story.key_shard_3", "story.key_shard_4"):
            if other != spec["id"] and other in delivered:
                overlap = mask & artlib.opaque_mask(delivered[other])
                if overlap and other > spec["id"]:
                    errors.append(f"overlaps {other} by {len(overlap)} pixels")
    if spec["id"] == "story.key_whole":
        shards = [delivered.get(f"story.key_shard_{n}") for n in range(1, 5)]
        if all(s is not None for s in shards):
            union = set().union(*(artlib.opaque_mask(s) for s in shards))
            if union != mask:
                errors.append("isn't exactly the union of the four shards")
    return errors, warnings


# ---- review pictures --------------------------------------------------------------------------------------------------

ZOOM = {"background": 2, "scene": 2, "store": 1, "portrait": 2, "item": 4, "heartsong": 4, "curio": 4, "icon_upgrade": 4, "icon_kind": 4, "emblem": 4, "story": 4, "ui": 8, "fx": 2, "lock_face": 2,
        "brand": 2}


def sheet(category, specs, images, status):
    from PIL import ImageFont
    try:
        font = ImageFont.truetype(artlib.path("assets/fonts/kenney_mini.ttf"), 16)
    except OSError:
        font = ImageFont.load_default()
    z = ZOOM.get(category, 2)
    items = [(s, images[s["id"]]) for s in specs if s["id"] in images]
    if not items:
        return None
    cw = max(i.width for _, i in items) * z + 24
    ch = max(i.height for _, i in items) * z + 44
    cols = max(1, min(len(items), 1800 // cw))
    rows = (len(items) + cols - 1) // cols
    out = Image.new("RGBA", (cols * cw, rows * ch), (33, 27, 45, 255))
    d = ImageDraw.Draw(out)
    for n, (s, img) in enumerate(items):
        x, y = (n % cols) * cw, (n // cols) * ch
        big = img.resize((img.width * z, img.height * z), Image.NEAREST)
        out.alpha_composite(big, (x + 12, y + 8))
        ok = status[s["id"]]["status"]
        colour = (111, 220, 140, 255) if ok == "ok" else (255, 107, 107, 255)
        d.text((x + 12, y + ch - 30), s["id"].split(".", 1)[1][:28], font=font, fill=colour)
    return out


# Where the counter screen covers a background (native coordinates; see art/reference/layout/counter_overlay.png).
COVERED = [(0, 0, 639, 27), (31, 54, 119, 142), (6, 167, 144, 240), (410, 33, 626, 353), (6, 245, 151, 300),
           (12, 310, 125, 345), (160, 310, 250, 345)]
CALM = [(158, 245, 400, 300), (220, 150, 340, 237)]


def layout(bg):
    """A background at the game's 1280x720 (2x a round-1 picture, 4x a round-2 one) with the counter screen's fixed lines
    and covered areas drawn over it."""
    frame = bg.resize((1280, 720), Image.NEAREST)
    d = ImageDraw.Draw(frame, "RGBA")
    for x0, y0, x1, y1 in COVERED:
        d.rectangle((x0 * 2, y0 * 2, x1 * 2 + 1, y1 * 2 + 1), fill=(255, 64, 88, 50), outline=(255, 64, 88, 255), width=2)
    for x0, y0, x1, y1 in CALM:
        d.rectangle((x0 * 2, y0 * 2, x1 * 2 + 1, y1 * 2 + 1), outline=(255, 181, 46, 255), width=2)
    d.line((0, 476, frame.width - 1, 476), fill=(78, 163, 255, 255), width=2)
    d.line((300, 196, 805, 196), fill=(78, 163, 255, 255), width=2)
    d.rectangle((460, 332, 652, 475), outline=(199, 146, 255, 255), width=2)
    return frame


def mockups(images, manifest):
    """The counter at each tier with whatever has been delivered: background, a portrait, the chest, things on the
    shelf, and the slip's panel, at the game's 1280x720."""
    made = []
    portraits = [s["id"] for s in manifest["assets"] if s["category"] == "portrait" and s["id"] in images]
    things = [s["id"] for s in manifest["assets"] if s["category"] in ("item", "heartsong") and s["id"] in images]
    skins, locks = ["wood", "green", "blue", "purple"], ["rusty", "guild", "dwarven", "guild"]
    for tier in range(1, 5):
        bg = images.get(f"bg.shop{tier}")
        if bg is None:
            continue
        frame = bg.resize((1280, 720), Image.NEAREST)
        d = ImageDraw.Draw(frame, "RGBA")
        d.rectangle((0, 0, 1279, 55), fill=(13, 10, 19, 235))
        d.rectangle((820, 66, 1252, 706), fill=(33, 27, 45, 245), outline=(59, 49, 80, 255), width=3)
        d.rectangle((62, 108, 238, 284), fill=(33, 27, 45, 255), outline=(59, 49, 80, 255), width=3)
        if portraits:
            p = images[portraits[(tier - 1) * 7 % len(portraits)]]
            frame.alpha_composite(p.resize((152, 152), Image.NEAREST), (74, 120))
        # The chest is drawn in code (tools/custom_art.py); the game's build of it, with a lock, at its 2x.
        for part, y in (("body", 416), ("lid", 332)):
            for name in (f"{skins[tier - 1]}_{part}", f"lock_{locks[tier - 1]}_{part}"):
                p = artlib.path(f"assets/sprites/chest/{name}.png")
                if os.path.exists(p):
                    img = artlib.open_rgba(p)
                    frame.alpha_composite(img.resize((img.width * 2, img.height * 2), Image.NEAREST), (460, y))
        for i, tid in enumerate(things[tier * 3:tier * 3 + 5]):
            t = images[tid]
            box = artlib.bbox(artlib.opaque_mask(t))
            t = t.crop((box[0], box[1], box[2] + 1, box[3] + 1))
            t = t.resize((t.width * 2, t.height * 2), Image.NEAREST)  # at 2x on the shelf, six slots from x 300
            frame.alpha_composite(t, (300 + 84 * i + 42 - t.width // 2, 196 - t.height))
        path = os.path.join(REVIEW, f"mockup_counter_tier{tier}.png")
        frame.save(path)
        made.append(path)
    return made


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--manifest")
    ap.add_argument("--only", default="")
    ap.add_argument("--sheets", action="store_true")
    ap.add_argument("--strict", action="store_true")
    ap.add_argument("--out", default="art/generated", help="where report.json and _review/ go")
    args = ap.parse_args()
    global REVIEW
    REVIEW = os.path.join(artlib.path(args.out), "_review")

    manifest = artlib.load_manifest(args.manifest)
    specs = [s for s in manifest["assets"] if s["id"].startswith(args.only)]
    delivered = {}
    for s in manifest["assets"]:
        p = artlib.path(s["output"])
        if os.path.exists(p):
            delivered[s["id"]] = artlib.open_rgba(p)

    report, failed, missing = {}, 0, []
    for s in specs:
        if s["id"] not in delivered:
            report[s["id"]] = {"status": "missing", "priority": s["priority"]}
            missing.append(s)
            continue
        errors, warnings = check(s, manifest, delivered[s["id"]], delivered)
        img = delivered[s["id"]]
        colours = len({c[:3] for c in artlib.pixels(img) if c[3]})
        report[s["id"]] = {"status": "fail" if errors else "ok", "errors": errors, "warnings": warnings,
                           "colours": colours, "priority": s["priority"]}
        if s.get("calm") and img.size == tuple(s["size"]):
            report[s["id"]]["calm"] = {k: round(v, 3) for k, v in artlib.calm_metrics(img).items()}
        failed += bool(errors)

    os.makedirs(artlib.path(args.out), exist_ok=True)
    summary = Counter(r["status"] for r in report.values())
    with open(os.path.join(artlib.path(args.out), "report.json"), "w") as f:
        json.dump({"summary": dict(summary), "assets": report}, f, indent=1)
        f.write("\n")

    for sid, r in report.items():
        if r["status"] == "fail":
            print(f"FAIL {sid}: " + "; ".join(r["errors"]))
        for wmsg in r.get("warnings", []):
            print(f"warn {sid}: {wmsg}")
    p1_missing = [s["id"] for s in missing if s["priority"] == "P1"]
    print(f"{summary.get('ok', 0)} ok, {summary.get('fail', 0)} failed, {summary.get('missing', 0)} missing "
          f"({len(p1_missing)} of them P1) -> {os.path.join(args.out, 'report.json')}")

    if args.sheets:
        os.makedirs(REVIEW, exist_ok=True)
        by_cat = {}
        for s in specs:
            by_cat.setdefault(s["category"], []).append(s)
        for cat, cs in by_cat.items():
            img = sheet(cat, cs, delivered, report)
            if img is not None:
                img.save(os.path.join(REVIEW, f"{cat}.png"))
        for s_ in specs:
            if s_["category"] == "background" and s_["id"] in delivered:
                layout(delivered[s_["id"]]).save(os.path.join(REVIEW, f"{s_['id'].replace('.', '_')}_layout.png"))
        for path in mockups(delivered, manifest):
            print(f"mock-up: {os.path.relpath(path, artlib.ROOT)}")
        print(f"contact sheets: {os.path.relpath(REVIEW, artlib.ROOT)}/")

    sys.exit(1 if failed or (args.strict and p1_missing) else 0)


if __name__ == "__main__":
    main()
