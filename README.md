# Keywright

A small game about the satisfaction of opening treasure chests. Three hundred years ago the Locking shut every treasure
in the realm into a warded chest, to starve the demon of greed chained behind the Gilded Gate. The chests are still
everywhere, and only a Keywright can open one, once it's rightfully theirs; everyone else sells them unopened, and the
ward lets nothing through but sound. You're one of Bram's kin, with the Key-mark on your hand, and he hands you his
stall. People bring you chests and price them their own way (by what they heard, by weight, by the chest's colour, by
a dream) and tell you where they found them. Shake each one yourself, gently, firmly and hard, find what they missed
or misjudged, work out what their dreams, riddles and tags point at, trade for it, and open it on the spot. Move the shop inward, from the Brindle
crossroads to Thornwick Market, the capital and the Royal Emporium by the Gate, because the stall comes with twenty
years of Bram's unpaid tithe: every fifth evening the Gilded Guild comes for a due that climbs from 20g to 45,000g in
five weeks (25 days: a run is an hour and a half to two hours). Pay them all (or the whole debt at once, the moment you have it) and the stall is free; find the four
shards of the Aldric Key on the way and the Crown Chest opens on your counter. The world is in
[docs/world.md](docs/world.md).

**Status:** Early Access (0.24): a rebuilt world (four places, 27 sellers with their own stories and rules, six kinds
of things a sword-and-sorcery realm would lock away, every one a find worth having, and warded things only a solved
puzzle brings out), runs of five five-day weeks against the Guild's dues (then endless), each day with its business
(the Guild's night, two days with something on in town, Nim's days and the story's); one model for the economy (the
shop's **power index**: what makes ordinary things fetch more puts every seller's price up as much, so the shop's
growth makes the numbers bigger, not the game easier, and a purchase that would leave you unable to afford chests
warns first); **79 curios** in four rarities that make the run's build, six of them ways to play of their own
(enchantment, buying sight unseen, haggling, the perfect read, false bottoms and wards) with a legendary capstone
each, with Nim bringing more of what you buy and his stock growing across runs; restorers that make things come out
better; commissions, seventeen kinds of event, a calendar, and a story; a journal that remembers every regular, find
and curio across runs; nine **Heartsongs**, the rarest
things in the realm (about a third of one a run, worth nothing in gold, each with its own song, its own reveal and its
own use on display), and an ending that needs all nine; four of Bram's kin to play and eight challenge levels each,
every run starting the same; saves, settings and a guided start. The pixel art was made for the game to a written brief
(`art/`: one style, one palette, every asset specified); recorded sounds and the font are from Kenney's CC0 packs (see
[CREDITS.md](CREDITS.md)). Built with Godot
4.7 (.NET / C#), targeting PC on Steam. Design rationale: [docs/design.md](docs/design.md).

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
2. **Shake it yourself.** How fast you drag is how hard you shake (the words under the chest show gentle, firm or
   hard as you go, and which you've tried). Light things (coin jingles, potions slosh, gems
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
5. **End of day:** once the stall has earned 50g, spend your earnings on moving inward (each place brings richer
   customers, a new look and something new), tools (the Witchlight shows what's enchanted, the Lodestone silences iron
   and demon gold, Fine Lockpicks), cabinets, a crier and a night market for more customers, and a buyers' trade that
   puts up everything you sell (brass scales, an auction block, a royal warrant), and restorers who make everything
   likelier to come out better than its chest. From Crownhold, choose which of the shop's synergies to put to work (six
   of a kind at a quality earns one, or a feat: perfect reads, surprises, hidden things found, bargains; each puts up
   what its kind of thing sells for). Sellers hear of everything that makes ordinary things fetch more at your shop,
   and ask that much more (the shop's power index): an upgrade makes the numbers bigger, and anything that would leave
   you less than two typical chests' worth shows its price in red and asks first. Once
   your purse could pay off everything you owe, the Guild offers its **Bookkeeping Engine** for exactly that sum: it
   reads every chest for you, so a won run can be finished fast.
6. **The week:** five days, each with its business: on the fifth evening Mister Vane collects the Guild's tithe; on a
   story week the first morning is the story's. Short, pawn what's on display or sell your curios back; once a run Bram
   will vouch for you; otherwise the Guild seals the shop. Start a day nearly broke and Bram dips into his secret stash
   for you (twice a run). Twice a week (once on a story week) Nim the peddler brings **curios**, the run's build: each
   bends one rule (a sealed bid for chests you never touch, a haggler's knot, a smuggler's hook, a truthglass that pays
   for perfect reads...), each card shows its rarity (common, uncommon, rare, legendary), which ways of trading it's for
   and what it goes with, and costs a fixed sum by rarity plus a share of your purse. Nim notices what you buy and brings
   more like it, and keeps his legendaries for a collector. From the second week, regulars post **commissions**
   (deliver what they want from a chest instead of selling it), and two days a week something happens in town: a fair, Locking Day, the Crown's buyer, a
   tourney, a foreclosure sale, a day of mourning when nobody shakes hard, the crier's reward, the Sleepers' Moon when
   every seller dreams true, Wardens' Day riddles, the Guild's count, a storm, the Stranger with a chained chest... The
   ledger's **calendar** shows this week and next, day by day: the story, the events, Nim's days and the Guild's nights. The Hooded Woman trades shards of the Aldric Key for
   the finds she asks for. Whenever you have all that's left of the debt, you can pay it off and finish early.

**Between runs:** a run ends with a score (dues paid, the debt cleared and how early, the Crown Chest, the shop, the
collection, the Heartsongs), multiplied by the run's challenge level; each of Bram's kin keeps their best. Nothing
carries over that makes a run stronger (0.22 retired the talents): what grows is your judgment, the journal, and Nim's
stock. New game asks who minds the stall:

| Who | Plays |
|---|---|
| Kit, the Ear | the game as it was, with Bram's lessons |
| Hesk, the Scale | can't shake hard or drop a chest, but weighs every one: the weight left over is what he couldn't hear (unlocked by paying the third due) |
| Rosa, the Tongue | everybody haggles with her (though they open higher for her), and a lowball gets a second try; whoever walks out stays away that week (unlocked by clearing the debt) |
| Isolde, the Collection | displays from the first day and visitors pay a coin a point every night, but her collection can't be pawned (unlocked by opening the Crown Chest) |

The first weeks are gentle and a first run usually falls in the middle ones, with a purse that grew, just not fast
enough; it takes a few runs of sharper judgment, and a build that comes together, to clear the debt. Clearing it with someone opens
their next challenge level, and a quarter more to the score. Each level raises the Guild's dues, and deals the run as
many banes as its number (thin crowds, sharp sellers, demon gold about, a wet year that rusts every lock, ...) and as
many boons (welcome gifts on the first morning, Bram's old picks, a slow season, room for one more curio, ...), drawn
from twelve of each for every run and shown before it starts.

Day one is guided: Uncle Bram, who's handing you the stall, brings in three customers with one lesson each (and a
fourth, on passing, opens the second morning). After that, each seller's first visit is a lesson in their habit and
their rule (taught once: a second run doesn't repeat them), and each new part of the shop (moves, lockpicking,
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
| Ledger (sounds, regulars, calendar, collection, curios, synergies) | L, or the Ledger button |
| Skip a seller's lesson | Skip lesson, over Bram's box |
| Pick a lock | Space or left click when the pick is in the green; F for the crowbar |
| Open the chest | It opens by itself (click or Space to hurry it) |
| Keep something for display | Its Keep button after an opening (Swap when the display is full); sell it back from the exhibition at day end |
| Deliver to a commission | Its Deliver button after an opening |
| Buy or sell back a curio | Nim's screen, the morning he visits |
| Pay the Guild, pawn, ask Bram | The Guild's screen, every fifth evening |
| Pay off the whole debt early | "Pay it all off" on the day-end Guild panel (or "Pay it all" on the Guild's night), once you have it |
| Choose synergies | Synergies, at the end of a day |
| Next tip from Bram | Click his box |
| Reread today's event | Click it in the top bar |
| Choose each kind's sound | Settings, Item sounds: click a kind's sound to hear the next |
| Pause, settings, quit to title | Esc, or Menu in the top bar |
| GM mode (testing: +99,999 gold, skip the day, unlock the journal) | Settings, GM mode; then its panel beside the pause menu |
| Choose who minds the stall, and the challenge level | New game: click a character, then - and + |
| Read a seller's page in the ledger | Click them (or Ledger, or L): their line is framed |
| Sell something on display | Click it on the shelf (at the counter or the day's end), then Sell |
| Keep or decline a Heartsong | Keep it or Decline, on its reveal |
| Ask Nim for something else | Show me something else, on Nim's screen (once a visit) |

## Development

The code is split so the game rules can be tested without the engine:

| Folder | What's in it |
|---|---|
| `scripts/Core` | Pure C# rules: kinds, qualities and the shaking rules, chests, where chests come from (`Origins.cs`), sellers and how they judge, trade and tell their stories, the appraisal slip, a careful player's read and strategy, sellers' lessons, synergies, upgrades, the shop's progress and exhibition, the build (`Build.cs`: naming, luck, what things fetch), the run (the Guild's dues, curios, commissions, events, the story), Bram's kin, challenge levels and the score (`Meta.cs`, `Kin.cs`), Bram's hints. No Godot types. |
| `scripts/Phases` | The screens: title, the character screen, counter, lockpicking, opening, end of day, the Guild's night, Nim's curios, story scenes, the end of a run. |
| `scripts/Ui` | Visuals: the shakeable chest, the ledger, Bram, the seller, HUD, particles and styling. |
| `scripts/Audio` | Recorded sounds, the synthesized slosh, and placeholder effects. |
| `scripts/Debug` | Balance simulation (with the lesson test, the builds report and the experience playtest), the shake test and an autoplay bot for screenshots and recordings. |
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
godot --headless --path . -- --shake-test                  # check that shaking follows the rules
xvfb-run -a godot --path . --rendering-driver opengl3 -- \
  --autoplay --days=1 --shots=$PWD/shots                   # play the guided day, save screenshots
# add --start-tier=3 --gold=600 to jump ahead with the tools you'd have by then,
# or --start-week=6 for a shop as a careful player would have it by then (--gold=N sets its purse);
# --character=hesk|rosa|isolde and --challenge=N play one of Bram's kin at a level (--terms=gifts,rust: just those terms);
# --curios=foot,star,ledger starts with those curios
xvfb-run -a godot --path . -- --shell-demo --shots=$PWD/shots_shell  # title, character screen, pause, settings
xvfb-run -a godot --path . -- --vault-demo --quit-at-end --shots=$PWD/shots_vault  # the finale: the Crown Chest
godot --headless --path . --export-release "Windows Desktop" build/windows/Keywright.exe
```

Add `--write-movie movies/day.avi --fixed-fps 60` to the autoplay command to record video with sound, and
`--no-tutorial` to turn off Bram's tips and the scripted first day.
