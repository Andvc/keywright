using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright;

/// <summary>
/// The game's languages. Translations are gettext files in locale/, one per language named for its locale (de.po,
/// pt_BR.po, zh_CN.po), made from the template tools/loc writes there (keywright.pot): drop one in and it's offered in
/// Settings > Game > Language ("System" follows the computer's own language when there's a translation for it, else
/// English). The language is set when the game starts (<see cref="Apply"/>): <see cref="Loc"/> looks texts up in it,
/// numbers are written its way, and the font falls back to Galmuri for letters Kenney Mini hasn't got.
/// </summary>
public static class Localization
{
    public const string Folder = "res://locale";

    /// <summary>The pseudo-translation's code (Settings, in GM mode or a debug build; or --pseudo).</summary>
    public const string PseudoCode = "pseudo";

    /// <summary>What each language is called in itself, for the ones a translation is likeliest in.</summary>
    static readonly Dictionary<string, string> Endonyms = new()
    {
        ["en"] = "English", ["de"] = "Deutsch", ["fr"] = "Français", ["es"] = "Español", ["es_419"] = "Español (Latinoamérica)",
        ["it"] = "Italiano", ["pt"] = "Português", ["pt_BR"] = "Português (Brasil)", ["nl"] = "Nederlands", ["pl"] = "Polski",
        ["cs"] = "Čeština", ["tr"] = "Türkçe", ["ru"] = "Русский", ["uk"] = "Українська", ["sv"] = "Svenska", ["da"] = "Dansk",
        ["nb"] = "Norsk", ["fi"] = "Suomi", ["hu"] = "Magyar", ["ro"] = "Română", ["el"] = "Ελληνικά", ["ja"] = "日本語",
        ["ko"] = "한국어", ["zh_CN"] = "简体中文", ["zh_TW"] = "繁體中文", [PseudoCode] = "Pseudo (test)",
    };

    static readonly List<string> _available = new() { "en" };
    static bool _loaded;

    /// <summary>The languages there are translations for (English first), by locale.</summary>
    public static IReadOnlyList<string> Available
    {
        get
        {
            Load();
            return _available;
        }
    }

    /// <summary>Every choice in Settings: the system's language, then each one there is (the pseudo-translation too,
    /// for testing).</summary>
    public static IEnumerable<(string Code, string Name)> Languages =>
        new[] { ("", "System") }.Concat(Available.Select(code => (code, Name(code))))
            .Concat(Settings.GmOffered ? new[] { (PseudoCode, Name(PseudoCode)) } : Array.Empty<(string, string)>());

    /// <summary>A language's name in itself ("Deutsch"), or its code if it's one we've no name for.</summary>
    public static string Name(string code) => code is { Length: > 0 } ? Endonyms.GetValueOrDefault(code, code) : "System";

    /// <summary>The language chosen in Settings, or the computer's.</summary>
    public static string Chosen => Settings.Language is { Length: > 0 } chosen && (chosen == PseudoCode || Available.Contains(chosen))
        ? chosen : Best(OS.GetLocale());

    /// <summary>The language the game started in (a change in Settings waits for a restart).</summary>
    public static string Current { get; private set; } = "en";

    /// <summary>The nearest language there is to a locale ("pt_BR" for "pt_BR", "de" for "de_AT", "zh_CN" for
    /// "zh_Hans_CN"), else English.</summary>
    public static string Best(string locale)
    {
        if (string.IsNullOrEmpty(locale)) return "en";
        string normal = locale.Replace('-', '_');
        if (normal.StartsWith("zh")) normal = normal.Contains("Hant") || normal.EndsWith("TW") || normal.EndsWith("HK") ? "zh_TW" : "zh_CN";
        string exact = Available.FirstOrDefault(c => c.Equals(normal, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;
        string language = normal.Split('_')[0];
        return Available.FirstOrDefault(c => c.Split('_')[0] == language) ?? "en";
    }

    /// <summary>Loads every translation in locale/ (once).</summary>
    static void Load()
    {
        if (_loaded) return;
        _loaded = true;
        if (!DirAccess.DirExistsAbsolute(Folder)) return;
        // An exported game keeps "de.po.import" or "de.po.remap" where the editor has "de.po".
        foreach (string file in DirAccess.GetFilesAt(Folder))
        {
            string name = file.EndsWith(".import") ? file[..^".import".Length] : file.EndsWith(".remap") ? file[..^".remap".Length] : file;
            if (!name.EndsWith(".po")) continue;
            string code = name[..^".po".Length];
            if (_available.Contains(code)) continue;
            if (ResourceLoader.Load<Translation>($"{Folder}/{name}") is not { } translation)
            {
                GD.PushWarning($"Localization: {name} couldn't be read");
                continue;
            }
            translation.Locale = code;
            TranslationServer.AddTranslation(translation);
            _available.Add(code);
        }
    }

    /// <summary>Sets the game in the chosen language: called once, at start-up, before anything's built.</summary>
    public static void Apply(bool pseudo = false)
    {
        Load();
        string code = pseudo ? PseudoCode : Chosen;
        Current = code;
        bool translated = code != "en" && code != PseudoCode;
        TranslationServer.SetLocale(translated ? code : "en");
        Loc.Lookup = translated ? text => TranslationServer.Translate(text).ToString() : null;
        Loc.PluralLookup = translated ? (one, many, n) => TranslationServer.TranslatePlural(one, many, n).ToString() : null;
        Loc.Pseudo = code == PseudoCode;
        var culture = Culture(translated ? code : "en");
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        SetUpFont();
    }

    /// <summary>How numbers are written in a language (45,000 in English, 45.000 in German).</summary>
    static CultureInfo Culture(string code)
    {
        try
        {
            return code == "en" ? CultureInfo.GetCultureInfo("en-US") : CultureInfo.GetCultureInfo(code.Replace('_', '-'));
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    /// <summary>
    /// Kenney Mini has ASCII and Latin-1 (enough for English and western Europe); Galmuri 9 (OFL) has the rest of Latin,
    /// Cyrillic, Greek, Hangul, kana and the Japanese kanji, without smoothing so it stays pixel art beside it; and the
    /// computer's own fonts anything else (Simplified Chinese wants a font of its own: see docs/release.md).
    /// </summary>
    static void SetUpFont()
    {
        const string fallback = "res://assets/fonts/galmuri9.ttf";
        if (UiKit.Font is not FontFile kenney || !ResourceLoader.Exists(fallback)) return;
        var galmuri = GD.Load<FontFile>(fallback);
        galmuri.Antialiasing = TextServer.FontAntialiasing.None;
        galmuri.Hinting = TextServer.Hinting.None;
        galmuri.SubpixelPositioning = TextServer.SubpixelPositioning.Disabled;
        galmuri.AllowSystemFallback = true;
        kenney.Fallbacks = new Godot.Collections.Array<Font> { galmuri };
    }
}
