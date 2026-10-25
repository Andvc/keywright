"""
Turns a generated image into a finished asset at its native size, by its spec in art/manifest.json:

  1. takes the background out (the image's own alpha, a --key colour, or the colour around the border, flood-filled);
  2. snaps every colour to the master palette;
  3. finds the art-pixel grid the model drew and samples each cell's main colour (--mode grid), or reduces the image by
     area (--mode area: backgrounds, scenes, store art, or any sprite whose grid can't be found);
  4. places the content on the asset's canvas (centred, or on the baseline, or in its box), with binary alpha;
  5. caps the colour count (merging the least-used colours into their nearest), enforces the 1-pixel outline;
  6. writes the asset's `output` (or --out), and with --preview a 4x copy beside it.

Usage:
  python3 art/tools/pixelize.py raw.png --id item.barrow_silver
  python3 art/tools/pixelize.py raw.png --id bg.shop1 --offset-y 60      # area mode; crop window moved down 60 px
  python3 art/tools/pixelize.py sheet.png --id fx.sparkle                  # sprite sheets: split into frames first
Options: --mode grid|area, --key '#ff00ff', --block N (the art-pixel size, if detection picks wrong), --despeckle,
--no-outline, --out PATH, --preview. Needs only Pillow. Check the result with art/tools/validate.py.
"""
import argparse
import os
import sys
from collections import Counter, deque

from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import artlib  # noqa: E402

OUTLINE = (0x1a, 0x14, 0x20)
OUTLINE_OK = {(0x1a, 0x14, 0x20), (0x0b, 0x08, 0x10)}
CLEAR = (0, 0, 0, 0)


# ---- background removal ---------------------------------------------------------------------------------------------

def remove_background(img, key=None, tolerance=48):
    """Alpha from the image if it has any; else key out the --key colour or the border's colour, flood-filled in from
    the edges so the same colour inside the subject survives."""
    alpha = img.getchannel("A")
    if alpha.getextrema()[0] < 128 and sum(1 for v in artlib.pixels(alpha) if v < 128) > img.width * img.height // 100:
        return _threshold(img)
    w, h = img.size
    px = img.load()
    if key is None:
        border = [px[x, y][:3] for x in range(w) for y in (0, h - 1)] + [px[x, y][:3] for y in range(h) for x in (0, w - 1)]
        rounded = Counter((r // 8, g // 8, b // 8) for r, g, b in border)
        (kr, kg, kb), count = rounded.most_common(1)[0]
        if count < len(border) * 0.5:
            return img  # no single background colour: leave it opaque (the validator will say so)
        key = (kr * 8 + 4, kg * 8 + 4, kb * 8 + 4)

    def close(c):
        return (c[0] - key[0]) ** 2 + (c[1] - key[1]) ** 2 + (c[2] - key[2]) ** 2 <= tolerance ** 2

    out = img.copy()
    op = out.load()
    seen = bytearray(w * h)
    queue = deque()
    for x in range(w):
        queue.extend(((x, 0), (x, h - 1)))
    for y in range(h):
        queue.extend(((0, y), (w - 1, y)))
    while queue:
        x, y = queue.popleft()
        i = y * w + x
        if seen[i]:
            continue
        seen[i] = 1
        if not close(px[x, y]):
            continue
        op[x, y] = CLEAR
        for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
            if 0 <= nx < w and 0 <= ny < h and not seen[ny * w + nx]:
                queue.append((nx, ny))
    return out


def _threshold(img):
    out = img.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            px[x, y] = (r, g, b, 255) if a >= 128 else CLEAR
    return out


# ---- palette ----------------------------------------------------------------------------------------------------------

def snap(img, colours):
    """Every opaque pixel to its nearest palette colour (Pillow's quantizer, no dithering); alpha kept binary."""
    pal = Image.new("P", (1, 1))
    flat = [v for c in colours for v in c]
    flat += flat[:3] * (256 - len(colours))
    pal.putpalette(flat)
    rgb = img.convert("RGB").quantize(palette=pal, dither=Image.Dither.NONE).convert("RGB")
    out = Image.new("RGBA", img.size, CLEAR)
    a = img.getchannel("A").point(lambda v: 255 if v >= 128 else 0)
    out.paste(rgb, (0, 0), a)
    return out


def cap_colours(img, limit):
    """Merges the least-used colours into their nearest remaining colour until at most `limit` are left."""
    px = img.load()
    counts = Counter(px[x, y][:3] for y in range(img.height) for x in range(img.width) if px[x, y][3])
    if len(counts) <= limit:
        return img
    keep = [c for c, _ in counts.most_common()]
    while len(keep) > limit:
        drop = keep.pop()
        target = min(keep, key=lambda c: artlib.distance(drop, c))
        for y in range(img.height):
            for x in range(img.width):
                if px[x, y][3] and px[x, y][:3] == drop:
                    px[x, y] = (*target, 255)
    return img


# ---- the grid ---------------------------------------------------------------------------------------------------------

def _profile(img, axis, box):
    """How many pixel changes there are between each column (axis 0) or row (axis 1) and the one before it."""
    px = img.load()
    x0, y0, x1, y1 = box
    if axis == 0:
        return {x: sum(px[x, y] != px[x - 1, y] for y in range(y0, y1 + 1)) for x in range(x0 + 1, x1 + 1)}
    return {y: sum(px[x, y] != px[x, y - 1] for x in range(x0, x1 + 1)) for y in range(y0 + 1, y1 + 1)}


def _period(profile, lo, hi):
    """The block size and phase whose grid lines land on the most changes (the smallest such size wins ties)."""
    if not profile:
        return None
    keys = sorted(profile)
    start, end = keys[0], keys[-1]
    mean = sum(profile.values()) / len(profile) or 1
    results = []
    p = lo
    while p <= hi:
        best = (0.0, 0.0)
        for phase10 in range(0, int(p * 10), 5):
            phase = phase10 / 10
            hits, n = 0, 0
            pos = start + phase
            while pos <= end:
                hits += profile.get(int(round(pos)), 0)
                n += 1
                pos += p
            if n >= 3:
                score = hits / n / mean
                if score > best[0]:
                    best = (score, phase)
        results.append((best[0], p, best[1]))
        p = round(p + max(0.05, p * 0.01), 3)
    top = max(r[0] for r in results)
    for score, p, phase in sorted(results, key=lambda r: r[1]):
        if score >= top * 0.95:
            return p, start + phase
    return None


def grid_sample(img, expected=None, block=None, span=None):
    """Finds the drawn grid and returns one pixel per cell (the cell's main colour, read from its middle).

    `span` (cells across the content, when the asset must fill its canvas's width) fixes the block size outright;
    otherwise it's searched for near `expected` (the size the prompt asked for), or exactly `block`."""
    mask = artlib.opaque_mask(img)
    box = artlib.bbox(mask) or (0, 0, img.width - 1, img.height - 1)
    if span:
        p = q = (box[2] - box[0] + 1) / span
        ox, oy = float(box[0]), float(box[1])
    else:
        if block:
            lo = hi = float(block)
        else:
            guess = expected or 16
            lo, hi = max(2.0, guess * 0.7), guess * 1.4
        px_period = _period(_profile(img, 0, box), lo, hi)
        py_period = _period(_profile(img, 1, box), lo, hi)
        if not px_period or not py_period:
            return None
        (p, ox), (q, oy) = px_period, py_period
        # Pixels are square: if the two disagree, trust the one nearer what was asked for.
        if abs(p - q) / max(p, q) >= 0.08:
            p = q = min((p, q), key=lambda v: abs(v - (expected or v)))
        else:
            p = q = (p + q) / 2
        # Back up to the first line at or before the content's edge.
        while ox > box[0] + 0.5:
            ox -= p
        while oy > box[1] + 0.5:
            oy -= q
    cols = int((box[2] + 1 - ox) / p + 0.5)
    rows = int((box[3] + 1 - oy) / q + 0.5)
    src = img.load()
    out = Image.new("RGBA", (max(1, cols), max(1, rows)), CLEAR)
    op = out.load()
    for j in range(rows):
        for i in range(cols):
            x0, y0 = ox + i * p, oy + j * q
            votes = Counter()
            for yy in range(int(y0 + q * 0.3), int(y0 + q * 0.7) + 1):
                for xx in range(int(x0 + p * 0.3), int(x0 + p * 0.7) + 1):
                    if 0 <= xx < img.width and 0 <= yy < img.height:
                        votes[src[xx, yy]] += 1
            if votes:
                op[i, j] = votes.most_common(1)[0][0]
    return out, p


def area_reduce(img, size, opaque, aspect_offset=0):
    """Reduces by area: an opaque picture cropped to the target's aspect (moved down by aspect_offset source pixels);
    a sprite's content scaled to fit."""
    w, h = size
    if opaque:
        src_w, src_h = img.size
        crop_h = round(src_w * h / w)
        if crop_h <= src_h:
            top = max(0, min(src_h - crop_h, (src_h - crop_h) // 2 + aspect_offset))
            img = img.crop((0, top, src_w, top + crop_h))
        else:
            crop_w = round(src_h * w / h)
            left = (src_w - crop_w) // 2
            img = img.crop((left, 0, left + crop_w, src_h))
        return img.convert("RGB").resize((w, h), Image.BOX).convert("RGBA")
    box = artlib.bbox(artlib.opaque_mask(img))
    if not box:
        return Image.new("RGBA", size, CLEAR)
    content = img.crop((box[0], box[1], box[2] + 1, box[3] + 1))
    scale = min(w / content.width, h / content.height)
    target = (max(1, int(content.width * scale)), max(1, int(content.height * scale)))
    # Premultiply so the edge colours don't pull toward the transparent black.
    rgb = Image.new("RGB", content.size, (0, 0, 0))
    rgb.paste(content.convert("RGB"), (0, 0), content.getchannel("A"))
    small_rgb = rgb.resize(target, Image.BOX)
    small_a = content.getchannel("A").resize(target, Image.BOX)
    out = Image.new("RGBA", target, CLEAR)
    spx, apx, op = small_rgb.load(), small_a.load(), out.load()
    for y in range(target[1]):
        for x in range(target[0]):
            a = apx[x, y]
            if a >= 128:
                r, g, b = spx[x, y]
                op[x, y] = (min(255, r * 255 // a), min(255, g * 255 // a), min(255, b * 255 // a), 255)
    return out


# ---- placing it -------------------------------------------------------------------------------------------------------

def place(content, spec):
    """Puts the trimmed content on the asset's canvas where its category wants it."""
    w, h = spec["size"]
    m = spec.get("margin", 0) or 0
    canvas = Image.new("RGBA", (w, h), CLEAR)
    box = artlib.bbox(artlib.opaque_mask(content))
    if not box:
        return canvas
    content = content.crop((box[0], box[1], box[2] + 1, box[3] + 1))
    cw, ch = content.size
    target = None
    if spec["category"] == "chest_lock":
        target = spec["geometry"]["lid" if spec["part"] == "lid" else "body"]["lock_seat"]
    elif spec["category"] == "story" and spec.get("box"):
        target = spec["box"]
    if target:
        x0, y0, x1, y1 = target
        canvas.alpha_composite(content, (x0 + (x1 - x0 + 1 - cw) // 2, y0 + (y1 - y0 + 1 - ch) // 2))
        return canvas
    x = (w - cw) // 2
    if spec.get("baseline") is not None:
        y = spec["baseline"] - ch + 1
    elif spec["category"] == "chest":
        y = 0 if spec["part"] == "body" else h - ch
    else:
        y = (h - ch) // 2
    if cw > w - 2 * m or ch > h - 2 * m:
        print(f"warning: content {cw}x{ch} is bigger than {w - 2 * m}x{h - 2 * m}; it will be cut", file=sys.stderr)
    canvas.alpha_composite(content, (max(0, x), max(0, y)))
    return canvas


def enforce_outline(img, spec):
    mask = artlib.opaque_mask(img)
    px = img.load()
    for (x, y) in artlib.silhouette_edges(mask, img.size, artlib.outline_borders(spec)):
        if px[x, y][:3] not in OUTLINE_OK:
            px[x, y] = (*OUTLINE, 255)
    return img


def despeckle(img):
    """Lone pixels whose colour none of their 8 neighbours share take their neighbours' main colour."""
    px = img.load()
    w, h = img.size
    changes = []
    for y in range(1, h - 1):
        for x in range(1, w - 1):
            c = px[x, y]
            if not c[3]:
                continue
            around = [px[x + dx, y + dy] for dx in (-1, 0, 1) for dy in (-1, 0, 1) if dx or dy]
            if c not in around and all(a[3] for a in around):
                changes.append((x, y, Counter(around).most_common(1)[0][0]))
    for x, y, c in changes:
        px[x, y] = c
    return img


# ---- one asset --------------------------------------------------------------------------------------------------------

def process(img, spec, manifest, mode=None, key=None, block=None, offset_y=0, outline=True, speckles=False):
    opaque = spec["background"] == "opaque"
    mode = mode or spec.get("mode", "grid")
    if mode == "script":
        raise SystemExit(f"{spec['id']} is drawn by script or by hand from its recipe, not from an image")
    colours = sorted(artlib.allowed_colours(spec, manifest))
    w, h = spec["size"]
    m = spec.get("margin", 0) or 0
    if not opaque:
        img = remove_background(img, key)
    img = snap(img, colours)
    small = None
    if mode == "grid":
        expected = spec.get("gen_block")
        if not expected:
            box = artlib.bbox(artlib.opaque_mask(img)) or (0, 0, img.width - 1, img.height - 1)
            expected = max(2.0, (box[2] - box[0] + 1) / max(1, w - 2 * m) * (0.9 if not opaque else 1))
        # Opaque pictures and chest parts fill their canvas's width, which fixes the grid.
        span = w if (opaque or spec["category"] == "chest") and not block else None
        found = grid_sample(img, expected, block, span)
        if found:
            small, period = found
            sb = artlib.bbox(artlib.opaque_mask(small))
            fits = sb and (sb[2] - sb[0] + 1 <= w - 2 * m + (2 if opaque else 0)) and (sb[3] - sb[1] + 1 <= h - 2 * m + (2 if opaque else 0))
            if not fits:
                print(f"note: the grid found ({period:.1f} px) gives {small.size}, too big for {w}x{h}; reducing by area",
                      file=sys.stderr)
                small = None
    if small is None:
        small = area_reduce(img, (w, h) if opaque else (w - 2 * m, h - 2 * m), opaque, offset_y)
        small = snap(small, colours)
    if opaque:
        out = Image.new("RGBA", (w, h), CLEAR)
        out.alpha_composite(small.crop((0, 0, min(w, small.width), min(h, small.height))),
                            ((w - min(w, small.width)) // 2, (h - min(h, small.height)) // 2))
        if out.getchannel("A").getextrema()[0] < 255:
            out = small.resize((w, h), Image.NEAREST)
        out = Image.merge("RGBA", (*out.convert("RGB").split(), Image.new("L", (w, h), 255)))
    else:
        out = place(small, spec)
    if speckles:
        out = despeckle(out)
    out = cap_colours(out, spec.get("max_colours") or 64)
    if outline and spec.get("outline"):
        out = enforce_outline(out, spec)
    return out


def process_sheet(img, spec, manifest, **kw):
    """A sprite sheet: the source split into equal columns, one per frame, each made into a frame of frame_size."""
    n, (fw, fh) = spec["frames"], spec["frame_size"]
    sheet = Image.new("RGBA", tuple(spec["size"]), CLEAR)
    step = img.width / n
    frame_spec = {**spec, "size": [fw, fh], "category": "fx_frame", "margin": 0}
    for i in range(n):
        part = img.crop((round(i * step), 0, round((i + 1) * step), img.height))
        frame = process(part, frame_spec, manifest, **kw)
        sheet.alpha_composite(frame, (i * fw, 0))
    return sheet


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("input")
    ap.add_argument("--id", required=True)
    ap.add_argument("--manifest")
    ap.add_argument("--mode", choices=["grid", "area"])
    ap.add_argument("--key", help="background colour to remove, e.g. '#ff00ff'")
    ap.add_argument("--block", type=float, help="the art-pixel size in the source, if detection picks wrong")
    ap.add_argument("--offset-y", type=int, default=0, help="area mode, opaque: move the crop window down (source px)")
    ap.add_argument("--despeckle", action="store_true")
    ap.add_argument("--no-outline", action="store_true")
    ap.add_argument("--out")
    ap.add_argument("--preview", action="store_true")
    args = ap.parse_args()

    manifest = artlib.load_manifest(args.manifest)
    spec = artlib.asset(manifest, args.id)
    img = artlib.open_rgba(args.input)
    key = artlib.hex_rgb(args.key) if args.key else None
    kw = dict(mode=args.mode, key=key, block=args.block, offset_y=args.offset_y, outline=not args.no_outline,
              speckles=args.despeckle)
    out = process_sheet(img, spec, manifest, **kw) if spec.get("frames") else process(img, spec, manifest, **kw)
    dest = args.out or artlib.path(spec["output"])
    os.makedirs(os.path.dirname(dest), exist_ok=True)
    out.save(dest)
    if args.preview:
        out.resize((out.width * 4, out.height * 4), Image.NEAREST).save(dest[:-4] + "_4x.png")
    colours = len({c[:3] for c in artlib.pixels(out) if c[3]})
    print(f"{args.id}: {out.size[0]}x{out.size[1]}, {colours} colours -> {os.path.relpath(dest, artlib.ROOT)}")


if __name__ == "__main__":
    main()
