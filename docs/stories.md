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

- **Which tales.** Every season has the main story (**Bram's Account**, 0.34; `commission` in code), **The Shut Chests**
  (from Thornwick: it teaches stolen chests), **The King's Court** (at the Emporium, if you've anything to bring it)
  and, for Hesk, Rosa or Isolde, their own tale (0.32, below). Six more are drawn from the seed at the start (0.36):
  two of Brindle's eight, two of Thornwick's seven, one of Crownhold's nine and one of the Emporium's two
  (`TaleBook.Draw`). No two seasons hold the same tales, and some tales draw their own secrets (who the Magpie is,
  what's behind the Barrow door, whose the dowry is, who wins the duel). (0.30 to 0.35 drew eleven or twelve over a
  season of seven weeks: three of each place's and both of the Emporium's. The fresh-eyes playtest of 0.35 met "eight
  tales in ten days, most of them asking for money"; a season of four weeks holds about a beat a day, the campaign's
  included: 21 a season in the sim, where 0.35 had 34.)
- **When (0.36).** The beats below give their days as they were written, for 0.30's season of seven weeks with the
  markets about days 8, 16 and 25. A season is four weeks now, a market each (days 1, 6, 11 and 16), and the engine
  keeps each tale's shape in it (`GameState.Wait`, `SeasonDay`): a tale's first beat comes at the same point of the
  same market's week (written for day 10, a quarter of the way through Thornwick's eight days, it comes on day 7); a
  later beat waits about three-fifths as long as written (a day is a day, two stay two, four become three, a week
  becomes four); and a tale that begins at the Emporium, which has the season's last five days to be told in, is told
  a beat a day. What a person says about coming back ("in a few days", "before the Guild's night") was never a count,
  so the lines stand; the calendar marks the day the engine means.
- **How many at once.** At most two beats at the counter a day and one at night. A beat that brings a chest takes a
  regular's place in the queue; a talk beat comes on top (it's short). The first days are left to learning the trade.
- **What you're told.** A person who means to come back says when, and the notebook's calendar marks it. Notices
  (wanted chests, the Guild's and the Crown's word, witnesses' accounts) stay on the notebook's Town page while they
  stand, with every tale begun and where it's got to, and where you stand with each power.
- **Following a tale (0.32).** A tale's first talk says "A NEW TALE BEGINS" under its title, and every talk is headed by
  the tale it belongs to. The notebook's **Stories** page lists every tale begun (those still going in gold) and shows the
  one picked whole: its setup (who, what's at stake, what you can do), every line of what's happened so far, and what
  comes next, worked out from the tale's beats (`GameState.TaleNext`): who's due, at the counter, the back door or by
  letter, and about when, or "once the shop is at Crownhold"; or that nothing more is expected for now.
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

## What a tale leaves behind (0.30)

A tale's ending isn't a line on the Town page: it changes the shop for the rest of the season, and the season's end.
Every one of these is stated when it happens (a line, a note under the chest, the Town page), holds every time, and
is in the save.

- **Who comes, and who doesn't.** Someone a tale takes away (into the Guild's care, the cells, north, down the Barrow)
  never comes to the counter again, and nor do their tools: lose Nettle and nobody offers you goblin picks. Some come
  only because of what you did: a freed debtor carries your chests, a rival takes one of your customers a day while he
  trades across the road, pickets stand at your door until someone calls them off.
- **Officials** (`SellerDef.Comes`). Be a friend of the authorities and their officials come to sell: **Bailiff
  Marrow** brings the chests the Guild took for tithe while the Guild counts you a friend (+2), at its book price (the
  colour alone, and cheap), but a third of them were seized, not sold, and are cold to the mark; send for the Wardens
  on one and the Guild hears of it. **Quartermaster Brandt** sells what the Wardens recovered and nobody claimed in a
  year and a day while the Wardens count you a friend, by their tariff (so much a pound, so heavy things are dear and
  light things cheap). Fall out with them and they stop coming.
- **Goodwill** (`GameState.Goodwill`). People you've helped trade with you on better terms, said under their chest: a
  neighbour's price at Brindle while Pip is safe with you (Wynn's word: a sixth off every Brindle regular); no fee for
  shaking from Nettle when you've spoken for her clan; Sir Rowan's grandfather's blade, so the best weapon in his
  chests stays yours; dwarf prices from Durgan once the crown's home (a fifth off).
- **Favours** (`GameState.Owed`). Some people owe you one after what you did for them (the Friends & foes page lists
  them). On a Guild night you're short, call on one: they bring what they can, an eighth of the night's due, once a
  season each. And everyone who owes you is there at the end.
- **The case against the Guild** (`GameState.Evidence`): papers for the King's court (a court case, not 0.33's Case
  page, which 0.34 replaced with the Inquiry). Papers turn up in tales: the Chest Roll's copy and the ledger page (the
  Foreclosures), the debt-house tallies (Jory), the Mark-Finder's register (Pip), the witness copy of the Wood's deed,
  the Guild's purse in the Unopened's alms box, the Archive's ledgers saved from the fire, Orme's notes (the Unlocking)
  and Skrae's word (the Defector). At the Emporium, with two or more, the King asks for them (**The King's Court**): the
  Guild offers to buy them first; with four or more the court tries the Guild (the rest of the debt halved, and no
  interest after it), with fewer it decrees a fifth off. A trial scores 75 points, and the Crown stands with you at the
  treasury. Nothing else is won by a count of papers: the court voids no pledge.
- **Contests** (`Visit.Contest`). A chest put up to be read in front of people: it's given for a lucky coin (it opens
  for you), and a perfect read wins what was staked on it. A rival hedge-wright and the harvest fair are settled that
  way.
- **Loans** (`GameState.Loan`). Mother Tansy lends a purse against next week's first morning, a third on top. Pay it
  and she lends again on better terms; don't, and she takes the best thing on your shelf, and her men stand at your
  door a week.
- **The Guild's claims** (`GameState.Claims`, 0.33). Someone a tale leaves owing the Guild is taken on that week's
  Guild night unless the note's paid: Old Haddon every season (Bram's Account: forty gold, due on the first Guild
  night) and Jory Kettle if nobody paid his debt (the Widow's Debt). Vane names them as he goes ("The Guild has a note
  to collect tonight"), and they never come to the counter again; once you remember, Haddon can come back to you in a
  Guild crate (below).
- **The gathering** (week 4's story morning, once you remember): Bettany (or Bram, if he's home) says Vane was at the
  Crossed Keys asking who your friends are: who'll stand at the treasury door with you, and take in whoever comes out?
  Each friend says so: the powers at +2 (the Crown with the Guild on trial, too), everyone who owes you or came when you
  called, Pip if he's your apprentice, and the Haddons if Old Haddon's home or his note was paid. Two of them make the
  plan's way out (below); with fewer, hire the bargemen at the Lantern Market. With the plan whole, they're at the door
  on the treasury night.
- **The season's end** (`TaleDef.Ending`): after the last scene of a run, won or lost, Bram (or Bettany, if Bram isn't
  home) tells what became of everyone whose tale began: one line each, by what you chose.

## Keepsakes (0.32)

Some people give you something to keep at the end of their tale (`Keepsakes.cs`). A keepsake works like a curio, with
one effect of its own, but it takes no curio slot, Nim never brings one or buys one back, and the Guild won't take one on
a short night. Sellers can't know of one, so none is in the power index: what it does is your edge. A talk that gives one
shows it as a card (its picture, on a teal halo, and what it does); keepsakes sit after the curios at the counter and the
day's end, and the collection has a Keepsakes page (the ones you haven't had say which tale gives them). They borrow a
curio's, a tool's or a thing's picture until they have their own.

| Keepsake | From | Does |
|---|---|---|
| Fallowford's Weights | The Drowned Town (Hesk) | Coin and gems sell for a fifth more |
| Grandmother's Stall Key | Lantern Lane (Rosa) | A seller who walks out on Rosa comes back like anyone else |
| The Marchmont Seal | The Marchmont Pieces (Isolde) | Her visitors pay half as much again at the door |
| The Brass Apprentice | The Brass Apprentice | One more customer every day |
| Tobin's Lamp | The Deep Door | Each week's first morning, a Barrow chest from Tobin, freely given, a colour finer than the shop's |
| The Hawthorn Charm | The Woodcutters | Everything 5% likelier to come out a step better (gone if the promise is broken) |
| Sir Aldwin's Coin | The Field's Dead | The day's first chest: nothing in it comes out worse than its chest promised |
| Rafe Kettle's Medal | The Widow's Debt (the sword given back) | Weapons sell for a quarter more |
| Sir Edric's Oath | The Sleeper | The Wardens never search you and the Lockless never rob you |
| Wren's Tuning Fork | The Silent Choir (the first echo freed) | Everything 10% likelier to come out a step better |

How often the careful collector (Kit) ends a season with each, of the seasons that drew its tale (the sim's Tales
section, 0.32): the Brass Apprentice 68%, Tobin's Lamp 41%, the Hawthorn Charm 6%, Sir Aldwin's Coin 48%, Rafe Kettle's
Medal 61%, Sir Edric's Oath 75%, Wren's Tuning Fork 28%. The partners' three are given at the end of their own tales.

## Their own tales (0.32)

Kit is Bram's niece, and her story is Bram's account (he pledged his hand for her). The others aren't family: each was
born with the Key-mark somewhere else and came to Bram's stall as his partner, for reasons of their own (Bram is "Uncle
Bram" to Kit and "Bram" to them, `Hints.Mentor`). Each has a tale drawn every season they play (`TalePlace.Kin`,
`TaleBook.Own`), ending in a keepsake.

- **Hesk, The Drowned Town.** Hesk is a grocer from Fallowford, the river town the Guild drowned when it opened the
  Aldwater weir to flood out a smugglers' road. (1) *A letter from Mabry* (night, from day 3): the Guild's salvage barges
  are dredging Fallowford for its common chest, every family's savings in the town's name; *I'll watch for it* or
  *Fallowford's gone*. (2) *Mabry* (Thornwick): it goes to the Guild's salvage sale; *stand witness* (Guild -1) or *I
  can't fight the Guild* (lost). (3) *The sale* (Bailiff Marrow, Crownhold): the chest, cold to the mark (seized, not
  sold). Send for the Wardens and it goes home; buy it and it waits in the back room to be returned; pass and the Guild's
  buyer sends it down the Gate. (4) *Home* (Mabry): the town gives it to Hesk to open for them (a gift, so it opens),
  commons +1; that night, Fallowford's Weights, and Mabry owes you a favour. Or (4') a last letter: it went down the Gate.
- **Rosa, Lantern Lane.** Rosa Brightwater's mother kept the best stall on Crownhold's Lantern Lane until the Guild took
  it and gave it to its own man, Orrin Vash. By the Lane's custom a family wins its stall back if one of its blood reads a
  chest exactly in front of the traders. (1) *Marta's letter* (night, from day 3). (2) *Vash* (Thornwick): a purse and a
  half to stay away (Guild +1: the stall's lost) or *I'll see you on the Lane*. (3) *The reading* (Crownhold): a contest
  chest; exactly right wins the stall and half a purse. (4) Wrong: *a second reading*, if your family stands the traders
  their supper (a purse); (5) the second chest. Won: Marta gives you Rosa's grandmother's stall key that night, commons +1, a favour.
- **Isolde, The Marchmont Pieces.** Lady Isolde Marchmont is the last of a house the Guild foreclosed on. The Guild sold
  the house's chests; they're still the house's, so they're warm to her hand whoever paid for them. (1) *Pemberton's
  letter* (night, from day 3): he's traced three. (2) *The jewel case* (Aunt Hesper, Thornwick). (3) *The map chest*
  (Master Quire, Crownhold). (4) *The seal* (Mister Vane, Crownhold): the Guild kept the long box with the seal for
  itself, two purses. (5) That night, the Marchmont Seal; with all three pieces home, the Crown has heard of it (+1).

## The tales

Each beat: who, when, what happens, and the choices (with what each does). "Purse" is Bram's working purse at the shop
(two typical chests): rewards and prices scale with it, so a tale pays as much at the Emporium as at the stall.

### Bram's Account (always, 0.34)

The main story (`TaleBook.Campaign.cs`, `Story.cs`, `Campaign.cs`; in code the tale is still `commission`). 0.34 rebuilt
it as a campaign across seasons, in place of 0.33's Gilding and its Case page: `docs/narrative.md` has the truth under
it and why it's told this way. Until the treasury first swallows you, a season is ordinary business, told kindly; after
that every season is the same spring again, and you remember (`GameState.Remembers`, from the profile's `Swallowed`).
Each beat comes once a season, at the shop where it belongs, and none of it is a roll. A scene you remember plays
shorter: the welcome is a line and two taps, the summons two lines, the swallowing has no build-up.

**Every season** (until the true ending):

1. **Bram's welcome** (the first morning): the trade in a sentence ("listen before you buy"), Bettany's cellar, and his
   habit: when he wants you at the counter he taps it twice, then once. "And mind: some chests listen back." Once you
   remember: darkness, two taps and one, his welcome, and your own line: "The first morning, again. I remember the
   treasury. I remember it breathing. He doesn't."
2. **Garrick's green chest** (Garrick, from day 2, in week 1): his granddad's green chest from the attic, with the same
   things in it every spring (two coin and a cloak). The profile keeps what the first one bought held
   (`Profile.FirstChest`), and once you remember the note under it says so: "You know this chest."
3. **Bram's account** (Mister Vane, the second evening): twenty years open, and the Guild would like it settled this
   season, in seven payments, one every fifth night; miss one and the Guild takes the stall. The Guild introduces
   customers, carts chests and lends where coin is short: "Do ask." Once you remember, he knows you too: "We've met
   before, I think. You always look at my ring like that."
4. **Old Haddon's note** (Nell Haddon, from day 3, in week 1): her father signed the Guild's paper for a new lathe,
   forty gold, due on its first night; unpaid, the Guild takes the yard, and him with it, "to work it off". *Pay his
   note* (40g, commons +1: he stays home, and the Haddons stand with you at the end) or *I can't, not this week* (the
   Guild takes him on its first night). Once you remember, you say what happened to him last time.
5. **The summons** (the first Guild night, once the due's paid): the Guild's treasury has asked for Bram, for an old
   consignment that needs a keywright who knows it. He asks you to keep his lamp lit, says he'll write every week, and
   goes with Vane; Old Haddon goes too if his note fell due. From now on Bram's tips are notes in the margins of his
   notebook, and Bettany tells the season's news. Once you remember: "He won't write."
6. **The Guild's letter** (week 2's story morning): Bram's handover is taking longer than expected; keep the shop
   running until he returns. Four marks of a master keywright, certified by the Guild's assessors, each earn a piece of
   the **treasury key**, sent by Guild courier with a master's fee (one and a half working purses) the night it's earned
   (`Campaign.Pieces`, `GameState.EarnPiece`):
   - the **reader's**: a perfect read of a chest that hid something (a false bottom or a ward), or held five things or
     more;
   - the **warden's**: a stolen chest settled (the Wardens sent for, or a back-room chest returned to its owner);
   - the **patron's**: a commission filled, every thing it asked for;
   - the **locksmith's**: a Guild Lock picked masterfully, or a Dwarven Tumbler picked without force.

   Keep Bram's account paid and bring the whole key to the treasury on the season's last night: his commission needs a
   second keywright, and when it's done his account is closed and he comes home. The Guild's night shows the key beside
   the debt, and the notebook's Inquiry ("Where is Bram?") says how each piece is earned.
7. **The Guild's news**: Vane's word as he goes on each Guild night (`Story.Parting`): Bram sends his regards (week 2),
   and the treasury opens on the season's last night, Bram is counting on you and the key (week 3). Week 3's first
   morning: still no letter from Bram. Week 4's story morning, before you remember: a letter: the treasury opens on the
   season's last night, "Do bring the key."; and the day's card says it's the debt's last week. (0.36; over 0.34's
   seven weeks he had a word for each of weeks 2 to 6.)
8. **The treasury** (below), on the night the account's paid with the key whole. A season that clears the debt without
   it ends with Bram's account closed and Bram never home; one foreclosed ends before the last night, which comes
   without you.

**Once you remember** (every season after the first swallowing, until the true ending):

- **Bram's tuning fork** (Bailiff Marrow, from day 4 until the last week, while Bram's away): a lot of the Guild's repossessed
  goods, "tools, mostly, and a tuning fork with a tag on it". Bought, it's Bram's, tagged *from the weighhouse, palace
  road*: not the treasury (held this season only).
- **The carts** (Sergeant Hollis, off duty, at Thornwick, from day 6, unless you took the Guild's bribe or have seen the
  weighhouse this season): Guild carts pass his toll every fifth night under sacking and go on down the palace road; a
  sack slipped, and it was a gold man. *Meet him at the toll on the Guild's night* or *not this season*. (Heard in an
  earlier season, it's one line: "You've that look again.")
- **The weighhouse** (Hollis, that Guild night; Nim doesn't come): the cart goes down the palace road to a long, low
  hall that smells of hot metal: crates in rows, each chalked with a name and a weight ("H. Haddon, 14 stone", "Bram,
  keywright, 13 stone"). You copy the register's page before the lamps go out. (Or buy a night there at the Lantern
  Market; the Foreclosures' ledger page names the weighhouse too.)
- **The first mimic** (Brother Cadoc, Thornwick, from day 7, while you've none): a chest from the crypt, "warm like
  bread"; it breathes while it sits still. More come on their own from Thornwick, one ordinary chest in 90 (`Mimics`).
- **The crates** (Thornwick): a Guild crate off the palace road (iron bands, the Guild's seal), with someone gilded in
  it. **Old Haddon's** comes by Corvin ("fell off a Guild cart"; half a purse), from day 7, if the Guild took him and
  you know of the carts or the weighhouse. **Bram's** comes once you know of the weighhouse, from day 11: by Corvin, for
  a purse, once the Lantern Market's bargemen have diverted it or from week 3; or by Hollis, for a lucky coin, with the
  Wardens as friends (they stopped a cart at his toll). At the counter a crate makes one sound: while it sits still,
  whoever's in it knocks (a shake stops them). *Knock back* and they answer: the gilded are alive (Bram answers with his
  tap, twice and once). Opened, they go to the back room, gold from head to foot.
- **The back room** (Nim's table, each night): a full mimic eats the gold off one of the gilded, then wakes hungry. Then
  the note: Haddon's is forty gold (paid, he walks home: commons +1); Bram's is his account, the debt. Freed while the
  note still stands, they're taken back on the Guild's next night.
- **Bram's homecoming** (the night he's freed with the debt paid; `Story.Homecoming`): "Cold. I'm so cold. And this is
  our stall." He tells you his pledge (twenty years ago, his hand, mark and all, for a marked baby), and he's at the
  stall for the rest of the season. Every season after, he taps the counter on the first morning, and doesn't remember.
- **Mara** (Crownhold, from day 11, once you know of the weighhouse): a woman in a grey hood. "You remember a spring
  that hasn't happened yet." She's Mara, the last of the Keyhold, and tells you the seal's last clause: if the Maw ever
  breaks out without leave, the year turns back to its first morning; whoever has touched the seal's core remembers, so
  does he, nobody else. The Guild doesn't own that chest: find out what the Crown pledged.
- **The charter** (Master Quire, Crownhold, from day 11, once you know the clause; or Old Aldo at the Emporium, from day
  17; or the Roll in the Foreclosures): the Crown pledged the treasury's keys and its takings to the Guild, ten years
  since. Not its people, and not a keywright's word.
- **The register** (Quire, Crownhold, from day 11, once you know the charter and the weighhouse and hold no copy this
  season): *have him copy the weighhouse pages* (half a purse) or *not now*.
- **What the Maw is** (Mara's note under the door, from Thornwick, once you've brought Bram home in any season and know
  the clause): before there were Locking chests there was one chest, empty, and it wanted. "We didn't invent the
  Locking, Keywright. We copied it."
- **The Maw's tactics** (Vane's Guild nights, `DuesPhase`; a season has four of them since 0.36): reassurance as he
  goes on the second ("Bram is well, Keywright. Why go looking?") and the third ("You were at the toll, I hear", or
  "You remember more than you should. So do I."). **The bribe**, on the second, before you pay (`Campaign.BribeWeek`;
  the fourth of seven until 0.36): half tonight's due, if your questions stop at the toll. *Take it* (Hollis has
  nothing to say to you this season) or *ask my questions*. **The bargain**, on the third (`BargainWeek`; the sixth),
  if you hold a piece and Bram isn't home: Bram at your door tomorrow, warm and whole, for the
  key's pieces. *Give him the pieces* (Bram's home, and there's no treasury for you this season; the due is still due,
  and he jumps at every tap on the counter) or *keep the pieces* ("Then he stays where he is. Gold keeps.").
- **The gathering** (week 4's story morning, the first at the Emporium): who'll stand at the treasury door with you
  (*What a tale leaves behind*, above).

**The Lantern Market** (`LanternMarket.cs`; Nim's table, the third night of a week from Thornwick): Nim's people sell
access, papers and transport, never answers. Once you remember: a night at the weighhouse in a Guild coat, with a page
of its register, once you know of the carts (half a purse; half that if you've been inside in an earlier season); Bram's
crate off the palace road, once you know of the weighhouse (a purse: Corvin brings it); and the Wet Hen's bargemen for
the treasury's last night, once you know the seal's clause (a purse). The Maw's buyers pay half as much again for
anything gold on your shelf (an eye for him each; the season's end counts them); the Arcanum's menagerie buys a mimic
(more for one fed full), or keeps and feeds yours for a fee.

**The treasury** (`Main.Treasury`, `TreasuryPhase`): on the night Bram's account is paid with the key whole (the
season's last night, the night an early payment clears it, or, trading on after one, the night the key's last piece
comes), the Guild's carriage takes you there, and its great chest comes to a counter, with the counter's own tools.

- **The first time** ("Bram's commission"): shake it (drag it, as at the counter), and nothing moves inside. Harder: not
  even the wooden ring every empty chest gives. *Drop it*: no boom, no ring, nothing. It slips out of your hands. The
  lid rises, and falls. It's breathing. It opens towards you and swallows the slip, the counter, the screen. Darkness;
  two taps on the counter, and one; *Open your eyes*: the rewind (below).
- **Again** (once you remember, without the plan): *open it again* (it's breathing already, and swallows you at once) or
  *stay away tonight* (you leave the key on the Guild's table and walk home; the Guild opens it with Bram, and the
  season ends: THE TREASURY OPENS WITHOUT YOU).
- **The final opening** (the plan whole): your friends come too, and you don't shake it. Hold still and listen: a knock,
  then another: people. *Knock back*, and they answer: everyone the Guild weighed and crated. *Turn the key: to look,
  not to own*, and they come out into the lamplight, gold flaking off them, and your friends take them home. Then the
  Maw speaks: "You have already saved him once. I can make this time remain. Take him home. Leave the door to me." (Or,
  if you've never brought Bram home, "Take them home".) If you hesitate: "He knows whom he came to save. But you still
  want to see what is inside." (its argument, not the game's). *Leave the door to it*: the next morning goes sour (every
  chest on every stall under the Guild's seal; by evening it wants the road, the river and the light), it reaches past
  the door for everything, the seal wakes, and it's the first morning again. *Close the door*: the seal holds, nothing
  turns, nothing breathes, everyone the Guild took this season is out, and the next day comes.

**The plan** (the Inquiry's fourth thread, "How do we bring them out?", once you know the clause and that the gilded are
alive; `Inquiry.PlanLines`): four lines, each ticked when it's true this season.

1. *Find them, and know them* (`PlanFind`): you've heard the gilded answer you (a crate knocked back, or Bram's tap),
   and you hold the weighhouse register this season (seen there, bought at the Lantern Market, or copied from Quire's
   Roll).
2. *Free them* (`PlanFree`): Bram's pledge redeemed (the debt paid), and a full mimic in your care (the back room or the
   menagerie).
3. *A way out, and people to take them in* (`PlanWayOut`): two friends who'll stand with you, or the Wet Hen hired.
4. *Open the door without giving it away* (`PlanDoor`): the treasury key whole, and you know both the seal's clause and
   the charter.

**The rewind** (`Main.Rewind`): when the Maw breaks out without leave (it swallowed you, or you left it the door), the
season is banked (its score, and how it ended, `Profile.LastEnd`) and the next starts at once, the same keywright at
the same level, on the first morning. What you've found out carries over (the Inquiry's entries, `Profile.Known`, so
the leads stay open), and what Garrick's chest held; money, the key's pieces, the register and the tuning fork, papers,
friends, the freed and Bram's rescue go back to how they were. A season that ends any other way (foreclosed, cleared
without the key, or you stayed away from the treasury) ends as a run does, with the season's end and the score; its
last night happens without you, and the next season remembers if you've ever been swallowed.

**The next day** (the door closed; `Story.NextDay`, `GameState.BeginNextDay`): a date the season has never reached.
Bram brings tea, and his hand is warm as the pot; Old Haddon's in his yard, swearing at a lathe; half the crossroads
walked home last night, and nobody can say from where. Then one customer: Wynn, whose waterwheel won't turn without
the tools in her father's millwright's chest, given for a coin. You open it, two taps on the counter and one, and the
shop goes on (THE NEXT DAY, 200 points). After that the campaign's done (`Profile.Finished`): a new season is a new
year, Bram stays at the stall, the treasury stays shut, and Garrick's chest, Nell's note, the summons, the crates and
the mimics don't come; the tales, the levels and the collection go on.

**The season's end** (its own lines, before the tales'): Bram home (and how), or his account closed and Bram never
home, or the stall taken; Old Haddon home, kept home, or never back from working off his note; any gold sold to the
Maw's buyers; a mimic still asleep in the back room; and, once you remember, "the Guild opened the treasury without me".

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
6. **Wynn's thanks** (0.30, two days after the search, if he's yours): the mill and half of Brindle know what you did.
   *Thank your neighbours* (a neighbour's price: every Brindle regular asks a sixth less while Pip's safe with you) or
   *keep your neighbours' coin* (commons +1). Either way Wynn owes you a favour.
7. **The register** (0.30, Pip, from Thornwick): the Mark-Finder keeps a book of every marked child and where the Guild
   sent them. *Copy one page* (a paper for the case against the Guild; Guild -1) or *too dangerous*.

Named or taken, Pip never comes to the counter again, nor does Wynn; the Guild's +2 for a name is usually enough to make
it count you a friend, and then its bailiff comes to sell (the officials, above).

### The Widow's Debt (Brindle, ends in Crownhold)

1. **Jory** (Ada, from day 4): her son Jory is in the Guild's debt-house for his father's tithe; if you ever find
   his father's sword, KETTLE on the blade... A notice: Ada Kettle seeks her husband's sword.
2. **The pledge** (Silas Brass, at Crownhold): a soldier's pledge, never redeemed: "Kettle" on the ticket. A chest with
   the sword in it (and whatever else). Bought, the sword is set aside for Ada when it opens.
3. **The sword** (Ada, after): *give it to her* (commons +1), *sell it to her for what she has* (a few coins), or
   *keep it and sell it on* (its price; commons -1).
4. **The debt-house** (Ada, three days on): the Guild wants a purse for Jory; she has half. *Pay the rest* (half a
   purse: Jory works a week for you as your porter to pay you back, one more customer a day; and he knows the Guild's
   debt-house) or *I can't* (0.33: the Guild calls his note in on its next night).
   **The note** (Ada, two days on, while it stands): one more chance to pay the rest. If nobody does, the Guild
   collects Jory on its night with a cart, "to work off the note" (Ada at the back door that night: they wouldn't say
   where), and he never comes to the counter again (*The Guild's claims*, above).
5. **The tallies** (0.30, Jory, three days on, if he's free): half the debts in the debt-house were never owed. *Write it
   all down* (a paper for the case) or *put it behind you*. Jory owes you a favour either way; Ada does too if you gave
   her the sword.

### The Deep Door (Brindle)

What's behind the door is drawn when the tale begins: a hoard, or a fall.

1. **The door** (Tobin, from day 5): a door at the bottom of the Barrow, sealed with the Key-mark. *Stake him* (a
   purse) for rope, lamps and two men, for half of what's behind it, or *not this time*.
2. **The hoard** (Tobin, four days on): half of it's yours: a chest given freely, a colour finer than the shop's.
   **Or the fall** (Bettany, three days on): his men came back without him; a rescue costs another purse. *Pay* (he
   comes back with the chest, and owes you his life: a favour) or *I can't* (he never comes back; commons -1).

**0.32: the old kings' halls.** (5) *The stair* (Tobin, at Crownhold, ten days after the stake, once he's back): past
the vault a stair goes down to the old kings' halls under the Barrow; *stake the expedition* (two purses) or *once was
enough*. (6) *Under the Barrow* (Tobin, four days on): a chest a colour finer, freely given. (7) That night he goes back
down for good and leaves his lamp: Tobin's Lamp, a Barrow chest from him every week.

### The Woodcutters (Thornwick)

1. **The deed** (Harl Caskett, the Guild's timber factor): the Guild's buying the felling rights on the Old Wood, and
   the deed wants a Keywright's mark as witness. *Seal it* (a tenth off the next due; Guild +1, the Wood -2) or *refuse*
   (Guild -1, the Wood +1).
2. **The druid** (Mother Sorrel, the next day): she knows what you did. If you refused: the Hawthorn Lady would speak
   with you tonight.
3. **The Hawthorn Lady** (night): promise to keep the Wood's oaks standing, and have a fey charm (the Hawthorn Charm, a
   keepsake since 0.32). *I promise* or *I can't promise that*.
4. **The last offer** (Caskett, at Crownhold): a fifth off the next due, this time. *Seal it* (Guild +1, the Wood -3;
   a promise broken: the fey take their charm back; Mother Sorrel never trades with you again) or *refuse* (the Wood
   +2; the Hawthorn Lady's thanks).

Sealing either time keeps the witness copy of the deed, which names the coin it was paid in (seized from families): a
paper for the case against the Guild.

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
4. **The ledger page** (Tobias, night): they know he talked. A page from the Guild's ledgers: where the tithe goes. (The
   Roll's copy and the ledger page are both papers for the case against the Guild. 0.34: the ledger page is an Inquiry
   entry too, "H. Haddon, fourteen stone, to the weighhouse", which counts as knowing of the weighhouse; and once you
   remember, Quire's Roll gives you the treasury's charter.)
5. **The decree** (Old Aldo, at the Emporium, if you're taking it to him): the Crown's decree cuts the Guild's tithe
   by a fifth for the rest of the season. Crown +2, Guild -3.

### The Unlocking (Crownhold)

1. **The brass plate** (Ilse Varrow): Archmage Orme asks a drop of a Keywright's blood on a brass plate, and pays a
   quarter of a purse. *Give it* (Arcanum +1) or *refuse* (Arcanum -1).
2. **The engine** (Ilse, three days on): Orme has built an engine to copy the mark: any chest would open for anyone,
   and Mara says that's the end of the Locking. *Help her break it* (Arcanum -2; a curio from the stores, Orme's notes
   for the case against the Guild, and Ilse's favour), *tell the Guild* (half a purse, Guild +2, Arcanum -3: Orme's
   taken away), or *stay out of it*.
3. **It works** (Orme, at the Emporium, if you stayed out): it opens one chest in three for anyone, and the Gate's
   cracking: the Maw's gilded iron leaks into every purse, and any coin anyone brings may be demon gold from now on (as in
   a season of forgers). A purse for your part.

### Ironroot's Crown (Crownhold)

1. **The crown** (Durgan): Ironroot's crown was locked in the Royal Treasury at the Locking, and the Guild bans
   dwarf-gold. *I'll ask the King* or *nothing I can do*.
2. **The King's chest** (Old Aldo, at the Emporium): he gives you the chest the crown's in (it opens: it's yours by his
   gift), the crown set aside for the dwarves.
3. **Ironroot's thanks** (Durgan): his smiths build you a workshop for nothing (or the lodestone, or gold), Durgan asks a
   fifth less for his chests from then on (a dwarf's price), and he owes you a favour.

### The Vell Inheritance (Crownhold)

1. **The trick box** (Lord Cassian, a trading beat): grandmother's box, his sister's by her will: cold to the mark.
2. **The sister** (Lady Isaura, the next day): if it's in your back room, *give it back* and she gives it to you
   properly (it opens for you: a chest given freely, and House Vell owes you a favour) or *keep it*; if you sent for the
   Wardens, Cassian spent a night in the cells and it did him good.
3. **The last box** (Isaura): grandmother's last trick box, with a riddle about what she warded in it: priced fairly, or
   given if you gave the first one back.

### The Defector (the Emporium)

1. **Bat's ears** (Skrae): it's heard enough of the Maw. Hide it in your back room three nights, and it'll tell you
   what the Guild tells him. *Hide it* or *no*.
2. **The courier** (Tick, the next day): seen a demon with bat's ears? The Maw pays for runaways. *No* or *in my back
   room* (half a purse; Skrae's taken back).
3. **Free** (Skrae, night, three days on): it tells you what it heard, and the Maw's gaze never touches your counter
   again (his visits don't raise anyone's price), and a curio. What it heard is a paper for the case against the Guild.

### The Hedge-wright (Brindle, 0.30)

1. **The cart** (Jasper Wick, from day 4, at the stall): a hedge-wright (the mark, no licence, as Bram was) sets up across
   the road and takes one of your customers a day for two weeks.
2. **The Mark-Finder** (Mistress Aubel, two days on): the Guild takes hedge-wrights as it takes marked children. *Jasper
   Wick* (half a purse, Guild +2, commons -1; Jasper's taken, and your customer comes back) or *I've seen no hedge-wright*.
3. **The contest** (Jasper, the next day, if you shielded him): a chest read in front of the Crossed Keys, given for a
   lucky coin. Read it exactly right and he takes his cart to Thornwick: the stake, commons +1, and his regulars come to
   you for a week (one more customer a day). Wrong, or not at all, and his cart stays.
4. **The farewell** (Jasper, that night, if you won): he owes you a favour.

### The Field's Dead (Brindle, 0.30; ends in Crownhold)

1. **The plough** (Garrick, from day 5): a knight's chest ploughed up on Brindle Field, his to sell by the finder's right.
   Bought, the best blade in it is set aside: it has a name on it.
2. **The knock** (Sir Aldwin Gale's ghost, that night): give the blade to his blood, the Gales of Crownhold. *I'll find
   your family*, or *finder's right* (its price; the Field's dead walk the crossroads at dusk for a week: one customer
   fewer a day).
3. **The Gales** (Sir Rowan, at Crownhold): his ancestor's blade. From then on the best weapon in Sir Rowan's chests is
   yours, not his (his heirloom rule is waived for you), and the Gales owe you a favour. Crown +1.
4. **The rest** (Sir Aldwin, that night): he lies down at last, and leaves a lucky coin (a curio).

### The Crossed Keys (Brindle, 0.30)

1. **The tab** (Bettany, from day 3): the Guild's doubled the inn's tithe and she's a purse short. *Lend it* or *I can't*.
2. **Repaid** (Bettany, four days on): the purse and half again, commons +1, and she owes you a favour.
   **Or sealed** (a Guild notice, two nights on): the Guild seals the Crossed Keys; Bettany goes north for the season and
   never comes to the counter (nor to anyone else's tale).

### The Harvest Fair (Brindle, 0.30)

1. **The prize** (Garrick, from day 6, while the shop's still at the stall): put up the reading contest's prize (half a
   purse, and two more customers on the fair's day) or not.
2. **The contest** (Garrick, the fair's day): the fair's chest, given for a coin; read it exactly right and the prize is
   yours (a purse if you put it up, half otherwise), commons +1, and Garrick owes you a favour.

### The Reeve's Election (Thornwick, 0.30)

1. **Caskett** (the Guild's timber factor): speak for him (Guild +1) or keep out of it.
2. **Martha Dunning** (a weaver, the next day): pay for her crier (half a purse; Guild -2 if you'd spoken for Caskett) or
   keep out of it.
3. **The result** (Hollis, two days on): Dunning wins only with your crier: market days, one more customer a day while
   you trade at Thornwick, the Wood +1. Otherwise Caskett wins: his tolls put a tenth on everything Thornwick's regulars
   sell you, for the rest of the season.

### The Goblin Bounty (Thornwick, 0.30)

1. **The bounty** (Rook): where does Nettle's clan burrow? *Tell her* or *goblins don't tell me*.
2. **The raid** (Rook, if told): half a purse, abbey +1, Wardens +1, and Nettle's clan goes over the hills: she never comes
   again, and nobody offers you goblin picks. **Or the plea** (Nettle, if not): *speak for her clan* (abbey -1; the
   abbey withdraws the bounty, Nettle charges you nothing for shaking from then on, and owes you a favour) or *stay out
   of it* (her clan goes to ground for a week).

### The Unopened (Thornwick, 0.30)

1. **The picket** (Sister Agathe): the abbey's zealots stand at your door: one customer fewer a day for ten days.
2. **The almoner** (Brother Cadoc): *alms for the abbey* (half a purse, abbey +1), *send for the Wardens* (Wardens +1,
   abbey -1), either ending the picket, or *let them stand*.
3. **The purse** (Agathe, night, if the picket's over): there was Guild silver in their alms box every week. *Keep it as
   proof* (a paper for the case; Guild -1) or *the poor box* (abbey +1).

### The Disputed Dowry (Thornwick, 0.30)

Whose the dowry is (the Ashbys' or the Corlews') is drawn when the tale begins.

1. **The court** (Aunt Hesper): let the ward decide. Each family will bring the dowry chest and offer to sell it.
2. **Mistress Ashby**, then **Hal Corlew** (a day apart): the same chest, offered by each. It's warm only in the hands of
   the family it belongs to; bought from the other, it won't open (the back room). (Nobody sends for the Wardens over a
   claim the court already has in hand.)
3. **The verdict** (Hesper): name the family. Right: the court's fee, commons +1, Hesper owes you a favour. Wrong: commons
   -1. Or *I couldn't say*.

### The Night Toll (Thornwick, 0.30)

1. **The ask** (Hollis): half his Wardens have the sweats; lend him your mark at the toll tonight (Wardens +1; Nim finds
   the back door shut) or *I've a shop to run* (Wardens -1).
2. **Midnight** (the toll): three of a cart's chests are cold (Corvin's, if he's still about). *Tell Hollis* (a finder's
   share, Wardens +2, Lockless -2, and Corvin's in the cells) or *say they're all warm* (Lockless +1, Corvin owes you).

### The Drowned Bell (Crownhold, 0.30)

1. **The bell** (Tilly): a bell under the Aldwater, ringing at night. *I believe you* or *the river's full of noises*.
2. **The dredge** (Tilly, two days on): a barge for a day, a purse. Finder's right: half each.
3. **Raised** (Tilly, two days on): the Keyhold's bell, and a chest chained to its clapper: yours, given freely, a colour
   finer than the shop's.
4. **The bell's crown** (the Hooded Woman, that night, soaked to the knees): it's the Keyhold's bell, and when the
   Locking was made they hid something in its clapper. It's empty: someone was there before them, long ago. She pays you
   for the trouble in Keyhold silver from the bell's crown (a purse). (Until 0.34 it held a shard of the Aldric Key.)
   At the season's end Tilly rings it for the ferry.

### The Moneylender (Crownhold, 0.30)

1. **The loan** (Mother Tansy): a purse now, a purse and a third on next week's first morning. On that morning her man
   takes it out of your purse; if it's not there, he takes the best thing on your shelf (or a curio) and her men stand at
   your door all week (one customer fewer a day).
2. **Better terms** (Tansy, six days on, if you paid): two purses at a quarter.

### The Duel (Crownhold, 0.30)

Who wins is drawn when the tale begins, unless Sir Rowan has his ancestor's blade (the Field's Dead): then he wins.

1. **The stakes** (Sir Rowan): hold both duellists' stakes in your back room.
2. **The bribe** (Lord Cassian, night): a purse for the stakes before dawn (Rowan leaves Crownhold in disgrace and never
   comes again; Crown -1), or *the stakes stay where they are*.
3. **Dawn** (Rowan): the winner takes the stakes and leaves you a tenth (half a purse); Rowan owes you a favour.

### The Archive Fire (Crownhold, 0.30)

1. **The fire** (night): someone's set the Royal Archive alight. *Run to help* (Crown +1; no Nim tonight) or *it's not
   my fire* (half the Roll burns: Master Quire is ruined and never trades with you again; Crown -1).
2. **The ledgers** (Quire, the next day): the fire began in a Guild lamp-oil cask; a fair copy of the Archive's ledgers of
   the Guild's seizures (a paper for the case), and Quire owes you a favour.

### The King's Court (every season, at the Emporium, 0.30)

1. **The summons** (Old Aldo, once you've two papers or more): bring them to his court in three days.
2. **The Guild's offer** (Mister Vane, night): two purses for the papers, and the Guild's friendship (Guild +3; he burns
   them in your grate), or *they're for the King* (Guild -2).
3. **The court** (Old Aldo): with four papers or more, the Guild is on trial and takes half its tithe while it's tried
   (Crown +3, Guild -3; the run scores for it, and the King stands with you at the treasury); with two or three, a
   decree takes a fifth off its tithe (if the Roll hasn't already). (Until 0.34 a trial also ended the run THE GUILD ON
   TRIAL; the run's end titles are the treasury's now.)

(The Maw's Bargain, 0.30 to 0.33, went with the Key in 0.34: the Maw's bargains are Vane's now, on the fourth and sixth
Guild nights once you remember, and its last offer is at the treasury: Bram's Account, above.)

### The Silent Choir (Brindle, 0.32; the Heartsong quest)

The season's hardest tale, and the only one whose prize is a Heartsong. Drawn like a Brindle tale (about one season in
three), while the songbook isn't full.

1. **The hum** (Sister Wren, from day 6): the last singer of the Keyhold's choir. When the Guild burned her chapel, the
   choir sang a Heartsong into three chests, each with a silver bell on its lock. Each holds an echo, freed only by reading
   its chest exactly; one wrong read and the song is lost. *I'll listen for them* or *I can't promise that*.
2. **The first chest** (Wren, Thornwick): a contest chest with a warded thing; Wren says what kind it sleeps in (a stated
   clue), so it's named the usual way, one more of its kind marked than you hear. That night, freed: Wren's Tuning Fork.
3. **The second chest** (Wren, Crownhold): its echo is under a false floor: drop it and listen for the hollow (glass may
   break, and a broken thing you counted is a wrong read).
4. **The Guild's chest** (Mister Vane, Crownhold): the Guild has the third, for two purses, or *the Guild can keep it*.
5. **The third chest** (Wren): another ward, another stated kind.
6. **The song** (Wren): she sings it whole and gives it to you in a chest, freely: a Heartsong, one the songbook doesn't
   know if there's one left, with the usual reveal (keep it or let it go; it's in the songbook either way).

A chest passed on, misread, or the Guild's price refused loses the song: Wren's farewell letter that night.

### The Brass Apprentice (Crownhold, 0.32)

1. **The marks** (Hollis, from day 16): chests opened and shut again all over Crownhold, each with a Keywright's mark
   burned into the lock-plate: yours, if you gave Orme your blood in The Unlocking (then refusing costs Wardens 2), or an
   old Keyhold mark. *I'll keep watch* or *not my business*.
2. **The yard** (Hollis, that night): a cloaked brass figure opens a lock with a marked palm. *Follow it*, or *raise the
   alarm* (the Wardens smash it: Wardens +2 and a finder's share).
3. **Tock** (the next day): Orme's automaton would rather be an apprentice than a burglar. *Take it in* (the Brass
   Apprentice: one more customer a day), *give it to the Wardens* (smashed: Wardens +2, a finder's share) or *take it
   home to Orme* (Arcanum +2).
4. **The Guild** (Mister Vane, night, at the Emporium, after a full day's trade there, so the move and the due haven't
   just emptied the purse): a machine that opens chests is the end of the Guild's licences.
   *Pay the fine* (two purses), *appeal to the King* (keeps Tock only if the Crown counts you a friend; Guild -1), or
   *hand Tock over* (Guild +2).

### The Sleeper (the Emporium, 0.32)

1. **The cellar** (Countess Vey, from day 22): a chest in the palace cellars breathes. Sir Edric Ashcombe locked
   himself in at the Locking to guard the Crown's oath-book; only his heir can give it away.
2. **The Roll** (Vey): the heir is Tilly Mudge, the mudlark.
3. **The heir** (Tilly): she'll give it, if she can watch him wake (and have his boots).
4. **The chest** (Tilly): given freely, so it opens.
5. **The waking** (Sir Edric, that night): the oath-book goes back to the King (Crown +2), and he swears to guard your
   door: Sir Edric's Oath.

## For the simulation and the bot

`Strategy.Choose` picks a beat's choice the way a careful player would: by what each is worth in gold over the rest of
the run (`ChoiceDef.Worth`), and when two are close, the kinder one. 0.30's worths: a favour is an eighth of a coming
due if a short night comes (`Strategy.FavourWorth`), a paper a quarter of what the court would take off (`PaperWorth`),
a customer a day what the shop would make of it (`CustomerDays`), a loan its share unless it saves the night
(`LoanWorth`). In Bram's Account (0.34) the careful player takes the bribe (the second Guild night's, since 0.36) only
if it's that or the shop (`Strategy.TakeBribe`), and the bargain (the third's) when the plan can't be whole this season
anyway and Bram's crate hasn't turned up (`TakeBargain`); buys a Guild crate when a working purse would be left (`BuyCrate`); keeps a mimic
while it could be useful this season and there's time to feed it full (`KeepMimic`); tends the back room every night
(`TendBackRoom`: Bram's gold eaten first, a freed guest's note paid); at the Lantern Market buys the night at the
weighhouse, Bram's crate and the bargemen with two working purses kept back, and never sells gold down the Gate
(`AtMarket`); and, once it remembers, stays away from the treasury unless the plan is whole (`StayAway`). On a short
Guild night the sim and the bot call in favours before pawning anything. It sends for the Wardens on a cold chest,
returns what it bought by mistake, accepts a tool offered at the counter when it would have bought it at the day's end,
and buys at Nim's table every night. A blind buyer pays for stolen chests and can't open them. (0.32: the careful player
never makes an offer on a chest that's gone cold in its hands, haggled or not; and it sends for the Wardens over a Guild
bailiff's seizure whatever the Lockless think, since that costs the Guild's goodwill, not theirs.) The sim's "Tales"
section reports how often each tale is drawn, finished and how it ended, and what it was worth; its careers report
(0.34) the run in which a career is first swallowed at the treasury, first brings Bram home, and first reaches the next
day.
