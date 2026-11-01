using System;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright;

/// <summary>How the game's window sits on the screen.</summary>
public enum ScreenMode { Windowed, Fullscreen, Exclusive }

/// <summary>
/// The player's settings, kept in user://settings.cfg (through <see cref="Storage"/>):
/// <list type="bullet">
/// <item>display: windowed, fullscreen (a borderless window over the whole screen) or exclusive fullscreen; the window's
/// size; scaling to fit the screen or by whole numbers only (every art pixel the same size); vsync and a frame cap; how
/// much the screen shakes and flashes; the outlines round people and the counter;</item>
/// <item>sound: the master volume, sound effects and the Heartsongs' songs on buses of their own, and muting in the
/// background;</item>
/// <item>the game: its language, Uncle Bram's tips (for new games, and the one in progress), pausing in the background;
/// the controller cursor's speed; which sound each kind of thing makes; and GM mode (for testing: debug builds and --gm
/// only);</item>
/// <item>the keys and buttons, kept by <see cref="Controls"/> in the same file.</item>
/// </list>
/// A value of the wrong type in the file falls back to its default rather than stopping the game.
/// </summary>
public static class Settings
{
    const string Path = "user://settings.cfg";

    public static ScreenMode Screen { get; set; } = ScreenMode.Fullscreen;

    /// <summary>The window's size when windowed (0x0 until chosen: the largest of <see cref="WindowSizes"/> that fits).</summary>
    public static Vector2I WindowSize { get; set; }

    /// <summary>Scale the 1280x720 screen by whole numbers only, so every art pixel is the same size (bars at the sides).</summary>
    public static bool PixelPerfect { get; set; }
    public static bool VSync { get; set; } = true;

    /// <summary>Frames a second at most (0: no limit).</summary>
    public static int MaxFps { get; set; }

    /// <summary>How strong screen shake and flashes are: 1 full, 0.5 reduced, 0 off.</summary>
    public static float Shake { get; set; } = 1f;

    /// <summary>The outline round everyone and along the counter's edge (<see cref="Ui.UiKit.Outline"/>).</summary>
    public static bool Outlines { get; set; } = true;

    /// <summary>0 to 1: everything, the sound effects, and the Heartsongs' songs.</summary>
    public static float Volume { get; set; } = 0.8f;
    public static float Effects { get; set; } = 1f;
    public static float Songs { get; set; } = 1f;
    public static bool MuteInBackground { get; set; }

    /// <summary>The language, as a locale ("en", "zh_CN"...), or empty to follow the system's.</summary>
    public static string Language { get; set; } = "";
    public static bool Tips { get; set; } = true;

    /// <summary>Open the menu when the game's window loses focus (a seller's patience stops with it).</summary>
    public static bool PauseInBackground { get; set; } = true;

    /// <summary>How fast a controller's stick moves the cursor: 0.5 to 2.</summary>
    public static float CursorSpeed { get; set; } = 1f;

    /// <summary>GM mode, for testing: the pause menu adds 99,999 gold, skips the day, or fills the journal (--gm turns it on too).</summary>
    public static bool Gm { get; set; }

    /// <summary>GM mode can be seen in Settings: in a debug build, or once --gm has turned it on.</summary>
    public static bool GmOffered => OS.IsDebugBuild() || Gm;

    /// <summary>The frame caps on offer.</summary>
    public static readonly int[] FpsCaps = { 0, 30, 60, 120, 144, 240 };

    /// <summary>The window sizes on offer, each 16:9 (the ones that fit the screen are shown).</summary>
    public static readonly Vector2I[] WindowSizes =
        { new(1280, 720), new(1600, 900), new(1920, 1080), new(2560, 1440), new(3200, 1800), new(3840, 2160) };

    /// <summary>For each kind of thing, which of its sounds to use (an index into <see cref="Audio.SoundSets.For"/>).</summary>
    static readonly int[] _soundSets = new int[Enum.GetValues<Core.Category>().Length];

    public static int SoundSet(Core.Category kind) => _soundSets[(int)kind];

    public static void SetSoundSet(Core.Category kind, int option) => _soundSets[(int)kind] = option;

    public static void Load()
    {
        var file = Storage.Read(Path, text =>
        {
            var config = new ConfigFile();
            return config.Parse(text) == Error.Ok ? config : null;
        });
        if (file != null)
        {
            // Before 0.27 there was only fullscreen, on or off.
            if (file.HasSectionKey("video", "fullscreen") && !file.HasSectionKey("video", "screen"))
                Screen = Get(file, "video", "fullscreen", false) ? ScreenMode.Fullscreen : ScreenMode.Windowed;
            Screen = (ScreenMode)Math.Clamp(Get(file, "video", "screen", (int)Screen), 0, 2);
            WindowSize = new Vector2I(Get(file, "video", "window_width", 0), Get(file, "video", "window_height", 0));
            PixelPerfect = Get(file, "video", "pixel_perfect", PixelPerfect);
            VSync = Get(file, "video", "vsync", VSync);
            MaxFps = Math.Max(0, Get(file, "video", "max_fps", MaxFps));
            Shake = Math.Clamp(Get(file, "video", "shake", Shake), 0f, 1f);
            Outlines = Get(file, "video", "outlines", Outlines);
            Volume = Math.Clamp(Get(file, "audio", "volume", Volume), 0f, 1f);
            Effects = Math.Clamp(Get(file, "audio", "effects", Effects), 0f, 1f);
            Songs = Math.Clamp(Get(file, "audio", "songs", Songs), 0f, 1f);
            MuteInBackground = Get(file, "audio", "mute_in_background", MuteInBackground);
            Language = Get(file, "game", "language", Language);
            Tips = Get(file, "game", "tips", Tips);
            PauseInBackground = Get(file, "game", "pause_in_background", PauseInBackground);
            CursorSpeed = Math.Clamp(Get(file, "controls", "cursor_speed", CursorSpeed), 0.5f, 2f);
            Gm = Gm || Get(file, "game", "gm", Gm);
            foreach (var kind in Enum.GetValues<Core.Category>())
                _soundSets[(int)kind] = Math.Max(0, Get(file, "sounds", kind.ToString().ToLowerInvariant(), 0));
        }
        Controls.Load(file);
        Apply();
    }

    public static void Save()
    {
        var file = new ConfigFile();
        file.SetValue("video", "screen", (int)Screen);
        file.SetValue("video", "window_width", WindowSize.X);
        file.SetValue("video", "window_height", WindowSize.Y);
        file.SetValue("video", "pixel_perfect", PixelPerfect);
        file.SetValue("video", "vsync", VSync);
        file.SetValue("video", "max_fps", MaxFps);
        file.SetValue("video", "shake", Shake);
        file.SetValue("video", "outlines", Outlines);
        file.SetValue("audio", "volume", Volume);
        file.SetValue("audio", "effects", Effects);
        file.SetValue("audio", "songs", Songs);
        file.SetValue("audio", "mute_in_background", MuteInBackground);
        file.SetValue("game", "language", Language);
        file.SetValue("game", "tips", Tips);
        file.SetValue("game", "pause_in_background", PauseInBackground);
        file.SetValue("game", "gm", Gm);
        file.SetValue("controls", "cursor_speed", CursorSpeed);
        foreach (var kind in Enum.GetValues<Core.Category>())
            file.SetValue("sounds", kind.ToString().ToLowerInvariant(), _soundSets[(int)kind]);
        Controls.Save(file);
        Storage.Write(Path, file.EncodeToText());
    }

    /// <summary>The display, sound and language settings, as they are now.</summary>
    public static void Apply()
    {
        ApplyAudio();
        ApplyDisplay();
    }

    /// <summary>The buses the sounds play on (Effects and Songs, both into Master), at their volumes.</summary>
    public static void ApplyAudio(bool muted = false)
    {
        foreach (string bus in new[] { EffectsBus, SongsBus })
        {
            if (AudioServer.GetBusIndex(bus) >= 0) continue;
            AudioServer.AddBus();
            int index = AudioServer.BusCount - 1;
            AudioServer.SetBusName(index, bus);
            AudioServer.SetBusSend(index, "Master");
        }
        SetBus("Master", muted ? 0 : Volume);
        SetBus(EffectsBus, Effects);
        SetBus(SongsBus, Songs);
    }

    public const string EffectsBus = "Effects", SongsBus = "Songs";

    static void SetBus(string name, float volume)
    {
        int index = AudioServer.GetBusIndex(name);
        AudioServer.SetBusVolumeDb(index, Mathf.LinearToDb(Mathf.Max(0.0001f, volume)));
        AudioServer.SetBusMute(index, volume <= 0.001f);
    }

    /// <summary>The window: its mode, size (centred on its screen), scaling, vsync and frame cap. Headless runs (the sim,
    /// the tests) have no window to change. It goes through the root <see cref="Window"/>, never the display server
    /// alone: the window node works out the stretch and the mouse from its own size, and it only learns a new size
    /// when it's the one that set it.</summary>
    public static void ApplyDisplay()
    {
        Engine.MaxFps = MaxFps;
        if (DisplayServer.GetName() == "headless" || (Engine.GetMainLoop() as SceneTree)?.Root is not Window root) return;
        root.ContentScaleStretch = PixelPerfect ? Window.ContentScaleStretchEnum.Integer : Window.ContentScaleStretchEnum.Fractional;
        var vsync = VSync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled;
        if (DisplayServer.WindowGetVsyncMode() != vsync) DisplayServer.WindowSetVsyncMode(vsync);
        var mode = Screen switch
        {
            ScreenMode.Exclusive => Window.ModeEnum.ExclusiveFullscreen,
            ScreenMode.Fullscreen => Window.ModeEnum.Fullscreen,
            _ => Window.ModeEnum.Windowed,
        };
        if (root.Mode != mode) root.Mode = mode;
        if (Screen != ScreenMode.Windowed) return;
        var size = WindowSize.X > 0 ? WindowSize : FittingSizes().Last();
        var area = DisplayServer.ScreenGetUsableRect(root.CurrentScreen);
        size = new Vector2I(Math.Min(size.X, area.Size.X), Math.Min(size.Y, area.Size.Y));
        root.Size = size;
        root.Position = area.Position + (area.Size - size) / 2;
    }

    /// <summary>The window sizes that fit the screen the game is on (always the smallest).</summary>
    public static Vector2I[] FittingSizes()
    {
        var area = DisplayServer.GetName() == "headless" ? new Vector2I(1920, 1080)
            : DisplayServer.ScreenGetUsableRect((Engine.GetMainLoop() as SceneTree)?.Root.CurrentScreen ?? 0).Size;
        var fits = WindowSizes.Where(s => s.X <= area.X && s.Y <= area.Y).ToArray();
        return fits.Length > 0 ? fits : new[] { WindowSizes[0] };
    }

    /// <summary>Everything back to how a new player finds it (the keys and buttons too).</summary>
    public static void Reset()
    {
        Screen = ScreenMode.Fullscreen;
        WindowSize = default;
        PixelPerfect = false;
        VSync = true;
        MaxFps = 0;
        Shake = 1f;
        Outlines = true;
        Volume = 0.8f;
        Effects = 1f;
        Songs = 1f;
        MuteInBackground = false;
        Tips = true;
        PauseInBackground = true;
        CursorSpeed = 1f;
        Array.Clear(_soundSets);
        Controls.Reset();
        Apply();
    }

    public static string ShakeWord => Shake >= 1f ? Loc.T("Full") : Shake > 0f ? Loc.T("Reduced") : Loc.T("Off");

    public static string ScreenWord => Screen switch
    {
        ScreenMode.Exclusive => Loc.T("Exclusive fullscreen"),
        ScreenMode.Fullscreen => Loc.T("Fullscreen"),
        _ => Loc.T("Windowed"),
    };

    static T Get<T>(ConfigFile file, string section, string key, T fallback)
    {
        if (!file.HasSectionKey(section, key)) return fallback;
        try
        {
            var value = file.GetValue(section, key);
            // Numbers may have been written as either kind.
            if (typeof(T) == typeof(float) && value.VariantType is Variant.Type.Int or Variant.Type.Float) return (T)(object)(float)value.AsDouble();
            if (typeof(T) == typeof(int) && value.VariantType is Variant.Type.Int or Variant.Type.Float) return (T)(object)(int)value.AsInt64();
            if (typeof(T) == typeof(bool) && value.VariantType == Variant.Type.Bool) return (T)(object)value.AsBool();
            if (typeof(T) == typeof(string) && value.VariantType == Variant.Type.String) return (T)(object)value.AsString();
        }
        catch (Exception e)
        {
            GD.PushWarning($"settings.cfg: [{section}] {key} couldn't be read ({e.Message})");
        }
        return fallback;
    }
}
