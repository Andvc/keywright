# Keywright design notes

## The game in one paragraph

Three hundred years ago the Locking shut every treasure in the realm of Aldmark into a warded chest, to starve the
demon Avarax, the Gilded Maw. The chests are still everywhere, and only a Keywright can open one, once it's rightfully
theirs: everyone else sells them unopened, and the ward lets nothing through but sound (the whole world is in
`docs/world.md`). You're one of Bram's kin, with the Key-mark, and Bram hands you his stall at the Brindle crossroads.
People bring you chests and price them their own way: some shake them and price what they heard, some go by the
weight or the chest's colour, some go by a trade they know, some dream up a number, and each has a rule of their own (a rotten
chest, a fee for every shake, a wager, a clock, a claim on what's inside). They tell you where the chest came from, and
not all of them tell the truth. Shake each one yourself (gently, firmly, hard), drop it to listen for a false bottom,
count the rattles, and work out what they missed, made up or misjudged. Mark what you think is inside on your
appraisal slip and trade the way each customer trades: pay their price, haggle, or name your own. Pass, and your gut
tells you whether you were right; buy, and the chest bursts open on the spot: each thing's true quality, a tick or a
cross against your slip, and now and then something far better than the chest promised. What you acquire builds the
shop's name (synergies) and its exhibition, which draw more and better customers, and the shop moves inward, from the
crossroads to Thornwick Market, Crownhold and the Royal Emporium by the Gilded Gate. The stall comes with Bram's twenty
years of unpaid tithe to the Gilded Guild: every fifth evening Mister Vane collects a due that climbs from 20g to
45,000g over five weeks (25 days, an hour and a half to two hours of play). Curios from Nim the peddler are the run's
build: each bends a rule of the trade, the ones that share a way of trading (enchantment, sight unseen, haggling, the
perfect read, false bottoms, wards, and more beside) make each other stronger, and Nim, who notices what you buy,
brings more of it. Whatever makes ordinary things fetch more puts up what every seller asks as well (the shop's power
index), so the shop's growth makes the numbers bigger without making a bad buy good; the last week's due is more than
ordinary trade can make, and a build that has come together is what carries it. Regulars pay well for commissions,
something different happens in town two days a week, and a hooded woman trades shards of the Aldric Key for the
finds only a good ear turns up. Pay all five dues, or the whole of what's left at once whenever you have it, and
the stall is free (and with the Key whole, the Crown Chest opens on your counter and the Locking holds); fall short and
the Guild seals the shop.

Across runs, four of Bram's kin can mind the stall, each reading chests or running the shop their own way: Kit listens,
Hesk weighs, Rosa haggles, Isolde collects. Clearing the debt with one opens the next of eight challenge levels for
them, each adding one of the Guild's harsher terms. Every run ends with a score; times the challenge level's
multiplier, it's renown, which buys talents that come along on every run.

## The world

`docs/world.md` is the reference for anything that speaks, sells, falls out of a chest or happens in a scene: why the
world is full of chests (the Locking), why people sell them unopened (only a Keywright can open one, once it's theirs),
why you can (the Key-mark), and what opening them does (coin for the realm, treasure back in the light, and every chest
a little looser on the Maw's chains, which is why the Guild wants you opening them). Every rule has a reason you could
tell a stranger in a tavern: the ward lets only sound through, so everybody shakes and guesses; the old iron lock under
the ward still needs picking; people hid their best things under a false bottom when the Wardens came round; demon gold
is iron under the gilt, so the lodestone silences it.

The shop's four tiers are four places, a journey inward from the edge of the realm to the Gate at its heart, and each
brings people who belong there: villagers, an innkeeper and a young delver at the crossroads; a goblin, a huntress, a
druid, a monk and a smuggler at Thornwick; an archmage and his apprentice, a dwarf, a noblewoman, a knight, a
pawnbroker and a mudlark in Crownhold; demons, a court lady, a fence, the King in disguise and once in a while the Maw
himself at the Royal Emporium. Every chest has an **origin** (`Origins`: the millpond, the Barrow, a goblin warren, the
abbey crypt, the Maw's hoard...), and the origin decides what it tends to hold. Sellers say where theirs came from
(`SellerDef.Stories`), always truthfully: whether a seller is lying was never a mechanic worth playing (v18 took the
tall tales out), so the place is a clue to read, never a trap, and a careful player reads every chest by its place
(`Strategy.Likely`). Some sellers tell you something more, always true and always worth working out
(`SellerDef.States`, below).

## The puzzle: rules you'd expect from real life

Three core rules, from the first day:

1. **Every kind of thing has one sound, and it only sounds when the shake is strong enough to stir it.** Coin
   jingles, potions slosh and gems tinkle at the gentlest shake; arms only clank at a firm one; garb flumps and
   scrolls rustle only at a hard one.
2. **While arms clank, you can't hear anything delicate.** Shake gently and the steel stays put.
3. **Sellers price only what they think is there.** Each has their own way of judging (below). Those who listen say
   what they heard, and the slip starts marked with it.

Then, one at a time as the shop grows, more layers of deduction, each taught by the seller it belongs to:

| Layer | From | The rule | How you tell |
|---|---|---|---|
| False bottoms | Thornwick | a sealed hollow under the floor never rattles | drop the chest: a hollow BOOM (glass may break) |
| Two or three of a kind | Crownhold | each thing sounds from its own spot, so more of them rattle more often | count the words over the chest; most sellers never count |
| Sand | Crownhold | a scroll case of river sand rustles just like a scroll, but pours at the gentlest shake; parchment never does | a rustle at a gentle shake is sand |
| Enchantment | Crownhold | anything can be enchanted (worth double); it sounds like anything else | under the **Witchlight** it glows |
| Demon gold | Royal Emporium | gilded iron jingles and tinkles like gold and gems | near the **Lodestone** iron can't move: a fake goes quiet |

Sound-alikes (sand, demon gold) are the deductive heart of the later shops: the sound tells you the kind of thing,
but only the rules tell you whether it's real. Soft things only sound at a hard shake, which hides them from anyone
who won't shake hard (most sellers) or isn't allowed to (Brother Ansel's crypt chests: take the vestments on faith,
or pay to be sure).

How fast you drag sets the shake strength (gentle from 40 px/s, firm from 520, hard from 1150). While you shake,
three words under the chest (gentle, firm, hard) show how hard you're shaking and, in green, the strengths you've
already given it a fair go at; at rest the line under the chest says the same ("Shaken gently and firmly. Not hard
yet."). That's your hands, not the chest: what's inside is still only in the sounds. (v5 to v10 had no readout at all,
and the beginner playtest showed new players couldn't tell a firm shake from a hard one, or which a lesson was still
waiting for.) `ShakeBox` gates physics impacts by `Chest.Sounds`/`Chest.Audible`, and the shake test checks the two
agree, thing by thing, with and without the lodestone (`Magnet` in code).

## What a chest holds, and how good it is

A chest's **colour** says what to expect: wood (common), green (uncommon), blue (rare), purple (epic). Each thing
inside rolls its own quality around that: 77% as promised, 16% one worse, 6% one better, 0.8% two better, 0.2%
three better (what would fall below common or above legendary stays at the end). Value is the kind's base times
1, 3, 10, 30 or 100 by quality (v18; v12 to v17: 1, 3, 8, 20, 50).

Surprises: in v10 one thing in five came out better (two steps in 2%), a third of a careful player's income came from
them, and one lucky chest could pay for a renovation; v12 to v17 cut that to one in twelve and a seventh of the income,
which kept the balance tight and the openings tame. v18 lets them back up: a careful player sees about one thing in
eight come out better (with the luck curios) and one in 140 jump two steps or more, and surprises are about a quarter
of what the chests hold.
So a long dry spell doesn't drag on, a pity counter (`GameState.DrySpell`) counts the things opened since the last
surprise: after 15 (7 with a Lucky Shell), each chest you buy has a 3% chance per thing more (up to 50%) that one thing
in it is a step better, and any surprise resets it. The Lucky Coin and the Four-Leaf Clover work on top of this.

The slip shows what each thing would be worth **if it's as good as the chest promises** (`Categories.Nominal`).
Sellers price by the **average** (`Categories.Expected`), which keeps the economy easy to balance.

Neither number answers the question a player actually has, so the panel shows an **odds bar** under the total: "Worth
more than 55g? 62%". It's exact (`Appraisal.Outcomes` combines every ticked thing's quality odds, what a false bottom
might hold, and the shop's sale bonuses), updates as you tick rows and as you move the offer slider, and runs from red
(a long shot) through gold to green (a safe bet). Why not show the average: 80% of things come out exactly as promised
and the average includes rare big upgrades, so most chests are worth a little *less* than it, and a player who paid it
would lose a little on most buys. Why not just the promised total: a price a little above it can still be worth a
gamble, and one a little below is nearly always a good buy, and the bare number says neither. With the bar there are
no sums to do, and it's only as good as your listening, so the read is still yours. The sim's careful player buys
when the bar is on their side, which wins more often than buying on the average (85 to 92% of buys profit) and still
profits from every seller.

| Kind | Sound | Stirs at | Note | Common to legendary |
|---|---|---|---|---|
| Coin | jingle | gentle | delicate | Barrow Silver 12g, Moonsilver Marks, Dwarf-Gold Crowns, Sunken Sovereigns, Dragon Gold 600g |
| Weapons | clank | firm | loud, iron | Goblin Cleaver 5g, Dwarven Axe, Warden's Blade, Flamebrand, Dragonslayer 250g |
| Cloaks | flump | hard | soft | Wolfskin Cloak 6g, Elven Cloak, Cloak of Shadows, Archmage's Mantle, Phoenix Mantle 300g |
| Potions | slosh | gentle | delicate, glass | Healing Draught 8g, Elixir of Vigour, Invisibility Draught, Mermaid's Breath, Ambrosia 400g |
| Spells | rustle | hard | soft; sand sounds alike | Scroll of Sparks 5g, Scroll of Fireball, Summoner's Grimoire, Lich's Tome, Keyhold Codex 250g |
| Gems | tinkle | gentle | delicate | Moonstone 20g, Elfstone, Blood Ruby, Starstone, Dragon's Eye 1000g |

The kinds are what a sword-and-sorcery realm would lock away: currency, weapons, cloaks, potions, spells and gems.
Each keeps the sound you'd expect of it, so the three rules read the same in any shop. Until 0.16 the commonest of each
was something nobody would want (copper pennies, a rusty dagger, a moth-eaten cloak, a swamp tonic, a tattered map, a
brass ring) and a whole kind was paper; 0.17 made every one a find you'd be glad of, from barrow silver and a goblin's
cleaver up, without touching a value or a sound, so the balance stayed put (the kinds are still Arms, Garb and Scrolls in
code, and an old save's journal is renamed, `Categories.Renamed`). From Crownhold,
**anything can be enchanted** (5% of things, more from the Arcanum): an Enchanted Elven Cloak is still an elven cloak, the
same quality and colour, worth twice as much and twice the exhibition points. It sounds like anything else; under the
Witchlight it glows. A fake (demon gold, a paste gem, a scroll case of sand) is worth 1g; broken glass is worth
nothing.

## Heartsongs: the rarest things in the realm

Nine collectibles of a kind of their own (`Heartsongs.cs`, world lore in `docs/world.md`): things loved more than gold
that the archmages sang the Locking into. They're for the moment of finding one: a sound you've never heard, then a
reveal like nothing else in the game.

- **Mythic** (red), a quality nothing else reaches (`Rarity.Mythic`); no chest promises it.
- **Their own sounds.** Each sings its own synthesized song (`Sfx`: a music box, a pipe, fey chimes, a heartbeat,
  birdsong, a hum, a bell, a shimmer, a crackle and a sigh), none of them like any kind of thing. The rule is one people
  can reason about: it sings at the gentlest shake, and nothing drowns it out or stills it (`Chest.Audible` and `ShakeBox`
  apply it the same way, and the shake test checks it). Only a Keywright hears one, so no seller ever prices one
  (`SellerDef.Notices`): hearing one means the chest is worth more than its price says. When one sings, its row takes the
  top of the slip, in red; tick it like anything else (a perfect read counts it).
- **Rare.** Any chest a seller brings (not a lesson's or the first day's) has a 0.3% chance (`Heartsongs.Chance`), rolled
  with its own dice so nothing else changes. That's about one run in two for the careful player (about two runs in three
  see one by the last week, the first around day 13; v17's 0.4% was one a run, but a find is worth far more now). One you've never found is three times likelier; never two of the same in a run.
- **An ordinary price.** A merchant takes it for a trinket as fine as its chest: 10g times the chest colour's multiplier
  (10g in wood, 300g in purple), on the slip and in the sale. The Guild won't take one in pawn.
- **Display: highly recommended.** It needs no shelf and takes no slot (it hangs on a cord over the counter, at any
  shop, even Bram's stall); the opening's button says so. It adds no exhibition points: each does one big thing instead.
  They're the rarest things in Aldmark, so each is worth a run built around it (v17's were a percent here and there, and
  a find felt like nothing). None shows what a chest is worth: the Bell counts, the Star lights up enchantment, the
  rest is still yours to work out.

| Heartsong | On display |
|---|---|
| The Lullaby Box | it calms every seller: nobody hurries you or minds what you do with their chest (kid gloves and a pocket watch at once), and everyone asks a fifth less |
| The Shepherd's Pipe | two more customers every day, and anyone who walks out comes back at the end of the day, at their first price (once) |
| The Fey Chimes | a third of what comes out as its chest promised comes out a step better |
| The Dragon's Egg | coin and gems sell for twice their worth |
| The Clockwork Lark | two more commissions every week, and every commission pays half as much again |
| The Hearthstone | door money every night: 10g a point of exhibition at the stall, more in a grander shop (`ShopTiers.Scale`: x3, x8, x20) |
| The Drowned Bell | as a chest comes to the counter it tolls once for every thing inside, false bottom and all: Pell's ticket on every chest |
| The Bottled Star | enchanted things glow whenever they sound, with or without the witchlight, and are 15 points likelier |
| The Imp's Ember | glass never breaks, dropped or forced; nothing comes out worse than its chest promised; every fake you open (demon gold, paste, sand) turns real |

Tuning (`--sim=songs`, the careful collector, v20): on display from the first day, against 48% of runs cleared with
none, the Dragon's Egg clears 99%, the Fey Chimes 96%, the Lullaby Box 87%, the Shepherd's Pipe 85%, the Imp's Ember
68%, the Clockwork Lark 64%, the Hearthstone 60% and the Bottled Star 59% (a Four-Leaf Clover from day 1: 80%, a
Lucky Coin 57%). The Drowned Bell is the exception in the sim (51%), because the sim's careful player already counts
perfectly; for a person it's a Pell's ticket on every chest. As they really turn up (in about three runs in five, from
mid-run), the careful player clears 45% with no talents, against a third with no Heartsongs at all.

**The ending.** Bram's kin remember every Heartsong they've opened (`Profile.Songs`, the ledger's Songs page). The Crown
Chest holds the Locking's tune; the Heartsongs are its verses. With all nine known when it's opened, Mara sings the
Locking whole: the Maw starves, the Gate is only a gate, every chest in Aldmark opens (`Story.VaultEpilogue`, worth 200
points). With fewer, she renews it and says how many verses the family knows. In the sim's careers, all nine are known
around the sixteenth run and the first Crown Chest sung whole comes around the seventeenth: after the first clear,
before level 4. Each Heartsong found is worth 20 points too.

## Ways to judge a chest

Each answers one question by a rule you'd expect from real life. All of them are perceptions; nothing states a value.

| How | From | Tells you | Cost or risk |
|---|---|---|---|
| Shake gently, firmly, hard | start | which kinds are inside, and how many of each | Brother Ansel hates a hard shake; Dorrin's chests stand two seconds of it; Snikkit charges for every strength after the first |
| Drop it (lift it high, let go) | Thornwick | a hollow BOOM means a false bottom | glass may break (half the time each); Pell hates it |
| Witchlight (200g) | Crownhold | always lit once bought: anything enchanted glows when it makes a sound | none |
| Lodestone (400g) | Royal Emporium | iron can't move: no clanks to drown things out, and demon gold goes quiet | Lady Ermine hates it |

**Left out: the scale.** A weight only means something if you add up what each thing should weigh in your head, which
isn't intuitive, and the drop already gives the one weight-like fact that matters (hollow or solid). Weight comes back
as a seller's habit instead (Wenna the Huntress goes by it, and the ledger says which kinds are heavy), and as
Hesk's way of reading a chest.

**Lockpicking** (from Thornwick) comes after you buy a locked chest: set each pin when the pick passes the
green. A masterful pick (no slips, mostly perfect) makes everything worth 25% more; the crowbar is quick but scratches
everything (-10%) and breaks glass. Fine Lockpicks (50g) widen the sweet spot.

## Passing: your gut

When you pass, the seller takes the chest away unopened: opening it after you turned it down wouldn't make sense. But
you get feedback straight away, the way you would in life: a gut feeling, from what it really held against their
price (or what they'd have taken, if they wanted you to name one). Six bands, from "You have a feeling you dodged a
cart" through "Could have gone either way" to "Even the heavens pity you", each with a few variants. Words alone are
hard to rank, so the feeling also has a colour and a meter: a strip from red ("missed out") to green ("right call"),
with a marker that slides to where this call landed (`Gut.PassScore`: even at one half, a third of the price or
less at the green end, four times it or more at the red). A seller who storms off leaves the same feeling. After an
opening, the verdict gets a gut line and meter too, from what it held against what you paid, from "fleeced" to "a
steal" (`Gut.BuyScore`).

## Opening a chest

It opens by itself (no holding): a short rumble in the chest's own colour, then the burst. A thing **worse** than its
chest comes first: it pops up in the chest's colour as if all's well, then sags with a deflating slide while the
colour drains to what it really is ("WORSE..."), and flops onto the table. Things as good as promised land quickly.
An enchanted thing lands in a purple glow ("ENCHANTED! x2"). A thing **better** than its chest rises out still glowing in the chest's
colour while the room dims, trembles, and climbs a quality at a time ("BETTER!", "EVEN BETTER!!", "INCREDIBLE!!!"),
each step with a flash, a ring and a rising arpeggio, before it slams onto the table. Two or more steps up also stop
time, flash the room white, and throw confetti and a fanfare. Each quality lands with its own effect, not just its
colour: common, a puff of dust; uncommon, rising sparkles and a chime; rare, a pillar of light and a ring; epic,
crackling lightning, a boom and a lingering aura; legendary, all that with turning rays, a fountain of coins and a
fanfare. `--opening-demo` opens a crafted chest with one of each to preview them.

## Sellers: one way of judging, one rule, and a story

The same rules make a different puzzle with each seller, because each judges a chest a different way, with a blind
spot a careful player can see through; and since v15 each comes from somewhere, tells you where the chest came from
(true or not), and many have **a rule of their own that changes what you can do or what the deal is**. Every rule is
something a real person might insist on. Not every visit is a deal: often the right answer is to pass.

| How they judge | Who | Their blind spot | How to beat it |
|---|---|---|---|
| Listen at one strength | Penn, Snikkit and Fennick (gently), Ansel and Pell (firmly), Hob and Shelly (hard) | what their strength can't stir or a clank drowns; they never count | shake at the strengths they didn't; count |
| Listen gently, then hard | Skrae, Gerda (hard) | nothing they can hear; enchantment, and what's warded | the witchlight; the echoes and the dowry custom (below) |
| Count the rattles | Mags, Quill (firmly) | a rattle's a rattle: a dagger costs what a purse does | buy when the rattles are good kinds |
| Weight | Wenna | heavy means valuable: arms are heavy and cheap, potions and scrolls light | buy light chests of good things |
| The chest's colour | Dorrin, Wren, Orrin, Corisande, Aldous, Vexis, Malbranche, Madame Oriel, the Maw | never look inside | buy when it holds more than a chest that colour usually does |
| A dream | Mother Bramble | the price has nothing to do with the chest | add it up, buy when the dream's cheap |
| Knows only arms | Grit | prices other things at half, and gems as diamonds | haggle; mind the tinkles |
| Knows only her gems | Ermine | prices her gems at what she paid; the rest comes free, but some gems are paste | weigh the free things against the paste |
| No idea | Tam, Old Aldo | Tam takes a few coins (and his millpond chests are sometimes empty); Aldo lets your slip set the price | shake first; read Aldo's exactly |
| Packed it himself | Silas | sells demon gold as gold and gems, cheap | the lodestone silences fakes |

**The rules** (`SellerDef` fields; `Visit`, `Shop.Buy`, `Shop.Settle` and the counter carry them out, and
`GameState.Terms` puts each under the chest in words):

| Rule | Who | What it does | Why it's a puzzle |
|---|---|---|---|
| A rotten chest (`ShakeLimit`) | Dorrin | two seconds of shaking, all told, then it creaks and won't be shaken again | one strength: hard hears everything that isn't drowned out; gentle hears the coin a clank would hide |
| A fee for shaking (`ShakeFee`) | Snikkit | every strength you try after the first puts his price up a tenth | is the hard shake worth a tenth? Gentle and hard between them hear everything firm would |
| No hard shake (`HatesHardShake`) | Brother Ansel | half again if you do | the crypt's vestments only flump at a hard shake: tick them on faith or pay to be sure |
| No drop (`HatesDrop`) | Pell | half again if you do | his ticket counts the false bottom instead |
| No lodestone (`HatesMagnet`) | Lady Ermine | half again if you use it | a quarter of her gems are paste |
| A clock (`Patience`) | Vexis | twelve seconds | gently, then hard, and decide |
| A dream price | Mother Bramble | the price is a dream (0.4 to 1.8 times a usual chest) | add it up yourself |
| A wager (`Wager`) | Archmage Orrin | read his chest exactly and a third of the price comes back | a careful read makes his dear chests cheap |
| An heirloom (`Heirloom`) | Sir Aldous | the best arms inside are his, for a fee he names (three-fifths of what it's worth as promised) | value the rest; a chest with no blade is all yours |
| A devil's due (`TakesDearest`) | Malbranche | half price, but the dearest thing inside is his | many middling things suit his bargain; one grand thing doesn't |
| The King's game (`SlipPrice`) | Old Aldo | his price is your slip: exactly right and he takes a quarter off; wrong, and you pay what it truly held and a quarter more | the purest test of a read; a careless one costs dear |
| The Maw's gaze (`Gaze`) | Avarax, the Gilded Maw | one coin for the chest, but everyone after him today asks twice as much | count who's still waiting |

**Stories** (`SellerDef.Stories`; `Market.Story`). Every seller's line is a story about where the chest came from and
what happened to them, with what they heard or how heavy it felt woven in ("Turned up under the plough on the old
battlefield. Shook it hard: heard a clank. 30g, and that's a bargain!"). It's always true: the place decides what the
chest tends to hold, so the careful player reads every chest by its place.

**What they tell you besides** (`SellerDef.States`, `Visit.Clue`, `Strategy.Reckon`), and chests that change the rules
of hearing. Each is a deduction, on a rule from real life, taught by its seller's lesson:

| Seller | The clue | The deduction |
|---|---|---|
| Fennick the Smuggler | **straw** (`Packs`, `Chest.Packed`): packed so nothing clinks at the Wardens' toll | everything stirs a strength later (`Chest.Needs`): a gentle shake proves nothing, a firm one hears coin, potions and gems, a hard one weapons (whose clank then drowns the rest), and cloaks and spells never stir. He shakes firmly, so he never prices a weapon; nobody can price the soft things, so read them by the road's odds. Still often a false bottom: drop it |
| Pell the Pawnbroker | **the pawn ticket** (`Clue.Articles`): how many articles were pledged, false bottom and all; he won't have pledges dropped | count every separate sound at every strength against the ticket: one short is under the false bottom |
| Wenna the Huntress | **her game scale** (`Clue.Pounds`): the weight, to the pound; her chests hang on a mare who spooks at a hard shake (`Packhorse`) | take what you heard from the weight: what's left never stirred (a cloak 3 lb, a spell 1, a purse under the floor 4). Her scale shows beside the chest, like Hesk's |
| Mother Bramble | **a dream** (`Clue.Dream`, `Market.DreamtOf`): she always dreams true, as a sign (magpie, thorn, raven's wing, river, owl, star), of what the Wood warded inside, or else of the dearest thing (never under a false bottom) | if the dreamt kind never sounds at any strength, it's the warded thing: name it |
| Gerda the Matchmaker | **the dowry custom** (`Clue.Dowry`): a Thornwick dowry always holds coin, a cloak and a potion | she shakes hard and hears everything; whichever of the three you can't hear (after every strength) is warded |
| Wren the Apprentice | **the packing tag** (`Clue.Tag`): every kind packed, warded or not | a kind on the tag that never sounds is warded |
| Lady Corisande | **grandmother's riddle** (`Clue.Riddle`) on the lid, about one thing inside (the warded one, if there is one) | answer the riddle; if the answer never sounds, it's warded |
| Quill the Archivist | **the ward-roll** (`Clue.Roll`): the warded thing's name, every other letter faded ("B_o_d R_b_"); a blank roll, nothing warded | read the name (the words are ordinary ones: 'R_b_' is a ruby, 'E_i_i_' an elixir) and name its kind |
| Skrae | **its echoes** (`Clue.Echo`): the shape of everything inside, warded or not (round coin, long weapon, soft cloak, hollow potion, flat spell, tiny gem) | one more echo of a shape than you can hear is warded |
| Madame Oriel | **three cards** (`Clue.Cards`) from the Locking's deck (Coins, Swords, Mantles, Cups, Wands, Stars) | a suit drawn twice is what the ward holds; three different suits, nothing |

**Warded things** (`Chest.Warded`, from Thornwick). Some chests hold one thing their last owner warded: it never
sounds, weighs nothing, isn't on a pawn ticket or counted by the Drowned Bell, and nobody prices it. It comes out on
opening only if your slip named it (`Chest.Names`): one more of its kind marked than the chest otherwise holds, with no
kind over-marked and no false bottom ticked that isn't there, so guessing at every kind doesn't work. Named, the ward
lets go after everything else has landed, with the slow reveal false bottoms used to have ("...wait. The ward lets go
of what you named!"); not named, it shows, greyed, in the ward, and the results say which clue pointed at it
(`Visit.WardHint`). The gut on a pass counts it, and so does the odds bar once you've marked it. Each seller who wards
says something that points at it (above), always true, and solving it is a deduction from what you can and can't hear:
a kind the clue names that never sounds. The Sleepers' Moon and Wardens' Day hand everyone a dream or a riddle.
(v19: a false bottom is now just a hollow you hear with a drop; the puzzle that pays for working out is the ward.)

Three ways to trade:
- **Fixed price**: buy or pass.
- **Haggle**: they ask high (about 1.9 times what they think it's worth). You get one offer: they take it if it's at
  least what they'd really accept, name a final price if not, or storm off if it's under 35% of their ask.
- **Name a price**: they wait for your offer. Two tries.

| Seller | Place | Comes from | Judges by | Trades | Their rule |
|---|---|---|---|---|---|
| Little Tam | Brindle | the millpond, Gran's attic | no idea | name a price | (30% empty) |
| Widow Penn | Brindle | her Tobb's kitbag, the attic | a gentle shake | fixed | |
| Hob the Farmer | Brindle | the old battlefield | a hard shake | haggle | |
| Mags the Innkeeper | Brindle | unpaid tabs | counting rattles (firmly) | fixed | |
| Dorrin the Delver | Brindle | the Barrow | colour | fixed, cheap | a rotten chest |
| Snikkit the Goblin | Thornwick | a goblin warren | a gentle shake | fixed | a tenth more per strength |
| Wenna the Huntress | Thornwick | outlaws' camps, wolves' dens | weight | fixed, fair | her game scale; no hard shake (her mare); 80% locked |
| Mother Bramble | Thornwick | the Old Wood | a dream | fixed | her dream (half warded) |
| Brother Ansel | Thornwick | the abbey crypt | a firm shake | fixed, steep | no hard shake |
| Fennick the Smuggler | Thornwick | the smugglers' road | a firm shake, through straw | haggle | straw (45% false bottoms) |
| Gerda the Matchmaker | Thornwick | a bride's dowry | a hard shake | fixed | the dowry custom (half warded) |
| Wren the Apprentice | Crownhold | the Arcanum stores | colour | name a price | the packing tag (half warded; 25% enchanted) |
| Archmage Orrin | Crownhold | the Arcanum stores | colour | fixed | a wager |
| Grit the Miner | Crownhold | a dwarf-hold | knowing arms (hard) | haggle | |
| Lady Corisande | Crownhold | a noble's strongroom | colour | fixed | grandmother's riddle (half warded) |
| Pell the Pawnbroker | Crownhold | forgotten pawn lots | a firm shake, never counting | fixed, steep | the pawn ticket; no drop (60% pairs, 35% false bottoms) |
| Quill the Archivist | Crownhold | the Archive vaults | counting rattles (firmly) | fixed, steep | the ward-roll (half warded) |
| Shelly the Mudlark | Crownhold | the Aldwater mud | a hard shake | fixed | (half her scrolls are sand) |
| Sir Aldous | Crownhold | a knight's war chest | colour | fixed | an heirloom |
| Vexis the Imp | Royal Emporium | the Maw's hoard | colour | fixed | twelve seconds (things often better than their colour) |
| Malbranche | Royal Emporium | the Maw's hoard | colour | fixed, cheap | a devil's due |
| Skrae | Royal Emporium | the Maw's hoard | a gentle shake, then a hard one | fixed | its echoes (half warded; enchantment 20%) |
| Madame Oriel | Royal Emporium | a courtier's chest | colour | fixed | three cards (half warded) |
| Lady Ermine | Royal Emporium | a noble's strongroom | her gems, at what she paid | fixed | no lodestone (25% paste) |
| Silas Gilt | Royal Emporium | the Maw's hoard | packed it himself | fixed, at about half | (60% demon gold) |
| Old Aldo | Royal Emporium | the Royal Treasury | none | your slip | the King's game |
| Avarax, the Gilded Maw | Royal Emporium | the Maw's hoard | colour | one coin | the Maw's gaze |

For listeners, whether the chest holds something they can't price (unheard, or a second of a kind they didn't count)
is rolled first, at their miss rate (55% by default; 45% for Pell, 50% for Ansel, 60% for Snikkit; Skrae and Silas
miss nothing), which keeps buying blind from paying while listening does. Everyone brings two of a kind now and
then (8%, from Crownhold). Old Aldo comes in less often than the others (`Visits`), and the Maw rarely.

## A seller's first visit is a lesson

The first time each seller comes in (with tips on), their visit is a lesson in their habit, built so it can't fail
to show it: Fennick's is packed in straw with an unpriced weapon and a false bottom, Pell's ticket counts one more than
you can hear, Wenna's weighs a cloak more than you heard, Mother Bramble dreams of a star with no tinkle inside, Shelly's
has a scroll case of sand, Silas's has demon gold, Malbranche's holds four things for him to take the best of. Bram gives one step at a time; each step has a check
(`LessonStep.Done`, on what's been heard, the slip, the visit) that runs every frame, so a step done early counts.
Until the steps are done, the trade buttons wait (unless the step is about trading, like naming Tam a price); the
lesson can always be skipped. Steps adapt to the tools you own (Skrae's asks you to mark what glows if you have the
witchlight; Silas's to use the lodestone if you have one). Ansel's lesson leaves the choice to you: tick the crypt's
vestments on faith, or shake hard and pay half again to hear them. The new rules each get theirs: one hard shake for
Dorrin's rotten chest, gentle-then-hard for Snikkit's fee, an exact read for Orrin's wager and the King's game, the
queue to count for the Maw. Day one is still the scripted stall day (three customers since 0.20; Mags's lesson in
passing opens the second morning); Tam, Hob and Mags teach naming a price, haggling and counting rattles on their
first visit after it.

**Taught once, across runs** (v20). What Bram has taught (his tips, each seller's lesson, the scripted first day) is
kept in the profile (`Profile.Taught`, given to `GameState.NewRun`), so a second run with tips on doesn't teach it
again: the experience playtest found runs 2 and 3 replaying a dozen lessons the player had already had.

Lessons and Bram's tips never talk over each other. A lesson waits until any tip already up (a new shop, a lock, a
new way of trading) has been read. While a lesson runs, tips that would come up don't pile up behind it: they wait
for their moment to come round again. And a lesson's `Covers` marks the tips it already teaches as seen (Hob's lesson
is the haggling tip; Fennick's, the drop and the boom; Dorrin's, the story tip). A new shop's crowd doesn't all arrive
on its first day: with tips on, no more than two lessons a day (`GameState.LessonsPerDay`), and one a day at
Thornwick, which also brings locks, the drop, straw and warded things (the experience playtest's heaviest days). Bram's kin who come knowing the trade (no lessons) still get a word on each seller's
terms the first time they meet them (`seller_*` in `Hints.cs`), and the terms are always under the chest. `--autoplay` prints every message Bram shows, and flags any that was replaced before it could be read.

`--sim` ends with a lesson test: every lesson played the careful way (the same strategy the bot and the sim use),
with and without the shop's tools, must complete every step in order.

## How the shop opens up

Everything arrives when it's needed, each with a word from Bram, so the first days are only about listening:

| When | What opens up | Bram explains |
|---|---|---|
| Day 1 | shaking, the slip and the odds bar, buying | one lesson per customer (scripted) |
| Day 2 | passing (Mags, first thing), haggling, naming a price, counting rattles; sellers' stories and the ledger (L) | each seller's first visit is a lesson; the ledger and the story tip on a later customer |
| The second evening | the Gilded Guild and Bram's twenty years of back-tithe: a due every fifth evening, the first in three nights (until then nothing shows it) | a scene: Vane, then Bram |
| Day 3 | Nim's curios (his first visit) | a tip: room for three, so pick ones that go together |
| The stall has earned 50g, all told (`GameState.UpgradesAt`) | moves and tools at the end of the day; until then the day-end screen shows how far there is to go | what Thornwick brings |
| Move to Thornwick | locks and lockpicking; the display shelf, keeping things, the exhibition and cabinets; with tips on, a quieter first day (a customer short) at each new shop | the lock (the first time, the pick waits while he talks); the shelf and why an exhibition matters |
| Move to Crownhold | synergies: the journal has tallied everything from day one, so some are ready to switch on | what a synergy is and where to choose one |
| The Royal Emporium | the biggest crowd, the lodestone, the Royal Seal | that the Gate is just down the street |
| Week 1 | two gentle events (days 2 and 4), Nim on day 3 | a tip with each |
| Week 2 | the Locking and the Maw; Bram's shard of the Aldric Key; the Hooded Woman's commissions, which pay in shards of the Key; the regulars' commissions; two events a week | a scene; a tip with each |
| A short night | pawning, and (once a run) Bram's word | a tip |
| A day that starts nearly broke | Bram's secret stash (twice a run) | the day card, and a tip the first time |
| The day's event | a chip in the top bar that reopens it | a tip pointing at it |

The moves are renovations under the hood (`shop2` to `shop4`, `ShopTiers.Name`): Bram's Stall, Thornwick Market,
Crownhold, the Royal Emporium. `GameState.UpgradesOpen`, `DisplayOpen` and `SynergiesOpen` hold the gates, so the sim and the bot follow them too. "Earned" is what
the chests you've bought brought in (the day end's "the chests held", added up): a careful player gets there in two or
three days, and moves once the purse holds the move and Thornwick's working purse besides (about day 7 since v21, whose
working purse is two of Thornwick's typical chests; with less, the move would leave nothing to buy with, and the
warning says so). (Counting profit instead took five to eight days at the stall with nothing new, too long.)

## The run: the Guild's dues

The days come in weeks of five (three in 0.20). At the end of each week's fifth day Mister Vane, Collector for the
Gilded Guild, comes for the week's tithe (`Guild.Due`, `DuesPhase`). Bram's debt is five weeks long (eight until
0.21); after that the Guild keeps coming for "interest", twice as much each week since v19 (half as much again
before), for as long as you last (endless).

| Week | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | then |
|---|---|---|---|---|---|---|---|---|---|
| Due | 40g | 100g | 300g | 800g | 1,400g | 2,400g | 3,800g | 7,000g | x1.5 a week |

(v13; v14's are below.) These follow what a careful shop earns on its own judgment, now that no curio or synergy adds
a share to sales (v10 and v11 climbed to 26,000g, which only the +50% curios made payable). The last week is the wall:
twice the week before, so the debt's last two weeks (10,800g) are rarely in hand before week 8.

**v14: dues for a career, not a run.** With characters, challenge levels and talents, the question changed from "does
a careful player clear the debt?" to "how does a player get from their first run to their first clear, and on up the
levels?" (see *Balance across a career*). The dues now start lighter and finish steeper:

| Week | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | then |
|---|---|---|---|---|---|---|---|---|---|
| Due | 40g | 100g | 200g | 500g | 1,300g | 2,800g | 6,000g | 12,000g | x1.5 a week |

The first four weeks ask for little: a first-timer's purse grows and they get through them (v13's 300g in week 3 sent a
third of them to Bram's word, whose interest then sank them in week 4). From week 5 the dues climb faster than a
learner's shop grows; week 8 is the wall that only good judgment with some talents behind it gets over.

**v18: bigger numbers, faster dues.** The owner: tight constraints made upgrades feel optional and growth flat; let
the numbers scale and the dues climb faster. Qualities are now worth 1, 3, 10, 30 and 100 times a common; the buyers'
trades, the crier and the night market, synergies that put up their kind's price and powerful Heartsongs all compound;
each move inward multiplies what a customer is worth several times. The dues keep the first four weeks for learning
and then climb six to eight times a week, as fast as a growing shop does:

| Week | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | then |
|---|---|---|---|---|---|---|---|---|---|
| Due | 40g | 100g | 200g | 400g | 1,200g | 7,000g | 55,000g | 200,000g | x1.5 a week |

A careful collector's purse on the Guild's night goes from about 130g in week 1 to 2,000g in week 3, 20,000g in week 5
and 200,000g in week 8; a strong run takes off at the Royal Emporium and pays it all off in week 6 or 7.

**v19: the debt grows faster.** The owner: the debt grew too slowly, and the game was too easy even with no talents.
Warded things and two events a week had also put up what a careful shop earns (the careful collector with no talents
cleared 64% of runs before this). The first three weeks stay small, for learning the stall; from week 4 each due is
three to six times the last, so the Guild's night is felt every week, and after the debt the interest doubles weekly:

| Week | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | then |
|---|---|---|---|---|---|---|---|---|---|
| Due | 40g | 100g | 280g | 900g | 5,000g | 30,000g | 130,000g | 420,000g | x2 a week |

The careful collector with no talents now clears about three runs in ten (v18: a little under half), with every
talent about six in ten (v18: three in four); a first run almost never does, and gets through the first three weeks
before the dues outgrow it.

**v20: shorter runs.** The owner: runs were far too long for a game meant to be played again and again (the
experience playtest: a careful run took five hours over 40 days and 400 customers, 150 of its minutes at the Royal
Emporium doing the same again). Cut the run by at least a third and the customers by a third, and rebalance around
it. A week is now three days (`Guild.DaysPerWeek`), so the debt is 24 days; the stall gets three customers a day and
every shop after it five (`ShopTiers.Customers`, before signs, criers, an exhibition and curios: four to seven
before); the moves inward are cheaper (Thornwick 50g once the stall has earned 50g, Crownhold 160g, the Emporium
1,200g) and Thornwick's and Crownhold's chests a little finer (`Sellers.Village`, `Town`), so each move pays about as
well as the last and a short run doesn't stall in the middle. Bram's working purse follows what a day's chests cost
at each shop (20g, 40g, 250g, 2,500g: a move to the Emporium with 500g left bought nothing for days), and his stash
with it. The dues are set against the careful collector's purse with curios, week by week (`--sim=trace` shows one run
day by day):

| Week | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | then |
|---|---|---|---|---|---|---|---|---|---|
| Day | 3 | 6 | 9 | 12 | 15 | 18 | 21 | 24 | |
| Due | 15g | 30g | 60g | 150g | 400g | 1,500g | 20,000g | 170,000g | x2 a week |

The first five weeks ask a tenth or so of a growing purse, while you learn the stall and move inward; week 6 bites,
week 7 is the wall for a shop that hasn't taken off at the Emporium, and week 8 for most of the rest. A careful run
reaches Thornwick on day 4, Crownhold on day 10 and the Royal Emporium on day 15 (medians), and a strong one pays the
whole debt off in week 7. A first run takes about an hour and three quarters (24 days, 130 customers) against
three hours before, and a full careful run about two (24 days, 150 customers) against five.

**v21: working weeks again, and fewer of them.** The owner: keep the five-day week, each with its tax day, two days
with something on in town, and two days for Nim (or Nim once and the story, on a week with a story); shorten the run by
weeks instead. A week is five days again (`Guild.DaysPerWeek`) and the debt five weeks (`Guild.Weeks`): 25 days.
Every day has a role (`DayRole`, planned a week ahead by `Events.Plan` and shown on the ledger's calendar): the fifth
is the Guild's; a story week's first (weeks 1, 2 and 4, and the week after the debt: `Story.TellsOn`) is the story's,
its scene before the shop opens; Nim comes twice a week, never on consecutive days (once on a story week: on the third
in the first week, after two days at the stall); and the two days left have something on in town (gentle things in the
first week). The shop has four customers a day at the stall, six at Thornwick and in Crownhold and five at the Royal
Emporium (`ShopTiers.Customers`), whose every customer is worth a day anywhere else.

| Week | 1 | 2 | 3 | 4 | 5 | then |
|---|---|---|---|---|---|---|
| Day | 5 | 10 | 15 | 20 | 25 | |
| Due | 20g | 60g | 300g | 4,000g | 45,000g | x2 a week |

The first three weeks ask little while you learn the stall and move inward (a careful run reaches Thornwick on day 7,
Crownhold on day 16 and the Royal Emporium on day 18, medians). The fourth asks for a few good days at the capital: a
fifth of careful runs, still at Thornwick or new to Crownhold, need Bram's word for it. The fifth is the wall: more than
the Royal Emporium makes on ordinary trade with every upgrade bought (*The shop's power index*, below), so a run that
clears has a build that came together, or a Heartsong's luck.

**Paying it off early.** Whenever you have all that's left of the debt (`GameState.DebtLeft`: this week's due if it's
unpaid, and every week's after it to the last), you can pay it at once: from the day-end screen's Guild panel, which
shows what's left and turns into a "Pay it all off" button (with a confirmation), or on the Guild's night beside the
week's due. The debt is cleared and the run is won, however early (`ClearedDay`; the profile keeps your fastest).
Keep trading after, and nobody comes until week 6, when the Guild's interest starts (twice the week before, every week). Since v18 a run that takes off
pays it off a week or two early (or buys the Bookkeeping Engine for the same sum and finishes fast); the rest meet the
wall on the last night.

The first weeks are small while you learn the stall and the village; from week 4 the dues climb with what a growing
shop can earn, so every evening's spending is a decision: a renovation or cabinet now pays back over days, but the
Guild comes on a fixed night. Once Vane has introduced himself (the second evening; nothing mentions the due before
then), the HUD shows the next due and when, and the day-end screen shows it against your gold.

**Spending is guided, and warned.** Bram's working purse (`GameState.WorkingPurse`) is what two of the shop's typical
chests cost: the median ask among chests of the colour its sellers usually bring (`Market.TypicalAsk`: 13g at the stall,
80g at Thornwick, 355g in Crownhold, 1,580g at the Emporium, at plain prices), times what sellers ask of this shop
(`GameState.PriceFactor`, *The shop's power index*). The day-end screen says to keep it for tomorrow. Any purchase, at
the shop or from Nim, is checked against the working purse it would leave you needing (`ReserveIf`: with the
purchase's effect on the index, and at the bigger shop for a move inward); one that would leave less shows its price in
red and asks before it buys, saying why (`ShortNote`: the Guild tonight, or "After the Move to Thornwick, two typical
chests at Thornwick Market would cost about 160g, and you'd have 45g. You may not be able to afford chests for a
while."). v20's purse (20, 40, 250 and 2,500g by shop, and an orange price) didn't follow what an upgrade does to
prices: an auction block makes every chest dearer the moment word gets round, and a shop that had spent down to the
old line couldn't afford a chest for days. The sim's careful player keeps the same purse (`Strategy.Reserve`), plus
what bad days before the Guild wouldn't cover of the next due (usual days, for a curio: a curio is bought to earn).

**Bram's secret stash** is the safety net for a beginner who spends too much or buys badly: start a day with less than
20g at the stall and he slips you 30g; 40g/60g in the village, 120g/200g in town, 1,000g/1,500g at the Emporium. Twice
a run (three times before 0.20's shorter runs) (`GameState.StashLeft`); the day card says so, with how many are left, and Bram explains it the first
time. The careful player hardly needs it (about once in two runs).

Short on the night: pawn what's on display (at 60% of its price) or sell curios back (half what you paid). Once a run,
if even that won't do, Bram vouches for you: everything you have goes to the Guild and the rest, with half as much
again, is added to next week's due (not in the last week: the last of the debt is the last). Otherwise the Guild takes the
shop and the run is over. A short night is also the most dramatic moment the shop has: what do you give up first?

## The shop's power index: the numbers behind a run (v21)

The owner asked for one model in place of a pile of requirements, and suggested its shape: ordinary growth (the shop's
upgrades, a general rise in what things sell for) should move a run along an exponential curve, bigger numbers to meet
bigger dues, without making the game easier, so that a player who would lose still loses and one who would win still
wins; a build should add to that and also make the curve a little steeper; and with the index capped and purple the
finest a chest comes, the late debt should take a build. This is that model, and the evidence that it holds.

**What a chest is worth.** Things are worth 1, 3, 10, 30 and 100 times a common of their kind by quality
(`Categories.QualityMultiplier`). Each thing's quality is rolled around its chest's colour (`Categories.Odds`: 16% a step
worse, 77% the same, 7% better), so a chest is worth on average about 1.14 times what its colour promises
(`Categories.Expected` against `Nominal`), and surprises (a step better or more) are about half of what a careful
player's chests hold once the restorers and luck curios are in. Sellers price what they think is there at that
average, times their own markup.

**The staircase.** Each shop has a typical colour (`ShopTiers.Typical`: common at the stall, green at Thornwick, blue in
Crownhold, purple at the Emporium) and its own crowd; the median ask among its typical chests at plain prices
(`Market.TypicalAsk`) is 13g, 80g, 355g and 1,580g. A careful player makes about 10g of profit a customer at the stall,
30g at Thornwick, 215g in Crownhold and 3,000g at the Emporium. Each move inward is the big step; within a shop, the
upgrades and the build grow it.

**The power index** (`GameState.PowerIndex`): what an ordinary thing a seller brings fetches here, luck and all, against
its plain worth, averaged over the shop's kinds by their share of what its sellers bring (`Market.ValueShares`), at the
typical colour. In it: the buyers' trades and the Dragon's Egg; the shop's name for a kind (an active synergy), the
trades' curios and the gallery lamps; and luck (the restorers, the Lucky Shop, the luck curios), at half what it adds
(`GameState.LuckPricedIn`: a seller prices the middle of a chest's range, not its best day, and 0.20 found that luck
priced in whole made the careful player, who goes by the odds bar, buy less), a lucky coin's chest weighted by how many
chests a day are bought and a clover's by the week. Left out: what no seller can price (enchantment, false bottoms,
wards, a Heartsong in the chest, a shooting star), your eye (fees and bonuses for a perfect read), what's taken off a
price (a haggler's tricks, a sealed bid) and today's event. With the upgrades a careful player owns by each move it's 1.0 at the stall and Thornwick, about 1.3
in Crownhold and 1.7 at the Emporium (a working purse of 26g, 160g, 890g and 5,420g), and with every upgrade 2.7
(8,660g) (`--sim=index` prints it shop by shop, and each curio's share of it); only synergies, curios and Heartsongs
take it further, and a chest's colour stops at purple.

**Prices** (`GameState.PriceFactor`, used as `Boosts.Dearer`): a seller's ask is their estimate, times their own
markup and the dice, times the index, times what puts prices up or down besides (the Guild's sharp sellers, the
Lullaby Box, a foreclosure sale, the barker's horn, the cursed crescent). The chest's colour and the seller's habit
stay their own multipliers.

**The margin is the growth rate.** What a gold spent on chests brings back (what they held over what was paid) is how
fast a careful player's purse grows; buying blind has next to none. Measured by the index when each chest was bought
(`--sim`, *Margins*):

| Index when bought | under 1.05 | 1.05-1.3 | 1.3-1.7 | 1.7-2.2 | 2.2 and up |
|---|---|---|---|---|---|
| Careful, no curios | 1.86 | 1.70 | 1.73 | 1.73 | 1.81 |
| Pays the asking price | 1.19 | 1.15 | 1.14 | (1.37) | (1.19) |
| Careful collector, with curios | 1.88 | 1.86 | 1.98 | 2.10 | 2.60 |

Ordinary growth leaves the careful player's margin where it was and the blind buyer's next to nothing: the numbers
get bigger, not the game easier. The collector's margin climbs as its build comes together: the curve gets steeper.
(Bracketed: a blind buyer rarely gets that far, under 2% of their chests.)

**Dues against the ceiling.** The dues climb faster than ordinary growth, and the last week's 45,000g is more than the
Royal Emporium makes with every upgrade on a careful player's margin: a careful player who buys no curios clears about
one run in seven, most of those on a Heartsong's luck, against three in ten for one who builds as Nim's curios come,
and two to nine in ten once a way of trading has come together (*Curios: builds*).

**The working purse** (`GameState.WorkingPurse`): two typical chests at what sellers ask here; every purchase that
would leave less than it would need warns first (*The run*, above).

**A curio's budget.** The economy compounds: an edge in the margin is an edge every day after, so what a run takes in
goes roughly as the fourth power of the margin's excess over one. A curio that isn't priced in is held to its rarity's
share of that (as Nim would bring it: a common a few tenths more takings, an uncommon up to about four-fifths, a rare
up to about double, a legendary, arriving late and only for a builder, about the same), and curios of one way of
trading multiply each other.

## Curios: builds

Curios (`Curios.All`) are the run's build: oddments from the Locking, each bending one rule of the trade, each tagged
with the ways of trading it rewards (`CurioTag`). You have room for three (a Curio Case and a Velvet Case add one each,
a talent another), so a run is a handful of choices that have to fit together. 0.20 made them the build (60 curios,
and the named rule, which held what they added to what your slip named). 0.21 (the owner: rare curios came too often
and too early, the named rule was clumsy, builds are the heart of the fun and each should play differently) gives them
rarities, has Nim build with you, lets the numbers do what the named rule did, and gives six ways of trading a way to
play of their own: 78 curios in all.

**Rarity** (`CurioRarity`: common, uncommon, rare and legendary; green, blue, purple and gold on Nim's cloth,
`Palette.Of`). Each card's rarity is rolled first, by week (`Curios.Rates`): commons and uncommons only in the first
week; rares from the second (5% of cards, rising to 15%); legendaries from the third (2% rising to 6%). A rare needs a
shop at Thornwick and a legendary one in Crownhold (`Curios.RarityTier`). A legendary is a way of trading's capstone,
opened for good by doing that trade often enough across runs (`Feat`, `Curios.Feats`: 40 surprises for Fate's Spindle,
40 perfect reads for the Truthglass, 25 chests talked down for the Golden Tongue, 15 false bottoms opened for the
Smugglers' Compass, 10 warded things named for the Aldric Seal, 15 enchanted things for the Archmage's Staff, 20 chests
taken sight unseen for Fortune's Blindfold), and the ledger's curio page shows how far along you are.

**Prices: a fixed sum and a share** (`Curios.Price`): 10g and 6% of what was in your purse when Nim arrived for a
common, 25g and 10% for an uncommon, 75g and 15% for a rare, 250g and 20% for a legendary. The fixed part keeps a curio
dear at the stall, where a share of a few coins is nothing (0.20's share alone made a common cost a coin or two on
Nim's first visit); the share keeps one a choice at the Emporium, where any fixed sum would be pocket change. He buys
them back for half what you paid.

**Nim builds with you** (`Curios.Stock`). A curio that shares a tag with one you keep is four times as likely on his
cloth. Every visit he lays out at least one for the way of trading of the curio you got most recently
(`Curios.Leaning`: you liked the chalk? he has another like it), two once you keep two of its curios
(`Curios.Building`); on his first visit to a grander shop, one for a way of trading that opens there (the card says
"New at Thornwick": false bottoms and wards there, enchantment in Crownhold); and he brings a legendary only for a way
of trading you're building. The builds report's focus check (`--sim=focus`, below) is why: with seven visits a run and
78 curios, a player trying to build around false bottoms ended most runs with none of them, enchantment almost never
came together, and building around one way of trading did no better than buying whatever came. Now the careful
collector, buying as they come, ends about three runs in ten with three or more curios of one way of trading, and those
runs clear the debt two to three times as often as the rest.

**No named rule: the numbers instead.** Every curio, synergy and restorer reaches everything in a chest you buy. What
keeps the shop's growth from making buying blind pay is the power index (above): whatever puts up what ordinary things
fetch here, or how often they come out better than their chest, puts up what every seller asks. A build's edge is in
what no seller can price: enchantment, and what's under a false bottom or in a ward (nobody knows it's there); your eye
(fees and bonuses for a perfect read); what you pay (a haggler's tricks); and how you buy (a chest taken sight unseen).
Wards still need naming: that's the warded-thing puzzle itself, not a condition on a bonus.

**Six ways to play** (`Curios.Main`), each with a way in (a common or uncommon), a multiplier, an engine that grows over
the run, and a legendary capstone:

- **Enchantment** (from Crownhold): look for the glow. Nobody can hear enchantment and no seller prices it. The draught
  makes it three times as common, the aether jar commoner with every enchanted thing found, the chalk spreads it to a
  second thing in the chest, the ring makes it worth four times a plain thing instead of twice, and the Archmage's Staff
  enchants everything in a chest that has any.
- **Sight unseen** (from the stall): keep your hands off. A chest bought without a shake, a drop or the lodestone
  (`Visit.Handled`, `Purchase.SightUnseen`) is 15% cheaper with a sealed bid (the price goes back up the moment you lay
  a hand on it, and the seller says so), luckier with the lucky dip, insured by the Underwriters' bond (40% of a
  shortfall back), cheaper again for every chest bought today with the wholesaler's tally; Fortune's Blindfold takes 30%
  more off and mends anything worse than its chest. Assembled, it's a build that needs no ear: the puzzle stops
  mattering and the payoff is how many chests you can carry.
- **Haggling** (from the stall): talk them down. The signet's second try, the silver tongue's lower floor, the Waiting
  Game and the Closing Bell, the knot (every chest talked down makes every seller ask a little less), the haggler's
  purse (everyone haggles, and part of what you talk them down by comes back); the Golden Tongue: nobody walks out on
  any offer, and they come down to the least they'd take, and a tenth below it.
- **The perfect read** (from the stall): count every thing. Fees (the seal, and the register, which grows with every
  perfect read), streaks (the tuning fork), stakes (the Ledger of Certainty, 15% either way), certainty (the cursed
  crescent: no surprises, no letdowns, and cheaper chests), and the Truthglass: a perfect read sells for half as much
  again.
- **False bottoms** (from Thornwick): drop it and listen for the boom. The hook doubles them, the log makes them
  likelier with every one opened, the rod makes what's under one a step better, the black-market pass pays double for
  it; the Smugglers' Compass puts one in every chest.
- **Wards** (from Thornwick): read the sign. The Warden's Lantern makes every sign mean a ward, the pillow makes a fifth
  of the quiet sellers dream of one, the tally makes each ward named worth more than the last, the Ward-Key makes what
  you name a step better; the Aldric Seal gives every chest a ward and its seller a sign for it.

Around them: luck, crowds, display, the trades, rough hands, fakes, coffers, patrons and the Songcatcher's Net. A curio
can carry two tags, which is how builds cross (the token is luck and sight unseen; the ward-key wards and the perfect
read). The whole list (`--sim=curiolist`; T: the shop tier Nim brings it from):

| Way of trading | Curio | | Does | Opens |
|---|---|---|---|---|
| enchantment | Witch's Monocle | C, T3 | Enchanted things glow when they sound, witchlight or not. | once you've had a shop in Crownhold, or after two runs |
|  | Witch's Draught | U, T3 | Enchanted things turn up three times as often. | from the start |
|  | Aether Jar | U, T3 | Each enchanted thing you find this run makes every thing 2% likelier to be enchanted (up to 20%). | once you've had a shop in Crownhold, or after two runs |
|  | Enchanter's Chalk | U, T3 | An enchantment spreads: a chest with an enchanted thing in it has a second one. | once you've had a shop in Crownhold, or after two runs |
|  | Spellbinder's Ring | R, T3 | Enchanted things are worth four times as much, not twice. | once you've had the Royal Emporium, or after four runs |
|  | Archmage's Staff | L, T3 | Everything in a chest with an enchanted thing in it is enchanted. | after 15 enchanted things found, all told |
| sight unseen | Sealed Bid | C | Take a chest on its seller's word, without a shake, a drop or the lodestone, and they knock 15% off. | from the start |
|  | Lucky Dip | U | Everything in a chest you take sight unseen is 8% likelier to come out a step better. | after your first run |
|  | Underwriter's Bond | U, T2 | A chest taken sight unseen that holds less than you paid: the Underwriters pay back 40% of the difference. | after your first run |
|  | Wholesaler's Tally | U, T2 | Every chest you buy today: the next seller asks 4% less (up to 20%). | once you've had a shop in Crownhold, or after two runs |
|  | Fortune's Blindfold | L, T3 | A chest you take sight unseen costs 30% less, and nothing in it comes out worse than its chest. | after 20 chests taken sight unseen, all told |
| haggling | Merchant's Signet | C | Hagglers hear you out (an offer too low gets a second try), and come down 10% further. | from the start |
|  | Silver Tongue | U | Hagglers come down a fifth further, and sellers who wait for your offer take a fifth less. | after your first run |
|  | Waiting Game | U | Every chest you pass makes the next seller ask 5% less (up to 20%). Buying starts it again. | from the start |
|  | Closing Bell | U | The last customer of the day asks 25% less. | from the start |
|  | Haggler's Knot | U, T2 | Word gets round: each chest you've talked down this run makes every seller ask 1% less (up to 8%). | after your first run |
|  | Haggler's Purse | R, T2 | Everyone haggles with you, and 40% of whatever you talk them down by comes back to you. | once you've had the Royal Emporium, or after four runs |
|  | Golden Tongue | L, T3 | Everyone haggles with you, and nobody walks out on an offer: however low, they come down to the least they'd take, and 10% less than that. | after 25 chests talked down, all told |
| the perfect read | Ear Trumpet | C | Nothing drowns out or muffles a sound for you: no clank, and no straw. | from the start |
|  | Pocket Watch | C | Impatient sellers give you twice as long, and a rotten chest stands twice the shaking. | from the start |
|  | Counting Beads | C, T2 | Every chest comes with a tally of how many things are in it, false bottom and all. | from the start |
|  | Appraiser's Seal | U | A perfect read pays you a valuer's fee: 8% of what the chest held. | from the start |
|  | Cursed Crescent | U | No surprises, no letdowns: everything comes out exactly as its chest promised. Sellers sense the curse and ask 5% less. | after your first run |
|  | Valuer's Register | U, T2 | A perfect read pays 0.5% of what the chest held for every perfect read so far this run (up to 12%). | after your first run |
|  | Tuning Fork | R | Each perfect read in a row: everything in the next chest sells for 3% more (up to 30%). A wrong slip starts again. | once you've had the Royal Emporium, or after four runs |
|  | Ledger of Certainty | R, T2 | A perfect read: everything in the chest sells for 15% more. A wrong slip: 15% less. | once you've had the Royal Emporium, or after four runs |
|  | Truthglass | L, T3 | A perfect read: everything in the chest sells for half as much again. | after 40 perfect reads, all told |
| false bottoms | Padded Crate | C, T2 | Glass never breaks when you drop a chest. | from the start |
|  | Smuggler's Hook | U, T2 | Smugglers come your way: false bottoms turn up twice as often. | from the start |
|  | Hollow Rod | U, T2 | What's under a false bottom comes out a step better: people hid their best. | after your first run |
|  | Smuggler's Log | U, T2 | Each false bottom you open this run makes them 2% likelier (up to 20%): smugglers trust a buyer who keeps quiet. | after your first run |
|  | Black Market Pass | R, T2 | Thornwick's black market pays double for anything that came out from under a false bottom. | once you've had the Royal Emporium, or after four runs |
|  | Smugglers' Compass | L, T3 | Every chest has a false bottom. | after 15 false bottoms opened, all told |
| wards | Warden's Lantern | C, T2 | A seller who leaves a sign always means it: every dream, dowry, tag, riddle, roll, echo and card points at a warded thing. | from the start |
|  | Sleeper's Pillow | U, T2 | Sellers dream true for you: 20% of them have warded something, and tell you their dream. | after your first run |
|  | Wardbreaker's Tally | U, T2 | Each warded thing you name this run makes warded things sell for 10% more. | after your first run |
|  | Ward-Key | R, T2 | A warded thing you name comes out a step better. | once you've had the Royal Emporium, or after four runs |
|  | The Aldric Seal | L, T3 | Every chest has a warded thing, and its seller tells you a sign for it. | after 10 warded things named, all told |
| luck | Lucky Shell | C | A dry spell ends twice as soon: luck starts to turn after 5 plain finds, not 10. | from the start |
|  | Gambler's Token | U | Luck swings both ways: twice as many surprises, good and bad, in everything you open. | after your first run |
|  | Lucky Coin | U | The first chest you buy each day: everything in it gets two tries at its quality. | from the start |
|  | Rabbit's Foot | U | Everything you open is 5% likelier to come out a step better than its chest. | from the start |
|  | Wishbone | U | Something that comes out better than its chest sells for half as much again. | after your first run |
|  | Horseshoe | U, T2 | Luck runs in streaks: each chest with a surprise in it makes the next 6% luckier (up to 30%). | after your first run |
|  | Four-Leaf Clover | R, T2 | The first chest you buy each week: everything in it comes out a step better. | from the start |
|  | Shooting Star | R, T2 | Once a day, the first thing to come out better than its chest climbs a step further. | once you've cleared Bram's debt |
|  | Fate's Spindle | L, T3 | Each surprise this run makes everything 1% likelier to come out a step better (up to 30%). | after 40 surprises (things better than their chest), all told |
| crowds | Shop Lantern | U | Open late: one more customer every day. | from the start |
|  | Queue Ticket | U | The first chest you pass each day comes back at the end of the day, asking 20% less. | from the start |
|  | Barker's Horn | U, T2 | Two more customers every day, but word's out: everyone asks 5% more. | once you've had a shop in Crownhold, or after two runs |
|  | Guest Book | U, T2 | The day's first customer is the better sort: their chest is a colour finer. | after your first run |
| display | Collector's Catalogue | C | Every new kind of find this run pays 4g (more in a grander shop). | from the start |
|  | Bell Jar | C, T2 | Room to display one more thing. | from the start |
|  | Visitors' Book | U, T2 | Every night, visitors leave 6g for each point of your exhibition (more in a grander shop). | from the start |
|  | Velvet Rope | U, T2 | Your exhibition counts double. | once you've had a shop in Crownhold, or after two runs |
|  | Gallery Lamps | R, T2 | Your exhibition sells for you: everything fetches 1.5% more for each point of it (up to 60%). | once you've had the Royal Emporium, or after four runs |
| a trade | Smith's Glove | C | You mend weapons: they never come out worse than their chest promised, and are 15% likelier to come out better. | from the start |
|  | Hatter's Cap | C | You mend cloaks: they never come out worse than their chest promised, and are 15% likelier to come out better. | from the start |
|  | Whetstone | U | Weapons sell for three-quarters more. | from the start |
|  | Tailor's Dummy | U | Cloaks sell for half as much again, and count 3 times over on display. | after your first run |
|  | Moneychanger's Scales | U | Coin sells for a fifth more for every purse of it in the chest. | after your first run |
|  | Alchemist's Still | U, T2 | Potions sell for half as much again, and broken glass still sells for half. | once you've had a shop in Crownhold, or after two runs |
|  | Scribe's Desk | U, T2 | Spells sell for half as much again, and sand sells as ink, for a third of a spell. | once you've had a shop in Crownhold, or after two runs |
|  | Gem Cutter's Wheel | U, T3 | Gems are 33% likelier to come out a step better. | once you've had a shop in Crownhold, or after two runs |
| rough hands | Locksmith's Oil | C, T2 | Every lock has one pin fewer. | from the start |
|  | Burglar's Knife | U, T2 | Forcing a lock breaks nothing and scratches nothing. | from the start |
|  | Kid Gloves | U, T2 | Nobody minds a hard shake, a drop or the lodestone on their chest. | from the start |
| fakes | Jeweller's Loupe | C, T4 | Paste gems and demon gold sell as costume pieces: a quarter of the real thing. | from the start |
|  | Fence's Mark | R, T3 | Crooks come your way twice as often, and the Maw's buyers take any fake off you for what the real thing would fetch. | once you've had the Royal Emporium, or after four runs |
| coffers | Rainy-Day Purse | C | Bram's stash has one more dip in it. | from the start |
|  | Pawn Ticket | C | On a short night, what you pawn fetches four-fifths of its price, not three-fifths. | from the start |
|  | Guildsman's Marker | C | Bram's word is good twice a run, not once. | from the start |
|  | Savings Jar | U | Every night: 3% of your gold as interest (up to 20g at the stall, more in a grander shop). | from the start |
|  | Miser's Lockbox | R, T2 | Every Guild night, once you've paid: 25% of what's left in your purse. | once you've had the Royal Emporium, or after four runs |
| patrons | Letter of Introduction | U | Regulars take what you deliver a quality lower than they asked for. | from the start |
|  | Patron's Ribbon | U | Commissions pay half as much again. | from the start |
|  | Errand Book | U, T2 | The regulars post 2 more commissions every week. | once you've had a shop in Crownhold, or after two runs |
| Heartsongs | Songcatcher's Net | R | Heartsongs are three times as likely to be in a chest. | once you've heard a Heartsong |

**Luck** (`GameState.Rules`, `Turn`) runs in a fixed order, the same as a roll at purchase and as a transform of the
odds for the slip and the index: a lucky coin's second try, fixed steps (the clover, the hollow rod, the ward-key),
chance steps (the restorers, the rabbit's foot, the gem cutter's wheel, the horseshoe's streak, the glove's and the
cap's trade luck, the Lucky Shop's name, Fate's Spindle, a lucky dip's for a chest taken sight unseen), the token's
swing, mending (the glove, the cap, the Imp's Ember, a blindfold's for a chest taken sight unseen), the Fey Chimes, and
last the crescent, which holds everything to its chest's colour. The shooting star comes after, once a day. The opening
names whoever did it ("Rabbit's Foot: 1 thing came out better!").

**Measuring them** (`--sim=builds`, ten minutes): each curio alone, given when Nim could first have brought it (its
shop, its rarity's shop and week: `Style.Arrives`, since a legendary measured from the first day said nothing about a
run); each way of trading's three strongest the same way; a player buying for one way of trading from Nim
(`Style.Focus`); the sight-unseen build a piece at a time, with a buyer who is careful until two of its curios are kept
and never lays a hand on a chest after (`Care.Unseen`); pairs against their parts; and a buyer who never shakes, with
each other build from the first day. `--sim=curio --who=a,b` measures a few quickly, `--sim=focus --who=TAG` shows
what a builder ends up keeping (`--who=any`: the careful collector; `--trace`: Nim's visits one by one). The careful
player's taste (`Strategy.CurioWorth`) is about a fifth of what each adds when Nim brings it, `Strategy.CurioScore`
adds half again for each kept curio it shares a way of trading with, and a curio is bought against usual days, not bad
ones, before the Guild (`Strategy.Reserve`): saving for the last week's wall otherwise kept every late build from coming
together.

What they measured, as 0.21 shipped (100 runs each; the careful collector with no curios clears about a quarter of the
time in this report):

| Rarity | Each alone, as Nim brings it: added takings | At the top | At the bottom |
|---|---|---|---|
| Common | +0% to +61% | the Warden's Lantern, the lucky shell, the catalogue | the insurance ones (the purse, the pawn ticket, the marker: they're for bad nights, not takings), the bell jar, the loupe |
| Uncommon | -3% to +71% | the hook and the rod, the lantern, the knot, the queue ticket, the crescent, the rabbit's foot, the Closing Bell | the knife, the scribe's desk, the velvet rope, the lucky coin, the wishbone |
| Rare | +3% to +100% | the shooting star, the Ledger of Certainty, the tuning fork, the black-market pass, the Ward-Key | the fence's mark, the gallery lamps, the lockbox |
| Legendary | +32% to +110% | the Truthglass, the Smugglers' Compass | the Archmage's Staff and the Golden Tongue (they come late: from the first day they're worth +55% and +267%) |

A way of trading's three strongest, as Nim brings them, clear: false bottoms (the compass, the hook, the rod) 82%,
wards (the ward-key, the lantern, the Aldric Seal) 81%, luck (the star, the spindle, the foot) 82%, the perfect read
(the Truthglass, the ledger, the fork) 73%, coffers 69%, crowds 63%, display 58%, enchantment (the draught, the staff,
the ring) 57%, haggling 48%, patrons 46%; the sight-unseen build (sealed, dip, bond and tally, careful until two are
kept) 71%, and with Fortune's Blindfold 78%. Pairs from one way of trading are worth more together: the hook and the
black-market pass +176 points of takings beyond their parts, the log and the hook +163, the hook and the rod +153,
the sealed bid and the lucky dip +166 for a buyer who never shakes, the Ward-Key and the lantern +101, the ledger and
the Truthglass +86, the spindle and the star +68, the draught and the ring +65. The careful collector buying as they
come clears 42%, and ends about three runs in ten with three or more curios of one way of trading, which clear two to
three times as often as the rest (`--sim=focus --who=any`).

A buyer who never shakes is foreclosed with every other way of trading's best three, given from the first day, but
one: the false-bottom capstone (the compass's false bottom in every chest, a step better with the rod) carries even a
blind buyer (76%), which the owner allowed for a build that has come together, and which a run can't have early (the
compass is a Crownhold legendary, brought only to a builder). The luck and enchantment builds keep a blind buyer going
to the last week, and clear a tenth of the time.

## Commissions

From week 2, three regulars you've met post a commission each week (`Patrons.Roll`), good until the Guild's next
visit: one thing of a kind at a quality (a step better than the shop's chests usually are), two or three of a kind
(a step lower), or (from Crownhold) anything enchanted. They pay two or three times what such a thing sells
for. A board under the seller lists them, the slip marks the kinds they want with a gold pennant, and anything that
comes out of a chest you open can be delivered instead of sold. A commission makes a sound worth more than its price:
a chest with a flump in it is worth chasing when Penn wants silk.

## Events

A day or two a week (none in the first, which is for learning the stall; one in weeks 2 and 3; two from week 4), on
days of their own, something happens in town (the day card
announces it, and a chip in the top bar keeps it there all day: click it to read it again; `Events`). Each week's plan
(its two event days, the kind wanted, and Nim's day) is made a week ahead (`Events.Plan`, `GameState.Plan` and
`NextPlan`, saved), so the ledger's calendar can show this week and next. They all come round before any comes twice;
the first week with anything on has a gentle one (it changes the day, not how to judge a chest).

| Event | From | That day |
|---|---|---|
| Fair Day | start | three more customers |
| Locking Day | start | three hundred years since the Locking: everything sells for a quarter more |
| The Crown's Buyer | start | one kind of thing sells for double |
| A Foreclosure Sale | start | the Guild has foreclosed on a manor: everyone asks a fifth less |
| The Tourney | start | knights in town: weapons and cloaks sell for half again as much |
| A Storm | start | two fewer customers, but every chest a colour better |
| A Royal Chest | start | someone brings one of King Aldric's own chests, gilded with the royal seal: a colour better than its seller thinks, with something enchanted inside |
| The Stranger | start | an extra customer with a black chest wrapped in iron chains: nothing rattles, but a drop tells you how heavy it is (heavy ones hold more: often junk, sometimes a fortune) |
| A Day of Mourning | start | the old Duke's funeral: nobody stands for a hard shake (cloaks and spells go unheard; read the rest) |
| The Town Crier | start | Lord Harrow's crier posts a reward for one thing of a kind, as good as the regulars ask: three times its price, today only (a one-day commission) |
| The Minstrels' Fair | start | anyone with a box that hums brings it: a Heartsong three times as likely |
| The Sleepers' Moon | Thornwick | everyone dreamt true, like Mother Bramble: every ordinary seller tells you their dream (of the thing they warded, if they warded one, which most did; else of the dearest thing that can sound: a dream never reaches under a false bottom) |
| Wardens' Day | Thornwick | the old custom: every ordinary seller has warded one thing and left a riddle for it |
| The Guild's Count | Thornwick | a Guild clerk tallies every article in every chest, false bottoms and all (Pell's ticket, for everyone) |
| The Guild's Assessor | Thornwick | a tenth of every sale goes to the Guild |
| A Barge Aground | Crownhold | a treasure barge on the Aldwater: lots come in twos and threes |
| A Witchlight Moon | Crownhold | the old enchantments wake: things are far likelier to be enchanted |

The puzzle days (`Events.Restate`) give every seller without a clue of their own (and not the King, whose price is your
slip) the day's clue, so the whole day is a run of Mother Bramble's and Lady Corisande's puzzles, or of Pell's
counting.

## The ledger's calendar

The ledger's Calendar page (it replaced Places, which was reference nobody needed once every seller says where their
chest came from) shows three weeks, a day to a box: today framed in gold, the days gone by faded, each day's event
(this week's and next week's are planned), the morning Nim comes by, and each Guild's night with what it will want
(this week's in red if the purse is short). Hovering a day tells its whole story; until then, the next thing coming is
told under the grid. Nim comes every week from the first (its third morning), on a day without an event when he can.

## The story

The world's story (`docs/world.md`) is told in scenes (`ScenePhase`, `Story`) on the second evening, at the start of
some weeks, on the Guild's nights and at the end. Each scene answers one question and leaves the next one open:

- The second evening: Mister Vane of the Gilded Guild. Every Keywright pays the Guild a tithe "to keep the Gate"; Bram
  opened chests unlicensed for twenty years, and the back-tithe comes with the stall, in eight weekly payments.
- Week 2: Bram tells you what the chests are (the Locking, King Aldric, the Maw behind the Gilded Gate) and gives you
  his one secret, a shard of the Aldric Key, broken in four when the Locking was made.
- Weeks 3, 5 and 7: the Hooded Woman asks for something only a Keywright's chests hold (scrolls, a gem, something
  enchanted; or coin or a blade while the shop is small, each with its reason) and pays a shard for it. Miss one and
  she asks again the next week, until the debt's last week, even if the debt's already paid off.
- Week 4: Bram knows her voice; the Guild has been buying every chest with a royal seal. Week 5: the whole Key opens
  the Crown Chest, King Aldric's own, which holds the heart of the Locking; the debt is a leash.
- Week 6: Vane offers to halve that night's tithe for your shards. Take it and there's no Crown Chest.
- Week 7: she is Mara, the last archmage of the Keyhold and Bram's old partner. Every tithe goes down through the Gate:
  the Maw is buying his way out, and every chest opened loosens the Locking. At the Royal Emporium, Bram spots the seal
  on Old Aldo's ring: the tired old man who's been playing his game with your slip is the King.
- Debt paid, with the Key whole: the King gives you the Crown Chest, so it opens for you (a gilded chest with the best of
  everything inside, opened like any other), and the Locking holds: the Gate goes quiet. Then you keep trading
  against the Guild's interest, or retire. Paid off early without the whole Key, you can keep trading: the Guild
  leaves you alone until week 9, and if Mara's last shard comes, the Crown Chest opens that night.
- Foreclosed: the Guild seals the shop, and your licence with it.

Internally the shards are still `MapPieces` and the Crown Chest is the vault (`VaultOpened`, `--vault-demo`), so
saves and profiles carry over. Every run is recorded in `user://profile.json` (runs, debts cleared, Crown Chests
opened, the furthest week), shown on the title screen and at the end of a run.

## Across runs: Bram's kin, challenge levels and talents

A run's own loop (a chest, a day, a week, five weeks) needed a reason to play another run that isn't the same run
again. Three things carry over, all in `scripts/Core/Meta.cs` (definitions, the score) and `Kin.cs` (what they do to a
run, as a partial of `GameState`); the profile (`Profile` in `Save.cs`, user://profile.json) keeps renown, talents, each
character's open levels and best score.

**Bram's kin** (`Characters`). Each changes one part of how you judge a chest or run the shop, and pays for it; the rest
of the game is the same. They're all Bram's family, so the story's lines ("your uncle") hold for each.

| Who | Unlocked by | What's different |
|---|---|---|
| Kit, the Ear | from the start | the game as it was: every strength, the drop, and (with tips on) Bram's lessons as each seller arrives |
| Hesk, the Scale | paying the third week's due | a bad back: no hard shake and no drop. Every chest sits on his scale, and he knows what each kind weighs (coin 4 lb, arms 6, garb 3, potions 3, scrolls 1, gems 1); demon gold weighs half what gold does and a case of sand three times a scroll, so the weight left over once the heard things are counted is what he couldn't hear, what's under a false bottom, or a fake weighing short |
| Rosa, the Tongue | clearing the debt | everybody haggles with her (fixed-price sellers ask high and hear one offer instead, though never more than a bit over the price they'd have named, nor holding out for more than it; a crook still names his too-good price, and nobody haggles with the King or the Maw), but they know her: they open 3% higher and come down 3% less; an offer too low gets a second try; a seller who walks out on her doesn't come back that week |
| Isolde, the Collection | opening the Crown Chest | a two-place shelf from the stall on; visitors pay 1g per exhibition point every night (not more in a grander shop: that version cleared days early); her collection can't be pawned, so a short night can only take her curios |

Every new judgment follows a rule people know: things weigh what they weigh, and gold is heavier than iron (the old
test for a gilded crown). Hesk's scale shows the total and the slip shows what the ticked things would weigh; the sum
is the player's to do. The careful player's read is `Strategy.Weigh`: the likeliest mix of garb, scrolls, a false
bottom and short-weight fakes for the leftover, given the seller's habits. Bram's lessons are Kit's (the others come
knowing the trade, `GameState.GivesLessons`); each of the others gets a first-morning tip on their way (`kin_*` in
`Hints.cs`). Curios a character can't use aren't in Nim's stock (`GameState.NoUse`: no padded crate for Hesk, no
signet for Rosa, no visitors' book for Isolde).

**Challenge levels** (`Challenges`). Eight, each adding one term on top of the ones below, opened one at a time per
character by clearing the debt at the level below. Each adds a quarter to the renown a run earns (x3 at level 8).
Terms are rules that hold every time, not hidden rolls:

| Level | Term |
|---|---|
| 1 | Bram's stretched thin: his stash has one dip fewer, his word one time fewer |
| 2 | Fussy sellers: everyone minds a drop (price up by half) |
| 3 | Demon gold about: one coin in ten is demon gold, whoever brings it |
| 4 | Lean start: half the starting gold |
| 5 | Thin crowds: one customer fewer every day |
| 6 | The Guild's interest: every due a quarter higher |
| 7 | Sharp sellers: everyone asks, and takes, a tenth more |
| 8 | Hurried: every seller gives you 20 seconds |

They're ordered mildest first by what each does on its own to the careful player with every talent (the sim's
`Trial`, one term at a time; v14's figures are in *Balance across a career*): the stash and the word matter to people
who get into trouble more than to the careful player, so they come first; a thinner margin on every chest is the
hardest, because the last weeks need every coin the shop's growth can compound.

**Score and renown** (`Score`). Points for the Guild dues paid (10 each), the debt cleared (100) and every day to
spare (5), the Crown Chest (100) or the Key's shards held (10 each), moves (25 each), the collection (2 per find) and
every week after the debt you kept the Guild paid (25). A foreclosed run still scores what it did, so every run moves
you on. Renown is the score times the level's multiplier, banked when the run ends (retiring, foreclosure, or starting
over on a saved shop); the run's end shows the lines, the renown and anything it opened.

**Talents** (`Talents`). A root and three branches, each talent building on the one above it (12 in all, 5,500
renown). Like curios and synergies, **no talent adds a share to what things sell for or takes one off the
dues**: they soften a bad night, widen your choice of curios, or open the shop's name sooner.

| Branch | Talents (cost) |
|---|---|
| (root) | Bram's Kin (50): start with 10g more |
| Bram's Lessons | Rainy Day (100): a fourth stash dip; Pawnbroker's Friend (250): pawning at 70%; Good Word (500): Bram vouches twice; Mara's Favour (1,000): once a run, Mara pays a night nothing you own can cover (not the last week) |
| Nim's Favour | Familiar Face (100): four curios on Nim's counter; Second Look (250): once a visit, another four; Keepsake (500): on Nim's first visit, one curio of your choice is free; Curio Cabinet (1,000): a fourth curio slot |
| The Trade | Early Name (250): synergies from Thornwick; Trade Secrets (500): the witchlight and the lodestone at half price; Royal Favour (1,000): one more synergy slot |

Bram's Ledger (100: every seller's habits in the ledger from the start) went in v21, when the ledger started keeping
every regular you've met across runs.

Two first drafts sped the run up rather than widening it, and went: Keepsake as a free common curio on the first
morning (alone, +8 points of clears; with Early Name and Old Picks, free fine lockpicks from the start, +22), because a
curio or 50g on day one compounds through the whole run. A curio of your choice on Nim's first visit, and the sellers'
habits in the ledger, widen choices without moving the timeline as much.

## The shop's look

Each move changes the shop around you: Bram's trestle table under a striped awning at the Brindle crossroads, with
the mill, the river and the Barrow's stones behind it at dusk; a stone-walled shop at Thornwick Market with timber
beams, oil lamps and a window onto the Old Wood; a panelled shop in Crownhold whose display window looks over the
roofs to the palace at sunset; the Royal Emporium's colonnade, crimson drapes and marble counter, with the Gilded Gate
glowing down the avenue. Each is one 640x360 picture made by `tools/import_art.py` (a backdrop drawn in
`tools/custom_art.py`, the architecture, a few props, and the counter), shown at 2x behind the counter and dimmed
behind the lockpicking, opening and day-end screens (`UiKit.MakeShop`). What you keep stands on the shelf (the window
sill in town).

All the pictures are the game's own, drawn in code so they follow the world rather than a pack's: every portrait
stands against its place (the crossroads' dusk, the Old Wood's green, Crownhold's blue, the Emporium's crimson, the
story's night), the Maw is a borrowed body gilded all over, demon gold is the real thing gone dull and greenish, the
paste gem is a cut stone with no fire in it, and a scroll case of sand looks like the mudlark's "maps" it is. Things
are 24px with one light (top left), one dark outline and one palette of materials, so a coin from Brindle and a
crown from the vault sit together on a shelf.

## Title, saves and settings

The game opens on a title screen (Continue, New game, Talents once there's a run behind you, Settings, Quit) over
your shop as you left it. New game opens the character screen (`SetupPhase`): the four of Bram's kin (locked ones say
what unlocks them), the chosen one's rules, and the challenge level with its terms and multiplier. The shop is
saved to `user://save.json` at the start of each day (`GameState.Save` and `Load`, plain JSON from Core), so leaving
mid-day starts that day over; the pause menu (Esc, or Menu in the top bar) says so. Settings (`user://settings.cfg`):
volume, fullscreen, screen shake and flashes (full, reduced or off), Uncle Bram's tips, and **item sounds**: for each
kind of thing, which of three or four sounds it makes (`Audio.SoundSets`; recorded ones from Kenney's CC0 packs via
`tools/import_art.py`, a few synthesized in `Sfx`). Each kind's sound stays the one you'd expect in real life (a
purse, a pot, a thump of cloth, a bottle, pages, glass), so any set keeps the rules readable; the choice is taste.
Click a kind's option to hear the next one.

**GM mode** (v21, for testing; Settings > GM mode, or `--gm`): the pause menu gets a GM panel beside it: +99,999 gold,
skip the rest of the day (the customers still waiting go home, and the day ends as usual, so the Guild's night still
comes), and unlock the whole journal (every regular, every find and its enchanted star, every curio Nim could carry,
in the run and the profile). Nothing in it is fair, and the panel says so.

## Collection, synergies and exhibition

Every thing you open goes in the ledger's collection the first time you find it (33 entries: five qualities of six
kinds and three fakes; finding one enchanted stars its entry) and in the journal's tally of what you've acquired,
kind by quality.

**The journal is kept across runs** (v21; the owner: a regular's page and a collection entry should open the moment
you discover them, and stay open). It's Bram's kin's ledger, not the run's: a regular's page opens the first time they
come to your counter, a collection entry the first time you find the thing, a curio the first time Nim lays it out,
and each stays open in every run after (`Profile.Met`, `Found`, `FoundEnchanted` and `CuriosSeen`, merged into a run by
`GameState.Remember`; `Profile.Note` keeps them when a run ends). The run's own finds still score and still pay a
collector's catalogue. The ledger's **Curios** page shows all 78: the ones you've met with what they do, the ones you
keep, and the ones Nim doesn't carry for you yet, dark, with what opens them (a legendary's feat with how far along you
are). Bram's Ledger, the talent that showed every regular from the start, had nothing left to do and went; a profile
that had it gets its 100 renown back. Each thing looks the same everywhere: the collection, the shelf, the exhibition and the keep chooser
all show its own sprite (`UiKit.MakeItemIcon`, from icons `tools/import_art.py` halves when a sprite is too big).

**Synergies** turn that tally into goals. Each has five levels; six of a kind at common or better reaches level 1
(ten before 0.20's shorter runs), six at uncommon or better level 2, and so on to six legendary for level 5. Five are
achievements instead, each fed by a way of trading the curios reward. A synergy is what the shop gets known for, and a
name works the way it does in real life: it brings people and chances your way, spares you some breakage, and buyers
pay more for the kind of thing it's known for. Sellers hear of a name too: what it puts on a kind's price goes into
the shop's power index (0.20 held it to the things your slip named instead; see *Curios: builds*).

| Synergy | Needs | Does (per level) |
|---|---|---|
| Goldsmith (Crownhold) | coin and gems | coin and gems turn up 15% more often and sell for 20% more |
| Armourer | weapons | soldiers, knights and dwarves come 25% more often; weapons sell for 20% more |
| Apothecary (Thornwick) | potions | glass 20% less likely to break, dropped or forced (never, at level 5); potions sell for 20% more |
| Spellkeeper (Thornwick) | spells | chests 5% likelier to have a false bottom; spells sell for 20% more |
| Cloakmaker | cloaks | exhibition +25%; cloaks sell for 20% more |
| Keen Eye | perfect reads in a row (3, 5, 8, 12, 18) | a perfect read earns a valuer's fee of 8% of what the chest held |
| Lucky Shop | surprises (3, 6, 10, 15, 22) | everything is 4% likelier to come out a step better |
| Finder (Thornwick) | false bottoms found and warded things named (2, 4, 7, 11, 16) | false bottoms 4% likelier; what you find under one or name in a ward sells for 25% more |
| Haggler | chests talked down or named a price for (3, 6, 10, 15, 22) | hagglers and sellers who wait for an offer settle for 4% less |
| Arcanist (Crownhold) | enchanted things found (2, 5, 9, 14, 20) | each thing 4% likelier to be enchanted; enchanted things sell for 20% more |

Only **one** synergy is active at a time, chosen at the end of a day (the day-end screen's Synergies button opens the
journal with switches). A Crownhold Charter (600g) and then a Royal Seal (2,500g, Royal Emporium, only once you have the
charter) add a slot each. The Synergies button is on the day-end screen from the moment you move to Crownhold, even
before any synergy is reached ("Synergies: none reached yet"), so Bram's tip always has something to point at. That makes a
specific kind of chest worth wanting: a player chasing Goldsmith level 3 wants blue chests with coin and gems (a
noble's strongroom, the Maw's hoard), and will pay up for them.

After an opening, anything not fake or broken can be **kept for display** instead of sold: you give up its price. The
button shows the points it adds (a star for enchanted, which counts double). With the display full, it becomes **Swap**:
pick something on display to sell, and the new thing takes its place. Display space: a shelf with room for one, then
six cabinets of one each, each on offer only once the one before it is bought: Oak Cabinet 80g and Oak Dresser 160g
(Thornwick), Glass Cabinet 400g and Glass Case 700g (Crownhold), Gilded Cabinet 1,500g and Gilded Case 2,500g (Royal
Emporium); a Bell
Jar (a curio) holds one more. The **exhibition** score is the display's prestige (1, 2, 4, 8 or 16 by quality; double
if enchanted). Fifteen points bring one more customer a day (only one since 0.20: a fine exhibition draws better
chests, not crowds), and each point makes a chest 1% likelier to be a colour better (up to 40%). Past 40 an exhibition brings nothing more, unless visitors pay to see it (the Visitors'
Book curio, Isolde's door, the Hearthstone).

## Stages

| Place | Cost | Customers/day | Chests | New |
|---|---|---|---|---|
| Bram's Stall, the Brindle crossroads | start | 4 | mostly wood | jingle, clank, flump; three strengths; clanks drown jingles; from day 2, haggling, naming a price, counting rattles, stories; Dorrin's rotten Barrow chests |
| Thornwick Market | 50g, once the stall has earned 50g | 6 | green and blue | slosh and rustle; locks; false bottoms and the drop; the display shelf and cabinets; Snikkit's fee, Wenna's scale and mare, warded things with Bramble's dreams and Gerda's dowries, Ansel's crypt, Fennick's straw |
| Crownhold | 160g | 6 | mostly blue, some purple | gems; two or three of a kind and the pawn ticket (Pell); sand (Shelly); enchantment and the Witchlight; synergies and the Crownhold Charter; Wren's packing tags, Orrin's wager, Grit, Corisande's riddles, Quill's ward-rolls, Sir Aldous's heirloom |
| The Royal Emporium | 1,200g | 5 | blue and purple | Vexis's hourglass, Malbranche's due, Skrae's echoes, Madame Oriel's cards, Silas's demon gold, Ermine's paste, the King's game, the Maw's gaze, the Lodestone, the Royal Seal |

Each move multiplies what a customer is worth (the sim's careful player, 0.21: about 10g of profit a customer at the
stall, 30g at Thornwick, 215g at Crownhold, 3,000g at the Royal Emporium with its trades and restorers), so a careful
player saves for the next move before anything else (`Strategy.Saving`; curios, priced mostly at a share of the purse,
don't wait on it). Besides: the Painted Sign (60g) a customer a day; the Town Crier (700g, Crownhold) one; the Night Market
(6,000g, Emporium) one; the Curio Case (500g) and the Velvet Case (10,000g) a curio slot each; the Patrons' Book
(8,000g) two more commissions a week. And the **buyers' trades**, which multiply what everything sells for: Brass
Scales (150g, Thornwick, +20%), the Auction Block (1,200g, Crownhold, +30%) and the Royal Warrant (12,000g, Emporium,
+50%), together x2.34. Sellers hear of it and ask that much more of a shop known to sell high (`GameState.Buyers` in
`Boosts.Dearer`): a trade multiplies the edge your judgment gives you, and never makes buying blind pay (before that
was priced in, buying blind at the Emporium made 600g a visit, as much as a careful read).

**The restorers** (v20; the owner asked for upgrades that make quality upgrades likelier, not only sales dearer): the
Restorer's Bench (200g, Thornwick), the Restorer's Workshop (1,500g, Crownhold) and the Master Restorer (15,000g,
Emporium) make each thing in a chest you buy 5%, then another 6% and 8%, likelier to come out a step better than its
chest (`Upgrade.Luck`, `GameState.RestorerLuck`): more surprises, and every luck curio stronger beside them. Word gets
round that things come out better at your shop, so sellers ask about half of what the restorers are worth (luck at
half in the power index, `GameState.LuckPricedIn`): with all of it priced in, a careful player who goes by the odds bar
bought less (a step better is a long tail, and the bar looks at the middle); with none, buying blind from the sellers
who hear everything (Skrae, Silas) paid at the Emporium.

**The Bookkeeping Engine** (`Upgrades.Engine`). Once your purse could pay off everything left of the debt
(`GameState.CanPayOff`), the Guild offers its counting clockwork for exactly that sum (`GameState.Cost`), first on the
day-end cards. It marks every chest's real contents on the slip as it comes to the counter (`Appraisal.Recorded`:
every real thing by kind, enchantment, a false bottom; not how good: that's still the opening), so there's nothing to
shake: a run that's already won can be finished in an afternoon, still owing the Guild everything. It can't read
through the Stranger's chains. The sim's careful player pays off instead; `--engine` starts the bot with it.
Day one is scripted: three customers, one lesson each (a gentle shake and the slip; a hard shake for a coat;
listening past a clank); the second morning opens with Mags, who heard everything there was (passing).

## Evidence (`godot --headless --path . -- --sim`, 300 runs per style, up to 9 weeks)

Each style plays whole runs as Kit at level 0 with no talents: buying chests, curios (a careful player's taste,
`Strategy.CurioScore`, keeping enough back for the Guild) and upgrades (keeping Bram's working purse), delivering
commissions, paying the Guild (pawning, or Bram's word, when short), taking Bram's stash when a day starts nearly
broke, and paying off the whole debt as soon as it can (always paying off rather than buying the Bookkeeping Engine).
These are the perfect listeners; how real players get there is in *Balance across a career*, below. v21's numbers
(five five-day weeks; v20's three-day weeks are in the git history):

| Style | Still trading after each due, weeks 1-5 | Debt cleared | Crown Chest | Foreclosed (median week) | Thornwick | Crownhold | Royal Emporium |
|---|---|---|---|---|---|---|---|
| Pays their price, every chest | 100 100 95 47 1% | 1% | 1% | all (week 4) | day 8 | never | never |
| One hard shake, keeps the seller's marks | 100 100 100 95 0% | 0% | 0% | all (week 5) | day 7 | day 23 | never |
| Every strength, drops when safe | 100 100 100 99 22% | 22% | 21% | 99% (week 5) | day 7 | day 16 | day 20 |
| + Witchlight and Lodestone | 100 100 100 99 30% | 30% | 28% | 98% (week 5) | day 7 | day 16 | day 20 |
| + keeps grand things | 100 100 100 99 35% | 35% | 33% | 97% (week 5) | day 7 | day 16 | day 20 |

("Foreclosed" counts the Guild's endless interest after the debt, which doubles weekly.)

Week by week for the careful collector (gold on the Guild's night, before paying):

| Week (day) | 1 (5) | 2 (10) | 3 (15) | 4 (20) | 5 (25) | 6 (30) | 7 (35) |
|---|---|---|---|---|---|---|---|
| Due | 20 | 60 | 300 | 4,000 | 45,000 | 90,000 | 180,000 |
| Gold, 10th percentile | 79 | 195 | 412 | 748 | 698 | 57,475 | 73,436 |
| Gold, median | 161 | 459 | 1,097 | 7,211 | 18,684 | 143,853 | 183,045 |
| Paid on the night | 100% | 100% | 100% | 99% | 25% | 78% | 53% |
| At the Royal Emporium | 0% | 0% | 14% | 49% | 78% | 100% | 100% |

The first three weeks are for growing; the fourth sends about a quarter of careful runs to Bram's word (the ones
still at Thornwick or new to Crownhold); the fifth is the wall. The careful collector pays the debt off early in 31% of
runs (cleared in week 4: 12%, week 5: 21%, the rest of the 35% on the night), fills about 11 commissions a run, opens
Bram's stash once in three runs. Profit per customer: about 10g at the stall, 29g at Thornwick, 200g in Crownhold and
2,000g at the Royal Emporium. Surprises: about one thing in four comes out a step better than its chest (one in fifty
two or more), worth 47% of what the chests held.

What curios and synergies are worth, to the careful player with both tools who sells everything:

| | Debt cleared | Median day cleared | Median gold on the night, week 2 / 4 / 5 |
|---|---|---|---|
| With both | 30% | 22 | 464 / 6,657 / 18,305 |
| No curios | 14% | 21 | 509 / 6,431 / 11,168 |
| No synergies | 4% | 22 | 464 / 5,121 / 7,876 |
| Neither | 1% | 24 | 509 / 4,517 / 5,251 |

Ordinary progression alone (every upgrade, no curio) clears about one run in seven, most of those with a Heartsong on
display; the build doubles it, and a build that comes together (three or more curios of one way of trading) clears
two to three times as often again (*Curios: builds*). Without synergies, almost nothing clears: the shop's name for a
kind (priced in) and the Keen Eye's fee for a perfect read (not) are most of a careful shop's late growth. The margins
(*The shop's power index*): the careful player's held 1.70-1.85 per gold spent at every index, the blind buyer's
1.14-1.24, the careful collector's climbing from 1.85 to 2.50 as its build comes together.

Per seller, at their own place (with the shop's tools, buyers' trades and restorers), profit per visit for a careful
player, against paying their price blind. Every habit and every rule pays for putting up with it; only Tam (a few
coins for anything) pays blind, Hob and Mags by a coin or three at the stall (Mags as in v19), and the Maw's coin pays
anyone (its price is the rest of the day, which the table doesn't count). Ansel's markup went up a tenth (1.8-2.1) when
Thornwick's chests got finer, and in v21 Quill's (1.85-2.15) and Silas's demon gold (two coins in three of the gold
and gems he sells): without the named rule, a restorer's luck reaches what a blind buyer bought, and both had tipped
into paying a blind buyer a few coins a visit.

| | Tam | Penn | Hob | Mags | Dorrin | Snikkit | Wenna | Bramble | Ansel |
|---|---|---|---|---|---|---|---|---|---|
| Careful | +13 | +5 | +11 | +7 | +2 | +11 | +13 | +25 | +22 |
| Blind | +12 | 0 | +1 | +3 | 0 | -6 | -1 | -22 | -6 |

| | Fennick | Gerda | Wren | Orrin | Grit | Corisande | Pell | Quill | Shelly |
|---|---|---|---|---|---|---|---|---|---|
| Careful | +20 | +15 | +41 | +193 | +312 | +119 | +299 | +119 | +31 |
| Blind | -45 | -32 | -16 | -14 | -1,095 | -42 | -28 | -26 | -28 |

| | Aldous | Vexis | Malbranche | Skrae | Ermine | Oriel | Silas | Aldo | the Maw |
|---|---|---|---|---|---|---|---|---|---|
| Careful | +77 | +82 | +56 | +201 | +138 | +138 | +185 | +529 | +1,850 |
| Blind | -35 | -494 | -211 | -2 | -76 | -452 | -74 | -694 | +1,699 |

The careful player plays the King's game only on a read it could check every way (every strength and the drop): a
wrong read costs a quarter more than the chest held, and Hesk, who can't shake hard or drop, never plays it. It buys
the Maw's coin when the chest's worth beats what the rest of the day would cost (what's still waiting, doubled, at the
share it would buy anyway).

Every seller's lesson completes, played the careful way, with and without the tools of their place.

## Balance across a career

v13 gave runs a meta-progression; v14 sets the balance for a career, not a run, and v15 keeps that curve through the
new world (the numbers below are v21's unless marked: runs of 25 days, an hour and a half to two hours each). The goal, in the owner's words: no
pressure before players have learned the puzzles, no easy win in the first few runs, a purse that keeps growing but not
steeply enough for the middle and late dues, and judgment and talents together pushing the curve up until a run
clears; then the challenge levels.

**Learners.** The sim's careful player is a perfect listener. A learner (`Style.Skill`, 0 to 1) plays the same way
with a learner's mistakes, each shrinking as skill grows: at skill 0 they skip each strength 40% of the time, count a
pair as one half the time, believe a seller's claim a third of the time, forget the drop half the time (and drop
glass a quarter of the time), forget the lodestone, miss a glow or a boom on the slip, judge a chest's worth with a 45%
spread either way, pick locks worse, and buy curios on a whim. Skill 0 is a first-timer who has had Bram's lessons:
they know the rules but don't yet apply them every time.

| Skill (Kit, level 0) | No talents: cleared, still trading after week 4 | Every talent |
|---|---|---|
| 0.15 (a first run) | 5%; 91% | 11% |
| 0.5 | 13%; 93% | 36% |
| 0.75 | 21%; 97% | 62% |
| 1 (the careful collector) | 40%; 99% | 79% |

(A first run gets through the first three weeks, needs Bram's word in the fourth about half the time, and falls in
the fifth; it clears about one time in twenty, nearly always with a Heartsong on display; without Heartsongs the
careful player clears a little over a fifth of the time. v20's were 5%, 13%, 24% and 48% with no talents, 27% to 85%
with all.)

**Careers.** Each simulated player starts at skill 0 to 0.3 and improves toward their own best (0.75 to 1) at their
own pace (a tenth to three tenths of the way each run); every run is Kit's at the highest level open, scored at the
end of the debt's last week or when the shop is lost, and its renown spent on the cheapest talent going. The pace of learning is a
guess; what the sim shows is the shape (`--sim`, 40 players over 30 runs):

| Run | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 12 | 16 | 20 | 24 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Skill | 0.15 | 0.29 | 0.41 | 0.50 | 0.57 | 0.63 | 0.68 | 0.71 | 0.74 | 0.77 | 0.80 | 0.84 | 0.86 | 0.87 |
| Cleared | 3% | 5% | 5% | 8% | 5% | 10% | 20% | 28% | 28% | 35% | 30% | 38% | 35% | 20% |
| Lost in week (median) | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 |
| Challenge level | 0 | 0 | 0.1 | 0.1 | 0.2 | 0.3 | 0.4 | 0.6 | 0.8 | 1.1 | 1.8 | 3.0 | 4.3 | 5.5 |
| Talents (of 12) | 1.6 | 2.9 | 3.5 | 4.3 | 5.0 | 5.7 | 6.3 | 6.8 | 7.4 | 8.1 | 9.2 | 10.8 | 11.8 | 12 |
| Renown | 163 | 175 | 161 | 189 | 195 | 229 | 245 | 294 | 328 | 395 | 409 | 547 | 630 | 589 |

v21: a run that's lost is lost in the last week (the fourth sends half of the first runs to Bram's word, the fifth
is the wall); the debt is first cleared around the eighth run (a quarter of players by run 6, half by 8, three
quarters by 11). Then the levels: level 1 around run 11, 2 around 14, 4 around 20 (all by run 30); level 6 is reached
by about half of players by run 30, and level 8 stays a wall (nobody by run 30). All nine Heartsongs are known around
run 17 and the Crown Chest is first sung whole around run 19. (v20: first clear around run 7, level 4 around 16, level
6 around 26.)

Getting there: v13's dues (40, 100, 300, 800, 1,400, 2,400, 3,800, 7,000) let the careful player with no talents
clear 61% of the time and half of all careers clear by their fifth run, with a third of first-timers already short in
week 3 (Bram's word then, his interest sinking them in week 4). Raising the late dues alone (to 10,000) made it harder
without making it gentler; making week 7 heavy did nothing (anyone short in week 7 leans on Bram's word, and its
interest lands on week 8). Lighter early dues with a heavier last week did both.

**The challenge levels**, ordered by what each term does alone to the careful player with every talent (81% on the
usual terms; v15's figures): Bram stretched thin (81%), demon gold about (73%), fussy sellers (72%), thin crowds at
the stall (69%), a lean start (67%), the Guild's interest (67%), sharp sellers (61%), a hurried town (59%). The new
world's hurried town is the harshest (twenty seconds for a goblin's fee, a rotten chest and the King's game), so it
moved from level 6 to 8; the rest kept their places (within a few points of each other, which is noise at 150 runs).
All together, level by level: 81, 81, 72, 67, 61, 43, 31, 13, 6%. The first levels are an on-ramp for a careful player
and bite a learner who still leans on Bram's stash and word and drops glass.

**Bram's kin** on the new curve (the careful collector, 150 runs each; cleared %; v21):

| | No talents | All, level 0 | Level 2 | Level 4 | Level 6 | Level 8 |
|---|---|---|---|---|---|---|
| Kit | 40% | 79% | 77% | 55% | 25% | 7% |
| Hesk | 40% | 73% | 73% | 57% | 23% | 10% |
| Rosa | 40% | 81% | 81% | 69% | 31% | 19% |
| Isolde | 46% | 81% | 76% | 57% | 23% | 13% |

A full talent tree moves the median clear day from 21 to 18 (the shop's growth is the build's, not the talents'),
and the four of Bram's kin are within a few points of each other (v20: Kit 48 / 85 / 76 / 66 / 24 / 16%; Rosa's
haggling and the Golden Tongue suit each other, which keeps her a little ahead at the top). (v19: Kit 29 / 61 / 66 / 42 / 15 / 5%; v18: Kit
42 / 75 / 68 / 53 / 23 / 9%. v19's warded puzzles first held Hesk back, 19% with no talents and 35% with
all: the careful player only called a named kind silent once every strength had been tried, and he never shakes hard.
It now asks only that the named kind had a fair chance to sound, as a player would: a dream of a river and no slosh
at a gentle shake is enough.)

v18's bigger economy shrugged off the old terms (level 8 cleared 31% with every talent), so they're sharper: the
Guild's interest a quarter (was a tenth), sharp sellers a tenth (a twentieth), thin crowds at every shop (the stall
only), demon gold one coin in ten (twenty). Isolde's door money grows with the shop at a quarter of its pace
(`Characters.IsoldeScale`: flat, she cleared 26% with no talents; at the shop's pace, 62%), and Rosa's hagglers no
longer open higher for her (a seller who stays away a week is price enough when an Emporium regular is worth
thousands).

v14 set Rosa's wariness at a twentieth; v15's sellers who'd name a cheap price (Dorrin, Wenna, Malbranche) made
haggling dearer for her than their price, so now a seller who'd name a price never holds out for more than it with
Rosa, and her wariness is 3% (`Market.RosaWary`, `ConvertedAsk`, `ConvertedFloor`). Hesk loses the King's game (he can
never be sure enough of a read to play it) and wins at the top, where he shrugs off two of the terms (he never drops,
and his scale catches demon gold).

## Playtesting the experience (`--sim=experience`)

The owner asked for automated playtests that judge a player's experience, not just the economy: where players repeat
the same thing too long, where too much arrives at once, the peaks and the boring stretches, across a whole journey
from knowing nothing to a second and third run. `Simulation.Experience.cs` plays 30 learners through their first three
runs (Kit, tips on, skill 0.15, then 0.3, then 0.42; the profile, its talents, its songbook, what Bram has taught and
Nim's stock carry over) and records each run as **beats** (a customer, a morning, Nim, an evening, the Guild's night, a
scene), each with:

- **minutes at a human's pace**: twelve seconds a customer, more for each strength tried, the lodestone and the drop,
  haggling and naming a price, half a minute for a lesson, the opening's show (longer for surprises, a ward, a
  Heartsong, a lock), scenes by the line, Nim and the evening by what was bought;
- **novelty and load**: each thing met for the first time (a seller, a mechanic, a shop, an event, a curio owned, a
  build taking shape) is new, weighted by how much there is to learn about it; met again, it's familiar; taught by
  Bram, it weighs a little less; a lesson replayed in a later run is flagged;
- **peaks**: a surprise, a legendary surprise, a jackpot (three times the recent median profit), a commission done, a
  ward given up, a Heartsong, a curio's luck, a new shop, a story reveal; each wears off with repetition;
- **choices**: a chest whose worth is close to its price (a real decision), a seller's way of trading, Nim's cards you
  can afford, an evening's upgrades and synergies; the same seller's decision weighs less each visit.

From those it reports, day by day and per run: minutes, what's new, load (over 6 in a day is too much at once),
novelty, peaks, choices, an engagement score, and **stale stretches** (eight minutes or more of customers with nothing
new, nothing high and nothing to decide, on a moving average so one surprise doesn't break a long dull run), then what
to change.

What it found in 0.19, and what 0.20 and 0.21 changed (medians over the 30 learners):

| | 0.19 | 0.20 | 0.21 |
|---|---|---|---|
| Run 1: length | 172 minutes, 30 days, 238 customers | 109 minutes, 24 days, 134 customers | 104 minutes, 25 days, 141 customers |
| Run 1: players with a day over capacity | 100% (heaviest day's load 7.9, day 4) | 50% (6.2, day 5, at Thornwick) | 33% (5.7, day 1) |
| Run 2: length; new things an hour | 169 minutes, 35 days; 3.5 | 102 minutes, 24 days; 8.9 | 101 minutes, 25 days; 7.2 |
| Run 2: time in stale stretches; lessons replayed | 13% (longest 14 minutes); 20 minutes of them | none; none | none; none |
| Run 3: length; new things an hour | 303 minutes, 40 days, 402 customers; 2.3 | 82 minutes, 24 days, 112 customers; 6.3 | 103 minutes, 25 days, 147 customers; 6.1 |
| Run 3: time in stale stretches; lessons replayed | 10% (longest 11 minutes); 29 minutes | none; none | none; none |
| High points an hour, runs 1 / 2 / 3 | 5.9 / 5.6 / 5.9 | 8.8 / 10.5 / 11.2 | 6.7 / 7.2 / 8.2 |

0.21's five-day weeks put Thornwick's first day at six customers, and three first-timers in five had a day over
capacity there (6.4); with tips on, a grander shop's first day is now a quiet one, a customer short
(`GameState.QuietOpening`), and the heaviest day is the first (5.7). High points are fewer an hour than 0.20's: the
same peaks over a little more trading, and a learner's builds come later.

(A strong player's full run, all 24 days with the Emporium's crowds, takes about two hours.)

The changes it drove: three-day weeks and fewer customers (*The run*), the first day's last lesson and Vane's debt
moved to the second day and Nim's first visit to the third, one newcomer a day at Thornwick, Bram's teaching kept
across runs, no event in the first week (and two a week only from the fourth, when the crowd is familiar), and Nim's
stock growing run by run so a later run meets curios the first didn't. What it still says: Thornwick is the busiest
stretch of a first run (locks, the drop, straw and warded things all arrive there; half of first-timers have one day
just over capacity), and a third run still brings far less that's new than a first (6 new things an hour against 43;
its longest stretch with nothing new is six days): a learner who hasn't reached the Emporium meets the same shops
again. The next lever is content only later runs meet (Bram's kin, challenge terms,
the unlocked curios), and more ways for a familiar shop to play differently.

## What was cut, and why

1. **Auction (v1).** The game did the judging, and bidding against one bot had a dominant strategy.
2. **One unique sound per item (v2).** 28 sounds to memorize, with no logic connecting a sound to a thing.
3. **Sound families with variants, many tools, haggling, lockpicking (v3).** Too much at once, and profit came from
   habits the player couldn't see.
4. **v4** rebuilt the puzzle on shake strength. It was clear, but every profit was predictable and every seller
   alike. **v5** keeps v4's three rules and adds variety that follows its own believable rules: quality rolls around
   the chest's colour, three ways to trade, sellers with one habit and one reward each, the drop, lockpicking, and a
   collection worth keeping. **v6** gives each seller their own way of judging a chest, so each visit is a different
   puzzle. **v7** adds layers of deduction one at a time (padding and the ear trumpet, counting, sand), a lesson for
   each seller's first visit, a gut feeling instead of opening a chest you passed on, openings that build to their
   surprises, kinder quality odds with longer upward tails, a slip that reads at the chest's colour, and synergies
   that give the collection goals. **v8** makes padding certain (any cloth pads, so the rule never seems to misfire),
   makes enchantment something any thing can be, replaces "worth about" with an odds bar, colours the gut feeling,
   shows a worse-than-expected find as a letdown, and stops lessons and tips talking over each other. **v9** drops
   padding, opens the shop up a step at a time (renovations once the stall earns 120g, the display shelf with the
   village shop, synergies with the town shop, each introduced by Bram), gives every shop its own look, and adds the
   title screen, daily saves, pause menu and settings. **v10** turns the shop into a run: the Grey Guild's weekly dues
   (eight weeks, then endless), curios from Nim, commissions, an event each week, the Stranger's sealed chests and the
   gilded ones, and the story of King Aldric's vault. **v11** makes surprises rare (with a pity counter for long dry
   spells), adds Bram's secret stash as a beginner's safety net, guides early spending (a working purse, orange
   prices), makes commons 1g at Nim's, chains the cabinets and synergy slots one after another, keeps the day's
   event in the top bar, lets you choose each kind's sound, and, after a beginner's playtest, shows how hard you're
   shaking.
   **v15** rebuilds the world around the game (`docs/world.md`): the Locking explains the chests, the Key-mark why
   only you can open them, the Gilded Guild's tithe the debt, the Aldric Key and the Crown Chest the story; the shop
   moves inward through four places; the kinds become coin, arms, garb, potions, scrolls and gems; every chest has an
   origin and every seller a story about it, true or tall; and the cast is rebuilt by place, with rules of their own
   (a rotten chest, a fee for shaking, a wager, an heirloom, a devil's due, the King's game, the Maw's gaze).
5. **Opening a passed chest (v5 to v6).** Immediate feedback, but it made no sense: they're taking it home. The gut
   feeling keeps the feedback and keeps the chest closed.
6. **Cloth padding (v7 to v8).** Cloth packed around hard things made them sound one shake harder (with an Ear Trumpet
   to hear through it). First it was a hidden roll, so it seemed to work only sometimes; made certain in v8, it was
   still too unintuitive to reason about. Gone in v9, trumpet and all; Granny Wick's puzzle is now her sewing.
7. **Magic as its own kind of thing (v5 to v7).** A magic thing had its own name and sprite and showed one colour
   better than its quality, so a purple chest's "gold" find could arrive without the upgrade fanfare, and its picture
   didn't match anything in the journal. Enchantment is now something any thing can be.
8. **The Royal Warrant as the goal (v5 to v9).** A 5,000g upgrade and an exhibition of 40, with nothing pressing until
   then: the days felt the same from the town shop on, and nothing ever went wrong. v10 replaces it with the Guild's
   weekly dues (pressure that climbs), curios, commissions, events and a story with a vault at the end.
9. **Percentage bonuses from curios and synergies (v7 to v11).** +50% on coins and jewelry (or metal, or cloth and
   paper), +50% on everything with a cursed crescent, a fifth off every due, +3% a level on every sale, a fee of 3% a
   level of what a perfect read's chest held, commissions +50%, one chest in six a step better. A careful buy's margin
   is a few percent of what the chest holds, so a few percent more on every sale multiplied how fast the shop grew:
   the sim's careful player cleared the debt 69% of the time with curios and synergies and never without, and the
   dues had climbed to 26,000g to keep up. v12 makes curios rule-benders and synergies a name that brings chances
   your way, and sets the dues by what judgment earns: take every curio and synergy away and a careful player
   finishes about a day later. **v20 brings shares back, on a condition:** the owner wanted builds to be the heart of
   a run, with strong effects and dues that scale with them. What made v7-v11's shares a problem was that they paid
   whatever you bought; v20's pay only on what your slip named (the named rule), so they multiply judgment, a misread
   loses them, and a blind buyer with the best build is still foreclosed (`--sim=builds`). The dues climb to meet them.

10. **Made-up sounds and sworn false bottoms (v6 to v14).** Tobias the Tinker claimed a sound that wasn't there, and
    Captain Gull always priced in a false bottom. Both were the same puzzle as everyone else's (check it yourself) with
    a hidden roll on top. v15 moves unreliability to where people really lie: where a chest came from. The place is a
    clue (a barrow holds grave goods), the story is true as often as the seller is honest, and the truth comes out when
    the chest opens. The old cast went with the old world: Sergeant Brask's weight is Wenna's, Old Marta's forgotten
    false bottoms are Fennick's smuggling chests, Granny Wick's unheard sewing is Brother Ansel's vestments, Madame
    Zara's trick boxes are Lady Corisande's, Baron Vex's clock is Vexis's hourglass, Pip's sharp ears are Skrae's, and
    Magister Orrin's dreams went to Mother Bramble while Orrin took up a wager.

11. **Lies about where a chest came from (v15 to v17).** The owner, again: "figuring out whether the seller is lying"
    is a false premise; it never changed how anyone played. v18 makes every story true, drops the honesty tally and
    the tall tales, and gives the sellers who leaned on it a clue worth working out instead: Fennick's straw (a strength
    later, and soft things never), Pell's ticket (a count, false bottom and all), Wenna's scale (a weight, and a mare
    who won't be shaken hard) and Mother Bramble's dream (the dearest thing, as a sign). The Drowned Bell, which tolled
    for lies, now counts every chest.

12. **Tight numbers (v12 to v17).** Keeping every curio, synergy and talent off sale prices, and the dues to what
    judgment alone earned, kept the balance honest but made upgrades feel optional, Heartsongs a percent here and
    there, and growth flat. v18 lets the shop grow (qualities 1/3/10/30/100, buyers' trades, synergies on sale prices,
    powerful Heartsongs) and the dues climb as fast; it keeps two guards: sellers price your trade in (so buying blind
    never pays), and talents still add nothing to sales.

13. **The named rule (v20).** What a curio, synergy or restorer added, it added only to what the slip you bought it on
    named, so a build amplified judgment and a misread forfeited it. It worked, but the owner found it clumsy: a bonus
    that might or might not apply depending on a count nobody could check until the chest opened. v21 lets everything
    reach everything in the chest and keeps the same guard in the numbers: whatever makes ordinary things fetch more goes
    into the shop's power index, and every seller asks that much more (*The shop's power index*).

14. **Three-day weeks (v20) and the pure share of the purse (v20).** Three-day weeks shortened the run but crowded the
    week (a tax night every third evening, Nim once a week, events squeezed in); v21 keeps the working week and cuts the
    weeks instead. A curio priced at a share alone cost a coin or two on Nim's first visit and a rare could be had in
    week one; v21 adds a fixed sum by rarity and keeps rares and legendaries for later weeks and grander shops.

15. **Bram's Ledger (talent, v13 to v20)**, which showed every regular from the start. With the journal kept across
    runs there was nothing left for it to do; profiles that had it get their renown back.

16. **v20's working purse** (20, 40, 250 and 2,500g by shop). It didn't follow what an upgrade does to prices, so an
    auction block could leave a shop that had kept exactly that much unable to buy a chest for days. v21's is two of the
    shop's typical chests at what sellers ask now, worked out again for every purchase.

## Open questions

- Heartsongs: is a sound you've never heard, then a reveal, the heart-racing moment it should be, and does "highly
  recommended" read as advice rather than an order? About one run in two finds one: rare enough, and is a find that
  can win a run a thrill or a fluke?
  Is the songbook (nine, across runs) a goal people chase, and is the ending worth it?
- The stories: do players read them, and use a place as a clue (a barrow's grave goods, a goblin warren's coin)? Do
  the stated clues read as puzzles (subtract the heard from Wenna's weight, count against Pell's ticket, read the
  straw a strength late, decode Bramble's dream), or as homework?
- v18's numbers: does growth feel exciting from week 4 on, and does the Bookkeeping Engine feel like a reward for a
  won run, or like skipping the game?
- The new rules: is Dorrin's two seconds readable with the countdown over the chest? Does a goblin's tenth feel like a
  real choice? Does the King's game (a quarter off, or a quarter more) feel like a test or a trap for a learner? The
  Maw comes one day in ten: is "everyone after him asks twice as much" a thrill to weigh, or a coin nobody can refuse?
- Hesk never plays the King's game in the sim (no hard shake, no drop: he can't be sure of a read). Is that a fair
  cost of his back, or should the scale count as checking?

- Are curios still exciting next to the bigger upgrades? Which do players pick, and does anyone take the token or the
  crescent?
- Paying off early: does a week-4 finish feel like the reward it should, or like the story ending before Mara's last
  piece? (Keep trading, and her last ask still comes.)
- Is drag speed a good stand-in for shake strength with a real mouse? The thresholds need playtesting; the words under
  the chest now show where you are.
- Vexis's twelve seconds: enough to shake gently and hard and decide, but is it fun or just stressful?
- Should displayed things ever sell for more than their price (a famous piece)? Today they only draw customers.
- Is counting by ear (and by the words over the chest) readable with four or five things rattling at once?
- Wenna, Mags and Mother Bramble pay a little even blind (a fair price for a heavy chest, a rattle's price for a purse,
  a cheap dream). Kindly sellers, or holes?
- Synergies reach level 4 within 30 days for most careful players; level 5 is a long goal. Is the pace right?
- The odds bar takes the arithmetic away but not the read: it only knows what you ticked. Does it still leave room
  for a hunch (paying a little over the odds for a chest you suspect holds more), or does everyone just follow it?
- Enchantment is now x2 instead of x4: still exciting enough to hunt with the witchlight?
- The shop saves at the start of each day only, and sellers don't remember you.
- A week at the stall before the first renovation (the careful player moves on day 7): long enough to learn the three
  rules, or too long?
- The sim's careful player is a perfect listener: real players will clear the debt less often. Is 45,000g in week 5
  a wall people want to climb again, or a cliff? Bram's word (once a run) and his stash (twice) are the safety nets;
  are they enough for a beginner, and too much for a good player?
- v21: does Nim's leaning (always something for what you bought last) make a build feel chosen, or handed out? Does
  anyone build sight unseen, and does a run where the puzzle stops mattering feel like a reward or like the game going
  away? Does the red price and its warning help a player keep a working purse, or nag? Is the curio page a collection
  people want to fill, and are the legendaries' feats goals or chores?
- Curios are chosen by a fixed taste in the sim: which ones do players actually pick, and do any feel useless (the
  knife, the hook)?
- Bram's kin: is Hesk's arithmetic (the scale against the slip) a puzzle or a chore? Does Rosa's haggling feel like
  her own game, with sellers asking her more? Does Isolde's door money make keeping things a real choice from the
  stall on?
- Careers: the learner model's mistakes and its pace of learning are guesses. Playtest how many runs a first clear
  takes (the sim says about eight) and whether losing in week 5 several runs running feels like "nearly" or
  like a wall. The talents lift the careful player from 41% to 79%; should the tree be a loadout instead?
- With the steeper late dues, the exhibition matters: a careful player who sells everything clears 21%, one who keeps
  grand things 44%. Is keeping things taught early enough (Bram's "keep" tip) for that to be fair?
- Challenge levels 1 to 3 barely trouble the careful player (they bite people who use the stash and the drop); is
  that a gentle on-ramp or free renown?
- The Crown Chest usually comes with clearing the debt for a careful player (the Hooded Woman's asks are met by volume at
  the Emporium). Should her asks be harder, or limited to their own week?
