#!/usr/bin/env python3
"""Check that every round-2 source reaches its runtime path with the intended pixels.

Run from any directory after tools/import_art.py. This only reads images and writes
round2_integration_assets.json beside this script; it never changes delivered art.
"""
import hashlib
import json
import sys
from collections import Counter
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "art/tools"))
from validate import _unlit


def rgba(path):
    with Image.open(ROOT / path) as image:
        return image.convert("RGBA")


def double(image):
    return image.resize((image.width * 2, image.height * 2), Image.Resampling.NEAREST)


def main():
    manifest = json.loads((ROOT / "art/manifest.json").read_text(encoding="utf-8"))
    layout = json.loads((ROOT / "assets/figures/layout.json").read_text(encoding="utf-8"))
    assets = [asset for asset in manifest["assets"] if asset["redo"]]
    assert len(assets) == 242
    assert set(layout) == {a["id"].split(".", 1)[1] for a in assets if a["category"] == "figure"}
    records = []
    checks = 0
    for asset in assets:
        source = rgba(asset["output"])
        category = asset["category"]
        name = Path(asset["output"]).name
        key = asset["id"].split(".")[-1]
        targets = []
        if category in {"sky", "place", "frame", "shelf", "counter", "sheet", "prop", "figure"}:
            folder = {"prop": "props", "figure": "figures"}.get(category, category)
            targets.append((f"assets/{folder}/{name}", source))
            if category == "prop" and name.startswith("lantern_"):
                targets.append((f"assets/props/{name[:-4]}_unlit.png", _unlit(source)))
            if category == "figure":
                entry = layout[key]
                assert entry["height"] == asset["height"], key
                assert entry["bounds"] == list(source.getbbox()), key
                left, top, right, bottom = entry["head_crop"]
                assert top == source.getbbox()[1] and right - left == bottom - top, key
                assert 0 <= left < right <= source.width and 0 <= top < bottom <= source.height, key
                face = source.crop(entry["head_crop"]).resize((38, 38), Image.Resampling.NEAREST)
                targets.extend([(f"assets/sprites/sellers/faces/{key}.png", face),
                                (f"assets/sprites/sellers/{key}.png", double(face))])
        elif category in {"item", "heartsong"}:
            trimmed = source.crop(source.getbbox())
            targets.extend([(f"assets/sprites/items/{name}", trimmed),
                            (f"assets/sprites/items/icons/{name}", trimmed)])
        elif category == "curio":
            targets.append((f"assets/ui/curios/{name}", double(source)))
        elif category == "icon_upgrade":
            targets.append((f"assets/ui/icons/{key}.png", double(source)))
        elif category == "icon_kind":
            targets.append((f"assets/ui/kinds/{key}.png", double(source)))
        elif category == "emblem":
            targets.append((f"assets/ui/{name}", double(source)))
        elif category == "story":
            targets.append((f"assets/ui/{name.replace('key_shard_', 'key_')}", double(source)))
        elif category == "scene":
            targets.append((f"assets/backgrounds/scene_{name}", double(source)))
        elif category == "lock_face":
            targets.append((f"assets/sprites/lockpick/{name}", source))
        else:
            raise AssertionError(f"Unmapped redraw: {asset['id']}")
        destinations = []
        for path, expected in targets:
            actual = rgba(path)
            assert actual.size == expected.size and actual.tobytes() == expected.tobytes(), path
            checks += 1
            destinations.append({"path": path, "size": list(actual.size), "exact_rgba": True})
        records.append({"id": asset["id"], "source": asset["output"],
                        "source_sha256": hashlib.sha256((ROOT / asset["output"]).read_bytes()).hexdigest(),
                        "destinations": destinations})
    report = {"redraw_assets": len(records), "runtime_image_checks": checks,
              "figure_layout_entries": len(layout), "failures": 0,
              "categories": dict(sorted(Counter(a["category"] for a in assets).items())),
              "assets": records}
    output = Path(__file__).with_name("round2_integration_assets.json")
    output.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"PASS: {len(records)} redraws, {checks} exact runtime image checks, {len(layout)} figure layouts")


if __name__ == "__main__":
    main()
