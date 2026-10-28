# Keywright art: the brief for generating it

This folder is a complete brief for Keywright's art. A generation workflow (Codex with GPT-Image-2) reads it, makes
the assets, checks each one with the tools here, and pushes the results; the game builds `assets/` from what it
delivers.

**Round 2 (0.25) is the current job.** Round 1 (0.23) replaced the old code-drawn placeholders, but it came out far
too busy to look at for hours. Round 2 redraws most of it, quieter. **Start with
[`ART_DIRECTION.md`](ART_DIRECTION.md)** ("Why round 2" first), then [`manifest.json`](manifest.json) (every asset, its
spec and its prompt). [`INVENTORY.md`](INVENTORY.md) is the same list as tables, for reading.

## For the owner: starting round 2

Start a Codex session on this repository, on the branch this brief is on, and give it:

> Follow `art/README.md` for round 2. Make batch 9 (the style anchors) on a branch `art/quiet` made from this one, push
> it with `python3 art/tools/validate.py --sheets` run, and stop: I'll look at `art/generated/_review/` and approve
> the anchors or ask for changes before anything else is made.

Once the anchors are approved, tell it to go on with batches 10-15. When it's done, a Claude session merges
`art/quiet`, re-runs the importer and checks the game.

## What's here

| Path | What it is |
|---|---|
| `ART_DIRECTION.md` | The style bible: why round 2, the pillars and the detail budget, the technical canon, how quiet is measured, each category's rules, the generation pipeline, the review checklist |
| `manifest.json` | 236 assets. Each has: `id`, `category`, `title`, `size` (native pixels), `background` (transparent/opaque), `max_colours`, `calm` (the most lone specks, edges and brightness step allowed), `outline`, `margin`, `baseline` (things), `priority`, `batch`, `redo` (round 2 redraws it), `output` (where to write it), `replaces` (the game file it becomes), `description`, `details`, `palette` (ramp:steps), `avoid`, `context`, `prompt` (assembled, ready to send), `gen_size`, `mode`, `gen_block` (the art-pixel size the prompt asks for), and category extras (`composition` for portraits, `frames` for effects) |
| `INVENTORY.md` | The manifest as tables, with what round 2 redraws |
| `palette/` | The master palette: `keywright.hex` (74 colours), `.gpl` (GIMP/Aseprite), `.png` (a swatch to pass to the model), `ramps.md` (what each ramp is for) |
| `reference/quiet/` | Round 2's references: `chest_drawn_4x.png` (the chest the game draws in code: the level of simplicity to aim for), `busy_and_quiet.png` (the old art beside round 1's: the target is between them), `value_plan_counter.png` (a counter background's light and dark zones under what the screen covers) |
| `reference/current/` | Contact sheets of the art before round 1. Use for **what** each thing is and its rough size; never for style |
| `reference/screens/` | In-game screenshots from round 1: where each kind of asset appears and how big |
| `reference/layout/` | The counter screen's layout at 640x360 (halve the numbers for round 2's 320x180 backgrounds) |
| `tools/` | `pixelize.py` (raw image to finished asset), `validate.py` (checks, report, review pictures), `build_manifest.py` and `assets_*.py` (the source of the manifest), `make_references.py` (the pictures in `reference/quiet/`), `make_palette.py`, `artlib.py`. Python 3 and Pillow only |
| `generated/` | The delivered art (round 1's until round 2 replaces it), `report.json`, `_review/`, `DELIVERY.md` and `_workflow/` (round 1's own scripts, kept as a record: reuse or replace them) |

## Round 2: what to make, in what order

Round 2 redraws **every asset whose `redo` is true** (211 of them: the backgrounds, story scenes, portraits, things,
Heartsongs, curios, upgrade and kind icons, the Guild's emblem, the Aldric Key and the lock faces), each to its new
spec. It keeps round 1's UI frames, effects, logo, app icons and store art (25 assets), which aren't listed in the
batches below. The chest is no longer generated: it's drawn in code (`tools/custom_art.py`).

| Batch | What | Assets |
|---|---|---|
| 9 | **Style anchors**: `bg.shop1`, `portrait.bram`, `item.dwarf_gold_crowns`, `heartsong.lullaby_box`, `curio.monocle`, `icon.upgrade.lamp`, `icon.kind.coins`. **Stop here for the owner's approval** | 7 |
| 10 | The other shop backgrounds and the four story scenes | 7 |
| 11 | Things in chests and Heartsongs | 40 |
| 12 | Curios | 78 |
| 13 | The other portraits (Mara and the King unmasked last: P3) | 37 |
| 14 | Upgrade and kind icons, the Guild's emblem, the Aldric Key and its shards | 39 |
| 15 | Lock faces (P2) | 3 |

Batch 9 decides what everything else looks like, so it waits for a person: push it with the review pictures and stop
until the owner approves it (or asks for changes: then redo it and stop again). Every later batch is held to its
anchors.

## Making one asset

1. **Prompt.** Send the asset's `prompt` as it is. If an attempt fails, add a correction at the end rather than
   rewriting it ("simpler: fewer, larger shapes, flat colour, no texture" is the usual one; "the outline is missing on
   the right"; "no background"), and record the change (see Delivery).
2. **Request.** Size `gen_size` (or the nearest the API offers), PNG, the highest quality, a **transparent background**
   when `background` is `transparent` (if the API can't, ask for a flat `#FF00FF` background and pass `--key '#ff00ff'`
   to the pixelizer). Ask for 2-4 candidates and keep the calmest one that shows what the spec says.
3. **References** (as input images, when the API takes them): the palette swatch `palette/keywright.png`; for batch 9,
   `reference/quiet/chest_drawn_4x.png` for sprites and icons (say it shows the level of simplicity, not the object);
   after batch 9, the category's approved round-2 anchor (`manifest.json`'s `anchors`, at 4x). For `portrait.king`,
   pass round 2's `portrait.aldo` (the same man). **Never pass round 1's art** (anything in `art/generated/` that
   isn't round 2's): its style is what round 2 replaces. The value plan is for checking a background, not for the
   model.
4. **Post-process**: `python3 art/tools/pixelize.py raw.png --id <id> --preview`. Everything in round 2 is `grid` mode:
   it finds the art-pixel grid the model drew, snaps colours to the palette, places the content on the canvas
   (centred, or on the baseline), quiets stray specks, caps colours and enforces the outline, then prints the asset's
   quiet measures and whether they pass. If it picks the wrong grid, pass `--block N` (the art-pixel size in the raw
   image).
   - **Backgrounds and scenes** fill a 1536x1024 image's width: the pixelizer reads 320 cells across and keeps 180
     rows from the middle. Move that window with `--offset-y N` (source pixels, down) until the counter's top lands on
     row 119 and the shelf's on row 49 (check the `bg_shop*_layout.png` that `validate.py --sheets` makes);
     regenerate if both can't line up.
   - `derive`: the key shards, cut from `story.key_whole` along jagged lines (`generation` says how).
5. **Validate:** `python3 art/tools/validate.py --only <id>`. Fix every failure. "Too busy" means **regenerate it
   simpler**. Never blur, posterize, smooth or flatten it to pass: it comes out smeared (0.25 tried it on round 1's
   art). Editing a few pixels by hand is fine; it's pixel art.
6. **Review** it beside its anchor (`validate.py --sheets` writes contact sheets and the four counter mock-ups to
   `art/generated/_review/`): a few flat shapes, the same light, outline and calm as the anchor, readable at 1x, the
   subject the spec describes. For a background, look at its mock-up for a minute: nothing should keep pulling at the
   eye.

## Delivery

- Work on a branch named **`art/quiet`**, made from the branch this brief is on (`prototype/0.25` when written).
- Change nothing outside **`art/generated/`**. If a spec is wrong or can't be met, make the closest asset and say so.
- Commit per batch ("Art round 2, batch 11: things in chests and Heartsongs"), and push after each batch.
- What goes in `art/generated/`:
  - every finished PNG at its manifest `output` path, replacing round 1's (native size, no scaling);
  - `report.json` and `_review/` from `python3 art/tools/validate.py --sheets` run last over everything;
  - `DELIVERY.md`: a **Round 2** section with, per batch, what was made, anything skipped or failing (and why), every
    prompt change (id: what and why), and anything you'd want a person to look at;
  - raw generations are optional: only as small `.webp` files under `art/generated/_raw/` (PNG and JPEG there are
    git-ignored).
- **Done** means `python3 art/tools/validate.py --strict` passes (no failures, quiet included; nothing P1 missing), the
  mock-ups look calm, and `DELIVERY.md` says what's left.

## Changing the brief

The manifest is generated: edit `tools/assets_*.py` (or `build_manifest.py` for the style preamble, the categories'
frames, colour caps and quiet limits) and run `python3 art/tools/build_manifest.py`. The palette comes from
`tools/make_palette.py`, the reference pictures from `tools/make_references.py`. Don't edit `manifest.json` or
`INVENTORY.md` by hand.

## Putting it in the game

`tools/import_art.py` builds `assets/` from `art/generated/` (`python3 tools/import_art.py`, then
`godot --headless --path . --import`). For anything not in `art/generated/` (a new seller, thing or curio, until its
picture is made) it falls back to a code-drawn placeholder at the same size. Round 2 needs no code change: every size is
the game's, except the backgrounds and scenes, which are now 320x180 and which the importer doubles to the 640x360 the
game shows at 2x.

The sizes the game uses (round 1 set them in 0.23; the chest changed in 0.25):

| Category | Native size | Shown |
|---|---|---|
| Shop backgrounds, story scenes | 320x180 (the importer doubles them) | 4x |
| Portraits | 76x76; a 38x38 face crop | 2x at the counter, 3x in scenes, 1x in the ledger and tips |
| Things, Heartsongs | 32x32 | 2x on the table and the shelf; 1x-2x in slots, all at one scale so sizes compare |
| Curios | 28x28 (the importer scales them 2x) | 56 px slots |
| Chest | 48 wide (lid 21, body 15, open 30), drawn in code | 4x (built at 2x, shown at 2x) |
| Upgrade icons | 24x24 (the importer scales them 2x) | 48 px; 1x on the slip |
| Kind icons | 16x16 (the importer scales them 2x) | 32 px |
| Guild emblem, Aldric Key | 24x24; 96x24 (whole and four shards) | 2x and 1x |
| Effects | explosion 8 frames of 128x128; coin, sparkle, dust sheets | 2-4x |
| UI frames | 12x12 | 2x |
| Lock faces | 160x160 | 2x |
| Logo, app icon | 320x72; 16-64 | 2x; the window's and the .exe's icons |

Not used yet: the parchment slip and the white speech bubble (`ui.slip_paper`, `ui.bubble`, `ui.bubble_tail`). They're
light, and every colour the slip and the sellers' lines use is made for the dark panels; using them means a dark set of
text colours for both. The importer builds them into `assets/ui/` all the same.
