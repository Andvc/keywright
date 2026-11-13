using System;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>
/// The road inward (0.36): the morning the shop opens at a new market, the season's road with its four stops and the
/// Gate at its end, the stall's cart rolling in from the last one, and what this market brings. A due paid moves the
/// shop (<see cref="GameState.MoveInward"/>), so this is what paying it bought: the week's high point, and a look at how
/// far there is to go. A click (or the act key) opens the shop.
/// </summary>
public partial class RoadPhase : Phase
{
    public override bool HoldsNotes => true;

    const float CardW = 200, CardH = 112, Gap = 56, GateW = 72, RoadY = 404, CardsY = 252;
    static readonly float X0 = (1280 - (ShopTiers.Max * CardW + ShopTiers.Max * Gap + GateW)) / 2;

    readonly Action _then;
    Node2D _cart;
    bool _done;

    public RoadPhase(Action then) => _then = then;

    public RoadPhase() { }

    /// <summary>The middle of a stop's picture (the Gate is the one past the last market).</summary>
    static float StopX(int tier) => tier > ShopTiers.Max ? X0 + ShopTiers.Max * (CardW + Gap) + GateW / 2 : X0 + (tier - 1) * (CardW + Gap) + CardW / 2;

    public override void _Ready()
    {
        int tier = State.Tier;
        Add(UiKit.MakeShop(tier, 0.32f, ShopHour.Morning, State));
        Add(UiKit.MakeLabel(State.Week <= Guild.Weeks ? Loc.T($"WEEK {State.Week} OF {Guild.Weeks}") : Loc.T($"WEEK {State.Week}"), 16, Palette.Muted,
            HorizontalAlignment.Center, outline: 4).Place(0, 96, 1280, 20));
        var title = Add(UiKit.MakeLabel(ShopTiers.Name(tier).ToUpperInvariant(), 48, Palette.Gold, HorizontalAlignment.Center, outline: 6).Place(0, 122, 1280, 60));
        title.PivotOffset = new Vector2(640, 30);

        // The road under the pictures: walked as far as here in gold, the rest still to come.
        Add(new Painter(p =>
        {
            for (float x = StopX(1); x < StopX(ShopTiers.Max + 1); x += 12)
                p.DrawRect(new Rect2(x, RoadY - 2, 6, 4), x <= StopX(tier) ? Palette.Gold.Darkened(0.15f) : Palette.PanelBorder.Lightened(0.1f));
            for (int stop = 1; stop <= ShopTiers.Max + 1; stop++)
            {
                var at = new Vector2(StopX(stop), RoadY);
                bool reached = stop <= tier;
                p.DrawCircle(at, 9, new Color("#120e1a"));
                p.DrawCircle(at, 6, reached ? Palette.Gold : Palette.PanelBorder.Lightened(0.15f));
            }
        }));
        for (int stop = 1; stop <= ShopTiers.Max; stop++) Add(Postcard(stop, tier));
        Add(Gate(tier));

        // The stall's cart, rolling in from the market it left.
        _cart = Add(new Painter(DrawCart) { Position = new Vector2(StopX(Math.Max(1, tier - 1)), RoadY - 12), ZIndex = 5 });
        var roll = _cart.CreateTween();
        roll.TweenInterval(0.45);
        roll.TweenProperty(_cart, "position:x", StopX(tier), 1.3f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        roll.TweenCallback(Callable.From(() =>
        {
            if (!Alive) return;
            Sfx.Play("fanfare", 1f, -6);
            Fx.Burst(this, new Vector2(StopX(tier), RoadY - 8), Palette.Gold, 18, 160f);
            title.Scale = Vector2.One * 1.12f;
            title.CreateTween().TweenProperty(title, "scale", Vector2.One, 0.3f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        }));
        Sfx.PlayGroup("door_open", 0.9f, -8);

        // What this market brings, and what the Guild wants here.
        // Each as tall as its words (a market's blurb runs to three lines in French), the one under the other.
        const float textX = 200, textW = 880;
        float y = 452;
        if (Upgrades.MoveInto(tier) is Upgrade move)
        {
            float h = Mathf.Max(22, UiKit.TextHeight(move.Blurb, textW));
            Add(UiKit.MakeLabel(move.Blurb, 16, Palette.Text, HorizontalAlignment.Center, outline: 4).Wrapped().Place(textX, y, textW, h));
            y += h + 8;
        }
        if (State.GuildIntroduced && !State.DebtCleared)
        {
            int due = State.DueNow;
            string onward = State.NextMarket > 0
                ? Loc.T($"The Guild's due here: {due}g, on the fifth night. Paid, the road goes on to {ShopTiers.Name(State.NextMarket)}.")
                : Loc.T($"The last of Bram's debt: {due}g, on the fifth night.");
            Add(UiKit.MakeLabel(onward, 16, Palette.Warn, HorizontalAlignment.Center, outline: 4).Wrapped()
                .Place(textX, y, textW, Mathf.Max(22, UiKit.TextHeight(onward, textW))));
        }
        Add(UiKit.MakeButton(Loc.T("Open the shop"), Palette.Gold, 24, Finish).Place(480, 584, 320, 64));
        Game.Moment($"00_road_{tier}", 2.2);
    }

    /// <summary>A market's picture on the road: where you've been, where you are (framed in gold) and where you're going (in shadow).</summary>
    Control Postcard(int stop, int tier)
    {
        float x = StopX(stop) - CardW / 2;
        var frame = new Control { ClipContents = true, MouseFilter = Control.MouseFilterEnum.Ignore }.Place(x, CardsY, CardW, CardH);
        float scale = CardW / 320f;
        foreach (string layer in new[] { "sky/morning", $"place/{ShopView.Places[stop - 1]}" })
            frame.AddChild(new Sprite2D
            {
                Texture = ArtTexture.Load($"res://assets/{layer}.png"), Centered = false, Scale = Vector2.One * scale,
                TextureFilter = TextureFilterEnum.Linear,
            });
        var card = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        card.AddChild(frame);
        bool here = stop == tier, ahead = stop > tier;
        frame.Modulate = ahead ? new Color(0.28f, 0.26f, 0.36f) : here ? Colors.White : new Color(0.72f, 0.7f, 0.74f);
        card.AddChild(new Painter(p => p.DrawRect(new Rect2(x - 2, CardsY - 2, CardW + 4, CardH + 4), here ? Palette.Gold : Palette.PanelBorder, false, here ? 4 : 2)));
        card.AddChild(UiKit.MakeLabel(ShopTiers.Name(stop), 16, here ? Palette.Gold : ahead ? Palette.Muted : Palette.Text, HorizontalAlignment.Center, outline: 4)
            .Place(x - 20, CardsY - 28, CardW + 40, 20));
        return card;
    }

    /// <summary>The Gilded Gate, at the road's end: the Guild's own mark, dark until the stall stands beside it.</summary>
    Control Gate(int tier)
    {
        float x = StopX(ShopTiers.Max + 1);
        var gate = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        bool near = tier >= ShopTiers.Max;
        gate.AddChild(new Painter(p =>
        {
            p.DrawCircle(new Vector2(x, CardsY + CardH / 2), GateW / 2, new Color("#120e1a"));
            p.DrawArc(new Vector2(x, CardsY + CardH / 2), GateW / 2, 0, Mathf.Tau, 40, near ? Palette.Gold : Palette.PanelBorder, near ? 3 : 2);
        }));
        var mark = new TextureRect { Texture = ArtTexture.Load("res://assets/ui/guild.png"), MouseFilter = Control.MouseFilterEnum.Ignore }
            .Place(x - 22, CardsY + CardH / 2 - 24, 44, 48);
        mark.Modulate = near ? Colors.White : new Color(0.4f, 0.38f, 0.48f);
        gate.AddChild(mark);
        gate.AddChild(UiKit.MakeLabel(Loc.T("The Gilded Gate"), 16, near ? Palette.Gold : Palette.Muted, HorizontalAlignment.Center, outline: 4)
            .Place(x - 90, CardsY - 28, 180, 20));
        return gate;
    }

    /// <summary>The stall on its cart: an awning over a box, two wheels (three pixels a cell, like the rest).</summary>
    static void DrawCart(Painter p)
    {
        const float c = 3;
        void Cell(float x, float y, float w, float h, Color color) => p.DrawRect(new Rect2(x * c, y * c, w * c, h * c), color);
        Cell(-7, -9, 14, 2, Palette.Bad);
        for (int i = 0; i < 3; i++) Cell(-5 + i * 4, -9, 2, 2, Palette.Text);
        Cell(-6, -7, 1, 5, new Color("#6e3d22"));
        Cell(5, -7, 1, 5, new Color("#6e3d22"));
        Cell(-7, -2, 14, 4, new Color("#8e5b37"));
        Cell(-7, -2, 14, 1, new Color("#ad7d52"));
        foreach (float wheel in new[] { -4.5f, 4.5f })
        {
            p.DrawCircle(new Vector2(wheel * c, 3 * c), 2.2f * c, new Color("#2b2236"));
            p.DrawCircle(new Vector2(wheel * c, 3 * c), 0.9f * c, new Color("#766d8b"));
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed(Controls.Act) && e is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) return;
        GetViewport().SetInputAsHandled();
        Finish();
    }

    public void Finish()
    {
        if (_done) return;
        _done = true;
        Sfx.Play("bid");
        _then?.Invoke();
    }
}
