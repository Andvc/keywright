using System;
using System.Collections.Generic;
using Godot;
using Loc = Keywright.Core.Loc;

namespace Keywright.Ui;

/// <summary>Small helpers for building pixel-art UI in code with consistent styling.</summary>
public static class UiKit
{
    /// <summary>Screen pixels per UI art pixel. The 9-slice frames in assets/ui are pre-scaled by this.</summary>
    public const int Px = 2;

    /// <summary>9-slice margin: the frames are 12x12 art with 4-pixel margins (outline, bevel, a corner rivet and fill).</summary>
    const int Slice = 4 * Px;

    static Font _font;
    static Texture2D _panel, _frame, _button, _buttonPressed;

    /// <summary>The language's font (<see cref="Localization.MainFont"/>), once the language is set.</summary>
    public static Font Font => _font ??= Localization.MainFont ?? GD.Load<Font>("res://assets/fonts/kenney_mini.ttf");
    static Texture2D PanelTex => _panel ??= ArtTexture.Load("res://assets/ui/panel.png");
    static Texture2D FrameTex => _frame ??= ArtTexture.Load("res://assets/ui/frame.png");
    static Texture2D ButtonTex => _button ??= ArtTexture.Load("res://assets/ui/button.png");
    static Texture2D ButtonPressedTex => _buttonPressed ??= ArtTexture.Load("res://assets/ui/button_pressed.png");

    /// <summary>
    /// Kenney Mini is drawn on an 8px grid, so it's shown at 16, 24, 32...; other sizes render with uneven pixels. In
    /// Japanese and Chinese, Fusion Pixel's grid is 12px: a size comes down to the multiple of 12 at or under it (16 is
    /// 12, 24 and 32 are 24, 48 is 48), so it never takes more room than the English did.
    /// </summary>
    public static int SnapFont(int size)
    {
        int snapped = Math.Max(16, (int)MathF.Round(size / 8f) * 8);
        return Localization.Grid == 12 ? Math.Max(12, snapped / 12 * 12) : snapped;
    }

    /// <summary>Low-contrast row backgrounds let the appraisal's dark ink read as printing on paper.</summary>
    public static void StylePaperButton(Button button)
    {
        foreach (string state in new[] { "normal", "hover", "pressed", "disabled", "focus" })
        {
            var box = new StyleBoxFlat
            {
                BgColor = state == "hover" ? new Color(0.40f, 0.26f, 0.12f, 0.12f)
                    : state == "pressed" ? new Color(0.40f, 0.26f, 0.12f, 0.2f) : new Color(0.40f, 0.26f, 0.12f, 0.04f),
                BorderColor = Palette.InkGold, AntiAliasing = false,
            };
            if (state == "focus") box.SetBorderWidthAll(2);
            button.AddThemeStyleboxOverride(state, box);
        }
        button.AddThemeColorOverride("font_color", Palette.Ink);
    }

    /// <summary>How wide a text is at this size, in the game's font (a translation's letters can be twice as wide).</summary>
    public static float TextWidth(string text, int size = 16) => Font.GetStringSize(text, HorizontalAlignment.Left, -1, SnapFont(size)).X;

    /// <summary>
    /// How tall a wrapped label this wide needs to be for the text: broken into lines the way a label breaks them (at
    /// words), each a line of the font and the theme's spacing between them.
    /// </summary>
    public static float TextHeight(string text, float width, int size = 16)
    {
        size = SnapFont(size);
        var paragraph = new TextParagraph
        {
            Width = width,
            BreakFlags = TextServer.LineBreakFlag.Mandatory | TextServer.LineBreakFlag.WordBound | TextServer.LineBreakFlag.Adaptive,
        };
        paragraph.AddString(text, Font, size);
        int lines = Math.Max(1, paragraph.GetLineCount());
        int spacing = ThemeDB.GetDefaultTheme().GetConstant("line_spacing", "Label"); // loc: keys (a theme type)
        return lines * Font.GetHeight(size) + (lines - 1) * spacing;
    }

    /// <summary>The text, or as much of it as fits in this width with "..." after it (Kenney Mini has no ellipsis).</summary>
    public static string Fit(string text, float width, int size = 16)
    {
        size = SnapFont(size);
        if (Font.GetStringSize(text, HorizontalAlignment.Left, -1, size).X <= width) return text;
        for (int n = text.Length - 1; n > 0; n--)
        {
            string cut = text[..n].TrimEnd() + "...";
            if (Font.GetStringSize(cut, HorizontalAlignment.Left, -1, size).X <= width) return cut;
        }
        return text;
    }

    /// <summary>
    /// A column of a run's boons or banes under a title, a name to a line, each with what it does (and why) on hover.
    /// Returns where the next line would go.
    /// </summary>
    public static float MakeTermList(Control parent, string title, System.Collections.Generic.IEnumerable<string> keys, Color color, float x, float y,
        float width, int count)
    {
        parent.AddChild(MakeLabel(title, 16, color.Darkened(0.15f)).Place(x, y, width, 20));
        float pitch = count > 6 ? 17 : 20;
        y += 21;
        foreach (string key in keys)
        {
            var term = Core.Challenges.Get(key);
            var line = MakeLabel(Fit(term.Name, width), 16, color).Place(x, y, width, 20);
            line.MouseFilter = Control.MouseFilterEnum.Pass;
            line.TooltipText = Loc.T($"{term.Name}: {term.Rule}\n{term.Why}");
            parent.AddChild(line);
            y += pitch;
        }
        return y;
    }

    public static Label MakeLabel(string text, int size, Color color,
        HorizontalAlignment align = HorizontalAlignment.Left, int outline = 0)
    {
        var label = new Label
        {
            Text = text,
            HorizontalAlignment = align,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        label.AddThemeFontSizeOverride("font_size", SnapFont(size));
        label.AddThemeColorOverride("font_color", color);
        if (outline > 0)
        {
            label.AddThemeConstantOverride("outline_size", 4 * ((outline + 3) / 4));
            label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.9f));
        }
        return label;
    }

    public static Label Wrapped(this Label label)
    {
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        return label;
    }

    static StyleBoxTexture Slice9(Texture2D texture, Color modulate, float padding)
    {
        var box = new StyleBoxTexture { Texture = texture, ModulateColor = modulate };
        box.SetTextureMarginAll(Slice);
        box.SetContentMarginAll(padding);
        return box;
    }

    /// <summary>The dark beveled panel background.</summary>
    public static StyleBoxTexture PanelBox(float padding = 12) => Slice9(PanelTex, Colors.White, padding);

    /// <summary>A dark panel, optionally with a colored border.</summary>
    public static Panel MakePanel(Color? border = null)
    {
        var panel = new Panel();
        panel.AddThemeStyleboxOverride("panel", PanelBox());
        if (border is Color color) panel.AddChild(MakeFrame(color));
        return panel;
    }

    /// <summary>A colored 1-art-pixel border that fills its parent Control.</summary>
    public static NinePatchRect MakeFrame(Color color)
    {
        var frame = new NinePatchRect
        {
            Texture = FrameTex,
            Modulate = color,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            PatchMarginLeft = Slice, PatchMarginTop = Slice, PatchMarginRight = Slice, PatchMarginBottom = Slice,
        };
        frame.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        return frame;
    }

    public static Button MakeButton(string text, Color accent, int size = 16, Action onPressed = null)
    {
        // FocusMode.None keeps Space free for lockpicking and holding the lid.
        var button = new Button { Text = text, FocusMode = Control.FocusModeEnum.None };
        button.AddThemeFontSizeOverride("font_size", SnapFont(size));
        var face = accent.Darkened(0.15f);
        button.AddThemeStyleboxOverride("normal", Slice9(ButtonTex, face, 8));
        button.AddThemeStyleboxOverride("hover", Slice9(ButtonTex, face.Lightened(0.25f), 8));
        button.AddThemeStyleboxOverride("pressed", Slice9(ButtonPressedTex, face, 8));
        button.AddThemeStyleboxOverride("disabled", Slice9(ButtonTex, new Color("#3a3448"), 8));
        button.AddThemeColorOverride("font_color", Colors.White);
        button.AddThemeColorOverride("font_hover_color", Colors.White);
        button.AddThemeColorOverride("font_pressed_color", Palette.Text);
        button.AddThemeColorOverride("font_disabled_color", Palette.Muted.Darkened(0.2f));
        button.AddThemeConstantOverride("outline_size", 4);
        button.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.85f));
        // A soft click under whatever the button does.
        button.Pressed += () => Main.Instance?.Sfx?.Play("tick", 1.1f, -16);
        if (onPressed != null) button.Pressed += onPressed;
        return button;
    }

    public static T Place<T>(this T control, float x, float y, float width, float height) where T : Control
    {
        control.Position = new Vector2(x, y);
        control.Size = new Vector2(width, height);
        if (TrackPlacement) control.SetMeta(PlacedMeta, new Vector2(width, height));
        return control;
    }

    /// <summary>
    /// Remember the size each control was placed at, so <see cref="Debug.Overflow"/> can tell which texts outgrew
    /// theirs (on while screenshots are taken, --shots).
    /// </summary>
    public static bool TrackPlacement;

    public const string PlacedMeta = "placed_size";

    /// <summary>A control given a new size on purpose (a box grown to fit its text): the overflow check measures from it.</summary>
    public static void Resized(Control control)
    {
        if (TrackPlacement) control.SetMeta(PlacedMeta, control.Size);
    }

    /// <summary>A pixel sprite at an integer scale, centered on <paramref name="center"/> (outlined: see
    /// <see cref="Outline"/>).</summary>
    public static Sprite2D MakeSprite(string path, Vector2 center, int scale, bool outlined = false)
    {
        var sprite = new Sprite2D
        {
            Texture = ArtTexture.Load(path),
            Position = center,
            Scale = Vector2.One * scale,
        };
        return outlined ? Outline(sprite) : sprite;
    }

    /// <summary>Every outline on screen, which the Outlines setting shows or hides at once (<see cref="ShowOutlines"/>).</summary>
    public const string Outlines = "outlines";

    static readonly Dictionary<(Texture2D, bool), Texture2D> _rings = new();

    /// <summary>
    /// Gives <paramref name="sprite"/> a one-pixel outline round its picture's silhouette, outside it, in
    /// <see cref="Palette.Outline"/>: what has to stand out from the scene behind it (everyone's figure and face). The game
    /// draws it rather than the art, which round 2 draws without outlines. It's a sprite of its own behind the picture,
    /// lit and moved with it, and the Outlines setting shows or hides every one. A <paramref name="whole"/> figure is
    /// outlined beyond its picture's edges too (its feet stand on the bottom one); a face cut from a figure isn't, so the
    /// cut stays bare. Call it again after changing the sprite's picture.
    /// </summary>
    public static T Outline<T>(T sprite, bool whole = false) where T : Sprite2D
    {
        foreach (var old in sprite.GetChildren())
            if (old.IsInGroup(Outlines)) old.QueueFree();
        if (!ArtTexture.OutlinesAllowed) return sprite;
        var ring = new Sprite2D
        {
            Texture = Ring(sprite.Texture, whole),
            Centered = sprite.Centered,
            // One pixel bigger all round: centred, it lines up by itself; from the corner, it starts a pixel up and left.
            Position = sprite.Centered ? Vector2.Zero : new Vector2(-1, -1),
            ShowBehindParent = true,
            Visible = Settings.Outlines,
        };
        ring.AddToGroup(Outlines);
        sprite.AddChild(ring);
        return sprite;
    }

    /// <summary>Shows or hides every outline on screen, for the Outlines setting.</summary>
    public static void ShowOutlines(SceneTree tree, bool on)
    {
        on &= ArtTexture.OutlinesAllowed;
        foreach (var node in tree.GetNodesInGroup(Outlines))
            if (node is CanvasItem item) item.Visible = on;
    }

    /// <summary>The outline of a picture alone, a pixel bigger on every side: the ring round its silhouette (inside the
    /// picture's own edges unless it's <paramref name="whole"/>).</summary>
    static Texture2D Ring(Texture2D picture, bool whole)
    {
        if (_rings.TryGetValue((picture, whole), out var done)) return done;
        var art = picture.GetImage();
        if (art.IsCompressed()) art.Decompress();
        art.Convert(Image.Format.Rgba8);
        int w = art.GetWidth(), h = art.GetHeight();
        bool Solid(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && art.GetPixel(x, y).A >= 0.5f;
        var ring = Image.CreateEmpty(w + 2, h + 2, false, Image.Format.Rgba8);
        int edge = whole ? 1 : 0;
        for (int y = -edge; y < h + edge; y++)
            for (int x = -edge; x < w + edge; x++)
                if (!Solid(x, y) && (Solid(x - 1, y) || Solid(x + 1, y) || Solid(x, y - 1) || Solid(x, y + 1)))
                    ring.SetPixel(x + 1, y + 1, Palette.Outline);
        return _rings[(picture, whole)] = ImageTexture.CreateFromImage(ring);
    }

    /// <summary>The square every thing's art fits in (trimmed, a sprite is at most this): slots scale by it, so things
    /// side by side are all at the same scale and their sizes compare.</summary>
    const int ThingArt = 30;

    /// <summary>
    /// A thing's own sprite for a small slot, the same picture it had when it came out of the chest: at the largest
    /// integer scale at which any thing would fit a <paramref name="box"/>-pixel square, centered on
    /// <paramref name="center"/> (or standing on it). Something enchanted glows behind it; fakes and broken things look dull.
    /// </summary>
    public static Node2D MakeItemIcon(Core.Item item, Vector2 center, float box, bool standing = false)
    {
        var texture = ArtTexture.Load($"res://assets/sprites/items/icons/{item.Sprite}.png");
        int scale = Math.Max(1, (int)(box / ThingArt));
        var middle = standing ? new Vector2(0, -texture.GetHeight() * scale / 2f) : Vector2.Zero;
        var icon = new Node2D { Position = center };
        if (item.Magic && !item.Broken)
            icon.AddChild(new Painter(p =>
            {
                for (int i = 3; i >= 1; i--) p.DrawCircle(middle, box * 0.5f * i / 3f, Palette.Magic with { A = 0.16f });
            }));
        var sprite = new Sprite2D { Texture = texture, Scale = Vector2.One * scale, Position = middle };
        if (item.Fake || item.Broken) sprite.Modulate = new Color(0.6f, 0.6f, 0.65f);
        icon.AddChild(sprite);
        return icon;
    }

    /// <summary>A curio's picture (56px, already at its integer scale), centered on <paramref name="center"/>.</summary>
    public static Sprite2D MakeCurioIcon(Core.CurioDef curio, Vector2 center) => MakeSprite($"res://assets/ui/curios/{curio.Icon}.png", center, 1);

    /// <summary>
    /// A keepsake (0.32): its picture fitted to a curio's 56px, on a teal halo so it never reads as a curio (it takes no
    /// slot). Until each has its own art, it borrows a curio's, a tool's or a thing's picture (Core.Keepsakes).
    /// </summary>
    public static Node2D MakeKeepsakeIcon(Core.KeepsakeDef keepsake, Vector2 center, float box = 56)
    {
        var holder = new Node2D { Position = center };
        holder.AddChild(new Painter(p =>
        {
            p.DrawCircle(Vector2.Zero, box * 0.46f, new Color(Palette.Keepsake, 0.22f));
            p.DrawArc(Vector2.Zero, box * 0.46f, 0, Mathf.Tau, 40, new Color(Palette.Keepsake, 0.85f), 2f);
        }));
        var texture = ArtTexture.Load(keepsake.Icon);
        float size = Math.Max(texture.GetWidth(), texture.GetHeight());
        // A whole scale where the picture is small (a thing's 30px at 2x), never larger than the box.
        float scale = size * 2 <= box ? Mathf.Floor(box / size) : box * 0.86f / size;
        holder.AddChild(new Sprite2D { Texture = texture, Scale = Vector2.One * Math.Min(scale, 1.5f * box / size) });
        return holder;
    }

    /// <summary>A keepsake as a tooltip says it: its name, what it does, and that it takes no slot.</summary>
    public static string KeepsakeTooltip(Core.KeepsakeDef keepsake) =>
        Loc.T($"{keepsake.Name}: {keepsake.Blurb}") + "\n\n" + Loc.T("A keepsake, from a tale: it takes no curio slot, and it's never sold.");

    /// <summary>
    /// A question that wants an answer before going on: the screen dimmed, a framed panel with a title and what's at
    /// stake, and two buttons. The caller adds it, and frees it when either is pressed.
    /// </summary>
    public static Control MakeConfirm(string title, string text, Color color, string yes, Action onYes, string no, Action onNo)
    {
        var root = new Control { ZIndex = 50 }.Place(0, 0, 1280, 720);
        root.AddChild(new ColorRect { Color = new Color(0, 0, 0, 0.6f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920));
        var panel = MakePanel(color.Darkened(0.2f)).Place(330, 200, 620, 290);
        root.AddChild(panel);
        panel.AddChild(MakeLabel(title, 32, color, HorizontalAlignment.Center, outline: 4).Place(0, 20, 620, 40));
        panel.AddChild(MakeLabel(text, 16, Palette.Text, HorizontalAlignment.Center).Wrapped().Place(40, 72, 540, 112));
        panel.AddChild(MakeButton(yes, color, 24, onYes).Place(60, 204, 240, 56));
        panel.AddChild(MakeButton(no, Palette.PanelBorder.Lightened(0.2f), 24, onNo).Place(320, 204, 240, 56));
        return root;
    }

    /// <summary>
    /// How your gut rates a call: a strip running from red (a bad call) to green (a great one), with a marker that slides
    /// out from the middle to <paramref name="score"/> (0 to 1). The words at either end say what each end means.
    /// </summary>
    public static Control MakeGutMeter(float score, float width, string worst, string best)
    {
        const float h = 16, words = 120;
        var meter = new Control { MouseFilter = Control.MouseFilterEnum.Ignore, Size = new Vector2(width + 2 * words, 30) };
        meter.AddChild(MakeLabel(worst, 16, Palette.Bad, HorizontalAlignment.Right, outline: 4).Place(0, 0, words - 8, 20));
        meter.AddChild(MakeLabel(best, 16, Palette.Good, outline: 4).Place(words + width + 8, 0, words, 20));
        float at = 0.5f;
        var strip = new Painter(p =>
        {
            for (int x = 0; x < width; x += 4) p.DrawRect(new Rect2(x, 0, 4, h), Palette.Gradient(x / width).Darkened(0.2f));
            p.DrawRect(new Rect2(0, 0, width, h), Palette.PanelBorder, false, 3);
            // The marker: a bright notch with a little pointer above and below.
            float mx = Mathf.Round(width * at);
            var c = Palette.Gradient(at).Lightened(0.35f);
            p.DrawRect(new Rect2(mx - 3, -4, 6, h + 8), Colors.Black);
            p.DrawRect(new Rect2(mx - 1, -2, 2, h + 4), c);
        }) { Position = new Vector2(words, 2) };
        meter.AddChild(strip);
        meter.CreateTween().TweenMethod(Callable.From<float>(t =>
        {
            at = t;
            strip.QueueRedraw();
        }), 0.5f, Mathf.Clamp(score, 0f, 1f), 0.6f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out).SetDelay(0.2f);
        return meter;
    }

    /// <summary>The 32px icon for a kind of thing (coins, metal...), centered on <paramref name="center"/>.</summary>
    public static Sprite2D MakeKindIcon(Core.Category category, Vector2 center) => MakeKindIcon(category.ToString().ToLowerInvariant(), center);

    /// <summary>A 32px row icon by name, e.g. "hidden" for a false bottom.</summary>
    public static Sprite2D MakeKindIcon(string name, Vector2 center) => MakeSprite($"res://assets/ui/kinds/{name}.png", center, 1);

    /// <summary>A chunky pixel tick (right) or cross (wrong), 24px at scale 1, centered on <paramref name="center"/>.</summary>
    public static Painter MakeCheck(bool right, Vector2 center, int scale)
    {
        var color = right ? Palette.Good : Palette.Bad;
        // Drawn on an 8x8 grid of 3px squares so it reads as pixel art next to the font.
        string[] rows = right
            ? new[] { "........", ".......#", "......##", "#....##.", "##..##..", ".####...", "..##....", "........" }
            : new[] { "##....##", ".##..##.", "..####..", "...##...", "..####..", ".##..##.", "##....##", "........" };
        return new Painter(p =>
        {
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                if (rows[y][x] != '#') continue;
                p.DrawRect(new Rect2(x * 3 - 13, y * 3 - 11, 5, 5), new Color(0, 0, 0, 0.8f));
            }
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                if (rows[y][x] == '#') p.DrawRect(new Rect2(x * 3 - 12, y * 3 - 12, 3, 3), color);
        }) { Position = center, Scale = Vector2.One * scale };
    }

    /// <summary>
    /// The mark for a thing nobody could have read: on a chest bought sight unseen, what you didn't mark isn't a miss.
    /// A closed eye, on the grid <see cref="MakeCheck"/> uses, in a colour that's neither right nor wrong.
    /// </summary>
    public static Painter MakeUnseenMark(Vector2 center, int scale)
    {
        string[] rows = { ".........", "#.......#", ".##...##.", "...###...", "..#.#.#..", ".#..#..#.", "........." };
        return new Painter(p =>
        {
            for (int y = 0; y < rows.Length; y++)
            for (int x = 0; x < rows[y].Length; x++)
                if (rows[y][x] == '#') p.DrawRect(new Rect2(x * 3 - 14, y * 3 - 11, 5, 5), new Color(0, 0, 0, 0.8f));
            for (int y = 0; y < rows.Length; y++)
            for (int x = 0; x < rows[y].Length; x++)
                if (rows[y][x] == '#') p.DrawRect(new Rect2(x * 3 - 13, y * 3 - 12, 3, 3), Palette.Unseen);
        }) { Position = center, Scale = Vector2.One * scale };
    }

    /// <summary>A pixel-framed slider with a gold coin for a grabber (the offer slider, the volume).</summary>
    public static void StyleSlider(HSlider slider)
    {
        var coin = ArtTexture.Load("res://assets/sprites/items/dwarf_gold_crowns.png");
        var track = new StyleBoxFlat { BgColor = new Color("#120e1a"), BorderColor = Palette.PanelBorder, AntiAliasing = false };
        track.SetBorderWidthAll(3);
        track.SetContentMarginAll(6);
        var fill = new StyleBoxFlat { BgColor = Palette.Good.Darkened(0.45f), AntiAliasing = false };
        fill.SetContentMarginAll(6);
        slider.AddThemeStyleboxOverride("slider", track);
        slider.AddThemeStyleboxOverride("grabber_area", fill);
        slider.AddThemeStyleboxOverride("grabber_area_highlight", fill);
        slider.AddThemeIconOverride("grabber", coin);
        slider.AddThemeIconOverride("grabber_highlight", coin);
    }

    /// <summary>
    /// The shop's native 320x180 layers at 4x, with a sky and lighting for the hour and props for purchased tools.
    /// The counter's top edge is at y 476 and the display shelf's board is at y 196.
    /// </summary>
    public static ShopView MakeShop(int tier, float brightness = 1f, ShopHour hour = ShopHour.Midday,
        Core.GameState state = null, bool transition = false) => new(tier, brightness, hour, state, transition);

    /// <summary>A 640x360 picture filling the screen at 2x, dimmed to <paramref name="brightness"/>, with a vignette.</summary>
    public static Node2D MakeBackdrop(string path, float brightness = 1f)
    {
        var root = new Node2D();
        // Beyond the picture's edges, for when the camera shakes or leans in.
        root.AddChild(new ColorRect { Color = Palette.Bg, MouseFilter = Control.MouseFilterEnum.Ignore }.Place(-120, -120, 1520, 960));
        root.AddChild(new Sprite2D
        {
            Texture = ArtTexture.Load(path),
            Centered = false,
            Scale = Vector2.One * 2,
            Modulate = new Color(brightness, brightness, brightness * 1.03f),
        });
        root.AddChild(Vignette(brightness >= 0.8f ? 0.05f : 0.1f));
        return root;
    }

    /// <summary>A stepped vignette: hard bands read as pixel-art lighting rather than a smooth gradient.</summary>
    static Painter Vignette(float alpha) => new(p =>
    {
        for (int i = 0; i < 5; i++)
        {
            float inset = i * 24;
            p.DrawRect(new Rect2(-100, -100, 1480, 100 + inset), new Color(0, 0, 0, alpha));
            p.DrawRect(new Rect2(-100, 720 - inset, 1480, 100 + inset), new Color(0, 0, 0, alpha));
            p.DrawRect(new Rect2(-100, -100, 100 + inset, 920), new Color(0, 0, 0, alpha));
            p.DrawRect(new Rect2(1280 - inset, -100, 100 + inset, 920), new Color(0, 0, 0, alpha));
        }
    });
}
