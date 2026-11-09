using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Keywright.Core;

namespace CurioTest;

/// <summary>One thing a curio's card promises, and a run that throws <see cref="Failed"/> if the game doesn't do it.</summary>
public sealed record Check(string Curio, string What, Action Run);

/// <summary>A check that failed: what the card says, and what the game did instead.</summary>
public sealed class Failed : Exception
{
    public Failed(string message) : base(message) { }
}

/// <summary>What a chest brought back when it was bought and opened (<see cref="T.BuyAndOpen"/>).</summary>
/// <param name="Value">Everything inside as it sells here, with the seller's terms settled and the bond paid.</param>
public sealed record Opened(Purchase Purchase, ReadReport Report, int Value, int Settled, int Insured, int Fee, int Paid);

/// <summary>
/// The checks' tools: a shop to test in, a chest and a visit, the opening's sums (the game's and the sim's order), and
/// the asserts. A curio's effect is best checked against a twin: the same state, the same chest, with and without it.
/// </summary>
public static class T
{
    /// <summary>
    /// A new run on day 1 at a shop tier (with the tools a player would own by then, as --start-tier gives them; pass
    /// <paramref name="bare"/> for none), with gold and these curios.
    /// </summary>
    public static GameState Shop(int tier = 1, int gold = 100000, int seed = 7, string character = Characters.Kit, bool bare = false,
        params string[] curios)
    {
        var s = GameState.NewRun(seed, character, guided: false);
        if (tier > 1 && !bare) s.SkipTo(tier, gold);
        else
        {
            if (tier > 1) typeof(GameState).GetProperty("Tier")!.SetValue(s, tier);
            s.Gold = gold;
        }
        foreach (string c in curios) s.GiveCurio(c);
        s.StartDay();
        return s;
    }

    /// <summary>The same shop twice: without the curio, and with it (both on day 1, same seed).</summary>
    public static (GameState Without, GameState With) Twins(string curio, int tier = 1, int gold = 100000, int seed = 7,
        string character = Characters.Kit, bool bare = false, params string[] others) =>
        (Shop(tier, gold, seed, character, bare, others), Shop(tier, gold, seed, character, bare, others.Append(curio).ToArray()));

    public static Item I(Category kind, Rarity quality = Rarity.Common, bool magic = false, bool fake = false) => new(kind, quality, magic, fake);

    /// <summary>A chest of these things, of this colour (a false bottom, a lock, packing as asked).</summary>
    public static Chest Chest(Rarity colour, params Item[] items) => new(items, colour);

    /// <summary>
    /// A visit by this seller with this chest, priced by them as the game would (their judgment, the shop's terms and
    /// index); <paramref name="ask"/> fixes the price instead.
    /// </summary>
    public static Visit Visit(GameState s, string seller, Chest chest, int? ask = null, Trade? trade = null, int seed = 1) =>
        Market.Price(new Random(seed), Sellers.Get(seller), chest, s.Tier, ask, trade, null, null, null, s.Boosts);

    /// <summary>A visit made exactly as the shop makes its customers' (the game's own GameState.NewVisit, curios and all).</summary>
    public static Visit NewVisit(GameState s, string seller) =>
        (Visit)typeof(GameState).GetMethod("NewVisit", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(s, new object[] { Sellers.Get(seller), false })!;

    /// <summary>
    /// Brings a visit to the counter the way the day does (<c>GameState.NextCustomer</c>: the build's word on the price,
    /// the Waiting Game, a sealed bid). <paramref name="behind"/> customers wait after it (none: it's the day's last).
    /// </summary>
    public static Visit Counter(GameState s, Visit v, int behind = 1)
    {
        s.Queue.Clear();
        s.Queue.Add(v);
        for (int i = 0; i < behind; i++) s.Queue.Add(Visit(s, "pip", Chest(Rarity.Common, I(Category.Coins)), ask: 1));
        s.NextCustomer();
        return s.Current;
    }

    /// <summary>A slip with these counts marked (as good as the chest's colour promises).</summary>
    public static Appraisal Slip(GameState s, Chest chest, params (Category Kind, int Count)[] marks)
    {
        var slip = new Appraisal(chest.Quality, s.Tier) { Model = s };
        foreach (var (kind, count) in marks) slip.Set(kind, count, false);
        return slip;
    }

    /// <summary>A slip that reads the chest exactly (every kind and count; magic marked; the false bottom if there's one).</summary>
    public static Appraisal Exact(GameState s, Chest chest)
    {
        var slip = Appraisal.Recorded(chest, s.Tier);
        slip.Model = s;
        return slip;
    }

    /// <summary>
    /// Buys the visit at its asking price (or <paramref name="price"/>, for an offer) on this slip and opens it the way
    /// the game and the sim do: luck at purchase, the lock's grade, each thing's sale here, the seller's terms, the bond,
    /// a perfect read's fee, the read recorded. Gold moves as it would.
    /// </summary>
    public static Opened BuyAndOpen(GameState s, Visit v, Appraisal slip, PickGrade grade = PickGrade.Clean, int? price = null)
    {
        var rng = new Random(1);
        if (v.Trade == Trade.Offer || price != null)
        {
            var response = v.Offer(price ?? Math.Max(1, v.Price), rng);
            if (response.Kind != ResponseKind.Accept) throw new Failed($"the offer of {price} was {response.Kind}, not accepted");
        }
        else v.Buy(rng);
        var purchase = s.Buy(v, slip);
        var report = slip.Check(v.Chest);
        s.Picked(v.Chest.Lock, grade);
        int value = v.Chest.Everything.Sum(item => purchase.IsClaimed(item) ? purchase.ClaimPays : s.SaleValue(item, grade, purchase));
        int settled = s.Settle(purchase, report.Perfect, value);
        value += settled;
        int insured = s.Insure(purchase, value);
        value += insured;
        int fee = report.Perfect ? s.PerfectFee(value) : 0;
        s.RecordRead(report.Perfect);
        s.ReadTrue(v.Chest, report.Perfect);
        return new Opened(purchase, report, value, settled, insured, fee, v.PricePaid);
    }

    // ---- Asserts ----

    public static void True(bool ok, string what)
    {
        if (!ok) throw new Failed(what);
    }

    public static void Eq<TV>(TV expected, TV actual, string what)
    {
        if (!EqualityComparer<TV>.Default.Equals(expected, actual)) throw new Failed($"{what}: expected {expected}, got {actual}");
    }

    /// <summary>Within a relative tolerance (default 2%, for rounding to whole coins).</summary>
    public static void Near(double expected, double actual, string what, double tolerance = 0.02)
    {
        double scale = Math.Max(1, Math.Abs(expected));
        if (Math.Abs(expected - actual) > tolerance * scale + 0.5) throw new Failed($"{what}: expected about {expected:0.###}, got {actual:0.###}");
    }

    /// <summary>The card's own words, for a check's description.</summary>
    public static string Card(string curio) => Curios.Get(curio) is CurioDef c ? $"{c.Name}: {c.Blurb}" : curio;
}
