using System;
using System.Linq;
using System.Text;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// Who made what, from the title screen: the game's own credits (CREDITS.md says the same at more length), then the
/// notices its parts ask to travel with it: Godot Engine's licence and those of the libraries built into it, Galmuri's
/// font licence and Steamworks.NET's.
/// </summary>
public partial class CreditsView : Node2D
{
    /// <summary>Who's making Keywright, for its first line ("A game by ..."): empty until the owner says.</summary>
    public const string Studio = "";

    const float PanelX = 140, PanelY = 60, PanelW = 1000, PanelH = 600;

    bool _closing;
    public event Action Closed;

    static Audio.Sfx Sfx => Main.Instance.Sfx;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        ZIndex = 40;
        var blocker = new ColorRect { Color = new Color(0, 0, 0, 0.75f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920);
        AddChild(blocker);
        AddChild(UiKit.MakePanel(Palette.Gold.Darkened(0.3f)).Place(PanelX, PanelY, PanelW, PanelH));
        AddChild(UiKit.MakeLabel(Loc.T("CREDITS"), 32, Palette.Gold, HorizontalAlignment.Center, outline: 4).Place(PanelX, PanelY + 12, PanelW, 40));
        AddChild(UiKit.MakeButton(Loc.T("Close"), Palette.Text, 16, Close).Place(PanelX + PanelW - 140, PanelY + 16, 116, 36));

        var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }.Place(PanelX + 32, PanelY + 68, PanelW - 64, PanelH - 92);
        AddChild(scroll);
        var column = new VBoxContainer { CustomMinimumSize = new Vector2(PanelW - 84, 0) };
        column.AddThemeConstantOverride("separation", 6);
        scroll.AddChild(column);

        void Add(Label label, float height)
        {
            label.CustomMinimumSize = new Vector2(PanelW - 84, height);
            column.AddChild(label);
        }
        void Heading(string text) => Add(UiKit.MakeLabel(text, 24, Palette.Gold, HorizontalAlignment.Center, outline: 4).Wrapped(), 36);
        void Line(string text, Color? color = null) => Add(UiKit.MakeLabel(text, 16, color ?? Palette.Text, HorizontalAlignment.Center).Wrapped(), 0);
        void Gap() => column.AddChild(new Control { CustomMinimumSize = new Vector2(0, 14) });

        Heading("KEYWRIGHT"); // loc: keys (the name)
        if (Studio.Length > 0) Line(Loc.T($"A game by {Studio}"));
        Line(Loc.T($"Early Access {Main.Version}"), Palette.Muted);
        Gap();
        Heading(Loc.T("Pictures"));
        Line(Loc.T("Made for Keywright to the brief in its art folder, generated with OpenAI's image generation and fitted to its " +
                   "pixel grid and palette a picture at a time. The chest is drawn in code."));
        Gap();
        Heading(Loc.T("Sounds and lettering"));
        Line(Loc.T("Kenney (kenney.nl), CC0: the Kenney Mini font, and the Impact Sounds, RPG Audio, Casino Audio and Interface " +
                   "Sounds packs. The Heartsongs and the other music-box sounds are made in code."));
        Line(Loc.T("Galmuri 9 by Lee Minseo (quiple/galmuri), SIL Open Font License 1.1: the letters for translations."));
        Line(Loc.T("Fusion Pixel 12px by TakWolf (TakWolf/fusion-pixel-font), SIL Open Font License 1.1, with glyphs from Ark " +
                   "Pixel, Cubic 11 and Galmuri: the letters for Japanese and Chinese."));
        Gap();
        Heading(Loc.T("Steam"));
        Line("Steamworks.NET (c) 2013-2022 Riley Labrecque, MIT License. Steamworks SDK (c) Valve Corporation."); // loc: keys (a notice)
        Gap();
        Heading(Loc.T("Made with Godot Engine"));
        Line(Engine.GetLicenseText(), Palette.Muted);
        Gap();
        Heading(Loc.T("Inside Godot Engine"));
        Line(Loc.T("Godot is built with these libraries, under these licences:"), Palette.Muted);
        Line(Components(), Palette.Muted);
        foreach (var (name, text) in Licences())
        {
            Gap();
            Line(name, Palette.Gold.Darkened(0.15f));
            Line(text, Palette.Muted);
        }
        Gap();
        Line(Loc.T("Thank you for playing."), Palette.Gold);
    }

    /// <summary>Every third-party part of the engine, with its licence: "FreeType: FTL", one to a line.</summary>
    static string Components()
    {
        var sb = new StringBuilder();
        foreach (var component in Engine.GetCopyrightInfo())
        {
            string name = component["name"].AsString();
            var licences = component["parts"].AsGodotArray().Select(part => part.AsGodotDictionary()["license"].AsString()).Distinct();
            sb.Append(name).Append(": ").Append(string.Join(", ", licences)).Append('\n');
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>The full text of each licence the engine's parts are under.</summary>
    static (string Name, string Text)[] Licences() =>
        Engine.GetLicenseInfo().Select(pair => (pair.Key.AsString(), pair.Value.AsString().Trim())).OrderBy(l => l.Item1).ToArray();

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed(Controls.Back)) return;
        GetViewport().SetInputAsHandled();
        Close();
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
