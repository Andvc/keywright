"""Overlay the reviewed density-study art without changing the game's logical layout.

The production catalog owns the high-resolution source pictures. This importer owns runtime paths,
legacy logical dimensions, and derived portraits/lanterns. art/generated remains the reproducible
baseline for dimensions and crops; rerunning this never treats a high-resolution runtime file as native.
"""
import json
from pathlib import Path
import sys

from PIL import Image


WORLD = {"sky", "place", "frame", "shelf", "counter", "prop", "figure"}
RETAINED = {"ui", "fx", "brand", "store"}


def apply_density(root, *, preserve_existing=False, check_only=False):
    root = Path(root)
    catalog_path = root / "art/density3/catalog.json"
    if not catalog_path.exists():
        return
    catalog = json.loads(catalog_path.read_text(encoding="utf-8"))
    if catalog.get("version") != 1:
        raise ValueError(f"Unsupported density catalog: {catalog_path}")
    specs = {spec["id"]: spec for spec in json.loads((root / "art/manifest.json").read_text(encoding="utf-8"))["assets"]}
    metadata_path = root / "art/density3/runtime.json"
    textures = {}
    if preserve_existing and metadata_path.exists():
        textures.update(json.loads(metadata_path.read_text(encoding="utf-8"))["textures"])
    layout = json.loads((root / "assets/figures/layout.json").read_text(encoding="utf-8"))
    jobs = []
    skipped = {}
    for asset_id, entry in catalog["assets"].items():
        status = entry.get("status", "pending")
        if status in {"pending", "retained"}:
            skipped[status] = skipped.get(status, 0) + 1
            continue
        spec = specs[asset_id]
        category = spec["category"]
        if category in RETAINED:
            continue
        relative_source = entry.get("source", entry.get("file"))
        if not relative_source:
            raise ValueError(f"No production source for {asset_id}")
        source = (root / relative_source).resolve()
        if not source.is_relative_to((root / "art/density3/assets").resolve()):
            raise ValueError(f"Density source escapes its asset directory: {relative_source}")
        native = tuple(spec["size"])
        factor = entry["physical_scale_from_manifest"]
        if tuple(entry["source_size"]) != native or factor != (8 if category in WORLD or category == "scene" else 4):
            raise ValueError(f"Unexpected density scale contract for {asset_id}")
        with Image.open(source) as picture:
            if picture.size != tuple(entry["physical_size"]) or picture.size != tuple(n * factor for n in native):
                raise ValueError(f"Unexpected production size for {asset_id}: {picture.size}")
        jobs.append((spec, entry, source, native, factor))

    sys.path.insert(0, str(root / "art/tools"))
    from validate import _unlit

    crop_adjustments = []
    written = 0

    def put(image, target, logical, entry, reference_scale, *, derivation=None):
        nonlocal written
        physical = list(image.size)
        if not check_only:
            destination = root / target
            destination.parent.mkdir(parents=True, exist_ok=True)
            image.save(destination)
        data = {"asset_id": entry["id"], "source": entry.get("source", entry.get("file")),
                "logical_size": list(logical), "physical_size": physical, "reference_scale": reference_scale}
        if entry.get("output_sha256"):
            data["source_sha256"] = entry["output_sha256"]
        if derivation:
            data["derivation"] = derivation
        textures[f"res://{target}"] = data
        written += 1

    for spec, entry, source, native, factor in jobs:
        category = spec["category"]
        name = Path(spec["output"]).name
        key = Path(name).stem
        with Image.open(source) as opened:
            picture = opened.convert("RGBA")
        if category in WORLD or category == "sheet":
            folder = "props" if category == "prop" else "figures" if category == "figure" else category
            put(picture, f"assets/{folder}/{name}", native, entry, 4 if category in WORLD else 2)
            if category == "prop" and key.startswith("lantern_"):
                put(_unlit(picture), f"assets/props/{key}_unlit.png", native, entry, 4, derivation="unlit")
            if category == "figure":
                crop = layout[key]["head_crop"]
                head = picture.crop(tuple(value * factor for value in crop))
                for target, logical, backing in ((f"assets/sprites/sellers/{name}", 76, 152),
                                                  (f"assets/sprites/sellers/faces/{name}", 38, 76)):
                    put(head.resize((backing, backing), Image.Resampling.NEAREST), target, (logical, logical),
                        entry, 1, derivation={"kind": "head_crop", "native_crop": crop,
                                             "density": "fine detail retained for smaller UI slots"})
        elif category in {"item", "heartsong"}:
            with Image.open(root / spec["output"]) as legacy:
                bounds = legacy.convert("RGBA").getbbox()
            if bounds is None:
                raise ValueError(f"Empty baseline item: {spec['id']}")
            legacy_crop = tuple(value * factor for value in bounds)
            alpha = picture.getchannel("A")
            outside = alpha.copy()
            outside.paste(0, legacy_crop)
            pixels = sum(outside.histogram()[1:])
            physical_bounds = picture.getbbox()
            # The new 3px grid can cross an old 4px trim boundary. Expand by whole logical units,
            # retaining the exact pixel pitch and every contour pixel instead of cutting or stretching it.
            expanded = (min(bounds[0], physical_bounds[0] // factor), min(bounds[1], physical_bounds[1] // factor),
                        max(bounds[2], (physical_bounds[2] + factor - 1) // factor),
                        max(bounds[3], (physical_bounds[3] + factor - 1) // factor))
            crop = tuple(value * factor for value in expanded)
            if pixels:
                crop_adjustments.append({"id": spec["id"], "preserved_extra_pixels": pixels,
                                         "legacy_crop": list(bounds), "native_crop": list(expanded),
                                         "logical_size_before": [bounds[2] - bounds[0], bounds[3] - bounds[1]],
                                         "logical_size_after": [expanded[2] - expanded[0], expanded[3] - expanded[1]]})
            trimmed = picture.crop(crop)
            logical = (expanded[2] - expanded[0], expanded[3] - expanded[1])
            for folder in ("assets/sprites/items", "assets/sprites/items/icons"):
                put(trimmed, f"{folder}/{name}", logical, entry, 2,
                    derivation={"kind": "item_crop", "legacy_crop": list(bounds), "native_crop": list(expanded)})
        elif category == "curio":
            put(picture, f"assets/ui/curios/{name}", tuple(n * 2 for n in native), entry, 1)
        elif category == "icon_upgrade":
            put(picture, f"assets/ui/icons/{name.removeprefix('upgrade_')}", tuple(n * 2 for n in native), entry, 1)
        elif category == "icon_kind":
            put(picture, f"assets/ui/kinds/{name.removeprefix('kind_')}", tuple(n * 2 for n in native), entry, 1)
        elif category in {"emblem", "story"}:
            put(picture, f"assets/ui/{name.replace('key_shard_', 'key_')}", tuple(n * 2 for n in native), entry, 1)
        elif category == "lock_face":
            put(picture, f"assets/sprites/lockpick/{name}", native, entry, 2)
        elif category == "scene":
            put(picture, f"assets/backgrounds/scene_{name}", (640, 360), entry, 2)
        else:
            raise ValueError(f"No runtime density mapping for {spec['id']}")

    metadata = {"version": 1, "reference_height": catalog["reference_height"],
                "pixel_pitch": catalog["pixel_pitch"], "outlines": catalog["outlines"],
                "textures": dict(sorted(textures.items()))}
    if not check_only:
        encoded = json.dumps(metadata, indent=2, ensure_ascii=False) + "\n"
        metadata_path.write_text(encoded, encoding="utf-8")
        (root / "assets/art_density.json").write_text(encoded, encoding="utf-8")
        (metadata_path.parent / "import-report.json").write_text(json.dumps({"written": written, "skipped": skipped,
            "item_crop_adjustments": crop_adjustments}, indent=2) + "\n", encoding="utf-8")
    print(f"Density {'check' if check_only else 'import'}: {len(jobs)} sources, {written} runtime textures; skipped {skipped}.")
    if crop_adjustments:
        print(f"Preserved the complete silhouettes of {len(crop_adjustments)} items by expanding their legacy crop on whole logical units.")
    return metadata
