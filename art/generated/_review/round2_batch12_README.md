# Batch 12: chest items and Heartsongs

All 40 new objects pass with zero warnings. The two batch-9s category anchors remain unchanged.
The world direction is C2 / 4x / useful detail / no outlines; inventory sprites intentionally remain
32x32 native and appear at 2x in the game, as specified in the brief.

Start with [all 42 objects at actual 2x UI scale](round2_batch12_overview_2x.png).
The three inspection pages pair actual 2x with 4x enlargement:
[page 1](round2_batch12_objects_1.png), [page 2](round2_batch12_objects_2.png),
[page 3](round2_batch12_objects_3.png). Individual 4x files are in `round2_batch12_assets/`.

Check the shared round silhouettes and broad material faces; the coin stamps, weapon heads, cloak fasteners,
bottle shapes, book emblems and gem colours should identify each object at 2x. Heartsongs preserve their
individual silhouettes and signature features, including seven pipes and five chime rods.
Material rims, seams, scale gaps and internal cast shadows are intentional; no outline ring is added.

Review the standard `counter_*_times.png` sheets for all four shops and times. They show the current layered
art with reference UI; they are not live game captures. Remaining curio/icon/lock redraws and game integration
continue under the owner's existing authorization.

Rebuild these object sheets with `python3 art/generated/_workflow/round2_batch12_review.py`.
Exact prompts, appended corrections, reference and output hashes, all candidate checks, selections and manual
pixel repairs are recorded in [the provenance file](../_workflow/round2_batch12.json).
Full PNG sources and rejected candidates remain in the local archive outside the checkout; selected source
previews are small WebPs under `../_raw/`.
