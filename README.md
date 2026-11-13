# Keywright

A small game about the satisfaction of opening treasure chests. Three hundred years ago the Locking shut every treasure
in the realm into a warded chest, to starve the demon of greed chained behind the Gilded Gate. The chests are still
everywhere, and only a Keywright can open one, once it's rightfully theirs; everyone else sells them unopened, and the
ward lets nothing through but sound. You have the Key-mark on your hand, and Bram hands you his stall. People bring
you chests and price them their own way (by what they heard, by weight, by the chest's colour, by a dream) and tell
you where they found them. Shake each one yourself, find what they missed or misjudged, work out what their dreams,
riddles and tags point at, trade for it, and open it on the spot.

The stall comes with twenty years of Bram's unpaid tithe to the Gilded Guild. A season is four weeks of five days,
and on every fifth night the Guild comes for a due: 40g, 170g, 700g and 5,000g. Each one paid, its carters move the
stall a market further in: from the Brindle crossroads to Thornwick Market, to Crownhold, to the Royal Emporium by the
Gate. Pay all four and Bram's account is closed; earn the four pieces of the treasury key on the way and the Guild's
carriage takes you to its treasury on the last night, where the story turns. A season is about an hour and a half.
The world is in [docs/world.md](docs/world.md).

**Status:** Early Access (0.36). 0.36 is a redesign for pace and for decisions at the counter
([docs/redesign-0.36.md](docs/redesign-0.36.md) has what a review of 0.35 found and what changed):

- **A season is four weeks, a market each.** The moves inward aren't bought any more: paying the week's due moves the
  shop (0.35's season was seven weeks and two and a half hours, and most of a first one was spent at Thornwick).
- **The ear.** A keywright's ear is good for eight careful listens a day. A strength you haven't tried on a chest, or
  a drop, costs one; shaking again the same way is free. So which check to make on which chest is the day's
  decision, what a seller misses is what tells you, and a chest bought on less than everything is a real reveal.
- **Openings delight more than they disappoint**: a thing comes out better than its chest more often than worse.
- **A speech says itself**, a sentence after another: one click a speech, not one a sentence.
- **Music and ambience**: a theme for each market, the night, the Guild and the treasury, written and rendered in
  code (`tools/music`), on its own volume.

Under it, what earlier versions built: four places and 27 sellers, each with one way of judging a chest, a rule of
their own and true stories; six kinds of thing a sword-and-sorcery realm would lock away, warded things only a solved
puzzle brings out, mimics and Guild crates; one model for the economy (the shop's **power index**: what makes ordinary
things fetch more puts every seller's price up as much); **81 curios** in four rarities that make the season's build,
with Nim bringing more of what you buy; **tales** that run across a season with choices that cost and change things,
standing with nine powers, keepsakes, and a season's end that says what became of everyone; a **campaign across
seasons** ([docs/narrative.md](docs/narrative.md)): the treasury that swallows you, the year that turns back, the
Inquiry, and the next day; nine **Heartsongs**; four keywrights to play and eight challenge levels each, every season
starting the same; translations into French, Japanese and Simplified and Traditional Chinese; saves that survive
crashes, a full settings menu, a controller that does everything the mouse does, Steam achievements and rich presence,
and Windows and Linux builds from CI on every push ([docs/release.md](docs/release.md)). The pixel art is made for the
game to a written brief (`art/`); the chest is drawn in code; recorded sounds and the font are from Kenney's CC0 packs
(see [CREDITS.md](CREDITS.md)). Built with Godot 4.7 (.NET / C#), targeting PC on Steam. Design rationale:
[docs/design.md](docs/design.md).

Keywright was called LootFall until 0.18, on a branch of [Andvc/LootFall](https://github.com/Andvc/LootFall); that name
and repository now belong to a different game. Saves and the profile from then come over on the first start.

## The loop

1. **A customer brings a chest**, tells you where it came from (a millpond, the Barrow, a goblin warren, the Maw's
   hoard; the place says what it tends to hold) and prices it their way: some say how hard they
   shook it and what they heard, some heft it, some go by its colour, some dream, some have no idea. The
   chest's colour (wood, green, blue, purple) says how good things inside tend to be. Each customer has one habit worth
   learning, and many have a rule of their own: a rotten chest that stands two seconds of shaking, a goblin who charges
   for every shake, a smuggler who packs his chests in straw (everything stirs a strength later), a pawn ticket that
   counts every article (false bottom and all), a huntress's game scale, an imp with an hourglass, an archmage's wager,
   a knight's claim on the family sword, a devil's due, an old man whose price is whatever your slip says. What they tell
   you is always true: working out what it means is the game.
   Some chests hold a **warded thing**: it makes no sound and weighs nothing, and it only comes out for someone who names
   it. Whoever warded it left a sign, and working it out is a puzzle: a druid's dream (a river means a potion), a
   Thornwick dowry (always coin, a cloak and a potion), an apprentice's packing tag, grandmother's riddle on the lid, a
   ward-roll with every other letter faded, a bat-eared demon's echoes, a cartomancer's cards. If the sign names
   something you can't hear, mark one more of it on the slip; open the chest, and the ward gives it up.
2. **Shake it yourself, and choose how.** How fast you drag is how hard you shake (the words under the chest show
   gentle, firm or hard as you go, and which you've tried). Your ear is good for eight careful listens a day (the
   pips over the chest): each strength you haven't tried on this chest costs one, and so does a drop; shaking again
   the same way is free. A hard shake stirs everything, but a clank drowns the delicate things; what the seller
   missed tells you where a listen is worth spending. Light things (coin jingles, potions slosh, gems
   tinkle) rattle at a gentle shake; weapons only clank at a firm one, and their clanking drowns out anything delicate;
   soft things (cloaks flump, spells rustle) only sound at a hard shake. Lift it high and let go to drop it: a hollow
   boom means a false bottom (but glass may break). As the shop grows, more to work out: two or three of a kind (count
   the rattles), river sand that rustles like a scroll, enchanted things that only a witchlight shows, demon gold.
3. **Correct the slip and trade.** Mark how many of each kind are in there; the slip adds them up at what they'd be
   worth if they're as good as the chest's colour promises, and an odds bar (red to green) says how likely it is to be
   worth more than the price, or your offer. Then pay their price, haggle, or name your own, depending on who it is.
   Pass, and they take it away unopened: your gut tells you, on a red-to-green scale, whether that was wise.
4. **Pick the lock and watch it burst open:** everything flies out with its true quality and a tick or a cross
   against your slip; anything better than its chest climbs, quality by quality, with more fuss for every step, and
   anything worse sags as its colour drains. The profit is stamped across the screen with your gut's verdict and why
   the seller missed it. Keep grand things for your exhibition (enchanted ones count double).
   Once in a long while, about one run in three, a chest sings: a sound like nothing else, that only you can hear.
   That's a **Heartsong**, one of nine things loved more than gold that the Locking was sung into. Nobody's priced it;
   it comes out last, in mythic red, with what it does on display, and it's yours to keep (free) or let go. No merchant
   would pay a coin for one, but singing in your shop each one changes the trade (two more customers a day, coin and
   gems at double, a bell that counts what's in every chest...). Find all nine, over your runs, and the Crown Chest
   ends differently.
5. **End of day:** from Thornwick on, spend your earnings on tools (the Witchlight shows what's enchanted, the
   Lodestone silences iron and demon gold, Fine Lockpicks), a kettle and a listening screen for more listens,
   cabinets, a sign and a crier for more customers, and a buyers' trade that
   puts up everything you sell (brass scales, an auction block, a royal warrant), and restorers who make everything
   likelier to come out better than its chest. From Crownhold, choose which of the shop's synergies to put to work (six
   of a kind at a quality earns one, or a feat: perfect reads, surprises, hidden things found, bargains; each puts up
   what its kind of thing sells for). Sellers hear of everything that makes ordinary things fetch more at your shop,
   and ask that much more (the shop's power index): an upgrade makes the numbers bigger, and anything that would leave
   you less than two typical chests' worth shows its price in red and asks first. Once
   your purse could pay off everything you owe, the Guild offers its **Bookkeeping Engine** for exactly that sum: it
   reads every chest for you, so a won run can be finished fast.
6. **The week:** five days at one market, each with its business: on the fifth evening Mister Vane collects the
   Guild's tithe, and with it paid the Guild's carters have the shop at the next market by morning (the road, on the
   first morning there, shows where you are and what the place brings); the first morning of weeks 1, 2 and 4 is the
   story's. Short, pawn what's on display or sell your curios back; once a run Bram will vouch for you; otherwise the
   Guild seals the shop. Start a day nearly broke and Bram dips into his secret stash
   for you (twice a run). Every night, after the shutters are down, Nim the peddler brings **curios**, the run's build
   (one a night at first, more once he has a lantern, a stool and a standing order): each
   bends one rule (a sealed bid for chests you never touch, a haggler's knot, a smuggler's hook, a truthglass that pays
   for perfect reads...), each card shows its rarity (common, uncommon, rare, legendary), which ways of trading it's for
   and what it goes with, and costs a fixed sum by rarity plus a share of your purse. Nim notices what you buy and brings
   more like it, and keeps his legendaries for a collector. From the second week, regulars post **commissions**
   (deliver what they want from a chest instead of selling it), and two days a week something happens in town: a fair, Locking Day, the Crown's buyer, a
   tourney, a foreclosure sale, a day of mourning when nobody shakes hard, the crier's reward, the Sleepers' Moon when
   every seller dreams true, Wardens' Day riddles, the Guild's count, a storm, the Stranger with a chained chest... The
   notebook's **calendar** shows the Guild's debt and this week and next, day by day: the story, the events, who said
   they'd come back and the Guild's nights. The Guild's assessors send a piece of the treasury key for
   each of four marks of the trade (a hidden thing read true, a stolen chest settled, a commission filled, an old lock
   picked without force). Whenever you have all that's left of the debt, you can pay it off and finish early.
7. **The town:** not everyone comes to sell. People come with their troubles (a widow's son in the Guild's debt-house, a
   millpond boy with the Key-mark and a Mark-Finder on his trail, a sergeant hunting the Magpie), come back when they
   said they would, ask things of you and remember what you did; the best tools are offered at the counter by the
   people who make them; a chest your mark goes cold on isn't the seller's to sell (send for the Wardens, or it waits in
   your back room). The notebook's **Town** page shows where you stand with each power, the notices on the board, and
   every tale so far.

**Between runs:** a run ends with a score (dues paid, the debt cleared and how early, the key's pieces and the
treasury, the shop, the collection, the Heartsongs), multiplied by the run's challenge level; each keywright keeps
their best. Nothing
carries over that makes a run stronger (0.22 retired the talents): what grows is your judgment, the journal, and Nim's
stock. New game asks who minds the stall:

| Who | Plays |
|---|---|
| Kit, the Ear | the game as it was, with Bram's lessons |
| Hesk, the Scale | can't shake hard or drop a chest, but weighs every one: the weight left over is what he couldn't hear (unlocked by paying the third due) |
| Rosa, the Tongue | everybody haggles with her (though they open higher for her), and a lowball gets a second try; whoever walks out stays away that week (unlocked by clearing the debt) |
| Isolde, the Collection | displays from the first day and visitors pay to see her collection every night, but it can't be pawned (unlocked by opening the great chest at the Guild's treasury) |

The first three dues are gentle and a first season usually falls at the last, at the Emporium, with a purse that
grew, just not fast enough; a second or third, with sharper judgment and a build that comes together, clears it, and
that's where the story turns. Clearing it with someone opens their next challenge level, and a quarter more to the
score. Each level raises the Guild's dues by a fifth, and deals the run as many banes as its number (thin crowds,
sharp sellers, demon gold about, a wet year that rusts every lock, a head cold, ...) and as many boons (welcome gifts
on the first morning, Bram's old picks, a slow season, a rested ear, room for one more curio, ...), drawn from
thirteen of each for every run and shown before it starts: the levels are where the game gets hard.

Day one is guided: Uncle Bram, who's handing you the stall, brings in three customers with one lesson each (and a
fourth, on passing, opens the second morning). After that, each seller's first visit is a lesson in their habit and
their rule (taught once: a second run doesn't repeat them), and each new part of the shop (the ear, lockpicking,
the display shelf, synergies, the Guild, curios, commissions) comes with a word from Bram when it opens up. The shop
saves at the start of every day; your best runs are kept on the title screen.

## Controls

| Action | Input |
|---|---|
| Shake a chest | Drag it with the left mouse button: slowly for gentle, fast for hard |
| Drop it | Lift it high and let go |
| Count a kind on the slip | Click its row: one more each click (up to three from Crownhold), then back to none |
| Mark one of a kind enchanted | Click the star on its row (once you have the Witchlight, which stays lit) |
| Make an offer | Set the slider, then Offer |
| Lodestone | Button on the right, once you own it |
| Notebook (sounds, regulars, calendar and the debt, the town, synergies) | N (Y on a controller), or the Notebook button on the slip |
| The Guild's debt, week by week | Click the due in the top bar: the notebook opens at it |
| Collection (finds, curios, Heartsongs, every run) | Collection, on the title screen or in the pause menu |
| Skip a seller's lesson | Skip lesson, over Bram's box |
| Pick a lock | Space, X on a controller, or left click when the pick is in the green; F or RB for the crowbar |
| Open the chest | It opens by itself (click, Space or X to hurry it) |
| Keep something for display | Its Keep button after an opening (Swap when the display is full); sell it back from the exhibition at day end |
| Deliver to a commission | Its Deliver button after an opening |
| Buy or sell back a curio | Nim's screen, the morning he visits |
| Pay the Guild, pawn, ask Bram | The Guild's screen, every fifth evening |
| Pay off the whole debt early | "Pay it all off" on the day-end Guild panel (or "Pay it all" on the Guild's night), once you have it |
| Choose synergies | Synergies, at the end of a day |
| Next tip from Bram | Click his box |
| Reread today's event | Click it in the top bar |
| Choose each kind's sound | Settings, Item sounds: click a kind's sound to hear the next |
| Pause, settings, quit to title | Esc (Start on a controller), or Menu in the top bar; Esc or B closes what's open |
| With a controller | The left stick moves the pointer, A clicks and drags (grab a chest and flick the stick to shake it), the D-pad nudges it, the right stick scrolls |
| Rebind keys and buttons | Settings, Controls: click one, then press the key or button |
| Change the language | Settings, Game (from the next start) |
| Achievements | Collection, Achievements (and on Steam) |
| Credits | Credits, on the title screen |
| GM mode (testing: +99,999 gold, skip the day, fill the notebook and the collection) | Settings, GM mode; then its panel beside the pause menu |
| Choose who minds the stall, and the challenge level | New game: click a character, then - and + |
| Read a seller's page in the notebook | Click them (or Notebook, or N): their line is framed |
| Sell something on display | Click it on the shelf (at the counter or the day's end), then Sell |
| Keep or decline a Heartsong | Keep it or Decline, on its reveal |
| Ask Nim for something else | Show me something else, on Nim's screen (once a visit) |

## Development

The code is split so the game rules can be tested without the engine:

| Folder | What's in it |
|---|---|
| `scripts/Core` | Pure C# rules: kinds, qualities and the shaking rules, chests, where chests come from (`Origins.cs`), sellers and how they judge, trade and tell their stories, the appraisal slip, a careful player's read and strategy, sellers' lessons, synergies, upgrades, the shop's progress and exhibition, the build (`Build.cs`: naming, luck, what things fetch), the run (the Guild's dues, curios, commissions, events, the story), Bram's kin, challenge levels and the score (`Meta.cs`, `Kin.cs`), Bram's hints. No Godot types. |
| `scripts/Phases` | The screens: title, the character screen, counter, lockpicking, opening, end of day, the Guild's night, Nim's curios, story scenes, the end of a run. |
| `scripts/Ui` | Visuals: the shakeable chest, the notebook and the collection, Bram, the seller, HUD, particles and styling. |
| `scripts/Audio` | Recorded sounds, the synthesized slosh and Heartsongs, and the music's player (`Music`; the tracks are made by `tools/music`). |
| `scripts/Debug` | Balance simulation (with the lesson test, the builds report and the experience playtest), the shake, save, language and controller tests, and an autoplay bot for screenshots and recordings. |
| `scripts/` | The game's frame: `Main` (start, phases, the run's files), `Settings`, `Controls` (actions and bindings), `Localization` (languages and fonts), `Platform` (Steam), `Storage` (crash-safe files). |
| `locale/` | The text for translators (`keywright.pot`, made by `tools/loc`) and their translations; `locale/README.md` is for them. |
| `steam/` | The Steam library (Windows and Linux), rich presence's wording, and the Steamworks set-up (`steam/README.md`). |
| `tools/loc` | Checks that every text the player reads is translatable, and writes the template. |
| `assets/` | Sprites, portraits, backgrounds, sounds, icons, UI frames and font, built by `tools/import_art.py` from `art/generated/` and Kenney's CC0 packs. |
| `art/` | The art's brief (its style, palette, and every asset's spec and prompt), the tools that fit and check a picture, and the finished pictures in `art/generated/`. |

Useful commands (Linux; run `tools/setup-cloud.sh` first in a fresh container):

```bash
dotnet build                                               # compile
godot --headless --path . -- --sim                         # balance report
godot --headless --path . -- --sim=quick                   # the economy in a few minutes
godot --headless --path . -- --sim=builds                  # each curio, each way of trading, pairs, blind buyers with builds
godot --headless --path . -- --sim=experience              # a player's first three runs: pace, load, freshness, peaks
godot --headless --path . -- --sim=trace --seed=1001       # one careful run, day by day
godot --headless --path . -- --sim=terms                   # each challenge boon and bane alone, then whole levels
godot --headless --path . -- --sim=learners                # clear rates from a first-timer to the careful player
dotnet run --project tools/curiotest                       # every curio's card against the game
godot --headless --path . -- --shake-test                  # check that shaking follows the rules (11 minutes)
godot --headless --path . -- --save-test                   # saves: crash-safe writes, backups, old versions
godot --headless --path . -- --loc-test                    # the language layer, with a made-up translation
dotnet run --project tools/loc -- check                    # every text the player reads goes through Loc
dotnet run --project tools/loc -- extract                  # rewrite locale/keywright.pot
xvfb-run -a godot --path . -- --pad-test --no-tutorial --seed=5  # play the counter with a simulated controller
xvfb-run -a godot --path . -- --ear-test --no-tutorial --seed=5  # shake the chest the ways hands do: which listens are spent
xvfb-run -a godot --path . --rendering-driver opengl3 -- \
  --autoplay --days=1 --shots=$PWD/shots                   # play the guided day, save screenshots
# add --start-tier=3 --gold=600 to jump ahead with the tools you'd have by then,
# or --start-week=3 for a shop as a careful player would have it by then (--gold=N sets its purse);
# --character=hesk|rosa|isolde and --challenge=N play one of Bram's kin at a level (--terms=gifts,rust: just those terms);
# --curios=foot,star,ledger starts with those curios
xvfb-run -a godot --path . -- --shell-demo --shots=$PWD/shots_shell  # title, character screen, pause, settings
xvfb-run -a godot --path . -- --treasury-demo --shots=$PWD/shots_treasury  # the season's last night: the treasury
python3 tools/music/make.py                                # write and render the music and ambience again (numpy, ffmpeg)
python3 tools/po_merge.py --new                            # texts the translations in locale/ still lack
godot --headless --path . --export-release "Windows Desktop" build/windows/Keywright.exe
godot --headless --path . --export-release "Linux" build/linux/Keywright.x86_64   # then add steam/'s library
```

`--pseudo` on any of these shows every text pseudo-translated (bracketed and a third longer), to find untranslated
text and tight layouts. CI (`.github/workflows/build.yml`) runs the checks and builds Windows and Linux on every push.

Add `--write-movie movies/day.avi --fixed-fps 60` to the autoplay command to record video with sound, and
`--no-tutorial` to turn off Bram's tips and the scripted first day.
