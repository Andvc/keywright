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
