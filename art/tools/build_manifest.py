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

VERSION = 1

STYLE = (
    "Pixel art game asset for Keywright, a warm, lantern-lit fantasy merchant game set in the sword-and-sorcery realm "
    "of Aldmark. Clean, hand-placed pixel art on a strict square grid: every art pixel is one solid, perfectly square "
    "block of a single flat colour. No anti-aliasing, no blur, no gradients, no texture noise, no dithering, no "
    "painterly brushwork. A limited palette of warm, hue-shifted colours: shadows lean violet-blue, highlights lean "
    "warm yellow. One warm key light from the upper left, cool violet ambient shadow on the right and underneath. "
    "Strong readable silhouette. No text, letters, numbers, runes that look like writing, signatures or watermarks."
)

# What every category shares: native size, background, colour cap, outline, placement and the prompt's frame.
CATEGORIES = {
    "background": dict(
        folder="backgrounds", size=(640, 360), background="opaque", max_colours=48, outline=False, margin=0,
        batch=4, priority="P1", gen_size="1536x1024", mode="area",
        display="2x behind the counter (1280x720); dimmed to 30-60% behind every other screen",
        frame=("A {w}x{h} pixel art background for the shop screen of a 2D game (16:9). Front elevation: a flat stage seen "
               "straight on, no vanishing-point perspective, no people. The shop counter's top is one straight horizontal "
               "edge across the whole width at {counter_pct}% of the height (row 238 of 360); below it is the counter's "
               "plain, low-contrast front face. A display shelf's top surface runs across the middle-left at {shelf_pct}% "
               "of the height (row 98 of 360). The area just above the counter in the middle is calm and mid-dark. "
               "Full-bleed, opaque, no border, no frame. Generate it 16:9 inside the image; it will be cropped to 16:9 and "
               "reduced to {w}x{h}."),
        avoid=["people or animals in the foreground", "text or signs with letters", "objects standing on the counter top",
               "a vanishing-point floor", "photographic detail"],
    ),
    "portrait": dict(
        folder="portraits", size=(76, 76), background="opaque", max_colours=32, outline=False, margin=0,
        batch=1, priority="P1", gen_size="1024x1024", mode="grid",
        display="2x at the counter (152 px in a 176 px frame), 3x in story scenes, 1x in the ledger and Bram's tips; "
                "the 38x38 face crop (x 19-56, y 14-51) at 1x in the waiting queue and ledger rows",
        frame=("A {w}x{h} pixel art character portrait (a head-and-shoulders bust) for a game's dialogue box, drawn so each "
               "art pixel is a crisp square block about {k} px wide (the whole image is the {w}x{h} grid enlarged). "
               "{view} The head is centred left-to-right with the eyes about 45% of the way down; the top of the head is "
               "about a fifth of the way down; the shoulders are cut by the bottom edge. The figure has a 1-pixel very dark "
               "plum outline where it meets the background. Background: {backdrop}, in three or four flat stepped bands, "
               "darker toward the corners, nothing else in it. Full-bleed, opaque, no frame, no border, no text."),
        avoid=["hands or props entering from outside the frame", "a frame or border", "a busy background",
               "a face outside the centre", "photorealism", "glowing auras"],
    ),
    "item": dict(
        folder="items", size=(32, 32), background="transparent", max_colours=20, outline=True, margin=1, baseline=30,
        batch=2, priority="P1", gen_size="1024x1024", mode="grid",
        display="2x on the opening table (64 px); 1x or 2x in slots (collection, exhibition, shelf, pawn list); the game "
                "trims to the content",
        frame=("{title}: a {w}x{h} pixel art game item sprite, drawn so each art pixel is a crisp square block "
               "{k} px wide (the whole image is the {w}x{h} grid enlarged). Three-quarter view from slightly above, as if "
               "resting on a shop counter; upright and centred left-to-right, its lowest pixels on the bottom of the "
               "frame, filling about 90% of the frame. 1-pixel very dark plum outline around the whole silhouette. "
               "Transparent background, no shadow, no glow, no frame, nothing else in the image."),
        avoid=["a background or floor", "a drop shadow", "a glow or aura around it", "unrelated extra objects",
               "text or letters", "soft edges"],
    ),
    "heartsong": dict(
        folder="heartsongs", size=(32, 32), background="transparent", max_colours=20, outline=True, margin=1, baseline=30,
        batch=2, priority="P1", gen_size="1024x1024", mode="grid",
        display="2x at the opening and in the Heartsong reveal (64 px), 1x-2x on the shelf and in the ledger",
        frame=("{title}, one of the nine Heartsongs (a beloved object the realm's archmages sang a spell into): a {w}x{h} "
               "pixel art game item sprite, drawn so each art pixel is a crisp square block {k} px wide (the whole image "
               "is the {w}x{h} grid enlarged). Handmade, old and lovingly worn, warmer and more personal than any treasure, "
               "with a single deep-red accent. Three-quarter view from slightly above, upright, centred, its lowest pixels "
               "on the bottom of the frame, filling about 90% of the frame. 1-pixel very dark plum outline around the "
               "silhouette. Transparent background, no shadow, no glow, no frame, nothing else in the image."),
        avoid=["a background or floor", "a drop shadow", "a glow or aura around it", "musical notes floating",
               "text or letters", "soft edges"],
    ),
    "curio": dict(
        folder="curios", size=(28, 28), background="transparent", max_colours=20, outline=True, margin=1,
        batch=3, priority="P1", gen_size="1024x1024", mode="grid",
        display="2x in 56 px slots: Nim's cards, the ledger's Curios page, the counter's curio row, the Guild's night",
        frame=("{title}: a {w}x{h} pixel art icon of a single small object, drawn so each art pixel is a crisp square "
               "block about {k} px wide (the whole image is the {w}x{h} grid enlarged). A curio: an oddment from an old "
               "warded chest, made for a trade. Three-quarter view from slightly above, centred both ways, filling about "
               "90% of the frame. 1-pixel very dark plum outline around the silhouette. Transparent background, no shadow, "
               "no glow, no frame, nothing else in the image."),
        avoid=["a background", "a drop shadow", "a glow or aura around it", "more than one object unless described",
               "text, letters or numerals", "soft edges"],
    ),
    "chest": dict(
        folder="chest", background="transparent", max_colours=20, outline=True, margin=0,
        batch=4, priority="P1", gen_size="1536x1024", mode="grid",
        display="2x at the counter and the opening (192 px wide), 1x-2x on the title screen; the game stacks lid over "
                "body, lifts the lid when the chest strains, swaps to the open frame, and lets light out of the seam",
        frame=("A {w}x{h} pixel art piece of a treasure chest for a 2D game, drawn so each art pixel is a crisp square "
               "block {k} px wide. Front view from slightly above, perfectly symmetrical left to right. {part} "
               "1-pixel very dark plum outline around the silhouette. Transparent background, no shadow, no glow, no "
               "light from the seam, nothing else in the image."),
        avoid=["perspective that isn't symmetrical", "a glow", "a shadow under it", "text or letters",
               "a different silhouette from the template"],
    ),
    "chest_lock": dict(
        folder="chest", background="transparent", max_colours=12, outline=True, margin=0,
        batch=4, priority="P1", gen_size="1024x1024", mode="grid",
        display="stacked over the chest's lid and body at 2x, on locked chests",
        frame=("A {w}x{h} pixel art overlay for a treasure chest: only {part}, on a fully transparent canvas the size of "
               "the chest part it sits on, drawn so each art pixel is a crisp square block {k} px wide. Front view, "
               "symmetrical. 1-pixel very dark plum outline. Nothing else in the image."),
        avoid=["any part of the chest itself", "a glow", "text or letters"],
    ),
    "icon_upgrade": dict(
        folder="icons", size=(24, 24), background="transparent", max_colours=12, outline=True, margin=1,
        batch=5, priority="P1", gen_size="1024x1024", mode="grid",
        display="2x (48 px) on the day's-end upgrade cards; 1x on the slip for the Witchlight and the Lodestone",
        frame=("A {w}x{h} pixel art UI icon of {subject}, drawn so each art pixel is a crisp square block about {k} px "
               "wide. Bold, simple shapes that read at {w} pixels; three-quarter or front view; centred, filling about "
               "90% of the frame; 1-pixel very dark plum outline. Transparent background, no shadow, no glow, no frame."),
        avoid=["fine detail that won't read at 24 px", "a background", "text, letters or numerals"],
    ),
    "icon_kind": dict(
        folder="kinds", size=(16, 16), background="transparent", max_colours=8, outline=True, margin=1,
        batch=5, priority="P1", gen_size="1024x1024", mode="grid",
        display="2x (32 px) at the start of each row of the appraisal slip, in the ledger and in Settings > Item sounds",
        frame=("A {w}x{h} pixel art symbol icon of {subject}, drawn so each art pixel is a crisp square block {k} px wide. "
               "One simple, iconic shape, like a map symbol, readable at {w} pixels; centred, filling the frame; 1-pixel "
               "very dark plum outline. Transparent background, nothing else."),
        avoid=["detail", "more than one object", "a background", "text, letters or numerals"],
    ),
    "emblem": dict(
        folder="emblems", background="transparent", max_colours=12, outline=True, margin=1,
        batch=5, priority="P1", gen_size="1024x1024", mode="grid",
        display="see the asset",
        frame=("A {w}x{h} pixel art emblem of {subject}, drawn so each art pixel is a crisp square block about {k} px wide. "
               "Front view, centred, symmetrical where it can be; 1-pixel very dark plum outline. Transparent background, "
               "no shadow, no glow."),
        avoid=["text, letters or numerals", "a background", "a glow"],
    ),
    "story": dict(
        folder="story", background="transparent", max_colours=12, outline=True, margin=1,
        batch=5, priority="P1", gen_size="1536x1024", mode="grid",
        display="see the asset",
        frame=("A {w}x{h} pixel art game sprite of {subject}, drawn so each art pixel is a crisp square block about {k} px "
               "wide. Front view, lying flat as if on a table, centred; 1-pixel very dark plum outline. Transparent "
               "background, no shadow, no glow."),
        avoid=["text, letters or numerals", "a background", "a glow"],
    ),
    "ui": dict(
        folder="ui", background="transparent", max_colours=8, outline=False, margin=0,
        batch=6, priority="P2", gen_size="none", mode="script",
        display="nine-slice UI textures at 2x; see the asset for slice margins and tinting",
        frame="(Draw by script or by hand from the exact recipe below; no image generation needed.)",
        avoid=["anything outside the recipe"],
    ),
    "fx": dict(
        folder="fx", background="transparent", max_colours=6, outline=False, margin=0,
        batch=6, priority="P2", gen_size="1536x1024", mode="grid",
        display="additive and tinted by the game; see the asset",
        frame=("A pixel art animation sprite sheet: {frames} frames of {fw}x{fh} art pixels in a single row (a {w}x{h} "
               "sheet), each art pixel a crisp square block. {subject} {colour_rule} Transparent background; each frame "
               "centred in its cell; no text."),
        avoid=["a background", "frames of different sizes", "text"],
    ),
    "lock_face": dict(
        folder="lockpick", size=(160, 160), background="transparent", max_colours=20, outline=True, margin=0,
        batch=6, priority="P2", gen_size="1024x1024", mode="grid",
        display="2x (320 px) in the middle of the lockpicking screen; the game draws the sweet spot, pins and the pick on "
                "top",
        frame=("A {w}x{h} pixel art face of {subject}, seen straight on, as the background of a lockpicking minigame, drawn "
               "so each art pixel is a crisp square block about {k} px wide. A round lock face, centred, with a keyhole "
               "in the middle and a plain, dark, empty ring track around it (between 52 and 68 art pixels from the centre) "
               "where the game draws the pick's sweet spot. 1-pixel very dark plum outline. Transparent outside the lock, "
               "no shadow, no glow, no text."),
        avoid=["anything drawn inside the ring track", "a background", "text, letters or numerals", "a pick or key"],
    ),
    "brand": dict(
        folder="brand", background="transparent", max_colours=24, outline=True, margin=1,
        batch=7, priority="P2", gen_size="1536x1024", mode="grid",
        display="see the asset",
        frame=("{subject}, as {w}x{h} pixel art, each art pixel a crisp square block. 1-pixel very dark plum outline. "
               "Transparent background, no shadow, no glow."),
        avoid=["misspelled letters", "a background", "soft edges"],
    ),
    "scene": dict(
        folder="scenes", size=(640, 360), background="opaque", max_colours=48, outline=False, margin=0,
        batch=7, priority="P3", gen_size="1536x1024", mode="area",
        display="2x behind story scenes, dimmed to about 40% under the dialogue and the portraits",
        frame=("A {w}x{h} pixel art background for a story scene in a 2D game (16:9). Front elevation, a flat stage seen "
               "straight on, no people. The centre third and the bottom third stay calm (portraits and a dialogue box "
               "cover them). Full-bleed, opaque, no border. Generate it 16:9 inside the image; it will be cropped to "
               "16:9 and reduced to {w}x{h}."),
        avoid=["people or creatures", "text or letters", "busy detail in the centre"],
    ),
    "store": dict(
        folder="store", background="opaque", max_colours=96, outline=False, margin=0,
        batch=8, priority="P3", gen_size="1536x1024", mode="area",
        display="the Steam store page and library (not used in the game)",
        frame=("Key art for the Steam store page of Keywright, in the game's pixel art style at a coarse pixel size (each "
               "art pixel a crisp square block). {subject} Leave the logo area described below clear: the logo is added "
               "afterwards. No text."),
        avoid=["text or letters (the logo is composited later)", "photorealism", "a 3D render look"],
    ),
}


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
    k = None
    if cat["mode"] == "grid":
        gw, gh = (int(v) for v in cat["gen_size"].split("x"))
        k = max(1, min(gw // w, gh // h))
    fields = dict(w=w, h=h, k=k, title=asset["title"], subject=asset.get("subject", asset["title"]),
                  counter_pct=round(238 / 360 * 100), shelf_pct=round(98 / 360 * 100), **asset.get("frame_fields", {}))
    frame = cat["frame"].format(**fields)
    parts = [STYLE, frame]
    if asset.get("description"):
        parts.append(f"Subject: {asset['description']}")
    if asset.get("details"):
        parts.append("It must show: " + "; ".join(asset["details"]) + ".")
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
        cat = CATEGORIES[a["category"]]
        spec = {k: v for k, v in cat.items() if k not in ("frame", "avoid", "folder")}
        spec.update({k: v for k, v in a.items() if k not in ("frame_fields",)})
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
             "use it.", ""]
    order = list(CATEGORIES)
    total = {}
    for a in manifest["assets"]:
        total.setdefault(a["category"], []).append(a)
    lines.append("| Category | Assets | Native size | Batch | Priority |")
    lines.append("|---|---|---|---|---|")
    for c in order:
        if c not in total:
            continue
        sizes = sorted({f"{a['size'][0]}x{a['size'][1]}" for a in total[c]})
        pr = sorted({a['priority'] for a in total[c]})
        batches = sorted({a['batch'] for a in total[c]})
        lines.append(f"| {c} | {len(total[c])} | {', '.join(sizes[:4])}{'...' if len(sizes) > 4 else ''} | "
                     f"{', '.join(map(str, batches))} | {', '.join(pr)} |")
    lines.append(f"| **all** | **{len(manifest['assets'])}** | | | |")
    lines.append("")
    lines.append("Style anchors (batch 0, approved before the rest): " +
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
