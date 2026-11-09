using System;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// Bram's echo horn on the counter (0.35, docs/world.md): a Keyhold clerk's horn from the Locking, which keeps what's
/// said across a counter and says it back. Click it for the run's history (<see cref="HistoryView"/>). Drawn in code at
/// the 3px art's grain, a brass bell on a little wooden box, until it has a picture of its own.
/// </summary>
public partial class EchoHorn : Control
{
    const int Cell = 3, Cells = 22;

    /// <summary>Where it stands at the counter: on its far end, left of whoever's behind it (the counter's top is y 476).</summary>
    public static readonly Vector2 AtCounter = new(6, 476 - 20 * Cell);
    static readonly Color Brass = new("#c9963a"), BrassLight = new("#f2c96e"), BrassDark = new("#7a5418"), Mouth = new("#24160e");

    readonly GameState _state;
    Painter _art;
    bool _hover;

    public EchoHorn() { }

    public EchoHorn(GameState state) => _state = state;

    /// <summary>The history it opened, over the phase it stands in (the bot closes it again).</summary>
    public event Action<HistoryView> Opened;

    public override void _Ready()
    {
        Size = new Vector2(Cells * Cell, Cells * Cell);
        MouseFilter = MouseFilterEnum.Stop;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        TooltipText = Words.Wrap(Loc.T("Bram's echo horn: it keeps everything said across this counter. Click it to hear it all again."));
        _art = new Painter(Paint);
        AddChild(_art);
        MouseEntered += () => Hover(true);
        MouseExited += () => Hover(false);
        GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                AcceptEvent();
                Open();
            }
        };
    }

    /// <summary>
    /// Opens what it kept over the phase it stands in, unless the notebook or the history is open already (it's freed
    /// with the phase; while it's open the counter is busy and nothing behind it hears a key).
    /// </summary>
    public HistoryView Open()
    {
        if (HistoryView.Open > 0 || NotebookView.Open > 0 || GetParent() is not Node phase) return null;
        var view = new HistoryView(_state) { ZIndex = 60 };
        phase.AddChild(view);
        Opened?.Invoke(view);
        return view;
    }

    void Hover(bool on)
    {
        _hover = on;
        _art.QueueRedraw();
    }

    static Rect2 C(int x, int y, int w, int h) => new(x * Cell, y * Cell, w * Cell, h * Cell);

    void Paint(Painter p)
    {
        float lift = _hover ? 1.15f : 1f;
        Color Lit(Color c) => new(Mathf.Min(1, c.R * lift), Mathf.Min(1, c.G * lift), Mathf.Min(1, c.B * lift), c.A);
        // The box: lit top, dark foot, a brass plate.
        p.DrawRect(C(3, 15, 16, 6), Lit(Palette.Wood));
        p.DrawRect(C(3, 15, 16, 1), Lit(Palette.WoodLight));
        p.DrawRect(C(3, 20, 16, 1), Palette.WoodDark);
        p.DrawRect(C(9, 17, 4, 2), Lit(Brass));
        // The neck, up from the box.
        p.DrawRect(C(9, 10, 2, 5), Lit(BrassDark));
        p.DrawRect(C(9, 10, 1, 5), Lit(Brass));
        // The bell, flaring up and to the right from the neck, and its open mouth.
        var bell = new[] { new Vector2(9, 10), new Vector2(13, 1), new Vector2(21, 8), new Vector2(11, 12) }.Select(v => v * Cell).ToArray();
        p.DrawColoredPolygon(bell, Lit(Brass));
        p.DrawColoredPolygon(new[] { new Vector2(9, 10), new Vector2(13, 1), new Vector2(14, 2), new Vector2(10, 11) }.Select(v => v * Cell).ToArray(), Lit(BrassLight));
        var center = new Vector2(17, 4.5f) * Cell;
        var rim = Enumerable.Range(0, 16).Select(i => center + new Vector2(Mathf.Cos(i * Mathf.Tau / 16) * 5.2f, Mathf.Sin(i * Mathf.Tau / 16) * 3.6f).Rotated(0.7f) * Cell).ToArray();
        p.DrawColoredPolygon(rim, Lit(BrassDark));
        var inside = rim.Select(v => center + (v - center) * 0.72f).ToArray();
        p.DrawColoredPolygon(inside, Mouth);
        // A glint on the rim, when it's ready to speak.
        if (_hover) p.DrawRect(new Rect2(rim[12], new Vector2(Cell, Cell)), BrassLight);
    }
}
