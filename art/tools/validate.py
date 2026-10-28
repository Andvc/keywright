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
import pixelize  # noqa: E402
import scene  # noqa: E402

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

    if spec.get("value"):
        lo, hi = spec["value"]
        lit = [artlib._luma(px[p]) for p in mask]
        mean = sum(lit) / max(1, len(lit))
        if not lo <= mean <= hi:
            errors.append(f"mean brightness {mean:.0f}, want {lo}-{hi} ({'too dark' if mean < lo else 'too light'} for its layer)")
    errors += layer_checks(spec, img, mask)

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
        if spec["category"] in ("item", "heartsong", "curio", "icon_upgrade", "icon_kind", "emblem", "figure", "prop"):
            cx = (box[0] + box[2]) / 2
            if abs(cx - (w - 1) / 2) > (4 if spec["category"] == "figure" else 1):
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


def layer_checks(spec, img, mask):
    """The shop scene's layers have to fit together (art/tools/scene.py): where each one's edges and lines fall."""
    errors = []
    cat, (w, h) = spec["category"], spec["size"]
    box = artlib.bbox(mask)
    if not box:
        return errors
    px = img.load()
    if cat == "figure":
        if box[3] != h - 1:
            errors.append(f"feet on row {box[3]}, want the bottom row ({h - 1})")
        tall = box[3] - box[1] + 1
        if abs(tall - spec["height"]) > 2:
            errors.append(f"{tall} pixels tall, want {spec['height']} (its height next to everyone else)")
    if cat == "shelf":
        top = pixelize._board_top(img)
        if abs(top - spec["board_row"]) > 1:
            errors.append(f"the board's top is on row {top}, want row {spec['board_row']} (things stand on it)")
    if cat == "frame":
        band = [x for x in range(w) for y in range(scene.HUD_ROWS) if (x, y) not in mask]
        if band:
            errors.append(f"the top band (rows 0-{scene.HUD_ROWS - 1}, under the top bar) has {len(band)} gaps: it runs right across")
        x0, x1, y0, y1 = scene.POST_WIDTH + 4, w - scene.POST_WIDTH - 5, scene.FRAME_DEPTH[1] + 2, scene.COUNTER_ROW - 1
        inside = [(x, y) for (x, y) in mask if x0 <= x <= x1 and y0 <= y <= y1]
        if inside:
            errors.append(f"{len(inside)} pixels in the view (x {x0}-{x1}, rows {y0}-{y1}): the middle stays open")
    if cat == "place":
        holes = [(x, y) for y in range(scene.COUNTER_ROW - 7, h) for x in range(w) if (x, y) not in mask]
        if holes:
            errors.append(f"{len(holes)} transparent pixels below row {scene.COUNTER_ROW - 7}: the ground or floor runs "
                          "under the counter")
    if cat == "sheet":
        middle = img.crop((12, 12, w - 12, h - 12))
        m = artlib.calm_metrics(middle)
        if m["edges"] > 0.02 or m["specks"] > 0.002:
            errors.append(f"the paper's middle isn't plain (edges {m['edges']:.3f}, specks {m['specks']:.3f}): the slip "
                          "is printed there")
    return errors


# ---- review pictures --------------------------------------------------------------------------------------------------

ZOOM = {"scene": 2, "store": 1, "item": 4, "heartsong": 4, "curio": 4, "icon_upgrade": 4, "icon_kind": 4, "emblem": 4,
        "story": 4, "ui": 8, "fx": 2, "lock_face": 2, "brand": 2, "sky": 2, "place": 2, "frame": 2, "shelf": 4,
        "counter": 2, "figure": 2, "sheet": 1, "prop": 4}


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


def _chest(skin, lock=None):
    """The chest as the game draws it (tools/custom_art.py, built into assets/ at 2x), back at its art size."""
    out = Image.new("RGBA", (48, 36), (0, 0, 0, 0))
    for part, y in (("lid", 0), ("body", 21)):
        for name in (f"{skin}_{part}", f"lock_{lock}_{part}" if lock else None):
            p = artlib.path(f"assets/sprites/chest/{name}.png") if name else None
            if p and os.path.exists(p):
                img = artlib.open_rgba(p)
                out.alpha_composite(img.resize((img.width // 2, img.height // 2), Image.NEAREST), (0, y))
    return out


def _unlit(lantern):
    """A lantern by day: its bright, warm glass darkened (the game does the same)."""
    out = lantern.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a and artlib._luma((r, g, b)) > 150 and r > b + 40:
                px[x, y] = (59, 49, 80, 255)
    return out


def mockups(images, manifest):
    """The counter at each shop tier, built from whatever's been delivered (placeholders for the rest, as
    art/tools/scene.py draws them): the four times of day in a grid, and midday at full size with what the counter
    screen puts over it."""
    made = []
    specs = {s["id"]: s for s in manifest["assets"]}
    skins, locks = ["wood", "green", "blue", "purple"], ["rusty", "guild", "dwarven", "guild"]
    sellers = {1: ["hob", "mags", "penn"], 2: ["wenna", "fennick", "ansel"], 3: ["pell", "corisande", "aldous"],
               4: ["silas", "oriel", "vane"]}
    things = [s["id"] for s in manifest["assets"] if s["category"] in ("item", "heartsong") and s["id"] in images]
    for tier, key in scene.TIERS.items():
        who = next((f"figure.{k}" for k in sellers[tier] if f"figure.{k}" in images), f"figure.{sellers[tier][0]}")
        height = specs[who]["height"] if who in specs else 124
        frames = {}
        for time in scene.TIMES:
            lantern = images.get(f"prop.lantern_{key}")
            if lantern is not None and not scene.TIMES[time]["lit"]:
                lantern = _unlit(lantern)
            layers = {"sky": images.get(f"sky.{time}"), "place": images.get(f"place.{key}"),
                      "frame": images.get(f"frame.{key}"), "shelf": images.get(f"shelf.{key}"),
                      "counter": images.get(f"counter.{key}"), "sheet": images.get(f"sheet.{key}"),
                      "figure": images.get(who), "lantern": lantern,
                      "prop_left": images.get("prop.scales") if tier >= 2 else None,
                      "prop_right": images.get("prop.witchlight") if tier >= 3 else None}
            frame = scene.compose(layers, tier, time, height, chest=_chest(skins[tier - 1], locks[tier - 1]))
            for i, tid in enumerate(things[tier * 3:tier * 3 + 4] if tier >= 2 else []):
                t = images[tid]
                b = artlib.bbox(artlib.opaque_mask(t))
                t = t.crop((b[0], b[1], b[2] + 1, b[3] + 1))
                t = t.resize((t.width * 2, t.height * 2), Image.NEAREST)
                frame.alpha_composite(t, (300 + 84 * i + 42 - t.width // 2, 196 - t.height))
            frames[time] = frame
        grid = Image.new("RGBA", (1280, 720), (0, 0, 0, 255))
        for i, time in enumerate(scene.TIMES):
            grid.alpha_composite(frames[time].resize((640, 360), Image.NEAREST), ((i % 2) * 640, (i // 2) * 360))
        path = os.path.join(REVIEW, f"counter_{key}_times.png")
        grid.save(path)
        made.append(path)
        ui = frames["midday"].copy()
        d = ImageDraw.Draw(ui, "RGBA")
        d.rectangle((0, 0, 1279, 55), fill=(13, 10, 19, 150))
        for bx, by, bw, bh in scene.UI.values():
            d.rectangle((bx, by, bx + bw, by + bh), outline=(255, 64, 88, 220), width=3)
        path = os.path.join(REVIEW, f"counter_{key}.png")
        ui.save(path)
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
        for path in mockups(delivered, manifest):
            print(f"mock-up: {os.path.relpath(path, artlib.ROOT)}")
        print(f"contact sheets: {os.path.relpath(REVIEW, artlib.ROOT)}/")

    sys.exit(1 if failed or (args.strict and p1_missing) else 0)


if __name__ == "__main__":
    main()
