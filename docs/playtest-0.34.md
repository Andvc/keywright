# 0.34's fresh-eyes playtest

An agent with no context and no access to the code played the game by screenshots (`tools/playtest/`: the README and
nothing else), sound off. Part 1 was a first season from a fresh profile, day 1 to day 11. Part 2 covered four
checkpoints: a first season's last weeks (`--start-week=6`), a remembered season (`--season=2`), a remembered
season's third week, and the final opening (`--treasury-demo=final`). The first part ran on a frozen build. Fixes went
in while it played, and its last two checkpoints ran on a newer build. Below is each finding, what was decided and
why. "Round" is the commit that fixed it (rounds 1 to 6 on `prototype/0.34`).

## What it liked

- The core loop. Sound words at three strengths, a clank drowning a jingle, the slip and its odds bar, the gut, a
  perfect read. Its best moment was naming Hesper's warded coin.
- The hook: the pale man watching, Vane's debt, Bram taken for "temporary work", the letter's four key pieces.
- The remembered season is "far more gripping" than the first: Vane's "you always look at my ring", the weighhouse
  crates chalked with names and weights, a crate that knocks back. It called the Inquiry's leads the best writing in
  the game.
- The final opening and the next day: "gold flaking off them like old paint", "Day thirty-six. I've never seen the
  calendar say that."

## Fixed

| Finding | Fix | Round |
|---|---|---|
| Hesper's lesson asked for a hard shake on a mourning day: Buy and Pass stayed off (a soft-lock but for Skip) | No lesson step asks for a strength the day forbids (`Lessons.Tried`) | 1 |
| The day's profit showed red on a day commissions paid well | The day's profit counts everything: chests, commissions, curios, door money, tales | 1 |
| Dangling promises (Bettany's "news, if you're buying", "ask after her cellar", "we'll talk in the morning") | Lines that promised what never came removed or paid off | 1 |
| Bettany repaid a day late, and a coin short | She repays on the night she promised, exactly half again | 1 |
| A curio's "?" tooltip ran off the screen in one line | Tooltips wrapped (`Words.Wrap`) | 1 |
| The reader's key piece and its fee came silently | Said at the opening and in a note over the top bar | 2 |
| A quick drop did nothing, with no feedback | A cue when the chest is high enough to drop | 2 |
| The Lantern Market's word came while Nim's Lantern (a curio) was for sale | The market opens in a first season only when there's something to do there; the tip reworded | 2 |
| The "two days a week are different" tip came on day 9 | The day's event tip comes first | 2 |
| The Goblin Bounty asked where Nettle's clan burrows before you'd met her | Only once you know Nettle | 3 |
| The Counting Beads seemed to do nothing on some chests | Their text says exactly which chests they tally | 3 |
| No feedback that a chest wasn't stolen | "Your mark stays warm" while a stolen chest is wanted | 4 |
| Notes over the top bar covered scene titles and the notebook's tabs | Notes wait during scenes and while the notebook is open | 4, 5 |
| A `--start-week` jump played a day past the treasury's night, showed two "4 of 4" notes, and its next season didn't remember | The jump goes straight to the treasury; each piece's note keeps its own count; the jump's notes are dropped; its season is banked in memory | 5, 6 |
| A seller's stated clue (a tally) scrolled away under the chest after a drop and a mark | The clue stays ahead of the latest notes while it fits | 6 |
| The result's explanation was cut off after three lines | Four lines fit; each thing named once | 6 |
| Sound words piled into an unreadable cloud | Each keeps its own spot (players count by them); little drift | 6 |
| "Curios 3/3" over four icons | Keepsakes named apart at the day's end and the counter | 6 |
| Flick's countdown unexplained | A term under the chest ("In a hurry: 20 seconds to decide") and a tooltip | 6 |
| The lockpick's needle turns back after each pin, unexplained; its tip said "the ward's let go" (a word the game uses for warded things) | Tip reworded: no ward, and the turn explained | 6 |
| Pip's acceptance seemed arbitrary | His page says two tries and six coins is plenty (what the sim's careful player offers); a refusal line no longer implies a rule | 6 |
| A ward's caption named the thing as it was before a curio's luck bettered it | The freed thing is looked up as it is now (`Chest.Unwarded` by id) | 6 |
| "Send for the Wardens" stayed after the seller left | Hidden once the visit's over | 6 |
| The gut gave a verdict on a stolen chest you passed | "A stolen chest: it would never have opened for you." | 6 |
| A kept mimic stamped as a profit, explained as "Empty!" | "A MIMIC IN THE BACK ROOM", a mimic's own explanation, no gut | 6 |
| A Guild crate was "a green chest", "theirs to sell", a perfect read, and nobody named | Its own note, no mark line, no read at all (`ReadReport.NoRead`), and the explanation names who's inside | 6 |
| N did nothing at the day's end | N opens the notebook at the day's end and at Nim's table, as at the counter | 6 |
| Jailing Corvin left a lead that relied on him | The lead names the bargemen alone once he's gone | 6 |
| Bram "brought out of that chest" while he'd stood beside you at the treasury; his warm hand said twice | The season's end says how he came home; the hand is the next morning's | 6 |
| The Maw's whisper ("He knows whom he came to save...") hard to read | Drawn in the Maw's gold | 6 |
| A storm's "chests come a colour better", and the lesson chest that day was wood | A lesson's chest follows the day's rules too (`Lesson.MakeOn`, `Chest.Brighten`) | 6 |
| "Ada priced nothing, so why 3 gold?" (and Bettany "0 rattles", 4 gold) | "With nothing heard, Ada asked a few coins for the chest itself."; the counting line lost a "she" that was wrong for Master Quire | 6 |
| The treasury's breath was easy to miss | "It's breathing." holds a second longer | 6 |
| Delivery buttons against the bottom edge | The button rows tightened | 6 |

## Left as they are, and why

- **Faces.** Archmage Orme is "she" but shows the old bearded archmage, Harl Caskett has Garrick's face, and Wynn has
  Ada's. These figures are borrowed until drawn (`Look`). The 3px art comes from originals kept outside the
  repository, so a new figure has to come from the art pipeline. Orme needs one most, because the borrowed figure
  contradicts the text.
- **Repetition.** Sellers repeat the same story verbatim, and a wood chest's things carry the same few names. More
  variants per seller and place would be welcome; it's writing, plus four translations each. One name per kind and
  quality is the item table's design.
- **Tales are crowded.** Eight tales began in ten days, many of them asking for money. The tale draw and staging are
  balance work: the experience sim measures load, and the first season's tales feed its key and its clear rate. This
  is for the owner to weigh.
- **The first swallowing is abrupt** (no summary, straight to the first morning). That's the proposal's shock. The
  remembered first morning names it ("I remember the treasury. I remember it breathing.").
- **A remembered season replays its first days.** By design: the same spring comes round, recognised (Garrick's green
  chest, Vane's ring). The tester's notebook forgot what it had met only because a debug launch starts a fresh profile.
  A real profile carries what's met and taught.
- **The slip counts one of a kind until Crownhold.** Stall and Thornwick chests never hold two of a kind, and
  Crownhold's tip teaches counting ("click a row again for each one"). The tester jumped straight to the final
  opening and skipped that tip.
- **Gilt's odds bar trusts what you tick.** Spotting his fakes is the deduction (a lodestone stills demon gold).
- **The odds bar leaves out commissions.** It weighs what a chest sells for; the commission board is beside it.
- **Jargon before its definition** (sight unseen, lodestone, witchlight). Nim's cards define their terms ("?"), and Bram
  explains sight unseen the first time a card uses it.
- **The character screen's unlock hints name the treasury.** The tester called it "intriguing, mild spoiler". Left.
- **Bram's lesson box overlaps the notebook's foot.** Intended: a lesson stays readable while you look something up
  in the notebook.
