# Batch 10: scenes and counter props

Delivered 2026-10-31 after approval of batch 9s. **C2 at 4x, with useful detail and no outlines.**
All 32 assets pass the unchanged validator, with zero warnings.

Start with [all four shops](round2_batch10_shops.png), then [the counter props](round2_batch10_props.png)
and [the four story scenes](round2_batch10_stories.png).

## Shop previews

The dedicated shop pictures use the same approved Hob and three approved small items in every shop so their
layers can be compared consistently. The world is 320x180 at 4x; the appraisal sheet and shelf items use 2x.
The existing study HUD, speech bubble and code-drawn chest are review fixtures. Their text still names Bram's
stall in every picture. These are composition mock-ups, not live game captures or new character assignments.

| Shop | Full 1280x720 preview | Standard four-hour preview |
|---|---|---|
| Bram's stall | [Play view](round2_batch10_stall_playview.png) | [Morning / midday / evening / night](counter_stall_times.png) |
| Thornwick | [Play view](round2_batch10_thornwick_playview.png) | [Four hours](counter_thornwick_times.png) |
| Crownhold | [Play view](round2_batch10_crownhold_playview.png) | [Four hours](counter_crownhold_times.png) |
| Emporium | [Play view](round2_batch10_emporium_playview.png) | [Four hours](counter_emporium_times.png) |

The standard validator pictures use placeholder people for the three later shops and inherited round-1 shelf
items awaiting their redraw batches. Their red rectangles in `counter_{shop}.png` mark interface areas;
they are not strokes in the artwork. Morning/evening/night skies and every shop layer are now delivered.

## What was checked

- No enclosing ink contours in the selected source illustrations or native assets; material rims, plank seams
  and actual shadows remain. The pixelizer also runs with `--no-outline`.
- Top bands cover the HUD, side posts stay outside the open view, shelves register at row 49, and counters at
  row 119. Hob's face remains below the speech bubble. Plain appraisal writing fields are unobstructed.
- All four lighting states retain a calm trade area. Paper keeps its readable interface value.
- Props remain legible at native size and 4x. Lamps retain glass and metal, the scales retain two level pans,
  and the very small gavel retains head, handle and sound block without decorative collars.
- The Guild eye contains a keyhole; Bram has his hearth, chair and opened chests; Keyhold has broken pillars
  and the violet keyhole; the vault focuses one beam on the Crown Chest before the Gilded Gate.
- Story scenes leave their lower third broad enough for dialogue. Individual enlarged assets are in
  [round2_batch10_assets/](round2_batch10_assets/).

World comparisons use the assembled native scene without the separate sheet, interface or code chest:

| Shop | Specks | Edges | Brightness step |
|---|---:|---:|---:|
| Stall | 0.011943 | 0.180123 | 8.7207 |
| Thornwick | 0.008939 | 0.189199 | 7.2222 |
| Crownhold | 0.009399 | 0.181427 | 7.4540 |
| Emporium | 0.008356 | 0.160453 | 6.9968 |

All remain below the C2 comparison ceilings of 0.05 / 0.27 / 14. These comparisons supplement the per-asset rules.

## Record and scope

The built-in image tool produced **77 candidates**: two per asset, twelve C corrections and a fourth gavel.
[Exact prompts and provenance](../_workflow/round2_batch10.json) record every candidate, reference hash,
actual source size, check, choice and final processing command. [Delivery notes](../DELIVERY.md#batch-10-scenes-and-counter-props-without-outlines)
explain prompt changes and remaining work. Full source PNGs and rejects are retained in the local production
archive; committed WebPs are small, lossy viewing copies.

The whole-manifest check is **70 OK, 160 failed, 37 missing (35 P1 missing), exit 1**. All failures and missing
files belong to later batches. All 235 non-batch-10 asset hashes/missing states and validation results match the
baseline. No new limits, recolours, manual pixel repairs or extra smoothing were used.

Reproduce these dedicated pictures with:

```text
python3 art/generated/_workflow/round2_batch10_review.py
```

The standard report and sheets come from `python3 art/tools/validate.py --sheets`, run after production
processing. Historical study pictures and dedicated batch-9s previews are preserved.
**Stop after this batch.** Batches 11-15 and game integration are not included.
