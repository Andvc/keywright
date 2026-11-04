using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>One line in a scene: who says it (a portrait key) and what they say.</summary>
public sealed record Line(string Speaker, string Text);

/// <summary>
/// What's going on behind the counter (docs/world.md). Three hundred years ago the Locking shut every treasure in
/// Aldmark into a warded chest, to starve Avarax, the Gilded Maw, chained behind the Gilded Gate. Only a Keywright can
/// open one, once it's rightfully theirs, and every one opened loosens the Locking. Bram opened chests unlicensed for
/// twenty years: the Gilded Guild, which licenses Keywrights and secretly serves the Maw, wants its back-tithe. Bram
/// kept one shard of the Aldric Key; Mara, the last archmage of the Keyhold, trades the others for things only a
/// Keywright's chests hold. The whole Key opens the Crown Chest, which renews the Locking. Scenes play on the story
/// mornings (the first day of weeks 1, 2 and 4, and of the first week after the debt: <see cref="TellsOn"/>), when the
/// Guild comes, and at the end; a week without a story morning may have a note on its first day's card.
/// </summary>
public static class Story
{
    public const string Bram = "bram", Vane = "vane", Hood = "hood", Nim = "nim", StrangerKey = "stranger", Mara = "mara", King = "king";

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

    /// <summary>The second evening: the Guild introduces itself, and what Bram owes (on this run's terms).</summary>
    public static List<Line> Intro(GameState state) => new()
    {
        new(Vane, Loc.T($"Good evening. {Guild.Collector}, Collector for the Gilded Guild. Every Keywright in Aldmark pays the Guild a tithe, to keep the Gate. Your uncle never paid a penny.")),
        new(Bram, Loc.T("Twenty years I opened chests for folk who couldn't afford a Guild Keywright! For pennies!")),
        new(Vane, Loc.T($"Twenty years of tithe, Bram: {Money.Say(Enumerable.Range(1, Guild.Weeks).Sum(state.DueFor))}. The Guild will be reasonable. {Capital(Sellers.Number(Guild.Weeks))} payments, one a week, starting small: {Money.Say(state.DueFor(1))}, {Sellers.Number(Guild.DaysPerWeek - GameState.IntroDay)} nights from now.")),
        new(Vane, Loc.T("Miss one, and the Guild seals the stall. Good evening, Keywright.")),
        new(Bram, Loc.T("Every fifth night, then. Don't look like that: a good ear pays any debt. We'll talk in the morning.")),
    };

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
                lines.Add(state.FirstDayTaught
                    ? new(Bram, Loc.T($"Another season at the crossroads, {state.Kin.Name}. The Guild hasn't come sniffing yet. Let's make the most of it."))
                    : Welcome(state));
                lines.Add(new(Bram, Loc.T("Folk bring us chests nobody else can open. You buy one, it opens for you, and you sell what's inside. " +
                                    "So listen before you buy: that's the whole trade.")));
                break;
            case 2:
                lines.Add(new(Bram, Loc.T("You've a right to know what these chests are. Three hundred years ago the Maw, Avarax, ruled this " +
                                    "realm through its gold. Every coin was his eye.")));
                lines.Add(new(Bram, Loc.T("King Aldric and the archmages locked every treasure away and chained the Maw behind the Gilded " +
                                    "Gate. The Locking, they called it. The chests are what's left.")));
                lines.Add(new(Bram, Loc.T("And this is my one secret: a shard of the Aldric Key, broken in four when the Locking was made. " +
                                    "It's yours now. Don't let the Guild see it.")));
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
                lines.Add(new(Hood, Loc.T("My name is Mara. I was the last archmage of the Keyhold. Your uncle and I found the first shard " +
                                    "together, a long time ago.")));
                lines.Add(new(Mara, Loc.T("The whole Key opens the Crown Chest: King Aldric's own, with the heart of the Locking inside. The " +
                                    "Guild doesn't want your gold. It wants that chest.")));
                lines.Add(new(Mara, Loc.T("Every tithe the Guild takes goes down through the Gilded Gate. The Maw is buying his way out, a coin " +
                                    "at a time. And every chest you open loosens the Locking.")));
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
        if (state.MapPieces >= Pieces) lines.AddRange(MapWhole(state));
        else if (state.MapGiven)
            lines.Add(new(Bram, Loc.T("And the Guild has the shards. Somewhere under the palace, they're looking for the Crown Chest. Let them look.")));
        else if (state.Week < Guild.Weeks)
            lines.Add(new(Bram, Loc.T($"And we've {Sellers.Number(state.MapPieces)} of the Key's {Sellers.Number(Pieces)} shards. Mara will keep asking: keep trading, and we may find the Crown Chest yet.")));
        else
            lines.Add(new(Bram, Loc.T("The Crown Chest can wait. The Key's not going anywhere, and neither are we.")));
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
        lines.Add(new(King, Loc.T("Three hundred years it's waited in my family, and not one of us could open it. It's yours by right now, Keywright. " +
                            "It opens for nobody else.")));
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
        _ => new(Bram, Loc.T($"There you are, {state.Kin.Name}! My mark's faded, but look at your hand: it's yours now. And so is the stall.")),
    };

    public static List<Line> SeasonEnd(GameState state)
    {
        var lines = new List<Line> { new(Bram, Loc.T("So. Let me tell you what became of everyone, this season.")) };
        foreach (var tale in TaleBook.All.Where(t => state.Drew(t.Key) && t.Ending != null))
            if (tale.Ending(state) is Line line) lines.Add(line);
        if (lines.Count == 1) return new List<Line>();
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
                new(Bram, Loc.T("And the chests, Mara. Every chest in Aldmark. They're opening. All of them, all at once.")),
                new(Mara, Loc.T("The Guild has nothing to sell, and nothing to keep. Go home, Mister Vane.")),
                new(Bram, Loc.T("Three hundred years of treasure, back in the light. We're going to need a bigger stall.")),
            };
        var lines = new List<Line>
        {
            new(Mara, Loc.T("Listen. The Gate's gone quiet. The Locking holds: another three hundred years, if we're lucky.")),
            new(Bram, Loc.T("King Aldric's own hoard, on our counter. The old man said it's ours to keep.")),
        };
        lines.Add(known == 0
            ? new(Mara, Loc.T("The Locking was a song, in nine verses: nine Heartsongs, hidden in chests. If you ever hear one sing, keep it."))
            : new(Mara, Loc.T($"Your songbook holds {Sellers.Number(known)} of the nine Heartsongs. Find every verse, and the next Keywright won't renew the Locking. They'll finish it.")));
        lines.Add(new(Bram, Loc.T("The Guild will keep coming, you know. Debts are funny things. But now we can afford to make them wait.")));
        return lines;
    }

    public static List<Line> Foreclosed(GameState state) => new()
    {
        new(Vane, Guild.Foreclosed),
        new(Bram, state.DebtCleared ? Loc.T($"{state.Week} weeks, and every coin of the tithe paid. Let them have the building: they can't have the mark.")
            : state.Week <= 2 ? Loc.T("So soon? Well. We'll start again somewhere. A good ear is never broke for long.")
            : Loc.T($"{state.Week} weeks. You did better than I ever did. We'll start again somewhere.")),
    };
}
