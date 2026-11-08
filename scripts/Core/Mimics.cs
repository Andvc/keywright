using System;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// Mimics (0.33; 0.34, docs/narrative.md): where the Locking frays, an old ward goes hungry, eats what was in its chest and
/// lives on gold. A mimic breathes: while the chest sits still you can hear it, the only sound a still chest ever makes,
/// and it holds its breath while it's shaken. Nothing inside rattles, so a seller who listens thinks the chest is empty
/// and prices it so (one who prices by colour prices it by its colour, as ever). They breathe only once you remember the
/// treasury (the first one you meet is the first you've reason to listen for). Opened, it's a creature with a finite
/// appetite: keep it in the back room and feed it on Guild nights, leave it with the Arcanum's menagerie (they feed it),
/// or sell it to them. Fed twice it's full and falls asleep, and a full mimic will eat the gold off one of the gilded,
/// then wake hungry again. Every rule holds every time: a breathing chest always holds a mimic, and a mimic always breathes.
/// </summary>
public static class Mimics
{
    /// <summary>
    /// From Thornwick on (the old wards fray away from the crossroads), one chest a seller brings in this many breathes:
    /// two or three a season. (One in 45, at a working purse each, was six a season and a ninth more clears.)
    /// </summary>
    public static int OneIn { get; set; } = 90;
    public const int FromTier = 2;

    /// <summary>What the menagerie pays for one, in working purses (a purse is two typical chests here); more for one you've raised.</summary>
    public const float PursesFor = 0.6f, PursesGrown = 1.6f;

    /// <summary>What a kept mimic eats on each Guild night, in purses; and the Guild nights fed before it's full.</summary>
    public const float FeedPurses = 0.15f;
    public const int GrowsAfter = 2;

    /// <summary>What the menagerie asks to keep and feed one for you, in purses (once).</summary>
    public const float KeepingPurses = 0.4f;
}

public partial class GameState
{
    /// <summary>A mimic in your care (the back room, or the menagerie's): the Guild nights it's been fed since it last ate a shell (null: none).</summary>
    public int? Mimic { get; private set; }

    /// <summary>Your mimic is fed to its fill, asleep on the gold: it will eat the gold off one of the gilded.</summary>
    public bool MimicFull => Mimic >= Mimics.GrowsAfter;

    /// <summary>Your mimic is kept and fed at the Arcanum's menagerie, not in the back room.</summary>
    public bool MimicAtMenagerie { get; private set; }

    /// <summary>What the menagerie asks to keep yours.</summary>
    public int KeepingPrice => Purses(Mimics.KeepingPurses);

    /// <summary>What the menagerie pays for a mimic just opened, and for the one you keep (more once it's grown).</summary>
    public int FreshMimicPrice => Purses(Mimics.PursesFor);
    public int KeptMimicPrice => Purses(MimicFull ? Mimics.PursesGrown : Mimics.PursesFor);

    /// <summary>What it eats on a Guild night.</summary>
    public int MimicFeed => Purses(Mimics.FeedPurses);

    /// <summary>Mimics opened this season (the sim's tally).</summary>
    public int MimicsFound { get; private set; }

    int _breaths;

    /// <summary>
    /// Maybe the chest a seller's brought is breathing (its own dice, after they've priced it, so every other roll of the
    /// day stays as it was): an ordinary chest from Thornwick on, nothing of a tale's, a gift's, a thief's, or with a
    /// Heartsong or a ward in it. Then it's empty but for the mimic, and they price it for what they notice in it.
    /// </summary>
    Visit Breathe(Visit visit)
    {
        var chest = visit.Chest;
        if (!Remembers || CampaignDone || Tier < Mimics.FromTier || visit.Gift || visit.Lesson != null || chest.Stolen || chest.Song != null || chest.Warded != null ||
            chest.Sealed || chest.Tale != null || chest.Breathing || chest.Crate != null)
            return visit;
        var rng = new Random(unchecked(Seed * 7331 + Day * 131 + ++_breaths));
        if (rng.Next(Mimics.OneIn) != 0) return visit;
        var mimic = new Chest(Array.Empty<Item>(), chest.Quality, lockTier: chest.Lock) { Breathing = true };
        var priced = Market.Price(rng, visit.Seller, mimic, Tier, boosts: Boosts, origin: visit.Origin);
        priced.HearsOut = visit.HearsOut;
        priced.Unruffled = visit.Unruffled;
        ApplyGoodwill(priced);
        return priced;
    }

    /// <summary>A breathing chest's been opened: the mimic's out.</summary>
    public void OpenedMimic()
    {
        MimicsFound++;
        Note("mimic");
    }

    /// <summary>Whether a mimic just opened can be kept (there's room for one in the back room).</summary>
    public bool CanKeepMimic => Mimic == null;

    /// <summary>The mimic just opened goes to the Arcanum's menagerie. Returns what it paid.</summary>
    public int SellFreshMimic()
    {
        int price = FreshMimicPrice;
        Earn(price);
        return price;
    }

    /// <summary>The mimic just opened goes in the back room, to be fed on Guild nights.</summary>
    public bool KeepMimic()
    {
        if (!CanKeepMimic) return false;
        Mimic = 0;
        MimicAtMenagerie = false;
        Log(Story.Tale, Loc.T("You kept a mimic in the back room. It eats gold on Guild nights, and no thief will try a door it's behind."));
        return true;
    }

    /// <summary>
    /// Your mimic goes to the Arcanum's menagerie to be kept and fed for you (for those who'd rather not do the feeding):
    /// still yours, it's full after the same two Guild nights, and they'll bring it when it's wanted. False if you can't pay.
    /// </summary>
    public bool EntrustMimic()
    {
        if (Mimic == null || MimicAtMenagerie || Gold < KeepingPrice) return false;
        Gold -= KeepingPrice;
        MimicAtMenagerie = true;
        Log(Story.Tale, Loc.T("You left your mimic with the Arcanum's menagerie. They'll feed it, and bring it when it's wanted."));
        return true;
    }

    /// <summary>The kept mimic goes to the menagerie for good (at the Lantern Market). Returns what it paid.</summary>
    public int SellKeptMimic()
    {
        if (Mimic == null) return 0;
        int price = KeptMimicPrice;
        Mimic = null;
        MimicAtMenagerie = false;
        Earn(price);
        return price;
    }

    /// <summary>
    /// A Guild night, for a mimic in your care (before the Guild collects): it eats (the menagerie feeds theirs), or slips
    /// away hungry; fed twice, it's full, and sleeps.
    /// </summary>
    void FeedMimic()
    {
        if (Mimic is not int fed) return;
        if (!MimicAtMenagerie)
        {
            if (Gold < MimicFeed)
            {
                Mimic = null;
                Log(Story.Tale, Loc.T("Your mimic went hungry on the Guild's night, and slipped away through the back door."));
                return;
            }
            Gold -= MimicFeed;
        }
        bool wasFull = MimicFull;
        Mimic = fed + 1;
        if (MimicFull && !wasFull)
        {
            Note("full");
            Log(Story.Tale, Loc.T("Your mimic ate its fill tonight, curled up on the gold, and fell asleep. It wanted something. It didn't want everything."));
        }
    }

    void SaveMimic(SaveData data)
    {
        data.Mimic = Mimic ?? -1;
        data.MimicsFound = MimicsFound;
        data.MimicAtMenagerie = MimicAtMenagerie;
    }

    void LoadMimic(SaveData data)
    {
        Mimic = data.Mimic >= 0 ? data.Mimic : null;
        MimicsFound = data.MimicsFound;
        MimicAtMenagerie = data.MimicAtMenagerie;
    }
}
