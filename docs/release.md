# From early access to a Steam release

Where Keywright stands against what a released PC game on Steam is expected to have, what 0.27 built for it, and what
only the owner can do. Written for 0.27 (November 2026).

## Where it stands

| Area | State | Notes |
|---|---|---|
| Saves and the profile | Ready | Crash-safe writes, backups, versions and migration, damaged files set aside and named. |
| Display settings | Ready | Windowed, fullscreen, exclusive; window sizes that fit the screen; pixel-perfect or smooth; vsync; frame cap. |
| Sound settings | Ready | Master, effects, the Heartsongs' songs; muting when the window loses focus. |
| Keyboard and mouse | Ready | Every key rebindable; the mouse needs none. |
| Controllers | Ready | Xbox, PlayStation and Switch layouts; the stick drives the pointer, so a controller does all a mouse does. |
| Languages | Foundation ready | Every text translatable (2,648 texts in 0.28, about 131,000 characters); no translation yet. |
| Fonts | Ready but Chinese | Western European in the house font, the rest of Latin, Cyrillic, Greek, Korean and Japanese in Galmuri. |
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

**Translations:** about 23,000 words per language (0.28's tales added a third). Professional translation runs roughly $2,000-3,500 per
language, then a pass of play in that language. Run the game with `--pseudo` first and fix the screens that
overflow. For Chinese, add a pixel font with the full Simplified set: Fusion Pixel 12px (OFL), from its GitHub
releases, as a second fallback in `Localization.SetUpFont`.

**Steam Deck:** request a compatibility review once a release candidate is on Steam. The game already meets the
usual points: full controller support, fullscreen by default, text at least 10px at 1280x800, no launcher.

## Known gaps

- **Layouts for longer languages.** Under `--pseudo`, a few labels overflow: the top bar's title runs over "early
  access", the character cards' descriptions, the collection's heading. Fix these when a translation shows which
  ones matter.
- **Chinese** needs its font (above). Japanese uses Galmuri's 6,400 kanji, which covers everyday text.
- **Saving** happens at the start of each day only: quitting mid-day replays that day (by design so far; see
  docs/design.md).
- **No crash reporter.** Godot writes logs to the user folder's `logs/` (Settings > Game > Open the folder), which is
  where a player's bug report should start.
- **macOS** isn't built: it needs signing and notarization on a Mac account, and Steam doesn't require it.
- **Untested here:** the Steam overlay and Steam Input with a real client (only possible on a machine with Steam),
  and a real controller (the tests send the same events one would).
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
