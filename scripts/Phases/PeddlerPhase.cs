using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>
/// Nim the peddler drops by before the shop opens, twice a week (once on a story week): three curios laid out on the
/// counter, each bending one rule of the trade, each asking a fixed sum by its rarity and a share of what's in your
/// purse this morning. Each card says which ways of trading it's for, and which of yours it goes with. Buy what suits
/// how you deal (there's room for a few), sell back what doesn't for half, and open the shop. A curio that would leave
/// you short of two of the shop's typical chests has a red price, and Nim asks first.
/// </summary>
public partial class PeddlerPhase : Phase
{
    // Tall enough for five lines of a narrow card's description above what it goes with.
    const float CardH = 384, CardsY = 96, CardGap = 20, CardsRight = 1260;

    // Three cards to the right of Nim; four (a familiar face) start nearer him and are a little narrower.
    float CardW => State.PeddlerCount > 3 ? 225 : 260;
    float CardsX => CardsRight - State.PeddlerCount * (CardW + CardGap) + CardGap;

    SellerView _nim;
    MentorView _mentor;
    Node2D _board;
    Control _confirm;
    NotebookView _notebook;
    bool _leaving;

    public MentorView Mentor => _mentor;

    /// <summary>What's on the counter, and what each costs today.</summary>
    public System.Collections.Generic.IEnumerable<CurioDef> Stock => State.PeddlerStock.Where(c => !State.HasCurio(c.Key));

    public override void _Ready()
    {
        Add(UiKit.MakeShop(State.Tier, 0.6f));
        var nim = new SellerDef { Key = Story.Nim, Name = Peddler.Name };
        _nim = Add(new SellerView(nim) { Position = new Vector2(190, 196) });
        _nim.Enter();
        _nim.Say(Peddler.Greetings[(State.Week - 2 + Peddler.Greetings.Length) % Peddler.Greetings.Length]);
        Sfx.PlayGroup("door_open", 1.2f, -6);
        Add(UiKit.MakeLabel("NIM'S CURIOS", 32, Palette.Magic, HorizontalAlignment.Center, outline: 6).Place(CardsX, 58, CardsRight - CardsX, 40));
        _board = Add(new Node2D());
        _mentor = Add(new MentorView { Position = new Vector2(24, 560), ZIndex = 20 });
        Build();
        _mentor.Teach(State, "peddler");
        Game.Hud.SetGold(State.Gold, animate: false);
        Game.Moment("22_peddler", 0.8);
    }

    /// <summary>The top bar's due was clicked: the notebook at the Guild's debt, to weigh a curio against it.</summary>
    public override bool ShowDebt()
    {
        if (_notebook != null || _confirm != null || _leaving) return false;
        _notebook = Add(new NotebookView(State, NotebookView.CalendarPage, focusDebt: true) { ZIndex = 30 });
        _notebook.Closed += () => _notebook = null;
        return true;
    }

    void Build()
    {
        foreach (var child in _board.GetChildren()) child.QueueFree();
        var stock = State.PeddlerStock;
        for (int i = 0; i < stock.Count; i++) _board.AddChild(MakeCard(stock[i], CardsX + i * (CardW + CardGap), CardsY));
        if (stock.Count == 0)
            _board.AddChild(UiKit.MakeLabel("Nim's sold out: you own every curio Nim carries!", 24, Palette.Muted, HorizontalAlignment.Center)
                .Place(CardsX, 220, CardsRight - CardsX, 30));
        string note = $"Each curio costs a set sum for its rarity, plus a share of the gold you had this morning ({State.PeddlerPurse}g).";
        if (stock.Count > 0)
            _board.AddChild(UiKit.MakeLabel(note, 16, Palette.Muted, HorizontalAlignment.Center, outline: 4)
                .Place(CardsX, CardsY + CardH + 4, CardsRight - CardsX, 20));
        // Once a visit, Nim packs these away and lays out something else.
        if (State.CanRerollPeddler)
            _board.AddChild(UiKit.MakeButton("Show me something else", Palette.Magic, 16, Reroll).Place(660, 610, 300, 64));

        // Your curios: click one to sell it back for half.
        const float y = CardsY + CardH + 32;
        _board.AddChild(UiKit.MakeLabel($"YOUR CURIOS {State.CurioKeys.Count}/{State.CurioSlots}", 16, Palette.Magic).Place(CardsX, y, 300, 20));
        _board.AddChild(UiKit.MakeLabel("Click one to sell it back to Nim for half what you paid.", 16, Palette.Muted).Place(CardsX + 220, y, 600, 20));
        for (int i = 0; i < State.CurioSlots; i++)
        {
            float x = CardsX + i * 76;
            var slot = UiKit.MakePanel(i < State.CurioKeys.Count ? Palette.Magic.Darkened(0.2f) : Palette.PanelBorder).Place(x, y + 26, 68, 68);
            _board.AddChild(slot);
            if (i >= State.CurioKeys.Count) continue;
            var curio = Curios.Get(State.CurioKeys[i]);
            slot.AddChild(UiKit.MakeCurioIcon(curio, new Vector2(34, 34)));
            var sell = new Button
            {
                Flat = true, FocusMode = Control.FocusModeEnum.None,
                TooltipText = $"{curio.Name}: {curio.Blurb}\nClick to sell it back for {State.CurioValue(curio.Key)}g.",
            };
            sell.Pressed += () => SellBack(curio.Key);
            slot.AddChild(sell.Place(0, 0, 68, 68));
        }
        _board.AddChild(UiKit.MakeButton("Open the shop", Palette.Gold, 24, Leave).Place(980, 610, 280, 64));
    }

    Control MakeCard(CurioDef curio, float x, float y)
    {
        bool owned = State.HasCurio(curio.Key);
        int price = State.CuriosPrice(curio);
        var color = Palette.Of(curio.Rarity);
        var card = UiKit.MakePanel(owned ? Palette.PanelBorder : color).Place(x, y, CardW, CardH);
        var shine = new Painter(p =>
        {
            for (int i = 3; i >= 1; i--) p.DrawCircle(new Vector2(CardW / 2, 62), 20 + i * 10, color with { A = 0.08f });
        });
        card.AddChild(shine);
        card.AddChild(UiKit.MakeCurioIcon(curio, new Vector2(CardW / 2, 62)));
        // Four narrower cards: the name in the small size, so the blurb has room.
        bool narrow = State.PeddlerCount > 3;
        float nameH = narrow ? 40 : 60;
        card.AddChild(UiKit.MakeLabel(curio.Name, narrow ? 16 : 24, owned ? Palette.Muted : Palette.Text, HorizontalAlignment.Center, outline: 4)
            .Wrapped().Place(10, 100, CardW - 20, nameH));
        // Its rarity in the corner, and what it's for under its name (just the first way of trading on a narrow card).
        card.AddChild(UiKit.MakeLabel(Curios.RarityName(curio.Rarity), 16, color, HorizontalAlignment.Right).Place(CardW - 110, 8, 100, 20));
        string tags = narrow ? Curios.TagName(curio.Tags[0], curio) : Curios.Tags(curio);
        card.AddChild(UiKit.MakeLabel(tags, 16, color, HorizontalAlignment.Center).Place(0, 100 + nameH, CardW, 20));
        card.AddChild(UiKit.MakeLabel(curio.Blurb, 16, Palette.Muted, HorizontalAlignment.Center).Wrapped()
            .Place(14, 124 + nameH, CardW - 28, CardH - 84 - 124 - nameH));
        // What it goes with: a curio you keep that shares a way of trading with it; or a way of trading this shop opens.
        if (!owned && State.OwnedCurios.FirstOrDefault(c => c.Tags.Intersect(curio.Tags).Any()) is CurioDef kin)
        {
            // As much of it as fits: the curio it goes with, or the way of trading they share.
            const int perLetter = 9;
            string with = $"Goes with your {kin.Name}";
            if (with.Length * perLetter > CardW - 8) with = $"With your {kin.Name}";
            if (with.Length * perLetter > CardW - 8) with = $"Goes with your {Curios.TagName(kin.Tags.Intersect(curio.Tags).First(), kin)}";
            card.AddChild(UiKit.MakeLabel(with, 16, Palette.Good, HorizontalAlignment.Center).Place(0, CardH - 82, CardW, 20));
        }
        else if (!owned && curio.MinTier == State.Tier && curio.MinTier > 1 && curio.Tags.Any(Curios.Main.Contains))
            card.AddChild(UiKit.MakeLabel($"New at {ShopTiers.Name(State.Tier)}", 16, Palette.Magic, HorizontalAlignment.Center).Place(0, CardH - 82, CardW, 20));
        if (owned)
        {
            card.AddChild(UiKit.MakeLabel("Yours", 24, Palette.Good, HorizontalAlignment.Center, outline: 4).Place(0, CardH - 50, CardW, 30));
            return card;
        }
        string warning = price == 0 ? null : State.ShortNote(price, curio: curio);
        var buy = UiKit.MakeButton(State.CuriosFull ? "No room" : price == 0 ? "Take it, free" : $"Buy {price}g",
            price == 0 ? Palette.Good : warning != null ? Palette.Bad : Palette.Gold, 16, () => AskBuy(curio)).Place(30, CardH - 56, CardW - 60, 44);
        buy.Disabled = State.CuriosFull || State.Gold < price;
        buy.TooltipText = State.CuriosFull ? "Sell a curio back to Nim first to make room."
            : $"{Curios.Base(curio.Rarity)}g for a {Curios.RarityName(curio.Rarity)} curio, and {Synergies.Pct(Curios.Share(curio.Rarity))} of your purse this morning." +
              (warning != null ? $" {warning}" : "");
        card.AddChild(buy);
        return card;
    }

    /// <summary>Buys it, or first asks if it would leave you short (<see cref="GameState.ShortNote"/>).</summary>
    public void AskBuy(CurioDef curio)
    {
        if (_confirm != null) return;
        int price = State.CuriosPrice(curio);
        string warning = price == 0 ? null : State.ShortNote(price, curio: curio);
        if (warning == null)
        {
            Buy(curio);
            return;
        }
        _nim.Say(Peddler.Stretch);
        _confirm = Add(UiKit.MakeConfirm("ARE YOU SURE?", warning, Palette.Bad, "Buy anyway", () =>
        {
            CloseConfirm();
            Buy(curio);
        }, "Not now", CloseConfirm));
        Game.Moment("22_short", 0.4);
    }

    /// <summary>"Not now": closes the warning without buying (the bot uses this).</summary>
    public void CloseConfirm()
    {
        _confirm?.QueueFree();
        _confirm = null;
    }

    public bool Buy(CurioDef curio)
    {
        int price = State.CuriosPrice(curio);
        if (!State.BuyCurio(curio, price)) return false;
        Sfx.PlayGroup("pay");
        Sfx.Play("sting", 1.2f, -4);
        Game.Hud.SetGold(State.Gold);
        _nim.Say(Peddler.Thanks[State.CurioKeys.Count % Peddler.Thanks.Length], Palette.Good);
        _nim.Bob();
        FloatText($"{curio.Name}!", new Vector2(CardsX + 1.5f * CardW + CardGap, 560), Palette.Magic, 24);
        Build();
        Game.Moment("22_bought", 0.4);
        return true;
    }

    public void Reroll()
    {
        if (!State.CanRerollPeddler) return;
        State.RerollPeddler();
        Sfx.PlayGroup("clack", 1.1f, -4);
        _nim.Say(Peddler.More);
        Build();
    }

    public void SellBack(string key)
    {
        int value = State.SellCurio(key);
        if (value <= 0) return;
        Sfx.PlayGroup("pay");
        Game.Hud.SetGold(State.Gold);
        _nim.Say(Peddler.Back);
        FloatText($"+{value}g", new Vector2(CardsX + 120, 420), Palette.Gold, 24);
        Build();
    }

    public async void Leave()
    {
        if (_leaving) return;
        _leaving = true;
        _nim.Say(Peddler.Bye[State.Week % Peddler.Bye.Length]);
        Sfx.PlayGroup("door_close", 1.2f, -6);
        await ToSignal(_nim.Leave(), Tween.SignalName.Finished);
        if (!Alive) return;
        Game.OpenShop();
    }
}
