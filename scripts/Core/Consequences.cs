using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// What a tale leaves behind (docs/stories.md, "What a tale leaves behind", 0.30): better terms from people you've helped
/// (<see cref="Goodwill"/>), favours owed and called in on a short Guild night (<see cref="Owed"/>), papers for the case
/// against the Guild (<see cref="Evidence"/>), Mother Tansy's loan, effects that last a while (a rival across the road,
/// pickets at the door, a porter, market days: <see cref="Until"/>), nights away from the shop, a shard of the Key by
/// another road, and the Maw's bargain. Every one is stated when it happens and holds every time; all of it is saved.
/// </summary>
public sealed partial class GameState
{
    // ---- Goodwill: better terms from friends ----

    /// <summary>A neighbour's price at Brindle while Pip's safe with you (Wynn's word): a sixth off.</summary>
    public const float NeighbourPrice = 5f / 6;

    /// <summary>A dwarf's price from Durgan once Ironroot's crown is home: a fifth off.</summary>
    public const float DwarfPrice = 0.8f;

    /// <summary>Reeve Caskett's toll on Thornwick's regulars: a tenth on.</summary>
    public const float TollPrice = 1.1f;

    /// <summary>
    /// What someone asks you, for what you did for them or their people (or what a tale's laid on them): a factor on
    /// their price and the line under their chest saying why; and whether their own rule is waived for you.
    /// </summary>
    public (float Factor, string Note, bool Waived) Goodwill(SellerDef seller)
    {
        if (seller == null) return (1, null, false);
        // Wynn's neighbours: every Brindle regular, while her boy is your apprentice.
        if (seller.Tier == 1 && Flagged(TaleBook.Neighbours) && !Gone.Contains(TaleBook.Pip))
            return (NeighbourPrice, Loc.T("A neighbour's price, for Wynn's sake: a sixth off."), false);
        if (seller.Key == TaleBook.Durgan && Flagged(TaleBook.CrownHome))
            return (DwarfPrice, Loc.T("A dwarf's price, for the crown: a fifth off."), false);
        if (seller.Key == TaleBook.Nettle && Flagged(TaleBook.GoblinFriend))
            return (1, Loc.T("Nettle charges a friend of the clan nothing for shaking."), true);
        if (seller.Key == TaleBook.Rowan && Flagged(TaleBook.AldwinHome))
            return (1, Loc.T("Sir Rowan leaves the best blade to whoever brought Aldwin's home."), true);
        if (seller.Tier == 2 && Flagged(TaleBook.Tolls))
            return (TollPrice, Loc.T("Reeve Caskett's toll: a tenth on."), false);
        return (1, null, false);
    }

    /// <summary>A regular's chest, as a friend (or a toll) would have it: the price and the terms.</summary>
    void ApplyGoodwill(Visit visit)
    {
        var (factor, note, waived) = Goodwill(visit.Seller);
        if (note == null) return;
        visit.Waived = waived;
        if (factor < 1) visit.Reduce(factor, note);
        else if (factor > 1) visit.Increase(factor, note);
    }

    // ---- Favours ----

    /// <summary>
    /// Who owes you a favour, and from which tale (the Friends &amp; foes page lists them). On a Guild night you're short,
    /// call one in: they bring an eighth of the night's due, once a season each.
    /// </summary>
    public Dictionary<string, string> Owed { get; } = new();

    /// <summary>Favours called in this season, in order (the season's end remembers who came).</summary>
    public List<string> CalledIn { get; } = new();

    public const float FavourShare = 0.125f;

    /// <summary>Someone owes you one now (a second time changes nothing).</summary>
    public void Owe(string person, string tale)
    {
        if (!CalledIn.Contains(person)) Owed.TryAdd(person, tale);
    }

    /// <summary>What a favour brings tonight: an eighth of the due.</summary>
    public int FavourPays => Math.Max(1, Money.Round(DueNow * FavourShare));

    /// <summary>Who you could call on tonight: the Guild's here, you're short, and they're still around.</summary>
    public IEnumerable<string> Callable => DuesTonight && !PaidThisWeek && Gold < DueNow
        ? Owed.Keys.Where(p => !Gone.Contains(p)).ToList() : Enumerable.Empty<string>();

    /// <summary>Calls a favour in: they bring what they can, and you're square.</summary>
    public int CallIn(string person)
    {
        if (!Callable.Contains(person)) return 0;
        int pays = FavourPays;
        Owed.Remove(person);
        CalledIn.Add(person);
        TaleGold(pays);
        return pays;
    }

    // ---- The case against the Guild ----

    /// <summary>Papers for the case against the Guild ("roll", "ledger", "tallies"...), from the tales: <see cref="TaleBook.Paper"/> names each.</summary>
    public HashSet<string> Evidence { get; } = new();

    /// <summary>A paper for the case: kept until the King asks for it (or the Guild buys it).</summary>
    public void Gather(string paper) => Evidence.Add(paper);

    /// <summary>The court's verdict: the Guild on trial, its tithe halved for the rest of the debt (and its interest after).</summary>
    public bool OnTrial => Flagged(TaleBook.Verdict);

    // ---- Mother Tansy's loan ----

    /// <summary>What Mother Tansy's owed (0: nothing), and the morning she comes for it.</summary>
    public int LoanOwed { get; private set; }

    public int LoanDue { get; private set; }

    /// <summary>Borrows from Mother Tansy: the purse now, the purse and her share on the given morning.</summary>
    public void Borrow(int purse, float share, int dueDay)
    {
        TaleGold(purse);
        LoanOwed += Money.Round(purse * (1 + share));
        LoanDue = dueDay;
    }

    /// <summary>
    /// Mother Tansy's morning: she takes what she's owed out of the purse; if it's not there, the best thing on the shelf
    /// (or a curio) instead, and her men stand at your door a week. Returns what came of it, for the morning's notice.
    /// </summary>
    public string CollectLoan()
    {
        if (LoanOwed <= 0 || Day < LoanDue) return null;
        int owed = LoanOwed;
        LoanOwed = 0;
        if (Gold >= owed)
        {
            Gold -= owed;
            Flags.Add(TaleBook.LoanPaid);
            Log("tansy", Loc.T($"Mother Tansy took her {Money.Say(owed)}, and called you a good risk."));
            return Loc.T($"Mother Tansy's man came for her {Money.Say(owed)} at first light, and went away happy.");
        }
        Gold = 0;
        // What's missing comes off the shelf: the best thing on display, or else a curio.
        var best = Display.Where(i => !i.IsSong).OrderByDescending(i => SaleValue(i)).FirstOrDefault();
        string took;
        if (best != null)
        {
            Display.Remove(best);
            took = best.Name;
        }
        else if (_curios.Count > 0)
        {
            var curio = Curios.Get(_curios[0]);
            TakeCurio(curio.Key);
            took = curio.Name;
        }
        else took = null;
        Flags.Add(TaleBook.LoanDefaulted);
        Until[TaleBook.TansyMen] = Day + Guild.DaysPerWeek - 1;
        Log("tansy", took == null ? Loc.T("You couldn't pay Mother Tansy, and had nothing she wanted. Her men stand at your door.")
            : Loc.T($"You couldn't pay Mother Tansy. She took your {took}, and her men stand at your door."));
        return took == null ? Loc.T("Mother Tansy's men came for her money and found none. They'll stand at your door all week.")
            : Loc.T($"Mother Tansy's men came for her money and took your {took} instead. They'll stand at your door all week.");
    }

    // ---- Effects that last a while ----

    /// <summary>
    /// Effects a tale sets for a while, to the last day they hold ("hedge.trading", "porter"...): a rival across the
    /// road, pickets at the door, a crowd, a night away. <see cref="While"/> reads them.
    /// </summary>
    public Dictionary<string, int> Until { get; } = new();

    /// <summary>Whether an effect a tale set is still in force today.</summary>
    public bool While(string key) => Until.TryGetValue(key, out int day) && Day <= day;

    /// <summary>
    /// Customers the tales add or take away today: a rival hedge-wright takes one, pickets, a moneylender's men and the
    /// Field's walking dead turn one away, the market days a reeve gave you bring one while you trade at Thornwick, a beaten rival's regulars one more
    /// for a week, and the harvest fair two on its day.
    /// </summary>
    public int TaleCustomers => (While(TaleBook.Rival) ? -1 : 0) + (While(TaleBook.Picket) ? -1 : 0) + (While(TaleBook.TansyMen) ? -1 : 0) +
                                (While(TaleBook.Haunt) ? -1 : 0) + (Flagged(TaleBook.MarketDays) && Tier == 2 ? 1 : 0) + (While(TaleBook.Crowd) ? 1 : 0) +
                                (Until.TryGetValue(TaleBook.FairDay, out int fair) && fair == Day ? FairCrowd : 0);

    /// <summary>The harvest fair's day: the whole county comes past the stall.</summary>
    public const int FairCrowd = 2;

    /// <summary>Tonight you're away from the shop (at the toll, at the Archive's fire): Nim finds the back door shut.</summary>
    public bool AwayTonight => Until.TryGetValue(TaleBook.Away, out int day) && day == Day;

    /// <summary>A tale takes you out tonight: no Nim.</summary>
    public void GoOutTonight() => Until[TaleBook.Away] = Day;


    // ---- Saving ----

    void SaveConsequences(SaveData data)
    {
        data.Owed = new Dictionary<string, string>(Owed);
        data.CalledIn = CalledIn.ToList();
        data.Evidence = Evidence.ToList();
        data.LoanOwed = LoanOwed;
        data.LoanDue = LoanDue;
        data.Until = new Dictionary<string, int>(Until);
    }

    void LoadConsequences(SaveData data)
    {
        foreach (var (person, tale) in data.Owed ?? new Dictionary<string, string>()) Owed[person] = tale;
        CalledIn.AddRange(data.CalledIn ?? new List<string>());
        Evidence.UnionWith(data.Evidence ?? new List<string>());
        LoanOwed = data.LoanOwed;
        LoanDue = data.LoanDue;
        foreach (var (key, day) in data.Until ?? new Dictionary<string, int>()) Until[key] = day;
    }
}
