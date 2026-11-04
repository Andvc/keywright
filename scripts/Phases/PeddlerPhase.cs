using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>
/// Nim the peddler comes by every night after the shutters are down (0.28): one curio on the counter at first, one more
/// for each of his upgrades (his lantern, his stool, his standing order), which he offers himself, with a case for your
/// curios. Each curio bends one rule of the trade and asks a fixed sum by its rarity and a share of what's in your purse
/// tonight; each card says which ways of trading it's for, and which of yours it goes with. Buy what suits how you deal,
/// sell back what doesn't for half, see to the back room (a stolen chest returned to its owner, or sold to Nim's
/// friends), and go to bed. A curio that would leave you short of two of the shop's typical chests has a red price, and
/// Nim asks first.
/// </summary>
public partial class PeddlerPhase : Phase
{
    // Tall enough for five lines of a narrow card's description above what it goes with.
    // Nim stands on the left; his cloth is the rest of the counter, the cards centred on it however many he lays out.
    // The rows under the cloth start clear of Bram's corner (his tips, bottom left).
    const float CardH = 384, CardsY = 96, CardGap = 20, AreaX = 340, AreaW = 920, CardsRight = AreaX + AreaW, RowX = 560;

    // One card to the right of Nim, up to four (with his upgrades), the fourth starting nearer him and a little narrower.
    float CardW => CardWidth(State.PeddlerCount > 3);
    static float CardWidth(bool narrow) => narrow ? 225 : 260;

    /// <summary>
    /// How a curio's name sits on its card: in the large size where it fits on one line of a wide card, else in the small
    /// size (two lines at most); the box as tall as it is, so the ways of trading and the description follow right under
    /// it and the description has the rest of the card (the fit test measures the same).
    /// </summary>
    public static (float W, float H, int Size) NameBox(string name, bool narrow)
    {
        float w = CardWidth(narrow) - 20;
        if (!narrow)
        {
            float large = UiKit.TextHeight(name, w, 24);
            if (large <= UiKit.TextHeight("A", w, 24) + 0.5f) return (w, large, 24); // loc: keys
        }
        return (w, Mathf.Max(UiKit.TextHeight(name, w, 16), 20), 16);
    }

    /// <summary>A card's description box under a name that tall: down to what it goes with.</summary>
    public static (float W, float H) BlurbBox(bool narrow, float nameH) => (CardWidth(narrow) - 28, CardH - 84 - (NameY + 22) - nameH);

    /// <summary>Where the name starts, just under the curio's picture; its ways of trading and its description follow.</summary>
    const float NameY = 96;
    float CardsX => AreaX + (AreaW - (State.PeddlerCount * (CardW + CardGap) - CardGap)) / 2;

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
        var shop = Add(UiKit.MakeShop(State.Tier, 1f, ShopHour.Night, State));
        var nim = new SellerDef { Key = Story.Nim, Name = Peddler.Name };
        _nim = Add(new SellerView(nim));
        shop.Light(_nim.Figure);
        _nim.Enter();
        _nim.Say(Peddler.Greetings[State.Day % Peddler.Greetings.Length]);
        Sfx.PlayGroup("door_open", 1.2f, -6);
        Add(UiKit.MakeLabel(Loc.T("NIM'S CURIOS"), 32, Palette.Magic, HorizontalAlignment.Center, outline: 6).Place(AreaX, 58, AreaW, 40));
        _board = Add(new Node2D());
        _mentor = Add(new MentorView { Position = new Vector2(24, 560), ZIndex = 20 });
        Build();
        _mentor.Teach(State, "peddler");
        // A card that leans on Bram's word or on buying sight unseen: Bram says what it means, once.
        var terms = State.PeddlerStock.SelectMany(Glossary.For).ToHashSet();
        if (terms.Contains(Term.SightUnseen)) _mentor.Teach(State, "term_sightunseen");
        if (terms.Contains(Term.BramsWord)) _mentor.Teach(State, "term_bramsword");
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
            _board.AddChild(UiKit.MakeLabel(Loc.T("Nim's sold out: you own every curio Nim carries!"), 24, Palette.Muted, HorizontalAlignment.Center)
                .Place(AreaX, 220, AreaW, 30));
        string note = Loc.T($"Each curio costs a set sum for its rarity, plus a share of the gold you have tonight ({State.PeddlerPurse}g).");
        if (stock.Count > 0)
            _board.AddChild(UiKit.MakeLabel(note, 16, Palette.Muted, HorizontalAlignment.Center, outline: 4)
                .Place(AreaX, CardsY + CardH + 4, AreaW, 20));

        // Your curios: click one to sell it back for half.
        const float y = CardsY + CardH + 32;
        _board.AddChild(UiKit.MakeLabel(Loc.T($"YOUR CURIOS {State.CurioKeys.Count}/{State.CurioSlots}"), 16, Palette.Magic).Place(RowX, y, 200, 20));
        // Full: Nim's prices still show, greyed out, and this says how to make room.
        _board.AddChild(State.CuriosFull
            ? UiKit.MakeLabel(Loc.T("No room for more: click one to sell it back for half."), 16, Palette.Warn).Place(RowX + 200, y, 560, 20)
            : UiKit.MakeLabel(Loc.T("Click one to sell it back for half."), 16, Palette.Muted).Place(RowX + 200, y, 560, 20));
        for (int i = 0; i < State.CurioSlots; i++)
        {
            float x = RowX + i * 76;
            // Each curio framed in its own rarity's colour, as Nim's cards are.
            var curio = i < State.CurioKeys.Count ? Curios.Get(State.CurioKeys[i]) : null;
            var slot = UiKit.MakePanel(curio != null ? Palette.Of(curio.Rarity).Darkened(0.2f) : Palette.PanelBorder).Place(x, y + 26, 68, 68);
            _board.AddChild(slot);
            if (curio == null) continue;
            slot.AddChild(UiKit.MakeCurioIcon(curio, new Vector2(34, 34)));
            var sell = new Button
            {
                Flat = true, FocusMode = Control.FocusModeEnum.None,
                TooltipText = Loc.T($"{curio.Name}: {curio.Blurb}\nClick to sell it back for {State.CurioValue(curio.Key)}g."),
            };
            sell.Pressed += () => SellBack(curio.Key);
            slot.AddChild(sell.Place(0, 0, 68, 68));
        }
        _board.AddChild(UiKit.MakeButton(Loc.T("Good night"), Palette.Gold, 24, Leave).Place(1040, 610, 220, 64));

        // What Nim offers besides: one more curio a night (his lantern, his stool, his order), and a case for yours.
        var offers = Upgrades.AtNim(State).OrderBy(u => u.NimCards > 0 ? 0 : 1).Take(2).ToList();
        // Each 232 wide, or as wide as its words in a longer language, ending where "Good night" begins.
        var labels = offers.Select(u => Loc.T($"{u.Name} {State.Cost(u)}g")).ToList();
        var widths = labels.Select(l => Mathf.Max(232, UiKit.TextWidth(l) + 28)).ToList();
        float offerX = RowX + 2 * 240 - 8 - widths.Sum() - 8 * (offers.Count - 1);
        for (int i = 0; i < offers.Count; i++)
        {
            var upgrade = offers[i];
            int price = State.Cost(upgrade);
            var button = UiKit.MakeButton(labels[i], Palette.Magic, 16, () => BuyUpgrade(upgrade));
            button.Disabled = State.Gold < price;
            button.TooltipText = upgrade.Blurb;
            _board.AddChild(button.Place(offerX, 614, widths[i], 32));
            offerX += widths[i] + 8;
        }

        // The back room: chests that won't open for you, and what can be done with them tonight.
        int held = State.BackRoom.Count;
        if (held > 0)
        {
            var room = UiKit.MakeButton(Loc.T($"The back room ({held})"), Palette.Warn, 16, OpenBackRoom).Place(RowX, 652, 472, 30);
            room.TooltipText = Loc.T("Chests that won't open for you: return them to their owners, or sell them to Nim's friends.");
            _board.AddChild(room);
        }
    }

    /// <summary>Buys one of Nim's own upgrades (or a curio case) off him, tonight.</summary>
    public bool BuyUpgrade(Upgrade upgrade)
    {
        if (!State.BuyOffered(upgrade)) return false;
        Sfx.PlayGroup("pay");
        Game.Hud.SetGold(State.Gold);
        _nim.Say(upgrade.NimCards > 0 ? Peddler.MoreCards : Peddler.Thanks[0], Palette.Good);
        _nim.Bob();
        Build();
        return true;
    }

    Control _backRoom;

    /// <summary>The back room: each chest that won't open for you, whose it is, and what returning or selling it brings.</summary>
    public void OpenBackRoom()
    {
        _backRoom?.QueueFree();
        _backRoom = null;
        var held = State.BackRoom.ToList();
        if (held.Count == 0) return;
        float h = 90 + held.Count * 56;
        var panel = UiKit.MakePanel(Palette.Warn.Darkened(0.3f)).Place(300, 120, 680, h);
        panel.ZIndex = 30;
        panel.AddChild(UiKit.MakeLabel(Loc.T("THE BACK ROOM"), 24, Palette.Warn, HorizontalAlignment.Center, outline: 4).Place(0, 12, 680, 30));
        for (int i = 0; i < held.Count; i++)
        {
            var chest = held[i];
            float y = 52 + i * 56;
            string what = chest.Key == "duel" ? Loc.T("The duel's stakes, held for the winner")
                : chest.Kept ? Loc.T($"{chest.Owner}'s parcel, kept for collection") : Loc.T($"A {Categories.ColourName(chest.Colour)} chest: {chest.Owner}'s");
            panel.AddChild(UiKit.MakeLabel(what, 16, Palette.Text).Place(20, y + 12, 300, 20));
            if (chest.Kept) continue;
            panel.AddChild(UiKit.MakeButton(Loc.T($"Return it +{State.FinderShare}g"), Palette.Good, 16, () => ReturnHeld(chest)).Place(330, y, 166, 44));
            panel.AddChild(UiKit.MakeButton(Loc.T($"Nim's friends +{State.FencePay(chest)}g"), Palette.Magic, 16, () => FenceHeld(chest)).Place(504, y, 160, 44));
        }
        panel.AddChild(UiKit.MakeButton(Loc.T("Close"), Palette.Text, 16, () =>
        {
            _backRoom?.QueueFree();
            _backRoom = null;
        }).Place(260, h - 44, 160, 36));
        _backRoom = Add(panel);
        Game.Moment("22_backroom", 0.4);
    }

    /// <summary>Returns a back-room chest to its owner: the Wardens' finder's share.</summary>
    public void ReturnHeld(Held held)
    {
        int pay = State.ReturnHeld(held);
        if (pay <= 0) return;
        Sfx.PlayGroup("pay");
        Game.Hud.SetGold(State.Gold);
        _nim.Say(Peddler.Returned);
        Build();
        if (State.BackRoom.Count > 0) OpenBackRoom();
        else _backRoom?.QueueFree();
    }

    /// <summary>Sells a back-room chest to Nim's friends.</summary>
    public void FenceHeld(Held held)
    {
        int pay = State.FenceHeld(held);
        Sfx.PlayGroup("pay");
        Game.Hud.SetGold(State.Gold);
        _nim.Say(Peddler.Fenced);
        Build();
        if (State.BackRoom.Count > 0) OpenBackRoom();
        else _backRoom?.QueueFree();
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
        // Four narrower cards: the name in the small size, so the blurb has room (and a long name on a wide one).
        bool narrow = State.PeddlerCount > 3;
        var (nameW, nameH, nameSize) = NameBox(curio.Name, narrow);
        card.AddChild(UiKit.MakeLabel(curio.Name, nameSize, owned ? Palette.Muted : Palette.Text, HorizontalAlignment.Center, outline: 4)
            .Wrapped().Place(10, NameY, nameW, nameH));
        // Its rarity in the corner, and what it's for under its name (just the first way of trading on a narrow card).
        card.AddChild(UiKit.MakeLabel(Curios.RarityName(curio.Rarity), 16, color, HorizontalAlignment.Right).Place(CardW - 110, 8, 100, 20));
        string tags = narrow ? Curios.TagName(curio.Tags[0], curio) : Curios.Tags(curio);
        if (UiKit.TextWidth(tags) > CardW - 8) tags = Curios.TagName(curio.Tags[0], curio);
        card.AddChild(UiKit.MakeLabel(tags, 16, color, HorizontalAlignment.Center).Place(0, NameY + nameH, CardW, 20));
        var (blurbW, blurbH) = BlurbBox(narrow, nameH);
        var blurb = UiKit.MakeLabel(curio.Blurb, 16, Palette.Muted, HorizontalAlignment.Center).Wrapped()
            .Place(14, NameY + 22 + nameH, blurbW, blurbH);
        card.AddChild(blurb);
        // The words of the trade it leans on ("sight unseen", "Bram's word"...), said exactly: a "?" in the corner, and
        // the same over its description.
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
        // What it goes with: a curio you keep that shares a way of trading with it; or a way of trading this shop opens.
        if (!owned && State.OwnedCurios.FirstOrDefault(c => c.Tags.Intersect(curio.Tags).Any()) is CurioDef kin)
        {
            // As much of it as fits: the curio it goes with, or the way of trading they share.
            const int perLetter = 9;
            string with = Loc.T($"Goes with your {kin.Name}");
            if (with.Length * perLetter > CardW - 8) with = Loc.T($"With your {kin.Name}");
            if (with.Length * perLetter > CardW - 8) with = Loc.T($"Goes with your {Curios.TagName(kin.Tags.Intersect(curio.Tags).First(), kin)}");
            card.AddChild(UiKit.MakeLabel(with, 16, Palette.Good, HorizontalAlignment.Center).Place(0, CardH - 82, CardW, 20));
        }
        else if (!owned && curio.MinTier == State.Tier && curio.MinTier > 1 && curio.Tags.Any(Curios.Main.Contains))
            card.AddChild(UiKit.MakeLabel(Loc.T($"New at {ShopTiers.Name(State.Tier)}"), 16, Palette.Magic, HorizontalAlignment.Center).Place(0, CardH - 82, CardW, 20));
        if (owned)
        {
            card.AddChild(UiKit.MakeLabel(Loc.T("Yours"), 24, Palette.Good, HorizontalAlignment.Center, outline: 4).Place(0, CardH - 50, CardW, 30));
            return card;
        }
        string warning = price == 0 ? null : State.ShortNote(price, curio: curio);
        // The price shows whether or not there's room for it: with no room, the button's greyed out, and says why.
        var buy = UiKit.MakeButton(price == 0 ? Loc.T("Take it, free") : Loc.T($"Buy {price}g"),
            price == 0 ? Palette.Good : warning != null ? Palette.Bad : Palette.Gold, 16, () => AskBuy(curio)).Place(30, CardH - 56, CardW - 60, 44);
        buy.Disabled = State.CuriosFull || State.Gold < price;
        string why = Loc.T($"{Curios.Base(curio.Rarity)}g for a {Curios.RarityName(curio.Rarity)} curio, and {Synergies.Pct(Curios.Share(curio.Rarity))} of your purse tonight.");
        buy.TooltipText = State.CuriosFull ? Loc.T($"No room: sell a curio back to Nim first. {why}")
            : warning != null ? Loc.T($"{why} {warning}") : why;
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
        _confirm = Add(UiKit.MakeConfirm(Loc.T("ARE YOU SURE?"), warning, Palette.Bad, Loc.T("Buy anyway"), () =>
        {
            CloseConfirm();
            Buy(curio);
        }, Loc.T("Not now"), CloseConfirm));
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
        FloatText($"{curio.Name}!", new Vector2(AreaX + AreaW / 2, 560), Palette.Magic, 24);
        Build();
        Game.Moment("22_bought", 0.4);
        return true;
    }

    public void SellBack(string key)
    {
        int value = State.SellCurio(key);
        if (value <= 0) return;
        Sfx.PlayGroup("pay");
        Game.Hud.SetGold(State.Gold);
        _nim.Say(Peddler.Back);
        FloatText(Loc.T($"+{value}g"), new Vector2(RowX + 120, 420), Palette.Gold, 24);
        Build();
    }

    public async void Leave()
    {
        if (_leaving) return;
        _leaving = true;
        _nim.Say(Peddler.Bye[State.Day % Peddler.Bye.Length]);
        Sfx.PlayGroup("door_close", 1.2f, -6);
        await ToSignal(_nim.Leave(), Tween.SignalName.Finished);
        if (!Alive) return;
        Game.StartDay();
    }
}
