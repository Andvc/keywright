using System;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// The collection, from the title screen or the pause menu: what Bram's kin have gathered in every run. Finds: every
/// thing, kind by quality, as it came out of the chest (a star if it's been found enchanted too). Curios: every curio,
/// the ones you've seen, the ones Nim carries, and what opens the rest. Heartsongs: the nine, the ones heard and what
/// each does on display. The notebook (<see cref="NotebookView"/>) keeps what's for trading.
/// </summary>
public partial class CollectionView : Node2D
{
    const float PanelX = 40, PanelY = 100, PanelW = 1200, PanelH = 518;
    const float TableX = 64;

    public const int FindsPage = 0, CuriosPage = 1, SongsPage = 2;

    readonly Collected _c;
    readonly int _firstPage;
    Node2D _page;
    Button[] _tabs;
    bool _closing;

    public event Action Closed;

    public CollectionView() { }

    public CollectionView(Collected collected, int page = FindsPage)
    {
        _c = collected;
        _firstPage = page;
    }

    static Audio.Sfx Sfx => Main.Instance.Sfx;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        ZIndex = 40;
        var blocker = new ColorRect { Color = new Color(0, 0, 0, 0.75f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920);
        blocker.GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true }) Close();
        };
        AddChild(blocker);
        AddChild(UiKit.MakePanel(Palette.Gold.Darkened(0.3f)).Place(PanelX, PanelY, PanelW, PanelH));
        AddChild(UiKit.MakeLabel("COLLECTION", 32, Palette.Gold, outline: 4).Place(PanelX + 24, PanelY + 10, 260, 40));
        _tabs = new[]
        {
            UiKit.MakeButton("Finds", Palette.Gold, 16, () => ShowPage(FindsPage)).Place(PanelX + 270, PanelY + 14, 96, 36),
            UiKit.MakeButton("Curios", Palette.Magic, 16, () => ShowPage(CuriosPage)).Place(PanelX + 372, PanelY + 14, 96, 36),
            UiKit.MakeButton("Heartsongs", Palette.Of(Rarity.Mythic), 16, () => ShowPage(SongsPage)).Place(PanelX + 474, PanelY + 14, 136, 36),
        };
        foreach (var tab in _tabs) AddChild(tab);
        AddChild(UiKit.MakeButton("Close", Palette.Text, 16, Close).Place(PanelX + PanelW - 144, PanelY + 12, 120, 40));
        _page = new Node2D();
        AddChild(_page);
        ShowPage(_firstPage);

        Sfx.PlayGroup("book", 1f, -4);
        Modulate = Colors.Transparent;
        CreateTween().TweenProperty(this, "modulate", Colors.White, 0.15f);
    }

    /// <summary>0: finds, 1: curios, 2: Heartsongs.</summary>
    public void ShowPage(int page)
    {
        foreach (var child in _page.GetChildren()) child.QueueFree();
        for (int i = 0; i < _tabs.Length; i++) _tabs[i].Modulate = i == page ? Colors.White : new Color(1, 1, 1, 0.6f);
        if (page == SongsPage) BuildSongs();
        else if (page == CuriosPage) BuildCurios();
        else BuildFinds();
    }

    /// <summary>
    /// A grid: one row per kind, one column per quality, then the fake. Found things show as they came out of the chest;
    /// the rest are "?". A star means one's been found enchanted too.
    /// </summary>
    void BuildFinds()
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
                bool found = _c.Found(entry.Entry);
                bool enchanted = _c.FoundEnchanted(entry.Entry);
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
        string thisRun = _c.InRun ? $" ({_c.FoundThisRun} this run)" : "";
        _page.AddChild(UiKit.MakeLabel($"Found {_c.FoundCount} of {Collection.Size}, in every run so far{thisRun}. " +
                                       "A star: found enchanted too. Hover one for its name and price.", 16, Palette.Muted)
            .Place(TableX, PanelY + PanelH - 34, PanelW - 48, 20));
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
        var detail = UiKit.MakeLabel("", 16, Palette.Text).Wrapped();
        detail.Place(TableX, gridY + 6 * (cellH + gap) + 4, PanelW - 48, 44);
        int seen = Curios.All.Count(c => _c.SawCurio(c.Key)), open = Curios.All.Count(c => _c.Carried(c.Key));
        string summary = $"Seen {seen} of {Curios.All.Length}; Nim carries {open} of them {(_c.InRun ? "this run" : "now")}. " +
                         "Hover one for what it does, or what opens it.";
        for (int i = 0; i < Curios.All.Length; i++)
        {
            var curio = Curios.All[i];
            float x = TableX - 8 + i % columns * (cellW + gap), y = gridY + i / columns * (cellH + gap);
            bool saw = _c.SawCurio(curio.Key), carried = _c.Carried(curio.Key), yours = _c.Owns(curio.Key);
            var cell = UiKit.MakePanel(saw ? Palette.Of(curio.Rarity) : Palette.PanelBorder).Place(x, y, cellW, cellH);
            cell.MouseFilter = Control.MouseFilterEnum.Stop;
            _page.AddChild(cell);
            string text;
            if (saw)
            {
                cell.AddChild(UiKit.MakeCurioIcon(curio, new Vector2(cellW / 2, cellH / 2)));
                if (yours) cell.AddChild(UiKit.MakeLabel("*", 24, Palette.Gold, outline: 4).Place(cellW - 20, -6, 20, 30));
                text = $"{curio.Name} ({Curios.RarityName(curio.Rarity)}; {Curios.Tags(curio)}{(yours ? "; yours" : "")}): {curio.Blurb}";
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
                    ? $" So far: {_c.FeatDone(feat.Feat)} of {feat.Needed}."
                    : "";
                text = $"Locked: a {Curios.RarityName(curio.Rarity)} curio for {Curios.Tags(curio)}. Nim brings it {Curios.UnlockText(curio)}.{progress}";
            }
            cell.MouseEntered += () => detail.Text = text;
            cell.MouseExited += () => detail.Text = summary;
        }
        detail.Text = summary;
        _page.AddChild(detail);
    }

    /// <summary>
    /// The songbook: the nine Heartsongs, three by three. One the family has heard shows its picture, who loved it and
    /// what it does on display (its story on hover), and whether it's singing in the shop now; the rest are "?".
    /// </summary>
    void BuildSongs()
    {
        int known = _c.SongsKnown;
        _page.AddChild(UiKit.MakeLabel("Nine things loved more than gold, sung into the Locking and hidden in chests among millions. Only a " +
                                       "Keywright hears one sing; to anyone else it's worth nothing. Hover a card for its story. " +
                                       $"Bram's kin know {known} of the nine" +
                                       (known == Heartsongs.All.Length ? ": the Crown Chest can be sung whole." : ": with all nine, the Crown Chest's ending changes."),
            16, Palette.Muted).Wrapped().Place(TableX, PanelY + 56, PanelW - 48, 60));
        const float cardW = 376, cardH = 128, gap = 8;
        for (int i = 0; i < Heartsongs.All.Length; i++)
        {
            var song = Heartsongs.All[i];
            bool found = _c.KnowsSong(song.Key);
            bool singing = _c.Singing(song.Key);
            float x = TableX - 8 + i % 3 * (cardW + gap), y = PanelY + 118 + i / 3 * (cardH + 4);
            var card = new Panel { MouseFilter = Control.MouseFilterEnum.Pass }.Place(x, y, cardW, cardH);
            card.AddThemeStyleboxOverride("panel", UiKit.PanelBox(4));
            if (singing) card.AddChild(UiKit.MakeFrame(Palette.Of(Rarity.Mythic)));
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
            if (singing) card.AddChild(UiKit.MakeLabel("singing", 16, Palette.Gold, HorizontalAlignment.Right).Place(cardW - 100, 6, 92, 20));
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
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
