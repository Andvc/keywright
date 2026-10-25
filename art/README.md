# Keywright art: the brief for generating it

This folder is a complete brief for replacing Keywright's placeholder art. A generation workflow (Codex with
GPT-Image-2) reads it, makes every asset, checks each one with the tools here, and pushes the results. The game's code
is then changed to use them; that last step is not part of this brief.

**Start with [`ART_DIRECTION.md`](ART_DIRECTION.md)** (the style everything shares), then
[`manifest.json`](manifest.json) (every asset, its spec and its prompt). [`INVENTORY.md`](INVENTORY.md) is the same list
as tables, for reading.

## What's here

| Path | What it is |
|---|---|
| `ART_DIRECTION.md` | The style bible: pixel density, palette, light, outline, perspective, each category's rules, the generation pipeline, the review checklist |
| `manifest.json` | 263 assets. Each has: `id`, `category`, `title`, `size` (native pixels), `background` (transparent/opaque), `max_colours`, `outline`, `margin`, `baseline` (things), `priority`, `batch`, `output` (where to write it), `replaces` (the game file it replaces, or null if new), `description`, `details`, `palette` (ramp:steps), `avoid`, `context` (what it is in the game), `prompt` (assembled, ready to send), `gen_size`, `mode` (grid/area/derive/script), `gen_block` (the art-pixel size the prompt asks for), and category extras (`composition` for portraits, `geometry` for the chest and locks, `frames` for effects) |
| `INVENTORY.md` | The manifest as tables |
| `palette/` | The master palette: `keywright.hex` (74 colours), `.gpl` (GIMP/Aseprite), `.png` (a swatch to pass to the model), `ramps.md` (what each ramp is for) |
| `reference/current/` | Contact sheets of today's placeholder art. Use for **what** each thing is and its rough size; **not** for style (the new style replaces it) |
| `reference/screens/` | In-game screenshots: where each kind of asset appears and how big |
| `reference/layout/` | The counter screen's layout: which parts of a background the UI covers, the counter line (row 238) and the shelf line (row 98) |
| `tools/` | `pixelize.py` (raw image to finished asset), `validate.py` (checks, report, review sheets), `build_manifest.py` and `assets_*.py` (the source of the manifest), `make_palette.py`, `artlib.py`. Python 3 and Pillow only |

## Order of work

| Batch | What | Assets |
|---|---|---|
| 0 | **Style anchors**: one of each category (`bg.shop1`, `portrait.bram`, `item.dwarf_gold_crowns`, `heartsong.lullaby_box`, `curio.monocle`, `chest.wood.lid/body/open`, `icon.upgrade.lamp`, `icon.kind.coins`) | 10 |
| 1 | The other portraits (P1) | 35 |
| 2 | Things in chests and Heartsongs | 40 |
| 3 | Curios | 78 |
| 4 | The other shop backgrounds, chest finishes and lock overlays | 24 |
| 5 | Upgrade and kind icons, the Guild's emblem, the Aldric Key | 39 |
| 6 | P2: the Crown Chest, UI frames, effects, lock faces | 14 |
| 7 | P3 and branding: new UI pieces, logo and app icons, story scenes, two new portraits | 14 |
| 8 | P3: Steam store art (not used in the game) | 9 |

Do batch 0 first and hold every later batch to it. If the workflow can pause for a person, pause after batch 0 with
the review sheets; if it can't, review the anchors against `ART_DIRECTION.md`'s checklist yourself and regenerate until
they pass before going on. Batches 1-6 are the game's needs; 7 and 8 are optional and can wait.

## Making one asset

1. **Prompt.** Send the asset's `prompt` as it is. If an attempt fails, add a correction at the end ("the outline is
   missing on the right"; "no background") rather than rewriting it, and record the change (see Delivery).
2. **Request.** Size `gen_size` (or the nearest the API offers), PNG, the highest quality, a **transparent background**
   when `background` is `transparent` (if the API can't, ask for a flat `#FF00FF` background and pass `--key '#ff00ff'`
   to the pixelizer). Ask for 2-4 candidates and keep the best.
3. **References** (as input images, when the API takes them): the palette swatch `palette/keywright.png`; after batch
   0, the category's approved anchor (`manifest.json`'s `anchors`, at 4x); for a chest finish other than wood, edit the
   wood part (`generation` says so); for `portrait.king`, pass `portrait.aldo`. Today's placeholder
   (`reference_current`) may be passed for the subject's shape, never for style.
4. **Post-process** (`mode`):
   - `grid` (sprites, portraits, the chest, icons): `python3 art/tools/pixelize.py raw.png --id <id> --preview`. It finds
     the art-pixel grid the model drew, snaps colours to the palette, places the content on the canvas (centred, on
     the baseline, or in its box), caps colours and enforces the outline. If it picks the wrong grid, pass `--block N`
     (the art-pixel size in the raw image).
   - `area` (backgrounds, scenes, store art): the same command; it crops to the target's aspect and reduces by area.
     For backgrounds, move the crop window with `--offset-y N` until the counter's top lands on row 238 (check the
     `_layout.png` picture `validate.py --sheets` makes), and regenerate if the shelf (row 98) can't line up too.
   - `derive`: made from other assets, not generated (`generation` says how: the key shards cut from `story.key_whole`;
     the library logo from `brand.logo`).
   - `script`: the UI frames: draw the exact pixels in `description` with a few lines of Pillow.
   - Effects (`frames`): generate the sheet; the pixelizer splits it into equal frames and fits each to `frame_size`.
5. **Validate:** `python3 art/tools/validate.py --only <id>`. Fix every failure (regenerate, or edit pixels by hand:
   that's fine, it's pixel art). Warnings are judgement calls.
6. **Review** it beside its anchor (`validate.py --sheets` writes contact sheets and counter mock-ups to
   `art/generated/_review/`): same light, same outline, same density, reads at 1x, says what the spec says.

## Delivery

- Work on a branch named **`art/gpt-image-2`**, made from the branch this brief is on (`prototype/0.22` when written).
- Change nothing outside **`art/generated/`**. If a spec is wrong or can't be met, make the closest asset and say so.
- Commit per batch ("Art batch 2: things in chests and Heartsongs"), and push after each batch.
- What goes in `art/generated/`:
  - every finished PNG at its manifest `output` path (native size, no scaling);
  - for store art, also the final Steam-size image (`deliver_size`: the native art scaled by `scale`, nearest, with
    `brand.logo` composited where `needs_logo`) under `art/generated/store/final/`;
  - `report.json` and `_review/` from `python3 art/tools/validate.py --sheets` run last over everything;
  - `DELIVERY.md`: per batch, what was made, anything skipped or failing (and why), every prompt change (id: what and
    why), and anything you'd want a person to look at;
  - raw generations are optional: only as small `.webp` files under `art/generated/_raw/` (PNG and JPEG there are
    git-ignored).
- **Done** means `python3 art/tools/validate.py --strict` passes for P1 and P2 (no failures, nothing P1 missing), the
  sheets look like one game, and `DELIVERY.md` says what's left.

## Changing the brief

The manifest is generated: edit `tools/assets_*.py` (or `build_manifest.py` for the shared templates) and run
`python3 art/tools/build_manifest.py`. The palette comes from `tools/make_palette.py`. Don't edit `manifest.json` or
`INVENTORY.md` by hand.

## After delivery: putting it in the game (for the game's side, not the generator)

Today's art is drawn by `tools/custom_art.py` and built into `assets/` by `tools/import_art.py`. The new art replaces
it at these sizes (native, then the game scales by whole numbers):

| Category | New native size | Today | Code that changes |
|---|---|---|---|
| Shop backgrounds | 640x360 | the same | none: drop-in |
| Portraits | 76x76 | 38x38 | `SellerView.FaceScale` 4 to 2, `ScenePhase.FaceScale` 6 to 3, ledger and Bram's tips 2 to 1, the setup screen's cards, the waiting queue and ledger rows use the 38x38 face crop |
| Things, Heartsongs | 32x32 | up to 24x24 | `OpeningPhase.ScaleFor`, the slot sizes in `UiKit.MakeItemIcon` callers (the shelf, exhibition, pawn list, ledger) |
| Curios | 28x28 | 24x24 | none: the importer scales 2x into the same 56x56 slot |
| Chest | 96 wide (lid 42, body 30, open 60) | 16 wide at 12-14x | `ChestView` art size constants and `PixelSize` 2; the lock overlays replace `DrawLidLock`/`DrawBodyLock`; a `crown` skin for the vault |
| Upgrade icons | 24x24 | 16x16 at 3x | each upgrade gets its own (`Upgrade.Icon` = its key); the importer scales 2x into the same 48x48 |
| Kind icons | 16x16 | the same | none: the importer scales 2x |
| Guild emblem, Aldric Key | 24x24; 96x24 (whole and four shards) | 22x24; one 18x10 shard | the dues screen shows shard n for the nth piece |
| Effects | explosion 8 frames of 128x128; coin, sparkle, dust sheets | 6 frames of 112x100; code-drawn | `Fx.Explosion`'s frame count and scale; `Fx.Coin` and particles use the new sheets |
| UI frames | 12x12 at 2x | 8x8 at 3x | `UiKit`'s nine-slice margins |
| Lock faces | 160x160 | vector circles | `LockpickPhase.DrawDial` draws over the face |
| Logo, app icon | 320x72; 16-64 | a text label; the purple chest | the title screen; `assets/ui/icon.png`; the .exe icon in CI |
| Story scenes, Mara and the King unmasked, new UI pieces | 640x360; 76x76 | none | `ScenePhase` backdrops; the vault and the reveal's portraits; the slip and the speech bubble |

`tools/import_art.py` will take `art/generated/` as its source for these instead of drawing them, so `assets/` stays a
build product.
