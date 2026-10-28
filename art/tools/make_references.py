"""
Round 2's reference pictures:

art/reference/scene/ (from art/tools/scene.py, with placeholder layers: the layout, not the look)
  layers.png          the shop scene's layers one by one, back to front, and what they make together
  counter_layout.png  the counter screen at 1280x720: the layers' lines (the top band, the shelf, the counter's edge,
                      the lantern hooks, the prop spots) and what the screen puts over them
  times_of_day.png    the same scene at morning, midday, evening and night: the sky changes, and the light on the rest
  flat_to_pixels.png  what to draw (a flat-colour illustration on magenta) and what pixelize.py's flat mode makes of it

art/reference/quiet/
  chest_drawn_4x.png  the chest as the game draws it (tools/custom_art.py), every finish, closed, locked and open, at
                      4x: how simple a sprite should be
  busy_and_quiet.png  0.22's code-drawn stall (quiet, but bare) beside round 1's (attractive, too busy), and round 1's
                      chest beside the drawn one. Made from round 1's art, so it's only rebuilt while that's still in
                      art/generated (the chest went in 0.25: git has it)

Run: python3 art/tools/make_references.py  (needs Pillow, and the game's tools/ folder for the chest)
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import artlib  # noqa: E402

sys.path.insert(0, artlib.path("tools"))
import custom_art  # noqa: E402

QUIET = artlib.path("art/reference/quiet")
SCENE = artlib.path("art/reference/scene")
BG = (33, 27, 45, 255)
TEXT, GOOD, BAD, NOTE = (201, 196, 212, 255), (111, 220, 140, 255), (255, 107, 107, 255), (255, 181, 46, 255)


def font():
    try:
        return ImageFont.truetype(artlib.path("assets/fonts/kenney_mini.ttf"), 16)
    except OSError:
        return ImageFont.load_default()


def closed(finish, maker=None):
    lid, body, _ = custom_art.chest(finish)
    img = Image.new("RGBA", (48, 36), (0, 0, 0, 0))
    img.alpha_composite(lid, (0, 0))
    img.alpha_composite(body, (0, 21))
    if maker:
        hasp, plate = custom_art.chest_lock(maker)
        img.alpha_composite(hasp, (0, 0))
        img.alpha_composite(plate, (0, 21))
    return img


def big(img, k=4):
    return img.resize((img.width * k, img.height * k), Image.NEAREST)


def chest_sheet():
    finishes = list(custom_art.CHEST_FINISHES)
    makers = ["rusty", "guild", "dwarven", "rusty", "guild", None, None]
    cw = 48 * 4 + 24
    out = Image.new("RGBA", (len(finishes) * cw + 24, 36 + 3 * 160 + 8), BG)
    d = ImageDraw.Draw(out)
    for i, f in enumerate(finishes):
        x = 24 + i * cw
        d.text((x, 10), f, font=font(), fill=TEXT)
        out.alpha_composite(big(closed(f)), (x, 36))
        out.alpha_composite(big(closed(f, makers[i])), (x, 36 + 160))
        out.alpha_composite(big(custom_art.chest(f)[2]), (x, 36 + 320 + 24))
    out.save(os.path.join(QUIET, "chest_drawn_4x.png"))


def busy_and_quiet():
    lid, body = artlib.path("art/generated/chest/wood_lid.png"), artlib.path("art/generated/chest/wood_body.png")
    stall = artlib.path("art/generated/backgrounds/shop1.png")
    if not (os.path.exists(lid) and os.path.exists(stall)) or Image.open(stall).width != 640:
        print("busy_and_quiet.png: round 1's art isn't in art/generated any more; kept as it is")
        return
    f = font()
    out = Image.new("RGBA", (24 + 640 + 24 + 640 + 24, 40 + 360 + 56 + 144 + 24), BG)
    d = ImageDraw.Draw(out)
    old = artlib.open_rgba(artlib.path("art/reference/current/shop_backgrounds_1x.png")).crop((0, 0, 640, 360))
    out.alpha_composite(old, (24, 40))
    out.alpha_composite(artlib.open_rgba(stall), (688, 40))
    d.text((24, 12), "0.22, drawn in code: quiet, but bare", font=f, fill=TEXT)
    d.text((688, 12), "round 1 (0.23): attractive, but too busy to live with", font=f, fill=BAD)
    y = 40 + 360 + 24
    gen = Image.new("RGBA", (96, 72), (0, 0, 0, 0))
    gen.alpha_composite(artlib.open_rgba(lid), (0, 0))
    gen.alpha_composite(artlib.open_rgba(body), (0, 42))
    out.alpha_composite(big(gen, 2), (688, y + 24))
    d.text((688, y), "round 1's chest", font=f, fill=BAD)
    out.alpha_composite(big(closed("wood", "rusty")), (920, y + 24))
    d.text((920, y), "drawn in code: the target", font=f, fill=GOOD)
    for i, line in enumerate(["Round 2 aims between the two pictures: round 1's", "colour and places, at the old art's calm: a few big,",
                              "flat shapes; low contrast behind the counter; detail", "only where it tells you what a thing is."]):
        d.text((24, y + i * 24), line, font=f, fill=TEXT)
    out.save(os.path.join(QUIET, "busy_and_quiet.png"))


# ---- the shop scene (art/tools/scene.py's layout, placeholder layers) ----------------------------------------------

def _scene():
    import scene
    return scene


def layers_sheet():
    """Each layer alone, back to front, as a small picture on a checkerboard (transparency), and the scene they make."""
    scene = _scene()
    f = font()
    names = [("sky", "the sky: the time of day (shared)"), ("place", "the place: the sky keyed out"),
             ("figure", "the figure: behind the counter"), ("shelf", "the display shelf"),
             ("frame", "the frame: top band, posts"), ("lantern", "lanterns on the posts"),
             ("counter", "the counter's top"), ("sheet", "the appraisal sheet (2x)")]
    tile_w, tile_h = 320, 180
    out = Image.new("RGBA", (24 + 4 * (tile_w + 24), 48 + 2 * (tile_h + 48) + 360 + 64), BG)
    d = ImageDraw.Draw(out)
    d.text((24, 12), "Back to front: each layer is its own picture; the game stacks them (art/tools/scene.py)", font=f, fill=TEXT)
    for i, (name, label) in enumerate(names):
        x, y = 24 + (i % 4) * (tile_w + 24), 48 + (i // 4) * (tile_h + 48)
        check = Image.new("RGBA", (tile_w, tile_h), (60, 54, 72, 255))
        cd = ImageDraw.Draw(check)
        for cy in range(0, tile_h, 10):
            for cx in range(0, tile_w, 10):
                if (cx + cy) // 10 % 2:
                    cd.rectangle((cx, cy, cx + 9, cy + 9), fill=(74, 68, 88, 255))
        layer = scene.placeholder(name, 1, "evening" if name == "lantern" else "midday")
        pos = {"figure": scene.FIGURE_AT, "shelf": scene.SHELF_AT, "counter": (0, scene.COUNTER_ROW),
               "lantern": (scene.LANTERNS[0][0] - 8, scene.LANTERNS[0][1])}.get(name, (0, 0))
        if name == "sheet":
            check.alpha_composite(layer.resize((layer.width * tile_h // layer.height, tile_h), Image.NEAREST), (tile_w // 2 - 60, 0))
        else:
            canvas = Image.new("RGBA", (scene.GRID_W, scene.GRID_H), (0, 0, 0, 0))
            canvas.alpha_composite(layer, pos)
            check.alpha_composite(canvas)
        out.alpha_composite(check, (x, y))
        d.text((x, y + tile_h + 6), f"{i + 1}. {label}", font=f, fill=TEXT)
    composed = scene.compose({}, 1, "midday", chest=closed("wood", "rusty"))
    y = 48 + 2 * (tile_h + 48)
    out.alpha_composite(composed.resize((640, 360), Image.NEAREST), (24, y))
    d.text((688, y), "Together, with the chest (drawn in code) on the counter.", font=f, fill=TEXT)
    d.text((688, y + 24), "Placeholders: this is the layout, not the look.", font=f, fill=NOTE)
    out.save(os.path.join(SCENE, "layers.png"))


def counter_layout():
    """The counter screen with every line the layers must keep, and what the screen puts over them."""
    scene = _scene()
    f = font()
    shot = scene.compose({}, 1, "midday", chest=closed("wood", "rusty"), ui=True)
    out = Image.new("RGBA", (1280, 720 + 150), BG)
    out.alpha_composite(shot, (0, 0))
    d = ImageDraw.Draw(out, "RGBA")
    s = scene.SCALE
    lines = [
        (scene.HUD_ROWS * s, "the top bar covers rows 0-13: the frame's top band runs right across, plain and dark"),
        (scene.SHELF_ROW * s, f"the shelf's top: row {scene.SHELF_ROW} (x {scene.SHELF_SPAN[0]}-{scene.SHELF_SPAN[1]})"),
        (scene.COUNTER_ROW * s, f"the counter's edge: row {scene.COUNTER_ROW}"),
    ]
    for y, label in lines:
        d.line((0, y, 1279, y), fill=(78, 163, 255, 255), width=2)
    d.text((308, scene.SHELF_ROW * s - 22), lines[1][1], font=f, fill=(20, 16, 28, 255))
    d.text((330, scene.COUNTER_ROW * s - 22), lines[2][1], font=f, fill=(255, 255, 255, 255))
    x0, y0, x1, y1 = scene.CHEST
    d.rectangle((x0 * s, y0 * s, x1 * s, y1 * s), outline=(199, 146, 255, 255), width=2)
    for x, base in scene.PROP_SPOTS:
        d.rectangle(((x - 12) * s, (base - 24) * s, (x + 12) * s, base * s), outline=(111, 220, 140, 255), width=2)
    for x, y in scene.LANTERNS:
        d.ellipse((x * s - 6, y * s - 6, x * s + 6, y * s + 6), outline=(111, 220, 140, 255), width=2)
    sx, sy, sw, sh = scene.SHEET
    d.text((sx + 24, sy + 24), "the appraisal sheet", font=f, fill=(60, 40, 30, 255))
    d.text((sx + 24, sy + 48), "216x320 at 2x: the slip in dark ink on it", font=f, fill=(60, 40, 30, 255))
    for name, (bx, by, bw, bh) in scene.UI.items():
        d.text((bx + 8, by + 6), name, font=f, fill=(255, 158, 146, 255))
    legend = [
        "The world is 320x180 at 4x. Blue: the lines the layers keep. Purple: the chest. Green: the prop spots beside it and",
        "the lantern hooks on the posts. Red: what the counter screen puts over the scene (the speech bubble over the",
        "seller's head, the notes, the board, the queue, curios). Figures stand on column 52, their feet 68 rows below the",
        "counter's edge (off screen); shorter folk stand on a box so their head and shoulders always show.",
        "Placeholders: this is the layout, not the look.",
    ]
    for i, line in enumerate(legend):
        d.text((16, 720 + 14 + i * 26), line, font=f, fill=NOTE if i == len(legend) - 1 else TEXT)
    out.save(os.path.join(SCENE, "counter_layout.png"))


def times_of_day():
    scene = _scene()
    f = font()
    out = Image.new("RGBA", (24 + 2 * (640 + 24), 24 + 2 * (360 + 40)), BG)
    d = ImageDraw.Draw(out)
    for i, time in enumerate(scene.TIMES):
        x, y = 24 + (i % 2) * (640 + 24), 24 + (i // 2) * (360 + 40)
        shot = scene.compose({}, 1, time, chest=closed("wood", "rusty"))
        out.alpha_composite(shot.resize((640, 360), Image.NEAREST), (x, y))
        tint = scene.TIMES[time]["tint"]
        d.text((x, y + 366), f"{time}: its sky; the rest lit x{tint[0]:.2f}, {tint[1]:.2f}, {tint[2]:.2f}"
                             + ("; the lanterns lit" if scene.TIMES[time]["lit"] else ""), font=f, fill=TEXT)
    out.save(os.path.join(SCENE, "times_of_day.png"))


def _flat_figure(d, k):
    """A plain figure drawn the way the brief asks: flat shapes, no outlines, no texture (to show the process)."""
    S = lambda *v: [c * k for c in v]
    skin, shade = (225, 168, 130), (189, 126, 95)
    d.ellipse(S(360, 1290, 470, 1360), fill=(70, 40, 30))
    d.ellipse(S(554, 1290, 664, 1360), fill=(70, 40, 30))
    d.rectangle(S(390, 900, 470, 1320), fill=(90, 70, 60))
    d.rectangle(S(554, 900, 634, 1320), fill=(80, 60, 52))
    d.rounded_rectangle(S(330, 470, 694, 960), 60 * k, fill=(150, 110, 70))
    d.polygon(S(512, 470, 694, 470, 694, 960, 600, 960), fill=(125, 90, 58))
    d.rounded_rectangle(S(270, 500, 340, 880), 30 * k, fill=(150, 110, 70))
    d.rounded_rectangle(S(684, 500, 754, 880), 30 * k, fill=(125, 90, 58))
    d.ellipse(S(268, 860, 342, 930), fill=skin)
    d.ellipse(S(682, 860, 756, 930), fill=shade)
    d.ellipse(S(402, 220, 622, 470), fill=skin)
    d.chord(S(402, 220, 622, 470), 300, 90, fill=shade)
    d.ellipse(S(448, 326, 480, 358), fill=(40, 30, 40))
    d.ellipse(S(544, 326, 576, 358), fill=(40, 30, 40))
    d.rounded_rectangle(S(478, 392, 548, 410), 8 * k, fill=(150, 70, 60))
    d.ellipse(S(330, 200, 694, 270), fill=(220, 190, 110))
    d.rounded_rectangle(S(420, 110, 604, 240), 30 * k, fill=(205, 172, 92))
    d.rectangle(S(420, 200, 604, 226), fill=(150, 60, 50))


def flat_to_pixels():
    """What to draw, and what the pipeline makes of it."""
    import pixelize
    f = font()
    k = 3
    big_img = Image.new("RGB", (1024 * k, 1536 * k), (255, 0, 255))
    _flat_figure(ImageDraw.Draw(big_img), k)
    source = big_img.resize((1024, 1536), Image.LANCZOS).convert("RGBA")
    manifest = artlib.load_manifest()
    spec = dict(id="figure.example", category="figure", size=[96, 160], background="transparent", fit="figure",
                height=124, outline=True, margin=0, max_colours=16, calm={"specks": 0.03})
    pixels = pixelize.process(source, spec, manifest, mode="flat")
    out = Image.new("RGBA", (24 + 427 + 48 + 384 + 48 + 384 + 24, 64 + 640 + 72), BG)
    d = ImageDraw.Draw(out)
    out.alpha_composite(source.resize((427, 640), Image.LANCZOS), (24, 64))
    out.alpha_composite(big(pixels), (24 + 427 + 48, 64))
    calm = artlib.calm_metrics(pixels)
    d.text((24, 16), "1. What to draw: flat shapes on magenta", font=f, fill=TEXT)
    d.text((24, 36), "   (1024x1536; no outlines, no texture)", font=f, fill=TEXT)
    d.text((24 + 427 + 48, 16), "2. pixelize.py --mode flat: 96x160,", font=f, fill=TEXT)
    d.text((24 + 427 + 48, 36), "   at 4x as the game shows it", font=f, fill=TEXT)
    x3 = 24 + 427 + 48 + 384 + 48
    d.text((x3, 16), "3. The same at 1x, then 2x", font=f, fill=TEXT)
    out.alpha_composite(pixels, (x3, 64))
    out.alpha_composite(pixels.resize((192, 320), Image.NEAREST), (x3 + 120, 64))
    notes = [f"specks {calm['specks']:.3f}  edges {calm['edges']:.3f}  busy {calm['busy']:.1f}: the outline is added,",
             "the eyes are 2 pixels, the shading stays two flat tones. This shows the process, not the style."]
    for i, line in enumerate(notes):
        d.text((24, 64 + 640 + 16 + i * 24), line, font=f, fill=NOTE if i else TEXT)
    out.save(os.path.join(SCENE, "flat_to_pixels.png"))


def main():
    os.makedirs(QUIET, exist_ok=True)
    os.makedirs(SCENE, exist_ok=True)
    chest_sheet()
    busy_and_quiet()
    layers_sheet()
    counter_layout()
    times_of_day()
    flat_to_pixels()
    print("references -> art/reference/quiet/, art/reference/scene/")


if __name__ == "__main__":
    main()
