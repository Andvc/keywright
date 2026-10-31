# Chosen direction: C2 at 4x

Owner's instruction, 2026-10-30: **“C2 4X;可以有细节”** — C2, 4x; detail is welcome.

The visual reference is [C2 A](raw/C2_A.webp), processed as [C2 at 4x](out/C2_4x.png).
Keep its warm outlined cartoon character, expressive farmer, material cues and lived-in setting. The world stays
320x180 at 4x; the sheet, inventory objects and icons retain their specified UI sizes at 2x. This approves a direction;
the rebuilt batch-9 anchors still require review before batches 10-15.

## What carries into production

- Dark plum structural outlines, rounded silhouettes, cel shading and softer outlines in the distance.
- Three or four readable features per figure or object, when its native size permits. Seams, nails, a patch, a cord,
  worn corners, fences, grouped flowers, herbs and a cart may tell the story. They are not automatically defects.
- Broad areas between details, a clear central trade area, plain paper where the game prints text.
- The existing layered layout. The study's misplaced top beam, shelf and face are not approved layout coordinates.
  Hob's height and placement must keep his face below the speech bubble; the counter edge remains scene row 119.

Do not strip the chosen style back to the first batch's almost featureless flat shapes. Do not copy C2's scattered
road specks or fine brushwork uniformly over every surface. The useful detail should survive at native size.

## Calibration fixed before batch 9r generation

Measured with the unchanged `artlib.calm_metrics`, using `out/C2_4x.png` (28 palette colours). Coordinates below
are native pixels, left/top inclusive and right/bottom exclusive. The regional crops include some adjacent material;
they are reference samples, not magically recovered isolated production layers.

| Sample / crop | Specks | Edges | Brightness step |
|---|---:|---:|---:|
| Whole (0,0,320,180) | 0.042629 | 0.254738 | 13.0511 |
| Visible landscape (100,35,300,120) | 0.055312 | 0.338970 | 15.4866 |
| Hob region (12,15,100,124) | 0.078244 | 0.352864 | 21.8452 |
| Counter (0,128,320,180) | 0.014843 | 0.163432 | 6.5478 |

The whole image is the comparison benchmark (rounded ceiling 0.05 / 0.27 / 14), not a validator rule for a small
isolated object. Layer limits round the corresponding samples upward: scenery 0.06 / 0.35 / 16; figure 0.08 / 0.36 /
22; counter 0.02 / 0.18 / 7. The frame uses the whole-image ceiling. Sky and blank paper keep their existing quiet
limits because they provide quiet space. Small sprites and icons retain their existing native-size edge/speck limits;
their large silhouette-to-area ratio is not comparable to a full scene. Lock tracks keep their existing quiet limits.
These are ceilings, not targets to fill with noise, and are not raised to excuse a generated failure.

Colour caps: scenery 32 (C2 uses 28); frame and figures 24; shelf, counter, inventory objects, Heartsongs, curios and
lock faces 16; props, upgrade icons, emblem and story key 12; kind icons 8. Sky 10 and sheet 12 are unchanged. The
74-colour master palette, binary alpha, outlines, dimensions, margins and layer geometry remain unchanged.

The study's original prompts, outputs and review pictures remain a historical comparison. Do not rerun its review
after replacing the anchors: F1 is composed from production files and would silently change the original baseline.

## Anchor checkpoint

Batch 9r was completed on 2026-10-31: the brief and manifest were rewritten, all 13 anchors were remade, and the
layered stall and representative sprites were validated and visually reviewed. See the
[review guide](../generated/_review/round2_batch9r_README.md) and
[delivery record](../generated/DELIVERY.md#batch-9r-c2-at-4x---awaiting-owner-review).

This is the stop point for owner review. No later batch or game integration is approved by this direction choice alone.
