using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Keywright;

/// <summary>
/// What the keys and a controller's buttons do. The game asks for an action, never a key: each action has keyboard keys
/// and controller buttons, a player can bind both again (Settings > Controls), and their choice is kept in settings.cfg.
/// The mouse needs no binding: it clicks and drags everything, and a controller moves and clicks a cursor in its place
/// (<see cref="Ui.GamepadCursor"/>: the left stick and A), so everything the mouse can do, a controller can.
/// </summary>
public static class Controls
{
    /// <summary>Set a pin, hurry an opening, the next line of a scene (Space).</summary>
    public const string Act = "act";
    /// <summary>Force a lock with the crowbar (F).</summary>
    public const string Force = "force";
    public const string Notebook = "notebook";
    /// <summary>The pause menu (Esc).</summary>
    public const string Pause = "pause";
    /// <summary>Close whatever's open on top (Esc).</summary>
    public const string Back = "back";

    /// <summary>The actions a player can bind, in the order the Controls page lists them, and what it calls each.</summary>
    public static readonly (string Action, string Name)[] Bindable =
    {
        (Act, "Set a pin, hurry, next line"),
        (Force, "Force a lock"),
        (Notebook, "Notebook"),
        (Pause, "Menu"),
        (Back, "Back, close"),
    };

    static readonly Dictionary<string, (Key[] Keys, JoyButton[] Buttons)> Defaults = new()
    {
        [Act] = (new[] { Key.Space, Key.Enter }, new[] { JoyButton.X }),
        [Force] = (new[] { Key.F }, new[] { JoyButton.RightShoulder }),
        [Notebook] = (new[] { Key.N, Key.L }, new[] { JoyButton.Y }),
        [Pause] = (new[] { Key.Escape }, new[] { JoyButton.Start }),
        [Back] = (new[] { Key.Escape }, new[] { JoyButton.B }),
    };

    static readonly Dictionary<string, List<Key>> _keys = new();
    static readonly Dictionary<string, List<JoyButton>> _buttons = new();

    /// <summary>The last thing pressed or moved was a controller (prompts show its buttons, not keys).</summary>
    public static bool UsingPad { get; set; }

    /// <summary>The bindings in the settings file (or the defaults), into Godot's input map.</summary>
    public static void Load(ConfigFile file)
    {
        foreach (var (action, _) in Bindable)
        {
            var (keys, buttons) = Defaults[action];
            _keys[action] = Read(file, $"{action}_keys", keys.Select(k => (int)k)).Select(k => (Key)k).ToList();
            _buttons[action] = Read(file, $"{action}_buttons", buttons.Select(b => (int)b)).Select(b => (JoyButton)b).ToList();
        }
        Apply();
    }

    public static void Save(ConfigFile file)
    {
        foreach (var (action, _) in Bindable)
        {
            file.SetValue("controls", $"{action}_keys", _keys[action].Select(k => (int)k).ToArray());
            file.SetValue("controls", $"{action}_buttons", _buttons[action].Select(b => (int)b).ToArray());
        }
    }

    public static void Reset()
    {
        foreach (var (action, _) in Bindable)
        {
            _keys[action] = Defaults[action].Keys.ToList();
            _buttons[action] = Defaults[action].Buttons.ToList();
        }
        Apply();
    }

    public static IReadOnlyList<Key> Keys(string action) => _keys[action];
    public static IReadOnlyList<JoyButton> Buttons(string action) => _buttons[action];

    /// <summary>
    /// Binds the action to this key alone. Another action that had it gives it up (the menu and Back may share one, as
    /// Esc does), and one left with nothing takes this action's old key, so the two swap: no key does two things and
    /// no action is left without one. Says which action it moved, if any, or null.
    /// </summary>
    public static string Bind(string action, Key key) => Bind(_keys, action, key);

    public static string Bind(string action, JoyButton button) => Bind(_buttons, action, button);

    static string Bind<T>(Dictionary<string, List<T>> map, string action, T input) where T : struct, Enum
    {
        var old = map[action].ToList();
        map[action] = new List<T> { input };
        string moved = null;
        foreach (var (other, _) in Bindable)
        {
            if (other == action || Shares(action, other) || !map[other].Remove(input)) continue;
            moved = other;
            // Left with nothing, it takes this action's old one: the two swap.
            if (map[other].Count == 0)
                foreach (var spare in old)
                    if (!spare.Equals(input) && !map.Values.Any(inputs => inputs.Contains(spare)))
                    {
                        map[other].Add(spare);
                        break;
                    }
        }
        Apply();
        return moved;
    }

    /// <summary>The menu and Back may share a key or a button (Esc, by default): one opens it, the other closes things.</summary>
    static bool Shares(string a, string b) => (a, b) is (Pause, Back) or (Back, Pause);

    public static string Name(string action) => Bindable.FirstOrDefault(b => b.Action == action).Name ?? action;

    static void Apply()
    {
        foreach (var (action, _) in Bindable)
        {
            if (!InputMap.HasAction(action)) InputMap.AddAction(action);
            InputMap.ActionEraseEvents(action);
            foreach (var key in _keys[action]) InputMap.ActionAddEvent(action, new InputEventKey { Keycode = key });
            foreach (var button in _buttons[action]) InputMap.ActionAddEvent(action, new InputEventJoypadButton { ButtonIndex = button, Device = -1 });
        }
    }

    static IEnumerable<int> Read(ConfigFile file, string key, IEnumerable<int> fallback)
    {
        if (file == null || !file.HasSectionKey("controls", key)) return fallback;
        try
        {
            var value = file.GetValue("controls", key);
            if (value.VariantType == Variant.Type.PackedInt32Array) return value.AsInt32Array();
            if (value.VariantType == Variant.Type.Array) return value.AsGodotArray().Select(v => v.AsInt32());
        }
        catch (Exception e)
        {
            GD.PushWarning($"settings.cfg: [controls] {key} couldn't be read ({e.Message})");
        }
        return fallback;
    }

    /// <summary>What a key is called on its cap ("Space", "N", "Esc").</summary>
    public static string KeyName(Key key) => key switch
    {
        Key.Escape => "Esc",
        Key.Enter => "Enter",
        Key.Space => "Space",
        _ => OS.GetKeycodeString(key),
    };

    /// <summary>What a controller's button is called, in the controller's own words where they differ (PlayStation's
    /// shapes, Nintendo's swapped letters).</summary>
    public static string ButtonName(JoyButton button)
    {
        string pad = Input.GetConnectedJoypads().Count > 0 ? Input.GetJoyName(Input.GetConnectedJoypads()[0]).ToLowerInvariant() : "";
        bool playstation = pad.Contains("ps") || pad.Contains("dualshock") || pad.Contains("dualsense") || pad.Contains("playstation");
        bool nintendo = pad.Contains("nintendo") || pad.Contains("switch") || pad.Contains("joy-con");
        return button switch
        {
            JoyButton.A => playstation ? "Cross" : nintendo ? "B" : "A",
            JoyButton.B => playstation ? "Circle" : nintendo ? "A" : "B",
            JoyButton.X => playstation ? "Square" : nintendo ? "Y" : "X",
            JoyButton.Y => playstation ? "Triangle" : nintendo ? "X" : "Y",
            JoyButton.LeftShoulder => playstation ? "L1" : "LB",
            JoyButton.RightShoulder => playstation ? "R1" : "RB",
            JoyButton.LeftStick => playstation ? "L3" : "LS",
            JoyButton.RightStick => playstation ? "R3" : "RS",
            JoyButton.Back => playstation ? "Share" : "Back",
            JoyButton.Start => playstation ? "Options" : "Start",
            JoyButton.Guide => "Home",
            JoyButton.DpadUp => "D-pad up",
            JoyButton.DpadDown => "D-pad down",
            JoyButton.DpadLeft => "D-pad left",
            JoyButton.DpadRight => "D-pad right",
            _ => $"Button {(int)button}",
        };
    }

    /// <summary>
    /// What to press for an action, for a prompt: its first key, or its first button when a controller is in use
    /// ("N", "Y"); empty if it has neither.
    /// </summary>
    public static string Hint(string action)
    {
        if (UsingPad && _buttons.TryGetValue(action, out var buttons) && buttons.Count > 0) return ButtonName(buttons[0]);
        return _keys.TryGetValue(action, out var keys) && keys.Count > 0 ? KeyName(keys[0]) : "";
    }

    /// <summary>A text with "{act}", "{force}", "{notebook}", "{pause}" or "{back}" in it, with what to press for each.</summary>
    public static string Fill(string text)
    {
        if (text == null || !text.Contains('{')) return text;
        foreach (var (action, _) in Bindable) text = text.Replace("{" + action + "}", Hint(action));
        return text;
    }

    /// <summary>"[N]" for a button's label, or nothing if the action has no binding for the device in use.</summary>
    public static string Tag(string action) => Hint(action) is { Length: > 0 } hint ? $"[{hint}]" : "";
}
