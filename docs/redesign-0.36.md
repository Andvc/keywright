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
too). Four dues, the last the wall. A season is about 80 minutes, everyone who keeps paying sees every market, and a
season that's going to fail fails at a due, early, not after two hours at Thornwick.

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

A speech plays its sentences one after another by itself (one click a speech, a click to finish it at once), and
the opening scene is cut to what a first chest needs.

### 5. Music and ambience

A soundtrack made in code like the Heartsongs' (a theme for the title, each market, the night, the Guild and the
treasury) and ambience beds for each place, on their own bus with a Music volume.

## Status

- [ ] 1. The four-week season (Core, UI, tales, campaign, saves, sims)
- [ ] 2. The ear (Core, counter, HUD, strategy, lessons, sims)
- [ ] 3. Quality odds
- [ ] 4. Dialogue pacing and the opening scene
- [ ] 5. Music and ambience
- [ ] Rebalance (`--sim`, `--sim=experience`, `--sim=careers`, `--sim=levels`)
- [ ] Translations (fr, ja, zh_CN, zh_TW), fit tests, bot runs in each
- [ ] Docs (`design.md`, `world.md`, `narrative.md`, `stories.md`, README, CLAUDE.md), version 0.36
