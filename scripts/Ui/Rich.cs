using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// Dialogue and tips with what matters coloured (0.35): BBCode for a <see cref="RichTextLabel"/>, the text's own brackets
/// escaped ("[N]" is a key, not a tag), and phrases highlighted wherever they stand: the words of the trade in the
/// language being played (the kinds as the slip names them, their sounds, the strengths, the glossary's terms), a
/// seller's price and what they state, the action a lesson asks for.
/// </summary>
public static class Rich
{
    /// <summary>What a sound is, as the words over the chest say it.</summary>
    public static readonly Color Sound = new("#9ad7ff");

    /// <summary>How hard to shake.</summary>
    public static readonly Color Strength = new("#8fe3a6");

    /// <summary>A label that can colour words, in the game's font and its fallbacks (the theme's), wrapped at words.</summary>
    public static RichTextLabel Make(int size, Color color, HorizontalAlignment align = HorizontalAlignment.Left, int outline = 0)
    {
        var rich = new RichTextLabel
        {
            BbcodeEnabled = true,
            ScrollActive = false,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            HorizontalAlignment = align,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        rich.AddThemeFontSizeOverride("normal_font_size", UiKit.SnapFont(size));
        rich.AddThemeColorOverride("default_color", color);
        if (outline > 0)
        {
            rich.AddThemeConstantOverride("outline_size", 4 * ((outline + 3) / 4));
            rich.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.9f));
        }
        return rich;
    }

    /// <summary>The text as BBCode shows it, word for word: its own brackets aren't tags.</summary>
    public static string Escape(string text) => string.IsNullOrEmpty(text) ? "" : text.Replace("[", "[lb]"); // loc: keys

    /// <summary>A whole text in one colour.</summary>
    public static string Colour(string text, Color color) => $"[color=#{color.ToHtml(false)}]{Escape(text)}[/color]";

    /// <summary>
    /// The text, escaped, with these phrases coloured wherever they appear (the longest first; never inside another). A
    /// phrase in a language written with spaces matches a whole word, ignoring case, with its own ending ("clank" in
    /// "clanking", "Cloak" in "cloaks"); one in Japanese or Chinese matches where it stands.
    /// </summary>
    public static string Mark(string text, IEnumerable<(string Phrase, Color Color)> marks)
    {
        if (string.IsNullOrEmpty(text)) return "";
        var spans = new List<(int Start, int End, Color Color)>();
        foreach (var (phrase, color) in marks.Where(m => !string.IsNullOrWhiteSpace(m.Phrase)).OrderByDescending(m => m.Phrase.Length))
        {
            bool spaced = phrase.All(c => c < '　');
            int from = 0;
            while (from < text.Length)
            {
                int at = text.IndexOf(phrase, from, spaced ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                if (at < 0) break;
                int end = at + phrase.Length;
                from = at + 1;
                if (spaced)
                {
                    if (at > 0 && char.IsLetter(text[at - 1])) continue;
                    // A word's own ending goes with it (a few letters); a phrase of several words must end where it ends.
                    if (!phrase.Contains(' ')) while (end < text.Length && char.IsLetter(text[end]) && end - at < phrase.Length + 4) end++;
                    else if (end < text.Length && char.IsLetter(text[end])) continue;
                }
                if (spans.Any(s => at < s.End && end > s.Start)) continue;
                spans.Add((at, end, color));
                from = end;
            }
        }
        spans.Sort((a, b) => a.Start.CompareTo(b.Start));
        var built = new StringBuilder();
        int i = 0;
        foreach (var (start, end, color) in spans)
        {
            built.Append(Escape(text[i..start])).Append($"[color=#{color.ToHtml(false)}]").Append(Escape(text[start..end])).Append("[/color]"); // loc: keys
            i = end;
        }
        return built.Append(Escape(text[i..])).ToString();
    }

    /// <summary>A text with the trade's words coloured, and any of these phrases besides.</summary>
    public static string Trade(string text, params (string Phrase, Color Color)[] more) => Mark(text, TradeWords.Concat(more));

    static List<(string, Color)> _trade, _strengths, _paper;

    /// <summary>
    /// Just the three strengths (and how a shake is said): what a seller's own words are coloured by. Their talk is full of
    /// "coin" and "gold" as money, so the kinds would mislead there; how they shook is their way of judging.
    /// </summary>
    public static IReadOnlyList<(string Phrase, Color Color)> Strengths => _strengths ??= TradeWords.Where(w => w.Color == Strength).ToList();

    /// <summary>The trade's words in inks that read on the slip's paper: the kinds gold-brown, sounds blue, strengths green.</summary>
    public static IReadOnlyList<(string Phrase, Color Color)> OnPaper => _paper ??= TradeWords
        .Select(w => (w.Phrase, w.Color == Sound ? InkSound : w.Color == Strength ? InkStrength : w.Color == Palette.Magic ? Palette.InkMagic : Palette.InkGold))
        .ToList();

    /// <summary>A sound, in ink.</summary>
    public static readonly Color InkSound = new("#1f4f7a");

    /// <summary>A strength, in ink.</summary>
    public static readonly Color InkStrength = new("#256b34");

    /// <summary>
    /// The words of the trade in the language being played: the kinds as the slip names them (and one of them), the sound
    /// each makes, the three strengths, and the glossary's terms (without an article in front).
    /// </summary>
    public static IReadOnlyList<(string Phrase, Color Color)> TradeWords => _trade ??= BuildTrade();

    static List<(string, Color)> BuildTrade()
    {
        var words = new List<(string, Color)>();
        foreach (var def in Categories.Kinds)
        {
            words.Add((def.Name, Palette.Gold));
            // "Cloaks" on the slip, "a cloak" in a tip.
            if (def.Name.Length > 4 && def.Name.EndsWith('s')) words.Add((def.Name[..^1], Palette.Gold));
            words.Add((def.Word, Sound));
        }
        words.Add((Loc.T("boom"), Sound));
        foreach (var level in new[] { Core.Shake.Gentle, Core.Shake.Firm, Core.Shake.Hard })
        {
            words.Add((Categories.LevelName(level), Strength));
            words.Add((Categories.Adverb(level), Strength));
        }
        foreach (var term in Enum.GetValues<Term>())
        {
            string name = Glossary.Name(term);
            // "A perfect read": "perfect read" wherever it's said (an article of three letters or fewer, before two words or more).
            string[] parts = name.Split(' ');
            if (parts.Length >= 3 && parts[0].Length <= 3) name = string.Join(' ', parts[1..]);
            words.Add((name, Palette.Magic));
        }
        words.Add((Loc.T("False bottom"), Palette.Magic));
        return words.Where(w => w.Item1.Length >= 2).Distinct().ToList();
    }
}
