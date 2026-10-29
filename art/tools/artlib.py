"""Shared helpers for the art tools: the manifest, the palette, colour distance, masks. Needs only Pillow."""
import json
import math
import os

from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
ART = os.path.dirname(HERE)
ROOT = os.path.dirname(ART)


def load_manifest(path=None):
    with open(path or os.path.join(ART, "manifest.json")) as f:
        return json.load(f)


def asset(manifest, asset_id):
    for a in manifest["assets"]:
        if a["id"] == asset_id:
            return a
    raise KeyError(f"no asset {asset_id} in the manifest")


def hex_rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def rgb_hex(c):
    return "#" + "".join(f"{v:02x}" for v in c[:3])


def palette(manifest=None):
    """The master palette as RGB tuples, in ramp order."""
    path = os.path.join(ROOT, (manifest or load_manifest())["palette"]["hex"])
    with open(path) as f:
        return [hex_rgb(line.strip()) for line in f if line.strip()]


def ramps():
    """The palette's ramps, {name: [RGB, ...]} dark to light, as make_palette builds them."""
    from make_palette import build
    return {name: [hex_rgb(c) for c in cs] for name, cs, _ in build()}


def hinted_colours(spec):
    """The palette colours an asset's prompt asks it to lean on: its `palette` hints, 'umber:2-4', 'gold:5' or 'night'
    (the whole ramp), steps 1-based."""
    by_name = ramps()
    out = set()
    for hint in spec.get("palette") or []:
        name, _, steps = hint.partition(":")
        cs = by_name.get(name, [])
        lo, _, hi = steps.partition("-")
        span = range(int(lo), int(hi or lo) + 1) if steps else range(1, len(cs) + 1)
        out.update(cs[i - 1] for i in span if 1 <= i <= len(cs))
    return out


def allowed_colours(spec, manifest):
    """What an asset's opaque pixels may be: the palette, plus any listed exception."""
    colours = set(palette(manifest))
    for h in spec.get("palette_exception") or []:
        colours.add(hex_rgb(h))
    return colours


def _lin(c):
    c /= 255
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def oklab(rgb):
    r, g, b = (_lin(v) for v in rgb[:3])
    l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b
    m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b
    s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b
    l, m, s = (math.copysign(abs(v) ** (1 / 3), v) for v in (l, m, s))
    return (0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
            1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
            0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s)


def distance(a, b):
    """Perceptual distance between two RGB colours (OKLab, Euclidean)."""
    la, lb = oklab(a), oklab(b)
    return math.dist(la, lb)


def nearest(rgb, colours, cache=None):
    if cache is not None and rgb in cache:
        return cache[rgb]
    best = min(colours, key=lambda c: distance(rgb, c))
    if cache is not None:
        cache[rgb] = best
    return best


def opaque_mask(img):
    """A set of (x, y) for every opaque pixel (alpha 255)."""
    a = img.getchannel("A") if "A" in img.getbands() else None
    w, h = img.size
    if a is None:
        return {(x, y) for y in range(h) for x in range(w)}
    px = a.load()
    return {(x, y) for y in range(h) for x in range(w) if px[x, y] == 255}


def edge_pixels(mask, size):
    """Opaque pixels that touch transparency (4-neighbour) or the canvas edge."""
    w, h = size
    out = set()
    for (x, y) in mask:
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nx, ny = x + dx, y + dy
            if not (0 <= nx < w and 0 <= ny < h) or (nx, ny) not in mask:
                out.add((x, y))
                break
    return out


def outline_borders(spec):
    """Which canvas sides count as the silhouette's edge: all four, except where chest parts meet each other (the
    lid's bottom row is the seam over the body's top row)."""
    if spec.get("category") == "chest":
        return {"lid": {"left", "right", "top"}, "body": {"left", "right", "bottom"}}.get(spec.get("part"),
                                                                                      {"left", "right", "top", "bottom"})
    # A scene's layers run off the canvas's edges (the frame's top and posts, the shelf's supports): only where they
    # meet transparency is an edge.
    if spec.get("fit") in ("frame", "shelf"):
        return set()
    return {"left", "right", "top", "bottom"}


def silhouette_edges(mask, size, borders):
    """Opaque pixels that touch transparency (4-neighbour), or one of the given canvas sides."""
    w, h = size
    out = set()
    for (x, y) in mask:
        for dx, dy, side in ((1, 0, "right"), (-1, 0, "left"), (0, 1, "bottom"), (0, -1, "top")):
            nx, ny = x + dx, y + dy
            if not (0 <= nx < w and 0 <= ny < h):
                if side in borders:
                    out.add((x, y))
                    break
            elif (nx, ny) not in mask:
                out.add((x, y))
                break
    return out


def outside_edge(mask, size):
    """Opaque pixels that touch transparency (4-neighbour); the canvas border doesn't count."""
    w, h = size
    out = set()
    for (x, y) in mask:
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nx, ny = x + dx, y + dy
            if 0 <= nx < w and 0 <= ny < h and (nx, ny) not in mask:
                out.add((x, y))
                break
    return out


def bbox(mask):
    if not mask:
        return None
    xs = [p[0] for p in mask]
    ys = [p[1] for p in mask]
    return min(xs), min(ys), max(xs), max(ys)


def pixels(img):
    """Every pixel, flat (Pillow 12 renamed getdata)."""
    flat = getattr(img, "get_flattened_data", None)
    return list(flat() if flat else img.getdata())


def path(rel):
    return os.path.join(ROOT, rel)


def open_rgba(p):
    img = Image.open(p)
    return img.convert("RGBA")


# ---- how quiet a picture is (round 2) --------------------------------------------------------------------------------
# 0.23's art measured two to six times as busy as the old code-drawn art at the size the player sees it: a third of the
# pixels in a typical thing were lone specks. These are the measures the validator holds every asset to (thresholds
# per category in the manifest, `calm`), and the pixelizer's quiet stage works toward.

def _interior(img):
    """Opaque pixels that don't touch transparency (the outline and the silhouette's edge aren't texture)."""
    mask = opaque_mask(img)
    return mask - edge_pixels(mask, img.size) if "A" in img.getbands() else mask


def _luma(c):
    return 0.299 * c[0] + 0.587 * c[1] + 0.114 * c[2]


def calm_metrics(img):
    """specks: the share of interior pixels unlike all four neighbours; edges: the share of neighbouring interior pairs
    that differ; busy: the mean brightness step between neighbouring interior pixels (0-255), so many strong edges
    score high and a few soft ones low."""
    px = img.load()
    inside = _interior(img)
    if not inside:
        return dict(specks=0.0, edges=0.0, busy=0.0)
    specks = 0
    for (x, y) in inside:
        c = px[x, y]
        if all(px[x + dx, y + dy] != c for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
            specks += 1
    pairs = differ = 0
    step = 0.0
    for (x, y) in inside:
        c = px[x, y]
        for nx, ny in ((x + 1, y), (x, y + 1)):
            if (nx, ny) in inside:
                d = px[nx, ny]
                pairs += 1
                if d != c:
                    differ += 1
                    step += abs(_luma(c) - _luma(d))
    return dict(specks=specks / len(inside), edges=differ / max(1, pairs), busy=step / max(1, pairs))


def quiet(img, min_region=3, max_step=0.09, keep=()):
    """Takes texture out without touching the drawing: a lone speck, or a region smaller than `min_region`, takes the
    colour of the neighbour it shares the most border with, but only when the two are close (OKLab distance under
    `max_step`), so eyes, glints and deliberate accents stay. `keep` colours (the outline) are never changed."""
    px = img.load()
    w, h = img.size
    keep = set(keep)
    for _ in range(2):
        seen = set()
        changes = []
        for y in range(h):
            for x in range(w):
                c = px[x, y]
                if (x, y) in seen or c[3] == 0 or c[:3] in keep:
                    continue
                region, border, queue = [], {}, [(x, y)]
                seen.add((x, y))
                while queue:
                    cx, cy = queue.pop()
                    region.append((cx, cy))
                    for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                        if not (0 <= nx < w and 0 <= ny < h):
                            continue
                        d = px[nx, ny]
                        if d == c:
                            if (nx, ny) not in seen:
                                seen.add((nx, ny))
                                queue.append((nx, ny))
                        elif d[3] and d[:3] not in keep:
                            border[d] = border.get(d, 0) + 1
                if len(region) >= min_region or not border:
                    continue
                target = max(border, key=border.get)
                if distance(c[:3], target[:3]) < max_step:
                    changes.extend((p, target) for p in region)
        if not changes:
            break
        for (x, y), target in changes:
            px[x, y] = target
    return img
