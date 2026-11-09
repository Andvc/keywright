# Curio audit B

> Written before 0.35's fixes, against 7be794f: it records what each card promised and what the game did then. Every finding here was then fixed in the game, or the card was corrected where its promise couldn't hold (see docs/design.md, "Every card does what it says"); the checks in this folder now all pass.

Group: ledger, truthglass, crate, hook, rod, log, pass, compass, wardlamp, pillow, wardtally, wardkey, aldricseal,
shell, coin, foot, wishbone, horseshoe, clover, star, spindle, lantern, queue, horn, guestbook, catalogue, dome.

Checks: `tools/curiotest/ChecksB.cs`, 74 of them, about five seconds:

    dotnet run --project tools/curiotest -- --only=ledger,truthglass,crate,hook,rod,log,pass,compass,wardlamp,pillow,wardtally,wardkey,aldricseal,shell,coin,foot,wishbone,horseshoe,clover,star,spindle,lantern,queue,horn,guestbook,catalogue,dome

Result at 7be794f (0.35 in progress): **50 of 74 checks passed, 24 failed**. Every failure is the game doing
something other than what the card (or the glossary line its tooltip adds, or the opening's line about it) says; none
is a harness problem. The whole harness at this commit: `51 of 75 checks passed, 24 failed.`

The finding most likely behind "some curios don't appear to behave as described" is X6: the opening's luck line
thanks whichever luck curio you hold first for every bit of luck in the chest, so with a Restorer's Bench a Lucky Coin,
Four-Leaf Clover, Horseshoe, Hollow Rod or Ward-Key "makes things come out better" on about three chests in ten it never
touched.

How: each card read as an English player reads it (the blurb with its constants filled in, and the glossary terms
`Glossary.For` adds to its tooltip); every effect site traced (Core, the sim, `Strategy`, the phases); each promise
tested against the Core rules with twins (same seed, same chest, with and without the curio), crafted chests for the
exact rules, fixed-seed counts for chances, and real day queues (`GameState.StartDay`) for crowd and chest rules. Chance
checks use `Close` (a relative tolerance): `T.Near`'s half-coin slack is for gold and lets any share between 0 and 1 pass.
The sim plays the same Core calls as the counter and the opening (`StartDay`, `NextCustomer`, `Buy`,
`SaleValue(item, grade, purchase)`, `Collect`, `Pass`), so where a curio fails in the game it fails in the sim the same
way; the two places the careful player's model differs are under "The sim" below.

## Verdicts

| key | verdict | in one line |
|---|---|---|
| ledger | MISMATCH (edge) | +15%/-15% as said, but a blank slip on a chest bought sight unseen is charged as a wrong slip |
| truthglass | OK | a perfect read: everything x1.5 (with the ledger, x1.725) |
| crate | OK | a dropped chest never breaks glass (0 of 600 potions) |
| hook | OK | every seller's false-bottom chance exactly doubled |
| rod | PARTIAL | a step better up to blue; a hidden purple thing (84% of a purple chest's) gets nothing; thanked for others' luck (X6) |
| log | OK | +2 points a false bottom, capped at +20 |
| pass | PARTIAL | x2 at the opening, at any shop; kept on the shelf and sold later, plain price (62g -> 24g) |
| compass | PARTIAL | every seller-rolled chest; not Pip's empty ones, tale chests, the Stranger's (about 1% of chests) |
| wardlamp | PARTIAL | sign sellers always ward; the Sleepers' Moon's dreams still point at the dearest thing 40% of the time |
| pillow | MISMATCH + bug | 20% of sellers with nothing else to say = 11-13% of all sellers; and a friend's waived terms are lost |
| wardtally | OK | warded things +10% a ward named (the first counts itself) |
| wardkey | PARTIAL | a step better up to blue; a purple warded thing gets nothing; thanked for others' luck (X6) |
| aldricseal | PARTIAL + bug | 8-15% of chests have no ward with a sign (Brass, Rook, the King, tales, event days); friend's terms lost |
| shell | OK | dry-spell luck from 5 plain finds, not 10, exactly |
| coin | MISMATCH (credit line) | the rule is right (first chest a day rolls twice); the opening thanks it for a restorer's luck on every later chest (X6) |
| foot | OK | +10 points of step chance (the "likelier" convention); purple shines; its luck line counts others' luck too (X6) |
| wishbone | PARTIAL | x1.5 on things above their colour; not on a purple thing luck made shine (33% of Emporium surprises), not when sold from the shelf |
| horseshoe | MISMATCH (credit line) | the rule is right (+10 points a chest in a row, to 50); the opening thanks it with no streak going (X6) |
| clover | MISMATCH (credit line) | the rule is right (the week's first chest, everything a step better); the opening thanks it all week (X6) |
| star | PARTIAL | never falls in a blue or purple chest (0 of 357 at Crownhold and the Emporium) |
| spindle | OK | +2 points a surprise, to 50 |
| lantern | OK | one more customer every day |
| queue | OK | first pass a day comes back last, at 80% of the first ask (rounded) |
| horn | OK | two more customers; price factor x1.05 (rounding leaves 39% of Thornwick's asks unchanged) |
| guestbook | PARTIAL | not finer when purple (45% of Emporium mornings), not on top of a storm or an exhibition's better chests, not when a tale's chest comes first |
| catalogue | OK | 4g x the shop's scale per new journal entry this run |
| dome | OK | one more display slot |

Counts: 13 OK, 9 PARTIAL, 5 MISMATCH (three of them only in the opening's credit line), 0 DEAD, 0 UI-ONLY.

## Findings that cross several cards

**X6. The opening thanks the wrong curio for luck.** `Build.Luck` (`Build.cs:276-285`) gives everything luck changed
in a chest to the first luck curio you hold, in the order clover, coin, (dip, sight unseen), spindle, foot, horseshoe,
wheel, rod, ward-key, whether or not that curio's rule was in force for this chest; only with none of them held is it
put down to "Your restorers". `Purchase.Luck` carries it to the opening, which shows it as "Four-Leaf Clover: 2 things
came out better!" (`OpeningPhase.cs:124`, `Curios.LuckLine`). With the Restorer's Bench (8% a thing, a Thornwick tool
from Cadoc) and rare chests of four, the opening said the coin, clover, rod, ward-key or horseshoe lifted things in 92-98
of 300 chests where it did nothing: the coin after the day's first chest, the clover after the week's first, the
horseshoe with no streak, the rod and ward-key on chests with no false bottom or ward. The rabbit's foot and the spindle
are thanked for the restorers' share as well as their own (and the spindle before any surprise); when the clover or
coin is held, the foot's lifts are put down to them. A player reads "the clover works every day". Fix, `Build.cs:279-282`:
thank only curios whose rule applied to this chest, e.g.

    var lucky = new List<string>();
    if (firstThisWeek) lucky.Add("clover");
    if (firstToday) lucky.Add("coin");
    if (unseen) lucky.Add("dip");
    if (Surprises > 0) lucky.Add("spindle");
    lucky.Add("foot");
    if (LuckStreak > 0) lucky.Add("horseshoe");
    if (chest.Everything.Any(i => i.Category == Category.Gems)) lucky.Add("wheel");
    if (chest.Hidden != null) lucky.Add("rod");
    if (unwarded != null) lucky.Add("wardkey");

and count, for the rod and ward-key, only the hidden or warded thing; whatever is left over is the restorers'. Checks:
"the opening thanks the ... only ..." under coin, clover, horseshoe, rod and wardkey.

**X1. A hidden or warded purple thing can't take luck's step.** `Build.Luck` lets luck's step past purple make a thing
shine (enchanted) instead of gold, but not a hidden one (`Build.cs:267`, `bool shone = shine && !hidden ...`), and
`Chest.Improve` strips enchantment off whatever's under a false bottom (`Chest.cs:234`); `Categories.Lift` stops at
purple (`Items.cs:144`). So a purple thing under a false bottom or in a ward gets nothing from a step, though the
glossary's "A step better" (`Glossary.cs:49-50`), shown under the rod, ward-key, clover, foot and star, says a purple
thing luck would lift "comes out enchanted instead". 84.0% of what a purple chest hides rolls purple or gold (6.1% in a
blue chest, 0.8% in a green one). It is the whole of the rod's and the ward-key's failure at the Emporium; at the edges it
touches the clover, foot, horseshoe, spindle and the dry spell on a false bottom. (Letting a hidden thing shine wouldn't
break the read rule: `Appraisal.Check` already reads a shine as `Lucky`, which counts as right, `Appraisal.cs:266`.)

**X2. What a chest's opening put on a thing is lost if it's kept on display and sold or pawned later.**
`GameState.SellFromDisplay` (`Shop.cs:905-909`) and `PawnValue` (`Run.cs:124`) price a displayed thing with
`SaleValue(item)`: no purchase, so `SaleRaw` (`Build.cs:406-418`) has no chest (not hidden, not warded, the chest's
colour taken as the thing's own, no read). Displayed things are also rebuilt plain when a save loads (`Shop.cs:610-611`).
Yet `Keep` (`Shop.cs:895-902`, called with the opening's price, `OpeningPhase.cs:1036, 1214`) charges the full opening
price as the price given up. The pass (x2 and the hidden x1.3: 62g at the opening, 24g off the shelf) and the wishbone
(x1.5: 36g -> 24g) have checks; the ledger, truthglass, ward tally, tuning fork and the Finder's name lose theirs the
same way. Fix: remember what the opening priced a kept thing at (a dictionary by `Item.Id`, saved in `SaveData`) and pay
that from the shelf and the pawnbroker; or carry the flags (`FromHidden`, `Warded`, the chest's colour) on the `Item`.

**X3. `Events.Restate` drops a friend's terms.** It rebuilds the visit (`Events.cs:180-183`) without `Waived` (or the
price note `ApplyGoodwill` wrote, `Consequences.cs:49-56`). A sleeper's pillow and the Aldric Seal restate visits every
morning (`Build.cs:546`; so do the counting beads, `Build.cs:547`, and the Sleepers' Moon, Wardens' Day and the Guild's
Count, `Run.cs:474`): Nettle, a friend of the clan, charges for shaking again; Sir Rowan, once Aldwin's home, claims the
best blade again. The price cut itself survives (the ask is copied). Fix: `Waived = visit.Waived` in the initializer at
`Events.cs:182`, and carry the price note (a small copy method on `Visit`, since `PriceNote` has a private setter).

**X4. The glossary's "A false bottom" says marking it makes the thing come out; the game always gives it.**
`Glossary.cs:59-60`, shown under the hook, rod, log, pass and compass (and the beads): "mark the false bottom on your
slip and what's under it comes out". In the code the thing under a false bottom is in `Chest.Everything` whatever the
slip says (`Chest.cs:70`), an unmarked one is read `MissedHidden` (`Appraisal.cs:280-281`), and the opening lands and
sells every read (`OpeningPhase.cs:223, 733, 794`); the log counts it as opened (`Build.cs:501-505`). Marking decides only
the perfect read. Fix the definition: "...lands with a hollow boom. Mark it on your slip for a perfect read: what's
under it comes out either way." (The log check "a false bottom left off the slip still comes out" pins the code's
behaviour.)

**X5. Rounding.** `Money.Round` (`Random.cs:31`) rounds asks over 30g to 5g. The horn's 5% leaves 39.4% of Thornwick's
asks unchanged (23.6% at Crownhold, 17.5% at the Emporium) while the crowd's asks rise 4.6-4.8% on average; the queue
ticket's 20% is exact on 29.8% of Thornwick asks (20.4% on average). Not a bug in either curio, but a player comparing
one ask with the last will see 0% or 10%.

## The sim and the careful player

- **S1.** Hesk's scale read (`Strategy.Weigh`, `Strategy.cs:190`) puts a false bottom's chance at
  `seller.HiddenChance + Boosts.Hidden`, ignoring the hook's `HiddenFactor` and the compass's `AlwaysHidden`;
  `GameState.HiddenChance` (`Kin.cs:133-138`) has the right number. With either curio the sim's Hesk under-reads false
  bottoms. Fix: use `state.HiddenChance(seller)` there.
- **S2.** With the compass every chest has a false bottom, but the careful player ticks one only after a hollow drop
  (`Hearing.cs:93`) or a count or a scale (`Strategy.Reckon`, `Strategy.cs:125-131`). When it doesn't drop (glass heard
  and no crate, Brass, a rushed seller, Hesk) it leaves the guaranteed false bottom off the slip and loses the perfect
  read, while `Strategy.Worth` still values it (`Strategy.cs:280-281`). The sim's builds report undervalues the compass
  with the reading curios. Fix: in `Reckon`, `if (state?.Boosts.AlwaysHidden == true && slip.Tier >= 2) slip.Hidden = true;`.

## Each curio

### ledger: Ledger of Certainty (rare, from Thornwick) -- MISMATCH (edge)

Card: "A perfect read: everything in the chest sells for 15% more. A wrong slip: 15% less." (Term: a perfect read.)

Where: `Build.Read` (`Build.cs:361-363`, the ledger at 362) multiplies every thing in a bought chest in `SaleRaw`
(`Build.cs:417`), reached through `SaleValue(item, grade, purchase)` (`Build.cs:402`) with `Purchase.Perfect`
(`Shop.cs:71` -> `Appraisal.Check`, `Appraisal.cs:51`). The opening prices each thing as it lands with it
(`OpeningPhase.cs:733`); the sim the same (`Simulation.cs:1009`). The slip assumes its read is right
(`Build.cs:365-366`, `Read(true)`). `Strategy.CurioWorth` 20 (`Strategy.cs:520`). Nothing on screen says the ledger
changed a price.

Works: a perfect read +15% on everything, the false bottom's thing and the named warded thing included; a missed thing,
or a thing marked that isn't there, -15%.

Fails: a chest bought sight unseen on a blank slip is charged as a wrong slip, everything 15% less (77g -> 65g in the
check). The opening's own rule is that a thing you didn't mark on a chest you never handled "is no miss"
(`OpeningPhase.cs:96-100`), and the card names only a perfect read and a wrong slip. Scenario: the Ledger and a Sealed
Bid; buy Pip's chest without touching it; every thing sells 15% under its price and nothing says why.

Fix: code, `Build.cs:402-403`: `from == null || (from.SightUnseen && from.Slip.Blank) ? null : from.Perfect` (the
tuning fork's streak still applies, the ledger and truthglass don't); or wording: "...15% more. Any other slip, even a
blank one: 15% less."

Notes: a perfect slip whose counted glass breaks when the lock is forced stops being perfect (`Shop.cs:70-71`, "once
it's open"), so the ledger then takes 15% off: the game's rule for every perfect-read payer, but a 30-point swing here.
X2 applies.

Checks: 4 (3 pass; "a chest bought sight unseen on a blank slip is no 'wrong slip'" fails).

### truthglass: Truthglass (legendary, from Crownhold) -- OK

Card: "A perfect read: everything in the chest sells for half as much again." (Term: a perfect read.)

Where: `Build.cs:363`, on the same paths as the ledger. `Strategy.CurioWorth` 22 (`Strategy.cs:521`).

A perfect read x1.5 on everything; any other slip x1 (no penalty); with the ledger x1.725. X2 and the forced-glass note
above apply.

Checks: 3 pass.

### crate: Padded Crate (common, from Thornwick) -- OK

Card: "Glass never breaks when you drop a chest."

Where: `GameState.DropSturdier` = 1 (`Shop.cs:399`), so `Chest.Drop`'s break chance is x0 (`Chest.cs:198-205`). The
counter (`CounterPhase.cs:1109`) and the sim (`Simulation.cs:641`) both drop with it; the careful player drops whatever's
inside once it has one (`Strategy.cs:79-80`). Nim never brings it to Hesk, who can't drop (`Kin.cs:196`).

0 of 600 potions broke in 300 drops (without it, about half). Forcing a lock still breaks glass (not promised).

Checks: 2 pass.

### hook: Smuggler's Hook (uncommon, from Thornwick) -- OK

Card: "Smugglers come your way: false bottoms turn up twice as often." (Term: a false bottom.)

Where: `HiddenFactor = 2` (`Shop.cs:385`), multiplying `seller.HiddenChance + boosts.Hidden` in `Market.Roll`
(`Market.cs:159`); the careful player's number is `GameState.HiddenChance` (`Kin.cs:133-138`, used by `Strategy.Worth`,
`Strategy.cs:281`).

Every seller's chance exactly doubles (one over 50% would cap at certain; none starts there); Ada's chests have one
twice as often over 6,000 rolls. Only chests a seller rolls (not a tale's, a lesson's or the Stranger's). S1 applies.

Checks: 2 pass.

### rod: Hollow Rod (uncommon, from Thornwick) -- PARTIAL

Card: "What's under a false bottom comes out a step better: people hid their best." (Terms: a false bottom, a step
better.)

Where: the shift for the hidden thing (`Build.cs:91`), applied in `Luck` (`Build.cs:260-268`); the slip's odds
(`Build.cs:238-240`); the opening credits it (`Build.cs:281`).

Works: green -> blue, blue -> purple, the rest of the chest unchanged; the slip's odds for a false bottom move a step.

Fails (X1): at the Emporium a purple thing under a false bottom stays purple and plain (248g with or without the rod in
the check). Five hidden things in six in a purple chest are purple, so at the Emporium the rod does nothing to most of
what it's for.

Fix: wording "...a step better (as far as purple)"; or code, X1: let a hidden thing shine when luck lifts it past purple
(drop `!hidden` at `Build.cs:267`, keep `Magic` for a `Shone` hidden thing at `Chest.cs:234`).

Also X6: held with no clover, coin, spindle, foot or horseshoe, the rod is thanked for a restorer's luck on any chest,
false bottom or not (98 of 300 plain chests with the Restorer's Bench).

Checks: 4 (2 pass; "at the Emporium ... still comes out a step better" and "the opening thanks the rod only for what
was under a false bottom" fail).

### log: Smuggler's Log (uncommon, from Thornwick) -- OK

Card: "Smugglers trust a buyer who keeps quiet: each false bottom you open this run makes false bottoms 2% likelier (up
to 20%)." (Term: a false bottom.)

Where: `Boosts.Hidden += min(0.2, 0.02 x BottomsOpened)` (`Shop.cs:379`) into `Market.Roll` (`Market.cs:159`);
`BottomsOpened++` for every bought chest with a false bottom (`Build.cs:501-505`). Saved.

+2 points of chance per false bottom (absolute, as every "likelier" card is), capped at +20 after ten; with the hook,
doubled. A false bottom left off the slip counts too: true to the code, where its thing comes out anyway, not to the
glossary (X4).

Checks: 2 pass.

### pass: Black Market Pass (rare, from Thornwick) -- PARTIAL

Card: "Thornwick's black market pays double for anything from under a false bottom." (Term: a false bottom.)

Where: `Special` (`Build.cs:351`) for `item == chest.Hidden` in `SaleRaw` (`Build.cs:413-415`); the slip's false bottom
doubles too (`Appraisal.HiddenValue`, `Appraisal.cs:180`, through `Value` -> `Special`). It pays at any shop, Thornwick
or further in.

Fails (X2): a thing from under a false bottom kept on display and later sold (or pawned on a short night) fetches its
plain price: 62g at the opening, 24g from the shelf (the x2 and the hidden x1.3 both gone), though keeping it cost 62g.

Fix: X2.

Checks: 3 (2 pass; "kept on the shelf, sold later, still fetches double" fails).

### compass: Smugglers' Compass (legendary, from Crownhold) -- PARTIAL

Card: "Every chest has a false bottom." (Term: a false bottom.)

Where: `AlwaysHidden` (`Shop.cs:389`) in `Market.Roll` (`Market.cs:159`); the careful player's chance is 1
(`Kin.cs:137`).

Works: every chest a seller rolls, for every seller (60 each).

Fails: chests made anywhere else. Pip's empty chests return before the false-bottom roll (`Market.cs:105`; 88 of 300 of
Pip's had none); tale chests (`TaleBook.Fill`/`Brought`/`Given`, `TaleBook.cs:69-91`: first.chest, field.plough...);
the Stranger's chained chest (`Events.cs:216-244`); lessons with tips on (`Lessons.cs:83-86`); mimics in a remembered
season (`Mimics.cs:75`). Over 60 days each at Crownhold and the Emporium, 22 of 843 chests had none; over 150 days with
the plan's events, about 1%.

Fix: roll the false bottom before the empty return at `Market.cs:105`; give `TaleBook.Fill` the state and add a hidden
thing when `s.Boosts.AlwaysHidden`; or word it "Every chest a seller brings you has a false bottom." S2 applies.

Checks: 3 (1 passes; "Pip's empty ones too" and "every chest a day brings" fail).

### wardlamp: Warden's Lantern (common, from Thornwick) -- PARTIAL

Card: "Every sign a seller leaves (a dream, a tag, a riddle, a card...) points at a warded thing." (Term: a warded
thing.)

Where: `AlwaysWard` (`Shop.cs:390`) in `Market.Roll` (`Market.cs:166`).

Works: Sorrel, Hesper, Ilse, Isaura, Quire, Skrae and Zell ward every time with it (200 of 200 each; about half
without), and the sign names the warded kind.

Fails: the Sleepers' Moon. Its dreams are made by `Events.Restate`, which wards only 60% of them (`Events.cs:176`,
`DreamWards`, `Events.cs:66`) and is never told about the lantern (`Run.cs:474`); the rest dream of the dearest thing,
which you can hear. 15 of 48 such dreams in the check.

Fix, `Run.cs:474`: `bool always = Boosts.AlwaysWard;` before the loop, then
`Events.Restate(Rng, Queue[i], e.Key, Tier, ward: e.Key == "dreaming" && always)` (not on the Guild's Count, whose tally
isn't a sign).

Checks: 2 (1 passes; "on the Sleepers' Moon" fails).

### pillow: Sleeper's Pillow (uncommon, from Thornwick) -- MISMATCH + bug

Card: "Sellers dream true for you: 20% of them have warded something, and tell you their dream." (Term: a warded
thing.)

Where: `GameState.Curiosities` (`Build.cs:538-549`), each morning (`Shop.cs:692`): a visit with no clue of its own, not a
lesson's, not sealed (`Build.cs:544`) dreams with chance `PillowShare`, through `Events.Restate(..., ward: true)`
(`Events.cs:171-184`). The doc comment at `Build.cs:534` still says "a third". `Strategy.CurioWorth` 7.

(a) The 20% is of sellers with nothing else to tell you. From Thornwick in about half the crowd already states something
(a weight, a ticket, a tag, a riddle, a roll, echoes, cards, a dowry, Sorrel's own dream, the King's game), so over 300
days each: 12.1% of Thornwick's sellers, 11.0% of Crownhold's and 13.0% of the Emporium's dreamt (20.5-21.5% of those
with nothing to say). Every pillow dream does have a ward and names it.

(b) X3: the restated visit loses a friend's waived terms (Nettle charges a friend of the clan for shaking again).

Fix: wording "Sellers with nothing else to tell you dream true: a fifth of them have warded something, and tell you their
dream." (or `PillowShare` about 0.4 to make it a fifth of everyone); and X3's fix.

Checks: 3 (1 passes; "20% of the sellers who come in" and "keeps the terms they give a friend" fail).

### wardtally: Wardbreaker's Tally (uncommon, from Thornwick) -- OK

Card: "Each warded thing you name this run makes warded things sell for 10% more." (Term: a warded thing.)

Where: `Special` (`Build.cs:352`), x(1 + 0.1 x `WardsNamed`); `WardsNamed++` at purchase (`Build.cs:509`, from
`Shop.Buy`), before the opening sells. Saved.

10% a ward named, added up, no cap; the first named ward already counts itself; the false bottom's thing and the rest
unchanged. Notes: the slip's model passes `warded: false` (`Build.cs:367`), so the odds bar shows neither the tally nor
the warded x1.3; X2.

Checks: 1 pass (three wards in turn).

### wardkey: Ward-Key (rare, from Thornwick) -- PARTIAL

Card: "A warded thing you name comes out a step better." (Terms: a warded thing, a step better.)

Where: the shift for the thing the ward gave up (`Build.cs:91`), known once `Chest.Open` has let it out (`Shop.cs:814`,
`Chest.cs:115-123`; `Build.cs:260-265`). The parameter comment at `Build.cs:84` says "two steps"; the card and the code
say one.

Works: green -> blue, blue -> purple, the rest unchanged.

Fails (X1): a purple warded thing in a purple chest gets nothing (248g with or without the key). Most Emporium wards.
And X6: held alone among the luck curios, the key is thanked for a restorer's luck on chests with no ward at all (98 of
300 with the Restorer's Bench).

Fix: as the rod; X6.

Checks: 3 (1 passes; "the opening thanks the ward-key only for a warded thing named" and "at the Emporium" fail).

### aldricseal: The Aldric Seal (legendary, from Crownhold) -- PARTIAL + bug

Card: "Every chest has a warded thing, and its seller tells you a sign for it." (Term: a warded thing.)

Where: `Curiosities` with every eligible visit dreaming (`Build.cs:540`, the same path as the pillow), and `AlwaysWard`
for sign sellers (`Shop.cs:390`). `Strategy.CurioWorth` 11.

Fails: over 60 days each at Crownhold and the Emporium, 113 of 845 chests had no ward with a sign (15.3% and 8.3% over
150 days with the plan's events): sellers who state something else are skipped (`Build.cs:544`: Brass's ticket x59,
Rook's scale x21; on the Guild's Count, everyone's tally), the King (`Events.cs:173`), tale chests (staged after the
seal's morning, `Shop.cs:692` against `701`), the Stranger (sealed), and the Sleepers' Moon's own dreams (warded 60% of
the time, `Run.cs:474`, before the seal sees the queue).

Bug: X3 (Nettle, a friend, charges for shaking once the seal gives him a sign).

Fix: wording "Every seller with nothing else to tell you has warded something, and tells you a sign for it."; the
wardlamp's fix at `Run.cs:474` (it covers the seal too); X3's fix.

Checks: 2 fail.

### shell: Lucky Shell (common) -- OK

Card: "A dry spell ends twice as soon: luck starts to turn after 5 plain finds, not 10." (Term: a dry spell.)

Where: `PityStart`/`PityChance` (`Run.cs:260-262`), `Pity` at purchase (`Run.cs:268-279`, from `Shop.cs:816`). Nim doesn't
bring it with the Charm boon, which does the same (`Kin.cs:201`).

The chance is (n - 5) x 3% (to 50%) against (n - 10) x 3%, exactly; after 8 plain finds a chest has something lifted 9%
of the time (never without it).

Checks: 2 pass.

### coin: Lucky Coin (uncommon) -- MISMATCH (the opening's credit line; the rule is right)

Card: "The first chest you buy each day: each thing in it rolls its quality twice and keeps the better roll."

Where: `TwoTries` on the day's first purchase (`Build.cs:92`, `firstToday = Today.Bought == 1` at `Build.cs:256`, counted
at `Shop.cs:800`); the second roll keeps the better (`Build.cs:138`); the slip (`Build.cs:72, 163-176`) and the power
index (`Build.cs:606`); its line at the opening (`Build.cs:279-284` -> `OpeningPhase.cs:124`).

Works: 7% of the things in a common chest beat their first roll (the second roll's odds), nothing comes out worse, the
day's second chest gets nothing from it.

Fails (X6): the opening says "Lucky Coin: N things came out better!" on every later chest of the day that a restorer
lifted (92 of 300 with the Restorer's Bench), as if the coin worked all day.

Fix: X6. Note: `Today.Bought` counts a stolen chest (`Shop.cs:800`, before its early return at `803-809`), so if the
day's first buy is a stolen chest (which never opens), a Guild crate or a mimic, the coin is spent on it. True to the
card's words; a player expects "the first chest you open".

Checks: 4 (3 pass; "the opening thanks the coin only for the day's first chest" fails).

### foot: Rabbit's Foot (uncommon) -- OK

Card: "Everything you open is 10% likelier to come out a step better than its chest." (Term: a step better.)

Where: `StepChance` +0.1 (`Build.cs:103`), rolled at `Build.cs:140`; the whole step chance is capped at 90%
(`Build.cs:92`).

"10% likelier" is a 10-point chance of being lifted a step (the convention of every luck card): a thing a step below
its colour is lifted only to its colour, one already better goes further, and the share that ends up better than its
chest rises by 7-9 points. Lifted past purple, a thing shines (10% of purple coins did). Its line at the opening counts
a restorer's lifts as the foot's, and is the clover's or the coin's when one of those is held (X6).

Checks: 2 pass.

### wishbone: Wishbone (uncommon) -- PARTIAL

Card: "Something that comes out better than its chest sells for half as much again." (Term: a surprise.)

Where: `General` (`Build.cs:317-318`): `quality > chest` only; in `SaleRaw` (`Build.cs:415`) and the slip's `Value`
(`Build.cs:367`).

Works: a thing above its chest's colour x1.5, on the slip too; at its colour, x1.

Fails: (a) a purple thing luck made shine, the glossary's step past purple and a surprise to the shop (`Build.cs:498`
counts it, feeding the spindle, the horseshoe and the feat), gets no half again (192g, not 288g): 11.0% of Crownhold's
surprises, 32.6% of the Emporium's. (The opening's surprise banner doesn't count it either, `OpeningPhase.cs:213`.)
(b) X2: kept on display and sold later, 36g -> 24g.

Fix: in `SaleRaw` pass whether the thing is a surprise, `item.Quality > chest || item.Shone`, to `General`; in `Value`,
count the shine share (`ShineShare`) at x1.5 as well; X2 for (b).

Checks: 4 (2 pass; "a purple thing luck made shine" and "kept on the shelf" fail).

### horseshoe: Horseshoe (uncommon, from Thornwick) -- MISMATCH (the opening's credit line; the rule is right)

Card: "Each chest in a row with a surprise makes the next one's things 10% likelier to come out a step better (up to
50%)." (Term: a surprise.)

Where: `StepChance` +min(0.5, 0.1 x `LuckStreak`) (`Build.cs:104`); the streak is counted after the chest's own luck
(`Build.cs:500`, `Shop.cs:815-817`), so it's the next chest's. Saved.

Works: +1 for each chest with a surprise (a shine too), back to 0 after one without; three in a row, 30% lifted. A
mimic or a crate ends the streak; a stolen chest doesn't count either way.

Fails (X6): with no streak going the horseshoe adds nothing, yet the opening says "Horseshoe: N things came out better!"
whenever a restorer lifted something (98 of 300 chests with the Restorer's Bench), so it seems to work without a streak.

Fix: X6.

Checks: 4 (3 pass; "the opening thanks the horseshoe only with a streak going" fails).

### clover: Four-Leaf Clover (rare, from Thornwick) -- MISMATCH (the opening's credit line; the rule is right)

Card: "The first chest you buy each week: everything in it comes out a step better." (Term: a step better.)

Where: `CloverPending` (`Build.cs:75`), the shift (`Build.cs:91`), spent on the week's first chest that's opened
(`Build.cs:256-257`); the power index (`Build.cs:607`). Saved (`CloverWeek`).

Works: everything lifts a step, the false bottom's thing and the named ward included; purple things shine; the week's
second chest gets nothing from it; the next week's first does.

Fails (X6): first in the thank-you order, the clover is named for every bit of luck in every chest all week: "Four-Leaf
Clover: N things came out better!" on 93 of 300 later chests with the Restorer's Bench (and for the foot's lifts too).
A player reads "the clover works on every chest".

Fix: X6. Notes: X1 (a hidden or warded purple thing can't step); a gold thing can't; a Guild crate or mimic bought first
spends it, a stolen chest doesn't (`Luck` never runs on one), unlike the coin.

Checks: 3 (2 pass; "the opening thanks the clover only for the week's first chest" fails).

### star: Shooting Star (rare, from Thornwick, once the debt's been cleared) -- PARTIAL

Card: "Once a day, the first thing to come out better than its chest climbs a step further." (Terms: a surprise, a step
better.)

Where: `Build.Luck` (`Build.cs:292-300`): once a day, the first thing with `Quality > chest && Quality < Rarity.Epic`
goes up a step.

Works: wood and green chests, once a day (rare -> purple in a green chest).

Fails: in a blue chest anything better is purple or gold, and in a purple chest gold or a shine, so the `< Rarity.Epic`
test (`Build.cs:295`) rules out every surprise in them and the star never falls there. With a day's first chest each
time (400 days, the shop's crowd, its restorers and a rabbit's foot): at Crownhold 0 of 136 blue or purple chests with a
surprise got it (63 of 67 wood or green did); at the Emporium 0 of 221 (25 of 26). The glossary says a purple thing luck
lifts comes out enchanted. Nim brings it only after a cleared run, when a player spends most days at Crownhold and the
Emporium.

Fix, `Build.cs:294-299`: take the first surprise that isn't gold (nor a hidden purple thing, X1); a purple one shines
(`i with { Magic = true, Shone = true }`, and add it to `_shone` so `Counted` counts it), anything else goes `Up()`.

Checks: 4 (1 passes; "in a blue chest" and "at Crownhold"/"at the Royal Emporium" fail).

### spindle: Fate's Spindle (legendary, from Crownhold) -- OK

Card: "Each surprise this run makes everything 2% likelier to come out a step better (up to 50%)." (Terms: a surprise,
a step better.)

Where: `StepChance` +min(0.5, 0.02 x `Surprises`) (`Build.cs:106`); `Surprises` counted at purchase (`Build.cs:498-499`).
Saved.

2 points a surprise, 50 from the 25th; after 10, 20% lifted. Notes: the whole step chance stops at 90% (`Build.cs:92`)
with enough luck stacked; its line at the opening also counts a restorer's lifts (X6).

Checks: 3 pass.

### lantern: Shop Lantern (uncommon) -- OK

Card: "Open late: one more customer every day."

Where: `CustomersPerDay` +1 (`Shop.cs:484`), filled by `StartDay` (`Shop.cs:666`); the sim plays the same day.

Exactly one more chest at the counter on each of 10 days.

Checks: 1 pass.

### queue: Queue Ticket (uncommon) -- OK

Card: "The first chest you pass each day comes back at the end of the day, asking 20% less."

Where: `Passed` (`Build.cs:484-492`): once a day, for a chest you pass (not a walkout, a Wardens' call, a lesson's or one
already back), `Visit.Again(0.8)` (`Visit.cs:111-120`) goes to the end of the queue; from `Shop.Pass` (`Shop.cs:924-929`),
which the counter (`CounterPhase.cs:1478`) and the sim (`Simulation.cs:982`) call.

Notes: the price is 80% of their first ask (anger, a shaking fee or the Maw's gaze don't come back with them) and the
least they'd take 80% of theirs, rounded (X5); "the end of the day" is the end of the queue when you pass (a Shepherd's
Pipe walkout can come after it).

Checks: 3 pass.

### horn: Barker's Horn (uncommon, from Thornwick) -- OK

Card: "Two more customers every day, but word's out: everyone asks 5% more."

Where: `CustomersPerDay` +2 (`Shop.cs:484`); `PriceFactorAt` x1.05 (`Build.cs:620-622`) into `Boosts.Dearer`
(`Shop.cs:382`) and every seller's estimate (`Market.cs:375`); the Stranger (`Run.cs:462`) and tale chests
(`TaleBook.cs:82-83`) use it too.

+2 every day; the price factor exactly x1.05; the crowd's asks 5% dearer on average. Notes: X5 (39% of Thornwick's asks
don't move after rounding, others move up to 10%); with tips on, a seller's first visit is a lesson priced with no boosts
(`Lessons.cs:83-86`), so it doesn't ask more (it's outside the power index too); a gift is still a coin.

Checks: 3 pass.

### guestbook: Guest Book (uncommon, from Thornwick) -- PARTIAL

Card: "The day's first customer is the better sort: their chest is a colour finer."

Where: the first visit `StartDay` makes is made with `BetterChests = 1` (`Shop.cs:679, 737`; a lesson's chest is
brightened, `Shop.cs:733`), and `Market.CreateVisit` adds one colour, never past purple (`Market.cs:70`).

Works: at Thornwick, 40 of 40 mornings a colour finer than the twin's.

Fails: (a) purple can't be finer: 45% of the Emporium's first customers (16% of Crownhold's) bring purple anyway; 20 of
40 Emporium mornings got nothing. (b) It replaces the day's chance of a finer chest instead of adding to it: on a storm
(every chest a colour finer) the first customer's is no finer than anyone's, and with an exhibition's better chests (up
to 40%) that share of mornings gets nothing extra. (c) Tales come first: on a week's first morning the apprentice's or
Tobin's gift chest goes to the front (`Tales.cs:444, 447`), and a tale's trading beat can replace the better sort
(`Tales.cs:458`).

Fix: give `Boosts` a separate finer step for the guest book, applied after the `BetterChests` roll (`Market.cs:70`,
`Shop.cs:737`); stage the apprentice's and Tobin's chests after the first regular, and leave the guest book's visit out of
the beats' replacements, when the guest book is kept; word the purple case: "...a colour finer (purple at the finest)".

Checks: 4 (1 passes; "at the Emporium too", "on a stormy day" and "with an apprentice" fail).

### catalogue: Collector's Catalogue (common) -- OK

Card: "Every new kind of find this run pays 4g (more in a grander shop)."

Where: `Collect` (`Shop.cs:873-889`) -> `Catalogue` (`Run.cs:505-511`), 4g x `ShopTiers.Scale` (4, 8, 16, 28g); from the
opening (`OpeningPhase.cs:730`) and the sim (`Simulation.cs:1042`).

"A kind of find" is a journal entry: each named thing (Barrow Silver and Moonsilver Marks are two), demon gold included,
broken glass and Heartsongs not. Known from an earlier run but new this run, it pays.

Checks: 2 pass.

### dome: Bell Jar (common, from Thornwick) -- OK

Card: "Room to display one more thing."

Where: `DisplaySlots` +1 while there's a display (Thornwick on, or Isolde's shelf) (`Shop.cs:333-334`); `Keep`
(`Shop.cs:895-902`); the day's end draws the slots (`DayEndPhase.cs:332-334`) and the counter's shelf however many are
kept (`CounterPhase.cs:463-473`).

Checks: 1 pass.

## Seeing the failures in the game

The checks are Core-level; none of these curios is UI-only. To watch one in a bot run (under Xvfb):

- star: `--curios=star,foot --start-tier=4 --days=6 --no-tutorial --shots=shots_star`: Emporium openings with a
  surprise never show the "Shooting Star" line.
- guestbook: `--curios=guestbook --start-tier=2 --event=storm --days=2 --no-tutorial --shots=shots_guest`: the first
  customer's chest is the storm's colour, like everyone's.
- X6: `--curios=clover --start-tier=3 --days=3 --no-tutorial --shots=shots_clover`: the `08_luck` captures of the second
  and third day's openings say "Four-Leaf Clover: ... came out better!" (the bench's luck), though the clover was spent
  on day 1's first chest. The same with `--curios=coin` after each day's first chest.
- rod / wardkey: `--curios=rod,compass --start-tier=4 --days=4 --no-tutorial --shots=shots_rod`: what comes out from under
  the false bottom of a purple chest is purple and plain (look at the thing, not the line: X6 can thank the rod anyway).
- pass / wishbone (X2): `--curios=pass,compass --start-tier=2 --days=8`, keep a false-bottom find on the shelf, then sell
  it from the shelf: the confirm shows the plain price.
