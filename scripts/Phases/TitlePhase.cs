using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>
/// The title screen (0.32): your shop as you left it (or the roadside stall), and a locked chest that strains and
/// bursts open. Everything on the screen comes out of it: the name first, rising out of the light, then Continue, New
/// game, the Collection, Settings, Quit and the Credits, each flung up out of the chest and landing in its place. Any
/// click, key or button finishes it at once (a click on something that has already landed is just a click). The first
/// title of a session plays in full (a little over two seconds); coming back to it after a run, from the pause menu or
/// the character screen, it's quicker. Starting over when there's a saved shop asks first.
/// </summary>
public partial class TitlePhase : Phase
{
    const float ButtonsX = 490, ButtonW = 300;

    /// <summary>The chest's lid seam, and its mouth once it's open: where everything comes out.</summary>
    static readonly Vector2 ChestAt = new(640, 300), Mouth = new(640, 254), LogoAt = new(640, 92);

    static readonly Color Light = Palette.Of(Rarity.Legendary);

    /// <summary>A session sees the whole burst once; after that the title comes back quicker.</summary>
    static bool _played;

    readonly SaveData _save;
    ChestView _chest;
    Node2D _lid, _rays;
    Vector2 _lidFrom;
    CpuParticles2D _motes;
    Control _confirm, _collectionButton, _settingsButton;
    SettingsView _settings;
    CollectionView _collection;

    // The burst's timeline, in seconds since the title's first frame: the build, the burst, the name, the menu.
    float _t, _build, _burstAt, _itemsAt, _end;
    bool _burst, _done, _quiet, _creaked, _thumped, _shotBuild, _shotName, _shotMenu;
    Painter _flare;
    ulong _skippedFrame;
    readonly List<Flyer> _flyers = new();
    readonly List<(CanvasItem Item, float At)> _fades = new();

    /// <summary>Something that comes out of the chest: where it lands, when it leaves, and the curve it takes.</summary>
    sealed class Flyer
    {
        public CanvasItem Node;
        public Vector2 Home, Size, BaseScale = Vector2.One;

        /// <summary>The curve's middle points: up and out of the mouth, then down beside its place (a cubic's P1, P2).</summary>
        public Vector2 Up, Down;

        public float Leaves, Flight, Spin;

        /// <summary>The name rises straight up out of the light, a little past its place, and settles back.</summary>
        public bool Straight;

        public bool Left, Landed;
        public Action OnLeave, OnLand;
    }

    public TitlePhase(SaveData save = null)
    {
        _save = save;
    }

    public TitlePhase() { }

    public override void _Ready()
    {
        bool full = !_played;
        _played = true;
        // In full: the chest strains for most of a second, bursts, the name rises, and the menu follows a beat apart.
        _build = full ? 0.85f : 0.3f;
        float nameFlight = full ? 0.6f : 0.32f, stagger = full ? 0.11f : 0.055f, flight = full ? 0.5f : 0.32f;
        float nameLeaves = _build + 0.04f, nameLands = nameLeaves + nameFlight;
        _itemsAt = _build + (full ? 0.3f : 0.14f);

        Add(UiKit.MakeShop(_save?.Tier ?? 1, 0.45f));
        Add(new Painter(p =>
        {
            // Darker bands behind the name and the buttons, in hard steps, so they read over any shop.
            p.DrawRect(new Rect2(0, 0, 1280, 720), new Color(0, 0, 0, 0.2f));
            for (int i = 0; i < 3; i++)
            {
                p.DrawRect(new Rect2(0, 60 + i * 8, 1280, 140 - i * 16), new Color(0, 0, 0, 0.18f));
                p.DrawRect(new Rect2(400 + i * 16, 360 + i * 8, 480 - i * 32, 260 - i * 16), new Color(0, 0, 0, 0.16f));
            }
        }));

        // Slow rays behind the chest once it's open, and motes of its light drifting up out of it.
        _rays = Fx.Rays(this, Mouth, Light, 230f, 12, 0.1f);
        _rays.ZIndex = 0;
        _rays.Modulate = Colors.Transparent;
        _chest = Add(new ChestView { Position = ChestAt, Skin = "purple", Lock = 2, GlowColor = Light, Glow = 0.2f });
        _motes = Add(Motes());

        var logo = Add(UiKit.MakeSprite("res://assets/ui/logo.png", LogoAt, 2));
        Launch(logo, LogoAt, Vector2.Zero, nameLeaves, nameFlight, straight: true, onLand: () =>
        {
            Sfx.Play("chime", 0.9f, -8);
            Sfx.PlayGroup("land", 0.7f, -6);
            Game.Shake(0.12f);
        });
        Fade(Add(UiKit.MakeLabel(Loc.T("Every treasure in Aldmark is locked in a chest. Only a Keywright can open one."), 16, Palette.Text,
            HorizontalAlignment.Center, outline: 4).Place(0, 164, 1280, 24)), nameLands);

        // The menu, each in its place; each comes out of the chest in turn, alternately to the right and the left.
        var menu = new List<Control>();
        float y = 380;
        if (_save != null)
        {
            var go = Add(UiKit.MakeButton(Loc.T("Continue"), Palette.Gold, 24, () => Game.ContinueGame(_save)).Place(ButtonsX, y, ButtonW, 56));
            // Centred under the button, with room either side for a longer language's shop names.
            var summary = Add(UiKit.MakeLabel(_save.Summary, 16, Palette.Muted, HorizontalAlignment.Center, outline: 4)
                .Place(ButtonsX - 100, y + 60, ButtonW + 200, 20));
            menu.Add(go);
            Fade(summary, _itemsAt + flight);
            y += 92;
        }
        menu.Add(Add(UiKit.MakeButton(Loc.T("New game"), _save == null ? Palette.Gold : Palette.Text, _save == null ? 24 : 16, NewGame)
            .Place(ButtonsX, y, ButtonW, _save == null ? 56 : 44)));
        y += _save == null ? 70 : 56;
        menu.Add(_collectionButton = Add(UiKit.MakeButton(Loc.T("Collection"), Palette.Text, 16, () => OpenCollection()).Place(ButtonsX, y, ButtonW, 44)));
        y += 56;
        menu.Add(_settingsButton = Add(UiKit.MakeButton(Loc.T("Settings"), Palette.Text, 16, OpenSettings).Place(ButtonsX, y, ButtonW, 44)));
        y += 56;
        menu.Add(Add(UiKit.MakeButton(Loc.T("Quit"), Palette.Text, 16, () => Game.QuitGame()).Place(ButtonsX, y, ButtonW, 44)));
        // Who made what (and the notices the engine and the fonts ask to come with the game): last out, into the corner.
        var credits = Add(UiKit.MakeButton(Loc.T("Credits"), Palette.Text, 16, () => OpenCredits()).Place(1120, 670, 140, 36));

        for (int i = 0; i < menu.Count; i++)
        {
            var button = menu[i];
            var home = button.Position + button.Size / 2;
            float side = i % 2 == 0 ? 1 : -1;
            // Each a little higher than the one before: they land as a rising run of notes.
            float pitch = 0.95f + 0.1f * i, rush = 1.5f + 0.06f * i;
            Launch(button, home, button.Size, _itemsAt + i * stagger, flight,
                up: new Vector2(640 + side * 260, 20), down: new Vector2(640 + side * 520, home.Y), spin: side * 0.5f,
                onLeave: () => Sfx.Play("whoosh", rush, -16),
                onLand: () => Sfx.Play("blip", pitch, -9));
        }
        var corner = credits.Position + credits.Size / 2;
        Launch(credits, corner, credits.Size, _itemsAt + menu.Count * stagger, flight,
            up: new Vector2(900, 20), down: new Vector2(corner.X, 560), spin: 0.4f,
            onLeave: () => Sfx.Play("whoosh", 1.9f, -16),
            onLand: () => Sfx.Play("blip", 1.6f, -10));
        _end = _flyers.Max(f => f.Leaves + f.Flight);

        Fade(Add(UiKit.MakeLabel(Loc.T($"Early Access {Main.Version}"), 16, Palette.Muted, outline: 4).Place(24, 684, 400, 20)), _end - 0.1f);
        if (Game.Profile?.Summary is string record)
            Fade(Add(UiKit.MakeLabel(record, 16, Palette.Gold.Darkened(0.15f), HorizontalAlignment.Center, outline: 4).Place(0, 196, 1280, 20)), nameLands + 0.1f);
        // A file this version couldn't read was set aside (Storage): say so, once.
        if (Storage.Notices.Count > 0)
        {
            Fade(Add(UiKit.MakeLabel(string.Join(" ", Storage.Notices), 16, Palette.Warn, HorizontalAlignment.Center, outline: 4)
                .Wrapped().Place(140, 222, 1000, 44)), nameLands + 0.1f);
            Storage.Notices.Clear();
        }
        Step(0);
    }

    /// <summary>Something comes out of the chest at <paramref name="leaves"/> and lands at <paramref name="home"/> (its centre).</summary>
    void Launch(CanvasItem node, Vector2 home, Vector2 size, float leaves, float flight, Vector2 up = default, Vector2 down = default,
        float spin = 0, bool straight = false, Action onLeave = null, Action onLand = null)
    {
        node.Visible = false;
        node.ZIndex = 6;
        // Not to be clicked until it's landed.
        if (node is Control control) control.MouseFilter = Control.MouseFilterEnum.Ignore;
        _flyers.Add(new Flyer
        {
            Node = node, Home = home, Size = size, BaseScale = node is Node2D sprite ? sprite.Scale : Vector2.One, Leaves = leaves,
            Flight = flight, Up = up, Down = down, Spin = spin, Straight = straight, OnLeave = onLeave, OnLand = onLand,
        });
    }

    /// <summary>Words that fade in at a moment (the line under the name once it's landed, the save's summary under Continue).</summary>
    void Fade(CanvasItem item, float at)
    {
        item.Modulate = Colors.Transparent;
        _fades.Add((item, at));
    }

    /// <summary>Light drifting up out of the open chest, slowly, for as long as the title's up.</summary>
    static CpuParticles2D Motes()
    {
        var ramp = new Gradient();
        ramp.Offsets = new[] { 0f, 0.25f, 1f };
        ramp.Colors = new[] { Light with { A = 0f }, Light.Lightened(0.5f) with { A = 0.85f }, Light with { A = 0f } };
        return new CpuParticles2D
        {
            Emitting = false,
            Amount = 14,
            Lifetime = 3.2f,
            Texture = Fx.Pixel,
            Material = Fx.Additive(),
            EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle,
            EmissionRectExtents = new Vector2(64, 4),
            Direction = Vector2.Up,
            Spread = 14,
            InitialVelocityMin = 18,
            InitialVelocityMax = 44,
            Gravity = new Vector2(0, -6),
            ScaleAmountMin = 1f,
            ScaleAmountMax = 2f,
            ColorRamp = ramp,
            Position = Mouth,
            ZIndex = 4,
        };
    }

    public override void _Process(double delta)
    {
        // The squash of the last things to land plays out after the menu's usable.
        if (_done && _t > _end + 1f)
        {
            _t += (float)delta;
            // Open, its light breathing.
            _chest.Glow = 0.5f + 0.08f * MathF.Sin(_t * 1.7f);
            return;
        }
        Step((float)delta);
    }

    /// <summary>Moves the timeline on and puts everything where it is at that moment.</summary>
    void Step(float dt)
    {
        _t += dt;
        float fx = Settings.Shake;
        if (!_burst)
        {
            // It strains: a rumble, a widening crack of light, the hum rising, and the room leaning in.
            float p = Math.Min(1f, _t / _build);
            _chest.Rumble = 0.25f + 0.65f * p;
            _chest.LidGap = 12f * p * p;
            _chest.Glow = 0.2f + 0.8f * p;
            if (dt > 0) Sfx.Hum(0.15f + 0.55f * p);
            Game.Zoom(1f + 0.05f * fx * p * p, ChestAt);
            // A creak as it starts, and something inside knocks to get out.
            if (!_creaked && dt > 0)
            {
                _creaked = true;
                Sfx.Play("strain", 0.9f, -10);
            }
            if (!_thumped && p >= 0.55f)
            {
                _thumped = true;
                _chest.Wobble(0.35f);
                Sfx.Play("thud", 0.8f, -9);
            }
            if (!_shotBuild && p >= 0.75f)
            {
                _shotBuild = true;
                Game.Moment("00_title_build");
            }
            if (p >= 1f) Burst();
        }
        else
        {
            float d = _t - _burstAt;
            // The lid blows straight up, spinning, and out of the top of the picture, clear of everything that follows it.
            if (_lid != null && _lid.Visible)
            {
                _lid.Position = _lidFrom + new Vector2(-90f * d, -1500f * d + 400f * d * d);
                _lid.Rotation = -6f * d;
                if (_lid.GlobalPosition.Y < -240f || d > 1f) _lid.Visible = false;
            }
            _flare?.QueueRedraw();
            // The camera snaps back past where it was, and settles; the light dies down to a glow.
            Game.Zoom(d < 0.9f ? 1f + 0.05f * fx * MathF.Exp(-7f * d) * MathF.Cos(16f * d) : 1f, ChestAt);
            _chest.Glow = d < 1f ? Mathf.Lerp(1f, 0.5f, d) : 0.5f + 0.08f * MathF.Sin(_t * 1.7f);
            _rays.Modulate = Colors.White with { A = Math.Min(1f, d / 0.5f) };
        }

        foreach (var f in _flyers) Fly(f);
        foreach (var (item, at) in _fades) item.Modulate = Colors.White with { A = Math.Clamp((_t - at) / 0.25f, 0f, 1f) };

        if (_burst && !_shotName && _t >= _burstAt + 0.25f)
        {
            _shotName = true;
            Game.Moment("00_title_name");
        }
        if (!_shotMenu && _t >= _itemsAt + 0.42f)
        {
            _shotMenu = true;
            Game.Moment("00_title_menu");
        }
        if (!_done && _t >= _end) Done();
    }

    /// <summary>The lid goes, the light comes out, and everything after comes out of it.</summary>
    void Burst()
    {
        _burst = true;
        _burstAt = _t;
        Sfx.Hum(0);
        _chest.Rumble = 0;
        _chest.LidGap = 0;
        _chest.Glow = 1f;
        _lid = _chest.DetachLid();
        _lidFrom = _lid.Position;
        _chest.ShowOpen();
        _motes.Emitting = true;
        if (_quiet)
        {
            _lid.Visible = false;
            return;
        }
        Sfx.Play("burst", 1f, -4);
        Sfx.Play("reveal_big", 1f, -7);
        float fx = Settings.Shake;
        if (fx > 0) Game.HitStop(0.07 * fx);
        Game.Shake(0.3f);
        Game.Flash(Light.Lightened(0.6f), 0.4f, 0.45f);
        // The light comes out: a flare from the mouth, a column of it, one ring, and sparks.
        _flare = Add(new Painter(DrawFlare) { Position = Mouth, Material = Fx.Additive(), ZIndex = 5 });
        Fx.Pillar(this, Mouth, Light, 150f, 600f, 1f);
        Fx.Ring(this, Mouth, Light.Lightened(0.4f), 260f, 0.55f, 5f);
        Fx.Burst(this, Mouth, Light, 50, 380f);
        Game.Moment("00_title_burst", 0.05);
    }

    /// <summary>
    /// The light as the lid goes: a bright core that swells and fades in half a second, in hard-edged rings (it's pixel
    /// art), and a flat streak across the mouth.
    /// </summary>
    void DrawFlare(Painter p)
    {
        float d = _t - _burstAt;
        if (d > 0.6f)
        {
            p.Visible = false;
            return;
        }
        // As bright as the flashes are set to be (never gone: it's the light the menu comes out of).
        float k = d / 0.6f, fade = (1 - k) * (1 - k) * (0.4f + 0.6f * Settings.Shake);
        float r = 40f + 150f * (1 - (1 - k) * (1 - k));
        for (int i = 4; i >= 1; i--) p.DrawCircle(Vector2.Zero, r * i / 4f, Light.Lightened(0.3f + 0.12f * (4 - i)) with { A = 0.16f * fade });
        float w = 120f + 260f * k;
        p.DrawRect(new Rect2(-w, -3, w * 2, 6), Light.Lightened(0.7f) with { A = 0.5f * fade });
    }

    /// <summary>Where a thing is on its way out of the chest, and how it lands: a squash that springs back.</summary>
    void Fly(Flyer f)
    {
        float local = _t - f.Leaves;
        if (local < 0)
        {
            f.Node.Visible = false;
            return;
        }
        f.Node.Visible = true;
        if (!f.Left)
        {
            f.Left = true;
            if (!_quiet) f.OnLeave?.Invoke();
        }
        float k = Math.Min(1f, local / f.Flight);
        Vector2 at;
        float scale, rotation;
        if (f.Straight)
        {
            float e = EaseOutBack(k);
            at = Mouth.Lerp(f.Home, e);
            scale = Mathf.Lerp(0.2f, 1f, Math.Min(1f, e));
            rotation = 0;
        }
        else
        {
            // Up and out of the mouth, down beside its place, and in: a cubic, so it never crosses the chest on its way.
            float u = k, v = 1 - k;
            at = v * v * v * Mouth + 3 * u * v * v * f.Up + 3 * u * u * v * f.Down + u * u * u * f.Home;
            scale = Mathf.Lerp(0.3f, 1f, 1 - v * v);
            rotation = f.Spin * v * v;
        }
        float squash = 0;
        if (k >= 1)
        {
            if (!f.Landed)
            {
                f.Landed = true;
                if (f.Node is Control control) control.MouseFilter = Control.MouseFilterEnum.Stop;
                if (!_quiet) f.OnLand?.Invoke();
            }
            float s = (local - f.Flight) / 0.38f;
            if (s < 1) squash = 0.16f * (1 - s) * (1 - s) * MathF.Cos(s * MathF.PI * 2.5f);
        }
        var stretch = new Vector2(scale * (1 + squash), scale * (1 - squash));
        switch (f.Node)
        {
            case Control c:
                c.PivotOffset = f.Size / 2;
                c.Position = at - f.Size / 2;
                c.Scale = stretch;
                c.Rotation = rotation;
                break;
            case Node2D n:
                n.Position = at;
                n.Scale = f.BaseScale * stretch;
                n.Rotation = rotation;
                break;
        }
    }

    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1;
        float m = t - 1;
        return 1 + c3 * m * m * m + c1 * m * m;
    }

    /// <summary>Everything's out: the menu's yours.</summary>
    void Done()
    {
        _done = true;
        Game.Moment("00_title", _quiet ? 0.1 : 0.45);
    }

    /// <summary>Everything in its place at once (a click, a key or a button while it's still coming out).</summary>
    public void Finish()
    {
        if (_done) return;
        _quiet = true;
        if (!_burst) Burst();
        if (_lid != null) _lid.Visible = false;
        // Past every landing and its squash: all of it settled.
        _t = Math.Max(_t, _end + 0.5f);
        _shotBuild = _shotName = _shotMenu = true;
        Step(0);
        Game.Zoom(1f, ChestAt);
        Sfx.Play("chime", 1.2f, -14);
    }

    public override void _Input(InputEvent e)
    {
        if (_done)
        {
            // The press that finished it (or the controller's A, which the pointer turns into a click a frame later)
            // doesn't also press whatever landed under the pointer.
            if (_swallow && e is InputEventMouseButton { Pressed: true } && Engine.GetProcessFrames() <= _skippedFrame + 1)
                GetViewport().SetInputAsHandled();
            return;
        }
        // Something opened over the title while the rest was still coming out (a button that had landed): whatever's
        // pressed finishes the burst and still goes to what's open.
        bool over = _collection != null || _settings != null || _credits != null || _confirm != null;
        switch (e)
        {
            case InputEventMouseButton { Pressed: true } click when click.ButtonIndex is MouseButton.Left or MouseButton.Right or MouseButton.Middle:
                // Something that's already landed takes its click as usual (where the click is, in the scene's terms).
                var pointer = ((InputEventMouseButton)MakeInputLocal(click)).Position;
                if (!over && _flyers.Any(f => f.Landed && f.Node is Control c && c.GetGlobalRect().HasPoint(pointer))) return;
                Skip(swallow: !over);
                break;
            case InputEventKey { Pressed: true, Echo: false }:
                Skip(swallow: !over);
                break;
            case InputEventJoypadButton { Pressed: true }:
            case InputEventScreenTouch { Pressed: true }:
                // Not swallowed: the controller's pointer and touch go on to do what they do, once everything's in place.
                Finish();
                if (!over) _swallow = true;
                _skippedFrame = Engine.GetProcessFrames();
                break;
        }
    }

    bool _swallow;

    /// <summary>A press that cuts the burst short: everything in place, and (unless something's open over the title) the press ends there.</summary>
    void Skip(bool swallow)
    {
        Finish();
        _skippedFrame = Engine.GetProcessFrames();
        _swallow = swallow;
        if (swallow) GetViewport().SetInputAsHandled();
    }

    public override void _ExitTree()
    {
        // Gone mid-burst (a quick click on New game): the hum stops with it.
        if (!_burst) Main.Instance?.Sfx?.Hum(0);
    }

    // For the title test (--title-test): whether it's all out, and two of the buttons to aim at.
    public bool Finished => _done;
    public bool Settled => _flyers.All(f => f.Landed && f.Node.Visible && f.Node is not Control { MouseFilter: Control.MouseFilterEnum.Ignore });
    public Control CollectionButton => _collectionButton;
    public Control SettingsButton => _settingsButton;
    public bool HasLanded(Control button) => _flyers.Any(f => f.Node == button && f.Landed);
    public bool CollectionOpen => _collection != null;
    public bool SettingsOpen => _settings != null;

    /// <summary>Where a menu button sits once it's landed (its centre).</summary>
    public Vector2 HomeOf(Control button) => _flyers.First(f => f.Node == button).Home;

    void NewGame()
    {
        if (_save == null)
        {
            Game.OpenSetup();
            return;
        }
        if (_confirm != null) return;
        // Starting over loses the saved shop: ask first.
        _confirm = Add(new Control { MouseFilter = Control.MouseFilterEnum.Ignore }.Place(0, 0, 1280, 720));
        _confirm.ZIndex = 20;
        _confirm.AddChild(new ColorRect { Color = new Color(0, 0, 0, 0.6f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920));
        _confirm.AddChild(UiKit.MakePanel(Palette.Bad).Place(390, 250, 500, 220));
        _confirm.AddChild(UiKit.MakeLabel(Loc.T("Start over?"), 32, Palette.Text, HorizontalAlignment.Center, outline: 4).Place(390, 272, 500, 40));
        _confirm.AddChild(UiKit.MakeLabel(Loc.T($"Your shop ({_save.Summary}) will be gone for good. Its score so far is noted when the new run starts."),
            16, Palette.Muted, HorizontalAlignment.Center).Wrapped().Place(420, 320, 440, 64));
        _confirm.AddChild(UiKit.MakeButton(Loc.T("Start over"), Palette.Bad, 16, Game.OpenSetup).Place(420, 392, 200, 48));
        _confirm.AddChild(UiKit.MakeButton(Loc.T("Keep my shop"), Palette.Gold, 16, () =>
        {
            _confirm.QueueFree();
            _confirm = null;
        }).Place(660, 392, 200, 48));
    }

    /// <summary>Everything Bram's kin have found, the curios they've seen and the Heartsongs they've heard, in every run.</summary>
    CreditsView _credits;

    public CreditsView OpenCredits()
    {
        if (_credits != null) return _credits;
        _credits = Add(new CreditsView());
        _credits.Closed += () => _credits = null;
        return _credits;
    }

    public CollectionView OpenCollection(int page = CollectionView.FindsPage)
    {
        if (_collection != null) return _collection;
        _collection = Add(new CollectionView(new Collected(Game.Profile), page));
        _collection.Closed += () => _collection = null;
        return _collection;
    }

    void OpenSettings()
    {
        if (_settings != null) return;
        _settings = Add(new SettingsView());
        _settings.ZIndex = 30;
        _settings.Closed += () => _settings = null;
    }
}
