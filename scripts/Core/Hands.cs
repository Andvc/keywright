using System;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// What a build takes off your hands at the counter (0.37; docs/design.md, "Builds that do the routine for you").
/// Every chest asks for the same motions: shake it gently, firmly and hard, drop it, tick what you heard, star what
/// glowed, work out what a sign points at, guess what a haggler would take. In a first week that's the game being
/// learned; by the third it's a routine, and 0.36 tried to break it by rationing listens, which only made the routine
/// costlier (the owner: "restricting how many times players can listen is not the solution"). So the routine itself
/// goes, a piece at a time, to the build: each way of trading has a curio that does its part of it for you, and the
/// shaking itself goes to Bram's cradle once you've learned what the strengths tell. A build is then not only which
/// chests pay, but what the counter asks of you.
/// <para>
/// These are the first curios that tell you something of what a chest holds, which the rule used to forbid: they're
/// rare, they're what a build is for, and none says what a chest is worth (how good each thing is stays the opening's).
/// Buying sight unseen has none: it never touches the chest to begin with.
/// </para>
/// </summary>
public sealed partial class GameState
{
    /// <summary>
    /// Every chest is read as it comes in, its contents on the slip before a hand's laid on it: the Guild's Bookkeeping
    /// Engine, or the tinkers' Pocket Engine (the perfect read's rarest curio).
    /// </summary>
    public bool ReadsChests => Has(Upgrades.Engine) || HasCurio("engine");

    /// <summary>A Scrivener's Quill ticks each thing on the slip as it's heard, counted (the perfect read's).</summary>
    public bool TicksHeard => HasCurio("quill");

    /// <summary>A Lantern Moth stars every enchanted thing in a chest as it comes in, witchlight or not (enchantment's).</summary>
    public bool MarksMagic => HasCurio("moth");

    /// <summary>A Toll-man's Gauge marks a false bottom as the chest comes in, with no drop (false bottoms').</summary>
    public bool MarksHidden => HasCurio("gauge");

    /// <summary>The Wardens' Primer names the warded thing a sign points at (the wards').</summary>
    public bool NamesWards => HasCurio("primer");

    /// <summary>A Factor's Abacus reckons the least a haggler would take: the offer starts on it (haggling's).</summary>
    public bool KnowsFloors => HasCurio("abacus");

    /// <summary>
    /// Bram's cradle is on the counter: it rocks a chest gently, firmly and hard for you. Once the trade's ways reach
    /// Thornwick's (<see cref="Ways"/>): a first season shakes by hand for its week at the stall, which is how the
    /// strengths are learned, and any later one has the cradle from its first morning.
    /// </summary>
    public bool Cradle => Ways >= Market.CraftTier;

    /// <summary>
    /// The strengths the cradle rocks this chest at: every one its seller lets pass for nothing (not a hard shake where
    /// that's minded or can't be done). Where shaking has a price of its own (a goblin's fee for each strength, a rotten
    /// chest's few seconds, a seller who won't wait), it rocks once, gently, and the rest is yours to decide.
    /// </summary>
    public Shake[] CradleLevels(Visit visit)
    {
        var seller = visit.Seller;
        if (visit.ShakeFee > 0 || ShakeLimit(seller) > 0 || Impatient(seller)) return new[] { Shake.Gentle };
        bool hard = CanShakeHardAt(seller) && !MindsHardShake(seller);
        return hard ? new[] { Shake.Gentle, Shake.Firm, Shake.Hard } : new[] { Shake.Gentle, Shake.Firm };
    }

    /// <summary>Whether a chest can be read for you at all: not one chained shut, a Guild crate or a mimic's.</summary>
    static bool Readable(Chest chest) => !chest.Sealed && chest.Crate == null && !chest.Breathing;

    /// <summary>
    /// Writes on a slip what the build knows of a chest before it's touched (and keeps what's already there): a Lantern
    /// Moth's stars, a Toll-man's Gauge's false bottom, the Wardens' Primer's naming; with an engine, all of it. The
    /// counter does this as the chest arrives, and the sim's careful player before it reckons the rest.
    /// </summary>
    /// <returns>Whether anything was written.</returns>
    public bool Premark(Appraisal slip, Visit visit)
    {
        var chest = visit.Chest;
        if (!Readable(chest)) return false;
        bool wrote = false;
        if (ReadsChests)
        {
            var record = Appraisal.Recorded(chest, Tier, Ways);
            foreach (var kind in Categories.All.Select(c => c.Category))
            {
                if (record.Count(kind) == slip.Count(kind) && record.Magic(kind) == slip.Magic(kind)) continue;
                slip.Set(kind, record.Count(kind), record.Magic(kind));
                wrote = true;
            }
            wrote |= slip.Hidden != record.Hidden;
            slip.Hidden = record.Hidden;
            return wrote;
        }
        if (MarksMagic)
            foreach (var kind in chest.Items.Where(i => i.Magic && !i.Fake && !i.Broken && !i.IsSong).Select(i => i.Category).Distinct())
            {
                if (slip.Magic(kind)) continue;
                slip.Set(kind, Math.Max(1, slip.Count(kind)), true);
                wrote = true;
            }
        if (MarksHidden && slip.CountsHidden && slip.Hidden != (chest.Hidden != null))
        {
            slip.Hidden = chest.Hidden != null;
            wrote = true;
        }
        if (NamesWards && chest.Warded is Item warded)
        {
            // One more of its kind than the chest holds: that's what names it (Chest.Names).
            int named = chest.Items.Count(i => i.Category == warded.Category) + 1;
            if (slip.Count(warded.Category) < named)
            {
                slip.Set(warded.Category, named, slip.Magic(warded.Category));
                wrote = true;
            }
        }
        return wrote;
    }

    /// <summary>
    /// A Scrivener's Quill writes down a thing as it's first heard: one more of its kind on the slip (up to the most it
    /// counts), starred if it glowed. It writes what was heard, not what's so: sand is ticked as a spell like any rustle.
    /// </summary>
    /// <returns>Whether the slip changed (false without the quill).</returns>
    public bool Tick(Appraisal slip, Item item, bool glowed)
    {
        if (!TicksHeard) return false;
        var kind = item.Category;
        bool room = slip.Count(kind) < slip.MaxOf(kind), star = glowed && !slip.Magic(kind);
        if (!room && !star) return false;
        slip.Set(kind, Math.Min(slip.MaxOf(kind), slip.Count(kind) + 1), slip.Magic(kind) || glowed);
        return true;
    }

    /// <summary>The kind the Wardens' Primer has named on this chest (so a quill counting what's heard keeps one more of it), or null.</summary>
    public Category? PrimerNames(Visit visit) => NamesWards && Readable(visit.Chest) && visit.Chest.Warded is Item warded ? warded.Category : null;
}
