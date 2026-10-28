"""
Round 2's reference pictures, under art/reference/quiet/:

  chest_drawn_4x.png      the chest as the game draws it (tools/custom_art.py), every finish, closed, locked and open,
                          at 4x: how simple a sprite should be
  busy_and_quiet.png      0.22's code-drawn stall (quiet, but bare) beside round 1's (attractive, too busy), and round
                          1's chest beside the drawn one. Made from round 1's art, so it's only rebuilt while that's
                          still in art/generated (the chest went in 0.25: git has it)
  value_plan_counter.png  a counter background's values (where it's light, mid and dark) under what the screen covers,
                          at 320x180 shown at the game's 1280x720: the composition to keep, not a picture to copy

Run: python3 art/tools/make_references.py  (needs Pillow, and the game's tools/ folder for the chest)
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import artlib  # noqa: E402
import validate  # noqa: E402

sys.path.insert(0, artlib.path("tools"))
import custom_art  # noqa: E402

OUT = artlib.path("art/reference/quiet")
BG = (33, 27, 45, 255)
TEXT, GOOD, BAD = (201, 196, 212, 255), (111, 220, 140, 255), (255, 107, 107, 255)


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
    out.save(os.path.join(OUT, "chest_drawn_4x.png"))


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
    out.save(os.path.join(OUT, "busy_and_quiet.png"))


# A counter background's values, at 320x180: (box, grey). The greys are the brightness to aim for (0 black, 255
# white), between the old code-drawn stalls' (calm but bare) and round 1's (darker, and busy everywhere).
PLAN = [
    ((0, 0, 319, 118), 100),     # the back wall or the view
    ((0, 0, 319, 22), 70),       # the top: an awning, a beam
    ((62, 23, 204, 74), 150),    # the one soft light
    ((0, 23, 30, 118), 70),      # the sides: posts, walls, drapes
    ((289, 23, 319, 118), 70),
    ((110, 75, 170, 118), 84),   # behind the chest
    ((0, 119, 319, 120), 165),   # the counter's top edge
    ((0, 121, 319, 179), 60),    # the counter's front
]
# What the counter screen puts over a background, in the order of validate.py's COVERED and CALM boxes (at 640x360).
COVERED_NAMES = ["", "portrait", "speech", "the slip", "commission board", "queue", "curios"]
CALM_NAMES = ["text under the chest", "behind the chest"]
LEGEND = [
    "Greys are the brightness to aim for (0 black, 255 white): the top 70, the back wall or view 100, the one soft light",
    "150, the sides 70, behind the chest 84, the counter's top edge 165, its front 60. Each zone is nearly one flat colour.",
    "Lines: the counter's top is row 119 of 180 (screen y 476); the shelf's top is row 49 (screen y 196), x 75-201.",
    "Red: covered by the counter screen (the bar covers rows 0-13). Amber: keep calm, the chest and the text under it.",
]


def value_plan():
    plan = Image.new("RGB", (320, 180), (0, 0, 0))
    d = ImageDraw.Draw(plan)
    for box, grey in PLAN:
        d.rectangle(box, fill=(grey, grey, grey))
    d.line((75, 49, 201, 49), fill=(165, 165, 165))  # the shelf's top
    out = Image.new("RGBA", (1280, 720 + 24 + len(LEGEND) * 24 + 16), BG)
    out.paste(plan.resize((1280, 720), Image.NEAREST), (0, 0))
    d = ImageDraw.Draw(out, "RGBA")
    f = font()
    d.text((256, 100), "the one soft light: sky or a window", font=f, fill=(20, 16, 28, 255))
    d.text((308, 174), "the shelf's top", font=f, fill=(20, 16, 28, 255))
    for (x0, y0, x1, y1), label in zip(validate.COVERED, COVERED_NAMES):
        d.rectangle((x0 * 2, y0 * 2, x1 * 2 + 1, y1 * 2 + 1), outline=(255, 64, 88, 220), width=3)
        if label:
            d.text((x0 * 2 + 8, y1 * 2 - 22), label, font=f, fill=(255, 158, 146, 255))
    for (x0, y0, x1, y1), label in zip(validate.CALM, CALM_NAMES):
        d.rectangle((x0 * 2, y0 * 2, x1 * 2 + 1, y1 * 2 + 1), outline=(255, 181, 46, 230), width=3)
        d.text((x0 * 2 + 8, y0 * 2 + 6), label, font=f, fill=(255, 181, 46, 255))
    for i, line in enumerate(LEGEND):
        d.text((16, 720 + 16 + i * 24), line, font=f, fill=TEXT)
    out.save(os.path.join(OUT, "value_plan_counter.png"))


def main():
    os.makedirs(OUT, exist_ok=True)
    chest_sheet()
    busy_and_quiet()
    value_plan()
    print(f"references -> {os.path.relpath(OUT, artlib.ROOT)}/")


if __name__ == "__main__":
    main()
