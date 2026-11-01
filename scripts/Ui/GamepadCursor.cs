using System.Collections.Generic;
using Godot;

namespace Keywright.Ui;

/// <summary>
/// A controller plays the game the way the mouse does. The left stick moves the pointer (the D-pad nudges it), and A is
/// the left button: click, or hold and drag. Grab the chest and flick the stick to shake it; the pointer moves faster
/// while A is held, so a hard shake is a flick away. The right stick scrolls. The other buttons are actions
/// (<see cref="Controls"/>). The pointer is the system's own, moved where the stick says, and touching the mouse hands
/// it back.
/// </summary>
public partial class GamepadCursor : Node
{
    /// <summary>Pointer speed at full tilt (screen pixels a second, times <see cref="Settings.CursorSpeed"/>): browsing,
    /// with A held (a chest's hard shake starts at 1,150), and on the D-pad.</summary>
    const float Speed = 900f, DragSpeed = 2600f, PadSpeed = 280f, DeadZone = 0.2f;

    /// <summary>How often a held right stick scrolls, in seconds.</summary>
    const double ScrollEvery = 0.1;

    Vector2 _stick, _scroll, _pos;
    readonly HashSet<JoyButton> _dpad = new();
    bool _down;
    double _scrollWait;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _pos = GetViewport().GetMousePosition();
    }

    public override void _Input(InputEvent e)
    {
        switch (e)
        {
            case InputEventJoypadMotion motion:
                if (Mathf.Abs(motion.AxisValue) > DeadZone) Controls.UsingPad = true;
                switch (motion.Axis)
                {
                    case JoyAxis.LeftX: _stick.X = motion.AxisValue; break;
                    case JoyAxis.LeftY: _stick.Y = motion.AxisValue; break;
                    case JoyAxis.RightX: _scroll.X = motion.AxisValue; break;
                    case JoyAxis.RightY: _scroll.Y = motion.AxisValue; break;
                }
                break;
            case InputEventJoypadButton button:
                Controls.UsingPad = true;
                if (button.ButtonIndex == JoyButton.A)
                {
                    Press(button.Pressed);
                    GetViewport().SetInputAsHandled();
                }
                else if (button.ButtonIndex is JoyButton.DpadUp or JoyButton.DpadDown or JoyButton.DpadLeft or JoyButton.DpadRight)
                {
                    if (button.Pressed) _dpad.Add(button.ButtonIndex);
                    else _dpad.Remove(button.ButtonIndex);
                    GetViewport().SetInputAsHandled();
                }
                break;
            case InputEventMouseMotion mouse when mouse.Position.DistanceTo(_pos) > 2f:
                // The mouse itself moved: it has the pointer back.
                Controls.UsingPad = false;
                _pos = mouse.Position;
                break;
            case InputEventKey { Pressed: true }:
                Controls.UsingPad = false;
                break;
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        var move = Shaped(_stick);
        var pad = new Vector2((_dpad.Contains(JoyButton.DpadRight) ? 1 : 0) - (_dpad.Contains(JoyButton.DpadLeft) ? 1 : 0),
            (_dpad.Contains(JoyButton.DpadDown) ? 1 : 0) - (_dpad.Contains(JoyButton.DpadUp) ? 1 : 0));
        var step = move * (_down ? DragSpeed : Speed) * Settings.CursorSpeed + pad * PadSpeed;
        if (step != Vector2.Zero)
        {
            _pos = (_pos + step * dt).Clamp(Vector2.Zero, new Vector2(1279, 719));
            GetViewport().WarpMouse(_pos);
            // Sliders and drags listen for movement with the button held: tell them, as a mouse would.
            if (_down)
                Input.ParseInputEvent(new InputEventMouseMotion
                {
                    Position = ToWindow(_pos), GlobalPosition = ToWindow(_pos), ButtonMask = MouseButtonMask.Left,
                    Relative = step * dt,
                });
        }
        _scrollWait -= delta;
        if (Mathf.Abs(_scroll.Y) > 0.5f && _scrollWait <= 0)
        {
            _scrollWait = ScrollEvery;
            var wheel = _scroll.Y < 0 ? MouseButton.WheelUp : MouseButton.WheelDown;
            foreach (bool pressed in new[] { true, false })
                Input.ParseInputEvent(new InputEventMouseButton
                {
                    ButtonIndex = wheel, Pressed = pressed, Position = ToWindow(_pos), GlobalPosition = ToWindow(_pos), Factor = 1,
                });
        }
    }

    /// <summary>The left button, at the pointer.</summary>
    void Press(bool pressed)
    {
        _down = pressed;
        _pos = GetViewport().GetMousePosition();
        Input.ParseInputEvent(new InputEventMouseButton
        {
            ButtonIndex = MouseButton.Left, Pressed = pressed, ButtonMask = pressed ? MouseButtonMask.Left : 0,
            Position = ToWindow(_pos), GlobalPosition = ToWindow(_pos),
        });
    }

    /// <summary>A stick's tilt, past the dead zone, eased so small tilts are fine and full tilt is fast.</summary>
    static Vector2 Shaped(Vector2 stick)
    {
        float length = stick.Length();
        if (length < DeadZone) return Vector2.Zero;
        float t = Mathf.Min(1f, (length - DeadZone) / (1f - DeadZone));
        return stick / length * t * t;
    }

    /// <summary>A point on the 1280x720 screen in the window's own pixels (events arrive the way the system sends them).</summary>
    Vector2 ToWindow(Vector2 point) => GetViewport().GetScreenTransform() * point;
}
