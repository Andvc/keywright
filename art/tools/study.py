"""
The style study (0.25). Batch 9 read as a little too plain, so before round 2 goes on, the stall's counter is drawn in
four styles at three levels of detail, made into pixels at both densities the game could use, and put together the way
the counter screen shows them, side by side, for the owner to choose a direction. art/study/README.md says how to run it.

  python3 art/tools/study.py prompts                           # art/study/prompts.md and prompts.json, from here
  python3 art/tools/study.py scene F2 raw.png [--offset-y N]   # a generated concept into art/study/out/
  python3 art/tools/study.py item F raw.png                    # a style's thing into art/study/out/
  python3 art/tools/study.py review                            # art/study/review/: the overviews, each variant, things

A variant is a style and a level of detail. Styles: F flat storybook (round 2 as briefed), C outlined cartoon, P soft
painted, X pixel art. Levels: 1 plain (batch 9's budget), 2 lived-in, 3 rich. F1 is batch 9 as delivered (nothing to
generate). F, C and P are drawn as illustrations and made into pixels twice: on the 320x180 grid at 4x (round 2's
density) and on a 640x360 grid at 2x (round 1's). X is drawn as pixel art on its own grid: X1-X3 on 320x180, X2d on
640x360. Each review picture is the counter screen at 1280x720 with what the game puts over the art: the top bar, the
seller's speech bubble, the chest and the appraisal sheet.
"""
import argparse
import hashlib
import json
import os
import sys

from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import artlib  # noqa: E402
import pixelize  # noqa: E402
import scene  # noqa: E402
import validate  # noqa: E402

STUDY = artlib.path("art/study")
OUT = os.path.join(STUDY, "out")
REVIEW = os.path.join(STUDY, "review")
UI = os.path.join(STUDY, "ui")

INTRO = ("Concept art for Keywright, a cosy fantasy shop game that people look at for hours: one whole screen of the "
         "game, made to choose the game's art style. ")

# Each style: how it's drawn (`text`, for the scene and the thing alike) and what it means for a whole scene (`scene`).
STYLES = {
    "F": dict(name="Flat storybook", mode="flat", text=(
        "Style: a flat-colour illustration, like a picture book printed in flat inks. Clean, vector-like shapes, each a "
        "solid fill with a crisp edge; no outlines or line work, no gradients, no texture. Two or three flat tones for "
        "each material: a lit side, a shaded side, a small highlight. Soft, muted, warm colours; shadows lean toward "
        "violet, lights toward warm yellow; one gentle light from the upper left."), scene="",
        avoid=["outlines or line work", "gradients", "texture or noise"]),
    "C": dict(name="Outlined cartoon", mode="flat", text=(
        "Style: a clean cartoon illustration, like a modern storybook adventure game. Every shape has a bold, even "
        "outline in dark plum (#1a1420), about half a percent of the picture's width thick; flat cel shading in two or "
        "three tones with crisp-edged shadow shapes; no gradients, no texture. Soft, muted, warm colours; one gentle "
        "light from the upper left."), scene="",
        avoid=["thin, sketchy or broken lines", "gradients", "texture or noise"]),
    "P": dict(name="Soft painted", mode="flat", text=(
        "Style: a soft gouache storybook painting. Simple shapes with softened edges, gentle gradients, a light "
        "painterly texture, three or four tones for each material. Warm, muted colours; light from the upper left."),
        scene=" The gradients run across big areas (the sky, the fields, the counter), and distant things are paler "
              "and bluer, as in a hazy summer landscape.",
        avoid=["harsh contrast", "heavy brush texture", "photographic detail"]),
    "X": dict(name="Pixel art", mode="grid", text=(
        "Style: hand-made pixel art, like a calm 16-bit adventure game. The whole picture is drawn on a grid of {grid} "
        "big square pixels (each about {cell} pixels of this image), every pixel one solid colour: large clusters of "
        "flat colour, a limited, muted palette, dark outlines around shapes, little or no dithering, no noise or "
        "speckle. Warm, soft colours; light from the upper left."),
        scene=" Big areas (the sky, the fields, the counter) stay flat clusters, with no texture over them.",
        avoid=["noise, speckle or dithering over big areas", "blurry or soft pixels", "pixels of different sizes"]),
}

DETAIL = {
    1: ("Plain", "Detail: plain. A few big, simple shapes; at most two small details on any part; plain areas stay "
                 "plain."),
    2: ("Lived-in", "Detail: lived-in. Each part has three or four well-chosen details that tell you about the place, "
                    "and nothing is scattered: a bundle of dried herbs and a little brass bell hanging from the awning; "
                    "a crate of turnips at the side of the stall; a cart with a barrel on the road; a few flower "
                    "patches and a low fence along the verge; two birds over the hills; smoke curling from the mill; "
                    "seams and a few nail heads on the counter's planks."),
    3: ("Rich", "Detail: rich. A lively scene full of small stories, but organised: the most detail in the middle "
                "distance, and calm, simple areas where the game's text sits (above Hob's head, the counter's front, "
                "the right third). The awning hung with herb bundles, a bell and a string of little flags; crates, "
                "sacks and a sleeping cat at the stall's sides; a cart and two small travellers on the road; fields "
                "in strips with fences and flowers, sheep on the hills, the castle with its flags, the watermill with "
                "smoke from its chimney; texture suggested with a few strokes (wood grain, straw, stones in the "
                "road); sunlight and cast shadows (the awning's shadow along the back of the counter, the posts' "
                "shadows)."),
}

SCENE = (
    "The picture: one screen of the game, a 16:9 view (keep everything that matters inside the middle 16:9 band; the "
    "top and bottom tenth of this image may be cut off), seen straight ahead from behind the counter of Bram's Stall, a "
    "roadside market stall at the Brindle crossroads, at midday. From top to bottom: along the very top, a plain dark "
    "wooden beam about an eighth of the view deep (the game's top bar sits on it), and under it the stall's awning, "
    "scalloped in alternating crimson and cream, reaching about a fifth of the way down. At the left and right edges, "
    "the stall's wooden posts, each with a small tin lantern hanging about a third of the way down. Under the awning, a "
    "plain wooden display shelf hung on two ropes, spanning the middle about a quarter of the way down, empty (the game "
    "puts things on it). Beyond the stall, the crossroads: a packed-earth road running away to low rolling hills, a "
    "wooden signpost with two blank arms just right of centre, a castle's small silhouette on a far hill, a river and a "
    "small watermill on the right, round trees, a clear sky with a few clouds. On the left, standing behind the counter "
    "and turned a little to the right: Hob the Farmer, a broad, sunburnt man in his fifties with a frayed straw hat, "
    "grey stubble and a shrewd half-smile, a brown work smock over a homespun shirt; the counter hides him from the "
    "waist down, and his face is about a third of the way down the view. The counter's top fills the bottom third: its "
    "far edge a straight horizontal line two thirds of the way down, dark brown planks running left to right; the "
    "middle of that edge stays empty (the game stands a chest there). The right third stays quiet and simple (the game "
    "covers it with a paper sheet).")

COMMON_AVOID = ["text, letters or numbers", "a chest", "anyone else at the counter", "a border or frame around the "
                "picture", "a dark vignette"]

ITEM = ("{intro}{style} The subject, a single thing found in a chest: {description} One object, in a three-quarter view "
        "from slightly above as if resting on a table, filling about eighty percent of the picture. Background: flat "
        "pure magenta (#FF00FF), nothing else in the picture.")

# Ramps the scene leans on (the snapping prefers these when two palette colours are about as near).
HINTS = ["leaf:2-4", "moss:2-3", "umber:2-5", "sea:2-3", "night:4-5", "parchment:2-3", "skin:2-4", "gold:3-5",
         "crimson:2-3", "azure:5-6", "steel:4", "night:9-10"]

# Pixel art's own grids, and the illustrations' two densities.
GRIDS = {4: (320, 180), 2: (640, 360)}


def variant_ids():
    ids = []
    for s in STYLES:
        for d in DETAIL:
            ids.append(f"{s}{d}")
    return ids + ["X2d"]


def parse(vid):
    """'F2' -> ('F', 2, densities), 'X2d' -> ('X', 2, [2]). F1 is batch 9 as delivered."""
    style, detail = vid[0], int(vid[1])
    if style == "X":
        return style, detail, [2] if vid.endswith("d") else [4]
    return style, detail, [4, 2]


def scene_prompt(vid):
    style, detail, densities = parse(vid)
    s = STYLES[style]
    grid = GRIDS[densities[0]]
    text = s["text"].format(grid=f"{grid[0]} by {grid[1]}", cell=f"{1536 / grid[0]:.1f}") + s["scene"]
    avoid = s["avoid"] + COMMON_AVOID
    return (INTRO + text + " " + DETAIL[detail][1] + "\n\n" + SCENE + "\n\nColours close to the attached palette "
            "(soft, muted and warm). Avoid: " + "; ".join(avoid) + ".")


def item_prompt(style, manifest):
    spec = artlib.asset(manifest, "item.dwarf_gold_crowns")
    s = STYLES[style]
    text = s["text"].format(grid="32 by 32", cell="32")
    return ITEM.format(intro=INTRO.replace("one whole screen of the game", "one thing from the game"), style=text,
                       description=spec["description"])


def cmd_prompts(_args):
    manifest = artlib.load_manifest()
    rows = []
    for vid in variant_ids():
        if vid == "F1":
            continue
        style, detail, densities = parse(vid)
        rows.append(dict(id=vid, kind="scene", style=STYLES[style]["name"], detail=DETAIL[detail][0],
                         made=[f"{GRIDS[d][0]}x{GRIDS[d][1]} at {d}x" for d in densities], gen_size="1536x1024",
                         candidates=2, prompt=scene_prompt(vid)))
    for style in STYLES:
        rows.append(dict(id=f"item_{style}", kind="item", style=STYLES[style]["name"], detail="Lived-in",
                         made=["32x32, shown at 2x"], gen_size="1024x1024", candidates=2,
                         prompt=item_prompt(style, manifest)))
    os.makedirs(STUDY, exist_ok=True)
    with open(os.path.join(STUDY, "prompts.json"), "w") as f:
        json.dump(rows, f, indent=1)
        f.write("\n")
    lines = ["# Style study: the prompts", "",
             "Generated by `python3 art/tools/study.py prompts` (edit the definitions there, not this file). Pass the "
             "palette swatch `art/palette/keywright.png` with every request. Never pass round 1's or round 2's art.",
             ""]
    for r in rows:
        lines += [f"## {r['id']}: {r['style']}, {r['detail'].lower()} ({r['kind']})", "",
                  f"Size {r['gen_size']}, {r['candidates']} candidates; made into {', '.join(r['made'])}.", "",
                  "```", r["prompt"], "```", ""]
    with open(os.path.join(STUDY, "prompts.md"), "w") as f:
        f.write("\n".join(lines))
    print(f"{len(rows)} prompts -> art/study/prompts.md, prompts.json")


def _spec(density, mode):
    w, h = GRIDS[density]
    # At 4x, quieted like a place (regions of up to six pixels); at 2x, like most things (two).
    return dict(id="study.scene", category="place" if density == 4 else "study", size=[w, h], background="opaque",
                fit="canvas", max_colours=32 if density == 4 else 48, outline=False, margin=0, mode=mode,
                calm={"specks": 1}, palette=HINTS)


def _log(entry):
    path = os.path.join(OUT, "log.json")
    log = json.load(open(path)) if os.path.exists(path) else {}
    log[entry["out"]] = entry
    with open(path, "w") as f:
        json.dump(log, f, indent=1, sort_keys=True)
        f.write("\n")


def _sha(path):
    with open(path, "rb") as f:
        return hashlib.sha256(f.read()).hexdigest()[:16]


def cmd_scene(args):
    vid = args.variant
    if vid not in variant_ids() or vid == "F1":
        raise SystemExit(f"no variant {vid} to make (F1 is batch 9): {', '.join(v for v in variant_ids() if v != 'F1')}")
    style, _, densities = parse(vid)
    manifest = artlib.load_manifest()
    img = artlib.open_rgba(args.raw)
    os.makedirs(OUT, exist_ok=True)
    for d in densities:
        art = pixelize.process(img, _spec(d, STYLES[style]["mode"]), manifest, offset_y=args.offset_y)
        out = os.path.join(OUT, f"{vid}_{d}x.png")
        art.save(out)
        m = artlib.calm_metrics(art)
        colours = len({c[:3] for c in artlib.pixels(art)})
        _log(dict(out=os.path.basename(out), raw=os.path.basename(args.raw), raw_sha=_sha(args.raw), offset_y=args.offset_y,
                  colours=colours, **{k: round(v, 3) for k, v in m.items()}))
        print(f"{vid} at {d}x: {art.width}x{art.height}, {colours} colours, specks {m['specks']:.3f} edges "
              f"{m['edges']:.3f} busy {m['busy']:.1f} -> {os.path.relpath(out, artlib.ROOT)}")


def cmd_item(args):
    style = args.style
    if style not in STYLES:
        raise SystemExit(f"no style {style}: {', '.join(STYLES)}")
    manifest = artlib.load_manifest()
    spec = dict(artlib.asset(manifest, "item.dwarf_gold_crowns"))
    art = pixelize.process(artlib.open_rgba(args.raw), spec, manifest, mode=STYLES[style]["mode"])
    os.makedirs(OUT, exist_ok=True)
    out = os.path.join(OUT, f"item_{style}.png")
    art.save(out)
    _log(dict(out=os.path.basename(out), raw=os.path.basename(args.raw), raw_sha=_sha(args.raw)))
    print(f"item {style}: {art.width}x{art.height} -> {os.path.relpath(out, artlib.ROOT)}")


# ---- the review pictures ---------------------------------------------------------------------------------------------

def _font(size=16):
    try:
        return ImageFont.truetype(artlib.path("assets/fonts/kenney_mini.ttf"), size)
    except OSError:
        return ImageFont.load_default()


def batch9_world():
    """Batch 9 as delivered, the world at 320x180: its layers stacked as the game will stack them."""
    gen = lambda rel: artlib.open_rgba(artlib.path(f"art/generated/{rel}.png")) \
        if os.path.exists(artlib.path(f"art/generated/{rel}.png")) else None
    lantern = gen("props/lantern_stall")
    layers = {"sky": gen("sky/midday"), "place": gen("place/stall"), "frame": gen("frame/stall"),
              "shelf": gen("shelf/stall"), "counter": gen("counter/stall"), "figure": gen("figures/hob"),
              "lantern": validate._unlit(lantern) if lantern else None,
              "sheet": Image.new("RGBA", (216, 320), (0, 0, 0, 0))}
    screen = scene.compose(layers, 1, "midday", 128)
    return screen.resize((320, 180), Image.NEAREST)


def round1_world():
    path = artlib.path("assets/backgrounds/shop1.png")
    return artlib.open_rgba(path) if os.path.exists(path) else None


def mockup(world):
    """The counter screen at 1280x720: the art at whole-number scale, then what the game puts over it."""
    screen = world.convert("RGBA").resize((1280, 720), Image.NEAREST)
    chest = validate._chest("wood", "rusty")
    screen.alpha_composite(chest.resize((chest.width * 4, chest.height * 4), Image.NEAREST),
                           (scene.CHEST[0] * 4, scene.CHEST[1] * 4))
    sheet_path = artlib.path("art/generated/sheet/stall.png")
    if os.path.exists(sheet_path):
        sheet = artlib.open_rgba(sheet_path)
        x, y, w, h = scene.SHEET
        screen.alpha_composite(sheet.resize((w, h), Image.NEAREST), (x, y))
    bubble = os.path.join(UI, "bubble.png")
    if os.path.exists(bubble):
        bx, by, _, _ = scene.UI["bubble"]
        screen.alpha_composite(artlib.open_rgba(bubble), (bx, by))
    hud = os.path.join(UI, "hud.png")
    if os.path.exists(hud):
        screen.alpha_composite(artlib.open_rgba(hud), (0, 0))
        screen.alpha_composite(Image.new("RGBA", (1280, 3), (0, 0, 0, 153)), (0, 57))
    # Ticks at the edges on the shelf's row (196) and the counter's edge (476), to line a concept up by.
    tick = Image.new("RGBA", (10, 2), (255, 64, 88, 255))
    for y in (scene.SHELF_ROW * 4, scene.COUNTER_ROW * 4):
        screen.alpha_composite(tick, (0, y - 1))
        screen.alpha_composite(tick, (1270, y - 1))
    return screen


def _numbers(world):
    m = artlib.calm_metrics(world)
    return f"specks {m['specks']:.3f}  edges {m['edges']:.3f}  step {m['busy']:.1f}"


def _cell(text, w, h):
    img = Image.new("RGBA", (w, h), (30, 24, 40, 255))
    d = ImageDraw.Draw(img)
    d.rectangle((0, 0, w - 1, h - 1), outline=(59, 49, 80, 255), width=2)
    f = _font(16)
    tw = d.textlength(text, font=f)
    d.text(((w - tw) / 2, h / 2 - 8), text, font=f, fill=(155, 146, 173, 255))
    return img


def worlds():
    """{(variant, density): world image} for everything made so far, batch 9 included."""
    found = {}
    w = batch9_world()
    found[("F1", 4)] = w
    for vid in variant_ids():
        for d in (4, 2):
            p = os.path.join(OUT, f"{vid}_{d}x.png")
            if os.path.exists(p):
                found[(vid, d)] = artlib.open_rgba(p)
    return found


def overview(found, density, path):
    cw, ch, cap, head, side, gap = 640, 360, 30, 100, 210, 16
    cols, rows = list(DETAIL), list(STYLES)
    W = side + len(cols) * (cw + gap) + gap
    H = head + len(rows) * (ch + cap + gap) + gap
    img = Image.new("RGBA", (W, H), (13, 10, 19, 255))
    d = ImageDraw.Draw(img)
    big, small = _font(32), _font(16)
    grid = "320x180 at 4x (round 2's density)" if density == 4 else "640x360 at 2x (round 1's density)"
    d.text((gap, 14), f"Style study: the stall's counter, the world on a {grid}", font=big, fill=(242, 193, 78, 255))
    for c, lvl in enumerate(cols):
        d.text((side + c * (cw + gap) + gap, head - 24), f"{lvl}  {DETAIL[lvl][0]}", font=small, fill=(236, 230, 245, 255))
    for r, style in enumerate(rows):
        y = head + r * (ch + cap + gap) + gap
        d.text((gap, y + ch // 2 - 20), f"{style}  {STYLES[style]['name']}", font=small, fill=(236, 230, 245, 255))
        for c, lvl in enumerate(cols):
            vid = f"{style}{lvl}"
            x = side + c * (cw + gap) + gap
            # Pixel art is drawn on its grid: only X2 is drawn at 640x360 too (X2d). Batch 9 is at 320x180 only.
            key = (f"{vid}d", 2) if style == "X" and density == 2 else (vid, density)
            world = found.get(key)
            if world is not None:
                img.alpha_composite(mockup(world).resize((cw, ch), Image.NEAREST), (x, y))
                d.text((x, y + ch + 6), f"{key[0]}   {_numbers(world)}", font=small, fill=(155, 146, 173, 255))
                continue
            if density == 2 and vid == "F1":
                why = "F1: batch 9, made at 320x180 only"
            elif density == 2 and style == "X" and lvl != 2:
                why = f"{vid}: pixel art, drawn at 320x180 only"
            else:
                why = f"{key[0]}: not made yet"
            img.alpha_composite(_cell(why, cw, ch), (x, y))
    img.convert("RGB").save(path)
    return path


def references(found, path):
    """The two ends: round 1 (too busy) and batch 9 (a little too plain), on the same screen."""
    r1 = round1_world()
    pair = [("Round 1 (0.23): too busy", r1), ("Batch 9 (round 2's anchors): a little too plain", found[("F1", 4)])]
    img = Image.new("RGBA", (2 * 640 + 3 * 16, 70 + 360 + 64), (13, 10, 19, 255))
    d = ImageDraw.Draw(img)
    d.text((16, 14), "The two ends: the study looks for the place between them", font=_font(32), fill=(242, 193, 78, 255))
    for i, (title, world) in enumerate(pair):
        x = 16 + i * (640 + 16)
        if world is None:
            img.alpha_composite(_cell("not found", 640, 360), (x, 70))
            continue
        img.alpha_composite(mockup(world).resize((640, 360), Image.NEAREST), (x, 70))
        d.text((x, 70 + 366), title, font=_font(16), fill=(236, 230, 245, 255))
        d.text((x, 70 + 388), _numbers(world), font=_font(16), fill=(155, 146, 173, 255))
    img.convert("RGB").save(path)
    return path


def items(path):
    entries = [("F1 (batch 9)", artlib.path("art/generated/items/dwarf_gold_crowns.png"))]
    entries += [(f"{s}  {STYLES[s]['name']}", os.path.join(OUT, f"item_{s}.png")) for s in STYLES]
    cw = 300
    img = Image.new("RGBA", (len(entries) * cw + 16, 70 + 32 * 4 + 90), (13, 10, 19, 255))
    d = ImageDraw.Draw(img)
    d.text((16, 14), "The same thing in each style: 1x, 2x (as on the table) and 4x", font=_font(32), fill=(242, 193, 78, 255))
    for i, (title, p) in enumerate(entries):
        x = 16 + i * cw
        d.text((x, 62), title, font=_font(16), fill=(236, 230, 245, 255))
        if not os.path.exists(p):
            img.alpha_composite(_cell("not made yet", cw - 24, 128), (x, 90))
            continue
        thing = artlib.open_rgba(p)
        base = Image.new("RGBA", (cw - 24, 32 * 4 + 16), (78, 52, 40, 255))  # the counter's brown under it
        base.alpha_composite(thing, (8, 8 + 32 * 3))
        base.alpha_composite(thing.resize((64, 64), Image.NEAREST), (48, 8 + 64))
        base.alpha_composite(thing.resize((128, 128), Image.NEAREST), (120, 8))
        img.alpha_composite(base, (x, 90))
    img.convert("RGB").save(path)
    return path


def cmd_review(_args):
    os.makedirs(REVIEW, exist_ok=True)
    found = worlds()
    made = [overview(found, 4, os.path.join(REVIEW, "overview_4x.png")),
            overview(found, 2, os.path.join(REVIEW, "overview_2x.png")),
            references(found, os.path.join(REVIEW, "references.png")),
            items(os.path.join(REVIEW, "items.png"))]
    for (vid, d), world in sorted(found.items()):
        p = os.path.join(REVIEW, f"{vid}_{d}x.png")
        mockup(world).convert("RGB").save(p)
        made.append(p)
    for p in made:
        print(os.path.relpath(p, artlib.ROOT))


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)
    sub.add_parser("prompts").set_defaults(run=cmd_prompts)
    s = sub.add_parser("scene")
    s.add_argument("variant")
    s.add_argument("raw")
    s.add_argument("--offset-y", type=int, default=0, help="move the 16:9 crop window down (source pixels)")
    s.set_defaults(run=cmd_scene)
    i = sub.add_parser("item")
    i.add_argument("style")
    i.add_argument("raw")
    i.set_defaults(run=cmd_item)
    sub.add_parser("review").set_defaults(run=cmd_review)
    args = ap.parse_args()
    args.run(args)


if __name__ == "__main__":
    main()
