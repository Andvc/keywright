using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// One of Bram's kin, who comes to mind the stall. Each reads chests or runs the shop their own way: one rule of the
/// trade that's different for them, and what it costs them. The rest of the game is the same for everyone.
/// </summary>
/// <param name="Title">What they're known for, in two or three words.</param>
/// <param name="Blurb">Who they are, in a sentence.</param>
/// <param name="Rules">What's different when you play them, one line each.</param>
/// <param name="Unlock">What unlocks them, for the character screen (null: from the start).</param>
public sealed record CharacterDef(string Key, string Name, string Title, string Blurb, string[] Rules, string Unlock, Func<Profile, bool> Unlocked);

public static class Characters
{
    public const string Kit = "kit", Hesk = "hesk", Rosa = "rosa", Isolde = "isolde";

    public static readonly CharacterDef[] All =
    {
        new(Kit, "Kit", "The Ear", "Bram's niece, who grew up behind his counter, with the Key-mark and his ear for a chest.",
            new[]
            {
                "Shakes at every strength and drops a chest to hear what's under the floor.",
                "With tips on, Uncle Bram teaches the trade as each new seller arrives.",
            }, null, _ => true),
        new(Hesk, "Hesk", "The Scale", "Bram's nephew, a grocer from the river towns, with the Key-mark, a bad back and an honest scale.",
            new[]
            {
                "A bad back: he can't shake a chest hard, or lift it to drop it.",
                "Every chest sits on his scale: he knows what everything in it weighs, to the half pound.",
                $"Coin {Pounds(Category.Coins)} lb, weapons {Pounds(Category.Arms)}, cloaks {Pounds(Category.Garb)}, potions {Pounds(Category.Potions)}, " +
                $"spells {Pounds(Category.Scrolls)}, gems {Pounds(Category.Gems)}. Demon gold weighs half what gold does; sand is heavier than a scroll.",
            }, "Pay the Guild's third due to unlock.", p => p.BestWeek >= 4),
        new(Rosa, "Rosa", "The Tongue", "Bram's niece from the Crownhold markets. She'd haggle with a statue, and win.",
            new[]
            {
                "Everybody haggles with Rosa: sellers who'd name a fixed price ask high and hear an offer instead.",
                "They know her: they open a little higher and come down a little less. An offer too low gets a second try.",
                "But word gets round: a seller who walks out on her won't come back that week.",
            }, "Clear Bram's debt to unlock.", p => p.Cleared >= 1),
        new(Isolde, "Isolde", "The Collection", "Bram's niece by marriage, a lady who has never sold a thing she liked.",
            new[]
            {
                $"A shelf from the start: she can display {IsoldeShelf} things at Bram's stall.",
                $"Visitors pay to see her collection: every night, {AdmissionPerPoint}g per point of exhibition (more in a grander shop).",
                "Her collection is not for pawning: on a short night, the Guild can only take her curios.",
            }, "Open the Crown Chest to unlock.", p => p.Vaults >= 1),
    };

    public static CharacterDef Get(string key) => All.FirstOrDefault(c => c.Key == key) ?? All[0];

    public static bool Exists(string key) => All.Any(c => c.Key == key);

    /// <summary>Isolde's shelf at the stall: room for this many things.</summary>
    public const int IsoldeShelf = 2;

    /// <summary>
    /// What Isolde's visitors pay, per exhibition point, every night at the stall; more in a grander shop, but only a
    /// quarter as fast as the shop grows (<see cref="IsoldeScale"/>). Door money that grew as fast as the shop made her the
    /// strongest of the kin by far (v18's sim: 62% of runs cleared with no talents, against Kit's 42%; half as fast, 54%);
    /// flat, the weakest (26%).
    /// </summary>
    public const int AdmissionPerPoint = 1;

    /// <summary>A quarter of <see cref="ShopTiers.Scale"/>: x1, x1, x2, x5.</summary>
    public static int IsoldeScale(int tier) => Math.Max(1, ShopTiers.Scale(tier) / 4);

    /// <summary>What everything weighs, for the ledger and a scale's tooltip.</summary>
    public static string PoundsTable =>
        $"Coin {Pounds(Category.Coins)} lb, weapons {Pounds(Category.Arms)}, cloaks {Pounds(Category.Garb)}, potions {Pounds(Category.Potions)}, " +
        $"spells {Pounds(Category.Scrolls)}, gems {Pounds(Category.Gems)}, a Heartsong {Pounds(Category.Heartsong)}.";

    /// <summary>What one thing of this kind weighs on Hesk's scale, in pounds.</summary>
    public static float Pounds(Category kind) => kind switch
    {
        Category.Arms => 6,
        Category.Coins => 4,
        Category.Garb => 3,
        Category.Potions => 3,
        _ => 1,
    };

    /// <summary>
    /// What a fake weighs: gilded iron half what the gold it apes does (gold is heavy, iron isn't), a bag of sand three
    /// times a bundle of papers.
    /// </summary>
    public static float FakePounds(Category kind) => kind == Category.Scrolls ? 3 : Pounds(kind) / 2;

    public static float Pounds(Item item) => item.Fake ? FakePounds(item.Category) : Pounds(item.Category);

    /// <summary>"13.5 lb", "4 lb".</summary>
    public static string Weight(float pounds) => $"{(pounds % 1 == 0 ? $"{pounds:0}" : $"{pounds:0.0}")} lb";
}

/// <summary>
/// One of a challenge level's terms: a bane the season brings against you, or a boon that eases one of the trade's
/// frustrations. A run at level N draws N of each (<see cref="Challenges.Draw"/>), on top of dues a step higher a level.
/// </summary>
/// <param name="Good">A boon, not a bane.</param>
/// <param name="Name">What it's called, in two to four words.</param>
/// <param name="Rule">What it does, in a sentence.</param>
/// <param name="Why">Why, in Aldmark: the reason you'd give a stranger in a tavern (docs/world.md).</param>
/// <param name="Clashes">A term of the other kind it can't be drawn with: they'd contradict each other.</param>
public sealed record TermDef(string Key, bool Good, string Name, string Rule, string Why, string Clashes = null);

/// <summary>
/// Challenge levels (opened per character by clearing Bram's debt at the level below, each a quarter more to the score).
/// At level N the Guild's dues are higher by N steps, and the run draws N banes and N boons: the season is harder, but
/// something always goes your way. The banes make the trade harder; the boons smooth over what's most frustrating about
/// it (a slow start, a short Guild night, a sweeping clock, a lock that slips, a curio you can't find).
/// </summary>
public static class Challenges
{
    public const int Max = 8;

    /// <summary>Each level puts every due up by this share of what it was at level 0 (settable, for the sim's tuning).</summary>
    public static float DebtStep { get; set; } = 0.1f;

    /// <summary>What a level does to the Guild's dues.</summary>
    public static float DebtFactor(int level) => 1 + DebtStep * Math.Clamp(level, 0, Max);

    // The banes.
    public const string ThinCrowds = "thin", SharpSellers = "sharp", Forgers = "forgers", Hurried = "hurried", FussySellers = "fussy",
        Stretched = "stretched", LeanStart = "lean", DearCurios = "dearcurios", Builders = "builders", Rust = "rust", Wary = "wary", Sparse = "sparse";

    // The boons.
    public const string Gifts = "gifts", NimsFavour = "nim", Savings = "savings", OldPicks = "oldpicks", Invited = "invited", Patient = "patient",
        Prepared = "prepared", HonestClerk = "pawn", GoodName = "goodname", Charm = "charm", FairDays = "fairdays", Drawer = "drawer";

    /// <summary>With thin crowds: how many of the week's last days (the Guild's day, and the day its collectors ride in) are a customer short.</summary>
    public const int ThinDays = 2;

    /// <summary>With sharp sellers: what the least a haggler (or a boy waiting for your offer) will take is multiplied by.</summary>
    public const float SharpFloor = 1.15f;

    /// <summary>With demon gold about: the chance any coin from any seller is gilded iron.</summary>
    public const float SlugChance = 0.1f;

    /// <summary>In a hurried season: seconds each seller gives you.</summary>
    public const float HurriedPatience = 20f;

    /// <summary>What Nim asks more, when he's raised his prices; and what the Guild's builders charge more.</summary>
    public const float DearCurioPrices = 1.25f, BuilderPrices = 1.25f;

    /// <summary>A slow season: impatient sellers give you this much longer, and a rotten chest stands this much more.</summary>
    public const float PatientFactor = 2f;

    /// <summary>
    /// With welcome gifts: how many of the first day's customers hand you their chest, and what for (a coin for luck: a
    /// chest opens only once it's bought or given).
    /// </summary>
    public const int Gifted = 2, GiftPrice = 1;

    /// <summary>With Bram's savings: what the starting purse is multiplied by.</summary>
    public const float SavingsFactor = 1.5f;

    public static readonly TermDef[] Banes =
    {
        new(ThinCrowds, false, "Thin crowds", "On the last two days of every week, one customer fewer.",
            "The Guild's collectors ride in the day before the tithe, and nobody wants to be seen at your door while they're about."),
        new(SharpSellers, false, "Sharp sellers", $"Sellers who haggle hold out for more: the least they'll take is {Synergies.Pct(SharpFloor - 1)} higher.",
            "Word's out that the Guild has you by the purse, and sellers think you'll pay what they ask."),
        new(Forgers, false, "Demon gold about", "One coin in ten is demon gold, whoever brings it.",
            "The Gate's cracking wider, and the Maw's gilded iron has spread all over the realm."),
        new(Hurried, false, "A hurried season", $"Every seller gives you {HurriedPatience:0} seconds to decide.",
            "It's harvest time, and nobody can spare more than a moment at your counter."),
        new(FussySellers, false, "Fussy sellers", "Every seller minds a drop: drop their chest and they ask half again.",
            "Old chests are the fashion this year, and everyone's precious about theirs."),
        new(Stretched, false, "Bram's stretched thin", "Bram's stash has one dip fewer, and his word is good one time fewer.",
            "Bram spent his savings mending the roof, and the Guild has stopped taking his word."),
        new(LeanStart, false, "A lean start", "You start with half the gold.",
            "The stall's licence cost more than Bram thought."),
        new(DearCurios, false, "Nim's raised his prices", "Nim asks a quarter more for every curio.",
            "The Guild is hunting Nim this season, and he charges for the risk."),
        new(Builders, false, "The Guild's builders", "Every upgrade and move costs a quarter more.",
            "The Guild says only its own builders may work on a licensed shop, at its own rates."),
        new(Rust, false, "A wet year", "Every lock has one more pin to pick.",
            "A wet winter has rusted every old lock in the realm stiff."),
        new(Wary, false, "Wary regulars", "The regulars post one commission fewer each week.",
            "Folk aren't sure your stall will last, and won't set you errands."),
        new(Sparse, false, "Nim's lean month", "Nim lays out three curios, not four.",
            "Pickings have been slim among the Locking's oddments."),
    };

    public static readonly TermDef[] Boons =
    {
        new(Gifts, true, "Welcome gifts", $"Your first {Sellers.Number(Gifted)} customers hand you their chests for a coin each.",
            "The crossroads loved Bram, and on your first morning two of his old regulars give you their chests for a lucky penny."),
        new(NimsFavour, true, "Nim's soft spot", "Nim will show you something else twice a visit, not once.",
            "Nim owes Bram a favour, and has decided you're it."),
        new(Savings, true, "Bram's savings", "You start with half as much gold again.",
            "Bram sold his old cart, and the money's yours.", LeanStart),
        new(OldPicks, true, "Bram's old picks", "You start with the Fine Lockpicks: a wider sweet spot on every lock.",
            "Bram found his old picks at the bottom of a drawer."),
        new(Invited, true, "Thornwick wants you", "The move to Thornwick Market costs half.",
            "Thornwick's council wants a Keywright in the market, and pays half the rent.", Builders),
        new(Patient, true, "A slow season", "Sellers who won't wait give you twice as long, and a rotten chest stands twice the shaking.",
            "Nobody's in a hurry this year.", Hurried),
        new(Prepared, true, "Bram's prepared", "Bram's stash has one more dip in it, and his word is good once more.",
            "Bram's been putting a little by, and the Guild still owes him a favour.", Stretched),
        new(HonestClerk, true, "An honest clerk", "On a short night, what you pawn fetches its full price.",
            "The Guild's pawn clerk was at school with your mother."),
        new(GoodName, true, "A good name", "Hagglers hear you out: an offer that's too low gets a second try, not a slammed door.",
            "Bram's kin have a name for fair dealing."),
        new(Charm, true, "A lucky charm", "After a run of plain finds, your luck turns twice as soon.",
            "Bram tied his mother's old lucky charm to your apron."),
        new(FairDays, true, "Fair days", "No days of mourning and no Guild assessors in town.",
            "The old Duke's in good health, and the Guild's assessors have their hands full in the capital."),
        new(Drawer, true, "Room for more", "Room for one more curio, from the start.",
            "Bram cleared out a drawer for your curios."),
    };

    public static TermDef Get(string key) => Banes.FirstOrDefault(t => t.Key == key) ?? Boons.FirstOrDefault(t => t.Key == key);

    static bool Clash(string a, string b) => Get(a)?.Clashes == b || Get(b)?.Clashes == a;

    /// <summary>
    /// A run's terms at this level: so many banes and as many boons, drawn a pair at a time (a bane, then a boon) from one
    /// shuffle of each pool, so a level's draw is the level below's with one more of each, and no boon is ever drawn with
    /// the bane it would contradict (twelve of each and four such pairs leave room for eight of each).
    /// </summary>
    public static (List<string> Boons, List<string> Banes) Draw(int seed, int level)
    {
        var rng = new Random(unchecked(seed * 92821 + 17));
        var banes = Banes.OrderBy(_ => rng.Next()).Select(t => t.Key).ToList();
        var boons = Boons.OrderBy(_ => rng.Next()).Select(t => t.Key).ToList();
        var good = new List<string>();
        var bad = new List<string>();
        for (int i = 0; i < Math.Clamp(level, 0, Max); i++)
        {
            bad.Add(banes.First(b => !bad.Contains(b) && !good.Any(g => Clash(g, b))));
            good.Add(boons.First(g => !good.Contains(g) && !bad.Any(b => Clash(g, b))));
        }
        return (good, bad);
    }

    /// <summary>These terms, by key, as a run's boons and banes (for debugging a term's screens: --terms).</summary>
    public static (List<string> Boons, List<string> Banes) Named(IEnumerable<string> keys)
    {
        var terms = keys.Select(k => Get(k.Trim())).Where(t => t != null).Distinct().ToList();
        return (terms.Where(t => t.Good).Select(t => t.Key).ToList(), terms.Where(t => !t.Good).Select(t => t.Key).ToList());
    }

    /// <summary>What a level does to a run's score: a quarter more for each.</summary>
    public static float Multiplier(int level) => 1 + 0.25f * Math.Clamp(level, 0, Max);
}

/// <summary>
/// How a run went, in points: the Guild's dues paid, the debt cleared (and how early), the vault (sung whole, with all nine
/// Heartsongs known), the map, the shop, the collection, the Heartsongs found, and every week after the debt you kept the
/// Guild paid. Points times the challenge level's multiplier are the run's score (each of Bram's kin keeps their best).
/// </summary>
public static class Score
{
    public const int PerDue = 10, Cleared = 100, PerDaySpare = 5, Vault = 100, PerPiece = 10, PerRenovation = 25, PerFind = 2, PerEndlessWeek = 25,
        PerSong = 20, SungWhole = 200;

    /// <summary>The run's points, line by line (lines worth nothing are left out).</summary>
    public static List<(string Label, int Points)> Lines(GameState state)
    {
        var lines = new List<(string, int)>();
        void Add(string label, int points)
        {
            if (points > 0) lines.Add((label, points));
        }
        int dues = Math.Min(state.WeeksPaid, Guild.Weeks);
        Add($"Guild dues paid ({dues} of {Guild.Weeks})", dues * PerDue);
        if (state.DebtCleared)
        {
            Add("Bram's debt cleared", Cleared);
            int spare = Math.Max(0, Guild.Weeks * Guild.DaysPerWeek - state.ClearedDay);
            Add($"Days to spare ({spare})", spare * PerDaySpare);
        }
        if (state.VaultOpened) Add("The Crown Chest", Vault);
        else Add($"Key shards ({state.MapPieces})", state.MapPieces * PerPiece);
        if (state.VaultOpened && state.SongWhole) Add("The Locking, sung whole", SungWhole);
        Add($"Heartsongs ({state.SongsFound.Count})", state.SongsFound.Count * PerSong);
        Add($"Moves ({state.Tier - 1})", (state.Tier - 1) * PerRenovation);
        Add($"Collection ({state.Found.Count} of {Collection.Size})", state.Found.Count * PerFind);
        int endless = Math.Max(0, state.WeeksPaid - Guild.Weeks);
        Add($"Weeks after the debt ({endless})", endless * PerEndlessWeek);
        return lines;
    }

    public static int Total(GameState state) => Lines(state).Sum(l => l.Points);

    /// <summary>The run's score: its points, times its challenge level's multiplier.</summary>
    public static int Final(GameState state) => (int)MathF.Round(Total(state) * Challenges.Multiplier(state.Challenge));
}
