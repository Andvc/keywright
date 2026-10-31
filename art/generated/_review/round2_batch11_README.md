# Round 2, batch 11: figures

All 37 remaining figures are delivered in the approved C2 / 4x / no-outline direction. All pass with zero
warnings. The previously approved Hob is included in contact sheets as the comparison anchor.

- Full figures at 4x: [page 1](round2_batch11_figures_1.png), [page 2](round2_batch11_figures_2.png),
  [page 3](round2_batch11_figures_3.png), [page 4](round2_batch11_figures_4.png).
- Actual screen-size counter crops: [1](round2_batch11_counter_1.png), [2](round2_batch11_counter_2.png),
  [3](round2_batch11_counter_3.png), [4](round2_batch11_counter_4.png), [5](round2_batch11_counter_5.png).
- [Identity reveals](round2_batch11_reveals.png): Aldo / King and Hood / Mara.
- Full composites: [Vexis](round2_batch11_vexis_playview.png), [Orrin](round2_batch11_orrin_playview.png),
  [Vane at night](round2_batch11_vane_playview.png), [Nim in the morning](round2_batch11_nim_playview.png).

These are `scene.py` composites with the existing reference UI, not live game captures. The reference speech
is Hob's text on every crop; it demonstrates placement, not character dialogue. The counter hides lower bodies,
short figures are lifted by the prescribed amount, and faces remain below the bubble. Tall hats, horns and crowns
can extend behind the bubble. Full figures and signature details were also reviewed independently.

Generation used the built-in image tool, with 78 candidates: two per figure, Snikkit C/D, Wenna C and Vexis C.
Every request attached the no-outline Hob anchor and palette. King also referenced the selected Aldo;
Mara referenced Hood's cloak. Exact prompts, reference hashes, candidate checks, commands and selections are in
[the provenance record](../_workflow/round2_batch11.json).

Tam uses three recorded palette swaps to correct yellow-looking skin; Isolde has five brown eye pixels repaired
after palette reduction. No outline ring, blur, smoothing or validation-limit changes were used.

The full manifest now reports **107 OK, 160 failed, 0 missing**. The remaining failures are inherited assets in
batches 12-15, which are authorized next, followed by game integration. The 230 assets outside batch 11 retain
their exact output hashes and check results. Historical style-study and batch-9s/10 dedicated previews remain intact.

Reproduce this review with `python3 art/generated/_workflow/round2_batch11_review.py`; regenerate the standard
contact sheets with `python3 art/tools/validate.py --sheets`.
