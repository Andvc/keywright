using System;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// The notebook: what Bram's kin keep for trading (N, the button on the slip, or clicking whoever's at the counter).
/// Sounds: one row per kind of thing your shop sees (its sound, how hard you must shake to hear it, how heavy it is, and
/// what one is worth if it's as good as each colour of chest promises; click a row to hear it), and the rules of
/// listening. Regulars: how each seller you've met judges a chest, trades and behaves (whoever's at the counter framed),
/// and Uncle Bram and the story's people on the Friends spread. Calendar: the Guild's debt (what's paid, what's left and
/// what's next: the purse's due in the top bar opens it here), today, the Guild's nights, the days Nim comes by, and
/// what's happening in town this week and next. Synergies: what the shop could be known for, how far along each is, and
/// (at the end of a day) which are active. What any run has learned stays in it: the sounds and regulars of the grandest
/// shop any run reached (<see cref="GameState.JournalTier"/>). What's been gathered (finds, curios, Heartsongs) is the
/// collection's (<see cref="CollectionView"/>).
/// </summary>
public partial class NotebookView : Node2D
{
    // Ends above Uncle Bram's box, so his tips stay readable with the notebook open.
    const float PanelX = 40, PanelY = 66, PanelW = 1200, PanelH = 518;
    const float TableX = 64, RowH = 40;

    public const int SoundsPage = 0, RegularsPage = 1, CalendarPage = 2, SynergiesPage = 3, TownPage = 4, StoriesPage = 5, InquiryPage = 6;

    readonly GameState _state;
    readonly int _firstPage;
    readonly bool _choosing;

    /// <summary>Whoever's at the counter: the Regulars page opens at their place, their entry framed.</summary>
    readonly string _current;

    /// <summary>Opened from the purse's due in the top bar: the calendar's debt is framed, and breathes for a moment.</summary>
    readonly bool _focusDebt;
    Node2D _page;
    bool _closing;

    public event Action Closed;

    public NotebookView() { }

    /// <param name="choosing">Synergies can be switched on and off (at the end of a day).</param>
    /// <param name="current">The seller at the counter, if any.</param>
    /// <param name="focusDebt">Opened to look at the debt.</param>
    public NotebookView(GameState state, int page = SoundsPage, bool choosing = false, string current = null, bool focusDebt = false)
    {
        _state = state;
        _firstPage = page;
        _choosing = choosing;
        _current = current;
        _focusDebt = focusDebt;
    }

    static Audio.Sfx Sfx => Main.Instance.Sfx;

    /// <summary>How many notebooks are open: the notes over the top bar (which would cover its tabs) wait meanwhile.</summary>
    public static int Open { get; private set; }

    public override void _EnterTree() => Open++;

    public override void _ExitTree() => Open = Math.Max(0, Open - 1);

    public override void _Ready()
    {
        // What a note over the top bar says is on these pages: it gives way to them.
        Main.Instance?.HideNote();
        var blocker = new ColorRect { Color = new Color(0, 0, 0, 0.7f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920);
        blocker.GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true }) Close();
        };
        AddChild(blocker);
        AddChild(UiKit.MakePanel(Palette.Gold.Darkened(0.3f)).Place(PanelX, PanelY, PanelW, PanelH));
        AddChild(UiKit.MakeLabel(Loc.T("NOTEBOOK"), 32, Palette.Gold, outline: 4).Place(PanelX + 24, PanelY + 10, 220, 40));
        Tab(SoundsPage, Loc.T("Sounds"), PanelX + 236, 96);
        Tab(RegularsPage, Loc.T("Regulars"), PanelX + 338, 112);
        Tab(CalendarPage, Loc.T("Calendar"), PanelX + 456, 112);
        Tab(TownPage, Loc.T("Town"), PanelX + 574, 88);
        Tab(StoriesPage, Loc.T("Stories"), PanelX + 668, 104);
        Tab(InquiryPage, Loc.T("Inquiry"), PanelX + 778, 88);
        // Synergies come with Crownhold: before that, the notebook just keeps its tally.
        if (_state.SynergiesOpen) Tab(SynergiesPage, Loc.T("Synergies"), PanelX + 872, 120);
        AddChild(UiKit.MakeButton(Loc.T($"Close {Controls.Tag(Controls.Notebook)}"), Palette.Text, 16, Close).Place(PanelX + PanelW - 144, PanelY + 12, 120, 40));
        _page = new Node2D();
        AddChild(_page);
        ShowPage(_firstPage);

        Sfx.PlayGroup("book", 1f, -4);
        Modulate = Colors.Transparent;
        CreateTween().TweenProperty(this, "modulate", Colors.White, 0.15f);
    }

    readonly System.Collections.Generic.Dictionary<int, Button> _tabs = new();

    void Tab(int page, string name, float x, float w)
    {
        _tabs[page] = UiKit.MakeButton(name, Palette.Text, 16, () => ShowPage(page)).Place(x, PanelY + 14, w, 36);
        AddChild(_tabs[page]);
    }

    /// <summary>0: sounds, 1: regulars, 2: calendar (and the debt), 3: synergies.</summary>
    public void ShowPage(int page)
    {
        foreach (var child in _page.GetChildren()) child.QueueFree();
        if (page == SynergiesPage && !_state.SynergiesOpen) page = SoundsPage;
        // The page that's open is the tab that's lit.
        foreach (var (key, tab) in _tabs) tab.Modulate = key == page ? Colors.White : new Color(1, 1, 1, 0.6f);
        if (page == RegularsPage) BuildRegulars(CurrentSpread);
        else if (page == CalendarPage) BuildCalendar();
        else if (page == TownPage) BuildTown();
        else if (page == StoriesPage) BuildStories();
        else if (page == InquiryPage) BuildInquiry();
        else if (page == SynergiesPage) BuildSynergies();
        else BuildSounds();
    }

    /// <summary>
    /// Each kind of synergy on a row: what it's about, its level (five pips), what its level does (or would do), what
    /// the next level needs, and whether it's active. At the end of a day, a button switches it on or off.
    /// </summary>
    void BuildSynergies()
    {
        int slots = _state.SynergySlots, active = _state.ActiveSynergies.Count;
        string intro = _choosing
            ? Loc.T($"Active: {active} of {slots}. Switch one on to put it to work; more slots come with a Crownhold Charter and a Royal Seal.")
            : Loc.T($"Active: {active} of {slots}. You choose at the end of a day.");
        _page.AddChild(UiKit.MakeLabel(intro, 16, Palette.Muted).Place(TableX, PanelY + 60, PanelW - 48, 20));
        // Ten synergies are more than the page holds: the rows scroll (the wheel, or the right stick), each as tall as
        // its words (a long effect takes three lines).
        float top = PanelY + 88, rowW = PanelW - 48;
        var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }
            .Place(TableX - 8, top, PanelW - 32, PanelY + PanelH - 12 - top);
        _page.AddChild(scroll);
        var list = new Control { MouseFilter = Control.MouseFilterEnum.Pass };
        scroll.AddChild(list);
        float y = 0;
        for (int i = 0; i < Synergies.All.Length; i++)
        {
            var synergy = Synergies.All[i];
            int level = synergy.Level(_state);
            bool on = _state.ActiveSynergies.Contains(synergy.Key);
            bool later = synergy.Tier > _state.Tier && level == 0;
            string effect = level > 0 ? synergy.Effect(level) : Loc.T($"Level 1: {synergy.Effect(1)}");
            var progress = synergy.Progress(_state).ToList();
            string next = progress.Count == 0 ? Loc.T("Highest level!") : Loc.T($"Next: {string.Join("\n", progress)}");
            float textH = Math.Max(UiKit.TextHeight(effect, 480), UiKit.TextHeight(next, 196));
            float rowH = Math.Max(54, textH + 12);
            var row = new Panel { MouseFilter = Control.MouseFilterEnum.Ignore }.Place(0, y, rowW, rowH);
            y += rowH + 4;
            row.AddThemeStyleboxOverride("panel", UiKit.PanelBox(4));
            if (on) row.AddChild(UiKit.MakeFrame(Palette.Magic));
            list.AddChild(row);
            var nameColor = later ? Palette.Muted.Darkened(0.2f) : on ? Palette.Magic : level > 0 ? Palette.Gold : Palette.Text;
            row.AddChild(UiKit.MakeLabel(synergy.Name, 16, nameColor).Place(12, 6, 220, 20));
            row.AddChild(UiKit.MakeLabel(later ? Loc.T($"{synergy.Blurb} (later)") : synergy.Blurb, 16, Palette.Muted).Place(12, 28, 220, 20));
            var pips = new Painter(p =>
            {
                for (int l = 0; l < SynergyDef.MaxLevel; l++)
                {
                    var c = l < level ? Palette.Of((Rarity)l) : Palette.Muted.Darkened(0.4f);
                    p.DrawRect(new Rect2(l * 22, 0, 16, 16), new Color(0, 0, 0, 0.5f));
                    if (l < level) p.DrawRect(new Rect2(l * 22 + 3, 3, 10, 10), c);
                    p.DrawRect(new Rect2(l * 22, 0, 16, 16), c, false, 2);
                }
            }) { Position = new Vector2(240, 18) };
            row.AddChild(pips);
            row.AddChild(UiKit.MakeLabel(effect, 16, level > 0 ? Palette.Text : Palette.Muted).Wrapped().Place(360, 6, 480, rowH - 8));
            row.AddChild(UiKit.MakeLabel(next, 16, Palette.Muted).Wrapped().Place(852, 6, 196, rowH - 8));
            if (_choosing && level > 0)
            {
                bool canTurnOn = on || active < slots;
                var toggle = UiKit.MakeButton(on ? Loc.T("Active") : Loc.T("Use"), on ? Palette.Magic : Palette.Good, 16, () =>
                {
                    bool now = _state.ToggleSynergy(synergy.Key);
                    Sfx.Play(now ? "sting" : "tick", now ? 1.3f : 1f, -4);
                    ShowPage(SynergiesPage);
                }).Place(1060, 10, 100, 34);
                toggle.Disabled = !canTurnOn;
                if (!canTurnOn) toggle.TooltipText = Loc.T("No free slot: switch another one off first.");
                row.AddChild(toggle);
            }
            else if (on) row.AddChild(UiKit.MakeLabel(Loc.T("Active"), 16, Palette.Magic, HorizontalAlignment.Center).Place(1060, 18, 100, 20));
        }
        list.CustomMinimumSize = new Vector2(rowW, y - 4);
    }

    void BuildSounds()
    {
        int tier = _state.JournalTier;
        float y = PanelY + 60;
        Header(Loc.T("Kind"), TableX + 48, y);
        Header(Loc.T("Sound"), TableX + 220, y);
        Header(Loc.T("Shake"), TableX + 330, y);
        Header(Loc.T("Weight"), TableX + 420, y);
        Header(Loc.T("Note"), TableX + 520, y);
        // Right-aligned over its columns, so it clears the tabs above (0.34 added one).
        Header(Loc.T("If it's as good as the chest:"), TableX + 760, y - 20, 4 * 104 - 8, HorizontalAlignment.Right);
        for (int c = 0; c < 4; c++) Header(Categories.ColourName((Rarity)c), TableX + 760 + c * 104, y);

        var kinds = Categories.UpTo(tier).ToList();
        for (int i = 0; i < kinds.Count; i++) _page.AddChild(MakeRow(kinds[i], y + 24 + i * RowH, tier));

        var rules = new[]
        {
            Loc.T("Light things rattle at a gentle shake, weapons at a firm one, soft things only at a hard one. Click a row to hear it."),
            Loc.T("While weapons clank, delicate things go unheard: shake gently to hear past it."),
            Loc.T("A strength you haven't tried on a chest costs a listen, and so does a drop. A hard shake stirs everything."),
            tier >= 2 ? Loc.T("Dropped, a chest with a false bottom booms hollow. Glass may break.") : null,
            tier >= 3 ? Loc.T("Two or three of a kind each sound from their own spot: count them.") : null,
            // The fakes share a line (0.36: with the ear's rule the page had one line too many at the Emporium).
            tier >= 3 ? Loc.T($"Sand rustles like a scroll, but pours at a gentle shake.{((tier >= 4 ? Loc.T(" Demon gold is iron under the gilt: a lodestone silences it.") : ""))}") : null,
            tier >= 3 ? Loc.T($"Anything can be enchanted: it sounds the same, but glows under a witchlight. Worth x{Categories.MagicMultiplier}."): null,
        }.Where(r => r != null).ToArray();
        // Seven rules at most (0.36: the ear's), wrapped (a longer language takes more lines): room down to the panel's foot.
        _page.AddChild(UiKit.MakeLabel(string.Join("\n", rules.Select(r => "- " + r)), 16, Palette.Text).Wrapped()
            .Place(TableX, y + 34 + kinds.Count * RowH, PanelW - 48, 180));
    }

    void Header(string text, float x, float y, float width = 200, HorizontalAlignment align = HorizontalAlignment.Left) =>
        _page.AddChild(UiKit.MakeLabel(text, 16, Palette.Gold.Darkened(0.2f), align).Place(x, y, width, 20));

    Control MakeRow(CategoryDef kind, float y, int tier)
    {
        var row = new Panel { MouseFilter = Control.MouseFilterEnum.Stop }.Place(TableX - 8, y, PanelW - 32, RowH - 4);
        row.AddThemeStyleboxOverride("panel", UiKit.PanelBox(4));
        row.AddChild(UiKit.MakeKindIcon(kind.Category, new Vector2(28, (RowH - 4) / 2)));
        row.AddChild(UiKit.MakeLabel(kind.Name, 16, Palette.Text).Place(56, 8, 160, 20));
        row.AddChild(UiKit.MakeLabel(kind.Word, 16, Palette.Word).Place(228, 8, 100, 20));
        row.AddChild(UiKit.MakeLabel(Categories.LevelName(kind.StirsAt), 16, Palette.Level(kind.StirsAt)).Place(338, 8, 100, 20));
        row.AddChild(UiKit.MakeLabel(Categories.HeftWord(kind.Category), 16, Palette.Text).Place(428, 8, 100, 20));
        string note = kind.Loud ? Loc.T("loud: drowns delicate") : kind.Delicate ? Loc.T("delicate: clanks drown it") : Loc.T("soft: hard shake only");
        if (kind.FakeName != null && kind.FakeIron && tier >= 4) note = Loc.T("delicate; demon gold apes it");
        else if (kind.FakeName != null && !kind.FakeIron && tier >= 3) note = Loc.T("soft; sand sounds alike");
        row.AddChild(UiKit.MakeLabel(note, 16, Palette.Muted).Place(528, 8, 236, 20));
        for (int c = 0; c < 4; c++)
            row.AddChild(UiKit.MakeLabel(Money.Short(Categories.Nominal(kind.Category, (Rarity)c)), 16, Palette.Of((Rarity)c)).Place(768 + c * 104, 8, 96, 20));
        row.GuiInput += e =>
        {
            if (e is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) return;
            Sfx.PlayGroup(Audio.SoundSets.Group(kind.Category), kind.Pitch, 2f);
            row.PivotOffset = row.Size / 2;
            var bump = row.CreateTween();
            bump.TweenProperty(row, "scale", Vector2.One * 1.02f, 0.05f);
            bump.TweenProperty(row, "scale", Vector2.One, 0.12f);
        };
        return row;
    }

    /// <summary>Which spread of the regulars a shop tier's sellers are on: one per place.</summary>
    static int Spread(int tier) => Math.Clamp(tier, 1, ShopTiers.Max) - 1;

    /// <summary>The Friends spread, after the four places: Uncle Bram and the story's people.</summary>
    const int FriendsSpread = ShopTiers.Max;

    /// <summary>The spread to open on: the place of whoever's at the counter, or this shop's.</summary>
    int CurrentSpread => Sellers.All.FirstOrDefault(s => s.Key == _current) is SellerDef seller ? Spread(seller.Tier) : Spread(_state.Tier);

    /// <summary>The person whose page is open below the list (whoever's at the counter, to begin with).</summary>
    string _selected;

    /// <summary>
    /// The regulars, a spread per place (every place any run has reached: what's learned stays learned), and the
    /// Friends spread; buttons along the bottom. Each person is a line in the list (whoever's at the counter framed in
    /// gold); click one for their page below it.
    /// </summary>
    void BuildRegulars(int spread)
    {
        int tier = _state.JournalTier;
        int total = Sellers.All.Count(s => s.Tier <= tier);
        int metCount = Sellers.All.Count(s => s.Tier <= tier && _state.Met.Contains(s.Key));
        _page.AddChild(UiKit.MakeLabel(Loc.T($"{metCount} of {total} met"), 16, Palette.Muted, HorizontalAlignment.Right)
            .Place(PanelX + PanelW - 224, PanelY + PanelH - 32, 200, 20));
        // A button a shop, each 180 wide or as wide as its name needs in a longer language.
        float buttonX = TableX;
        void SpreadButton(string name, Color color, Action open)
        {
            float width = Mathf.Max(180, UiKit.TextWidth(name) + 28);
            _page.AddChild(UiKit.MakeButton(name, color, 16, open).Place(buttonX, PanelY + PanelH - 44, width, 34));
            buttonX += width + 8;
        }
        for (int i = 0; i <= Spread(tier); i++)
        {
            int which = i;
            SpreadButton(ShopTiers.Name(i + 1), spread == i ? Palette.Gold : Palette.Text, () => ShowRegulars(which));
        }
        SpreadButton(Loc.T("Friends & foes"), spread == FriendsSpread ? Palette.Gold : Palette.Magic, () => ShowRegulars(FriendsSpread));

        var people = spread == FriendsSpread ? Friends() : Sellers.All.Where(s => s.Tier <= tier && Spread(s.Tier) == spread && _state.Met.Contains(s.Key))
            .Select(s => (s.Key, s.Name, Role: Loc.T($"{s.JudgeWord}; {Trade(s)}"), About: About(s))).ToList();
        if (people.Count == 0)
        {
            _page.AddChild(UiKit.MakeLabel(Loc.T("You haven't met anyone here yet."), 24, Palette.Muted, HorizontalAlignment.Center).Place(PanelX, 300, PanelW, 30));
            return;
        }
        if (people.All(p => p.Key != _selected)) _selected = people.Any(p => p.Key == _current) ? _current : people[0].Key;
        for (int i = 0; i < people.Count; i++) Entry(i, people[i].Key, people[i].Name, people[i].Role, spread);
        var chosen = people.First(p => p.Key == _selected);
        Page(chosen.Key, chosen.Name, chosen.Role, chosen.About);
    }

    /// <summary>Their page: how they trade, what they want, and what you've noticed about them.</summary>
    static string About(SellerDef seller)
    {
        string about = seller.Habit ?? "";
        if (seller.Wants != null) about += Loc.T($"\nWants: {seller.Wants}");
        if (seller.Facts.Length > 0) about += Loc.T($"\nYou've noticed: {string.Join(" ", seller.Facts)}");
        return about;
    }

    static string Trade(SellerDef seller) => seller.Trade switch { Core.Trade.Haggle => Loc.T("haggles"), Core.Trade.Offer => Loc.T("you name the price"), _ => Loc.T("fixed price") };

    /// <summary>Uncle Bram (always) and the story's people you've met: Nim, Mister Vane, the Hooded Woman, who's Mara.</summary>
    System.Collections.Generic.List<(string Key, string Name, string Role, string About)> Friends()
    {
        var people = new System.Collections.Generic.List<(string Key, string Name, string Role, string About)>
        {
            (Story.Bram, Hints.Mentor, Loc.T("your mentor"), Loc.T("Keywright at the Brindle crossroads for forty years, until his Key-mark faded and showed on " +
                                                  "your hand. Keeps a stash for bad days and a tip for everything. Never paid the Guild a penny.")),
        };
        if (_state.HasMet(Story.Nim))
            people.Add((Story.Nim, Peddler.Name, Loc.T("sells curios"), Loc.T("Comes by every night after the shutters are down, with oddments from the Locking, each " +
                                                             "bending one rule of the trade. Notices what you buy and brings more like it. Don't ask what Nim is.")));
        if (_state.HasMet(Story.Vane))
            people.Add((Story.Vane, Guild.Collector, Loc.T("the Gilded Guild"), Loc.T("Collector for the Guild that keeps the Gate. Comes at the end of every week for " +
                                                                    "the tithe Bram never paid. Miss one, and the Guild seals the shop.")));
        if (_state.HasMet(Story.Mara))
            people.Add((Story.Mara, Loc.T("Mara"), Loc.T("the Hooded Woman"), Loc.T("The last archmage of the Keyhold. She knows what the Keyhold put in the " +
                                                                                   "seal, and she can see on you that you remember.")));
        else if (_state.HasMet(Story.Hood))
            people.Add((Story.Hood, Story.HoodName, Loc.T("knows the Keyhold"), Loc.T("Knows Bram, and the Key-mark on your hand, and won't say who she is.")));
        // The people the tales have brought to the counter: a Warden, a clerk, a Mark-Finder, a mother.
        foreach (var person in Sellers.People.Where(p => _state.Met.Contains(p.Key)))
            people.Add((person.Key, person.Name, person.Of is Faction f ? Factions.Name(f) : "", About(person)));
        return people;
    }

    /// <summary>
    /// One person in the list, two to a row: face, name and how they deal. Whoever's at the counter is framed in gold, and
    /// the frame breathes; the one whose page is open below is marked. Click for their page.
    /// </summary>
    void Entry(int i, string key, string name, string role, int spread)
    {
        const float w = 566, h = 46;
        float x = TableX - 8 + i % 2 * 580, y = PanelY + 56 + i / 2 * 52;
        bool current = key == _current, selected = key == _selected;
        var row = UiKit.MakePanel(current ? Palette.Gold : selected ? Palette.Magic : Palette.PanelBorder).Place(x, y, w, h);
        row.MouseFilter = Control.MouseFilterEnum.Stop;
        row.MouseDefaultCursorShape = Control.CursorShape.PointingHand;
        row.TooltipText = Loc.T($"{name}: {role}. Click for their page.");
        row.GuiInput += e =>
        {
            if (e is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) return;
            _selected = key;
            Sfx.Play("tick", 1.2f, -8);
            ShowRegulars(spread);
        };
        _page.AddChild(row);
        if (current)
        {
            var pulse = row.CreateTween().SetLoops();
            pulse.TweenProperty(row, "self_modulate", new Color(1.35f, 1.3f, 1f), 0.7f);
            pulse.TweenProperty(row, "self_modulate", Colors.White, 0.7f);
        }
        var face = UiKit.MakeSprite($"res://assets/sprites/sellers/faces/{Sellers.LookOf(key)}.png", new Vector2(26, h / 2), 1, outlined: true);
        row.AddChild(face);
        row.AddChild(UiKit.MakeLabel(name, 16, current ? Palette.Gold : Palette.Text).Place(52, 4, 300, 20));
        row.AddChild(UiKit.MakeLabel(current ? Loc.T("at your counter") : role, 16, current ? Palette.Gold : Palette.Muted).Place(52, 24, 500, 20));
    }

    /// <summary>The open page under the list: a big face, the name, how they deal, and everything you know of them.</summary>
    void Page(string key, string name, string role, string about)
    {
        float y = PanelY + 56 + 4 * 52 + 6;
        var page = UiKit.MakePanel(key == _current ? Palette.Gold.Darkened(0.2f) : Palette.PanelBorder).Place(TableX - 8, y, PanelW - 32, PanelY + PanelH - 52 - y);
        page.MouseFilter = Control.MouseFilterEnum.Ignore;
        _page.AddChild(page);
        var portrait = UiKit.MakeSprite($"res://assets/sprites/sellers/{Sellers.LookOf(key)}.png", new Vector2(56, 58), 1, outlined: true);
        page.AddChild(portrait);
        page.AddChild(UiKit.MakeLabel(name, 24, Palette.Gold, outline: 4).Place(112, 10, 600, 30));
        page.AddChild(UiKit.MakeLabel(role, 16, Palette.Muted, HorizontalAlignment.Right).Place(600, 16, PanelW - 660, 20));
        page.AddChild(UiKit.MakeLabel(about, 16, Palette.Text).Wrapped().Place(112, 44, PanelW - 168, 144));
    }

    /// <summary>
    /// The Guild's debt across the top (once Mister Vane has told of it): what's left, what's next and when, and the five
    /// weeks of it, paid or to come. Under it, three weeks, a row each, a day to a box: today framed in gold and the days
    /// gone by faded. Each day's business (this week's and next week's are planned): a story morning, something on in
    /// town, Nim's curios, and the Guild's night with what it'll want. Hover a day for the whole story; until then, the
    /// next thing coming is told below.
    /// </summary>
    void BuildCalendar()
    {
        int today = _state.Day;
        // The Guild's nights show once Vane has introduced them (the second evening).
        bool guild = _state.GuildIntroduced;
        _page.AddChild(UiKit.MakeLabel(Loc.T($"Today is day {today}: week {_state.Week}, day {_state.DayOfWeek} of {Guild.DaysPerWeek}."), 16, Palette.Muted)
            .Place(TableX, PanelY + 60, PanelW - 48, 20));
        if (guild) BuildDebt(PanelY + 86);

        // Five days to a week: a story morning or Nim's, two days with something on, and the Guild's night.
        float gridY = guild ? PanelY + 168 : PanelY + 92;
        const float cellX = 176, cellW = 200, gap = 8, rowH = 90;
        var detail = UiKit.MakeLabel("", 16, Palette.Text).Wrapped();
        detail.Place(TableX, gridY + 3 * rowH + 6, PanelW - 48, 64);
        string upcoming = null;
        for (int row = 0; row < 3; row++)
        {
            int week = _state.Week + row;
            var plan = _state.PlanFor(week);
            float y = gridY + row * rowH;
            // The column starts a little into the margin, so "this week" fits in a longer language too.
            _page.AddChild(UiKit.MakeLabel(Loc.T($"WEEK {week}"), 16, Palette.Gold).Place(TableX - 6, y + 8, 116, 20));
            _page.AddChild(UiKit.MakeLabel(row == 0 ? Loc.T("this week") : row == 1 ? Loc.T("next week") : Loc.T("not planned"), 16, Palette.Muted).Place(TableX - 6, y + 28, 116, 20));
            if (week > Guild.Weeks)
                _page.AddChild(UiKit.MakeLabel(Loc.T("past the debt"), 16, Palette.Muted).Place(TableX - 6, y + 48, 116, 20));
            for (int d = 1; d <= Guild.DaysPerWeek; d++)
            {
                int day = GameState.DayNumber(week, d);
                bool isToday = day == today, past = day < today;
                var cell = UiKit.MakePanel(isToday ? Palette.Gold : Palette.PanelBorder).Place(cellX + (d - 1) * (cellW + gap), y, cellW, rowH - 8);
                cell.MouseFilter = Control.MouseFilterEnum.Stop;
                if (past) cell.Modulate = new Color(1, 1, 1, 0.45f);
                cell.AddChild(UiKit.MakeLabel(Loc.T($"DAY {day}"), 16, isToday ? Palette.Gold : Palette.Muted).Place(10, 6, 100, 20));
                if (isToday) cell.AddChild(UiKit.MakeLabel(Loc.T("today"), 16, Palette.Gold, HorizontalAlignment.Right).Place(cellW - 110, 6, 100, 20));

                var lines = new System.Collections.Generic.List<string>();
                const float ly = 30;
                var role = plan?.Role(d) ?? (d == Guild.DaysPerWeek ? DayRole.Tax : DayRole.None);
                if (role == DayRole.Story)
                {
                    cell.AddChild(UiKit.MakeLabel(Story.Teaser(week), 16, Palette.Gold).Wrapped().Place(10, ly, cellW - 20, 40));
                    lines.Add(Loc.T($"A story morning: {Story.Teaser(week).ToLowerInvariant()}, before you open."));
                }
                else if (role == DayRole.Event && Events.Find(plan.On(d)) is EventDef e)
                {
                    cell.AddChild(UiKit.MakeLabel(Events.Tag(e, plan.Wanted), 16, Palette.Magic).Wrapped().Place(10, ly, cellW - 20, 40));
                    lines.Add(Loc.T($"{e.Name}: {Events.Describe(e, plan.Wanted)}"));
                }
                // Whoever said they'd come back, on the day they said.
                var expected = _state.Expected().Where(x => x.Day == day).ToList();
                if (expected.Count > 0)
                {
                    float ey = role is DayRole.Story or DayRole.Event ? ly + 20 : ly;
                    // One line in the cell (a longer note, or a longer language, ends in "..."): the whole of it on hover, below.
                    var who = UiKit.MakeLabel(expected[0].Note, 16, Palette.Good).Place(10, ey + (role == DayRole.Tax ? 20 : 0), cellW - 20, 20);
                    who.ClipText = true;
                    who.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
                    cell.AddChild(who);
                    lines.AddRange(expected.Select(x => Loc.T($"{x.Note}: {Sellers.Person(x.Who).Name} said they'd come by.")));
                }
                if (role == DayRole.Tax && guild)
                {
                    bool paid = week <= _state.WeeksPaid;
                    int owed = week == _state.Week ? _state.DueNow : _state.DueFor(week);
                    string text = paid ? Loc.T("Guild: paid") : Loc.T($"Guild: {owed}g");
                    var colour = paid ? Palette.Good : week == _state.Week && _state.Gold < owed ? Palette.Warn : Palette.Text;
                    cell.AddChild(UiKit.MakeLabel(text, 16, colour).Place(10, ly, cellW - 20, 20));
                    lines.Add(paid ? Loc.T("The Guild's night: this week's tithe is already paid.") : Loc.T($"The Guild's night: Mister Vane comes for {owed}g after closing."));
                }
                else if (role == DayRole.Tax)
                    cell.AddChild(UiKit.MakeLabel(Loc.T("The week's end"), 16, Palette.Muted).Place(10, ly, cellW - 20, 20));
                if (plan == null && d == 1)
                    lines.Add(Loc.T("Next week's plan is made at the start of this one: its story and its days in town."));
                string story = Loc.T($"Day {day}{(isToday ? Loc.T(" (today)") : "")}: {((lines.Count == 0 ? Loc.T("nothing special planned.") : string.Join(" ", lines)))}");
                if (!past && !isToday && upcoming == null && lines.Count > 0) upcoming = Loc.T($"Next: {story}");
                cell.MouseEntered += () => detail.Text = story;
                cell.MouseExited += () => detail.Text = upcoming ?? "";
                _page.AddChild(cell);
            }
        }
        detail.Text = upcoming ?? "";
        _page.AddChild(detail);
    }

    /// <summary>
    /// The town (docs/stories.md): where you stand with each power (the four that change the trade always, the rest once
    /// they've an opinion), with what a friend gets and what a foe; the notices on the board; every tale begun, and where
    /// it's got to; and what's in the back room.
    /// </summary>
    void BuildTown()
    {
        float y = PanelY + 60;
        _page.AddChild(UiKit.MakeLabel(Loc.T("WHERE YOU STAND"), 16, Palette.Gold).Place(TableX, y, 400, 20));
        var factions = Factions.All.Where(f => Factions.Ruling.Contains(f) || _state.Standing(f) != 0).ToList();
        for (int i = 0; i < factions.Count; i++)
        {
            var f = factions[i];
            int standing = _state.Standing(f);
            float ry = y + 26 + i * 34;
            var row = UiKit.MakePanel(standing >= Factions.Strong ? Palette.Good.Darkened(0.3f) : standing <= -Factions.Strong ? Palette.Bad.Darkened(0.3f) : Palette.PanelBorder)
                .Place(TableX - 8, ry, 540, 30);
            row.MouseFilter = Control.MouseFilterEnum.Stop;
            var (friend, foe) = Factions.Effects(f);
            row.TooltipText = Loc.T($"{Factions.Who(f)}\nAt +2: {friend}\nAt -2: {foe}");
            row.AddChild(UiKit.MakeLabel(Factions.Name(f), 16, Palette.Text).Place(10, 5, 200, 20));
            // Seven pips, -3 to +3: filled out from the middle.
            row.AddChild(new Painter(p =>
            {
                for (int v = Factions.Min; v <= Factions.Max; v++)
                {
                    var c = new Vector2(230 + (v - Factions.Min) * 18, 15);
                    bool lit = v != 0 && (standing > 0 ? v > 0 && v <= standing : v < 0 && v >= standing);
                    p.DrawCircle(c, v == 0 ? 3 : 6, lit ? (v > 0 ? Palette.Good : Palette.Bad) : Palette.Muted.Darkened(0.4f));
                }
            }));
            string now = standing >= Factions.Strong ? friend : standing <= -Factions.Strong ? foe : Factions.Word(standing);
            row.AddChild(UiKit.MakeLabel(Factions.Word(standing), 16, standing > 0 ? Palette.Good : standing < 0 ? Palette.Warn : Palette.Muted).Place(368, 5, 164, 20));
            if (Math.Abs(standing) >= Factions.Strong && Factions.Ruling.Contains(f))
                row.TooltipText = Loc.T($"{Factions.Who(f)}\nNow: {now}");
            _page.AddChild(row);
        }
        float by = y + 32 + factions.Count * 34;
        int held = _state.BackRoom.Count;
        if (held > 0)
        {
            _page.AddChild(UiKit.MakeLabel(Loc.Plural(held, $"The back room: {held} chest that won't open for you.", $"The back room: {held} chests that won't open for you."),
                16, Palette.Warn).Place(TableX, by, 540, 20));
            by += 26;
        }
        // What the season's tales have left you (0.30): friends who owe you, and papers for the King.
        if (_state.Owed.Count > 0)
        {
            var owed = UiKit.MakeLabel(Loc.T($"Who owes you a favour: {Words.List(_state.Owed.Keys.Select(p => Sellers.Person(p).Name))}."), 16, Palette.Good)
                .Wrapped().Place(TableX, by, 540, 44);
            owed.MaxLinesVisible = 2;
            owed.ClipText = true;
            owed.MouseFilter = Control.MouseFilterEnum.Stop;
            owed.TooltipText = Loc.T("On a Guild night you're short, call on one of them: they bring an eighth of the night's due. Once a season each.");
            _page.AddChild(owed);
            by += 48;
        }
        if (_state.Evidence.Count > 0)
        {
            var papers = UiKit.MakeLabel(Loc.T($"Papers against the Guild: {_state.Evidence.Count} (the King's court tries it with {TaleBook.PapersForTrial})."),
                16, Palette.Magic).Wrapped().Place(TableX, by, 540, 44);
            papers.MouseFilter = Control.MouseFilterEnum.Stop;
            papers.TooltipText = Words.List(_state.Evidence.Select(TaleBook.Paper));
            _page.AddChild(papers);
        }

        // The board, and the tales.
        const float rx = 640, rw = 560;
        _page.AddChild(UiKit.MakeLabel(Loc.T("ON THE BOARD"), 16, Palette.Gold).Place(rx, y, rw, 20));
        var begun = TaleBook.All.Where(t => _state.TaleLog.ContainsKey(t.Key)).ToList();
        // Three notices, or two once there are more than three stories to make room for.
        var notices = _state.Notices.AsEnumerable().Reverse().Take(begun.Count > 3 ? 2 : 3).ToList();
        float ny = y + 26;
        if (notices.Count == 0)
        {
            _page.AddChild(UiKit.MakeLabel(Loc.T("Nothing posted."), 16, Palette.Muted).Place(rx, ny, rw, 20));
            ny += 28;
        }
        foreach (var notice in notices)
        {
            var card = UiKit.MakePanel(Palette.Gold.Darkened(0.45f)).Place(rx - 8, ny, rw, 64);
            card.MouseFilter = Control.MouseFilterEnum.Stop;
            card.TooltipText = notice.Text;
            card.AddChild(UiKit.MakeLabel(notice.Title, 16, Palette.Gold).Place(10, 4, rw - 20, 20));
            var text = UiKit.MakeLabel(notice.Text, 16, Palette.Text).Wrapped();
            text.MaxLinesVisible = 2;
            text.ClipText = true;
            card.AddChild(text.Place(10, 24, rw - 20, 38));
            _page.AddChild(card);
            ny += 70;
        }
        float ty = ny + 8;
        _page.AddChild(UiKit.MakeLabel(Loc.T("STORIES"), 16, Palette.Gold).Place(rx, ty, rw, 20));
        if (begun.Count == 0)
            _page.AddChild(UiKit.MakeLabel(Loc.T("Nothing's begun yet. People will come with their troubles."), 16, Palette.Muted).Wrapped().Place(rx, ty + 24, rw, 40));
        // Each story where it's got to, in two lines when they all fit (one each when they don't); the whole of it on hover.
        float top = ty + 26, room = PanelY + PanelH - 16 - top;
        bool two = begun.Count * 48 <= room;
        float step = two ? 48 : 24;
        for (int i = 0; i < Math.Min(begun.Count, (int)(room / step)); i++)
        {
            var tale = begun[i];
            var log = _state.TaleLog[tale.Key];
            var line = UiKit.MakeLabel(Loc.T($"{tale.Title}: {log[^1]}"), 16, Palette.Text);
            line.ClipText = true;
            line.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            if (two)
            {
                line.Wrapped();
                line.MaxLinesVisible = 2;
            }
            line.MouseFilter = Control.MouseFilterEnum.Stop;
            line.TooltipText = $"{tale.Title}\n{tale.Blurb}\n\n{string.Join("\n", log)}";
            _page.AddChild(line.Place(rx, top + i * step, rw, step - 4));
        }
    }

    string _story;

    /// <summary>
    /// The Stories page (0.32): every tale begun, the ones still going first, and the one picked, whole: what it's about,
    /// everything that's happened in it so far, and what comes next (who, where, about when), or that nothing's expected.
    /// </summary>
    // ---- The Inquiry (0.34, Core/Inquiry.cs) ----

    /// <summary>The thread open on the Inquiry page.</summary>
    static string _thread;

    /// <summary>
    /// The Inquiry: the questions you're following down the side, and the one picked whole: what you saw, who said what,
    /// what you tried and what came of it, and the leads (things you now have reason to do). Bram's thread starts with
    /// the treasury key, piece by piece and how each is earned; the plan's, with its four lines and what each still needs.
    /// Nothing here is an answer to pick: what's known is what you've found out.
    /// </summary>
    void BuildInquiry()
    {
        var threads = Inquiry.Threads.Where(t => t.Shows(_state)).ToList();
        float top = PanelY + 60;
        if (threads.Count == 0)
        {
            _page.AddChild(UiKit.MakeLabel(Loc.T("Nothing to follow yet. When something doesn't add up, it goes here: what you saw, what you were told, and what you tried."),
                16, Palette.Muted).Wrapped().Place(TableX, top, PanelW - 48, 60));
            return;
        }
        var thread = threads.FirstOrDefault(t => t.Key == _thread) ?? threads[0];
        _thread = thread.Key;

        // The questions down the side.
        const float listW = 340;
        for (int i = 0; i < threads.Count; i++)
        {
            var t = threads[i];
            bool picked = t == thread;
            var row = UiKit.MakePanel(picked ? Palette.Gold : Palette.PanelBorder).Place(TableX - 8, top + i * 54, listW - 4, 50);
            row.MouseFilter = Control.MouseFilterEnum.Stop;
            string key = t.Key;
            row.GuiInput += e =>
            {
                if (e is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) return;
                _thread = key;
                Sfx.Play("tick", 1.1f, -8);
                ShowPage(InquiryPage);
            };
            var ask = UiKit.MakeLabel(t.Ask, 16, picked ? Palette.Gold : Palette.Text).Wrapped().Place(12, 3, listW - 40, 44);
            ask.VerticalAlignment = VerticalAlignment.Center;
            row.AddChild(ask);
            _page.AddChild(row);
        }

        float x = TableX + listW + 8, w = PanelX + PanelW - 24 - x, y = top;
        _page.AddChild(UiKit.MakeLabel(thread.Ask.ToUpperInvariant(), 24, Palette.Gold, outline: 4).Place(x, y - 4, w, 30));
        y += 34;
        // Where is Bram: the treasury key, and how each piece is earned (the season's goal).
        if (thread.Key == Inquiry.Bram && !_state.CampaignDone)
        {
            _page.AddChild(UiKit.MakeLabel(Loc.T($"THE TREASURY KEY: {_state.Pieces.Count} OF {Campaign.Pieces.Length}"), 16, Palette.Magic).Place(x, y, w, 20));
            y += 24;
            foreach (string piece in Campaign.Pieces)
            {
                bool has = _state.Pieces.Contains(piece);
                _page.AddChild(UiKit.MakeCheck(has, new Vector2(x + 10, y + 10), 1));
                string line = has ? Campaign.PieceName(piece) : Loc.T($"{Campaign.PieceName(piece)}: {Campaign.PieceHow(piece)}");
                float lh = UiKit.TextHeight(line, w - 30);
                _page.AddChild(UiKit.MakeLabel(line, 16, has ? Palette.Good : Palette.Text).Wrapped().Place(x + 28, y, w - 30, lh));
                y += lh + 4;
            }
            y += 6;
        }
        // The plan: four lines, each ticked when it's true this season.
        if (thread.Key == Inquiry.Plan)
        {
            foreach (var (line, done, needs) in Inquiry.PlanLines(_state))
            {
                _page.AddChild(UiKit.MakeCheck(done, new Vector2(x + 10, y + 10), 1));
                _page.AddChild(UiKit.MakeLabel(line, 16, done ? Palette.Good : Palette.Gold).Place(x + 28, y, w - 30, 20));
                y += 22;
                if (!done)
                {
                    float nh = UiKit.TextHeight(needs, w - 30);
                    _page.AddChild(UiKit.MakeLabel(needs, 16, Palette.Muted).Wrapped().Place(x + 28, y, w - 30, nh));
                    y += nh;
                }
                y += 8;
            }
            _page.AddChild(UiKit.MakeLabel(_state.PlanWhole ? Loc.T("The plan is whole. Pay the account with the key whole, and go to the treasury.")
                : Loc.T("When every line is ticked, the treasury can end differently."), 16, _state.PlanWhole ? Palette.Good : Palette.Muted)
                .Wrapped().Place(x, y, w, 40));
            y += 48;
        }
        // The entries, each with how you came by it.
        var entries = _state.Entries(thread.Key).ToList();
        if (entries.Count == 0) return;
        var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }.Place(x, y, w, Math.Max(40, PanelY + PanelH - 16 - y));
        _page.AddChild(scroll);
        var list = new Control { MouseFilter = Control.MouseFilterEnum.Pass };
        scroll.AddChild(list);
        float ly = 0;
        foreach (var entry in entries)
        {
            var tag = UiKit.MakeLabel(Inquiry.WayName(entry.Way).ToUpperInvariant(), 16, entry.Way switch
            {
                EntryWay.Found => Palette.Gold,
                EntryWay.Seen => Palette.Magic,
                EntryWay.Tried or EntryWay.Heard => Palette.Good,
                EntryWay.Lead => Palette.Warn,
                _ => Palette.Muted,
            }).Place(0, ly, 80, 20);
            list.AddChild(tag);
            string text = entry.Text(_state);
            if (entry.Way == EntryWay.Said && entry.Who != null) text = Loc.T($"{Story.Name(entry.Who)}: {text}");
            if (entry.Held) text = Loc.T($"{text} (held this season)");
            else if (_state.KnownBefore.Contains(entry.Key) && !_state.Noted.Contains(entry.Key)) text = Loc.T($"{text} (remembered)");
            float textH = UiKit.TextHeight(text, w - 104);
            list.AddChild(UiKit.MakeLabel(text, 16, entry.Way == EntryWay.Lead ? Palette.Warn.Lightened(0.3f) : Palette.Text).Wrapped().Place(88, ly, w - 104, textH));
            ly += textH + 10;
        }
        list.CustomMinimumSize = new Vector2(w - 16, ly);
    }

    void BuildStories()
    {
        var begun = TaleBook.All.Where(t => _state.TaleLog.ContainsKey(t.Key))
            .OrderBy(t => _state.TaleNext(t) == null ? 1 : 0).ThenBy(t => Array.IndexOf(TaleBook.All, t)).ToList();
        float top = PanelY + 60;
        if (begun.Count == 0)
        {
            _page.AddChild(UiKit.MakeLabel(Loc.T("No stories yet. People will come to the counter with their troubles, and the notebook will follow each one: " +
                                                 "what it's about, what's happened, and what comes next."), 16, Palette.Muted).Wrapped()
                .Place(TableX, top, PanelW - 48, 60));
            return;
        }
        var tale = begun.FirstOrDefault(t => t.Key == _story) ?? begun[0];
        _story = tale.Key;

        // The list: each story's title, gold while it's going, grey once nothing more is expected.
        const float listW = 340;
        var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }
            .Place(TableX - 8, top, listW + 8, PanelY + PanelH - 16 - top);
        _page.AddChild(scroll);
        var list = new Control { MouseFilter = Control.MouseFilterEnum.Pass };
        scroll.AddChild(list);
        for (int i = 0; i < begun.Count; i++)
        {
            var t = begun[i];
            bool going = _state.TaleNext(t) != null, picked = t == tale;
            var row = UiKit.MakePanel(picked ? Palette.Gold : Palette.PanelBorder).Place(0, i * 38, listW - 12, 34);
            row.MouseFilter = Control.MouseFilterEnum.Stop;
            string key = t.Key;
            row.GuiInput += e =>
            {
                if (e is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) return;
                _story = key;
                Sfx.Play("tick", 1.1f, -8);
                ShowPage(StoriesPage);
            };
            var title = UiKit.MakeLabel(t.Title, 16, going ? Palette.Gold : Palette.Muted).Place(10, 7, listW - 40, 20);
            title.ClipText = true;
            title.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            row.AddChild(title);
            list.AddChild(row);
        }
        list.CustomMinimumSize = new Vector2(listW, begun.Count * 38);

        // The story picked, whole.
        float x = TableX + listW + 16, w = PanelX + PanelW - 24 - x, y = top;
        _page.AddChild(UiKit.MakeLabel(tale.Title.ToUpperInvariant(), 24, Palette.Gold, outline: 4).Place(x, y - 4, w, 30));
        y += 32;
        var about = UiKit.MakeLabel(tale.Blurb, 16, Palette.Text).Wrapped();
        float aboutH = UiKit.TextHeight(tale.Blurb, w);
        _page.AddChild(about.Place(x, y, w, aboutH));
        y += aboutH + 12;

        string next = _state.TaleNext(tale);
        float nextH = next == null ? 24 : UiKit.TextHeight(next, w) + 24;
        float bottom = PanelY + PanelH - 20 - nextH;
        _page.AddChild(UiKit.MakeLabel(Loc.T("SO FAR"), 16, Palette.Gold.Darkened(0.2f)).Place(x, y, w, 20));
        y += 24;
        var sofar = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }.Place(x, y, w, Math.Max(40, bottom - y - 8));
        _page.AddChild(sofar);
        var log = _state.TaleLog[tale.Key];
        string text = string.Join("\n", log.Select(l => "- " + l));
        var lines = UiKit.MakeLabel(text, 16, Palette.Text).Wrapped();
        lines.CustomMinimumSize = new Vector2(w - 16, UiKit.TextHeight(text, w - 16));
        sofar.AddChild(lines);
        // The latest at the bottom, in view.
        sofar.Ready += () => sofar.ScrollVertical = int.MaxValue;

        _page.AddChild(UiKit.MakeLabel(next != null ? Loc.T("WHAT COMES NEXT") : Loc.T("NOTHING MORE IS EXPECTED FOR NOW"), 16,
            next != null ? Palette.Gold.Darkened(0.2f) : Palette.Muted).Place(x, bottom, w, 20));
        if (next != null)
            _page.AddChild(UiKit.MakeLabel(next, 16, Palette.Good).Wrapped().Place(x, bottom + 22, w, nextH - 22));
    }

    /// <summary>
    /// The Guild's debt: a line on what's left and what's next, and a box for each week of it (paid in green, this week's
    /// framed, tonight's in orange when the purse is short), then what comes after. The purse's due in the top bar opens
    /// the notebook here, the box framed for a moment.
    /// </summary>
    void BuildDebt(float y)
    {
        // A box a week, and one for what comes after: seven weeks and "then" across the page (0.30's longer season).
        const float boxH = 44, gap = 8;
        float boxW = (PanelW - 32 - gap * Guild.Weeks) / (Guild.Weeks + 1);
        string when = When(_state.DaysToNextDue, _state.NextDueWeek);
        // What's left and what's next, with a word on paying it all off where there's room for it (in French, with a due
        // rolled over at a high level, there isn't).
        string Left(string tip) =>
            Loc.T($"{_state.DebtLeft}g of Bram's debt left. Next: {_state.NextDue}g {when}{(_state.Arrears > 0 ? Loc.T($" ({_state.Arrears}g rolled over)") : "")}.{tip}");
        string line = _state.DebtCleared
            ? Loc.T($"Bram's debt is cleared{(_state.ClearedDay > 0 ? Loc.T($" (day {_state.ClearedDay})") : "")}. The Guild still comes, for its interest: {_state.NextDue}g {when}.")
            : Left(_state.CanPayOff ? Loc.T(" You could pay it all off at the day's end.") : Loc.T(" Pay it all off early from a day's end."));
        if (!_state.DebtCleared && UiKit.TextWidth(line, 16) > PanelW - 216) line = Left("");
        var block = new Control { MouseFilter = Control.MouseFilterEnum.Ignore }.Place(TableX - 8, y, PanelW - 32, 76);
        _page.AddChild(block);
        block.AddChild(UiKit.MakeLabel(Loc.T("THE GUILD'S DEBT"), 16, Palette.Gold).Place(8, 0, 200, 20));
        block.AddChild(UiKit.MakeLabel(line, 16, Palette.Text).Place(184, 0, PanelW - 216, 20));
        for (int week = 1; week <= Guild.Weeks + 1; week++)
        {
            float x = (week - 1) * (boxW + gap);
            bool after = week > Guild.Weeks;
            bool paid = !after && week <= _state.WeeksPaid, now = !after && !paid && week == _state.NextDueWeek;
            bool short_ = now && _state.Gold < _state.NextDue;
            var colour = paid ? Palette.Good : now ? (short_ ? Palette.Warn : Palette.Gold) : Palette.PanelBorder;
            var box = UiKit.MakePanel(colour).Place(x, 26, boxW, boxH);
            box.MouseFilter = Control.MouseFilterEnum.Pass;
            block.AddChild(box);
            string title = after ? Loc.T("THEN") : Loc.T($"WEEK {week}");
            string amount = after ? Loc.T("x1.5 a week") : paid ? Loc.T($"{_state.DueFor(week)}g  paid") : Money.Short(now ? _state.NextDue : _state.DueFor(week));
            // The week on the first line, what it costs on the second (when it's due is in the line above, and the tooltip).
            box.AddChild(UiKit.MakeLabel(title, 16, now ? colour : Palette.Muted).Place(8, 2, boxW - 16, 20));
            box.AddChild(UiKit.MakeLabel(amount, 16, paid ? Palette.Good : now ? colour : after ? Palette.Muted : Palette.Text, HorizontalAlignment.Right)
                .Place(8, 22, boxW - 16, 20));
            box.TooltipText = after ? Loc.T("Once Bram's debt is paid, the Guild keeps coming for interest: each week half as much again as the last.")
                : paid ? Loc.T($"Week {week}: paid.") : Loc.T($"Week {week}: Mister Vane comes for {(now ? _state.NextDue : _state.DueFor(week))}g on the fifth evening.");
            if (now && _focusDebt)
            {
                var pulse = box.CreateTween().SetLoops(4);
                pulse.TweenProperty(box, "self_modulate", new Color(1.5f, 1.4f, 1.1f), 0.35f);
                pulse.TweenProperty(box, "self_modulate", Colors.White, 0.35f);
            }
        }
    }

    /// <summary>When the Guild comes next, as the top bar says it.</summary>
    string When(int days, int week) =>
        !_state.PaidThisWeek && days == 0 ? Loc.T("tonight!")
        : week > _state.Week + 1 ? Loc.T($"in week {week}")
        : _state.PaidThisWeek ? Loc.T("next week")
        : days == 1 ? Loc.T("tomorrow night") : Loc.T($"in {days + 1} days");

    static string Capital(string text) => char.ToUpperInvariant(text[0]) + text[1..];

    void ShowRegulars(int spread)
    {
        foreach (var child in _page.GetChildren()) child.QueueFree();
        BuildRegulars(spread);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed(Controls.Notebook) || e.IsActionPressed(Controls.Back))
        {
            GetViewport().SetInputAsHandled();
            Close();
        }
    }

    public void Close()
    {
        if (_closing || !IsInsideTree()) return;
        _closing = true;
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", Colors.Transparent, 0.12f);
        tween.TweenCallback(Callable.From(() =>
        {
            Closed?.Invoke();
            QueueFree();
        }));
    }
}
