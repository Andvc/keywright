using System;
using System.Collections.Generic;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// Uncle Bram at your elbow: a face and one short tip at a time about what's in front of you. When two tips come up
/// at once he gives one (<see cref="Teach"/>); the other isn't marked as taught, so it comes up the next time it
/// applies. A seller's lesson waits until the tip already up has been read; then the box shows the step to do and
/// stays up until it's done (or skipped). Since 0.35 a tip is said a sentence at a time (a click for the next), the
/// trade's words coloured (<see cref="Rich"/>), a lesson step's action named in a tab over the box, and every tip goes
/// in the run's history (<see cref="GameState.Hear"/>).
/// </summary>
public partial class MentorView : Control
{
    public const float W = 456, H = 128;
    /// <summary>The highest the box's top goes: just under the top bar.</summary>
    const float TopLimit = 60;
    readonly Queue<string> _queue = new();
    RichTextLabel _text;
    Label _more;
    Button _skip;
    Panel _panel;

    /// <summary>The text up now, cut into sentences, and the one showing.</summary>
    List<string> _sentences = new();
    int _at;

    /// <summary>What a lesson step asks you to do, in a tab over the box (0.35).</summary>
    Control _doTab;
    RichTextLabel _doText;

    /// <summary>Bram's face (or his notebook), which nods when a step says what to do.</summary>
    Node2D _face;

    /// <summary>The page pips under the text: how many sentences, and which one.</summary>
    Painter _pips;

    /// <summary>Where the box's bottom edge sits: a long tip grows it upward from here.</summary>
    float _bottom;
    bool _lesson, _waiting, _showingTip;

    /// <summary>A lesson step waiting for the tips in front of it to be read.</summary>
    (string Text, bool Waiting, string Action)? _held;

    public bool Showing => Visible;

    /// <summary>
    /// A lesson step is up that's done by doing something, and it's been read to its end (a seller's clock, Tick's
    /// hourglass, runs from then; the step can be done at any sentence).
    /// </summary>
    public bool WaitingForAction => Visible && _lesson && _waiting && ReadThrough;

    /// <summary>The last sentence of what's up is showing.</summary>
    bool ReadThrough => _at >= _sentences.Count - 1;

    /// <summary>Something's up with sentences still to come (a talk lets Bram finish before it goes on).</summary>
    public bool MidText => Visible && !ReadThrough;

    /// <summary>The player clicked on past a lesson step that's just for reading.</summary>
    public event Action LessonRead;

    /// <summary>The player skipped the lesson.</summary>
    public event Action LessonSkipped;

    /// <summary>Set by the autoplay bot: every message Bram shows, for reading the tutorial through.</summary>
    public static Action<string> Trace;

    public override void _Ready()
    {
        Size = new Vector2(W, H);
        _bottom = Position.Y + H;
        MouseFilter = MouseFilterEnum.Stop;
        PivotOffset = new Vector2(0, H);
        _panel = UiKit.MakePanel(Palette.Gold.Darkened(0.25f)).Place(0, 0, W, H);
        _panel.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_panel);
        // Bram's away at the treasury (0.34): his tips are the notes he left in the margins of his notebook.
        if (Main.Instance?.State?.BramNotes == true)
        {
            AddChild(_face = new Painter(p =>
            {
                p.DrawRect(new Rect2(24, 10, 56, 60), Palette.Paper);
                p.DrawRect(new Rect2(24, 10, 56, 60), Palette.Ink, false, 2);
                for (int line = 0; line < 5; line++) p.DrawRect(new Rect2(31, 20 + line * 10, line % 3 == 2 ? 26 : 42, 2), Palette.Ink.Lightened(0.35f));
            }));
            // Two lines if it needs them (a language's words for it can run long).
            AddChild(UiKit.MakeLabel(Loc.T("BRAM'S NOTES"), 16, Palette.Gold, HorizontalAlignment.Center).Wrapped().Place(4, 74, 96, 44));
        }
        else
        {
            var bram = new Sprite2D
            {
                Texture = ArtTexture.Load("res://assets/sprites/sellers/bram.png"),
                Position = new Vector2(52, 58),
            };
            AddChild(UiKit.Outline(bram));
            _face = bram;
            AddChild(UiKit.MakeLabel(Hints.Mentor.ToUpperInvariant(), 16, Palette.Gold, HorizontalAlignment.Center).Place(4, 98, 96, 20));
        }
        _text = Rich.Make(16, Palette.Text).Place(104, 8, W - 116, H - 30);
        _text.VerticalAlignment = VerticalAlignment.Center;
        AddChild(_text);
        _pips = new Painter(DrawPips) { Position = new Vector2(104, H - 16) };
        AddChild(_pips);
        _more = UiKit.MakeLabel("", 16, Palette.Gold, HorizontalAlignment.Right).Place(104, H - 26, W - 116, 20);
        AddChild(_more);
        string skip = Loc.T("Skip lesson");
        float skipW = Mathf.Max(132, UiKit.TextWidth(skip) + 24);
        _skip = UiKit.MakeButton(skip, Palette.Muted, 16, () => LessonSkipped?.Invoke()).Place(W - skipW, -30, skipW, 30);
        _skip.Visible = false;
        AddChild(_skip);
        // The action a lesson step asks for, over the box's left: gold, breathing gently, until the step's done.
        _doTab = UiKit.MakePanel(Palette.Gold.Darkened(0.2f)).Place(0, -34, W - skipW - 8, 32);
        _doTab.MouseFilter = MouseFilterEnum.Ignore;
        _doText = Rich.Make(16, Palette.Gold).Place(10, 6, W - skipW - 28, 22);
        _doTab.AddChild(_doText);
        _doTab.Visible = false;
        AddChild(_doTab);
        var breathe = _doTab.CreateTween().SetLoops();
        breathe.TweenProperty(_doTab, "modulate:a", 0.72f, 0.8f).SetTrans(Tween.TransitionType.Sine);
        breathe.TweenProperty(_doTab, "modulate:a", 1f, 0.8f).SetTrans(Tween.TransitionType.Sine);
        GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) Next();
        };
        Visible = false;
    }

    /// <summary>Bram has something up, or waiting to be: a tip, a line, or a lesson.</summary>
    public bool Busy => Visible || _lesson || _queue.Count > 0 || _held != null;

    /// <summary>
    /// Bram's tip for <paramref name="key"/> the first time it applies, if he isn't already saying something: one at a
    /// time. A tip he can't give now isn't marked as taught, so it comes up again the next time it applies. True if he
    /// gave it.
    /// </summary>
    public bool Teach(GameState state, string key)
    {
        if (key == null || Busy || Hints.Get(key) == null || !state.FirstTime(key)) return false;
        Say(Controls.Fill(Hints.Get(key)));
        // Taught for good, at once: quitting before the day's out doesn't bring it back.
        Main.Instance.NoteTaught();
        return true;
    }

    /// <summary>Shows a line now, or after the ones already waiting (and after any lesson).</summary>
    public void Say(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (!Visible && !_lesson)
        {
            Show(text);
            return;
        }
        _queue.Enqueue(text);
        UpdateMore();
    }

    /// <summary>
    /// Shows a lesson step: one to do (it stays up until it's done; <paramref name="action"/> says what, in the tab over
    /// the box) or one to read (click on). If a tip is still up, the step waits until it (and any after it) has been read.
    /// </summary>
    public void ShowLesson(string text, bool waiting, string action = null)
    {
        if (!_lesson && Visible)
        {
            _held = (text, waiting, action);
            UpdateMore();
            return;
        }
        Begin(text, waiting, action);
    }

    void Begin(string text, bool waiting, string action)
    {
        _held = null;
        _lesson = true;
        _waiting = waiting;
        _skip.Visible = true;
        _doTab.Visible = action != null;
        if (action != null) _doText.Text = Rich.Colour(action, Palette.Gold);
        Show(text);
    }

    /// <summary>The lesson's over: back to tips.</summary>
    public void EndLesson()
    {
        // Over before it began: the tips in front of it carry on as they were.
        if (_held != null)
        {
            _held = null;
            UpdateMore();
        }
        if (!_lesson) return;
        _lesson = false;
        _skip.Visible = false;
        _doTab.Visible = false;
        if (_queue.Count > 0) Show(_queue.Dequeue());
        else Visible = false;
    }

    /// <summary>
    /// The next sentence of what's up; past the last, the next waiting tip, or the box put away. A lesson step to do
    /// stays up.
    /// </summary>
    public void Next()
    {
        if (!ReadThrough)
        {
            _at++;
            ShowSentence();
            return;
        }
        if (_lesson)
        {
            if (!_waiting) LessonRead?.Invoke();
            return;
        }
        if (_queue.Count > 0) Show(_queue.Dequeue(), afterClick: true);
        else if (_held is var (text, waiting, action))
        {
            _showingTip = false;
            Begin(text, waiting, action);
        }
        else Visible = false;
    }

    void Show(string text, bool afterClick = false)
    {
        // A tip still up and unread when something else replaces it would never be seen: say so in the trace.
        if (Visible && _showingTip && !afterClick) Trace?.Invoke($"(replaced unread: {_text.Text})");
        Trace?.Invoke((_lesson ? (_waiting ? Loc.T("[do] ") : Loc.T("[read] ")) : "") + text);
        _showingTip = !_lesson;
        Visible = true;
        // Into the run's history whole, said a sentence at a time.
        Main.Instance?.State?.Hear(Hints.Mentor, text, _lesson ? Heard.Lesson : Heard.Tip);
        _sentences = Words.Sentences(text);
        if (_sentences.Count == 0) _sentences.Add(text);
        _at = 0;
        Scale = new Vector2(0.96f, 0.9f);
        CreateTween().TweenProperty(this, "scale", Vector2.One, 0.14f).SetTrans(Tween.TransitionType.Back);
        ShowSentence();
    }

    /// <summary>The sentence at <see cref="_at"/>, the trade's words coloured; the box fitted to it.</summary>
    void ShowSentence()
    {
        string sentence = _sentences[_at];
        _text.Text = Rich.Trade(sentence);
        Fit(sentence);
        UpdateMore();
        _pips.QueueRedraw();
        // Each sentence comes up a little, like a word said; the step's instruction (its last) with a nod from Bram.
        _text.Modulate = new Color(1, 1, 1, 0);
        _text.CreateTween().TweenProperty(_text, "modulate:a", 1f, 0.18f);
        if (_lesson && _waiting && ReadThrough) Nod();
        Main.Instance.Sfx.Play("blip", 0.8f + 0.04f * _at, -10);
    }

    /// <summary>Bram's face (or his notebook) dips and comes back: a nod at what to do.</summary>
    void Nod()
    {
        if (_face == null) return;
        var home = _face.Position;
        var nod = _face.CreateTween();
        nod.TweenProperty(_face, "position:y", home.Y + 4, 0.12f).SetTrans(Tween.TransitionType.Sine);
        nod.TweenProperty(_face, "position:y", home.Y, 0.18f).SetTrans(Tween.TransitionType.Sine);
        nod.TweenProperty(_face, "position:y", home.Y + 3, 0.12f).SetTrans(Tween.TransitionType.Sine);
        nod.TweenProperty(_face, "position:y", home.Y, 0.18f).SetTrans(Tween.TransitionType.Sine);
    }

    /// <summary>A pip a sentence, under the text: the ones read filled, the one showing gold. None for a single sentence.</summary>
    void DrawPips(Painter p)
    {
        if (_sentences.Count < 2) return;
        for (int i = 0; i < _sentences.Count; i++)
        {
            var at = new Vector2(i * 12 + 4, 0);
            if (i == _at) p.DrawCircle(at, 4, Palette.Gold);
            else if (i < _at) p.DrawCircle(at, 3, Palette.Muted);
            else p.DrawArc(at, 3, 0, Mathf.Tau, 12, Palette.Muted, 1.5f);
        }
    }

    /// <summary>
    /// Grows the box upward (its bottom edge stays put) when the text needs more than four lines: as many lines as the
    /// label itself breaks it into (a font's own measure breaks a little later, and a longer language shows it).
    /// </summary>
    void Fit(string sentence)
    {
        // Broken into lines the way the label breaks them (at words, a few pixels inside its width), then measured.
        var paragraph = new TextParagraph
        {
            Width = W - 116 - 6,
            BreakFlags = TextServer.LineBreakFlag.Mandatory | TextServer.LineBreakFlag.WordBound | TextServer.LineBreakFlag.Adaptive,
        };
        int size = UiKit.SnapFont(16);
        paragraph.AddString(sentence, UiKit.Font, size);
        int lines = Mathf.Max(1, paragraph.GetLineCount());
        float textHeight = lines * (UiKit.Font.GetHeight(size) + _text.GetThemeConstant("line_spacing"));
        float h = Mathf.Max(H, Mathf.Ceil(textHeight) + 48);
        Size = new Vector2(W, h);
        // Never up under the top bar (a long tip in French at the opening's top right): past it, the box grows down.
        Position = new Vector2(Position.X, Mathf.Max(_bottom - h, TopLimit));
        PivotOffset = new Vector2(0, h);
        _panel.Size = new Vector2(W, h);
        _text.Size = new Vector2(W - 116, h - 30);
        UiKit.Resized(_text);
        _more.Position = new Vector2(104, h - 26);
        _pips.Position = new Vector2(104, h - 16);
    }

    void UpdateMore()
    {
        int more = _queue.Count + (_held != null ? 1 : 0);
        _more.Text = !ReadThrough ? Loc.T("click to go on")
            : _lesson ? (_waiting ? Loc.T("do it to go on") : Loc.T("click to go on"))
            : more > 0 ? Loc.T($"click for more ({more})") : Loc.T("click to close");
    }
}
