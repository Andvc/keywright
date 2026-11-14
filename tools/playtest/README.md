# Playtest kit

A way to have someone who has never seen the code play the game by screenshots: a person over a remote shell, or an
agent with no context (0.34's fresh-eyes playtest was one). Give the playtester this README and nothing else from the
repository, so what they report is what a new player meets.

The game runs in a virtual 1280x720 screen (Xvfb display :77), from this checkout (or from `GAME=/path/to/worktree`, to
keep a build steady while you change the code). Its files (settings, saves, profile) go to `PLAYTEST_DIR` (default
`$TMPDIR/keywright-playtest`), so it starts as a brand-new install; delete that folder to start fresh again. Needs Xvfb,
xdotool, ImageMagick's `import` and bc.

Everything goes through `pt` in this folder. Every command that does something prints the path of a screenshot taken
just after it: open that PNG to see the screen. Coordinates are screen pixels on the 1280x720 picture, (0,0) top left.

| Command | What it does |
|---|---|
| `pt start [options]` | Starts the game (and the virtual screen). Takes a screenshot after 6 seconds. |
| `pt stop` | Quits the game. |
| `pt shot` | Just a screenshot. |
| `pt click X Y [wait]` | Left-click at X,Y, wait (default 0.8 s), screenshot. |
| `pt move X Y` | Hover at X,Y (tooltips appear after a moment), screenshot. |
| `pt key NAME [wait]` | Press a key (`space`, `Escape`, `Return`, `n`, `f`, ...), screenshot. |
| `pt holdkey NAME SECONDS` | Hold a key down that long. |
| `pt drag X1 Y1 X2 Y2` | A slow press-move-release (for sliders). |
| `pt shake X Y gentle\|firm\|hard [SECONDS]` | Grabs whatever is at X,Y and shakes it at that strength (default 2.5 s), then lets go. Two screenshots are taken during the shake (what's heard shows as words over the chest, and they fade fast) and one after. |
| `pt drop X Y` | Grabs what's at X,Y, lifts it about 175 px and lets go. |
| `pt wait SECONDS` | Waits, then a screenshot (for animations). |
| `pt log [N]` | The last lines of the game's own log (errors show up here). |

## Controls (as the game sets them)

- Mouse for almost everything. Holding the left button on an object and moving the mouse drags it.
- Space (or Enter): the game's "act" key (it says when it wants it).
- F: force, where the game offers it.
- N: the notebook.
- Esc: the pause menu / back.

Some moments are about timing (pressing a key at the right instant while something moves). A screenshot-and-act loop
can't time things finely: if you can't manage one, take whatever other way the game offers and say so in your notes;
don't count it against the game unless the game gives no other way.

## Things worth knowing about the setup (not the game)

- There's no sound you can hear. If the game seems to rely on sound, say what you see it doing instead (on-screen
  words, effects) and whether that was enough to play by.
- Each action takes a few seconds of real time, so the game may move on (timers, animations) between your screenshots.
  Take an extra `pt shot` or `pt wait 1` when you're unsure what state it's in.

## Skipping ahead

A season takes an hour and a half (four weeks of five days, a market each), so these launch options start later in
the game, the shop fast-forwarded by the sim's careful player (things may look a little odd right at the jump):
`pt start --start-week=2 --no-tutorial` (the first morning at the second market), `pt start --start-week=4
--no-tutorial` (a first season's last week, to its last night), `pt start --season=2` (the first days of a remembered
season), `pt start --season=2 --start-week=3 --no-tutorial`, and `pt start --treasury-demo=final` (the final opening).
