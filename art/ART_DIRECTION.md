# Keywright: art direction (round 2: quiet first)

This is the style bible for every picture in the game. `art/manifest.json` lists each asset with its own spec; this
document is what they all share. Where the two disagree, the manifest's per-asset spec wins for that asset, and the
disagreement is a bug to report.

Round 1 (0.23) replaced the code-drawn placeholders. Round 2 (0.25) redraws most of it because it was too busy to live
with. Read "Why round 2" first: it's the reason for every rule below.

## The game, for an artist

Keywright is a 2D merchant game set in **Aldmark**, a sword-and-sorcery realm. Three hundred years ago, every treasure
was shut into a warded chest to starve the demon of greed, **Avarax, the Gilded Maw**. The chests never went away. Folk
trade them unopened, and only a **Keywright** can open one, and only after buying it. You are one of Uncle Bram's kin.
The player sits behind a shop counter and listens to each seller's chest (shaking it, dropping it). Then they buy or
pass, and open what they bought: the things come flying out onto the table. Over a run the shop moves inward, from a
roadside stall to the Royal Emporium by the **Gilded Gate**. Meanwhile the **Gilded Guild** collects a tithe that is
secretly feeding the Maw. `docs/world.md` is the world bible; read it before drawing anyone or anything.

The screen is a stage: a shop background, a seller's portrait at the left, the chest in the middle, the appraisal slip
on the right. **The player looks at it for hours**, and spends that time reading the slip, listening and thinking. The
art is the room they think in. It should be pleasant to sit in and never pull their attention away from the chest.
Openings are the climax: the lid strains, light leaks from the seam, and each thing flies out and lands on the table
with its price. The game draws that spectacle (light, sparkles, shake); the pictures themselves stay calm.

## Why round 2

The owner's verdict on round 1: far too cluttered, too much intricate detail, shapes that aren't clear or simple. The
backgrounds were attractive at first glance and uncomfortable to look at over time. The chests had the same problem
and looked worse than the old code-drawn ones. The goal is art that is **comfortable and natural to look at**, not art
that keeps drawing attention to how impressive it is.

Measured at each asset's native size (`artlib.calm_metrics`, over the inside of the picture: the outline and the
silhouette's edge don't count):

| Assets | Lone specks | Edges | Brightness step |
|---|---|---|---|
| 0.22's code-drawn backgrounds (at 320x180) | 0.003 | 0.096 | 5.4 |
| Round 1's backgrounds and scenes (at 320x180) | 0.078 | 0.348 | 11.0 |
| 0.22's code-drawn things | 0.104 | 0.50 | |
| Round 1's things / curios | 0.405 / 0.413 | 0.746 / 0.744 | |
| 0.22's portraits (38x38) | 0.049 | 0.316 | |
| Round 1's portraits (76x76) | 0.128 | 0.398 | 21 |
| Round 1's upgrade / kind icons | 0.342 / 0.233 | 0.712 / 0.609 | |
| The chest drawn in code (0.25) | 0.000 | 0.313 | 13.8 |

*Lone specks*: the share of pixels unlike all four of their neighbours. *Edges*: the share of neighbouring pairs
that differ. *Brightness step*: the mean brightness difference between neighbours (0-255). At the size the player sees
them, round 1's backgrounds had two to four times as many edges as the old ones, the chest five to six times, and a
third of the pixels in a typical thing were lone specks: texture, not drawing.

Where it came from:

1. **The brief asked for density.** It set one art pixel to two screen pixels everywhere. The chest went from 16 art
   pixels wide to 96, and every extra pixel became room for detail.
2. **The descriptions asked for detail**: visible grain, rivets, runes, five or six features a thing, "careful
   ornament". The colour caps were generous (48 for a background, 32 for a portrait, 20 for a thing).
3. **The model's idea of pixel art** is the glinting, textured illustration style. Round 1's anchors were in that style,
   and every later batch followed them.
4. **The pipeline kept everything.** Backgrounds were paintings shrunk by area, so brushwork became pixel noise. Every
   speck survived the palette snap, and nothing measured clutter.
5. **No value plan.** Backgrounds had full contrast everywhere, competing with the chest and the slip.

What round 2 changes: a quiet style preamble, a detail budget, smaller colour caps, backgrounds drawn on a coarse grid
(320x180, the importer doubles them), limits on clutter that the validator enforces, a quiet stage in the pixelizer,
and new anchors that the owner approves before anything else is made. The chest is now drawn in code
(`tools/custom_art.py`), because it is the thing the player looks at most; its sheet, `art/reference/quiet/
chest_drawn_4x.png`, shows the level of simplicity every sprite should aim for. Post-processing can't fix a busy
drawing (0.25 tried flattening round 1's art: it came out smeared and blotchy), so the drawing itself has to be simple.

## The look in one line

**Quiet storybook pixel art**: warm, simple and readable, like the flat colour plates of an old picture book. Big
shapes, few colours, calm surfaces, and detail only where it tells you what a thing is.

`art/reference/quiet/busy_and_quiet.png` sets the target between two pictures: round 1's colour and sense of place, at
the calm of the old code-drawn art.

### Pillars

1. **Quiet first.** Every picture is made of a few large, flat shapes. When in doubt, leave it out: an empty plane of
   one colour is a feature, not a gap.
2. **Shape and value, not detail.** Everything must read at its smallest size (a 32 px thing at 1x, a portrait's face
   at 38 px) by its silhouette and its light and dark, never by ornament.
3. **A detail budget.** A thing, a curio or a portrait shows at most **two** details: the ones that say what it is,
   drawn large. Nothing smaller. A background shows three or four things beyond its walls, sky and counter.
4. **No texture.** Wood is flat colour with at most a few plank lines, stone is a few flat blocks, cloth is flat with a
   fold or two, hair is two or three flat masses. No grain, speckle, weave, strands, scratches or noise, anywhere.
5. **A value plan.** The background is muted, mostly mid to dark and low in contrast, with one soft light. The chest,
   the person and the things carry the contrast, and the UI sits on top of it all. See
   `art/reference/quiet/value_plan_counter.png`.
6. **Warm light, cool shadow.** One gentle warm light from the upper left. Shadows shift toward violet, highlights
   toward warm yellow (the palette's ramps already do this: walk a ramp, don't mix your own colours). Two or three
   tones per material; at most a highlight or two per object.
7. **One world.** Same palette, same light, same outline everywhere: a curio and a portrait drawn by different hands
   must still belong on the same counter.
8. **The game draws the magic.** Glows, auras, sparkles, light rays, enchantment shimmer, rarity colours around slots,
   shadows under things and screen shake are all drawn by the game at runtime. Never bake them into an asset (unless
   the asset is itself an effect, in the FX category).

## Technical canon (not negotiable)

| Rule | Value |
|---|---|
| Screen | 1280x720, scaled up to the player's window by whole numbers |
| Pixel density | **backgrounds and scenes: four screen pixels to an art pixel** (320x180; the importer doubles them to the 640x360 the game shows at 2x). The chest: four (it's drawn in code). Portraits, things, curios, icons and lock faces: two (their sizes are the game's and haven't changed; they're simpler by what's drawn, not by size). Portraits also sit at 3x in story scenes and 1x in the ledger |
| Scaling | integer only, nearest-neighbour; never rotate, skew or resample a finished asset |
| File | PNG, 8 bits per channel. Sprites RGBA with a transparent background; backgrounds and portraits fully opaque |
| Alpha | binary: every pixel is fully opaque (255) or fully transparent (0). No soft edges, no drop shadows, no glow halos |
| Palette | the master palette only (`art/palette/keywright.hex`, 74 colours in 14 ramps; see below). Every opaque pixel must be an exact palette colour |
| Colours per asset | backgrounds and scenes 24; portraits 20; things, Heartsongs, curios and lock faces 12; upgrade icons, the emblem and the Key 8; kind icons 6 (round 1 allowed 48, 32, 20, 12 and 8) |
| Quiet | at most the category's lone specks, edges and brightness step (below); the validator fails anything busier |
| Outline | sprites on transparency have a 1-pixel outer outline in **night2 `#1a1420`** (or night1 `#0b0810` for the deepest shadow side). Every opaque pixel that touches transparency (4-neighbour) is one of those two. Interior lines use the darkest ramp colour of the material they're on, never pure black |
| Anti-aliasing | none |
| Dithering | none, anywhere (round 1 allowed it in skies; every checker pixel is a speck) |
| Gradients | none: shade with two or three flat steps per material, in broad bands |
| Texture | none (see the pillars) |
| Text | none (no letters, numbers, runes that read as letters, signatures or watermarks), except the logo |
| Margins | sprites keep a 1-pixel transparent margin on every side of their canvas, unless the asset says otherwise |

### How quiet is measured

`art/tools/artlib.py`'s `calm_metrics` reads the inside of a finished asset at its native size: *lone specks* (pixels
unlike all four neighbours), *edges* (neighbouring pairs that differ) and *brightness step* (the mean brightness
difference between neighbours, so many strong edges score high and a few soft ones low). The manifest gives each
category its limits (`calm`), and `validate.py` fails an asset over any of them with "too busy: ... (draw it simpler)":

| Category | Lone specks | Edges | Brightness step |
|---|---|---|---|
| Backgrounds, scenes (320x180) | 0.015 | 0.18 | 7.5 |
| Portraits | 0.06 | 0.32 | 19 |
| Things, Heartsongs, curios | 0.15 | 0.58 | |
| Upgrade icons, the emblem, the Key | 0.15 | 0.60 | |
| Kind icons | 0.15 | 0.62 | |
| Lock faces | 0.04 | 0.22 | 10 |

The limits sit between the old code-drawn art, which passes, and round 1's, which doesn't: every round-1 asset fails
them. Round 1's calmest portraits (Brother Ansel, the Hooded Woman) miss only on the specks of their backdrops, so they
are about as busy as a portrait may be. Passing the numbers is necessary, not sufficient: the review still asks whether
anything draws the eye that shouldn't.

`pixelize.py`'s quiet stage (`artlib.quiet`) merges a lone speck or a region of a few pixels into its neighbour when
the two colours are close. That removes texture left over from generation and keeps eyes, glints and accents. It can't
rescue a busy drawing, and nothing else should try to: **regenerate it simpler; never blur, posterize, smooth or
flatten a picture to pass**.

### Camera and perspective

- **Things (items, Heartsongs, curios) and icons**: a three-quarter view from slightly above (the camera about 25
  degrees above the table), the object upright and facing the viewer, as if resting on the shop's counter. Long
  objects (swords, staves, axes, scrolls, pipes) lie on a diagonal from lower left to upper right, handle or base at
  the lower left. Things rest on an invisible floor: their lowest pixels sit on the bottom of the content box.
- **Portraits**: head-and-shoulders busts in three-quarter view, **turned toward screen right** (where the chest is),
  eyes looking right or at the viewer. Exceptions, drawn frontal: the Hooded Woman, the Stranger and the Gilded Maw.
- **Backgrounds and scenes**: a flat stage seen straight on (front elevation): the back wall or view behind the
  counter, and the counter's front face below it. No vanishing-point perspective.

### Light and shading

- Key light: warm and gentle, from the upper left. Light faces are the lighter end of a ramp, shade faces the darker
  end; the shadow side is the right and the underside.
- Ambient: cool and violet (the shop's evening). Shadows go one ramp step darker and, where the palette allows, toward
  violet: a shadowed gold goes to `gold1`/`gold2` (brown-orange), not to grey.
- Highlights: one or two pixels of the lightest ramp colour (or `night10` white on metal, glass and gems), on the upper
  left of the object's main curve. Never a highlight on every edge, never glints scattered over a surface.
- Rim light: on dark subjects against dark backdrops (demons, the Maw, cloaks of shadow), a 1-pixel cool rim on the
  right edge (`steel3`, `arcane4` or `azure5`) to separate them from the background.
- Materials, two or three tones each: metal may add one bright highlight; cloth has a fold or two; glass shows the
  colour behind it and one white dot; a gem is a dark facet, a light facet and one glint; wood is flat with plank
  lines only.

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
| Heartsongs | handmade, worn by love, never perfect | warm, with one mythic-red accent each (a ribbon, an ember, a jewel) |

### The palette

`art/palette/keywright.hex` (also `.gpl` for GIMP/Aseprite and `.png` as a swatch to show the model) holds 74
colours in 14 ramps, dark to light. `art/palette/ramps.md` lists them with what each is for. The ramps are built with
the hue shift already in them; shade by walking a ramp, and move to a neighbouring ramp for the deepest shadows (for
example skin into `umber`, gold into `umber`, violet into `night`). Use few of them at once: a picture that touches
every ramp is never quiet.

The game's own colours are in it exactly: the UI gold `#f2c14e` (gold5), magic violet `#c792ff` (arcane5), the outline
`#1a1420` (night2), and the six rarity colours below.

### Rarity colours

The UI frames everything in its rarity's colour, so an asset never needs a coloured border. The rarity's hue should
appear in the thing itself as an accent, not as a tint over the whole sprite.

| Rarity | Colour | In the art |
|---|---|---|
| Common | `#c9c4d4` (night8) | plain, honest materials: silver, iron, wool, glass, quartz |
| Uncommon | `#62d46f` (leaf5) | a craftsman's work: elven, dwarven; a green accent where it fits |
| Rare | `#4ea3ff` (azure4) | fine work with a touch of magic: sapphire, moonsilver, blue fire |
| Epic | `#b46bff` (arcane4) | powerful and arcane: violet light, lich-craft, archmages' things |
| Legendary | `#ffb52e` (flame3) | a marvel: dragon gold, phoenix fire, the archmages' own codex |
| Mythic | `#ff4058` (crimson5) | the nine Heartsongs only |

Quality climbs by **material and one accent**, not by more ornament: a common thing is plain iron or wool; a legendary
one is gold with one gem or one flame. Fakes (demon gold, paste gems, a scroll case full of sand) look convincing at a
glance and wrong on a second look: demon gold has a dull, iron-grey core; paste is too clear and too even; the sand
case's seal is cheap.

### Places

Every portrait has a backdrop in the colour of its place, and every background is one of the places. Keep these moods,
muted:

| Place | Time and light | Key colours |
|---|---|---|
| Brindle Crossroads (Bram's stall, tier 1) | dusk: a soft violet-to-peach sky, a gold lamp, a striped awning | arcane2-4 sky, gold4-5 lamp, leaf1-3 hills, umber wood |
| Thornwick Market (tier 2) | afternoon in a stone shop at the Old Wood's edge | night2-6 stone, umber shelves, dark pines through the window, gold lamp |
| Crownhold (tier 3) | sunset over the capital's roofs | umber panelling, azure1-3 slate, flame2-3 sunset, brass |
| The Royal Emporium (tier 4) | gilded, with something wrong behind it | crimson velvet, gold, night5-9 marble, a faint sickly gold glow from the Gate |
| The story (Guild, Keyhold, the Gate) | night and lamplight | arcane1-3, gold, night1-3 |
| Bram's kin (the player characters) | Bram's back room, firelight | umber, flame, gold |

## Categories

Each category below is a template the manifest's assets fill in. Sizes are native (art pixels); "on screen" is after
the game's scaling. The manifest's `redo` says which categories round 2 redraws; the rest (UI frames, effects, the
logo and app icons, store art) stay as round 1 made them.

### Shop backgrounds (`bg.shop1` to `bg.shop4`)

- **320x180**, opaque, four screen pixels to an art pixel: the importer doubles it to 640x360 and the game shows that
  at 2x behind the counter (and dimmed to 30-60% behind every other screen).
- **Fixed lines** (the game places things on them), at 320x180: the counter's top edge is row **119**, and everything
  below it is the counter's front face. The display shelf's top surface is row **49**, spanning at least x 75-201, in
  every tier (at the stall it's a plank hung from the awning). Kept things stand on it, and Heartsongs hang above it.
- **The value plan** (`art/reference/quiet/value_plan_counter.png`): a mid-dark top (an awning, a beam); a back wall or
  view at mid value; one soft, lighter area (the sky or a window) between the portrait and the slip, above the shelf;
  darker sides; just behind the chest (x 110-170, rows 75-118) one or two flat mid-dark colours; a lit top edge to the
  counter; and a dark, plain counter front, since text, the commission board and the queue sit on it. Each zone is
  nearly one flat colour.
- **What covers it** at the counter: the top bar (rows 0-13), the seller's portrait, the speech bubble, the slip (the
  right third), the text under the chest, and the board, curios and queue on the counter front. The value plan marks
  them; `art/reference/layout/counter_overlay.png` has the same zones at 640x360 (halve its numbers).
- Three to five large planes, three or four things to see (named in each description), one focal light. No people, no
  text, nothing standing on the counter top, no foliage clusters, no small clouds, no bright sunsets. Up to 24 colours.

### Story scenes (`scene.*`)

- 320x180 like the backgrounds, shown under the dialogue at about 40% brightness, with portraits over the left and
  right thirds and a dialogue box along the bottom. The centre and the bottom third stay especially plain.

### Portraits (`portrait.*`)

- 76x76, opaque: the person over a backdrop of their place. Shown at 2x at the counter (in a 176 px frame), 3x in
  story scenes, 1x in the ledger and Bram's tips, and cropped to the face (the 38x38 box at x 19-56, y 14-51) for the
  waiting queue and ledger rows.
- **Composition template** (every portrait, so they line up and crop the same way): head centred near x 38; the eye
  line at row 34 (plus or minus 2); the top of the head (not counting hats, horns, hoods) around row 14-18; the chin
  around row 50; shoulders from about row 56, wider than the head, cut by the bottom edge. The face must sit inside the
  crop box x 19-56, y 14-51.
- **Simple**: the face in three flat skin tones with clear eyes (at least 2 pixels high) and mouth; hair as two or
  three flat masses, no strands; clothes in flat colours with a fold or two; **two signature details**, drawn large
  (the manifest lists them first). A beard is one or two flat shapes, not a texture.
- Backdrop: one flat colour of their place, a little darker toward the corners in two steps. Nothing else in it: no
  glow rings, no mottling, no scenery.
- The figure has a 1-pixel `night2` contour where it meets the backdrop.
- Each person has a one-line character (in the manifest) and should be recognisable from that line alone. Up to 20
  colours.

### Things in chests (`item.*`) and Heartsongs (`heartsong.*`)

- 32x32 canvas, transparent; the content (with its outline) fits in 30x30 with a 1-pixel margin, horizontally centred,
  its lowest pixels on row 30 (things stand on the table and on the shelf). The game trims to the content and shows
  them at 2x on the table and the shelf, 1x or 2x in slots.
- Relative size within the 30x30 box (the game shows every thing at the same scale, so sizes compare): a coin purse or
  gem 16-22 px, a potion 18-26 px tall, a book or scroll 22-26 px, a weapon on the diagonal 28-30 px, a cloak 26-30 px.
- One bold shape, each material in two or three flat tones, **two details at most**, no engraving or pattern. Up to 12
  colours.
- Each of the six kinds has five qualities, common to legendary: a family that climbs by material and one accent.
- **Heartsongs** are the nine rarest things in the realm, each a beloved object given to the Locking (see the
  manifest). Same rules; warmer, more personal, visibly old and cared-for; each has a single mythic-red accent.
- Enchanted, broken, fake and surprise variants are effects the game adds; draw only the plain thing (fakes are their
  own sprites).

### Curios (`curio.*`)

- 28x28 canvas, transparent, content in 26x26 with a 1-pixel margin, centred both ways (curios sit in slots, not on
  the table). Shown at 2x in 56 px slots: Nim's cards, the ledger and the counter.
- Curios are oddments from the Locking that bend a rule of the trade, each made for a trade (a gambler's token, a
  smuggler's hook, an appraiser's seal). Draw the object as one iconic thing that hints at what it does (the Waiting
  Game is an hourglass, the Ear Trumpet is an ear trumpet).
- Rarity shows in the material, simply: common, a plain object; uncommon, a good material; rare, one fine material and
  a single small gem or trim; legendary, gold with one gem or rune, still a simple, bold shape. Up to 12 colours.

### The chest (drawn in code)

- Not generated. `tools/custom_art.py` draws it (`chest`, `chest_lock`): 48 art pixels wide (lid 21 rows, body 15,
  open 30), seven finishes on one silhouette, three lock makers' hasps and plates laid over it. The importer builds it
  into `assets/sprites/chest/` at 2x, and `ChestView` shows that at 2x (192 px).
- `art/reference/quiet/chest_drawn_4x.png` is the bar for how simple a sprite should be.

### Icons (`icon.upgrade.*`, `icon.kind.*`)

- Upgrade icons: 24x24 transparent, content 22x22 centred. Shown at 2x (48 px) on the day's-end upgrade cards and 1x
  on the slip. One per upgrade (the manifest has 27), each a simple object a shopkeeper would buy (a painted sign,
  brass scales, an oak cabinet). Up to 8 colours, bold shapes, readable at 1x.
- Kind icons: 16x16 transparent, content 14x14 centred, shown at 2x in the slip's rows and the ledger: one symbol per
  kind of thing (coin, weapons, cloaks, potions, spells, gems), plus the Heartsong and the false bottom. Up to 6
  colours. They must read at a glance next to a word, like map symbols.

### Emblems and story props (`emblem.*`, `story.*`)

- The Gilded Guild's emblem (24x24): a gold coin stamped with a watching eye above a keyhole. Shown at 2x and 1x.
- The Aldric Key (`story.key_whole`, 96x24): a royal gold key whose bow is a crown. Its four shards are cut from it
  along jagged lines and kept in place on the same 96x24 canvas, so they stack back into the whole key. Up to 8
  colours.

### Lockpicking (`lock.face.*`)

- 160x160 transparent: the face of a lock as seen when picking it, in plain flat metal, with a keyhole in the middle
  and a plain dark ring track (radius 52-68 from the centre) where the game draws the sweet spot and the pick. Shown at
  2x. Up to 12 colours.

### Kept from round 1: UI skin, effects, logo and app icons, store art

- `ui.*` (nine-slice frames, drawn by script), `fx.*` (sprite sheets the game tints and draws additively), `brand.*`
  (the logo and app icons) and `store.*` (Steam key art) aren't redrawn in round 2. Their specs in the manifest are
  round 1's.

## Generating with GPT-Image-2

1. **Anchors first.** Round 2's batch 9 is one asset per category (`bg.shop1`, `portrait.bram`,
   `item.dwarf_gold_crowns`, `heartsong.lullaby_box`, `curio.monocle`, `icon.upgrade.lamp`, `icon.kind.coins`).
   Make them, push them with their review pictures, and **stop until the owner approves them**: they decide what every
   later batch looks like. Every later request in a category passes its round-2 anchor (and the palette swatch) as
   reference images. Never pass round 1's art: its style is what round 2 moves away from.
2. **Prompt** = the quiet style preamble (in the manifest) + the category's frame + the asset's own description and
   its details (cut to the budget) + the things to avoid. The manifest's `prompt` field is already assembled; adjust
   it only to fix a failed attempt, and record what changed. The usual correction is "simpler: fewer, larger shapes,
   flat colour, no texture".
3. **Ask for the grid.** Each prompt gives the art's native size and asks for each art pixel to be drawn as a clean
   square block (32x32 art at 32 px a block in a 1024x1024 image; a 320x180 background at about 4.8 px a block across
   a 1536x1024 image). Ask for a transparent background (or a flat `#FF00FF` one when transparency isn't available)
   and nothing else in the frame.
4. **Post-process** with `art/tools/pixelize.py`: it finds the art-pixel grid, samples each block's main colour, snaps
   to the palette, makes alpha binary, places the content on the canvas, quiets stray specks, caps the colours and
   enforces the outline. Backgrounds and scenes are read on their grid too (round 1 shrank them by area, and their
   brushwork became noise).
5. **Validate** with `art/tools/validate.py`: size, alpha, palette, colour count, outline, margins, baseline, and how
   quiet it is. It writes `art/generated/report.json` and review pictures under `art/generated/_review/`. Nothing
   ships with a failure.
6. **Review** at the size it's seen: the counter mock-ups and layout pictures for backgrounds, the contact sheets beside
   the anchor for the rest. Look at a background with the counter screen over it for a minute: if anything keeps
   drawing the eye, simplify it.

### Common failures and what to do

| Failure | Fix |
|---|---|
| "Too busy" from the validator | regenerate with "simpler: fewer, larger shapes, flat colour, no texture"; if it keeps failing, cut a detail from the description. Never filter it to pass |
| Texture on surfaces (grain, speckle, mottled skies, strands of hair) | regenerate restating "no texture"; the quiet stage only removes stray specks, not a pattern |
| A background with too much contrast, a bright sunset or many small things (plants, clouds, bottles) | regenerate asking for muted colour, low contrast and fewer, bigger things; check it against the value plan |
| The counter or the shelf off its row | move the crop with `--offset-y` (source pixels); regenerate if both can't line up |
| Blurry or mushy pixels | the source must already be pixel art on the grid; regenerate asking for clean square blocks |
| A frame, background, floor shadow or glow around a sprite | regenerate: transparent background, "no shadow, no glow, no frame" |
| Wrong perspective or light direction | pass the anchor as a reference; restate "lit from the upper left" |
| Text or rune-letters appear | regenerate with "no text, no letters" |
| A portrait off the template (face outside the crop box) | regenerate with the template; never shrink a finished portrait |
| Style drift across a batch | compare each new asset to its round-2 anchor side by side before accepting it |

## Review checklist

- [ ] `validate.py` passes: size, canvas, transparency, palette, colour count, outline, and quiet
- [ ] At the size it's seen, nothing in it draws the eye except what should (the chest, the person, the thing)
- [ ] A few big, flat shapes; no texture; at most two details on a thing, a curio or a portrait
- [ ] Backgrounds: muted, mostly mid to dark, one soft light, a plain space behind the chest and a plain counter front;
      the counter's top on row 119 and the shelf's on row 49
- [ ] Lit from the upper left; shadows cool, highlights warm; a 1-pixel `night2` outline on sprites
- [ ] Reads as a silhouette at 1x and is the subject the manifest describes
- [ ] Quality shows in the material (common humble, legendary precious); Heartsongs unmistakable
- [ ] No text, frames, glows, shadows or effects baked in
- [ ] Looks like its round-2 anchor
