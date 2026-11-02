# Translating Keywright

`keywright.pot` is every text the player reads: about 2,740 of them, 132,000 characters (the length of a novel),
from button labels to Mister Vane's speeches. It's made from the code by `tools/loc` and should never be edited by
hand. The game is translated into French (`fr.po`), Japanese (`ja.po`), Simplified Chinese (`zh_CN.po`) and
Traditional Chinese (`zh_TW.po`, made from the Simplified with OpenCC's Taiwan phrases, then checked).
`GLOSSARY.md` has the words of the world in each of them and each language's style: start there for a new one.

## Making a translation

1. Open `keywright.pot` in a gettext editor ([Poedit](https://poedit.net) is free) and start a translation for your
   language. Save it here as `<locale>.po`: `de.po`, `fr.po`, `es.po`, `pt_BR.po`, `ru.po`, `pl.po`, `ja.po`, `ko.po`,
   `zh_CN.po`.
2. Start the game: the language is offered in Settings > Game > Language (and chosen by itself when it's the
   computer's). A change takes effect the next time the game starts. `--lang=fr` plays one run in a language
   without changing the setting.
3. `dotnet run --project tools/loc -- verify` checks every translation against the template: what's untranslated,
   and what would show wrong (a hole or a `{price}` lost, the wrong number of plural forms, a letter the language's
   fonts haven't got). `godot --headless --path . -- --loc-test` loads each through Godot.
4. When the game's text changes, run `dotnet run --project tools/loc -- extract` and, in Poedit, *Catalog > Update
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
Short forms are better than cut-off ones; where a label can't be shortened, tell us. A run with `--shots=DIR` (the
shell demo, `--shell-demo`, or the bot, `--autoplay`) prints an `[overflow]` line for every label or button whose
text outgrew its box, in whatever language `--lang` sets.

## Words slotted in

Some texts are filled with words the game translates on their own: a chest's colour (`{colour}`, `{0}` in "A {0}
chest"), a strength of shake, a quality, a count in words, a kind of thing. The glossary lists the forms each language
chose so the sentences around them agree (French uses masculine forms: "un coffre en bois", "au niveau fort"). Where
a language needs a different word for each case, the game asks for each case: Skrae's echoes are counted one kind
at a time ("{0} round", with its plural), and the dowry's warded thing has a line for each kind.

## Letters

Kenney Mini, the game's font, has English and western European letters (Latin-1, with oe, the en dash and French's
no-break spaces added by `tools/fonts.py`). Galmuri 9 fills in the rest of Latin (Polish, Czech, Turkish), Cyrillic,
Greek and Korean. Japanese and Chinese are written in Fusion Pixel 12px, cut down to each language's standard
character set and every character its translation uses (`python3 tools/fonts.py fusion DIR`): run it again when a
translation brings a rare character, and `verify` names any that's missing. Fusion Pixel is drawn on a 12px grid, so
in those languages text is shown at 12, 24, 36 or 48px (`UiKit.SnapFont`) where English uses 16, 24, 32 or 48.
