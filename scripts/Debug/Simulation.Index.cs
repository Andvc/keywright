using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Debug;

/// <summary>
/// The power index report (`--sim=index`): what a typical chest costs at each shop, what its chests hold kind by kind,
/// and what the shop's index and working purse come to with the ordinary upgrades a careful player owns there, and with
/// each curio on top. For tuning the prices and the reserve warnings.
/// </summary>
public static partial class Simulation
{
    public static void IndexReport()
    {
        GD.Print("=== The shop's power index ===");
        GD.Print("tier  colour  typical ask  shares (value)");
        for (int tier = 1; tier <= ShopTiers.Max; tier++)
            GD.Print($"{tier,4}  {Categories.ChestSkin(ShopTiers.Typical(tier)),-6}  {Market.TypicalAsk(tier),11}g  " +
                     string.Join("  ", Market.ValueShares(tier).Select(s => $"{Categories.Get(s.Kind).Name.ToLowerInvariant()} {100 * s.Share:0}%")));

        GD.Print("\nWhat the crowd asks at plain prices (10th / 25th / median / 75th / 90th percentile):");
        for (int tier = 1; tier <= ShopTiers.Max; tier++)
        {
            var rng = new Random(77 + tier);
            var crowd = Sellers.All.Where(s => Market.Weight(s, tier) > 0 && !s.SlipPrice && s.Gaze <= 0).ToArray();
            var asks = Enumerable.Range(0, 400).Select(_ => Market.CreateVisit(rng, rng.PickWeighted(crowd, s => Market.Weight(s, tier)), tier).Price)
                .OrderBy(p => p).ToList();
            GD.Print($"{tier,4}  " + string.Join(" / ", new[] { 0.1, 0.25, 0.5, 0.75, 0.9 }.Select(q => $"{asks[(int)(q * asks.Count)]}g")));
        }

        GD.Print("\nWith the ordinary upgrades of each shop (what --start-tier owns), then all of them:");
        GD.Print("tier  index  price factor  working purse   (trades x restorers)");
        for (int tier = 1; tier <= ShopTiers.Max; tier++)
        {
            var state = GameState.NewRun(1, guided: false);
            state.SkipTo(tier, 1000);
            GD.Print($"{tier,4}  {state.PowerIndex,5:0.00}  {state.PriceFactor,12:0.00}  {state.WorkingPurse,13}g   ({state.Buyers:0.00} x {1 + 2 * state.RestorerLuck:0.00})");
        }
        var full = GameState.NewRun(1, guided: false);
        full.SkipTo(ShopTiers.Max, 1000);
        foreach (var u in Upgrades.All.Where(u => u.Key != Upgrades.Engine)) full.Grant(u.Key);
        GD.Print($"   4* {full.PowerIndex,5:0.00}  {full.PriceFactor,12:0.00}  {full.WorkingPurse,13}g   (every upgrade)");

        GD.Print("\nEach curio's share of the index at the Royal Emporium with every upgrade (what sellers price in):");
        float baseIndex = full.PowerIndex;
        foreach (var curio in Curios.All)
        {
            float with = full.IndexIf(curio: curio);
            if (Math.Abs(with - baseIndex) > 0.001f)
                GD.Print($"  {curio.Name,-24} {100 * (with / baseIndex - 1),6:+0.0;-0.0}%");
        }
    }
}

public static partial class Simulation
{
    /// <summary>Every curio as a markdown table for the design doc (`--sim=curiolist`): by way of trading, rarity, shop, what opens it, what it does.</summary>
    public static void CurioTable()
    {
        GD.Print("| Way of trading | Curio | | Does | Opens |");
        GD.Print("|---|---|---|---|---|");
        var order = Curios.Main.Concat(Enum.GetValues<CurioTag>().Where(t => !Curios.Main.Contains(t))).ToList();
        foreach (var group in Curios.All.GroupBy(c => c.Tags[0]).OrderBy(g => order.IndexOf(g.Key)))
        {
            bool first = true;
            foreach (var c in group.OrderBy(c => c.Rarity).ThenBy(c => c.MinTier))
            {
                string rarity = c.Rarity.ToString()[..1] + (c.MinTier > 1 ? $", T{c.MinTier}" : "");
                GD.Print($"| {(first ? Curios.TagName(group.Key) : "")} | {c.Name} | {rarity} | {c.Blurb} | {Curios.UnlockText(c)} |");
                first = false;
            }
        }
    }
}

public static partial class Simulation
{
    /// <summary>Where the shop's power index stood when a chest was bought, for the margins report: these are the bands' tops.</summary>
    static readonly float[] IndexBands = { 1.05f, 1.3f, 1.7f, 2.2f };

    static int Band(float index)
    {
        int band = Array.FindIndex(IndexBands, top => index < top);
        return band < 0 ? IndexBands.Length : band;
    }

    static string BandName(int band) => band == 0 ? $"<{IndexBands[0]:0.00}" : band < IndexBands.Length ? $"<{IndexBands[band]:0.0}" : $"{IndexBands[^1]:0.0}+";

    /// <summary>
    /// What a gold spent on chests brings back (held / paid, curios' and settlements' takings in), by the shop's power
    /// index when each was bought: the careful collector, the same without curios, and the buyer who pays blind. If the
    /// index does its job, the careful player's margin holds as the index climbs and the blind buyer's stays under one.
    /// </summary>
    static void PrintMargins(List<Log> blind, List<Log> careful, int campaigns)
    {
        var plain = Enumerable.Range(0, campaigns).Select(i => Play(new Style("careful, no curios", Care.Tools, true, Curios: false), 1000 + i).Log).ToList();
        GD.Print("\nMargins: what a gold spent on chests brought back, by the shop's power index when bought (and the share of chests bought there)");
        GD.Print($"{"",-24}" + string.Join("", Enumerable.Range(0, IndexBands.Length + 1).Select(b => $"{BandName(b),16}")));
        foreach (var (name, logs) in new[] { ("careful collector", careful), ("careful, no curios", plain), ("buys at their price", blind) })
        {
            int bought = Math.Max(1, logs.Sum(l => l.BoughtAt.Sum()));
            string Cell(int b)
            {
                double spent = logs.Sum(l => l.SpentAt[b]);
                int n = logs.Sum(l => l.BoughtAt[b]);
                return n < 20 ? $"{"-",16}" : $"{logs.Sum(l => l.HeldAt[b]) / Math.Max(1, spent),9:0.00} ({100f * n / bought,3:0}%)";
            }
            GD.Print($"  {name,-22}" + string.Join("", Enumerable.Range(0, IndexBands.Length + 1).Select(Cell)));
        }
    }
}
