using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// A full figure across the counter, with their name and speech in the top-left bubble. A speech is typed out a
/// character at a time in the speaker's own voice (0.37, <see cref="Typewriter"/>: the bubble sized for all of it, a
/// sentence after another; a click shows the rest), the trade's words coloured, and whatever the counter marks (the
/// price, what they state) too, with a stir of the figure as its sentence begins. What they say goes in the run's history.
/// </summary>
public partial class SellerView : Node2D
{
    public const float BubbleW = 288, BubbleH = 164;

    /// <summary>How wide the bubble gets for a long speech (a tale's news) before it grows any taller.</summary>
    const float WideW = 420;
    readonly SellerDef _seller;
    Node2D _body;
    public Node2D Figure => _body;
    public event Action Clicked;
    public bool Clickable { get; init; }
    public Vector2 BubbleAt { get; init; } = new(12, 66);
    RichTextLabel _text;
    Panel _bubble;
    Painter _tail;

    /// <summary>The speech's sentences, its colour, the phrases to colour besides the trade's words, and what types it out.</summary>
    List<string> _sentences = new();
    Color _color = Palette.Text;
    (string, Color)[] _marks = Array.Empty<(string, Color)>();
    Typewriter _type;

    /// <summary>Phrases the next speech colours (the counter sets the price and what the seller states).</summary>
    public (string Phrase, Color Color)[] Marks { get; set; } = Array.Empty<(string, Color)>();

    public SellerView() { }
    public SellerView(SellerDef seller) => _seller = seller;

    public override void _Ready()
    {
        int height = ShopView.FigureHeight(_seller.Look);
        // Tick the imp (in Vexis's figure, until the new cast is drawn) stands in front of the counter.
        bool imp = _seller.Look == "vexis";
        int lift = Math.Max(0, 108 - height);
        var origin = new Vector2(16, imp ? 476 - 159 * 4 : (28 - lift) * 4);
        _body = new Node2D { ZIndex = imp ? -20 : -80 };
        AddChild(_body);
        if (lift > 0 && !imp)
            _body.AddChild(new Painter(p =>
            {
                p.DrawRect(new Rect2(96, 748 - lift * 4, 208, lift * 4), Palette.Wood);
                p.DrawRect(new Rect2(96, 748 - lift * 4, 208, 12), Palette.WoodLight);
            }));
        var figure = UiKit.Outline(new Sprite2D
        {
            Texture = ArtTexture.Load($"res://assets/figures/{_seller.Look}.png"),
            Centered = false, Position = origin, Scale = Vector2.One * 4,
            TextureFilter = TextureFilterEnum.Nearest,
        }, whole: true);
        // A ghost (Sir Aldwin): pale, cold and half there.
        if (_seller.Ghost) figure.Modulate = new Color(0.72f, 0.86f, 1f, 0.55f);
        _body.AddChild(figure);
        if (Clickable)
        {
            var face = new Button
            {
                Flat = true, FocusMode = Control.FocusModeEnum.None,
                MouseDefaultCursorShape = Control.CursorShape.PointingHand,
                TooltipText = Loc.T($"{_seller.Name}: click to read their page in your notebook."),
            };
            face.Pressed += () => Clicked?.Invoke();
            float top = Math.Max(216, origin.Y + (159 - height) * 4);
            _body.AddChild(face.Place(80, top, 244, Math.Max(40, 476 - top)));
        }
        _bubble = UiKit.MakePanel(Palette.Text);
        _bubble.Place(BubbleAt.X, BubbleAt.Y, BubbleW, BubbleH);
        _bubble.ZIndex = 20;
        AddChild(_bubble);
        // A long name (Archmage Thessaly Orme, or anyone in a language that writes names longer) widens the bubble.
        _nameW = UiKit.TextWidth(_seller.Name, 24);
        _bubble.AddChild(UiKit.MakeLabel(_seller.Name, 24, Palette.Gold).Place(14, 8, Math.Max(BubbleW - 28, _nameW + 4), 28));
        _text = Rich.Make(16, Palette.Text).Place(14, 40, BubbleW - 28, BubbleH - 48);
        _text.VerticalAlignment = VerticalAlignment.Center;
        _bubble.AddChild(_text);
        // A click on the bubble: the rest of the speech at once.
        _bubble.MouseFilter = Control.MouseFilterEnum.Stop;
        _bubble.GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } && MidSpeech) FinishSpeech();
        };
        _type = new Typewriter(_text);
        _type.SegmentBegan += Began;
        _tail = new Painter(p => p.DrawColoredPolygon(
            new[] { new Vector2(174, _bubble.Size.Y - 2), new Vector2(198, _bubble.Size.Y - 2), new Vector2(186, _bubble.Size.Y + 12) }, Palette.Text));
        _bubble.AddChild(_tail);
    }

    float _nameW;

    /// <summary>
    /// Says a line: typed out a character at a time, a sentence after another (a click on the bubble shows the rest).
    /// <paramref name="record"/>: into the run's history (a talk notes its whole lines itself).
    /// </summary>
    public void Say(string line, Color? color = null, bool record = true)
    {
        if (record) Main.Instance?.State?.Hear(_seller.Name, line, Heard.Seller);
        _sentences = Words.Sentences(line);
        if (_sentences.Count == 0) _sentences.Add(line ?? "");
        _color = color ?? Palette.Text;
        _marks = Marks;
        Marks = Array.Empty<(string, Color)>();
        // Short replies leave the hats and faces of taller visitors visible below the bubble. Measured at full height
        // for the whole speech (it doesn't grow as it's said), with room for the frame under the last line. A speech
        // too long for the bubble widens it first (keeping the speaker's face in view), and only then grows it.
        float width = Math.Max(BubbleW, _nameW + 32), height = Fit(line, width);
        if (height > BubbleH) height = Fit(line, width = Math.Max(WideW, width));
        _text.Size = new Vector2(width - 28, height - 48);
        UiKit.Resized(_text);
        _bubble.Size = new Vector2(width, height);
        _tail.QueueRedraw();
        _bubble.PivotOffset = new Vector2(width / 2, 0);
        _bubble.Scale = new Vector2(0.96f, 0.9f);
        _bubble.CreateTween().TweenProperty(_bubble, "scale", Vector2.One, 0.12f).SetTrans(Tween.TransitionType.Back);
        // Their price and what they state, as marked, and how hard they shook, if they say. The whole speech is laid
        // out at once (nothing moves as it's typed) and typed onto it in their voice.
        var marks = Rich.Strengths.Concat(_marks).ToList();
        string whole = string.Join(" ", _sentences.Select(s => Rich.Mark(s, marks)));
        _type.Begin(_sentences, $"[color=#{_color.ToHtml(false)}]{whole}[/color]", Typewriter.Voice(_seller.Key));
    }

    /// <summary>A sentence with something marked in it (the price, what they state): the speaker stirs as they begin it.</summary>
    void Began(int segment)
    {
        if (segment < _sentences.Count && _marks.Length > 0 && _marks.Any(m => _sentences[segment].Contains(m.Item1, StringComparison.OrdinalIgnoreCase)))
            Bob(3);
    }

    /// <summary>Still saying it: there's more of the speech to come.</summary>
    public bool MidSpeech => _type?.Typing ?? false;

    /// <summary>The rest of the speech at once (a click, or the act key).</summary>
    public void FinishSpeech() => _type?.Finish();

    public override void _Process(double delta) => _type?.Process(delta);

    /// <summary>The bubble's height for the whole speech at this width: at least a short one's, and room for every line.</summary>
    static float Fit(string line, float width) => Math.Max(110, 48 + UiKit.TextHeight(line ?? "", width - 28 - 6) + 10);

    public void Enter()
    {
        _body.Position = new Vector2(-420, 0);
        _body.CreateTween().TweenProperty(_body, "position:x", 0f, 0.35f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    }
    public Tween Leave()
    {
        _bubble.Hide();
        var tween = _body.CreateTween();
        tween.TweenProperty(_body, "position:x", -460f, 0.3f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
        return tween;
    }
    public void Bob(float amount = 6)
    {
        var tween = _body.CreateTween();
        tween.TweenProperty(_body, "position:y", -amount, 0.06f);
        tween.TweenProperty(_body, "position:y", amount * 0.5f, 0.08f);
        tween.TweenProperty(_body, "position:y", 0f, 0.08f);
    }
}
