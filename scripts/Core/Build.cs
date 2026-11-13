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

    /// <summary>A chest with an enchanted thing in it has everything real in it enchanted (the archmage's staff).</summary>
    bool EnchantsAll => false;

    /// <summary>
    /// Something here shows an enchantment (a witchlight, a monocle, a moth): a glow could have been seen and marked.
    /// With no light, an enchanted thing counted as its kind is read right (0.37: it was "missed the glow", and no read
    /// of a chest with one in it could be perfect until the witchlight was bought).
    /// </summary>
    bool Lit => true;
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
        return new LuckRules(firstToday && HasCurio("coin"), shift, Math.Min(0.9f, step), HasCurio("token"),
            Mends(kind) || (unseen && HasCurio("blindfold")) || (firstToday && Keeps(Core.Keepsakes.Aldwin)),
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
        Synergies.LuckBonus(Active("fortune")) + (HasCurio("spindle") ? Math.Min(Curios.SpindleMax, Curios.SpindleStep * Surprises) : 0) +
        (Keeps(Core.Keepsakes.Hawthorn) ? Core.Keepsakes.HawthornLuck : 0) + (Keeps(Core.Keepsakes.Fork) ? Core.Keepsakes.ForkLuck : 0);

    /// <summary>
    /// Never worse than its chest promised: a smith's glove for weapons, a hatter's cap for cloaks, the Imp's Ember and the
    /// Master Restorer for all (0.30: a step better was the master's, but luck stops at purple, and the Emporium's chests
    /// are purple).
    /// </summary>
    bool Mends(Category kind) => (kind == Category.Arms && HasCurio("gauntlet")) || (kind == Category.Garb && HasCurio("cap")) || Shows(Heartsongs.Ember) ||
                                 Has("master");

    /// <summary>Luck's step: as far as purple, never into gold (<see cref="Categories.Lift"/>).</summary>
    static Rarity Up(Rarity q, int steps = 1) => Categories.Lift(q, steps);

    /// <summary>
    /// Whether luck's step would have taken a thing past purple: it can't make gold (no restorer makes a legend), so the
    /// thing comes out enchanted instead (0.30: a lucky purple thing shines). Without it, luck did nothing at the Emporium.
    /// </summary>
    static bool PastPurple(Rarity q, int steps) => steps > 0 && q < Rarity.Legendary && (int)q + steps > (int)Rarity.Epic;

    /// <summary>
    /// A thing's quality, turned by the rules (the base roll is the chest's own, made when it was packed), and whether
    /// luck lifted it past purple (<see cref="PastPurple"/>): then it comes out enchanted.
    /// </summary>
    (Rarity Quality, bool Shine, bool Swung) Turn(Rarity q, Rarity chest, LuckRules r)
    {
        bool shine = false, swung = false;
        Rarity Lucky(Rarity x, int steps = 1)
        {
            shine |= PastPurple(x, steps);
            return Up(x, steps);
        }
        if (r.TwoTries && Categories.RollQuality(Rng, chest) is var again && again > q) q = again;
        q = Lucky(q, r.Shift);
        if (r.Step > 0 && Rng.NextFloat() < r.Step) q = Lucky(q);
        // A gambler's token swings a thing that came out as promised, as a surprise would: up is luck's step (a purple
        // thing shines, 0.35: up from purple was nothing, so at the Emporium it only ever swung down), down is a step worse.
        if (r.Swing && q == chest && Rng.NextFloat() < Curios.TokenSwing(chest))
        {
            int steps = Categories.SwingStep(Rng);
            q = steps > 0 ? Lucky(q, steps) : Categories.Lower(q, -steps);
            swung = true;
        }
        if (r.Mend && q < chest) q = chest;
        if (r.Chimes && q == chest && q < Rarity.Epic && Rng.NextFloat() < Heartsongs.ChimesLuck) q = Up(q);
        return r.Steady ? (chest, false, false) : (q, shine, swung);
    }

    /// <summary>The same turning, on the odds of each quality, for the slip and the power index.</summary>
    static float[] Turn(float[] d, Rarity chest, LuckRules r) => Turn(d, chest, r, out _);

    /// <param name="shine">How much of it luck lifted past purple: the chance a thing comes out a purple that shines (enchanted).</param>
    static float[] Turn(float[] d, Rarity chest, LuckRules r, out float shine)
    {
        int c = (int)chest;
        float shone = 0;
        // Luck's shift: as far as purple, never into gold; what's gold stays gold, and what would have gone past purple shines.
        float[] Shifted(float[] x, int steps)
        {
            var y = new float[5];
            for (int q = 0; q < 5; q++) y[(int)Categories.Lift((Rarity)q, steps)] += x[q];
            return y;
        }
        float PastPurpleShare(float[] x, int steps) => Enumerable.Range(0, 5).Where(q => PastPurple((Rarity)q, steps)).Sum(q => x[q]);
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
        if (r.Shift > 0)
        {
            shone += PastPurpleShare(d, r.Shift);
            d = Shifted(d, r.Shift);
        }
        if (r.Step > 0)
        {
            // What's already shining stays so; of the rest at purple, a step's worth shines now.
            shone += (d[(int)Rarity.Epic] - shone) * r.Step;
            var up = Shifted(d, 1);
            d = d.Select((p, q) => p * (1 - r.Step) + up[q] * r.Step).ToArray();
        }
        if (r.Swing)
        {
            float moved = d[c] * Curios.TokenSwing(chest);
            d[c] -= moved;
            foreach (var (steps, p) in Categories.SwingSteps)
            {
                if (steps > 0 && PastPurple(chest, steps)) shone += moved * p;
                d[(int)(steps > 0 ? Categories.Lift(chest, steps) : Categories.Lower(chest, -steps))] += moved * p;
            }
        }
        if (r.Mend)
        {
            for (int q = 0; q < c; q++)
            {
                d[c] += d[q];
                d[q] = 0;
            }
        }
        if (r.Chimes && c < (int)Rarity.Epic)
        {
            float moved = d[c] * Heartsongs.ChimesLuck;
            d[c] -= moved;
            d[c + 1] += moved;
        }
        if (r.Steady)
        {
            d = new float[5];
            d[c] = 1;
            shone = 0;
        }
        shine = Math.Clamp(shone, 0, d[(int)Rarity.Epic]);
        return d;
    }

    /// <summary>
    /// Of the things here that come out purple in a chest of this colour, the share luck made shine (enchanted, worth
    /// double): for what a purple thing is worth on the slip and to the careful player.
    /// </summary>
    float ShineShare(Category kind, Rarity chest, bool hidden)
    {
        if (hidden) return 0;
        var d = Turn(Spread(chest), chest, Rules(kind, false, false, CoinPending, CloverPending, false), out float shine);
        return d[(int)Rarity.Epic] > 0 ? shine / d[(int)Rarity.Epic] : 0;
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
    /// A shooting star, once a day, on a chest just bought: after every other luck, a dry spell's too (0.35: it looked
    /// before the dry spell's lift, and missed what that made better). Returns the opening's line for it, if it fell.
    /// </summary>
    (string, int)? Star(Chest chest)
    {
        // A shooting star, once a day: the first thing to come out better than its chest climbs a step further; a purple
        // one shines (0.35: it only ever climbed things below purple, so in a blue or purple chest it never fell). No
        // seller prices a star: it's once a day, and nobody knows which day it'll be.
        if (HasCurio("star") && !HasCurio("crescent") && _starDay != Day &&
            chest.Everything.FirstOrDefault(i => !i.Fake && !i.Broken && !i.IsSong && i.Quality > chest.Quality && i.Quality < Rarity.Legendary &&
                                                 !(i.Quality == Rarity.Epic && i.Magic)) is Item fallen)
        {
            _starDay = Day;
            bool shines = fallen.Quality == Rarity.Epic;
            chest.Improve(i => i.Id != fallen.Id ? i : shines ? i with { Magic = true, Shone = true } : i with { Quality = Up(i.Quality) });
            if (shines) _shone.Add(fallen.Id);
            return ("star", 1);
        }
        return null;
    }

    /// <summary>Things in the chest just bought that luck made shine (purple, enchanted): surprises, as much as a step better.</summary>
    readonly HashSet<int> _shone = new();

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
        var magicBefore = chest.Everything.ToDictionary(i => i.Id, i => i.Magic);
        var unwarded = chest.Unwarded?.Id;
        var swung = new HashSet<int>();
        chest.Improve(i =>
        {
            if (i.Fake || i.Broken) return i;
            var r = Rules(i.Category, i == chest.Hidden, i.Id == unwarded, firstToday, firstThisWeek, unseen);
            var (quality, shine, turned) = Turn(i.Quality, chest.Quality, r);
            if (turned) swung.Add(i.Id);
            // Luck past purple makes a thing shine, read as the plain thing it was. What's hidden (under a false bottom, in
            // a ward) shines only with the curio that promises it a step (a hollow rod, a ward-key: 0.35, at purple they did
            // nothing); other luck stops at purple there, as 0.30 had it.
            bool hidden = i == chest.Hidden || i.Id == unwarded;
            bool own = (i == chest.Hidden && HasCurio("rod")) || (i.Id == unwarded && HasCurio("wardkey"));
            bool shone = shine && !i.IsSong && !i.Magic && (!hidden || own);
            return i with { Quality = quality, Magic = i.Magic || shone, Shone = i.Shone || shone };
        });
        _shone.Clear();
        _shone.UnionWith(chest.Everything.Where(i => i.Magic && !magicBefore[i.Id]).Select(i => i.Id));
        bool Better(Item i) => !i.Fake && !i.Broken && (i.Quality > before[i.Id] || (i.Magic && !magicBefore[i.Id]));
        // Which curio to thank for a thing luck made better: the one whose rule reached it (0.35: the opening thanked the
        // first luck curio you held for anything, so a clover seemed to work on every chest).
        string Thanked(Item i)
        {
            if (swung.Contains(i.Id)) return "token";
            if (i == chest.Hidden && HasCurio("rod")) return "rod";
            if (i.Id == unwarded && HasCurio("wardkey")) return "wardkey";
            if (firstThisWeek && HasCurio("clover")) return "clover";
            if (firstToday && HasCurio("coin")) return "coin";
            if (unseen && HasCurio("dip")) return "dip";
            if (i.Category == Category.Gems && HasCurio("wheel")) return "wheel";
            // A trade's glove or cap: its step (what it mended is said on its own line).
            if (i.Category == Category.Arms && HasCurio("gauntlet")) return "gauntlet+"; // loc: keys
            if (i.Category == Category.Garb && HasCurio("cap")) return "cap+"; // loc: keys
            if (HasCurio("horseshoe") && LuckStreak > 0) return "horseshoe";
            if (HasCurio("spindle") && Surprises > 0) return "spindle";
            if (HasCurio("foot")) return "foot";
            return RestorerLuck > 0 ? "restorer" : null;
        }
        // A thing mended: it came out as promised where it would have been worse.
        bool Mended(Item i) => !i.Fake && !i.Broken && i.Quality == chest.Quality && before[i.Id] < chest.Quality;
        if (HasCurio("crescent")) changes.Add(("crescent", chest.Everything.Count(i => !i.Fake && !i.Broken && (i.Quality != before[i.Id] || i.Magic != magicBefore[i.Id]))));
        else
        {
            foreach (var group in chest.Everything.Where(Better).GroupBy(Thanked).Where(g => g.Key != null && g.Key != "token"))
                changes.Add((group.Key, group.Count()));
            // A gambler's token, for what it swung either way.
            if (swung.Count > 0) changes.Add(("token", chest.Everything.Count(i => swung.Contains(i.Id) && (i.Quality != before[i.Id] || i.Magic != magicBefore[i.Id]))));
            // What a mender saved: a smith's glove the weapons, a hatter's cap the cloaks, a blindfold anything bought unseen.
            if (HasCurio("gauntlet") && chest.Everything.Count(i => i.Category == Category.Arms && Mended(i)) is var arms and > 0) changes.Add(("gauntlet", arms));
            if (HasCurio("cap") && chest.Everything.Count(i => i.Category == Category.Garb && Mended(i)) is var garb and > 0) changes.Add(("cap", garb));
            if (unseen && HasCurio("blindfold") && chest.Everything.Count(i => Mended(i) && !(i.Category == Category.Arms && HasCurio("gauntlet")) &&
                                                                              !(i.Category == Category.Garb && HasCurio("cap"))) is var blind and > 0)
                changes.Add(("blindfold", blind));
            // The Master Restorer's hands: nothing comes out worse than its chest.
            if (Has("master") && chest.Everything.Count(Mended) is var restored and > 0) changes.Add(("restorer", restored));
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
    /// <param name="shone">Luck made it shine past purple: a surprise too (0.35: the wishbone passed it over).</param>
    float General(Category kind, Rarity quality, Rarity chest, int count, bool shone = false) =>
        Certain(kind, count) * ((quality > chest || shone) && HasCurio("wishbone") ? Curios.WishboneBonus : 1);

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
        // Keepsakes (0.32): a tale's thanks, which no seller knows of.
        if (Keeps(Core.Keepsakes.Weights) && kind is Category.Coins or Category.Gems) b *= Core.Keepsakes.WeightsBonus;
        if (Keeps(Core.Keepsakes.Medal) && kind == Category.Arms) b *= Core.Keepsakes.MedalBonus;
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

    public bool EnchantsAll => HasCurio("staff");

    /// <summary>A light to see a glow by (<see cref="ISlipModel.Lit"/>).</summary>
    public bool Lit => Witchlight || MarksMagic || ReadsChests;

    public float Value(Category kind, Rarity quality, bool magic, Rarity chest, int count, bool hidden = false) =>
        Categories.Value(kind, quality, magic) * SaleMultiplier(kind) * Read(true) *
        (kind == Category.Heartsong ? 1 : General(kind, quality, chest, count) * Special(magic, hidden, false)) *
        // A purple thing luck might make shine: worth double then, and a wishbone's half again on top if it wasn't a surprise already.
        (quality == Rarity.Epic && !magic && kind != Category.Heartsong
            ? 1 + ShineShare(kind, chest, hidden) * (HasCurio("wishbone") && quality <= chest ? 2 * Curios.WishboneBonus - 1 : 1) : 1);

    /// <summary>
    /// What a thing of this kind usually fetches here in a chest of this colour, luck and the shop's name and all: for a
    /// careful player's guess at what they couldn't hear.
    /// </summary>
    public float Expected(Category kind, Rarity chest)
    {
        var turned = Turn(Spread(chest), chest, Rules(kind, false, false, false, false, false), out float shine);
        return (turned.Select((p, q) => p * Categories.Value(kind, (Rarity)q, false) * General(kind, (Rarity)q, chest, 1)).Sum() +
                shine * Categories.Value(kind, Rarity.Epic, false) * General(kind, Rarity.Epic, chest, 1, shone: true)) * SaleMultiplier(kind);
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
    /// day put on everything (<see cref="SaleMultiplier"/>), what the shop's name and build put on it, and out of a chest
    /// you bought (<paramref name="from"/>), what your read of it earned.
    /// </summary>
    public int SaleValue(Item item, PickGrade grade = PickGrade.Clean, Purchase from = null) =>
        (int)MathF.Round(SaleRaw(item, grade, from?.Chest, from?.Verdict));

    /// <param name="warded">Whether it's the chest's warded thing, named (by default: the one the ward gave up).</param>
    float SaleRaw(Item item, PickGrade grade, Chest chest, bool? perfect, bool? warded = null)
    {
        float value = item.Broken ? BrokenValue(item) : item.Fake ? FakeValue(item) : item.Value;
        value *= GradeMultiplier(grade) * SaleMultiplier(item.Category);
        int count = chest == null ? 1 : Math.Max(1, chest.Everything.Count(i => i.Category == item.Category && !i.Fake && !i.Broken));
        if (!item.Fake && !item.Broken && !item.IsSong)
        {
            bool hidden = chest != null && item == chest.Hidden;
            bool ward = warded ?? (chest?.Unwarded != null && item.Id == chest.Unwarded.Id);
            value *= General(item.Category, item.Quality, chest?.Quality ?? item.Quality, count, item.Shone) * Special(item.Magic, hidden, ward);
        }
        // Broken glass a still makes good, sand a desk sells as ink, paste the loupe or the Maw's buyers take: priced as the
        // real thing is here (0.35: what the shop puts on its kind, its trade's curio, the lamps, skipped them, so a still's
        // broken potion fetched a third of a whole one, not half).
        else if ((item.Broken && BrokenValue(item) > 0) || (item.Fake && FakeValue(item) != item.Value)) value *= Certain(item.Category, count);
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
    /// perfect read so far this run, this one included, on what the chest held. Paid before the read is recorded.
    /// </summary>
    public int PerfectFee(int held) => (int)MathF.Round(held * PerfectShare);

    /// <summary>What a perfect read's fees come to, as a share of what the chest held (0.35: the register didn't count the read being paid for, so the first paid nothing).</summary>
    public float PerfectShare => Synergies.KeenFee(Active("keen")) + (HasCurio("seal") ? Curios.SealFee : 0) +
                                 (HasCurio("register") ? Math.Min(Curios.RegisterMax, Curios.RegisterStep * (PerfectReads + 1)) : 0);

    // ---- The customers ----

    /// <summary>
    /// A customer is at the counter. What the build takes off their price (the Waiting Game, a wholesaler's tally, a
    /// haggler's knot, the Closing Bell for the day's last) and, with a sealed bid or a blindfold, what they'd take for
    /// it sight unseen (until you lay a hand on it).
    /// </summary>
    void Greet(Visit visit)
    {
        // A seller's first visit (a lesson) too (0.35: curios rested while Bram taught, which read as curios not working).
        if (visit == null || visit.Talk != null) return;
        visit.Chest.Unmasked = HasCurio("trumpet");
        if (HasCurio("hourglass") && PassStreak > 0)
        {
            float off = Math.Min(Curios.WaitMax, Curios.WaitStep * PassStreak);
            visit.Reduce(1 - off, Loc.T($"Waiting Game: {Synergies.Pct(off)} off."));
        }
        if (HasCurio("wholesale") && Today.Bought > 0)
        {
            float off = Math.Min(Curios.WholesaleMax, Curios.WholesaleStep * Today.Bought);
            visit.Reduce(1 - off, Loc.T($"Wholesaler's Tally: {Synergies.Pct(off)} off."));
        }
        if (HasCurio("knot") && Bargains > 0)
        {
            float off = Math.Min(Curios.KnotMax, Curios.KnotStep * Bargains);
            visit.Reduce(1 - off, Loc.T($"Haggler's Knot: {Synergies.Pct(off)} off."));
        }
        // The day's last seller: nobody left with a chest (someone may still come in to talk).
        if (HasCurio("bell") && Queue.All(v => v.Talk != null)) visit.Reduce(Curios.BellPrice, Loc.T($"Closing Bell: the day's last, {Synergies.Pct(1 - Curios.BellPrice)} off."));
        if (UnseenPrice < 1) visit.Seal(UnseenPrice, Loc.T($"Sight unseen: {Synergies.Pct(1 - UnseenPrice)} off while you keep your hands off it."));
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
        int surprised = chest.Everything.Count(i => !i.Fake && !i.Broken && !i.IsSong && (i.Quality > chest.Quality || _shone.Contains(i.Id)));
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
        // Talked down: an offer brought the price below what was asked when you made it (a curio's discount, taken at
        // the asking price, isn't talking anyone down).
        if (visit.OfferedAt > 0 && visit.PricePaid < visit.OfferedAt) Bargains++;
        else if (visit.Trade == Trade.Offer && visit.OffersMade > 0) Bargains++;
    }

    /// <summary>A haggler's purse: what a haggle talked a seller down by comes back again.</summary>
    public int Rebate(Visit visit) => HasCurio("coffer") && visit.Trade == Trade.Haggle && visit.OfferedAt > visit.PricePaid
        ? Money.Round((visit.OfferedAt - visit.PricePaid) * Curios.CofferShare) : 0;

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
        float Worth(LuckRules r)
        {
            var turned = Turn(spread, colour, r, out float shine);
            return turned.Select((p, q) => p * Categories.Value(kind, (Rarity)q, false) * General(kind, (Rarity)q, colour, 1)).Sum() +
                   shine * Categories.Value(kind, Rarity.Epic, false) * General(kind, Rarity.Epic, colour, 1, shone: true);
        }
        float plain = spread.Select((p, q) => p * Categories.Value(kind, (Rarity)q, false)).Sum();
        float sure = plain * Certain(kind, 1);
        // A cursed crescent's steadiness is no seller's business: they ask its five in a hundred less, no more (0.35).
        var always = Rules(kind, false, false, false, false, false) with { Steady = false };
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
        if (DuesTonight && !PaidThisWeek && left < DueNow) return Loc.T($"The Guild comes tonight for {DueNow}g, and you'd have {left}g left.");
        int keep = ReserveIf(upgrade, curio);
        if (left >= keep) return null;
        bool moving = upgrade != null && upgrade.SetsTier > Tier;
        float rise = (float)keep / Math.Max(1, WorkingPurse) - 1;
        string dearer = moving || rise < 0.05f ? "" : Loc.T($" (sellers will ask about {Synergies.Pct(rise)} more once they hear of it)");
        return Loc.T($"After the {upgrade?.Name ?? curio?.Name}, two typical chests {(moving ? Loc.T($"at {ShopTiers.Name(upgrade.SetsTier)}") : Loc.T("here"))} would cost about {keep}g{dearer}, and you'd have {left}g. You may not be able to afford chests for a while.");
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

    /// <summary>What was in your purse when Nim came by tonight: part of his price is a share of it.</summary>
    public int PeddlerPurse { get; private set; }

    /// <summary>The shop Nim last came to: on his first visit to a grander one he brings something for a way of trading that opens there.</summary>
    public int PeddlerTier { get; private set; }

    /// <summary>What Nim asks for a curio today (nothing, while the keepsake is still to give).</summary>
    public int CuriosPrice(CurioDef curio) => Money.Round(Curios.Price(curio, PeddlerPurse) * (Under(Challenges.DearCurios) ? Challenges.DearCurioPrices : 1));
}
