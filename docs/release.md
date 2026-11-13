# From early access to a Steam release

Where Keywright stands against what a released PC game on Steam is expected to have, what 0.27 built for it, and what
only the owner can do. Written for 0.27 and brought up to date for 0.29 and 0.36 (November 2026).

## Where it stands

| Area | State | Notes |
|---|---|---|
| Saves and the profile | Ready | Crash-safe writes, backups, versions and migration, damaged files set aside and named. |
| Display settings | Ready | Windowed, fullscreen, exclusive; window sizes that fit the screen; pixel-perfect or smooth; vsync; frame cap. |
| Sound settings | Ready | Master, music (0.36), effects, the Heartsongs' songs; muting when the window loses focus. |
| Music | Written in code, unheard | Seven themes and four beds of ambience, composed and rendered by `tools/music` (0.36). Checked by their numbers only: listen before release (below). |
| Keyboard and mouse | Ready | Every key rebindable; the mouse needs none. |
| Controllers | Ready | Xbox, PlayStation and Switch layouts; the stick drives the pointer, so a controller does all a mouse does. |
| Languages | Translated | English, French, Japanese, Simplified and Traditional Chinese: all 4,056 texts (0.36; 2,743 in 0.29), checked and played in each. A native speaker's pass is still worth buying (below). |
| Fonts | Ready | Western European in the house font; Japanese and Chinese in Fusion Pixel 12px; the rest of Latin, Cyrillic, Greek and Korean in Galmuri. |
| Steam | Ready to switch on | Achievements, rich presence, overlay pause, launch through Steam: needs the app ID and Steamworks set-up. |
| Builds | Ready | CI exports Windows and Linux, smoke-tests both and keeps them as artifacts, on every push. |
| Credits and licences | Ready | In game (title > Credits) and in CREDITS.md; the studio's name to add. |
| Store page, ratings, legal | Owner's | See below. |

## What 0.27 added

**Saves.** Every file the game keeps goes through `Storage`: written to a temp file, flushed, then swapped in with the
old one kept as `.bak`, so a crash or power cut mid-write loses nothing. A file that can't be read falls back to its
backup, and one that can't be read at all is set aside (never deleted) and named on the title screen. Saves and the
profile carry a version: an older one is migrated (a real bug turned up here, a rename applied on every load), a newer
one is left alone. The user folder is `%APPDATA%/Keywright` on Windows (`~/.local/share/Keywright` on Linux), one
place for Steam Cloud, and files from the old LootFall folders come across. `--save-test` checks all of it.

**Settings.** Four pages (Display, Sound, Controls, Game), each with Defaults. The window is changed through Godot's
root window node: changing the size through the display server alone left the game drawn at the old size, with
clicks landing in the wrong place, which would have hit every windowed player. The game pauses when it loses focus
(a setting) and can mute too. The Game page opens the folder with the saves and the logs.

**Input.** The game asks for actions, never keys: set a pin, force a lock, the notebook, the menu, back. Each has keys
and controller buttons, rebound by pressing them; a key another action had moves over, or the two swap. Prompts name
what to press on the device in use ("[N]", "[Y]", "Square" on a PlayStation pad). A controller drives the mouse pointer
itself (`GamepadCursor`), faster while A is held so a flick of the stick shakes a chest hard, so every screen works
with a controller without being rebuilt. `--pad-test` plays the counter with a simulated controller, in a 1280x720, a
1.25x and a letterboxed window.

**Languages.** Every text the player reads goes through `Loc.T`. An interpolated one is looked up as a sentence with
numbered holes, so a translation can reorder them; counts use plural forms. A Roslyn tool (`tools/loc`) wrapped
2,100 strings, telling text from keys by how each is used, and now checks that nothing new slips through (CI runs
it) and writes the template, `locale/keywright.pot`. Drop a translated `<locale>.po` in `locale/` and Settings offers
it. `--pseudo` shows every text bracketed and a third longer; the sim gives identical numbers that way, proof that
no translated text is used as a key. `--loc-test` runs a made-up German translation through Godot.
`locale/README.md` is for translators.

## What 0.29 added

**Translations.** French, Japanese, Simplified Chinese and Traditional Chinese, every text, made by Claude agents
from a glossary written first (`locale/GLOSSARY.md`: the world's names, each person's voice, each language's
typography), names and labels before sentences so every sentence uses the names the buttons show. Traditional Chinese
is the Simplified made with OpenCC's Taiwan phrases, then checked by hand. `tools/loc -- verify` checks every file
against the template (holes, `{price}`-style names, plural forms, a letter the fonts haven't got); `--loc-test` loads
each through Godot; and each language was played on every screen, with `[overflow]` lines (`Debug.Overflow`) naming
any text that outgrew its box. Making the game translatable showed places where English words were glued together
in code; those are sentences now (a list's commas, a sum's "g", Skrae's echoes counted one kind at a time).

**Fonts.** Japanese and Chinese are drawn in Fusion Pixel 12px (OFL), a pixel font on a 12px grid, cut to each
language's standard set plus every character its translation uses (`tools/fonts.py fusion`): 1.4 MB for Japanese and
Simplified, 2.6 MB for Traditional. Line breaking in Chinese and Japanese needs ICU's data, which the export now
includes. Kenney Mini gained French's oe, the en dash and its no-break spaces.

**Steam.** `Platform` connects when Steam is there and does nothing when it isn't, so the same build is DRM-free
without the library. On Steam: 23 achievements (kept in the profile too, so none are lost off Steam, and listed in the
collection for everyone), rich presence
("Day 3 at Thornwick Market"), the overlay pausing the game, and relaunching through Steam once there's an app ID.
`steam/README.md` is the Steamworks side, step by step. The exported Linux build was tested here with the real Steam
library and no client: it starts, says so in its log, and plays.

**Builds.** `.github/workflows/build.yml` checks the build (no warnings) and the text, then exports Windows and Linux
with the Steam library in place, plays two days with the bot on each, and uploads `Keywright-<version>-windows-x64`
and `-linux-x64`. The version is `application/config/version` in project.godot, shown on the title screen and
stamped into the exe.

**Smaller things.** Credits in game, with the licences of Godot, its libraries, Galmuri and Steamworks.NET. The
speech bubble no longer clips a four-line story.

## What only the owner can do

**Steamworks** (steam/README.md has the details):
- Sign up as a Steamworks partner, pay the app fee, and put the app ID in `Platform.AppId`.
- Achievements: enter the 23 (`--achievements` prints them) with two 256x256 icons each, 46 in all; the game's
  pixel art can supply them.
- Upload `steam/rich_presence.vdf`; set Steam Cloud (Auto-Cloud paths in the README); mark full controller support.
- Depots: upload the CI artifacts with SteamPipe. A CI step can do this later with a Steam builder account kept in
  repository secrets.

**The store page** has to be up as "Coming Soon" for at least two weeks before release. It needs:
- capsules: header 920x430, small 462x174, main 1232x706, vertical 748x896, library capsule 600x900, library
  hero 3840x1240, library logo, page background 1438x810;
- five or more screenshots at 1920x1080, a trailer, the description and tags;
- the content survey, a price and a release date.

**Legal and money:** the Steam Distribution Agreement, tax and bank details, and a short privacy note (the game
collects nothing and sends nothing but what Steam does).

**Credits:** set `CreditsView.Studio` to the name the game is published under.

**The music (0.36):** every track was written and rendered in code in a cloud session with no way to hear it. Play
a season with the sound on. If a theme grates or thins out over an hour, its tune is a function in
`tools/music/make.py` (and `python3 tools/music/make.py --wav DIR` writes WAVs to judge without the game); if the
whole approach doesn't hold up, a composer's loops dropped into `assets/audio/music` and `assets/audio/ambience`
under the same names play with no code changed. The Music volume starts at 60%.

**The ear, in the hand (0.36):** a listen is spent once the chest has really been shaken at a strength for a moment
(back and forth, at that speed, with nothing harder just before), so winding up to a hard shake, lifting the chest
or carrying it shouldn't spend one. `--ear-test` checks that with a pointer driven the ways hands move, at sixty and
at twelve frames a second, and a fresh-eyes playtest found (and got fixed) the first version's failure to register
at all on a slow machine. A pointer on a path still isn't a person: ten minutes with a real mouse and a real
controller will say whether listens ever go by accident, or a shake you meant doesn't count.
`CounterPhase.ListenHold` (how long each strength takes) and `ShakeBox.TurnHold` are the numbers.

**Translations:** the four languages are complete and consistent, but no native speaker has read them. Before
release, pay one for a pass of play in each (a review, not a translation: a few days each, well under the $2,000-3,500
a translation from scratch costs), and list the languages on the store page. A new language is a `.po` file from
`locale/keywright.pot` and the glossary; Korean and Russian would need only the text (Galmuri has their letters).

**Steam Deck:** request a compatibility review once a release candidate is on Steam. The game already meets the
usual points: full controller support, fullscreen by default, text at least 10px at 1280x800, no launcher.

## Known gaps

- **Translations by machine.** Every text has been checked by tools and seen on screen, but a native reader will
  still find lines that read stiffly; see above.
- **Saving** happens at the start of each day only: quitting mid-day replays that day (by design so far; see
  docs/design.md).
- **No crash reporter.** Godot writes logs to the user folder's `logs/` (Settings > Game > Open the folder), which is
  where a player's bug report should start.
- **macOS** isn't built: it needs signing and notarization on a Mac account, and Steam doesn't require it.
- **Untested here:** the Steam overlay and Steam Input with a real client (only possible on a machine with Steam),
  a real controller (the tests send the same events one would), and, since 0.36, how the music sounds and how the
  ear's listens feel under a real hand (above).
- **A demo:** Steam Next Fest wants a separate demo app. The game would need a demo mode (the first week, say, then a
  wishlist note). That's quick to add once the store page exists.

## Cutting a release

1. Bump `config/version` in project.godot and update the version in CREDITS.md if it's named there.
2. Run the checks: `dotnet build` (0 warnings), `--sim` (compare with the last release), the lesson test,
   `--save-test`, `--loc-test`, `--shake-test` (eleven minutes), `--pad-test`, `dotnet run --project tools/loc --
   check`, and a bot run with `--shots` to look over.
3. Push to `main`, or merge into it: CI builds and smoke-tests both platforms.
4. Download the artifacts, upload them to their depots with SteamPipe, and set the build live on a beta branch first.
5. On a machine with Steam: start it from the library, earn an achievement, open the overlay, quit, start it on
   another machine (Steam Cloud), and play a day with a controller.
