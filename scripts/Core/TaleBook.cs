using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The tales a season can hold (docs/stories.md), beat by beat: who comes, what they say, what they ask, and what each
/// answer does. The engine (<see cref="GameState"/>'s tale half, Tales.cs) stages them; the sim's careful player answers
/// them by <see cref="Choice.Worth"/>.
/// </summary>
public static partial class TaleBook
{
    // The people tales bring (and the regulars they're about).
    public const string Pip = "pip", Ada = "ada", Bettany = "bettany", Tobin = "tobin", Nettle = "nettle", Rook = "rook", Sorrel = "sorrel",
        Cadoc = "cadoc", Corvin = "corvin", Hesper = "hesper", Flick = "flick", Hollis = "hollis", Ilse = "ilse", Orme = "orme", Durgan = "durgan",
        Isaura = "isaura", Cassian = "cassian", Brass = "brass", Quire = "quire", Rowan = "rowan", Tobias = "tobias", Aubel = "aubel", Wynn = "wynn",
        Caskett = "caskett", Hawthorn = "hawthorn", Skrae = "skrae", Tick = "tick", Aldo = "aldo", Magpie = "magpie";

    // What tales leave behind that the rest of the game reads.
    public const string Apprentice = "pip.apprentice", Porter = "kettle.porter", GateCracked = "unlock.cracked", GazeKnown = "skrae.free",
        Decree = "fore.decree";

    // 0.30: what tales leave behind (Consequences.cs): goodwill, rules, the court, the loan, and effects that last a while.
    public const string Neighbours = "pip.neighbours", CrownHome = "crown.home", GoblinFriend = "goblins.friend", AldwinHome = "field.home",
        Tolls = "reeve.tolls", MarketDays = "reeve.market", Verdict = "court.verdict", Bargain = "bargain.taken", LoanPaid = "tansy.paid",
        LoanDefaulted = "tansy.defaulted";

    // Effects that last a while (GameState.Until): to the last day each holds.
    public const string Rival = "hedge.trading", Picket = "unopened.picket", TansyMen = "tansy.men", Crowd = "hedge.crowd", Away = "away",
        Haunt = "field.haunt", FairDay = "fair.day";

    // 0.30's people (and Bram, who tells what became of everyone).
    public const string Bram = Story.Bram, Jasper = "jasper", Aldwin = "aldwin", Dunning = "dunning", Agathe = "agathe", Tansy = "tansy", Marrow = "marrow",
        Brandt = "brandt", Garrick = "garrick", Tilly = "tilly", Vey = "vey", Zell = "zell", Maw = "maw", Ashby = "ashby", Corlew = "corlew",
        Jory = "jory";

    /// <summary>A paper for the case against the Guild (<see cref="GameState.Evidence"/>), as the Town page and the court name it.</summary>
    public static string Paper(string key) => key switch
    {
        "roll" => Loc.T("the Chest Roll's copy"),
        "ledger" => Loc.T("a page of the Guild's ledger"),
        "tallies" => Loc.T("the debt-house tallies"),
        "register" => Loc.T("the Mark-Finder's register"),
        "deed" => Loc.T("the witness copy of the Wood's deed"),
        "purse" => Loc.T("the Guild's purse from the Unopened's alms box"),
        "books" => Loc.T("the Archive's ledgers, saved from the fire"),
        "notes" => Loc.T("Archmage Orme's notes"),
        "word" => Loc.T("Skrae's word"),
        _ => key,
    };

    /// <summary>The Magpie's suspects: who she might be (drawn per season), and the two things a witness could see of each.</summary>
    static readonly string[] Suspects = { Bettany, Hesper, Rook };

    static List<Line> Say(string who, params string[] lines) => lines.Select(t => new Line(who, t)).ToList();

    static Choice C(string label, Func<string> doIt, float worth = 0, int cost = 0, bool kind = false) =>
        new() { Label = label, Do = doIt, Worth = worth, Cost = cost, Kind = kind };

    /// <summary>A choice's worth: gold, and what its changes in standing are worth to the rest of the run (<see cref="Strategy.SwayWorth"/>).</summary>
    static float W(GameState s, float gold, params (Faction Faction, int By)[] sways) => gold + Strategy.SwayWorth(s, sways);

    static string Name(string person) => Sellers.Person(person).Name;

    // ---- Chests for tales ----

    /// <summary>A chest of the place's sort, at this colour: a few things, as that place would hold them.</summary>
    static Chest Fill(Random rng, string origin, Rarity colour, int tier, int count, params Category[] sure)
    {
        var kinds = Categories.UpTo(tier).Select(c => c.Category).ToList();
        var likely = Origins.Get(origin).Kinds;
        var picked = sure.Where(kinds.Contains).ToList();
        while (picked.Count < count) picked.Add(rng.PickWeighted(kinds.ToArray(), k => likely[(int)k]));
        var lockTier = tier >= 2 && rng.NextFloat() < 0.4f ? LockTiers.For(colour) : null;
        return new Chest(picked.Select(k => new Item(k, Categories.RollQuality(rng, colour))), colour, lockTier: lockTier);
    }

    static Rarity Finer(int tier, int steps) => (Rarity)Math.Min((int)Rarity.Epic, (int)ShopTiers.Typical(tier) + steps);

    /// <summary>A seller's visit with a tale's chest and their own words for it (with {price} and the like).</summary>
    static Visit Brought(GameState s, Random rng, string seller, Chest chest, string pitch, string origin) =>
        Market.Price(rng, Sellers.Get(seller), chest, s.Tier, pitch: pitch, boosts: s.Boosts, origin: Origins.Get(origin));

    /// <summary>A chest given freely, for the lucky coin: it's rightfully yours, so it opens.</summary>
    static Visit Given(GameState s, Random rng, string giver, Chest chest, string pitch, string origin)
    {
        var visit = Market.Price(rng, Sellers.Person(giver), chest, s.Tier, ask: Challenges.GiftPrice, trade: Trade.Fixed, pitch: pitch, origin: Origins.Get(origin));
        visit.Gift = true;
        return visit;
    }

    /// <summary>A chest put up to be read in front of people (<see cref="Visit.Contest"/>): given for a lucky coin, and a perfect read wins the prize.</summary>
    static Visit Contested(GameState s, Random rng, string who, Chest chest, string pitch, string origin, int prize)
    {
        var visit = Given(s, rng, who, chest, pitch, origin);
        visit.Contest = true;
        visit.Prize = prize;
        return visit;
    }

    /// <summary>The best weapon in a chest, taken out and kept for someone (a dead knight's blade), with what it would have sold for.</summary>
    static void SetAside(GameState s, Visit v, string keepsake)
    {
        if (v.Chest.Everything.Where(i => i.Category == Category.Arms && !i.Fake).OrderByDescending(i => i.Value).FirstOrDefault() is Item blade)
        {
            v.Chest.Remove(blade);
            s.SetAside[keepsake] = s.SaleValue(blade);
        }
    }

    /// <summary>Tobin's weekly chest (Tobin's Lamp, 0.32): what he's found under the Barrow, a colour finer than the shop's, given freely.</summary>
    public static Visit TobinChest(GameState s, Random rng) =>
        Given(s, rng, Tobin, Fill(rng, "barrow", Finer(s.Tier, 1), s.Tier, rng.Next(2, 4)),
            rng.PickOne(new[]
            {
                Loc.T("From the old kings' halls, Keywright, with Tobin's compliments. One coin, for luck."),
                Loc.T("Tobin sent this up from under the Barrow. Says you're to have the first look. One coin, for luck."),
            }), "barrow");

    /// <summary>Pip's weekly chest, once he's your apprentice: something he's fished out, at the shop's colour, given freely.</summary>
    public static Visit ApprenticeChest(GameState s, Random rng) =>
        Given(s, rng, Pip, Fill(rng, "millpond", ShopTiers.Typical(s.Tier), s.Tier, rng.Next(1, 4)),
            rng.PickOne(new[]
            {
                Loc.T("Fished it out for you, master! It's yours: you taught me the trade. One coin, for luck."),
                Loc.T("From the millpond, and I didn't shake it. Well, a bit. One coin, for luck, like Bram says."),
            }), "millpond");

    /// <summary>
    /// A curio as a gift (one Nim could have brought, of this rarity or the nearest): its name, or gold if there's no room.
    /// <paramref name="remember"/> notes which curio it was ("charm:" and its key), for a gift that can be taken back.
    /// </summary>
    static string GiftCurio(GameState s, Random rng, CurioRarity rarity, CurioTag? tag = null, string remember = null)
    {
        var pool = Curios.All.Where(c => !s.HasCurio(c.Key) && s.CurioOpen(c.Key) && !s.NoUse(c) && c.MinTier <= s.Tier && (tag == null || c.Has(tag.Value)))
            .OrderBy(c => Math.Abs((int)c.Rarity - (int)rarity)).ThenBy(_ => rng.Next()).ToList();
        if (s.CuriosFull || pool.Count == 0 || !s.TakesCurios)
        {
            int gold = s.Purses(0.5f);
            s.TaleGold(gold);
            return Money.Say(gold);
        }
        s.GiveCurio(pool[0].Key);
        if (remember != null) s.Flags.Add($"{remember}:{pool[0].Key}");
        return pool[0].Name;
    }

    // ---- The tales ----

    static readonly TaleDef[] Season28 =
    {
        // The Shut Chests: stolen chests, the Wardens, and who the Magpie is. Every season, from Thornwick.
        new()
        {
            Key = "magpie", Title = Loc.T("The Shut Chests"), Place = TalePlace.Always, Secrets = Suspects,
            Blurb = Loc.T("Someone is stealing chests around Thornwick: the Magpie, whom nobody's seen. Every chest she takes is one the Guild " +
                          "foreclosed on. The Wardens want a name, and witnesses will bring clues."),
            Ending = MagpieEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "magpie.notice", Who = Hollis, Tier = 2, Gap = 3, If = s => s.Day > s.TierSince,
                    Says = s => Say(Hollis,
                        Loc.T("Sergeant Hollis, Wardens of the Road, Thornwick post. Someone broke into the abbey crypt last night and took Saint " +
                              "Ide's reliquary chest: green, iron-bound, the saint's key on its lid."),
                        Loc.T("If it comes to your counter, you'll know it. A Keywright's mark goes cold on a chest that isn't the seller's to " +
                              "sell. Lay a hand on any chest and feel. Send for us, and the Wardens pay a finder's share."))
                        .Append(s.BramHere
                            ? new Line(Story.Bram, Loc.T("He's right. Shake anything, and your mark tells you whose it is: warm, the seller's; " +
                                                         "cold, stolen. A stolen chest won't open for whoever buys it."))
                            // Bram's at the treasury (0.34): you remember what he taught you.
                            : new Line(Story.Self, Loc.T("Bram taught me that: shake anything, and the mark says whose it is. Warm, the seller's; " +
                                                         "cold, stolen. A stolen chest won't open for whoever buys it."))).ToList(),
                    Then = s =>
                    {
                        s.Post("reliquary", Loc.T("Wanted: Saint Ide's reliquary"),
                            Loc.T("Stolen from the abbey crypt: a green chest, iron-bound, with the saint's key on its lid. A finder's share for " +
                                  "its return. Sergeant Hollis, Thornwick."));
                        s.Log("magpie", Loc.T("Hollis posted a notice: Saint Ide's reliquary chest has been stolen from the abbey."));
                    },
                    Asks = s => new()
                    {
                        C(Loc.T("I'll send for you."), () => { s.Sway(Faction.Wardens, 1); return Loc.T("Good. The Wardens remember their friends."); },
                            W(s, 0, (Faction.Wardens, 1)), kind: true),
                        C(Loc.T("I don't work for the Wardens."), () =>
                        {
                            s.Sway(Faction.Wardens, -1);
                            s.Sway(Faction.Lockless, 1);
                            return Loc.T("Then I'll be keeping an eye on your back room, Keywright.");
                        }, W(s, 0, (Faction.Wardens, -1), (Faction.Lockless, 1))),
                    },
                },
                new()
                {
                    Key = "magpie.reliquary", Who = Flick, After = "magpie.notice", Gap = 1, Tier = 2,
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "crypt", Rarity.Uncommon, s.Tier, 2, Category.Garb);
                        chest.Owner = Loc.T("the abbey of Saint Ide");
                        chest.Tale = "reliquary";
                        return Brought(s, rng, Flick, chest, Loc.T("A green one, iron-bound, with a pretty key on the lid. It came my way. {price}, quick."), "pocket");
                    },
                    Bought = (s, v) => s.Log("magpie", Loc.T("You bought the reliquary from Flick. It won't open: it's in your back room.")),
                    Passed = (s, v) =>
                    {
                        if (v.Outcome == VisitOutcome.Reported)
                        {
                            s.Flags.Add("reliquary.home");
                            s.Sway(Faction.Abbey, 2);
                            s.TakeDown("reliquary");
                            s.Log("magpie", Loc.T("Flick brought the reliquary, and you sent for the Wardens: it's gone home to the abbey."));
                        }
                        else
                        {
                            s.Flags.Add("reliquary.lost");
                            s.Log("magpie", Loc.T("Flick brought the reliquary, and you let it go. It's gone."));
                        }
                    },
                },
                new()
                {
                    Key = "magpie.cadoc", Who = Cadoc, After = "magpie.reliquary", Gap = 1, Tier = 2,
                    Says = s => s.Flagged("reliquary.home") || s.Flagged("returned:reliquary")
                        ? Say(Cadoc, Loc.T("Saint Ide's chest came home. The Wardens say it was your doing, Keywright. The abbey owes you, and it pays " +
                                           "its debts: our restorers' bench is yours at half the price, whenever you want it."))
                        : s.BackRoom.Any(h => h.Key == "reliquary")
                            ? Say(Cadoc, Loc.T("Flick sold you the saint's chest? Then it's in your back room, and it will never open for you. Give it " +
                                               "back to us: the abbey pays the finder's share, and blesses your door."))
                            : Say(Cadoc, Loc.T("The saint's chest is gone. Somewhere a Keywright will try to open it, and it won't, and they'll never " +
                                               "know why. Pray for whoever has it.")),
                    Asks = s => s.BackRoom.FirstOrDefault(h => h.Key == "reliquary") is Held held && !s.Flagged("reliquary.home")
                        ? new()
                        {
                            C(Loc.T($"Give it back (+{s.FinderShare}g)"), () =>
                            {
                                s.ReturnHeld(held);
                                s.Sway(Faction.Abbey, 2);
                                s.Flags.Add("abbey.bench");
                                s.TakeDown("reliquary");
                                s.Log("magpie", Loc.T("You gave the reliquary back to the abbey."));
                                return Loc.T("Bless you. The saint will remember, and so will we.");
                            }, W(s, s.FinderShare, (Faction.Wardens, 1), (Faction.Abbey, 2)), kind: true),
                            C(Loc.T("Keep it"), () =>
                            {
                                s.Sway(Faction.Abbey, -2);
                                return Loc.T("Then may it sit in the dark with you, Keywright.");
                            }, W(s, s.FencePay(held), (Faction.Abbey, -2))),
                        }
                        : new(),
                    Then = s =>
                    {
                        if (s.Flagged("reliquary.home"))
                        {
                            s.Sway(Faction.Abbey, 1);
                            s.Flags.Add("abbey.bench");
                        }
                    },
                },
                new()
                {
                    Key = "magpie.feather", Who = Magpie, Night = true, Letter = true, After = "magpie.cadoc", Gap = 3, Tier = 2,
                    Says = s => Say(Magpie,
                        Loc.T("\"Keywright. The abbey's chest was my runner's mistake, and Flick has been told. I take back what the Guild took from " +
                              "families who couldn't pay its tithe, and I give it back to them. Leave me be, and I'll remember it. M.\" A black " +
                              "feather falls out of the letter.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Burn it"), () =>
                        {
                            s.Sway(Faction.Lockless, 1);
                            return Loc.T("The feather curls up in the grate. Somewhere, somebody knows.");
                        }, W(s, 0, (Faction.Lockless, 1)), kind: true),
                        C(Loc.T("Show it to Hollis"), () =>
                        {
                            s.Sway(Faction.Wardens, 1);
                            s.Sway(Faction.Lockless, -1);
                            return Loc.T("Hollis turns the feather over. \"Her calling card. She's close, then.\"");
                        }, W(s, 0, (Faction.Wardens, 1), (Faction.Lockless, -1))),
                    },
                },
                new()
                {
                    Key = "magpie.carter", Who = Hollis, After = "magpie.feather", Gap = 2, Tier = 2,
                    Says = s => Say(Hollis, Loc.T($"Three more thefts this week, every one a chest the Guild had foreclosed on. A carter saw the " +
                                                  $"thief: {Clue(s, 0)}.")),
                    Then = s =>
                    {
                        s.Post("magpie1", Loc.T("The carter's word"), Loc.T($"A carter saw the Magpie: {Clue(s, 0)}."));
                        s.Log("magpie", Loc.T($"A carter saw the Magpie: {Clue(s, 0)}."));
                    },
                },
                new()
                {
                    Key = "magpie.goblins", Who = Nettle, After = "magpie.carter", Gap = 2, Tier = 2,
                    Says = s => Say(Nettle, Loc.T($"Wardens say a goblin did the abbey! Nettle's cousin saw the thief on the burrow road: " +
                                                  $"{Clue(s, 1)}. Not a goblin! Tell the sergeant.")),
                    Then = s =>
                    {
                        s.Post("magpie2", Loc.T("A goblin's word"), Loc.T($"Nettle's cousin saw the Magpie on the burrow road: {Clue(s, 1)}."));
                        s.Log("magpie", Loc.T($"Nettle's cousin saw the Magpie: {Clue(s, 1)}."));
                    },
                },
                new()
                {
                    Key = "magpie.bounty", Who = Hollis, After = "magpie.goblins", Gap = 2, Tier = 2,
                    Says = s => Say(Hollis,
                        Loc.T($"The Magpie's bounty is up to {Money.Say(s.Purses(0.5f))}. We've three names: Bettany Crane at the Crossed Keys, Aunt " +
                              $"Hesper the matchmaker, and Rook Varga, who's hunting the bounty herself."),
                        Loc.T("You've heard what the carter saw, and the goblin. You've met all three. Who is it?")),
                    Asks = s => Suspects.Select(who => C(Name(who), () => Accuse(s, who),
                            who == s.Secret("magpie") ? W(s, s.Purses(0.5f), (Faction.Wardens, 2), (Faction.Commons, 1), (Faction.Lockless, -3))
                                : W(s, -s.Purses(0.5f), (Faction.Wardens, -1), (Faction.Commons, -1))))
                        .Append(C(Loc.T("I won't name anyone"), () =>
                        {
                            s.Sway(Faction.Wardens, -1);
                            s.Flags.Add("magpie.silent");
                            s.Log("magpie", Loc.T("You wouldn't name anyone for the Magpie's bounty."));
                            return Loc.T("Have it your way. The bounty stands, if you change your mind.");
                        }, W(s, s.Purses(0.4f), (Faction.Wardens, -1), (Faction.Lockless, 2)), kind: true)).ToList(),
                },
                new()
                {
                    Key = "magpie.thanks", Who = Magpie, Night = true, Letter = true, After = "magpie.bounty", Gap = 1, Tier = 2,
                    If = s => s.Flagged("magpie.wrong"),
                    Says = s => Say(Magpie, Loc.T("\"You sent an innocent woman to the Wardens' cells, Keywright. I'll get her out. And thank you for " +
                                                  "the diversion: it kept the Wardens busy all week. M.\"")),
                    Then = s => s.Sway(Faction.Lockless, 1),
                },
                new()
                {
                    Key = "magpie.door", Who = Magpie, Night = true, After = "magpie.bounty", Gap = 1, Tier = 2,
                    If = s => s.Flagged("magpie.silent"),
                    Says = s => Say(s.Secret("magpie"),
                        Loc.T("A knock at the back door. A hooded woman: she lowers her hood, and smiles."),
                        Loc.T("You didn't give them my name, Keywright. I'm grateful, and the Lockless pay their debts. Take this: from a Guild " +
                              "strongroom, so it's nobody's now but yours.")),
                    Then = s =>
                    {
                        var rng = new Random(unchecked(s.Seed * 31 + s.Day));
                        string gift = GiftCurio(s, rng, CurioRarity.Rare);
                        s.Sway(Faction.Lockless, 2);
                        s.Log("magpie", Loc.T($"The Magpie came to the back door: {Name(s.Secret("magpie"))}. She left you {gift}."));
                    },
                },
            },
        },

        // The Marked Child: a millpond boy with the Key-mark, and the Guild's Mark-Finder.
        new()
        {
            Key = "pip", Title = Loc.T("The Marked Child"), Place = TalePlace.Brindle,
            Blurb = Loc.T("Pip Hollow, the washerwoman's boy, has the Key-mark on his palm. The Guild's Mark-Finders take marked children into " +
                          "its counting-houses, and they're testing hands at the fair."),
            Ending = PipEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "pip.itch", Who = Pip, Gap = 3,
                    Says = s => Say(Pip, Loc.T("Mister? My hand's been itching ever since I fished up that last chest. Look: there's a mark on it. " +
                                               "Like a key. Like yours.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Tell him what it means"), () =>
                        {
                            s.Flags.Add("pip.told");
                            s.Log("pip", Loc.T("Pip has the Key-mark, and you told him what it means."));
                            return Loc.T("A Keywright? Me? Mum'll faint.");
                        }, kind: true),
                        C(Loc.T("Tell him to keep it hidden"), () =>
                        {
                            s.Log("pip", Loc.T("Pip has the Key-mark. You told him to keep his hand in his pocket."));
                            return Loc.T("In my pocket. Got it. Is it a bad thing, mister?");
                        }),
                    },
                },
                new()
                {
                    Key = "pip.finder", Who = Aubel, After = "pip.itch", Gap = 2,
                    Says = s => Say(Aubel,
                        Loc.T("Mistress Aubel, Mark-Finder for the Gilded Guild. We test the children's hands at the fair tomorrow. The Guild " +
                              "takes the marked ones into its care: they want for nothing, all their lives."),
                        Loc.T($"A Keywright knows the itch when it starts. If you know of a marked child, a name is worth {Money.Say(s.Purses(0.75f))} to the Guild.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Pip Hollow, the millpond boy"), () =>
                        {
                            s.TaleGold(s.Purses(0.75f));
                            s.Sway(Faction.Guild, 2);
                            s.Sway(Faction.Commons, -2);
                            s.Goes(Pip);
                            s.Flags.Add("pip.named");
                            s.Goes(Wynn);
                            s.Log("pip", Loc.T("You gave the Mark-Finder Pip's name. The Guild took him into its care, and Wynn left Brindle."));
                            return Loc.T("The Guild thanks you. He'll be well looked after. And the Guild remembers its friends: its bailiff calls on them first.");
                        }, W(s, s.Purses(0.75f), (Faction.Guild, 2), (Faction.Commons, -2)) - s.Purses(0.3f)),
                        C(Loc.T("None that I know of"), () =>
                        {
                            s.Log("pip", Loc.T("You told the Mark-Finder you knew of no marked child."));
                            return Loc.T("Hm. The fair will tell.");
                        }, W(s, 0), kind: true),
                    },
                },
                new()
                {
                    Key = "pip.wynn", Who = Wynn, After = "pip.finder", Gap = 1, If = s => !s.Flagged("pip.named"),
                    Says = s => Say(Wynn,
                        Loc.T("I'm Pip's mum. The Guild's men are going door to door, looking at children's hands. He told me what you are."),
                        Loc.T("Will you hide him? He'd sleep in your back room and work for his keep, and he's a good boy. Please.")),
                    Asks = s => new()
                    {
                        C(Loc.T("He can stay, as my apprentice"), () =>
                        {
                            s.Flags.Add("pip.hiding");
                            s.Sway(Faction.Commons, 1);
                            s.Log("pip", Loc.T("You took Pip on as your apprentice, and hid him in the back room."));
                            return Loc.T("Bless you. Oh, bless you.");
                        }, W(s, s.Purses(0.5f) * Math.Max(0, Guild.Weeks - s.Week), (Faction.Commons, 1), (Faction.Guild, -2)), kind: true),
                        C(Loc.T("I can't risk it"), () =>
                        {
                            s.Goes(Pip);
                            s.Goes(Wynn);
                            s.Sway(Faction.Commons, -1);
                            s.Log("pip", Loc.T("You wouldn't hide Pip. The Guild found him at the fair."));
                            return Loc.T("Then they'll find him. They always do.");
                        }, W(s, 0, (Faction.Commons, -1))),
                    },
                },
                new()
                {
                    Key = "pip.search", Who = Aubel, Night = true, After = "pip.wynn", Gap = 2, If = s => s.Flagged("pip.hiding"),
                    Says = s => Say(Aubel, Loc.T("A knock after dark. \"Mistress Aubel. Open up, Keywright. We're told there's a marked child under your roof.\"")),
                    Asks = s =>
                    {
                        var asks = new List<Choice>
                        {
                            C(Loc.T("There's no child here"), () =>
                            {
                                s.Sway(Faction.Guild, -2);
                                s.Flags.Add(Apprentice);
                                s.Log("pip", Loc.T("You turned the Mark-Finder away. Pip's your apprentice now."));
                                return Loc.T("We'll see. The Guild is patient, Keywright.");
                            }, W(s, s.Purses(0.5f) * Math.Max(0, Guild.Weeks - s.Week), (Faction.Guild, -2)), kind: true),
                            C(Loc.T("Take him"), () =>
                            {
                                s.Sway(Faction.Guild, 1);
                                s.Sway(Faction.Commons, -2);
                                s.Goes(Pip);
                                s.Goes(Wynn);
                                s.Log("pip", Loc.T("You let the Mark-Finder take Pip."));
                                return Loc.T("A wise choice.");
                            }, W(s, 0, (Faction.Guild, 1), (Faction.Commons, -2))),
                        };
                        if (s.GraceLeft)
                            asks.Add(C(Loc.T("The Guild still takes Bram's word"), () =>
                            {
                                s.SpendWord();
                                s.Flags.Add(Apprentice);
                                s.Log("pip", Loc.T("Bram gave his word for you, and the Mark-Finder went away. Pip's your apprentice now."));
                                return Loc.T("Bram's word. Very well. The Guild will remember it was spent on this.");
                            }, W(s, s.Purses(0.5f) * Math.Max(0, Guild.Weeks - s.Week)) - s.Purses(0.6f)));
                        return asks;
                    },
                },
                // 0.30: Wynn's thanks (a neighbour's price, and a favour), and the Mark-Finder's register.
                new()
                {
                    Key = "pip.thanks", Who = Wynn, After = "pip.search", Gap = 2, If = s => s.Flagged(Apprentice),
                    Says = s => Say(Wynn,
                        Loc.T("Pip says you stood in the door and told the Guild there was no child here. The whole mill knows it, and half of Brindle."),
                        Loc.T("We haven't the coin to thank you. But we've neighbours, and every one of them will sell to you at a neighbour's price " +
                              "while my boy's safe with you: a sixth off.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Thank your neighbours for me"), () =>
                        {
                            s.Flags.Add(Neighbours);
                            s.Owe(Wynn, "pip");
                            s.Log("pip", Loc.T("Brindle's regulars sell to you at a neighbour's price now, a sixth off, while Pip's safe with you. Wynn owes you a favour."));
                            return Loc.T("And if the Guild ever comes for more than its due, send for me. The whole mill will come.");
                        }, W(s, s.Purses(0.4f) + Strategy.FavourWorth(s))),
                        C(Loc.T("Keep your neighbours' coin"), () =>
                        {
                            s.Sway(Faction.Commons, 1);
                            s.Owe(Wynn, "pip");
                            s.Log("pip", Loc.T("You wouldn't take a neighbour's price for hiding Pip. Brindle hasn't stopped talking about it. Wynn owes you a favour."));
                            return Loc.T("Then I'll tell everyone that too. And if the Guild comes for more than its due, send for me.");
                        }, W(s, Strategy.FavourWorth(s), (Faction.Commons, 1)), kind: true),
                    },
                },
                new()
                {
                    Key = "pip.register", Who = Pip, After = "pip.thanks", Gap = 6, Tier = 2, If = s => s.Flagged(Apprentice),
                    Says = s => Say(Pip,
                        Loc.T("Master, the Mark-Finder was at the Thornwick fair. She keeps a book: every marked child's name, and where the Guild " +
                              "sent them. Some of them were six."),
                        Loc.T("She leaves it on her table while she eats her pie. I could copy a page. I'm quick, and I'm small.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Copy one page, and run"), () =>
                        {
                            s.Gather("register");
                            s.Sway(Faction.Guild, -1);
                            s.Log("pip", Loc.T("Pip copied a page of the Mark-Finder's register: children the Guild took, and where it sent them. A paper for the case against the Guild."));
                            return Loc.T("Done before she'd finished her crust! Forty names. I knew two of them.");
                        }, W(s, Strategy.PaperWorth(s), (Faction.Guild, -1))),
                        C(Loc.T("Too dangerous. Stay here"), () =>
                        {
                            s.Log("pip", Loc.T("You wouldn't let Pip near the Mark-Finder's register."));
                            return Loc.T("Yes, master. I'll sweep, then.");
                        }, kind: true),
                    },
                },
            },
        },

        // The Widow's Debt: Ada's son in the Guild's debt-house, and her husband's sword in a pawn lot.
        new()
        {
            Key = "kettle", Title = Loc.T("The Widow's Debt"), Place = TalePlace.Brindle,
            Blurb = Loc.T("Ada Kettle's son Jory is in the Guild's debt-house for his dead father's tithe. Her husband Rafe's sword, with KETTLE " +
                          "on the blade, was pawned years ago in Crownhold."),
            Ending = KettleEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "kettle.jory", Who = Ada, Gap = 4,
                    Says = s => Say(Ada,
                        Loc.T("My boy Jory's in the Guild's debt-house in Crownhold, for his father's tithe: the Guild says Rafe owed it. Two years " +
                              "he's been in there."),
                        Loc.T("And if you ever find Rafe's sword in a chest, dear, KETTLE on the blade... it was pawned when he died. I'd give " +
                              "anything to have it back.")),
                    Then = s =>
                    {
                        s.Post("kettle", Loc.T("Ada Kettle seeks her husband's sword"),
                            Loc.T("A soldier's sword with KETTLE on the blade, pawned in Crownhold years ago. Ada Kettle, at Brindle, would give anything for it."));
                        s.Log("kettle", Loc.T("Ada's son Jory is in the Guild's debt-house. Her husband Rafe's sword was pawned in Crownhold."));
                    },
                },
                new()
                {
                    Key = "kettle.pledge", Who = Brass, After = "kettle.jory", Gap = 1, Tier = 3,
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "pawn", ShopTiers.Typical(s.Tier), s.Tier, 2, Category.Arms);
                        chest.Tale = "kettle";
                        return Brought(s, rng, Brass, chest, Loc.T("A soldier's pledge, never redeemed. The ticket says \"Kettle: one sword, and sundries\". " +
                                                                    "{articles} in all, it says. {price}."), "pawn");
                    },
                    Bought = (s, v) =>
                    {
                        // Bram takes the sword out before it's sold: it's Ada's to have, whatever you decide.
                        if (v.Chest.Everything.Where(i => i.Category == Category.Arms && !i.Fake).OrderByDescending(i => i.Value).FirstOrDefault() is Item sword)
                        {
                            v.Chest.Remove(sword);
                            s.SetAside["kettle"] = s.SaleValue(sword);
                        }
                        s.Flags.Add("kettle.bought");
                        s.Log("kettle", Loc.T("You bought Silas Brass's pledge with Rafe Kettle's sword in it. Bram set the sword aside for Ada."));
                    },
                    Passed = (s, v) => s.Log("kettle", Loc.T("You passed on Silas Brass's pledge with Rafe Kettle's sword in it. Someone else will have it.")),
                },
                new()
                {
                    Key = "kettle.sword", Who = Ada, After = "kettle.pledge", Gap = 1, Tier = 3, If = s => s.Flagged("kettle.bought"),
                    Says = s => Say(Ada, Loc.T("I came up to Crownhold to see Jory, and Bram sent word: you found Rafe's sword! Oh, dear. May I see it?")),
                    Asks = s => new()
                    {
                        C(Loc.T("It's yours"), () =>
                        {
                            s.Sway(Faction.Commons, 1);
                            s.Owe(Ada, "kettle");
                            // 0.32: and Rafe's medal, to keep.
                            s.GiveKeepsake(Keepsakes.Medal);
                            s.Log("kettle", Loc.T("You gave Ada her husband's sword, and she gave you his medal to keep. She owes you a favour, she says, and she means it."));
                            return Loc.T("Rafe's sword. Oh, Rafe. Here: his medal, from two wars. He'd want it with someone kind. Thank you, dear.");
                        }, W(s, Strategy.KeepsakeWorth(s), (Faction.Commons, 1)), kind: true),
                        C(Loc.T($"Sell it to her for what she has ({ShopTiers.Scale(s.Tier) * 5}g)"), () =>
                        {
                            s.TaleGold(ShopTiers.Scale(s.Tier) * 5);
                            s.Sway(Faction.Commons, 1);
                            s.Log("kettle", Loc.T("You sold Ada her husband's sword for what she had."));
                            return Loc.T("It's all I have, but it's his. Thank you.");
                        }, W(s, ShopTiers.Scale(s.Tier) * 5, (Faction.Commons, 1))),
                        C(Loc.T($"Sell it on ({s.SetAside.GetValueOrDefault("kettle")}g)"), () =>
                        {
                            s.TaleGold(s.SetAside.GetValueOrDefault("kettle"));
                            s.Sway(Faction.Commons, -1);
                            s.Log("kettle", Loc.T("You sold Rafe Kettle's sword on."));
                            return Loc.T("Oh. Of course. It's yours to sell, I suppose.");
                        }, W(s, s.SetAside.GetValueOrDefault("kettle"), (Faction.Commons, -1))),
                    },
                },
                new()
                {
                    Key = "kettle.debt", Who = Ada, After = "kettle.pledge", Gap = 3, Tier = 3, Expect = Loc.T("Ada, about Jory"),
                    Says = s => Say(Ada, Loc.T($"The debt-house wants {Money.Say(s.Purses(1))} for Jory, and I've saved half of it. If you could find the " +
                                               $"rest... he's a strong lad, and he'd work for you till it was paid.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Pay the rest ({s.Purses(0.5f)}g)"), () =>
                        {
                            s.Flags.Add(Porter);
                            s.PorterUntil = s.Day + Guild.DaysPerWeek;
                            s.Sway(Faction.Commons, 1);
                            s.Log("kettle", Loc.T("You paid the rest of Jory's debt. He'll carry chests in for you for a week, to pay you back: one more customer a day."));
                            return Loc.T("Jory's free! He'll work a week for you to pay you back, dear. He'll carry anything.");
                        }, W(s, s.Purses(0.3f) * Guild.DaysPerWeek, (Faction.Commons, 1)), cost: s.Purses(0.5f), kind: true),
                        C(Loc.T("I can't"), () =>
                        {
                            // 0.33: the Guild calls Jory's note in on its next night (Gilding.cs).
                            s.Claim(Jory, s.Week + 1);
                            s.Log("kettle", Loc.T("You couldn't pay Jory's debt. The Guild has called his note in, on its next night."));
                            return Loc.T("I understand, dear. I'll keep saving. They say they'll collect him if it's not paid. Collect. Like a parcel.");
                        }),
                    },
                },
                // 0.33: the Guild's night is coming for Jory: one more chance.
                new()
                {
                    Key = "kettle.claim", Who = Ada, After = "kettle.debt", Gap = 2, Tier = 3, If = s => s.Claims.ContainsKey(Jory),
                    Expect = Loc.T("Ada, about Jory's note"),
                    Says = s => Say(Ada,
                        Loc.T("The Guild's posted Jory's note on the debt-house door, dear. It falls due on the Guild's night. Old Haddon's went " +
                              "the same way, and look at him."),
                        Loc.T($"I've still half of it. {Money.Say(s.Purses(0.5f))} more and he's free. Please.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Pay the rest ({s.Purses(0.5f)}g)"), () =>
                        {
                            s.Redeem(Jory);
                            s.Flags.Add(Porter);
                            s.PorterUntil = s.Day + Guild.DaysPerWeek;
                            s.Sway(Faction.Commons, 1);
                            s.Log("kettle", Loc.T("You paid Jory's note the week the Guild called it in. He'll carry chests in for you for a week: one more customer a day."));
                            return Loc.T("Jory's free! Oh, dear. He'll work a week for you, and carry anything.");
                        }, W(s, s.Purses(0.3f) * Guild.DaysPerWeek + s.Purses(0.5f), (Faction.Commons, 1)), cost: s.Purses(0.5f), kind: true),
                        C(Loc.T("I can't"), () =>
                        {
                            s.Log("kettle", Loc.T("You couldn't pay Jory's note before the Guild's night."));
                            return Loc.T("Then I'll sit with him tonight. Whatever they do, he won't be alone.");
                        }),
                    },
                },
                new()
                {
                    Key = "kettle.gilded", Who = Ada, Night = true, After = "kettle.debt", Gap = 0, Tier = 3, If = s => s.IsTaken(Jory),
                    Says = s => Say(Ada,
                        Loc.T("Ada at the back door in her shawl, shaking. \"They came for Jory with a cart, dear. To work off the note, they said. " +
                              "They wouldn't say where.\""),
                        Loc.T("\"I don't care what they say he owes. He's my boy.\"")),
                    Then = s => s.Log("kettle", Loc.T("The Guild collected Jory Kettle on its night. He stands gold in the debt-house yard.")),
                },
                // 0.30: what Jory saw in the debt-house's books.
                new()
                {
                    Key = "kettle.tallies", Who = Jory, After = "kettle.debt", Gap = 3, Tier = 3, If = s => s.Flagged(Porter),
                    Says = s => Say(Jory,
                        Loc.T("Two years I scrubbed the debt-house floors, and the clerks never minded me reading over their shoulders. Half the " +
                              "debts in there were never owed: the Guild adds a tithe to a dead man's name and sends for his son."),
                        Loc.T("I can write down what I saw, names and sums. Would it help anyone?")),
                    Asks = s => new()
                    {
                        C(Loc.T("Write it all down"), () =>
                        {
                            s.Gather("tallies");
                            s.Owe(Jory, "kettle");
                            s.Log("kettle", Loc.T("Jory wrote down the debt-house's tallies: debts the Guild made up. A paper for the case against the Guild. He owes you a favour."));
                            return Loc.T("Every name I can remember. And I don't forget a sum.");
                        }, W(s, Strategy.PaperWorth(s) + Strategy.FavourWorth(s)), kind: true),
                        C(Loc.T("Put it behind you, Jory"), () =>
                        {
                            s.Owe(Jory, "kettle");
                            s.Log("kettle", Loc.T("Jory put the debt-house behind him. He owes you a favour."));
                            return Loc.T("Maybe you're right. I'll carry chests and forget it.");
                        }, W(s, Strategy.FavourWorth(s))),
                    },
                },
            },
        },

        // The Deep Door: a door sealed with the Key-mark at the bottom of the Barrow.
        new()
        {
            Key = "door", Title = Loc.T("The Deep Door"), Place = TalePlace.Brindle, Secrets = new[] { "hoard", "fall" }, // loc: keys
            Blurb = Loc.T("Tobin the adventurer has found a door at the bottom of the Barrow, sealed with the Key-mark and untouched since the " +
                          "Locking. He needs a Keywright's stake to go through it."),
            Ending = DoorEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "door.ask", Who = Tobin, Gap = 5,
                    Says = s => Say(Tobin,
                        Loc.T("There's a door at the bottom of the Barrow, Keywright: stone, with a keyhole shaped like your mark. Nobody's been " +
                              "through it since the Locking."),
                        Loc.T($"Stake me {Money.Say(s.Purses(1))} for rope, lamps and two good men, and half of what's behind it is yours.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Here's the money ({s.Purses(1)}g)"), () =>
                        {
                            s.Flags.Add("door.staked");
                            s.Flags.Add($"away:{Tobin}");
                            s.Promise(s.Day + 4, Tobin, Loc.T("Tobin, back from the Barrow"));
                            s.Log("door", Loc.T("You staked Tobin to go through the Barrow's deep door."));
                            return Loc.T("You won't regret it. Four days, and I'll be back.");
                        }, s.Purses(0.1f), cost: s.Purses(1)),
                        C(Loc.T("Not this time"), () => Loc.T("Someone at the Crossed Keys will stake me, then."), kind: true),
                    },
                },
                new()
                {
                    Key = "door.hoard", Who = Tobin, After = "door.ask", Gap = 4,
                    If = s => s.Flagged("door.staked") && s.Secret("door") == "hoard",
                    Brings = (s, rng) =>
                    {
                        s.Flags.Remove($"away:{Tobin}");
                        s.Kept(Tobin);
                        return Given(s, rng, Tobin, Fill(rng, "barrow", Finer(s.Tier, 1), s.Tier, 3),
                            Loc.T("Behind the door: a vault nobody had touched since the Locking. Half's yours, like I said: this one, freely given. " +
                                  "One coin, for luck."), "barrow");
                    },
                    Bought = (s, v) => s.Log("door", Loc.T("Tobin came back from the deep door with a hoard, and gave you your half.")),
                    Passed = (s, v) => s.Log("door", Loc.T("Tobin came back from the deep door with a hoard. You wouldn't take your half.")),
                },
                new()
                {
                    Key = "door.fall", Who = Bettany, After = "door.ask", Gap = 3,
                    If = s => s.Flagged("door.staked") && s.Secret("door") == "fall",
                    Says = s => Say(Bettany,
                        Loc.T("Tobin's men came back to the Crossed Keys without him. He's alive: they heard him calling, under a fallen arch past " +
                              "the door."),
                        Loc.T($"A rescue wants a crew from Crownhold, and that's {Money.Say(s.Purses(1))}. Nobody here has it, love.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Pay for the rescue ({s.Purses(1)}g)"), () =>
                        {
                            s.Flags.Add("door.rescued");
                            s.Owe(Tobin, "door");
                            s.Promise(s.Day + 2, Tobin, Loc.T("Tobin, rescued"));
                            s.Log("door", Loc.T("Tobin was trapped past the deep door. You paid for his rescue."));
                            return Loc.T("I'll send for the crew tonight. Bless you, love.");
                        }, W(s, s.Purses(1.6f) - s.Purses(1), (Faction.Commons, 1)), cost: s.Purses(1), kind: true),
                        C(Loc.T("I can't"), () =>
                        {
                            s.Goes(Tobin);
                            s.Kept(Tobin);
                            s.Sway(Faction.Commons, -1);
                            s.Log("door", Loc.T("Tobin was trapped past the deep door, and there was no rescue."));
                            return Loc.T("Then I'll light a candle for him at the Crossed Keys.");
                        }, W(s, 0, (Faction.Commons, -1))),
                    },
                },
                new()
                {
                    Key = "door.saved", Who = Tobin, After = "door.fall", Gap = 2, If = s => s.Flagged("door.rescued"),
                    Brings = (s, rng) =>
                    {
                        s.Flags.Remove($"away:{Tobin}");
                        s.Kept(Tobin);
                        return Given(s, rng, Tobin, Fill(rng, "barrow", Finer(s.Tier, 1), s.Tier, 3),
                            Loc.T("You came for me. Half of what's behind that door is yours, freely given, and I'm never going down again. One coin, for luck."),
                            "barrow");
                    },
                    Bought = (s, v) =>
                    {
                        s.Sway(Faction.Commons, 1);
                        s.Owe(Tobin, "door");
                        s.Log("door", Loc.T("Tobin came back alive, and gave you half of what was behind the door. He owes you his life, he says."));
                    },
                },
                // 0.32: behind the vault, a stair that goes further down than the Barrow should.
                new()
                {
                    Key = "door.stair", Who = Tobin, After = "door.ask", Gap = 10, Tier = 3,
                    If = s => s.Did("door.hoard") || s.Did("door.saved"),
                    Says = s => Say(Tobin,
                        s.Did("door.saved")
                            ? Loc.T("I swore I'd never go down again. But I keep dreaming of what I saw past the arch that fell on me: a stair, going down, and " +
                                    "light at the bottom of it.")
                            : Loc.T("Behind the vault there's a stair. It goes down further than the Barrow should go, and there's light at the bottom, and water, and bells."),
                        Loc.T($"The old kings' halls, I reckon: the ones the songs say the Barrow was built over. Stake me {Money.Say(s.Purses(2))}, a proper " +
                              $"crew this time, and half of whatever's down there is yours, every week I'm digging.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Stake the expedition ({s.Purses(2)}g)"), () =>
                        {
                            s.Flags.Add("door.deep");
                            s.Flags.Add($"away:{Tobin}");
                            s.Promise(s.Day + 4, Tobin, Loc.T("Tobin, back from under the Barrow"));
                            s.Log("door", Loc.T("Past the deep door there's a stair down to the old kings' halls. You staked Tobin's expedition."));
                            return Loc.T("Four days. If I'm not back in six, don't send anyone. I mean it.");
                        }, s.Purses(2f) + Strategy.KeepsakeWorth(s) * 0.5f, cost: s.Purses(2)),
                        C(Loc.T("Once was enough"), () =>
                        {
                            s.Log("door", Loc.T("Tobin found a stair down to the old kings' halls. You told him once was enough."));
                            return Loc.T("You're right. You're right. I'll stop dreaming about it. Probably.");
                        }, kind: true),
                    },
                },
                new()
                {
                    Key = "door.under", Who = Tobin, After = "door.stair", Gap = 4, Tier = 3, If = s => s.Flagged("door.deep"),
                    Brings = (s, rng) =>
                    {
                        s.Flags.Remove($"away:{Tobin}");
                        s.Kept(Tobin);
                        return Given(s, rng, Tobin, Fill(rng, "barrow", Finer(s.Tier, 1), s.Tier, 4),
                            Loc.T("Halls, Keywright, halls under halls, and the kings asleep in them with their crowns on. Your half: freely given. One coin, for luck."),
                            "barrow");
                    },
                    Bought = (s, v) => s.Log("door", Loc.T("Tobin came back from the old kings' halls under the Barrow, and gave you your half.")),
                    Passed = (s, v) => s.Log("door", Loc.T("Tobin came back from the old kings' halls under the Barrow. You wouldn't take your half.")),
                },
                new()
                {
                    Key = "door.lamp", Who = Tobin, After = "door.under", Gap = 0, Night = true,
                    Says = s => Say(Tobin,
                        Loc.T("I'm going back down, and I'm staying: there are years of halls down there. Here, my lamp. Hang it in your window."),
                        Loc.T("Every week, first thing, I'll send up a chest for you, given freely, the way it should be. The lamp's how my lads will find you.")),
                    Then = s =>
                    {
                        s.GiveKeepsake(Keepsakes.Lamp);
                        s.Log("door", Loc.T("Tobin is digging under the Barrow for good. He gave you his lamp: a chest from him every week."));
                    },
                },
            },
        },

        // The Woodcutters: the Guild wants the Old Wood's oak, and the fair folk want a promise.
        new()
        {
            Key = "wood", Title = Loc.T("The Woodcutters"), Place = TalePlace.Thornwick,
            Blurb = Loc.T("The Guild is buying the felling rights on the Old Wood, and its deed wants a Keywright's mark as witness. The druids " +
                          "and the fair folk who live there want a promise instead."),
            Ending = WoodEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "wood.deed", Who = Caskett, Tier = 2, Gap = 6, If = s => s.Day > s.TierSince,
                    Says = s => Say(Caskett,
                        Loc.T("Harl Caskett, timber factor for the Gilded Guild. The Guild's buying the felling rights on the Old Wood. The druids " +
                              "won't sign, so the deed wants a licensed Keywright's mark as witness."),
                        Loc.T($"Put your mark to it, and the Guild pays a witness's fee: {Money.Say(WitnessFee(s, 0.1f))}, a tenth of your next due.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Seal it"), () =>
                        {
                            s.TaleGold(WitnessFee(s, 0.1f));
                            s.Sway(Faction.Guild, 1);
                            s.Sway(Faction.Wood, -2);
                            s.Flags.Add("wood.sealed");
                            s.Gather("deed");
                            s.Log("wood", Loc.T("You put your mark to the Guild's deed for the Old Wood. The witness copy names the coin it was paid in: seized from families."));
                            return Loc.T("A pleasure. The axes go in at the full moon.");
                        }, W(s, WitnessFee(s, 0.1f), (Faction.Guild, 1), (Faction.Wood, -2))),
                        C(Loc.T("Refuse"), () =>
                        {
                            s.Sway(Faction.Guild, -1);
                            s.Sway(Faction.Wood, 1);
                            s.Flags.Add("wood.refused");
                            s.Log("wood", Loc.T("You wouldn't put your mark to the Guild's deed for the Old Wood."));
                            return Loc.T("The Guild will find another Keywright. It always does.");
                        }, W(s, s.Purses(0.4f), (Faction.Guild, -1), (Faction.Wood, 1)), kind: true),
                    },
                },
                new()
                {
                    Key = "wood.sorrel", Who = Sorrel, After = "wood.deed", Gap = 1, Tier = 2,
                    Says = s => s.Flagged("wood.sealed")
                        ? Say(Sorrel, Loc.T("You put your mark to Caskett's deed, child. The axes come at the full moon, and the Wood will remember " +
                                            "whose mark it was."))
                        : Say(Sorrel, Loc.T("You wouldn't put your mark to Caskett's deed. The Hawthorn Lady has heard of it, and she would speak " +
                                            "with you. Leave your back door unlatched tonight.")),
                },
                new()
                {
                    Key = "wood.lady", Who = Hawthorn, Night = true, After = "wood.sorrel", Gap = 0, Tier = 2, If = s => s.Flagged("wood.refused"),
                    Says = s => Say(Hawthorn,
                        Loc.T("The door opens on its own, and the shop smells of hawthorn. \"Keywright. The Guild will find another mark for its " +
                              "deed, unless the one who refused it keeps refusing.\""),
                        Loc.T("\"Promise me you'll keep the Wood's oaks standing, and have this: a charm the Wood has kept since before the Locking. " +
                              "A promise to us is kept, or paid for.\"")),
                    Asks = s => new()
                    {
                        C(Loc.T("I promise"), () =>
                        {
                            // 0.32: the Wood's charm is a keepsake (no slot), and goes if the promise is broken.
                            s.GiveKeepsake(Keepsakes.Hawthorn);
                            s.Flags.Add("wood.promised");
                            s.Sway(Faction.Wood, 1);
                            s.Log("wood", Loc.T("You promised the Hawthorn Lady to keep the Wood's oaks standing. She gave you the Hawthorn Charm."));
                            return Loc.T("\"Kept, or paid for.\" She leaves a sprig of hawthorn, bound in silver, on the counter, and is gone.");
                        }, W(s, s.Purses(0.4f) + Strategy.KeepsakeWorth(s), (Faction.Wood, 1)), kind: true),
                        C(Loc.T("I can't promise that"), () => Loc.T("\"Then we'll speak no more of it.\" The hawthorn smell is gone by morning.")),
                    },
                },
                new()
                {
                    Key = "wood.again", Who = Caskett, After = "wood.deed", Gap = 5, Tier = 3, If = s => s.Flagged("wood.refused"),
                    Says = s => Say(Caskett, Loc.T($"Last time, Keywright. The Guild will pay a fifth of your next due for your mark on the Wood's deed: " +
                                                   $"{Money.Say(WitnessFee(s, 0.2f))}.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Seal it"), () =>
                        {
                            s.TaleGold(WitnessFee(s, 0.2f));
                            s.Sway(Faction.Guild, 1);
                            s.Sway(Faction.Wood, -3);
                            string broken = "";
                            if (s.Keeps(Keepsakes.Hawthorn))
                            {
                                s.TakeKeepsake(Keepsakes.Hawthorn);
                                broken = Loc.T(" A promise broken: the Hawthorn Charm has crumbled to dry leaves.");
                            }
                            // A charm given before 0.32 was a curio.
                            else if (s.Flags.FirstOrDefault(f => f.StartsWith("charm:")) is string charm && s.HasCurio(charm[6..]))
                            {
                                s.TakeCurio(charm[6..]);
                                broken = Loc.T($" A promise broken: the Hawthorn Lady's charm is gone from your case.");
                            }
                            s.Gather("deed");
                            s.Goes(Sorrel);
                            s.Flags.Add("wood.felled");
                            s.Log("wood", Loc.T("You sealed the Guild's deed for the Old Wood after all. Mother Sorrel won't trade with you again.") + broken);
                            return Loc.T("Sensible. The axes go in tomorrow.") + broken;
                        }, W(s, WitnessFee(s, 0.2f), (Faction.Guild, 1), (Faction.Wood, -3)) - (s.Flagged("wood.promised") ? s.Purses(0.6f) : 0)),
                        C(Loc.T("Refuse"), () =>
                        {
                            s.Sway(Faction.Guild, -1);
                            s.Sway(Faction.Wood, 2);
                            s.Flags.Add("wood.kept");
                            s.Log("wood", Loc.T("You refused the Guild's deed for the Old Wood a second time."));
                            return Loc.T("Then the Guild will remember it.");
                        }, W(s, s.Purses(0.5f), (Faction.Guild, -1), (Faction.Wood, 2)), kind: true),
                    },
                },
                new()
                {
                    Key = "wood.thanks", Who = Hawthorn, Night = true, After = "wood.again", Gap = 0, Tier = 3, If = s => s.Flagged("wood.kept"),
                    Says = s => Say(Hawthorn, Loc.T("Hawthorn again, and a voice from nowhere: \"Twice you refused them. The Wood keeps its own.\" " +
                                                    "Something is left on the counter.")),
                    Then = s =>
                    {
                        string gift = GiftCurio(s, new Random(unchecked(s.Seed * 41 + s.Day)), CurioRarity.Uncommon);
                        s.Log("wood", Loc.T($"The Hawthorn Lady thanked you with {gift}."));
                    },
                },
            },
        },

        // The Parcel: a smuggler's parcel kept overnight, and the Wardens' search.
        new()
        {
            Key = "parcel", Title = Loc.T("The Parcel"), Place = TalePlace.Thornwick,
            Blurb = Loc.T("Corvin Tallow the smuggler needs somewhere to keep a parcel for two nights, no questions asked, and the Wardens are " +
                          "searching the market."),
            Ending = ParcelEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "parcel.ask", Who = Corvin, Tier = 2, Gap = 7, If = s => s.Day > s.TierSince,
                    Says = s => Say(Corvin, Loc.T($"A favour, Keywright. Keep this parcel in your back room two nights. Don't open it: it isn't yours, " +
                                                  $"so it wouldn't anyway. {Money.Say(s.Purses(0.5f))} when I collect.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Deal"), () =>
                        {
                            s.BackRoom.Add(new Held { Owner = Name(Corvin), Key = "parcel", Colour = Rarity.Uncommon, Day = s.Day });
                            s.Flags.Add("parcel.kept");
                            s.Log("parcel", Loc.T("You're keeping Corvin's parcel in your back room."));
                            return Loc.T("Good. Two nights. Nobody needs to know.");
                        }, s.Friend(Faction.Wardens) ? W(s, s.Purses(0.5f), (Faction.Lockless, 1)) : -s.Purses(0.3f)),
                        C(Loc.T("No"), () => Loc.T("Pity. Forget I asked."), kind: true),
                    },
                },
                new()
                {
                    Key = "parcel.search", Who = Hollis, Night = true, After = "parcel.ask", Gap = 1, Tier = 2, If = s => s.Flagged("parcel.kept"),
                    Says = s => s.Friend(Faction.Wardens)
                        ? Say(Hollis, Loc.T("Hollis at the door, after dark. \"We're searching every back room on the market tonight. Not yours: I " +
                                            "told the lads you're a friend of the Wardens. Good night, Keywright.\""))
                        : Say(Hollis, Loc.T("Hollis at the door, after dark. \"Wardens. We're searching every back room on the market tonight.\" He " +
                                            "comes out with Corvin's parcel. \"And what's this?\"")),
                    Asks = s => s.Friend(Faction.Wardens) ? new() : new()
                    {
                        C(Loc.T("It's Corvin Tallow's"), () =>
                        {
                            s.BackRoom.RemoveAll(h => h.Key == "parcel");
                            s.Sway(Faction.Wardens, 1);
                            s.Sway(Faction.Lockless, -2);
                            s.Goes(Corvin);
                            s.Flags.Add("parcel.given");
                            s.Log("parcel", Loc.T("The Wardens found Corvin's parcel, and you told them whose it was. Corvin's gone from the market."));
                            return Loc.T("Contraband brandy, and a chest off the Guild's foreclosure list. Thank you, Keywright.");
                        }, W(s, 0, (Faction.Wardens, 1), (Faction.Lockless, -2)), kind: true),
                        C(Loc.T("It's mine"), () =>
                        {
                            s.BackRoom.RemoveAll(h => h.Key == "parcel");
                            s.TaleGold(-Math.Min(s.Gold, s.Purses(0.5f)));
                            s.Sway(Faction.Wardens, -1);
                            s.Sway(Faction.Lockless, -1);
                            s.Flags.Add("parcel.taken");
                            s.Log("parcel", Loc.T("The Wardens found Corvin's parcel. You said it was yours, and paid the fine."));
                            return Loc.T($"Contraband, Keywright. That's a fine of {Money.Say(s.Purses(0.5f))}, and it's confiscated.");
                        }, W(s, -s.Purses(0.5f), (Faction.Wardens, -1), (Faction.Lockless, -1))),
                    },
                },
                new()
                {
                    Key = "parcel.collect", Who = Corvin, After = "parcel.search", Gap = 1, Tier = 2, Expect = Loc.T("Corvin, for his parcel"),
                    If = s => s.BackRoom.Any(h => h.Key == "parcel"),
                    Says = s => Say(Corvin, Loc.T($"My parcel, safe and sound. You're a friend, Keywright. Here's {Money.Say(s.Purses(0.5f))}, and " +
                                                  $"something from my own pocket.")),
                    Then = s =>
                    {
                        s.BackRoom.RemoveAll(h => h.Key == "parcel");
                        s.TaleGold(s.Purses(0.5f));
                        s.Sway(Faction.Lockless, 1);
                        string gift = GiftCurio(s, new Random(unchecked(s.Seed * 43 + s.Day)), CurioRarity.Uncommon);
                        s.Log("parcel", Loc.T($"Corvin collected his parcel: he paid, and gave you {gift}."));
                    },
                },
            },
        },

        // The Foreclosures: why the Guild's foreclosed chests won't open, and what the Crown can do about it.
        new()
        {
            Key = "fore", Title = Loc.T("The Foreclosures"), Place = TalePlace.Crownhold,
            Blurb = Loc.T("Half the chests the Guild forecloses on won't open for any Keywright it sells them to. A clerk in its counting-house " +
                          "wants to know why, and the answer is in the Chest Roll."),
            Ending = ForeEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "fore.clerk", Who = Tobias, Tier = 3, Gap = 10, If = s => s.Day > s.TierSince,
                    Says = s => Say(Tobias,
                        Loc.T("Tobias Fenn, clerk at the Guild's counting-house. Forgive me: I've a question I can't ask at work."),
                        Loc.T("Half the chests the Guild forecloses on won't open for any Keywright it sells them to. The ledgers say they're sent " +
                              "\"below\", unopened. Why won't they open?")),
                    Asks = s => new()
                    {
                        C(Loc.T("Seizure isn't a sale: they're still the families'"), () =>
                        {
                            s.Flags.Add("fore.told");
                            s.Log("fore", Loc.T("Tobias Fenn asked why the Guild's foreclosed chests won't open. You told him: they're still the families'."));
                            return Loc.T("Then the Guild's been selling stolen chests. For a hundred years. Oh dear.");
                        }, W(s, s.Purses(0.3f)), kind: true),
                        C(Loc.T("Ask the Guild"), () =>
                        {
                            s.Log("fore", Loc.T("Tobias Fenn asked why the Guild's foreclosed chests won't open. You sent him back to the Guild."));
                            return Loc.T("I... yes. Of course. Forget I asked.");
                        }),
                    },
                },
                new()
                {
                    Key = "fore.rowan", Who = Rowan, After = "fore.clerk", Gap = 2, Tier = 3, If = s => s.Flagged("fore.told"),
                    Brings = (s, rng) => Brought(s, rng, Rowan, Fill(rng, "warchest", ShopTiers.Typical(s.Tier), s.Tier, 3, Category.Arms),
                        Loc.T("The Guild took my family's hall and sold its chests. I bought grandfather's war chest back, and it's mine to sell: it " +
                              "won't open for their Keywright. The sword's mine, for {fee}. The rest: {price}."), "warchest"),
                    Bought = (s, v) => s.Log("fore", Loc.T("Sir Rowan sold you his grandfather's war chest, bought back from the Guild: it was his, so it opened for you.")),
                },
                new()
                {
                    Key = "fore.roll", Who = Quire, After = "fore.clerk", Gap = 4, Tier = 3, If = s => s.Flagged("fore.told"),
                    Says = s => Say(Quire,
                        Loc.T("Master Quire, the Royal Archivist. Your clerk friend came to me. The Chest Roll agrees with you: every chest the " +
                              "Guild has seized is still in the families' names."),
                        Loc.T("By the ward's own law the Guild has been selling stolen chests for a century, and sending the ones that won't open " +
                              "down through the Gate. I've made you a copy of the Roll.")),
                    Then = s =>
                    {
                        s.Find("quire.law");
                        s.Find("roll.crown");
                    },
                    Asks = s => new()
                    {
                        C(Loc.T("I'll take it to the King"), () =>
                        {
                            s.Flags.Add("fore.king");
                            s.Gather("roll");
                            s.Sway(Faction.Crown, 1);
                            s.Log("fore", Loc.T("Master Quire gave you a copy of the Chest Roll. You'll take it to the King."));
                            return Loc.T("The King. Yes. If anyone can make the Guild answer for it, he can.");
                        }, W(s, s.Purses(1.5f), (Faction.Crown, 1)), kind: true),
                        C(Loc.T($"Sell the copy to the Guild ({s.Purses(0.5f)}g)"), () =>
                        {
                            s.TaleGold(s.Purses(0.5f));
                            s.Sway(Faction.Guild, 2);
                            s.Goes(Tobias);
                            s.Flags.Add("fore.sold");
                            s.Log("fore", Loc.T("You sold the copy of the Chest Roll to the Guild. Tobias Fenn hasn't been seen since."));
                            return Loc.T("You'd... very well. It's yours to sell.");
                        }, W(s, s.Purses(0.5f), (Faction.Guild, 2))),
                    },
                },
                new()
                {
                    Key = "fore.ledger", Who = Tobias, Night = true, After = "fore.roll", Gap = 1, Tier = 3, If = s => s.Flagged("fore.king"),
                    Says = s => Say(Tobias,
                        Loc.T("Tobias at the back door, with a travelling bag. \"They know I talked. I'm leaving Crownhold tonight. Take this: a page " +
                              "from the Guild's own ledgers. Every tithe, and where it goes. 'Below.'\"")
                        + (s.Flagged(Porter) ? Loc.T(" Jory Kettle's with him: \"I'll swear to the King what the debt-house is full of.\"") : "")),
                    Then = s =>
                    {
                        s.Flags.Add("fore.ledger");
                        s.Gather("ledger");
                        s.Find("ledger");
                        s.Find("tobias.secret");
                        s.Goes(Tobias);
                        s.Log("fore", Loc.T("Tobias Fenn left Crownhold, and left you a page of the Guild's ledgers: where the tithe goes."));
                    },
                },
                new()
                {
                    Key = "fore.decree", Who = Aldo, After = "fore.ledger", Gap = 1, Tier = 4, Expect = Loc.T("The King, for the Roll"),
                    Says = s => Say(Aldo,
                        Loc.T("You've something for the Crown, I hear. The Roll, and a page of the Guild's own ledgers. Let me see."),
                        Loc.T("The Guild's been robbing my subjects in my name, and sending it to the Maw. My decree: the Guild's tithe on " +
                              "Keywrights is cut by a fifth, from tonight. Vane will be... displeased.")),
                    Then = s =>
                    {
                        s.Flags.Add(Decree);
                        s.Sway(Faction.Crown, 2);
                        s.Sway(Faction.Guild, -3);
                        s.Post("decree", Loc.T("By the King's decree"), Loc.T("The Gilded Guild's tithe on Keywrights is cut by a fifth. Aldric IX, Rex."));
                        s.Log("fore", Loc.T("The King cut the Guild's tithe by a fifth. The Guild will remember who brought him the Roll."));
                    },
                },
            },
        },

        // The Unlocking: an archmage's engine to copy the mark.
        new()
        {
            Key = "unlock", Title = Loc.T("The Unlocking"), Place = TalePlace.Crownhold,
            Blurb = Loc.T("Archmage Orme of the Arcanum is building an engine to copy the Key-mark, so that any chest would open for anyone. She " +
                          "needs a Keywright's blood, and Mara says it would end the Locking."),
            Ending = UnlockEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "unlock.plate", Who = Ilse, Tier = 3, Gap = 11, If = s => s.Day > s.TierSince,
                    Says = s => Say(Ilse, Loc.T($"My mistress, Archmage Orme, asks a favour: one drop of a Keywright's blood, on a brass plate. That's all. " +
                                                $"She'll pay {Money.Say(s.Purses(0.25f))}.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Give it"), () =>
                        {
                            s.TaleGold(s.Purses(0.25f));
                            s.Sway(Faction.Arcanum, 1);
                            s.Flags.Add("unlock.gave");
                            s.Log("unlock", Loc.T("You gave Archmage Orme a drop of your blood on a brass plate."));
                            return Loc.T("Thank you! She'll be so pleased. I think.");
                        }, W(s, s.Purses(0.25f), (Faction.Arcanum, 1))),
                        C(Loc.T("Refuse"), () =>
                        {
                            s.Sway(Faction.Arcanum, -1);
                            s.Log("unlock", Loc.T("You wouldn't give Archmage Orme your blood."));
                            return Loc.T("Oh. She'll find someone else. She always does.");
                        }, W(s, 0, (Faction.Arcanum, -1)), kind: true),
                    },
                },
                new()
                {
                    Key = "unlock.engine", Who = Ilse, After = "unlock.plate", Gap = 3, Tier = 3, Expect = Loc.T("Ilse, worried"),
                    Says = s => Say(Ilse,
                        s.Flagged("unlock.gave")
                            ? Loc.T("Your blood's in her engine. She's building something to copy the mark: with it, any chest would open for anyone.")
                            : Loc.T("She found another Keywright's blood. She's building an engine to copy the mark: any chest would open for anyone."),
                        Loc.T("Mara came to the Arcanum last night. She says if the mark can be copied, the Locking's finished, and the realm with " +
                              "it. I don't know what to do.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Help her break it"), () =>
                        {
                            s.Sway(Faction.Arcanum, -2);
                            s.Flags.Add("unlock.broken");
                            s.Gather("notes");
                            s.Owe(Ilse, "unlock");
                            string gift = GiftCurio(s, new Random(unchecked(s.Seed * 47 + s.Day)), CurioRarity.Uncommon, CurioTag.Enchantment);
                            s.Log("unlock", Loc.T($"You helped Ilse break Orme's engine. She gave you {gift} from the Arcanum's stores."));
                            return Loc.T($"Then I'll do it tonight. Take this, from the stores: {gift}. She'll never miss it.");
                        }, W(s, s.Purses(0.4f) + Strategy.PaperWorth(s) + Strategy.FavourWorth(s), (Faction.Arcanum, -2)), kind: true),
                        C(Loc.T("Tell the Guild"), () =>
                        {
                            s.TaleGold(s.Purses(0.5f));
                            s.Sway(Faction.Guild, 2);
                            s.Sway(Faction.Arcanum, -3);
                            s.Goes(Orme);
                            s.Log("unlock", Loc.T("You told the Guild about Orme's engine. They took her away, and paid you."));
                            return Loc.T("The Guild? Oh no. Oh, no.");
                        }, W(s, s.Purses(0.5f), (Faction.Guild, 2), (Faction.Arcanum, -3))),
                        C(Loc.T("Stay out of it"), () =>
                        {
                            s.Flags.Add("unlock.left");
                            s.Log("unlock", Loc.T("You stayed out of Orme's engine."));
                            return Loc.T("I... all right. I'll stay out of it too.");
                        }, W(s, s.Purses(1) - s.Purses(1.5f))),
                    },
                },
                new()
                {
                    Key = "unlock.works", Who = Orme, After = "unlock.engine", Gap = 3, Tier = 4, If = s => s.Flagged("unlock.left"),
                    Says = s => Say(Orme,
                        Loc.T("It works, Keywright. Partly: one chest in three opens for anyone at all. The Guild has offered me the moon for it."),
                        Loc.T($"And the Gate is cracking: the Maw's gilded iron is leaking into every purse in the realm. Here: {Money.Say(s.Purses(1))}, your share, for the blood.")),
                    Then = s =>
                    {
                        s.TaleGold(s.Purses(1));
                        s.Flags.Add(GateCracked);
                        s.Log("unlock", Loc.T("Orme's engine works, partly, and the Gate is cracking: demon gold is everywhere now, in any purse of coin."));
                    },
                },
            },
        },

        // Ironroot's Crown: the dwarves' crown, locked in the Royal Treasury.
        new()
        {
            Key = "crown", Title = Loc.T("Ironroot's Crown"), Place = TalePlace.Crownhold,
            Blurb = Loc.T("Ironroot's crown was locked in the Royal Treasury at the Locking, and the Guild bans dwarf-gold. Durgan Ironhand wants " +
                          "it home under the mountain, and only the King can give it."),
            Ending = CrownEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "crown.ask", Who = Durgan, Tier = 3, Gap = 11, If = s => s.Day > s.TierSince,
                    Says = s => Say(Durgan,
                        Loc.T("At the Locking, the King's men locked Ironroot's crown in the Royal Treasury with everything else. Three hundred " +
                              "years it's sat there."),
                        Loc.T("The Guild calls our gold contraband because we won't pay its tithe. If the crown came home, every dwarf in Ironroot " +
                              "would remember who brought it.")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll ask the King"), () =>
                        {
                            s.Flags.Add("crown.asking");
                            s.Sway(Faction.Ironroot, 1);
                            s.Log("crown", Loc.T("You told Durgan you'd ask the King for Ironroot's crown."));
                            return Loc.T("Ha! A surface-dweller with a spine. We'll see.");
                        }, W(s, s.Purses(1), (Faction.Ironroot, 1)), kind: true),
                        C(Loc.T("Nothing I can do"), () => Loc.T("Hmph. Nobody ever can.")),
                    },
                },
                new()
                {
                    Key = "crown.king", Who = Aldo, After = "crown.ask", Gap = 1, Tier = 4, If = s => s.Flagged("crown.asking"),
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "treasury", Rarity.Rare, s.Tier, 1);
                        chest = new Chest(chest.Items.Append(new Item(Category.Gems, Rarity.Legendary)), Rarity.Rare, lockTier: chest.Lock);
                        chest.Tale = "crown";
                        return Given(s, rng, Aldo, chest, Loc.T("Ironroot's crown? It's in a chest I can't open, like everything I own. Here: I give it to " +
                                                                "you. Keep what else is in it, and give the dwarves their crown."), "treasury");
                    },
                    Bought = (s, v) =>
                    {
                        if (v.Chest.Everything.Where(i => i.Category == Category.Gems).OrderByDescending(i => i.Value).FirstOrDefault() is Item crown)
                            v.Chest.Remove(crown);
                        s.Flags.Add("crown.found");
                        s.Log("crown", Loc.T("The King gave you the chest with Ironroot's crown in it. Bram set the crown aside for the dwarves."));
                    },
                },
                new()
                {
                    Key = "crown.thanks", Who = Durgan, After = "crown.king", Gap = 1, Tier = 3, If = s => s.Flagged("crown.found"),
                    Says = s => Say(Durgan, Loc.T("The crown! Ironroot owes you a debt it won't forget, Keywright. Our smiths will build you what you " +
                                                  "need, for nothing.")),
                    Then = s =>
                    {
                        s.Sway(Faction.Ironroot, 3);
                        s.Flags.Add(CrownHome);
                        s.Owe(Durgan, "crown");
                        var gift = new[] { "workshop", "magnet" }.Select(Upgrades.Get).FirstOrDefault(u => !s.Has(u.Key) && (u.Requires == null || s.Has(u.Requires)));
                        if (gift != null) s.Grant(gift.Key);
                        else s.TaleGold(s.Purses(1.5f));
                        s.Log("crown", gift != null ? Loc.T($"Ironroot's smiths built you a {gift.Name}, for nothing.") : Loc.T("Ironroot paid you for the crown in dwarf-gold."));
                    },
                },
            },
        },

        // The Vell Inheritance: a brother selling his sister's chests.
        new()
        {
            Key = "vell", Title = Loc.T("The Vell Inheritance"), Place = TalePlace.Crownhold,
            Blurb = Loc.T("Lord Cassian Vell is selling his sister's chests to pay his debts, and they aren't his to sell: their grandmother's " +
                          "will left them to Lady Isaura. A chest sold by the wrong hands is cold to the mark."),
            Ending = VellEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "vell.box", Who = Cassian, Tier = 3, Gap = 9, If = s => s.Day > s.TierSince,
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "strongroom", ShopTiers.Typical(s.Tier), s.Tier, 3);
                        chest.Owner = Loc.T("Lady Isaura Vell");
                        chest.Tale = "vell";
                        return Brought(s, rng, Cassian, chest, Loc.T("Grandmother's trick box. I need it gone before the tables open tonight: {price}, " +
                                                                     "and not a word to my sister."), "strongroom");
                    },
                    Bought = (s, v) => s.Log("vell", Loc.T("You bought a trick box from Lord Cassian. It won't open: it's his sister's.")),
                    Passed = (s, v) =>
                    {
                        s.Flags.Add(v.Outcome == VisitOutcome.Reported ? "vell.wardens" : "vell.passed");
                        s.Log("vell", v.Outcome == VisitOutcome.Reported
                            ? Loc.T("Lord Cassian tried to sell you his sister's trick box. You sent for the Wardens.")
                            : Loc.T("Lord Cassian tried to sell you his sister's trick box. You passed."));
                    },
                },
                new()
                {
                    Key = "vell.sister", Who = Isaura, After = "vell.box", Gap = 1, Tier = 3,
                    Says = s => s.BackRoom.Any(h => h.Key == "vell")
                        ? Say(Isaura, Loc.T("My brother sold you grandmother's box. It's mine, by her will, so it won't open for you. Give it back to " +
                                            "me, and I'll give it to you properly. Then it will."))
                        : s.Flagged("vell.wardens")
                            ? Say(Isaura, Loc.T("Cassian spent a night in the Wardens' cells over grandmother's box. It did him good. Thank you, Keywright."))
                            : Say(Isaura, Loc.T("Cassian sold one of grandmother's boxes to someone. It will never open for them, and he knows it.")),
                    Asks = s => s.BackRoom.FirstOrDefault(h => h.Key == "vell") is Held held
                        ? new()
                        {
                            C(Loc.T("Give it back"), () =>
                            {
                                s.BackRoom.Remove(held);
                                s.Flags.Add("vell.given");
                                s.Owe(Isaura, "vell");
                                s.Sway(Faction.Crown, 1);
                                s.Log("vell", Loc.T("You gave Lady Isaura her grandmother's box back."));
                                return Loc.T("Thank you. I'll bring it back tomorrow: freely given, it'll open for you.");
                            }, W(s, s.Purses(1), (Faction.Crown, 1)), kind: true),
                            C(Loc.T("Keep it"), () =>
                            {
                                s.Sway(Faction.Crown, -1);
                                s.Log("vell", Loc.T("You kept Lady Isaura's box."));
                                return Loc.T("Then it sits in your back room forever. How very Vell of you.");
                            }, W(s, s.FencePay(held), (Faction.Crown, -1))),
                        }
                        : new(),
                    Then = s =>
                    {
                        if (s.Flagged("vell.wardens")) s.Sway(Faction.Crown, 1);
                    },
                },
                new()
                {
                    Key = "vell.gift", Who = Isaura, After = "vell.sister", Gap = 1, Tier = 3, If = s => s.Flagged("vell.given"),
                    Expect = Loc.T("Isaura, with grandmother's box"),
                    Brings = (s, rng) => Given(s, rng, Isaura, Fill(rng, "strongroom", ShopTiers.Typical(s.Tier), s.Tier, 3),
                        Loc.T("Grandmother's box, freely given. Now it's yours, and it will open. One coin, for luck: she'd have insisted."), "strongroom"),
                    Bought = (s, v) => s.Log("vell", Loc.T("Lady Isaura gave you grandmother's box, and it opened.")),
                },
                new()
                {
                    Key = "vell.last", Who = Isaura, After = "vell.sister", Gap = 4, Tier = 3,
                    Brings = (s, rng) =>
                    {
                        // Grandmother warded her heart in it; her riddle is about it. Given, to whoever gave the first box back.
                        var chest = Fill(rng, "strongroom", Finer(s.Tier, 1), s.Tier, 2);
                        chest.Ward(new Item(Category.Gems, Finer(s.Tier, 1)));
                        if (!s.Flagged("vell.given"))
                            return Market.Price(rng, Sellers.Get(Isaura), chest, s.Tier, boosts: s.Boosts, origin: Origins.Get("strongroom"),
                                pitch: Loc.T("Grandmother's last trick box. Her riddle: \"{clue}\" Whoever names her heart may keep it. {price}."));
                        var gift = Given(s, rng, Isaura, chest, Loc.T("Grandmother's last trick box, and you gave me the first one back. Her riddle: " +
                                                                      "\"{clue}\" Whoever names her heart may keep it. One coin, for luck."), "strongroom");
                        return gift;
                    },
                    Bought = (s, v) => s.Log("vell", Loc.T("You bought grandmother Vell's last trick box.")),
                },
            },
        },

        // The Defector: a demon who wants out of the Maw's service.
        new()
        {
            Key = "defector", Title = Loc.T("The Defector"), Place = TalePlace.Emporium,
            Blurb = Loc.T("Skrae, a demon with bat's ears, has heard enough of the Maw and wants out. It needs a back room to hide in for three " +
                          "nights, and the Maw's couriers are looking for it."),
            Ending = DefectorEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "skrae.ask", Who = Skrae, Tier = 4, Gap = 15, If = s => s.Day > s.TierSince,
                    Says = s => Say(Skrae,
                        Loc.T("I've heard enough of the Maw, Keywright. Every word he says to the Guild, I hear. I want out."),
                        Loc.T("Hide me in your back room three nights, and I'll tell you what I've heard. You'll want to know.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Hide it"), () =>
                        {
                            s.Flags.Add("skrae.hidden");
                            s.Flags.Add($"away:{Skrae}");
                            s.Log("defector", Loc.T("You hid Skrae in your back room."));
                            return Loc.T("Three nights. I'll be quiet. I'm very good at quiet.");
                        }, W(s, s.Purses(1.2f)), kind: true),
                        C(Loc.T("No"), () => Loc.T("Then I'll go back. Forget you saw me.")),
                    },
                },
                new()
                {
                    Key = "skrae.tick", Who = Tick, After = "skrae.ask", Gap = 1, Tier = 4, If = s => s.Flagged("skrae.hidden"),
                    Says = s => Say(Tick, Loc.T($"Seen a demon with bat's ears? Ran off from the Gate. The Maw pays {Money.Say(s.Purses(0.5f))} for runaways. Tick, tock.")),
                    Asks = s => new()
                    {
                        C(Loc.T("No"), () =>
                        {
                            s.Log("defector", Loc.T("Tick came looking for Skrae. You said nothing."));
                            return Loc.T("Tick, tock. We'll find it.");
                        }, W(s, s.Purses(1.2f)), kind: true),
                        C(Loc.T($"In my back room ({s.Purses(0.5f)}g)"), () =>
                        {
                            s.TaleGold(s.Purses(0.5f));
                            s.Goes(Skrae);
                            s.Flags.Add("skrae.sold");
                            s.Log("defector", Loc.T("You gave Skrae up to Tick, for the Maw's reward."));
                            return Loc.T("Delicious. The Maw thanks you, mortal.");
                        }, s.Purses(0.5f)),
                    },
                },
                new()
                {
                    Key = "skrae.free", Who = Skrae, Night = true, After = "skrae.tick", Gap = 2, Tier = 4,
                    If = s => s.Flagged("skrae.hidden") && !s.Flagged("skrae.sold"),
                    Says = s => Say(Skrae,
                        Loc.T("They've stopped looking. Here's what I heard: the Maw's gaze is greed, and greed only catches where there's gold " +
                              "on show. Keep your counter bare when he comes, and his gaze slides right off."),
                        Loc.T("And this, from his hoard. He won't miss it. Goodbye, Keywright. I'm going somewhere quiet.")),
                    Then = s =>
                    {
                        s.Flags.Add(GazeKnown);
                        s.Gather("word");
                        s.Flags.Remove($"away:{Skrae}");
                        string gift = GiftCurio(s, new Random(unchecked(s.Seed * 53 + s.Day)), CurioRarity.Uncommon);
                        s.Log("defector", Loc.T($"Skrae went free. The Maw's gaze won't touch your counter again, and it left you {gift}."));
                    },
                },
            },
        },
    };

    /// <summary>Every tale a season can draw: 0.28's, and 0.30's from Brindle, Thornwick, Crownhold and the Emporium (TaleBook.*.cs).</summary>
    public static readonly TaleDef[] All = CampaignTales().Concat(Season28).Concat(BrindleTales()).Concat(ThornwickTales()).Concat(CrownholdTales()).Concat(EmporiumTales())
        .Concat(KinTales()).Concat(LateTales()).ToArray();

    static readonly Dictionary<string, Beat> Beats = All.SelectMany(t => t.Beats).ToDictionary(b => b.Key);

    /// <summary>For the sim's comparisons: a season with no tales at all (the tools are still brought to the counter).</summary>
    public static bool Enabled = true;

    public static Beat Find(string key) => key != null && Beats.TryGetValue(key, out var beat) ? beat : null;

    public static TaleDef Get(string key) => All.FirstOrDefault(t => t.Key == key);

    public static bool Exists(string key) => All.Any(t => t.Key == key);

    /// <summary>
    /// The tales a season holds: the Shut Chests always, then two of Brindle's three, one of Thornwick's two, two of
    /// Crownhold's four, and the Emporium's.
    /// </summary>
    /// <summary>For the bot (--tales=a,b): tales every season holds besides what it draws, to check them on demand.</summary>
    public static List<string> Forced { get; } = new();

    public static List<string> Draw(int seed)
    {
        if (!Enabled) return new List<string>();
        var rng = new Random(unchecked(seed * 31337 + 7));
        var drawn = All.Where(t => t.Place == TalePlace.Always).Select(t => t.Key).ToList();
        // 0.30: three of each place's tales (of seven to nine) over the longer season; 0.32: all three of the Emporium's, so
        // the late game has its own stories. The tale of whoever minds the stall is added by the run (TaleBook.Own).
        foreach (var (place, count) in new[] { (TalePlace.Brindle, 3), (TalePlace.Thornwick, 3), (TalePlace.Crownhold, 3), (TalePlace.Emporium, 3) })
            drawn.AddRange(All.Where(t => t.Place == place).OrderBy(_ => rng.Next()).Take(count).Select(t => t.Key));
        drawn.AddRange(Forced.Where(k => Exists(k) && !drawn.Contains(k)));
        return drawn;
    }

    /// <summary>A tale's secret for a season (who the Magpie is), from the seed and the tale's name: the same all season.</summary>
    public static string Secret(int seed, string tale)
    {
        var def = Get(tale);
        if (def == null || def.Secrets.Length == 0) return null;
        int hash = tale.Aggregate(17, (h, c) => unchecked(h * 31 + c));
        return def.Secrets[new Random(unchecked(seed * 7919 + hash)).Next(def.Secrets.Length)];
    }

    /// <summary>What a witness saw of the Magpie: the first clue or the second, which together fit only her.</summary>
    public static string Clue(GameState s, int which)
    {
        string magpie = s.Secret("magpie");
        // Each clue fits two of the three suspects; the two a season gives fit only the Magpie.
        string[] order = magpie switch
        {
            Bettany => new[] { "left", "ink" },
            Hesper => new[] { "ink", "boots" },
            _ => new[] { "boots", "left" },
        }; // loc: keys
        return order[Math.Clamp(which, 0, 1)] switch
        {
            "left" => Loc.T("she held the lantern in her right hand and the crowbar in her left"),
            "ink" => Loc.T("she left inky fingermarks all over the gate"),
            _ => Loc.T("she went over the wall in riding boots, and rode off"),
        };
    }

    /// <summary>Names someone for the Magpie's bounty: the truth pays, and puts her away; a mistake puts away someone else.</summary>
    static string Accuse(GameState s, string who)
    {
        s.Goes(who);
        if (who == s.Secret("magpie"))
        {
            s.TaleGold(s.Purses(0.5f));
            s.Sway(Faction.Wardens, 2);
            s.Sway(Faction.Commons, 1);
            s.Sway(Faction.Lockless, -3);
            s.Flags.Add("magpie.caught");
            s.Log("magpie", Loc.T($"You named {Name(who)}, and she was the Magpie. The Guild's foreclosed chests went home to their families."));
            return Loc.T($"We found her hoard under {Hideout(who)}: every chest the Guild foreclosed on this year. They're going home. Here's your bounty.");
        }
        s.Sway(Faction.Wardens, -1);
        s.Sway(Faction.Commons, -1);
        s.Flags.Add("magpie.wrong");
        s.Log("magpie", Loc.T($"You named {Name(who)} for the Magpie. The Wardens took her in. They found nothing."));
        return Loc.T("Then we'll take her in. If you're wrong, Keywright, it's on your head.");
    }

    static string Hideout(string who) => who switch
    {
        Bettany => Loc.T("the Crossed Keys' cellar"),
        Hesper => Loc.T("a hayloft full of wedding chests"),
        _ => Loc.T("her hunting lodge in the Old Wood"),
    };

    /// <summary>The Guild's witness fee for the Wood's deed: a share of your next due.</summary>
    static int WitnessFee(GameState s, float share) => Money.Round(s.NextDue * share);
}
