using System;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// What the echo horn kept (0.35): everything said to you this run, oldest first and the newest in view, a day at a
/// time, each line under who said it (Bram's tips and lessons in gold, sellers at the counter in white, people's lines
/// in their own colour), the trade's words coloured as they were. Esc, the notebook key or a click outside closes it.
/// </summary>
public partial class HistoryView : Node2D
{
    const float PanelX = 140, PanelY = 66, PanelW = 1000, PanelH = 560;

    readonly GameState _state;
    bool _closing;

    public event Action Closed;

    public HistoryView() { }

    public HistoryView(GameState state) => _state = state;

    /// <summary>How many are open: notes over the top bar wait meanwhile, as they do for the notebook.</summary>
    public static int Open { get; private set; }

    public override void _EnterTree() => Open++;

    public override void _ExitTree() => Open = Math.Max(0, Open - 1);

    public override void _Ready()
    {
        Main.Instance?.HideNote();
        var blocker = new ColorRect { Color = new Color(0, 0, 0, 0.7f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920);
        blocker.GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) Close();
        };
        AddChild(blocker);
        AddChild(UiKit.MakePanel(Palette.Gold.Darkened(0.3f)).Place(PanelX, PanelY, PanelW, PanelH));
        AddChild(UiKit.MakeLabel(Loc.T("THE ECHO HORN"), 32, Palette.Gold, outline: 4).Place(PanelX + 24, PanelY + 10, 600, 40));
        AddChild(UiKit.MakeLabel(Loc.T("Everything said across your counter this season, as the horn kept it."), 16, Palette.Muted)
            .Place(PanelX + 24, PanelY + 52, PanelW - 200, 20));
        // As wide as its words need (French's "Fermer [Échap]" is wider than English's).
        string close = Loc.T($"Close {Controls.Tag(Controls.Back)}");
        float closeW = Math.Max(120, UiKit.TextWidth(close) + 32);
        AddChild(UiKit.MakeButton(close, Palette.Text, 16, Close).Place(PanelX + PanelW - 24 - closeW, PanelY + 12, closeW, 40));

        var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }.Place(PanelX + 20, PanelY + 84, PanelW - 40, PanelH - 104);
        AddChild(scroll);
        var list = new VBoxContainer { CustomMinimumSize = new Vector2(PanelW - 64, 0) };
        list.AddThemeConstantOverride("separation", 6);
        scroll.AddChild(list);
        var said = _state?.History ?? new();
        if (said.Count == 0)
            list.AddChild(UiKit.MakeLabel(Loc.T("Nothing said yet. Whatever's said at the counter, the horn keeps."), 16, Palette.Muted));
        int day = -1;
        foreach (var line in said)
        {
            if (line.Day != day)
            {
                day = line.Day;
                list.AddChild(UiKit.MakeLabel(Loc.T($"Day {day}"), 16, Palette.Gold.Darkened(0.15f), HorizontalAlignment.Center));
            }
            var who = UiKit.MakeLabel(line.Kind == Heard.Lesson ? Loc.T($"{line.Who}, a lesson") : line.Who, 16, Colour(line.Kind));
            list.AddChild(who);
            var text = Rich.Make(16, Palette.Text);
            text.FitContent = true;
            text.CustomMinimumSize = new Vector2(PanelW - 96, 0);
            // Bram's tips and lessons with the trade's words coloured, as he said them; anyone else's with how they shook.
            text.Text = line.Kind is Heard.Tip or Heard.Lesson ? Rich.Trade(line.Text) : Rich.Mark(line.Text, Rich.Strengths);
            var indent = new MarginContainer();
            indent.AddThemeConstantOverride("margin_left", 24);
            indent.AddChild(text);
            list.AddChild(indent);
        }
        // The newest in view: the scroll waits a frame for the list to be laid out.
        Callable.From(() => scroll.ScrollVertical = int.MaxValue).CallDeferred();
        Main.Instance?.Sfx.Play("blip", 0.7f, -8);
    }

    /// <summary>Whose line it was, by its colour.</summary>
    static Color Colour(Heard kind) => kind switch
    {
        Heard.Tip or Heard.Lesson => Palette.Gold,
        Heard.Seller => Palette.Text,
        _ => Palette.Magic,
    };

    /// <summary>
    /// Back or the notebook key closes it; no other key reaches what's behind it (the talk's next line, the notebook)
    /// while it's open. The mouse wheel scrolls it, and a controller's pointer still clicks.
    /// </summary>
    public override void _Input(InputEvent e)
    {
        if (_closing) return;
        if (e.IsActionPressed(Controls.Back) || e.IsActionPressed(Controls.Notebook))
        {
            GetViewport().SetInputAsHandled();
            Close();
        }
        else if (e is InputEventKey || e.IsActionPressed(Controls.Act) || e.IsActionPressed(Controls.Pause) || e.IsActionPressed(Controls.Force))
            GetViewport().SetInputAsHandled();
    }

    public void Close()
    {
        if (_closing) return;
        _closing = true;
        Closed?.Invoke();
        QueueFree();
    }
}
