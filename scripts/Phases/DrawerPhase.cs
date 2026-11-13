using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>
/// Bram's drawer (0.37): the first morning of a season after the first, before the shop opens. Three plain curios he's
/// kept, one for each of three ways of trading (<see cref="Curios.Drawer"/>); one of them is yours for nothing, so a
/// build begins on the first day and no two seasons begin alike. A first season never sees it: Nim brings its first
/// curio on the third night, with his own introduction.
/// </summary>
public partial class DrawerPhase : Phase
{
    public override bool HoldsNotes => true;

    const float CardW = 260, CardH = 384, CardGap = 28, CardsY = 190, NameY = 96;

    readonly Action _then;
    readonly List<CurioDef> _cards = new();
    readonly List<Control> _panels = new();
    bool _done;

    public DrawerPhase(Action then) => _then = then;

    public DrawerPhase() { }

    /// <summary>What's in the drawer (the bot chooses through this).</summary>
    public IReadOnlyList<CurioDef> Cards => _cards;

    public bool Done => _done;

    public override void _Ready()
    {
        Add(UiKit.MakeShop(State.Tier, 0.4f, ShopHour.Morning, State));
        Add(UiKit.MakeLabel(Loc.T("BRAM'S DRAWER"), 32, Palette.Gold, HorizontalAlignment.Center, outline: 6).Place(0, 78, 1280, 40));
        string word = State.BramHere
            ? Loc.T($"\"Three things I've kept from the old days, {State.Kin.Name}. Take one for the season: each suits a different way of trading.\"")
            : Loc.T("Three things Bram kept from the old days. Take one for the season: each suits a different way of trading.");
        Add(UiKit.MakeLabel(word, 16, Palette.Text, HorizontalAlignment.Center, outline: 4).Wrapped().Place(190, 126, 900, 44));

        _cards.AddRange(Curios.Drawer(State));
        float x0 = (1280 - (_cards.Count * (CardW + CardGap) - CardGap)) / 2;
        for (int i = 0; i < _cards.Count; i++)
        {
            var card = MakeCard(_cards[i], x0 + i * (CardW + CardGap), CardsY);
            _panels.Add(card);
            Add(card);
            // Each comes up out of the drawer in turn.
            card.Modulate = Colors.Transparent;
            card.Position += new Vector2(0, 26);
            var rise = card.CreateTween().SetParallel();
            rise.TweenProperty(card, "modulate", Colors.White, 0.22f).SetDelay(0.15f + 0.12f * i);
            rise.TweenProperty(card, "position:y", CardsY, 0.3f).SetDelay(0.15f + 0.12f * i).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        }
        Add(UiKit.MakeButton(Loc.T("Take nothing"), Palette.PanelBorder.Lightened(0.15f), 16, () => Take(null)).Place(1080, 648, 170, 40));
        Sfx.PlayGroup("door_open", 1.1f, -8);
        Game.Moment("23_drawer", 0.9);
    }

    Control MakeCard(CurioDef curio, float x, float y)
    {
        var color = Palette.Of(curio.Rarity);
        var card = UiKit.MakePanel(color).Place(x, y, CardW, CardH);
        card.AddChild(new Painter(p =>
        {
            for (int i = 3; i >= 1; i--) p.DrawCircle(new Vector2(CardW / 2, 62), 20 + i * 10, color with { A = 0.08f });
        }));
        card.AddChild(UiKit.MakeCurioIcon(curio, new Vector2(CardW / 2, 62)));
        var (nameW, nameH, nameSize) = PeddlerPhase.NameBox(curio.Name, narrow: false);
        card.AddChild(UiKit.MakeLabel(curio.Name, nameSize, Palette.Text, HorizontalAlignment.Center, outline: 4).Wrapped().Place(10, NameY, nameW, nameH));
        card.AddChild(UiKit.MakeLabel(Curios.RarityName(curio.Rarity), 16, color, HorizontalAlignment.Right).Place(CardW - 110, 8, 100, 20));
        string tags = Curios.Tags(curio);
        if (UiKit.TextWidth(tags) > CardW - 8) tags = Curios.TagName(curio.Tags[0], curio);
        card.AddChild(UiKit.MakeLabel(tags, 16, color, HorizontalAlignment.Center).Place(0, NameY + nameH, CardW, 20));
        var (blurbW, blurbH) = PeddlerPhase.BlurbBox(false, nameH);
        var blurb = UiKit.MakeLabel(curio.Blurb, 16, Palette.Muted, HorizontalAlignment.Center).Wrapped().Place(14, NameY + 22 + nameH, blurbW, blurbH);
        card.AddChild(blurb);
        if (Glossary.For(curio).Count > 0)
        {
            string explain = Glossary.Explain(curio);
            var badge = UiKit.MakePanel(Palette.Magic).Place(8, 8, 28, 28);
            badge.MouseFilter = Control.MouseFilterEnum.Stop;
            badge.TooltipText = explain;
            badge.AddChild(UiKit.MakeLabel("?", 16, Palette.Magic, HorizontalAlignment.Center).Place(0, 4, 28, 20)); // loc: keys
            card.AddChild(badge);
            blurb.MouseFilter = Control.MouseFilterEnum.Pass;
            blurb.TooltipText = explain;
        }
        card.AddChild(UiKit.MakeButton(Loc.T("Take this one"), Palette.Good, 16, () => Take(curio)).Place(30, CardH - 56, CardW - 60, 44));
        return card;
    }

    /// <summary>Takes one (or nothing), and the shop opens.</summary>
    public async void Take(CurioDef curio)
    {
        if (_done) return;
        _done = true;
        State.TakeFromDrawer(curio);
        if (curio != null)
        {
            int index = _cards.IndexOf(curio);
            Sfx.Play("sting", 1.2f, -4);
            for (int i = 0; i < _panels.Count; i++)
            {
                var panel = _panels[i];
                var tween = panel.CreateTween().SetParallel();
                if (i == index)
                {
                    panel.PivotOffset = new Vector2(CardW / 2, CardH / 2);
                    tween.TweenProperty(panel, "scale", Vector2.One * 1.06f, 0.18f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
                }
                else tween.TweenProperty(panel, "modulate:a", 0.25f, 0.2f);
            }
            if (index >= 0) FloatText($"{curio.Name}!", _panels[index].Position + new Vector2(CardW / 2, -8), Palette.Magic, 24);
            Game.Moment("23_drawer_taken", 0.5);
            await ToSignal(GetTree().CreateTimer(0.9), SceneTreeTimer.SignalName.Timeout);
            if (!Alive) return;
        }
        _then?.Invoke();
    }
}
