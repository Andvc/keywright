# Keywright art delivery

> **0.25:** round 1's chest was retired: the chest and its locks are drawn in code now (`tools/custom_art.py`), so
> its files, its review sheets and `_workflow/finalize_chests.py` were removed (git has them). Round 2 redraws most
> of the rest, quieter (`art/README.md`); record it in a Round 2 section at the end.

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
