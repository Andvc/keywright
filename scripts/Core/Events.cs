using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <param name="MinTier">Only from this shop tier, where it makes a difference.</param>
/// <param name="Gentle">Can come in the first week with anything on, while the stall's still new: it changes the day, not how to judge a chest.</param>
public sealed record EventDef(string Key, string Name, string Blurb, int MinTier = 1, bool Gentle = false);

/// <summary>What a day of the week is for: the week's story, something on in town, Nim's curios, or the Guild's night.</summary>
public enum DayRole { None, Story, Event, Curio, Tax }

/// <summary>
/// A week's plan, made a week ahead so the ledger's calendar can show it. Every one of the five days has its business:
/// the last is the Guild's night, two have something on in town, and a story week's first is the story's (Nim comes every
/// night since 0.28). Also the kind the Crown's buyer (or the crier's lord) wants.
/// All from the seed.
/// </summary>
public sealed class WeekPlan
{
    public int Week { get; set; }

    /// <summary>Each day's event (index: the day of the week less one), or null.</summary>
    public List<string> Days { get; set; } = new();

    /// <summary>The kind the Crown's buyer pays double for, or the crier's reward is for.</summary>
    public Category Wanted { get; set; }

    /// <summary>The days of the week Nim came by in the morning, in a plan from before 0.28 (he comes every night now).</summary>
    public List<int> Peddlers { get; set; } = new();

    /// <summary>The day the week's story is told (0: none this week).</summary>
    public int StoryDay { get; set; }

    /// <summary>Before 0.21, Nim's one day of the week (a save from then keeps it).</summary>
    public int Peddler { get; set; }

    public string On(int dayOfWeek) => dayOfWeek >= 1 && dayOfWeek <= Days.Count ? Days[dayOfWeek - 1] : null;

    /// <summary>Nim came by that morning (a plan from before 0.28).</summary>
    public bool NimOn(int dayOfWeek) => dayOfWeek > 0 && (Peddlers.Count > 0 ? Peddlers.Contains(dayOfWeek) : Peddler == dayOfWeek);

    /// <summary>What a day of the week is for.</summary>
    public DayRole Role(int dayOfWeek) => dayOfWeek == Guild.DaysPerWeek ? DayRole.Tax : StoryDay == dayOfWeek ? DayRole.Story
        : NimOn(dayOfWeek) ? DayRole.Curio : On(dayOfWeek) != null ? DayRole.Event : DayRole.None;
}

/// <summary>
/// Two days a week something happens in town, and each changes what a good day looks like: more customers or fewer,
/// better prices or worse, a stranger with a chained chest, a royal chest, a rule everyone keeps that day. Some are
/// puzzles: on the Sleepers' Moon every seller dreams true, on Wardens' Day every chest has a warded thing and a riddle
/// for it, on the Guild's Count a clerk tallies every article. The first week's are gentle ones: they change the day,
/// not how to judge a chest. Nim comes every night.
/// </summary>
public static class Events
{
    /// <summary>Days with something on, every week (the first week's are gentle: <see cref="EventDef.Gentle"/>).</summary>
    public const int PerWeek = 2;

    /// <summary>Nim's first visit: the first week's third morning, after the welcome and a gentle day in town.</summary>
    public const int FirstPeddlerDay = 3;

    public const int MarketCustomers = 3, StormCustomers = -2;
    public const float FestivalSales = 1.25f, BuyerSales = 2f, WreckPairs = 0.35f, TourneySales = 1.5f, AssessorSales = 0.9f,
        EstatePrices = 0.8f, MoonMagic = 0.3f, MinstrelSongs = 3f, DreamWards = 0.6f, CrierReward = 3f;

    public const string Crier = "crier";

    public static readonly EventDef[] All =
    {
        new("market", Loc.T("Fair Day"), Loc.T("The whole county's in town for the fair: three more customers today."), Gentle: true),
        new("festival", Loc.T("Locking Day"), Loc.T("Three hundred years since the Locking, and everyone's celebrating: everything sells for a quarter more today."), Gentle: true),
        new("buyer", Loc.T("The Crown's Buyer"), Loc.T("The Crown's buyer is in town, paying double for {kind} today."), Gentle: true),
        new("gilded", Loc.T("A Royal Chest"), Loc.T("Someone's bringing in one of King Aldric's own chests, gilded, with the royal seal. They think the gold is paint.")),
        new("stranger", Loc.T("The Stranger"), Loc.T("A stranger in black is coming by with a chest wrapped in iron chains. Nothing in it can rattle.")),
        new("storm", Loc.T("A Storm"), Loc.T("Few will brave the rain, but those who do bring their best: chests come in a colour better."), Gentle: true),
        new("wreck", Loc.T("A Barge Aground"), Loc.T("A treasure barge ran aground on the Aldwater: today's lots come in twos and threes."), MinTier: 3),
        new("estate", Loc.T("A Foreclosure Sale"), Loc.T("The Guild has foreclosed on a manor, and its chests are being sold off quick: everyone asks a fifth less today."), Gentle: true),
        new("tourney", Loc.T("The Tourney"), Loc.T("Knights from four counties are in town for the tourney: weapons and cloaks sell for half again as much today."), Gentle: true),
        new("mourning", Loc.T("A Day of Mourning"), Loc.T("The old Duke is being buried, and the whole town's in black. Nobody will stand for a hard shake today.")),
        new("crier", Loc.T("The Town Crier"), Loc.T("Old Lord Harrow lost his family's {kind} to the Locking. His crier offers a reward for one today: three times its price.")),
        new("minstrel", Loc.T("The Minstrels' Fair"), Loc.T("Minstrels are trading songs in the square, and anyone with a box that hums has brought it: a Heartsong is three times as likely today.")),
        new("dreaming", Loc.T("The Sleepers' Moon"), Loc.T("The Old Wood's moon was full, and all who slept under it dreamt true, like Mother Sorrel: every seller tells you their dream. Some warded something."), MinTier: 2),
        new("wardens", Loc.T("Wardens' Day"), Loc.T("The old custom: ward a keepsake into a chest before you sell it, and leave a riddle for it. Name the answer, and the ward gives it up."), MinTier: 2),
        new("census", Loc.T("The Guild's Count"), Loc.T("The Gilded Guild is counting everything that changes hands: a clerk tallies every article in every chest today, false bottoms and all."), MinTier: 2),
        new("assessor", Loc.T("The Guild's Assessor"), Loc.T("A Guild assessor sits in the corner all day, and takes a tenth of every sale for the Guild."), MinTier: 2),
        new("moon", Loc.T("A Witchlight Moon"), Loc.T("Under a witchlight moon the old enchantments wake: things in today's chests are far likelier to be enchanted."), MinTier: 3),
    };

    public static EventDef Get(string key) => All.First(e => e.Key == key);

    public static EventDef Find(string key) => All.FirstOrDefault(e => e.Key == key);

    static string Kind(Category kind) => Categories.Get(kind).Name.ToLowerInvariant();

    /// <summary>The event in a few words (23 letters at most: the top bar's chip), for the top bar and the calendar.</summary>
    public static string Tag(EventDef e, Category kind) => e.Key switch
    {
        "market" => Loc.T("Fair Day: +3 customers"),
        "festival" => Loc.T("Locking Day: sales +25%"),
        "buyer" => Loc.T($"Crown buys {Kind(kind)} x2"),
        "gilded" => Loc.T("A royal chest today"),
        "stranger" => Loc.T("The Stranger's in town"),
        "storm" => Loc.T("Storm: better chests"),
        "wreck" => Loc.T("Barge aground: pairs"),
        "estate" => Loc.T("Foreclosure sale: -20%"),
        "tourney" => Loc.T("Tourney: arms, cloaks"),
        "mourning" => Loc.T("Mourning: no hard shake"),
        "crier" => Loc.T($"Crier wants {Kind(kind)}"),
        "minstrel" => Loc.T("Minstrels: songs x3"),
        "dreaming" => Loc.T("Sleepers' Moon: dreams"),
        "wardens" => Loc.T("Wardens' Day: riddles"),
        "census" => Loc.T("The Guild's Count"),
        "assessor" => Loc.T("Assessor: sales -10%"),
        "moon" => Loc.T("Witchlight: more magic"),
        _ => e.Name,
    };

    /// <summary>The event's blurb, with the buyer's (or the crier's) kind filled in.</summary>
    public static string Describe(EventDef e, Category kind) => e.Blurb.Replace("{kind}", Kind(kind));

    /// <summary>
    /// The plan for a week. The fifth day is the Guild's. A week with a story morning (<see cref="Story.TellsOn"/>) has
    /// it on the first day. Two of the days left have something on in town (gentle ones in the first week), that haven't
    /// come lately (they all come round before any comes twice). Nim comes every night (0.28), so no day is his.
    /// </summary>
    /// <param name="fair">Fair days (a challenge's boon): no days of mourning, and no Guild assessors.</param>
    public static WeekPlan Plan(int seed, int week, int tier, ICollection<string> seen, bool fair = false)
    {
        var rng = new Random(unchecked(seed * 104729 + week * 7907));
        var plan = new WeekPlan { Week = week, Days = Enumerable.Repeat<string>(null, Guild.DaysPerWeek).ToList() };
        plan.Wanted = rng.PickOne(Categories.UpTo(tier).Select(c => c.Category).ToArray());
        var open = Enumerable.Range(1, Guild.DaysPerWeek - 1).ToList();
        if (Story.TellsOn(week))
        {
            plan.StoryDay = 1;
            open.Remove(1);
        }
        // (Nim comes every night since 0.28, so no morning is his: the days that were are free for the town.)
        open = open.OrderBy(_ => rng.Next()).ToList();
        foreach (int day in open.Take(PerWeek).OrderBy(d => d))
            plan.Days[day - 1] = Pick(rng, tier, seen, plan.Days, gentle: week == 1, fair).Key;
        return plan;
    }


    /// <summary>The days a fair season never brings.</summary>
    static readonly string[] Unfair = { "mourning", "assessor" };

    static EventDef Pick(Random rng, int tier, ICollection<string> seen, IList<string> week, bool gentle = false, bool fair = false)
    {
        bool Fits(EventDef e) => e.MinTier <= tier && (!gentle || e.Gentle) && !week.Contains(e.Key) && !(fair && Unfair.Contains(e.Key));
        var fresh = All.Where(e => Fits(e) && !seen.Contains(e.Key)).ToArray();
        if (fresh.Length == 0)
        {
            foreach (var e in All.Where(Fits)) seen.Remove(e.Key);
            fresh = All.Where(Fits).ToArray();
        }
        var pick = rng.PickOne(fresh);
        seen.Add(pick.Key);
        return pick;
    }

    /// <summary>
    /// A day's puzzle, told to every ordinary seller (not a lesson's, nor anyone with a clue of their own, nor the King,
    /// whose price is your slip): on the Sleepers' Moon a dream (of what they warded, or else the dearest thing), on
    /// Wardens' Day a riddle for the thing they warded, on the Guild's Count the clerk's tally.
    /// </summary>
    /// <param name="ward">They've warded something, whatever the day (a sleeper's pillow's dreams).</param>
    public static Visit Restate(Random rng, Visit visit, string eventKey, int tier, bool ward = false)
    {
        if (visit.Lesson != null || visit.Talk != null || visit.Says != Clue.None || visit.Chest.Sealed || visit.Seller.SlipPrice) return visit;
        var says = eventKey switch { "dreaming" => Clue.Dream, "wardens" => Clue.Riddle, "census" => Clue.Tally, _ => Clue.None };
        if (says == Clue.None) return visit;
        if (ward || says == Clue.Riddle || (says == Clue.Dream && rng.NextFloat() < DreamWards)) Market.WardOne(rng, visit.Seller, visit.Chest, tier);
        // A riddle is only left for a warded thing: a chest with every kind in it already has nothing to ward.
        if (says == Clue.Riddle && visit.Chest.Warded == null) return visit;
        // The same visit, with what they now tell you (0.35: a new visit lost a friend's waived terms, a tale's beat, a gift).
        var clue = Market.MakeClue(rng, visit.Seller, visit.Chest, says);
        visit.Clue = clue.Note;
        visit.ClueKinds = clue.Kinds;
        visit.Says = says;
        return visit;
    }

    /// <summary>The crier's reward: one thing of the wanted kind, as good as the regulars ask for, today only.</summary>
    public static Commission Reward(Category kind, int tier, int day) => new()
    {
        Patron = Crier, Kind = CommissionKind.Piece, Category = kind, Quality = Patrons.Asked(tier), Lapses = day,
        Reward = Money.Round(CrierReward * Categories.Nominal(kind, Patrons.Asked(tier))),
    };

    /// <summary>
    /// The Stranger: never says where it's from, never lets you open it first. It's wrapped tight in iron chains, so
    /// nothing inside can move; all you can tell is how heavy it is. Mostly junk, sometimes a fair lot, now and then a fortune.
    /// </summary>
    public static readonly SellerDef Stranger = new()
    {
        Key = "stranger", Name = Loc.T("The Stranger"), Short = Loc.T("Stranger"), Tier = 1, Judge = Judgment.Clueless,
        Kinds = new[] { 1f, 1, 1, 1, 1, 1 }, Colours = new[] { 1f, 0, 0, 0 },
        Habit = Loc.T("A stranger in black who sells chests wrapped tight in iron chains, and never says where they're from. Nothing inside " +
                "can move, so nothing rattles: all you can learn is how heavy it is, and heavy ones hold more."),
        Pitches = new[] { Loc.T("Chained tight. No rattles, and no questions. {price}."), Loc.T("Where's it from? Somewhere that wanted it kept shut. {price}."),
                          Loc.T("Some chests are better left shut. Not this one. {price}.") },
        Accepts = new[] { Loc.T("Wise."), Loc.T("We'll meet again.") },
        Leaves = new[] { Loc.T("Another time.") },
        Passes = new[] { Loc.T("As you like."), Loc.T("Someone else, then.") },
        Startled = new[] { Loc.T("Careful."), Loc.T("It won't open for that.") },
        Fussed = new[] { Loc.T("Careful. That will cost you.") },
    };

    public const float StrangerJunk = 0.45f, StrangerTreasure = 0.2f;

    /// <summary>The Stranger's sealed chest, priced at what an ordinary chest at this shop holds, at what sellers ask here.</summary>
    /// <param name="prices">What sellers ask of this shop against anyone (<see cref="GameState.PriceFactor"/>).</param>
    /// <param name="ways">The trade's ways (<see cref="GameState.Ways"/>): two of a kind only where the slip counts them.</param>
    public static Visit StrangerVisit(Random rng, int tier, float prices = 1, int ways = 0)
    {
        bool pairs = Math.Max(tier, ways) >= Market.PairsTier;
        // Each thing a kind of its own, unless the trade counts two of a kind here.
        List<Category> Pick(int n, Func<Category, float> weight)
        {
            var picked = new List<Category>();
            while (picked.Count < n)
            {
                var free = pairs ? Categories.UpTo(tier).Select(c => c.Category).ToArray() : Categories.UpTo(tier).Select(c => c.Category).Except(picked).ToArray();
                if (free.Length == 0) break;
                picked.Add(rng.PickWeighted(free, weight));
            }
            return picked;
        }
        var colour = (Rarity)Math.Clamp(tier - 1, 0, 3);
        float roll = rng.NextFloat();
        List<Item> items;
        if (roll < StrangerJunk)
        {
            // Junk: a light thing or two, the worst of them.
            int n = rng.Next(1, 3);
            items = Pick(n, k => 1 / Categories.Heft(k)).Select(k => new Item(k, Rarity.Common)).ToList();
        }
        else if (roll < 1 - StrangerTreasure)
        {
            int n = rng.Next(2, 4);
            items = Pick(n, _ => 1f).Select(k => new Item(k, Categories.RollQuality(rng, colour))).ToList();
        }
        else
        {
            // A fortune: heavy things, two colours better than the shop's, one of them enchanted.
            int n = rng.Next(3, 5);
            var best = (Rarity)Math.Min(4, (int)colour + 2);
            items = Pick(n, Categories.Heft).Select((k, i) => new Item(k, Categories.RollQuality(rng, best), Magic: i == 0)).ToList();
        }
        var chest = new Chest(items, colour, isSealed: true);
        int ask = Money.Round(Market.TypicalChest(Sellers.All.First(s => s.Tier == tier), colour, tier) * 1.1f * prices);
        return Market.Price(rng, Stranger, chest, tier, ask, Trade.Fixed, noticed: Array.Empty<Category>());
    }

    /// <summary>
    /// Gilds a visit: the chest is one of King Aldric's own, a colour better than its seller thinks (gold, if it was
    /// purple), with something enchanted in it. The seller's price stays what it was.
    /// </summary>
    public static Visit Gild(Random rng, Visit visit)
    {
        var old = visit.Chest;
        var colour = (Rarity)Math.Min(4, (int)old.Quality + 1);
        // A Heartsong stays as it is (it's no seller's, and never enchanted); so does what's warded, a colour finer.
        var items = old.Items.Select(i => i.IsSong ? i : i with { Quality = Categories.RollQuality(rng, colour) }).ToList();
        if (items.All(i => i.IsSong)) items.Add(new Item(Category.Coins, Categories.RollQuality(rng, colour)));
        var plain = items.Where(i => !i.IsSong).ToArray();
        var lit = rng.PickOne(plain);
        items[items.IndexOf(lit)] = lit with { Magic = true, Fake = false };
        var chest = new Chest(items, colour, old.Hidden, old.Lock, gilded: true, packed: old.Packed);
        if (old.Warded != null) chest.Ward(old.Warded with { Quality = Categories.RollQuality(rng, colour) });
        return new Visit(visit.Seller, chest, visit.Noticed, visit.Trade, visit.Ask, visit.Floor, visit.Pitch)
        {
            Origin = visit.Origin, Fee = visit.Fee, Clue = visit.Clue, ClueKinds = visit.ClueKinds, Says = visit.Says, HearsOut = visit.HearsOut,
            Unruffled = visit.Unruffled,
        };
    }
}
