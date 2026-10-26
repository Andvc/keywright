using System;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// The shop's ledger. Sounds: one row per kind of thing your shop sees (its sound, how hard you must shake to hear it,
/// how heavy it is, and what one is worth if it's as good as each colour of chest promises; click a row to hear it),
/// and the rules of listening. Regulars: how each seller you've met judges a chest, trades and behaves (whoever's at the
/// counter framed), and Uncle Bram and the story's people on the Friends spread. Calendar: today, the Guild's
/// nights and what they'll cost, the days Nim comes by, and what's happening in town this week and next. Collection:
/// everything Bram's kin have found, this run and every one before. Curios: every curio, the ones you've seen, the ones
/// Nim carries, and what opens the rest. Synergies: what the shop could be known for, how far along each is, and (at the
/// end of a day) which are active. Songs: the Heartsongs Bram's kin have opened, once there's one to show. What any
/// run has learned stays in it: the sounds and regulars of the grandest shop any run reached (<see cref="GameState.JournalTier"/>).
/// </summary>
public partial class LedgerView : Node2D
{
    // Ends above Uncle Bram's box, so his tips stay readable with the ledger open.
    const float PanelX = 40, PanelY = 66, PanelW = 1200, PanelH = 518;
    const float TableX = 64, RowH = 40;

    readonly GameState _state;
    readonly int _firstPage;
    readonly bool _choosing;

    /// <summary>Whoever's at the counter: the Regulars page opens at their place, their entry framed.</summary>
    readonly string _current;
    Node2D _page;
    bool _closing;

    public event Action Closed;

    public LedgerView() { }

    /// <param name="choosing">Synergies can be switched on and off (at the end of a day).</param>
    /// <param name="current">The seller at the counter, if any.</param>
    public LedgerView(GameState state, int page = 0, bool choosing = false, string current = null)
    {
        _state = state;
        _firstPage = page;
        _choosing = choosing;
        _current = current;
    }

    static Audio.Sfx Sfx => Main.Instance.Sfx;

    public override void _Ready()
    {
        var blocker = new ColorRect { Color = new Color(0, 0, 0, 0.7f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920);
        blocker.GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true }) Close();
        };
        AddChild(blocker);
        AddChild(UiKit.MakePanel(Palette.Gold.Darkened(0.3f)).Place(PanelX, PanelY, PanelW, PanelH));
        AddChild(UiKit.MakeLabel("LEDGER", 32, Palette.Gold, outline: 4).Place(PanelX + 24, PanelY + 10, 200, 40));
        AddChild(UiKit.MakeButton("Sounds", Palette.Text, 16, () => ShowPage(0)).Place(PanelX + 190, PanelY + 14, 96, 36));
        AddChild(UiKit.MakeButton("Regulars", Palette.Text, 16, () => ShowPage(1)).Place(PanelX + 292, PanelY + 14, 112, 36));
        AddChild(UiKit.MakeButton("Calendar", Palette.Text, 16, () => ShowPage(4)).Place(PanelX + 410, PanelY + 14, 112, 36));
        AddChild(UiKit.MakeButton("Collection", Palette.Text, 16, () => ShowPage(2)).Place(PanelX + 528, PanelY + 14, 128, 36));
        AddChild(UiKit.MakeButton("Curios", Palette.Magic, 16, () => ShowPage(6)).Place(PanelX + 662, PanelY + 14, 96, 36));
        // Synergies come with Crownhold: before that, the journal just keeps its tally.
        if (_state.SynergiesOpen)
            AddChild(UiKit.MakeButton("Synergies", Palette.Text, 16, () => ShowPage(3)).Place(PanelX + 764, PanelY + 14, 120, 36));
        if (_state.Songbook.Count > 0)
            AddChild(UiKit.MakeButton("Songs", Palette.Of(Rarity.Mythic), 16, () => ShowPage(5)).Place(PanelX + 890, PanelY + 14, 88, 36));
        AddChild(UiKit.MakeButton("Close [L]", Palette.Text, 16, Close).Place(PanelX + PanelW - 144, PanelY + 12, 120, 40));
        _page = new Node2D();
        AddChild(_page);
        ShowPage(_firstPage);

        Sfx.PlayGroup("book", 1f, -4);
        Modulate = Colors.Transparent;
        CreateTween().TweenProperty(this, "modulate", Colors.White, 0.15f);
    }

    /// <summary>0: sounds, 1: regulars, 2: collection, 3: synergies, 4: calendar, 5: Heartsongs, 6: curios.</summary>
    public void ShowPage(int page)
    {
        foreach (var child in _page.GetChildren()) child.QueueFree();
        if (page == 5 && _state.Songbook.Count > 0) BuildSongs();
        else if (page == 6) BuildCurios();
        else if (page == 1) BuildRegulars(CurrentSpread);
        else if (page == 2) BuildCollection();
        else if (page == 4) BuildCalendar();
        else if (page == 3 && _state.SynergiesOpen) BuildSynergies();
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
            ? $"Active: {active} of {slots}. Switch one on to put it to work; more slots come with a Crownhold Charter and a Royal Seal."
            : $"Active: {active} of {slots}. You choose at the end of a day.";
        _page.AddChild(UiKit.MakeLabel(intro, 16, Palette.Muted).Place(TableX, PanelY + 60, PanelW - 48, 20));
        const float rowH = 58;
        for (int i = 0; i < Synergies.All.Length; i++)
        {
            var synergy = Synergies.All[i];
            float y = PanelY + 88 + i * rowH;
            int level = synergy.Level(_state);
            bool on = _state.ActiveSynergies.Contains(synergy.Key);
            bool later = synergy.Tier > _state.Tier && level == 0;
            var row = new Panel { MouseFilter = Control.MouseFilterEnum.Ignore }.Place(TableX - 8, y, PanelW - 32, rowH - 4);
            row.AddThemeStyleboxOverride("panel", UiKit.PanelBox(4));
            if (on) row.AddChild(UiKit.MakeFrame(Palette.Magic));
            _page.AddChild(row);
            var nameColor = later ? Palette.Muted.Darkened(0.2f) : on ? Palette.Magic : level > 0 ? Palette.Gold : Palette.Text;
            row.AddChild(UiKit.MakeLabel(synergy.Name, 16, nameColor).Place(12, 6, 220, 20));
            row.AddChild(UiKit.MakeLabel(later ? $"{synergy.Blurb} (later)" : synergy.Blurb, 16, Palette.Muted).Place(12, 28, 220, 20));
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
            string effect = level > 0 ? synergy.Effect(level) : $"Level 1: {synergy.Effect(1)}";
            row.AddChild(UiKit.MakeLabel(effect, 16, level > 0 ? Palette.Text : Palette.Muted).Wrapped().Place(360, 6, 480, 46));
            var progress = synergy.Progress(_state).ToList();
            string next = progress.Count == 0 ? "Highest level!" : $"Next: {string.Join("\n", progress)}";
            row.AddChild(UiKit.MakeLabel(next, 16, Palette.Muted).Wrapped().Place(852, 6, 196, 46));
            if (_choosing && level > 0)
            {
                bool canTurnOn = on || active < slots;
                var toggle = UiKit.MakeButton(on ? "Active" : "Use", on ? Palette.Magic : Palette.Good, 16, () =>
                {
                    bool now = _state.ToggleSynergy(synergy.Key);
                    Sfx.Play(now ? "sting" : "tick", now ? 1.3f : 1f, -4);
                    ShowPage(3);
                }).Place(1060, 10, 100, 34);
                toggle.Disabled = !canTurnOn;
                if (!canTurnOn) toggle.TooltipText = "No free slot: switch another one off first.";
                row.AddChild(toggle);
            }
            else if (on) row.AddChild(UiKit.MakeLabel("Active", 16, Palette.Magic, HorizontalAlignment.Center).Place(1060, 18, 100, 20));
        }
    }

    /// <summary>
    /// The songbook: the nine Heartsongs, three by three. One the family has opened shows its picture, who loved it and
    /// what it does on display (its story on hover), and whether it's singing in the shop now; the rest are "?".
    /// </summary>
    void BuildSongs()
    {
        int known = Heartsongs.All.Count(h => _state.Songbook.Contains(h.Key));
        _page.AddChild(UiKit.MakeLabel("Nine things loved more than gold, sung into the Locking and hidden in chests among millions. Only a " +
                                       "Keywright hears one sing; to anyone else it's worth nothing. Hover a card for its story. " +
                                       $"Bram's kin know {known} of the nine" +
                                       (known == Heartsongs.All.Length ? ": the Crown Chest can be sung whole." : ": with all nine, the Crown Chest's ending changes."),
            16, Palette.Muted).Wrapped().Place(TableX, PanelY + 56, PanelW - 48, 60));
        const float cardW = 376, cardH = 128, gap = 8;
        for (int i = 0; i < Heartsongs.All.Length; i++)
        {
            var song = Heartsongs.All[i];
            bool found = _state.Songbook.Contains(song.Key);
            bool shown = _state.Shows(song.Key);
            float x = TableX - 8 + i % 3 * (cardW + gap), y = PanelY + 118 + i / 3 * (cardH + 4);
            var card = new Panel { MouseFilter = Control.MouseFilterEnum.Pass }.Place(x, y, cardW, cardH);
            card.AddThemeStyleboxOverride("panel", UiKit.PanelBox(4));
            if (shown) card.AddChild(UiKit.MakeFrame(Palette.Of(Rarity.Mythic)));
            _page.AddChild(card);
            if (!found)
            {
                card.AddChild(UiKit.MakeLabel("?", 32, Palette.Muted, HorizontalAlignment.Center).Place(0, 44, cardW, 40));
                card.TooltipText = "Not heard yet.";
                continue;
            }
            card.TooltipText = $"{song.Name}, from {song.From}. {song.Story}";
            card.AddChild(UiKit.MakeItemIcon(Heartsongs.Make(song, Rarity.Common), new Vector2(32, 40), 48));
            // Who gave it is in the tooltip with its story: the card has room for what it does.
            card.AddChild(UiKit.MakeLabel(song.Name, 16, Palette.Of(Rarity.Mythic)).Place(64, 6, cardW - 72, 20));
            card.AddChild(UiKit.MakeLabel(song.Effect, 16, Palette.Text).Wrapped().Place(64, 30, cardW - 72, 92));
            if (shown) card.AddChild(UiKit.MakeLabel("singing", 16, Palette.Gold, HorizontalAlignment.Right).Place(cardW - 100, 6, 92, 20));
        }
    }

    /// <summary>
    /// A grid: one row per kind, one column per quality, then the fake. Found things show as they came out of the chest;
    /// the rest are "?". A star means you've found one enchanted too.
    /// </summary>
    void BuildCollection()
    {
        const float cell = 64, gridX = TableX + 140, gridY = PanelY + 92, step = cell + 76;
        string[] columns = { "Common", "Uncommon", "Rare", "Epic", "Legendary", "Fake" };
        for (int c = 0; c < columns.Length; c++)
            _page.AddChild(UiKit.MakeLabel(columns[c], 16, c < 5 ? Palette.Of((Rarity)c) : Palette.Muted, HorizontalAlignment.Center)
                .Place(gridX + c * step - 38, gridY - 26, cell + 66, 20));
        var entries = Collection.Entries.ToList();
        int row = 0;
        foreach (var kind in Categories.Kinds)
        {
            float y = gridY + row * cell;
            _page.AddChild(UiKit.MakeKindIcon(kind.Category, new Vector2(TableX + 16, y + cell / 2 - 6)));
            _page.AddChild(UiKit.MakeLabel(kind.Name, 16, Palette.Text).Place(TableX + 40, y + cell / 2 - 16, 100, 20));
            foreach (var entry in entries.Where(e => e.Category == kind.Category))
            {
                int column = entry.Fake ? 5 : (int)entry.Quality;
                float x = gridX + column * step;
                bool found = _state.InJournal(entry.Entry);
                bool enchanted = _state.InJournalEnchanted(entry.Entry);
                var slot = UiKit.MakePanel(found ? Palette.Of(entry.Rarity) : Palette.PanelBorder).Place(x, y, cell - 10, cell - 10);
                slot.TooltipText = !found ? "Not found yet"
                    : $"{entry.Entry}: {entry.Value}g" + (enchanted ? $" (found enchanted too: {entry.Value * Categories.MagicMultiplier}g)" : "");
                slot.MouseFilter = Control.MouseFilterEnum.Pass;
                _page.AddChild(slot);
                if (found)
                {
                    slot.AddChild(UiKit.MakeItemIcon(entry, new Vector2((cell - 10) / 2, (cell - 10) / 2), 48));
                    if (enchanted)
                        slot.AddChild(UiKit.MakeLabel("*", 24, Palette.Magic, outline: 4).Place(cell - 26, -8, 24, 30));
                }
                else
                {
                    slot.AddChild(UiKit.MakeLabel("?", 24, Palette.Muted, HorizontalAlignment.Center).Place(0, 16, cell - 10, 30));
                }
            }
            row++;
        }
        _page.AddChild(UiKit.MakeLabel($"Found {_state.JournalFound} of {Collection.Size}, in every run so far ({_state.Found.Count} this run). " +
                                       "A star: found enchanted too. Hover one for its name and price.", 16, Palette.Muted).Place(TableX, PanelY + PanelH - 34, PanelW - 48, 20));
    }

    /// <summary>
    /// Every curio, a cell each, in the order of the ways of trading they're for: one you've seen (Nim's laid it out, or
    /// you've had it) shows its picture in its rarity's colour; one Nim carries that you haven't met yet is a question
    /// mark; one he keeps back is locked, and says what opens it (a legendary one, with how far along Bram's kin are).
    /// Hover one for the whole story.
    /// </summary>
    void BuildCurios()
    {
        const int columns = 14;
        const float cellW = 76, cellH = 66, gap = 4, gridY = PanelY + 60;
        var profile = Main.Instance.Profile;
        var detail = UiKit.MakeLabel("", 16, Palette.Text).Wrapped();
        detail.Place(TableX, gridY + 6 * (cellH + gap) + 4, PanelW - 48, 44);
        int seen = Curios.All.Count(c => _state.SawCurio(c.Key)), open = Curios.All.Count(c => _state.CurioOpen(c.Key));
        string summary = $"Seen {seen} of {Curios.All.Length}; Nim carries {open} of them this run. Hover one for what it does, or what opens it.";
        for (int i = 0; i < Curios.All.Length; i++)
        {
            var curio = Curios.All[i];
            float x = TableX - 8 + i % columns * (cellW + gap), y = gridY + i / columns * (cellH + gap);
            bool saw = _state.SawCurio(curio.Key), carried = _state.CurioOpen(curio.Key);
            var cell = UiKit.MakePanel(saw ? Palette.Of(curio.Rarity) : Palette.PanelBorder).Place(x, y, cellW, cellH);
            cell.MouseFilter = Control.MouseFilterEnum.Stop;
            _page.AddChild(cell);
            string text;
            if (saw)
            {
                cell.AddChild(UiKit.MakeCurioIcon(curio, new Vector2(cellW / 2, cellH / 2)));
                if (_state.HasCurio(curio.Key)) cell.AddChild(UiKit.MakeLabel("*", 24, Palette.Gold, outline: 4).Place(cellW - 20, -6, 20, 30));
                text = $"{curio.Name} ({Curios.RarityName(curio.Rarity)}; {Curios.Tags(curio)}{(_state.HasCurio(curio.Key) ? "; yours" : "")}): {curio.Blurb}";
            }
            else if (carried)
            {
                cell.AddChild(UiKit.MakeLabel("?", 24, Palette.Muted, HorizontalAlignment.Center).Place(0, 18, cellW, 30));
                text = $"A {Curios.RarityName(curio.Rarity)} curio Nim carries, for {Curios.Tags(curio)}. You haven't seen it yet.";
            }
            else
            {
                var icon = UiKit.MakeCurioIcon(curio, new Vector2(cellW / 2, cellH / 2));
                icon.Modulate = new Color(0.08f, 0.07f, 0.1f, 0.9f);
                cell.AddChild(icon);
                string progress = curio.Unlock == CurioUnlock.Feat && Curios.Feats.TryGetValue(curio.Key, out var feat)
                    ? $" So far: {profile.Done(feat.Feat) + Curios.Progress(_state, feat.Feat)} of {feat.Needed}."
                    : "";
                text = $"Locked: a {Curios.RarityName(curio.Rarity)} curio for {Curios.Tags(curio)}. Nim brings it {Curios.UnlockText(curio)}.{progress}";
            }
            cell.MouseEntered += () => detail.Text = text;
            cell.MouseExited += () => detail.Text = summary;
        }
        detail.Text = summary;
        _page.AddChild(detail);
    }

    void BuildSounds()
    {
        int tier = _state.JournalTier;
        float y = PanelY + 60;
        Header("Kind", TableX + 48, y);
        Header("Sound", TableX + 220, y);
        Header("Shake", TableX + 330, y);
        Header("Weight", TableX + 420, y);
        Header("Note", TableX + 520, y);
        Header("If it's as good as the chest:", TableX + 760, y - 20);
        for (int c = 0; c < 4; c++) Header(Categories.ChestSkin((Rarity)c), TableX + 760 + c * 104, y);

        var kinds = Categories.UpTo(tier).ToList();
        for (int i = 0; i < kinds.Count; i++) _page.AddChild(MakeRow(kinds[i], y + 24 + i * RowH, tier));

        var rules = new[]
        {
            "Light things rattle at a gentle shake, weapons at a firm one, soft things only at a hard one. Click a row to hear it.",
            "While weapons clank, delicate things go unheard: shake gently to hear past it.",
            tier >= 2 ? "Dropped, a chest with a false bottom booms hollow. Glass may break." : null,
            tier >= 3 ? "Two or three of a kind each sound from their own spot: count them. Sand rustles like a scroll, but pours at a gentle shake." : null,
            tier >= 3 ? $"Anything can be enchanted: it sounds the same, but glows under a witchlight. Worth x{Categories.MagicMultiplier}." +
                        (tier >= 4 ? " Demon gold is iron under the gilt: a lodestone silences it." : "") : null,
        }.Where(r => r != null).ToArray();
        _page.AddChild(UiKit.MakeLabel(string.Join("\n", rules.Select(r => "- " + r)), 16, Palette.Text)
            .Place(TableX, y + 34 + kinds.Count * RowH, PanelW - 48, 140));
    }

    void Header(string text, float x, float y) => _page.AddChild(UiKit.MakeLabel(text, 16, Palette.Gold.Darkened(0.2f)).Place(x, y, 200, 20));

    Control MakeRow(CategoryDef kind, float y, int tier)
    {
        var row = new Panel { MouseFilter = Control.MouseFilterEnum.Stop }.Place(TableX - 8, y, PanelW - 32, RowH - 4);
        row.AddThemeStyleboxOverride("panel", UiKit.PanelBox(4));
        row.AddChild(UiKit.MakeKindIcon(kind.Category, new Vector2(28, (RowH - 4) / 2)));
        row.AddChild(UiKit.MakeLabel(kind.Name, 16, Palette.Text).Place(56, 8, 160, 20));
        row.AddChild(UiKit.MakeLabel(kind.Word, 16, Palette.Word).Place(228, 8, 100, 20));
        row.AddChild(UiKit.MakeLabel(Categories.LevelName(kind.StirsAt), 16, Palette.Level(kind.StirsAt)).Place(338, 8, 100, 20));
        row.AddChild(UiKit.MakeLabel(Categories.HeftWord(kind.Category), 16, Palette.Text).Place(428, 8, 100, 20));
        string note = kind.Loud ? "loud: drowns delicate" : kind.Delicate ? "delicate: clanks drown it" : "soft: hard shake only";
        if (kind.FakeName != null && kind.FakeIron && tier >= 4) note = "delicate; demon gold apes it";
        else if (kind.FakeName != null && !kind.FakeIron && tier >= 3) note = "soft; sand sounds alike";
        row.AddChild(UiKit.MakeLabel(note, 16, Palette.Muted).Place(528, 8, 236, 20));
        for (int c = 0; c < 4; c++)
            row.AddChild(UiKit.MakeLabel($"{Categories.Nominal(kind.Category, (Rarity)c)}g", 16, Palette.Of((Rarity)c)).Place(768 + c * 104, 8, 96, 20));
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
        _page.AddChild(UiKit.MakeLabel($"{metCount} of {total} met", 16, Palette.Muted, HorizontalAlignment.Right)
            .Place(PanelX + PanelW - 224, PanelY + PanelH - 32, 200, 20));
        const float buttonW = 180, step = 188;
        for (int i = 0; i <= Spread(tier); i++)
        {
            int which = i;
            _page.AddChild(UiKit.MakeButton(ShopTiers.Name(i + 1), spread == i ? Palette.Gold : Palette.Text, 16, () => ShowRegulars(which))
                .Place(TableX + i * step, PanelY + PanelH - 44, buttonW, 34));
        }
        _page.AddChild(UiKit.MakeButton("Friends & foes", spread == FriendsSpread ? Palette.Gold : Palette.Magic, 16, () => ShowRegulars(FriendsSpread))
            .Place(TableX + (Spread(tier) + 1) * step, PanelY + PanelH - 44, buttonW, 34));

        var people = spread == FriendsSpread ? Friends() : Sellers.All.Where(s => s.Tier <= tier && Spread(s.Tier) == spread && _state.Met.Contains(s.Key))
            .Select(s => (s.Key, s.Name, Role: $"{s.JudgeWord}; {Trade(s)}", About: s.Habit)).ToList();
        if (people.Count == 0)
        {
            _page.AddChild(UiKit.MakeLabel("You haven't met anyone here yet.", 24, Palette.Muted, HorizontalAlignment.Center).Place(PanelX, 300, PanelW, 30));
            return;
        }
        if (people.All(p => p.Key != _selected)) _selected = people.Any(p => p.Key == _current) ? _current : people[0].Key;
        for (int i = 0; i < people.Count; i++) Entry(i, people[i].Key, people[i].Name, people[i].Role, spread);
        var chosen = people.First(p => p.Key == _selected);
        Page(chosen.Key, chosen.Name, chosen.Role, chosen.About);
    }

    static string Trade(SellerDef seller) => seller.Trade switch { Core.Trade.Haggle => "haggles", Core.Trade.Offer => "you name the price", _ => "fixed price" };

    /// <summary>Uncle Bram (always) and the story's people you've met: Nim, Mister Vane, the Hooded Woman, who's Mara.</summary>
    System.Collections.Generic.List<(string Key, string Name, string Role, string About)> Friends()
    {
        var people = new System.Collections.Generic.List<(string Key, string Name, string Role, string About)>
        {
            (Story.Bram, Hints.Mentor, "your mentor", "Keywright at the Brindle crossroads for forty years, until his Key-mark faded and showed on " +
                                                  "your hand. Keeps a stash for bad days and a tip for everything. Never paid the Guild a penny."),
        };
        if (_state.HasMet(Story.Nim))
            people.Add((Story.Nim, Peddler.Name, "sells curios", "Comes by twice a week with oddments from the Locking, each bending one rule of the trade. " +
                                                             "Notices what you buy and brings more like it. Don't ask what Nim is."));
        if (_state.HasMet(Story.Vane))
            people.Add((Story.Vane, Guild.Collector, "the Gilded Guild", "Collector for the Guild that keeps the Gate. Comes at the end of every week for " +
                                                                    "the tithe Bram never paid. Miss one, and the Guild seals the shop."));
        if (_state.HasMet(Story.Mara))
            people.Add((Story.Mara, "Mara", "the Hooded Woman", Story.VaultSeller.Habit + " She trades the Key's shards for what she asks."));
        else if (_state.HasMet(Story.Hood))
            people.Add((Story.Hood, Story.HoodName, "trades shards", "Knows Bram, and the Key-mark on your hand. Trades shards of the Aldric Key for " +
                                                                 "things she asks for, and won't say who she is."));
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
        row.TooltipText = $"{name}: {role}. Click for their page.";
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
        row.AddChild(UiKit.MakeSprite($"res://assets/sprites/sellers/faces/{key}.png", new Vector2(26, h / 2), 1));
        row.AddChild(UiKit.MakeLabel(name, 16, current ? Palette.Gold : Palette.Text).Place(52, 4, 300, 20));
        row.AddChild(UiKit.MakeLabel(current ? "at your counter" : role, 16, current ? Palette.Gold : Palette.Muted).Place(52, 24, 500, 20));
    }

    /// <summary>The open page under the list: a big face, the name, how they deal, and everything you know of them.</summary>
    void Page(string key, string name, string role, string about)
    {
        float y = PanelY + 56 + 4 * 52 + 6;
        var page = UiKit.MakePanel(key == _current ? Palette.Gold.Darkened(0.2f) : Palette.PanelBorder).Place(TableX - 8, y, PanelW - 32, PanelY + PanelH - 52 - y);
        page.MouseFilter = Control.MouseFilterEnum.Ignore;
        _page.AddChild(page);
        page.AddChild(UiKit.MakeSprite($"res://assets/sprites/sellers/{key}.png", new Vector2(56, 58), 1));
        page.AddChild(UiKit.MakeLabel(name, 24, Palette.Gold, outline: 4).Place(112, 10, 600, 30));
        page.AddChild(UiKit.MakeLabel(role, 16, Palette.Muted, HorizontalAlignment.Right).Place(600, 16, PanelW - 660, 20));
        page.AddChild(UiKit.MakeLabel(about, 16, Palette.Text).Wrapped().Place(112, 44, PanelW - 168, 96));
    }

    /// <summary>
    /// Three weeks, a row each, a day to a box: today framed in gold and the days gone by faded. Each day's business
    /// (this week's and next week's are planned): a story morning, something on in town, Nim's curios, and the Guild's
    /// night with what it'll want. Hover a day for the whole story; until then, the next thing coming is told below.
    /// </summary>
    void BuildCalendar()
    {
        int today = _state.Day;
        string due = _state.DebtCleared ? $"Bram's debt is cleared; the Guild still comes for {_state.NextDue}g on day {today + _state.DaysToNextDue}."
            : _state.DuesTonight && !_state.PaidThisWeek ? $"The Guild comes tonight for {_state.DueNow}g."
            : $"The Guild comes next for {_state.NextDue}g at the end of day {today + _state.DaysToNextDue}.";
        // The Guild's nights show once Vane has introduced them (the second evening).
        bool guild = _state.GuildIntroduced;
        _page.AddChild(UiKit.MakeLabel($"Today is day {today}: week {_state.Week}, day {_state.DayOfWeek} of {Guild.DaysPerWeek}.{(guild ? $" {due}" : "")}", 16, Palette.Muted)
            .Place(TableX, PanelY + 60, PanelW - 48, 20));

        // Five days to a week: a story morning or Nim's, two days with something on, and the Guild's night.
        const float cellX = 176, cellW = 200, gap = 8, rowH = 108, gridY = PanelY + 90;
        var detail = UiKit.MakeLabel("", 16, Palette.Text).Wrapped();
        detail.Place(TableX, gridY + 3 * rowH + 8, PanelW - 48, 64);
        string upcoming = null;
        for (int row = 0; row < 3; row++)
        {
            int week = _state.Week + row;
            var plan = _state.PlanFor(week);
            float y = gridY + row * rowH;
            _page.AddChild(UiKit.MakeLabel($"WEEK {week}", 16, Palette.Gold).Place(TableX, y + 8, 110, 20));
            _page.AddChild(UiKit.MakeLabel(row == 0 ? "this week" : row == 1 ? "next week" : "not planned", 16, Palette.Muted).Place(TableX, y + 28, 110, 20));
            if (week > Guild.Weeks)
                _page.AddChild(UiKit.MakeLabel("after the debt", 16, Palette.Muted).Place(TableX, y + 48, 116, 20));
            for (int d = 1; d <= Guild.DaysPerWeek; d++)
            {
                int day = GameState.DayNumber(week, d);
                bool isToday = day == today, past = day < today;
                var cell = UiKit.MakePanel(isToday ? Palette.Gold : Palette.PanelBorder).Place(cellX + (d - 1) * (cellW + gap), y, cellW, rowH - 8);
                cell.MouseFilter = Control.MouseFilterEnum.Stop;
                if (past) cell.Modulate = new Color(1, 1, 1, 0.45f);
                cell.AddChild(UiKit.MakeLabel($"DAY {day}", 16, isToday ? Palette.Gold : Palette.Muted).Place(10, 6, 100, 20));
                if (isToday) cell.AddChild(UiKit.MakeLabel("today", 16, Palette.Gold, HorizontalAlignment.Right).Place(cellW - 90, 6, 80, 20));

                var lines = new System.Collections.Generic.List<string>();
                const float ly = 30;
                var role = plan?.Role(d) ?? (d == Guild.DaysPerWeek ? DayRole.Tax : DayRole.None);
                if (role == DayRole.Story)
                {
                    cell.AddChild(UiKit.MakeLabel(Story.Teaser(week), 16, Palette.Gold).Wrapped().Place(10, ly, cellW - 20, 40));
                    lines.Add($"A story morning: {Story.Teaser(week).ToLowerInvariant()}, before you open. Nim doesn't come this week but once.");
                }
                else if (role == DayRole.Event && Events.Find(plan.On(d)) is EventDef e)
                {
                    cell.AddChild(UiKit.MakeLabel(Events.Tag(e, plan.Wanted), 16, Palette.Magic).Wrapped().Place(10, ly, cellW - 20, 40));
                    lines.Add($"{e.Name}: {Events.Describe(e, plan.Wanted)}");
                }
                else if (role == DayRole.Curio)
                {
                    cell.AddChild(UiKit.MakeLabel("Nim: curios", 16, Palette.Good).Place(10, ly, cellW - 20, 20));
                    lines.Add("Nim the peddler comes by in the morning with curios.");
                }
                if (role == DayRole.Tax && guild)
                {
                    bool paid = week <= _state.WeeksPaid;
                    int owed = week == _state.Week ? _state.DueNow : _state.DueFor(week);
                    string text = paid ? "Guild: paid" : $"Guild: {owed}g";
                    var colour = paid ? Palette.Good : week == _state.Week && _state.Gold < owed ? Palette.Warn : Palette.Text;
                    cell.AddChild(UiKit.MakeLabel(text, 16, colour).Place(10, ly, cellW - 20, 20));
                    lines.Add(paid ? "The Guild's night: this week's tithe is already paid." : $"The Guild's night: Mister Vane comes for {owed}g after closing.");
                }
                else if (role == DayRole.Tax)
                    cell.AddChild(UiKit.MakeLabel("The week's end", 16, Palette.Muted).Place(10, ly, cellW - 20, 20));
                if (plan == null && d == 1)
                    lines.Add("Next week's plan is made at the start of this one: its story, its days in town, and Nim's.");
                string story = $"Day {day}{(isToday ? " (today)" : "")}: " + (lines.Count == 0 ? "nothing special planned." : string.Join(" ", lines));
                if (!past && !isToday && upcoming == null && lines.Count > 0) upcoming = "Next: " + story;
                cell.MouseEntered += () => detail.Text = story;
                cell.MouseExited += () => detail.Text = upcoming ?? "";
                _page.AddChild(cell);
            }
        }
        detail.Text = upcoming ?? "";
        _page.AddChild(detail);
    }

    static string Capital(string text) => char.ToUpperInvariant(text[0]) + text[1..];

    void ShowRegulars(int spread)
    {
        foreach (var child in _page.GetChildren()) child.QueueFree();
        BuildRegulars(spread);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.L or Key.Escape })
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
