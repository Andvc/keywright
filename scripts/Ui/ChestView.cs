using System;
using Godot;

namespace Keywright.Ui;

/// <summary>
/// The chest in its finish (lid, body and the open box: drawn 48 pixels wide by tools/custom_art.py and built into
/// assets/ at 2x by tools/import_art.py, so its pictures are 96 wide), its lock maker's hasp and plate over the seam,
/// and the light that leaks from it. The node's origin is the centre of the lid seam.
/// </summary>
public partial class ChestView : Node2D
{
    const int ArtWidth = 96, LidRows = 42, BodyRows = 30, OpenRimRows = 30;

    static readonly string[] Makers = { null, "rusty", "guild", "dwarven" };

    /// <summary>
    /// A lock's maker, by <see cref="Core.LockTier.Index"/>, for its pictures (the chest's lock_{maker}_* overlays, the lock's
    /// face when it's picked), or null for no lock.
    /// </summary>
    public static string Maker(int lockIndex) => lockIndex > 0 && lockIndex < Makers.Length ? Makers[lockIndex] : null;

    /// <summary>Screen pixels per art pixel. Set before adding to the tree.</summary>
    public int PixelSize = 2;

    /// <summary>Sprite set under res://assets/sprites/chest/ (wood, green, blue, purple, gold, black, crown).</summary>
    public string Skin = "wood";

    /// <summary>Its lock, by maker (<see cref="Core.LockTier.Index"/>); 0 leaves the seat empty, as on a chest already opened.</summary>
    public int Lock
    {
        get => _lock;
        set
        {
            _lock = value;
            if (_lid != null) FitLock();
        }
    }

    /// <summary>0..1 continuous jitter while the lid strains.</summary>
    public float Rumble;

    /// <summary>How far (screen px) the lid is lifted off the body.</summary>
    public float LidGap;

    /// <summary>0..1 intensity of the light leaking through the seam.</summary>
    public float Glow;

    public Color GlowColor = Colors.White;

    public float Width => ArtWidth * PixelSize;
    public float Top => -LidRows * PixelSize;
    public float Bottom => BodyRows * PixelSize;

    /// <summary>The glow and its shafts are measured in sixteenths of the chest's width, whatever its art.</summary>
    float Unit => Width / 16f;

    readonly Random _rng = new();
    Node2D _rig;
    Sprite2D _lid, _body, _lidLock, _bodyLock;
    Painter _shadow, _backGlow, _seamLight;
    float _time;
    float _wobble;
    bool _lidDetached, _open;
    int _lock;

    public override void _Ready()
    {
        _rig = new Node2D();
        AddChild(_rig);
        float half = Width / 2;

        _shadow = new Painter(DrawShadow);
        _backGlow = new Painter(DrawBackGlow) { Material = Fx.Additive() };
        _body = new Sprite2D
        {
            Texture = Load("body"),
            Centered = false,
            Position = new Vector2(-half, 0),
            Scale = Vector2.One * PixelSize,
        };
        _lid = new Sprite2D
        {
            Texture = Load("lid"),
            Scale = Vector2.One * PixelSize,
        };
        _seamLight = new Painter(DrawSeamLight) { Material = Fx.Additive() };

        // The lock's hasp and plate: the size of the parts they sit on, so they lie on them exactly.
        _lidLock = new Sprite2D();
        _bodyLock = new Sprite2D { Centered = false };
        _lid.AddChild(_lidLock);
        _body.AddChild(_bodyLock);
        FitLock();

        _rig.AddChild(_shadow);
        _rig.AddChild(_backGlow);
        _rig.AddChild(_body);
        _rig.AddChild(_lid);
        _rig.AddChild(_seamLight);
        PlaceLid();
    }

    Texture2D Load(string part, string skin = null) => GD.Load<Texture2D>($"res://assets/sprites/chest/{skin ?? Skin}_{part}.png");

    void FitLock()
    {
        string maker = Maker(_lock);
        _lidLock.Texture = maker == null ? null : Load("lid", $"lock_{maker}");
        _bodyLock.Texture = maker == null || _open ? null : Load("body", $"lock_{maker}");
    }

    /// <summary>A quick side-to-side rattle, e.g. when shaken.</summary>
    public void Wobble(float amount) => _wobble = Mathf.Max(_wobble, amount);

    /// <summary>Hands the lid over to the caller (to fling it off) and stops positioning it.</summary>
    public Node2D DetachLid()
    {
        _lidDetached = true;
        return _lid;
    }

    /// <summary>Swaps the body for the open box: the lid gone, its rim and the dark inside.</summary>
    public void ShowOpen()
    {
        _open = true;
        _body.Texture = Load("open");
        _body.Position = new Vector2(-Width / 2, -OpenRimRows * PixelSize);
        _bodyLock.Texture = null;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _time += dt;
        _wobble = Mathf.Max(0, _wobble - dt * 1.6f);

        float jitter = Rumble * Rumble * 5f;
        _rig.Position = new Vector2(Rand() * jitter, Rand() * jitter * 0.5f);
        _rig.Rotation = MathF.Sin(_time * 38f) * 0.08f * _wobble + Rand() * 0.012f * Rumble;
        if (!_lidDetached) PlaceLid();

        _backGlow.QueueRedraw();
        _seamLight.QueueRedraw();
    }

    void PlaceLid() => _lid.Position = new Vector2(0, -LidRows * PixelSize / 2f - LidGap);

    float Rand() => (float)(_rng.NextDouble() * 2 - 1);

    void DrawShadow(Painter p)
    {
        float y = Bottom + Unit * 0.5f;
        float w = Width * 0.5f;
        // Stepped oval: flat rectangles read as a pixel-art shadow.
        p.DrawRect(new Rect2(-w, y - Unit * 0.5f, w * 2, Unit), new Color(0, 0, 0, 0.35f));
        p.DrawRect(new Rect2(-w * 0.85f, y - Unit, w * 1.7f, Unit * 2), new Color(0, 0, 0, 0.2f));
    }

    void DrawBackGlow(Painter p)
    {
        if (Glow <= 0.01f) return;
        var center = new Vector2(0, -Unit * 2);
        for (int i = 6; i >= 1; i--)
        {
            float r = Unit * (3 + i * 2.6f * (0.6f + Glow * 0.7f));
            p.DrawCircle(center, r, GlowColor with { A = 0.05f * Glow });
        }
        // Bright band behind the seam: visible only through the gap as the lid lifts.
        if (!_open)
            p.DrawRect(new Rect2(-Width / 2 + Unit, -LidGap - 1, Width - 2 * Unit, LidGap + 2), GlowColor.Lightened(0.6f) with { A = Glow });
    }

    void DrawSeamLight(Painter p)
    {
        if (Glow <= 0.01f) return;
        var c = GlowColor.Lightened(0.4f);
        float y = _open ? -OpenRimRows * PixelSize + Unit : -LidGap * 0.5f;
        float half = Width / 2 - Unit;
        p.DrawRect(new Rect2(-half, y - Unit / 4f, half * 2, Unit / 2f), c with { A = 0.6f * Glow });

        const int shafts = 7;
        for (int i = 0; i < shafts; i++)
        {
            float x = -half * 0.85f + i * (half * 1.7f / (shafts - 1));
            float flicker = 0.6f + 0.4f * MathF.Sin(_time * 9 + i * 1.7f);
            float h = Unit * (2 + 9 * Glow * flicker);
            float w = Unit * 0.5f;
            var shaft = new[]
            {
                new Vector2(x - w, y), new Vector2(x + w, y),
                new Vector2(x + w * 2.5f, y - h), new Vector2(x - w * 2.5f, y - h),
            };
            var colors = new[] { c with { A = 0.25f * Glow }, c with { A = 0.25f * Glow }, c with { A = 0f }, c with { A = 0f } };
            p.DrawPolygon(shaft, colors);
        }
    }
}
