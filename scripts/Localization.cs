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
            .Concat(OS.IsDebugBuild() || Settings.Gm ? new[] { (PseudoCode, Name(PseudoCode)) } : Array.Empty<(string, string)>());

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

    /// <summary>
    /// Sets the game in the chosen language: called once, at start-up, before anything's built. <paramref name="lang"/>
    /// (--lang=CODE) plays in a language for this run only, for testing: its fonts even without a translation yet.
    /// </summary>
    public static void Apply(bool pseudo = false, string lang = null)
    {
        Load();
        string code = pseudo ? PseudoCode : !string.IsNullOrEmpty(lang) ? lang : Chosen;
        Current = code;
        bool translated = code != "en" && code != PseudoCode && Available.Contains(code);
        TranslationServer.SetLocale(translated ? code : "en");
        Loc.Lookup = translated ? text => TranslationServer.Translate(text).ToString() : null;
        Loc.PluralLookup = translated ? (one, many, n) => TranslationServer.TranslatePlural(one, many, n).ToString() : null;
        Loc.Pseudo = code == PseudoCode;
        // In the log, for a bug report (and CI's check that an exported game carries its translations).
        GD.Print($"Languages: {string.Join(", ", Available)}; playing in {(translated ? code : Loc.Pseudo ? "pseudo" : "en")}");
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

    /// <summary>The languages written in Fusion Pixel 12px (OFL), cut down for each by tools/fonts.py.</summary>
    static readonly HashSet<string> Cjk = new() { "ja", "zh_CN", "zh_TW" };

    /// <summary>
    /// The font everything's written in: Kenney Mini, on an 8px grid, for English and the languages of western Europe
    /// (with oe, the en dash and French's no-break spaces added by tools/fonts.py), and Fusion Pixel, on a 12px grid,
    /// for Japanese and Chinese (Latin letters and digits too, so a line keeps one look).
    /// </summary>
    public static Font MainFont { get; private set; }

    /// <summary>The main font's pixel grid: it's shown at whole multiples of it (<see cref="UiKit.SnapFont"/>).</summary>
    public static int Grid { get; private set; } = 8;

    /// <summary>
    /// Kenney Mini has ASCII and Latin-1 (enough for English and western Europe); Galmuri 9 (OFL) has the rest of Latin,
    /// Cyrillic, Greek, Hangul, kana and the Japanese kanji, without smoothing so it stays pixel art beside it; Fusion
    /// Pixel (OFL) Japanese and Chinese, as the main font in those languages and the last fallback in the others (the
    /// language list names Simplified Chinese in it); and the computer's own fonts anything else.
    /// </summary>
    static void SetUpFont()
    {
        var fonts = FontsFor(Current);
        if (fonts.Count == 0) return;
        fonts[0].Fallbacks = new Godot.Collections.Array<Font>(fonts.Skip(1));
        MainFont = fonts[0];
        Grid = Cjk.Contains(Current) && fonts[0].ResourcePath.Contains("fusion") ? 12 : 8;
        // The game's controls hang under Node2Ds and canvas layers, which don't pass a window's theme on: they take
        // their font from Godot's default theme, which the project setting fills with Kenney Mini.
        ThemeDB.FallbackFont = MainFont;
        ThemeDB.FallbackFontSize = UiKit.SnapFont(16);
        var theme = ThemeDB.GetDefaultTheme();
        theme.DefaultFont = MainFont;
        theme.DefaultFontSize = UiKit.SnapFont(16);
    }

    /// <summary>
    /// A language's fonts, main font first and then what it falls back to: Fusion Pixel's cut for the language, Kenney
    /// Mini and Galmuri in Japanese and Chinese; Kenney Mini, Galmuri and Fusion Pixel's Simplified Chinese in the rest.
    /// </summary>
    public static List<FontFile> FontsFor(string code)
    {
        var kenney = PixelFont("res://assets/fonts/kenney_mini.ttf");
        var galmuri = PixelFont("res://assets/fonts/galmuri9.ttf");
        var fusion = PixelFont($"res://assets/fonts/fusion_pixel_12_{(Cjk.Contains(code) ? code : "zh_CN")}.ttf");
        var order = Cjk.Contains(code) && fusion != null ? new[] { fusion, kenney, galmuri } : new[] { kenney, galmuri, fusion };
        return order.Where(f => f != null).ToList();
    }

    /// <summary>A pixel font, drawn without smoothing so its pixels stay square (null if it isn't there).</summary>
    static FontFile PixelFont(string path)
    {
        if (!ResourceLoader.Exists(path)) return null;
        var font = GD.Load<FontFile>(path);
        font.Antialiasing = TextServer.FontAntialiasing.None;
        font.Hinting = TextServer.Hinting.None;
        font.SubpixelPositioning = TextServer.SubpixelPositioning.Disabled;
        font.GenerateMipmaps = false;
        font.AllowSystemFallback = true;
        return font;
    }
}
