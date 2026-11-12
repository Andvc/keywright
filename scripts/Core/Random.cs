using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

public static class RandomExtensions
{
    public static float NextFloat(this Random rng) => (float)rng.NextDouble();
    public static float Between(this Random rng, float min, float max) => min + (max - min) * (float)rng.NextDouble();
    public static T PickOne<T>(this Random rng, T[] items) => items[rng.Next(items.Length)];

    /// <summary>Picks by weight. Returns default if nothing has weight.</summary>
    public static T PickWeighted<T>(this Random rng, T[] items, Func<T, float> weight)
    {
        float total = items.Sum(weight);
        if (total <= 0) return default;
        float roll = rng.NextFloat() * total;
        foreach (var item in items)
        {
            roll -= weight(item);
            if (roll <= 0) return item;
        }
        return items[^1];
    }
}

public static class Money
{
    /// <summary>Small prices to the coin, bigger ones to the nearest 5.</summary>
    public static int Round(float value) => value <= 30 ? Math.Max(1, (int)MathF.Round(value)) : (int)MathF.Round(value / 5f) * 5;

    /// <summary>A sum as people say it ("12 gold", "45,000 gold"): the interface writes "45000g", but nobody talks like that.</summary>
    /// <summary>A sum as the interface writes it: "45000g" in English, "45000 po", "45000G", "45000金".</summary>
    public static string Short(int amount) => Loc.T($"{amount}g");

    public static string Say(int amount) => amount == 1 ? Loc.T("one coin") : Loc.T($"{amount.ToString("N0", System.Globalization.CultureInfo.CurrentCulture)} gold");
}

/// <summary>Words joined the way the player's language joins them.</summary>
public static class Words
{
    /// <summary>
    /// How long a sentence is left to be read before the next is said by itself (0.36: a speech plays through without a
    /// click a sentence; a click shows the rest at once). CJK text says as much in fewer characters.
    /// </summary>
    public static double ReadTime(string sentence)
    {
        int length = sentence?.Length ?? 0;
        bool dense = sentence != null && sentence.Any(c => c >= 0x3000);
        return Math.Clamp(0.5 + (dense ? 0.11 : 0.045) * length, 1.0, dense ? 3.4 : 2.8);
    }

    const string Ends = ".!?\u2026\u3002\uff01\uff1f", Closers = "\"'\u201d\u2019)\u00bb\u300d\u300f\uff09]";

    /// <summary>
    /// A text cut into sentences, for dialogue and tips shown one at a time (0.35): at a full stop, a question or an
    /// exclamation (with any closing quote after it) followed by a space, or by nothing in a language written without
    /// spaces (Japanese and Chinese stops end a sentence where they stand); at line breaks too. An ellipsis or a stop
    /// with a small letter after it doesn't end one ("That song... you know it."), and a scrap of a sentence ("Morning!",
    /// "Quick!") goes with its neighbour rather than standing alone.
    /// </summary>
    public static List<string> Sentences(string text)
    {
        var sentences = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return sentences;
        foreach (string paragraph in text.Split('\n'))
        {
            var pieces = new List<string>();
            int start = 0;
            for (int i = 0; i < paragraph.Length; i++)
            {
                char c = paragraph[i];
                if (Ends.IndexOf(c) < 0) continue;
                bool wide = c >= '\u3000';
                int end = i + 1;
                // A run of stops ("?!", "...") is one ending.
                while (end < paragraph.Length && Ends.IndexOf(paragraph[end]) >= 0) end++;
                while (end < paragraph.Length && Closers.IndexOf(paragraph[end]) >= 0) end++;
                bool ellipsis = end - i >= 3 && paragraph[i] == '.' || c == '\u2026';
                if (!wide)
                {
                    // A Latin stop ends a sentence only before a space (or the end), and not before a small letter.
                    if (end < paragraph.Length && paragraph[end] != ' ' && paragraph[end] != '\u00a0') continue;
                    int next = end;
                    while (next < paragraph.Length && (paragraph[next] == ' ' || paragraph[next] == '\u00a0')) next++;
                    if (next < paragraph.Length && (char.IsLower(paragraph[next]) || ellipsis && !char.IsUpper(paragraph[next]))) continue;
                }
                pieces.Add(paragraph[start..end].Trim());
                start = end;
                i = end - 1;
            }
            if (start < paragraph.Length && paragraph[start..].Trim().Length > 0) pieces.Add(paragraph[start..].Trim());
            // Scraps join a neighbour: the next sentence if it's the first, else the one before.
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces.Count == 1 || !Scrap(pieces[i])) continue;
                if (i == 0)
                {
                    pieces[1] = pieces[0] + (Spaced(pieces[0]) ? " " : "") + pieces[1];
                    pieces.RemoveAt(0);
                }
                else
                {
                    pieces[i - 1] = pieces[i - 1] + (Spaced(pieces[i - 1]) ? " " : "") + pieces[i];
                    pieces.RemoveAt(i);
                }
                i--;
            }
            sentences.AddRange(Balanced(pieces.Where(p => p.Length > 0).ToList()));
        }
        return sentences;
    }

    static readonly (char Open, char Close)[] Quotes = { ('"', '"'), ('\u201c', '\u201d'), ('\u300c', '\u300d'), ('\u00ab', '\u00bb') };

    /// <summary>A quotation cut across sentences is closed at the end of each piece and opened again at the next.</summary>
    static List<string> Balanced(List<string> pieces)
    {
        foreach (var (open, close) in Quotes)
        {
            int depth = 0;
            for (int i = 0; i < pieces.Count; i++)
            {
                string piece = pieces[i];
                bool startsInside = depth > 0;
                if (open == close) depth = piece.Count(ch => ch == open) % 2 == 1 ? (depth > 0 ? 0 : 1) : depth;
                else depth = Math.Max(0, depth + piece.Count(ch => ch == open) - piece.Count(ch => ch == close));
                if (startsInside) piece = open + piece;
                if (depth > 0) piece += close;
                pieces[i] = piece;
            }
        }
        return pieces;
    }

    /// <summary>Too short to stand alone: a word or two ("Morning!"), or a few characters in Japanese or Chinese.</summary>
    static bool Scrap(string sentence) => Spaced(sentence) ? sentence.Length < 10 : sentence.Length < 4;

    /// <summary>Written with spaces between words (not Japanese or Chinese).</summary>
    static bool Spaced(string text) => text.Any(c => c == ' ') || text.All(c => c < '\u3000');

    /// <summary>
    /// A list as the language writes one: "coin, gems, spells" in English, "硬貨、宝石、呪文" in Japanese (the
    /// translators' "{0}, {1}").
    /// </summary>
    public static string List(IEnumerable<string> items)
    {
        string result = null;
        foreach (string item in items)
            result = result == null ? item : Loc.T($"{result}, {item}");
        return result ?? "";
    }

    /// <summary>
    /// A long text broken into lines for a tooltip (a tooltip's label doesn't wrap by itself, and a long one ran off the
    /// screen, 0.34): at spaces, about <paramref name="width"/> letters a line; a language written without spaces
    /// (Japanese, Chinese) every <paramref name="width"/> / 2 characters. Line breaks already in it are kept.
    /// </summary>
    public static string Wrap(string text, int width = 64)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var lines = new List<string>();
        foreach (string paragraph in text.Split('\n'))
        {
            bool spaced = paragraph.Length < width || paragraph.Count(c => c == ' ') * 12 >= paragraph.Length;
            if (!spaced)
            {
                int n = Math.Max(8, width / 2);
                for (int i = 0; i < paragraph.Length; i += n) lines.Add(paragraph.Substring(i, Math.Min(n, paragraph.Length - i)));
                continue;
            }
            var line = new System.Text.StringBuilder();
            foreach (string word in paragraph.Split(' '))
            {
                if (line.Length > 0 && line.Length + 1 + word.Length > width)
                {
                    lines.Add(line.ToString());
                    line.Clear();
                }
                if (line.Length > 0) line.Append(' ');
                line.Append(word);
            }
            lines.Add(line.ToString());
        }
        return string.Join("\n", lines);
    }
}
