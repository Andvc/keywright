"""
Builds the master palette (art/palette/keywright.hex, .gpl, .png) from ramps defined in OKLCH, so every ramp steps
evenly in lightness and shifts its hue the way a painter would: shadows toward violet, light toward warm yellow.
Anchors are colours the game already uses (the UI's gold and magic violet, the six rarity colours, the outline)
and land in the palette exactly. Run: python3 art/tools/make_palette.py
"""
import math
import os

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(os.path.dirname(HERE), "palette")


def srgb_to_linear(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def linear_to_srgb(c):
    c = max(0.0, min(1.0, c))
    return 12.92 * c if c <= 0.0031308 else 1.055 * c ** (1 / 2.4) - 0.055


def hex_to_oklch(h):
    r, g, b = (srgb_to_linear(int(h[i:i + 2], 16) / 255) for i in (1, 3, 5))
    l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b
    m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b
    s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b
    l, m, s = (math.copysign(abs(v) ** (1 / 3), v) for v in (l, m, s))
    L = 0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s
    a = 1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s
    bb = 0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s
    return L, math.hypot(a, bb), math.degrees(math.atan2(bb, a)) % 360


def oklch_to_hex(L, C, H):
    a, b = C * math.cos(math.radians(H)), C * math.sin(math.radians(H))
    l = (L + 0.3963377774 * a + 0.2158037573 * b) ** 3
    m = (L - 0.1055613458 * a - 0.0638541728 * b) ** 3
    s = (L - 0.0894841775 * a - 1.2914855480 * b) ** 3
    r = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s
    g = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s
    bl = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s
    return "#" + "".join(f"{round(linear_to_srgb(v) * 255):02x}" for v in (r, g, bl))


def lerp_hue(h0, h1, t):
    d = ((h1 - h0 + 180) % 360) - 180
    return (h0 + d * t) % 360


# name, steps, (L darkest, L lightest), hue at the middle, chroma at the middle, hue drift (shadow, light),
# anchors {index: hex} that replace the computed colour at that step, and what the ramp is for.
RAMPS = [
    # Hand-picked: the UI's own darks and greys (Palette.cs), the outline, and white for glints.
    ("night", 10, (0.0, 1.0), 300, 0.0, (300, 300),
     {0: "#0b0810", 1: "#1a1420", 2: "#2b2236", 3: "#3b3150", 4: "#554b69", 5: "#766d8b", 6: "#9b92ad", 7: "#c9c4d4",
      8: "#ece6f5", 9: "#ffffff"},
     "the outline (night2), shadow, stone, iron in shade, smoke, the common rarity, near-white, white glints"),
    ("umber", 7, (0.20, 0.84), 58, 0.085, (35, 75), {}, "wood, leather, fur, rope, earth, dark skin"),
    ("skin", 5, (0.40, 0.90), 48, 0.09, (32, 62), {}, "skin from fair to brown (pick a window of three), cork, clay"),
    ("gold", 6, (0.30, 0.94), 84, 0.13, (55, 98), {4: "#f2c14e"}, "gold, brass, candlelight, straw, honey"),
    ("flame", 4, (0.45, 0.93), 62, 0.18, (32, 90), {2: "#ffb52e"}, "fire, embers, amber, the legendary rarity"),
    ("crimson", 6, (0.22, 0.80), 18, 0.17, (5, 28), {4: "#ff4058"}, "blood, royal red, demon skin, rubies, the mythic rarity"),
    ("rose", 3, (0.55, 0.86), 2, 0.11, (355, 15), {}, "cheeks, lips, silk, shell, pink potions"),
    ("leaf", 6, (0.22, 0.92), 145, 0.14, (165, 125), {4: "#62d46f"}, "foliage, moss, emerald, goblins, the uncommon rarity"),
    ("moss", 3, (0.32, 0.66), 118, 0.085, (130, 105), {}, "olive, swamp, old bronze patina, sickly light"),
    ("sea", 5, (0.25, 0.88), 182, 0.09, (200, 165), {}, "water, teal glass, Aldwater, verdigris"),
    ("azure", 6, (0.20, 0.94), 252, 0.13, (268, 228), {3: "#4ea3ff"}, "sky, sapphire, cold magic, the rare rarity, the Guild's blue"),
    ("arcane", 6, (0.20, 0.92), 302, 0.15, (290, 312), {3: "#b46bff", 4: "#c792ff"}, "magic, enchantment, the Keyhold, amethyst, the epic rarity"),
    ("steel", 4, (0.36, 0.88), 262, 0.035, (268, 250), {}, "steel, silver, pewter, glass edges, moonlight"),
    ("parchment", 3, (0.64, 0.94), 80, 0.07, (68, 92), {}, "paper, parchment, bone, sand, linen"),
]


def build():
    ramps = []
    for name, steps, (lo, hi), hue, chroma, (h_dark, h_light), anchors, use in RAMPS:
        colours = []
        for i in range(steps):
            t = i / (steps - 1)
            L = lo + (hi - lo) * t
            # Chroma peaks a little above the middle and falls off toward the ends: shadows and highlights are greyer.
            c = chroma * (1 - (2 * t - 1.1) ** 2 * 0.35)
            h = lerp_hue(h_dark, h_light, t)
            colours.append(anchors.get(i) or oklch_to_hex(L, max(0.0, c), h))
        ramps.append((name, colours, use))
    return ramps


def main():
    from PIL import Image, ImageDraw
    os.makedirs(OUT, exist_ok=True)
    ramps = build()
    flat = [c for _, cs, _ in ramps for c in cs]
    assert len(flat) == len(set(flat)), "duplicate colours"
    with open(os.path.join(OUT, "keywright.hex"), "w") as f:
        f.write("\n".join(c[1:] for c in flat) + "\n")
    with open(os.path.join(OUT, "keywright.gpl"), "w") as f:
        f.write("GIMP Palette\nName: Keywright\nColumns: 9\n#\n")
        for name, cs, _ in ramps:
            for i, c in enumerate(cs):
                r, g, b = (int(c[k:k + 2], 16) for k in (1, 3, 5))
                f.write(f"{r:3d} {g:3d} {b:3d}\t{name}{i + 1}\n")
    with open(os.path.join(OUT, "ramps.md"), "w") as f:
        f.write("| Ramp | Colours, dark to light | Use |\n|---|---|---|\n")
        for name, cs, use in ramps:
            f.write(f"| {name} | {' '.join('`' + c + '`' for c in cs)} | {use} |\n")
    # A swatch: one row per ramp, 32px chips, at 1x (the .png is also a palette reference image for prompts).
    chip, pad = 32, 4
    w = max(len(cs) for _, cs, _ in ramps) * (chip + pad) + pad
    img = Image.new("RGB", (w, len(ramps) * (chip + pad) + pad), (14, 10, 19))
    d = ImageDraw.Draw(img)
    for y, (_, cs, _) in enumerate(ramps):
        for x, c in enumerate(cs):
            x0, y0 = pad + x * (chip + pad), pad + y * (chip + pad)
            d.rectangle((x0, y0, x0 + chip - 1, y0 + chip - 1), fill=c)
    img.save(os.path.join(OUT, "keywright.png"))
    print(f"{len(flat)} colours in {len(ramps)} ramps -> {OUT}")


if __name__ == "__main__":
    main()
