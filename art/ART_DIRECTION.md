# Keywright: art direction

This is the style bible for every picture in the game. `art/manifest.json` lists each asset with its own spec; this
document is what they all share. Where the two disagree, the manifest's per-asset spec wins for that asset, and the
disagreement is a bug to report.

## The game, for an artist

Keywright is a 2D merchant game set in **Aldmark**, a sword-and-sorcery realm where, three hundred years ago, every
treasure was shut into a warded chest to starve the demon of greed, **Avarax, the Gilded Maw**. The chests never went
away. Folk trade them unopened; only a **Keywright** (you, one of Uncle Bram's kin) can open one, and only after buying
it. The player sits behind a shop counter, listens to each seller's chest (shaking it, dropping it), buys or passes,
and opens what they bought: the things come flying out onto the table. The shop moves inward over a run, from a
roadside stall to the Royal Emporium by the **Gilded Gate**, while the **Gilded Guild** collects a tithe that is
secretly feeding the Maw. `docs/world.md` is the world bible; read it before drawing anyone or anything.

The screen is a stage: a shop background, a seller's portrait at the left, the chest in the middle, the appraisal slip
on the right. Openings are the climax: the lid strains, light leaks from the seam, and each thing flies out and lands
on the table with its price. The art has to make those moments feel like opening a treasure.

## The look in one line

**Lantern-lit storybook pixel art**: warm, handmade and readable, like a well-thumbed illustrated ledger. Treasures
glint, people have faces you remember, and the Guild's gold looks a little too clean.

### Pillars

1. **Silhouette first.** Everything must read at its smallest size (a 32 px thing at 1x, a portrait cropped to the
   face at 38 px). If it doesn't read as a black shape, add contrast, not detail.
2. **Warm light, cool shadow.** One warm key light from the upper left; shadows shift toward violet and blue, highlights
   toward yellow (the palette's ramps already do this: walk a ramp, don't mix your own colours).
3. **Things you'd want to find.** No junk. A common thing is humble but well made (grave silver, a goblin's cleaver); a
   legendary one is a marvel. Quality should be visible in the object itself: materials, ornament, craftsmanship.
4. **One world.** Same pixel density, same palette, same light, same outline everywhere. A curio and a portrait drawn
   by different hands must still look like they belong on the same counter.
5. **The game draws the magic.** Glows, auras, sparkles, light rays, enchantment shimmer, rarity colours around slots,
   shadows under things and screen-shake are all drawn by the game at runtime. Never bake them into an asset (unless the
   asset is itself an effect, in the FX category).

## Technical canon (not negotiable)

| Rule | Value |
|---|---|
| Screen | 1280x720, scaled up to the player's window by whole numbers |
| Pixel density | **one art pixel = two screen pixels** (a 640x360 virtual canvas) for every world asset. Exceptions are listed per asset (portraits zoom to 3x in story scenes and sit at 1x in the ledger; icons may sit at 1x in dense UI) |
| Scaling | integer only, nearest-neighbour; never rotate, skew or resample a finished asset |
| File | PNG, 8 bits per channel. Sprites RGBA with a transparent background; backgrounds and portraits fully opaque RGB(A) |
| Alpha | binary: every pixel is fully opaque (255) or fully transparent (0). No soft edges, no drop shadows, no glow halos |
| Palette | the master palette only (`art/palette/keywright.hex`, 74 colours in 14 ramps; see below). Every opaque pixel must be an exact palette colour |
| Colours per asset | items, curios, Heartsongs and chest parts: at most 20; icons: at most 12; portraits: at most 32; backgrounds and scenes: at most 48; store art: free (see its section) |
| Outline | sprites on transparency have a 1-pixel outer outline in **night2 `#1a1420`** (or night1 `#0b0810` for the deepest shadow side). Every opaque pixel that touches transparency (4-neighbour) is one of those two. Interior lines use the darkest ramp colour of the material they're on, never pure black |
| Anti-aliasing | none on the silhouette. Inside, only hand-placed single pixels from the palette, sparingly |
| Dithering | none in sprites. In backgrounds only, ordered 2x2 checker where two large flat areas meet (sky bands, fog) |
| Gradients | none: shade with 2-5 flat steps per material (stepped bands, not smooth ramps) |
| Text | none (no letters, numbers, runes that read as letters, signatures or watermarks), except the logo |
| Margins | sprites keep a 1-pixel transparent margin on every side of their canvas, unless the asset says otherwise |

### Camera and perspective

- **Things (items, Heartsongs, curios), icons and the chest**: a three-quarter view from slightly above (the camera
  about 25 degrees above the table), the object upright and facing the viewer, as if resting on the shop's counter.
  Long objects (swords, staves, axes, scrolls, pipes) lie on a diagonal from lower left to upper right, handle or base
  at the lower left. Things rest on an invisible floor: their lowest pixels sit on the bottom of the content box.
- **Portraits**: head-and-shoulders busts in three-quarter view, **turned toward screen right** (where the chest is),
  eyes looking right or at the viewer. Exceptions, drawn frontal: the Hooded Woman, the Stranger and the Gilded Maw.
- **Backgrounds**: a flat stage seen straight on (front elevation): the back wall or view behind the counter, and the
  counter's front face below it. No vanishing-point perspective beyond a hint of depth in floorboards.

### Light and shading

- Key light: warm (lamplight), from the upper left, about 45 degrees. Light faces are the lighter end of a ramp, shade
  faces the darker end; the cast shadow side is the right and the underside.
- Ambient: cool and violet (the shop's night). Shadows go one or two ramp steps darker and, where the palette allows,
  toward violet: a shadowed gold goes to `gold1`/`gold2` (brown-orange), not to grey.
- Highlights: one or two pixels of the lightest ramp colour (or `night10` white on metal, glass and gems), on the upper
  left of each curved surface. Never a highlight on every edge.
- Rim light: on dark subjects against dark backdrops (demons, the Maw, cloaks of shadow), a 1-pixel cool rim on the
  right edge (`steel3`, `arcane4` or `azure5`) to separate them from the background.
- Materials, three to five values each: metal is high-contrast with sharp highlights; cloth is low-contrast with soft
  folds; glass shows the background colour through it and a white specular dot; gems have a dark facet, a mid facet, a
  light facet and a white glint; wood shows grain as two-tone streaks, not single-pixel noise.

### Shape language

| Who or what | Shapes | Colours |
|---|---|---|
| Village folk, Bram's kin | round, soft, slightly stocky | homespun browns, greens, faded red, warm skin |
| Thornwick and the Old Wood | organic, leafy, crooked | deep greens, moss, birch white, amber light |
| Crownhold | upright, tidy, tall | slate blue, brick orange, ink, brass |
| The Royal Emporium | ornate, symmetrical | crimson velvet, gold leaf, white marble |
| The Gilded Guild | rigid, symmetrical, polished; the emblem is a watching eye over a keyhole on a gold coin | black, cold gold, a little sickly green in the shadows |
| Demons and the Maw | sharp, horned, top-heavy, glowing eyes | crimson, black, tarnished gold, ember orange |
| Archmages and the Keyhold | tall, starry, flowing | indigo and violet with cyan rune light |
| Heartsongs | handmade, worn by love, never perfect | warm, with one mythic-red accent each (a ribbon, an ember, a jewel, a glow-coloured detail) |

### The palette

`art/palette/keywright.hex` (also `.gpl` for GIMP/Aseprite and `.png` as a swatch to show the model) holds 74
colours in 14 ramps, dark to light. `art/palette/ramps.md` lists them with what each is for. The ramps are built with
the hue shift already in them; shade by walking a ramp, and move to a neighbouring ramp for the deepest shadows (for
example skin into `umber`, gold into `umber`, violet into `night`).

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

Fakes (demon gold, paste gems, a scroll case full of sand) look convincing at a glance and wrong on a second look: demon
gold has a dull, iron-grey core showing at the edges; paste is too clear and too even; the sand case's seal is cheap.

### Places

Every portrait has a backdrop in the colour of its place, and every background is one of the places. Keep these moods:

| Place | Time and light | Key colours |
|---|---|---|
| Brindle Crossroads (Bram's stall, tier 1) | dusk: violet sky, gold lamp, a striped awning | arcane1-3 sky, gold4-5 lamp, leaf2-4 fields, umber wood |
| Thornwick Market (tier 2) | afternoon in a stone shop at the Old Wood's edge | night3-5 stone, umber shelves, leaf through the window, gold lamp |
| Crownhold (tier 3) | sunset over the capital's roofs | azure2-4 slate, flame2-3 sunset, umber panelling, brass |
| The Royal Emporium (tier 4) | bright and gilded, with something wrong behind it | crimson velvet, gold leaf, night9-10 marble, a sickly gold glow from the Gate |
| The story (Guild, Keyhold, the Gate) | night and lamplight | arcane1-3, gold, night1-3 |
| Bram's kin (the player characters) | Bram's back room, firelight | umber, flame, gold |

## Categories

Each category below is a template the manifest's assets fill in. Sizes are native (art pixels); "on screen" is after
the game's scaling.

### Shop backgrounds (`bg.shop1` to `bg.shop4`)

- 640x360, opaque, shown at 2x behind the counter (and dimmed to 30-60% behind every other screen).
- **Fixed lines** (the game places things on them): the counter's top edge is row **238**; everything below it is the
  counter's front face. The display shelf's top surface is row **98**, spanning at least x 150-402, in every tier (at
  the stall it's a plank hung from the awning: one character starts with a shelf there). Kept things stand on it, and
  Heartsongs hang above it on strings from about row 63.
- **What covers it** at the counter (see `art/reference/layout/counter_overlay.png`): the top bar (rows 0-27), the
  seller's portrait (x 31-119, y 54-142) and speech bubble (x 6-144, y 167-240), the appraisal slip (x 410-626, y
  33-353), text under the chest (around x 160-400, y 245-300), the commission board, curios and waiting queue on the
  counter front (x 6-250, y 245-350). The chest stands on the counter at x 230-326 (its bottom on row 238).
- So: keep the interest in the top band and between the portrait and the slip (x 120-410, y 28-150); keep the area
  just behind the chest (x 220-340, y 150-238) calm and mid-dark so the chest stands out; keep the counter front low in
  contrast (text sits on it). No people, no text, no items standing on the counter top.
- Dithering is allowed here (2x2 checker between sky bands). Up to 48 colours.

### Portraits (`portrait.*`)

- 76x76, opaque: the person over a backdrop of their place. Shown at 2x at the counter (in a 176 px frame), 3x in
  story scenes, 1x in the ledger and Bram's tips, and cropped to the face (the 38x38 box at x 19-56, y 14-51) for the
  waiting queue and ledger rows.
- **Composition template** (every portrait, so they line up and crop the same way): head centred near x 38; the eye
  line at row 34 (plus or minus 2); the top of the head (not counting hats, horns, hoods) around row 14-18; the chin
  around row 50; shoulders from about row 56, wider than the head, cut by the bottom edge. The face must sit inside the
  crop box x 19-56, y 14-51.
- Backdrop: a soft vignette in their place's colours (darker toward the corners, in 3-4 flat stepped bands, no
  gradients), plain enough that the head reads. A hint of their place is welcome (a few leaves, a shelf edge, stars),
  never a scene.
- The figure has a 1-pixel `night2` contour where it meets the backdrop; inside, the usual ramp shading.
- Expression and age matter: each person has a one-line character (in the manifest) and should be recognisable from
  that line alone. Eyes are at least 2 pixels high so they read at 1x.
- No frames, no names, no hands entering from outside the frame unless the spec asks.

### Things in chests (`item.*`) and Heartsongs (`heartsong.*`)

- 32x32 canvas, transparent; the content (with its outline) fits in 30x30 with a 1-pixel margin, horizontally centred,
  its lowest pixels on row 30 (things stand on the table and on the shelf). The game trims to the content and shows
  them at 2x on the table, 1x or 2x in slots.
- Relative size within the 30x30 box (the game shows every thing at the same scale, so sizes compare): a coin purse or
  gem 16-22 px, a potion 18-26 px tall, a book or scroll 22-26 px, a weapon on the diagonal 28-30 px, a cloak 26-30 px.
- Each of the six kinds has five qualities, common to legendary, and they should look like a family that climbs: the
  same kind of object getting finer (more ornament, rarer material, magical light in the material itself).
- **Heartsongs** are the nine rarest things in the realm, each a beloved object given to the Locking (see the
  manifest). Same size rules; warmer, more personal, visibly old and cared-for; each has a single mythic-red accent.
  They should look like nothing else in the game at a glance.
- Enchanted, broken, fake and surprise variants are effects the game adds; draw only the plain thing (fakes are their
  own sprites).

### Curios (`curio.*`)

- 28x28 canvas, transparent, content in 26x26 with a 1-pixel margin, centred both ways (curios sit in slots, not on
  the table). Shown at 2x in 56 px slots: Nim's cards, the ledger and the counter.
- Curios are oddments from the Locking that bend a rule of the trade, each made for a trade (a gambler's token, a
  smuggler's hook, an appraiser's seal). Draw the object, clearly, as a single iconic thing; the manifest says what it
  does so the object can hint at it (the Waiting Game is an hourglass, the Ear Trumpet is an ear trumpet).
- Rarity shows in material and finish: a common curio is a plain object; uncommon, well made; rare, precious; a
  legendary is the finest thing of its trade, with gold and a gem or rune.

### The chest (`chest.*`)

- One design, seven finishes, three parts, so the game can open it: **lid** 96x42 (the closed lid, its bottom edge
  the seam), **body** 96x30 (below the seam: the band with the lock seat, the base), **open** 96x60 (the open chest:
  the far rim and dark interior over the same body). Shown at 2x (192 px wide) at the counter and the opening.
- Every finish uses the exact same silhouette and the same geometry (the validator checks every finish's alpha masks
  against the wood chest's): a strongbox with a gently domed lid whose top face shows as a strip, corner brackets, a
  riveted band across the seam, the **lock seat** centred on the seam (the lid's x 40-55, rows 34-41 and the body's x
  38-57, rows 0-13, left plain: the lock overlays go there) and a ward rune on the lid's front, faintly carved (not
  glowing). The lid's bottom row and the body's top row are the seam, where the two parts meet: they're not outlined
  like the rest of the silhouette. Make the wood chest first; make every other finish by editing it (only materials
  and colours change).
- Finishes (the chest's colour promises the quality inside): **wood** (common: plain oak boards, iron fittings),
  **green** (uncommon: green-painted wood, bronze fittings), **blue** (rare: blue lacquer, steel bands), **purple**
  (epic: violet lacquer, silver fittings, a faint star motif), **gold** (legendary and gilded chests: gold-leafed, royal
  fittings), **black** (the Stranger's sealed chest: black iron wrapped in chains, no lock seat showing), and **crown**
  (the Crown Chest, optional: royal crimson and gold with the Aldric crown on the lid).
- **Lock overlays** (`chest.lock.*`): for each lock maker, a lid overlay (the hasp) and a body overlay (the lock
  plate), each on a transparent canvas the size of the part it sits on and only inside its lock seat, so the game
  simply stacks them. Rusty Padlock (iron, rust), Guild Lock (blue steel with the Guild's eye), Dwarven Tumbler
  (bronze, geared).
- No glow in the seam, no shadow under the chest: the game draws both.

### Icons (`icon.upgrade.*`, `icon.kind.*`)

- Upgrade icons: 24x24 transparent, content 22x22 centred. Shown at 2x (48 px) on the day's-end upgrade cards and 1x
  on the slip. One per upgrade (the manifest has 27), each a simple object a shopkeeper would buy (a painted sign,
  brass scales, an oak cabinet). Up to 12 colours, bold shapes, readable at 1x.
- Kind icons: 16x16 transparent, content 14x14 centred, shown at 2x in the slip's rows and the ledger: one symbol per
  kind of thing (coin, weapons, cloaks, potions, spells, gems), plus the Heartsong and the false bottom. Up to 8 colours.
  They must read at a glance next to a word, like map symbols.

### Emblems and story props (`emblem.*`, `story.*`)

- The Gilded Guild's emblem (24x24): a gold coin stamped with a watching eye above a keyhole. Shown at 2x and 1x.
- The Aldric Key: four shards (each on a 32x16 canvas) that fit together into the whole key (`story.key_whole`,
  96x24): a royal key whose bow is a crown, snapped into four jagged pieces. The shards are what the Hooded Woman pays
  in; the game shows how many you hold.

### UI skin (`ui.*`)

- Nine-slice textures, greyscale (the game tints them), drawn at 2x density. The existing frames are plain bevels;
  these add a little craft (a gold-leaf corner, a stitched edge) without busy patterns that stretch badly. Exact
  geometry is in the manifest; these may be drawn by hand or by script rather than generated.

### Effects (`fx.*`)

- Sprite sheets in a single row of equal frames, white and light greys on transparency (the game tints them and draws
  them additively, so brightness is opacity). Binary alpha still applies.

### Lockpicking (`lock.face.*`)

- 160x160 transparent: the face of a lock as seen when picking it, with a keyhole in the middle and a plain dark ring
  track (radius 52-68 from the centre) where the game draws the sweet spot and the pick. Shown at 2x.

### Logo, app icon, store art (`brand.*`, `store.*`)

- The logo is the word KEYWRIGHT in chunky pixel letters with a key worked into it; it must spell the word exactly.
- The app icon is the purple chest with a key, readable at 16x16.
- Store capsules are key art for the Steam page, made in the game's pixel style at a coarser pixel size (3-4 screen
  pixels per art pixel); they're the one place gradients of light (still in palette colours) and more colours are
  allowed. Leave clear space for the logo, composited later.

## Generating with GPT-Image-2

1. **Style anchors first.** Generate the batch-0 assets (one of each category: `bg.shop1`, `portrait.bram`,
   `item.dwarf_gold_crowns`, `heartsong.lullaby_box`, `curio.monocle`, the wood chest, `icon.upgrade.lamp`,
   `icon.kind.coins`), post-process them, and get them approved before anything else. Every later request in a category
   passes its anchor (and the palette swatch) as reference images, so 250 assets stay one style.
2. **Prompt** = the global style preamble (in the manifest) + the category template + the asset's own description +
   the things to avoid. The manifest's `prompt` field is already assembled; adjust it only to fix a failed attempt, and
   record what changed.
3. **Ask for the grid.** Each prompt says the art's native size and asks for each art pixel to be drawn as a clean
   square block (for example 32x32 art drawn at 32x, a 1024x1024 image). Ask for a transparent background (or a flat
   `#FF00FF` one when transparency isn't available) and nothing else in the frame.
4. **Post-process** with `art/tools/pixelize.py` (or something that does the same): find the art-pixel grid, sample
   each block's dominant colour, snap every colour to the palette, make alpha binary, trim and place the content on the
   asset's canvas, enforce the outline, and cap the colour count. Keep the raw generation out of git (or as a small
   `.webp` under `art/generated/_raw/` if it helps review).
5. **Validate** with `art/tools/validate.py`: size, alpha, palette, colour count, outline, margins and baseline,
   matching chest silhouettes. It writes `art/generated/report.json` and contact sheets under
   `art/generated/_review/`. Nothing ships with a failure.
6. **Review** each contact sheet against this document: does it read at 1x, is it lit from the upper left, does it look
   like it belongs next to its anchor, does it say what the manifest says?

### Common failures and what to do

| Failure | Fix |
|---|---|
| Blurry or mushy pixels after downscaling | regenerate asking for fewer, larger shapes; the source must already be pixel art at the target grid |
| Too many colours, noisy texture | simplify the description; ask for flat colour clusters; the pixelizer's colour cap should not have to remove more than a few |
| A frame, background, floor shadow or glow around a sprite | regenerate; transparent background, "no shadow, no glow, no frame" |
| Wrong perspective or light direction | pass the category anchor as a reference image; restate "lit from the upper left" |
| Text or rune-letters appear | regenerate with "no text, no letters" |
| A portrait off the template (face outside the crop box) | regenerate with the template; never shrink a finished portrait |
| Style drift across a batch | compare each new asset to the anchor side by side before accepting it |

## Review checklist

- [ ] Native size and canvas exactly as the manifest says; transparent where it should be, opaque where it should be
- [ ] Palette colours only; colour count within the limit
- [ ] 1-pixel `night2` outline on sprites; no soft edges anywhere
- [ ] Lit from the upper left; shadows cool, highlights warm
- [ ] Reads as a silhouette at 1x; the subject is the one the manifest describes, with its listed features
- [ ] Quality visible in the object (common humble, legendary marvellous); Heartsongs unmistakable
- [ ] No text, frames, glows, shadows or effects baked in
- [ ] Looks like its category's anchor
