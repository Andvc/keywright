using System;
using System.Threading.Tasks;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// A Heartsong coming out of a chest, as the rarest thing in Aldmark should: the room goes dark, the song plays, a pillar
/// of red light stands on the chest and the thing rises out of it into turning rays, then a hit of white, shockwaves and
/// a shower of sparks as the banner slams in. What it is, who gave it and what it does on display stay up, with Keep
/// and Decline, until you choose: kept, it flies up to sing in the shop (for nothing: it's worth nothing in gold);
/// declined, it comes apart into red motes and goes back to the Locking. Used for an opening and for a chest that bursts
/// as you pass on it.
/// </summary>
public partial class SongReveal : Node2D
{
    static readonly Vector2 Hover = new(640, 236);
    const float PanelX = 250, PanelY = 352, PanelW = 780, PanelH = 330;

    readonly Item _song;
    readonly Vector2 _from;
    readonly string _intro;
    readonly TaskCompletionSource<bool> _chosen = new();
    Sprite2D _sprite;
    Button _keep, _decline;
    ColorRect _dark;
    Node2D _glow;
    double _sinceSong;
    bool _open, _done;

    public SongReveal() { }

    /// <param name="from">Where it comes out (the chest's seam).</param>
    /// <param name="intro">What's said as it starts ("...listen.", "The chest bursts open!").</param>
    public SongReveal(Item song, Vector2 from, string intro = "...listen.")
    {
        _song = song;
        _from = from;
        _intro = intro;
    }

    static Audio.Sfx Sfx => Main.Instance.Sfx;
    static Main Game => Main.Instance;

    /// <summary>Keep and Decline are up (the bot waits for this).</summary>
    public bool Open => _open && !_done;

    /// <summary>The choice: true to keep it.</summary>
    public Task<bool> Chosen => _chosen.Task;

    public override void _Ready()
    {
        ZIndex = 60;
        _dark = new ColorRect { Color = new Color(0.02f, 0.0f, 0.03f, 0f), MouseFilter = Control.MouseFilterEnum.Stop }.Place(-100, -100, 1480, 920);
        AddChild(_dark);
        _ = Play();
    }

    bool Alive => IsInsideTree() && !IsQueuedForDeletion();

    async Task Wait(double seconds)
    {
        if (!Alive) return;
        await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
    }

    async Task Play()
    {
        var red = Palette.Of(Rarity.Mythic);
        var gold = Palette.Gold;
        var def = _song.SongDef;

        // The room goes dark, a red glow gathers on the chest, and the song begins.
        _dark.CreateTween().TweenProperty(_dark, "color:a", 0.88f, 0.6f);
        var vignette = new Painter(p =>
        {
            for (int i = 6; i >= 1; i--) p.DrawCircle(Vector2.Zero, 120f * i, red with { A = 0.035f });
        }) { Position = Hover, Material = Fx.Additive(), Modulate = Colors.Transparent };
        AddChild(vignette);
        vignette.CreateTween().TweenProperty(vignette, "modulate", Colors.White, 1.2f);
        Say(_intro, _from + new Vector2(0, -130), red, 32);
        Sfx.Play($"song_{_song.Song}", 1f, 0);
        _sinceSong = 0;
        var gather = Fx.Aura(this, _from, red, 90f);
        gather.ZIndex = 1;
        await Wait(1.2);
        if (!Alive) return;

        // A pillar of light stands on the chest, and the song rises out of it, growing, trailing sparks.
        Fx.Pillar(this, _from + new Vector2(0, 40), red, 120f, 760f, 2.2f);
        Fx.Pillar(this, _from + new Vector2(0, 40), gold, 44f, 760f, 1.6f);
        Game.Flash(red, 0.3f, 0.4f);
        Sfx.Play("rise", 0.7f, -2);
        gather.QueueFree();
        var texture = GD.Load<Texture2D>($"res://assets/sprites/items/{_song.Sprite}.png");
        int scale = Math.Max(1, (int)MathF.Min(110f / texture.GetHeight(), 120f / texture.GetWidth()));
        var raysBack = Fx.Rays(this, Hover, red, 520f, 18, 0.32f);
        var raysFront = Fx.Rays(this, Hover, gold, 380f, 10, 0.22f);
        raysFront.CreateTween().SetLoops().TweenProperty(raysFront, "rotation", -Mathf.Tau, 6f).AsRelative();
        raysBack.Modulate = raysFront.Modulate = Colors.Transparent;
        raysBack.CreateTween().TweenProperty(raysBack, "modulate", Colors.White, 1.4f);
        raysFront.CreateTween().TweenProperty(raysFront, "modulate", Colors.White, 1.4f);
        _glow = Fx.Aura(this, Hover, red, 170f);
        _glow.ZIndex = 4;
        _sprite = new Sprite2D { Texture = texture, Position = _from, Scale = Vector2.One * scale * 0.3f, Modulate = red.Lightened(0.6f), ZIndex = 6 };
        AddChild(_sprite);
        var trail = Fx.Sparks(red, 60, 40f, 140f, spread: 50f, lifetime: 1.4f, gravity: -120f);
        trail.OneShot = false;
        trail.Explosiveness = 0f;
        trail.Emitting = true;
        trail.ZIndex = 5;
        _sprite.AddChild(trail);
        var rise = _sprite.CreateTween().SetParallel();
        rise.TweenProperty(_sprite, "position", Hover, 1.5f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        rise.TweenProperty(_sprite, "scale", Vector2.One * scale, 1.5f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        rise.TweenProperty(_sprite, "modulate", Colors.White, 1.5f);
        rise.TweenProperty(_sprite, "rotation", Mathf.Tau, 1.5f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        await ToSignal(rise, Tween.SignalName.Finished);
        if (!Alive) return;

        // The hit: white, then red; the room shakes, three shockwaves, sparks and lightning, and the banner slams in.
        Game.HitStop(0.25);
        Game.Flash(Colors.White, 0.8f, 0.5f);
        Game.Shake(0.55f);
        Sfx.Play("reveal_big", 0.7f, 0);
        Sfx.Play("fanfare", 0.8f, -2);
        Sfx.Play("boom", 0.6f, -4);
        for (int i = 0; i < 3; i++)
        {
            int ring = i;
            GetTree().CreateTimer(0.12 * i).Timeout += () =>
            {
                if (Alive) Fx.Ring(this, Hover, ring == 1 ? gold : red, 760f - 140f * ring, 1.0f, 12f - 3f * ring);
            };
        }
        Fx.Burst(this, Hover, red, 160, 520f);
        Fx.Burst(this, Hover, gold, 90, 380f);
        Fx.Lightning(this, Hover, gold, 8, 260f, 0.7f);
        Fx.Confetti(this, Hover + new Vector2(0, 40), 140);
        var banner = UiKit.MakeLabel("A HEARTSONG!", 64, red, HorizontalAlignment.Center, outline: 8).Place(140, 40, 1000, 80);
        banner.PivotOffset = new Vector2(500, 40);
        banner.Scale = Vector2.One * 2.6f;
        banner.Modulate = Colors.Transparent;
        banner.ZIndex = 8;
        AddChild(banner);
        var slam = banner.CreateTween().SetParallel();
        slam.TweenProperty(banner, "scale", Vector2.One, 0.35f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        slam.TweenProperty(banner, "modulate", Colors.White, 0.15f);
        var shimmer = banner.CreateTween().SetLoops();
        shimmer.TweenProperty(banner, "self_modulate", red.Lightened(0.45f), 0.6f).SetDelay(0.4f);
        shimmer.TweenProperty(banner, "self_modulate", Colors.White, 0.6f);
        var bob = _sprite.CreateTween().SetLoops();
        bob.TweenProperty(_sprite, "position:y", Hover.Y - 10, 0.9f).SetTrans(Tween.TransitionType.Sine);
        bob.TweenProperty(_sprite, "position:y", Hover.Y + 10, 0.9f).SetTrans(Tween.TransitionType.Sine);
        await Wait(0.5);
        if (!Alive) return;

        // What it is, and what it does: it stays up until you choose.
        var panel = UiKit.MakePanel(red.Darkened(0.25f)).Place(PanelX, PanelY + 80, PanelW, PanelH);
        panel.Modulate = Colors.Transparent;
        panel.ZIndex = 8;
        AddChild(panel);
        panel.AddChild(UiKit.MakeLabel(def.Name, 32, red.Lightened(0.15f), HorizontalAlignment.Center, outline: 6).Place(0, 12, PanelW, 40));
        bool known = Game.State?.Songbook.Contains(_song.Song) ?? false;
        panel.AddChild(UiKit.MakeLabel($"One of the nine, from {def.From}.{(known ? "" : " New to Bram's kin!")}", 16, known ? Palette.Muted : gold,
            HorizontalAlignment.Center).Wrapped().Place(40, 54, PanelW - 80, 44));
        panel.AddChild(UiKit.MakeLabel(def.Story, 16, Palette.Text, HorizontalAlignment.Center).Wrapped().Place(40, 98, PanelW - 80, 44));
        panel.AddChild(UiKit.MakeLabel("ON DISPLAY", 16, gold.Darkened(0.1f), HorizontalAlignment.Center).Place(0, 146, PanelW, 20));
        panel.AddChild(UiKit.MakeLabel(def.Effect, 16, gold, HorizontalAlignment.Center, outline: 4).Wrapped().Place(40, 166, PanelW - 80, 44));
        panel.AddChild(UiKit.MakeLabel("Free to keep, and it needs no shelf. Each one kept makes the next rarer.",
            16, Palette.Muted, HorizontalAlignment.Center).Place(20, 216, PanelW - 40, 20));
        _keep = UiKit.MakeButton("Keep it", gold, 24, Keep).Place(PanelW / 2 - 330, 254, 320, 56);
        _keep.TooltipText = $"It sings in your shop from now on: {def.Effect}";
        _decline = UiKit.MakeButton("Decline", Palette.PanelBorder.Lightened(0.25f), 24, Decline).Place(PanelW / 2 + 10, 254, 320, 56);
        _decline.TooltipText = "It goes back to the Locking, to turn up in some other chest some day. It stays in your songbook.";
        panel.AddChild(_keep);
        panel.AddChild(_decline);
        var up = panel.CreateTween().SetParallel();
        up.TweenProperty(panel, "position:y", PanelY, 0.4f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        up.TweenProperty(panel, "modulate", Colors.White, 0.3f);
        _open = true;
        Game.Moment("07_heartsong", 0.6);
    }

    public override void _Process(double delta)
    {
        // While it waits for you, it sings again now and then, softer.
        if (!Open) return;
        _sinceSong += delta;
        if (_sinceSong < 7) return;
        _sinceSong = 0;
        Sfx.Play($"song_{_song.Song}", 1f, -8);
    }

    /// <summary>Keeps it: it flies up to sing in the shop.</summary>
    public async void Keep()
    {
        if (!Open) return;
        _done = true;
        _keep.Disabled = _decline.Disabled = true;
        var red = Palette.Of(Rarity.Mythic);
        Sfx.Play($"song_{_song.Song}", 1f, -2);
        Sfx.Play("upgrade", 1.1f, -4);
        Say("It sings for your shop now!", Hover + new Vector2(0, -120), Palette.Gold, 32);
        Fx.Ring(this, _sprite.Position, Palette.Gold, 300f, 0.6f, 8f);
        Fx.Burst(this, _sprite.Position, Palette.Gold, 80, 360f);
        var fly = _sprite.CreateTween().SetParallel();
        fly.TweenProperty(_sprite, "position", new Vector2(640, 96), 0.7f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
        fly.TweenProperty(_sprite, "scale", _sprite.Scale * 0.4f, 0.7f);
        await ToSignal(fly, Tween.SignalName.Finished);
        if (!Alive) return;
        Fx.Burst(this, _sprite.Position, red, 60, 260f);
        await Close();
        _chosen.TrySetResult(true);
    }

    /// <summary>Declines it: it comes apart into red motes, back to the Locking.</summary>
    public async void Decline()
    {
        if (!Open) return;
        _done = true;
        _keep.Disabled = _decline.Disabled = true;
        var red = Palette.Of(Rarity.Mythic);
        Sfx.Play("deflate", 0.7f, -4);
        Say("It goes back to the Locking...", Hover + new Vector2(0, -120), Palette.Muted, 24);
        var motes = Fx.Sparks(red, 120, 60f, 220f, spread: 180f, lifetime: 1.6f, gravity: -160f);
        motes.Position = _sprite.Position;
        AddChild(motes);
        motes.Emitting = true;
        var fade = _sprite.CreateTween().SetParallel();
        fade.TweenProperty(_sprite, "modulate:a", 0f, 0.9f);
        fade.TweenProperty(_sprite, "position:y", _sprite.Position.Y - 80, 0.9f);
        fade.TweenProperty(_sprite, "scale", _sprite.Scale * 1.4f, 0.9f);
        await ToSignal(fade, Tween.SignalName.Finished);
        if (!Alive) return;
        await Close();
        _chosen.TrySetResult(false);
    }

    async Task Close()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.45f);
        await ToSignal(tween, Tween.SignalName.Finished);
        QueueFree();
    }

    void Say(string text, Vector2 at, Color color, int size)
    {
        var label = UiKit.MakeLabel(text, size, color, HorizontalAlignment.Center, outline: 6).Place(at.X - 460, at.Y - 24, 920, 48);
        label.ZIndex = 9;
        label.Modulate = Colors.Transparent;
        AddChild(label);
        var tween = label.CreateTween();
        tween.TweenProperty(label, "modulate", Colors.White, 0.2f);
        tween.TweenInterval(1.4f);
        tween.TweenProperty(label, "modulate:a", 0f, 0.5f);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }

    public override void _ExitTree()
    {
        // Freed before a choice (the phase went away): nothing kept.
        _chosen.TrySetResult(false);
    }
}
