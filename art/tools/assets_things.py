"""Things in chests (six kinds, five qualities each, and the three fakes) and the nine Heartsongs."""

QUALITY = ["common", "uncommon", "rare", "epic", "legendary"]


def item(key, title, kind, quality, description, details, palette, size_hint, context="", anchor=False, avoid=()):
    asset = {
        "id": f"item.{key}",
        "category": "item",
        "title": title,
        "kind": kind,
        "quality": quality,
        "description": description,
        "details": details,
        "palette": palette,
        "size_hint": size_hint,
        "context": context,
        "replaces": f"assets/sprites/items/{key}.png (and assets/sprites/items/icons/{key}.png)",
        "reference_current": "art/reference/current/items_4x.png",
        "avoid": list(avoid),
    }
    if anchor:
        asset["anchor_for"] = "item"
        asset["batch"] = 0
    return asset


def song(key, sprite, title, giver, description, details, palette, effect, anchor=False):
    asset = {
        "id": f"heartsong.{sprite}",
        "category": "heartsong",
        "title": title,
        "heartsong": key,
        "description": description,
        "details": details,
        "palette": palette + ["crimson:4-5"],
        "size_hint": "22-30 px",
        "context": f"Given to the Locking by {giver}. On display: {effect}",
        "replaces": f"assets/sprites/items/{sprite}.png (and assets/sprites/items/icons/{sprite}.png)",
        "reference_current": "art/reference/current/items_4x.png",
    }
    if anchor:
        asset["anchor_for"] = "heartsong"
        asset["batch"] = 0
    return asset


ASSETS = [
    # ---- Coin: jingles at a gentle shake ----
    item("barrow_silver", "Barrow Silver", "coin", "common",
         "A small heap of old grave-silver coins from the Barrow: irregular, hand-struck silver discs, some tarnished "
         "almost black at the edges, one showing a crude crowned head.",
         ["five or six coins in a low heap", "uneven tarnish (dark edges, bright faces)", "a crude stamped head on one"],
         ["steel:1-4", "night:2-4", "night:10"], "18-22 px wide, 12-16 px tall",
         "The commonest coin: grave silver from the Barrow."),
    item("moonsilver_marks", "Moonsilver Marks", "coin", "uncommon",
         "Elven silver coins with a crescent moon stamped on each: a neat stack of five with one coin leaning against it, "
         "cool bluish silver with a pale sheen.",
         ["a neat stack plus one leaning coin", "crescent moons on the faces", "a cool blue-white sheen"],
         ["steel:2-4", "azure:4-6", "night:10"], "18-22 px wide, 18-22 px tall"),
    item("dwarf_gold_crowns", "Dwarf-Gold Crowns", "coin", "rare",
         "Thick dwarven gold coins with a square hole through the middle, stamped with a hammer and anvil: one big coin "
         "standing face-on in front of a short stack of three.",
         ["one large face-on coin with a square hole", "a hammer-and-anvil stamp", "a short stack behind it",
          "rich, warm gold"],
         ["gold:1-6", "umber:1-2"], "20-24 px wide, 20-24 px tall",
         "Also the game's coin everywhere: the purse in the top bar, the offer slider's handle, the coins that fly into "
         "the purse. It must read as 'gold coins' at 1x.", anchor=True),
    item("sunken_sovereigns", "Sunken Sovereigns", "coin", "epic",
         "Gold sovereigns from the drowned town under the Aldwater: a heap of large gold coins crusted with teal "
         "verdigris and a tiny barnacle, a strand of dark green river weed draped over them, the gold still gleaming "
         "beneath.",
         ["a heap of large gold coins", "teal verdigris crust", "a strand of river weed", "a bright gleam on top"],
         ["gold:2-6", "sea:2-4", "leaf:1-2"], "20-26 px wide, 16-20 px tall"),
    item("dragon_gold", "Dragon Gold", "coin", "legendary",
         "A glittering pile of red-gold coins from a dragon's hoard, crowned with a small jewelled crown and a ruby, the "
         "coins with a warm, fiery sheen.",
         ["a tall pile of red-gold coins", "a small jewelled crown on top", "a ruby", "fiery highlights"],
         ["gold:2-6", "flame:2-4", "crimson:3-5"], "24-30 px wide, 22-28 px tall"),
    item("demon_gold", "Demon Gold", "coin", "fake",
         "A heap of coins that pass for gold at a glance but are gilded iron: dull brassy gilt worn through to dark grey "
         "iron at the rims, a faint sickly green tinge, one coin stamped with a tiny horned face.",
         ["a heap like Barrow Silver's", "gilt worn through to grey iron at the edges", "a sickly green tinge",
          "a tiny horned face on one coin"],
         ["gold:2-4", "steel:1-2", "moss:1-3"], "18-22 px wide, 12-16 px tall",
         "The Maw's fake: iron under the gilt, so the lodestone silences it."),
    # ---- Weapons: clank at a firm shake ----
    item("goblin_cleaver", "Goblin Cleaver", "weapons", "common",
         "A crude, chunky goblin cleaver: a broad notched iron blade, a stubby handle wrapped in dirty leather and string, "
         "lying on the diagonal, handle at the lower left.",
         ["a broad notched blade", "a leather-wrapped handle with string", "chunky goblin workmanship"],
         ["steel:1-3", "umber:2-4", "night:3-5"], "on the diagonal, 24-28 px"),
    item("dwarven_axe", "Dwarven Axe", "weapons", "uncommon",
         "A well-made dwarven axe: a single-bitted steel head engraved with simple knotwork, an oak haft bound with bronze "
         "rings, lying on the diagonal, haft at the lower left.",
         ["a steel axe head with knotwork", "an oak haft with bronze bands", "solid, sturdy proportions"],
         ["steel:2-4", "umber:3-5", "gold:3-4"], "on the diagonal, 26-30 px"),
    item("wardens_blade", "Warden's Blade", "weapons", "rare",
         "A straight longsword of the Wardens who carried the Locking's chests: a plain, fine steel blade with a blue "
         "sheen, a crossguard shaped like a key's bow, a blue leather grip and a round pommel, on the diagonal, hilt at "
         "the lower left.",
         ["a straight steel blade with a blue sheen", "a key-shaped crossguard", "a blue leather grip"],
         ["steel:2-4", "azure:2-4", "night:10"], "on the diagonal, 28-30 px"),
    item("flamebrand", "Flamebrand", "weapons", "epic",
         "A sword whose dark steel blade is veined with molten orange and red, its edge flickering like embers, a black "
         "crossguard with red-gold fittings, on the diagonal, hilt at the lower left. The fire is in the blade's own "
         "pixels, not a glow around it.",
         ["a dark blade with molten orange veins", "an ember-bright edge", "a black guard with red-gold fittings"],
         ["night:2-4", "flame:1-4", "crimson:3-4", "gold:4-5"], "on the diagonal, 28-30 px"),
    item("dragonslayer", "Dragonslayer", "weapons", "legendary",
         "A legendary greatsword: a broad blade etched with pale cyan rune-lines (patterns, not letters), a gold "
         "crossguard shaped like spread dragon wings, a ruby pommel, a crimson grip, on the diagonal, filling the box.",
         ["a broad blade with cyan rune-lines", "gold dragon-wing crossguard", "a ruby pommel"],
         ["steel:2-4", "azure:5-6", "gold:3-6", "crimson:3-5"], "on the diagonal, 30 px"),
    # ---- Cloaks: flump at a hard shake ----
    item("wolfskin_cloak", "Wolfskin Cloak", "cloaks", "common",
         "A rough wolf-pelt cloak hanging as if on a peg: grey-brown fur, the wolf's head forming the hood with two ears, "
         "a bone toggle at the throat.",
         ["grey-brown fur in shaggy clumps", "the wolf's head as the hood, with ears", "a bone toggle"],
         ["umber:2-5", "night:4-7", "parchment:2"], "24-28 px wide, 26-30 px tall"),
    item("elven_cloak", "Elven Cloak", "cloaks", "uncommon",
         "A fine forest-green hooded cloak of elven weave hanging in long elegant folds, a silver leaf-shaped clasp at the "
         "throat.",
         ["long elegant folds", "forest green", "a silver leaf clasp"],
         ["leaf:1-4", "steel:3-4"], "22-26 px wide, 26-30 px tall"),
    item("cloak_of_shadows", "Cloak of Shadows", "cloaks", "rare",
         "A deep blue-black hooded cloak that seems to drink the light, its hem fraying into wisps of shadow, a dim "
         "violet lining showing at the opening.",
         ["blue-black cloth", "the hem dissolving into wisps", "a dim violet lining"],
         ["night:1-4", "azure:1-2", "arcane:2-3"], "22-28 px wide, 26-30 px tall"),
    item("archmages_mantle", "Archmage's Mantle", "cloaks", "epic",
         "A rich indigo mantle with a high stiff collar, embroidered with small silver stars, edged with gold braid.",
         ["a high stiff collar", "silver star embroidery", "gold braid edging"],
         ["arcane:1-4", "steel:3-4", "gold:4-5"], "24-28 px wide, 24-28 px tall"),
    item("phoenix_mantle", "Phoenix Mantle", "cloaks", "legendary",
         "A cloak of red and gold phoenix feathers layered like flames, rising to a gold clasp at the collar; it looks "
         "warm to the touch.",
         ["layers of feathers shaped like flames", "red at the hem rising to gold", "a gold clasp"],
         ["crimson:3-5", "flame:2-4", "gold:4-6"], "24-30 px wide, 26-30 px tall"),
    # ---- Potions: slosh at a gentle shake; glass ----
    item("healing_draught", "Healing Draught", "potions", "common",
         "A round-bellied glass flask of red healing potion with a cork stopper and a twine tie around the neck.",
         ["a round glass belly", "red liquid with a light reflection", "cork and twine"],
         ["crimson:2-4", "steel:3-4", "skin:2-3", "night:10"], "16-20 px wide, 20-24 px tall"),
    item("elixir_of_vigour", "Elixir of Vigour", "potions", "uncommon",
         "A tall square glass bottle of bright orange elixir with a red wax-sealed cork.",
         ["a tall square bottle", "bright orange liquid", "a red wax seal over the cork"],
         ["flame:2-4", "steel:3-4", "crimson:3"], "12-16 px wide, 22-26 px tall"),
    item("invisibility_draught", "Invisibility Draught", "potions", "rare",
         "A teardrop-shaped flask whose pale blue potion fades toward clear at the top (in flat steps of paler blue, not "
         "transparency), with a silver stopper.",
         ["a teardrop flask", "pale blue liquid fading upward in steps", "a silver stopper"],
         ["azure:4-6", "steel:3-4", "night:10"], "16-20 px wide, 20-24 px tall"),
    item("mermaids_breath", "Mermaid's Breath", "potions", "epic",
         "A seashell-shaped glass vial of sea-teal liquid with tiny bubbles inside, a pearl stopper.",
         ["a scallop-shell-shaped vial", "sea-teal liquid with bubbles", "a pearl stopper"],
         ["sea:2-5", "rose:2-3", "night:9-10"], "14-18 px wide, 22-26 px tall"),
    item("ambrosia", "Ambrosia", "potions", "legendary",
         "An ornate golden amphora-shaped flask of shining golden nectar with a gold stopper and a single drip running "
         "down its side.",
         ["an amphora-shaped golden flask", "shining nectar", "a gold stopper", "one golden drip"],
         ["gold:2-6", "flame:3-4"], "16-20 px wide, 22-26 px tall"),
    # ---- Spells: rustle at a hard shake ----
    item("scroll_of_sparks", "Scroll of Sparks", "spells", "common",
         "A rolled parchment scroll tied with a blue ribbon and a blue wax seal, a few tiny spark marks on the paper (not "
         "writing), lying on the diagonal.",
         ["a rolled parchment", "blue ribbon and seal", "tiny spark marks"],
         ["parchment:1-3", "azure:2-4", "umber:3"], "on the diagonal, 22-26 px"),
    item("scroll_of_fireball", "Scroll of Fireball", "spells", "uncommon",
         "A partly unrolled parchment between two wooden rollers, showing a fiery orange-red circular sigil (a pattern, "
         "not letters).",
         ["parchment between wooden rollers", "a fiery circular sigil"],
         ["parchment:1-3", "flame:2-4", "umber:3-4"], "22-26 px wide, 18-22 px tall"),
    item("summoners_grimoire", "Summoner's Grimoire", "spells", "rare",
         "A thick purple leather grimoire with brass corners and a clasp, a golden eye on the cover, standing upright.",
         ["thick purple leather binding", "a golden eye on the cover", "brass corners and a clasp"],
         ["arcane:1-4", "gold:3-5", "parchment:3"], "18-22 px wide, 20-24 px tall"),
    item("lichs_tome", "Lich's Tome", "spells", "epic",
         "A black-bound tome with a bone-white skull on the cover, iron corners, and pages tinged a faint sickly "
         "green-violet, standing upright.",
         ["black binding", "a bone-white skull", "iron corners", "faintly tinged pages"],
         ["night:1-3", "parchment:2-3", "steel:2", "moss:2-3"], "18-22 px wide, 20-24 px tall"),
    item("keyhold_codex", "Keyhold Codex", "spells", "legendary",
         "The archmages' own codex: a deep blue book bound in gold, a golden keyhole emblem on the cover, a key-shaped "
         "gold clasp and gilded page edges, standing upright.",
         ["deep blue binding with gold", "a golden keyhole emblem", "a key-shaped clasp", "gilded page edges"],
         ["azure:1-3", "gold:3-6"], "20-24 px wide, 22-26 px tall"),
    item("sand_case", "Scroll Case of Sand", "spells", "fake",
         "A cheap tin scroll case lying tipped over with its cap off, pouring a little heap of sand instead of a scroll.",
         ["a dented tin scroll case", "the cap off beside it", "sand spilling into a small heap"],
         ["steel:1-3", "parchment:1-3", "gold:3"], "on the diagonal, 24-28 px",
         "Sand pours at a gentle shake; parchment only rustles at a hard one."),
    # ---- Gems: tinkle at a gentle shake ----
    item("moonstone", "Moonstone", "gems", "common",
         "A smooth round cabochon of milky, faintly blue-white moonstone with a soft shimmer, loose.",
         ["a smooth round dome", "milky blue-white", "a soft shimmer line"],
         ["steel:3-4", "azure:5-6", "night:8-10"], "14-18 px"),
    item("elfstone", "Elfstone", "gems", "uncommon",
         "A green gem in a silver leaf-shaped pendant on a fine silver chain, laid in a loop.",
         ["a green gem", "a leaf-shaped silver setting", "a fine chain"],
         ["leaf:2-6", "steel:2-4"], "14-18 px wide, 18-22 px tall"),
    item("blood_ruby", "Blood Ruby", "gems", "rare",
         "A large, classically faceted deep-red ruby with a bright white glint.",
         ["a faceted ruby (table, crown and pavilion facets)", "deep red to bright red facets", "a white glint"],
         ["crimson:1-6", "night:10"], "18-22 px wide, 16-20 px tall"),
    item("starstone", "Starstone", "gems", "epic",
         "A round sapphire cabochon with a six-rayed star in it, set in a gold bezel.",
         ["a round blue cabochon", "a six-rayed star inside", "a gold bezel"],
         ["azure:1-6", "gold:3-5"], "18-22 px"),
    item("dragons_eye", "Dragon's Eye", "gems", "legendary",
         "A great amber-gold gem with a dark slit pupil like a dragon's eye, held in a spiky gold mount.",
         ["an amber-gold gem with a slit pupil", "a spiky gold mount"],
         ["flame:2-4", "gold:2-6", "night:1"], "22-26 px wide, 18-22 px tall"),
    item("paste_gem", "Paste Gem", "gems", "fake",
         "A big faceted gem of cheap glass: too clear and too even, greyish, without fire, with a chip out of one edge.",
         ["a big faceted 'gem'", "flat, lifeless greys", "a chip on one edge"],
         ["steel:1-4", "night:6-9"], "18-22 px wide, 16-20 px tall",
         "Lady Ermine's maid packs paste with the real ones."),
    # ---- The nine Heartsongs ----
    song("lullaby", "lullaby_box", "The Lullaby Box", "a farmer's wife at Brindle",
         "The music box that sang her children to sleep: a small wooden box with a hand-painted lid of flowers and a "
         "crescent moon, the lid slightly ajar, a little brass crank on the side, its corners worn pale with years of "
         "loving use, a faded red ribbon tied around it.",
         ["a small wooden music box", "painted flowers and a crescent moon on the lid", "a brass crank",
          "worn corners", "a faded red ribbon"],
         ["umber:2-6", "gold:4-5", "leaf:3-4", "azure:4"],
         "calms every seller: nobody hurries you, and everyone asks a fifth less.", anchor=True),
    song("pipe", "shepherds_pipe", "The Shepherd's Pipe", "a shepherd of the Brindle hills",
         "Hand-carved wooden pan pipes of seven graded reeds bound with twine, worn smooth by a shepherd's hands, a red "
         "woollen cord with a tassel hanging from one end.",
         ["seven graded wooden pipes", "twine bindings", "a red woollen tassel"],
         ["umber:2-6", "parchment:2-3"],
         "calls folk to your door: two more customers a day."),
    song("chimes", "fey_chimes", "The Fey Chimes", "the fair folk of the Old Wood",
         "Wind chimes from the fair folk's door between the worlds: five slender rods of glass and silver hanging on "
         "threads from a curved birch branch, a red thread tied around the branch.",
         ["a curved birch branch", "five hanging glass and silver rods of different lengths", "a red thread"],
         ["night:8-10", "steel:2-4", "leaf:4-6", "sea:4-5"],
         "fey luck: a third of what comes out as promised comes out a step better."),
    song("egg", "dragons_egg", "The Dragon's Egg", "Ysolt, the last dragon of the Ember Peaks",
         "A large scaly egg, deep red and black, with ember-orange warmth glowing between its scales (in the scales' own "
         "colours), resting in a small nest of straw.",
         ["an upright egg covered in scales", "ember-orange lines between dark red scales", "a little straw nest"],
         ["crimson:1-4", "flame:2-4", "gold:4-5"],
         "dragon-gold: coin and gems sell for twice their worth."),
    song("lark", "clockwork_lark", "The Clockwork Lark", "a tinker of Crownhold",
         "A small brass mechanical songbird perched on a wooden stand, a wind-up key in its back, rivets on its wings, a "
         "red enamel breast, its beak open as if about to sing.",
         ["a brass songbird with riveted wings", "a wind-up key in its back", "a red enamel breast", "a wooden perch"],
         ["gold:2-5", "umber:3-4"],
         "carries word across Aldmark: two more commissions a week, each paying half again."),
    song("hearth", "hearthstone", "The Hearthstone", "the dwarves of Ironroot",
         "A rounded hearth stone worn smooth by three thousand years of warm evenings, grey with soot at its base, glowing "
         "ember-red lines running through it like veins.",
         ["a smooth rounded stone", "ember-red veins", "soot at the base"],
         ["night:4-7", "umber:1-2", "flame:2-3"],
         "folk come for miles to sit by it, and pay at the door for every point of exhibition."),
    song("bell", "drowned_bell", "The Drowned Bell", "old Aldwater, before the river took it",
         "A small bronze chapel bell gone green with verdigris, a loop at the top with a frayed red bell rope, a strand of "
         "river weed caught on its lip.",
         ["a bronze bell with green verdigris", "a frayed red rope", "a strand of river weed"],
         ["sea:2-5", "gold:2-4", "leaf:2"],
         "it still counts: it tolls once for every thing in a chest as it comes to the counter."),
    song("star", "bottled_star", "The Bottled Star", "the archmages of the Keyhold",
         "A round glass bottle with a cork, holding a tiny star: a bright white-gold point with four short pixel rays, "
         "a red string tied around the neck with a little blank card.",
         ["a clear glass bottle", "a tiny bright star inside, drawn in pixels", "a red string around the neck"],
         ["steel:3-4", "night:10", "gold:5-6", "azure:5-6"],
         "starlight: enchanted things glow whenever they sound, and turn up far more often."),
    song("ember", "imps_ember", "The Imp's Ember", "Pib, an imp who loved a miller's daughter",
         "An imp's own heart: a glowing red-orange coal shaped like a heart, tiny flames licking at its top, cradled in a "
         "small black iron dish.",
         ["a heart-shaped coal", "red-orange glow in its own colours", "tiny flames on top", "a small iron dish"],
         ["flame:1-4", "night:2-4"],
         "its heat tempers all it warms: glass never breaks, nothing comes out worse, and every fake burns true."),
]
