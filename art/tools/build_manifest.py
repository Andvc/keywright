"""
Builds art/manifest.json and art/INVENTORY.md: every picture Keywright needs, with its spec and an assembled prompt.

The asset descriptions live here (art/tools/assets_*.py hold the long lists); ART_DIRECTION.md is the style they share.
Run: python3 art/tools/build_manifest.py
"""
import json
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ART = os.path.dirname(HERE)
ROOT = os.path.dirname(ART)
sys.path.insert(0, HERE)

from make_palette import build as build_palette  # noqa: E402
import assets_cast  # noqa: E402
import assets_things  # noqa: E402
import assets_curios  # noqa: E402
import assets_world  # noqa: E402

VERSION = 2

# Round 2 (0.25): quiet art. Round 1's preamble asked for clean pixels but its descriptions asked for detail, its grids
# were fine and its backgrounds were shrunk from paintings; the art came back two to six times as busy as the game's
# old code-drawn art. This one asks, first of all, for art that's easy to look at for hours.
STYLE = (
    "Pixel art game asset for Keywright, a cosy fantasy merchant game the player looks at for hours, so the art must be "
    "quiet and comfortable to look at: large, simple shapes of flat colour; a few clear forms; generous plain areas; "
    "detail only where it tells you what the thing is. Every art pixel is one solid, perfectly square block of a single "
    "flat colour on a strict grid. No texture of any kind (no wood grain, no stone speckle, no fabric weave, no fur "
    "strands, no scratches, no noise), no dithering, no gradients, no anti-aliasing, no painterly brushwork, no "
    "sparkles or glints scattered over surfaces. Shade each material with two or three flat tones in broad bands, and "
    "at most a highlight or two per object. Soft, muted, warm colour with hue-shifted shadows (toward violet) and "
    "highlights (toward warm yellow); never neon, never every colour at once. One gentle light from the upper left. A "
    "clear silhouette. No text, letters, numbers or rune-like writing."
)

# The counter screen's fixed lines, in the 640x360 the game shows a background at (2x); a round-2 background is drawn
# at half that (320x180, four screen pixels to an art pixel) and the importer doubles it.
COUNTER_ROW, SHELF_ROW, SHELF_X, CALM_BOX = 238, 98, (150, 402), (220, 150, 340, 237)

# What makes a picture quiet enough, measured at its native size by art/tools/artlib.py's calm_metrics: specks (lone
# pixels unlike all four neighbours, as a share of the inside), edges (neighbouring pairs that differ) and busy (the
# mean brightness step between neighbours, 0-255). Set between the game's old code-drawn art, which passes, and round
# 1's, which doesn't (a third of the pixels in a typical round-1 thing were lone specks).
CALM = {
    "scenery": dict(specks=0.015, edges=0.18, busy=7.5),
    "portrait": dict(specks=0.06, edges=0.32, busy=19),
    "sprite": dict(specks=0.15, edges=0.58),
    "icon": dict(specks=0.15, edges=0.6),
    "small_icon": dict(specks=0.15, edges=0.62),
    "lock_face": dict(specks=0.04, edges=0.22, busy=10),
}

QUIET_AVOID = ["texture or noise of any kind (grain, speckle, weave, strands, scratches)", "fine detail or tiny ornament",
               "highlights or sparkles scattered over surfaces", "many colours at once"]

# What every category shares: native size, background, colour cap, outline, placement, the prompt's frame, how quiet it
# must measure (calm), and whether round 2 redraws it (redo, in the given batch; batch 9 is round 2's anchors).
CATEGORIES = {
    "background": dict(
        folder="backgrounds", size=(320, 180), background="opaque", max_colours=24, outline=False, margin=0,
        batch=10, priority="P1", gen_size="1536x1024", mode="grid", redo=True, calm=CALM["scenery"],
        display="behind the counter at 4x (the importer doubles it to 640x360, shown at 2x); dimmed to 30-60% behind "
                "every other screen",
        frame=("A {w}x{h} pixel art background for the shop screen of a 2D game (16:9), drawn so each art pixel is a "
               "crisp square block about {k} px wide: the picture is the {w}x{h} grid enlarged to fill the image's width "
               "(it will be cropped to 16:9). Front elevation: a flat stage seen straight on, no vanishing-point "
               "perspective, no people. It stays behind the game for hours, so it must be quiet: three to five large "
               "planes, each nearly one flat colour, muted and fairly dark, with low contrast between them; one soft, "
               "brighter area (the sky or a window) and nothing else that catches the eye. The counter's top is one "
               "straight horizontal edge across the whole width at row {counter_row} of {h}; below it the counter's front "
               "is plain, flat and dark (text sits on it). A display shelf's top surface is one straight line at row "
               "{shelf_row} (from x {shelf_x0} to x {shelf_x1}). Just above the counter in the middle (x {calm_x0}-"
               "{calm_x1}, rows {calm_y0}-{calm_y1}) is the quietest part of the picture, one or two flat mid-dark colours, "
               "because the chest stands there. Full-bleed, opaque, no border, no frame."),
        avoid=["people or animals in the foreground", "text or signs with letters", "objects standing on the counter top",
               "a vanishing-point floor", "photographic detail", "a bright, saturated sunset", "busy foliage or many "
               "small clouds", "strong contrast anywhere but the one focal light"] + QUIET_AVOID,
    ),
    "portrait": dict(
        folder="portraits", size=(76, 76), background="opaque", max_colours=20, outline=False, margin=0,
        batch=13, priority="P1", gen_size="1024x1024", mode="grid", redo=True, calm=CALM["portrait"],
        display="2x at the counter (152 px in a 176 px frame), 3x in story scenes, 1x in the ledger and Bram's tips; "
                "the 38x38 face crop (x 19-56, y 14-51) at 1x in the waiting queue and ledger rows",
        frame=("A {w}x{h} pixel art character portrait (a head-and-shoulders bust) for a game's dialogue box, drawn so each "
               "art pixel is a crisp square block about {k} px wide (the whole image is the {w}x{h} grid enlarged). "
               "{view} The head is centred left-to-right with the eyes about 45% of the way down; the top of the head is "
               "about a fifth of the way down; the shoulders are cut by the bottom edge. Simple and easy to read: the face "
               "in three flat skin tones with clear eyes and mouth; hair as two or three flat masses, no strands; clothes "
               "in flat colours with a fold or two; one or two signature details, drawn large. The figure has a 1-pixel "
               "very dark plum outline where it meets the background. Background: {backdrop}, one flat colour, a little "
               "darker toward the corners in two steps, nothing else in it. Full-bleed, opaque, no frame, no border, no "
               "text."),
        avoid=["hands or props entering from outside the frame", "a frame or border", "a busy background",
               "a face outside the centre", "photorealism", "glowing auras", "strands of hair",
               "small accessories crowding the figure"] + QUIET_AVOID,
    ),
    "item": dict(
        folder="items", size=(32, 32), background="transparent", max_colours=12, outline=True, margin=1, baseline=30,
        batch=11, priority="P1", gen_size="1024x1024", mode="grid", redo=True, calm=CALM["sprite"],
        display="2x on the opening table (64 px); 1x or 2x in slots (collection, exhibition, shelf, pawn list); the game "
                "trims to the content",
        frame=("{title}: a {w}x{h} pixel art game item sprite, drawn so each art pixel is a crisp square block "
               "{k} px wide (the whole image is the {w}x{h} grid enlarged). Three-quarter view from slightly above, as if "
               "resting on a shop counter; upright and centred left-to-right, its lowest pixels on the bottom of the "
               "frame, filling about 90% of the frame. A simple, bold shape: each material in two or three flat tones, "
               "at most one small accent, no engraving or pattern. 1-pixel very dark plum outline around the whole "
               "silhouette. Transparent background, no shadow, no glow, no frame, nothing else in the image."),
        avoid=["a background or floor", "a drop shadow", "a glow or aura around it", "unrelated extra objects",
               "text or letters", "soft edges"] + QUIET_AVOID,
    ),
    "heartsong": dict(
        folder="heartsongs", size=(32, 32), background="transparent", max_colours=12, outline=True, margin=1, baseline=30,
        batch=11, priority="P1", gen_size="1024x1024", mode="grid", redo=True, calm=CALM["sprite"],
        display="2x at the opening and in the Heartsong reveal (64 px), 1x-2x on the shelf and in the ledger",
        frame=("{title}, one of the nine Heartsongs (a beloved object the realm's archmages sang a spell into): a {w}x{h} "
               "pixel art game item sprite, drawn so each art pixel is a crisp square block {k} px wide (the whole image "
               "is the {w}x{h} grid enlarged). Handmade, old and loved, warmer and more personal than any treasure, with "
               "a single deep-red accent; a simple, bold shape in flat tones. Three-quarter view from slightly above, "
               "upright, centred, its lowest pixels on the bottom of the frame, filling about 90% of the frame. 1-pixel "
               "very dark plum outline around the silhouette. Transparent background, no shadow, no glow, no frame, "
               "nothing else in the image."),
        avoid=["a background or floor", "a drop shadow", "a glow or aura around it", "musical notes floating",
               "text or letters", "soft edges"] + QUIET_AVOID,
    ),
    "curio": dict(
        folder="curios", size=(28, 28), background="transparent", max_colours=12, outline=True, margin=1,
        batch=12, priority="P1", gen_size="1024x1024", mode="grid", redo=True, calm=CALM["sprite"],
        display="2x in 56 px slots: Nim's cards, the ledger's Curios page, the counter's curio row, the Guild's night",
        frame=("{title}: a {w}x{h} pixel art icon of a single small object, drawn so each art pixel is a crisp square "
               "block about {k} px wide (the whole image is the {w}x{h} grid enlarged). A curio: an oddment from an old "
               "warded chest, made for a trade. A simple, bold shape: each material in two or three flat tones, at most "
               "one accent. Three-quarter view from slightly above, centred both ways, filling about 90% of the frame. "
               "1-pixel very dark plum outline around the silhouette. Transparent background, no shadow, no glow, no "
               "frame, nothing else in the image."),
        avoid=["a background", "a drop shadow", "a glow or aura around it", "more than one object unless described",
               "text, letters or numerals", "soft edges"] + QUIET_AVOID,
    ),
    "icon_upgrade": dict(
        folder="icons", size=(24, 24), background="transparent", max_colours=8, outline=True, margin=1,
        batch=14, priority="P1", gen_size="1024x1024", mode="grid", redo=True, calm=CALM["icon"],
        display="2x (48 px) on the day's-end upgrade cards; 1x on the slip for the Witchlight and the Lodestone",
        frame=("A {w}x{h} pixel art UI icon of {subject}, drawn so each art pixel is a crisp square block about {k} px "
               "wide. Bold, simple shapes that read at {w} pixels, in two or three flat tones per material; three-quarter "
               "or front view; centred, filling about 90% of the frame; 1-pixel very dark plum outline. Transparent "
               "background, no shadow, no glow, no frame."),
        avoid=["fine detail that won't read at 24 px", "a background", "text, letters or numerals"] + QUIET_AVOID,
    ),
    "icon_kind": dict(
        folder="kinds", size=(16, 16), background="transparent", max_colours=6, outline=True, margin=1,
        batch=14, priority="P1", gen_size="1024x1024", mode="grid", redo=True, calm=CALM["small_icon"],
        display="2x (32 px) at the start of each row of the appraisal slip, in the ledger and in Settings > Item sounds",
        frame=("A {w}x{h} pixel art symbol icon of {subject}, drawn so each art pixel is a crisp square block {k} px wide. "
               "One simple, iconic shape, like a map symbol, readable at {w} pixels, in two or three flat tones; centred, "
               "filling the frame; 1-pixel very dark plum outline. Transparent background, nothing else."),
        avoid=["detail", "more than one object", "a background", "text, letters or numerals"] + QUIET_AVOID,
    ),
    "emblem": dict(
        folder="emblems", background="transparent", max_colours=8, outline=True, margin=1,
        batch=14, priority="P1", gen_size="1024x1024", mode="grid", redo=True, calm=CALM["icon"],
        display="see the asset",
        frame=("A {w}x{h} pixel art emblem of {subject}, drawn so each art pixel is a crisp square block about {k} px wide. "
               "Front view, centred, symmetrical where it can be, in a few flat tones; 1-pixel very dark plum outline. "
               "Transparent background, no shadow, no glow."),
        avoid=["text, letters or numerals", "a background", "a glow"] + QUIET_AVOID,
    ),
    "story": dict(
        folder="story", background="transparent", max_colours=8, outline=True, margin=1,
        batch=14, priority="P1", gen_size="1536x1024", mode="grid", redo=True, calm=CALM["icon"],
        display="see the asset",
        frame=("A {w}x{h} pixel art game sprite of {subject}, drawn so each art pixel is a crisp square block about {k} px "
               "wide. Front view, lying flat as if on a table, centred, in a few flat tones; 1-pixel very dark plum "
               "outline. Transparent background, no shadow, no glow."),
        avoid=["text, letters or numerals", "a background", "a glow"] + QUIET_AVOID,
    ),
    "ui": dict(
        folder="ui", background="transparent", max_colours=8, outline=False, margin=0,
        batch=6, priority="P2", gen_size="none", mode="script", redo=False,
        display="nine-slice UI textures at 2x; see the asset for slice margins and tinting",
        frame="(Draw by script or by hand from the exact recipe below; no image generation needed.)",
        avoid=["anything outside the recipe"],
    ),
    "fx": dict(
        folder="fx", background="transparent", max_colours=6, outline=False, margin=0,
        batch=6, priority="P2", gen_size="1536x1024", mode="grid", redo=False,
        display="additive and tinted by the game; see the asset",
        frame=("A pixel art animation sprite sheet: {frames} frames of {fw}x{fh} art pixels in a single row (a {w}x{h} "
               "sheet), each art pixel a crisp square block. {subject} {colour_rule} Transparent background; each frame "
               "centred in its cell; no text."),
        avoid=["a background", "frames of different sizes", "text"],
    ),
    "lock_face": dict(
        folder="lockpick", size=(160, 160), background="transparent", max_colours=12, outline=True, margin=0,
        batch=15, priority="P2", gen_size="1024x1024", mode="grid", redo=True, calm=CALM["lock_face"],
        display="2x (320 px) in the middle of the lockpicking screen; the game draws the sweet spot, pins and the pick on "
                "top",
        frame=("A {w}x{h} pixel art face of {subject}, seen straight on, as the background of a lockpicking minigame, drawn "
               "so each art pixel is a crisp square block about {k} px wide. A round lock face, centred, in plain flat "
               "metal (two or three tones, no scratches), with a keyhole in the middle and a plain, dark, empty ring "
               "track around it (between 52 and 68 art pixels from the centre) where the game draws the pick's sweet "
               "spot. 1-pixel very dark plum outline. Transparent outside the lock, no shadow, no glow, no text."),
        avoid=["anything drawn inside the ring track", "a background", "text, letters or numerals", "a pick or key"]
              + QUIET_AVOID,
    ),
    "brand": dict(
        folder="brand", background="transparent", max_colours=24, outline=True, margin=1,
        batch=7, priority="P2", gen_size="1536x1024", mode="grid", redo=False,
        display="see the asset",
        frame=("{subject}, as {w}x{h} pixel art, each art pixel a crisp square block. 1-pixel very dark plum outline. "
               "Transparent background, no shadow, no glow."),
        avoid=["misspelled letters", "a background", "soft edges"],
    ),
    "scene": dict(
        folder="scenes", size=(320, 180), background="opaque", max_colours=24, outline=False, margin=0,
        batch=10, priority="P1", gen_size="1536x1024", mode="grid", redo=True, calm=CALM["scenery"],
        display="behind story scenes at 4x (the importer doubles it to 640x360, shown at 2x), dimmed to about 40% under "
                "the dialogue and the portraits",
        frame=("A {w}x{h} pixel art background for a story scene in a 2D game (16:9), drawn so each art pixel is a crisp "
               "square block about {k} px wide: the picture is the {w}x{h} grid enlarged to fill the image's width (it "
               "will be cropped to 16:9). Front elevation, a flat stage seen straight on, no people. Quiet: three or four "
               "large planes of nearly flat, muted colour with low contrast and one soft focal light; the centre third "
               "and the bottom third stay especially plain (portraits and a dialogue box cover them). Full-bleed, opaque, "
               "no border."),
        avoid=["people or creatures", "text or letters", "busy detail in the centre", "strong contrast"] + QUIET_AVOID,
    ),
    "store": dict(
        folder="store", background="opaque", max_colours=96, outline=False, margin=0,
        batch=8, priority="P3", gen_size="1536x1024", mode="area", redo=False,
        display="the Steam store page and library (not used in the game)",
        frame=("Key art for the Steam store page of Keywright, in the game's pixel art style at a coarse pixel size (each "
               "art pixel a crisp square block). {subject} Leave the logo area described below clear: the logo is added "
               "afterwards. No text."),
        avoid=["text or letters (the logo is composited later)", "photorealism", "a 3D render look"],
    ),
}

# Categories round 2 drops from the manifest: the chest is drawn in code (tools/custom_art.py, chest and chest_lock),
# so every finish shares one silhouette exactly and it stays as calm as the thing the player looks at most must be.
RETIRED = {"chest", "chest_lock"}

# Round 2 shows the redrawn things with this many of their listed details at most: the rest only crowd a small picture.
DETAILS_KEPT = {"portrait": 2, "item": 2, "heartsong": 2, "curio": 2, "icon_upgrade": 2, "icon_kind": 1, "emblem": 2,
                "story": 2, "lock_face": 2}


def palette_lookup():
    ramps = build_palette()
    by_name = {name: colours for name, colours, _ in ramps}
    return by_name, [c for _, cs, _ in ramps for c in cs]


def palette_steps(specs, by_name):
    """{'umber': [2, 3, 4], ...} from specs like 'umber:2-4', 'gold:5' or 'night' (the whole ramp), steps 1-based."""
    steps = {}
    for spec in specs:
        ramp, _, rng = spec.partition(":")
        if ramp not in by_name:
            raise ValueError(f"unknown ramp {spec}")
        n = len(by_name[ramp])
        if not rng:
            idx = range(1, n + 1)
        else:
            lo, _, hi = rng.partition("-")
            idx = range(int(lo), int(hi or lo) + 1)
        for i in idx:
            if not 1 <= i <= n:
                raise ValueError(f"{spec}: {ramp} has {n} colours")
        steps.setdefault(ramp, set()).update(idx)
    return {r: sorted(s) for r, s in steps.items()}


def describe_palette(specs, by_name):
    """'umber 2-4 (#4b2212 #6e3d22 #8e5b37); gold ...': the colours an asset should lean on."""
    out = []
    for ramp, idx in palette_steps(specs, by_name).items():
        if len(idx) == len(by_name[ramp]):
            label = ramp
        elif idx == list(range(idx[0], idx[-1] + 1)):
            label = f"{ramp} {idx[0]}" if len(idx) == 1 else f"{ramp} {idx[0]}-{idx[-1]}"
        else:
            label = f"{ramp} {','.join(map(str, idx))}"
        out.append(f"{label} ({' '.join(by_name[ramp][i - 1] for i in idx)})")
    return "; ".join(out)


def layout_fields(w):
    """The counter screen's fixed lines at a picture's own width (640 is the game's; round 2 draws at 320)."""
    f = w / 640
    x0, y0, x1, y1 = CALM_BOX
    return dict(counter_row=round(COUNTER_ROW * f), shelf_row=round(SHELF_ROW * f), shelf_x0=round(SHELF_X[0] * f),
                shelf_x1=round(SHELF_X[1] * f), calm_x0=round(x0 * f), calm_x1=round(x1 * f), calm_y0=round(y0 * f),
                calm_y1=round(COUNTER_ROW * f) - 1)


def assemble(asset, cat, by_name):
    w, h = asset["size"]
    k = None
    if cat["mode"] == "grid":
        gw, gh = (int(v) for v in cat["gen_size"].split("x"))
        # A picture fills the image's width (cropped to 16:9 after); a sprite fits whole in it.
        k = round(gw / w, 1) if cat["background"] == "opaque" else max(1, min(gw // w, gh // h))
    fields = dict(w=w, h=h, k=k, title=asset["title"], subject=asset.get("subject", asset["title"]),
                  **layout_fields(w), **asset.get("frame_fields", {}))
    frame = cat["frame"].format(**fields)
    parts = [STYLE, frame]
    if asset.get("description"):
        parts.append(f"Subject: {asset['description']}")
    details = asset.get("details") or []
    kept = DETAILS_KEPT.get(asset["category"])
    if kept is not None and len(details) > kept:
        details = details[:kept]
    if details:
        parts.append("It must show: " + "; ".join(details) + (", and nothing smaller." if kept else "."))
    if asset.get("palette"):
        parts.append("Lean on these palette ramps (dark to light): " + describe_palette(asset["palette"], by_name) + ".")
    avoid = cat["avoid"] + asset.get("avoid", [])
    parts.append("Avoid: " + "; ".join(avoid) + ".")
    return "\n\n".join(parts)


def main():
    by_name, flat = palette_lookup()
    assets = []
    for module in (assets_world, assets_cast, assets_things, assets_curios):
        assets.extend(module.ASSETS)

    ids = [a["id"] for a in assets]
    dupes = {i for i in ids if ids.count(i) > 1}
    assert not dupes, f"duplicate ids: {dupes}"

    out_assets = []
    for a in assets:
        if a["category"] in RETIRED:
            continue
        cat = CATEGORIES[a["category"]]
        spec = {k: v for k, v in cat.items() if k not in ("frame", "avoid", "folder")}
        spec.update({k: v for k, v in a.items() if k not in ("frame_fields",)})
        if cat.get("redo"):
            # Round 2's order: its anchors first (approved before anything else), then each category's batch.
            spec["batch"] = 9 if a.get("anchor_for") else cat["batch"]
            spec["size"] = list(cat["size"]) if a["category"] in ("background", "scene") else spec.get("size")
        spec.setdefault("size", cat.get("size"))
        assert spec["size"], f"{a['id']} has no size"
        spec["output"] = a.get("output") or f"art/generated/{cat['folder']}/{a['id'].split('.', 1)[1].replace('.', '_')}.png"
        spec["avoid"] = cat["avoid"] + a.get("avoid", [])
        spec["prompt"] = assemble({**a, "size": spec["size"]}, cat, by_name)
        if cat["mode"] == "grid":
            gw, gh = (int(v) for v in cat["gen_size"].split("x"))
            fw, fh = a.get("frame_size") or spec["size"]
            spec["gen_block"] = max(1, min(gw // (spec["size"][0] if not a.get("frame_size") else fw * a["frames"]),
                                           gh // fh))
        out_assets.append(spec)

    manifest = {
        "version": VERSION,
        "game": "Keywright",
        "art_direction": "art/ART_DIRECTION.md",
        "palette": {"hex": "art/palette/keywright.hex", "gpl": "art/palette/keywright.gpl",
                    "swatch": "art/palette/keywright.png", "ramps": "art/palette/ramps.md", "colours": len(flat)},
        "outline_colour": "#1a1420",
        "outline_colours_allowed": ["#1a1420", "#0b0810"],
        "round": 2,
        "round_notes": "Round 2 redraws everything marked redo, quieter (art/README.md says how and in what order); the "
                       "chest is drawn in code now and isn't listed.",
        "style_preamble": STYLE,
        "anchors": [a["id"] for a in out_assets if a.get("anchor_for")],
        "categories": {k: {kk: vv for kk, vv in v.items() if kk not in ("frame",)} for k, v in CATEGORIES.items()},
        "assets": out_assets,
    }
    with open(os.path.join(ART, "manifest.json"), "w") as f:
        json.dump(manifest, f, indent=1, ensure_ascii=True)
        f.write("\n")
    write_inventory(manifest)
    counts = {}
    for a in out_assets:
        counts[a["priority"]] = counts.get(a["priority"], 0) + 1
    print(f"{len(out_assets)} assets ({', '.join(f'{k}: {v}' for k, v in sorted(counts.items()))}) -> art/manifest.json")


def write_inventory(manifest):
    """The same list for people: a table per category."""
    lines = ["# Keywright: art inventory", "",
             "Every picture the game needs, generated from `art/tools/build_manifest.py` (edit there, not here). The full "
             "spec and the assembled prompt for each asset are in `art/manifest.json`; the shared style is "
             "`art/ART_DIRECTION.md`.", "",
             "Priorities: **P1** replaces something the game shows today; **P2** replaces a code-drawn stand-in or adds "
             "polish the game is ready for; **P3** is new (story scenes, extra portraits, store art) and needs code to "
             "use it.", "",
             "Round 2 (0.25) redraws every category marked *redraw*, quieter, in batches 9 (its anchors) to 15; the "
             "chest is drawn in code now (`tools/custom_art.py`) and isn't listed. See `art/README.md`.", ""]
    order = list(CATEGORIES)
    total = {}
    for a in manifest["assets"]:
        total.setdefault(a["category"], []).append(a)
    lines.append("| Category | Assets | Native size | Batch | Priority | Round 2 |")
    lines.append("|---|---|---|---|---|---|")
    for c in order:
        if c not in total:
            continue
        sizes = sorted({f"{a['size'][0]}x{a['size'][1]}" for a in total[c]})
        pr = sorted({a['priority'] for a in total[c]})
        batches = sorted({a['batch'] for a in total[c]})
        redo = "redraw" if CATEGORIES[c].get("redo") else "keep"
        lines.append(f"| {c} | {len(total[c])} | {', '.join(sizes[:4])}{'...' if len(sizes) > 4 else ''} | "
                     f"{', '.join(map(str, batches))} | {', '.join(pr)} | {redo} |")
    lines.append(f"| **all** | **{len(manifest['assets'])}** | | | | |")
    lines.append("")
    lines.append("Style anchors (round 2: batch 9, approved before the rest): " +
                 ", ".join(f"`{i}`" for i in manifest["anchors"]) + ".")
    lines.append("")
    for c in order:
        if c not in total:
            continue
        lines.append(f"## {c}")
        lines.append("")
        lines.append("| Id | Title | Size | Pri | Output | Replaces | Description |")
        lines.append("|---|---|---|---|---|---|---|")
        for a in total[c]:
            desc = (a.get("description") or a.get("subject") or "").replace("|", "/")
            rep = a.get("replaces") or "(new)"
            lines.append(f"| `{a['id']}` | {a['title']} | {a['size'][0]}x{a['size'][1]} | {a['priority']} | "
                         f"`{a['output']}` | `{rep}` | {desc} |")
        lines.append("")
    with open(os.path.join(ART, "INVENTORY.md"), "w") as f:
        f.write("\n".join(lines))


if __name__ == "__main__":
    main()
