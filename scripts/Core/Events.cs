using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <param name="MinTier">Only from this shop tier, where it makes a difference.</param>
/// <param name="Gentle">Can come in the first week with anything on, while the stall's still new: it changes the day, not how to judge a chest.</param>
public sealed record EventDef(string Key, string Name, string Blurb, int MinTier = 1, bool Gentle = false);

/// <summary>
/// A week's plan, made a week ahead so the ledger's calendar can show it: which days something happens in town, the
/// kind the Crown's buyer (or the crier's lord) wants, and the day Nim comes by. All from the seed.
/// </summary>
public sealed class WeekPlan
{
    public int Week { get; set; }

    /// <summary>Each day's event (index: the day of the week less one), or null.</summary>
    public List<string> Days { get; set; } = new();

    /// <summary>The kind the Crown's buyer pays double for, or the crier's reward is for.</summary>
    public Category Wanted { get; set; }

    /// <summary>The day of the week Nim comes by.</summary>
    public int Peddler { get; set; }

    public string On(int dayOfWeek) => dayOfWeek >= 1 && dayOfWeek <= Days.Count ? Days[dayOfWeek - 1] : null;

}

/// <summary>
/// Most weeks something happens in town for a day (from the fourth, two days of every three), and each changes what a good day looks like:
/// more customers or fewer, better prices or worse, a stranger with a chained chest, a royal chest, a rule everyone
/// keeps that day. Some are puzzles: on the Sleepers' Moon every seller dreams true, on Wardens' Day every chest has a
/// warded thing and a riddle for it, on the Guild's Count a clerk tallies every article. Nim comes every week.
/// </summary>
public static class Events
{
    /// <summary>Nothing's on in the first week (it's for learning the stall: the playtest found it crowded enough).</summary>
    public const int FromWeek = 2;

    /// <summary>
    /// How many days of a week have something on: one in the first three weeks, while there's plenty that's new; two from
    /// the fourth, when the shop's crowd is familiar (the experience playtest found the long stays at Crownhold and the
    /// Emporium the dullest stretches of a run).
    /// </summary>
    public static int PerWeek(int week) => week < 4 ? 1 : 2;

    /// <summary>Nim's first visit: the first week's last morning, once the stall's found its feet (and before Vane's first visit).</summary>
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
    /// The plan for a week: Nim's day (the first week's last, else any day but the Guild's), and its events
    /// (<see cref="PerWeek"/>: none in the first week), on other days than Nim's while there are any, that haven't come
    /// lately (they all come round before any comes twice).
    /// </summary>
    public static WeekPlan Plan(int seed, int week, int tier, ICollection<string> seen)
    {
        var rng = new Random(unchecked(seed * 104729 + week * 7907));
        var plan = new WeekPlan { Week = week, Days = Enumerable.Repeat<string>(null, Guild.DaysPerWeek).ToList() };
        // Not two mornings running: after his first visit (the first week's last morning), the second week's second.
        plan.Peddler = week == 1 ? FirstPeddlerDay : week == 2 ? 2 : rng.Next(1, Guild.DaysPerWeek);
        plan.Wanted = rng.PickOne(Categories.UpTo(tier).Select(c => c.Category).ToArray());
        if (week < FromWeek) return plan;
        var days = Enumerable.Range(1, Guild.DaysPerWeek).OrderBy(d => d == plan.Peddler ? 1 : 0).ThenBy(_ => rng.Next()).Take(PerWeek(week));
        // The first week with anything on has a gentle one: it changes the day, not how to judge a chest.
        foreach (int day in days.OrderBy(d => d))
            plan.Days[day - 1] = Pick(rng, tier, seen, plan.Days, gentle: week == FromWeek).Key;
        return plan;
    }

    static EventDef Pick(Random rng, int tier, ICollection<string> seen, IList<string> week, bool gentle = false)
    {
        bool Fits(EventDef e) => e.MinTier <= tier && (!gentle || e.Gentle) && !week.Contains(e.Key);
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
        return new Visit(visit.Seller, visit.Chest, visit.Claim, visit.Trade, visit.Ask, visit.Floor, visit.Pitch)
        {
            Origin = visit.Origin, Fee = visit.Fee, Clue = clue.Note, ClueKinds = clue.Kinds, Says = says, HearsOut = visit.HearsOut,
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
        Habit = "Sells chests wrapped in iron chains: nothing inside can move, so nothing rattles. Heavy ones hold more.",
        Pitches = new[] { "Chained tight. No rattles, no questions. {price}.", "Where from? Somewhere that wanted it kept shut. {price}.",
                          "Some chests are better left shut. Not this one. {price}." },
        Accepts = new[] { "Wise.", "We'll meet again." },
        Leaves = new[] { "Another time." },
    };

    public const float StrangerJunk = 0.45f, StrangerTreasure = 0.2f;

    /// <summary>The Stranger's sealed chest, priced at what an ordinary chest at this shop holds.</summary>
    public static Visit StrangerVisit(Random rng, int tier)
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
        int ask = Money.Round(Market.TypicalChest(Sellers.All.First(s => s.Tier == tier), colour, tier) * 1.1f);
        return Market.Price(rng, Stranger, chest, tier, ask, Trade.Fixed, claim: Array.Empty<Category>());
    }

    /// <summary>
    /// Gilds a visit: the chest is one of King Aldric's own, a colour better than its seller thinks (gold, if it was
    /// purple), with something enchanted in it. The seller's price stays what it was.
    /// </summary>
    public static Visit Gild(Random rng, Visit visit)
    {
        var old = visit.Chest;
        var colour = (Rarity)Math.Min(4, (int)old.Quality + 1);
        var items = old.Items.Select(i => i with { Quality = Categories.RollQuality(rng, colour) }).ToList();
        if (items.Count == 0) items.Add(new Item(Category.Coins, Categories.RollQuality(rng, colour)));
        int magic = rng.Next(items.Count);
        items[magic] = items[magic] with { Magic = true, Fake = false };
        var chest = new Chest(items, colour, old.Hidden, old.Lock, gilded: true, packed: old.Packed);
        return new Visit(visit.Seller, chest, visit.Claim, visit.Trade, visit.Ask, visit.Floor, visit.Pitch)
        {
            Origin = visit.Origin, Fee = visit.Fee, Clue = visit.Clue, ClueKinds = visit.ClueKinds, Says = visit.Says,
        };
    }
}
