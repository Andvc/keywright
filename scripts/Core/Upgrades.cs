using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <param name="MinTier">The shop tier needed before it's on offer.</param>
/// <param name="SetsTier">For renovations: the shop tier it moves you to.</param>
/// <param name="Slots">For display cabinets: how many things it shows.</param>
/// <param name="Icon">Icon under res://assets/ui/icons/.</param>
/// <param name="SynergySlots">How many more synergies it lets you have active.</param>
/// <param name="CurioSlots">How many more curios it gives you room for.</param>
/// <param name="Customers">How many more customers come every day.</param>
/// <param name="Commissions">How many more commissions the regulars post each week.</param>
/// <param name="Requires">An upgrade that has to be bought first (the cabinet before it, the charter before the seal).</param>
/// <param name="Sales">How much more everything sells for (a buyers' trade: they multiply together).</param>
/// <param name="Luck">How much likelier everything you open is to come out a step better (a restorer: they add up).</param>
public sealed record Upgrade(string Key, string Name, string Blurb, int Cost, int MinTier, string Icon, int SetsTier = 0, int Slots = 0,
    int SynergySlots = 0, int CurioSlots = 0, int Customers = 0, int Commissions = 0, string Requires = null, float Sales = 0, float Luck = 0)
{
    public bool IsRenovation => SetsTier > 0;
}

/// <summary>
/// What you can spend your earnings on. Moves (renovations) bring richer customers and new things to learn; tools each answer
/// one question your ears can't; cabinets show off what you keep, which draws more and better customers; a buyers' trade
/// (scales, an auction block, a royal warrant) puts up what everything sells for, and a restorer's bench makes what you
/// name likelier to come out better than its chest. Each is a real step up: the Guild's dues climb fast enough to need
/// them. Sellers hear about the trades, and ask more of a shop that has them (<see cref="GameState.Buyers"/>); a
/// restorer works from your slip, which no seller sees.
/// </summary>
public static class Upgrades
{
    public static readonly Upgrade[] All =
    {
        new("shop2", "Move to Thornwick", "A shop at Thornwick Market: 5 customers a day, better chests, locks, and two new sounds: potions and spells.",
            50, 1, "shop", SetsTier: 2),

        new("sign", "Painted Sign", "One more customer every day.", 60, 2, "sign", Customers: 1),
        new("scales", "Brass Scales", "Honest weights draw buyers who pay for them: everything sells for a fifth more.", 150, 2, "scales", Sales: 0.2f),
        new("cabinet1", "Oak Cabinet", "Room to display one more thing. Displays draw customers.", 80, 2, "cabinet", Slots: 1),
        new("picks", "Fine Lockpicks", "A wider sweet spot when you pick a lock.", 80, 2, "picks"),
        new("cabinet2", "Oak Dresser", "Room to display one more thing.", 160, 2, "cabinet", Slots: 1, Requires: "cabinet1"),
        new("bench", "Restorer's Bench", "What you name gets cleaned and mended: each thing is 5% likelier to come out a step better than its chest.",
            200, 2, "bench", Luck: 0.05f),
        new("shop3", "Move to Crownhold", "A shop in the capital: richer customers, archmages with enchanted things, a dwarf, and gems.",
            160, 2, "shop", SetsTier: 3),

        new("lamp", "Witchlight", "Always lit: anything enchanted glows when it makes a sound. Nobody else can see it.", 400, 3, "lamp"),
        new("crier", "Town Crier", "Your name, shouted across Crownhold: one more customer every day.", 700, 3, "crier", Customers: 1),
        new("block", "Auction Block", "Buyers bid against each other for what you sell: everything fetches 30% more.", 1200, 3, "block", Sales: 0.3f),
        new("cabinet3", "Glass Cabinet", "Room to display one more thing.", 400, 3, "cabinet", Slots: 1, Requires: "cabinet2"),
        new("charter", "Crownhold Charter", "Two synergies at once.", 600, 3, "charter", SynergySlots: 1),
        new("case", "Curio Case", "Room for one more curio.", 500, 3, "case", CurioSlots: 1),
        new("cabinet4", "Glass Case", "Room to display one more thing.", 700, 3, "cabinet", Slots: 1, Requires: "cabinet3"),
        new("workshop", "Restorer's Workshop", "A workshop and a journeyman: what you name is another 6% likelier to come out a step better.", 1500, 3, "bench",
            Requires: "bench", Luck: 0.06f),
        new("shop4", "The Royal Emporium", "By the Gilded Gate: 5 customers a day, the court, demons, and a fence for the Maw.", 1200, 3, "shop",
            SetsTier: 4),

        new("magnet", "Lodestone", "Iron can't move near it: no clanking, and demon gold goes quiet.", 1000, 4, "magnet"),
        new("cabinet5", "Gilded Cabinet", "Room to display one more thing.", 1500, 4, "cabinet", Slots: 1, Requires: "cabinet4"),
        new("seal", "Royal Seal", "Three synergies at once.", 2500, 4, "seal", SynergySlots: 1, Requires: "charter"),
        new("cabinet6", "Gilded Case", "Room to display one more thing.", 2500, 4, "cabinet", Slots: 1, Requires: "cabinet5"),

        // The Royal Emporium's own: big sums to grow into while the Guild's dues climb.
        new("night", "Night Market", "Open after dark: one more customer every day.", 6000, 4, "night", Customers: 1),
        new("warrant", "Royal Warrant", "By appointment to the Crown: everything sells for half as much again.", 12000, 4, "warrant", Sales: 0.5f),
        new("patrons", "Patrons' Book", "The regulars post two more commissions a week.", 8000, 4, "patrons", Commissions: 2),
        new("master", "Master Restorer", "A master of the craft, from the palace workshops: what you name is another 8% likelier to come out a step better.",
            15000, 4, "bench", Requires: "workshop", Luck: 0.08f),
        new("velvet", "Velvet Case", "Room for one more curio.", 10000, 4, "case", CurioSlots: 1, Requires: "case"),

        // The Guild's own counting-house clockwork, sold for exactly what you still owe (GameState.Cost): on offer once you could pay it.
        new(Engine, "Bookkeeping Engine", "The Guild's clockwork reads every chest on your counter and ticks the slip for you. " +
            "Costs all you owe.", 0, 1, "engine"),
    };

    public const string Engine = "engine";

    public static Upgrade Get(string key) => All.First(u => u.Key == key);

    /// <summary>
    /// What's on offer now: nothing until the stall has earned its keep; then the right tier, not owned yet, only the
    /// next renovation, and only once what it builds on is bought (one cabinet after another, the charter before the seal).
    /// </summary>
    public static IEnumerable<Upgrade> Available(GameState state) => !state.UpgradesOpen ? Enumerable.Empty<Upgrade>() :
        All.Where(u => u.MinTier <= state.Tier && !state.Has(u.Key) && (!u.IsRenovation || u.SetsTier == state.Tier + 1) &&
                       (u.Requires == null || state.Has(u.Requires)) && (u.Key != Engine || state.EngineOffered));
}
