using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>
/// A few lines between the people behind the story, over the scene's own backdrop (or the shop, dimmed): Bram on the
/// right, whoever he's talking to on the left, the one speaking lit and the other in shadow. Click (or Space) for the next
/// line; Skip jumps to the end. Then it hands on.
/// </summary>
public partial class ScenePhase : Phase
{
    const int FaceScale = 3;
    static readonly Vector2 Left = new(250, 300), Right = new(1030, 300);

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
    Label _name, _text, _more;
    int _index = -1;
    bool _done;

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
        Add(_backdrop == null ? UiKit.MakeShop(State.Tier, 0.3f) : UiKit.MakeBackdrop($"res://assets/backgrounds/scene_{_backdrop}.png", 0.6f));
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
        _text = UiKit.MakeLabel("", 16, Palette.Text).Wrapped().Place(24, 50, 952, 80);
        box.AddChild(_text);
        _more = UiKit.MakeLabel("", 16, Palette.Muted, HorizontalAlignment.Right).Place(600, 128, 380, 20);
        box.AddChild(_more);
        var click = new Button { Flat = true, FocusMode = Control.FocusModeEnum.None };
        click.Pressed += Advance;
        box.AddChild(click.Place(0, 0, 1000, 160));
        Add(UiKit.MakeButton("Skip", Palette.PanelBorder.Lightened(0.15f), 16, Finish).Place(1160, 670, 100, 34));
        Advance();
    }

    static Face MakeFace(string key, Vector2 at)
    {
        var face = new Face { Root = new Node2D { Position = at }, Portrait = new Sprite2D { Scale = Vector2.One * FaceScale } };
        face.Root.AddChild(UiKit.MakePanel(Palette.PanelBorder).Place(-128, -128, 256, 256));
        face.Root.AddChild(face.Portrait);
        face.Name = UiKit.MakeLabel("", 24, Palette.Gold, HorizontalAlignment.Center, outline: 4).Place(-200, 134, 400, 30);
        face.Root.AddChild(face.Name);
        Show(face, key);
        return face;
    }

    static void Show(Face face, string key)
    {
        face.Who = key;
        face.Portrait.Texture = GD.Load<Texture2D>($"res://assets/sprites/sellers/{key}.png");
        face.Name.Text = Story.Name(key);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Space or Key.Enter })
        {
            GetViewport().SetInputAsHandled();
            Advance();
        }
    }

    /// <summary>The next line, or on once they've all been said.</summary>
    public void Advance()
    {
        if (_done) return;
        if (++_index >= _lines.Count)
        {
            Finish();
            return;
        }
        var line = _lines[_index];
        Trace?.Invoke($"{Story.Name(line.Speaker)}: {line.Text}");
        _name.Text = Story.Name(line.Speaker);
        _text.Text = line.Text;
        _more.Text = _index < _lines.Count - 1 ? "click for more" : "click to go on";
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
        _then?.Invoke();
    }
}
