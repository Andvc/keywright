using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// What the slip needs to know about the shop to value what's marked on it: how good a named thing of a kind is likely to
/// come out (luck), and what one fetches here (the shop's trades, its name, curios, the day). <see cref="GameState"/> is
/// one; a slip without one values things at their plain worth.
/// </summary>
public interface ISlipModel
{
    /// <summary>The chances of each quality for a thing of this kind you've named, in a chest of this colour.</summary>
    /// <param name="hidden">It's under a false bottom you found.</param>
    (Rarity Quality, float Chance)[] Odds(Category kind, Rarity chest, bool hidden = false);

    /// <summary>
    /// What one named thing of this kind fetches here at this quality, if the read is right: with <paramref name="count"/>
    /// of its kind named in the chest.
    /// </summary>
    float Value(Category kind, Rarity quality, bool magic, Rarity chest, int count, bool hidden = false);
}

/// <summary>
/// The run's build: what curios (and synergies and talents) do to chests, prices and the purse. The rule they all keep:
/// what puts value on a chest puts it on what you <b>named</b> (<see cref="Item.Named"/>: counted on the slip you bought
/// it on, without counting more of its kind than were there). The shop's own upgrades (its trades, its restorers) put
/// value on everything, and sellers, who hear of them, ask that much more (<see cref="Boosts.Dearer"/>). So a build
/// amplifies judgment and never pays for buying blind, and a wrong read forfeits what the build would have added.
/// </summary>
public sealed partial class GameState : ISlipModel
{
    // ---- Naming ----

    /// <summary>
    /// What a slip names in a chest, by item: things counted right (the first of each kind, enchanted ones first if it
    /// marked one enchanted), a false bottom it marked, a warded thing it named; nothing of a kind it counted more of than
    /// there were (a guess at a crowd names nothing).
    /// </summary>
    public static (HashSet<int> Named, HashSet<int> Magic) Naming(Chest chest, Appraisal slip)
    {
        var report = slip.Check(chest);
        var ghosts = report.Ghosts.ToHashSet();
        var named = report.Items.Where(r => r.Kind is ReadKind.Right or ReadKind.RightMagic or ReadKind.NotMagic or ReadKind.FoundHidden &&
                                            !ghosts.Contains(r.Item.Category)).Select(r => r.Item.Id).ToHashSet();
        var magic = report.Items.Where(r => r.Kind == ReadKind.RightMagic && named.Contains(r.Item.Id)).Select(r => r.Item.Id).ToHashSet();
        return (named, magic);
    }

    /// <summary>Marks what the slip you bought it on named (<see cref="Naming"/>), before luck, so luck knows.</summary>
    public static void Name(Chest chest, Appraisal slip)
    {
        var (named, magic) = Naming(chest, slip);
        chest.Improve(i => i with { Named = named.Contains(i.Id), MagicNamed = magic.Contains(i.Id) });
    }

    // ---- Counters the build keeps ----

    /// <summary>Chests passed since the last one bought (the Waiting Game).</summary>
    public int PassStreak { get; private set; }

    /// <summary>Chests in a row with something named in them that came out better than its chest (the horseshoe).</summary>
    public int LuckStreak { get; private set; }

    /// <summary>Enchanted things named this run (the Aether Jar).</summary>
    public int MagicNamed { get; private set; }

    /// <summary>Things you named that came out better than their chest, this run (the Lucky Shop's name).</summary>
    public int Surprises { get; private set; }

    /// <summary>False bottoms found and warded things named, this run (a Finder's name).</summary>
    public int HiddenFound { get; private set; }

    /// <summary>Chests talked down below what their seller first asked, this run (a Haggler's name).</summary>
    public int Bargains { get; private set; }

    // ---- Luck ----

    /// <summary>
    /// What the shop's restorers add to the chance a thing you named comes out a step better: they work from the slip, and
    /// sell what nobody named as it was found.
    /// </summary>
    public float RestorerLuck => Upgrades.All.Where(u => u.Luck > 0 && Has(u.Key)).Sum(u => u.Luck);

    /// <summary>The first chest bought today is still to come (a lucky coin's two tries).</summary>
    bool CoinPending => HasCurio("coin") && Today.Bought == 0;

    /// <summary>The first chest bought this week is still to come (a four-leaf clover's step).</summary>
    bool CloverPending => HasCurio("clover") && CloverWeek != Week;

    /// <summary>The week a four-leaf clover last brought luck: once a week, to the first chest you buy.</summary>
    public int CloverWeek { get; private set; }

    /// <summary>How a thing's quality is turned, in order, once its chest is bought.</summary>
    readonly record struct LuckRules(bool TwoTries, int Shift, float Step, bool Swing, bool Mend, bool Chimes, bool Star, bool Steady);

    /// <param name="named">The slip named it.</param>
    /// <param name="hidden">Under a false bottom you found (a hollow rod's step).</param>
    /// <param name="warded">A warded thing you named (a ward-key's two steps).</param>
    /// <param name="first">Whether this chest is the day's (and the week's) first bought: for the slip, the next one; at purchase, this one.</param>
    LuckRules Rules(Category kind, bool named, bool hidden, bool warded, bool firstToday, bool firstThisWeek)
    {
        float step = 0;
        if (named)
        {
            step += RestorerLuck;
            if (HasCurio("foot")) step += Curios.FootLuck;
            if (kind == Category.Gems && HasCurio("wheel")) step += Curios.WheelLuck;
            if (HasCurio("horseshoe")) step += Math.Min(Curios.HorseshoeMax, Curios.HorseshoeStep * LuckStreak);
            if ((kind == Category.Arms && HasCurio("gauntlet")) || (kind == Category.Garb && HasCurio("cap"))) step += Curios.TradeLuck;
            step += Synergies.LuckBonus(Active("fortune"));
        }
        int shift = (named && firstThisWeek && HasCurio("clover") ? 1 : 0) + (named && hidden && HasCurio("rod") ? 1 : 0) +
                    (warded && HasCurio("wardkey") ? 2 : 0);
        bool mend = (kind == Category.Arms && HasCurio("gauntlet")) || (kind == Category.Garb && HasCurio("cap")) || Shows(Heartsongs.Ember);
        return new LuckRules(named && firstToday && HasCurio("coin"), shift, Math.Min(0.9f, step), HasCurio("token"), mend, Shows(Heartsongs.Chimes),
            named && HasCurio("star"), HasCurio("crescent"));
    }

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
        if (r.Star && q > chest) q = Up(q);
        return r.Steady ? chest : q;
    }

    /// <summary>The same turning, on the odds of each quality, for the slip.</summary>
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
        if (r.Star)
        {
            var star = new float[5];
            for (int q = 0; q < 5; q++) star[q > c ? Math.Min(4, q + 1) : q] += d[q];
            d = star;
        }
        if (r.Steady)
        {
            d = new float[5];
            d[c] = 1;
        }
        return d;
    }

    public (Rarity Quality, float Chance)[] Odds(Category kind, Rarity chest, bool hidden = false)
    {
        var d = new float[5];
        foreach (var (q, p) in Categories.Odds(chest)) d[(int)q] += p;
        var r = Rules(kind, true, hidden, false, CoinPending, CloverPending);
        d = Turn(d, chest, r);
        return d.Select((p, q) => ((Rarity)q, p)).Where(o => o.p > 0).ToArray();
    }

    /// <summary>
    /// What luck did to a chest you've just bought, with the curios that did it and how many things each changed (the
    /// opening says so). Named things get the curios' and the restorers' luck; everything gets the Heartsongs'.
    /// </summary>
    List<(string Curio, int Changed)> Luck(Chest chest)
    {
        var changes = new List<(string, int)>();
        if (chest.Sealed) return changes;
        bool firstToday = Today.Bought == 1, firstThisWeek = CloverPending;
        if (firstThisWeek) CloverWeek = Week;
        var before = chest.Everything.ToDictionary(i => i.Id, i => i.Quality);
        var unwarded = chest.Unwarded?.Id;
        chest.Improve(i =>
        {
            if (i.Fake || i.Broken) return i;
            var r = Rules(i.Category, i.Named, i == chest.Hidden, i.Id == unwarded && i.Named, firstToday, firstThisWeek);
            return i with { Quality = Turn(i.Quality, chest.Quality, r) };
        });
        int Count(Func<Item, bool> which) => chest.Everything.Count(i => !i.Fake && !i.Broken && which(i) && i.Quality != before[i.Id]);
        // Which curio to thank: the first that could have done it (the opening names one, with how many things).
        if (HasCurio("crescent")) changes.Add(("crescent", Count(_ => true)));
        else
        {
            string named = new[] { "clover", "coin", "foot", "star", "horseshoe", "wheel", "rod", "wardkey" }.FirstOrDefault(HasCurio);
            int byNamed = Count(i => i.Named && i.Quality > before[i.Id]);
            if (named != null && byNamed > 0) changes.Add((named, byNamed));
            else if (RestorerLuck > 0 && Count(i => i.Quality > before[i.Id]) is var restored and > 0) changes.Add(("restorer", restored));
            if (HasCurio("token") && Count(i => i.Quality < before[i.Id]) is var swung and > 0) changes.Add(("token", swung));
            foreach (var key in new[] { "gauntlet", "cap" })
                if (HasCurio(key) && Count(i => i.Quality == chest.Quality && before[i.Id] < chest.Quality) is var mended and > 0) changes.Add((key, mended));
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
    /// What naming a thing puts on its price: the shop's name for its kind (an active synergy), enchantment's (the
    /// Arcanist, the Spellbinder's Ring), the trades' curios, a wishbone on a surprise, the tuning fork's streak, the
    /// Aether Jar, and a Finder's name on what was hidden.
    /// </summary>
    /// <param name="count">How many of its kind were named in the chest (a moneychanger's scales count purses).</param>
    float NamedBonus(Category kind, Rarity quality, bool magic, Rarity chest, int count, bool hidden)
    {
        float b = 1 + Synergies.SaleBonus(this, kind);
        if (magic) b *= (1 + Synergies.MagicSales(this)) * (HasCurio("ring") ? Curios.RingMagic / Categories.MagicMultiplier : 1);
        b *= kind switch
        {
            Category.Arms when HasCurio("whetstone") => Curios.WhetstoneBonus,
            Category.Garb when HasCurio("dummy") => Curios.TradeBonus,
            Category.Potions when HasCurio("still") => Curios.TradeBonus,
            Category.Scrolls when HasCurio("desk") => Curios.TradeBonus,
            Category.Coins when HasCurio("scales") => 1 + Curios.ScalesStep * count,
            _ => 1,
        };
        if (quality > chest && HasCurio("wishbone")) b *= Curios.WishboneBonus;
        if (HasCurio("fork")) b *= 1 + Math.Min(Curios.ForkMax, Curios.ForkStep * PerfectStreak);
        if (HasCurio("aether")) b *= 1 + Math.Min(Curios.AetherMax, Curios.AetherStep * MagicNamed);
        if (hidden) b *= 1 + Synergies.FinderSales(Active("finder"));
        if (HasCurio("lamps")) b *= 1 + Curios.LampsPerPoint * Math.Min(ExhibitionCap, Exhibition);
        return b;
    }

    public float Value(Category kind, Rarity quality, bool magic, Rarity chest, int count, bool hidden = false) =>
        Categories.Value(kind, quality, magic) * SaleMultiplier(kind) * (kind == Category.Heartsong ? 1 : NamedBonus(kind, quality, magic, chest, count, hidden)) *
        (HasCurio("ledger") ? Curios.LedgerRight : 1);

    /// <summary>
    /// What a thing nobody named fetches here on average: its worth at its chest's odds, with the shop's trades and the
    /// Heartsongs' luck (they reach everything), without anything naming would add. For a careful player's guess at what
    /// they couldn't hear.
    /// </summary>
    public float Unnamed(Category kind, Rarity chest)
    {
        var d = new float[5];
        foreach (var (q, p) in Categories.Odds(chest)) d[(int)q] += p;
        d = Turn(d, chest, Rules(kind, false, false, false, false, false));
        return d.Select((p, q) => p * Categories.Value(kind, (Rarity)q, false)).Sum() * SaleMultiplier(kind);
    }

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
    /// day put on everything (<see cref="SaleMultiplier"/>), and, out of a chest you bought (<paramref name="from"/>), what
    /// naming it put on it (<see cref="NamedBonus"/>) and a ledger's verdict on the read.
    /// </summary>
    public int SaleValue(Item item, PickGrade grade = PickGrade.Clean, Purchase from = null) =>
        Sale(item, grade, from?.Chest, item.Named, item.MagicNamed, from == null ? null : from.Perfect);

    float SaleRaw(Item item, PickGrade grade, Chest chest, bool named, bool magicNamed, bool? perfect)
    {
        float value = item.Broken ? BrokenValue(item) : item.Fake ? FakeValue(item) : item.Value;
        value *= GradeMultiplier(grade) * SaleMultiplier(item.Category);
        if (chest != null && named && !item.Fake && !item.Broken && !item.IsSong)
        {
            int count = chest.Everything.Count(i => i.Category == item.Category && i.Named);
            bool hidden = item == chest.Hidden || (chest.Unwarded != null && item.Id == chest.Unwarded.Id);
            value *= NamedBonus(item.Category, item.Quality, item.Magic && magicNamed, chest.Quality, Math.Max(1, count), hidden);
        }
        if (perfect is bool right && HasCurio("ledger")) value *= right ? Curios.LedgerRight : Curios.LedgerWrong;
        return value;
    }

    int Sale(Item item, PickGrade grade, Chest chest, bool named, bool magicNamed, bool? perfect) =>
        (int)MathF.Round(SaleRaw(item, grade, chest, named, magicNamed, perfect));

    /// <summary>
    /// What everything in a chest would have sold for here today, bought on this slip: a false bottom's and a warded
    /// thing's too, with what the slip would have named. For your gut, when you pass.
    /// </summary>
    public int Fetches(Chest chest, Appraisal slip = null)
    {
        if (slip == null) return chest.All.Sum(i => SaleValue(i));
        var (named, magic) = Naming(chest, slip);
        bool perfect = slip.Check(chest).Perfect;
        // A warded thing the slip names would have come out; one it doesn't stays in the ward, unsold.
        bool freed = chest.Warded != null && chest.Names(slip);
        float total = chest.Everything.Sum(i => SaleRaw(i, PickGrade.Clean, chest, named.Contains(i.Id), magic.Contains(i.Id), perfect));
        if (freed) total += SaleRaw(chest.Warded, PickGrade.Clean, chest, true, false, perfect);
        return (int)MathF.Round(total);
    }

    /// <summary>
    /// A perfect read's fees: a keen eye's (the synergy) and an appraiser's seal's, on what the chest held.
    /// </summary>
    public int PerfectFee(int held) => (int)MathF.Round(held * (Synergies.KeenFee(Active("keen")) + (HasCurio("seal") ? Curios.SealFee : 0)));

    // ---- The customers ----

    /// <summary>A customer is at the counter: the Waiting Game's patience, and the Closing Bell for the day's last.</summary>
    void Greet(Visit visit)
    {
        if (visit == null || visit.Lesson != null) return;
        visit.Chest.Unmasked = HasCurio("trumpet");
        if (HasCurio("hourglass") && PassStreak > 0)
        {
            float off = Math.Min(Curios.WaitMax, Curios.WaitStep * PassStreak);
            visit.Reduce(1 - off, $"Waiting Game: {Synergies.Pct(off)} off.");
        }
        if (HasCurio("bell") && Queue.Count == 0) visit.Reduce(Curios.BellPrice, "Closing Bell: the day's last, at half.");
    }

    /// <summary>
    /// A chest's been passed: the Waiting Game counts it, and with a queue ticket the seller comes back at the end of the
    /// day for a fifth less.
    /// </summary>
    void Passed(Visit visit)
    {
        PassStreak++;
        if (HasCurio("queue") && visit != null && visit.Lesson == null && !visit.Returned && visit.Outcome == VisitOutcome.Passed)
            Queue.Add(visit.Again(Curios.QueuePrice));
    }

    /// <summary>What a chest just bought did to the build's counters: the horseshoe's streak, surprises, finds, bargains.</summary>
    void Counted(Visit visit, Chest chest)
    {
        PassStreak = 0;
        MagicNamed += chest.Everything.Count(i => i.MagicNamed && i.Magic && !i.Fake);
        int surprised = chest.Everything.Count(i => i.Named && !i.Fake && !i.Broken && !i.IsSong && i.Quality > chest.Quality);
        Surprises += surprised;
        LuckStreak = surprised > 0 ? LuckStreak + 1 : 0;
        if (chest.Hidden is Item hidden && hidden.Named) HiddenFound++;
        if (chest.Unwarded != null) HiddenFound++;
        if (visit.FirstAsk > 0 && visit.PricePaid < visit.FirstAsk && visit.Trade != Trade.Fixed) Bargains++;
        else if (visit.Trade == Trade.Offer) Bargains++;
    }

    /// <summary>A haggler's purse: what a haggle talked a seller down by comes back again.</summary>
    public int Rebate(Visit visit) => HasCurio("coffer") && visit.Trade == Trade.Haggle && visit.FirstAsk > visit.PricePaid
        ? Money.Round((visit.FirstAsk - visit.PricePaid) * Curios.CofferShare) : 0;

    /// <summary>
    /// The day's crowd, as the build has it: with a sleeper's pillow a third of the sellers with nothing to tell have
    /// warded something and say what they dreamt; with counting beads, every chest with nothing else said comes with a
    /// tally of what's in it.
    /// </summary>
    void Curiosities()
    {
        for (int i = 0; i < Queue.Count; i++)
        {
            var visit = Queue[i];
            if (visit.Lesson != null || visit.Says != Clue.None || visit.Chest.Sealed) continue;
            if (HasCurio("pillow") && Rng.NextFloat() < Curios.PillowShare)
                Queue[i] = Events.Restate(Rng, visit, "dreaming", Tier, ward: true);
            else if (HasCurio("beads")) Queue[i] = Events.Restate(Rng, visit, "census", Tier);
        }
    }

    // ---- Nim ----

    /// <summary>What was in your purse when Nim arrived this morning: his prices are a share of it.</summary>
    public int PeddlerPurse { get; private set; }

    /// <summary>What Nim asks for a curio today (nothing, while the keepsake is still to give).</summary>
    public int CuriosPrice(CurioDef curio) => Keepsake ? 0 : Curios.Price(curio, PeddlerPurse, Tier);
}
