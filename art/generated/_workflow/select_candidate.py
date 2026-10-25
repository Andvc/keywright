"""Promote a reviewed candidate while preserving its provenance."""
from pathlib import Path
import argparse
import json
import shutil

from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
ap = argparse.ArgumentParser()
ap.add_argument("id")
ap.add_argument("candidate", type=int)
ap.add_argument("--reason", required=True)
args = ap.parse_args()
manifest = json.loads((ROOT / "art/manifest.json").read_text("utf-8"))
spec = next(s for s in manifest["assets"] if s["id"] == args.id)
src = ROOT / f"art/generated/_review/candidates/{args.id}.c{args.candidate}.png"
out = ROOT / spec["output"]
out.parent.mkdir(parents=True, exist_ok=True)
shutil.copyfile(src, out)
provenance = ROOT / f"art/generated/_workflow/candidates/{args.id}.c{args.candidate}.json"
record = json.loads(provenance.read_text("utf-8"))[args.id]
record["selected_candidate"] = args.candidate
record["selection_reason"] = args.reason
path = ROOT / "art/generated/_workflow/generations.json"
records = json.loads(path.read_text("utf-8")) if path.exists() else {}
records[args.id] = record
path.write_text(json.dumps(records, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")
print(args.id, args.candidate)
