"""
The shop scene's layers (round 2; art/tools/scene.py says where each sits): the skies (one per time of day, shared), and
per shop tier the place across the counter, the shop's frame, its display shelf, its counter, its appraisal sheet and
the lanterns on its posts; and the props that stand on the counter once you've bought them.
"""

ASSETS = []


def add(**asset):
    asset.setdefault("details", [])
    asset.setdefault("avoid", [])
    ASSETS.append(asset)
    return asset


PLACES = {1: "the Brindle crossroads (Bram's Stall)", 2: "Thornwick Market", 3: "Crownhold", 4: "the Royal Emporium"}
KEYS = {1: "stall", 2: "thornwick", 3: "crownhold", 4: "emporium"}

# ---- The sky: the time of day, behind every place --------------------------------------------------------------------

SKIES = [
    ("morning", "Early morning", "P1",
     "Pale clear blue at the top, through a soft lemon to a warm peach band low down; a low, pale-gold sun near the left "
     "edge, half a disc; one long, thin, pale cloud.",
     ["azure:5-6", "parchment:3", "rose:3", "gold:6"]),
    ("midday", "Midday", "P1",
     "Clear soft blue, a little paler towards the bottom; two or three plain, rounded white clouds; no sun in view.",
     ["azure:4-6", "night:9-10"]),
    ("evening", "Evening", "P1",
     "Deep violet at the top, through rose to a warm orange band low down; the sun low near the right edge, a half disc of "
     "pale gold; one long cloud lit rose underneath.",
     ["arcane:2-3", "rose:1-3", "flame:2-4", "gold:5"]),
    ("night", "Night", "P2",
     "Deep blue at the top, a little lighter towards the bottom; a pale full moon high on the left; a few small single "
     "stars, far apart.",
     ["azure:1-2", "night:1-3", "steel:4", "night:9"]),
]
for key, title, priority, description, palette in SKIES:
    add(id=f"sky.{key}", category="sky", title=f"{title} sky", time=key, priority=priority,
        anchor_for="sky" if key == "midday" else None, replaces=None, description=description, palette=palette,
        context=f"Behind every place: the game shows it {'(and tints the scene to match) ' if key != 'midday' else ''}"
                f"{'for the first customers of a day' if key == 'morning' else 'through the middle of a day' if key == 'midday' else 'for the last customers and the day end' if key == 'evening' else 'on the Guild night'}.")

# ---- The place: the view across the counter, the sky keyed out ---------------------------------------------------------

add(id="place.stall", category="place", title="The Brindle crossroads, from Bram's Stall", tier=1, anchor_for="place",
    priority="P1", replaces="assets/backgrounds/shop1.png",
    description=(
        "The Brindle crossroads seen from a roadside stall: a packed-earth road running straight away from the viewer to "
        "the horizon, and another crossing it in the middle distance; a plain wooden signpost with two blank arms at the "
        "crossing, a little right of centre; low rolling hills along the horizon in two flat tones, with the small "
        "silhouette of a castle on a far hill right of centre; a calm river as one flat band on the far right, with a "
        "small watermill (a plain wooden house and its wheel); two or three round trees. The bottom third is plain grass "
        "and earth."),
    details=["the road running away to the horizon", "the signpost at the crossing", "a castle's small silhouette on a far hill",
             "the watermill on the right"],
    palette=["leaf:2-4", "moss:2-3", "umber:3-5", "sea:2-3", "night:4-5", "parchment:2"],
    context="Bram's Stall, tier 1: behind the seller and the chest all day.")

add(id="place.thornwick", category="place", title="Thornwick Market, from the shop", tier=2, priority="P1",
    replaces="assets/backgrounds/shop2.png",
    description=(
        "Inside a small stone shop on Thornwick's market square, looking toward its front: a wall of big grey stone blocks "
        "(a few large flat shapes with thin mortar lines), and in the middle of it a wide open doorway, its heavy wooden "
        "door folded back, through which the square shows: the striped awning of one stall, a stone well, and the dark "
        "pines of the Old Wood beyond; on the right, a small window of four panes. A plain flagstone floor."),
    details=["the stone wall as big flat blocks", "the open doorway onto the square", "the dark pines beyond"],
    palette=["night:3-6", "umber:2-5", "leaf:1-3", "steel:1-2", "gold:4"],
    context="Thornwick Market, tier 2.")

add(id="place.crownhold", category="place", title="Crownhold, from the shop", tier=3, priority="P1",
    replaces="assets/backgrounds/shop3.png",
    description=(
        "A merchant's shop in Crownhold, the capital, looking toward its front: warm oak panelling in a few tall plain "
        "panels; in the middle, a wide display window of six panes onto a cobbled street, with a simple skyline of roofs "
        "and two spires beyond in one or two dark tones; on the right, the shop's door, closed, with a small brass bell "
        "above it. A plain wooden floor."),
    details=["the oak panelling", "the wide window onto roofs and spires", "the door with its bell"],
    palette=["umber:1-5", "azure:1-3", "steel:1-3", "gold:4-5", "night:3-6"],
    context="Crownhold, tier 3.")

add(id="place.emporium", category="place", title="The Royal Emporium, from the counter", tier=4, priority="P1",
    replaces="assets/backgrounds/shop4.png",
    description=(
        "A grand hall of the Royal Emporium, looking toward its entrance: two white marble columns and crimson drapes at "
        "the sides; in the middle, a tall open archway onto a paved avenue, and at the far end of the avenue, small and "
        "soft, the Gilded Gate: a tall arched gate of gold bars with a faint flat halo of sickly pale gold around it (the "
        "one thing that feels wrong). A polished floor of large pale tiles."),
    details=["the marble columns and crimson drapes", "the archway onto the avenue", "the Gilded Gate, small and faintly sickly gold"],
    palette=["crimson:2-4", "gold:3-5", "night:5-9", "steel:3-4", "moss:3"],
    context="The Royal Emporium by the Gilded Gate, tier 4.")

# ---- The shop's frame: the top (the top bar sits on it) and the side posts ---------------------------------------------

FRAMES = {
    1: ("the front edge of the stall's canvas awning: wide, even red-and-cream stripes ending in a scalloped edge",
        "two plain wooden posts holding the awning up", "the awning's underside, in shadow",
        ["crimson:2-3", "parchment:2-3", "umber:2-4"]),
    2: ("a heavy dark timber beam, with a plain iron hook under each end", "two square timber posts",
        "the beam's face, dark and plain", ["umber:1-4", "night:2-4", "steel:1-2"]),
    3: ("an oak lintel with one plain moulding along it", "two panelled oak pilasters with small brass caps",
        "the lintel's plain frieze", ["umber:1-5", "gold:3-4"]),
    4: ("a crimson velvet valance with a thin gold fringe", "two white marble columns with plain gold capitals",
        "the valance's top, plain dark velvet", ["crimson:1-3", "gold:3-5", "night:7-9"]),
}
for tier, (top, posts, band, palette) in FRAMES.items():
    add(id=f"frame.{KEYS[tier]}", category="frame", title=f"The shop's frame at {PLACES[tier]}", tier=tier,
        anchor_for="frame" if tier == 1 else None, priority="P1", replaces=None,
        subject=top, frame_fields={"posts": posts, "band": band},
        description=f"Across the top, {top}; at the sides, {posts}.",
        details=[top.split(":")[0], posts], palette=palette,
        context="In front of the place and the seller: frames the view, carries the top bar (day, debt, purse) on its "
                "plain top band, and holds the display shelf and the lanterns.")

# ---- The display shelf, hung from the frame: kept things stand on it ---------------------------------------------------

SHELVES = {
    1: ("a plain plank", "two ropes", ["umber:3-5", "parchment:1-2"]),
    2: ("a long oak shelf", "two black iron chains", ["umber:2-5", "night:2-4"]),
    3: ("a walnut shelf with a thin brass rail along its front", "two brass rods", ["umber:1-4", "gold:3-5"]),
    4: ("a long glass shelf in a slim gilded frame", "two gilded chains", ["steel:3-4", "azure:5-6", "gold:3-5"]),
}
for tier, (board, supports, palette) in SHELVES.items():
    add(id=f"shelf.{KEYS[tier]}", category="shelf", title=f"Display shelf at {PLACES[tier]}", tier=tier,
        anchor_for="shelf" if tier == 1 else None, priority="P1", replaces=None,
        subject=board, frame_fields={"supports": supports},
        description=f"{board[0].upper() + board[1:]}, hung on {supports}.", details=[board, supports], palette=palette,
        context="Where kept things stand (the game puts them on its top surface) and Heartsongs hang above. Shown once "
                "the shop has a display (Thornwick, or Isolde from day one).")

# ---- The counter's top: the bottom third of the screen -----------------------------------------------------------------

COUNTERS = {
    1: ("a trestle table top of three or four wide, rough planks, grey-brown, running left to right", ["umber:2-4", "night:3-5"]),
    2: ("a solid oak counter top of broad planks running left to right, honey-brown", ["umber:2-5"]),
    3: ("a polished walnut counter top with a thin brass strip along its far edge", ["umber:1-4", "gold:4-5"]),
    4: ("a polished counter top of deep green-black marble with a few soft paler veins, and a thin gold strip along its far "
        "edge", ["sea:1-2", "night:2-4", "gold:4-5"]),
}
for tier, (description, palette) in COUNTERS.items():
    add(id=f"counter.{KEYS[tier]}", category="counter", title=f"Counter at {PLACES[tier]}", tier=tier,
        anchor_for="counter" if tier == 1 else None, priority="P1", replaces=None,
        description=description[0].upper() + description[1:] + ".", details=[description.split(",")[0]], palette=palette,
        context="The chest stands on its far edge; the notes, the commission board, the waiting queue, curios and Bram's "
                "tips sit on it, so it stays plain and mid-dark.")

# ---- The appraisal sheet: behind the slip, grander at every move -------------------------------------------------------

SHEETS = {
    1: ("a sheet of rough, pale unbleached paper, its edges a little uneven", "a rough plank board showing as a narrow rim "
        "around it", "two round nail heads in the top corners", ["parchment:2-3", "umber:2-4", "night:4"]),
    2: ("a cream sheet", "a wooden clipboard showing as a narrow rim", "a small brass clip in the top-left corner",
        ["parchment:2-3", "umber:3-5", "gold:4-5"]),
    3: ("a fine white ledger page", "a dark green leather folder showing as a narrow rim",
        "small gold corner pieces in all four corners", ["parchment:3", "night:9", "leaf:1-2", "gold:4-5"]),
    4: ("an ivory vellum sheet with a thin gold line inside its edge", "a slim gilded frame",
        "a small crimson wax seal in the bottom-right corner", ["parchment:3", "night:9", "gold:3-5", "crimson:3-4"]),
}
for tier, (paper, rim, ornament, palette) in SHEETS.items():
    add(id=f"sheet.{KEYS[tier]}", category="sheet", title=f"Appraisal sheet at {PLACES[tier]}", tier=tier,
        anchor_for="sheet" if tier == 1 else None, priority="P1", replaces=None,
        subject=paper, frame_fields={"rim": rim, "ornament": ornament},
        description=f"{paper[0].upper() + paper[1:]}, on {rim}; {ornament}.", details=[paper, ornament], palette=palette,
        context="Behind the appraisal slip (the right third of the counter screen): the game prints the slip's rows, "
                "totals and buttons on it in dark ink, so the paper's middle stays plain.")

# ---- Lanterns on the frame's posts (lit; the game makes the unlit one) -------------------------------------------------

LANTERNS = {
    1: "a small tin lantern with a ring on top, its glass glowing warm yellow",
    2: "a black iron lantern with four small glass panes glowing warm yellow",
    3: "a brass lamp with a round glass globe glowing warm yellow",
    4: "a gilded lantern with a clear crystal globe glowing warm yellow",
}
for tier, description in LANTERNS.items():
    add(id=f"prop.lantern_{KEYS[tier]}", category="prop", title=f"Lantern at {PLACES[tier]}", tier=tier, size=[16, 24],
        anchor_for="prop" if tier == 1 else None, priority="P1", replaces=None, hangs=True,
        description=f"{description[0].upper() + description[1:]}, hanging from a short hook.", details=[description.split(",")[0]],
        palette=["night:2-4", "gold:4-6", "flame:3-4"] + (["steel:2-3"] if tier < 3 else ["gold:3"]),
        context="Hung on the frame's posts; lit in the evening and on the Guild night (the game makes the unlit one by "
                "darkening the glass, and draws the glow).")

# ---- What you've bought, on the counter beside the chest ---------------------------------------------------------------

PROPS = [
    ("witchlight", "The Witchlight", "lamp", [16, 24], "P1",
     "A small brass lantern whose flame burns violet.", ["gold:3-5", "arcane:3-5", "night:2-3"]),
    ("lodestone", "The Lodestone", "magnet", [20, 16], "P1",
     "A dark, lumpy grey-black stone sitting in a little brass cradle.", ["night:2-5", "gold:3-5"]),
    ("scales", "Brass Scales", "scales", [24, 24], "P1",
     "Brass balance scales with two shallow pans on a short post.", ["gold:2-5", "umber:2-3"]),
    ("gavel", "The auctioneer's gavel", "block", [22, 12], "P2",
     "A wooden gavel lying on a round wooden sound block.", ["umber:2-5"]),
    ("seal", "The Royal Seal", "seal", [12, 20], "P2",
     "A gold seal stamp with a crimson handle, standing upright.", ["gold:3-5", "crimson:2-4"]),
    ("book", "The Patrons' Book", "patrons", [22, 14], "P2",
     "A thick closed book bound in green leather with a gold clasp.", ["leaf:1-3", "gold:4-5", "parchment:2"]),
    ("picks", "Fine Lockpicks", "picks", [22, 10], "P2",
     "A rolled leather case of lockpicks, tied with a strap.", ["umber:2-4", "steel:2-3"]),
]
for key, title, upgrade, size, priority, description, palette in PROPS:
    add(id=f"prop.{key}", category="prop", title=title, size=size, priority=priority, replaces=None, upgrade=upgrade,
        description=description, details=[description.split(",")[0].rstrip(".")], palette=palette,
        context=f"Stands on the counter beside the chest once you own the {title.replace('The ', '')} (upgrade `{upgrade}`).")
