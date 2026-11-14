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
   `-- --sim=trace --seed=N [--character=KEY]` prints one careful collector's run day by day (purse, customers, spending,
   upgrades, curios, the Guild, and each tale's lines as they happen) for tuning the economy's pace and following a tale
   through. `-- --sim=terms [--who=a,b]` (ten minutes) plays Kit with each
   challenge term alone, then the dues alone and whole levels as drawn; `-- --sim=levels [--debtstep=S]` just the
   levels, one to eight; `-- --sim=careers` just the careers (learners at the highest level open, run by run;
   `--level=0` keeps them at level 0, as a player following the story would), with the campaign's milestones.
   0.36: `-- --sim=learners` (two minutes) plays Kit at level 0 at six skills, a first-timer to the careful player, for
   tuning the dues; with any sim `--dues=a,b,c,d` tries other dues; `--sim=terms
   --level=N` weighs each term at that level's dues (at level 0 the careful player clears nine seasons in ten, so a
   bane alone doesn't show). 0.37: a sim's season is a first one unless told otherwise: `--veteran` plays it as a later
   season (Bram's drawer, Nim from the first night, commissions from the first week) and `--ways=N` with the trade's
   ways as far as market N (`GameState.Ways`: `--sim=learners --veteran --ways=4` is a later season's table); the
   careers and the experience report carry a profile from season to season, so theirs are later seasons by themselves. **Never run `dotnet build` while a sim or a bot is running**: the build replaces the
   assembly under it, and the run stops short without saying so.
3. Visual changes: run the autoplay bot under Xvfb with `--shots=<dir>` and look at the PNGs.
   Bot flags: `--seed=N --quit-at-end --days=N --no-tutorial --start-tier=N --gold=N --start-week=N --character=KEY
   --challenge=N --terms=a,b --heartsongs=P --songs=all --event=KEY --curios=a,b,c --gm --season=N --knew=all --veteran
   --ways=N --profile` (`--veteran` plays a later season, as a save that has played one would: Bram's drawer on the
   first morning, the short story scenes; `--ways=N` with the trade's ways as far as market N, so `--veteran --ways=4`
   at the stall has locks, false bottoms, pairs, enchantment and Bram's cradle from day 1; `--profile` uses and keeps
   the stored profile, so two runs in a row under one `XDG_DATA_HOME` are a first season and a second; `--terms` plays just those
   challenge boons and banes, by key, whatever the level deals; `--curios` owns those from the
   start, to see a build's screens; `--gm` turns GM mode on, for its panel beside the pause menu; `--event` makes every
   day that event, to check
   its screens; `--quit-at-end` quits after the first opening: use `--days`
   for longer runs; `--heartsongs=0.3` puts a Heartsong in that share of chests, `--songs=all` plays as if all nine were
   known). `--start-tier`
   jumps to a shop tier with the tools a player would own by then (use it to check later tiers' screens);
   `--start-week` fast-forwards a run with the sim's careful collector (curios, commissions, exhibition and all; `--gold`
   then sets the purse, e.g. 0 to see a short night and foreclosure; `--days` counts from day 1, so add the weeks
   skipped; it stops early, on the night the treasury's due, and goes straight to it, so you play that; a jump's season
   is banked in memory, so the season after its treasury remembers). The bot plays scenes, Nim, the Guild's night and deliveries, opens the short-of-chests warning once (and
   says "Not now"), keeps every Heartsong (after a look at its reveal), and quits once a run has ended (below). With tips
   on, every seller's first visit is a lesson, so a guided run at a tier shows those lessons; the bot logs any lesson
   it couldn't finish. `--opening-demo` opens a crafted chest with every quality and two surprises (record it);
   `--treasury-demo` plays the treasury on the season's last night (the first swallowing; `--treasury-demo=final` the
   final opening with the plan whole, then the next day; with `--season=2`, the remembered one, where you can stay away);
   `--engine` starts with the Bookkeeping Engine;
   `--sight-unseen` buys every other plain chest it can afford without touching it (the opening's blind-buy screen);
   `--mimics=N` makes one chest in N breathe from Thornwick on (a mimic at the counter, the opening and the market; they
   only breathe in a remembered season, so add `--season=2`);
   `--tales=a,b` puts those tales in the season whatever it draws (with `--start-week`, to check a late tale's beats);
   `--season=N` plays a remembered season from its first morning (as if swallowed at the treasury before) and `--knew=all` as if every Inquiry entry had been learned in an
   earlier season (the leads it opens, the beats that need it, and those who'd have told you noticing you know).
   At a run's end the bot retires (or goes back to the title) as a player would, so what comes between runs plays too.
   `--title-test` drives the title's burst with real input (skipping, pressing a landed button) and exits 1 on a failure.
   `--shell-demo` walks the title screen, its saves (0.37: the three cards, an empty one, the erase question) and its
   collection (finds, curios, Heartsongs, from a made-up profile), the character screen, a new game (Hesk, at challenge level 8: the most boons and banes the character screen and the
   pause menu ever list), the pause menu and its collection, and the settings (with a look at the scene with the outlines turned off, when the art has them; with `--gm`, the GM panel too: gold, the
   notebook and the collection filled, a skip refused mid-scene, and "Fulfil the quests", whose result it prints: the
   key whole, the plan whole, the debt cleared, the treasury due). A bot run shows the notebook's pages, clicks the
   Guild's due in the top bar once it's up (the notebook at the debt), opens the collection over the counter, and the
   echo horn's history once there's a good deal in it (`02_history`).
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
   failure. The full shake test takes about eleven minutes (`--n=4` for a quick look). Anything about how
   a shake is read from the hand: `xvfb-run ... -- --hand-test --no-tutorial --seed=5` (0.36's `--ear-test`) drives a
   pointer through the real counter the ways hands move (smooth shakes at each strength, a hard one with its wind-up
   and wind-down, a mouse that reports in jumps, twelve frames a second, an uneven pace, a lift, a carry) and checks which strengths
   each counted as tried (`Visit.Checks`: what Bram's lessons wait for, the hint under the chest and the opening's
   closed eye go by); exit 1 on a failure. Run it on its own (it needs its frames), and in the scaled window too (as
   the pad test, above: the software renderer drops to ten frames a second there, which is where 0.37 found a firm
   shake reading as hard). The bot and Bram's cradle shake at
   exact strengths (`ShakeBox.AutoShake`), so they can't show this: a change to `ShakeBox` or to how `CounterPhase`
   counts a strength (`TryTime`) isn't verified until this passes. The playtest tool's shakes (`tools/playtest/pt`:
   flips of 6, 38 and 95 pixels) are the same motions as the test's "mouse that reports in jumps": change them together.
7. Text: `dotnet run --project tools/loc -- check` must say every text is wrapped (it lists any new one that isn't),
   and `-- extract` rewrites locale/keywright.pot (commit it). `-- verify` checks each translation in locale/ against
   it (untranslated texts, holes and `{price}`-style names kept, plural forms, letters the language's fonts have;
   `--strict` fails on any of it): a text added or changed in English comes up untranslated in all four until it's
   translated (`locale/GLOSSARY.md` has each language's words and voice). `python3 tools/po_merge.py` (0.36) brings
   each translation up to the template after an `extract` (new texts untranslated, texts that have gone dropped);
   `--new` lists what's untranslated, and `--add FILE.json` writes translations in (`{"fr": {msgid: msgstr, or a list
   of plural forms}, "ja": ...}`). `godot --headless --path . -- --loc-test`
   runs the language layer through Godot with a made-up German translation, then loads each translation and checks
   it. `-- --sim=quick --pseudo` must print the same numbers as without (only the words change: a key that got
   translated shows up as a different number), and a bot or shell-demo run with `--pseudo` shows every translated
   text in brackets, so anything plain is untranslated, and a third longer, so a layout too tight shows. Any run
   with `--shots` also prints `[overflow]` for each label or button whose text outgrew the box it was placed in
   (`Debug.Overflow`): after changing a screen or its words, run the shell demo and a bot run with `--lang=fr` and
   `--lang=ja` (French runs longest, Japanese and Chinese are drawn on a 12px grid) and read those lines. A run only
   deals a few curios and keepsakes, so `godot --headless --path . -- --fit-test --lang=CODE` measures every curio's
   name and description on Nim's cards (wide and narrow) and every keepsake's card, in that language's font, and exits
   1 if one doesn't fit: run it for each language after changing a curio's or keepsake's words (or the cards).
8. Curios: `dotnet run --project tools/curiotest` (0.35) runs 217 checks, every curio card's promises against the game
   (most as twins, with and without it; `--only=key,key`; `AUDIT_A/B/C/D.md` beside it say what each card promises and
   where its rule lives; D is 0.37's: the six that do a build's routine, and the trade's ways). It compiles Core itself, so internal members are reachable. A new curio, or a change to one's
   rule or card, needs its checks; the run must end "N of N checks passed" (it names any curio with none).
9. Fresh eyes (0.34): `tools/playtest/pt` drives the game in a virtual 1280x720 screen for a playtester who plays by
   screenshots (start, click, shake at a strength, drop, keys; a fresh profile in `PLAYTEST_DIR`; `GAME=` a frozen
   worktree keeps the build steady while you work). For a playtest by an agent with no context, give it
   `tools/playtest/README.md` and nothing else of the repository, and ask for a play log, what it understood of the
   story, and its top issues with screenshots.
10. Music (0.36): `python3 tools/music/make.py [names]` (numpy, and ffmpeg with libvorbis) writes and renders the themes
   and the ambience into `assets/audio/music` and `assets/audio/ambience`, and prints each one's peak, loudness and
   seam; then `godot --headless --path . --import`. A cloud session has no way to hear it (`--wav DIR` writes WAVs
   for someone who can), so say plainly that a change to it was checked by its numbers only. Code-drawn icons and
   curios (`ICONS3`, `CURIOS3` in `tools/custom_art.py`) are redrawn by `python3 tools/import_art.py --icons-only`.

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
  instead (`Categories.HiddenMultiplier`, in `Special`; `Chest` strips magic off anything hidden); luck shines one past
  purple only with the curio that promises it a step (a hollow rod, a ward-key: `Build.Luck`, 0.35).
- Listening is free, and a build does the routine (0.37, `Hands.cs`, a partial of `GameState`; `docs/design.md`,
  "Listening is free, and a build does the routine"). 0.36 rationed listens (`Ear.cs`: eight a day, a meter, a kettle
  and a screen); the owner found it served no purpose, and it's gone: **don't put a cost or a count on shaking
  again.** The repetition is the same motions on every chest, so 0.37 gives them away a piece at a time. Each main
  way of trading but sight unseen has one curio that does its share (`ReadsChests`: the Pocket Engine, as the
  Bookkeeping Engine does; `TicksHeard`: the Scrivener's Quill; `MarksMagic`: the Lantern Moth; `MarksHidden`: the
  Toll-man's Gauge; `NamesWards`: the Wardens' Primer; `KnowsFloors`: the Factor's Abacus), written on the slip as
  the chest arrives (`GameState.Premark`, `CounterPhase.Premark`, one note under the chest) or as things sound
  (`Tick`, `CounterPhase.Quill`); the sim's careful player gets the same (`Simulation.Slip`, `Strategy.FirstOffer`).
  And the shaking itself goes to Bram's cradle (`GameState.Cradle`, `CradleLevels`, `CounterPhase.Rock`: a button, or
  the act key, rocks the chest at each strength its seller allows for nothing, a second each; once gently where
  shaking has a price; never a drop), from the fourth day at Thornwick in a first season (`CradleDay`: the first three
  are a newcomer's heaviest) and from the first morning after (`Veteran`, with `Ways >= Market.CraftTier`). The slip's rows show what's been heard on this chest (a dot and a darker word), a
  click counts one more and a right-click one fewer (`Appraisal.Uncount`). Which checks a chest has had is still kept
  (`Visit.Checks`, `Tried`, `Check`: a strength counts once shaken at for `CounterPhase.TryTime`, by `ShakeBox.Level`;
  the chest's target glides between the pointer's reports, `ShakeBox._reportEvery`, over the longer of the last two
  gaps between them, so a shake reads the same at twelve frames a second, or at an uneven pace, as at sixty: followed
  as it came, a low frame rate's lurches read a firm shake as hard), for Bram's lessons,
  the hint under the chest and the opening, where a thing nothing you did could have told you of is unheard (the
  closed eye, `OpeningPhase.Unheard`, `Visit.CouldHear`, `NoteAudible`), not missed: a cross is for what you listened
  to and got wrong. `--hand-test` (above) checks a hand's shake is counted. A new curio of this sort belongs to one
  way of trading, does that way's routine and nothing wider, is rare, has a line in `Premark` (or its own hook), a
  place in the sim's careful player, and checks in `tools/curiotest/ChecksD.cs`.
- Six kinds of thing a sword-and-sorcery realm would lock away (`Category`: Coins, Arms, Garb, Potions, Scrolls, Gems,
  shown as Coin, Weapons, Cloaks, Potions, Spells, Gems; the fakes are demon gold, paste gems and scroll cases of sand).
  Every thing is a find worth having (no letters, no rusty daggers); rename one in `Categories.Renamed` too, so saves'
  journals carry over. One sound per kind of thing, the one you'd expect in real life (`CategoryDef` in `Items.cs`; alternatives for each
  kind, chosen in Settings > Item sounds, are in `Audio.SoundSets`: play `SoundSets.Group(kind)`); kinds differ in how
  hard you must shake to hear them (`StirsAt`) and whether they drown out or get drowned out (`Loud`, `Delicate`).
  Introduce new things one at a time: by shop tier (`CategoryDef.Tier`, `SellerDef.Tier`) and with a Bram hint
  (`Hints.cs`).
- A seller's first visit (with tips on) is a lesson (`Lessons.cs`): a chest that guarantees what it teaches (and follows
  the day's rules like any other: a storm makes it a colour finer, `Lesson.MakeOn`, `Chest.Brighten`; and the build,
  priced with the shop's `Boosts` and dealt with by `GameState.Welcome`, so curios hold for it: `GameState.Lesson`; a
  scripted lesson with a price of its own follows the day too, `MakeOn`'s `dearer`: Bettany's lesson in passing asks
  what a tourney or the Crown's buyer makes her chest worth and a bit more, `GameState.Bettany`, or "Pass" would be
  bad advice that day), and
  steps whose `Done` checks run every frame on the hearing, the slip and the visit, whatever Bram's box shows (0.35: a
  step is an action, named in a tab over the box, `LessonStep.Do`; the first day is lessons too, `Lessons.FirstDay`). Keep checks to things the player
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
- What's said is typed out a character at a time, a sentence after another (0.37, `Ui/Typewriter.cs`; the sentences
  are 0.35's `Words.Sentences`, and stay: the echo horn's lines, Bram's dots, the speaker's stir and the click that
  shows the rest all go by them): Bram's box (`MentorView`: dots, "click to go on"), scenes, talks, letters, the
  treasury's captions and the sellers' bubbles. A `Typewriter` is given the sentences and the whole speech's markup
  (`Begin`), lays the speech out whole (the label is sized for all of it, and `VisibleCharactersBehavior` is
  `CharsAfterShaping`, so nothing reflows), and types it from the owner's `_Process` (`Process(delta)`) at the
  player's text speed (`Settings.TextSpeed`: At once is 0.36's way, a sentence whole and the next once it's had time
  to be read, `Words.ReadTime`; half the rate for Japanese and Chinese), with a soft blip pitched to the speaker
  (`Typewriter.Voice`, `Sfx` "talk"; "scratch" for a letter or Bram's notes; `Settings.TextSounds`; only the speech
  begun latest is heard). A speech still says itself (0.36), a click or the act action shows the rest at once
  (`Finish`), and the next goes on: one click a speech, never one a sentence or a character. Anything new that talks
  uses a `Typewriter` the same way (`SellerView.MidSpeech`/`FinishSpeech`),
  and starts by itself: a talk's first line follows Bram's word without a click (`TalkPhase`'s opening; the playtest
  sat at a blank bubble). What matters is coloured
  where it's said (`Rich`, a `RichTextLabel` with BBCode, its own brackets escaped): the trade's words in tips and
  notes (`Rich.TradeWords`; on the slip's paper `Rich.OnPaper`), a seller's price and stated clue (`SellerView.Marks`,
  with a bob), strengths only in sellers' own words (`Rich.Strengths`: their "coin" is money), the seller's rule under
  the chest in gold. Everything said goes into `GameState.History` (`Hear`, saved, the last 400 lines), which Bram's
  echo horn shows (`EchoHorn` at the counter's end in the counter and talks, beside closing up at the day's end;
  `HistoryView`, which swallows keys while open and sets `HistoryView.Open`, so notes wait and the counter is busy).
- The chest is two layers (0.35, `ChestView`): the lid, and under it the box, shut, or (once the lid lifts at all) the
  open box with its inside in perspective (`custom_art._chest3_open`), lit from within in the chest's colour
  (`DrawInnerLight`, additive) with a soft spill from the crack while the lid's on; shafts only out of the open box,
  settling to `ChestView.RestGlow`. A new opening effect lights the inside; it never draws a bar between the halves.
- The opening builds to something (0.37, `OpeningPhase`; `docs/design.md`, "Opening a chest"). How much rides on a
  chest (`_stakes`: its colour, what was paid against the working purse, how full it is) sets how long it strains
  (0.9 to 1.8 seconds, the lid thumping faster, `Thumps`) and how long it draws breath before its last thing
  (`LastBreath`, longest when that thing decides whether the chest pays for itself); a glint out of the crack says
  something better than promised (or enchanted) is in there, never what (`Tell`); everything flies out wrapped in the
  chest's light and is shown by its landing; the stake bar under the tally fills towards a notch at what was paid
  ("PAID FOR!", `PaidFor`), and a chest that paid twice over or more is stamped with how many times, with fuss only
  when the profit is a real share of a working purse. Keep the fuss tied to the stakes: a hundred chests a season
  can't all be an event. An enchanted thing keeps light flowing over it wherever it's shown (`Enchant.Flow`,
  `Enchant.Aura`, `assets/shaders/enchanted.gdshader`; `UiKit.MakeItemIcon` does it for any slot).
- Heartsongs are introduced on a quiet first-week evening (the `songs_intro` tip), and a season that ends with none ever
  heard gets Bram's word on them between runs, once a profile (`Story.SongsUnheard`, in `Main.EndRun`).
- Heartsongs (`Heartsongs.cs`, `Category.Heartsong`, `Rarity.Mythic`, red): nine collectibles, about a third of one a
  run while the debt lasts (v22; 0.37's season with listening free at 0.26% a chest: 0.40 a season for the careful player and 0.27 for a first; 0.36's counted listens left chests unshaken and wanted 0.32%, 0.30's thirty-five days at 0.16%: `--sim=songcount`,
  and tune it again if a season's length, or how many chests a careful player buys, changes), in any seller's chest
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
  it's about 2.1 at the Emporium (0.30: 2.7 when the buyers' trades were bigger); the last due (6,600g in 0.37's four weeks, where the Emporium's
  week is the season's last and nine chests in ten are bought at an index under 1.7; 5,000g in 0.36, 12,000g over 0.30's seven) is set above what that
  earns on a careful margin, so carrying it takes a build or tales gone well. Bram's working purse (`GameState.WorkingPurse`: two typical chests, `Market.TypicalAsk`, at
  `PriceFactor`) is the day-end advice, what `Strategy.Reserve` keeps, and the line a purchase warns before crossing:
  `ReserveIf`/`ShortNote` work out the purse after the purchase (its effect on the index; the bigger shop for a move),
  and DayEndPhase and PeddlerPhase show a red price and a confirmation. Route anything that changes prices through
  `PriceFactor` and anything that changes what ordinary things fetch through the index, deciding whether a seller could
  know of it.
- The trade's words (0.32, `Glossary.cs`): sight unseen, Bram's word, the stash, a short night, a perfect read, a step
  better, a surprise, a dry spell, exhibition points, wards, false bottoms, haggling, mended, each defined once and exactly
  (with the rule's own numbers). A curio lists the words it uses (`Glossary.CurioTerms`): Nim's cards show a "?" with
  them, and the curio tooltips and the collection add the definitions; Bram explains sight unseen and his word the first
  time a card uses them. A curio's text says exactly what it does: no "luckier", "their least" or "makes them likelier".
  At the opening, a chest bought sight unseen (not a gift or the Crown Chest) gets a "Bought sight unseen" tag, and what
  the slip never marked gets a closed eye (`UiKit.MakeUnseenMark`, `OpeningPhase.Unseen`), not a cross: nothing was read,
  so nothing was missed. The gut and the profit stamp judge the same sum: everything the chest brought back (what's
  inside as it sells here, a perfect read's fee, a rebate, the bond) against what was paid.
- Curios are the run's build (`Curios.cs`, `Build.cs`): 85 of them, each bending one rule, each tagged with the ways of
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
  a decision about the index. Never let a curio tell you what a chest is *worth* (how good a thing is, or a price):
  until 0.37 the rule was "what a chest holds", which the six that do a build's routine now break on purpose (above),
  and only they. A curio made for one of the trade's ways (enchantment, false bottoms, locks) is brought wherever
  chests have that way in them (`Curios.TierFor`, by `GameState.Ways`), the rest by the shop's market.
  A price a curio or a rule changes is kept exact and shown to the coin, at least a coin off or on (`Visit.Raise`,
  `Visit.Rounded`); "talked down" is from the price on the table at your first offer (`Visit.OfferedAt`); a thing kept
  on the shelf sells for at least what it would have fetched at its opening (`GameState.ShelfValue`), and screens show
  the exhibition's own points (`GameState.Points`). The day's curios (`Curiosities`, `Compass`) run after the tales
  have staged their chests; the opening's luck line thanks the curio whose rule reached each thing (`Build.Luck`).
- Synergies (`Synergies.cs`) come from what's been acquired (six of a kind at a quality per level) or achieved (perfect
  reads, surprises, hidden things found, bargains, enchanted things); only active ones (one slot, more with upgrades)
  do anything, chosen at the end of a day. Each is a real power: its effect and 20% a level on what its kind sells for
  (`Synergies.SaleBonus`, priced into the power index).
- The economy grows fast and the Guild's dues grow faster (v18: the old tight constraints made upgrades feel optional).
  Qualities are worth 1/2/4/8/24 times a common (`Categories.QualityMultiplier`; 1/3/10/30/100 until 0.30, when each
  step inward was three to ten times the last and the owner found the numbers ran away); each move inward multiplies what
  a customer is worth a few times over (the sim's "profit per customer at each shop": about 7, 14, 115 and 150g in 0.37's season, whose
  week at the Emporium is its last; 0.35's seven weeks made 490g there).
  **Gold is rare** (0.30): a chest's own roll makes a thing gold only `Categories.GoldShare` (6%) as often as the spread
  says, and luck never makes gold (`Categories.Lift`: restorers, curios, a dry spell, a hoard's better things stop at
  purple); luck that would have lifted a purple thing past purple makes it shine instead (enchanted, `Build.PastPurple`,
  `Item.Shone`: read as the plain thing it was, "luck made it shine!"), priced into the index and the slip
  (`ShineShare`). A careful season finds 0.7 gold things, half of seasons none (the sim's "gold things a run"; about two over 0.30's seven weeks).
  The luck curios and restorers were made about twice as likely to step (a step is worth twice, not three times), and
  the Master Restorer mends (nothing worse than its chest) rather than lifting. buyers' trades (`Upgrade.Sales`:
  scales, auction block, royal warrant) multiply what everything sells for, and restorers (`Upgrade.Luck`: the bench,
  the workshop, the master) make everything likelier to come out a step better; both go into the power index. Flat
  sums (door money, a catalogue's fee) grow with `ShopTiers.Scale`. The careful player keeps a working purse and what the next due needs beyond what bad
  days would earn (`Strategy.Reserve`; until 0.36 it saved for the next move before anything else). The Bookkeeping Engine (`Upgrades.Engine`) is the Guild's: offered
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
- The run (`Run.cs`, a partial of `GameState`): four weeks of five days, a market each (0.36; seven weeks in 0.30 to
  0.35, five before that, eight of three in 0.20); every day has a role (`DayRole`, `WeekPlan.Role`), planned a week
  ahead for the market it will be played at (`Events.Plan`, `GameState.Plan`/`NextPlan`) for the notebook's calendar
  (`NotebookView.BuildCalendar`, with the Guild's debt across its top): the fifth is the Gilded Guild's tithe
  (`Guild.Due`: 40, 170, 700 and 6,600g, then endless x1.5; 5,000g in 0.36, before listening was free again; Vane
  introduces it on the second evening,
  `GameState.IntroDay`), and paying it moves the shop a market further in (`MoveInward`: Thornwick from day 6,
  Crownhold from day 11, the Emporium from day 16); a story week's first day is the story's (`Story.TellsOn`: weeks 1,
  2 and 4 (the gathering: who stands with you), and the week after the debt; `GameState.StoryToday`); Nim comes every
  night from the third (0.28; from the first in a later season); and two days have an event (`Events.PerWeek`; gentle
  ones in the first week). Pawning,
  Bram's word once a run, foreclosure; commissions (`Commissions.cs`, from the second week's third morning,
  `Patrons.FromDay`; from the first week's third morning in a later season, `CommissionsFrom`) pay for each find as it's handed over, always at least a quarter over what it would sell for
  (`GameState.CommissionPay`, `Patrons.Premium`), so delivering always beats selling; the story's scenes (`Story.cs`;
  the campaign, below). Balance is set for a career, not a run (the sim's "Learners" and "Careers" sections; learners
  are the careful player with mistakes that shrink as `Style.Skill` goes from 0 to 1). **Level 0 is the story's
  difficulty** (0.36): the first three dues are paid by nearly everyone (a first-timer 98%), so every season sees all
  four markets, and the last, at the Emporium, is the wall; the challenge levels are where it gets hard. 0.37, with
  listening free again and the last due at 6,600g, 120 runs each (`--sim=quick`, `--sim=learners`): the careful
  player buying curios as they come clears 93% of first seasons (86%
  with no curios, 73% with no tales, 48% with neither, 92% with no Heartsongs), a learner at 0.9 85%, at 0.75 78%, at
  0.5 53%, at 0.3 33%, and a first run 18% (15% with no Heartsongs: one found is often a first season's making);
  the same hands in a later season (`--sim=learners --veteran --ways=4`) clear 29, 43, 66, 86, 95 and 99%.
  Careers (`--sim=careers --n=60`): the debt is first cleared on the second or third season (half of players by the
  second, three quarters by the third), level 1 around run 4, level 2 around 7, level 4 around 15 (every
  career by run 30), level 6 by just under half of careers by run 30 and level 8 by one in thirty; the levels as
  drawn clear 89, 79, 61, 42, 25, 18, 9 and 5% (`--sim=levels --veteran --ways=4 --n=150`: a level is always played in
  a later season; the dues 30% up a level, `Challenges.DebtStep`, 20% until 0.37, when a later season began opening
  with more and the careful player cleared level 8 one time in five).
  The campaign: the first swallowing comes with the first clear, on the second or third season; Bram is first
  brought home, and the next day (the true ending) reached, around the fifth (the fourth to the sixth), by nearly
  every career by run 30. (0.36, with counted listens and every season starting from nothing: the true ending around
  the tenth, level 4 around the twenty-sixth.) (0.35, seven weeks: the careful player 70%, a learner at 0.75 55%, a first run none; first clear around the
  sixth run, the true ending around the thirteenth, at two and a half hours a season.) The careful player keeps a
  full mimic for the final opening unless it has Guild nights enough to fill it again (`Strategy.SpendMimic`): the
  plan's "free them" is what a season most often lacks. A run's length is part of the balance: `--sim=experience`
  should keep a first run under two hours (0.37: 104 minutes over 20 days and 115 customers, a second 90 and a third 87; 0.36: 104, 94 and 89; 0.35: 143 over 30 days), a high point every seven minutes or better, no stale
  stretches, and every day's median load under 6: the first days at a new market are the ones to watch
  (5.4 to 5.8 on the first at Thornwick, 5.5 to 6.0 on the first at Crownhold, by 30 or 120 players; eight first-timers
  in ten still have some day just over), and with tips on they're quiet ones (the first a customer short and with no locked chests,
  `GameState.QuietOpening`; the first two with nobody whose lesson is a warded thing's puzzle, `Settling`). A new
  thing to learn at a market goes on a later day than these, in Core, so the sim sees it. `--sim=kin [--who=KEY]` runs
  just the characters' table, for tuning one. Careless styles are foreclosed (buying at their price clears 2%, one
  hard shake a chest 13%), and no early week sends careful players to Bram's word. The sim's evening runs in the
  game's order (0.36: the day's end and its spending, then the Guild's night; it used to pay first, which once a due
  moves the shop had its player fitting out the next market before trading a day there), and `Strategy.Reserve`'s
  bad day is this season's (`BadDay`: about half what the sim's "profit per customer at each shop" times a day's
  customers comes to; retune it when the economy's size changes, or the careful player saves for the wrong due).
  Anything added to the run goes in `SaveData` too; the profile across runs is `Profile` (user://profile.json).
  Bram's stash (`GameState.Stash`, twice a run) tops up a day that starts nearly broke. Bram's working purse
  (`GameState.WorkingPurse`) is the day-end advice, the purchase warning and what `Strategy.Reserve` keeps, so the
  advice is what the sim's careful player does: tune them together. The whole debt can be paid off early
  (`GameState.DebtLeft`, `PayOff`) from the day end or the Guild's night; the sim's careful player does, in four
  seasons in five, and reports the week each run cleared (about one in five in week 3, the rest in week 4). Surprises
  (`Categories.Odds`: since 0.36 a thing comes out better than its chest about one time in seven and worse one in
  eleven, where it was the other way round; the restorers and luck curios add to it), with a pity counter
  (`GameState.DrySpell`).
- Across runs (`Meta.cs` for the definitions and the score, `Kin.cs` for what they do to a run): four who can mind the
  stall (`Characters`), each changing one part of how chests are judged or the shop is run and paying for it (Hesk: no hard
  shake or drop, a scale; Rosa: everybody haggles, walkouts stay away a week; Isolde: a shelf and door money from day
  one, no pawning her collection). Only Kit is Bram's kin (0.32): Hesk, Rosa and Isolde are his partners, each born with
  the mark somewhere else, with a background, a welcome (`Story.Welcome`), a season's-end line and a tale of their own
  drawn every season they play (`TalePlace.Kin`, `TaleBook.Own`, `TaleBook.Kin.cs`). Bram is "Uncle Bram" to Kit and
  "Bram" to the others (`Hints.Mentor`, by `Hints.Kin`, set when a run starts or loads): a line that calls the player his
  kin, or him their uncle, is Kit's alone (`state.Is(Characters.Kit)`); and eight challenge levels (`Challenges`, opened per character by clearing at the
  level below, a quarter more to the score each). A level raises every due by `Challenges.DebtStep` and deals the run
  that many banes and as many boons (`TermDef`, `Challenges.Draw`: nested, so a level's draw is the one below's and a
  pair more, and never a boon with the bane it contradicts; a real run draws from `Profile.TermsSeed`, so the setup
  screen shows what will be played, and keeps them in `SaveData`). Banes make the trade harder; boons smooth over what's
  most frustrating about it (a slow start, a short night, the clock, a slipping lock, a curio you can't find). A term
  is a rule that holds every time, with a reason in `docs/world.md`, routed through `GameState.Under`/`Favoured` where
  its rule lives; the setup screen and the pause menu list a run's terms (`UiKit.MakeTermList`). `--sim=terms` measures
  each alone and the levels, `--sim=levels` just the levels as drawn (`--debtstep=S` tries another step): keep each
  term within several points of the others and every level harder than the one below. Weigh a term at a level's dues
  (0.36, `--sim=terms --level=3 [--who=a,b] [--n=400]`: at level 0 the careful collector clears nine seasons in ten
  and nothing shows; 0.37, in a later season, which every level is, `--veteran --ways=4`: at level 3's dues, 70%
  with none, each bane costs up to nine points and a boon gives up to six; thin crowds was cut to the Guild's day and
  Nim's soft spot to the second week on, each having been worth twelve).
  0.36 cut Thornwick's welcome from half the second due to a quarter. Twelve banes and twelve boons (0.36's head
  cold and rested ear went with the counted listens). Every first season starts the same, and nothing is bought or banked that makes a run stronger (v22 retired the talents bought
  with renown: Bram's Kin's 10g, Familiar Face's fourth card and Second Look's reroll are everyone's now,
  `GameState.StartingGold`, `Curios.Laid`, `RerollPeddler`; Mara's Favour is the Guild Promissory Note, a curio; the
  rest were curios or upgrades already, or went), only what's known (the journal, what Bram has taught, Nim's stock and
  the legendaries' feats) and, since 0.37, what a later season opens with for having traded before (the trade's
  ways, the cradle, a pick from Bram's drawer: "A season starts in one of three states", above). Characters add no share to sale prices and take none off dues (the shop's growth is
  upgrades', synergies' and Heartsongs'). A run's score is its points times the level's multiplier (`Score.Final`,
  each character's best in `Profile.BestScore`). Bram's lessons are
  Kit's (`GameState.GivesLessons`); a new character gets a `kin_*` tip. Route seller traits through
  `GameState.MindsDrop`/`MindsMagnet`/`MindsHardShake`/`Impatient` and upgrade prices through `GameState.Cost`, so the
  terms apply everywhere. The profile (`Profile`) keeps each character's open levels and best score.
- The shop opens up a step at a time, moving inward (`ShopTiers.Name`: Bram's Stall, Thornwick Market, Crownhold, the
  Royal Emporium; four customers a day at the stall and five further in, `ShopTiers.Customers`;
  `GameState.UpgradesOpen` and `DisplayOpen`
  with Thornwick, `SynergiesOpen` with Crownhold), each introduced by a Bram tip when it happens. **Paying a week's
  due is what moves the shop** (0.36, `GameState.MoveInward`, from `PayDues`, `AskGrace` and `PayOff`): a market a
  week, never bought (`shop2` to `shop4` are still upgrades under the hood, owned by the move, and their blurbs are
  what the road says of each market). On a market's first morning `RoadPhase` shows the season's road (the four
  markets as pictures, the Gate at its end, the cart rolling in, what the place brings and what the Guild wants
  there; `GameState.MovedFrom`, `TierSince`, `NextMarket`), the day's end at the stall shows the due as the way on,
  and the Guild's panel says where paying leads. Gate new
  systems the same way, in Core, so the sim and the bot follow them.
- A season starts in one of three states (0.37; `docs/design.md`, "A season after the first"): the first ever on a
  save (`SeasonsBefore == 0`: everything taught, each of the trade's ways met a market at a time), a later one
  (`GameState.Veteran`), and one after the true ending (`CampaignDone`: a later one, with no summons, mimics or
  treasury). In a later season nothing is said twice (Bram's tips and lessons are the profile's, and the story's
  exposition has short forms: `Story.Intro`, `WeekStart`, `Summons`; write one for any new scene that explains);
  what a chest can be goes by **the trade's ways** (`GameState.Ways`: the grandest market any season on the save
  has traded at, `Market.CraftTier` 2 for locks, false bottoms and wards, `PairsTier` and `MagicTier` 3 for two or
  three of a kind and enchantment; `Market.Roll`, `TaleBook.Fill` and `Events.StrangerVisit` all go by it), and the
  slip counts by it too (`Appraisal.MaxCount`, `CountsHidden`, built with `ways:`): **a chest must never hold what
  its slip can't mark**, so anything new that fills a chest takes the ways. Kinds of thing, sellers, prices and
  tools stay with the market. Bram turns out his drawer on the first morning (`DrawerPhase`, `Curios.Drawer`: three
  plain curios for three ways of trading, one for nothing), Nim comes from the first night (`PeddlerToday`) and
  commissions from the first week's third morning (`CommissionsFrom`); the `ways` and `cradle` tips say why the counter looks
  different. A later season is easier than a first for the same hands (the table in the design doc), on purpose.
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
  (0.37: **F1** during a season, `Main._UnhandledInput`; `Settings.Gm` is the session's, never saved, and not a row
  in Settings any more; `--gm`) puts a panel beside the pause menu: +99,999 gold (`Main.GmGold`), skip the day
  (`GameState.SkipRestOfDay`), fill the notebook and the collection (`KnowAll` on the run and the profile), and
  **Fulfil the quests** (`GameState.GmFulfil`, `Main.GmFulfil`: this week's commissions, the key's four pieces, the
  debt paid off whether or not Vane has called, `SettleDebt`, and every check of the true ending, `PlanWhole`, so the
  final opening comes that night). A new requirement of the campaign or the true ending goes into `GmFulfil` too.
- The game opens on `TitlePhase` (0.32): the chest strains and bursts open, the name rises out of the light and the
  menu's buttons fly out of the chest into place (a timeline in `_Process`; any key, button or click finishes it at
  once; shorter coming back from a run; shake, flashes and the hit stop through `Main`). `Main` saves `GameState.Save()` to the save's own file (`Main.SavePath`) at the start of each day (not
  for the bot or debugging flags) and the pause action (Esc, Start) opens `PauseMenu`. There are three saves (0.37:
  `Settings.Slot`, `Main.SlotPath`: the first is user://save.json and profile.json as always, the others in
  user://slot2/ and slot3/; `SlotsView` from the title's bottom-left button: Play, Start a new save, Erase;
  `Main.UseSlot`, `EraseSlot`, `PeekSlot`): never write "user://save.json" or "profile.json" anywhere else. A run ends on `RunEndPhase`:
  a lost one (0.36: where most first seasons end, at the last due) says how near it was and offers Another season
  beside the way back to the title (`RunEndPhase.Again`, `Main.AnotherSeason`: banked like any other, then straight
  to who minds the stall). A day: `Main.StartDay` (the
  road on a market's first morning, then the week's scene), the counter, `DayEndPhase`, then `Main.CloseDay` (Vane's introduction the second evening,
  `DuesPhase` on the fifth). Anything added to `GameState` that should survive a day boundary goes in `SaveData` too.
- Files (0.27): everything the game keeps goes through `Storage` (`Read`, `Write`, `Delete`): a write goes to a temp
  file, flushed, then replaces the old one, which is kept as `.bak`; a read falls back to the `.bak`, and a file that
  can't be read is set aside (`name.unreadable-<time>.json`) and named on the title screen (`Storage.Notices`), never
  deleted; an erased save is set aside the same way (`Storage.Erase`: `name.erased-<time>.json`). The user folder is `Keywright` (`config/use_custom_user_dir`: `%APPDATA%/Keywright` on Windows, so Steam
  Cloud can name one place), and `Storage.CarryOver` brings files from Godot's old app_userdata folders. `SaveData`
  (version 9: a 0.36 save's Tea Kettle, Listening Screen, Echo Stone and Plugs are refunded) and `Profile` (`Profile.CurrentVersion`) carry a version: a file from a newer game is left alone, an
  older one is migrated in `FromJson` (`Categories.Renamed` only for saves up to version 2, from LootFall 0.17; a
  run saved before 0.36, version 7 or older, picks up at the market it had reached, on the same day of its week,
  with the dues before it paid and every day it remembers moved back with it: `--save-test` checks it).
- `Settings` (user://settings.cfg; `SettingsView`'s pages Display, Sound, Controls and Game) holds the window
  (windowed, fullscreen or exclusive, a size that fits the screen, pixel-perfect or smooth scaling, vsync, a frame
  cap), screen shake and flashes (route new screen effects through `Main.Shake`/`Flash`), the outlines, the volumes
  (the `Effects`, `Songs` and `Music` buses, under Master: a Heartsong's "song_" sounds go to Songs, the themes and
  the ambience to Music), muting and pausing when
  the window loses focus, tips, how fast speech is typed and whether it blips (`TextSpeed`, `TextSounds`), language,
  the controls, and which save is being played (`Slot`). `Settings.ApplyDisplay` changes the window through the
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
- Following a tale (0.32): a talk is headed by its tale's title ("A NEW TALE BEGINS" on its first); the notebook's
  Stories page (`NotebookView.StoriesPage`) lists every tale begun and shows one whole: its setup (the tale's `Blurb`,
  which says who, what's at stake and what you can do), its log, and what comes next (`GameState.TaleNext`: the first
  beat that could come, by who, where and about when). Keepsakes (`Keepsakes.cs`): what a tale's people give you to
  keep. Each works like a curio with one effect of its own (`GameState.Keeps`, hooked where its rule lives) but takes no
  slot, is never sold, bought back or pawned, and isn't in the power index; `GiveKeepsake` in a talk shows it as a card
  (`TalkPhase.ShowKept`); they sit after the curios at the counter and the day's end, and the collection has a
  Keepsakes page. Saved in `SaveData.KeepsakesHeld` (`SaveData.Keepsakes` is still what tales set aside,
  `GameState.SetAside`), remembered in `Profile.KeepsakesSeen`. A new keepsake needs its tale, its effect, a picture
  (one borrowed until drawn) and a line on the collection page. The Silent Choir (`TaleBook.Late.cs`) is the season-long
  quest whose prize is a Heartsong: three contest chests (`Visit.ContestFor` says what an exact read wins) and the
  Guild's price; one wrong read loses it. A season draws two of Brindle's tales and two of Thornwick's, one of Crownhold's and one of the Emporium's (0.36;
  three of each over 0.30's seven weeks).
- The main story (0.34, after the owner's narrative proposal; `docs/narrative.md` has the truth under it and what was
  adopted, adapted and left out, `docs/world.md` "The laws of the Locking"). It's a campaign across seasons on the profile
  (`Campaign.cs`, `Inquiry.cs`, `TaleBook.Campaign.cs`, `Story.cs`, `Phases/TreasuryPhase.cs`). **The first season**:
  Bram minds the stall with you for a week and teaches; on the first Guild night Vane brings the treasury's summons
  (`SummonsTonight`, `Summon`) and Bram goes, with Old Haddon if his note to the Guild wasn't paid (`Claims`,
  `GuildCollects`, run once a night before the summons). From then on Bram speaks through his notes (`BramNotes`;
  `MentorView`'s "Bram's notes", `Story.Narrator` is Bettany), and a line of his in a tale is yours while he's away. The
  Guild's letter (week 2) sets the commission: keep his account paid and earn the treasury key's four pieces
  (`Campaign.Pieces`: a hidden thing or five things read true, a stolen chest settled, a commission filled, an old lock
  picked without force; `EarnPiece`, each with a master's fee, `Campaign.FeePurses`, which replaced the Hooded Woman's
  shard commissions' pay). The night the account is paid with the key whole (`TreasuryDue`) the Guild's carriage takes
  you to the treasury, whose great chest is the Maw: the first time it swallows you (`TreasuryMode.First`, `Swallow`),
  and the year turns back (`Main.Rewind`: the season banked, the next starts at once with the same keywright and level).
  **Later seasons** you remember (`Profile.Swallowed`, `GameState.Remembers`): the same spring comes round (Garrick's green
  chest on the second morning, `TaleBook.FirstChest`, recognised from `Profile.FirstChest`; the same letters), Vane
  speaks for something that remembers too, and the investigation is the **Inquiry** (the notebook's Inquiry page):
  threads (where is Bram, the breathing chest, the first morning, the plan), entries seen, said, tried, heard or found,
  and leads while they're open; what's known carries over (`Profile.Known`, `KnownBefore`), what's held (a copy of the
  register, Bram's tuning fork) doesn't. Discoveries open actions: Hollis and the carts (follow one to the weighhouse),
  Guild crates that knock (`Chest.Crate`: knock back at the counter, `CounterPhase.KnockBack`; bought, someone gilded in
  the back room, `TakeIn`), a full mimic eats the gold off them (`Ungild`) and a redeemed note brings them home
  (`RedeemGuest`, Bram's is his account: the debt), Mara (the last of the Keyhold) and the seal's clause, the charter from
  Quire or King Aldo, the Lantern Market's passage and papers. Vane bribes on the second Guild night and bargains for
  Bram on the third (`Campaign.BribeWeek`, `BargainWeek`, `BribeTonight`, `BargainTonight`; the fourth and sixth of
  0.34's seven). Once you remember you can stay away from the treasury
  (`TreasuryMode.Again`); with the plan whole (`PlanWhole`: find them and know them, free them, a way out, the door
  opened without giving it away: `Inquiry.PlanLines`) it's the final opening (`TreasuryMode.Final`): hold still, knock
  back, turn the key, they come out, and the Maw's last offer (`Story.FinalOffer`: leave the door to it and the year
  turns anyway; close it and it's **the next day**, `BeginNextDay`: Bram's tea, Wynn's waterwheel chest, and the
  profile's `Finished`). After that a season is a new year (`CampaignDone`: no mimics, no summons). Proposal ideas left
  out, and why, are in `docs/narrative.md`. A `Line` can note an Inquiry entry (`Line.Clue`, noted when it's said by
  `ScenePhase`, or `Story.Heard`); a new entry goes in `Inquiry.Entries` with text that matches what gives it, and a new
  beat in `TaleBook.Campaign.cs` with its `If` against the campaign's state. The careful player's campaign is in
  `Strategy` (`TakeBribe`, `TakeBargain`, `AtMarket`, `KeepMimic`, `TendBackRoom`, `BuyCrate`, `StayAway`), and the sim
  reports how a first season goes (`--sim=quick`) and the campaign across careers (`--sim=careers [--level=0]`: the run
  first swallowed, Bram first brought home, the next day).
- Mimics and the Lantern Market (0.33; 0.34). A mimic (`Mimics.cs`) is a chest that ate what was in it: only once you
  remember the treasury (0.34), from Thornwick, one ordinary chest in 90 (`Mimics.OneIn`, its own dice after the seller's
  priced it, `Breathe`), and Brother Cadoc's warm chest from the crypt every remembered spring; empty but for the mimic,
  priced for what the seller notices (nothing, unless they price by colour). It breathes while it sits still (the
  counter's `Breathe`: a sound, a word and the lid lifting; held when shaken), and an empty chest rings like a drum when
  shaken firmly (`CounterPhase.Resonate`: a mimic or a crate never does), `Hearing.HeardBreath`. Opened, it's sold to
  the Arcanum's menagerie (0.6 of a working purse), kept in the back room (it eats 0.15 of a purse each Guild night, keeps
  the Lockless from robbing you) or kept by the menagerie for a fee; fed twice it's full (`MimicFull`) and eats the gold
  off one of the gilded. A mimic or a Guild crate is no read (`ReadReport.NoRead`: no perfect read for an empty slip), and
  one kept is stamped as in the back room, not as a profit. `--mimics=N` makes one chest in N breathe. The Lantern Market (`LanternMarket.cs`) is at Nim's
  table on the third night of each week from Thornwick (`MarketTonight`): once you remember it sells passage and papers,
  never an answer (a night at the weighhouse with a look at its register, the Wet Hen's bargemen for the treasury's last
  night, a crate diverted off the palace road), the Maw's buyers take gold things off the shelf at half as much again
  (each an eye for him, `EyesSold`, read back at the season's end), and the menagerie; `Strategy.AtMarket` is the careful
  player there.
- What a tale leaves behind (0.30; `docs/stories.md`, "What a tale leaves behind"; `Consequences.cs`): who comes (and
  who never does again: `Goes`, and their tools with them), officials who come only while their masters count you a
  friend (`SellerDef.Comes`: Bailiff Marrow, a third of his seized chests cold; Quartermaster Brandt, by the pound),
  better terms from friends (`GameState.Goodwill`, said under the chest: Wynn's neighbour's price, Nettle's and Sir
  Rowan's rules waived, Durgan's dwarf price, Caskett's tolls), favours called in on a short Guild night (`Owed`,
  `CallIn`: an eighth of the due each, once a season; `DuesPhase` shows them first), papers for the King's court
  (`Evidence`, `TaleBook.Paper`: four put the Guild on trial, half its tithe, `OnTrial`), contests (`Visit.Contest`: a
  gift chest whose perfect read wins `Prize`), Mother Tansy's loan (`Borrow`, collected the next week's first morning,
  `CollectLoan`), effects that last a while (`Until`, `While`: a rival, pickets, a haunting, a crowd; `TaleCustomers`),
  and nights away (`GoOutTonight`: no Nim).
  The ending reads them: week 4's gathering and the treasury night name your allies (`Story.Allies`), and after the
  run's last scene, won or lost, the season's end tells what became of everyone (`Story.SeasonEnd`, each tale's
  `TaleDef.Ending`, shown once a run by `Main.EndRun`). 0.30's tales are in `TaleBook.Brindle.cs`, `.Thornwick.cs`,
  `.Crownhold.cs`, `.Emporium.cs` (built by methods, so the partial files' order never matters) and their endings with
  0.28's in `TaleBook.Endings.cs`; a season draws the Shut Chests and the King's Court always, and (0.36) two of Brindle's,
  two of Thornwick's, one of Crownhold's and one of the Emporium's. A new tale needs an `Ending` too.
- The people and their tales (0.28; `docs/world.md` has the order of things and the cast, `docs/stories.md` every tale
  beat by beat). The cast was replaced: every regular is someone with a place, a faction (`SellerDef.Of`), a want
  (`Wants`) and things you notice about them (`Facts`, the Magpie's suspects' clues), keeping an old regular's trading
  rule and, until drawn, their figure (`Look`; `Sellers.LookOf` everywhere a picture is loaded); old keys are mapped
  (`Sellers.Renamed`, read through by `SaveData.Renamed` for saves (version 5) and the profile (version 2)). People a
  tale brings who don't trade are `Sellers.People` (Hollis, Tobias, Aubel...). A tale (`TaleDef`, `TaleBook.cs`) is
  beats (`Beat`): who comes, after which beat and how many days, at what shop, if what (`If`), and either a talk
  (`Says`, `Asks`: `Choice`s with a cost, a doing that returns the reply, and a `Worth` for the careful player) or a
  chest (`Brings`, with `Bought`/`Passed` hooks). The engine (`Tales.cs`, a partial of `GameState`) draws a season's
  tales from the seed (`TaleBook.Draw`, above), keeps each tale's shape in the four-week season (0.36,
  `GameState.Wait`, `SeasonDay`: a beat's `Gap` is still written for 0.30's seven weeks, with the markets about days
  8, 16 and 25, so write a new one the same way; a first beat comes at the same point of the same market's week, a
  later one waits about three-fifths as long, and a tale that begins at the Emporium is told a beat a day), stages at
  most two beats a day (`StageTales`: a talk joins the queue as
  a `Visit.Talking`, a chest replaces a regular) and one a night (`StartNight`), what follows today's beat with no gap
  ("that night", `Continues`) before what has waited longest, and keeps what came of them: `Flags`, `Gone` (who never
  comes again), `Notices`, `TaleLog`, `Keepsakes`, all in `SaveData`. A tale's secret (who the Magpie is) is drawn from the
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
  the day, are the exceptions; Flick's profit is the Wardens' finder's shares). Stories say what they state, never what
  they heard, and how they shook it only where the deal turns on it (0.32: Pip never shakes, Ada can't bear to, Bettany
  and Quire charge by the rattle, Nettle's fee, Cadoc's rule, Skrae's echoes; the slip's header already says how each
  priced) (`{heft}`,
  `{colour}`, `{magic}`, `{articles}`, `{pounds}`, `{dream}`, `{clue}`, `{price}`, `{fee}`), and must fit the speech
  bubble (about 170 characters with the price filled in).
- `ShakeBox` gates sounds with the same rules as `Chest.Audible`: change both together (the shake test checks them).
- UI is built in code with `UiKit` and `Painter` (absolute positions on a 1280x720 canvas), not in .tscn files.
- Buttons use `FocusMode.None`: the act action (Space) is reserved for lockpicking and holding the chest open. The
  notebook action is N (L too, as it was when it was the ledger) or Y.
- `Hearing.Read` and `Strategy` model a careful player (what they hear at every strength the seller allows, the drop
  where it's worth it, what the build has already written on the slip, `GameState.Premark`; what they offer and
  keep) for the sim and the bot. Update them when a rule, tool or trade changes.
- Music (0.36, `scripts/Audio/Music.cs`, `tools/music`): a theme and a bed of ambience at a time, chosen from the
  screen and the shop whenever `Main.SwitchTo` changes phase (`Music.For`: the title and the stall `crossroads`, each
  market further in its own, `night` for the day's end and Nim, `guild` for the Guild's night, `treasury`; the beds
  `day`, `market`, `hall`, `night`), cross-faded, on the Music bus. A new screen with a mood of its own gets a case in
  `Music.For`; a new theme is a function in `tools/music/make.py` (written a bar at a time, rendered by `synth.py`,
  every loop seamless) and a line in its `TRACKS`. A Heartsong's song has the room to itself (`Music.Duck`, called
  from `Sfx.Play`). The chests' sounds are the game: keep the music under them (it starts at 60%).
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
  two to eight times too big. The chest is drawn at the same density in code (0.32, `custom_art.chest3`; `python3
  tools/import_art.py --chests-only` redraws it) and listed in `art_density.json` as `drawn`, which the density import
  keeps and `--density-check` checks. Anything else not in it (effects, the UI frames, a placeholder) loads as it is. The game draws at the window's resolution (`canvas_items`), so a cell is exactly 3 pixels at 1440p and
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
  generated: `tools/custom_art.py` draws it (`chest3`, `chest3_lock`: one silhouette for every finish, at the 3px
  density since 0.32: three boards to the dome, two straps, guards, bands and the mark, no grain and no outline), the bar
  for how calm a picture should be. `tools/import_art.py` builds `assets/` from it (needs Pillow): it copies each picture at
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
  story scenes 320x180, which the importer doubles to 640x360, shown at 2x; the chest 128 cells wide at the 3px
  density (lid 56, body 40, open 80; each cell 3x3 pixels at 1440p), reported to the layout as 96 wide and shown at 2x
  (`ChestView`), with its lock maker's hasp and plate
  laid over it (`ChestView.Lock`) and its own finish for the Crown Chest (`crown`); full figures 96x160 at 4x at the
  counter and in story scenes, and their head crops, 76x76 at 1x in the notebook and Bram's tips and 38x38
  (`sprites/sellers/faces/`) in the queue, the notebook's rows and, at 3x, the character cards; things 32x32 (trimmed) at 2x on the table and the
  shelf, 1x in small slots; curios 28x28 at 2x; upgrade icons 24x24 at 2x (1x on the slip); kind icons 16x16 at 2x;
  UI frames 12x12 at 2x (`UiKit.Px`, 4-pixel margins); the lock faces 160x160 at 2x under the dial (its ring at
  `LockpickPhase.Radius`); effects (`Fx`) keep to the 3px art's grain since 0.32 (particles one or two cells, `Fx.Cell`;
  light as a soft radial glow, `Fx.Glow`; thin rings and rays; flashes tinted and scaled to the find), the coin, sparkle
  and dust sheets at 2-3x; the logo 320x72 at 2x; the window's and the .exe's icons
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
- Nobody has heard 0.36's music or 0.37's text blips: a cloud session has no sound device, so they were written,
  rendered and checked by their numbers (`tools/music`; `Sfx` "talk" and "scratch"). And nothing 0.37 changed about
  feel has met a hand: the typing speed, Bram's cradle, the opening's pauses. All of it is in `docs/release.md`
  ("What only a person can check").
