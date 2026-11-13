using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>One line in a scene: who says it (a portrait key), what they say, and the Inquiry entry it notes, if any.</summary>
public sealed record Line(string Speaker, string Text, string Clue = null);

/// <summary>
/// What's going on behind the counter (docs/world.md; what's underneath it, docs/narrative.md). Bram minds the stall
/// with you for the first week; on the first Guild night Vane brings the treasury's summons, and he goes. The Guild is
/// kind: it writes, it sends the treasury key's pieces as you earn them, and it invites you to the treasury on the night
/// Bram's account is paid with the key whole. The treasury's chest is the Maw. After the first swallowing you remember
/// every season, and Vane speaks for something that remembers too. Scenes play on the story mornings (the first day of
/// weeks 1, 2 and 6, and of the first week after the debt: <see cref="TellsOn"/>), when the Guild comes (its parting
/// line, <see cref="Parting"/>), at the treasury and at the season's end. A line can note an Inquiry entry
/// (<see cref="Line.Clue"/>, noted when it's said: <see cref="Heard"/>).
/// </summary>
public static class Story
{
    public const string Bram = "bram", Vane = "vane", Hood = "hood", Nim = "nim", StrangerKey = "stranger", Mara = "mara", King = "king";

    /// <summary>You, remembering (the character minding the stall): the lines only you could say.</summary>
    public const string Self = "self";

    /// <summary>The main story's log in the notebook's Stories page (TaleBook.Campaign.cs).</summary>
    public const string Tale = "commission"; // loc: keys

    /// <summary>Old Haddon's look (an old regular's figure, until he's drawn).</summary>
    public const string HaddonLook = "pell"; // loc: keys

    /// <summary>The week of the gathering: who'll stand with you at the treasury.</summary>
    public const int GatheringWeek = 4;

    /// <summary>The lines' notes for the Inquiry, once they've been said.</summary>
    public static void Heard(GameState state, IEnumerable<Line> lines)
    {
        foreach (var line in lines)
            if (line.Clue != null) state.Note(line.Clue);
    }

    /// <summary>The story's people, for the ledger's Friends page: Uncle Bram, Nim, Mister Vane, the Hooded Woman (Mara, once she says so).</summary>
    public static readonly string[] People = { Bram, Nim, Vane, Hood, Mara };

    public static string HoodName => Loc.T("The Hooded Woman");

    /// <summary>Whether a week has a story morning (its first day): the welcome, the Guild's letter, the gathering, the Guild's interest after the debt.</summary>
    public static bool TellsOn(int week) => week is 1 or 2 or GatheringWeek || week == Guild.Weeks + 1;

    /// <summary>The picture behind a story morning: Bram's back room, the Guild's counting-house once the debt's paid.</summary>
    public static string Setting(int week) => week > Guild.Weeks ? "guild" : "bram";

    public static string Name(string speaker) => speaker switch
    {
        Bram => Hints.Mentor,
        Vane => Guild.Collector,
        Hood => HoodName,
        Mara => Loc.T("Mara"),
        King => Loc.T("King Aldric"),
        Nim => Peddler.Name,
        Self => Characters.Get(Hints.Kin).Name,
        StrangerKey => Events.Stranger.Name,
        TaleBook.Magpie => Loc.T("The Magpie"),
        _ => Sellers.Find(speaker)?.Name ?? speaker,
    };

    /// <summary>Whose picture a speaker wears: yours for your own lines, everyone else's own.</summary>
    public static string Look(string speaker) => speaker == Self ? Hints.Kin : Sellers.LookOf(speaker);

    /// <summary>Who tells the season's news when Bram's away: Bettany at the Crossed Keys, who hears everything first.</summary>
    public static string Narrator(GameState state) => state.BramHere ? Bram : TaleBook.Bettany;

    static string Capital(string word) => char.ToUpperInvariant(word[0]) + word[1..];

    // ---- The first week ----

    /// <summary>
    /// The second evening: Vane, the Guild's Collector, about Bram's old account (a business matter, kindly put). From
    /// the second season you remember, he knows you.
    /// </summary>
    public static List<Line> Intro(GameState state)
    {
        // A season after the first (0.37): everyone at the counter has heard this, so it's the figures and no more.
        if (state.Veteran)
        {
            var again = new List<Line>
            {
                new(Vane, Loc.T($"{Guild.Collector}, for the Gilded Guild, about Bram's account. {Money.Say(Enumerable.Range(1, Guild.Weeks).Sum(state.DueFor))} in all this season: {Sellers.Number(Guild.Weeks)} payments, one every fifth night, and the first is {Money.Say(state.DueFor(1))}, {Sellers.Number(Guild.DaysPerWeek - GameState.IntroDay)} nights from now.")),
            };
            if (state.Remembers && !state.CampaignDone)
                again.Add(new(Vane, Loc.T("And you, Keywright. We've met before, I think. You always look at my ring like that."), "vane.again"));
            again.Add(new(Bram, Loc.T("Every fifth night, as ever. A good ear pays any debt.")));
            return again;
        }
        var lines = new List<Line>
        {
            state.Is(Characters.Kit)
                ? new(Vane, Loc.T($"Good evening. {Guild.Collector}, Collector for the Gilded Guild. I'm here about your uncle's account: twenty years open, and the Guild would like it settled this season."))
                : new(Vane, Loc.T($"Good evening. {Guild.Collector}, Collector for the Gilded Guild. I'm here about Bram's account: twenty years open, and the Guild would like it settled this season.")),
            new(Bram, Loc.T("Twenty years. Has it been? Well. A debt's a debt.")),
            new(Vane, Loc.T($"{Money.Say(Enumerable.Range(1, Guild.Weeks).Sum(state.DueFor))} in all. {Capital(Sellers.Number(Guild.Weeks))} payments, one every fifth night, starting small: {Money.Say(state.DueFor(1))}, {Sellers.Number(Guild.DaysPerWeek - GameState.IntroDay)} nights from now.")),
            new(Vane, Loc.T("Miss one, and the Guild takes the stall in settlement. A formality. I'm sure it won't come to that.")),
            new(Vane, Loc.T("And each one paid, the Guild's carters move your stall a market further in: Thornwick, Crownhold, the Royal Emporium by the Gate. Richer chests pay richer dues.")),
            new(Vane, Loc.T("And the Guild is here to help, Keywright. We introduce customers, cart chests, lend where coin is short. Do ask.")),
        };
        if (state.Remembers && !state.CampaignDone)
            lines.Add(new(Vane, Loc.T("And you, Keywright. We've met before, I think. You always look at my ring like that."), "vane.again"));
        lines.Add(new(Bram, Loc.T("Every fifth night, then. A good ear pays any debt.")));
        return lines;
    }

    /// <summary>
    /// A story morning (<see cref="TellsOn"/>): the welcome on the first day (and, once you remember, what you remember),
    /// the Guild's letter in week 2, the gathering in week 6, the Guild's interest after the debt.
    /// </summary>
    public static List<Line> WeekStart(GameState state)
    {
        var lines = new List<Line>();
        switch (state.Week)
        {
            case 1:
                if (state.CampaignDone)
                {
                    lines.Add(new(Bram, Loc.T($"Another year at the crossroads, {state.Kin.Name}. The treasury's shut, the tea's hot, and the chests are waiting.")));
                    break;
                }
                if (state.Remembers)
                {
                    // How the last season ended (the profile's): swallowed, stayed away, or the last night came without the key.
                    lines.Add(state.LastEnd switch
                    {
                        Campaign.Swallowed or Campaign.Bargained => new(Self, Loc.T("Darkness. Then two taps on the counter, and one.")),
                        Campaign.Stayed => new(Self, Loc.T("I stayed away from the treasury. The dark came anyway, and then two taps on the counter, and one.")),
                        _ => new(Self, Loc.T("The last night came, and the dark with it. Then two taps on the counter, and one.")),
                    });
                    lines.Add(state.FirstDayTaught
                        ? new(Bram, Loc.T($"There you are, {state.Kin.Name}. Another season at the crossroads. The Guild hasn't come sniffing yet."))
                        : Welcome(state));
                    lines.Add(new(Self, Loc.T("The first morning, again. I remember the treasury. I remember it breathing. He doesn't."), "again"));
                    break;
                }
                lines.Add(state.FirstDayTaught || state.Veteran
                    ? new(Bram, Loc.T($"Another season at the crossroads, {state.Kin.Name}. The Guild hasn't come sniffing yet. Let's make the most of it."))
                    : Welcome(state));
                // What the trade is, and Bram's tap: said once a profile (0.37: every season before the first clear began with it).
                if (state.Veteran)
                {
                    lines.Add(new(Bram, Loc.T("You know the trade, and the chests know you. Two taps and one when I want you at the counter.")));
                    break;
                }
                lines.Add(new(Bram, Loc.T("Folk bring us chests nobody else can open. You buy one, it opens for you, and you sell what's inside. " +
                                          "So listen before you buy: that's the whole trade.")));
                lines.Add(new(Bram, Loc.T("Bettany from the Crossed Keys will be by. She hears everything first, and she'll tell you most of it.")));
                lines.Add(new(Bram, Loc.T("When I want you at the counter I'll tap it, twice and once. Like that. And mind: some chests listen back.")));
                break;
            case 2:
                // The Guild's letter, read at the counter (yours to read out: Vane isn't there).
                if (state.CampaignDone || state.BramHere) break;
                if (state.Remembers)
                {
                    lines.Add(new(Self, Loc.T("A letter with the Guild's seal: \"Bram's handover is taking longer than expected. Keep the shop running until he returns.\"")));
                    lines.Add(new(Self, Loc.T("\"Earn the treasury key, keep his account paid, and bring the key to the treasury on the season's last night.\""), "commission"));
                    lines.Add(new(Self, Loc.T("The same letter, word for word. Last time, the treasury breathed.")));
                    break;
                }
                if (state.Veteran)
                {
                    // A season after the first, before you remember: the same terms, in a line.
                    lines.Add(new(Self, Loc.T("A letter with the Guild's seal, and the terms it always sets: keep Bram's account paid every fifth night, earn the four pieces of the treasury key, and bring the key to the treasury on the season's last night."), "commission"));
                    break;
                }
                lines.Add(new(Self, Loc.T("A letter with the Guild's seal: \"Bram's handover is taking longer than expected. Keep the shop running until he returns.\"")));
                lines.Add(new(Self, Loc.T("\"Meanwhile the Guild's assessors will certify your work. Four marks of a master keywright earn the four pieces " +
                                          "of the treasury key, each with a master's fee: a hidden thing read true, a stolen chest settled, a commission " +
                                          "filled, an old lock picked without force.\"")));
                lines.Add(new(Self, Loc.T("\"Pay Bram's account as it falls due, every fifth night, and bring the whole key to the treasury on the " +
                                          "season's last night. His commission needs a second keywright. When it's done and his account is paid, he comes home.\""), "commission"));
                break;
            case GatheringWeek:
                lines.AddRange(Gathering(state));
                break;
            case Guild.Weeks + 1:
                lines.Add(new(Vane, Loc.T("The tithe is paid. But the Gate is hungry, and the Guild has decided you owe it something for its patience.")));
                lines.Add(new(Narrator(state), Loc.T("They'll keep coming, and every week they'll want half as much again. Let's see how long we can make them wait.")));
                break;
        }
        return lines;
    }

    /// <summary>What a story morning holds, in a few words (the calendar's): 20 letters at most.</summary>
    public static string Teaser(int week) => week switch
    {
        1 => Loc.T("Bram's welcome"),
        2 => Loc.T("The Guild's letter"),
        GatheringWeek => Loc.T("Who stands with you"),
        _ => Loc.T("The Guild's terms"),
    };

    /// <summary>
    /// A week without a story morning: a word on its first day's card, or null. Still no letter from Bram; the debt's last
    /// week, and the treasury.
    /// </summary>
    public static string WeekNote(GameState state)
    {
        if (state.CampaignDone) return null;
        // The last week's word stands whatever the morning told (0.36: the gathering is that morning's scene).
        if (state.Week == Guild.Weeks)
            return state.DebtCleared ? Loc.T("The last week of the tithe, and it's already paid: nobody's coming for anything this week.")
                : state.KeyWhole ? Loc.T("The debt's last week. Pay it, and the Guild's carriage takes you and the key to the treasury.")
                : Loc.T("The debt's last week. Pay this, and Bram's account is closed.");
        if (TellsOn(state.Week)) return null;
        if (state.Week == 3 && state.BramNotes) return Loc.T("Still no letter from Bram. The Guild's came on time.");
        return null;
    }

    /// <summary>The Guild's night in week 6: the treasury, and who'll stand with you there.</summary>
    static List<Line> Gathering(GameState state)
    {
        var lines = new List<Line>();
        if (state.CampaignDone) return lines;
        if (!state.Remembers)
        {
            if (state.BramNotes)
                lines.Add(new(Self, Loc.T("A letter with the Guild's seal: \"The treasury opens on the season's last night. Bram looks forward to seeing you. Do bring the key.\"")));
            return lines;
        }
        string teller = Narrator(state);
        lines.Add(new(teller, Loc.T("Vane was at the Crossed Keys last night, asking who your friends are.")));
        lines.Add(new(teller, Loc.T("When you go to the treasury, who'll stand at the door with you, and take in whoever comes out?")));
        var allies = Allies(state, tonight: false);
        lines.AddRange(allies.Take(4));
        lines.Add(allies.Count == 0
            ? new(teller, Loc.T("Nobody, it seems. Make some friends before the last night, or hire the bargemen at the Lantern Market."))
            : new(teller, allies.Count >= Campaign.AlliesNeeded ? Loc.T("That's a crowd, that is. Whatever comes out of that treasury, it'll have somewhere to go.")
                : Loc.T("It's a start. One more, before the last night, and they'll have somewhere to go.")));
        return lines;
    }

    // ---- The summons, the Guild's nights, the Maw's tactics ----

    /// <summary>
    /// The first Guild night, the due settled: the treasury's summons, and Bram goes with Vane. Old Haddon goes too, if
    /// his note wasn't paid. Once you remember, it's short.
    /// </summary>
    public static List<Line> Summons(GameState state)
    {
        var lines = new List<Line>();
        if (state.Remembers)
        {
            lines.Add(new(Vane, Loc.T("One more thing. The Guild's treasury has asked for Bram. An old consignment needs his attention.")));
            lines.Add(new(Bram, Loc.T("Mind the stall. I'll write every week.")));
            lines.Add(new(Self, Loc.T("He won't write. I know where that carriage goes, and what waits at the end of the road.")));
        }
        else if (state.Veteran)
        {
            lines.Add(new(Vane, Loc.T("One more thing. The Guild's treasury has asked for Bram: an old consignment. The carriage is outside.")));
            lines.Add(new(Bram, Loc.T($"Mind the stall, {state.Kin.Name}. I'll write every week.")));
        }
        else
        {
            lines.Add(new(Vane, Loc.T("One more thing. The Guild's treasury has asked for Bram: an old consignment needs a keywright who knows it. Temporary work. The carriage is outside.")));
            lines.Add(new(Bram, Loc.T("The treasury? After twenty years they want me? Well. It's honest work, and it'll pay down the account.")));
            lines.Add(new(Bram, Loc.T($"Mind the stall, {state.Kin.Name}. I'll write every week. And keep my lamp lit: it's never let me down.")));
            lines.Add(new(Bram, Loc.T("Two taps and one, and I'll be back before you've missed me.")));
        }
        if (state.IsTaken(Campaign.Haddon))
            lines.Add(new(Vane, Loc.T("And Old Haddon comes with us. His note fell due tonight: he'll work it off at the treasury, with Bram.")));
        return lines;
    }

    /// <summary>
    /// What Vane says as he goes, once the night's due is settled: the Guild's kind news of Bram before you remember; after,
    /// the voice of something that remembers too. Null when there's nothing to say.
    /// </summary>
    public static Line Parting(GameState state)
    {
        // Someone a tale made yours, whose note to the Guild fell due tonight, unpaid (Haddon's is the summons').
        if (state.Taken.Where(t => t.Value == state.Week && t.Key != Campaign.Haddon).Select(t => t.Key).OrderBy(k => k, System.StringComparer.Ordinal).FirstOrDefault() is string owed)
            return new(Vane, Loc.T($"One more thing. The Guild has a note to collect tonight: {Sellers.Person(owed).Name}. They'll work it off."));
        if (state.CampaignDone || state.DebtCleared || state.Week == 1 || !state.BramAway) return null;
        if (!state.Remembers)
            return state.Week switch
            {
                2 => new(Vane, Loc.T("Bram sends his regards. The handover's slow work, but he's in good spirits.")),
                3 => new(Vane, Loc.T("The treasury opens on the season's last night. Bram is counting on you, and on the key.")),
                _ => null,
            };
        return state.Week switch
        {
            2 => new(Vane, Loc.T("Bram is well, Keywright. Why go looking? Some doors are better left shut.")),
            3 => state.Knows("carts")
                ? new(Vane, Loc.T("You were at the toll, I hear. The palace road is no place for a shopkeeper."))
                : new(Vane, Loc.T("You remember more than you should, Keywright. So do I.")),
            _ => null,
        };
    }

    /// <summary>The second Guild night, once you remember: half the due, if your questions stop at the toll.</summary>
    public static Line Bribe => new(Vane, Loc.T("Before you pay. The Guild could forget half of tonight's due, Keywright. All it asks is that your questions stop at the toll."));
    public static string BribeTaken => Loc.T("Sensible. You'll find Sergeant Hollis has nothing to say to you this season.");
    public static string BribeRefused => Loc.T("Then ask your questions. You'll find the answers heavy.");

    /// <summary>The third Guild night, once you remember: Bram home tomorrow, for the key's pieces.</summary>
    public static Line Bargain(GameState state) =>
        new(Vane, Loc.Plural(state.Pieces.Count,
            $"Before you pay. You want Bram home. The Guild can have him at your door tomorrow, warm and whole. All it asks is your {state.Pieces.Count} piece of the key.",
            $"Before you pay. You want Bram home. The Guild can have him at your door tomorrow, warm and whole. All it asks is your {state.Pieces.Count} pieces of the key."));
    public static string BargainTaken => Loc.T("Done. He'll be home for breakfast. And you won't be at the treasury this season.");
    public static string BargainRefused => Loc.T("Then he stays where he is. Gold keeps.");

    // ---- The debt paid, the treasury ----

    /// <summary>The debt is paid: the last due, or all that's left of it at once, ahead of time; and what comes of it.</summary>
    public static List<Line> Cleared(GameState state)
    {
        int early = Guild.Weeks - state.Week;
        string when = early > 0
            ? Loc.Plural(early, $"{Capital(Sellers.Number(early))} week early", $"{Capital(Sellers.Number(early))} weeks early")
            : Loc.T("Before I've even come for it");
        var lines = new List<Line>
        {
            early > 0 || !state.DuesTonight
                ? new(Vane, Loc.T($"All of it? Now? {when}. Then Bram's account with the Guild is closed."))
                : new(Vane, Guild.Paid(Guild.Weeks)),
        };
        if (state.CampaignDone)
        {
            lines.Add(new(Bram, Loc.T("Free and clear, another year. Put the kettle on.")));
            return lines;
        }
        if (state.BramHere)
            lines.Add(new(Bram, Loc.T("Free and clear! Twenty years, and the account's closed. Look at my hand: it's warm.")));
        if (state.Week > Guild.Weeks || state.TreasuryDone) return lines;
        if (state.KeyWhole)
            lines.Add(state.Remembers
                ? new(Vane, Loc.T("The treasury, then. Tonight. I'll send the carriage."))
                : new(Vane, Loc.T("And the treasury key is whole, I see. Then the Guild's carriage will take you to the treasury tonight. Bram is waiting.")));
        else if (state.Week < Guild.Weeks)
            lines.Add(new(Vane, Loc.Plural(Campaign.Pieces.Length - state.Pieces.Count,
                $"The treasury opens on the season's last night. Bring the whole key: you're {Campaign.Pieces.Length - state.Pieces.Count} piece short.",
                $"The treasury opens on the season's last night. Bring the whole key: you're {Campaign.Pieces.Length - state.Pieces.Count} pieces short.")));
        else
            lines.Add(new(Vane, Loc.T("No key? Then the Guild will open the treasury without you tonight. Bram will be so disappointed.")));
        return lines;
    }

    /// <summary>Bram, freed and his pledge redeemed, comes home: his account of the pledge, and a quiet evening.</summary>
    public static List<Line> Homecoming(GameState state)
    {
        var lines = new List<Line>
        {
            new(Bram, Loc.T("Cold. I'm so cold. And this is our stall. Two taps and one: I heard you knock back.")),
            state.Is(Characters.Kit)
                ? new(Bram, Loc.T("Twenty years ago the Mark-Finders came for you: a marked baby, at the crossroads. I pledged them my hand, mark and all. That was my account."), "pledge")
                : new(Bram, Loc.T("Twenty years ago the Mark-Finders came for a marked baby, my niece Kit. I pledged them my hand, mark and all. That was my account."), "pledge"),
            new(Bram, Loc.T("This spring they called it in. The treasury was never a job. It was a scale, and a crate, and the dark.")),
            new(Bram, Loc.T("And now look: you paid it. Put the kettle on. Tonight we're just two people in a shop.")),
        };
        if (state.SavedBramBefore)
            lines.Add(new(Self, Loc.T("I've brought him home before. He doesn't remember it. I don't want to do it again: I want it to last.")));
        return lines;
    }

    /// <summary>The Guild's carriage, before the treasury: what's said on the way in.</summary>
    public static List<Line> TreasuryInvite(GameState state)
    {
        var lines = new List<Line>();
        if (state.PlanWhole)
        {
            lines.Add(new(Self, Loc.T("The key, whole. The register. A full mimic, asleep in a basket. And friends at the door.")));
            lines.AddRange(Allies(state, tonight: true).Take(3));
            if (state.BramHere) lines.Add(new(Bram, Loc.T("Whatever's in there, we take them home. All of them.")));
            lines.Add(new(Vane, Loc.T("Keywright. You've brought company. No matter: the chest is waiting.")));
            return lines;
        }
        if (state.Remembers)
        {
            lines.Add(new(Vane, Loc.T("The carriage, Keywright. You know the way.")));
            return lines;
        }
        lines.Add(new(Vane, Loc.T("The carriage, Keywright. The treasury is under the palace, and Bram's commission is waiting.")));
        lines.Add(new(Vane, Loc.T("Here it is: the old consignment. The treasury's great chest. The Guild has never been able to open it.")));
        lines.Add(new(Vane, Loc.T("You have the key. Open it, and Bram's account is closed.")));
        return lines;
    }

    /// <summary>The Maw's last offer, at the final opening.</summary>
    public static string FinalOffer(GameState state) => state.SavedBramBefore || state.BramHome
        ? Loc.T("You have already saved him once. I can make this time remain. Take him home. Leave the door to me.")
        : Loc.T("I can make this time remain. Take them home. Leave the door to me.");

    /// <summary>What it says if you hesitate. Its argument, not the game's.</summary>
    public static string Hesitation => Loc.T("He knows whom he came to save. But you still want to see what is inside.");

    /// <summary>The door left to the Maw: the next day, going sour, and then it reaches for everything.</summary>
    public static List<Line> DoorLeft(GameState state) => new()
    {
        new(Narrator(state), Loc.T("The next morning came. Bram brought the tea, and the market opened, and every chest on every stall wore the Guild's seal.")),
        new(Narrator(state), Loc.T("By noon there was nothing anyone could buy that the treasury hadn't sold them. By evening it wanted the road, and the river, and the light.")),
        new(Self, Loc.T("It can't keep a bargain. It can't stop wanting. It reached past the door for everything, and the seal woke.")),
    };

    /// <summary>The next day: a date the campaign has never reached.</summary>
    public static List<Line> NextDay(GameState state) => new()
    {
        new(Bram, Loc.T("Morning. Tea's hot. Here: feel my hand. Warm as the pot.")),
        // The season's last night was as far as any spring ever got; earlier, it's still a morning nothing turned back from.
        state.Day >= Guild.Weeks * Guild.DaysPerWeek
            ? new(Self, Loc.T("Day twenty-one. I've never seen the calendar say that."))
            : new(Self, Loc.T("The morning after the treasury, and nothing's turned back.")),
        new(Bram, Loc.T("Old Haddon's in his yard, swearing at a lathe. Half the crossroads walked home last night, and nobody can say from where.")),
        new(TaleBook.Wynn, Loc.T("Keywright! The waterwheel's broken, and my father's old millwright's chest has the only tools that fit it. Open it for us?")),
    };

    /// <summary>The waterwheel's tools, out of an ordinary chest: the shop goes on.</summary>
    public static List<Line> ShopGoesOn(GameState state) => new()
    {
        new(TaleBook.Wynn, Loc.T("That's them! The wheel will turn by noon. Bless you, Keywright.")),
        new(Bram, Loc.T("Treasure's still treasure. But that's the best thing we've opened all year, and I'll fight anyone who says different.")),
        new(Self, Loc.T("Two taps on the counter, and one. The next customer. The shop goes on.")),
    };

    public static List<Line> Foreclosed(GameState state)
    {
        var lines = new List<Line> { new(Vane, Guild.Foreclosed) };
        if (state.BramHere)
            lines.Add(new(Bram, state.DebtCleared ? Loc.T($"{state.Week} weeks, and every coin of the tithe paid. Let them have the building: they can't have the mark.")
                : state.Week <= 2 ? Loc.T("So soon? Well. We'll start again somewhere. A good ear is never broke for long.")
                : Loc.T($"{state.Week} weeks. You did better than I ever did. We'll start again somewhere.")));
        if (state.Remembers && !state.CampaignDone)
            lines.Add(new(Self, Loc.T("The last night will come without me, and the treasury will open with Bram in it. And then it'll be spring.")));
        return lines;
    }

    // ---- Who stands with you ----

    /// <summary>
    /// Who stands with you, by what the season's tales made of them: the powers who count you a friend, and people who
    /// owe you (or called a favour in already). Each says what they'll do; on the treasury night, what they're doing.
    /// </summary>
    public static List<Line> Allies(GameState state, bool tonight)
    {
        var lines = new List<Line>();
        string teller = Narrator(state);
        if (state.Friend(Faction.Wardens))
            lines.Add(new(TaleBook.Hollis, tonight ? Loc.T("The Wardens have the palace road. No Guild carriage gets past my toll tonight.")
                : Loc.T("The Wardens will hold the palace road, when it comes to it. You've sent enough chests home.")));
        if (state.Friend(Faction.Commons))
            lines.Add(new(teller, tonight ? Loc.T("Half of Brindle walked to the palace to see it opened. The Guild's men can't get through the crowd.")
                : Loc.T("Brindle's with you: half the crossroads would walk to the palace if you asked them.")));
        if (state.Friend(Faction.Lockless))
            lines.Add(new(teller, tonight ? Loc.T("A black feather on the counter: the Guild's carriages have lost their wheels in Lantern Lane. All of them.")
                : Loc.T("A black feather on the counter, and a note: \"When the Guild comes for you, the Lockless will be in its way.\"")));
        if (state.Friend(Faction.Crown) || state.OnTrial)
            lines.Add(new(TaleBook.Aldo, tonight ? Loc.T("The palace guard is at the treasury door. My Guild has no business here tonight.")
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
        if (state.Rescued.Contains(Campaign.Haddon) || state.Flagged("haddon.redeemed"))
            lines.Add(new(TaleBook.Nell, tonight ? Loc.T("Father brought his cart. Whoever comes out of there rides home.") : Loc.T("Father says he owes you a cart, and a wheel, and his neck. Call us.")));
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
        TaleBook.Quire => tonight ? Loc.T("I've brought the Roll. Whatever the Guild says tonight, the charter says otherwise.") : Loc.T("The Roll will say what the Crown pledged, when it matters."),
        TaleBook.Ilse => tonight ? Loc.T("Every lamp in the Arcanum is lit tonight. The Guild won't find a shadow to hide in.") : Loc.T("The Arcanum's apprentices will stand with you. Some of the archmages, too."),
        TaleBook.Hesper => tonight ? Loc.T("Two whole families are here, dear: the ones you married off. They'd like a word with the Guild.") : Loc.T("Two families owe you their wedding, dear. They'll come."),
        TaleBook.Garrick => tonight ? Loc.T("Brought the plough horses. The Guild's carriages can go round.") : Loc.T("Call, and the whole fair comes, horses and all."),
        TaleBook.Jory => tonight ? Loc.T("I carried chests for you. Tonight I'll carry anything that tries to get past this door.") : Loc.T("I owe you my freedom. Call, and I'll come."),
        TaleBook.Corvin => tonight ? Loc.T("The Wet Hen's at the river stairs, if anyone needs to leave in a hurry.") : Loc.T("Need anything carried away from the Guild quietly? Ask for the Wet Hen."),
        _ => null,
    };

    // ---- Between seasons, and the season's end ----

    /// <summary>
    /// Between runs, once (0.32): a season that ended without a Heartsong ever heard gets Bram's word on them, so a new
    /// player knows what they're listening for: the rarest things in Aldmark, and what one does on display.
    /// </summary>
    /// <summary>Bram's word on the Heartsongs between seasons, once a profile; Bettany's, if he isn't home to give it (0.34).</summary>
    public static List<Line> SongsUnheard(GameState state)
    {
        string teller = Narrator(state);
        return new()
        {
            new(teller, Loc.T("No Heartsong this season. Don't take it hard: most seasons never hear one.")),
            new(teller, Loc.T("They're the rarest things in Aldmark. Nine of them, things loved more than gold, sung into the Locking and " +
                              "lost among millions of chests. About one season in three turns one up.")),
            new(teller, Loc.T("Only a Keywright hears one. At the gentlest shake it sings a tune no other thing makes, and the seller never " +
                              "knows it's there: they price the chest as if it weren't.")),
            new(teller, Loc.T("Nobody would pay a penny for one. But on display, each does one great thing for the shop that no gold can " +
                              "buy, for as long as it sings: the kind of thing a whole season can be built around.")),
            new(teller, Loc.T("So shake every chest gently first, and listen. The collection's Heartsongs page keeps a place for each of the nine.")),
        };
    }

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
        _ => new(Bram, Loc.T($"There you are, {state.Kin.Name}! Look at your hand: the mark's yours now. And so is the stall.")),
    };

    /// <summary>
    /// The season's end (0.30): what became of everyone whose tale began, a line each, by what you chose (each tale's
    /// <see cref="TaleDef.Ending"/>), after the run's last scene, won or lost. Bram tells it if he's home; otherwise
    /// Bettany, who hears everything first.
    /// </summary>
    public static List<Line> SeasonEnd(GameState state)
    {
        string teller = Narrator(state);
        var lines = new List<Line>
        {
            teller == Bram ? new(Bram, Loc.T("So. Let me tell you what became of everyone, this season."))
                : new(teller, Loc.T("Pull up a stool, love. Here's what became of everyone this season.")),
        };
        lines.AddRange(MainEnding(state, teller));
        foreach (var tale in TaleBook.All.Where(t => state.Drew(t.Key) && t.Ending != null))
            if (tale.Ending(state) is Line line) lines.Add(line.Speaker == Bram ? line with { Speaker = teller } : line);
        lines.Add(state.Character switch
        {
            Characters.Hesk => new(teller, Loc.T("And you, Hesk. A grocer from a drowned town, with the mark on your hand. What becomes of you is up to you.")),
            Characters.Rosa => new(teller, Loc.T("And you, Rosa. A Brightwater of Lantern Lane, with the mark on your hand. What becomes of you is up to you.")),
            Characters.Isolde => new(teller, Loc.T("And you, my lady. The last Marchmont, with the mark on your hand. What becomes of you is up to you.")),
            _ when teller == Bram => new(teller, Loc.T("And you, Kit. My kin, with the mark on your hand. What becomes of you is up to you.")),
            _ => new(teller, Loc.T("And you. Bram's kin, with the mark on your hand. What becomes of you is up to you.")),
        });
        return lines;
    }

    /// <summary>The season's end for the main story: Bram, Old Haddon, the Maw's eyes, a mimic; and, once you remember, what the last night means.</summary>
    static IEnumerable<Line> MainEnding(GameState state, string teller)
    {
        // After the true ending a season is a new year: the main story's told.
        if (state.CampaignDone) yield break;
        // Bram tells it whenever he's home (Narrator), so his own lines are his.
        if (state.TreasuryEnd == Campaign.Morning)
        {
            yield return new(teller, Loc.T("The treasury's shut for good, and everyone who came out of it is home. Nobody can say from where, and nobody asks."));
            // (His warm hand was the next morning's: not said twice.)
            yield return new(teller, state.Flagged(GameState.BramFromTheChest)
                ? Loc.T("And I'm home. You brought me out of that chest with all the rest, and I still can't say it without shaking.")
                : Loc.T("And I'm home. You had me out of my Guild crate before that night, and I still can't say it without shaking."));
        }
        else if (state.BramHome)
            yield return new(teller, state.Flagged("bargained")
                ? Loc.T("I came home on the Guild's word, warm and whole. I don't know what you gave them for it, and I jump at every tap on the counter.")
                : Loc.T("I'm home, and my hand is warm. You brought me out of a Guild crate, and I still can't say it without shaking."));
        else if (state.DebtCleared)
            yield return new(teller, Loc.T("Bram's account is closed, every coin paid, and Bram never came home. The Guild says the treasury keeps long hours."));
        else
            yield return new(teller, Loc.T("The Guild took the stall, and Bram never came home. Nobody's had a letter."));
        if (state.Rescued.Contains(Campaign.Haddon))
            yield return new(teller, Loc.T("Old Haddon walked home from a Guild crate, cold as a well. He's making wheels again, and he won't sign anything."));
        else if (state.Flagged("haddon.redeemed") && !state.IsTaken(Campaign.Haddon))
            yield return new(teller, Loc.T("Old Haddon kept his yard, because somebody paid his note. He's making you a wheel. You don't need one."));
        else if (state.IsTaken(Campaign.Haddon))
            yield return new(teller, Loc.T("Old Haddon never came home from working off his note. Nell keeps the yard, and a lamp in the window."));
        // What the Lantern Market and the back room saw.
        if (state.EyesSold > 0)
            yield return new(teller, Loc.Plural(state.EyesSold, $"And you sold {state.EyesSold} gold thing to the Maw's buyers. Every coin of it an eye for him. I hope it was worth it.",
                $"And you sold {state.EyesSold} gold things to the Maw's buyers. Every coin of it an eye for him. I hope it was worth it."));
        if (state.Mimic != null)
            yield return new(teller, Loc.T("And there's still a mimic in the back room, asleep on the gold. Don't wake it."));
        if (state.Remembers && state.TreasuryEnd != Campaign.Morning)
            yield return new(Self, Loc.T("And on the last night the Guild opened the treasury without me. I know what comes after: two taps, and one, and spring."));
    }
}
