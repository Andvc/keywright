using System;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The Gilded Guild: it licenses Keywrights and takes a weekly tithe "to keep the Gate" (and sends it through the Gate,
/// to the Maw). Bram opened chests unlicensed for twenty years, and the back-tithe comes with the stall. Its Collector,
/// Mister Vane, comes for the week's due at the end of every fifth day, and each week's due is bigger than the last. Pay all five
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

    /// <summary>The weeks of Bram's debt (eight until 0.20's shorter runs). After the last, the dues keep coming (endless).</summary>
    public const int Weeks = 5;

    /// <summary>
    /// Each week's due: small for the first three weeks, while you learn the stall and a growing purse is all you need,
    /// then a steep climb: the fourth asks for a few good days at the capital, and the last for more than ordinary trade
    /// makes even at the Royal Emporium with every upgrade (the power index has a ceiling, and chests top out at purple).
    /// Carrying it takes a build come together (or a Heartsong's luck). A first run usually needs Bram's word in the
    /// fourth week and falls in the fifth; a strong run can have all of it early, and pay it off.
    /// </summary>
    static readonly int[] Dues = { 20, 60, 300, 4000, 45000 };

    /// <summary>After the debt: each week twice as much, for as long as you last.</summary>
    const float EndlessGrowth = 2f;

    /// <summary>What the Guild wants at the end of this week.</summary>
    public static int Due(int week)
    {
        if (week <= Weeks) return Dues[Math.Max(0, week - 1)];
        double due = Dues[^1] * Math.Pow(EndlessGrowth, week - Weeks);
        return (int)(Math.Round(due / 1000) * 1000);
    }

    /// <summary>All of Bram's debt, as Vane tells it the first evening.</summary>
    public static int Debt => Dues.Sum();

    /// <summary>Pawned things fetch this much of what they'd sell for: the Guild drives a hard bargain.</summary>
    public const float PawnRate = 0.6f;

    public const string Collector = "Mister Vane";

    /// <summary>What Vane says when he comes for a week's due, week by week (the last line repeats).</summary>
    static readonly string[] Arrivals =
    {
        "Good evening. The Guild's tithe, if you please. In gold: the Guild has no use for promises.",
        "Still here, I see. Good. The Guild hates to see a Keywright go to waste.",
        "I hear you've opened a great many chests this week. The Guild approves. Do keep opening them.",
        "My masters have begun to ask about you. That is rarely good news. One more week after this one.",
        "The last of Bram's debt. Every coin of it, mind. Gold you can see.",
    };

    static readonly string[] Endless =
    {
        "Debts are curious things, Keywright. They grow while nobody's watching them.",
        "The Gate is hungry this week, and the Guild has decided you owe it something for its patience.",
        "Interest, you understand. Nothing personal. My master is very fond of interest.",
    };

    public static string Arrival(int week) => week <= Weeks ? Arrivals[Math.Min(week, Arrivals.Length) - 1] : Endless[(week - Weeks - 1) % Endless.Length];

    public static string Paid(int week) => week switch
    {
        Weeks => "Paid in full. The Guild has no further claim on Bram's stall. For now.",
        > Weeks => "Paid. Until next week, Keywright.",
        _ => "Paid, and promptly. Until next week, then.",
    };

    public const string Short = "Short? Then the Guild will take what's on your shelf instead. At the Guild's price, naturally.";

    public const string Foreclosed = "Not enough. The Guild seals this shop tonight, and your licence with it. Your uncle will be so disappointed.";

    /// <summary>When the Guild Promissory Note pays a night.</summary>
    public const string NotePaid = "A Guild note, older than the Locking? ...However did you come by that? The Guild honours its own paper. Paid.";

    /// <summary>When the whole debt is paid off at once.</summary>
    public const string PaidOff = "All of it, tonight? ...How very thorough of you. The Guild has no further claim on Bram's stall.";

    /// <summary>When Bram's word buys a week.</summary>
    public const string Grace = "Bram's word? How touching. One week, then. The rest comes due next week, and half as much again.";
}
