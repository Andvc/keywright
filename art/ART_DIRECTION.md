# Keywright: art direction (round 2: a quiet scene in layers)

This is the style bible for every picture in the game. `art/manifest.json` lists each asset with its own spec;
`art/tools/scene.py` lays out the shop scene; this document is what they all share. Where they disagree, the
manifest's per-asset spec wins for that asset, and the disagreement is a bug to report.

Round 1 (0.23) replaced the code-drawn placeholders and came out too busy to live with. Round 2 (0.25) redraws it in a
different way, and builds the shop from layers. Read "Why round 2" first: it's the reason for every rule below.

## The game, for an artist

Keywright is a 2D merchant game set in **Aldmark**, a sword-and-sorcery realm. Three hundred years ago, every treasure
was shut into a warded chest to starve the demon of greed, **Avarax, the Gilded Maw**. The chests never went away. Folk
trade them unopened, and only a **Keywright** can open one, and only after buying it. You are one of Uncle Bram's kin,
behind a shop's counter. Sellers come up to it one by one, each with a chest. You listen to it (shaking it, dropping
it), buy or pass, and open what you bought: the things come flying out onto the counter. Over a run the shop moves
inward, from a roadside stall to the Royal Emporium by the **Gilded Gate**. Meanwhile the **Gilded Guild** collects a
tithe that is secretly feeding the Maw. `docs/world.md` is the world bible; read it before drawing anyone or anything.

The screen is the view across your counter: the place beyond it, the seller standing on the other side, the chest
between you, your appraisal sheet at your right hand. **The player looks at it for hours**, reading the sheet,
listening and thinking. The art is the room they think in: it should be pleasant to sit in, it should feel like a real
place at a real time of day, and it should never pull their attention away from the chest. The game draws the
spectacle (light from the seam, sparkles, shake); the pictures themselves stay calm.

## Why round 2

The owner's verdict on round 1: far too cluttered, too much intricate detail, shapes that aren't clear or simple. The
backgrounds were attractive at first glance and uncomfortable to look at over time; the chests looked worse than the old
code-drawn ones. The goal is art that is **comfortable and natural to look at**, not art that keeps drawing attention to
how impressive it is.

Measured at each asset's native size (`artlib.calm_metrics`, over the inside of the picture: the outline and the
silhouette's edge don't count):

| Assets | Lone specks | Edges | Brightness step |
|---|---|---|---|
| 0.22's code-drawn backgrounds (at 320x180) | 0.003 | 0.096 | 5.4 |
| Round 1's backgrounds and scenes (at 320x180) | 0.078 | 0.348 | 11.0 |
| 0.22's code-drawn things | 0.104 | 0.50 | |
| Round 1's things / curios | 0.405 / 0.413 | 0.746 / 0.744 | |
| Round 1's portraits (76x76) | 0.128 | 0.398 | 21 |
| Round 1's upgrade / kind icons | 0.342 / 0.233 | 0.712 / 0.609 | |
| The chest drawn in code (0.25) | 0.000 | 0.313 | 13.8 |
| A flat illustration made into pixels by round 2's pipeline: a figure / a place / a counter | 0.000 | 0.044 / 0.025 / 0.062 | 2.5 / 0.8 / 1.9 |

*Lone specks*: the share of pixels unlike all four of their neighbours. *Edges*: the share of neighbouring pairs that
differ. *Brightness step*: the mean brightness difference between neighbours (0-255). A third of the pixels in a typical
round-1 thing were lone specks: texture, not drawing.

Where it came from:

1. **We asked a model for pixel art.** A model's idea of pixel art is the glinting, textured, detail-at-every-pixel
   illustration style. However the prompt asked for "clean pixels", the drawing it produced was busy, and round 1's
   anchors carried that into every batch.
2. **The brief asked for density.** One art pixel to two screen pixels everywhere: the chest went from 16 art pixels wide
   to 96, and every extra pixel became room for detail.
3. **The descriptions asked for detail**: visible grain, rivets, runes, five or six features a thing. The colour caps
   were generous (48 for a background, 32 for a portrait).
4. **The pipeline kept everything.** Backgrounds were paintings shrunk by area, so brushwork became pixel noise, and
   nothing measured clutter.
5. **One picture per screen.** Each shop was a single painting with the counter, the shelf and the view baked in, at
   full contrast everywhere and the same at every hour.

Round 2's answer:

- **Never ask for pixels.** Codex generates **flat-colour illustrations** (clean vector-like shapes, flat fills, no
  outlines, no texture) on a flat magenta background, and `pixelize.py --mode flat` makes the pixel art: it gives each
  art pixel the commonest colour of the block of the picture it covers, so flat areas stay flat and nothing finer than a
  block survives. `art/reference/scene/flat_to_pixels.png` shows it.
- **A coarse grid for the world**: the scene is drawn at 320x180, four screen pixels to an art pixel.
- **A detail budget**: two details a figure or thing, three or four elements a place.
- **The scene in layers**, each with one job (below), lit for the time of day.
- **Limits that are measured**: every asset has quiet limits and the validator fails anything busier. Every round-1
  asset fails them; flat art made into pixels passes them easily.
- **The chest is drawn in code** (`tools/custom_art.py`): the thing the player looks at most. Its sheet,
  `art/reference/quiet/chest_drawn_4x.png`, is the bar for how simple a sprite should be.
- **Anchors first**: one of each kind of asset, approved by the owner before anything else is made.

Post-processing can't fix a busy drawing (0.25 tried flattening round 1's art: it came out smeared and blotchy). The
drawing itself has to be simple, which is why the generator never draws pixels.

### What batch 9 taught

Batch 9, the anchors, was the method's first real test, and it holds: all thirteen pass every quiet limit with room to
spare (the stall's place: lone specks 0.000, edges 0.027, brightness step 0.6; Hob: 0.002, 0.119, 6.7), and the stall
reads calm at every hour. What went wrong wasn't the drawing:

1. **The pipeline joined areas the model drew apart.** Its colour snapping joined clusters by their nearest pair, so the
   in-between tones along an edge chained two areas into one: Hob's straw brim melted into his face, and his beard into
   it too. It now joins clusters only when every pair of their colours is alike, leans to the ramps the prompt names
   when two palette colours are about as near (the stall's earth road had gone gold), keeps areas drawn apart apart (the
   smaller steps a shade along its ramp), and gives the rim anti-aliased into the magenta the colour inside it.
   `--recolour` puts right a material that still lands in the wrong ramp.
2. **The brief asked the stall's counter for "grey-brown"**, with the night ramp beside umber: the model drew a taupe
   the palette could only meet half purple, half brown. It's plain dark brown wood now, in umber.
3. **The brief gave the midday sky azure 4**, the palette's strongest blue, which made it the loudest colour on the
   screen. It's azure 5-6 and steel 4 now: a soft, light blue.

And the owner found the anchors calm but a little too plain. So before batch 10, the style study (`art/study/`) draws
the stall's counter in four styles at three levels of detail and at both densities. Batch 9 is then made again in the
style chosen, and this document is rewritten for it: the preamble, the detail budgets, and the quiet limits, taken from
the chosen variant's numbers.

## The look in one line

**Flat-colour storybook pixel art**: the flat colour plates of an old picture book, in chunky pixels. Big shapes, few
colours, calm surfaces, soft light that changes with the hour, and detail only where it tells you what a thing is.

### Pillars

1. **Quiet first.** Every picture is a few large, flat shapes. When in doubt, leave it out: an empty plane of one colour
   is a feature, not a gap.
2. **Each layer has one job.** The sky says the hour, the place says where you are, the figure says who you're dealing
   with, the frame says whose shop it is, the counter is where the trade happens, the sheet is your reasoning. Nothing in
   a layer competes with another layer's job.
3. **Shape and value, not detail.** Everything reads at its smallest size by its silhouette and its light and dark.
4. **No texture, no line work.** Wood is flat with a few plank seams, stone a few flat blocks, cloth flat with a fold
   or two, hair two or three flat masses. Shapes meet shape to shape, without outlines (the pipeline adds a single dark
   outline around sprites, outside them).
5. **A value plan.** The sky and the place are muted and low in contrast; the frame and the counter a little darker
   (they're nearest, in the shop's shade); the figure and the chest carry the contrast; the sheet is light paper with
   dark ink. See `art/reference/scene/counter_layout.png`.
6. **Warm light, cool shadow.** One gentle light from the upper left; shadows lean violet, highlights warm yellow; two
   or three tones a material, at most a highlight or two an object. The time of day is the game's: it changes the sky
   and tints the rest.
7. **One world.** Same palette, same light, same flat style everywhere: a curio and a figure drawn by different hands
   must still belong on the same counter.
8. **The game draws the magic.** Glows, auras, sparkles, light rays, enchantment shimmer, rarity colours around slots,
   shadows under things, lantern glow and screen shake are all drawn at runtime. Never bake them in.

## The shop scene

The counter screen is built from layers (`art/tools/scene.py` has every number; `art/reference/scene/layers.png` shows
them one by one). The world is a **320x180 grid at 4x**; the appraisal sheet, things and icons are UI-sized, at 2x.
Back to front:

| Layer | One per | Its job | Size, where it goes |
|---|---|---|---|
| **sky** | time of day (morning, midday, evening, night), shared by every place | the hour | 320x180, opaque, full screen |
| **place** | shop tier | where you are: the view across the counter | 320x180; the sky keyed out (transparent) |
| **figure** | person | who's at the counter | 96x160, full length, feet on the bottom row; stood behind the counter on column 52, feet 68 rows below its edge (off screen), so the counter hides their legs |
| **shelf** | shop tier | what you show off (kept things stand on it) | 136x40, hung from the frame; its board's top on row 49 (x 75-201) |
| **frame** | shop tier | whose shop it is; carries the top bar | 320x180: a band across the top (rows 0-13 under the top bar, plain and dark; the structure ending by row 34) and posts down the sides (within 20 columns of each edge); the middle open |
| **lanterns** | shop tier | evening coming on | 16x24, hung on the posts at (8, 60) and (311, 60); lit in the evening and at night (the game makes the unlit one and draws the glow) |
| **counter** | shop tier | where the trade happens | 320x61, rows 119-179: its far edge on row 119 (screen y 476) |
| **props** | upgrade | what you've bought, on the counter | small sprites standing on row 119 either side of the chest (columns 99 and 183) |
| **chest** | (drawn in code) | the puzzle | 48x36, standing on the counter's edge in the middle |
| **sheet** | shop tier | your appraisal, grander at every move | 216x320 at 2x, behind the slip (screen x 820-1252, y 66-706) |

The interface sits on top: the top bar on the frame's band; the speech bubble top left, over the seller's head; the
seller's words, the commission board, the queue and curios on the counter; the slip printed on the sheet in dark ink.
`art/reference/scene/counter_layout.png` marks every line and every covered area.

**The time of day.** A day runs from morning (its first customers) through midday to evening (its last ones and the day's
end); the Guild comes at night. The game swaps the sky and lights the rest with a tint (morning a little cool, evening
warm and lower, night dark blue), lights the lanterns in the evening and at night, and draws their glow. So each place,
frame, shelf, counter and figure is drawn once, in neutral daylight, and looks right at every hour; only the sky is drawn
four times. `art/reference/scene/times_of_day.png` shows it.

**The view.** You stand behind your counter; the seller stands across it, facing you. The place is what's behind them:
the crossroads beyond the stall, a shop's front (its door, its window onto the street) in town. Keep the left third and
the middle simple (the seller stands at the left, the chest in the middle in front of them); put the place's landmark in
the middle distance just right of centre; the right third is under the sheet most of the time.

## How to draw for the pipeline

- Draw a **flat-colour illustration**, not pixel art: crisp vector-like shapes, each one flat colour. No outlines, no
  gradients, no texture, no brush strokes, no dithering, no pixel grid.
- **Big enough to survive**: the pipeline gives each art pixel the commonest colour of its block of the picture, so
  anything thinner than about a hundredth of the picture's width (a hair-thin line, a tiny stud) disappears or breaks.
  Make features bold: eyes are clear dark dots, a mouth a short simple shape.
- **Neighbouring parts in clearly different colours**: a hat unlike the face under it, a beard unlike the skin, a sleeve
  unlike the smock over it. The pipeline gives each flat area its nearest palette colour, and two parts drawn in nearly
  the same colour can land on one (batch 9's Hob: a straw brim in the face's tan).
- **Flat pure magenta (#FF00FF) means empty**: the background behind a sprite, the sky in a place, the open middle of the
  frame. Never use magenta or hot pink in the subject (violet and crimson are fine).
- **Fill the frame the way the category asks** (each prompt says how): a figure from the top of the head to the feet, a
  thing on the bottom edge, the counter as the bottom third. The pipeline fits it: a figure to its height, a thing to its
  box, the shelf so its board lands on its row.
- Generate at the manifest's `gen_size` (1536x1024 for scenes, 1024x1536 for figures and sheets, 1024x1024 for the rest).

## Technical canon (not negotiable)

| Rule | Value |
|---|---|
| Screen | 1280x720, scaled up to the player's window by whole numbers |
| Pixel density | the world (sky, places, figures, the frame, shelf, counter, lanterns, props, story scenes, the chest): **four screen pixels to an art pixel**, a 320x180 grid. Interface-sized pictures (the sheet, things, Heartsongs, curios, icons, lock faces): **two** |
| Scaling | integer only, nearest-neighbour; never rotate, skew or resample a finished asset |
| File | PNG, 8 bits per channel, binary alpha (every pixel fully opaque or fully transparent) |
| Palette | the master palette only (`art/palette/keywright.hex`, 74 colours in 14 ramps): every opaque pixel an exact palette colour (the pipeline snaps them, an area at a time) |
| Colours per asset | places and story scenes 24; figures and the frame 16; things, Heartsongs, curios, sheets and lock faces 12; the sky, shelf, counter and props 10; upgrade icons, the emblem and the Key 8; kind icons 6 |
| Quiet | at most the category's lone specks, edges and brightness step (below); the validator fails anything busier |
| Value | the counter's opaque pixels average 45-115 in brightness (0-255), the place 45-150, the frame 25-130, the sheet 165-228 |
| Outline | sprites (figures, props, the shelf, things, curios, icons, lock faces) and the frame's inner edge: a 1-pixel outline in **night2 `#1a1420`**, outside the shape (the pipeline adds it). The sky, places, counter, sheets and scenes have none |
| Text | none (no letters, numbers, runes that read as letters, signatures or watermarks), except the logo |

### How quiet is measured

`calm_metrics` reads the inside of a finished asset at its native size. The manifest gives each category its limits
(`calm`), and `validate.py` fails an asset over any of them with "too busy: ... (draw it simpler)":

| Category | Lone specks | Edges | Brightness step |
|---|---|---|---|
| Skies | 0.005 | 0.05 | 3 |
| Places, story scenes | 0.01 | 0.14 | 6 |
| The frame | 0.02 | 0.20 | 10 |
| Counters | 0.01 | 0.10 | 5 |
| Sheets (and their middle must be plain: edges 0.02) | 0.01 | 0.08 | 4 |
| Figures | 0.03 | 0.18 | 9 |
| Things, Heartsongs, curios, props, shelves | 0.06 | 0.50 | |
| Upgrade icons, the emblem, the Key / kind icons | 0.06 | 0.55 / 0.60 | |
| Lock faces | 0.03 | 0.20 | 9 |

Passing the numbers is necessary, not sufficient: the review still asks whether anything draws the eye that shouldn't.
`pixelize.py` also merges a lone speck or a tiny region into its neighbour when the colours are close, but it can't
rescue a busy drawing, and nothing else should try: **regenerate it simpler; never blur, posterize or smooth a picture
to pass**.

### Camera and perspective

- **The scene**: straight ahead at standing eye level, across the counter. The place is a flat stage (no floor of tiles
  or boards drawn in perspective lines; a road running away is one plain shape); the counter is a flat strip whose far
  edge is a straight line.
- **Figures**: full length, standing upright, facing you, **turned a little to the right** (toward the chest), eyes
  looking right or at you. Frontal and symmetrical: the Hooded Woman, the Stranger, Skrae and the Maw.
- **Things, curios, props, icons**: three-quarter view from slightly above, as if resting on the counter; long things
  on a diagonal from lower left to upper right.

### Light and shading

- One gentle warm light from the upper left. Each material in two or three flat tones in broad areas: a lit side, a
  shaded side. Shadows go one ramp step darker and toward violet.
- Highlights: one or two small shapes of the lightest tone (or white on metal, glass and gems), on the upper left of
  the main curve. Never a highlight on every edge, never glints scattered over a surface.
- Draw everything in neutral daylight: the game makes it morning, evening or night.

### Shape language

| Who or what | Shapes | Colours |
|---|---|---|
| Village folk, Bram's kin | round, soft, slightly stocky | homespun browns, greens, faded red, warm skin |
| Thornwick and the Old Wood | organic, leafy, crooked | deep greens, moss, birch white, amber light |
| Crownhold | upright, tidy, tall | slate blue, brick orange, ink, brass |
| The Royal Emporium | grand, symmetrical, simple | crimson velvet, gold, white marble |
| The Gilded Guild | rigid, symmetrical, polished; the emblem is a watching eye over a keyhole on a gold coin | black, cold gold, a little sickly green in the shadows |
| Demons and the Maw | sharp, horned, top-heavy, glowing eyes | crimson, black, tarnished gold, ember orange |
| Archmages and the Keyhold | tall, starry, flowing | indigo and violet with cyan rune light |
| Heartsongs | handmade, worn by love, never perfect | warm, with one mythic-red accent each |

### The palette

`art/palette/keywright.hex` (also `.gpl` for GIMP/Aseprite and `.png` as a swatch to show the model) holds 74
colours in 14 ramps, dark to light; `art/palette/ramps.md` says what each is for. Each prompt names the ramps to lean on;
the pipeline snaps every colour to the palette. Use few colours at once: a picture that touches every ramp is never
quiet. The game's own colours are in it: the UI gold `#f2c14e`, magic violet `#c792ff`, the outline `#1a1420`, and the
rarity colours below.

### Rarity colours

The UI frames everything in its rarity's colour, so an asset never needs a coloured border. Quality climbs by
**material and one accent**, not by more ornament:

| Rarity | Colour | In the art |
|---|---|---|
| Common | `#c9c4d4` (night8) | plain, honest materials: silver, iron, wool, glass, quartz |
| Uncommon | `#62d46f` (leaf5) | a craftsman's work: elven, dwarven; a green accent where it fits |
| Rare | `#4ea3ff` (azure4) | fine work with a touch of magic: sapphire, moonsilver, blue fire |
| Epic | `#b46bff` (arcane4) | powerful and arcane: violet light, lich-craft, archmages' things |
| Legendary | `#ffb52e` (flame3) | a marvel: dragon gold, phoenix fire, the archmages' own codex |
| Mythic | `#ff4058` (crimson5) | the nine Heartsongs only |

Fakes (demon gold, paste gems, a scroll case full of sand) look convincing at a glance and wrong on a second look.

### Places

| Place (tier) | The view across the counter | Key colours |
|---|---|---|
| Brindle Crossroads, Bram's Stall (1) | the crossroads: a road to the horizon, a signpost, hills with a castle, a river and a mill | leaf, moss, umber, sea |
| Thornwick Market (2) | inside a stone shop: its open door onto the square, the Old Wood's pines beyond | night (stone), umber, leaf |
| Crownhold (3) | a panelled shop: its wide window onto roofs and spires, the door with its bell | umber, azure (slate), brass |
| The Royal Emporium (4) | a grand hall: columns, drapes, an archway onto the avenue and the Gilded Gate, faintly sickly gold | crimson, gold, marble |

## Categories

Sizes are native (art pixels). The manifest's `redo` says what round 2 redraws; the UI frames, effects, logo, app icons
and store art stay as round 1 made them.

### Sky (`sky.*`)

320x180, opaque. Three to five broad horizontal bands, a simple sun or moon, a cloud or two, nothing else. Morning
(pale blue to peach, the sun low on the left), midday (clear soft blue, white clouds), evening (violet to orange, the
sun low on the right), night (deep blue, a moon, a few stars far apart).

### Place (`place.*`)

320x180, the sky keyed out (magenta in the generated picture, transparent once pixelized). The view across the counter,
in three to five large, muted, low-contrast planes; nobody in it; no counter in it (rows from 112 down are hidden by the
counter, but keep them opaque ground or floor). The horizon or the far wall's foot a little above the middle.

### The shop's frame (`frame.*`)

320x180, transparent in the middle. Across the top, the shop's own structure nearest you (the stall's awning, a timber
beam, an oak lintel, a velvet valance), its topmost rows (0-13, under the top bar) a plain dark band right across, the
structure ending by row 34; down the sides, posts within 20 columns of each edge. A little darker than the place.

### Display shelf (`shelf.*`)

136x40: the board across the bottom (its top on row 34 of the canvas, which the game puts on scene row 49), its
supports (ropes, chains, rods) rising from its ends to the top edge (they disappear behind the frame). Nothing on it:
kept things stand on it in the game.

### Counter (`counter.*`)

320x61, opaque: the counter's top seen from behind it, its far edge the top row with a slightly lighter lip. Plain and
mid-dark (average brightness 45-115): text, the board and the chest sit on it. A few plank seams at most.

### Figures (`figure.*`)

96x160, transparent, full length, feet on the bottom row, horizontally centred. Each has a `height` in the manifest
(its whole silhouette, hats and horns included): a goblin is 76, most adults 112-128, a knight 142, the archmage in his
hat 146. The pipeline scales each figure to its height, so they compare. Storybook proportions (the head about a fifth
of the height), arms relaxed, **two signature details** drawn large, clothes down to the feet, flat skin tones, eyes as
clear dark dots. The game crops each face for the queue, the notebook and the character cards, so keep the head
unhidden (hats are fine; a hood can shadow the face only where the description says so).

### Appraisal sheet (`sheet.*`)

216x320 (shown at 2x), opaque: light warm paper, **plain and empty in the middle** (the slip is printed on it in dark
ink: rows, totals, buttons), with a narrow rim around it and a small ornament only in the corners. It grows grander with
the shop: rough unbleached paper nailed to a plank; a cream sheet on a clipboard; a ledger page in a leather folder;
ivory vellum in a gilded frame with a wax seal.

### Props (`prop.*`)

Small sprites at 4x density, transparent, outlined: lanterns hang from their hook at the top (lit, glass glowing warm
yellow; the game darkens the glass by day), the rest stand on the bottom row (the Witchlight, the Lodestone, the Brass
Scales, the gavel, the Royal Seal, the Patrons' Book, the lockpicks). One simple object each, a little smaller than the
chest.

### Things (`item.*`) and Heartsongs (`heartsong.*`)

32x32, transparent; the content (with its outline) within 30x30, centred, its lowest pixels on row 30. Shown at 2x on
the counter and the shelf. One bold shape, two details at most; the six kinds climb from common to legendary by material
and one accent. **Heartsongs** are the nine rarest things in the realm: warmer, older, more personal, each with one
mythic-red accent.

### Curios (`curio.*`)

28x28, transparent, centred, shown at 2x in 56 px slots. One iconic object that hints at what it does; rarity in the
material (plain, good, fine with one gem, gold with one gem or rune).

### Icons, emblems and story props

Upgrade icons 24x24 (8 colours), kind icons 16x16 (6 colours, map symbols), the Guild's emblem 24x24, the Aldric Key
96x24 with its four shards cut from it along jagged lines. Bold, simple, readable at 1x.

### Lock faces (`lock.face.*`)

160x160 at 2x, transparent outside the lock: flat metal, a keyhole in the middle and a plain dark ring track from radius
52 to 68 (the game draws the pick there).

### Story scenes (`scene.*`)

320x180, opaque, with their own sky: three or four large muted planes and one soft focal light; the middle and the
bottom third plain (figures stand there and the dialogue box covers the bottom).

### The chest

Drawn in code (`tools/custom_art.py`), not generated: 48x36, seven finishes on one silhouette, three lock makers' hasps
and plates laid over it.

### Kept from round 1

`ui.*` (nine-slice frames, drawn by script), `fx.*` (effect sheets), `brand.*` (logo, app icons) and `store.*` (Steam
art) aren't redrawn; their specs are round 1's.

## Generating with GPT-Image-2

1. **Anchors first.** Batch 9 is one of each new kind of asset: the midday sky, the stall's place, frame, shelf,
   counter, sheet and lantern, Hob the Farmer's figure, a thing, a Heartsong, a curio and two icons. Pixelize them, run
   `validate.py --sheets` (its `counter_stall*.png` mock-ups put them together at every hour), push, and **stop until
   the owner approves them**. Every later request passes its category's round-2 anchor as a reference. Never pass round
   1's art: its style is what round 2 moves away from.
2. **Prompt** = the flat style preamble + the category's frame + the asset's description and details (cut to the
   budget) + the palette ramps + what to avoid; the manifest's `prompt` is assembled. Adjust it only to fix a failed
   attempt, adding a correction at the end ("simpler: fewer, bigger shapes; flat colour; no texture" is the usual one),
   and record what changed.
3. **Request** `gen_size`, PNG, high quality, 2-4 candidates; keep the calmest that shows what the spec says.
4. **Pixelize**: `python3 art/tools/pixelize.py raw.png --id <id> --preview`.
5. **Validate**: `python3 art/tools/validate.py --only <id>`. Fix every failure by regenerating; editing a few pixels by
   hand is fine.
6. **Review** at the size it's seen: the counter mock-ups for the scene's layers and figures, the contact sheets beside
   the anchor for the rest. Look at a mock-up for a minute: if anything keeps drawing the eye, simplify it.

### Common failures and what to do

| Failure | Fix |
|---|---|
| The model draws pixel art anyway (a pixel grid, speckle, dithering) | regenerate restating "a flat vector illustration, not pixel art"; pass the anchor |
| "Too busy" from the validator | regenerate with "simpler: fewer, bigger shapes, flat colour, no texture"; cut a detail if it keeps failing. Never filter it to pass |
| Outlines or line work in the drawing | regenerate with "no outlines: shapes meet shape to shape" (the pipeline adds the outline) |
| Gradients or soft shading (banding after pixelizing) | regenerate with "flat fills only, two or three tones" |
| Thin parts break up or vanish (a staff, a rope, a smile) | ask for them bolder; or let them go if the figure still reads |
| Magenta left in the subject, or the subject keyed out | the subject mustn't be magenta or hot pink; regenerate, or pass `--key` with the background's actual colour |
| A material lands in the wrong ramp (an earth road gone gold, a shirt's shade gone yellow) | the pipeline snaps each flat area to its nearest palette colour, leaning to the ramps the prompt names; when one still lands wrong, `pixelize.py --recolour '#from=#to'` (palette colour for palette colour) puts it right all over the asset. Record it |
| Two parts the model drew apart come out one colour (a brim into the face) | the pipeline keeps drawn edges apart by stepping the smaller part a shade along its ramp, but not when that's far from what was drawn: regenerate with the two in clearly different colours |
| A place or frame off its lines (the counter's edge, the frame's band, the open middle) | move the crop with `--offset-y` (source pixels, down); regenerate if it can't line up |
| A figure the wrong height or off-centre | the pipeline scales it to its `height`: check the whole figure is in the picture, feet included |
| The sheet's middle isn't plain | regenerate: "plain and empty in the middle; ornament only in the corners" |
| Wrong perspective or light direction | pass the anchor; restate "lit from the upper left" |
| Text or rune-letters appear | regenerate with "no text, no letters" |
| Style drift across a batch | compare each new asset to its anchor side by side before accepting it |

## Review checklist

- [ ] `validate.py` passes: size, transparency, palette, colour count, outline, quiet, value, and the layer's lines
- [ ] In the counter mock-ups, at every hour, nothing draws the eye but the seller, the chest and the sheet
- [ ] A few big, flat shapes; no texture, no line work; at most two details on a figure or thing
- [ ] The layer does its one job and nothing else (no counter in a place, nothing in the frame's middle, nothing on the shelf)
- [ ] Lit from the upper left in neutral daylight; shadows cool, highlights warm
- [ ] Reads at its smallest size and is the subject the manifest describes
- [ ] Looks like its round-2 anchor
