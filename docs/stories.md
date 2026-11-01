# Keywright: the tales

What goes on between visits: the stories that run through a season, beat by beat, and what your choices do to them.
`docs/world.md` has the world they come from (who rules, who wants what, why chests are stolen and what the mark
knows); `docs/design.md` has the rules and the numbers. The code is `scripts/Core/Tales.cs` (the engine and the
standing), `TaleBook.cs` (the tales themselves) and `Offers.cs` (tools people bring to the counter).

## How a tale goes

A **tale** is a story in **beats**: one person comes to the counter (or knocks after dark) and something happens. A
beat comes once the one before it is done and enough days have passed, at a shop grand enough for its people, and if
its conditions hold (what you chose before, where you stand with someone). Some beats are just talk: the person says
what they've come to say and asks something of you, and you choose. Some bring a chest, and what you do with it (buy
it, pass, send for the Wardens) is the choice. Some come at night, between the day's end and Nim's table.

- **Which tales.** Every season has the main story (**The Key**) and **The Shut Chests** (from Thornwick: it teaches
  stolen chests). Six more are drawn from the seed at the start: two of Brindle's three, one of Thornwick's two, two of
  Crownhold's four, and the Emporium's. No two seasons hold the same tales, and some tales draw their own secrets (who
  the Magpie is, what's behind the Barrow door).
- **How many at once.** At most two beats at the counter a day and one at night. A beat that brings a chest takes a
  regular's place in the queue; a talk beat comes on top (it's short). The first days are left to learning the trade.
- **What you're told.** A person who means to come back says when, and the notebook's calendar marks it. Notices
  (wanted chests, the Guild's and the Crown's word, witnesses' accounts) stay on the notebook's Town page while they
  stand, with every tale begun and where it's got to, and where you stand with each power.
- **What a choice costs.** A choice's price is on its button. Every consequence is stated when it happens (a line, a
  note under the person, the Town page), and nothing is a hidden roll: a tale's secret is drawn when the tale begins
  and never changes, and a rule a tale sets (the Wardens search an enemy every Guild night) holds every time.

## Where you stand

Each power keeps an account of you, from -3 to +3, shown on the Town page with what it means at the level you're at.
A choice moves it by one to three. Effects at +2 and over, and at -2 and under:

| Who | At +2 or better | At -2 or worse |
|---|---|---|
| **The commons** | they send their neighbours: one more customer a day | they stay away: one customer fewer a day |
| **The Wardens** | Hollis warns you before a search (nothing's found), and returns pay a third more | they search your back room every Guild night: anything stolen in it is taken, and you're fined a tenth of the due |
| **The Lockless** | Nim's friends pay half (not a quarter) of what a back-room chest cost you | they rob you on the third night of every week: a tenth of your purse |
| **The Guild** | it overlooks a twentieth of every due | it audits you: a twentieth more on every due |

The abbey, the Old Wood, the Arcanum, Ironroot and the Crown keep accounts too: their tales read them, and a person
whose people you're at +2 with offers their tools a quarter cheaper (at -2, half as much again).

## Stolen chests, the mark and the back room

- **The mark.** A chest opens only for a Keywright it rightfully belongs to. Lay a hand on one (any shake, the drop or
  the lodestone) and the mark answers: warm if the chest is the seller's to sell, **cold** if it isn't. The note under
  the chest says so, every time. Taken sight unseen, you only find out when it won't open.
- **Who sells them.** Flick, the Magpie's runner, nearly always; Corvin the smuggler now and then; Lord Cassian his
  sister's; and whatever a tale brings. Nobody else: honest folk don't sell what isn't theirs, and nobody lies about it
  (Flick says only that it "came my way").
- **Send for the Wardens** (from Thornwick, where they keep a post): the seller runs, the chest goes home, and the
  Wardens pay you the finder's share (a fifth of a typical chest here). Wardens +1, Lockless -1.
- **Bought anyway**, it won't open: it goes to the **back room**. Each night you can return it to its owner (the
  Wardens post every theft, so the owner's known: the finder's share, a third more with the Wardens at +2; Wardens +1)
  or sell it to Nim's friends, who ransom such things back (a quarter of what you paid, half with the Lockless at +2;
  Lockless +1). A Wardens' search takes it for nothing.
- **Given back.** A chest's owner can give it to you, and a gift is as rightful as a sale: then it opens. Some tales
  end that way.
- **Lessons.** Sergeant Hollis's first notice teaches the mark (with tips on, Bram explains it), and Flick's first
  visit is a lesson: lay a hand on it, feel the cold, send for the Wardens.

## Tools from the counter

The best tools of the trade aren't sold in shops: the people who make them come to the counter to offer them (never
on the first day at a new place: word of a move takes a day to get round), and if you say no they come back a few days
later, once you've the gold to pay them (a tradesman calls on a shop that can afford him). The day's end keeps the building (moves, cabinets, the sign, the crier, the
charter and the seal) and the Guild's Bookkeeping Engine.

| Tool | Who brings it | Why them |
|---|---|---|
| Fine Lockpicks | Nettle | goblin picks: her clan's open anything in the hills |
| Brass Scales | Sergeant Hollis | the Wardens stamp honest weights on the market |
| Restorer's Bench | Brother Cadoc | the abbey's restorers mend relics, and teach |
| Witchlight | Ilse Varrow | the Arcanum lights its stores with it |
| Auction Block | Silas Brass | he runs Lantern Lane's auctions |
| Restorer's Workshop | Durgan Ironhand | a dwarf workshop, and his nephew as journeyman |
| Lodestone | Durgan Ironhand | a dwarf knows iron |
| Night Market | Gilt | his customers come after dark |
| Patrons' Book | Madame Zell | her clients, and their wants |
| Royal Warrant | Old Aldo | it's his to give |
| Master Restorer | Countess Vey | her own restorer, from the palace workshops |
| Curio Case, Velvet Case | Nim, at his table | somewhere to keep what he sells you |
| Nim's lantern, stool and standing order | Nim, at his table | each lays out one more curio a night |

## Nim's table, every night

Nim comes by every night after the shutters are down (after the Guild on its nights; from the third, so the guided first
days aren't crowded), with whatever the day turned
up: **one curio** at first. A lantern over the back door (Nim's Lantern), a stool by the counter (Nim's Stool) and a
standing order (Nim's Order) each lay out one more, four at most; Nim offers them himself, at his table. The cases he
offers too. His stock builds with you as before (curios for the way you trade are likelier, a legendary only for a
way you're building); with one card a night, every night's is the one most likely to fit.

## The tales

Each beat: who, when, what happens, and the choices (with what each does). "Purse" is Bram's working purse at the shop
(two typical chests): rewards and prices scale with it, so a tale pays as much at the Emporium as at the stall.

### The Key (always)

The main story, as before: Bram's welcome (week 1), his secret and the Hooded Woman (week 2), Mara's truth and the
King (week 4), the Guild's offer on the fourth Guild night (give Vane the shards for half that due, or keep them for
Mara), the shards for Mara's asks, the Crown Chest once the debt's paid. Two tales feed into it: the Foreclosures can
bring the Crown's decree, and the Defector tells you what the Guild means to do.

### The Shut Chests (always, from Thornwick)

Somebody is stealing chests around Thornwick: the Magpie, whom nobody's seen. Who it is is drawn when the tale begins:
Bettany Crane, Aunt Hesper or Rook Varga.

1. **The first notice** (Sergeant Hollis, the first day at Thornwick or after): the abbey's reliquary chest, green,
   with Saint Ide's key on its lid, has been stolen from the crypt. A Keywright's mark goes cold on a stolen chest; send
   for the Wardens and they pay a finder's share. *I'll send for you* (Wardens +1) or *I don't work for the Wardens*
   (Wardens -1, Lockless +1).
2. **The reliquary** (Flick, a day or two later): the green chest, cold to the mark. Send for the Wardens (it goes home
   to the abbey), buy it (back room), or pass (it's gone for good).
3. **The almoner** (Brother Cadoc, the next day): if it came home, the abbey's thanks (abbey +2) and its bench offered
   at half price; if it's in your back room, he asks for it back and pays the finder's share; if you let it go, he
   grieves.
4. **A black feather** (night, three days on): a letter from the Magpie, with a feather: she takes back what the
   Guild took from families, the abbey's chest was her runner's mistake, and if you ever want work, leave a lamp in the
   window. *Burn it* (nothing), *show it to Hollis* (Wardens +1, Lockless -1).
5. **The carter's word** (Hollis, a few days on): three more thefts, every one a chest the Guild foreclosed on; a
   carter saw the thief. The first clue, posted as a notice.
6. **The goblins' word** (Nettle, a few days on): goblins get the blame for everything; her cousin saw the thief too.
   The second clue.
7. **The bounty** (Hollis, a few days on): who is it? Name Bettany, Hesper or Rook, or nobody.
   - Right: the Magpie's taken, and the Guild's foreclosed chests go back to their families. The bounty (half a purse),
     Wardens +2, commons +1, Lockless -3; she never comes again.
   - Wrong: an innocent spends the season in the Wardens' cells (never comes again), Wardens -1, commons -1, and the
     Magpie's thank-you note that night: Lockless +1.
   - Nobody: Wardens -1. That night the Magpie comes to the back door herself, hood down: one of the three, and a
     rare curio for your silence. Lockless +2.

**The clues.** Each candidate is known by three things, noted on their page once you've met them: Bettany pulls pints
left-handed, has ink on her fingers from the tally, and wears house slippers; Hesper is right-handed, inky from the
marriage contracts, and rides in boots; Rook draws her bow left-handed, has a hunter's clean hands, and rides in boots.
Each clue fits two of them (left-handed; ink on her fingers; riding boots), and the two clues together fit only the
Magpie.

### The Marked Child (Brindle)

1. **The itch** (Pip, from day 3): his palm itches since he touched one of your chests, and there's a mark on it, like
   yours. *Tell him what it means* or *tell him to keep it hidden* (Pip remembers either way).
2. **The Mark-Finder** (Mistress Aubel, two days on): the Guild tests hands at the fair; a name pays well (three-quarters of a purse).
   *Pip Hollow* (the gold, Guild +2, commons -2; Pip goes into the Guild's care) or *none I know of*.
3. **His mother** (Wynn Hollow, the next day): they're searching houses; will you hide him? *As my apprentice*
   (commons +1) or *I can't risk it* (the Guild finds him anyway; commons -1).
4. **The search** (night, two days on): Aubel at the door with two Guild men. *There's no child here* (Guild -2),
   *take him* (Guild +1, commons -2), or *the Guild still takes Bram's word* (spends his word for the run).
5. **The apprentice** (every week after, while he's yours): on each week's first morning Pip brings you a chest he's
   fished out, given freely (it opens), at the shop's colour. And he's in the last scene.

### The Widow's Debt (Brindle, ends in Crownhold)

1. **Jory** (Ada, from day 4): her son Jory is in the Guild's debt-house for his father's tithe; if you ever find
   his father's sword, KETTLE on the blade... A notice: Ada Kettle seeks her husband's sword.
2. **The pledge** (Silas Brass, at Crownhold): a soldier's pledge, never redeemed: "Kettle" on the ticket. A chest with
   the sword in it (and whatever else). Bought, the sword is set aside for Ada when it opens.
3. **The sword** (Ada, after): *give it to her* (commons +1), *sell it to her for what she has* (a few coins), or
   *keep it and sell it on* (its price; commons -1).
4. **The debt-house** (Ada, three days on): the Guild wants a purse for Jory; she has half. *Pay the rest* (half a
   purse: Jory works a week for you as your porter to pay you back, one more customer a day; and he knows the Guild's
   debt-house) or
   *I can't* (Ada's last visit).

### The Deep Door (Brindle)

What's behind the door is drawn when the tale begins: a hoard, or a fall.

1. **The door** (Tobin, from day 5): a door at the bottom of the Barrow, sealed with the Key-mark. *Stake him* (a
   purse) for rope, lamps and two men, for half of what's behind it, or *not this time*.
2. **The hoard** (Tobin, four days on): half of it's yours: a chest given freely, a colour finer than the shop's.
   **Or the fall** (Bettany, three days on): his men came back without him; a rescue costs another purse. *Pay* (he
   comes back with the chest) or *I can't* (he never comes back; commons -1).

### The Woodcutters (Thornwick)

1. **The deed** (Harl Caskett, the Guild's timber factor): the Guild's buying the felling rights on the Old Wood, and
   the deed wants a Keywright's mark as witness. *Seal it* (a tenth off the next due; Guild +1, the Wood -2) or *refuse*
   (Guild -1, the Wood +1).
2. **The druid** (Mother Sorrel, the next day): she knows what you did. If you refused: the Hawthorn Lady would speak
   with you tonight.
3. **The Hawthorn Lady** (night): promise to keep the Wood's oaks standing, and have a fey charm (a curio). *I promise*
   or *I can't promise that*.
4. **The last offer** (Caskett, at Crownhold): a fifth off the next due, this time. *Seal it* (Guild +1, the Wood -3;
   a promise broken: the fey take their charm back) or *refuse* (the Wood +2; the Hawthorn Lady's thanks).

### The Parcel (Thornwick)

1. **The parcel** (Corvin): keep it in your back room two nights, for half a purse when he collects. *Done* or *no*.
2. **The search** (night, the next): the Wardens are searching the market. With the Wardens at +2, Hollis warned you
   and nothing's found. Otherwise: *hand it over* (Wardens +1, Lockless -2, Corvin never comes again) or *say it's
   yours* (they find it: a fine of half a purse, Wardens -1, Lockless -1).
3. **Collection** (Corvin, two days after): half a purse, Lockless +1, and a smuggler's curio from his own pocket.

### The Foreclosures (Crownhold)

1. **The clerk** (Tobias Fenn, the Guild's counting-house): half the chests the Guild forecloses on won't open for any
   Keywright it sells them to. Why? *Seizure isn't a sale* or *ask the Guild*.
2. **The war chest** (Sir Rowan): the Guild took his family's hall for its tithe and sold its chests; he bought his
   grandfather's war chest back at the Guild's sale, and it won't open for their Keywright. He sells it to you (it's
   warm: it was always his to sell, so it opens for you), priced like any of his chests: the sword is his, for the fee
   he names, as always.
3. **The Roll** (Master Quire): the Chest Roll agrees: everything the Guild seized is still in the families' names.
   The Guild's been selling stolen chests for a century, and sending the ones that won't open down through the Gate.
   *Take it to the King* (when you're at the Emporium), *sell your copy to the Guild* (half a purse, Guild +2; Tobias
   vanishes), or *keep it*.
4. **The ledger page** (Tobias, night): they know he talked. A page from the Guild's ledgers: where the tithe goes.
5. **The decree** (Old Aldo, at the Emporium, if you're taking it to him): the Crown's decree cuts the Guild's tithe
   by a fifth for the rest of the season. Crown +2, Guild -3.

### The Unlocking (Crownhold)

1. **The brass plate** (Ilse Varrow): Archmage Orme asks a drop of a Keywright's blood on a brass plate, and pays a
   quarter of a purse. *Give it* (Arcanum +1) or *refuse* (Arcanum -1).
2. **The engine** (Ilse, three days on): Orme has built an engine to copy the mark: any chest would open for anyone,
   and Mara says that's the end of the Locking. *Help her break it* (Arcanum -2; her witchlight at half price, or a
   curio), *tell the Guild* (half a purse, Guild +2, Arcanum -3: Orme's taken away), or *stay out of it*.
3. **It works** (Orme, at the Emporium, if you stayed out): it opens one chest in three for anyone, and the Gate's
   cracking: the Maw's gilded iron leaks into every purse, and any coin anyone brings may be demon gold from now on (as in
   a season of forgers). A purse for your part.

### Ironroot's Crown (Crownhold)

1. **The crown** (Durgan): Ironroot's crown was locked in the Royal Treasury at the Locking, and the Guild bans
   dwarf-gold. *I'll ask the King* or *nothing I can do*.
2. **The King's chest** (Old Aldo, at the Emporium): he gives you the chest the crown's in (it opens: it's yours by his
   gift), the crown set aside for the dwarves.
3. **Ironroot's thanks** (Durgan): his smiths build you a workshop for nothing (or the lodestone, or gold).

### The Vell Inheritance (Crownhold)

1. **The trick box** (Lord Cassian, a trading beat): grandmother's box, his sister's by her will: cold to the mark.
2. **The sister** (Lady Isaura, the next day): if it's in your back room, *give it back* and she gives it to you
   properly (it opens for you: a chest given freely) or *keep it*; if you sent for the Wardens, Cassian spent a night
   in the cells and it did him good.
3. **The last box** (Isaura): grandmother's last trick box, with a riddle about what she warded in it: priced fairly, or
   given if you gave the first one back.

### The Defector (the Emporium)

1. **Bat's ears** (Skrae): it's heard enough of the Maw. Hide it in your back room three nights, and it'll tell you
   what the Guild tells him. *Hide it* or *no*.
2. **The courier** (Tick, the next day): seen a demon with bat's ears? The Maw pays for runaways. *No* or *in my back
   room* (half a purse; Skrae's taken back).
3. **Free** (Skrae, night, three days on): it tells you what it heard, and the Maw's gaze never touches your counter
   again (his visits don't raise anyone's price), and a curio.

## For the simulation and the bot

`Strategy.Choose` picks a beat's choice the way a careful player would: by what each is worth in gold over the rest of
the run (`ChoiceDef.Worth`), and when two are close, the kinder one. It sends for the Wardens on a cold chest, returns
what it bought by mistake, accepts a tool offered at the counter when it would have bought it at the day's end, and
buys at Nim's table every night. A blind buyer pays for stolen chests and can't open them. The sim's "Tales" section
reports how often each tale is drawn, finished and how it ended, and what it was worth.
