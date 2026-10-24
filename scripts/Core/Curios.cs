using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

public enum CurioRarity { Common, Uncommon, Rare }

/// <summary>
/// What a curio is about: the way of trading it rewards. Curios that share a tag make each other better, and Nim, who
/// knows a collector when he sees one, brings more of what you already keep (<see cref="Curios.Stock"/>).
/// </summary>
public enum CurioTag { Luck, Reading, Bargain, Crowd, Display, Kind, Hollows, Arcana, Rough, Fence, Coffers, Patrons, Song }

/// <summary>
/// When Nim starts carrying a curio: from the first run, or once Bram's kin have been somewhere or done something. His
/// stock grows across runs (<see cref="Profile.Opens"/>): a first run meets the plainer half, and each later one
/// something new to build around.
/// </summary>
public enum CurioUnlock { Start, SecondRun, Crownhold, Emporium, Cleared, Heartsong }

/// <summary>
/// A curio: an oddment from the Locking, from Nim the peddler, that bends one of the shop's rules for as long as you own
/// it. They're the shape of a run: room for a few (more with a curio case), and which ones you keep decides which chests
/// you want, how you handle them, how you trade, and what you risk. None of them tells you what a chest is worth before
/// you buy it. What they put on a chest they put on what you <i>named</i> on the slip (<see cref="Item.Named"/>): a
/// curio amplifies your judgment, it doesn't replace it, and a wrong read forfeits it.
/// </summary>
/// <param name="Icon">Icon under res://assets/ui/curios/.</param>
/// <param name="MinTier">Nim only offers it from this shop tier (where it does anything).</param>
/// <param name="Tags">The ways of trading it rewards (the first is its main one).</param>
/// <param name="Kind">For a trade's curio (<see cref="CurioTag.Kind"/>): the kind of thing it's for.</param>
public sealed record CurioDef(string Key, string Name, string Blurb, CurioRarity Rarity, string Icon, int MinTier, CurioTag[] Tags, Category? Kind = null)
{
    public bool Has(CurioTag tag) => Tags.Contains(tag);

    /// <summary>When Nim starts carrying it.</summary>
    public CurioUnlock Unlock => Curios.UnlockOf(Key);
}

public static class Curios
{
    // What each effect does, for the rules (GameState, Build.cs) and the blurbs.
    public const float InterestRate = 0.1f, TicketRate = 0.8f, LoupeFakes = 0.25f, InkFakes = 1f / 3, WatchPatience = 2f, TokenSwing = 0.25f,
        FootLuck = 0.05f, WheelLuck = 0.33f, HorseshoeStep = 0.06f, HorseshoeMax = 0.3f, WishboneBonus = 1.5f, SealFee = 0.2f, ForkStep = 0.05f,
        ForkMax = 0.5f, LedgerRight = 1.25f, LedgerWrong = 0.75f, TongueFloor = 0.8f, WaitStep = 0.1f, WaitMax = 0.4f, BellPrice = 0.6f,
        CofferShare = 1f, QueuePrice = 0.8f, HornDearer = 1.1f, WhetstoneBonus = 1.75f, TradeBonus = 1.5f, ScalesStep = 0.2f, StillBroken = 0.5f,
        AetherStep = 0.03f, AetherMax = 0.6f, LockboxShare = 0.3f, PillowShare = 1f / 3, NetSongs = 3f, RibbonPay = 2f, CrescentPrices = 0.8f,
        TradeLuck = 0.15f, LampsPerPoint = 0.015f, RingMagic = 4f, SignetFloor = 0.9f, MarkCrooks = 2f;

    public const int InterestPerTier = 120, CataloguePerTier = 6, BookPerPoint = 8, ShellPityAfter = 5, LanternCustomers = 1, HornCustomers = 2,
        ErrandCommissions = 2, DummyPrestige = 3, RopeExhibition = 2, HookHidden = 2, DraughtMagic = 2, GuestbookEvery = 3;

    static readonly CurioTag[] Luck = { CurioTag.Luck }, Reading = { CurioTag.Reading }, Bargain = { CurioTag.Bargain }, Crowd = { CurioTag.Crowd },
        Display = { CurioTag.Display }, Kind = { CurioTag.Kind }, Hollows = { CurioTag.Hollows }, Arcana = { CurioTag.Arcana }, Rough = { CurioTag.Rough },
        Fence = { CurioTag.Fence }, Coffers = { CurioTag.Coffers }, Patrons = { CurioTag.Patrons };

    static CurioTag[] Both(CurioTag a, CurioTag b) => new[] { a, b };

    const CurioRarity C = CurioRarity.Common, U = CurioRarity.Uncommon, R = CurioRarity.Rare;

    public static readonly CurioDef[] All =
    {
        // Luck: what you named comes out better than its chest, and what that's worth.
        new("coin", "Lucky Coin", "The first chest you buy each day: everything you named gets two tries at its quality.", U, "coin", 1, Luck),
        new("shell", "Lucky Shell", $"A dry spell ends twice as soon: luck starts to turn after {ShellPityAfter} plain finds, not {GameState.PityAfter}.", C, "shell", 1, Luck),
        new("foot", "Rabbit's Foot", $"Everything you name is {Pct(FootLuck)} likelier to come out a step better than its chest.", U, "foot", 1, Luck),
        new("wishbone", "Wishbone", "Something you named that comes out better than its chest sells for half as much again.", U, "wishbone", 1, Luck),
        new("horseshoe", "Horseshoe", $"Luck runs in streaks: each chest with a surprise in it makes the next {Pct(HorseshoeStep)} luckier (up to {Pct(HorseshoeMax)}).",
            U, "horseshoe", 2, Luck),
        new("token", "Gambler's Token", "Luck swings both ways: twice as many surprises, good and bad, in everything you open.", U, "token", 1,
            Both(CurioTag.Luck, CurioTag.Rough)),
        new("clover", "Four-Leaf Clover", "The first chest you buy each week: everything you named comes out a step better.", R, "clover", 1, Luck),
        new("star", "Shooting Star", "Something you named that comes out better than its chest climbs a step further.", R, "star", 2, Luck),

        // Reading: the perfect read, and knowing more to make one.
        new("trumpet", "Ear Trumpet", "Nothing drowns out or muffles a sound for you: no clank, and no straw.", C, "trumpet", 1, Reading),
        new("crescent", "Cursed Crescent", $"No surprises, no letdowns: everything comes out exactly as its chest promised. Sellers sense the curse and ask {Pct(1 - CrescentPrices)} less.",
            U, "crescent", 1, Both(CurioTag.Bargain, CurioTag.Reading)),
        new("seal", "Appraiser's Seal", $"A perfect read pays you a valuer's fee: {Pct(SealFee)} of what the chest held.", U, "seal", 1, Reading),
        new("beads", "Counting Beads", "Every chest comes with a tally of how many things are in it, false bottom and all.", C, "beads", 2,
            Both(CurioTag.Reading, CurioTag.Hollows)),
        new("monocle", "Witch's Monocle", "Enchanted things glow when they sound, witchlight or not.", C, "monocle", 3, Both(CurioTag.Reading, CurioTag.Arcana)),
        new("fork", "Tuning Fork", $"Each perfect read in a row: what you name in the next chest sells for {Pct(ForkStep)} more (up to half as much again). A wrong slip starts again.",
            R, "fork", 1, Reading),
        new("ledger", "Ledger of Certainty", "A perfect read: everything in the chest sells for a quarter more. A wrong slip: a quarter less.", R, "ledger", 2, Reading),

        // Bargain: what you pay.
        new("signet", "Merchant's Signet", $"Hagglers hear you out (an offer too low gets a second try), and come down {Pct(1 - SignetFloor)} further.", C, "signet", 1, Bargain),
        new("tongue", "Silver Tongue", "Hagglers come down a fifth further, and sellers who wait for your offer take a fifth less.", U, "tongue", 1, Bargain),
        new("hourglass", "Waiting Game", $"Every chest you pass makes the next seller ask {Pct(WaitStep)} less (up to {Pct(WaitMax)}). Buying starts it again.",
            U, "hourglass", 1, Both(CurioTag.Bargain, CurioTag.Crowd)),
        new("bell", "Closing Bell", $"The last customer of the day asks {Pct(1 - BellPrice)} less.", U, "bell", 1, Bargain),
        new("coffer", "Haggler's Purse", "Everyone haggles with you, and whatever you talk them down by comes back to you a second time.", R, "coffer", 1,
            Both(CurioTag.Bargain, CurioTag.Coffers)),

        // Crowd: who comes in.
        new("lantern", "Shop Lantern", "Open late: one more customer every day.", U, "lantern", 1, Crowd),
        new("queue", "Queue Ticket", "Anyone you pass comes back at the end of the day asking a fifth less. Once each.", U, "queue", 1, Both(CurioTag.Crowd, CurioTag.Bargain)),
        new("horn", "Barker's Horn", "Two more customers every day, but word's out: everyone asks a tenth more.", U, "horn", 2, Crowd),
        new("guestbook", "Guest Book", "Every third customer of the day brings a chest a colour better.", U, "guestbook", 2, Both(CurioTag.Crowd, CurioTag.Luck)),

        // Display: what you keep.
        new("catalogue", "Collector's Catalogue", $"Every new find in your journal pays {CataloguePerTier}g (more in a grander shop).", C, "catalogue", 1, Display),
        new("dome", "Bell Jar", "Room to display one more thing.", C, "dome", 2, Display),
        new("book", "Visitors' Book", $"Every night, visitors leave {BookPerPoint}g for each point of your exhibition (more in a grander shop).", U, "book", 2,
            Both(CurioTag.Display, CurioTag.Coffers)),
        new("rope", "Velvet Rope", "Your exhibition counts double.", U, "rope", 2, Display),
        new("lamps", "Gallery Lamps", $"Your exhibition sells for you: what you name fetches {Pct(LampsPerPoint)} more for each point of it (up to {Pct(LampsPerPoint * GameState.ExhibitionCap)}).",
            R, "lamps", 2, Display),

        // The trades: one kind of thing.
        new("gauntlet", "Smith's Glove", $"You mend weapons: they never come out worse than their chest promised, and those you name are {Pct(TradeLuck)} likelier to come out better.",
            C, "gauntlet", 1, Both(CurioTag.Kind, CurioTag.Luck), Category.Arms),
        new("cap", "Hatter's Cap", $"You mend cloaks: they never come out worse than their chest promised, and those you name are {Pct(TradeLuck)} likelier to come out better.",
            C, "cap", 1, Both(CurioTag.Kind, CurioTag.Luck), Category.Garb),
        new("whetstone", "Whetstone", "Weapons you name sell for three-quarters more.", U, "whetstone", 1, Kind, Category.Arms),
        new("dummy", "Tailor's Dummy", $"Cloaks you name sell for half as much again, and count {DummyPrestige} times over on display.", U, "dummy", 1,
            Both(CurioTag.Kind, CurioTag.Display), Category.Garb),
        new("scales", "Moneychanger's Scales", "Coin you name sells for a quarter more for every purse of it in the chest.", U, "scales", 1, Kind, Category.Coins),
        new("still", "Alchemist's Still", "Potions you name sell for half as much again, and broken glass still sells for half.", U, "still", 2,
            Both(CurioTag.Kind, CurioTag.Rough), Category.Potions),
        new("desk", "Scribe's Desk", "Spells you name sell for half as much again, and sand sells as ink, for a third of a spell.", U, "desk", 2,
            Both(CurioTag.Kind, CurioTag.Fence), Category.Scrolls),
        new("wheel", "Gem Cutter's Wheel", $"Gems you name are {Pct(WheelLuck)} likelier to come out a step better.", U, "wheel", 3, Both(CurioTag.Kind, CurioTag.Luck),
            Category.Gems),

        // Hollows: false bottoms and wards.
        new("hook", "Smuggler's Hook", "Smugglers come your way: false bottoms turn up twice as often.", U, "hook", 2, Hollows),
        new("crate", "Padded Crate", "Glass never breaks when you drop a chest.", C, "crate", 2, Both(CurioTag.Hollows, CurioTag.Rough)),
        new("rod", "Hollow Rod", "What's under a false bottom you found comes out a step better: people hid their best.", U, "rod", 2, Hollows),
        new("pillow", "Sleeper's Pillow", "Sellers dream true for you: a third of them have warded something, and tell you their dream.", U, "pillow", 2, Hollows),
        new("wardkey", "Ward-Key", "A warded thing you name comes out two steps better.", R, "wardkey", 2, Both(CurioTag.Hollows, CurioTag.Reading)),

        // Arcana: enchantment.
        new("draught", "Witch's Draught", "Enchanted things turn up twice as often.", U, "draught", 3, Arcana),
        new("aether", "Aether Jar", $"Every enchanted thing you name: what you name sells for {Pct(AetherStep)} more, for the rest of the run (up to {Pct(AetherMax)}).",
            U, "aether", 3, Arcana),
        new("ring", "Spellbinder's Ring", "Enchanted things you name are worth four times as much, not twice.", R, "ring", 3, Arcana),

        // Rough hands: locks and breakage.
        new("oil", "Locksmith's Oil", "Every lock has one pin fewer.", C, "oil", 2, Rough),
        new("knife", "Burglar's Knife", "Forcing a lock breaks nothing and scratches nothing.", U, "knife", 2, Rough),
        new("gloves", "Kid Gloves", "Nobody minds a hard shake, a drop or the lodestone on their chest.", U, "gloves", 2, Both(CurioTag.Rough, CurioTag.Reading)),

        // The fence: fakes.
        new("loupe", "Jeweller's Loupe", "Paste gems and demon gold sell as costume pieces: a quarter of the real thing.", C, "loupe", 4, Fence),
        new("mark", "Fence's Mark", "Crooks come your way twice as often, and the Maw's buyers take any fake off you for what the real thing would fetch.", R, "mark", 3,
            Both(CurioTag.Fence, CurioTag.Reading)),

        // Coffers: the purse, and bad nights.
        new("purse", "Rainy-Day Purse", "Bram's stash has one more dip in it.", C, "purse", 1, Coffers),
        new("ticket", "Pawn Ticket", "On a short night, what you pawn fetches four-fifths of its price, not three-fifths.", C, "ticket", 1, Coffers),
        new("jar", "Savings Jar", $"Every night: a tenth of your gold as interest (up to {InterestPerTier}g at the stall, more in a grander shop).", U, "jar", 1, Coffers),
        new("marker", "Guildsman's Marker", "Bram's word is good twice a run, not once.", C, "marker", 1, Coffers),
        new("lockbox", "Miser's Lockbox", $"Every Guild night, once you've paid: {Pct(LockboxShare)} of what's left in your purse.", R, "lockbox", 2, Coffers),

        // Patrons: commissions.
        new("letter", "Letter of Introduction", "Regulars take what you deliver a quality lower than they asked for.", U, "letter", 1, Patrons),
        new("ribbon", "Patron's Ribbon", "Commissions pay twice as much.", U, "ribbon", 1, Patrons),
        new("errand", "Errand Book", $"The regulars post {ErrandCommissions} more commissions every week.", U, "errand", 2, Patrons),

        // And the rest.
        new("watch", "Pocket Watch", "Impatient sellers give you twice as long.", U, "watch", 4, Reading),
        new("net", "Songcatcher's Net", "Heartsongs are three times as likely to be in a chest.", R, "net", 1, new[] { CurioTag.Song }),
    };

    static string Pct(float fraction) => Synergies.Pct(fraction);

    /// <summary>
    /// What Nim keeps back until later runs: the second layer of each way of trading after a first run, the capital's and
    /// the Emporium's curios once you've had a shop there (or after two and four runs, so every early run brings something
    /// new), the Shooting Star once the debt's been cleared, the net once a Heartsong's been heard. Everything else he
    /// brings from the start.
    /// </summary>
    static readonly Dictionary<string, CurioUnlock> Unlocks = new()
    {
        ["wishbone"] = CurioUnlock.SecondRun, ["horseshoe"] = CurioUnlock.SecondRun, ["token"] = CurioUnlock.SecondRun,
        ["crescent"] = CurioUnlock.SecondRun, ["tongue"] = CurioUnlock.SecondRun, ["guestbook"] = CurioUnlock.SecondRun,
        ["dummy"] = CurioUnlock.SecondRun, ["scales"] = CurioUnlock.SecondRun, ["rod"] = CurioUnlock.SecondRun, ["pillow"] = CurioUnlock.SecondRun,
        ["monocle"] = CurioUnlock.Crownhold, ["still"] = CurioUnlock.Crownhold, ["desk"] = CurioUnlock.Crownhold, ["wheel"] = CurioUnlock.Crownhold,
        ["aether"] = CurioUnlock.Crownhold, ["horn"] = CurioUnlock.Crownhold, ["rope"] = CurioUnlock.Crownhold, ["errand"] = CurioUnlock.Crownhold,
        ["fork"] = CurioUnlock.Emporium, ["ledger"] = CurioUnlock.Emporium, ["coffer"] = CurioUnlock.Emporium, ["lamps"] = CurioUnlock.Emporium,
        ["ring"] = CurioUnlock.Emporium, ["wardkey"] = CurioUnlock.Emporium, ["mark"] = CurioUnlock.Emporium, ["lockbox"] = CurioUnlock.Emporium,
        ["star"] = CurioUnlock.Cleared, ["net"] = CurioUnlock.Heartsong,
    };

    public static CurioUnlock UnlockOf(string key) => Unlocks.GetValueOrDefault(key, CurioUnlock.Start);

    /// <summary>What opens a curio, as the ledger and the run's end say it: "after your first run".</summary>
    public static string UnlockText(CurioUnlock unlock) => unlock switch
    {
        CurioUnlock.SecondRun => "after your first run",
        CurioUnlock.Crownhold => "once you've had a shop in Crownhold, or after two runs",
        CurioUnlock.Emporium => "once you've had the Royal Emporium, or after four runs",
        CurioUnlock.Cleared => "once you've cleared Bram's debt",
        CurioUnlock.Heartsong => "once you've heard a Heartsong",
        _ => "from the start",
    };

    public static CurioDef Get(string key) => All.First(c => c.Key == key);

    public static bool Exists(string key) => All.Any(c => c.Key == key);

    /// <summary>Room for this many curios, more with a curio case.</summary>
    public const int Slots = 3;

    /// <summary>Nim comes every week, from the first.</summary>
    public const int FirstWeek = 1;

    /// <summary>
    /// A curio's price, as a share of what's in your purse when Nim arrives: the economy grows fast, so a fixed price would
    /// be a fortune early and pocket change late. A curio always costs about as much of your means, and buying one is
    /// always a choice against a renovation, a tool or the Guild.
    /// </summary>
    public static float Share(CurioRarity rarity) => rarity switch
    {
        CurioRarity.Common => 0.12f,
        CurioRarity.Uncommon => 0.2f,
        _ => 0.3f,
    };

    /// <summary>The least Nim takes for one, whatever's in your purse: a share of what a day's chests cost at this shop.</summary>
    public static int Floor(CurioRarity rarity, int tier) => Math.Max(1, (int)(ShopTiers.Working(tier) * rarity switch
    {
        CurioRarity.Common => 0.25f,
        CurioRarity.Uncommon => 0.5f,
        _ => 1f,
    }));

    /// <summary>What Nim asks for a curio, from what was in your purse when he arrived.</summary>
    public static int Price(CurioDef curio, int purse, int tier) => Math.Max(Floor(curio.Rarity, tier), Money.Round(purse * Share(curio.Rarity)));

    /// <summary>How much likelier Nim is to bring a curio that shares a tag with one you keep (he knows a collector).</summary>
    const float Kindred = 1.5f;

    /// <summary>
    /// What Nim lays out on a visit: three curios (four for a familiar face) you don't own and could use, rarer ones more
    /// often as the weeks go by, and ones that go with what you keep more often than not. Leaves out what he's just shown you.
    /// </summary>
    public static List<CurioDef> Stock(Random rng, GameState state, int count = 3, ICollection<string> shown = null)
    {
        var pool = All.Where(c => c.MinTier <= state.Tier && state.CurioOpen(c.Key) && !state.HasCurio(c.Key) && !state.NoUse(c) &&
                                  (shown == null || !shown.Contains(c.Key))).ToList();
        var stock = new List<CurioDef>();
        var owned = state.OwnedCurios.SelectMany(c => c.Tags).ToHashSet();
        float rare = Math.Min(0.3f, 0.06f + 0.04f * state.Week), uncommon = 0.38f;
        while (stock.Count < count && pool.Count > 0)
        {
            var pick = rng.PickWeighted(pool.ToArray(), c => c.Rarity switch
            {
                CurioRarity.Rare => rare,
                CurioRarity.Uncommon => uncommon,
                _ => 1 - rare - uncommon,
            } / Math.Max(1, pool.Count(o => o.Rarity == c.Rarity)) * (c.Tags.Any(owned.Contains) ? 1 + Kindred : 1));
            stock.Add(pick);
            pool.Remove(pick);
        }
        // Nim's first visit always has something cheap.
        var commons = pool.Where(c => c.Rarity == CurioRarity.Common).ToArray();
        if (state.Week <= FirstWeek && stock.Count > 0 && stock.All(c => c.Rarity != CurioRarity.Common) && commons.Length > 0)
            stock[^1] = rng.PickOne(commons);
        return stock;
    }

    public static string RarityName(CurioRarity rarity) => rarity.ToString().ToLowerInvariant();

    /// <summary>A tag as the cards show it: "luck", "the trades: weapons".</summary>
    public static string TagName(CurioTag tag, CurioDef curio = null) => tag switch
    {
        CurioTag.Kind when curio?.Kind is Category kind => Categories.Get(kind).Name.ToLowerInvariant(),
        CurioTag.Kind => "a trade",
        CurioTag.Reading => "reading",
        CurioTag.Bargain => "bargains",
        CurioTag.Crowd => "crowds",
        CurioTag.Display => "display",
        CurioTag.Hollows => "hollows",
        CurioTag.Arcana => "arcana",
        CurioTag.Rough => "rough hands",
        CurioTag.Fence => "fakes",
        CurioTag.Coffers => "coffers",
        CurioTag.Patrons => "patrons",
        CurioTag.Song => "Heartsongs",
        _ => "luck",
    };

    /// <summary>"luck, reading": the tags a card shows.</summary>
    public static string Tags(CurioDef curio) => string.Join(", ", curio.Tags.Select(t => TagName(t, curio)));

    /// <summary>What the opening says a curio (or a Heartsong on display) did to a chest: "Lucky Coin: 2 things came out better!"</summary>
    public static string LuckLine(string key, int things)
    {
        string n = $"{things} thing{(things == 1 ? "" : "s")}";
        string name = Exists(key) ? Get(key).Name : Heartsongs.Exists(key) ? Heartsongs.Get(key).Name : key == "restorer" ? "Your restorers" : key;
        return key switch
        {
            "token" => $"{name}: {n} swung!",
            "gauntlet" or "cap" => $"{name}: {n} mended.",
            "crescent" => $"{name}: {n} held steady.",
            Heartsongs.Ember => $"{name}: {n} tempered!",
            _ => $"{name}: {n} came out better!",
        };
    }
}

/// <summary>The peddler who sells curios.</summary>
public static class Peddler
{
    public const string Name = "Nim the Peddler";

    public static readonly string[] Greetings =
    {
        "Psst. Curios! Lucky, cursed, and everything in between.",
        "Back again! Nim's got just the thing. Several things, actually.",
        "Don't tell the Guild I was here. Now, what catches your eye?",
        "Fresh from a dead wizard's attic. Mostly fresh.",
    };

    public static readonly string[] Thanks = { "A fine choice. Mostly harmless.", "Pleasure doing business!", "It likes you. I can tell." };
    public static readonly string[] Bye = { "Next week, then. Same time, roughly.", "Off I go. Mind the Guild." };
}
