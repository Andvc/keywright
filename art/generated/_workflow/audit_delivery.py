"""Check upload scope, English-only text, PNG metadata, and native output sizes."""
from pathlib import Path
import json
import re
import subprocess

from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
CJK = re.compile(r"[\u2e80-\u2fff\u3040-\u30ff\u3100-\u312f\u3400-\u4dbf\u4e00-\u9fff\uf900-\ufaff\U00020000-\U000323af]")
errors = []
files = sorted((ROOT / "art/generated").rglob("*"))
for path in files:
    if not path.is_file() or "__pycache__" in path.parts:
        continue
    rel = path.relative_to(ROOT).as_posix()
    if not rel.isascii():
        errors.append("Non-ASCII file name: " + rel)
    if path.suffix.lower() == ".png":
        with Image.open(path) as img:
            if CJK.search(str(img.info)):
                errors.append("Non-English PNG metadata: " + rel)
    elif path.suffix.lower() in {".md", ".json", ".jsonl", ".txt", ".py"}:
        content = path.read_text("utf-8")
        if CJK.search(content):
            errors.append("CJK content: " + rel)

staged = subprocess.check_output(["git", "diff", "--cached", "--name-only", "-z"], cwd=ROOT).decode("utf-8").split("\0")
for path in filter(None, staged):
    if not path.startswith("art/generated/"):
        errors.append("Staged file outside delivery scope: " + path)

manifest = json.loads((ROOT / "art/manifest.json").read_text("utf-8"))
delivered = 0
for spec in manifest["assets"]:
    path = ROOT / spec["output"]
    if path.exists():
        delivered += 1
        with Image.open(path) as img:
            if list(img.size) != spec["size"]:
                errors.append("Wrong native size: " + spec["id"])

report = {"files_checked": sum(p.is_file() and "__pycache__" not in p.parts for p in files),
          "assets_delivered": delivered, "errors": errors,
          "image_text_review": "Visual review is required separately; PNG metadata is checked here."}
(ROOT / "art/generated/_workflow/upload_audit.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print(json.dumps(report))
raise SystemExit(bool(errors))
