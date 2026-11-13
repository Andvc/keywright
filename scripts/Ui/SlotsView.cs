using System;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// The saves (0.37), from the title screen: three of them, each with a profile and a season of its own (what's been
/// taught and found, the campaign, the season in progress). A card each: what's on it, Play to switch to it, and Erase
/// to start it completely fresh (asked twice over; the files are set aside, not destroyed: <see cref="Storage.Erase"/>).
/// An empty save is a first season, with everything to learn.
/// </summary>
public partial class SlotsView : Node2D
{
    const float PanelX = 100, PanelY = 110, PanelW = 1080, PanelH = 500, CardW = 332, CardH = 360, CardGap = 24;

    bool _closing;
    Control _confirm;
    Node2D _cards;
    public event Action Closed;

    /// <summary>A save was switched to or erased: the title screen starts again on it.</summary>
    public event Action Changed;

    static Audio.Sfx Sfx => Main.Instance.Sfx;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        ZIndex = 40;
        AddChild(new ColorRect { Color = new Color(0, 0, 0, 0.75f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920));
        AddChild(UiKit.MakePanel(Palette.Gold.Darkened(0.3f)).Place(PanelX, PanelY, PanelW, PanelH));
        AddChild(UiKit.MakeLabel(Loc.T("SAVES"), 32, Palette.Gold, HorizontalAlignment.Center, outline: 4).Place(PanelX, PanelY + 12, PanelW, 40));
        AddChild(UiKit.MakeButton(Loc.T("Close"), Palette.Text, 16, Close).Place(PanelX + PanelW - 140, PanelY + 16, 116, 36));
        AddChild(UiKit.MakeLabel(Loc.T("Each save keeps its own seasons, what's been learned and found, and its own story."), 16, Palette.Muted,
            HorizontalAlignment.Center).Place(PanelX, PanelY + 58, PanelW, 20));
        Build();
        Main.Instance.Moment("40_saves", 0.3);
    }

    void Build()
    {
        _cards?.QueueFree();
        _cards = new Node2D();
        AddChild(_cards);
        float x0 = PanelX + (PanelW - (Settings.Slots * (CardW + CardGap) - CardGap)) / 2;
        for (int slot = 1; slot <= Settings.Slots; slot++) _cards.AddChild(MakeCard(slot, x0 + (slot - 1) * (CardW + CardGap), PanelY + 96));
    }

    Control MakeCard(int slot, float x, float y)
    {
        bool playing = slot == Settings.Slot;
        var (profile, save) = Main.PeekSlot(slot);
        bool empty = profile == null;
        var card = UiKit.MakePanel(playing ? Palette.Gold : Palette.PanelBorder.Lightened(0.1f)).Place(x, y, CardW, CardH);
        card.AddChild(UiKit.MakeLabel(Loc.T($"SAVE {slot}"), 24, playing ? Palette.Gold : Palette.Text, outline: 4).Place(18, 12, 180, 30));
        if (playing) card.AddChild(UiKit.MakeLabel(Loc.T("playing"), 16, Palette.Good, HorizontalAlignment.Right).Place(CardW - 138, 18, 120, 20));

        string body;
        if (empty) body = Loc.T("Empty. A first season, with everything to learn.");
        else
        {
            string seasons = Loc.Plural(profile.Runs, $"{profile.Runs} season played", $"{profile.Runs} seasons played");
            string story = profile.Finished ? Loc.T("The story's told: the next day came.")
                : profile.Swallowed > 0 ? Loc.T("You remember the treasury.")
                : profile.Cleared > 0 ? Loc.T("Bram's debt has been cleared.")
                : Loc.T("Bram's debt hasn't been cleared yet.");
            string reached = profile.BestTier > 0 ? Loc.T($"As far as {ShopTiers.Name(Math.Clamp(profile.BestTier, 1, ShopTiers.Max))}.") : "";
            string now = save != null ? Loc.T($"In progress: {save.Summary}") : Loc.T("No season in progress.");
            var lines = new System.Collections.Generic.List<string> { seasons, story };
            if (reached.Length > 0) lines.Add(reached);
            lines.Add(now);
            body = string.Join("\n", lines);
        }
        card.AddChild(UiKit.MakeLabel(body, 16, empty ? Palette.Muted : Palette.Text).Wrapped().Place(18, 56, CardW - 36, 176));

        if (!playing)
        {
            card.AddChild(UiKit.MakeButton(empty ? Loc.T("Start a new save") : Loc.T("Play this save"), Palette.Gold, 16, () => Play(slot)).Place(18, CardH - 112, CardW - 36, 44));
        }
        else if (empty)
        {
            card.AddChild(UiKit.MakeLabel(Loc.T("Close this, and choose New game."), 16, Palette.Muted, HorizontalAlignment.Center).Wrapped()
                .Place(18, CardH - 112, CardW - 36, 44));
        }
        if (!empty)
        {
            var erase = UiKit.MakeButton(Loc.T("Erase, and start fresh"), Palette.Bad, 16, () => AskErase(slot)).Place(18, CardH - 58, CardW - 36, 40);
            erase.TooltipText = Loc.T("Everything on this save is set aside (not deleted: its files stay in the save folder under another name), and it starts from a first season.");
            card.AddChild(erase);
        }
        return card;
    }

    /// <summary>Switches to a save, and the title begins again on it.</summary>
    public void Play(int slot)
    {
        if (_closing) return;
        _closing = true;
        Sfx?.Play("chime", 1.1f, -8);
        Changed?.Invoke();
        Main.Instance.UseSlot(slot);
    }

    /// <summary>Erasing takes a yes: it's everything a save has learned, found and reached.</summary>
    void AskErase(int slot)
    {
        if (_confirm != null) return;
        _confirm = UiKit.MakeConfirm(Loc.T($"ERASE SAVE {slot}?"),
            Loc.T("Its seasons, everything learned and found on it, and its story start over from nothing. The other saves aren't touched."),
            Palette.Bad, Loc.T("Erase it"), () => Erase(slot), Loc.T("Keep it"), CloseConfirm);
        AddChild(_confirm);
    }

    void CloseConfirm()
    {
        _confirm?.QueueFree();
        _confirm = null;
    }

    /// <summary>Erases a save (the bot's shell demo uses this too): the one being played starts fresh at the title.</summary>
    public void Erase(int slot)
    {
        CloseConfirm();
        Sfx?.Play("lose", 1.1f, -8);
        bool playing = slot == Settings.Slot;
        Main.Instance.EraseSlot(slot);
        if (playing)
        {
            _closing = true;
            Changed?.Invoke();
            Main.Instance.UseSlot(slot);
            return;
        }
        Build();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed(Controls.Back) && !e.IsActionPressed(Controls.Pause)) return;
        GetViewport().SetInputAsHandled();
        if (_confirm != null) CloseConfirm();
        else Close();
    }

    public void Close()
    {
        if (_closing) return;
        _closing = true;
        Sfx?.Play("tick");
        Closed?.Invoke();
        QueueFree();
    }
}
