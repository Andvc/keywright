# Curio audit D

0.37's six curios that do a way of trading's routine at the counter (`scripts/Core/Hands.cs`; `docs/design.md`,
"Listening is free, and a build does the routine"), and the two rules 0.37 put under every curio: the trade's ways
across seasons, and a read with no light. Unlike A, B and C this was written with the rules, not against a finished
game: each card's promise below has its check in `tools/curiotest/ChecksD.cs`, and all of them pass.

Run: `dotnet run --project tools/curiotest -- --only=quill,engine,moth,gauge,primer,abacus,monocle,compass`

(0.36's two curios for its counted listens, the Echo Stone and the Lamb's-Wool Plugs, went out with the listens:
their checks are gone from `ChecksA.cs`, and a save that held one is paid what it cost, `SaveData` version 9.)

## The six

| Curio | Card | Where it acts | What it never does |
|---|---|---|---|
| quill (U, perfect read) | "Your slip ticks each thing as you hear it, and counts them. What you work out is still yours to mark." | `GameState.Tick`, from `CounterPhase.Quill` the first time each thing sounds (and the sim's `Slip(..., counted)`): one more of its kind, up to the most the slip counts; starred if it glowed | tick what never sounds (a warded thing, what's under a false bottom); tell sand from a spell: it writes what was heard |
| engine (L, perfect read) | "Every chest is read for you: what's truly in it is on your slip as it comes in, false bottom and warded thing and all." | `GameState.ReadsChests` (with the Guild's Bookkeeping Engine), `Premark` copying `Appraisal.Recorded`; `Strategy.Recorded` for the careful player | read a chest chained shut, a Guild crate or a mimic's; open before its feat (a hundred perfect reads, `Curios.Feats`) |
| moth (R, enchantment) | "Every enchanted thing in a chest is starred on your slip as the chest comes in, witchlight or not." | `MarksMagic`, in `Premark`: each kind with an enchanted thing gets a star (and a count of one if it had none); `GameState.Lit`, so the slip expects the star | star a fake, a broken thing or a Heartsong; say how many of the kind there are |
| gauge (R, false bottoms) | "A false bottom is marked on your slip as the chest comes in: no drop needed." | `MarksHidden`, in `Premark` (only where the slip has the row, `Appraisal.CountsHidden`); `Strategy.ShouldDrop` is false with it | say what's under the floor |
| primer (R, wards) | "When a sign points at a warded thing, your slip names it for you." | `NamesWards`, in `Premark`: one more of the warded thing's kind than the chest holds (`Chest.Names`); `PrimerNames` keeps a quill's count from undoing it | name a ward no sign points at (there's none: every ward has its sign), or count the rest of the kind for you beyond what the chest holds |
| abacus (R, haggling) | "You know the least a haggler will take: your offer starts on it, and they take it." | `KnowsFloors`: `CounterPhase` starts the slider on `Visit.Floor` and says so; `Strategy.FirstOffer(visit, worth, state)` offers it | apply to a fixed price, or make a chest worth less than the floor worth buying |

Each tells you something of what a chest holds, which no curio did before 0.37. None says what a chest is worth: no
quality, no price, and the odds bar still only adds up the slip.

## The rules under them

| Rule | Where | Checked as |
|---|---|---|
| A read with no light: an enchanted thing counted as its kind, with no way to see it glow, is read right | `ISlipModel.Lit`, `Appraisal.Check` | `monocle`: the same slip is a perfect read in the dark and a miss with the monocle lit |
| The trade's ways go by the grandest market any season on the save reached | `GameState.Ways`, `Market.Roll`, `Appraisal.MaxCount`/`CountsHidden` | `compass`: a first season's stall has the stall's ways; after a season that reached Crownhold the stall's slip has a false-bottom row and counts to three, the cradle and the drawer are there |
| A first season's cradle comes on its fourth day at Thornwick | `GameState.Cradle`, `CradleDay` | `compass`: not on days 6 and 8, there on day 9, and at Crownhold whatever the day |
| A chest never holds what its slip can't mark | `Market.Roll`, `TaleBook.Fill`, `Events.StrangerVisit`, all by the ways | `compass`: 4,000 of a first season's stall chests hold no pair, lock, false bottom or enchantment; a later season's do |

## Measured (`--sim=builds --n=100`, as Nim brings each, against the careful player with no curios)

@@AUDIT_BUILDS@@
