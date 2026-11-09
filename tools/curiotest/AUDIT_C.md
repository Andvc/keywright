# Curio audit C

> Written before 0.35's fixes, against 7be794f: it records what each card promised and what the game did then. Every finding here was then fixed in the game, or the card was corrected where its promise couldn't hold (see docs/design.md, "Every card does what it says"); the checks in this folder now all pass.

Group C, 26 curios: book, rope, lamps, gauntlet, cap, whetstone, dummy, scales, still, desk, wheel, oil, knife, gloves,
loupe, mark, purse, ticket, marker, jar, lockbox, note, letter, ribbon, errand, net.

For each one: the card as an English player reads it, every place it acts (game, sim, UI), a verdict, and the checks in
`tools/curiotest/ChecksC.cs`. The checks take their numbers from the card's words ("three-quarters more" is 1.75 in the
check, not `Curios.WhetstoneBonus`), compare twins (the same shop with and without the curio, on a day with no event),
and run the game's own Core paths (`GameState.Buy`/`Luck`, `SaleValue`, `EndDay`, `PayDues`, `Patrons.Roll`, `NewVisit`).

Run: `dotnet run --project tools/curiotest -- --only=book,rope,lamps,gauntlet,cap,whetstone,dummy,scales,still,desk,wheel,oil,knife,gloves,loupe,mark,purse,ticket,marker,jar,lockbox,note,letter,ribbon,errand,net`

Result (game at 7be794f, prototype/0.35): **56 checks, 43 pass, 13 fail**. Every failure is the game's, not the check's.
(Whole harness: 44 of 57 passed, 13 failed.)

## Verdicts

| Curio | Verdict | In a line |
|---|---|---|
| book | OK, UI misreports | Pays 6g x shop scale x points every night; the day end's "Visitors leave Ng a night" leaves out the scale (half at Thornwick, a seventh at the Emporium). |
| rope | OK, UI misreports | Exhibition x2, and all it draws; per-item "+N exhibition" texts show the undoubled points. |
| lamps | OK (edge) | +1.5%/point to 60% on everything real; skips fakes and broken glass that other curios make saleable (shared root cause, below). |
| gauntlet | OK, UI misreports | Mends weapons and adds a 25% step; the opening's "Smith's Glove: n mended" counts every mended thing, cloaks too. |
| cap | OK, UI misreports | Same as the glove, for cloaks. |
| whetstone | OK | Weapons x1.75. |
| dummy | OK, UI misreports | Cloaks x1.5 and 3x points; per-item "+N exhibition" texts show 1x. |
| scales | OK | Coin x(1 + 0.2 per real purse in the chest). |
| still | **MISMATCH** | A broken potion sells for half its plain worth, a third of what the still makes an unbroken one fetch. |
| desk | **MISMATCH** | Sand sells for a third of a spell's plain worth, two-ninths of what a spell fetches with the desk. |
| wheel | OK | Gems: a 50% step (shine at purple). |
| oil | OK | One pin fewer on every lock. |
| knife | **PARTIAL** (sim, UI) | Core right; the sim always forces with it (no masterful picks, and no Locksmith key piece while it's held); lockpick and opening texts still say forcing scratches and breaks. |
| gloves | OK | Nobody minds (even fussy sellers); terms text drops it. |
| loupe | PARTIAL (edge) | A quarter of plain worth, not of what the real thing fetches when lamps, a synergy or the trades raise it. |
| mark | **MISMATCH** | Crooks come x1.25 (home tier) to x1.6, not x2; fakes fetch the real thing's plain worth, not what it fetches here. |
| purse | OK | Three stashes, not two. |
| ticket | OK | Pawn at 4/5, not 3/5. |
| marker | OK (edge) | Two words, not one; like Bram's word itself, never after the debt (week 8 on). |
| jar | OK | 3% a night, rounded down, capped at 20/40/80/140g. |
| lockbox | PARTIAL (edge) | +25% after paying a Guild night's due; nothing if you "Pay it all" that night (the sim pays the due first and gets it). |
| note | PARTIAL (edge) | Works once, short nights only, not week 7; never offered on the Guild's interest nights (week 8 on). |
| letter | **MISMATCH** (generous) | Takes one quality lower, but for every commission, Lord Harrow's crier's too, not just regulars'. |
| ribbon | **PARTIAL** | Raises the posted reward by half, not the "quarter over the sale" floor: a better-than-asked thing, or anything at the Emporium with the Royal Warrant, gets x1.2 or nothing. |
| errand | PARTIAL (edge) | +2 a week, except where one-commission-a-kind caps the board (Thornwick with the Clockwork Lark: +1). |
| net | OK | Songs x3. |

## The findings, worst first

### mark: Fence's Mark (MISMATCH)

Card: "Crooks come twice as often, and the Maw's buyers pay for any fake what the real thing fetches."

1. **Crooks come x1.25 to x1.6, not x2.** `Shop.cs:676-677` doubles a crook's weight in the day's draw, but the draw is
   without replacement (`Shop.cs:668-669`: nobody comes twice in a day while there's anyone new). The crooks (Tilly in
   Crownhold; the Countess and Gilt at the Emporium) are home-tier sellers already drawn on about 70% of days, so they can
   only rise to about 90%. Measured over 1,500 first days each (check "crooks ... come twice as often"):

   | Shop | Without | With | Ratio |
   |---|---|---|---|
   | Crownhold (Tilly) | 0.699 a day | 0.889 | x1.27 |
   | Emporium, all three | 1.669 a day | 2.161 | x1.29 |
   | Emporium, Tilly (a tier below, weight 0.6) | 0.222 | 0.359 | x1.62 |
   | Emporium, the Countess / Gilt | 0.727 / 0.719 | 0.902 / 0.899 | x1.24 / x1.25 |

   Fix: reword ("Crooks come more often"), or let the mark bring a crook back a second time in a day (at `Shop.cs:668`,
   keep a crook in `fresh` until they've come twice), and re-measure.

2. **A fake fetches the real thing's plain worth, not what the real thing fetches here.** `FakeValue` (`Build.cs:388-395`)
   returns `Categories.Value(kind, quality)`; `SaleRaw` (`Build.cs:406-419`) then applies the grade, the buyers' trades and
   the day, but the guard at `Build.cs:410` skips `General()` for fakes: an active synergy for that kind, the gallery lamps,
   the moneychanger's scales, the keepsakes. Scenario: Emporium, mark and lamps with 20 exhibition points, a chest with a
   purple gem and a purple paste gem: the gem sells for 208g, the paste for 160g (check "...what the real thing fetches
   here"). In a plain shop they match (check passes).

Sim: the careful player's worth counts spotted fakes at `Expected x SaleMultiplier` (`Strategy.cs:283`), the same rule
as the game (it shares the gap). The crowd is Core (`StartDay`): the same in game and sim.

### still: Alchemist's Still (MISMATCH)

Card: "Potions sell for half as much again, and broken glass still sells for half."

Potions x1.5: `Build.cs:328` (in `Certain`, so in the power index too). Broken potions: `BrokenValue`, `Build.cs:382`:
`Categories.Value(kind, quality, magic) * 0.5`, and `SaleRaw` (`Build.cs:410`) skips `General()` for broken things. So a
broken potion sells for half its plain worth, which is a third of what the still makes an unbroken one fetch, and no
synergy (the Apothecary) or lamps apply either. Scenario: a rare potion sells for 48g with the still; broken by the
crowbar it sells for 16g, where "half" is 24g (check "broken glass sells for half of what that potion fetches here").

Fix (code): let the broken potion go through `Certain(kind, count)` too (below, "shared root cause"), which makes it
24g. Or (wording): "...and broken glass still sells for half its worth."

### desk: Scribe's Desk (MISMATCH)

Card: "Spells sell for half as much again, and sand sells as ink, for a third of a spell."

Spells x1.5: `Build.cs:329`. Sand: `FakeValue`, `Build.cs:393`: a third of the spell's plain worth; `General()` (with the
desk's own x1.5) is skipped (`Build.cs:410`). Scenario: a rare spell sells for 30g with the desk; a rare scroll case of
sand beside it sells for 7g, where a third of the spell is 10g (check "sand sells for a third of a spell"). Fix: as the
still (code), or "...for a third of a spell's worth".

### letter: Letter of Introduction (MISMATCH, in the player's favour)

Card: "Regulars take what you deliver a quality lower than they asked for."

`LetterLeeway` (`Run.cs:374`) returns 1 for every commission and ignores which; `Takes`/`WantedBy` (`Run.cs:365-371`)
use it for the opening's delivery buttons (`OpeningPhase.cs:1059-1095`) and the sim (`Strategy.ShouldDeliver`,
`Strategy.CommissionWorth` at `Strategy.cs:456`). So Lord Harrow's crier (`Events.Reward`, `Events.cs:187`, posted on
the crier's day: not a regular) also takes a quality lower. Scenario: the crier wants "a weapon, uncommon or better";
with the letter a wood weapon is taken (check "only regulars: Lord Harrow's crier..."). The comment on `Takes`
(`Run.cs:367-370`) agrees with the card ("for the regulars"). Fix: `HasCurio("letter") && c.Patron != Events.Crier ? 1 : 0`,
or reword the card to "Commissions take...".

### ribbon: Patron's Ribbon (PARTIAL)

Card: "Commissions pay half as much again."

`CommissionReward` x1.5 (`Run.cs:377-378`), so the board's posted share (`CommissionEach`, `CounterPhase.cs:587-593`,
`DayEndPhase.cs:201`) is half as much again. But what a delivery pays is `CommissionPay` (`Run.cs:387`): the most of that
share and a quarter over the sale (`Patrons.Premium`). The ribbon raises only the first, so whenever a quarter over the
sale is close to or above the share, the ribbon adds less than half, or nothing:

- At the stall, a green weapon for "green or better" (reward 20g): 20g -> 30g, x1.5 (check passes).
- A blue weapon for the same commission (sells for 20g): 25g -> 30g, x1.2 (check fails).
- An enchanted blue weapon (sells for 40g): 50g -> 50g, nothing.
- Emporium with its tools and the Royal Warrant (buyers x2.0), a purple gem for "purple or better" (reward 320g, sells for
  319g): 399g -> 480g, x1.2 (check fails). Every exactly-asked delivery there is the same, and a whetstone or a synergy
  for the kind does it anywhere.

The sim values commissions by the same rule (`Strategy.cs:457-459`), so the sim is right about the ribbon; the card is
what overpromises. Fix: either pay `max(share, floor) x 1.5` (multiply the floor too, so a delivery always pays exactly
half again: check its balance with `--sim=curio --who=ribbon` and `--sim=builds`), or reword ("Commissions post half as
much again"). The Clockwork Lark's pay (`Heartsongs.LarkPay`) has the same shape.

### knife: Burglar's Knife (PARTIAL: sim and UI)

Card: "Forcing a lock breaks nothing and scratches nothing."

Core is right: `GradeMultiplier` (`Run.cs:348`) gives a forced lock x1, not x0.9; `ForceSturdier` (`Run.cs:351`) is 1, so
`Chest.Force` (`Chest.cs:208-213`) breaks no glass. The game's lock (`LockpickPhase.cs:152`) and the sim
(`Simulation.cs:696`) both force with it. Checks pass.

1. **The sim always forces with the knife** (`Simulation.cs:691`: `grade = state.HasCurio("knife") ? PickGrade.Forced`).
   In the game the knife only takes the sting out of forcing; you still pick, for a masterful +25%. The sim's careful
   player picks masterfully 30% of the time, cleanly 55% and forces 15% (`Simulation.cs:693`), so its locked chests
   average x1.06 (0.3 x 1.25 + 0.55 + 0.15 x 0.9) without the knife and x1.0 with it: holding the knife costs the sim's
   careful player about 6% of every locked chest, against the glass it saves on the 15% it would have forced. Worse, the treasury key's
   Locksmith piece needs a lock opened without force (`Campaign.cs:152-156`), so a sim player holding the knife can never
   earn it. `Strategy.CurioWorth["knife"] = 1` reflects this. Fix: drop the knife from the grade roll at
   `Simulation.cs:691` (pick as usual; a forced outcome is then harmless).
2. **The UI still says forcing scratches and breaks**: `LockpickPhase.cs:64` ("Forcing it is quick, but it scratches
   everything and breaks glass." / "Forcing it will break it."), `LockpickPhase.cs:162` ("CRACK. Forced open: everything's
   scratched, and any glass is broken."), `OpeningPhase.cs:961` (" Forced open: everything -10%."). The sums shown per
   thing are right (they use `SaleValue`). Fix: word these on `State.GradeMultiplier(PickGrade.Forced) >= 1` and
   `State.ForceSturdier >= 1`. To see it: `--autoplay --curios=knife --start-tier=2 --shots=shots_knife`, a forced lock's
   `06_lockpick` and `08_results` captures.

### book: Visitors' Book (OK; the day end misreports it)

Card: "Every night, visitors leave 6g per exhibition point (more in a grander shop)."

`Overnight` (`Run.cs:496-501`, run by `EndDay`, `Shop.cs:941`, from `DayEndPhase.cs:34` and `Simulation.cs:1126`) pays
`6 x ShopTiers.Scale(tier) x Exhibition` into the purse and `Today.Curios`. Checks pass at Thornwick, Crownhold and the
Emporium, three nights running. But the day end's exhibition line (`DayEndPhase.cs:348`) says "Visitors leave
{6 x score}g a night", without the scale, while the same screen's "Curios +Ng" (`DayEndPhase.cs:86`) shows the real sum:
at Thornwick the line says half what comes in, in Crownhold a quarter, at the Emporium a seventh. Fix:
`Curios.BookPerPoint * ShopTiers.Scale(State.Tier) * score` (better, one Core property used by both). To see it:
`--curios=book --start-tier=3 --shots=...`, the `09_dayend` capture once something's on the shelf.

Note: `NoUse` (`Kin.cs:197`) keeps the book from Isolde as if it "would do nothing for this character", but it works for
her like anyone (Overnight has no character check), on top of her door money.

### dummy: Tailor's Dummy, and rope: Velvet Rope (OK; per-item points misreported)

Dummy: "Cloaks sell for half as much again, and count 3 times over on display." Rope: "Your exhibition counts double."

`Certain` (`Build.cs:327`) gives cloaks x1.5; `Exhibition` (`Shop.cs:338-339`) counts a cloak's points x3 (dummy) and the
whole x2 (rope), and everything that reads it follows: a customer (`Shop.cs:461`), finer chests (`Shop.cs:464`), the
book, the lamps, Isolde's door money. Checks pass. But every per-thing points figure the player reads is the raw
`Item.Prestige`: the kept thing's "+N exhibition" (`OpeningPhase.cs:1132`), the keep button and its tooltip
(`OpeningPhase.cs:1154-1157`), the full-shelf swap (`OpeningPhase.cs:1177-1189`), the shelf tooltips and "comes off
display: N points fewer" (`CounterPhase.cs:473,492`, `DayEndPhase.cs:343,386`), the pawn button (`DuesPhase.cs:234`).
Keep a rare cloak with both and the opening says "+4 exhibition" while the exhibition grows by 24, so the dummy and the
rope look as if they did nothing. Fix: a Core `GameState.Points(Item)` (prestige x the dummy's 3 for a cloak x the
tailor's bonus x the rope's 2) used in those places. To see it: `--curios=dummy,rope --start-tier=3 --shots=...`, the
`09_swap`/`08_verdict` captures after keeping a cloak.

Sim: the careful collector decides what to keep by raw `Prestige` (`Strategy.cs:736-741`, Isolde's `Collects` at
`Strategy.cs:754-759`), so it never keeps cloaks for the dummy's tripled points (a strategy gap, not a different rule).

### gauntlet: Smith's Glove, and cap: Hatter's Cap (OK; the opening's line miscounts)

Cards: "You mend weapons: never worse than their chest promised, and 25% likelier to come out better." (cap: cloaks)

`Mends` (`Build.cs:114`) and `StepChance` (`Build.cs:105`: +0.25 for the kind), applied by `Rules`/`Turn`
(`Build.cs:88-150`) in `Luck` (`Build.cs:253`), which `GameState.Buy` runs for the game and the sim alike; the slip and
the power index read the same rules through `Odds`/`Turn`. Checks pass: weapons below a blue chest's colour always come
out blue or better and cloaks don't (and the other way round for the cap); a weapon as good as its chest steps up 25% of
the time with the glove, 0% without. ("25% likelier" is the game's convention for an extra one-in-four chance of a step,
as the foot, the dip and the wheel use it.)

The opening's credit line (`Build.cs:287-288`) counts every mended thing for each of them, whatever its kind: with the
glove and the cap, a blue chest with a green weapon and a green cloak reads "Smith's Glove: 2 things mended." and
"Hatter's Cap: 2 things mended." (`OpeningPhase.cs:124`; checks "the opening's ... counts weapons only" and "...cloaks
only" fail). The same count also credits the glove with what the Master Restorer mends. And the glove's and cap's 25%
step is never credited (they aren't in `lucky`, `Build.cs:279-281`): it's thanked to another luck curio, or nobody. Fix:
filter the count by kind (gauntlet: `Category.Arms`, cap: `Category.Garb`, the blindfold: the rest).

### loupe: Jeweller's Loupe (PARTIAL, edge)

Card: "Paste gems and demon gold sell as costume pieces: a quarter of the real thing."

`FakeValue`, `Build.cs:392`: a quarter of plain worth for fake coin and gems (sand left alone); checks pass in a plain
shop. With anything that raises what the real thing fetches (lamps, the Goldsmith synergy, the scales), the paste gets
none of it (`Build.cs:410`): Emporium, lamps at 20 points: real purple gem 208g, paste 40g, where a quarter is 52g.
Same root cause and fix as the mark. (Nim brings it only at the Emporium, `MinTier` 4, though a year of forgers puts
demon gold in every chest from the stall.)

### lockbox: Miser's Lockbox (PARTIAL, edge)

Card: "Every Guild night, once you've paid, your purse grows by 25% of what's left in it."

`PayDues` (`Run.cs:171-173`) adds `Money.Round(gold x 0.25)`; the dues screen floats it (`DuesPhase.cs:328`). Checks pass
on two Guild nights. But "Pay it all" on a Guild night (`DuesPhase.cs:334-336`) goes through `PayOff` (`Run.cs:199-210`),
which never grows the purse: paying off on the first Guild night with 100,000g leaves 82,010g with or without the
lockbox (20,500g missing). The sim pays the week's due (`Simulation.cs:1132` -> `Dues` -> `PayDues`) and then pays off
(`Simulation.cs:1133-1137`), so it gets the lockbox where the game doesn't. (Paying off from the fifth day's end,
`Main.cs:974`, skips that night's Guild visit altogether, and the lockbox with it.) Fix: in `PayAll`, `State.PayDues()`
then `State.PayOff()` (as the sim does), or grow the purse in
`PayOff` when it's a Guild night. Rounding: `Money.Round` makes it at least 1g (a purse emptied by the due still gets
1g) and rounds above 30g to fives.

### note: Guild Promissory Note (PARTIAL, edge)

Card: "The Guild's own paper: once a run, on a Guild night you can't cover, it pays what's short (not the last week)."

`CanCallNote`/`CallNote` (`Kin.cs:211-221`), offered on the dues screen when short (`DuesPhase.cs:261-273`) and by the
sim before Bram's word (`Simulation.cs:1279-1283`). Checks pass: it pays exactly what's short, the due is paid, it's
gone after, it isn't offered when pawning covers the night, and not in week 7. But `Week < Guild.Weeks` also rules out
every week after the debt (the Guild's interest, week 8 on, played after "Keep trading", `RunEndPhase.cs:95`), which
the card doesn't: a short night in week 8 with the note unused offers only "Let the Guild take the shop". Fix:
`Week != Guild.Weeks`, or "(not in the debt's last week or after it)". Bram's word (`CanAskGrace`, `Run.cs:92`) has the
same limit against a glossary that says only "Not in the debt's last week." (relevant to the marker).

Note: in week 1 the note can never be called, since selling the note itself back (25g) covers the 20g due; that's
consistent with "a night you can't cover".

### errand: Errand Book (PARTIAL, edge)

Card: "The regulars post 2 more commissions every week."

`Patrons.Roll` (`Commissions.cs:120-121`) asks for 2 more, rolled each week's first morning (`Run.cs:451`). Checks
pass at Thornwick, Crownhold and the Emporium, and on week 2's morning (2 -> 4). But the board posts one commission per
kind of thing (`Commissions.cs:137-138`), so it caps at the shop's kinds (+1 enchanted from Crownhold): with the
Clockwork Lark singing at Thornwick (2 + 2 = 4 already, five kinds), the book adds 1, not 2 (check fails). Fix: let a
second commission of a kind be posted once every kind has one, or note the cap.

### lamps: Gallery Lamps (OK, with the shared edge)

Card: "Everything sells for 1.5% more per exhibition point (up to 60%)."

`Certain` (`Build.cs:333`): `1 + 0.015 x min(40, Exhibition)`, in every real thing's sale (from a chest, off the shelf,
pawned, the commission floor) and in the power index. Checks pass: 20 points, x1.3 on every kind, from a chest and off
the shelf; 48 and 64 points, x1.6. "Everything" excludes fakes and broken glass a curio makes saleable (`Build.cs:410`).

### Shared root cause (still, desk, loupe, mark, lamps)

`SaleRaw` (`Build.cs:406-419`) multiplies a thing by `General()` (the shop's name for its kind, the trades' curios, the
lamps, the wishbone, the keepsakes) only when it's real and unbroken (`Build.cs:410`). That's right for a fake nobody can
sell, but four curios make fakes or glass saleable "as" something, and then they fetch that thing's plain worth only. A
one-place fix: for a fake or broken thing whose value comes from a curio (`FakeValue(item) > item.Value` or
`BrokenValue(item) > 0`), also multiply by `Certain(item.Category, count)` (not `General`: a fake isn't a surprise for
the wishbone). That makes the still's broken potion 24g, the desk's sand 10g, the loupe's paste 52g and the mark's paste
208g in the scenarios above, and the lamps then reach them too. The power index is unaffected (it prices real things).

## The rest, OK

- **whetstone** (Whetstone): "Weapons sell for three-quarters more." `Certain`, `Build.cs:326` (sale and index). Check: x1.75 on weapons only, off the shelf and from a chest.
- **scales** (Moneychanger's Scales): "Coin sells for a fifth more for every purse of it in the chest." `Certain`, `Build.cs:330`, with the count of real purses from `SaleRaw` (`Build.cs:412`; the slip passes its own count, `Appraisal.cs:107`). Checks: 1, 2, 3 purses give x1.2, x1.4, x1.6; demon gold doesn't count. (A purse sold off the shelf counts as one, x1.2.)
- **wheel** (Gem Cutter's Wheel): "Gems are 50% likelier to come out a step better." `StepChance`, `Build.cs:103`; credited at the opening (`Build.cs:281`). Check: gems as good as their chest step up half the time (wood and blue chests), and shine half the time in a purple one (luck never makes gold, as the glossary says); coin unaffected.
- **oil** (Locksmith's Oil): "Every lock has one pin fewer." `Oiled`, `Shop.cs:402-407`; the lock you pick (`LockpickPhase.cs:42`), the counter's "N pins" (`CounterPhase.cs:309`), and the sim's odds (`Simulation.cs:690`: a pin fewer, +0.1 masterful). Check: 2/3/4 pins become 1/2/3, no lock stays none, a rusty year's extra pin cancels.
- **gloves** (Kid Gloves): "Nobody minds a hard shake, a drop or the lodestone on their chest." `KidGloves` (`Shop.cs:425`) in `MindsDrop`/`MindsMagnet`/`MindsHardShake` (`Kin.cs:123-127`), which the counter (`CounterPhase.cs:384-386,1133,1240`), the terms text (`Kin.cs:171-174`) and the sim (`Strategy.cs:24,79,88`, `Simulation.cs:546`) all use. Checks: Cadoc, Brass and the Countess stop minding, and their terms stop saying so; a fussy year too. (Rook's mare and a day of mourning still forbid a hard shake: those aren't anyone minding.)
- **purse** (Rainy-Day Purse): "Bram's stash helps you out three times a run, not twice." `StashLeft`, `Run.cs:222-223`, in `DipIntoStash` (`Run.cs:238-245`, each morning, game and sim). Check: eight broke mornings, 2 dips without, 3 with.
- **ticket** (Pawn Ticket): "On a short night, what you pawn fetches four-fifths of its price, not three-fifths." `PawnRate`, `Run.cs:126` (`Guild.PawnRate` 0.6); `PawnValue`/`Pawn` (`Run.cs:124,138-144`), shown and used on the dues screen (`DuesPhase.cs:232-234,283`) and by the sim (`Strategy.PawnOrder`). Check: on a real short night, 0.6 vs 0.8 of the sale price, and the purse gets it. (Nim still lays it out for Isolde, who can't pawn: `NoUse`, `Kin.cs:200`, leaves her out.)
- **marker** (Guildsman's Marker): "Bram's word is good twice a run, not once." `Graces`, `Run.cs:86`, through `CanAskGrace`/`AskGrace` (`Run.cs:92-108`), the dues screen (`DuesPhase.cs:251-258`) and the sim (`Simulation.cs:1285-1289`). Check: a second short night gets his word with the marker only; a third doesn't. Edge as the note: Bram's word is never offered after the debt (week 8 on).
- **jar** (Savings Jar): "Every night: 3% of your gold as interest (up to 20g at the stall, more in a grander shop)." `Overnight`, `Run.cs:490-495` (`EndDay`, game and sim; the day end's "Curios" figure). Checks: 3% rounded down, capped at 20/40/80/140g, across eleven purses and four shops, and seven nights running. (Rounded down: 66g earns 1g.)
- **net** (Songcatcher's Net): "Heartsongs are three times as likely to be in a chest." `Sing`, `Shop.cs:754-760` (every regular's chest, `Shop.cs:743`, and the Stranger's, `Run.cs:462`; tale and lesson chests never get a song, with or without it). Check: with the chance raised to 1 in 20 for counting, 10,000 chests each: x3.

## Sim against game

Every effect above is in Core and shared, except:
- the knife: the sim always forces (`Simulation.cs:691`), the game lets you pick (above);
- the lockbox: the sim pays the week's due before paying off, the game's "Pay it all" doesn't (above);
- the oil: the sim's lock is a roll moved +0.1 masterful (+0.05 clean) per pin taken off (`Simulation.cs:690-694`), the game's is the timing game with one pin fewer;
- display choices: the sim keeps by raw prestige (dummy, rope) and never values a spotted fake for the loupe or the desk, nor dropping glass for the still (`Strategy.cs:79-81,283,736`): strategy gaps, the rules are the same.

## How this was run

The worktree was created at `b75f96c` (main), which has no `tools/curiotest`; its branch had no commits of its own, so
it was fast-forwarded to `prototype/0.35` (`7be794f`) before the audit. Nothing under `scripts/` was changed.
