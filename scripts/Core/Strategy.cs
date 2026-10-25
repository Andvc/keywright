using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// How a careful player plays: when to drop a chest or use the magnet, what to offer, what to keep for display. The
/// balance simulation and the autoplay bot share it, so the bot plays the way the numbers in the design notes assume.
/// </summary>
public static class Strategy
{
    static readonly Shake[] Every = { Shake.Gentle, Shake.Firm, Shake.Hard };

    /// <summary>
    /// Which strengths to shake at: all three, except gently and hard when there's no time or every strength after the
    /// first costs (hard hears everything firm does), or gently and firmly when the seller won't have it shaken hard (or
    /// Hesk's back, or Wenna's mare, won't). A rotten chest stands one strength: hard, unless what's in their chests is
    /// mostly delicate (a clank would drown it out). Through straw a gentle shake stirs nothing: firmly, then hard. Kid
    /// gloves and a pocket watch lift the sellers' limits.
    /// </summary>
    public static Shake[] Levels(Visit visit, GameState state = null)
    {
        bool hard = (state?.CanShakeHardAt(visit.Seller) ?? !visit.Seller.Packhorse) && !(state?.MindsHardShake(visit.Seller) ?? visit.Seller.HatesHardShake);
        // Through an ear trumpet nothing is drowned out or muffled: a hard shake hears it all.
        if (visit.Chest.Unmasked && hard) return new[] { Shake.Hard };
        // A rotten chest stands one strength (two, with a pocket watch's patience).
        float limit = state?.ShakeLimit(visit.Seller) ?? visit.Seller.ShakeLimit;
        if (limit > 0 && limit < 2 * visit.Seller.ShakeLimit) return new[] { DelicateShare(visit) > 0.5f ? Shake.Gentle : hard ? Shake.Hard : Shake.Firm };
        if (limit > 0) return hard ? new[] { Shake.Gentle, Shake.Hard } : new[] { Shake.Gentle, Shake.Firm };
        if (visit.Chest.Packed && !visit.Chest.Unmasked) return hard ? new[] { Shake.Firm, Shake.Hard } : new[] { Shake.Firm };
        if (!hard) return new[] { Shake.Gentle, Shake.Firm };
        return Rushed(visit, state) || visit.Seller.ShakeFee > 0 ? new[] { Shake.Gentle, Shake.Hard } : Every;
    }

    /// <summary>The gentlest shake that stirs this kind in this chest (straw muffles a strength; 4: never).</summary>
    static int Needs(Visit visit, Category kind) => (int)Categories.Get(kind).StirsAt + (visit.Chest.Packed && !visit.Chest.Unmasked ? 1 : 0);

    /// <summary>Whether the shaking so far could have stirred this kind (it might still be drowned out).</summary>
    static bool CouldStir(Visit visit, Hearing hearing, Category kind) => Every.Any(l => hearing.HasTried(l, false) && (int)l >= Needs(visit, kind));

    /// <summary>
    /// Whether a clank may have drowned out something delicate: weapons sounded, and no strength was tried that stirs
    /// delicate things but not weapons (gently, or firmly through straw), nor with the magnet holding the iron still.
    /// </summary>
    static bool MaybeDrowned(Visit visit, Hearing hearing) => !visit.Chest.Unmasked && hearing.HeardAny(Category.Arms) &&
        !Every.Any(l => (int)l >= Needs(visit, Category.Coins) && (hearing.HasTried(l, true) || (hearing.HasTried(l, false) && (int)l < Needs(visit, Category.Arms))));

    /// <summary>Every kind at this shop could have been stirred, and nothing delicate drowned out: what sounded is all there is, but a false bottom.</summary>
    static bool HeardAll(Visit visit, Hearing hearing, int tier) =>
        Categories.UpTo(tier).All(k => CouldStir(visit, hearing, k.Category)) && !MaybeDrowned(visit, hearing);

    /// <summary>What a careful player expects in this chest: what the place it came from holds, or what the seller usually brings.</summary>
    static float[] Likely(Visit visit) => visit.Origin?.Kinds ?? visit.Seller.Kinds;

    /// <summary>The kind a chest from this place (or this seller) most often holds, at this shop.</summary>
    public static Category? MostLikely(Visit visit, int tier)
    {
        var likely = Likely(visit);
        return Categories.UpTo(tier).OrderByDescending(k => likely[(int)k.Category]).Select(k => (Category?)k.Category).FirstOrDefault();
    }

    /// <summary>How much of what's likely in this chest is delicate (coin, potions, gems), at this seller's shop.</summary>
    static float DelicateShare(Visit visit)
    {
        var likely = Likely(visit);
        var kinds = Categories.UpTo(visit.Seller.Tier).ToList();
        return kinds.Where(k => k.Delicate).Sum(k => likely[(int)k.Category]) / Math.Max(0.01f, kinds.Sum(k => likely[(int)k.Category]));
    }

    /// <summary>An impatient seller, and no pocket watch to make the time stretch.</summary>
    static bool Rushed(Visit visit, GameState state) => state == null ? visit.Seller.Patience > 0 : state.Impatient(visit.Seller) && !state.HasCurio("watch");

    /// <summary>
    /// Drop it to listen for a false bottom, unless there's glass in there (and no padded crate) or the seller would
    /// mind (or it's Hesk, who can't lift it).
    /// </summary>
    public static bool ShouldDrop(Visit visit, Hearing hearing, int tier, GameState state = null) =>
        tier >= 2 && (state?.CanDrop ?? true) && !(state?.MindsDrop(visit.Seller) ?? visit.Seller.HatesDrop) && !Rushed(visit, state) && !hearing.Dropped &&
        (state?.DropSturdier >= 1 || !Categories.All.Any(c => c.Fragile && hearing.HeardAny(c.Category)));

    /// <summary>
    /// Where to check with the magnet: each strength at which a jingle or tinkle was heard (a gilded fake goes quiet
    /// there), unless the seller would mind or there's no time.
    /// </summary>
    public static Shake[] MagnetLevels(Visit visit, Hearing hearing, GameState state)
    {
        if (!state.Has("magnet") || state.MindsMagnet(visit.Seller) || Rushed(visit, state)) return Array.Empty<Shake>();
        var gilded = Categories.All.Where(c => c.FakeName != null && c.FakeIron).Select(c => c.Category).ToArray();
        // The gentlest strength each one sounded at: where a fake would go quiet.
        return gilded.Select(c => Every.FirstOrDefault(l => hearing.HeardAt(c, l, false))).Where(l => l != Shake.Still)
            .Distinct().OrderBy(l => l).ToArray();
    }

    public static bool ShouldUseMagnet(Visit visit, Hearing hearing, GameState state) => MagnetLevels(visit, hearing, state).Length > 0;

    /// <summary>
    /// What a careful player ticks when they couldn't shake hard enough to hear it: on faith, a soft kind the seller
    /// usually brings or the place holds (Brother Ansel's crypt vestments). Their habit is in the ledger.
    /// </summary>
    public static void OnFaith(Visit visit, Appraisal slip, Hearing hearing, GameState state = null)
    {
        if (Weighed(visit, state)) return;
        var likely = Likely(visit);
        float total = likely.Sum();
        foreach (var kind in Categories.UpTo(slip.Tier).Where(k => k.StirsAt == Shake.Hard && slip.Count(k.Category) == 0 && !CouldStir(visit, hearing, k.Category)))
            if (likely[(int)kind.Category] / total >= FaithShare) slip.Set(kind.Category, 1);
    }

    /// <summary>The Bookkeeping Engine read this chest (it can't through chains).</summary>
    public static bool Recorded(Visit visit, GameState state) => (state?.Has(Upgrades.Engine) ?? false) && !visit.Chest.Sealed;

    /// <summary>Someone counted what's inside, false bottom and all: Pell's ticket, or the Drowned Bell.</summary>
    static bool Counted(Visit visit, GameState state) => visit.Says is Clue.Articles or Clue.Tally || (state?.Shows(Heartsongs.Bell) ?? false);

    /// <summary>A scale has accounted for what couldn't be heard: Hesk's, or Wenna's (she says what hers read).</summary>
    static bool Weighed(Visit visit, GameState state) => (state?.Weighs ?? false) || visit.Says == Clue.Pounds;

    /// <summary>
    /// A careful read of what the seller said besides what they heard. A weight (Wenna's game scale, or Hesk's) accounts
    /// for what couldn't be heard; Pell's ticket counts every article, so once everything has been heard, one more is
    /// under a false bottom; a clue that points at a warded thing (<see cref="WardedKind"/>) gets it named: one more of
    /// its kind on the slip.
    /// </summary>
    public static void Reckon(Appraisal slip, Hearing hearing, Visit visit, GameState state = null)
    {
        if (Weighed(visit, state)) Weigh(slip, hearing, visit, state);
        else if (Counted(visit, state) && HeardAll(visit, hearing, slip.Tier))
            slip.Hidden = Market.ArticleCount(visit.Chest) > slip.Marked.Where(k => k != Category.Heartsong).Sum(slip.Count) + hearing.Spotted().Count();
        if (WardedKind(visit, slip, hearing) is Category warded) slip.Set(warded, slip.Count(warded) + 1, slip.Magic(warded));
        // The Imp's Ember burns every fake true: what was caught out counts after all.
        if (state?.Shows(Heartsongs.Ember) ?? false)
            foreach (var kind in hearing.Spotted().GroupBy(k => k))
                slip.Set(kind.Key, slip.Count(kind.Key) + kind.Count(), slip.Magic(kind.Key));
    }

    /// <summary>
    /// What a careful player works out is warded, from the seller's clue and what they heard, or null:
    /// Mother Bramble's sign, or grandmother's riddle, names a kind there's nothing of to hear; a kind on the Arcanum's
    /// tag, or in a Thornwick dowry, never sounded; the faded ward-roll names it; a suit came up twice in Madame Oriel's
    /// cards; Skrae's echoes come back off one more of a kind than sounded. The ones that turn on hearing wait until
    /// that kind has had a fair chance to sound.
    /// </summary>
    public static Category? WardedKind(Visit visit, Appraisal slip, Hearing hearing)
    {
        var kinds = visit.ClueKinds;
        bool Silent(Category kind) => slip.Count(kind) == 0 && FairChance(visit, hearing, kind);
        return visit.Says switch
        {
            Clue.Dream or Clue.Riddle when kinds.Length == 1 && Silent(kinds[0]) => kinds[0],
            Clue.Tag or Clue.Dowry => kinds.Where(Silent).Select(k => (Category?)k).FirstOrDefault(),
            Clue.Roll when kinds.Length == 1 => kinds[0],
            Clue.Cards => kinds.GroupBy(k => k).Where(g => g.Count() > 1).Select(g => (Category?)g.Key).FirstOrDefault(),
            Clue.Echo => kinds.GroupBy(k => k).Where(g => g.Count() > slip.Count(g.Key) && FairChance(visit, hearing, g.Key))
                .Select(g => (Category?)g.Key).FirstOrDefault(),
            _ => null,
        };
    }

    /// <summary>
    /// Whether a kind has had a fair chance to sound: a strength that stirs it, and nothing that could have drowned it
    /// (a delicate thing under a clank). What hasn't, can't be called silent (Hesk, who never shakes hard, can still
    /// tell a warded potion from a dream of a river).
    /// </summary>
    static bool FairChance(Visit visit, Hearing hearing, Category kind) =>
        CouldStir(visit, hearing, kind) && !(Categories.Get(kind).Delicate && MaybeDrowned(visit, hearing));

    /// <summary>How much of what a seller brings has to be one kind before a careful player takes it on faith.</summary>
    const float FaithShare = 0.35f;

    /// <summary>
    /// Hesk's read of his scale: once everything he heard is accounted for (and any fake he caught out), the weight left
    /// over is what he couldn't hear (cloth and paper, which only a hard shake stirs, and whatever is under a false
    /// bottom), less what any gilded fake he didn't catch weighs short. He puts down the likeliest explanation, given
    /// what the seller usually brings.
    /// </summary>
    public static void Weigh(Appraisal slip, Hearing hearing, Visit visit, GameState state)
    {
        var seller = visit.Seller;
        int tier = slip.Tier;
        float known = Categories.UpTo(tier).Sum(k => slip.Count(k.Category) * Characters.Pounds(k.Category)) + hearing.Spotted().Sum(Characters.FakePounds) +
                      slip.Count(Category.Heartsong) * Characters.Pounds(Category.Heartsong);
        float left = visit.Chest.Pounds - known;
        var likely = Likely(visit);
        float total = likely.Sum();
        float Unheard(Category kind) => Categories.Get(kind).Tier <= tier ? UnheardOdds * likely[(int)kind] / total : 0;

        // Under a false bottom: nothing, or one of the things people hide (a drop, if there was one, settles which).
        var hidden = new List<(Category? Kind, float Chance)>();
        float hiddenChance = tier >= 2 ? seller.HiddenChance + (state?.Boosts.Hidden ?? 0) : 0;
        if (!hearing.Hollow) hidden.Add((null, hearing.Dropped ? 1 : 1 - hiddenChance));
        if (tier >= 2 && (!hearing.Dropped || hearing.Hollow))
            foreach (var (kind, chance) in Market.HiddenOdds(tier)) hidden.Add((kind, chance * (hearing.Hollow ? 1 : hiddenChance)));

        // Gilded fakes he heard but didn't catch with the magnet: each weighs half what it should.
        bool magnet = Every.Any(l => hearing.HasTried(l, true));
        float Fakes(Category kind) => magnet ? 0 : (seller.Fakes.Contains(kind) ? seller.FakeChance : 0) +
                                                   (kind == Category.Coins ? state?.Boosts.Slugs ?? 0 : 0);
        int coins = slip.Count(Category.Coins), jewels = slip.Count(Category.Gems);
        float Binomial(int n, int k, float p) => k > n || (p <= 0 && k > 0) ? 0 : Choose(n, k) * MathF.Pow(p, k) * MathF.Pow(1 - p, n - k);

        (int Cloth, int Paper, Category? Hidden, int Slugs, int Paste) best = (0, 0, null, 0, 0);
        float bestChance = 0;
        int room = Market.MaxItems - slip.Marked.Sum(slip.Count);
        for (int cloth = 0; cloth <= room; cloth++)
        for (int paper = 0; cloth + paper <= room; paper++)
        foreach (var (kind, hiddenOdds) in hidden)
        for (int slugs = 0; slugs <= coins; slugs++)
        for (int paste = 0; paste <= jewels; paste++)
        {
            float weight = cloth * Characters.Pounds(Category.Garb) + paper * Characters.Pounds(Category.Scrolls) +
                           (kind is Category k ? Characters.Pounds(k) : 0) - slugs * Characters.FakePounds(Category.Coins) -
                           paste * Characters.FakePounds(Category.Gems);
            if (MathF.Abs(weight - left) > 0.01f) continue;
            float chance = MathF.Pow(Unheard(Category.Garb), cloth) * MathF.Pow(Unheard(Category.Scrolls), paper) * hiddenOdds *
                           Binomial(coins, slugs, Fakes(Category.Coins)) * Binomial(jewels, paste, Fakes(Category.Gems));
            if (chance <= bestChance) continue;
            bestChance = chance;
            best = (cloth, paper, kind, slugs, paste);
        }
        if (bestChance <= 0) return;
        slip.Set(Category.Garb, best.Cloth);
        if (tier >= 2) slip.Set(Category.Scrolls, best.Paper);
        slip.Hidden = best.Hidden != null;
        slip.Set(Category.Coins, coins - best.Slugs, slip.Magic(Category.Coins));
        if (jewels > 0) slip.Set(Category.Gems, jewels - best.Paste, slip.Magic(Category.Gems));
    }

    /// <summary>For Hesk's read: how likely one more thing he couldn't hear is, before the seller's habits.</summary>
    const float UnheardOdds = 0.4f;

    static float Choose(int n, int k)
    {
        float c = 1;
        for (int i = 0; i < k; i++) c = c * (n - i) / (i + 1);
        return c;
    }

    /// <summary>
    /// A haggler's first answer: about half their asking price, never more than the chest is worth to you. When they ask
    /// you to name a price: half what it's worth to you, or just a few coins to someone who has no idea.
    /// </summary>
    /// <param name="floor">How much further than usual sellers come down for this shop (a silver tongue, a Haggler's name: <see cref="Boosts.Floor"/>).</param>
    public static int FirstOffer(Visit visit, int worth, float floor = 1) => visit.Trade switch
    {
        // Nobody walks out on a golden tongue: a coin, and they come down to the least they'd take.
        _ when visit.Unruffled => 1,
        // A haggler who'll hear you out (a merchant's signet) can be tried with a cheeky offer first.
        Trade.Haggle when visit.HearsOut && visit.OffersMade == 0 => Money.Round(Math.Min(visit.Ask * 0.3f, worth * 0.8f) * floor),
        Trade.Haggle => Money.Round(Math.Min(visit.Ask * 0.55f, worth * 0.8f) * floor),
        _ when visit.Seller.Judge == Judgment.Clueless => Money.Round(Math.Min(worth * 0.5f, visit.Seller.Tier * 4) * floor),
        _ => Money.Round(Math.Max(1, worth * 0.5f * floor)),
    };

    /// <summary>
    /// What the chest is worth to a careful player who knows the seller: what the odds bar says it's more likely than not
    /// worth (they buy when the bar says it's better than even), plus a fair guess at what couldn't be checked (soft
    /// things in a chest they couldn't shake hard, a false bottom in a chest they couldn't drop), and a bit more from a
    /// seller whose things run better than their colour. With the shop's state: at what things sell for there (curios,
    /// synergies, today's event), and with a fair share of any commission the chest could fill.
    /// </summary>
    public static int Worth(Visit visit, Appraisal slip, Hearing hearing, GameState state = null)
    {
        var seller = visit.Seller;
        if (state != null) slip.Model ??= state;
        float worth = slip.LikelyWorth() + (state == null ? 0 : CommissionWorth(state, slip));
        // A scale (Hesk's, or Wenna's word for hers) has already accounted for what couldn't be heard, and the Bookkeeping
        // Engine for everything.
        bool weighed = Weighed(visit, state) || Recorded(visit, state);
        var likely = Likely(visit);
        float total = likely.Sum();
        // Things nobody shook hard enough to stir (or that straw keeps still), and delicate things a clank may have drowned
        // out (a rotten chest shaken only hard): a fair guess, from what the seller usually brings.
        bool drowned = !Recorded(visit, state) && MaybeDrowned(visit, hearing);
        foreach (var kind in Categories.UpTo(slip.Tier).Where(k => slip.Count(k.Category) == 0))
            if ((!CouldStir(visit, hearing, kind.Category) && !weighed) || (kind.Delicate && drowned))
                worth += likely[(int)kind.Category] / total * 2.5f * (state?.Expected(kind.Category, slip.Chest) ?? Categories.Expected(kind.Category, slip.Chest)) * 0.8f;
        // Pell's ticket settles a false bottom once everything has been heard; so does a scale.
        bool counted = Counted(visit, state) && HeardAll(visit, hearing, slip.Tier);
        if (!hearing.Dropped && !slip.Hidden && slip.Tier >= 2 && !weighed && !counted)
            worth += (state?.HiddenChance(seller) ?? seller.HiddenChance) * slip.HiddenValue;
        // A fence's mark sells a fake for what the real thing would fetch: what was caught out is worth having after all.
        if (state?.HasCurio("mark") ?? false) worth += hearing.Spotted().Sum(k => Categories.Expected(k, slip.Chest) * state.SaleMultiplier(k));
        worth *= 1 + seller.QualityBoost * 1.2f;
        // A knight's claim on the family arms: the best of them goes for his fee. A devil's: the dearest thing, for nothing.
        if (seller.Heirloom is Category heirloom && slip.Count(heirloom) > 0)
            worth -= Math.Max(0, slip.OneValue(heirloom, slip.Magic(heirloom)) - visit.Fee);
        if (seller.TakesDearest)
            worth -= slip.Marked.Select(k => (float)slip.OneValue(k, slip.Magic(k))).Append(slip.Hidden ? slip.HiddenValue : 0).DefaultIfEmpty(0).Max();
        // A Heartsong's worth nothing in gold, but singing in the shop it's worth more than any chest: a careful player
        // pays up for one (what two of the shop's chests would cost).
        if (state != null && slip.Count(Category.Heartsong) > 0) worth += state.WorkingPurse;
        return (int)worth;
    }

    /// <summary>How often a careful player's slip turns out exactly right: for a wager, or the King's game.</summary>
    const float PerfectOdds = 0.7f;

    /// <summary>
    /// Whether to pay the price on the table, given what the chest is worth to you and the seller's terms: an archmage's
    /// wager makes it cheaper for a careful reader; the King's game is worth playing only on a read you could check every
    /// way (every strength, and the drop: a wrong read costs a quarter more than the chest held); the Maw's single coin
    /// is dear if a busy day is still waiting, all asking twice as much.
    /// </summary>
    public static bool WorthBuying(Visit visit, int worth, GameState state = null, Hearing hearing = null)
    {
        var seller = visit.Seller;
        if (seller.SlipPrice) return hearing == null || Recorded(visit, state) || (hearing.HasTried(Shake.Hard, false) && hearing.Dropped);
        if (seller.Gaze > 0) return worth - visit.Ask > (state?.Queue.Sum(v => v.Price) ?? 0) * (seller.Gaze - 1) * GazeBuys;
        return worth > visit.Ask * (1 - seller.Wager * PerfectOdds);
    }

    /// <summary>For the Maw's gaze: how much of what's still waiting a careful player would have bought anyway.</summary>
    const float GazeBuys = 0.4f;

    /// <summary>When they want more than your first offer: most of what it's worth to you.</summary>
    public static int SecondOffer(int worth, float floor = 1) => Money.Round(Math.Max(1, worth * 0.85f * floor));

    /// <summary>
    /// What the commissions add to a chest: for each one the slip's kinds could fill, the chance one of them is good
    /// enough, times what the commission pays over its sale price. Half of it: it might go to another chest first.
    /// </summary>
    public static float CommissionWorth(GameState state, Appraisal slip)
    {
        float extra = 0;
        foreach (var c in state.Commissions.Where(c => !c.Done && c.Kind != CommissionKind.Enchanted))
        {
            int n = slip.Count(c.Category);
            if (n == 0) continue;
            float p = Categories.Odds(slip.Chest).Where(o => o.Quality >= c.Quality - state.LetterLeeway(c)).Sum(o => o.Chance);
            float hit = 1 - MathF.Pow(1 - p, Math.Min(n, c.Needed - c.Delivered));
            float sale = Categories.Nominal(c.Category, c.Quality) * state.SaleMultiplier(c.Category);
            float each = Math.Max(state.CommissionEach(c), sale * Patrons.Premium) - sale;
            extra += hit * Math.Max(0, each) * (c.Story ? 1f : 0.5f);
        }
        return extra;
    }

    /// <summary>Hand it over if a commission wants it: it always pays more than selling it would.</summary>
    public static Commission ShouldDeliver(GameState state, Item item, int saleValue) => state.WantedBy(item);

    /// <summary>What a careful player's shop of each tier clears in a bad day's trading (about half a usual one).</summary>
    static readonly int[] BadDay = { 0, 20, 70, 500, 7000 };

    /// <summary>
    /// Gold to keep back: the working purse (two of the shop's typical chests at what sellers ask here, Bram's advice and
    /// the line the shop warns before crossing: <see cref="GameState.WorkingPurse"/>), as it would be after the upgrade
    /// or curio being considered (at the bigger shop, for a move inward), and whatever of the next due a run of bad days
    /// before the Guild comes wouldn't cover (the first day back is spent building up again, so it doesn't count).
    /// </summary>
    /// <remarks>
    /// A curio is bought to earn: against it, only what usual days (twice a bad one) wouldn't cover is kept back for the
    /// Guild, or no build would ever come together in the weeks the dues are steepest.
    /// </remarks>
    public static int Reserve(GameState state, Upgrade upgrade = null, CurioDef curio = null)
    {
        int tier = Math.Max(upgrade?.SetsTier ?? 0, state.Tier);
        int day = BadDay[tier] * (curio != null ? 2 : 1);
        return state.ReserveIf(upgrade, curio) + Math.Max(0, state.NextDue - Math.Max(0, state.DaysToNextDue - 1) * day);
    }

    /// <summary>
    /// Whether to buy an upgrade now: a renovation is judged by what the bigger shop will make before the Guild comes;
    /// anything big has to leave the due after next as well (it pays back over weeks, not days).
    /// </summary>
    public static bool Affordable(GameState state, Upgrade upgrade)
    {
        int reserve = Reserve(state, upgrade) + (upgrade.IsRenovation ? 0 : Saving(state)), cost = state.Cost(upgrade);
        // A move inward, when the shop as it is couldn't make the next due before the Guild comes: the move is the way to
        // pay it, so only the bigger shop's working purse is kept back.
        if (upgrade.IsRenovation && !CouldStay(state)) reserve = state.ReserveIf(upgrade);
        if (!upgrade.IsRenovation && cost >= 1000) reserve += state.DueFor(state.Week + (state.PaidThisWeek ? 2 : 1));
        return state.CanBuy(upgrade) && state.Gold - cost >= reserve;
    }

    /// <summary>Whether the shop as it is could make the next due before the Guild comes (a usual day being twice a bad one).</summary>
    static bool CouldStay(GameState state) => state.Gold + state.DaysToNextDue * 2 * BadDay[state.Tier] >= state.NextDue;

    /// <summary>
    /// What a careful player puts by for the next move inward before spending on anything else: each move multiplies
    /// what a customer is worth several times over.
    /// </summary>
    static int Saving(GameState state) => Upgrades.Available(state).FirstOrDefault(u => u.IsRenovation) is Upgrade next ? state.Cost(next) : 0;

    /// <summary>
    /// About what each curio adds to a careful player's run on its own, a fifth of the hundredths it adds to what the run
    /// takes in when Nim brings it (the sim's builds report, `--sim=builds`): where to start. What it's worth to a particular shop is <see cref="CurioScore"/>.
    /// </summary>
    public static readonly Dictionary<string, float> CurioWorth = new()
    {
        ["monocle"] = 2, ["draught"] = 8, ["aether"] = 5, ["chalk"] = 3, ["ring"] = 7, ["staff"] = 9,
        ["sealed"] = 0, ["dip"] = 0, ["bond"] = 0, ["wholesale"] = 0, ["token"] = 4, ["blindfold"] = 0,
        ["signet"] = 2, ["tongue"] = 4, ["knot"] = 13, ["hourglass"] = 12, ["bell"] = 11, ["coffer"] = 9, ["goldentongue"] = 9,
        ["trumpet"] = 4, ["watch"] = 3, ["beads"] = 2, ["seal"] = 10, ["register"] = 10, ["crescent"] = 3, ["fork"] = 16, ["ledger"] = 20,
        ["truthglass"] = 22,
        ["crate"] = 3, ["hook"] = 14, ["rod"] = 16, ["log"] = 12, ["pass"] = 14, ["compass"] = 22,
        ["wardlamp"] = 13, ["pillow"] = 7, ["wardtally"] = 4, ["wardkey"] = 14, ["aldricseal"] = 11,
        ["shell"] = 7, ["coin"] = 3, ["foot"] = 12, ["wishbone"] = 3, ["horseshoe"] = 10, ["clover"] = 6, ["star"] = 18, ["spindle"] = 12,
        ["lantern"] = 14, ["queue"] = 16, ["horn"] = 8, ["guestbook"] = 10,
        ["catalogue"] = 6, ["dome"] = 2, ["book"] = 12, ["rope"] = 2, ["lamps"] = 4,
        ["gauntlet"] = 4, ["cap"] = 4, ["whetstone"] = 4, ["dummy"] = 4, ["scales"] = 6, ["still"] = 4, ["desk"] = 2, ["wheel"] = 8,
        ["oil"] = 2, ["knife"] = 1, ["gloves"] = 3, ["loupe"] = 1, ["mark"] = 2,
        ["purse"] = 1, ["ticket"] = 1, ["marker"] = 1, ["note"] = 2, ["jar"] = 12, ["lockbox"] = 4,
        ["letter"] = 9, ["ribbon"] = 11, ["errand"] = 3, ["net"] = 7,
    };

    /// <summary>A cursed crescent holds everything to its chest's colour: luck is wasted beside it, and it on luck.</summary>
    const float Cursed = 15f;

    /// <summary>
    /// What a careful player sees in a curio that shares a way of trading with one they keep: it's worth more together, by
    /// this share of its own worth for each (a weak curio doesn't become a strong one by having company).
    /// </summary>
    const float Kindred = 0.5f;

    /// <summary>How much a careful player building around one way of trading (the sim's builds) wants curios for it.</summary>
    const float Focused = 12f;

    /// <summary>
    /// What a curio is worth to this shop, now: its own worth, less if the shop can't use it yet (a display curio without
    /// a display, a trade's curio for a kind it doesn't see), more for each curio kept that shares a way of trading with
    /// it (<see cref="Kindred"/>), less beside a cursed crescent if it's luck, and much more for the way of trading a
    /// build is about (<paramref name="focus"/>).
    /// </summary>
    public static float CurioScore(GameState state, CurioDef curio, CurioTag? focus = null, string without = null)
    {
        float score = CurioWorth.GetValueOrDefault(curio.Key, 3);
        bool display = state.DisplayOpen || state.DisplaySlots > 0;
        score *= curio.Key switch
        {
            "book" => state.Exhibition >= 10 ? 1 : 0.2f,
            "rope" or "lamps" or "dome" => display ? 1 : 0.3f,
            "letter" or "ribbon" or "errand" => state.Week >= Patrons.FromWeek - 1 ? 1 : 0.5f,
            "catalogue" => state.Week <= 2 ? 1 : 0.4f,
            _ => 1,
        };
        if (curio.Kind is Category kind && Categories.Get(kind).Tier > state.Tier) score *= 0.3f;
        var kept = state.OwnedCurios.Where(c => c.Key != curio.Key && c.Key != without).ToList();
        score *= 1 + Kindred * kept.Count(c => c.Tags.Intersect(curio.Tags).Any());
        if (curio.Key == "crescent") score -= Cursed * kept.Count(c => c.Has(CurioTag.Luck));
        else if (curio.Has(CurioTag.Luck) && kept.Any(c => c.Key == "crescent")) score -= Cursed;
        // A trade's curio goes with the shop's name for that kind (a smith's, a tailor's...).
        if (curio.Kind is Category trade && state.ActiveSynergies.Any(s => Synergies.All.First(d => d.Key == s).Kinds.Contains(trade))) score *= 1 + Kindred;
        // Building around one way of trading: its curios are wanted far more, and once Nim could bring them, anything
        // else is a stopgap.
        if (focus is CurioTag f && curio.Has(f)) score = score * 1.5f + Focused;
        else if (focus is CurioTag g && Curios.All.Any(c => c.Has(g) && Curios.Reachable(state, c))) score *= 0.5f;
        return score;
    }

    /// <summary>
    /// What to buy from Nim: the curio on offer worth most to this shop (<see cref="CurioScore"/>) that leaves enough for
    /// the Guild, if there's room, or if it's clearly better than the least useful one kept (which goes back to Nim for
    /// half). Returns the curio and the one to sell, or nulls.
    /// </summary>
    public static (CurioDef Buy, string Sell) ChooseCurio(GameState state, IEnumerable<CurioDef> stock, CurioTag? focus = null)
    {
        var wanted = stock.Where(c => !state.HasCurio(c.Key)).Select(c => (Curio: c, Score: CurioScore(state, c, focus)))
            .Where(c => c.Score > 0).OrderByDescending(c => c.Score);
        foreach (var (curio, score) in wanted)
        {
            int price = state.CuriosPrice(curio);
            string sell = null;
            if (state.CuriosFull)
            {
                var (worst, least) = state.OwnedCurios.Select(c => (c.Key, Score: CurioScore(state, c, focus, curio.Key))).OrderBy(c => c.Score).First();
                if (score - least < 4) continue;
                sell = worst;
                price -= state.CurioValue(worst);
            }
            // A curio is priced mostly at a share of the purse, so it never waits on saving for the next move: only on
            // the Guild and the working purse it would leave.
            if (state.Gold - price >= Reserve(state, curio: curio) || price == 0) return (curio, sell);
        }
        return (null, null);
    }

    /// <summary>The curio a careful player would part with first: the least use to the shop.</summary>
    static IEnumerable<string> LeastUseFirst(GameState state) => state.OwnedCurios.OrderBy(c => CurioScore(state, c)).Select(c => c.Key);

    /// <summary>What to pawn when short, cheapest loss first: displayed things, then curios. Null when it's hopeless.</summary>
    public static List<object> PawnOrder(GameState state)
    {
        if (!state.CanCover) return null;
        var order = new List<object>();
        int gold = state.Gold;
        foreach (var item in state.Display.Where(state.CanPawn).OrderBy(i => i.Prestige).ThenBy(state.PawnValue))
        {
            if (gold >= state.DueNow) break;
            order.Add(item);
            gold += state.PawnValue(item);
        }
        foreach (var key in LeastUseFirst(state).ToList())
        {
            if (gold >= state.DueNow) break;
            order.Add(key);
            gold += state.CurioValue(key);
        }
        return order;
    }

    /// <summary>Pay off the whole debt as soon as there's enough: it's what the run is for.</summary>
    public static bool ShouldPayOff(GameState state) => state.CanPayOff;

    /// <summary>The Guild's offer for the map: a careful player takes it only if it's that or the shop.</summary>
    public static bool TakeOffer(GameState state) => state.Gold < state.DueNow && state.Gold >= state.DueNow / 2;

    /// <summary>Most a careful collector gives up, in sale price, for each exhibition point.</summary>
    public const int GoldPerPoint = 20;

    /// <summary>
    /// Keep things that look grand for what they'd sell for (a fine coat shows as well as a diamond and costs a tenth of
    /// it) while there's room; swap out the plainest when a grander one turns up. Stop at the most an exhibition does for
    /// customers and chests, unless visitors pay to see it (a visitors' book).
    /// </summary>
    /// <returns>Whether to keep it, and what to sell from the display to make room (or null).</returns>
    public static (bool Keep, Item Replace) ShouldKeep(GameState state, Item item, int value)
    {
        // A Heartsong: worth nothing sold, a great deal singing in the shop, and it takes no room. Always kept.
        if (item.IsSong) return (true, null);
        if (state.Is(Characters.Isolde)) return Collects(state, item, value);
        if (!item.Displayable || item.Prestige < 2 || state.Tier < 3 || (state.Exhibition >= GameState.ExhibitionCap && !state.HasCurio("book")) ||
            value > item.Prestige * GoldPerPoint || state.Gold - value < 20 + 15 * state.Tier)
            return (false, null);
        if (!state.DisplayFull) return (true, null);
        var plainest = state.Display.Where(i => !i.IsSong).OrderBy(i => i.Prestige).First();
        return plainest.Prestige < item.Prestige ? (true, plainest) : (false, null);
    }

    /// <summary>Isolde keeps what her visitors will pay back in this many nights, while she can spare it.</summary>
    public const int PaybackNights = 12;

    /// <summary>
    /// Isolde's collecting: keep anything whose door money (and a point's worth of drawing customers) pays back its sale
    /// price within <see cref="PaybackNights"/>, while there's room or something plainer to sell to make room, and
    /// enough left for tomorrow and the Guild: her collection is no help on a short night.
    /// </summary>
    static (bool Keep, Item Replace) Collects(GameState state, Item item, int value)
    {
        if (!item.Displayable || state.DisplaySlots == 0 || value > item.Prestige * Characters.AdmissionPerPoint * Characters.IsoldeScale(state.Tier) * PaybackNights) return (false, null);
        Item replace = null;
        if (state.DisplayFull)
        {
            replace = state.Display.Where(i => !i.IsSong).OrderBy(i => i.Prestige).ThenBy(i => i.Value).First();
            if (replace.Prestige >= item.Prestige) return (false, null);
        }
        int back = replace == null ? 0 : state.SaleValue(replace);
        return state.Gold + back - value >= Reserve(state) ? (true, replace) : (false, null);
    }
}
