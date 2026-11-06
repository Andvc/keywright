using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The Gilding (docs/narrative.md): what's pledged to the Guild and never redeemed is the Maw's, and what the Maw owns
/// turns to gold. Bram pledged his own hand twenty years ago, for a marked child; this spring the Guild called the pledge
/// in (the seven weeks of the debt), and the gold climbs his arm by the calendar until it's redeemed. Someone a tale has
/// made yours who owes the Guild (a claim) falls due on a Guild night, and if it's still unpaid the Guild collects them:
/// they stand gilded where they were taken, until the Locking's sung again. None of it is a roll.
/// </summary>
public partial class GameState
{
    /// <summary>
    /// Bram's pledge is redeemed: the debt paid (by you, not the Maw), or every pledge to the Maw made void (the Crown Chest
    /// opened and the Locking sung again).
    /// </summary>
    public bool PledgeRedeemed => (DebtCleared && !GateOpens) || VaultOpened;

    /// <summary>
    /// How far the gold has climbed Bram's arm: 0 none (or his pledge redeemed), 1 the hand, 2 the forearm, 3 the throat.
    /// A step every other week, by the calendar: the hand from the start, the forearm from the third week, the throat from
    /// the fifth.
    /// </summary>
    public int BramGilt => PledgeRedeemed || Day <= 0 ? 0 : Week >= 5 ? 3 : Week >= 3 ? 2 : 1;

    /// <summary>Bram wears a glove over his hand until he tells you why (week 4) or you've worked it out (this season or one before).</summary>
    public bool BramGloved => BramGilt > 0 && !Clues.Contains(Story.Told) && !Solved.Contains(Case.Glove) && !KnownBefore.Contains(Case.Glove);

    /// <summary>Vane's offer for Bram's pledge comes tonight (the sixth Guild night, with shards to give and the debt still owed).</summary>
    public bool PledgeOfferTonight => DuesTonight && Week == Story.PledgeWeek && !PaidThisWeek && MapPieces > 0 && !MapGiven && !DebtCleared;

    /// <summary>Who owes the Guild, and the week whose Guild night their note falls due on.</summary>
    public Dictionary<string, int> Claims { get; } = new();

    /// <summary>Who the Guild has collected this season, and the shop they were taken at: gilded, standing there.</summary>
    public Dictionary<string, int> Gilded { get; } = new();

    /// <summary>Someone owes the Guild: on that week's Guild night, if it's still unpaid, the Guild collects them.</summary>
    public void Claim(string person, int week) => Claims[person] = week;

    /// <summary>A claim paid off: they're safe. True if there was one.</summary>
    public bool Redeem(string person) => Claims.Remove(person);

    /// <summary>The Guild has collected them this season (and the Locking hasn't been sung again since).</summary>
    public bool IsGilded(string person) => Gilded.ContainsKey(person) && !VaultOpened;

    /// <summary>Old Haddon stands at the crossroads until the Guild's cart comes for him, on the first Guild night.</summary>
    public bool HaddonStands => !Flagged(Story.Carted) && !VaultOpened;

    /// <summary>
    /// The Guild's night (called as the night starts, whether it was paid or not): Haddon carted off on the first, and
    /// every claim fallen due collected.
    /// </summary>
    void GuildCollects()
    {
        if (!DuesTonight) return;
        FeedMimic();
        Flags.Add(Story.Carted);
        foreach (var (person, week) in Claims.Where(c => c.Value <= Week).OrderBy(c => c.Key, StringComparer.Ordinal).ToList())
        {
            Claims.Remove(person);
            Gilded[person] = Tier;
            Goes(person);
            Log(Story.Tale, Loc.T($"The Guild collected {Sellers.Person(person).Name} on its night: gold from head to foot, where they were taken."));
        }
    }

    /// <summary>
    /// The gilded in sight of the counter at a shop (figure looks, nearest first): Haddon at the crossroads until the
    /// cart, Thornwick's three in the square, and whoever the Guild collected there. None once the Locking's sung again.
    /// </summary>
    public IEnumerable<string> Statues(int tier)
    {
        if (VaultOpened) yield break;
        if (tier == 1 && HaddonStands) yield return Story.HaddonLook;
        foreach (var (person, at) in Gilded.OrderBy(g => g.Key, StringComparer.Ordinal))
            if (at == tier) yield return Sellers.LookOf(person);
        if (tier == 2)
            foreach (string look in Story.ThornwickGilded.Skip(Flagged(Mimics.ThornwickWoke) ? 1 : 0)) yield return look;
    }

    void SaveGilding(SaveData data)
    {
        data.Claims = new Dictionary<string, int>(Claims);
        data.Gilded = new Dictionary<string, int>(Gilded);
    }

    void LoadGilding(SaveData data)
    {
        foreach (var (person, week) in data.Claims ?? new Dictionary<string, int>()) Claims[person] = week;
        foreach (var (person, tier) in data.Gilded ?? new Dictionary<string, int>()) Gilded[person] = tier;
    }
}
