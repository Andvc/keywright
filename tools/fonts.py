#!/usr/bin/env python3
"""
The game's fonts for its languages (needs fontTools: pip install fonttools).

    python3 tools/fonts.py kenney
        Adds the letters French needs that Kenney Mini (CC0) hasn't got to assets/fonts/kenney_mini.ttf: oe and OE
        (an o and an e sharing a column, as a pixel font draws them), the en dash, and the no-break spaces French puts
        before ; : ! ? and between thousands. Runs again harmlessly: it only adds what's missing.

    python3 tools/fonts.py fusion DIR
        Cuts Fusion Pixel 12px (OFL, https://github.com/TakWolf/fusion-pixel-font) down for Japanese, Simplified and
        Traditional Chinese. DIR is an unpacked release, e.g. 2026.10.25's
        fusion-pixel-font-12px-proportional-ttf-v2026.10.25.zip. Each language keeps its standard character set
        (JIS X 0208, GB2312, Big5), Latin-1, the punctuation, and every character its translation (locale/<code>.po)
        uses, so the fonts stay a few MB instead of 7 each. Run it again when a translation brings a character
        outside the standard set (tools/loc's verify names any the fonts haven't got).

Fusion Pixel is drawn on a 12px grid (100 units a pixel): the game shows it at 12, 24, 36 or 48px (UiKit.SnapFont),
never between, so its pixels stay square.
"""
import os
import re
import shutil
import sys

from fontTools import subset
from fontTools.ttLib import TTFont
from fontTools.ttLib.tables._g_l_y_f import Glyph, GlyphComponent

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FONTS = os.path.join(ROOT, "assets", "fonts")
LOCALE = os.path.join(ROOT, "locale")

# The game's code for a language, Fusion Pixel's variant for it, and the codec whose character set is its standard.
FUSION = {
    "ja": ("ja", "euc_jp"),
    "zh_CN": ("zh_hans", "gb2312"),
    "zh_TW": ("zh_tw", "big5"),
}


def kenney():
    path = os.path.join(FONTS, "kenney_mini.ttf")
    font = TTFont(path)
    cmap = font.getBestCmap()
    unit = 128  # one pixel
    added = []

    def add(code, name, components=(), advance=0):
        if code in cmap:
            return
        glyph = Glyph()
        if components:
            glyph.numberOfContours = -1
            glyph.components = []
            for base, dx in components:
                part = GlyphComponent()
                part.glyphName = cmap[ord(base)]
                part.x, part.y = dx, 0
                part.flags = 0x4  # round to the grid
                glyph.components.append(part)
        else:
            glyph.numberOfContours = 0
        font["glyf"][name] = glyph
        font["hmtx"][name] = (advance, 0)
        order = font.getGlyphOrder()
        if name not in order:
            font.setGlyphOrder(order + [name])
        for table in font["cmap"].tables:
            if table.isUnicode():
                table.cmap[code] = name
        added.append(name)

    adv = lambda ch: font["hmtx"][cmap[ord(ch)]][0]
    # o and e are four pixels wide with a pixel between: they share a column, seven wide.
    add(0x0153, "oe", [("o", 0), ("e", adv("o") - 2 * unit)], adv("o") + adv("e") - 2 * unit)
    add(0x0152, "OE", [("O", 0), ("E", adv("O") - 2 * unit)], adv("O") + adv("E") - 2 * unit)
    add(0x2013, "endash", [("-", 0), ("-", unit)], adv("-") + unit)
    add(0x00A0, "uni00A0", advance=adv(" "))
    add(0x202F, "uni202F", advance=unit)
    if not added:
        print("kenney_mini.ttf has them all already")
        return
    font["glyf"].glyphOrder = font.getGlyphOrder()
    font.save(path)
    print(f"kenney_mini.ttf: added {', '.join(added)}")


def charset(codec):
    """Every character a two-byte codec can write (the standard set its region's text keeps to)."""
    found = set()
    for first in range(0x81, 0xFF):
        for second in range(0x40, 0xFF):
            try:
                text = bytes([first, second]).decode(codec)
            except UnicodeDecodeError:
                continue
            if len(text) == 1:
                found.add(text)
    return found


def po_text(code):
    """Every character in a translation's msgstr lines."""
    path = os.path.join(LOCALE, f"{code}.po")
    if not os.path.exists(path):
        return set()
    chars = set()
    with open(path, encoding="utf-8") as f:
        inside = False
        for line in f:
            if line.startswith("msgstr"):
                inside = True
            elif line.startswith(("msgid", "#")) or not line.strip():
                inside = False
            if inside:
                for text in re.findall(r'"(.*)"', line):
                    chars.update(text)
    return chars


def fusion(release):
    keep_always = {chr(c) for c in range(0x20, 0x7F)} | {chr(c) for c in range(0xA0, 0x100)}
    keep_always |= {chr(c) for c in range(0x2000, 0x2070)}  # general punctuation
    keep_always |= {chr(c) for c in range(0x3000, 0x3040)}  # CJK punctuation
    keep_always |= {chr(c) for c in range(0xFF00, 0xFFF0)}  # full-width forms
    keep_always |= {chr(c) for c in range(0x2190, 0x2200)}  # arrows
    keep_always |= set("○●◆◇□■△▲▽▼☆★♪…※")
    for code, (variant, codec) in FUSION.items():
        source = os.path.join(release, f"fusion-pixel-12px-proportional-{variant}.ttf")
        wanted = keep_always | charset(codec) | po_text(code)
        font = TTFont(source)
        cmap = font.getBestCmap()
        have = {c for c in wanted if ord(c) in cmap}
        missing = sorted(c for c in po_text(code) if ord(c) not in cmap and not c.isspace())
        options = subset.Options()
        options.layout_features = ["*"]
        options.name_IDs = ["*"]
        options.name_languages = ["*"]
        options.notdef_outline = True
        options.glyph_names = False
        options.hinting = False
        subsetter = subset.Subsetter(options)
        subsetter.populate(unicodes=[ord(c) for c in have])
        subsetter.subset(font)
        out = os.path.join(FONTS, f"fusion_pixel_12_{code}.ttf")
        font.save(out)
        print(f"{os.path.basename(out)}: {len(have)} characters, {os.path.getsize(out) // 1024} KB"
              + (f"; the translation uses {len(missing)} it hasn't got: {''.join(missing)}" if missing else ""))
    # The licence travels with the fonts: Fusion Pixel's, and those of the fonts it draws glyphs from.
    with open(os.path.join(FONTS, "fusion-pixel-OFL.txt"), "w", encoding="utf-8") as out:
        with open(os.path.join(release, "OFL.txt"), encoding="utf-8") as f:
            out.write(f.read())
        licences = os.path.join(release, "LICENSES")
        for name in sorted(os.listdir(licences)) if os.path.isdir(licences) else []:
            for file in sorted(os.listdir(os.path.join(licences, name))):
                with open(os.path.join(licences, name, file), encoding="utf-8") as f:
                    out.write(f"\n\n---- {name} (glyphs Fusion Pixel draws from) ----\n\n{f.read()}")


if __name__ == "__main__":
    if len(sys.argv) >= 2 and sys.argv[1] == "kenney":
        kenney()
    elif len(sys.argv) >= 3 and sys.argv[1] == "fusion":
        fusion(sys.argv[2])
    else:
        print(__doc__)
        sys.exit(1)
