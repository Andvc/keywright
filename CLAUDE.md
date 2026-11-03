# Keywright: notes for Claude

Solo-developed PC game for Steam (Godot 4.7.2 .NET, C#, 2D, GL Compatibility renderer). The owner knows Unity/C#
and works through cloud sessions rather than locally, so changes must be verified here, not left for them to test.

Until 0.18 the game was called LootFall and lived on the `prototype/chest-loop` branch of Andvc/LootFall. That name
and repository now belong to a different game (an auto-battler about opening chests); the two share only the
toolchain. Godot keeps a game's files in a folder named after it, so on first start `Main.CarryOverOldFiles` brings
the profile and a readable save over from the old `app_userdata/LootFall` folder.

## Setup in a fresh container

Run `tools/setup-cloud.sh` (add `--export-templates` before exporting). It installs the .NET 8 SDK and Godot to
`/opt/godot` and symlinks `godot`. GitHub release downloads work through the proxy; Unity's servers do not.

## Verify every change

1. `dotnet build` must report 0 warnings and 0 errors.
2. Gameplay/balance changes: `godot --headless --path . -- --sim` and compare the report before and after. Its last
   section is the lesson test: every seller's lesson must complete when played the careful way; the one before it,
   "Bram's kin", plays each character at challenge levels 0 to 8.
   `-- --sim=quick` (three minutes) prints the main table, the careful collector week by week, the margins by power
   index, the career targets (the careful player and learners, both without Heartsongs, and the careful player with no
   curios) and, for the careful player and a first run, the dues week by week and profit per customer at each shop,
   for tuning the economy.
   `-- --sim=songs` runs only the Heartsongs section (how often, each one on display from day 1 against curios, and the
   careful player with and without Heartsongs) for tuning them quickly, and `-- --sim=songcount [--heartsongs=P]` just
   how many a run finds (the careful player and a first run), for tuning `Heartsongs.Chance`; `-- --sim=sellers` only the per-seller table and
   the lesson test (ten seconds), for a seller's rule; `-- --sim=seller --who=KEY` plays one seller's visits and prints
   each chest, clue, slip and deal, for debugging a rule. `-- --sim=builds [--n=100]` (ten minutes) is the curio
   report: each curio alone, given when Nim could first have brought it (`Style.Arrives`; takings as a geometric mean,
   and clear rate, against the careful collector with no curios), each way of trading's best three the same way and a
   careful player buying for it (`Style.Focus`), the sight-unseen build a piece at a time (`Care.Unseen`), the designed
   pairs against their parts, and a blind buyer with each other build. `-- --sim=curio --who=a,b` measures a few curios
   quickly; `-- --sim=focus --who=TAG` (or `any`, or `--trace` for Nim's visits) shows what a builder ends up keeping;
   `-- --sim=index` prints the power index, typical asks and working purse by shop, and each curio's share of the
   index; `-- --sim=curiolist` prints the curio table for `docs/design.md`. `-- --sim=experience` plays 30
   learners through their first three runs (tips on, profile carried over) and reports a human's minutes, what's new
   and how much at once (load), peaks, choices and stale stretches, day by day, with what to change.
   `-- --sim=trace --seed=N` prints one careful collector's run day by day (purse, customers, spending, upgrades,
   curios, the Guild) for tuning the economy's pace. `-- --sim=terms [--who=a,b]` (ten minutes) plays Kit with each
   challenge term alone, then the dues alone and whole levels as drawn; `-- --sim=levels [--debtstep=S]` just the
   levels, one to eight; `-- --sim=careers` just the careers (learners at the highest level open, run by run).
3. Visual changes: run the autoplay bot under Xvfb with `--shots=<dir>` and look at the PNGs.
   Bot flags: `--seed=N --quit-at-end --days=N --no-tutorial --start-tier=N --gold=N --start-week=N --character=KEY
   --challenge=N --terms=a,b --heartsongs=P --songs=all --event=KEY --curios=a,b,c --gm` (`--terms` plays just those
   challenge boons and banes, by key, whatever the level deals; `--curios` owns those from the
   start, to see a build's screens; `--gm` turns GM mode on, for its panel beside the pause menu; `--event` makes every
   day that event, to check
   its screens; `--quit-at-end` quits after the first opening: use `--days`
   for longer runs; `--heartsongs=0.3` puts a Heartsong in that share of chests, `--songs=all` plays as if all nine were
   known, so `--vault-demo --songs=all` shows the other ending). `--start-tier`
   jumps to a shop tier with the tools a player would own by then (use it to check later tiers' screens);
   `--start-week` fast-forwards a run with the sim's careful collector (curios, commissions, exhibition and all; `--gold`
   then sets the purse, e.g. 0 to see a short night and foreclosure; `--days` counts from day 1, so add the weeks
   skipped). The bot plays scenes, Nim, the Guild's night and deliveries, opens the short-of-chests warning once (and
   says "Not now"), keeps every Heartsong (after a look at its reveal), and quits when a run ends. With tips
   on, every seller's first visit is a lesson, so a guided run at a tier shows those lessons; the bot logs any lesson
   it couldn't finish. `--opening-demo` opens a crafted chest with every quality and two surprises (record it);
   `--vault-demo` opens the Crown Chest, the finale's nine-thing opening; `--engine` starts with the Bookkeeping Engine.
   `--shell-demo` walks the title screen and its collection (finds, curios, Heartsongs, from a made-up profile), the
   character screen, a new game (Hesk, at challenge level 8: the most boons and banes the character screen and the
   pause menu ever list), the pause menu and its collection, and the settings (with a look at the scene with the outlines turned off, when the art has them; with `--gm`, the GM panel too: gold, the
   notebook and the collection filled, a skip refused mid-scene). A bot run shows the notebook's pages, clicks the
   Guild's due in the top bar once it's up (the notebook at the debt), and opens the collection over the counter.
   Keep screenshot folders named `shots*` (git- and export-ignored). Under Xvfb there's no window manager, so
   fullscreen stays at 1280x720 (the 3px art at 1.5 pixels a cell): for another size, a windowed settings.cfg in the
   user folder (below) and a screen that size. After changing art or how a picture is loaded:
   `python3 art/tools/density3_check.py`, `python3 tools/import_art.py --density-check`, `godot --headless --path .
   --import`, then `--density-review` at 1440p and a bot run.
4. Shaking or physics changes: `godot --headless --path . -- --shake-test` should hear everything the rules say is
   audible at each strength (and with the magnet, the lodestone in game), with no leaks.
5. For feel/timing, record with `--write-movie movies/lot.avi --fixed-fps 60` and convert with ffmpeg.
6. Saves, the profile or `Storage`: `godot --headless --path . -- --save-test` (writes that survive a crash, the
   `.bak` fallback, unreadable files set aside, old save versions and renames, a newer version left alone). Input,
   prompts or the pointer: `xvfb-run ... -- --pad-test --no-tutorial --seed=5` plays the counter with a simulated
   controller (the stick to the chest, A and flicks to a hard shake, Y, B, Start, then rebinding); run it in a scaled
   window too (a `[video]` `screen=0`, `window_width=1600`, `window_height=900` settings.cfg in the user folder,
   removed afterwards, under `-screen 0 1920x1080x24`), since the pointer's sums only show there. Both exit 1 on a
   failure. The full shake test takes about eleven minutes (`--n=4` for a quick look).
7. Text: `dotnet run --project tools/loc -- check` must say every text is wrapped (it lists any new one that isn't),
   and `-- extract` rewrites locale/keywright.pot (commit it). `-- verify` checks each translation in locale/ against
   it (untranslated texts, holes and `{price}`-style names kept, plural forms, letters the language's fonts have;
   `--strict` fails on any of it): a text added or changed in English comes up untranslated in all four until it's
   translated (`locale/GLOSSARY.md` has each language's words and voice). `godot --headless --path . -- --loc-test`
   runs the language layer through Godot with a made-up German translation, then loads each translation and checks
   it. `-- --sim=quick --pseudo` must print the same numbers as without (only the words change: a key that got
   translated shows up as a different number), and a bot or shell-demo run with `--pseudo` shows every translated
   text in brackets, so anything plain is untranslated, and a third longer, so a layout too tight shows. Any run
   with `--shots` also prints `[overflow]` for each label or button whose text outgrew the box it was placed in
   (`Debug.Overflow`): after changing a screen or its words, run the shell demo and a bot run with `--lang=fr` and
   `--lang=ja` (French runs longest, Japanese and Chinese are drawn on a 12px grid) and read those lines.

## Conventions

- The world comes first (`docs/world.md`): the Locking, the Key-mark, the Gilded Guild, the Maw, the four places. Any
  new seller, thing, event or line belongs to a place and has a reason you could tell a stranger in a tavern; check it
  against the world doc and add to it there.
- `scripts/Core` stays engine-free (no `using Godot`) so it can be simulated headlessly.
- Design rules (see `docs/design.md`): the puzzle is three rules (a thing only sounds once the shake stirs it;
  clanks drown out delicate things; sellers price only what they think is there). Never show the player what a chest
  is worth before purchase: the slip only adds up what the player ticked, and its odds bar only says how likely that
  is to beat the price (`Appraisal.ChanceAbove`); each item's real quality is rolled around the chest's colour
  (`Categories.Odds`) so openings still surprise. Clues are
  perceptual (which sounds come at which strength, what glows, what the lodestone silences, a hollow drop, what straw
  muffles) or stated, and always true (where it came from, and what some tell you besides: `SellerDef.States`, Brass's
  ticket counting every article, Rook's scale, and the signs that point at a warded thing). Nobody says what they
  heard (v22 retired the claims): a seller prices what they noticed (`Visit.Noticed`, never shown), and the slip
  starts empty, its header saying how they came to their price (`SellerDef.Priced`), so working out what they priced
  is the player's deduction, from how they judge.
  No mechanic is about whether a seller is lying: none of them do. Any new way to judge a chest must follow a rule
  people know from real life, and should be a deduction (subtract what you heard from a count or a weight, read a sign).
  The slip values things as good as the chest's colour promises (`Categories.Nominal`); sellers price by the average
  (`Categories.Expected`), times the shop's power index (`GameState.PriceFactor`, below); `Strategy` buys when the odds
  bar is on its side (`Appraisal.LikelyWorth`), like a player would. Layers of deduction (false bottoms, counting, sound-alikes like sand and demon gold) arrive one shop at a
  time, each taught by the seller it belongs to. Rules apply every time or not at all: a hidden roll that makes a rule
  work only sometimes reads as a bug, and a rule people can't reason about from real life (v7's cloth padding) goes. Enchantment (`Item.Magic`) is something any
  thing you can hear can be: x2 value and exhibition, same quality, colour and sprite. A hidden thing (under a false
  bottom, or warded) never is, since nobody could see it glow and no read could be right about it: it fetches x1.3
  instead (`Categories.HiddenMultiplier`, in `Special`; `Chest` strips magic off anything hidden).
- Six kinds of thing a sword-and-sorcery realm would lock away (`Category`: Coins, Arms, Garb, Potions, Scrolls, Gems,
  shown as Coin, Weapons, Cloaks, Potions, Spells, Gems; the fakes are demon gold, paste gems and scroll cases of sand).
  Every thing is a find worth having (no letters, no rusty daggers); rename one in `Categories.Renamed` too, so saves'
  journals carry over. One sound per kind of thing, the one you'd expect in real life (`CategoryDef` in `Items.cs`; alternatives for each
  kind, chosen in Settings > Item sounds, are in `Audio.SoundSets`: play `SoundSets.Group(kind)`); kinds differ in how
  hard you must shake to hear them (`StirsAt`) and whether they drown out or get drowned out (`Loud`, `Delicate`).
  Introduce new things one at a time: by shop tier (`CategoryDef.Tier`, `SellerDef.Tier`) and with a Bram hint
  (`Hints.cs`).
- A seller's first visit (with tips on) is a lesson (`Lessons.cs`): a chest that guarantees what it teaches, and
  steps whose `Done` checks run every frame on the hearing, the slip and the visit. Keep checks to things the player
  does, so they can't miss; the sim's lesson test plays each one. A lesson waits for tips already up
  (`MentorView.ShowLesson`), `CounterPhase.Teach` holds other tips back during one, and `Lesson.Covers` lists the tips
  it makes redundant. Read a guided `--autoplay` run's `[Bram ...]` lines to check the tutorial reads in order. Bram
  gives one tip at a time (`MentorView.Teach`): a tip that comes up while he's saying something else isn't marked as
  taught, so it comes up the next time it applies; call tips in order of what matters most. What Bram has taught is
  kept in the profile (`Profile.Taught`, `GameState.Taught`) the moment he says it (`Main.NoteTaught`; a lesson once
  its seller is at the counter), and merged into a continued run (`GameState.Remember`), so nothing is taught twice on
  a profile; the scripted first day (`GameState.FirstDayScripted`) teaches three customers, and Bettany (passing) opens
  the second. Clicking whoever's at the counter (or the notebook button, or N) opens the notebook on their page, framed
  (`Visit.LookedUp`); Mother Sorrel's lesson teaches it (the `lookup` tip for the others).
- Heartsongs (`Heartsongs.cs`, `Category.Heartsong`, `Rarity.Mythic`, red): nine collectibles, about a third of one a
  run while the debt lasts (v22; 0.30's longer season at 0.16% a chest, `--sim=songcount`), in any seller's chest
  (`Heartsongs.Chance`, own dice, rolled in `GameState.Sing` after the seller's priced
  it: no seller ever hears one, `SellerDef.Notices`); each one kept makes the next a quarter as likely (`Heartsongs.Fade`,
  `GameState.SongsKept`). Each sings its own synth song (`Sfx` "song_{key}"), at the gentlest shake, never drowned or
  stilled; worth nothing in gold (`Heartsongs.BaseValue` 0: never sold, never priced, never pawned). Its reveal
  (`SongReveal`, at an opening or in the counter) stops everything with what it does on display until the player keeps
  it (free, no slot, `GameState.TakeSong`) or declines it (`LetGo`: it's in the songbook either way); passing on a
  chest you can't afford with one singing in it bursts it, and the song alone comes out (`GameState.Bursts`/`Burst`).
  The careful player pays up to a working purse over a chest's contents for one (`Strategy.Worth`). Each does one big
  thing on display (`GameState.Shows`): they're the rarest things in Aldmark and each should be worth a run built
  around it (`--sim=songs` shows what each is worth from day 1; none shows what a chest is worth). Luck
  (`Chest.Improve`) never touches one.
  The profile's songbook (`Profile.Songs`, merged into `GameState.Songbook`) changes the Crown Chest's ending with all
  nine (`Story.VaultEpilogue`). Iterate `Categories.Kinds` (not `All`) where the six ordinary kinds are meant.
- The shop's power index (`GameState.PowerIndex`, `Build.cs`, v21) is the economy's one model: what an ordinary thing a
  seller brings fetches here against its plain worth, averaged over the shop's kinds (`Market.ValueShares`) at its
  typical colour (`ShopTiers.Typical`: common to purple). In it, in full: the buyers' trades and the Dragon's Egg, an
  active synergy's sale bonus, the trades' curios and the gallery lamps; luck (restorers, the Lucky Shop, luck curios)
  at half (`LuckPricedIn`), a day's or a week's first chest weighted by how often. Out: what no seller can price
  (enchantment, false bottoms, wards, Heartsongs in a chest, the shooting star), fees and bonuses for a perfect read,
  price cuts, and today's event. Every ask is multiplied by it (`PriceFactor`, with the sharp sellers, the Lullaby Box,
  the estate sale, the horn and the crescent; the seller's markup, the dice and the colour stay their own), so ordinary
  growth makes the numbers bigger and leaves the careful player's margin flat (the sim's margins report: check it holds
  when anything is added to the index or left out of it), and a build's edge is what isn't priced. With every upgrade
  it's about 2.1 at the Emporium (0.30: 2.7 when the buyers' trades were bigger); the last due (12,000g since 0.30,
  45,000 until then) is set above what that earns on a careful margin, so carrying it takes a build or tales gone well. Bram's working purse (`GameState.WorkingPurse`: two typical chests, `Market.TypicalAsk`, at
  `PriceFactor`) is the day-end advice, what `Strategy.Reserve` keeps, and the line a purchase warns before crossing:
  `ReserveIf`/`ShortNote` work out the purse after the purchase (its effect on the index; the bigger shop for a move),
  and DayEndPhase and PeddlerPhase show a red price and a confirmation. Route anything that changes prices through
  `PriceFactor` and anything that changes what ordinary things fetch through the index, deciding whether a seller could
  know of it.
- Curios are the run's build (`Curios.cs`, `Build.cs`): 79 of them, each bending one rule, each tagged with the ways of
  trading it rewards (`CurioTag`); six are the main builds, each a different way to play (`Curios.Main`: enchantment,
  sight unseen, haggling, the perfect read, false bottoms, wards), each with a way in, a multiplier, an engine and a
  legendary capstone. Rarities (`CurioRarity`: common, uncommon, rare, legendary) are rolled per card by week
  (`Curios.Rates`: rares from week 2, legendaries from week 3) and gated by shop (`RarityTier`); a legendary opens for good
  by a feat across runs (`Feat`, `Curios.Feats`, `Profile.Feats`, banked in `Profile.Bank`). Price: a fixed sum by
  rarity plus a share of the purse Nim found you with (`Curios.Price`, `GameState.PeddlerPurse`). Nim comes every night
  from the third (0.28: after the day's end and the Guild, `GameState.StartNight`, `PeddlerPhase`, `Curios.FirstNight`) with one curio (`Curios.Laid`), one
  more for each of his own upgrades, which he offers at his table (`Upgrade.NimCards`: his lantern, stool and standing
  order; `GameState.PeddlerCount`), with the curio cases; there's no "show me something else". Your curios sit under his
  cards each framed in its own rarity's colour, as his cards are, and with no room left his prices still show, on
  greyed-out buttons, with how to make room (0.30). Nim builds with you
  (`Curios.Stock`): curios sharing a tag four times as likely, one for the way of trading of the curio you got
  last once he lays out two or more (`Leaning`; two once you keep two of it, `Building`), one that opens at a grander shop on his first visit there
  (`PeddlerTier`), legendaries only for a way of trading you're building. There's no named rule since v21: a curio
  reaches everything in a chest, and the power index keeps the balance; wards still need naming (that's their puzzle).
  A sight-unseen chest is one bought without a shake, a drop or the lodestone (`Visit.Handled`, set by
  `CounterPhase.Handled` and the sim's `Deal`; `Purchase.SightUnseen`). Each curio is held to its rarity's budget as Nim
  would bring it (the builds report), and a way of trading's curios should multiply each other. The slip values things
  through the shop (`Appraisal.Model`, `ISlipModel`), so the odds bar is honest about a build. Luck runs in one order
  (`GameState.Rules`, `Turn`) both as a roll at purchase and as a transform of the odds. A new curio needs its tags,
  rarity and unlock, a `Strategy.CurioWorth` (about a fifth of what it adds as Nim brings it), its picture (an entry in
  `art/tools/assets_curios.py`, made like the rest of `art/generated/`; a placeholder in `CURIOS`,
  `tools/custom_art.py`, until then), its effect wherever the rule lives (`Build.cs` for luck, value, prices and the counters), and
  a decision about the index. Never let a curio tell you what a chest holds.
- Synergies (`Synergies.cs`) come from what's been acquired (six of a kind at a quality per level) or achieved (perfect
  reads, surprises, hidden things found, bargains, enchanted things); only active ones (one slot, more with upgrades)
  do anything, chosen at the end of a day. Each is a real power: its effect and 20% a level on what its kind sells for
  (`Synergies.SaleBonus`, priced into the power index).
- The economy grows fast and the Guild's dues grow faster (v18: the old tight constraints made upgrades feel optional).
  Qualities are worth 1/2/4/8/24 times a common (`Categories.QualityMultiplier`; 1/3/10/30/100 until 0.30, when each
  step inward was three to ten times the last and the owner found the numbers ran away); each move inward multiplies what
  a customer is worth a few times over (the sim's "profit per customer at each shop": about 7, 12, 70 and 490g).
  **Gold is rare** (0.30): a chest's own roll makes a thing gold only `Categories.GoldShare` (6%) as often as the spread
  says, and luck never makes gold (`Categories.Lift`: restorers, curios, a dry spell, a hoard's better things stop at
  purple); luck that would have lifted a purple thing past purple makes it shine instead (enchanted, `Build.PastPurple`,
  `Item.Shone`: read as the plain thing it was, "luck made it shine!"), priced into the index and the slip
  (`ShineShare`). A careful run finds about two gold things, a third of runs none (the sim's "gold things a run").
  The luck curios and restorers were made about twice as likely to step (a step is worth twice, not three times), and
  the Master Restorer mends (nothing worse than its chest) rather than lifting. buyers' trades (`Upgrade.Sales`:
  scales, auction block, royal warrant) multiply what everything sells for, and restorers (`Upgrade.Luck`: the bench,
  the workshop, the master) make everything likelier to come out a step better; both go into the power index. Flat
  sums (door money, a catalogue's fee) grow with `ShopTiers.Scale`. The careful player saves for the next move before
  anything else (`Strategy.Saving`), except curios. The Bookkeeping Engine (`Upgrades.Engine`) is the Guild's: offered
  once `CanPayOff`, for exactly `DebtLeft` (`GameState.Cost`), it marks every chest's real contents on the slip as it
  comes in (`Appraisal.Recorded`, `CounterPhase.Record`), so a won run can be finished fast; the sim's careful player
  pays off instead.
- Warded things (`Chest.Warded`, `SellerDef.Wards`, `Market.WardOne`): one thing a chest's last owner warded never
  sounds, weighs nothing, isn't counted by a ticket or a bell, and is never priced; it comes out on opening only if the
  slip named it (`Chest.Names`: one more of its kind marked than the chest holds, no kind over-marked, no false false
  bottom), with the reveal the false bottoms used to have (`OpeningPhase.Unward`), and the gut counts it. Each is
  pointed at by a stated clue (`Clue`, carried by the visit as `Visit.Says`, made by `Market.MakeClue`): Sorrel's
  dream sign, Hesper's dowry custom, Ilse's tag, Isaura's riddle, Quire's faded roll, Skrae's echoes, Zell's cards,
  and the Sleepers' Moon's dreams and Wardens' Day's riddles for everyone. `Strategy.WardedKind` is the careful
  player's solution: each is a deduction (a kind named that you can't hear), never a guess. A new puzzle seller needs
  its clue there, a `Visit.WardHint` line, a lesson and a portrait.
- The run (`Run.cs`, a partial of `GameState`): seven weeks of five days (0.30; five until then, eight of three in 0.20); every day has a
  role (`DayRole`, `WeekPlan.Role`), planned a week ahead (`Events.Plan`, `GameState.Plan`/`NextPlan`) for the notebook's
  calendar (`NotebookView.BuildCalendar`, with the Guild's debt across its top): the fifth is the Gilded Guild's tithe (`Guild.Due`: 20, 60, 160, 450, 1,300,
  4,000 and 12,000g, about three times a week with no tenfold jumps, then endless x1.5; Vane introduces it on the second
  evening, `GameState.IntroDay`); a story week's first day is the story's (`Story.TellsOn`: weeks 1, 2, 4 and 6 (the
  gathering: who stands with you), and the week after the debt; `GameState.StoryToday`); the moves come about days 8,
  16 and 25 (`Upgrades`: 80, 300 and 3,000g); Nim comes
  every night from the third (0.28); and two days have an event (`Events.PerWeek`; gentle ones in the first week). Pawning, Bram's word once a run,
  foreclosure; commissions (`Commissions.cs`) pay for each find as it's handed over, always at least a quarter over
  what it would sell for (`GameState.CommissionPay`, `Patrons.Premium`), so delivering always beats selling; the story's scenes and the shards of the Aldric Key that open the Crown Chest
  (`Story.cs`; in code still `MapPieces` and the vault, so saves carry over). Balance is set for a career, not
  a run (the sim's "Learners" and "Careers" sections; learners are the careful player with mistakes that shrink as
  `Style.Skill` goes from 0 to 1): a first-timer gets through the first weeks and loses in the middle ones; better
  judgment pushes that later, and the run's build and its tales decide the rest (0.30, 300 runs: the careful player
  buying curios as they come clears a little under three times in five, 59%, the tales' consequences carrying it
  further than 0.28's 47%; with no curios about half, 51%; with no tales a third, 32%; with no Heartsongs about half; a
  learner at 0.75 a third; a first run about one in a hundred); careers first clear around the sixth run (a quarter of
  players by the fifth), clear level 4 in the late twenties, and each level after costs a few points more
  (the dues 15% up a level, `Challenges.DebtStep`; one career in five has cleared level 6 by run 30, level 8 is a wall). A run's length is part of the
  balance: `--sim=experience` should keep a first run under two and a half hours (0.30: 146 minutes over 30 days, the
  longer season and its tales; 0.28: 121 over 25) and no day over its load (6; the move to Thornwick still is, for
  about half of first runs, as since 0.27). `--sim=kin [--who=KEY]` runs just the characters' table, for tuning one. Careless styles (buying blind, one shake)
  are foreclosed within a few weeks, and no early week sends careful players to Bram's word (0.30: the fourth does for
  about one in twenty-five, the fifth and sixth for about one in twelve: the ones still a shop behind). Anything added to the run goes in `SaveData` too; the profile across runs is
  `Profile` (user://profile.json). Bram's stash (`GameState.Stash`, twice a run) tops up a day that starts
  nearly broke. Bram's working purse (`GameState.WorkingPurse`) is the day-end advice, the purchase warning and what
  `Strategy.Reserve` keeps, so the advice is what the sim's careful player does: tune them together. The whole debt
  can be paid off early (`GameState.DebtLeft`, `PayOff`) from the day end or the Guild's night; the sim's careful
  player does, and reports the week each run cleared (a strong run clears in week 6; week 7 is the wall for the rest). Surprises (`Categories.Odds`, about one thing in eight a step better; the restorers and luck curios take
  a careful player to one in four) are worth about a sixth of what the chests hold (0.30: a step is worth twice, not
  three times), with a pity counter
  (`GameState.DrySpell`); the sim reports their share of income.
- Across runs (`Meta.cs` for the definitions and the score, `Kin.cs` for what they do to a run): four of Bram's kin
  (`Characters`), each changing one part of how chests are judged or the shop is run and paying for it (Hesk: no hard
  shake or drop, a scale; Rosa: everybody haggles, walkouts stay away a week; Isolde: a shelf and door money from day
  one, no pawning her collection); and eight challenge levels (`Challenges`, opened per character by clearing at the
  level below, a quarter more to the score each). A level raises every due by `Challenges.DebtStep` and deals the run
  that many banes and as many boons (`TermDef`, `Challenges.Draw`: nested, so a level's draw is the one below's and a
  pair more, and never a boon with the bane it contradicts; a real run draws from `Profile.TermsSeed`, so the setup
  screen shows what will be played, and keeps them in `SaveData`). Banes make the trade harder; boons smooth over what's
  most frustrating about it (a slow start, a short night, the clock, a slipping lock, a curio you can't find). A term
  is a rule that holds every time, with a reason in `docs/world.md`, routed through `GameState.Under`/`Favoured` where
  its rule lives; the setup screen and the pause menu list a run's terms (`UiKit.MakeTermList`). `--sim=terms` measures
  each alone and the levels, `--sim=levels` just the levels as drawn (`--debtstep=S` tries another step): keep each
  term within several points of the others and every level harder than the one below. Every run starts the same: nothing carries over that makes a run stronger (v22 retired the talents bought
  with renown: Bram's Kin's 10g, Familiar Face's fourth card and Second Look's reroll are everyone's now,
  `GameState.StartingGold`, `Curios.Laid`, `RerollPeddler`; Mara's Favour is the Guild Promissory Note, a curio; the
  rest were curios or upgrades already, or went), only what's known (the journal, what Bram has taught, Nim's stock and
  the legendaries' feats). Characters add no share to sale prices and take none off dues (the shop's growth is
  upgrades', synergies' and Heartsongs'). A run's score is its points times the level's multiplier (`Score.Final`,
  each character's best in `Profile.BestScore`). Bram's lessons are
  Kit's (`GameState.GivesLessons`); a new character gets a `kin_*` tip. Route seller traits through
  `GameState.MindsDrop`/`MindsMagnet`/`MindsHardShake`/`Impatient` and upgrade prices through `GameState.Cost`, so the
  terms apply everywhere. The profile (`Profile`) keeps each character's open levels and best score.
- The shop opens up a step at a time, moving inward (`ShopTiers.Name`: Bram's Stall, Thornwick Market, Crownhold, the
  Royal Emporium; 4, 6, 6 and 5 customers a day, `ShopTiers.Customers`; `GameState.UpgradesOpen` once it has earned
  `UpgradesAt`, `DisplayOpen` with Thornwick, `SynergiesOpen` with Crownhold), each introduced by a Bram tip when it
  happens. Gate new
  systems the same way, in Core, so the sim and the bot follow them.
- Each shop tier is a stack of layers (`ShopView`, made by `UiKit.MakeShop`: the sky for the hour, the place, the
  shelf, the frame and its lanterns, the counter and the tools you've bought as props, each at 4x; the hour follows the
  day's customers in thirds, night for the Guild); keep the counter top at y 476 and the shelf at y 196. Story
  scenes have their own (`scene_{bram,guild,keyhold,vault}`, `ScenePhase`'s backdrop, `Story.Setting` for a week's).
- What Bram's kin know is kept in two books (0.25: the ledger had grown too many pages). The **notebook**
  (`NotebookView`; N, the button on the slip, or clicking whoever's at the counter) is for trading: Sounds, Regulars,
  Calendar (with the Guild's debt: what's paid, what's left, what's next; clicking the due in the top bar opens it there,
  `Main.ShowDebt`, wherever a screen can, `Phase.ShowDebt`) and Synergies. The **collection** (`CollectionView`, from the
  title screen and the pause menu) is what's been gathered: Finds, Curios and Heartsongs, read from the profile and the
  run being played (`Core.Collected`). Both are kept across runs (v21): regulars met, finds (and enchanted ones) and
  curios seen are in the profile (`Profile.Met`, `Found`, `FoundEnchanted`, `CuriosSeen`) and merged into a run's
  `GameState.Known` by `GameState.Remember` at the start and on continuing; the run's own `Found` still scores and pays a
  catalogue. What any run has learned stays in the notebook (v22): its sounds and regulars go as far as the grandest
  shop any run reached (`GameState.JournalTier`, `Profile.BestTier`), and the Regulars page has a Friends & foes spread
  (Uncle Bram always; Nim, Mister Vane, the Hooded Woman and Mara as they're met, `GameState.People`/`Profile.People`).
  The Regulars page is a list with the open page under it; it opens on whoever's at the counter, framed. GM mode
  (`Settings.Gm`, `--gm`) puts a panel beside the pause menu: +99,999 gold (`Main.GmGold`), skip the day
  (`GameState.SkipRestOfDay`), fill the notebook and the collection (`KnowAll` on the run and the profile).
- The game opens on `TitlePhase`; `Main` saves `GameState.Save()` to user://save.json at the start of each day (not
  for the bot or debugging flags) and the pause action (Esc, Start) opens `PauseMenu`. A day: `Main.StartDay` (the
  week's scene, Nim), the counter, `DayEndPhase`, then `Main.CloseDay` (Vane's introduction the second evening,
  `DuesPhase` on the fifth). Anything added to `GameState` that should survive a day boundary goes in `SaveData` too.
- Files (0.27): everything the game keeps goes through `Storage` (`Read`, `Write`, `Delete`): a write goes to a temp
  file, flushed, then replaces the old one, which is kept as `.bak`; a read falls back to the `.bak`, and a file that
  can't be read is set aside (`name.unreadable-<time>.json`) and named on the title screen (`Storage.Notices`), never
  deleted. The user folder is `Keywright` (`config/use_custom_user_dir`: `%APPDATA%/Keywright` on Windows, so Steam
  Cloud can name one place), and `Storage.CarryOver` brings files from Godot's old app_userdata folders. `SaveData`
  (version 4) and `Profile` (`Profile.CurrentVersion`) carry a version: a file from a newer game is left alone, an
  older one is migrated in `FromJson` (`Categories.Renamed` only for saves up to version 2, from LootFall 0.17).
- `Settings` (user://settings.cfg; `SettingsView`'s pages Display, Sound, Controls and Game) holds the window
  (windowed, fullscreen or exclusive, a size that fits the screen, pixel-perfect or smooth scaling, vsync, a frame
  cap), screen shake and flashes (route new screen effects through `Main.Shake`/`Flash`), the outlines, the volumes
  (the `Effects` and `Songs` buses, under Master: a Heartsong's "song_" sounds go to Songs), muting and pausing when
  the window loses focus, tips, language and the controls. `Settings.ApplyDisplay` changes the window through the
  root `Window` node (its `Mode`, `Size`, `Position`), never the display server alone: the node works out the
  stretch and the mouse from its own size.
- Languages (0.27; translated in 0.29 into French, Japanese, Simplified and Traditional Chinese): everything the
  player reads goes through `Loc` (Core) where it's written: `Loc.T("Pass")`, and `Loc.T($"Day {day} of {days}")`,
  which is looked up as one sentence with numbered holes ("Day {0} of {1}") so a translation can move them;
  `Loc.Plural(n, $"{n} week early", $"{n} weeks early")` for a count. Data tables are translated where they're
  written, so the language is set once, at start-up (`Localization.Apply`; a change in Settings takes a restart;
  `--lang=fr` plays one run in a language). A name that's also a key, kept in saves (a thing's `Names`, `FakeName`, a
  Heartsong's `Name`), is `Loc.Key` in the data and `Loc.T(name)` where it's shown (`Item.Name` does; `Entry` is the
  key); `Categories.ChestSkin` is the art's name for a colour and `ColourName` the player's. Never compare or look up
  by a text the player reads (compare keys, or `Loc.T` both sides), never cut or patch a translated text (no
  `.Replace("a ", "")` on it), never join an interpolated piece to a plain one (`Loc.T($"... {x} " + "...")` is a
  finished string by the time it's looked up, so it's never found: make every piece `$"..."`; `tools/loc check` says
  so), and mark English that must stay English (keys in a list) with `// loc: keys`. Words the
  code joins are joined by a text too, so each language punctuates its own way: sums as the interface writes them
  `Money.Short` ("{0}g"), as people say them `Money.Say`; lists `Words.List`, `Sellers.Join`; a name and what it does
  `Loc.T($"{name}: {blurb}")`. A word slotted into a sentence must read right in every language (French adjectives
  agree in gender and number, Japanese counts differently), so where the count or the case changes the words, write
  a text for each (`Categories.Echoes`, the dowry's line in `Visit.WardHint`) rather than "{0} {1}". `tools/loc`
  (Roslyn) wrapped the strings once and now checks, extracts and verifies them. Translations are gettext files in
  locale/ named for their locale (fr.po, zh_TW.po) from keywright.pot; drop one in and Settings offers it
  (`locale/README.md`); `locale/GLOSSARY.md` has each language's words of the world and voice, so a new text uses
  them. Kenney Mini covers English and western Europe (with oe, the en dash and French's no-break spaces added by
  `tools/fonts.py kenney`); Galmuri 9 (OFL) the rest of Latin, Cyrillic, Greek and Korean; Japanese and Chinese are
  drawn in Fusion Pixel 12px (OFL), cut to each language's character set and every character its translation uses
  (`tools/fonts.py fusion DIR`, from the font's release: run it again when a translation changes, `verify` names
  what's missing), on a 12px grid (`UiKit.SnapFont` rounds down to it: 16 is drawn at 12, 32 at 24), so a layout that fits
  English can still need a size chosen by measuring (`UiKit.TextWidth`), not by counting letters.
- Steam (0.27): `Platform` (scripts/Platform.cs, Steamworks.NET 2024.8.0 with SDK 1.60's library in steam/) starts
  with the game and does nothing without Steam or its library ("Steam: not running" in the log): achievements, rich
  presence (`Platform.Presence`, worded by steam/rich_presence.vdf), and the overlay opening the pause menu.
  `Platform.AppId` is 0 until there's a store page. Achievements are Core (`Achievements.cs`: each a condition on
  the run or the profile), checked whenever `Main.WriteProfile` runs and at each day's start, kept in
  `Profile.Achievements` (so they're never lost off Steam, and reach it later), listed on the collection's
  Achievements page, and announced over the top bar when Steam isn't there to announce them; `--achievements` prints the table for Steamworks. A new one needs its API
  name added in Steamworks too (steam/README.md has the rest of the Steamworks side).
- Releases: the version is project.godot's `application/config/version` (`Main.Version`, shown on the title screen
  and stamped into the exe); bump it for each release. The title's Credits (`CreditsView`) list the game's credits
  and the notices Godot, Galmuri and Steamworks.NET ask to travel with it; set `CreditsView.Studio` to the name the
  game's published under. CI (`.github/workflows/build.yml`) builds Windows and Linux on every push.
- Input (0.27): the game asks for an action, never a key (`Controls`: `act` sets a pin, hurries an opening, reads the
  next line; `force`, `notebook`, `pause`, `back`). Each has keys and controller buttons, rebound on the Controls page
  (a key another action had moves to the new one, and an action left with none takes the old one; the menu and Back
  share Esc) and kept in settings.cfg's [controls]. Prompts name what to press through `Controls.Tag`/`Hint`/`Fill`
  ("[N]", or "[Y]" once a controller's in use, `Controls.UsingPad`; Bram's tips say "{act}"). The mouse needs no
  binding, and a controller drives the pointer the mouse uses (`GamepadCursor`: the left stick moves it, faster
  while A is held so a flick shakes hard; A is the left button, by `Input.ParseInputEvent`; the D-pad nudges, the
  right stick scrolls), so anything the mouse does, a controller does.
- What a tale leaves behind (0.30; `docs/stories.md`, "What a tale leaves behind"; `Consequences.cs`): who comes (and
  who never does again: `Goes`, and their tools with them), officials who come only while their masters count you a
  friend (`SellerDef.Comes`: Bailiff Marrow, a third of his seized chests cold; Quartermaster Brandt, by the pound),
  better terms from friends (`GameState.Goodwill`, said under the chest: Wynn's neighbour's price, Nettle's and Sir
  Rowan's rules waived, Durgan's dwarf price, Caskett's tolls), favours called in on a short Guild night (`Owed`,
  `CallIn`: an eighth of the due each, once a season; `DuesPhase` shows them first), papers for the King's court
  (`Evidence`, `TaleBook.Paper`: four put the Guild on trial, half its tithe, `OnTrial`), contests (`Visit.Contest`: a
  gift chest whose perfect read wins `Prize`), Mother Tansy's loan (`Borrow`, collected the next week's first morning,
  `CollectLoan`), effects that last a while (`Until`, `While`: a rival, pickets, a haunting, a crowd; `TaleCustomers`),
  nights away (`GoOutTonight`: no Nim), a shard of the Key by another road (`GiveShard`), and the Maw's bargain
  (`TakeMawBargain`, `GateOpens`: the debt paid for the Key; not a clear, none of the debt's points, THE GATE OPENS).
  The ending reads them: week 6's gathering and the Crown Chest night name your allies (`Story.Allies`), and after the
  run's last scene, won or lost, the season's end tells what became of everyone (`Story.SeasonEnd`, each tale's
  `TaleDef.Ending`, shown once a run by `Main.EndRun`). 0.30's tales are in `TaleBook.Brindle.cs`, `.Thornwick.cs`,
  `.Crownhold.cs`, `.Emporium.cs` (built by methods, so the partial files' order never matters) and their endings with
  0.28's in `TaleBook.Endings.cs`; a season draws the Shut Chests and the King's Court always, three of Brindle's seven,
  three of Thornwick's seven, three of Crownhold's eight and both of the Emporium's. A new tale needs an `Ending` too.
- The people and their tales (0.28; `docs/world.md` has the order of things and the cast, `docs/stories.md` every tale
  beat by beat). The cast was replaced: every regular is someone with a place, a faction (`SellerDef.Of`), a want
  (`Wants`) and things you notice about them (`Facts`, the Magpie's suspects' clues), keeping an old regular's trading
  rule and, until drawn, their figure (`Look`; `Sellers.LookOf` everywhere a picture is loaded); old keys are mapped
  (`Sellers.Renamed`, read through by `SaveData.Renamed` for saves (version 5) and the profile (version 2)). People a
  tale brings who don't trade are `Sellers.People` (Hollis, Tobias, Aubel...). A tale (`TaleDef`, `TaleBook.cs`) is
  beats (`Beat`): who comes, after which beat and how many days, at what shop, if what (`If`), and either a talk
  (`Says`, `Asks`: `Choice`s with a cost, a doing that returns the reply, and a `Worth` for the careful player) or a
  chest (`Brings`, with `Bought`/`Passed` hooks). The engine (`Tales.cs`, a partial of `GameState`) draws a season's
  tales from the seed (`TaleBook.Draw`, above), stages at most two beats a day (`StageTales`: a talk joins the queue as a `Visit.Talking`, a chest
  replaces a regular) and one a night (`StartNight`), and keeps what came of them: `Flags`, `Gone` (who never comes
  again), `Notices`, `TaleLog`, `Keepsakes`, all in `SaveData`. A tale's secret (who the Magpie is) is drawn from the
  seed (`TaleBook.Secret`) and never changes; nothing in a tale is a hidden roll. Standing (`Faction`, -3..+3,
  `GameState.Sway`): at +2 and -2 the commons add or take a customer a day, the Guild takes a tenth off or on every due
  (`DueFor`, with the King's decree), the Wardens warn or search (`Wardens.Search`), the Lockless pay the fence double
  or rob (`Wardens.Robbery`); the rest price their people's tools (`StandingPrice`). Talks play in `TalkPhase` (counter or
  night), answered in the sim by `Strategy.Choose` (the most `Worth`, kinder on a tie). Tools are offered at the counter
  by the people who make them (`Upgrade.By`, `Offers.cs`; not on a move's first day; turned down, back in
  `GameState.OfferAgain` days once the morning's purse could pay; `Strategy.WantsOffer` decides as the day's end would); the day's end keeps the building. Stolen chests
  (`Chest.Owner`, `SellerDef.Steals`: Flick mostly, Corvin now and then, tale chests): the mark goes cold at the first
  touch (`CounterPhase.MarkAnswers`); from Thornwick you can send for the Wardens (`GameState.CallWardens`: the seller
  runs, `VisitOutcome.Reported`, a finder's share); bought, it won't open and waits in the back room (`BackRoom`,
  `Held`) to be returned (`ReturnHeld`) or sold to Nim's friends (`FenceHeld`), at Nim's table. A gift (`Visit.Gift`,
  a tale's chest given freely for a lucky coin) opens, and no seller's terms apply to it. The sim's "Tales" section
  reports what a run holds and what came of each tale. A new tale needs its beats in `TaleBook.cs`, its people (and
  their `Look`), its choices' `Worth`, its flags read wherever their effect lives, and its page in docs/stories.md.
- Every seller has one way of judging a chest (`Judgment`: listening, counting rattles, weight, colour, a dream, a
  trade they know, no idea) with a blind spot a careful player can see through, one way to trade,
  places their chests come from (`From`, `Origins.cs`: the place decides what a chest tends to hold) with a story for
  each (`Stories`, always true), and maybe one rule of their own that changes what you can do, what you're told or what
  the deal is (`Patience`, `HatesDrop`, `HatesMagnet`, `HatesHardShake`, `ShakeLimit`, `ShakeFee`, `Packs`, `Packhorse`,
  `States`, `Wards`, `Wager`, `Heirloom`, `TakesDearest`, `SlipPrice`, `Gaze`), worded under the chest by `GameState.Terms` (and
  `Visit.Clue` for what they state). `Strategy.Reckon` is the careful player's read of a stated clue. A rule must
  change what a careful player can work out or do, every visit: one a tool makes moot goes (v19 replaced the apprentice's and
  the noblewoman's sense for magic, which the witchlight made pointless, and Skrae's "hears all but magic", with puzzles).
  `Visits` sets how often they come (the Maw rarely). `Market` rolls and prices their chests; `Visit` trades, and its
  `Explain` says on the results and pass screens why a deal went the way it did;
  `Shop.Buy` takes a seller's due (an heirloom bought back, the devil's dearest thing) and `Shop.Settle` a wager or the King's game. A new rule needs the sim, the bot
  (`Strategy.Levels`, `Worth`, `WorthBuying`) and a lesson. The sim's per-seller table must show a careful player
  profits from each, and buying blind shouldn't (Pip, who takes a few coins, and the Maw, whose coin costs the rest of
  the day, are the exceptions; Flick's profit is the Wardens' finder's shares). Stories say how they shook it and what they state, never what they heard (`{heft}`,
  `{colour}`, `{magic}`, `{articles}`, `{pounds}`, `{dream}`, `{clue}`, `{price}`, `{fee}`), and must fit the speech
  bubble (about 170 characters with the price filled in).
- `ShakeBox` gates sounds with the same rules as `Chest.Audible`: change both together (the shake test checks them).
- UI is built in code with `UiKit` and `Painter` (absolute positions on a 1280x720 canvas), not in .tscn files.
- Buttons use `FocusMode.None`: the act action (Space) is reserved for lockpicking and holding the chest open. The
  notebook action is N (L too, as it was when it was the ledger) or Y.
- `Hearing.Read` and `Strategy` model a careful player (what they hear, when they drop or use the magnet, what they
  offer and keep) for the sim and the bot. Update them when a rule, tool or trade changes.
- Phases are swapped by `Main.SwitchTo`; check `Alive` after every `await` in a phase. Quit through `Main.QuitGame`
  (it frees the phase and bot and releases the sounds first), never `GetTree().Quit()`: long runs crashed on exit
  otherwise, and short ones leaked.
- Screen effects go through `Main` (`Shake`, `Flash`, `HitStop`, `Zoom`); capture points use `Main.Moment(name)`.
- Godot C# naming differs from GDScript in places (e.g. `CpuParticles2D`, `AudioStreamWav`).

## Art direction (pixel art)

- **The 3px art (0.31).** The game's pictures are `art/density3/assets/` (`README.md` there, in Chinese;
  `approval.json` ties the owner's approval, 2026-11-04, to every file's SHA-256): round 2's pictures remade from their
  full-size originals so a cell of the art is 3x3 screen pixels at 2560x1440 (round 2's 4x grid was 8 there), faces
  kept whole and natural (`face_protection.json`), no outlines. `tools/import_art.py` lays them over `assets/` after
  building the rest (`--density-only` just that, `--density-check` checks without writing; `art/tools/density3_check.py`
  checks the grid, sizes and hashes), each texture at its 1440p size (the world's layers 2560x1440, figures 768x1280,
  things, curios and icons at 4x their art, head crops at 2x), and writes `assets/art_density.json` with each one's
  logical size (the art size the layout below was made for) and physical size. **Load every picture through
  `ArtTexture.Load`** (`UiKit.MakeSprite`, `MakeBackdrop`, `MakeItemIcon` and the rest do): it returns the dense texture
  reporting its logical size, so every position, scale and click area stays round 2's; a plain `GD.Load` of one draws it
  two to eight times too big. Anything not in `art_density.json` (the chest, effects, the UI frames, a placeholder)
  loads as it is. The game draws at the window's resolution (`canvas_items`), so a cell is exactly 3 pixels at 1440p and
  a fraction more or less elsewhere (2.25 at 1080p, 1.5 at 720p), the exception to square pixels the owner chose; a run
  holds a few hundred MB of these textures (`ArtTexture` keeps what it has loaded). The full-size originals live
  outside the repository, in the art pipeline's archive (`art/density3/README.md`), so a new picture at this density
  comes from there (`art/tools/density3.py`); until then a new figure or thing shows its round-2 picture or placeholder
  at the same size. `--density-review --shots=DIR` (under `xvfb-run -s "-screen 0 2560x1440x24"`, a new folder each
  time) captures every figure, each shop at each hour, the story scenes, the notebook, the locks and the upgrade cards
  at 1440p, for an art review. Whether the art is outlined is the art's to say (`art_density.json`'s `outlines`,
  `ArtTexture.OutlinesAllowed`): the 3px art isn't, so `UiKit.Outline` adds none, the counter's edge line stays hidden
  and the Outlines setting is greyed out with a note (off by default).
- Round 2 (0.25-0.30, `art/generated/`, still the layout's baseline and what the 3px art was remade from) was made to
  the brief in `art/` (`README.md` first). Round 1 (0.23) came out too busy to
  look at for hours; round 2 (0.25) redraws it and builds the shop scene from layers (Codex, on the branch
  `art/round2`; `ART_DIRECTION.md`'s "Why round 2" has the measurements and the causes). Its first anchors came out
  calm but a little too plain; after the style study (`art/study/`, `art/tools/study.py`: four styles at three levels
  of detail, as the counter screen) the owner chose C2 at 4x, with detail and without outlines (`art/study/CHOICE.md`),
  and everything was made that way. The generator never draws pixel art: it draws cartoon illustrations on magenta,
  and `art/tools/pixelize.py --mode flat --no-outline` makes the pixel art an area at a time, so a model's texture
  never gets in. The style: warm storybook cartoons in chunky pixels, cel-shaded, three or four readable features on a
  figure or thing, no outlines in the art, the 74-colour palette in `art/palette/`, lit from the upper left. Until 0.31 the game
  outlined what has to stand out itself (the owner's request, 2026-11-01; the 3px art turns it off, above): one pixel of `Palette.Outline` (`#1a1420`,
  the chest's own) outside every character's silhouette, full figures and the head crops cut from them
  (`UiKit.Outline`: a sprite of its own behind the picture), and a row of it along the counter's far edge
  (`ShopView`); things, props and the other layers have none, and nobody draws outlines into the art. Settings >
  Outlines (`Settings.Outlines`, on by default) shows or hides every one at once: they're all in the `UiKit.Outlines`
  group (`UiKit.ShowOutlines`), so anything new that's outlined goes in it too. The shop scene is layers (`art/tools/scene.py` has the layout): a
  sky per time of day (morning, midday, evening, night; the rest tinted to match), and per shop tier the place across
  the counter, the shop's frame (the top bar sits on its band), the display shelf, the counter, the appraisal sheet and
  lanterns. `ShopView` stacks them at 4x and lights them for the hour; full-length figures stand behind the counter
  (`SellerView`: short folk lifted, Vexis the imp in front of it, the speech bubble top left), props for the tools
  you've bought stand beside the chest, and the slip is printed in dark ink on the sheet (`Palette.Ink`,
  `Palette.OnPaper`). The queue, the notebook and Bram's tips use head crops cut from the figures
  (`assets/figures/layout.json`). `manifest.json` has
  every asset with its spec, prompt and limits (`calm`: lone specks, edges and brightness step, measured by
  `artlib.calm_metrics`; `value`, a layer's brightness; the layers' lines), generated by `art/tools/build_manifest.py`
  from `art/tools/assets_*.py` (edit those, not the JSON), and `art/tools/pixelize.py`/`validate.py` turn a generated
  image into a finished asset and check it (a busy one fails: regenerate it simpler, never filter it; `validate.py
  --sheets` stacks whatever's delivered into counter mock-ups at every hour; `art/generated/DELIVERY.md` says how each
  batch was made). The chest isn't
  generated: `tools/custom_art.py` draws it (`chest`, `chest_lock`: one silhouette for every finish), the bar for how
  simple a sprite should be. `tools/import_art.py` builds `assets/` from it (needs Pillow): it copies each picture at
  its art size, scales it by a whole number where a slot wants it pre-scaled (backgrounds and scenes drawn at 320x180,
  UI frames, curios, icons, the emblem and the Key), and cuts each portrait's face. Change the art there and re-run
  rather than hand-editing `assets/`. A new seller, thing, curio or upgrade needs its picture made the same way (its
  entry in `assets_*.py`, then generated, pixelized and validated); until it has one, `tools/custom_art.py` draws a
  placeholder (the old code-drawn art, which the importer falls back to, scaled to the same size, for anything missing
  from `art/generated/`). `art/.gdignore` keeps Godot from importing the folder. Sounds and the font are Kenney's CC0
  packs (only GitHub is reachable from cloud sessions, so new sources must be GitHub-hosted); credit them in
  `CREDITS.md`.
- Keep pixels square: integer scales only, nearest filtering (project default); the 3px art's textures are the one
  exception (above), drawn at their logical size. Art sizes and how they're shown: the
  world (round 2's scene layers, figures 96x160, props) on a 320x180 grid at 4x, the appraisal sheet 216x320 at 2x;
  story scenes 320x180, which the importer doubles to 640x360, shown at 2x; the chest 48 wide (lid 21, body 15, open
  30), built at 2x and shown at 2x (`ChestView`), with its lock maker's hasp and plate
  laid over it (`ChestView.Lock`) and its own finish for the Crown Chest (`crown`); full figures 96x160 at 4x at the
  counter and in story scenes, and their head crops, 76x76 at 1x in the notebook and Bram's tips and 38x38
  (`sprites/sellers/faces/`) in the queue, the notebook's rows and, at 3x, the character cards; things 32x32 (trimmed) at 2x on the table and the
  shelf, 1x in small slots; curios 28x28 at 2x; upgrade icons 24x24 at 2x (1x on the slip); kind icons 16x16 at 2x;
  UI frames 12x12 at 2x (`UiKit.Px`, 4-pixel margins); the lock faces 160x160 at 2x under the dial (its ring at
  `LockpickPhase.Radius`); effects at 2-4x (`Fx`); the logo 320x72 at 2x; the window's and the .exe's icons
  (`assets/ui/icon.png`, `icon.ico`, which the Windows export writes into the exe) from the app icon drawn for each
  size. Kenney Mini at multiples of 8px (`UiKit.MakeLabel` snaps sizes;
  min 16).
- Kenney Mini has ASCII and Latin-1; Galmuri 9 (`Localization.SetUpFont`) is its fallback for other scripts, and
  `--font-sheet --shots=DIR` shows every script the fonts cover. Keep the English text ASCII all the same: it's the
  source every translation starts from, and the house style.
- Panels and buttons use the 9-slice frames from `UiKit` (`MakePanel`, `MakeFrame`, `MakeButton`), not StyleBoxFlat.
- To show a particular thing in a slot (collection, shelf, exhibition, chooser) use `UiKit.MakeItemIcon`, its own
  sprite from `assets/sprites/items/icons/` at the largest whole scale at which any thing would fit (by the art's 30px
  box), so it looks the same everywhere and things side by side compare in size.
  Kind icons (`UiKit.MakeKindIcon`) stand for a kind, not a thing. Every upgrade has its own icon
  (`assets/ui/icons/{key}.png`).

## Known gaps

- Wine 9 in this container can't run the exported .exe. `.github/workflows/build.yml` checks the text and the build, then exports Windows and Linux builds and smoke-tests each on its own runner on every push; download builds from the run's artifacts.
- The shop saves at the start of each day only (leaving mid-day replays that day), and sellers don't remember you
  (see `docs/design.md`).
