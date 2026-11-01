# Translating Keywright

`keywright.pot` is every text the player reads: about 2,100 of them, 97,000 characters (a little under the length of
a novel), from button labels to Mister Vane's speeches. It's made from the code by `tools/loc` and should never be
edited by hand.

## Making a translation

1. Open `keywright.pot` in a gettext editor ([Poedit](https://poedit.net) is free) and start a translation for your
   language. Save it here as `<locale>.po`: `de.po`, `fr.po`, `es.po`, `pt_BR.po`, `ru.po`, `pl.po`, `ja.po`, `ko.po`,
   `zh_CN.po`.
2. Start the game: the language is offered in Settings > Game > Language (and chosen by itself when it's the
   computer's). A change takes effect the next time the game starts.
3. When the game's text changes, run `dotnet run --project tools/loc -- extract` and, in Poedit, *Catalog > Update
   from POT file*: new texts come up untranslated and changed ones fuzzy. A text with no translation shows in English.

## What to keep

- `{0}`, `{1}` and so on are filled in by the game: numbers, names, other texts. Keep each one, in whatever order your
  grammar wants (`"{0} asks {1}g"` can become `"{1}g verlangt {0}"`). The comment `#: scripts/...` says where a text
  is used, which usually says what goes in each.
- `{price}`, `{fee}`, `{heft}`, `{colour}`, `{clue}`, `{act}`, `{notebook}` and the like are filled in too, later:
  keep them exactly as they are.
- `g` after a number is gold.
- Some texts are pieces of others (", {0} weeks early", " and "): translate them so they read well when joined,
  and say so in the file's comments if they can't.
- Plurals (`msgid_plural`) take as many forms as your language has: set `Plural-Forms` in the header (Poedit does).
- Names of people and places (Bram, Thornwick, the Gilded Guild) can stay as they are or be transliterated; things'
  names (Healing Draught) are for translating.

## Room on screen

The game is drawn on a 1280x720 canvas with a pixel font, and some labels have little room. Run the game with
`--pseudo` to see every text a third longer and in brackets: anything that overflows there will in German too.
Short forms are better than cut-off ones; where a label can't be shortened, tell us.

## Letters

Kenney Mini, the game's font, has English and western European letters (Latin-1: French, German, Spanish,
Portuguese, Italian, the Nordic languages). Galmuri 9 fills in the rest of Latin (Polish, Czech, Turkish), Cyrillic,
Greek, Korean and Japanese. Simplified and Traditional Chinese need a font of their own added first (see
docs/release.md); without it, missing characters come from the computer's own fonts.
