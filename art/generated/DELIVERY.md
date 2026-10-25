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

## Remaining work

Batches 2-8 are pending. The report lists undelivered assets explicitly.
This is a staged delivery, not yet a complete P1/P2 replacement set.
Game-side integration is outside this art brief.
