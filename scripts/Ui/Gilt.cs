using Godot;
using Keywright.Core;

namespace Keywright.Ui;

/// <summary>
/// The gilded on screen (0.33; 0.34, docs/narrative.md): someone the Guild took, gold all over, as they come out of a
/// crate. One shader (assets/shaders/gilded.gdshader); it's a colour change, so the art's pixels stay where they are.
/// </summary>
public static class Gilt
{
    static Shader _shader;
    static Shader Shader => _shader ??= GD.Load<Shader>("res://assets/shaders/gilded.gdshader");

    /// <summary>A gilded statue: the whole picture in gold.</summary>
    public static void Statue(CanvasItem sprite)
    {
        var material = new ShaderMaterial { Shader = Shader };
        material.SetShaderParameter("whole", true);
        sprite.Material = material;
    }
}
