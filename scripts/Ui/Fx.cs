using Godot;

namespace Keywright.Ui;

/// <summary>
/// Particle presets and pixel effects for the "juice". Since 0.32 they keep to the 3px art's grain and light: particles
/// are crisp squares one or two cells across (a cell is 1.5 of the layout's pixels), dwindling as they go; light is soft
/// (an additive radial glow, not stacked discs), and rings and rays are thin. The sprite sheets (assets/sprites/fx: the
/// spinning coin, a sparkle, a puff of dust) are rows of frames at their art size, shown at 2x or more; the burst is
/// drawn here (0.31's generated burst sheet read as a splat of blobs over the finer art).
/// </summary>
public static class Fx
{
    const int CoinFrames = 6, SparkleFrames = 4, DustFrames = 5;

    /// <summary>One cell of the 3px art, in layout pixels: the grain particles keep to.</summary>
    public const float Cell = 1.5f;

    static Texture2D _pixel, _coin, _sparkle, _glow;
    static SpriteFrames _dust;
    static Curve _dwindle;

    /// <summary>A hard 2x2 square: at a particle scale of 0.75 it's one cell of the art, at 1.5 two.</summary>
    public static Texture2D Pixel => _pixel ??= MakeSquare(2);

    /// <summary>A gold coin spinning: <see cref="CoinFrames"/> frames in a row (a sprite's Hframes, or particles').</summary>
    public static Texture2D Coin => _coin ??= ArtTexture.Load("res://assets/sprites/fx/coin_spin.png");

    static Texture2D Sparkle => _sparkle ??= ArtTexture.Load("res://assets/sprites/fx/sparkle.png");

    /// <summary>
    /// A soft round light, white at the middle and nothing at the edge (64 across): drawn additive and tinted, it's how
    /// light looks in the scene, with no rings of stacked discs.
    /// </summary>
    public static Texture2D Glow => _glow ??= new GradientTexture2D
    {
        Width = 64,
        Height = 64,
        Fill = GradientTexture2D.FillEnum.Radial,
        FillFrom = new Vector2(0.5f, 0.5f),
        FillTo = new Vector2(1f, 0.5f),
        Gradient = new Gradient
        {
            Offsets = new[] { 0f, 0.3f, 0.65f, 1f },
            Colors = new[] { Colors.White, new Color(1, 1, 1, 0.55f), new Color(1, 1, 1, 0.14f), new Color(1, 1, 1, 0) },
        },
    };

    /// <summary>Particles shrink to a quarter of their size over their life, so a burst thins out instead of popping off.</summary>
    static Curve Dwindle
    {
        get
        {
            if (_dwindle != null) return _dwindle;
            _dwindle = new Curve();
            _dwindle.AddPoint(new Vector2(0, 1));
            _dwindle.AddPoint(new Vector2(0.6f, 0.75f));
            _dwindle.AddPoint(new Vector2(1, 0.25f));
            return _dwindle;
        }
    }

    /// <summary>Particles that play a row of frames (the coin's spin, a sparkle's twinkle) over their life.</summary>
    static CanvasItemMaterial Animated(int frames, bool additive) => new()
    {
        BlendMode = additive ? CanvasItemMaterial.BlendModeEnum.Add : CanvasItemMaterial.BlendModeEnum.Mix,
        ParticlesAnimation = true,
        ParticlesAnimHFrames = frames,
        ParticlesAnimVFrames = 1,
        ParticlesAnimLoop = true,
    };

    /// <summary>A coin that spins as it goes (two turns a second), at 2x.</summary>
    public static Sprite2D SpinningCoin(Vector2 at)
    {
        var coin = new Sprite2D { Texture = Coin, Hframes = CoinFrames, Scale = Vector2.One * 2, Position = at };
        coin.CreateTween().SetLoops().TweenProperty(coin, "frame", CoinFrames - 1, 0.45f).From(0);
        return coin;
    }

    public static CanvasItemMaterial Additive() => new() { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };

    /// <summary>
    /// Sparks: one- and two-cell squares, white-hot at first, then the colour, dwindling and fading as they fall.
    /// </summary>
    public static CpuParticles2D Sparks(Color color, int amount, float speedMin, float speedMax,
        float spread = 70f, float lifetime = 1.1f, float gravity = 600f)
    {
        var ramp = new Gradient();
        ramp.Offsets = new[] { 0f, 0.15f, 0.6f, 1f };
        ramp.Colors = new[] { color.Lightened(0.8f), color.Lightened(0.35f), color, color with { A = 0f } };
        return new CpuParticles2D
        {
            Emitting = false,
            OneShot = true,
            Amount = amount,
            Lifetime = lifetime,
            Explosiveness = 0.95f,
            Texture = Pixel,
            Material = Additive(),
            Direction = Vector2.Up,
            Spread = spread,
            InitialVelocityMin = speedMin,
            InitialVelocityMax = speedMax,
            Gravity = new Vector2(0, gravity),
            DampingMin = 30,
            DampingMax = 110,
            ScaleAmountMin = 0.75f,
            ScaleAmountMax = 1.5f,
            ScaleAmountCurve = Dwindle,
            ColorRamp = ramp,
        };
    }

    /// <summary>A fountain of spinning gold coins.</summary>
    public static CpuParticles2D Coins(int amount) => new()
    {
        Emitting = false,
        OneShot = true,
        Amount = amount,
        Lifetime = 1.8f,
        Explosiveness = 0.9f,
        Texture = Coin,
        Material = Animated(CoinFrames, additive: false),
        AnimSpeedMin = 3,
        AnimSpeedMax = 5,
        AnimOffsetMax = 1,
        Direction = Vector2.Up,
        Spread = 45,
        InitialVelocityMin = 420,
        InitialVelocityMax = 780,
        Gravity = new Vector2(0, 1400),
        ScaleAmountMin = 2f,
        ScaleAmountMax = 2f,
    };

    /// <summary>A one-shot burst of sparks, and a few sparkles twinkling among them, that frees itself.</summary>
    public static void Burst(Node parent, Vector2 position, Color color, int amount = 40, float speed = 260f)
    {
        var particles = Sparks(color, amount, speed * 0.35f, speed, spread: 180f, lifetime: 0.8f, gravity: 200f);
        particles.Position = position;
        parent.AddChild(particles);
        particles.Emitting = true;
        var twinkles = Sparks(color, Mathf.Max(3, amount / 8), speed * 0.2f, speed * 0.6f, spread: 180f, lifetime: 0.9f, gravity: 60f);
        twinkles.Texture = Sparkle;
        twinkles.Material = Animated(SparkleFrames, additive: true);
        twinkles.AnimSpeedMin = twinkles.AnimSpeedMax = 1;
        twinkles.ScaleAmountMin = twinkles.ScaleAmountMax = 1.5f;
        twinkles.ScaleAmountCurve = null;
        twinkles.Position = position;
        parent.AddChild(twinkles);
        twinkles.Emitting = true;
        parent.GetTree().CreateTimer(1.2).Timeout += particles.QueueFree;
        parent.GetTree().CreateTimer(1.2).Timeout += twinkles.QueueFree;
    }

    /// <summary>A puff of dust where something lands (the dust cloud, and a scatter of grit), freed when done.</summary>
    public static void Dust(Node parent, Vector2 position, float strength = 1f)
    {
        var cloud = new AnimatedSprite2D
        {
            SpriteFrames = DustPuff(),
            Position = position + new Vector2(0, -8 * (strength >= 1.5f ? 3 : 2)),
            Scale = Vector2.One * (strength >= 1.5f ? 3 : 2),
            Modulate = new Color("#c9b79a") with { A = 0.7f },
        };
        parent.AddChild(cloud);
        cloud.AnimationFinished += cloud.QueueFree;
        cloud.Play("puff");

        var ramp = new Gradient();
        ramp.Offsets = new[] { 0f, 1f };
        ramp.Colors = new[] { new Color("#d2b688") with { A = 0.85f }, new Color("#9d8872") with { A = 0f } };
        var dust = new CpuParticles2D
        {
            Emitting = false,
            OneShot = true,
            Amount = (int)(12 * strength) + 4,
            Lifetime = 0.6f,
            Explosiveness = 1f,
            Texture = Pixel,
            Direction = Vector2.Up,
            Spread = 80,
            InitialVelocityMin = 60 * strength,
            InitialVelocityMax = 190 * strength,
            Gravity = new Vector2(0, 300),
            DampingMin = 120,
            DampingMax = 240,
            ScaleAmountMin = 0.75f,
            ScaleAmountMax = 1.5f,
            ColorRamp = ramp,
            Position = position,
        };
        parent.AddChild(dust);
        dust.Emitting = true;
        parent.GetTree().CreateTimer(1.0).Timeout += dust.QueueFree;
    }

    /// <summary>
    /// The burst when a lid flies off: a flare of soft light that swells and fades, a thin ring running out from it, a
    /// star of fine rays and a spray of grains, all in the tint. <paramref name="scale"/> is how big (3: a chest's).
    /// </summary>
    public static void Explosion(Node parent, Vector2 position, int scale, Color tint)
    {
        float size = 40f * scale;
        Flare(parent, position, tint, size, 0.55f);
        Ring(parent, position, tint.Lightened(0.3f), size * 1.6f, 0.5f, 3f);
        Star(parent, position, tint.Lightened(0.5f), 8, size * 1.3f, 0.4f);
        Burst(parent, position, tint, 18 * scale, 110f * scale);
    }

    /// <summary>Soft light swelling at a spot and fading: a lamp turned up and down. Returns it.</summary>
    public static Sprite2D Flare(Node parent, Vector2 at, Color color, float radius = 80f, float duration = 0.5f, float alpha = 0.9f)
    {
        var flare = new Sprite2D
        {
            Texture = Glow,
            Position = at,
            Material = Additive(),
            Modulate = color with { A = alpha },
            Scale = Vector2.One * (radius * 0.6f / 32f),
        };
        parent.AddChild(flare);
        var tween = flare.CreateTween().SetParallel();
        tween.TweenProperty(flare, "scale", Vector2.One * (radius / 32f), duration * 0.35f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(flare, "modulate:a", 0f, duration).SetEase(Tween.EaseType.In);
        tween.Chain().TweenCallback(Callable.From(flare.QueueFree));
        return flare;
    }

    /// <summary>A star of fine rays shooting out from a spot and fading, longer and shorter by turns: a glint.</summary>
    public static void Star(Node parent, Vector2 at, Color color, int rays = 8, float length = 120f, float duration = 0.4f)
    {
        float t = 0;
        var star = new Painter(p =>
        {
            float reach = length * (0.35f + 0.65f * Mathf.Ease(t, 0.4f));
            for (int i = 0; i < rays; i++)
            {
                float a = Mathf.Tau * i / rays + Mathf.Pi / rays * 0.5f;
                float l = i % 2 == 0 ? reach : reach * 0.55f;
                var dir = Vector2.FromAngle(a);
                var side = dir.Orthogonal() * Cell;
                p.DrawPolygon(new[] { side, dir * l, -side }, new[] { color, color with { A = 0 }, color });
            }
        }) { Position = at, Material = Additive(), Rotation = 0.2f };
        parent.AddChild(star);
        var tween = star.CreateTween().SetParallel();
        tween.TweenMethod(Callable.From<float>(v =>
        {
            t = v;
            star.QueueRedraw();
        }), 0f, 1f, duration);
        tween.TweenProperty(star, "modulate:a", 0f, duration).SetEase(Tween.EaseType.In);
        tween.Chain().TweenCallback(Callable.From(star.QueueFree));
    }

    static SpriteFrames DustPuff() => _dust ??= Frames("res://assets/sprites/fx/dust.png", DustFrames, "puff", 12);

    /// <summary>A row of equal frames as a one-shot animation.</summary>
    static SpriteFrames Frames(string path, int count, string name, int fps)
    {
        var sheet = ArtTexture.Load(path);
        int w = sheet.GetWidth() / count, h = sheet.GetHeight();
        var frames = new SpriteFrames();
        frames.RemoveAnimation("default");
        frames.AddAnimation(name);
        frames.SetAnimationLoopMode(name, SpriteFrames.LoopMode.None);
        frames.SetAnimationSpeed(name, fps);
        for (int i = 0; i < count; i++)
            frames.AddFrame(name, new AtlasTexture { Atlas = sheet, Region = new Rect2(i * w, 0, w, h) });
        return frames;
    }

    /// <summary>
    /// A beam of light standing on a spot (a rare find), fading out: a bright core a cell or two wide inside a softer
    /// shaft, both fading to nothing toward the top.
    /// </summary>
    public static void Pillar(Node parent, Vector2 at, Color color, float width = 40f, float height = 460f, float duration = 1.0f)
    {
        var beam = new Painter(p =>
        {
            void Band(float w, float a, Color c) =>
                p.DrawPolygon(new[] { new Vector2(-w / 2, 0), new Vector2(w / 2, 0), new Vector2(w / 2, -height), new Vector2(-w / 2, -height) },
                    new[] { c with { A = a }, c with { A = a }, c with { A = 0 }, c with { A = 0 } });
            Band(width, 0.14f, color);
            Band(width * 0.5f, 0.22f, color);
            Band(Mathf.Max(2 * Cell, width * 0.12f), 0.55f, color.Lightened(0.6f));
        }) { Position = at, Material = Additive(), Scale = new Vector2(0.2f, 1f) };
        parent.AddChild(beam);
        var tween = beam.CreateTween();
        tween.TweenProperty(beam, "scale:x", 1f, 0.14f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(beam, "modulate:a", 0f, duration).SetEase(Tween.EaseType.In);
        tween.TweenCallback(Callable.From(beam.QueueFree));
    }

    /// <summary>A thin ring of light running out from a spot and fading: a bright line in a soft band of its light.</summary>
    public static void Ring(Node parent, Vector2 at, Color color, float radius = 160f, float duration = 0.45f, float width = 3f)
    {
        float r = 8f, w = width;
        var ring = new Painter(p =>
        {
            p.DrawArc(Vector2.Zero, r, 0, Mathf.Tau, 72, color with { A = color.A * 0.22f }, w * 4);
            p.DrawArc(Vector2.Zero, r, 0, Mathf.Tau, 72, color, w);
        }) { Position = at, Material = Additive() };
        parent.AddChild(ring);
        var tween = ring.CreateTween().SetParallel();
        tween.TweenMethod(Callable.From<float>(v =>
        {
            r = v;
            // It thins as it spreads.
            w = Mathf.Max(Cell, width * (1f - 0.6f * (v - 8f) / Mathf.Max(1f, radius - 8f)));
            ring.QueueRedraw();
        }), 8f, radius, duration).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(ring, "modulate:a", 0f, duration).SetEase(Tween.EaseType.In);
        tween.Chain().TweenCallback(Callable.From(ring.QueueFree));
    }

    /// <summary>
    /// Fine bolts crackling out of a spot (an epic find): thin, jagged on the art's grain, a white core in a coloured
    /// edge, flickering for a moment; and a soft glow where they start.
    /// </summary>
    public static void Lightning(Node parent, Vector2 at, Color color, int bolts = 5, float reach = 110f, float duration = 0.45f)
    {
        var rng = new System.Random();
        Flare(parent, at, color, reach * 0.8f, duration, 0.6f);
        var bolt = new Painter(p =>
        {
            for (int b = 0; b < bolts; b++)
            {
                float angle = Mathf.Tau * b / bolts + (float)rng.NextDouble() * 0.7f;
                var dir = Vector2.FromAngle(angle);
                var points = new Vector2[6];
                for (int i = 0; i < points.Length; i++)
                {
                    float t = i / (float)(points.Length - 1);
                    var point = dir * reach * t * (0.75f + 0.25f * (float)rng.NextDouble())
                        + dir.Orthogonal() * ((float)rng.NextDouble() * 2 - 1) * 12f * t;
                    // On the art's grain, so a bolt steps like the pixels round it.
                    points[i] = (point / Cell).Round() * Cell;
                }
                p.DrawPolyline(points, color.Lightened(0.3f) with { A = 0.6f }, 2 * Cell);
                p.DrawPolyline(points, Colors.White, Cell);
            }
        }) { Position = at, Material = Additive() };
        parent.AddChild(bolt);
        var tween = bolt.CreateTween();
        tween.TweenMethod(Callable.From<float>(_ => bolt.QueueRedraw()), 0f, 1f, duration);
        tween.Parallel().TweenProperty(bolt, "modulate:a", 0f, duration).SetEase(Tween.EaseType.In);
        tween.TweenCallback(Callable.From(bolt.QueueFree));
    }

    /// <summary>A soft glow that stays behind something and breathes. Returns it, so it can be removed.</summary>
    public static Node2D Aura(Node parent, Vector2 at, Color color, float radius = 70f)
    {
        var aura = new Sprite2D
        {
            Texture = Glow,
            Position = at,
            Material = Additive(),
            Modulate = color with { A = 0.45f },
            Scale = Vector2.One * (radius / 32f),
            ZIndex = 4,
        };
        parent.AddChild(aura);
        var pulse = aura.CreateTween().SetLoops();
        pulse.TweenProperty(aura, "scale", Vector2.One * (radius * 1.12f / 32f), 0.8f).SetTrans(Tween.TransitionType.Sine);
        pulse.TweenProperty(aura, "scale", Vector2.One * (radius * 0.92f / 32f), 0.8f).SetTrans(Tween.TransitionType.Sine);
        return aura;
    }

    /// <summary>
    /// Slowly turning rays of light behind something (a legendary find): fine, long and short by turns, over a soft
    /// glow. Returns them.
    /// </summary>
    public static Node2D Rays(Node parent, Vector2 at, Color color, float radius = 150f, int count = 12, float alpha = 0.35f)
    {
        var rays = new Painter(p =>
        {
            p.DrawTextureRect(Glow, new Rect2(-radius * 0.6f, -radius * 0.6f, radius * 1.2f, radius * 1.2f), false, color with { A = alpha * 0.8f });
            for (int i = 0; i < count; i++)
            {
                float a = i * Mathf.Tau / count;
                float l = i % 2 == 0 ? radius : radius * 0.62f;
                float half = i % 2 == 0 ? 0.05f : 0.035f;
                p.DrawPolygon(new[] { Vector2.Zero, Vector2.FromAngle(a - half) * l, Vector2.FromAngle(a + half) * l },
                    new[] { color with { A = alpha }, color with { A = 0 }, color with { A = 0 } });
            }
        }) { Position = at, Material = Additive(), ZIndex = 3 };
        parent.AddChild(rays);
        rays.CreateTween().SetLoops().TweenProperty(rays, "rotation", Mathf.Tau, 12f).AsRelative();
        return rays;
    }

    /// <summary>
    /// A shower of many-coloured confetti (a find two or more qualities better than its chest): small flakes, two cells
    /// by one, tumbling as they fall.
    /// </summary>
    public static void Confetti(Node parent, Vector2 at, int amount = 90)
    {
        var colours = new Gradient();
        colours.Offsets = new[] { 0f, 0.25f, 0.5f, 0.75f, 1f };
        colours.Colors = new[] { new Color("#ff6b6b"), new Color("#ffb52e"), new Color("#6fdc8c"), new Color("#4ea3ff"), new Color("#c792ff") };
        var fade = new Gradient();
        fade.Offsets = new[] { 0f, 0.8f, 1f };
        fade.Colors = new[] { Colors.White, Colors.White, new Color(1, 1, 1, 0) };
        var confetti = new CpuParticles2D
        {
            Emitting = false,
            OneShot = true,
            Amount = amount,
            Lifetime = 2.2f,
            Explosiveness = 0.9f,
            Texture = Flake,
            Direction = Vector2.Up,
            Spread = 70,
            InitialVelocityMin = 340,
            InitialVelocityMax = 720,
            Gravity = new Vector2(0, 640),
            DampingMin = 70,
            DampingMax = 150,
            AngularVelocityMin = -300,
            AngularVelocityMax = 300,
            AngleMin = 0,
            AngleMax = 180,
            ScaleAmountMin = 0.75f,
            ScaleAmountMax = 1.25f,
            ColorInitialRamp = colours,
            ColorRamp = fade,
            Position = at,
        };
        parent.AddChild(confetti);
        confetti.Emitting = true;
        parent.GetTree().CreateTimer(2.6).Timeout += confetti.QueueFree;
    }

    static Texture2D _flake;

    /// <summary>A confetti flake: two cells by one, at a particle scale of 1.</summary>
    static Texture2D Flake => _flake ??= MakeRect(3, 2);

    static Texture2D MakeSquare(int size) => MakeRect(size, size);

    static Texture2D MakeRect(int w, int h)
    {
        var image = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
        image.Fill(Colors.White);
        return ImageTexture.CreateFromImage(image);
    }
}
