using System;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// Esc (or the Menu button) during a day: the game stops while it's up. Resume, the collection (what Bram's kin have
/// gathered, this run and every one before), the settings, back to the title screen, or quit. The shop is saved at the start of each day, so leaving mid-day starts that day over. In GM mode
/// (F1, since 0.37; it was a setting), a panel beside it adds gold, skips the day, fills the journal, or fulfils
/// everything the story asks of the season.
/// </summary>
public partial class PauseMenu : CanvasLayer
{
    const float W = 420, H = 462, X = (1280 - W) / 2, Y = (720 - H) / 2;

    SettingsView _settings;
    CollectionView _collection;

    public override void _Ready()
    {
        Layer = 60;
        ProcessMode = ProcessModeEnum.Always;
        GetTree().Paused = true;
        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore }.Place(0, 0, 1280, 720);
        AddChild(root);
        root.AddChild(new ColorRect { Color = new Color(0, 0, 0, 0.65f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920));
        root.AddChild(UiKit.MakePanel(Palette.Gold.Darkened(0.3f)).Place(X, Y, W, H));
        root.AddChild(UiKit.MakeLabel(Loc.T("PAUSED"), 32, Palette.Gold, HorizontalAlignment.Center, outline: 4).Place(X, Y + 18, W, 40));
        float y = Y + 80;
        root.AddChild(UiKit.MakeButton(Loc.T("Resume"), Palette.Gold, 24, Resume).Place(X + 60, y, W - 120, 52));
        root.AddChild(UiKit.MakeButton(Loc.T("Collection"), Palette.Text, 16, () => OpenCollection()).Place(X + 60, y + 66, W - 120, 44));
        root.AddChild(UiKit.MakeButton(Loc.T("Settings"), Palette.Text, 16, OpenSettings).Place(X + 60, y + 124, W - 120, 44));
        root.AddChild(UiKit.MakeButton(Loc.T("Quit to title"), Palette.Text, 16, () => Leave(toTitle: true)).Place(X + 60, y + 182, W - 120, 44));
        root.AddChild(UiKit.MakeButton(Loc.T("Quit game"), Palette.Bad, 16, () => Leave(toTitle: false)).Place(X + 60, y + 240, W - 120, 44));
        root.AddChild(UiKit.MakeLabel(Loc.T("The shop is saved at the start of each day: leave now and today starts over."), 16, Palette.Muted,
            HorizontalAlignment.Center).Wrapped().Place(X + 24, y + 296, W - 48, 44));
        if (Main.Instance.State is { Challenge: > 0 } state) BuildTerms(root, state);
        _root = root;
        if (Settings.Gm && Main.Instance.State != null) BuildGm(root);
        Main.Instance.Sfx.PlayGroup("book", 1.2f, -8);
    }

    Control _root;

    /// <summary>A challenge run's level, what it does to the dues, and its boons and banes (each explained on hover).</summary>
    static void BuildTerms(Control root, Core.GameState state)
    {
        const float tw = 260, tx = X - 24 - tw, th = 540, ty = (720 - th) / 2;
        int level = state.Challenge;
        root.AddChild(UiKit.MakePanel(Palette.Warn.Darkened(0.3f)).Place(tx, ty, tw, th));
        root.AddChild(UiKit.MakeLabel(Loc.T($"LEVEL {level}"), 32, Palette.Warn, HorizontalAlignment.Center, outline: 4).Place(tx, ty + 18, tw, 40));
        root.AddChild(UiKit.MakeLabel(Loc.T($"The Guild's dues +{Core.Synergies.Pct(Core.Challenges.DebtFactor(level) - 1)}"), 16, Palette.Warn,
            HorizontalAlignment.Center).Place(tx, ty + 62, tw, 20));
        float y = UiKit.MakeTermList(root, Loc.T("FOR YOU"), state.Boons, Palette.Good, tx + 20, ty + 96, tw - 40, state.Boons.Count);
        y = UiKit.MakeTermList(root, Loc.T("AGAINST YOU"), state.Banes, Palette.Warn, tx + 20, y + 10, tw - 40, state.Banes.Count);
        root.AddChild(UiKit.MakeLabel(Loc.T("Hover over one to read it."), 16, Palette.Muted, HorizontalAlignment.Center).Place(tx, Math.Max(y + 8, ty + th - 36), tw, 20));
    }

    Label _gmNote;

    /// <summary>GM mode: 99,999 gold, the rest of the day skipped, every page of the journal filled, the season's quests fulfilled.</summary>
    void BuildGm(Control root)
    {
        if (_gmNote != null) return;
        // Taller than the menu beside it: what "Fulfil the quests" did takes eight lines to say in French.
        const float gx = X + W + 24, gw = 260, gh = H + 96;
        root.AddChild(UiKit.MakePanel(Palette.Magic.Darkened(0.3f)).Place(gx, Y, gw, gh));
        root.AddChild(UiKit.MakeLabel(Loc.T("GM"), 32, Palette.Magic, HorizontalAlignment.Center, outline: 4).Place(gx, Y + 18, gw, 40));
        float y = Y + 80;
        root.AddChild(UiKit.MakeButton(Loc.T("+99,999 gold"), Palette.Gold, 16, () => Gm(Main.Instance.GmGold())).Place(gx + 20, y, gw - 40, 44));
        root.AddChild(UiKit.MakeButton(Loc.T("Skip the day"), Palette.Text, 16, SkipDay).Place(gx + 20, y + 58, gw - 40, 44));
        root.AddChild(UiKit.MakeButton(Loc.T("Unlock the journal"), Palette.Magic, 16, () => Gm(Main.Instance.GmUnlockJournal())).Place(gx + 20, y + 116, gw - 40, 44));
        // One click for everything the story asks (0.37): the key, the commissions, the debt, the true ending's checks.
        var fulfil = UiKit.MakeButton(Loc.T("Fulfil the quests"), Palette.Good, 16, () => Gm(Main.Instance.GmFulfil())).Place(gx + 20, y + 174, gw - 40, 44);
        fulfil.TooltipText = Loc.T("The treasury key's four pieces, this week's commissions, Bram's account paid off, and the true ending's checks: the final opening comes tonight.");
        root.AddChild(fulfil);
        _gmNote = UiKit.MakeLabel(Loc.T("For testing: nothing here is fair."), 16, Palette.Muted, HorizontalAlignment.Center).Wrapped();
        _gmNote.ClipText = true;
        root.AddChild(_gmNote.Place(gx + 12, y + 228, gw - 24, gh - 80 - 228 - 12));
    }

    void Gm(string done) => _gmNote.Text = done;

    void SkipDay()
    {
        string why = Main.Instance.CanSkipDay;
        if (why != null)
        {
            Gm(why);
            return;
        }
        Resume();
        Main.Instance.GmSkipDay();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if ((e.IsActionPressed(Controls.Pause) || e.IsActionPressed(Controls.Back)) && _settings == null && _collection == null)
        {
            GetViewport().SetInputAsHandled();
            Resume();
        }
        // F1 with the menu already up: the GM's panel beside it.
        else if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.F1 } && Main.Instance.State != null)
        {
            GetViewport().SetInputAsHandled();
            Settings.Gm = true;
            BuildGm(_root);
        }
    }

    /// <summary>The collection, over the pause menu: every run's finds, curios and Heartsongs, with this run's.</summary>
    public CollectionView OpenCollection(int page = CollectionView.FindsPage)
    {
        if (_collection != null) return _collection;
        _collection = new CollectionView(new Core.Collected(Main.Instance.Profile, Main.Instance.State), page);
        _collection.Closed += () => _collection = null;
        AddChild(_collection);
        return _collection;
    }

    void OpenSettings()
    {
        if (_settings != null) return;
        _settings = new SettingsView();
        _settings.Closed += () => _settings = null;
        AddChild(_settings);
    }

    public void Resume()
    {
        GetTree().Paused = false;
        QueueFree();
    }

    void Leave(bool toTitle)
    {
        GetTree().Paused = false;
        QueueFree();
        if (toTitle) Main.Instance.ToTitle();
        else Main.Instance.QuitGame();
    }
}
