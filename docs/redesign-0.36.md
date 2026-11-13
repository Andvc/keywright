# 0.36: the redesign

The owner, 12 November 2026: the game isn't engaging or fun enough, and lacks polish and completeness; reconsider
anything. This is what a review of 0.35 found, what 0.36 changes and why, and where the work stands. The rules and
numbers it settles on go into `docs/design.md` as each part lands.

## What the review found

Played by hand from a fresh install (`tools/playtest/pt`), by the bot at every shop, and through the game's own
reports (`--sim=experience`, `--sim=trace`, `--sim=quick`).

1. **A season is too long and too flat.** The experience report's own last line: "A first run takes about 2.4 hours:
   too long to want another." Its 30 first-timers all lose (median week 6, after 143 minutes and 169 customers), 93%
   lose their second run and 90% their third: seven hours of losing. A learner reaches Thornwick on day 8 and is still
   there on day 30, with one new thing a day; the report's engagement score falls from 28 on day 1 to about 10 from day
   20. A high point comes every eleven minutes.
2. **The counter is a procedure, not a decision.** Every check is free, so the right play is always all of them:
   gentle, firm, hard, drop, tick what sounded, read the odds bar, buy if it's green. A careful buyer profits on 85 to
   92% of chests, so the opening confirms what the slip already said. What each seller misses is in the notebook, but
   nothing makes knowing it matter.
3. **The story's hook comes after the first clear,** which the careers report puts around the sixth run: about
   fourteen hours in. The playtester called the remembered season "far more gripping than the first".
4. **There's too much talk between chests.** 0.35's sentence-at-a-time dialogue takes a click a sentence: thirteen
   before the first chest, and every tip and tale after it.
5. **It's silent.** No music and no ambience, in a game played for hours behind one counter.
6. **Most surprises are letdowns.** A thing comes out worse than its chest 16% of the time and better 7%: the
   opening's commonest event is "WORSE...".

What works, and stays: the three rules of sound and the sellers' ways of judging, the slip and its odds bar, the
openings, the world and its people, the campaign across seasons (the summons, the treasury, the Inquiry, the next
day), curios as the build, the art.

## What 0.36 does

### 1. A season is four weeks, and each week is a market

Twenty days. The stall trades at Brindle in week 1, Thornwick Market in week 2, Crownhold in week 3 and the Royal
Emporium in week 4. The move inward isn't bought any more: **paying the week's due moves the shop** (the Guild's
carters come that night; the Guild wants its keywright at the Gate by the season's end, which is the story's truth
too). Four dues, the last the wall. A season is about an hour and a half, everyone who keeps paying sees every
market, and a season that fails, fails at the last due with all four markets seen, not after two hours at Thornwick.

- Level 0 is the story's difficulty: most players should clear by their second or third season, which is when the
  treasury first swallows them and the campaign starts. The eight challenge levels are where it gets hard.
- Fewer tales a season (they were crowded: "eight tales began in ten days"), staged to fit; the campaign's beats
  keep their order (the summons on the first Guild night, the bribe on the second, the bargain on the third, the
  gathering in week 4).

### 2. The ear: you can't check everything

A keywright's ear is good for so many careful **listens** a day. Each new check on a chest spends one: a strength
you haven't tried on it, a drop, the lodestone. Shaking again at a strength you've tried is free (counting rattles
costs nothing), and so is a seller's lesson. With none left you trade on what you know: who's selling, where it came
from, the price, the colour.

So which check to make is the decision, and what a seller misses is what tells you: Ada priced what a gentle shake
stirs, so one hard shake hears everything she didn't; a clank at a hard shake means the delicate things are drowned,
and a gentle shake is worth a listen. A beginner who tries all three strengths on every chest runs out before the
last customer; someone who knows the regulars doesn't. Chests bought on less than everything make openings a reveal
again, and the sight-unseen curios a way of stretching a day.

### 3. Openings that delight more than they disappoint

The quality odds lean up, not down; sellers price the average as before, so the economy holds and the commonest
surprise is a good one.

### 4. Less talk between chests

A speech plays its sentences one after another by itself (one click a speech, a click to finish it at once). The
opening scene keeps its four speeches, each of which sets up something the campaign calls back to (the mark, the
trade in a sentence, Bettany, Bram's tap): four clicks before the first chest where 0.35 took thirteen.

### 5. Music and ambience

A soundtrack made in code like the Heartsongs' (a theme for the title, each market, the night, the Guild and the
treasury) and ambience beds for each place, on their own bus with a Music volume.

## Where it landed

Everything above is in 0.36. The numbers are the game's own reports on the finished build (`docs/design.md` has them
in full, with how each was measured), against the same reports on 0.35:

| | 0.35 | 0.36 |
|---|---|---|
| A season | 7 weeks, 35 days | 4 weeks, 20 days, a market each |
| A first season, played by a learner (`--sim=experience`) | 143 minutes, 169 customers, lost by all 30 | 104 minutes, 108 customers, cleared by about one in five |
| High points an hour, seasons 1 / 2 / 3 | 5.3 / 5.1 / 5.7 | 8.5 / 9.9 / 9.9 |
| Time in stale stretches (a first season) | 6% | none |
| The careful player clears (`--sim=quick`) | 70% | 90% |
| A learner at 0.75 / 0.5 / a first-timer | 55% / 13% / none | 66% / 43% / 20% |
| A chest bought by a careful player turns a profit | about nine in ten | 86%, on fewer checks (a perfect read on 63%) |
| A career first clears the debt (`--sim=careers`) | around the sixth season | the second or third |
| The treasury first swallows you (the story's turn) | around the sixth season, fourteen hours in | the second to fourth, five or six hours in |
| The true ending (the next day) | around the thirteenth season | around the tenth |
| The challenge levels, as drawn (the careful player) | | 77, 57, 43, 35, 18, 16, 9 and 6% |
| Dialogue | a click a sentence | a click a speech |
| Music | none | seven themes and four beds of ambience |

What's different from the plan above, and why:

- **The dues are 40, 170, 700 and 5,000g** (a dozen tunings, `--sim=learners --dues=...`): the first three are paid
  by nearly everyone, so everyone sees all four markets, and the last is the wall.
- **Five customers a day past the stall** (four there), not six: with listens counted, more customers is more to
  choose between, and a season stays an hour and a half.
- **Six tales a season, not eleven**, and each keeps its shape in fewer days (`GameState.Wait`).
- **The ear grew its own shelf**: a Tea Kettle and a Listening Screen for more listens, an Echo Stone and Lamb's-Wool
  Plugs among the curios, a head cold and a rested ear among the challenge terms, and at the opening a closed eye,
  not a cross, on what no check you made could have told you of.
- **The road** (`RoadPhase`): a screen of its own on a market's first morning, since the move is now the week's
  reward and not a purchase.
- **Heartsongs are twice as likely a chest**, so a season of a hundred chests finds one as often as 0.35's did
  (about one season in three).
- **The challenge terms were weighed again**, at a harder level's dues (at level 0 nothing shows): each bane costs
  the careful player one to eight points and a boon gives up to eight, and three that were out of line were changed.
- **The sim's careful player was put right** before the dues were settled: it paid the Guild before the day's end
  (the game has the day's end first), which once a due moves the shop had it fitting out the next market a day
  early, and its saving rule still measured a bad day by 0.35's Emporium. Corrected, it showed two things the old
  order hid: the dues could stand a little higher, and a learner's first day at Thornwick was badly overloaded
  (below).
- **A learner's first days at a market are quieter**: no locked chests on the first, no warded-thing puzzle in the
  first two. Five days is less room for Thornwick's lessons than 0.35's eight or more.

What couldn't be checked from a cloud session, and is worth an hour of the owner's time before anything else:

1. **How the music sounds.** It was written and rendered in code and checked by its numbers (peaks, loudness, where
   its energy sits, the loops' seams) and its spectrograms. Nobody has heard it. It has its own volume (60% to start).
2. **How the ear feels in the hand.** A fresh-eyes playtest (an agent with the playtest kit and nothing else) found
   that the ear's first version often didn't register a shake at all at a low frame rate: the meter lit and nothing
   counted. That's fixed (a shake is measured inside the shake box now, a physics step at a time) and `--ear-test`
   drives a pointer the ways hands move, at sixty and twelve frames a second, to keep it fixed. A pointer on a path
   still isn't a person: `CounterPhase.ListenHold` and `ShakeBox.TurnHold` are the numbers if a listen ever goes by
   accident or a shake you meant doesn't count.
3. **Whether level 0 is too gentle** for a player who already knows the game. It's tuned so a new player clears on a
   second or third season and meets the story; the challenge levels are where it gets hard.

## The fresh-eyes playtest

Run on the finished build by an agent given `tools/playtest/README.md` and nothing else (177 actions: the first three
days from a fresh install, then the first day at Thornwick). "I would keep playing. Working out what each seller
failed to notice is a strong core, and Thornwick is where it gets properly interesting." It worked out the ear's rule
by itself ("the rule is fair, and it made me think. I skipped gentle on Ada's chests because she had already priced
that, and used one hard shake because a hard shake stirs everything"), the four dues and the road, and that the
Guild is the villain.

Its top finding was the bug above ("I could not tell when a shake counted"). The others it found that were fixed:
Bram's "Pass" lesson contradicted by a town event that made buying right; a tale's first speech bubble blank until a
button was pressed; a wide speech bubble cutting off the ear's bar; a lost season ending on nothing but the way back
to the title. Left for the owner's judgement (`docs/design.md`, *Playtesting the experience*): the first two days
are very safe, nothing records what was heard once the words fade, and a handful of older wordings.

## Status

- [x] 1. The four-week season (Core, UI, tales, campaign, saves, sims)
- [x] 2. The ear (Core, counter, HUD, strategy, lessons, sims)
- [x] 3. Quality odds
- [x] 4. Dialogue pacing
- [x] 5. Music and ambience
- [x] Rebalance (`--sim=quick`, `--sim=learners`, `--sim=experience`, `--sim=careers`, `--sim=levels`, `--sim=terms`)
- [x] Translations (fr, ja, zh_CN, zh_TW), fit tests, bot runs in French and Japanese
- [x] Docs (`design.md`, `world.md`, `narrative.md`, `stories.md`, README, CLAUDE.md), version 0.36
- [x] A fresh-eyes playtest, the ear reworked for a real hand at any frame rate, and `--ear-test`
