using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <param name="MinTier">The shop tier needed before it's on offer.</param>
/// <param name="SetsTier">For renovations: the shop tier it moves you to.</param>
/// <param name="Slots">For display cabinets: how many things it shows.</param>
/// <param name="SynergySlots">How many more synergies it lets you have active.</param>
/// <param name="CurioSlots">How many more curios it gives you room for.</param>
/// <param name="Customers">How many more customers come every day.</param>
/// <param name="Commissions">How many more commissions the regulars post each week.</param>
/// <param name="Requires">An upgrade that has to be bought first (the cabinet before it, the charter before the seal).</param>
/// <param name="Sales">How much more everything sells for (a buyers' trade: they multiply together).</param>
/// <param name="Luck">How much likelier everything you open is to come out a step better (a restorer: they add up).</param>
/// <param name="By">Who brings it to the counter to offer it (<see cref="Offers"/>; Nim at his table), or null for the day's end.</param>
/// <param name="NimCards">How many more curios Nim lays out a night.</param>
public sealed record Upgrade(string Key, string Name, string Blurb, int Cost, int MinTier, int SetsTier = 0, int Slots = 0,
    int SynergySlots = 0, int CurioSlots = 0, int Customers = 0, int Commissions = 0, string Requires = null, float Sales = 0, float Luck = 0,
    string By = null, int NimCards = 0)
{
    public bool IsRenovation => SetsTier > 0;

    /// <summary>Bought from someone at the counter (or Nim), not at the day's end.</summary>
    public bool Offered => By != null;
}

/// <summary>
/// What you can spend your earnings on. Moves (renovations) bring richer customers and new things to learn; tools each answer
/// one question your ears can't; cabinets show off what you keep, which draws more and better customers; a buyers' trade
/// (scales, an auction block, a royal warrant) puts up what everything sells for, and a restorer's bench makes everything
/// you buy likelier to come out better than its chest. Each is a real step up: the Guild's dues climb fast enough to need
/// them. Sellers hear about the trades and the restorers, and ask more of a shop that has them (the shop's power index,
/// <see cref="GameState.PowerIndex"/>), so they make the numbers bigger, not a bad buy good.
/// </summary>
public static class Upgrades
{
    public static readonly Upgrade[] All =
    {
        new("shop2", Loc.T("Move to Thornwick"), Loc.T("A shop at Thornwick Market: 6 customers a day, better chests, locks, and two new sounds: potions and spells."),
            50, 1, SetsTier: 2),

        new("sign", Loc.T("Painted Sign"), Loc.T("One more customer every day."), 60, 2, Customers: 1),
        new("scales", Loc.T("Brass Scales"), Loc.T("Honest weights, Warden-stamped, draw buyers who pay for them: everything sells for a fifth more."), 150, 2, Sales: 0.2f, By: "hollis"),
        new("cabinet1", Loc.T("Oak Cabinet"), Loc.T("Room to display one more thing. Displays draw customers."), 80, 2, Slots: 1),
        new("picks", Loc.T("Fine Lockpicks"), Loc.T("Goblin picks: a wider sweet spot when you pick a lock."), 80, 2, By: "nettle"),
        new("cabinet2", Loc.T("Oak Dresser"), Loc.T("Room to display one more thing."), 160, 2, Slots: 1, Requires: "cabinet1"),
        new("bench", Loc.T("Restorer's Bench"), Loc.T("Everything you buy gets cleaned and mended: each thing is 5% likelier to come out a step better than its chest."),
            200, 2, Luck: 0.05f, By: "cadoc"),
        new("shop3", Loc.T("Move to Crownhold"), Loc.T("A shop in the capital: richer customers, archmages with enchanted things, a dwarf, and gems."),
            160, 2, SetsTier: 3),

        new("lamp", Loc.T("Witchlight"), Loc.T("Always lit: anything enchanted glows when it makes a sound. Nobody else can see it."), 400, 3, By: "ilse"),
        new("crier", Loc.T("Town Crier"), Loc.T("Your name, shouted across Crownhold: one more customer every day."), 700, 3, Customers: 1),
        new("block", Loc.T("Auction Block"), Loc.T("Buyers bid against each other for what you sell: everything fetches 30% more."), 1200, 3, Sales: 0.3f, By: "brass"),
        new("cabinet3", Loc.T("Glass Cabinet"), Loc.T("Room to display one more thing."), 400, 3, Slots: 1, Requires: "cabinet2"),
        new("charter", Loc.T("Crownhold Charter"), Loc.T("Two synergies at once."), 600, 3, SynergySlots: 1),
        new("case", Loc.T("Curio Case"), Loc.T("Room for one more curio."), 500, 3, CurioSlots: 1, By: Story.Nim),
        new("cabinet4", Loc.T("Glass Case"), Loc.T("Room to display one more thing."), 700, 3, Slots: 1, Requires: "cabinet3"),
        new("workshop", Loc.T("Restorer's Workshop"), Loc.T("A workshop and a journeyman: everything is another 6% likelier to come out a step better."), 1500, 3,
            Requires: "bench", Luck: 0.06f, By: "durgan"),
        new("shop4", Loc.T("The Royal Emporium"), Loc.T("By the Gilded Gate: 5 customers a day, the court, demons, and a fence for the Maw."), 1200, 3,
            SetsTier: 4),

        new("magnet", Loc.T("Lodestone"), Loc.T("Iron can't move near it: no clanking, and demon gold goes quiet."), 1000, 4, By: "durgan"),
        new("cabinet5", Loc.T("Gilded Cabinet"), Loc.T("Room to display one more thing."), 1500, 4, Slots: 1, Requires: "cabinet4"),
        new("seal", Loc.T("Royal Seal"), Loc.T("Three synergies at once."), 2500, 4, SynergySlots: 1, Requires: "charter"),
        new("cabinet6", Loc.T("Gilded Case"), Loc.T("Room to display one more thing."), 2500, 4, Slots: 1, Requires: "cabinet5"),

        // The Royal Emporium's own: big sums to grow into while the Guild's dues climb.
        new("night", Loc.T("Night Market"), Loc.T("Open after dark: one more customer every day."), 6000, 4, Customers: 1, By: "gilt"),
        new("warrant", Loc.T("Royal Warrant"), Loc.T("By appointment to the Crown: everything sells for half as much again."), 12000, 4, Sales: 0.5f, By: "aldo"),
        new("patrons", Loc.T("Patrons' Book"), Loc.T("The regulars post two more commissions a week."), 8000, 4, Commissions: 2, By: "zell"),
        new("master", Loc.T("Master Restorer"), Loc.T("A master of the craft, from the palace workshops: everything is another 8% likelier to come out a step better."),
            15000, 4, Requires: "workshop", Luck: 0.08f, By: "vey"),
        new("velvet", Loc.T("Velvet Case"), Loc.T("Room for one more curio."), 10000, 4, CurioSlots: 1, Requires: "case", By: Story.Nim),

        // Nim's own: each lays out one more curio at his table every night (he offers them himself).
        new("lantern", Loc.T("Nim's Lantern"), Loc.T("A lantern over the back door, so Nim knows he's welcome: one more curio on his table every night."),
            40, 1, NimCards: 1, By: Story.Nim),
        new("stool", Loc.T("A Stool for Nim"), Loc.T("Somewhere to sit by the counter: Nim stays longer, and lays out one more curio a night."),
            300, 2, NimCards: 1, Requires: "lantern", By: Story.Nim),
        new("order", Loc.T("Nim's Standing Order"), Loc.T("A standing order: Nim keeps the best of his pack for you, one more curio a night."),
            2500, 3, NimCards: 1, Requires: "stool", By: Story.Nim),

        // The Guild's own counting-house clockwork, sold for exactly what you still owe (GameState.Cost): on offer once you could pay it.
        new(Engine, Loc.T("Bookkeeping Engine"), Loc.T("The Guild's clockwork reads every chest on your counter and ticks the slip for you. " +
            "Costs all you owe."), 0, 1),
    };

    public const string Engine = "engine";

    public static Upgrade Get(string key) => All.First(u => u.Key == key);

    /// <summary>
    /// What's on offer now: nothing until the stall has earned its keep; then the right tier, not owned yet, only the
    /// next renovation, and only once what it builds on is bought (one cabinet after another, the charter before the seal).
    /// </summary>
    public static IEnumerable<Upgrade> Available(GameState state) => Open(state).Where(u => !u.Offered);

    /// <summary>Everything that could be bought now, wherever it's sold: at the day's end, from someone at the counter, or from Nim.</summary>
    public static IEnumerable<Upgrade> Open(GameState state) => !state.UpgradesOpen ? Enumerable.Empty<Upgrade>() :
        All.Where(u => u.MinTier <= state.Tier && !state.Has(u.Key) && (!u.IsRenovation || u.SetsTier == state.Tier + 1) &&
                       (u.Requires == null || state.Has(u.Requires)) && (u.Key != Engine || state.EngineOffered));

    /// <summary>What Nim offers at his table tonight: the next of his own, and a case for your curios.</summary>
    public static IEnumerable<Upgrade> AtNim(GameState state) => Open(state).Where(u => u.By == Story.Nim);
}
