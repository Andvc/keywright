"""Portraits: everyone who comes to the counter, the story's people and Bram's kin (assets/sprites/sellers/{key}.png)."""

THREE_QUARTER = ("Three-quarter view: the face and shoulders turned toward the right side of the image (about 30 degrees), "
                 "eyes looking to the right.")
FRONTAL = "Frontal view, facing the viewer squarely, symmetrical."

BACKDROPS = {
    "stall": ("a dusk-violet vignette (deep violet at the corners, a soft warm-gold glow behind the head), for Bram's "
              "stall at the Brindle crossroads", ["arcane:1-3", "night:3-5", "gold:4"]),
    "thornwick": ("a deep forest-green vignette (dark pine green at the corners, a soft lamplit glow behind the head), for "
                  "Thornwick at the edge of the Old Wood", ["leaf:1-3", "sea:1-2"]),
    "crownhold": ("a slate-blue vignette (dark blue-grey at the corners, lighter slate behind the head), for Crownhold, the "
                  "capital", ["azure:1-3", "steel:1-2"]),
    "emporium": ("a crimson velvet vignette (near-black crimson at the corners, a faint gold glow behind the head), for the "
                 "Royal Emporium by the Gilded Gate", ["crimson:1-3", "gold:3-4"]),
    "story": ("a deep night-violet vignette (almost black at the corners), for the story's people",
              ["night:1-3", "arcane:1-2"]),
    "kin": ("a warm firelit brown vignette (dark umber at the corners, an orange hearth glow behind the head), for Bram's "
            "back room", ["umber:1-3", "flame:1"]),
}


def portrait(key, title, place, description, details, palette, context, view=THREE_QUARTER, priority="P1",
             replaces=True, anchor=False, avoid=(), notes=None):
    backdrop, backdrop_palette = BACKDROPS[place]
    asset = {
        "id": f"portrait.{key}",
        "category": "portrait",
        "title": title,
        "place": place,
        "description": description,
        "details": details,
        "palette": palette + backdrop_palette,
        "context": context,
        "priority": priority,
        "replaces": f"assets/sprites/sellers/{key}.png" if replaces else None,
        "reference_current": "art/reference/current/portraits_4x.png" if replaces else None,
        "composition": {"head_centre_x": 38, "eye_line_y": 34, "head_top_y": "14-18 (hair, hats and horns may go higher)",
                        "chin_y": 50, "shoulders_from_y": 56, "face_crop": [19, 14, 56, 51]},
        "avoid": list(avoid),
        "frame_fields": {"view": view, "backdrop": backdrop},
    }
    if anchor:
        asset["anchor_for"] = "portrait"
        asset["batch"] = 0
    if notes:
        asset["notes"] = notes
    if priority == "P3":
        asset["batch"] = 7
    return asset


ASSETS = [
    # ---- Bram's Stall, the Brindle crossroads (tier 1) ----
    portrait("tam", "Little Tam", "stall",
             "A village boy of about ten who fishes chests out of the millpond. Round freckled face, a gap-toothed grin, "
             "messy brown hair with a strand of green pondweed caught in it, an oversized moss-green tunic with the "
             "sleeves rolled up, still a little damp. Cheerful and guileless.",
             ["freckles across the nose", "the gap in his front teeth", "a strand of pondweed in his hair",
              "a too-big green tunic"],
             ["skin:3-5", "umber:2-4", "leaf:2-4"],
             "Tier 1. Has no idea what's in his chests: a few coins buys anything."),
    portrait("penn", "Widow Penn", "stall",
             "A kindly soldier's widow in her sixties, selling her late husband Tobb's things. Grey hair in a neat bun, a "
             "soft lined face with rosy cheeks, eyes gently lowered, a small sad smile. A black mourning dress with a white "
             "lace collar, a plum knitted shawl over her shoulders, and a small tin soldier's medal pinned at her breast.",
             ["grey hair in a bun", "the plum shawl", "the small medal on her dress", "gentle, sad smile"],
             ["skin:3-5", "night:2-4", "rose:1-2", "steel:2-4"],
             "Tier 1. Shakes gently: heavy weapons and soft cloaks make no sound for her."),
    portrait("hob", "Hob the Farmer", "stall",
             "A broad, sunburnt farmer in his fifties who ploughs chests up out of the old battlefield. A frayed straw hat, a "
             "ruddy weathered face, bushy grey-brown eyebrows, grey stubble, a shrewd haggler's half-smile with a stalk of "
             "wheat in the corner of his mouth. A brown work smock over a homespun shirt, a clod of earth on one shoulder.",
             ["the straw hat", "ruddy cheeks and stubble", "the wheat stalk in his mouth", "a shrewd half-smile"],
             ["skin:2-4", "gold:3-5", "umber:2-5"],
             "Tier 1. Shakes hard (a clank drowns out the coin for him) and haggles."),
    portrait("mags", "Mags the Innkeeper", "stall",
             "The hearty innkeeper of the Crossed Keys inn, in her forties. Copper-red hair pinned up with loose curls, rosy "
             "freckled cheeks, a wide laughing mouth. A green bodice over a white blouse with puffed sleeves, a cream apron "
             "with two small crossed keys embroidered on its bib.",
             ["piled-up red hair", "a big warm laugh", "the crossed keys on her apron", "green bodice"],
             ["skin:3-5", "flame:1-2", "leaf:2-4", "parchment:2-3"],
             "Tier 1. Takes chests for unpaid tabs; counts the rattles and charges the same for each."),
    portrait("dorrin", "Dorrin the Delver", "stall",
             "A young adventurer of about twenty, just back from the Barrow. Tousled blond hair under a scorched leather "
             "headband, smudges of grave dirt on his cheeks, an eager grin with a chipped tooth, a cobweb on one shoulder. A "
             "battered brown leather jerkin with a small brass lantern clipped to the strap.",
             ["blond hair and a leather headband", "dirt smudges", "a cobweb on the shoulder", "the little lantern clip"],
             ["skin:3-5", "gold:4-5", "umber:2-5", "night:5-7"],
             "Tier 1. His rotten Barrow chests stand about two seconds of shaking."),
    # ---- Thornwick Market, at the edge of the Old Wood (tier 2) ----
    portrait("snikkit", "Snikkit the Goblin", "thornwick",
             "A wiry goblin from the hills. Leaf-green skin, long pointed ears sticking out sideways, big yellow eyes with "
             "slit pupils, a sly grin full of sharp teeth with one gold tooth. A ragged brown hood pushed back over a "
             "patched tunic, a strap of a sack over his shoulder.",
             ["green skin and huge pointed ears", "yellow eyes", "a sly toothy grin with a gold tooth", "ragged brown hood"],
             ["leaf:2-5", "gold:4-5", "umber:1-4"],
             "Tier 2. Sells what his cousins pinch; charges a tenth more for each shake strength after your first."),
    portrait("wenna", "Wenna the Huntress", "thornwick",
             "A huntress in her thirties who finds chests in outlaws' camps and wolves' dens. Tanned face with a pale scar "
             "across one cheek, an auburn braid over her shoulder, calm steady eyes. A deep green hood and a leather jerkin, "
             "a red feather tucked into the hood, a bowstring across her chest.",
             ["green hood with a red feather", "a scar on her cheek", "auburn braid", "the bowstring across her chest"],
             ["skin:2-4", "leaf:1-3", "umber:2-5", "crimson:3-4"],
             "Tier 2. Weighs every chest on her game scale; her chests hang on a skittish mare."),
    portrait("bramble", "Mother Bramble", "thornwick",
             "An ancient druid of the Old Wood. A wild cloud of white hair woven with twigs, ivy and small leaves, a deeply "
             "lined, kind face, eyes half-closed as if still dreaming, a faint serene smile. A heavy mossy green-brown cloak "
             "with a sprig of holly (red berries) at the collar.",
             ["wild white hair full of twigs and leaves", "dreaming half-closed eyes", "mossy cloak",
              "holly with red berries"],
             ["night:7-10", "skin:3-5", "moss:1-3", "leaf:2-4", "crimson:4"],
             "Tier 2. Dreams her price, and always dreams true, as a sign: a magpie, a thorn, a raven's wing, a river, an "
             "owl, a star."),
    portrait("ansel", "Brother Ansel", "thornwick",
             "A young monk in his early twenties selling the abbey crypt's chests to mend the abbey roof. Tonsured brown "
             "hair, a round earnest face, shy hopeful eyes, a small smile. A brown wool habit with a hood around the "
             "shoulders and a rope cord, a small wooden key-shaped holy symbol on a cord at his chest.",
             ["the tonsure", "brown habit and rope cord", "the wooden key-shaped symbol", "shy hopeful look"],
             ["skin:3-5", "umber:2-5", "parchment:1-2"],
             "Tier 2. Won't have the saint's things shaken hard."),
    portrait("fennick", "Fennick the Smuggler", "thornwick",
             "A lean smuggler in his forties who runs brandy past the Wardens' toll on the forest road. A black "
             "wide-brimmed hat pulled low, a thin black moustache and stubble, a knowing sideways smirk, a gold hoop "
             "earring. A dark coat with the collar turned up, wisps of straw stuck to it (he packs his chests in straw).",
             ["black wide-brimmed hat", "thin moustache and a smirk", "gold earring", "straw stuck to his coat"],
             ["skin:2-4", "night:1-4", "gold:3-5", "parchment:2"],
             "Tier 2. Packs in straw, so everything stirs a strength later; often a false bottom."),
    portrait("gerda", "Gerda the Matchmaker", "thornwick",
             "Thornwick's plump, grandmotherly matchmaker in her sixties, selling dowries from weddings that never happened. "
             "Silver hair under a white lace cap, rosy round cheeks, one knowing eyebrow raised, a matchmaker's sly smile. "
             "A red embroidered Thornwick bodice over a white blouse, a tiny posy of dried wedding flowers pinned to it.",
             ["white lace cap", "rosy cheeks and a raised eyebrow", "red embroidered bodice", "the dried posy"],
             ["skin:3-5", "night:8-9", "crimson:2-4", "rose:2-3"],
             "Tier 2. A Thornwick dowry always holds coin, a cloak and a potion."),
    # ---- Crownhold, the capital (tier 3) ----
    portrait("wren", "Wren the Apprentice", "crownhold",
             "An archmage's apprentice of about seventeen clearing the Arcanum stores. A copper-red bob haircut, round "
             "spectacles slipping down the nose, an ink smudge on one cheek, an eager flustered look with an open mouth. A "
             "blue apprentice's robe with a high collar, a quill behind one ear and a paper packing tag on a string around "
             "the neck.",
             ["round spectacles", "copper-red bob", "ink smudge", "a packing tag on a string", "a quill behind the ear"],
             ["skin:3-5", "flame:1-2", "azure:2-4", "parchment:2-3"],
             "Tier 3. Every chest has its packing tag listing each kind packed, warded or not."),
    portrait("orrin", "Archmage Orrin", "crownhold",
             "An archmage of the Arcanum, old enough to remember the Locking. A long white beard, bushy white eyebrows, "
             "twinkling knowing eyes and a hint of a smile, a tall pointed indigo hat with small silver stars, deep indigo "
             "robes with a silver star clasp.",
             ["tall pointed indigo hat with stars", "long white beard", "twinkling eyes", "silver star clasp"],
             ["arcane:1-3", "night:7-10", "skin:3-4", "steel:3-4"],
             "Tier 3. Wagers: read his chest exactly right and he returns a third of your price."),
    portrait("grit", "Grit the Miner", "crownhold",
             "A dwarf of the Ironroot hold. A huge braided orange beard bound with iron beads, a broad soot-smudged nose, "
             "small fierce eyes under thick brows, brass mining goggles pushed up on a dented iron helmet with a candle "
             "stub on top, a heavy leather apron over chain links.",
             ["the huge braided orange beard with iron beads", "goggles on a dented iron helmet", "the candle stub",
              "soot on the nose"],
             ["flame:1-3", "umber:1-4", "steel:1-3", "gold:3-4"],
             "Tier 3. Knows weapons; takes every tinkle for a diamond. Haggles."),
    portrait("corisande", "Lady Corisande", "crownhold",
             "A poised noblewoman in her forties selling her grandmother's trick boxes. Auburn hair in an elegant updo with "
             "a pearl pin, a pale composed face, one eyebrow raised in cool appraisal, a faint smile. A lavender silk gown "
             "with a high lace collar and a cameo brooch.",
             ["auburn updo with a pearl pin", "lace collar and cameo brooch", "cool appraising look", "lavender silk"],
             ["skin:3-5", "flame:1", "arcane:3-6", "night:8-10"],
             "Tier 3. Grandmother's riddle on each lid is about one thing inside."),
    portrait("pell", "Pell the Pawnbroker", "crownhold",
             "An elderly pawnbroker. Thinning grey hair combed flat, a sharp nose, a gold-rimmed monocle on a fine chain in "
             "one eye, a dry precise expression. An olive waistcoat over a white shirt with sleeve garters, a pawn ticket "
             "tucked into his waistcoat pocket.",
             ["the gold monocle and chain", "sharp nose", "olive waistcoat", "a ticket in his pocket"],
             ["skin:3-5", "night:6-8", "moss:1-3", "gold:4-5"],
             "Tier 3. His pawn ticket counts every article pledged, false bottom and all."),
    portrait("quill", "Quill the Archivist", "crownhold",
             "The Royal Archive's clerk, in his thirties. Neat dark hair parted in the middle, round wire glasses, a pale "
             "bookish face with a slightly anxious expression. An ink-blue scholar's robe with a white collar band, a long "
             "quill pen behind one ear and a rolled ward-roll under his arm.",
             ["round wire glasses", "a long quill behind the ear", "ink-blue robe", "a rolled parchment under his arm"],
             ["skin:3-5", "azure:1-3", "night:1-3", "parchment:2-3"],
             "Tier 3. The ward-roll names the warded thing, every other letter faded."),
    portrait("shelly", "Shelly the Mudlark", "crownhold",
             "A young mudlark of about sixteen who combs the Aldwater mud. Tangled blonde hair under a faded blue headscarf, "
             "mud smudges on her cheeks and nose, a cheeky grin. A patched brown smock and a string of small river shells "
             "around her neck.",
             ["blue headscarf over tangled blonde hair", "mud smudges", "a necklace of shells", "a cheeky grin"],
             ["skin:3-5", "gold:4-5", "azure:2-3", "umber:2-4", "parchment:3"],
             "Tier 3. Prices every rustle as a drowned wizard's spellbook; her scroll cases are often full of sand."),
    portrait("aldous", "Sir Aldous", "crownhold",
             "An ageing knight looking for his grandfather's sword. A steel helm with the visor raised and a red plume, a "
             "grey moustache, tired kind eyes, an earnest dignified expression. Steel pauldrons over a white tabard with a "
             "red emblem.",
             ["steel helm with a red plume, visor up", "grey moustache", "steel pauldrons", "white tabard with a red emblem"],
             ["steel:1-4", "night:8-10", "crimson:3-4", "skin:3-4"],
             "Tier 3. If there are weapons inside, the best is his, for a price he names."),
    # ---- The Royal Emporium, by the Gilded Gate (tier 4) ----
    portrait("vexis", "Vexis the Imp", "emporium",
             "A small, impatient imp. Crimson skin, two curved ivory horns, tufted pointed ears, yellow slit eyes, a toothy "
             "smirk, and a little gold hourglass hanging on a chain at its chest, sand running.",
             ["crimson skin and curved ivory horns", "yellow slit eyes", "the gold hourglass pendant", "toothy smirk"],
             ["crimson:2-5", "parchment:2-3", "gold:3-6"],
             "Tier 4. Gives you twelve seconds."),
    portrait("malbranche", "Malbranche", "emporium",
             "A skeletal devil who deals in bargains. A horned skull with tiny ember-red eyes deep in the sockets and a "
             "lipless grin, a high-collared black cloak trimmed with gold, a gold medallion shaped like a wax contract seal "
             "at the throat. Elegant and sinister.",
             ["a horned skull", "ember-red eyes in the sockets", "high black collar with gold trim", "the gold seal medallion"],
             ["parchment:1-3", "night:1-3", "gold:3-5", "flame:2"],
             "Tier 4. Cheap, but the dearest thing inside is his."),
    portrait("skrae", "Skrae", "emporium",
             "A demon with ears like a bat, listening. Dark violet leathery skin, two enormous ribbed bat ears spreading "
             "wide, small glowing red eyes, a little fanged mouth, head tilted as if catching an echo.",
             ["huge bat ears", "dark violet skin", "small red glowing eyes", "a fanged mouth"],
             ["arcane:1-3", "night:1-4", "crimson:4-5"],
             "Tier 4. Prices what it hears, fairly, and clicks to hear the echoes off everything inside.",
             view=FRONTAL),
    portrait("ermine", "Lady Ermine", "emporium",
             "A haughty lady of the court. A pale powdered face with a beauty mark, a tall white powdered wig with a "
             "jewelled pin, half-lidded disdainful eyes, rouged lips. An ermine stole (white fur with small black tips) "
             "over a violet gown, a large gem pendant.",
             ["tall white powdered wig", "the ermine stole with black tips", "a beauty mark", "disdainful half-lidded eyes"],
             ["night:7-10", "skin:4-5", "arcane:2-4", "rose:2-3"],
             "Tier 4. Knows only her own gems; some are paste. No lodestone near her gems."),
    portrait("oriel", "Madame Oriel", "emporium",
             "The court's cartomancer. Dark curly hair under a purple turban with a jewel at the front, gold hoop earrings, "
             "kohl-lined eyes and an enigmatic smile. A fan of three cards held up beside her face, their backs showing a "
             "simple star pattern (no letters or symbols).",
             ["purple turban with a jewel", "gold hoop earrings", "kohl-lined eyes", "three cards fanned beside her face"],
             ["skin:2-4", "arcane:2-5", "gold:4-5", "night:1-3"],
             "Tier 4. Draws three cards from the Locking's own deck: a suit drawn twice is what the ward holds."),
    portrait("silas", "Silas Gilt", "emporium",
             "A fence for the Maw. Slick black hair, a thin moustache, a too-wide smile with a gold tooth, shifty eyes. A "
             "purple velvet coat hung with gaudy gold chains, rings on every finger of the hand at his lapel.",
             ["slick black hair and thin moustache", "gold tooth in a too-wide smile", "gaudy gold chains",
              "purple velvet coat"],
             ["skin:3-5", "night:1-3", "arcane:2-4", "gold:3-6"],
             "Tier 4. Sells demon gold as gold, cheap."),
    portrait("aldo", "Old Aldo", "emporium",
             "A tired old man in a plain grey wool cloak with the hood pushed back: secretly King Aldric IX. A white beard, "
             "kind weary eyes, a thoughtful expression, one hand at his chin wearing a very fine gold signet ring with a "
             "ruby, the only rich thing about him.",
             ["plain grey hooded cloak", "white beard", "kind weary eyes", "the fine ruby signet ring on his hand"],
             ["night:4-8", "skin:3-5", "gold:4-6", "crimson:4"],
             "Tier 4. His price is your slip: read it exactly and he takes a quarter off."),
    portrait("maw", "Avarax, the Gilded Maw", "emporium",
             "The demon of greed in a borrowed body: a face like a mask of molten gold, gold dripping from the jaw, a crown "
             "of jagged gold spikes, glowing red eyes, a wide grin of gold teeth, layers of gold chains, rings and jewels "
             "piled on the shoulders. Overwhelming, gaudy, menacing.",
             ["a molten-gold face dripping gold", "a crown of gold spikes", "glowing red eyes", "a grin of gold teeth",
              "chains and jewels piled on the shoulders"],
             ["gold:1-6", "crimson:3-6", "night:1-2"],
             "Tier 4, rarely. Sells a chest for one coin, and everyone after him that day asks twice as much.",
             view=FRONTAL),
    # ---- The story's people ----
    portrait("bram", "Uncle Bram", "kin",
             "The old Keywright, your mentor, in his late sixties. A warm round face, crinkled smiling eyes, bushy white "
             "eyebrows, a white moustache and a neatly trimmed white beard, a red felt cap. A red waistcoat over a cream "
             "shirt. Proud and kindly, a little ashamed of the debt he's left you.",
             ["red felt cap", "white moustache and trimmed beard", "crinkled smiling eyes", "red waistcoat over cream shirt"],
             ["crimson:2-4", "night:8-10", "skin:3-5", "parchment:2-3"],
             "The mentor: his tips run the tutorial. The first portrait to make: the style anchor for every face.",
             anchor=True),
    portrait("vane", "Mister Vane", "story",
             "Collector of the Gilded Guild. Pale and gaunt, slicked-back black hair, cold grey eyes, a thin polite smile. "
             "A high-collared black coat with gold buttons, the Guild's emblem pinned at the collar: a small gold coin "
             "stamped with a watching eye above a keyhole. Polite, patient and quietly menacing.",
             ["pale gaunt face", "slicked-back black hair", "high black collar", "the gold eye-and-keyhole pin"],
             ["night:1-4", "steel:3-4", "gold:3-5", "skin:4-5"],
             "Comes on the Guild's nights for the tithe. He's the Maw's."),
    portrait("hood", "The Hooded Woman", "story",
             "A mysterious woman in a deep violet hooded cloak, her face almost hidden in the hood's shadow: only the line "
             "of a jaw and two faint violet-glowing eyes. A silver clasp shaped like a key at her throat.",
             ["deep violet hood", "face in shadow", "two faint violet eyes", "the silver key clasp"],
             ["arcane:1-4", "night:1-3", "steel:3-4"],
             "Mara, the last archmage of the Keyhold, before she says so. Trades the Aldric Key's shards for things only a "
             "Keywright's chests hold.",
             view=FRONTAL),
    portrait("nim", "Nim the Peddler", "story",
             "A horned, cheerful half-something who sells curios. Warm brown skin, two short curved horns, pointed ears, "
             "golden eyes, a wide friendly grin. A patched red and mustard coat with many pockets, odd trinkets dangling "
             "from the lapels (a tiny key, a bell, a bead). Nobody asks what Nim is.",
             ["two short curved horns", "a wide friendly grin", "golden eyes", "a patched coat hung with trinkets"],
             ["skin:2-4", "crimson:3-4", "gold:3-5", "parchment:1-2"],
             "Comes twice a week with curios: oddments from the Locking that bend a rule of the trade."),
    portrait("stranger", "The Stranger", "story",
             "A silent hooded figure in black. The face is entirely shadow except a faint glint where the eyes should be. "
             "A heavy iron chain is slung over one shoulder.",
             ["a black hood with no face visible", "a faint glint of eyes", "a heavy iron chain over one shoulder"],
             ["night:1-5", "steel:1-3"],
             "Brings a chest wrapped in iron chains, so nothing inside can move. Never says where from.",
             view=FRONTAL),
    # ---- Bram's kin: who minds the stall ----
    portrait("kit", "Kit", "kin",
             "Bram's niece, about eighteen, who grew up behind his counter: the Ear. Brown hair in a short ponytail under a "
             "red headband, freckles, bright attentive eyes, head slightly tilted as if listening, a small confident smile. "
             "A green tunic with a leather strap.",
             ["red headband", "brown ponytail", "freckles", "attentive, listening tilt of the head", "green tunic"],
             ["skin:3-5", "umber:2-4", "crimson:3-4", "leaf:2-4"],
             "The first character: shakes at every strength and drops the chest to hear what's under the floor."),
    portrait("hesk", "Hesk", "kin",
             "Bram's nephew, a grocer from the river towns, in his forties: the Scale. Stocky, a full brown beard, a flat "
             "green cap, an honest patient face with a slight wince from his bad back. A grocer's cream apron with a small "
             "brass balance-scale pin.",
             ["flat green cap", "full brown beard", "cream grocer's apron", "brass scale pin", "honest, patient face"],
             ["skin:3-4", "umber:2-4", "leaf:2-3", "gold:4"],
             "Can't shake hard or drop (a bad back); knows what everything weighs."),
    portrait("rosa", "Rosa", "kin",
             "Bram's niece from the Crownhold markets, early twenties: the Tongue. Fiery orange hair in a loose ponytail, a "
             "confident smirk, one eyebrow raised, sharp green eyes. A green market-trader's vest over a white shirt and a "
             "red neckerchief.",
             ["fiery orange hair", "a confident smirk and raised eyebrow", "red neckerchief", "green trader's vest"],
             ["skin:3-5", "flame:1-3", "leaf:2-4", "crimson:3-4"],
             "Everybody haggles with Rosa. She'd haggle with a statue, and win."),
    portrait("isolde", "Isolde", "kin",
             "Bram's niece by marriage, a lady who has never sold a thing she liked: the Collection. Elegant blonde hair in "
             "a braided crown, a calm proud gaze, a slight smile. A wine-red gown with a gold embroidered collar and an "
             "antique jewelled brooch.",
             ["blonde braided crown", "wine-red gown with gold collar", "an antique brooch", "calm, proud gaze"],
             ["skin:4-5", "gold:4-6", "crimson:2-4"],
             "Has a shelf from day one and charges visitors to see her collection."),
    # ---- Optional, new: faces the story would use ----
    portrait("mara", "Mara, unhooded", "story",
             "The last archmage of the Keyhold, around sixty, after she tells you her name: cropped silver hair, sharp violet "
             "eyes, a weathered face with a fine pale scar along one cheekbone, a resolute expression. The violet hood lies "
             "down around her shoulders, a silver key-shaped clasp at the throat.",
             ["cropped silver hair", "sharp violet eyes", "violet hood down on her shoulders", "the silver key clasp"],
             ["arcane:1-5", "night:6-9", "steel:3-4", "skin:3-4"],
             "New (P3): for the Hooded Woman after week 4's reveal. Integration: use it in scenes and the ledger once she's "
             "Mara.", priority="P3", replaces=False),
    portrait("king", "King Aldric IX", "emporium",
             "The same tired old man as Old Aldo, now as the King: a slim gold crown, the white beard, the kind weary eyes, a "
             "royal crimson mantle with an ermine collar, the ruby signet ring.",
             ["the same face as Old Aldo", "a slim gold crown", "crimson mantle with ermine collar", "ruby signet ring"],
             ["crimson:2-4", "night:8-10", "gold:4-6", "skin:3-5"],
             "New (P3): for the Crown Chest's scenes. Draw it after Old Aldo and pass that portrait as a reference.",
             priority="P3", replaces=False),
]
