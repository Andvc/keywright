using System;
using System.Collections.Generic;
using System.Linq;
using Keywright.Core;

namespace CurioTest;

/// <summary>
/// Audit D (0.37): the curios that do a build's routine for it (Core/Hands.cs): a quill that ticks what's heard, a
/// pocket engine that reads the chest, a moth that stars what's enchanted, a gauge that marks a false bottom, a primer
/// that names a warded thing, an abacus that knows a haggler's least. And the rules 0.37 changed under every curio: the
/// trade's ways carried across seasons, and an enchanted thing read with no light to see it by.
/// </summary>
public static class ChecksD
{
    public static IEnumerable<Check> Checks() => Quill().Concat(Engine()).Concat(Moth()).Concat(Gauge()).Concat(Primer()).Concat(Abacus()).Concat(Ways());

    static Visit Fixed(GameState s, Chest chest, int ask = 10) => T.Visit(s, "garrick", chest, ask: ask, trade: Trade.Fixed);

    static IEnumerable<Check> Quill()
    {
        yield return new("quill", "each thing is ticked as it's first heard, one more of its kind, counted; nothing without it", () =>
        {
            var (without, with) = T.Twins("quill", tier: 3);
            var chest = T.Chest(Rarity.Rare, T.I(Category.Coins, Rarity.Rare), T.I(Category.Coins, Rarity.Rare), T.I(Category.Arms, Rarity.Rare));
            foreach (var s in new[] { without, with })
            {
                var slip = T.Slip(s, chest);
                foreach (var item in chest.Items) s.Tick(slip, item, glowed: false);
                T.Eq(s == with ? 2 : 0, slip.Count(Category.Coins), $"coins ticked {(s == with ? "with" : "without")} it, after two purses sounded");
                T.Eq(s == with ? 1 : 0, slip.Count(Category.Arms), $"weapons ticked {(s == with ? "with" : "without")} it");
            }
            T.True(T.Slip(with, chest) is var exact && chest.Items.All(i => with.Tick(exact, i, false) || true) && exact.Check(chest).Perfect,
                "a plain chest heard through is a perfect read with nothing clicked");
        });
        yield return new("quill", "a thing that glows as it sounds is starred; a fourth of a kind isn't counted past the slip's three", () =>
        {
            var with = T.Shop(3, curios: "quill");
            var chest = T.Chest(Rarity.Rare, T.I(Category.Gems, Rarity.Rare, magic: true));
            var slip = T.Slip(with, chest);
            with.Tick(slip, chest.Items[0], glowed: true);
            T.True(slip.Count(Category.Gems) == 1 && slip.Magic(Category.Gems), "the gem that glowed is ticked and starred");
            var many = T.Slip(with, chest);
            for (int i = 0; i < 5; i++) with.Tick(many, T.I(Category.Coins), false);
            T.Eq(3, many.Count(Category.Coins), "five purses heard: the slip counts to three");
        });
        yield return new("quill", "it writes what was heard, not what's so: sand is ticked as a spell, and what never sounds isn't ticked", () =>
        {
            var with = T.Shop(4, curios: "quill");
            var sand = T.I(Category.Scrolls, Rarity.Rare, fake: true);
            var chest = new Chest(new[] { sand, T.I(Category.Coins, Rarity.Rare) }, Rarity.Rare, T.I(Category.Gems, Rarity.Rare));
            var slip = T.Slip(with, chest);
            foreach (var item in chest.Items) with.Tick(slip, item, false);
            T.Eq(1, slip.Count(Category.Scrolls), "the sand's rustle is ticked under Spells (it's yours to strike out)");
            T.True(!slip.Hidden && slip.Count(Category.Gems) == 0, "what's under the false bottom never sounded, and isn't on the slip");
            T.True(!slip.Check(chest).Perfect, "so that slip isn't a perfect read until you've worked the rest out");
        });
    }

    static IEnumerable<Check> Engine()
    {
        yield return new("engine", "every chest is read as it comes in: kinds, counts, the enchanted, the false bottom and the warded thing", () =>
        {
            var (without, with) = T.Twins("engine", tier: 4);
            Chest Make()
            {
                var chest = new Chest(new[] { T.I(Category.Coins, Rarity.Epic), T.I(Category.Coins, Rarity.Epic), T.I(Category.Gems, Rarity.Epic, magic: true),
                    T.I(Category.Scrolls, Rarity.Epic, fake: true) }, Rarity.Epic, T.I(Category.Gems, Rarity.Epic));
                chest.Ward(T.I(Category.Potions, Rarity.Epic));
                return chest;
            }
            var plain = T.Slip(without, Make());
            T.True(!without.Premark(plain, Fixed(without, Make())) && plain.Blank, "without it, nothing is written on the slip");
            var read = Make();
            var visit = Fixed(with, read);
            var slip = T.Slip(with, read);
            T.True(with.Premark(slip, visit), "with it, the slip is written as the chest comes in");
            T.Eq(2, slip.Count(Category.Coins), "both purses counted");
            T.True(slip.Count(Category.Gems) == 1 && slip.Magic(Category.Gems), "the enchanted gem ticked and starred");
            T.Eq(0, slip.Count(Category.Scrolls), "the sand isn't ticked");
            T.True(slip.Hidden, "the false bottom marked");
            T.True(read.Names(slip), "the warded potion named");
            T.True(Strategy.Recorded(visit, with) && !Strategy.Recorded(Fixed(without, Make()), without), "the careful player takes its word (Strategy.Recorded), as with the Guild's engine");
            var opened = T.BuyAndOpen(with, T.Counter(with, visit), slip);
            T.True(opened.Report.Perfect, "and the chest bought on that slip is a perfect read");
        });
        yield return new("engine", "not a chest chained shut, a Guild crate or a mimic's: there's nothing it can read", () =>
        {
            var with = T.Shop(4, curios: "engine");
            var chained = new Chest(new[] { T.I(Category.Coins, Rarity.Epic) }, Rarity.Epic, isSealed: true);
            var slip = T.Slip(with, chained);
            T.True(!with.Premark(slip, Fixed(with, chained)) && slip.Blank, "a sealed chest's slip stays blank");
            T.True(!Strategy.Recorded(Fixed(with, chained), with), "and the careful player doesn't take it as read");
            var mimic = new Chest(Array.Empty<Item>(), Rarity.Epic) { Breathing = true };
            var empty = T.Slip(with, mimic);
            T.True(!with.Premark(empty, Fixed(with, mimic)) && empty.Blank, "a mimic's slip stays blank");
        });
        yield return new("engine", "it opens by its feat alone: a hundred perfect reads, all told", () =>
        {
            T.True(Curios.Get("engine").Unlock == CurioUnlock.Feat && Curios.Feats["engine"] == (Feat.PerfectReads, 100), "the Pocket Engine's feat is 100 perfect reads");
            var profile = new Profile();
            T.True(!profile.Opens(Curios.Get("engine")), "a new profile's Nim doesn't carry it");
            profile.Feats[Feat.PerfectReads.ToString()] = 100;
            T.True(profile.Opens(Curios.Get("engine")), "after a hundred perfect reads, he does");
        });
    }

    static IEnumerable<Check> Moth()
    {
        yield return new("moth", "every enchanted thing in a chest is starred as it comes in, witchlight or not; a plain chest gets nothing", () =>
        {
            var (without, with) = T.Twins("moth", tier: 3, bare: true);
            T.True(!with.Witchlight, "premise: no witchlight, no monocle");
            var chest = T.Chest(Rarity.Rare, T.I(Category.Coins, Rarity.Rare, magic: true), T.I(Category.Arms, Rarity.Rare), T.I(Category.Gems, Rarity.Rare, magic: true));
            var slip = T.Slip(with, chest);
            T.True(with.Premark(slip, Fixed(with, chest)), "the moth writes on the slip");
            T.True(slip.Magic(Category.Coins) && slip.Magic(Category.Gems), "both enchanted kinds starred");
            T.True(slip.Count(Category.Arms) == 0 && !slip.Magic(Category.Arms), "the plain blade isn't ticked or starred");
            var none = T.Slip(without, chest);
            T.True(!without.Premark(none, Fixed(without, chest)) && none.Blank, "without it, nothing");
            var plainChest = T.Chest(Rarity.Rare, T.I(Category.Arms, Rarity.Rare));
            var blank = T.Slip(with, plainChest);
            T.True(!with.Premark(blank, Fixed(with, plainChest)) && blank.Blank, "a chest with nothing enchanted: the moth doesn't stir");
        });
        yield return new("moth", "a fake is never starred, and a starred slip filled in by ear is a perfect read", () =>
        {
            var with = T.Shop(4, bare: true, curios: "moth");
            var chest = T.Chest(Rarity.Epic, T.I(Category.Coins, Rarity.Epic, fake: true), T.I(Category.Garb, Rarity.Epic, magic: true), T.I(Category.Arms, Rarity.Epic));
            var slip = T.Slip(with, chest);
            with.Premark(slip, Fixed(with, chest));
            T.True(!slip.Magic(Category.Coins) && slip.Count(Category.Coins) == 0, "demon gold isn't starred");
            slip.Set(Category.Arms, 1);
            T.True(slip.Check(chest).Perfect, "the cloak starred by the moth, the blade ticked by ear: a perfect read");
        });
    }

    static IEnumerable<Check> Gauge()
    {
        yield return new("gauge", "a false bottom is marked as the chest comes in; a plain floor reads true", () =>
        {
            var (without, with) = T.Twins("gauge", tier: 2);
            var hollow = new Chest(new[] { T.I(Category.Coins, Rarity.Uncommon) }, Rarity.Uncommon, T.I(Category.Coins, Rarity.Uncommon));
            var solid = T.Chest(Rarity.Uncommon, T.I(Category.Coins, Rarity.Uncommon));
            var slip = T.Slip(with, hollow);
            T.True(with.Premark(slip, Fixed(with, hollow)) && slip.Hidden, "with it, the false bottom is on the slip before a hand's laid on the chest");
            var plain = T.Slip(with, solid);
            plain.Hidden = true;
            with.Premark(plain, Fixed(with, solid));
            T.True(!plain.Hidden, "a false bottom marked where there's none is struck out: the gauge reads true");
            var none = T.Slip(without, hollow);
            T.True(!without.Premark(none, Fixed(without, hollow)) && !none.Hidden, "without it, nothing");
        });
        yield return new("gauge", "no drop needed: the careful player doesn't drop a chest the gauge has read (and breaks no glass)", () =>
        {
            var (without, with) = T.Twins("gauge", tier: 2);
            var chest = T.Chest(Rarity.Uncommon, T.I(Category.Coins, Rarity.Uncommon));
            var heard = Hearing.Simulate(chest, new[] { Shake.Gentle, Shake.Firm, Shake.Hard });
            T.True(Strategy.ShouldDrop(Fixed(without, chest), heard, without.Tier, without), "premise: without it, a careful player drops this chest");
            T.True(!Strategy.ShouldDrop(Fixed(with, chest), heard, with.Tier, with), "with it, there's no reason to");
        });
    }

    static IEnumerable<Check> Primer()
    {
        yield return new("primer", "a warded thing is named on the slip: one more of its kind than the chest holds, and the ward gives it up", () =>
        {
            var (without, with) = T.Twins("primer", tier: 3);
            Chest Make()
            {
                var chest = T.Chest(Rarity.Rare, T.I(Category.Coins, Rarity.Rare), T.I(Category.Garb, Rarity.Rare));
                chest.Ward(T.I(Category.Potions, Rarity.Rare));
                return chest;
            }
            var read = Make();
            var slip = T.Slip(with, read, (Category.Coins, 1), (Category.Garb, 1));
            T.True(with.Premark(slip, Fixed(with, read)), "the primer writes on the slip");
            T.Eq(1, slip.Count(Category.Potions), "the potion nobody could hear is named");
            T.True(read.Names(slip), "and the ward takes that as its name");
            T.Eq((Category?)Category.Potions, with.PrimerNames(Fixed(with, read)), "the counter knows which kind it named (a quill keeps one more of it)");
            var opened = T.BuyAndOpen(with, T.Counter(with, Fixed(with, read)), slip);
            T.True(opened.Report.Perfect && read.Items.Any(i => i.Category == Category.Potions), "the chest opens with the potion in it: a perfect read");
            var bare = Make();
            var none = T.Slip(without, bare, (Category.Coins, 1), (Category.Garb, 1));
            T.True(!without.Premark(none, Fixed(without, bare)) && !bare.Names(none), "without it, the ward keeps it");
        });
        yield return new("primer", "a bride's warded purse beside another purse: it names two", () =>
        {
            var with = T.Shop(3, curios: "primer");
            var chest = T.Chest(Rarity.Rare, T.I(Category.Coins, Rarity.Rare), T.I(Category.Garb, Rarity.Rare), T.I(Category.Potions, Rarity.Rare));
            chest.Ward(T.I(Category.Coins, Rarity.Rare));
            var slip = T.Slip(with, chest, (Category.Coins, 1), (Category.Garb, 1), (Category.Potions, 1));
            with.Premark(slip, Fixed(with, chest));
            T.Eq(2, slip.Count(Category.Coins), "one purse heard, one warded: two on the slip");
            T.True(chest.Names(slip), "and that names it");
        });
    }

    static IEnumerable<Check> Abacus()
    {
        yield return new("abacus", "you know the least a haggler will take: the offer starts on it, and they take it", () =>
        {
            var (without, with) = T.Twins("abacus", tier: 2);
            Visit Haggle(GameState s) => T.Counter(s, T.Visit(s, "garrick", T.Chest(Rarity.Uncommon, T.I(Category.Coins, Rarity.Uncommon), T.I(Category.Arms, Rarity.Uncommon)),
                trade: Trade.Haggle));
            var plain = Haggle(without);
            var known = Haggle(with);
            T.Eq(plain.Floor, known.Floor, "premise: the same seller, the same least (the abacus changes no price)");
            int worth = known.Ask * 2;
            T.Eq(known.Floor, Strategy.FirstOffer(known, worth, with), "with it, the careful player's first offer is exactly their least");
            T.True(Strategy.FirstOffer(plain, worth, without) < plain.Floor, "without it, a first offer is a guess under it");
            var answer = known.Offer(Strategy.FirstOffer(known, worth, with), new Random(1));
            T.True(answer.Kind == ResponseKind.Accept && answer.Price == known.Floor, $"and they take it at once, for {known.Floor}g");
        });
        yield return new("abacus", "not for a chest worth less than their least, nor a fixed price", () =>
        {
            var with = T.Shop(2, curios: "abacus");
            var haggle = T.Counter(with, T.Visit(with, "garrick", T.Chest(Rarity.Uncommon, T.I(Category.Coins, Rarity.Uncommon)), trade: Trade.Haggle));
            T.True(Strategy.FirstOffer(haggle, haggle.Floor - 1, with) < haggle.Floor, "a chest worth less than their least gets no offer at it");
            T.True(with.KnowsFloors, "the shop knows floors");
        });
    }

    /// <summary>0.37's rules under every curio: the trade's ways across seasons, and a read with no light.</summary>
    static IEnumerable<Check> Ways()
    {
        yield return new("monocle", "with no light at all, an enchanted thing counted as its kind is read right (0.37); with one, an unmarked glow is a miss", () =>
        {
            var (dark, lit) = T.Twins("monocle", tier: 3, bare: true);
            var chest = T.Chest(Rarity.Rare, T.I(Category.Coins, Rarity.Rare, magic: true));
            var slip = T.Slip(dark, chest, (Category.Coins, 1));
            T.True(slip.Check(chest).Perfect, "no witchlight, no monocle: the enchanted purse ticked as a purse is a perfect read");
            var seen = T.Slip(lit, chest, (Category.Coins, 1));
            T.True(!seen.Check(chest).Perfect, "with the monocle lit, not starring it is a miss");
        });
        yield return new("compass", "the trade's ways go by the grandest market any season reached: a later season's stall has false bottoms and counts to three", () =>
        {
            var first = T.Shop(1, bare: true);
            T.Eq(1, first.Ways, "a first season at the stall: the stall's own ways");
            T.True(!new Appraisal(Rarity.Common, first.Tier, ways: first.Ways).CountsHidden, "no false-bottom row on its slip");
            T.Eq(1, new Appraisal(Rarity.Common, first.Tier, ways: first.Ways).MaxCount, "and it counts one of a kind");
            var later = GameState.NewRun(7, guided: false);
            later.Remember(new Profile { Runs = 1, BestTier = 3 });
            later.StartDay();
            T.Eq(3, later.Ways, "a season after one that reached Crownhold: Crownhold's ways, at the stall");
            var slip = new Appraisal(Rarity.Common, later.Tier, ways: later.Ways);
            T.True(slip.CountsHidden && slip.MaxCount == 3, "its slip has a false-bottom row and counts to three");
            T.True(later.Cradle && !first.Cradle, "and Bram's cradle is on its counter from the first morning");
            // A first season's week at Thornwick: by hand for three days (its heaviest), the cradle on the fourth.
            var thornwick = T.Shop(2, bare: true);
            void On(int day)
            {
                typeof(GameState).GetProperty("TierSince")!.SetValue(thornwick, 6);
                typeof(GameState).GetProperty("Day")!.SetValue(thornwick, day);
            }
            On(6);
            T.True(!thornwick.Cradle, "a first season's first day at Thornwick: still by hand");
            On(8);
            T.True(!thornwick.Cradle, "and its third");
            On(9);
            T.True(thornwick.Cradle, "the cradle comes out on the fourth");
            T.True(T.Shop(3, bare: true).Cradle, "and it's on the counter at Crownhold whatever the day");
            T.True(later.Veteran && later.DrawerOpen && Curios.Drawer(later).Count == Curios.DrawerCards, "with Bram's drawer open: three curios to choose one from");
            var card = Curios.Drawer(later)[0];
            later.TakeFromDrawer(card);
            T.True(later.HasCurio(card.Key) && !later.DrawerOpen && later.CurioValue(card.Key) <= 1, "one taken: it's yours, the drawer's shut, and it's worth a coin to Nim");
        });
        yield return new("compass", "a later season's stall chests can hold what Crownhold's do: two of a kind, an enchanted thing, a false bottom, a lock", () =>
        {
            var later = GameState.NewRun(11, guided: false);
            later.Remember(new Profile { Runs = 1, BestTier = 4 });
            var first = GameState.NewRun(11, guided: false);
            int pairs = 0, magic = 0, hidden = 0, locks = 0, firstOdd = 0;
            var rng = new Random(5);
            var sellers = Sellers.All.Where(s => s.Tier == 1).ToArray();
            for (int i = 0; i < 4000; i++)
            {
                var chest = Market.CreateVisit(rng, sellers[i % sellers.Length], 1, later.Boosts).Chest;
                if (chest.Items.GroupBy(x => x.Category).Any(g => g.Count() > 1)) pairs++;
                if (chest.Items.Any(x => x.Magic)) magic++;
                if (chest.Hidden != null) hidden++;
                if (chest.Lock.Locked) locks++;
                var plain = Market.CreateVisit(rng, sellers[i % sellers.Length], 1, first.Boosts).Chest;
                if (plain.Items.GroupBy(x => x.Category).Any(g => g.Count() > 1) || plain.Items.Any(x => x.Magic) || plain.Hidden != null || plain.Lock.Locked) firstOdd++;
            }
            T.Eq(0, firstOdd, "a first season's stall chests hold none of them");
            T.True(pairs > 100 && magic > 100 && hidden > 20, $"a later season's do: {pairs} with two of a kind, {magic} with something enchanted, {hidden} with a false bottom, {locks} locked, of 4000");
        });
    }
}
