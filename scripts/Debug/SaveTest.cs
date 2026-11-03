using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Godot;
using Keywright.Core;

namespace Keywright.Debug;

/// <summary>
/// Checks that a player's files survive what can happen to them: `godot --headless --path . -- --save-test`. It works in
/// a folder of its own (user://save-test), never on the player's files, and exits 1 if anything fails:
/// <list type="bullet">
/// <item>the safe writer keeps the last good file as a .bak, falls back to it when the file is damaged, sets a file it
/// can't read aside rather than writing over it, and a finished run's save goes with its backup;</item>
/// <item>a run played for a while by the sim's careful player saves, loads and saves again the same;</item>
/// <item>every older save version still loads (moved onto the current one), and a newer one, damaged JSON or a field of
/// the wrong type is turned down instead of throwing;</item>
/// <item>the profile does the same, and an old one without a version reads as version 1.</item>
/// </list>
/// </summary>
public static class SaveTest
{
    const string Dir = "user://save-test";
    static int _failures;

    public static bool Run()
    {
        _failures = 0;
        string dir = ProjectSettings.GlobalizePath(Dir);
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);

        TestStorage(dir);
        TestRun();
        TestProfile();

        Directory.Delete(dir, true);
        GD.Print(_failures == 0 ? "Save test: everything survived." : $"Save test: {_failures} FAILED");
        return _failures == 0;
    }

    static void Check(bool ok, string what)
    {
        GD.Print($"{(ok ? "ok  " : "FAIL")}  {what}");
        if (!ok) _failures++;
    }

    static void TestStorage(string dir)
    {
        string path = $"{Dir}/file.json", full = Path.Combine(dir, "file.json");
        Check(Storage.Write(path, "one") && Storage.Read(path, t => t) == "one", "a file is written and read back");
        Storage.Write(path, "two");
        Check(File.ReadAllText(full + ".bak") == "one" && !File.Exists(full + ".tmp"), "writing again keeps the last one as its .bak, and no temporary file behind");

        File.WriteAllText(full, "{ half a fi");
        string read = Storage.Read(path, t => t.StartsWith("{") ? null : t);
        Check(read == "one" && File.ReadAllText(full) == "one", "a damaged file falls back to its backup, which becomes the file again");
        Check(Directory.GetFiles(dir, "file.unreadable-*.json").Length == 1, "...and the damaged one is set aside, not deleted");

        string profile = $"{Dir}/profile.json";
        File.WriteAllText(Path.Combine(dir, "profile.json"), "{ \"Version\": 99, \"Runs\": 40 }");
        Check(Storage.Read(profile, Profile.FromJson) == null, "a profile from a newer version isn't read");
        Check(!File.Exists(Path.Combine(dir, "profile.json")) && Directory.GetFiles(dir, "profile.unreadable-*.json").Length == 1,
            "...and is set aside, so writing a new one can't destroy it");

        Storage.Write(path, "three");
        Storage.Delete(path);
        Check(!Storage.Exists(path), "a finished run's save goes, backup and all");
        Check(Storage.Read($"{Dir}/missing.json", t => t) == null, "a file that isn't there reads as nothing");
    }

    static void TestRun()
    {
        var state = GameState.NewRun(4242, Characters.Kit, 2, false);
        Simulation.FastForward(state, 12);
        // What 0.30's tales leave behind, so the round trip carries it: a favour, a paper, a loan, an effect that lasts.
        state.Owe(Core.TaleBook.Wynn, "pip");
        state.Gather("register");
        state.Borrow(30, 1f / 3, state.Day + 3);
        state.Until[Core.TaleBook.Picket] = state.Day + 4;
        string json = state.Save().ToJson();
        var data = SaveData.FromJson(json);
        Check(data != null, $"a run in week {state.Week} with {state.Gold}g, {state.OwnedCurios.Count()} curios saves");
        if (data == null) return;
        var loaded = GameState.Load(data);
        Check(loaded.Owed.ContainsKey(Core.TaleBook.Wynn) && loaded.Evidence.Contains("register") && loaded.LoanOwed == 40 &&
              loaded.While(Core.TaleBook.Picket), "a favour owed, a paper, a loan and a picket at the door come back");
        string again = loaded.Save().ToJson();
        bool same = Without(json, "SavedAt") == Without(again, "SavedAt");
        Check(same, "it loads and saves again the same");
        if (!same)
        {
            var a = JsonNode.Parse(json)!.AsObject();
            var b = JsonNode.Parse(again)!.AsObject();
            foreach (var (key, value) in a)
                if (key != "SavedAt" && value?.ToJsonString() != b[key]?.ToJsonString())
                    GD.Print($"      {key}: {Short(value)} -> {Short(b[key])}");
        }

        for (int version = 1; version < SaveData.CurrentVersion; version++)
        {
            var old = JsonNode.Parse(json)!.AsObject();
            old["Version"] = version;
            if (version < 4)
            {
                // Before 0.21 there were no plans, terms or curio lists: an old save hasn't got them.
                foreach (string field in new[] { "Plan", "NextPlan", "Boons", "Banes", "CuriosOpen" }) old.Remove(field);
                old["Day"] = 7;
            }
            var migrated = SaveData.FromJson(old.ToJsonString());
            bool loads = false;
            try
            {
                loads = migrated != null && GameState.Load(migrated).Day >= 0;
            }
            catch (Exception e)
            {
                GD.Print($"      {e.GetType().Name}: {e.Message}");
            }
            Check(loads && migrated!.Version == SaveData.CurrentVersion, $"a version {version} save is brought up to version {SaveData.CurrentVersion} and loads");
        }

        // 0.17's renames: an old save with the old names is brought up to the new ones; one with the new names is left be.
        string Renamed(int version, params string[] found)
        {
            var save = JsonNode.Parse(json)!.AsObject();
            save["Version"] = version;
            save["Found"] = new JsonArray(found.Select(f => (JsonNode)f).ToArray());
            var read = SaveData.FromJson(save.ToJsonString());
            return read == null ? "unreadable" : string.Join(", ", GameState.Load(read).Found.OrderBy(f => f));
        }
        Check(Renamed(2, "Copper Pennies", "Swamp Tonic", "Healing Draught") == "Barrow Silver, Elixir of Vigour, Healing Draught",
            "a save from before 0.17 has its old names brought up to the new ones");
        Check(Renamed(2, "Healing Draught", "Elixir of Vigour") == "Elixir of Vigour, Healing Draught",
            "a version 2 save with the new names keeps them (no potion moved up a step)");
        Check(Renamed(SaveData.CurrentVersion, "Healing Draught", "Elixir of Vigour", "Swamp Tonic") == "Elixir of Vigour, Healing Draught, Swamp Tonic",
            "a current save is never renamed");

        // 0.28's new cast: a 0.27 save's regulars, lessons and tips are brought over to who trades their way now.
        var cast = JsonNode.Parse(json)!.AsObject();
        cast["Version"] = 4;
        // (0.27 had no tales: none of their fields.)
        foreach (string field in new[] { "Tales", "BeatsDone", "Flags", "Gone", "Standing", "Notices", "BackRoom", "TaleLog", "OffersDeclined", "Keepsakes", "Promises", "PorterUntil" })
            cast.Remove(field);
        cast["Met"] = new JsonArray("penn", "tam", "bettany");
        cast["Tutored"] = new JsonArray("snikkit");
        cast["HintsSeen"] = new JsonArray("first_penn", "seller_wenna", "colours");
        var recast = SaveData.FromJson(cast.ToJsonString());
        Check(recast != null && recast.Met.OrderBy(m => m).SequenceEqual(new[] { "ada", "bettany", "pip" }) && recast.Tutored.SequenceEqual(new[] { "nettle" }) &&
              recast.HintsSeen.OrderBy(h => h).SequenceEqual(new[] { "colours", "first_ada", "seller_rook" }),
            "a 0.27 save's regulars, lessons and tips are renamed for the new cast");
        Check(recast != null && GameState.Load(recast).TalesDrawn.Count == 0, "a 0.27 save loads with no tales yet (they're drawn at the next morning)");

        var newer = JsonNode.Parse(json)!.AsObject();
        newer["Version"] = SaveData.CurrentVersion + 1;
        Check(SaveData.FromJson(newer.ToJsonString()) == null, "a save from a newer version isn't read");
        Check(SaveData.FromJson(json[..(json.Length / 2)]) == null, "half a save isn't read");
        var wrong = JsonNode.Parse(json)!.AsObject();
        wrong["Gold"] = "lots";
        Check(SaveData.FromJson(wrong.ToJsonString()) == null, "a field of the wrong type is turned down, not thrown");
    }

    static void TestProfile()
    {
        var profile = new Profile { Runs = 3, Cleared = 1, BestTier = 2 };
        profile.Songs.Add(Heartsongs.All[0].Key);
        profile.Challenge[Characters.Kit] = 2;
        string json = profile.ToJson();
        var back = Profile.FromJson(json);
        Check(back != null && back.ToJson() == json, "a profile reads back the same");
        var old = JsonNode.Parse(json)!.AsObject();
        old.Remove("Version");
        old["Met"] = new JsonArray("penn", "grit");
        old["Taught"] = new JsonArray("lesson:wren", "firstday");
        var read = Profile.FromJson(old.ToJsonString());
        Check(read?.Version == Profile.CurrentVersion && read.Met.SequenceEqual(new[] { "ada", "durgan" }) && read.Taught.SequenceEqual(new[] { "lesson:ilse", "firstday" }),
            "a profile from before versions reads as version 1 and is brought up to the new cast");
        Check(Profile.FromJson("{ \"Runs\": ") == null, "a damaged profile isn't read (so it's never replaced by an empty one)");
    }

    static string Short(JsonNode node)
    {
        string text = node?.ToJsonString() ?? "null";
        return text.Length > 160 ? text[..160] + "..." : text;
    }

    static string Without(string json, string field)
    {
        var node = JsonNode.Parse(json)!.AsObject();
        node.Remove(field);
        return node.ToJsonString();
    }
}
