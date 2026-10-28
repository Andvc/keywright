"""
The shop scene as layers (round 2): where each one sits, what it covers and how the time of day lights it. One place
for these numbers: validate.py's mock-ups, make_references.py's pictures and build_manifest.py's prompts read them here,
and the game's integration follows them.

The world is drawn on a 320x180 grid, four screen pixels to an art pixel (the 1280x720 screen); the appraisal sheet,
things and icons are UI-sized, two screen pixels to an art pixel. Back to front:

  sky       the time of day: morning, midday, evening, night (shared by every place)
  place     where the shop is: the view across the counter, the sky keyed out (one per shop tier)
  figure    whoever's at the counter, full length, standing behind it (the counter hides their legs)
  shelf     the display shelf, hung from the frame: kept things stand on it (one per tier)
  frame     the shop's own structure nearest you: the top (the top bar sits on it) and the side posts (one per tier)
  lanterns  hung on the frame's posts, lit in the evening and at night (one per tier)
  counter   the counter's top, the bottom third of the screen: the chest, your notes and the board sit on it
  props     what you've bought, standing on the counter beside the chest (the witchlight, the lodestone, the scales...)
  chest     drawn in code (tools/custom_art.py)
  sheet     the appraisal sheet behind the slip (one per tier), and then the interface

Run it to draw the layout reference with placeholder layers: python3 art/tools/scene.py out.png [tier] [time]
"""
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import artlib  # noqa: E402

GRID_W, GRID_H, SCALE = 320, 180, 4

# Fixed lines, in grid rows and columns (screen = x4).
HUD_ROWS = 14            # the top bar covers rows 0-13 (screen 0-56): the frame's top band is plain and dark there
FRAME_DEPTH = (18, 34)   # the frame's top structure ends between these rows across the middle
POST_WIDTH = 20          # the frame's side posts stay within this many columns of each edge
SHELF_ROW = 49           # the display shelf's top surface (screen y 196)
SHELF_SPAN = (75, 201)   # where kept things stand on it (screen x 300-804)
SHELF_AT = (70, 15)      # where the shelf's canvas goes (136x40; its board's top on its row 34 = row 49)
COUNTER_ROW = 119        # the counter's far edge (screen y 476); the counter fills rows 119-179
CHEST = (115, 83, 163, 119)          # the chest standing on the counter (screen 460-652, 332-476)
FIGURE_AT = (4, 28)      # where a figure's 96x160 canvas goes: centred on column 52, feet 68 rows below the counter's edge
FIGURE_FEET = 187        # the figure's feet, below the screen: the counter's edge is at waist height
FIGURE_SHOWS = 40        # at least this much of a figure shows above the counter (shorter folk stand on a box)
LANTERNS = ((8, 60), (311, 60))      # the hooks on the frame's posts, a lantern's top centre
PROP_SPOTS = ((99, COUNTER_ROW), (183, COUNTER_ROW))  # a prop's base centre, either side of the chest
SHEET = (820, 66, 432, 640)          # the appraisal sheet on screen (x, y, w, h): 216x320 art at 2x

# What the counter screen covers (screen pixels), for the mock-ups: the speech bubble (top left, over the figure's
# head), the text under the chest, the commission board, the waiting queue and Bram's tips on the counter.
UI = {
    "bubble": (12, 66, 288, 150),
    "notes": (316, 482, 484, 104),
    "board": (12, 492, 292, 102),
    "queue": (24, 612, 280, 76),
    "curios": (330, 612, 240, 70),
}

# The time of day: its sky, and the light on everything else (a multiply; the sky, the chest and the interface keep
# their own colours, the chest half as much).
TIMES = {
    "morning": dict(tint=(0.94, 0.96, 1.0), lit=False),
    "midday": dict(tint=(1.0, 1.0, 1.0), lit=False),
    "evening": dict(tint=(1.0, 0.86, 0.74), lit=True),
    "night": dict(tint=(0.5, 0.55, 0.8), lit=True),
}
TIERS = {1: "stall", 2: "thornwick", 3: "crownhold", 4: "emporium"}


def tinted(img, tint, strength=1.0):
    if tint == (1.0, 1.0, 1.0):
        return img
    t = [1 - strength * (1 - v) for v in tint]
    r, g, b, a = img.split()
    r, g, b = (ch.point(lambda v, k=k: int(v * k)) for ch, k in zip((r, g, b), t))
    return Image.merge("RGBA", (r, g, b, a))


def big(img):
    return img.resize((img.width * SCALE, img.height * SCALE), Image.NEAREST)


# ---- placeholders: flat stand-ins for whatever hasn't been delivered (the layout, not the look) -------------------

SKIES = {"morning": [(150, 176, 214), (196, 206, 222), (236, 214, 190)],
         "midday": [(98, 150, 214), (126, 170, 224), (164, 196, 232)],
         "evening": [(84, 62, 128), (176, 96, 120), (232, 146, 96)],
         "night": [(14, 18, 44), (24, 30, 64), (38, 44, 84)]}


def placeholder(layer, tier=1, time="midday", height=124):
    if layer == "sky":
        img = Image.new("RGBA", (GRID_W, GRID_H))
        d = ImageDraw.Draw(img)
        bands = SKIES[time]
        for i, c in enumerate(bands):
            d.rectangle((0, i * 40, GRID_W, GRID_H if i == len(bands) - 1 else i * 40 + 39), fill=(*c, 255))
        return img
    if layer == "place":
        img = Image.new("RGBA", (GRID_W, GRID_H), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        d.polygon([(0, 84), (60, 70), (140, 80), (230, 66), (320, 78), (320, 180), (0, 180)], fill=(92, 98, 128, 255))
        d.rectangle((0, 96, 320, 180), fill=(84, 104, 86, 255))
        d.rectangle((248, 62, 268, 84), fill=(80, 84, 112, 255))
        return img
    if layer == "frame":
        img = Image.new("RGBA", (GRID_W, GRID_H), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        d.rectangle((0, 0, 319, HUD_ROWS - 1), fill=(52, 40, 46, 255))
        d.rectangle((0, HUD_ROWS, 319, 26), fill=(120, 60, 60, 255))
        d.rectangle((0, 0, 11, 179), fill=(86, 62, 48, 255))
        d.rectangle((308, 0, 319, 179), fill=(86, 62, 48, 255))
        return img
    if layer == "shelf":
        img = Image.new("RGBA", (136, 40), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        d.rectangle((6, 0, 7, 34), fill=(150, 130, 100, 255))
        d.rectangle((128, 0, 129, 34), fill=(150, 130, 100, 255))
        d.rectangle((0, 34, 135, 39), fill=(130, 90, 60, 255))
        return img
    if layer == "counter":
        img = Image.new("RGBA", (GRID_W, GRID_H - COUNTER_ROW), (104, 72, 50, 255))
        d = ImageDraw.Draw(img)
        d.rectangle((0, 0, 319, 1), fill=(160, 116, 80, 255))
        for y in (20, 40):
            d.line((0, y, 319, y), fill=(84, 58, 42, 255))
        return img
    if layer == "figure":
        img = Image.new("RGBA", (96, 160), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        top = 159 - height
        head = max(14, height // 5)
        d.rounded_rectangle((28, top + head - 2, 67, 159), 8, fill=(70, 86, 120, 255))
        d.ellipse((48 - head // 2, top, 48 + head // 2, top + head), fill=(222, 170, 132, 255))
        return img
    if layer == "lantern":
        img = Image.new("RGBA", (16, 24), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        d.rectangle((7, 0, 8, 5), fill=(60, 50, 50, 255))
        d.rectangle((3, 6, 12, 21), fill=(60, 50, 50, 255))
        d.rectangle((5, 9, 10, 18), fill=(250, 210, 110, 255) if TIMES[time]["lit"] else (60, 66, 90, 255))
        return img
    if layer == "sheet":
        img = Image.new("RGBA", (216, 320), (110, 80, 56, 255))
        ImageDraw.Draw(img).rectangle((4, 4, 211, 315), fill=(226, 212, 178, 255))
        return img
    raise KeyError(layer)


# ---- composing ------------------------------------------------------------------------------------------------------

def compose(layers, tier=1, time="midday", figure_height=124, chest=None, ui=False, labels=False):
    """The counter screen at 1280x720 from the layers given ({layer: image or None}; missing ones are placeholders),
    lit for the time of day. With `ui`, what the counter screen puts over it, outlined; with `labels`, named."""
    tint = TIMES[time]["tint"]

    def layer(name, **kw):
        img = layers.get(name)
        return img if img is not None else placeholder(name, tier, time, **kw)

    world = Image.new("RGBA", (GRID_W, GRID_H), (0, 0, 0, 255))
    world.alpha_composite(layer("sky"))
    world.alpha_composite(tinted(layer("place"), tint))
    fig = tinted(layer("figure", height=figure_height), tint)
    lift = max(0, FIGURE_SHOWS - (figure_height - (FIGURE_FEET - COUNTER_ROW)))
    world.alpha_composite(fig, (FIGURE_AT[0], FIGURE_AT[1] - lift))
    world.alpha_composite(tinted(layer("shelf"), tint), SHELF_AT)
    world.alpha_composite(tinted(layer("frame"), tint))
    lantern = layer("lantern")
    for x, y in LANTERNS:
        world.alpha_composite(tinted(lantern, tint if not TIMES[time]["lit"] else (1, 1, 1)), (x - lantern.width // 2, y))
    world.alpha_composite(tinted(layer("counter"), tint), (0, COUNTER_ROW))
    for name, (x, base) in zip(("prop_left", "prop_right"), PROP_SPOTS):
        prop = layers.get(name)
        if prop is not None:
            world.alpha_composite(tinted(prop, tint), (x - prop.width // 2, base - prop.height))
    if chest is not None:
        world.alpha_composite(tinted(chest, tint, 0.5), (CHEST[0], CHEST[1]))
    screen = big(world)
    if TIMES[time]["lit"]:
        glow = Image.new("RGBA", screen.size, (0, 0, 0, 0))
        g = ImageDraw.Draw(glow)
        for x, y in LANTERNS:
            cx, cy = x * SCALE, (y + 14) * SCALE
            for r, a in ((72, 18), (48, 26), (28, 34)):
                g.ellipse((cx - r, cy - r, cx + r, cy + r), fill=(255, 196, 110, a))
        screen.alpha_composite(glow)
    sheet = layers.get("sheet")
    sheet = sheet if sheet is not None else placeholder("sheet", tier, time)
    x, y, w, h = SHEET
    screen.alpha_composite(sheet.resize((w, h), Image.NEAREST), (x, y))
    if ui:
        d = ImageDraw.Draw(screen, "RGBA")
        d.rectangle((0, 0, 1279, 55), fill=(13, 10, 19, 150))
        for name, (bx, by, bw, bh) in UI.items():
            d.rectangle((bx, by, bx + bw, by + bh), outline=(255, 64, 88, 220), width=3)
        if labels:
            from make_references import font
            f = font()
            for name, (bx, by, bw, bh) in UI.items():
                d.text((bx + 8, by + 6), name, font=f, fill=(255, 158, 146, 255))
    return screen


if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else "scene.png"
    tier = int(sys.argv[2]) if len(sys.argv) > 2 else 1
    time = sys.argv[3] if len(sys.argv) > 3 else "midday"
    sys.path.insert(0, artlib.path("tools"))
    import custom_art  # noqa: E402
    lid, body, _ = custom_art.chest("wood")
    chest = Image.new("RGBA", (48, 36), (0, 0, 0, 0))
    chest.alpha_composite(lid, (0, 0))
    chest.alpha_composite(body, (0, 21))
    compose({}, tier, time, chest=chest, ui=True).save(out)
    print(out)
