using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using Keywright.Core;

namespace Keywright.Ui;

public enum ShopHour { Morning, Midday, Evening, Night }

/// <summary>The round-2 scene, in the native 320x180 layout at four screen pixels per art pixel.</summary>
public partial class ShopView : Node2D
{
    public const int ScaleFactor = 4;
    public static ShopHour LastHour { get; private set; } = ShopHour.Morning;
    public static readonly string[] Places = { "stall", "thornwick", "crownhold", "emporium" };
    readonly int _tier;
    readonly float _brightness;
    readonly ShopHour _hour, _from;
    readonly GameState _state;
    readonly List<(CanvasItem Item, bool Chest)> _tinted = new();
    Node2D _world;
    Sprite2D _oldSky, _sky;
    readonly List<Sprite2D> _lit = new(), _unlit = new();
    Painter _glow;
    float _progress = 1;
    static Dictionary<string, JsonElement> _figures;

    public ShopView() { }
    public ShopView(int tier, float brightness, ShopHour hour, GameState state = null, bool transition = false)
    {
        _tier = Math.Clamp(tier, 1, 4);
        _brightness = brightness;
        _hour = hour;
        _from = transition ? LastHour : hour;
        _state = state;
        LastHour = hour;
    }

    public static ShopHour CustomerHour(GameState state)
    {
        int served = state.Today.Bought + state.Today.Passed;
        int total = Math.Max(1, served + state.Queue.Count + (state.Current == null ? 0 : 1));
        return (ShopHour)Math.Clamp(served * 3 / total, 0, 2);
    }

    public static int FigureHeight(string key)
    {
        _figures ??= JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            Godot.FileAccess.GetFileAsString("res://assets/figures/layout.json"));
        return _figures != null && _figures.TryGetValue(key, out var figure) ? figure.GetProperty("height").GetInt32() : 124;
    }

    static Color Tint(ShopHour hour) => hour switch
    {
        ShopHour.Morning => new Color(0.94f, 0.96f, 1),
        ShopHour.Evening => new Color(1, 0.86f, 0.74f),
        ShopHour.Night => new Color(0.5f, 0.55f, 0.8f),
        _ => Colors.White,
    };
    static bool IsLit(ShopHour hour) => hour is ShopHour.Evening or ShopHour.Night;

    Sprite2D Layer(Node parent, string path, Vector2 at, int z)
    {
        var sprite = new Sprite2D
        {
            Texture = GD.Load<Texture2D>($"res://assets/{path}.png"),
            Centered = false, Position = at * ScaleFactor, Scale = Vector2.One * ScaleFactor,
            ZIndex = z, TextureFilter = TextureFilterEnum.Nearest,
        };
        parent.AddChild(sprite);
        return sprite;
    }

    public override void _Ready()
    {
        string place = Places[_tier - 1];
        AddChild(new ColorRect { Color = Palette.Bg, ZIndex = -110, MouseFilter = Control.MouseFilterEnum.Ignore }
            .Place(-120, -120, 1520, 960));
        _oldSky = Layer(this, $"sky/{_from.ToString().ToLowerInvariant()}", Vector2.Zero, -100);
        _sky = Layer(this, $"sky/{_hour.ToString().ToLowerInvariant()}", Vector2.Zero, -99);
        _world = new Node2D();
        AddChild(_world);
        Layer(_world, $"place/{place}", Vector2.Zero, -90);
        Layer(_world, $"shelf/{place}", new Vector2(70, 15), -70);
        Layer(_world, $"frame/{place}", Vector2.Zero, -60);
        foreach (int x in new[] { 8, 311 })
        {
            _unlit.Add(Layer(this, $"props/lantern_{place}_unlit", new Vector2(x - 8, 60), -50));
            _lit.Add(Layer(this, $"props/lantern_{place}", new Vector2(x - 8, 60), -49));
        }
        Layer(_world, $"counter/{place}", new Vector2(0, 119), -40);
        // The counter's far edge gets the figures' outline, a row above it, so the counter reads against the place beyond
        // (the chest, the props and whoever stands in front of it cover it).
        _world.AddChild(new ColorRect { Color = Palette.Outline, ZIndex = -40, MouseFilter = Control.MouseFilterEnum.Ignore }
            .Place(0, 118 * ScaleFactor, 320 * ScaleFactor, ScaleFactor));
        BuildProps();
        _glow = new Painter(p =>
        {
            foreach (int x in new[] { 8, 311 })
            foreach (var ring in new[] { (72, 18), (48, 26), (28, 34) })
                p.DrawCircle(new Vector2(x * 4, 74 * 4), ring.Item1, new Color(1, 0.76f, 0.3f, ring.Item2 / 255f));
        }) { ZIndex = -39 };
        AddChild(_glow);
        SetLight(_from == _hour ? 1 : 0);
        if (_from != _hour)
            CreateTween().TweenMethod(Callable.From<float>(SetLight), 0f, 1f, 0.7f).SetTrans(Tween.TransitionType.Sine);
    }

    void BuildProps()
    {
        if (_state == null) return;
        // Two small groups around the layout's (99,119) and (183,119) base spots.
        // Tall tools go behind low cases/books so every purchased tool has a visible silhouette.
        foreach (var (key, art, x, y) in new[] {
            ("scales", "scales", 99, 116), ("lamp", "witchlight", 183, 119),
            ("seal", "seal", 196, 119), ("magnet", "lodestone", 109, 119),
            ("patrons", "book", 85, 119), ("block", "gavel", 173, 119),
            ("picks", "picks", 97, 119) })
        {
            if (!_state.Has(key)) continue;
            var prop = Layer(_world, $"props/{art}", Vector2.Zero, -30);
            prop.Position = new Vector2(x * 4 - prop.Texture.GetWidth() * 2, y * 4 - prop.Texture.GetHeight() * 4);
        }
    }

    /// <summary>Lights a world actor without tinting the actor's labels or speech bubble.</summary>
    public void Light(CanvasItem item, bool chest = false)
    {
        _tinted.Add((item, chest));
        ApplyTint(item, chest);
    }

    void ApplyTint(CanvasItem item, bool chest)
    {
        Color color = Tint(_from).Lerp(Tint(_hour), _progress);
        if (chest) color = Colors.White.Lerp(color, 0.5f);
        item.Modulate = color * new Color(_brightness, _brightness, _brightness, 1);
    }

    void SetLight(float progress)
    {
        _progress = progress;
        ApplyTint(_world, false);
        _oldSky.Modulate = new Color(_brightness, _brightness, _brightness);
        _sky.Modulate = new Color(_brightness, _brightness, _brightness, progress);
        float light = Mathf.Lerp(IsLit(_from) ? 1 : 0, IsLit(_hour) ? 1 : 0, progress);
        foreach (var sprite in _unlit)
        {
            ApplyTint(sprite, false);
        }
        foreach (var sprite in _lit) sprite.Modulate = new Color(_brightness, _brightness, _brightness, light);
        _glow.Modulate = new Color(1, 1, 1, light * _brightness);
        foreach (var (item, chest) in _tinted)
            if (GodotObject.IsInstanceValid(item)) ApplyTint(item, chest);
    }
}
