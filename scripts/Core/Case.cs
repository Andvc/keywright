using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Keywright.Core;

/// <summary>How a clue came to you: something heard through a chest, something found in one, something someone said, or
/// something you saw.</summary>
public enum ClueWay { Heard, Found, Said, Seen }

/// <summary>A word a blank can take: a key (kept in saves) and how it reads.</summary>
public sealed record CaseWord(string Key, string Text);

/// <summary>
/// One of the season's questions (docs/narrative.md, "The case"): a sentence with blanks, filled with words the clues
/// give you. Checked whole: right, or how many are wrong, never which (Golden Idol's form).
/// </summary>
public sealed class CaseQuestion
{
    public string Key { get; init; }
    /// <summary>The question as the notebook lists it: "Why is Haddon gold?"</summary>
    public string Ask { get; init; }
    /// <summary>The answer's sentence, with {0} and {1} for its blanks.</summary>
    public string Sentence { get; init; }
    /// <summary>The words each blank can take (the right one and the true-but-wrong ones a partial reading suggests).</summary>
    public CaseWord[][] Blanks { get; init; }
    /// <summary>The right word's key for each blank.</summary>
    public string[] Answer { get; init; }
    /// <summary>Words you have from the start: the obvious guesses.</summary>
    public string[] Free { get; init; } = Array.Empty<string>();
    /// <summary>What working it out does, as a line for the notebook.</summary>
    public string Worked { get; init; }
    /// <summary>What it changes, once (standing, a flag the story reads).</summary>
    public Action<GameState> Payoff { get; init; }
    /// <summary>A question of the career, not the season: it opens from the second season, and its clues carry over.</summary>
    public bool Career { get; init; }
}

/// <summary>A clue: what you learned, how, and which words it gives the question it belongs to.</summary>
public sealed class CaseClue
{
    public string Key { get; init; }
    public string Question { get; init; }
    public ClueWay Way { get; init; }
    public string Text { get; init; }
    public string[] Gives { get; init; } = Array.Empty<string>();
}

public static class Case
{
    public const string Haddon = "haddon", Glove = "glove", Gate = "gate", Guild = "guild", Crown = "crown", Hold = "hold", Spring = "spring";

    static CaseWord W(string key, string text) => new(key, text);

    public static readonly CaseQuestion[] Questions =
    {
        new()
        {
            Key = Haddon, Ask = Loc.T("Why is Haddon gold?"),
            Sentence = Loc.T("Old Haddon {0}, and what's pledged and never paid belongs to {1}."),
            Blanks = new[]
            {
                new[] { W("pledged", Loc.T("pledged himself to the Guild")), W("sold", Loc.T("sold his workshop to the Guild")), W("stole", Loc.T("stole a Locking chest")) },
                new[] { W("maw", Loc.T("the Maw")), W("guild", Loc.T("the Guild")), W("king", Loc.T("the King")) },
            },
            Answer = new[] { "pledged", "maw" }, Free = new[] { "sold", "guild" },
            Worked = Loc.T("Nell Haddon tells the crossroads what you worked out. Folk read their Guild papers twice now."),
            Payoff = s => s.Sway(Faction.Commons, 1),
        },
        new()
        {
            Key = Glove, Ask = Loc.T("What's under Bram's glove?"),
            Sentence = Loc.T("Bram's hand is {0}: twenty years ago he pledged it for {1}."),
            Blanks = new[]
            {
                new[] { W("gold", Loc.T("gold")), W("burned", Loc.T("burned")), W("marked", Loc.T("marked")) },
                new[] { W("child", Loc.T("a marked child")), W("stall", Loc.T("his stall")), W("shard", Loc.T("a shard of the Key")) },
            },
            Answer = new[] { "gold", "child" }, Free = new[] { "burned", "stall" },
            Worked = Loc.T("Bram stops hiding his hand. He'd rather you knew."),
            Payoff = s => s.Flags.Add("case.glove"),
        },
        new()
        {
            Key = Gate, Ask = Loc.T("Where do the gilded go?"),
            Sentence = Loc.T("The Guild carts the gilded to {0} and sells them to {1}, by weight."),
            Blanks = new[]
            {
                new[] { W("gate", Loc.T("the Gilded Gate")), W("treasury", Loc.T("the Royal Treasury")), W("hall", Loc.T("the Guild Hall")) },
                new[] { W("maw", Loc.T("the Maw")), W("crown", Loc.T("the Crown")), W("arcanum", Loc.T("the Arcanum")) },
            },
            Answer = new[] { "gate", "maw" }, Free = new[] { "hall", "crown" },
            Worked = Loc.T("Sergeant Hollis believes you, and the Wardens with him."),
            Payoff = s => s.Sway(Faction.Wardens, 1),
        },
        new()
        {
            Key = Guild, Ask = Loc.T("Whose is the Guild?"),
            Sentence = Loc.T("The Gilded Guild belongs to {0}, and every coin of its tithe is {1}."),
            Blanks = new[]
            {
                new[] { W("maw", Loc.T("the Maw")), W("king", Loc.T("the King")), W("own", Loc.T("nobody but itself")) },
                new[] { W("eye", Loc.T("his eye")), W("bread", Loc.T("his bread")), W("key", Loc.T("his key")) },
            },
            Answer = new[] { "maw", "eye" }, Free = new[] { "own", "bread" },
            Worked = Loc.T("Word gets round Lantern Lane that you know whose the Guild is. The Lockless count you less of a mark."),
            Payoff = s => s.Sway(Faction.Lockless, 1),
        },
        new()
        {
            Key = Crown, Ask = Loc.T("What is the Guild collecting?"),
            Sentence = Loc.T("The King pledged {0} to the Guild, and if the Crown can't pay on the season's last night, the Locking opens for {1}."),
            Blanks = new[]
            {
                new[] { W("chest", Loc.T("the Crown Chest")), W("emporium", Loc.T("the Royal Emporium")), W("ironroot", Loc.T("Ironroot's crown")) },
                new[] { W("maw", Loc.T("the Maw")), W("guild", Loc.T("the Guild")), W("heir", Loc.T("the King's heir")) },
            },
            Answer = new[] { "chest", "maw" }, Free = new[] { "emporium", "guild" },
            Worked = Loc.T("Word reaches the palace that you know. The Crown counts you a friend."),
            Payoff = s => s.Sway(Faction.Crown, 1),
        },
        new()
        {
            Key = Hold, Ask = Loc.T("How can the Locking hold?"),
            Sentence = Loc.T("Open the Crown Chest with {0} before the note falls due, or prove the Guild's pledges were {1}."),
            Blanks = new[]
            {
                new[] { W("key", Loc.T("the Aldric Key")), W("blood", Loc.T("a Keywright's blood")), W("songs", Loc.T("the nine Heartsongs")) },
                new[] { W("secret", Loc.T("made to the Maw in secret")), W("unsigned", Loc.T("never signed")), W("demon", Loc.T("paid in demon gold")) },
            },
            Answer = new[] { "key", "secret" }, Free = new[] { "blood", "unsigned" },
            Worked = Loc.T("You know what has to be done, and by when."),
            Payoff = s => s.Flags.Add("case.hold"),
        },
        new()
        {
            Key = Spring, Ask = Loc.T("Why does the spring come round again?"), Career = true,
            Sentence = Loc.T("When the Gate opens, the Locking {0}, and only {1} remember."),
            Blanks = new[]
            {
                new[] { W("turns", Loc.T("turns the year back")), W("new", Loc.T("starts a new Locking")), W("breaks", Loc.T("breaks for good")) },
                new[] { W("marked", Loc.T("the marked")), W("kings", Loc.T("the King's line")), W("mages", Loc.T("the archmages")) },
            },
            Answer = new[] { "turns", "marked" }, Free = new[] { "breaks", "mages" },
            Worked = Loc.T("You know why the seasons repeat. The way out is the song sung whole."),
            Payoff = s => s.Flags.Add("case.spring"),
        },
    };

    public static readonly CaseClue[] Clues =
    {
        // Why is Haddon gold?
        new() { Key = "statue", Question = Haddon, Way = ClueWay.Seen, Gives = new[] { "stole" },
            Text = Loc.T("Old Haddon the wheelwright stands at the crossroads, gold from his boots to his cap, the day after the Guild foreclosed on him. Folk say he stole a Locking chest.") },
        new() { Key = "nell", Question = Haddon, Way = ClueWay.Said, Gives = new[] { "pledged" },
            Text = Loc.T("Nell Haddon: \"He signed their paper for a new lathe, against his own hand. He laughed at that.\"") },
        new() { Key = "haddon.paper", Question = Haddon, Way = ClueWay.Found, Gives = new[] { "pledged" },
            Text = Loc.T("Haddon's Guild paper, under the tools in his chest: \"the borrower's own hand, and all of him, if the note is not met.\"") },
        new() { Key = "unopened.gold", Question = Haddon, Way = ClueWay.Said, Gives = new[] { "maw" },
            Text = Loc.T("Sister Agathe: \"What's pledged to the Guild is pledged to gold, and gold has one master.\"") },
        new() { Key = "mara.laws", Question = Haddon, Way = ClueWay.Said, Gives = new[] { "pledged", "maw" },
            Text = Loc.T("Mara: \"The Maw can't take: he can only be given, sold or pledged. Every Guild paper is a pledge to him, and what he owns turns to gold.\"") },

        // What's under Bram's glove?
        new() { Key = "vane.glove", Question = Glove, Way = ClueWay.Said,
            Text = Loc.T("Mister Vane, to Bram: \"The interest is showing, I see. Do keep your hand warm.\"") },
        new() { Key = "bram.fingers", Question = Glove, Way = ClueWay.Seen, Gives = new[] { "gold" },
            Text = Loc.T("On the second Guild night Bram's hand showed as he paid. His fingers are gold to the knuckle.") },
        new() { Key = "bettany.baby", Question = Glove, Way = ClueWay.Said, Gives = new[] { "child", "marked" },
            Text = Loc.T("Bettany: \"Twenty years back he took a marked baby out of the Guild's care. Nobody ever knew what he paid.\"") },
        new() { Key = Story.Told, Question = Glove, Way = ClueWay.Said, Gives = new[] { "gold", "child" },
            Text = Loc.T("Bram: \"I had nothing they'd take but my own hand, mark and all.\" He pledged it for a marked baby, twenty years ago.") },

        // Where do the gilded go?
        new() { Key = "cart", Question = Gate, Way = ClueWay.Seen,
            Text = Loc.T("On the first Guild night the Guild's cart took Old Haddon from the crossroads, under sacking.") },
        new() { Key = "hollis.carts", Question = Gate, Way = ClueWay.Said, Gives = new[] { "treasury" },
            Text = Loc.T("Sergeant Hollis: \"Guild carts pass my toll every fifth night. They don't stop at the Guild Hall: they go on down the palace road.\"") },
        new() { Key = "hollis.gate", Question = Gate, Way = ClueWay.Said, Gives = new[] { "gate", "maw" },
            Text = Loc.T("Hollis followed a cart: under the palace, a door with a seam of gold. They weigh the gilded by the stone, and something behind it takes them.") },
        new() { Key = "ledger", Question = Gate, Way = ClueWay.Found, Gives = new[] { "gate", "maw" },
            Text = Loc.T("A page of the Guild's ledgers: \"Sold down the Gate, by weight: H. Haddon, fourteen stone.\"") },
        new() { Key = "market.buyers", Question = Gate, Way = ClueWay.Said, Gives = new[] { "maw", "arcanum" },
            Text = Loc.T("The buyers at the Lantern Market: \"Gilded things, for the master. He takes what he's owed, by weight.\"") },

        // Whose is the Guild?
        new() { Key = "guild.demongold", Question = Guild, Way = ClueWay.Found, Gives = new[] { "maw" },
            Text = Loc.T("A chest from the Guild's own strongroom held demon gold: the Maw's gilded iron.") },
        new() { Key = "unopened.eye", Question = Guild, Way = ClueWay.Said, Gives = new[] { "eye" },
            Text = Loc.T("Sister Agathe: \"Every coin you let out of a chest is an eye for him. You open them. He sees.\"") },
        new() { Key = "mara.eyes", Question = Guild, Way = ClueWay.Said, Gives = new[] { "maw", "eye" },
            Text = Loc.T("Mara: \"Loose gold is his eye. With every tithe the Guild takes, he sees a little more of the realm he's buying.\"") },

        // What is the Guild collecting?
        new() { Key = "vane.kings", Question = Crown, Way = ClueWay.Said,
            Text = Loc.T("Mister Vane: \"Even kings pledge, Keywright. And the Crown's note falls due on the season's last night.\"") },
        new() { Key = "roll.crown", Question = Crown, Way = ClueWay.Found, Gives = new[] { "chest" },
            Text = Loc.T("The Chest Roll, as Mara read it: \"The Crown Chest: pledged to the Gilded Guild, against the Crown's note.\"") },
        new() { Key = "aldo.note", Question = Crown, Way = ClueWay.Said, Gives = new[] { "chest" },
            Text = Loc.T("Old Aldo: \"Ten years ago there was a famine, so I pledged the one thing the Crown owned outright.\"") },
        new() { Key = "mara.owner", Question = Crown, Way = ClueWay.Said, Gives = new[] { "maw" },
            Text = Loc.T("Mara: \"The Locking opens for its rightful owner. If the Crown can't pay on the season's last night, that's the Maw.\"") },

        // How can the Locking hold?
        new() { Key = "mara.key", Question = Hold, Way = ClueWay.Said, Gives = new[] { "key", "songs" },
            Text = Loc.T("Mara: \"If the Key is whole by the season's last night, the King will give the Crown Chest to a Keywright, and a gift can't be collected.\"") },
        new() { Key = "aldo.gift", Question = Hold, Way = ClueWay.Said, Gives = new[] { "key" },
            Text = Loc.T("Old Aldo: \"A gift is rightful. Give a thing away before the note falls due, to someone who can hold it, and there's nothing to collect.\"") },
        new() { Key = "mara.court", Question = Hold, Way = ClueWay.Said, Gives = new[] { "secret" },
            Text = Loc.T("Mara: \"A pledge made to someone who hides who they are isn't given freely. Prove the Guild hid its master, and every pledge it took is void.\"") },
        new() { Key = "quire.law", Question = Hold, Way = ClueWay.Said, Gives = new[] { "secret", "demon" },
            Text = Loc.T("Master Quire: \"By the ward's own law, a thing taken in secret was never given. The Guild's sales are stolen goods.\"") },
        new() { Key = "tobias.secret", Question = Hold, Way = ClueWay.Said, Gives = new[] { "secret" },
            Text = Loc.T("Tobias Fenn: \"Nobody who ever signed a Guild paper knew who they were signing to. The ledgers say it plain: 'below'.\"") },

        // Why does the spring come round again? (the career's)
        new() { Key = "dream", Question = Spring, Way = ClueWay.Said, Gives = new[] { "turns", "new" },
            Text = Loc.T("Bram: \"I dreamt we lost the stall, and I was gold, standing at the crossroads. Then I woke, and it was spring.\"") },
        new() { Key = "vane.again", Question = Spring, Way = ClueWay.Said, Gives = new[] { "turns", "kings" },
            Text = Loc.T("Mister Vane: \"We've met before, I think. You always look at my ring like that.\"") },
        new() { Key = "own.chest", Question = Spring, Way = ClueWay.Found, Gives = new[] { "marked" },
            Text = Loc.T("A chest from a Guild sale, marked inside the lid in your own hand and dated this spring. You don't remember marking it.") },
        new() { Key = "mara.clause", Question = Spring, Way = ClueWay.Said, Gives = new[] { "turns", "marked" },
            Text = Loc.T("Mara: \"If ever the Gate opens, the year turns back to its spring, and only the marked remember.\"") },
    };

    public static CaseQuestion Question(string key) => Questions.FirstOrDefault(q => q.Key == key);
    public static CaseClue Clue(string key) => Clues.FirstOrDefault(c => c.Key == key);

    /// <summary>The words a clue's channel is shown with: "heard", "found", "said", "seen".</summary>
    public static string WayName(ClueWay way) => way switch
    {
        ClueWay.Heard => Loc.T("heard"),
        ClueWay.Found => Loc.T("found"),
        ClueWay.Said => Loc.T("said"),
        _ => Loc.T("seen"),
    };

    /// <summary>Fills a question's sentence with words (null for a blank not yet filled, shown as a line).</summary>
    public static string Fill(CaseQuestion q, IReadOnlyList<string> picks) =>
        string.Format(CultureInfo.InvariantCulture, q.Sentence, Enumerable.Range(0, q.Blanks.Length)
            .Select(i => i < picks.Count && picks[i] != null ? Word(q, i, picks[i])?.Text ?? Blank : Blank).Cast<object>().ToArray());

    /// <summary>An unfilled blank, as the notebook shows it.</summary>
    public const string Blank = "______";

    /// <summary>A blank's word by its key (null if it isn't one of that blank's).</summary>
    public static CaseWord Word(CaseQuestion q, int blank, string key) => q.Blanks[blank].FirstOrDefault(w => w.Key == key);
}

/// <summary>The season's case (docs/narrative.md): the clues found and the questions worked out, and what carries over.</summary>
public partial class GameState
{
    /// <summary>Clues found this season (and the career question's from earlier seasons, <see cref="CareerClues"/>).</summary>
    public HashSet<string> Clues { get; } = new();
    /// <summary>Questions worked out this season.</summary>
    public HashSet<string> Solved { get; } = new();
    /// <summary>Questions worked out in any season, this one included (what the profile keeps).</summary>
    public HashSet<string> Truths { get; } = new();
    /// <summary>
    /// Questions worked out in an earlier season, as this one began: filled in already, in your own hand, and never paid off
    /// again (kept in the save, so a day played over doesn't count this season's work as an earlier one's).
    /// </summary>
    public HashSet<string> KnownBefore { get; } = new();
    /// <summary>The career question's clues found in earlier seasons (the profile's).</summary>
    public HashSet<string> CareerClues { get; } = new();
    /// <summary>Seasons played before this one, and how the last one ended (<see cref="Ends"/>), from the profile.</summary>
    public int SeasonsBefore { get; set; }
    public string LastEnd { get; set; }

    /// <summary>A clue just found, for the screen to note once (and clear).</summary>
    public CaseClue NewClue { get; set; }

    /// <summary>Notes a clue. True the first time; it opens its question if it wasn't already.</summary>
    public bool Find(string key)
    {
        var clue = Case.Clue(key);
        if (clue == null || !Clues.Add(key)) return false;
        var question = Case.Question(clue.Question);
        if (question?.Career == true) CareerClues.Add(key);
        // Noted on screen only while it's news: not for a question already worked out (this season or an earlier one).
        if (question == null || !Knows(question)) NewClue = clue;
        return true;
    }

    /// <summary>Whether a question is open: one of its clues has been found, or it was worked out in an earlier season.</summary>
    public bool Asking(CaseQuestion q) =>
        (!q.Career || SeasonsBefore > 0) && (KnownBefore.Contains(q.Key) || Clues.Any(c => Case.Clue(c)?.Question == q.Key));

    /// <summary>Whether the answer's known: worked out this season, or in an earlier one.</summary>
    public bool Knows(CaseQuestion q) => Solved.Contains(q.Key) || KnownBefore.Contains(q.Key);

    /// <summary>The words a blank can be filled with now: the obvious guesses, and what the clues have given.</summary>
    public List<CaseWord> Words(CaseQuestion q, int blank)
    {
        var given = Clues.Select(Case.Clue).Where(c => c?.Question == q.Key).SelectMany(c => c.Gives).ToHashSet();
        bool known = Knows(q);
        return q.Blanks[blank].Where(w => known || q.Free.Contains(w.Key) || given.Contains(w.Key)).ToList();
    }

    /// <summary>How many of the blanks are wrong (0: worked out; it pays off once, and is remembered).</summary>
    public int Check(CaseQuestion q, IReadOnlyList<string> picks)
    {
        int wrong = Enumerable.Range(0, q.Answer.Length).Count(i => i >= picks.Count || picks[i] != q.Answer[i]);
        // It pays off the first time it's ever worked out: a truth from an earlier season is known, not rewarded again
        // (every run starts the same; what carries over is what you know).
        if (wrong == 0 && !KnownBefore.Contains(q.Key) && Solved.Add(q.Key))
        {
            Truths.Add(q.Key);
            q.Payoff?.Invoke(this);
            Log(Story.Tale, Loc.T($"Worked out: {Case.Fill(q, picks)}"));
        }
        return wrong;
    }

    /// <summary>The questions open now, in the order they come.</summary>
    public IEnumerable<CaseQuestion> OpenQuestions => Case.Questions.Where(Asking);

    void SaveCase(SaveData data)
    {
        data.CaseClues = Clues.ToList();
        data.CaseSolved = Solved.ToList();
        data.CaseKnown = KnownBefore.ToList();
        data.SeasonsBefore = SeasonsBefore;
        data.LastEnd = LastEnd;
    }

    void LoadCase(SaveData data)
    {
        Clues.UnionWith((data.CaseClues ?? new List<string>()).Where(k => Case.Clue(k) != null));
        Solved.UnionWith((data.CaseSolved ?? new List<string>()).Where(k => Case.Question(k) != null));
        KnownBefore.UnionWith((data.CaseKnown ?? new List<string>()).Where(k => Case.Question(k) != null));
        Truths.UnionWith(Solved);
        Truths.UnionWith(KnownBefore);
        SeasonsBefore = data.SeasonsBefore;
        LastEnd = data.LastEnd;
        Continued = true;
    }

    /// <summary>A run picked up from a save (its seasons before and last end are its own, not the profile's now).</summary>
    public bool Continued { get; private set; }

    /// <summary>
    /// What the case remembers from earlier seasons (the profile's): answers worked out, the career question's clues,
    /// and (for a new run) how many seasons came before and how the last one ended.
    /// </summary>
    void RememberCase(Profile profile)
    {
        Truths.UnionWith(profile.Truths.Where(t => Case.Question(t) != null));
        CareerClues.UnionWith(profile.CareerClues.Where(c => Case.Clue(c) != null));
        Clues.UnionWith(CareerClues);
        if (Continued) return;
        KnownBefore.UnionWith(profile.Truths.Where(t => Case.Question(t) != null));
        SeasonsBefore = profile.Runs;
        LastEnd = profile.LastEnd;
    }
}
