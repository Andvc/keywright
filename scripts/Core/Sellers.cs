using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// How a seller decides what their chest is worth. Each is something a real person might do, and each has a blind spot
/// a careful listener can see through.
/// </summary>
public enum Judgment
{
    /// <summary>Shakes it at their own strength and prices what they heard.</summary>
    Ear,
    /// <summary>Shakes it gently, then hard: hears everything there is to hear.</summary>
    Thorough,
    /// <summary>Counts the rattles and charges the same for each, whatever is rattling.</summary>
    Counter,
    /// <summary>Hefts it: heavy means valuable. Weapons and coin weigh a lot; spells and gems next to nothing.</summary>
    Weight,
    /// <summary>Judges by the chest's colour alone: a blue chest is worth what a blue chest usually holds.</summary>
    Colour,
    /// <summary>Names a number that came to them in a dream. It has nothing to do with the chest.</summary>
    Dreamer,
    /// <summary>Knows weapons; prices everything else at half, except gems, which he takes for diamonds.</summary>
    Miner,
    /// <summary>Knows her own gems (at what she paid for them) and nothing else in the chest.</summary>
    Jeweller,
    /// <summary>Never shook it, has no idea, takes a few coins.</summary>
    Clueless,
    /// <summary>Knows exactly what's in it, because he packed the fakes himself.</summary>
    Crook,
}

/// <summary>What a seller tells you (never what they heard: that's for you to work out): always true, and worth working out.</summary>
public enum Clue
{
    None,
    /// <summary>The pawn ticket: how many things were pledged (Pell).</summary>
    Articles,
    /// <summary>The weight on a game scale, to the half pound (Wenna).</summary>
    Pounds,
    /// <summary>A dream, as a sign, of what the Wood warded in it, or else of the dearest thing (Mother Bramble).</summary>
    Dream,
    /// <summary>The Arcanum's packing tag: every kind of thing packed, warded or not (Wren).</summary>
    Tag,
    /// <summary>Grandmother's riddle about one thing inside: the warded one, if there is one (Lady Corisande).</summary>
    Riddle,
    /// <summary>The Thornwick custom: a dowry chest always holds coin, a cloak and a potion (Gerda).</summary>
    Dowry,
    /// <summary>The Archive's ward-roll: the warded thing's name, faded (Quill).</summary>
    Roll,
    /// <summary>A bat's echoes: the shape of every thing inside, warded or not (Skrae).</summary>
    Echo,
    /// <summary>Three cards from the Locking's deck: a suit drawn twice is what's warded (Madame Oriel).</summary>
    Cards,
    /// <summary>A Guild clerk's tally of every article in it, false bottom and all (the Guild's Count).</summary>
    Tally,
    /// <summary>Sister Wren's word: the kind of thing her echo sleeps in, warded (the Silent Choir, 0.32).</summary>
    Choir,
}

/// <summary>How a seller trades.</summary>
public enum Trade
{
    /// <summary>Names a price. Take it or leave it.</summary>
    Fixed,
    /// <summary>Asks high. You get one offer: they take it, name a final price, or storm off if it's insulting.</summary>
    Haggle,
    /// <summary>Asks you to name a price. Two tries.</summary>
    Offer,
}

/// <summary>
/// Someone who brings a chest to the counter: who they are, where their chests come from (and they say so: the place is
/// a stated clue to what's inside), one way of judging what it's worth, one way of trading, and one rule that changes
/// what you can do, what you know or what the deal is (a clock, a rotten chest, a price that climbs with every shake, a
/// straw-packed chest, a pawn ticket, a scale, a dream, a wager, a claim on what's inside).
/// Together with the sounds in the chest, that makes each visit its own small puzzle. See docs/world.md.
/// </summary>
public sealed class SellerDef
{
    public string Key { get; init; }
    public string Name { get; init; }

    string _look;

    /// <summary>The figure they wear (assets/figures/{look}.png): until the new cast is drawn, an old regular's.</summary>
    public string Look
    {
        get => _look ?? Key;
        init => _look = value;
    }

    /// <summary>Whose people they are: their standing with you prices the tools they offer (<see cref="GameState.StandingPrice"/>).</summary>
    public Faction? Of { get; init; }

    /// <summary>What they want, in a sentence, for their page in the notebook.</summary>
    public string Wants { get; init; }

    /// <summary>What you notice about them, for their page (the Magpie's suspects are known by it).</summary>
    public string[] Facts { get; init; } = Array.Empty<string>();

    /// <summary>How often their chest isn't theirs to sell (<see cref="Chest.Owner"/>): the mark goes cold on it.</summary>
    public float Steals { get; init; }

    /// <summary>A ghost: drawn pale and half there (Sir Aldwin).</summary>
    public bool Ghost { get; init; }

    /// <summary>
    /// Someone who comes only while this holds (0.30: the officials, while their masters count you a friend): null, as
    /// any regular. Read every morning, so they stop coming the day it doesn't.
    /// </summary>
    public Func<GameState, bool> Comes { get; init; }

    /// <summary>The shop tier at which they start coming in.</summary>
    public int Tier { get; init; }

    /// <summary>The share of days they come in at all (1: every day; the King one day in four, the Maw one in ten).</summary>
    public float Visits { get; init; } = 1f;

    /// <summary>Where their chests come from, and how often each.</summary>
    public (string Origin, float Weight)[] From { get; init; } = Array.Empty<(string, float)>();

    /// <summary>What they say about a chest from each place, with {price} and the like. Where it came from is always true.</summary>
    public Dictionary<string, string[]> Stories { get; init; } = new();

    /// <summary>Their chests are packed in straw (<see cref="Chest.Packed"/>): everything stirs a strength later.</summary>
    public bool Packs { get; init; }

    /// <summary>What they tell you besides what they heard.</summary>
    public Clue States { get; init; }

    /// <summary>
    /// How often their chest holds a warded thing (<see cref="Chest.Warded"/>): sealed in the ward, silent and weightless,
    /// given up only to a Keywright who names it. Only sellers whose clue can tell you it's there bring them.
    /// </summary>
    public float Wards { get; init; }

    /// <summary>Their chests hang on a packhorse that spooks at a hard shake: nobody shakes them hard (Wenna's).</summary>
    public bool Packhorse { get; init; }

    /// <summary>Their chests are half rotten: this many seconds of shaking, all told, and no more (0: as long as you like).</summary>
    public float ShakeLimit { get; init; }

    /// <summary>Every strength you shake at after the first puts their price up by this share.</summary>
    public float ShakeFee { get; init; }

    /// <summary>They give back this share of the price if your slip turns out exactly right.</summary>
    public float Wager { get; init; }

    /// <summary>A kind of thing that's theirs by right: if any is inside, the best of it goes back to them, for a price they name.</summary>
    public Category? Heirloom { get; init; }

    /// <summary>The dearest thing inside is theirs.</summary>
    public bool TakesDearest { get; init; }

    /// <summary>Their price is whatever your slip says the chest holds (the King's game).</summary>
    public bool SlipPrice { get; init; }

    /// <summary>Buy from them, and everyone who comes to the counter after them today asks this much more (the Maw's gaze).</summary>
    public float Gaze { get; init; }

    /// <summary>What they say when their price goes up after a shake, with {price}.</summary>
    public string[] PriceUp { get; init; } = { Loc.T("{price}, now.") };

    /// <summary>What Bram calls them, when the first word of their name won't do.</summary>
    public string Nick { get; init; }

    /// <summary>How hard they shake it, for judgments that listen.</summary>
    public Shake Shakes { get; init; } = Shake.Hard;

    public Judgment Judge { get; init; } = Judgment.Ear;
    public Trade Trade { get; init; } = Trade.Fixed;

    float[] _kinds;

    /// <summary>
    /// How likely each kind of thing is in their chests, by Category: what their places hold, all told (or set directly,
    /// for the Stranger and the vault).
    /// </summary>
    public float[] Kinds
    {
        get => _kinds ??= Enumerable.Range(0, Categories.All.Length)
            .Select(k => From.Sum(f => f.Weight * Origins.Get(f.Origin).Kinds.ElementAtOrDefault(k)) / Math.Max(0.01f, From.Sum(f => f.Weight))).ToArray();
        init => _kinds = value;
    }

    /// <summary>How likely each chest colour is, common to epic.</summary>
    public float[] Colours { get; init; }

    /// <summary>What you learn about them, in a sentence.</summary>
    public string Habit { get; init; }

    /// <summary>For sellers with no stories (the Stranger, the vault): lines with {heft}, {colour}, {magic} and {price}.</summary>
    public string[] Pitches { get; init; } = Array.Empty<string>();
    public string[] Accepts { get; init; } = { Loc.T("Done."), Loc.T("Pleasure.") };
    public string[] Counters { get; init; } = { Loc.T("{price}, and that's my last word.") };
    public string[] Refusals { get; init; } = { Loc.T("More than that, surely.") };
    public string[] Leaves { get; init; } = { Loc.T("Forget it, then.") };

    /// <summary>What they say when you do what they won't stand for (their own dislike: a hard shake, a drop, the lodestone).</summary>
    public string[] Angry { get; init; } = { Loc.T("Careful! That'll cost you extra.") };

    /// <summary>What they say when you drop their chest in a season of fussy sellers, when a drop isn't their own dislike.</summary>
    public string[] Fussed { get; init; } = { Loc.T("Careful! That'll cost you extra.") };

    /// <summary>What they say when you drop their chest and they don't mind.</summary>
    public string[] Startled { get; init; } = Array.Empty<string>();

    /// <summary>What they say when you pass on their chest.</summary>
    public string[] Passes { get; init; } = Array.Empty<string>();

    /// <summary>Seconds they'll wait for an answer. 0: all day.</summary>
    public float Patience { get; init; }

    /// <summary>Drop their chest and they put the price up by half.</summary>
    public bool HatesDrop { get; init; }

    /// <summary>Put a magnet to their chest and they put the price up by half.</summary>
    public bool HatesMagnet { get; init; }

    /// <summary>Shake their chest hard and they put the price up by half.</summary>
    public bool HatesHardShake { get; init; }

    public float MagicChance { get; init; } = 0.05f;

    /// <summary>How often their chest has a false bottom (from the village shop on).</summary>
    public float HiddenChance { get; init; } = 0.08f;

    /// <summary>How often their chest is locked (from the village shop on).</summary>
    public float LockChance { get; init; } = 0.3f;

    /// <summary>The kinds they pass off fakes of: gilded iron for coin and gems, sand for spells.</summary>
    public Category[] Fakes { get; init; } = Array.Empty<Category>();

    /// <summary>How likely each thing of a kind they fake is the fake.</summary>
    public float FakeChance { get; init; }

    /// <summary>From the town shop on: how often each kind they bring comes two (or three) of a kind.</summary>
    public float PairChance { get; init; } = 0.08f;

    /// <summary>A kind they always bring: a lady her jewels.</summary>
    public Category? Always { get; init; }

    /// <summary>How often their chest has nothing in it at all.</summary>
    public float EmptyChance { get; init; }

    /// <summary>For judgments that listen: how often their chest holds something their shaking can't hear.</summary>
    public float MissRate { get; init; } = 0.55f;

    /// <summary>The chance each thing they bring is one quality better than its chest's colour suggests.</summary>
    public float QualityBoost { get; init; }

    /// <summary>For a fixed price: how much over what they think it's worth they ask.</summary>
    public (float Min, float Max) Markup { get; init; } = (Market.MarkupMin, Market.MarkupMax);

    /// <summary>For a haggle: the least they'll take, against what they think it's worth.</summary>
    public (float Min, float Max) Bottom { get; init; } = (Market.BottomMin, Market.BottomMax);

    /// <summary>Whether they shake it at all (and so what you can learn from what they say they heard).</summary>
    public bool Listens => Judge is Judgment.Ear or Judgment.Thorough or Judgment.Counter or Judgment.Miner;

    /// <summary>Soldiers, knights and dwarves: they bring a lot of weapons (a smithing name draws them).</summary>
    public bool Metalworker => Kinds[(int)Category.Arms] / Kinds.Sum() >= 0.3f;

    /// <summary>Whether they price each one of a kind they notice, or one of each kind.</summary>
    public bool Counts => Judge is Judgment.Counter or Judgment.Thorough or Judgment.Crook or Judgment.Jeweller;

    /// <summary>
    /// What this seller says is in the chest: what they heard, or (crooks, jewellers) what they packed. Never a Heartsong:
    /// only a Keywright hears one.
    /// </summary>
    public IEnumerable<Item> Notices(Chest chest) => (Judge switch
    {
        Judgment.Crook => chest.Items,
        Judgment.Jeweller => chest.Items.Where(i => i.Category == Category.Gems),
        Judgment.Thorough => chest.HeardAt(Shake.Gentle, plain: true).Union(chest.HeardAt(Shake.Hard, plain: true)),
        _ when Listens => chest.HeardAt(Shakes, plain: true),
        _ => Enumerable.Empty<Item>(),
    }).Where(i => !i.IsSong);

    /// <summary>How they put it in the ledger: how they judge a chest.</summary>
    public string JudgeWord => Judge switch
    {
        Judgment.Ear => Loc.T($"listens ({Categories.Adverb(Shakes)})"),
        Judgment.Thorough => Loc.T("listens gently, then hard"),
        Judgment.Counter => Loc.T("counts the rattles"),
        Judgment.Weight => Loc.T("goes by weight"),
        Judgment.Colour => Loc.T("goes by the chest's colour"),
        Judgment.Dreamer => Loc.T("dreams up a price"),
        Judgment.Miner => Loc.T("knows only weapons"),
        Judgment.Jeweller => Loc.T("knows only her own gems"),
        Judgment.Clueless => Loc.T("has no idea"),
        _ => Loc.T("knows exactly"),
    };

    /// <summary>
    /// How they came to their price, for the top of your slip: nobody says what's inside, so this is what to work from
    /// ("Penn priced what a gentle shake stirs"). One line of the slip, but for Grit's.
    /// </summary>
    public string Priced => Judge switch
    {
        // One line where it fits (the slip's 384px), so the line about what it values shows under it.
        Judgment.Ear => Loc.T($"{Who} priced what's heard at a {Categories.LevelName(Shakes)} shake."),
        Judgment.Thorough => Loc.T($"{Who} priced what's heard gently and hard."),
        Judgment.Counter => Loc.T($"{Who} priced each rattle at a {Categories.LevelName(Shakes)} shake alike."),
        Judgment.Weight => Loc.T($"{Who} priced it by the pound."),
        Judgment.Colour => Loc.T($"{Who} priced it by the chest's colour."),
        Judgment.Dreamer => Loc.T($"{Who}'s price came from a dream."),
        Judgment.Miner => Loc.T($"{Who} priced a hard shake: weapons in full, gems as diamonds, the rest at half."),
        Judgment.Jeweller => Loc.T($"{Who} priced only her own gems."),
        Judgment.Clueless => Loc.T($"{Who} has no idea what's inside."),
        _ => Loc.T($"{Who} priced everything in it as real."),
    };

    static readonly string[] Titles = { "Little", "Widow", "Old", "Mother", "Brother", "Archmage", "Lady", "Madame", "Sir", "The", "Aunt", "Master", "Lord", "Countess", "Sergeant", "Mistress" }; // loc: keys

    string _short;

    /// <summary>
    /// What Bram calls them: "Penn", "Hob", "Bramble". Each seller names theirs, so a translation can say what it is;
    /// one made up on the spot (Nim at his pack) falls back to the name without its title.
    /// </summary>
    public string Short
    {
        get => _short ?? Nick ?? Name.Split(' ').First(w => !Titles.Contains(w));
        init => _short = value;
    }

    /// <summary>The same, to start a sentence with: "The Maw asks".</summary>
    public string Who => char.ToUpperInvariant(Short[0]) + Short[1..];
}

public static class Sellers
{
    static readonly string[] Shrug = { Loc.T("Suit yourself."), Loc.T("Your loss."), Loc.T("Another time, then.") };

    /// <summary>What a seller says when they don't mind a drop (or their own lines, if they have them).</summary>
    static readonly string[] Startle = { Loc.T("Careful with that!"), Loc.T("Mind the floor!"), Loc.T("Easy!") };

    // Chest colours by shop: villagers bring wood, the court brings purple. (0.20: Thornwick's and Crownhold's a little
    // finer, so each move inward pays about as well as the last, and a shorter run doesn't stall in the middle.)
    static readonly float[] Stall = { 0.75f, 0.25f, 0, 0 }, Village = { 0.1f, 0.55f, 0.35f, 0 },
        Town = { 0, 0.15f, 0.6f, 0.25f }, Emporium = { 0, 0, 0.45f, 0.55f };

    static Dictionary<string, string[]> Tell(params (string Origin, string[] Lines)[] stories) => stories.ToDictionary(s => s.Origin, s => s.Lines);

    public static readonly SellerDef[] All =
    {
        // Brindle Crossroads: a millpond boy, a soldier's widow, a farmer, the innkeeper, and a delver back from the Barrow.
        new()
        {
            Key = "pip", Look = "tam", Name = Loc.T("Pip Hollow"), Short = Loc.T("Pip"), Tier = 1, Judge = Judgment.Clueless, Trade = Trade.Offer, EmptyChance = 0.3f, Colours = Stall,
            Of = Faction.Commons, From = new[] { ("millpond", 3f), ("attic", 1f) },
            Wants = Loc.T("A coin for his mother, and to be left alone. He has the Key-mark, and doesn't know what it means."),
            Stories = Tell(
                ("millpond", new[] { Loc.T("Fished it out of the millpond! Mum says never shake things, so I didn't. What'll you give me?"),
                                     Loc.T("It was stuck in the mill wheel. Mum does the mill's washing, and it came up in a sheet! How much will you give me?") }),
                ("attic", new[] { Loc.T("It was under Gran's bed. She says I can sell it if it buys Mum's soap. What's it worth?"),
                                  Loc.T("Found it in the hayloft, behind the cat. The cat didn't want it. How much?") })),
            Habit = Loc.T("A washerwoman's boy of nine who fishes chests out of the millpond and never shakes them: he has no idea what's in one, and " +
                    "asks you to name a price. A few coins will do, but a millpond chest is sometimes empty."),
            Accepts = new[] { Loc.T("Yes! Mum can have new soap!"), Loc.T("Really? Thanks, mister!") },
            Refusals = new[] { Loc.T("Is that all? Um... a bit more?"), Loc.T("Mum says don't take the first offer.") },
            Leaves = new[] { Loc.T("Aww. I'll ask at the Crossed Keys."), Loc.T("Never mind. Gran will have it back.") },
            Passes = new[] { Loc.T("Aww. Okay."), Loc.T("I'll throw it back, then.") },
            Startled = new[] { Loc.T("Hey! That's MY chest!"), Loc.T("Whoa! Do it again!") },
            Fussed = new[] { Loc.T("You dropped it! Now it costs more!") },
        },
        new()
        {
            Key = "ada", Look = "penn", Name = Loc.T("Ada Kettle"), Short = Loc.T("Ada"), Tier = 1, Shakes = Shake.Gentle, Colours = Stall,
            Of = Faction.Commons, From = new[] { ("kitbag", 3f), ("attic", 1f) },
            Wants = Loc.T("To buy her son Jory out of the Guild's debt-house, where he's working off his father's tithe."),
            Facts = new[] { Loc.T("Right-handed."), Loc.T("Hands red from the washtub, no ink."), Loc.T("Wooden clogs.") },
            Stories = Tell(
                ("kitbag", new[] { Loc.T("My Rafe's soldiering chest. Only a gentle shake: I couldn't bear to hear his things rattle. {price}, for Jory's debt?"),
                                   Loc.T("Rafe carried this through two wars. {price} would help my Jory.") }),
                ("attic", new[] { Loc.T("From the attic, dear, under Rafe's old saddle. {price}? Every coin's for Jory.") })),
            Habit = Loc.T("A soldier's widow selling her husband's things to free her son from the Guild's debt-house. She only ever shakes a chest " +
                    "gently, so what's heavy (a weapon) or soft (a cloak) never makes a sound for her, and she doesn't price it. A soldier's " +
                    "kitbag usually holds both."),
            Accepts = new[] { Loc.T("Thank you, dear. That's a week off Jory's debt."), Loc.T("Bless you. Rafe would have liked you.") },
            Leaves = new[] { Loc.T("I'm sorry, dear, I can't stand here all day."), Loc.T("I'll come back when you're less busy.") },
            Passes = new[] { Loc.T("That's all right, dear. I'll keep it a while longer."), Loc.T("Never mind. It was good of you to look.") },
            Startled = new[] { Loc.T("Oh! Gently, dear, please."), Loc.T("Oh my. Those were Rafe's things.") },
            Fussed = new[] { Loc.T("Those were Rafe's things! I'm sorry, but I'll want more now.") },
        },
        new()
        {
            Key = "garrick", Look = "hob", Name = Loc.T("Garrick Thorne"), Short = Loc.T("Garrick"), Tier = 1, Shakes = Shake.Hard, Trade = Trade.Haggle, Colours = Stall,
            Of = Faction.Commons, From = new[] { ("field", 1f) },
            Wants = Loc.T("His rent paid before the Guild's bailiffs come for Brindle Field, which his family's ploughed for two hundred years."),
            Stories = Tell(("field", new[] { Loc.T("Ploughed it up on Brindle Field, where the last battle was. {price}, and it's the rent!"),
                                             Loc.T("Every spring that field coughs up another one. Turned the plough right round. {price}: the bailiffs won't wait.") })),
            Habit = Loc.T("A farmer who ploughs chests up out of Brindle Field, the old battlefield, and shakes them hard, so a clanking weapon drowns " +
                    "out any coin for him. He haggles, and starts at about twice what he'll take."),
            Accepts = new[] { Loc.T("Deal! That's the rent, near enough."), Loc.T("Done! The bailiffs can whistle.") },
            Counters = new[] { Loc.T("Hmm. {price}, and that's my last word."), Loc.T("The rent won't pay itself. {price}.") },
            Leaves = new[] { Loc.T("Daylight robbery! Forget it."), Loc.T("I've cows to milk. Good day.") },
            Passes = new[] { Loc.T("Your loss. Field's full of them."), Loc.T("Suit yourself. I'll be back after the next ploughing.") },
            Startled = new[] { Loc.T("Steady on! It's not a sack of spuds."), Loc.T("Ha! Mind your toes.") },
            Fussed = new[] { Loc.T("Easy with my chest! That'll cost you extra.") },
        },
        new()
        {
            Key = "bettany", Look = "mags", Name = Loc.T("Bettany Crane"), Short = Loc.T("Bettany"), Tier = 1, Shakes = Shake.Firm, Judge = Judgment.Counter, Markup = (1.3f, 1.6f), Colours = Stall,
            Of = Faction.Commons, From = new[] { ("tab", 1f) },
            Wants = Loc.T("To keep the Crossed Keys, and her secrets. Everyone's news passes through her taproom first."),
            Facts = new[] { Loc.T("Pulls pints left-handed."), Loc.T("Ink on her fingers from the tally."), Loc.T("Wears house slippers; never rides.") },
            Stories = Tell(("tab", new[] { Loc.T("Some adventurer left this for his tab at the Crossed Keys. So much a rattle, as ever: {price}."),
                                           Loc.T("Three nights' ale and a broken bench, paid in chest. I charge by the rattle, love: {price}.") })),
            Habit = Loc.T("Keeps the Crossed Keys at the crossroads, takes chests for unpaid tabs, and hears everything first. She gives one a firm " +
                    "shake, counts the rattles, and charges the same for each, whatever's rattling: a goblin cleaver costs what a purse of coin does."),
            Accepts = new[] { Loc.T("Pleasure doing business!"), Loc.T("Done, love. Come by for a pint, and the news.") },
            Leaves = new[] { Loc.T("I've a taproom to run, love. Another time.") },
            Passes = new[] { Loc.T("Suit yourself, love. Someone'll have it."), Loc.T("Fair enough. I'll stick it behind the bar.") },
            Startled = new[] { Loc.T("Oi! I'll want that back in one piece."), Loc.T("Mind my floor, love.") },
            Fussed = new[] { Loc.T("Oi! You break it, you pay for it, love. Half again.") },
        },
        new()
        {
            Key = "tobin", Look = "dorrin", Name = Loc.T("Tobin Reed"), Short = Loc.T("Tobin"), Tier = 1, Judge = Judgment.Colour, ShakeLimit = 2f, Markup = (0.9f, 1.15f), Visits = 0.5f, Colours = Stall,
            Of = Faction.Commons, From = new[] { ("barrow", 1f) },
            Wants = Loc.T("One big find, to clear the Guild's claim on his father's tithe and never go down the Barrow again."),
            Stories = Tell(("barrow", new[] { Loc.T("Dragged it up from the Barrow's second vault. The damp's got into the wood: go easy, it won't stand much. {price}."),
                                              Loc.T("From the Barrow. Lost my torch and my nerve down there, but I kept the chest. It's half rotten. {price}, for the Guild."),
                                              Loc.T("I ran from a barrow-wight for this, so it's yours for {price}. Careful: it's falling apart.") })),
            Habit = Loc.T("A young delver who goes down the Barrow to pay what the Guild says his father owed. Prices a chest by its colour and never " +
                    "shakes it: Barrow chests are so rotten they stand about two seconds of shaking before they start to come apart. Grave " +
                    "goods: coin, weapons, gems."),
            Accepts = new[] { Loc.T("That's another week the Guild leaves me be."), Loc.T("Right. Back to the Barrow.") },
            Leaves = new[] { Loc.T("Sorry, I've a barrow to get back to.") },
            Passes = new[] { Loc.T("Fair enough. Someone at the Crossed Keys will want it."), Loc.T("Oh well. More where that came from.") },
            Startled = new[] { Loc.T("Careful, it's rotten!"), Loc.T("Whoa! It'll fall apart!") },
            Fussed = new[] { Loc.T("It's held together with damp and hope! You'll pay more for that.") },
        },

        // Thornwick Market: a goblin, a bounty-huntress, a druid, the abbey's almoner, a smuggler, the matchmaker, and the Magpie's runner.
        new()
        {
            Key = "nettle", Look = "snikkit", Name = Loc.T("Nettle"), Short = Loc.T("Nettle"), Tier = 2, Shakes = Shake.Gentle, ShakeFee = 0.1f, MissRate = 0.6f, Markup = (1.25f, 1.5f), Visits = 0.5f, Colours = Village,
            From = new[] { ("warren", 1f) },
            Wants = Loc.T("Her clan cleared of every theft in Thornwick, and a burrow with a door: a goblin's worth is the chests she keeps."),
            Stories = Tell(("warren", new[] { Loc.T("From our burrow. Not stolen! Goblins get the blame for everything. {price}. Looking is free. Looking more, not free!"),
                                              Loc.T("Nettle's clan dug it up fair and square. {price}. You want to shake more? You pay more!"),
                                              Loc.T("Burrow chest. Smells of goblin. Is fine. {price}, and more every time you shake it different.") })),
            PriceUp = new[] { Loc.T("Ooh, you like! Price go up: {price}."), Loc.T("More shaking, more gold! {price}."), Loc.T("Hehe. {price} now.") },
            Habit = Loc.T("A goblin of the hills, trading chests out of her clan's burrow (fairly: she'll tell you so). She only shakes gently, so she " +
                    "never prices what a gentle shake can't stir, and goblins charge for everything, looking included: each strength you try " +
                    "after your first puts her price up a tenth."),
            Accepts = new[] { Loc.T("Shiny! Shiny!"), Loc.T("Hehehe. Nettle's burrow gets a door!") },
            Leaves = new[] { Loc.T("Pah! Nettle has other customers.") },
            Passes = new[] { Loc.T("Pah! Nettle sell it to someone with eyes."), Loc.T("Your loss. Nettle's gain. Nettle always gains.") },
            Startled = new[] { Loc.T("Eek! Nettle's chest!"), Loc.T("Hehe, again! Again!") },
            Fussed = new[] { Loc.T("You drop Nettle's chest? Price go up!") },
        },
        new()
        {
            Key = "rook", Look = "wenna", Name = Loc.T("Rook Varga"), Short = Loc.T("Rook"), Tier = 2, Judge = Judgment.Weight, States = Clue.Pounds, Packhorse = true,
            Markup = (0.9f, 1.1f), Colours = Village, LockChance = 0.8f,
            Of = Faction.Wardens, From = new[] { ("camp", 2f), ("den", 1f) },
            Wants = Loc.T("The Magpie's bounty, the biggest the Wardens have ever posted."),
            Facts = new[] { Loc.T("Draws her bow left-handed."), Loc.T("A hunter's clean hands: no ink."), Loc.T("Rides in boots.") },
            Stories = Tell(
                ("camp", new[] { Loc.T("Took it off an outlaws' camp on the forest road, for the Wardens' bounty. My scale says {pounds}. {price}. Gently: the mare spooks."),
                                 Loc.T("The outlaws won't be needing it. {pounds} on my scale, and I price by the pound: {price}.") }),
                ("den", new[] { Loc.T("Found it in a wolf den. The wolves had eaten whoever was carrying it. {pounds} on my scale. {price}."),
                                Loc.T("Wolves don't hoard, but their dinners did. {pounds}, to the half pound. {price}. Easy with it: the mare.") })),
            Habit = Loc.T("A huntress who takes the Wardens' bounties, and sells what the outlaws had. She weighs every chest on her game scale and " +
                    "prices it by the pound, so light things (spells, gems) come cheap. Her chests hang on a mare who bolts at a hard shake: " +
                    "take the weight of what you heard from the scale's, and what's left is what never stirred."),
            Accepts = new[] { Loc.T("Done."), Loc.T("Good hunting.") },
            Leaves = new[] { Loc.T("The mare's restless. I'm off.") },
            Passes = new[] { Loc.T("No matter. It'll sell at the next market."), Loc.T("Fair enough.") },
            Startled = new[] { Loc.T("Easy! You'll have the mare bolting."), Loc.T("Steady.") },
            Fussed = new[] { Loc.T("Easy! You've upset the mare, and me. Half again.") },
        },
        new()
        {
            Key = "sorrel", Look = "bramble", Name = Loc.T("Mother Sorrel"), Short = Loc.T("Sorrel"), Tier = 2, Judge = Judgment.Dreamer, States = Clue.Dream, Wards = 0.5f, Markup = (1.2f, 1.2f),
            Colours = Village, HiddenChance = 0f,
            Of = Faction.Wood, From = new[] { ("wood", 1f) },
            Wants = Loc.T("To keep the Guild's axes out of the Old Wood."),
            Facts = new[] { Loc.T("Right-handed."), Loc.T("Fingers green from her herbs."), Loc.T("Goes barefoot, always.") },
            Stories = Tell(("wood", new[] { Loc.T("The Old Wood let this one go. I slept beside it and dreamt of {dream}. The oaks whispered its price: {price}."),
                                            Loc.T("Moss grew on it for a hundred years. Last night I dreamt of {dream}, and of a price: {price}.") })),
            Habit = Loc.T("The Old Wood's druid at Thornwick. Her price comes to her in a dream, and she always dreams true, in signs: of what the Wood " +
                    "warded in the chest, or else of the dearest thing in it. A magpie is coin, a thorn a weapon, a raven's wing a cloak, a river " +
                    "a potion, an owl a spell, a star a gem. If you can't hear what she dreamt of, it's warded: name it."),
            Accepts = new[] { Loc.T("As the Wood wills."), Loc.T("The oaks approve.") },
            Leaves = new[] { Loc.T("The Wood is calling me home.") },
            Passes = new[] { Loc.T("The Wood is patient. So am I."), Loc.T("Then it wasn't meant for you. Not yet.") },
            Startled = new[] { Loc.T("Mind the moss, child."), Loc.T("The Wood felt that.") },
            Fussed = new[] { Loc.T("The Wood felt that, child. So will your purse.") },
        },
        new()
        {
            Key = "cadoc", Look = "ansel", Name = Loc.T("Brother Cadoc"), Short = Loc.T("Cadoc"), Tier = 2, Shakes = Shake.Firm, HatesHardShake = true, MissRate = 0.55f, Markup = (1.8f, 2.1f), Colours = Village,
            Of = Faction.Abbey, From = new[] { ("crypt", 1f) },
            Wants = Loc.T("Bread for the poor at the abbey gate, and the Unopened kept off its steps."),
            Stories = Tell(("crypt", new[] { Loc.T("From the crypt of Saint Ide, by the Abbot's leave. A firm shake, but not hard, please: the saints are resting. {price}, for the poor."),
                                             Loc.T("A brother's reliquary, three hundred years asleep. {price} feeds the poor at our gate for a week.") })),
            Habit = Loc.T("The almoner of the abbey of Saint Ide, selling the crypt's chests to feed the poor at its gate. He shakes a chest firmly and " +
                    "won't have it shaken hard (half again if you do), so the holy cloaks a crypt chest is full of never make a sound for him, " +
                    "and go unpriced."),
            Accepts = new[] { Loc.T("Bless you. The gate eats tonight."), Loc.T("Saint Ide thanks you.") },
            Angry = new[] { Loc.T("Brother! The saints! Half again, for the sacrilege.") },
            Leaves = new[] { Loc.T("Forgive me, I'm due at vespers.") },
            Passes = new[] { Loc.T("Another day, perhaps. The poor are patient. They have to be."), Loc.T("Peace be with you.") },
            Startled = new[] { Loc.T("Brother! Gently!"), Loc.T("Oh! Saint Ide preserve us.") },
            Fussed = new[] { Loc.T("Brother! The saints! Half again, for the sacrilege.") },
        },
        new()
        {
            Key = "corvin", Look = "fennick", Name = Loc.T("Corvin Tallow"), Short = Loc.T("Corvin"), Tier = 2, Shakes = Shake.Firm, Packs = true, Trade = Trade.Haggle, HiddenChance = 0.45f,
            Bottom = (0.95f, 1.15f), Colours = Village, Steals = 0.15f,
            Of = Faction.Lockless, From = new[] { ("road", 1f) },
            Wants = Loc.T("One more good season on the smugglers' road, and then a boat, and the sea."),
            Stories = Tell(("road", new[] { Loc.T("Packed in straw, so nothing clinked at the Wardens' toll. {price}, and no questions."),
                                            Loc.T("Came up the smugglers' road. Don't ask whose. The straw stays: it muffles everything. {price}.") })),
            Habit = Loc.T("Runs brandy and chests past the Wardens' toll on the forest road, every chest packed in straw so nothing clinks: everything " +
                    "in it needs a strength harder to stir, and cloaks and spells won't stir at all. A firm shake, never a drop, and smugglers love " +
                    "a false bottom. Haggles. Now and then a chest he carries isn't his to sell."),
            Accepts = new[] { Loc.T("Pleasure. We never met."), Loc.T("Done. Mind the Wardens.") },
            Counters = new[] { Loc.T("{price}, and you're robbing me."), Loc.T("Tch. {price}, final.") },
            Leaves = new[] { Loc.T("Hmph. Plenty of other buyers."), Loc.T("I've a wagon to catch.") },
            Passes = new[] { Loc.T("Your loss. I was never here."), Loc.T("Suit yourself. Forget my face.") },
            Startled = new[] { Loc.T("Quiet! Do you want the Wardens round?"), Loc.T("Easy. That's straw, not stone.") },
            Fussed = new[] { Loc.T("Quiet, you fool! That noise will cost you.") },
        },
        new()
        {
            Key = "hesper", Look = "gerda", Name = Loc.T("Aunt Hesper"), Short = Loc.T("Hesper"), Tier = 2, Shakes = Shake.Hard, States = Clue.Dowry, Wards = 0.5f, MissRate = 0f,
            Markup = (1.15f, 1.35f), Colours = Village, HiddenChance = 0f,
            Of = Faction.Commons, From = new[] { ("dowry", 1f) },
            Wants = Loc.T("To marry off the whole county, and to know whose chests are whose: a matchmaker keeps every family's accounts."),
            Facts = new[] { Loc.T("Right-handed."), Loc.T("Ink on her fingers from the marriage contracts."), Loc.T("Rides her pony in boots, village to village.") },
            Stories = Tell(("dowry", new[] { Loc.T("A dowry, love, from a wedding that never was: {clue}, as ever. {price}."),
                                             Loc.T("The groom ran off to the Barrow. Her dowry: {clue}. {price}. I've another match for her already.") })),
            Habit = Loc.T("Thornwick's matchmaker, selling dowries from weddings that never happened. A Thornwick dowry always holds coin for luck, a " +
                    "cloak for warmth and a potion for health, and a bride often wards one of the three: whichever you can't hear is the warded " +
                    "one. Hesper shakes hard and prices what she hears, never what's warded."),
            Accepts = new[] { Loc.T("May it bring you luck."), Loc.T("Mind you use it well, love.") },
            Leaves = new[] { Loc.T("I've a match to make, love. Toodle-oo.") },
            Passes = new[] { Loc.T("Ah well. There's always another wedding."), Loc.T("Not to your taste? Nor was the groom to hers.") },
            Startled = new[] { Loc.T("Mind the bride's things, love!"), Loc.T("Ooh! Careful.") },
            Fussed = new[] { Loc.T("That's a bride's dowry, love! It'll cost you more now.") },
        },
        new()
        {
            Key = "flick", Look = "stranger", Name = Loc.T("Flick"), Short = Loc.T("Flick"), Tier = 2, Judge = Judgment.Colour, Patience = 20f, Steals = 0.85f, Visits = 0.5f,
            Markup = (0.55f, 0.7f), Colours = Village,
            Of = Faction.Lockless, From = new[] { ("pocket", 1f) },
            Wants = Loc.T("To be paid, to be fed, and never to be caught. Runs chests for the Magpie."),
            Stories = Tell(("pocket", new[] { Loc.T("It came my way, didn't it. {price}, quick. I can't stand about, mister."),
                                              Loc.T("Don't ask where it's from and I won't ask what you'll do with it. {price}. Twenty seconds."),
                                              Loc.T("A {colour} chest for {price}. Cheap, 'cause I'm in a hurry. Yes or no?") })),
            Habit = Loc.T("A pickpocket of twelve who runs chests for the Magpie: cheap, priced by colour, and in a hurry (twenty seconds). Most of what " +
                    "Flick sells isn't Flick's to sell: lay a hand on it, and your mark goes cold. From Thornwick in, you can send for the Wardens."),
            Accepts = new[] { Loc.T("Ta. Never saw you."), Loc.T("Done. I'm off.") },
            Leaves = new[] { Loc.T("Too slow, mister. I'm gone.") },
            Passes = new[] { Loc.T("Your loss."), Loc.T("Someone'll have it.") },
            Startled = new[] { Loc.T("Oi! Quiet!"), Loc.T("Careful! People are looking.") },
            Fussed = new[] { Loc.T("Drop it again and the price goes up. And the Wardens come running.") },
        },

        // 0.30's officials: they come only while their masters count you a friend (docs/stories.md).
        new()
        {
            Key = "marrow", Look = "vane", Name = Loc.T("Bailiff Marrow"), Short = Loc.T("Marrow"), Tier = 2, Judge = Judgment.Colour, Steals = 0.35f, Visits = 0.6f,
            Markup = (0.7f, 0.85f), Colours = Village, Of = Faction.Guild, From = new[] { ("seized", 1f) }, Comes = s => s.Friend(Faction.Guild),
            Wants = Loc.T("The Guild's books balanced, and its friends served first."),
            Stories = Tell(("seized", new[] { Loc.T("From a house the Guild sealed for tithe. A {colour} chest at the Guild's book price: {price}. The Guild doesn't haggle."),
                                              Loc.T("Taken for tithe, and sold to the Guild's friends first. The book says a {colour} chest is {price}, whatever's in it.") })),
            Habit = Loc.T("The Gilded Guild's bailiff, who sells what it took for tithe to the Guild's friends first, at its book price: the chest's colour, " +
                    "nothing more. A third of it was seized, not sold, and won't open for anyone: lay a hand on it. He comes while the Guild counts you a friend."),
            Accepts = new[] { Loc.T("Entered in the book. Good day."), Loc.T("The Guild thanks its friends.") },
            Leaves = new[] { Loc.T("I've three more houses to see to today.") },
            Passes = new[] { Loc.T("As you like. Someone else is a friend of the Guild."), Loc.T("Noted.") },
            Startled = new[] { Loc.T("That's Guild property until it's paid for."), Loc.T("Careful.") },
            Fussed = new[] { Loc.T("Damage to Guild property is added to the price.") },
        },
        new()
        {
            Key = "brandt", Look = "aldous", Name = Loc.T("Quartermaster Brandt"), Short = Loc.T("Brandt"), Tier = 2, Judge = Judgment.Weight, States = Clue.Pounds,
            Visits = 0.6f, Markup = (1.0f, 1.15f), Colours = Village, Of = Faction.Wardens, From = new[] { ("unclaimed", 1f) }, Comes = s => s.Friend(Faction.Wardens),
            Wants = Loc.T("The Wardens' store emptied, and the money in the widows' fund."),
            Stories = Tell(("unclaimed", new[] { Loc.T("Taken off a thief a year and a day ago, and nobody's claimed it. The Wardens' tariff is by the pound: {pounds}, so {price}."),
                                                 Loc.T("Unclaimed, a year and a day: it's the Crown's to sell now. {pounds} on the toll scale, at the tariff: {price}.") })),
            Habit = Loc.T("The Wardens' quartermaster at the Thornwick toll, selling what they took off thieves and nobody claimed in a year and a day. The " +
                    "Wardens' tariff is so much a pound, so heavy things come dear and light ones cheap. He comes while the Wardens count you a friend."),
            Accepts = new[] { Loc.T("For the widows' fund. Much obliged."), Loc.T("Sold, by the tariff.") },
            Leaves = new[] { Loc.T("The toll won't keep itself.") },
            Passes = new[] { Loc.T("It'll keep. It's kept a year already."), Loc.T("Fair enough.") },
            Startled = new[] { Loc.T("Easy with the Crown's property."), Loc.T("Steady.") },
            Fussed = new[] { Loc.T("That's breakage. Breakage goes on the price.") },
        },

        // Crownhold: an archmage and her apprentice, a dwarf, a noblewoman, a pawnbroker, the Archivist, a mudlark and a knight.
        new()
        {
            Key = "ilse", Look = "wren", Name = Loc.T("Ilse Varrow"), Short = Loc.T("Ilse"), Tier = 3, Judge = Judgment.Colour, States = Clue.Tag, Wards = 0.5f, MagicChance = 0.25f,
            HiddenChance = 0f, Markup = (1.3f, 1.5f), Colours = new[] { 0.2f, 0.6f, 0.2f, 0 },
            Of = Faction.Arcanum, From = new[] { ("arcanum", 1f) },
            Wants = Loc.T("To be an archmage one day. Lately, to know what her mistress is building in the cellar."),
            Stories = Tell(("arcanum", new[] { Loc.T("Archmage Orme's clearing out the Arcanum stores. The tag says: packed, {clue}. She says {price}."),
                                               Loc.T("This one's from the third cellar. I can read the tag, at least: {clue}. {price}, my mistress says.") })),
            Habit = Loc.T("The Arcanum's apprentice, clearing its stores for her mistress. Every chest still has its packing tag, listing each kind of " +
                    "thing packed in it, and the archmages warded the most dangerous: a kind on the tag that you can't hear is warded. Prices by " +
                    "the chest's colour, as she's told."),
            Accepts = new[] { Loc.T("Oh good! Don't tell my mistress I was nervous."), Loc.T("Thank you!") },
            Leaves = new[] { Loc.T("I'm late! She'll turn me into something.") },
            Passes = new[] { Loc.T("Oh. She won't be pleased."), Loc.T("That's all right. There's a whole cellar of them.") },
            Startled = new[] { Loc.T("Careful! Some of those are cursed!"), Loc.T("Eep!") },
            Fussed = new[] { Loc.T("She said to keep them in one piece! I'll have to ask more.") },
        },
        new()
        {
            Key = "orme", Look = "orrin", Name = Loc.T("Archmage Thessaly Orme"), Short = Loc.T("Orme"), Tier = 3, Judge = Judgment.Colour, Wager = 1f / 3, MagicChance = 0.2f, Markup = (1.3f, 1.5f), Colours = Town,
            Of = Faction.Arcanum, From = new[] { ("arcanum", 1f) },
            Wants = Loc.T("To understand the Locking well enough to undo it."),
            Stories = Tell(("arcanum", new[] { Loc.T("From the Arcanum's oldest stores. {price}. Tell me exactly what's in it, and a third comes back to you."),
                                               Loc.T("A {colour} chest: {price}. Read it true, Keywright, and I'll return a third. I study how your kind hears.") })),
            Habit = Loc.T("An archmage of the Arcanum who studies the Locking, and Keywrights. She prices by the chest's colour, and likes a wager: read " +
                    "her chest exactly right, every kind and every count, and she gives back a third of what you paid."),
            Accepts = new[] { Loc.T("Let us see."), Loc.T("We shall see what you heard.") },
            Leaves = new[] { Loc.T("My engine won't build itself. Good day.") },
            Passes = new[] { Loc.T("Prudent. Or timid. We shall never know."), Loc.T("Another time, Keywright.") },
            Startled = new[] { Loc.T("Hm. Bold."), Loc.T("Fascinating. Do that again.") },
            Fussed = new[] { Loc.T("That chest is older than your great-grandmother. A third more, I think.") },
        },
        new()
        {
            Key = "durgan", Look = "grit", Name = Loc.T("Durgan Ironhand"), Short = Loc.T("Durgan"), Tier = 3, Shakes = Shake.Hard, Judge = Judgment.Miner, Trade = Trade.Haggle, Bottom = (0.85f, 1f), Colours = Town,
            Of = Faction.Ironroot, From = new[] { ("hold", 1f) },
            Wants = Loc.T("Ironroot's crown, locked in the Royal Treasury since the Locking; and a buyer for dwarf-gold the Guild won't let him sell."),
            Stories = Tell(("hold", new[] { Loc.T("From the deep seam under Ironroot. {price}. Not a coin of it to the Guild, mind."),
                                            Loc.T("From Ironroot hold, sealed since my great-great-grandmother's day. {price}.") })),
            Habit = Loc.T("A dwarf trader of Ironroot Street, who knows a weapon when he hears one. He prices weapons in full, everything else at half, " +
                    "and takes every tinkle for a diamond. He shakes hard, and haggles."),
            Accepts = new[] { Loc.T("Done. Back to the deep."), Loc.T("Ha! Done.") },
            Counters = new[] { Loc.T("{price}. Any lower and I'll eat my helmet."), Loc.T("{price}, and that's stone-solid.") },
            Leaves = new[] { Loc.T("You take me for a fool? Good day.") },
            Passes = new[] { Loc.T("Hmph. Your loss, surface-dweller."), Loc.T("Back down the mine with it, then.") },
            Startled = new[] { Loc.T("Ha! Dwarf-made. It'll take worse than that."), Loc.T("Watch it.") },
            Fussed = new[] { Loc.T("Mind my stone-work! That'll cost you.") },
        },
        new()
        {
            Key = "isaura", Look = "corisande", Name = Loc.T("Lady Isaura Vell"), Short = Loc.T("Isaura"), Tier = 3, Judge = Judgment.Colour, States = Clue.Riddle, Wards = 0.5f, MagicChance = 0.15f,
            HiddenChance = 0f, Markup = (1.05f, 1.3f), Colours = Town,
            Of = Faction.Crown, From = new[] { ("strongroom", 1f) },
            Wants = Loc.T("To keep the Vell house standing, despite her brother's debts."),
            Stories = Tell(("strongroom", new[] { Loc.T("Grandmother's trick box. Her riddle's on the lid: \"{clue}\" {price}."),
                                                  Loc.T("From the Vell strongroom, before my brother finds it: {price}. Grandmother's riddle: \"{clue}\"") })),
            Habit = Loc.T("A noblewoman of an old house gone poor, selling her grandmother's trick boxes before her brother can. Each lid has " +
                    "grandmother's riddle about one thing inside, and if she warded something, the riddle is about that. Solve it: if you can't " +
                    "hear the answer, it's warded. Prices by colour."),
            Accepts = new[] { Loc.T("How delightful."), Loc.T("Grandmother would approve.") },
            Leaves = new[] { Loc.T("I have creditors at four. Do excuse me.") },
            Passes = new[] { Loc.T("Pity. Grandmother so loved a puzzle."), Loc.T("Another time, perhaps.") },
            Startled = new[] { Loc.T("Goodness!"), Loc.T("Do be careful, that's a Vell.") },
            Fussed = new[] { Loc.T("That is a Vell heirloom! The price has just gone up.") },
        },
        new()
        {
            Key = "brass", Look = "pell", Name = Loc.T("Silas Brass"), Short = Loc.T("Brass"), Tier = 3, Shakes = Shake.Firm, States = Clue.Articles, HatesDrop = true, HiddenChance = 0.35f,
            PairChance = 0.6f, MissRate = 0.45f, Markup = (2.3f, 2.7f), Colours = Town,
            Of = Faction.Commons, From = new[] { ("pawn", 1f) },
            Wants = Loc.T("His money; and, though he'll never say it, to be thought kind."),
            Stories = Tell(("pawn", new[] { Loc.T("Pledged twenty years ago on Lantern Lane and never redeemed. The ticket says {articles}. {price}."),
                                            Loc.T("Ticket four thousand and twelve: {articles}, it says. I price a lot kind by kind. {price}.") })),
            Habit = Loc.T("The pawnbroker of Lantern Lane, selling pledges nobody redeemed, often two or three of a kind. He prices one of each kind he " +
                    "hears and never counts how many. The pawn ticket counts every article pledged, false bottom and all, and he won't have pledges " +
                    "dropped: count what you hear against the ticket."),
            Accepts = new[] { Loc.T("Done. No refunds."), Loc.T("Sold, as heard.") },
            Angry = new[] { Loc.T("Not on my pledges! Half again, for the damage.") },
            Leaves = new[] { Loc.T("Time is money, and you're spending mine.") },
            Passes = new[] { Loc.T("Back on the shelf it goes."), Loc.T("As you like. Pledges keep.") },
            Startled = new[] { Loc.T("Mind the pledges!") },
            Fussed = new[] { Loc.T("Not on my pledges! Half again, for the damage.") },
        },
        new()
        {
            Key = "quire", Look = "quill", Name = Loc.T("Master Quire"), Short = Loc.T("Quire"), Tier = 3, Shakes = Shake.Firm, Judge = Judgment.Counter, States = Clue.Roll, Wards = 0.5f,
            HiddenChance = 0f, Markup = (1.85f, 2.15f), Colours = Town,
            Of = Faction.Crown, From = new[] { ("archive", 1f) },
            Wants = Loc.T("The Chest Roll kept true. Lately, to know why it says the Guild's chests aren't the Guild's."),
            Stories = Tell(("archive", new[] { Loc.T("From the Royal Archive's vaults, with its ward-roll: {clue}. Unclaimed since the Locking. So much a rattle: {price}."),
                                               Loc.T("The Chest Roll lists no owner for this one. The ward-roll reads {clue}, as far as I can make it out. I counted the rattles: {price}.") })),
            Habit = Loc.T("The Royal Archivist, keeper of the Chest Roll, selling unclaimed vault chests with their ward-rolls. A roll names what was " +
                    "warded in its chest, its letters faded by three hundred years: read the name, and name its kind on your slip. A blank roll " +
                    "means nothing was warded. He counts the rattles at a firm shake and charges the same for each."),
            Accepts = new[] { Loc.T("Duly recorded."), Loc.T("I'll enter it in the Roll.") },
            Leaves = new[] { Loc.T("The Archive closes at six. Good day.") },
            Passes = new[] { Loc.T("Very well. I'll file it under 'unsold'."), Loc.T("Noted.") },
            Startled = new[] { Loc.T("Careful! That's Crown property."), Loc.T("Oh dear.") },
            Fussed = new[] { Loc.T("That is Crown property! I'll have to add a fee for that.") },
        },
        new()
        {
            Key = "tilly", Look = "shelly", Name = Loc.T("Tilly Mudge"), Short = Loc.T("Tilly"), Tier = 3, Shakes = Shake.Hard, Fakes = new[] { Category.Scrolls }, FakeChance = 0.5f,
            Colours = new[] { 0.1f, 0.5f, 0.35f, 0.05f },
            Of = Faction.Commons, From = new[] { ("river", 1f) },
            Wants = Loc.T("To find the bell she hears tolling under the Aldwater on still nights."),
            Stories = Tell(("river", new[] { Loc.T("Out of the Aldwater mud at low tide! I'd bet my boots on what rustles. {price}."),
                                             Loc.T("The river gives and the river takes. Spellbooks, potions, who knows! {price}. Heard the bell again last night.") })),
            Habit = Loc.T("A mudlark who combs the Aldwater at low tide, shakes hard, and prices every rustle as a drowned wizard's spellbook. Her scroll " +
                    "cases are often full of river sand, which pours at the gentlest shake. Real parchment only rustles at a hard one."),
            Accepts = new[] { Loc.T("Mud's been good to me!"), Loc.T("The river provides.") },
            Leaves = new[] { Loc.T("Tide's turning. I'm off.") },
            Passes = new[] { Loc.T("Back in the barrow, then."), Loc.T("No skin off my nose.") },
            Startled = new[] { Loc.T("Ha! It's been through worse in the river."), Loc.T("Oop!") },
            Fussed = new[] { Loc.T("Oi! Mud or no mud, that's mine. It'll cost more now.") },
        },
        new()
        {
            Key = "rowan", Look = "aldous", Name = Loc.T("Sir Rowan Gale"), Short = Loc.T("Rowan"), Tier = 3, Judge = Judgment.Colour, Heirloom = Category.Arms, Markup = (1.1f, 1.3f), Colours = Town,
            Of = Faction.Crown, From = new[] { ("warchest", 1f) },
            Wants = Loc.T("His grandfather's sword, and his family's hall back from the Guild, which foreclosed on it."),
            Stories = Tell(("warchest", new[] { Loc.T("A Gale war chest. If my grandfather's sword's in it, it's mine, and I'll pay you {fee} for it. The rest is yours for {price}."),
                                                Loc.T("From the siege of Harrowgate. Any blade in it is my family's: {fee} for the best. {price} for the chest.") })),
            Habit = Loc.T("A knight without a war, whose family's hall the Guild foreclosed on. He's looking for his grandfather's sword. Prices by colour. " +
                    "If there are weapons in his chest, the best of them goes back to him for the price he names: value what's left."),
            Accepts = new[] { Loc.T("Honour is satisfied."), Loc.T("For the Gales!") },
            Leaves = new[] { Loc.T("I'm expected at the lists. Good day.") },
            Passes = new[] { Loc.T("Then I shall keep looking."), Loc.T("Another chest, another day.") },
            Startled = new[] { Loc.T("Have a care, sir!"), Loc.T("Steady!") },
            Fussed = new[] { Loc.T("Have a care with my grandfather's things! It'll cost you.") },
        },

        // The Royal Emporium, by the Gilded Gate: demons, a countess, a cartomancer, the Maw's fence, the King, and once in a while the Maw.
        new()
        {
            // 0.30: luck and hoards stop at purple (gold is rare), so Tick's purple chests can't come out better: more of
            // them are blue, which can, and he asks less over the colour (1.35-1.65 when a step better from purple was gold).
            Key = "tick", Look = "vexis", Name = Loc.T("Tick"), Short = Loc.T("Tick"), Tier = 4, Judge = Judgment.Colour, Patience = 12f, QualityBoost = 0.3f, Markup = (1.05f, 1.25f),
            Colours = new[] { 0, 0, 0.6f, 0.4f },
            From = new[] { ("gate", 1f) },
            Wants = Loc.T("Errands run, gold gone down the Gate, and one day off in three hundred years."),
            Stories = Tell(("gate", new[] { Loc.T("Fresh through the Gate, from the Maw's own hoard. {price}. And the sand's already running, mortal."),
                                            Loc.T("Errand for the Maw. A {colour} chest: {price}. Tick, tock.") })),
            Habit = Loc.T("An imp courier with an hourglass, selling from the Maw's hoard. Prices by colour and never looks inside, and gives you twelve " +
                    "seconds to decide. Things from the Maw's hoard are often better than their chest's colour."),
            Accepts = new[] { Loc.T("Delicious."), Loc.T("A pleasure, mortal.") },
            Leaves = new[] { Loc.T("Too slow! Back through the Gate I go.") },
            Passes = new[] { Loc.T("Pfft. Your loss, mortal."), Loc.T("Fine. Someone in the Emporium has more sense.") },
            Startled = new[] { Loc.T("Hee! Again!"), Loc.T("Clumsy mortal.") },
            Fussed = new[] { Loc.T("Careful, mortal! Breakages cost extra.") },
        },
        new()
        {
            Key = "sallow", Look = "malbranche", Name = Loc.T("Lord Sallow"), Short = Loc.T("Sallow"), Tier = 4, Judge = Judgment.Colour, TakesDearest = true, Markup = (0.5f, 0.65f), Colours = Emporium,
            From = new[] { ("gate", 1f) },
            Wants = Loc.T("To win every bargain he makes. He always has."),
            Stories = Tell(("gate", new[] { Loc.T("From the Maw's hoard. {price}, a pittance. My only condition: the dearest thing inside is mine."),
                                            Loc.T("A bargain, mortal: {price}. When it opens, I take the best of it. Everything else is yours.") })),
            Habit = Loc.T("A devil of the old law, who deals in bargains. His price is low, but the dearest thing inside is his. A chest of many middling " +
                    "things suits his deal; one grand thing and little else doesn't."),
            Accepts = new[] { Loc.T("Signed and sealed."), Loc.T("A pleasure doing business. It always is.") },
            Leaves = new[] { Loc.T("My time is worth more than yours, mortal.") },
            Passes = new[] { Loc.T("Another time. There is always another time."), Loc.T("How very wise of you. Or was it?") },
            Startled = new[] { Loc.T("Mind my bones."), Loc.T("Tsk.") },
            Fussed = new[] { Loc.T("Damaged goods cost more, mortal. Read the small print.") },
        },
        new()
        {
            Key = "skrae", Look = "skrae", Name = Loc.T("Skrae"), Short = Loc.T("Skrae"), Tier = 4, Judge = Judgment.Thorough, States = Clue.Echo, Wards = 0.5f, MissRate = 0f, MagicChance = 0.2f,
            HiddenChance = 0f, Markup = (1.2f, 1.45f), Colours = Emporium,
            From = new[] { ("gate", 1f) },
            Wants = Loc.T("Out: away from the Gate, and the Maw, for good. It has heard enough."),
            Stories = Tell(("gate", new[] { Loc.T("{price}, fairly. Then I clicked, like a bat, and listened. The echoes: {clue}."),
                                            Loc.T("I hear every coin in the realm, and every word the Maw says. {price}. My echoes: {clue}. Wards can't stop an echo.") })),
            Habit = Loc.T("A demon with ears like a bat, who has heard enough of the Maw. It shakes a chest gently, then hard, and prices what it hears, " +
                    "fairly. Then it clicks and listens to the echoes, which come back off everything inside, warded or not: round is coin, long a " +
                    "weapon, soft a cloak, hollow a potion, flat a spell, tiny a gem. An echo you can't hear is warded."),
            Accepts = new[] { Loc.T("Fair."), Loc.T("You won't regret it. Probably.") },
            Leaves = new[] { Loc.T("I hear someone counting gold elsewhere.") },
            Passes = new[] { Loc.T("I heard that coming."), Loc.T("Hm. Fair enough.") },
            Startled = new[] { Loc.T("Ow! My ears!"), Loc.T("Loud. Very loud.") },
            Fussed = new[] { Loc.T("My ears! That will cost you.") },
        },
        new()
        {
            Key = "vey", Look = "ermine", Name = Loc.T("Countess Vey"), Nick = Loc.T("the Countess"), Tier = 4, Judge = Judgment.Jeweller, Always = Category.Gems, HatesMagnet = true, Fakes = new[] { Category.Gems },
            FakeChance = 0.25f, Markup = (1.5f, 1.7f), Colours = Emporium,
            Of = Faction.Crown, From = new[] { ("strongroom", 1f) },
            Wants = Loc.T("To keep up appearances at court, a jewel at a time. Some of her gems are paste, and she knows which."),
            Stories = Tell(("strongroom", new[] { Loc.T("From my strongroom. My gems, and whatever my maid packed. {price}."),
                                                  Loc.T("The gems alone are worth {price}, I'm told. The rest I couldn't say. My maid packs.") })),
            Habit = Loc.T("A countess of the court who knows her own jewels and nothing else: she prices her gems at what she paid for them, and whatever " +
                    "else her maid packed comes free. Some of her gems are paste, and she won't have a lodestone near them (half again if you try)."),
            Accepts = new[] { Loc.T("Charming."), Loc.T("Do look after them.") },
            Angry = new[] { Loc.T("A lodestone? On MY gems? Half again, for the insult.") },
            Leaves = new[] { Loc.T("I'm expected at court. Good day.") },
            Passes = new[] { Loc.T("How very disappointing."), Loc.T("Well. Someone at court will have it.") },
            Startled = new[] { Loc.T("My gems!"), Loc.T("Must you?") },
            Fussed = new[] { Loc.T("My gems! Half again, for the fright.") },
        },
        new()
        {
            Key = "zell", Look = "oriel", Name = Loc.T("Madame Zell"), Short = Loc.T("Zell"), Tier = 4, Judge = Judgment.Colour, States = Clue.Cards, Wards = 0.5f, MagicChance = 0.15f, HiddenChance = 0f,
            Markup = (1.3f, 1.5f), Colours = Emporium,
            Of = Faction.Crown, From = new[] { ("court", 1f) },
            Wants = Loc.T("To be right, and to be paid for it. The cards don't lie; Madame Zell doesn't either."),
            Stories = Tell(("court", new[] { Loc.T("A courtier's chest, sold for his gambling debts. Three from the Locking's deck: {clue}. {price}."),
                                             Loc.T("The cards chose this one for you, dear. {clue}. A {colour} chest: {price}.") })),
            Habit = Loc.T("The court's cartomancer. She draws three cards from the Locking's own deck for each chest: a suit drawn twice is what the ward " +
                    "holds (Coins, Swords for weapons, Mantles for cloaks, Cups for potions, Wands for spells, Stars for gems); three different " +
                    "suits, and nothing's warded. Prices by colour."),
            Accepts = new[] { Loc.T("The cards are never wrong."), Loc.T("As it was foretold.") },
            Leaves = new[] { Loc.T("The cards say I'm wanted elsewhere.") },
            Passes = new[] { Loc.T("The cards did say you might."), Loc.T("As you wish, dear.") },
            Startled = new[] { Loc.T("I didn't see that coming. How rare."), Loc.T("Oh!") },
            Fussed = new[] { Loc.T("The cards warned me about you. Half again.") },
        },
        new()
        {
            Key = "gilt", Look = "silas", Name = Loc.T("Gilt"), Short = Loc.T("Gilt"), Tier = 4, Judge = Judgment.Crook, MissRate = 0f, Fakes = new[] { Category.Coins, Category.Gems }, FakeChance = 0.65f,
            HiddenChance = 0f, Colours = Emporium,
            From = new[] { ("gate", 1f) },
            Wants = Loc.T("The Maw's gilded iron sold as gold, so the Gate's gold buys more of the realm."),
            Stories = Tell(("gate", new[] { Loc.T("Out of the Maw's hoard, friend, through a crack in the Gate. I'm leaving town tonight, so {price}."),
                                            Loc.T("Hoard goods, very rare. Gold and gems, every one of them. For you, just {price}.") })),
            Habit = Loc.T("The Maw's fence. He knows exactly what's in his chests, because he packed the demon gold himself: gilded iron sold as gold and " +
                    "gems, cheap. What's real comes cheap; the fakes are worth nothing."),
            Accepts = new[] { Loc.T("Wonderful! No refunds."), Loc.T("A pleasure. I really must be going.") },
            Leaves = new[] { Loc.T("Can't stay. People to avoid.") },
            Passes = new[] { Loc.T("Your loss, friend. Your very great loss."), Loc.T("Suit yourself. I was never here.") },
            Startled = new[] { Loc.T("Easy, friend! Easy."), Loc.T("Whoa there.") },
            Fussed = new[] { Loc.T("Easy, friend! That'll cost a little extra.") },
        },
        new()
        {
            Key = "aldo", Look = "aldo", Name = Loc.T("Old Aldo"), Short = Loc.T("Aldo"), Tier = 4, Judge = Judgment.Clueless, SlipPrice = true, Visits = 0.25f, PairChance = 0.35f,
            MagicChance = 0.2f, HiddenChance = 0.25f, Colours = new[] { 0, 0, 0.3f, 0.7f },
            Of = Faction.Crown, From = new[] { ("treasury", 1f) },
            Wants = Loc.T("A Keywright he can trust with the one chest that matters."),
            Stories = Tell(("treasury", new[] { Loc.T("From an old house with a great many cellars. I've no idea what's in it. You tell me, and whatever you say it holds is my price."),
                                                Loc.T("Old things from an old house. Name what's inside: that's the price. Name it truly and I'll take a quarter off.") })),
            Habit = Loc.T("A tired old man in a plain cloak, with a very fine ring. He doesn't know what's in his chests: his price is whatever your slip " +
                    "says. Read it exactly and he takes a quarter off; read it wrong and you pay what it really holds, and a quarter more."),
            Accepts = new[] { Loc.T("Well. We shall see."), Loc.T("Hm. Let's see what you heard.") },
            Leaves = new[] { Loc.T("I'm needed elsewhere. Another day.") },
            Passes = new[] { Loc.T("Another day, then. I'll be back."), Loc.T("Hm. A careful one. Good.") },
            Startled = new[] { Loc.T("Steady. It's older than both of us."), Loc.T("Oh my.") },
            Fussed = new[] { Loc.T("Steady! That was my grandfather's. I'll want a little more.") },
        },
        new()
        {
            Key = "maw", Look = "maw", Name = Loc.T("Avarax, the Gilded Maw"), Nick = Loc.T("the Maw"), Tier = 4, Judge = Judgment.Colour, Gaze = 2f, Visits = 0.1f,
            Colours = new[] { 0, 0, 0.5f, 0.5f },
            From = new[] { ("gate", 1f) },
            Wants = Loc.T("Out."),
            Stories = Tell(("gate", new[] { Loc.T("A trifle from my hoard. One coin. All I ask is that you open it here, where I can see."),
                                            Loc.T("One coin, little Keywright. And then everyone in this city will want twice as much today. Isn't that fun?") })),
            Habit = Loc.T("The demon of greed himself, in a borrowed body dripping with rings. He sells a chest for one coin, but greed is catching: buy it, " +
                    "and everyone who comes to your counter after him that day asks twice as much."),
            Accepts = new[] { Loc.T("Yesss."), Loc.T("Open it. Let me see.") },
            Leaves = new[] { Loc.T("I have all the time in the world. You don't.") },
            Passes = new[] { Loc.T("Clever little thing."), Loc.T("Mm. Another day, then. There are so many days.") },
            Startled = new[] { Loc.T("Careful. I break things too."), Loc.T("Ha.") },
            Fussed = new[] { Loc.T("Careful. Greed always wants more.") },
        },
    };

    /// <summary>
    /// People who come only when a tale brings them (docs/stories.md): a Warden, a Guild clerk and its Mark-Finder, a
    /// mother, the Guild's timber factor, one of the fair folk, a gambler, a freed debtor. Never in the day's crowd.
    /// </summary>
    public static readonly SellerDef[] People =
    {
        new()
        {
            Key = "hollis", Look = "aldous", Name = Loc.T("Sergeant Hollis"), Short = Loc.T("Hollis"), Tier = 2, Visits = 0, Of = Faction.Wardens,
            Wants = Loc.T("Thieves caught and chests home; and to know why the Guild's foreclosed chests won't open for anyone."),
            Habit = Loc.T("The Wardens' sergeant at the Thornwick toll: posts the wanted notices, searches the market, pays a finder's share for a stolen chest sent home."),
        },
        // 0.34: the campaign's (TaleBook.Campaign.cs).
        new()
        {
            Key = "nell", Look = "shelly", Name = Loc.T("Nell Haddon"), Short = Loc.T("Nell"), Tier = 1, Visits = 0, Of = Faction.Commons,
            Wants = Loc.T("Her father's note paid, his yard kept, and him home."),
            Habit = Loc.T("Old Haddon the wheelwright's daughter, who keeps his yard at the crossroads going with her own two hands."),
        },
        new()
        {
            Key = "haddon", Look = Story.HaddonLook, Name = Loc.T("Old Haddon"), Short = Loc.T("Haddon"), Tier = 1, Visits = 0, Of = Faction.Commons,
            Wants = Loc.T("To make wheels, and never to sign anything again."),
            Habit = Loc.T("Brindle's wheelwright, who signed the Guild's paper for a new lathe."),
        },
        new()
        {
            Key = "tobias", Look = "quill", Name = Loc.T("Tobias Fenn"), Short = Loc.T("Tobias"), Tier = 3, Visits = 0, Of = Faction.Guild,
            Wants = Loc.T("To know where the tithe goes, and not to vanish for asking."),
            Habit = Loc.T("A clerk in the Gilded Guild's counting-house, with ink to the elbows and doubts he shouldn't have."),
        },
        new()
        {
            Key = "aubel", Look = "vane", Name = Loc.T("Mistress Aubel"), Short = Loc.T("Aubel"), Tier = 1, Visits = 0, Of = Faction.Guild,
            Wants = Loc.T("Every marked child in the county, for the Guild's counting-houses."),
            Habit = Loc.T("A Guild Mark-Finder, who tests children's hands at every fair and takes the marked ones \"into the Guild's care\"."),
        },
        // 0.32: the late tales' people.
        new()
        {
            Key = "wren", Look = "bramble", Name = Loc.T("Sister Wren"), Short = Loc.T("Wren"), Tier = 1, Visits = 0, Of = Faction.Abbey,
            Wants = Loc.T("To sing the Keyhold's song whole, once, before she dies."),
            Habit = Loc.T("The last singer of the Keyhold's choir, blind since the night the Guild burned her chapel. She hums a song that stops halfway."),
        },
        new()
        {
            Key = "tock", Look = "stranger", Name = Loc.T("Tock"), Short = Loc.T("Tock"), Tier = 3, Visits = 0, Of = Faction.Arcanum,
            Wants = Loc.T("To learn what's inside things, and then to be an apprentice."),
            Habit = Loc.T("Archmage Orme's brass automaton, which opens chests with a Keywright's mark on a brass plate. It wears a cloak."),
        },
        new()
        {
            Key = "edric", Look = "dorrin", Name = Loc.T("Sir Edric Ashcombe"), Short = Loc.T("Sir Edric"), Tier = 4, Visits = 0, Of = Faction.Crown,
            Wants = Loc.T("To know that the Gate held, and then to keep his oath somewhere useful."),
            Habit = Loc.T("A knight of King Aldric's guard, who locked himself into a chest at the Locking to guard the Crown's oath-book."),
        },
        // 0.32: the partners' own people.
        new()
        {
            Key = "mabry", Look = "shelly", Name = Loc.T("Mabry"), Short = Loc.T("Mabry"), Tier = 1, Visits = 0, Of = Faction.Commons,
            Wants = Loc.T("Drowned Fallowford's common chest home to its people, and her brother to write more often."),
            Habit = Loc.T("Hesk's sister, from the river town the Guild drowned when it opened the Aldwater weir."),
        },
        new()
        {
            Key = "vash", Look = "silas", Name = Loc.T("Orrin Vash"), Short = Loc.T("Vash"), Tier = 2, Visits = 0, Of = Faction.Guild,
            Wants = Loc.T("To keep the best stall on Lantern Lane, which the Guild gave him."),
            Habit = Loc.T("A Guild-licensed trader of Crownhold, who sells from the Brightwaters' old stall."),
        },
        new()
        {
            Key = "marta", Look = "gerda", Name = Loc.T("Marta Brightwater"), Short = Loc.T("Marta"), Tier = 1, Visits = 0, Of = Faction.Commons,
            Wants = Loc.T("Her family's stall on Lantern Lane back, before she's too old to stand at it."),
            Habit = Loc.T("Rosa's mother, who kept the best stall on Lantern Lane for thirty years and taught Rosa to haggle."),
        },
        new()
        {
            Key = "pemberton", Look = "aldous", Name = Loc.T("Pemberton"), Short = Loc.T("Pemberton"), Tier = 1, Visits = 0, Of = Faction.Crown,
            Wants = Loc.T("House Marchmont whole again, or as near as he can manage."),
            Habit = Loc.T("The Marchmonts' old steward, who stayed when the Guild took everything else, and keeps track of what it sold."),
        },
        new()
        {
            Key = "wynn", Look = "penn", Name = Loc.T("Wynn Hollow"), Short = Loc.T("Wynn"), Tier = 1, Visits = 0, Of = Faction.Commons,
            Wants = Loc.T("Her son kept out of the Guild's counting-houses."),
            Habit = Loc.T("Pip's mother, the mill's washerwoman."),
        },
        new()
        {
            Key = "caskett", Look = "hob", Name = Loc.T("Harl Caskett"), Short = Loc.T("Caskett"), Tier = 2, Visits = 0, Of = Faction.Guild,
            Wants = Loc.T("The Old Wood's oak, felled and sold, on the Guild's account."),
            Habit = Loc.T("The Gilded Guild's timber factor, with a deed for the Old Wood and a pen for whoever will witness it."),
        },
        new()
        {
            Key = "hawthorn", Look = "bramble", Name = Loc.T("The Hawthorn Lady"), Short = Loc.T("the Hawthorn Lady"), Tier = 2, Visits = 0, Of = Faction.Wood,
            Wants = Loc.T("The Wood's oaks left standing; and a promise, kept."),
            Habit = Loc.T("One of the fair folk of the Old Wood. She never used gold, and trades in promises."),
        },
        new()
        {
            Key = "cassian", Look = "silas", Name = Loc.T("Lord Cassian Vell"), Short = Loc.T("Cassian"), Tier = 3, Visits = 0, Judge = Judgment.Colour, Steals = 1f,
            Markup = (0.7f, 0.85f), Colours = Town, Of = Faction.Crown, From = new[] { ("strongroom", 1f) },
            Wants = Loc.T("Tonight's stake at cards. Tomorrow's, too."),
            Habit = Loc.T("Lady Isaura's brother, a gambler, selling his sister's chests cheap. They aren't his to sell."),
            Accepts = new[] { Loc.T("Splendid. The tables await."), Loc.T("Done. Not a word to my sister.") },
            Passes = new[] { Loc.T("Your loss. Someone will want it before the cards start."), Loc.T("Pity.") },
        },
        // 0.30's people.
        new()
        {
            Key = "jasper", Look = "fennick", Name = Loc.T("Jasper Wick"), Short = Loc.T("Jasper"), Tier = 1, Visits = 0, Of = Faction.Lockless,
            Wants = Loc.T("A good ear, a low price, and no licence: and never to see the inside of a Guild counting-house."),
            Habit = Loc.T("A hedge-wright: a Keywright without the Guild's licence, as Bram was for twenty years, trading from a cart."),
        },
        new()
        {
            Key = "aldwin", Look = "aldous", Name = Loc.T("Sir Aldwin Gale"), Short = Loc.T("Sir Aldwin"), Tier = 1, Visits = 0, Ghost = true,
            Wants = Loc.T("His blade in his family's hands, and then to rest."),
            Habit = Loc.T("A knight who fell on Brindle Field the year before the Locking, and has walked it since."),
        },
        new()
        {
            Key = "dunning", Look = "gerda", Name = Loc.T("Martha Dunning"), Short = Loc.T("Martha"), Tier = 2, Visits = 0, Of = Faction.Commons,
            Wants = Loc.T("Thornwick's roads free and its market open, and the Old Wood left standing."),
            Habit = Loc.T("A Thornwick weaver, standing for reeve against the Guild's money with nothing but a loom and a loud voice."),
        },
        new()
        {
            Key = "agathe", Look = "ansel", Name = Loc.T("Sister Agathe"), Short = Loc.T("Agathe"), Tier = 2, Visits = 0, Of = Faction.Abbey,
            Wants = Loc.T("Every chest left shut, as Saint Ide meant them; and to know who pays for the Unopened's bread."),
            Habit = Loc.T("A preacher of the Unopened, the abbey's zealots, who say every chest opened feeds the Maw."),
        },
        new()
        {
            Key = "tansy", Look = "mags", Name = Loc.T("Mother Tansy"), Short = Loc.T("Tansy"), Tier = 3, Visits = 0, Of = Faction.Lockless,
            Wants = Loc.T("Her money back, with a third on top, on the morning she said."),
            Habit = Loc.T("A moneylender of Lantern Lane, who lends to anyone short for the Guild and collects from everyone."),
        },
        new()
        {
            Key = "ashby", Look = "penn", Name = Loc.T("Mistress Ashby"), Short = Loc.T("Ashby"), Tier = 2, Visits = 0, Judge = Judgment.Colour, Markup = (1f, 1.1f),
            Colours = Village, Of = Faction.Commons, From = new[] { ("dowry", 1f) },
            Wants = Loc.T("Her daughter's dowry, which the Corlews say is theirs."),
            Habit = Loc.T("A Thornwick widow whose daughter was to marry a Corlew, until the wedding wasn't."),
            Accepts = new[] { Loc.T("There. The ward agrees with me.") },
            Passes = new[] { Loc.T("Then the court will hear it again.") },
        },
        new()
        {
            Key = "corlew", Look = "hob", Name = Loc.T("Hal Corlew"), Short = Loc.T("Corlew"), Tier = 2, Visits = 0, Judge = Judgment.Colour, Markup = (1f, 1.1f),
            Colours = Village, Of = Faction.Commons, From = new[] { ("dowry", 1f) },
            Wants = Loc.T("His sister's dowry, which the Ashbys say is theirs."),
            Habit = Loc.T("A Thornwick carter whose sister was to marry into the Ashbys, until the wedding wasn't."),
            Accepts = new[] { Loc.T("Knew it. Told them.") },
            Passes = new[] { Loc.T("Then the court will hear it again.") },
        },
        new()
        {
            Key = "jory", Look = "dorrin", Name = Loc.T("Jory Kettle"), Short = Loc.T("Jory"), Tier = 1, Visits = 0, Of = Faction.Commons,
            Wants = Loc.T("To pay you back for his freedom, and to see the Guild's debt-houses emptied."),
            Habit = Loc.T("Ada's son, two years in the Guild's debt-house for his father's tithe."),
        },
    };

    /// <summary>Anyone who can come to the counter, by key: a seller, or someone a tale brings.</summary>
    public static SellerDef Find(string key) => All.FirstOrDefault(s => s.Key == key) ?? People.FirstOrDefault(s => s.Key == key);

    /// <summary>Someone a tale brings to the counter (or the story's people, as they'd stand there): never null.</summary>
    public static SellerDef Person(string key) => Find(key) ?? new SellerDef { Key = key, Look = key, Name = Story.Name(key), Tier = 1, Visits = 0 };

    /// <summary>
    /// The picture to show for someone (assets/figures/{look}.png): the new cast wear the old cast's figures until they're
    /// drawn (docs/stories.md lists them), and the story's people their own.
    /// </summary>
    public static string LookOf(string key) => Find(key)?.Look ?? key;

    /// <summary>
    /// 0.28 replaced the cast (docs/world.md): the old regulars' keys, as saves and profiles from before kept them, and
    /// who trades their way now. Read through by <see cref="Profile"/> and <see cref="SaveData.FromJson"/>.
    /// </summary>
    public static readonly Dictionary<string, string> Renamed = new() // loc: keys
    {
        ["tam"] = "pip", ["penn"] = "ada", ["hob"] = "garrick", ["mags"] = "bettany", ["dorrin"] = "tobin",
        ["snikkit"] = "nettle", ["wenna"] = "rook", ["bramble"] = "sorrel", ["ansel"] = "cadoc", ["fennick"] = "corvin", ["gerda"] = "hesper",
        ["wren"] = "ilse", ["orrin"] = "orme", ["grit"] = "durgan", ["corisande"] = "isaura", ["pell"] = "brass", ["quill"] = "quire",
        ["shelly"] = "tilly", ["aldous"] = "rowan", ["vexis"] = "tick", ["malbranche"] = "sallow", ["ermine"] = "vey", ["oriel"] = "zell", ["silas"] = "gilt",
    };

    /// <summary>A key as it is now (an old regular's, renamed in 0.28; anything else as it was).</summary>
    public static string Current(string key) => key != null && Renamed.TryGetValue(key, out var now) ? now : key;

    /// <summary>Whose chest a stolen one is, as the Wardens' notice names them: someone from around the shop.</summary>
    public static string StolenFrom(Random rng, int tier) => rng.PickOne(tier switch
    {
        1 => new[] { Loc.T("a farmer at Brindle"), Loc.T("the Crossed Keys"), Loc.T("the miller's wife") },
        2 => new[] { Loc.T("a carter on the forest road"), Loc.T("a weaver in Thornwick"), Loc.T("a Thornwick family"), Loc.T("the abbey's gatehouse") },
        3 => new[] { Loc.T("a Crownhold merchant"), Loc.T("Lord Harrow's household"), Loc.T("a family on Lantern Lane"), Loc.T("a chapel by the Aldwater") },
        _ => new[] { Loc.T("a lady of the court"), Loc.T("a palace steward"), Loc.T("a banker of the Emporium"), Loc.T("a courtier's widow") },
    });


    /// <summary>A seller or a tale's person by key (an old regular's key finds who trades their way now).</summary>
    public static SellerDef Get(string key) => Find(Current(key)) ?? throw new KeyNotFoundException(key);

    /// <summary>What a seller says when you pass on their chest.</summary>
    public static string PassLine(Random rng, SellerDef seller = null) => rng.PickOne(seller?.Passes.Length > 0 ? seller.Passes : Shrug);

    /// <summary>What a seller says when you drop their chest and they don't mind.</summary>
    public static string StartleLine(Random rng, SellerDef seller) => rng.PickOne(seller.Startled.Length > 0 ? seller.Startled : Startle);

    /// <summary>What a seller says when you pass on their chest and a Heartsong bursts out of it to you.</summary>
    public static string BurstLine => Loc.T("Gods! It just... burst open! Keep the singing thing, then. The rest is going home with me.");

    public static string Number(int n) => n switch
    {
        1 => Loc.T("one"), 2 => Loc.T("two"), 3 => Loc.T("three"), 4 => Loc.T("four"), 5 => Loc.T("five"), 6 => Loc.T("six"),
        7 => Loc.T("seven"), 8 => Loc.T("eight"), _ => n.ToString(),
    };

    public static string Join(IReadOnlyList<string> words) =>
        words.Count <= 1 ? words.FirstOrDefault() ?? "" : Loc.T($"{Words.List(words.Take(words.Count - 1))} and {words[^1]}");
}
