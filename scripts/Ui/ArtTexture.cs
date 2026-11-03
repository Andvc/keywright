using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace Keywright.Ui;

/// <summary>
/// Keeps the game's layout in its original units while drawing the full detail of the density-study textures.
/// A size override changes the texture's reported size, not its image or the pixels uploaded to the renderer.
/// </summary>
public static class ArtTexture
{
    const string MetadataPath = "res://assets/art_density.json";
    static readonly Dictionary<string, Texture2D> Cache = new();
    static readonly Dictionary<string, (Vector2I Logical, Vector2I Physical)> Sizes = new();
    static bool _read, _outlinesAllowed = true;

    public static bool OutlinesAllowed
    {
        get { ReadMetadata(); return _outlinesAllowed; }
    }

    static void ReadMetadata()
    {
        if (_read) return;
        _read = true;
        if (!Godot.FileAccess.FileExists(MetadataPath)) return;
        using var document = JsonDocument.Parse(Godot.FileAccess.GetFileAsString(MetadataPath));
        var root = document.RootElement;
        if (root.GetProperty("version").GetInt32() != 1)
            throw new InvalidOperationException($"Unsupported art density metadata: {MetadataPath}");
        _outlinesAllowed = root.GetProperty("outlines").GetBoolean();
        foreach (var entry in root.GetProperty("textures").EnumerateObject())
        {
            var logical = entry.Value.GetProperty("logical_size");
            var physical = entry.Value.GetProperty("physical_size");
            var size = new Vector2I(logical[0].GetInt32(), logical[1].GetInt32());
            var pixels = new Vector2I(physical[0].GetInt32(), physical[1].GetInt32());
            if (size.X <= 0 || size.Y <= 0 || pixels.X <= 0 || pixels.Y <= 0)
                throw new InvalidOperationException($"Invalid art density size: {entry.Name}");
            Sizes.Add(entry.Name, (size, pixels));
        }
    }

    public static Texture2D Load(string path)
    {
        ReadMetadata();
        if (Cache.TryGetValue(path, out var cached)) return cached;
        if (!Sizes.TryGetValue(path, out var sizes)) return Cache[path] = GD.Load<Texture2D>(path);
        using var imported = GD.Load<Texture2D>(path);
        using var image = imported.GetImage();
        if (image.GetSize() != sizes.Physical)
            throw new InvalidOperationException($"Art density import is stale for {path}: expected {sizes.Physical}, got {image.GetSize()}");
        if (image.IsCompressed() && image.Decompress() != Error.Ok)
            throw new InvalidOperationException($"Cannot decompress art texture: {path}");
        var texture = ImageTexture.CreateFromImage(image);
        texture.SetSizeOverride(sizes.Logical);
        return Cache[path] = texture;
    }
}
