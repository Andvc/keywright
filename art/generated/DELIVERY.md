# Keywright art delivery

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

## Remaining work

Batches 1-8 are pending. The report lists undelivered assets explicitly.
This is a staged delivery, not yet a complete P1/P2 replacement set.
Game-side integration is outside this art brief.
