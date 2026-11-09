using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>
/// A few lines between the people behind the story, over the scene's own backdrop (or the shop, dimmed): Bram on the
/// right, whoever he's talking to on the left, the one speaking lit and the other in shadow. A line is said a sentence
/// at a time (0.35), the trade's words coloured, the speaker stirring with each; click (or Space) for the next sentence,
/// then the next line; Skip jumps to the end. Every line goes in the run's history. Then it hands on.
/// </summary>
public partial class ScenePhase : Phase
{
    public override bool HoldsNotes => true;

    const int FaceScale = 4;
    static readonly Vector2 Left = new(250, 688), Right = new(1030, 688);

    /// <summary>A face in the scene: who it shows now (Mara takes the Hooded Woman's place when she lowers her hood).</summary>
    sealed class Face
    {
        public Node2D Root;
        public Sprite2D Portrait;
        public Label Name;
        public string Who;
    }

    readonly List<Line> _lines;
    readonly string _title, _backdrop;
    readonly Action _then;
    readonly Dictionary<string, Face> _faces = new();
    Label _name, _more;
    RichTextLabel _text;
    Painter _pips;
    int _index = -1;
    bool _done;

    /// <summary>The line being said, cut into sentences, and the one showing.</summary>
    List<string> _sentences = new();
    int _at;

    /// <summary>The bot reads the lines through.</summary>
    public static Action<string> Trace;

    /// <param name="backdrop">A picture under res://assets/backgrounds/scene_*.png (bram, guild, keyhold, vault), or the shop.</param>
    public ScenePhase(List<Line> lines, Action then, string title = null, string backdrop = null)
    {
        _lines = lines;
        _then = then;
        _title = title;
        _backdrop = backdrop;
    }

    /// <summary>Whose place a speaker takes: Mara is the Hooded Woman, with her hood down.</summary>
    static string Place(string speaker) => speaker == Story.Mara ? Story.Hood : speaker;

    public ScenePhase() { }

    public bool Done => _done;

    public override void _Ready()
    {
        Add(_backdrop == null ? UiKit.MakeShop(State.Tier, 0.65f, ShopView.LastHour, State) : UiKit.MakeBackdrop($"res://assets/backgrounds/scene_{_backdrop}.png", 0.8f));
        if (_title != null)
            Add(UiKit.MakeLabel(_title, 32, Palette.Gold, HorizontalAlignment.Center, outline: 6).Place(0, 72, 1280, 40));

        // Bram stands on the right; everyone else takes the left, in the order they first speak.
        var places = _lines.Select(l => Place(l.Speaker)).Distinct().ToList();
        var others = places.Where(s => s != Story.Bram).ToList();
        foreach (string place in places)
        {
            var at = place == Story.Bram ? Right : Left + new Vector2(others.IndexOf(place) * 260, 0);
            var face = MakeFace(_lines.First(l => Place(l.Speaker) == place).Speaker, at);
            Add(face.Root);
            _faces[place] = face;
        }

        var box = Add(UiKit.MakePanel(Palette.Gold.Darkened(0.3f)).Place(140, 500, 1000, 160));
        _name = UiKit.MakeLabel("", 24, Palette.Gold, outline: 4).Place(24, 14, 600, 30);
        box.AddChild(_name);
        _text = Rich.Make(16, Palette.Text).Place(24, 50, 952, 80);
        box.AddChild(_text);
        // A pip a sentence, bottom left: which one of the line's is showing.
        _pips = new Painter(p =>
        {
            if (_sentences.Count < 2) return;
            for (int i = 0; i < _sentences.Count; i++)
            {
                var at = new Vector2(i * 12, 0);
                if (i == _at) p.DrawCircle(at, 4, Palette.Gold);
                else if (i < _at) p.DrawCircle(at, 3, Palette.Muted);
                else p.DrawArc(at, 3, 0, Mathf.Tau, 12, Palette.Muted, 1.5f);
            }
        }) { Position = new Vector2(30, 138) };
        box.AddChild(_pips);
        _more = UiKit.MakeLabel("", 16, Palette.Muted, HorizontalAlignment.Right).Place(600, 128, 380, 20);
        box.AddChild(_more);
        var click = new Button { Flat = true, FocusMode = Control.FocusModeEnum.None };
        click.Pressed += Advance;
        box.AddChild(click.Place(0, 0, 1000, 160));
        Add(UiKit.MakeButton(Loc.T("Skip"), Palette.PanelBorder.Lightened(0.15f), 16, Finish).Place(1160, 670, 100, 34));
        Advance();
    }

    static Face MakeFace(string key, Vector2 at)
    {
        var face = new Face { Root = new Node2D { Position = at }, Portrait = new Sprite2D
            { Scale = Vector2.One * FaceScale, Centered = false, Position = new Vector2(-192, -636), TextureFilter = CanvasItem.TextureFilterEnum.Nearest } };
        face.Root.AddChild(face.Portrait);
        // Just above the dialogue box (its top is at 500), not under it.
        face.Name = UiKit.MakeLabel("", 24, Palette.Gold, HorizontalAlignment.Center, outline: 4).Place(-200, -224, 400, 30);
        face.Root.AddChild(face.Name);
        Show(face, key);
        return face;
    }

    static void Show(Face face, string key)
    {
        face.Who = key;
        face.Portrait.Texture = ArtTexture.Load($"res://assets/figures/{Story.Look(key)}.png");
        UiKit.Outline(face.Portrait, whole: true);
        face.Portrait.Material = null;
        face.Name.Text = Story.Name(key);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed(Controls.Act))
        {
            GetViewport().SetInputAsHandled();
            Advance();
        }
    }

    /// <summary>The next sentence of the line; past its last, the next line; on once they've all been said.</summary>
    public void Advance()
    {
        if (_done) return;
        if (_index >= 0 && _at < _sentences.Count - 1)
        {
            _at++;
            ShowSentence(fresh: false);
            return;
        }
        if (++_index >= _lines.Count)
        {
            Finish();
            return;
        }
        var line = _lines[_index];
        Trace?.Invoke($"{Story.Name(line.Speaker)}: {line.Text}");
        State?.Hear(Story.Name(line.Speaker), line.Text, Heard.Dialogue);
        // A line that gives the case a clue notes it as it's said (the top bar shows it).
        if (line.Clue != null) State?.Find(line.Clue);
        _name.Text = Story.Name(line.Speaker);
        _sentences = Words.Sentences(line.Text);
        if (_sentences.Count == 0) _sentences.Add(line.Text);
        _at = 0;
        ShowSentence(fresh: true);
    }

    /// <summary>The sentence showing, coloured; the speaker lit (and stirring), the rest in shadow.</summary>
    void ShowSentence(bool fresh)
    {
        var line = _lines[_index];
        _text.Text = Rich.Trade(_sentences[_at]);
        _text.Modulate = new Color(1, 1, 1, 0);
        _text.CreateTween().TweenProperty(_text, "modulate:a", 1f, 0.16f);
        _more.Text = _at < _sentences.Count - 1 || _index < _lines.Count - 1 ? Loc.T("click for more") : Loc.T("click to go on");
        _pips.QueueRedraw();
        if (!fresh)
        {
            // Another sentence from the same mouth: a smaller stir.
            if (_faces.GetValueOrDefault(Place(line.Speaker)) is { } speaker)
            {
                speaker.Root.Scale = Vector2.One * 1.02f;
                speaker.Root.CreateTween().TweenProperty(speaker.Root, "scale", Vector2.One, 0.14f).SetTrans(Tween.TransitionType.Back);
            }
            Sfx.Play("tick", (line.Speaker == Story.Vane ? 0.7f : line.Speaker == Story.Bram ? 1f : 1.25f) + 0.03f * _at, -10);
            return;
        }
        foreach (var (place, face) in _faces)
        {
            bool speaking = place == Place(line.Speaker);
            face.Root.Modulate = speaking ? Colors.White : new Color(0.45f, 0.42f, 0.5f);
            if (!speaking) continue;
            if (face.Who != line.Speaker)
            {
                // The hood comes down.
                Show(face, line.Speaker);
                Game.Flash(Palette.Magic, 0.25f);
            }
            face.Root.Scale = Vector2.One * 1.04f;
            face.Root.CreateTween().TweenProperty(face.Root, "scale", Vector2.One, 0.18f).SetTrans(Tween.TransitionType.Back);
        }
        Sfx.Play("tick", line.Speaker == Story.Vane ? 0.7f : line.Speaker == Story.Bram ? 1f : 1.25f, -8);
        Game.Moment($"20_scene_{line.Speaker}", 0.4);
    }

    void Finish()
    {
        if (_done) return;
        _done = true;
        // Skipped lines were still said: into the history with the rest.
        if (State != null)
            for (int i = Math.Max(0, _index + 1); i < _lines.Count; i++) State.Hear(Story.Name(_lines[i].Speaker), _lines[i].Text, Heard.Dialogue);
        // Skipped lines still happened: their clues are noted too.
        if (State != null) Story.Heard(State, _lines);
        _then?.Invoke();
    }
}
