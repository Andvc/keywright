using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// 0.30's Crownhold tales (docs/stories.md): a bell under the Aldwater with a shard of the Key in it, Mother Tansy's
/// loans, a duel whose stakes you hold, and a fire at the Royal Archive.
/// </summary>
public static partial class TaleBook
{
    static IEnumerable<TaleDef> CrownholdTales() => new[]
    {
        // The Drowned Bell: Tilly hears a bell under the river, and the Keyhold hid a shard in it.
        new TaleDef
        {
            Key = "bell", Title = Loc.T("The Drowned Bell"), Place = TalePlace.Crownhold,
            Blurb = Loc.T("Tilly Mudge hears a bell ringing under the Aldwater at night, and nobody believes her."),
            Ending = BellEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "bell.hear", Who = Tilly, Tier = 3, Gap = 12, If = s => s.Day > s.TierSince,
                    Says = s => Say(Tilly,
                        Loc.T("There's a bell under the Aldwater. I hear it nights, off the old stairs below the palace, ringing when there's no wind " +
                              "and no current. Nobody believes me."),
                        Loc.T("A mudlark hears what's in the mud. Same as you hear what's in a chest. You believe me, don't you?")),
                    Asks = s => new()
                    {
                        C(Loc.T("I believe you"), () =>
                        {
                            s.Flags.Add("bell.believed");
                            s.Log("bell", Loc.T("Tilly Mudge hears a bell under the Aldwater. You believe her."));
                            return Loc.T("You do? Then I'll find exactly where. Give me two days.");
                        }, W(s, s.Purses(0.8f)), kind: true),
                        C(Loc.T("The river's full of noises"), () =>
                        {
                            s.Log("bell", Loc.T("Tilly Mudge hears a bell under the Aldwater. You didn't believe her."));
                            return Loc.T("It is. And one of them's a bell.");
                        }),
                    },
                },
                new()
                {
                    Key = "bell.dredge", Who = Tilly, After = "bell.hear", Gap = 2, Tier = 3, If = s => s.Flagged("bell.believed"),
                    Says = s => Say(Tilly,
                        Loc.T("Found it. Under the Keyhold stairs, where the archmages used to come down to the water. It's big, and there's something " +
                              "chained to it."),
                        Loc.T($"A barge and a dredge for a day is {Money.Say(s.Purses(1))}. Finder's right: whatever comes up, half's yours.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Hire the barge ({s.Purses(1)}g)"), () =>
                        {
                            s.Flags.Add("bell.dredged");
                            s.Promise(s.Day + 2, Tilly, Loc.T("Tilly, with the bell"));
                            s.Log("bell", Loc.T("You hired a barge to dredge Tilly's bell out of the Aldwater."));
                            return Loc.T("Two days, and we'll have it up. Bring a towel.");
                        }, W(s, s.Purses(1.6f) + (s.MapPieces < Story.Pieces && !s.MapGiven ? s.Purses(1.5f) : 0)), cost: s.Purses(1), kind: true),
                        C(Loc.T("Too dear"), () =>
                        {
                            s.Log("bell", Loc.T("You wouldn't pay to dredge Tilly's bell."));
                            return Loc.T("Then it'll ring down there a while longer.");
                        }),
                    },
                },
                new()
                {
                    Key = "bell.raised", Who = Tilly, After = "bell.dredge", Gap = 2, Tier = 3, If = s => s.Flagged("bell.dredged"),
                    Brings = (s, rng) =>
                    {
                        s.Kept(Tilly);
                        return Given(s, rng, Tilly, Fill(rng, "river", Finer(s.Tier, 1), s.Tier, 3),
                            Loc.T("Up it came, a bell as big as a cart, and this chained to the clapper. Finder's right: half each, and this half's " +
                                  "yours. One coin, for luck."), "river");
                    },
                    Bought = (s, v) =>
                    {
                        s.Flags.Add("bell.raised");
                        s.Log("bell", Loc.T("Tilly's bell came up out of the Aldwater, with a chest chained to its clapper: your half, freely given."));
                    },
                    Passed = (s, v) =>
                    {
                        s.Flags.Add("bell.raised");
                        s.Log("bell", Loc.T("Tilly's bell came up out of the Aldwater. You let her keep the chest that was chained to it."));
                    },
                },
                new()
                {
                    Key = "bell.shard", Who = Story.Hood, Night = true, After = "bell.raised", Gap = 0, Tier = 3,
                    Says = s => Say(s.Week >= Story.OfferWeek ? Story.Mara : Story.Hood,
                        s.Week >= Story.OfferWeek
                            ? Loc.T("Mara at the back door, soaked to the knees. \"That's the Keyhold's bell. We rang it from the river stairs, and when " +
                                    "the Locking was made we hid something in its clapper.\"")
                            : Loc.T("The hooded woman at the back door, soaked to the knees. \"That's the Keyhold's bell. We rang it from the river stairs, " +
                                    "and when the Locking was made we hid something in its clapper.\""),
                        s.MapGiven || s.MapPieces >= Story.Pieces
                            ? Loc.T("\"It's empty. Someone was there before us, long ago. Take this for the trouble: Keyhold silver, from the bell's crown.\"")
                            : Loc.T("\"A shard of the Aldric Key. I've been looking for it for twenty years, and a mudlark heard it ring. It's yours, Keywright.\"")),
                    Then = s =>
                    {
                        if (s.GiveShard())
                        {
                            s.Flags.Add("bell.shard");
                            s.Log("bell", Loc.T("A shard of the Aldric Key was hidden in the Keyhold's bell. The Hooded Woman gave it to you."));
                        }
                        else
                        {
                            s.TaleGold(s.Purses(1));
                            s.Log("bell", Loc.T("The Keyhold's bell was empty. The Hooded Woman paid you in Keyhold silver for the trouble."));
                        }
                    },
                },
            },
        },

        // The Moneylender: Mother Tansy of Lantern Lane lends against next week's first morning.
        new TaleDef
        {
            Key = "tansy", Title = Loc.T("The Moneylender"), Place = TalePlace.Crownhold,
            Blurb = Loc.T("Mother Tansy of Lantern Lane lends to anyone short for the Guild, against next week's first morning."),
            Ending = TansyEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "tansy.offer", Who = Tansy, Tier = 3, Gap = 11, If = s => s.Day > s.TierSince,
                    Says = s => Say(Tansy,
                        Loc.T("Mother Tansy, of Lantern Lane. Short for the Guild? Everyone in Crownhold is, dear. That's my living."),
                        Loc.T($"{Money.Say(s.Purses(1))} now, and {Money.Say(Money.Round(s.Purses(1) * (1 + LoanShare)))} on the first morning of next week. " +
                              $"If it's not there, I take something nice off your shelf. Fair's fair.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Borrow it (+{s.Purses(1)}g)"), () =>
                        {
                            s.Borrow(s.Purses(1), LoanShare, GameState.DayNumber(s.Week + 1, 1));
                            s.Log("tansy", Loc.T("You borrowed from Mother Tansy, to pay back with a third on top on the first morning of next week."));
                            return Loc.T("Lovely. My man will call. He's very punctual.");
                        }, LoanWorth(s, 1, LoanShare)),
                        C(Loc.T("No, thank you"), () =>
                        {
                            s.Log("tansy", Loc.T("You wouldn't borrow from Mother Tansy."));
                            return Loc.T("You know where I am, dear. Everyone does, sooner or later.");
                        }),
                    },
                },
                new()
                {
                    Key = "tansy.again", Who = Tansy, After = "tansy.offer", Gap = 6, Tier = 3, If = s => s.Flagged(LoanPaid) && s.LoanOwed == 0,
                    Says = s => Say(Tansy,
                        Loc.T("You paid on the nail, dear. I like that in a Keywright. So: better terms."),
                        Loc.T($"{Money.Say(s.Purses(2))} now, and only a quarter on top, on the first morning of next week. For a grander shop, perhaps?")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Borrow it (+{s.Purses(2)}g)"), () =>
                        {
                            s.Borrow(s.Purses(2), BetterShare, GameState.DayNumber(s.Week + 1, 1));
                            s.Log("tansy", Loc.T("You borrowed from Mother Tansy again, on better terms: a quarter on top."));
                            return Loc.T("Lovely. Same as before, then.");
                        }, LoanWorth(s, 2, BetterShare)),
                        C(Loc.T("I'm square, and I'll stay square"), () =>
                        {
                            s.Log("tansy", Loc.T("You didn't borrow from Mother Tansy again."));
                            return Loc.T("Sensible. Dull, but sensible.");
                        }, kind: true),
                    },
                },
            },
        },

        // The Duel: Sir Rowan and Lord Cassian meet on the Aldwater steps, and you hold the stakes.
        new TaleDef
        {
            Key = "duel", Title = Loc.T("The Duel"), Place = TalePlace.Crownhold, Secrets = new[] { Rowan, Cassian },
            Blurb = Loc.T("Sir Rowan and Lord Cassian will fight on the Aldwater steps, and someone has to hold the stakes."),
            Ending = DuelEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "duel.stakes", Who = Rowan, Tier = 3, Gap = 13, If = s => s.Day > s.TierSince && !s.Gone.Contains(Cassian),
                    Says = s => Say(Rowan,
                        Loc.T("Lord Cassian Vell called the Gales a family of debtors across a card table. We meet on the Aldwater steps at dawn, " +
                              "the day after tomorrow."),
                        Loc.T("We each stake a chest, and the winner takes both. Someone honest must hold them. A Keywright's back room, I thought.")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll hold the stakes"), () =>
                        {
                            s.Flags.Add("duel.holding");
                            s.BackRoom.Add(new Held { Owner = Name(Rowan), Paid = 0, Colour = ShopTiers.Typical(s.Tier), Key = "duel", Day = s.Day });
                            s.Log("duel", Loc.T("You're holding the stakes for Sir Rowan's duel with Lord Cassian, in your back room."));
                            return Loc.T("Thank you. Dawn, the day after tomorrow. Come and watch, if you've the stomach.");
                        }, W(s, s.Purses(0.5f) + Strategy.FavourWorth(s)), kind: true),
                        C(Loc.T("It's not my quarrel"), () =>
                        {
                            s.Log("duel", Loc.T("You wouldn't hold the stakes for Sir Rowan's duel."));
                            return Loc.T("No. I suppose it isn't.");
                        }),
                    },
                },
                new()
                {
                    Key = "duel.bribe", Who = Cassian, Night = true, After = "duel.stakes", Gap = 1, Tier = 3, If = s => s.Flagged("duel.holding"),
                    Says = s => Say(Cassian,
                        Loc.T("Lord Cassian at the back door, smelling of brandy, with a purse. \"Hand me both stakes tonight, and this is yours. Rowan " +
                              "need never know where they went.\"")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Take his purse ({s.Purses(1)}g)"), () =>
                        {
                            s.TaleGold(s.Purses(1));
                            s.BackRoom.RemoveAll(h => h.Key == "duel");
                            s.Flags.Add("duel.sold");
                            s.Goes(Rowan);
                            s.Sway(Faction.Crown, -1);
                            s.Log("duel", Loc.T("You sold the duel's stakes to Lord Cassian. Sir Rowan left Crownhold in disgrace, and won't trade with you again."));
                            return Loc.T("\"Splendid. Not a word to anyone, Keywright. Least of all my sister.\"");
                        }, W(s, s.Purses(1), (Faction.Crown, -1)) - s.Purses(0.8f)),
                        C(Loc.T("The stakes stay where they are"), () =>
                        {
                            s.Flags.Add("duel.honest");
                            s.Log("duel", Loc.T("Lord Cassian tried to buy the duel's stakes off you. You sent him away."));
                            return Loc.T("\"Honest. How very tiresome.\"");
                        }, W(s, s.Purses(0.5f) + Strategy.FavourWorth(s)), kind: true),
                    },
                },
                new()
                {
                    Key = "duel.dawn", Who = Rowan, After = "duel.bribe", Gap = 1, Tier = 3, If = s => s.Flagged("duel.honest"),
                    Says = s => DuelWinner(s) == Rowan
                        ? Say(Rowan, s.Flagged(AldwinHome)
                            ? Loc.T("I fought with great-great-grandfather's blade, and Cassian's sword broke on it at the third pass. The stakes are mine. A " +
                                    "tenth is yours for holding them.")
                            : Loc.T("Cassian slipped on the wet steps at the third pass, and yielded. The stakes are mine. A tenth is yours for holding them."))
                        : Say(Rowan, Loc.T("Cassian fights better drunk than I do sober. He has the stakes, and I've a cut to remember him by. He left a tenth " +
                                           "for you, for holding them.")),
                    Then = s =>
                    {
                        string winner = DuelWinner(s);
                        s.BackRoom.RemoveAll(h => h.Key == "duel");
                        s.TaleGold(s.Purses(0.5f));
                        s.Flags.Add(winner == Rowan ? "duel.rowan" : "duel.cassian");
                        s.Owe(Rowan, "duel");
                        s.Log("duel", winner == Rowan ? Loc.T("Sir Rowan won his duel with Lord Cassian. You were paid for holding the stakes, and the Gales owe you a favour.")
                            : Loc.T("Lord Cassian won his duel with Sir Rowan. You were paid for holding the stakes, and Rowan owes you a favour for your honesty."));
                    },
                },
            },
        },

        // The Archive's Fire: someone sets the Royal Archive alight, and Master Quire is carrying the Chest Roll out himself.
        new TaleDef
        {
            Key = "fire", Title = Loc.T("The Archive Fire"), Place = TalePlace.Crownhold,
            Blurb = Loc.T("Someone has set the Royal Archive alight, and the Chest Roll with it."),
            Ending = FireEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "fire.night", Who = Quire, Night = true, Tier = 3, Gap = 14, If = s => s.Day > s.TierSince,
                    Says = s => Say(Quire,
                        Loc.T("A boy hammering at the back door: \"The Archive's burning! Master Quire's carrying the Roll out himself, and he can't " +
                              "lift the half of it!\""),
                        Loc.T("You can see the glow over the rooftops from here. It'll take all night.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Run to help"), () =>
                        {
                            s.Flags.Add("fire.helped");
                            s.GoOutTonight();
                            s.Sway(Faction.Crown, 1);
                            s.Log("fire", Loc.T("You ran to the Archive fire and carried the Chest Roll out with Master Quire, all night."));
                            return Loc.T("You're out of the door before the boy's finished. Nim will find the back door shut tonight.");
                        }, W(s, Strategy.PaperWorth(s) + Strategy.FavourWorth(s), (Faction.Crown, 1)) - s.Purses(0.2f), kind: true),
                        C(Loc.T("It's not my fire"), () =>
                        {
                            s.Flags.Add("fire.burned");
                            s.Goes(Quire);
                            s.Sway(Faction.Crown, -1);
                            s.Log("fire", Loc.T("Half the Chest Roll burned in the Archive fire. Master Quire is ruined, and won't be trading this season."));
                            return Loc.T("The glow lasts till dawn. In the morning, half of Crownhold smells of burnt parchment.");
                        }, W(s, 0, (Faction.Crown, -1))),
                    },
                },
                new()
                {
                    Key = "fire.ledgers", Who = Quire, After = "fire.night", Gap = 1, Tier = 3, If = s => s.Flagged("fire.helped"),
                    Says = s => Say(Quire,
                        Loc.T("We saved the Roll. Every volume. And the fire started in a lamp-oil cask with the Guild's own stamp on it."),
                        Loc.T("The Guild wanted the Archive's ledgers of its seizures burnt. Here: a fair copy, in your hand and mine. Keep it somewhere " +
                              "they won't think to look.")),
                    Then = s =>
                    {
                        s.Gather("books");
                        s.Owe(Quire, "fire");
                        s.Log("fire", Loc.T("Master Quire gave you a copy of the Archive's ledgers of the Guild's seizures. A paper for the case against the Guild. He owes you a favour."));
                    },
                },
            },
        },
    };

    /// <summary>Mother Tansy's share on top of a first loan, and on better terms.</summary>
    const float LoanShare = 1f / 3, BetterShare = 0.25f;

    /// <summary>
    /// What a loan's worth to the careful player: her share's a cost, unless it's the Guild's night soon and the purse is
    /// short of the due, when it keeps the stall.
    /// </summary>
    static float LoanWorth(GameState s, float purses, float share)
    {
        int purse = s.Purses(purses);
        bool short_ = s.Gold < s.NextDue && s.Gold + purse >= s.NextDue && s.DaysToNextDue <= 1;
        return short_ ? s.DebtLeft * 0.5f : -purse * share;
    }

    /// <summary>Who wins the duel: whoever has Aldwin Gale's blade, or else the season's secret.</summary>
    static string DuelWinner(GameState s) => s.Flagged(AldwinHome) ? Rowan : s.Secret("duel");

    static Line BellEnding(GameState s)
    {
        if (s.Flagged("bell.shard"))
            return new(Tilly, Loc.T("The Keyhold's bell hangs over the river stairs again, and I ring it for the ferry. A shard of the Key was in it. A mudlark heard it first!"));
        if (s.Flagged("bell.raised"))
            return new(Tilly, Loc.T("The bell came up, and I ring it for the ferry now. The posh ones call me the bell-finder. I call me rich."));
        return s.Flagged("bell.believed") ? new(Tilly, Loc.T("It still rings down there, nights. You believed me, though. That's something.")) : null;
    }

    static Line TansyEnding(GameState s)
    {
        if (s.Flagged(LoanDefaulted))
            return new(Tansy, Loc.T("You owed me, dear, and I took what you owed off your shelf. No hard feelings. Business is business."));
        if (s.Flagged(LoanPaid))
            return new(Tansy, Loc.T("Paid on the nail, every time. If you ever want to go into lending, dear, I'd take you as a partner."));
        return s.Did("tansy.offer") ? new(Tansy, Loc.T("You never borrowed a penny from me. I don't know whether to admire you or worry about you.")) : null;
    }

    static Line DuelEnding(GameState s)
    {
        if (s.Flagged("duel.sold"))
            return new(Bram, Loc.T("Sir Rowan left Crownhold after the duel that never was. Cassian tells everyone he won it. You know better."));
        if (s.Flagged("duel.rowan"))
            return new(Rowan, Loc.T("Cassian apologised to the Gales in the Hall of Keys, in front of everyone. I've rarely enjoyed anything so much."));
        if (s.Flagged("duel.cassian"))
            return new(Isaura, Loc.T("My brother won a duel. He's been insufferable ever since. Thank you for holding the stakes honestly, Keywright."));
        return s.Did("duel.stakes") ? new(Bram, Loc.T("Sir Rowan and Lord Cassian fought on the Aldwater steps. Nobody agrees who won.")) : null;
    }

    static Line FireEnding(GameState s)
    {
        if (s.Flagged("fire.helped"))
            return new(Quire, Loc.T("The Roll's safe in a new vault, and the Guild's lamp-oil cask is in my office, as evidence. You carried half of it, Keywright."));
        return s.Flagged("fire.burned") ? new(Bram, Loc.T("Half the Chest Roll burned in the Archive fire. Half of Aldmark can't prove whose chests are whose. The Guild's delighted.")) : null;
    }
}
