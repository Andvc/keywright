using System.Text;
using System.Text.RegularExpressions;

namespace LocTool;

/// <summary>
/// Checks the translations in locale/ against the template: `dotnet run --project tools/loc -- verify [--strict]`.
/// For each &lt;locale&gt;.po it counts what's translated, untranslated, fuzzy and left over from texts that are gone,
/// and fails (exit 1) on what would show wrong in the game: a hole ("{0}") or a token ("{price}") lost or invented, the
/// wrong number of plural forms, or a character none of the language's fonts can draw. --strict fails on untranslated
/// texts too. The fonts are the ones Localization.SetUpFont gives the language: Fusion Pixel's cut for Japanese and
/// Chinese (tools/fonts.py), else Kenney Mini; then the fallbacks.
/// </summary>
static class Verify
{
    static readonly Regex Hole = new(@"\{[0-9]+\}");
    static readonly Regex Token = new(@"\{[a-z_]+\}");

    public static int Run(string root, bool strict)
    {
        string locale = Path.Combine(root, "locale");
        var template = Po.Read(Path.Combine(locale, "keywright.pot"));
        var ids = template.Select(e => e.Key).ToHashSet();
        int failures = 0;
        foreach (string path in Directory.GetFiles(locale, "*.po").OrderBy(p => p))
        {
            string code = Path.GetFileNameWithoutExtension(path);
            var po = Po.Read(path);
            int plurals = Po.Plurals(path);
            var byKey = po.ToDictionary(e => e.Key);
            var problems = new List<string>();
            int translated = 0, untranslated = 0, fuzzy = 0;
            var fonts = Fonts(root, code);
            var missingGlyphs = new SortedSet<char>();
            foreach (var source in template)
            {
                if (!byKey.TryGetValue(source.Key, out var e) || e.Strings.All(string.IsNullOrEmpty))
                {
                    untranslated++;
                    continue;
                }
                if (e.Fuzzy) fuzzy++;
                else translated++;
                if (source.Plural != null && e.Strings.Count != plurals)
                    problems.Add($"{Short(source.Id)}: {e.Strings.Count} plural forms, the header says {plurals}");
                for (int i = 0; i < e.Strings.Count; i++)
                {
                    string text = e.Strings[i];
                    if (string.IsNullOrEmpty(text)) continue;
                    string english = source.Plural != null && (i > 0 || plurals == 1) ? source.Plural : source.Id;
                    if (!SameHoles(english, text, source.Plural != null))
                        problems.Add($"{Short(source.Id)}: holes {List(Hole, english)} in English, {List(Hole, text)} in \"{Short(text)}\"");
                    if (List(Token, english) != List(Token, text))
                        problems.Add($"{Short(source.Id)}: tokens {List(Token, english)} in English, {List(Token, text)} in \"{Short(text)}\"");
                    foreach (char c in text)
                        if (!char.IsWhiteSpace(c) && !char.IsControl(c) && fonts.Count > 0 && !fonts.Any(f => f.Contains(c)))
                            missingGlyphs.Add(c);
                }
            }
            int obsolete = po.Count(e => !ids.Contains(e.Key));
            if (missingGlyphs.Count > 0)
                problems.Add($"characters its fonts haven't got: {new string(missingGlyphs.ToArray())} (tools/fonts.py fusion adds a translation's characters)");
            if (strict && untranslated > 0) problems.Add($"{untranslated} untranslated");
            Console.WriteLine($"{code}.po: {translated} of {template.Count} translated, {untranslated} untranslated, {fuzzy} fuzzy" +
                              (obsolete > 0 ? $", {obsolete} no longer used" : "") + (problems.Count == 0 ? ": ok" : $", {problems.Count} problem(s)"));
            foreach (string p in problems.Take(40)) Console.WriteLine("  " + p);
            if (problems.Count > 40) Console.WriteLine($"  ... and {problems.Count - 40} more");
            failures += problems.Count;
        }
        return failures > 0 ? 1 : 0;
    }

    /// <summary>The same numbered holes; a plural's singular form may leave the number out ("a week" for 1).</summary>
    static bool SameHoles(string english, string text, bool plural)
    {
        var want = Hole.Matches(english).Select(m => m.Value).OrderBy(s => s).ToList();
        var got = Hole.Matches(text).Select(m => m.Value).OrderBy(s => s).ToList();
        return want.SequenceEqual(got) || plural && got.All(want.Contains) && got.Distinct().Count() == got.Count;
    }

    static string List(Regex regex, string text) => string.Join(" ", regex.Matches(text).Select(m => m.Value).OrderBy(s => s));

    static string Short(string text) => (text.Length > 50 ? text[..47] + "..." : text).Replace("\n", "\\n");

    /// <summary>The fonts a language is drawn in, as Localization.SetUpFont chooses them.</summary>
    static List<Cmap> Fonts(string root, string code)
    {
        string dir = Path.Combine(root, "assets", "fonts");
        string own = Path.Combine(dir, $"fusion_pixel_12_{code}.ttf");
        var paths = File.Exists(own)
            ? new[] { own, Path.Combine(dir, "kenney_mini.ttf"), Path.Combine(dir, "galmuri9.ttf") }
            : new[] { Path.Combine(dir, "kenney_mini.ttf"), Path.Combine(dir, "galmuri9.ttf"), Path.Combine(dir, "fusion_pixel_12_zh_CN.ttf") };
        return paths.Where(File.Exists).Select(Cmap.Read).ToList();
    }
}

/// <summary>A gettext file's entries, as much of the format as Poedit and tools/loc write.</summary>
sealed class Po
{
    public string Context, Id, Plural;
    public List<string> Strings = new();
    public bool Fuzzy;
    public string Key => (Context != null ? Context + "\u0004" : "") + Id;

    public static List<Po> Read(string path)
    {
        var entries = new List<Po>();
        Po current = null;
        Action<string> append = null;
        foreach (string raw in File.ReadLines(path, Encoding.UTF8))
        {
            string line = raw.Trim();
            if (line.Length == 0)
            {
                Finish();
                continue;
            }
            if (line.StartsWith("#,"))
            {
                current ??= new Po();
                current.Fuzzy |= line.Contains("fuzzy");
                continue;
            }
            if (line.StartsWith('#')) continue;
            var m = Regex.Match(line, @"^(msgctxt|msgid_plural|msgid|msgstr(?:\[(\d+)\])?)\s+""(.*)""$");
            if (m.Success)
            {
                current ??= new Po();
                var entry = current;
                string value = Unquote(m.Groups[3].Value);
                switch (m.Groups[1].Value)
                {
                    case "msgctxt":
                        entry.Context = value;
                        append = s => entry.Context += s;
                        break;
                    case "msgid":
                        entry.Id = value;
                        append = s => entry.Id += s;
                        break;
                    case "msgid_plural":
                        entry.Plural = value;
                        append = s => entry.Plural += s;
                        break;
                    default:
                        int index = entry.Strings.Count;
                        entry.Strings.Add(value);
                        append = s => entry.Strings[index] += s;
                        break;
                }
            }
            else if (line.StartsWith('"') && line.EndsWith('"') && append != null)
                append(Unquote(line[1..^1]));
        }
        Finish();
        return entries;

        void Finish()
        {
            if (current?.Id is { Length: > 0 }) entries.Add(current);
            current = null;
            append = null;
        }
    }

    /// <summary>nplurals from the header (1 if there's none).</summary>
    public static int Plurals(string path)
    {
        var m = Regex.Match(File.ReadAllText(path), @"nplurals\s*=\s*(\d+)");
        return m.Success ? int.Parse(m.Groups[1].Value) : 1;
    }

    static string Unquote(string s)
    {
        var sb = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '\\' && i + 1 < s.Length)
            {
                char n = s[++i];
                sb.Append(n switch { 'n' => '\n', 't' => '\t', '"' => '"', '\\' => '\\', _ => n });
            }
            else sb.Append(s[i]);
        }
        return sb.ToString();
    }
}

/// <summary>The characters a TrueType or OpenType font has, from its cmap table (formats 4 and 12).</summary>
sealed class Cmap
{
    readonly HashSet<int> _codes = new();

    public bool Contains(char c) => _codes.Contains(c);

    public static Cmap Read(string path)
    {
        var cmap = new Cmap();
        byte[] data = File.ReadAllBytes(path);
        int tables = U16(data, 4);
        for (int t = 0; t < tables; t++)
        {
            int record = 12 + t * 16;
            if (Encoding.ASCII.GetString(data, record, 4) != "cmap") continue;
            int start = (int)U32(data, record + 8);
            int count = U16(data, start + 2);
            for (int s = 0; s < count; s++)
            {
                int platform = U16(data, start + 4 + s * 8), encoding = U16(data, start + 6 + s * 8);
                if (!(platform == 3 && (encoding == 1 || encoding == 10) || platform == 0)) continue;
                int sub = start + (int)U32(data, start + 8 + s * 8);
                int format = U16(data, sub);
                if (format == 4) cmap.Format4(data, sub);
                else if (format == 12) cmap.Format12(data, sub);
            }
        }
        return cmap;
    }

    void Format4(byte[] d, int sub)
    {
        int segments = U16(d, sub + 6) / 2;
        int ends = sub + 14, starts = ends + segments * 2 + 2, deltas = starts + segments * 2, offsets = deltas + segments * 2;
        for (int i = 0; i < segments; i++)
        {
            int end = U16(d, ends + i * 2), begin = U16(d, starts + i * 2), delta = (short)U16(d, deltas + i * 2);
            int offset = U16(d, offsets + i * 2);
            for (int c = begin; c <= end && c != 0xFFFF; c++)
            {
                int glyph;
                if (offset == 0) glyph = (c + delta) & 0xFFFF;
                else
                {
                    int at = offsets + i * 2 + offset + (c - begin) * 2;
                    glyph = at + 1 < d.Length ? U16(d, at) : 0;
                    if (glyph != 0) glyph = (glyph + delta) & 0xFFFF;
                }
                if (glyph != 0) _codes.Add(c);
            }
        }
    }

    void Format12(byte[] d, int sub)
    {
        long groups = U32(d, sub + 12);
        for (long g = 0; g < groups; g++)
        {
            int at = sub + 16 + (int)g * 12;
            long first = U32(d, at), last = U32(d, at + 4);
            for (long c = first; c <= last && c <= 0xFFFF; c++) _codes.Add((int)c);
        }
    }

    static int U16(byte[] d, int at) => d[at] << 8 | d[at + 1];

    static long U32(byte[] d, int at) => (long)d[at] << 24 | (long)d[at + 1] << 16 | (long)d[at + 2] << 8 | d[at + 3];
}
