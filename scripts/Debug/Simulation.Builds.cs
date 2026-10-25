using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Debug;

/// <summary>
/// The builds report (`--sim=builds`): what each curio is worth on its own, what each way of trading is worth built
/// around, which pairs are worth more together than apart, and that no build makes buying blind pay.
/// </summary>
public static partial class Simulation
{
    /// <summary>
    /// How a style did over its runs: cleared, the median day it cleared, the takings to the eighth week (their geometric
    /// mean: the economy compounds, so a curio's worth is a factor, and one lucky run shouldn't swamp it), and the median
    /// week lost.
    /// </summary>
    sealed record Measured(float Cleared, int ClearDay, double Takings, int LostWeek);

    /// <summary>Plays the style's runs side by side (each run is its own state; nothing in Core is shared but read-only tables).</summary>
    static Measured Measure(Style style, int campaigns)
    {
        var logs = Enumerable.Range(0, campaigns).AsParallel().Select(i => Play(style, 5000 + i, Guild.Weeks * Guild.DaysPerWeek).Log).ToList();
        var days = logs.Where(l => l.ClearedDay > 0).Select(l => l.ClearedDay).OrderBy(d => d).ToList();
        var lost = logs.Where(l => l.LostWeek > 0).Select(l => l.LostWeek).OrderBy(w => w).ToList();
        return new Measured(100f * days.Count / logs.Count, days.Count * 2 > logs.Count ? days[days.Count / 2] : 0,
            Math.Exp(logs.Average(l => Math.Log(Math.Max(1, l.Takings)))), lost.Count * 2 > logs.Count ? lost[lost.Count / 2] : 0);
    }

    /// <summary>Takings against the baseline's, as a percentage more (or less).</summary>
    static string Gain(Measured m, Measured baseline) => $"{Pct(m, baseline),5:+0;-0}%";

    static string Row(Measured m, Measured baseline) =>
        $"{Gain(m, baseline)}   cleared {m.Cleared,4:0}%  day {(m.ClearDay == 0 ? "-" : m.ClearDay.ToString()),3}" +
        (m.LostWeek > 0 ? $"   lost wk {m.LostWeek}" : "");

    public static void Builds(int campaigns)
    {
        var watch = Stopwatch.StartNew();
        // The careful collector, buying nothing from Nim: what one curio, or a build, adds to that. Curios come when Nim
        // could first have brought each (its shop, its rarity's shop and week), as in a run.
        var plain = new Style("careful, no curios", Care.Tools, true, Curios: false, Arrives: true);
        // A buyer who never lays a hand on a chest, and one who stops once two sight-unseen curios are kept: that build's.
        var blind = new Style("blind", Care.Blind, false, Curios: false);
        var unseen = new Style("sight unseen", Care.Unseen, true, Curios: false, Arrives: true);
        var baseline = Measure(plain, campaigns);
        var blindBase = Measure(blind, campaigns);
        GD.Print($"=== Builds ({campaigns} runs each, {Guild.Weeks} weeks of {Guild.DaysPerWeek} days; takings to the debt's last week, against the careful " +
                 "collector with no curios; curios come when Nim could first have brought each) ===");
        GD.Print($"baseline: takings {baseline.Takings:0}, cleared {baseline.Cleared:0}%, day {baseline.ClearDay}");
        GD.Print($"a buyer who never shakes: {Row(blindBase, baseline)}");

        GD.Print("\nEach curio on its own, as Nim would bring it (a sight-unseen one from the first day, with a buyer who never shakes):");
        var alone = new Dictionary<string, Measured>();
        foreach (var curio in Curios.All)
        {
            var style = curio.Tags[0] == CurioTag.Blind ? blind with { Given = new[] { curio.Key } } : plain with { Given = new[] { curio.Key } };
            var m = alone[curio.Key] = Measure(style, campaigns);
            GD.Print($"  {curio.Name,-24} {Curios.RarityName(curio.Rarity),-9} {Curios.Tags(curio),-30} {Row(m, baseline)}");
        }

        // Each way of trading: its three strongest curios as Nim would bring them, and a player who buys for it from Nim.
        GD.Print("\nBuilds: each way of trading's three strongest, as Nim would bring them; then a player buying for it from Nim");
        var builds = new Dictionary<CurioTag, string[]>();
        foreach (var tag in Enum.GetValues<CurioTag>())
        {
            var best = Curios.All.Where(c => c.Has(tag)).OrderByDescending(c => alone[c.Key].Takings).Take(3).Select(c => c.Key).ToArray();
            if (best.Length < 2) continue;
            builds[tag] = best;
            var who = tag == CurioTag.Blind ? unseen : plain;
            var given = Measure(who with { Given = best }, campaigns);
            var bought = Measure(tag == CurioTag.Blind ? unseen with { Curios = true, Focus = tag, Given = null } : new Style("focused", Care.Tools, true, Focus: tag), campaigns);
            GD.Print($"  {Curios.TagName(tag),-17} {string.Join("+", best),-30} {Row(given, baseline)}");
            GD.Print($"  {"",-17} {"bought for it",-30} {Row(bought, baseline)}");
        }
        var open = Measure(new Style("careful", Care.Tools, true), campaigns);
        GD.Print($"  {"(any)",-17} {"bought as they come",-30} {Row(open, baseline)}");

        // The sight-unseen build, a piece at a time: careful until two such curios are kept, then never a hand on a chest.
        GD.Print("\nSight unseen, a piece at a time (careful until two are kept, then never shakes; from the first day, then as Nim would bring them):");
        foreach (var set in new[] { new[] { "sealed" }, new[] { "sealed", "dip" }, new[] { "sealed", "bond" }, new[] { "dip", "bond" }, new[] { "sealed", "dip", "bond" },
                     new[] { "sealed", "dip", "bond", "wholesale" }, new[] { "sealed", "blindfold" }, new[] { "sealed", "dip", "blindfold" },
                     new[] { "sealed", "dip", "bond", "blindfold" } })
            GD.Print($"  {string.Join("+", set),-34} {Row(Measure(unseen with { Given = set, Arrives = false }, campaigns), baseline)}   " +
                     $"as Nim brings them: {Row(Measure(unseen with { Given = set }, campaigns), baseline)}");

        // Pairs that should be worth more together: what the pair adds beyond what each adds alone.
        GD.Print("\nPairs, as Nim would bring them: what each adds alone, together, and the difference (positive: they're worth more together)");
        foreach (var (a, b) in Pairs)
        {
            bool sightUnseen = Curios.Get(a).Tags[0] == CurioTag.Blind;
            var both = Measure(sightUnseen ? blind with { Given = new[] { a, b } } : plain with { Given = new[] { a, b } }, campaigns);
            float ga = Pct(alone[a], baseline), gb = Pct(alone[b], baseline), gab = Pct(both, baseline);
            GD.Print($"  {a,-12} + {b,-12} {ga,5:+0;-0}% {gb,5:+0;-0}%  together {gab,5:+0;-0}%   synergy {gab - ga - gb,5:+0;-0}  " +
                     $"(cleared {alone[a].Cleared:0}% / {alone[b].Cleared:0}% / {both.Cleared:0}%)");
        }

        // No other build makes buying blind pay: a buyer who doesn't listen, with each from the first day, is still foreclosed.
        GD.Print("\nBuying blind with each other build from the first day (must still be foreclosed):");
        GD.Print($"  {"no curios",-44} {Row(blindBase, baseline)}");
        foreach (var (tag, build) in builds.Where(b => b.Key != CurioTag.Blind))
            GD.Print($"  {Curios.TagName(tag) + ": " + string.Join("+", build),-44} {Row(Measure(blind with { Given = build }, campaigns), baseline)}");
        GD.Print($"\n({watch.Elapsed.TotalSeconds:0}s)");
    }

    /// <summary>
    /// What a player building around one way of trading ends up keeping (`--sim=focus --who=TAG`): how many curios of it,
    /// how often each, and how often they clear with each count. For seeing whether a build comes together.
    /// </summary>
    public static void FocusReport(string tagName, int campaigns, bool trace = false)
    {
        if (trace)
        {
            _traceNim = true;
            var t = Enum.GetValues<CurioTag>().First(t => t.ToString().Equals(tagName, StringComparison.OrdinalIgnoreCase));
            for (int i = 0; i < 3; i++)
            {
                GD.Print($"run {i}:");
                var (log, st) = Play(new Style("focused", Care.Tools, true, Focus: t), 5000 + i, Guild.Weeks * Guild.DaysPerWeek);
                GD.Print($"  -> cleared day {log.ClearedDay}, kept [{string.Join(",", st.CurioKeys)}]");
            }
            _traceNim = false;
            return;
        }
        if (tagName == "any")
        {
            // The careful collector, buying as they come: how often a build comes together by itself, and what it's worth.
            var all = Enumerable.Range(0, campaigns).AsParallel().Select(i => Play(new Style("careful", Care.Tools, true), 5000 + i, Guild.Weeks * Guild.DaysPerWeek)).ToList();
            int Most((Log Log, GameState State) r) => r.State.OwnedCurios.SelectMany(c => c.Tags.Take(1)).Where(t => t is not (CurioTag.Trade or CurioTag.Song))
                .GroupBy(t => t).Select(g => g.Count()).DefaultIfEmpty(0).Max();
            GD.Print($"=== The careful collector, buying as they come ({campaigns} runs): cleared {100f * all.Count(r => r.Log.ClearedDay > 0) / campaigns:0}% ===");
            for (int n = 0; n <= 5; n++)
            {
                var these = all.Where(r => Most(r) == n).ToList();
                if (these.Count > 0) GD.Print($"  most curios of one way of trading at the end {n}: {100f * these.Count / campaigns,3:0}% of runs, cleared {100f * these.Count(r => r.Log.ClearedDay > 0) / these.Count,3:0}%");
            }
            GD.Print("  their main way of trading (3+ kept): " + string.Join("  ", all.Where(r => Most(r) >= 3).GroupBy(r => r.State.OwnedCurios.Select(c => c.Tags[0])
                .Where(t => t is not (CurioTag.Trade or CurioTag.Song)).GroupBy(t => t).OrderByDescending(g => g.Count()).First().Key)
                .Select(g => $"{Curios.TagName(g.Key)} {g.Count()}")));
            return;
        }
        var tag = Enum.GetValues<CurioTag>().FirstOrDefault(t => t.ToString().Equals(tagName, StringComparison.OrdinalIgnoreCase) || Curios.TagName(t) == tagName);
        var style = tag == CurioTag.Blind ? new Style("sight unseen", Care.Unseen, true, Focus: tag) : new Style("focused", Care.Tools, true, Focus: tag);
        var runs = Enumerable.Range(0, campaigns).AsParallel().Select(i => Play(style, 5000 + i, Guild.Weeks * Guild.DaysPerWeek)).ToList();
        GD.Print($"=== Building around {Curios.TagName(tag)} ({campaigns} runs) ===");
        GD.Print($"cleared {100f * runs.Count(r => r.Log.ClearedDay > 0) / campaigns:0}%");
        for (int n = 0; n <= 5; n++)
        {
            var these = runs.Where(r => r.State.OwnedCurios.Count(c => c.Has(tag)) == n).ToList();
            if (these.Count > 0) GD.Print($"  {n} of its curios at the end: {100f * these.Count / campaigns,3:0}% of runs, cleared {100f * these.Count(r => r.Log.ClearedDay > 0) / these.Count,3:0}%");
        }
        GD.Print("  kept at the end (share of runs): " + string.Join("  ", Curios.All.Select(c => (c, n: runs.Count(r => r.State.HasCurio(c.Key))))
            .Where(x => x.n > 0).OrderByDescending(x => x.n).Take(16).Select(x => $"{x.c.Key} {100f * x.n / campaigns:0}%")));
        GD.Print("  ever bought (share of runs): " + string.Join("  ", Curios.All.Select(c => (c, n: runs.Count(r => r.Log.Curios.Contains(c.Key))))
            .Where(x => x.n > 0).OrderByDescending(x => x.n).Take(16).Select(x => $"{x.c.Key} {100f * x.n / campaigns:0}%")));
        GD.Print($"  curios kept at the end: {runs.Average(r => r.State.OwnedCurios.Count()):0.0} of {runs.Average(r => r.State.CurioSlots):0.0} slots");
        GD.Print("  seen on Nim's counter (share of runs): " + string.Join("  ", Curios.All.Where(c => c.Has(tag)).Select(c =>
            $"{c.Key} {100f * runs.Count(r => r.State.CuriosSeen.Contains(c.Key)) / campaigns:0}%")));
        GD.Print($"  reached Thornwick {100f * runs.Count(r => r.Log.ReachedTier[2] > 0) / campaigns:0}% (median day {Median(runs.Where(r => r.Log.ReachedTier[2] > 0).Select(r => (float)r.Log.ReachedTier[2])):0}), " +
                 $"Crownhold {100f * runs.Count(r => r.Log.ReachedTier[3] > 0) / campaigns:0}% (day {Median(runs.Where(r => r.Log.ReachedTier[3] > 0).Select(r => (float)r.Log.ReachedTier[3])):0})");
    }

    /// <summary>
    /// A quick look at a few curios (`--sim=curio --who=a,b`): each alone and all together, from when Nim could first have
    /// brought each (and from the first day), against the careful player with no curios, or for a sight-unseen curio the
    /// buyer who never shakes. For tuning one curio without the whole builds report.
    /// </summary>
    public static void CurioCheck(string[] keys, int campaigns)
    {
        var watch = Stopwatch.StartNew();
        keys = keys.Where(Curios.Exists).ToArray();
        var plain = new Style("careful, no curios", Care.Tools, true, Curios: false);
        var blind = new Style("blind", Care.Blind, false, Curios: false);
        bool Blind(string key) => Curios.Get(key).Tags[0] == CurioTag.Blind;
        var baseline = Measure(plain, campaigns);
        var blindBase = keys.Any(Blind) ? Measure(blind, campaigns) : null;
        GD.Print($"=== Curios ({campaigns} runs each; takings against the careful player with no curios) ===");
        GD.Print($"  {"careful, no curios",-40} takings {baseline.Takings,8:0}   cleared {baseline.Cleared,4:0}%");
        if (blindBase != null) GD.Print($"  {"never shakes, no curios",-40} {Row(blindBase, baseline)}");
        foreach (var key in keys)
        {
            var who = Blind(key) ? blind : plain;
            var curio = Curios.Get(key);
            GD.Print($"  {curio.Name + " (" + Curios.RarityName(curio.Rarity) + ")",-28} {"as Nim brings it",-11} {Row(Measure(who with { Given = new[] { key }, Arrives = true }, campaigns), baseline)}");
            GD.Print($"  {"",-28} {"from day 1",-11} {Row(Measure(who with { Given = new[] { key } }, campaigns), baseline)}");
        }
        if (keys.Length > 1)
        {
            var who = keys.Any(Blind) ? blind : plain;
            GD.Print($"  {string.Join("+", keys),-28} {"as Nim brings them",-11} {Row(Measure(who with { Given = keys, Arrives = true }, campaigns), baseline)}");
            GD.Print($"  {"",-28} {"from day 1",-11} {Row(Measure(who with { Given = keys }, campaigns), baseline)}");
        }
        GD.Print($"({watch.Elapsed.TotalSeconds:0}s)");
    }

    /// <summary>
    /// One careful collector's run, day by day (`--sim=trace --seed=N`): the shop, the purse, the customers, what was bought
    /// and earned, the upgrades and curios, the Guild. For tuning the economy's pace.
    /// </summary>
    public static void Trace(int seed, int days = Guild.Weeks * Guild.DaysPerWeek)
    {
        var style = Styles[^1];
        var state = GameState.NewRun(seed, style.Character, style.Challenge, guided: false);
        var log = new Log();
        var rng = new Random(seed);
        GD.Print($"Trace: the careful collector, seed {seed}");
        GD.Print("day wk tier   gold  cust bought   spent  earned  curios  upgrades / curios");
        for (int day = 1; day <= days; day++)
        {
            var owned = Upgrades.All.Where(u => state.Has(u.Key)).Select(u => u.Key).ToHashSet();
            var curios = state.CurioKeys.ToHashSet();
            int gold = state.Gold;
            int customers = 0;
            bool alive = PlayDay(state, style, rng, log);
            customers = state.Today.Bought + state.Today.Passed;
            var bought = Upgrades.All.Where(u => state.Has(u.Key) && !owned.Contains(u.Key)).Select(u => u.Key)
                .Concat(state.CurioKeys.Where(k => !curios.Contains(k)).Select(k => "+" + k));
            GD.Print($"{state.Day,3} {state.Week,2} {state.Tier,4} {gold,6} {customers,5} {state.Today.Bought,6} {state.Today.Spent,7} {state.Today.Earned,7} " +
                     $"{state.Today.Curios - state.Today.CurioSpent,7}  {string.Join(" ", bought)}" +
                     (state.DuesTonight ? $"   | dues {state.DueFor(state.Week)}{(alive ? "" : " LOST")}" : ""));
            if (!alive) break;
        }
    }

    static float Pct(Measured m, Measured baseline) => (float)(100 * m.Takings / Math.Max(1, baseline.Takings) - 100);

    /// <summary>Curios meant to go together (the design's combinations), for the pairs table.</summary>
    static readonly (string, string)[] Pairs =
    {
        ("draught", "ring"), ("chalk", "ring"), ("aether", "draught"), ("draught", "staff"),
        ("sealed", "dip"), ("dip", "bond"), ("sealed", "blindfold"),
        ("tongue", "coffer"), ("knot", "coffer"), ("signet", "knot"), ("tongue", "goldentongue"),
        ("seal", "register"), ("fork", "ledger"), ("ledger", "truthglass"), ("trumpet", "seal"),
        ("hook", "rod"), ("hook", "pass"), ("rod", "pass"), ("log", "hook"),
        ("pillow", "wardkey"), ("wardlamp", "wardkey"), ("pillow", "wardtally"),
        ("foot", "wishbone"), ("coin", "clover"), ("spindle", "star"),
        ("hourglass", "queue"), ("horn", "lantern"), ("jar", "lockbox"), ("letter", "ribbon"), ("whetstone", "gauntlet"),
    };
}
