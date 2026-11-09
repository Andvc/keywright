# Curio audit A: 26 curios against their cards

> Written before 0.35's fixes, against 7be794f: it records what each card promised and what the game did then. Every finding here was then fixed in the game, or the card was corrected where its promise couldn't hold (see docs/design.md, "Every card does what it says"); the checks in this folder now all pass.

Group: monocle, draught, aether, chalk, ring, staff, sealed, dip, bond, wholesale, token, blindfold, signet, tongue,
knot, hourglass, bell, coffer, goldentongue, trumpet, watch, beads, seal, register, crescent, fork.

Checks: `tools/curiotest/ChecksA.cs` (69 checks; one or more per curio). Run them with

    dotnet run --project tools/curiotest -- --only=monocle,draught,aether,chalk,ring,staff,sealed,dip,bond,wholesale,token,blindfold,signet,tongue,knot,hourglass,bell,coffer,goldentongue,trumpet,watch,beads,seal,register,crescent,fork

Result (with ChecksExample's sealed check): **36 of 70 checks passed, 34 failed.** Every remaining failure is the
game's: three checks that failed because they were wrong were fixed (the bond's payout is paid to the 5g like every
sum over 30g; the fork's sale is rounded to the coin; a lesson check whose 9g ask could not show a 5% discount). Only
`scripts/` was read; nothing in it was changed.

Line numbers are at 7be794f (prototype/0.35). "Game" is the counter and opening (`scripts/Phases`), "sim" is
`scripts/Debug/Simulation.cs` and `scripts/Core/Strategy.cs`.

## Verdicts

| key | verdict | in a line |
|---|---|---|
| monocle | OK | `Witchlight` lit by it (Shop.cs:428); counter, ShakeBox and sim all read it. UI nit: the slip's sign calls it "Starlight". |
| draught | OK | Enchantment chance x3 (Shop.cs:386, Market.cs:135); measured x3.1. |
| aether | OK | +2 points a find, capped at 20 (Shop.cs:381); takes effect from the next morning's chests. |
| chalk | PARTIAL | Second enchanted thing is added; but two enchanted things of one kind can't be read perfectly (shared cause C5). |
| ring | OK | x4 in sales and on the slip (Build.cs:348). UI nit: the opening still shouts "ENCHANTED! x2". |
| staff | PARTIAL | Enchants everything at packing; 24% of its chests then can't be read perfectly and the slip undervalues them (C5). |
| sealed | PARTIAL | 15% off works, but handling leaves 15 of 104 asks off their full price (7 dearer) (C2); lessons exempt (C1). |
| dip | OK | +15% step chance sight unseen, false bottom included (Build.cs:90). |
| bond | OK | 40% of the shortfall, sight unseen only (Build.cs:524). Game and sim differ on kept mimics and crates (edge). |
| wholesale | PARTIAL | 4% a purchase works at large asks; at Thornwick 9 of 45 asks don't move (C2); lessons exempt (C1). |
| token | MISMATCH | At purple chests (the Emporium) it adds only bad surprises; elsewhere about x1.85; the opening never credits its good swings. |
| blindfold | PARTIAL | 30% off and the mending work; handling leaves 30 of 104 asks off their full price (41 with a sealed bid) (C2); lessons exempt (C1). |
| signet | PARTIAL | Works for market hagglers; a tale's haggler and a lesson's walk out on the first low offer (C3, C1). |
| tongue | PARTIAL | A fifth off every floor, tale chests included; not a lesson's floor (C1). |
| knot | MISMATCH | Counts a chest bought at a curio-reduced ask as "talked down", so it feeds itself (C4); its 1% steps vanish in rounding: after one, 0 of 45 Thornwick asks drop (C2); lessons exempt (C1). |
| hourglass | PARTIAL | 5%/pass up to 20% works; stall asks of 4-10g don't drop after a pass (C2); buying a stolen chest doesn't start it again; lessons exempt (C1). |
| bell | PARTIAL | Works when the last visitor is a customer; a tale's talk queued last takes the bell; a lesson last gets nothing (C1). |
| coffer | MISMATCH | Pays 40% of curio discounts as if talked down, nothing for talking down from a raised price (C4); "everyone" isn't crooks, the King, the Maw or a lesson (C1). |
| goldentongue | PARTIAL | Market sellers never walk out; tale and lesson hagglers do (C3, C1); "everyone" isn't crooks, the King or the Maw. |
| trumpet | PARTIAL | Clank and straw both lifted (Build.cs:454); not on a lesson's chest (C1). Sim/bot then shake only hard and miss sand. |
| watch | OK | Patience and rot limit x2 (Shop.cs:418, Kin.cs:144), shown in Bram's note. |
| beads | PARTIAL | Every market chest with nothing said gets the tally; tale chests (and the weekly gift chests) never do (C3). |
| seal | OK | 8% of what the chest held on a perfect read (Build.cs:441). |
| register | MISMATCH | Off by one: "for each one so far" excludes the read being paid, so the run's first perfect read pays nothing. |
| crescent | MISMATCH | The dry spell's pity still lifts things a step (Run.cs:268); sellers ask 5.3-9.4% less, not 5%. |
| fork | PARTIAL | +3%/read, cap 30%, reset on a wrong read all correct; a Guild crate or a mimic (no read) breaks the streak in the game. |

OK: 8 (monocle, draught, aether, ring, dip, bond, watch, seal). PARTIAL: 13 (chalk, staff, sealed, wholesale,
blindfold, signet, tongue, hourglass, bell, goldentongue, trumpet, beads, fork). MISMATCH: 5 (token, knot, coffer,
register, crescent). DEAD: none. UI-ONLY: none (monocle and ring have a UI-only text nit).

## Shared causes

Most failures trace to five causes; fixing these fixes most of the table.

### C1. A seller's first visit (a lesson, with tips on) skips the build

`GameState.Greet` returns before doing anything for a lesson (Build.cs:453), so a lesson's chest gets no ear trumpet
(Build.cs:454 is after the return), no Waiting Game, Wholesaler's Tally, Haggler's Knot, Closing Bell, Sealed Bid or
Blindfold price. A lesson's visit is priced without the shop's boosts (Lessons.cs:83-87 passes no `boosts`), so it
isn't converted to haggling (coffer, goldentongue), gets no floor curio (signet, tongue, goldentongue) and no price
factor (crescent's 5%); and `HearsOut`/`Unruffled` are set only for market visits (Shop.cs:740-741, after the lesson
branch at Shop.cs:731-736). Kit with tips on meets a lesson for every new seller at every shop, so a player with these
curios sees them "not work" on that seller's first visit.

Failing checks: sealed, blindfold, hourglass, wholesale, knot, bell ("...first visit (a lesson, with tips on)..."),
signet, tongue, coffer, goldentongue, trumpet. Example: Garrick's lesson chest asks 9g; with the Closing Bell, as the
day's last, it should ask 7g and asks 9g. Rowan's lesson (95g) with the Blindfold should ask 65g sight unseen, asks 95g.

Fix: either apply the build to lessons (set `Chest.Unmasked` before the lesson check in `Greet`; drop the lesson check
for the price curios, since `Reduce` scales the lesson's scripted floor with its ask, and Garrick's "offer about half"
stays teachable; set `HearsOut`/`Unruffled` in `NewVisit`'s lesson branch too; price lessons with the shop's
`Boosts with { Dearer = 1 }` so everyone haggles), or say so where curios are explained (a line in the lesson's first
step or in `Glossary`: "While Bram teaches, curios rest"). The sim's lesson test (`--sim=sellers`) should be rerun after.

### C2. Every price is rounded after every step, which swallows a small discount

`Visit.Reduce`/`Raise` (Visit.cs:204-209, 303-308) round the ask and the floor with `Money.Round` (Random.cs:31: to
the coin up to 30g, to the 5g above, with `MathF.Round`'s round-half-to-even) after each curio's factor. A discount
smaller than half a rounding step disappears while the note under the chest still says it ("Haggler's Knot: 1% off."),
and the sight-unseen price can't be undone exactly (`Handle` divides the rounded price, Visit.cs:160).

Measured over the asks each shop's crowd makes (3000 visits a shop):

| promised | stall: asks unchanged | Thornwick | Crownhold | Emporium |
|---|---|---|---|---|
| 1% (knot's step) | 100% | 100% | 91% | 70% |
| 4% (wholesale's step) | 40% | 42% | 26% | 15% |
| 5% (hourglass's step) | 30% | 31% | 17% | 11% |
| 15% (sealed) | 9% | 2% | 2% | 1% |

Failing checks: knot ("at Thornwick..., a few chests talked down": 17 of 45 asks don't move after three; after one, 0 of
45 move), wholesale (9 of 45 Thornwick asks), hourglass (stall asks 4-10g), sealed and blindfold round trips (handled,
15 of 104 asks aren't their full price, 7 of them dearer: 50g becomes 45g, 115g becomes 120g; blindfold 30 of 104;
both 41 of 104, a 1-coin chest becomes 2 coins).

Fix: keep the exact price in `Visit` (`float _ask, _floor`), multiply factors into it, and show and charge
`Money.Round(_ask)`; `Handle` then restores the exact pre-seal price. For curio discounts to show at all on small asks,
round a reduced price to the coin rather than the 5g (a 38g price is fine), or make a curio's discount take at least one
rounding step off.

### C3. Tale chests miss part of the build

A tale's chest is priced by `TaleBook.Brought` (TaleBook.cs:82-83) with the shop's boosts (so haggling, floors and the
price factor apply) but never gets `HearsOut`/`Unruffled` (set only in `GameState.NewVisit`, Shop.cs:740-741; `Tales.Stage`,
Tales.cs:455-456, doesn't set them). And `Curiosities()` (the beads' tally, Build.cs:538) runs at Shop.cs:692, before
`StageTales()` at Shop.cs:701, which swaps a regular for the tale's chest (Tales.cs:458) and inserts the weekly gift
chests (Tales.cs:444, 447).

Failing checks: signet and goldentongue ("a haggler bringing a tale's chest..." walks out on a 1-coin offer), beads
("a tale's chest..." 12 of 12 untallied: `first.chest`, Garrick's spring chest).

Fix: set `HearsOut`/`Unruffled` in one helper called by `NewVisit` and `Tales.Stage`; run `Curiosities()` after
`StageTales()`, and make `Events.Restate` (Events.cs:171-184) set `Clue`/`ClueKinds`/`Says` on the visit it's given (they
have setters) instead of building a new `Visit`, which would drop `Beat`, `Gift`, `Contest` and `Prize`.

### C4. "Talked down" is measured from the seller's first ask

`Rebate` (Build.cs:517-518) and the knot's count, `Counted` (Build.cs:512), compare the price paid with `Visit.FirstAsk`,
the ask before anything changed it. So (a) a haggler's chest bought at an ask a curio lowered (Waiting Game, Wholesaler's
Tally, the knot itself, the bell, a sealed bid) counts as talked down: the purse rebates 40% of the curio's discount
and the knot counts a bargain; and (b) talking down from a raised price (angered by a drop, the Maw's gaze, a goblin's
shake fee) counts for nothing unless you go below the first ask.

Failing checks: coffer ("nothing back for a chest bought at the price asked": 8g back on Garrick's 400g ask, 380g after
one pass, no offer made), coffer ("talked down from a price that went up": Brother Cadoc angered from 400g to 600g,
talked down to 500g, nothing back; 40g expected), knot ("a chest bought at its asking price wasn't talked down":
bargains 1 -> 2 after buying at the knot-reduced 495g ask, so the knot feeds itself). The same count opens the Golden
Tongue (`Feat.Bargains`), so its unlock is inflated too.

Fix: record the ask on the table when the first offer is made (`Visit.AskBeforeOffer`), and use
`OffersMade > 0 ? AskBeforeOffer - PricePaid : 0` in `Rebate` and `Counted` (keep `Trade.Offer` counting as a bargain).

### C5. The slip marks one enchanted thing per kind

`Appraisal` keeps a single magic flag per kind (Appraisal.cs:63, `ToggleMagic` at 164, `ValueOf` at 176), and `Check`
gives `RightMagic` to one enchanted thing of a kind and `MissedMagic` to the next (Appraisal.cs:267-269). A chest with two
enchanted things of one kind can't be read perfectly, not even by the Bookkeeping Engine's exact record
(`Appraisal.Recorded`), and the slip and odds bar value only one of them as enchanted. Without curios that's 2 of 392
enchanted chests at Crownhold; with the chalk 13 of 387; with the staff 95 of 392 (24%).

Failing checks: chalk and staff ("...can be read perfectly"): `Coins RightMagic, Coins MissedMagic`.

Fix: store an enchanted count per kind (0..Count), cycle it in `ToggleMagic`, give that many `RightMagic` in `Check`,
value that many enchanted in `ValueOf`/`Work`, and have `Recorded` set the real count. (The slip's row needs a way to
show "2 enchanted".)

## Per curio

### monocle: Witch's Monocle (common, Crownhold)
Card: "Enchanted things glow when they sound, witchlight or not."
Where: `GameState.Witchlight` (Shop.cs:428); counter `_lamp = State.Witchlight` (CounterPhase.cs:125-126) into
`ShakeBox.Witchlight` (ShakeBox.cs:377, the glowing word); sim `Listen` (Simulation.cs:621); Nim never brings it with the
lamp or the Bottled Star (`NoUse`, Kin.cs:198).
Verdict: OK. UI nit: the slip's sign (CounterPhase.cs:642) is labelled "Starlight" when the witchlight comes from the
monocle; seen when the lodestone is owned too (two signs show short names), i.e. at the Emporium without the lamp.
Fix: `State.Has("lamp") ? "Witchlight" : State.HasCurio("monocle") ? "Monocle" : "Starlight"`.
Sim note: only the "+ light and lodestone" styles look (`lamp = tools && state.Witchlight`); the "every strength, drops"
style ignores a monocle it owns (minor).
Checks: "without the witchlight lamp, enchanted things glow when they sound": ok.

### draught: Witch's Draught (uncommon, Crownhold)
Card: "Enchanted things turn up three times as often."
Where: `Boosts.MagicFactor` (Shop.cs:386) multiplies each thing's enchantment chance (Market.cs:135; enchantment exists
only from Crownhold, its MinTier).
Verdict: OK. Rowan's chests at Crownhold: 5.1% of things enchanted, 15.8% with it (x3.1).
Checks: "enchanted things turn up three times as often (Crownhold and the Emporium)": ok.

### aether: Aether Jar (uncommon, Crownhold)
Card: "Each enchanted thing you find this run makes everything 2% likelier to be enchanted (up to 20%)."
Where: `Boosts.Magic += min(0.2, 0.02 * MagicFound)` (Shop.cs:381); `MagicFound` counts enchanted things opened
(`Acquire`, Shop.cs:449, via `Collect` in the opening and the sim), shone ones included.
Verdict: OK (2 points added to the chance, the game's usual "likelier"; tripled by the draught). Note: a day's chests
are packed at dawn, so today's finds count from tomorrow.
Checks: "each enchanted thing found this run: everything 2% likelier..., up to 20%": ok (0.00/0.06/0.12/0.18/0.20 after
0/3/6/9/12; a 5% seller's things 15% enchanted after five finds).

### chalk: Enchanter's Chalk (uncommon, Crownhold)
Card: "An enchantment spreads: a chest with an enchanted thing in it has a second one."
Where: `Boosts.MagicSpreads` (Shop.cs:391), Market.cs:146-155 (one plain real thing made enchanted).
Verdict: PARTIAL. Every market chest with an enchanted thing and another real thing has two (3000 visits). But (C5)
when the second is of the first one's kind, no slip can read the chest perfectly. Wording: a chest of a single thing
can't have a second; lesson and tale chests are never enchanted, so they're not affected.
Checks: "a chest with an enchanted thing in it has a second one": ok. "the chalk's two enchanted things of one kind can
be read perfectly": FAIL (`Coins RightMagic, Coins MissedMagic`).

### ring: Spellbinder's Ring (rare, Crownhold)
Card: "Enchanted things are worth four times as much, not twice."
Where: `Special` (Build.cs:348) multiplies enchanted things by `RingMagic / MagicMultiplier` (4/2) wherever they're
sold (`SaleRaw`, the slip's `Value`).
Verdict: OK (x4 sold, on the slip and out of a chest just bought). UI nit: the opening's float text is always
"ENCHANTED! x2" (OpeningPhase.cs:717); the value under it is right. Fix: `HasCurio("ring") ? "ENCHANTED! x4" : ...`.
To see it: `--start-tier=3 --curios=ring,staff,draught --days=4 --shots=shots_ring` and look at an opening with an
enchanted thing (moments `08_reveal`/`08_results`).
Checks: "enchanted things are worth four times as much, not twice (sold, and on the slip)": ok.

### staff: Archmage's Staff (legendary, Crownhold)
Card: "Everything in a chest with an enchanted thing in it is enchanted."
Where: `Boosts.MagicAll` (Shop.cs:392), Market.cs:149 (every real thing in a market chest with an enchanted one).
Verdict: PARTIAL. Every enchanted market chest is enchanted throughout (392 of 392). But (C5) 24% of them hold two
enchanted things of one kind, and those can never be read perfectly (the Engine's exact record reads `Coins RightMagic,
Coins MissedMagic`), so the staff fights the perfect-read curios and the slip undervalues its chests. Wording: a false
bottom's thing is never enchanted (by the hidden-thing rule), and a purple thing that luck makes shine at purchase
doesn't spread; "everything you can hear" would be exact.
Checks: "everything in a chest with an enchanted thing in it is enchanted": ok. "a chest the staff enchanted throughout
can be read perfectly": FAIL.

### sealed: Sealed Bid (common, the stall)
Card: "Buy a chest sight unseen and its seller knocks 15% off."
Where: `Greet` seals the price (Build.cs:471, `UnseenPrice` 475); `Visit.Seal`/`Handle` (Visit.cs:143-164); the counter
calls `Handle` on a shake, a drop or the lodestone (CounterPhase.cs:367, 1094, 1239 into 1204-1223); the sim's careful
player always handles (Simulation.cs:542); the "sight unseen" style doesn't (Simulation.cs:44-45).
Verdict: PARTIAL. 15% off ask and floor, offer sellers included, and back up when handled. But (C2) 15 of 104 asks are
not their full price after handling, 7 of them dearer (3g->4g, 50g->45g, 115g->120g); and (C1) no discount on a
lesson's chest. Fix: C2's exact price (or remember the pre-seal ask/floor in `Seal` and restore them in `Handle` when
nothing else changed them), and C1.
Checks: ChecksExample's "15% less; handled, the full price": ok. "a seller who waits for an offer takes 15% less...":
ok. "handled, any ask comes back to exactly the full price": FAIL. "a seller's first visit (a lesson)...": FAIL (9g, not 8g).

### dip: Lucky Dip (uncommon, the stall)
Card: "Everything in a chest you take sight unseen is 15% likelier to come out a step better."
Where: `Rules` adds `DipLuck` to the step chance when `unseen` (Build.cs:90), from `Buy` -> `Luck(chest, !visit.Handled)`
(Shop.cs:815); the opening thanks it (Build.cs:280).
Verdict: OK. Each thing (the false bottom's too) gets a 15% chance of a step, sight unseen only. Note: the step is from
the thing's rolled quality, so a thing rolled a colour worse only comes back to its chest's colour; the share of
surprises rises 13.9 points at common chests, 11.4-12.5 at green to purple (purple ones shine). The slip's odds
(Build.cs:239) never include it (unseen is false there), which is moot: a sight-unseen slip is blank.
Checks: "sight unseen, each thing is 15% likelier...; handled, no likelier": ok. "the false bottom's thing...": ok.

### bond: Underwriter's Bond (uncommon, Thornwick)
Card: "A chest bought sight unseen holds less than you paid? The Underwriters pay 40% of it back."
Where: `Insure` (Build.cs:524-531), called by the opening (OpeningPhase.cs:890) and the sim (Simulation.cs:1103).
Verdict: OK (40% of the shortfall, paid to the 5g like any sum over 30g; nothing when handled or when it held more). The
doc comment says "half the difference"; the constant and card say 40%. Game/sim edges: a mimic kept in the back room
adds nothing to the game's `held` (OpeningPhase.cs:404-416) but its price to the sim's (Simulation.cs:1082-1098), so
only the game pays the bond on it; the game insures a Guild crate bought sight unseen (40% of its price), the sim never
opens one (Simulation.cs:944-958).
Checks: "sight unseen and it held less than you paid: 40%...": ok. "nothing back for a chest you handled, or one that
held more": ok.

### wholesale: Wholesaler's Tally (uncommon, Thornwick)
Card: "Each chest you buy makes every seller after it today ask 4% less (up to 20%)."
Where: `Greet` (Build.cs:460-464), on `Today.Bought` (Shop.cs:800; stolen chests, crates and gifts count).
Verdict: PARTIAL. Exact at 500g (480, 460 ... 400, then the next day 500). But (C2) after one purchase 9 of 45
Thornwick asks don't move (10-12g, 35-60g: 40g * 0.96 = 38.4, rounded back to 40); (C1) lessons exempt (Nettle's 30g
lesson asks 30g, not 24g, after five purchases).
Checks: "each chest bought...4% less, up to 20%; tomorrow...": ok. "at Thornwick..., one chest bought takes something
off every ask": FAIL. "a seller's first visit (a lesson)...": FAIL.

### token: Gambler's Token (uncommon, the stall)
Card: "Twice as many surprises in everything you open: good ones and bad ones."
Where: `Rules.Swing` (Build.cs:92); `Turn` swings a thing at its chest's colour 25% of the time (Build.cs:141, via
`Categories.Swing`, Items.cs:222-228); the odds version (Build.cs:189-195); the opening's line (Build.cs:286).
Verdict: MISMATCH.
- Purple chests (the Emporium's): a good swing goes through `Lift`, which stops at purple (Items.cs:144-145, 228), and
  the swing doesn't shine the way luck's other steps do (`Lucky` isn't used at Build.cs:141). So the token adds only bad
  surprises there: good 0.64% -> 0.64% (x1.0), bad 16.0% -> 30.4%. By the glossary's "a step better" ("a purple thing it
  would lift comes out enchanted instead") it should shine.
- Elsewhere it's about x1.85, not x2: green good x1.83, bad x1.84; blue x1.94 and x1.84; common x1.98 (a common thing
  can't be worse). `TokenSwing` 0.25; about 0.3 would double.
- The opening counts only bad swings: "Gambler's Token: n swung!" (Build.cs:286); a good swing is credited to nobody, or
  to "Your restorers" when you own them (42 better and 111 worse swung; 111 credited).
Fix: swing up through `Lucky` at Build.cs:141 (and move the purple share of an upward swing into `shone` at 189-195);
count `i.Quality != before` for the token's line (or mark which things the swing moved); tune `TokenSwing` if "twice"
should be exact.
Checks: "green and blue chests: twice as many surprises": ok (within 15%). "purple chests...: twice as many good
surprises too": FAIL (77 -> 77 of 12000). "the opening credits the token with what it swung better": FAIL.

### blindfold: Fortune's Blindfold (legendary, Crownhold)
Card: "Sight unseen, a chest costs 30% less, and nothing in it comes out worse than promised."
Where: `UnseenPrice` (Build.cs:475) and `Greet`'s seal (Build.cs:471); `Rules.Mend` when unseen (Build.cs:93), applied
after the token's swing in `Turn` (Build.cs:142), to everything including the false bottom's thing.
Verdict: PARTIAL. 30% off (100g -> 70g, back to 100g when handled), and nothing came out worse in 8000 things sight
unseen even with the token swinging (handled, some did). But (C2) handling leaves 30 of 104 asks off their full price
(15 dearer), 41 of 104 with a sealed bid; (C1) Rowan's 95g lesson costs 95g sight unseen, not 65g. Note: fakes and
broken glass aren't "mended" (they're not a quality).
Checks: "sight unseen, a chest costs 30% less; handled, the full price": ok. "nothing comes out worse than promised":
ok. "handled, any ask comes back to exactly the full price": FAIL. "a seller's first visit (a lesson)...": FAIL.

### signet: Merchant's Signet (common, the stall)
Card: "Hagglers hear you out (an offer too low gets a second try), and come down 10% further."
Where: `HearsOut` (Shop.cs:740) read by `Visit.Offer` (Visit.cs:244-248); `Boosts.Floor` x0.9 (Shop.cs:387) on haggle and
offer floors (Market.cs:418).
Verdict: PARTIAL. Market hagglers: a second try, then a walkout on a second insult; floors 10% lower (tale chests too).
But (C3) a haggler bringing a tale's chest walks out on the first 1-coin offer, and (C1) so does Garrick in his lesson.
Checks: "a haggler hears out an offer too low...": ok. "hagglers come down 10% further": ok. "a haggler bringing a
tale's chest hears you out too": FAIL. "a haggler's first visit (a lesson)...": FAIL.

### tongue: Silver Tongue (uncommon, the stall)
Card: "Hagglers come down a fifth further, and those who wait for an offer take a fifth less."
Where: `Boosts.Floor` x0.8 (Shop.cs:387), Market.cs:418.
Verdict: PARTIAL. Haggle and offer floors a fifth lower, a tale's haggler too. (C1) a lesson's floor is scripted
(Garrick's: 45% of his ask, Lessons.cs:112) and doesn't move: 4g with or without it.
Checks: "hagglers come down a fifth further, and those who wait for an offer take a fifth less": ok. "a haggler's first
visit (a lesson)...": FAIL.

### knot: Haggler's Knot (uncommon, Thornwick)
Card: "Each chest you talk down this run makes every seller ask 1% less (up to 8%)."
Where: `Greet` (Build.cs:465-469) on `Bargains` (Build.cs:512-513).
Verdict: MISMATCH.
- (C4) A haggler's chest bought at its asking price counts as talked down when a curio lowered that price, the knot
  included: after one real bargain, every haggler asks 1% less, so buying any of them at their ask adds a bargain, and
  the next asks 2% less (bargains 1 -> 2 in the check).
- (C2) Its 1% steps are below the 5g rounding step at nearly every ask it meets: after one bargain, none of 45 Thornwick
  asks (10-150g) go down; after three, 17 of 45 still don't; 91% of Crownhold asks ignore a 1% step.
- (C1) A lesson's ask ignores it (Nettle's 30g lesson after eight bargains: 30g, not 28g).
Checks: "each chest talked down this run makes every seller ask 1% less, up to 8%": ok (at 500g). "a chest bought at
its asking price wasn't talked down...": FAIL. "at Thornwick..., a few chests talked down take something off every
ask": FAIL. "a seller's first visit (a lesson)...": FAIL.

### hourglass: Waiting Game (uncommon, the stall)
Card: "Every chest you pass makes the next seller ask 5% less (up to 20%). Buying starts it again."
Where: `Greet` (Build.cs:455-459) on `PassStreak`, counted by `Passed` (Build.cs:486) from `GameState.Pass` (pass,
walkout, the Wardens) and reset by `Counted` (Build.cs:497).
Verdict: PARTIAL. 5/10/15/20/20% after 1-5 passes; reset after buying. But: a stolen chest's purchase returns from `Buy`
before `Counted` (Shop.cs:803-809), so buying one doesn't start it again (the next seller still 5% off); (C2) at the stall
asks of 4-10g don't drop after one pass (10g * 0.95 = 9.5, rounded to even: 10); (C1) Garrick's lesson after four passes
asks 9g, not 7g. Fix: reset `PassStreak` on the stolen path too.
Checks: "every chest passed...; buying starts it again": ok. "buying a chest that wasn't its seller's to sell starts it
again too": FAIL. "at the stall, one chest passed takes something off every ask": FAIL. "a seller's first visit (a
lesson)...": FAIL.

### bell: Closing Bell (uncommon, the stall)
Card: "The last customer of the day asks 25% less."
Where: `Greet` when `Queue.Count == 0` (Build.cs:470).
Verdict: PARTIAL. The day's last customer asks 25% less, anyone before them their full price. But a tale's talk or a
tool's maker is queued at a random place, last included (Tales.cs:462, 536), and then the last customer with a chest has
someone behind them and gets nothing, while the talk "gets" the bell; (C1) a lesson last gets nothing (9g, not 7g).
Fix: `Queue.All(v => v.Talk != null)` instead of `Queue.Count == 0`; C1.
Checks: "the day's last customer asks 25% less...": ok. "...even when someone comes to talk after them": FAIL. "...when
it's a seller's first visit (a lesson)": FAIL.

### coffer: Haggler's Purse (rare, Thornwick)
Card: "Everyone haggles with you, and 40% of whatever you talk them down by comes back to you as well."
Where: `Boosts.Haggles` (Shop.cs:383) converts a fixed price to haggling (Market.cs:379); `HearsOut` (Shop.cs:740);
`Rebate` (Build.cs:517-518) paid in `Buy` (Shop.cs:810-812) and shown by the opening (OpeningPhase.cs:967).
Verdict: MISMATCH.
- (C4) 40% of a curio's discount comes back as if talked down: Garrick's 400g at 380g after one pass (Waiting Game),
  bought at that ask with no offer: 8g back. With the purse, every fixed seller is a haggler, so every sealed bid, bell,
  wholesale and knot discount is rebated.
- (C4) Talked down from a raised price, nothing: Cadoc, angered from 400g to 600g, taken to 500g: 0g back (40g by the
  card). Debatable if anger is meant to be a penalty, but the card says "whatever you talk them down by".
- "Everyone" isn't: crooks (Gilt), the King and the Maw keep a price to take or leave (Market.cs:379, as with Rosa), and
  (C1) a lesson's seller keeps a fixed price (Nettle's lesson). Fix the card ("Everyone who'd name a price, but crooks,
  the King and the Maw...") or the rule.
Checks: "everyone haggles with you: a seller who'd name a price haggles instead": ok. "40% of what you talk a seller
down by comes back": ok. "nothing back for a chest bought at the price asked...": FAIL. "talked down from a price that
went up...": FAIL. "everyone haggles: crooks, the King and the Maw too": FAIL (wording). "...a seller's first visit (a
lesson)": FAIL.

### goldentongue: Golden Tongue (legendary, Crownhold)
Card: "Everyone haggles and nobody walks out: go as low as you like, and they come down to 10% under the least they'd take."
Where: `Boosts.Haggles` and `Floor` x0.9 (Shop.cs:383, 388); `Unruffled` (Shop.cs:741) read first in `Visit.Offer`
(Visit.cs:234-238): any offer under the floor gets the floor.
Verdict: PARTIAL. Market sellers (haggler, offer, converted fixed) answer a 1-coin offer with their floor, 10% lower than
without. But (C3) a haggler with a tale's chest and (C1) Durgan in his lesson walk out on it; "everyone haggles"
excludes crooks, the King and the Maw (as for the purse). Note: an impatient seller still leaves when their time runs out;
the card's colon reads "nobody walks out" as about offers, so no check for it.
Checks: "nobody walks out: go as low as you like...": ok. "they come down to 10% under the least they'd take": ok.
"everyone haggles: a seller who'd name a price haggles instead": ok. "nobody walks out: not a haggler bringing a tale's
chest": FAIL. "...not on a seller's first visit (a lesson)": FAIL. "everyone haggles: crooks, the King and the Maw
too": FAIL (wording).

### trumpet: Ear Trumpet (common, the stall)
Card: "Nothing drowns out or muffles a sound for you: no clank, and no straw."
Where: `Greet` sets `Chest.Unmasked` (Build.cs:454); `Chest.Needs`/`Audible` (Chest.cs:168-186) for your ears, `plain:
true` for sellers'; `ShakeBox.Impact` (ShakeBox.cs:353-356); the sim's `Hearing.Listen` (Hearing.cs:117); `Strategy.Levels`
(Strategy.cs:26).
Verdict: PARTIAL. Coin under a clank heard at firm and hard; through straw, coin at gentle and a cloak at hard; sellers
still hear it their way. (C1) A lesson's chest (Garrick's, which teaches exactly this) is masked as without it.
Sim/bot note: with the trumpet the careful player shakes only hard (Strategy.cs:26), so it never hears sand pour at a
gentle shake (`Hearing.TooSoon`) and marks a scroll case of sand as a real spell (checked: without the trumpet it's
spotted). The sim undervalues the trumpet and the bot gets fooled; shaking gently and hard would keep both.
Checks: "no clank drowns out a delicate thing for you...": ok. "no straw muffles a sound for you...": ok. "on a seller's
first visit (a lesson)...": FAIL.

### watch: Pocket Watch (common, the stall)
Card: "Impatient sellers give you twice as long, and a rotten chest stands twice the shaking."
Where: `Patience` (Shop.cs:418) and `ShakeLimit` (Kin.cs:144-145), read by the counter's timer and rot (CounterPhase.cs:109,
142, 238, 378-395), Bram's note (`Terms`, Kin.cs:158-159), the sim (Strategy.cs:28-30, 72) and the bot (Autoplay.cs:357).
Verdict: OK. Flick 20s -> 40s, Tick 12s -> 24s, Tobin's rot 2s -> 4s, and the notes say 40 and 4 seconds.
Checks: "impatient sellers give you twice as long, and a rotten chest stands twice the shaking...": ok.

### beads: Counting Beads (common, Thornwick)
Card: "A chest its seller says nothing else about comes with a tally of every thing in it, false bottom and all."
Where: `Curiosities` (Build.cs:538-549) restates each queued visit with nothing said as a `Clue.Tally` (Events.cs:171-184,
`Market.Articles`: everything, false bottom included, never a warded thing or a Heartsong); the careful player reads it
(`Strategy.Counted`, Strategy.cs:114).
Verdict: PARTIAL. Every market chest with nothing said gets the tally, false bottom counted (and a careful read marks
the false bottom from it); Rook's scale stays her only word. (C3) Tale chests never do: Curiosities runs before
`StageTales` (Shop.cs:692 vs 701), which puts the tale's chest in a regular's place and the weekly gift chests at the front
(12 of 12 tale chests in the check, all `first.chest`; over 6 seeds and 40 days 24 of 24 tale chests, against 827
tallied). Also skipped by design: the Stranger's chained chest, the King's (Events.cs:173), and a seller who dreamt with a
pillow.
Checks: "a chest its seller says nothing else about comes with a tally...": ok. "a tale's chest its seller says nothing
else about comes with a tally too": FAIL.

### seal: Appraiser's Seal (uncommon, the stall)
Card: "A perfect read pays you a valuer's fee: 8% of what the chest held."
Where: `PerfectShare` (Build.cs:441), `PerfectFee` (438) on the opening's held (OpeningPhase.cs:886, after the seller's
terms are settled) and the sim's (Simulation.cs:1104). Note: no clash with the Royal Seal upgrade's key ("seal": `Has`
vs `HasCurio`).
Verdict: OK (38g on a 480g chest; nothing on a wrong slip or without it). Sim note: the sim's fee is on held plus the
bond's payout (Simulation.cs:1103-1104), the game's on held alone; only a perfect read bought sight unseen that held
less than its price differs.
Checks: "a perfect read pays a valuer's fee of 8%...; a wrong one, nothing": ok.

### register: Valuer's Register (uncommon, Thornwick)
Card: "A perfect read pays 0.5% of what the chest held for each one so far this run (up to 12%)."
Where: `PerfectShare` (Build.cs:442) on `PerfectReads`, which `RecordRead` increments (Shop.cs:456) after the fee is
paid (OpeningPhase.cs:886 then 898; Simulation.cs:1104 then 1105).
Verdict: MISMATCH (off by one). The read being paid isn't one "so far": the run's first perfect read pays nothing (a
480g chest: 0g, 2.4g by the card), the second 0.5%, and the opening shows no valuer's fee line at all on the first, so
the curio looks dead. Cap and steps otherwise right (5% after ten, 12% after thirty). Fix:
`RegisterStep * (PerfectReads + 1)` in the fee (it's only paid on a perfect read), or the card: "for each one before it
this run".
Checks: "a perfect read pays 0.5% a perfect read so far, up to 12%; a wrong one nothing": ok. "the run's first perfect
read pays 0.5% (it's the one so far)": FAIL.

### crescent: Cursed Crescent (uncommon, the stall)
Card: "Everything comes out exactly as its chest promised. Sellers sense the curse and ask 5% less."
Where: `Rules.Steady` (Build.cs:94) makes `Turn` return the chest's colour (Build.cs:144; odds 210-215); the shooting star
skips it (Build.cs:294); `PriceFactor` x0.95 (Build.cs:622); the opening's "held steady" line (Build.cs:276).
Verdict: MISMATCH.
- The dry spell's pity runs after luck in `Buy` (Shop.cs:815-816) and lifts a thing a step with no crescent check
  (Run.cs:268-279). With the crescent nothing ever beats its chest, so the dry spell only grows and the pity reaches its
  50%: 14 of 400 things in 100 common chests came out better, 7 of 60 chests in another run.
- Sellers ask more than 5% less, by shop: the stall 9.2%, Thornwick 5.3%, Crownhold 7.9%, the Emporium 9.4% (with the
  tools a player has there; a luckless Emporium only 1.3%). The power index prices luck in at half (`KindPower`,
  Build.cs:592-609), and the crescent's steadiness takes that luck (good and bad) out of the index on top of the 5%.
Fix: skip the pity lift (and freeze the dry spell) when `HasCurio("crescent")`; then either say "Sellers sense the
curse: they price in no luck, and ask 5% less besides", or work the index without `Steady` so only the 5% shows.
Checks: "everything comes out exactly as its chest promised (better or worse rolls, and luck, held steady)": ok.
"...however long the dry spell": FAIL. "sellers ask 5% less (at each shop...)": FAIL.

### fork: Tuning Fork (rare, the stall)
Card: "Each perfect read in a row: the next chest's things sell for 3% more (up to 30%)."
Where: `Read` (Build.cs:361) on `PerfectStreak` (Shop.cs:455), in every sale out of a chest just bought (`SaleRaw`,
Build.cs:417) and on the slip (`Value`, Build.cs:366).
Verdict: PARTIAL (edge). +3% a read up to 30%, reset by a wrong read, applied whether the next read is right or not.
But the opening records a Guild crate or a mimic as a wrong read (`RecordRead(perfect)` with `perfect` false for a
`NoRead`, OpeningPhase.cs:881, 898), which breaks the streak, though a `NoRead` is "no slip right or wrong about it"
(Appraisal.cs:48) and `ReadTrue` already ignores both (Campaign.cs:147). The sim resets on a mimic but never on a crate
(no `RecordRead`, Simulation.cs:944-958), so game and sim differ. Fix: skip `RecordRead` for a `NoRead` in the opening and
the sim. (Crates and mimics come only in a remembered season.)
Checks: "each perfect read in a row...; a wrong read starts again": ok. "...whether its own read is perfect or not": ok.
"a Guild crate (no read...) doesn't break the streak": FAIL.

## Checks that were wrong, and fixed

- bond: the payout on an 83g shortfall is 35g, not 33.2g: `Money.Round` pays sums over 30g to the 5g, as every price.
  The check allows half a step (2.5g). (The rounding that matters, a discount vanishing, is C2.)
- fork: a 160g gem's sale is rounded to the coin (1.0625 for 1.06); the check uses a 960g thing and 0.1%.
- hourglass's lesson check first compared with `T.Near`, whose +0.5 absolute tolerance hid the 9g lesson ask's missing
  discount; the lesson checks now expect `Money.Round(ask * factor)` exactly, on an ask where the discount shows.

## Harness output (this group)

    36 of 70 checks passed, 34 failed.
