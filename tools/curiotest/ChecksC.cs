using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Keywright.Core;

namespace CurioTest;

/// <summary>
/// Audit C (tools/curiotest/AUDIT_C.md): the display, the trades, rough hands, the fence, the coffers, the patrons and
/// the Songcatcher's Net. Each check takes its numbers from the card's own words ("three-quarters more" is 1.75 here, not
/// <c>Curios.WhetstoneBonus</c>), so a card and its constant can't drift apart unnoticed. Twins (the same shop with and
/// without the curio) wherever the card names a difference; the shop has no event today (<see cref="Calm"/>), so a sum
/// is the curio's alone.
/// </summary>
public static class ChecksC
{
    public static IEnumerable<Check> Checks()
    {
        // ---- Display ----

        yield return new("book", "every night, visitors leave 6g per exhibition point (x2 at Thornwick, x4 in Crownhold, x7 at the Emporium)", () =>
        {
            foreach (int tier in new[] { 2, 3, 4 })
            {
                var (without, with) = Pair("book", tier);
                foreach (var s in new[] { without, with }) Show(s, T.I(Category.Garb, Rarity.Rare), T.I(Category.Gems, Rarity.Uncommon));
                T.Eq(6, with.Exhibition, $"tier {tier}: exhibition points (a rare cloak and an uncommon gem)");
                for (int night = 1; night <= 3; night++)
                    T.Eq(6 * ShopTiers.Scale(tier) * 6, Night(with) - Night(without), $"tier {tier}, night {night}: visitors' coins");
            }
        });

        yield return new("book", "the visitors' coins are the day end's \"Curios\" figure: 6g x the shop's scale x the points (not 6g x the points)", () =>
        {
            var (_, with) = Pair("book", 3);
            Show(with, T.I(Category.Garb, Rarity.Rare), T.I(Category.Gems, Rarity.Uncommon));
            with.EndDay();
            // DayEndPhase.cs:348 tells the player "Visitors leave {Curios.BookPerPoint * score}g a night": 36g here.
            T.Eq(6 * 4 * 6, with.Today.Curios, "what the night brought in, in Crownhold (the day end's own line says 36g)");
        });

        yield return new("book", "nothing on display, nothing left (and nothing at all without the book)", () =>
        {
            var (without, with) = Pair("book", 2);
            T.Eq(0, Night(with) - Night(without), "an empty shelf");
        });

        yield return new("rope", "your exhibition counts double (and what it draws: finer chests, a customer, visitors' coins)", () =>
        {
            var (without, with) = Pair("rope", 2, others: "book");
            // 4 + 2 + 1 + 1 = 8 points: under the customer's 15, so only the rope's double draws one.
            foreach (var s in new[] { without, with })
                Show(s, T.I(Category.Garb, Rarity.Rare), T.I(Category.Gems, Rarity.Uncommon), T.I(Category.Arms), T.I(Category.Coins));
            T.Eq(8, without.Exhibition, "exhibition without the rope");
            T.Eq(16, with.Exhibition, "exhibition with the rope");
            Within(2 * without.BetterChests, with.BetterChests, 0.0001, "the chance of a finer chest");
            T.Eq(without.ExhibitionCustomers + 1, with.ExhibitionCustomers, "customers drawn by the exhibition");
            T.Eq(2 * Night(without), Night(with), "visitors' coins (with a visitors' book)");
        });

        yield return new("rope", "doubled with everything else that counts points: an enchanted thing, a tailor's dummy's cloak", () =>
        {
            var (without, with) = Pair("rope", 2, others: "dummy");
            foreach (var s in new[] { without, with }) Show(s, T.I(Category.Garb, Rarity.Uncommon), T.I(Category.Gems, Rarity.Common, magic: true));
            // A cloak counts 3 times over (2 x 3), an enchanted thing double (1 x 2).
            T.Eq(8, without.Exhibition, "exhibition without the rope");
            T.Eq(16, with.Exhibition, "exhibition with the rope");
        });

        yield return new("lamps", "everything sells for 1.5% more per exhibition point: 20 points, 30% more, every kind", () =>
        {
            var (without, with) = Pair("lamps", 2);
            foreach (var s in new[] { without, with }) Show(s, T.I(Category.Gems, Rarity.Legendary), T.I(Category.Garb, Rarity.Rare));
            T.Eq(20, with.Exhibition, "exhibition");
            foreach (var kind in Kinds)
            {
                var item = T.I(kind, Rarity.Epic);
                T.Near(without.SaleValue(item) * 1.3, with.SaleValue(item), $"{kind}, sold from the shelf");
            }
            // And out of a chest bought and opened, as the opening sells it.
            var a = Sold(without, Rarity.Rare, T.I(Category.Arms, Rarity.Rare), T.I(Category.Potions, Rarity.Rare));
            var b = Sold(with, Rarity.Rare, T.I(Category.Arms, Rarity.Rare), T.I(Category.Potions, Rarity.Rare));
            for (int i = 0; i < a.Count; i++) T.Near(a[i].Value * 1.3, b[i].Value, $"{a[i].Item.Category}, out of a chest");
        });

        yield return new("lamps", "up to 60%: 40 points and past them, no more", () =>
        {
            foreach (int legends in new[] { 3, 4 })
            {
                var (without, with) = Pair("lamps", 2);
                foreach (var s in new[] { without, with })
                    Show(s, Enumerable.Range(0, legends).Select(_ => T.I(Category.Gems, Rarity.Legendary)).ToArray());
                var item = T.I(Category.Coins, Rarity.Epic);
                T.Near(without.SaleValue(item) * 1.6, with.SaleValue(item), $"{with.Exhibition} points");
            }
        });

        // ---- The trades ----

        foreach (var (curio, kind, name) in new[] { ("gauntlet", Category.Arms, "weapons"), ("cap", Category.Garb, "cloaks") })
        {
            yield return new(curio, $"mends {name}: never worse than their chest promised (a blue chest's green and wood ones come out blue)", () =>
            {
                var (without, with) = Pair(curio);
                var other = kind == Category.Arms ? Category.Garb : Category.Arms;
                for (int i = 0; i < 40; i++)
                {
                    var plain = Opened(without, Rarity.Rare, T.I(kind), T.I(kind, Rarity.Uncommon), T.I(other));
                    var mended = Opened(with, Rarity.Rare, T.I(kind), T.I(kind, Rarity.Uncommon), T.I(other));
                    T.True(plain.Items.Where(x => x.Category == kind).All(x => x.Quality < Rarity.Rare), $"without the {curio}, {name} stay as they were");
                    T.True(mended.Items.Where(x => x.Category == kind).All(x => x.Quality >= Rarity.Rare), $"with the {curio}, every one of the {name} comes out blue or better");
                    T.True(mended.Items.Where(x => x.Category == other).All(x => x.Quality == Rarity.Common), $"the {curio} mends {name} only");
                }
            });

            yield return new(curio, $"{name} are 25% likelier to come out better (a one-in-four chance of a step, on top of the chest's own roll); other kinds aren't", () =>
            {
                var (without, with) = Pair(curio);
                var other = kind == Category.Arms ? Category.Garb : Category.Arms;
                foreach (var colour in new[] { Rarity.Common, Rarity.Rare })
                {
                    Within(0, Stepped(without, kind, colour), 0.001, $"{name} in a {colour} chest, without");
                    Within(0.25, Stepped(with, kind, colour), 0.025, $"{name} in a {colour} chest, with the {curio}");
                    Within(0, Stepped(with, other, colour), 0.001, $"{other} in a {colour} chest, with the {curio}");
                }
            });

            yield return new(curio, $"the opening's \"{Curios.Get(curio).Name}: n mended\" counts {name} only (with the glove and the cap both)", () =>
            {
                var s = Calm(T.Shop(curios: new[] { "gauntlet", "cap" }));
                var chest = T.Chest(Rarity.Rare, T.I(Category.Arms, Rarity.Uncommon), T.I(Category.Garb, Rarity.Uncommon));
                var opened = T.BuyAndOpen(s, T.Visit(s, "garrick", chest, ask: 1, trade: Trade.Fixed), T.Exact(s, chest));
                var line = opened.Purchase.Luck.FirstOrDefault(l => l.Curio == curio);
                T.Eq(1, line.Changed, $"what the opening says the {Curios.Get(curio).Name} mended (one {kind} was below blue; the other thing mended was the other curio's)");
            });
        }

        yield return new("whetstone", "weapons sell for three-quarters more; nothing else does", () =>
        {
            var (without, with) = Pair("whetstone");
            foreach (var kind in Kinds)
            {
                var item = T.I(kind, Rarity.Rare);
                T.Near(without.SaleValue(item) * (kind == Category.Arms ? 1.75 : 1), with.SaleValue(item), $"{kind}");
            }
            var a = Sold(without, Rarity.Rare, T.I(Category.Arms, Rarity.Rare), T.I(Category.Arms, Rarity.Epic), T.I(Category.Coins, Rarity.Rare));
            var b = Sold(with, Rarity.Rare, T.I(Category.Arms, Rarity.Rare), T.I(Category.Arms, Rarity.Epic), T.I(Category.Coins, Rarity.Rare));
            for (int i = 0; i < a.Count; i++)
                T.Near(a[i].Value * (a[i].Item.Category == Category.Arms ? 1.75 : 1), b[i].Value, $"{a[i].Item.Name}, out of a chest");
        });

        yield return new("dummy", "cloaks sell for half as much again; nothing else does", () =>
        {
            var (without, with) = Pair("dummy");
            foreach (var kind in Kinds)
            {
                var item = T.I(kind, Rarity.Rare);
                T.Near(without.SaleValue(item) * (kind == Category.Garb ? 1.5 : 1), with.SaleValue(item), $"{kind}");
            }
            var a = Sold(without, Rarity.Rare, T.I(Category.Garb, Rarity.Rare), T.I(Category.Gems, Rarity.Rare));
            var b = Sold(with, Rarity.Rare, T.I(Category.Garb, Rarity.Rare), T.I(Category.Gems, Rarity.Rare));
            for (int i = 0; i < a.Count; i++)
                T.Near(a[i].Value * (a[i].Item.Category == Category.Garb ? 1.5 : 1), b[i].Value, $"{a[i].Item.Name}, out of a chest");
        });

        yield return new("dummy", "cloaks count 3 times over on display (an enchanted one too); other things as they were", () =>
        {
            var (without, with) = Pair("dummy", 2);
            foreach (var s in new[] { without, with })
                Show(s, T.I(Category.Garb, Rarity.Rare), T.I(Category.Garb, Rarity.Common, magic: true), T.I(Category.Gems, Rarity.Uncommon));
            // 4 + 2 + 2 points; with the dummy 4x3 + 2x3 + 2.
            T.Eq(8, without.Exhibition, "exhibition without the dummy");
            T.Eq(20, with.Exhibition, "exhibition with the dummy");
        });

        yield return new("scales", "coin sells for a fifth more for every purse of it in the chest (one, two, three purses)", () =>
        {
            for (int n = 1; n <= 3; n++)
            {
                var (without, with) = Pair("scales");
                var a = Sold(without, Rarity.Rare, Enumerable.Range(0, n).Select(_ => T.I(Category.Coins, Rarity.Rare)).Append(T.I(Category.Arms, Rarity.Rare)).ToArray());
                var b = Sold(with, Rarity.Rare, Enumerable.Range(0, n).Select(_ => T.I(Category.Coins, Rarity.Rare)).Append(T.I(Category.Arms, Rarity.Rare)).ToArray());
                for (int i = 0; i < a.Count; i++)
                    T.Near(a[i].Value * (a[i].Item.Category == Category.Coins ? 1 + 0.2 * n : 1), b[i].Value, $"{n} purses: {a[i].Item.Name}");
            }
        });

        yield return new("scales", "demon gold isn't a purse of coin: two real purses and a fake sell as two", () =>
        {
            var (without, with) = Pair("scales");
            var b = Sold(with, Rarity.Rare, T.I(Category.Coins, Rarity.Rare), T.I(Category.Coins, Rarity.Rare), T.I(Category.Coins, Rarity.Rare, fake: true));
            var a = Sold(without, Rarity.Rare, T.I(Category.Coins, Rarity.Rare), T.I(Category.Coins, Rarity.Rare), T.I(Category.Coins, Rarity.Rare, fake: true));
            for (int i = 0; i < a.Count; i++)
                if (!a[i].Item.Fake) T.Near(a[i].Value * 1.4, b[i].Value, "a real purse beside a fake");
        });

        yield return new("still", "potions sell for half as much again; nothing else does", () =>
        {
            var (without, with) = Pair("still");
            foreach (var kind in Kinds)
            {
                var item = T.I(kind, Rarity.Rare);
                T.Near(without.SaleValue(item) * (kind == Category.Potions ? 1.5 : 1), with.SaleValue(item), $"{kind}");
            }
        });

        yield return new("still", "broken glass still sells for half: something, where it used to fetch nothing", () =>
        {
            var (without, with) = Pair("still");
            var broken = new Item(Category.Potions, Rarity.Rare, Broken: true);
            T.Eq(0, without.SaleValue(broken), "a broken potion, without the still");
            T.True(with.SaleValue(broken) > 0, "a broken potion sells for something with the still");
        });

        yield return new("still", "broken glass sells for half of what that potion fetches here (with the still's own half again)", () =>
        {
            var (_, with) = Pair("still");
            var potion = T.I(Category.Potions, Rarity.Rare);
            // Broken by the crowbar out of a chest bought and opened, as the opening sells it.
            var chest = T.Chest(Rarity.Rare, potion, T.I(Category.Arms, Rarity.Rare));
            chest.Force(with.Rng);
            var opened = T.BuyAndOpen(with, T.Visit(with, "garrick", chest, ask: 1, trade: Trade.Fixed), T.Exact(with, chest));
            var broken = chest.Everything.Single(i => i.Category == Category.Potions);
            T.True(broken.Broken, "the crowbar broke the potion");
            int half = (int)MathF.Round(with.SaleValue(potion) / 2f), sold = with.SaleValue(broken, PickGrade.Clean, opened.Purchase);
            if (Math.Abs(sold - half) > 1)
                throw new Failed($"a broken rare potion sold for {sold}g; an unbroken one sells here for {with.SaleValue(potion)}g with the still, so half is {half}g " +
                                 $"(it gets half its plain worth, {Categories.Value(Category.Potions, Rarity.Rare, false)}g / 2: the still's x1.5, a synergy and the lamps skip broken things)");
        });

        yield return new("desk", "spells sell for half as much again; nothing else does", () =>
        {
            var (without, with) = Pair("desk");
            foreach (var kind in Kinds)
            {
                var item = T.I(kind, Rarity.Rare);
                T.Near(without.SaleValue(item) * (kind == Category.Scrolls ? 1.5 : 1), with.SaleValue(item), $"{kind}");
            }
        });

        yield return new("desk", "sand sells as ink, where it used to fetch a coin; demon gold and paste don't", () =>
        {
            var (without, with) = Pair("desk");
            var sand = T.I(Category.Scrolls, Rarity.Rare, fake: true);
            T.Eq(1, without.SaleValue(sand), "sand without the desk");
            T.True(with.SaleValue(sand) > 1, "sand with the desk");
            T.Eq(without.SaleValue(T.I(Category.Gems, Rarity.Rare, fake: true)), with.SaleValue(T.I(Category.Gems, Rarity.Rare, fake: true)), "paste");
        });

        yield return new("desk", "sand sells for a third of a spell (as a spell sells here, with the desk's own half again)", () =>
        {
            var (_, with) = Pair("desk");
            var a = Sold(with, Rarity.Rare, T.I(Category.Scrolls, Rarity.Rare), T.I(Category.Scrolls, Rarity.Rare, fake: true));
            int spell = a.Single(x => !x.Item.Fake).Value, sand = a.Single(x => x.Item.Fake).Value;
            if (Math.Abs(sand - spell / 3f) > 1)
                throw new Failed($"a rare scroll case of sand sold for {sand}g beside a rare spell at {spell}g: a third of the spell is {spell / 3f:0.#}g " +
                                 $"(it gets a third of the spell's plain worth, {Categories.Value(Category.Scrolls, Rarity.Rare, false)}g / 3)");
        });

        yield return new("wheel", "gems are 50% likelier to come out a step better (a coin-flip's step, on top of the chest's own roll); other kinds aren't", () =>
        {
            var (without, with) = Pair("wheel", 3);
            foreach (var colour in new[] { Rarity.Common, Rarity.Rare })
            {
                Within(0, Stepped(without, Category.Gems, colour), 0.001, $"gems in a {colour} chest, without");
                Within(0.5, Stepped(with, Category.Gems, colour), 0.03, $"gems in a {colour} chest, with the wheel");
                Within(0, Stepped(with, Category.Coins, colour), 0.001, $"coin in a {colour} chest, with the wheel");
            }
            // In a purple chest luck can't make gold: the step makes it shine instead (the glossary's "a step better").
            Within(0.5, Stepped(with, Category.Gems, Rarity.Epic), 0.03, "gems in a purple chest come out enchanted");
        });

        // ---- Rough hands ----

        yield return new("oil", "every lock has one pin fewer (the lock you pick and the one the counter names); no lock stays no lock", () =>
        {
            var (without, with) = Pair("oil", 2);
            foreach (var lockTier in LockTiers.All)
            {
                T.Eq(lockTier.Pins, without.Oiled(lockTier).Pins, $"{lockTier.Name} without the oil");
                T.Eq(lockTier.Locked ? lockTier.Pins - 1 : 0, with.Oiled(lockTier).Pins, $"{lockTier.Name} with the oil");
            }
            // A wet year's extra pin, and the oil's one fewer.
            with.Banes.Add(Challenges.Rust);
            T.Eq(LockTiers.All[2].Pins, with.Oiled(LockTiers.All[2]).Pins, "a rusty year, oiled");
        });

        yield return new("knife", "forcing a lock scratches nothing: forced, everything sells as for a clean pick", () =>
        {
            var (without, with) = Pair("knife");
            foreach (var kind in Kinds)
            {
                var item = T.I(kind, Rarity.Rare);
                T.Eq(with.SaleValue(item, PickGrade.Clean), with.SaleValue(item, PickGrade.Forced), $"{kind}, forced with the knife");
                T.Near(without.SaleValue(item, PickGrade.Clean) * 0.9, without.SaleValue(item, PickGrade.Forced), $"{kind}, forced without it (-10%)");
            }
        });

        yield return new("knife", "forcing a lock breaks nothing: the glass comes through the crowbar", () =>
        {
            var (without, with) = Pair("knife", 2);
            for (int i = 0; i < 30; i++)
            {
                foreach (var (s, knife) in new[] { (without, false), (with, true) })
                {
                    var chest = T.Chest(Rarity.Rare, T.I(Category.Potions, Rarity.Rare), T.I(Category.Potions), T.I(Category.Arms));
                    var opened = T.BuyAndOpen(s, T.Visit(s, "garrick", chest, ask: 1, trade: Trade.Fixed), T.Exact(s, chest), PickGrade.Forced);
                    // As the lock's phase (and the sim) force it: the chest's crowbar, at the shop's sturdiness.
                    var broke = chest.Force(s.Rng, s.ForceSturdier);
                    T.Eq(knife ? 0 : 2, broke.Count, knife ? "glass broken by the crowbar, with the knife" : "glass broken by the crowbar, without it");
                }
            }
        });

        yield return new("gloves", "nobody minds a hard shake, a drop or the lodestone (Cadoc, Brass, the Countess), and the chest's terms don't say so", () =>
        {
            var (without, with) = Pair("gloves", 4);
            var cadoc = Sellers.Get("cadoc");
            var brass = Sellers.Get("brass");
            var vey = Sellers.Get("vey");
            T.True(without.MindsHardShake(cadoc) && without.MindsDrop(brass) && without.MindsMagnet(vey), "without the gloves, each minds their own");
            T.True(!with.MindsHardShake(cadoc), "Cadoc minds a hard shake, with the gloves");
            T.True(!with.MindsDrop(brass), "Brass minds a drop, with the gloves");
            T.True(!with.MindsMagnet(vey), "the Countess minds the lodestone, with the gloves");
            foreach (var key in new[] { "cadoc", "brass", "vey" })
            {
                string plain = without.Terms(T.Visit(without, key, T.Chest(Rarity.Rare, T.I(Category.Coins)), ask: 10));
                string gloved = with.Terms(T.Visit(with, key, T.Chest(Rarity.Rare, T.I(Category.Coins)), ask: 10));
                T.True(plain?.Contains("Minds") == true, $"{key}'s terms say what they mind, without the gloves");
                T.True(gloved?.Contains("Minds") != true, $"{key}'s terms with the gloves: \"{gloved}\"");
            }
        });

        yield return new("gloves", "nobody minds, even in a year of fussy sellers (everyone minds a drop)", () =>
        {
            var (without, with) = Pair("gloves", 2);
            without.Banes.Add(Challenges.FussySellers);
            with.Banes.Add(Challenges.FussySellers);
            var garrick = Sellers.Get("garrick");
            T.True(without.MindsDrop(garrick), "a fussy year, without the gloves");
            T.True(!with.MindsDrop(garrick), "a fussy year, with the gloves");
        });

        // ---- The fence ----

        yield return new("loupe", "paste gems and demon gold sell for a quarter of the real thing (in a plain shop); sand doesn't", () =>
        {
            var (without, with) = Pair("loupe", 4);
            foreach (var kind in new[] { Category.Gems, Category.Coins })
                foreach (var quality in new[] { Rarity.Rare, Rarity.Epic })
                {
                    var real = T.I(kind, quality);
                    var fake = T.I(kind, quality, fake: true);
                    T.Eq(1, without.SaleValue(fake), $"a {quality} fake {kind} without the loupe");
                    T.Near(with.SaleValue(real) / 4.0, with.SaleValue(fake), $"a {quality} fake {kind} with the loupe");
                }
            T.Eq(1, with.SaleValue(T.I(Category.Scrolls, Rarity.Rare, fake: true)), "sand with the loupe");
        });

        yield return new("loupe", "a quarter of the real thing as it sells here: with gallery lamps lit (20 points, +30%)", () =>
        {
            var (_, with) = Pair("loupe", 4, others: "lamps");
            Show(with, T.I(Category.Gems, Rarity.Legendary), T.I(Category.Garb, Rarity.Rare));
            var a = Sold(with, Rarity.Epic, T.I(Category.Gems, Rarity.Epic), T.I(Category.Gems, Rarity.Epic, fake: true));
            int real = a.Single(x => !x.Item.Fake).Value, paste = a.Single(x => x.Item.Fake).Value;
            if (Math.Abs(paste - real / 4f) > 1)
                throw new Failed($"a purple paste gem sold for {paste}g beside the real one at {real}g: a quarter of it is {real / 4f:0.#}g (the lamps skip fakes)");
        });

        yield return new("mark", "crooks (Tilly, the Countess, Gilt: those who deal in fakes) come by more often", () =>
        {
            var report = new List<string>();
            bool ok = true;
            foreach (int tier in new[] { 3, 4 })
            {
                double plain = CrooksPerDay(tier, mark: false), marked = CrooksPerDay(tier, mark: true), ratio = marked / plain;
                report.Add($"{ShopTiers.Name(tier)}: {plain:0.###} crooks a day without the mark, {marked:0.###} with it: x{ratio:0.##}");
                ok &= ratio >= 1.15;
            }
            if (!ok) throw new Failed(string.Join("; ", report));
        });

        yield return new("mark", "any fake sells for what the real thing fetches (a plain shop): paste, demon gold and sand", () =>
        {
            var (without, with) = Pair("mark", 4);
            foreach (var kind in new[] { Category.Gems, Category.Coins, Category.Scrolls })
            {
                var a = Sold(with, Rarity.Epic, T.I(kind, Rarity.Epic), T.I(kind, Rarity.Epic, fake: true));
                T.Eq(a.Single(x => !x.Item.Fake).Value, a.Single(x => x.Item.Fake).Value, $"a purple fake {kind} against the real one");
                T.Eq(1, without.SaleValue(T.I(kind, Rarity.Epic, fake: true)), $"a fake {kind} without the mark");
            }
        });

        yield return new("mark", "...what the real thing fetches here: with gallery lamps lit (20 points, +30%)", () =>
        {
            var (_, with) = Pair("mark", 4, others: "lamps");
            Show(with, T.I(Category.Gems, Rarity.Legendary), T.I(Category.Garb, Rarity.Rare));
            var a = Sold(with, Rarity.Epic, T.I(Category.Gems, Rarity.Epic), T.I(Category.Gems, Rarity.Epic, fake: true));
            int real = a.Single(x => !x.Item.Fake).Value, paste = a.Single(x => x.Item.Fake).Value;
            if (paste != real) throw new Failed($"a purple paste gem sold for {paste}g beside the real one at {real}g (the lamps, a synergy and the trades' curios skip fakes)");
        });

        // ---- Coffers ----

        yield return new("purse", "Bram's stash helps you out three times a run, not twice (every morning nearly broke)", () =>
        {
            var (without, with) = Pair("purse", gold: 0);
            for (int day = 0; day < 8; day++)
                foreach (var s in new[] { without, with })
                {
                    s.Gold = 0;
                    s.EndDay();
                    s.StartDay();
                }
            T.Eq(2, without.StashUsed, "times the stash helped, without the purse");
            T.Eq(3, with.StashUsed, "times the stash helped, with the purse");
        });

        yield return new("ticket", "on a short night, what you pawn fetches four-fifths of its price, not three-fifths", () =>
        {
            var (without, with) = Pair("ticket", 2);
            foreach (var (s, share) in new[] { (without, 0.6), (with, 0.8) })
            {
                Evening(s, 5);
                var item = T.I(Category.Gems, Rarity.Epic);
                Show(s, item);
                s.Gold = 0;
                T.True(s.DuesTonight && s.Gold < s.DueNow, "a short night");
                int price = s.SaleValue(item);
                T.Near(price * share, s.PawnValue(item), $"what the Guild offers, {(share > 0.7 ? "with" : "without")} the ticket");
                int pawned = s.Pawn(item);
                T.Near(price * share, pawned, "what pawning brought in");
                T.Eq(pawned, s.Gold, "the purse after pawning");
            }
        });

        yield return new("marker", "Bram's word is good twice a run, not once", () =>
        {
            var (without, with) = Pair("marker");
            foreach (var s in new[] { without, with })
            {
                Evening(s, 5);
                s.Gold = 0;
                T.True(s.CanAskGrace, "the first short night: Bram's word");
                s.AskGrace();
                s.StartDay();
                Evening(s, 10);
                s.Gold = 0;
            }
            T.True(!without.CanAskGrace, "a second short night, without the marker: no word left");
            T.True(with.CanAskGrace, "a second short night, with the marker: his word again");
            with.AskGrace();
            with.StartDay();
            Evening(with, 15);
            with.Gold = 0;
            T.True(!with.CanAskGrace, "a third short night: not three times");
        });

        yield return new("jar", "every night: 3% of your gold as interest, up to 20g at the stall (40, 80, 140g inward)", () =>
        {
            foreach (var (tier, cap) in new[] { (1, 20), (2, 40), (3, 80), (4, 140) })
                foreach (int gold in new[] { 0, 20, 34, 100, 250, 666, 1000, 2000, 4000, 4700, 100000 })
                {
                    var (without, with) = Pair("jar", tier);
                    // (Set after the first morning: Bram's stash tops up a purse that starts nearly broke.)
                    without.Gold = with.Gold = gold;
                    T.Eq(Math.Min(gold * 3 / 100, cap), Night(with) - Night(without), $"tier {tier}, {gold}g in the purse");
                }
        });

        yield return new("jar", "every night, on what's in the purse that night (a week of nights at the stall)", () =>
        {
            var (_, with) = Pair("jar", gold: 300);
            for (int night = 1; night <= 7; night++)
            {
                int gold = with.Gold;
                with.EndDay();
                T.Eq(Math.Min(gold * 3 / 100, 20), with.Gold - gold, $"night {night}, {gold}g");
                if (with.DuesTonight && !with.PaidThisWeek) PayInFull(with);
                with.StartDay();
            }
        });

        yield return new("lockbox", "every Guild night, once you've paid, your purse grows by 25% of what's left in it", () =>
        {
            var (without, with) = Pair("lockbox");
            foreach (int night in new[] { 5, 10 })
            {
                foreach (var s in new[] { without, with })
                {
                    Evening(s, night);
                    s.Gold = 1000;
                    T.True(s.PayDues(), "the due paid");
                }
                T.Eq(1000 - with.DueFor(night / 5), without.Gold, "what's left without the lockbox");
                T.Near(without.Gold * 1.25, with.Gold, $"Guild night {night / 5}: what's left, grown by a quarter");
                foreach (var s in new[] { without, with }) s.StartDay();
            }
        });

        yield return new("lockbox", "...on a Guild night when you pay all of it (the dues screen's \"Pay it all\")", () =>
        {
            var (without, with) = Pair("lockbox");
            foreach (var s in new[] { without, with })
            {
                Evening(s, 5);
                T.True(s.DuesTonight && s.CanPayOff, "a Guild night you can pay off");
                // The dues screen's "Pay it all" (GameState.PayAll: tonight's due, then the rest).
                T.True(s.PayAll() > 0, "paid off");
            }
            // The lockbox grew what was left after tonight's due by a quarter, before the rest of the debt was paid.
            if (with.Gold <= without.Gold)
                throw new Failed($"paying all of it on a Guild night left {without.Gold}g without the lockbox and {with.Gold}g with it: the lockbox added nothing");
        });

        yield return new("note", "once a run, on a Guild night you can't cover, it pays what's short (and the due is paid)", () =>
        {
            var s = Calm(T.Shop(curios: new[] { "note" }));
            Evening(s, 10);
            s.Gold = 10;
            T.True(!s.CanCover, "nothing to pawn but the note's own half (25g) against 60g");
            T.True(s.CanCallNote, "the note can be called");
            int due = s.DueNow, paid = s.CallNote();
            T.Eq(due - 10, paid, "what was short");
            T.True(s.PaidThisWeek, "the week's due is paid");
            T.Eq(0, s.Gold, "the purse after (it paid what was short, not more)");
            s.StartDay();
            Evening(s, 15);
            s.Gold = 10;
            T.True(!s.CanCover, "the next week, short again");
            T.True(!s.CanCallNote, "once a run");
        });

        yield return new("note", "only a night you can't cover: not when pawning would", () =>
        {
            var s = Calm(T.Shop(2, curios: new[] { "note" }, bare: true));
            Evening(s, 10);
            Show(s, T.I(Category.Gems, Rarity.Epic));
            s.Gold = 10;
            T.True(s.CanCover, "a purple gem on the shelf covers it");
            T.True(!s.CanCallNote, "the note isn't for a night pawning covers");
        });

        yield return new("note", "not the last week (the seventh)", () =>
        {
            var s = Calm(T.Shop(curios: new[] { "note" }));
            Evening(s, 35);
            s.Gold = 0;
            T.True(!s.CanCover && s.Week == Guild.Weeks, "short in the last week");
            T.True(!s.CanCallNote, "the note in the last week");
        });

        yield return new("note", "a Guild night after the debt (the Guild's interest, week 8 on) isn't the last week: it pays", () =>
        {
            var s = Calm(T.Shop(curios: new[] { "note" }));
            Evening(s, 35);
            PayInFull(s);
            T.True(s.DebtCleared, "the debt cleared in week 7");
            s.StartDay();
            Evening(s, 40);
            s.Gold = 0;
            T.True(s.DuesTonight && !s.PaidThisWeek && !s.CanCover, "week 8: the Guild's interest, and nothing to cover it");
            T.True(s.CanCallNote, "the note on a short night in week 8 (CanCallNote requires Week < Guild.Weeks, so it's never offered after the debt)");
        });

        // ---- Patrons ----

        yield return new("letter", "regulars take a quality lower than they asked for (one, not two), of the kind they asked for", () =>
        {
            var (without, with) = Pair("letter");
            var piece = new Commission { Patron = "garrick", Kind = CommissionKind.Piece, Category = Category.Arms, Quality = Rarity.Rare, Reward = 40 };
            var lot = new Commission { Patron = "ada", Kind = CommissionKind.Lot, Category = Category.Garb, Quality = Rarity.Uncommon, Needed = 2, Reward = 30 };
            T.True(!without.Takes(piece, T.I(Category.Arms, Rarity.Uncommon)), "a green weapon for 'rare or better', without the letter");
            T.True(with.Takes(piece, T.I(Category.Arms, Rarity.Uncommon)), "a green weapon for 'rare or better', with the letter");
            T.True(!with.Takes(piece, T.I(Category.Arms, Rarity.Common)), "two qualities lower");
            T.True(!with.Takes(piece, T.I(Category.Garb, Rarity.Rare)), "the wrong kind");
            T.True(with.Takes(lot, T.I(Category.Garb, Rarity.Common)), "a lot: a wood cloak for 'uncommon or better'");
            with.Commissions.Add(piece);
            T.True(with.WantedBy(T.I(Category.Arms, Rarity.Uncommon)) == piece, "the opening offers it for the commission");
            T.True(with.Deliver(T.I(Category.Arms, Rarity.Uncommon), piece) > 0 && piece.Done, "and the delivery is taken and paid");
        });

        yield return new("letter", "only regulars: Lord Harrow's crier (a day's reward, not a regular) wants what he asked for", () =>
        {
            var (without, with) = Pair("letter");
            var crier = Events.Reward(Category.Arms, 1, with.Day);
            T.Eq(Events.Crier, crier.Patron, "the crier's reward");
            T.True(!without.Takes(crier, T.I(Category.Arms, Rarity.Common)), "a wood weapon for the crier's 'uncommon or better', without the letter");
            T.True(!with.Takes(crier, T.I(Category.Arms, Rarity.Common)),
                "the crier took a wood weapon for 'uncommon or better' with the letter: LetterLeeway gives every commission a quality's leeway, not just the regulars'");
        });

        yield return new("ribbon", "the reward each commission posts is half as much again (the board, a piece and a lot)", () =>
        {
            var (without, with) = Pair("ribbon");
            foreach (var c in new[]
                     {
                         new Commission { Patron = "garrick", Kind = CommissionKind.Piece, Category = Category.Arms, Quality = Rarity.Uncommon, Reward = 20 },
                         new Commission { Patron = "ada", Kind = CommissionKind.Lot, Category = Category.Garb, Quality = Rarity.Common, Needed = 2, Reward = 18 },
                     })
            {
                T.Near(without.CommissionReward(c) * 1.5, with.CommissionReward(c), $"{c.Kind}: the reward");
                T.Near(without.CommissionEach(c) * 1.5, with.CommissionEach(c), $"{c.Kind}: each thing's share", 0.1);
            }
        });

        yield return new("ribbon", "a delivery pays half as much again: a weapon just as asked, at the stall", () =>
        {
            var (without, with) = Pair("ribbon");
            var (a, b) = Delivered(without, with, T.I(Category.Arms, Rarity.Uncommon), Rarity.Uncommon, 20);
            T.Near(a * 1.5, b, $"paid {a}g without, {b}g with");
        });

        yield return new("ribbon", "a delivery pays half as much again: a better weapon than asked (blue for 'green or better')", () =>
        {
            var (without, with) = Pair("ribbon");
            var (a, b) = Delivered(without, with, T.I(Category.Arms, Rarity.Rare), Rarity.Uncommon, 20);
            if (Math.Abs(b - a * 1.5) > 1)
                throw new Failed($"paid {a}g without the ribbon and {b}g with it: x{b / (double)a:0.##}, not x1.5 (the pay is the most of the reward and a quarter over " +
                                 $"the sale, {Math.Ceiling(with.SaleValue(T.I(Category.Arms, Rarity.Rare)) * 1.25)}g; the ribbon raises only the reward)");
        });

        yield return new("ribbon", "a delivery pays half as much again: at the Emporium with its tools and the Royal Warrant, a purple gem as asked", () =>
        {
            var (without, with) = Pair("ribbon", 4, bare: false);
            foreach (var s in new[] { without, with }) s.Grant("warrant");
            var gem = T.I(Category.Gems, Rarity.Epic);
            var (a, b) = Delivered(without, with, gem, Patrons.Asked(4), Money.Round(Patrons.PieceReward * Categories.Nominal(Category.Gems, Patrons.Asked(4))));
            if (Math.Abs(b - a * 1.5) > 1)
                throw new Failed($"paid {a}g without the ribbon and {b}g with it: x{b / (double)a:0.##}, not x1.5 (it sells for {with.SaleValue(gem)}g here, so a quarter " +
                                 $"over that, {Math.Ceiling(with.SaleValue(gem) * 1.25)}g, beats the reward without the ribbon)");
        });

        yield return new("errand", "the regulars post 2 more commissions every week (Thornwick, Crownhold, the Emporium)", () =>
        {
            foreach (int tier in new[] { 2, 3, 4 })
                for (int seed = 1; seed <= 25; seed++)
                {
                    var (without, with) = Pair("errand", tier, bare: false);
                    int a = Patrons.Roll(new Random(seed), without).Count, b = Patrons.Roll(new Random(seed), with).Count;
                    T.Eq(a + 2, b, $"tier {tier}, roll {seed}: commissions posted");
                }
        });

        yield return new("errand", "on the week's first morning, as the board fills (the second week, at Thornwick)", () =>
        {
            var (without, with) = Pair("errand", 2, bare: false);
            foreach (var s in new[] { without, with })
            {
                Evening(s, 5);
                PayInFull(s);
                s.StartDay();
            }
            T.Eq(2, s0(without), "the regulars' commissions, week 2, without the book");
            T.Eq(4, s0(with), "the regulars' commissions, week 2, with the book");

            static int s0(GameState s) => s.Commissions.Count(c => c.Patron != Events.Crier);
        });

        yield return new("errand", "2 more with the Clockwork Lark singing at Thornwick too (four kinds of regulars' asks, five kinds of thing)", () =>
        {
            var (without, with) = Pair("errand", 2, bare: false);
            foreach (var s in new[] { without, with }) s.TakeSong(Heartsongs.Make(Heartsongs.Get(Heartsongs.Lark), Rarity.Common));
            int a = Patrons.Roll(new Random(3), without).Count, b = Patrons.Roll(new Random(3), with).Count;
            if (b != a + 2)
                throw new Failed($"{a} commissions with the Lark, {b} with the Lark and the book: one commission a kind of thing, and Thornwick has five kinds");
        });

        yield return new("net", "Heartsongs are three times as likely to be in a chest", () =>
        {
            float chance = Heartsongs.Chance;
            try
            {
                // A song in one chest in twenty, so the count is big enough to compare (the game's is one in 600).
                Heartsongs.Chance = 0.05f;
                var (without, with) = Pair("net");
                int a = Songs(without, 10000), b = Songs(with, 10000);
                Within(3, b / (double)a, 0.4, $"{a} songs in 10,000 chests without the net, {b} with it");
            }
            finally
            {
                Heartsongs.Chance = chance;
            }
        });
    }

    // ---- Helpers ----

    static readonly Category[] Kinds = Categories.Kinds.Select(k => k.Category).ToArray();

    /// <summary>No event today (a festival puts every sale up, a storm brightens chests): the sums are the curio's alone.</summary>
    static GameState Calm(GameState s)
    {
        s.ForcedEvent = "calm";
        return s;
    }

    /// <summary>The same shop without and with the curio, at this tier (bare: no tools, unless asked), on a calm day.</summary>
    static (GameState Without, GameState With) Pair(string curio, int tier = 1, int gold = 100000, bool bare = true, params string[] others)
    {
        var (a, b) = T.Twins(curio, tier, gold, 7, Characters.Kit, bare, others);
        return (Calm(a), Calm(b));
    }

    static void Show(GameState s, params Item[] items) => s.Display.AddRange(items);

    /// <summary>Pays tonight's due in full (from a purse topped up for it, then put back as it was).</summary>
    static void PayInFull(GameState s)
    {
        int gold = s.Gold;
        s.Gold = s.DueNow;
        if (!s.PayDues()) throw new Failed("the due couldn't be paid");
        s.Gold = gold;
    }

    /// <summary>
    /// Plays on from this morning to the evening of <paramref name="day"/> (its day's end done, its Guild night still to
    /// come), each Guild night before it paid in full.
    /// </summary>
    static void Evening(GameState s, int day)
    {
        while (s.Day < day)
        {
            s.EndDay();
            if (s.DuesTonight && !s.PaidThisWeek) PayInFull(s);
            s.StartDay();
            s.ForcedEvent = "calm";
        }
        s.EndDay();
    }

    /// <summary>One night (the day's end and its Guild night, paid in full) and the next morning: what the night brought in.</summary>
    static int Night(GameState s)
    {
        int gold = s.Gold;
        s.EndDay();
        int got = s.Gold - gold;
        if (s.DuesTonight && !s.PaidThisWeek) PayInFull(s);
        s.Gold = gold;
        s.StartDay();
        s.Gold = gold;
        return got;
    }

    /// <summary>Buys a chest of these things (fixed price, an exact slip) and opens it as the game does: what each thing sold for.</summary>
    static List<(Item Item, int Value)> Sold(GameState s, Rarity colour, params Item[] items)
    {
        var chest = T.Chest(colour, items);
        var opened = T.BuyAndOpen(s, T.Visit(s, "garrick", chest, ask: 1, trade: Trade.Fixed), T.Exact(s, chest));
        NoDrySpell(s);
        return chest.Everything.Select(i => (i, s.SaleValue(i, PickGrade.Clean, opened.Purchase))).ToList();
    }

    /// <summary>Buys a chest of these things and opens it: the chest as it came out.</summary>
    static Chest Opened(GameState s, Rarity colour, params Item[] items)
    {
        var chest = T.Chest(colour, items);
        T.BuyAndOpen(s, T.Visit(s, "garrick", chest, ask: 1, trade: Trade.Fixed), T.Exact(s, chest));
        NoDrySpell(s);
        return chest;
    }

    /// <summary>A long dry spell lifts something at random (the pity): reset it, so a run of test chests doesn't.</summary>
    static void NoDrySpell(GameState s) => typeof(GameState).GetProperty(nameof(GameState.DrySpell))!.SetValue(s, 0);

    static readonly MethodInfo LuckRule = typeof(GameState).GetMethod("Luck", BindingFlags.NonPublic | BindingFlags.Instance, null,
        new[] { typeof(Chest), typeof(bool) }, null)!;

    /// <summary>Luck on a chest just bought (what Buy runs, before a dry spell's pity): which curio did what, to how many.</summary>
    static List<(string Curio, int Changed)> Luck(GameState s, Chest chest, bool unseen = false) =>
        (List<(string Curio, int Changed)>)LuckRule.Invoke(s, new object[] { chest, unseen })!;

    /// <summary>Of many things of this kind as good as their chest's colour, the share luck makes a step better (or shine, at purple).</summary>
    static double Stepped(GameState s, Category kind, Rarity colour, int n = 4000)
    {
        int better = 0;
        for (int i = 0; i < n; i++)
        {
            var chest = T.Chest(colour, T.I(kind, colour));
            Luck(s, chest);
            if (chest.Items[0].Quality > colour || chest.Items[0].Magic) better++;
        }
        return better / (double)n;
    }

    /// <summary>
    /// A regular's piece commission for this kind at this quality and reward, and this thing delivered to it at what it
    /// would sell for here: what each twin paid.
    /// </summary>
    static (int Without, int With) Delivered(GameState without, GameState with, Item item, Rarity asked, int reward)
    {
        int Pay(GameState s)
        {
            var c = new Commission { Patron = "garrick", Kind = CommissionKind.Piece, Category = item.Category, Quality = asked, Reward = reward };
            s.Commissions.Add(c);
            // (A first commission filled also earns the treasury key's patron piece, and its master's fee: not the ribbon's.)
            int before = s.Today.Commissions, paid = s.Deliver(item, c);
            T.Eq(before + paid, s.Today.Commissions, "the day's commission takings");
            return paid;
        }
        return (Pay(without), Pay(with));
    }

    /// <summary>How many of a day's customers deal in fakes, on average over many first days (each its own seed), at this tier.</summary>
    static double CrooksPerDay(int tier, bool mark, int days = 1500)
    {
        int crooks = 0;
        for (int seed = 1; seed <= days; seed++)
        {
            var s = T.Shop(tier, seed: seed, curios: mark ? new[] { "mark" } : Array.Empty<string>());
            crooks += s.Queue.Count(v => v.Seller.Fakes.Length > 0);
        }
        return crooks / (double)days;
    }

    /// <summary>How many of so many chests brought to the counter (GameState.NewVisit, as the day makes them) have a Heartsong in them.</summary>
    static int Songs(GameState s, int chests)
    {
        int songs = 0;
        for (int i = 0; i < chests; i++)
            if (T.NewVisit(s, "garrick").Chest.Song != null) songs++;
        return songs;
    }

    /// <summary>For shares and chances: within so much either way (T.Near's slack is for whole coins).</summary>
    static void Within(double expected, double actual, double slack, string what)
    {
        if (Math.Abs(expected - actual) > slack) throw new Failed($"{what}: expected {expected:0.###} (within {slack:0.###}), got {actual:0.###}");
    }
}
