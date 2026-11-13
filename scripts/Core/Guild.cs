using System;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The Gilded Guild: it licenses Keywrights and takes a weekly tithe "to keep the Gate" (and sends it through the Gate,
/// to the Maw). Bram opened chests unlicensed for twenty years, and the back-tithe comes with the stall. Its Collector,
/// Mister Vane, comes for the week's due at the end of every fifth day, and each week's due is bigger than the last. Pay
/// one and the Guild's carters move the shop a market further in (0.36: <see cref="GameState.MoveInward"/>); pay all four
/// and Bram's debt is cleared; the Guild keeps coming after that, for whatever more it can squeeze (endless). Fall
/// short and you can pawn what's on display and your curios; if that's still not enough, the Guild takes the shop.
/// </summary>
public static class Guild
{
    /// <summary>
    /// A Guild week: five days, each with its own business (<see cref="WeekPlan"/>): the Guild's night on the last, two
    /// days with something on in town, and Nim twice (or Nim once and the story). 0.20 tried three-day weeks to shorten
    /// the run; v21 keeps the week a working week and shortens the run by weeks instead.
    /// </summary>
    public const int DaysPerWeek = 5;

    /// <summary>
    /// The weeks of Bram's debt: four since 0.36, one at each market (eight until 0.20's shorter runs, five until 0.30's
    /// tales, seven until 0.35, whose first run took 2.4 hours and was lost every time). After the last, the dues keep
    /// coming (endless).
    /// </summary>
    public const int Weeks = 4;

    /// <summary>
    /// Each week's due is the toll for the road inward: paid, the stall trades a market further in the next morning, at
    /// chests several times dearer. So each asks for a good week where you are and leaves a working purse for where you're
    /// going: a few good days at the stall, a week of Thornwick's locks and false bottoms, a week of the capital's
    /// enchantments, and the last, at the Royal Emporium, for more than ordinary trade makes there (the power index has a
    /// ceiling, and chests top out at purple): carrying it takes a build come together or tales gone well. (0.30 to 0.35:
    /// seven dues, 20 to 12,000, over 35 days, with the moves bought for 80, 300 and 3,000.)
    /// </summary>
    static int[] Dues = { 40, 170, 700, 5000 };

    /// <summary>For tuning (`--sim=quick --dues=a,b,c,d`): other dues, one a week of the debt.</summary>
    public static void TryDues(int[] dues)
    {
        if (dues.Length == Weeks) Dues = dues;
    }

    /// <summary>After the debt: each week half as much again, for as long as you last (twice as much until 0.30).</summary>
    const float EndlessGrowth = 1.5f;

    /// <summary>What the Guild wants at the end of this week.</summary>
    public static int Due(int week)
    {
        if (week <= Weeks) return Dues[Math.Max(0, week - 1)];
        double due = Dues[^1] * Math.Pow(EndlessGrowth, week - Weeks);
        return (int)(Math.Round(due / 100) * 100);
    }

    /// <summary>All of Bram's debt, as Vane tells it the first evening.</summary>
    public static int Debt => Dues.Sum();

    /// <summary>Pawned things fetch this much of what they'd sell for: the Guild drives a hard bargain.</summary>
    public const float PawnRate = 0.6f;

    public static string Collector => Loc.T("Mister Vane");

    /// <summary>What Vane says when he comes for a week's due, week by week (the last line repeats).</summary>
    static readonly string[] Arrivals =
    {
        Loc.T("Good evening. The Guild's tithe, if you please. In gold: the Guild has no use for promises."),
        Loc.T("Still here, I see. Good. The Guild hates to see a Keywright go to waste."),
        Loc.T("My masters have begun to ask about you. That is rarely good news. One more week after this one."),
        Loc.T("The last of Bram's debt. Every coin of it, mind. Gold you can see."),
    };

    static readonly string[] Endless =
    {
        Loc.T("Debts are curious things, Keywright. They grow while nobody's watching them."),
        Loc.T("The Gate is hungry this week, and the Guild has decided you owe it something for its patience."),
        Loc.T("Interest, you understand. Nothing personal. My master is very fond of interest."),
    };

    public static string Arrival(int week) => week <= Weeks ? Arrivals[Math.Min(week, Arrivals.Length) - 1] : Endless[(week - Weeks - 1) % Endless.Length];

    public static string Paid(int week) => week switch
    {
        Weeks => Loc.T("Paid in full. The Guild has no further claim on Bram's stall. For now."),
        > Weeks => Loc.T("Paid. Until next week, Keywright."),
        _ => Loc.T("Paid, and promptly. Until next week, then."),
    };

    public static string Short => Loc.T("Short? Then the Guild will take what's on your shelf instead. At the Guild's price, naturally.");

    public static string Foreclosed => Hints.Kin == Characters.Kit
        ? Loc.T("Not enough. The Guild seals this shop tonight, and your licence with it. Your uncle will be so disappointed.")
        : Loc.T("Not enough. The Guild seals this shop tonight, and your licence with it. Bram will be so disappointed.");

    /// <summary>When the Guild Promissory Note pays a night.</summary>
    public static string NotePaid => Loc.T("A Guild note, older than the Locking? ...However did you come by that? The Guild honours its own paper. Paid.");

    /// <summary>When the whole debt is paid off at once.</summary>
    public static string PaidOff => Loc.T("All of it, tonight? ...How very thorough of you. The Guild has no further claim on Bram's stall.");

    /// <summary>When Bram's word buys a week.</summary>
    public static string Grace => Loc.T("Bram's word? How touching. One week, then. The rest comes due next week, and half as much again.");
}
