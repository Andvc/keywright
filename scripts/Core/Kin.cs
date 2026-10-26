using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// Who's minding the stall this run (one of Bram's kin), and on what terms (the challenge level, and the banes and boons it
/// drew). All fixed when the run starts; nothing else carries over from runs before but what's known (the journal, what
/// Bram has taught).
/// </summary>
public sealed partial class GameState
{
    /// <summary>Which of Bram's kin is minding the stall.</summary>
    public string Character { get; init; } = Characters.Kit;

    public CharacterDef Kin => Characters.Get(Character);

    public bool Is(string character) => Character == character;

    /// <summary>The challenge level: the Guild's dues are that many steps higher, and the run has as many banes and boons.</summary>
    public int Challenge { get; init; }

    /// <summary>This run's banes (<see cref="Challenges.Banes"/>), in the order they were dealt when it started.</summary>
    public List<string> Banes { get; } = new();

    /// <summary>This run's boons (<see cref="Challenges.Boons"/>), in the order they were dealt when it started.</summary>
    public List<string> Boons { get; } = new();

    /// <summary>One of the season's banes is against you this run.</summary>
    public bool Under(string bane) => Banes.Contains(bane);

    /// <summary>One of the boons is with you this run.</summary>
    public bool Favoured(string boon) => Boons.Contains(boon);

    /// <summary>
    /// Bram's lessons (each new seller's first visit, and the scripted first day) are for Kit, with tips on: the others
    /// come to the stall knowing the trade, though Bram still has a word for what's new.
    /// </summary>
    public bool GivesLessons => Guided && Is(Characters.Kit);

    /// <summary>A new run: who, and on what terms; and the gold and tools they start with. Every run starts the same.</summary>
    /// <param name="taught">What Bram taught in earlier runs (<see cref="Profile.Taught"/>): not taught again.</param>
    /// <param name="curios">The curios Nim carries this run (<see cref="Profile.OpenCurios"/>); null: all of them.</param>
    /// <param name="terms">The level's banes and boons, drawn on the character screen; null: drawn from the run's seed.</param>
    public static GameState NewRun(int seed, string character = Characters.Kit, int challenge = 0, bool guided = true,
        IEnumerable<string> taught = null, IEnumerable<string> curios = null, (List<string> Boons, List<string> Banes)? terms = null)
    {
        var state = new GameState(seed)
        {
            Character = Characters.Exists(character) ? character : Characters.Kit,
            Challenge = Math.Clamp(challenge, 0, Challenges.Max),
        };
        var (boons, banes) = terms ?? Challenges.Draw(seed, state.Challenge);
        state.Boons.AddRange(boons);
        state.Banes.AddRange(banes);
        state.Learned(taught, firstDay: true);
        state.Guided = guided;
        state.CuriosOpen = curios?.ToHashSet();
        state.Gold = StartGold(state);
        // Bram's old picks: the fine lockpicks from the first day.
        if (state.Favoured(Challenges.OldPicks)) state._owned.Add("picks");
        return state;
    }

    static int StartGold(GameState state) => state.Under(Challenges.LeanStart) ? StartingGold / 2
        : state.Favoured(Challenges.Savings) ? Money.Round(StartingGold * Challenges.SavingsFactor) : StartingGold;

    // ---- Hesk: a bad back and an honest scale ----

    /// <summary>Hesk's back won't let him shake a chest hard.</summary>
    public bool CanShakeHard => !Is(Characters.Hesk);

    /// <summary>Nor will Wenna's mare, for anyone.</summary>
    public bool CanShakeHardAt(SellerDef seller) => CanShakeHard && !seller.Packhorse && !EventIs("mourning");

    /// <summary>Nor lift it to drop it.</summary>
    public bool CanDrop => !Is(Characters.Hesk);

    /// <summary>Every chest goes on Hesk's scale.</summary>
    public bool Weighs => Is(Characters.Hesk);

    // ---- Rosa: everybody haggles, and word gets round ----

    /// <summary>Everybody haggles with Rosa.</summary>
    public bool HagglesAll => Is(Characters.Rosa);

    /// <summary>Sellers who walked out on Rosa this week: they won't be back until the next.</summary>
    readonly HashSet<string> _snubbed = new();

    public IReadOnlyCollection<string> Snubbed => _snubbed;

    /// <summary>
    /// The seller just gone walked out. On an offer of Rosa's, word gets round, and they stay away this week; with the
    /// Shepherd's Pipe singing, they come back at the end of the day, calmer, at their first price (once).
    /// </summary>
    void NoteWalkout(Visit visit)
    {
        if (visit is not { Outcome: VisitOutcome.Left } || visit.Lesson != null) return;
        if (Shows(Heartsongs.Pipe) && !visit.Returned) Queue.Add(visit.Again());
        else if (HagglesAll && visit.OffersMade > 0) _snubbed.Add(visit.Seller.Key);
    }

    // ---- Isolde: a collection that pays its way ----

    /// <summary>What Isolde's visitors pay tonight: so much a point of exhibition.</summary>
    public int Admission => Exhibition * ((Is(Characters.Isolde) ? Characters.AdmissionPerPoint * Characters.IsoldeScale(Tier) : 0) +
                                          (Shows(Heartsongs.Hearth) ? Heartsongs.HearthPerPoint * ShopTiers.Scale(Tier) : 0));

    /// <summary>
    /// Whether this displayed thing can go to the Guild on a short night (never Isolde's, and never a Heartsong: greed
    /// can't price one).
    /// </summary>
    public bool CanPawn(Item item) => !Is(Characters.Isolde) && !item.IsSong && Display.Contains(item);

    // ---- The Guild's terms ----

    /// <summary>A seller who'll put the price up if you drop their chest (everyone, with fussy sellers about).</summary>
    public bool MindsDrop(SellerDef seller) => (seller.HatesDrop || Under(Challenges.FussySellers)) && !KidGloves;

    public bool MindsMagnet(SellerDef seller) => seller.HatesMagnet && !KidGloves;

    public bool MindsHardShake(SellerDef seller) => seller.HatesHardShake && !KidGloves;

    /// <summary>
    /// How likely this seller's chest is to have a false bottom at this shop: their own habit, and what the shop's name
    /// and curios add (always, with the smugglers' compass).
    /// </summary>
    public float HiddenChance(SellerDef seller)
    {
        if (Tier < 2) return 0;
        var boosts = Boosts;
        return boosts.AlwaysHidden ? 1 : Math.Min(1, (seller.HiddenChance + boosts.Hidden) * boosts.HiddenFactor);
    }

    /// <summary>
    /// How many seconds of shaking a rotten chest stands (0: as long as you like): twice as long with a pocket watch, and
    /// in a slow season.
    /// </summary>
    public float ShakeLimit(SellerDef seller) => seller.ShakeLimit * (HasCurio("watch") ? Curios.WatchPatience : 1) *
                                                 (Favoured(Challenges.Patient) ? Challenges.PatientFactor : 1);

    /// <summary>A seller who won't wait all day (everyone, when the town's in a hurry).</summary>
    public bool Impatient(SellerDef seller) => Patience(seller) > 0;

    /// <summary>
    /// A seller's terms, as Bram would put them under the chest: their rule, and anything they won't stand for (with the
    /// Guild's terms and kid gloves taken into account). Null if they have none.
    /// </summary>
    public string Terms(Visit visit)
    {
        var seller = visit.Seller;
        var terms = new List<string>();
        if (seller.ShakeLimit > 0) terms.Add($"Rotten: it stands {ShakeLimit(seller):0} seconds of shaking, all told.");
        if (visit.Chest.Packed) terms.Add("Packed in straw: everything needs a strength harder to stir.");
        if (seller.Packhorse) terms.Add("On a skittish mare: no hard shake.");
        else if (EventIs("mourning") && CanShakeHard) terms.Add("A day of mourning: no hard shake.");
        if (seller.ShakeFee > 0) terms.Add($"Each strength you try after the first puts the price up {Synergies.Pct(seller.ShakeFee)}.");
        if (seller.Wager > 0) terms.Add("Read it exactly right and he gives back a third of the price.");
        if (seller.Heirloom is Category heirloom) terms.Add($"The best {Categories.Get(heirloom).Name.ToLowerInvariant()} inside goes back to him, for {visit.Fee}g.");
        if (seller.TakesDearest) terms.Add("The dearest thing inside is his.");
        if (seller.SlipPrice) terms.Add("His price is your slip: exactly right, a quarter off; wrong, what it holds and a quarter more.");
        if (seller.Gaze > 0) terms.Add($"Buy, and everyone after him today asks {(seller.Gaze == 2 ? "twice as much" : $"x{seller.Gaze:0.#}")}.");
        var minds = new List<string>();
        if (MindsHardShake(seller)) minds.Add("a hard shake");
        if (MindsDrop(seller)) minds.Add("a drop");
        if (MindsMagnet(seller)) minds.Add("the lodestone");
        if (minds.Count > 0) terms.Add($"Minds {Sellers.Join(minds)}: half again if you do.");
        return terms.Count == 0 ? null : string.Join(" ", terms);
    }

    /// <summary>
    /// What an upgrade costs this run: its price (a quarter more from the Guild's builders; the move to Thornwick half,
    /// when Thornwick wants you), and the Bookkeeping Engine whatever is left of the debt.
    /// </summary>
    public int Cost(Upgrade upgrade) => upgrade.Key == Upgrades.Engine ? DebtLeft
        : Money.Round(upgrade.Cost * (Under(Challenges.Builders) ? Challenges.BuilderPrices : 1) *
                      (upgrade.SetsTier == 2 && Favoured(Challenges.Invited) ? 0.5f : 1));

    /// <summary>
    /// The Guild offers its Bookkeeping Engine once you could pay off everything you owe: it would rather be paid twice.
    /// </summary>
    public bool EngineOffered => CanPayOff && !Has(Upgrades.Engine);

    /// <summary>A curio that would do nothing for this character on these terms: Nim doesn't bother bringing it.</summary>
    public bool NoUse(CurioDef curio) => curio.Key switch
    {
        "crate" => !CanDrop,
        "book" => Is(Characters.Isolde),
        "monocle" => Has("lamp") || Shows(Heartsongs.Star),
        // What a boon already does as well.
        "ticket" => Favoured(Challenges.HonestClerk),
        "shell" => Favoured(Challenges.Charm),
        _ => false,
    };

    // ---- Nim's visits, and the Guild's own paper ----

    /// <summary>The Guild Promissory Note (a curio) has paid a short night: it's spent.</summary>
    public bool NoteUsed { get; private set; }

    /// <summary>The note would pay what's short tonight: nothing you own can cover it, and it's not the last week.</summary>
    public bool CanCallNote => HasCurio("note") && !NoteUsed && DuesTonight && !PaidThisWeek && !CanCover && Week < Guild.Weeks;

    /// <summary>The note pays what's short, and the due is paid. Returns what it paid.</summary>
    public int CallNote()
    {
        if (!CanCallNote) return 0;
        int paid = DueNow - Gold;
        NoteUsed = true;
        Gold += paid;
        PayDues();
        return paid;
    }

    /// <summary>How many times Nim has shown you something else on this visit.</summary>
    public int PeddlerRerolls { get; private set; }

    /// <summary>How many times a visit Nim will show you something else: once, or twice when he has a soft spot for you.</summary>
    public int RerollsPerVisit => Favoured(Challenges.NimsFavour) ? 2 : 1;

    /// <summary>Nim will pack up what's on the counter and show you something else.</summary>
    public bool CanRerollPeddler => PeddlerToday && PeddlerRerolls < RerollsPerVisit;

    /// <summary>Nim packs up what's on the counter and lays out something else.</summary>
    public void RerollPeddler()
    {
        if (!CanRerollPeddler) return;
        PeddlerRerolls++;
        var shown = PeddlerStock.Select(c => c.Key).ToHashSet();
        PeddlerStock.Clear();
        PeddlerStock.AddRange(Curios.Stock(new Random(unchecked(Seed * 6151 + Day * 31 + 7 * PeddlerRerolls)), this, PeddlerCount, shown));
        CuriosSeen.UnionWith(PeddlerStock.Select(c => c.Key));
    }

    /// <summary>How many curios Nim lays out (one fewer in a lean month).</summary>
    public int PeddlerCount => Curios.Laid - (Under(Challenges.Sparse) ? 1 : 0);
}
