using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Keywright.Core;

namespace CurioTest;

/// <summary>
/// Audit A (0.35): the enchantment, sight-unseen, haggling and perfect-read curios, each card's promises against the
/// game's Core rules. A check that fails is a place where the game doesn't do what the card says (AUDIT_A.md has the
/// scenario, the evidence and a proposed fix for each). Most failures share one of five causes: a seller's lesson visit
/// skips the build (<c>GameState.Greet</c>, lesson pricing); prices are rounded to the coin or the 5g after every step
/// (<c>Visit.Raise</c>, <c>Money.Round</c>), which swallows a small discount; tale chests miss part of the build; "talked
/// down" is measured from the first ask (<c>Rebate</c>, <c>Counted</c>); the slip marks one enchanted thing per kind.
/// </summary>
public static class ChecksA
{
    public static IEnumerable<Check> Checks() =>
        Monocle().Concat(Draught()).Concat(Aether()).Concat(Chalk()).Concat(Ring()).Concat(Staff())
            .Concat(Sealed()).Concat(Dip()).Concat(Bond()).Concat(Wholesale()).Concat(Token()).Concat(Blindfold())
            .Concat(Signet()).Concat(Tongue()).Concat(Knot()).Concat(Hourglass()).Concat(Bell()).Concat(Coffer()).Concat(GoldenTongue())
            .Concat(Trumpet()).Concat(Watch()).Concat(Beads()).Concat(Seal()).Concat(Register()).Concat(Crescent()).Concat(Fork()).Concat(Ear());

    // ---- Enchantment ----

    static IEnumerable<Check> Monocle()
    {
        yield return new("monocle", "without the witchlight lamp, enchanted things glow when they sound", () =>
        {
            var (without, with) = T.Twins("monocle", tier: 3);
            T.True(!without.Has("lamp") && !without.Witchlight, "premise: Crownhold before the lamp, nothing glows without the monocle");
            T.True(with.Witchlight, "with the monocle, the witchlight is lit (the counter and the sim both read GameState.Witchlight)");
            var chest = T.Chest(Rarity.Rare, T.I(Category.Coins, Rarity.Rare, magic: true), T.I(Category.Arms, Rarity.Rare));
            var slip = Hearing.Simulate(chest, Every, lamp: with.Witchlight).Read(chest.Quality, with.Tier);
            T.True(slip.Magic(Category.Coins), "the enchanted coin glowed, and a careful read marks it enchanted");
            T.True(!slip.Magic(Category.Arms), "the plain blade didn't glow");
        });
    }

    static IEnumerable<Check> Draught()
    {
        yield return new("draught", "enchanted things turn up three times as often (Crownhold and the Emporium)", () =>
        {
            foreach (int tier in new[] { 3, 4 })
            {
                var (without, with) = T.Twins("draught", tier, bare: true);
                double plain = MagicShare(without, tier, "rowan"), more = MagicShare(with, tier, "rowan");
                Ratio(3, more / plain, $"at {ShopTiers.Name(tier)}: the share of things enchanted, with it against without ({plain:P1} -> {more:P1})", 0.1);
            }
        });
    }

    static IEnumerable<Check> Aether()
    {
        yield return new("aether", "each enchanted thing found this run: everything 2% likelier to be enchanted, up to 20%", () =>
        {
            var s = T.Shop(3, bare: true, curios: "aether");
            for (int found = 0; found <= 13; found++)
            {
                Close(Math.Min(0.2, 0.02 * found), s.Boosts.Magic, $"{found} enchanted things found: what's added to every thing's chance", 0.0001);
                s.Collect(T.I(Category.Coins, Rarity.Rare, magic: true)); // as the opening notes each find
            }
            var plain = T.Shop(3, bare: true);
            var fresh = T.Shop(3, bare: true, curios: "aether");
            for (int i = 0; i < 5; i++) fresh.Collect(T.I(Category.Gems, Rarity.Rare, magic: true));
            // A seller whose things are enchanted 5% of the time: 15% with five found (2% a find, added).
            Close(0.15, MagicShare(fresh, 3, "rowan"), "a 5% seller's things, with five enchanted things found", 0.015);
            Close(0.05, MagicShare(plain, 3, "rowan"), "the same seller without the jar", 0.01);
        });
    }

    static IEnumerable<Check> Chalk()
    {
        yield return new("chalk", "a chest with an enchanted thing in it has a second one (Crownhold's chests)", () =>
        {
            var s = T.Shop(3, bare: true, curios: "chalk");
            var rng = new Random(3);
            int judged = 0, single = 0;
            for (int i = 0; i < 3000; i++)
            {
                var v = Market.CreateVisit(rng, Sellers.Get("rowan"), 3, s.Boosts);
                var real = v.Chest.Items.Where(x => !x.Fake && !x.IsSong).ToList();
                int magic = real.Count(x => x.Magic);
                if (magic == 0) continue;
                if (real.Count < 2)
                {
                    single++; // a chest of one thing can't have a second (the card can't mean it)
                    continue;
                }
                judged++;
                T.True(magic >= 2, $"a chest of {Describe(v.Chest)} has one enchanted thing");
            }
            T.True(judged > 100, $"premise: enough enchanted chests to judge ({judged}; {single} held a single thing)");
        });
        yield return new("chalk", "the chalk's two enchanted things of one kind can be read perfectly (the slip marks one enchanted per kind)", () =>
        {
            var s = T.Shop(3, bare: true, curios: "chalk");
            var chest = T.Chest(Rarity.Rare, T.I(Category.Coins, Rarity.Rare, magic: true), T.I(Category.Coins, Rarity.Rare, magic: true), T.I(Category.Arms, Rarity.Rare));
            Perfect(s, chest, "an exact slip of two enchanted coins and a blade");
        });
    }

    static IEnumerable<Check> Ring()
    {
        yield return new("ring", "enchanted things are worth four times as much, not twice (sold, and on the slip)", () =>
        {
            var (without, with) = T.Twins("ring", tier: 3, bare: true);
            foreach (var kind in Categories.Kinds.Select(k => k.Category))
            {
                var plain = T.I(kind, Rarity.Rare);
                var magic = T.I(kind, Rarity.Rare, magic: true);
                Ratio(2, without.SaleValue(magic) / (double)without.SaleValue(plain), $"{kind}, enchanted against plain, without the ring", 0.01);
                Ratio(4, with.SaleValue(magic) / (double)with.SaleValue(plain), $"{kind}, enchanted against plain, with the ring", 0.01);
            }
            var chest = T.Chest(Rarity.Rare, T.I(Category.Gems, Rarity.Rare, magic: true));
            var slip = T.Slip(with, chest, (Category.Gems, 1));
            Ratio(4, slip.OneValue(Category.Gems, true) / (double)slip.OneValue(Category.Gems), "the slip's enchanted gem against a plain one", 0.01);
            // Out of a chest just bought, as the opening sells it.
            var bought = T.BuyAndOpen(with, T.Counter(with, T.Visit(with, "ada", chest, ask: 10, trade: Trade.Fixed)), T.Exact(with, chest));
            Ratio(4, bought.Value / (double)with.SaleValue(T.I(Category.Gems, Rarity.Rare)), "an enchanted gem opened and sold, against a plain one", 0.01);
        });
    }

    static IEnumerable<Check> Staff()
    {
        yield return new("staff", "everything in a chest with an enchanted thing in it is enchanted (Crownhold's chests)", () =>
        {
            var s = T.Shop(3, bare: true, curios: "staff");
            var rng = new Random(3);
            int judged = 0;
            for (int i = 0; i < 3000; i++)
            {
                var v = Market.CreateVisit(rng, Sellers.Get("rowan"), 3, s.Boosts);
                var real = v.Chest.Items.Where(x => !x.Fake && !x.IsSong).ToList();
                if (!real.Any(x => x.Magic)) continue;
                judged++;
                T.True(real.All(x => x.Magic), $"a chest of {Describe(v.Chest)} isn't enchanted throughout");
                // (A false bottom's thing never is: nobody could see it glow. The card's "everything" leaves it out.)
                T.True(v.Chest.Hidden == null || !v.Chest.Hidden.Magic, "a false bottom's thing stays plain, as every hidden thing does");
            }
            T.True(judged > 100, $"premise: enough enchanted chests to judge ({judged})");
        });
        yield return new("staff", "a chest the staff enchanted throughout can be read perfectly (two enchanted coins and an enchanted blade)", () =>
        {
            var s = T.Shop(3, bare: true, curios: "staff");
            var chest = T.Chest(Rarity.Rare, T.I(Category.Coins, Rarity.Rare, magic: true), T.I(Category.Coins, Rarity.Rare, magic: true),
                T.I(Category.Arms, Rarity.Rare, magic: true));
            Perfect(s, chest, "an exact slip (the Bookkeeping Engine's record) of a staff chest");
        });
    }

    // ---- Sight unseen ----

    static IEnumerable<Check> Sealed()
    {
        yield return new("sealed", "a seller who waits for an offer takes 15% less sight unseen, and their full price once handled", () =>
        {
            var (without, with) = T.Twins("sealed", tier: 4, bare: true);
            var a = T.Counter(without, T.Visit(without, "ada", Rich(), trade: Trade.Offer));
            var b = T.Counter(with, T.Visit(with, "ada", Rich(), trade: Trade.Offer));
            T.Near(a.Floor * 0.85, b.Floor, "the least they'd take, sight unseen");
            b.Handle();
            T.Near(a.Floor, b.Floor, "handled, the least they'd take");
        });
        yield return new("sealed", "handled, any ask comes back to exactly the full price", () =>
            RoundTrip("sealed"));
        yield return new("sealed", "a seller's first visit (a lesson, with tips on) knocks 15% off sight unseen too", () =>
            LessonAsk("sealed", "garrick", 1, 0.85f, "sight unseen"));
    }

    static IEnumerable<Check> Dip()
    {
        yield return new("dip", "sight unseen, each thing is 15% likelier to come out a step better; handled, no likelier", () =>
        {
            // Common chests of common things: nothing else lifts them, so every step is the dip's.
            var colour = Rarity.Common;
            var dip = T.Shop(1, bare: true, curios: "dip");
            var (better, _, n) = Opened(dip, Rolled(colour, exact: true), 2000, handled: false);
            Close(0.15, better / (double)n, "the share of things a step better, sight unseen", 0.015);
            var handled = Opened(T.Shop(1, bare: true, curios: "dip"), Rolled(colour, exact: true), 1000, handled: true);
            Close(0, handled.Better / (double)handled.Things, "the share a step better, handled", 0.002);
            var none = Opened(T.Shop(1, bare: true), Rolled(colour, exact: true), 1000, handled: false);
            Close(0, none.Better / (double)none.Things, "the share a step better without the dip", 0.002);
        });
        yield return new("dip", "everything in the chest: the false bottom's thing is 15% likelier too", () =>
        {
            var s = T.Shop(2, bare: true, curios: "dip");
            int better = 0, n = 2000;
            var rng = new Random(5);
            for (int i = 0; i < n; i++)
            {
                var chest = new Chest(new[] { T.I(Category.Arms) }, Rarity.Common, hidden: T.I(Category.Coins));
                Buy(s, chest, rng, handled: false);
                if (chest.Hidden.Quality > Rarity.Common) better++;
            }
            Close(0.15, better / (double)n, "the false bottom's coin, a step better", 0.02);
        });
    }

    static IEnumerable<Check> Bond()
    {
        yield return new("bond", "sight unseen and it held less than you paid: 40% of the shortfall back, into the purse", () =>
        {
            var s = T.Shop(2, curios: "bond");
            var chest = T.Chest(Rarity.Common, T.I(Category.Coins), T.I(Category.Arms));
            var v = T.Counter(s, Fixed(s, 100, chest: chest));
            int gold = s.Gold;
            var o = T.BuyAndOpen(s, v, T.Slip(s, chest));
            int held = o.Value - o.Insured;
            T.True(held < 100, $"premise: it held less than its price ({held}g)");
            // (Sums over 30g are paid to the 5g, as every price is: Money.Round.)
            Close((100 - held) * 0.4, o.Insured, $"the Underwriters' payout on a {100 - held}g shortfall", 2.5);
            T.Eq(gold - 100 + o.Insured, s.Gold, "the purse after paying and the payout");
        });
        yield return new("bond", "nothing back for a chest you handled, or one that held more than you paid", () =>
        {
            var s = T.Shop(2, curios: "bond");
            var chest = T.Chest(Rarity.Common, T.I(Category.Coins), T.I(Category.Arms));
            var v = T.Counter(s, Fixed(s, 100, chest: chest));
            v.Handle();
            T.Eq(0, T.BuyAndOpen(s, v, T.Slip(s, chest)).Insured, "handled: no bond");
            var rich = T.Chest(Rarity.Rare, T.I(Category.Gems, Rarity.Rare), T.I(Category.Coins, Rarity.Rare));
            T.Eq(0, T.BuyAndOpen(s, T.Counter(s, Fixed(s, 5, chest: rich)), T.Slip(s, rich)).Insured, "held more than its price: no bond");
        });
    }

    static IEnumerable<Check> Wholesale()
    {
        yield return new("wholesale", "each chest bought makes every seller after it today ask 4% less, up to 20%; tomorrow, nobody's heard", () =>
        {
            var s = T.Shop(2, curios: "wholesale");
            for (int bought = 0; bought <= 6; bought++)
            {
                var v = T.Counter(s, Fixed(s, 500));
                T.Eq(Money.Round(500 * (1 - Math.Min(0.2f, 0.04f * bought))), v.Ask, $"a 500g ask after {bought} bought today");
                v.Buy(new Random(1));
                s.Buy(v, T.Slip(s, v.Chest));
            }
            s.EndDay();
            s.StartDay();
            T.Eq(500, T.Counter(s, Fixed(s, 500)).Ask, "the next morning's first seller");
        });
        yield return new("wholesale", "at Thornwick (where Nim first brings it), one chest bought takes something off every ask", () =>
            Lowers("wholesale", 2, Asks(10, 150), s => s.Today.Bought = 1, "one chest bought today"));
        yield return new("wholesale", "a seller's first visit (a lesson, with tips on) asks less too", () =>
            LessonAsk("wholesale", "nettle", 2, 0.8f, "after five chests bought today", s => s.Today.Bought = 5));
    }

    static IEnumerable<Check> Token()
    {
        yield return new("token", "green and blue chests: twice as many surprises, good ones and bad ones", () =>
        {
            foreach (var colour in new[] { Rarity.Uncommon, Rarity.Rare })
            {
                var without = Opened(T.Shop(4, bare: true), Rolled(colour), 3000, handled: true);
                var with = Opened(T.Shop(4, bare: true, curios: "token"), Rolled(colour), 3000, handled: true);
                Ratio(2, with.Better / (double)without.Better, $"{Categories.ColourName(colour)} chests, good surprises with it against without", 0.15);
                Ratio(2, with.Worse / (double)without.Worse, $"{Categories.ColourName(colour)} chests, bad surprises with it against without", 0.15);
            }
        });
        yield return new("token", "purple chests (the Emporium): twice as many surprises, good ones too (a purple thing it swings up shines)", () =>
        {
            var without = Opened(T.Shop(4, bare: true), Rolled(Rarity.Epic), 3000, handled: true);
            var with = Opened(T.Shop(4, bare: true, curios: "token"), Rolled(Rarity.Epic), 3000, handled: true);
            Ratio(2, (with.Worse + with.Better) / (double)(without.Worse + without.Better), "purple chests, surprises either way with it against without", 0.15);
            // Gold is rare in a purple chest's own roll, so its good surprises are few; the token's up-swings shine, as luck's do.
            T.True(with.Better >= 2 * Math.Max(1, without.Better), $"purple chests, good surprises with it against without ({without.Better} -> {with.Better} of {with.Things})");
        });
        yield return new("token", "the opening credits the token with what it swung better, as well as worse (\"Gambler's Token: n swung!\")", () =>
        {
            // Things exactly as good as their chest, and nothing else lucky: whatever moves, the token moved.
            var s = T.Shop(3, bare: true, curios: "token");
            var rng = new Random(4);
            int up = 0, down = 0, credited = 0;
            for (int i = 0; i < 300; i++)
            {
                var chest = T.Chest(Rarity.Uncommon, T.I(Category.Coins, Rarity.Uncommon), T.I(Category.Arms, Rarity.Uncommon));
                typeof(GameState).GetProperty("DrySpell")!.SetValue(s, 0);
                var p = Buy(s, chest, rng, handled: true);
                up += chest.Items.Count(x => x.Quality > Rarity.Uncommon);
                down += chest.Items.Count(x => x.Quality < Rarity.Uncommon);
                credited += p.Luck.Where(l => l.Curio == "token").Sum(l => l.Changed);
            }
            T.True(up > 0 && down > 0, $"premise: it swung things both ways ({up} better, {down} worse)");
            T.Eq(up + down, credited, $"things the opening says the token swung ({up} better, {down} worse)");
        });
    }

    static IEnumerable<Check> Blindfold()
    {
        yield return new("blindfold", "sight unseen, a chest costs 30% less; handled, the full price", () =>
        {
            var s = T.Shop(3, curios: "blindfold");
            var v = T.Counter(s, Fixed(s, 100));
            T.Eq(70, v.Ask, "a 100g chest sight unseen");
            v.Handle();
            T.Eq(100, v.Ask, "handled");
        });
        yield return new("blindfold", "sight unseen, nothing comes out worse than promised (a gambler's token swinging too); handled, it can", () =>
        {
            var unseen = Opened(T.Shop(3, bare: true, curios: new[] { "blindfold", "token" }), Rolled(Rarity.Uncommon), 2000, handled: false);
            T.Eq(0, unseen.Worse, $"things worse than their green chest, of {unseen.Things}, sight unseen");
            var handled = Opened(T.Shop(3, bare: true, curios: new[] { "blindfold", "token" }), Rolled(Rarity.Uncommon), 500, handled: true);
            T.True(handled.Worse > 0, "premise: handled, some come out worse");
            // The false bottom's thing too.
            var s = T.Shop(3, bare: true, curios: "blindfold");
            var rng = new Random(9);
            for (int i = 0; i < 500; i++)
            {
                var chest = new Chest(new[] { T.I(Category.Arms, Rarity.Uncommon) }, Rarity.Uncommon, hidden: T.I(Category.Coins, Categories.RollQuality(rng, Rarity.Uncommon)));
                Buy(s, chest, rng, handled: false);
                T.True(chest.Hidden.Quality >= Rarity.Uncommon, "a false bottom's coin worse than its green chest");
            }
        });
        yield return new("blindfold", "handled, any ask comes back to exactly the full price (with a sealed bid too)", () =>
        {
            RoundTrip("blindfold");
            RoundTrip("sealed", "blindfold");
        });
        yield return new("blindfold", "a seller's first visit (a lesson, with tips on) costs 30% less sight unseen too", () =>
            LessonAsk("blindfold", "rowan", 3, 0.7f, "sight unseen"));
    }

    // ---- Haggling ----

    static IEnumerable<Check> Signet()
    {
        yield return new("signet", "a haggler hears out an offer too low (a second try), and walks out on a second", () =>
        {
            var s = T.Shop(1, curios: "signet");
            var v = T.Counter(s, T.NewVisit(s, "garrick"));
            T.True(v.Trade == Trade.Haggle && v.Ask > 3, "premise: Garrick haggles");
            T.Eq(ResponseKind.TryAgain, v.Offer(1, new Random(1)).Kind, "a 1-coin offer");
            T.True(v.CanOffer, "a second try");
            T.Eq(ResponseKind.Leave, v.Offer(1, new Random(1)).Kind, "a second 1-coin offer");
            var plain = T.Shop(1);
            var w = T.Counter(plain, T.NewVisit(plain, "garrick"));
            T.Eq(ResponseKind.Leave, w.Offer(1, new Random(1)).Kind, "without the signet, the first 1-coin offer");
        });
        yield return new("signet", "hagglers come down 10% further", () =>
        {
            var (without, with) = T.Twins("signet", tier: 4, bare: true);
            T.Near(T.Visit(without, "garrick", Rich(), trade: Trade.Haggle).Floor * 0.9, T.Visit(with, "garrick", Rich(), trade: Trade.Haggle).Floor,
                "the least a haggler will take");
        });
        yield return new("signet", "a haggler bringing a tale's chest hears you out too", () =>
        {
            var s = T.Shop(2, curios: "signet");
            var v = T.Counter(s, Brought(s, "garrick", T.Chest(Rarity.Uncommon, T.I(Category.Coins, Rarity.Uncommon), T.I(Category.Arms, Rarity.Uncommon))));
            T.True(v.Trade == Trade.Haggle && v.Ask > 3, "premise: Garrick haggles");
            T.Eq(ResponseKind.TryAgain, v.Offer(1, new Random(1)).Kind, "a 1-coin offer for a tale's chest");
        });
        yield return new("signet", "a haggler's first visit (a lesson, with tips on) hears you out too", () =>
        {
            var s = T.Shop(1, curios: "signet");
            var v = T.Counter(s, LessonVisit(s, "garrick"));
            T.Eq(ResponseKind.TryAgain, v.Offer(1, new Random(1)).Kind, "a 1-coin offer in Garrick's lesson");
        });
    }

    static IEnumerable<Check> Tongue()
    {
        yield return new("tongue", "hagglers come down a fifth further, and those who wait for an offer take a fifth less", () =>
        {
            var (without, with) = T.Twins("tongue", tier: 4, bare: true);
            T.Near(T.Visit(without, "garrick", Rich(), trade: Trade.Haggle).Floor * 0.8, T.Visit(with, "garrick", Rich(), trade: Trade.Haggle).Floor,
                "the least a haggler will take");
            T.Near(T.Visit(without, "ada", Rich(), trade: Trade.Offer).Floor * 0.8, T.Visit(with, "ada", Rich(), trade: Trade.Offer).Floor,
                "the least a seller who waits for an offer will take");
            T.Near(Brought(without, "garrick", Rich()).Floor * 0.8, Brought(with, "garrick", Rich()).Floor, "a haggler with a tale's chest");
        });
        yield return new("tongue", "a haggler's first visit (a lesson, with tips on) comes down a fifth further too", () =>
        {
            var (without, with) = T.Twins("tongue");
            int plain = T.Counter(without, LessonVisit(without, "garrick")).Floor, least = T.Counter(with, LessonVisit(with, "garrick")).Floor;
            // A lesson's chest is a few coins, and both floors are rounded from the same exact sum: a fifth off whatever
            // rounded to the plain one (0.36: at 4g that's 3 or 4; the tale's chest above checks the fifth exactly).
            int low = Money.Round((plain - 0.5f) * 0.8f), high = Money.Round((plain + 0.49f) * 0.8f);
            T.True(least >= low && least <= high && least <= plain, $"the least Garrick takes in his lesson ({plain}g without it, {least}g with: a fifth off is {low}g to {high}g)");
        });
    }

    static IEnumerable<Check> Knot()
    {
        yield return new("knot", "each chest talked down this run makes every seller ask 1% less, up to 8%", () =>
        {
            var s = T.Shop(2, curios: "knot");
            T.Eq(500, T.Counter(s, Fixed(s, 500)).Ask, "before any chest is talked down");
            TalkDown(s);
            T.Eq(1, s.Bargains, "one chest talked down");
            T.Eq(495, T.Counter(s, Fixed(s, 500)).Ask, "a 500g ask after one chest talked down");
            for (int i = 0; i < 9; i++) TalkDown(s);
            T.Eq(460, T.Counter(s, Fixed(s, 500)).Ask, $"a 500g ask after {s.Bargains} talked down (8% at most)");
        });
        yield return new("knot", "a chest bought at its asking price wasn't talked down (even when a curio took something off it)", () =>
        {
            var s = T.Shop(2, curios: "knot");
            TalkDown(s);
            var v = T.Counter(s, T.Visit(s, "garrick", Rich(), ask: 500, trade: Trade.Haggle));
            T.Eq(495, v.Ask, "premise: the knot's 1% off a haggler's ask");
            v.Buy(new Random(1));
            s.Buy(v, T.Slip(s, v.Chest));
            T.Eq(1, s.Bargains, "chests talked down after buying one at the price asked");
        });
        yield return new("knot", "at Thornwick (where Nim first brings it), a few chests talked down take something off every ask", () =>
            Lowers("knot", 2, Asks(10, 150), s => typeof(GameState).GetProperty("Bargains")!.SetValue(s, 3), "three chests talked down"));
        yield return new("knot", "a seller's first visit (a lesson, with tips on) asks less too", () =>
            LessonAsk("knot", "nettle", 2, 0.92f, "after eight chests talked down", s => typeof(GameState).GetProperty("Bargains")!.SetValue(s, 8)));
    }

    static IEnumerable<Check> Hourglass()
    {
        yield return new("hourglass", "every chest passed makes the next seller ask 5% less, up to 20%; buying starts it again", () =>
        {
            var s = T.Shop(1, curios: "hourglass");
            var line = Enumerable.Range(0, 7).Select(_ => Fixed(s, 100)).ToArray();
            var v = Line(s, line);
            T.Eq(100, v.Ask, "the first seller");
            for (int passed = 1; passed <= 5; passed++)
            {
                v = PassAndNext(s);
                T.Eq(Money.Round(100 * (1 - Math.Min(0.2f, 0.05f * passed))), v.Ask, $"after {passed} passed");
            }
            v.Buy(new Random(1));
            s.Buy(v, T.Slip(s, v.Chest));
            s.NextCustomer();
            T.Eq(100, s.Current.Ask, "after buying");
        });
        yield return new("hourglass", "buying a chest that wasn't its seller's to sell starts it again too", () =>
        {
            var s = T.Shop(2, curios: "hourglass");
            var stolen = T.Chest(Rarity.Common, T.I(Category.Coins));
            stolen.Owner = "hollis";
            var v = Line(s, Fixed(s, 100), Fixed(s, 100, chest: stolen), Fixed(s, 100));
            v = PassAndNext(s);
            T.Eq(95, v.Ask, "premise: one passed, the stolen chest's seller asks 5% less");
            v.Buy(new Random(1));
            s.Buy(v, T.Slip(s, v.Chest));
            s.NextCustomer();
            T.Eq(100, s.Current.Ask, "the seller after a stolen chest was bought");
        });
        yield return new("hourglass", "at the stall, one chest passed takes something off every ask", () =>
            Lowers("hourglass", 1, Asks(4, 30), s => typeof(GameState).GetProperty("PassStreak")!.SetValue(s, 1), "one chest passed"));
        yield return new("hourglass", "a seller's first visit (a lesson, with tips on) asks less after passes too", () =>
            LessonAsk("hourglass", "garrick", 1, 0.8f, "after four passed", s => typeof(GameState).GetProperty("PassStreak")!.SetValue(s, 4)));
    }

    static IEnumerable<Check> Bell()
    {
        yield return new("bell", "the day's last customer asks 25% less; anyone before them, their full price", () =>
        {
            var s = T.Shop(1, curios: "bell");
            T.Eq(100, T.Counter(s, Fixed(s, 100), behind: 1).Ask, "with someone behind");
            T.Eq(75, T.Counter(s, Fixed(s, 100), behind: 0).Ask, "the day's last");
        });
        yield return new("bell", "the last customer asks 25% less even when someone comes to talk after them", () =>
        {
            var s = T.Shop(2, curios: "bell");
            var v = Line(s, Fixed(s, 100), Talker());
            T.Eq(75, v.Ask, "the last chest of the day, with a tale's talk still to come");
        });
        yield return new("bell", "the last customer asks 25% less when it's a seller's first visit (a lesson, with tips on)", () =>
            LessonAsk("bell", "garrick", 1, 0.75f, "the day's last", behind: 0));
    }

    static IEnumerable<Check> Coffer()
    {
        yield return new("coffer", "everyone haggles with you: a seller who'd name a price haggles instead", () =>
        {
            var (without, with) = T.Twins("coffer", tier: 2);
            T.Eq(Trade.Fixed, T.NewVisit(without, "ada").Trade, "Ada, without it");
            T.Eq(Trade.Haggle, T.NewVisit(with, "ada").Trade, "Ada, with it");
        });
        yield return new("coffer", "40% of what you talk a seller down by comes back to you", () =>
        {
            var s = T.Shop(2, curios: "coffer");
            var v = T.Counter(s, T.Visit(s, "garrick", Rich(), ask: 400, trade: Trade.Haggle));
            v.SetFloor(200);
            int gold = s.Gold;
            T.Eq(ResponseKind.Accept, v.Offer(300, new Random(1)).Kind, "premise: an offer of 300g for a 400g ask");
            var p = s.Buy(v, T.Slip(s, v.Chest));
            T.Eq(40, p.Rebate, "the purse's share of the 100g talked down");
            T.Eq(gold - 300 + 40, s.Gold, "the purse");
        });
        yield return new("coffer", "nothing back for a chest bought at the price asked (a curio's discount isn't talking them down)", () =>
        {
            var s = T.Shop(2, curios: new[] { "coffer", "hourglass" });
            var v = Line(s, Fixed(s, 20), T.Visit(s, "garrick", Rich(), ask: 400, trade: Trade.Haggle), Fixed(s, 1));
            v = PassAndNext(s);
            T.Eq(380, v.Ask, "premise: the Waiting Game's 5% off Garrick's 400g");
            v.Buy(new Random(1));
            T.Eq(0, s.Buy(v, T.Slip(s, v.Chest)).Rebate, "the rebate for paying the asking price");
        });
        yield return new("coffer", "talked down from a price that went up (an angry seller), 40% of what you talked them down by", () =>
        {
            var s = T.Shop(2, curios: "coffer");
            var v = T.Counter(s, T.Visit(s, "cadoc", Rich(), ask: 400));
            T.True(v.Trade == Trade.Haggle, "premise: with the purse, Brother Cadoc haggles");
            v.SetFloor(200);
            v.Anger(new Random(1)); // dropped it, and he minds: half again
            T.Eq(600, v.Ask, "premise: his price, angered");
            T.Eq(ResponseKind.Accept, v.Offer(500, new Random(1)).Kind, "premise: an offer of 500g");
            T.Eq(40, s.Buy(v, T.Slip(s, v.Chest)).Rebate, "the purse's share of the 100g talked down");
        });
        yield return new("coffer", "everyone but Old Aldo and the Maw haggles: crooks too", () => Everyone("coffer"));
        yield return new("coffer", "everyone haggles: a seller's first visit (a lesson, with tips on) too", () =>
        {
            var s = T.Shop(2, curios: "coffer");
            T.Eq(Trade.Haggle, T.Counter(s, LessonVisit(s, "nettle")).Trade, "Nettle's lesson");
        });
    }

    static IEnumerable<Check> GoldenTongue()
    {
        yield return new("goldentongue", "nobody walks out: go as low as you like, and they come down to the least they'd take", () =>
        {
            var s = T.Shop(3, curios: "goldentongue");
            foreach (var key in new[] { "garrick", "pip", "ada" })
            {
                var v = T.Counter(s, T.NewVisit(s, key));
                var r = v.Offer(1, new Random(1));
                T.True(v.Deciding, $"{key}: still at the counter after a 1-coin offer ({r.Kind})");
                T.Eq(v.Floor, v.Ask, $"{key}: the price they come down to");
                T.True(v.CanBuy, $"{key}: you can buy at it");
            }
        });
        yield return new("goldentongue", "they come down to 10% under the least they'd take", () =>
        {
            var (without, with) = T.Twins("goldentongue", tier: 4, bare: true);
            T.Near(T.Visit(without, "garrick", Rich(), trade: Trade.Haggle).Floor * 0.9, T.Visit(with, "garrick", Rich(), trade: Trade.Haggle).Floor,
                "the least a haggler will take");
        });
        yield return new("goldentongue", "everyone haggles: a seller who'd name a price haggles instead", () =>
        {
            var s = T.Shop(3, curios: "goldentongue");
            T.Eq(Trade.Haggle, T.NewVisit(s, "ada").Trade, "Ada");
        });
        yield return new("goldentongue", "nobody walks out: not a haggler bringing a tale's chest", () =>
        {
            var s = T.Shop(3, curios: "goldentongue");
            var v = T.Counter(s, Brought(s, "garrick", Rich()));
            v.Offer(1, new Random(1));
            T.True(v.Deciding, $"Garrick with a tale's chest, after a 1-coin offer: {v.Outcome}");
        });
        yield return new("goldentongue", "nobody walks out: not on a seller's first visit (a lesson, with tips on)", () =>
        {
            var s = T.Shop(3, curios: "goldentongue");
            var v = T.Counter(s, LessonVisit(s, "durgan"));
            T.True(v.Trade == Trade.Haggle, "premise: Durgan haggles");
            v.Offer(1, new Random(1));
            T.True(v.Deciding, $"Durgan's lesson, after a 1-coin offer: {v.Outcome}");
        });
        yield return new("goldentongue", "everyone but Old Aldo and the Maw haggles: crooks too", () => Everyone("goldentongue"));
    }

    // ---- The perfect read ----

    static IEnumerable<Check> Trumpet()
    {
        yield return new("trumpet", "no clank drowns out a delicate thing for you (the seller's ears are their own)", () =>
        {
            Chest Make() => T.Chest(Rarity.Uncommon, T.I(Category.Coins, Rarity.Uncommon), T.I(Category.Arms, Rarity.Uncommon));
            var (without, with) = T.Twins("trumpet", tier: 2);
            var a = T.Counter(without, T.Visit(without, "ada", Make())).Chest;
            var b = T.Counter(with, T.Visit(with, "ada", Make())).Chest;
            foreach (var level in new[] { Shake.Firm, Shake.Hard })
            {
                T.True(!a.HeardAt(level).Any(i => i.Category == Category.Coins), $"premise: without it, the clank drowns the coin at {level}");
                T.True(b.HeardAt(level).Any(i => i.Category == Category.Coins), $"with it, the coin under the clank at {level}");
                T.True(!b.HeardAt(level, plain: true).Any(i => i.Category == Category.Coins), $"the seller still hears the clank drown it at {level}");
            }
        });
        yield return new("trumpet", "no straw muffles a sound for you: everything stirs at its own strength", () =>
        {
            Chest Make() => new(new[] { T.I(Category.Coins, Rarity.Uncommon), T.I(Category.Garb, Rarity.Uncommon) }, Rarity.Uncommon, packed: true);
            var (without, with) = T.Twins("trumpet", tier: 2);
            var a = T.Counter(without, T.Visit(without, "corvin", Make())).Chest;
            var b = T.Counter(with, T.Visit(with, "corvin", Make())).Chest;
            T.True(!a.HeardAt(Shake.Gentle).Any() && !a.HeardAt(Shake.Hard).Any(i => i.Category == Category.Garb), "premise: through straw, a gentle shake stirs nothing, and nothing stirs a cloak");
            T.True(b.HeardAt(Shake.Gentle).Any(i => i.Category == Category.Coins), "with it, the coin at a gentle shake");
            T.True(b.HeardAt(Shake.Hard).Any(i => i.Category == Category.Garb), "with it, the cloak at a hard shake");
        });
        yield return new("trumpet", "on a seller's first visit (a lesson, with tips on), no clank drowns anything out either", () =>
        {
            var s = T.Shop(1, curios: "trumpet");
            var v = T.Counter(s, LessonVisit(s, "garrick"));
            T.True(v.Chest.HeardAt(Shake.Hard).Any(i => i.Category == Category.Coins), "Garrick's lesson chest: the coin under the clank, at a hard shake");
        });
    }

    static IEnumerable<Check> Watch()
    {
        yield return new("watch", "impatient sellers give you twice as long, and a rotten chest stands twice the shaking (and Bram's note says so)", () =>
        {
            var (without, with) = T.Twins("watch", tier: 4);
            foreach (var key in new[] { "flick", "tick" })
            {
                var seller = Sellers.Get(key);
                T.True(without.Patience(seller) > 0, $"premise: {key} is in a hurry");
                T.Eq(2 * without.Patience(seller), with.Patience(seller), $"{key}'s patience");
            }
            var tobin = Sellers.Get("tobin");
            T.Eq(2 * without.ShakeLimit(tobin), with.ShakeLimit(tobin), "how much shaking Tobin's rotten chest stands");
            T.True(with.Terms(T.Visit(with, "flick", T.Chest(Rarity.Common, T.I(Category.Coins)))).Contains("40 seconds"), "the note under Flick's chest says 40 seconds");
            T.True(with.Terms(T.Visit(with, "tobin", T.Chest(Rarity.Common, T.I(Category.Coins)))).Contains("4 seconds of shaking"), "the note under Tobin's chest says 4 seconds");
        });
    }

    static IEnumerable<Check> Beads()
    {
        yield return new("beads", "a chest its seller says nothing else about comes with a tally of every thing in it, false bottom and all", () =>
        {
            var s = T.Shop(2, curios: "beads");
            var chest = new Chest(new[] { T.I(Category.Coins, Rarity.Uncommon), T.I(Category.Arms, Rarity.Uncommon) }, Rarity.Uncommon, hidden: T.I(Category.Gems, Rarity.Uncommon));
            var rook = T.Visit(s, "rook", T.Chest(Rarity.Uncommon, T.I(Category.Coins, Rarity.Uncommon)));
            s.Queue.Clear();
            s.Queue.Add(T.Visit(s, "ada", chest));
            s.Queue.Add(rook);
            typeof(GameState).GetMethod("Curiosities", Any)!.Invoke(s, null);
            var v = s.Queue[0];
            T.Eq(Clue.Tally, v.Says, "what Ada's chest comes with");
            T.Eq(3, Market.ArticleCount(v.Chest), "things the tally counts (two, and the false bottom's)");
            T.Eq(Loc.T($"The Guild clerk's tally: {Market.Articles(chest)}."), v.Clue, "the note under the chest");
            T.Eq(Clue.Pounds, s.Queue[1].Says, "Rook, who says what her scale reads, says only that");
            // The careful player reads it: everything heard, and one more counted, is a false bottom.
            var heard = Hearing.Simulate(chest, Every);
            var slip = heard.Read(chest.Quality, s.Tier);
            Strategy.Reckon(slip, heard, v, s);
            T.True(slip.Hidden, "a careful read of the tally marks the false bottom");
        });
        yield return new("beads", "a tale's chest its seller says nothing else about comes with a tally too", () =>
        {
            var untallied = new List<string>();
            int tales = 0;
            foreach (int seed in new[] { 1, 2, 3 })
            {
                var s = T.Shop(2, seed: seed, curios: "beads");
                for (int day = 0; day < 20; day++)
                {
                    foreach (var v in s.Queue.Where(v => v.Beat != null && v.Talk == null && !v.Gift && !v.Chest.Sealed && !v.Seller.SlipPrice))
                    {
                        tales++;
                        if (v.Says == Clue.None) untallied.Add($"{v.Beat} (day {s.Day})");
                    }
                    s.EndDay();
                    s.StartDay();
                }
            }
            T.True(tales > 0, "premise: a tale brought a chest to the counter");
            T.True(untallied.Count == 0, $"{untallied.Count} of {tales} tale chests came with nothing said and no tally: {string.Join(", ", untallied.Distinct().Take(6))}");
        });
    }

    static IEnumerable<Check> Seal()
    {
        yield return new("seal", "a perfect read pays a valuer's fee of 8% of what the chest held; a wrong one, nothing", () =>
        {
            var (without, with) = T.Twins("seal", tier: 4, bare: true);
            var o = T.BuyAndOpen(with, T.Counter(with, Fixed(with, 10, chest: Rich())), T.Exact(with, Rich()));
            T.True(o.Report.Perfect, "premise: an exact slip is a perfect read");
            T.Near(o.Value * 0.08, o.Fee, "the fee on a perfect read");
            var chest = Rich();
            var wrong = T.BuyAndOpen(with, T.Counter(with, Fixed(with, 10, chest: chest)), T.Slip(with, chest, (Category.Gems, 1)));
            T.Eq(0, wrong.Fee, "the fee on a wrong slip");
            T.Eq(0, T.BuyAndOpen(without, T.Counter(without, Fixed(without, 10, chest: Rich())), T.Exact(without, Rich())).Fee, "without the seal");
        });
    }

    /// <summary>The ear's curios (0.36): a listen back for a perfect read (an echo stone), and for a chest bought sight unseen (lamb's-wool plugs).</summary>
    static IEnumerable<Check> Ear()
    {
        yield return new("echo", "a perfect read gives a listen back; a wrong one, none; never more than the day began with", () =>
        {
            var (without, with) = T.Twins("echo", tier: 4, bare: true);
            foreach (var s in new[] { without, with })
            {
                var v = T.Counter(s, Fixed(s, 10, chest: Rich()));
                T.True(s.Listen(v, Keywright.Core.Check.Shaking(Shake.Hard)) && s.Listen(v, Keywright.Core.Check.Shaking(Shake.Gentle)), "premise: two listens spent on it");
                v.Handle();
                T.True(T.BuyAndOpen(s, v, T.Exact(s, Rich())).Report.Perfect, "premise: an exact slip is a perfect read");
            }
            T.Eq(without.Listens - 2, without.ListensLeft, "listens left after a perfect read that cost two, without it");
            T.Eq(with.Listens - 1, with.ListensLeft, "listens left after a perfect read that cost two, with it");
            var chest = Rich();
            var wrong = T.Counter(with, Fixed(with, 10, chest: chest));
            with.Listen(wrong, Keywright.Core.Check.Shaking(Shake.Hard));
            wrong.Handle();
            int before = with.ListensLeft;
            T.BuyAndOpen(with, wrong, T.Slip(with, chest, (Category.Gems, 1)));
            T.Eq(before, with.ListensLeft, "listens left after a wrong slip");
            with.RestEar();
            var free = T.Counter(with, Fixed(with, 10, chest: Rich()));
            T.BuyAndOpen(with, free, T.Exact(with, Rich()));
            T.Eq(with.Listens, with.ListensLeft, "a perfect read with none spent: no more listens than the day's");
        });
        yield return new("plugs", "a chest bought sight unseen gives a listen back; one you've handled, none", () =>
        {
            var (without, with) = T.Twins("plugs", tier: 2, bare: true);
            foreach (var s in new[] { without, with })
            {
                // One listen spent on a first chest (bought handled), then a second taken as it stands.
                var first = T.Counter(s, Fixed(s, 10, chest: Rich()));
                T.True(s.Listen(first, Keywright.Core.Check.Shaking(Shake.Hard)), "premise: a listen spent on the first chest");
                first.Handle();
                T.BuyAndOpen(s, first, T.Slip(s, Rich()));
            }
            T.Eq(with.Listens - 1, with.ListensLeft, "after a handled chest, with them: no listen back");
            foreach (var s in new[] { without, with })
            {
                var second = T.Counter(s, Fixed(s, 10, chest: Rich()));
                T.True(!second.Handled, "premise: the second chest is untouched");
                T.BuyAndOpen(s, second, T.Slip(s, Rich()));
            }
            T.Eq(without.Listens - 1, without.ListensLeft, "after a chest bought sight unseen, without them");
            T.Eq(with.Listens, with.ListensLeft, "after a chest bought sight unseen, with them: the listen's back");
        });
    }

    static IEnumerable<Check> Register()
    {
        yield return new("register", "a perfect read pays 0.5% a perfect read so far, up to 12%; a wrong one nothing", () =>
        {
            var s = T.Shop(4, bare: true, curios: "register");
            typeof(GameState).GetProperty("PerfectReads")!.SetValue(s, 10);
            // The next perfect read is the eleventh: it counts too.
            Close(0.055, s.PerfectShare, "the fee's share for the read after ten", 0.0001);
            typeof(GameState).GetProperty("PerfectReads")!.SetValue(s, 30);
            Close(0.12, s.PerfectShare, "the fee's share after thirty (12% at most)", 0.0001);
            var chest = Rich();
            T.Eq(0, T.BuyAndOpen(s, T.Counter(s, Fixed(s, 10, chest: chest)), T.Slip(s, chest, (Category.Gems, 1))).Fee, "a wrong slip");
        });
        yield return new("register", "the run's first perfect read pays 0.5% (it's the one so far)", () =>
        {
            var s = T.Shop(4, bare: true, curios: "register");
            var o = T.BuyAndOpen(s, T.Counter(s, Fixed(s, 10, chest: Rich())), T.Exact(s, Rich()));
            T.True(o.Report.Perfect, "premise: a perfect read");
            T.Near(o.Value * 0.005, o.Fee, $"the fee on a {o.Value}g chest, the run's first perfect read");
        });
    }

    static IEnumerable<Check> Crescent()
    {
        yield return new("crescent", "everything comes out exactly as its chest promised (better or worse rolls, and luck, held steady)", () =>
        {
            var s = T.Shop(4, curios: new[] { "crescent", "foot" });
            foreach (var colour in new[] { Rarity.Common, Rarity.Uncommon, Rarity.Epic })
            {
                var (better, worse, n) = Opened(s, Rolled(colour), 1000, handled: true);
                T.True(better == 0 && worse == 0, $"{Categories.ColourName(colour)} chests: {better} better and {worse} worse of {n}");
            }
        });
        yield return new("crescent", "everything comes out exactly as its chest promised, however long the dry spell", () =>
        {
            var s = T.Shop(1, bare: true, curios: "crescent");
            var (better, _, n) = Opened(s, Rolled(Rarity.Common, exact: true), 100, handled: true, drySpell: true);
            T.Eq(0, better, $"things better than their common chest, of {n} (dry spell now {s.DrySpell})");
        });
        yield return new("crescent", "sellers ask 5% less (at each shop, as a player would have it)", () =>
        {
            var said = new List<string>();
            bool off = false;
            foreach (int tier in new[] { 1, 2, 3, 4 })
            {
                var (without, with) = T.Twins("crescent", tier);
                double ratio = with.PriceFactor / without.PriceFactor;
                said.Add($"{ShopTiers.Name(tier)} {1 - ratio:P1} less");
                off |= Math.Abs(ratio - 0.95) > 0.005;
            }
            T.True(!off, $"what sellers ask with it against without: {string.Join(", ", said)}");
        });
    }

    static IEnumerable<Check> Fork()
    {
        yield return new("fork", "each perfect read in a row: the next chest's things sell for 3% more, up to 30%; a wrong read starts again", () =>
        {
            var s = T.Shop(1, bare: true, curios: "fork");
            // A dear thing (960g), so selling to the coin is well under a tenth of a percent.
            var gem = T.I(Category.Gems, Rarity.Legendary, magic: true);
            for (int streak = 0; streak <= 12; streak++)
            {
                Close(1 + Math.Min(0.3, 0.03 * streak), Bonus(s, gem), $"after {streak} perfect reads in a row", 0.001);
                s.RecordRead(true);
            }
            s.RecordRead(false);
            Close(1, Bonus(s, gem), "after a wrong read", 0.001);
        });
        yield return new("fork", "the next chest's things sell for more whether its own read is perfect or not", () =>
        {
            var s = T.Shop(1, bare: true, curios: "fork");
            for (int i = 0; i < 3; i++) s.RecordRead(true);
            var chest = Rich();
            var v = T.Counter(s, Fixed(s, 10, chest: chest));
            v.Buy(new Random(1));
            var p = s.Buy(v, T.Slip(s, chest, (Category.Gems, 1)));
            T.True(!p.Perfect, "premise: a wrong slip");
            Close(1.09, s.SaleValue(chest.Items[0], PickGrade.Clean, p) / (double)s.SaleValue(chest.Items[0]), "what its gem sells for, against a plain sale", 0.005);
        });
        yield return new("fork", "a Guild crate (no read: nothing to be right or wrong about) doesn't break the streak", () =>
        {
            var s = T.Shop(2, bare: true, curios: "fork");
            for (int i = 0; i < 3; i++) s.RecordRead(true);
            var crate = new Chest(Array.Empty<Item>(), Rarity.Uncommon) { Crate = Campaign.Bram };
            var o = T.BuyAndOpen(s, T.Counter(s, Fixed(s, 10, chest: crate)), new Appraisal(crate.Quality, s.Tier));
            T.True(o.Report.NoRead, "premise: a crate is no read");
            Close(1.09, Bonus(s, T.I(Category.Gems, Rarity.Legendary, magic: true)), "the next chest's gem, after three perfect reads and a crate", 0.001);
        });
    }

    // ---- Tools ----

    static readonly BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    static readonly Shake[] Every = { Shake.Gentle, Shake.Firm, Shake.Hard };

    static void Ratio(double expected, double actual, string what, double tolerance) =>
        T.True(Math.Abs(actual / expected - 1) <= tolerance, $"{what}: expected x{expected:0.##}, got x{actual:0.###}");

    static void Close(double expected, double actual, string what, double within) =>
        T.True(Math.Abs(expected - actual) <= within, $"{what}: expected {expected:0.####}, got {actual:0.####}");

    static string Describe(Chest chest) => string.Join(", ", chest.Items.Select(i => (i.Magic ? "enchanted " : "") + i.Category));

    /// <summary>A chest worth a few hundred gold (three purple gems), so a 5g rounding is under 1% of any price on it.</summary>
    static Chest Rich() => T.Chest(Rarity.Epic, T.I(Category.Gems, Rarity.Epic), T.I(Category.Gems, Rarity.Epic), T.I(Category.Gems, Rarity.Epic));

    static Visit Fixed(GameState s, int ask, string seller = "ada", Chest chest = null) =>
        T.Visit(s, seller, chest ?? T.Chest(Rarity.Common, T.I(Category.Coins)), ask: ask, trade: Trade.Fixed);

    /// <summary>The queue as given; the first comes to the counter (<c>GameState.NextCustomer</c>: the build's word on the price).</summary>
    static Visit Line(GameState s, params Visit[] visits)
    {
        s.Queue.Clear();
        s.Queue.AddRange(visits);
        s.NextCustomer();
        return s.Current;
    }

    /// <summary>The customer at the counter is passed (as the counter's Pass button does), and the next comes in.</summary>
    static Visit PassAndNext(GameState s)
    {
        s.Current.Pass(new Random(1));
        s.Pass();
        s.NextCustomer();
        return s.Current;
    }

    /// <summary>A seller's first visit with tips on: their lesson, made as the shop makes it (<c>GameState.NewVisit</c>).</summary>
    /// <summary>A seller's lesson as this shop makes one (GameState.Lesson: its prices, a curio's terms), from its own dice.</summary>
    static Visit LessonVisit(GameState s, string seller) => s.Lesson(Lessons.For(seller), rng: new Random(1));

    /// <summary>
    /// A seller's lesson brought to the counter in a shop with this curio (after <paramref name="setup"/>): it should ask
    /// what the card says any seller asks, to the coin or the 5g as every price is rounded (<c>Money.Round</c>).
    /// </summary>
    static void LessonAsk(string curio, string seller, int tier, float factor, string what, Action<GameState> setup = null, int behind = 1)
    {
        var s = T.Shop(tier, curios: curio);
        setup?.Invoke(s);
        var lesson = LessonVisit(s, seller);
        int ask = lesson.Ask;
        var v = T.Counter(s, lesson, behind);
        // To the coin, and never nothing off (Visit.Rounded).
        T.Eq(Visit.Rounded(ask * factor, ask), v.Ask, $"{Sellers.Get(seller).Short}'s lesson chest ({ask}g), {what}");
    }

    /// <summary>Someone come to talk, not to sell (a tale's beat, a tool offered), as the tales queue them.</summary>
    static Visit Talker(string who = TaleBook.Hollis) => Visit.Talking(Sellers.Person(who), new Talk { Who = who, Key = "curiotest" });

    /// <summary>A tale's chest brought by a regular, priced as the tales price it (<c>TaleBook.Brought</c>).</summary>
    static Visit Brought(GameState s, string seller, Chest chest) =>
        (Visit)typeof(TaleBook).GetMethod("Brought", Any)!.Invoke(null, new object[] { s, new Random(1), seller, chest, "{price}, for a tale.", "barrow" });

    /// <summary>A haggler's chest bought for less than they asked: one chest talked down.</summary>
    static void TalkDown(GameState s)
    {
        var v = T.Counter(s, T.Visit(s, "garrick", T.Chest(Rarity.Common, T.I(Category.Coins)), ask: 200, trade: Trade.Haggle));
        v.SetFloor(100);
        T.Eq(ResponseKind.Accept, v.Offer(150, new Random(1)).Kind, "premise: talked down from 200g to 150g");
        s.Buy(v, T.Slip(s, v.Chest));
    }

    /// <summary>Asks a seller can make: to the coin up to 30g, to the 5g above (<c>Money.Round</c>).</summary>
    static IEnumerable<int> Asks(int from, int to) => Enumerable.Range(from, to - from + 1).Where(a => a <= 30 || a % 5 == 0);

    /// <summary>
    /// The curio's first step, at the shop Nim first brings it to: every ask a seller there can make comes to the counter
    /// for less than it was.
    /// </summary>
    static void Lowers(string curio, int tier, IEnumerable<int> asks, Action<GameState> setup, string what)
    {
        var stuck = new List<int>();
        int n = 0;
        foreach (int ask in asks)
        {
            n++;
            var s = T.Shop(tier, curios: curio);
            setup(s);
            var v = T.Counter(s, Fixed(s, ask));
            if (v.Ask >= ask) stuck.Add(ask);
        }
        T.True(stuck.Count == 0, $"{what} at {ShopTiers.Name(tier)}: {stuck.Count} of {n} asks don't go down at all ({string.Join(", ", stuck.Take(14).Select(a => a + "g"))}{(stuck.Count > 14 ? ", ..." : "")})");
    }

    /// <summary>Sight unseen and then handled, every ask a seller can make (to 400g) is back where it was.</summary>
    static void RoundTrip(params string[] curios)
    {
        var off = new List<string>();
        int n = 0;
        foreach (int ask in Asks(1, 400))
        {
            n++;
            var s = T.Shop(3, curios: curios);
            var v = T.Counter(s, Fixed(s, ask));
            v.Handle();
            if (v.Ask != ask) off.Add($"{ask}g->{v.Ask}g");
        }
        T.True(off.Count == 0, $"{string.Join(" and ", curios)}: {off.Count} of {n} asks aren't their full price once handled ({string.Join(", ", off.Take(10))}{(off.Count > 10 ? ", ..." : "")})");
    }

    /// <summary>An exact slip of the chest reads it perfectly.</summary>
    static void Perfect(GameState s, Chest chest, string what)
    {
        var report = T.Exact(s, chest).Check(chest);
        T.True(report.Perfect, $"{what} reads: {string.Join(", ", report.Items.Select(r => $"{r.Item.Category} {r.Kind}"))}");
    }

    /// <summary>A shop's enchanted things, as a share of the real things in this seller's chests (3000 visits).</summary>
    static double MagicShare(GameState s, int tier, string seller)
    {
        var rng = new Random(3);
        int things = 0, magic = 0;
        for (int i = 0; i < 3000; i++)
        {
            var real = Market.CreateVisit(rng, Sellers.Get(seller), tier, s.Boosts).Chest.Items.Where(x => !x.Fake && !x.IsSong).ToList();
            things += real.Count;
            magic += real.Count(x => x.Magic);
        }
        return magic / (double)things;
    }

    /// <summary>Things of one kind as packed: rolled around the colour (or exactly at it).</summary>
    static Func<Random, Chest> Rolled(Rarity colour, int count = 4, bool exact = false) =>
        rng => new Chest(Enumerable.Range(0, count).Select(_ => T.I(Category.Coins, exact ? colour : Categories.RollQuality(rng, colour))), colour);

    /// <summary>Buys a chest at a coin, as the counter does (luck at purchase), handled or sight unseen.</summary>
    static Purchase Buy(GameState s, Chest chest, Random rng, bool handled)
    {
        var v = T.Visit(s, "ada", chest, ask: 1, trade: Trade.Fixed);
        if (handled) v.Handle();
        v.Buy(rng);
        return s.Buy(v, T.Slip(s, chest));
    }

    /// <summary>
    /// Buys chests (each packed fresh) and counts what came out better (or shone) and worse than its chest's colour. The dry
    /// spell is kept out of it (it lifts a thing now and then), unless asked for.
    /// </summary>
    static (int Better, int Worse, int Things) Opened(GameState s, Func<Random, Chest> pack, int chests, bool handled, bool drySpell = false)
    {
        var rng = new Random(11);
        int better = 0, worse = 0, n = 0;
        for (int c = 0; c < chests; c++)
        {
            var chest = pack(rng);
            if (!drySpell) typeof(GameState).GetProperty("DrySpell")!.SetValue(s, 0);
            Buy(s, chest, rng, handled);
            foreach (var i in chest.Everything.Where(i => !i.Fake && !i.Broken && !i.IsSong))
            {
                n++;
                if (i.Quality > chest.Quality || i.Shone) better++;
                else if (i.Quality < chest.Quality) worse++;
            }
        }
        return (better, worse, n);
    }

    /// <summary>What a thing out of a chest just bought sells for here, against the same thing sold plain (the fork's share).</summary>
    static double Bonus(GameState s, Item item)
    {
        var chest = T.Chest(item.Quality, item);
        var purchase = new Purchase(T.Visit(s, "ada", chest, ask: 1, trade: Trade.Fixed), T.Slip(s, chest));
        return s.SaleValue(item, PickGrade.Clean, purchase) / (double)s.SaleValue(item);
    }

    /// <summary>Everyone haggles: the sellers who'd name a price and never haggle with Rosa (a crook, the King, the Maw).</summary>
    static void Everyone(string curio)
    {
        var s = T.Shop(4, curios: curio);
        // Crooks (Gilt) haggle with you; the King's price is your slip, and the Maw's a coin, as the card says.
        var fixedPrice = new[] { "gilt" }.Where(key => T.Visit(s, key, Rich()).Trade != Trade.Haggle).ToList();
        T.True(fixedPrice.Count == 0, $"still a price to take or leave from {string.Join(", ", fixedPrice)}");
        T.True(Curios.Get(curio).Blurb.Contains("Old Aldo and the Maw"), "the card names Old Aldo (the King: his price is your slip) and the Maw, who don't haggle");
    }
}
