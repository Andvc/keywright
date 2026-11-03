"""Source-based local facial colour retention on the same exact 3px grid.

Used only for reviewed feature rectangles. It never synthesizes facial geometry,
adds outlines, or changes the silhouette. Palette mode stays in the game palette;
source24 is an explicit local-colour exception for review when needed.
"""
import argparse
import json
from pathlib import Path

import numpy as np
from PIL import Image
import artlib
import pixelize


def protected_master(master, source, transform, protection, manifest):
    sx, sy = transform["scale_x"], transform["scale_y"]
    tx, ty = transform["translate_x"], transform["translate_y"]
    aligned = source.transform(master.size, Image.Transform.AFFINE,
                               (1/sx, 0, -tx/sx, 0, 1/sy, -ty/sy), Image.Resampling.NEAREST)
    aligned.putalpha(master.getchannel("A"))
    result = master.copy()
    for region in protection["regions"]:
        box = tuple(region["box"] if isinstance(region, dict) else region)
        mode = region.get("mode", protection.get("mode", "palette")) if isinstance(region, dict) else protection.get("mode", "palette")
        patch = aligned.crop(box)
        if mode.startswith("source24"):
            alpha = patch.getchannel("A")
            patch = patch.convert("RGB").quantize(colors=24, method=Image.Quantize.MEDIANCUT,
                                                    dither=Image.Dither.NONE).convert("RGBA")
            patch.putalpha(alpha)
            if "matched" in mode:
                # Match the dominant source skin tone to the existing skin fill,
                # retaining the original local lip/nose/eye colour differences.
                source_data = np.array(patch)
                old_data = np.array(master.crop(box))
                opaque = source_data[:, :, 3] != 0
                if opaque.any():
                    anchor = region.get("match_anchor") if isinstance(region, dict) else None
                    if anchor:
                        source_anchor = np.array(aligned.crop(tuple(anchor)))
                        old_anchor = np.array(master.crop(tuple(anchor)))
                        valid = old_anchor[:, :, 3] != 0
                        if not valid.any():
                            raise ValueError("Face colour match anchor must contain opaque skin")
                        source_base = np.median(source_anchor[valid, :3], axis=0).astype(np.int16)
                        old_base = np.median(old_anchor[valid, :3], axis=0).astype(np.int16)
                    else:
                        colours, counts = np.unique(source_data[opaque, :3], axis=0, return_counts=True)
                        source_base = colours[counts.argmax()].astype(np.int16)
                        colours, counts = np.unique(old_data[opaque, :3], axis=0, return_counts=True)
                        old_base = colours[counts.argmax()].astype(np.int16)
                    source_data[:, :, :3] = np.clip(source_data[:, :, :3].astype(np.int16) + old_base-source_base,0,255).astype(np.uint8)
                    patch = Image.fromarray(source_data)
        else:
            patch = pixelize.snap(patch, artlib.palette(manifest))
        if isinstance(region, dict) and region.get("dark_features_only") is not None:
            anchor = region.get("match_anchor")
            if not anchor:
                raise ValueError("dark_features_only requires an explicit source skin anchor")
            raw = np.array(aligned.crop(box))
            anchor_data = np.array(aligned.crop(tuple(anchor)))
            valid = anchor_data[:, :, 3] != 0
            anchor_colour = np.median(anchor_data[valid, :3], axis=0)
            weights = np.array([0.2126, 0.7152, 0.0722])
            keep = (raw[:, :, :3] @ weights) <= (anchor_colour @ weights)-float(region["dark_features_only"])
            old = np.array(master.crop(box))
            new = np.array(patch)
            new[~keep] = old[~keep]
            patch = Image.fromarray(new)
        result.paste(patch, box[:2])
    return result


def protected_cells(finished, master, protection, phase):
    """Keep supported dark feature strokes when a 3x3 majority would erase them."""
    result = finished.copy()
    changed = 0
    px, py = phase
    for region in protection["regions"]:
        box = region["box"] if isinstance(region, dict) else region
        mode = region.get("mode", protection.get("mode", "palette")) if isinstance(region, dict) else protection.get("mode", "palette")
        if not mode.endswith("_detail"):
            continue
        x0, y0, x1, y1 = box
        for y in range(((y0 + py + 2)//3)*3-py, y1-2, 3):
            for x in range(((x0 + px + 2)//3)*3-px, x1-2, 3):
                block = master.crop((x, y, x+3, y+3))
                if block.getchannel("A").getextrema() != (255, 255):
                    continue
                current = result.getpixel((x+1, y+1))
                light = lambda c: c[0]*0.2126+c[1]*0.7152+c[2]*0.0722
                supported = [(n, c) for n, c in block.getcolors() if n >= 2 and light(c) < light(current)-22]
                if supported:
                    _, colour = max(supported, key=lambda pair:(pair[0],-light(pair[1])))
                    result.paste(colour, (x,y,x+3,y+3))
                    changed += 1
    return result, changed


def candidate(asset_id, regions, mode, destination):
    import density3
    root = density3.ROOT
    work = root.parent / "records/density3_review_20261104"
    record = json.loads((root / "art/density3/records" / (asset_id.replace('.', '_')+'.json')).read_text(encoding='utf-8'))
    master = Image.open(work / "masters" / (asset_id.replace('.', '_')+'.png')).convert('RGBA')
    source = work / "source_overrides" / (asset_id.replace('.', '_')+'.png')
    if not source.is_file():
        source = root.parent / "records" / record['original_source']
    protection = {"mode":mode,"regions":regions}
    patched = protected_master(master, Image.open(source).convert('RGBA'), record['source_transform'], protection, density3.MANIFEST)
    spec = artlib.asset(density3.MANIFEST,asset_id)
    pixels = density3.cells3(patched, spec.get('alpha_min',0.5), record['grid_phase'])
    pixels, count = protected_cells(pixels, patched, protection, record['grid_phase'])
    destination = Path(destination)
    destination.mkdir(parents=True,exist_ok=True)
    path = destination / (asset_id.replace('.','_')+'_'+mode+'.png')
    pixels.save(path)
    info = {"id":asset_id,**protection,"changed_detail_cells":count,"output":str(path),"sha256":density3.sha(path)}
    (path.with_suffix('.json')).write_text(json.dumps(info,indent=2),encoding='utf-8')
    return info


if __name__ == '__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--id',required=True)
    parser.add_argument('--region',action='append',required=True,help='physical x0,y0,x1,y1')
    parser.add_argument('--mode',choices=['palette','palette_detail','source24','source24_detail','source24_matched','source24_matched_detail'],default='palette')
    parser.add_argument('--out',required=True)
    args=parser.parse_args()
    print(json.dumps(candidate(args.id,[[int(v) for v in r.split(',')] for r in args.region],args.mode,args.out)))
