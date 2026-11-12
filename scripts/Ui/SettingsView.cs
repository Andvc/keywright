using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// The settings, over whatever screen you're on (it keeps working while the game is paused), on four pages:
/// <list type="bullet">
/// <item>Display: the window's mode and size, scaling, vsync, a frame cap, screen shake and flashes, outlines;</item>
/// <item>Sound: the volumes, muting in the background, and which sound each kind of thing makes;</item>
/// <item>Controls: each action's key and controller button, bound again by clicking it and pressing the new one, and
/// how fast a controller moves the pointer;</item>
/// <item>Game: the language, Uncle Bram's tips, pausing in the background, the folder your files are in, and GM mode
/// (debug builds and --gm only).</item>
/// </list>
/// Changes apply at once and are saved when it closes; Defaults puts the page back as a new player finds it.
/// </summary>
public partial class SettingsView : Control
{
    const float W = 800, H = 600, X = (1280 - W) / 2, Y = (720 - H) / 2, LabelX = X + 40, ControlX = X + 420, ControlW = 320, RowStep = 52;

    public const int DisplayPage = 0, SoundPage = 1, ControlsPage = 2, GamePage = 3;
    static readonly string[] PageNames = { Loc.T("Display"), Loc.T("Sound"), Loc.T("Controls"), Loc.T("Game") };

    public event Action Closed;

    readonly Dictionary<int, Button> _tabs = new();
    Control _page;
    int _open;
    SoundSetsView _sounds;

    /// <summary>A binding being waited for: the action, and whether it's a key (or a controller button).</summary>
    (string Action, bool Key, Button Button)? _binding;
    double _bindingLeft;
    Label _bindNote;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        AddChild(new ColorRect { Color = new Color(0, 0, 0, 0.6f), MouseFilter = MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920));
        AddChild(UiKit.MakePanel(Palette.Gold.Darkened(0.3f)).Place(X, Y, W, H));
        AddChild(UiKit.MakeLabel(Loc.T("SETTINGS"), 32, Palette.Gold, HorizontalAlignment.Center, outline: 4).Place(X, Y + 14, W, 40));
        for (int i = 0; i < PageNames.Length; i++)
        {
            int page = i;
            _tabs[page] = UiKit.MakeButton(PageNames[page], Palette.Text, 16, () => ShowPage(page)).Place(X + 64 + i * 172, Y + 64, 160, 36);
            AddChild(_tabs[page]);
        }
        _page = new Control { MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_page.Place(0, 0, 1280, 720));
        AddChild(UiKit.MakeButton(Loc.T("Defaults"), Palette.Muted, 16, ResetPage).Place(X + 40, Y + H - 64, 160, 44));
        AddChild(UiKit.MakeButton(Loc.T("Done"), Palette.Gold, 24, Close).Place(X + W - 240, Y + H - 68, 200, 48));
        ShowPage(DisplayPage);
        Modulate = Colors.Transparent;
        CreateTween().TweenProperty(this, "modulate", Colors.White, 0.12f);
    }

    public void ShowPage(int page)
    {
        _binding = null;
        _open = page;
        foreach (var child in _page.GetChildren()) child.QueueFree();
        foreach (var (key, tab) in _tabs) tab.Modulate = key == page ? Colors.White : new Color(1, 1, 1, 0.6f);
        switch (page)
        {
            case SoundPage: BuildSound(); break;
            case ControlsPage: BuildControls(); break;
            case GamePage: BuildGame(); break;
            default: BuildDisplay(); break;
        }
    }

    float Row(int index) => Y + 124 + index * RowStep;

    void Label(string text, int row, string tip = null)
    {
        var label = UiKit.MakeLabel(text, 16, Palette.Text).Place(LabelX, Row(row) + 8, 360, 20);
        if (tip != null)
        {
            label.TooltipText = tip;
            label.MouseFilter = MouseFilterEnum.Pass;
        }
        _page.AddChild(label);
    }

    /// <summary>A button that shows a setting's value and moves it on when clicked.</summary>
    void Choice(string text, int row, Func<string> value, Action next, string tip = null, bool enabled = true)
    {
        Label(text, row, tip);
        Button button = null;
        button = UiKit.MakeButton(value(), Palette.PanelBorder.Lightened(0.2f), 16, () =>
        {
            next();
            button.Text = value();
        }).Place(ControlX, Row(row), ControlW, 36);
        button.Disabled = !enabled;
        if (tip != null) button.TooltipText = tip;
        _page.AddChild(button);
    }

    static string OnOff(bool on) => on ? Loc.T("On") : Loc.T("Off");

    /// <summary>A 0-100% slider for one of the volumes (or anything 0 to 1).</summary>
    void Slider(string text, int row, float value, Action<float> set, double min = 0, double max = 100, Func<double, string> show = null)
    {
        Label(text, row);
        var slider = new HSlider { MinValue = min, MaxValue = max, Step = 5, Value = value * 100, FocusMode = FocusModeEnum.None };
        UiKit.StyleSlider(slider);
        var amount = UiKit.MakeLabel("", 16, Palette.Text, HorizontalAlignment.Right).Place(ControlX + ControlW - 72, Row(row) + 8, 72, 20);
        show ??= v => $"{Mathf.RoundToInt(v)}%";
        amount.Text = show(slider.Value);
        slider.ValueChanged += v =>
        {
            set((float)v / 100f);
            amount.Text = show(v);
        };
        _page.AddChild(slider.Place(ControlX, Row(row) + 6, ControlW - 84, 24));
        _page.AddChild(amount);
    }

    void BuildDisplay()
    {
        Choice(Loc.T("Window"), 0, () => Settings.ScreenWord, () =>
        {
            Settings.Screen = (ScreenMode)(((int)Settings.Screen + 1) % 3);
            Settings.ApplyDisplay();
            ShowPage(DisplayPage);
        }, Loc.T("Fullscreen is a borderless window over the whole screen; exclusive fullscreen takes the screen over."));
        var sizes = Settings.FittingSizes();
        Choice(Loc.T("Window size"), 1, () => Settings.Screen == ScreenMode.Windowed ? SizeWord(CurrentSize(sizes)) : Loc.T("(fullscreen)"), () =>
        {
            int at = Array.IndexOf(sizes, CurrentSize(sizes));
            Settings.WindowSize = sizes[(at + 1) % sizes.Length];
            Settings.ApplyDisplay();
        }, enabled: Settings.Screen == ScreenMode.Windowed);
        Choice(Loc.T("Scaling"), 2, () => Settings.PixelPerfect ? Loc.T("Whole pixels only") : Loc.T("Fit the screen"), () =>
        {
            Settings.PixelPerfect = !Settings.PixelPerfect;
            Settings.ApplyDisplay();
        }, Loc.T("Whole pixels only: every art pixel the same size, with bars round the picture when the screen isn't a whole number of times 1280x720."));
        Choice(Loc.T("Vsync"), 3, () => OnOff(Settings.VSync), () =>
        {
            Settings.VSync = !Settings.VSync;
            Settings.ApplyDisplay();
        }, Loc.T("Waits for the screen to be ready for each frame: no tearing."));
        Choice(Loc.T("Frame rate limit"), 4, () => Settings.MaxFps == 0 ? Loc.T("None") : Loc.T($"{Settings.MaxFps} a second"), () =>
        {
            int at = Array.IndexOf(Settings.FpsCaps, Settings.MaxFps);
            Settings.MaxFps = Settings.FpsCaps[(at + 1) % Settings.FpsCaps.Length];
            Settings.ApplyDisplay();
        });
        Choice(Loc.T("Screen shake and flashes"), 5, () => Settings.ShakeWord,
            () => Settings.Shake = Settings.Shake >= 1f ? 0.5f : Settings.Shake > 0f ? 0f : 1f);
        // The art says whether it's drawn to be outlined (0.31's 3px art isn't: its shapes are colour against colour).
        bool outlines = ArtTexture.OutlinesAllowed;
        Choice(Loc.T("Outlines"), 6, () => OnOff(Settings.Outlines && outlines), ToggleOutlines,
            outlines ? Loc.T("A dark outline round everyone and along the counter's edge, so they stand out from the scene.")
                : Loc.T("This art style uses colour shapes without outlines."), enabled: outlines);
    }

    static Vector2I CurrentSize(Vector2I[] sizes)
    {
        var size = Settings.WindowSize.X > 0 ? Settings.WindowSize : sizes.Last();
        return sizes.Contains(size) ? size : sizes.Last();
    }

    static string SizeWord(Vector2I size) => Loc.T($"{size.X} x {size.Y}");

    /// <summary>Outlines on or off, everywhere on screen at once (the bot's shell demo uses this too).</summary>
    public void ToggleOutlines()
    {
        if (!ArtTexture.OutlinesAllowed) return;
        Settings.Outlines = !Settings.Outlines;
        UiKit.ShowOutlines(GetTree(), Settings.Outlines);
    }

    void BuildSound()
    {
        Slider(Loc.T("Volume"), 0, Settings.Volume, v =>
        {
            Settings.Volume = v;
            Settings.ApplyAudio();
        });
        Slider(Loc.T("Music"), 1, Settings.Music, v =>
        {
            Settings.Music = v;
            Settings.ApplyAudio();
        });
        Slider(Loc.T("Sound effects"), 2, Settings.Effects, v =>
        {
            Settings.Effects = v;
            Settings.ApplyAudio();
        });
        Slider(Loc.T("Heartsongs' songs"), 3, Settings.Songs, v =>
        {
            Settings.Songs = v;
            Settings.ApplyAudio();
        });
        Choice(Loc.T("Mute in the background"), 4, () => OnOff(Settings.MuteInBackground), () => Settings.MuteInBackground = !Settings.MuteInBackground,
            Loc.T("Silence while another window has the focus."));
        Label(Loc.T("Item sounds"), 5, Loc.T("Which sound each kind of thing makes: every one is the sound you'd expect."));
        _page.AddChild(UiKit.MakeButton(Loc.T("Choose..."), Palette.PanelBorder.Lightened(0.2f), 16, OpenSounds).Place(ControlX, Row(5), ControlW, 36));
    }

    void BuildControls()
    {
        const float keyX = ControlX - 60, padX = ControlX + 140, cellW = 180;
        _page.AddChild(UiKit.MakeLabel(Loc.T("Keyboard"), 16, Palette.Muted, HorizontalAlignment.Center).Place(keyX, Row(0) - 16, cellW, 20));
        _page.AddChild(UiKit.MakeLabel(Loc.T("Controller"), 16, Palette.Muted, HorizontalAlignment.Center).Place(padX, Row(0) - 16, cellW, 20));
        for (int i = 0; i < Controls.Bindable.Length; i++)
        {
            var (action, name) = Controls.Bindable[i];
            float y = Row(0) + 4 + i * 44;
            _page.AddChild(UiKit.MakeLabel(name, 16, Palette.Text).Place(LabelX, y + 8, 300, 20));
            Button key = null, pad = null;
            key = UiKit.MakeButton(Names(Controls.Keys(action).Select(Controls.KeyName)), Palette.PanelBorder.Lightened(0.2f), 16,
                () => StartBinding(action, true, key)).Place(keyX, y, cellW, 36);
            pad = UiKit.MakeButton(Names(Controls.Buttons(action).Select(Controls.ButtonName)), Palette.PanelBorder.Lightened(0.2f), 16,
                () => StartBinding(action, false, pad)).Place(padX, y, cellW, 36);
            _page.AddChild(key);
            _page.AddChild(pad);
        }
        _bindNote = UiKit.MakeLabel(Loc.T("Click one, then press the key or button it should be."), 16, Palette.Muted).Place(LabelX, Row(0) + 4 + 5 * 44, 720, 20);
        _page.AddChild(_bindNote);
        Slider(Loc.T("Controller pointer speed"), 5, Settings.CursorSpeed / 2f, v => Settings.CursorSpeed = Mathf.Clamp(v * 2f, 0.5f, 2f),
            25, 100, v => $"{v / 50:0.0}x");
        _page.AddChild(UiKit.MakeLabel(Loc.T("Mouse: click and drag everything. Controller: the left stick moves the pointer, A clicks and drags " +
            "(grab a chest and shake it with the stick), the D-pad nudges and the right stick scrolls."), 16, Palette.Muted)
            .Wrapped().Place(LabelX, Row(6) + 4, 720, 72)); // three lines in a longer language, above the buttons
    }

    static string Names(IEnumerable<string> names) => names.Any() ? Words.List(names) : Loc.T("(none)");

    void StartBinding(string action, bool key, Button button)
    {
        _binding = (action, key, button);
        _bindingLeft = 6;
        button.Text = key ? Loc.T("Press a key...") : Loc.T("Press a button...");
        _bindNote.Text = key ? Loc.T("Press the key (Esc to keep the old one).") : Loc.T("Press the button (A and the D-pad are the pointer's).");
    }

    public override void _Process(double delta)
    {
        if (_binding == null) return;
        _bindingLeft -= delta;
        if (_bindingLeft <= 0) ShowPage(ControlsPage);
    }

    public override void _Input(InputEvent e)
    {
        if (_binding is not var (action, key, _)) return;
        if (key && e is InputEventKey { Pressed: true, Echo: false } press)
        {
            GetViewport().SetInputAsHandled();
            string moved = press.Keycode != Key.Escape ? Controls.Bind(action, press.Keycode) : null;
            ShowPage(ControlsPage);
            if (moved != null)
                _bindNote.Text = Loc.T($"{Controls.KeyName(press.Keycode)} was for \"{Controls.Name(moved)}\": that has {Names(Controls.Keys(moved).Select(Controls.KeyName))} now.");
        }
        else if (!key && e is InputEventJoypadButton { Pressed: true } button)
        {
            GetViewport().SetInputAsHandled();
            if (button.ButtonIndex is JoyButton.A or JoyButton.DpadUp or JoyButton.DpadDown or JoyButton.DpadLeft or JoyButton.DpadRight)
            {
                _bindNote.Text = Loc.T("A and the D-pad move and click the pointer: pick another button.");
                return;
            }
            string moved = Controls.Bind(action, button.ButtonIndex);
            ShowPage(ControlsPage);
            if (moved != null)
                _bindNote.Text = Loc.T($"{Controls.ButtonName(button.ButtonIndex)} was for \"{Controls.Name(moved)}\": that has {Names(Controls.Buttons(moved).Select(Controls.ButtonName))} now.");
        }
    }

    void BuildGame()
    {
        var languages = Localization.Languages.ToArray();
        Label restart = null;
        // "System" in the player's language; every other language by its own name.
        Choice(Loc.T("Language"), 0, () => Settings.Language is { Length: > 0 } code ? Localization.Name(code) : Loc.T("System"), () =>
        {
            int at = Array.FindIndex(languages, l => l.Code == Settings.Language);
            Settings.Language = languages[(at + 1) % languages.Length].Code;
            restart.Visible = Localization.Chosen != Localization.Current;
        }, Loc.T("The language the game is in (System: the computer's own, when the game has it)."));
        // Everything's in the language the game started in: a new one waits for the next start.
        restart = UiKit.MakeLabel(Loc.T($"In {Localization.Name(Localization.Chosen)} from the next time the game starts."), 16, Palette.Warn)
            .Place(LabelX, Row(5) + 8, 720, 20);
        restart.Visible = Localization.Chosen != Localization.Current;
        _page.AddChild(restart);
        Choice(Loc.T("Bram's tips"), 1, () => OnOff(Settings.Tips), () =>
        {
            Settings.Tips = !Settings.Tips;
            if (Main.Instance.State != null) Main.Instance.State.Guided = Settings.Tips;
        });
        Choice(Loc.T("Pause in the background"), 2, () => OnOff(Settings.PauseInBackground), () => Settings.PauseInBackground = !Settings.PauseInBackground,
            Loc.T("Opens the menu when another window takes the focus, so a waiting seller's patience doesn't run out."));
        Label(Loc.T("Your saves and settings"), 3, OS.GetUserDataDir());
        _page.AddChild(UiKit.MakeButton(Loc.T("Open the folder"), Palette.PanelBorder.Lightened(0.2f), 16, () => OS.ShellOpen(OS.GetUserDataDir()))
            .Place(ControlX, Row(3), ControlW, 36));
        if (Settings.GmOffered)
            Choice(Loc.T("GM mode (for testing)"), 4, () => OnOff(Settings.Gm), () => Settings.Gm = !Settings.Gm,
                Loc.T("Puts gold, a skipped day and the whole journal in the pause menu."));
    }

    /// <summary>The page that's open, back as a new player finds it.</summary>
    void ResetPage()
    {
        switch (_open)
        {
            case DisplayPage:
                Settings.Screen = ScreenMode.Fullscreen;
                Settings.WindowSize = default;
                Settings.PixelPerfect = false;
                Settings.VSync = true;
                Settings.MaxFps = 0;
                Settings.Shake = 1f;
                if (Settings.Outlines) ToggleOutlines();
                Settings.ApplyDisplay();
                break;
            case SoundPage:
                Settings.Volume = 0.8f;
                Settings.Effects = 1f;
                Settings.Songs = 1f;
                Settings.Music = 0.6f;
                Settings.MuteInBackground = false;
                foreach (var kind in Enum.GetValues<Core.Category>()) Settings.SetSoundSet(kind, 0);
                Settings.ApplyAudio();
                break;
            case ControlsPage:
                Controls.Reset();
                Settings.CursorSpeed = 1f;
                break;
            case GamePage:
                Settings.Language = "";
                Settings.Tips = true;
                Settings.PauseInBackground = true;
                break;
        }
        ShowPage(_open);
    }

    /// <summary>Opens the item sounds chooser on top (the bot's shell demo uses this too).</summary>
    public void OpenSounds()
    {
        if (_sounds != null) return;
        _sounds = new SoundSetsView { ZIndex = 5 };
        _sounds.Closed += () => _sounds = null;
        AddChild(_sounds);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed(Controls.Back) && _sounds == null && _binding == null)
        {
            GetViewport().SetInputAsHandled();
            Close();
        }
    }

    public void Close()
    {
        Settings.Save();
        Closed?.Invoke();
        QueueFree();
    }
}
