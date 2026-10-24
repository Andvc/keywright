using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// A synergy in the collection journal: something the shop gets known for once you've acquired enough of something
/// (ten of a kind at a quality or better), or done something often enough (perfect reads in a row). Each has five
/// levels; only active synergies do anything, and the shop starts with room for one.
/// </summary>
public sealed class SynergyDef
{
    public string Key { get; init; }
    public string Name { get; init; }

    /// <summary>What it's about, in a few words.</summary>
    public string Blurb { get; init; }

    /// <summary>The kinds to acquire ten of (empty for achievement synergies).</summary>
    public Category[] Kinds { get; init; } = Array.Empty<Category>();

    /// <summary>For achievement synergies: what's needed for each level, and how far you've got.</summary>
    public int[] Steps { get; init; }
    public Func<GameState, int> Achieved { get; init; }
    public string StepWord { get; init; }

    /// <summary>What it does at a level (1 to 5).</summary>
    public Func<int, string> Effect { get; init; }

    /// <summary>The shop tier from which you could make progress (for the ledger: later ones are shown greyed).</summary>
    public int Tier { get; init; } = 1;

    public const int MaxLevel = 5;

    /// <summary>Of each kind, how many you need at the level's quality or better (ten until 0.20's shorter runs).</summary>
    public const int PerKind = 6;

    /// <summary>The level the shop has reached: 0 until the first is.</summary>
    public int Level(GameState state)
    {
        if (Kinds.Length > 0)
        {
            for (int level = MaxLevel; level >= 1; level--)
                if (Kinds.All(k => state.Acquired(k, (Rarity)(level - 1)) >= PerKind)) return level;
            return 0;
        }
        int have = Achieved(state);
        return Steps.Count(step => have >= step);
    }

    /// <summary>What the next level needs, and how far along you are: one line per kind, or one for an achievement.</summary>
    public IEnumerable<string> Progress(GameState state)
    {
        int next = Level(state) + 1;
        if (next > MaxLevel) yield break;
        if (Kinds.Length > 0)
        {
            var quality = (Rarity)(next - 1);
            foreach (var kind in Kinds)
                yield return $"{Math.Min(PerKind, state.Acquired(kind, quality))}/{PerKind} {Categories.Get(kind).Name.ToLowerInvariant()}" +
                             (quality == Rarity.Common ? "" : $", {Categories.QualityName(quality)} or better");
        }
        else yield return $"{Achieved(state)}/{Steps[next - 1]} {StepWord}";
    }
}

/// <summary>
/// What the shop gets known for. A name brings people and chances your way (more of a kind in the chests, smiths at the
/// door, false bottoms, enchanted things, luck), spares you some breakage, and makes buyers pay more for what you can
/// vouch for: the things you named (<see cref="Item.Named"/>). Five levels each, from what you've collected or done;
/// picking which to have active is a choice that shapes the run, and each feeds the way of trading that earned it.
/// </summary>
public static class Synergies
{
    public static readonly SynergyDef[] All =
    {
        new()
        {
            Key = "jewelry", Name = "Goldsmith", Blurb = "Coin and gems", Kinds = new[] { Category.Coins, Category.Gems }, Tier = 3,
            Effect = l => $"Moneychangers and jewellers come your way: coin and gems turn up {Pct(MoneyKinds(l))} more often, and those you name sell for {Pct(KindSales(l))} more.",
        },
        new()
        {
            Key = "smith", Name = "Armourer", Blurb = "Weapons", Kinds = new[] { Category.Arms },
            Effect = l => $"Soldiers, knights and dwarves come {Pct(SmithVisits(l) - 1)} more often, and weapons you name sell for {Pct(KindSales(l))} more.",
        },
        new()
        {
            Key = "vintner", Name = "Apothecary", Blurb = "Potions", Kinds = new[] { Category.Potions }, Tier = 2,
            Effect = l => (l >= SynergyDef.MaxLevel ? "Glass never breaks, dropped or forced" : $"Glass is {Pct(Sturdier(l))} less likely to break") +
                          $", and potions you name sell for {Pct(KindSales(l))} more.",
        },
        new()
        {
            Key = "archivist", Name = "Spellkeeper", Blurb = "Spells", Kinds = new[] { Category.Scrolls }, Tier = 2,
            Effect = l => $"People trust you with secrets: chests are {Pct(HiddenBonus(l))} likelier to have a false bottom, and spells you name sell for {Pct(KindSales(l))} more.",
        },
        new()
        {
            Key = "tailor", Name = "Cloakmaker", Blurb = "Cloaks", Kinds = new[] { Category.Garb },
            Effect = l => $"Fine drapery: your exhibition counts +{Pct(ShowBonus(l))}, and cloaks you name sell for {Pct(KindSales(l))} more.",
        },
        new()
        {
            Key = "keen", Name = "Keen Eye", Blurb = "Perfect reads in a row", Steps = new[] { 3, 5, 8, 12, 18 }, StepWord = "perfect reads in a row",
            Achieved = s => s.BestStreak,
            Effect = l => $"A perfect read earns you a valuer's fee: {Pct(KeenFee(l))} of what the chest held.",
        },
        new()
        {
            Key = "fortune", Name = "Lucky Shop", Blurb = "Surprises named", Steps = new[] { 3, 6, 10, 15, 22 }, StepWord = "surprises in things you named",
            Achieved = s => s.Surprises,
            Effect = l => $"Luck follows a lucky shop: everything you name is {Pct(LuckBonus(l))} likelier to come out a step better.",
        },
        new()
        {
            Key = "finder", Name = "Finder", Blurb = "False bottoms and wards", Steps = new[] { 2, 4, 7, 11, 16 }, StepWord = "hidden or warded things found", Tier = 2,
            Achieved = s => s.HiddenFound,
            Effect = l => $"Chests are {Pct(FinderHidden(l))} likelier to have a false bottom, and what you find under one or name in a ward sells for {Pct(FinderSales(l))} more.",
        },
        new()
        {
            Key = "haggler", Name = "Haggler", Blurb = "Chests talked down", Steps = new[] { 3, 6, 10, 15, 22 }, StepWord = "chests talked down or named a price for",
            Achieved = s => s.Bargains,
            Effect = l => $"Word of a hard bargainer: hagglers and sellers who wait for an offer settle for {Pct(HagglerFloor(l))} less.",
        },
        new()
        {
            Key = "arcane", Name = "Arcanist", Blurb = "Enchanted things found", Steps = new[] { 2, 5, 9, 14, 20 }, StepWord = "enchanted things", Tier = 3,
            Achieved = s => s.MagicFound,
            Effect = l => $"Mages seek you out: each thing is {Pct(MagicBonus(l))} likelier to be enchanted, and enchanted things you name sell for {Pct(KindSales(l))} more.",
        },
    };

    public static SynergyDef Get(string key) => All.First(s => s.Key == key);

    /// <summary>"12%", the same in every locale.</summary>
    public static string Pct(float fraction) => $"{MathF.Round(fraction * 100)}%";

    public static float MoneyKinds(int level) => 0.15f * level;
    public static float Sturdier(int level) => Math.Min(1f, 0.2f * level);
    public static float HiddenBonus(int level) => 0.05f * level;
    public static float ShowBonus(int level) => 0.25f * level;
    public static float KeenFee(int level) => 0.08f * level;
    public static float MagicBonus(int level) => 0.04f * level;
    public static float LuckBonus(int level) => 0.04f * level;
    public static float FinderHidden(int level) => 0.04f * level;
    public static float FinderSales(int level) => 0.25f * level;
    public static float HagglerFloor(int level) => 0.04f * level;

    /// <summary>What a name for a kind of thing puts on what you name of it, at a level.</summary>
    public static float KindSales(int level) => 0.2f * level;

    /// <summary>The synergies that put up what a thing of a kind sells for, by key (the Arcanist's is for enchanted things, below).</summary>
    static readonly (string Key, Category Kind)[] Trades =
    {
        ("jewelry", Category.Coins), ("jewelry", Category.Gems), ("smith", Category.Arms), ("vintner", Category.Potions),
        ("archivist", Category.Scrolls), ("tailor", Category.Garb),
    };

    /// <summary>What the active synergies put on the sale price of a thing of this kind you named.</summary>
    public static float SaleBonus(GameState state, Category kind) => Trades.Where(t => t.Kind == kind).Sum(t => KindSales(state.Active(t.Key)));

    /// <summary>And on an enchanted thing's, with the Arcanist active.</summary>
    public static float MagicSales(GameState state) => KindSales(state.Active("arcane"));

    /// <summary>How much more often soldiers, knights and dwarves (who bring a lot of weapons) come in.</summary>
    public static float SmithVisits(int level) => 1f + 0.25f * level;
}
