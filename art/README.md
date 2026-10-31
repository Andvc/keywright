# Keywright art: the brief for generating it

This folder is a complete brief for Keywright's art. A generation workflow (Codex with its built-in image tool) reads it, makes
the assets, checks each one with the tools here, and pushes the results; the game builds `assets/` from what it
delivers.

**Round 2 (0.25) is the current job.** Round 1 (0.23) came out far too busy to look at for hours. Round 2 redraws it
in the selected C2 style at 4x, with useful detail and no outlines, and builds the shop scene from layers: a sky for each time of day, and for each shop the place across
the counter, the shop's frame, its display shelf, its counter, its appraisal sheet and its lanterns, with props for
what you've bought and full-length figures for everyone who comes to the counter. **Start with
[`ART_DIRECTION.md`](ART_DIRECTION.md)** ("Why round 2" and "The shop scene" first), then
[`manifest.json`](manifest.json) (every asset, its spec and its prompt). [`INVENTORY.md`](INVENTORY.md) is the same
list as tables.

## For the owner: remaining round 2 and integration authorized

The style study is complete. The owner chose **C2 at 4x, with detail allowed** on 2026-10-30, then corrected it to
**no outlines** on 2026-10-31 and requested rollback of the outlined batch.
[study/CHOICE.md](study/CHOICE.md) records the visual reference, detail budgets, colour caps and measured quiet limits.

Batch 9r was reverted. **Batch 9s delivers the same 13 anchors without drawn or automatic outlines; all pass.**
Start with the [counter preview](generated/_review/round2_batch9s_playview.png),
[small objects](generated/_review/round2_batch9s_sprites.png) and
[review guide](generated/_review/round2_batch9s_README.md).
The owner approved these anchors on 2026-10-31 ("可以，下一步做吧"). **Batch 10 now delivers all 32 remaining
scene assets, with zero validation errors or warnings.** Review the [four shops](generated/_review/round2_batch10_shops.png),
[props](generated/_review/round2_batch10_props.png), [story scenes](generated/_review/round2_batch10_stories.png)
and [batch-10 guide](generated/_review/round2_batch10_README.md).
The owner then approved this work and requested completion of everything: **"大致ok接着完成所有内容"**.
Continue batches 11-15 and the game integration documented below. Batch 11 now delivers all 37 remaining
figures, including the identity reveals, with zero errors or warnings; see its
[review guide](generated/_review/round2_batch11_README.md). Batch 12 now delivers all 40 remaining inventory
objects and Heartsongs, with zero errors or warnings; see the [object review guide](generated/_review/round2_batch12_README.md).
Batches 13-15 and integration are next.
The original study pictures remain available under `study/review/`; do not regenerate them using the new anchors.

## What's here

| Path | What it is |
|---|---|
| `ART_DIRECTION.md` | The style bible: why round 2, the look, the shop scene's layers, how to draw for the pipeline, the technical canon, how quiet is measured, each category's rules, generating, the review checklist |
| `manifest.json` | 267 assets. Each has: `id`, `category`, `title`, `size` (native pixels), `background`, `max_colours`, `calm` (the most lone specks, edges and brightness step allowed), `value` (a layer's brightness range), `fit` (how the pixelizer fits it), category extras (`height` for figures, `board_row` for shelves, `baseline`), `outline`, `margin`, `priority`, `batch`, `redo`, `output`, `replaces`, `description`, `details`, `palette`, `avoid`, `context`, `prompt` (assembled, ready to send), `gen_size`, `mode`. Its `scene` section has the shop scene's layout |
| `INVENTORY.md` | The manifest as tables |
| `palette/` | The master palette: `keywright.hex` (74 colours), `.gpl` (GIMP/Aseprite), `.png` (a swatch to pass to the model), `ramps.md` |
| `reference/scene/` | The shop scene with placeholder layers (the layout, not the look): `layers.png` (each layer, back to front), `counter_layout.png` (every line and covered area), `times_of_day.png`, and `flat_to_pixels.png` (what to draw and what the pipeline makes of it) |
| `reference/quiet/` | `chest_drawn_4x.png` (the chest the game draws in code: how simple a sprite should be) and `busy_and_quiet.png` (the old art beside round 1's) |
| `reference/current/`, `reference/screens/` | The art before round 1, and in-game screenshots from round 1: for **what** each thing is, never for style |
| `tools/` | `pixelize.py` (raw image to finished asset; `--mode flat` for round 2), `validate.py` (checks, report, contact sheets and the counter mock-ups), `scene.py` (the scene's layout and how the layers stack), `build_manifest.py` and `assets_*.py` (the source of the manifest), `make_references.py`, `make_palette.py`, `artlib.py`. Python 3 and Pillow only |
| `study/` | The style study: four styles at three levels of detail, its prompts and review pictures (`study/README.md`) |
| `generated/` | The delivered art (round 1's until round 2 replaces it), `report.json`, `_review/`, `DELIVERY.md` and `_workflow/` (round 1's own scripts, kept as a record) |

## Round 2: what to make, in what order

Round 2 makes **every asset whose `redo` is true** (242 of them). It keeps round 1's UI frames, effects, logo, app icons
and store art (25 assets). The chest is drawn in code, and round 1's single shop backgrounds and portraits are replaced by
the scene's layers and the figures.

| Batch | What | Assets |
|---|---|---|
| 9 | **Anchors**: `sky.midday`, the stall's `place`, `frame`, `shelf`, `counter`, `sheet` and lantern, `figure.hob`, `item.dwarf_gold_crowns`, `heartsong.lullaby_box`, `curio.monocle`, `icon.upgrade.lamp`, `icon.kind.coins`. **Stop here for the owner's approval** (done: delivered and reviewed) | 13 |
| S | **The style study**: completed; owner chose **C2 at 4x, detail allowed**. See `study/CHOICE.md` | 16 prompts |
| 9r | Reverted: the owner rejected the outlines | 13 |
| 9s | **Approved 2026-10-31: C2 at 4x with detail, without outlines**, through the 0.25 pipeline with automatic outlines disabled | 13 |
| 10 | **Delivered 2026-10-31:** the other skies, places, frames, shelves, counters, sheets and lanterns, the counter's props, and the four story scenes; all pass | 32 |
| 11 | **Delivered:** the other figures, including Mara and the King; all pass | 37 |
| 12 | **Delivered:** things in chests and Heartsongs; all pass | 40 |
| 13 | Curios | 78 |
| 14 | Upgrade and kind icons, the Guild's emblem, the Aldric Key and its shards | 39 |
| 15 | Lock faces (P2) | 3 |

Batch 9 decides what everything else looks like, so it waits for a person: push it with the review pictures and stop
until the owner approves it (or asks for changes: then redo it and stop again). Every later batch is held to its
anchors.

## Making one asset

1. **Prompt.** Send the asset's `prompt` as it is. It asks for a **C2 cartoon illustration without outlines, never a pixel grid**, on flat
   magenta. If an attempt fails, add a correction at the end rather than rewriting it ("retain the C2 shapes and required details, remove contour strokes"; restate the specific layer geometry), and record the
   change (see Delivery).
2. **Request.** Size `gen_size` (1536x1024 for scenes, 1024x1536 for figures and sheets, 1024x1024 for the rest), PNG,
   the highest quality, 2-4 candidates; keep the best C2 match that preserves the required features and fits the layout.
3. **References**: attach `palette/keywright.png` and the approved `study/raw/C2_A.webp` (or its local full-size
   PNG), using C2 for colour, shapes and material detail, not its outlines or whole-scene layout. For batch 9s use the category's
   exact layer geometry; `reference/scene/counter_layout.png` explains placement only. After owner approval, use the
   rebuilt category anchor and the stall's same-kind layer for later shops. For `figure.king`, also pass the new
   `figure.aldo`. Batch 9r sources may be edit targets for removing outlines while preserving geometry and details.
   Never use round 1 or the superseded plain batch-9 anchors as a style reference.
4. **Pixelize**: `python3 art/tools/pixelize.py raw.png --id <id> --no-outline --preview`. In `flat` mode it keys out the magenta,
   fits the picture to the canvas the way its category says, gives each art pixel the commonest colour of its block,
   snaps to the palette, quiets stray specks and caps the colours, then prints the asset's quiet
   measures. For a whole picture (a sky, a place, the frame, a scene), `--offset-y N` moves the 16:9 crop window down N
   source pixels; for a magenta that isn't pure, `--key '#rrggbb'`. If a material lands in the wrong ramp (an earth road
   gone gold), `--recolour '#from=#to'` swaps one palette colour for another all over the asset; record it.
   Every round-2 redraw has `outline: false`; use `--no-outline` explicitly as well. Inspect source and result:
   disabling the automatic ring does not remove outlines already drawn by the model.
5. **Validate:** `python3 art/tools/validate.py --only <id>`. Fix every failure by regenerating ("too busy" means
   simpler; a layer off its lines means the composition, see `reference/scene/counter_layout.png`). Never blur,
   posterize or smooth a picture to pass. Editing a few pixels by hand is fine.
6. **Review** with `validate.py --sheets`: the contact sheets beside the anchors, and for the scene's layers and figures
   the counter mock-ups (`counter_{shop}.png` at midday with the interface's areas marked, `counter_{shop}_times.png` at
   all four hours), which stack whatever's been delivered with placeholders for the rest. Look at one for a minute:
   nothing should keep pulling at the eye.

## Delivery

- Work on a branch named **`art/round2`**, made from the branch this brief is on (`prototype/0.25` when written).
- Production batches change **`art/generated/`**. The owner's style choice additionally authorizes the brief, its
  source generators, regenerated manifest/inventory and the decision record in `art/study/`; gameplay stays outside scope.
- Commit per batch ("Art round 2, batch 11: figures"), and push after each batch.
- What goes in `art/generated/`:
  - every finished PNG at its manifest `output` path (round 1's are replaced where the path is the same);
  - `report.json` and `_review/` from `python3 art/tools/validate.py --sheets` run last over everything;
  - `DELIVERY.md`: a **Round 2** section with, per batch, what was made, anything skipped or failing (and why), every
    prompt change (id: what and why), and anything you'd want a person to look at;
  - raw generations are optional: only as small `.webp` files under `art/generated/_raw/` (PNG and JPEG there are
    git-ignored).
- **Done** means `python3 art/tools/validate.py --strict` passes (no failures; nothing P1 missing), the counter mock-ups
  look calm at every hour, and `DELIVERY.md` says what's left.

## Changing the brief

The manifest is generated: edit `tools/assets_*.py` (or `build_manifest.py` for the style preamble, the categories'
frames, colour caps and quiet limits, and `scene.py` for the scene's layout) and run
`python3 art/tools/build_manifest.py`. The reference pictures come from `tools/make_references.py`, the palette from
`tools/make_palette.py`. Don't edit `manifest.json` or `INVENTORY.md` by hand.

## Putting it in the game

`tools/import_art.py` builds `assets/` from `art/generated/` (`python3 tools/import_art.py`, then
`godot --headless --path . --import`), falling back to a code-drawn placeholder for anything that isn't there.

**Drop-in as soon as they land** (same paths and sizes as round 1's): things, Heartsongs, curios, upgrade and kind icons,
the Guild's emblem, the Aldric Key, lock faces and story scenes (320x180: the importer doubles them).

**The next code step, once the scene's layers and the figures are delivered** (until then the game keeps round 1's
shop pictures and portraits):

1. The importer builds the new folders into `assets/` (`sky`, `place`, `frame`, `shelf`, `counter`, `sheet`, `props`,
   `figures`), the lanterns' unlit frame (the glass darkened, as `validate.py`'s `_unlit` does), and each figure's face
   crop (the head, from the top of the silhouette) for the queue, the notebook's rows and the character cards.
2. A shop view (replacing `UiKit.MakeShop`) stacks the layers as `art/tools/scene.py` does, at 4x, and lights them for
   the time of day: the morning for a day's first third of customers, midday, then evening for the last ones and the
   day's end, night for the Guild; Nim's mornings are morning. It swaps the sky and crossfades the tint between
   customers, lights the lanterns and draws their glow in the evening and at night.
3. The counter stands the seller's figure behind the counter (`SellerView` without its portrait frame; shorter folk on
   a box, the imp on the counter) with the speech bubble top left over their head and the name in it; Nim on his
   mornings and Vane on the Guild's night stand there too.
4. The display shelf comes from the shelf layer (things on its row 49, Heartsongs hanging above it); the bought tools
   stand at the prop spots beside the chest.
5. The slip is printed on the sheet in dark ink: a dark set of text colours for the slip (rarities darkened for paper).
6. Story scenes stand the figures where the portraits were; the notebook and Bram's tips show their heads and shoulders.
