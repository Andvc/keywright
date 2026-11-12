#!/usr/bin/env python3
"""Builds assets/ from Keywright's art and the Kenney font and sounds. Re-run after the art changes.

Sources:
  - The pictures: art/generated/ (the dedicated art, made to the brief in art/; see art/README.md), at their native
    sizes, scaled here where a slot wants them bigger. Anything missing there falls back to the placeholder drawn in
    code by tools/custom_art.py (things, portraits, chests, curios, backdrops, effects) or below (UI frames, icons),
    scaled to the same sizes.
  - Kenney Font Package, Impact Sounds, RPG Audio, Casino Audio, Interface Sounds (CC0, see CREDITS.md)
    https://kenney.nl (mirror: github.com/ETdoFresh/kenney.nl)

Usage: python3 tools/import_art.py [--cache DIR] [--density-only] [--density-check] [--chests-only]
Requires Pillow. Kenney's packs are cloned (partially) into the cache on first run.
"""
import argparse
import hashlib
import json
import os
import shutil
import subprocess
import sys

from PIL import Image

import custom_art

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ASSETS = os.path.join(ROOT, "assets")
GENERATED = os.path.join(ROOT, "art", "generated")

# A portrait's face, for the small places (the waiting queue, the ledger's rows): the 38x38 box every portrait keeps
# its face in (art/ART_DIRECTION.md's composition template).
FACE = (19, 14, 57, 52)


def gen(*parts):
    """A picture from art/generated, or None if it wasn't delivered."""
    path = os.path.join(GENERATED, *parts)
    return Image.open(path).convert("RGBA") if os.path.exists(path) else None


def scaled(img, k):
    return img.resize((img.width * k, img.height * k), Image.NEAREST)


def backdrop_size(img):
    """A backdrop at the game's 640x360: round 2 draws them at 320x180 (four screen pixels to an art pixel), doubled here;
    round 1's were drawn at 640x360."""
    return scaled(img, 640 // img.width) if img is not None and img.width < 640 else img


def trimmed(img):
    return img.crop(img.getbbox())

KENNEY = ("https://github.com/ETdoFresh/kenney.nl", [
    "kenney_fontpackage", "kenney_impactsounds", "kenney_rpgaudio", "kenney_casinoaudio", "kenney_interfacesounds",
])

# Curios (Curios.All in scripts/Core/Curios.cs) are drawn 28px and shown at 2x in 56px slots.
CURIO_SLOT = 56

# Group -> Kenney source files (pack folder, file stem).
def _variants(pack, stem, indices, digits=3, sep="_"):
    return [(pack, f"{stem}{sep}{i:0{digits}d}") for i in indices]

IMPACT, RPG, CASINO, UI = "kenney_impactsounds", "kenney_rpgaudio", "kenney_casinoaudio", "kenney_interfacesounds"

SOUNDS = {
    # Kinds of things: the sound you'd expect from each in a shaken chest. Bottles' slosh is synthesized in Sfx.cs.
    "jingle": [(RPG, "handleCoins"), (RPG, "handleCoins2")],
    "clank": _variants(IMPACT, "impactTin_medium", range(5)),
    "flump": [(RPG, f"cloth{i}") for i in range(1, 5)],
    "rustle": [(CASINO, "cardShove1"), (CASINO, "cardShove2"), (CASINO, "cardShove4")],
    "tinkle": _variants(UI, "glass", [1, 2, 3]),

    # Alternatives for each kind (scripts/Audio/SoundSets.cs), to audition and choose in Settings > Item sounds.
    "jingle_chips": [(CASINO, f"chipsCollide{i}") for i in range(1, 5)],
    "jingle_handle": [(CASINO, f"chipsHandle{i}") for i in range(1, 7)],
    "clank_light": _variants(IMPACT, "impactMetal_light", range(5)),
    "clank_pot": [(RPG, f"metalPot{i}") for i in range(1, 4)],
    "clank_plate": _variants(IMPACT, "impactPlate_medium", range(5)),
    "flump_soft": _variants(IMPACT, "impactSoft_medium", range(5)),
    "flump_heavy": _variants(IMPACT, "impactSoft_heavy", range(5)),
    "flump_leather": [(RPG, "dropLeather"), (RPG, "handleSmallLeather"), (RPG, "handleSmallLeather2"), (RPG, "clothBelt"),
                      (RPG, "clothBelt2")],
    "slosh_clink": _variants(IMPACT, "impactGlass_light", range(5)),
    "rustle_book": [(RPG, f"bookFlip{i}") for i in range(1, 4)],
    "rustle_slide": [(CASINO, f"cardSlide{i}") for i in range(1, 9)],
    "rustle_shuffle": [(CASINO, "cardShuffle"), (CASINO, "cardFan1"), (CASINO, "cardFan2")],
    "tinkle_glass": _variants(UI, "glass", [4, 5, 6]),
    "tinkle_plate": _variants(IMPACT, "impactPlate_light", range(5)),

    # Handling the chest and the shop
    "land": _variants(IMPACT, "impactPlank_medium", range(5)),
    "clack": _variants(IMPACT, "impactMetal_heavy", range(3)),
    "pay": [(CASINO, f"chipsStack{i}") for i in range(1, 4)],
    "door_open": [(RPG, "doorOpen_1"), (RPG, "doorOpen_2")],
    "door_close": [(RPG, "doorClose_1"), (RPG, "doorClose_2")],
    "latch": [(RPG, "metalLatch"), (RPG, "metalClick")],
    "book": [(RPG, "bookOpen")],
}

UI_SCALE = 3


def hex_rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def fetch(url, paths, cache):
    dest = os.path.join(cache, url.rstrip("/").split("/")[-1])
    if not os.path.isdir(dest):
        subprocess.run(["git", "clone", "-q", "--filter=blob:none", "--no-checkout", "--depth", "1", url, dest], check=True)
    subprocess.run(["git", "-C", dest, "checkout", "HEAD", "--", *paths], check=True)
    return dest


def walk(folder):
    return [os.path.join(here, f) for here, _, files in os.walk(folder) for f in files]


def save(image, *parts):
    path = os.path.join(ASSETS, *parts)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    image.save(path)
    return path


def build_chests():
    """The chest's parts (lid, body, open) in each finish, and the lock makers' overlays, at the 3px density (0.32):
    drawn in code a pixel a cell (custom_art.chest3: 128 cells wide), each cell made 3x3 pixels, the 1440p size, and
    registered with the layout size the game places them at (96 wide, as since 0.25; ChestView shows them at 2x), so
    nothing moves. 0.23's generated chests (art/generated/chest) were retired in 0.25: too busy for the thing the player
    looks at most; 0.25's 48-pixel chest (custom_art.chest) is now only the window icon's fallback."""
    drawn = {}
    for finish in custom_art.CHEST3_FINISHES:
        for part, img in zip(("lid", "body", "open"), custom_art.chest3(finish)):
            drawn.update(save_drawn(img, f"chest.{finish}.{part}", "sprites", "chest", f"{finish}_{part}.png"))
    for maker in custom_art.CHEST3_LOCKS:
        for part, img in zip(("lid", "body"), custom_art.chest3_lock(maker)):
            drawn.update(save_drawn(img, f"chest.lock_{maker}.{part}", "sprites", "chest", f"lock_{maker}_{part}.png"))
    register_drawn(drawn, prefix="drawn.chest.")


# Art drawn in code at the 3px density: a cell is 3x3 pixels at 1440p, where the layout's pixel is 2 (shown at
# DRAWN_SCALE layout pixels a picture pixel, as the chest is).
DRAWN_PITCH, DRAWN_SCALE = 3, 2


def build_drawn_icons():
    """Upgrade icons drawn in code at the 3px density (custom_art.ICONS3: the Tea Kettle and the Listening Screen, which
    came after the 3px art was made): 32 cells square, each cell 3x3 pixels at 1440p, placed as 48 like the rest."""
    drawn = {}
    for key, draw in custom_art.ICONS3.items():
        cells = draw()
        physical = cells.resize((cells.width * DRAWN_PITCH, cells.height * DRAWN_PITCH), Image.NEAREST)
        path = save(physical, "ui", "icons", f"{key}.png")
        rel = os.path.relpath(path, ROOT).replace(os.sep, "/")
        with open(path, "rb") as stream:
            digest = hashlib.sha256(stream.read()).hexdigest()
        drawn[f"res://{rel}"] = {"asset_id": f"drawn.icon.upgrade.{key}", "source": "tools/custom_art.py", "drawn": True,
                                 "logical_size": [physical.width // 2, physical.height // 2], "physical_size": list(physical.size),
                                 "reference_scale": 1, "pixel_pitch": DRAWN_PITCH, "sha256": digest}
    # Curios drawn in code (custom_art.CURIOS3: the ear's, 0.36): 28 cells square, four pixels a cell, placed as 56.
    for key, draw in custom_art.CURIOS3.items():
        cells = draw()
        physical = cells.resize((cells.width * 4, cells.height * 4), Image.NEAREST)
        path = save(physical, "ui", "curios", f"{key}.png")
        rel = os.path.relpath(path, ROOT).replace(os.sep, "/")
        with open(path, "rb") as stream:
            digest = hashlib.sha256(stream.read()).hexdigest()
        drawn[f"res://{rel}"] = {"asset_id": f"drawn.icon.curio.{key}", "source": "tools/custom_art.py", "drawn": True,
                                 "logical_size": [physical.width // 2, physical.height // 2], "physical_size": list(physical.size),
                                 "reference_scale": 1, "pixel_pitch": 4, "sha256": digest}
    register_drawn(drawn, prefix="drawn.icon.")


def save_drawn(cells, asset, *parts):
    """Saves a picture drawn a pixel a cell at its 1440p size, and returns its art_density.json entry: the size the
    layout places it at (each cell is 3/4 of a layout pixel at 2x), and the size of the texture itself."""
    physical = cells.resize((cells.width * DRAWN_PITCH, cells.height * DRAWN_PITCH), Image.NEAREST)
    per = 2 * DRAWN_SCALE
    if physical.width % per or physical.height % per:
        raise ValueError(f"{asset}: {cells.size} cells don't make a whole layout size")
    path = save(physical, *parts)
    rel = os.path.relpath(path, ROOT).replace(os.sep, "/")
    with open(path, "rb") as stream:
        digest = hashlib.sha256(stream.read()).hexdigest()
    return {f"res://{rel}": {"asset_id": f"drawn.{asset}", "source": "tools/custom_art.py", "drawn": True,
                             "logical_size": [physical.width // per, physical.height // per],
                             "physical_size": list(physical.size), "reference_scale": DRAWN_SCALE,
                             "pixel_pitch": DRAWN_PITCH, "sha256": digest}}


def register_drawn(entries, prefix):
    """Puts drawn pictures' sizes in assets/art_density.json (replacing every earlier entry under the same prefix);
    the density import keeps them when it rewrites the file."""
    path = os.path.join(ASSETS, "art_density.json")
    with open(path, encoding="utf-8") as stream:
        metadata = json.load(stream)
    textures = {k: v for k, v in metadata["textures"].items() if not v.get("asset_id", "").startswith(prefix)}
    textures.update(entries)
    metadata["textures"] = dict(sorted(textures.items()))
    with open(path, "w", encoding="utf-8") as stream:
        stream.write(json.dumps(metadata, indent=2, ensure_ascii=False) + "\n")
    print(f"Registered {len(entries)} drawn textures under {prefix}")


def build_brand():
    """The logo (title screen), the window icon (the purple chest with a key) and the .exe's, from the icon drawn for
    each size (16 to 64; 128 and 256 are the 64 scaled up)."""
    logo = gen("brand", "logo.png")
    if logo:
        save(logo, "ui", "logo.png")
    sizes = [gen("brand", f"app_icon_{n}.png") for n in (16, 32, 48, 64)]
    if all(sizes):
        big = [scaled(sizes[-1], 2), scaled(sizes[-1], 4)]
        os.makedirs(os.path.join(ASSETS, "ui"), exist_ok=True)
        big[-1].save(os.path.join(ASSETS, "ui", "icon.ico"), format="ICO", sizes=[(n, n) for n in (16, 32, 48, 64, 128, 256)],
                     append_images=sizes + big[:1])
    icon = gen("brand", "app_icon_64.png")
    if icon is None:
        lid, body, _ = custom_art.chest("purple")
        closed = Image.new("RGBA", (48, 36), (0, 0, 0, 0))
        closed.alpha_composite(lid, (0, 0))
        closed.alpha_composite(body, (0, 21))
        icon = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        icon.alpha_composite(closed, (8, 14))
    save(icon, "ui", "icon.png")


def build_ui():
    """Pixel-art 9-slice frames: 12x12 art, shown at 2x (UiKit.Px), with 4-pixel slice margins. The panel's colours are
    its own; the frame and the buttons are greys the game tints."""
    pieces = ("panel", "frame", "button", "button_pressed", "slip_paper", "bubble", "bubble_tail")
    if gen("ui", "panel.png"):
        for name in pieces:
            art = gen("ui", f"{name}.png")
            if art:
                save(scaled(art, 2), "ui", f"{name}.png")
        return
    k0, k1, k2, k3, k4 = (11, 8, 16, 255), (26, 20, 32, 255), (43, 34, 54, 255), (59, 49, 80, 255), (194, 144, 38, 255)
    clear = (0, 0, 0, 0)

    def frame(fill, top, bottom, border, gold=None, size=12):
        img = Image.new("RGBA", (size, size), fill)
        px = img.load()
        for i in range(size):
            px[i, 0] = px[i, size - 1] = px[0, i] = px[size - 1, i] = border
        for i in range(1, size - 1):
            px[i, 1], px[1, i] = top, top
            px[i, size - 2], px[size - 2, i] = bottom, bottom
        for x, y in [(0, 0), (size - 1, 0), (0, size - 1), (size - 1, size - 1)]:
            px[x, y] = clear
        if gold:
            for x, y in [(2, 2), (size - 3, 2), (2, size - 3), (size - 3, size - 3)]:
                px[x, y] = gold
        return scaled(img, 2)

    save(frame(k2, k3, k1, k0, gold=k4), "ui", "panel.png")
    line = Image.new("RGBA", (12, 12), clear)
    px = line.load()
    for i in range(2, 10):
        px[i, 1] = px[i, 10] = px[1, i] = px[10, i] = (255, 255, 255, 255)
    save(scaled(line, 2), "ui", "frame.png")
    grey = lambda v: (v, v, v, 255)
    save(frame(grey(190), grey(235), grey(130), grey(2)), "ui", "button.png")
    save(frame(grey(130), grey(130), grey(235), grey(2)), "ui", "button_pressed.png")


# Tool and upgrade icons, 16x16 pixel art drawn here (no CC0 pack has these), pre-scaled like the UI frames.
ICON_PALETTE = {
    "k": (20, 16, 26), "g": (242, 193, 78), "G": (176, 128, 44), "w": (240, 232, 214), "s": (190, 196, 210),
    "S": (110, 116, 134), "r": (200, 60, 60), "b": (140, 90, 46), "B": (92, 56, 28), "c": (110, 200, 230),
    "p": (160, 110, 230), "R": (255, 70, 90), "m": (150, 24, 44),
}
ICONS = {
    # The restorers' upgrades: a workbench with a hammer, and the glint of a thing made better.
    "bench": [
        "................",
        "...........k....",
        "..........kgk...",
        "...........k....",
        "..kkkkkk........",
        "..kSSSSk..k.....",
        "..kkkkkk.kwk....",
        "....kbk...k.....",
        "....kbk.........",
        ".kkkkkkkkkkkkkk.",
        ".kbbbbbbbbbbbbk.",
        ".kBBBBBBBBBBBBk.",
        ".kkkkkkkkkkkkkk.",
        "..kBk......kBk..",
        "..kBk......kBk..",
        "..kkk......kkk..",
    ],
    "sign": [
        "................",
        ".kkkkkkkkkkkkkk.",
        ".kbbbbbbbbbbbbk.",
        ".kkkkkkkkkkkkkk.",
        "...k........k...",
        "...k........k...",
        ".kkkkkkkkkkkkkk.",
        ".kggggggggggggk.",
        ".kgkkggkkgkkggk.",
        ".kgkggkgkgkgkgk.",
        ".kgkkggkkgkkggk.",
        ".kggggggggggggk.",
        ".kkkkkkkkkkkkkk.",
        "................",
        "................",
        "................",
    ],
    "shop": [
        "................",
        ".......kk.......",
        "......krrk......",
        ".....krrrrk.....",
        "....krrrrrrk....",
        "...krrrrrrrrk...",
        "..krrrrrrrrrrk..",
        ".kkkkkkkkkkkkkk.",
        "..kbbbbbbbbbbk..",
        "..kbkkbbbbkkbk..",
        "..kbkcbbbbkcbk..",
        "..kbkkbkkbkkbk..",
        "..kbbbbkBkbbbk..",
        "..kbbbbkBkbbbk..",
        "..kkkkkkkkkkkk..",
        "................",
    ],
    "lamp": [
        "................",
        ".......kk.......",
        "......kSSk......",
        ".....kkkkkk.....",
        "....kSkppkSk....",
        "....kkpwwpkk....",
        "....kSpwwpSk....",
        "....kkpppkkk....",
        "....kSkppkSk....",
        ".....kkkkkk.....",
        "......kSSk......",
        "......kkkk......",
        "...p........p...",
        ".....p....p.....",
        "................",
        "................",
    ],
    "cabinet": [
        "................",
        "..kkkkkkkkkkkk..",
        "..kBbbbbbbbbBk..",
        "..kBkkkkkkkkBk..",
        "..kBkcwccgckBk..",
        "..kBkccccgckBk..",
        "..kBkkkkkkkkBk..",
        "..kBkcrccpckBk..",
        "..kBkcrccpckBk..",
        "..kBkkkkkkkkBk..",
        "..kBkcccgcckBk..",
        "..kBkccgggckBk..",
        "..kBkkkkkkkkBk..",
        "..kBbbbbbbbbBk..",
        "..kkkkkkkkkkkk..",
        "..kk........kk..",
    ],
    "picks": [
        "................",
        "...........kk...",
        "..........ksk...",
        ".........ksk....",
        "........ksk.....",
        ".......ksk......",
        "......ksk..kk...",
        ".....ksk..ksk...",
        "...kkkk..ksk....",
        "..kgggk.ksk.....",
        "..kgGgkksk......",
        "..kgggkkk.......",
        "...kkkk.........",
        "................",
        "................",
        "................",
    ],
    "magnet": [
        "................",
        "...kkkkkkkkkk...",
        "..krrrrrrrrrrk..",
        ".krrrrrrrrrrrrk.",
        ".krrrkkkkkkrrrk.",
        ".krrrk....krrrk.",
        ".krrrk....krrrk.",
        ".krrrk....krrrk.",
        ".krrrk....krrrk.",
        ".kSSSk....kSSSk.",
        ".ksssk....ksssk.",
        ".ksssk....ksssk.",
        ".kkkkk....kkkkk.",
        "................",
        "................",
        "................",
    ],
    "charter": [
        "................",
        "..kkkkkkkkkkkk..",
        ".kwwwwwwwwwwwwk.",
        "..kwsssssssswk..",
        "..kwwwwwwwwwwk..",
        "..kwssssssswwk..",
        "..kwwwwwwwwwwk..",
        "..kwsssssswwwk..",
        "..kwwwwwwwrrwk..",
        "..kwwwwwwrrrrk..",
        "..kwwwwwwwrrwk..",
        ".kwwwwwwwwrwwwk.",
        "..kkkkkkkkkkkk..",
        "................",
        "................",
        "................",
    ],
    "seal": [
        "................",
        "................",
        ".....kkkkkk.....",
        "....krrrrrrk....",
        "...krrgrrgrrk...",
        "..krrrgggggrrk..",
        "..krrrgGGGgrrk..",
        "..krrrrrrrrrrk..",
        "..krrrrrrrrrrk..",
        "...krrrrrrrrk...",
        "....krrrrrrk....",
        ".....kkkkkk.....",
        "....kr....rk....",
        "...kr......rk...",
        "................",
        "................",
    ],
    "case": [
        "................",
        "..kkkkkkkkkkkk..",
        "..kBBBBBBBBBBk..",
        "..kBkkkkkkkkBk..",
        "..kBkcccccckBk..",
        "..kBkcpccgckBk..",
        "..kBkcccccckBk..",
        "..kBkkkkkkkkBk..",
        "..kBkcccccckBk..",
        "..kBkcgccrckBk..",
        "..kBkcccccckBk..",
        "..kBkkkkkkkkBk..",
        "..kBBBBBgBBBBk..",
        "..kkkkkkkkkkkk..",
        "................",
        "................",
    ],
    "night": [
        "................",
        "......kkkk......",
        "....kkggggk.....",
        "...kgggkkk......",
        "..kgggk.........",
        "..kggk......k...",
        ".kgggk.....kwk..",
        ".kgggk......k...",
        ".kgggk..........",
        ".kGggk..........",
        "..kGggk.......k.",
        "..kGGggk.....kgk",
        "...kGGGgkkkkkGk.",
        "....kkGGGGGGkk..",
        "......kkkkkk....",
        "................",
    ],
    "patrons": [
        "................",
        "..kkkkkkkkkkkk..",
        ".kBBBBBBBBBBBBk.",
        ".kBkwwwwwwwwkBk.",
        ".kBkwsssssswkBk.",
        ".kBkwwwwwwwwkBk.",
        ".kBkwssssswwkBk.",
        ".kBkwwwwwwwwkBk.",
        ".kBkwsssswwwkBk.",
        ".kBkwwwwwwwwkBk.",
        ".kBkwwwwwrrwkBk.",
        ".kBkwwwwwrrwkBk.",
        ".kBkkkkkkkkkkBk.",
        ".kBBBBBBBBBBBBk.",
        "..kkkkkkkkkkkk..",
        "................",
    ],
    # A buyers' trade: brass scales, a crier's handbell, an auction gavel, a royal warrant with its seal.
    "scales": [
        "................",
        ".......kk.......",
        "......kggk......",
        "..kkkkkggkkkkk..",
        "..kgggggggggggk.",
        "..kkkkkggkkkkk..",
        "..k....gg....k..",
        ".k.k...gg...k.k.",
        "k...k..gg..k...k",
        "kkkkk..gg..kkkkk",
        "kGGGk..gg..kGGGk",
        ".kkk...gg...kkk.",
        ".......gg.......",
        ".....kkggkk.....",
        "....kGGGGGGk....",
        "....kkkkkkkk....",
    ],
    "crier": [
        "................",
        ".......kk.......",
        "......kbbk......",
        "......kbbk......",
        ".......kk.......",
        "......kggk......",
        ".....kggggk.....",
        "....kggwgggk....",
        "....kgwggggk....",
        "...kgggggggGk...",
        "...kggggggGGk...",
        "..kGGGGGGGGGGk..",
        "..kkkkkkkkkkkk..",
        ".......kk.......",
        ".......kk.......",
        "................",
    ],
    "block": [
        "................",
        ".kkkkkkk........",
        "kbbbbbbbk.......",
        "kbBBBBBbk.......",
        "kbbbbbbbk.......",
        ".kkkkkkkk.......",
        "....kbk.........",
        ".....kbk........",
        "......kbk.......",
        ".......kbk......",
        "........kbk.....",
        ".........kk.....",
        "..kkkkkkkkkkkk..",
        ".kBBBBBBBBBBBBk.",
        ".kkkkkkkkkkkkkk.",
        "................",
    ],
    "warrant": [
        "................",
        "..kkkkkkkkkkk...",
        ".kwwwwwwwwwwwk..",
        ".kwkkkkkkkkwwk..",
        ".kwwwwwwwwwwwk..",
        ".kwkkkkkkwwwwk..",
        ".kwwwwwwwwwwwk..",
        ".kwkkkkkkkwwwk..",
        ".kwwwwwwwwwwwk..",
        ".kwwwwwwkkkwwk..",
        ".kwwwwwkrrrkwk..",
        ".kwwwwwkrrrkwk..",
        ".kkkkkkkkrkkkk..",
        "........krk.....",
        ".......kr.rk....",
        "................",
    ],
    # The Bookkeeping Engine: a brass clockwork with a gear on top, a slip coming out of it.
    "engine": [
        "................",
        "....kkk.........",
        "...kgggk..kkkk..",
        "..kgkkkgk.kwwk..",
        "..kgk.kgk.kwsk..",
        "..kgkkkgk.kwwk..",
        "...kgggk..kwsk..",
        ".kkkkkkkkkkwwkk.",
        ".kGGGGGGGGGGGGk.",
        ".kGgggggggggGGk.",
        ".kGgkkgkkgkkgGk.",
        ".kGgggggggggGGk.",
        ".kGGGGGGGGGGGGk.",
        ".kkkkkkkkkkkkkk.",
        "..kk........kk..",
        "................",
    ],
    "crown": [
        "................",
        "................",
        "..k....kk....k..",
        ".kgk..kggk..kgk.",
        ".kggk.kggk.kggk.",
        ".kgggkggggkgggk.",
        ".kggggggggggggk.",
        ".kgrggggggggrgk.",
        ".kggggrggrggggk.",
        ".kGGGGGGGGGGGGk.",
        ".kkkkkkkkkkkkkk.",
        "................",
        "................",
        "................",
        "................",
        "................",
    ],
}


# One small icon per kind of thing (matching Category in scripts/Core/Items.cs), for the slip and the ledger rows,
# where item sprites are too big. Saved at 2x under assets/ui/kinds/.
KIND_ICONS = {
    # A Heartsong: a red heart with a note in it.
    "heartsong": [
        "................",
        "................",
        "..kkkk....kkkk..",
        ".kRRRRk..kRRRRk.",
        "kRRwRRRkkRRRRRRk",
        "kRwRRRRRRRRkkRRk",
        "kRRRRRRRRRRkwkRk",
        "kRRRRRRRRRRkRkRk",
        ".kRRRRRRkkkkRkk.",
        "..kRRRRkwwkRRk..",
        "...kRRRkwwkRk...",
        "....kmRRkkRmk...",
        ".....kmRRRmk....",
        "......kmmmk.....",
        ".......kkk......",
        "................",
    ],
    "coins": [
        "................",
        "................",
        "................",
        ".....kkkkkk.....",
        "....kggggggk....",
        "....kGGGGGGk....",
        "...kkkkkkkkkk...",
        "..kggggggggggk..",
        "..kGGGGGGGGGGk..",
        "..kkkkkkkkkkkk..",
        ".kggggggggggggk.",
        ".kGGGGGGGGGGGGk.",
        ".kggggggggggggk.",
        ".kGGGGGGGGGGGGk.",
        "..kkkkkkkkkkkk..",
        "................",
    ],
    "arms": [
        "................",
        ".............kk.",
        "............kwk.",
        "...........kssk.",
        "..........kssk..",
        ".........kssk...",
        "........kssk....",
        "...kk..kssk.....",
        "...kGkkssk......",
        "....kGssk.......",
        "....kkGk........",
        "...kbkkGk.......",
        "..kbk..kk.......",
        ".kbk............",
        ".kk.............",
        "................",
    ],
    "garb": [
        "................",
        "......kkkk......",
        ".....kgggGk.....",
        "....krrrrrrk....",
        "...krrrrrrrrk...",
        "...krrkrrkrrk...",
        "..krrrkrrkrrrk..",
        "..krrrkrrkrrrk..",
        "..krrrkrrkrrrk..",
        ".krrrrkrrkrrrrk.",
        ".krrrrkrrkrrrrk.",
        ".krrrrkrrkrrrrk.",
        ".kkkkkkkkkkkkkk.",
        "................",
        "................",
        "................",
    ],
    "potions": [
        "................",
        "......kkkk......",
        "......kbbk......",
        ".......kk.......",
        "......kwck......",
        "......kwck......",
        ".....kccccck....",
        "....kccccccck...",
        "....kwcccccck...",
        "....kwcccccck...",
        "....kccccccck...",
        "....kccccccck...",
        "....kccccccck...",
        ".....kkkkkkk....",
        "................",
        "................",
    ],
    "scrolls": [
        "................",
        "...kkkkkkkkkk...",
        "..kbbbbbbbbbbk..",
        "...kkkkkkkkkk...",
        "....kwwwwwwk....",
        "....kwkkkkwk....",
        "....kwwwwwwk....",
        "....kwkkkwwk....",
        "....kwwwwwwk....",
        "....kwkkkkwk....",
        "....kwwwwwwk....",
        "...kkkkkkkkkk...",
        "..kbbbbbbbbbbk..",
        "...kkkkkkkkkk...",
        "................",
        "................",
    ],
    "hidden": [
        "................",
        "................",
        ".kkkkkkkkkkkkkk.",
        ".kbbbbbbbbbbbbk.",
        ".kb..........bk.",
        ".kb..........bk.",
        ".kb..........bk.",
        ".kbBBBBBBBBBBbk.",
        ".kb..gggg....bk.",
        ".kb.gGGGGg...bk.",
        ".kbBBBBBBBBBBbk.",
        ".kkkkkkkkkkkkkk.",
        "................",
        "................",
        "................",
        "................",
    ],
    "gems": [
        "................",
        "................",
        ".....kkkkkk.....",
        "....kcwcccck....",
        "...kcwcccccck...",
        "..kkkkkkkkkkkk..",
        "..kcwcccccccck..",
        "...kcwcccccck...",
        "....kcwcccck....",
        ".....kcccck.....",
        "......kcck......",
        ".......kk.......",
        "................",
        "................",
        "................",
        "................",
    ],
}


# The shop at each tier, as one 640x360 picture shown at 2x behind the counter (and, dimmed, behind the other screens):
# a backdrop from custom_art (the place outside or the wall behind), the architecture drawn here, a few props, and the
# counter. The
# counter's top edge is at y 238 (476 on screen); the HUD covers the top 28 rows; on the counter screen the appraisal
# panel covers x 410 and up. The display shelf (from the village shop) is at y 98, where the game stands what you keep.
SHOP_W, SHOP_H, COUNTER_Y, SHELF_Y = 640, 360, 238, 98


def build_shops():
    from PIL import ImageDraw
    props = custom_art.props()

    def backdrop(img, shade=1.0):
        if shade != 1.0:
            img = Image.eval(img.convert("RGB"), lambda v: int(v * shade)).convert("RGBA")
        return img

    def prop(img, name, x, bottom):
        p = props[name]
        img.alpha_composite(p, (x, bottom - p.height))

    def shadow(img, box, alpha):
        layer = Image.new("RGBA", img.size, (0, 0, 0, 0))
        ImageDraw.Draw(layer).rectangle(box, fill=(0, 0, 0, alpha))
        img.alpha_composite(layer)

    def glow(img, cx, cy, radius, color):
        # Hard-edged rings, brighter toward the middle: pixel-art light, not a smooth gradient.
        layer = Image.new("RGBA", img.size, (0, 0, 0, 0))
        d = ImageDraw.Draw(layer)
        for i in range(4, 0, -1):
            r = radius * i // 4
            d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=(*color, 22))
        img.alpha_composite(layer)

    def lamp(img, d, x, top, length):
        d.line((x, top, x, top + length), fill=(40, 30, 24, 255))
        d.rectangle((x - 4, top + length, x + 4, top + length + 10), fill=(58, 40, 26, 255), outline=(30, 20, 14, 255))
        d.rectangle((x - 2, top + length + 2, x + 2, top + length + 8), fill=(255, 214, 120, 255))
        glow(img, x, top + length + 5, 34, (255, 190, 90))

    def shelf(d, top, bottom_color, top_color, edge_color, bracket):
        d.rectangle((150, top, 402, top + 5), fill=top_color)
        d.line((150, top, 402, top), fill=edge_color)
        d.line((150, top + 5, 402, top + 5), fill=bottom_color)
        for x in (158, 388):
            d.polygon([(x, top + 6), (x + 8, top + 6), (x, top + 14)], fill=bracket)

    def planks(d, y0, y1, color, seam, every, x0=0, x1=SHOP_W):
        d.rectangle((x0, y0, x1, y1), fill=color)
        for x in range(x0 + every // 2, x1, every):
            d.line((x, y0, x, y1), fill=seam)

    shops = {}

    # The roadside stall: a trestle table under a striped awning at the Brindle crossroads.
    img = backdrop(custom_art.crossroads())
    d = ImageDraw.Draw(img)
    shadow(img, (0, 58, SHOP_W, 74), 70)
    d = ImageDraw.Draw(img)
    for i, x in enumerate(range(0, SHOP_W, 16)):
        colour = (184, 74, 60, 255) if i % 2 == 0 else (239, 225, 193, 255)
        d.rectangle((x, 26, x + 15, 58), fill=colour)
        d.ellipse((x, 52, x + 15, 66), fill=colour, outline=(59, 36, 24, 255))
        d.line((x, 26, x + 15, 26), fill=(59, 36, 24, 255))
    d.rectangle((0, 26, SHOP_W, 30), fill=(122, 46, 38, 255))
    for x in (6, SHOP_W - 14):
        d.rectangle((x, 30, x + 7, COUNTER_Y), fill=(110, 72, 38, 255), outline=(52, 32, 18, 255))
    lamp(img, d, 120, 66, 10)
    d = ImageDraw.Draw(img)
    prop(img, "crate", 560, COUNTER_Y)
    prop(img, "barrel", 520, COUNTER_Y)
    d.rectangle((0, COUNTER_Y, SHOP_W, COUNTER_Y + 11), fill=(160, 112, 60, 255))
    d.line((0, COUNTER_Y, SHOP_W, COUNTER_Y), fill=(196, 140, 84, 255))
    d.line((0, COUNTER_Y + 11, SHOP_W, COUNTER_Y + 11), fill=(70, 44, 22, 255))
    for x in range(70, SHOP_W, 150):
        d.line((x, COUNTER_Y + 1, x, COUNTER_Y + 10), fill=(110, 72, 38, 255))
    # A sacking cloth hangs down the front, ragged at the hem, with the trestle legs showing below.
    for x in (40, 300, 560):
        d.line((x, COUNTER_Y + 12, x + 22, SHOP_H), fill=(70, 44, 22, 255), width=4)
        d.line((x + 22, COUNTER_Y + 12, x, SHOP_H), fill=(70, 44, 22, 255), width=4)
    d.rectangle((0, COUNTER_Y + 12, SHOP_W, 326), fill=(176, 148, 104, 255))
    for y in range(COUNTER_Y + 14, 326, 2):
        for x in range((y // 2) % 2, SHOP_W, 2):
            d.point((x, y), fill=(160, 132, 92, 255))
    for x in range(0, SHOP_W, 6):
        d.line((x, COUNTER_Y + 16, x + 3, COUNTER_Y + 16), fill=(120, 94, 60, 255))
    for x in range(0, SHOP_W, 8):
        d.polygon([(x, 326), (x + 8, 326), (x + 4, 331 + (x // 8) % 3)], fill=(176, 148, 104, 255))
    shops[1] = img

    # The village shop: an old stone wall with a window onto the Old Wood, timber beams, oil lamps, and a shelf for what
    # you keep.
    img = backdrop(custom_art.thornwick_wall(), shade=0.92)
    d = ImageDraw.Draw(img)
    shadow(img, (0, 40, SHOP_W, 48), 60)
    d = ImageDraw.Draw(img)
    d.rectangle((0, 26, SHOP_W, 40), fill=(78, 44, 18, 255))
    d.line((0, 27, SHOP_W, 27), fill=(110, 69, 32, 255))
    d.line((0, 40, SHOP_W, 40), fill=(40, 22, 10, 255))
    for x in (0, SHOP_W - 10):
        d.rectangle((x, 26, x + 9, COUNTER_Y), fill=(78, 44, 18, 255), outline=(40, 22, 10, 255))
    d.polygon([(10, 41), (34, 41), (10, 65)], fill=(78, 44, 18, 255), outline=(40, 22, 10, 255))
    d.polygon([(SHOP_W - 11, 41), (SHOP_W - 35, 41), (SHOP_W - 11, 65)], fill=(78, 44, 18, 255), outline=(40, 22, 10, 255))
    # Wainscoting along the bottom of the wall, behind the counter.
    d.rectangle((10, 168, SHOP_W - 11, COUNTER_Y), fill=(120, 78, 40, 255))
    d.rectangle((10, 168, SHOP_W - 11, 171), fill=(92, 56, 26, 255))
    d.line((10, 168, SHOP_W - 11, 168), fill=(150, 100, 56, 255))
    for x in range(30, SHOP_W - 11, 28):
        d.line((x, 172, x, COUNTER_Y), fill=(98, 62, 30, 255))
    lamp(img, d, 132, 41, 18)
    lamp(img, d, 520, 41, 18)
    d = ImageDraw.Draw(img)
    shelf(d, SHELF_Y, (78, 44, 18, 255), (147, 96, 47, 255), (176, 122, 64, 255), (78, 44, 18, 255))
    # Counter props stay in the corner by the appraisal panel, clear of the hints under the chest.
    prop(img, "jar", 394, COUNTER_Y)
    prop(img, "barrel", 470, COUNTER_Y)
    d = ImageDraw.Draw(img)
    planks(d, COUNTER_Y + 12, SHOP_H, (138, 90, 44, 255), (104, 66, 32, 255), 40)
    d.rectangle((0, COUNTER_Y, SHOP_W, COUNTER_Y + 11), fill=(176, 122, 64, 255))
    d.line((0, COUNTER_Y, SHOP_W, COUNTER_Y), fill=(208, 154, 90, 255))
    d.line((0, COUNTER_Y + 11, SHOP_W, COUNTER_Y + 11), fill=(78, 44, 18, 255))
    d.rectangle((0, 300, SHOP_W, 304), fill=(104, 66, 32, 255))
    shops[2] = img

    # The town shop: panelled walls and a big display window onto the square at sunset; what you keep stands on the
    # sill. Brass-edged walnut counter, lanterns.
    img = Image.new("RGBA", (SHOP_W, SHOP_H), (168, 142, 110, 255))
    d = ImageDraw.Draw(img)
    # Plaster above, with a faint mottle.
    for y in range(26, 116, 3):
        for x in range((y * 7) % 11, SHOP_W, 11):
            d.point((x, y), fill=(156, 130, 100, 255))
    img.alpha_composite(custom_art.crownhold_view(380, 72), (140, 26))
    d = ImageDraw.Draw(img)
    d.rectangle((134, 24, 526, 29), fill=(52, 32, 20, 255))
    for x in (134, 200, 266, 332, 398, 464, 522):
        d.rectangle((x, 26, x + 4, SHELF_Y), fill=(52, 32, 20, 255))
    d.rectangle((136, 60, 524, 62), fill=(52, 32, 20, 255))
    # The sill: what you keep stands against the window.
    d.rectangle((128, SHELF_Y, 532, SHELF_Y + 7), fill=(206, 194, 170, 255))
    d.line((128, SHELF_Y, 532, SHELF_Y), fill=(236, 228, 210, 255))
    d.line((128, SHELF_Y + 7, 532, SHELF_Y + 7), fill=(110, 96, 80, 255))
    # A dado rail, then panelled wood down to the counter.
    d.rectangle((0, 116, SHOP_W, COUNTER_Y), fill=(104, 70, 44, 255))
    d.rectangle((0, 112, SHOP_W, 117), fill=(70, 44, 26, 255))
    d.line((0, 112, SHOP_W, 112), fill=(130, 90, 58, 255))
    for x in range(0, SHOP_W, 80):
        d.rectangle((x + 6, 126, x + 74, 228), fill=(112, 76, 48, 255), outline=(60, 38, 22, 255))
        d.line((x + 7, 127, x + 73, 127), fill=(140, 98, 64, 255))
        d.line((x + 7, 127, x + 7, 227), fill=(140, 98, 64, 255))
    # Goods on the far wall: a shelf of jars and bottles.
    d.rectangle((548, 84, 628, 88), fill=(70, 44, 26, 255))
    d = ImageDraw.Draw(img)
    lamp(img, d, 110, 30, 30)
    lamp(img, d, 562, 30, 22)
    d = ImageDraw.Draw(img)
    prop(img, "jar", 556, 84)
    prop(img, "flask", 578, 84)
    prop(img, "small_jar", 598, 84)
    prop(img, "flask", 398, COUNTER_Y)
    d = ImageDraw.Draw(img)
    d.rectangle((0, COUNTER_Y + 12, SHOP_W, SHOP_H), fill=(74, 46, 28, 255))
    for x in range(0, SHOP_W, 80):
        d.rectangle((x + 8, COUNTER_Y + 22, x + 72, 312), fill=(82, 52, 32, 255), outline=(46, 28, 16, 255))
        d.line((x + 9, COUNTER_Y + 23, x + 71, COUNTER_Y + 23), fill=(106, 68, 44, 255))
    d.rectangle((0, COUNTER_Y, SHOP_W, COUNTER_Y + 11), fill=(90, 58, 36, 255))
    d.line((0, COUNTER_Y + 1, SHOP_W, COUNTER_Y + 1), fill=(120, 80, 52, 255))
    d.rectangle((0, COUNTER_Y + 10, SHOP_W, COUNTER_Y + 12), fill=(201, 160, 64, 255))
    d.line((0, COUNTER_Y + 10, SHOP_W, COUNTER_Y + 10), fill=(240, 208, 112, 255))
    shops[3] = img

    # The Emporium: a colonnade in daylight with the Gilded Gate down the avenue, crimson drapes with gold, a marble
    # ledge and a marble counter.
    img = backdrop(custom_art.emporium_hall())
    d = ImageDraw.Draw(img)
    crimson = [(106, 20, 26, 255), (138, 28, 36, 255), (168, 36, 46, 255), (138, 28, 36, 255)]
    for x0, x1 in ((0, 46), (SHOP_W - 46, SHOP_W)):
        for i, x in enumerate(range(x0, x1, 6)):
            d.rectangle((x, 26, x + 5, SHOP_H), fill=crimson[i % 4])
        d.rectangle((x0, 118, x1, 124), fill=(212, 168, 56, 255), outline=(140, 100, 30, 255))
    d.rectangle((0, 26, SHOP_W, 44), fill=(138, 28, 36, 255))
    for x in range(0, SHOP_W, 20):
        d.ellipse((x, 38, x + 19, 52), fill=(138, 28, 36, 255))
        d.line((x + 2, 52, x + 17, 52), fill=(212, 168, 56, 255))
        for fx in range(x + 2, x + 18, 3):
            d.line((fx, 52, fx, 55), fill=(240, 200, 90, 255))
    d.line((0, 27, SHOP_W, 27), fill=(212, 168, 56, 255))
    shelf(d, SHELF_Y, (150, 140, 130, 255), (232, 228, 220, 255), (255, 255, 255, 255), (212, 168, 56, 255))
    prop(img, "jar", 394, COUNTER_Y)
    d = ImageDraw.Draw(img)
    d.rectangle((0, COUNTER_Y + 16, SHOP_W, SHOP_H), fill=(52, 44, 60, 255))
    for x in range(0, SHOP_W, 96):
        d.rectangle((x + 10, COUNTER_Y + 26, x + 86, 318), outline=(212, 168, 56, 255))
        d.line((x + 20, COUNTER_Y + 40, x + 60, 300), fill=(70, 62, 80, 255))
    d.rectangle((0, COUNTER_Y, SHOP_W, COUNTER_Y + 12), fill=(236, 232, 224, 255))
    for x in range(10, SHOP_W, 37):
        d.line((x, COUNTER_Y + 2, x + 14, COUNTER_Y + 10), fill=(189, 182, 172, 255))
    d.rectangle((0, COUNTER_Y + 12, SHOP_W, COUNTER_Y + 16), fill=(212, 168, 56, 255))
    d.line((0, COUNTER_Y + 12, SHOP_W, COUNTER_Y + 12), fill=(244, 216, 120, 255))
    shops[4] = img

    for tier, img in shops.items():
        save(backdrop_size(gen("backgrounds", f"shop{tier}.png")) or img, "backgrounds", f"shop{tier}.png")


def build_items():
    """Each thing's sprite (32x32 art, trimmed to what's drawn), for the opening and the shelf (2x) and the same picture
    for the small slots (the collection, the exhibition, the keep chooser), so a thing looks the same everywhere."""
    for key, img in custom_art.items().items():
        art = gen("items", f"{key}.png") or gen("heartsongs", f"{key}.png")
        img = trimmed(art if art else img)
        save(img, "sprites", "items", f"{key}.png")
        save(img, "sprites", "items", "icons", f"{key}.png")


def build_portraits():
    """Head-and-shoulders crops from the full figures: 76px for cards/tips, 38px for the waiting queue.

    Round-one portraits are only a fallback when a figure has not been delivered yet.
    """
    drawn = custom_art.portraits()
    with open(os.path.join(ROOT, "art", "manifest.json"), encoding="utf-8") as f:
        figures = {a["id"].split(".", 1)[1]: a for a in json.load(f)["assets"] if a["category"] == "figure"}
    keys = list(dict.fromkeys([*drawn, *figures]))
    layout = {}
    for key in keys:
        figure = gen("figures", f"{key}.png")
        if figure is not None:
            save(figure, "figures", f"{key}.png")
            bounds = figure.getbbox()
            # Begin at the silhouette's top, so hats and shorter folk retain their identity. Include shoulders,
            # but not held tools lower down. The figure canon centres the head around column 48.
            side = max(32, min(48, round(figures[key]["height"] * 0.34)))
            left = figure.width // 2 - side // 2
            crop = (left, bounds[1], left + side, bounds[1] + side)
            face = figure.crop(crop).resize((38, 38), Image.Resampling.NEAREST)
            save(scaled(face, 2), "sprites", "sellers", f"{key}.png")
            save(face, "sprites", "sellers", "faces", f"{key}.png")
            layout[key] = {"height": figures[key]["height"], "bounds": list(bounds), "head_crop": list(crop)}
            continue
        art = gen("portraits", f"{key}.png")
        if art is None and key not in drawn:
            continue
        img = art if art is not None else scaled(drawn[key], 2)
        save(img, "sprites", "sellers", f"{key}.png")
        save(img.crop(FACE), "sprites", "sellers", "faces", f"{key}.png")
    os.makedirs(os.path.join(ASSETS, "figures"), exist_ok=True)
    with open(os.path.join(ASSETS, "figures", "layout.json"), "w", encoding="utf-8") as f:
        json.dump(layout, f, indent=2)


def build_scene_layers():
    """Keep scene art native: the shop renders these layers at exactly 4x (the paper at 2x)."""
    sys.path.insert(0, os.path.join(ROOT, "art", "tools"))
    from validate import _unlit

    for folder in ("sky", "place", "frame", "shelf", "counter", "sheet", "props"):
        source = os.path.join(GENERATED, folder)
        if not os.path.isdir(source):
            continue
        for name in sorted(os.listdir(source)):
            if not name.endswith(".png") or name.endswith("_4x.png"):
                continue
            image = gen(folder, name)
            save(image, folder, name)
            if folder == "props" and name.startswith("lantern_"):
                save(_unlit(image), folder, name[:-4] + "_unlit.png")


def build_curios():
    """Curios fill a 56px slot at 2x: 28x28 art, or a 24x24 placeholder centred."""
    for key, img in custom_art.curios().items():
        art = gen("curios", f"{key}.png")
        if art:
            save(scaled(art, 2), "ui", "curios", f"{key}.png")
            continue
        img = scaled(trimmed(img), 2)
        slot = Image.new("RGBA", (CURIO_SLOT, CURIO_SLOT), (0, 0, 0, 0))
        slot.alpha_composite(img, ((CURIO_SLOT - img.width) // 2, (CURIO_SLOT - img.height) // 2))
        save(slot, "ui", "curios", f"{key}.png")


def build_signs():
    """The Gilded Guild's emblem, and the Aldric Key whole and in its four shards, at 2x. Each shard keeps its place on the
    whole key's canvas, so the ones you hold stack back into the key."""
    save(scaled(gen("emblems", "guild.png") or custom_art.guild_sign(), 2), "ui", "guild.png")
    whole = gen("story", "key_whole.png")
    if whole:
        save(scaled(whole, 2), "ui", "key_whole.png")
        for n in range(1, 5):
            save(scaled(gen("story", f"key_shard_{n}.png"), 2), "ui", f"key_{n}.png")
        return
    shard = custom_art.key_shard()
    whole = Image.new("RGBA", (96, 24), (0, 0, 0, 0))
    for n in range(1, 5):
        piece = Image.new("RGBA", (96, 24), (0, 0, 0, 0))
        piece.alpha_composite(shard, ((n - 1) * 24, (24 - shard.height) // 2))
        whole.alpha_composite(piece)
        save(scaled(piece, 2), "ui", f"key_{n}.png")
    save(scaled(whole, 2), "ui", "key_whole.png")


def build_icons():
    """An icon per upgrade (24x24 art at 2x: 48px) and per kind of thing (16x16 at 2x: 32px)."""
    folder = os.path.join(GENERATED, "icons")
    if os.path.isdir(folder):
        for name in sorted(os.listdir(folder)):
            if name.startswith("upgrade_"):
                save(scaled(gen("icons", name), 2), "ui", "icons", name[len("upgrade_"):])
    else:
        for name, rows in ICONS.items():
            save(pixel_icon(rows, name).resize((16 * UI_SCALE, 16 * UI_SCALE), Image.NEAREST), "ui", "icons", f"{name}.png")
    for name, rows in KIND_ICONS.items():
        art = gen("kinds", f"kind_{name}.png")
        save(scaled(art, 2) if art else pixel_icon(rows, name).resize((32, 32), Image.NEAREST), "ui", "kinds", f"{name}.png")


def build_effects():
    """The burst (a row of frames the game tints and adds), the spinning coin, a sparkle and a puff of dust; and the
    faces of the three lock makers' locks, for picking."""
    save(gen("fx", "explosion.png") or custom_art.burst(), "sprites", "fx", "burst.png")
    for name in ("coin_spin", "sparkle", "dust"):
        art = gen("fx", f"{name}.png")
        if art:
            save(art, "sprites", "fx", f"{name}.png")
    for maker in ("rusty", "guild", "dwarven"):
        art = gen("lockpick", f"face_{maker}.png")
        if art:
            save(art, "sprites", "lockpick", f"face_{maker}.png")


def build_scenes():
    """The story's own backdrops (640x360, at 2x; round 2 draws them at 320x180), where there are any: Bram's back room,
    the Guild's counting-house, the Keyhold, the vault under the Gate."""
    for key in ("bram", "guild", "keyhold", "vault"):
        art = gen("scenes", f"{key}.png")
        if art:
            save(backdrop_size(art), "backgrounds", f"scene_{key}.png")


def pixel_icon(rows, name):
    assert len(rows) == 16 and all(len(r) == 16 for r in rows), f"icon {name} must be 16x16"
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    px = img.load()
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch != ".":
                px[x, y] = (*ICON_PALETTE[ch], 255)
    return img


# The Gilding (0.33, scripts/Ui/Gilt.cs): for each picture of Bram, which pixels turn to gold at which step of his
# pledge. Steps are boxes over the picture (in its own pixels); inside a box only his skin's colours count, so the
# shirt, the beard and the waistcoat stay as they are. The mask's red channel is the step times 64 (0: never).
GILD_SKIN = {(225, 168, 130), (189, 126, 95), (206, 94, 0)}
GILD_MASKS = {
    # name: (picture, [(step, (x0, y0, x1, y1)) as fractions of its size])
    "figure": ("figures/bram.png", [(1, (0.23, 0.648, 0.42, 0.735)), (2, (0.22, 0.538, 0.42, 0.648)), (3, (0.43, 0.367, 0.59, 0.422))]),
    "portrait": ("sprites/sellers/bram.png", [(3, (0.40, 0.55, 0.72, 0.74))]),
    "face": ("sprites/sellers/faces/bram.png", [(3, (0.40, 0.55, 0.72, 0.74))]),
}


# The gilded who stand in view of the counter (Core.GameState.Statues): their figures, gold all over, on a plinth.
# Drawn at the 3px art's grain (100 cells tall, shown at 1.5 canvas pixels a cell). Old Haddon wears Silas Brass's
# figure and Thornwick's three are old regulars' until they're drawn; Jory Kettle is his own.
STATUE_LOOKS = ["pell", "hob", "gerda", "penn", "dorrin"]
STATUE_CELLS = 100


def gild_pixel(r, g, b):
    l = min(1.0, (0.3 * r + 0.55 * g + 0.15 * b) / 255)
    dark, mid, light = (102, 56, 13), (219, 153, 41), (255, 237, 148)
    a, c, t = (dark, mid, l * 2) if l < 0.5 else (mid, light, (l - 0.5) * 2)
    return tuple(int(a[i] + (c[i] - a[i]) * t) for i in range(3))


def build_statues():
    from PIL import Image
    out = os.path.join(ASSETS, "props")
    os.makedirs(out, exist_ok=True)
    for look in STATUE_LOOKS:
        src = os.path.join(ASSETS, "figures", f"{look}.png")
        if not os.path.exists(src):
            continue
        im = Image.open(src).convert("RGBA")
        im = im.crop(im.getbbox())
        w = max(1, round(im.size[0] * STATUE_CELLS / im.size[1]))
        im = im.resize((w, STATUE_CELLS), Image.LANCZOS)
        px = im.load()
        for y in range(im.size[1]):
            for x in range(im.size[0]):
                r, g, b, a = px[x, y]
                px[x, y] = (*gild_pixel(r, g, b), 255) if a > 110 else (0, 0, 0, 0)
        plinth_w, plinth_h = max(w, 40) + 6, 9
        sheet = Image.new("RGBA", (plinth_w, STATUE_CELLS + plinth_h), (0, 0, 0, 0))
        sheet.alpha_composite(im, ((plinth_w - w) // 2, 0))
        sp = sheet.load()
        for y in range(plinth_h):
            for x in range(plinth_w):
                edge = x in (0, plinth_w - 1) or y == plinth_h - 1
                colour = (52, 48, 58) if edge else (150, 146, 140) if y < 2 else (112, 108, 112)
                sp[x, STATUE_CELLS + y] = (*colour, 255)
        sheet.save(os.path.join(out, f"statue_{look}.png"))
    print(f"statues: {len(STATUE_LOOKS)}")


def build_gild_masks():
    from PIL import Image
    build_statues()
    out = os.path.join(ASSETS, "ui", "gild")
    os.makedirs(out, exist_ok=True)
    for name, (picture, steps) in GILD_MASKS.items():
        im = Image.open(os.path.join(ASSETS, picture)).convert("RGBA")
        w, h = im.size
        mask = Image.new("RGB", (w, h), (0, 0, 0))
        px, mx = im.load(), mask.load()
        count = 0
        for step, (fx0, fy0, fx1, fy1) in steps:
            for y in range(int(fy0 * h), int(fy1 * h)):
                for x in range(int(fx0 * w), int(fx1 * w)):
                    r, g, b, a = px[x, y]
                    if a > 128 and (r, g, b) in GILD_SKIN and mx[x, y][0] == 0:
                        mx[x, y] = (step * 64, 0, 0)
                        count += 1
        mask.save(os.path.join(out, f"bram_{name}.png"))
        print(f"gild mask bram_{name}: {count} pixels")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--cache", default=os.path.expanduser("~/.cache/keywright-art"))
    parser.add_argument("--density-only", action="store_true", help="overlay art/density3 without rebuilding Kenney or legacy assets")
    parser.add_argument("--density-check", action="store_true", help="validate the density overlay and report crops without writing files")
    parser.add_argument("--chests-only", action="store_true", help="redraw just the chests and their locks (custom_art.chest3)")
    parser.add_argument("--icons-only", action="store_true", help="redraw just the upgrade icons drawn in code (custom_art.ICONS3)")
    parser.add_argument("--gild-only", action="store_true", help="redraw just the Gilding's masks for Bram's pictures (scripts/Ui/Gilt.cs)")
    args = parser.parse_args()
    if args.gild_only:
        build_gild_masks()
        return
    from import_density import apply_density, check_drawn
    if args.chests_only:
        build_chests()
        check_drawn(ROOT)
        return
    if args.icons_only:
        build_drawn_icons()
        check_drawn(ROOT)
        return
    if args.density_only or args.density_check:
        apply_density(ROOT, preserve_existing=True, check_only=args.density_check)
        check_drawn(ROOT)
        return
    os.makedirs(args.cache, exist_ok=True)

    kenney = fetch(*KENNEY, args.cache)

    # Rebuilt from scratch so renamed or dropped pictures and sounds don't linger. Godot's .import files stay (they
    # keep each file's uid) unless what they import is gone.
    folders = [os.path.join(ASSETS, f) for f in
               ("sprites", "audio", "backgrounds", "ui", "sky", "place", "frame", "shelf", "counter", "sheet", "props", "figures")]
    for folder in folders:
        for path in walk(folder):
            if not path.endswith(".import"):
                os.remove(path)

    build_items()
    build_portraits()
    build_scene_layers()
    build_effects()
    build_chests()
    build_brand()
    build_curios()
    build_signs()
    build_shops()
    build_scenes()
    build_ui()
    build_icons()
    apply_density(ROOT)
    build_drawn_icons()
    build_gild_masks()

    for group, files in SOUNDS.items():
        out = os.path.join(ASSETS, "audio", group)
        os.makedirs(out, exist_ok=True)
        for i, (pack, stem) in enumerate(files):
            shutil.copy(os.path.join(kenney, pack, "Audio", f"{stem}.ogg"), os.path.join(out, f"{i}.ogg"))

    fonts = os.path.join(ASSETS, "fonts")
    os.makedirs(fonts, exist_ok=True)
    shutil.copy(os.path.join(kenney, "kenney_fontpackage/Fonts/Kenney Mini.ttf"), os.path.join(fonts, "kenney_mini.ttf"))

    for folder in folders:
        for path in walk(folder):
            if path.endswith(".import") and not os.path.exists(path[:-len(".import")]):
                os.remove(path)
        for here, dirs, files in os.walk(folder, topdown=False):
            if not dirs and not files:
                os.rmdir(here)

    count = sum(len(files) for _, _, files in os.walk(ASSETS))
    print(f"Wrote {count} files to {ASSETS}")


if __name__ == "__main__":
    sys.exit(main())
