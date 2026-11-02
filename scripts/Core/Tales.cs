using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>The powers of Aldmark that keep an account of a Keywright (docs/world.md, docs/stories.md).</summary>
public enum Faction { Commons, Guild, Wardens, Lockless, Crown, Abbey, Wood, Arcanum, Ironroot }

public static class Factions
{
    public const int Min = -3, Max = 3;

    /// <summary>Where an account starts to change the trade: +2 is a friend, -2 a foe.</summary>
    public const int Strong = 2;

    public static readonly Faction[] All = Enum.GetValues<Faction>();

    /// <summary>The four whose account changes the trade itself; the others' is read by their tales and their people's offers.</summary>
    public static readonly Faction[] Ruling = { Faction.Commons, Faction.Guild, Faction.Wardens, Faction.Lockless };

    public static string Name(Faction faction) => faction switch
    {
        Faction.Commons => Loc.T("The commons"),
        Faction.Guild => Loc.T("The Gilded Guild"),
        Faction.Wardens => Loc.T("The Wardens"),
        Faction.Lockless => Loc.T("The Lockless"),
        Faction.Crown => Loc.T("The Crown"),
        Faction.Abbey => Loc.T("The abbey"),
        Faction.Wood => Loc.T("The Old Wood"),
        Faction.Arcanum => Loc.T("The Arcanum"),
        _ => Loc.T("Ironroot"),
    };

    /// <summary>Who they are, in a line, for the Town page.</summary>
    public static string Who(Faction faction) => faction switch
    {
        Faction.Commons => Loc.T("Farmers, innkeepers, widows and children: everyone who pays the tithe by selling what their grandparents left them."),
        Faction.Guild => Loc.T("It licenses Keywrights, takes the tithe, lends to the Crown and forecloses on debtors."),
        Faction.Wardens => Loc.T("The road-wardens: tolls, the peace, the Chest Roll, and every stolen chest."),
        Faction.Lockless => Loc.T("Thieves, fences, smugglers and hedge-wrights. Their queen is the Magpie."),
        Faction.Crown => Loc.T("King Aldric IX, his court, and a treasury he can't spend."),
        Faction.Abbey => Loc.T("The abbey of Saint Ide at Thornwick, its relics and its almoners."),
        Faction.Wood => Loc.T("The fair folk and the druids, who trade in favours and keep their promises."),
        Faction.Arcanum => Loc.T("Crownhold's college of archmages, licensed by the Guild."),
        _ => Loc.T("The dwarves, who forge the locks and won't pay the Guild's tithe."),
    };

    /// <summary>What an account at this level means, in a word.</summary>
    public static string Word(int standing) => standing switch
    {
        <= -3 => Loc.T("hated"),
        -2 => Loc.T("a foe"),
        -1 => Loc.T("wary"),
        0 => Loc.T("unknown"),
        1 => Loc.T("warm"),
        2 => Loc.T("a friend"),
        _ => Loc.T("an ally"),
    };

    /// <summary>What a friend of theirs gets, and a foe, in a sentence each, for the Town page.</summary>
    public static (string Friend, string Foe) Effects(Faction faction) => faction switch
    {
        Faction.Commons => (Loc.T("They send their neighbours: one more customer a day."), Loc.T("They stay away: one customer fewer a day.")),
        Faction.Guild => (Loc.T("It overlooks a twentieth of every due."), Loc.T("It audits you: a twentieth more on every due.")),
        Faction.Wardens => (Loc.T("Hollis warns you before a search, and returns pay a third more."),
                            Loc.T("They search your back room every Guild night: what's stolen is taken, and you're fined.")),
        Faction.Lockless => (Loc.T("Nim's friends pay half what a back-room chest cost you, not a quarter."),
                             Loc.T("They rob you on the third night of every week: a tenth of your purse.")),
        _ => (Loc.T("Their people offer their tools a quarter cheaper."), Loc.T("Their people ask half as much again for their tools.")),
    };
}

/// <summary>
/// A choice at the end of a talk: what its button says, what it costs, what it does (returning what the person says to
/// it), and what the careful player reckons it's worth in gold (<see cref="Strategy.Choose"/>).
/// </summary>
public sealed class Choice
{
    public string Label { get; init; }
    public int Cost { get; init; }
    public Func<string> Do { get; init; }

    /// <summary>What it's worth to the careful player, in gold over the rest of the run (the sim's).</summary>
    public float Worth { get; init; }

    /// <summary>The kinder of two choices worth about the same: the careful player's tie-break.</summary>
    public bool Kind { get; init; }

    /// <summary>A plain "go on": nothing to decide.</summary>
    public static Choice On(string label = null) => new() { Label = label ?? Loc.T("Go on") };
}

/// <summary>
/// Someone at the counter (or at the back door after dark, or a letter) with something to say and, maybe, something to
/// ask: a tale's beat (<see cref="Beat"/>), a tool offered (<see cref="Offers"/>), or the night's news.
/// </summary>
public sealed class Talk
{
    /// <summary>Who's talking (a person's key, for their figure and name).</summary>
    public string Who { get; init; }

    /// <summary>The beat or offer it plays ("magpie.notice", "offer:scales").</summary>
    public string Key { get; init; }

    public bool Night { get; init; }

    /// <summary>A letter, not a visit: no one stands at the counter.</summary>
    public bool Letter { get; init; }

    /// <summary>A tool offered (<see cref="Offers"/>): the upgrade's key, so the careful player can weigh it as it would at the day's end.</summary>
    public string Offer { get; init; }

    public List<Line> Lines { get; init; } = new();

    /// <summary>What you can say. None: a single "Go on".</summary>
    public List<Choice> Choices { get; init; } = new();

    /// <summary>What happens whatever you choose (after the choice).</summary>
    public Action Then { get; init; }

    public bool Done { get; set; }

    /// <summary>The choices on offer: its own, or a single "go on".</summary>
    public IReadOnlyList<Choice> Options => Choices.Count > 0 ? Choices : new[] { Choice.On(Night ? Loc.T("Good night") : null) };
}

/// <summary>A notice on the board (the notebook's Town page): a wanted chest, a witness's word, the Guild's or the Crown's.</summary>
public sealed class Notice
{
    public string Key { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public int Day { get; set; }
}

/// <summary>Someone's word that they'll come by on a day (the calendar shows it): kept or not, it's what they said.</summary>
public sealed class Promise
{
    public int Day { get; set; }
    public string Who { get; set; }
    public string Note { get; set; }
}

/// <summary>A chest in the back room that won't open for you: bought from someone it wasn't theirs to sell, or kept for someone.</summary>
public sealed class Held
{
    /// <summary>Whose it is.</summary>
    public string Owner { get; set; }

    /// <summary>What you paid for it (nothing, for one you're keeping for someone).</summary>
    public int Paid { get; set; }

    public Rarity Colour { get; set; }

    /// <summary>A tale's chest ("reliquary", "parcel", "vell"), or null for an ordinary stolen one.</summary>
    public string Key { get; set; }

    public int Day { get; set; }

    /// <summary>It's someone's to collect (a smuggler's parcel): it can't be returned or sold.</summary>
    public bool Kept => Key == "parcel";
}

/// <summary>
/// One beat of a tale: someone comes to the counter (or the back door) and something happens. It comes once the beat
/// it follows is done and <see cref="Gap"/> days have passed, at a shop of <see cref="Tier"/> or grander, if
/// <see cref="If"/> holds. A talk beat says <see cref="Says"/> and asks <see cref="Asks"/>; a trading beat brings a chest
/// (<see cref="Brings"/>) and what you do with it is the choice (<see cref="Bought"/>, <see cref="Passed"/>).
/// </summary>
public sealed class Beat
{
    public string Key { get; init; }
    public string Tale { get; internal set; }

    /// <summary>Who comes (a person's key).</summary>
    public string Who { get; init; }

    public bool Night { get; init; }

    /// <summary>A letter at night, not a knock.</summary>
    public bool Letter { get; init; }

    /// <summary>The beat it follows (null: the tale's start).</summary>
    public string After { get; init; }

    /// <summary>Days after the beat it follows (or, for a tale's first, not before this day of the run).</summary>
    public int Gap { get; init; } = 1;

    public int Tier { get; init; } = 1;

    public Func<GameState, bool> If { get; init; }

    public Func<GameState, List<Line>> Says { get; init; }

    public Func<GameState, List<Choice>> Asks { get; init; }

    /// <summary>When it's played, whatever was chosen (before the choice's own doing).</summary>
    public Action<GameState> Then { get; init; }

    /// <summary>A trading beat's visit: the chest they bring and what they say.</summary>
    public Func<GameState, Random, Visit> Brings { get; init; }

    public Action<GameState, Visit> Bought { get; init; }

    /// <summary>Passed on, walked out on, or the Wardens sent for.</summary>
    public Action<GameState, Visit> Passed { get; init; }

    /// <summary>What the calendar says when they're expected ("Tobin, back from the Barrow"), if they said they'd come.</summary>
    public string Expect { get; init; }

    public bool Trades => Brings != null;
}

/// <summary>Which part of the realm a tale starts in: every season draws some of each (<see cref="TaleBook.Draw"/>).</summary>
public enum TalePlace { Always, Brindle, Thornwick, Crownhold, Emporium }

public sealed class TaleDef
{
    public string Key { get; init; }
    public string Title { get; init; }

    /// <summary>What it's about, in a line, for the Town page.</summary>
    public string Blurb { get; init; }

    public TalePlace Place { get; init; }

    /// <summary>What the tale can hold secret, one drawn per run (who the Magpie is; what's behind the door).</summary>
    public string[] Secrets { get; init; } = Array.Empty<string>();

    Beat[] _beats = Array.Empty<Beat>();

    public Beat[] Beats
    {
        get => _beats;
        init
        {
            _beats = value;
            foreach (var beat in _beats) beat.Tale = Key;
        }
    }
}

/// <summary>
/// The tales a season holds (docs/stories.md): which were drawn, which beats are done and when, what was chosen
/// (<see cref="Flags"/>), who's gone for good, the notices on the board, the back room, and where you stand with each
/// power. Every beat is played through <see cref="Choose"/> (a talk) or the counter (a chest); the sim plays the same.
/// </summary>
public sealed partial class GameState
{
    /// <summary>
    /// For the sim's "no curios" diagnostic (ordinary progression, no build): a curio a tale would give comes as its worth
    /// in gold instead. Always true in the game.
    /// </summary>
    public bool TakesCurios { get; set; } = true;

    /// <summary>At most this many beats come to the counter a day (a talk comes on top of the day's customers).</summary>
    public const int BeatsPerDay = 2;

    /// <summary>A tool offered at the counter and turned down comes back after this many days.</summary>
    public const int OfferAgain = 3;

    // ---- Standing ----

    readonly Dictionary<Faction, int> _standing = new();

    public int Standing(Faction faction) => _standing.GetValueOrDefault(faction);

    public bool Friend(Faction faction) => Standing(faction) >= Factions.Strong;

    public bool Foe(Faction faction) => Standing(faction) <= -Factions.Strong;

    /// <summary>Accounts changed since the last time anyone looked (the talk and the day's end say so).</summary>
    public List<(Faction Faction, int By)> Swayed { get; } = new();

    public void Sway(Faction faction, int by)
    {
        int was = Standing(faction), now = Math.Clamp(was + by, Factions.Min, Factions.Max);
        if (now == was) return;
        _standing[faction] = now;
        Swayed.Add((faction, now - was));
    }

    /// <summary>A person's people at +2 offer their tools a quarter cheaper; at -2, half as much again.</summary>
    public float StandingPrice(Faction? faction) => faction is not Faction f ? 1f : Friend(f) ? 0.75f : Foe(f) ? 1.5f : 1f;

    // ---- Tales ----

    /// <summary>The tales this season holds (drawn from the seed on the first morning).</summary>
    public List<string> TalesDrawn { get; } = new();

    readonly Dictionary<string, int> _beatsDone = new();

    /// <summary>The last day Jory carries chests for you (one more customer a day till then), or 0.</summary>
    public int PorterUntil { get; set; }

    /// <summary>What's been chosen and what's come of it: "pip.apprentice", "magpie.named".</summary>
    public HashSet<string> Flags { get; } = new();

    /// <summary>People who'll never come to the counter again this season: jailed, taken, gone north.</summary>
    public HashSet<string> Gone { get; } = new();

    /// <summary>What's happened in each tale, a line a beat, for the Town page.</summary>
    public Dictionary<string, List<string>> TaleLog { get; } = new();

    public List<Notice> Notices { get; } = new();

    public List<Held> BackRoom { get; } = new();

    /// <summary>Who said they'd come back, and when (the calendar's): a tale's own word, kept until the day comes or it's played.</summary>
    public List<Promise> Promises { get; } = new();

    /// <summary>Someone says they'll be back on a day: the calendar marks it.</summary>
    public void Promise(int day, string who, string note)
    {
        Promises.RemoveAll(p => p.Who == who);
        Promises.Add(new Promise { Day = day, Who = who, Note = note });
    }

    /// <summary>They came (or won't): the calendar's mark goes.</summary>
    public void Kept(string who) => Promises.RemoveAll(p => p.Who == who);

    /// <summary>What a thing a tale set aside would have sold for (Rafe Kettle's sword), for the choice about it.</summary>
    public Dictionary<string, int> Keepsakes { get; } = new();

    readonly Dictionary<string, int> _offerDeclined = new();

    public bool Did(string beat) => _beatsDone.ContainsKey(beat);

    public int DoneOn(string beat) => _beatsDone.GetValueOrDefault(beat);

    /// <summary>Whether something's been chosen or has come of it ("pip.apprentice").</summary>
    public bool Flagged(string flag) => Flags.Contains(flag);

    public bool Drew(string tale) => TalesDrawn.Contains(tale);

    /// <summary>A tale's secret this season (who the Magpie is), from the seed: it never changes.</summary>
    public string Secret(string tale) => TaleBook.Secret(Seed, tale);

    public void Log(string tale, string line)
    {
        if (!TaleLog.TryGetValue(tale, out var log)) TaleLog[tale] = log = new List<string>();
        log.Add(line);
    }

    public void Post(string key, string title, string text)
    {
        Notices.RemoveAll(n => n.Key == key);
        Notices.Add(new Notice { Key = key, Title = title, Text = text, Day = Day });
    }

    public void TakeDown(string key) => Notices.RemoveAll(n => n.Key == key);

    /// <summary>A person won't come again this season.</summary>
    public void Goes(string person) => Gone.Add(person);

    /// <summary>Gold from (or for) a tale: a reward, a fee, a fine.</summary>
    public void TaleGold(int amount)
    {
        Gold = Math.Max(0, Gold + amount);
        Today.Tales += amount;
    }

    /// <summary>A sum in "purses": rewards and prices scale with Bram's working purse, so a tale pays as much anywhere.</summary>
    public int Purses(float purses) => Money.Round(Math.Max(1, WorkingPurse) * purses);

    void DrawTales()
    {
        if (TalesDrawn.Count > 0 || Day > 1 && !TaleBook.Enabled) return;
        TalesDrawn.AddRange(TaleBook.Draw(Seed));
    }

    /// <summary>Whether a beat could come now: its tale's drawn, it isn't done, what it follows is done long enough ago, and its conditions hold.</summary>
    bool Ready(Beat beat)
    {
        if (!Drew(beat.Tale) || Did(beat.Key) || Tier < beat.Tier || Gone.Contains(beat.Who)) return false;
        int since = beat.After == null ? 0 : Did(beat.After) ? DoneOn(beat.After) : -1;
        if (since < 0 || Day < since + beat.Gap) return false;
        return beat.If == null || beat.If(this);
    }

    /// <summary>
    /// The beats that could come today (or tonight), one per tale: the one that's waited longest first (so no tale starves
    /// another), and among those, the season's own order.
    /// </summary>
    public IEnumerable<Beat> ReadyBeats(bool night) =>
        TaleBook.All.Where(t => Drew(t.Key)).Select(t => t.Beats.FirstOrDefault(b => b.Night == night && Ready(b))).Where(b => b != null)
            .OrderBy(b => ReadySince(b)).ThenBy(b => (unchecked(Seed * 31 + Day * 17 + b.Key.Length * 7) & 0xff));

    /// <summary>The day a beat could first have come: the day what it follows was done, and its gap.</summary>
    int ReadySince(Beat beat) => (beat.After == null ? 0 : DoneOn(beat.After)) + beat.Gap;

    /// <summary>
    /// Today's beats into the queue: a talk at a random place among the day's customers, a trading beat instead of one of
    /// them. None on the first day (or the scripted first two), one on a quiet day at a new shop.
    /// </summary>
    void StageTales()
    {
        if (Day <= 1 || (FirstDayScripted && Day <= 2)) return;
        var rng = new Random(unchecked(Seed * 7411 + Day));
        int room = QuietOpening ? 1 : BeatsPerDay;
        foreach (var beat in ReadyBeats(night: false).Take(room).ToList())
            Stage(beat, rng);
        // Bram's kin's apprentice brings a chest he's fished out, on each week's first morning.
        if (Flagged(TaleBook.Apprentice) && DayOfWeek == 1 && !Gone.Contains(TaleBook.Pip))
            Queue.Insert(0, TaleBook.ApprenticeChest(this, rng));
        StageOffers(rng);
    }

    void Stage(Beat beat, Random rng)
    {
        if (beat.Trades)
        {
            var visit = beat.Brings(this, rng);
            visit.Beat = beat.Key;
            var regulars = Queue.Select((v, i) => (v, i)).Where(p => p.v.Lesson == null && p.v.Talk == null && p.v.Beat == null).ToArray();
            if (regulars.Length > 0) Queue[regulars[rng.Next(regulars.Length)].i] = visit;
            else Queue.Add(visit);
            return;
        }
        Queue.Insert(rng.Next(0, Queue.Count + 1), Visit.Talking(Sellers.Person(beat.Who), MakeTalk(beat)));
    }

    Talk MakeTalk(Beat beat) => new()
    {
        Who = beat.Who, Key = beat.Key, Night = beat.Night, Letter = beat.Letter,
        Lines = beat.Says?.Invoke(this) ?? new List<Line>(),
        Choices = beat.Asks?.Invoke(this) ?? new List<Choice>(),
        Then = beat.Then == null ? null : () => beat.Then(this),
    };

    /// <summary>
    /// What you say: pays the choice's cost, does what it does and the beat's own doing, and marks the beat played.
    /// Returns what the person says to it (or null). A choice you can't afford does nothing.
    /// </summary>
    public string Choose(Talk talk, Choice choice)
    {
        if (talk == null || talk.Done) return null;
        choice ??= talk.Options[0];
        if (Gold < choice.Cost) return null;
        talk.Done = true;
        // Whoever came is someone you've met now: their page is in the notebook.
        foreach (string who in talk.Lines.Select(l => l.Speaker).Append(talk.Who).Where(w => Sellers.Find(w) != null)) Met.Add(who);
        // A tool bought at the counter is the shop's spending, not the town's.
        if (choice.Cost > 0 && talk.Offer != null) Gold -= choice.Cost;
        else if (choice.Cost > 0) TaleGold(-choice.Cost);
        talk.Then?.Invoke();
        string reply = choice.Do?.Invoke();
        if (TaleBook.Find(talk.Key) is Beat beat) _beatsDone[beat.Key] = Day;
        return reply;
    }

    /// <summary>The trading beat at the counter has been bought: it's done, and the tale hears of it.</summary>
    void BeatBought(Visit visit)
    {
        if (TaleBook.Find(visit.Beat) is not Beat beat || Did(beat.Key)) return;
        _beatsDone[beat.Key] = Day;
        beat.Bought?.Invoke(this, visit);
    }

    /// <summary>The trading beat at the counter has gone without a sale.</summary>
    void BeatPassed(Visit visit)
    {
        if (TaleBook.Find(visit.Beat) is not Beat beat || Did(beat.Key)) return;
        _beatsDone[beat.Key] = Day;
        beat.Passed?.Invoke(this, visit);
    }

    /// <summary>
    /// Who's said they'll come back, and when, for the calendar: each drawn tale's next beat that someone promised, at
    /// the day it's due (or tomorrow, if it's overdue).
    /// </summary>
    public IEnumerable<(int Day, string Who, string Note)> Expected()
    {
        foreach (var tale in TaleBook.All.Where(t => Drew(t.Key)))
            foreach (var beat in tale.Beats.Where(b => b.Expect != null && !Did(b.Key) && b.After != null && Did(b.After) && !Gone.Contains(b.Who)))
            {
                if (beat.Tier > Tier || (beat.If != null && !beat.If(this))) continue;
                yield return (Math.Max(Day + 1, DoneOn(beat.After) + beat.Gap), beat.Who, beat.Expect);
            }
        foreach (var promise in Promises.Where(p => p.Day > Day)) yield return (promise.Day, promise.Who, promise.Note);
    }

    // ---- Tools offered at the counter ----

    /// <summary>
    /// A maker comes to the counter with their tool once it's open, a day after the move that opened it at the earliest
    /// (word of a new shop takes a day to get round, and the first day somewhere grander is busy enough).
    /// </summary>
    void StageOffers(Random rng)
    {
        if (Queue.Count(v => v.Talk != null) >= BeatsPerDay || Tier > 1 && Day <= TierSince) return;
        var upgrade = Offers.Due(this).FirstOrDefault();
        if (upgrade == null) return;
        Queue.Insert(rng.Next(0, Queue.Count + 1), Visit.Talking(Sellers.Person(Offers.For(upgrade.Key).Who), Offers.Talk(this, upgrade)));
    }

    /// <summary>A tool turned down today: its maker comes back with it in a few days, once you could pay.</summary>
    public void Declined(string upgrade) => _offerDeclined[upgrade] = Day;

    /// <summary>
    /// Whether a tool's maker would come with it today: the first time whatever's in your purse (so you know it's there,
    /// and what it costs); once turned down, a few days later and only when the morning's purse could pay their price.
    /// </summary>
    public bool OfferDue(string upgrade) => !_offerDeclined.TryGetValue(upgrade, out int day) ||
                                            Day >= day + OfferAgain && Gold >= OfferPrice(Upgrades.Get(upgrade));

    /// <summary>What a tool costs from the person who makes it: as the day's end would charge, and their people's account of you.</summary>
    public int OfferPrice(Upgrade upgrade) => Money.Round(Cost(upgrade) * StandingPrice(Sellers.Find(Offers.For(upgrade.Key)?.Who)?.Of) *
                                                          (upgrade.Key == "bench" && Flagged("abbey.bench") ? 0.5f : 1));

    /// <summary>Buys a tool from the person at the counter (or Nim): owned from now on.</summary>
    public bool BuyOffered(Upgrade upgrade)
    {
        int price = upgrade.By == Story.Nim ? Cost(upgrade) : OfferPrice(upgrade);
        if (Gold < price || Has(upgrade.Key) || !Offers.Open(this, upgrade)) return false;
        Gold -= price;
        _owned.Add(upgrade.Key);
        return true;
    }

    // ---- Stolen chests and the back room ----

    /// <summary>What the Wardens pay for a stolen chest sent home: a fifth of a typical chest here, a third more for a friend.</summary>
    public int FinderShare => Money.Round(Market.TypicalAsk(Tier) * PriceFactor * Wardens.FinderShare * (Friend(Faction.Wardens) ? Wardens.FriendBonus : 1));

    /// <summary>What Nim's friends pay for a chest in the back room: a quarter of what it cost you, half for a friend of the Lockless.</summary>
    public int FencePay(Held held) => held.Paid <= 0 ? 0 : Money.Round(held.Paid * (Friend(Faction.Lockless) ? Wardens.FriendFence : Wardens.Fence));

    /// <summary>The Wardens keep a post at Thornwick and inward: from there you can send for them.</summary>
    public bool CanCallWardens(Visit visit) => Tier >= Wardens.FromTier && visit is { Deciding: true, Talk: null } && visit.Chest.Stolen && visit.Handled;

    /// <summary>
    /// Sends for the Wardens over a stolen chest: the seller runs, the chest goes home, and you get the finder's share.
    /// The Wardens think better of you; the Lockless, worse. Returns what the seller says as they go.
    /// </summary>
    public string CallWardens(Visit visit)
    {
        if (!CanCallWardens(visit)) return null;
        string line = visit.Report(Rng);
        int share = FinderShare;
        Gold += share;
        Today.Tales += share;
        Today.Reported++;
        Sway(Faction.Wardens, 1);
        Sway(Faction.Lockless, -1);
        return line;
    }

    /// <summary>Returns a back-room chest to its owner (the Wardens posted every theft): the finder's share, and the Wardens' thanks.</summary>
    public int ReturnHeld(Held held)
    {
        if (!BackRoom.Contains(held) || held.Kept) return 0;
        BackRoom.Remove(held);
        int pay = FinderShare;
        TaleGold(pay);
        Sway(Faction.Wardens, 1);
        if (held.Key != null) Flags.Add($"returned:{held.Key}");
        return pay;
    }

    /// <summary>Sells a back-room chest to Nim's friends, who ransom such things back: a little gold, and the Lockless' thanks.</summary>
    public int FenceHeld(Held held)
    {
        if (!BackRoom.Contains(held) || held.Kept) return 0;
        BackRoom.Remove(held);
        int pay = FencePay(held);
        TaleGold(pay);
        Sway(Faction.Lockless, 1);
        if (held.Key != null) Flags.Add($"fenced:{held.Key}");
        return pay;
    }

    /// <summary>A bought chest that isn't the seller's to sell: it won't open, so it goes to the back room.</summary>
    void Hold(Visit visit)
    {
        BackRoom.Add(new Held { Owner = visit.Chest.Owner, Paid = visit.PricePaid, Colour = visit.Chest.Quality, Key = visit.Chest.Tale, Day = Day });
        Today.Held++;
    }

    // ---- The night ----

    /// <summary>Tonight's talks, in order: a tale's beat at the back door or a letter, the Wardens' search, the Lockless' visit.</summary>
    public List<Talk> Tonight { get; } = new();

    /// <summary>
    /// After the day's end (and the Guild, on its night): a tale's night beat, if one's ready; the Wardens' search for a
    /// foe of theirs on a Guild night; the Lockless' visit to a foe of theirs on the third night of a week; and Nim's table.
    /// </summary>
    public void StartNight()
    {
        Tonight.Clear();
        if (ReadyBeats(night: true).FirstOrDefault() is Beat beat) Tonight.Add(MakeTalk(beat));
        if (DuesTonight && Foe(Faction.Wardens) && !Friend(Faction.Wardens)) Tonight.Add(Wardens.Search(this));
        if (DayOfWeek == Wardens.RobberyNight && Foe(Faction.Lockless) && Gold > 0) Tonight.Add(Wardens.Robbery(this));
        PeddlerStock.Clear();
        if (PeddlerToday) StockPeddler();
    }

    /// <summary>Nim's table tonight: what's in his pack, priced from what's in your purse now.</summary>
    void StockPeddler()
    {
        PeddlerStock.Clear();
        PeddlerPurse = Gold;
        PeddlerStock.AddRange(Curios.Stock(new Random(unchecked(Seed * 6151 + Day)), this, PeddlerCount));
        PeddlerTier = Tier;
        CuriosSeen.UnionWith(PeddlerStock.Select(c => c.Key));
        People.Add(Story.Nim);
    }

    /// <summary>The tale's people and Bram's kin's standing as saved.</summary>
    void SaveTales(SaveData data)
    {
        data.Tales = TalesDrawn.ToList();
        data.BeatsDone = new Dictionary<string, int>(_beatsDone);
        data.Flags = Flags.ToList();
        data.Gone = Gone.ToList();
        data.Standing = _standing.ToDictionary(p => p.Key.ToString(), p => p.Value);
        data.Notices = Notices.ToList();
        data.BackRoom = BackRoom.ToList();
        data.TaleLog = TaleLog.ToDictionary(p => p.Key, p => p.Value.ToList());
        data.OffersDeclined = new Dictionary<string, int>(_offerDeclined);
        data.Keepsakes = new Dictionary<string, int>(Keepsakes);
        data.Promises = Promises.ToList();
        data.PorterUntil = PorterUntil;
    }

    void LoadTales(SaveData data)
    {
        // A save from before the tales (0.27 and older) starts them now, from the seed.
        if (data.Tales != null) TalesDrawn.AddRange(data.Tales.Where(TaleBook.Exists));
        foreach (var (beat, day) in data.BeatsDone ?? new Dictionary<string, int>()) _beatsDone[beat] = day;
        Flags.UnionWith(data.Flags ?? new List<string>());
        Gone.UnionWith(data.Gone ?? new List<string>());
        foreach (var (key, value) in data.Standing ?? new Dictionary<string, int>())
            if (Enum.TryParse<Faction>(key, out var faction)) _standing[faction] = Math.Clamp(value, Factions.Min, Factions.Max);
        Notices.AddRange(data.Notices ?? new List<Notice>());
        BackRoom.AddRange(data.BackRoom ?? new List<Held>());
        foreach (var (tale, lines) in data.TaleLog ?? new Dictionary<string, List<string>>()) TaleLog[tale] = lines.ToList();
        foreach (var (key, day) in data.OffersDeclined ?? new Dictionary<string, int>()) _offerDeclined[key] = day;
        foreach (var (key, value) in data.Keepsakes ?? new Dictionary<string, int>()) Keepsakes[key] = value;
        Promises.AddRange(data.Promises ?? new List<Promise>());
        PorterUntil = data.PorterUntil;
    }
}

/// <summary>The Wardens of the Road and the Lockless: what they pay, when they search, and when the thieves come.</summary>
public static class Wardens
{
    /// <summary>The Wardens keep a post from Thornwick inward: you can send for them from there.</summary>
    public const int FromTier = 2;

    /// <summary>A finder's share, against a typical chest at the shop; a friend of the Wardens gets a third more.</summary>
    public const float FinderShare = 0.2f, FriendBonus = 4f / 3;

    /// <summary>What Nim's friends pay for a back-room chest, against what it cost: a quarter, or half for a friend.</summary>
    public const float Fence = 0.25f, FriendFence = 0.5f;

    /// <summary>A foe of the Wardens is fined this share of the week's due when they search.</summary>
    public const float Fine = 0.1f;

    /// <summary>A foe of the Lockless is robbed of this share of their purse, on this night of every week.</summary>
    public const float Robbed = 0.1f;

    public const int RobberyNight = 3;

    /// <summary>The Wardens search a foe's back room on a Guild night: everything stolen in it taken, and a fine.</summary>
    public static Talk Search(GameState state)
    {
        int fine = Money.Round(state.DueFor(state.Week) * Fine);
        int taken = state.BackRoom.Count(h => !h.Kept);
        return new Talk
        {
            Who = TaleBook.Hollis, Key = "night:search", Night = true,
            Lines =
            {
                new(TaleBook.Hollis, Loc.T("Wardens. Open up. We've a warrant for your back room, and we'll be back with one every Guild night until " +
                                          "you give us a reason not to.")),
                new(TaleBook.Hollis, taken > 0
                    ? Loc.T($"{Loc.Plural(taken, $"{taken} chest", $"{taken} chests")} that aren't yours, and a fine of {Money.Say(fine)}. Good night, Keywright.")
                    : Loc.T($"Nothing this time. The fine's {Money.Say(fine)} all the same: for our trouble.")),
            },
            Then = () =>
            {
                state.BackRoom.RemoveAll(h => !h.Kept);
                state.TaleGold(-Math.Min(state.Gold, fine));
            },
        };
    }

    /// <summary>The Lockless visit a foe of theirs: a tenth of the purse gone by morning.</summary>
    public static Talk Robbery(GameState state)
    {
        int lost = Money.Round(state.Gold * Robbed);
        return new Talk
        {
            Who = TaleBook.Flick, Key = "night:robbery", Night = true, Letter = true,
            Lines =
            {
                new(Story.Bram, Loc.T($"The back shutter's been forced. {Money.Say(lost)} gone from the strongbox, and a black feather on the counter. " +
                                      $"The Lockless don't forget a Keywright who sets the Wardens on them.")),
            },
            Then = () => state.TaleGold(-Math.Min(state.Gold, lost)),
        };
    }
}
