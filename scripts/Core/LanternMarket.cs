using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The Lantern Market (0.33; 0.34, docs/narrative.md): Nim's people, under the Barrow at midnight, from Thornwick on. On
/// the third night of a week Nim can take you down instead of just laying out his table. It sells what can't be bought
/// by daylight, and never an answer: once you remember, a night at the Guild's weighhouse (and a look at its register),
/// the bargemen of the Wet Hen for the treasury's last night, and a crate diverted off the palace road. The Maw's buyers
/// pay over the odds for anything gold, and every coin of it is an eye for him; the Arcanum's menagerie buys a mimic, or
/// keeps and feeds yours.
/// </summary>
public static class LanternMarket
{
    public const int FromTier = 2;

    /// <summary>The night of the week the lanterns are lit.</summary>
    public const int Night = 3;

    /// <summary>What the Maw's buyers pay for a gold thing, against what it sells for here.</summary>
    public const float GatePremium = 1.5f;
}

public partial class GameState
{
    /// <summary>
    /// Nim can take you down to the Lantern Market tonight. Before you remember, only when there's something to do down
    /// there (a gold thing the Maw's buyers want, or a mimic for the menagerie): a stall of nothing is noise (0.34).
    /// </summary>
    public bool MarketTonight => Tier >= LanternMarket.FromTier && PeddlerToday && DayOfWeek == LanternMarket.Night && !Foreclosed &&
                                 (Remembers || GoldForTheGate.Any() || Mimic != null);

    /// <summary>Gold things sold down the Gate this season: each an eye for the Maw (the season's end reads them back).</summary>
    public int EyesSold { get; private set; }

    // ---- Once you remember: access, papers, transport ----

    /// <summary>A night at the weighhouse, with a look at its register: offered once you know the Guild carts people (or have heard of its carts).</summary>
    public bool PassOffered => Remembers && !CampaignDone && (Knows("carts") || KnowsInventory) && !Noted.Contains("register");

    /// <summary>Half price to a face Nim's people know: you've used a pass in an earlier season.</summary>
    public int PassPrice => Purses(Campaign.PassPurses * (KnownBefore.Contains("weighhouse") ? 0.5f : 1));

    public bool BuyPass()
    {
        if (!PassOffered || Gold < PassPrice) return false;
        Gold -= PassPrice;
        Note("weighhouse");
        Note("register");
        Log(Story.Tale, Loc.T("You bought a night at the Guild's weighhouse from Nim's people, and copied a page of its register."));
        return true;
    }

    /// <summary>The Wet Hen's bargemen, for the treasury's last night: a way out for whoever comes out.</summary>
    public bool BargeOffered => Remembers && !CampaignDone && Knows("seal") && !Flagged("barge.hired") && !TreasuryDone;

    public int BargePrice => Purses(Campaign.BargePurses);

    public bool HireBarge()
    {
        if (!BargeOffered || Gold < BargePrice) return false;
        Gold -= BargePrice;
        Flags.Add("barge.hired");
        Log(Story.Tale, Loc.T("You hired the Wet Hen's bargemen for the treasury's last night: a way out, for whoever comes out."));
        return true;
    }

    /// <summary>A crate diverted off the palace road (Bram's, chalked with his name): it comes to the counter tomorrow.</summary>
    public bool DivertOffered => Remembers && !CampaignDone && KnowsInventory && BramNotes && !Flagged("crate.diverted") &&
                                 !Guests.ContainsKey(Campaign.Bram) && !Rescued.Contains(Campaign.Bram) && !TreasuryDone;

    public int DivertPrice => Purses(Campaign.DivertPurses);

    public bool Divert()
    {
        if (!DivertOffered || Gold < DivertPrice) return false;
        Gold -= DivertPrice;
        Flags.Add("crate.diverted");
        Log(Story.Tale, Loc.T("You paid the bargemen to take a crate chalked with Bram's name off the palace road. Corvin will bring it."));
        return true;
    }

    // ---- The Maw's buyers ----

    /// <summary>Things on display the Maw's buyers want: anything gold in colour (never a Heartsong).</summary>
    public IEnumerable<Item> GoldForTheGate => Display.Where(i => i.Quality == Rarity.Legendary && !i.IsSong && !i.Fake && !i.Broken);

    public int GatePrice(Item item) => Money.Round(SaleValue(item) * LanternMarket.GatePremium);

    /// <summary>Sells a gold thing off the shelf to the Maw's buyers. Returns what they paid.</summary>
    public int SellToGate(Item item)
    {
        if (!GoldForTheGate.Contains(item) || !Display.Remove(item)) return 0;
        int price = GatePrice(item);
        Earn(price);
        EyesSold++;
        Log(Story.Tale, Loc.T($"You sold {item.Name} to the Maw's buyers at the Lantern Market, for {price}g. An eye for him."));
        return price;
    }

    void SaveMarket(SaveData data) => data.EyesSold = EyesSold;

    void LoadMarket(SaveData data) => EyesSold = data.EyesSold;
}
