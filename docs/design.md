# Keywright design notes

## The game in one paragraph

Three hundred years ago the Locking shut every treasure in the realm of Aldmark into a warded chest, to starve the
demon Avarax, the Gilded Maw. The chests are still everywhere, and only a Keywright can open one, once it's rightfully
theirs: everyone else sells them unopened, and the ward lets nothing through but sound (the whole world is in
`docs/world.md`). You're one of Bram's kin, with the Key-mark, and Bram hands you his stall at the Brindle crossroads.
People bring you chests and price them their own way: some shake them and price what they heard, some go by the
weight or the chest's colour, some go by a trade they know, some dream up a number, and each has a rule of their own (a rotten
chest, a fee for every shake, a wager, a clock, a claim on what's inside). They tell you where the chest came from, and
what they tell you is always true. Shake each one yourself (gently, firmly, hard), drop it to listen for a false bottom,
count the rattles, and work out what they missed or misjudged. Mark what you think is inside on your
appraisal slip and trade the way each customer trades: pay their price, haggle, or name your own. Pass, and your gut
tells you whether you were right; buy, and the chest bursts open on the spot: each thing's true quality, a tick or a
cross against your slip, and now and then something far better than the chest promised. What you acquire builds the
shop's name (synergies) and its exhibition, which draw more and better customers. The stall comes with Bram's twenty
years of unpaid tithe to the Gilded Guild: every fifth evening Mister Vane collects a due, 40g, then 170g, 700g and
6,600g, and each one paid moves the shop a market further in: a season is four weeks, one each at the Brindle
crossroads, Thornwick Market, Crownhold and the Royal Emporium by the Gilded Gate (twenty days, about an hour and a
half of play; 0.36, *The run*). Listening costs nothing (0.37 took 0.36's counted listens out again: *Listening is
free, and a build does the routine*). Curios from Nim the peddler are the run's
build: each bends a rule of the trade, the ones that share a way of trading (enchantment, sight unseen, haggling, the
perfect read, false bottoms, wards, and more beside) make each other stronger, and Nim, who notices what you buy,
brings more of it. Since 0.37 a build also changes what the counter asks of you: each way of trading has a curio that
does its share of the routine (a quill that ticks what's heard, a gauge that finds the false bottom), and Bram's
cradle does the shaking once you've learned what the strengths tell. Whatever makes ordinary things fetch more puts up what every seller asks as well (the shop's power
index), so the shop's growth makes the numbers bigger without making a bad buy good; the last week's due is more than
ordinary trade can make, and a build that has come together is what carries it. Regulars pay well for commissions,
and something different happens in town two days a week. Bram minds the stall with you for the first week; then the
Guild's treasury sends for him, and the Guild's assessors send the four pieces of the treasury key for four marks of the
trade a careful ear makes. Pay every due, or the whole of what's left at once whenever you have it, and Bram's account
is closed; with the key whole too, the Guild's carriage takes you to its treasury, where the great chest is the Maw
(*The story*, below). Fall short and the Guild seals the shop.

Across runs, four of Bram's kin can mind the stall, each reading chests or running the shop their own way: Kit listens,
Hesk weighs, Rosa haggles, Isolde collects. Clearing the debt with one opens the next of eight challenge levels for
them, each adding one of the Guild's harsher terms. Every run ends with a score, times the challenge level's
multiplier. What carries over is what you know, and since 0.37 the trade's ways with it: a later season's chests have
every layer you've met from its first morning (locks, false bottoms, wards, two of a kind, enchantment), nothing Bram
has told you is told again, and he turns out his drawer for you (*A season after the first*). Since 0.34 that's
also the story's: after the treasury first swallows you, every season is the same spring again, you remember, and the
notebook's Inquiry keeps what you've found out, until a season ends with the treasury door closed on the Maw and
everyone the Guild took brought out.

## The world

`docs/world.md` is the reference for anything that speaks, sells, falls out of a chest or happens in a scene: why the
world is full of chests (the Locking), why people sell them unopened (only a Keywright can open one, once it's theirs),
why you can (the Key-mark), and what opening them does (coin for the realm, treasure back in the light, and every chest
a little looser on the Maw's chains, which is why the Guild wants you opening them). Every rule has a reason you could
tell a stranger in a tavern: the ward lets only sound through, so everybody shakes and guesses; the old iron lock under
the ward still needs picking; people hid their best things under a false bottom when the Wardens came round; demon gold
is iron under the gilt, so the lodestone silences it.

The shop's four tiers are four places, a journey inward from the edge of the realm to the Gate at its heart, and each
brings people who belong there: a millpond boy, a soldier's widow, a farmer, the innkeeper and a young delver at the
crossroads; a goblin, a bounty-huntress, a druid, the abbey's almoner, a smuggler, the matchmaker and the Magpie's
runner at Thornwick; an archmage and her apprentice, a dwarf, a noblewoman, a knight, the pawnbroker, the Archivist and
a mudlark in Crownhold; demons, a countess, a cartomancer, the Maw's fence, the King in disguise and once in a while
the Maw himself at the Royal Emporium. Since 0.28 every one of them is someone, with a faction, a want and a story that
goes on between visits (below, *The people and their tales*). Every chest has an **origin** (`Origins`: the millpond, the Barrow, a goblin warren, the
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
3. **Sellers price only what they think is there.** Each has their own way of judging (below). Nobody says what they
   heard (v22): the slip starts empty, its header says how the seller came to their price ("Ada priced what a gentle
   shake stirs"), and working out what they priced is yours, by shaking the way they did.

Then, one at a time as the shop grows, more layers of deduction, each taught by the seller it belongs to:

| Layer | From | The rule | How you tell |
|---|---|---|---|
| False bottoms | Thornwick | a sealed hollow under the floor never rattles | drop the chest: a hollow BOOM (glass may break) |
| Two or three of a kind | Crownhold | each thing sounds from its own spot, so more of them rattle more often | count the words over the chest; most sellers never count |
| Sand | Crownhold | a scroll case of river sand rustles just like a scroll, but pours at the gentlest shake; parchment never does | a rustle at a gentle shake is sand |
| Enchantment | Crownhold | anything you can hear can be enchanted (worth double); it sounds like anything else. A hidden thing (under a false bottom, or warded) never is: it fetches a third more instead | under the **Witchlight** it glows |
| Demon gold | Royal Emporium | gilded iron jingles and tinkles like gold and gems | near the **Lodestone** iron can't move: a fake goes quiet |
| Mimics (0.33; since 0.34 only once you remember the treasury) | Thornwick | a mimic ate everything in its chest; nothing rattles, so a seller who listens thinks it's empty | it **breathes** while the chest sits still (and holds its breath while it's shaken), and never rings the way an empty box does at a firm shake (0.34): listen before you shake |

Sound-alikes (sand, demon gold) are the deductive heart of the later shops: the sound tells you the kind of thing,
but only the rules tell you whether it's real. Soft things only sound at a hard shake, which hides them from anyone
who won't shake hard (most sellers) or isn't allowed to (Brother Cadoc's crypt chests: take the vestments on faith,
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
inside rolls its own quality around that: 77.5% as promised, 9% one worse, 11% one better, 2% two better, 0.5%
three better (what would fall below common or above legendary stays at the end). Until 0.36 it was 16% worse and 7%
better, so the commonest surprise at an opening was "WORSE..."; now one thing in seven comes out better and one in
eleven worse. Sellers price the average, so chests cost a little more and a careful buyer's margin is what it was. Value
is the kind's base times 1, 2, 4, 8 or 24 by quality (0.30; v18 to 0.29: 1, 3, 10, 30, 100; v12 to v17: 1, 3, 8, 20, 50).

**Gold is rare** (0.30). The owner found gold things too common late in a run, and every step inward three to ten times
the last. So each colour is now worth twice the one below, and gold three times purple, but:

- a chest's own roll makes a thing gold only 8% as often as the spread above says (`Categories.GoldShare`): about one
  purple thing in two hundred, and the rest stay purple;
- **luck never makes gold** (`Categories.Lift`): restorers, luck curios, a dry spell, a hoard's better things and the
  Chimes lift things as far as purple. A thing is gold when it went into the chest gold: no restorer can make a legend.
- Instead, luck that would have lifted a purple thing past purple makes it **shine**: it comes out enchanted (worth
  double, `Build.PastPurple`), priced into the index (at half, as all luck is) and into the slip's value of a purple
  thing (`ShineShare`), so a luck build still pays at the Emporium. The Master Restorer mends instead of lifting
  (nothing comes out worse than its chest), and Tick's hoard is more often blue, which can come out better.

A careful run finds a gold thing or two (the sim's count is on the main table's "gold things a run" line), and Bram's
tip says so the first time. (Until 0.34 the Crown Chest's nine-thing opening held five more.)

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
- **Rare, and rarer for each one kept** (v22). Any chest a seller brings (not a lesson's or the first day's) has a
  0.24% chance (`Heartsongs.Chance`), rolled with its own dice so nothing else changes, and each one kept this run makes
  the next a quarter as likely (`Heartsongs.Fade`, `GameState.SongsKept`). That's about a third of a Heartsong a run
  (`--sim=songcount`; 0.36, at 0.32% a chest over a season of about a hundred: 0.37 for the careful player while the
  debt lasts, 0.26 for a first run; the chance was 0.16% over 0.30's 35 days): a third of runs see one
  (0.21's 0.7% was nearly one a run, and they stopped feeling rare). One you've never found is three times likelier;
  never two of the same in a run. At this pace all nine take a long career (one player in eight by run 30).
- **Worth nothing in gold** (v22). No merchant would buy one and no seller knows it's there: it adds nothing to the
  slip, the sale or the gut, and the Guild won't take one in pawn (`Heartsongs.BaseValue` is 0). A careful player still
  pays up for a chest one sings in (the sim's: up to a working purse over what the rest is worth), because of what it
  does on display.
- **Its own moment, and a choice** (`SongReveal`, v22). When one comes out, everything stops: the room goes dark, the
  song plays, a pillar of red light stands on the chest and the thing rises out of it into turning rays, then a hit of
  white, shockwaves, sparks, lightning and confetti as the banner slams in. Its name, who gave it, its story and what it
  does on display stay up, with **Keep it** and **Decline**, until you choose (it sings again now and then while you
  think). Kept, it flies up to sing in the shop, free: no shelf, no slot, no gold. Declined, it comes apart into red
  motes and goes back to the Locking; it's in the songbook either way. Declining is a real choice: each one kept makes
  the next rarer, so a player after a particular song may let another go.
- **It won't leave with its seller** (v22). Pass on a chest you can't afford while a Heartsong sings in it, and the
  chest shudders and bursts: the song comes out to you alone, with its reveal and the same choice, and the seller goes
  off with everything else (`GameState.Bursts`, `Burst`). What you can afford and pass on goes with them.
- **On display.** It needs no shelf and takes no slot (it hangs on a cord over the counter, at any shop, even Bram's
  stall; hover it there for what it does, click it to let it go). It adds no exhibition points: each does one big thing
  instead. They're the rarest things in Aldmark, so each is worth a run built around it (v17's were a percent here and
  there, and a find felt like nothing). None shows what a chest is worth: the Bell counts, the Star lights up
  enchantment, the rest is still yours to work out.

| Heartsong | On display |
|---|---|
| The Lullaby Box | it calms every seller: nobody hurries you or minds what you do with their chest (kid gloves and a pocket watch at once), and everyone asks a fifth less |
| The Shepherd's Pipe | two more customers every day, and anyone who walks out comes back at the end of the day, at their first price (once) |
| The Fey Chimes | a third of what comes out as its chest promised comes out a step better |
| The Dragon's Egg | coin and gems sell for twice their worth |
| The Clockwork Lark | two more commissions every week, and every commission pays half as much again |
| The Hearthstone | door money every night: 10g a point of exhibition at the stall, more in a grander shop (`ShopTiers.Scale`: x2, x4, x7 since 0.30; x3, x8, x20 before) |
| The Drowned Bell | as a chest comes to the counter it tolls once for every thing inside, false bottom and all: Brass's ticket on every chest |
| The Bottled Star | enchanted things glow whenever they sound, with or without the witchlight, and are 15 points likelier |
| The Imp's Ember | glass never breaks, dropped or forced; nothing comes out worse than its chest promised; every fake you open (demon gold, paste, sand) turns real |

Tuning (`--sim=songs`, the careful collector, v22): on display from the first day, against 48% of runs cleared with
none, the Lullaby Box clears 94%, the Fey Chimes 93%, the Shepherd's Pipe 85%, the Clockwork Lark 83%, the Dragon's
Egg and the Imp's Ember 75%, the Hearthstone 65% and the Bottled Star 58% (a curio from day 1: a Lucky Coin 55%, a
Four-Leaf Clover 51%). The Drowned Bell is the exception in the sim (46%), because the sim's careful player already
counts perfectly; for a person it's a Brass's ticket on every chest. As they really turn up (in about a third of runs,
the first around day 15), the careful player clears 48%, against 43% with no Heartsongs at all.

0.30 (seven weeks; against 56% cleared with none): the Lullaby Box 99%, the Shepherd's Pipe, the Fey Chimes and the
Dragon's Egg 97%, the Clockwork Lark 93%, the Imp's Ember 86%, the Hearthstone 85%, the Bottled Star 75% and the
Drowned Bell 62% (a curio from day 1: a Lucky Coin 68%, a Letter of Introduction 71%, a Four-Leaf Clover 67%). The
longer season brought more chests, and at 0.22's chance a chest (0.24%) half a Heartsong a run while the debt lasted;
at 0.16% it's a third again (`--sim=songcount`: 0.33 a run, a first run 0.21), the first found around day 25.

**The songbook.** Bram's kin remember every Heartsong they've opened (`Profile.Songs`, the collection's Heartsongs
page, and the "Sung Whole" achievement for all nine), and each found is worth 20 points. Since 0.34 that's all a
songbook is: the Heartsongs keep their display effects, and nothing is gated on all nine. Until then the Crown Chest
held the Locking's tune and the Heartsongs its verses: opened with all nine known, Mara sang the Locking whole and every
chest in Aldmark opened (`Story.VaultEpilogue`, worth 200 points), and in the sim's careers all nine were known around
the sixteenth run and the first Crown Chest sung whole came around the seventeenth.

## Ways to judge a chest

Each answers one question by a rule you'd expect from real life. All of them are perceptions; nothing states a value.

| How | From | Tells you | Cost or risk |
|---|---|---|---|
| Shake gently, firmly, hard (by hand, or on Bram's cradle: below) | start | which kinds are inside, and how many of each | Brother Cadoc hates a hard shake; Tobin's chests stand two seconds of it; Nettle charges for every strength after the first |
| Drop it (lift it high, let go) | Thornwick; the first morning of any season once one has traded there (*A season after the first*) | a hollow BOOM means a false bottom | glass may break (half the time each); Brass hates it |
| Witchlight (200g) | Crownhold | always lit once bought: anything enchanted glows when it makes a sound | none |
| Lodestone (400g) | Royal Emporium | iron can't move: no clanks to drown things out, and demon gold goes quiet | Countess Vey hates it |

**Left out: the scale.** A weight only means something if you add up what each thing should weigh in your head, which
isn't intuitive, and the drop already gives the one weight-like fact that matters (hollow or solid). Weight comes back
as a seller's habit instead (Rook Varga goes by it, and the notebook says which kinds are heavy), and as
Hesk's way of reading a chest.

**Lockpicking** (from Thornwick) comes after you buy a locked chest: set each pin when the pick passes the
green. A masterful pick (no slips, mostly perfect) makes everything worth 25% more; the crowbar is quick but scratches
everything (-10%) and breaks glass. Fine Lockpicks (50g) widen the sweet spot.

## Listening is free, and a build does the routine (0.37)

**The counted listens are gone.** 0.36 gave a keywright's ear eight careful listens a day: every new check on a chest
(a strength, a drop, the lodestone) spent one, so that which check to make would be a decision. The owner played it:
"The hearing-ability mechanic serves no meaningful purpose. Repetitive gameplay is a real issue, but restricting how
many times players can listen is not the solution." It wasn't. At the stall it never pinched (0.36's playtester ended
its days with listens unspent); further in it fell hardest on whoever was still learning (at the same dues the sim's
learner at 0.5 clears 43% of seasons with the ear and 68% without), and what a player met was a meter standing
between them and the one thing the game is about: the same routine, only dearer. So `Ear.cs` is out with everything built on it: the meter over
the chest, the Tea Kettle and the Listening Screen (a save that had bought them gets its gold back: `SaveData` version
9), the Echo Stone and the Lamb's-Wool Plugs, a head cold and a rested ear among the challenge terms, Bram's three
tips on it, and the careful player's `NextCheck`. A chest can be shaken as often as its seller will stand.

**Where the repetition comes from.** Every chest asks for the same motions: shake it gently, firmly and hard; drop it;
tick what sounded; star what glowed; work out what a sign points at; guess what a haggler would take. In a first week
that is the game being learned. By the third it's a procedure, the same for every chest. More distinctive sellers
(0.28 to 0.32) treated the symptom, and a price on every motion (0.36) left the procedure as it was. The owner's other
idea was the one to follow: **builds that play differently**. Until 0.37 a build changed which chests pay; now it also
changes what the counter asks of your hands. Each way of trading has one curio that does that way's share of the
routine (`Core/Hands.cs`, a partial of `GameState`):

| Way of trading | What it asked, every chest | The curio that does it | | What's still yours |
|---|---|---|---|---|
| the perfect read | tick each thing as it sounds, and count them | **Scrivener's Quill**: the slip ticks each thing the first time you hear it, one more of its kind, starred if it glowed (`Tick`) | U, the stall | what never sounds (a warded thing, what's under a false bottom), and striking out what it got wrong: it writes what was heard, so sand is ticked as a spell |
| the perfect read | all of it | **Pocket Engine**: every chest's real contents are on the slip as it comes in, false bottom and warded thing and all (`ReadsChests`, the Guild's Bookkeeping Engine in a coat pocket) | L, Crownhold; opened for good by a hundred perfect reads across seasons | the price: whether it's worth it, and what to offer |
| enchantment | shake until everything has sounded with the witchlight lit, and star what glowed | **Lantern Moth**: every enchanted thing is starred as the chest comes in, witchlight or not (`MarksMagic`) | R, Crownhold (Thornwick, in a season that already knows Crownhold's ways) | counting the rest |
| false bottoms | the drop, and what it breaks | **Toll-man's Gauge**: a false bottom is marked as the chest comes in (`MarksHidden`) | R, Thornwick | the glass you no longer have to risk, and Brass, who hates a drop, is no trouble |
| wards | working out which kind the sign points at, and marking one more of it | **Wardens' Primer**: the slip names the warded thing a sign points at (`NamesWards`: one more of its kind than the chest holds) | R, Thornwick | hearing the rest, so the count it adds to is right |
| haggling | guessing the least they'd take, and risking a walkout to find it | **Factor's Abacus**: the offer starts on the least a haggler will take, and they take it (`KnowsFloors`, `Visit.Floor`) | R, Thornwick | whether the chest is worth that |
| sight unseen | nothing | none | | it never lays a hand on the chest to begin with |

They are rare on purpose (one uncommon, four rares, a legendary), found like any curio (Nim leans towards the way of
trading you're building) and take a slot like any curio. None is in a first season, which is for learning the
motions by hand: the quill, the gauge, the primer and the abacus open after a first season, the moth once a season
has traded at Crownhold (or after two), the engine by its feat. Each makes its way of trading *faster to play* more
than richer: as Nim brings them the quill adds nothing to a careful player's takings and the primer and the abacus about
2%, the gauge and the engine about 7% and the moth 11% (`--sim=curio`, 100 seasons each against the careful player
with no curios; the moth's is every enchanted thing a shop with no witchlight would have left unstarred). A build
around the perfect read with the quill plays in half the clicks; with the Pocket Engine it stops being about reading
at all and becomes about price, which is what 0.30's Bookkeeping Engine was for a run already won.

**The rule this breaks.** "Never let a curio tell you what a chest holds" stood from 0.20: a curio bends a rule, the
ear does the knowing. These six tell you something of what a chest holds, and that's the point of them: they are what
a build is *for*. The line that holds now is the older one behind it: **nothing says what a chest is worth before you
buy it.** No curio tells how good a thing is (each thing's quality is still rolled round its chest's colour and shown
only at the opening), none prices a chest, and the odds bar still only adds up what the slip says. A new curio that
tells what's in a chest must belong to one way of trading, do that way's routine and nothing wider, and be rare.

**Bram's cradle** (`GameState.Cradle`, `CradleLevels`, `CounterPhase.Rock`). The shaking itself goes to a tool
everybody gets: a cooper's cradle under the counter that rocks a chest gently, firmly and hard, a second each, at a
press of its button or the act key. You still do the listening (the words, the counting, the glow), and the drop is
still your hands'. It rocks only as far as the seller allows for nothing: no hard shake where that's minded or can't
be done, and where shaking has a price of its own (Nettle's fee for each strength, Tobin's rotten chest, a seller who
won't wait) it rocks once, gently, and the rest is yours to decide. A first season shakes by hand for its week at the
stall, which is how the strengths are learned (Bram's lessons ask for each by hand), and for its first three days at
Thornwick, which are a newcomer's heaviest (locks, the drop, straw, eight new people: *Playtesting the experience*);
the cradle comes out on the quiet fourth (`GameState.CradleDay`), in one of Bram's notes ("You've shaken enough
chests by hand: my old cradle's yours"). Any later season has it from its first morning (`GameState.Veteran`, with
`Ways >= Market.CraftTier`). The experience report counts
four seconds a strength by hand and 1.3 on the cradle: eight seconds a chest, ten minutes and more over a season's
hundred chests, and every one of them the dullest seconds at the counter.

**The slip keeps what you heard.** A kind's row on the slip darkens and gets a dot once you've heard one of it on this
chest (and the false-bottom row once a drop has boomed): 0.36's playtester asked for exactly this ("nothing records
what was heard once the words fade"). A click still counts one more of a kind (and past the most a chest holds, back
to none); a right-click now takes one off (`Appraisal.Uncount`), so a count clicked once too often doesn't have to go
all the way round.

**At the opening** a thing you didn't mark that nothing you did could have told you of (a strength you never shook
at, a false bottom you never dropped for) still isn't a miss: it gets the closed eye a sight-unseen chest's things
get, not a cross (`OpeningPhase.Unheard`, `Visit.Tried`, `Visit.CouldHear`; Bram's `unheard` tip). A cross is for what
you listened to and got wrong. The checks made on a chest are kept for that (`Visit.Checks`), and cost nothing.

**When a shake counts.** A strength counts as tried on a chest once it's been shaken at it for half a second in all
(`CounterPhase.TryTime`, by `ShakeBox.Level`: the hardest the chest has moved in the last moment, worked out in the
shake box a physics step at a time, whatever the frame rate). Nothing is spent by it any more, but three things still
go by it: Bram's lessons wait on "shake it firmly", the hint under the chest says which strengths are left to try,
and the opening's closed eye is for what no strength you tried could have stirred. 0.36 learned what a miscount
costs (its first ear read the hand's speed once a frame, and at twelve frames a second a firm shake lit the meter
and never counted: the playtester sat in a lesson shaking for half a minute), so `--hand-test` (0.36's `--ear-test`,
kept) still drives a pointer through the real counter the ways hands move (a gentle, a firm and a hard shake, a hard
one with its wind-up and wind-down, a mouse that reports in jumps, twelve frames a second, a lift, a carry) and checks
which strengths each counted as.

**The careful player** (`Hearing.Read`, `Strategy`, for the sim and the bot) listens to everything again: every
strength the seller allows for nothing, the drop where a false bottom is likely and nothing in the way of it (not
with a gauge), the lodestone where it's owned. `GameState.Premark` writes what the build knows before it starts,
`Strategy.FirstOffer` starts on the floor with an abacus, and the bot uses the cradle as a player would. A learner
still makes a learner's mistakes (`Style.Skill`), not a rationed ear's.

## Passing: your gut

When you pass, the seller takes the chest away unopened: opening it after you turned it down wouldn't make sense. But
you get feedback straight away, the way you would in life: a gut feeling, from what it really held against their
price (or what they'd have taken, if they wanted you to name one). Six bands, from "You have a feeling you dodged a
cart" through "Could have gone either way" to "Even the heavens pity you", each with a few variants. Words alone are
hard to rank, so the feeling also has a colour and a meter: a strip from red ("missed out") to green ("right call"),
with a marker that slides to where this call landed (`Gut.PassScore`: even at one half, a third of the price or
less at the green end, four times it or more at the red). A seller who storms off leaves the same feeling. After an
opening, the verdict gets a gut line and meter too, from "fleeced" to "a steal" (`Gut.BuyScore`). 0.32: it judges what
the profit stamp judges, everything the chest brought back (what's inside as it sells here, a perfect read's valuer's
fee, a haggler's rebate, the Underwriters' bond) against what you paid. Until then the gut saw only what was inside,
so a chest that lost on its contents but won on its fee could stamp PROFIT over a gut that said you'd been had.

**Sight unseen at the opening (0.32).** A chest bought without a shake, a drop or the lodestone is a blind buy, and
nothing on its slip was a read, so the opening doesn't mark it as one: a "Bought sight unseen" tag sits under the
tally (its tooltip says exactly what counts as sight unseen), and each thing the slip never marked gets a closed eye
in a neutral colour, with only its quality under it, never a red cross and "you missed it". What you did mark is still
checked, and a wrong mark is still a cross. Bram explains the closed eyes the first time.

## Opening a chest

It opens by itself (no holding): a rumble in the chest's own colour, longer the more that rides on it (0.37, below),
then the burst. A thing **worse** than its
chest comes first: it pops up in the chest's colour as if all's well, then sags with a deflating slide while the
colour drains to what it really is ("WORSE..."), and flops onto the table. Things as good as promised land quickly.
An enchanted thing lands in a purple glow ("ENCHANTED! x2"). A thing **better** than its chest rises out still glowing in the chest's
colour while the room dims, trembles, and climbs a quality at a time ("BETTER!", "EVEN BETTER!!", "INCREDIBLE!!!"),
each step with a flash, a ring and a rising arpeggio, before it slams onto the table. Two or more steps up also stop
time, flash the room white, and throw confetti and a fanfare. Each quality lands with its own effect, not just its
colour: common, a puff of dust; uncommon, rising sparkles and a chime; rare, a pillar of light and a ring; epic,
crackling lightning, a boom and a lingering aura; legendary, all that with turning rays, a fountain of coins and a
fanfare. A Heartsong comes out last, with its own reveal and a choice (above). `--opening-demo` opens a crafted chest
with one of each to preview them.

**Anticipation and payoff (0.37).** The owner asked for both: a reveal that builds more excitement about what's
inside, and one that feels more impactful when it comes. What it had was the same 0.9 seconds of rumble for every
chest and a tally that counted up: nothing in the build said this chest mattered more than the last one, and nothing
in the payoff said by how much you'd won.

- *How much rides on it* (`OpeningPhase._stakes`, 0 to 1: the chest's colour, what was paid against the shop's working
  purse, how full it is) sets the pace. A plain chest for a few coins still strains for 0.9 seconds and opens
  briskly; a dear purple one takes up to 1.8, and every pause after it is longer too. A hundred chests a season can't
  all be an event, so the fuss is spent where the money is.
- *The lid thumps* as the chest strains: seven knocks, closer and closer together, each a little harder and higher, a
  heartbeat that runs straight into the burst (`Thumps`).
- *The tell* (`Tell`). Just before the lid goes, a glint gets out of the crack in the colour of the finest thing
  that's better than its chest promised, and a shimmer of violet if anything in it is enchanted. It says something is
  coming, never what or which, and the surprises still come out last, so the wait for them starts here. A chest with
  nothing better in it gives nothing away.
- *Everything comes out wrapped* in the chest's light (a ball of its colour with a trail of sparks) and shows what it
  is only as it lands: the landing is the reveal, not the flight.
- *The stake bar* under the tally fills as each thing lands, towards a notch at what was paid: amber short of it,
  green past it, full at twice. Crossing the notch rings out "PAID FOR!": everything after is profit. Each thing
  that lands rings a note higher than the last, so a full table climbs a scale.
- *The last breath* (`LastBreath`). Before the last thing comes out the chest draws breath: the box glows up and a
  note rises, longer the more that rides on the chest, and longest when the chest hasn't paid for itself yet and this
  is the thing that decides it, with the notch swelling while you wait.
- *How many times over.* A chest that brought back twice what it cost or more has it stamped on the verdict's corner
  ("x3.4"), with a shower of coins out of the box from three times and the room joining in (confetti, rays, a
  fanfare, a shake) from five: but only when the profit is a real share of a working purse, so three times four coins
  is a sticker and no more.

**Enchanted things flow (0.37).** An enchanted thing used to burst purple as it landed and then sit there with three
faint rings behind its icon, and the owner found its status "too difficult to notice". Now light flows over it and
licks up off its edges for as long as it's on screen (`Enchant.Flow`, `assets/shaders/enchanted.gdshader`: two bands of
violet light crossing the picture, a rim that hugs its silhouette and rises off the top in tongues, drawn a cell of
the art at a time so the pixels stay pixels), over a soft glow with motes rising through it (`Enchant.Aura`).
Everything that shows a particular thing goes through it (`UiKit.MakeItemIcon`, the opening's table), so it reads the
same on the table, the shelf, the collection and a commission's chooser, at any size.

**Two layers** (0.35). The chest is its lid, drawn as it always is, over the box, and the moment the lid lifts at all
(the opening's strain, a mimic's breath, the Maw) the box under it is the open box: its front, and above the front's
thin rim its inside in one-point perspective, the back wall facing you, the side walls slanting out, and a planked
floor whose nearer boards are wider (`custom_art._chest3_open`). So what shows between lid and box is the box's own
inside, lit from within in the chest's colour (`ChestView.DrawInnerLight`: added over it, never covering it, brightest
on the floor), with a soft wash of that light spilling from the crack; never the white bar 0.34 drew between two
halves. At the burst the light pours up out of the open box in shafts of its colour and settles to a glow inside
(`ChestView.RestGlow`); the bright line 0.34 left along the open box's rim is gone.

What the shop's curios and the day did to the chest ("Rabbit's Foot: 2 things came out better!", "Locking Day +25%")
slides in under the tally only once everything is out (v22: told first, it gave the surprises away). Since 0.35 it
thanks the curio whose rule reached each thing (a clover only on the week's first chest, a hollow rod only for what
was under a false bottom, a gambler's token for what it swung either way), not the first luck curio you hold. The profit's
coins fly into the purse one by one, and the purse counts up as they land, from the first coin to the last (v22: it
used to wait for the last one, then count).

## Sellers: one way of judging, one rule, and a story

The same rules make a different puzzle with each seller, because each judges a chest a different way, with a blind
spot a careful player can see through; and since v15 each comes from somewhere, tells you where the chest came from
(true or not), and many have **a rule of their own that changes what you can do or what the deal is**. Every rule is
something a real person might insist on. Not every visit is a deal: often the right answer is to pass.

| How they judge | Who | Their blind spot | How to beat it |
|---|---|---|---|
| Listen at one strength | Ada, Nettle and Corvin (gently), Cadoc and Brass (firmly), Garrick and Tilly (hard) | what their strength can't stir or a clank drowns; they never count | shake at the strengths they didn't; count |
| Listen gently, then hard | Skrae, Hesper (hard) | nothing they can hear; enchantment, and what's warded | the witchlight; the echoes and the dowry custom (below) |
| Count the rattles | Bettany, Quire (firmly) | a rattle's a rattle: a dagger costs what a purse does | buy when the rattles are good kinds |
| Weight | Rook | heavy means valuable: arms are heavy and cheap, potions and scrolls light | buy light chests of good things |
| The chest's colour | Tobin, Ilse, Orme, Isaura, Rowan, Tick, Sallow, Madame Zell, the Maw | never look inside | buy when it holds more than a chest that colour usually does |
| A dream | Mother Sorrel | the price has nothing to do with the chest | add it up, buy when the dream's cheap |
| Knows only arms | Durgan | prices other things at half, and gems as diamonds | haggle; mind the tinkles |
| Knows only her gems | the Countess | prices her gems at what she paid; the rest comes free, but some gems are paste | weigh the free things against the paste |
| No idea | Pip, Old Aldo | Pip takes a few coins (and his millpond chests are sometimes empty); Aldo lets your slip set the price | shake first; read Aldo's exactly |
| Packed it himself | Gilt | sells demon gold as gold and gems, cheap | the lodestone silences fakes |

**The rules** (`SellerDef` fields; `Visit`, `Shop.Buy`, `Shop.Settle` and the counter carry them out, and
`GameState.Terms` puts each under the chest in words):

| Rule | Who | What it does | Why it's a puzzle |
|---|---|---|---|
| A rotten chest (`ShakeLimit`) | Tobin | two seconds of shaking, all told, then it creaks and won't be shaken again | one strength: hard hears everything that isn't drowned out; gentle hears the coin a clank would hide |
| A fee for shaking (`ShakeFee`) | Nettle | every strength you try after the first puts her price up a tenth | is the hard shake worth a tenth? Gentle and hard between them hear everything firm would |
| No hard shake (`HatesHardShake`) | Brother Cadoc | half again if you do | the crypt's vestments only flump at a hard shake: tick them on faith or pay to be sure |
| No drop (`HatesDrop`) | Brass | half again if you do | his ticket counts the false bottom instead |
| No lodestone (`HatesMagnet`) | Countess Vey | half again if you use it | a quarter of her gems are paste |
| A clock (`Patience`) | Tick | twelve seconds | gently, then hard, and decide |
| A dream price | Mother Sorrel | the price is a dream (0.4 to 1.8 times a usual chest) | add it up yourself |
| A wager (`Wager`) | Archmage Orme | read her chest exactly and a third of the price comes back | a careful read makes her dear chests cheap |
| An heirloom (`Heirloom`) | Sir Rowan | the best arms inside are his, for a fee he names (three-fifths of what it's worth as promised) | value the rest; a chest with no blade is all yours |
| A devil's due (`TakesDearest`) | Sallow | half price, but the dearest thing inside is his | many middling things suit his bargain; one grand thing doesn't |
| The King's game (`SlipPrice`) | Old Aldo | his price is your slip: exactly right and he takes a quarter off; wrong, and you pay what it truly held and a quarter more | the purest test of a read; a careless one costs dear |
| The Maw's gaze (`Gaze`) | Avarax, the Gilded Maw | one coin for the chest, but everyone after him today asks twice as much | count who's still waiting |

**Stories** (`SellerDef.Stories`; `Market.Story`). Every seller's line is a story about where the chest came from and
what happened to them, with how they shook it or what they state woven in ("Turned up under the plough on the old
battlefield. I gave it a good hard shake, like the hay. 30 gold, and that's a bargain!"). It's always true: the place
decides what the chest tends to hold, so the careful player reads every chest by its place. Nobody says what they heard (v22 retired the
claims, and the slip they marked): a seller still prices only what they noticed (`Visit.Noticed`), so the deduction
is the player's, from how they judge (the slip's header, `SellerDef.Priced`): shake as gently as Ada did, and what
you hear is what she priced.

**Voices** (v24). Everyone talks like who they are, wants what they want and says what they'd plausibly say at a
market counter: a farmer about the harvest, a goblin about shinies, a knight about honour, Vane politely and coldly,
Nim about Nim. Each seller has their own lines for a deal (`Accepts`), a walkout (`Leaves`), a pass (`Passes`), a drop
they don't mind (`Startled`) and one they do (`Fussed`), falling back on a few shared ones (`Sellers.PassLine`,
`StartleLine`). Speech says money the way people do (`Money.Say`: "30 gold", "one coin"); the interface keeps "30g".
Bram's tips and lessons, the rules under the chest (`GameState.Terms`), the slip's header and the descriptions of
curios, upgrades, events and terms are plain explanation: what it does, in a sentence, and why if it isn't obvious.

**What they tell you besides** (`SellerDef.States`, `Visit.Clue`, `Strategy.Reckon`), and chests that change the rules
of hearing. Each is a deduction, on a rule from real life, taught by its seller's lesson:

| Seller | The clue | The deduction |
|---|---|---|
| Corvin Tallow | **straw** (`Packs`, `Chest.Packed`): packed so nothing clinks at the Wardens' toll | everything stirs a strength later (`Chest.Needs`): a gentle shake proves nothing, a firm one hears coin, potions and gems, a hard one weapons (whose clank then drowns the rest), and cloaks and spells never stir. He shakes firmly, so he never prices a weapon; nobody can price the soft things, so read them by the road's odds. Still often a false bottom: drop it |
| Gilt Brass | **the pawn ticket** (`Clue.Articles`): how many articles were pledged, false bottom and all; he won't have pledges dropped | count every separate sound at every strength against the ticket: one short is under the false bottom |
| Rook Varga | **her game scale** (`Clue.Pounds`): the weight, to the pound; her chests hang on a mare who spooks at a hard shake (`Packhorse`) | take what you heard from the weight: what's left never stirred (a cloak 3 lb, a spell 1, a purse under the floor 4). Her scale shows beside the chest, like Hesk's |
| Mother Sorrel | **a dream** (`Clue.Dream`, `Market.DreamtOf`): she always dreams true, as a sign (magpie, thorn, raven's wing, river, owl, star), of what the Wood warded inside, or else of the dearest thing (never under a false bottom) | if the dreamt kind never sounds at any strength, it's the warded thing: name it |
| Aunt Hesper | **the dowry custom** (`Clue.Dowry`): a Thornwick dowry always holds coin, a cloak and a potion | she shakes hard and hears everything; whichever of the three you can't hear (after every strength) is warded |
| Ilse Varrow | **the packing tag** (`Clue.Tag`): every kind packed, warded or not | a kind on the tag that never sounds is warded |
| Lady Isaura | **grandmother's riddle** (`Clue.Riddle`) on the lid, about one thing inside (the warded one, if there is one) | answer the riddle; if the answer never sounds, it's warded |
| Master Quire | **the ward-roll** (`Clue.Roll`): the warded thing's name, every other letter faded ("B_o_d R_b_"); a blank roll, nothing warded | read the name (the words are ordinary ones: 'R_b_' is a ruby, 'E_i_i_' an elixir) and name its kind |
| Skrae | **its echoes** (`Clue.Echo`): the shape of everything inside, warded or not (round coin, long weapon, soft cloak, hollow potion, flat spell, tiny gem) | one more echo of a shape than you can hear is warded |
| Madame Zell | **three cards** (`Clue.Cards`) from the Locking's deck (Coins, Swords, Mantles, Cups, Wands, Stars) | a suit drawn twice is what the ward holds; three different suits, nothing |

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
| Pip Hollow | Brindle | the millpond, Gran's attic | no idea | name a price | (30% empty) |
| Ada Kettle | Brindle | her Tobb's kitbag, the attic | a gentle shake | fixed | |
| Garrick Thorne | Brindle | the old battlefield | a hard shake | haggle | |
| Bettany Crane | Brindle | unpaid tabs | counting rattles (firmly) | fixed | |
| Tobin Reed | Brindle | the Barrow | colour | fixed, cheap | a rotten chest |
| Nettle | Thornwick | a goblin warren | a gentle shake | fixed | a tenth more per strength |
| Rook Varga | Thornwick | outlaws' camps, wolves' dens | weight | fixed, fair | her game scale; no hard shake (her mare); 80% locked |
| Mother Sorrel | Thornwick | the Old Wood | a dream | fixed | her dream (half warded) |
| Brother Cadoc | Thornwick | the abbey crypt | a firm shake | fixed, steep | no hard shake |
| Corvin Tallow | Thornwick | the smugglers' road | a firm shake, through straw | haggle | straw (45% false bottoms) |
| Aunt Hesper | Thornwick | a bride's dowry | a hard shake | fixed | the dowry custom (half warded) |
| Ilse Varrow | Crownhold | the Arcanum stores | colour | name a price | the packing tag (half warded; 25% enchanted) |
| Archmage Orme | Crownhold | the Arcanum stores | colour | fixed | a wager |
| Durgan Ironhand | Crownhold | a dwarf-hold | knowing arms (hard) | haggle | |
| Lady Isaura | Crownhold | a noble's strongroom | colour | fixed | grandmother's riddle (half warded) |
| Gilt Brass | Crownhold | forgotten pawn lots | a firm shake, never counting | fixed, steep | the pawn ticket; no drop (60% pairs, 35% false bottoms) |
| Master Quire | Crownhold | the Archive vaults | counting rattles (firmly) | fixed, steep | the ward-roll (half warded) |
| Tilly Mudge | Crownhold | the Aldwater mud | a hard shake | fixed | (half her scrolls are sand) |
| Sir Rowan | Crownhold | a knight's war chest | colour | fixed | an heirloom |
| Tick | Royal Emporium | the Maw's hoard | colour | fixed | twelve seconds (things often better than their colour) |
| Sallow | Royal Emporium | the Maw's hoard | colour | fixed, cheap | a devil's due |
| Skrae | Royal Emporium | the Maw's hoard | a gentle shake, then a hard one | fixed | its echoes (half warded; enchantment 20%) |
| Madame Zell | Royal Emporium | a courtier's chest | colour | fixed | three cards (half warded) |
| Countess Vey | Royal Emporium | a noble's strongroom | her gems, at what she paid | fixed | no lodestone (25% paste) |
| Gilt | Royal Emporium | the Maw's hoard | packed it himself | fixed, at about half | (60% demon gold) |
| Old Aldo | Royal Emporium | the Royal Treasury | none | your slip | the King's game |
| Avarax, the Gilded Maw | Royal Emporium | the Maw's hoard | colour | one coin | the Maw's gaze |

For listeners, whether the chest holds something they can't price (unheard, or a second of a kind they didn't count)
is rolled first, at their miss rate (55% by default; 45% for Brass, 50% for Cadoc, 60% for Nettle; Skrae and Gilt
miss nothing), which keeps buying blind from paying while listening does. Everyone brings two of a kind now and
then (8%, from Crownhold). Old Aldo comes in less often than the others (`Visits`), and the Maw rarely.

## A seller's first visit is a lesson

The first time each seller comes in (with tips on), their visit is a lesson in their habit, built so it can't fail
to show it: Corvin's is packed in straw with an unpriced weapon and a false bottom, Brass's ticket counts one more than
you can hear, Rook's weighs a cloak more than you heard, Mother Sorrel dreams of a star with no tinkle inside, Tilly's
has a scroll case of sand, Gilt's has demon gold, Sallow's holds four things for him to take the best of. Bram gives one step at a time; each step has a check
(`LessonStep.Done`, on what's been heard, the slip, the visit) that runs every frame, so a step done early counts.
Until the steps are done, the trade buttons wait (unless the step is about trading, like naming Pip a price); the
lesson can always be skipped. Steps adapt to the tools you own (Skrae's asks you to mark what glows if you have the
witchlight; Gilt's to use the lodestone if you have one). Cadoc's lesson leaves the choice to you: tick the crypt's
vestments on faith, or shake hard and pay half again to hear them. The new rules each get theirs: one hard shake for
Tobin's rotten chest, gentle-then-hard for Nettle's fee, an exact read for Orme's wager and the King's game, the
queue to count for the Maw. Day one is still the scripted stall day (three customers since 0.20; Bettany's lesson in
passing opens the second morning); Pip, Garrick and Bettany teach naming a price, haggling and counting rattles on their
first visit after it.

**A step is an action** (0.35). Every step names the one thing it asks for in a tab over Bram's box ("Shake it gently",
"Tick Weapons and Coin", "Buy it": `LessonStep.Do`), and its check runs every frame whether the box is open, closed or
mid-sentence, so doing it moves the lesson on at once. 0.34's first day was made of tips, and a tip up on the screen
held the next one back: the playtest shook gently while the first tip was up, nothing happened, and they had to close
it and shake again. The first day is lessons now (`Lessons.FirstDay`: Pip's gentle shake and the slip, Ada's hard
shake, Garrick's clank, and Bettany's pass the next morning). A lesson's chest follows the build like any other
(`GameState.Lesson`, 0.35): it's priced as any chest here is, and the curios' words on the price and the deal (a sealed
bid, a closing bell, a signet, a golden tongue, an ear trumpet) hold for it, as players expect them to.

**Taught once, across runs** (v20). What Bram has taught (his tips, each seller's lesson, the scripted first day) is
kept in the profile (`Profile.Taught`, given to `GameState.NewRun`), so a second run with tips on doesn't teach it
again: the experience playtest found runs 2 and 3 replaying a dozen lessons the player had already had. Since v22 it's
written the moment he says it (`Main.NoteTaught`; a lesson once its seller is at the counter, not while they're
queued), and merged into a continued run too (`GameState.Remember`), so a day played over after quitting doesn't
teach it twice either.

**One tip at a time** (v22, `MentorView.Teach`). When two tips come up together Bram gives one; the other isn't marked
as taught, so it comes up the next time it applies (the most pressing is asked first: at the day's end, paying off the
debt before the Bookkeeping Engine before the shop's news).

**Who's who** (v22). Clicking whoever's at the counter opens the notebook on their page, their line framed in gold; the
notebook button and N do the same. Mother Sorrel's lesson starts with it ("click her to read hers", `Visit.LookedUp`),
and the `lookup` tip tells Bram's kin who come without lessons.

Lessons and Bram's tips never talk over each other. A lesson waits until any tip already up (a new shop, a lock, a
new way of trading) has been read. While a lesson runs, tips that would come up don't pile up behind it: they wait
for their moment to come round again. And a lesson's `Covers` marks the tips it already teaches as seen (Garrick's lesson
is the haggling tip; Corvin's, the drop and the boom; Tobin's, the story tip). A new shop's crowd doesn't all arrive
on its first day: with tips on, no more than two lessons a day (`GameState.LessonsPerDay`), and one a day at
Thornwick, which also brings locks, the drop, straw and warded things (the experience playtest's heaviest days). Bram's kin who come knowing the trade (no lessons) still get a word on each seller's
terms the first time they meet them (`seller_*` in `Hints.cs`), and the terms are always under the chest. `--autoplay` prints every message Bram shows, and flags any that was replaced before it could be read.

`--sim` ends with a lesson test: every lesson played the careful way (the same strategy the bot and the sim use),
with and without the shop's tools, must complete every step in order.

## What's said: typed out, a sentence after another, and the echo horn (0.35 to 0.37)

Nobody says a paragraph at once any more. Bram's tips and lessons, the scenes, the talks and the sellers' bubbles go a
sentence at a time (`Words.Sentences`: split at a full stop with a space after it, or a CJK stop; a quotation is closed
and reopened round the break; scraps too short to stand alone stay with their neighbour). Bram's box shows dots for
how far through it is and "click to go on"; a seller's bubble is sized for the whole speech and fills in as it's said
(a click shows the rest); a talk's lines come sentence by sentence before the choices.

**0.36: a speech says itself.** 0.35 asked a click for every sentence: thirteen before the first chest, and on
through every tip, scene and tale ("there's too much talk between chests" was the fresh playtester's first complaint,
and the experience report's). A sentence at a time is right for reading; a click a sentence isn't. Now each sentence
appears under the last once that one has had time to be read (`Words.ReadTime`: half a second and about 45ms a
letter, 110ms a character in Japanese and Chinese, between one second and about three), so a speech plays through
like someone talking; a click, or the act key, shows the rest of it at once, and the next click goes on. That's one
click a speech (a scene's line, a tip, a talk's turn) where there were three or four, and nothing the player hasn't
had time to read is hidden behind it: the whole speech stays up until they go on. A lesson's steps still wait for
what they ask ("do it to go on"), and a seller's bubble fills in the same way.

**0.37: a character at a time, with a voice.** The owner: "Dialogue should appear one character at a time, accompanied
by subtle text sounds, as in most pixel-art games. Keep the underlying text organized into segments, since
click-to-skip and the radio system still need that structure." So the sentences stay exactly as they were
(`Words.Sentences`: the echo horn's lines, Bram's dots, the speaker's stir and the click that shows the rest all still
go by them), and each is now typed onto the page (`Ui/Typewriter.cs`, one class for Bram's box, the sellers'
bubbles, scenes, talks, letters and the treasury's captions):

- The whole speech is laid out first and revealed glyph by glyph (`VisibleCharactersBehavior.CharsAfterShaping`), so
  nothing reflows as it's typed: a word never starts on one line and jumps to the next, and a bubble is still sized
  once for all of it.
- 52 characters a second (Settings > Game > Text speed: Slow 30, Normal 52, Fast 90, or At once, which brings each
  sentence whole as 0.36 did); half that in Japanese and Chinese, which say as much in fewer characters. A short
  breath at a comma, a longer one between sentences (0.3 to 0.75 seconds by the sentence's length: the typing was most
  of the reading).
- A soft blip every second letter, every third at the fast speed (`Sfx` "talk": 50ms, played well under the chests'
  sounds, on the Effects bus; Settings > Game > Text sounds turns it off), pitched to the speaker (`Typewriter.Voice`:
  Bram in the middle, Mister Vane low, everyone else somewhere between by their name, the same every time), with a
  hair of variation so it doesn't drone. A letter, and Bram's notes while he's away, scratch like a pen instead
  ("scratch"); the treasury's captions are said low. When two speeches are being typed at once (a seller's pitch, and
  Bram's word on it) only the later one is heard.
- A click, or the act key, still shows the rest of the speech at once, and the next goes on: one click a speech, as
  0.36 had it. A talk's choices appear when its last line is out.

What matters is coloured where it's said (`Rich`): the kinds of thing in gold, their sounds in blue, the strengths in
green, the trade's words in violet; a seller's price in gold and what they state (a tally, a dream, a tag) in violet,
the seller giving a little bob as they say it. The notes under the chest colour the seller's own rule gold, and the
slip's "how they priced it" line is in dark ink with its strength coloured: each seller's defining habit stands out
where you read it. Sellers' own words colour only strengths (their talk is full of "coin" meaning money).

**The echo horn** (`EchoHorn`, `HistoryView`): Bram's old Keyhold horn stands at the counter's end (and beside closing
up at the day's end). It keeps everything said across the counter this season (`GameState.History`, saved with the
run, the last 400 lines): Bram's tips and lessons, the scenes, every line of a talk and your answers, the sellers'
words. Click it for the list, a day at a time, the newest in view, each line under who said it and coloured as it was.
Bram mentions it on the second day (the `horn` tip).

## How the shop opens up

Everything arrives when it's needed, each with a word from Bram, so the first days are only about listening. Since
0.36 a market comes with each due paid, so the season's four weeks are its four steps:

| When | What opens up | Bram explains |
|---|---|---|
| Day 1 | shaking, the slip and the odds bar, buying | one lesson per customer (scripted) |
| Day 2 | passing (Bettany, first thing), haggling, naming a price, counting rattles; sellers' stories and the notebook (N) | each seller's first visit is a lesson; the notebook and the horn on later customers |
| The second evening | the Gilded Guild and Bram's twenty years of back-tithe: a due every fifth evening, the first in three nights (until then nothing shows it), and each one paid moves the stall a market further in | a scene: Vane, then Bram |
| Day 3 | Nim's curios (his first visit) | a tip: room for three, so pick ones that go together |
| Week 1 | two gentle events (days 2 and 4); the day's end shows the week at the crossroads and the due as the way on | a tip with each |
| The first Guild night (day 5) | the due paid, the Guild's carters move the shop; the treasury's summons: Bram goes with Vane (and Old Haddon, if his note wasn't paid); from then on his tips are notes in the margins of his notebook | a scene: Vane, then Bram |
| Day 6: Thornwick Market | the road (`RoadPhase`); the Guild's letter (keep Bram's account paid, and earn the treasury key's four pieces); locks and lockpicking, the drop and false bottoms, potions and spells; the display shelf, keeping things, the exhibition and cabinets; tools and the shop's fittings, at the day's end and from their makers at the counter; with tips on, a quieter first day (a customer short) at each new market | the lock (the first time, the pick waits while he talks); the shelf and why an exhibition matters; where to spend on the shop |
| Day 8 | the regulars' commissions; the notebook's Inquiry as it fills; two events a week | a tip with each |
| Day 9 | **Bram's cradle** (0.37): it does the shaking from here on | one of Bram's notes |
| Day 11: Crownhold | gems, archmages and enchantment, two or three of a kind in a chest; synergies: the journal has tallied everything from day one, so some are ready to switch on | what a synergy is and where to choose one |
| Day 16: the Royal Emporium | the court, demons and their gilded iron, the lodestone, the Royal Seal, the last due | that the Gate is just down the street |
| A short night | pawning, and (once a run) Bram's word | a tip |
| A day that starts nearly broke | Bram's secret stash (twice a run) | the day card, and a tip the first time |
| The day's event | a chip in the top bar that reopens it | a tip pointing at it |

The moves are renovations under the hood (`shop2` to `shop4`, `ShopTiers.Name`): Bram's Stall, Thornwick Market,
Crownhold, the Royal Emporium. Until 0.36 they were bought at the day's end (80, 300 and 3,000g), the first once the
stall had earned 50g; now `GameState.MoveInward` owns them the night a due is paid (or forgiven: Bram's word, a
favour called in, the whole debt paid off early), and they're never on offer. `GameState.UpgradesOpen` (with
Thornwick), `DisplayOpen` and `SynergiesOpen` hold the gates, so the sim and the bot follow them too. A week at the
stall is five days with one thing new in each: listening, then judging sellers, Nim, an event, the Guild.

That table is a **first** season. What a later one opens with is the next section.

## A season after the first (0.37)

The owner drew the line: a season starts in one of three states, and the game had been treating the second like the
first.

| | The state | What it is |
|---|---|---|
| 1 | the first season ever on a save (`GameState.SeasonsBefore == 0`) | everything is taught, and each of the trade's ways is met a market at a time (the table above) |
| 2 | a later season, before the game's been beaten (`GameState.Veteran`) | most of a player's hours: the seasons spent learning to clear the debt, and then the remembered ones of the campaign |
| 3 | a season after the true ending (`CampaignDone`) | a new year: everything of state 2, and no summons, mimics or treasury (0.34) |

What was wrong in the second state, found by having the bot play two seasons running on one profile (`--profile`) and
reading what the second one said and dealt:

- **Bram's tips and lessons did not repeat** (they've been kept in the profile since v20). What repeated was the
  story's exposition: Bram's welcome with the trade in a sentence and his tap, Vane's introduction of the debt, the
  summons, the Guild's letter read out in full, Hollis explaining the Shut Chests. Each has a short form now for
  anyone who's heard it (`Story.Intro`, `WeekStart`, `Summons`; Hollis's notice in `TaleBook`): the figures and
  what's different this time, no more. Vane's introduction is two lines where it was seven. A remembered season
  (after the treasury's first swallowing) already had lines of its own, and keeps them.
- **Everything a first season opens a market at a time was shut again every season.** No locks or false bottoms
  until Thornwick, one thing of each kind in a chest and no enchantment until Crownhold: so every season's first week
  was the plainest game there is, however many seasons you'd played. Those now go by **the trade's ways**
  (`GameState.Ways`: the grandest market any season on the save has traded at, and never less than the shop's own):
  locks, false bottoms and wards with Thornwick's ways (`Market.CraftTier`), two or three of a kind and enchanted
  things with Crownhold's (`Market.PairsTier`, `MagicTier`). A second season's first chest at the stall may be
  locked, with a floor that sits too high, two cloaks in it and one of them enchanted. Bram says why, once (the `ways`
  tip), and his cradle is on the counter from that first morning.
- **What stays with the market**: which kinds of thing a chest holds (potions and spells are still Thornwick's, gems
  the capital's), who trades there and how they judge (so a warded thing still comes with the Thornwick folk who ward
  them), what things fetch, and the tools: the witchlight is still Crownhold's. Before you own it an enchanted thing
  in a later season's chest is there and unseen: nobody priced it, it comes out "ENCHANTED! x2", and the slip doesn't
  hold against a perfect read a star you had no light to see (`ISlipModel.Lit`, `Appraisal.Check`).
- **The slip couldn't mark what some chests held.** "Multiple-item contents are already unlocked, but the appraisal
  sheet does not support marking them correctly": a tale's chest (`TaleBook.Fill`) and the Stranger's
  (`Events.StrangerVisit`) drew their kinds with replacement, so one could hold two of a kind at a market where the
  slip ticked a kind once: no way to mark it right, and no perfect read to be had. Both ends are fixed. Those chests
  hold one of a kind unless the trade's ways count pairs, and the slip counts by the ways too, never by the market
  alone (`Appraisal.MaxCount`, `CountsHidden`, built with `ways:`), so the slip can always say what a chest can hold.
  `tools/curiotest` holds both to it.

And so that a later season starts as itself, not as the first one again:

- **Bram's drawer** (`DrawerPhase`, `Curios.Drawer`). On the first morning Bram turns out the drawer under the
  counter: three plain curios (common or uncommon), each for a different way of trading, drawn from the seed. Take
  one for nothing, or none. A build begins on the first day, and no two seasons begin alike.
- **Nim comes from the first night** (not the third: his introduction was the reason for the wait), and **the
  regulars' commissions from the first week's third morning** (not the second week's).

A later season is a little easier than a first for the same hands: a curio from the first morning, Nim two nights
sooner, finer chests in the cheap weeks. `--sim=learners` against `--sim=learners --veteran --ways=4` (120 seasons
each, a first-timer to the careful player):

| Skill | 0.15 | 0.3 | 0.5 | 0.75 | 0.9 | 1.0 |
|---|---|---|---|---|---|---|
| a first season clears | 21% | 33% | 53% | 78% | 85% | 93% |
| a later season clears | 33% | 44% | 66% | 86% | 95% | 99% |

That's the one place "nothing carries over that makes a run stronger" now gives, and on purpose: the second state is
where a player is learning to clear, it's the same for everyone (nothing is bought, banked or ground for), and the
challenge levels are still there for whoever finds it easy. `--veteran` plays any bot or sim run as a later season,
`--ways=N` sets the ways, and `--profile` has the bot use and keep the stored profile, so two runs in a row are a
first season and a second.

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
Every day has a role (`DayRole`, planned a week ahead by `Events.Plan` and shown on the notebook's calendar): the fifth
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

**0.30: a longer season, and a gentler climb.** The owner: with the tales doubled, the season can be longer; but tenfold
jumps week on week (300, 4,000, 45,000) felt unreasonable, and gold things came too often late on. So the debt is seven
weeks (35 days), the qualities are worth 1, 2, 4, 8 and 24 times a common (each colour twice the one below, gold rare:
*What a chest holds*, above), the buyers' trades are smaller (the auction block a quarter, the royal warrant a third,
so the index tops out about 2.1 at the Emporium, not 2.7), the upgrades are priced for the new sums (a tier's tools at
two thirds, two fifths and a quarter of what they were: what its chests are worth now against then), and the dues
climb about three times a week, with nothing like a tenfold jump:

| Week | 1 | 2 | 3 | 4 | 5 | 6 | 7 | then |
|---|---|---|---|---|---|---|---|---|
| Day | 5 | 10 | 15 | 20 | 25 | 30 | 35 | |
| Due | 20g | 60g | 160g | 450g | 1,300g | 4,000g | 12,000g | x1.5 a week |

The moves cost 80g, 300g and 3,000g, and a careful run makes them about days 8, 16 and 25 (so about two weeks at each
shop, and the Emporium for the last two); profit per customer runs about 7, 12, 70 and 490g by shop (0.29: 10, 30,
190 and 4,500). Week 5 asks for a good week in Crownhold, week 6 for a start at the Emporium, and week 7 is the wall. A
strong run pays it all off in week 6. The tales carry a run further than before (the officials, favours on a short
night, market days, the court and the decree), so the careful player clears more often with them than without; the
figures are in *Evidence*, below. The interest after the debt grows half as much again a week (twice, until 0.30), and
Bram's stash is 20g/30g at the stall, 30g/40g at Thornwick, 50g/80g in Crownhold and 270g/400g at the Emporium. A run
saved in 0.29 carries its purse into the new economy at what its shop's chests are worth now against then
(`SaveData` version 6).

**0.36: four weeks, and each week a market.** The owner: the game isn't engaging or fun enough; reconsider anything.
The review (`docs/redesign-0.36.md`) found the season itself at the root of it. The experience playtest's own last
line was "A first run takes about 2.4 hours: too long to want another"; its thirty first-timers all lost (in week 6,
after 143 minutes and 169 customers), 93% lost their second run and 90% their third; a learner reached Thornwick on
day 8 and was still there on day 30, with one new thing a day, and the report's engagement score fell from 28 on the
first day to about 10 from the twentieth. The moves inward were bought (80, 300 and 3,000g), so whoever most needed
something new was whoever couldn't afford it.

So a season is four weeks, twenty days, and **paying a week's due is what moves the shop** (`GameState.MoveInward`):
the Guild's carters come that night, and the next morning the stall trades a market further in. Week 1 is Bram's
stall at the Brindle crossroads, week 2 Thornwick Market, week 3 Crownhold, week 4 the Royal Emporium by the Gate.
It's the story's truth too: the Guild wants its keywright at the Gate by the season's last night
(`docs/narrative.md`), so it pays the carters, and Vane says so the evening he introduces the debt. Everyone who keeps
paying sees every market, a due is no longer only a tax (it buys the week's high point: the road, a new place, new
people), and a season that's going to fail fails at a due, not after two hours at Thornwick.

| Week | 1 | 2 | 3 | 4 | then |
|---|---|---|---|---|---|
| Market | Bram's Stall | Thornwick Market | Crownhold | The Royal Emporium | |
| Day | 5 | 10 | 15 | 20 | |
| Due | 40g | 170g | 700g | 6,600g (0.37; 5,000g in 0.36) | x1.5 a week |

The first three dues take a quarter to a half of a careful purse and leave a working purse for the market they
open; nearly everyone pays them (a first-timer 98% of the time, with Bram's word now and then), so the wall is the
last, at the Emporium, as it was. Level 0 is the story's difficulty now. As 0.36 shipped: the careful player cleared
90% of seasons (88% with no Heartsongs, 79% with no curios, 61% with no tales, 31% with neither), a learner at 0.9
81%, at 0.75 66%, at 0.5 43%, at 0.3 26%, and a first run 20% (120 runs each, `--sim=quick`, `--sim=learners`); the
levels as drawn 77, 57, 43, 35, 18, 16, 9 and 6% (`--sim=levels`, 150 runs each); a career first cleared the debt on
its second or third season, brought Bram home and reached the next day around the tenth, cleared level 1 around the
seventh run, level 2 around the eleventh, level 4 around the twenty-sixth, level 6 in one career in twelve by run 30
and level 8 in none (`--sim=careers --n=60`). 0.37's figures are below (*0.37: listening free, and the last due at
6,600g*).

**Getting there.** The dues were tuned with `--sim=learners --dues=a,b,c,d` (six skills from a first-timer to the
careful player, 120 seasons each, two minutes). 40, 250, 1,200 and 7,000 cleared 10% at the bottom and 77% at the
top; 30, 150, 600 and 4,000 cleared 24% and 98%: no wall at all. The
shape wanted was a first-timer at about one in five, the middle of the learning curve at about four in ten and the
careful player at about nine in ten, with every due before the last paid by nearly everyone.

Two things in the sim had to be put right before its numbers meant that. Its careful player paid the due before the
day's end, where the game has the day's end first: harmless while a move was something bought, but once paying moves
the shop it had the sim's player buying the next market's fittings before trading a day there. And its saving rule
(`Strategy.Reserve`) still measured a bad day's trading at 0.35's Emporium, which made thousands a customer, so it
kept nothing back for the last due until its last two days. With the evening in the game's order and a bad day
measured from this season (15, 40, 250 and 650g a market), the same player was a little richer than the dues had been
set for, and they went from 40, 170, 650 and 4,200 to **40, 170, 700 and 5,000**.

What changed with it:

- **The road** (`RoadPhase`): the first morning at a market further in shows the season's road, its four stops as
  pictures and the Gilded Gate at its end, the stall's cart rolling in from the last one, what this market brings and
  what the Guild wants here. The day's end at the stall shows the due as the way on ("Next: Thornwick Market"), and
  the Guild's panel says where paying leads.
- **Five customers a day** at every market past the stall (four there; six at Thornwick and Crownhold until now): a
  day's listens are counted (*The ear*), and a first run is 104 minutes over 108 customers, a second 94 and a third 89
  (`--sim=experience`; 0.35: 143, 140 and 132, each lost at week 6). High points come 8.5 an hour in a first run and
  nearly ten after (5.3 and 5.1), no time is spent in stale stretches (6%), and the engagement score holds between 17
  and 50 to the last day (*Playtesting the experience*).
- **No moves to buy**: `Upgrades` still has them (their blurbs say what each market brings, for the road), but they're
  never on offer; tools and the shop's fittings open at Thornwick, where their makers are. Thornwick's welcome (a
  boon) takes a quarter off the second due, the one paid at its market, where it used to halve the move.
- **A market's first days are quiet ones** for a learner (tips on): the first a customer short, as before, and now
  with no locked chests either (`GameState.StartDay`, `Chest.Unlock`), and the first two with nobody whose lesson is
  a warded thing's puzzle (`GameState.Settling`): the place, its people, two new sounds and the drop are a day's
  learning, the lock and its picking the next day's, and a dream or a dowry the day after. With the sim's evening put
  in the game's order (above), a first-timer's first day at Thornwick turned out to ask 7.1 of a load of 6; it asks
  5.4 now (*Playtesting the experience*).
- **Fewer tales, told faster** (`TaleBook.Draw`, `GameState.Wait`): two of Brindle's and two of Thornwick's, one of
  Crownhold's and one of the Emporium's, with the Shut Chests, the King's Court, the campaign and whoever minds the
  stall's own: about a beat a day (21 a season) where 0.35 had 34 over its 35. The beats were written for seven weeks:
  a tale's first beat now comes at the same point of the same market's week (`SeasonDay`), a later one waits about
  three-fifths as long, and a tale that begins at the Emporium is told a beat a day.
- **The campaign's nights** keep their order: the summons on the first Guild night, Vane's bribe on the second (the
  fourth, of seven) and his bargain on the third (the sixth), the gathering on the fourth week's first morning.
- **Curios**: rare ones come with Thornwick's week and legendary ones with Crownhold's, a little oftener than over
  seven weeks (`Curios.Rates`): a build has eighteen nights to come together, not thirty-three.
- **The regulars' first commissions** come on the second week's third morning, not its first, which already brings a
  market, the Guild's letter, locks and the drop.
- **A lost season is one click from the next.** Four first seasons in five end at the last due, with every market
  seen, so the screen they end on matters: it says how near it was ("The Guild took the shop on the season's last
  night: 1,130g short of the last due."), what the season opened (a keywright, a level, Nim's new curios), and offers
  **Another season** beside the way back to the title (`RunEndPhase.Again`, `Main.AnotherSeason`: straight to who
  minds the stall).
- **Saves**: a run saved in 0.35 picks up at the market it had reached, on the same day of its week, with the dues
  before it paid, and every day the save remembers moved back with it (`SaveData` version 8).

**0.37: listening free, and the last due at 6,600g.** Taking the counted listens out (*Listening is free, and a build
does the routine*) made every hand stronger, and a learner's most: at 0.36's dues the six skills cleared 30, 50, 68,
89, 96 and 98% of seasons where they had cleared 20, 26, 43, 66, 81 and 90%: no wall at all for anyone who'd learned
the trade. The first three dues were left alone (they're paid by nearly everyone, which is their job) and the last
went up until a first-timer was back at about one season in five: 6,000g wasn't enough (25% and 94% at the two ends),
and **6,600g** is where it sits. Then two things were set by what the reports showed:

- **The dues go up 30% a level** (`Challenges.DebtStep`; 20% since 0.33). A challenge level is always played in a
  later season, which 0.37 gives the trade's ways from its first morning, a curio from Bram's drawer and Nim two
  nights sooner (*A season after the first*), so at 20% the careful player cleared the levels as drawn 93, 87, 81,
  65, 45, 33, 31 and 19% of the time, and half of careers had cleared level 8 by their thirtieth season where none
  had. At 30% the top half of the ladder is where 0.36 left it, and the bottom half is kinder (below).
- **Heartsongs: 0.26% a chest** (`Heartsongs.Chance`; 0.32% in 0.36). With no ration every chest is shaken, so none
  that sings goes by unheard: 0.32% found 0.46 a season where it had found 0.37. At 0.26% it's 0.40 for the careful
  player and 0.27 for a first season (`--sim=songcount`).

Where it stands (`--sim=quick`, `--sim=learners`, 120 seasons each; `--sim=levels --veteran --ways=4`, 150 each;
`--sim=careers --n=60`):

| | 0.36 | 0.37 |
|---|---|---|
| The dues | 40, 170, 700, 5,000g; +20% a level | 40, 170, 700, 6,600g; +30% a level |
| The careful player, a first season | 90% | 93% |
| ... with no Heartsongs / no curios / no tales / neither | 88 / 79 / 61 / 31% | 92 / 86 / 73 / 48% |
| A first season, by skill (0.15, 0.3, 0.5, 0.75, 0.9, 1) | 20, 26, 43, 66, 81, 90% | 18, 33, 53, 78, 85, 93% |
| A later season, the same hands | the same | 29, 43, 66, 86, 95, 99% |
| A first season with Bram's lessons on (120 first-timers, `--sim=experience --n=120 --runs=1`) | not measured at this size | 17% |
| The levels as drawn, 1 to 8 (the careful player) | 77, 57, 43, 35, 18, 16, 9, 6% | 89, 80, 63, 36, 25, 13, 9, 5% |
| Paid off early | two seasons in three | four in five (18% in week 3, the rest in week 4) |
| Profit a customer, by market | 7, 14, 85, 150g | 7, 14, 114, 151g |
| Gold things a season | | 0.7 (none in half of seasons) |

The middle of the learning curve is ten points kinder than 0.36's and the two ends are where they were: the ear cost
most to whoever was half-way to knowing the trade, and that's who got it back. A later season is kinder again, on
purpose. What that does to a career is in *Balance across a career*: the first clear still comes on the second or
third season, the story's ending around the fifth where it was the tenth, and the top of the challenge ladder is
the wall it was.

**Paying it off early.** Whenever you have all that's left of the debt (`GameState.DebtLeft`: this week's due if it's
unpaid, and every week's after it to the last), you can pay it at once: from the day-end screen's Guild panel, which
shows what's left and turns into a "Pay it all off" button (with a confirmation), or on the Guild's night beside the
week's due. The debt is cleared and the run is won, however early (`ClearedDay`; the profile keeps your fastest).
Keep trading after, and nobody comes until week 8, when the Guild's interest starts (half as much again as the week
before, every week; twice, from v19 until 0.30). Since v18 a run that takes off pays it off a week or two early (or buys the Bookkeeping Engine for the same sum and finishes fast); the rest meet the
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
20g at the stall and he slips you 30g; 30g/40g in the village, 50g/80g in town, 270g/400g at the Emporium (0.30; the
stash follows what a shop's chests cost). Twice a run (three times before 0.20's shorter runs) (`GameState.StashLeft`);
the day card says so, with how many are left, and Bram explains it the first time. The careful player hardly needs it
(about once in two or three runs).

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
30g at Thornwick, 200g in Crownhold and 2,000g at the Emporium. Each move inward is the big step; within a shop, the
upgrades and the build grow it.

**The power index** (`GameState.PowerIndex`): what an ordinary thing a seller brings fetches here, luck and all, against
its plain worth, averaged over the shop's kinds by their share of what its sellers bring (`Market.ValueShares`), at the
typical colour. In it: the buyers' trades and the Dragon's Egg; the shop's name for a kind (an active synergy), the
trades' curios and the gallery lamps; and luck (the restorers, the Lucky Shop, the luck curios), at half what it adds
(`GameState.LuckPricedIn`: a seller prices the middle of a chest's range, not its best day, and 0.20 found that luck
priced in whole made the careful player, who goes by the odds bar, buy less), a lucky coin's chest weighted by how many
chests a day are bought and a clover's by the week. Left out: what no seller can price (enchantment, false bottoms,
wards, a Heartsong in the chest, a shooting star), your eye (fees and bonuses for a perfect read), what's taken off a
price (a haggler's tricks, a sealed bid) and today's event. With the upgrades a careful player owns by each move it's
1.0 at the stall and Thornwick, about 1.3 in Crownhold and 1.7 at the Emporium (a working purse of 26g, 160g, 890g and
5,420g), and with every upgrade 2.7 (8,660g); `--sim=index` prints it shop by shop, and each curio's share of it. Only
synergies, curios and Heartsongs take it further, and a chest's colour stops at purple.

**Prices** (`GameState.PriceFactor`, used as `Boosts.Dearer`): a seller's ask is their estimate, times their own
markup and the dice, times the index, times what puts prices up or down besides (the Guild's sharp sellers, the
Lullaby Box, a foreclosure sale, the barker's horn, the cursed crescent). The chest's colour and the seller's habit
stay their own multipliers.

**The margin is the growth rate.** What a gold spent on chests brings back (what they held over what was paid) is how
fast a careful player's purse grows; buying blind has next to none. Measured by the index when each chest was bought
(`--sim`, *Margins*):

| Index when bought | under 1.05 | 1.05-1.3 | 1.3-1.7 | 1.7-2.2 | 2.2 and up |
|---|---|---|---|---|---|
| Careful, no curios | 1.85 | 1.75 | 1.75 | 1.70 | 1.83 |
| Pays the asking price | 1.17 | 1.14 | 1.21 | (1.24) | (1.18) |
| Careful collector, with curios | 1.85 | 1.85 | 2.06 | 2.06 | 2.50 |

Ordinary growth leaves the careful player's margin where it was and the blind buyer's next to nothing: the numbers
get bigger, not the game easier. The collector's margin climbs as its build comes together: the curve gets steeper.
(Bracketed: a blind buyer rarely gets that far, under 1% of their chests.)

**Dues against the ceiling.** The dues climb faster than ordinary growth, and the last week's 45,000g is more than the
Royal Emporium makes with every upgrade on a careful player's margin: a careful player who buys no curios clears about
one run in seven, most of those on a Heartsong's luck, against a third for one who builds as Nim's curios come,
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
play of their own: 78 curios in all, and 0.22's Guild Promissory Note (Mara's Favour, a talent until then) makes 79.
0.36 added two for its counted listens (an Echo Stone, Lamb's-Wool Plugs), which went out again with them in 0.37
(a save holding one is paid what it cost), and 0.37 adds the six that do a build's routine (*Listening is free, and a
build does the routine*): 85.

**Every card does what it says** (0.35). The owner found curios that didn't seem to behave as described, so each of
the 79 was audited against its card: `tools/curiotest` (a console program that compiles Core and runs 200 checks, a
card's every promise against the game, most as twins with and without the curio; `dotnet run --project
tools/curiotest`, `--only=key,key`) and the write-ups beside it (`AUDIT_A.md`, `_B`, `_C`; 0.37's six, and the
trade's ways, are `ChecksD.cs` and `AUDIT_D.md`: 217 checks now). What it found, and what changed:
- A curio's few percent off a price vanished in rounding (a sum is rounded to the coin up to 30g, to the 5g above), and
  a sealed bid's price came back wrong once handled. A visit now keeps its exact price and rounds what's shown and paid
  to the coin, at least a coin off for a discount and on for a markup (`Visit.Rounded`).
- A seller's first visit (a lesson) and a tale's chest went without the build: a lesson's price, the Closing Bell, the
  Waiting Game, a signet's patience, a golden tongue, an ear trumpet. They follow it now (`GameState.Lesson`,
  `Welcome`), and the day's curios (the beads, the pillow, the seal, the compass) come after the tales have brought
  their chests; a dreaming or tallying day restates a visit in place, keeping a friend's waived terms.
- "Talked down" is measured from the price on the table when you made your first offer (`Visit.OfferedAt`), so a
  curio's discount isn't a bargain and an angry seller's raised price is what you talk them down from.
- Luck: the opening thanks the curio whose rule reached each thing; a gambler's token swings up as well as down at
  purple (a purple thing shines); a shooting star makes a purple thing shine, and looks after the dry spell's lift; a
  hollow rod's and a ward-key's step reaches a purple thing too (luck can make a hidden thing shine); a wishbone counts a
  shine as a surprise; the cursed crescent stops the dry spell too, and sellers ask its 5% less, no more.
- What a curio makes saleable (a still's broken glass, a desk's sand, a loupe's paste, the mark's fakes) sells as its
  real kind does here, the shop's trades and lamps included; a thing kept on the shelf sells (or pawns) for at least
  what it would have fetched at its opening (`GameState.ShelfValue`); points on the screens are the exhibition's own
  (`GameState.Points`: a dummy's cloaks, a velvet rope).
- The register counts the read it's paying for; a crate or a mimic is no read for a tuning fork's streak; a blank slip on
  a chest bought sight unseen is no "wrong slip" for the ledger; a kind marked enchanted reads every enchanted one of it
  right (an archmage's staff could never be read perfectly); "Pay it all" on a Guild night pays the due first, so a
  miser's lockbox grows the purse; the note and Bram's word work after the debt's weeks; a patron's ribbon raises the
  whole delivery; the letter's leeway is the regulars' (not the crier's); the errand book's extra commissions can ask
  for a kind twice; the compass puts a false bottom in every chest, and the warden's lantern holds on a dreaming day.
- Cards that promised what no rule could keep say what's true: Old Aldo (whose price is your slip) and the Maw don't
  haggle (crooks do now); the pillow's fifth and the seal are of the sellers with nothing else to tell you; the mark
  brings crooks more often (nobody comes twice a day); the guest book's colour stops at purple.

**Rarity** (`CurioRarity`: common, uncommon, rare and legendary; green, blue, purple and gold on Nim's cloth,
`Palette.Of`). Each card's rarity is rolled first, by week (`Curios.Rates`): commons and uncommons only in the first
week; rares from the second (5% of cards, rising to 15%); legendaries from the third (2% rising to 6%). A rare needs a
shop at Thornwick and a legendary one in Crownhold (`Curios.RarityTier`). A legendary is a way of trading's capstone,
opened for good by doing that trade often enough across runs (`Feat`, `Curios.Feats`: 40 surprises for Fate's Spindle,
40 perfect reads for the Truthglass, 25 chests talked down for the Golden Tongue, 15 false bottoms opened for the
Smugglers' Compass, 10 warded things named for the Aldric Seal, 15 enchanted things for the Archmage's Staff, 20 chests
taken sight unseen for Fortune's Blindfold), and the collection's curio page shows how far along you are.

**Prices: a fixed sum and a share** (`Curios.Price`): 10g and 4.5% of what was in your purse when Nim came by for a
common, 25g and 7.5% for an uncommon, 75g and 11% for a rare, 250g and 15% for a legendary (0.28: a quarter less than
before, now that Nim comes every night and there are more nights to buy on). The fixed part keeps a curio
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
Wards still need naming: that's the warded-thing puzzle itself, not a condition on a bonus. (Since 0.37 each main way
of trading but sight unseen also has a curio that does its routine at the counter: the table is in *Listening is
free, and a build does the routine*.)

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
|  | Aether Jar | U, T3 | Each enchanted thing you find this run makes everything 2% likelier to be enchanted (up to 20%). | once you've had a shop in Crownhold, or after two runs |
|  | Enchanter's Chalk | U, T3 | An enchantment spreads: a chest with an enchanted thing in it has a second one. | once you've had a shop in Crownhold, or after two runs |
|  | Spellbinder's Ring | R, T3 | Enchanted things are worth four times as much, not twice. | once you've had the Royal Emporium, or after four runs |
|  | Lantern Moth | R, T3 | Every enchanted thing in a chest is starred on your slip as the chest comes in, witchlight or not. | once you've had a shop in Crownhold, or after two runs |
|  | Archmage's Staff | L, T3 | Everything in a chest with an enchanted thing in it is enchanted. | after 15 enchanted things found, all told |
| sight unseen | Sealed Bid | C | Buy a chest sight unseen and its seller knocks 15% off. | from the start |
|  | Lucky Dip | U | Everything in a chest you take sight unseen is 15% likelier to come out a step better. | after your first run |
|  | Underwriter's Bond | U, T2 | A chest bought sight unseen holds less than you paid? The Underwriters pay 40% of it back. | after your first run |
|  | Wholesaler's Tally | U, T2 | Each chest you buy makes every seller after it today ask 4% less (up to 20%). | once you've had a shop in Crownhold, or after two runs |
|  | Fortune's Blindfold | L, T3 | Sight unseen, a chest costs 30% less, and nothing in it comes out worse than promised. | after 20 chests taken sight unseen, all told |
| haggling | Merchant's Signet | C | Hagglers hear you out (an offer too low gets a second try), and come down 10% further. | from the start |
|  | Silver Tongue | U | Hagglers come down a fifth further, and those who wait for an offer take a fifth less. | after your first run |
|  | Waiting Game | U | Every chest you pass makes the next seller ask 5% less (up to 20%). Buying starts it again. | from the start |
|  | Closing Bell | U | The last customer of the day asks 25% less. | from the start |
|  | Haggler's Knot | U, T2 | Each chest you talk down this run makes every seller ask 1% less (up to 8%). | after your first run |
|  | Factor's Abacus | R | You know the least a haggler will take: your offer starts on it, and they take it. | after your first run |
|  | Haggler's Purse | R, T2 | Everyone but Old Aldo and the Maw haggles with you, and 40% of whatever you talk them down by comes back to you as well. | once you've had the Royal Emporium, or after four runs |
|  | Golden Tongue | L, T3 | All but Old Aldo and the Maw haggle and never walk out: go as low as you like, and they come down to 10% under their least. | after 25 chests talked down, all told |
| the perfect read | Ear Trumpet | C | Nothing drowns out or muffles a sound for you: no clank, and no straw. | from the start |
|  | Pocket Watch | C | Impatient sellers give you twice as long, and a rotten chest stands twice the shaking. | from the start |
|  | Counting Beads | C, T2 | A chest its seller says nothing else about comes with a tally of every thing in it, false bottom and all. | from the start |
|  | Appraiser's Seal | U | A perfect read pays you a valuer's fee: 8% of what the chest held. | from the start |
|  | Scrivener's Quill | U | Your slip ticks each thing as you hear it, and counts them. What you work out is still yours to mark. | after your first run |
|  | Cursed Crescent | U | Everything comes out exactly as its chest promised. Sellers sense the curse and ask 5% less. | after your first run |
|  | Valuer's Register | U, T2 | A perfect read pays 0.5% of what the chest held for each one so far this run (up to 12%). | after your first run |
|  | Tuning Fork | R | Each perfect read in a row: the next chest's things sell for 3% more (up to 30%). | once you've had the Royal Emporium, or after four runs |
|  | Ledger of Certainty | R, T2 | A perfect read: everything in the chest sells for 15% more. A wrong slip: 15% less. | once you've had the Royal Emporium, or after four runs |
|  | Truthglass | L, T3 | A perfect read: everything in the chest sells for half as much again. | after 40 perfect reads, all told |
|  | Pocket Engine | L, T3 | Every chest is read for you: what's truly in it is on your slip as it comes in, false bottom and warded thing and all. | after 100 perfect reads, all told |
| false bottoms | Padded Crate | C, T2 | Glass never breaks when you drop a chest. | from the start |
|  | Smuggler's Hook | U, T2 | Smugglers come your way: false bottoms turn up twice as often. | from the start |
|  | Hollow Rod | U, T2 | What's under a false bottom comes out a step better: people hid their best. | after your first run |
|  | Smuggler's Log | U, T2 | Smugglers trust a buyer who keeps quiet: each false bottom you open this run makes false bottoms 2% likelier (up to 20%). | after your first run |
|  | Toll-man's Gauge | R, T2 | A false bottom is marked on your slip as the chest comes in: no drop needed. | after your first run |
|  | Black Market Pass | R, T2 | Thornwick's black market pays double for anything from under a false bottom. | once you've had the Royal Emporium, or after four runs |
|  | Smugglers' Compass | L, T3 | Every chest has a false bottom. | after 15 false bottoms opened, all told |
| wards | Warden's Lantern | C, T2 | Every sign a seller leaves (a dream, a tag, a riddle, a card...) points at a warded thing. | from the start |
|  | Sleeper's Pillow | U, T2 | Sellers dream true for you: 20% of those with nothing else to tell you have warded something, and tell you their dream. | after your first run |
|  | Wardbreaker's Tally | U, T2 | Each warded thing you name this run makes warded things sell for 10% more. | after your first run |
|  | Ward-Key | R, T2 | A warded thing you name comes out a step better. | once you've had the Royal Emporium, or after four runs |
|  | Wardens' Primer | R, T2 | When a sign points at a warded thing, your slip names it for you. | after your first run |
|  | The Aldric Seal | L, T3 | Every seller with nothing else to tell you has warded a thing in their chest, and tells you a sign for it. | after 10 warded things named, all told |
| luck | Lucky Shell | C | A dry spell ends twice as soon: luck starts to turn after 5 plain finds, not 10. | from the start |
|  | Gambler's Token | U | Twice as many surprises in everything you open: good ones and bad ones. | after your first run |
|  | Lucky Coin | U | The first chest you buy each day: each thing in it rolls its quality twice and keeps the better roll. | from the start |
|  | Rabbit's Foot | U | Everything you open is 10% likelier to come out a step better than its chest. | from the start |
|  | Wishbone | U | Something that comes out better than its chest sells for half as much again. | after your first run |
|  | Horseshoe | U, T2 | Each chest in a row with a surprise makes the next one's things 10% likelier to come out a step better (up to 50%). | after your first run |
|  | Four-Leaf Clover | R, T2 | The first chest you buy each week: everything in it comes out a step better. | from the start |
|  | Shooting Star | R, T2 | Once a day, the first thing to come out better than its chest climbs a step further (a purple thing shines). | once you've cleared Bram's debt |
|  | Fate's Spindle | L, T3 | Each surprise this run makes everything 2% likelier to come out a step better (up to 50%). | after 40 surprises (things better than their chest), all told |
| crowds | Shop Lantern | U | Open late: one more customer every day. | from the start |
|  | Queue Ticket | U | The first chest you pass each day comes back at the end of the day, asking 20% less. | from the start |
|  | Barker's Horn | U, T2 | Two more customers every day, but word's out: everyone asks 5% more. | once you've had a shop in Crownhold, or after two runs |
|  | Guest Book | U, T2 | The day's first customer is the better sort: their chest is a colour finer (as far as purple). | after your first run |
| display | Collector's Catalogue | C | Every new kind of find this run pays 4g (more in a grander shop). | from the start |
|  | Bell Jar | C, T2 | Room to display one more thing. | from the start |
|  | Visitors' Book | U, T2 | Every night, visitors leave 6g per exhibition point (more in a grander shop). | from the start |
|  | Velvet Rope | U, T2 | Your exhibition counts double. | once you've had a shop in Crownhold, or after two runs |
|  | Gallery Lamps | R, T2 | Everything sells for 1.5% more per exhibition point (up to 60%). | once you've had the Royal Emporium, or after four runs |
| a trade | Smith's Glove | C | You mend weapons: never worse than their chest promised, and 25% likelier to come out better. | from the start |
|  | Hatter's Cap | C | You mend cloaks: never worse than their chest promised, and 25% likelier to come out better. | from the start |
|  | Whetstone | U | Weapons sell for three-quarters more. | from the start |
|  | Tailor's Dummy | U | Cloaks sell for half as much again, and count 3 times over on display. | after your first run |
|  | Moneychanger's Scales | U | Coin sells for a fifth more for every purse of it in the chest. | after your first run |
|  | Alchemist's Still | U, T2 | Potions sell for half as much again, and broken glass still sells for half. | once you've had a shop in Crownhold, or after two runs |
|  | Scribe's Desk | U, T2 | Spells sell for half as much again, and sand sells as ink, for a third of a spell. | once you've had a shop in Crownhold, or after two runs |
|  | Gem Cutter's Wheel | U, T3 | Gems are 50% likelier to come out a step better. | once you've had a shop in Crownhold, or after two runs |
| rough hands | Locksmith's Oil | C, T2 | Every lock has one pin fewer. | from the start |
|  | Burglar's Knife | U, T2 | Forcing a lock breaks nothing and scratches nothing. | from the start |
|  | Kid Gloves | U, T2 | Nobody minds a hard shake, a drop or the lodestone on their chest. | from the start |
| fakes | Jeweller's Loupe | C, T4 | Paste gems and demon gold sell as costume pieces: a quarter of the real thing. | from the start |
|  | Fence's Mark | R, T3 | Crooks come by more often, and the Maw's buyers pay for any fake what the real thing fetches. | once you've had the Royal Emporium, or after four runs |
| coffers | Rainy-Day Purse | C | Bram's stash helps you out three times a run, not twice. | from the start |
|  | Pawn Ticket | C | On a short night, what you pawn fetches four-fifths of its price, not three-fifths. | from the start |
|  | Guildsman's Marker | C | Bram's word is good twice a run, not once. | from the start |
|  | Savings Jar | U | Every night: 3% of your gold as interest (up to 20g at the stall, more in a grander shop). | from the start |
|  | Guild Promissory Note | R | The Guild's own paper: once a run, on a Guild night you can't cover, it pays what's short (not the last week). | once you've had a shop in Crownhold, or after two runs |
|  | Miser's Lockbox | R, T2 | Every Guild night, once you've paid, your purse grows by 25% of what's left in it. | once you've had the Royal Emporium, or after four runs |
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
a chest with a flump in it is worth chasing when Ada wants silk.

**Always more than selling** (v22, `GameState.CommissionPay`). What a commission pays for a thing is its share of the
reward, and never less than a quarter over what that thing would sell for here (`Patrons.Premium`): a thing better
than asked, an enchanted one, a shop whose buyers' trades put everything up, all pay more delivered than sold. Each
thing pays as it's handed over, a lot's pieces too (they used to pay only when the last came in, and a lot that
lapsed had cost you its sales). The deliver button says what it pays; the board says what each piece is worth at least.

## Events

A day or two a week (none in the first, which is for learning the stall; one in weeks 2 and 3; two from week 4), on
days of their own, something happens in town (the day card
announces it, and a chip in the top bar keeps it there all day: click it to read it again; `Events`). Each week's plan
(its two event days, the kind wanted, and Nim's day) is made a week ahead (`Events.Plan`, `GameState.Plan` and
`NextPlan`, saved), so the notebook's calendar can show this week and next. They all come round before any comes twice;
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
| The Sleepers' Moon | Thornwick | everyone dreamt true, like Mother Sorrel: every ordinary seller tells you their dream (of the thing they warded, if they warded one, which most did; else of the dearest thing that can sound: a dream never reaches under a false bottom) |
| Wardens' Day | Thornwick | the old custom: every ordinary seller has warded one thing and left a riddle for it |
| The Guild's Count | Thornwick | a Guild clerk tallies every article in every chest, false bottoms and all (Brass's ticket, for everyone) |
| The Guild's Assessor | Thornwick | a tenth of every sale goes to the Guild |
| A Barge Aground | Crownhold | a treasure barge on the Aldwater: lots come in twos and threes |
| A Witchlight Moon | Crownhold | the old enchantments wake: things are far likelier to be enchanted |

The puzzle days (`Events.Restate`) give every seller without a clue of their own (and not the King, whose price is your
slip) the day's clue, so the whole day is a run of Mother Sorrel's and Lady Isaura's puzzles, or of Brass's
counting.

## The notebook's calendar, and the debt

The notebook's Calendar page (it replaced Places, which was reference nobody needed once every seller says where their
chest came from) opens with the Guild's debt, once Mister Vane has told of it: what's left of it, the next due and
when, and a box for each of the five weeks (paid in green, this week's framed, orange when the purse is short), then
what comes after. The due in the top bar opens the notebook right there (0.25): the debt is what the run is about, and
it was the one thing on screen that didn't explain itself. Under the debt, three weeks, a day to a box: today framed
in gold, the days gone by faded, each day's event (this week's and next week's are planned), the morning Nim comes by,
and each Guild's night with what it will want (this week's in red if the purse is short). Hovering a day tells its
whole story; until then, the next thing coming is told under the grid. Nim comes every week from the first (its third
morning), on a day without an event when he can.

## The people and their tales (0.28)

**0.30: what tales leave behind.** The owner: side stories should change the game, not just the Town page (if the
Guild takes the child, he stops coming, and a friend of the authorities might find their officials selling to them; if
he stays, his mother might come back with a discount), interconnect, and reach the ending; and there should be many
more of them. So 0.30 adds fifteen tales (four at Brindle, five at Thornwick, four in Crownhold, two at the Emporium; with 0.28's, a
season draws three of each place's seven or eight, both of the Emporium's, the Shut Chests and the King's Court) and
gives every tale, old and new, consequences in
play: officials who sell only to the authorities' friends (`SellerDef.Comes`), better terms from people you've helped
(`Goodwill`), favours called in on a short Guild night (`Owed`), papers for a King's court that can put the Guild on
trial (`Evidence`), reading contests (`Visit.Contest`), a moneylender (`Borrow`), effects that last a while (a rival
across the road, pickets at the door, the Field's dead: `Until`), nights away from Nim, a shard of the Key by another
road, and the Maw's bargain, the debt paid for the Key. The ending reads all of it: the gathering in week 6 and the
Crown Chest night name who stands with you, the run-end title changes (THE GUILD ON TRIAL, THE GATE OPENS), the score
counts friends made and the trial, and after the last scene the season's end tells what became of everyone whose tale
began (`TaleDef.Ending`). `docs/stories.md` has every beat; the sim's "Tales" section reports how each tale went.
(0.34 took the shard and the Maw's bargain out with the Key: the gathering asks who'll stand with you at the treasury,
two friends are the plan's way out, and the run-end titles are the treasury's: *The story*, below.)

The owner, after playing a shopkeeper game whose customers had lives (revolutionaries and police, thieves and wanted
notices): the people at the counter were flat, the only progression across encounters was the Key's shards, and the
nightly shop wasn't a moment to look forward to. 0.28 thought the premise through (`docs/world.md`: what three hundred
years of locked treasure made of Aldmark, who rules it, who wants what from a Keywright) and rebuilt the cast and the
evenings around it. `docs/stories.md` has every tale beat by beat; this is how it works.

- **The order of things.** A chest opens only for a Keywright who rightfully owns it, so the Keywright is the realm's
  moneychanger and its judge of whose a chest is. Stolen chests won't open for anyone but their owner however often
  they're sold: theft is ransom, the Wardens of the Road keep the Chest Roll and post wanted notices, the Lockless (the
  Magpie's people) fence what won't open, and the Gilded Guild forecloses, which the ward doesn't count as a sale.
- **The cast** (`Sellers.All`, `Sellers.People`): every regular kept an old regular's trading rule (the puzzle's the
  same) and got a name, a place, a faction (`Of`), a want (`Wants`), and what you notice about them (`Facts`). People who
  only come when a tale brings them (a Warden sergeant, a Guild clerk, a Mark-Finder, a mother, one of the fair folk)
  are in `Sellers.People`. Until the new cast is drawn they wear the old figures (`Look`).
- **Tales** (`TaleDef`, `TaleBook`; the engine in `Tales.cs`): the Shut Chests every season, and six more drawn from the
  seed (two of Brindle's three, one of Thornwick's two, two of Crownhold's four, the Emporium's). A beat is one person
  at the counter, or at the back door after dark, or a letter; it comes a set number of days after the one before, at a
  grand enough shop, if what you chose before allows. Two a day at most, one a night. A talk beat says its piece and
  asks a question (`TalkPhase`): each answer has a price (on its button), a reply, and what it does. A trading beat brings
  a chest, and what you do with it is the answer. A tale's secret (who the Magpie is, what's behind the Barrow door) is
  drawn from the seed when it begins and never changes; nothing is a hidden roll.
- **What choices do.** Gold and chests given freely (they open), curios, tools for nothing, people gained (Pip as your
  apprentice, Jory as your porter) or lost for the season (jailed, taken into the Guild's care, down the Barrow), the
  Crown's decree on the tithe, a cracked Gate, and standing.
- **Standing** (`Faction`, -3 to +3, `GameState.Sway`): nine powers keep an account of you. At +2 or -2 the four that
  matter most change the trade every day: the commons a customer more or fewer, the Guild a tenth off or on every due,
  the Wardens warn you or search your back room every Guild night, the Lockless pay double at the fence or rob you once a
  week. The rest price their people's tools. The notebook's **Town** page shows where you stand, the notices on the
  board, every tale begun and what's happened in it; the calendar marks who said they'd come back, and when.
- **Stolen chests**: Flick (the Magpie's runner) mostly sells them, Corvin now and then, and tales bring some. Lay a
  hand on one and the mark goes cold (`CounterPhase.MarkAnswers`): from Thornwick you can send for the Wardens (the
  seller runs, the chest goes home, a finder's share is yours, the Lockless don't forget). Bought anyway, it won't open
  and waits in the back room, to be returned for the finder's share or sold to Nim's friends at night. A blind buyer
  pays for every one of them.
- **Tools from the counter** (`Offers`, `Upgrade.By`): the best tools are brought by the people who make them, at their
  standing's price, never on the first day at a new place, and come back a few days after a no once the morning's purse
  could pay them (the experience playtest: the first offer on the move to Thornwick made that day the heaviest of a
  first run, and makers calling again and again on a shop that couldn't pay made a run ten minutes longer for nothing).
  The day's end keeps the building.
- **Nim every night** from the third (`StartNight`, `PeddlerPhase`, `Curios.FirstNight`: on the first two as well, the
  guided first day was too much at once): one curio at first, one more for each of his own upgrades (a
  lantern, a stool, a standing order), which he offers at his table with the curio cases. Nothing to reroll: there's
  always tomorrow night. The night also brings a tale's knock, the Wardens' search or the Lockless' visit, and the back
  room.
- **Balance** (`--sim`, 300 runs): the careful collector clears 47% (v22: 48%), 41% with no tales; 36% buying no curios
  (v22: one in five: the tales' gold, tools and standing carry a run without a build) and 43% with no Heartsongs; a first
  run 4%. A careful season meets about 31 people come to talk (18 of them a tale's beats), buys three tools at the
  counter and two of Nim's upgrades, sends for the Wardens twice, and ends a friend of the Wardens (93% of runs) and of
  the Guild (half), a foe of the Lockless a quarter of the time and of the Arcanum two runs in five. It names the Magpie
  in a quarter of seasons and keeps quiet in half (the Lockless' friendship is worth more to it than the bounty), takes
  Jory on as a porter three times in four, and stakes the Barrow nearly always, paying for the rescue two times in five.
  The week 3 Guild night, which 0.27 never sent a careful player to Bram's word for, now does about one run in thirty: a
  tale gone wrong. 0.28 first set the last due at 55,000 for the tales' gold; the tales now pay half what they first did
  (the bounty, the blood, the Roll's sale, the clerk's tip), Sir Rowan sells his chest rather than giving it, the King's
  chest holds the crown and one thing, and the due is 45,000 again.

## The story

The world's story (`docs/world.md`; what's true underneath it, and why it's told this way, `docs/narrative.md`) is told
in scenes (`ScenePhase`, `Story`) on story mornings, the second evening, Guild nights, at the treasury and at the end,
and found at the counter. 0.34 rebuilt it around the owner's narrative proposal as a campaign across seasons, in place
of 0.33's mystery (the Gilding, Bram's glove, the notebook's Case page of sentences with blanks). In the player's words:
first I open chests to make a living and bring my uncle home; then I find out that some of the things being traded are
people; finally I open the greatest chest in the world knowing exactly whom I mean to bring out. The beats are **Bram's
Account** in `docs/stories.md` (`TaleBook.Campaign.cs`); the campaign's state is `Campaign.cs`, the notebook's Inquiry
`Inquiry.cs`, and the treasury `TreasuryPhase`.

- **A first season** (and every season until the treasury swallows you) is ordinary business, told kindly. Bram minds
  the stall with you for the first week (he taps the counter twice and once when he wants you); the second evening
  Mister Vane introduces Bram's twenty-year account, the season's debt; Garrick brings a green chest that's the same
  every spring, and Nell Haddon says the Guild has her father's note, due on its first night (pay it, forty gold, and he
  stays home). On the first Guild night the treasury summons Bram for "an old consignment", and he goes (Old Haddon too,
  if his note's unpaid); from then on his tips are notes in his notebook's margins, his promised letters never come, and
  the Guild's do ("Bram sends his regards"). In week 2 the Guild's letter sets the goal: keep his account paid, and earn
  the **treasury key**'s four pieces, each sent by the Guild's assessors with a master's fee of one and a half working
  purses for a mark of the trade (a hidden thing read true, or a chest of five things; a stolen chest settled; a
  commission filled; a Guild Lock picked masterfully or a Dwarven Tumbler without force), which the notebook's Inquiry
  lists under "Where is Bram?".
- **The treasury.** On the night the account is paid with the key whole (the last night, the night an early payment
  clears it, or, trading on, the night the last piece comes), the Guild's carriage takes you there and its great chest
  comes to a counter as Bram's commission, played with the counter's own tools: shaken, nothing moves; harder, not even
  the wooden ring every empty chest gives (0.34's empty box rings like a drum at a firm shake); dropped, nothing; it
  slips, the lid rises and falls, it breathes, and it swallows the screen. The season is banked, and the next starts at
  once with the same keywright at the same level: the first morning, two taps and one, and this time you remember
  (`Profile.Swallowed`; Isolde opens with it). A season that clears the debt without the key, or is foreclosed, just
  ends: Bram never came home.
- **Once you remember**, the notebook's **Inquiry** is the investigation: its threads (where is Bram?, open since the
  summons; what was the breathing chest?; why did the first morning come again?; and, later, how do we bring them out?)
  each hold entries seen, said, tried, heard or found, and leads (what you now have reason to do, shown until it's done
  this season). What you know carries across seasons (`Profile.Known`); what you hold (the register, Bram's tuning fork,
  the key's pieces) goes at the rewind. Nothing is an answer to pick from a list: each discovery opens something to do,
  and the critical ones have two roads. Garrick's chest is one you know; Bram's tuning fork turns up in a Guild sale,
  tagged from the weighhouse; Sergeant Hollis follows the Guild's carts down the palace road (or the Lantern Market
  sells a night there): the gilded are weighed, crated and registered by name. Guild crates come to the counter and
  knock while they sit still; knock back and they answer. Mimics breathe only once you remember (Brother Cadoc brings
  one every spring, and one chest in 90 breathes from Thornwick), and a full mimic eats the gold off one of the gilded;
  a note still standing is taken again on the Guild's next night. Freed, with his account paid, Bram comes home for the
  rest of the season. Mara, the last of the Keyhold, tells the seal's last clause (the safeguard that turns the year);
  Master Quire's Roll or the King tells what the Crown pledged (the treasury's keys and takings, not its people, not a
  keywright's word). Vane speaks for the Maw: reassurance, a bribe on the fourth Guild night (half the due, and Hollis
  stops talking to you), and on the sixth a bargain (Bram home tomorrow for the key's pieces, and no treasury that
  season). Once you remember you can stay away from the treasury (the Guild opens it with Bram, and the season ends) or
  be swallowed again.
- **The true ending** is a plan before it's a spectacle (`Inquiry.PlanLines`): find them (you've heard the gilded
  answer, and hold the weighhouse register), free them (the debt paid and a full mimic in your care), a way out (two
  friends who'll stand with you, as week 6's gathering asks, or the Wet Hen's bargemen), and open the door without
  giving it away (the key whole, and you know the clause and the charter). With all four, the treasury is a different
  scene: you hold still and listen, the people inside knock, you knock back, and you turn the key to look, not to own;
  they come out, and the Maw offers a private victory ("I can make this time remain. Take him home. Leave the door to
  me."). Leave the door to it, and it reaches past the door for everything, and the year turns; close the door, and the
  reward is **the next day**: a date the campaign has never reached, Bram's tea, and Wynn's ordinary chest of
  millwright's tools. After that a new season is a new year (`Profile.Finished`), and the tales, the levels and the
  collection go on.

Every season starts the same: what carries over is only what you know (and what Garrick's chest held). Scenes shrink
once you've seen them, and the sim's careful player follows the leads (`Strategy`), so its careers report says when a
career is first swallowed, first brings Bram home and first reaches the next day. The King's court still tries the Guild
for its papers (half its tithe while it's tried), but nothing is won by a count of papers, and no ending waits on the
Heartsongs. In code the main tale's key is still `commission`, the treasury's backdrop the vault (`scene_vault`), and
the old Crown Chest's finish is the great chest's; `--treasury-demo` plays the first swallowing and
`--treasury-demo=final` the final opening, `--season=N` a season you remember and `--knew=all` one in which every
Inquiry entry is known. Every run is recorded in `user://profile.json` (runs, debts cleared, the furthest week, and the
campaign: times swallowed, Bram brought home, the last offer taken, the true ending, what the Inquiry knows, what
Garrick's chest held and how the last season ended), shown on the title screen and at the end of a run.

## Across runs: who minds the stall, and challenge levels

A run's own loop (a chest, a day, a week, five weeks) needed a reason to play another run that isn't the same run
again. Since v22 nothing carries over that makes a run stronger: every run starts the same, and what grows is the
player (and what's known: the journal, what Bram has taught, Nim's stock, the legendaries' feats). Two things vary a
run, both in `scripts/Core/Meta.cs` (definitions, the score) and `Kin.cs` (what they do to a run, as a partial of
`GameState`); the profile (`Profile` in `Save.cs`, user://profile.json) keeps each character's open levels and best
score.

**Who minds the stall** (`Characters`). Each changes one part of how you judge a chest or run the shop, and pays for
it; the rest of the game is the same. Until 0.31 they were all Bram's family, so the story's lines ("your uncle") held
for each; 0.32 gave them lives of their own (the owner: not everyone needs a connection to Bram). Kit is still his niece;
Hesk (a grocer from the drowned river town of Fallowford), Rosa Brightwater (of Crownhold's Lantern Lane) and Lady Isolde
Marchmont (the last of a foreclosed house) are his partners, each born with the mark elsewhere, each with a welcome,
a season's-end line and a tale of their own every season they play, ending in a keepsake (`docs/stories.md`, "Their
own tales"). Lines that call the player Bram's kin are Kit's alone.

| Who | Unlocked by | What's different |
|---|---|---|
| Kit, the Ear | from the start | the game as it was: every strength, the drop, and (with tips on) Bram's lessons as each seller arrives |
| Hesk, the Scale | paying the third week's due | a bad back: no hard shake and no drop. Every chest sits on his scale, and he knows what each kind weighs (coin 4 lb, arms 6, garb 3, potions 3, scrolls 1, gems 1); demon gold weighs half what gold does and a case of sand three times a scroll, so the weight left over once the heard things are counted is what he couldn't hear, what's under a false bottom, or a fake weighing short |
| Rosa, the Tongue | clearing the debt | everybody haggles with her (fixed-price sellers ask high and hear one offer instead, though never more than a bit over the price they'd have named, nor holding out for more than it; a crook still names his too-good price, and nobody haggles with the King or the Maw), but they know her: they open 3% higher and come down 3% less; an offer too low gets a second try; a seller who walks out on her doesn't come back that week |
| Isolde, the Collection | opening the great chest at the Guild's treasury (0.34; the Crown Chest until then) | a two-place shelf from the stall on; visitors pay 2g for every five exhibition points every night, at any shop (`Characters.DoorCoins`, `DoorPoints`, 0.30); her collection can't be pawned, so a short night can only take her curios |

Every new judgment follows a rule people know: things weigh what they weigh, and gold is heavier than iron (the old
test for a gilded crown). Hesk's scale shows the total and the slip shows what the ticked things would weigh; the sum
is the player's to do. The careful player's read is `Strategy.Weigh`: the likeliest mix of garb, scrolls, a false
bottom and short-weight fakes for the leftover, given the seller's habits. Bram's lessons are Kit's (the others come
knowing the trade, `GameState.GivesLessons`); each of the others gets a first-morning tip on their way (`kin_*` in
`Hints.cs`). Curios a character can't use aren't in Nim's stock (`GameState.NoUse`: no padded crate for Hesk, no
signet for Rosa, no visitors' book for Isolde).

**Challenge levels** (`Challenges`, v24). Eight, opened one at a time per character by clearing the debt at the level
below, each a quarter more to a run's score (x3 at level 8). A level does two things:

- **The Guild's dues** go up by 30% of what they were at level 0 for each level (`Challenges.DebtStep`,
  `GameState.DueFor`; 20% from 0.33 to 0.36): level 8's are 3.4 times as much. 0.37 put it up because a level is
  always played in a later season, and a later season now opens with more (*The run*, "0.37": at 20% the levels as
  drawn cleared 93 down to 19%). (A tenth a level until 0.28, whose tales carried the top levels
  further: Kit cleared level 4 a third of the time and level 8 a fifth, against 0.27's 23% and 13%; 15% until 0.33,
  whose case (the Case page's payoffs, which 0.34 replaced with the Inquiry), mimics and Lantern Market did the same:
  120 careers cleared level 6 by run 30 32% of the time and level 8 7%, against 0.32's 23% and 4%, and at 20% a level
  21% and 3%, level 4 half by run 26 against 0.32's 25.)
- **Its terms**: the run is dealt as many **banes** as the level's number and as many **boons** (`TermDef`,
  `Challenges.Draw`), from twelve of each (thirteen in 0.36, whose head cold and rested ear went with the counted
  listens). The deal is nested (a bane, then a boon, from one
  shuffle of each pool), so a level's terms are the level below's and one pair more, and a boon never comes with the
  bane it contradicts (four pairs: savings and a lean start, Thornwick's welcome and the Guild's builders, a slow
  season and a hurried one, Bram prepared and Bram stretched thin). A real run deals from `Profile.TermsSeed` (the number of runs started), so the
  setup screen shows exactly what will be played; the pause menu shows them during the run, each explained on hover,
  and the save keeps them.

Banes make the trade harder; boons smooth over what's most frustrating about it (a slow start, a short night, a
sweeping clock, a lock that slips, a curio you can't find), so a harder season is never all bad, and two runs at the
same level differ. Every term is a rule that holds every time, with a reason in `docs/world.md`, routed through
`GameState.Under` or `Favoured` where its rule lives (so the sim and the bot play it too). There are twelve of each.

What each does alone to the careful collector, **at level 3's dues** (Kit, 150 seasons each, 47% with none;
`--sim=terms --level=3`; six points either way is noise at that size, and the five marked * were measured again over
400, 46% with none). At level 0 the careful collector clears nine seasons in ten whatever the terms, so a term alone
hardly shows there, which is why 0.36 weighs them at a level where half are lost.

| Bane | Rule | Cleared | | Boon | Rule | Cleared |
|---|---|---|---|---|---|---|
| Thin crowds | the week's last two days, a customer fewer | 38% * | | Welcome gifts | the first two customers' chests for a coin each | 49% |
| Sharp sellers | hagglers' least is 15% higher | 45% | | Nim's soft spot | Nim shows something else twice a visit | 48% |
| Demon gold about | one coin in ten is demon gold | 45% | | Bram's savings | half as much gold again to start | 52% * |
| A hurried season | every seller gives you 20 seconds | 46% | | Bram's old picks | the Fine Lockpicks from the start | 46% |
| Fussy sellers | everyone minds a drop | 45% | | Thornwick wants you | the second due, the one paid at Thornwick, a quarter lighter | 54% * |
| Bram's stretched thin | a stash dip fewer, Bram's word once fewer | 46% | | A slow season | twice the patience and twice a rotten chest's shaking | 51% |
| A lean start | half the gold to start | 41% * | | Bram's prepared | a stash dip more, Bram's word once more | 47% |
| Nim's raised his prices | curios a quarter dearer | 43% | | An honest clerk | pawning at full price | 50% |
| The Guild's builders | every upgrade a quarter dearer | 42% | | A good name | hagglers hear you out | 48% |
| A wet year | a pin more in every lock | 45% | | A lucky charm | the dry spell ends twice as soon | 46% |
| Wary regulars | a commission fewer a week | 42% * | | Fair days | no mourning, no assessors | 48% |
| Nim's lean month | a curio fewer on Nim's table | 45% | | Room for more | a curio slot more | 49% |
| A head cold | a listen fewer every day | 44% | | A rested ear | three listens more every day | 50% |

Every bane costs something (one to eight points) and no boon costs anything that isn't noise; thin crowds is the
heaviest bane, as it was in 0.35, and Thornwick's welcome the best boon. A level's dues do most of the work (alone
they clear 72, 59, 33, 12 and 7% at levels 1, 2, 4, 6 and 8; the levels as drawn 77, 57, 43, 35, 18, 16, 9 and 6%,
each harder than the one below), and the terms are what makes two seasons at a level play differently.

Three terms were changed in 0.36. Thornwick's welcome halved the price of the move until there was no move to buy,
and then half the second due, which was worth thirteen points; it takes a quarter off now. A head cold took two
listens a day and eight points, and takes one; a rested ear gave two listens and one point, and gives three. A listen
lost is one of the eight a day was planned around; a listen more is a ninth check on a chest already read.

Several boons hardly move the careful collector, who never needs a second look from Nim or a second try with a
haggler and has a steady hand on a lock: they're there for people. Bram stretched thin doesn't change how often the
careful collector clears, but ends a run that would have leaned on Bram's word a week sooner.

Whole levels as drawn (Kit, 200 runs each; 0.33, the dues 20% up a level, `--sim=levels`): 56 (level 1), 46, 42, 32, 24,
19, 19 and 9%. (Before 0.33, at 15%: 49 (level 0), 41, 33, 29, 24, 20, 20, 17 and 16%; v22's fixed terms: 48, 48, 42,
32, 24, -, 11, -, 4%.) A step of 7% a level flattened out after level 4 (42, 36, 33, 25, 24, 25, 21, 18%); the
first draft, at 15% a level and with the two strongest banes and boons as they first were, made levels 6 and 8 a wall
again (8 and 6%).

**Score** (`Score`). Points for the Guild dues paid (10 each), the debt cleared (100) and every day to spare (5); the
campaign (0.34): the treasury key's pieces (10 each), the treasury (100, swallowed or at the final opening, not for
staying away), Bram brought home (50, not by Vane's bargain on the sixth Guild night) and the next day (200); moves (25
each), the collection (2 per find), the Heartsongs found (20 each), every week after the debt you kept the Guild paid
(25), the Guild on trial (75) and friends made (5 for each favour owed you, called in or not); times the level's
multiplier (`Score.Final`). (Until 0.34 the Crown Chest was worth 100, or the Key's shards 10 each, and all nine
Heartsongs sung whole 200.) A foreclosed run still scores what it did. Each of Bram's kin keeps their best
(`Profile.BestScore`, on the character screen); the run's end shows the lines, the score and anything it opened (a
season the treasury swallows is banked without one, and the next begins).

**Talents: retired** (v22). Until 0.21 a run's score was renown, spent between runs on 12 talents in a tree (5,500
renown in all). They made a run stronger for having played before (the careful player cleared four runs in ten with
none, eight in ten with all), so a run's outcome said as much about the profile as about the play. Each went one of
three ways:

| Talent | Now |
|---|---|
| Bram's Kin (start with 10g more) | everyone's: every run starts with 50g (`GameState.StartingGold`) |
| Familiar Face (four curios on Nim's counter) | everyone's (`Curios.Laid`): more of a choice, not more power |
| Second Look (once a visit, Nim shows something else) | everyone's (`GameState.RerollPeddler`): a build is easier to find, not stronger |
| Mara's Favour (once a run, Mara pays a night nothing you own can cover) | a curio: the **Guild Promissory Note** (rare, Coffers, from a run that's reached Crownhold): the Guild honours its own paper, once |
| Rainy Day (a fourth stash dip), Pawnbroker's Friend (pawning at 70%), Good Word (Bram vouches twice) | already curios: the Rainy-Day Purse, the Pawn Ticket (80%), the Guildsman's Marker |
| Curio Cabinet (a fourth curio slot), Royal Favour (a second synergy) | already upgrades: the Curio Case, the Crownhold Charter |
| Keepsake (a free curio on Nim's first visit), Early Name (synergies from Thornwick), Trade Secrets (the tools at half price) | gone: each sped a run up without widening it |

An old profile's renown and talents are left behind when it loads (`Profile.FromJson`).

0.37's drawer (*A season after the first*) is the nearest thing to one since: a free plain curio on a later season's
first morning. It differs from the Keepsake talent it resembles in what it's for: three to choose from, each a
different way of trading, so it widens how a season starts as much as it speeds it; nothing is spent or ground for;
and it's everyone's from their second season.

## The shop's look

Each move changes the shop around you: Bram's trestle table under a striped awning at the Brindle crossroads, with the
mill, the river and the Barrow's stones behind it at dusk; a stone-walled shop at Thornwick Market with timber beams,
oil lamps and a window onto the Old Wood; a panelled shop in Crownhold whose display window looks over the roofs to
the palace at sunset; the Royal Emporium's colonnade, crimson drapes and marble counter, with the Gilded Gate glowing
down the avenue. Each is one picture (`art/generated/backgrounds/`; round 2's are 320x180, which the importer doubles
to 640x360), shown at 2x behind the counter and dimmed behind the lockpicking, opening and day-end screens
(`UiKit.MakeShop`). What you keep stands on the shelf (the window sill in town), and a Heartsong hangs above it. The
story's scenes have pictures of their own: Bram's back room for the mornings, his homecoming, the next day and the
season's end; the Guild's counting-house for Vane (his introduction, the summons, a foreclosure, the debt paid while
Bram's away, and the mornings after the debt); and the vault under the Gate for the treasury (the carriage, the great
chest itself and the door left to the Maw). The ruins of the Keyhold (`scene_keyhold`), where Mara lowered her hood
until 0.34, are only in the art reviews now: Mara speaks at the counter.

All the pictures are made for the game to one brief (`art/`: one 74-colour palette, one light from the upper left,
one dark outline), so they follow the world rather than a pack's: every portrait stands against its place (the
crossroads' dusk, the Old Wood's green, Crownhold's blue, the Emporium's crimson, the story's night), the Maw is a
borrowed body gilded all over, demon gold is the real thing gone dull and greenish, the paste gem is a cut stone with no
fire in it, and a scroll case of sand looks like the mudlark's "maps" it is. Things are 32px and all shown at one scale,
so a purse is smaller than a sword, and a coin from Brindle and a crown from the Royal Treasury sit together on a shelf.

The art is the room the player thinks in: they sit with it for hours, reading the slip and listening, so it has to be
comfortable to look at and never pull the eye from the chest. The first set (0.23) wasn't: a model asked for pixel art
draws texture and glints at every pixel, and it came out attractive at a glance and tiring over time, with two to six
times the edges and specks of the old code-drawn art at the size it's seen. Round 2 of the brief (0.25) never asks for
pixels: the generator draws flat-colour illustrations and the pipeline makes the pixel art a flat area at a time, on a
coarse grid (320x180 for the world, four screen pixels to an art pixel), and the validator fails anything busier than
it should be. The chest, the thing the player looks at most, is drawn in code (`tools/custom_art.py`): one silhouette
in seven finishes, with its lock maker's hasp and plate on it.

Round 2 also builds the shop from layers, each with one job, so the place can feel like a place at an hour of the day
without getting busier: a sky for the time of day (the day goes from morning through midday to evening as the
customers come, and the Guild comes at night: the sky changes, the rest is tinted, the lanterns are lit); the place
across the counter; the seller, standing full length on the far side of the counter (it hides their legs; a goblin
only just shows over it, a knight towers); the display shelf and the shop's own frame (the top bar sits on its band);
the counter, with the tools you've bought standing beside the chest; and the appraisal sheet the slip is printed on,
grander at every move. Until 0.23 every picture was drawn in code, and `tools/custom_art.py` still draws a
placeholder for anything new until its picture is made.

The first round-2 pictures were calm but too plain, so a style study (`art/study/`) drew the stall's counter in four
styles at three levels of detail before anything else was made; the owner chose warm storybook cartoons with detail
(C2, at the 4x grid), drawn without outlines, and 0.26 puts all of it in the game. The art stays without outlines,
but some things have to stand out from a detailed scene: the person you're trading with, and the counter's edge the
chest stands on. So the game draws those itself, one dark pixel outside every character's silhouette (full figures and
the faces cut from them) and a line along the counter's far edge, and a setting turns them off for anyone who'd rather
have the art as drawn.

**0.31: the 3px art.** After comparing grain sizes the owner chose 3px: every picture remade from its full-size
original so that one cell of the art is 3x3 screen pixels at 2560x1440 (twice as fine as round 2's 4x grid, which
was 8 pixels a cell there), with the faces kept whole and natural (two expressions corrected, thirteen faces' details
protected) and no outlines: the shapes stand out by colour against colour, so the game's own outlines are off and
the setting is greyed out with a note. All 267 pictures were reviewed and approved on 2026-11-04 (`art/density3/`).
Nothing moves: each texture is stored at its 1440p size and tells the layout the size it had before
(`ArtTexture`, `assets/art_density.json`), so every position, scale and click area is round 2's. A cell is exactly 3
pixels at 1440p, and a little more or less at other sizes (2.25 at 1080p, 1.5 at 720p); the chest, the effects and
the UI frames keep round 2's grain.

**0.32: the chest, the opening and Kit's face.** The owner asked for a chest with more refined detail at a higher
resolution, still clean and restrained, and for openings that don't look rough. The chest is drawn again in code at the
3px art's grain (`custom_art.chest3`: 128 cells wide where round 2's was 48, so it's as fine as everything round it),
the same silhouette and finishes: three boards to the lid's dome, lit on top and shaded where it curves under, two on
the box, each with a lit lip and a shaded foot; two straps with a pair of rivets each, guards on the dome's corners,
brackets at the feet, a band at the seam and one round the base, and the ward mark cut into the middle board (a jewel in
the finer finishes, the crown on the King's). No grain, no scratches, no outline: 0.23's lesson (six times as busy as
the calm chest) still holds. The lock makers' hasps and plates were redrawn to match. Nothing that places the chest
moved: the importer reports each picture's old layout size (`assets/art_density.json`, `drawn`). The opening kept every
step and changed how it looks: the white-out flashes became light in the find's colour, scaled to its quality; the
generated burst sheet (a splat of blobs) became a flare, a thin ring, a star of fine rays and a spray of one- and
two-cell sparks; light is a soft glow rather than stacked discs; rays, rings, lightning and confetti are finer. The
counter, the Guild's night, the run's end and the Heartsong's reveal use the same effects, so they changed with it.
Part of Kit's face read as unnaturally dark: the 3px conversion had snapped her freckles and blush to the darkest skin
tone and let two wood colours into her skin and eye whites. They were corrected cell by cell at the owner's request on
2026-11-04 (245 cells, no new shapes: `art/density3/README.md`, and the amendment in `approval.json`).

## Title, saves and settings

The game opens on a title screen (Continue, New game, Collection, Settings, Quit, Credits) over your shop as you left
it. 0.32: the menu comes out of the chest. The locked chest strains for a moment (a creak, a knock from inside, a
widening crack of light), bursts (the lid off the top of the screen, a column of light, a ring, sparks, a chord rather
than coins), the name rises out of the light, and the buttons fly up out of the chest and swing down into their places
one after another, each landing with a squash and a note a step higher than the last. About two seconds the first
time, half that coming back; any key, button or click finishes it at once. Afterwards the chest stays open, glowing
softly. (Until 0.31 a closed chest sat under the menu with coin sounds, and the owner found it ordinary.) New game opens the character screen (`SetupPhase`): Kit and Bram's three partners (locked ones say
what unlocks them), the chosen one's rules, and the challenge level with its terms and multiplier. The shop is
saved to `user://save.json` at the start of each day (`GameState.Save` and `Load`, plain JSON from Core), so leaving
mid-day starts that day over; the pause menu (Esc, or Menu in the top bar) says so. Settings (`user://settings.cfg`):
volume (effects, songs and, since 0.36, music), fullscreen, screen shake and flashes (full, reduced or off), outlines (round everyone and along the counter's
edge, on whatever screen is showing, for art drawn to be outlined: see the art, above; 0.31's isn't), Uncle Bram's tips,
how fast speech is typed and whether it blips (0.37: *What's said*), and **item sounds**: for each
kind of thing, which of three or four sounds it makes (`Audio.SoundSets`; recorded ones from Kenney's CC0 packs via
`tools/import_art.py`, a few synthesized in `Sfx`). Each kind's sound stays the one you'd expect in real life (a
purse, a pot, a thump of cloth, a bottle, pages, glass), so any set keeps the rules readable; the choice is taste.
Click a kind's option to hear the next one.

**Saves (0.37).** Three of them (`Settings.Slot`, `Main.SlotPath`). The first is the files the game always kept
(`user://save.json`, `profile.json`), so a player who had a save has it as Save 1, untouched; the others live in
`user://slot2/` and `slot3/`. A save is everything that's kept: its profile (what's been taught and found, the
campaign, each keywright's levels and best scores, the legendaries' feats) and its season in progress. The title's
bottom-left button names the one being played ("Save 1") and opens the Saves panel (`SlotsView`), a card each: how
many seasons it has played, where its story stands (the debt not cleared yet, cleared, you remember the treasury, the
story's told), how far in it has traded, and what's in progress. **Play this save** switches to it and the title
begins again on it; **Start a new save** on an empty one is the completely fresh start the owner asked for, a first
season with everything to learn; **Erase, and start fresh** empties a used one, after a question that says what
goes. Erasing destroys nothing: `Storage.Erase` renames the files where they are (`profile.erased-<time>.json`), the
way an unreadable file is set aside, so a slip of the hand can be put right from the save folder. Settings are the
machine's, not a save's. `--save-test` checks the paths, the switch and the erasing.

**GM mode** (v21, for testing). Since 0.37 it's behind **F1** during a season: until then it was a row in Settings,
where a player could switch it on out of curiosity. F1 opens the pause menu with the GM panel beside it, and it lasts
until the game is closed (it isn't saved; `--gm` still starts with it on). The panel: +99,999 gold; skip the rest of
the day (the customers still waiting go home, and the day ends as usual, so the Guild's night still comes); unlock
the whole journal (every regular, every find and its enchanted star, every curio Nim could carry, in the run and the
profile); and, since 0.37, **Fulfil the quests** (`GameState.GmFulfil`): everything the story asks of a season, at a
stroke. This week's commissions are filled and paid; the treasury key's four pieces come with their fees; Bram's
account is paid off (the gold for it goes into the purse first, and it's paid whether or not Mister Vane has called
yet); and the true ending's checks are met: you remember the treasury, you've heard the gilded and seen the
weighhouse register, you know the seal's clause and the charter, a full mimic waits, the Wet Hen is hired and Bram's
been brought home. So the Guild's carriage comes that night for the final opening, and the next day after it. The
panel says what it did; on a save whose story is already told it fills the commissions and says so. Nothing in it is
fair, and the panel says so.

## Music and ambience (0.36)

Until 0.36 the game had neither: hours behind one counter with nothing under the chests' sounds, which the review
counted among the reasons a season felt long. There's no composer and nothing to download here (only GitHub is
reachable from a cloud session), so the music is made the way the Heartsongs' songs are, in code, but offline and at
length: `tools/music/synth.py` is a small additive synth (a plucked lute, a whistle, strings, bells, a bass, a frame
drum; a loop that wraps whatever rings past its end and a reverb that goes round with it, so every loop is seamless),
and `tools/music/make.py` has the tunes written out a bar at a time and renders them to
`assets/audio/music/*.ogg` and `assets/audio/ambience/*.ogg` (about 4 MB in all; `python3 tools/music/make.py`,
needs numpy and ffmpeg).

| Theme | Where | What it is |
|---|---|---|
| `crossroads` | the title, the character screen, Bram's stall | a lilting air in D, six-eight, a whistle over a lute |
| `thornwick` | Thornwick Market | a jig in E dorian, whistle, lute and a frame drum |
| `crownhold` | Crownhold | stately, in D minor: a plucked keyboard's broken chords, strings, a slow flute |
| `emporium` | the Royal Emporium | a slow waltz in A minor on bells and a harp, over a low note that never quite stops |
| `night` | the day's end, Nim's table, a night's talk, a season's end | a music box over held strings, in G |
| `guild` | the Guild's night, scenes in its counting-house | low strings marking time, a bell every other bar |
| `treasury` | the treasury, the Keyhold | no tune: a low note that swells and sinks as if it breathed |

Each market further in has its own theme, so the road is heard as well as seen: paying a due changes the music. Under
the theme is a bed of ambience (`day`: birds and wind at the crossroads; `market`: a crowd's murmur; `hall`: the
Emporium's hush, voices kept low; `night`: crickets and an owl), a few decibels below it. `Music` (scripts/Audio) picks both
from the screen and the shop when `Main.SwitchTo` changes phase (`Music.For`), cross-fades, and carries a theme on
where it was when the next screen wants the same one. Both play on the Music bus, with its own volume in Settings >
Sound (60% to start with: it's a bed, not a performance); a Heartsong's song has the room to itself (`Music.Duck`:
the music steps back while one sings). The chests' sounds are untouched, and louder than the music by design: the
game is still played by ear.

What can't be checked from here is how it sounds to a person: the tracks were checked by their numbers (peaks at
half scale, loudness around -21 dB RMS, energy centred between 500 and 900 Hz so nothing sits on the chest sounds'
band, seams far smaller than an ordinary step between samples) and their spectrograms. See *Open questions*.

## The trade's words (0.32)

Players met curios that pay "on a chest bought sight unseen" or lean on "Bram's word" without ever being told exactly
what those are. `Glossary.cs` defines each of the trade's words once, with the rule's own numbers: sight unseen (no
shake, no drop, no lodestone; the witchlight and what a seller tells you don't count), Bram's word (once a run, on a
Guild night you can't pay: the Guild takes all your gold and the rest, half as much again, goes on next week's due;
not in the last week), Bram's stash, a short night, a perfect read, a step better, a surprise, a dry spell, exhibition
points, a warded thing, a false bottom, haggling and mended. A curio lists the words it uses; Nim's card shows a "?"
whose tooltip has their definitions, and so do the curio tooltips at the counter and the day's end and the
collection's curio page. A curio's own text says exactly what it does (no "10% luckier": "10% likelier to come out a
step better", and what ends a streak).

## Heartsongs for a first season (0.32)

A first season may never hear a Heartsong (about two in three don't), and then the rarest, most powerful things in the
game went unexplained. Bram mentions them on a quiet evening of the first week (what to listen for, and to buy the
chest if you ever hear one), the collection's Heartsongs page says how to hear one, and a season that ends with none
ever heard gets a short scene with Bram between runs (once a profile): how rare they are, that only a Keywright hears
one and the seller never knows, what one does on display, and that every one heard stays in the songbook for good.
The Silent Choir (`docs/stories.md`) is the one tale whose prize is a Heartsong, for a season's hardest reading.

## Collection, synergies and exhibition

Every thing you open goes in the collection the first time you find it (33 entries: five qualities of six
kinds and three fakes; finding one enchanted stars its entry) and in the journal's tally of what you've acquired,
kind by quality.

**Two books, kept across runs** (v21; the owner: a regular's page and a collection entry should open the moment you
discover them, and stay open). They're Bram's kin's, not the run's: a regular's page opens the first time they
come to your counter, a collection entry the first time you find the thing, a curio the first time Nim lays it out,
and each stays open in every run after (`Profile.Met`, `Found`, `FoundEnchanted` and `CuriosSeen`, merged into a run by
`GameState.Remember`; `Profile.Note` keeps them when a run ends). The run's own finds still score and still pay a
collector's catalogue. In 0.25 the ledger, which had grown seven pages, became two books: the **notebook**, for
trading (Sounds, Regulars, Calendar with the debt, Synergies; N, or the button on the slip), and the **collection**, for
what's been gathered (Finds, Curios, Heartsongs; from the title screen and the pause menu, so it's there between runs
too). The collection's **Curios** page shows all 79: the ones you've met with what they do, the ones you
keep, and the ones Nim doesn't carry for you yet, dark, with what opens them (a legendary's feat with how far along you
are). Bram's Ledger, the talent that showed every regular from the start, had nothing left to do and went in v21 (the
rest of the talents followed in v22). Each thing looks the same everywhere: the collection, the shelf, the exhibition and the keep chooser
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
| Bram's Stall, the Brindle crossroads | start | 4 | mostly wood | jingle, clank, flump; three strengths; clanks drown jingles; from day 2, haggling, naming a price, counting rattles, stories; Tobin's rotten Barrow chests |
| Thornwick Market | 50g, once the stall has earned 50g | 6 | green and blue | slosh and rustle; locks; false bottoms and the drop; the display shelf and cabinets; Nettle's fee, Rook's scale and mare, warded things with Sorrel's dreams and Hesper's dowries, Cadoc's crypt, Corvin's straw |
| Crownhold | 160g | 6 | mostly blue, some purple | gems; two or three of a kind and the pawn ticket (Brass); sand (Tilly); enchantment and the Witchlight; synergies and the Crownhold Charter; Ilse's packing tags, Orme's wager, Durgan, Isaura's riddles, Quire's ward-rolls, Sir Rowan's heirloom |
| The Royal Emporium | 1,200g | 5 | blue and purple | Tick's hourglass, Sallow's due, Skrae's echoes, Madame Zell's cards, Gilt's demon gold, the Countess's paste, the King's game, the Maw's gaze, the Lodestone, the Royal Seal |

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
who hear everything (Skrae, Gilt) paid at the Emporium.

**The Bookkeeping Engine** (`Upgrades.Engine`). Once your purse could pay off everything left of the debt
(`GameState.CanPayOff`), the Guild offers its counting clockwork for exactly that sum (`GameState.Cost`), first on the
day-end cards. It marks every chest's real contents on the slip as it comes to the counter (`Appraisal.Recorded`:
every real thing by kind, enchantment, a false bottom; not how good: that's still the opening), so there's nothing to
shake: a run that's already won can be finished in an afternoon, still owing the Guild everything. It can't read
through the Stranger's chains. The sim's careful player pays off instead; `--engine` starts the bot with it.
Day one is scripted: three customers, one lesson each (a gentle shake and the slip; a hard shake for a coat;
listening past a clank); the second morning opens with Bettany, who heard everything there was (passing).

## Evidence (`godot --headless --path . -- --sim`, 300 runs per style, up to 11 weeks)

Each style plays whole runs as Kit at level 0: buying chests, curios (a careful player's taste,
`Strategy.CurioScore`, keeping enough back for the Guild) and upgrades (keeping Bram's working purse), delivering
commissions, playing the tales (`Strategy.Choose`), paying the Guild (calling in favours, then pawning, or Bram's word,
when short), taking Bram's stash when a day starts nearly broke, and paying off the whole debt as soon as it can
(always paying off rather than buying the Bookkeeping Engine). These are the perfect listeners; how real players get
there is in *Balance across a career*, below. 0.30's numbers, with the tales and what they leave behind (seven
five-day weeks; 0.28's, with five, are in brackets where they're quoted; earlier versions' are in the git history):

| Style | Still trading after each due, weeks 1-7 | Debt cleared | Crown Chest | Foreclosed (median week) | Thornwick | Crownhold | Royal Emporium |
|---|---|---|---|---|---|---|---|
| Pays their price, every chest | 100 100 93 53 8 1 0% | 0% | 0% | all (week 5) | day 11 | never | never |
| One hard shake, ticks what it hears | 100 100 100 100 88 29 1% | 1% | 1% | all (week 6) | day 8 | never | never |
| Every strength, drops when safe | 100 100 100 100 97 86 58% | 58% | 58% | 81% (week 7) | day 8 | day 16 | day 26 |
| + Witchlight and Lodestone | 100 100 100 100 97 86 59% | 59% | 59% | 72% (week 7) | day 8 | day 16 | day 26 |
| + keeps grand things | 100 100 100 100 97 87 59% | 59% | 59% | 68% (week 7) | day 8 | day 16 | day 25 |

("Foreclosed" counts the Guild's endless interest after the debt, half as much again each week, over the four weeks
the sim plays on. The "Crown Chest" column is 0.30's: 0.34's sim reports the treasury instead, as "door", the share of
runs that went into the Guild's treasury on the last night.)

Week by week for the careful collector (gold on the Guild's night, before paying):

| Week (day) | 1 (5) | 2 (10) | 3 (15) | 4 (20) | 5 (25) | 6 (30) | 7 (35) |
|---|---|---|---|---|---|---|---|
| Due | 20 | 60 | 160 | 450 | 1,300 | 4,000 | 12,000 |
| Gold, 10th percentile | 70 | 178 | 293 | 499 | 585 | 1,333 | 2,951 |
| Gold, median | 117 | 330 | 591 | 1,833 | 3,566 | 5,664 | 11,282 |
| Paid on the night | 100% | 100% | 100% | 100% | 97% | 90% | 65% |
| At the Royal Emporium | 0% | 0% | 1% | 11% | 47% | 73% | 84% |

(What the Guild asks on the night is a little less on average from week 6: a friend of the Guild pays a tenth less,
the King's decree takes a fifth off and a trial half, so about 3,800g and 10,100g.) The first four weeks are for
growing and moving inward, and the careful collector pays every one of them; the fifth asks for a good week in
Crownhold and the sixth for a start at the Emporium (one careful run in five needs Bram's word once, mostly in those
two weeks), and the seventh is the wall. The careful collector pays the debt off early in 49% of runs (cleared in week
5: 11%, week 6: 20%, week 7: 28%; 0.28: 44%, from week 3), fills about 21 commissions a run (7,600g: each piece pays
at least a quarter over what it would sell for), opens Bram's stash once in two or three runs, calls in two favours
and makes three or four friends a run, and finds about two gold things (none in a third of runs; the careful players
who don't keep grand things, about one, and none in more than four runs in ten). Profit per customer:
about 7g at the stall, 12g at Thornwick, 70g in Crownhold and 490g at the Royal Emporium (0.28: 10, 31, 180 and
3,800). Surprises: one thing in four or five comes out a step better than its chest (two steps or more, one in
seventy-five), worth a sixth of what the chests held (0.28: three in ten, worth half, when a step was worth three times).

What curios and synergies are worth, to the careful player with both tools who sells everything:

| | Debt cleared | Median day cleared | Median gold on the night, week 2 / 4 / 5 |
|---|---|---|---|
| With both | 59% | 31 | 330 / 2,004 / 3,625 |
| No curios | 51% | 31 | 440 / 2,208 / 3,310 |
| No synergies | 18% | 35 | 330 / 1,531 / 2,223 |
| Neither | 1% | 35 | 437 / 1,804 / 2,202 |

Ordinary progression alone (every upgrade, the tales, no curio) clears about half of runs (0.28: three in
ten; the tales' consequences, the longer season and dues that climb about three times a week make the difference);
curios add eight points (they pay over the run: what they cost shows in the gold until week 4), and a build that comes
together (three or more curios of one way of trading) clears more often still (*Curios: builds*). Without synergies,
few runs clear: the shop's name for a kind (priced in) and the Keen Eye's fee for a perfect read (not) are most of a
careful shop's late growth. The margins (*The shop's power index*): the careful player's held 1.72-1.83 per gold
spent at every index, the blind buyer's 1.12-1.19, the careful collector's climbing from 1.84 to 2.6 as its build
comes together.

Per seller, at their own place (with the shop's tools, buyers' trades and restorers), profit per visit for a careful
player, against paying their price blind. Every habit and every rule pays for putting up with it; only Pip (a few
coins for anything) pays blind, Bettany by two coins and Garrick by a coin or none at the stall (as in v19), and the Maw's coin
pays anyone (its price is the rest of the day, which the table doesn't count). Cadoc's markup went up a tenth (1.8-2.1)
when Thornwick's chests got finer, and in v21 Quire's (1.85-2.15) and Gilt's demon gold (two coins in three of the gold
and gems he sells): without the named rule, a restorer's luck reaches what a blind buyer bought, and both had tipped
into paying a blind buyer a few coins a visit. 0.30's sums moved two more: Tick prices by colour, and the Maw's hoard
comes out better than its colour often enough that at 0.29's markup (1.35-1.65, on mostly purple chests) a careful
player lost by him, and at 1.0-1.2 a blind buyer won (10 a visit), so he asks 1.05-1.25 of chests three in five blue;
Quartermaster Brandt's tariff is 1.0-1.15 (at 0.95-1.1 a blind buyer made a coin a visit). Flick (0.28: the Magpie's
runner, every chest of his stolen) pays a careful player in finder's shares, sending for the Wardens, and a blind
buyer pays for chests that won't open. The officials (0.30) come while their masters count you a friend: Bailiff
Marrow sells what the Guild seized (a third of his chests cold), Brandt what the Wardens took off thieves.

| | Pip | Ada | Garrick | Bettany | Tobin | Nettle | Rook | Sorrel | Cadoc | Corvin |
|---|---|---|---|---|---|---|---|---|---|---|
| Careful | +8 | +4 | +8 | +4 | +2 | +5 | +9 | +10 | +9 | +14 |
| Blind | +7 | 0 | 0 | +2 | 0 | -2 | 0 | -10 | -5 | -27 |

| | Hesper | Flick | Marrow | Brandt | Ilse | Orme | Durgan | Isaura | Brass | Quire |
|---|---|---|---|---|---|---|---|---|---|---|
| Careful | +6 | +10 | +13 | +4 | +23 | +61 | +89 | +41 | +84 | +36 |
| Blind | -16 | -21 | -2 | -2 | -6 | -11 | -234 | -10 | -20 | -19 |

| | Tilly | Rowan | Tick | Sallow | Skrae | the Countess | Zell | Gilt | Aldo | the Maw |
|---|---|---|---|---|---|---|---|---|---|---|
| Careful | +17 | +22 | +31 | +22 | +48 | +20 | +33 | +48 | +68 | +448 |
| Blind | -12 | -11 | -13 | -33 | -34 | -29 | -117 | -18 | -153 | +431 |

The careful player plays the King's game only on a read it could check every way (every strength and the drop): a
wrong read costs a quarter more than the chest held, and Hesk, who can't shake hard or drop, never plays it. It buys
the Maw's coin when the chest's worth beats what the rest of the day would cost (what's still waiting, doubled, at the
share it would buy anyway).

Every seller's lesson completes, played the careful way, with and without the tools of their place.

## Balance across a career

v13 gave runs a meta-progression; v14 sets the balance for a career, not a run, and v15 keeps that curve through the
new world (the numbers below are 0.30's unless marked: runs of 35 days, about two and a half hours each). The goal, in the owner's words: no
pressure before players have learned the puzzles, no easy win in the first few runs, a purse that keeps growing but not
steeply enough for the middle and late dues, and judgment pushing the curve up until a run clears; then the challenge
levels. Since v22 only judgment, and what's known (the journal, Nim's stock, the legendaries' feats), carries from run
to run: until then talents did half the pushing.

**0.36: the story's difficulty, then the levels.** The goal above ("no easy win in the first few runs") was set
when a run was an hour and the story's turn didn't exist. By 0.35 a season took two and a half hours, the first clear
came around the sixth, and the treasury, the Inquiry and everything the campaign is came after it: fourteen hours of
losing before the game showed what it was. 0.36 keeps "no pressure before the puzzles are learned" and "judgment
pushes the curve up", and moves the wall: level 0 is the difficulty a new player meets the story at, and the eight
challenge levels are the game for someone who has (*The run*, "0.36: four weeks, and each week a market").

| Skill (Kit, level 0, 120 seasons each) | 0.35 cleared | 0.36 cleared | 0.36: still trading after the third due |
|---|---|---|---|
| 0.15 (a first season) | none | 20% | 98% |
| 0.3 | | 26% | 98% |
| 0.5 | 13% | 43% | 100% |
| 0.75 | 55% | 66% | 100% |
| 0.9 | | 81% | 100% |
| 1 (the careful collector) | 70% | 90% | 100% |

Nobody is lost before the Emporium to speak of: a season that fails, fails at the last due, on the twentieth night,
having seen every market. That's the other half of the change: a lost season in 0.35 ended in week 6 at Thornwick or
Crownhold, two shops short of anything new.

Careers (`--sim=careers --n=60`, 30 seasons each at the highest level open):

| First... | a quarter of players | half | three quarters | by season 30 |
|---|---|---|---|---|
| clear the debt (level 0) | season 2 | 3 | 5 | all |
| swallowed at the treasury | 2 | 4 | 5 | all |
| clear level 1 | 4 | 7 | 10 | all |
| clear level 2 | 9 | 11 | 16 | all |
| Bram brought home | 6 | 10 | 13 | 98% |
| the next day (the true ending) | 7 | 10 | 13 | 98% |
| clear level 4 | 21 | 26 | | 65% |
| clear level 6 | | | | 8% |
| clear level 8 | | | | none |
| all nine Heartsongs known | | | | 17% |

(0.35: the debt first cleared around season 5 or 6, level 4 around 23, level 6 by 13% and level 8 by none; Bram
first home and the next day around season 13, by 83% of careers by season 30.) The levels as drawn clear 77, 57, 43,
35, 18, 16, 9 and 6% for the careful collector, so the top is the wall it was and the climb to it starts from a
kinder place. A season is an hour and a half, so the true ending is about fifteen hours in where it was over thirty.

**0.37: the same first clear, a shorter story, the same wall.** Free listening gave most to whoever was half-way to
knowing the trade, and a later season now opens with more (*A season after the first*), so the middle of a career
moves faster than 0.36's. The dues rise 30% a level to hold the top (*The run*, "0.37"). The same report on the
finished build (`--sim=careers --n=60`):

| First... | a quarter of players | half | three quarters | by season 30 | 0.36's half |
|---|---|---|---|---|---|
| clear the debt (level 0) | season 2 | 2 | 3 | all | 3 |
| swallowed at the treasury | 2 | 2 | 3 | all | 4 |
| clear level 1 | 4 | 5 | 6 | all | 7 |
| Bram brought home | 4 | 5 | 6 | 98% | 10 |
| the next day (the true ending) | 4 | 5 | 6 | 98% | 10 |
| clear level 2 | 6 | 7 | 10 | all | 11 |
| clear level 4 | 11 | 15 | 18 | 97% | 26 |
| clear level 6 | 21 | 28 | | 57% | (8% by season 30) |
| clear level 8 | | | | 2% | (none) |
| all nine Heartsongs known | | | | 18% | (17%) |

A first season is cleared by about one player in six (17 or 18% by every measure), a second by half. The treasury's
turn comes on the second or third season, as it was meant to, and the true ending around the fifth: seven or eight
hours in, where 0.36 had it at fifteen. That's the part of the change nobody asked for by name, and it's the owner's
to judge (*Open questions*): the remembered seasons were where a career used to stall on a debt it couldn't clear,
and that stall is what went. A career kept at level 0 reaches the next day on the same fifth season
(`--sim=careers --level=0`: Bram home by the third, fifth and sixth season for a quarter, half and three quarters of
players), so the story now takes as long as its own steps do. Level 4 is half-way up a thirty-season career where it was near
its end, level 6 is reached by a little over half of careers, and level 8 is the wall it was: one career in fifty.
For the careful collector the levels as drawn clear 89, 80, 63, 36, 25, 13, 9 and 5% in a later season (which every
level is played in), against 0.36's 77, 57, 43, 35, 18, 16, 9 and 6%: kinder to level 3, the same from 4 up.

**Learners.** The sim's careful player is a perfect listener. A learner (`Style.Skill`, 0 to 1) plays the same way
with a learner's mistakes, each shrinking as skill grows: at skill 0 they skip each strength 40% of the time, count a
pair as one half the time, tick the kind a place usually holds without hearing it a third of the time, forget the
drop half the time (and drop
glass a quarter of the time), forget the lodestone, miss a glow or a boom on the slip, judge a chest's worth with a 45%
spread either way, pick locks worse, and buy curios on a whim. Skill 0 is a first-timer who has had Bram's lessons:
they know the rules but don't yet apply them every time.

| Skill (Kit, level 0) | Cleared | Still trading after week 5 |
|---|---|---|
| 0.15 (a first run) | 1% | 73% |
| 0.3 | 4% | 90% |
| 0.5 | 11% | 90% |
| 0.75 | 32% | 97% |
| 0.9 | 53% | 97% |
| 1 (the careful collector) | 56% | 96% |

(150 runs a skill; the main table's 300 put the careful collector at 59%. A first run gets through the first four
weeks, leans on Bram's word in the fifth, and falls in the sixth (one in six still trades after it); it clears about
one time in a hundred. The careful player clears 52% with no Heartsongs at all and 32% with no tales (the quick sim's
120 runs), 51% buying no curios (the main table's 300). 0.28's were 5%, 10%, 17%, 29%, 42% and 53%, in a five-week
season; v22's 4%, 7%, 11%, 28%, 37% and 48%, before the tales; v21's 5%, 13%, 21% and 40% with no talents, 11% to 79%
with all; v20's 5% to 48% with none, 27% to 85% with all.)

**Careers.** Each simulated player starts at skill 0 to 0.3 and improves toward their own best (0.75 to 1) at their
own pace (a tenth to three tenths of the way each run); every run is Kit's at the highest level open, scored at the
end of the debt's last week or when the shop is lost. The pace of learning is a guess; what the sim shows is the shape
(`--sim`, 40 players over 30 runs):

| Run | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 12 | 16 | 20 | 24 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Skill | 0.15 | 0.29 | 0.41 | 0.50 | 0.57 | 0.63 | 0.68 | 0.71 | 0.74 | 0.77 | 0.80 | 0.84 | 0.86 | 0.87 |
| Cleared | 3% | 0% | 8% | 10% | 20% | 15% | 25% | 33% | 15% | 33% | 28% | 23% | 23% | 15% |
| Lost in week (median) | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 |
| Challenge level | 0 | 0 | 0 | 0.1 | 0.2 | 0.3 | 0.4 | 0.6 | 0.9 | 1.0 | 1.4 | 2.5 | 3.3 | 4.0 |
| Score | 161 | 162 | 192 | 187 | 219 | 227 | 250 | 283 | 278 | 341 | 328 | 368 | 428 | 443 |

0.33 (the dues 20% higher a level; 120 careers, `--sim=careers --n=120`): the debt is first cleared around the sixth run
(a quarter of players by run 4, half by 6, three quarters by 9); level 1 around run 11, 2 around 15, 4 around 26 (seven
players in ten by run 30); level 6 is cleared by one player in five by run 30 and level 8 by 3%, as in 0.32 (measured
the same way: level 4 around 25, level 6 23%, level 8 4%). All nine Heartsongs are known by about one player in four by
run 30, and the Crown Chest sung whole by one in six. (0.34 has no Crown Chest to sing: its careers report says
instead in which run a career is first swallowed at the treasury, first brings Bram home and first reaches the next
day.)

0.30 (seven weeks, the dues 15% higher a level): a run that's lost is lost in the sixth week, a shop or two behind
(the fifth sends most first runs to Bram's word, the sixth is the wall for them, the seventh for everyone else); the
debt is first cleared around the sixth run (a quarter of players by run 5, half by 6, three quarters by 9). Then the
levels, each a little harder than the last: level 1 around run 12, 2 around 16, 4 around 27 (two players in three by
run 30); level 6 is cleared by one player in five by run 30 and level 8 by nobody. With a Heartsong in about one run
in three, all nine are known by one player in eight by run 30, and the Crown Chest sung whole by one in ten.

0.28 (the dues 15% higher a level): a run that's lost is lost in the last week (the fourth sends half of the first runs
to Bram's word, the fifth is the wall); the debt is first cleared around the fifth run (a quarter of players by run 4,
half by 5, three quarters by 7: the tales carry a learner further than 0.27 did). Then the levels, each a little
harder than the last: level 1 around run 8, 2 around 13, 4 around 23 (three players in four by run 30); level 6 is
cleared by one player in five by run 30 and level 8 by almost nobody (3%). With a Heartsong in about one run in three,
all nine are known by one player in eight by run 30, and the Crown Chest sung whole by one in ten: the songbook is a
long collection. (v24, the dues a tenth higher a level and no tales: first clear around run 7, level 1 around 12, 2
around 16, 4 around 25, level 6 by one player in three by run 30 and level 8 by one in twelve; at a tenth a level
0.28's tales would have opened level 6 to two players in five. v22's fixed terms: level 1 around run 11, 2 around 13,
4 around 23, level 6 by one player in seven by run 30 and level 8 a wall, 3%. v21, talents bought cheapest first:
first clear around run 8, level 4 around 20, level 6 by half of players by run 30, all nine Heartsongs around run 17.
v20: first clear around run 7, level 4 around 16, level 6 around 26.)

Getting there: v13's dues (40, 100, 300, 800, 1,400, 2,400, 3,800, 7,000) let the careful player with no talents
clear 61% of the time and half of all careers clear by their fifth run, with a third of first-timers already short in
week 3 (Bram's word then, his interest sinking them in week 4). Raising the late dues alone (to 10,000) made it harder
without making it gentler; making week 7 heavy did nothing (anyone short in week 7 leans on Bram's word, and its
interest lands on week 8). Lighter early dues with a heavier last week did both.

**The challenge levels** (v24: the dues a tenth higher a level, 15% since 0.28's tales, and as many boons as banes drawn for each run; see
*Across runs*). Until v23 each level added one fixed term, ordered by what each did alone (v15, with every talent:
Bram stretched thin 81%, demon gold about 73%, fussy sellers 72%, thin crowds 69%, a lean start 67%, the Guild's
interest 67%, sharp sellers 61%, a hurried town 59%); with the talents gone (v22) the top levels were a wall and the
first two were free. Now each level costs a few points, all the way up.

**Bram's kin** on the new curve (the careful collector, 150 runs each; cleared %; 0.30, the season seven weeks and the
dues 15% higher a level, with 0.28's, five weeks, after the slash):

| | Level 0 | Level 1 | Level 2 | Level 4 | Level 6 | Level 8 |
|---|---|---|---|---|---|---|
| Kit | 56 / 51% | 51 / 48% | 42 / 40% | 26 / 26% | 17 / 17% | 12 / 17% |
| Hesk | 58 / 50% | 49 / 46% | 41 / 31% | 29 / 27% | 17 / 16% | 7 / 15% |
| Rosa | 59 / 50% | 54 / 45% | 42 / 38% | 28 / 34% | 16 / 25% | 7 / 13% |
| Isolde | 51 / 54% | 45 / 49% | 35 / 38% | 24 / 30% | 17 / 22% | 10 / 19% |

The median clear day is 29 to 32 at every level (0.28: 20 to 22, in a 25-day season), and the four of Bram's kin are
within a few points of each other (Isolde a few behind below level 3, on 0.30's door money, below;
v24's Rosa, whose haggling and the Golden Tongue suit each other, was a little ahead; the tales favour nobody). At a
tenth a level the tales made the top levels gentler (Kit: level 4 35%, level 6 27%, level 8 23%, on 150 runs), so 0.28
puts the dues up 15% a level. (v21, no talents and then
every talent at levels 0, 2, 4, 6 and 8: Kit 40 / 79 / 77 / 55 / 25 / 7%; v20: Kit 48 / 85 / 76 / 66 / 24 / 16%;
v19: Kit 29 / 61 / 66 / 42 / 15 / 5%; v18: Kit
42 / 75 / 68 / 53 / 23 / 9%. v19's warded puzzles first held Hesk back, 19% with no talents and 35% with
all: the careful player only called a named kind silent once every strength had been tried, and he never shakes hard.
It now asks only that the named kind had a fair chance to sound, as a player would: a dream of a river and no slosh
at a gentle shake is enough.)

v18's bigger economy shrugged off the old terms (level 8 cleared 31% with every talent), so they're sharper: the
Guild's interest a quarter (was a tenth), sharp sellers a tenth (a twentieth), thin crowds at every shop (the stall
only), demon gold one coin in ten (twenty). Isolde's door money grew with the shop at a quarter of its pace
(`Characters.IsoldeScale`: flat, she cleared 26% with no talents; at the shop's pace, 62%) until 0.30, whose smaller
sums made that a coin a point at every shop, and a coin a point worth so much more against the gentler dues that she
cleared 81% of runs to Kit's 63%. With 0.30's rarer Heartsongs (Kit 56%), a coin for every two points cleared
71%, for every three 49%, and two for every five 51%, a few points behind the others at every level: the rate turns
how much a careful collector keeps on show, and so the customers and finer chests an exhibition brings. It's two
coins for every five points (`Characters.DoorCoins`, `DoorPoints`). Rosa's hagglers no longer open higher for her (a seller who stays away a week is price enough when an Emporium regular is worth
thousands).

v14 set Rosa's wariness at a twentieth; v15's sellers who'd name a cheap price (Tobin, Rook, Sallow) made
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

0.30's longer season made a first run 146 minutes over 30 days; 0.32's is 143 (177 customers), the same shape. The
move to Thornwick (day 8) and the few days after it are still the busiest stretch: three first-timers in four have one
day just over capacity there (0.31: a little over half; the heaviest day's load 6.3, 0.31's 6.2), and what arrives on
those days is the same (warded things, lockpicking, the drop, Mother Sorrel). A second and third run have no day over
capacity.

**0.36: the report that started a redesign.** Run on 0.35, the report's last line was "A first run takes about 2.4
hours: too long to want another", and its table said why: all thirty first-timers lost, in week 6, and the engagement
score fell from 28 on the first day to about 10 from the twentieth, with a learner still at Thornwick on day 30.
0.36's season of four weeks, a market each, was built against it (*The run*, *The ear*), and this is the same report
on both (medians over the 30 learners):

| | 0.35 | 0.36 |
|---|---|---|
| Run 1: length | 143 minutes, 30 days (of 35), 169 customers | 104 minutes, 20 days, 108 customers |
| Run 1: how it ends | lost by all 30, in week 6 | cleared by one in five (120 seasons: 20%); lost in week 4, at the last due |
| Run 1: new things an hour; high points an hour | 30.7; 5.3 | 49.8; 8.5 |
| Run 1: time in stale stretches | 6% (longest 9 minutes) | none |
| Run 1: the heaviest day (median load) | day 9, 5.7 | day 11, 5.6; day 6, 5.4 |
| Run 1: players with some day over capacity | 40% | 70% |
| Run 2: length; high points an hour; stale | 140 minutes; 5.1; 5% | 94 minutes; 9.9; none |
| Run 3: length; high points an hour; stale | 132 minutes; 5.7; 7% | 89 minutes; 9.9; none |

A high point comes every seven minutes in a first season and every six after, where it came every eleven; nothing
is stale, because a market lasts five days; and three seasons take four and three quarter hours where they took
seven, with a first clear likely inside them.

What the faster season costs is room: Thornwick's locks, drop, straw, warded things and eight new people have five
days, not eight or more, and with the sim's evening in the game's order (*The run*) the report showed what a
first-timer's first day there really asked: 7.1 of a load of 6, over for three in four. Three things spread it, all
for a learner only (tips on): a market's first day has no locked chests, its first two days bring nobody whose
lesson is a warded thing's puzzle, and the first day is still a customer short. Every day's median load is under 6
now (5.4 on the first day at Thornwick, 4.7 on the next two, 5.6 on the first at Crownhold); seven first-timers in
ten still have one day over it somewhere, most often the day Corvin's straw and false bottom come together, against
four in ten in 0.35 and five to seven in ten from 0.31 to 0.34. A third season brings a fifth of what a first does
that's new (9.1 new things an hour against 49.8), which is what the challenge levels' terms, the other keywrights and
the campaign's remembered seasons are for.

**0.37** (the same report; its second and third seasons are later seasons now, as they are in the game: until 0.37
the report carried over only what Bram had taught, which was all a profile changed):

| | 0.36 | 0.37 |
|---|---|---|
| Run 1: length | 104 minutes, 20 days, 108 customers | 104 minutes, 20 days, 115 customers |
| Run 1: how it ends | cleared by about one in five | cleared by 17% (120 first-timers, `--n=120 --runs=1`); lost in week 4, at the last due |
| Run 1: new things an hour; high points an hour | 49.8; 8.5 | 48.4; 8.7 |
| Run 1: time in stale stretches | none | none |
| Run 1: the heaviest days (median load, 30 and 120 players) | day 11, 5.6; day 6, 5.4 | day 11, 5.5 to 6.0; day 6, 5.4 to 5.8; day 8, 5.1 |
| Run 1: players with some day over capacity | 70% | 77 to 81% |
| Run 2: length; high points an hour; cleared | 94 minutes; 9.9 | 90 minutes; 10.1; half |
| Run 3: length; high points an hour; cleared | 89 minutes; 9.9 | 86 minutes; 10.1; 57% |
| Run 2: its first day (the report's engagement measure) | 10 | 17 |

A first season is the length it was: every chest is shaken at every strength again, by hand until the cradle comes
on the ninth day, and a second and a third season are a few minutes shorter than 0.36's. What changed most is how a
later season starts. Run the way 0.36 ran it (nothing carried over but what Bram had taught), a second season's
first day had nothing new, no high point and a choice and a half: the flattest day in the report, with 10 on its
engagement measure. With Bram's drawer, Nim that night and chests that can hold a false bottom or something
enchanted, it has a build's first choice and a first surprise, and scores 17. A first-timer's heaviest days are
still the first at Thornwick and the first at Crownhold, a shade over where 0.36 left them and under the line of 6
on the medians. That's why a first season's cradle waits for Thornwick's fourth day (`GameState.CradleDay`), and
why nothing else new should land on a market's first.

The report's standing advice for later seasons ("repeat runs need fresh content: builds that play differently,
things only later runs meet") is what 0.37's routine curios are: all six open after a first season, and each
changes what the counter asks for.

**0.36's fresh-eyes playtest** (an agent given `tools/playtest/README.md` and nothing else; 177 actions: the first
three days from a fresh install, then the first day at Thornwick). Its verdict: "I would keep playing. Working out
what each seller failed to notice is a strong core, and Thornwick is where it gets properly interesting." What it
understood without being told: the three rules, each regular's blind spot, that a new strength costs a listen and a
repeat doesn't, the four dues and that each one paid moves the stall, the key's four pieces, and that the Guild reads
as the villain. What it found, and what was done:

- **"I could not tell when a shake counted."** The ear's first version (*The ear*, "When a shake counts"). Reworked,
  and `--ear-test` added.
- **Bram said "Pass" on a day the Tourney made buying right** (Bettany's 5g cleaver worth 8g against her 7g). Her
  passing lesson now asks what the day makes its chest worth and a bit more, whatever's on in town
  (`GameState.Bettany`, `Lessons.Dearer`).
- **A tale's first bubble stayed blank until "Go on".** Since a speech says itself, a talk's first line has to start
  itself too, once Bram's word has been said (`TalkPhase`).
- **A wide speech bubble cut the ear's bar** ("R EAR"). The bar keeps clear of the widest bubble.
- **"Shaken gently and firmly and hard."** A list, now ("gently, firmly and hard").
- **A lost season ended on a way back to the title.** It ends on how near it was and **Another season** (*The run*).
- Left as they are, and worth the owner's eye: the first two days are very safe (every bar "over 99%" until day
  three, and the ear never pinches at the stall: by design, the stall is for learning, but it's a quiet hour);
  nothing records what was heard once the words fade (a mark on the slip's row would help, and help a deaf player
  more: it stops short of the slip filling itself in, which v22 took away on purpose); Bram's box can grow over the
  chest's captions in a long lesson; the notebook's Weight column reads oddly beside "light things rattle"; Pip and
  Flick call everyone "mister"; "1 exhibition points".

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
    the chest opens. The old cast went with the old world: Sergeant Brask's weight is Rook's, Old Marta's forgotten
    false bottoms are Corvin's smuggling chests, Granny Wick's unheard sewing is Brother Cadoc's vestments, Madame
    Zara's trick boxes are Lady Isaura's, Baron Vex's clock is Tick's hourglass, Pip's sharp ears are Skrae's, and
    Magister Orme's dreams went to Mother Sorrel while Orme took up a wager.

11. **Lies about where a chest came from (v15 to v17).** The owner, again: "figuring out whether the seller is lying"
    is a false premise; it never changed how anyone played. v18 makes every story true, drops the honesty tally and
    the tall tales, and gives the sellers who leaned on it a clue worth working out instead: Corvin's straw (a strength
    later, and soft things never), Brass's ticket (a count, false bottom and all), Rook's scale (a weight, and a mare
    who won't be shaken hard) and Mother Sorrel's dream (the dearest thing, as a sign). The Drowned Bell, which tolled
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
    runs there was nothing left for it to do (v21 gave its renown back; v22 retired renown with the other talents).

16. **v20's working purse** (20, 40, 250 and 2,500g by shop). It didn't follow what an upgrade does to prices, so an
    auction block could leave a shop that had kept exactly that much unable to buy a chest for days. v21's is two of the
    shop's typical chests at what sellers ask now, worked out again for every purchase.

17. **Sellers' claims (v5 to v21).** Every listener said what they'd heard ("Shook it hard: heard a clank"), and the
    slip started ticked with it. It did the first half of the puzzle for you, and a pre-ticked slip read as the seller's
    word to be trusted or untrusted when nobody lies. v22: sellers still price only what they noticed, but nobody says
    it and the slip starts empty; its header says how they came to their price, and what they priced is the player's
    to work out by shaking the way they did.

18. **Talents (v13 to v21).** Twelve, bought between runs with renown. They made a run stronger for having played
    before: by v21 the careful player cleared four runs in ten with none and eight in ten with all, so the outcome said
    as much about the profile as about the play. v22 retires them (*Across runs*): three are everyone's now, one is a
    curio, five were curios or upgrades already, three went. Renown went with them; the score stays.

19. **A Heartsong's price (v16 to v21).** It sold as a trinket as fine as its chest, so keeping one meant paying its
    price to keep it ("Display it (-300g)"), and at nearly one a run they stopped feeling rare. v22: worth nothing in
    gold, kept for free or declined, a third as often, rarer still for each one kept, and the reveal waits for you.

20. **Enchanted hidden things (v8 to v21).** A thing under a false bottom or warded could be enchanted, and nothing
    could show it (no sound, so no glow): a read could be perfect in every way you could check and still be wrong. v22:
    a hidden thing is never enchanted, and fetches a third more instead.
21. **Fixed challenge terms (v13 to v23).** Eight levels, each adding one of the Guild's harsher terms on top of the
    ones below, always the same order. Tuned when talents could carry a player past them, they made levels 6 to 8 a
    wall once the talents went (the careful collector cleared level 8 one run in twenty-five), and every level-5 run
    was the same run. v24: every level raises the dues a little and deals as many boons as banes, drawn for each run,
    so a level is harder but never all bad, and two runs at it differ.

22. **The seven-week season (0.30 to 0.35), and the five-week one before it.** Thirty-five days, about two and a half
    hours, and a first-timer lost every one of them in the sixth week; the story's turn (the treasury) came after a
    career's sixth season. Each lengthening had a reason (room for the tales, a gentler curve), and each made a lost
    run cost more and a won one come later. 0.36: four weeks, a market each (*The run*).

23. **Buying the moves (until 0.35).** "Move to Thornwick" for 80g, Crownhold for 300, the Emporium for 3,000, at the
    day's end, with a working purse kept back. It made the biggest thing that happens in a season (a new place, new
    people, new sounds) something a struggling player couldn't have: the experience report's learners reached
    Thornwick on day 8 and were still there on day 30. It also made the due purely a tax. 0.36: the due paid is the
    move.

24. **The whole routine by hand, every chest (until 0.35).** Every strength, the drop and the lodestone on every
    chest, for nothing. With no cost the right play was all of them every time, which is a procedure; nine chests in
    ten were profitable for whoever followed it, so an opening only confirmed the slip. 0.36 put a price on each check
    (the ear: 27, below). 0.37 took the price off again and gave the procedure to the build and to Bram's cradle
    (*Listening is free, and a build does the routine*).

25. **A click a sentence (0.35).** Dialogue a sentence at a time was right; waiting on a click for each was three or
    four clicks a speech, all day. 0.36: a speech says itself, and a click finishes or dismisses it.

26. **Odds that leaned down (v12 to 0.35).** A thing came out a step worse than its chest's colour 16% of the time and
    better 7% (v10's one-in-five jackpots had been cut hard, and the letdowns never were), so the opening's commonest
    event was "WORSE...". 0.36: worse 9%, better 13.5% (sellers price the average, whatever it is, so the margins
    didn't move): *What a chest holds*.

27. **The ear: counted listens (0.36).** Eight careful listens a day, one for each new check on a chest, with a Tea
    Kettle, a Listening Screen, an Echo Stone and Lamb's-Wool Plugs to stretch them and a head cold and a rested ear
    among the challenge terms. The owner, after playing it: "The hearing-ability mechanic serves no meaningful
    purpose... restricting how many times players can listen is not the solution." It made the routine dearer, not
    different: it never pinched in a first week, it fell hardest on whoever was still learning, and what a player
    met was a meter between them and the chest. Kept from it: the closed eye at the opening for what
    nothing you did could have told you of, the record of which checks a chest has had (`Visit.Checks`), the test
    that a hand's shake is counted at any frame rate (`--hand-test`), and five customers a day past the stall.

28. **Every season starting from nothing (until 0.36).** Locks and false bottoms shut until Thornwick, two of a kind
    and enchantment until Crownhold, Nim until the third night, commissions until the second week, and the story's
    exposition said in full, in every season. Right for a first season and wrong for the tenth: *A season after the
    first*.

29. **Speech a sentence at once (0.35, 0.36).** Each sentence appeared whole. The owner asked for what most pixel-art
    games do: a character at a time, with a soft sound. The sentences stayed (the horn, the dots and the click all
    need them); how each arrives changed. "At once" is still a text speed in Settings.

30. **GM mode as a row in Settings (v21 to 0.36).** Anyone could switch it on. It's behind F1 now, lasts until the
    game closes, and isn't saved.

31. **One save (until 0.36).** Starting completely fresh meant finding the save folder and deleting files. Three
    saves now, and erasing one sets its files aside.

## Open questions

- Heartsongs: is a sound you've never heard, then the reveal and its choice, the heart-racing moment it should be?
  About a third of a Heartsong a run, rarer for each kept: rare enough to cheer, and is declining one ever the right
  call? Is a find that can win a run a thrill or a fluke?
  Is the songbook (nine, across runs) a goal people chase now that no ending waits on it (0.34)?
- The stories: do players read them, and use a place as a clue (a barrow's grave goods, a goblin warren's coin)? Do
  the stated clues read as puzzles (subtract the heard from Rook's weight, count against Brass's ticket, read the
  straw a strength late, decode Sorrel's dream), or as homework?
- v18's numbers: does growth feel exciting from week 4 on, and does the Bookkeeping Engine feel like a reward for a
  won run, or like skipping the game?
- The new rules: is Tobin's two seconds readable with the countdown over the chest? Does a goblin's tenth feel like a
  real choice? Does the King's game (a quarter off, or a quarter more) feel like a test or a trap for a learner? The
  Maw comes one day in ten: is "everyone after him asks twice as much" a thrill to weigh, or a coin nobody can refuse?
- Hesk never plays the King's game in the sim (no hard shake, no drop: he can't be sure of a read). Is that a fair
  cost of his back, or should the scale count as checking?

- Are curios still exciting next to the bigger upgrades? Which do players pick, and does anyone take the token or the
  crescent?
- Paying off early: does a week-4 finish feel like the reward it should, or like the story ending before the treasury
  key is whole? (Keep trading, and the treasury comes the night its last piece does.)
- Is drag speed a good stand-in for shake strength with a real mouse? The thresholds need playtesting; the words under
  the chest now show where you are.
- Tick's twelve seconds: enough to shake gently and hard and decide, but is it fun or just stressful?
- Should displayed things ever sell for more than their price (a famous piece)? Today they only draw customers.
- Is counting by ear (and by the words over the chest) readable with four or five things rattling at once?
- Rook, Bettany and Mother Sorrel pay a little even blind (a fair price for a heavy chest, a rattle's price for a purse,
  a cheap dream). Kindly sellers, or holes?
- Synergies reach level 4 within 30 days for most careful players; level 5 is a long goal. Is the pace right?
- The odds bar takes the arithmetic away but not the read: it only knows what you ticked. Does it still leave room
  for a hunch (paying a little over the odds for a chest you suspect holds more), or does everyone just follow it?
- Enchantment is now x2 instead of x4: still exciting enough to hunt with the witchlight?
- The shop saves at the start of each day only, and sellers don't remember you.
- A week at the stall before Thornwick (five days since 0.36, whoever you are): long enough to learn the three
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
  takes (the sim says about seven) and whether losing in week 5 several runs running feels like "nearly" or
  like a wall. With the talents gone (v22), every run starts the same; v24's challenge levels step down a few points a
  level (the careful collector clears level 4 about a quarter of the time and level 8 about one run in six). Does each
  level feel harder than the last, or do the boons make some draws feel easy?
- A third of a Heartsong a run makes the songbook a long collection (all nine by one player in eight by run 30, the
  sim says): is finding a new one a peak worth the wait, or should unknown songs be likelier still?
- With the steeper late dues, the exhibition matters: a careful player who sells everything clears 21%, one who keeps
  grand things 44%. Is keeping things taught early enough (Bram's "keep" tip) for that to be fair?
- Challenge terms (v24): do players read a run's boons and banes before they start, and plan around them (save for
  Thornwick when it's half price, keep the lockpicks busy, skip the drop when sellers are fussy)? Is a draw that's
  plainly kind (the first morning's gifts and Thornwick's welcome together) a treat or a flaw, and should a player be
  able to see the next draw before choosing a level, as they can now (the setup screen shows the run's own)?
- The treasury key's four pieces (a hidden thing read true, a stolen chest settled, a commission filled, an old lock
  picked without force) are marks of the trade a careful season makes anyway. Do they make the key a goal across the
  season, or does a careful player have it without trying? And does the first swallowing land as the shock it should,
  at the end of what felt like a won run?
- 0.34's campaign: does a second season show at once that knowing changes play (Garrick's chest, Old Haddon's note,
  Bram's tuning fork), do the Inquiry's leads read as things to do rather than homework, and does the plan's
  four-line list make the true ending feel earned before it's shown?
- 0.37's typed speech: nobody has heard the blips (a cloud session has no sound device; they were checked by their
  numbers: 50ms, played at -15dB under a peak of -12.7dBFS). Are they the subtle patter most pixel-art games have, or
  do they wear over an hour? Is 52 characters a second the right default? Both are the player's to change (Settings >
  Game), and the blip is one line in `Sfx`.
- Bram's cradle: does a button that shakes the chest feel like relief, or like the game playing itself? It's a second
  a strength and the listening is still yours, but that's a judgement only a hand can make. If it's too much, it
  could wait for a later season; if it's welcome, it could come sooner than Thornwick's fourth day
  (`GameState.CradleDay`).
- The routine curios: do they make a build *play* differently, as meant, or only save clicks? The quill adds nothing
  to a careful player's takings and takes a slot: will anyone keep it over a curio that pays? (If not, it wants to be
  a common, or to come without a slot.) And is the Pocket Engine, which ends the reading game for whoever finds it,
  a capstone or a spoiler?
- A later season's first week with every way already in its chests: does it read as the game remembering you, or as
  Brindle losing its plainness? The stall's people and prices are still the stall's.
- The opening's tell: once a glint means something better is coming, does a chest with no glint feel decided before
  it opens? And over a hundred chests, is the breath before the last thing felt, or waited through?
- 0.37 made the middle of a career about twice as fast: the true ending comes around the fifth season (seven or
  eight hours) where 0.36 had the tenth, and level 4 around the fifteenth where it had the twenty-sixth; the first
  clear and the top of the ladder are where they were. The story is now as long as its own steps take: a career
  kept at level 0 (`--sim=careers --level=0`) reaches the next day on the same fifth season, so it's the plan's
  four lines over two or three remembered seasons that set it, no longer a debt that couldn't be cleared. Is that the
  right length for what the Inquiry has to show, or did the old stall give it room it needs? To lengthen it, the
  plan would have to ask for more (a lead that takes a season to ripen); making the debt harder again would only
  put the stall back.
- Level 0 is the story's difficulty now (the careful player clears nine seasons in ten, a first-timer about one in
  six, and half of careers by their second). Is a first clear on a second or third season the hook it's meant to be,
  with the treasury's turn straight after it, or does it make the Guild a paper tiger until the challenge levels?
  The dues are four numbers (`Guild.Dues`) and `--sim=learners --dues=a,b,c,d` tries others in a few minutes.
- The first three dues are paid by nearly everyone and the last is the wall. Is a season whose only real test is its
  last night tense all the way, or only then? A stiffer third due would test Crownhold too, at the cost of seasons
  that never see the Emporium.
- The music was written and rendered in code and checked by its numbers; nobody has heard it. It may be pleasant,
  thin, or wearing over an hour: it has its own volume, every theme is a function in `tools/music/make.py`, and a
  composer's tracks would drop into the same seven file names.
- Six tales a season instead of eleven: does a season still feel like a town with its own troubles, and do two
  seasons still feel different? A tale that begins at Crownhold has ten days to finish; some endings will be read at
  the season's end rather than played.
- A speech that says itself: is the pace right for a slow reader (it never hides anything: the whole speech stays up
  until a click), and for a fast one (a click shows the rest at once)?
