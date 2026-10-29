"""
Turns a generated image into a finished asset at its native size, by its spec in art/manifest.json.

Round 2 (`--mode flat`, every redrawn asset): the image is a flat-colour illustration, not pixel art, and this makes the
pixel art. It
  1. takes the background out (the image's own alpha, or the flat key colour, magenta: everywhere for a scene's layers,
     where the sky shows through windows; flood-filled in from the border for a sprite, so a pink detail inside it stays);
  2. maps the picture onto the canvas the way its category says (`fit`): the whole frame cropped to the canvas's aspect
     (a sky, a place, the shop's frame, a sheet, a story scene), the content stretched to a strip (the counter), the
     content scaled to the figure's height with its feet on the bottom row, to a sprite's box (on its baseline or
     centred), or to the shelf's width with the board's top on its row;
  3. snaps every colour to the palette an area at a time (each flat area the model drew to its nearest palette colour,
     leaning to the ramps the prompt named; areas drawn apart kept apart, a detail stepping along its ramp; the rim
     anti-aliased into the magenta given the colour inside it) and gives each art pixel the commonest colour of the
     block of the picture it covers, so flat areas stay flat and anti-aliased edges, stray specks and anything finer
     than a block don't survive; if a material still lands in the wrong ramp, --recolour puts it right;
  4. quiets it (a lone speck, or a region of a few pixels, takes its neighbour's colour when the two are close; --no-quiet
     skips it), caps the colour count, and draws a 1-pixel outline around a sprite's silhouette, outside it;
  5. writes the asset's `output` (or --out), and with --preview a 4x copy beside it.

Round 1's modes are kept for the assets round 2 doesn't redraw: `grid` finds the art-pixel grid the model drew and
samples each cell; `area` reduces by area.

Usage:
  python3 art/tools/pixelize.py raw.png --id figure.hob --preview
  python3 art/tools/pixelize.py raw.png --id place.stall --offset-y 40    # the crop window moved down 40 source pixels
  python3 art/tools/pixelize.py raw.png --id place.stall --recolour '#c29026=#ad7d52'   # the road back to earth
Options: --mode flat|grid|area, --key '#ff00ff', --block N (grid mode's art-pixel size), --despeckle, --no-quiet,
--no-outline, --recolour '#from=#to' (repeatable), --out PATH, --preview. Needs only Pillow. Check the result with art/tools/validate.py: it holds every asset
to its category's calm thresholds, and the quiet stage can't rescue a busy drawing (regenerate it simpler).
"""
import argparse
import math
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

# How small a region the quiet stage merges away, by how big the picture is.
QUIET_REGION = {"background": 6, "scene": 6, "portrait": 3, "lock_face": 4, "sky": 8, "place": 6, "frame": 4,
                "counter": 6, "sheet": 6, "figure": 3, "shelf": 3}


# ---- flat mode (round 2): a flat-colour illustration made into pixel art -------------------------------------------

MAGENTA = (255, 0, 255)


def _keyed(c, key, within=150):
    return sum(abs(a - b) for a, b in zip(c[:3], key)) < within


def _key_everywhere(img, key, within):
    out = img.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            if px[x, y][3] and _keyed(px[x, y], key, within):
                px[x, y] = CLEAR
    return out


def flat_alpha(img, spec, key=None):
    """Binary alpha: the image's own, if it has any; else the key colour taken out. A whole picture is opaque; a scene's
    layer (and the counter's strip) loses the key everywhere, since the sky shows through windows and arches; a sprite
    loses it flood-filled in from the border, and then only where it's nearly pure key (a hole through it), so a pink
    detail inside survives."""
    fit = spec.get("fit")
    if spec["background"] == "opaque" and fit != "strip":
        return Image.merge("RGBA", (*img.convert("RGB").split(), Image.new("L", img.size, 255)))
    alpha = img.getchannel("A")
    if sum(1 for v in artlib.pixels(alpha) if v < 128) > img.width * img.height // 100:
        return _threshold(img)
    key = key or MAGENTA
    if fit in ("frame", "strip"):
        return defringe(_key_everywhere(img, key, 150), key)
    return defringe(_key_everywhere(remove_background(img, key, tolerance=60), key, 60), key)


def defringe(img, key):
    """The anti-aliased rim where the subject met the key colour: a pixel on the shape's edge that's the colour just
    inside it with some of the key mixed in takes that inner colour, so the edge doesn't keep a pink line of its own."""
    from PIL import ImageFilter
    alpha = img.getchannel("A").point(lambda v: 255 if v >= 128 else 0)
    inside = alpha.filter(ImageFilter.MinFilter(5))
    w, h = img.size
    a, ins = artlib.pixels(alpha), artlib.pixels(inside)
    px = img.load()
    changes = []
    for i, (on, deep) in enumerate(zip(a, ins)):
        if not on or deep:
            continue
        x, y = i % w, i // w
        p = px[x, y][:3]
        best = None
        for dy in range(-3, 4):
            for dx in range(-3, 4):
                X, Y = x + dx, y + dy
                if 0 <= X < w and 0 <= Y < h and ins[Y * w + X]:
                    c = px[X, Y][:3]
                    towards = [k - v for k, v in zip(key, c)]
                    span = math.hypot(*towards) or 1
                    t = sum((q - v) * k for q, v, k in zip(p, c, towards)) / span ** 2
                    off = math.dist(p, [v + t * k for v, k in zip(c, towards)])
                    if best is None or off < best[0]:
                        best = (off, t, span, c)
        if best and 0.1 < best[1] < 1 and best[0] < max(8, 0.3 * best[1] * best[2]):
            changes.append((x, y, best[3]))
    for x, y, c in changes:
        px[x, y] = (*c, 255)
    return img


def _content_box(img):
    box = artlib.bbox(artlib.opaque_mask(img))
    if not box:
        raise SystemExit("nothing left once the background's out: is the key colour right?")
    return box[0], box[1], box[2] + 1, box[3] + 1


def snap_areas(img, colours, prefer=(), clusters=128, merge=0.035, edge=0.04, bonus=0.015):
    """Every opaque pixel to a palette colour, an area at a time (snapping pixel by pixel instead makes an area whose
    colour sits between two palette colours flicker between them: speckle).
      1. The picture's own colours are clustered, and clusters that look nearly alike (OKLab, every pair of their
         colours under `merge`) are joined: a flat area's noise and anti-aliasing. Every pair, so a run of in-between
         tones along an edge can't chain two areas into one (a hat's brim into the face under it).
      2. Each area goes to its nearest palette colour by eye, never much louder than drawn (a muted brown between umber
         and gold goes to the umber), and a little nearer (`bonus`) for a colour the prompt asked for (`prefer`, the
         asset's ramps) of the same family: the earth road stays earth when gold is as near.
      3. Two areas the picture draws apart (more than `edge`, along a real stretch of border) that land on one colour
         are kept apart: one steps along that colour's ramp toward its own lightness, a shade of the same material
         (the smaller, the detail, unless the larger's step is nearer what was drawn), if that's not much further."""
    rgb = img.convert("RGB")
    alpha = img.getchannel("A").point(lambda v: 255 if v >= 128 else 0)
    q = rgb.quantize(colors=clusters, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
    pal = q.getpalette()
    index = artlib.pixels(q)
    opaque = artlib.pixels(alpha)
    counts = Counter(i for i, a in zip(index, opaque) if a)
    if not counts:
        return Image.new("RGBA", img.size, CLEAR)
    found = list(counts)
    seeds = [tuple(pal[i * 3:i * 3 + 3]) for i in found]
    labs = [artlib.oklab(c) for c in seeds]
    n = len(found)
    far = [[math.dist(labs[i], labs[j]) for j in range(n)] for i in range(n)]
    members = {i: [i] for i in range(n)}
    while len(members) > 1:
        alive = list(members)
        d, i, j = min((far[i][j], i, j) for x, i in enumerate(alive) for j in alive[x + 1:])
        if d >= merge:
            break
        members[i] += members.pop(j)
        for k in members:
            far[i][k] = far[k][i] = max(far[i][k], far[j][k])
    groups = list(members.values())
    group_of = {found[s]: g for g, ss in enumerate(groups) for s in ss}
    size = [sum(counts[found[s]] for s in ss) for ss in groups]
    means = [tuple(round(sum(seeds[s][c] * counts[found[s]] for s in ss) / size[g]) for c in range(3))
             for g, ss in enumerate(groups)]

    prefer = set(prefer)

    def hue(c):
        _, a, b = artlib.oklab(c)
        return math.degrees(math.atan2(b, a)) % 360, math.hypot(a, b)

    def kin(a, b):
        """Two colours of one family: near in hue and in how strong a colour they are (two greys are)."""
        (ha, ca), (hb, cb) = hue(a), hue(b)
        if ca < 0.03 or cb < 0.03:
            return ca < 0.03 and cb < 0.03
        return abs((ha - hb + 180) % 360 - 180) <= 16 and abs(ca - cb) <= 0.03

    def cost(g, c):
        # A colour no louder than drawn: a muted brown between umber and gold goes to the umber.
        louder = max(0.0, hue(c)[1] - hue(means[g])[1] - 0.02)
        return (artlib.distance(means[g], c) + 0.5 * louder
                - (bonus if c in prefer and kin(means[g], c) else 0))

    target = [min(colours, key=lambda c: cost(g, c)) for g in range(len(groups))]

    w, h = img.size
    label = [group_of[i] if a else -1 for i, a in zip(index, opaque)]
    border = Counter()
    for y in range(h):
        row = y * w
        for x in range(w):
            g = label[row + x]
            if g < 0:
                continue
            if x + 1 < w and label[row + x + 1] >= 0 and label[row + x + 1] != g:
                border[min(g, label[row + x + 1]), max(g, label[row + x + 1])] += 1
            if y + 1 < h and label[row + w + x] >= 0 and label[row + w + x] != g:
                border[min(g, label[row + w + x]), max(g, label[row + w + x])] += 1
    neighbours = {}
    for (a, b), count in border.items():
        neighbours.setdefault(a, {})[b] = count
        neighbours.setdefault(b, {})[a] = count

    # 3. Drawn edges.
    def drawn(a, b):
        count = border[min(a, b), max(a, b)]
        return count >= max(8, 0.08 * math.sqrt(min(size[a], size[b]))) and artlib.distance(means[a], means[b]) >= edge

    ramp_of = {c: (cs, i) for cs in artlib.ramps().values() for i, c in enumerate(cs)}
    def step(g, other):
        """g a shade along its colour's ramp, toward its own lightness: (how much further from what was drawn, shade)."""
        ramp, i = ramp_of.get(target[g], (None, None))
        if ramp is None:
            return None
        j = i - 1 if artlib.oklab(means[g])[0] < artlib.oklab(means[other])[0] else i + 1
        if not 0 <= j < len(ramp) or ramp[j] in {target[o] for o in neighbours.get(g, {}) if drawn(g, o)}:
            return None
        return cost(g, ramp[j]) - cost(g, target[g]), ramp[j]

    for a, b in sorted(border, key=lambda pair: -border[pair]):
        if target[a] != target[b] or not drawn(a, b):
            continue
        small, large = (a, b) if size[a] < size[b] else (b, a)
        options = [(s[0] + extra, g, s[1]) for g, other, extra in ((small, large, 0), (large, small, 0.005))
                   if (s := step(g, other))]
        if options and min(options)[0] <= 0.05:
            _, g, shade = min(options)
            target[g] = shade

    mapping = [0] * 256
    for i, g in group_of.items():
        mapping[i] = g
    flat_pal = [v for c in target for v in c]
    q = q.point(mapping)
    q.putpalette(flat_pal + flat_pal[:3] * (256 - len(target)))
    out = Image.new("RGBA", img.size, CLEAR)
    out.paste(q.convert("RGB"), (0, 0), alpha)
    return out


def mode_downscale(img, region, size, colours, alpha_min=0.5, prefer=()):
    """Each target pixel takes the commonest palette colour of the block of `region` it covers (transparent when less
    than `alpha_min` of the block is opaque): flat areas stay flat, and anti-aliased edges, specks and texture finer
    than a block don't survive."""
    tw, th = size
    src = snap_areas(img.crop(region), colours, prefer)
    sw, sh = src.size
    data = artlib.pixels(src)
    col = [min(tw - 1, x * tw // sw) for x in range(sw)]
    row = [min(th - 1, y * th // sh) for y in range(sh)]
    counts = [None] * (tw * th)
    totals = [0] * (tw * th)
    for y in range(sh):
        r, base = row[y] * tw, y * sw
        for x in range(sw):
            i = r + col[x]
            totals[i] += 1
            c = data[base + x]
            if c[3]:
                d = counts[i]
                if d is None:
                    d = counts[i] = {}
                d[c[:3]] = d.get(c[:3], 0) + 1
    out = Image.new("RGBA", (tw, th), CLEAR)
    px = out.load()
    for j in range(th):
        for i in range(tw):
            d = counts[j * tw + i]
            if d and sum(d.values()) >= alpha_min * totals[j * tw + i]:
                px[i, j] = (*max(d, key=d.get), 255)
    return out


def _cropped_to_aspect(img, w, h, offset_y=0):
    """The whole picture, cropped to w:h (centred; moved down by offset_y source pixels)."""
    W, H = img.size
    crop_h = round(W * h / w)
    if crop_h <= H:
        top = max(0, min(H - crop_h, (H - crop_h) // 2 + offset_y))
        return 0, top, W, top + crop_h
    crop_w = round(H * w / h)
    left = (W - crop_w) // 2
    return left, 0, left + crop_w, H


def _board_top(img):
    """The top row of a shelf's board: the lowest run of rows that are mostly opaque across, from its top."""
    w, h = img.size
    px = img.load()
    wide = [sum(1 for x in range(w) if px[x, y][3]) >= 0.7 * w for y in range(h)]
    y = h - 1
    while y >= 0 and not wide[y]:
        y -= 1
    while y > 0 and wide[y - 1]:
        y -= 1
    return max(0, y)


def outline_outside(img):
    """A 1-pixel outline around the silhouette, outside it: the shape keeps its own edge pixels."""
    px = img.load()
    w, h = img.size
    add = []
    for y in range(h):
        for x in range(w):
            if px[x, y][3]:
                continue
            if any(0 <= x + dx < w and 0 <= y + dy < h and px[x + dx, y + dy][3]
                   for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                add.append((x, y))
    for x, y in add:
        px[x, y] = (*OUTLINE, 255)
    return img


def flat(img, spec, manifest, key=None, offset_y=0, calm=True, outline=True):
    w, h = spec["size"]
    fit = spec.get("fit", "sprite")
    colours = sorted(artlib.allowed_colours(spec, manifest))
    ring = 1 if outline and spec.get("outline") else 0
    img = flat_alpha(img, spec, key)
    prefer = artlib.hinted_colours(spec)
    alpha_min = spec.get("alpha_min", 0.5)
    canvas = Image.new("RGBA", (w, h), CLEAR)
    if fit in ("canvas", "frame"):
        canvas = mode_downscale(img, _cropped_to_aspect(img, w, h, offset_y), (w, h), colours, alpha_min, prefer)
    elif fit == "strip":
        canvas = mode_downscale(img, _content_box(img), (w, h), colours, alpha_min, prefer)
    else:
        box = _content_box(img)
        bw, bh = box[2] - box[0], box[3] - box[1]
        m = spec.get("margin", 0) or 0
        if fit == "figure":
            th = spec["height"] - 2 * ring
            tw = max(1, round(bw * th / bh))
            small = mode_downscale(img, box, (min(tw, w - 2 * ring), th), colours, alpha_min, prefer)
            canvas.alpha_composite(small, ((w - small.width) // 2, h - ring - th))
        elif fit == "shelf":
            tw = w - 2 * ring
            th = max(1, round(bh * tw / bw))
            small = mode_downscale(img, box, (tw, th), colours, alpha_min, prefer)
            canvas.alpha_composite(small, (ring, spec["board_row"] - _board_top(small)))
        else:
            iw, ih = w - 2 * m - 2 * ring, h - 2 * m - 2 * ring
            scale = min(iw / bw, ih / bh)
            tw, th = max(1, round(bw * scale)), max(1, round(bh * scale))
            small = mode_downscale(img, box, (tw, th), colours, alpha_min, prefer)
            x = (w - tw) // 2
            # On its baseline (a thing on the table, a prop on the counter), hung from the top (a lantern), or centred.
            y = (spec["baseline"] - ring - th + 1 if spec.get("baseline") is not None
                 else ring if spec.get("hangs") else (h - th) // 2)
            canvas.alpha_composite(small, (x, y))
    if spec["background"] == "opaque":
        canvas = Image.merge("RGBA", (*canvas.convert("RGB").split(), Image.new("L", (w, h), 255)))
    if calm and spec.get("calm"):
        canvas = artlib.quiet(canvas, QUIET_REGION.get(spec["category"], 2), keep=OUTLINE_OK)
    canvas = cap_colours(canvas, spec.get("max_colours") or 64)
    if ring:
        canvas = outline_outside(canvas)
    return canvas


def process(img, spec, manifest, mode=None, key=None, block=None, offset_y=0, outline=True, speckles=False, calm=True):
    opaque = spec["background"] == "opaque"
    mode = mode or spec.get("mode", "grid")
    if mode == "flat":
        return flat(img, spec, manifest, key, offset_y, calm, outline)
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
            if opaque and small.height > h:
                # A picture is drawn to fill the image's width; it's cropped to its aspect here, a whole cell at a time
                # (centred, or moved down by --offset-y source pixels).
                top = max(0, min(small.height - h, (small.height - h) // 2 + round(offset_y / period)))
                small = small.crop((0, top, small.width, top + h))
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
    if calm and spec.get("calm"):
        out = artlib.quiet(out, QUIET_REGION.get(spec["category"], 2), keep=OUTLINE_OK)
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


def recolour(img, swaps, allowed):
    """A material that snapped into the wrong ramp (an earth road gone gold, a shirt's shade gone yellow) put right
    the same way everywhere in the asset: '#from=#to', both palette colours."""
    table = {}
    for swap in swaps:
        a, _, b = swap.partition("=")
        a, b = artlib.hex_rgb(a.strip()), artlib.hex_rgb(b.strip())
        if b not in allowed:
            raise SystemExit(f"--recolour {swap}: {artlib.rgb_hex(b)} isn't a palette colour")
        table[a] = b
    px = img.load()
    for y in range(img.height):
        for x in range(img.width):
            c = px[x, y]
            if c[3] and c[:3] in table:
                px[x, y] = (*table[c[:3]], c[3])
    return img


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("input")
    ap.add_argument("--id", required=True)
    ap.add_argument("--manifest")
    ap.add_argument("--mode", choices=["flat", "grid", "area"])
    ap.add_argument("--key", help="background colour to remove, e.g. '#ff00ff'")
    ap.add_argument("--block", type=float, help="the art-pixel size in the source, if detection picks wrong")
    ap.add_argument("--offset-y", type=int, default=0, help="a whole picture: move the crop window down (source px)")
    ap.add_argument("--despeckle", action="store_true")
    ap.add_argument("--no-quiet", action="store_true", help="skip the quiet stage (texture specks and tiny regions)")
    ap.add_argument("--no-outline", action="store_true")
    ap.add_argument("--recolour", action="append", default=[], metavar="FROM=TO",
                    help="after snapping, one palette colour for another, e.g. '#c29026=#ad7d52' (repeatable)")
    ap.add_argument("--out")
    ap.add_argument("--preview", action="store_true")
    args = ap.parse_args()

    manifest = artlib.load_manifest(args.manifest)
    spec = artlib.asset(manifest, args.id)
    img = artlib.open_rgba(args.input)
    key = artlib.hex_rgb(args.key) if args.key else None
    kw = dict(mode=args.mode, key=key, block=args.block, offset_y=args.offset_y, outline=not args.no_outline,
              speckles=args.despeckle, calm=not args.no_quiet)
    out = process_sheet(img, spec, manifest, **kw) if spec.get("frames") else process(img, spec, manifest, **kw)
    if args.recolour:
        out = recolour(out, args.recolour, artlib.allowed_colours(spec, manifest))
    dest = args.out or artlib.path(spec["output"])
    os.makedirs(os.path.dirname(dest), exist_ok=True)
    out.save(dest)
    if args.preview:
        out.resize((out.width * 4, out.height * 4), Image.NEAREST).save(dest[:-4] + "_4x.png")
    colours = len({c[:3] for c in artlib.pixels(out) if c[3]})
    calm = ""
    if spec.get("calm"):
        m = artlib.calm_metrics(out)
        over = [k for k, v in spec["calm"].items() if m[k] > v]
        calm = (f", specks {m['specks']:.3f} edges {m['edges']:.3f} busy {m['busy']:.1f}"
                + (f" (too busy: {', '.join(over)}; regenerate it simpler)" if over else " (quiet enough)"))
    print(f"{args.id}: {out.size[0]}x{out.size[1]}, {colours} colours{calm} -> {os.path.relpath(dest, artlib.ROOT)}")


if __name__ == "__main__":
    main()
