"""
Figures (round 2): everyone who comes to the counter, the story's people and Bram's kin, full length, standing. They
replace round 1's portraits: the game stands them behind the counter (which hides their legs), crops their faces for
the queue and the notebook's rows, and stands them in story scenes.

`height` is the whole silhouette in art pixels, hats, hair and horns included (the canvas is 96x160, feet on the bottom
row): what a goblin and a knight are next to each other. The counter's edge comes at 68 rows above the feet; anyone
shorter than 108 stands on a box behind it so their head and shoulders show (art/tools/scene.py).
"""

THREE_QUARTER = "Turned a little toward the right, looking right or at the viewer."
FRONTAL = "Facing the viewer squarely, symmetrical."

ASSETS = []


def figure(key, title, place, height, description, details, palette, context, view=THREE_QUARTER, priority="P1",
           held=None, anchor=False, notes=None):
    asset = {
        "id": f"figure.{key}",
        "category": "figure",
        "title": title,
        "place": place,
        "height": height,
        "description": description,
        "details": details,
        "palette": palette,
        "context": context,
        "priority": priority,
        "replaces": f"assets/sprites/sellers/{key}.png",
        "frame_fields": {"view": view, "held": f", holding {held} at chest height" if held else ""},
        "avoid": [],
    }
    if anchor:
        asset["anchor_for"] = "figure"
    if notes:
        asset["notes"] = notes
    ASSETS.append(asset)


# ---- Bram's Stall, the Brindle crossroads (tier 1) ----
figure("tam", "Little Tam", "stall", 84,
       "A village boy of about ten who fishes chests out of the millpond: round freckled face, a gap-toothed grin, messy "
       "brown hair, an oversized moss-green tunic with the sleeves rolled up, short brown breeches, bare feet.",
       ["a too-big green tunic", "a gap-toothed grin"], ["skin:3-5", "umber:2-4", "leaf:2-4"],
       "Tier 1. Has no idea what's in his chests: a few coins buys anything.")
figure("penn", "Widow Penn", "stall", 112,
       "A kindly soldier's widow in her sixties, small and neat: grey hair in a bun, a soft face with rosy cheeks and a "
       "small sad smile, a long black mourning dress to her ankles, a plum knitted shawl round her shoulders.",
       ["grey hair in a bun", "the plum shawl over a black dress"], ["skin:3-5", "night:2-4", "rose:1-2"],
       "Tier 1. Shakes gently: heavy weapons and soft cloaks make no sound for her.")
figure("hob", "Hob the Farmer", "stall", 128,
       "A broad, sunburnt farmer in his fifties who ploughs chests up out of the old battlefield: a frayed straw hat, a "
       "ruddy face with grey stubble and a shrewd half-smile, a brown work smock over a homespun shirt, brown trousers "
       "and heavy boots.",
       ["the straw hat", "the brown work smock"], ["skin:2-4", "gold:3-5", "umber:2-5"],
       "Tier 1. Shakes hard (a clank drowns out the coin for him) and haggles.", anchor=True)
figure("mags", "Mags the Innkeeper", "stall", 118,
       "The hearty innkeeper of the Crossed Keys, in her forties: copper-red hair pinned up, rosy freckled cheeks, a wide "
       "laughing mouth, a green bodice over a white blouse, a long brown skirt and a cream apron.",
       ["piled-up red hair", "the cream apron over a green bodice"], ["skin:3-5", "flame:1-2", "leaf:2-3", "parchment:2-3"],
       "Tier 1. Takes chests for unpaid tabs; counts the rattles and charges the same for each.")
figure("dorrin", "Dorrin the Delver", "stall", 124,
       "A young adventurer of about twenty, just back from the Barrow: tousled blond hair under a leather headband, an "
       "eager grin, a battered brown leather jerkin, dark breeches and tall boots, a small brass lantern at his belt.",
       ["blond hair and a leather headband", "the little lantern at his belt"], ["skin:3-5", "gold:4-5", "umber:2-4"],
       "Tier 1. His rotten Barrow chests stand about two seconds of shaking.")

# ---- Thornwick Market (tier 2) ----
figure("snikkit", "Snikkit the Goblin", "thornwick", 76,
       "A wiry goblin from the hills: leaf-green skin, long pointed ears sticking out sideways, big yellow eyes, a sly "
       "toothy grin, a ragged brown hood pushed back, a patched tunic and bare green feet, a sack over one shoulder.",
       ["green skin and huge pointed ears", "the ragged brown hood"], ["leaf:2-4", "umber:2-4", "gold:5"],
       "Tier 2. Sells what his cousins pinch; charges a tenth more for each shake strength after the first.")
figure("wenna", "Wenna the Huntress", "thornwick", 128,
       "A huntress in her thirties: a tanned face with calm steady eyes, an auburn braid over her shoulder, a deep green "
       "hood with a red feather, a leather jerkin, green breeches and soft boots, a bow on her back.",
       ["green hood with a red feather", "the bow on her back"], ["skin:2-4", "leaf:1-3", "umber:2-4", "crimson:3"],
       "Tier 2. Weighs every chest on her game scale; her chests hang on a skittish mare.")
figure("bramble", "Mother Bramble", "thornwick", 112,
       "An ancient druid of the Old Wood, a little stooped: a wild cloud of white hair with a few leaves in it, a kind "
       "lined face with eyes half-closed as if dreaming, a heavy mossy green-brown cloak to the ground, a sprig of holly "
       "with red berries at the collar.",
       ["wild white hair with leaves", "the mossy cloak to the ground"], ["night:8-9", "moss:1-3", "umber:2-3", "crimson:3"],
       "Tier 2. Dreams her price, and always dreams true, as a sign.")
figure("ansel", "Brother Ansel", "thornwick", 120,
       "A young monk in his early twenties: tonsured brown hair, a round earnest face with a shy hopeful smile, a brown "
       "wool habit to his sandals with a rope cord, a small wooden key-shaped symbol on a cord at his chest.",
       ["the tonsure", "brown habit and rope cord"], ["skin:3-5", "umber:2-5"],
       "Tier 2. Won't have the saint's things shaken hard.")
figure("fennick", "Fennick the Smuggler", "thornwick", 132,
       "A lean smuggler in his forties: a black wide-brimmed hat pulled low, a thin black moustache and a knowing smirk, a "
       "gold hoop earring, a long dark coat with the collar turned up, dark boots.",
       ["black wide-brimmed hat", "the long dark coat, collar up"], ["skin:2-4", "night:2-4", "gold:5"],
       "Tier 2. Packs in straw, so everything stirs a strength later; often a false bottom.")
figure("gerda", "Gerda the Matchmaker", "thornwick", 110,
       "Thornwick's plump, grandmotherly matchmaker in her sixties: silver hair under a white lace cap, rosy round cheeks, "
       "a matchmaker's sly smile, a red embroidered bodice over a white blouse, a full dark skirt.",
       ["white lace cap", "red bodice"], ["skin:3-5", "night:8-9", "crimson:2-4", "night:3"],
       "Tier 2. A Thornwick dowry always holds coin, a cloak and a potion.")

# ---- Crownhold (tier 3) ----
figure("wren", "Wren the Apprentice", "crownhold", 116,
       "An archmage's apprentice of about seventeen: a copper-red bob, round spectacles slipping down the nose, an eager "
       "flustered look, a long blue apprentice's robe with a high collar, a paper tag on a string round the neck.",
       ["round spectacles and a copper-red bob", "the long blue robe"], ["skin:3-5", "flame:1-2", "azure:2-4", "parchment:3"],
       "Tier 3. Every chest has its packing tag listing each kind packed, warded or not.")
figure("orrin", "Archmage Orrin", "crownhold", 146,
       "An archmage old enough to remember the Locking: a long white beard to his belt, bushy white eyebrows, twinkling "
       "eyes, a tall pointed indigo hat with a few small silver stars, deep indigo robes to the floor.",
       ["tall pointed indigo hat", "the long white beard"], ["arcane:1-3", "night:8-10", "steel:3-4"],
       "Tier 3. Wagers: read his chest exactly right and he returns a third of your price.")
figure("grit", "Grit the Miner", "crownhold", 92,
       "A dwarf of the Ironroot hold, short and broad: a huge braided orange beard, small fierce eyes under thick brows, "
       "a dented iron helmet with a candle stub on top, a heavy leather apron over chain links, big boots.",
       ["the huge braided orange beard", "the dented helmet with a candle"], ["skin:2-4", "flame:2-3", "night:3-5", "umber:2-4"],
       "Tier 3. Knows weapons; takes every tinkle for a diamond. Haggles.")
figure("corisande", "Lady Corisande", "crownhold", 128,
       "A poised noblewoman in her forties: auburn hair in an elegant updo with a pearl pin, a pale composed face with one "
       "eyebrow raised, a long lavender silk gown with a high lace collar.",
       ["auburn updo with a pearl pin", "the lavender gown"], ["skin:4-5", "flame:1", "arcane:4-5", "night:9"],
       "Tier 3. Grandmother's riddle on each lid is about one thing inside.")
figure("pell", "Pell the Pawnbroker", "crownhold", 116,
       "An elderly pawnbroker, thin and precise: thinning grey hair combed flat, a sharp nose, a gold monocle on a chain, "
       "an olive waistcoat over a white shirt with sleeve garters, dark trousers.",
       ["the gold monocle and chain", "the olive waistcoat"], ["skin:3-5", "night:6-9", "moss:1-2", "gold:4-5"],
       "Tier 3. His pawn ticket counts every article pledged, false bottom and all.")
figure("quill", "Quill the Archivist", "crownhold", 124,
       "The Royal Archive's clerk, in his thirties: neat dark hair parted in the middle, round wire glasses, an anxious "
       "bookish face, a long ink-blue scholar's robe with a white collar band, a rolled parchment under his arm.",
       ["round wire glasses", "the rolled parchment under his arm"], ["skin:3-5", "azure:1-3", "parchment:2-3"],
       "Tier 3. The ward-roll names the warded thing, every other letter faded.")
figure("shelly", "Shelly the Mudlark", "crownhold", 112,
       "A young mudlark of about sixteen who combs the Aldwater mud: tangled blonde hair under a faded blue headscarf, a "
       "cheeky grin, a patched brown smock, rolled-up trousers, muddy bare feet, a string of small shells round her neck.",
       ["blue headscarf over blonde hair", "the patched brown smock"], ["skin:3-5", "gold:5", "azure:3", "umber:2-4"],
       "Tier 3. Prices every rustle as a drowned wizard's spellbook; her scroll cases are often full of sand.")
figure("aldous", "Sir Aldous", "crownhold", 142,
       "An ageing knight looking for his grandfather's sword: a steel helm with the visor raised and a red plume, a grey "
       "moustache, tired kind eyes, steel armour with a white tabard bearing a red emblem, steel boots.",
       ["steel helm with a red plume", "the white tabard over armour"], ["steel:1-4", "night:9", "crimson:3-4", "skin:3-4"],
       "Tier 3. If there are weapons inside, the best is his, for a price he names.")

# ---- The Royal Emporium (tier 4) ----
figure("vexis", "Vexis the Imp", "emporium", 64,
       "A small, impatient imp: crimson skin, two curved ivory horns, pointed ears, yellow eyes, a toothy smirk, small bat "
       "wings, a thin tail, and a little gold hourglass on a chain at its chest.",
       ["crimson skin and curved ivory horns", "the gold hourglass pendant"], ["crimson:2-4", "parchment:3", "gold:4-5"],
       "Tier 4. Gives you twelve seconds.", notes="Small enough to stand on the counter (the game puts it there).")
figure("malbranche", "Malbranche", "emporium", 140,
       "A tall skeletal devil who deals in bargains: a horned skull with tiny ember-red eyes and a lipless grin, a long "
       "high-collared black cloak trimmed with gold, a gold medallion like a wax seal at the throat.",
       ["a horned skull", "the high black collar with gold trim"], ["parchment:2-3", "night:1-3", "gold:4-5", "flame:2"],
       "Tier 4. Cheap, but the dearest thing inside is his.")
figure("skrae", "Skrae", "emporium", 112,
       "A demon that listens: dark violet leathery skin, two enormous bat ears spreading wide, small red eyes, a little "
       "fanged mouth, a hunched thin body in a plain dark wrap, head tilted as if catching an echo.",
       ["huge bat ears", "dark violet skin"], ["arcane:1-3", "night:2-3", "crimson:4"],
       "Tier 4. Prices what it hears, fairly, and clicks to hear the echoes off everything inside.", view=FRONTAL)
figure("ermine", "Lady Ermine", "emporium", 144,
       "A haughty lady of the court: a tall white powdered wig, a pale powdered face with a beauty mark and half-lidded "
       "disdainful eyes, an ermine stole (white fur, a few black tips) over a long violet gown.",
       ["tall white powdered wig", "the ermine stole"], ["night:8-10", "arcane:2-4", "skin:5", "night:1"],
       "Tier 4. Knows only her own gems; some are paste. No lodestone near her gems.")
figure("oriel", "Madame Oriel", "emporium", 128,
       "The court's cartomancer: dark curly hair under a purple turban with a jewel at the front, gold hoop earrings, an "
       "enigmatic smile, a long flowing purple dress, a fan of three plain-backed cards in one hand.",
       ["purple turban with a jewel", "three cards fanned in her hand"], ["skin:2-3", "arcane:2-4", "gold:4-5"],
       "Tier 4. Draws three cards from the Locking's own deck: a suit drawn twice is what the ward holds.",
       held="three cards fanned out")
figure("silas", "Silas Gilt", "emporium", 124,
       "A fence for the Maw: slick black hair, a thin moustache, a too-wide smile with a gold tooth, a purple velvet coat "
       "hung with gaudy gold chains, black trousers and shiny shoes.",
       ["the purple velvet coat", "gaudy gold chains"], ["skin:3-5", "night:2-3", "arcane:2-3", "gold:4-5"],
       "Tier 4. Sells demon gold as gold, cheap.")
figure("aldo", "Old Aldo", "emporium", 118,
       "A tired old man in a plain grey wool cloak to his ankles with the hood pushed back: secretly King Aldric IX. A white "
       "beard, kind weary eyes, and a fine gold ring with a ruby on one hand, the only rich thing about him.",
       ["the plain grey hooded cloak", "the ruby ring"], ["skin:3-5", "night:5-8", "gold:5", "crimson:4"],
       "Tier 4. His price is your slip: read it exactly and he takes a quarter off.")
figure("maw", "Avarax, the Gilded Maw", "emporium", 146,
       "The demon of greed in a borrowed body: a face like a mask of molten gold, a crown of jagged gold spikes, glowing "
       "red eyes, a wide grin of gold teeth, a heavy robe hung with gold chains, big and top-heavy.",
       ["a molten-gold face and a crown of gold spikes", "a heavy robe hung with gold chains"],
       ["gold:2-6", "crimson:3-5", "night:1-3"], "Tier 4, rarely. Sells a chest for one coin, and everyone after him that "
       "day asks twice as much.", view=FRONTAL)

# ---- Bram's kin and the story's people ----
figure("bram", "Uncle Bram", "kin", 120,
       "The old Keywright, your mentor, in his late sixties: a warm round face with crinkled smiling eyes, a white "
       "moustache and trimmed white beard, a red felt cap, a red waistcoat over a cream shirt, brown trousers, a little "
       "stout.",
       ["red felt cap", "the red waistcoat"], ["skin:3-5", "night:9-10", "crimson:2-4", "parchment:2-3", "umber:2-3"],
       "The mentor: his tips run the tutorial (the notebook and his tips show his head and shoulders).")
figure("vane", "Mister Vane", "story", 136,
       "Collector of the Gilded Guild, tall and thin: a pale gaunt face, slicked-back black hair, cold grey eyes, a thin "
       "polite smile, a long high-collared black coat with gold buttons, the Guild's small gold eye-and-keyhole pin.",
       ["pale gaunt face and slicked-back hair", "the long high-collared black coat"], ["skin:4-5", "night:1-3", "gold:4-5"],
       "Comes on the Guild's nights for the tithe. He's the Maw's.")
figure("hood", "The Hooded Woman", "story", 124,
       "A mysterious woman in a deep violet hooded cloak to the ground, her face hidden in the hood's shadow but for two "
       "faint violet-glowing eyes; a silver clasp shaped like a key at her throat.",
       ["the deep violet hooded cloak", "the silver key clasp"], ["arcane:1-4", "night:1-2", "steel:3-4"],
       "Mara, the last archmage of the Keyhold, before she says so. Trades the Aldric Key's shards for things.", view=FRONTAL)
figure("nim", "Nim the Peddler", "story", 116,
       "A horned, cheerful half-something who sells curios: warm brown skin, two short curved horns, golden eyes, a wide "
       "friendly grin, a long patched red-and-mustard coat with many pockets, a peddler's pack on the back.",
       ["two short curved horns", "the patched red-and-mustard coat"], ["skin:1-3", "crimson:2-3", "gold:3-4", "parchment:2"],
       "Comes twice a week with curios: the game stands Nim behind the counter on those mornings.")
figure("stranger", "The Stranger", "story", 138,
       "A tall, silent figure in a black hooded cloak to the ground, the face all shadow but a faint glint where the eyes "
       "should be, a heavy iron chain slung over one shoulder.",
       ["a black hood with no face", "the heavy iron chain"], ["night:1-4", "steel:1-3"],
       "Brings a chest wrapped in iron chains, so nothing inside can move. Never says where from.", view=FRONTAL)
figure("kit", "Kit", "kin", 116,
       "Bram's niece, about eighteen, who grew up behind his counter: the Ear. Brown hair in a short ponytail under a red "
       "headband, freckles, bright attentive eyes, the head tilted as if listening, a green tunic, brown breeches, boots.",
       ["red headband and ponytail", "the listening tilt of the head"], ["skin:3-5", "crimson:3", "leaf:2-3", "umber:2-4"],
       "The first character (the character screen's cards show their heads and shoulders).")
figure("hesk", "Hesk", "kin", 120,
       "Bram's nephew, a grocer from the river towns, in his forties: the Scale. Stocky, a full brown beard, a flat green "
       "cap, an honest patient face, a long cream grocer's apron over a brown shirt, a hand at his sore back.",
       ["flat green cap and brown beard", "the long cream apron"], ["skin:3-5", "umber:2-4", "leaf:2-3", "parchment:2-3"],
       "Can't shake hard or drop (a bad back); knows what everything weighs.")
figure("rosa", "Rosa", "kin", 118,
       "Bram's niece from the Crownhold markets, in her early twenties: the Tongue. Fiery orange hair in a loose ponytail, a "
       "confident smirk, a green market-trader's vest over a white shirt, a red neckerchief, a long brown skirt.",
       ["fiery orange hair", "the red neckerchief and green vest"], ["skin:3-5", "flame:2-3", "leaf:2-3", "crimson:3"],
       "Everybody haggles with Rosa.")
figure("isolde", "Isolde", "kin", 124,
       "Bram's niece by marriage, a lady who has never sold a thing she liked: the Collection. Blonde hair in a braided "
       "crown, a calm proud gaze, a long wine-red gown with a gold embroidered collar.",
       ["blonde braided crown", "the wine-red gown with a gold collar"], ["skin:4-5", "gold:4-6", "crimson:2-3"],
       "Has a shelf from day one and charges visitors to see her collection.")
figure("mara", "Mara, unhooded", "story", 122,
       "The last archmage of the Keyhold, around sixty, after she tells you her name: cropped silver hair, sharp violet "
       "eyes, a weathered face, the violet hooded cloak with its hood down, a silver key-shaped clasp at the throat.",
       ["cropped silver hair", "the violet cloak, hood down"], ["skin:3-4", "night:8-9", "arcane:2-4", "steel:3"],
       "After week 4's reveal: in scenes and at the counter.", priority="P3")
figure("king", "King Aldric IX", "emporium", 124,
       "The same tired old man as Old Aldo, now as the King: a slim gold crown, the white beard, the kind weary eyes, a long "
       "royal crimson mantle with an ermine collar, the ruby ring.",
       ["the same face as Old Aldo", "a slim gold crown and crimson mantle"], ["skin:3-5", "crimson:2-4", "gold:4-5", "night:9-10"],
       "For the Crown Chest's scenes. Draw it after Old Aldo and pass that figure as a reference.", priority="P3")
