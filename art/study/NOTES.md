# Keywright style study — 2026-10-30

Status: study delivered; the owner selected **C2 at 4x, with detail allowed**, on 2026-10-30. See
[CHOICE.md](CHOICE.md) for the production brief calibration and the next batch-9r review checkpoint.
The study results and observations below describe the original study delivery and remain unchanged.

## Integration and scope

`prototype/0.25` at `16094aac662661e6c20baed205caa74978057501` was merged into `art/round2` from `965349fa53c7eefbc11f8570997950e6e152aa2c`. This was a clean fast-forward. All work after that merge is confined to `art/study/`; the upstream brief, processing tools, production art and game assets were not edited.

The 16 prompts in [prompts.md](prompts.md) / [prompts.json](prompts.json) each received two independently generated candidates: 24 scene candidates and eight item candidates, 32 in total. One candidate per prompt was kept. F1 remains the delivered batch-9 baseline and was not regenerated.

Generation used the built-in `image_gen` tool. Every call attached only `art/palette/keywright.png` as its colour reference. No round-1 or round-2 artwork was supplied. Original prompt text was sent unchanged, followed only by the corrections listed below when needed and this request metadata:

> Request metadata: Generate one independent candidate, PNG, {size}. The attached image is a colour palette reference only; do not reproduce the swatch or its layout. No other art reference.

`{size}` was `1536x1024` for scenes and `1024x1024` for items. The tool did not always honour those dimensions: kept C1-B is 1672x941, all eight item originals are 1254x1254, and rejected C3-A / P1-A are 1672x941 / 1672x940. Every other scene original is 1536x1024. Inputs were not stretched to fake the requested sizes; the repository processor handled their actual dimensions.

[raw/generations.json](raw/generations.json) records all 32 requests, exact appended text, source filenames, dimensions, SHA-256 hashes and keep decisions. [raw/selection.json](raw/selection.json) records the processing input and offset for each kept candidate. The 16 small WebPs in `raw/` are viewing copies of the kept originals, totalling about 1.3 MB. Full PNGs remain locally in the git-ignored `raw/` directory. Processing used those full PNGs, not the reduced, lossy WebPs; the WebPs alone are not byte-identical reproduction inputs.

## What to look at

1. [The two references](review/references.png): round 1 and F1/batch 9.
2. [320x180 at 4x](review/overview_4x.png) and [640x360 at 2x](review/overview_2x.png).
3. [The coin comparison](review/items.png): 1x, 2x and 4x.
4. The individual `review/<ID>_<4x|2x>.png` files at 1280x720. The matching `out/` PNGs show the art without UI occlusion.

The empty F1, X1 and X3 cells in the 2x overview are intentional and labelled by the review tool. They are not missing deliverables: only X2d is requested at the denser native pixel grid.

## Kept candidates, offsets and observations

Offsets are source-image pixels passed to `study.py scene --offset-y`. Positive values move the crop down. Item offsets do not apply.

| Variant | Kept | Offset | Why kept; remaining issues |
|---|---|---:|---|
| F1 | Existing batch 9 | n/a | Unmodified baseline art, composed again by the review tool. |
| F2 | B | 61 | Counter edge is closer to row 476 than A; the centre is clear and Hob is cut at the waist. B moves the cart to the right, mostly under the sheet. The correction reduced some detail, but the source still has gradients and fine foliage; Hob's face is obscured by the bubble after alignment. |
| F3 | A | 49 | A has the road travellers, an empty central counter and less right-side tabletop clutter than B. B has a large cloth in the paper area and omits the two travellers. A still exceeds a strictly flat-ink treatment; foreground plants and cast shadows make this a noticeably busier cell. |
| C1 | B | 0 | B keeps the beam, a simpler background and more of Hob's face visible than A. It arrived almost 16:9, leaving no useful vertical crop travel: the counter edge remains about 40 screen pixels below the 476 tick, so the review chest floats. A's lower counter and larger figure were also poor fits. |
| C2 | A | 73 | A has the straighter, higher counter and a clearer right tabletop; B puts a cloth, utensils and more flowers there. A includes the barrel cart and keeps the middle empty. The background has fine painterly detail rather than uniform bold outlines; the bubble covers Hob's face. |
| C3 | B | 39 | B includes the cart and two travellers and aligns the counter better than the almost-16:9 A. Its right cloth is covered by the paper; left-side vessels, flowers and hanging flags remain busy. Hob's upper face overlaps the bubble; the dark outlines make this high contrast after snapping. |
| P1 | B | 59 | B removes A's foreground crates, fruit and cloth, with broad open road and counter areas. It is the quietest new scene by all three 4x measures. The source remains more textured than the two-details budget, and the bubble overlaps the hat/upper face. |
| P2 | B | 80 | B moves the barrel cart to the left middle distance and leaves a cleaner right third than A. The requested counter alignment reaches the crop's lower limit and lands within a few screen pixels of the tick. Both candidates drift toward detailed, realistic painting; the correction only partly helps, and the bubble hides the face. |
| P3 | B | 59 | B retains both travellers and the cart and concentrates foreground props at the sides. A has a slightly higher counter but omits the cart. The middle stays usable; B's hand-on-cheek pose makes the bubble overlap more noticeable, and painted wood/foliage survives as many palette edges. |
| X1 | B | 73 | B has a higher, straighter counter and larger colour clusters than A. The middle and right table are clear. It still draws more than two details per part and does not maintain a perfect authored pixel grid; the bubble overlaps Hob's hat and upper face. |
| X2 | B | 65 | B aligns the counter with less crop movement than A and keeps the centre clear. Both omit the requested road cart; B also draws three birds rather than two. Left foreground vessels and cloth remain visible, and the bubble obscures the face. |
| X3 | B | -1 | B restores the cart and two travellers missing from A, and its counter already sits close to the target line. The central chest area is clear, with rich side clusters. Foreground produce and foliage are dense, and the upper face still partly overlaps the bubble. |
| X2d | A | 48 | A's counter is higher and its right middle distance is quieter than B's. Hob is farther toward the centre, so his face is readable beside the bubble. The paper covers the large right cloth. This is a separate 640x360-directed generation, not a higher-resolution processing of X2's source. |
| item_F | B | n/a | B has a more square, open front hole and a clear three-coin stack. The anvil survives as a broad mark; the hammer is weak at 1x. A's front hole is more foreshortened. |
| item_C | A | n/a | A has fewer decorative marks than B, a readable stack and simpler hammer/anvil shapes. B's larger hole comes with extra rune-like ornament. The outline and palette reduction consume much of the stamp detail at 1x. |
| item_P | B | n/a | B's broad square hole survives reduction better than A's narrow slit. The pile reads at table scale; the brushwork largely disappears in the 32x32 result and the small hammer is not separately legible. |
| item_X | B | n/a | B's coarser clusters and larger hole survive better than A. Automatic background detection initially failed; explicit magenta-key preparation fixed the purple rectangle and hole. The grid could not fit the sprite margin, so the unchanged processor used its documented area-reduction fallback; this is not a clean native 32x32 grid success. |

## Corrections appended to second candidates

These were appended after the full original prompt, never substituted for it. All other candidates used the original prompt plus request metadata only.

### F2-B

> Correction: strictly flat vector-like solid colour regions, no gradients and no painterly texture; simplify each part to three or four details. Preserve a substantial top beam within the central 16:9 band; put the counter far edge at two thirds of that band.

### C1-B

> Correction: plain means only a few large shapes and at most two details on any part; use flat cel colours and bold uniform plum outlines throughout, including the background, with no painterly texture. Keep the dark beam visible within the central 16:9 crop. In the 1536x1024 source, put the counter's far edge at about y=650, with its middle empty.

### P1-B

> Correction: enforce the plain detail level: at most two details on each part, broad smooth gouache colour shapes, no individual grass blades, leaf stippling, scattered pebbles or wood-grain texture. Keep the entire top beam inside the central 16:9 band and raise the counter's far edge to two thirds of that band; keep the middle empty.

### P2-B

> Correction: this is a soft gouache storybook illustration, not a realistic oil painting or photograph. Use simple broad shapes, softened edges, gentle gradients and only light painterly texture; three or four meaningful details per part. Preserve the empty centre and quiet right third. Keep the top beam within the central 16:9 crop and the counter edge two thirds down that crop.

### X1-B

> Correction: enforce the plain detail budget and uniform large pixel grid. Each pixel is one solid colour; big flat clusters in sky, hills, clothing and counter, no gradients, no microtexture, no stippled grass. Keep a large dark beam inside the middle 16:9 band and the far edge of the counter two thirds down that band.

### X3-B

> Correction: include the specified cart and two small travellers on the road in the middle distance. Keep the rich details organised around large flat pixel clusters, with a quiet right third and empty middle of the counter. Keep the counter's far edge two thirds down the central 16:9 band.

### item_X-B

> Correction: use an actual coarse 32 by 32 sprite grid, with large uniform square pixels and one solid palette colour per pixel, no subpixel detail or gradients. Keep the front coin's open square hole broad and square, and simplify the hammer and anvil stamps so they read at 32 by 32.

## Processing and review

Python 3.13.1 and Pillow 11.1.0 were used on Windows. The local `python3` command is a Windows App Execution Alias without an interpreter, so in the processing shell `python3` was aliased to the installed `python.exe`. Every kept scene and item was then run through the repository commands:

```text
python3 art/tools/study.py scene <ID> art/study/raw/<ID>_<candidate>.png --offset-y <offset>
python3 art/tools/study.py item <F|C|P|X> art/study/raw/<input>.png
python3 art/tools/study.py review
```

The illustration scenes use the same kept input for both densities. No custom repainting, smoothing or metric-threshold changes were made.

### Pixel coin background repair

The final visual inspection found a purple rectangle behind item X. Both X candidates have slightly varying magenta borders, so `pixelize.remove_background`'s automatic border-bin majority test left them opaque. This is a preparation issue, not intentional purple artwork.

Only for X-B, the existing repository `pixelize.flat_alpha` function was used with the explicit magenta key and the coin's manifest spec:

```python
import sys
sys.path.insert(0, "art/tools")
import artlib, pixelize

spec = artlib.asset(artlib.load_manifest(), "item.dwarf_gold_crowns")
pixelize.flat_alpha(
    artlib.open_rgba("art/study/raw/item_X_B.png"),
    spec,
    key=(255, 0, 255),
).save("art/study/raw/item_X_B_alpha.png")
```

Then `python3 art/tools/study.py item X art/study/raw/item_X_B_alpha.png` and `python3 art/tools/study.py review` were run again. The generated original remains unchanged; the prepared input's hash and operation are recorded in `raw/generations.json`. This removes the external key and key-coloured hole and uses the tool's existing edge decontamination. No tool source was changed.

The final item-X processing reported: `the grid found (35.1 px) gives (32, 25), too big for 32x32; reducing by area`. This fallback is part of the existing pipeline. The final item is transparent with no purple key pixels, but the fallback makes the X item a weaker test of an exact model-authored pixel grid.

## Measured results

Values are `specks / edges / brightness step`, from `out/log.json` and the review tool. These describe the full native art, including areas covered by UI; they are observations, not newly approved quiet limits.

| Variant | 4x: 320x180 | 2x: 640x360 |
|---|---|---|
| F1 / batch 9 | 0.002 / 0.068 / 5.2 | Not made at this density |
| F2 | 0.012 / 0.183 / 7.7 | 0.010 / 0.132 / 4.8 |
| F3 | 0.023 / 0.226 / 11.0 | 0.022 / 0.186 / 7.6 |
| C1 | 0.015 / 0.170 / 11.2 | 0.013 / 0.131 / 7.7 |
| C2 | 0.043 / 0.255 / 13.1 | 0.041 / 0.233 / 10.8 |
| C3 | 0.052 / 0.281 / 17.1 | 0.055 / 0.269 / 14.0 |
| P1 | 0.008 / 0.161 / 5.5 | 0.008 / 0.128 / 3.6 |
| P2 | 0.035 / 0.286 / 10.4 | 0.031 / 0.287 / 8.4 |
| P3 | 0.051 / 0.310 / 13.7 | 0.041 / 0.283 / 10.4 |
| X1 | 0.033 / 0.241 / 11.2 | Not requested |
| X2 | 0.063 / 0.324 / 14.9 | Not requested |
| X3 | 0.096 / 0.367 / 20.0 | Not requested |
| X2d | Not requested | 0.038 / 0.261 / 10.4 |

The round-1 reference measures 0.055 / 0.274 / 7.2 on its native grid. Comparing step and edge counts across different grids or different source compositions is not a controlled density-only test.

## Findings and limits for the choice

- F2 and P1 are the quietest new 4x options. F2 adds lived-in props; P1 shows how gentle painting becomes broad palette regions. F3 adds more narrative detail without reaching the edge density of the richest C/P/X cells.
- C increases outline contrast, but C2/C3 do not keep the bold line treatment equally throughout the landscape. F2/F3 also retain painterly gradients and foliage in the originals despite their flat-ink prompts. These cells show what the requests produced, rather than proving strict style adherence.
- P2/P3 retain enough brush and wood texture to become busy palette transitions. X3 has the largest 4x edge density and brightness step. Many cells are busier than the old round-1 reference by at least one measure; a richer cell should not automatically become the new quiet standard.
- Layout is the main shared limitation. Most generations place the beam and hanging shelf too high relative to the counter. Aligning the counter near screen row 476 crops out much of the top beam and leaves the shelf well above row 196. The supplied bubble then hides much or all of Hob's face in several cells. C1 retains more top structure but leaves the counter too low; X2d keeps the face most readable beside the bubble. Both the unoccluded `out/` art and full UI mockups should be considered when judging style.
- No single vertical offset can correct the shelf, face and counter simultaneously in these originals. The review UI and its two tick rows were not moved to conceal this. A chosen direction will still need correctly composed layered anchors.
- At 32x32 the silhouettes, holes and stack heights carry more information than the hammer/anvil stamps. F and P lose much of their distinct source finish; C has stronger outline contrast; X required the preparation/fallback documented above. Assess the 2x examples, not only the 4x enlargements.

## Verification and stopping point

All 16 prompt IDs have A/B candidates, one kept image, a saved WebP and a processing record. The 24 expected native PNGs and 25 review PNGs exist with the specified final sizes; scene alpha is fully opaque, item alpha contains transparency, and the item outputs have no remaining magenta/purple key background. Candidate hashes and unchanged original prompt prefixes were checked.

The final `python3 art/tools/study.py review` completed successfully. References, both overviews and the item sheet were inspected; full-size F2 and X2d mockups were also checked for layout. The regenerated F1/reference pictures are tool outputs from the existing baseline art, not new art.

No game build or gameplay regression was needed for these isolated study files. No production asset import, new anchor batch, other round-2 art, brief rewrite or quiet-limit change was made. The next decision belongs to the owner: choose a cell/density or a stated mix before any further round-2 art is produced.
