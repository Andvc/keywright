using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// What the slip needs to know about the shop to value what's marked on it: how good a thing of a kind is likely to
/// come out here (luck), and what one fetches here (the shop's trades, its name, curios, the day). <see cref="GameState"/>
/// is one; a slip without one values things at their plain worth.
/// </summary>
public interface ISlipModel
{
    /// <summary>The chances of each quality for a thing of this kind in a chest of this colour.</summary>
    /// <param name="hidden">It's under a false bottom.</param>
    (Rarity Quality, float Chance)[] Odds(Category kind, Rarity chest, bool hidden = false);

    /// <summary>
    /// What one thing of this kind fetches here at this quality, if the read is right: with <paramref name="count"/> of
    /// its kind in the chest.
    /// </summary>
    float Value(Category kind, Rarity quality, bool magic, Rarity chest, int count, bool hidden = false);
}

/// <summary>
/// The run's build: what curios (and synergies, restorers and Heartsongs) do to chests, prices and the purse. Every
/// effect reaches everything in a chest you buy, and the balance is in the prices (<see cref="PowerIndex"/>): whatever
/// makes the things a seller can price worth more here (what the shop sells them for, how often they come out better
/// than their chest) puts every seller's price up as much, weighted by how often it applies. So the shop's growth,
/// ordinary or built, makes the numbers bigger without making a bad buy a good one, and buying blind never pays by it.
/// What no seller can price is where a build earns its edge: enchantment, false bottoms and wards (nobody knows they're
/// there), your eye (fees and streaks for a perfect read), what you pay (a haggler's tricks), and how you buy (a chest
/// taken sight unseen). (v20 kept the balance with a rule instead: a build reached only what your slip named.)
/// </summary>
public sealed partial class GameState : ISlipModel
{
    // ---- Counters the build keeps (this run) ----

    /// <summary>Chests passed since the last one bought (the Waiting Game).</summary>
    public int PassStreak { get; private set; }

    /// <summary>Chests in a row with something in them that came out better than its chest (the horseshoe).</summary>
    public int LuckStreak { get; private set; }

    /// <summary>Things that came out better than their chest, this run (the Lucky Shop's name, Fate's Spindle).</summary>
    public int Surprises { get; private set; }

    /// <summary>False bottoms marked on the slip and warded things named, this run (a Finder's name).</summary>
    public int HiddenFound { get; private set; }

    /// <summary>Chests talked down below what their seller first asked, or bought at a price you named, this run.</summary>
    public int Bargains { get; private set; }

    /// <summary>Perfect reads this run, all told (a valuer's register).</summary>
    public int PerfectReads { get; private set; }

    /// <summary>False bottoms in chests you bought, this run (a smuggler's log).</summary>
    public int BottomsOpened { get; private set; }

    /// <summary>Warded things named and given up, this run (a wardbreaker's tally).</summary>
    public int WardsNamed { get; private set; }

    /// <summary>Chests taken sight unseen (bought without a shake, a drop or the lodestone), this run.</summary>
    public int SightUnseen { get; private set; }

    // ---- Luck ----

    /// <summary>What the shop's restorers add to the chance a thing comes out a step better than its chest.</summary>
    public float RestorerLuck => Upgrades.All.Where(u => u.Luck > 0 && Has(u.Key)).Sum(u => u.Luck);

    /// <summary>The first chest bought today is still to come (a lucky coin's two tries).</summary>
    bool CoinPending => HasCurio("coin") && Today.Bought == 0;

    /// <summary>The first chest bought this week is still to come (a four-leaf clover's step).</summary>
    bool CloverPending => HasCurio("clover") && CloverWeek != Week;

    /// <summary>The week a four-leaf clover last brought luck: once a week, to the first chest you buy.</summary>
    public int CloverWeek { get; private set; }

    /// <summary>How a thing's quality is turned, in order, once its chest is bought.</summary>
    readonly record struct LuckRules(bool TwoTries, int Shift, float Step, bool Swing, bool Mend, bool Chimes, bool Steady);

    /// <param name="hidden">Under a false bottom (a hollow rod's step).</param>
    /// <param name="warded">A warded thing the slip named (a ward-key's two steps).</param>
    /// <param name="firstToday">The day's first chest bought (a lucky coin's two tries).</param>
    /// <param name="firstThisWeek">The week's first (a four-leaf clover's step).</param>
    /// <param name="unseen">Taken sight unseen (a lucky dip's luck, a blindfold's mending).</param>
    LuckRules Rules(Category kind, bool hidden, bool warded, bool firstToday, bool firstThisWeek, bool unseen)
    {
        float step = StepChance(kind) + (unseen && HasCurio("dip") ? Curios.DipLuck : 0);
        int shift = (firstThisWeek && HasCurio("clover") ? 1 : 0) + (hidden && HasCurio("rod") ? 1 : 0) + (warded && HasCurio("wardkey") ? 1 : 0);
        return new LuckRules(firstToday && HasCurio("coin"), shift, Math.Min(0.9f, step), HasCurio("token"), Mends(kind) || (unseen && HasCurio("blindfold")),
            Shows(Heartsongs.Chimes), HasCurio("crescent"));
    }

    /// <summary>
    /// The chance a thing of this kind comes out a step better than its chest, however it was bought: the restorers, a
    /// rabbit's foot, a gem cutter's wheel, a horseshoe's streak, a trade's glove or cap, the shop's name for luck, and
    /// Fate's Spindle.
    /// </summary>
    float StepChance(Category kind) =>
        RestorerLuck + (HasCurio("foot") ? Curios.FootLuck : 0) + (kind == Category.Gems && HasCurio("wheel") ? Curios.WheelLuck : 0) +
        (HasCurio("horseshoe") ? Math.Min(Curios.HorseshoeMax, Curios.HorseshoeStep * LuckStreak) : 0) +
        ((kind == Category.Arms && HasCurio("gauntlet")) || (kind == Category.Garb && HasCurio("cap")) ? Curios.TradeLuck : 0) +
        Synergies.LuckBonus(Active("fortune")) + (HasCurio("spindle") ? Math.Min(Curios.SpindleMax, Curios.SpindleStep * Surprises) : 0);

    /// <summary>Never worse than its chest promised: a smith's glove for weapons, a hatter's cap for cloaks, the Imp's Ember for all.</summary>
    bool Mends(Category kind) => (kind == Category.Arms && HasCurio("gauntlet")) || (kind == Category.Garb && HasCurio("cap")) || Shows(Heartsongs.Ember);

    static Rarity Up(Rarity q, int steps = 1) => (Rarity)Math.Min((int)Rarity.Legendary, (int)q + steps);

    /// <summary>A thing's quality, turned by the rules (the base roll is the chest's own, made when it was packed).</summary>
    Rarity Turn(Rarity q, Rarity chest, LuckRules r)
    {
        if (r.TwoTries && Categories.RollQuality(Rng, chest) is var again && again > q) q = again;
        q = Up(q, r.Shift);
        if (r.Step > 0 && Rng.NextFloat() < r.Step) q = Up(q);
        if (r.Swing && q == chest && Rng.NextFloat() < Curios.TokenSwing) q = Categories.Swing(Rng, q);
        if (r.Mend && q < chest) q = chest;
        if (r.Chimes && q == chest && q < Rarity.Legendary && Rng.NextFloat() < Heartsongs.ChimesLuck) q = Up(q);
        return r.Steady ? chest : q;
    }

    /// <summary>The same turning, on the odds of each quality, for the slip and the power index.</summary>
    static float[] Turn(float[] d, Rarity chest, LuckRules r)
    {
        int c = (int)chest;
        float[] Shifted(float[] x, int steps)
        {
            var y = new float[5];
            for (int q = 0; q < 5; q++) y[Math.Min(4, q + steps)] += x[q];
            return y;
        }
        if (r.TwoTries)
        {
            // The better of two tries: the chance it's at most q is the base chance, squared.
            var base_ = Categories.Odds(chest);
            float[] cdf = new float[5];
            float acc = 0;
            for (int q = 0; q < 5; q++)
            {
                acc += base_.Where(o => (int)o.Quality == q).Sum(o => o.Chance);
                cdf[q] = acc * acc;
            }
            d = new float[5];
            for (int q = 0; q < 5; q++) d[q] = cdf[q] - (q > 0 ? cdf[q - 1] : 0);
        }
        if (r.Shift > 0) d = Shifted(d, r.Shift);
        if (r.Step > 0)
        {
            var up = Shifted(d, 1);
            d = d.Select((p, q) => p * (1 - r.Step) + up[q] * r.Step).ToArray();
        }
        if (r.Swing)
        {
            float moved = d[c] * Curios.TokenSwing;
            d[c] -= moved;
            var swings = Categories.SwingOdds(chest);
            foreach (var (q, p) in swings) d[(int)q] += moved * p;
        }
        if (r.Mend)
        {
            for (int q = 0; q < c; q++)
            {
                d[c] += d[q];
                d[q] = 0;
            }
        }
        if (r.Chimes && c < 4)
        {
            float moved = d[c] * Heartsongs.ChimesLuck;
            d[c] -= moved;
            d[c + 1] += moved;
        }
        if (r.Steady)
        {
            d = new float[5];
            d[c] = 1;
        }
        return d;
    }

    static float[] Spread(Rarity chest)
    {
        var d = new float[5];
        foreach (var (q, p) in Categories.Odds(chest)) d[(int)q] += p;
        return d;
    }

    public (Rarity Quality, float Chance)[] Odds(Category kind, Rarity chest, bool hidden = false) =>
        Turn(Spread(chest), chest, Rules(kind, hidden, false, CoinPending, CloverPending, false))
            .Select((p, q) => ((Rarity)q, p)).Where(o => o.p > 0).ToArray();

    /// <summary>The day a shooting star last fell: once a day.</summary>
    int _starDay;

    /// <summary>
    /// What luck did to a chest you've just bought, with the curios that did it and how many things each changed (the
    /// opening says so).
    /// </summary>
    /// <param name="unseen">It was taken sight unseen.</param>
    List<(string Curio, int Changed)> Luck(Chest chest, bool unseen)
    {
        var changes = new List<(string, int)>();
        bool firstToday = Today.Bought == 1, firstThisWeek = CloverPending;
        if (firstThisWeek) CloverWeek = Week;
        var before = chest.Everything.ToDictionary(i => i.Id, i => i.Quality);
        var unwarded = chest.Unwarded?.Id;
        chest.Improve(i =>
        {
            if (i.Fake || i.Broken) return i;
            var r = Rules(i.Category, i == chest.Hidden, i.Id == unwarded, firstToday, firstThisWeek, unseen);
            return i with { Quality = Turn(i.Quality, chest.Quality, r) };
        });
        int Count(Func<Item, bool> which) => chest.Everything.Count(i => !i.Fake && !i.Broken && which(i) && i.Quality != before[i.Id]);
        // Which curio to thank: the first that could have done it (the opening names one, with how many things).
        if (HasCurio("crescent")) changes.Add(("crescent", Count(_ => true)));
        else
        {
            var lucky = new List<string> { "clover", "coin" };
            if (unseen) lucky.Add("dip");
            lucky.AddRange(new[] { "spindle", "foot", "horseshoe", "wheel", "rod", "wardkey" });
            string thanked = lucky.FirstOrDefault(HasCurio);
            int better = Count(i => i.Quality > before[i.Id]);
            if (thanked != null && better > 0) changes.Add((thanked, better));
            else if (RestorerLuck > 0 && better > 0) changes.Add(("restorer", better));
            if (HasCurio("token") && Count(i => i.Quality < before[i.Id]) is var swung and > 0) changes.Add(("token", swung));
            foreach (var key in unseen ? new[] { "gauntlet", "cap", "blindfold" } : new[] { "gauntlet", "cap" })
                if (HasCurio(key) && Count(i => i.Quality == chest.Quality && before[i.Id] < chest.Quality) is var mended and > 0) changes.Add((key, mended));
        }
        // A shooting star, once a day: the first thing to come out better than its chest climbs a step further (no seller
        // prices a star: it's once a day, and nobody knows which day it'll be).
        if (HasCurio("star") && !HasCurio("crescent") && _starDay != Day &&
            chest.Everything.FirstOrDefault(i => !i.Fake && !i.Broken && !i.IsSong && i.Quality > chest.Quality && i.Quality < Rarity.Legendary) is Item fallen)
        {
            _starDay = Day;
            chest.Improve(i => i.Id == fallen.Id ? i with { Quality = Up(i.Quality) } : i);
            changes.Add(("star", 1));
        }
        // The Imp's Ember burns the lie out of demon gold, paste and sand: every fake comes out real.
        if (Shows(Heartsongs.Ember) && chest.Items.Count(i => i.Fake) is var fakes && fakes > 0)
        {
            chest.Improve(i => i.Fake ? i with { Fake = false } : i);
            changes.Add((Heartsongs.Ember, fakes));
        }
        return changes.Where(c => c.Item2 > 0).ToList();
    }

    // ---- What things fetch ----

    /// <summary>
    /// What the shop's name and build put on a thing of this kind, whoever bought it how (all of it in the power index):
    /// what it's sure to fetch (<see cref="Certain"/>), and a wishbone's half again on a surprise.
    /// </summary>
    /// <param name="count">How many of its kind were in the chest (a moneychanger's scales count purses).</param>
    float General(Category kind, Rarity quality, Rarity chest, int count) =>
        Certain(kind, count) * (quality > chest && HasCurio("wishbone") ? Curios.WishboneBonus : 1);

    /// <summary>What a thing of this kind is sure to fetch here against its worth: the shop's name for its kind (an active synergy), the trades' curios, gallery lamps.</summary>
    float Certain(Category kind, int count)
    {
        float b = 1 + Synergies.SaleBonus(this, kind);
        b *= kind switch
        {
            Category.Arms when HasCurio("whetstone") => Curios.WhetstoneBonus,
            Category.Garb when HasCurio("dummy") => Curios.TradeBonus,
            Category.Potions when HasCurio("still") => Curios.TradeBonus,
            Category.Scrolls when HasCurio("desk") => Curios.TradeBonus,
            Category.Coins when HasCurio("scales") => 1 + Curios.ScalesStep * count,
            _ => 1,
        };
        if (HasCurio("lamps")) b *= 1 + Curios.LampsPerPoint * Math.Min(ExhibitionCap, Exhibition);
        return b;
    }

    /// <summary>
    /// What a thing nobody but you knows is there fetches, against an ordinary one (never in the index): enchantment (the
    /// Arcanist's name, a spellbinder's ring), what was under a false bottom (a third more, a Finder's name, a black-market
    /// pass), a warded thing named (a third more, a Finder's name, a wardbreaker's tally).
    /// </summary>
    float Special(bool magic, bool hidden, bool warded)
    {
        float b = 1;
        if (magic) b *= (1 + Synergies.MagicSales(this)) * (HasCurio("ring") ? Curios.RingMagic / Categories.MagicMultiplier : 1);
        // A hidden thing is never enchanted: it fetches a third more instead (what someone hid is a little special).
        if (hidden || warded) b *= Categories.HiddenMultiplier * (1 + Synergies.FinderSales(Active("finder")));
        if (hidden && HasCurio("pass")) b *= Curios.PassBonus;
        if (warded && HasCurio("wardtally")) b *= 1 + Curios.WardTallyStep * WardsNamed;
        return b;
    }

    /// <summary>
    /// What your eye earns on a chest you read (never in the index: no seller can price it): a tuning fork's streak, and
    /// on the slip's verdict a ledger's quarter either way and a truthglass's double for a perfect read. The slip
    /// assumes its read is right.
    /// </summary>
    float Read(bool? perfect) => (HasCurio("fork") ? 1 + Math.Min(Curios.ForkMax, Curios.ForkStep * PerfectStreak) : 1) *
                                 (perfect is bool right ? (HasCurio("ledger") ? right ? Curios.LedgerRight : Curios.LedgerWrong : 1) *
                                                          (right && HasCurio("truthglass") ? Curios.TruthglassBonus : 1) : 1);

    public float Value(Category kind, Rarity quality, bool magic, Rarity chest, int count, bool hidden = false) =>
        Categories.Value(kind, quality, magic) * SaleMultiplier(kind) * Read(true) *
        (kind == Category.Heartsong ? 1 : General(kind, quality, chest, count) * Special(magic, hidden, false));

    /// <summary>
    /// What a thing of this kind usually fetches here in a chest of this colour, luck and the shop's name and all: for a
    /// careful player's guess at what they couldn't hear.
    /// </summary>
    public float Expected(Category kind, Rarity chest) =>
        Turn(Spread(chest), chest, Rules(kind, false, false, false, false, false))
            .Select((p, q) => p * Categories.Value(kind, (Rarity)q, false) * General(kind, (Rarity)q, chest, 1)).Sum() * SaleMultiplier(kind);

    /// <summary>What a broken thing still fetches: nothing, but an alchemist's still makes half of a broken potion good.</summary>
    float BrokenValue(Item item) => item.Category == Category.Potions && HasCurio("still") ? Categories.Value(item.Category, item.Quality, item.Magic) * Curios.StillBroken : 0;

    /// <summary>
    /// What a fake sells for: nothing much, unless a fence's mark finds the Maw's buyers for it (what the real thing
    /// would fetch), a jeweller's loupe sells paste and demon gold as costume pieces, or a scribe's desk sand as ink.
    /// </summary>
    float FakeValue(Item item)
    {
        float real = Categories.Value(item.Category, item.Quality, false);
        if (HasCurio("mark")) return real;
        if (HasCurio("loupe") && item.Category is Category.Coins or Category.Gems) return real * Curios.LoupeFakes;
        if (HasCurio("desk") && item.Category == Category.Scrolls) return real * Curios.InkFakes;
        return item.Value;
    }

    /// <summary>
    /// What a thing fetches when you sell it: its worth, how cleanly the lock came open, what the shop's trades and the
    /// day put on everything (<see cref="SaleMultiplier"/>), what the shop's name and build put on it, and out of a chest
    /// you bought (<paramref name="from"/>), what your read of it earned.
    /// </summary>
    public int SaleValue(Item item, PickGrade grade = PickGrade.Clean, Purchase from = null) =>
        (int)MathF.Round(SaleRaw(item, grade, from?.Chest, from == null ? null : from.Perfect));

    /// <param name="warded">Whether it's the chest's warded thing, named (by default: the one the ward gave up).</param>
    float SaleRaw(Item item, PickGrade grade, Chest chest, bool? perfect, bool? warded = null)
    {
        float value = item.Broken ? BrokenValue(item) : item.Fake ? FakeValue(item) : item.Value;
        value *= GradeMultiplier(grade) * SaleMultiplier(item.Category);
        if (!item.Fake && !item.Broken && !item.IsSong)
        {
            int count = chest == null ? 1 : chest.Everything.Count(i => i.Category == item.Category && !i.Fake && !i.Broken);
            bool hidden = chest != null && item == chest.Hidden;
            bool ward = warded ?? (chest?.Unwarded != null && item.Id == chest.Unwarded.Id);
            value *= General(item.Category, item.Quality, chest?.Quality ?? item.Quality, Math.Max(1, count)) * Special(item.Magic, hidden, ward);
        }
        if (chest != null) value *= Read(perfect);
        return value;
    }

    /// <summary>
    /// What everything in a chest would have sold for here today, bought on this slip: a false bottom's and a warded
    /// thing's too, if the slip would have named it. For your gut, when you pass.
    /// </summary>
    public int Fetches(Chest chest, Appraisal slip = null)
    {
        bool? perfect = slip == null ? null : slip.Check(chest).Perfect;
        float total = chest.Everything.Sum(i => SaleRaw(i, PickGrade.Clean, chest, perfect));
        // A warded thing the slip names would have come out; one it doesn't stays in the ward, unsold.
        if (slip != null && chest.Warded != null && chest.Names(slip)) total += SaleRaw(chest.Warded, PickGrade.Clean, chest, perfect, warded: true);
        return (int)MathF.Round(total);
    }

    /// <summary>
    /// A perfect read's fees: a keen eye's (the synergy), an appraiser's seal's, and a valuer's register's for every
    /// perfect read so far this run, on what the chest held.
    /// </summary>
    public int PerfectFee(int held) => (int)MathF.Round(held * PerfectShare);

    /// <summary>What a perfect read's fees come to, as a share of what the chest held.</summary>
    public float PerfectShare => Synergies.KeenFee(Active("keen")) + (HasCurio("seal") ? Curios.SealFee : 0) +
                                 (HasCurio("register") ? Math.Min(Curios.RegisterMax, Curios.RegisterStep * PerfectReads) : 0);

    // ---- The customers ----

    /// <summary>
    /// A customer is at the counter. What the build takes off their price (the Waiting Game, a wholesaler's tally, a
    /// haggler's knot, the Closing Bell for the day's last) and, with a sealed bid or a blindfold, what they'd take for
    /// it sight unseen (until you lay a hand on it).
    /// </summary>
    void Greet(Visit visit)
    {
        if (visit == null || visit.Lesson != null) return;
        visit.Chest.Unmasked = HasCurio("trumpet");
        if (HasCurio("hourglass") && PassStreak > 0)
        {
            float off = Math.Min(Curios.WaitMax, Curios.WaitStep * PassStreak);
            visit.Reduce(1 - off, $"Waiting Game: {Synergies.Pct(off)} off.");
        }
        if (HasCurio("wholesale") && Today.Bought > 0)
        {
            float off = Math.Min(Curios.WholesaleMax, Curios.WholesaleStep * Today.Bought);
            visit.Reduce(1 - off, $"Wholesaler's Tally: {Synergies.Pct(off)} off.");
        }
        if (HasCurio("knot") && Bargains > 0)
        {
            float off = Math.Min(Curios.KnotMax, Curios.KnotStep * Bargains);
            visit.Reduce(1 - off, $"Haggler's Knot: {Synergies.Pct(off)} off.");
        }
        if (HasCurio("bell") && Queue.Count == 0) visit.Reduce(Curios.BellPrice, $"Closing Bell: the day's last, {Synergies.Pct(1 - Curios.BellPrice)} off.");
        if (UnseenPrice < 1) visit.Seal(UnseenPrice, $"Sight unseen: {Synergies.Pct(1 - UnseenPrice)} off while you keep your hands off it.");
    }

    /// <summary>What a seller takes for a chest you buy without a shake, a drop or the lodestone: a sealed bid's price, and a blindfold's.</summary>
    public float UnseenPrice => (HasCurio("sealed") ? Curios.SealedPrice : 1) * (HasCurio("blindfold") ? Curios.BlindfoldPrice : 1);

    /// <summary>The day a queue ticket last sent someone to the back of the queue: once a day.</summary>
    int _queuedOn;

    /// <summary>
    /// A chest's been passed: the Waiting Game counts it, and with a queue ticket the day's first seller passed comes back
    /// at the end of the day for a fifth less.
    /// </summary>
    void Passed(Visit visit)
    {
        PassStreak++;
        if (HasCurio("queue") && _queuedOn != Day && visit != null && visit.Lesson == null && !visit.Returned && visit.Outcome == VisitOutcome.Passed)
        {
            _queuedOn = Day;
            Queue.Add(visit.Again(Curios.QueuePrice));
        }
    }

    /// <summary>What a chest just bought did to the build's counters: the horseshoe's streak, surprises, finds, bargains.</summary>
    void Counted(Visit visit, Chest chest, Appraisal slip)
    {
        PassStreak = 0;
        int surprised = chest.Everything.Count(i => !i.Fake && !i.Broken && !i.IsSong && i.Quality > chest.Quality);
        Surprises += surprised;
        LuckStreak = surprised > 0 ? LuckStreak + 1 : 0;
        if (chest.Hidden != null)
        {
            BottomsOpened++;
            if (slip.Hidden) HiddenFound++;
        }
        if (chest.Unwarded != null)
        {
            HiddenFound++;
            WardsNamed++;
        }
        if (!visit.Handled) SightUnseen++;
        if (visit.FirstAsk > 0 && visit.PricePaid < visit.FirstAsk && visit.Trade != Trade.Fixed) Bargains++;
        else if (visit.Trade == Trade.Offer) Bargains++;
    }

    /// <summary>A haggler's purse: what a haggle talked a seller down by comes back again.</summary>
    public int Rebate(Visit visit) => HasCurio("coffer") && visit.Trade == Trade.Haggle && visit.FirstAsk > visit.PricePaid
        ? Money.Round((visit.FirstAsk - visit.PricePaid) * Curios.CofferShare) : 0;

    /// <summary>
    /// The Underwriters' bond: a chest taken sight unseen that held less than you paid for it pays back half the
    /// difference. Paid into the purse; returns what it paid.
    /// </summary>
    public int Insure(Purchase purchase, int held)
    {
        if (!HasCurio("bond") || !purchase.SightUnseen || held >= purchase.Paid) return 0;
        int payout = Money.Round((purchase.Paid - held) * Curios.BondShare);
        Gold += payout;
        Today.Curios += payout;
        return payout;
    }

    /// <summary>
    /// The day's crowd, as the build has it: with a sleeper's pillow a third of the sellers with nothing to tell (with
    /// the Aldric Seal, all of them) have warded something and say what they dreamt; with counting beads, every chest
    /// with nothing else said comes with a tally of what's in it.
    /// </summary>
    void Curiosities()
    {
        float dreams = HasCurio("aldricseal") ? 1 : HasCurio("pillow") ? Curios.PillowShare : 0;
        for (int i = 0; i < Queue.Count; i++)
        {
            var visit = Queue[i];
            if (visit.Lesson != null || visit.Says != Clue.None || visit.Chest.Sealed) continue;
            if (dreams > 0 && Rng.NextFloat() < dreams)
                Queue[i] = Events.Restate(Rng, visit, "dreaming", Tier, ward: true);
            else if (HasCurio("beads")) Queue[i] = Events.Restate(Rng, visit, "census", Tier);
        }
    }

    // ---- The shop's power index ----

    /// <summary>
    /// The shop's power index: what an ordinary thing a seller brings fetches here, luck and all, against its plain worth,
    /// on average over what this shop's sellers bring (each kind by its share of what's in their chests,
    /// <see cref="Market.ValueShares"/>) at the colour their chests usually are (<see cref="ShopTiers.Typical"/>). In it:
    /// the buyers' trades, the restorers, the shop's name for a kind and for luck, the Heartsongs that put up prices or
    /// luck, and every curio that does the same for anything a seller can price, each weighted by how often it applies
    /// (a lucky coin's chest by how many a day you buy, a clover's by the week). Every seller asks that much more
    /// (<see cref="PriceFactor"/>), so it makes the numbers bigger, not a bad buy good. Left out: what no seller can
    /// price (enchantment, false bottoms, wards, Heartsongs in the chest), your eye (fees and streaks for a perfect read),
    /// how you buy (sight unseen), what curios take off a price, and today's event (a festival is a good day to buy).
    /// </summary>
    public float PowerIndex => IndexAt(Tier);

    /// <summary>How many chests a day a careful player buys, for every customer: what a day's first chest is weighted by.</summary>
    const float BuysPerCustomer = 0.4f;

    /// <summary>
    /// How much of what luck adds sellers price in: what a chest is sure to fetch goes in whole, but a step better than
    /// its chest is a long tail, and a seller prices what a chest usually holds, the middle of its range, not its best
    /// day; luck moves the middle about half as far as the average. (A buyer who goes by the odds bar weighs a chest the
    /// same way: 0.20's restorers, priced in whole, made the careful player buy less; priced in at nothing, buying blind
    /// paid at the Emporium.)
    /// </summary>
    public const float LuckPricedIn = 0.5f;

    /// <summary>The power index at a shop of this tier (its sellers, and the colour its chests usually are).</summary>
    public float IndexAt(int tier)
    {
        var colour = ShopTiers.Typical(tier);
        float index = 0;
        foreach (var (kind, share) in Market.ValueShares(tier)) index += share * KindPower(kind, colour);
        return index;
    }

    /// <summary>
    /// What a thing of this kind in a chest of this colour fetches here on average against its plain worth, as sellers
    /// price it: what it's sure to fetch, and half of what luck adds to that (<see cref="LuckPricedIn"/>), a day's first
    /// chest weighted by how many a day are bought and a week's by how many a week.
    /// </summary>
    float KindPower(Category kind, Rarity colour)
    {
        var spread = Spread(colour);
        float Worth(LuckRules r) => Turn(spread, colour, r).Select((p, q) => p * Categories.Value(kind, (Rarity)q, false) * General(kind, (Rarity)q, colour, 1)).Sum();
        float plain = spread.Select((p, q) => p * Categories.Value(kind, (Rarity)q, false)).Sum();
        float sure = plain * Certain(kind, 1);
        var always = Rules(kind, false, false, false, false, false);
        float lucky = Worth(always), usual = lucky;
        float perDay = Math.Max(1, CustomersPerDay * BuysPerCustomer);
        if (HasCurio("coin")) lucky += (Worth(always with { TwoTries = true }) - usual) / perDay;
        if (HasCurio("clover")) lucky += (Worth(always with { Shift = always.Shift + 1 }) - usual) / (perDay * Guild.DaysPerWeek);
        return PlainSales(kind) * (sure + LuckPricedIn * (lucky - sure)) / plain;
    }

    /// <summary>What the buyers' trades and the Dragon's Egg put on a thing of this kind, without today's event.</summary>
    float PlainSales(Category kind) => Buyers * (kind is Category.Coins or Category.Gems && Shows(Heartsongs.Egg) ? Heartsongs.EggSales : 1);

    /// <summary>
    /// What sellers ask of this shop, against what they'd ask anyone: its power index, and what puts prices up or down
    /// besides (the Lullaby Box, a foreclosure sale, a barker's horn, the cursed crescent).
    /// </summary>
    public float PriceFactor => PriceFactorAt(Tier);

    float PriceFactorAt(int tier) => IndexAt(tier) *
                                     (Shows(Heartsongs.Lullaby) ? Heartsongs.LullabyPrices : 1) * (EventIs("estate") ? Events.EstatePrices : 1) *
                                     (HasCurio("horn") ? Curios.HornDearer : 1) * (HasCurio("crescent") ? Curios.CrescentPrices : 1);

    /// <summary>How many of a shop's typical chests the working purse holds.</summary>
    public const int ChestsKept = 2;

    /// <summary>
    /// The working purse: enough for two of this shop's typical chests (the median of what its crowd asks at plain
    /// prices, <see cref="Market.TypicalAsk"/>) at what sellers ask here (<see cref="PriceFactor"/>). Bram's advice at the
    /// day's end, what the careful player keeps back, and the line a purchase from the shop or Nim warns before crossing.
    /// </summary>
    public int WorkingPurse => WorkingPurseAt(Tier);

    /// <summary>The working purse at a shop of this tier, with what the shop owns now.</summary>
    public int WorkingPurseAt(int tier) => Money.Round(ChestsKept * Market.TypicalAsk(tier) * PriceFactorAt(tier));

    /// <summary>
    /// The working purse as it would be with this upgrade (at the bigger shop, for a move inward) or curio too: its effect
    /// on the power index and on what sellers ask, before you buy it.
    /// </summary>
    public int ReserveIf(Upgrade upgrade = null, CurioDef curio = null) =>
        With(upgrade, curio, () => WorkingPurseAt(upgrade != null && upgrade.SetsTier > Tier ? upgrade.SetsTier : Tier));

    /// <summary>The power index with this upgrade or curio too.</summary>
    public float IndexIf(Upgrade upgrade = null, CurioDef curio = null) =>
        With(upgrade, curio, () => IndexAt(upgrade != null && upgrade.SetsTier > Tier ? upgrade.SetsTier : Tier));

    /// <summary>
    /// Whether buying this, at this price, would leave less gold than the working purse it would need: the shop shows it
    /// in red and asks first.
    /// </summary>
    public bool LeavesShort(int price, Upgrade upgrade = null, CurioDef curio = null) => Gold - price < ReserveIf(upgrade, curio);

    /// <summary>
    /// Why buying this would leave you short, as the shop's warning says it, or null if it wouldn't: the Guild tonight,
    /// or what two of the shop's typical chests would cost after it (at the bigger shop, for a move inward; with sellers
    /// asking more for what it adds) against what you'd have left.
    /// </summary>
    public string ShortNote(int price, Upgrade upgrade = null, CurioDef curio = null)
    {
        int left = Gold - price;
        if (DuesTonight && !PaidThisWeek && left < DueNow) return $"The Guild comes tonight for {DueNow}g, and you'd have {left}g left.";
        int keep = ReserveIf(upgrade, curio);
        if (left >= keep) return null;
        bool moving = upgrade != null && upgrade.SetsTier > Tier;
        float rise = (float)keep / Math.Max(1, WorkingPurse) - 1;
        string dearer = moving || rise < 0.05f ? "" : $" (sellers will ask about {Synergies.Pct(rise)} more once they hear of it)";
        return $"After the {upgrade?.Name ?? curio?.Name}, two typical chests {(moving ? $"at {ShopTiers.Name(upgrade.SetsTier)}" : "here")} would cost " +
               $"about {keep}g{dearer}, and you'd have {left}g. You may not be able to afford chests for a while.";
    }

    /// <summary>Works something out as if this upgrade or curio were owned too.</summary>
    T With<T>(Upgrade upgrade, CurioDef curio, Func<T> work)
    {
        bool addUpgrade = upgrade != null && _owned.Add(upgrade.Key);
        bool addCurio = curio != null && !_curios.Contains(curio.Key);
        if (addCurio) _curios.Add(curio.Key);
        try
        {
            return work();
        }
        finally
        {
            if (addUpgrade) _owned.Remove(upgrade.Key);
            if (addCurio) _curios.Remove(curio.Key);
        }
    }

    // ---- Nim ----

    /// <summary>What was in your purse when Nim arrived this morning: part of his price is a share of it.</summary>
    public int PeddlerPurse { get; private set; }

    /// <summary>The shop Nim last came to: on his first visit to a grander one he brings something for a way of trading that opens there.</summary>
    public int PeddlerTier { get; private set; }

    /// <summary>What Nim asks for a curio today (nothing, while the keepsake is still to give).</summary>
    public int CuriosPrice(CurioDef curio) => Money.Round(Curios.Price(curio, PeddlerPurse) * (Under(Challenges.DearCurios) ? Challenges.DearCurioPrices : 1));
}
