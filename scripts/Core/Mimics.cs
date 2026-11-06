using System;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// Mimics (0.33, docs/narrative.md, "The side stories"): where the Locking frays, an old ward goes hungry, eats what was
/// in its chest and lives on gold. A mimic breathes: while the chest sits still you can hear it, the only sound a still
/// chest ever makes, and it holds its breath while it's shaken. Nothing inside rattles, so a seller who listens thinks
/// the chest is empty and prices it so (one who prices by colour prices it by its colour, as ever). Opened, it's a
/// creature: the Arcanum's menagerie buys it, or you keep it in the back room and feed it on Guild nights, and one fed
/// long enough grows big enough to eat the gold off one of the gilded. Every rule holds every time: a breathing chest
/// always holds a mimic, and a mimic always breathes.
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

    /// <summary>What a kept mimic eats on each Guild night, in purses; and the Guild nights fed before it's grown.</summary>
    public const float FeedPurses = 0.15f;
    public const int GrowsAfter = 2;

    /// <summary>The flag for the Thornwick statue a grown mimic woke (one of the three).</summary>
    public const string ThornwickWoke = "mimic.thornwick"; // loc: keys
}

public partial class GameState
{
    /// <summary>A mimic kept in the back room: the Guild nights it's been fed (null: none).</summary>
    public int? Mimic { get; private set; }

    public bool MimicGrown => Mimic >= Mimics.GrowsAfter;

    /// <summary>What the menagerie pays for a mimic just opened, and for the one you keep (more once it's grown).</summary>
    public int FreshMimicPrice => Purses(Mimics.PursesFor);
    public int KeptMimicPrice => Purses(MimicGrown ? Mimics.PursesGrown : Mimics.PursesFor);

    /// <summary>What it eats on a Guild night.</summary>
    public int MimicFeed => Purses(Mimics.FeedPurses);

    /// <summary>Mimics opened this season, and the gilded a grown one has woken (the sim's tally, the season's end).</summary>
    public int MimicsFound { get; private set; }
    public int MimicsWoke { get; private set; }

    int _breaths;

    /// <summary>
    /// Maybe the chest a seller's brought is breathing (its own dice, after they've priced it, so every other roll of the
    /// day stays as it was): an ordinary chest from Thornwick on, nothing of a tale's, a gift's, a thief's, or with a
    /// Heartsong or a ward in it. Then it's empty but for the mimic, and they price it for what they notice in it.
    /// </summary>
    Visit Breathe(Visit visit)
    {
        var chest = visit.Chest;
        if (Tier < Mimics.FromTier || visit.Gift || visit.Lesson != null || chest.Stolen || chest.Song != null || chest.Warded != null || chest.Sealed ||
            chest.Tale != null || chest.Breathing)
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
    public void OpenedMimic() => MimicsFound++;

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
        Log(Story.Tale, Loc.T("You kept a mimic in the back room. It eats gold on Guild nights, and no thief will try a door it's behind."));
        return true;
    }

    /// <summary>The kept mimic goes to the menagerie (at the Lantern Market). Returns what it paid.</summary>
    public int SellKeptMimic()
    {
        if (Mimic == null) return 0;
        int price = KeptMimicPrice;
        Mimic = null;
        Earn(price);
        return price;
    }

    /// <summary>
    /// A Guild night, for a kept mimic (before the Guild collects): it eats, or slips away hungry; grown, it eats the gold
    /// off one of the gilded nearest the counter, and they wake.
    /// </summary>
    void FeedMimic()
    {
        if (Mimic is not int fed) return;
        if (Gold < MimicFeed)
        {
            Mimic = null;
            Log(Story.Tale, Loc.T("Your mimic went hungry on the Guild's night, and slipped away through the back door."));
            return;
        }
        Gold -= MimicFeed;
        Mimic = fed + 1;
        if (MimicGrown && Ungild() is string who)
            Log(Story.Tale, Loc.T($"Your mimic is grown. Tonight it ate the gold off {who}, who woke up cold, and very confused."));
    }

    /// <summary>A grown mimic's meal: whoever the Guild collected, or one of Thornwick's three. Who woke, or null.</summary>
    string Ungild()
    {
        if (VaultOpened) return null;
        if (Gilded.Keys.OrderBy(k => k, StringComparer.Ordinal).FirstOrDefault() is string person)
        {
            Gilded.Remove(person);
            Gone.Remove(person);
            Flags.Add($"ungilded.{person}"); // loc: keys
            MimicsWoke++;
            Sway(Faction.Commons, 1);
            return Sellers.Person(person).Name;
        }
        if (Flagged(Mimics.ThornwickWoke)) return null;
        Flags.Add(Mimics.ThornwickWoke);
        MimicsWoke++;
        Sway(Faction.Commons, 1);
        return Loc.T("a cooper in Thornwick's square");
    }

    void SaveMimic(SaveData data)
    {
        data.Mimic = Mimic ?? -1;
        data.MimicsFound = MimicsFound;
        data.MimicsWoke = MimicsWoke;
    }

    void LoadMimic(SaveData data)
    {
        Mimic = data.Mimic >= 0 ? data.Mimic : null;
        MimicsFound = data.MimicsFound;
        MimicsWoke = data.MimicsWoke;
    }
}
