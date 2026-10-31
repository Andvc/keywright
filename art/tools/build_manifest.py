"""
Builds art/manifest.json and art/INVENTORY.md: every picture Keywright needs, with its spec and an assembled prompt.

The asset descriptions live in art/tools/assets_*.py; ART_DIRECTION.md is the style they share and scene.py the shop
scene's layout. Run: python3 art/tools/build_manifest.py
"""
import json
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ART = os.path.dirname(HERE)
ROOT = os.path.dirname(ART)
sys.path.insert(0, HERE)

from make_palette import build as build_palette  # noqa: E402
import assets_curios  # noqa: E402
import assets_figures  # noqa: E402
import assets_scene  # noqa: E402
import assets_things  # noqa: E402
import assets_world  # noqa: E402
import scene  # noqa: E402

VERSION = 5

# Owner's correction, 2026-10-31: C2 at 4x, detail allowed, no outlines.
# This overrides C2's inked edges in the historical reference. See art/study/CHOICE.md.
STYLE = (
    "A warm storybook cartoon illustration for Keywright, a cosy fantasy shop game, based on the approved C2 "
    "reference's rounded shapes, warm colours, readable faces and crafted objects, but WITHOUT ANY OUTLINES. "
    "Separate shapes with adjacent colour fills and two or three cel-shaded material tones, never inked contour "
    "strokes. No black, dark plum or other coloured line tracing around silhouettes or inside shapes; no comic "
    "linework, border strokes or dark halos. Natural eyes, small facial features, material seams and cast-shadow "
    "areas may stay dark where needed, but never form an encircling stroke. A script makes chunky 4x pixel art "
    "afterwards: draw a smooth illustration, not a pixel grid. Keep recognisable, purposeful detail: three or four "
    "bold features per figure or object where the small size permits, such as a patch, a seam, a nail head, a tied "
    "cord or a carved mark, expressed as filled shapes. Larger scenery can have grouped flowers, fences and signs "
    "of daily use at its edges. Preserve broad areas between these features; no all-over speckling, hatching, fine "
    "grain or individual hair strands. Keep details broad enough to survive reduction. Use two or three main "
    "cel-shaded tones per material and a restrained highlight; distant scenery has softer, lighter colour contrast. "
    "Soft, muted, warm colours; shadows lean violet, lights warm yellow, one gentle light from the upper left, "
    "neutral daylight. Plain writing areas stay plain. No text, letters, numbers or rune-like marks. Flat pure "
    "magenta (#FF00FF) means empty: never use magenta or hot pink in the subject itself."
)

MAGENTA_BG = "Background: flat pure magenta #FF00FF, nothing else in the picture."

# How quiet a finished asset must measure, at its native size (art/tools/artlib.py's calm_metrics, over the inside of
# the picture): lone specks (pixels unlike all four neighbours), edges (neighbouring pairs that differ) and busy (the
# mean brightness step between neighbours, 0-255). C2's measured whole image and representative crops establish
# the revised ceilings before batch 9r generation; crop coordinates and rationale are in art/study/CHOICE.md.
CALM = {
    "sky": dict(specks=0.005, edges=0.05, busy=3),
    "scenery": dict(specks=0.06, edges=0.35, busy=16),
    "frame": dict(specks=0.05, edges=0.27, busy=14),
    "counter": dict(specks=0.02, edges=0.18, busy=7),
    "sheet": dict(specks=0.01, edges=0.08, busy=4),
    "figure": dict(specks=0.08, edges=0.36, busy=22),
    "sprite": dict(specks=0.06, edges=0.5),
    "icon": dict(specks=0.06, edges=0.55),
    "small_icon": dict(specks=0.06, edges=0.6),
    "lock_face": dict(specks=0.03, edges=0.2, busy=9),
}

QUIET_AVOID = ["outlines, contour strokes or ink linework", "pixel-art style or visible pixels", "all-over texture, grain, hatching or noise",
               "airbrushed gradients", "details too thin to survive reduction", "highlights scattered over surfaces",
               "unmotivated decorative clutter"]
SPRITE_AVOID = ["a background other than flat magenta", "a drop shadow", "a glow or aura", "text or letters",
                "more than one object unless described"] + QUIET_AVOID

# What every category shares: native size, background, colour cap, outline, how the pixelizer fits it to its canvas
# (`fit`), the prompt's frame, how quiet it must measure (`calm`), the mean brightness range of its opaque pixels
# (`value`, 0-255), whether round 2 redraws it (`redo`), and its batch (batch 9 is round 2's anchors, first).
CATEGORIES = {
    "sky": dict(
        folder="sky", size=(320, 180), background="opaque", max_colours=10, outline=False, margin=0, fit="canvas",
        batch=10, priority="P1", gen_size="1536x1024", mode="flat", redo=True, calm=CALM["sky"],
        display="the back of the scene at 4x, behind every place; the game picks it for the time of day",
        frame=("{title}: a plain sky for the back of a game scene. It fills the whole 16:9 picture and there is nothing "
               "else in it (no ground, hills, buildings, trees or birds). Three to five broad horizontal bands of flat "
               "colour from top to bottom, with simple flat shapes for the sun or moon and the clouds."),
        avoid=["anything but sky", "stars all over"] + QUIET_AVOID,
    ),
    "place": dict(
        folder="place", size=(320, 180), background="transparent", max_colours=32, outline=False, margin=0, fit="frame",
        batch=10, priority="P1", gen_size="1536x1024", mode="flat", redo=True, calm=CALM["scenery"], value=(45, 150),
        alpha_min=0.5,
        display="at 4x behind the seller and the counter, lit by the time of day; the sky shows where it's keyed out",
        frame=("{title}: the view across a shop counter, seen straight ahead from behind it at standing eye level, as a "
               "16:9 picture. Two things are left out because the game adds them: the counter itself (it covers the "
               "bottom third) and the sky: everything that would be sky, above the skyline and seen through windows, "
               "doors or arches, is flat pure magenta #FF00FF. Nobody is in it. The horizon, or the foot of the far wall, "
               "a little above the middle; the bottom third plain ground or floor; the left third and the middle "
               "simple (the game stands someone at the left and puts things in the middle, in front); the place's "
               "landmark in the middle distance just right of centre. Muted, mid to dark values with low contrast, in "
               "three to five large planes, with small groups of purposeful detail around the edges."),
        avoid=["people or animals", "a chest", "text or signs with letters", "a counter or table in the foreground", "a sky "
               "(it's magenta)", "a floor of tiles or boards in perspective lines", "uniformly dense foliage or detail across the centre", "strong contrast in the distance"]
              + QUIET_AVOID,
    ),
    "frame": dict(
        folder="frame", size=(320, 180), background="transparent", max_colours=24, outline=False, margin=0, fit="frame",
        batch=10, priority="P1", gen_size="1536x1024", mode="flat", redo=True, calm=CALM["frame"], value=(25, 130),
        display="at 4x in front of the place and the seller: the top bar sits on its top band, the shelf and the "
                "lanterns hang from it",
        frame=("{title}: the part of a shop nearest the viewer that frames the view, seen straight on from inside. "
               "Across the top of the picture, {subject}, about a sixth of the picture's height deep; the topmost "
               "twelfth of the picture is {band}, plain and dark (the game's top bar sits on it). Down the left and right "
               "edges, {posts}, each about a twentieth of the picture's width. Everything "
               "else, the whole middle of the picture, is flat pure magenta #FF00FF. Symmetrical, and a little darker "
               "than the room beyond: it's in the shop's own shade."),
        avoid=["anything in the middle of the picture", "a shelf", "lamps or lanterns", "people"] + QUIET_AVOID,
    ),
    "shelf": dict(
        folder="shelf", size=(136, 40), background="transparent", max_colours=16, outline=False, margin=0, fit="shelf",
        board_row=34, batch=10, priority="P1", gen_size="1536x1024", mode="flat", redo=True, calm=CALM["sprite"],
        display="at 4x hung from the frame, its board's top on screen row 196 (x 300-804): kept things stand on it",
        frame=("{title}: a single long, thin display shelf seen straight on: {subject}, about twenty times as wide as it "
               "is thick, spanning the picture's width, with {supports} rising straight up from its two ends to the top "
               "edge of the picture. Nothing on it. " + MAGENTA_BG),
        avoid=["things on the shelf", "brackets below it", "a wall behind it"] + SPRITE_AVOID,
    ),
    "counter": dict(
        folder="counter", size=(320, 61), background="opaque", max_colours=16, outline=False, margin=0, fit="strip",
        batch=10, priority="P1", gen_size="1536x1024", mode="flat", redo=True, calm=CALM["counter"], value=(45, 115),
        display="at 4x across the bottom third of the screen, its far edge on screen row 476",
        frame=("{title}: a shop counter's top seen from behind it at standing height: a very wide, flat strip filling "
               "the bottom third of the picture from edge to edge, its far edge a straight horizontal line with a "
               "slightly lighter lip. Above the strip, flat pure magenta #FF00FF. Plain, calm and mid-dark: the game "
               "puts its notes, a chest and a board on it."),
        avoid=["things on the counter", "a front panel", "perspective lines converging", "all-over grain, scratches or bright decoration in the centre"]
              + QUIET_AVOID,
    ),
    "figure": dict(
        folder="figures", size=(96, 160), background="transparent", max_colours=24, outline=False, margin=0, fit="figure",
        batch=11, priority="P1", gen_size="1024x1536", mode="flat", redo=True, calm=CALM["figure"], alpha_min=0.45,
        display="at 4x standing behind the counter (it hides their legs); the face cropped for the queue, the notebook "
                "and the character cards; standing in story scenes",
        frame=("{title}: a full-length figure for a game, standing upright, from the top of the head (or hat) to the "
               "feet, centred and filling about ninety percent of the picture's height. {view} Storybook proportions, "
               "the head about a fifth of the height; arms relaxed{held}. The face in flat skin tones, the eyes clear "
               "dark dots and a simple mouth, readable from across a room. " + MAGENTA_BG),
        avoid=["a background or floor", "a shadow", "a frame", "small accessories crowding the figure", "strands of "
               "hair", "a weapon unless described"] + QUIET_AVOID,
    ),
    "sheet": dict(
        folder="sheet", size=(216, 320), background="opaque", max_colours=12, outline=False, margin=0, fit="canvas",
        batch=10, priority="P1", gen_size="1024x1536", mode="flat", redo=True, calm=CALM["sheet"], value=(165, 228),
        display="at 2x behind the appraisal slip (x 820-1252, y 66-706): the slip is printed on it in dark ink",
        frame=("{title}: a sheet to write an appraisal on, seen flat and straight on, filling a tall picture (two wide by "
               "three tall) from edge to edge: {subject}, one flat light warm tone, plain and empty in the middle (no "
               "writing, lines, stains or texture); around it, {rim}, only a few hundredths of the width thick; "
               "{ornament}, small, and nothing else."),
        avoid=["writing or ruled lines", "stains, creases or texture", "a dark paper", "a table behind it"] + QUIET_AVOID,
    ),
    "prop": dict(
        folder="props", background="transparent", max_colours=12, outline=False, margin=0, fit="sprite",
        batch=10, priority="P1", gen_size="1024x1024", mode="flat", redo=True, calm=CALM["sprite"],
        display="at 4x: on the counter beside the chest, or (lanterns) hung on the frame's posts",
        frame=("{title}: a single small object for a shop game, seen straight on and a little from above, centred, "
               "filling about eighty percent of the picture{stance}. " + MAGENTA_BG),
        avoid=SPRITE_AVOID,
    ),
    "item": dict(
        folder="items", size=(32, 32), background="transparent", max_colours=16, outline=False, margin=1, baseline=30,
        fit="sprite", batch=12, priority="P1", gen_size="1024x1024", mode="flat", redo=True, calm=CALM["sprite"],
        display="2x on the opening table and the shelf (64 px); 1x or 2x in slots",
        frame=("{title}: a single treasure for a game, seen three-quarters from slightly above as if resting on a "
               "counter, upright and centred, filling about ninety percent of the picture, its lowest point on the "
               "bottom edge. One bold silhouette, each material in two or three main tones, with three or four readable features. "
               + MAGENTA_BG),
        avoid=SPRITE_AVOID,
    ),
    "heartsong": dict(
        folder="heartsongs", size=(32, 32), background="transparent", max_colours=16, outline=False, margin=1,
        baseline=30, fit="sprite", batch=12, priority="P1", gen_size="1024x1024", mode="flat", redo=True,
        calm=CALM["sprite"],
        display="2x at the opening and in the Heartsong reveal (64 px), 1x-2x on the shelf and in the collection",
        frame=("{title}, one of the nine Heartsongs (a beloved object the realm's archmages sang a spell into): a single "
               "object, handmade, old and loved, warmer and more personal than any treasure, with one deep-red accent; "
               "seen three-quarters from slightly above, upright, centred, filling about ninety percent of the picture, "
               "its lowest point on the bottom edge. " + MAGENTA_BG),
        avoid=["musical notes floating"] + SPRITE_AVOID,
    ),
    "curio": dict(
        folder="curios", size=(28, 28), background="transparent", max_colours=16, outline=False, margin=1, fit="sprite",
        batch=13, priority="P1", gen_size="1024x1024", mode="flat", redo=True, calm=CALM["sprite"],
        display="2x in 56 px slots: Nim's cards, the collection, the counter's curio row, the Guild's night",
        frame=("{title}: a single small object, an oddment from an old warded chest, made for a trade; seen "
               "three-quarters from slightly above, centred, filling about ninety percent of the picture. One bold "
               "shape, each material in two or three main tones, with up to three readable features. " + MAGENTA_BG),
        avoid=SPRITE_AVOID,
    ),
    "icon_upgrade": dict(
        folder="icons", size=(24, 24), background="transparent", max_colours=12, outline=False, margin=1, fit="sprite",
        batch=14, priority="P1", gen_size="1024x1024", mode="flat", redo=True, calm=CALM["icon"],
        display="2x (48 px) on the day's-end upgrade cards; 1x on the slip for the Witchlight and the Lodestone",
        frame=("A bold, simple icon of {subject}, readable when tiny: three-quarter or front view, centred, filling "
               "about ninety percent of the picture, each material in two or three flat tones. " + MAGENTA_BG),
        avoid=["fine detail"] + SPRITE_AVOID,
    ),
    "icon_kind": dict(
        folder="kinds", size=(16, 16), background="transparent", max_colours=8, outline=False, margin=1, fit="sprite",
        batch=14, priority="P1", gen_size="1024x1024", mode="flat", redo=True, calm=CALM["small_icon"],
        display="2x (32 px) at the start of each row of the appraisal slip, in the notebook and in Settings",
        frame="A symbol of {subject}, like a map symbol: one simple shape in two or three flat tones, centred, filling "
              "the picture. " + MAGENTA_BG,
        avoid=["detail"] + SPRITE_AVOID,
    ),
    "emblem": dict(
        folder="emblems", background="transparent", max_colours=12, outline=False, margin=1, fit="sprite",
        batch=14, priority="P1", gen_size="1024x1024", mode="flat", redo=True, calm=CALM["icon"],
        display="see the asset",
        frame="An emblem of {subject}: front view, centred, symmetrical where it can be, in a few flat tones. " + MAGENTA_BG,
        avoid=SPRITE_AVOID,
    ),
    "story": dict(
        folder="story", background="transparent", max_colours=12, outline=False, margin=1, fit="sprite",
        batch=14, priority="P1", gen_size="1536x1024", mode="flat", redo=True, calm=CALM["icon"],
        display="see the asset",
        frame="{subject}, seen from straight above as if lying on a table, centred, in a few flat tones. " + MAGENTA_BG,
        avoid=SPRITE_AVOID,
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
        folder="lockpick", size=(160, 160), background="transparent", max_colours=16, outline=False, margin=0,
        fit="sprite", batch=15, priority="P2", gen_size="1024x1024", mode="flat", redo=True, calm=CALM["lock_face"],
        display="2x (320 px) in the middle of the lockpicking screen; the game draws the sweet spot, pins and the pick "
                "on top",
        frame=("The round face of {subject}, seen straight on, for the background of a lockpicking game: centred, "
               "filling the picture, plain flat metal in two or three tones, a keyhole in the middle and a plain, dark, "
               "empty ring track around it from two thirds to six sevenths of the way out from the centre (the game draws "
               "the pick there). Outside the lock, flat pure magenta #FF00FF."),
        avoid=["anything drawn inside the ring track", "a pick or key"] + SPRITE_AVOID,
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
        folder="scenes", size=(320, 180), background="opaque", max_colours=32, outline=False, margin=0, fit="canvas",
        batch=10, priority="P1", gen_size="1536x1024", mode="flat", redo=True, calm=CALM["scenery"],
        display="behind story scenes at 4x, dimmed to about 40% under the dialogue and the figures",
        frame=("{title}: a picture for a story scene in a game, 16:9, seen straight on, nobody in it: three or four "
               "large planes of muted flat colour with low contrast and one soft focal light; the middle and the bottom "
               "third especially plain (figures stand there and a dialogue box covers the bottom). Full-bleed."),
        avoid=["people or creatures", "text or letters", "busy detail in the middle", "strong contrast"] + QUIET_AVOID,
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

# Categories the manifest doesn't list any more: the chest is drawn in code (tools/custom_art.py); round 1's single
# shop backgrounds became the scene's layers (assets_scene.py), and its portraits full-length figures (assets_figures.py).
RETIRED = {"chest", "chest_lock", "background", "portrait"}

# The most of an asset's listed details its prompt names: the rest only crowd a small picture.
DETAILS_KEPT = {"figure": 4, "place": 8, "frame": 4, "shelf": 3, "counter": 3, "sheet": 2, "prop": 3, "item": 4,
                "heartsong": 4, "curio": 3, "icon_upgrade": 3, "icon_kind": 1, "emblem": 3, "story": 3, "lock_face": 3}


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


def assemble(asset, cat, by_name):
    w, h = asset["size"]
    fields = dict(w=w, h=h, title=asset["title"], subject=asset.get("subject", asset["title"]), **asset.get("frame_fields", {}))
    if asset["category"] == "prop":
        fields["stance"] = (", hanging from the top edge of the picture by its hook" if asset.get("hangs")
                            else ", standing with its base on the bottom edge")
    if cat["mode"] == "grid":
        gw, gh = (int(v) for v in cat["gen_size"].split("x"))
        fields["k"] = max(1, min(gw // w, gh // h))
    frame = cat["frame"].format(**fields)
    parts = [STYLE, frame] if cat["mode"] == "flat" else [frame]
    if asset.get("description"):
        parts.append(f"Subject: {asset['description']}")
    details = asset.get("details") or []
    kept = DETAILS_KEPT.get(asset["category"])
    if kept is not None and len(details) > kept:
        details = details[:kept]
    if details:
        suffix = ". Keep these features readable after reduction." if cat.get("redo") else "."
        parts.append("It must show: " + "; ".join(details) + suffix)
    if asset.get("palette"):
        parts.append("Lean on these palette colours (dark to light; the swatch has them all): "
                     + describe_palette(asset["palette"], by_name) + ".")
    avoid = cat["avoid"] + asset.get("avoid", [])
    parts.append("Avoid: " + "; ".join(avoid) + ".")
    return "\n\n".join(parts)


def scene_layout():
    """The shop scene's layout, for the manifest: what the game does with the layers (art/tools/scene.py)."""
    return {
        "grid": [scene.GRID_W, scene.GRID_H], "screen_pixels_per_art_pixel": scene.SCALE,
        "order": ["sky", "place", "figure", "shelf", "frame", "lanterns", "counter", "props", "chest", "sheet", "interface"],
        "hud_rows": scene.HUD_ROWS, "frame_depth_rows": list(scene.FRAME_DEPTH), "post_width": scene.POST_WIDTH,
        "shelf_row": scene.SHELF_ROW, "shelf_span": list(scene.SHELF_SPAN), "shelf_canvas_at": list(scene.SHELF_AT),
        "counter_row": scene.COUNTER_ROW, "chest_box": list(scene.CHEST),
        "figure_canvas_at": list(scene.FIGURE_AT), "figure_feet_row": scene.FIGURE_FEET,
        "figure_shows_at_least": scene.FIGURE_SHOWS, "lantern_hooks": [list(p) for p in scene.LANTERNS],
        "prop_spots": [list(p) for p in scene.PROP_SPOTS], "sheet_on_screen": list(scene.SHEET),
        "times": {k: v["tint"] for k, v in scene.TIMES.items()},
        "reference": "art/reference/scene/",
    }


def main():
    by_name, flat = palette_lookup()
    assets = []
    for module in (assets_scene, assets_figures, assets_world, assets_things, assets_curios):
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
        spec.update({k: v for k, v in a.items() if k not in ("frame_fields",) and v is not None})
        if cat.get("redo"):
            # Round 2's order: its anchors first (approved before anything else), then each category's batch.
            spec["batch"] = 9 if a.get("anchor_for") else cat["batch"]
            if cat.get("size"):
                spec["size"] = list(cat["size"]) if not a.get("size") or a["category"] != "prop" else a["size"]
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
        if a["category"] == "prop" and a.get("hangs"):
            spec["baseline"] = None
        elif a["category"] == "prop":
            spec["baseline"] = spec["size"][1] - 1
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
        "round_notes": "Owner selected C2 at 4x, with detail allowed and NO OUTLINES (art/study/CHOICE.md). Generate warm "
                       "cartoon colour shapes without drawn or automatic contours; pixelize.py --mode flat makes the "
                       "pixel art. Rebuild the 13 anchors "
                       "and stop for approval before batches 10-15. The chest is drawn in code and isn't listed.",
        "style_preamble": STYLE,
        "scene": scene_layout(),
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
             "`art/ART_DIRECTION.md`, and the shop scene's layout `art/tools/scene.py`.", "",
             "Priorities: **P1** the game needs it; **P2** polish the game is ready for; **P3** new, and needs code to "
             "use it.", "",
             "Round 2 (0.25) redraws every category marked *redraw*, flat and quiet, in batches 9 (its anchors) to 15, "
             "and builds the shop scene from layers: a sky for each time of day, and for each shop the place, its frame, "
             "shelf, counter, appraisal sheet and lanterns, with props for what you've bought and full-length figures for "
             "everyone at the counter. The chest is drawn in code (`tools/custom_art.py`). See `art/README.md`.", ""]
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
