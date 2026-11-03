"""Build the approved 3px-at-1440p art from archived full-size illustrations.

Full-resolution sources stay in the production archive. Originals and the old
native art are never overwritten. New art uses baked integer 3px cells with
logical-size metadata, so the engine preserves the existing scene geometry.
"""
from __future__ import annotations

import argparse
from concurrent.futures import ProcessPoolExecutor, as_completed
import hashlib
import json
from pathlib import Path
import sys

import numpy as np
from PIL import Image
from scipy.ndimage import distance_transform_edt

sys.path.insert(0, str(Path(__file__).resolve().parent))
import artlib
import pixelize

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "art/density3"
MANIFEST = json.loads((ROOT / "art/manifest.json").read_text(encoding="utf-8"))
WORLD = {"sky", "place", "frame", "shelf", "counter", "prop", "figure", "scene"}
RETAINED = {"ui", "fx", "brand", "store"}
VERSION = 1
PITCH = 3
NEAREST = Image.Resampling.NEAREST


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def write_json(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")


def inventory(archive):
    selected = {}
    for batch in ("9s", "10", "11", "12", "13", "14", "15"):
        record = json.loads((ROOT / f"art/generated/_workflow/round2_batch{batch}.json").read_text(encoding="utf-8"))
        for row in record.get("selected", record.get("assets", [])):
            selected[row["id"]] = {"batch": batch, "selection": row,
                                   "source": str(archive / f"batch{batch}_c2_4x_no_outline/sources" / row["source_file"]) if row.get("source_file") else None,
                                   "derived_from": row.get("source") if not row.get("source_file") else None}
    assets = []
    for spec in MANIFEST["assets"]:
        if spec["category"] in RETAINED:
            assets.append({"id": spec["id"], "category": spec["category"], "status": "retained",
                           "reason": "Code/UI, effects, brand or store artwork outside the selected world-density change.",
                           "file": spec["output"], "source_size": spec["size"]})
        else:
            row = selected[spec["id"]]
            if row["source"] and not Path(row["source"]).is_file():
                raise FileNotFoundError(row["source"])
            assets.append({"id": spec["id"], "category": spec["category"], "status": "pending", **row})
    return selected, assets


def fixed_phase(spec):
    """Match the shop's normal 1440p placements where a fixed placement exists."""
    category = spec["category"]
    if category == "figure":
        y = (119 - 159) * 8 if spec["id"] == "figure.vexis" else (28 - max(0, 108 - spec["height"])) * 8
        return 32 % 3, y % 3
    if category == "shelf":
        return 560 % 3, 120 % 3
    if category == "counter":
        return 0, 952 % 3
    if category == "sheet":
        return 1640 % 3, 132 % 3
    return 0, 0


def cells3(image, alpha_min=0.5, phase=(0, 0)):
    """Majority colour and coverage in exact 3x3 cells; no smoothing or dithering."""
    data = np.asarray(image, dtype=np.uint8)
    h, w = data.shape[:2]
    px, py = phase
    right, bottom = (-(w + px)) % PITCH, (-(h + py)) % PITCH
    # Texture boundary pixels are extended for an opaque canvas and left empty
    # for a cutout. The leading/trailing incomplete display cell is cropped.
    if np.all(data[:, :, 3] == 255):
        padded = np.pad(data, ((py, bottom), (px, right), (0, 0)), mode="edge")
    else:
        padded = np.pad(data, ((py, bottom), (px, right), (0, 0)), mode="constant")
    packed = ((padded[:, :, 0].astype(np.uint32) << 16)
              | (padded[:, :, 1].astype(np.uint32) << 8) | padded[:, :, 2])
    packed = np.where(padded[:, :, 3] >= 128, packed + 1, 0)
    colours, inverse = np.unique(packed, return_inverse=True)
    hh, ww = packed.shape
    rows, cols = hh // PITCH, ww // PITCH
    indexed = inverse.reshape(hh, ww).astype(np.uint16)
    blocks = indexed.reshape(rows, PITCH, cols, PITCH).transpose(0, 2, 1, 3).reshape(rows, cols, 9)
    counts = np.zeros((rows, cols, len(colours)), dtype=np.uint8)
    yy, xx = np.indices((rows, cols))
    for i in range(9):
        counts[yy, xx, blocks[:, :, i]] += 1
    empty_index = np.flatnonzero(colours == 0)
    coverage = np.full((rows, cols), 9, dtype=np.uint8)
    if len(empty_index):
        coverage -= counts[:, :, empty_index[0]]
        counts[:, :, empty_index[0]] = 0
    winner = counts.argmax(axis=2)
    centre = blocks[:, :, 4]
    winner = np.where((counts[yy, xx, centre] == counts.max(axis=2)) & (colours[centre] != 0), centre, winner)
    packed_out = colours[winner]
    visible = (coverage >= alpha_min * 9) & (packed_out != 0)
    rgb = np.maximum(packed_out.astype(np.int64) - 1, 0).astype(np.uint32)
    small = np.stack(((rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255, visible.astype(np.uint32) * 255), axis=2).astype(np.uint8)
    small[~visible] = 0
    out = small.repeat(3, axis=0).repeat(3, axis=1)[py:py + h, px:px + w]
    return Image.fromarray(out)


def prepare(spec, selected, original):
    factor = 8 if spec["category"] in WORLD else 4
    w, h = (v * factor for v in spec["size"])
    fit = spec.get("fit", "sprite")
    keyed = pixelize.flat_alpha(original, spec)
    box = (pixelize._cropped_to_aspect(keyed, *spec["size"], selected.get("offset_y", 0))
           if fit in ("canvas", "frame") else pixelize._content_box(keyed))
    cropped = keyed.crop(box)
    snapped = pixelize.snap_areas(cropped, sorted(artlib.allowed_colours(spec, MANIFEST)), artlib.hinted_colours(spec))
    snapped = pixelize.cap_colours(snapped, spec.get("max_colours", 64))
    recolours = selected.get("recolours", selected.get("recolour", []))
    if recolours:
        snapped = pixelize.recolour(snapped, recolours, artlib.allowed_colours(spec, MANIFEST))
    filled = 0
    if fit == "strip":
        data = np.array(snapped)
        empty = data[:, :, 3] == 0
        filled = int(empty.sum())
        if filled:
            near = distance_transform_edt(empty, return_distances=False, return_indices=True)
            data[empty] = data[near[0][empty], near[1][empty]]
            snapped = Image.fromarray(data)
    canvas = Image.new("RGBA", (w, h))
    x = y = 0
    if fit in ("canvas", "frame", "strip"):
        tw, th = w, h
    elif fit == "figure":
        th = spec["height"] * factor
        tw = min(w, round(snapped.width * th / snapped.height))
        x, y = (w - tw) // 2, h - th
    elif fit == "shelf":
        tw, th = w, round(snapped.height * w / snapped.width)
        temporary = snapped.resize((tw, th), NEAREST)
        y = spec["board_row"] * factor - pixelize._board_top(temporary)
    else:
        m = (spec.get("margin") or 0) * factor
        scale = min((w - 2 * m) / snapped.width, (h - 2 * m) / snapped.height)
        tw, th = max(1, round(snapped.width * scale)), max(1, round(snapped.height * scale))
        x = (w - tw) // 2
        y = 0 if spec.get("hangs") else (h - th) // 2
        if spec.get("baseline") is not None:
            y = (spec["baseline"] + 1) * factor - th
    canvas.alpha_composite(snapped.resize((tw, th), NEAREST), (x, y))
    if spec["background"] == "opaque":
        canvas.putalpha(255)
    transform = {"scale_x": tw / snapped.width, "scale_y": th / snapped.height,
                 "translate_x": x - box[0] * tw / snapped.width, "translate_y": y - box[1] * th / snapped.height}
    return canvas, {"crop": list(box), "source_transform": transform, "recolours": recolours,
                    "filled_strip_edge_pixels": filled, "physical_scale_from_manifest": factor}


def work(job):
    spec, row, archive, cache, overrides = job
    original_path = Path(row["source"])
    override_path = Path(overrides) / (spec["id"].replace(".", "_") + ".png")
    source = override_path if override_path.is_file() else original_path
    source_hash = sha(source)
    key = spec["id"].replace(".", "_")
    cached = Path(cache) / f"{key}.png"
    cached_meta = Path(cache) / f"{key}.json"
    spec_hash = hashlib.sha256(json.dumps({k: v for k, v in spec.items() if k not in ("prompt", "description", "avoid", "details")}, sort_keys=True).encode()).hexdigest()
    old = json.loads(cached_meta.read_text(encoding="utf-8")) if cached_meta.is_file() else {}
    if (cached.is_file() and old.get("source_sha256") == source_hash and old.get("spec_sha256") == spec_hash and old.get("pipeline_version") == VERSION):
        master = Image.open(cached).convert("RGBA")
        metadata = old
    else:
        original = Image.open(source).convert("RGBA")
        master, metadata = prepare(spec, row["selection"], original)
        metadata.update(pipeline_version=VERSION, source_sha256=source_hash, spec_sha256=spec_hash,
                        original_size=list(original.size))
        cached.parent.mkdir(parents=True, exist_ok=True)
        master.save(cached)
        write_json(cached_meta, metadata)
    phase = fixed_phase(spec)
    protection_file = OUTPUT / "face_protection.json"
    protections = json.loads(protection_file.read_text(encoding="utf-8")) if protection_file.is_file() else {}
    protection = protections.get("assets", protections).get(spec["id"])
    detail_cells = 0
    if protection:
        import face_preserve
        master = face_preserve.protected_master(master, Image.open(source).convert("RGBA"),
                                               metadata["source_transform"], protection, MANIFEST)
    finished = cells3(master, spec.get("alpha_min", 0.5), phase)
    if protection:
        finished, detail_cells = face_preserve.protected_cells(finished, master, protection, phase)
    relative = Path(spec["output"]).relative_to("art/generated")
    output = OUTPUT / "assets" / relative
    output.parent.mkdir(parents=True, exist_ok=True)
    finished.save(output)
    if protection and protection.get("selected_candidate_sha256"):
        if sha(output) != protection["selected_candidate_sha256"]:
            raise ValueError(f"Production differs from visually selected face candidate: {spec['id']}")
    data = np.asarray(finished)
    opaque = data[:, :, 3] != 0
    if not opaque.any():
        raise ValueError(f"Empty produced image: {spec['id']}")
    if spec["background"] == "opaque" and not np.all(opaque):
        raise ValueError(f"Nonopaque canvas: {spec['id']}")
    result = {"id": spec["id"], "category": spec["category"], "title": spec["title"],
              "status": "produced_pending_review", "source_size": spec["size"], "physical_size": list(finished.size),
              "file": output.relative_to(ROOT).as_posix(), "pixel_pitch": 3, "grid_phase": list(phase),
              "batch": row["batch"], "candidate": row["selection"]["candidate"],
              "original_source": original_path.relative_to(Path(archive)).as_posix(),
              "original_source_sha256": sha(original_path), "has_face_override": source != original_path,
              "override_name": override_path.name if source != original_path else None,
              "output_sha256": sha(output), "colours": len(np.unique(data[opaque, :3], axis=0)), **metadata}
    if protection:
        result.update(face_protection=protection, face_protection_version=1, protected_detail_cells=detail_cells)
    write_json(OUTPUT / "records" / f"{key}.json", result)
    return spec["id"], result["colours"], result["physical_size"]


def rebuild_catalog(selected, all_assets):
    approval_path = OUTPUT / "approval.json"
    approval = json.loads(approval_path.read_text(encoding="utf-8")) if approval_path.is_file() else {}
    approved_assets = approval.get("assets", {})
    result = {}
    for entry in all_assets:
        record = OUTPUT / "records" / (entry["id"].replace(".", "_") + ".json")
        if entry["status"] == "retained":
            result[entry["id"]] = entry
        elif record.is_file():
            result[entry["id"]] = json.loads(record.read_text(encoding="utf-8"))
        else:
            result[entry["id"]] = {"id": entry["id"], "category": entry["category"], "status": "pending"}
        row = result[entry["id"]]
        fingerprint = row.get("output_sha256")
        if entry["status"] == "retained":
            fingerprint = sha(ROOT / entry["file"])
        row["user_approval"] = "approved" if fingerprint and approved_assets.get(entry["id"]) == fingerprint else "pending"
        if row["status"].startswith("produced_"):
            row["status"] = "produced_approved" if row["user_approval"] == "approved" else "produced_pending_review"
            write_json(record, row)
    write_json(OUTPUT / "catalog.json", {"version": 1, "reference_height": 1440, "reference_width": 2560,
                                        "pixel_pitch": 3, "outlines": False,
                                        "user_review": "approved" if all(r["user_approval"] == "approved" for r in result.values()) else "pending",
                                        "assets": result})
    return {status: sum(v["status"] == status for v in result.values()) for status in sorted({v["status"] for v in result.values()})}


def derive_key_shards():
    parent_record = OUTPUT / "records/story_key_whole.json"
    if not parent_record.is_file():
        return
    parent = json.loads(parent_record.read_text(encoding="utf-8"))
    whole = Image.open(ROOT / parent["file"]).convert("RGBA")
    pixels = np.asarray(whole)
    h, w = pixels.shape[:2]
    steps = [0, 0, 0, 1, 1, 1, 0, 0, -1, -1, 0, 0, 1, 1, 2, 2, 1, 0, 0, -1, -1, 0, 0, 0]
    parts = [np.zeros_like(pixels) for _ in range(4)]
    for y in range(0, h, 3):
        old_y = min(23, int((y + 1.5) / 4))
        cuts = [26 + steps[old_y], 50 + steps[(old_y + 8) % 24],
                74 + steps[(old_y + 16) % 24] if old_y < 16 else 72]
        for x in range(0, w, 3):
            n = sum((x + 1.5) / 4 >= cut for cut in cuts)
            parts[n][y:y + 3, x:x + 3] = pixels[y:y + 3, x:x + 3]
    merged = Image.new("RGBA", whole.size)
    for n, data in enumerate(parts, 1):
        asset_id = f"story.key_shard_{n}"
        output = OUTPUT / f"assets/story/key_shard_{n}.png"
        image = Image.fromarray(data)
        image.save(output)
        merged.alpha_composite(image)
        colours = data[data[:, :, 3] != 0, :3]
        record = {**parent, "id": asset_id, "title": f"Aldric Key shard {n}", "candidate": "derived",
                  "file": output.relative_to(ROOT).as_posix(), "output_sha256": sha(output),
                  "derived_from": "story.key_whole", "colours": len(np.unique(colours, axis=0)),
                  "derivation": "Disjoint jagged cuts aligned to complete3px cells; four pieces reconstruct exact RGBA whole."}
        write_json(OUTPUT / "records" / (asset_id.replace(".", "_") + ".json"), record)
    assert merged.tobytes() == whole.tobytes(), "Key pieces must exactly reconstruct the whole"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--archive", type=Path, default=ROOT.parent / "records")
    parser.add_argument("--review-work", type=Path, default=ROOT.parent / "records/density3_review_20261104")
    parser.add_argument("--category", action="append")
    parser.add_argument("--id", action="append")
    parser.add_argument("--exclude", action="append", default=[])
    parser.add_argument("--workers", type=int, default=4)
    parser.add_argument("--catalog-only", action="store_true")
    args = parser.parse_args()
    selected, all_assets = inventory(args.archive)
    write_json(args.review_work / "inventory.json", {"assets": all_assets, "total": len(all_assets), "reprocess": len(selected)})
    specs = [s for s in MANIFEST["assets"] if s["id"] in selected and selected[s["id"]]["source"]
             and (not args.category or s["category"] in args.category)
             and (not args.id or s["id"] in args.id) and s["id"] not in args.exclude]
    if not args.catalog_only:
        jobs = [(s, selected[s["id"]], str(args.archive), str(args.review_work / "masters"), str(args.review_work / "source_overrides")) for s in specs]
        with ProcessPoolExecutor(max_workers=args.workers) as pool:
            futures = [pool.submit(work, j) for j in jobs]
            for n, future in enumerate(as_completed(futures), 1):
                asset_id, colours, size = future.result()
                print(f"{n}/{len(jobs)} {asset_id}: {size[0]}x{size[1]}, {colours} colours", flush=True)
    derive_key_shards()
    print(json.dumps(rebuild_catalog(selected, all_assets)), flush=True)


if __name__ == "__main__":
    main()
