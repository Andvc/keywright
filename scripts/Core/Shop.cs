using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>The shop's size: who comes in, how many, and what they bring.</summary>
public static class ShopTiers
{
    public const int Max = 4;

    public static string Name(int tier) => tier switch
    {
        1 => "Bram's Stall",
        2 => "Thornwick Market",
        3 => "Crownhold",
        _ => "The Royal Emporium",
    };

    /// <summary>
    /// What a shop of this tier's chests are worth against the stall's, roughly: flat sums (a curio's fees, door money)
    /// grow with it, so they matter as much in the Emporium as at the stall.
    /// </summary>
    public static int Scale(int tier) => tier switch
    {
        1 => 1,
        2 => 3,
        3 => 8,
        _ => 20,
    };

    /// <summary>
    /// Customers a day at a shop of this tier, before signs, criers and an exhibition: four at the stall, six at
    /// Thornwick and in Crownhold, five at the Emporium, whose every customer is worth a day's takings anywhere else.
    /// (Four to seven until 0.20; 0.20's three at the stall and five after it left 0.21's working purse, two typical
    /// chests, out of reach for days after each move.)
    /// </summary>
    public static int Customers(int tier) => tier switch
    {
        1 => 4,
        2 or 3 => 6,
        _ => 5,
    };

    /// <summary>
    /// The colour a shop of this tier's chests usually are (the median of what its crowd brings): common at the stall,
    /// green at Thornwick, blue in Crownhold, purple at the Emporium. Purple is the finest a chest comes; the power index
    /// and the working purse are worked out at it.
    /// </summary>
    public static Rarity Typical(int tier) => (Rarity)Math.Clamp(tier - 1, 0, (int)Rarity.Epic);
}

/// <summary>A chest bought today, with the slip the player bought it on. It's opened straight away.</summary>
public sealed class Purchase
{
    public Visit Visit { get; }
    public Appraisal Slip { get; }
    public Chest Chest => Visit.Chest;
    public int Paid => Visit.PricePaid;

    /// <summary>How the lock came open (Clean if there was none).</summary>
    public PickGrade Grade { get; set; } = PickGrade.Clean;

    /// <summary>What curios' luck (and the restorers', and the Heartsongs') did to things inside when you bought it: whose, and how many things.</summary>
    public IReadOnlyList<(string Curio, int Changed)> Luck { get; init; } = Array.Empty<(string, int)>();

    /// <summary>How many things luck made better (or swung) in all.</summary>
    public int Improved => Luck.Sum(l => l.Changed);

    /// <summary>Whether the slip it was bought on was exactly right (once it's open: forcing may have broken glass since).</summary>
    public bool Perfect => Slip.Check(Chest).Perfect;

    /// <summary>What the seller claims of what's inside (an heirloom, the dearest thing), and what they pay you for it.</summary>
    public Item Claimed { get; init; }
    public int ClaimPays { get; init; }

    /// <summary>Whether this thing is the seller's, not yours to sell.</summary>
    public bool IsClaimed(Item item) => Claimed != null && item.Id == Claimed.Id;

    /// <summary>What a haggler's purse gave back when you bought it (already in your purse).</summary>
    public int Rebate { get; init; }

    /// <summary>Taken sight unseen: bought without a shake, a drop or the lodestone.</summary>
    public bool SightUnseen => !Visit.Handled;

    public Purchase(Visit visit, Appraisal slip)
    {
        Visit = visit;
        Slip = slip;
    }
}

/// <summary>How the day went, for the end-of-day summary.</summary>
public sealed class DayStats
{
    public int Bought, Passed, Spent, Earned, PerfectReads, NewFinds;

    /// <summary>Things handed over for commissions, and what completed commissions paid.</summary>
    public int Delivered, Commissions;

    /// <summary>What Bram slipped you from his secret stash this morning.</summary>
    public int Stash;

    /// <summary>What curios brought in (a catalogue's fees, interest, visitors' coins) and what curios cost.</summary>
    public int Curios, CurioSpent;

    /// <summary>What Isolde's visitors paid at the door tonight.</summary>
    public int Admission;

    public int Profit => Earned - Spent;
}

public sealed partial class GameState
{
    public const int StartingGold = 40;

    /// <summary>Display space you start with: a shelf behind the counter, with room for one thing.</summary>
    public const int ShelfSlots = 1;

    /// <summary>Exhibition points that draw one more customer a day (only one: a fine exhibition draws better chests, not crowds).</summary>
    public const int ExhibitionPerCustomer = 15, ExhibitionMaxCustomers = 1;

    /// <summary>Past this, an exhibition draws no more customers and no better chests.</summary>
    public const int ExhibitionCap = 40;

    /// <summary>Bram lets you spend on the shop once the stall has earned this much, all told.</summary>
    public const int UpgradesAt = 50;

    public int Seed { get; }
    public int Gold { get; set; } = StartingGold;
    public int Day { get; private set; }
    public int Tier { get; private set; } = 1;
    public Random Rng { get; private set; } = new();

    /// <summary>Whether Bram's tips are shown (and the first day is the scripted one).</summary>
    public bool Guided { get; set; } = true;

    public HashSet<string> HintsSeen { get; } = new();

    /// <summary>Regulars you've met: their habits go in the ledger.</summary>
    public HashSet<string> Met { get; } = new();

    /// <summary>Every kind of thing you've found, by its collection name.</summary>
    public HashSet<string> Found { get; } = new();

    /// <summary>Collection entries you've also found enchanted (the journal stars them).</summary>
    public HashSet<string> FoundEnchanted { get; } = new();

    /// <summary>Curios Nim has laid out for you, or you've owned, this run (the journal's curio page).</summary>
    public HashSet<string> CuriosSeen { get; } = new();

    /// <summary>The journal from earlier runs (the profile's): the ledger shows it with this run's finds.</summary>
    public Journal Known { get; } = new();

    /// <summary>
    /// What Bram's kin remember from earlier runs: the profile's journal, and its regulars as people you've met (their
    /// habits are in the ledger, and they post commissions like any regular).
    /// </summary>
    public void Remember(Profile profile)
    {
        Known.Met.UnionWith(profile.Met);
        Known.Found.UnionWith(profile.Found);
        Known.Enchanted.UnionWith(profile.FoundEnchanted);
        Known.Curios.UnionWith(profile.CuriosSeen);
        Met.UnionWith(profile.Met);
    }

    /// <summary>The journal has this thing: found this run or an earlier one.</summary>
    public bool InJournal(string entry) => Found.Contains(entry) || Known.Found.Contains(entry);

    /// <summary>The journal has this thing found enchanted.</summary>
    public bool InJournalEnchanted(string entry) => FoundEnchanted.Contains(entry) || Known.Enchanted.Contains(entry);

    /// <summary>The journal's count of things found, all runs together.</summary>
    public int JournalFound => Found.Union(Known.Found).Count();

    /// <summary>The journal's curio page shows this curio: Nim's laid it out, or you've had it, this run or an earlier one.</summary>
    public bool SawCurio(string key) => CuriosSeen.Contains(key) || Known.Curios.Contains(key) || HasCurio(key);

    /// <summary>For the GM: every page of the journal filled (regulars, things, curios), and every Heartsong known.</summary>
    public void KnowAll()
    {
        Met.UnionWith(Sellers.All.Select(s => s.Key));
        Known.Found.UnionWith(Collection.Entries.Select(e => e.Entry));
        Known.Enchanted.UnionWith(Collection.Entries.Where(e => !e.Fake).Select(e => e.Entry));
        Known.Curios.UnionWith(Curios.All.Select(c => c.Key));
        Songbook.UnionWith(Heartsongs.All.Select(h => h.Key));
    }

    /// <summary>What you kept to show off in the shop instead of selling (Heartsongs too, which take no shelf space).</summary>
    public List<Item> Display { get; } = new();

    /// <summary>
    /// The Heartsongs Bram's kin have ever opened: the profile's (<see cref="Profile.Songs"/>, merged in when a run starts
    /// or loads) and this run's.
    /// </summary>
    public HashSet<string> Songbook { get; } = new();

    /// <summary>The Heartsongs opened this run, in the order they turned up.</summary>
    public List<string> SongsFound { get; } = new();

    /// <summary>This Heartsong is on display: its effect is on.</summary>
    public bool Shows(string song) => Display.Any(i => i.IsSong && i.Song == song);

    /// <summary>Bram's kin have opened all nine: the Crown Chest can be sung whole.</summary>
    public bool SongWhole => Heartsongs.All.All(h => Songbook.Contains(h.Key));

    /// <summary>Sellers whose first-visit lesson has been given.</summary>
    public HashSet<string> Tutored { get; } = new();

    /// <summary>The scripted first day was taught in an earlier run: this one starts with the usual crowd.</summary>
    public bool FirstDayTaught { get; private set; }

    /// <summary>This run teaches the scripted first day (and Mags opens the second): the first run with tips on.</summary>
    public bool FirstDayScripted => GivesLessons && Tier == 1 && !FirstDayTaught;

    public const string FirstDayKey = "firstday", LessonKey = "lesson:";

    /// <summary>What Bram has taught this run, as the profile keeps it (<see cref="Profile.Taught"/>).</summary>
    public IEnumerable<string> Taught => HintsSeen.Concat(Tutored.Select(k => LessonKey + k))
        .Concat(FirstDayTaught || (Day >= 1 && GivesLessons) ? new[] { FirstDayKey } : Array.Empty<string>());

    /// <summary>Places chests come from that you've heard of (from a seller's story): the ledger's Places page.</summary>
    public HashSet<string> Places { get; } = new();

    /// <summary>A seller's told you where their chest came from: that's a place you've heard of.</summary>
    public void HeardStory(Visit visit)
    {
        if (visit.Origin != null && visit.Seller != Story.VaultSeller) Places.Add(visit.Origin.Key);
    }

    // The shop opens up a step at a time: renovations once the stall has earned its keep, a shelf to display things
    // with the village shop, synergies with the town shop, where people start to know you by what you collect.

    /// <summary>What the chests you've bought and opened have brought in, all told.</summary>
    public int TotalEarned { get; private set; }

    /// <summary>Renovations and tools are on offer: the stall has earned <see cref="UpgradesAt"/>.</summary>
    public bool UpgradesOpen { get; private set; }

    /// <summary>Keeping things for display: from the village shop, which has a shelf (Isolde brings her own).</summary>
    public bool DisplayOpen => Tier >= 2 || Is(Characters.Isolde);

    /// <summary>Synergies: from the town shop (the village shop, with an early name).</summary>
    public bool SynergiesOpen => Tier >= (Knows("early") ? 2 : 3);

    public int DisplaySlots => DisplayOpen ? (Is(Characters.Isolde) ? Characters.IsoldeShelf : ShelfSlots) + Upgrades.All.Where(u => Has(u.Key)).Sum(u => u.Slots) +
                                             (HasCurio("dome") ? 1 : 0) : 0;
    public bool DisplayFull => Display.Count(i => !i.IsSong) >= DisplaySlots;

    /// <summary>The exhibition: what your displayed things are worth to visitors' eyes, not their purses.</summary>
    public int Exhibition => (int)MathF.Round(Display.Sum(i => i.Prestige * (i.Category == Category.Garb && HasCurio("dummy") ? Curios.DummyPrestige : 1)) *
                                              (1 + Synergies.ShowBonus(Active("tailor"))) * (HasCurio("rope") ? Curios.RopeExhibition : 1));

    // The collection journal: everything acquired, by kind and quality, and the feats synergies ask for.
    readonly int[,] _acquired = new int[Categories.All.Length, 5];

    /// <summary>How many things of this kind you've acquired at this quality or better (fakes and broken glass don't count).</summary>
    public int Acquired(Category kind, Rarity atLeast)
    {
        int n = 0;
        for (int q = (int)atLeast; q < 5; q++) n += _acquired[(int)kind, q];
        return n;
    }

    public int MagicFound { get; private set; }
    public int PerfectStreak { get; private set; }
    public int BestStreak { get; private set; }

    /// <summary>The synergies in effect, no more than there are slots for.</summary>
    public HashSet<string> ActiveSynergies { get; } = new();

    /// <summary>One synergy at a time, more with a guild charter and a royal seal.</summary>
    public int SynergySlots => 1 + Upgrades.All.Where(u => Has(u.Key)).Sum(u => u.SynergySlots) + (Knows("favour") ? 1 : 0);

    public int SynergyLevel(string key) => Synergies.Get(key).Level(this);

    /// <summary>A synergy's level if it's active, else 0.</summary>
    public int Active(string key) => SynergiesOpen && ActiveSynergies.Contains(key) ? SynergyLevel(key) : 0;

    /// <summary>Turns a reached synergy on (if there's a free slot) or off. Returns whether it's active now.</summary>
    public bool ToggleSynergy(string key)
    {
        if (ActiveSynergies.Remove(key) || !SynergiesOpen) return false;
        if (ActiveSynergies.Count >= SynergySlots || SynergyLevel(key) == 0) return false;
        ActiveSynergies.Add(key);
        return true;
    }

    /// <summary>What the exhibition, synergies, the Guild's terms, curios and who's at the counter do to the chests people bring.</summary>
    public Boosts Boosts => new(BetterChests + (EventIs("storm") ? 1 : 0),
        Synergies.HiddenBonus(Active("archivist")) + Synergies.FinderHidden(Active("finder")) +
        (HasCurio("log") ? Math.Min(Curios.LogMax, Curios.LogStep * BottomsOpened) : 0),
        Synergies.MagicBonus(Active("arcane")) + (Shows(Heartsongs.Star) ? Heartsongs.StarMagic : 0) + (EventIs("moon") ? Events.MoonMagic : 0) +
        (HasCurio("aether") ? Math.Min(Curios.AetherMax, Curios.AetherStep * MagicFound) : 0),
        EventIs("wreck") ? Events.WreckPairs : 0, Synergies.MoneyKinds(Active("jewelry")), PriceFactor,
        Under(Challenges.Forgers) ? Challenges.SlugChance : 0, HagglesAll || HasCurio("coffer") || HasCurio("goldentongue"))
    {
        HiddenFactor = HasCurio("hook") ? Curios.HookHidden : 1,
        MagicFactor = HasCurio("draught") ? Curios.DraughtMagic : 1,
        Floor = (HasCurio("tongue") ? Curios.TongueFloor : 1) * (HasCurio("signet") ? Curios.SignetFloor : 1) * (1 - Synergies.HagglerFloor(Active("haggler"))) *
                (HasCurio("goldentongue") ? Curios.GoldenFloor : 1),
        AlwaysHidden = HasCurio("compass"),
        AlwaysWard = HasCurio("wardlamp") || HasCurio("aldricseal"),
        MagicSpreads = HasCurio("chalk"),
        MagicAll = HasCurio("staff"),
    };

    /// <summary>How much less likely glass is to break (a vintner's packing).</summary>
    public float Sturdier => Synergies.Sturdier(Active("vintner"));

    /// <summary>When you drop a chest: in a padded crate, or by the Imp's Ember, glass never breaks.</summary>
    public float DropSturdier => HasCurio("crate") || Shows(Heartsongs.Ember) ? 1f : Sturdier;

    /// <summary>A lock as you'll pick it: one pin fewer with a locksmith's oil.</summary>
    public LockTier Oiled(LockTier lockTier) => HasCurio("oil") && lockTier.Pins > 1 ? lockTier with { Pins = lockTier.Pins - 1 } : lockTier;

    /// <summary>
    /// How long a seller gives you, in seconds (0: all day): an impatient one's patience, or everyone's when the town is
    /// in a hurry; twice as long with a pocket watch, and all day while the Lullaby Box plays.
    /// </summary>
    public float Patience(SellerDef seller)
    {
        if (Shows(Heartsongs.Lullaby)) return 0;
        float patience = seller.Patience;
        if (Under(Challenges.Hurried)) patience = patience > 0 ? Math.Min(patience, Challenges.HurriedPatience) : Challenges.HurriedPatience;
        return patience * (HasCurio("watch") ? Curios.WatchPatience : 1);
    }

    /// <summary>
    /// In kid gloves, a seller who won't stand for a hard shake, a drop or the magnet doesn't notice; with the Lullaby Box
    /// playing, nobody minds.
    /// </summary>
    public bool KidGloves => HasCurio("gloves") || Shows(Heartsongs.Lullaby);

    /// <summary>Enchanted things glow when they sound: the witchlight lamp, the Bottled Star's light, or a witch's monocle.</summary>
    public bool Witchlight => Has("lamp") || Shows(Heartsongs.Star) || HasCurio("monocle");

    /// <summary>
    /// What everything of this kind sells for here today, against its worth: the buyers' trades you've set up (scales,
    /// the auction block, the royal warrant, multiplied together), the Dragon's Egg for coin and gems, and today's event.
    /// (What the shop's name and build add is <see cref="General"/> and <see cref="Special"/>.)
    /// </summary>
    public float SaleMultiplier(Category kind) => EventSales(kind) * PlainSales(kind);

    /// <summary>
    /// What the buyers' trades you've set up put on every sale, multiplied together. Word gets round: they're in the
    /// shop's power index, so sellers ask that much more of a shop known to sell high, and a trade multiplies your edge;
    /// it never makes buying blind pay.
    /// </summary>
    public float Buyers => Upgrades.All.Where(u => u.Sales > 0 && Has(u.Key)).Aggregate(1f, (m, u) => m * (1 + u.Sales));

    /// <summary>Notes something you opened in the journal.</summary>
    public void Acquire(Item item)
    {
        if (item.Fake || item.Broken || item.IsSong) return;
        _acquired[(int)item.Category, (int)item.Quality]++;
        if (item.Magic) MagicFound++;
    }

    /// <summary>Notes whether a bought chest's slip was perfect, for the streak.</summary>
    public void RecordRead(bool perfect)
    {
        PerfectStreak = perfect ? PerfectStreak + 1 : 0;
        if (perfect) PerfectReads++;
        BestStreak = Math.Max(BestStreak, PerfectStreak);
    }

    /// <summary>People come to see the exhibition: one more customer a day per so many points, up to two.</summary>
    public int ExhibitionCustomers => Math.Min(ExhibitionMaxCustomers, Exhibition / ExhibitionPerCustomer);

    /// <summary>And a fine exhibition draws a better class of customer: the chance a chest is one colour better.</summary>
    public float BetterChests => MathF.Min(ExhibitionCap, Exhibition) * 0.01f;

    readonly HashSet<string> _owned = new();

    public List<Visit> Queue { get; } = new();
    public Visit Current { get; private set; }
    public DayStats Today { get; private set; } = new();

    /// <summary>The purchase being opened.</summary>
    public Purchase Opening { get; set; }

    public GameState(int seed)
    {
        Seed = seed;
    }

    public bool Has(string upgrade) => _owned.Contains(upgrade);

    public int CustomersPerDay => Math.Max(2, ShopTiers.Customers(Tier) + Upgrades.All.Where(u => Has(u.Key)).Sum(u => u.Customers) + ExhibitionCustomers +
                                            (Shows(Heartsongs.Pipe) ? Heartsongs.PipeCustomers : 0) +
                                            (HasCurio("lantern") ? Curios.LanternCustomers : 0) + (HasCurio("horn") ? Curios.HornCustomers : 0) +
                                            EventCustomers - (Under(Challenges.ThinCrowds) ? 1 : 0) - (QuietOpening ? 1 : 0));

    /// <summary>The first day the shop's had at this tier (a move inward is bought the evening before).</summary>
    public int TierSince { get; private set; }

    /// <summary>
    /// With Bram's tips on, the first day at a grander shop is a quieter one, a customer short, while you find your feet:
    /// a move brings a new place, new sellers and new things to learn all at once (the experience playtest: too much on one
    /// day at Thornwick).
    /// </summary>
    bool QuietOpening => Guided && Tier > 1 && Day == TierSince;

    public bool CanBuy(Upgrade upgrade) => UpgradesOpen && Gold >= Cost(upgrade) && Upgrades.Available(this).Contains(upgrade);

    public void BuyUpgrade(Upgrade upgrade)
    {
        if (!CanBuy(upgrade)) return;
        Gold -= Cost(upgrade);
        _owned.Add(upgrade.Key);
        if (upgrade.SetsTier > 0)
        {
            Tier = upgrade.SetsTier;
            TierSince = Day + 1;
        }
    }

    /// <summary>Hands out a hint key once. Returns false if it's been shown before or tips are off.</summary>
    public bool FirstTime(string hint) => Guided && HintsSeen.Add(hint);

    /// <summary>What carries over to tomorrow, to be written to a save file (taken between days).</summary>
    public SaveData Save()
    {
        var data = new SaveData
        {
            SavedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            Seed = Seed, Gold = Gold, Day = Day, Tier = Tier, Guided = Guided, TotalEarned = TotalEarned,
            Character = Character, Challenge = Challenge, Talents = Talents.ToList(), Snubbed = _snubbed.ToList(), MaraUsed = MaraUsed,
            KeepsakeTaken = KeepsakeTaken, FirstDayTaught = FirstDayTaught,
            UpgradesOpen = UpgradesOpen, MagicFound = MagicFound, PerfectStreak = PerfectStreak, BestStreak = BestStreak,
            Owned = _owned.ToList(), HintsSeen = HintsSeen.ToList(), Met = Met.ToList(), Found = Found.ToList(),
            FoundEnchanted = FoundEnchanted.ToList(), Tutored = Tutored.ToList(), ActiveSynergies = ActiveSynergies.ToList(), CuriosSeen = CuriosSeen.ToList(),
            Places = Places.ToList(),
            Display = Display.Select(i => new SavedItem { Category = i.Category, Quality = i.Quality, Magic = i.Magic, Song = i.Song }).ToList(),
            SongsFound = SongsFound.ToList(),
            WeeksPaid = WeeksPaid, Arrears = Arrears, GraceUsed = GraceUsed > 0, GraceCount = GraceUsed, BramPiece = BramPiece, StashUsed = StashUsed, ClearedDay = ClearedDay, CloverWeek = CloverWeek, PeddlerTier = PeddlerTier, TierSince = TierSince,
            CurioPaid = _curios.Select(key => _curioPaid.GetValueOrDefault(key)).ToList(),
            DrySpell = DrySpell, HoodPieces = HoodPieces, MapGiven = MapGiven, VaultOpened = VaultOpened,
            PassStreak = PassStreak, LuckStreak = LuckStreak, Surprises = Surprises, HiddenFound = HiddenFound, Bargains = Bargains,
            PerfectReads = PerfectReads, BottomsOpened = BottomsOpened, WardsNamed = WardsNamed, SightUnseen = SightUnseen,
            CuriosOpen = CuriosOpen?.ToList(), Curios = _curios.ToList(), EventsSeen = _eventsSeen.ToList(), Plan = Plan, NextPlan = NextPlan,
            Commissions = Commissions.Select(c => new SavedCommission
            {
                Patron = c.Patron, Kind = c.Kind, Category = c.Category, Quality = c.Quality, Needed = c.Needed, Delivered = c.Delivered,
                Reward = c.Reward, Story = c.Story, Carried = c.Carried, Lapses = c.Lapses,
            }).ToList(),
        };
        foreach (int count in _acquired) data.Acquired.Add(count);
        return data;
    }

    /// <summary>A shop as it was saved: ready to start the next day.</summary>
    public static GameState Load(SaveData data)
    {
        var state = new GameState(data.Seed)
        {
            Character = Characters.Exists(data.Character) ? data.Character : Characters.Kit,
            Challenge = Math.Clamp(data.Challenge, 0, Challenges.Max),
            Talents = new HashSet<string>(data.Talents.Where(Core.Talents.Exists)),
            MaraUsed = data.MaraUsed, KeepsakeTaken = data.KeepsakeTaken, FirstDayTaught = data.FirstDayTaught,
            Gold = data.Gold, Day = data.Day, Tier = Math.Clamp(data.Tier, 1, ShopTiers.Max), Guided = data.Guided,
            TotalEarned = data.TotalEarned, UpgradesOpen = data.UpgradesOpen, MagicFound = data.MagicFound,
            PerfectStreak = data.PerfectStreak, BestStreak = data.BestStreak,
            WeeksPaid = data.WeeksPaid, Arrears = data.Arrears, GraceUsed = Math.Max(data.GraceCount, data.GraceUsed ? 1 : 0), BramPiece = data.BramPiece,
            StashUsed = Math.Max(0, data.StashUsed), ClearedDay = data.ClearedDay, CloverWeek = data.CloverWeek, PeddlerTier = data.PeddlerTier, TierSince = data.TierSince, DrySpell = data.DrySpell, HoodPieces = data.HoodPieces,
            MapGiven = data.MapGiven,
            VaultOpened = data.VaultOpened, NextPlan = data.NextPlan,
            PassStreak = data.PassStreak, LuckStreak = data.LuckStreak, Surprises = data.Surprises,
            HiddenFound = data.HiddenFound, Bargains = data.Bargains, PerfectReads = data.PerfectReads, BottomsOpened = data.BottomsOpened,
            WardsNamed = data.WardsNamed, SightUnseen = data.SightUnseen, CuriosOpen = data.CuriosOpen?.Where(Curios.Exists).ToHashSet(),
            // A save from before plans: this week's one event and Nim's day, as they were.
            Plan = data.Plan ?? new WeekPlan
            {
                Week = Math.Max(0, data.Day - 1) / Guild.DaysPerWeek + 1, Wanted = data.BuyerKind, Peddler = data.PeddlerDay,
                Days = Enumerable.Range(1, Guild.DaysPerWeek).Select(d => d == data.EventDay ? data.WeekEvent : null).ToList(),
            },
        };
        // Curios from an older version that are gone now are simply gone; the rest keep what you paid (or half today's price).
        for (int i = 0; i < data.Curios.Count; i++)
        {
            string key = data.Curios[i];
            if (!Curios.Exists(key) || state._curios.Contains(key)) continue;
            state._curios.Add(key);
            if (i < data.CurioPaid.Count && data.CurioPaid[i] > 0) state._curioPaid[key] = data.CurioPaid[i];
        }
        state._eventsSeen.AddRange(data.EventsSeen);
        // A save from before plans: next week's is made now, as it would have been this week.
        state.NextPlan ??= Events.Plan(state.Seed, state.Plan.Week + 1, state.Tier, state._eventsSeen);
        state._snubbed.UnionWith(data.Snubbed);
        state.Commissions.AddRange(data.Commissions.Select(c => new Commission
        {
            Patron = c.Patron, Kind = c.Kind, Category = c.Category, Quality = c.Quality, Needed = c.Needed, Delivered = c.Delivered,
            Reward = c.Reward, Story = c.Story, Carried = c.Carried, Lapses = c.Lapses,
        }));
        state._owned.UnionWith(data.Owned);
        state.HintsSeen.UnionWith(data.HintsSeen);
        state.Met.UnionWith(data.Met);
        // Things renamed in 0.17 keep their place in the journal.
        state.Found.UnionWith(data.Found.Select(name => Categories.Renamed.GetValueOrDefault(name, name)));
        state.FoundEnchanted.UnionWith(data.FoundEnchanted.Select(name => Categories.Renamed.GetValueOrDefault(name, name)));
        state.SongsFound.AddRange(data.SongsFound.Where(Heartsongs.Exists).Distinct());
        state.Songbook.UnionWith(state.SongsFound);
        state.Tutored.UnionWith(data.Tutored);
        state.CuriosSeen.UnionWith(data.CuriosSeen.Where(Curios.Exists));
        state.Places.UnionWith(data.Places.Where(key => Origins.All.Any(o => o.Key == key)));
        state.ActiveSynergies.UnionWith(data.ActiveSynergies.Where(key => Synergies.All.Any(s => s.Key == key)));
        state.Display.AddRange(data.Display.Where(i => i.Category != Category.Heartsong || Heartsongs.Exists(i.Song))
            .Select(i => new Item(i.Category, i.Quality, i.Magic, Song: i.Song)));
        int kinds = state._acquired.GetLength(0), qualities = state._acquired.GetLength(1);
        for (int i = 0; i < Math.Min(data.Acquired.Count, kinds * qualities); i++)
            state._acquired[i / qualities, i % qualities] = data.Acquired[i];
        return state;
    }

    /// <summary>For debugging and screenshots: jump ahead with the tools a player would have by then.</summary>
    /// <summary>For debugging (--engine): owns an upgrade without buying it.</summary>
    public void Grant(string upgrade) => _owned.Add(upgrade);

    public void SkipTo(int tier, int gold)
    {
        foreach (var upgrade in Upgrades.All.Where(u => (u.IsRenovation ? u.SetsTier <= tier : u.MinTier < tier) && u.Key != Upgrades.Engine))
            _owned.Add(upgrade.Key);
        Tier = tier;
        Gold = gold;
        UpgradesOpen = true;
    }

    public void StartDay()
    {
        Day++;
        Rng = new Random(unchecked(Seed * 7919 + Day));
        Queue.Clear();
        Current = null;
        Today = new DayStats();
        _songRng = new Random(unchecked(Seed * 3571 + Day));
        PeddlerRerolled = false;
        DipIntoStash();
        if (DayOfWeek == 1) StartWeek();
        PeddlerStock.Clear();
        PeddlerPurse = Gold;
        if (PeddlerToday)
        {
            PeddlerStock.AddRange(Curios.Stock(new Random(unchecked(Seed * 6151 + Day)), this, PeddlerCount));
            PeddlerTier = Tier;
        }
        CuriosSeen.UnionWith(PeddlerStock.Select(c => c.Key));

        if (Day == 1 && FirstDayScripted)
        {
            Queue.AddRange(FirstDay(Rng));
            return;
        }

        // Each renovation brings its own crowd; the old regulars still drop by now and then. Nobody comes twice in a
        // day unless everyone else already has. A smithing name draws smiths and soldiers. With tips on, no more than
        // a few newcomers (each a lesson) a day: the rest of a new shop's crowd comes in over the next days. Anyone who
        // walked out on Rosa this week stays away, while there's anyone else.
        // Some come in only some days (a delver between barrows, the King, the Maw).
        var sellers = Sellers.All.Where(s => s.Tier <= Tier && (s.Visits >= 1 || Rng.NextFloat() < s.Visits)).ToArray();
        if (sellers.Count(s => !_snubbed.Contains(s.Key)) >= 2) sellers = sellers.Where(s => !_snubbed.Contains(s.Key)).ToArray();
        float smiths = Synergies.SmithVisits(Active("smith"));
        while (Queue.Count < CustomersPerDay)
        {
            var fresh = sellers.Where(s => Queue.All(v => v.Seller != s)).ToArray();
            var pool = fresh.Length > 0 ? fresh : sellers.Where(s => Queue[^1].Seller != s).ToArray();
            if (Queue.Count(v => v.Lesson != null) >= LessonsPerDay(Tier))
            {
                var known = pool.Where(s => !Teaches(s)).ToArray();
                if (known.Length > 0) pool = known;
            }
            // A fence's mark draws crooks: whoever deals in fakes comes twice as often.
            var seller = Rng.PickWeighted(pool, s => (s.Tier == Tier ? 3f : s.Tier == Tier - 1 ? 0.6f : 0.15f) * (s.Metalworker ? smiths : 1f) *
                                                     (s.Fakes.Length > 0 && HasCurio("mark") ? Curios.MarkCrooks : 1f));
            // A guest book: the day's first customer is the better sort, with a chest a colour finer.
            Queue.Add(NewVisit(seller, HasCurio("guestbook") && Queue.Count == 0));
        }
        // The scripted first day's last lesson, passing, opens the second (the playtest: four new things on the first day
        // and Vane's debt besides was too much at once).
        if (Day == 2 && FirstDayScripted)
        {
            // Instead of the day's last customer, or of Mags herself if she was coming anyway (her own lesson waits).
            int mags = Queue.FindIndex(v => v.Seller.Key == "mags");
            if (mags >= 0 && Queue[mags].Lesson != null) Tutored.Remove("mags");
            Queue.RemoveAt(mags >= 0 ? mags : Queue.Count - 1);
            Queue.Insert(0, Mags(Rng));
        }
        StageEvent();
        Curiosities();
    }

    /// <summary>
    /// With tips on, at most this many sellers' lessons in one day (the playtest: three, on a day of four or five customers,
    /// was too much at once; and Thornwick brings locks, the drop, straw and warded things too, so one a day there).
    /// </summary>
    public static int LessonsPerDay(int tier) => tier == 2 ? 1 : 2;

    /// <summary>Their next visit would be a lesson.</summary>
    bool Teaches(SellerDef seller) => GivesLessons && Lessons.For(seller.Key) != null && !Tutored.Contains(seller.Key);

    /// <summary>A seller's visit: the first time (with tips on), the lesson that introduces their habit.</summary>
    /// <param name="finer">Their chest is a colour finer (a guest book's better sort).</param>
    Visit NewVisit(SellerDef seller, bool finer = false)
    {
        var lesson = Lessons.For(seller.Key);
        if (GivesLessons && lesson != null && Tutored.Add(seller.Key))
        {
            var visit = lesson.Make(Rng, Tier);
            visit.Lesson = lesson;
            return visit;
        }
        var fresh = Market.CreateVisit(Rng, seller, Tier, finer ? Boosts with { BetterChests = 1 } : Boosts);
        // A merchant's signet (or Rosa's charm): hagglers hear out an offer that's too low, instead of storming off. A
        // golden tongue: nobody storms off at all.
        fresh.HearsOut = HasCurio("signet") || HasCurio("coffer") || HagglesAll || HasCurio("goldentongue");
        fresh.Unruffled = HasCurio("goldentongue");
        Sing(fresh);
        return fresh;
    }

    /// <summary>Its own dice, so a Heartsong turning up (or not) leaves every other roll of the day as it was.</summary>
    Random _songRng = new();

    /// <summary>
    /// Maybe a Heartsong in a chest someone's brought, after they've priced it (they never know it's there). One at a
    /// time: never one that's already been found this run or is in a chest waiting at the counter.
    /// </summary>
    Visit Sing(Visit visit)
    {
        var taken = SongsFound.Concat(Queue.Select(v => v.Chest.Song?.Song)).Where(k => k != null).ToHashSet();
        if (Heartsongs.Roll(_songRng, Songbook, taken, (EventIs("minstrel") ? Events.MinstrelSongs : 1) * (HasCurio("net") ? Curios.NetSongs : 1)) is HeartsongDef song) visit.Chest.AddSong(Heartsongs.Make(song, visit.Chest.Quality));
        return visit;
    }

    /// <summary>
    /// The first day teaches one thing per customer: a gentle shake and the slip (Tam never shook his chest), a hard
    /// shake (Penn shook hers too gently to hear the cloak), and a gentle shake under a clank (Hob shook hard and the
    /// knife drowned out the coins). The second opens with passing (<see cref="Mags"/>).
    /// </summary>
    static IEnumerable<Visit> FirstDay(Random rng)
    {
        // Tam names a price today, so the first lesson is only about listening; from tomorrow he asks you to name one.
        yield return Market.Price(rng, Sellers.Get("tam"), new Chest(new[] { new Item(Category.Coins, Rarity.Common) }), 1, 4, Trade.Fixed,
            "Fished it out of the millpond this morning. I didn't shake it, it might break! Is {price} okay?");
        yield return Scripted(rng, "penn", 3, Category.Garb);
        yield return Scripted(rng, "hob", 7, Category.Coins, Category.Arms);
    }

    /// <summary>The second morning's first customer: Mags heard everything there was, and asks what it's worth. Pass.</summary>
    static Visit Mags(Random rng) => Scripted(rng, "mags", 7, Category.Arms);

    static Visit Scripted(Random rng, string seller, int ask, params Category[] kinds) =>
        Market.Price(rng, Sellers.Get(seller), new Chest(kinds.Select(k => new Item(k, Rarity.Common))), 1, ask, Trade.Fixed);

    /// <summary>Calls in the next customer, if anyone is waiting.</summary>
    public bool NextCustomer()
    {
        if (Current != null) NoteWalkout(Current);
        Current = null;
        if (Queue.Count == 0) return false;
        Current = Queue[0];
        Queue.RemoveAt(0);
        Greet(Current);
        return true;
    }

    /// <summary>Records meeting a seller. True the first time.</summary>
    public bool Meet(SellerDef seller) => Met.Add(seller.Key);

    public Purchase Buy(Visit visit, Appraisal slip)
    {
        Gold -= visit.PricePaid;
        Today.Bought++;
        Today.Spent += visit.PricePaid;
        int rebate = Rebate(visit);
        Gold += rebate;
        Today.Spent -= rebate;
        // Named truly, a warded thing comes out of the ward: it's in the chest for the luck and the opening.
        visit.Chest.Open(slip);
        var luck = Luck(visit.Chest, !visit.Handled);
        Pity(visit.Chest);
        Counted(visit, visit.Chest, slip);
        var (claimed, pays) = Claim(visit);
        HeardStory(visit);
        // The Maw's gaze: everyone still waiting today wants a little more.
        if (visit.Seller.Gaze > 0)
            foreach (var waiting in Queue) waiting.Raise(visit.Seller.Gaze);
        return new Purchase(visit, slip) { Luck = luck, Claimed = claimed, ClaimPays = pays, Rebate = rebate };
    }

    /// <summary>
    /// What the seller claims of what's inside, once it's yours: a knight's family weapons (the best of them, for the fee
    /// he named), or a devil's due (the dearest thing, for nothing).
    /// </summary>
    static (Item Item, int Pays) Claim(Visit visit)
    {
        var seller = visit.Seller;
        if (seller.Heirloom is Category kind)
        {
            var best = visit.Chest.Everything.Where(i => i.Category == kind && !i.Fake).OrderByDescending(i => i.Value).FirstOrDefault();
            return best == null ? (null, 0) : (best, visit.Fee);
        }
        // A devil can't price a Heartsong, so he never takes one.
        if (seller.TakesDearest) return (visit.Chest.Everything.Where(i => !i.IsSong).OrderByDescending(i => i.Value).FirstOrDefault(), 0);
        return (null, 0);
    }

    /// <summary>
    /// What the seller's terms come to once the chest is open (<paramref name="held"/>: what came out, as sold): a won
    /// wager's share of the price back, or the King's game settled (a quarter back for a perfect read; a wrong one pays
    /// what it truly held and a quarter more, as far as your purse goes). Added to (or taken from) your gold with the
    /// chest's takings.
    /// </summary>
    public int Settle(Purchase purchase, bool perfect, int held)
    {
        var seller = purchase.Visit.Seller;
        if (seller.Wager > 0) return perfect ? Money.Round(purchase.Paid * seller.Wager) : 0;
        if (seller.SlipPrice)
            return perfect ? Money.Round(purchase.Paid * Market.SlipRefund)
                : -Math.Min(Gold + held, Math.Max(0, Money.Round(held * Market.SlipFine) - purchase.Paid));
        return 0;
    }

    /// <summary>
    /// Notes a find in the collection and the journal (a new kind of find this run pays a collector's catalogue). True
    /// the first time it's in the journal at all, this run or any before (a Heartsong: the first time Bram's kin have
    /// ever opened it).
    /// </summary>
    public bool Collect(Item item)
    {
        if (item.IsSong)
        {
            if (!SongsFound.Contains(item.Song)) SongsFound.Add(item.Song);
            if (!Songbook.Add(item.Song)) return false;
            Today.NewFinds++;
            return true;
        }
        Acquire(item);
        if (item.Magic && !item.Broken && !item.Fake) FoundEnchanted.Add(item.Entry);
        bool known = Known.Found.Contains(item.Entry);
        if (item.Broken || !Found.Add(item.Entry)) return false;
        Today.NewFinds++;
        Catalogue();
        return !known;
    }

    /// <summary>
    /// Puts something on display instead of selling it: you give up its price. A Heartsong needs no shelf (Bram hangs
    /// it from the awning, if he must), so it can go on display at any shop, and takes no room from anything else.
    /// </summary>
    public bool Keep(Item item, int value)
    {
        if (!(DisplayOpen || item.IsSong) || !item.Displayable || (DisplayFull && !item.IsSong) || Gold < value) return false;
        Display.Add(item);
        Gold -= value;
        Today.Earned -= value;
        return true;
    }

    /// <summary>Takes something off display and sells it.</summary>
    public void SellFromDisplay(Item item)
    {
        if (!Display.Remove(item)) return;
        Gold += SaleValue(item);
    }

    /// <summary>For the GM: the rest of today skipped. The customer at the counter and everyone waiting go home.</summary>
    public void SkipRestOfDay()
    {
        if (Current is { Deciding: true })
        {
            Current.Pass(Rng);
            Pass();
        }
        Current = null;
        Queue.Clear();
    }

    /// <summary>The customer at the counter has gone without a sale (passed, or walked out).</summary>
    public void Pass()
    {
        Today.Passed++;
        Passed(Current);
    }

    public void Earn(int amount)
    {
        Gold += amount;
        Today.Earned += amount;
    }

    /// <summary>
    /// Closes the day: adds what its chests brought in to the total (renovations open once it's enough) and what curios
    /// bring in overnight.
    /// </summary>
    public void EndDay()
    {
        TotalEarned += Today.Earned;
        if (TotalEarned >= UpgradesAt) UpgradesOpen = true;
        Overnight();
    }
}
