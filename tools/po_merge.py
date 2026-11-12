#!/usr/bin/env python3
"""Brings the translations in locale/ up to the template (0.36), as a gettext editor's "Update from POT" would:

    python3 tools/po_merge.py                 # every <locale>.po: the template's order and comments, old translations kept
    python3 tools/po_merge.py --new           # prints the texts no translation has yet, as JSON (msgid, and its plural)
    python3 tools/po_merge.py --add FILE.json # adds translations: {"fr": {"msgid": "msgstr" or ["one", "many"]}, "ja": ...}

Texts the template no longer has are dropped; a text with no translation is left empty (it shows in English, and
`tools/loc -- verify` lists it). Run `dotnet run --project tools/loc -- extract` first.
"""
import json
import os
import re
import sys

LOCALE = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "locale")


def unquote(text):
    return json.loads(text)


def quote(text):
    return json.dumps(text, ensure_ascii=False)


def parse(path):
    """A .po or .pot as (header lines, entries): an entry is its comments, msgid, msgid_plural and msgstr(s)."""
    entries, header = [], []
    cur = None
    field = None
    with open(path, encoding="utf-8") as stream:
        lines = stream.read().split("\n")
    seen_first = False
    for line in lines:
        if line.startswith("#~"):
            continue
        if line.startswith("#"):
            if cur is None or "msgid" in cur:
                if cur is not None:
                    entries.append(cur)
                cur = {"comments": []}
            cur["comments"].append(line)
            field = None
            continue
        if line.startswith("msgid_plural "):
            cur["msgid_plural"] = unquote(line[len("msgid_plural "):])
            field = "msgid_plural"
            continue
        if line.startswith("msgid "):
            if cur is not None and "msgid" in cur:
                entries.append(cur)
                cur = None
            if cur is None:
                cur = {"comments": []}
            cur["msgid"] = unquote(line[len("msgid "):])
            field = "msgid"
            continue
        match = re.match(r"(msgstr(?:\[\d\])?) (.*)", line)
        if match:
            field = match.group(1)
            cur[field] = unquote(match.group(2))
            continue
        if line.startswith('"') and field:
            cur[field] += unquote(line)
    if cur is not None and "msgid" in cur:
        entries.append(cur)
    head = next((e for e in entries if e["msgid"] == ""), None)
    return head, [e for e in entries if e["msgid"] != ""]


def emit(key, text):
    """A field as the files write it: on one line if it fits in 79 columns, else opened empty with the text under
    it, a line for each line of the text."""
    line = f"{key} {quote(text)}"
    if len(line) <= 79 and "\n" not in text.rstrip("\n"):
        return [line]
    pieces = text.split("\n")
    parts = [piece + "\n" for piece in pieces[:-1]] + ([pieces[-1]] if pieces[-1] else [])
    return [f'{key} ""'] + [quote(part) for part in parts]


def translated(entry):
    return any(v for k, v in entry.items() if k.startswith("msgstr"))


def write(path, top, head, pot_entries, old, added, forms):
    out = list(top)
    out.append('msgid ""')
    out.append('msgstr ""')
    for line in head["msgstr"].split("\n"):
        if line:
            out.append(quote(line + "\n"))
    out.append("")
    missing = 0
    for entry in pot_entries:
        out.extend(c for c in entry["comments"] if c.startswith("#:"))
        out.extend(emit("msgid", entry["msgid"]))
        have = old.get(entry["msgid"])
        new = added.get(entry["msgid"])
        if "msgid_plural" in entry:
            out.extend(emit("msgid_plural", entry["msgid_plural"]))
            texts = [have.get(f"msgstr[{i}]", "") for i in range(forms)] if have and translated(have) else None
            if new is not None:
                texts = new if isinstance(new, list) else [new] * forms
            if texts is None:
                texts = [""] * forms
            texts = (texts + [texts[-1]] * forms)[:forms]
            for i, text in enumerate(texts):
                out.extend(emit(f"msgstr[{i}]", text))
            missing += not any(texts)
        else:
            text = new if new is not None else (have.get("msgstr", "") if have else "")
            if isinstance(text, list):
                text = text[0]
            out.extend(emit("msgstr", text))
            missing += not text
        out.append("")
    with open(path, "w", encoding="utf-8") as stream:
        stream.write("\n".join(out))
    return missing


def main():
    args = sys.argv[1:]
    _, pot = parse(os.path.join(LOCALE, "keywright.pot"))
    codes = sorted(f[:-3] for f in os.listdir(LOCALE) if f.endswith(".po"))
    if "--new" in args:
        _, entries = parse(os.path.join(LOCALE, codes[0] + ".po"))
        known = {e["msgid"] for e in entries if translated(e)}
        fresh = [{k: e[k] for k in ("msgid", "msgid_plural") if k in e} for e in pot if e["msgid"] not in known]
        print(json.dumps(fresh, ensure_ascii=False, indent=1))
        return
    added = {}
    if "--add" in args:
        with open(args[args.index("--add") + 1], encoding="utf-8") as stream:
            added = json.load(stream)
    for code in codes:
        path = os.path.join(LOCALE, code + ".po")
        with open(path, encoding="utf-8") as stream:
            top = []
            for line in stream:
                if not line.startswith("#"):
                    break
                top.append(line.rstrip("\n"))
        head, entries = parse(path)
        forms = int(re.search(r"nplurals=(\d)", head["msgstr"]).group(1))
        old = {e["msgid"]: e for e in entries}
        missing = write(path, top, head, pot, old, added.get(code, {}), forms)
        print(f"{code}.po: {len(pot)} texts, {missing} without a translation, {len([e for e in entries if e['msgid'] not in {p['msgid'] for p in pot}])} dropped")


if __name__ == "__main__":
    main()
