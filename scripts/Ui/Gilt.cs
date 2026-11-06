using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// The Gilding on screen (0.33, docs/narrative.md): Bram's skin turning to gold a step at a time as his pledge comes due
/// (<see cref="GameState.BramGilt"/>), with a glove over the hand while he hides it (<see cref="GameState.BramGloved"/>),
/// and the gilded statues, gold all over. One shader (assets/shaders/gilded.gdshader) and, for Bram, a mask for each
/// picture of him saying which pixels gild at which step (assets/ui/gild/, drawn from his picture's skin by
/// tools/import_art.py --gild-only). It's a colour change, so the art's pixels stay where they are.
/// </summary>
public static class Gilt
{
    /// <summary>Which picture of Bram: his full figure, the notebook's and tips' head crop, or the small face.</summary>
    public const string Figure = "figure", Portrait = "portrait", Face = "face";

    static Shader _shader;
    static Shader Shader => _shader ??= GD.Load<Shader>("res://assets/shaders/gilded.gdshader");

    /// <summary>Bram as the gold has left him today: nothing when his pledge is clear (or before the run starts).</summary>
    public static void Bram(CanvasItem sprite, GameState state, string picture)
    {
        int stage = state?.BramGilt ?? 0;
        string mask = $"res://assets/ui/gild/bram_{picture}.png";
        if (stage <= 0 || !ResourceLoader.Exists(mask))
        {
            sprite.Material = null;
            return;
        }
        var material = new ShaderMaterial { Shader = Shader };
        material.SetShaderParameter("mask", GD.Load<Texture2D>(mask));
        material.SetShaderParameter("stage", (float)stage);
        material.SetShaderParameter("glove", state.BramGloved);
        material.SetShaderParameter("whole", false);
        sprite.Material = material;
    }

    /// <summary>A gilded statue: the whole picture in gold.</summary>
    public static void Statue(CanvasItem sprite)
    {
        var material = new ShaderMaterial { Shader = Shader };
        material.SetShaderParameter("whole", true);
        sprite.Material = material;
    }
}
