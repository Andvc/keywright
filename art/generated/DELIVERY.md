# Keywright art delivery

> **0.25:** round 1's chest was retired: the chest and its locks are drawn in code now (`tools/custom_art.py`), so
> its files, its review sheets and `_workflow/finalize_chests.py` were removed (git has them), with round 1's anchors
> and its counter mock-ups. Round 2 (`art/README.md`) builds the shop scene from layers. Its current direction is
> C2 at 4x, with detail allowed. The latest delivery is batch 9r in the Round 2 section below.

## Historical round-1 delivery

Source brief: `prototype/0.22`, commit `9e52f8380d4873dd2ea0a01fb0ab4704198e72c4`.
Delivery branch: `art/gpt-image-2`. All changes are confined to `art/generated/`.

Generation uses the built-in image generation tool, with GPT-Image-2 requested.
The tool does not return an independently verifiable model identifier.
The original per-asset prompts are in `art/manifest.json`; additions are recorded
in `_workflow/prompt_changes.json`. Source-image hashes and processing choices
are in `_workflow/generations.json`. Full-resolution raw generations are retained
locally outside this checkout. All uploaded names, notes and visible text are English.

## Batch 0: style anchors

Delivered all 10 anchors: Bram's stall, Uncle Bram, Dwarf-Gold Crowns, the Lullaby
Box, Witch's Monocle, the three wood chest parts, Witchlight and the coin-kind icon.
All 10 pass the supplied validator. The counter composition, native-size sprites
and portrait crop were visually reviewed beside the shared style requirements.

Processing repairs: reserve the dark outline when enforcing the colour cap;
register the stall source to shelf row 98, counter row 238 and awning row 44;
register chest sources to their native part envelopes; fill seam/base rows and
lock mounting seats; reuse the exact body in the lower half of the open chest.
These repairs are reproducible in `_workflow/finish_assets.py`.

Accepted warning: the Witchlight is a tall lantern, with a relatively narrow
silhouette (59% of its canvas width). Its flame and handle remain readable.

## Batch 1: character portraits

Delivered all 35 remaining P1 portraits. Each was compared at native size,
enlarged size and in the 38x38 face crop. Two candidates were requested per
portrait; Mother Bramble was regenerated with an explicit elderly-woman
correction after the first attempts inherited the male style anchor's beard.
Quill's interrupted candidate was retried. All 45 delivered assets pass validation.

Portrait reference instructions preserve the original manifest prompt and explain
that Bram supplies framing, lighting and pixel density only. The exact appended
instructions and candidate choice are recorded per asset in
`_workflow/generations.json`; additional correction prompts are recorded in
`_workflow/prompt_changes.json`.

The Dwarf-Gold Crowns anchor was also fitted to its specified 20-24 pixel content
envelope. Item processing now respects each asset's size hint before baseline
placement, so small coins and large weapons retain their relative size.

## Batch 2: things in chests and Heartsongs

Delivered all 40 assets in this batch: 32 additional chest items and eight
Heartsongs, completing the item and Heartsong categories with their anchors.
Two successful candidates per asset were compared after retrying two transport
failures. All 85 finished assets pass the supplied validator.

Appended reference instructions identify the palette and category anchor as
style references only. The per-item size hint is repeated in the instruction
and enforced during processing, then the content is placed on baseline row 30.
The exact instructions and chosen candidate are in `_workflow/generations.json`.
The native-size review checks include coin material, weapon silhouette, cloak
shape, bottle shape, book emblems, gems and each Heartsong's distinctive detail.

## Batch 3: Curios

Delivered all 78 Curios, completing the category with its Monocle anchor.
Two successful candidates per asset were compared at 28x28 and enlarged size;
interrupted requests were retried. The approved warm storybook pixel style was
retained throughout. The silver tongue charm was recoloured from skin-like
highlights to the steel ramp. The errand notebook received distinct checkboxes
and a pencil after reduction softened those small details.

## Batch 4: shops, chest finishes and locks

Delivered all 24 assets: three shop backgrounds, five sets of chest finishes
and six lock overlays. Shelf surfaces are registered to row 98 and counter
surfaces to row 238. Final source landmark values are recorded per background
in `_workflow/generations.json`; the counter mock-ups were visually checked.

Every chest finish shares the wood template's alpha silhouette. Closed parts
meet on full-width seams, lock overlays fit their mounting seats, and each open
chest uses the exact finished body pixels in its lower half. The wood anchor's
open image was refreshed to enforce that same byte-for-byte relationship.
The black chest's central seats carry crossed chains instead of empty plates.

## Batch 5: icons, Guild emblem and Aldric Key

Delivered all 39 assets, including 26 upgrade icons, seven kind icons, the Guild
emblem, the whole Aldric Key and four derived shards. All generated subjects
had two successful candidates. The shards retain their positions on the 96x24
canvas, use jagged breaks near x26, x50 and x74, and have outlined break edges.
They do not overlap and reconstruct the delivered whole key exactly.

## Batch 6: Crown Chest, UI, effects and lock faces

Delivered all 14 assets. The four UI pieces follow the manifest's exact pixel
recipes. The three lock faces retain a plain dark interaction annulus from
radius 52 through 68 around (80,80). Effects are equal-cell strips; common
sampling preserves relative expansion and rotation across frames. The four
sparkle motifs were restored by hand after reduction lost the smallest mark.
The Crown Chest uses the shared silhouette and exact body reuse. Pixels newly
exposed by applying the shared mask are snapped back to the master palette.

## Batch 7: branding and story assets

Delivered all 14 assets: three scripted UI pieces, the English Keywright logo,
four independently generated app-icon sizes, four story scenes and the two new
portraits. The King uses Aldo as his identity reference. The 64px and 48px icon
keys were polished to the gold ramp, and the 16px key was simplified by hand.
Gold is reserved during colour reduction so the small key remains visible.

## Batch 8: Steam art

Delivered all nine native assets plus all nine required Steam-size exports
under `store/final/`. Exports use the specified integer nearest-neighbour scale.
The approved English logo is composited at an integer scale in the requested
areas; the header and main capsule use a smaller logo to preserve faces.
The library logo is derived from the approved brand logo on transparency.

The library hero received a third candidate to concentrate both sellers, the
chest and the player's hands inside its central third. The outer thirds retain
scenery suitable for cropping. Store compositions and final logo placements
were visually reviewed, including the small capsule at its delivery size.

## Prompt changes and provenance

Original manifest prompts were retained. Palette and approved-anchor reference
instructions were appended to identify the reference's role and prevent copying
the anchor's subject. Their exact text is recorded by asset ID in
`_workflow/generations.json` and `_workflow/prompt_changes.json`.

| Asset | Additional correction and reason |
| --- | --- |
| `bg.shop1` | Align the counter, shelf and awning; simplify noisy foliage and keep the counter front calm. |
| `portrait.bram` | Reduce and register the head to the required face crop, eye line and chin line. |
| `chest.wood.body` | Generate the body alone with a full-width rectangular seam and an opaque mounting seat. |
| `chest.wood.open` | Remove the lid entirely, show an empty interior and preserve the matching lower body. |
| `portrait.bramble` | Make the character explicitly an elderly woman with a bare chin after the male reference influenced initial attempts. |
| `store.library_hero` | Reduce the narrative group into the central third to satisfy the crop-safe composition. |

Exact correction text is in `_workflow/prompt_changes.json`. Candidate choices,
raw source hashes, pixel repairs, landmark registration and final PNG hashes
are recorded in `_workflow/generations.json`. Full-resolution raw generations
and unselected candidates are retained locally outside the delivery checkout.
`_workflow/delivery_manifest.json` lists every native and Steam export hash.

## Final verification

All 263 manifest assets are delivered, including optional batches 7 and 8.
`python art/tools/validate.py --strict --sheets` reports 263 OK, zero failures
and zero missing assets. Nothing is skipped. Cross-asset checks additionally
verify chest bodies and masks, non-overlapping key shards and exact reassembly,
visible animation frames, Steam dimensions and gold in all four app icons.

Six width warnings are accepted after native-size review: Witchlight (59%),
Glass Cabinet (45%), Royal Seal (55%), Warden's Lantern (58%), Fate's Spindle
(58%) and Tailor's Dummy (42%). These are deliberately tall objects; increasing
their width would distort the lanterns, cabinet, stamp handle, spindle or stand.

Uploaded filenames and text contain no Chinese. PNG metadata is checked, and
visible lettering was reviewed separately: branding reads KEYWRIGHT; object
scribbles are nonlinguistic marks. All repository changes stay in
`art/generated/`. Game-side integration remains outside this art brief.

## Round 2

### Batch 9: original anchors - superseded by batch 9r

This is the historical record of the first anchor delivery. Its standard counter previews and native asset paths
now show batch 9r; the original files remain in git. Use the dedicated batch-9r previews below for the current review.

Based on `prototype/0.25` at `35ab8bfd3d35933b1e66b5851098e9e5deb5d329`,
on branch `art/round2`. All 13 batch-9 anchors are delivered. Batches 10-15
await the owner's review of these anchors.

Start with [`_review/counter_stall.png`](_review/counter_stall.png) and
[`_review/counter_stall_times.png`](_review/counter_stall_times.png). The latter
shows morning, midday, evening and night, in that order. Only the midday sky
is a batch-9 deliverable; the other three skies are the brief's placeholders.
All seven stall layers and Hob are the new anchors in these mock-ups. The chest
is the existing code-drawn chest. The marked rectangles show interface areas.
[`_review/round2_batch9_sprites.png`](_review/round2_batch9_sprites.png) shows
the six small anchors at 1x, 2x and 4x without the inherited round-1 assets.

Delivered: `sky.midday`, `place.stall`, `frame.stall`, `shelf.stall`,
`counter.stall`, `sheet.stall`, `prop.lantern_stall`, `figure.hob`,
`item.dwarf_gold_crowns`, `heartsong.lullaby_box`, `curio.monocle`,
`icon.upgrade.lamp` and `icon.kind.coins`. Nothing in batch 9 is missing.

The built-in image generator made two candidates per asset and a third for
the frame (27 generations). The manifest prompt was retained verbatim, with
request metadata and the corrections below appended. Candidate B was selected
for every asset except the frame (C). References used for the revision calls
were the new candidate, the flat-to-pixels process example, the palette and/or
the code-drawn chest as a simplicity guide; no round-1 art was used as a style
reference. The tool supplies no independently verifiable model identifier.
Requested and actual image sizes, exact complete prompts, reference lists,
candidate checks, source hashes and final hashes are recorded in
[`_workflow/round2_batch9.json`](_workflow/round2_batch9.json).

All selected images went through the supplied, unmodified `pixelize.py` in
flat mode. The frame uses `--offset-y -60` (source crop starts at row 20) to
keep the dark top band and move the scallops out of the open-view area. The
only manual pixel repair is seven orange pixels in `icon.upgrade.lamp`, changed
to its existing gold: the pixelizer adds the outline after capping colours,
which had left nine colours against an eight-colour cap. Their coordinates and
before/after colours are recorded. No extra blur, smoothing or posterization
was applied. Small source WebP previews are under `_raw/round2_batch9_*`;
full-resolution original PNGs and rejected candidates remain in the local
production archive outside the checkout.

Prompt corrections, beyond the original manifest prompt:

| Asset | Correction and reason |
|---|---|
| `sky.midday` | B: three straight bands and two small plain clouds; remove tonal variation and gradients from A. |
| `place.stall` | B: retain the crossroads landmarks; remove grass tufts, road patches, bushes, reflections and architectural detail. Use broad hill planes, round tree crowns and a simple mill and wheel. |
| `frame.stall` | B: add a continuous plain dark top band, simplify the scallops and posts. C: make the band taller, scallops shallower and posts flush with the edges; A intruded into the open view and B had a top-band gap. |
| `shelf.stall` | B: replace twisted rope detail with plain strips and remove wood texture and shading. |
| `counter.stall` | B: remove diagonal light patches and irregular seams; retain wide flat planks and a straight light lip. |
| `sheet.stall` | B: smaller corner nails, narrower plain wood rim and uniform unbleached paper, to leave the writing area quiet. |
| `figure.hob` | B: simplify hat, folds, stubble and hands, relax both arms and use clear dot eyes; preserve the farmer's identity. |
| `prop.lantern_stall` | B: broad cap, one plain yellow window, simple base and ring; remove bevels, roof tiers and reflections after A failed the speck and colour limits. |
| `icon.upgrade.lamp` | B: fewer brass shapes and one lavender flame on a dark window; remove sparks, reflections and flame core after A failed quiet and colour limits. |
| `icon.kind.coins` | B: three flat inks, broad tops/sides/separations; remove rim glints and facets after A exceeded the colour cap. |
| `item.dwarf_gold_crowns` | B: flat hammer and anvil marks, plain rear coins, fewer highlights; retain the square hole and short stack. |
| `heartsong.lullaby_box` | B: two flower shapes and a crescent, simple red ribbon and crank; remove leaves, gears, distressed edges and detailed bow after A failed quiet and colour limits. |
| `curio.monocle` | B: flat violet lens, simple silver rim and a short solid curved chain; remove individual links, scratches and fragmented reflections after A failed the speck limit. |

Validation: `python3 art/tools/validate.py --sheets` was run over the entire
manifest. **All 13 anchors pass, with zero warnings.** The whole-manifest result
is **38 OK, 164 failed, 65 missing (58 P1 missing), exit 1**. The failures are
inherited round-1 assets assigned to later redo batches, and the missing files
belong to later batches. Before this work the baseline was 25 OK, 169 failed
and 73 missing. Every non-batch-9 asset's validation result is unchanged.
`report.json` retains these failures and missing entries; this is an anchor
delivery, and does not claim round 2 is complete.

The counter composition at all four hours and the native/2x/4x small sprites
were visually reviewed. Please judge the overall quietness, Hob's readability,
the muted landscape, the plain writing sheet, and whether the awning and shelf
feel comfortable around the chest. Morning/evening/night sky artwork will be
made in batch 10 after approval. No game integration is included in this batch.

### Batch 9r: C2 at 4x - awaiting owner review

Delivered on 2026-10-31, following the owner's 2026-10-30 choice of **C2 at 4x, with detail allowed**.
Based on `art/round2` at `efa929082557e6b9ddd2981fc7390643654fda79`, which already contains
`prototype/0.25` at `16094aac662661e6c20baed205caa74978057501` and the completed style study.
The brief, manifest source, generated manifest and inventory now describe C2's warm outlined forms and readable
material details. [The choice record](../study/CHOICE.md) fixes the colour caps and quiet limits measured from C2
before generation. The palette, dimensions, layer registration and validator implementation are unchanged.
All 25 retained round-1 asset specifications are unchanged.

Start with the [counter with interface](_review/round2_batch9r_playview.png),
[small objects at 1x/2x/4x](_review/round2_batch9r_sprites.png) and
[review guide](_review/round2_batch9r_README.md).
[The four-time counter sheet](_review/counter_stall_times.png) uses the new stall anchors at every hour;
morning, evening and night skies remain the existing placeholders. These are composition mock-ups, not live
game captures. The world remains 320x180 at 4x; inventory and interface assets retain their specified 1x/2x uses.

The built-in image generator made **33 candidates for 13 assets**: two each, plus two further attempts for the
frame, two for the Lullaby Box, two for the monocle and one for the hanging lantern. Every request attached the
master palette swatch and the approved `study/raw/C2_A.webp`. Targeted revision requests also attached the
preceding candidate. No superseded plain anchor or round-1 image was used as a style reference.
The tool does not expose an independently verifiable model identifier.

The manifest prompt was sent verbatim, with reference instructions and candidate corrections appended.
Exact base prompts and additions, references, requested and actual sizes, all 33 candidate checks, source hashes,
selection reasons, processing commands, repairs and final hashes are in
[`_workflow/round2_batch9r.json`](_workflow/round2_batch9r.json).
The selected source previews are the 13 `_raw/round2_batch9r_*.webp` files. These are small, lossy viewing copies;
full-resolution PNGs and rejected candidates are retained in the local production archive outside the checkout.

| Asset | Kept | Selection and correction |
|---|---|---|
| `sky.midday` | B | Broad blue bands and clear cloud groups; fewer small cloud fragments than A. |
| `place.stall` | B | Keep the road, signpost, castle, mill and wheel, cart, fences, flowers and smoke; reduce scattered foreground pebbles. |
| `frame.stall` | D | Preserve the striped awning, rope, herbs and bell. B and C narrow the edge decorations; D moves a smaller bell fully against the right post to clear the open view. |
| `shelf.stall` | B | Thin plank, broad worn edge, end nails and rope knots; A's thick, heavily marked board failed the speck limit. |
| `counter.stall` | B | Tabletop only, with readable seams, end nails and worn edges. Remove A's front fascia and keep the central surface calm. |
| `sheet.stall` | B | Thin rim and small corner nails around plain, light paper. |
| `figure.hob` | B | Broad readable face, frayed hat, band, stubble, stitched patch and rolled sleeves; both candidates passed, and B reads more clearly in the counter composition. |
| `prop.lantern_stall` | C | Compact brass shape, one broad yellow window and a clear dark contour. Earlier reflections fragmented at the small native size. |
| `item.dwarf_gold_crowns` | B | Bold stamp, open square hole and rear stack; fewer chipped bevels and scattered glints than A. |
| `heartsong.lullaby_box` | D | Keep two large painted flowers, crescent, ribbon and crank. Earlier fine wear and ornament broke into isolated pixels. |
| `curio.monocle` | D | Large round violet lens, rim, glint and a short continuous chain; remove individual chain links and repair the contour palette mapping. |
| `icon.upgrade.lamp` | B | Clear lantern and violet flame; connect one cap-highlight pixel to preserve a single readable glint. |
| `icon.kind.coins` | B | Three simple overlapping coins; correct five mis-mapped contour pixels to the outline colour. |

All selected sources went through the supplied, unmodified `pixelize.py` in flat mode. The final frame uses
`--offset-y -80` to retain the continuous top beam and clear the open view; its source crop starts at row zero.
The monocle uses the existing `--recolour '#180939=#1a1420'` option for 47 contour pixels that snapped to the
arcane ramp. The coin-kind icon similarly maps four `#3b0014` pixels and one `#280c06` pixel to `#1a1420`.
There is exactly one manual native-pixel repair: the upgrade lamp at (8,8), `#626d80` to `#9b92ad`, joining two
diagonal highlight pixels into one continuous highlight. No extra blur, smoothing or posterization was used.

Validation: `python3 art/tools/validate.py --sheets` checks the whole manifest.
**All 13 batch-9r anchors pass, with zero warnings.**
The full result remains **38 OK, 164 failed, 65 missing (58 P1 missing), exit 1**. The failures and missing files
belong to later batches; every non-anchor output hash and validation result is unchanged from the pre-generation
baseline under the revised brief. `report.json` retains those unresolved entries. This does not claim that all
round-2 art is complete.

The counter, interface overlap, all four lighting states and small sprites were visually reviewed. Hob's face
remains below the dialogue panel, the shelf and counter meet their registered rows, and the writing area stays
plain. The landscape's right-side landmarks remain in the asset even where the appraisal sheet covers them.
The composed native world measures 0.023373 specks, 0.202268 edges and 11.4348 brightness step, below C2's
whole-image comparison ceilings (0.05 / 0.27 / 14), while retaining the selected material and character details.
The dedicated previews are reproducible with
`python3 art/generated/_workflow/round2_batch9r_review.py`.

The historical style-study prompts, outputs and review images have not been regenerated; only its decision/status
notes changed. **Stop here for owner review.** No batch 10-15 artwork or game integration is part of this delivery.
