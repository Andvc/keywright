using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The campaign across seasons (0.34, docs/narrative.md). Bram minds the stall with you for a week, then goes with Vane to
/// the Guild's treasury; you keep his account paid and earn the treasury key's four pieces from the trade, and on the night
/// the account is paid with the key whole you open the treasury's chest, which is the Maw, and it swallows you: the seal's
/// safeguard turns the year back, and from then on you remember. Later seasons are the investigation (the Inquiry), the
/// rescue (crates from the weighhouse, a full mimic, a note redeemed) and the plan; the true ending opens the door
/// without giving the Maw anything, and the next day comes. Every season starts the same: only what you know carries over.
/// </summary>
public static class Campaign
{
    /// <summary>The treasury key's four pieces, each from a mark of the trade (docs/narrative.md).</summary>
    public const string Reader = "reader", Warden = "warden", Patron = "patron", Locksmith = "locksmith"; // loc: keys
    public static readonly string[] Pieces = { Reader, Warden, Patron, Locksmith };

    /// <summary>Who can be in a Guild crate: Bram, and Old Haddon if his note wasn't paid.</summary>
    public const string Bram = Story.Bram, Haddon = "haddon";

    /// <summary>Old Haddon's note to the Guild (his lathe): what it takes to redeem him, at any shop.</summary>
    public const int HaddonNote = 40;

    /// <summary>
    /// The master's fee the Guild pays with each piece of the key, in working purses (it wants the key whole: what the
    /// Hooded Woman's shard commissions paid until 0.34).
    /// </summary>
    public const float FeePurses = 0.5f;

    /// <summary>Two friends who'll stand with you make a way out for the freed (or the Wet Hen, hired).</summary>
    public const int AlliesNeeded = 2;

    /// <summary>The Lantern Market's prices, in working purses: a night at the weighhouse, the Wet Hen for the last night, a crate diverted.</summary>
    public const float PassPurses = 0.5f, BargePurses = 1f, DivertPurses = 1f;

    /// <summary>How a season ended, as the profile keeps it for the next one.</summary>
    public const string Swallowed = "swallowed", Stayed = "stayed", Bargained = "bargain", Morning = "morning", Free = "free", Lost = "foreclosed"; // loc: keys

    public static string PieceName(string piece) => piece switch
    {
        Reader => Loc.T("The reader's piece"),
        Warden => Loc.T("The warden's piece"),
        Patron => Loc.T("The patron's piece"),
        _ => Loc.T("The locksmith's piece"),
    };

    /// <summary>How a piece is earned, as the Guild's letter and the notebook put it.</summary>
    public static string PieceHow(string piece) => piece switch
    {
        Reader => Loc.T("Read a chest true that hid something (a false bottom or a ward), or one holding five things or more."),
        Warden => Loc.T("Settle a stolen chest: send it home, or hand it to the Wardens."),
        Patron => Loc.T("Fill a commission: every thing it asked for, delivered."),
        _ => Loc.T("Pick an old lock without force: a Guild Lock masterfully, or a Dwarven Tumbler cleanly."),
    };
}

public partial class GameState
{
    // ---- Across seasons (the profile's, kept with the run) ----

    /// <summary>You've been swallowed at the treasury before: you remember the seasons since (the profile's, at the season's start).</summary>
    public bool Remembers { get; set; }

    /// <summary>The true ending has been reached on this profile: a new season is a new year, and the main story is done.</summary>
    public bool CampaignDone { get; set; }

    /// <summary>Seasons played before this one, and how the last one ended (<see cref="Campaign"/>'s ends), from the profile.</summary>
    public int SeasonsBefore { get; set; }

    /// <summary>
    /// A season's been played on this profile before (0.37): the three states a season starts in are a first-ever one
    /// (everything taught, each of the trade's ways met a market at a time), a later one before the true ending (this:
    /// nothing said twice, every way met so far open from the first morning, <see cref="Ways"/>), and a new year after it
    /// (<see cref="CampaignDone"/>).
    /// </summary>
    public bool Veteran => SeasonsBefore > 0 || VeteranTried;

    /// <summary>For the bot and the sim (`--veteran`): a season played as a later one.</summary>
    public static bool VeteranTried { get; set; }
    public string LastEnd { get; set; }

    /// <summary>Bram has been brought home in an earlier season (the Maw's last offer says so).</summary>
    public bool SavedBramBefore { get; set; }

    /// <summary>What the first-morning chest held, the first time it was opened on this profile (kinds, as item keys).</summary>
    public List<string> FirstChest { get; } = new();

    /// <summary>A run picked up from a save (its campaign facts are its own, not the profile's now).</summary>
    public bool Continued { get; private set; }

    // ---- This season ----

    /// <summary>The treasury key's pieces earned this season.</summary>
    public HashSet<string> Pieces { get; } = new();

    public bool KeyWhole => Pieces.Count >= Campaign.Pieces.Length;

    /// <summary>
    /// Pieces just earned, with the fee each came with and how many the key had then, for the screen to show one at a time.
    /// </summary>
    public Queue<(string Piece, int Fee, int Count)> NewPieces { get; } = new();

    /// <summary>Bram has gone with Vane to the treasury (the first Guild night).</summary>
    public bool BramAway { get; private set; }

    /// <summary>Bram is home, freed and his pledge redeemed (or sent home by the Maw's bargain), for the rest of the season.</summary>
    public bool BramHome { get; private set; }

    /// <summary>The gilded in the back room, by person: whether a mimic has eaten the gold off them yet.</summary>
    public Dictionary<string, bool> Guests { get; } = new();

    /// <summary>Brought home this season: freed, and their note redeemed.</summary>
    public HashSet<string> Rescued { get; } = new();

    /// <summary>Who owes the Guild, and the week whose Guild night their note falls due on.</summary>
    public Dictionary<string, int> Claims { get; } = new();

    /// <summary>Who the Guild has taken this season (their note fell due unpaid), and the week it took them.</summary>
    public Dictionary<string, int> Taken { get; } = new();

    /// <summary>The treasury's been opened this season (swallowed, stayed away, or the final opening), and how it ended.</summary>
    public string TreasuryEnd { get; private set; }

    public bool TreasuryDone => TreasuryEnd != null;

    /// <summary>
    /// Bram is with you at the stall: the first week, before the summons; once he's home; and in every season after the
    /// true ending.
    /// </summary>
    public bool BramHere => !BramAway || BramHome || CampaignDone;

    /// <summary>Whether Bram is the one who speaks: present, or (away) through the notes in his notebook.</summary>
    public bool BramNotes => !BramHere;

    // ---- The key ----

    /// <summary>
    /// A piece of the treasury key, for a mark of the trade: the Guild's assessors send it, with a master's fee (a note on
    /// the screen). True the first time.
    /// </summary>
    public bool EarnPiece(string piece)
    {
        // Bram home on the Maw's bargain: no treasury for you this season, so no key either.
        if (CampaignDone || TreasuryDone || Flagged("bargained") || !Campaign.Pieces.Contains(piece) || !Pieces.Add(piece)) return false;
        int fee = Purses(Campaign.FeePurses);
        TaleGold(fee);
        NewPieces.Enqueue((piece, fee, Pieces.Count));
        Log(Story.Tale, Loc.T($"{Campaign.PieceName(piece)} of the treasury key came by Guild courier, with a master's fee of {fee}g and Mister Vane's compliments."));
        return true;
    }

    /// <summary>A chest read true: the reader's piece, if it hid something or held five things or more.</summary>
    public void ReadTrue(Chest chest, bool perfect)
    {
        if (!perfect || chest == null || chest.Breathing || chest.Crate != null) return;
        if (chest.Hidden != null || chest.Unwarded != null || chest.Everything.Count(i => !i.IsSong) >= 5) EarnPiece(Campaign.Reader);
    }

    /// <summary>A lock picked: the locksmith's piece for an old lock opened without force.</summary>
    public void Picked(LockTier lockTier, PickGrade grade)
    {
        if (lockTier == null) return;
        if ((lockTier.Index >= 2 && grade == PickGrade.Masterful) || (lockTier.Index >= 3 && grade != PickGrade.Forced)) EarnPiece(Campaign.Locksmith);
    }

    // ---- Bram ----

    /// <summary>The first Guild night, its due settled: Vane brings the treasury's summons, and Bram goes.</summary>
    public bool SummonsTonight => !CampaignDone && !BramAway && Week == 1 && DuesTonight && PaidThisWeek && !Foreclosed;

    /// <summary>Bram goes with Vane. Old Haddon too, if his note wasn't paid.</summary>
    public void Summon()
    {
        BramAway = true;
        Note("summons");
    }

    // ---- The Guild's claims, and the gilded ----

    /// <summary>Someone owes the Guild: on that week's Guild night, if it's still unpaid, the Guild takes them.</summary>
    public void Claim(string person, int week) => Claims[person] = week;

    /// <summary>A claim paid off: they're safe. True if there was one.</summary>
    public bool Redeem(string person) => Claims.Remove(person);

    /// <summary>The Guild has taken them this season, and they're not home.</summary>
    public bool IsTaken(string person) => Taken.ContainsKey(person) && !Rescued.Contains(person);

    /// <summary>
    /// The Guild's night (as the night starts, paid or not): a kept mimic eats, every claim fallen due is taken, and anyone
    /// freed whose note still stands is taken again.
    /// </summary>
    public void GuildCollects()
    {
        // Once a night: before the summons, if that's tonight, or as the night starts.
        if (!DuesTonight || _collectedOn == Day) return;
        _collectedOn = Day;
        FeedMimic();
        foreach (var (person, week) in Claims.Where(c => c.Value <= Week).OrderBy(c => c.Key, StringComparer.Ordinal).ToList())
        {
            Claims.Remove(person);
            Taken[person] = Week;
            if (person == Campaign.Haddon) Note("haddon.taken");
            else
            {
                Goes(person);
                Log(Story.Tale, Loc.T($"The Guild took {Sellers.Person(person).Name} on its night, to work off the note."));
            }
        }
        // Freed, but the note still stands: the Guild's men come back for them.
        foreach (string person in Guests.Where(g => g.Value && !Resolved(g.Key)).Select(g => g.Key).OrderBy(k => k, StringComparer.Ordinal).ToList())
        {
            Guests.Remove(person);
            Log(Story.Tale, Loc.T($"The Guild's men came to the back door for {Story.Name(person)}. Their note still stood, and they took them back."));
        }
    }

    int _collectedOn;

    /// <summary>Whether a guest's note is settled: Bram's is his account (the debt), Haddon's his own.</summary>
    public bool Resolved(string person) => person == Campaign.Bram ? DebtCleared : !Claims.ContainsKey(person) && Flagged($"{person}.redeemed"); // loc: keys

    /// <summary>What it takes to redeem a guest's note now (0 if it's settled, or can't be paid here: Bram's is the debt).</summary>
    public int NoteFor(string person) => person == Campaign.Haddon && !Resolved(person) ? Campaign.HaddonNote : 0;

    /// <summary>A crate opened: someone the Guild took is in the back room, gold from head to foot.</summary>
    public void TakeIn(string person)
    {
        if (Guests.ContainsKey(person) || Rescued.Contains(person)) return;
        Guests[person] = false;
        Note(person == Campaign.Bram ? "tap" : "alive");
        Log(Story.Tale, Loc.T($"{Story.Name(person)} is in the back room: gold from head to foot, and knocking now and then."));
    }

    /// <summary>A full mimic eats the gold off a guest. True if it could.</summary>
    public bool Ungild(string person)
    {
        if (!Guests.TryGetValue(person, out bool freed) || freed || !MimicFull) return false;
        Guests[person] = true;
        Mimic = 0;
        Note("shell");
        Log(Story.Tale, Loc.T($"Your mimic ate the gold off {Story.Name(person)}, and fell asleep again. They're cold, and shaking, and alive."));
        TryHome(person);
        return true;
    }

    /// <summary>Pays a guest's note (Haddon's). True if it was paid.</summary>
    public bool RedeemGuest(string person)
    {
        int note = NoteFor(person);
        if (note <= 0 || Gold < note) return false;
        Gold -= note;
        Claims.Remove(person);
        Flags.Add($"{person}.redeemed"); // loc: keys
        Log(Story.Tale, Loc.T($"You paid {Story.Name(person)}'s note to the Guild: {note}g."));
        TryHome(person);
        return true;
    }

    /// <summary>Freed and their note settled: home. Bram's homecoming waits for its scene (<see cref="BramComesHome"/>).</summary>
    public void TryHome(string person)
    {
        if (!Guests.TryGetValue(person, out bool freed) || !freed || !Resolved(person)) return;
        Guests.Remove(person);
        Rescued.Add(person);
        if (person == Campaign.Bram) Flags.Add(BramComing);
        else
        {
            Sway(Faction.Commons, 1);
            Log(Story.Tale, Loc.T($"{Story.Name(person)} walked home tonight. Cold, and furious with the Guild, and home."));
        }
    }

    /// <summary>Bram is freed and redeemed: his homecoming tonight (a scene), then he's home.</summary>
    public const string BramComing = "bram.coming"; // loc: keys

    public bool BramComesHome => Flagged(BramComing) && !BramHome;

    public void WelcomeBram()
    {
        Flags.Remove(BramComing);
        BramHome = true;
        Note("pledge");
        Note("home");
    }

    // ---- The Maw's tactics, once you remember (Vane's Guild nights) ----

    /// <summary>The Guild nights of the Maw's tactics (0.36: the second and third of four; the fourth and sixth of seven until then).</summary>
    public const int BribeWeek = 2, BargainWeek = 3;

    /// <summary>The second Guild night: half the due, if your questions stop at the toll.</summary>
    public bool BribeTonight => Remembers && !CampaignDone && DuesTonight && !PaidThisWeek && Week == BribeWeek && !DebtCleared && !Flagged("bribe.asked");

    /// <summary>The third: Bram home tomorrow, warm and whole, for the key's pieces.</summary>
    public bool BargainTonight => Remembers && !CampaignDone && DuesTonight && !PaidThisWeek && Week == BargainWeek && Pieces.Count > 0 && !BramHome &&
                                  !TreasuryDone && !Flagged("bargain.asked");

    public void AnswerBribe(bool take)
    {
        Flags.Add("bribe.asked");
        if (!take) return;
        Flags.Add("bribed");
        _halfDue = true;
    }

    public void AnswerBargain(bool take)
    {
        Flags.Add("bargain.asked");
        if (!take) return;
        Flags.Add("bargained");
        Pieces.Clear();
        Guests.Remove(Campaign.Bram);
        BramHome = true;
    }

    // ---- The plan, and the treasury ----

    public bool PlanFind => KnowsAlive && Noted.Contains("register");
    public bool PlanFree => DebtCleared && MimicFull;
    public bool PlanWayOut => Story.Allies(this, tonight: false).Count >= Campaign.AlliesNeeded || Flagged("barge.hired");
    public bool PlanDoor => KeyWhole && Knows("seal") && Knows("charter");
    public bool PlanWhole => Remembers && !CampaignDone && PlanFind && PlanFree && PlanWayOut && PlanDoor;

    /// <summary>
    /// The treasury tonight: the account paid with the key whole, on or before the season's last night. (A season that
    /// ends without it ends; the Guild opens the treasury without you, and the year turns.)
    /// </summary>
    public bool TreasuryDue => !CampaignDone && !TreasuryDone && !Flagged("bargained") && DebtCleared && KeyWhole && Week <= Guild.Weeks && !Foreclosed;

    /// <summary>Opened it, and it swallowed you.</summary>
    public void Swallow()
    {
        TreasuryEnd = Campaign.Swallowed;
        Note("swallowed");
    }

    /// <summary>Remembered, and stayed away: the Guild opens it with Bram.</summary>
    public void StayAway() => TreasuryEnd = Campaign.Stayed;

    /// <summary>The Maw's last offer, taken: the door left to it (and then it reaches past the door, and the year turns).</summary>
    public void LeaveTheDoor() => TreasuryEnd = Campaign.Bargained;

    /// <summary>The door closed on it with everyone out: the true ending.</summary>
    public void CloseTheDoor()
    {
        TreasuryEnd = Campaign.Morning;
        // Bram came out of the chest with everyone else, if he wasn't home already (the season's end says which).
        if (!BramHome) Flags.Add(BramFromTheChest);
        // Everyone the Guild took this season was in the chest, and came out with the rest.
        foreach (string person in Guests.Keys.Concat(Taken.Keys).ToList()) Rescued.Add(person);
        Guests.Clear();
        Rescued.Add(Campaign.Bram);
        BramHome = true;
    }

    public const string BramFromTheChest = "bram.door"; // loc: keys

    /// <summary>The next day's one chest (0.34): Wynn's father's millwright's chest, for the waterwheel, given freely.</summary>
    public const string WaterwheelBeat = "next.day"; // loc: keys

    /// <summary>
    /// The next day, after the door's been closed: a date the season has never reached, with one customer at the counter,
    /// Wynn the miller, with an ordinary chest of tools for the waterwheel.
    /// </summary>
    public void BeginNextDay()
    {
        Day++;
        Queue.Clear();
        var chest = new Chest(new[]
        {
            new Item(Category.Arms, Rarity.Rare), new Item(Category.Arms, Rarity.Uncommon), new Item(Category.Coins, Rarity.Common),
        }, Rarity.Uncommon, lockTier: LockTiers.All[1]);
        var visit = Market.Price(new Random(Seed), Sellers.Person(TaleBook.Wynn), chest, Tier, ask: Challenges.GiftPrice, trade: Trade.Fixed,
            pitch: Loc.T("The millwright's chest. My father's tools are in it, and the wheel won't turn without them. One coin, for luck."));
        visit.Gift = true;
        visit.Beat = WaterwheelBeat;
        Queue.Add(visit);
    }

    /// <summary>The first-morning chest's been bought: what was in it, for every spring after (the profile keeps the first).</summary>
    public void RecordFirstChest(Chest chest)
    {
        if (FirstChest.Count > 0 || chest == null) return;
        FirstChest.AddRange(chest.Everything.Where(i => !i.IsSong).Select(i => i.Entry));
    }

    /// <summary>For the treasury demo (--treasury-demo): the last night, the account paid, the key whole, and (final) the plan whole.</summary>
    public void DemoTreasury(bool final)
    {
        while (Week < Guild.Weeks) Day += Guild.DaysPerWeek;
        WeeksPaid = Guild.Weeks;
        ClearedDay = Day;
        BramAway = true;
        foreach (string piece in Campaign.Pieces) Pieces.Add(piece);
        if (!final) return;
        Remembers = true;
        foreach (string key in new[] { "swallowed", "again", "weighhouse", "register", "alive", "tap", "seal", "charter", "mimic", "full" }) Note(key);
        Mimic = Mimics.GrowsAfter;
        Flags.Add("barge.hired");
        NewEntries.Clear();
        Guests[Campaign.Bram] = true;
        Rescued.Add(Campaign.Bram);
        Guests.Remove(Campaign.Bram);
        BramHome = true;
    }

    /// <summary>How this season ended (or would, now), for the profile and the next first morning.</summary>
    public string SeasonEndKey => TreasuryEnd ?? (DebtCleared ? Campaign.Free : Campaign.Lost);

    // ---- The season begins ----

    /// <summary>
    /// What a season knows of the campaign, from the profile (a new run's start, or a saved one's): what's known, whether
    /// you remember, and what the first-morning chest held; Old Haddon's note, due on the first Guild night.
    /// </summary>
    void RememberCampaign(Profile profile)
    {
        CampaignDone |= profile.Finished;
        // After the true ending, a season is a new year: nobody's note is the story's.
        if (CampaignDone) Claims.Remove(Campaign.Haddon);
        if (Continued) return;
        Remembers = profile.Swallowed > 0;
        SavedBramBefore = profile.Rescues > 0;
        KnownBefore.UnionWith(profile.Known.Where(k => Inquiry.Get(k) is Entry { Held: false }));
        FirstChest.Clear();
        FirstChest.AddRange(profile.FirstChest);
        SeasonsBefore = profile.Runs;
        LastEnd = profile.LastEnd;
        if (Remembers && !CampaignDone) Note("again");
    }

    /// <summary>A new season's campaign: Haddon's note is due on the first Guild night.</summary>
    void StartCampaign() => Claim(Campaign.Haddon, 1);

    /// <summary>Each morning: in the third week, still no letter from Bram.</summary>
    void CampaignMorning()
    {
        if (Week == 3 && DayOfWeek == 1 && BramNotes && !CampaignDone) Note("noletter");
    }

    void SaveCampaign(SaveData data)
    {
        data.Remembers = Remembers;
        data.CampaignDone = CampaignDone;
        data.SeasonsBefore = SeasonsBefore;
        data.LastEnd = LastEnd;
        data.SavedBramBefore = SavedBramBefore;
        data.FirstChest = FirstChest.ToList();
        data.Pieces = Pieces.ToList();
        data.BramAway = BramAway;
        data.BramHome = BramHome;
        data.Guests = new Dictionary<string, bool>(Guests);
        data.Rescued = Rescued.ToList();
        data.Claims = new Dictionary<string, int>(Claims);
        data.Taken = new Dictionary<string, int>(Taken);
        data.TreasuryEnd = TreasuryEnd;
    }

    void LoadCampaign(SaveData data)
    {
        Remembers = data.Remembers;
        CampaignDone = data.CampaignDone;
        SeasonsBefore = data.SeasonsBefore;
        LastEnd = data.LastEnd;
        SavedBramBefore = data.SavedBramBefore;
        FirstChest.AddRange(data.FirstChest ?? new List<string>());
        Pieces.UnionWith((data.Pieces ?? new List<string>()).Where(Campaign.Pieces.Contains));
        BramAway = data.BramAway;
        BramHome = data.BramHome;
        foreach (var (person, freed) in data.Guests ?? new Dictionary<string, bool>()) Guests[person] = freed;
        Rescued.UnionWith(data.Rescued ?? new List<string>());
        foreach (var (person, week) in data.Claims ?? new Dictionary<string, int>()) Claims[person] = week;
        foreach (var (person, week) in data.Taken ?? new Dictionary<string, int>()) Taken[person] = week;
        TreasuryEnd = data.TreasuryEnd;
        Continued = true;
    }
}
