using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// Types a speech onto a label a character at a time (0.37), with a soft blip as the letters come: the way most
/// pixel-art games say their lines. The speech is still made of segments (its sentences, <see cref="Words.Sentences"/>):
/// the whole of it is laid out first, so nothing reflows as it's typed; each segment follows the last after a breath;
/// <see cref="Finish"/> shows the rest at once (a click, the act key); and the history, the pips and the speaker's stir
/// all go by the segments, as they did when a speech came a sentence at a time (0.35, 0.36).
/// <para>
/// How fast is the player's (Settings > Game > Text speed; "At once" brings each sentence whole, as 0.36 did), and the
/// blips can be turned off. Japanese and Chinese say as much in fewer characters, so they're typed at half the rate.
/// </para>
/// </summary>
public sealed class Typewriter
{
    readonly Control _label;
    readonly List<int> _ends = new();
    readonly List<string> _segments = new();
    string _plain = "";
    int _shown, _segment, _sinceBlip;
    double _wait, _carry;
    float _pitch = 1f, _volume = -15f;
    string _sound = "talk"; // loc: keys
    bool _began;
    readonly Random _rng = new();

    /// <summary>A segment has begun to be typed (its index): the speaker stirs, the pips move.</summary>
    public event Action<int> SegmentBegan;

    /// <summary>The whole speech is out (typed to its end, or finished at a click).</summary>
    public event Action Done;

    public Typewriter(RichTextLabel label)
    {
        _label = label;
        // Laid out whole, and shown glyph by glyph: a word never starts on one line and jumps to the next as it grows.
        label.VisibleCharactersBehavior = TextServer.VisibleCharactersBehavior.CharsAfterShaping;
    }

    public Typewriter(Label label)
    {
        _label = label;
        label.VisibleCharactersBehavior = TextServer.VisibleCharactersBehavior.CharsAfterShaping;
    }

    /// <summary>Characters are still to come.</summary>
    public bool Typing => _shown < _plain.Length;

    /// <summary>The segment being typed (the last, once it's all out).</summary>
    public int Segment => Math.Min(_segment, Math.Max(0, _segments.Count - 1));

    public int Segments => _segments.Count;

    /// <summary>
    /// Begins a speech. <paramref name="segments"/> are its sentences as they read (no markup);
    /// <paramref name="markup"/> is the whole of it as the label shows it (BBCode for a rich label), the sentences joined
    /// by a space. <paramref name="pitch"/> is the speaker's voice.
    /// </summary>
    public void Begin(IReadOnlyList<string> segments, string markup, float pitch = 1f, string sound = "talk", float volume = -15f) // loc: keys
    {
        _segments.Clear();
        _segments.AddRange(segments.Count > 0 ? segments : new[] { "" });
        _plain = string.Join(" ", _segments);
        _ends.Clear();
        int at = 0;
        for (int i = 0; i < _segments.Count; i++)
        {
            at += _segments[i].Length + (i < _segments.Count - 1 ? 1 : 0);
            _ends.Add(at);
        }
        _pitch = pitch;
        _sound = sound;
        _volume = volume;
        _shown = 0;
        _segment = 0;
        _wait = 0;
        _carry = 0;
        _sinceBlip = 0;
        _began = false;
        _latest = new WeakReference<Typewriter>(this);
        if (_label is RichTextLabel rich) rich.Text = markup;
        else if (_label is Label plain) plain.Text = markup;
        Apply();
        // Nothing to type (an empty line), or text that comes at once: out it comes.
        if (_plain.Length == 0) Finish();
    }

    /// <summary>The rest of the speech at once.</summary>
    public void Finish()
    {
        bool was = Typing;
        _shown = _plain.Length;
        _segment = Math.Max(0, _segments.Count - 1);
        _wait = 0;
        Apply();
        if (was || !_began)
        {
            _began = true;
            Done?.Invoke();
        }
    }

    /// <summary>Types on (call every frame while the speech is up).</summary>
    public void Process(double delta)
    {
        if (!Typing) return;
        if (!_began)
        {
            _began = true;
            SegmentBegan?.Invoke(0);
        }
        if (_wait > 0)
        {
            _wait -= delta;
            if (_wait > 0) return;
            delta = -_wait;
            _wait = 0;
        }
        float rate = Rate;
        if (rate <= 0)
        {
            // At once: the segment whole, and the next once this one's had time to be read (as 0.36 said a speech).
            WholeSegment();
            return;
        }
        _carry += delta * rate;
        bool typed = false;
        while (_carry >= 1 && Typing && _wait <= 0)
        {
            _carry -= 1;
            Step();
            typed = true;
        }
        if (typed) Apply();
        if (!Typing) Done?.Invoke();
    }

    /// <summary>One more character: a breath after a comma, a longer one at a sentence's end, a blip for a letter.</summary>
    void Step()
    {
        char c = _plain[_shown];
        _shown++;
        bool dense = c >= 0x3000;
        if (char.IsLetterOrDigit(c) && ++_sinceBlip >= (dense ? 1 : Settings.TextSpeed >= TextSpeed.Fast ? 3 : 2))
        {
            _sinceBlip = 0;
            Blip();
        }
        if (_shown >= _ends[_segment] && _segment < _segments.Count - 1)
        {
            // The sentence is out: a breath, then the next begins.
            _wait = Rest(_segments[_segment]);
            _segment++;
            _carry = 0;
            int next = _segment;
            SegmentBegan?.Invoke(next);
        }
        else if (Pauses.IndexOf(c) >= 0 && _shown < _plain.Length) _wait = 0.09;
        else if ((c == '…' || (c == '.' && _shown < _plain.Length && _plain[_shown] == '.')) && _shown < _plain.Length) _wait = 0.07;
    }

    void WholeSegment()
    {
        _shown = _ends[_segment];
        Blip();
        if (_segment < _segments.Count - 1)
        {
            _wait = Words.ReadTime(_segments[_segment]);
            _segment++;
            int next = _segment;
            Apply();
            SegmentBegan?.Invoke(next);
            return;
        }
        Apply();
        Done?.Invoke();
    }

    const string Pauses = ",;:—–、，；：";

    /// <summary>Characters a second, at the player's text speed (0: a sentence at once); half as many for a dense script.</summary>
    float Rate
    {
        get
        {
            float rate = Settings.TextSpeed switch
            {
                TextSpeed.Slow => 30,
                TextSpeed.Normal => 52,
                TextSpeed.Fast => 90,
                _ => 0,
            };
            return _plain.Any(ch => ch >= 0x3000) ? rate * 0.5f : rate;
        }
    }

    /// <summary>The breath between two sentences: the typing was most of the reading, so it's short, longer after a long one.</summary>
    static double Rest(string segment)
    {
        bool dense = segment.Any(c => c >= 0x3000);
        return Math.Clamp(0.22 + (dense ? 0.02 : 0.007) * segment.Length, 0.3, 0.75);
    }

    /// <summary>
    /// The speech begun most lately: when two are being typed at once (a seller's pitch, and Bram's word on it), only
    /// that one is heard, and the other is typed in silence until it's done.
    /// </summary>
    static WeakReference<Typewriter> _latest;

    void Blip()
    {
        if (!Settings.TextSounds) return;
        if (_latest != null && _latest.TryGetTarget(out var latest) && latest != this && latest.Typing && GodotObject.IsInstanceValid(latest._label) &&
            latest._label.IsVisibleInTree()) return;
        Main.Instance?.Sfx?.Play(_sound, _pitch * (0.96f + 0.08f * (float)_rng.NextDouble()), _volume);
    }

    /// <summary>Shows as much as has been typed: by the label's own count of characters, which the text's (the sentences joined) should match.</summary>
    void Apply()
    {
        if (!GodotObject.IsInstanceValid(_label)) return;
        if (_label is RichTextLabel rich)
        {
            int total = rich.GetTotalCharacterCount();
            rich.VisibleCharacters = _shown >= _plain.Length ? -1 : total == _plain.Length || _plain.Length == 0 ? _shown : (int)Math.Round(_shown * (double)total / _plain.Length);
        }
        else if (_label is Label plain) plain.VisibleCharacters = _shown >= _plain.Length ? -1 : _shown;
    }

    /// <summary>
    /// A speaker's voice: the pitch their blips are said at, the same every time for the same name. Bram's is the
    /// middle of it, Mister Vane's low, and everyone else somewhere between a low voice and a high one.
    /// </summary>
    public static float Voice(string speaker)
    {
        if (string.IsNullOrEmpty(speaker)) return 1f;
        if (speaker == Story.Bram) return 1f;
        if (speaker == Story.Vane) return 0.72f;
        int hash = 17;
        foreach (char c in speaker) hash = unchecked(hash * 31 + c);
        return 0.8f + (uint)hash % 13 * 0.05f;
    }
}

/// <summary>How fast speech is typed (Settings > Game): a sentence at once, or a character at a time.</summary>
public enum TextSpeed { AtOnce, Slow, Normal, Fast }
