# Style study

Batch 9, round 2's anchors, came out calm but a little too plain; round 1 was far too busy. Before anything else is made,
this study draws the stall's counter in four styles at three levels of detail and turns each into pixels the way the
game would show it. It lays them out side by side so the owner can choose a direction. `art/tools/study.py` holds the
prompts and makes the review pictures.

## What's compared

| | 1 Plain | 2 Lived-in | 3 Rich |
|---|---|---|---|
| **F** Flat storybook (round 2 as briefed: flat fills, no outlines) | F1: batch 9 as delivered | F2 | F3 |
| **C** Outlined cartoon (bold dark outlines, cel shading) | C1 | C2 | C3 |
| **P** Soft painted (gouache: soft edges, gentle gradients, haze) | P1 | P2 | P3 |
| **X** Pixel art (drawn as pixels by the model, calm 16-bit) | X1 | X2 (and X2d) | X3 |

- **Detail levels.**
  - *Plain* is batch 9's budget: two details on any part.
  - *Lived-in* gives each part three or four meaningful details: herbs and a bell on the awning, a cart on the road, flowers by the fence, smoke from the mill, nail heads on the counter.
  - *Rich* is a full, lively scene, but organised: most of the detail in the middle distance, sunlight and cast shadows, and calm areas left where the game's text sits.
- **Two densities.** F, C and P are drawn as illustrations and turned into pixels twice from the same picture:
  - on round 2's 320x180 grid, shown at 4x;
  - on round 1's 640x360 grid, shown at 2x.

  X is drawn as pixel art on its own grid: X1-X3 on 320x180, and X2d on 640x360.
- **One thing in each style.** The Dwarf-Gold Crowns, at 1x, at 2x (as on the table) and at 4x. Batch 9's version is beside them.

Every review picture is the counter screen at 1280x720. It shows the art first, then what the game puts over it: the top bar, the seller's speech bubble, the chest and the appraisal sheet. Under each picture are its measured numbers: lone specks, edges and brightness step, as in `ART_DIRECTION.md`. Whichever direction is chosen, its numbers become round 2's quiet limits.

## For Codex: making it

Work on `art/round2` after merging `prototype/0.25` into it, and change nothing outside `art/study/`.

1. **Prompts.** They're in `prompts.md` and `prompts.json`; `python3 art/tools/study.py prompts` writes them.
   - Scenes are 1536x1024 and items 1024x1024, with two candidates each and the palette swatch `art/palette/keywright.png` attached.
   - Send each prompt as it is. If an attempt fails, add a correction at the end and record it.
   - Never pass round 1's or round 2's art as a reference: each variant should stand on its own.
2. **Choose by the layout, not by prettiness.** Keep the candidate that best has:
   - the dark beam along the top;
   - Hob on the left, cut at the waist by the counter;
   - the counter's far edge two thirds of the way down, with its middle empty;
   - a quiet right third.
3. **Scene:** `python3 art/tools/study.py scene <ID> raw.png [--offset-y N]`.
   - `--offset-y` moves the 16:9 crop window down by N source pixels. Use it to bring the counter's far edge to screen row 476 in the review pictures, where the chest stands on it.
   - The review pictures have ticks at rows 196 and 476 to check it by.
4. **Item:** `python3 art/tools/study.py item <F|C|P|X> raw.png`.
5. **Review pictures:** `python3 art/tools/study.py review`.
6. **Commit and stop.** Commit:
   - `art/study/out/` and `art/study/review/`;
   - a small `.webp` of each kept raw image in `art/study/raw/` (PNG and JPEG there are git-ignored);
   - `art/study/NOTES.md`: for each variant, the candidate kept, its offset, any correction, and anything that went wrong.

   Push, and **stop**: the owner chooses the direction before any more round-2 art is made.

## For the owner: judging it

Look at these in order:

1. `review/references.png`: the two ends, round 1 and batch 9, on the same screen.
2. `review/overview_4x.png` and `review/overview_2x.png`: every variant as the counter screen, at each density.
3. `review/items.png`: the same thing in each style.
4. `review/<ID>_<4x|2x>.png`: any variant at full size. Give the ones you like a minute each: the game is looked at for hours.

Then name a cell ("P2 at 2x") or a mix ("F2's content with C's outlines").

Once you've chosen, the brief is rewritten for that direction:
- the style preamble;
- the detail budgets;
- the colour caps;
- the quiet limits, taken from the chosen variant's numbers.

Batch 9 is then made again in that style, before anything else.
