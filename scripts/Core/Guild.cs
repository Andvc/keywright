using System;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The Gilded Guild: it licenses Keywrights and takes a weekly tithe "to keep the Gate" (and sends it through the Gate,
/// to the Maw). Bram opened chests unlicensed for twenty years, and the back-tithe comes with the stall. Its Collector,
/// Mister Vane, comes for the week's due at the end of every fifth day, and each week's due is bigger than the last. Pay all eight
/// and Bram's debt is cleared; the Guild keeps coming after that, for whatever more it can squeeze (endless). Fall
/// short and you can pawn what's on display and your curios; if that's still not enough, the Guild takes the shop.
/// </summary>
public static class Guild
{
    /// <summary>
    /// A Guild week: three days of trading, then Vane. (Five until 0.20: runs took three hours and more, most of it at the
    /// Royal Emporium doing the same again; three keeps a night with the Guild in every stretch of play.)
    /// </summary>
    public const int DaysPerWeek = 3;

    /// <summary>The weeks of Bram's debt. After the last, the dues keep coming (endless).</summary>
    public const int Weeks = 8;

    /// <summary>
    /// Each week's due: small for the first three weeks, while you learn the stall and a growing purse is all you need,
    /// then climbing (three to six times a week) faster than a learner's shop grows, with the last week the wall. A first
    /// run usually falls in the middle weeks; better judgment and talents push that later, until the debt is cleared. A
    /// strong run can have all of it a week or two early, and pay it off.
    /// </summary>
    static readonly int[] Dues = { 15, 30, 60, 150, 400, 1500, 20000, 170000 };

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
        "Evening. The Guild's tithe, if you please. In gold.",
        "Week two. You're still here. Good: the Guild hates to waste a Keywright.",
        "Two hundred and eighty. Your uncle never made it past week three, you know.",
        "I hear you've been opening a great many chests. The Guild approves. Keep opening.",
        "My masters are curious about you. That's rarely good news.",
        "Thirty thousand. The Guild is starting to take you seriously. So is its master.",
        "One more week after this. Then the tithe's paid. Unless you'd like to sell us the shop now?",
        "The last of it. Every coin, mind. Gold you can see.",
    };

    static readonly string[] Endless =
    {
        "Debts are funny things. They grow when you're not looking.",
        "The Gate is hungry this week. The Guild has decided you owe us for our patience.",
        "Interest, you understand. Nothing personal. My master is very fond of interest.",
    };

    public static string Arrival(int week) => week <= Weeks ? Arrivals[Math.Min(week, Arrivals.Length) - 1] : Endless[(week - Weeks - 1) % Endless.Length];

    public static string Paid(int week) => week switch
    {
        Weeks => "Paid in full. The Guild has no further claim on Bram's stall. For now.",
        > Weeks => "Paid. See you next week.",
        _ => "Paid. Until next week, then.",
    };

    public const string Short = "Short? Then the Guild will take what's on your shelves. What you've got, at our price.";

    public const string Foreclosed = "Not enough. The Guild seals this shop, and your licence with it. Your uncle would be so disappointed.";
}
