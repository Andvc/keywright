# Keywright on Steam

What the game does on Steam is in `scripts/Platform.cs`: achievements, rich presence, and pausing when the overlay
opens. It's Steamworks.NET 2024.8.0 (NuGet, MIT) over Valve's Steamworks SDK 1.60, whose library is here:
`steam_api64.dll` for Windows and `libsteam_api.so` for Linux, from the same Steamworks.NET release (a library from
another SDK version won't match the wrapper). CI puts each beside the exported game. Without Steam, or without the
library, the game says so in its log ("Steam: not running, so playing without it.") and plays the same, so a DRM-free
build is the same build without the library.

## Setting it up in Steamworks

1. **App ID.** Put it in `Platform.AppId` (scripts/Platform.cs). A release build started outside Steam then asks Steam
   to start it (`SteamAPI.RestartAppIfNecessary`); debug builds never do. To test on Steam before the store page is
   live, put a `steam_appid.txt` holding the app ID beside the exe (never ship it).
2. **Achievements** (Stats & Achievements): one for each of these, API name exactly as written. Each needs two 256x256
   JPG icons (achieved, and the greyed one). `godot --headless --path . -- --achievements` prints this table from the
   code (`scripts/Core/Achievements.cs`); names and descriptions go into Steamworks in each language too:
   `-- --achievements --lang=fr` (and `ja`, `zh_CN`, `zh_TW`) prints them in that language.

| API name | Name | Description | Hidden |
|---|---|---|---|
| FIRST_CHEST | Opened for You | Buy a chest and open it. |  |
| THORNWICK | Thornwick Market | Move Bram's stall to Thornwick Market. |  |
| CROWNHOLD | Crownhold | Move the shop to Crownhold. |  |
| EMPORIUM | By Royal Appointment | Open the Royal Emporium. |  |
| FIRST_TITHE | The Guild's Due | Pay the Gilded Guild its first tithe. |  |
| DEBT_CLEARED | Paid in Full | Clear Bram's debt to the Guild. |  |
| EARLY_CLEAR | Ahead of Time | Clear the debt with a week to spare. |  |
| CROWN_CHEST | The Crown Chest | Open King Aldric's Crown Chest. | yes |
| HEARTSONG | A Song in the Box | Keep a Heartsong. |  |
| SONGBOOK | Sung Whole | Hear all nine Heartsongs, across your runs. | yes |
| ENCHANTED | It Glows | Find an enchanted thing. |  |
| PERFECT_READ | A Perfect Read | Tick exactly what's in a chest before you open it. |  |
| STREAK | Never Wrong | Make five perfect reads in a row. |  |
| FALSE_BOTTOM | Hollow | Find what's under a false bottom. |  |
| WARD | Named | Name a warded thing, and see it come out. |  |
| COLLECTOR | Collector | Find half the things there are, across your runs. |  |
| CATALOGUE | Every Thing in Aldmark | Find every thing there is, across your runs. |  |
| KIT | The Ear | Clear the debt as Kit. |  |
| HESK | The Scale | Clear the debt as Hesk. |  |
| ROSA | The Tongue | Clear the debt as Rosa. |  |
| ISOLDE | The Collection | Clear the debt as Isolde. |  |
| LEVEL_4 | Hard Bargains | Clear the debt at challenge level 4. |  |
| LEVEL_8 | The Wall | Clear the debt at challenge level 8. |  |

   They're checked whenever the game saves, ends a day or a run, and kept in the profile as well, so one earned
   offline reaches Steam the next time the game runs on it.
3. **Rich presence** (Community > Rich Presence): upload `rich_presence.vdf` (English) and one file per language:
   `rich_presence_french.vdf`, `_japanese`, `_schinese` and `_tchinese`, the same tokens in Steam's language names.
   Friends see "Day 3 at Thornwick Market" or "At the title screen" in their own language.
4. **Steam Cloud** (Auto-Cloud): on Windows, root `WinAppDataRoaming`, subdirectory `Keywright`, patterns `save.json`,
   `profile.json` and their `.bak` (not `settings.cfg`: screens and keys differ between computers). On Linux, root
   `LinuxXdgDataHome`, same subdirectory and patterns. A quota of 10 files and 10 MB is plenty.
5. **Depots**: the Windows and Linux builds are the CI artifacts (`Keywright-<version>-windows-x64`, `-linux-x64`),
   each a folder to upload as it is. Launch option: `Keywright.exe` on Windows, `Keywright.x86_64` on Linux.
6. **Controllers**: the game reads controllers itself (Xbox, PlayStation and Switch layouts; the stick moves a
   pointer). Mark it as having full controller support, and in Steam Input leave the default configuration on
   "Gamepad" so Steam passes the controller through.
7. **Steam Deck**: 1280x800 shows the game at 1280x720 with bars above and below, every text at least 10 pixels high,
   and a controller can do everything the mouse does. Set the default window to fullscreen (it is already).

## The library's licence

Steamworks.NET is MIT (`Steamworks.NET-LICENSE.md`). `steam_api64.dll` and `libsteam_api.so` are Valve's, shipped
under the Steamworks SDK agreement that comes with a Steamworks account.
