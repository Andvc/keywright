using System;
using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>
/// After the last customer: how the day went, then what to spend it on. Until the stall has earned its keep there's
/// nothing to buy, just how far it has to go. Then renovations bring new customers, new things and new sounds to
/// learn; tools tell you more about each chest. From the village shop, cabinets show off what you keep, and the
/// exhibition along the bottom draws more and better customers (click something in it to sell it). From the town
/// shop, the Synergies button opens the journal, where you choose which of the shop's synergies are active. Bram
/// introduces each as it opens up. Down the right: the Guild's next due, this week's commissions and your curios; on
/// the fifth evening the button closes up for Mister Vane.
/// </summary>
public partial class DayEndPhase : Phase
{
    const float CardW = 400, CardH = 112, CardGapX = 16, CardGapY = 8, CardsX = 40, CardsY = 206, SideX = 872, SideW = 368;

    Node2D _cards, _side;
    Label _goldLabel, _header;
    MentorView _mentor;
    Button _synergies;
    NotebookView _notebook;
    bool _built;

    public MentorView Mentor => _mentor;

    public override void _Ready()
    {
        bool shopWasOpen = State.UpgradesOpen;
        State.EndDay();
        Add(UiKit.MakeShop(State.Tier, 0.7f, ShopHour.Evening, State));
        Game.Hud.SetDay(State.Day, State.Tier);
        Game.Hud.SetGold(State.Gold);

        var today = State.Today;
        Game.Hud.SetDue(State);
        Add(UiKit.MakeLabel(Loc.T($"END OF DAY {State.Day}"), 32, Palette.Text, HorizontalAlignment.Center, outline: 4).Place(0, 64, 1280, 40));
        BuildSummary(today);

        _header = Add(Header("", CardsX, 182));
        _goldLabel = Add(UiKit.MakeLabel("", 16, Palette.Gold, HorizontalAlignment.Right).Place(156, 182, 700, 20)); // right-aligned: room to the left for longer words
        _cards = Add(new Node2D());
        _side = Add(new Node2D());
        // Bram first: building the cards can already call for a tip (the Bookkeeping Engine's, the first time it's offered).
        // The echo horn on the counter, beside closing up: what was said today and before (0.35).
        Add(new EchoHorn(State) { Position = new Vector2(SideX - 76, 644) });
        _mentor = Add(new MentorView { Position = new Vector2(40, 440), ZIndex = 20 });
        BuildCards();

        bool dues = State.DuesTonight && !State.PaidThisWeek;
        bool intro = !State.GuildIntroduced;
        Add(UiKit.MakeButton(dues ? Loc.T("Close up: Vane is waiting") : intro ? Loc.T("Close up for the night") : Loc.T($"Open the shop: Day {State.Day + 1}"),
            dues ? Palette.Warn : Palette.Gold, dues ? 16 : 24, NextDay).Place(SideX, 626, SideW, 64));
        _synergies = Add(UiKit.MakeButton("", Palette.Magic.Darkened(0.2f), 16, OpenSynergies).Place(SideX, 586, SideW, 34));
        RefreshSynergyButton();
        // One tip at a time, the most pressing first: the rest wait for an evening when they're still news.
        if (State.CanPayOff) Teach("payoff");
        if (State.EngineOffered) Teach("engine");
        if (!shopWasOpen && State.UpgradesOpen) Celebrate(Loc.T("The shop's open for business!"));
        Unlocked();
        Teach(State.UpgradesOpen ? "upgrades" : "dayend");
        if (State.Display.Count > 0) Teach("exhibition");
        // A quiet evening in the first week: what to listen for, before anyone's heard one sing.
        if (State.Day >= 3 && State.Songbook.Count == 0 && !State.Taught.Contains("heard_Heartsong")) Teach("songs_intro");
        Sfx.Play(today.Profit >= 0 ? "win" : "lose", 1f, -6);
        Game.Moment("09_dayend", 0.5);
    }

    /// <summary>The day in figures, side by side: what you bought and passed on, spent and took in, and the profit.</summary>
    void BuildSummary(DayStats today)
    {
        // The day's profit is everything it brought in against what it spent: the chests (less what went to a commission
        // instead of being sold), the commissions, the curios, the door money and the town (a key piece's fee too).
        int profit = today.Profit + today.Commissions + today.Curios + today.Admission + today.Tales;
        var figures = new System.Collections.Generic.List<(string Label, string Value, Color Color)>
        {
            (Loc.T("Bought"), today.Bought.ToString(), Palette.Text),
            (Loc.T("Passed"), today.Passed.ToString(), Palette.Text),
            (Loc.T("Spent"), Money.Short(today.Spent), Palette.Text),
            (Loc.T("The chests held"), Money.Short(today.Earned), Palette.Gold),
            (Loc.T("Profit"), (profit > 0 ? "+" : "") + Money.Short(profit), profit > 0 ? Palette.Good : profit < 0 ? Palette.Bad : Palette.Text),
        };
        if (today.Commissions > 0) figures.Add((Loc.T("Commissions"), Loc.T($"+{today.Commissions}g"), Palette.Good));
        if (today.Curios > 0) figures.Add((Loc.T("Curios"), Loc.T($"+{today.Curios}g"), Palette.Magic));
        if (today.Admission > 0) figures.Add((Loc.T("Visitors"), Loc.T($"+{today.Admission}g"), Palette.Magic));
        // What the day's people brought or cost (rewards, fees, the Wardens' finder's shares), and what went to the back room.
        if (today.Tales != 0) figures.Add((Loc.T("Town"), today.Tales > 0 ? Loc.T($"+{today.Tales}g") : Loc.T($"{today.Tales}g"), today.Tales > 0 ? Palette.Good : Palette.Bad));
        if (today.Held > 0) figures.Add((Loc.T("Back room"), today.Held.ToString(), Palette.Warn));
        if (today.PerfectReads > 0) figures.Add((Loc.T("Perfect reads"), today.PerfectReads.ToString(), Palette.Gold));
        if (today.NewFinds > 0) figures.Add((Loc.T("New finds"), today.NewFinds.ToString(), Palette.Magic));
        // Up to nine side by side: narrower when there are many, so they always fit the screen. Past nine (a busy day: a
        // key piece's fee, a commission, a curio...) the least of them give way, new finds first, then perfect reads.
        foreach (string minor in new[] { Loc.T("New finds"), Loc.T("Perfect reads") })
            if (figures.Count > 9) figures.RemoveAll(f => f.Label == minor);
        const float gap = 8;
        float w = Math.Min(140, (1240 - (figures.Count - 1) * gap) / figures.Count);
        float x0 = 640 - (figures.Count * w + (figures.Count - 1) * gap) / 2;
        for (int i = 0; i < figures.Count; i++)
        {
            var (label, value, color) = figures[i];
            var chip = Add(UiKit.MakePanel(label == Loc.T("Profit") ? color.Darkened(0.2f) : null).Place(x0 + i * (w + gap), 102, w, 52));
            chip.AddChild(UiKit.MakeLabel(label, 16, Palette.Muted, HorizontalAlignment.Center).Place(0, 4, w, 20));
            chip.AddChild(UiKit.MakeLabel(value, 24, color, HorizontalAlignment.Center, outline: 4).Place(0, 20, w, 30));
        }
    }

    /// <summary>Bram's tip, the first time, if he isn't saying something else. True if he said it.</summary>
    bool Teach(string key) => _mentor.Teach(State, key);

    /// <summary>What the shop's grown into brings, said once: a shelf to display things on, synergies, the Emporium.</summary>
    void Unlocked()
    {
        RefreshSynergyButton();
        if (State.DisplayOpen) Teach("display");
        if (State.SynergiesOpen && Teach("synergy")) Pulse(_synergies);
        if (State.Tier >= ShopTiers.Max) Teach("emporium");
    }

    /// <summary>A banner across the top for something that just opened up.</summary>
    void Celebrate(string text)
    {
        Sfx.Play("fanfare", 1f, -6);
        FloatText(text, new Vector2(640, 168), Palette.Gold, 32);
    }

    /// <summary>
    /// Renovation first, then tools by price. Things you can't afford yet still show, so you can plan. Before the
    /// stall has earned its keep: how far it has to go.
    /// </summary>
    void BuildCards()
    {
        foreach (var child in _cards.GetChildren()) child.QueueFree();
        BuildSide();
        // Bram's working purse, and (with a renovation on offer) the bigger one its shop would need.
        var next = Upgrades.Available(State).FirstOrDefault(u => u.IsRenovation);
        string keep = next == null ? Loc.T($"Keep about {State.WorkingPurse}g for tomorrow's chests.")
            : Loc.T($"Keep about {State.WorkingPurse}g for chests ({State.ReserveIf(next)}g after renovating).");
        _goldLabel.Text = State.UpgradesOpen ? Loc.T($"You have {State.Gold}g. {keep}") : Loc.T($"You have {State.Gold}g");
        _header.Text = State.UpgradesOpen ? Loc.T("SPEND IT ON THE SHOP") : Loc.T("FIX UP THE STALL");
        if (!State.UpgradesOpen)
        {
            _cards.AddChild(MakeGoal());
            // Isolde's shelf is up before the stall has earned its keep.
            BuildExhibition();
            return;
        }
        // The Guild's Bookkeeping Engine first, when it's offered: it's the way to finish a won run quickly.
        var offers = Upgrades.Available(State).OrderByDescending(u => u.Key == Upgrades.Engine).ThenByDescending(u => u.IsRenovation)
            .ThenBy(State.Cost).Take(6).ToList();
        if (offers.Count == 0)
            _cards.AddChild(UiKit.MakeLabel(Loc.T("Nothing left to buy. The shop is complete."), 24, Palette.Muted, HorizontalAlignment.Center)
                .Place(CardsX, CardsY + 80, 2 * CardW + CardGapX, 30));
        for (int i = 0; i < offers.Count; i++)
        {
            float x = CardsX + i % 2 * (CardW + CardGapX), y = CardsY + i / 2 * (CardH + CardGapY);
            _cards.AddChild(MakeCard(offers[i], x, y));
        }
        // Offered after something sold off the display: the evening's own tips have had their turn.
        if (_built && offers.Any(u => u.Key == Upgrades.Engine)) Teach("engine");
        _built = true;

        BuildExhibition();
    }

    /// <summary>Down the right: the Guild's next due, this week's commissions, and your curios.</summary>
    void BuildSide()
    {
        foreach (var child in _side.GetChildren()) child.QueueFree();

        // The Guild, once Vane has introduced it. The first evening, only a hint of the visit to come.
        if (State.GuildIntroduced) BuildGuild();
        else
        {
            var watcher = UiKit.MakePanel(Palette.PanelBorder).Place(SideX, 182, SideW, 112);
            _side.AddChild(watcher);
            watcher.AddChild(new TextureRect { Texture = ArtTexture.Load("res://assets/ui/guild.png"), MouseFilter = Control.MouseFilterEnum.Ignore }
                .Place(14, 14, 44, 48));
            watcher.AddChild(UiKit.MakeLabel(State.Day <= 1 ? Loc.T("A pale man with a gilded chain has been watching the stall all day. He's still outside.")
                : Loc.T("The pale man with the gilded chain is back, and this time he's crossing the road to the stall."), 16, Palette.Muted)
                .Wrapped().Place(70, 12, SideW - 84, 88));
        }

        // This week's commissions.
        var board = UiKit.MakePanel(Palette.PanelBorder).Place(SideX, 302, SideW, 196);
        _side.AddChild(board);
        board.AddChild(UiKit.MakeLabel(Loc.T("COMMISSIONS"), 16, Palette.Gold.Darkened(0.2f)).Place(14, 10, 200, 20));
        if (State.GuildIntroduced)
            board.AddChild(UiKit.MakeLabel(Loc.T("until the Guild comes"), 16, Palette.Muted, HorizontalAlignment.Right).Place(150, 10, 204, 20));
        if (State.Commissions.Count == 0)
            board.AddChild(UiKit.MakeLabel(!State.GuildIntroduced ? Loc.T("Regulars will start asking you for things soon.")
                : State.Week < Patrons.FromWeek ? Loc.T("Regulars start asking for things once the Guild has been.")
                : Loc.T("Nobody's asking for anything this week."), 16, Palette.Muted).Wrapped().Place(14, 40, SideW - 28, 60));
        for (int i = 0; i < Math.Min(4, State.Commissions.Count); i++)
        {
            var c = State.Commissions[i];
            float y = 36 + i * 39;
            var color = c.Done ? Palette.Good : Palette.Text;
            string patron = c.Who;
            string reward = c.Done ? Loc.T("DONE") : c.Needed > 1 ? Loc.T($"{State.CommissionEach(c)}g each") : Money.Short(State.CommissionEach(c));
            // Clipped short of the reward on the right (a long name in another language ends in "...", whole on hover).
            var who = UiKit.MakeLabel(patron, 16, color).Place(14, y, 340 - UiKit.TextWidth(reward), 20);
            who.ClipText = true;
            who.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            who.TooltipText = patron;
            who.MouseFilter = Control.MouseFilterEnum.Pass;
            board.AddChild(who);
            board.AddChild(UiKit.MakeLabel(reward, 16, c.Done ? Palette.Good : Palette.Gold, HorizontalAlignment.Right).Place(180, y, 174, 20));
            board.AddChild(UiKit.MakeLabel($"{c.What} {c.Progress}", 16, Palette.Muted).Place(14, y + 18, SideW - 28, 20));
        }

        // Curios.
        var curios = UiKit.MakePanel(Palette.PanelBorder).Place(SideX, 504, SideW, 78); // two lines of note under the title
        _side.AddChild(curios);
        curios.AddChild(UiKit.MakeLabel(Loc.T($"CURIOS {State.CurioKeys.Count}/{State.CurioSlots}"), 16, Palette.Magic).Place(14, 8, 120, 20));
        // A tale's keepsakes sit after the curios, ringed in their own colour, and take no slot: said under the count.
        if (State.Keepsakes.Count > 0 && State.CurioKeys.Count > 0)
            curios.AddChild(UiKit.MakeLabel(Loc.T($"KEEPSAKES {State.Keepsakes.Count}"), 16, Palette.Keepsake).Place(14, 30, 104, 20));
        if (State.CurioKeys.Count == 0 && State.Keepsakes.Count == 0)
            curios.AddChild(UiKit.MakeLabel(State.PeddlerToday ? Loc.T("Nim the peddler comes by tonight, after closing, with a curio.")
                : Loc.T("Nim the peddler comes by every night after closing, from the third."), 16, Palette.Muted)
                .Wrapped().Place(14, 28, SideW - 28, 44));
        // Up to six (with curio cases), and a tale's keepsakes after them: closer together when there are more than four.
        int shown = State.CurioKeys.Count + State.Keepsakes.Count;
        float step = Math.Min(60, (SideW - 124) / Math.Max(1, shown));
        for (int i = 0; i < State.CurioKeys.Count; i++)
        {
            var curio = Curios.Get(State.CurioKeys[i]);
            var spot = new Control { TooltipText = Glossary.Tooltip(curio), MouseFilter = Control.MouseFilterEnum.Pass }
                .Place(120 + i * step, 6, step - 4, 62);
            spot.AddChild(UiKit.MakeCurioIcon(curio, new Vector2((step - 4) / 2, 31)));
            curios.AddChild(spot);
        }
        for (int i = 0; i < State.Keepsakes.Count; i++)
        {
            var keepsake = Keepsakes.Get(State.Keepsakes[i]);
            var spot = new Control { TooltipText = UiKit.KeepsakeTooltip(keepsake), MouseFilter = Control.MouseFilterEnum.Pass }
                .Place(120 + (State.CurioKeys.Count + i) * step, 6, step - 4, 62);
            spot.AddChild(UiKit.MakeKeepsakeIcon(keepsake, new Vector2((step - 4) / 2, 31), Math.Min(56, step - 4)));
            curios.AddChild(spot);
        }
    }

    /// <summary>
    /// The Guild's next due: how much, when, and how close you are; what's left of the debt in all, and (once you have
    /// it) a button to pay it all off and finish early.
    /// </summary>
    void BuildGuild()
    {
        int due = State.NextDue;
        bool tonight = State.DuesTonight && !State.PaidThisWeek;
        bool enough = State.Gold >= due;
        var guild = UiKit.MakePanel(tonight ? Palette.Warn.Darkened(0.2f) : Palette.PanelBorder).Place(SideX, 182, SideW, 112);
        _side.AddChild(guild);
        guild.AddChild(new TextureRect { Texture = ArtTexture.Load("res://assets/ui/guild.png"), MouseFilter = Control.MouseFilterEnum.Ignore }
            .Place(14, 14, 44, 48));
        string title = tonight ? Loc.T("VANE COMES TONIGHT") : State.DebtCleared ? Loc.T("THE GUILD'S INTEREST") : Loc.T("THE GUILD'S DUE");
        guild.AddChild(UiKit.MakeLabel(title, 16, tonight ? Palette.Warn : Palette.Muted).Place(70, 10, 290, 20));
        guild.AddChild(UiKit.MakeLabel(Money.Short(due), 32, enough ? Palette.Gold : Palette.Bad, outline: 4).Place(70, 28, 200, 40));
        string when = tonight ? Loc.T("tonight") : Loc.T($"end of day {State.Day + State.DaysToNextDue}");
        guild.AddChild(UiKit.MakeLabel(when, 16, Palette.Muted, HorizontalAlignment.Right).Place(180, 32, 176, 20));
        if (!State.DebtCleared)
            guild.AddChild(UiKit.MakeLabel(Loc.T($"debt left {State.DebtLeft}g"), 16, Palette.Muted, HorizontalAlignment.Right).Place(160, 52, 196, 20));

        // Enough for everything that's left: finish now, however early.
        if (State.CanPayOff)
        {
            var payOff = UiKit.MakeButton(Loc.T($"Pay it all off: {State.DebtLeft}g"), Palette.Good, 16, ConfirmPayOff).Place(14, 72, SideW - 28, 32);
            payOff.TooltipText = Loc.T("Every week's due that's left of Bram's debt, to the last, all at once. The run is won; you can keep trading after.");
            guild.AddChild(payOff);
            return;
        }
        float fill = Math.Clamp(State.Gold / (float)Math.Max(1, due), 0, 1);
        guild.AddChild(new Painter(p =>
        {
            p.DrawRect(new Rect2(0, 0, SideW - 28, 14), new Color("#120e1a"));
            p.DrawRect(new Rect2(0, 0, (SideW - 28) * fill, 14), (enough ? Palette.Gold : Palette.Bad).Darkened(0.25f));
            p.DrawRect(new Rect2(0, 0, SideW - 28, 14), Palette.PanelBorder, false, 2);
        }) { Position = new Vector2(14, 74) });
        string week = State.DebtCleared ? Loc.T("Bram's debt is paid") : State.Week <= Guild.Weeks ? Loc.T($"Week {State.Week} of {Guild.Weeks}") : Loc.T($"Week {State.Week}");
        guild.AddChild(UiKit.MakeLabel(enough ? Loc.T($"{week}. You have it.") : Loc.T($"{week}. {due - State.Gold}g short."), 16,
            enough ? Palette.Muted : Palette.Warn).Place(14, 88, SideW - 28, 20));
    }

    Control _confirm;

    /// <summary>Paying off the debt ends the run's story: say what it costs and ask first.</summary>
    public void ConfirmPayOff()
    {
        if (_confirm != null || !State.CanPayOff) return;
        int left = State.DebtLeft;
        _confirm = Add(new Control { ZIndex = 50 }.Place(0, 0, 1280, 720));
        _confirm.AddChild(new ColorRect { Color = new Color(0, 0, 0, 0.6f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920));
        var panel = UiKit.MakePanel(Palette.Good.Darkened(0.2f)).Place(330, 210, 620, 270);
        _confirm.AddChild(panel);
        panel.AddChild(UiKit.MakeLabel(Loc.T("PAY OFF THE DEBT?"), 32, Palette.Good, HorizontalAlignment.Center, outline: 4).Place(0, 20, 620, 40));
        int early = Guild.Weeks - State.Week;
        string weeks = State.Week < Guild.Weeks ? Loc.Plural(early, $", {early} week early", $", {early} weeks early") : "";
        panel.AddChild(UiKit.MakeLabel(Loc.T($"Pay the Guild {left}g now: every week's due that's left, to the last. Bram's debt is cleared{weeks}, and you'd have {State.Gold - left}g left. You can keep trading after."), 16, Palette.Text, HorizontalAlignment.Center)
            .Wrapped().Place(40, 74, 540, 90));
        panel.AddChild(UiKit.MakeButton(Loc.T("Pay it off"), Palette.Good, 24, PayOff).Place(60, 184, 240, 56));
        panel.AddChild(UiKit.MakeButton(Loc.T("Not yet"), Palette.PanelBorder.Lightened(0.2f), 24, CancelPayOff).Place(320, 184, 240, 56));
        Game.Moment("09_payoff", 0.4);
    }

    public void CancelPayOff()
    {
        _confirm?.QueueFree();
        _confirm = null;
    }

    public void PayOff()
    {
        if (_confirm == null) return;
        CancelPayOff();
        Game.PayOffDebt();
    }

    /// <summary>What's on display, what it does for the shop, and a click to sell any of it.</summary>
    void BuildExhibition()
    {
        const float x0 = CardsX, y0 = 562;
        BuildSongs(x0, y0);
        if (!State.DisplayOpen) return;
        int score = State.Exhibition;
        string effect = score == 0 ? Loc.T("Keep grand things when you open a chest, and they'll show here.")
            : Loc.Plural(State.ExhibitionCustomers,
                $"{score} points: +{State.ExhibitionCustomers} customer a day, better chests {Synergies.Pct(State.BetterChests)} of the time.",
                $"{score} points: +{State.ExhibitionCustomers} customers a day, better chests {Synergies.Pct(State.BetterChests)} of the time.");
        var shown = State.Display.Where(i => !i.IsSong).ToList();
        _cards.AddChild(UiKit.MakeLabel(Loc.T($"EXHIBITION {shown.Count}/{State.DisplaySlots}"), 16, Palette.Magic).Place(x0, y0, 300, 20));
        _cards.AddChild(UiKit.MakeLabel(effect, 16, Palette.Muted).Place(x0 + 200, y0, 616, 20));
        for (int i = 0; i < State.DisplaySlots; i++)
        {
            bool filled = i < shown.Count;
            var slot = UiKit.MakePanel(filled ? Palette.Of(shown[i].Rarity) : Palette.PanelBorder).Place(x0 + i * 56, y0 + 26, 50, 50);
            slot.MouseFilter = Control.MouseFilterEnum.Pass;
            _cards.AddChild(slot);
            if (!filled) continue;
            var item = shown[i];
            slot.AddChild(UiKit.MakeItemIcon(item, new Vector2(25, 25), 44));
            var sell = new Button { Flat = true, FocusMode = Control.FocusModeEnum.None, TooltipText = Loc.T($"{item.Name}, {State.Points(item)} points{(item.Magic ? Loc.T(" (double: enchanted)") : "")}. Click to sell it for {State.ShelfValue(item)}g.") };
            sell.Pressed += () => AskSell(item);
            slot.AddChild(sell.Place(0, 0, 50, 50));
        }
        string found = Loc.T($"Collection: {State.Found.Count} of {Collection.Size} found.");
        string book = State.BookCoins > 0 ? Loc.T($" Visitors leave {State.BookCoins}g a night.")
            : State.Admission > 0 ? Loc.T($" Visitors pay {State.Admission}g a night at the door.") : "";
        _cards.AddChild(UiKit.MakeLabel(found + book, 16, Palette.Muted).Place(x0, y0 + 84, 816, 20));
    }

    /// <summary>
    /// The Heartsongs singing in the shop, to the right of the exhibition (or alone, before there's a shelf): what each
    /// does, and a click to sell it (it goes back to the Locking, and its effect with it).
    /// </summary>
    void BuildSongs(float x0, float y0)
    {
        var songs = State.Display.Where(i => i.IsSong).ToList();
        if (songs.Count == 0) return;
        float x = State.DisplayOpen ? x0 + 816 - songs.Count * 56 : x0;
        var red = Palette.Of(Rarity.Mythic);
        _cards.AddChild(UiKit.MakeLabel(Loc.T("HEARTSONGS"), 16, red, State.DisplayOpen ? HorizontalAlignment.Right : HorizontalAlignment.Left)
            .Place(State.DisplayOpen ? x0 + 516 : x0, y0 + (State.DisplayOpen ? 84 : 0), 300, 20));
        for (int i = 0; i < songs.Count; i++)
        {
            var item = songs[i];
            var slot = UiKit.MakePanel(red).Place(x + i * 56, y0 + 26, 50, 50);
            slot.MouseFilter = Control.MouseFilterEnum.Pass;
            _cards.AddChild(slot);
            slot.AddChild(UiKit.MakeItemIcon(item, new Vector2(25, 25), 44));
            var sell = new Button { Flat = true, FocusMode = Control.FocusModeEnum.None,
                TooltipText = Loc.T($"{item.Name}, singing: {item.SongDef.Effect} Click to let it go (it fetches nothing).") };
            sell.Pressed += () => AskSell(item);
            slot.AddChild(sell.Place(0, 0, 50, 50));
        }
    }

    /// <summary>Sell something on display (or let a Heartsong go)? Asked first: it comes off display, and what it did with it.</summary>
    public void AskSell(Item item)
    {
        if (_confirm != null || !State.Display.Contains(item)) return;
        int value = State.ShelfValue(item);
        string text = item.IsSong
            ? Loc.T($"{item.Name} goes back to the Locking, and takes this with it: {item.SongDef.Effect} It fetches nothing.")
            : Loc.T($"Sell the {item.Name} for {value}g? It comes off display: {State.Points(item)} exhibition points fewer.");
        _confirm = Add(UiKit.MakeConfirm(item.IsSong ? Loc.T("LET IT GO?") : Loc.T("SELL IT?"), text, item.IsSong ? Palette.Of(Rarity.Mythic) : Palette.Gold,
            item.IsSong ? Loc.T("Let it go") : Loc.T($"Sell: +{value}g"), () => SellFromDisplay(item), Loc.T("Keep it"), CancelPayOff));
    }

    public void SellFromDisplay(Item item)
    {
        CancelPayOff();
        if (!State.Display.Contains(item)) return;
        int value = State.ShelfValue(item);
        State.SellFromDisplay(item);
        Game.Hud.SetGold(State.Gold);
        if (item.IsSong) Sfx.Play("deflate", 0.8f, -4);
        else Sfx.PlayGroup("pay");
        FloatText(item.IsSong ? Loc.T($"{item.Name} goes back to the Locking.") : Loc.T($"+{value}g"), new Vector2(640, 560), item.IsSong ? Palette.Of(Rarity.Mythic) : Palette.Gold, 24);
        BuildCards();
    }

    /// <summary>Before renovations: what the stall has earned so far, against what it needs, and what's coming.</summary>
    Control MakeGoal()
    {
        const float w = 2 * CardW + CardGapX, h = 168;
        var card = UiKit.MakePanel(Palette.Gold.Darkened(0.2f)).Place(CardsX, CardsY, w, h);
        card.AddChild(new TextureRect { Texture = ArtTexture.Load("res://assets/ui/icons/shop2.png"), MouseFilter = Control.MouseFilterEnum.Ignore }
            .Place(20, 20, 48, 48));
        card.AddChild(UiKit.MakeLabel(Loc.T("Earn your keep"), 24, Palette.Gold).Place(84, 16, 600, 30));
        card.AddChild(UiKit.MakeLabel(Loc.T($"Once the stall has earned {GameState.UpgradesAt}g, all told, you can move on to Thornwick Market: more customers, richer chests and a shelf for your best finds."), 16, Palette.Muted)
            .Wrapped().Place(84, 50, w - 110, 44));
        int earned = Math.Clamp(State.TotalEarned, 0, GameState.UpgradesAt);
        float fill = earned / (float)GameState.UpgradesAt;
        card.AddChild(new Painter(p =>
        {
            p.DrawRect(new Rect2(0, 0, w - 104, 24), new Color("#120e1a"));
            p.DrawRect(new Rect2(0, 0, (w - 104) * fill, 24), Palette.Gold.Darkened(0.25f));
            p.DrawRect(new Rect2(0, 0, (w - 104) * fill, 5), Palette.Gold.Lightened(0.2f));
            p.DrawRect(new Rect2(0, 0, w - 104, 24), Palette.PanelBorder, false, 3);
        }) { Position = new Vector2(84, 112) });
        card.AddChild(UiKit.MakeLabel(Loc.T($"The chests have brought in {State.TotalEarned}g of {GameState.UpgradesAt}g"), 16, Palette.Text, HorizontalAlignment.Center,
            outline: 4).Place(84, 114, w - 104, 20));
        return card;
    }

    /// <summary>
    /// An upgrade: icon, name, what it does, and its price (a Buy button once you can afford it, red, with a question
    /// first, if it would leave you short for the Guild tonight or for two of the shop's typical chests after it).
    /// </summary>
    Control MakeCard(Upgrade upgrade, float x, float y)
    {
        int cost = State.Cost(upgrade);
        bool affordable = State.Gold >= cost, engine = upgrade.Key == Upgrades.Engine;
        int left = State.Gold - cost;
        string warning = engine ? null : State.ShortNote(cost, upgrade);
        var card = UiKit.MakePanel(upgrade.IsRenovation || engine ? Palette.Gold : Palette.PanelBorder).Place(x, y, CardW, CardH);
        card.AddChild(new TextureRect
        {
            Texture = ArtTexture.Load($"res://assets/ui/icons/{upgrade.Key}.png"),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        }.Place(12, 12, 48, 48));
        // A long name steps down a size, so the price beside it never covers it.
        bool small = upgrade.Name.Length > 16 || UiKit.TextWidth(upgrade.Name, 24) > 216;
        card.AddChild(UiKit.MakeLabel(upgrade.Name, small ? 16 : 24, upgrade.IsRenovation || engine ? Palette.Gold : Palette.Text)
            .Place(70, small ? 12 : 8, 216, 30));
        card.AddChild(UiKit.MakeLabel(upgrade.Blurb, 16, Palette.Muted).Wrapped().Place(70, 40, CardW - 82, 66));
        if (affordable)
        {
            var buy = UiKit.MakeButton(Money.Short(cost), warning != null ? Palette.Bad : Palette.Good, 16, () => AskBuy(upgrade)).Place(CardW - 112, 8, 100, 30);
            buy.TooltipText = engine ? Loc.T($"Every chest read for you, for good. You'd still owe the Guild {State.DebtLeft}g, and have {left}g to earn it with.")
                : warning ?? Loc.T($"Buy the {upgrade.Name}. Leaves you {left}g.");
            card.AddChild(buy);
        }
        else
        {
            var price = UiKit.MakeLabel(Money.Short(cost), 24, Palette.Muted, HorizontalAlignment.Right).Place(CardW - 132, 6, 120, 30);
            price.TooltipText = Loc.T($"{cost - State.Gold}g more to go");
            price.MouseFilter = Control.MouseFilterEnum.Pass;
            card.AddChild(price);
        }
        return card;
    }

    /// <summary>Buys it, or first asks if it would leave you short (<see cref="GameState.ShortNote"/>).</summary>
    public void AskBuy(Upgrade upgrade)
    {
        if (_confirm != null || !State.CanBuy(upgrade)) return;
        string warning = upgrade.Key == Upgrades.Engine ? null : State.ShortNote(State.Cost(upgrade), upgrade);
        if (warning == null)
        {
            Buy(upgrade);
            return;
        }
        _confirm = Add(UiKit.MakeConfirm(Loc.T("ARE YOU SURE?"), warning, Palette.Bad, Loc.T("Buy anyway"), () =>
        {
            CancelPayOff();
            Buy(upgrade);
        }, Loc.T("Not now"), CancelPayOff));
        Game.Moment("09_short", 0.4);
    }

    public void Buy(Upgrade upgrade)
    {
        if (!State.CanBuy(upgrade)) return;
        int cost = State.Cost(upgrade);
        State.BuyUpgrade(upgrade);
        Game.Hud.SetGold(State.Gold);
        Game.Hud.SetDay(State.Day, State.Tier);
        Sfx.PlayGroup("pay");
        if (upgrade.IsRenovation)
        {
            Sfx.Play("fanfare", 1f, -4);
            Game.Flash(Palette.Gold, 0.35f);
            Game.Shake(0.3f);
            FloatText($"{ShopTiers.Name(State.Tier)}!", new Vector2(640, 150), Palette.Gold, 40);
            Unlocked();
        }
        else if (upgrade.SynergySlots > 0)
        {
            RefreshSynergyButton();
            if (Teach("slots")) Pulse(_synergies);
        }
        else if (upgrade.Customers > 1 || upgrade.Commissions > 0 || upgrade.CurioSlots > 0 || upgrade.Sales > 0 || upgrade.Key == Upgrades.Engine)
        {
            Sfx.Play("upgrade", 1f, -4);
            Game.Flash(Palette.Gold, 0.2f);
            FloatText($"{upgrade.Name}!", new Vector2(640, 150), Palette.Gold, 32);
        }
        else
        {
            Sfx.Play("sting", 1.2f, -4);
            FloatText("-" + Money.Short(cost), new Vector2(640, 150), Palette.Gold, 32);
        }
        BuildCards();
    }

    /// <summary>All the upgrades this player would buy in order, keeping enough back for the Guild (the bot uses this).</summary>
    public void BuyInOrder(string[] order)
    {
        foreach (var key in order)
        {
            var upgrade = Upgrades.Get(key);
            if (Strategy.Affordable(State, upgrade)) Buy(upgrade);
        }
    }

    /// <summary>The button says how many synergies are active, and stands out when a reached one is waiting for a slot.</summary>
    void RefreshSynergyButton()
    {
        int reached = Synergies.All.Count(x => x.Level(State) > 0), active = State.ActiveSynergies.Count;
        _synergies.Text = reached == 0 ? Loc.T("Synergies: none reached yet") : Loc.T($"Synergies: {active} of {State.SynergySlots} active");
        // From the town shop on it's always there, even before the first is reached: the journal shows how close you are.
        _synergies.Visible = State.SynergiesOpen;
        bool waiting = active < State.SynergySlots && reached > active;
        _synergies.Modulate = waiting ? new Color(1.3f, 1.3f, 1.3f) : Colors.White;
        if (waiting) _synergies.Text += Loc.T(" (choose!)");
    }

    /// <summary>Draws the eye to a button Bram has just mentioned.</summary>
    static void Pulse(Control control)
    {
        control.PivotOffset = control.Size / 2;
        var tween = control.CreateTween().SetLoops(3);
        tween.TweenProperty(control, "scale", Vector2.One * 1.06f, 0.25f);
        tween.TweenProperty(control, "scale", Vector2.One, 0.25f);
    }

    /// <summary>Opens the journal on the synergies page, where they can be switched on and off.</summary>
    public void OpenSynergies()
    {
        if (_notebook != null) return;
        _notebook = Add(new NotebookView(State, NotebookView.SynergiesPage, choosing: true) { ZIndex = 30 });
        _notebook.Closed += () =>
        {
            _notebook = null;
            RefreshSynergyButton();
            BuildCards();
        };
        Game.Moment("09_synergies", 0.4);
    }

    public void CloseSynergies() => _notebook?.Close();

    /// <summary>N opens the notebook here too, at the calendar (what's coming), as it does at the counter.</summary>
    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed(Controls.Notebook) || _notebook != null) return;
        GetViewport().SetInputAsHandled();
        OpenNotebook(NotebookView.CalendarPage);
    }

    void OpenNotebook(int page)
    {
        _notebook = Add(new NotebookView(State, page) { ZIndex = 30 });
        _notebook.Closed += () => _notebook = null;
    }

    /// <summary>A note over the top bar was clicked: the notebook at the Inquiry.</summary>
    public override bool ShowInquiry()
    {
        if (_notebook != null || HistoryView.Open > 0) return false;
        OpenNotebook(NotebookView.InquiryPage);
        return true;
    }

    /// <summary>The top bar's due was clicked: the notebook at the Guild's debt (where paying it off is weighed).</summary>
    public override bool ShowDebt()
    {
        if (_notebook != null || HistoryView.Open > 0) return false;
        _notebook = Add(new NotebookView(State, NotebookView.CalendarPage, focusDebt: true) { ZIndex = 30 });
        _notebook.Closed += () => _notebook = null;
        Game.Moment("09_debt", 0.4);
        return true;
    }

    /// <summary>Keeps the highest synergies active, the way the balance simulation's careful player does (the bot uses this).</summary>
    public void ChooseSynergies()
    {
        Debug.Simulation.ChooseSynergies(State);
        _notebook?.ShowPage(NotebookView.SynergiesPage);
        RefreshSynergyButton();
    }

    public void NextDay()
    {
        Sfx.Play("bid");
        Game.CloseDay();
    }
}
