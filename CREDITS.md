# Credits

## Art

The pictures were made for Keywright (0.23) to the brief in `art/`: one style (`art/ART_DIRECTION.md`), one palette
(`art/palette/`), and every asset's spec and prompt (`art/manifest.json`). A Codex workflow generated them with
OpenAI's image generation (GPT-Image-2 requested), then fitted each to the game's pixel grid, palette and outline and
checked it with `art/tools/pixelize.py` and `validate.py`; `art/generated/DELIVERY.md` records how each batch was made
and every prompt change. They're in `art/generated/`: the four shops behind the counter and four story scenes, 38
portraits, the 33 things in chests and the nine Heartsongs, the 79 curios, the three lock makers' lock faces, an icon
for each upgrade and each kind of thing, the Gilded Guild's emblem, the Aldric Key and its four shards, the UI frames,
the burst, coin, sparkle and dust, the logo and the app icons. `python3 tools/import_art.py` builds `assets/` from
them.

The chest (0.25) is drawn in code (`tools/custom_art.py`): seven finishes and the three lock makers' hasps and plates.
Round 2 of the brief (0.25) asks for the rest to be redrawn flat and quiet, and for the shop scene in layers; until it's
delivered, the pictures above are round 1's.

Until 0.23 the pictures were placeholders drawn in code (`tools/custom_art.py`, with the frames, icons and shop
architecture in `tools/import_art.py`); the importer still draws those for anything not yet in `art/generated/`.

## Font and sound

The music (a theme for the title and the crossroads, one for each market further in, the night, the Guild's night and
the treasury) and the ambience under it (0.36) are written out in `tools/music/make.py` and rendered by
`tools/music/synth.py` into `assets/audio/music/` and `assets/audio/ambience/`: nobody else's notes or recordings.

The Heartsongs' songs, and the other synthesized sounds (the slosh, the hum, the opening's stings), are made in code at
startup (`scripts/Audio/Sfx.cs`).


By [Kenney](https://kenney.nl), released under [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/):

- **Kenney Mini** from the Kenney Font Package.
- Impact Sounds, RPG Audio, Casino Audio and Interface Sounds: the sound each kind of thing makes when you shake a
  chest (coins, tin, cloth, cards, glass), the chest landing, coins, doors and latches.

The license doesn't require attribution; it's given here because Kenney made the game sound good.

**Galmuri 9** by Lee Minseo ([quiple/galmuri](https://github.com/quiple/galmuri)), under the
[SIL Open Font License 1.1](assets/fonts/galmuri-OFL.txt): the letters Kenney Mini hasn't got (the rest of Latin,
Cyrillic, Greek, Hangul, kana and the Japanese kanji), for translations.

**Fusion Pixel 12px** by TakWolf ([TakWolf/fusion-pixel-font](https://github.com/TakWolf/fusion-pixel-font), release
2026.10.25), under the [SIL Open Font License 1.1](assets/fonts/fusion-pixel-OFL.txt), with glyphs from Ark Pixel,
Cubic 11 and Galmuri (their licences are in the same file): the font for Japanese, Simplified and Traditional Chinese,
cut down to each language's standard character set by `tools/fonts.py`. Kenney Mini has oe, the en dash and the
no-break spaces French needs added by the same script.
`tools/import_art.py` records exactly which source file each sound in `assets/audio/` came from.

Earlier versions (up to 0.15) used pixel art from Pixel-boy's CC0
[Superpowers Asset Packs](https://github.com/sparklinlabs/superpowers-asset-packs); thanks to them for the start.
