# Keywright art delivery

> **0.25:** round 1's chest was retired: the chest and its locks are drawn in code now (`tools/custom_art.py`), so
> its files, its review sheets and `_workflow/finalize_chests.py` were removed (git has them), with round 1's anchors
> and its counter mock-ups. Round 2 (`art/README.md`) builds the shop scene from layers. The current direction is
> C2 at 4x, with detail and **no outlines**. The latest delivery is batch 10 in the Round 2 section below.

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

### Batch 9: original anchors - historical

The following records the original plain batch. Its native paths and standard counter previews now show batch 9s;
the original files remain in git. Use the dedicated batch-9s previews below for the current review.

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

### Batch 9r: rolled back

The outlined C2 anchors in commit `f4ee68b312fa2116b376100730f25455f0988b62` were rejected.
On 2026-10-31 the owner requested rollback and clarified: **C2 at 4x, but no outlines**.
Revert commit `9539d8f` restored the pre-9r state and was pushed before replacement work.
The rejected art, prompts and delivery record remain available in that original commit and the local archive.

### Batch 9s: C2 at 4x without outlines

Delivered on 2026-10-31. The corrected brief retains C2's warm colours, rounded forms, expressive farmer and
meaningful details, while removing inked outer and internal contours. Adjacent material colours and cel-shaded
areas define the shapes. Natural facial features, plank gaps, material rims and shadows remain where needed.
Every round-2 `redo` specification now has `outline: false`; processing also passes `--no-outline`.
The 25 retained round-1 specifications are unchanged. See [the owner's decision](../study/CHOICE.md).

Review the [counter with interface](_review/round2_batch9s_playview.png),
[small objects at 1x/2x/4x](_review/round2_batch9s_sprites.png) and
[four times of day](_review/counter_stall_times.png).
The [review guide](_review/round2_batch9s_README.md) also links to individual layers and Hob.
These are composition mock-ups using the existing study interface and code-drawn chest, not live game captures.
The world is 320x180 at 4x; inventory and interface assets retain their specified 1x/2x uses.
Only the midday sky is new; morning, evening and night skies remain existing placeholders.

The built-in image generator made **27 edit candidates for 13 anchors**: two per asset and a third for the
upgrade-lamp icon. Each request attached the corresponding full-size 9r source as the edit target, the palette
swatch and the original C2 scene. Reference roles explicitly override the outlines in those sources and preserve
the asset's geometry, subject and useful details. The lamp's third candidate uses its new outline-free B as the
target. The tool does not provide an independently verifiable model identifier.

The manifest prompt was sent unchanged, followed by exact edit instructions and the recorded corrections.
[`_workflow/round2_batch9s.json`](_workflow/round2_batch9s.json) contains all 27 requests, reference hashes,
requested and actual sizes, candidate checks, selection reasons, processing commands and final PNG hashes.
Small selected-source WebPs are under `_raw/round2_batch9s_*`; these are lossy viewing copies.
All full PNGs and rejected candidates remain in the local production archive outside the checkout.

| Asset | Kept | Selection / correction |
|---|---|---|
| `sky.midday` | A | Clear cloud groups and open central sky; both pass. |
| `place.stall` | B | Preserve the road, sign, castle, mill, cart, flowers and fences; broader colour shapes in the foliage. |
| `frame.stall` | B | Remove enclosing awning strokes while retaining the top beam, herbs, bell and rope lashings. |
| `shelf.stall` | B | Preserve knots, nail heads and the worn plank; A's reduced highlights create too many lone pixels. |
| `counter.stall` | A | Readable seams, nails and worn edges with broad, quiet wood areas. |
| `sheet.stall` | B | Plain writing field with restrained edge shading; no inked paper contour. |
| `figure.hob` | A | Preserve the hat, stubble, rolled sleeves and patch; A has clearer native-size colour areas. |
| `prop.lantern_stall` | B | Steel body and warm window without an ink border; fewer fragmented reflections. |
| `item.dwarf_gold_crowns` | B | Preserve the square hole, hammer/anvil marks and rear stack; A exceeds the speck limit. |
| `heartsong.lullaby_box` | B | Keep the original two flower marks, crescent, ribbon and crank; remove A's extra sprigs and scattered surface wear. |
| `curio.monocle` | B | Violet lens, glint and short curled chain; replace A's mottled silver with broad metal tones. |
| `icon.upgrade.lamp` | C | Broaden the body, shorten the loop, remove the detached flame spark and consolidate metal highlights; A/B exceed the speck limit and are too narrow. |
| `icon.kind.coins` | B | Three coins separated by their material tones; fewer lone pixels than A. |

The supplied `pixelize.py` and validator are unmodified. All selected sources use flat mode with no automatic
outline. The frame retains `--offset-y -80` (source crop starts at row zero) to register the top band.
There are **no manual pixel repairs or palette recolours** in this batch, and no additional blur, smoothing or
posterization. The C2 colour caps and quiet ceilings were fixed before generation and remain unchanged.

`python3 art/tools/validate.py --sheets` reports **13/13 anchors passing, with zero warnings**.
The complete manifest reports **38 OK, 164 failed, 65 missing (58 P1 missing), exit 1**: later batches still contain
inherited art that needs redrawing and missing files. Every non-anchor output hash and validation result is unchanged
from the baseline under the corrected brief. `report.json` retains the unresolved entries; round 2 is not complete.

Visual review checked the outline-free source and native assets, Hob's face below the dialogue panel, shelf/counter
registration, plain paper, small-object readability and all four lighting states.
The composed native world measures 0.011943 specks, 0.180123 edges and 8.7207 brightness step, within the C2
whole-image comparison ceilings of 0.05 / 0.27 / 14.
Reproduce the dedicated previews with `python3 art/generated/_workflow/round2_batch9s_review.py`.

The original style-study prompts, outputs and review pictures remain unchanged.
**Stop for owner review here.** No batches 10-15 or game integration are included.

The owner subsequently approved batch 9s on 2026-10-31. Its dedicated previews remain historical snapshots;
the standard counter/contact sheets now reflect batch 10 below.

### Batch 10: scenes and counter props without outlines

Delivered on 2026-10-31, based on approved batch 9s at `10d697cce338bcd585c80831fc0271cf105b51d7`.
The owner approved the no-outline anchors and asked for the next step. All **32 batch-10 assets** are delivered:
three skies, three places, three frames, three shelves, three counters, three sheets, ten props and four story
scenes. The selected C2/4x direction and its useful detail remain unchanged.

Start with the [four shops](_review/round2_batch10_shops.png), [small props](_review/round2_batch10_props.png),
[story scenes](_review/round2_batch10_stories.png) and [review guide](_review/round2_batch10_README.md).
These are composition mock-ups, not live game captures. The dedicated shop views use approved Hob and the
same three approved objects throughout; their reused study HUD still names Bram's stall.
Standard `counter_{shop}.png` and `counter_{shop}_times.png` show the manifest's sellers, using placeholders
where batch-11 figures are missing, and inherited shelf items awaiting later redraws.

The built-in image tool made **77 candidates**: A/B for all 32, twelve C corrections and a fourth gavel.
It does not expose an independently verifiable model identifier. Every unchanged manifest prompt, exact appended
instruction, reference hash, requested/actual size, candidate result, selection and final command is in
[`_workflow/round2_batch10.json`](_workflow/round2_batch10.json). All requests attached the palette and an
approved no-outline category anchor; non-lantern props used the approved Lullaby Box for the material language.
Guild C and vault C additionally used their own A as an edit target. Full-size originals and rejects remain in
the local archive; only small selected-source WebPs are committed.

Every request appended the reference roles, the required absence of contours, native-size readability and PNG
quality/size intent. Candidate B requested an independent composition with the same subject and registration.
Category additions were:

- Skies: opaque sky only, broad horizontal bands, one sun/moon and sparse clouds/stars.
- Places: magenta only in sky glimpses, opaque lower 40%, visible landmarks away from the top frame, calm floor.
- Frames: continuous top band, shallow upper structure, narrow side posts, magenta middle and no bottom bar.
- Shelves: a long slim straight board, supports reaching the top and magenta through all gaps.
- Counters: a full-width opaque lower strip, level far edge, no legs or front face, restrained dark material tones.
- Sheets: uniform light writing field, all fittings confined to the rim, no paper texture or ink contour.
- Props: repeat the native dimensions, broad material features, full silhouette, magenta gaps and no ground shadow.
- Story scenes: opaque full-bleed setting, quiet middle/lower third, reference for style only, broad side landmarks.

Additional corrections, with the original prompt retained:

| Asset | Appended change and reason |
|---|---|
| `sky.morning`, `sky.evening`, `sky.night` | C: uniform broad bands, clear disc without a halo, smaller lower cloud shapes; night has no clouds and four sparse stars. A/B had noisy snapped gradients or large pale clouds. |
| `place.emporium` | C: shaded grey-lilac marble, mid-dark floor, restrained crimson and distant gold gate; A/B exceeded the brightness ceiling. |
| `frame.emporium` | C: dark continuous crimson band, shaded marble, restrained gold fringe and no hanging tassel clutter; A/B were too bright. |
| `prop.lantern_crownhold` | C: shorter ring/cap, broad round glass, plain sides and low foot, with no internal flame or fittings; A/B exceeded quiet limits. |
| `prop.lantern_emporium` | C: larger crystal globe, simple gold supports and a few broad reflection areas; remove tiny machinery and fragmented reflections. |
| `prop.lodestone` | C: low wide stone, broad planes and simple brass cup; remove feet, rivets and tiny chips. |
| `prop.scales` | C: broad beam and pans, short plain post, low foot and fewer material tones; remove finials, collars and small fittings. |
| `prop.gavel` | C: low horizontal wooden hammer and sound block without metal bands. D: plain cylinder, straight handle and low oval block using uniform broad browns; A/B/C remained too fragmented at 22x12. |
| `prop.book`, `prop.picks` | Before A/B: low wide shapes, few uniform material areas; single cream page block for the closed book, broad strap and a few steel tips for the pouch. Avoid repeated page lines and tiny stitching. |
| `scene.guild` | C edits A: preserve the iron door, ledger walls and almond keyhole eye; remove bright side windows, plants, eye rays and reflective floor grid, leaving one hanging light. |
| `scene.vault` | C edits A: remove extra torches, treasure props, tiled floor and gold floor circles; keep one muted beam on the Crown Chest, with the Gilded Gate and dark side pillars. |

Selections:

| Asset | Kept | Reason |
|---|---|---|
| `counter.crownhold` | B | restrained walnut planes and brass far edge; A includes an out-of-palette black crop pixel. |
| `counter.emporium` | B | dark green marble with broad veins and thin gold far edge; A falls just below the value floor. |
| `counter.thornwick` | B | broad wood planks with readable knots and end nails, quiet centre. |
| `frame.crownhold` | B | fewer isolated hardware highlights; both pass. Crop offset -80 preserves the top band. |
| `frame.emporium` | C | darker crimson top band and shaded marble retain the gold fringe without exceeding the value ceiling. Crop offset -80. |
| `frame.thornwick` | A | clean top band and side posts; B intrudes into the protected view. Crop offset -80 preserves the band. |
| `place.crownhold` | B | oak panels, six-pane window, twin spires and doorbell read clearly at native size. |
| `place.emporium` | C | shaded marble and broad darker floor pass the value limit; distant gilded gate remains legible. |
| `place.thornwick` | B | readable stone shop, open doorway, well and distant pines with fewer competing colour areas. |
| `prop.book` | B | green cover, cream page block and gold clasp; A adds leaf decoration and fails quiet limits. |
| `prop.gavel` | D | broad head, long handle and low sound block in five native colours. A/B/C exceed quiet limits. |
| `prop.lantern_crownhold` | C | broad round glass and plain brass supports; A/B exceed small-sprite quiet limits. |
| `prop.lantern_emporium` | C | broad crystal globe and simple gold supports; A/B have fragmented reflections and too many edges. |
| `prop.lantern_thornwick` | B | clear iron body and warm rectangular glass at 16x24. |
| `prop.lodestone` | C | low wide stone in a simple brass cup; A/B have narrow stone and fussy mounting hardware. |
| `prop.picks` | B | broad leather roll, closing strap and steel tips; A exceeds the speck limit. |
| `prop.scales` | C | level pans, plain beam and short post remain readable at 24x24; A/B have excessive fittings. |
| `prop.seal` | A | red handle and gold foot stay distinct; B exceeds the speck limit. |
| `prop.witchlight` | B | purple flame stays recognisable inside brass framework; A exceeds quiet limits. |
| `scene.bram` | A | hearth, worn chair, open chests and dusk window with a plain lower third; B contains fewer open chests. |
| `scene.guild` | C | one hanging lamp, iron vault door, almond-shaped keyhole eye and ledger walls; removes A's bright side windows and preserves the eye lost in B. |
| `scene.keyhold` | B | broken pillars, circular ruin floor and violet keyhole; distant ruins fit the setting better than A's intact castle. |
| `scene.vault` | C | one muted beam on the Crown Chest and Gilded Gate, plain dark floor. A/B add multiple torches, treasure props and busy floor markings. |
| `sheet.crownhold` | A | plain cream paper in a green folder, four small gold corner fittings. |
| `sheet.emporium` | A | thin gold frame and small crimson seal; B adds unnecessary seal ribbons. |
| `sheet.thornwick` | B | plain writing field, recognisable brass clip and restrained wooden rim. |
| `shelf.crownhold` | A | brass rods and walnut board; B has fine dark end-cap strokes and more scattered highlights. |
| `shelf.emporium` | A | pale turquoise glass and gold chains; B is bluer with more isolated highlights. |
| `shelf.thornwick` | B | iron chains, worn oak board and fittings remain legible with a broad standing surface. |
| `sky.evening` | C | clear setting sun and low grouped cloud, no dark sun halo; lower warm bands are mostly behind the place layer. |
| `sky.morning` | C | distinct pale sun and one low cloud; broad flat bands avoid A/B's mottled colour transitions. |
| `sky.night` | C | cloud-free dark upper field, four sparse stars and a clear moon; A/B inherited large pale clouds. |

All sources were processed by the unchanged `pixelize.py` in flat mode with `--no-outline --preview`.
The three frames use `--offset-y -80`, placing their source crop at row zero and preserving the HUD band.
There are **no manual pixel repairs, palette recolours, additional smoothing or changes to validation limits**.
Native output hashes match the selected pre-delivery candidates.

The final `python3 art/tools/validate.py --sheets` run over everything reports **32/32 batch-10 assets passing,
zero warnings**. The full manifest is **70 OK, 160 failed, 37 missing (35 P1 missing), exit 1**.
Compared with the 38/164/65 baseline, this batch replaces four failing story scenes and fills 28 missing layers/props.
All **235 non-batch-10 asset hashes or missing states, errors and warnings are unchanged**. The remaining
failures and missing files belong to batches 11-15; the complete round is not yet finished.

Source and native views, all four shops at four hours, paper writing areas, the shelf/counter/HUD registration,
small tools and story landmarks were visually checked. The four native assembled worlds have specks at most
0.011943, edges at most 0.189199 and brightness step at most 8.7208, within the C2 comparison ceilings.
[`round2_batch10_review.py`](_workflow/round2_batch10_review.py) reproduces the dedicated previews.
Original style-study outputs and dedicated batch-9s previews are preserved.

**Stop after pushing batch 10.** No batch-11 figures, later object/icon/lock batches or game integration are included.


### Batch 11: full-length figures

The owner approved batch 10 and requested **“大致ok接着完成所有内容”**. This supersedes the previous
batch-10 stop: complete batches 11-15 and the documented game integration. This commit delivers all 37
remaining figures, including the P3 identity reveals. No figures were skipped; all 37 pass with zero warnings.

Review [the figure guide](_review/round2_batch11_README.md), its four full-figure pages, five actual-size
counter crop pages, identity pairs and four full composites. All 38 figures, including the approved Hob, were
inspected against the counter. Faces stay below the speech bubble; the tallest hats/horns can extend behind it.
These are reference composites, not live captures, and the overlay uses Hob's example speech for placement.
All four shops at all four hours were also reviewed. Shelf items still include pending round-1 redraws.

The built-in image tool produced **78 candidates**: A/B for every figure, Snikkit C/D, Wenna C and Vexis C.
It does not expose a verifiable model identifier. Every unchanged manifest prompt, exact appended instruction,
reference hash, source size, native check, choice and command is recorded in
[`_workflow/round2_batch11.json`](_workflow/round2_batch11.json). Full PNG originals and rejected candidates
remain in the local production archive; only 37 small selected-source WebPs are committed.

Every prompt appended: the approved no-outline Hob as a style reference only, the master palette, full-body
framing including hats/horns/feet, the exact target height, expressive face and clear signature features,
broad two-or-three-tone fills, no enclosing or internal ink contours, no floor shadow, flat magenta empty space,
and PNG/quality/resolution intent. B asks for an independent version of the same specified person and outfit.

Additional prompt changes:

| Asset(s) | Appended change and reason |
|---|---|
| `figure.bramble` | Explicitly an elderly woman, without beard or moustache, following Mother Bramble's canon. |
| `figure.aldo`, `figure.aldous`, `figure.ansel`, `figure.bram`, `figure.bramble`, `figure.corisande`, `figure.ermine`, `figure.fennick`, `figure.gerda`, `figure.grit`, `figure.hesk`, `figure.isolde`, `figure.king`, `figure.kit`, `figure.mara`, `figure.oriel`, `figure.orrin`, `figure.pell`, `figure.quill`, `figure.rosa`, `figure.shelly`, `figure.silas`, `figure.vane`, `figure.wren` | Muted rosy-peach skin, neutral daylight and no gold/yellow/orange skin patches, after early reductions snapped warm skin to the gold ramp. |
| `figure.king` A/B | Attach selected Aldo A and preserve his face, age, white hair/beard/moustache and ruby ring; change only royal costume and crown. |
| `figure.mara` A/B | Attach selected Hood A; preserve the violet cloak and silver key clasp while revealing the weathered older woman and cropped silver hair. |
| `figure.snikkit` C | Edit A's mustard skin to leaf green, retaining identity, clothes and sack. |
| `figure.snikkit` D | C remained above the edge limit; broaden body and simplify skin, clothes and sack to fewer broad tones and one patch. |
| `figure.wenna` C | Edit A's gold-looking skin to muted tanned peach; preserve hood, feather, braid, bow and clothing. |
| `figure.vexis` C | A/B exceeded quiet limits at 64px height; compact body, larger head and fewer broad material tones while retaining horns, wings, tail and hourglass. |

Selections:

| Asset | Kept | Reason |
|---|---|---|
| `figure.tam` | A | A has broader clothing regions and a clearer child silhouette; three palette swaps correct yellow skin to the skin ramp. |
| `figure.penn` | A | Clearer eyes and soft sad smile; grey bun, plum shawl and black mourning dress remain distinct. |
| `figure.mags` | A | Broader apron folds and a clearer natural skin area than B; red updo and green bodice read well. |
| `figure.dorrin` | A | Clearer face and less fragmented leather than B, with the headband and belt lantern intact. |
| `figure.snikkit` | D | Green skin and a clear goblin silhouette with simpler cloth and sack; passes the edge limit that B/C exceeded. |
| `figure.wenna` | C | Corrects the gold-looking face to natural tanned skin while preserving the green hood, red feather, auburn braid and bow. |
| `figure.bramble` | B | The broad moss-green cloak, white hair and holly make a clearer elderly woman druid than A's gold streaks. |
| `figure.ansel` | A | Clearer youthful face with a broad quiet brown habit, rope belt and readable wooden key pendant. |
| `figure.fennick` | A | The black wide-brim hat and long dark coat read more clearly than B; the thin moustache remains legible. |
| `figure.gerda` | B | The warmer grandmotherly expression is clearer at native size, with white cap, red bodice and dark skirt intact. |
| `figure.wren` | B | Cleaner face and paper-tag silhouette after reduction, with quieter blue robe folds than A. |
| `figure.orrin` | B | Stars remain readable on hat and robe without exceeding calm limits, with a clear white beard and expressive face. |
| `figure.grit` | A | Stocky dwarf proportions, candle helmet and orange braid remain legible with less fragmented chainmail than B. |
| `figure.corisande` | A | Clearer face and quieter large lavender folds; pearl hairpin and high lace collar survive reduction. |
| `figure.pell` | A | Natural skin and quieter olive waistcoat than B's yellow skin and broken shading; clear elderly thin silhouette. |
| `figure.quill` | B | The face avoids A's yellow patches and the white collar, rolled parchment and dark blue robe stay distinct. |
| `figure.shelly` | B | Blue headscarf, shell necklace and patched smock are distinct, with more natural bare feet than A. |
| `figure.aldous` | B | Passes calm checks unlike A, with clear red plume, raised visor, moustache and white heraldic tabard. |
| `figure.vexis` | C | The compact redraw keeps horns, wings, tail and hourglass readable at 64 pixels and passes calm limits that A/B failed. |
| `figure.malbranche` | B | The skull, gold seal and black cloak read clearly with broader, less fragmented cloth than A. |
| `figure.skrae` | B | The huge bat ears, red eyes, fangs and hunched stance read more clearly than A after reduction. |
| `figure.ermine` | B | Clearer haughty face and complete shoes, with tall white wig, ermine stole and violet gown intact. |
| `figure.oriel` | B | The three-card fan and face are clearer, with a distinct purple turban and gold hoops. |
| `figure.silas` | B | Confident pocket pose and readable moustache, gold chains and purple velvet coat with quieter trousers. |
| `figure.aldo` | A | More readable weary eyes and old face than B; grey travelling cloak, white beard and ruby ring retained. |
| `figure.maw` | A | Sharper red-eyed expression and clear crown-like gold face, with broad dark cloth between heavy gold chains. |
| `figure.bram` | B | Clearer kind face and natural skin, stout build, red felt cap and waistcoat with less broken shading than A. |
| `figure.vane` | A | The pale face and restrained black high-collar coat fit his cold presence; avoids B's yellow facial patches. |
| `figure.hood` | A | The violet eyes and silver key clasp are clearer than B; face remains fully concealed and cloak folds stay broad. |
| `figure.nim` | A | Natural warm brown skin and clearer patchwork pockets, pack and horns than B's gold skin patches. |
| `figure.stranger` | A | A clear heavy iron chain and dark hood with small eye glints; broader quieter cloth than B. |
| `figure.kit` | B | Keeps natural skin rather than A's yellow face and arms; red headband, ponytail and listening tilt remain distinct. |
| `figure.hesk` | B | Clearer face and natural arm colour, with the stocky grocer's green cap, apron and hand-at-back pose intact. |
| `figure.rosa` | A | The confident pose, red scarf and orange ponytail are clear, with much quieter skirt folds than B. |
| `figure.isolde` | B | Readable braided blonde crown, gold collar and wine gown, with complete feet. Five eye pixels use the existing brown hair-shadow colour to remove false red eyes caused by palette reduction. |
| `figure.mara` | B | Clearer weathered face and violet eyes, with cropped silver hair and the same cloak and key clasp as Hood A. |
| `figure.king` | B | Clearer face than A; directly references the selected Aldo A to preserve identity beneath crown and royal mantle. |


All selected sources were processed with the unchanged `pixelize.py --no-outline --preview` in flat mode.
Tam's three supported palette swaps are `#ffb52e=#e1a882`, `#e1a882=#bd7e5f`, `#f2c14e=#fed5b3`.
Isolde B has five eye pixels changed from `#6c0026` to the existing `#8e5b37` at (49,46), (50,46), (49,47),
(54,47), (55,47), preventing palette reduction from giving her false red eyes. There are no other manual repairs,
additional raster filters or changes to caps/quiet limits. Source and output hashes are recorded.

Final `python3 art/tools/validate.py --sheets`: **37/37 batch-11 assets OK, zero warnings**. Full manifest:
**107 OK, 160 failed, 0 missing (0 P1 missing), exit 1**. The 230 non-batch assets have unchanged output hashes,
missing states, errors and warnings. The remaining 160 failures are inherited round-1 assets in batches 12-15.
Historical study pictures and dedicated batch-9s/10 previews remain unchanged.

**Continue batches 12-15, then game integration.** The complete round is not yet finished.


### Batch 12: chest items and Heartsongs

The owner's request to finish all remaining work continues to authorize this batch and the following integration.
All **40** remaining inventory objects and Heartsongs are delivered; none skipped. All pass with zero warnings.
They retain the approved C2 character, useful material details and no outlines, at the specified 32x32 native /
2x UI scale. The music box and dwarf-gold anchors remain unchanged.

See [the review guide](_review/round2_batch12_README.md), the actual-size overview and three 2x/4x comparison
pages. All selected source illustrations and native sprites were inspected. The four shops at all four hours
were also reviewed; these are reference composites, not live game captures.

The built-in image tool produced **124 candidates**: A/B for every asset plus 44 corrections.
It does not expose a verifiable model identifier. The unchanged manifest prompts, every exact append,
reference hashes, actual source dimensions, all candidate checks and selection decisions are in
[`_workflow/round2_batch12.json`](_workflow/round2_batch12.json). Full original/rejected PNGs remain in the local
production archive. Only 40 small selected-source WebPs are committed.

Every prompt appends the category anchor as a style reference only, the master palette, full-object framing,
the 32x32 target, broad cel-shaded surfaces, explicit no-outline constraints, pure magenta empty space and
PNG/resolution/quality intent. B requests an independent version of the same object. After the first coin/weapon
attempts, later A/B prompts also request at most eight solid colours, large plain fields, no gradient or
polygonal patchwork, and three broad gem facets. Exact candidate-specific timing is preserved in the JSON.
Corrections retain each original manifest prompt and append instructions; no prompt is silently rewritten.

Additional corrections and decisions (full appended wording in the provenance file):

| Asset | Candidates | Kept | Reason |
|---|---|---|---|
| `item.barrow_silver` | A/B/C/D/E | E | The corrected edit replaces mottling with broad tarnished silver planes; the crowned profile and rough old-coin silhouette remain readable. E is the first candidate to pass unchanged pipeline limits. |
| `item.moonsilver_marks` | A/B/C/D | D | The large incised crescent and orderly five-coin stack read cleanly at 2x; D has the calmest broad silver-blue face. |
| `item.sunken_sovereigns` | A/B/C/D | D | Plain gold coin faces, broad teal corrosion, one cream barnacle and a curved weed ribbon survive reduction. D passes without copying the dwarf anchor emblem. |
| `item.dragon_gold` | A/B/C/D | C | The staggered tall gold stack, small jewelled crown and distinct red ruby stay legible at 2x. C preserves volume with broad warm fills and passes. |
| `item.demon_gold` | A/B/C/D | C | C keeps the low old-coin heap, broad worn iron and sickly green patches, and a bold horned stamp. It passes both quiet limits and remains different from bright dragon gold. |
| `item.goblin_cleaver` | A/B | B | The broad dark blade, notched cutting edge and tied leather grip remain distinct at 2x; B passes both quiet limits without contour strokes. |
| `item.dwarven_axe` | A/B/C/D | D | The large steel head, single angular knot and brown oak haft with two bronze bands remain clear. D gives the wood the correct brown material and uses broad uninterrupted shading. |
| `item.wardens_blade` | A/B/C/D | D | D has a broad continuous steel-blue blade, readable key-bow guard, plain blue grip and round pommel; the enlarged blade survives the native 32-pixel scale. |
| `item.flamebrand` | A/B/C/D/E | E | The edited broad blade preserves a single molten orange branching vein, ember edge, black guard and red-gold fittings. Large charcoal areas remain continuous at 2x and all checks pass. |
| `item.dragonslayer` | A/B/C/D | D | D gives a broad steel blade, two cyan geometric motifs, wing-shaped guard and ruby pommel. One manual pixel joins the ruby's red front facet, removing an isolated notch without altering its silhouette. |
| `item.wolfskin_cloak` | A/B/C | C | C preserves the wolf head, ears, bone toggle and broad shaggy pelt. Two manually inspected pixels remove stray fur-colour dots in otherwise continuous cape areas. |
| `item.elven_cloak` | A/B | A | A preserves the single silver leaf clasp, deep forest-green fabric and broad elegant folds. The hanging cloak silhouette is distinct at 2x and all checks pass. |
| `item.cloak_of_shadows` | A/B | A | A keeps the blue-black hood, dim violet lining and wispy hem while landing exactly on the required baseline; B loses its bottom pixel during reduction. |
| `item.archmages_mantle` | A/B | B | B retains the stiff high collar, silver stars, gold braid and indigo main fabric. The stars are grouped on broad panels and the native sprite passes. |
| `item.phoenix_mantle` | A/B/C | C | C groups the phoenix feathers into broad gold, orange and red tiers; the warm hanging silhouette and gold clasp remain clear with quieter internal surfaces. |
| `item.healing_draught` | A/B | B | B keeps the round glass flask, red liquid, cork and tied twine with a readable connected highlight and passes at native size. |
| `item.elixir_of_vigour` | A/B/C | C | C keeps the tall square bottle, orange elixir and red wax seal. Broad liquid faces and a single glass highlight remain calm at native size. |
| `item.invisibility_draught` | A/B | B | The teardrop bottle and silver stopper stay clear; broad pale blue liquid bands fade toward the clear upper neck, with no magenta holes in the vessel. |
| `item.mermaids_breath` | A/B/C | C | Shell-like round teal bottle, pearl stopper and two bubbles remain distinct at 2x; the corrected broad glass faces pass without a contour stroke. |
| `item.ambrosia` | A/B/C | C | Broad golden amphora with two handles and faceted stopper; calmer uninterrupted gold areas preserve its ceremonial silhouette. |
| `item.scroll_of_sparks` | A/B/C | C | Cream rolled parchment, blue tie and single spark motif stay readable; fewer scroll bands and no scattered sparks. |
| `item.scroll_of_fireball` | A/B/C | C | Large orange-red flame disc survives on the unrolled cream parchment; two rollers read cleanly without decorative sparks or distressed specks. |
| `item.summoners_grimoire` | A/B/C | C | Broad purple cover, gold eye and clasp clearly identify the book at 2x; removing the ray border and rivets restores calm surfaces. |
| `item.lichs_tome` | A/B/C | C | Plain black cover, large ivory skull, iron corners and green-violet page edge remain distinct; simplified skull features read at 2x. |
| `item.keyhold_codex` | A/B | A | Blue and gold volume with the clearest large keyhole and a readable key clasp; quieter central emblem than B. |
| `item.sand_case` | A/B | A | Battered iron cylinder, detached lid and spilling yellow sand read clearly at native UI size; A retains broader metal faces and passes. |
| `item.moonstone` | A/B | B | Round milky blue-white cabochon with one broad shimmer wave; B has the clearest smooth dome at 2x. |
| `item.elfstone` | A/B/C/D | D | Large green leaf gem in broad silver leaf-shaped setting; a short simple chain loop preserves its pendant identity without noisy alternating link glints. |
| `item.blood_ruby` | A/B | B | Strong red cut-gem silhouette, broad lit table and dark lower facets; B is calmer with a clear single glint. |
| `item.starstone` | A/B/C | C | Plain deep-blue sapphire face, single solid six-pointed star and broad gold setting read clearly at 2x; no fragmented gem facets or glossy dots. |
| `item.dragons_eye` | A/B/C | C | Broad amber oval with dark vertical slit and four gold claws; three isolated brown reduction pixels joined to their surrounding material faces by hand. |
| `item.paste_gem` | A/B | B | Dull grey cut glass, low-contrast broad facets and an obvious chipped right corner distinguish the fake gem from the saturated treasures. |
| `heartsong.shepherds_pipe` | A/B/C/D | D | Exactly seven graded wooden pipes, two broad twine bindings and red tassel; the fresh front-facing composition fixes the six-pipe count and passes at 2x. |
| `heartsong.fey_chimes` | A/B/C | C | Five clearly separated glass-and-silver rods of different lengths, curved birch and red thread; plain rods preserve the distinctive hanging silhouette at 2x. |
| `heartsong.dragons_egg` | A/B/C/D | D | Upright dark-red scaled egg with broad ember-orange gaps and a small straw nest; larger scale fields and a few wide straw ends preserve its identity at 2x. |
| `heartsong.clockwork_lark` | A/B/C | C | Compact brass songbird with open beak, red breast, two wing rivets, winding key and wooden perch; broad metal faces stay readable at 2x. |
| `heartsong.hearthstone` | A/B/C/D | D | Smooth rounded grey stone, broad red branching vein and soot-dark base; plain stone fields read calmly without mottled soot. |
| `heartsong.drowned_bell` | A/B/C | C | Bronze bell with one broad teal patina region, red rope and river weed; one hand-restored weed-tip pixel brings the silhouette to baseline row 30. |
| `heartsong.bottled_star` | A/B/C | C | Plain round lavender-blue bottle, cork, four-point warm star, red string and blank tag; broad reflection replaces fragmented glass glints. |
| `heartsong.imps_ember` | A/B/C | C | Clear heart-shaped red-orange coal, two little flames and plain dark iron dish; the heart stays recognizable without fragmented coal cracks. |


Corrections simplify specific fragmented surfaces: coin faces/corrosion; broad blade fields and hilts; cloak
fur and feather tiers; glass highlights, bottle facets and scroll curls; book frames, rivets and emblems;
gem settings and chain highlights; pipe bindings, chime ornaments, egg scales/nest, brass bird fittings,
stone soot and other Heartsong surface details. Required identities, major motifs and material colours remain.
Some assets required more than the brief's usual two-to-four candidates because their first four failed the
unchanged native-size quiet limits; these extra correction attempts are recorded rather than relaxing limits.

All selected sources use the unchanged `pixelize.py --no-outline --preview` in flat mode. The only manual
pixel repairs, explicitly allowed by the brief, are listed below; there are no additional raster filters.

- `item.dragonslayer` D: (4,28) `#9c1839` to `#ff4058` — Join the ruby front facet; keep its darker underside on the bottom row..
- `item.wolfskin_cloak` C: (8,25) `#9d8872` to `#906273` — Remove a lone grey dot surrounded on all four sides by the same shaded fur..
- `item.wolfskin_cloak` C: (13,10) `#906273` to `#9d8872` — Join the pale shoulder fur area beside the hood..
- `item.dragons_eye` C: (6,9) `#8e5b37` to `#ffe47d` — Join the pale upper-left gold claw face; remove an isolated brown reduction pixel..
- `item.dragons_eye` C: (6,18) `#8e5b37` to `#ce5e00` — Join the orange side field immediately above and below this isolated brown reduction pixel..
- `item.dragons_eye` C: (10,23) `#8e5b37` to `#ffe47d` — Join the pale lower gold setting face on its right and below..
- `heartsong.drowned_bell` C: (10,30) `#000000` to `#004e22` — Restore the final river-weed tip pixel lost in block reduction, joining the weed above and meeting baseline row 30..

Supported palette swaps: none.

Final `python3 art/tools/validate.py --sheets`: **40/40 batch-12 assets OK, zero warnings**. Full manifest:
**147 OK, 120 failed, 0 missing (0 P1 missing), exit 1**. All 227 non-batch outputs and check results are
unchanged. The 120 remaining failures are inherited round-1 assets awaiting batches 13-15. No caps, quiet
limits, pixelizer or validator were changed. Historical study and dedicated batch-9s/10/11 previews remain intact.

**Continue batches 13-15, then the documented game integration.** The complete round is not yet finished.
