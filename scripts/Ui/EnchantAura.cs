using System;
using Godot;

namespace Keywright.Ui;

/// <summary>
/// The glow behind an enchanted thing (<see cref="Enchant.Aura"/>): a soft violet light that breathes, and a few motes
/// that rise through it on the art's grid and fade. A node of its own, so it moves wherever it's put (a paused screen
/// included).
/// </summary>
public partial class EnchantAura : Node2D
{
    static readonly Random Rng = new();

    public float Radius = 24;

    /// <summary>Draw the light as well as the motes (off where something else already glows behind the thing).</summary>
    public bool Light = true;
    double _time = Rng.NextDouble() * 10;
    readonly (float Angle, float Speed, float Reach, float Offset)[] _motes = new (float, float, float, float)[5];

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        for (int i = 0; i < _motes.Length; i++)
            _motes[i] = ((float)(Rng.NextDouble() * Math.Tau), 0.35f + (float)Rng.NextDouble() * 0.35f, 0.55f + (float)Rng.NextDouble() * 0.45f, (float)Rng.NextDouble());
    }

    public override void _Process(double delta)
    {
        _time += delta;
        QueueRedraw();
    }

    public override void _Draw()
    {
        float breath = 0.5f + 0.5f * MathF.Sin((float)_time * 1.9f);
        // The light: three rings of it, swelling a little as it breathes.
        for (int i = Light ? 3 : 0; i >= 1; i--)
            DrawCircle(Vector2.Zero, Radius * (i / 3f) * (0.92f + 0.12f * breath), Palette.Magic with { A = 0.10f + 0.07f * breath });
        // Motes: each rises from the foot of the glow to its top, drifting on a slow curl, and fades at both ends.
        float cell = MathF.Max(2f, MathF.Round(Radius / 9f));
        foreach (var (angle, speed, reach, offset) in _motes)
        {
            float life = (float)((_time * speed + offset) % 1.0);
            float x = MathF.Cos(angle + life * 2.2f) * Radius * reach * 0.8f;
            float y = Radius * 0.75f - life * Radius * 1.7f;
            float alpha = MathF.Sin(life * MathF.PI);
            var at = new Vector2(MathF.Round(x / cell) * cell, MathF.Round(y / cell) * cell);
            DrawRect(new Rect2(at - Vector2.One * cell / 2, Vector2.One * cell), Palette.Magic.Lightened(0.45f) with { A = alpha * 0.9f });
        }
    }
}
