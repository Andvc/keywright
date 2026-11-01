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

    static readonly string[] Titles = { "Little", "Widow", "Old", "Mother", "Brother", "Archmage", "Lady", "Madame", "Sir", "The" }; // loc: keys

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
        // Brindle Crossroads: villagers, an innkeeper, and a young adventurer back from the Barrow.
        new()
        {
            Key = "tam", Name = Loc.T("Little Tam"), Short = Loc.T("Tam"), Tier = 1, Judge = Judgment.Clueless, Trade = Trade.Offer, EmptyChance = 0.3f, Colours = Stall,
            From = new[] { ("millpond", 3f), ("attic", 1f) },
            Stories = Tell(
                ("millpond", new[] { Loc.T("I fished it out of the millpond! It's still dripping. Mum says never shake things, so I didn't. What'll you give me?"),
                                     Loc.T("It was stuck in the mill wheel. Dad says it's full of frogs. How much will you give me for it?") }),
                ("attic", new[] { Loc.T("It was under Gran's bed. She says I can sell it if I don't tell Mum. What's it worth?"),
                                  Loc.T("I found it in our hayloft, behind the cat. The cat didn't want it. How much?") })),
            Habit = Loc.T("A village boy who fishes chests out of the millpond and never shakes them: he has no idea what's in one, and asks you to " +
                    "name a price. A few coins will do, but a millpond chest is sometimes empty."),
            Accepts = new[] { Loc.T("Yay! I'm buying sweets!"), Loc.T("Really? Thanks, mister!") },
            Refusals = new[] { Loc.T("Is that all? Um... a bit more?"), Loc.T("Mum says don't take the first offer.") },
            Leaves = new[] { Loc.T("Aww. I'll ask someone else."), Loc.T("Never mind. Gran will have it back.") },
            Passes = new[] { Loc.T("Aww. Okay."), Loc.T("I'll throw it back, then.") },
            Startled = new[] { Loc.T("Hey! That's MY chest!"), Loc.T("Whoa! Do it again!") },
            Fussed = new[] { Loc.T("You broke it! Now it costs more!") },
        },
        new()
        {
            Key = "penn", Name = Loc.T("Widow Penn"), Short = Loc.T("Penn"), Tier = 1, Shakes = Shake.Gentle, Colours = Stall,
            From = new[] { ("kitbag", 3f), ("attic", 1f) },
            Stories = Tell(
                ("kitbag", new[] { Loc.T("My Tobb's old soldiering chest. I only gave it a gentle shake: I couldn't bear to hear his things rattle about. Would {price} do?"),
                                   Loc.T("Tobb carried this through two wars, dear. I couldn't bring myself to shake it more than gently. Is {price} fair?") }),
                ("attic", new[] { Loc.T("It was Tobb's mother's, from the attic. Just a gentle shake, dear: my wrists aren't what they were. {price}?") })),
            Habit = Loc.T("A soldier's widow selling her husband's things. She only ever shakes a chest gently, so what's heavy (a weapon) or soft " +
                    "(a cloak) never makes a sound for her, and she doesn't price it. A soldier's kitbag usually holds both."),
            Accepts = new[] { Loc.T("Thank you, dear. Truly."), Loc.T("Bless you. Tobb would have liked you.") },
            Leaves = new[] { Loc.T("I'm sorry, dear, I can't stand here all day."), Loc.T("I'll come back when you're less busy.") },
            Passes = new[] { Loc.T("That's all right, dear. I'll keep it a while longer."), Loc.T("Never mind. It was good of you to look.") },
            Startled = new[] { Loc.T("Oh! Gently, dear, please."), Loc.T("Oh my. Those were Tobb's things.") },
            Fussed = new[] { Loc.T("Those were Tobb's things! I'm sorry, but I'll want more now.") },
        },
        new()
        {
            Key = "hob", Name = Loc.T("Hob the Farmer"), Short = Loc.T("Hob"), Tier = 1, Shakes = Shake.Hard, Trade = Trade.Haggle, Colours = Stall,
            From = new[] { ("field", 1f) },
            Stories = Tell(("field", new[] { Loc.T("Turned up under the plough on the old battlefield. I gave it a good hard shake, like the hay. {price}, and that's a bargain!"),
                                             Loc.T("Every spring that field coughs up another one. Shook it like a sack of turnips. Say, {price}?") })),
            Habit = Loc.T("A farmer who ploughs chests up out of the old battlefield and shakes them hard, so a clanking weapon drowns out any " +
                    "coin for him. He haggles, and starts at about twice what he'll take."),
            Accepts = new[] { Loc.T("Deal! Wait till I tell the missus."), Loc.T("Done! Easy money.") },
            Counters = new[] { Loc.T("Hmm. {price}, and that's my last word."), Loc.T("The wife'd skin me. {price}.") },
            Leaves = new[] { Loc.T("Daylight robbery! Forget it."), Loc.T("I've cows to milk. Good day.") },
            Passes = new[] { Loc.T("Your loss. Field's full of them."), Loc.T("Suit yourself. I'll be back after the next ploughing.") },
            Startled = new[] { Loc.T("Steady on! It's not a sack of spuds."), Loc.T("Ha! Mind your toes.") },
            Fussed = new[] { Loc.T("Easy with my chest! That'll cost you extra.") },
        },
        new()
        {
            Key = "mags", Name = Loc.T("Mags the Innkeeper"), Short = Loc.T("Mags"), Tier = 1, Shakes = Shake.Firm, Judge = Judgment.Counter, Markup = (1.3f, 1.6f), Colours = Stall,
            From = new[] { ("tab", 1f) },
            Stories = Tell(("tab", new[] { Loc.T("Some adventurer left this to settle his tab. I gave it a good shake and counted the rattles. So much a rattle, love: {price}."),
                                           Loc.T("Three nights' ale and a broken bench, and he paid me in chest. I charge by the rattle, love: {price}.") })),
            Habit = Loc.T("Keeps the Crossed Keys at the crossroads and takes chests for unpaid tabs. She gives one a firm shake, counts the " +
                    "rattles, and charges the same for each, whatever's rattling: a goblin cleaver costs what a purse of coin does."),
            Accepts = new[] { Loc.T("Pleasure doing business!"), Loc.T("Done, love. Come by for a pint.") },
            Leaves = new[] { Loc.T("I've a taproom to run, love. Another time.") },
            Passes = new[] { Loc.T("Suit yourself, love. Someone'll have it."), Loc.T("Fair enough. I'll stick it behind the bar.") },
            Startled = new[] { Loc.T("Oi! I'll want that back in one piece."), Loc.T("Mind my floor, love.") },
            Fussed = new[] { Loc.T("Oi! You break it, you pay for it, love. Half again.") },
        },
        new()
        {
            Key = "dorrin", Name = Loc.T("Dorrin the Delver"), Short = Loc.T("Dorrin"), Tier = 1, Judge = Judgment.Colour, ShakeLimit = 2f, Markup = (0.9f, 1.15f), Visits = 0.5f, Colours = Stall,
            From = new[] { ("barrow", 1f) },
            Stories = Tell(("barrow", new[] { Loc.T("Dragged it up from the Barrow's second vault. The damp's got into the wood: go easy, it won't stand much. {price}."),
                                              Loc.T("From the Barrow. I lost my torch and my nerve down there, but I kept the chest. It's half rotten. {price}."),
                                              Loc.T("I ran from a barrow-wight for this, so it's yours for {price}. Careful: it's falling apart.") })),
            Habit = Loc.T("A young adventurer back from the Barrow. Prices a chest by its colour and never shakes it: Barrow chests are so rotten " +
                    "they stand about two seconds of shaking before they start to come apart. Grave goods: coin, weapons, gems."),
            Accepts = new[] { Loc.T("Drinks are on me tonight!"), Loc.T("Right. Back to the Barrow!") },
            Leaves = new[] { Loc.T("Sorry, I've a barrow to get back to.") },
            Passes = new[] { Loc.T("Fair enough. Someone at the Crossed Keys will want it."), Loc.T("Oh well. More where that came from.") },
            Startled = new[] { Loc.T("Careful, it's rotten!"), Loc.T("Whoa! It'll fall apart!") },
            Fussed = new[] { Loc.T("It's held together with damp and hope! You'll pay more for that.") },
        },

        // Thornwick Market: a goblin, a huntress, a druid, a monk, a smuggler and a matchmaker.
        new()
        {
            Key = "snikkit", Name = Loc.T("Snikkit the Goblin"), Short = Loc.T("Snikkit"), Tier = 2, Shakes = Shake.Gentle, ShakeFee = 0.1f, MissRate = 0.6f, Markup = (1.25f, 1.5f), Visits = 0.5f, Colours = Village,
            From = new[] { ("warren", 1f) },
            Stories = Tell(("warren", new[] { Loc.T("From the warren, yes yes. Very shiny. Snikkit shake it gentle, only gentle. {price}. Looking is free. Looking more, not free!"),
                                              Loc.T("Snikkit's cousins find it, Snikkit sells it. Gentle shake only. {price}. You want to shake more? You pay more!"),
                                              Loc.T("Warren chest. Smells of goblin. Is fine. {price}, and more every time you shake it different.") })),
            PriceUp = new[] { Loc.T("Ooh, you like! Price go up: {price}."), Loc.T("More shaking, more gold! {price}."), Loc.T("Hehe. {price} now.") },
            Habit = Loc.T("A goblin selling what his cousins pinch: warren chests, mostly coin. He only shakes gently, so he never prices what " +
                    "a gentle shake can't stir, and goblins charge for everything, looking included: each strength you try after your " +
                    "first puts his price up a tenth."),
            Accepts = new[] { Loc.T("Shiny! Shiny!"), Loc.T("Hehehe. Sucker. I mean, friend.") },
            Leaves = new[] { Loc.T("Pah! Snikkit has other customers.") },
            Passes = new[] { Loc.T("Pah! Snikkit sell it to someone with eyes."), Loc.T("Your loss. Snikkit's gain. Snikkit always gains.") },
            Startled = new[] { Loc.T("Eek! Snikkit's chest!"), Loc.T("Hehe, again! Again!") },
            Fussed = new[] { Loc.T("You drop Snikkit's chest? Price go up!") },
        },
        new()
        {
            Key = "wenna", Name = Loc.T("Wenna the Huntress"), Short = Loc.T("Wenna"), Tier = 2, Judge = Judgment.Weight, States = Clue.Pounds, Packhorse = true,
            Markup = (0.9f, 1.1f), Colours = Village, LockChance = 0.8f,
            From = new[] { ("camp", 2f), ("den", 1f) },
            Stories = Tell(
                ("camp", new[] { Loc.T("Took it off an outlaws' camp on the Thornwick road. My game scale says {pounds}. {price}. Go gently: the mare spooks."),
                                 Loc.T("The outlaws won't be needing it. {pounds} on my scale, and I price by the pound: {price}.") }),
                ("den", new[] { Loc.T("Found it in a wolf den. The wolves had eaten whoever was carrying it. {pounds} on my scale. {price}."),
                                Loc.T("Wolves don't hoard, but their dinners did. {pounds}, to the half pound. {price}. Easy with it: the mare.") })),
            Habit = Loc.T("A huntress who weighs every chest on her game scale and prices it by the pound, so light things (spells, gems) come " +
                    "cheap. Her chests hang on a mare who bolts at a hard shake, so nobody can shake one hard: take the weight of what you " +
                    "heard from what the scale says, and what's left is what never stirred."),
            Accepts = new[] { Loc.T("Done."), Loc.T("Good hunting.") },
            Leaves = new[] { Loc.T("The mare's restless. I'm off.") },
            Passes = new[] { Loc.T("No matter. It'll sell at the next market."), Loc.T("Fair enough.") },
            Startled = new[] { Loc.T("Easy! You'll have the mare bolting."), Loc.T("Steady.") },
            Fussed = new[] { Loc.T("Easy! You've upset the mare, and me. Half again.") },
        },
        new()
        {
            Key = "bramble", Name = Loc.T("Mother Bramble"), Short = Loc.T("Bramble"), Tier = 2, Judge = Judgment.Dreamer, States = Clue.Dream, Wards = 0.5f, Markup = (1.2f, 1.2f),
            Colours = Village, HiddenChance = 0f,
            From = new[] { ("wood", 1f) },
            Stories = Tell(("wood", new[] { Loc.T("The Old Wood let this one go. I slept beside it and dreamt of {dream}. The oaks whispered its price: {price}."),
                                            Loc.T("Moss grew on it for a hundred years. Last night I dreamt of {dream}, and of a price: {price}.") })),
            Habit = Loc.T("A druid of the Old Wood. Her price comes to her in a dream, and she always dreams true, in signs: of what the Wood warded " +
                    "in the chest, or else of the dearest thing in it. A magpie is coin, a thorn a weapon, a raven's wing a cloak, a river a " +
                    "potion, an owl a spell, a star a gem. If you can't hear what she dreamt of, it's warded: name it."),
            Accepts = new[] { Loc.T("As the Wood wills."), Loc.T("The oaks approve.") },
            Leaves = new[] { Loc.T("The Wood is calling me home.") },
            Passes = new[] { Loc.T("The Wood is patient. So am I."), Loc.T("Then it wasn't meant for you. Not yet.") },
            Startled = new[] { Loc.T("Mind the moss, child."), Loc.T("The Wood felt that.") },
            Fussed = new[] { Loc.T("The Wood felt that, child. So will your purse.") },
        },
        new()
        {
            Key = "ansel", Name = Loc.T("Brother Ansel"), Short = Loc.T("Ansel"), Tier = 2, Shakes = Shake.Firm, HatesHardShake = true, MissRate = 0.55f, Markup = (1.8f, 2.1f), Colours = Village,
            From = new[] { ("crypt", 1f) },
            Stories = Tell(("crypt", new[] { Loc.T("From the abbey crypt, by the Abbot's leave. I shook it firmly, but not hard, please: the saints are resting. {price}."),
                                             Loc.T("Brother Cuthbert's reliquary. I gave it a firm shake, as is seemly. {price}, for the roof fund.") })),
            Habit = Loc.T("A young monk selling the crypt's chests to mend the abbey roof. He shakes a chest firmly and won't have it shaken hard " +
                    "(half again if you do), so the holy cloaks a crypt chest is full of never make a sound for him, and go unpriced."),
            Accepts = new[] { Loc.T("Bless you."), Loc.T("The roof thanks you.") },
            Angry = new[] { Loc.T("Brother! The saints! Half again, for the sacrilege.") },
            Leaves = new[] { Loc.T("Forgive me, I'm due at vespers.") },
            Passes = new[] { Loc.T("Another day, perhaps. The roof can wait. A little."), Loc.T("Peace be with you.") },
            Startled = new[] { Loc.T("Brother! Gently!"), Loc.T("Oh! Saint Cuthbert preserve us.") },
            Fussed = new[] { Loc.T("Brother! The saints! Half again, for the sacrilege.") },
        },
        new()
        {
            Key = "fennick", Name = Loc.T("Fennick the Smuggler"), Short = Loc.T("Fennick"), Tier = 2, Shakes = Shake.Firm, Packs = true, Trade = Trade.Haggle, HiddenChance = 0.45f,
            Bottom = (0.95f, 1.15f), Colours = Village,
            From = new[] { ("road", 1f) },
            Stories = Tell(("road", new[] { Loc.T("Packed in straw, so nothing clinked at the Wardens' toll. I gave it a firm shake. {price}, and no questions."),
                                            Loc.T("Came up the smugglers' road. Don't ask. The straw muffles everything, but I gave it a firm shake. {price}.") })),
            Habit = Loc.T("Runs brandy past the Wardens' toll on the forest road, every chest packed in straw so nothing clinks: everything in it " +
                    "needs a strength harder to stir, and cloaks and spells won't stir at all. He gives it a firm shake, never drops it, and " +
                    "smugglers love a false bottom. Haggles."),
            Accepts = new[] { Loc.T("Pleasure. We never met."), Loc.T("Done. Mind the Wardens.") },
            Counters = new[] { Loc.T("{price}, and you're robbing me."), Loc.T("Tch. {price}, final.") },
            Leaves = new[] { Loc.T("Hmph. Plenty of other buyers."), Loc.T("I've a wagon to catch.") },
            Passes = new[] { Loc.T("Your loss. I was never here."), Loc.T("Suit yourself. Forget my face.") },
            Startled = new[] { Loc.T("Quiet! Do you want the Wardens round?"), Loc.T("Easy. That's straw, not stone.") },
            Fussed = new[] { Loc.T("Quiet, you fool! That noise will cost you.") },
        },
        new()
        {
            Key = "gerda", Name = Loc.T("Gerda the Matchmaker"), Short = Loc.T("Gerda"), Tier = 2, Shakes = Shake.Hard, States = Clue.Dowry, Wards = 0.5f, MissRate = 0f,
            Markup = (1.15f, 1.35f), Colours = Village, HiddenChance = 0f,
            From = new[] { ("dowry", 1f) },
            Stories = Tell(("dowry", new[] { Loc.T("A dowry, love, from a wedding that never was: {clue}, as ever. Shaken hard: {price}."),
                                             Loc.T("The groom ran off to the Barrow. Her dowry: {clue}. Shaken hard: {price}.") })),
            Habit = Loc.T("Thornwick's matchmaker, selling dowries from weddings that never happened. A Thornwick dowry always holds coin for luck, " +
                    "a cloak for warmth and a potion for health, and a bride often wards one of the three: whichever you can't hear is the " +
                    "warded one. Gerda shakes hard and prices what she hears, never what's warded."),
            Accepts = new[] { Loc.T("May it bring you luck."), Loc.T("Mind you use it well, love.") },
            Leaves = new[] { Loc.T("I've a match to make, love. Toodle-oo.") },
            Passes = new[] { Loc.T("Ah well. There's always another wedding."), Loc.T("Not to your taste? Nor was the groom to hers.") },
            Startled = new[] { Loc.T("Mind the bride's things, love!"), Loc.T("Ooh! Careful.") },
            Fussed = new[] { Loc.T("That's a bride's dowry, love! It'll cost you more now.") },
        },

        // Crownhold: an archmage and his apprentice, a dwarf, a noblewoman, a pawnbroker, an archivist, a mudlark and a knight.
        new()
        {
            Key = "wren", Name = Loc.T("Wren the Apprentice"), Short = Loc.T("Wren"), Tier = 3, Judge = Judgment.Colour, States = Clue.Tag, Wards = 0.5f, MagicChance = 0.25f,
            HiddenChance = 0f, Markup = (1.3f, 1.5f), Colours = new[] { 0.2f, 0.6f, 0.2f, 0 },
            From = new[] { ("arcanum", 1f) },
            Stories = Tell(("arcanum", new[] { Loc.T("Master Orrin's clearing out the Arcanum stores. The tag says: packed, {clue}. Master says {price}."),
                                               Loc.T("This one's from the third cellar. I can read the tag, at least: {clue}. {price}, Master says.") })),
            Habit = Loc.T("An archmage's apprentice clearing out the Arcanum stores. Every chest still has its packing tag, listing each kind of " +
                    "thing packed in it, and the archmages warded the most dangerous: a kind on the tag that you can't hear is warded. " +
                    "Prices by the chest's colour, as his master tells him."),
            Accepts = new[] { Loc.T("Oh good! Don't tell Master I was nervous."), Loc.T("Thank you!") },
            Leaves = new[] { Loc.T("I'm late! Master will turn me into something.") },
            Passes = new[] { Loc.T("Oh. Master won't be pleased."), Loc.T("That's all right. There's a whole cellar of them.") },
            Startled = new[] { Loc.T("Careful! Some of those are cursed!"), Loc.T("Eep!") },
            Fussed = new[] { Loc.T("Master said to keep them in one piece! I'll have to ask more.") },
        },
        new()
        {
            Key = "orrin", Name = Loc.T("Archmage Orrin"), Short = Loc.T("Orrin"), Tier = 3, Judge = Judgment.Colour, Wager = 1f / 3, MagicChance = 0.2f, Markup = (1.3f, 1.5f), Colours = Town,
            From = new[] { ("arcanum", 1f) },
            Stories = Tell(("arcanum", new[] { Loc.T("I packed this myself, when the Locking was new. {price}. Tell me exactly what's in it, and a third comes back to you."),
                                               Loc.T("A {colour} chest from my own stores. {price}. Read it true, Keywright, and I'll return a third. I like to see the craft done well.") })),
            Habit = Loc.T("An archmage old enough to remember the Locking. He prices by the chest's colour, and likes a wager: read his chest " +
                    "exactly right, every kind and every count, and he gives back a third of what you paid."),
            Accepts = new[] { Loc.T("Let us see."), Loc.T("We shall see what you heard.") },
            Leaves = new[] { Loc.T("I have three hundred years of reading to catch up on. Good day.") },
            Passes = new[] { Loc.T("Prudent. Or timid. We shall never know."), Loc.T("Another time, Keywright.") },
            Startled = new[] { Loc.T("Hm. Bold."), Loc.T("In my day we were gentler with the Locking's work.") },
            Fussed = new[] { Loc.T("That chest is older than your great-grandmother. A third more, I think.") },
        },
        new()
        {
            Key = "grit", Name = Loc.T("Grit the Miner"), Short = Loc.T("Grit"), Tier = 3, Shakes = Shake.Hard, Judge = Judgment.Miner, Trade = Trade.Haggle, Bottom = (0.85f, 1f), Colours = Town,
            From = new[] { ("hold", 1f) },
            Stories = Tell(("hold", new[] { Loc.T("Dug it out of the deep seam under Ironroot. Gave it a hard shake, with dwarf's hands. {price}."),
                                            Loc.T("From Ironroot hold, sealed since my great-great-grandmother's day. I gave it a proper shake. {price}.") })),
            Habit = Loc.T("A dwarf of Ironroot who knows a weapon when he hears one. He prices weapons in full, everything else at half, and takes " +
                    "every tinkle for a diamond. He shakes hard, and haggles."),
            Accepts = new[] { Loc.T("Done. Back to the deep."), Loc.T("Ha! Done.") },
            Counters = new[] { Loc.T("{price}. Any lower and I'll eat my helmet."), Loc.T("{price}, and that's stone-solid.") },
            Leaves = new[] { Loc.T("You take me for a fool? Good day.") },
            Passes = new[] { Loc.T("Hmph. Your loss, surface-dweller."), Loc.T("Back down the mine with it, then.") },
            Startled = new[] { Loc.T("Ha! Dwarf-made. It'll take worse than that."), Loc.T("Watch it.") },
            Fussed = new[] { Loc.T("Mind my stone-work! That'll cost you.") },
        },
        new()
        {
            Key = "corisande", Name = Loc.T("Lady Corisande"), Short = Loc.T("Corisande"), Tier = 3, Judge = Judgment.Colour, States = Clue.Riddle, Wards = 0.5f, MagicChance = 0.15f,
            HiddenChance = 0f, Markup = (1.05f, 1.3f), Colours = Town,
            From = new[] { ("strongroom", 1f) },
            Stories = Tell(("strongroom", new[] { Loc.T("Grandmother's trick box. Her riddle's on the lid: \"{clue}\" {price}."),
                                                  Loc.T("From our strongroom: {price}. Grandmother's riddle: \"{clue}\"") })),
            Habit = Loc.T("A noblewoman selling her grandmother's trick boxes. Each lid has grandmother's riddle about one thing inside, and if " +
                    "she warded something, the riddle is about that. Solve it: if you can't hear the answer, it's warded. Prices by colour."),
            Accepts = new[] { Loc.T("How delightful."), Loc.T("Grandmother would approve.") },
            Leaves = new[] { Loc.T("I have a fitting at four. Do excuse me.") },
            Passes = new[] { Loc.T("Pity. Grandmother so loved a puzzle."), Loc.T("Another time, perhaps.") },
            Startled = new[] { Loc.T("Goodness!"), Loc.T("Do be careful, that's a de Vere.") },
            Fussed = new[] { Loc.T("That is a de Vere heirloom! The price has just gone up.") },
        },
        new()
        {
            Key = "pell", Name = Loc.T("Pell the Pawnbroker"), Short = Loc.T("Pell"), Tier = 3, Shakes = Shake.Firm, States = Clue.Articles, HatesDrop = true, HiddenChance = 0.35f,
            PairChance = 0.6f, MissRate = 0.45f, Markup = (2.3f, 2.7f), Colours = Town,
            From = new[] { ("pawn", 1f) },
            Stories = Tell(("pawn", new[] { Loc.T("Pledged twenty years ago and never redeemed. The ticket says {articles}. I gave it a firm shake. {price}."),
                                            Loc.T("Ticket four thousand and twelve: {articles}, it says. I price a lot kind by kind, at a firm shake. {price}.") })),
            Habit = Loc.T("Sells pawn lots nobody redeemed, often two or three of a kind. He prices one of each kind he hears and never counts " +
                    "how many. The pawn ticket counts every article pledged, false bottom and all, and he won't have pledges dropped: " +
                    "count what you hear against the ticket."),
            Accepts = new[] { Loc.T("Done. No refunds."), Loc.T("Sold, as heard.") },
            Angry = new[] { Loc.T("Not on my pledges! Half again, for the damage.") },
            Leaves = new[] { Loc.T("Time is money, and you're spending mine.") },
            Passes = new[] { Loc.T("Back on the shelf it goes."), Loc.T("As you like. Pledges keep.") },
            Startled = new[] { Loc.T("Mind the pledges!") },
            Fussed = new[] { Loc.T("Not on my pledges! Half again, for the damage.") },
        },
        new()
        {
            Key = "quill", Name = Loc.T("Quill the Archivist"), Short = Loc.T("Quill"), Tier = 3, Shakes = Shake.Firm, Judge = Judgment.Counter, States = Clue.Roll, Wards = 0.5f,
            HiddenChance = 0f, Markup = (1.85f, 2.15f), Colours = Town,
            From = new[] { ("archive", 1f) },
            Stories = Tell(("archive", new[] { Loc.T("From the Royal Archive's vaults, with its ward-roll: {clue}. Three hundred years of damp. So much a rattle: {price}."),
                                               Loc.T("Unclaimed since the Locking. The ward-roll reads {clue}, as far as I can make it out. I counted the rattles: {price}.") })),
            Habit = Loc.T("The Royal Archive's clerk, selling unclaimed vault chests with their ward-rolls. A roll names what was warded in its " +
                    "chest, its letters faded by three hundred years: read the name, and name its kind on your slip. A blank roll means " +
                    "nothing was warded. He counts the rattles at a firm shake and charges the same for each."),
            Accepts = new[] { Loc.T("Duly recorded."), Loc.T("I'll file the receipt.") },
            Leaves = new[] { Loc.T("The Archive closes at six. Good day.") },
            Passes = new[] { Loc.T("Very well. I'll file it under 'unsold'."), Loc.T("Noted.") },
            Startled = new[] { Loc.T("Careful! That's Crown property."), Loc.T("Oh dear.") },
            Fussed = new[] { Loc.T("That is Crown property! I'll have to add a fee for that.") },
        },
        new()
        {
            Key = "shelly", Name = Loc.T("Shelly the Mudlark"), Short = Loc.T("Shelly"), Tier = 3, Shakes = Shake.Hard, Fakes = new[] { Category.Scrolls }, FakeChance = 0.5f,
            Colours = new[] { 0.1f, 0.5f, 0.35f, 0.05f },
            From = new[] { ("river", 1f) },
            Stories = Tell(("river", new[] { Loc.T("Out of the Aldwater mud at low tide! I shook it hard, and I'd bet my boots on what rustles. {price}."),
                                             Loc.T("The river gives and the river takes. Shook it hard: spellbooks, potions, who knows! {price}.") })),
            Habit = Loc.T("Combs the Aldwater mud at low tide, shakes hard, and prices every rustle as a drowned wizard's spellbook. Her scroll " +
                    "cases are often full of river sand, which pours at the gentlest shake. Real parchment only rustles at a hard one."),
            Accepts = new[] { Loc.T("Mud's been good to me!"), Loc.T("The river provides.") },
            Leaves = new[] { Loc.T("Tide's turning. I'm off.") },
            Passes = new[] { Loc.T("Back in the barrow, then."), Loc.T("No skin off my nose.") },
            Startled = new[] { Loc.T("Ha! It's been through worse in the river."), Loc.T("Oop!") },
            Fussed = new[] { Loc.T("Oi! Mud or no mud, that's mine. It'll cost more now.") },
        },
        new()
        {
            Key = "aldous", Name = Loc.T("Sir Aldous"), Short = Loc.T("Aldous"), Tier = 3, Judge = Judgment.Colour, Heirloom = Category.Arms, Markup = (1.1f, 1.3f), Colours = Town,
            From = new[] { ("warchest", 1f) },
            Stories = Tell(("warchest", new[] { Loc.T("Grandfather's war chest. If his sword's in it, it's mine, and I'll pay you {fee} for it. The rest is yours for {price}."),
                                                Loc.T("The de Lacy war chest, from the siege of Harrowgate. Any blade in it is my family's: {fee} for the best. {price} for the chest.") })),
            Habit = Loc.T("A knight looking for his grandfather's sword. Prices by colour. If there are weapons in his chest, the best of them goes " +
                    "back to him for the price he names: value what's left."),
            Accepts = new[] { Loc.T("Honour is satisfied."), Loc.T("For the de Lacys!") },
            Leaves = new[] { Loc.T("I'm expected at the lists. Good day.") },
            Passes = new[] { Loc.T("Then I shall keep looking."), Loc.T("Another chest, another day.") },
            Startled = new[] { Loc.T("Have a care, sir!"), Loc.T("Steady!") },
            Fussed = new[] { Loc.T("Have a care with my grandfather's things! It'll cost you.") },
        },

        // The Royal Emporium, by the Gilded Gate: demons, a court lady, a cartomancer, a fence, the King, and once in a while the Maw.
        new()
        {
            Key = "vexis", Name = Loc.T("Vexis the Imp"), Short = Loc.T("Vexis"), Tier = 4, Judge = Judgment.Colour, Patience = 12f, QualityBoost = 0.2f, Markup = (1.35f, 1.65f),
            Colours = new[] { 0, 0, 0.2f, 0.8f },
            From = new[] { ("gate", 1f) },
            Stories = Tell(("gate", new[] { Loc.T("Fresh through the Gate, from the Maw's own hoard. {price}. And the sand's already running, mortal."),
                                            Loc.T("Hot off the Gilded Gate. A {colour} chest: {price}. Tick, tock.") })),
            Habit = Loc.T("An imp with an hourglass, selling from the Maw's hoard. Prices by colour and never looks inside, and gives you twelve " +
                    "seconds to decide. Things from the Maw's hoard are often better than their chest's colour."),
            Accepts = new[] { Loc.T("Delicious."), Loc.T("A pleasure, mortal.") },
            Leaves = new[] { Loc.T("Too slow! Back through the Gate I go.") },
            Passes = new[] { Loc.T("Pfft. Your loss, mortal."), Loc.T("Fine. Someone in the Emporium has more sense.") },
            Startled = new[] { Loc.T("Hee! Again!"), Loc.T("Clumsy mortal.") },
            Fussed = new[] { Loc.T("Careful, mortal! Breakages cost extra.") },
        },
        new()
        {
            Key = "malbranche", Name = Loc.T("Malbranche"), Short = Loc.T("Malbranche"), Tier = 4, Judge = Judgment.Colour, TakesDearest = true, Markup = (0.5f, 0.65f), Colours = Emporium,
            From = new[] { ("gate", 1f) },
            Stories = Tell(("gate", new[] { Loc.T("From the Maw's hoard. {price}, a pittance. My only condition: the dearest thing inside is mine."),
                                            Loc.T("A bargain, mortal: {price}. When it opens, I take the best of it. Everything else is yours.") })),
            Habit = Loc.T("A skeletal devil who deals in bargains. His price is low, but the dearest thing inside is his. A chest of many middling " +
                    "things suits his deal; one grand thing and little else doesn't."),
            Accepts = new[] { Loc.T("Signed and sealed."), Loc.T("A pleasure doing business. It always is.") },
            Leaves = new[] { Loc.T("My time is worth more than yours, mortal.") },
            Passes = new[] { Loc.T("Another time. There is always another time."), Loc.T("How very wise of you. Or was it?") },
            Startled = new[] { Loc.T("Mind my bones."), Loc.T("Tsk.") },
            Fussed = new[] { Loc.T("Damaged goods cost more, mortal. Read the small print.") },
        },
        new()
        {
            Key = "skrae", Name = Loc.T("Skrae"), Short = Loc.T("Skrae"), Tier = 4, Judge = Judgment.Thorough, States = Clue.Echo, Wards = 0.5f, MissRate = 0f, MagicChance = 0.2f,
            HiddenChance = 0f, Markup = (1.2f, 1.45f), Colours = Emporium,
            From = new[] { ("gate", 1f) },
            Stories = Tell(("gate", new[] { Loc.T("Shaken gently, then hard: {price}. Then I clicked, like a bat. The echoes: {clue}."),
                                            Loc.T("I hear every coin in the realm. {price}. My echoes: {clue}. Wards can't stop an echo.") })),
            Habit = Loc.T("A demon with ears like a bat. It shakes a chest gently, then hard, and prices what it hears, fairly. Then it clicks and " +
                    "listens to the echoes, which come back off everything inside, warded or not: round is coin, long a weapon, soft a " +
                    "cloak, hollow a potion, flat a spell, tiny a gem. An echo you can't hear is warded."),
            Accepts = new[] { Loc.T("Fair."), Loc.T("You won't regret it. Probably.") },
            Leaves = new[] { Loc.T("I hear someone counting gold elsewhere.") },
            Passes = new[] { Loc.T("I heard that coming."), Loc.T("Hm. Fair enough.") },
            Startled = new[] { Loc.T("Ow! My ears!"), Loc.T("Loud. Very loud.") },
            Fussed = new[] { Loc.T("My ears! That will cost you.") },
        },
        new()
        {
            Key = "ermine", Name = Loc.T("Lady Ermine"), Short = Loc.T("Ermine"), Tier = 4, Judge = Judgment.Jeweller, Always = Category.Gems, HatesMagnet = true, Fakes = new[] { Category.Gems },
            FakeChance = 0.25f, Markup = (1.5f, 1.7f), Colours = Emporium,
            From = new[] { ("strongroom", 1f) },
            Stories = Tell(("strongroom", new[] { Loc.T("From my strongroom. My gems, and whatever my maid packed. {price}."),
                                                  Loc.T("The gems alone are worth {price}, I'm told. The rest I couldn't say. My maid packs.") })),
            Habit = Loc.T("A lady of the court who knows her own jewels and nothing else: she prices her gems at what she paid for them, and " +
                    "whatever else her maid packed comes free. Some of her gems are paste, and she won't have a lodestone near them (half " +
                    "again if you try)."),
            Accepts = new[] { Loc.T("Charming."), Loc.T("Do look after them.") },
            Angry = new[] { Loc.T("A lodestone? On MY gems? Half again, for the insult.") },
            Leaves = new[] { Loc.T("I'm expected at court. Good day.") },
            Passes = new[] { Loc.T("How very disappointing."), Loc.T("Well. Someone at court will have it.") },
            Startled = new[] { Loc.T("My gems!"), Loc.T("Must you?") },
            Fussed = new[] { Loc.T("My gems! Half again, for the fright.") },
        },
        new()
        {
            Key = "oriel", Name = Loc.T("Madame Oriel"), Short = Loc.T("Oriel"), Tier = 4, Judge = Judgment.Colour, States = Clue.Cards, Wards = 0.5f, MagicChance = 0.15f, HiddenChance = 0f,
            Markup = (1.3f, 1.5f), Colours = Emporium,
            From = new[] { ("court", 1f) },
            Stories = Tell(("court", new[] { Loc.T("A courtier's chest, sold for his gambling debts. Three from the Locking's deck: {clue}. {price}."),
                                             Loc.T("The cards chose this one for you, dear. {clue}. A {colour} chest: {price}.") })),
            Habit = Loc.T("The court's cartomancer. She draws three cards from the Locking's own deck for each chest: a suit drawn twice is what " +
                    "the ward holds (Coins, Swords for weapons, Mantles for cloaks, Cups for potions, Wands for spells, Stars for gems); " +
                    "three different suits, and nothing's warded. Prices by colour."),
            Accepts = new[] { Loc.T("The cards are never wrong."), Loc.T("As it was foretold.") },
            Leaves = new[] { Loc.T("The cards say I'm wanted elsewhere.") },
            Passes = new[] { Loc.T("The cards did say you might."), Loc.T("As you wish, dear.") },
            Startled = new[] { Loc.T("I didn't see that coming. How rare."), Loc.T("Oh!") },
            Fussed = new[] { Loc.T("The cards warned me about you. Half again.") },
        },
        new()
        {
            Key = "silas", Name = Loc.T("Silas Gilt"), Short = Loc.T("Silas"), Tier = 4, Judge = Judgment.Crook, MissRate = 0f, Fakes = new[] { Category.Coins, Category.Gems }, FakeChance = 0.65f,
            HiddenChance = 0f, Colours = Emporium,
            From = new[] { ("gate", 1f) },
            Stories = Tell(("gate", new[] { Loc.T("Out of the Maw's hoard, friend, through a crack in the Gate. I'm leaving town tonight, so {price}."),
                                            Loc.T("Hoard goods, very rare. Gold and gems, every one of them. For you, just {price}.") })),
            Habit = Loc.T("A fence for the Maw's hoard. He knows exactly what's in his chests, because he packed the demon gold himself: gilded " +
                    "iron sold as gold and gems, cheap. What's real comes cheap; the fakes are worth nothing."),
            Accepts = new[] { Loc.T("Wonderful! No refunds."), Loc.T("A pleasure. I really must be going.") },
            Leaves = new[] { Loc.T("Can't stay. People to avoid.") },
            Passes = new[] { Loc.T("Your loss, friend. Your very great loss."), Loc.T("Suit yourself. I was never here.") },
            Startled = new[] { Loc.T("Easy, friend! Easy."), Loc.T("Whoa there.") },
            Fussed = new[] { Loc.T("Easy, friend! That'll cost a little extra.") },
        },
        new()
        {
            Key = "aldo", Name = Loc.T("Old Aldo"), Short = Loc.T("Aldo"), Tier = 4, Judge = Judgment.Clueless, SlipPrice = true, Visits = 0.25f, PairChance = 0.35f,
            MagicChance = 0.2f, HiddenChance = 0.25f, Colours = new[] { 0, 0, 0.3f, 0.7f },
            From = new[] { ("treasury", 1f) },
            Stories = Tell(("treasury", new[] { Loc.T("From an old house with a great many cellars. I've no idea what's in it. You tell me, and whatever you say it holds is my price."),
                                                Loc.T("Old things from an old house. Name what's inside: that's the price. Name it truly and I'll take a quarter off.") })),
            Habit = Loc.T("A tired old man in a plain cloak, with a very fine ring. He doesn't know what's in his chests: his price is whatever " +
                    "your slip says. Read it exactly and he takes a quarter off; read it wrong and you pay what it really holds, and a " +
                    "quarter more."),
            Accepts = new[] { Loc.T("Well. We shall see."), Loc.T("Hm. Let's see what you heard.") },
            Leaves = new[] { Loc.T("I'm needed elsewhere. Another day.") },
            Passes = new[] { Loc.T("Another day, then. I'll be back."), Loc.T("Hm. A careful one. Good.") },
            Startled = new[] { Loc.T("Steady. It's older than both of us."), Loc.T("Oh my.") },
            Fussed = new[] { Loc.T("Steady! That was my grandfather's. I'll want a little more.") },
        },
        new()
        {
            Key = "maw", Name = Loc.T("Avarax, the Gilded Maw"), Nick = Loc.T("the Maw"), Tier = 4, Judge = Judgment.Colour, Gaze = 2f, Visits = 0.1f,
            Colours = new[] { 0, 0, 0.5f, 0.5f },
            From = new[] { ("gate", 1f) },
            Stories = Tell(("gate", new[] { Loc.T("A trifle from my hoard. One coin. All I ask is that you open it here, where I can see."),
                                            Loc.T("One coin, little Keywright. And then everyone in this city will want twice as much today. Isn't that fun?") })),
            Habit = Loc.T("The demon of greed himself, in a borrowed body dripping with rings. He sells a chest for one coin, but greed is " +
                    "catching: buy it, and everyone who comes to your counter after him that day asks twice as much."),
            Accepts = new[] { Loc.T("Yesss."), Loc.T("Open it. Let me see.") },
            Leaves = new[] { Loc.T("I have all the time in the world. You don't.") },
            Passes = new[] { Loc.T("Clever little thing."), Loc.T("Mm. Another day, then. There are so many days.") },
            Startled = new[] { Loc.T("Careful. I break things too."), Loc.T("Ha.") },
            Fussed = new[] { Loc.T("Careful. Greed always wants more.") },
        },
    };

    public static SellerDef Get(string key) => All.First(s => s.Key == key);

    /// <summary>What a seller says when you pass on their chest.</summary>
    public static string PassLine(Random rng, SellerDef seller = null) => rng.PickOne(seller?.Passes.Length > 0 ? seller.Passes : Shrug);

    /// <summary>What a seller says when you drop their chest and they don't mind.</summary>
    public static string StartleLine(Random rng, SellerDef seller) => rng.PickOne(seller.Startled.Length > 0 ? seller.Startled : Startle);

    /// <summary>What a seller says when you pass on their chest and a Heartsong bursts out of it to you.</summary>
    public static string BurstLine => Loc.T("Gods! It just... burst open! Keep the singing thing, then. The rest is going home with me.");

    public static string Number(int n) => n switch { 1 => Loc.T("one"), 2 => Loc.T("two"), 3 => Loc.T("three"), 4 => Loc.T("four"), 5 => Loc.T("five"), 6 => Loc.T("six"), _ => n.ToString() };

    public static string Join(IReadOnlyList<string> words) =>
        words.Count <= 1 ? words.FirstOrDefault() ?? "" : Loc.T($"{string.Join(", ", words.Take(words.Count - 1))} and {words[^1]}");
}
