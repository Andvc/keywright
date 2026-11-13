using System;
using System.Collections.Generic;
using Godot;

namespace Keywright.Ui;

/// <summary>
/// An enchanted thing on screen (0.37): light flowing over the picture and licking up off its edges
/// (assets/shaders/enchanted.gdshader) and, behind it, a soft violet glow that breathes with a few motes rising through
/// it. Until 0.37 an enchanted thing had a burst as it landed and three faint rings behind its icon, and the owner
/// found it "too difficult to notice": anything that shows one (the opening's table, the shelf, the collection, a
/// chooser) goes through here now, so it reads the same everywhere, moving, at any size.
/// </summary>
public static class Enchant
{
    static Shader _shader;
    static Shader Shader => _shader ??= GD.Load<Shader>("res://assets/shaders/enchanted.gdshader");
    static readonly Random Rng = new();

    /// <summary>The name of the layer of light over a thing's sprite.</summary>
    const string Layer = "Enchantment"; // loc: keys

    /// <summary>The art's cells of empty margin put round a picture, for the light round its edge to be drawn in.</summary>
    const int Margin = 6;

    /// <summary>Each picture with its margin, made once (the image is the picture's own, at its own density).</summary>
    static readonly Dictionary<Texture2D, Texture2D> Padded = new();

    /// <summary>
    /// Light flows over this thing's sprite and rises off its edges: a layer of its own over the picture (a child, so
    /// it moves, squashes and fades with it), which leaves the picture, its size and its place as they are.
    /// </summary>
    public static void Flow(Sprite2D sprite, float strength = 1f)
    {
        if (sprite.Texture == null || sprite.HasNode(Layer)) return;
        var padded = Pad(sprite.Texture);
        var material = new ShaderMaterial { Shader = Shader };
        material.SetShaderParameter("cells", padded.GetSize());
        material.SetShaderParameter("phase", (float)(Rng.NextDouble() * 20));
        material.SetShaderParameter("strength", strength);
        sprite.AddChild(new Sprite2D
        {
            Name = Layer,
            Texture = padded,
            Centered = sprite.Centered,
            Offset = sprite.Centered ? sprite.Offset : sprite.Offset - Vector2.One * Margin,
            Material = material,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        });
    }

    /// <summary>The light off again (a thing that's stopped being enchanted to look at: broken, sold).</summary>
    public static void Still(Sprite2D sprite) => sprite.GetNodeOrNull(Layer)?.QueueFree();

    static Texture2D Pad(Texture2D texture)
    {
        if (Padded.TryGetValue(texture, out var done)) return done;
        using var image = texture.GetImage();
        if (image.IsCompressed()) image.Decompress();
        image.Convert(Image.Format.Rgba8);
        // A dense picture reports its art's size, not its image's (ArtTexture): the margin is in the art's cells.
        var art = texture.GetSize();
        int w = image.GetWidth(), h = image.GetHeight();
        int pad = Math.Max(1, Mathf.RoundToInt(Margin * w / Math.Max(1f, art.X)));
        using var big = Image.CreateEmpty(w + 2 * pad, h + 2 * pad, false, Image.Format.Rgba8);
        big.BlitRect(image, new Rect2I(0, 0, w, h), new Vector2I(pad, pad));
        var made = ImageTexture.CreateFromImage(big);
        made.SetSizeOverride(new Vector2I(Mathf.RoundToInt(art.X) + 2 * Margin, Mathf.RoundToInt(art.Y) + 2 * Margin));
        return Padded[texture] = made;
    }

    /// <summary>
    /// The glow behind an enchanted thing, <paramref name="radius"/> pixels out from the node's middle: a soft violet
    /// light that breathes, and motes that rise through it and fade. Add it under the picture.
    /// </summary>
    /// <param name="light">The light as well as the motes (false where something else already glows behind the thing).</param>
    public static Node2D Aura(float radius, Vector2 middle = default, bool light = true) =>
        new EnchantAura { Radius = radius, Position = middle, Light = light };
}
