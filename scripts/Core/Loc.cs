using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Keywright.Core;

/// <summary>
/// What the player reads, in their language. Every text that's shown goes through <see cref="T(string)"/>, where it's
/// written: <c>Loc.T("Pass")</c>, and an interpolated one, <c>Loc.T($"Day {day} of {days}")</c>, is looked up as one
/// sentence with numbered holes ("Day {0} of {1}"), so a translation can put them where its grammar wants them; the
/// holes are filled after. <see cref="Plural"/> picks a sentence by a count. English is the source and the fallback: a
/// text with no translation shows as written, and the sim and the tests, which set no <see cref="Lookup"/>, run in
/// English. `tools/loc` lists every text for translators (locale/keywright.pot) and wrapped them all in the first place.
/// <para>
/// Data tables are translated where they're written too, so a language is chosen when the game starts (changing it
/// takes a restart). A name that's also a key, kept in saves (a thing's name, a Heartsong's), stays English where it's
/// defined, marked by <see cref="Key"/> so it's still listed, and is translated where it's shown.
/// </para>
/// </summary>
public static class Loc
{
    static Func<string, string> _lookup;
    static Func<string, string, int, string> _plural;
    static bool _pseudo;
    static readonly ConcurrentDictionary<string, string> _cache = new();

    /// <summary>Finds a text's translation (null or empty if there's none). The game sets it when it starts.</summary>
    public static Func<string, string> Lookup
    {
        get => _lookup;
        set { _lookup = value; _cache.Clear(); }
    }

    /// <summary>Finds the form of a sentence for a count (its English singular and plural, and the count).</summary>
    public static Func<string, string, int, string> PluralLookup
    {
        get => _plural;
        set { _plural = value; _cache.Clear(); }
    }

    /// <summary>
    /// Pseudo-translation, to see what isn't translated and which layouts are too tight for longer languages: every
    /// translated text comes out in brackets, a third longer, its vowels doubled ("[Daay {0} oof {1} ~~~]"). Holes and
    /// "{price}"-style names are left alone.
    /// </summary>
    public static bool Pseudo
    {
        get => _pseudo;
        set { _pseudo = value; _cache.Clear(); }
    }

    /// <summary>Anything to do: a language other than English, or the pseudo-translation.</summary>
    internal static bool Active => _lookup != null || _pseudo;

    /// <summary>A text the player reads, in their language.</summary>
    public static string T(string text)
    {
        if (string.IsNullOrEmpty(text) || !Active) return text;
        return _cache.GetOrAdd(text, Translate);
    }

    /// <summary>An interpolated text, translated as one sentence with its holes numbered, then filled.</summary>
    public static string T(ref Text text) => text.Finish();

    /// <summary>
    /// One of two sentences by a count: <c>Loc.Plural(n, $"{n} week early", $"{n} weeks early")</c>. A translation
    /// has as many forms as its language needs (Polish has three, Japanese one).
    /// </summary>
    public static string Plural(int count, string one, string many)
    {
        if (!Active) return count == 1 ? one : many;
        string found = _plural?.Invoke(one, many, count);
        string result = string.IsNullOrEmpty(found) ? (count == 1 ? one : many) : found;
        return _pseudo ? Pseudolocalize(result) : result;
    }

    /// <inheritdoc cref="Plural(int, string, string)"/>
    public static string Plural(int count, ref Text one, ref Text many)
    {
        if (!Active) return count == 1 ? one.Finish() : many.Finish();
        string oneTemplate = one.Template, manyTemplate = many.Template;
        string found = _plural?.Invoke(oneTemplate, manyTemplate, count);
        string template = string.IsNullOrEmpty(found) ? (count == 1 ? oneTemplate : manyTemplate) : found;
        if (_pseudo) template = Pseudolocalize(template);
        return (count == 1 ? one : many).Fill(template);
    }

    /// <summary>
    /// A name that's also a key, so it stays English in the data and in saves: listed for the translators like any
    /// other text, and translated with <see cref="T(string)"/> where it's shown.
    /// </summary>
    public static string Key(string text) => text;

    static string Translate(string text)
    {
        string found = _lookup?.Invoke(text);
        string result = string.IsNullOrEmpty(found) ? text : found;
        return _pseudo ? Pseudolocalize(result) : result;
    }

    /// <summary>The pseudo-translation of a text (see <see cref="Pseudo"/>).</summary>
    public static string Pseudolocalize(string text)
    {
        var sb = new StringBuilder(text.Length * 4 / 3 + 6);
        sb.Append('[');
        int letters = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            // Holes ("{0}") and names filled in later ("{price}") pass through untouched.
            if (c == '{')
            {
                int end = text.IndexOf('}', i);
                if (end > i)
                {
                    sb.Append(text, i, end - i + 1);
                    i = end;
                    continue;
                }
            }
            sb.Append(c);
            if (char.IsLetter(c)) letters++;
            if ("aeiouAEIOU".IndexOf(c) >= 0) sb.Append(c);
        }
        if (letters > 0) sb.Append(' ').Append('~', Math.Max(1, letters / 6));
        sb.Append(']');
        return sb.ToString();
    }

    /// <summary>
    /// Fills a translated template's holes ("{0}", "{1}") with what was interpolated; anything else in braces (a
    /// "{price}" to be filled later) is left as it is.
    /// </summary>
    internal static string Fill(string template, List<string> holes)
    {
        if (holes == null || holes.Count == 0 || template.IndexOf('{') < 0) return template;
        var sb = new StringBuilder(template.Length + 16);
        for (int i = 0; i < template.Length; i++)
        {
            char c = template[i];
            if (c == '{')
            {
                int j = i + 1, n = 0;
                while (j < template.Length && char.IsDigit(template[j])) n = n * 10 + (template[j++] - '0');
                if (j > i + 1 && j < template.Length && template[j] == '}' && n < holes.Count)
                {
                    sb.Append(holes[n]);
                    i = j;
                    continue;
                }
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// An interpolated text on its way to <see cref="T(ref Text)"/>. With no translation to do it builds the string
    /// as C# would; otherwise it keeps the sentence with numbered holes ("Day {0} of {1}") to look up, and what goes
    /// in each, formatted as C# would format it.
    /// </summary>
    [InterpolatedStringHandler]
    public ref struct Text
    {
        readonly StringBuilder _text;
        readonly List<string> _holes;
        readonly bool _plain;

        public Text(int literalLength, int formattedCount)
        {
            _plain = !Active;
            _text = new StringBuilder(literalLength + formattedCount * 8);
            _holes = _plain ? null : new List<string>(formattedCount);
        }

        public void AppendLiteral(string text) => _text.Append(text);

        public void AppendFormatted<T>(T value) => Hole(Format(value, null));

        public void AppendFormatted<T>(T value, string format) => Hole(Format(value, format));

        public void AppendFormatted<T>(T value, int alignment) => Hole(Align(Format(value, null), alignment));

        public void AppendFormatted<T>(T value, int alignment, string format) => Hole(Align(Format(value, format), alignment));

        public void AppendFormatted(string value) => Hole(value ?? "");

        void Hole(string text)
        {
            if (_plain)
            {
                _text.Append(text);
                return;
            }
            _text.Append('{').Append(_holes.Count).Append('}');
            _holes.Add(text);
        }

        static string Format<T>(T value, string format) => value switch
        {
            null => "",
            IFormattable formattable => formattable.ToString(format, CultureInfo.CurrentCulture),
            _ => value.ToString() ?? "",
        };

        static string Align(string text, int alignment) => alignment >= 0 ? text.PadLeft(alignment) : text.PadRight(-alignment);

        /// <summary>The sentence with its holes numbered, as the translators see it.</summary>
        internal string Template => _text.ToString();

        internal string Fill(string template) => Loc.Fill(template, _holes);

        /// <summary>The text in the player's language.</summary>
        internal string Finish() => _plain ? _text.ToString() : Fill(T(Template));
    }
}
