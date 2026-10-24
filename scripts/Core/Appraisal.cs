using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>How one thing in an opened chest compares with what the slip said.</summary>
public enum ReadKind { Right, RightMagic, SpottedFake, SpottedBroken, FoundHidden, Missed, MissedMagic, NotMagic, Fooled, Broken, MissedHidden }

/// <param name="Hidden">It was under a false bottom.</param>
/// <param name="Warded">The ward gave it up: you named it.</param>
public sealed record ItemRead(Item Item, ReadKind Kind, bool Hidden = false, bool Warded = false)
{
    public bool Right => Kind is ReadKind.Right or ReadKind.RightMagic or ReadKind.SpottedFake or ReadKind.SpottedBroken or ReadKind.FoundHidden;

    public string Note => Kind switch
    {
        ReadKind.RightMagic => "enchanted!",
        ReadKind.SpottedFake => "fake, as you thought",
        ReadKind.SpottedBroken => "broken, as you knew",
        ReadKind.FoundHidden => "the false bottom!",
        ReadKind.Missed => "you missed it",
        ReadKind.MissedMagic => "enchanted: missed the glow",
        ReadKind.NotMagic => "not enchanted",
        ReadKind.Fooled => "FAKE",
        ReadKind.Broken => "broken",
        ReadKind.MissedHidden => "a false bottom!",
        _ => "",
    };
}

/// <summary>The slip checked against what was really inside.</summary>
/// <param name="Ghosts">One entry for each thing the slip counted that wasn't in the chest.</param>
/// <param name="GhostHidden">A false bottom marked on the slip that wasn't there.</param>
/// <param name="KeptWarded">A warded thing the ward kept, because the slip didn't name it truly, or null.</param>
public sealed record ReadReport(IReadOnlyList<ItemRead> Items, IReadOnlyList<Category> Ghosts, bool GhostHidden, Item KeptWarded = null)
{
    public bool Perfect => Items.All(r => r.Right) && Ghosts.Count == 0 && !GhostHidden && KeptWarded == null;
}

/// <summary>
/// The player's appraisal slip: how many of each kind of thing they think are in the chest, whether one of them is
/// enchanted, and whether there's a false bottom. It starts with what the seller says is in it; the player corrects it from
/// what they hear themselves. It only adds up what the player marked, at what such a thing is worth if it's as good as
/// the chest's colour says: the chest itself still has to be opened to see how good each thing really is.
/// </summary>
public sealed class Appraisal
{
    readonly int[] _count = new int[Categories.All.Length];
    readonly bool[] _magic = new bool[Categories.All.Length];

    /// <summary>The chest's colour: what each thing is probably worth.</summary>
    public Rarity Chest { get; }

    /// <summary>The shop tier, which decides what a false bottom might hold and whether to count.</summary>
    public int Tier { get; }

    /// <summary>The player thinks there's a false bottom.</summary>
    public bool Hidden
    {
        get => _hidden;
        set
        {
            _hidden = value;
            _outcomes = null;
        }
    }

    bool _hidden;

    // The outcomes, worked out once per state of the slip (the odds bar asks again whenever the offer slider moves).
    IReadOnlyList<(int Value, float Chance)> _outcomes;

    /// <summary>
    /// The shop the slip is filled in at (<see cref="GameState"/>): what a thing you name fetches there today (its trades,
    /// its name, its curios, the day) and how likely it is to come out better than its chest (its luck). Every value on
    /// the slip, the odds bar and the careful player's sums are what the things it names would sell for here if the read
    /// is right, not what they're worth in the abstract. Null: their worth, at the chest's odds.
    /// </summary>
    public ISlipModel Model
    {
        get => _model;
        set
        {
            _model = value;
            _outcomes = null;
        }
    }

    ISlipModel _model;

    /// <summary>What one named thing of a kind fetches at this quality (with <see cref="Count"/> of its kind named).</summary>
    float Fetch(Category kind, Rarity quality, bool magic, bool hidden = false) =>
        _model?.Value(kind, quality, magic, Chest, Math.Max(1, Count(kind)), hidden) ?? Categories.Value(kind, quality, magic);

    /// <summary>How good a named thing of this kind is likely to come out.</summary>
    (Rarity Quality, float Chance)[] Odds(Category kind, bool hidden = false) => _model != null ? _model.Odds(kind, Chest, hidden) : Categories.Odds(Chest);

    /// <summary>The most of one kind the slip counts: two or three of a kind turn up from the town shop on.</summary>
    public int MaxCount => Tier >= Market.PairsTier ? 3 : 1;

    public Appraisal(Rarity chest, int tier, IEnumerable<Category> marked = null)
    {
        Chest = chest;
        Tier = tier;
        foreach (var category in marked ?? Enumerable.Empty<Category>())
            _count[(int)category] = Math.Min(MaxCount, _count[(int)category] + 1);
    }

    public Appraisal(Visit visit, int tier) : this(visit.Chest.Quality, tier, visit.Claim)
    {
    }

    /// <summary>
    /// The Bookkeeping Engine's record of a chest: every real thing inside by kind (gilded iron and sand aren't), whether
    /// any of a kind is enchanted, and a false bottom if there is one. How good each thing is, it can't say: that's still
    /// the opening's.
    /// </summary>
    public static Appraisal Recorded(Chest chest, int tier)
    {
        var slip = new Appraisal(chest.Quality, tier) { Hidden = chest.Hidden != null && tier >= 2 };
        // The Guild's assessors register warded goods too: the engine names it.
        var things = chest.Items.Where(i => !i.Fake && !i.Broken).Concat(chest.Warded == null ? Array.Empty<Item>() : new[] { chest.Warded });
        foreach (var kind in things.GroupBy(i => i.Category))
            slip.Set(kind.Key, kind.Count(), kind.Any(i => i.Magic));
        return slip;
    }

    public int Count(Category category) => _count[(int)category];

    /// <summary>One of them glowed: it's magic.</summary>
    public bool Magic(Category category) => _magic[(int)category] && _count[(int)category] > 0;

    /// <summary>The most of this kind the slip counts (a chest never holds two Heartsongs).</summary>
    public int MaxOf(Category category) => category == Category.Heartsong ? 1 : MaxCount;

    public void Set(Category category, int count, bool magic = false)
    {
        _count[(int)category] = Math.Clamp(count, 0, MaxOf(category));
        _magic[(int)category] = magic && count > 0;
        _outcomes = null;
    }

    /// <summary>One more of this kind, or back to none after the most there can be.</summary>
    public void CycleCount(Category category)
    {
        int count = Count(category) >= MaxOf(category) ? 0 : Count(category) + 1;
        Set(category, count, Magic(category));
    }

    /// <summary>Marks one of them magic, or not. Marking a kind you hadn't ticked ticks one.</summary>
    public void ToggleMagic(Category category) => Set(category, Math.Max(1, Count(category)), !Magic(category));

    public IEnumerable<Category> Marked => Categories.All.Select(c => c.Category).Where(c => Count(c) > 0);

    /// <summary>What one of this kind fetches here if it's as good as the chest's colour.</summary>
    public int OneValue(Category category, bool magic = false) => (int)MathF.Round(Fetch(category, Chest, magic));

    /// <summary>What the slip says this kind adds up to.</summary>
    public int ValueOf(Category category)
    {
        int count = Count(category);
        if (count == 0) return 0;
        return Magic(category) ? (count - 1) * OneValue(category) + OneValue(category, true) : count * OneValue(category);
    }

    /// <summary>What a false bottom fetches here, on average over what people hide, as good as the colour promises.</summary>
    public int HiddenValue => (int)MathF.Round(Market.HiddenOdds(Tier).Sum(h => h.Chance * Fetch(h.Kind, Chest, false, true)));

    /// <summary>What the chest is worth if the slip is right and everything is as good as its colour.</summary>
    public int Worth => Marked.Sum(ValueOf) + (Hidden ? HiddenValue : 0);

    /// <summary>
    /// What it's worth on average if the slip is right, allowing for things coming out better or worse than the colour.
    /// The careful player's model knows this; the screen doesn't show it.
    /// </summary>
    public int ExpectedWorth => (int)MathF.Round(Marked.Sum(c => (Count(c) - (Magic(c) ? 1 : 0)) * Expected(c, false) + (Magic(c) ? Expected(c, true) : 0)) +
                                                 (Hidden ? Market.HiddenOdds(Tier).Sum(h => h.Chance * Expected(h.Kind, false, true)) : 0));

    /// <summary>What one named thing of this kind fetches here on average, luck and all.</summary>
    float Expected(Category kind, bool magic, bool hidden = false) => kind == Category.Heartsong
        ? Fetch(kind, Chest, false)
        : Odds(kind, hidden).Sum(o => o.Chance * Fetch(kind, o.Quality, magic, hidden));

    /// <summary>
    /// Everything the chest could turn out to be worth if the slip is right, with how likely each total is: each thing's
    /// quality lands around the chest's colour (<see cref="Categories.Odds"/>), and a false bottom might hold any of the
    /// things people hide. Mostly it's what the colour promises; sometimes less; now and then a lot more.
    /// </summary>
    public IReadOnlyList<(int Value, float Chance)> Outcomes() => _outcomes ??= Work();

    IReadOnlyList<(int Value, float Chance)> Work()
    {
        var totals = new Dictionary<float, float> { [0f] = 1f };
        void Add(IEnumerable<(float Value, float Chance)> thing)
        {
            var next = new Dictionary<float, float>();
            foreach (var (total, p) in totals)
            foreach (var (value, q) in thing)
                next[total + value] = next.GetValueOrDefault(total + value) + p * q;
            totals = next;
        }
        // A Heartsong is priced as a trinket exactly as fine as its chest: no spread.
        IEnumerable<(float, float)> One(Category kind, bool magic, bool hidden = false) => kind == Category.Heartsong
            ? new[] { (Fetch(kind, Chest, false), 1f) }
            : Odds(kind, hidden).Select(o => (Fetch(kind, o.Quality, magic, hidden), o.Chance));

        foreach (var kind in Marked)
            for (int i = 0; i < Count(kind); i++)
                Add(One(kind, Magic(kind) && i == 0));
        if (Hidden)
            Add(Market.HiddenOdds(Tier).SelectMany(h => One(h.Kind, false, true).Select(o => (o.Item1, o.Item2 * h.Chance))));
        return totals.GroupBy(t => (int)MathF.Round(t.Key)).Select(g => (g.Key, g.Sum(t => t.Value))).OrderBy(o => o.Key).ToList();
    }

    /// <summary>
    /// What the chest is more likely than not worth at least, if the slip is right: the most you can pay and still have
    /// the odds bar on your side.
    /// </summary>
    public int LikelyWorth()
    {
        float below = 0;
        foreach (var (value, chance) in Outcomes())
            if ((below += chance) >= 0.5f) return value;
        return 0;
    }

    /// <summary>The chance the chest is worth more than this price, if the slip is right.</summary>
    public float ChanceAbove(int price) => Outcomes().Where(o => o.Value > price).Sum(o => o.Chance);

    /// <summary>
    /// Checks the slip against the chest. Of each kind, the slip's count goes to the real, whole things first (magic
    /// first if it marked one magic), then broken glass, then fakes; whatever's left over is a ghost.
    /// </summary>
    public ReadReport Check(Chest chest)
    {
        var reads = new List<ItemRead>();
        var ghosts = new List<Category>();
        foreach (var kind in Categories.All.Select(c => c.Category))
        {
            var items = chest.Items.Where(i => i.Category == kind)
                .OrderBy(i => i.Fake ? 2 : i.Broken ? 1 : 0).ThenBy(i => Magic(kind) && i.Magic ? 0 : 1).ToList();
            int marked = Count(kind);
            bool magicLeft = Magic(kind);
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                bool counted = i < marked;
                ReadKind read;
                if (item.Fake) read = counted ? ReadKind.Fooled : ReadKind.SpottedFake;
                else if (item.Broken) read = counted ? ReadKind.Broken : ReadKind.SpottedBroken;
                else if (!counted) read = item.Magic ? ReadKind.MissedMagic : ReadKind.Missed;
                else if (item.Magic) read = magicLeft ? ReadKind.RightMagic : ReadKind.MissedMagic;
                else read = ReadKind.Right;
                if (read == ReadKind.RightMagic) magicLeft = false;
                reads.Add(new ItemRead(item, read, Warded: chest.Unwarded != null && item.Id == chest.Unwarded.Id));
            }
            // Marked magic but nothing magic was there: the first one counted wasn't.
            if (magicLeft && !items.Any(i => i.Magic && !i.Fake))
            {
                int first = reads.FindIndex(r => r.Item.Category == kind && r.Kind == ReadKind.Right);
                if (first >= 0) reads[first] = reads[first] with { Kind = ReadKind.NotMagic };
            }
            for (int extra = items.Count; extra < marked; extra++) ghosts.Add(kind);
        }
        if (chest.Hidden != null)
            reads.Add(new ItemRead(chest.Hidden, Hidden ? ReadKind.FoundHidden : ReadKind.MissedHidden, true));
        return new ReadReport(reads, ghosts, Hidden && chest.Hidden == null, chest.Warded);
    }
}
