"""Icons, emblems, UI, effects, lock faces, branding, story scenes and store art (the shop scene is assets_scene.py's;
the chest is drawn in code)."""

ASSETS = []


def add(**asset):
    asset.setdefault("details", [])
    asset.setdefault("avoid", [])
    ASSETS.append(asset)
    return asset


# ---------------------------------------------------------------------------------------------------------------------
# The shop scene (skies, places, the shop's frame, shelves, counters, appraisal sheets, lanterns, props) is in
# assets_scene.py since round 2: one picture per layer instead of one per shop.

# The chest and its locks are drawn in code (tools/custom_art.py: chest, chest_lock) since round 2: one silhouette for
# every finish, exactly, and as calm as the thing the player looks at most must be.

# ---------------------------------------------------------------------------------------------------------------------
# Upgrade icons (scripts/Core/Upgrades.cs): one per upgrade. Today several share an icon; the new ones are marked.

UPGRADES = [
    ("shop2", "Move to Thornwick", "shop", "a small stone-and-timber shopfront with a green awning and a pine tree beside it"),
    ("shop3", "Move to Crownhold", "shop", "a tall town shopfront with a slate roof, a blue banner and a lit window"),
    ("shop4", "The Royal Emporium", "shop", "a grand white-marble shopfront with a crimson awning and a small gold crown "
                                           "over the door"),
    ("sign", "Painted Sign", "sign", "a hanging painted shop sign showing a little chest, on an iron bracket"),
    ("scales", "Brass Scales", "scales", "a brass balance scale with two pans"),
    ("cabinet1", "Oak Cabinet", "cabinet", "a small oak display cabinet with two glass doors"),
    ("cabinet2", "Oak Dresser", "cabinet", "an oak dresser with open shelves above and drawers below"),
    ("cabinet3", "Glass Cabinet", "cabinet", "a tall glass-fronted cabinet in a brass frame"),
    ("cabinet4", "Glass Case", "cabinet", "a glass display case on four legs (a vitrine)"),
    ("cabinet5", "Gilded Cabinet", "cabinet", "a cabinet covered in gilded carving"),
    ("cabinet6", "Gilded Case", "cabinet", "a gilded vitrine with a crimson velvet cushion inside"),
    ("picks", "Fine Lockpicks", "picks", "a fan of slender steel lockpicks spilling from a leather roll"),
    ("bench", "Restorer's Bench", "bench", "a wooden workbench with a small hammer and a polishing cloth on it"),
    ("workshop", "Restorer's Workshop", "bench", "an anvil and a vice with tools hanging on a rack above"),
    ("master", "Master Restorer", "bench", "a master restorer's gold-rimmed magnifying headband and a fine gold brush on a "
                                          "velvet cloth"),
    ("lamp", "Witchlight", "lamp", "a small lantern burning with a violet witch-flame"),
    ("crier", "Town Crier", "crier", "a town crier's brass hand bell with a red ribbon on the handle"),
    ("block", "Auction Block", "block", "an auctioneer's wooden gavel on a round sounding block"),
    ("charter", "Crownhold Charter", "charter", "a parchment charter with a red wax seal and ribbon"),
    ("case", "Curio Case", "case", "an open wooden case with velvet-lined compartments holding small curios"),
    ("velvet", "Velvet Case", "case", "an open jewellery case lined with plush violet velvet"),
    ("magnet", "Lodestone", "magnet", "a horseshoe of dark lodestone (a natural magnet stone) with brass-capped tips and "
                                     "a few iron filings clinging to them"),
    ("seal", "Royal Seal", "seal", "a gold royal seal stamp with a crown-shaped handle"),
    ("night", "Night Market", "night", "a striped market tent under a crescent moon, a lantern hanging at its door"),
    ("warrant", "Royal Warrant", "warrant", "a royal warrant: a parchment with a gold crown seal and a red ribbon"),
    ("patrons", "Patrons' Book", "patrons", "a thick green book with a gold ribbon and a quill"),
    ("engine", "Bookkeeping Engine", "engine", "a brass clockwork machine of gears with a paper tape curling out of it "
                                              "and a small gold watching eye on top"),
]
for key, title, current, subject in UPGRADES:
    add(id=f"icon.upgrade.{key}", category="icon_upgrade", title=title, subject=subject, upgrade=key,
        batch=0 if key == "lamp" else 5, anchor_for="icon_upgrade" if key == "lamp" else None,
        replaces=f"assets/ui/icons/{current}.png",
        notes=None if key == current else f"New: today this upgrade shares the '{current}' icon. Integration: set "
                                           f"Upgrade.Icon to '{key}'.",
        context=f"The {title} upgrade, bought at the end of a day." + (
            " Also shown on the slip as the tool's button." if key in ("lamp", "magnet") else ""),
        reference_current="art/reference/current/upgrade_icons_2x.png", palette=["night:2-10", "umber", "gold", "steel"])

KINDS = [
    ("coins", "Coin", "a small stack of three gold coins", ["gold:2-6"]),
    ("arms", "Weapons", "a sword pointing up and to the right", ["steel:1-4", "umber:3"]),
    ("garb", "Cloaks", "a hooded cloak hanging", ["crimson:2-4"]),
    ("potions", "Potions", "a round potion flask", ["azure:3-6", "steel:4"]),
    ("scrolls", "Spells", "a rolled scroll with a seal", ["parchment:1-3", "crimson:4"]),
    ("gems", "Gems", "a faceted gem", ["azure:2-6", "night:10"]),
    ("heartsong", "Heartsong", "a red heart with a small curl of song rising from it", ["crimson:2-6"]),
    ("hidden", "False bottom", "a box seen from the side with a hidden drawer in its floor", ["umber:1-5", "gold:5"]),
]
for key, title, subject, palette in KINDS:
    add(id=f"icon.kind.{key}", category="icon_kind", title=title, subject=subject, batch=0 if key == "coins" else 5,
        anchor_for="icon_kind" if key == "coins" else None, replaces=f"assets/ui/kinds/{key}.png", palette=palette,
        context="Starts each row of the appraisal slip and the notebook's sounds table.",
        reference_current="art/reference/current/kind_icons_4x.png")

# ---------------------------------------------------------------------------------------------------------------------
# Emblems and story props.

# assets/ui/icons/crown.png and the red chest (assets/sprites/chest/red_*.png) aren't used by the game: no replacement.

add(id="emblem.guild", category="emblem", title="The Gilded Guild's emblem", size=[24, 24], replaces="assets/ui/guild.png",
    subject="the Gilded Guild's emblem: a gold coin stamped with a watching eye above a keyhole",
    description="A gold coin with a raised rim, stamped with a single watching eye above a keyhole: polished, cold, a "
                "little too perfect.",
    details=["a round gold coin with a rim", "a watching eye", "a keyhole below it"], palette=["gold:1-6", "night:1-2"],
    display="2x on the Guild's night and the day's end, 1x in the top bar's due chip",
    context="The Guild licenses Keywrights and takes a tithe 'to keep the Gate'. It's the Maw's.")

add(id="story.key_whole", category="story", title="The Aldric Key, whole", size=[96, 24], priority="P1",
    subject="the Aldric Key: King Aldric's royal gold key, lying flat, its bow a small crown with a ring in it at the "
            "left, a long decorated shaft with a collar and a ward-ring, and intricate teeth (wards) at the right end",
    description="The whole key, filling the 96x24 canvas left to right (bow at x 0-27, shaft, teeth at x 72-95). Made "
                "first: the four shards are cut from it.",
    details=["a crown-shaped bow with a ring", "a decorated shaft", "intricate teeth at the right end",
             "faint mended crack lines near x 26, 50 and 74 (where the shards meet)"],
    palette=["gold:1-6", "night:10"], replaces=None,
    context="Shown when the Key is whole (the vault's scene); its four shards are what the Hooded Woman pays in.")

KEY_SHARDS = [
    (1, "the bow, snapped off jaggedly just past the collar", [0, 0, 27, 23]),
    (2, "the upper shaft, jagged breaks at both ends", [24, 4, 51, 19]),
    (3, "the lower shaft with the ward-ring, jagged breaks at both ends", [48, 4, 75, 19]),
    (4, "the end of the shaft and the teeth, a jagged break at the left", [72, 0, 95, 23]),
]
for n, what, box in KEY_SHARDS:
    add(id=f"story.key_shard_{n}", category="story", title=f"The Aldric Key, shard {n}", size=[96, 24], priority="P1",
        replaces="assets/ui/key.png" if n == 1 else None, mode="derive",
        subject=f"shard {n} of the Aldric Key: {what}",
        description=f"Shard {n} of 4: {what}. Not generated: cut from story.key_whole along jagged lines near x 26, 50 "
                    f"and 74, each piece left in place on the 96x24 canvas (roughly x {box[0]}-{box[2]}), the rest "
                    f"transparent, re-outlined along its break.",
        details=["only this shard's pixels, in place", "a jagged break that fits its neighbours",
                 "the four never overlap and together are exactly story.key_whole"],
        palette=["gold:1-6", "night:10"], box=box,
        generation="Cut from story.key_whole (by script or by hand), then outline the break edges.",
        notes="Today one generic shard (assets/ui/key.png) is drawn four times. Integration: show shard n for the nth "
              "piece held (trimmed to its pixels).",
        context="The Hooded Woman pays in shards; with all four, the Key opens the Crown Chest.")

# ---------------------------------------------------------------------------------------------------------------------
# UI skin: nine-slices at 2x. Exact recipes; draw by script or by hand.

UI = [
    ("panel", [12, 12], "P2", "assets/ui/panel.png",
     "The dark panel behind every UI box, not tinted. 12x12: a 1px night1 (#0b0810) border with the four corner pixels "
     "transparent; inside it a 1px bevel (top and left rows night4 #3b3150, bottom and right night2 #1a1420); the fill "
     "night3 (#2b2236); a single gold4 (#c29026) pixel just inside each corner of the bevel. Nine-slice margins: 4px.",
     "Today: 8x8 at 3x (UiKit.MakePanel)."),
    ("frame", [12, 12], "P2", "assets/ui/frame.png",
     "A border the game tints (rarity colours, gold, red): a 1px pure white #ffffff line one pixel in from the edge, its "
     "four corner pixels cut, everything else transparent. Nine-slice margins: 4px. Palette exception: tinted textures "
     "use neutral greys (#ffffff, #ebebeb, #bebebe, #828282, #020202) so the tint stays true.",
     "Today: 8x8 at 3x (UiKit.MakeFrame, modulated)."),
    ("button", [12, 12], "P2", "assets/ui/button.png",
     "A button face the game tints by its accent colour: a 1px #020202 border with the four corner pixels transparent; "
     "inside it a 1px bevel (top and left #ebebeb, bottom and right #828282); fill #bebebe; one #ffffff pixel at the "
     "top-left inner corner. Nine-slice margins: 4px. Palette exception: neutral greys, as for ui.frame.",
     "Today: 8x8 at 3x (UiKit.MakeButton, modulated: hover lightens it)."),
    ("button_pressed", [12, 12], "P2", "assets/ui/button_pressed.png",
     "The pressed button: as ui.button with the bevel swapped (top and left #828282, bottom and right #ebebeb) and the "
     "fill #828282. Nine-slice margins: 4px. Palette exception: neutral greys.",
     "Today: 8x8 at 3x."),
    ("slip_paper", [24, 24], "P3", None,
     "New: a parchment nine-slice for the appraisal slip: parchment3 fill with a few parchment2 fibres near the edges, a "
     "1px umber2 border with slightly ragged corners, a 2px darker parchment1 band inside the bottom edge. Nine-slice "
     "margins: 8px. Not tinted.",
     "New look for the slip (integration: CounterPhase's panel and its text colours would change to ink on paper)."),
    ("bubble", [24, 24], "P3", None,
     "New: a speech-bubble nine-slice: night10 fill, a 1px night2 border with rounded corners (two-pixel radius), no tail "
     "(the tail is ui.bubble_tail). Nine-slice margins: 8px. Not tinted.",
     "New look for sellers' speech (integration: SellerView)."),
    ("bubble_tail", [12, 8], "P3", None,
     "New: the speech bubble's tail, pointing up from the bubble's top edge: night10 with a 1px night2 outline along "
     "its two slanted sides and its tip; its bottom row night10 with no outline, so it covers the bubble's border where "
     "they join.", "With ui.bubble. (The first delivery followed an older wording that left the tip open instead.)"),
]
for key, size, priority, replaces, recipe, notes in UI:
    add(id=f"ui.{key}", category="ui", title=key.replace("_", " ").capitalize(), size=size, priority=priority,
        batch=6 if priority == "P2" else 7, replaces=replaces, description=recipe, notes=notes,
        palette={"panel": ["night:1-4", "gold:4"], "slip_paper": ["parchment", "umber:2"],
                 "bubble": ["night:2", "night:10"], "bubble_tail": ["night:2", "night:10"]}.get(key, []),
        palette_exception=["#ffffff", "#ebebeb", "#bebebe", "#828282", "#020202"] if key in ("frame", "button",
                                                                                              "button_pressed") else None)

# ---------------------------------------------------------------------------------------------------------------------
# Effects: white and greys, drawn additively and tinted by the game (the coin is the exception: real gold).

FX = [
    ("explosion", 8, 128, 128, "P1", "assets/sprites/fx/burst.png",
     "A burst for a chest whose lid bursts open: frame 1 a small bright star-shaped core; frames 2-4 an expanding ring "
     "of light with blocky debris flying out; frames 5-8 the ring breaking into fading puffs and a few last sparks, "
     "frame 8 almost empty.",
     "Today 6 frames of 112x100 (Fx.Explosion, shown at 4-6x). Integration: 8 frames, shown at 2-3x."),
    ("coin_spin", 6, 12, 12, "P2", None,
     "A gold coin spinning: face-on, three-quarter, edge-on, three-quarter (back), face-on (back), three-quarter. Gold "
     "ramp colours, not white (the game doesn't tint this one).",
     "New: the coins that fly into the purse (today the Dwarf-Gold Crowns item sprite)."),
    ("sparkle", 4, 9, 9, "P2", None,
     "A twinkle: a single pixel, then a small plus, then a four-pointed star with a bright centre, then a small plus "
     "again.", "New: sparkles on rare finds and glints (today 2px squares)."),
    ("dust", 5, 24, 16, "P2", None,
     "A puff of dust when a chest lands: a low wide cloud of blocky puffs expanding sideways and fading.",
     "New: the drop's landing (today plain particles)."),
]
for key, frames, fw, fh, priority, replaces, subject, notes in FX:
    add(id=f"fx.{key}", category="fx", title=key.replace("_", " ").capitalize(), size=[frames * fw, fh],
        priority=priority, batch=6, replaces=replaces, frames=frames, frame_size=[fw, fh], subject=subject,
        notes=notes,
        palette=["gold:2-6"] if key == "coin_spin" else ["night:6-10"],
        frame_fields={"frames": frames, "fw": fw, "fh": fh,
                      "colour_rule": "Gold coin colours, drawn as is (the game doesn't tint this one)." if key == "coin_spin"
                      else "White and light greys only (the game tints it and draws it additively)."})

# ---------------------------------------------------------------------------------------------------------------------
# Lockpicking: the lock's face, one per maker. The game draws the ring track's sweet spot, the pins and the pick.

FACES = [
    ("rusty", "a rusty iron padlock", "pitted grey iron with patches of rust, a few dents, a plain keyhole",
     ["steel:1-3", "umber:2-4", "night:1-4"]),
    ("guild", "a Gilded Guild lock", "blue-steel with gold trim and rivets, the Guild's watching eye engraved above the "
                                    "keyhole", ["azure:1-4", "steel:1-4", "gold:3-5", "night:1-3"]),
    ("dwarven", "a dwarven tumbler lock", "bronze with a geared outer edge, rivets and knotwork, a deep keyhole",
     ["gold:1-4", "flame:1-2", "umber:1-3", "night:1-3"]),
]
for key, subject, look, palette in FACES:
    add(id=f"lock.face.{key}", category="lock_face", title=f"Lock face: {subject}", subject=subject, lock=key,
        description=f"The round face of {subject}: {look}. A keyhole at the centre; a plain, very dark ring track between "
                    f"radius 52 and 68 from the centre (the game draws there); the outer rim beyond radius 68 decorated.",
        details=["a keyhole at the centre (80, 80)", "an empty dark ring track from radius 52 to 68", "a decorated rim"],
        palette=palette, replaces=None,
        notes="Today LockpickPhase draws the dial with plain vector circles. Integration: draw this under the dial.",
        geometry={"centre": [80, 80], "track_radius": [52, 68]})

# ---------------------------------------------------------------------------------------------------------------------
# Branding.

add(id="brand.logo", category="brand", title="Keywright logo", size=[320, 72], priority="P2", batch=7,
    subject="the game's logo: the word KEYWRIGHT in chunky, slightly ornate gold pixel letters with a dark plum outline, "
            "the K's leg formed by an old key and a small keyhole in the G",
    description="KEYWRIGHT, spelled exactly, in tall gold pixel capitals (gold ramp, lit from the upper left), a 1px "
                "night2 outline and a 2px night1 drop edge below the letters, a key worked into the K and a keyhole in "
                "the G.",
    details=["the letters K-E-Y-W-R-I-G-H-T exactly", "gold letters with a dark outline", "a key in the K",
             "a keyhole in the G"],
    palette=["gold:1-6", "night:1-2", "umber:1-2"], replaces=None,
    notes="New: the title screen (today a text label) and the store art. If the model can't spell it, letter it by hand "
          "in pixels.")
for size in (64, 48, 32, 16):
    add(id=f"brand.app_icon_{size}", category="brand", title=f"App icon {size}x{size}", size=[size, size],
        priority="P2", batch=7, max_colours=16 if size >= 32 else 10,
        subject="the game's icon: a small violet (epic) treasure chest with gold fittings, a golden key standing in its "
                "lock",
        description=f"The purple chest seen from the front, slightly from above, filling the {size}x{size} square, with "
                    "a golden key in the lock. Drawn for this size: at 16 and 32 the shapes are simplified by hand, not "
                    "shrunk.",
        details=["the purple chest", "a gold key in the lock", "readable at 16x16"],
        palette=["arcane:1-5", "gold:2-6", "night:1-2"],
        replaces="assets/ui/icon.png" if size == 64 else None,
        notes="The window and project icon (64); the Windows .exe icon is built from 16, 32, 48 and 64 (scaled 4x for "
              "256) in CI.")

# ---------------------------------------------------------------------------------------------------------------------
# Story scenes (new): backdrops for the scenes that today reuse the dimmed shop.

SCENES = [
    ("guild", "The Gilded Guild's counting-house",
     "A long, dim counting-house hall: tall ledger shelves as plain dark shapes, a great iron strongroom door at the back "
     "with the Guild's eye-and-keyhole emblem above it in gold, cold gold light from a single lamp. Black and gold, "
     "quiet and still.",
     ["night:1-5", "gold:3-5", "moss:1-2"], "Mister Vane's evenings and the Guild's nights (DuesPhase)."),
    ("bram", "Bram's back room",
     "The small back room behind Bram's stall: a low beamed ceiling, a hearth with a warm fire at one side, a few "
     "shelves with old opened chests as simple shapes, a worn armchair, a small window onto the dusk. Cosy and quiet.",
     ["umber:1-5", "flame:1-3", "gold:4-5", "arcane:1-2"], "Bram's scenes (the welcome, his secret shard)."),
    ("keyhold", "The ruins of the Keyhold",
     "The broken tower of the archmages at night: a circular chamber open to the sky, a few cracked pillars as plain grey "
     "shapes, a great keyhole carved into the floor glowing faintly violet, a handful of stars. Calm and dark.",
     ["arcane:1-5", "night:1-5", "azure:3-4"], "The Hooded Woman's scenes."),
    ("vault", "Beneath the Gilded Gate",
     "A vault under the palace: a few wide steps down to the great arched Gilded Gate, its gold bars glowing a faint, "
     "sickly gold, and in front of it, on a plain stone dais, the Crown Chest under a single soft beam of light. Dark "
     "and still.",
     ["gold:2-5", "crimson:1-3", "night:1-5"], "The Crown Chest's scene and the finale."),
]
for key, title, description, palette, context in SCENES:
    add(id=f"scene.{key}", category="scene", title=title, description=description, palette=palette, replaces=None,
        notes=f"{context} ScenePhase shows it behind the week's scene (Story.Setting).")

# ---------------------------------------------------------------------------------------------------------------------
# Steam store art (not used in the game). Native art in the game's pixel style, scaled by whole numbers to Steam's
# sizes; the logo (brand.logo) composited where Steam wants the name.

KEY_ART = ("The player's hands at Bram's stall counter at dusk, a violet chest straining open with warm light spilling "
           "from its seam and coins, a sword hilt and a glowing potion bursting out; behind the counter a hooded seller "
           "and Uncle Bram; the Gilded Gate faintly on the horizon.")
STORE = [
    ("header_capsule", [460, 215], 2, "logo across the upper middle", True),
    ("small_capsule", [231, 87], 2, "logo filling the middle (the art is mostly the chest)", True),
    ("main_capsule", [616, 353], 2, "logo in the upper left third", True),
    ("vertical_capsule", [374, 448], 2, "logo across the top quarter", True),
    ("page_background", [719, 405], 2, "no logo; dark and calm, the art faded toward the edges", False),
    ("library_capsule", [300, 450], 2, "logo across the lower third", True),
    ("library_hero", [960, 310], 4, "no logo (Steam places it); the subject in the middle third, nothing important at "
                                    "the edges", False),
    ("community_icon", [92, 92], 2, "no logo: the chest with a key, like the app icon", False),
]
for key, size, factor, logo, needs_logo in STORE:
    add(id=f"store.{key}", category="store", title=key.replace("_", " ").capitalize(), size=size, replaces=None,
        scale=factor, deliver_size=[size[0] * factor, size[1] * factor], logo=logo, needs_logo=needs_logo,
        subject=KEY_ART if key not in ("community_icon",) else "The violet chest with a golden key in its lock.",
        description=f"Keep this area clear for the logo: {logo}.",
        palette=["arcane", "gold", "flame", "umber", "night", "crimson"],
        notes=f"Steam size {size[0] * factor}x{size[1] * factor} = this art at {factor}x (nearest). Deliver the native "
              f"art and the final image ({'with' if needs_logo else 'without'} brand.logo composited).")
add(id="store.library_logo", category="store", title="Library logo", size=[640, 360], scale=2, deliver_size=[1280, 720],
    background="transparent", replaces=None, needs_logo=True, mode="derive",
    generation="Made from brand.logo: scaled by a whole number and centred on the transparent canvas.",
    subject="brand.logo on a transparent 640x360 canvas, centred, as large as fits with a small margin",
    description="The logo alone, transparent around it, for Steam's library (placed over the hero).",
    notes="Made from brand.logo, scaled by a whole number; not generated separately.")
