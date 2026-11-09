using System;
using Godot;

namespace Keywright.Ui;

/// <summary>
/// The chest in its finish (lid, body and the open box), its lock maker's hasp and plate over the seam, and the light
/// that leaks from it. The node's origin is the centre of the lid seam. Two layers (0.35): the lid, as it always is, and
/// under it the box: shut, or (once the lid lifts at all) the open box, its outside and its inside in perspective, so
/// whatever shows between lid and box is the box's own inside, lit from within in the chest's colour, never a gap. Since 0.32 the pictures are the 3px art's grain
/// (tools/custom_art.py's chest3: 128 cells wide, each 3x3 pixels at 1440p), loaded through <see cref="ArtTexture"/>,
/// which reports the layout size they had at round 2's grain (96 wide, shown at 2x), so nothing that places the chest
/// moved.
/// </summary>
public partial class ChestView : Node2D
{
    const int ArtWidth = 96, LidRows = 42, BodyRows = 30, OpenRimRows = 30;

    /// <summary>The open box's picture in cells of the 3px art: 128 wide, its top 40 the rims and the inside.</summary>
    const float ArtCells = 128, InsideCells = 40;

    /// <summary>Where the light inside settles once the chest is open and its burst is over (the shafts are gone by then).</summary>
    public const float RestGlow = 0.3f;

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
    Painter _shadow, _backGlow, _innerLight, _seamLight;
    float _time;
    float _wobble;
    bool _lidDetached, _open, _inside;
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
        _innerLight = new Painter(DrawInnerLight) { Material = Fx.Additive() };
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
        _rig.AddChild(_innerLight);
        _rig.AddChild(_lid);
        _rig.AddChild(_seamLight);
        PlaceLid();
    }

    Texture2D Load(string part, string skin = null) => ArtTexture.Load($"res://assets/sprites/chest/{skin ?? Skin}_{part}.png");

    void FitLock()
    {
        string maker = Maker(_lock);
        _lidLock.Texture = maker == null ? null : Load("lid", $"lock_{maker}");
        _bodyLock.Texture = maker == null || _open ? null : Load("body", $"lock_{maker}");
    }

    /// <summary>A mimic's breath (0.33): the lid lifts a hair and settles, as something inside breathes in and out.</summary>
    public void Breathe()
    {
        if (_lidDetached) return;
        var tween = CreateTween();
        tween.TweenMethod(Callable.From<float>(g => LidGap = g), 0f, 5f, 0.8f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.TweenMethod(Callable.From<float>(g => LidGap = g), 5f, 0f, 1.1f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
    }

    /// <summary>A quick side-to-side rattle, e.g. when shaken.</summary>
    public void Wobble(float amount) => _wobble = Mathf.Max(_wobble, amount);

    /// <summary>Hands the lid over to the caller (to fling it off) and stops positioning it.</summary>
    public Node2D DetachLid()
    {
        _lidDetached = true;
        return _lid;
    }

    /// <summary>The lid's gone: the open box for good, its rims and its inside, and no lock plate.</summary>
    public void ShowOpen()
    {
        _open = true;
        ShowInside(true);
        _bodyLock.Texture = null;
    }

    /// <summary>
    /// The box under the lid: shut, or open (its inside showing wherever the lid doesn't cover it). The lock's plate stays
    /// where it was on the box's front: the open picture has the rims and the inside above it.
    /// </summary>
    void ShowInside(bool inside)
    {
        if (_inside == inside) return;
        _inside = inside;
        _body.Texture = Load(inside ? "open" : "body");
        _body.Position = new Vector2(-Width / 2, inside ? -OpenRimRows * PixelSize : 0);
        _bodyLock.Position = new Vector2(0, inside ? OpenRimRows : 0);
        _innerLight.QueueRedraw();
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
        // The moment the lid lifts (a strain, a mimic's breath, the Maw), what shows under it is the box's own inside.
        if (!_open) ShowInside(LidGap > 0.25f);

        _backGlow.QueueRedraw();
        _innerLight.QueueRedraw();
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
        // A soft light swelling behind the chest as the lid strains (one smooth glow: stacked discs showed their rings).
        var center = new Vector2(0, -Unit * 2);
        float r = Unit * (6 + 14 * Glow);
        p.DrawTextureRect(Fx.Glow, new Rect2(center.X - r, center.Y - r, 2 * r, 2 * r), false, GlowColor with { A = 0.55f * Glow });
    }

    /// <summary>A point of the open box's picture, by its cell (x across 128, y down its top 40), in the chest's own space.</summary>
    Vector2 InsideAt(float cx, float cy) =>
        new(-Width / 2 + cx * Width / ArtCells, -OpenRimRows * PixelSize + cy * OpenRimRows * PixelSize / InsideCells);

    /// <summary>
    /// The light inside the box (0.35), in the chest's own colour: added over the inside, never covering it, so the floor
    /// and walls still show through. Brightest on the floor, where the things are, and a pool of it in the middle. Under
    /// the lid, so while the lid strains it's what fills the crack.
    /// </summary>
    void DrawInnerLight(Painter p)
    {
        if (!_inside || Glow <= 0.01f) return;
        var c = GlowColor;
        float far = 0.16f * Glow, near = 0.4f * Glow;
        p.DrawPolygon(new[] { InsideAt(9, 12), InsideAt(118, 12), InsideAt(122, 37), InsideAt(5, 37) },
            new[] { c with { A = far }, c with { A = far }, c with { A = near }, c with { A = near } });
        var pool = InsideAt(64, 30);
        float rx = Width * 0.4f, ry = OpenRimRows * PixelSize * 0.3f;
        p.DrawTextureRect(Fx.Glow, new Rect2(pool.X - rx, pool.Y - ry, 2 * rx, 2 * ry), false, c with { A = 0.45f * Glow });
    }

    /// <summary>
    /// Shafts of light out of the box, in its colour: out of the crack while the lid strains, up out of the open box at the
    /// burst, fading as its light settles (0.35: no bright line along the seam or the rim, which read as a bar of white
    /// between two halves, and stayed on the open box after).
    /// </summary>
    void DrawSeamLight(Painter p)
    {
        float strength = _open ? Mathf.Clamp((Glow - RestGlow) / (1 - RestGlow), 0, 1) : Glow;
        if (strength <= 0.01f) return;
        var c = GlowColor.Lightened(0.25f);
        if (!_open)
        {
            // While the lid's on, the light only spills out of the crack: a soft wash over the lid's foot and the box's top.
            float spillW = Width * 0.62f, spillH = Unit * 1.2f + LidGap;
            p.DrawTextureRect(Fx.Glow, new Rect2(-spillW, -LidGap * 0.5f - spillH, 2 * spillW, 2 * spillH), false, c with { A = 0.32f * strength });
            return;
        }
        float y = InsideAt(64, 14).Y;
        float half = Width / 2 - Unit;

        const int shafts = 7;
        for (int i = 0; i < shafts; i++)
        {
            float x = -half * 0.85f + i * (half * 1.7f / (shafts - 1));
            float flicker = 0.6f + 0.4f * MathF.Sin(_time * 9 + i * 1.7f);
            float h = Unit * (2 + 9 * strength * flicker);
            float w = Unit * 0.5f;
            var shaft = new[]
            {
                new Vector2(x - w, y), new Vector2(x + w, y),
                new Vector2(x + w * 2.5f, y - h), new Vector2(x - w * 2.5f, y - h),
            };
            var colors = new[] { c with { A = 0.22f * strength }, c with { A = 0.22f * strength }, c with { A = 0f }, c with { A = 0f } };
            p.DrawPolygon(shaft, colors);
        }
    }
}
