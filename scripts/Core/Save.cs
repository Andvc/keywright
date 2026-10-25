using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Keywright.Core;

/// <summary>
/// Everything that carries over from one day to the next, kept as plain data so it can be written to a file. The shop
/// keeps its books at the start of each day: today's customers are rolled again from the seed when you come back.
/// </summary>
public sealed class SaveData
{
    /// <summary>4: 0.21's five weeks of five days (older saves are moved onto them: <see cref="FromJson"/>).</summary>
    public const int CurrentVersion = 4;

    public int Version { get; set; } = CurrentVersion;
    public string SavedAt { get; set; }

    public int Seed { get; set; }
    public int Gold { get; set; }
    public int Day { get; set; }
    public int Tier { get; set; }
    public bool Guided { get; set; }

    /// <summary>Who's minding the stall, on what terms, with what talents (fixed for the run).</summary>
    public string Character { get; set; }
    public int Challenge { get; set; }
    public List<string> Talents { get; set; } = new();
    public List<string> Snubbed { get; set; } = new();
    public bool MaraUsed { get; set; }
    public bool KeepsakeTaken { get; set; }
    public int TotalEarned { get; set; }
    public bool UpgradesOpen { get; set; }
    public int MagicFound { get; set; }
    public int PerfectStreak { get; set; }
    public int BestStreak { get; set; }

    public List<string> Owned { get; set; } = new();
    public List<string> HintsSeen { get; set; } = new();

    /// <summary>The scripted first day was taught in an earlier run (from the profile): this run starts like any other.</summary>
    public bool FirstDayTaught { get; set; }
    public List<string> Met { get; set; } = new();
    public List<string> Found { get; set; } = new();
    public List<string> FoundEnchanted { get; set; } = new();
    public List<string> Tutored { get; set; } = new();

    /// <summary>Curios seen this run (0.21: the journal's curio page).</summary>
    public List<string> CuriosSeen { get; set; } = new();

    /// <summary>Places heard of.</summary>
    public List<string> Places { get; set; } = new();
    public List<string> ActiveSynergies { get; set; } = new();
    public List<SavedItem> Display { get; set; } = new();
    public List<string> SongsFound { get; set; } = new();

    /// <summary>The journal's tally: kind by kind, five qualities each.</summary>
    public List<int> Acquired { get; set; } = new();

    // The run: the Guild's dues, the map, curios, this week's commissions, event and peddler.
    public int WeeksPaid { get; set; }
    public int Arrears { get; set; }
    public bool GraceUsed { get; set; }
    public int GraceCount { get; set; }
    public int StashUsed { get; set; }
    public int ClearedDay { get; set; }
    public int CloverWeek { get; set; }

    /// <summary>The shop Nim last came to (0.21): on his first visit to a grander one he brings something that opens there.</summary>
    public int PeddlerTier { get; set; }

    /// <summary>The first day at the shop's tier (0.21): with tips on, a quieter one.</summary>
    public int TierSince { get; set; }
    public int DrySpell { get; set; }

    /// <summary>The curios Nim carries this run (null: all of them, as before 0.20).</summary>
    public List<string> CuriosOpen { get; set; }

    /// <summary>The build's counters: chests passed since the last bought, chests in a row with a surprise, surprises, finds, bargains (0.20), perfect reads, false bottoms opened, wards named and chests taken sight unseen (0.21).</summary>
    public int PassStreak { get; set; }
    public int LuckStreak { get; set; }
    public int PerfectReads { get; set; }
    public int BottomsOpened { get; set; }
    public int WardsNamed { get; set; }
    public int SightUnseen { get; set; }
    public int Surprises { get; set; }
    public int HiddenFound { get; set; }
    public int Bargains { get; set; }
    public bool BramPiece { get; set; }
    public int HoodPieces { get; set; }
    public bool MapGiven { get; set; }
    public bool VaultOpened { get; set; }
    public List<string> Curios { get; set; } = new();

    /// <summary>What was paid for each curio, in the same order.</summary>
    public List<int> CurioPaid { get; set; } = new();
    public List<string> EventsSeen { get; set; } = new();

    /// <summary>This week's plan and next week's (the calendar's).</summary>
    public WeekPlan Plan { get; set; }
    public WeekPlan NextPlan { get; set; }

    /// <summary>Before plans (0.18): the week's one event, its day, the buyer's kind and Nim's day.</summary>
    public string WeekEvent { get; set; }
    public int EventDay { get; set; }
    public Category BuyerKind { get; set; }
    public int PeddlerDay { get; set; }
    public List<SavedCommission> Commissions { get; set; } = new();

    static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public string ToJson() => JsonSerializer.Serialize(this, Options);

    /// <summary>
    /// The save in this text, or null if it isn't one this version can read. A shop saved before the Guild came calling
    /// (version 1) picks up as if every week so far had been paid, with Bram's piece of the map if it's past the first.
    /// </summary>
    public static SaveData FromJson(string json)
    {
        try
        {
            var data = JsonSerializer.Deserialize<SaveData>(json);
            if (data == null || data.Tier < 1 || data.Version > CurrentVersion || data.Version < 1) return null;
            if (data.Version == 1)
            {
                data.Version = 2;
                data.WeeksPaid = data.Day / OldDaysPerWeek;
                data.BramPiece = data.Day >= OldDaysPerWeek;
                data.Owned.Remove("warrant");
            }
            // 0.20's three-day weeks (version 3), or the five-day weeks before them (version 2), onto 0.21's five weeks of
            // five days: as much of the debt behind you (eight weeks of it then), at the same point in the week, and the
            // week's plan made again.
            if (data.Version is 2 or 3)
            {
                int oldDays = data.Version == 2 ? OldDaysPerWeek : ShortDaysPerWeek;
                int week = data.Day / oldDays, into = data.Day % oldDays;
                int Weeks(int old) => (int)Math.Round(old * Guild.Weeks / (double)OldWeeks);
                into = data.Version == 3 ? into * 2 : into;
                data.Day = Weeks(week) * Guild.DaysPerWeek + Math.Min(into, Guild.DaysPerWeek - 1);
                data.WeeksPaid = Math.Min(data.Day / Guild.DaysPerWeek, Weeks(data.WeeksPaid));
                if (data.ClearedDay > 0) data.ClearedDay = Math.Max(1, data.Day);
                data.Version = CurrentVersion;
                data.Plan = null;
                data.NextPlan = null;
                data.EventDay = 0;
                data.PeddlerDay = 0;
            }
            return data;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Days in a week before 0.20, and in 0.20's; weeks of Bram's debt before 0.21.</summary>
    const int OldDaysPerWeek = 5, ShortDaysPerWeek = 3, OldWeeks = 8;

    /// <summary>A line for the title screen: "Week 3, day 2, Town Shop, 840g".</summary>
    public string Summary => $"Week {Day / Guild.DaysPerWeek + 1}, day {Day % Guild.DaysPerWeek + 1}, {ShopTiers.Name(Tier)}, {Gold}g";
}

/// <summary>A commission, as it's saved.</summary>
public sealed class SavedCommission
{
    public string Patron { get; set; }
    public CommissionKind Kind { get; set; }
    public Category Category { get; set; }
    public Rarity Quality { get; set; }
    public int Needed { get; set; }
    public int Delivered { get; set; }
    public int Reward { get; set; }
    public bool Story { get; set; }
    public bool Carried { get; set; }
    public int Lapses { get; set; }
}

/// <summary>
/// What you've done across every run, kept apart from the saved shop: runs started, debts cleared, vaults opened, the
/// furthest week reached and the most gold held; renown to spend and the talents it's bought; and for each of Bram's
/// kin, the highest challenge level open and the best score.
/// </summary>
public sealed class Profile
{
    public int Runs { get; set; }
    public int Cleared { get; set; }
    public int Vaults { get; set; }

    /// <summary>The Heartsongs Bram's kin have ever opened (the ledger's songbook): all nine change the Crown Chest's ending.</summary>
    public List<string> Songs { get; set; } = new();

    /// <summary>Crown Chests opened with all nine Heartsongs known: the Locking sung whole.</summary>
    public int SungWhole { get; set; }

    /// <summary>Notes the Heartsongs a run has opened. Returns how many of them are new to the songbook.</summary>
    public int Learn(GameState state)
    {
        int before = Songs.Count;
        foreach (var song in state.SongsFound.Where(s => !Songs.Contains(s))) Songs.Add(song);
        return Songs.Count - before;
    }
    public int BestWeek { get; set; }
    public int BestGold { get; set; }

    /// <summary>The grandest shop any run has had (Nim's stock grows with it).</summary>
    public int BestTier { get; set; }

    /// <summary>Whether Nim carries a curio yet: his stock grows as Bram's kin get further (<see cref="CurioUnlock"/>).</summary>
    public bool Opens(CurioDef curio) => curio.Unlock switch
    {
        CurioUnlock.SecondRun => Runs >= 1,
        CurioUnlock.Crownhold => BestTier >= 3 || Runs >= 2,
        CurioUnlock.Emporium => BestTier >= 4 || Runs >= 4,
        CurioUnlock.Cleared => Cleared > 0,
        CurioUnlock.Heartsong => Songs.Count > 0,
        CurioUnlock.Feat => Curios.Feats.TryGetValue(curio.Key, out var feat) && Done(feat.Feat) >= feat.Needed,
        _ => true,
    };

    /// <summary>Whether Nim will carry a curio once this run is banked: a legendary one counts the run's feats too.</summary>
    public bool OpensAfter(CurioDef curio, GameState run) => Opens(curio) ||
        (curio.Unlock == CurioUnlock.Feat && Curios.Feats.TryGetValue(curio.Key, out var feat) && Done(feat.Feat) + Curios.Progress(run, feat.Feat) >= feat.Needed);

    /// <summary>What Bram's kin have done across every run that opens a legendary curio (a <see cref="Core.Feat"/>'s name: how many).</summary>
    public Dictionary<string, int> Feats { get; set; } = new();

    /// <summary>How many times Bram's kin have done this, all told.</summary>
    public int Done(Feat feat) => Feats.GetValueOrDefault(feat.ToString());

    /// <summary>
    /// The collection journal, across runs: the regulars you've met (their habits), the things you've found (and found
    /// enchanted), and the curios you've seen. Each unlocks when you first come across it and stays unlocked.
    /// </summary>
    public List<string> Met { get; set; } = new();
    public List<string> Found { get; set; } = new();
    public List<string> FoundEnchanted { get; set; } = new();
    public List<string> CuriosSeen { get; set; } = new();

    /// <summary>Everything Nim carries now.</summary>
    public IEnumerable<string> OpenCurios => Curios.All.Where(Opens).Select(c => c.Key);

    /// <summary>The fewest days it's taken to clear Bram's debt (0 until it's been cleared).</summary>
    public int FastestClear { get; set; }

    /// <summary>Renown to spend on talents, and all that's ever been earned.</summary>
    public int Renown { get; set; }
    public int RenownEarned { get; set; }

    public List<string> Talents { get; set; } = new();

    /// <summary>
    /// What Bram has taught, across runs: his tips by key, each seller's lesson ("lesson:" and their key), and the scripted
    /// first day ("firstday"). A new run with tips on doesn't teach them again (Settings can start the lessons over).
    /// </summary>
    public List<string> Taught { get; set; } = new();

    /// <summary>Per character: the highest challenge level open (clearing the debt at a level opens the next).</summary>
    public Dictionary<string, int> Challenge { get; set; } = new();

    /// <summary>Per character: the best score a run of theirs has made.</summary>
    public Dictionary<string, int> BestScore { get; set; } = new();

    /// <summary>Who and what level the last run was, for the character screen.</summary>
    public string LastCharacter { get; set; } = Characters.Kit;
    public int LastChallenge { get; set; }

    public bool Unlocked(CharacterDef character) => character.Unlocked(this);

    public int MaxChallenge(string character) => Challenge.GetValueOrDefault(character);

    public bool Knows(string talent) => Talents.Contains(talent);

    /// <summary>A talent that's next on its branch (what it builds on is learned) and affordable.</summary>
    public bool CanLearn(TalentDef talent) => !Knows(talent.Key) && (talent.Requires == null || Knows(talent.Requires)) && Renown >= talent.Cost;

    public bool Learn(TalentDef talent)
    {
        if (!CanLearn(talent)) return false;
        Renown -= talent.Cost;
        Talents.Add(talent.Key);
        return true;
    }

    /// <summary>The debt's been cleared at this run's level: the next level opens for this character.</summary>
    public void ClearedAt(GameState state) =>
        Challenge[state.Character] = Math.Max(MaxChallenge(state.Character), Math.Min(Challenges.Max, state.Challenge + 1));

    /// <summary>A run is over: its renown and its feats are banked, and its score noted. Returns the renown it earned.</summary>
    public int Bank(GameState state)
    {
        foreach (var feat in Enum.GetValues<Feat>()) Feats[feat.ToString()] = Done(feat) + Curios.Progress(state, feat);
        int renown = Score.Renown(state), score = Score.Total(state);
        Renown += renown;
        RenownEarned += renown;
        BestScore[state.Character] = Math.Max(BestScore.GetValueOrDefault(state.Character), score);
        return renown;
    }

    static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public string ToJson() => JsonSerializer.Serialize(this, Options);

    public static Profile FromJson(string json)
    {
        try
        {
            var profile = JsonSerializer.Deserialize<Profile>(json) ?? new Profile();
            // Bram's Ledger (a talent until 0.21, when every regular you meet stays in the ledger anyway): its renown back.
            if (profile.Talents.Remove("ledger")) profile.Renown += Core.Talents.LedgerRefund;
            profile.Talents.RemoveAll(t => !Core.Talents.Exists(t));
            profile.Songs.RemoveAll(s => !Heartsongs.Exists(s));
            return profile;
        }
        catch (JsonException)
        {
            return new Profile();
        }
    }

    /// <summary>Notes how far a run got (call as it goes, and when it ends).</summary>
    public void Note(GameState state)
    {
        Learn(state);
        Remember(Met, state.Met);
        Remember(Found, state.Found);
        Remember(FoundEnchanted, state.FoundEnchanted);
        Remember(CuriosSeen, state.CuriosSeen);
        foreach (string key in state.Taught.Where(k => !Taught.Contains(k))) Taught.Add(key);
        BestWeek = Math.Max(BestWeek, state.Week);
        BestGold = Math.Max(BestGold, state.Gold);
        BestTier = Math.Max(BestTier, state.Tier);
        if (state.ClearedDay > 0 && (FastestClear == 0 || state.ClearedDay < FastestClear)) FastestClear = state.ClearedDay;
    }

    static void Remember(List<string> journal, IEnumerable<string> found)
    {
        foreach (string key in found) if (!journal.Contains(key)) journal.Add(key);
    }

    /// <summary>For the GM: every page of the journal filled, and every Heartsong known.</summary>
    public void KnowAll()
    {
        Remember(Met, Sellers.All.Select(s => s.Key));
        Remember(Found, Collection.Entries.Select(e => e.Entry));
        Remember(FoundEnchanted, Collection.Entries.Where(e => !e.Fake).Select(e => e.Entry));
        Remember(CuriosSeen, Curios.All.Select(c => c.Key));
        Remember(Songs, Heartsongs.All.Select(h => h.Key));
    }

    /// <summary>A line for the title screen, or null before the first run.</summary>
    public string Summary => Runs == 0 ? null
        : $"Best: week {BestWeek}   Debts cleared: {Cleared}{(FastestClear > 0 ? $" (fastest: day {FastestClear})" : "")}   Crown Chests opened: {Vaults}" +
          (Songs.Count > 0 ? $"   Heartsongs: {Songs.Count}/{Heartsongs.All.Length}" : "");
}

/// <summary>Something on display, as it's saved.</summary>
public sealed class SavedItem
{
    public Category Category { get; set; }
    public Rarity Quality { get; set; }
    public bool Magic { get; set; }

    /// <summary>Which Heartsong (for the Heartsong kind).</summary>
    public string Song { get; set; }
}

/// <summary>
/// What Bram's kin remember from earlier runs (the profile's journal, <see cref="Profile.Met"/> and the rest): regulars
/// met, things found and found enchanted, curios seen. A run starts with it and adds its own.
/// </summary>
public sealed class Journal
{
    public HashSet<string> Met { get; } = new();
    public HashSet<string> Found { get; } = new();
    public HashSet<string> Enchanted { get; } = new();
    public HashSet<string> Curios { get; } = new();
}
