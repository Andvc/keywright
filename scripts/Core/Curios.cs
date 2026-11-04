using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>How rare a curio is: what it costs, and how often (and how late in a run) Nim has one.</summary>
public enum CurioRarity { Common, Uncommon, Rare, Legendary }

/// <summary>
/// What a curio is about: the way of trading it rewards. Curios that share a tag make each other better, and Nim, who
/// knows a collector when he sees one, brings more of what you already keep (<see cref="Curios.Stock"/>). Six are the
/// main builds, each a different way to play: enchantment, buying sight unseen, haggling, the perfect read, false
/// bottoms and wards. The rest feed or stand beside them.
/// </summary>
public enum CurioTag { Luck, Appraisal, Haggling, Crowds, Display, Trade, Compartments, Wards, Enchantment, Blind, Rough, Fakes, Coffers, Patrons, Song }

/// <summary>
/// When Nim starts carrying a curio: from the first run, or once Bram's kin have been somewhere or done something
/// (a legendary one: done its trade often enough, across runs: <see cref="Feat"/>). His stock grows across runs
/// (<see cref="Profile.Opens"/>): a first run meets the plainer half, and each later one something new to build around.
/// </summary>
public enum CurioUnlock { Start, SecondRun, Crownhold, Emporium, Cleared, Heartsong, Feat }

/// <summary>What Bram's kin have done across every run, that opens a legendary curio (<see cref="Profile.Feats"/>).</summary>
public enum Feat { Surprises, PerfectReads, Bargains, FalseBottoms, Wards, Enchanted, SightUnseen }

/// <summary>
/// A curio: an oddment from the Locking, from Nim the peddler, that bends one of the shop's rules for as long as you own
/// it. They're the shape of a run: room for a few (more with a curio case), and which ones you keep decides which chests
/// you want, how you handle them, how you trade, and what you risk. None of them tells you what a chest is worth before
/// you buy it. What one puts on a chest it puts on everything in it; what makes ordinary things worth more puts every
/// seller's price up as much (the shop's power index, <see cref="GameState.PowerIndex"/>), so the edge a build gives is
/// in what no seller can price.
/// </summary>
/// <param name="Icon">Icon under res://assets/ui/curios/.</param>
/// <param name="MinTier">Nim only offers it from this shop tier (where it does anything).</param>
/// <param name="Tags">The ways of trading it rewards (the first is its main one).</param>
/// <param name="Kind">For a trade's curio (<see cref="CurioTag.Trade"/>): the kind of thing it's for.</param>
public sealed record CurioDef(string Key, string Name, string Blurb, CurioRarity Rarity, string Icon, int MinTier, CurioTag[] Tags, Category? Kind = null)
{
    public bool Has(CurioTag tag) => Tags.Contains(tag);

    /// <summary>When Nim starts carrying it.</summary>
    public CurioUnlock Unlock => Curios.UnlockOf(Key);
}

public static class Curios
{
    // What each effect does, for the rules (GameState, Build.cs) and the blurbs.
    public const float InterestRate = 0.03f, TicketRate = 0.8f, LoupeFakes = 0.25f, InkFakes = 1f / 3, WatchPatience = 2f, TokenSwing = 0.25f,
        FootLuck = 0.1f, WheelLuck = 0.5f, HorseshoeStep = 0.1f, HorseshoeMax = 0.5f, WishboneBonus = 1.5f, SealFee = 0.08f, ForkStep = 0.03f,
        ForkMax = 0.3f, LedgerRight = 1.15f, LedgerWrong = 0.85f, TongueFloor = 0.8f, WaitStep = 0.05f, WaitMax = 0.2f, BellPrice = 0.75f,
        CofferShare = 0.4f, QueuePrice = 0.8f, HornDearer = 1.05f, WhetstoneBonus = 1.75f, TradeBonus = 1.5f, ScalesStep = 0.2f, StillBroken = 0.5f,
        LockboxShare = 0.25f, PillowShare = 0.2f, NetSongs = 3f, RibbonPay = 1.5f, CrescentPrices = 0.95f, TradeLuck = 0.25f, LampsPerPoint = 0.015f,
        RingMagic = 4f, SignetFloor = 0.9f, MarkCrooks = 2f,
        // 0.21's engines, the sight-unseen buyer's, the haggler's and the reader's.
        AetherStep = 0.02f, AetherMax = 0.2f, SpindleStep = 0.02f, SpindleMax = 0.5f, DipLuck = 0.15f, SealedPrice = 0.85f, BlindfoldPrice = 0.7f,
        BondShare = 0.4f, WholesaleStep = 0.04f, WholesaleMax = 0.2f, KnotStep = 0.01f, KnotMax = 0.08f, GoldenFloor = 0.9f, TruthglassBonus = 1.5f,
        RegisterStep = 0.005f, RegisterMax = 0.12f, LogStep = 0.02f, LogMax = 0.2f, PassBonus = 2f, WardTallyStep = 0.1f;

    public const int InterestPerTier = 20, CataloguePerTier = 4, BookPerPoint = 6, ShellPityAfter = 5, LanternCustomers = 1, HornCustomers = 2,
        ErrandCommissions = 2, DummyPrestige = 3, RopeExhibition = 2, HookHidden = 2, DraughtMagic = 3;

    static CurioTag[] T(params CurioTag[] tags) => tags;

    const CurioTag Luck = CurioTag.Luck, Appraisal = CurioTag.Appraisal, Haggling = CurioTag.Haggling, Crowds = CurioTag.Crowds, Display = CurioTag.Display,
        Trade = CurioTag.Trade, Compartments = CurioTag.Compartments, Wards = CurioTag.Wards, Enchantment = CurioTag.Enchantment, Blind = CurioTag.Blind,
        Rough = CurioTag.Rough, Fakes = CurioTag.Fakes, Coffers = CurioTag.Coffers, Patrons = CurioTag.Patrons;

    const CurioRarity C = CurioRarity.Common, U = CurioRarity.Uncommon, R = CurioRarity.Rare, L = CurioRarity.Legendary;

    public static readonly CurioDef[] All =
    {
        // Enchantment: nobody can hear it, and no seller prices it. See it glow, find more of it, make it worth more.
        new("monocle", Loc.T("Witch's Monocle"), Loc.T("Enchanted things glow when they sound, witchlight or not."), C, "monocle", 3, T(Enchantment, Appraisal)),
        new("draught", Loc.T("Witch's Draught"), Loc.T("Enchanted things turn up three times as often."), U, "draught", 3, T(Enchantment)),
        new("aether", Loc.T("Aether Jar"), Loc.T($"Each enchanted thing you find this run makes everything {Pct(AetherStep)} likelier to be enchanted (up to {Pct(AetherMax)})."),
            U, "aether", 3, T(Enchantment)),
        new("chalk", Loc.T("Enchanter's Chalk"), Loc.T("An enchantment spreads: a chest with an enchanted thing in it has a second one."), U, "chalk", 3, T(Enchantment)),
        new("ring", Loc.T("Spellbinder's Ring"), Loc.T("Enchanted things are worth four times as much, not twice."), R, "ring", 3, T(Enchantment)),
        new("staff", Loc.T("Archmage's Staff"), Loc.T("Everything in a chest with an enchanted thing in it is enchanted."), L, "staff", 3, T(Enchantment)),

        // Sight unseen: buy a chest without laying a hand on it. Cheaper, luckier, insured: the build that needs no ear.
        new("sealed", Loc.T("Sealed Bid"), Loc.T($"Buy a chest sight unseen and its seller knocks {Pct(1 - SealedPrice)} off."),
            C, "sealed", 1, T(Blind)),
        new("dip", Loc.T("Lucky Dip"), Loc.T($"Everything in a chest you take sight unseen is {Pct(DipLuck)} likelier to come out a step better."), U, "dip", 1, T(Blind, Luck)),
        new("bond", Loc.T("Underwriter's Bond"), Loc.T($"A chest bought sight unseen holds less than you paid? The Underwriters pay {Pct(BondShare)} of it back."),
            U, "bond", 2, T(Blind, Coffers)),
        new("wholesale", Loc.T("Wholesaler's Tally"), Loc.T($"Each chest you buy makes every seller after it today ask {Pct(WholesaleStep)} less (up to {Pct(WholesaleMax)})."),
            U, "wholesale", 2, T(Blind, Haggling)),
        new("token", Loc.T("Gambler's Token"), Loc.T("Twice as many surprises in everything you open: good ones and bad ones."), U, "token", 1, T(Luck, Blind)),
        new("blindfold", Loc.T("Fortune's Blindfold"), Loc.T($"Sight unseen, a chest costs {Pct(1 - BlindfoldPrice)} less, and nothing in it comes out worse than promised."), L,
            "blindfold", 3, T(Blind)),

        // Haggling: what you pay. Talk them down, and word gets round.
        new("signet", Loc.T("Merchant's Signet"), Loc.T($"Hagglers hear you out (an offer too low gets a second try), and come down {Pct(1 - SignetFloor)} further."), C, "signet", 1,
            T(Haggling)),
        new("tongue", Loc.T("Silver Tongue"), Loc.T("Hagglers come down a fifth further, and those who wait for an offer take a fifth less."), U, "tongue", 1, T(Haggling)),
        new("knot", Loc.T("Haggler's Knot"), Loc.T($"Each chest you talk down this run makes every seller ask {Pct(KnotStep)} less (up to {Pct(KnotMax)})."),
            U, "knot", 2, T(Haggling)),
        new("hourglass", Loc.T("Waiting Game"), Loc.T($"Every chest you pass makes the next seller ask {Pct(WaitStep)} less (up to {Pct(WaitMax)}). Buying starts it again."),
            U, "hourglass", 1, T(Haggling, Crowds)),
        new("bell", Loc.T("Closing Bell"), Loc.T($"The last customer of the day asks {Pct(1 - BellPrice)} less."), U, "bell", 1, T(Haggling)),
        new("coffer", Loc.T("Haggler's Purse"), Loc.T($"Everyone haggles with you, and {Pct(CofferShare)} of whatever you talk them down by comes back to you as well."), R, "coffer", 2,
            T(Haggling, Coffers)),
        new("goldentongue", Loc.T("Golden Tongue"), Loc.T($"Everyone haggles and nobody walks out: go as low as you like, and they come down to {Pct(1 - GoldenFloor)} under the least they'd take."),
            L, "goldentongue", 3, T(Haggling)),

        // The perfect read: count every thing, mark every glow and boom, and be paid for it.
        new("trumpet", Loc.T("Ear Trumpet"), Loc.T("Nothing drowns out or muffles a sound for you: no clank, and no straw."), C, "trumpet", 1, T(Appraisal)),
        new("watch", Loc.T("Pocket Watch"), Loc.T("Impatient sellers give you twice as long, and a rotten chest stands twice the shaking."), C, "watch", 1, T(Appraisal)),
        new("beads", Loc.T("Counting Beads"), Loc.T("Every chest comes with a tally of how many things are in it, false bottom and all."), C, "beads", 2, T(Appraisal, Compartments)),
        new("seal", Loc.T("Appraiser's Seal"), Loc.T($"A perfect read pays you a valuer's fee: {Pct(SealFee)} of what the chest held."), U, "seal", 1, T(Appraisal)),
        new("register", Loc.T("Valuer's Register"), Loc.T($"A perfect read pays {Fine(RegisterStep)} of what the chest held for each one so far this run (up to {Pct(RegisterMax)})."),
            U, "register", 2, T(Appraisal)),
        new("crescent", Loc.T("Cursed Crescent"), Loc.T($"Everything comes out exactly as its chest promised. Sellers sense the curse and ask {Pct(1 - CrescentPrices)} less."),
            U, "crescent", 1, T(Appraisal, Haggling)),
        new("fork", Loc.T("Tuning Fork"), Loc.T($"Each perfect read in a row: the next chest's things sell for {Pct(ForkStep)} more (up to {Pct(ForkMax)})."),
            R, "fork", 1, T(Appraisal)),
        new("ledger", Loc.T("Ledger of Certainty"), Loc.T($"A perfect read: everything in the chest sells for {Pct(LedgerRight - 1)} more. A wrong slip: {Pct(1 - LedgerWrong)} less."),
            R, "ledger", 2, T(Appraisal)),
        new("truthglass", Loc.T("Truthglass"), Loc.T("A perfect read: everything in the chest sells for half as much again."), L, "truthglass", 3, T(Appraisal)),

        // False bottoms: drop it, hear the boom, find what was hidden. Nobody prices what's under the floor.
        new("crate", Loc.T("Padded Crate"), Loc.T("Glass never breaks when you drop a chest."), C, "crate", 2, T(Compartments, Rough)),
        new("hook", Loc.T("Smuggler's Hook"), Loc.T("Smugglers come your way: false bottoms turn up twice as often."), U, "hook", 2, T(Compartments)),
        new("rod", Loc.T("Hollow Rod"), Loc.T("What's under a false bottom comes out a step better: people hid their best."), U, "rod", 2, T(Compartments)),
        new("log", Loc.T("Smuggler's Log"), Loc.T($"Smugglers trust a buyer who keeps quiet: each false bottom you open this run makes false bottoms {Pct(LogStep)} likelier (up to {Pct(LogMax)})."),
            U, "log", 2, T(Compartments)),
        new("pass", Loc.T("Black Market Pass"), Loc.T("Thornwick's black market pays double for anything from under a false bottom."), R, "pass", 2, T(Compartments)),
        new("compass", Loc.T("Smugglers' Compass"), Loc.T("Every chest has a false bottom."), L, "compass", 3, T(Compartments)),

        // Wards: read the sign, name what never sounds, and the ward gives it up.
        new("wardlamp", Loc.T("Warden's Lantern"), Loc.T("Every sign a seller leaves (a dream, a tag, a riddle, a card...) points at a warded thing."),
            C, "wardlamp", 2, T(Wards)),
        new("pillow", Loc.T("Sleeper's Pillow"), Loc.T($"Sellers dream true for you: {Pct(PillowShare)} of them have warded something, and tell you their dream."), U, "pillow", 2,
            T(Wards)),
        new("wardtally", Loc.T("Wardbreaker's Tally"), Loc.T($"Each warded thing you name this run makes warded things sell for {Pct(WardTallyStep)} more."), U, "wardtally", 2, T(Wards)),
        new("wardkey", Loc.T("Ward-Key"), Loc.T("A warded thing you name comes out a step better."), R, "wardkey", 2, T(Wards, Appraisal)),
        new("aldricseal", Loc.T("The Aldric Seal"), Loc.T("Every chest has a warded thing, and its seller tells you a sign for it."), L, "aldricseal", 3, T(Wards)),

        // Luck: what comes out better than its chest. Sellers hear of a lucky shop; the art is in which chest gets the luck.
        new("shell", Loc.T("Lucky Shell"), Loc.T($"A dry spell ends twice as soon: luck starts to turn after {ShellPityAfter} plain finds, not {GameState.PityAfter}."), C, "shell", 1,
            T(Luck)),
        new("coin", Loc.T("Lucky Coin"), Loc.T("The first chest you buy each day: each thing in it rolls its quality twice and keeps the better roll."), U, "coin", 1, T(Luck)),
        new("foot", Loc.T("Rabbit's Foot"), Loc.T($"Everything you open is {Pct(FootLuck)} likelier to come out a step better than its chest."), U, "foot", 1, T(Luck)),
        new("wishbone", Loc.T("Wishbone"), Loc.T("Something that comes out better than its chest sells for half as much again."), U, "wishbone", 1, T(Luck)),
        new("horseshoe", Loc.T("Horseshoe"), Loc.T($"Each chest in a row with a surprise makes the next one's things {Pct(HorseshoeStep)} likelier to come out a step better (up to {Pct(HorseshoeMax)})."),
            U, "horseshoe", 2, T(Luck)),
        new("clover", Loc.T("Four-Leaf Clover"), Loc.T("The first chest you buy each week: everything in it comes out a step better."), R, "clover", 2, T(Luck)),
        new("star", Loc.T("Shooting Star"), Loc.T("Once a day, the first thing to come out better than its chest climbs a step further."), R, "star", 2, T(Luck)),
        new("spindle", Loc.T("Fate's Spindle"), Loc.T($"Each surprise this run makes everything {Pct(SpindleStep)} likelier to come out a step better (up to {Pct(SpindleMax)})."),
            L, "spindle", 3, T(Luck)),

        // Crowds: who comes in.
        new("lantern", Loc.T("Shop Lantern"), Loc.T("Open late: one more customer every day."), U, "lantern", 1, T(Crowds)),
        new("queue", Loc.T("Queue Ticket"), Loc.T($"The first chest you pass each day comes back at the end of the day, asking {Pct(1 - QueuePrice)} less."), U, "queue", 1,
            T(Crowds, Haggling)),
        new("horn", Loc.T("Barker's Horn"), Loc.T($"Two more customers every day, but word's out: everyone asks {Pct(HornDearer - 1)} more."), U, "horn", 2, T(Crowds)),
        new("guestbook", Loc.T("Guest Book"), Loc.T("The day's first customer is the better sort: their chest is a colour finer."), U, "guestbook", 2, T(Crowds, Luck)),

        // Display: what you keep.
        new("catalogue", Loc.T("Collector's Catalogue"), Loc.T($"Every new kind of find this run pays {CataloguePerTier}g (more in a grander shop)."), C, "catalogue", 1, T(Display)),
        new("dome", Loc.T("Bell Jar"), Loc.T("Room to display one more thing."), C, "dome", 2, T(Display)),
        new("book", Loc.T("Visitors' Book"), Loc.T($"Every night, visitors leave {BookPerPoint}g per exhibition point (more in a grander shop)."), U, "book", 2,
            T(Display, Coffers)),
        new("rope", Loc.T("Velvet Rope"), Loc.T("Your exhibition counts double."), U, "rope", 2, T(Display)),
        new("lamps", Loc.T("Gallery Lamps"), Loc.T($"Everything sells for {Fine(LampsPerPoint)} more per exhibition point (up to {Pct(LampsPerPoint * GameState.ExhibitionCap)})."),
            R, "lamps", 2, T(Display)),

        // The trades: one kind of thing. What it adds every seller hears of, so it pays in the chests full of that kind.
        new("gauntlet", Loc.T("Smith's Glove"), Loc.T($"You mend weapons: never worse than their chest promised, and {Pct(TradeLuck)} likelier to come out better."),
            C, "gauntlet", 1, T(Trade, Luck), Category.Arms),
        new("cap", Loc.T("Hatter's Cap"), Loc.T($"You mend cloaks: never worse than their chest promised, and {Pct(TradeLuck)} likelier to come out better."),
            C, "cap", 1, T(Trade, Luck), Category.Garb),
        new("whetstone", Loc.T("Whetstone"), Loc.T("Weapons sell for three-quarters more."), U, "whetstone", 1, T(Trade), Category.Arms),
        new("dummy", Loc.T("Tailor's Dummy"), Loc.T($"Cloaks sell for half as much again, and count {DummyPrestige} times over on display."), U, "dummy", 1, T(Trade, Display), Category.Garb),
        new("scales", Loc.T("Moneychanger's Scales"), Loc.T("Coin sells for a fifth more for every purse of it in the chest."), U, "scales", 1, T(Trade), Category.Coins),
        new("still", Loc.T("Alchemist's Still"), Loc.T("Potions sell for half as much again, and broken glass still sells for half."), U, "still", 2, T(Trade, Rough), Category.Potions),
        new("desk", Loc.T("Scribe's Desk"), Loc.T("Spells sell for half as much again, and sand sells as ink, for a third of a spell."), U, "desk", 2, T(Trade, Fakes), Category.Scrolls),
        new("wheel", Loc.T("Gem Cutter's Wheel"), Loc.T($"Gems are {Pct(WheelLuck)} likelier to come out a step better."), U, "wheel", 3, T(Trade, Luck), Category.Gems),

        // Rough hands: locks and breakage.
        new("oil", Loc.T("Locksmith's Oil"), Loc.T("Every lock has one pin fewer."), C, "oil", 2, T(Rough)),
        new("knife", Loc.T("Burglar's Knife"), Loc.T("Forcing a lock breaks nothing and scratches nothing."), U, "knife", 2, T(Rough, Blind)),
        new("gloves", Loc.T("Kid Gloves"), Loc.T("Nobody minds a hard shake, a drop or the lodestone on their chest."), U, "gloves", 2, T(Rough, Appraisal)),

        // The fence: fakes.
        new("loupe", Loc.T("Jeweller's Loupe"), Loc.T("Paste gems and demon gold sell as costume pieces: a quarter of the real thing."), C, "loupe", 4, T(Fakes)),
        new("mark", Loc.T("Fence's Mark"), Loc.T("Crooks come twice as often, and the Maw's buyers pay for any fake what the real thing fetches."), R, "mark", 3,
            T(Fakes, Appraisal)),

        // Coffers: the purse, and bad nights.
        new("purse", Loc.T("Rainy-Day Purse"), Loc.T("Bram's stash helps you out three times a run, not twice."), C, "purse", 1, T(Coffers)),
        new("ticket", Loc.T("Pawn Ticket"), Loc.T("On a short night, what you pawn fetches four-fifths of its price, not three-fifths."), C, "ticket", 1, T(Coffers)),
        new("marker", Loc.T("Guildsman's Marker"), Loc.T("Bram's word is good twice a run, not once."), C, "marker", 1, T(Coffers)),
        new("jar", Loc.T("Savings Jar"), Loc.T($"Every night: {Pct(InterestRate)} of your gold as interest (up to {InterestPerTier}g at the stall, more in a grander shop)."), U, "jar", 1,
            T(Coffers)),
        new("lockbox", Loc.T("Miser's Lockbox"), Loc.T($"Every Guild night, once you've paid, your purse grows by {Pct(LockboxShare)} of what's left in it."), R, "lockbox", 2, T(Coffers)),
        // Mara's favour (a talent until 0.22): the Guild honours its own paper, once.
        new("note", Loc.T("Guild Promissory Note"), Loc.T("The Guild's own paper: once a run, on a Guild night you can't cover, it pays what's short (not the last week)."), R, "note", 1, T(Coffers)),

        // Patrons: commissions.
        new("letter", Loc.T("Letter of Introduction"), Loc.T("Regulars take what you deliver a quality lower than they asked for."), U, "letter", 1, T(Patrons)),
        new("ribbon", Loc.T("Patron's Ribbon"), Loc.T("Commissions pay half as much again."), U, "ribbon", 1, T(Patrons)),
        new("errand", Loc.T("Errand Book"), Loc.T($"The regulars post {ErrandCommissions} more commissions every week."), U, "errand", 2, T(Patrons)),

        new("net", Loc.T("Songcatcher's Net"), Loc.T("Heartsongs are three times as likely to be in a chest."), R, "net", 1, T(CurioTag.Song)),
    };

    static string Pct(float fraction) => Synergies.Pct(fraction);

    /// <summary>"1.5%": a share too small for whole percents (the same in every locale).</summary>
    static string Fine(float fraction) => (MathF.Round(fraction * 1000) / 10).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "%";

    /// <summary>
    /// What Nim keeps back until later runs: the second layer of each way of trading after a first run, the capital's and
    /// the Emporium's curios once you've had a shop there (or after two and four runs, so every early run brings something
    /// new), the Shooting Star once the debt's been cleared, the net once a Heartsong's been heard, and each legendary
    /// once Bram's kin have done its trade often enough (<see cref="Feats"/>). Everything else he brings from the start.
    /// </summary>
    static readonly Dictionary<string, CurioUnlock> Unlocks = new()
    {
        ["wishbone"] = CurioUnlock.SecondRun, ["horseshoe"] = CurioUnlock.SecondRun, ["token"] = CurioUnlock.SecondRun,
        ["crescent"] = CurioUnlock.SecondRun, ["tongue"] = CurioUnlock.SecondRun, ["guestbook"] = CurioUnlock.SecondRun,
        ["dummy"] = CurioUnlock.SecondRun, ["scales"] = CurioUnlock.SecondRun, ["rod"] = CurioUnlock.SecondRun, ["pillow"] = CurioUnlock.SecondRun,
        ["dip"] = CurioUnlock.SecondRun, ["bond"] = CurioUnlock.SecondRun, ["knot"] = CurioUnlock.SecondRun, ["register"] = CurioUnlock.SecondRun,
        ["log"] = CurioUnlock.SecondRun, ["wardtally"] = CurioUnlock.SecondRun,
        ["monocle"] = CurioUnlock.Crownhold, ["still"] = CurioUnlock.Crownhold, ["desk"] = CurioUnlock.Crownhold, ["wheel"] = CurioUnlock.Crownhold,
        ["aether"] = CurioUnlock.Crownhold, ["horn"] = CurioUnlock.Crownhold, ["rope"] = CurioUnlock.Crownhold, ["errand"] = CurioUnlock.Crownhold,
        ["chalk"] = CurioUnlock.Crownhold, ["wholesale"] = CurioUnlock.Crownhold,
        ["fork"] = CurioUnlock.Emporium, ["ledger"] = CurioUnlock.Emporium, ["coffer"] = CurioUnlock.Emporium, ["lamps"] = CurioUnlock.Emporium,
        ["ring"] = CurioUnlock.Emporium, ["wardkey"] = CurioUnlock.Emporium, ["mark"] = CurioUnlock.Emporium, ["lockbox"] = CurioUnlock.Emporium,
        ["note"] = CurioUnlock.Crownhold,
        ["pass"] = CurioUnlock.Emporium,
        ["star"] = CurioUnlock.Cleared, ["net"] = CurioUnlock.Heartsong,
        ["spindle"] = CurioUnlock.Feat, ["truthglass"] = CurioUnlock.Feat, ["goldentongue"] = CurioUnlock.Feat, ["compass"] = CurioUnlock.Feat,
        ["aldricseal"] = CurioUnlock.Feat, ["staff"] = CurioUnlock.Feat, ["blindfold"] = CurioUnlock.Feat,
    };

    /// <summary>
    /// What opens each legendary curio: its trade, done often enough across runs. A careful reader opens the truthglass in
    /// a run or two; nobody opens the blindfold without trying to buy sight unseen.
    /// </summary>
    public static readonly Dictionary<string, (Feat Feat, int Needed)> Feats = new()
    {
        ["spindle"] = (Feat.Surprises, 40), ["truthglass"] = (Feat.PerfectReads, 40), ["goldentongue"] = (Feat.Bargains, 25),
        ["compass"] = (Feat.FalseBottoms, 15), ["aldricseal"] = (Feat.Wards, 10), ["staff"] = (Feat.Enchanted, 15), ["blindfold"] = (Feat.SightUnseen, 20),
    };

    /// <summary>A feat, as the curio page says it: "chests taken sight unseen".</summary>
    public static string FeatName(Feat feat) => feat switch
    {
        Feat.Surprises => Loc.T("surprises (things better than their chest)"),
        Feat.PerfectReads => Loc.T("perfect reads"),
        Feat.Bargains => Loc.T("chests talked down"),
        Feat.FalseBottoms => Loc.T("false bottoms opened"),
        Feat.Wards => Loc.T("warded things named"),
        Feat.Enchanted => Loc.T("enchanted things found"),
        _ => Loc.T("chests taken sight unseen"),
    };

    /// <summary>A run's progress on a feat (banked into the profile when the run ends).</summary>
    public static int Progress(GameState state, Feat feat) => feat switch
    {
        Feat.Surprises => state.Surprises,
        Feat.PerfectReads => state.PerfectReads,
        Feat.Bargains => state.Bargains,
        Feat.FalseBottoms => state.BottomsOpened,
        Feat.Wards => state.WardsNamed,
        Feat.Enchanted => state.MagicFound,
        _ => state.SightUnseen,
    };

    public static CurioUnlock UnlockOf(string key) => Unlocks.GetValueOrDefault(key, CurioUnlock.Start);

    /// <summary>What opens a curio, as the ledger and the run's end say it: "after your first run".</summary>
    public static string UnlockText(CurioDef curio) => curio.Unlock switch
    {
        CurioUnlock.SecondRun => Loc.T("after your first run"),
        CurioUnlock.Crownhold => Loc.T("once you've had a shop in Crownhold, or after two runs"),
        CurioUnlock.Emporium => Loc.T("once you've had the Royal Emporium, or after four runs"),
        CurioUnlock.Cleared => Loc.T("once you've cleared Bram's debt"),
        CurioUnlock.Heartsong => Loc.T("once you've heard a Heartsong"),
        CurioUnlock.Feat when Feats.TryGetValue(curio.Key, out var feat) => Loc.T($"after {feat.Needed} {FeatName(feat.Feat)}, all told"),
        _ => Loc.T("from the start"),
    };

    public static CurioDef Get(string key) => All.First(c => c.Key == key);

    public static bool Exists(string key) => All.Any(c => c.Key == key);

    /// <summary>Room for this many curios, more with a curio case.</summary>
    public const int Slots = 3;

    /// <summary>How many curios Nim lays out a night, before his upgrades (0.28: every night, one; 0.22-0.27: four, twice a week).</summary>
    public const int Laid = 1;

    /// <summary>Nim's first week (his first night always has something cheap).</summary>
    public const int FirstWeek = 1;

    /// <summary>
    /// The first night Nim comes by: the third, as 0.27's first visit was the third morning (the experience playtest:
    /// Nim on the first night as well was too much for the guided first day).
    /// </summary>
    public const int FirstNight = 3;

    /// <summary>
    /// A curio's price: a fixed sum by how rare it is (<see cref="Base"/>), and a share of what's in your purse when Nim
    /// arrives (<see cref="Share"/>). The fixed part keeps a curio dear at the stall, where a share of a few coins is
    /// nothing; the share keeps it a choice at the Emporium, where any fixed sum would be pocket change.
    /// </summary>
    public static int Base(CurioRarity rarity) => rarity switch
    {
        CurioRarity.Common => 10,
        CurioRarity.Uncommon => 20,
        CurioRarity.Rare => 50,
        _ => 150,
    };

    /// <remarks>
    /// 0.28: a sixth less than 0.27's 6/10/15/20%, now that Nim comes every night: more nights to buy on, so each
    /// purchase takes a smaller share.
    /// </remarks>
    public static float Share(CurioRarity rarity) => rarity switch
    {
        CurioRarity.Common => 0.05f,
        CurioRarity.Uncommon => 0.085f,
        CurioRarity.Rare => 0.125f,
        _ => 0.17f,
    };

    /// <summary>What Nim asks for a curio, from what was in your purse when he arrived.</summary>
    public static int Price(CurioDef curio, int purse) => Base(curio.Rarity) + Money.Round(Math.Max(0, purse) * Share(curio.Rarity));

    /// <summary>How much likelier Nim is to bring a curio that shares a tag with one you keep (he knows a collector).</summary>
    const float Kindred = 3f;

    /// <summary>
    /// The ways of trading you're building: those you keep two or more curios of. Nim brings a legendary curio only for one
    /// of them (<see cref="Stock"/>).
    /// </summary>
    public static HashSet<CurioTag> Building(GameState state) =>
        state.OwnedCurios.SelectMany(c => c.Tags).GroupBy(t => t).Where(g => g.Count() >= 2).Select(g => g.Key).ToHashSet();

    /// <summary>
    /// The way of trading Nim thinks you're after: what the curio you got most recently is for (a trade's and the
    /// Heartsongs' aside). He lays out something for it on every visit, so a build comes together, and turning to another
    /// is a matter of buying one of its curios.
    /// </summary>
    public static CurioTag? Leaning(GameState state) =>
        state.OwnedCurios.Reverse().Select(c => c.Tags[0]).Cast<CurioTag?>().FirstOrDefault(t => t is not (CurioTag.Trade or CurioTag.Song));

    /// <summary>
    /// How likely each card Nim lays out is to be of each rarity, week by week: nothing rare in the first week, nothing
    /// legendary before the third, and never much of either (0.20's rares, a quarter of the cards by the sixth week and
    /// likelier still beside a curio they went with, made every run's build out of them).
    /// </summary>
    public static float[] Rates(int week) => week switch
    {
        <= 1 => new[] { 0.70f, 0.30f, 0f, 0f },
        2 => new[] { 0.55f, 0.40f, 0.05f, 0f },
        3 => new[] { 0.45f, 0.43f, 0.10f, 0.02f },
        4 => new[] { 0.40f, 0.43f, 0.13f, 0.04f },
        _ => new[] { 0.35f, 0.44f, 0.15f, 0.06f },
    };

    /// <summary>The shop tier a curio of this rarity needs before Nim lays one out: rare ones from Thornwick, legendary ones from Crownhold.</summary>
    public static int RarityTier(CurioRarity rarity) => rarity switch
    {
        CurioRarity.Rare => 2,
        CurioRarity.Legendary => 3,
        _ => 1,
    };

    /// <summary>Whether Nim could lay this curio out at this shop, this week (its own tier, its rarity's tier and week).</summary>
    public static bool Reachable(GameState state, CurioDef curio) =>
        curio.MinTier <= state.Tier && RarityTier(curio.Rarity) <= state.Tier && Rates(state.Week)[(int)curio.Rarity] > 0;

    /// <summary>
    /// What Nim lays out on a visit: three curios (four for a familiar face) you don't own and could use. Each card's
    /// rarity is rolled first (<see cref="Rates"/>, among the rarities he has something of), then which curio of it: ones
    /// that go with what you keep four times as often. Always one for the way of trading you lean towards
    /// (<see cref="Leaning"/>, two once you're building it), and on his first visit to a grander shop one for a way of trading that opens there (the
    /// first chance to turn to it); a legendary one only for a way of trading you're building (two of its curios kept:
    /// <see cref="Building"/>). Leaves out what he's just shown you.
    /// </summary>
    public static List<CurioDef> Stock(Random rng, GameState state, int count = 3, ICollection<string> shown = null)
    {
        var building = Building(state);
        var pool = All.Where(c => Reachable(state, c) && state.CurioOpen(c.Key) && !state.HasCurio(c.Key) && !state.NoUse(c) &&
                                  (c.Rarity != CurioRarity.Legendary || building.Contains(c.Tags[0])) && (shown == null || !shown.Contains(c.Key))).ToList();
        var stock = new List<CurioDef>();
        var owned = state.OwnedCurios.SelectMany(c => c.Tags).ToHashSet();
        var rates = Rates(state.Week);
        while (stock.Count < count && pool.Count > 0)
        {
            var rarities = Enum.GetValues<CurioRarity>().Where(r => rates[(int)r] > 0 && pool.Any(c => c.Rarity == r)).ToArray();
            if (rarities.Length == 0) break;
            var rarity = rng.PickWeighted(rarities, r => rates[(int)r]);
            var pick = rng.PickWeighted(pool.Where(c => c.Rarity == rarity).ToArray(), c => c.Tags.Any(owned.Contains) ? 1 + Kindred : 1);
            stock.Add(pick);
            pool.Remove(pick);
        }
        // Cards he'll always lay out, each in place of the last random one: something for the way of trading you lean
        // towards, and at a grander shop than he last saw, something for a way of trading that opens there.
        var fixedCards = new List<CurioDef>();
        void Lay(Func<CurioDef, bool> wanted)
        {
            if (stock.Any(c => wanted(c) && !fixedCards.Contains(c)))
            {
                fixedCards.Add(stock.First(c => wanted(c) && !fixedCards.Contains(c)));
                return;
            }
            var fits = pool.Where(wanted).ToArray();
            int spare = stock.FindLastIndex(c => !fixedCards.Contains(c));
            if (fits.Length == 0) return;
            var pick = rng.PickWeighted(fits, c => rates[(int)c.Rarity]);
            if (stock.Count < count) stock.Add(pick);
            else if (spare < 0) return;
            else
            {
                pool.Add(stock[spare]);
                stock[spare] = pick;
            }
            pool.Remove(pick);
            fixedCards.Add(pick);
        }
        // (With a single card, the night's is left to chance and the leaning odds: one for the leaning every night would be the same card.)
        if (count >= 2 && Leaning(state) is CurioTag leaning)
        {
            Lay(c => c.Has(leaning));
            // A collector who's building it gets a second.
            if (building.Contains(leaning) && count > 2) Lay(c => c.Has(leaning));
        }
        if (state.Tier > state.PeddlerTier && state.PeddlerTier > 0) Lay(c => c.MinTier == state.Tier && c.Tags.Any(Main.Contains));
        // Nim's first night always has something cheap (in place of a card he didn't mean to lay out).
        var commons = pool.Where(c => c.Rarity == CurioRarity.Common).ToArray();
        int loose = stock.FindLastIndex(c => !fixedCards.Contains(c));
        if (state.Day <= FirstNight && loose >= 0 && stock.All(c => c.Rarity != CurioRarity.Common) && commons.Length > 0)
            stock[loose] = rng.PickOne(commons);
        return stock;
    }

    public static string RarityName(CurioRarity rarity) => rarity switch
    {
        CurioRarity.Common => Loc.T("common"),
        CurioRarity.Uncommon => Loc.T("uncommon"),
        CurioRarity.Rare => Loc.T("rare"),
        _ => Loc.T("legendary"),
    };

    /// <summary>A tag as the cards show it: "luck", "sight unseen", "weapons".</summary>
    public static string TagName(CurioTag tag, CurioDef curio = null) => tag switch
    {
        CurioTag.Trade when curio?.Kind is Category kind => Categories.Get(kind).Name.ToLowerInvariant(),
        CurioTag.Trade => Loc.T("a trade"),
        CurioTag.Appraisal => Loc.T("the perfect read"),
        CurioTag.Haggling => Loc.T("haggling"),
        CurioTag.Crowds => Loc.T("crowds"),
        CurioTag.Display => Loc.T("display"),
        CurioTag.Compartments => Loc.T("false bottoms"),
        CurioTag.Wards => Loc.T("wards"),
        CurioTag.Enchantment => Loc.T("enchantment"),
        CurioTag.Blind => Loc.T("sight unseen"),
        CurioTag.Rough => Loc.T("rough hands"),
        CurioTag.Fakes => Loc.T("fakes"),
        CurioTag.Coffers => Loc.T("coffers"),
        CurioTag.Patrons => Loc.T("patrons"),
        CurioTag.Song => Loc.T("Heartsongs"),
        _ => Loc.T("luck"),
    };

    /// <summary>The six builds that change how you play, for the cards and the builds report.</summary>
    public static readonly CurioTag[] Main =
        { CurioTag.Enchantment, CurioTag.Blind, CurioTag.Haggling, CurioTag.Appraisal, CurioTag.Compartments, CurioTag.Wards };

    /// <summary>"luck, the perfect read": the tags a card shows.</summary>
    public static string Tags(CurioDef curio) => Words.List(curio.Tags.Select(t => TagName(t, curio)));

    /// <summary>What the opening says a curio (or a Heartsong on display) did to a chest: "Lucky Coin: 2 things came out better!"</summary>
    public static string LuckLine(string key, int things)
    {
        string n = Loc.Plural(things, $"{things} thing", $"{things} things");
        string name = Exists(key) ? Get(key).Name : Heartsongs.Exists(key) ? Loc.T(Heartsongs.Get(key).Name) : key == "restorer" ? Loc.T("Your restorers") : key;
        return key switch
        {
            "token" => Loc.T($"{name}: {n} swung!"),
            "gauntlet" or "cap" or "blindfold" => Loc.T($"{name}: {n} mended."),
            "crescent" => Loc.T($"{name}: {n} held steady."),
            Heartsongs.Ember => Loc.T($"{name}: {n} tempered!"),
            _ => Loc.T($"{name}: {n} came out better!"),
        };
    }
}

/// <summary>The peddler who sells curios.</summary>
public static class Peddler
{
    public static string Name => Loc.T("Nim the Peddler");

    // Nim talks about Nim the way other people talk about the weather: cheerfully, and as if it couldn't be helped.
    public static readonly string[] Greetings =
    {
        Loc.T("Psst! Keywright! Shutters down? Good. Nim's pack is full of things that came out of chests and never went back in."),
        Loc.T("Evening! Nim kept the good ones for you. Well. Nim kept some ones for you."),
        Loc.T("Quick, before the Guild's night watch comes round the corner. Look, look, but don't point."),
        Loc.T("Out of a wizard's strongroom, these, this very night. The wizard won't miss them. Nim checked."),
        Loc.T("A good day? Nim heard. Nim hears everything after dark."),
    };

    public static readonly string[] Thanks =
    {
        Loc.T("Good eye. Nim would have kept that one, if Nim were the keeping sort."),
        Loc.T("Done! Treat it kindly. It's older than the Guild."),
        Loc.T("It likes you. Nim can always tell when they like someone."),
    };

    public static readonly string[] Bye = { Loc.T("Nim's off. Tomorrow night, same back door."), Loc.T("Mind the Guild, Keywright. Nim always does."),
        Loc.T("Sleep well. Nim never does.") };

    /// <summary>When you buy one of his upgrades: more on his table every night.</summary>
    public static string MoreCards => Loc.T("Oh, Nim likes it here. Nim will bring more. Every night, more!");

    /// <summary>When a back-room chest goes home, or to his friends.</summary>
    public static string Returned => Loc.T("Back to its owner? How honest. Nim won't tell his friends.");
    public static string Fenced => Loc.T("Nim's friends will see it gets back to whoever wants it most. For a price. Always a price.");

    /// <summary>When a curio would leave you short of Bram's working purse.</summary>
    public static string Stretch => Loc.T("That would leave your purse thin, that would. Still want it?");

    /// <summary>When you ask to see something else.</summary>
    public static string More => Loc.T("Not to your taste? Nim has more. Nim always has more.");

    /// <summary>When you sell one back.</summary>
    public static string Back => Loc.T("Back in the pack it goes. Half what you paid: Nim's fair, ask anyone.");
}
