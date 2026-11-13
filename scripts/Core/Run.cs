using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The run around the days: five-day weeks, each day with its business (the story, something on in town, Nim, the
/// Guild's night); curios that bend the shop's rules; commissions from regulars; and the shards of the Aldric Key.
/// </summary>
public sealed partial class GameState
{
    // ---- Weeks and the Guild's dues ----

    /// <summary>The week today is in, from 1.</summary>
    public int Week => Math.Max(0, Day - 1) / Guild.DaysPerWeek + 1;

    /// <summary>Today's place in the week, 1 to <see cref="Guild.DaysPerWeek"/> (0 before the first day).</summary>
    public int DayOfWeek => Day <= 0 ? 0 : (Day - 1) % Guild.DaysPerWeek + 1;

    /// <summary>The Guild comes tonight.</summary>
    public bool DuesTonight => Day > 0 && DayOfWeek == Guild.DaysPerWeek;

    /// <summary>Days of trading after today before the Guild comes.</summary>
    public int DaysToDue => Guild.DaysPerWeek - Math.Max(1, DayOfWeek);

    public int WeeksPaid { get; private set; }

    /// <summary>Vane has introduced the Guild (the first evening): until then, nothing shows the due.</summary>
    public bool GuildIntroduced => Day > IntroDay || WeeksPaid > 0;

    /// <summary>
    /// Mister Vane introduces himself on the second evening: the first day is for learning to listen (the playtest:
    /// three lessons and a debt on the same day was too much at once), and the first due is the next night.
    /// </summary>
    public const int IntroDay = 2;

    public bool Foreclosed { get; private set; }

    /// <summary>All eight weeks of Bram's debt are paid. The Guild keeps coming (endless), but the run is won.</summary>
    public bool DebtCleared => WeeksPaid >= Guild.Weeks;

    /// <summary>The day the debt was cleared, on the eighth Guild night or paid off early (0 until it is).</summary>
    public int ClearedDay { get; private set; }

    /// <summary>The week the debt was cleared in (0 until it is).</summary>
    public int ClearedWeek => ClearedDay <= 0 ? 0 : (ClearedDay - 1) / Guild.DaysPerWeek + 1;

    /// <summary>This week's due (with the Guild's half-price deal for the map, and anything rolled over from last week).</summary>
    public int DueNow => Money.Round(DueFor(Week) * (_halfDue ? 0.5f : 1)) + Arrears;

    /// <summary>A week's due: the Guild's, a step higher for each challenge level.</summary>
    public int DueFor(int week)
    {
        // Thornwick wants you (a boon): its council pays a share of the due you owe at its market.
        float invited = week == Challenges.InvitedWeek && Favoured(Challenges.Invited) ? 1 - Challenges.InvitedShare : 1;
        float due = Guild.Due(week) * (Challenge > 0 ? Challenges.DebtFactor(Challenge) : 1) * TaleDues * invited;
        return Challenge > 0 || TaleDues != 1 || invited != 1 ? Money.Round(due) : Guild.Due(week);
    }

    /// <summary>
    /// What the tales do to the Guild's dues (docs/stories.md): a friend of the Guild has a twentieth overlooked, a foe is
    /// audited a twentieth more, and the King's decree cuts the tithe by a fifth.
    /// </summary>
    float TaleDues => (Friend(Faction.Guild) ? 1 - GuildFavour : Foe(Faction.Guild) ? 1 + GuildFavour : 1f) * (Flagged(TaleBook.Decree) ? Decreed : 1f) *
                      (OnTrial ? Tried : 1f);

    /// <summary>What the Guild overlooks for a friend (and adds for a foe), and what's left of a due after the King's decree.</summary>
    public const float GuildFavour = 0.05f, Decreed = 0.8f;

    /// <summary>What's left of a due while the Guild's on trial (the King's court).</summary>
    public const float Tried = 0.5f;

    bool _halfDue;

    /// <summary>What's left of last week's due after Bram vouched for you, with the Guild's interest.</summary>
    public int Arrears { get; private set; }

    /// <summary>How many times Bram has vouched for you this run.</summary>
    public int GraceUsed { get; private set; }

    /// <summary>
    /// Bram can vouch for you once a run (once more with a guildsman's marker, and when he's prepared; not at all when
    /// he's stretched thin): a short night rolls over to next week instead of costing the shop.
    /// </summary>
    public bool GraceLeft => GraceUsed < Graces;

    public int Graces => 1 + (HasCurio("marker") ? 1 : 0) + (Favoured(Challenges.Prepared) ? 1 : 0) - (Under(Challenges.Stretched) ? 1 : 0);

    /// <summary>The Guild's interest on a due that rolls over.</summary>
    public const float GraceInterest = 1.5f;

    /// <summary>Bram's word is good for one week, but not for the last of the debt.</summary>
    public bool CanAskGrace => GraceLeft && Gold < DueNow && Week != Guild.Weeks;

    /// <summary>
    /// Bram vouches for you: everything you have goes to the Guild, and the rest (with interest) is added to next week's
    /// due. Once a run.
    /// </summary>
    public int AskGrace()
    {
        if (!CanAskGrace) return 0;
        int owed = (int)MathF.Ceiling((DueNow - Gold) * GraceInterest);
        Gold = 0;
        GraceUsed++;
        WeeksPaid++;
        _halfDue = false;
        Arrears = owed;
        MoveInward();
        return owed;
    }

    /// <summary>
    /// The road inward (0.36): a week's due settled, the Guild's carters move the shop a market further in that night, so
    /// each week of the season is a market: Brindle, Thornwick, Crownhold, the Royal Emporium. Until 0.35 a move was
    /// bought (80, 300 and 3,000g), and a learner who couldn't afford Crownhold spent twenty days at Thornwick. The Guild
    /// wants its keywright at the Gate by the season's last night (docs/narrative.md), so it pays the carters.
    /// </summary>
    void MoveInward()
    {
        int tier = Math.Min(ShopTiers.Max, WeeksPaid + 1);
        if (tier <= Tier) return;
        for (int t = Tier + 1; t <= tier; t++) _owned.Add($"shop{t}");
        MovedFrom = Tier;
        Tier = tier;
        TierSince = Day + 1;
        UpgradesOpen = true;
    }

    /// <summary>The market the shop left tonight, when a due's just moved it inward (0 otherwise): for the night's screens.</summary>
    public int MovedFrom { get; private set; }

    /// <summary>The market the next due paid would move the shop to (0: it's as far in as it goes).</summary>
    public int NextMarket => Tier < ShopTiers.Max && !DebtCleared ? Tier + 1 : 0;

    /// <summary>What a miser's lockbox added on the last Guild night paid.</summary>
    public int Hoarded { get; private set; }

    /// <summary>This week's due is paid (it's the Guild's night, and they've been).</summary>
    public bool PaidThisWeek => WeeksPaid >= Week;

    /// <summary>The week of the next due the Guild will come for: this one, or the first not paid yet.</summary>
    public int NextDueWeek => PaidThisWeek ? WeeksPaid + 1 : Week;

    /// <summary>The next due the Guild will come for, and the days of trading left before it.</summary>
    public int NextDue => PaidThisWeek ? DueFor(NextDueWeek) + Arrears : DueNow;
    public int DaysToNextDue => (NextDueWeek - Week) * Guild.DaysPerWeek + DaysToDue;

    /// <summary>What pawning a displayed thing fetches tonight: the Guild's price (better for a pawnbroker's friend, better still with a pawn ticket).</summary>
    public int PawnValue(Item item) => (int)MathF.Round(ShelfValue(item) * PawnRate);

    float PawnRate => Favoured(Challenges.HonestClerk) ? 1 : HasCurio("ticket") ? Curios.TicketRate : Guild.PawnRate;

    /// <summary>What a curio fetches if you sell it back: half what you paid for it.</summary>
    public int CurioValue(string key) => Math.Max(1, (_curioPaid.TryGetValue(key, out int paid) ? paid : Curios.Base(Curios.Get(key).Rarity)) / 2);

    /// <summary>Everything that could be pawned to make up a due.</summary>
    public int PawnableWorth => Display.Where(CanPawn).Sum(PawnValue) + _curios.Sum(CurioValue);

    /// <summary>Whether there's any way to pay tonight, pawning everything.</summary>
    public bool CanCover => Gold + PawnableWorth >= DueNow;

    /// <summary>Pawns something on display to make up the due.</summary>
    public int Pawn(Item item)
    {
        if (!CanPawn(item) || !Display.Remove(item)) return 0;
        int value = PawnValue(item);
        Gold += value;
        return value;
    }

    /// <summary>Sells a curio back (to Nim, or to the Guild on a short night) for half what you paid.</summary>
    public int SellCurio(string key)
    {
        int value = CurioValue(key);
        if (!_curios.Remove(key)) return 0;
        _curioPaid.Remove(key);
        Gold += value;
        return value;
    }

    /// <summary>Pays the week's due, if there's enough gold.</summary>
    public bool PayDues()
    {
        int due = DueNow;
        if (Gold < due) return false;
        Gold -= due;
        WeeksPaid++;
        _halfDue = false;
        Arrears = 0;
        if (DebtCleared && ClearedDay == 0)
        {
            ClearedDay = Day;
            // Bram's pledge is his account: paid, a freed Bram can come home.
            TryHome(Campaign.Bram);
        }
        MoveInward();
        // A miser's lockbox: what's left after the Guild's been grows a fifth.
        Hoarded = HasCurio("lockbox") ? Money.Round(Gold * Curios.LockboxShare) : 0;
        Gold += Hoarded;
        return true;
    }

    /// <summary>
    /// What's left of Bram's debt: this week's due if it's still to pay (with anything rolled over), and every week's
    /// after it, to the eighth. Nothing once it's cleared.
    /// </summary>
    public int DebtLeft
    {
        get
        {
            if (DebtCleared) return 0;
            int left = PaidThisWeek ? Arrears : DueNow;
            for (int week = Math.Max(Week, WeeksPaid) + 1; week <= Guild.Weeks; week++) left += DueFor(week);
            return left;
        }
    }

    /// <summary>
    /// On a Guild night, the whole of what's left: tonight's due first, as any Guild night (a miser's lockbox grows what's
    /// left in the purse), then the rest. Returns what was paid, all told (0.35: paying it all skipped the lockbox).
    /// </summary>
    public int PayAll()
    {
        if (!CanPayOff) return 0;
        int before = Gold, hoarded = 0;
        if (DuesTonight && !PaidThisWeek && PayDues()) hoarded = Hoarded;
        PayOff();
        return before + hoarded - Gold;
    }

    /// <summary>You could pay off the whole debt now, and finish ahead of time.</summary>
    public bool CanPayOff => GuildIntroduced && !DebtCleared && !Foreclosed && Gold >= DebtLeft;

    /// <summary>
    /// Pays off everything that's left of the debt at once: the run is won, however early. If you keep trading, the
    /// Guild leaves you alone until its interest starts, after the eighth week.
    /// </summary>
    public int PayOff() => CanPayOff ? SettleDebt() : 0;

    /// <summary>The whole debt paid, whatever the night (the GM's stroke pays it before Mister Vane has even called).</summary>
    int SettleDebt()
    {
        int paid = DebtLeft;
        Gold -= paid;
        WeeksPaid = Guild.Weeks;
        Arrears = 0;
        _halfDue = false;
        ClearedDay = Day;
        TryHome(Campaign.Bram);
        MoveInward();
        return paid;
    }

    // ---- Bram's secret stash ----

    /// <summary>
    /// How many times Bram will dip into his secret stash for you in a run (once more with a rainy-day purse, and when
    /// he's prepared; once less when he's stretched thin).
    /// </summary>
    public const int StashTimes = 2;

    public int StashUsed { get; private set; }

    public int StashLeft => Math.Max(0, StashTimes + (HasCurio("purse") ? 1 : 0) + (Favoured(Challenges.Prepared) ? 1 : 0) -
                                        (Under(Challenges.Stretched) ? 1 : 0) - StashUsed);

    /// <summary>
    /// Start a day with less than <c>Below</c> and Bram slips you <c>Gift</c> from his secret stash, while it lasts: a
    /// little at the stall, more for a bigger shop, whose chests cost more.
    /// </summary>
    public static (int Below, int Gift) Stash(int tier) => tier switch
    {
        1 => (20, 30),
        2 => (30, 40),
        3 => (50, 80),
        _ => (270, 400),
    };

    /// <summary>Nearly broke at the start of a day: Bram helps out, if he still can.</summary>
    void DipIntoStash()
    {
        var (below, gift) = Stash(Tier);
        if (StashLeft <= 0 || Gold >= below) return;
        StashUsed++;
        Gold += gift;
        Today.Stash = gift;
    }

    // ---- Surprises ----

    /// <summary>Things opened since the last one that came out better than its chest promised.</summary>
    public int DrySpell { get; private set; }

    /// <summary>
    /// After this many things without a surprise, each chest you buy gets a growing chance (this much per thing more)
    /// that something in it is a step better than its colour, up to a cap: a long dry spell doesn't last forever.
    /// </summary>
    public const int PityAfter = 10;
    public const float PityStep = 0.03f, PityMax = 0.5f;

    /// <summary>A lucky shell makes a dry spell end sooner.</summary>
    int PityStart => HasCurio("shell") || Favoured(Challenges.Charm) ? Curios.ShellPityAfter : PityAfter;

    public float PityChance => Math.Clamp((DrySpell - PityStart) * PityStep, 0f, PityMax);

    /// <summary>
    /// A chest you've just bought: after a long dry spell, maybe one thing in it is a step better. Then the spell ends if
    /// anything in it beat its chest's colour, or grows by what's in it.
    /// </summary>
    void Pity(Chest chest)
    {
        var eligible = chest.Everything.Where(i => !i.Fake && !i.Broken && !i.IsSong).ToList();
        var lift = eligible.Where(i => i.Quality < Rarity.Epic).ToArray();
        // Under the cursed crescent everything comes out exactly as promised, a dry spell or not (0.35).
        if (lift.Length > 0 && !HasCurio("crescent") && Rng.NextFloat() < PityChance)
        {
            var pick = Rng.PickOne(lift);
            chest.Improve(i => i.Id == pick.Id ? i with { Quality = Categories.Lift(i.Quality) } : i);
        }
        if (chest.Everything.Any(i => !i.Fake && !i.Broken && i.Quality > chest.Quality)) DrySpell = 0;
        else DrySpell += eligible.Count;
    }

    /// <summary>The Guild takes the shop: the run is over.</summary>
    public void Foreclose() => Foreclosed = true;

    // ---- The map ----

    // ---- Curios ----

    readonly List<string> _curios = new();

    /// <summary>What you paid for each curio: Nim (or the Guild) gives half of it back.</summary>
    readonly Dictionary<string, int> _curioPaid = new();

    public IReadOnlyList<string> CurioKeys => _curios;

    /// <summary>The curios Nim carries this run (his stock grows across runs); null: all of them.</summary>
    public HashSet<string> CuriosOpen { get; private set; }

    public bool CurioOpen(string key) => CuriosOpen == null || CuriosOpen.Contains(key);

    /// <summary>For the sim's builds: owns a curio from the start, without paying, room or not.</summary>
    public void GiveCurio(string key)
    {
        if (!HasCurio(key)) _curios.Add(key);
        CuriosSeen.Add(key);
    }

    /// <summary>A curio taken back by whoever gave it (a promise broken): gone, for nothing.</summary>
    public void TakeCurio(string key)
    {
        _curios.Remove(key);
        _curioPaid.Remove(key);
    }

    /// <summary>Bram gives his word for you to the Guild (a tale's, not a short night's): it's spent for the run.</summary>
    public void SpendWord()
    {
        if (GraceLeft) GraceUsed++;
    }
    public IEnumerable<CurioDef> OwnedCurios => _curios.Select(Curios.Get);
    public bool HasCurio(string key) => _curios.Contains(key);
    public int CurioSlots => Curios.Slots + Upgrades.All.Where(u => Has(u.Key)).Sum(u => u.CurioSlots) + (Favoured(Challenges.Drawer) ? 1 : 0);
    public bool CuriosFull => _curios.Count >= CurioSlots;

    public bool BuyCurio(CurioDef curio, int price)
    {
        if (CuriosFull || HasCurio(curio.Key) || Gold < price) return false;
        Gold -= price;
        _curios.Add(curio.Key);
        _curioPaid[curio.Key] = price;
        CuriosSeen.Add(curio.Key);
        Today.CurioSpent += price;
        return true;
    }

    /// <summary>
    /// Nim comes by every night (0.28), after the day's end and the Guild: from the third in a first season (the first
    /// two have enough new), from the first in any later one (0.37).
    /// </summary>
    public bool PeddlerToday => Day >= (Veteran ? 1 : Curios.FirstNight) && !AwayTonight;

    // ---- Bram's drawer (0.37) ----

    /// <summary>The drawer's been offered this season (something taken from it, or nothing).</summary>
    public bool DrawerDone { get; private set; }

    /// <summary>
    /// Bram's drawer is open (<see cref="Curios.Drawer"/>): the first morning of a season after the first, with nothing
    /// taken from it yet.
    /// </summary>
    public bool DrawerOpen => Veteran && Day <= 1 && !DrawerDone && _curios.Count < CurioSlots && Curios.Drawer(this).Count > 0;

    /// <summary>Takes one curio out of Bram's drawer (or none): it's yours, and worth a coin to Nim.</summary>
    public void TakeFromDrawer(CurioDef curio)
    {
        if (DrawerDone) return;
        DrawerDone = true;
        if (curio == null || HasCurio(curio.Key)) return;
        _curios.Add(curio.Key);
        _curioPaid[curio.Key] = 2;
        CuriosSeen.Add(curio.Key);
    }

    /// <summary>The week's story is told this morning.</summary>
    public bool StoryToday => Plan != null && Plan.Week == Week && Plan.StoryDay > 0 && Plan.StoryDay == DayOfWeek;

    /// <summary>What today is for (<see cref="WeekPlan.Role"/>).</summary>
    public DayRole TodayRole => Plan != null && Plan.Week == Week ? Plan.Role(DayOfWeek) : DayOfWeek == Guild.DaysPerWeek ? DayRole.Tax : DayRole.None;

    /// <summary>What Nim has today.</summary>
    public List<CurioDef> PeddlerStock { get; } = new();

    /// <summary>What a pick grade does to a thing's price: forcing with a burglar's knife scratches nothing.</summary>
    public float GradeMultiplier(PickGrade grade) => grade == PickGrade.Forced && HasCurio("knife") ? 1f : Picks.Multiplier(grade);

    /// <summary>How much less likely glass is to break when you force a lock (a burglar's knife or the Imp's Ember: it never does).</summary>
    public float ForceSturdier => HasCurio("knife") || Shows(Heartsongs.Ember) ? 1f : Sturdier;

    /// <summary>What today's event does to the price of a thing of this kind.</summary>
    float EventSales(Category kind) => (EventIs("festival") ? Events.FestivalSales : 1) *
                                       (EventIs("buyer") && kind == BuyerKind ? Events.BuyerSales : 1) *
                                       (EventIs("tourney") && kind is Category.Arms or Category.Garb ? Events.TourneySales : 1) *
                                       (EventIs("assessor") ? Events.AssessorSales : 1);

    // ---- Commissions ----

    /// <summary>This week's commissions (they lapse when the Guild comes).</summary>
    public List<Commission> Commissions { get; } = new();

    /// <summary>A commission this thing would do for, if any.</summary>
    public Commission WantedBy(Item item) => Commissions.FirstOrDefault(c => Takes(c, item));

    /// <summary>
    /// Whether a commission would take this thing: what it asks for, or (with a letter of introduction, for the regulars)
    /// a quality lower.
    /// </summary>
    public bool Takes(Commission c, Item item) => c.Wants(item, LetterLeeway(c));

    /// <summary>How many qualities lower than asked a commission will take.</summary>
    public int LetterLeeway(Commission c) => HasCurio("letter") && c.Patron != Events.Crier ? 1 : 0;

    /// <summary>What a commission offers, all told, before what a thing would sell for comes into it (a lot: for all of it).</summary>
    public int CommissionReward(Commission c) => (int)MathF.Round(c.Reward * (Shows(Heartsongs.Lark) ? Heartsongs.LarkPay : 1) *
                                                                   (HasCurio("ribbon") ? Curios.RibbonPay : 1));

    /// <summary>What a commission offers for each thing it takes (a lot's reward shared out).</summary>
    public int CommissionEach(Commission c) => Money.Round(CommissionReward(c) / (float)c.Needed);

    /// <summary>
    /// What handing over a thing that would sell for <paramref name="sale"/> pays: the commission's share for it, and
    /// never less than a quarter over the sale (<see cref="Patrons.Premium"/>). A commission always pays more than selling.
    /// </summary>
    /// (A patron's ribbon puts half as much again on the whole of it, the quarter over the sale too: 0.35, it was only the reward.)
    public int CommissionPay(Commission c, int sale)
    {
        int floor = Math.Max(sale + 1, (int)MathF.Ceiling(sale * Patrons.Premium * (HasCurio("ribbon") ? Curios.RibbonPay : 1)));
        return Math.Max(CommissionEach(c), floor);
    }

    /// <summary>
    /// Hands something over for a commission instead of selling it (giving up <paramref name="forgo"/>, its sale price,
    /// if that was already counted; <paramref name="sale"/> is what it would sell for). Each thing pays as it's handed
    /// over, a lot's too. Returns what it paid.
    /// </summary>
    public int Deliver(Item item, Commission commission, int forgo = 0, int sale = -1)
    {
        if (!Takes(commission, item)) return 0;
        int pay = CommissionPay(commission, sale >= 0 ? sale : Math.Max(forgo, SaleValue(item)));
        Gold -= forgo;
        Today.Earned -= forgo;
        commission.Delivered++;
        Today.Delivered++;
        Gold += pay;
        Today.Commissions += pay;
        if (commission.Done) EarnPiece(Campaign.Patron);
        return pay;
    }

    // ---- Events ----

    /// <summary>This week's plan (its events, the kind wanted, Nim's day) and next week's, made a week ahead.</summary>
    public WeekPlan Plan { get; private set; }
    public WeekPlan NextPlan { get; private set; }

    /// <summary>The kind the Crown's buyer pays double for (or the crier's reward is for) this week.</summary>
    public Category BuyerKind => Plan?.Wanted ?? Category.Coins;

    readonly List<string> _eventsSeen = new();

    /// <summary>The plan for a week, if it's been made yet (this week's and next week's have).</summary>
    public WeekPlan PlanFor(int week) => Plan?.Week == week ? Plan : NextPlan?.Week == week ? NextPlan : null;

    /// <summary>A day's number, from its week and its day of the week.</summary>
    public static int DayNumber(int week, int dayOfWeek) => (week - 1) * Guild.DaysPerWeek + dayOfWeek;


    public EventDef TodayEvent => Day <= 0 ? null : ForcedEvent != null ? Events.Find(ForcedEvent)
        : Plan?.On(DayOfWeek) is string key ? Events.Find(key) : null;

    /// <summary>
    /// What tomorrow holds, for the day's end (0.36: a reason to open the shop again): the Guild's night, something on in
    /// town, or someone who said they'd come by; null when it's a day like any other (or the Guild is at the door tonight,
    /// and tomorrow is wherever its carters take the shop).
    /// </summary>
    public string Tomorrow()
    {
        if (Day <= 0 || (DuesTonight && !PaidThisWeek)) return null;
        int next = DayOfWeek + 1;
        var plan = next <= Guild.DaysPerWeek ? Plan : NextPlan;
        if (next > Guild.DaysPerWeek) next = 1;
        if (plan?.On(next) is string key && Events.Find(key) is EventDef e) return Loc.T($"Tomorrow: {e.Name}");
        if (GuildIntroduced && next == Guild.DaysPerWeek && Week >= NextDueWeek) return Loc.T($"Tomorrow night: the Guild, for {NextDue}g");
        foreach (var (day, who, note) in Expected())
            if (day == Day + 1) return Loc.T($"Tomorrow: {note ?? Story.Name(who)}");
        return null;
    }

    /// <summary>For checking an event's screens (the bot's --event): it happens every day.</summary>
    public string ForcedEvent { get; set; }

    public bool EventIs(string key) => TodayEvent?.Key == key;

    int EventCustomers => EventIs("market") ? Events.MarketCustomers : EventIs("storm") ? Events.StormCustomers : 0;

    /// <summary>
    /// A new week: last week's commissions lapse; from the second week, the regulars ask for things and the Hooded Woman
    /// for hers. Last week's plan for this one comes due (Nim's day and the events), and next week's is made. All from
    /// the seed, so a saved shop gets the same week.
    /// </summary>
    void StartWeek()
    {
        var rng = new Random(unchecked(Seed * 104729 + Week * 7907));
        Commissions.Clear();
        _snubbed.Clear();
        bool fair = Favoured(Challenges.FairDays);
        Plan = NextPlan?.Week == Week ? NextPlan : Events.Plan(Seed, Week, Tier, _eventsSeen, fair);
        // Next week is a market further in (0.36), while there's a due to pay: its days are planned for that market.
        NextPlan = Events.Plan(Seed, Week + 1, Math.Max(Tier, Math.Min(ShopTiers.Max, Week + 1)), _eventsSeen, fair);
        // The regulars' first commissions wait a day (Patrons.FromDay): the first morning at Thornwick has enough new.
        if (Week < CommissionsFrom || (Week == CommissionsFrom && Patrons.FromDay > 1)) return;

        Commissions.AddRange(Patrons.Roll(rng, this));
    }

    /// <summary>
    /// The week the regulars start asking for things: the second in a first season (after the first visit from the Guild),
    /// the first in any later one, whose regulars know the shop (0.37).
    /// </summary>
    public int CommissionsFrom => Veteran ? 1 : Patrons.FromWeek;

    /// <summary>The regulars' first commissions, on the second morning of the week they start asking.</summary>
    void FirstCommissions()
    {
        if (Week != CommissionsFrom || DayOfWeek != Patrons.FromDay || Patrons.FromDay <= 1) return;
        Commissions.AddRange(Patrons.Roll(new Random(unchecked(Seed * 104729 + Week * 7907 + 13)), this));
    }

    /// <summary>
    /// Today's event, if it brings someone or something: the Stranger joins the queue, one chest is gilded, the crier
    /// posts a reward, or every seller tells you a dream, a riddle or a clerk's tally.
    /// </summary>
    void StageEvent()
    {
        Commissions.RemoveAll(c => c.Lapses > 0 && c.Lapses < Day);
        if (EventIs("stranger"))
            Queue.Insert(Rng.Next(1, Queue.Count + 1), Sing(Events.StrangerVisit(Rng, Tier, PriceFactor, Ways)));
        if (EventIs("gilded"))
        {
            var plain = Queue.Select((v, i) => (v, i)).Where(p => p.v.Lesson == null).ToArray();
            if (plain.Length > 0)
            {
                var (visit, index) = plain[Rng.Next(plain.Length)];
                Queue[index] = Events.Gild(Rng, visit);
            }
        }
        if (EventIs(Events.Crier)) Commissions.Add(Events.Reward(BuyerKind, Tier, Day));
        if (TodayEvent is { Key: "dreaming" or "wardens" or "census" } e)
            for (int i = 0; i < Queue.Count; i++) Queue[i] = Events.Restate(Rng, Queue[i], e.Key, Tier, ward: e.Key == "dreaming" && Boosts.AlwaysWard);
    }

    // ---- Each night ----

    /// <summary>
    /// What comes in overnight: interest on savings and visitors' coins (curios), and the door money for Isolde's
    /// collection. Paid at the end of the day.
    /// </summary>
    void Overnight()
    {
        if (Admission > 0)
        {
            Today.Admission = Admission;
            Gold += Today.Admission;
        }
        if (HasCurio("jar"))
        {
            int interest = Math.Min((int)(Gold * Curios.InterestRate), Curios.InterestPerTier * ShopTiers.Scale(Tier));
            Gold += interest;
            Today.Curios += interest;
        }
        if (BookCoins > 0)
        {
            int coins = BookCoins;
            Gold += coins;
            Today.Curios += coins;
        }
    }

    /// <summary>What a visitors' book takes in a night: so much a point of the exhibition, more in a grander shop.</summary>
    public int BookCoins => HasCurio("book") ? Curios.BookPerPoint * ShopTiers.Scale(Tier) * Exhibition : 0;

    /// <summary>A new find in the journal: a collector's catalogue pays for it.</summary>
    void Catalogue()
    {
        if (!HasCurio("catalogue")) return;
        int fee = Curios.CataloguePerTier * ShopTiers.Scale(Tier);
        Gold += fee;
        Today.Curios += fee;
    }
}
