using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Keywright.Core;

namespace CurioTest;

/// <summary>
/// Audit B (tools/curiotest/AUDIT_B.md): the perfect read's payers, false bottoms, wards, luck, crowds and display. Each
/// check runs what a card says against the game's Core rules: a twin with and without the curio, the same chest, and
/// the difference the card names. A failing check is a curio that doesn't do what its card says (the audit has the
/// scenario, the evidence and a fix).
/// </summary>
public static class ChecksB
{
    const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    // ---- Tools ----

    /// <summary>Sets something only the rules set (a dry spell, a streak, today's tally, the day).</summary>
    static void Set(GameState s, string property, object value) => typeof(GameState).GetProperty(property, Any)!.SetValue(s, value);

    static object Call(GameState s, string method, params object[] args) => typeof(GameState).GetMethod(method, Any)!.Invoke(s, args);

    /// <summary>The chance a thing of this kind comes out a step better than its chest (GameState.StepChance: the luck curios' sum).</summary>
    static float StepChance(GameState s, Category kind = Category.Coins) => (float)Call(s, "StepChance", kind);

    /// <summary>
    /// A new run at this shop, as T.Shop makes one (bare: no tools), with today's event fixed ("none": no event) so the
    /// day's crowd is the same for twins, and these curios. Day 1 has started.
    /// </summary>
    static GameState Build(int tier, string[] curios, bool bare = false, int seed = 7, string forcedEvent = "none", string character = Characters.Kit)
    {
        var s = GameState.NewRun(seed, character, guided: false);
        if (tier > 1 && !bare) s.SkipTo(tier, 100000);
        else
        {
            if (tier > 1) Set(s, "Tier", tier);
            s.Gold = 100000;
        }
        foreach (string c in curios) s.GiveCurio(c);
        s.ForcedEvent = forcedEvent;
        s.StartDay();
        return s;
    }

    /// <summary>A shop at this tier with the tools a player would own by then.</summary>
    static GameState Shop(int tier, params string[] curios) => Build(tier, curios);

    /// <summary>A shop at this tier with no tools at all (no restorers' luck, no buyers' trades): the cleanest twin.</summary>
    static GameState Bare(int tier, params string[] curios) => Build(tier, curios, bare: true);

    /// <summary>The next day (the night in between, as the game closes a day).</summary>
    static void NextDay(GameState s)
    {
        s.EndDay();
        s.StartDay();
    }

    static Item I(Category kind, Rarity quality, bool magic = false, bool fake = false) => T.I(kind, quality, magic, fake);

    /// <summary>A plain fixed-price visit by a seller with no rule of her own (Ada), for a crafted chest.</summary>
    static Visit Plain(Chest chest, int ask = 1) => new(Sellers.Get("ada"), chest, Array.Empty<Category>(), Trade.Fixed, ask, ask, "");

    /// <summary>Buys a crafted chest as the shop does (the luck at purchase, the dry spell, the build's counters).</summary>
    static Purchase Buy(GameState s, Chest chest, Appraisal slip = null)
    {
        var v = Plain(chest);
        v.Buy(new Random(1));
        return s.Buy(v, slip ?? T.Slip(s, chest));
    }

    /// <summary>A chest of this many coins, all as good as its colour (any better is luck's doing).</summary>
    static Chest Coins(Rarity colour, int count = 4) => T.Chest(colour, Enumerable.Range(0, count).Select(_ => I(Category.Coins, colour)).ToArray());

    /// <summary>Things in it that came out better than its colour (a step up, or a purple one luck made shine).</summary>
    static int Better(Chest chest) => chest.Everything.Count(i => !i.Fake && !i.Broken && (i.Quality > chest.Quality || i.Shone));

    /// <summary>
    /// Buys chests of plain things at their colour (each on a dry spell of 0, so no pity lift) and returns the share of
    /// things that came out better than their chest. <paramref name="before"/> runs before each purchase.
    /// </summary>
    static float LiftedShare(GameState s, Rarity colour, int chests, Action before = null, int per = 4)
    {
        int better = 0;
        for (int i = 0; i < chests; i++)
        {
            Set(s, "DrySpell", 0);
            before?.Invoke();
            var chest = Coins(colour, per);
            Buy(s, chest);
            better += Better(chest);
        }
        return (float)better / (chests * per);
    }

    /// <summary>
    /// Buys this many chests (each on a dry spell of 0) and counts those the opening's luck lines thank this curio for
    /// (<see cref="Purchase.Luck"/>, shown by OpeningPhase as "Lucky Coin: 2 things came out better!"), and those luck
    /// changed at all.
    /// </summary>
    static (int Credited, int Lifted) Credits(GameState s, string curio, Func<Chest> make, int chests = 300)
    {
        int credited = 0, lifted = 0;
        for (int i = 0; i < chests; i++)
        {
            Set(s, "DrySpell", 0);
            var chest = make();
            var purchase = Buy(s, chest);
            if (purchase.Improved > 0) lifted++;
            if (purchase.Luck.Any(l => l.Curio == curio)) credited++;
        }
        return (credited, lifted);
    }

    /// <summary>A day's customers with a chest (not someone come to talk), for each of this many days from today.</summary>
    static IEnumerable<Visit> Days(GameState s, int days) => DaysWithEvents(s, days).Select(d => d.Visit);

    /// <summary>The same, with the day's event (null: none).</summary>
    static IEnumerable<(Visit Visit, string Event)> DaysWithEvents(GameState s, int days)
    {
        for (int d = 0; d < days; d++)
        {
            if (d > 0) NextDay(s);
            foreach (var v in s.Queue.Where(v => v.Talk == null).ToList()) yield return (v, s.TodayEvent?.Key);
        }
    }

    /// <summary>Why a visit is out of the ordinary, for a check's message (with the day's event, if it restated the seller's clue).</summary>
    static string Why(Visit v, string day = null) =>
        v.Beat != null ? $"tale:{v.Beat}" : v.Gift ? $"gift:{v.Seller.Key}" : v.Chest.Sealed ? "stranger" : v.Chest.Breathing ? "mimic"
        : v.Chest.Items.Count == 0 ? $"empty:{v.Seller.Key}" : v.Seller.SlipPrice ? "aldo (king)"
        : day is "dreaming" or "wardens" or "census" && v.Says != v.Seller.States ? $"{day} day" : v.Says != Clue.None && !v.PointsAtWard ? $"says {v.Says}:{v.Seller.Key}"
        : v.Seller.Key;

    static string Tally(IEnumerable<string> reasons) =>
        string.Join(", ", reasons.GroupBy(r => r).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} x{g.Count()}"));

    /// <summary>
    /// A share or a chance within a relative tolerance (T.Near's half-coin slack is for gold: it would let any share
    /// between 0 and 1 pass).
    /// </summary>
    static void Close(double expected, double actual, string what, double tolerance)
    {
        if (Math.Abs(expected - actual) > tolerance * Math.Abs(expected) + 1e-5)
            throw new Failed($"{what}: expected about {expected:0.####}, got {actual:0.####}");
    }

    // ---- The checks ----

    public static IEnumerable<Check> Checks()
    {
        // ======== The perfect read ========

        // A Thornwick chest with a thing under a false bottom and one in a ward: "everything in the chest" is all of it.
        static Chest ReadChest()
        {
            var chest = new Chest(new[] { I(Category.Coins, Rarity.Uncommon), I(Category.Arms, Rarity.Uncommon), I(Category.Garb, Rarity.Uncommon) },
                Rarity.Uncommon, hidden: I(Category.Coins, Rarity.Uncommon));
            chest.Ward(I(Category.Potions, Rarity.Uncommon));
            return chest;
        }

        static (Opened Without, Opened With) ReadTwins(string curio, int tier, Func<GameState, Chest, Appraisal> slip, string other = null)
        {
            var without = other == null ? Bare(tier) : Bare(tier, other);
            var with = other == null ? Bare(tier, curio) : Bare(tier, other, curio);
            var a = ReadChest();
            var b = ReadChest();
            return (T.BuyAndOpen(without, T.Visit(without, "ada", a, ask: 5, trade: Trade.Fixed), slip(without, a)),
                T.BuyAndOpen(with, T.Visit(with, "ada", b, ask: 5, trade: Trade.Fixed), slip(with, b)));
        }

        yield return new("ledger", "a perfect read: everything in the chest (false bottom and warded thing too) sells for 15% more", () =>
        {
            var (plain, read) = ReadTwins("ledger", 2, T.Exact);
            T.True(read.Report.Perfect, "precondition: the exact slip is a perfect read");
            T.Near(plain.Value * Curios.LedgerRight, read.Value, "what the chest brought back");
        });

        yield return new("ledger", "a wrong slip (one thing missed): everything sells for 15% less", () =>
        {
            var (plain, read) = ReadTwins("ledger", 2, (s, c) => T.Slip(s, c, (Category.Coins, 1), (Category.Arms, 1)));
            T.True(!read.Report.Perfect, "precondition: the slip missed the cloak");
            T.Near(plain.Value * Curios.LedgerWrong, read.Value, "what the chest brought back");
        });

        yield return new("ledger", "a wrong slip (a thing marked that isn't there): everything sells for 15% less", () =>
        {
            var (plain, read) = ReadTwins("ledger", 2, (s, c) =>
            {
                var slip = T.Exact(s, c);
                slip.Set(Category.Scrolls, 1);
                return slip;
            });
            // (A slip that names something that isn't there keeps the ward shut, so the warded potion is a ghost too.)
            T.True(read.Report.Ghosts.Contains(Category.Scrolls), "precondition: a scroll marked that isn't there");
            T.Near(plain.Value * Curios.LedgerWrong, read.Value, "what the chest brought back");
        });

        yield return new("ledger", "a chest bought sight unseen on a blank slip is no 'wrong slip' (the opening marks it unseen, not missed): no 15% off", () =>
        {
            var (plain, blind) = ReadTwins("ledger", 2, (s, c) => T.Slip(s, c));
            T.True(blind.Purchase.SightUnseen && blind.Purchase.Slip.Blank, "precondition: bought sight unseen on a blank slip");
            T.Near(plain.Value, blind.Value, "what a blank-slip chest brought back (the card names only a perfect read and a wrong slip)");
        });

        yield return new("truthglass", "a perfect read: everything in the chest sells for half as much again", () =>
        {
            var (plain, read) = ReadTwins("truthglass", 3, T.Exact);
            T.True(read.Report.Perfect, "precondition: perfect read");
            T.Near(plain.Value * Curios.TruthglassBonus, read.Value, "what the chest brought back");
        });

        yield return new("truthglass", "a slip that isn't perfect: no bonus, and nothing taken off", () =>
        {
            var (plain, read) = ReadTwins("truthglass", 3, (s, c) => T.Slip(s, c, (Category.Coins, 1)));
            T.Near(plain.Value, read.Value, "what the chest brought back");
        });

        yield return new("truthglass", "with a Ledger of Certainty too, a perfect read pays both (x1.5 x1.15)", () =>
        {
            var (plain, read) = ReadTwins("truthglass", 3, T.Exact, other: "ledger");
            // The twin without the truthglass has the ledger: the truthglass adds its half on top of the ledger's 15%.
            T.Near(plain.Value * Curios.TruthglassBonus, read.Value, "what the chest brought back");
        });

        // ======== False bottoms ========

        yield return new("crate", "glass never breaks when you drop a chest (without it, about half does)", () =>
        {
            var (without, with) = (Bare(2), Bare(2, "crate"));
            var rng = new Random(3);
            int brokeWith = 0, brokeWithout = 0;
            for (int i = 0; i < 300; i++)
            {
                brokeWith += T.Chest(Rarity.Uncommon, I(Category.Potions, Rarity.Uncommon), I(Category.Potions, Rarity.Common)).Drop(rng, with.DropSturdier).Broke.Count;
                brokeWithout += T.Chest(Rarity.Uncommon, I(Category.Potions, Rarity.Uncommon), I(Category.Potions, Rarity.Common)).Drop(rng, without.DropSturdier).Broke.Count;
            }
            T.Eq(0, brokeWith, "glass broken in 300 drops with the crate");
            T.True(brokeWithout > 200, $"without the crate, glass breaks (broke {brokeWithout} of 600)");
        });

        yield return new("crate", "the drop's sturdiness is total (what the counter and the sim both read when a chest is dropped)", () =>
        {
            var with = Bare(3, "crate");
            T.Eq(1f, with.DropSturdier, "the drop's sturdiness with the crate");
        });

        yield return new("hook", "the chance of a false bottom is doubled for every seller who can have one (the careful player's number)", () =>
        {
            foreach (int tier in new[] { 2, 3, 4 })
            {
                var (without, with) = (Bare(tier), Bare(tier, "hook"));
                foreach (var seller in Sellers.All.Where(s => s.Tier <= tier && s.HiddenChance > 0 && s.HiddenChance <= 0.5f))
                    Close(2 * without.HiddenChance(seller), with.HiddenChance(seller), $"{seller.Key}'s chance of a false bottom at tier {tier}", 0.001);
            }
        });

        yield return new("hook", "false bottoms turn up twice as often in the chests sellers bring", () =>
        {
            var (without, with) = (Bare(2), Bare(2, "hook"));
            var seller = Sellers.Get("ada");
            int n = 6000;
            float rate(GameState s, int seed)
            {
                var rng = new Random(seed);
                return (float)Enumerable.Range(0, n).Count(_ => Market.CreateVisit(rng, seller, 2, s.Boosts).Chest.Hidden != null) / n;
            }
            float plain = rate(without, 11), hooked = rate(with, 12);
            Close(2 * plain, hooked, $"Ada's false bottoms (without the hook {plain:P1})", 0.15);
        });

        yield return new("rod", "what's under a false bottom comes out a step better (green chest: green -> blue; the rest unchanged)", () =>
        {
            foreach (var (tier, colour) in new[] { (2, Rarity.Uncommon), (3, Rarity.Rare) })
            {
                var with = Bare(tier, "rod");
                var chest = new Chest(new[] { I(Category.Arms, colour) }, colour, hidden: I(Category.Coins, colour));
                Buy(with, chest);
                T.Eq(colour + 1, chest.Hidden.Quality, $"the hidden coin in a {colour} chest");
                T.Eq(colour, chest.Items[0].Quality, "the weapon on top");
            }
        });

        yield return new("rod", "the opening thanks the rod only for what was under a false bottom (not for a restorer's luck on a plain chest)", () =>
        {
            var with = Shop(3, "rod");
            T.True(with.RestorerLuck > 0, "precondition: the shop has a restorer");
            var (credited, lifted) = Credits(with, "rod", () => Coins(Rarity.Rare));
            T.True(lifted > 30, $"precondition: the restorer lifted things ({lifted} chests)");
            T.Eq(0, credited, $"chests with no false bottom whose opening said \"Hollow Rod: ... came out better!\" (of {lifted} the restorer lifted)");
        });

        yield return new("rod", "the slip's odds for a false bottom are a step better too (the odds bar is honest about it)", () =>
        {
            var (without, with) = (Bare(2), Bare(2, "rod"));
            float AtLeast(GameState s, Rarity q) => s.Odds(Category.Coins, Rarity.Uncommon, hidden: true).Where(o => o.Quality >= q).Sum(o => o.Chance);
            Close(AtLeast(without, Rarity.Uncommon), AtLeast(with, Rarity.Rare), "chance a hidden coin in a green chest is at least a step up", 0.001);
        });

        yield return new("rod", "at the Emporium (purple chests) what's under a false bottom still comes out a step better", () =>
        {
            var (without, with) = (Bare(4), Bare(4, "rod"));
            var a = new Chest(new[] { I(Category.Arms, Rarity.Epic) }, Rarity.Epic, hidden: I(Category.Gems, Rarity.Epic));
            var b = new Chest(new[] { I(Category.Arms, Rarity.Epic) }, Rarity.Epic, hidden: I(Category.Gems, Rarity.Epic));
            var plain = T.BuyAndOpen(without, T.Visit(without, "ada", a, ask: 5, trade: Trade.Fixed), T.Exact(without, a));
            var rodded = T.BuyAndOpen(with, T.Visit(with, "ada", b, ask: 5, trade: Trade.Fixed), T.Exact(with, b));
            // A step past purple can't be gold; the glossary says luck makes it shine instead, but a hidden thing never shines.
            T.True(b.Hidden.Quality > Rarity.Epic || b.Hidden.Magic || rodded.Value > plain.Value,
                $"the hidden purple gem came out {b.Hidden.Quality}{(b.Hidden.Magic ? ", enchanted" : "")}, worth {rodded.Value} against {plain.Value} without the rod: no step at all");
        });

        yield return new("log", "each false bottom bought this run makes false bottoms 2 points likelier, up to 20", () =>
        {
            var with = Bare(2, "log");
            var ada = Sellers.Get("ada");
            float start = with.HiddenChance(ada);
            for (int n = 1; n <= 14; n++)
            {
                var chest = new Chest(new[] { I(Category.Arms, Rarity.Uncommon) }, Rarity.Uncommon, hidden: I(Category.Coins, Rarity.Uncommon));
                var slip = T.Exact(with, chest);
                Buy(with, chest, slip);
                T.Eq(n, with.BottomsOpened, "false bottoms counted");
                Close(start + Math.Min(Curios.LogMax, Curios.LogStep * n), with.HiddenChance(ada), $"Ada's chance of a false bottom after {n}", 0.001);
            }
        });

        yield return new("log", "a false bottom left off the slip still comes out (sold) and counts as opened", () =>
        {
            var with = Bare(2, "log");
            var chest = new Chest(new[] { I(Category.Arms, Rarity.Uncommon) }, Rarity.Uncommon, hidden: I(Category.Coins, Rarity.Uncommon));
            var opened = T.BuyAndOpen(with, T.Visit(with, "ada", chest, ask: 5, trade: Trade.Fixed), T.Slip(with, chest, (Category.Arms, 1)));
            T.Eq(1, with.BottomsOpened, "false bottoms counted");
            T.True(opened.Value > with.SaleValue(chest.Items[0], PickGrade.Clean, opened.Purchase), "the hidden coin was sold with the weapon");
        });

        yield return new("pass", "anything from under a false bottom sells for double (the rest as before), at Thornwick and further in", () =>
        {
            foreach (int tier in new[] { 2, 4 })
            {
                var (without, with) = (Bare(tier), Bare(tier, "pass"));
                Chest Make() => new(new[] { I(Category.Arms, Rarity.Uncommon) }, Rarity.Uncommon, hidden: I(Category.Coins, Rarity.Uncommon));
                var a = Make();
                var b = Make();
                var pa = Buy(without, a, T.Exact(without, a));
                var pb = Buy(with, b, T.Exact(with, b));
                T.Near(2 * without.SaleValue(a.Hidden, PickGrade.Clean, pa), with.SaleValue(b.Hidden, PickGrade.Clean, pb), $"the hidden coin at tier {tier}", 0.03);
                T.Eq(without.SaleValue(a.Items[0], PickGrade.Clean, pa), with.SaleValue(b.Items[0], PickGrade.Clean, pb), "the weapon on top");
            }
        });

        yield return new("pass", "the slip values a false bottom at double too", () =>
        {
            var (without, with) = (Bare(2), Bare(2, "pass"));
            var chest = new Chest(new[] { I(Category.Arms, Rarity.Uncommon) }, Rarity.Uncommon, hidden: I(Category.Coins, Rarity.Uncommon));
            var a = T.Exact(without, chest);
            var b = T.Exact(with, chest);
            T.Near(2 * a.HiddenValue, b.HiddenValue, "the slip's false bottom", 0.03);
        });

        yield return new("pass", "a thing from under a false bottom kept on the shelf, sold later, still fetches double", () =>
        {
            var with = Bare(2, "pass");
            var chest = new Chest(new[] { I(Category.Arms, Rarity.Uncommon) }, Rarity.Uncommon, hidden: I(Category.Coins, Rarity.Uncommon));
            var purchase = Buy(with, chest, T.Exact(with, chest));
            var coin = chest.Hidden;
            int atOpening = with.SaleValue(coin, PickGrade.Clean, purchase);
            T.True(with.Keep(coin, atOpening), "precondition: kept on display");
            int gold = with.Gold;
            with.SellFromDisplay(coin);
            T.Near(atOpening, with.Gold - gold, $"sold from the shelf (the opening priced it at {atOpening}g)");
        });

        yield return new("compass", "every ordinary seller's chest has a false bottom (the careful player's chance is 1)", () =>
        {
            foreach (var seller in Sellers.All.Where(s => s.Key != "pip"))
            {
                int tier = Math.Max(3, seller.Tier);
                var with = Bare(tier, "compass");
                T.Eq(1f, with.HiddenChance(seller), $"{seller.Key}'s chance of a false bottom");
                var rng = new Random(5);
                for (int i = 0; i < 60; i++)
                    T.True(Market.CreateVisit(rng, seller, tier, with.Boosts).Chest.Hidden != null, $"{seller.Key}'s chest #{i} has no false bottom");
            }
        });

        yield return new("compass", "every chest has a false bottom: Pip's empty ones too", () =>
        {
            var with = Bare(3, "compass");
            var rng = new Random(5);
            var visits = Enumerable.Range(0, 300).Select(_ => Market.CreateVisit(rng, Sellers.Get("pip"), 3, with.Boosts)).ToList();
            int none = visits.Count(v => v.Chest.Hidden == null);
            T.Eq(0, none, $"Pip's chests without a false bottom (of 300; {visits.Count(v => v.Chest.Items.Count == 0)} empty)");
        });

        yield return new("compass", "every chest a day brings has a false bottom (60 days at Crownhold and the Emporium)", () =>
        {
            var missing = new List<string>();
            int total = 0;
            foreach (int tier in new[] { 3, 4 })
            {
                var with = Build(tier, new[] { "compass" }, forcedEvent: null, seed: 11 + tier);
                foreach (var (v, day) in DaysWithEvents(with, 60))
                {
                    total++;
                    if (v.Chest.Hidden == null) missing.Add(Why(v, day));
                }
            }
            T.True(missing.Count == 0, $"{missing.Count} of {total} chests had no false bottom: {Tally(missing)}");
        });

        // ======== Wards ========

        string[] signs = { "sorrel", "hesper", "ilse", "isaura", "quire", "skrae", "zell" };

        yield return new("wardlamp", "every sign seller's chest wards something, and their sign names it (without it, about half)", () =>
        {
            foreach (string key in signs)
            {
                var seller = Sellers.Get(key);
                var (without, with) = (Bare(seller.Tier), Bare(seller.Tier, "wardlamp"));
                var rng = new Random(9);
                int plain = 0;
                for (int i = 0; i < 200; i++)
                {
                    var v = Market.CreateVisit(rng, seller, seller.Tier, with.Boosts);
                    T.True(v.Chest.Warded != null, $"{key}'s chest #{i} has nothing warded");
                    T.True(v.PointsAtWard && v.ClueKinds.Contains(v.Chest.Warded.Category), $"{key}'s sign doesn't name the warded {v.Chest.Warded.Category}");
                    if (Market.CreateVisit(rng, seller, seller.Tier, without.Boosts).Chest.Warded != null) plain++;
                }
                T.True(plain is > 60 and < 140, $"without the lantern {key} wards about half the time ({plain} of 200)");
            }
        });

        yield return new("wardlamp", "on the Sleepers' Moon every seller's dream points at a warded thing", () =>
        {
            var with = Build(2, new[] { "wardlamp" }, bare: true, forcedEvent: "dreaming");
            int dreams = 0, unwarded = 0;
            foreach (var v in Days(with, 12).Where(v => v.Says == Clue.Dream))
            {
                dreams++;
                if (v.Chest.Warded == null) unwarded++;
            }
            T.True(dreams > 30, $"precondition: the moon brings dreams ({dreams})");
            T.Eq(0, unwarded, $"dreams (of {dreams}) that pointed at the dearest thing, not a warded one");
        });

        yield return new("pillow", "a fifth of the sellers with nothing else to tell you dream true (a warded thing and its sign)", () =>
        {
            int dreamers = 0, quiet = 0;
            foreach (int tier in new[] { 2, 3, 4 })
            {
                var with = Build(tier, new[] { "pillow" }, seed: 20 + tier);
                // On days with no event (an event day gives everyone something to tell you).
                foreach (var (v, day) in DaysWithEvents(with, 80).Where(d => d.Event == null))
                {
                    if (v.Lesson != null || v.Seller.States != Clue.None || v.Seller.SlipPrice || v.Chest.Sealed || v.Gift) continue;
                    quiet++;
                    if (v.Says == Clue.Dream) dreamers++;
                }
            }
            Close(Curios.PillowShare, (float)dreamers / quiet, $"share of the {quiet} sellers with nothing else to tell you who dreamt ({dreamers})", 0.25);
        });

        yield return new("pillow", "every seller who tells you a pillow dream has warded something, and the dream names it", () =>
        {
            int dreams = 0;
            foreach (int tier in new[] { 2, 3, 4 })
            {
                var with = Build(tier, new[] { "pillow" }, seed: 30 + tier);
                foreach (var v in Days(with, 60).Where(v => v.Says == Clue.Dream && v.Seller.States != Clue.Dream))
                {
                    dreams++;
                    T.True(v.Chest.Warded != null, $"{v.Seller.Key}'s pillow dream at tier {tier} points at nothing warded");
                    T.True(v.ClueKinds.SequenceEqual(new[] { v.Chest.Warded.Category }), $"{v.Seller.Key}'s dream names {string.Join("/", v.ClueKinds)}, the ward holds {v.Chest.Warded.Category}");
                }
            }
            T.True(dreams > 40, $"precondition: pillow dreams happened ({dreams})");
        });

        yield return new("pillow", "a seller who dreams for you keeps the terms they give a friend (Nettle charges a friend of the clan nothing for shaking)", () =>
        {
            var with = Bare(2, "pillow");
            with.Flags.Add(TaleBook.GoblinFriend);
            for (int i = 0; i < 200; i++)
            {
                with.Queue.Clear();
                with.Queue.Add(T.NewVisit(with, TaleBook.Nettle));
                T.True(with.Queue[0].Waived, "precondition: a friend's visit has the shaking fee waived");
                Call(with, "Curiosities");
                if (with.Queue[0].Says != Clue.Dream) continue;
                T.True(with.Queue[0].Waived, "Nettle dreamt for you and now charges for shaking again (the waiver was lost)");
                return;
            }
            throw new Failed("precondition: Nettle never dreamt in 200 tries");
        });

        yield return new("wardtally", "each warded thing named this run makes warded things sell for 10% more (the first counts itself)", () =>
        {
            var (without, with) = (Bare(2), Bare(2, "wardtally"));
            for (int n = 1; n <= 3; n++)
            {
                Chest Make()
                {
                    var c = new Chest(new[] { I(Category.Arms, Rarity.Uncommon) }, Rarity.Uncommon, hidden: I(Category.Coins, Rarity.Uncommon));
                    c.Ward(I(Category.Potions, Rarity.Uncommon));
                    return c;
                }
                var a = Make();
                var b = Make();
                var pa = Buy(without, a, T.Exact(without, a));
                var pb = Buy(with, b, T.Exact(with, b));
                T.Eq(n, with.WardsNamed, "warded things named");
                T.Near(without.SaleValue(a.Unwarded, PickGrade.Clean, pa) * (1 + Curios.WardTallyStep * n), with.SaleValue(b.Unwarded, PickGrade.Clean, pb),
                    $"the warded potion after {n} named", 0.03);
                T.Eq(without.SaleValue(a.Hidden, PickGrade.Clean, pa), with.SaleValue(b.Hidden, PickGrade.Clean, pb), "the false bottom's coin (not warded)");
                T.Eq(without.SaleValue(a.Items.First(i => i.Category == Category.Arms), PickGrade.Clean, pa),
                    with.SaleValue(b.Items.First(i => i.Category == Category.Arms), PickGrade.Clean, pb), "the weapon");
            }
        });

        yield return new("wardkey", "a warded thing you name comes out a step better (green -> blue; blue -> purple)", () =>
        {
            foreach (var (tier, colour) in new[] { (2, Rarity.Uncommon), (3, Rarity.Rare) })
            {
                var with = Bare(tier, "wardkey");
                var chest = new Chest(new[] { I(Category.Arms, colour) }, colour);
                chest.Ward(I(Category.Coins, colour));
                Buy(with, chest, T.Exact(with, chest));
                T.True(chest.Unwarded != null, "precondition: the ward gave it up");
                T.Eq(colour + 1, chest.Unwarded.Quality, $"the named coin in a {colour} chest");
                T.Eq(colour, chest.Items.First(i => i.Category == Category.Arms).Quality, "the weapon");
            }
        });

        yield return new("wardkey", "the opening thanks the ward-key only for a warded thing named (not for a restorer's luck on a chest with no ward)", () =>
        {
            var with = Shop(3, "wardkey");
            T.True(with.RestorerLuck > 0, "precondition: the shop has a restorer");
            var (credited, lifted) = Credits(with, "wardkey", () => Coins(Rarity.Rare));
            T.True(lifted > 30, $"precondition: the restorer lifted things ({lifted} chests)");
            T.Eq(0, credited, $"chests with no ward whose opening said \"Ward-Key: ... came out better!\" (of {lifted} the restorer lifted)");
        });

        yield return new("wardkey", "at the Emporium (purple chests) a warded thing you name still comes out a step better", () =>
        {
            var (without, with) = (Bare(4), Bare(4, "wardkey"));
            Chest Make()
            {
                var c = new Chest(new[] { I(Category.Arms, Rarity.Epic) }, Rarity.Epic);
                c.Ward(I(Category.Gems, Rarity.Epic));
                return c;
            }
            var a = Make();
            var b = Make();
            var plain = T.BuyAndOpen(without, T.Visit(without, "ada", a, ask: 5, trade: Trade.Fixed), T.Exact(without, a));
            var keyed = T.BuyAndOpen(with, T.Visit(with, "ada", b, ask: 5, trade: Trade.Fixed), T.Exact(with, b));
            T.True(b.Unwarded.Quality > Rarity.Epic || b.Unwarded.Magic || keyed.Value > plain.Value,
                $"the named purple gem came out {b.Unwarded.Quality}, worth {keyed.Value} against {plain.Value} without the key: no step at all");
        });

        yield return new("aldricseal", "every seller with nothing else to tell you has warded a thing and gives its sign (60 days at Crownhold and the Emporium, tales' chests too)", () =>
        {
            var missing = new List<string>();
            int total = 0;
            foreach (int tier in new[] { 3, 4 })
            {
                var with = Build(tier, new[] { "aldricseal" }, forcedEvent: null, seed: 40 + tier);
                foreach (var (v, day) in DaysWithEvents(with, 60))
                {
                    // Someone with something of their own to tell you (a ticket, a scale, the King's game, the day's event)
                    // tells you that; a gift, the Stranger's sealed chest and a lesson's are their own.
                    if (v.Seller.States is Clue.Articles or Clue.Pounds || v.Seller.SlipPrice || v.Chest.Sealed || v.Gift || v.Lesson != null ||
                        day is "census" or "wardens") continue;
                    total++;
                    if (v.Chest.Warded == null || !v.PointsAtWard) missing.Add(Why(v, day));
                }
            }
            T.True(missing.Count == 0, $"{missing.Count} of {total} chests had no warded thing with a sign: {Tally(missing)}");
        });

        yield return new("aldricseal", "a seller who wards for you keeps the terms they give a friend (Nettle charges a friend of the clan nothing for shaking)", () =>
        {
            var with = Bare(3, "aldricseal");
            with.Flags.Add(TaleBook.GoblinFriend);
            with.Queue.Clear();
            with.Queue.Add(T.NewVisit(with, TaleBook.Nettle));
            T.True(with.Queue[0].Waived, "precondition: a friend's visit has the shaking fee waived");
            Call(with, "Curiosities");
            T.True(with.Queue[0].Chest.Warded != null, "precondition: the seal warded something");
            T.True(with.Queue[0].Waived, "Nettle now charges for shaking again (the waiver was lost when the seal gave him a sign)");
        });

        // ======== Luck ========

        yield return new("shell", "a dry spell's luck starts after 5 plain finds, not 10 (3% more for each thing since)", () =>
        {
            var (without, with) = (Bare(1), Bare(1, "shell"));
            foreach (int dry in new[] { 0, 5, 6, 8, 10, 11, 20, 40 })
            {
                Set(without, "DrySpell", dry);
                Set(with, "DrySpell", dry);
                Close(Math.Clamp((dry - Curios.ShellPityAfter) * GameState.PityStep, 0, GameState.PityMax), with.PityChance, $"with the shell, after {dry} plain things", 0.001);
                Close(Math.Clamp((dry - GameState.PityAfter) * GameState.PityStep, 0, GameState.PityMax), without.PityChance, $"without it, after {dry} plain things", 0.001);
            }
        });

        yield return new("shell", "after 8 plain finds, a chest bought has something lifted about 9% of the time (without the shell, never)", () =>
        {
            var (without, with) = (Bare(1), Bare(1, "shell"));
            float Lifted(GameState s)
            {
                int lifted = 0;
                for (int i = 0; i < 3000; i++)
                {
                    Set(s, "DrySpell", 8);
                    var chest = Coins(Rarity.Common, 2);
                    Buy(s, chest);
                    if (Better(chest) > 0) lifted++;
                }
                return lifted / 3000f;
            }
            T.Eq(0f, Lifted(without), "chests with a lift, without the shell");
            Close(3 * GameState.PityStep, Lifted(with), "chests with a lift, with the shell", 0.15);
        });

        yield return new("coin", "the first chest each day: each thing rolls twice and keeps the better (about 7% of things in a common chest beat it)", () =>
        {
            var (without, with) = (Bare(1), Bare(1, "coin"));
            void NewDay(GameState s) => Set(s, "Today", new DayStats());
            float plain = LiftedShare(without, Rarity.Common, 1500, () => NewDay(without));
            float lucky = LiftedShare(with, Rarity.Common, 1500, () => NewDay(with));
            T.Eq(0f, plain, "things better than a common chest, without the coin");
            float twice = Categories.Odds(Rarity.Common).Where(o => o.Quality > Rarity.Common).Sum(o => o.Chance);
            Close(twice, lucky, "things better than a common chest, first chest of the day", 0.12);
        });

        yield return new("coin", "only the first chest each day: the second that day gets nothing", () =>
        {
            var with = Bare(1, "coin");
            int better = 0;
            for (int i = 0; i < 800; i++)
            {
                Set(with, "Today", new DayStats());
                Set(with, "DrySpell", 0);
                Buy(with, Coins(Rarity.Common));
                Set(with, "DrySpell", 0);
                var second = Coins(Rarity.Common);
                Buy(with, second);
                better += Better(second);
            }
            T.Eq(0, better, "things lifted in the day's second chest");
        });

        yield return new("coin", "the opening thanks the coin only for the day's first chest (not for a restorer's luck on the day's others)", () =>
        {
            var with = Shop(3, "coin");
            T.True(with.RestorerLuck > 0, "precondition: the shop has a restorer");
            Buy(with, Coins(Rarity.Rare));
            var (credited, lifted) = Credits(with, "coin", () => Coins(Rarity.Rare));
            T.True(lifted > 30, $"precondition: the restorer lifted things ({lifted} chests)");
            T.Eq(0, credited, $"later chests the same day whose opening said \"Lucky Coin: ... came out better!\" (of {lifted} the restorer lifted)");
        });

        yield return new("coin", "keeps the better roll: nothing comes out worse than it went in (a rare chest, 600 days)", () =>
        {
            var with = Bare(3, "coin");
            for (int i = 0; i < 600; i++)
            {
                Set(with, "Today", new DayStats());
                var chest = Coins(Rarity.Rare);
                Buy(with, chest);
                T.True(chest.Everything.All(t => t.Quality >= Rarity.Rare), "a thing came out worse after its second roll");
            }
        });

        yield return new("foot", "everything you open gets a 10% chance to come out a step better", () =>
        {
            var (without, with) = (Bare(1), Bare(1, "foot"));
            Close(Curios.FootLuck, StepChance(with) - StepChance(without), "the step chance it adds", 0.001);
            T.Eq(0f, LiftedShare(without, Rarity.Common, 1500), "things lifted without the foot");
            Close(Curios.FootLuck, LiftedShare(with, Rarity.Common, 1500), "things lifted with the foot", 0.12);
        });

        yield return new("foot", "at the Emporium, a purple thing it lifts comes out enchanted (luck never makes gold)", () =>
        {
            var with = Bare(4, "foot");
            int shone = 0, things = 0;
            for (int i = 0; i < 800; i++)
            {
                Set(with, "DrySpell", 0);
                var chest = Coins(Rarity.Epic);
                Buy(with, chest);
                things += 4;
                shone += chest.Everything.Count(t => t.Shone && t.Magic && t.Quality == Rarity.Epic);
                T.True(chest.Everything.All(t => t.Quality <= Rarity.Epic), "luck made something gold");
            }
            Close(Curios.FootLuck, (float)shone / things, "purple things that shone", 0.15);
        });

        yield return new("wishbone", "something that comes out better than its chest sells for half as much again (at its colour: no change)", () =>
        {
            var (without, with) = (Bare(1), Bare(1, "wishbone"));
            Chest Make() => T.Chest(Rarity.Common, I(Category.Coins, Rarity.Uncommon), I(Category.Arms, Rarity.Common));
            var a = Make();
            var b = Make();
            var pa = Buy(without, a);
            var pb = Buy(with, b);
            T.Near(1.5 * without.SaleValue(a.Items[0], PickGrade.Clean, pa), with.SaleValue(b.Items[0], PickGrade.Clean, pb), "the green coin from a common chest", 0.03);
            T.Eq(without.SaleValue(a.Items[1], PickGrade.Clean, pa), with.SaleValue(b.Items[1], PickGrade.Clean, pb), "the common weapon");
        });

        yield return new("wishbone", "the slip counts it: a thing better than its chest is worth half again on the slip's odds", () =>
        {
            var (without, with) = (Bare(1), Bare(1, "wishbone"));
            T.Near(1.5 * without.Value(Category.Coins, Rarity.Uncommon, false, Rarity.Common, 1), with.Value(Category.Coins, Rarity.Uncommon, false, Rarity.Common, 1),
                "a green coin from a common chest, on the slip", 0.01);
        });

        yield return new("wishbone", "a purple thing luck made shine (a surprise, counted as one) sells for half as much again", () =>
        {
            var with = Bare(4, "wishbone", "foot");
            var without = Bare(4, "foot");
            for (int i = 0; i < 400; i++)
            {
                Set(with, "DrySpell", 0);
                var chest = Coins(Rarity.Epic);
                int before = with.Surprises;
                var purchase = Buy(with, chest);
                if (chest.Everything.FirstOrDefault(t => t.Shone) is not Item shone) continue;
                T.True(with.Surprises > before, "precondition: the shop counted it as a surprise");
                // The same thing, sold by a shop with no wishbone.
                var twin = Coins(Rarity.Epic);
                var p2 = Buy(without, twin);
                float plain = without.SaleValue(shone, PickGrade.Clean, p2);
                T.Near(1.5 * plain, with.SaleValue(shone, PickGrade.Clean, purchase), "the shining purple coin");
                return;
            }
            throw new Failed("precondition: nothing shone in 400 purple chests");
        });

        yield return new("wishbone", "a surprise kept on the shelf, sold later, still fetches half as much again", () =>
        {
            var with = Bare(2, "wishbone");
            var chest = T.Chest(Rarity.Common, I(Category.Coins, Rarity.Uncommon));
            var purchase = Buy(with, chest);
            var coin = chest.Items[0];
            int atOpening = with.SaleValue(coin, PickGrade.Clean, purchase);
            T.True(with.Keep(coin, atOpening), "precondition: kept on display");
            int gold = with.Gold;
            with.SellFromDisplay(coin);
            T.Near(atOpening, with.Gold - gold, $"sold from the shelf (the opening priced it at {atOpening}g)");
        });

        yield return new("horseshoe", "each chest in a row with a surprise adds 10 points to the next one's step chance, up to 50", () =>
        {
            var (without, with) = (Bare(2), Bare(2, "horseshoe"));
            foreach (int streak in new[] { 0, 1, 3, 5, 6, 9 })
            {
                Set(with, "LuckStreak", streak);
                Close(Math.Min(Curios.HorseshoeMax, Curios.HorseshoeStep * streak), StepChance(with) - StepChance(without), $"after {streak} chests in a row", 0.001);
            }
        });

        yield return new("horseshoe", "the streak counts chests in a row with a surprise, and a chest without one ends it", () =>
        {
            var with = Bare(2, "horseshoe");
            Buy(with, T.Chest(Rarity.Uncommon, I(Category.Coins, Rarity.Rare)));
            T.Eq(1, with.LuckStreak, "after one chest with a surprise");
            Buy(with, T.Chest(Rarity.Uncommon, I(Category.Arms, Rarity.Rare)));
            T.Eq(2, with.LuckStreak, "after two");
            Buy(with, T.Chest(Rarity.Uncommon));
            T.Eq(0, with.LuckStreak, "after a chest with nothing better in it");
        });

        yield return new("horseshoe", "the opening thanks the horseshoe only with a streak going (not for a restorer's luck after a chest with no surprise)", () =>
        {
            var with = Shop(3, "horseshoe");
            T.True(with.RestorerLuck > 0, "precondition: the shop has a restorer");
            var (credited, lifted) = Credits(with, "horseshoe", () =>
            {
                Set(with, "LuckStreak", 0);
                return Coins(Rarity.Rare);
            });
            T.True(lifted > 30, $"precondition: the restorer lifted things ({lifted} chests)");
            T.Eq(0, credited, $"chests bought with no streak whose opening said \"Horseshoe: ... came out better!\" (of {lifted} the restorer lifted)");
        });

        yield return new("horseshoe", "three in a row: the next chest's things come out a step better about 30% of the time", () =>
        {
            var with = Bare(2, "horseshoe");
            Close(0.3, LiftedShare(with, Rarity.Common, 1500, () => Set(with, "LuckStreak", 3)), "things lifted", 0.1);
        });

        yield return new("clover", "the first chest each week: everything in it a step better (false bottom and warded thing too); the second, nothing", () =>
        {
            var with = Bare(2, "clover");
            Chest Make()
            {
                var c = new Chest(new[] { I(Category.Coins, Rarity.Uncommon), I(Category.Arms, Rarity.Uncommon) }, Rarity.Uncommon, hidden: I(Category.Scrolls, Rarity.Uncommon));
                c.Ward(I(Category.Garb, Rarity.Uncommon));
                return c;
            }
            var first = Make();
            Buy(with, first, T.Exact(with, first));
            T.True(first.Everything.All(t => t.Quality == Rarity.Rare), $"the week's first chest: {string.Join(", ", first.Everything.Select(t => $"{t.Category} {t.Quality}"))}");
            var second = Make();
            Buy(with, second, T.Exact(with, second));
            T.True(second.Everything.All(t => t.Quality == Rarity.Uncommon), "the week's second chest was lifted too");
            // The next week's first chest (day 6).
            while (with.DayOfWeek != 1 || with.Day == 1) NextDay(with);
            var next = Make();
            Buy(with, next, T.Exact(with, next));
            T.True(next.Everything.All(t => t.Quality == Rarity.Rare), "the next week's first chest wasn't lifted");
        });

        yield return new("clover", "the opening thanks the clover only for the week's first chest (not for a restorer's luck on the rest)", () =>
        {
            var with = Shop(3, "clover");
            T.True(with.RestorerLuck > 0, "precondition: the shop has a restorer");
            Buy(with, Coins(Rarity.Rare));
            var (credited, lifted) = Credits(with, "clover", () => Coins(Rarity.Rare));
            T.True(lifted > 30, $"precondition: the restorer lifted things ({lifted} chests)");
            T.Eq(0, credited, $"later chests the same week whose opening said \"Four-Leaf Clover: ... came out better!\" (of {lifted} the restorer lifted)");
        });

        yield return new("clover", "a purple chest's things come out enchanted instead (luck never makes gold)", () =>
        {
            var with = Bare(4, "clover");
            var chest = Coins(Rarity.Epic, 3);
            Buy(with, chest);
            T.True(chest.Items.All(t => t.Quality == Rarity.Epic && t.Magic && t.Shone), "the week's first purple chest's coins didn't shine");
        });

        yield return new("star", "once a day, the first thing better than its chest climbs a step further (green chest: rare -> purple)", () =>
        {
            var with = Bare(2, "star");
            var chest = T.Chest(Rarity.Uncommon, I(Category.Coins, Rarity.Rare), I(Category.Arms, Rarity.Uncommon));
            var purchase = Buy(with, chest);
            T.Eq(Rarity.Epic, chest.Items.First(t => t.Category == Category.Coins).Quality, "the rare coin");
            T.True(purchase.Luck.Contains(("star", 1)), "the opening names the star");
            var again = T.Chest(Rarity.Uncommon, I(Category.Coins, Rarity.Rare));
            Buy(with, again);
            T.Eq(Rarity.Rare, again.Items[0].Quality, "a second chest the same day");
            NextDay(with);
            var tomorrow = T.Chest(Rarity.Uncommon, I(Category.Coins, Rarity.Rare));
            Buy(with, tomorrow);
            T.Eq(Rarity.Epic, tomorrow.Items[0].Quality, "the next day's first");
        });

        yield return new("star", "in a blue chest, a purple thing (better than its chest) climbs a step further: it shines", () =>
        {
            var with = Bare(3, "star");
            var chest = T.Chest(Rarity.Rare, I(Category.Coins, Rarity.Epic), I(Category.Arms, Rarity.Rare));
            var purchase = Buy(with, chest);
            var coin = chest.Items.First(t => t.Category == Category.Coins);
            T.True(coin.Magic || purchase.Luck.Any(l => l.Curio == "star"), $"the purple coin from a blue chest came out {coin.Quality}{(coin.Magic ? ", enchanted" : ", plain")}: the star did nothing");
        });

        foreach (int tier in new[] { 3, 4 })
            yield return new("star", $"it falls on the day's first surprise at {ShopTiers.Name(tier)} too (400 days of its crowd's chests, the shop's restorers and a rabbit's foot)", () =>
            {
                var with = Shop(tier, "star", "foot");
                var rng = new Random(4);
                // The crowd as the shop draws it (each day's first chest a new day, so the star is always still to fall).
                var crowd = Sellers.All.Where(s => Market.Weight(s, tier) > 0 && !s.SlipPrice && s.Gaze <= 0 && s.Steals <= 0).ToArray();
                var surprised = new int[5];
                var fell = new int[5];
                for (int day = 0; day < 400; day++)
                {
                    Set(with, "Day", with.Day + 1);
                    Set(with, "Today", new DayStats());
                    var v = Market.CreateVisit(rng, rng.PickWeighted(crowd, s => Market.Weight(s, tier)), tier, with.Boosts);
                    if (v.Chest.Stolen || v.Chest.Items.Count == 0) continue;
                    v.Chest.RemoveSong();
                    var purchase = Buy(with, v.Chest, T.Exact(with, v.Chest));
                    if (Better(v.Chest) == 0) continue;
                    // Nothing climbs past a shining purple thing (the card: a purple thing shines; luck never makes gold).
                    bool fellHere = purchase.Luck.Any(l => l.Curio == "star");
                    if (!fellHere && !v.Chest.Everything.Any(i => !i.Fake && !i.Broken && !i.IsSong && i.Quality > v.Chest.Quality &&
                                                                  i.Quality < Rarity.Legendary && !(i.Quality == Rarity.Epic && i.Magic))) continue;
                    surprised[(int)v.Chest.Quality]++;
                    if (fellHere) fell[(int)v.Chest.Quality]++;
                }
                string Row(Rarity c) => $"{Categories.ColourName(c)} {fell[(int)c]} of {surprised[(int)c]}";
                T.True(surprised.Sum() > 30, $"precondition: chests with a surprise ({surprised.Sum()})");
                T.True(fell.Sum() == surprised.Sum(),
                    $"the day's first chest had a surprise {surprised.Sum()} times; the star fell {fell.Sum()} times (by colour: {Row(Rarity.Common)}, {Row(Rarity.Uncommon)}, {Row(Rarity.Rare)}, {Row(Rarity.Epic)})");
            });

        yield return new("spindle", "each surprise this run adds 2 points to every thing's step chance, up to 50", () =>
        {
            var (without, with) = (Bare(3), Bare(3, "spindle"));
            foreach (int surprises in new[] { 0, 1, 10, 24, 25, 60 })
            {
                Set(with, "Surprises", surprises);
                Close(Math.Min(Curios.SpindleMax, Curios.SpindleStep * surprises), StepChance(with) - StepChance(without), $"after {surprises} surprises", 0.001);
            }
        });

        yield return new("spindle", "surprises are counted: two things better than their chest add two", () =>
        {
            var with = Bare(3, "spindle");
            Buy(with, T.Chest(Rarity.Uncommon, I(Category.Coins, Rarity.Rare), I(Category.Arms, Rarity.Rare), I(Category.Garb, Rarity.Uncommon)));
            T.Eq(2, with.Surprises, "surprises this run");
        });

        yield return new("spindle", "after 10 surprises, things come out a step better about 20% of the time", () =>
        {
            var with = Bare(3, "spindle");
            Close(0.2, LiftedShare(with, Rarity.Common, 1500, () => Set(with, "Surprises", 10)), "things lifted", 0.1);
        });

        // ======== Crowds ========

        static int Customers(GameState s) => s.Queue.Count(v => v.Talk == null);

        yield return new("lantern", "one more customer every day (10 days)", () =>
        {
            var (without, with) = (Build(1, Array.Empty<string>(), seed: 3), Build(1, new[] { "lantern" }, seed: 3));
            T.Eq(without.CustomersPerDay + Curios.LanternCustomers, with.CustomersPerDay, "customers a day");
            for (int d = 1; d <= 10; d++)
            {
                T.Eq(Customers(without) + 1, Customers(with), $"chests at the counter on day {d}");
                NextDay(without);
                NextDay(with);
            }
        });

        yield return new("horn", "two more customers every day (10 days at Thornwick)", () =>
        {
            var (without, with) = (Build(2, Array.Empty<string>(), seed: 3), Build(2, new[] { "horn" }, seed: 3));
            T.Eq(without.CustomersPerDay + Curios.HornCustomers, with.CustomersPerDay, "customers a day");
            for (int d = 1; d <= 10; d++)
            {
                T.Eq(Customers(without) + 2, Customers(with), $"chests at the counter on day {d}");
                NextDay(without);
                NextDay(with);
            }
        });

        yield return new("horn", "everyone asks 5% more (the shop's price factor, and a seller's ask for the same chest)", () =>
        {
            var (without, with) = (Shop(3), Shop(3, "horn"));
            Close(without.PriceFactor * Curios.HornDearer, with.PriceFactor, "what sellers ask of the shop", 0.001);
            // Big chests, so asks rounded to 5g (Money.Round, over 30g) stay within a percent.
            foreach (string key in new[] { "ada", "orme", "durgan", "rowan" })
            {
                Chest Make() => T.Chest(Rarity.Epic, I(Category.Coins, Rarity.Epic), I(Category.Coins, Rarity.Epic), I(Category.Gems, Rarity.Epic),
                    I(Category.Gems, Rarity.Epic), I(Category.Arms, Rarity.Epic), I(Category.Garb, Rarity.Epic));
                var a = Market.Price(new Random(8), Sellers.Get(key), Make(), 3, boosts: without.Boosts);
                var b = Market.Price(new Random(8), Sellers.Get(key), Make(), 3, boosts: with.Boosts);
                // Both asks are rounded to 5g (Money.Round over 30g): up to 2.5g off each, so 5.5g between them at most.
                T.True(a.Price >= 120, $"precondition: {key}'s price is big enough for 5% to show ({a.Price}g)");
                T.True(Math.Abs(b.Price - a.Price * Curios.HornDearer) <= 5.5, $"{key}'s price: {a.Price}g without the horn, {b.Price}g with it (5% more is {a.Price * Curios.HornDearer:0.#}g)");
            }
        });

        yield return new("horn", "on average over Thornwick's crowd, asks are 5% dearer (each ask rounds to whole coins, or 5g over 30g)", () =>
        {
            var (without, with) = (Shop(2), Shop(2, "horn"));
            var crowd = Sellers.All.Where(s => Market.Weight(s, 2) > 0 && !s.SlipPrice && s.Gaze <= 0).ToArray();
            double plain = 0, dearer = 0;
            for (int i = 0; i < 2000; i++)
            {
                var seller = crowd[i % crowd.Length];
                var a = Market.CreateVisit(new Random(i), seller, 2, without.Boosts);
                var b = Market.CreateVisit(new Random(i), seller, 2, with.Boosts);
                plain += a.Price;
                dearer += b.Price;
            }
            Close(Curios.HornDearer, dearer / plain, "what the crowd asks, all told, against without the horn", 0.01);
        });

        yield return new("queue", "the first chest passed each day comes back at the end of the day asking 20% less (once a day)", () =>
        {
            var with = Bare(1, "queue");
            var chest = T.Chest(Rarity.Common, I(Category.Coins, Rarity.Common));
            var v = T.Counter(with, T.Visit(with, "ada", chest, ask: 20, trade: Trade.Fixed), behind: 2);
            v.Pass(new Random(1));
            with.Pass();
            var back = with.Queue.Last();
            T.True(back.Returned && back.Chest == chest, "the passed chest is back, last in the queue");
            T.Eq(16, back.Ask, "its ask (20 less 20%)");
            // A second pass the same day: no second ticket.
            with.NextCustomer();
            with.Current.Pass(new Random(1));
            with.Pass();
            T.Eq(1, with.Queue.Count(q => q.Returned), "chests back today");
            // The next day it works again.
            NextDay(with);
            var other = T.Counter(with, T.Visit(with, "ada", T.Chest(Rarity.Common, I(Category.Arms, Rarity.Common)), ask: 10, trade: Trade.Fixed));
            other.Pass(new Random(1));
            with.Pass();
            T.True(with.Queue.Last().Returned && with.Queue.Last().Ask == 8, "the next day's first passed chest is back at 8g");
        });

        yield return new("queue", "a seller who walks out isn't 'a chest you pass': the ticket waits for the first one you do pass", () =>
        {
            var with = Bare(1, "queue");
            var v = T.Counter(with, T.Visit(with, "ada", T.Chest(Rarity.Common, I(Category.Coins, Rarity.Common)), ask: 20, trade: Trade.Fixed));
            v.TimeUp(new Random(1));
            with.Pass();
            T.True(!with.Queue.Any(q => q.Returned), "a walkout came back");
            with.NextCustomer();
            with.Current.Pass(new Random(1));
            with.Pass();
            T.Eq(1, with.Queue.Count(q => q.Returned), "the first chest passed is back");
        });

        yield return new("queue", "a haggler comes back asking 20% under their first ask (and settles for 20% less)", () =>
        {
            var with = Bare(2, "queue");
            var v = T.Counter(with, T.Visit(with, "garrick", Coins(Rarity.Rare, 3), trade: Trade.Haggle));
            int ask = v.FirstAsk, floor = v.Floor;
            v.Pass(new Random(1));
            with.Pass();
            var back = with.Queue.Last();
            T.Near(ask * Curios.QueuePrice, back.Ask, $"the ask (first {ask}g; asks over 30g round to 5g)", 0.06);
            T.Near(floor * Curios.QueuePrice, back.Floor, "the least they'd take", 0.06);
        });

        yield return new("guestbook", "the day's first customer brings a chest a colour finer (Thornwick, 40 mornings)", () =>
        {
            for (int seed = 1; seed <= 40; seed++)
            {
                var without = Build(2, Array.Empty<string>(), seed: seed);
                var with = Build(2, new[] { "guestbook" }, seed: seed);
                var a = without.Queue.First(v => v.Talk == null);
                var b = with.Queue.First(v => v.Talk == null);
                T.Eq(a.Seller.Key, b.Seller.Key, "precondition: the same first customer");
                T.Eq(a.Chest.Quality + 1, b.Chest.Quality, $"{b.Seller.Key}'s chest on morning {seed}");
            }
        });

        yield return new("guestbook", "the day's first customer brings a chest a colour finer, as far as purple, at the Emporium too (40 mornings)", () =>
        {
            int failed = 0;
            for (int seed = 1; seed <= 40; seed++)
            {
                var without = Build(4, Array.Empty<string>(), seed: seed);
                var with = Build(4, new[] { "guestbook" }, seed: seed);
                var a = without.Queue.First(v => v.Talk == null);
                var b = with.Queue.First(v => v.Talk == null);
                // The card says as far as purple: a purple chest stays purple, anything else is a colour finer.
                var expected = (Rarity)Math.Min((int)Rarity.Epic, (int)a.Chest.Quality + 1);
                if (b.Chest.Quality != expected) failed++;
            }
            T.Eq(0, failed, "mornings (of 40) the first chest wasn't a colour finer (as far as purple)");
        });

        yield return new("guestbook", "on a stormy day (every chest a colour finer) the first customer's is finer still", () =>
        {
            var without = Build(2, Array.Empty<string>(), seed: 5, forcedEvent: "storm");
            var with = Build(2, new[] { "guestbook" }, seed: 5, forcedEvent: "storm");
            var a = without.Queue.First(v => v.Talk == null);
            var b = with.Queue.First(v => v.Talk == null);
            T.True(b.Chest.Quality > a.Chest.Quality, $"the first chest was {b.Chest.Quality} with the guest book and {a.Chest.Quality} without: the storm's colour, no finer");
        });

        yield return new("guestbook", "on a week's first morning with an apprentice (Pip brings his chest first) the first customer is still the better sort", () =>
        {
            var with = Build(2, new[] { "guestbook" }, seed: 5);
            with.Flags.Add(TaleBook.Apprentice);
            while (with.DayOfWeek != 1 || with.Day == 1) NextDay(with);
            var first = with.Queue.First(v => v.Talk == null);
            T.True(first.Chest.Quality > ShopTiers.Typical(with.Tier), $"the day's first customer is {first.Seller.Key} ({(first.Gift ? "a gift" : "selling")}) with a {first.Chest.Quality} chest, the shop's usual colour");
        });

        // ======== Display ========

        yield return new("catalogue", "every new kind of find this run pays 4g at the stall, 7 times that at the Emporium; the same find twice pays once", () =>
        {
            foreach (int tier in new[] { 1, 4 })
            {
                var (without, with) = (Bare(tier), Bare(tier, "catalogue"));
                int fee = Curios.CataloguePerTier * ShopTiers.Scale(tier);
                foreach (var (item, pays) in new[]
                         {
                             (I(Category.Coins, Rarity.Common), true), (I(Category.Coins, Rarity.Common), false), (I(Category.Coins, Rarity.Uncommon), true),
                             (I(Category.Coins, Rarity.Common, fake: true), true), (I(Category.Potions, Rarity.Common) with { Broken = true }, false),
                         })
                {
                    int g0 = with.Gold, w0 = without.Gold;
                    with.Collect(item);
                    without.Collect(item);
                    T.Eq(0, without.Gold - w0, "without the catalogue");
                    T.Eq(pays ? fee : 0, with.Gold - g0, $"{item.Entry}{(item.Broken ? " (broken)" : "")} at tier {tier}");
                }
            }
        });

        yield return new("catalogue", "a find known from an earlier run is still new this run, and pays", () =>
        {
            var with = Bare(1, "catalogue");
            var item = I(Category.Arms, Rarity.Common);
            with.Known.Found.Add(item.Entry);
            int g0 = with.Gold;
            with.Collect(item);
            T.Eq(Curios.CataloguePerTier, with.Gold - g0, "the catalogue's fee");
        });

        yield return new("dome", "room to display one more thing (Thornwick's shelf; Isolde's own)", () =>
        {
            var (without, with) = (Bare(2), Bare(2, "dome"));
            T.Eq(without.DisplaySlots + 1, with.DisplaySlots, "display slots");
            int kept = 0;
            for (int i = 0; i < with.DisplaySlots + 2; i++)
                if (with.Keep(I(Category.Coins, Rarity.Common), 0)) kept++;
            T.Eq(with.DisplaySlots, kept, "things the shelf took");
            var isolde = Build(1, Array.Empty<string>(), bare: true, character: Characters.Isolde);
            var isoldeDome = Build(1, new[] { "dome" }, bare: true, character: Characters.Isolde);
            T.Eq(isolde.DisplaySlots + 1, isoldeDome.DisplaySlots, "Isolde's display slots");
        });
    }
}
