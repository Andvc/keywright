using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The Lantern Market (0.33, docs/narrative.md, "The side stories"): Nim's people, under the Barrow at midnight, from
/// Thornwick on. On the third night of a week Nim can take you down instead of just laying out his table. It buys what
/// can't be sold by daylight and sells what can't be bought: a whisperer's secret for the case; the Maw's buyers, who pay
/// over the odds for anything gold (and every coin of it is an eye for him); the Arcanum's menagerie, for a mimic.
/// </summary>
public static class LanternMarket
{
    public const int FromTier = 2;

    /// <summary>The night of the week the lanterns are lit.</summary>
    public const int Night = 3;

    /// <summary>A secret's price, in working purses.</summary>
    public const float SecretPurses = 0.35f;

    /// <summary>What the Maw's buyers pay for a gold thing, against what it sells for here.</summary>
    public const float GatePremium = 1.5f;
}

public partial class GameState
{
    /// <summary>Nim can take you down to the Lantern Market tonight.</summary>
    public bool MarketTonight => Tier >= LanternMarket.FromTier && PeddlerToday && DayOfWeek == LanternMarket.Night && !Foreclosed;

    /// <summary>Gold things sold down the Gate this season: each an eye for the Maw (the season's end reads them back).</summary>
    public int EyesSold { get; private set; }

    /// <summary>The night a secret was last bought (one a night).</summary>
    int _secretNight;

    /// <summary>
    /// What the whisperer has tonight: a clue not yet found to a question you're asking, one that gives a word you still
    /// need first. Null when there's nothing to sell (or you've bought tonight's).
    /// </summary>
    public CaseClue Whisper
    {
        get
        {
            if (_secretNight == Day) return null;
            var open = OpenQuestions.Where(q => !Knows(q)).ToList();
            CaseClue Best(bool needed) => Case.Clues.FirstOrDefault(c => !Clues.Contains(c.Key) && c.Key != LanternBuyers &&
                open.Any(q => q.Key == c.Question && (!needed || Enumerable.Range(0, q.Blanks.Length)
                    .Any(b => c.Gives.Contains(q.Answer[b]) && Words(q, b).All(w => w.Key != q.Answer[b])))));
            return Best(true) ?? Best(false);
        }
    }

    public int SecretPrice => Purses(LanternMarket.SecretPurses);

    /// <summary>Buys tonight's secret: the clue is yours. False if there's none or you can't pay.</summary>
    public bool BuySecret()
    {
        if (Whisper is not CaseClue clue || Gold < SecretPrice) return false;
        Gold -= SecretPrice;
        _secretNight = Day;
        Find(clue.Key);
        Log(Story.Tale, Loc.T("You bought a secret at the Lantern Market."));
        return true;
    }

    /// <summary>The buyers' own words, the first time you walk past them: a clue to where the gilded go.</summary>
    public const string LanternBuyers = "market.buyers"; // loc: keys

    /// <summary>Down to the market: the Maw's buyers are the first thing you see.</summary>
    public void VisitMarket() => Find(LanternBuyers);

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

    void SaveMarket(SaveData data)
    {
        data.EyesSold = EyesSold;
        data.SecretNight = _secretNight;
    }

    void LoadMarket(SaveData data)
    {
        EyesSold = data.EyesSold;
        _secretNight = data.SecretNight;
    }
}
