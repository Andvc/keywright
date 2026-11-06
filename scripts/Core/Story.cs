using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>One line in a scene: who says it (a portrait key), what they say, and the clue it gives the case, if any.</summary>
public sealed record Line(string Speaker, string Text, string Clue = null);

/// <summary>
/// What's going on behind the counter (docs/world.md; what's underneath it, docs/narrative.md). Three hundred years ago
/// the Locking shut every treasure in Aldmark into a warded chest, to starve Avarax, the Gilded Maw, warded under the
/// capital behind the Gilded Gate, and gave it laws that bind him first: he can't take, only be given, sold or pledged.
/// So the Gilded Guild lends, and every Guild paper is a pledge to him: what he owns turns to gold (the Gilding, Old
/// Haddon at the crossroads on the first morning). Twenty years ago Bram pledged his own hand to keep a marked child
/// from the Guild's Mark-Finders; this spring the Guild calls it in (the debt), and the gold climbs his arm. The King
/// pledged the Crown Chest, the heart of the Locking, and the Crown's note falls due on the season's last night: given
/// away to a Keywright with the whole Aldric Key, it can't be collected. Bram kept one shard; Mara, the last archmage of
/// the Keyhold, trades the others. Scenes play on the story mornings (the first day of weeks 1, 2, 4 and 6, and of the
/// first week after the debt: <see cref="TellsOn"/>), when the Guild comes (its parting line, <see cref="Parting"/>),
/// and at the end; a week without a story morning may have a note on its first day's card. Lines can carry a clue for
/// the case (<see cref="Line.Clue"/>, found when they're said: <see cref="Heard"/>).
/// </summary>
public static class Story
{
    public const string Bram = "bram", Vane = "vane", Hood = "hood", Nim = "nim", StrangerKey = "stranger", Mara = "mara", King = "king";

    /// <summary>The main story's log in the notebook (the Gilding, TaleBook.Gilding.cs), and its keys.</summary>
    public const string Tale = "gilding", Carted = "gild.carted", Told = "bram.told"; // loc: keys

    /// <summary>Old Haddon, gilded at the crossroads (wearing an old regular's figure until he's drawn), and Thornwick's three.</summary>
    public const string HaddonLook = "pell"; // loc: keys
    public static readonly string[] ThornwickGilded = { "hob", "gerda", "penn" }; // loc: keys

    /// <summary>The week Vane offers to tear up Bram's pledge for the shards (the Key for the debt: the Gate opens).</summary>
    public const int PledgeWeek = 6;

    /// <summary>How a season ended, as the profile keeps it for the next one (<see cref="End"/>).</summary>
    public const string Sung = "sung", Held = "held", Trial = "trial", Free = "free", Lost = "foreclosed", GateEnd = "gate"; // loc: keys

    /// <summary>How this season ended (or would, now).</summary>
    public static string End(GameState state) =>
        state.GateOpens ? GateEnd
        : state.VaultOpened ? state.SongWhole ? Sung : Held
        : state.DebtCleared ? state.OnTrial ? Trial : Free
        : Lost;

    /// <summary>
    /// The Gate opened at that season's end (it was lost, or the debt paid with no Key and no court, or by the Maw): the
    /// Locking turned the year back to its spring, and only the marked remember.
    /// </summary>
    public static bool TurnedBack(string end) => end is Free or Lost or GateEnd;

    /// <summary>The case's clues in lines that have just been said.</summary>
    public static void Heard(GameState state, IEnumerable<Line> lines)
    {
        foreach (var line in lines)
            if (line.Clue != null) state.Find(line.Clue);
    }

    /// <summary>The story's people, for the ledger's Friends page: Uncle Bram, Nim, Mister Vane, the Hooded Woman (Mara, once she says so).</summary>
    public static readonly string[] People = { Bram, Nim, Vane, Hood, Mara };

    public static string HoodName => Loc.T("The Hooded Woman");

    /// <summary>The Aldric Key comes in four shards: Bram's, and three the Hooded Woman trades for.</summary>
    public const int Pieces = 4;

    /// <summary>The week the Guild offers to take the shards off your hands for half that week's due.</summary>
    public const int OfferWeek = 4;

    /// <summary>
    /// Whether a week has a story morning (its first day): the welcome, Bram's secret and the Hooded Woman, Mara's
    /// truth, and the Guild's interest after the debt.
    /// </summary>
    public static bool TellsOn(int week) => week is 1 or 2 or 4 or GatheringWeek || week == Guild.Weeks + 1;

    /// <summary>The week Mara asks who'll stand with you (0.30's longer season): the Key's last stretch, before the debt's last week.</summary>
    public const int GatheringWeek = 6;

    /// <summary>
    /// The picture behind a story morning (assets/backgrounds/scene_*.png): Bram's back room for his welcome and his secret,
    /// the Keyhold for Mara's truth, the Guild's counting-house once the debt's paid.
    /// </summary>
    public static string Setting(int week) => week > Guild.Weeks ? "guild" : week >= OfferWeek ? "keyhold" : "bram";

    /// <summary>Who brings the Crown Chest to the counter (nobody's selling: it's yours, by the King's gift).</summary>
    public static readonly SellerDef VaultSeller = new()
    {
        Key = Hood, Name = HoodName, Tier = 4, Judge = Judgment.Clueless,
        Kinds = new[] { 1f, 1, 1, 1, 1, 1 }, Colours = new[] { 0f, 0, 0, 1 },
        Habit = Loc.T("Mara, the last archmage of the Keyhold, and Bram's old partner. She's hunted the Aldric Key for twenty years."),
        Pitches = new[] { Loc.T("The Crown Chest. The King gives it to you: it's yours by right. Open it, Keywright.") },
    };

    public static string Name(string speaker) => speaker switch
    {
        Bram => Hints.Mentor,
        Vane => Guild.Collector,
        Hood => HoodName,
        Mara => Loc.T("Mara"),
        King => Loc.T("King Aldric"),
        Nim => Peddler.Name,
        StrangerKey => Events.Stranger.Name,
        TaleBook.Magpie => Loc.T("The Magpie"),
        _ => Sellers.Find(speaker)?.Name ?? speaker,
    };

    /// <summary>
    /// The second evening: the Guild calls in Bram's pledge (on this run's terms), and Vane notices his glove. From the
    /// second season, Vane knows the Keywright: the career's question.
    /// </summary>
    public static List<Line> Intro(GameState state)
    {
        var lines = new List<Line>
        {
            state.Is(Characters.Kit)
                ? new(Vane, Loc.T($"Good evening. {Guild.Collector}, Collector for the Gilded Guild. Twenty years ago your uncle pledged the Guild something, for a favour. He's never paid a penny of interest on it."))
                : new(Vane, Loc.T($"Good evening. {Guild.Collector}, Collector for the Gilded Guild. Twenty years ago your partner pledged the Guild something, for a favour. He's never paid a penny of interest on it.")),
            new(Bram, Loc.T("I know what I pledged, and why. I'd pledge it again.")),
            new(Vane, Loc.T($"Then the Guild calls the pledge in, Bram. Twenty years of interest: {Money.Say(Enumerable.Range(1, Guild.Weeks).Sum(state.DueFor))}. {Capital(Sellers.Number(Guild.Weeks))} payments, one a week, starting small: {Money.Say(state.DueFor(1))}, {Sellers.Number(Guild.DaysPerWeek - GameState.IntroDay)} nights from now.")),
            new(Vane, Loc.T("Miss one, and the Guild collects the pledge, and the stall with it.")),
        };
        if (state.SeasonsBefore > 0)
            lines.Add(new(Vane, Loc.T("And you, Keywright. We've met before, I think. You always look at my ring like that."), "vane.again"));
        lines.Add(new(Vane, Loc.T("The interest is showing, I see, Bram. Do keep your hand warm. Good evening."), "vane.glove"));
        lines.Add(new(Bram, Loc.T("Every fifth night, then. And don't look at the glove. A good ear pays any debt: we'll talk in the morning.")));
        return lines;
    }

    /// <summary>
    /// A story morning (<see cref="TellsOn"/>): the welcome on the first day, Bram's secret and the Hooded Woman in week
    /// 2, Mara's truth in week 4 (the night the Guild makes its offer), the Guild's interest after the debt. Whenever she
    /// wants something for her next shard, she says what.
    /// </summary>
    public static List<Line> WeekStart(GameState state)
    {
        int week = state.Week;
        var lines = new List<Line>();
        switch (week)
        {
            case 1:
                // A season after the Gate opened is the same spring again: only the marked remember, and only a little.
                if (state.SeasonsBefore > 0 && TurnedBack(state.LastEnd))
                    lines.Add(new(Bram, Loc.T("Strange. I dreamt we lost the stall, and I was gold, standing at the crossroads. Then I woke, and it was spring."), "dream"));
                lines.Add(state.FirstDayTaught
                    ? new(Bram, Loc.T($"Another season at the crossroads, {state.Kin.Name}. The Guild hasn't come sniffing yet. Let's make the most of it."))
                    : Welcome(state));
                lines.Add(new(Bram, Loc.T("Have you seen the crossroads? Old Haddon the wheelwright is standing in the middle of it, gold from his " +
                                          "boots to his cap. Solid gold."), "statue"));
                lines.Add(new(Bram, Loc.T("The Guild foreclosed on him yesterday, and this morning he was gold. Folk are saying he stole a Locking " +
                                          "chest. Haddon? He wouldn't steal a nail.")));
                lines.Add(new(Bram, Loc.T("Folk bring us chests nobody else can open. You buy one, it opens for you, and you sell what's inside. " +
                                    "So listen before you buy: that's the whole trade.")));
                break;
            case 2:
                lines.Add(new(Bram, Loc.T("You've a right to know what these chests are. Three hundred years ago the Maw, Avarax, ruled this " +
                                    "realm through its gold. Every coin was his eye.")));
                lines.Add(new(Bram, Loc.T("King Aldric and the archmages couldn't kill him: you can't kill greed. So they locked every treasure " +
                                          "away, and warded the Maw under the capital, behind the Gilded Gate. The Locking, they called it.")));
                lines.Add(new(Bram, Loc.T("And this is my one secret: a shard of the Aldric Key, broken in four when the Locking was made. " +
                                    "It's yours now. Don't let the Guild see it.")));
                if (state.BramGloved)
                    lines.Add(new(Bram, Loc.T("And no, I won't talk about the glove. Not yet. I'll tell you when I can.")));
                lines.Add(state.Is(Characters.Kit)
                    ? new(Hood, Loc.T("So you're Bram's kin. You have the mark, and his ear for a chest, they say. I have something your " +
                                      "uncle has wanted for twenty years. Find me what I ask for, and it's yours."))
                    : new(Hood, Loc.T("So you're Bram's partner. You have the mark, and a good ear for a chest, they say. I have something Bram " +
                                      "has wanted for twenty years. Find me what I ask for, and it's yours.")));
                break;
            case 4:
                lines.Add(state.MapPieces >= 2
                    ? new(Bram, Loc.T("That's... a second shard. Where did she get it? Never mind. Never mind!"))
                    : new(Bram, Loc.T("That woman in the hood. I know that voice. Hear her out.")));
                // She gives her name with her hood up, and lowers it after.
                lines.Add(state.Is(Characters.Kit)
                    ? new(Hood, Loc.T("My name is Mara. I was the last archmage of the Keyhold. Your uncle and I found the first shard " +
                                      "together, a long time ago."))
                    : new(Hood, Loc.T("My name is Mara. I was the last archmage of the Keyhold. Bram and I found the first shard " +
                                      "together, a long time ago.")));
                lines.Add(new(Mara, Loc.T("The Locking has laws, and they bind the Maw first. A locked thing opens only for its rightful owner. " +
                                          "And what's pledged and not redeemed belongs to the one it was pledged to.")));
                lines.Add(new(Mara, Loc.T("The Maw can't take: he can only be given, sold or pledged. So the Guild lends, and every Guild paper " +
                                          "is a pledge to him. What he owns turns to gold. Old Haddon, for one."), "mara.laws"));
                lines.Add(new(Mara, Loc.T("And loose gold is his eye. With every tithe the Guild takes, he sees a little more of the realm he's " +
                                          "buying."), "mara.eyes"));
                if (!state.Clues.Contains(Told))
                {
                    lines.Add(state.Is(Characters.Kit)
                        ? new(Bram, Loc.T("Then hear it from me first, Kit. Twenty years ago the Mark-Finders came for you: a baby with the mark " +
                                          "on her palm. I had nothing they'd take but my own hand, mark and all."), Told)
                        : new(Bram, Loc.T("Then hear it from me first. Twenty years ago the Mark-Finders came for a marked baby: my niece, Kit. " +
                                          "I had nothing they'd take but my own hand, mark and all. She's safe in the north."), Told));
                    // (A season you'd worked it out in before, he's worn no glove: he says the rest without it.)
                    lines.Add(state.BramGloved
                        ? new(Bram, Loc.T("So now you know what's under the glove. The Guild's been collecting it a finger at a time. Don't " +
                                          "look like that: I'd do it again."))
                        : new(Bram, Loc.T("The Guild's been collecting it a finger at a time. Don't look like that: I'd do it again.")));
                }
                lines.Add(new(Mara, Loc.T("The whole Key opens the Crown Chest: King Aldric's own, with the heart of the Locking inside. The " +
                                    "Guild doesn't want your gold. It wants that chest.")));
                lines.Add(state.Tier >= 4
                    ? new(Bram, Loc.T($"And that old fellow at the Emporium, the one with the ring? Look at the seal on it, {state.Kin.Name}. That's the King. He's been testing you."))
                    : new(Mara, Loc.T("The King knows. He's looking for a Keywright he can trust with the Crown Chest. Be that Keywright.")));
                if (state.MapPieces > 0)
                    lines.Add(new(Bram, Loc.T("Vane's been asking about the shards. Whatever he offers you tonight, think hard.")));
                break;
            case GatheringWeek:
                lines.AddRange(Gathering(state));
                break;
            case Guild.Weeks + 1:
                lines.Add(new(Vane, Loc.T("The tithe is paid. But the Gate is hungry, and the Guild has decided you owe it something for its patience.")));
                lines.Add(new(Bram, Loc.T("They'll keep coming, and every week they'll want half as much again. Let's see how long we can make them wait.")));
                break;
        }
        var ask = state.Commissions.FirstOrDefault(c => c.Story);
        if (ask != null) lines.Add(new(week >= OfferWeek ? Mara : Hood, HoodRequest(state, ask)));
        return lines;
    }

    /// <summary>What a story morning holds, in a few words (the calendar's): 20 letters at most.</summary>
    public static string Teaser(int week) => week switch
    {
        1 => Loc.T("Bram's welcome"),
        2 => Loc.T("Bram's secret"),
        4 => Loc.T("The Hooded Woman"),
        GatheringWeek => Loc.T("Who stands with you"),
        _ => Loc.T("The Guild's terms"),
    };

    /// <summary>What the Hooded Woman asks for her next shard, and by when.</summary>
    static string HoodRequest(GameState state, Commission ask) =>
        Loc.T($"{HoodAsks(ask)} Bring me {ask.Phrase} {(state.DebtCleared ? Loc.T("this week") : Loc.T("before the Guild comes again"))}, and the next shard is yours.");

    /// <summary>
    /// A week without a story morning: a word on its first day's card, or null. The Hooded Woman's note when she wants
    /// something; the debt's last week.
    /// </summary>
    public static string WeekNote(GameState state)
    {
        if (TellsOn(state.Week)) return null;
        var ask = state.Commissions.FirstOrDefault(c => c.Story);
        if (ask != null) return Loc.T($"A note from the Hooded Woman: \"{HoodRequest(state, ask)}\"");
        if (state.MapGiven && state.Week == OfferWeek + 1)
            return Loc.T("A note from the Hooded Woman: \"You gave them the shards. Now the Guild will have the Crown Chest. Pay your debt: it's all you have left.\"");
        if (state.Week == Guild.Weeks)
            return state.DebtCleared ? Loc.T("The last week of the tithe, and it's already paid: nobody's coming for anything this week.")
                : state.Clues.Contains("vane.kings") ? Loc.T("The debt's last week, and the Crown's note falls due the same night. Pay this, and the stall is yours.")
                : Loc.T("The debt's last week. Pay this, and the stall is yours. Truly yours.");
        return null;
    }

    /// <summary>What the Hooded Woman says when she asks for the next shard's price: why she wants it.</summary>
    public static string HoodAsks(Commission ask) => ask.Kind == CommissionKind.Enchanted
        ? Loc.T("Something enchanted. The last shard was hidden with magic, and only magic will find it.")
        : ask.Category switch
        {
            Category.Scrolls => Loc.T("A spellbook. The Keyhold wrote down where it hid every shard, in spells only a Keyhold book can hold."),
            Category.Gems => Loc.T("A gem. The Key was set with them, and they remember where the rest of it went."),
            Category.Coins => Loc.T("Old coin. The Key-smiths were paid in it, and every coin was stamped with where they worked."),
            _ => Loc.T("A blade. The Wardens who carried the shards away were buried with their swords."),
        };

    public static string HoodThanks(int piece) => piece switch
    {
        1 => Loc.T("Good. Here: the second shard. Don't let the Guild see it."),
        2 => Loc.T("The third shard. One more, and the Key is whole."),
        _ => Loc.T("The last shard. Bram will know what to do with it. So will I."),
    };

    /// <summary>The Guild's offer, on the night of <see cref="OfferWeek"/>.</summary>
    public static List<Line> Offer(int pieces) => new()
    {
        new(Vane, Loc.T($"Before you pay. The Guild knows about the Key. {Count(pieces)}, I hear.")),
        new(Vane, Loc.T("Give them to me, and the Guild will forget half of tonight's tithe. Generous, no?")),
    };

    static string Count(int pieces) => Capital(Sellers.Number(pieces)) + (pieces == 1 ? Loc.T(" shard") : Loc.T(" shards"));

    static string Capital(string word) => char.ToUpperInvariant(word[0]) + word[1..];

    public static string OfferTaken => Loc.T("A sensible choice. The Guild remembers its friends. So does its master.");
    public static string OfferRefused => Loc.T("Pity. The Guild remembers that too.");

    /// <summary>
    /// Vane's offer on the sixth Guild night (<see cref="PledgeWeek"/>): Bram's pledge torn up, the whole debt with it,
    /// for the shards. Taken, the Maw has the Key and the Gate opens (<see cref="GameState.TakeMawBargain"/>).
    /// </summary>
    public static List<Line> PledgeOffer(GameState state) => new()
    {
        new(Vane, Loc.T("Before you pay. The gold's at Bram's throat, I see. The Guild could tear up his pledge tonight, and the whole debt with it.")),
        new(Vane, Loc.T($"All it asks is the Key. {Count(state.MapPieces)}, I hear. Such a small thing, for a man's life.")),
    };

    public static string PledgeTaken => Loc.T("A sensible choice. Bram's hand is his own again, and the Key is my master's.");
    public static string PledgeRefused => Loc.T("Then the gold goes on climbing. Do give him my regards.");

    /// <summary>
    /// What Vane says as he goes, once the night's due is settled: the week's cliffhanger (docs/narrative.md, "The
    /// season"), or whose note the Guild is collecting tonight. Null when there's nothing more to say.
    /// </summary>
    public static Line Parting(GameState state)
    {
        // Someone the season made yours, whose note to the Guild falls due tonight, unpaid.
        if (state.Claims.Where(c => c.Value <= state.Week).Select(c => c.Key).OrderBy(k => k, System.StringComparer.Ordinal).FirstOrDefault() is string owed)
            return new(Vane, Loc.T($"One more thing. The Guild has a debt of its own to collect tonight: {Sellers.Person(owed).Name}. We'll send the cart."));
        if (state.DebtCleared) return null;
        return state.Week switch
        {
            1 => new(Vane, Loc.T("Don't mind the cart at the crossroads tonight. The Guild collects what it's owed. Even statues."), "cart"),
            2 => state.BramGloved
                ? new(Vane, Loc.T("Your glove's slipped, Bram. Gold to the knuckle already? The hand first. Then the arm."), "bram.fingers")
                : new(Vane, Loc.T("Gold to the knuckle already, Bram? The hand first. Then the arm."), "bram.fingers"),
            3 => new(Vane, Loc.T("Have you seen Thornwick's square? So many statues. Every one of them someone who didn't pay.")),
            4 => new(Vane, Loc.T("A word of advice, Keywright: even kings pledge. And the Crown's note falls due on the season's last night."), "vane.kings"),
            5 => new(Vane, Loc.T("Ten days, Keywright. Then the Guild collects the Crown Chest, in person.")),
            PledgeWeek => new(Vane, Loc.T("Five days. The Guild will be at the palace on the last night. Do come and watch.")),
            _ => null,
        };
    }

    /// <summary>The debt is paid: the last due, or all that's left of it at once, ahead of time.</summary>
    public static List<Line> Cleared(GameState state)
    {
        if (state.GateOpens)
            return new List<Line>
            {
                new(Vane, Loc.T("The Guild's tithe is paid, Keywright. Every coin. By a friend of the Guild. You know which one.")),
                new(Bram, Loc.T("What did you do? The Key... where's the Key? What did you DO?")),
                new(Mara, Loc.T("The Gate's open, Bram. I can feel it from here. The Maw has the Key, and the Locking won't hold the year.")),
            };
        int early = Guild.Weeks - state.Week;
        string when = early > 0
            ? Loc.Plural(early, $"{Capital(Sellers.Number(early))} week early", $"{Capital(Sellers.Number(early))} weeks early")
            : Loc.T("Before I've even come for it");
        var lines = early > 0 || !state.DuesTonight
            ? new List<Line>
            {
                new(Vane, Loc.T($"All of it? Now? {when}. Then the Guild... has no further claim on Bram's stall.")),
                new(Bram, Loc.T("Free and clear, and ahead of time! Twenty years, and the stall is ours.")),
            }
            : new List<Line>
            {
                new(Vane, Guild.Paid(Guild.Weeks)),
                new(Bram, Loc.T("Free and clear! Twenty years, and the stall is ours.")),
            };
        // His pledge is redeemed: the gold's gone from his hand.
        lines.Add(new(Bram, Loc.T("And look at my hand. It's warm. It's mine again.")));
        if (state.MapPieces >= Pieces && state.Week <= Guild.Weeks) lines.AddRange(MapWhole(state));
        else if (state.MapGiven)
            lines.Add(new(Bram, Loc.T("And the Guild has the shards. Somewhere under the palace, they're looking for the Crown Chest. Let them look.")));
        else if (state.Week < Guild.Weeks)
        {
            lines.Add(new(Bram, Loc.T($"And we've {Sellers.Number(state.MapPieces)} of the Key's {Sellers.Number(Pieces)} shards. Mara will keep asking: keep trading, and we may find the Crown Chest yet.")));
            lines.Add(new(Bram, Loc.T("But the Crown's note falls due on the season's last night. If the Key isn't whole by then, the Guild collects the Crown Chest.")));
        }
        else
            lines.Add(new(Bram, Loc.T("But we haven't the whole Key, and the Crown's note falls due at midnight. Whatever the Guild collects tonight, it won't be us.")));
        return lines;
    }

    /// <summary>
    /// The Key is whole and the debt is paid: Mara comes for Bram, the King brings the Crown Chest, and whoever stands with
    /// you holds the Guild back while it's opened (<see cref="Allies"/>).
    /// </summary>
    public static List<Line> MapWhole(GameState state = null)
    {
        var lines = new List<Line>
        {
            new(Mara, Loc.T("Bram. It's been a long time.")),
            new(Bram, Loc.T("Mara. The Key. You found all of it.")),
            new(Mara, Loc.T("Your Keywright did the finding. And the King has come himself, with the Crown Chest. Quickly: the Guild is a day behind us.")),
        };
        var allies = state == null ? new List<Line>() : Allies(state, tonight: true);
        if (state != null && allies.Count == 0) lines.Add(new(Mara, Loc.T("There's nobody to hold them back. We'll have to be quick.")));
        lines.AddRange(allies.Take(4));
        lines.Add(new(King, Loc.T("Three hundred years it's waited in my family. My note falls due at midnight, and a thing given away can't " +
                                  "be collected: it's yours by right now, Keywright. It opens for nobody else.")));
        return lines;
    }

    /// <summary>
    /// Week 6's story morning: the Guild knows about the Key, and Mara asks who'll stand with you when it's whole. Each
    /// friend you've made says so (<see cref="Allies"/>).
    /// </summary>
    static List<Line> Gathering(GameState state)
    {
        var lines = new List<Line>
        {
            new(Bram, Loc.T("Vane was at the Crossed Keys last night, asking about the Key. Asking who your friends are.")),
            new(state.MapPieces > 0 && !state.MapGiven ? Mara : Bram, state.MapGiven
                ? Loc.T("The Guild has the shards, and it's digging under the palace for the Crown Chest. When it finds it, we'll need every friend we've got.")
                : Loc.T("When the Key is whole, the Guild will come for it with everything it has. Who'll stand with you when it does?")),
        };
        // The King's plan: the Key whole by the last night, or his court with the Guild's own papers.
        if (!state.MapGiven)
            lines.Add(new(Mara, Loc.T("The King has sent word. If the Key is whole by the season's last night, he'll give the Crown Chest to " +
                                      "a Keywright, and a thing given away can't be collected."), "mara.key"));
        lines.Add(new(Mara, Loc.T("Or bring his court the Guild's own papers. A pledge made to someone who hides who they are isn't given " +
                                  "freely: prove the Guild hid its master, and every pledge it took is void."), "mara.court"));
        var allies = Allies(state, tonight: false);
        lines.AddRange(allies.Take(4));
        lines.Add(allies.Count == 0
            ? new(Bram, Loc.T("Nobody, it seems. Well. We've the stall, and each other. That's more than I had at your age."))
            : new(Bram, allies.Count >= 3 ? Loc.T("That's a crowd, that is. Twenty years I traded alone, and look at you.")
                : Loc.T("It's a start. Make a few more friends before the end, if you can.")));
        return lines;
    }

    /// <summary>
    /// Who stands with you, by what the season's tales made of them: the powers who count you a friend, and people who
    /// owe you (or called a favour in already). Each says what they'll do; on the Crown Chest night, what they're doing.
    /// </summary>
    public static List<Line> Allies(GameState state, bool tonight)
    {
        var lines = new List<Line>();
        if (state.Friend(Faction.Wardens))
            lines.Add(new(TaleBook.Hollis, tonight ? Loc.T("The Wardens have the palace road. No Guild carriage gets past my toll tonight.")
                : Loc.T("The Wardens will hold the palace road, when it comes to it. You've sent enough chests home.")));
        if (state.Friend(Faction.Commons))
            lines.Add(new(Bram, tonight ? Loc.T("Half of Brindle walked to the palace to see it opened. The Guild's men can't get through the crowd.")
                : Loc.T("Brindle's with you: half the crossroads would walk to the palace if you asked them.")));
        if (state.Friend(Faction.Lockless))
            lines.Add(new(Bram, tonight ? Loc.T("A black feather on the counter: the Guild's carriages have lost their wheels in Lantern Lane. All of them.")
                : Loc.T("A black feather on the counter, and a note: \"When the Guild comes for you, the Lockless will be in its way.\"")));
        if (state.Friend(Faction.Crown) || state.OnTrial)
            lines.Add(new(TaleBook.Aldo, tonight ? Loc.T("The palace guard is at the Emporium's door. My Guild has no business here tonight.")
                : Loc.T("The Crown's with you, Keywright. I've a palace guard with very little to do.")));
        if (state.Friend(Faction.Wood))
            lines.Add(new(TaleBook.Hawthorn, tonight ? Loc.T("Hawthorn has grown across the Guild's road. They'll be all night cutting through it.")
                : Loc.T("The Wood keeps its own. When they come for you, they'll find thorns.")));
        if (state.Friend(Faction.Ironroot))
            lines.Add(new(TaleBook.Durgan, tonight ? Loc.T("Ironroot's smiths have barred the Gate road with iron. Let the Guild try lifting it.")
                : Loc.T("Ironroot owes you a crown. Ask, and you'll have a hundred dwarves at your door.")));
        // People who owe you, or came when you called: the season's friends by name (the factions say the rest).
        foreach (string person in state.Owed.Keys.Concat(state.CalledIn).Distinct().Where(p => !state.Gone.Contains(p)))
            if (FriendLine(person, tonight) is string line) lines.Add(new(person, line));
        if (state.Flagged(TaleBook.Apprentice) && !state.Gone.Contains(TaleBook.Pip))
            lines.Add(new(TaleBook.Pip, tonight ? Loc.T("I'll hold the lamp, master! I'm not scared. Much.") : Loc.T("I'll stand with you, master. I'm small, but I'm quick.")));
        return lines;
    }

    /// <summary>A friend by name, standing with you (a favour owed, or one already called in).</summary>
    static string FriendLine(string person, bool tonight) => person switch
    {
        TaleBook.Wynn => tonight ? Loc.T("The whole mill's here, and every one of us has a washing paddle.") : Loc.T("Send for me, and the whole mill comes."),
        TaleBook.Ada => tonight ? Loc.T("Rafe's sword's at the door, dear, and Jory's holding it.") : Loc.T("Rafe would have stood with you. So will we, dear."),
        TaleBook.Tobin => tonight ? Loc.T("I've brought two delvers and a lot of rope. Nobody's getting through that door.") : Loc.T("You dug me out of the Barrow. I'll dig you out of anything."),
        TaleBook.Bettany => tonight ? Loc.T("The Crossed Keys is closed tonight. Everyone's here instead.") : Loc.T("The Crossed Keys stands with you, love. Every last drunk in it."),
        TaleBook.Rowan => tonight ? Loc.T("A Gale at the door, with a Gale's blade. Let them come.") : Loc.T("The Gales pay their debts. Call, and I'll be there."),
        TaleBook.Nettle => tonight ? Loc.T("Goblins in the Guild's cellar. Lots of goblins. Very busy.") : Loc.T("The clan comes when you call. All of it. Even the uncles."),
        TaleBook.Isaura => tonight ? Loc.T("House Vell is at the palace, and so, for once, is my brother.") : Loc.T("House Vell owes you. We pay our debts, Keywright."),
        TaleBook.Durgan => tonight ? Loc.T("Ironroot's here, Keywright, with hammers.") : Loc.T("A hundred dwarves, when you need them."),
        TaleBook.Jasper => tonight ? Loc.T("A hedge-wright at your back. Two Keywrights are harder to stop than one.") : Loc.T("When the Guild comes, send for the hedge-wright."),
        TaleBook.Quire => tonight ? Loc.T("I have the Roll, and the Roll says whose the Crown Chest is. Yours.") : Loc.T("The Roll will say whose the Key is, when it matters."),
        TaleBook.Ilse => tonight ? Loc.T("Every lamp in the Arcanum is lit tonight. The Guild won't find a shadow to hide in.") : Loc.T("The Arcanum's apprentices will stand with you. Some of the archmages, too."),
        TaleBook.Hesper => tonight ? Loc.T("Two whole families are here, dear: the ones you married off. They'd like a word with the Guild.") : Loc.T("Two families owe you their wedding, dear. They'll come."),
        TaleBook.Garrick => tonight ? Loc.T("Brought the plough horses. The Guild's carriages can go round.") : Loc.T("Call, and the whole fair comes, horses and all."),
        TaleBook.Jory => tonight ? Loc.T("I carried chests for you. Tonight I'll carry anything that tries to get past this door.") : Loc.T("I owe you my freedom. Call, and I'll come."),
        TaleBook.Corvin => tonight ? Loc.T("The Wet Hen's at the river stairs, if anyone needs to leave in a hurry.") : Loc.T("Need anything carried away from the Guild quietly? Ask for the Wet Hen."),
        _ => null,
    };

    /// <summary>
    /// Between runs, once (0.32): a season that ended without a Heartsong ever heard gets Bram's word on them, so a new
    /// player knows what they're listening for: the rarest things in Aldmark, and what one does on display.
    /// </summary>
    public static List<Line> SongsUnheard() => new()
    {
        new(Bram, Loc.T("No Heartsong this season. Don't take it hard: most seasons never hear one.")),
        new(Bram, Loc.T("They're the rarest things in Aldmark. Nine of them, things loved more than gold, sung into the Locking and " +
                        "lost among millions of chests. About one season in three turns one up.")),
        new(Bram, Loc.T("Only a Keywright hears one. At the gentlest shake it sings a tune no other thing makes, and the seller never " +
                        "knows it's there: they price the chest as if it weren't.")),
        new(Bram, Loc.T("Nobody would pay a penny for one. But on display, each does one great thing for the shop that no gold can " +
                        "buy, for as long as it sings: the kind of thing a whole season can be built around.")),
        new(Bram, Loc.T("And once anyone at this stall has heard one, it's in the songbook for good. Hear all nine, over all your seasons, and " +
                        "the Crown Chest ends another way.")),
        new(Bram, Loc.T("So shake every chest gently first, and listen. The collection's Heartsongs page keeps a place for each of the nine.")),
    };

    /// <summary>
    /// The season's end for the main story (0.33): what became of the gilded, what you worked out, and (once you know why
    /// the spring comes round again) what an open Gate means.
    /// </summary>
    static IEnumerable<Line> MainEnding(GameState state)
    {
        string end = End(state);
        if (end is Held or Sung)
            yield return new(Bram, Loc.T("Old Haddon walked home from the Gate the night the Locking was sung. He's making wheels again, and he won't sign anything."));
        else if (end == Trial)
            yield return new(Bram, Loc.T("The King's court found every Guild pledge void: made to the Maw, in secret. Old Haddon walked home from the Gate."));
        else
            yield return new(Bram, Loc.T("Old Haddon never came home. Nobody's seen the Guild's cart come back from the palace road."));
        int solved = state.Solved.Count;
        if (solved > 0)
            yield return new(Bram, Loc.Plural(solved, $"You worked out {solved} of the season's questions. Keep the notebook: what you know stays with you.",
                $"You worked out {solved} of the season's questions. Keep the notebook: what you know stays with you."));
        else if (state.OpenQuestions.Any())
            yield return new(Bram, Loc.T("The questions in your notebook are still open. Keep it: what you learn stays with you."));
        // What the Lantern Market and the back room saw (0.33).
        if (state.EyesSold > 0)
            yield return new(Bram, Loc.Plural(state.EyesSold, $"And you sold {state.EyesSold} gold thing down the Gate. Every coin of it an eye for him. I hope it was worth it.",
                $"And you sold {state.EyesSold} gold things down the Gate. Every coin of it an eye for him. I hope it was worth it."));
        if (state.MimicsWoke > 0)
            yield return new(Bram, Loc.T("That mimic of yours ate the gold off the gilded, and they walked home. The Arcanum wants to know how you fed it."));
        else if (state.Mimic != null)
            yield return new(Bram, Loc.T("And there's still a mimic in the back room, asleep on the gold. Don't wake it."));
        if (TurnedBack(end) && state.Truths.Contains(Case.Spring))
            yield return new(Bram, Loc.T("And the Gate's open. You know what that means: it'll be spring again, and only the marked will remember."));
    }

    /// <summary>
    /// The season's end (0.30): what became of everyone whose tale began, a line each, by what you chose (each tale's
    /// <see cref="TaleDef.Ending"/>), after the run's last scene, won or lost.
    /// </summary>
    /// <summary>
    /// Bram's welcome on the first morning (0.32): Kit's uncle hands her the stall and the mark that's passed to her; the
    /// others aren't family, and each came to him for reasons of their own (Meta.cs, TaleBook.Kin.cs).
    /// </summary>
    static Line Welcome(GameState state) => state.Character switch
    {
        Characters.Hesk => new(Bram, Loc.T("So you're Hesk. A partner with the mark and an honest scale: I'll take it. Mind the stall, mind your " +
                                           "back, and half of it's yours when the Guild's paid.")),
        Characters.Rosa => new(Bram, Loc.T("Rosa Brightwater, as I live. Your mother sold me my first lock-pick, forty years back. My mark's faded: " +
                                           "mind the stall, partner, and half of it's yours when the Guild's paid.")),
        Characters.Isolde => new(Bram, Loc.T("Lady Marchmont. Your grandfather and I opened chests together, before your house was grand. My mark's " +
                                             "faded: mind my stall, and put your collection on the shelf. The Guild's paid out of the takings.")),
        _ => state.BramGloved
            ? new(Bram, Loc.T($"There you are, {state.Kin.Name}! Don't mind the glove. Look at your hand instead: the mark's yours now. And so is the stall."))
            : new(Bram, Loc.T($"There you are, {state.Kin.Name}! Look at your hand: the mark's yours now. And so is the stall.")),
    };

    public static List<Line> SeasonEnd(GameState state)
    {
        var lines = new List<Line> { new(Bram, Loc.T("So. Let me tell you what became of everyone, this season.")) };
        lines.AddRange(MainEnding(state));
        foreach (var tale in TaleBook.All.Where(t => state.Drew(t.Key) && t.Ending != null))
            if (tale.Ending(state) is Line line) lines.Add(line);
        lines.Add(state.Character switch
        {
            Characters.Hesk => new(Bram, Loc.T("And you, Hesk. A grocer from a drowned town, with the mark on your hand. What becomes of you is up to you.")),
            Characters.Rosa => new(Bram, Loc.T("And you, Rosa. A Brightwater of Lantern Lane, with the mark on your hand. What becomes of you is up to you.")),
            Characters.Isolde => new(Bram, Loc.T("And you, my lady. The last Marchmont, with the mark on your hand. What becomes of you is up to you.")),
            _ => new(Bram, Loc.T("And you. Bram's kin, with the mark on your hand. What becomes of you is up to you.")),
        });
        return lines;
    }

    /// <summary>
    /// After the Crown Chest's been opened. It holds the Locking's tune; the Heartsongs hold its nine verses. With fewer
    /// than all nine known to Bram's kin, Mara renews it for another three hundred years. With all nine she sings it whole,
    /// and a lock sung whole is an ending: the Maw starves, the Gate is only a gate, and every chest in Aldmark opens.
    /// </summary>
    public static List<Line> VaultEpilogue(GameState state)
    {
        int known = Heartsongs.All.Count(h => state.Songbook.Contains(h.Key));
        if (state.SongWhole)
            return new()
            {
                new(Mara, Loc.T("The tune's in here. And your songbook has every verse: all nine Heartsongs. Then we don't renew the Locking. We finish it.")),
                new(Mara, Loc.T("Listen. That's the Maw. Starving, with nothing left to want. The Gate's just a gate now.")),
                new(Bram, Loc.T("And look at my hand, Mara. Warm. Every pledge to the Maw is void, and every one of the gilded is waking up.")),
                new(Bram, Loc.T("And the chests, Mara. Every chest in Aldmark. They're opening. All of them, all at once.")),
                new(Mara, Loc.T("The Guild has nothing to sell, and nothing to keep. Go home, Mister Vane.")),
                new(Bram, Loc.T("Three hundred years of treasure, back in the light. We're going to need a bigger stall.")),
            };
        var lines = new List<Line>
        {
            new(Mara, Loc.T("Listen. The Gate's gone quiet. The Locking holds: another three hundred years, if we're lucky.")),
            new(Bram, Loc.T("And every pledge to the Maw is void. Old Haddon will walk home from the Gate tonight, and everyone the Guild collected with him.")),
            new(Bram, Loc.T("King Aldric's own hoard, on our counter. The old man said it's ours to keep.")),
        };
        lines.Add(known == 0
            ? new(Mara, Loc.T("The Locking was a song, in nine verses: nine Heartsongs, hidden in chests. If you ever hear one sing, keep it."))
            : new(Mara, Loc.T($"Your songbook holds {Sellers.Number(known)} of the nine Heartsongs. Find every verse, and the next Keywright won't renew the Locking. They'll finish it.")));
        // The Keyhold's last secret, and the career's question.
        lines.Add(new(Mara, Loc.T("And one more thing, which the Keyhold never told anyone. We wrote a clause into the Locking: if ever the Gate " +
                                  "opens, the year turns back to its spring, and only the marked remember."), "mara.clause"));
        lines.Add(new(Bram, Loc.T("The Guild will keep coming, you know. Debts are funny things. But now we can afford to make them wait.")));
        return lines;
    }

    public static List<Line> Foreclosed(GameState state)
    {
        var lines = new List<Line>
        {
            new(Vane, Guild.Foreclosed),
            new(Bram, state.DebtCleared ? Loc.T($"{state.Week} weeks, and every coin of the tithe paid. Let them have the building: they can't have the mark.")
                : state.Week <= 2 ? Loc.T("So soon? Well. We'll start again somewhere. A good ear is never broke for long.")
                : Loc.T($"{state.Week} weeks. You did better than I ever did. We'll start again somewhere.")),
        };
        // The pledge was never redeemed: the Guild collects it.
        if (!state.PledgeRedeemed)
            lines.Add(new(Bram, Loc.T("And they'll collect my pledge with it, I expect. Don't look like that. I'd do it again.")));
        return lines;
    }
}
