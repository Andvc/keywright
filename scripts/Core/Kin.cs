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
    /// <remarks>Once the Unlocking has cracked the Gate (a tale), the Maw's gilded iron is about as in a season of forgers.</remarks>
    public bool Under(string bane) => Banes.Contains(bane) || (bane == Challenges.Forgers && Flagged(TaleBook.GateCracked));

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
        // How Bram's named in tips and scenes: Kit's uncle, everyone else's partner.
        Hints.Kin = state.Character;
        var (boons, banes) = terms ?? Challenges.Draw(seed, state.Challenge);
        state.Boons.AddRange(boons);
        state.Banes.AddRange(banes);
        state.Learned(taught, firstDay: true);
        state.Guided = guided;
        state.CuriosOpen = curios?.ToHashSet();
        state.Gold = StartGold(state);
        state.StartCampaign();
        // Bram's old picks: the fine lockpicks from the first day.
        if (state.Favoured(Challenges.OldPicks)) state._owned.Add("picks");
        return state;
    }

    static int StartGold(GameState state) => state.Under(Challenges.LeanStart) ? StartingGold / 2
        : state.Favoured(Challenges.Savings) ? Money.Round(StartingGold * Challenges.SavingsFactor) : StartingGold;

    // ---- Hesk: a bad back and an honest scale ----

    /// <summary>Hesk's back won't let him shake a chest hard.</summary>
    public bool CanShakeHard => !Is(Characters.Hesk);

    /// <summary>Nor will Rook's mare, for anyone.</summary>
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
        else if (HagglesAll && visit.OffersMade > 0 && !Keeps(Core.Keepsakes.StallKey)) _snubbed.Add(visit.Seller.Key);
    }

    // ---- Isolde: a collection that pays its way ----

    /// <summary>What Isolde's visitors pay tonight: so much a point of exhibition.</summary>
    public int Admission => (Is(Characters.Isolde) ? (int)(Exhibition * Characters.DoorCoins * (Keeps(Core.Keepsakes.Seal) ? Core.Keepsakes.SealDoor : 1f) / Characters.DoorPoints) : 0) +
                            (Shows(Heartsongs.Hearth) ? Exhibition * Heartsongs.HearthPerPoint * ShopTiers.Scale(Tier) : 0);

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
        if (Ways < Market.CraftTier) return 0;
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
        if (seller.ShakeLimit > 0) terms.Add(Loc.T($"Rotten: it stands {ShakeLimit(seller):0} seconds of shaking, all told."));
        if (Impatient(seller)) terms.Add(Loc.T($"In a hurry: {Patience(seller):0} seconds to decide, counting down beside them."));
        if (visit.Chest.Packed) terms.Add(Loc.T("Packed in straw: everything needs a strength harder to stir."));
        if (seller.Packhorse) terms.Add(Loc.T("On a skittish mare: no hard shake."));
        else if (EventIs("mourning") && CanShakeHard) terms.Add(Loc.T("A day of mourning: no hard shake."));
        if (visit.ShakeFee > 0) terms.Add(Loc.T($"Each strength you try after the first puts the price up {Synergies.Pct(visit.ShakeFee)}."));
        if (seller.Wager > 0) terms.Add(Loc.T("Read it exactly right and she gives back a third of the price."));
        if (visit.Contest) terms.Add(visit.ContestFor ?? Loc.T($"A contest: read it exactly right, and you win {visit.Prize}g."));
        if (visit.Heirloom is Category heirloom) terms.Add(Loc.T($"The best {Categories.Get(heirloom).Name.ToLowerInvariant()} inside goes back to him, for {visit.Fee}g."));
        if (seller.TakesDearest) terms.Add(Loc.T("The dearest thing inside is his."));
        if (seller.SlipPrice) terms.Add(Loc.T("His price is your slip: exactly right, a quarter off; wrong, what it holds and a quarter more."));
        if (seller.Gaze > 0) terms.Add(Loc.T($"Buy, and everyone after him today asks {(seller.Gaze == 2 ? Loc.T("twice as much") : $"x{seller.Gaze:0.#}")}."));
        var minds = new List<string>();
        if (MindsHardShake(seller)) minds.Add(Loc.T("a hard shake"));
        if (MindsDrop(seller)) minds.Add(Loc.T("a drop"));
        if (MindsMagnet(seller)) minds.Add(Loc.T("the lodestone"));
        if (minds.Count > 0) terms.Add(Loc.T($"Minds {Sellers.Join(minds)}: half again if you do."));
        // A friend's terms: the rule they'd have kept, waived (Nettle's fee, Sir Rowan's claim).
        if (visit.Waived && Goodwill(seller).Note is string waived) terms.Add(waived);
        return terms.Count == 0 ? null : string.Join(" ", terms);
    }

    /// <summary>
    /// What an upgrade costs this run: its price (a quarter more from the Guild's builders), and the Bookkeeping Engine
    /// whatever is left of the debt.
    /// </summary>
    public int Cost(Upgrade upgrade) => upgrade.Key == Upgrades.Engine ? DebtLeft
        : Money.Round(upgrade.Cost * (Under(Challenges.Builders) ? Challenges.BuilderPrices : 1));

    /// <summary>
    /// The Guild offers its Bookkeeping Engine once you could pay off everything you owe: it would rather be paid twice.
    /// </summary>
    public bool EngineOffered => CanPayOff && !Has(Upgrades.Engine);

    /// <summary>A curio that would do nothing for this character on these terms: Nim doesn't bother bringing it.</summary>
    public bool NoUse(CurioDef curio) => curio.Key switch
    {
        "crate" => !CanDrop,
        "monocle" => Has("lamp") || Shows(Heartsongs.Star),
        // Isolde never pawns her collection (0.35: Nim offered her a pawn ticket; and the visitors' book, which pays her
        // as it does anyone, he kept from her).
        "ticket" => Is(Characters.Isolde) || Favoured(Challenges.HonestClerk),
        // What a boon already does as well.
        "shell" => Favoured(Challenges.Charm),
        _ => false,
    };

    // ---- Nim's visits, and the Guild's own paper ----

    /// <summary>The Guild Promissory Note (a curio) has paid a short night: it's spent.</summary>
    public bool NoteUsed { get; private set; }

    /// <summary>The note would pay what's short tonight: nothing you own can cover it, and it's not the last week.</summary>
    public bool CanCallNote => HasCurio("note") && !NoteUsed && DuesTonight && !PaidThisWeek && !CanCover && Week != Guild.Weeks;

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

    /// <summary>
    /// How many curios Nim lays out tonight: one, and one more for each of his upgrades (his lantern, his stool, his
    /// standing order) and for a soft spot; one fewer in a lean month, but never none.
    /// </summary>
    public int PeddlerCount => Math.Max(1, Curios.Laid + Upgrades.All.Where(u => Has(u.Key)).Sum(u => u.NimCards) + (Favoured(Challenges.NimsFavour) ? 1 : 0) -
                                           (Under(Challenges.Sparse) ? 1 : 0));
}
