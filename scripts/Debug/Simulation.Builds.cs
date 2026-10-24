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
        // The careful collector, buying nothing from Nim: what one curio, or a build, adds to that.
        var plain = new Style("careful, no curios", Care.Tools, true, Curios: false);
        var baseline = Measure(plain, campaigns);
        GD.Print($"=== Builds ({campaigns} runs each, {Guild.Weeks} weeks of {Guild.DaysPerWeek} days; takings to the eighth week, against the careful " +
                 "collector with no curios) ===");
        GD.Print($"baseline: takings {baseline.Takings:0}, cleared {baseline.Cleared:0}%, day {baseline.ClearDay}");

        GD.Print("\nEach curio on its own, from the first day:");
        var alone = new Dictionary<string, Measured>();
        foreach (var curio in Curios.All)
        {
            var m = alone[curio.Key] = Measure(plain with { Given = new[] { curio.Key } }, campaigns);
            GD.Print($"  {curio.Name,-24} {Curios.RarityName(curio.Rarity),-9} {Curios.Tags(curio),-24} {Row(m, baseline)}");
        }

        // Each way of trading: its three strongest curios from the first day, and a careful player who buys for it from Nim.
        GD.Print("\nBuilds: the three strongest of each way of trading from the first day; then bought from Nim for that way of trading");
        var builds = new Dictionary<CurioTag, string[]>();
        foreach (var tag in Enum.GetValues<CurioTag>())
        {
            var best = Curios.All.Where(c => c.Has(tag)).OrderByDescending(c => alone[c.Key].Takings).Take(3).Select(c => c.Key).ToArray();
            if (best.Length < 2) continue;
            builds[tag] = best;
            var given = Measure(plain with { Given = best }, campaigns);
            var bought = Measure(new Style("focused", Care.Tools, true, Focus: tag), campaigns);
            GD.Print($"  {Curios.TagName(tag),-12} {string.Join("+", best),-28} {Row(given, baseline)}");
            GD.Print($"  {"",-12} {"bought for it",-28} {Row(bought, baseline)}");
        }
        var open = Measure(new Style("careful", Care.Tools, true), campaigns);
        GD.Print($"  {"(any)",-12} {"bought as they come",-28} {Row(open, baseline)}");

        // Pairs that should be worth more together: what the pair adds beyond what each adds alone.
        GD.Print("\nPairs: what each adds alone, together, and the difference (positive: they're worth more together)");
        foreach (var (a, b) in Pairs)
        {
            var both = Measure(plain with { Given = new[] { a, b } }, campaigns);
            float ga = Pct(alone[a], baseline), gb = Pct(alone[b], baseline), gab = Pct(both, baseline);
            GD.Print($"  {a,-10} + {b,-10} {ga,5:+0;-0}% {gb,5:+0;-0}%  together {gab,5:+0;-0}%   synergy {gab - ga - gb,5:+0;-0}  " +
                     $"(cleared {alone[a].Cleared:0}% / {alone[b].Cleared:0}% / {both.Cleared:0}%)");
        }

        // No build makes buying blind pay: a buyer who doesn't listen, with each build, is still foreclosed.
        GD.Print("\nBuying blind with each build (must still be foreclosed):");
        var blind = new Style("blind", Care.Blind, false, Curios: false);
        GD.Print($"  {"no curios",-40} {Row(Measure(blind, campaigns), baseline)}");
        foreach (var (tag, build) in builds)
            GD.Print($"  {Curios.TagName(tag) + ": " + string.Join("+", build),-40} {Row(Measure(blind with { Given = build }, campaigns), baseline)}");
        GD.Print($"\n({watch.Elapsed.TotalSeconds:0}s)");
    }

    /// <summary>
    /// One careful collector's run, day by day (`--sim=trace --seed=N`): the shop, the purse, the customers, what was bought
    /// and earned, the upgrades and curios, the Guild. For tuning the economy's pace.
    /// </summary>
    public static void Trace(int seed, int days = Guild.Weeks * Guild.DaysPerWeek)
    {
        var style = Styles[^1];
        var state = GameState.NewRun(seed, style.Character, style.Challenge, style.Talents, guided: false);
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
        ("foot", "wishbone"), ("foot", "star"), ("foot", "horseshoe"), ("coin", "clover"), ("token", "gauntlet"), ("token", "star"),
        ("hourglass", "queue"), ("queue", "bell"), ("tongue", "coffer"), ("rope", "book"), ("rope", "lamps"), ("dummy", "rope"),
        ("fork", "ledger"), ("seal", "fork"), ("beads", "rod"), ("pillow", "wardkey"), ("draught", "ring"), ("aether", "ring"),
        ("horn", "lantern"), ("horn", "guestbook"), ("jar", "lockbox"), ("letter", "ribbon"), ("ribbon", "errand"), ("whetstone", "gauntlet"),
    };
}
