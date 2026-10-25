"""Stage one complete art batch and shared delivery evidence, excluding local candidates."""
from pathlib import Path
import argparse
import json
import subprocess

ROOT=Path(__file__).resolve().parents[3]
ap=argparse.ArgumentParser()
ap.add_argument("batch",type=int)
args=ap.parse_args()
manifest=json.loads((ROOT/"art/manifest.json").read_text("utf-8"))
specs=[s for s in manifest["assets"] if s["batch"]==args.batch]
missing=[s["id"] for s in specs if not (ROOT/s["output"]).exists()]
if missing:
    raise SystemExit("Incomplete batch: "+", ".join(missing))
subprocess.run(["git","add","-u","--","art/generated"],cwd=ROOT,check=True)
paths=[s["output"] for s in specs]
for ext in ("*.py","*.json"):
    paths.extend(p.relative_to(ROOT).as_posix() for p in (ROOT/"art/generated/_workflow").glob(ext))
paths.extend(p.relative_to(ROOT).as_posix() for p in (ROOT/"art/generated/_review").glob("*.png")
             if not p.name.startswith("candidates_"))
if args.batch==8:
    paths.extend(p.relative_to(ROOT).as_posix() for p in (ROOT/"art/generated/store/final").glob("*.png"))
paths.extend(["art/generated/DELIVERY.md","art/generated/report.json"])
subprocess.run(["git","add","--",*paths],cwd=ROOT,check=True)
print(f"Staged batch {args.batch}: {len(specs)} finished assets and shared evidence.")
