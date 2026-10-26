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
/// the last is the Guild's night, two have something on in town, and Nim comes on the other two, unless the story has
/// a morning that week (the first), when Nim comes once. Also the kind the Crown's buyer (or the crier's lord) wants.
/// All from the seed.
/// </summary>
public sealed class WeekPlan
{
    public int Week { get; set; }

    /// <summary>Each day's event (index: the day of the week less one), or null.</summary>
    public List<string> Days { get; set; } = new();

    /// <summary>The kind the Crown's buyer pays double for, or the crier's reward is for.</summary>
    public Category Wanted { get; set; }

    /// <summary>The days of the week Nim comes by: two, or one on a week with a story morning.</summary>
    public List<int> Peddlers { get; set; } = new();

    /// <summary>The day the week's story is told (0: none this week).</summary>
    public int StoryDay { get; set; }

    /// <summary>Before 0.21, Nim's one day of the week (a save from then keeps it).</summary>
    public int Peddler { get; set; }

    public string On(int dayOfWeek) => dayOfWeek >= 1 && dayOfWeek <= Days.Count ? Days[dayOfWeek - 1] : null;

    /// <summary>Nim comes by that morning.</summary>
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
/// not how to judge a chest. Nim comes twice a week (once when the story has a morning).
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
        new("market", "Fair Day", "The whole county's in town for the fair: three more customers today.", Gentle: true),
        new("festival", "Locking Day", "Three hundred years since the Locking, and everyone's celebrating: everything sells for a quarter more today.", Gentle: true),
        new("buyer", "The Crown's Buyer", "The Crown's buyer is in town, paying double for {kind} today.", Gentle: true),
        new("gilded", "A Royal Chest", "Someone's bringing in one of King Aldric's own chests, gilded, with the royal seal. They think the gold is paint."),
        new("stranger", "The Stranger", "A stranger in black is coming by with a chest wrapped in iron chains. Nothing in it can rattle."),
        new("storm", "A Storm", "Few will brave the rain, but those who do bring their best: chests come in a colour better.", Gentle: true),
        new("wreck", "A Barge Aground", "A treasure barge ran aground on the Aldwater: today's lots come in twos and threes.", MinTier: 3),
        new("estate", "A Foreclosure Sale", "The Guild has foreclosed on a manor, and its chests are being sold off quick: everyone asks a fifth less today.", Gentle: true),
        new("tourney", "The Tourney", "Knights from four counties are in town for the tourney: weapons and cloaks sell for half again as much today.", Gentle: true),
        new("mourning", "A Day of Mourning", "The old Duke is being buried, and the whole town's in black. Nobody will stand for a hard shake today."),
        new("crier", "The Town Crier", "Old Lord Harrow lost his family's {kind} to the Locking. His crier offers a reward for one today: three times its price."),
        new("minstrel", "The Minstrels' Fair", "Minstrels are trading songs in the square, and anyone with a box that hums has brought it: a Heartsong is three times as likely today."),
        new("dreaming", "The Sleepers' Moon", "The Old Wood's moon was full, and all who slept under it dreamt true, like Mother Bramble: every seller tells you their dream. Some warded something.", MinTier: 2),
        new("wardens", "Wardens' Day", "The old custom: ward a keepsake into a chest before you sell it, and leave a riddle for it. Name the answer, and the ward gives it up.", MinTier: 2),
        new("census", "The Guild's Count", "The Gilded Guild is counting everything that changes hands: a clerk tallies every article in every chest today, false bottoms and all.", MinTier: 2),
        new("assessor", "The Guild's Assessor", "A Guild assessor sits in the corner all day, and takes a tenth of every sale for the Guild.", MinTier: 2),
        new("moon", "A Witchlight Moon", "Under a witchlight moon the old enchantments wake: things in today's chests are far likelier to be enchanted.", MinTier: 3),
    };

    public static EventDef Get(string key) => All.First(e => e.Key == key);

    public static EventDef Find(string key) => All.FirstOrDefault(e => e.Key == key);

    static string Kind(Category kind) => Categories.Get(kind).Name.ToLowerInvariant();

    /// <summary>The event in a few words (23 letters at most: the top bar's chip), for the top bar and the calendar.</summary>
    public static string Tag(EventDef e, Category kind) => e.Key switch
    {
        "market" => "Fair Day: +3 customers",
        "festival" => "Locking Day: sales +25%",
        "buyer" => $"Crown buys {Kind(kind)} x2",
        "gilded" => "A royal chest today",
        "stranger" => "The Stranger's in town",
        "storm" => "Storm: better chests",
        "wreck" => "Barge aground: pairs",
        "estate" => "Foreclosure sale: -20%",
        "tourney" => "Tourney: arms, cloaks",
        "mourning" => "Mourning: no hard shake",
        "crier" => $"Crier wants {Kind(kind)}",
        "minstrel" => "Minstrels: songs x3",
        "dreaming" => "Sleepers' Moon: dreams",
        "wardens" => "Wardens' Day: riddles",
        "census" => "The Guild's Count",
        "assessor" => "Assessor: sales -10%",
        "moon" => "Witchlight: more magic",
        _ => e.Name,
    };

    /// <summary>The event's blurb, with the buyer's (or the crier's) kind filled in.</summary>
    public static string Describe(EventDef e, Category kind) => e.Blurb.Replace("{kind}", Kind(kind));

    /// <summary>
    /// The plan for a week. The fifth day is the Guild's. A week with a story morning (<see cref="Story.TellsOn"/>) has
    /// it on the first day and Nim on one of the other three; any other week has Nim on two mornings that aren't next
    /// to each other. The days left have something on in town (gentle ones in the first week), that haven't come lately
    /// (they all come round before any comes twice). The first week is fixed: the welcome, a gentle day, Nim's first
    /// visit, another gentle day, the first due.
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
        if (week == 1) plan.Peddlers.Add(FirstPeddlerDay);
        else if (plan.StoryDay > 0) plan.Peddlers.Add(rng.PickOne(open.ToArray()));
        else plan.Peddlers.AddRange(rng.PickOne(Apart));
        foreach (int day in plan.Peddlers) open.Remove(day);
        foreach (int day in open.Take(PerWeek))
            plan.Days[day - 1] = Pick(rng, tier, seen, plan.Days, gentle: week == 1, fair).Key;
        return plan;
    }

    /// <summary>Two of the first four mornings that aren't next to each other: Nim's, in a week without a story.</summary>
    static readonly int[][] Apart = { new[] { 1, 3 }, new[] { 1, 4 }, new[] { 2, 4 } };

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
        if (visit.Lesson != null || visit.Says != Clue.None || visit.Chest.Sealed || visit.Seller.SlipPrice) return visit;
        var says = eventKey switch { "dreaming" => Clue.Dream, "wardens" => Clue.Riddle, "census" => Clue.Tally, _ => Clue.None };
        if (says == Clue.None) return visit;
        if (ward || says == Clue.Riddle || (says == Clue.Dream && rng.NextFloat() < DreamWards)) Market.WardOne(rng, visit.Seller, visit.Chest, tier);
        // A riddle is only left for a warded thing: a chest with every kind in it already has nothing to ward.
        if (says == Clue.Riddle && visit.Chest.Warded == null) return visit;
        var clue = Market.MakeClue(rng, visit.Seller, visit.Chest, says);
        return new Visit(visit.Seller, visit.Chest, visit.Noticed, visit.Trade, visit.Ask, visit.Floor, visit.Pitch)
        {
            Origin = visit.Origin, Fee = visit.Fee, Clue = clue.Note, ClueKinds = clue.Kinds, Says = says, HearsOut = visit.HearsOut, Unruffled = visit.Unruffled,
        };
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
        Key = "stranger", Name = "The Stranger", Tier = 1, Judge = Judgment.Clueless,
        Kinds = new[] { 1f, 1, 1, 1, 1, 1 }, Colours = new[] { 1f, 0, 0, 0 },
        Habit = "A stranger in black who sells chests wrapped tight in iron chains, and never says where they're from. Nothing inside " +
                "can move, so nothing rattles: all you can learn is how heavy it is, and heavy ones hold more.",
        Pitches = new[] { "Chained tight. No rattles, and no questions. {price}.", "Where's it from? Somewhere that wanted it kept shut. {price}.",
                          "Some chests are better left shut. Not this one. {price}." },
        Accepts = new[] { "Wise.", "We'll meet again." },
        Leaves = new[] { "Another time." },
        Passes = new[] { "As you like.", "Someone else, then." },
        Startled = new[] { "Careful.", "It won't open for that." },
        Fussed = new[] { "Careful. That will cost you." },
    };

    public const float StrangerJunk = 0.45f, StrangerTreasure = 0.2f;

    /// <summary>The Stranger's sealed chest, priced at what an ordinary chest at this shop holds, at what sellers ask here.</summary>
    /// <param name="prices">What sellers ask of this shop against anyone (<see cref="GameState.PriceFactor"/>).</param>
    public static Visit StrangerVisit(Random rng, int tier, float prices = 1)
    {
        var colour = (Rarity)Math.Clamp(tier - 1, 0, 3);
        var kinds = Categories.UpTo(tier).Select(c => c.Category).ToArray();
        float roll = rng.NextFloat();
        List<Item> items;
        if (roll < StrangerJunk)
        {
            // Junk: a light thing or two, the worst of them.
            int n = rng.Next(1, 3);
            items = Enumerable.Range(0, n).Select(_ => new Item(rng.PickWeighted(kinds, k => 1 / Categories.Heft(k)), Rarity.Common)).ToList();
        }
        else if (roll < 1 - StrangerTreasure)
        {
            int n = rng.Next(2, 4);
            items = Enumerable.Range(0, n).Select(_ => new Item(rng.PickOne(kinds), Categories.RollQuality(rng, colour))).ToList();
        }
        else
        {
            // A fortune: heavy things, two colours better than the shop's, one of them enchanted.
            int n = rng.Next(3, 5);
            var best = (Rarity)Math.Min(4, (int)colour + 2);
            items = Enumerable.Range(0, n).Select(i => new Item(rng.PickWeighted(kinds, Categories.Heft), Categories.RollQuality(rng, best),
                Magic: i == 0)).ToList();
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
