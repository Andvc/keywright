using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// A milestone worth a mark: on Steam an achievement (its <see cref="Key"/> is the API name to set up in Steamworks,
/// in capitals), and in the profile either way (<see cref="Profile.Achievements"/>), so nothing earned off Steam is
/// lost. Each is a plain condition on the run being played or on what's been done across runs, checked whenever the
/// game saves, ends a day or ends a run (<see cref="Achievements.Earned"/>), so it can't be missed by a moment.
/// </summary>
public sealed record AchievementDef(string Key, string Name, string Text, Func<GameState, Profile, bool> Done, bool Hidden = false);

public static class Achievements
{
    public static readonly AchievementDef[] All =
    {
        new("FIRST_CHEST", Loc.T("Opened for You"), Loc.T("Buy a chest and open it."), (s, p) => s?.Found.Count > 0 || p.Found.Count > 0),
        new("THORNWICK", Loc.T("Thornwick Market"), Loc.T("Move Bram's stall to Thornwick Market."), (s, p) => Math.Max(s?.Tier ?? 0, p.BestTier) >= 2),
        new("CROWNHOLD", Loc.T("Crownhold"), Loc.T("Move the shop to Crownhold."), (s, p) => Math.Max(s?.Tier ?? 0, p.BestTier) >= 3),
        new("EMPORIUM", Loc.T("By Royal Appointment"), Loc.T("Open the Royal Emporium."), (s, p) => Math.Max(s?.Tier ?? 0, p.BestTier) >= 4),
        new("FIRST_TITHE", Loc.T("The Guild's Due"), Loc.T("Pay the Gilded Guild its first tithe."), (s, p) => s?.WeeksPaid > 0 || p.Cleared > 0),
        new("DEBT_CLEARED", Loc.T("Paid in Full"), Loc.T("Clear Bram's debt to the Guild."), (s, p) => s?.DebtCleared == true || p.Cleared > 0),
        new("EARLY_CLEAR", Loc.T("Ahead of Time"), Loc.T("Clear the debt with a week to spare."),
            (s, p) => s?.DebtCleared == true && s.ClearedWeek > 0 && s.ClearedWeek < Guild.Weeks),
        new("TREASURY", Loc.T("The Treasury"), Loc.T("Open the great chest at the Guild's treasury."), (s, p) => s?.TreasuryEnd is Campaign.Swallowed or Campaign.Morning || p.Swallowed > 0, Hidden: true),
        new("HOME", Loc.T("Two Taps and One"), Loc.T("Bring Bram home."), (s, p) => s?.BramHome == true && !s.Flagged("bargained") || p.Rescues > 0, Hidden: true),
        new("NEXT_DAY", Loc.T("The Next Day"), Loc.T("See a morning the season has never reached."), (s, p) => s?.TreasuryEnd == Campaign.Morning || p.Finished, Hidden: true),
        new("HEARTSONG", Loc.T("A Song in the Box"), Loc.T("Keep a Heartsong."), (s, p) => s?.SongsKept > 0),
        new("SONGBOOK", Loc.T("Sung Whole"), Loc.T("Hear all nine Heartsongs, across your runs."), (s, p) => p.Songs.Count >= Heartsongs.All.Length, Hidden: true),
        new("ENCHANTED", Loc.T("It Glows"), Loc.T("Find an enchanted thing."), (s, p) => s?.MagicFound > 0 || p.FoundEnchanted.Count > 0),
        new("PERFECT_READ", Loc.T("A Perfect Read"), Loc.T("Tick exactly what's in a chest before you open it."), (s, p) => s?.BestStreak > 0),
        new("STREAK", Loc.T("Never Wrong"), Loc.T("Make five perfect reads in a row."), (s, p) => s?.BestStreak >= 5),
        new("FALSE_BOTTOM", Loc.T("Hollow"), Loc.T("Find what's under a false bottom."), (s, p) => s?.BottomsOpened > 0),
        new("WARD", Loc.T("Named"), Loc.T("Name a warded thing, and see it come out."), (s, p) => s?.WardsNamed > 0),
        new("COLLECTOR", Loc.T("Collector"), Loc.T("Find half the things there are, across your runs."),
            (s, p) => p.Found.Count * 2 >= Collection.Entries.Count()),
        new("CATALOGUE", Loc.T("Every Thing in Aldmark"), Loc.T("Find every thing there is, across your runs."),
            (s, p) => Collection.Entries.All(e => p.Found.Contains(e.Entry))),
        new("KIT", Loc.T("The Ear"), Loc.T("Clear the debt as Kit."), (s, p) => Cleared(s, p, Characters.Kit)),
        new("HESK", Loc.T("The Scale"), Loc.T("Clear the debt as Hesk."), (s, p) => Cleared(s, p, Characters.Hesk)),
        new("ROSA", Loc.T("The Tongue"), Loc.T("Clear the debt as Rosa."), (s, p) => Cleared(s, p, Characters.Rosa)),
        new("ISOLDE", Loc.T("The Collection"), Loc.T("Clear the debt as Isolde."), (s, p) => Cleared(s, p, Characters.Isolde)),
        new("LEVEL_4", Loc.T("Hard Bargains"), Loc.T("Clear the debt at challenge level 4."), (s, p) => ClearedAt(s, p, 4)),
        new("LEVEL_8", Loc.T("The Wall"), Loc.T("Clear the debt at challenge level 8."), (s, p) => ClearedAt(s, p, 8)),
    };

    /// <summary>Cleared as this character: in the run being played, or in an earlier one (their level 1 is open).</summary>
    static bool Cleared(GameState s, Profile p, string character) =>
        s?.DebtCleared == true && s.Character == character || p.Challenge.GetValueOrDefault(character) >= 1;

    /// <summary>Cleared at this challenge level or above (which opens the level after it).</summary>
    static bool ClearedAt(GameState s, Profile p, int level) =>
        s?.DebtCleared == true && s.Challenge >= level || p.Challenge.Values.Any(open => open > level);

    public static AchievementDef Get(string key) => All.FirstOrDefault(a => a.Key == key);

    /// <summary>The achievements done now that the profile hasn't marked yet (the run may be null: the title screen).</summary>
    public static List<AchievementDef> Earned(GameState state, Profile profile) =>
        All.Where(a => !profile.Achievements.Contains(a.Key) && a.Done(state, profile)).ToList();
}
