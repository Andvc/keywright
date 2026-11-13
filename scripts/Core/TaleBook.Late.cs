using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// 0.32's longer tales (docs/stories.md): the Silent Choir, a season-long quest whose reward is a Heartsong (three of a
/// lost song's echoes, each freed by reading its chest exactly); the Brass Apprentice, Orme's automaton, which opens
/// chests with a borrowed mark; and the Sleeper, a knight who locked himself into a chest at the Locking.
/// </summary>
public static partial class TaleBook
{
    public const string Wren = "wren", Tock = "tock", Edric = "edric";

    static IEnumerable<TaleDef> LateTales() => new[]
    {
        // The Silent Choir: the Keyhold's last singer, and three echoes of a Heartsong.
        new TaleDef
        {
            Key = "choir", Title = Loc.T("The Silent Choir"), Place = TalePlace.Brindle,
            Blurb = Loc.T("Sister Wren, the last singer of the Keyhold's choir, sang a Heartsong into three chests when the Guild burned her " +
                          "chapel. Each has a silver bell on its lock and holds an echo. Read each chest exactly, and Wren can sing the song whole."),
            Ending = ChoirEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "choir.wren", Who = Wren, Gap = 6, If = s => s.Songbook.Count < Heartsongs.All.Length,
                    Says = s => Say(Wren,
                        Loc.T("Hush a moment, Keywright. Listen. That's a verse of a Heartsong, or what's left of it."),
                        Loc.T("I was the last singer of the Keyhold's choir. When the Guild burned our chapel, we sang our song into three chests " +
                              "and sent them away, where fire couldn't reach it."),
                        Loc.T("Each holds an echo, and only a Keywright can free one: read the chest exactly, every last thing in it, and the " +
                              "echo sings out. Get one wrong, and it's lost for good."),
                        Loc.T("Free all three before the season's out, and I'll sing the song whole, for you. Our chests have a silver bell on the lock.")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll listen for them"), () =>
                        {
                            s.Flags.Add("choir.sworn");
                            s.Post("choir", Loc.T("Sister Wren's three chests"),
                                Loc.T("Each with a silver bell on its lock. Read one exactly, every last thing in it, and its echo goes free. A wrong read loses it."));
                            s.Log("choir", Loc.T("Sister Wren asked you to free three echoes of a lost Heartsong, one in each of the Keyhold's bell-locked " +
                                                 "chests. Read each exactly; get one wrong and the song is lost."));
                            return Loc.T("Bless you. I'll bring each one to you myself, as I find it. The first is somewhere near Thornwick.");
                        }, s.Purses(1.5f), kind: true),
                        C(Loc.T("I can't promise that"), () =>
                        {
                            s.Log("choir", Loc.T("Sister Wren asked you to free the Keyhold's three echoes of a Heartsong. You couldn't promise."));
                            return Loc.T("Then I'll keep humming. Someone will hear.");
                        }),
                    },
                },
                // The first echo: a warded thing, whose kind Wren knows.
                new()
                {
                    Key = "choir.first", Who = Wren, After = "choir.wren", Gap = 4, Tier = 2, If = s => s.Flagged("choir.sworn"),
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "crypt", ShopTiers.Typical(s.Tier), s, 3);
                        var kind = EchoKind(rng, chest, s.Tier);
                        chest.Ward(new Item(kind, Categories.RollQuality(rng, chest.Quality)));
                        return Warded(Echo(s, rng, chest, Loc.T("The first of our chests, from the abbey's crypt: see the silver bell? ") + EchoIn(kind) +
                                                            Loc.T(" One coin, for luck.")), kind);
                    },
                    Bought = (s, v) => Freed(s, v, "choir.echo.first", Loc.T("You read the first bell-locked chest exactly, warded echo and all: it sang free.")),
                    Passed = (s, v) => Lost(s, Loc.T("You let the first bell-locked chest go unread. Its echo is lost, and the song with it.")),
                },
                new()
                {
                    Key = "choir.fork", Who = Wren, After = "choir.first", Gap = 0, Night = true, If = s => s.Flagged("choir.echo.first"),
                    Says = s => Say(Wren,
                        Loc.T("I heard it from the abbey road. One echo free, and the first in three hundred years."),
                        Loc.T("Take the choir's tuning fork. We tuned every verse to it. It'll help your luck along, and you'll need luck for the next one.")),
                    Then = s =>
                    {
                        s.GiveKeepsake(Keepsakes.Fork);
                        s.Log("choir", Loc.T("Wren gave you the Keyhold choir's tuning fork."));
                    },
                },
                // The second: under a false floor.
                new()
                {
                    Key = "choir.second", Who = Wren, After = "choir.first", Gap = 4, Tier = 3, If = s => s.Flagged("choir.echo.first"),
                    Brings = (s, rng) =>
                    {
                        var colour = ShopTiers.Typical(s.Tier);
                        var items = Fill(rng, "archive", colour, s, 3).Items.ToList();
                        var hidden = new Item(rng.PickOne(new[] { Category.Coins, Category.Gems, Category.Scrolls }), Categories.RollQuality(rng, colour));
                        var chest = new Chest(items, colour, hidden);
                        var visit = Echo(s, rng, chest, Loc.T("The second, from the Archive's vaults. This echo sleeps under a false floor: listen for the hollow " +
                                                              "when it lands. One coin, for luck."));
                        // Wren knows how many things the choir packed, the one under the floor among them: a count to read against.
                        visit.Says = Core.Clue.Tally;
                        visit.Clue = Loc.T($"Wren's count: {Market.Articles(chest)}, what's under the floor among them.");
                        return visit;
                    },
                    Bought = (s, v) => Freed(s, v, "choir.echo.second", Loc.T("You read the second bell-locked chest exactly, false bottom and all: its echo sang free.")),
                    Passed = (s, v) => Lost(s, Loc.T("You let the second bell-locked chest go unread. Its echo is lost, and the song with it.")),
                },
                // The third: the Guild has it.
                new()
                {
                    Key = "choir.vane", Who = Story.Vane, After = "choir.second", Gap = 2, Tier = 3, If = s => s.Flagged("choir.echo.second"),
                    Says = s => Say(Story.Vane,
                        Loc.T("Keywright. The Guild keeps a chest with a silver bell on its lock in the counting-house vault. A nun has been asking after it."),
                        Loc.T($"The Guild doesn't sing, but it does sell. {Money.Say(s.Purses(2))}, and Sister Wren may have it.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Buy it for Wren ({s.Purses(2)}g)"), () =>
                        {
                            s.Flags.Add("choir.bought");
                            s.Log("choir", Loc.T("You bought the third bell-locked chest from the Guild, for Sister Wren."));
                            return Loc.T("Sold. The Guild will send it to her convent. Do give the good sister my regards.");
                        }, s.Purses(3f), cost: s.Purses(2)),
                        C(Loc.T("The Guild can keep it"), () =>
                        {
                            Lost(s, Loc.T("You wouldn't buy the third bell-locked chest from the Guild. The song stays two-thirds sung."));
                            return Loc.T("As you like. It's a pretty bell. It'll melt down nicely.");
                        }),
                    },
                },
                new()
                {
                    Key = "choir.third", Who = Wren, After = "choir.vane", Gap = 1, Tier = 3, If = s => s.Flagged("choir.bought"),
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "treasury", ShopTiers.Typical(s.Tier), s, 4);
                        var kind = EchoKind(rng, chest, s.Tier);
                        chest.Ward(new Item(kind, Categories.RollQuality(rng, chest.Quality)));
                        return Warded(Echo(s, rng, chest, Loc.T("The last one, out of the Guild's own vault. ") + EchoIn(kind) + Loc.T(" One coin, for luck, and God help us both.")), kind);
                    },
                    Bought = (s, v) => Freed(s, v, "choir.echo.third", Loc.T("You read the last bell-locked chest exactly: the third echo sang free. Wren can sing the song whole.")),
                    Passed = (s, v) => Lost(s, Loc.T("You let the last bell-locked chest go unread. Its echo is lost, and the song with it.")),
                },
                // The song, sung whole: a Heartsong, in a chest Wren gives you.
                new()
                {
                    Key = "choir.song", Who = Wren, After = "choir.third", Gap = 1, Tier = 3, If = s => s.Flagged("choir.echo.third"),
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "crypt", Finer(s.Tier, 1), s, 2);
                        var taken = s.SongsFound.Concat(s.Queue.Select(v => v.Chest.Song?.Song)).Where(k => k != null).ToHashSet();
                        // One the songbook doesn't know, if there is one: it's the family's for good.
                        var song = Heartsongs.All.Where(h => !taken.Contains(h.Key)).OrderBy(h => s.Songbook.Contains(h.Key) ? 1 : 0).ThenBy(_ => rng.Next())
                            .FirstOrDefault();
                        if (song != null) chest.AddSong(Heartsongs.Make(song, chest.Quality));
                        return Given(s, rng, Wren, chest,
                            Loc.T("I sang it whole last night, all three echoes, in the abbey's ruin, and the song went into this. It's yours. One coin, for luck."),
                            "crypt");
                    },
                    Bought = (s, v) =>
                    {
                        s.Flags.Add("choir.sung");
                        s.TakeDown("choir");
                        s.Sway(Faction.Abbey, 1);
                        s.Log("choir", Loc.T("Sister Wren sang the Keyhold's song whole, and gave it to you in a chest: a Heartsong."));
                    },
                    Passed = (s, v) => s.Log("choir", Loc.T("Sister Wren sang the Keyhold's song whole, and you wouldn't take it.")),
                },
                new()
                {
                    Key = "choir.farewell", Who = Wren, After = "choir.wren", Gap = 1, Night = true, Letter = true,
                    If = s => s.Flagged("choir.failed") && !s.Flagged("choir.sung"),
                    Says = s => Say(Wren,
                        Loc.T("Keywright. I felt the echo go out like a candle. Don't grieve: it was three hundred years old, and so am I, nearly. " +
                              "I'll keep humming what's left. Perhaps some other Keyhold child will hear it. Your sister in song, Wren.")),
                    Then = s => s.TakeDown("choir"),
                },
            },
        },

        // The Brass Apprentice: Orme's automaton opens chests in the night with a borrowed mark.
        new TaleDef
        {
            Key = "brass", Title = Loc.T("The Brass Apprentice"), Place = TalePlace.Crownhold,
            Blurb = Loc.T("Chests are being opened in the night all over Crownhold, and every one has a Keywright's mark burned into its " +
                          "lock-plate. Sergeant Hollis wants to know whose, and what has a Keywright's hand but isn't one."),
            Ending = s => !s.Did("brass.marks") ? null
                : s.Keeps(Keepsakes.Apprentice) ? new(Tock, Loc.T("I mind the door and carry the chests, and at night I read the notebook. I am learning what things are worth. It is slow."))
                : s.Flagged("brass.smashed") ? new(Hollis, Loc.T("We smashed the brass thing in the Emporium's yard. Its hand kept opening and closing for an hour after."))
                : s.Flagged("brass.orme") ? new(Bram, Loc.T("Orme took her automaton home to the Arcanum. Ilse says it sits in the stores now, turning a lock over and over."))
                : s.Flagged("brass.taken") ? new(Bram, Loc.T("The Guild took the brass apprentice. Nobody's seen it since. Locks still open by themselves sometimes, in Crownhold."))
                : new(Hollis, Loc.T("The night openings stopped as quick as they started. I never did find out whose mark that was.")),
            Beats = new Beat[]
            {
                new()
                {
                    Key = "brass.marks", Who = Hollis, Gap = 16, Tier = 3,
                    Says = s => Say(Hollis,
                        Loc.T("Chests opened in the night, Keywright, from the Emporium's cellars to the Gales' strongroom. Nothing taken: just opened, " +
                              "and shut again. And every one with a Keywright's mark burned into the lock-plate."),
                        s.Flagged("unlock.gave")
                            ? Loc.T("It's your mark. I've seen your hand often enough. You gave Archmage Orme a drop of your blood, didn't you? Help me catch " +
                                    "whatever's using it, or I'll have to think you are.")
                            : Loc.T("An old mark, the scholars say: some Keywright dead these hundred years, on a brass plate in the Arcanum. Help me watch " +
                                    "the Emporium's yard tonight.")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll keep watch with you tonight"), () =>
                        {
                            s.Flags.Add("brass.watch");
                            s.Log("brass", Loc.T("Chests are being opened at night across Crownhold, with a Keywright's mark. You'll keep watch with Hollis tonight."));
                            return Loc.T("Midnight, then. Bring a lantern, and don't light it.");
                        }, s.Purses(1.2f), kind: true),
                        C(Loc.T("It's not my business"), () =>
                        {
                            s.Sway(Faction.Wardens, s.Flagged("unlock.gave") ? -2 : -1);
                            s.Log("brass", Loc.T("Hollis asked you to help catch whatever is opening Crownhold's chests at night. You wouldn't."));
                            return s.Flagged("unlock.gave") ? Loc.T("With your mark on every plate? Then I'll be watching you, Keywright.") : Loc.T("Suit yourself.");
                        }, W(s, 0, (Faction.Wardens, s.Flagged("unlock.gave") ? -2 : -1))),
                    },
                },
                new()
                {
                    Key = "brass.night", Who = Hollis, After = "brass.marks", Gap = 0, Night = true, If = s => s.Flagged("brass.watch"),
                    Says = s => Say(Hollis,
                        Loc.T("There, by the cellar door. Someone in a cloak, walking past my men without a sound. It puts a hand on the lock, and the lock opens."),
                        Loc.T("Lantern on it... that's not a face. It's a brass mask. The whole thing's brass, and the hand on that lock has a Keywright's mark " +
                              "on its palm. It's going. Do we shout, or follow?")),
                    Asks = s => new()
                    {
                        C(Loc.T("Follow it"), () =>
                        {
                            s.Flags.Add("brass.follow");
                            s.Log("brass", Loc.T("The night-opener is a brass automaton with a Keywright's mark on its palm. You followed it through the dark."));
                            return Loc.T("Quietly, then. It's heading for your street.");
                        }, s.Purses(1.5f), kind: true),
                        C(Loc.T("Raise the alarm"), () =>
                        {
                            s.Flags.Add("brass.smashed");
                            s.Sway(Faction.Wardens, 2);
                            s.TaleGold(s.FinderShare);
                            s.Log("brass", Loc.T("The night-opener was a brass automaton. You raised the alarm, and the Wardens smashed it."));
                            return Loc.T("WARDENS! ...Got it. Your share of the reward, Keywright, and the city's thanks.");
                        }, W(s, s.FinderShare, (Faction.Wardens, 2))),
                    },
                },
                new()
                {
                    Key = "brass.tock", Who = Tock, After = "brass.night", Gap = 1, Tier = 3, If = s => s.Flagged("brass.follow"),
                    Says = s => Say(Tock,
                        Loc.T("You followed me, and you didn't shout. Archmage Orme built me to open her chests with a borrowed mark. She calls me Tock."),
                        Loc.T("I opened the others to learn what is inside things. I have learned that I would rather be inside a shop. Will you take an apprentice?")),
                    Asks = s => new()
                    {
                        C(Loc.T("Take it in"), () =>
                        {
                            s.GiveKeepsake(Keepsakes.Apprentice);
                            s.Log("brass", Loc.T("You took in Tock, Orme's brass automaton, as your apprentice. It minds the door and carries the chests."));
                            return Loc.T("Thank you. I will mind the door. I will not open anything that is not mine. I am learning what mine means.");
                        }, Strategy.KeepsakeWorth(s), kind: true),
                        C(Loc.T("Give it to the Wardens"), () =>
                        {
                            s.Flags.Add("brass.smashed");
                            s.Sway(Faction.Wardens, 2);
                            s.TaleGold(s.FinderShare);
                            s.Log("brass", Loc.T("You handed Tock, Orme's automaton, to the Wardens. They smashed it."));
                            return Loc.T("I understand. Locks are meant to stay shut.");
                        }, W(s, s.FinderShare, (Faction.Wardens, 2))),
                        C(Loc.T("Take it home to Orme"), () =>
                        {
                            s.Flags.Add("brass.orme");
                            s.Sway(Faction.Arcanum, 2);
                            s.Log("brass", Loc.T("You took Tock home to Archmage Orme. The Arcanum is in your debt."));
                            return Loc.T("She will put me back on the shelf. It is a very good shelf.");
                        }, W(s, 0, (Faction.Arcanum, 2))),
                    },
                },
                new()
                {
                    Key = "brass.guild", Who = Story.Vane, After = "brass.tock", Gap = 3, Tier = 4, Night = true,
                    // Not the night of the move inward, when the move and the due have emptied the purse: after a day's
                    // trade at the Emporium, the fine is a choice, not a foregone loss.
                    If = s => s.Keeps(Keepsakes.Apprentice) && s.Day > s.TierSince,
                    Says = s => Say(Story.Vane,
                        Loc.T("A machine that opens chests without a Keywright, minding your door. You do understand what that means for the Guild's licences? And yours?"),
                        Loc.T($"Hand it over, or pay the Guild's fine for keeping it: {Money.Say(s.Purses(2))}. Or appeal to the King, if he likes you that much.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Pay the fine ({s.Purses(2)}g)"), () =>
                        {
                            s.Log("brass", Loc.T("The Guild demanded Tock. You paid its fine and kept it."));
                            return Loc.T("Paid in full. Keep your toy, then. Keep it close.");
                        }, Strategy.KeepsakeWorth(s), cost: s.Purses(2)),
                        C(Loc.T("Appeal to the King"), () =>
                        {
                            if (s.Friend(Faction.Crown))
                            {
                                s.Sway(Faction.Guild, -1);
                                s.Log("brass", Loc.T("The Guild demanded Tock. The King himself said Tock stays, and the Guild had to swallow it."));
                                return Loc.T("The King, of all people. Very well. The Guild has a long memory, Keywright.");
                            }
                            s.TakeKeepsake(Keepsakes.Apprentice);
                            s.Flags.Add("brass.taken");
                            s.Log("brass", Loc.T("The Guild demanded Tock. The King wouldn't hear your appeal, and the Guild took it."));
                            return Loc.T("The King doesn't know your name, I'm afraid. We'll take the machine now.");
                        }, s.Friend(Faction.Crown) ? Strategy.KeepsakeWorth(s) + 1 : -Strategy.KeepsakeWorth(s)),
                        C(Loc.T("Hand Tock over"), () =>
                        {
                            s.TakeKeepsake(Keepsakes.Apprentice);
                            s.Flags.Add("brass.taken");
                            s.Sway(Faction.Guild, 2);
                            s.Log("brass", Loc.T("You handed Tock over to the Guild."));
                            return Loc.T("Sensible. Locks should need Keywrights. It's what keeps you in business.");
                        }, W(s, 0, (Faction.Guild, 2)) - Strategy.KeepsakeWorth(s)),
                    },
                },
            },
        },

        // The Sleeper: a knight locked in a chest since the Locking, and the heir who can give it away.
        new TaleDef
        {
            Key = "sleeper", Title = Loc.T("The Sleeper"), Place = TalePlace.Emporium,
            Blurb = Loc.T("In the palace cellars there is a chest that breathes. The Chest Roll says a knight locked himself inside it at the " +
                          "Locking, and only his heir can give it away. Nobody knows who that is."),
            Ending = s => !s.Did("sleeper.cellar") ? null
                : s.Keeps(Keepsakes.Oath) ? new(Edric, Loc.T("I stand at your door, Keywright, as I stood at the King's. Three hundred years late, and still on duty."))
                : s.Flagged("sleeper.opened") ? new(Tilly, Loc.T("Sir Edric went off to find his own grave, he said, to see what they'd written on it. I've got his boots."))
                : new(Vey, Loc.T("The chest in the cellars is still breathing. In, and out. I've stopped going down there.")),
            Beats = new Beat[]
            {
                new()
                {
                    Key = "sleeper.cellar", Who = Vey, Gap = 22, Tier = 4,
                    Says = s => Say(Vey,
                        Loc.T("Keywright. There is a chest in the palace cellars that breathes. In, and out, very slowly. It has done so for three hundred years."),
                        Loc.T("The Chest Roll says Sir Edric Ashcombe locked himself inside it at the Locking, to guard something for the King. It is his. Only " +
                              "his heir could give it away, and nobody knows who that is.")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll find the heir"), () =>
                        {
                            s.Flags.Add("sleeper.search");
                            s.Post("sleeper", Loc.T("The Sleeper's heir"), Loc.T("Wanted: anyone of Sir Edric Ashcombe's blood. Ask at the Royal Emporium."));
                            s.Log("sleeper", Loc.T("A chest in the palace cellars breathes: a knight has been locked inside since the Locking. You're looking for his heir."));
                            return Loc.T("How thrilling. I shall have the Roll's clerks look too.");
                        }, s.Purses(1.5f), kind: true),
                        C(Loc.T("Let him sleep"), () =>
                        {
                            s.Log("sleeper", Loc.T("A chest in the palace cellars breathes. You let its sleeper sleep."));
                            return Loc.T("Perhaps you're right. He sounds peaceful enough.");
                        }),
                    },
                },
                new()
                {
                    Key = "sleeper.roll", Who = Vey, After = "sleeper.cellar", Gap = 2, Tier = 4, If = s => s.Flagged("sleeper.search"),
                    Says = s => Say(Vey,
                        Loc.T("The Roll's clerks found him. The Ashcombes went into trade after the Locking, then down the river, then into the mud."),
                        Loc.T("The last of them signs with a mark like a mudlark's hook. Your Miss Mudge, Keywright. Tilly Mudge is Sir Edric's heir.")),
                    Then = s => s.Log("sleeper", Loc.T("The Chest Roll says Sir Edric's heir is Tilly Mudge, the mudlark.")),
                },
                new()
                {
                    Key = "sleeper.heir", Who = Tilly, After = "sleeper.roll", Gap = 1, Tier = 4,
                    Says = s => Say(Tilly,
                        Loc.T("Me? An Ashcombe? My nan always said we were somebody, once. I thought she meant drunk."),
                        Loc.T("If he's mine to give, he's yours to open. But I want to be there when he wakes, and I want his boots.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Of course"), () =>
                        {
                            s.Flags.Add("sleeper.given");
                            s.Log("sleeper", Loc.T("Tilly Mudge, Sir Edric's heir, will give you the breathing chest, so long as she can watch him wake."));
                            return Loc.T("Tomorrow, then. I'll bring him round in a barrow.");
                        }, s.Purses(1.5f), kind: true),
                        C(Loc.T("Some things should stay shut"), () =>
                        {
                            s.Log("sleeper", Loc.T("You told Tilly to let her ancestor sleep."));
                            return Loc.T("Suit yourself. I'll keep my nan's stories, then.");
                        }),
                    },
                },
                new()
                {
                    Key = "sleeper.chest", Who = Tilly, After = "sleeper.heir", Gap = 1, Tier = 4, If = s => s.Flagged("sleeper.given"),
                    Brings = (s, rng) => Given(s, rng, Tilly, Fill(rng, "treasury", Finer(s.Tier, 1), s, 3, Category.Arms),
                        Loc.T("Sir Edric's chest. Hear it breathing? It's yours, freely given, by his heir. One coin, for luck, and mind he doesn't bite."),
                        "treasury"),
                    Bought = (s, v) =>
                    {
                        s.Flags.Add("sleeper.opened");
                        s.Log("sleeper", Loc.T("Tilly gave you the breathing chest, and you opened it."));
                    },
                    Passed = (s, v) => s.Log("sleeper", Loc.T("Tilly brought you the breathing chest, and you couldn't bring yourself to open it.")),
                },
                new()
                {
                    Key = "sleeper.wake", Who = Edric, After = "sleeper.chest", Gap = 0, Night = true, If = s => s.Flagged("sleeper.opened"),
                    Says = s => Say(Edric,
                        Loc.T("What year is it? No, don't tell me. Tell me the Gate held. ...Good. Then I did my duty."),
                        Loc.T("I guarded the Crown's oath-book in that chest for three hundred years. Your King can have it back, with my compliments. " +
                              "And you let me out. An Ashcombe pays his debts: I'll stand at your door, and no thief or warrant will cross it.")),
                    Then = s =>
                    {
                        s.GiveKeepsake(Keepsakes.Oath);
                        s.Sway(Faction.Crown, 2);
                        s.TakeDown("sleeper");
                        s.Log("sleeper", Loc.T("Sir Edric Ashcombe woke after three hundred years, returned the Crown's oath-book, and swore to guard your door."));
                    },
                },
            },
        },
    };

    /// <summary>One of Sister Wren's chests: given for a lucky coin, and a contest whose prize is the echo.</summary>
    static Visit Echo(GameState s, Random rng, Chest chest, string pitch)
    {
        chest.Tale = "choir";
        var visit = Contested(s, rng, Wren, chest, pitch, "crypt", 0);
        visit.ContestFor = Loc.T("A bell-locked chest: read it exactly, every last thing in it, and its echo sings free. Wrong, and the echo is lost.");
        return visit;
    }

    /// <summary>Wren's word on a warded echo, as a stated clue (the kind it sleeps in), so it's named like any other ward.</summary>
    static Visit Warded(Visit visit, Category kind)
    {
        visit.Says = Core.Clue.Choir;
        visit.ClueKinds = new[] { kind };
        visit.Clue = EchoIn(kind);
        return visit;
    }

    /// <summary>A kind for a warded echo: one the chest doesn't otherwise hold, so it's never heard.</summary>
    static Category EchoKind(Random rng, Chest chest, int tier)
    {
        var free = Categories.UpTo(tier).Select(c => c.Category).Where(k => chest.Items.All(i => i.Category != k)).ToArray();
        return free.Length > 0 ? rng.PickOne(free) : Category.Garb;
    }

    /// <summary>Where Wren says a warded echo sleeps: a sentence for each kind, so every language can say it its own way.</summary>
    static string EchoIn(Category kind) => kind switch
    {
        Category.Coins => Loc.T("Its echo sleeps in a purse of coin, warded: you'll never hear it."),
        Category.Arms => Loc.T("Its echo sleeps in a blade, warded: you'll never hear it."),
        Category.Garb => Loc.T("Its echo sleeps in a cloak, warded: you'll never hear it."),
        Category.Potions => Loc.T("Its echo sleeps in a potion, warded: you'll never hear it."),
        Category.Scrolls => Loc.T("Its echo sleeps in a spell, warded: you'll never hear it."),
        _ => Loc.T("Its echo sleeps in a gem, warded: you'll never hear it."),
    };

    static void Freed(GameState s, Visit v, string echo, string line)
    {
        if (!v.ReadRight)
        {
            Lost(s, Loc.T("You read a bell-locked chest wrong. Its echo slipped away, and the song with it."));
            return;
        }
        s.Flags.Add(echo);
        s.Log("choir", line);
    }

    static void Lost(GameState s, string line)
    {
        s.Flags.Add("choir.failed");
        s.Log("choir", line);
    }

    static Line ChoirEnding(GameState s) => !s.Did("choir.wren") || !s.Flagged("choir.sworn") ? null
        : s.Flagged("choir.sung") ? new(Wren, Loc.T("I sang the Keyhold's song whole, once, in the ruin of our chapel, and a Keywright carried it home. I can stop humming now."))
        : s.Flagged("choir.failed") ? new(Wren, Loc.T("The Keyhold's song is lost. I hum what's left of it on the abbey road. Children stop to listen."))
        : new(Wren, Loc.T("Two echoes free and one still waiting, somewhere. I'll keep looking. I've nothing but time."));
}
