using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// A keepsake (0.32): something a tale's people give you to keep, not to sell. It works like a curio, with one effect of
/// its own, but it takes no curio slot, Nim never brings one and never buys one back, and the Guild won't take it on a
/// short night. Each comes from one tale, for seeing it through (docs/stories.md, "Keepsakes"). Sellers can't know of
/// one, so none is in the power index: what it does is your edge.
/// </summary>
/// <param name="Tale">The tale it comes from (its key).</param>
/// <param name="Icon">Its picture: a curio's or a thing's until it has its own (art/README).</param>
public sealed record KeepsakeDef(string Key, string Name, string Blurb, string Tale, string Icon);

public static class Keepsakes
{
    // What each one does, for the rules and the blurbs.
    public const float WeightsBonus = 1.2f, MedalBonus = 1.25f, HawthornLuck = 0.05f, SealDoor = 1.5f, ForkLuck = 0.1f;
    public const int ApprenticeCustomers = 1;

    public const string Weights = "fallowford", StallKey = "stallkey", Seal = "marchmont", Apprentice = "automaton", Lamp = "tobinlamp",
        Hawthorn = "hawthorn", Aldwin = "aldwin", Medal = "medal", Fork = "wrenfork", Oath = "oath";

    public static readonly KeepsakeDef[] All =
    {
        new(Weights, Loc.T("Fallowford's Weights"), Loc.T($"The drowned town's assayer's weights, true to the grain. Coin and gems sell for {Pct(WeightsBonus - 1)} more."),
            "hesk", "res://assets/ui/icons/scales.png"),
        new(StallKey, Loc.T("Mother's Stall Key"), Loc.T("The key to the Brightwaters' stall on Lantern Lane. Word's out that Rosa's fair: a seller who walks out on her comes back like anyone else."),
            "rosa", "res://assets/ui/curios/signet.png"),
        new(Seal, Loc.T("The Marchmont Seal"), Loc.T($"House Marchmont's seal, back on Isolde's hand. Her visitors pay {Pct(SealDoor - 1)} more at the door."),
            "isolde", "res://assets/ui/curios/seal.png"),
        new(Apprentice, Loc.T("The Brass Apprentice"), Loc.T($"Orme's automaton, minding the door and carrying chests: {Number(ApprenticeCustomers)} more customer every day."),
            "brass", "res://assets/sprites/items/icons/clockwork_lark.png"),
        new(Lamp, Loc.T("Tobin's Lamp"), Loc.T("Tobin digs under the Barrow for you now: on each week's first morning he brings a chest he's found, freely given, a colour finer than the shop's."),
            "door", "res://assets/ui/curios/lantern.png"),
        new(Hawthorn, Loc.T("The Hawthorn Charm"), Loc.T($"The fair folk's thanks for a promise kept. Everything you open is {Pct(HawthornLuck)} likelier to come out a step better."),
            "wood", "res://assets/sprites/items/icons/fey_chimes.png"),
        new(Aldwin, Loc.T("Sir Aldwin's Coin"), Loc.T("A knight's lucky coin, from a knight at rest. The first chest you buy each day: nothing in it comes out worse than its chest promised."),
            "field", "res://assets/ui/curios/coin.png"),
        new(Medal, Loc.T("Rafe Kettle's Medal"), Loc.T($"A soldier's medal from two wars, given by his widow. Weapons sell for {Pct(MedalBonus - 1)} more."),
            "kettle", "res://assets/ui/curios/gauntlet.png"),
        new(Oath, Loc.T("Sir Edric's Oath"), Loc.T("A knight of the old Crown has sworn to guard your door. The Wardens never search your back room, and the Lockless never rob you."),
            "sleeper", "res://assets/ui/icons/warrant.png"),
        new(Fork, Loc.T("Wren's Tuning Fork"), Loc.T($"The Keyhold choir's fork, from Sister Wren. Everything you open is {Pct(ForkLuck)} likelier to come out a step better."),
            "choir", "res://assets/ui/curios/fork.png"),
    };

    public static KeepsakeDef Get(string key) => All.First(k => k.Key == key);

    public static bool Exists(string key) => All.Any(k => k.Key == key);

    static string Pct(float share) => Synergies.Pct(share);

    static string Number(int n) => Sellers.Number(n);
}

/// <summary>The keepsakes a run holds (Keepsakes.cs): given by tales, kept for the season, saved with the run.</summary>
public sealed partial class GameState
{
    /// <summary>The keepsakes you hold, in the order you were given them.</summary>
    public List<string> Keepsakes { get; } = new();

    public bool Keeps(string key) => Keepsakes.Contains(key);

    /// <summary>A tale gives you something to keep: it's yours for the season, and the collection remembers it.</summary>
    public void GiveKeepsake(string key)
    {
        if (!Core.Keepsakes.Exists(key) || Keeps(key)) return;
        Keepsakes.Add(key);
        KeepsakesSeen.Add(key);
    }

    /// <summary>Taken back by whoever gave it (a promise broken).</summary>
    public void TakeKeepsake(string key) => Keepsakes.Remove(key);

    /// <summary>Every keepsake any run of yours has held (the profile's, merged in by <see cref="Remember"/>), for the collection.</summary>
    public HashSet<string> KeepsakesSeen { get; } = new();
}
