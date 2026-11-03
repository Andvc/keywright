using System;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>A full figure across the counter, with their name and speech in the top-left bubble.</summary>
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
    Label _text;
    Panel _bubble;
    Painter _tail;

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
            Texture = GD.Load<Texture2D>($"res://assets/figures/{_seller.Look}.png"),
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
        _text = UiKit.MakeLabel("", 16, Palette.Text).Wrapped().Place(14, 40, BubbleW - 28, BubbleH - 48);
        _text.VerticalAlignment = VerticalAlignment.Center;
        _bubble.AddChild(_text);
        _tail = new Painter(p => p.DrawColoredPolygon(
            new[] { new Vector2(174, _bubble.Size.Y - 2), new Vector2(198, _bubble.Size.Y - 2), new Vector2(186, _bubble.Size.Y + 12) }, Palette.Text));
        _bubble.AddChild(_tail);
    }

    float _nameW;

    public void Say(string line, Color? color = null)
    {
        _text.Text = line;
        // Short replies leave the hats and faces of taller visitors visible below the bubble. Measured at full height,
        // lines and the spacing between them, with room for the frame under the last. A speech too long for the
        // bubble widens it first (keeping the speaker's face in view), and only then grows it: it never spills out.
        float width = Math.Max(BubbleW, _nameW + 32), height = Fit(width);
        if (height > BubbleH) height = Fit(width = Math.Max(WideW, width));
        _text.Size = new Vector2(width - 28, height - 48);
        UiKit.Resized(_text);
        _bubble.Size = new Vector2(width, height);
        _tail.QueueRedraw();
        _text.AddThemeColorOverride("font_color", color ?? Palette.Text);
        _bubble.PivotOffset = new Vector2(width / 2, 0);
        _bubble.Scale = new Vector2(0.96f, 0.9f);
        _bubble.CreateTween().TweenProperty(_bubble, "scale", Vector2.One, 0.12f).SetTrans(Tween.TransitionType.Back);
    }
    /// <summary>The bubble's height for the line at this width: at least a short one's, and room for every line.</summary>
    float Fit(float width)
    {
        _text.Size = new Vector2(width - 28, BubbleH - 48);
        int lines = _text.GetLineCount();
        float textHeight = lines * _text.GetLineHeight() + Math.Max(0, lines - 1) * _text.GetThemeConstant("line_spacing");
        return Math.Max(110, 48 + textHeight + 10);
    }

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
