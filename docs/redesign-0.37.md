# 0.37: what the owner asked for, and what was built

The owner played 0.36 and sent five points, 13 November 2026. This is each of them, what was found on looking, what
0.37 does about it, and what a cloud session couldn't check. The rules and numbers are in `docs/design.md` (the
sections named in italics); the reasons in the world are in `docs/world.md`.

## 1. Dialogue, a character at a time

> Dialogue should appear one character at a time, accompanied by subtle text sounds, as in most pixel-art games.
> Keep the underlying text organized into segments, since click-to-skip and the radio system still need that
> structure.

0.36 said a speech a sentence at a time, each sentence appearing whole. Now each sentence is typed out
(`scripts/Ui/Typewriter.cs`; *What's said*):

- One class types everything that talks: Bram's box, the sellers' bubbles, scenes, talks, letters, the treasury.
- **The segments are untouched.** A speech is still its sentences (`Words.Sentences`): the echo horn keeps the same
  lines, Bram's dots count the same steps, a seller still stirs as the sentence with their price begins, and one
  click still shows the rest of the speech at once and the next goes on.
- The whole speech is laid out before the first letter shows, so nothing reflows as it's typed and a bubble is
  sized once.
- 52 characters a second, half that in Japanese and Chinese; a breath at a comma and a longer one between sentences.
  **Settings > Game > Text speed**: Slow, Normal, Fast, or At once (0.36's way).
- A soft blip every second letter, pitched to who's speaking (Bram in the middle, Vane low, each regular their own,
  the same every time). A letter or one of Bram's notes scratches like a pen. **Settings > Game > Text sounds**
  turns them off. When two speeches type at once, only the later one is heard.

## 2. The opening: anticipation, and payoff

> Anticipation: the reveal sequence should build more excitement about what's inside.
> Payoff: the reveal itself should feel more impactful and satisfying.

Every chest had the same 0.9 seconds of rumble, and a tally that counted up. Nothing said this chest mattered more
than the last, or by how much you'd won (*Opening a chest*).

Anticipation:

- **How much rides on the chest sets the pace**: its colour, what you paid against a working purse, how full it is.
  A plain chest for a few coins still opens briskly; a dear purple one strains for twice as long.
- **The lid thumps** as it strains, seven knocks closer and closer together, into the burst.
- **The tell**: a glint escapes the crack in the colour of the best thing that's better than the chest promised, and
  a violet shimmer if anything is enchanted. It says something is coming, never what. Most chests have none.
- **Everything comes out wrapped in light** and shows what it is only as it lands.
- **The last breath**: before the last thing, the chest draws breath, longest when the chest hasn't paid for itself
  yet and this is the thing that decides it.

Payoff:

- **The stake bar** under the tally fills towards a notch at what you paid: amber short of it, green past it.
  Crossing it rings out "PAID FOR!".
- Each thing that lands rings a note higher, so a full table climbs a scale.
- **How many times over**: from twice what it cost, the verdict is stamped "x3.4", with a shower of coins from three
  times and the whole room from five, when the profit is a real share of a working purse.
- **Enchanted things flow.** "Their enchanted status is currently too difficult to notice": light now runs over an
  enchanted thing and licks up off its edges, with motes rising behind it, everywhere one is shown (the opening's
  table, the shelf, the collection, a commission's chooser).

## 3. Repetition: builds and automation, not a ration

> The hearing-ability mechanic serves no meaningful purpose. Repetitive gameplay is a real issue, but restricting
> how many times players can listen is not the solution. [...] Introduce collectibles that reduce repetitive work
> for each build archetype. Blind-buying builds already eliminate the need to listen, so they don't need that
> particular convenience. [...] Also, explore other ways to reduce repetition that fit naturally with the core
> gameplay.

**The counted listens are gone**, with everything built on them: the meter, the Tea Kettle and the Listening Screen
(refunded in a save that had bought them), the Echo Stone and the Lamb's-Wool Plugs, a head cold and a rested ear
among the challenge terms. A chest can be shaken as often as its seller will stand.

**Each way of trading has a curio that does its share of the routine** (*Listening is free, and a build does the
routine*; `scripts/Core/Hands.cs`):

| Way of trading | Curio | | What it does |
|---|---|---|---|
| the perfect read | Scrivener's Quill | uncommon | ticks each thing on the slip as you hear it, and counts them |
| the perfect read | Pocket Engine | legendary (a hundred perfect reads, across seasons) | every chest's real contents are on the slip as it comes in: the appraisal sheet fills itself out perfectly |
| enchantment | Lantern Moth | rare | every enchanted thing is starred on the slip as the chest comes in, witchlight or not |
| false bottoms | Toll-man's Gauge | rare | a false bottom is marked as the chest comes in: no drop |
| wards | Wardens' Primer | rare | names the warded thing a sign points at |
| haggling | Factor's Abacus | rare | your offer starts on the least the seller would take, and they take it |
| sight unseen | none | | it never touches the chest |

They take a slot and come from Nim like any curio, and open after a first season (which is for learning the motions
by hand). They are the first curios that tell you something of what a chest holds. What still holds is the rule
behind that one: nothing says what a chest is *worth* before you buy it. How good each thing is stays the opening's.

**Other ways the routine got shorter:**

- **Bram's cradle.** A button beside the chest (or the act key) rocks it gently, firmly and hard, a second each;
  you still do the listening. From Thornwick in a first season, from the first morning in any later one. It rocks
  only once, gently, where shaking has a price (a goblin's fee, a rotten chest, a seller in a hurry), and never
  drops a chest.
- **The slip remembers what you heard**: a kind's row darkens and gets a dot once you've heard one. 0.36's playtester
  asked for it.
- **A right-click takes one off a count.**
- **Later seasons skip what's known** (point 4): no first week of one-thing chests, no exposition twice.

## 4. Repeat runs, and what stays unlocked

> Distinguish between three player states: first-ever run; subsequent runs before the player has beaten the game;
> subsequent runs after. The current problems mainly affect the second state.

The three states are in the code now (`SeasonsBefore == 0`, `GameState.Veteran`, `CampaignDone`; *A season after the
first*). Found by having the bot play two seasons running on one profile (`--profile`):

- **Tutorials.** Bram's tips and lessons already didn't repeat. The story's exposition did: his welcome, Vane's
  introduction of the debt, the summons, the Guild's letter, Hollis's explanation. Each now has a short form for a
  keywright who's heard it.
- **Features opened mid-run are open from a later season's first morning** ("the trade's ways", `GameState.Ways`):
  locks, false bottoms, two or three of a kind in a chest, enchanted things. What still comes with each market is
  its goods, its people, its prices and its tools.
- **"Multiple-item contents are already unlocked, but the appraisal sheet does not support marking them correctly."**
  Tale chests and the Stranger's chest could hold two of a kind at markets where the slip ticked a kind once. The
  slip now counts wherever a chest can hold more than one, and those chests hold only what the slip can say.
- **Bram's drawer.** On a later season's first morning Bram offers three plain curios, each for a different way of
  trading: take one. A build starts on day one, and seasons start differently.
- **Nim from the first night and commissions from the first week's third morning**, in a later season.
- **Enchanted items flow** (point 2).

## 5. Saves, and GM tools

> Implement a save-slot system that allows players to create a new save and start completely fresh. In GM mode, add
> a one-click option to fulfill quest requirements, including the true-ending checks. Hide GM mode behind an entry
> point such as F1, rather than exposing it in the settings menu.

- **Three saves** (*Title, saves and settings*). The title's bottom-left button ("Save 1") opens them: a card each
  with what's on it, **Play this save**, **Start a new save** on an empty one, and **Erase, and start fresh** (asked
  first; the files are set aside under another name, not destroyed). An existing save is Save 1, untouched.
- **GM mode is behind F1**, during a season. It's gone from Settings, lasts until the game closes and isn't saved.
- **Fulfil the quests** (the GM panel): this week's commissions filled and paid, the treasury key's four pieces,
  Bram's account paid off, and every check of the true ending met, so the final opening comes that night.

## Balance

Free listening and a minute saved a day made the careful player's season too safe at 0.36's dues (98% cleared), so
**the last due is 6,600g** (5,000g in 0.36); the first three are as they were. The game's own reports on the finished
build, against 0.36's:

| | 0.36 | 0.37 |
|---|---|---|
| The dues | 40, 170, 700, 5,000g | 40, 170, 700, 6,600g |
| The careful player clears (`--sim=quick`) | 90% | 93% |
| with no curios / no tales / neither | 79 / 61 / 31% | 86 / 73 / 49% |
| A learner at 0.75 / 0.5 / a first-timer, first season | 66 / 43 / 20% | 78 / 53 / 21% |
| The same hands in a later season (`--veteran --ways=4`) | the same | 86 / 66 / 33% |
| A first season's length (`--sim=experience`) | 104 minutes, 108 customers | @@EXP1@@ |
| A second and a third | 94 and 89 minutes | @@EXP23@@ |
| A career first clears the debt (`--sim=careers`) | the second or third season | @@CAREERS@@ |
| The challenge levels, as drawn | 77, 57, 43, 35, 18, 16, 9, 6% | @@LEVELS@@ |
| Curios | 81 | 85 |
| Challenge terms | 13 banes, 13 boons | 12 and 12 |

## What couldn't be checked from here

A cloud session has no sound device and no hand on a mouse. Worth the owner's time, in this order:

1. **The text blips.** Written as a 50ms synth tone and checked by their numbers (played at -15dB, under every chest
   sound). Nobody has heard them. If they grate, Settings > Game > Text sounds, or one line in `Sfx` ("talk").
2. **The typing speed.** 52 characters a second read well in recorded frames; whether it reads well is a reader's
   call. Settings > Game > Text speed.
3. **Bram's cradle in the hand**: relief, or the game playing itself?
4. **The opening's pace over a real session**: the breath before the last thing, and whether a chest with no glint
   feels flat now that a glint means something.
5. **Whether the routine curios feel like builds.** The numbers say they cost little and save a lot of clicks.

## Status

- [x] 1. Typed dialogue, text sounds, text speed (all speakers; segments kept)
- [x] 2. The opening: stakes, thumps, the tell, wrapped things, the stake bar, the last breath, times over; enchanted
  things flow
- [x] 3. The ear removed; six routine curios; Bram's cradle; heard marks; right-click
- [x] 4. Three states; the trade's ways; the slip counts what chests hold; the drawer; short exposition
- [x] 5. Three saves; GM behind F1; Fulfil the quests
- [x] Rebalance, translations (fr, ja, zh_CN, zh_TW), tests, docs, version 0.37
