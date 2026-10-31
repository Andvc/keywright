# Chosen direction: C2 at 4x, without outlines

Owner's instruction, 2026-10-30: **“C2 4X;可以有细节”** — C2, 4x; detail is welcome.

Owner's correction, 2026-10-31: **“回退。C2/4x但是不要描边”** — roll back; C2 at 4x, but no outlines.
Commit `f4ee68b` (the outlined batch 9r) was reverted by `9539d8f`. This correction overrides the outlines
visible in the historical C2 reference and the old automatic 1-pixel silhouette rule.

The visual reference is [C2 A](raw/C2_A.webp), processed as [C2 at 4x](out/C2_4x.png).
Keep its warm cartoon character, expressive farmer, material cues and lived-in setting, using colour shapes and
cel-shaded material tones without inked outer or internal contours. The world stays
320x180 at 4x; the sheet, inventory objects and icons retain their specified UI sizes at 2x. This approves a direction;
the rebuilt batch-9 anchors were approved on 2026-10-31 before starting batch 10.

## What carries into production

- Rounded silhouettes, cel shading and softer colour contrast in the distance, with no enclosing strokes or inked
  internal contours. Eyes, a mouth, natural seams and cast shadows may be dark; they must not trace every edge.
- Every round-2 `redo` asset has `outline: false`; pixelization adds no silhouette ring. Retained round-1 UI,
  branding, effects and store art remain outside this redraw.
- Three or four readable features per figure or object, when its native size permits. Seams, nails, a patch, a cord,
  worn corners, fences, grouped flowers, herbs and a cart may tell the story. They are not automatically defects.
- Broad areas between details, a clear central trade area, plain paper where the game prints text.
- The existing layered layout. The study's misplaced top beam, shelf and face are not approved layout coordinates.
  Hob's height and placement must keep his face below the speech bubble; the counter edge remains scene row 119.

Do not strip the chosen style back to the first batch's almost featureless flat shapes. Do not copy C2's scattered
road specks or fine brushwork uniformly over every surface. The useful detail should survive at native size.

## Calibration retained from the C2 study

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
The no-outline correction retains these ceilings and the detail budgets; removing contours is not a reason to erase
the useful features or to relax the limits.

Colour caps: scenery 32 (C2 uses 28); frame and figures 24; shelf, counter, inventory objects, Heartsongs, curios and
lock faces 16; props, upgrade icons, emblem and story key 12; kind icons 8. Sky 10 and sheet 12 are unchanged. The
74-colour master palette, binary alpha, dimensions, margins and layer geometry remain unchanged.

The study's original prompts, outputs and review pictures remain a historical comparison. Do not rerun its review
after replacing the anchors: F1 is composed from production files and would silently change the original baseline.

## Anchor checkpoint

Batch 9s was completed on 2026-10-31: the same 13 anchors were remade without outlines, preserving C2's useful
details and valid layer geometry. All 13 pass, with zero warnings. See the
[review guide](../generated/_review/round2_batch9s_README.md) and
[delivery record](../generated/DELIVERY.md#batch-9s-c2-at-4x-without-outlines).
Batch 9r's rejected files remain in git and the local production archive.

The owner approved batch 9s on 2026-10-31: **“可以，下一步做吧”** — approved; do the next step.
Batch 10 (the remaining scene assets) was completed against these anchors on 2026-10-31: all 32 pass, with zero
warnings. See its [review guide](../generated/_review/round2_batch10_README.md).

The owner subsequently approved batch 10 and requested **“大致ok接着完成所有内容”**: continue and finish
all remaining work. This supersedes the batch-10 stopping point and authorizes batches 11-15 and the game
integration listed in `art/README.md`, retaining C2 / 4x / meaningful detail / no outlines.
Batch 11 delivers all 37 remaining figures, including Mara and the King, with zero errors or warnings.
See the [figure review guide](../generated/_review/round2_batch11_README.md). Continue the remaining batches
and integration without another direction checkpoint.

Batch 12 delivers all 40 remaining inventory objects and Heartsongs, with zero errors or warnings.
The small objects retain their specified 32x32 native / 2x UI size; their useful identifying details and
no-outline treatment follow the same anchors. See the [object review guide](../generated/_review/round2_batch12_README.md).
Continue batches 13-15 and integration under the existing authorization.
