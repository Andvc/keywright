using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// One way of checking a chest that tells you something new: shaking it at a strength (with the lodestone on or off),
/// or dropping it. Each costs a listen the first time it's done to a chest (<see cref="GameState.Listen"/>).
/// </summary>
public readonly record struct Check(Shake Level, bool Magnet = false, bool Drop = false)
{
    public static Check Shaking(Shake level, bool magnet = false) => new(level, magnet);

    public static readonly Check Dropping = new(Shake.Still, false, true);

    /// <summary>As a visit remembers it (<see cref="Visit.Checks"/>).</summary>
    public string Key => Drop ? "drop" : Magnet ? $"{(int)Level}m" : $"{(int)Level}"; // loc: keys
}

/// <summary>
/// The ear (0.36). A keywright's ear is good for so many careful listens a day: each new check on a chest spends one
/// (a strength not yet tried on it, with the lodestone or without; a drop), and shaking again the way you already have
/// is free, so counting rattles costs nothing. With none left you trade on what you know: who's selling, how they
/// priced it, where it came from, the chest's colour.
///
/// Until 0.35 every check was free, so the right play was always all of them (gently, firmly, hard, the drop), and a
/// careful buyer profited on nine chests in ten: a procedure, not a decision, and an opening that confirmed the slip.
/// With listens to spend, what a seller misses is what tells you where to spend one (Ada priced what a gentle shake
/// stirs: one hard shake hears everything she didn't; a clank at a hard shake means the delicate things are drowned,
/// and a gentle shake is worth another), a day's queue is something to plan, and a chest bought on less than
/// everything is a real reveal. A rule people know: nobody tastes the fortieth wine as well as the first.
/// </summary>
public sealed partial class GameState
{
    /// <summary>Listens a day, before the shop's kettle and booth and what curios add.</summary>
    public const int ListensBase = 8;

    /// <summary>Listens spent today.</summary>
    public int ListensUsed { get; private set; }

    /// <summary>Listens a day: the ear's own, and what the shop and the build add.</summary>
    public int Listens => Math.Max(1, (ListensTried > 0 ? ListensTried : ListensBase) + Upgrades.All.Where(u => Has(u.Key)).Sum(u => u.Listens));

    /// <summary>For tuning (`--sim=quick --listens=N`): another ear to start with.</summary>
    public static int ListensTried { get; set; }

    public int ListensLeft => Math.Max(0, Listens - ListensUsed);

    /// <summary>
    /// Whether listening to this chest costs nothing: a seller's lesson (Bram's teaching, and its steps ask for
    /// particular shakes), or the Bookkeeping Engine's reading it for you.
    /// </summary>
    public bool ListensFree(Visit visit) => visit == null || visit.Lesson != null || visit.FreeListens || Has(Upgrades.Engine);

    /// <summary>This check's already been made on this chest: making it again costs nothing.</summary>
    public static bool Listened(Visit visit, Check check) => visit.Checks.Contains(check.Key);

    /// <summary>Whether this check could be made now: made already, free, or a listen left to spend on it.</summary>
    public bool CanListen(Visit visit, Check check) => Listened(visit, check) || ListensFree(visit) || ListensLeft > 0;

    /// <summary>
    /// Makes a check on a chest: spends a listen the first time (unless listening to it is free). False when there's
    /// none left: the check tells you nothing.
    /// </summary>
    public bool Listen(Visit visit, Check check)
    {
        if (visit == null) return false;
        if (Listened(visit, check)) return true;
        if (!ListensFree(visit))
        {
            if (ListensLeft <= 0) return false;
            ListensUsed++;
        }
        visit.Checks.Add(check.Key);
        return true;
    }

    /// <summary>A fresh ear, as each morning brings (and, for the sim's tables of chests, one chest after another).</summary>
    public void RestEar() => ListensUsed = 0;

    /// <summary>A listen back (an echo stone, for a perfect read): never more than the day began with.</summary>
    public void RestoreListen(int listens = 1) => ListensUsed = Math.Max(0, ListensUsed - listens);

    /// <summary>
    /// What a careful player allows this chest of what's left: its share among everyone still to come, with what earlier
    /// chests didn't need carried forward; one at least while there's any (<see cref="Strategy.NextCheck"/> spends it).
    /// </summary>
    public int ListenShare(Visit visit)
    {
        if (ListensFree(visit)) return int.MaxValue;
        int waiting = 1 + Queue.Count(v => v.Talk == null && !ListensFree(v));
        return ListensLeft <= 0 ? 0 : Math.Max(1, (int)Math.Ceiling(ListensLeft / (double)waiting));
    }
}
