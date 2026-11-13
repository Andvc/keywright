using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;
using Keywright.Core;

namespace Keywright;

/// <summary>
/// The game's own files in user:// (the save, the profile, the settings), kept so nothing a player has done can be lost
/// to a crash, a full disk or a version that can't read it:
/// <list type="bullet">
/// <item>a write goes to a temporary file first, flushed to the disk, and replaces the old file only once it's whole;
/// the old one stays beside it as a .bak (<see cref="Write"/>);</item>
/// <item>reading falls back to the .bak when the file is missing or can't be read (<see cref="Read{T}"/>);</item>
/// <item>a file this version can't read (damaged, or from a newer version) is set aside under a name that says so, and
/// never written over (<see cref="SetAside"/>); the title screen says what was (<see cref="Notices"/>).</item>
/// </list>
/// user:// is the "Keywright" folder in the system's place for application data (%APPDATA% on Windows, so Steam Cloud
/// can keep it), not Godot's own app_userdata (<see cref="CarryOver"/> brings files over from there).
/// </summary>
public static class Storage
{
    /// <summary>What happened to a file this run that the player should hear about (shown once, on the title screen).</summary>
    public static readonly List<string> Notices = new();

    /// <summary>
    /// What <paramref name="parse"/> makes of the file at <paramref name="path"/>, or of its backup if the file is missing
    /// or <paramref name="parse"/> turns it down (returns null). A file turned down is set aside first, so the next write
    /// can't destroy it; when only the backup was good, it becomes the file again. Null when neither is readable (or
    /// there's neither).
    /// </summary>
    public static T Read<T>(string path, Func<string, T> parse) where T : class
    {
        foreach (string candidate in new[] { path, path + ".bak" })
        {
            string text = ReadText(candidate);
            if (text == null) continue;
            T value = null;
            try
            {
                value = parse(text);
            }
            catch (Exception e)
            {
                GD.PushWarning($"Couldn't read {candidate}: {e.Message}");
            }
            if (value != null)
            {
                if (candidate != path)
                {
                    GD.Print($"{path} couldn't be read: carrying on from its backup");
                    Write(path, text);
                }
                return value;
            }
            SetAside(candidate);
        }
        return null;
    }

    /// <summary>Writes <paramref name="text"/> to <paramref name="path"/> safely (see the class). False if it couldn't.</summary>
    public static bool Write(string path, string text)
    {
        string full = Full(path), temp = full + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            using (var stream = new FileStream(temp, FileMode.Create, System.IO.FileAccess.Write, FileShare.None))
            {
                var bytes = new UTF8Encoding(false).GetBytes(text);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (File.Exists(full)) File.Replace(temp, full, full + ".bak", ignoreMetadataErrors: true);
            else File.Move(temp, full);
            return true;
        }
        catch (Exception e)
        {
            GD.PushError($"Couldn't write {path}: {e.Message}");
            try
            {
                if (File.Exists(temp)) File.Delete(temp);
            }
            catch (Exception)
            {
                // Nothing more to do: the old file is still there.
            }
            return false;
        }
    }

    /// <summary>Removes a file and its backup (a run that's over shouldn't come back from its .bak).</summary>
    public static void Delete(string path)
    {
        foreach (string file in new[] { Full(path), Full(path) + ".bak" })
        {
            try
            {
                if (File.Exists(file)) File.Delete(file);
            }
            catch (Exception e)
            {
                GD.PushError($"Couldn't remove {file}: {e.Message}");
            }
        }
    }

    public static bool Exists(string path) => File.Exists(Full(path)) || File.Exists(Full(path) + ".bak");

    /// <summary>
    /// Takes a file out of use at the player's asking (a save erased, 0.37) without destroying it: the file and its
    /// backup are moved beside themselves as "name.erased-yyyyMMdd-HHmmss.ext", where a misclick can still be undone by
    /// hand. True if there was anything to move.
    /// </summary>
    public static bool Erase(string path)
    {
        bool any = false;
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        foreach (string full in new[] { Full(path), Full(path) + ".bak" })
        {
            try
            {
                if (!File.Exists(full)) continue;
                bool backup = full.EndsWith(".bak");
                string stem = backup ? full[..^4] : full;
                string aside = Path.Combine(Path.GetDirectoryName(stem)!, $"{Path.GetFileNameWithoutExtension(stem)}.erased-{stamp}{(backup ? "-bak" : "")}{Path.GetExtension(stem)}");
                File.Move(full, aside, overwrite: true);
                any = true;
            }
            catch (Exception e)
            {
                GD.PushError($"Couldn't set {full} aside: {e.Message}");
            }
        }
        return any;
    }

    /// <summary>
    /// Moves a file this version can't read out of the way, as "name.unreadable-yyyyMMdd-HHmmss.ext" beside it, so it's
    /// never written over and could be recovered by hand (or by the version that wrote it).
    /// </summary>
    public static void SetAside(string path)
    {
        string full = Full(path);
        if (!File.Exists(full)) return;
        string dir = Path.GetDirectoryName(full)!, name = Path.GetFileName(full);
        string stem = name.EndsWith(".bak") ? name[..^4] : name;
        string ext = Path.GetExtension(stem), bare = Path.GetFileNameWithoutExtension(stem);
        string aside = Path.Combine(dir, $"{bare}.unreadable-{DateTime.Now:yyyyMMdd-HHmmss}{(name.EndsWith(".bak") ? "-bak" : "")}{ext}");
        try
        {
            File.Move(full, aside);
            GD.PushWarning($"{path} couldn't be read by this version: set aside as {Path.GetFileName(aside)}");
            if (!name.EndsWith(".bak"))
                Notices.Add(Loc.T($"{stem} couldn't be read (damaged, or from a newer version), so it was set aside as {Path.GetFileName(aside)}."));
        }
        catch (Exception e)
        {
            GD.PushError($"Couldn't set {path} aside: {e.Message}");
        }
    }

    static string ReadText(string path)
    {
        string full = Full(path);
        try
        {
            return File.Exists(full) ? File.ReadAllText(full, Encoding.UTF8) : null;
        }
        catch (Exception e)
        {
            GD.PushWarning($"Couldn't open {path}: {e.Message}");
            return null;
        }
    }

    static string Full(string path) => path.StartsWith("user://") || path.StartsWith("res://") ? ProjectSettings.GlobalizePath(path) : path;

    /// <summary>
    /// Brings the player's files over from where earlier versions kept them, where they aren't here already: Godot's own
    /// app_userdata folder (until 0.27), and before that LootFall's (until 0.18: only what this game can read, and not its
    /// settings, since that name now belongs to another game). Nothing here is ever overwritten.
    /// </summary>
    public static void CarryOver(Func<string, bool> readableSave, Func<string, bool> readableProfile)
    {
        string here = OS.GetUserDataDir();
        var godot = new[] { "Godot", "godot" };
        foreach (var (name, settings) in new[] { ("Keywright", true), ("LootFall", false) })
        {
            foreach (string folder in godot)
            {
                string old = Path.Combine(OS.GetDataDir(), folder, "app_userdata", name);
                if (!Directory.Exists(old) || Path.GetFullPath(old) == Path.GetFullPath(here)) continue;
                try
                {
                    Directory.CreateDirectory(here);
                    Bring(old, here, "profile.json", readableProfile);
                    Bring(old, here, "save.json", readableSave);
                    if (settings) Bring(old, here, "settings.cfg", _ => true);
                }
                catch (Exception e)
                {
                    GD.PushWarning($"Couldn't bring the old files over from {old}: {e.Message}");
                }
            }
        }
    }

    static void Bring(string from, string to, string file, Func<string, bool> readable)
    {
        string source = Path.Combine(from, file), target = Path.Combine(to, file);
        if (!File.Exists(source) || File.Exists(target) || !readable(File.ReadAllText(source))) return;
        File.Copy(source, target);
        GD.Print($"Brought {file} over from {from}");
    }
}
