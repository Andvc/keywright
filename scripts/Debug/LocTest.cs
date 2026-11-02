using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Debug;

/// <summary>
/// --loc-test (headless): the language layer, end to end through Godot's TranslationServer, with a small German
/// translation made on the spot: a plain text, an interpolated one with its holes moved, plurals, a text with no
/// translation, names kept as keys, the pseudo-translation, and that English needs none of it. Prints each check and
/// exits 1 if any failed.
/// </summary>
public static class LocTest
{
    public static bool Run()
    {
        int failures = 0;
        void Check(bool ok, string what, string got = null)
        {
            GD.Print($"{(ok ? "ok  " : "FAIL")}  {what}{(ok || got == null ? "" : $" (got \"{got}\")")}");
            if (!ok) failures++;
        }

        // English: nothing to look up, and an interpolated text comes out as C# would make it.
        Loc.Lookup = null;
        Loc.PluralLookup = null;
        Loc.Pseudo = false;
        int day = 3, days = 5;
        Check(Loc.T("Pass") == "Pass", "English: a text is itself");
        string english = Loc.T($"Day {day} of {days}");
        Check(english == "Day 3 of 5", "English: an interpolated text is filled in", english);
        Check(Loc.Plural(1, $"{1} thing", $"{1} things") == "1 thing" && Loc.Plural(4, $"{4} thing", $"{4} things") == "4 things",
            "English: plurals by the count");
        Check(Loc.T($"{12.5f:0.0}g and {{price}}") == "12.5g and {price}", "English: formats, and braces for later");

        // German, through Godot.
        var german = new Translation { Locale = "de" };
        german.AddMessage("Pass", "Passen");
        german.AddMessage("Day {0} of {1}", "Tag {0} von {1}");
        german.AddMessage("{0} asks {1}g", "{1}g verlangt {0}");
        german.AddMessage("Healing Draught", "Heiltrank");
        german.AddMessage("Enchanted {0}", "Verzauberter {0}");
        german.AddPluralMessage("{0} thing", new[] { "{0} Ding", "{0} Dinge" });
        TranslationServer.AddTranslation(german);
        TranslationServer.SetLocale("de");
        Loc.Lookup = text => TranslationServer.Translate(text).ToString();
        Loc.PluralLookup = (one, many, n) => TranslationServer.TranslatePlural(one, many, n).ToString();
        try
        {
            Check(Loc.T("Pass") == "Passen", "German: a text", Loc.T("Pass"));
            string filled = Loc.T($"Day {day} of {days}");
            Check(filled == "Tag 3 von 5", "German: holes filled", filled);
            string who = "Garrick";
            string moved = Loc.T($"{who} asks {13}g");
            Check(moved == "13g verlangt Garrick", "German: holes in another order", moved);
            string one = Loc.Plural(1, $"{1} thing", $"{1} things"), many = Loc.Plural(3, $"{3} thing", $"{3} things");
            Check(one == "1 Ding" && many == "3 Dinge", "German: plural forms", $"{one} / {many}");
            Check(Loc.T("Not in the catalogue") == "Not in the catalogue", "German: a text with no translation shows in English");
            // A thing's name is a key: it stays English in the data and is translated where it's shown.
            string entry = Loc.Key("Healing Draught");
            Check(entry == "Healing Draught", "a name kept as a key stays English");
            string shown = Loc.T($"Enchanted {Loc.T(entry)}");
            Check(shown == "Verzauberter Heiltrank", "...and is translated where it's shown", shown);

            Loc.Pseudo = true;
            string pseudo = Loc.T($"Day {day} of {days}");
            Check(pseudo.StartsWith("[") && pseudo.EndsWith("]") && pseudo.Contains('3') && pseudo.Contains('5'),
                "pseudo: bracketed, holes filled", pseudo);
            string kept = Loc.T("Ask {price}, pay {fee}.");
            Check(kept.Contains("{price}") && kept.Contains("{fee}"), "pseudo: {price} and {fee} untouched", kept);
            Check(Loc.Pseudolocalize("Pass").Length > "Pass".Length, "pseudo: longer than the text");
        }
        finally
        {
            Loc.Pseudo = false;
            Loc.Lookup = null;
            Loc.PluralLookup = null;
            TranslationServer.RemoveTranslation(german);
            TranslationServer.SetLocale("en");
        }

        // The game's catalogue, if it's been extracted: every hole numbered from 0 and none skipped.
        if (FileAccess.FileExists("res://locale/keywright.pot"))
        {
            string pot = FileAccess.GetFileAsString("res://locale/keywright.pot");
            var ids = pot.Split('\n').Where(l => l.StartsWith("msgid \"")).Select(l => l[7..^1]).ToList();
            var gaps = ids.Where(id =>
            {
                var holes = System.Text.RegularExpressions.Regex.Matches(id, @"\{(\d+)\}").Select(m => int.Parse(m.Groups[1].Value)).Distinct().ToList();
                return holes.Count > 0 && holes.Max() != holes.Count - 1;
            }).ToList();
            Check(ids.Count > 1000 && gaps.Count == 0, $"the catalogue: {ids.Count} texts, holes numbered in order", gaps.FirstOrDefault());
            CheckTranslations(pot, Check);
        }
        GD.Print(failures == 0 ? "Language test: every check passed." : $"Language test: {failures} FAILED");
        return failures == 0;
    }

    /// <summary>
    /// Each translation in locale/: it loads through Godot, translates every text of the catalogue, its plurals come
    /// out in its own forms, and every letter it uses is in one of the fonts the game gives that language.
    /// </summary>
    static void CheckTranslations(string pot, System.Action<bool, string, string> check)
    {
        var catalogue = Entries(pot);
        foreach (string code in Localization.Available.Where(c => c != "en"))
        {
            string path = $"res://locale/{code}.po";
            if (!FileAccess.FileExists(path)) continue;
            var entries = Entries(FileAccess.GetFileAsString(path));
            TranslationServer.SetLocale(code);
            int missing = catalogue.Count(e => e.Plural == null && TranslationServer.Translate(e.Id).ToString() == e.Id &&
                                                !entries.Any(t => t.Id == e.Id && t.Strings.Count > 0 && t.Strings[0] == e.Id));
            check(missing == 0, $"{code}: every text translated", $"{missing} not");
            var plural = catalogue.Where(e => e.Plural != null).ToList();
            // A form the same as the English counts when the file says so ("{0} long" is French too).
            bool Own(Entry e, string form, string english) => form.Length > 0 &&
                (form != english || entries.Any(t => t.Id == e.Id && t.Strings.Contains(form)));
            var notOwn = plural.Where(e => !Own(e, TranslationServer.TranslatePlural(e.Id, e.Plural, 1).ToString(), e.Id) ||
                                           !Own(e, TranslationServer.TranslatePlural(e.Id, e.Plural, 5).ToString(), e.Plural)).ToList();
            check(notOwn.Count == 0, $"{code}: {plural.Count} plurals in its own forms", notOwn.FirstOrDefault()?.Id);
            var fonts = Localization.FontsFor(code);
            var unknown = entries.SelectMany(e => e.Strings).SelectMany(s => s).Where(c => !char.IsWhiteSpace(c) && !char.IsControl(c))
                .Distinct().Where(c => !fonts.Any(f => f.HasChar(c))).ToList();
            check(unknown.Count == 0, $"{code}: its fonts have every letter it uses", new string(unknown.Take(20).ToArray()));
        }
        TranslationServer.SetLocale("en");
    }

    sealed record Entry(string Id, string Plural, List<string> Strings);

    /// <summary>A gettext file's entries (msgid, msgid_plural, each msgstr), continuation lines and escapes read.</summary>
    static List<Entry> Entries(string text)
    {
        var entries = new List<Entry>();
        string id = null, plural = null, field = null;
        var strings = new List<string>();
        void Finish()
        {
            if (!string.IsNullOrEmpty(id)) entries.Add(new Entry(id, plural, strings.Where(s => s.Length > 0).ToList()));
            id = plural = field = null;
            strings = new List<string>();
        }
        foreach (string raw in text.Split('\n'))
        {
            string line = raw.Trim();
            if (line.Length == 0) { Finish(); continue; }
            if (line.StartsWith('#')) continue;
            int quote = line.IndexOf('"');
            if (quote < 0 || !line.EndsWith('"')) continue;
            string value = line[(quote + 1)..^1].Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
            if (quote == 0)
            {
                if (field == "msgid") id += value;
                else if (field == "msgid_plural") plural += value;
                else if (field == "msgstr" && strings.Count > 0) strings[^1] += value;
                continue;
            }
            string key = line[..quote].Trim();
            if (key == "msgid") { if (id != null) Finish(); id = value; field = "msgid"; }
            else if (key == "msgid_plural") { plural = value; field = "msgid_plural"; }
            else if (key.StartsWith("msgstr")) { strings.Add(value); field = "msgstr"; }
        }
        Finish();
        return entries;
    }
}
