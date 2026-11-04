using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// 0.30's Thornwick tales (docs/stories.md): the reeve's election and what it does to the market, the abbey's bounty on
/// the hill goblins, the Unopened at your door (and who pays them), a dowry two families claim (let the ward decide), and
/// a night at the Wardens' toll.
/// </summary>
public static partial class TaleBook
{
    static IEnumerable<TaleDef> ThornwickTales() => new[]
    {
        // The Reeve's Election: the Guild's timber factor against a weaver, and what the winner does to the market.
        new TaleDef
        {
            Key = "reeve", Title = Loc.T("The Reeve's Election"), Place = TalePlace.Thornwick,
            Blurb = Loc.T("Thornwick elects a reeve: Harl Caskett, the Guild's timber factor, or Martha Dunning the weaver. The winner sets the " +
                          "market's tolls, and Caskett's tolls would come out of your purse."),
            Ending = ReeveEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "reeve.caskett", Who = Caskett, Tier = 2, Gap = 8, If = s => s.Day > s.TierSince,
                    Says = s => Say(Caskett,
                        Loc.T("Thornwick chooses its reeve at the week's end, and I'm standing. The Guild would take it kindly if the market's " +
                              "Keywright spoke for me."),
                        Loc.T("A reeve keeps the roads, and roads want tolls. Nothing a busy shop would notice.")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll speak for you"), () =>
                        {
                            s.Flags.Add("reeve.for_caskett");
                            s.Sway(Faction.Guild, 1);
                            s.Log("reeve", Loc.T("You spoke for Harl Caskett in the reeve's election."));
                            return Loc.T("Splendid. The Guild remembers its friends.");
                        }, W(s, 0, (Faction.Guild, 1))),
                        C(Loc.T("I'll keep out of it"), () =>
                        {
                            s.Log("reeve", Loc.T("You kept out of the reeve's election, so far."));
                            return Loc.T("As you like. It won't make any difference.");
                        }),
                    },
                },
                new()
                {
                    Key = "reeve.dunning", Who = Dunning, After = "reeve.caskett", Gap = 1, Tier = 2,
                    Says = s => Say(Dunning,
                        Loc.T("Martha Dunning, weaver, and Caskett's only rival for reeve. If he wins there's a toll on every cart into Thornwick, " +
                              "and every chest at this market costs a tenth more. And the Wood comes down."),
                        Loc.T($"He's the Guild's money. I've my loom. Pay for my crier, {Money.Say(s.Purses(0.5f))}, and the market will hear my side.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Pay for her crier ({s.Purses(0.5f)}g)"), () =>
                        {
                            s.Flags.Add("reeve.for_dunning");
                            if (s.Flagged("reeve.for_caskett")) s.Sway(Faction.Guild, -2);
                            s.Log("reeve", Loc.T("You paid for Martha Dunning's crier in the reeve's election."));
                            return Loc.T("Then Thornwick will hear it from one end to the other. Thank you.");
                        }, W(s, Strategy.CustomerDays(s, Math.Max(0, Strategy.ThornwickUntil(s) - s.Day))), cost: s.Purses(0.5f), kind: true),
                        C(Loc.T("I'll keep out of it"), () =>
                        {
                            s.Log("reeve", Loc.T("You wouldn't pay for Martha Dunning's crier."));
                            return Loc.T("Then it's the Guild's money against my loom. We know how that goes.");
                        }, W(s, -s.Purses(0.2f))),
                    },
                },
                new()
                {
                    Key = "reeve.result", Who = Hollis, After = "reeve.dunning", Gap = 2, Tier = 2,
                    Says = s => Say(Hollis, s.Flagged("reeve.for_dunning")
                        ? Loc.T("Martha Dunning is reeve of Thornwick, by forty votes. Her first act: the market stays open late, and every road in is " +
                                "free. You'll see more folk at your counter, while you trade here.")
                        : Loc.T("Harl Caskett is reeve of Thornwick. His first act: a toll on every cart into the market. Thornwick's traders will " +
                                "be asking a tenth more, and they'll tell you why.")),
                    Then = s =>
                    {
                        if (s.Flagged("reeve.for_dunning"))
                        {
                            s.Flags.Add(MarketDays);
                            s.Sway(Faction.Wood, 1);
                            s.Post("reeve", Loc.T("Reeve Dunning's market"), Loc.T("Thornwick's market stays open late, and every road in is free: one more customer a day while you trade at Thornwick."));
                            s.Log("reeve", Loc.T("Martha Dunning won. Thornwick's market stays open late: one more customer a day while you trade at Thornwick."));
                        }
                        else
                        {
                            s.Flags.Add(Tolls);
                            s.Post("reeve", Loc.T("Reeve Caskett's tolls"), Loc.T("A toll on every cart into Thornwick: Thornwick's regulars ask a tenth more."));
                            s.Log("reeve", Loc.T("Harl Caskett won. His tolls put a tenth on everything Thornwick's regulars sell you."));
                        }
                    },
                },
            },
        },

        // The Goblin Bounty: the abbey blames the hill goblins, and Rook wants to know where Nettle's clan burrows.
        new TaleDef
        {
            Key = "goblins", Title = Loc.T("The Goblin Bounty"), Place = TalePlace.Thornwick,
            Blurb = Loc.T("The abbey has put a bounty on the hill goblins, blaming them for the crypt thefts. Rook the huntress wants to know " +
                          "where Nettle's clan burrows, and Nettle swears they never stole a thing."),
            Ending = GoblinsEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "goblins.bounty", Who = Rook, Tier = 2, Gap = 9, If = s => s.Day > s.TierSince && !s.Gone.Contains(Nettle),
                    Says = s => Say(Rook,
                        Loc.T("The abbey's put a bounty on the hill goblins for its crypt thefts. I'd sooner catch the Magpie, but a bounty's a bounty, " +
                              "and goblins don't run as fast."),
                        Loc.T("You trade with Nettle. Where does her clan burrow? I'll give you a share.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Under the three stones on Crow Hill"), () =>
                        {
                            s.Flags.Add("goblins.told");
                            s.Log("goblins", Loc.T("You told Rook where Nettle's clan burrows."));
                            return Loc.T("Crow Hill. I'll have your share by the end of the week.");
                        }, W(s, s.Purses(0.5f), (Faction.Abbey, 1), (Faction.Wardens, 1)) - s.Purses(0.4f)),
                        C(Loc.T("Goblins don't tell me where they sleep"), () =>
                        {
                            s.Flags.Add("goblins.silent");
                            s.Log("goblins", Loc.T("You wouldn't tell Rook where Nettle's clan burrows."));
                            return Loc.T("No? Then I'll find it the slow way.");
                        }, W(s, s.Purses(0.4f) + Strategy.FavourWorth(s)), kind: true),
                    },
                },
                new()
                {
                    Key = "goblins.raid", Who = Rook, After = "goblins.bounty", Gap = 2, Tier = 2, If = s => s.Flagged("goblins.told"),
                    Says = s => Say(Rook, Loc.T($"Crow Hill, as you said. The clan ran before we got there, Nettle with them, over the hills and gone. " +
                                                $"The abbey paid anyway. Here's your share: {Money.Say(s.Purses(0.5f))}.")),
                    Then = s =>
                    {
                        s.Goes(Nettle);
                        s.TaleGold(s.Purses(0.5f));
                        s.Sway(Faction.Abbey, 1);
                        s.Sway(Faction.Wardens, 1);
                        s.Log("goblins", Loc.T("Rook raided Nettle's burrow. The clan's gone over the hills, and Nettle won't trade with you again."));
                    },
                },
                new()
                {
                    Key = "goblins.plea", Who = Nettle, After = "goblins.bounty", Gap = 1, Tier = 2, If = s => s.Flagged("goblins.silent"),
                    Says = s => Say(Nettle,
                        Loc.T("Rook is sniffing at our hill. The abbey thinks goblins took its chests. Goblins get the blame for everything!"),
                        Loc.T("You speak to the almoner? Brother Cadoc listens to Keywrights. He doesn't listen to goblins.")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll speak for your clan"), () =>
                        {
                            s.Flags.Add(GoblinFriend);
                            s.Sway(Faction.Abbey, -1);
                            s.Owe(Nettle, "goblins");
                            s.Post("goblins", Loc.T("The abbey's bounty withdrawn"), Loc.T("Brother Cadoc has withdrawn the abbey's bounty on the hill goblins, on a Keywright's word."));
                            s.Log("goblins", Loc.T("You spoke for Nettle's clan to Brother Cadoc, and the abbey withdrew its bounty. Nettle charges you nothing for shaking now, and owes you a favour."));
                            return Loc.T("A friend of the clan! Shake all you like. All strengths. Free! And when you need goblins, you call.");
                        }, W(s, s.Purses(0.3f) + Strategy.FavourWorth(s), (Faction.Abbey, -1)), kind: true),
                        C(Loc.T("I'll stay out of it"), () =>
                        {
                            s.Until[$"away:{Nettle}"] = s.Day + Guild.DaysPerWeek;
                            s.Log("goblins", Loc.T("You stayed out of the abbey's bounty. Nettle's clan has gone to ground for a week."));
                            return Loc.T("Then we go underground. Deep. A week, maybe more. Bye.");
                        }),
                    },
                },
            },
        },

        // The Unopened: the abbey's zealots at your door, and who pays for their bread.
        new TaleDef
        {
            Key = "unopened", Title = Loc.T("The Unopened"), Place = TalePlace.Thornwick,
            Blurb = Loc.T("The Unopened, the abbey's zealots, stand at your door telling your customers that every chest you open feeds the Maw. " +
                          "While they stand there, fewer people come in."),
            Ending = UnopenedEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "unopened.picket", Who = Agathe, Tier = 2, Gap = 10, If = s => s.Day > s.TierSince,
                    Says = s => Say(Agathe,
                        Loc.T("Sister Agathe, of the Unopened. Every chest you open loosens the Locking. Every coin you take out of one feeds the Maw."),
                        Loc.T("We'll stand at your door and tell your customers so, until you stop. We are very patient.")),
                    Then = s =>
                    {
                        s.Until[Picket] = s.Day + 10;
                        s.Log("unopened", Loc.T("The Unopened stand at your door. One customer a day turns away."));
                    },
                },
                new()
                {
                    Key = "unopened.cadoc", Who = Cadoc, After = "unopened.picket", Gap = 2, Tier = 2,
                    Says = s => Say(Cadoc,
                        Loc.T("They're ours, I'm afraid: the abbey's zealots. I can't order them off, but I can ask, if the abbey's had alms from you."),
                        Loc.T("Or send for the Wardens, and they'll move them on. The abbey won't thank you for it.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Alms for the abbey ({s.Purses(0.5f)}g)"), () =>
                        {
                            s.Until.Remove(Picket);
                            s.Flags.Add("unopened.alms");
                            s.Sway(Faction.Abbey, 1);
                            s.Log("unopened", Loc.T("You gave alms to the abbey, and Brother Cadoc asked the Unopened to leave your door."));
                            return Loc.T("Bless you. I'll speak to Sister Agathe tonight.");
                        }, W(s, Strategy.CustomerDays(s, Math.Max(0, s.Until.GetValueOrDefault(Picket) - s.Day)) + Strategy.PaperWorth(s) * 0.8f, (Faction.Abbey, 1)),
                            cost: s.Purses(0.5f), kind: true),
                        C(Loc.T("Send for the Wardens"), () =>
                        {
                            s.Until.Remove(Picket);
                            s.Flags.Add("unopened.wardens");
                            s.Sway(Faction.Wardens, 1);
                            s.Sway(Faction.Abbey, -1);
                            s.Log("unopened", Loc.T("The Wardens moved the Unopened on. The abbey wasn't pleased."));
                            return Loc.T("If you must. I'll pray for you both.");
                        }, W(s, Strategy.CustomerDays(s, Math.Max(0, s.Until.GetValueOrDefault(Picket) - s.Day)) + Strategy.PaperWorth(s) * 0.8f,
                            (Faction.Wardens, 1), (Faction.Abbey, -1))),
                        C(Loc.T("Let them stand"), () =>
                        {
                            s.Log("unopened", Loc.T("You let the Unopened stand at your door."));
                            return Loc.T("Patience is a virtue. Theirs, mostly.");
                        }),
                    },
                },
                new()
                {
                    Key = "unopened.purse", Who = Agathe, Night = true, After = "unopened.cadoc", Gap = 1, Tier = 2,
                    If = s => s.Flagged("unopened.alms") || s.Flagged("unopened.wardens"),
                    Says = s => Say(Agathe,
                        Loc.T("Sister Agathe at the back door, alone, with a purse. \"Every week there was one of these in our alms box. Guild silver, " +
                              "with the Guild's seal. We were paid to stand at hedge-wrights' doors. I didn't know.\""),
                        Loc.T("\"I can't keep it. I can't give it back to them. You take it.\"")),
                    Asks = s => new()
                    {
                        C(Loc.T("Keep it as proof"), () =>
                        {
                            s.Gather("purse");
                            s.Sway(Faction.Guild, -1);
                            s.Flags.Add("unopened.proof");
                            s.Log("unopened", Loc.T("Sister Agathe gave you the Guild's purse from the Unopened's alms box: proof the Guild pays them. A paper for the case against the Guild."));
                            return Loc.T("\"Then let it testify. I've stood at my last door.\"");
                        }, W(s, Strategy.PaperWorth(s), (Faction.Guild, -1))),
                        C(Loc.T("Give it to the poor box"), () =>
                        {
                            s.Sway(Faction.Abbey, 1);
                            s.Log("unopened", Loc.T("You sent the Guild's purse to the abbey's poor box."));
                            return Loc.T("\"The poor box. Yes. At least it'll feed someone.\"");
                        }, W(s, 0, (Faction.Abbey, 1)), kind: true),
                    },
                },
            },
        },

        // The Dowry: two families claim one chest, and the court says let the ward decide.
        new TaleDef
        {
            Key = "dowry", Title = Loc.T("The Disputed Dowry"), Place = TalePlace.Thornwick, Secrets = new[] { Ashby, Corlew },
            Blurb = Loc.T("Two families, the Ashbys and the Corlews, claim the same dowry chest, and Aunt Hesper's court says: let the ward " +
                          "decide. A chest is warm only in the hands of the family it belongs to."),
            Ending = DowryEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "dowry.court", Who = Hesper, Tier = 2, Gap = 7, If = s => s.Day > s.TierSince,
                    Says = s => Say(Hesper,
                        Loc.T("A wedding that never happened, dear, and two families who both say the dowry chest is theirs: the Ashbys and the Corlews. " +
                              "The court says let the ward decide."),
                        Loc.T("Each of them will bring it to you and offer to sell it. Your mark will know which of them it belongs to. Tell me after.")),
                    Then = s =>
                    {
                        s.Post("dowry", Loc.T("Let the ward decide"), Loc.T("The Ashbys and the Corlews both claim a dowry chest. Each will offer it to you: lay a hand on it, and your mark knows whose it is."));
                        s.Log("dowry", Loc.T("Two families claim one dowry chest. Each will offer it to you, and your mark will know whose it is."));
                    },
                },
                new()
                {
                    Key = "dowry.ashby", Who = Ashby, After = "dowry.court", Gap = 1, Tier = 2,
                    Brings = (s, rng) => DowryChest(s, rng, Ashby, Loc.T("The Ashby dowry. My daughter's, by every right. Let the ward decide, they say: " +
                                                                         "well, here it is. Buy it, if your mark agrees. {price}.")),
                    Bought = (s, v) => DowryHandled(s, v, Ashby),
                    Passed = (s, v) => DowryHandled(s, v, Ashby),
                },
                new()
                {
                    Key = "dowry.corlew", Who = Corlew, After = "dowry.ashby", Gap = 1, Tier = 2,
                    Brings = (s, rng) => DowryChest(s, rng, Corlew, Loc.T("Hal Corlew. The dowry's ours: my sister packed it herself. The court's let me bring " +
                                                                          "it to you. {price}, and the ward will tell you I'm right.")),
                    Bought = (s, v) => DowryHandled(s, v, Corlew),
                    Passed = (s, v) => DowryHandled(s, v, Corlew),
                },
                new()
                {
                    Key = "dowry.verdict", Who = Hesper, After = "dowry.corlew", Gap = 1, Tier = 2,
                    Says = s => Say(Hesper, Loc.T("Well, dear? The court's waiting, and so are two very cross families. Whose is the dowry?")),
                    Asks = s => new()
                    {
                        Rule(s, Ashby, Loc.T("The Ashbys'")),
                        Rule(s, Corlew, Loc.T("The Corlews'")),
                        C(Loc.T("I couldn't say"), () =>
                        {
                            s.Log("dowry", Loc.T("You couldn't tell the court whose the dowry was."));
                            return Loc.T("Then they'll go on suing each other until the chest rots. Thank you so much.");
                        }),
                    },
                },
            },
        },

        // The Toll: a night at the Wardens' toll on the forest road, with a hand on every cart.
        new TaleDef
        {
            Key = "toll", Title = Loc.T("The Night Toll"), Place = TalePlace.Thornwick,
            Blurb = Loc.T("Half the Wardens are down with the sweats, and Sergeant Hollis needs a Keywright's hand on the carts at the forest " +
                          "toll tonight: a stolen chest goes cold under the mark."),
            Ending = TollEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "toll.ask", Who = Hollis, Tier = 2, Gap = 11, If = s => s.Day > s.TierSince,
                    Says = s => Say(Hollis,
                        Loc.T("Half my Wardens are down with the sweats, and the smugglers know it. Lend me your mark at the toll tonight: a hand on " +
                              "every cart, and tell me which chests are cold."),
                        Loc.T("You'd miss your peddler. I'd owe you.")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll come tonight"), () =>
                        {
                            s.Flags.Add("toll.helping");
                            s.GoOutTonight();
                            s.Sway(Faction.Wardens, 1);
                            s.Log("toll", Loc.T("You went to the Wardens' toll for the night, with your mark."));
                            return Loc.T("Good. Bring a coat. The forest road's cold after dark.");
                        }, W(s, s.Purses(0.3f), (Faction.Wardens, 1))),
                        C(Loc.T("I've a shop to run"), () =>
                        {
                            s.Sway(Faction.Wardens, -1);
                            s.Log("toll", Loc.T("You wouldn't go to the Wardens' toll."));
                            return Loc.T("Course you have. So have the smugglers.");
                        }, W(s, 0, (Faction.Wardens, -1))),
                    },
                },
                new()
                {
                    Key = "toll.night", Who = Hollis, Night = true, After = "toll.ask", Gap = 0, Tier = 2, If = s => s.Flagged("toll.helping"),
                    Says = s => Say(Hollis, s.Gone.Contains(Corvin)
                        ? Loc.T("Midnight at the toll. A carter you don't know, under a tarpaulin. Your hand on his chests: three of them are as cold as " +
                                "the grave. Hollis is watching your face.")
                        : Loc.T("Midnight at the toll, and Corvin Tallow's cart under a tarpaulin. Your hand on his chests: three of them are as cold as " +
                                "the grave. Corvin's watching your face, and so is Hollis.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Tell Hollis they're cold"), () =>
                        {
                            bool corvin = !s.Gone.Contains(Corvin);
                            if (corvin) s.Goes(Corvin);
                            s.TaleGold(s.Purses(0.5f));
                            s.Sway(Faction.Wardens, 2);
                            s.Sway(Faction.Lockless, -2);
                            s.Flags.Add("toll.told");
                            s.Log("toll", corvin ? Loc.T("You told Hollis which of Corvin's chests were stolen. Corvin's in the cells; the Wardens paid you a finder's share.")
                                : Loc.T("You told Hollis which of the carter's chests were stolen. The Wardens paid you a finder's share."));
                            return Loc.T("Three chests going home. That's a Keywright's work, that is. Here's your share.");
                        }, W(s, s.Purses(0.5f), (Faction.Wardens, 2), (Faction.Lockless, -2))),
                        C(Loc.T("Say they're all warm"), () =>
                        {
                            bool corvin = !s.Gone.Contains(Corvin);
                            if (corvin) s.Owe(Corvin, "toll");
                            s.Sway(Faction.Lockless, 1);
                            s.Flags.Add("toll.quiet");
                            s.Log("toll", corvin ? Loc.T("You told Hollis Corvin's chests were warm. They weren't. Corvin owes you a favour.")
                                : Loc.T("You told Hollis the carter's chests were warm. They weren't. The Lockless heard about it."));
                            return Loc.T("All warm? Huh. Then on you go, carter.");
                        }, W(s, Strategy.FavourWorth(s), (Faction.Lockless, 1))),
                    },
                },
            },
        },
    };

    /// <summary>The dowry chest, as one family brings it: it's warm only if it's theirs (the season's secret).</summary>
    static Visit DowryChest(GameState s, Random rng, string family, string pitch)
    {
        var chest = Fill(rng, "dowry", ShopTiers.Typical(s.Tier), s.Tier, 3, Category.Coins, Category.Garb, Category.Potions);
        chest.Tale = "dowry";
        if (s.Secret("dowry") != family) chest.Owner = Family(family == Ashby ? Corlew : Ashby);
        return Brought(s, rng, family, chest, pitch, "dowry");
    }

    /// <summary>Who a dowry chest belongs to, as the back room names its owner.</summary>
    static string Family(string family) => family == Ashby ? Loc.T("the Ashby family") : Loc.T("the Corlew family");

    /// <summary>Whatever you did with a family's offer, what your hand told you is what you know (the careful player always lays one on).</summary>
    static void DowryHandled(GameState s, Visit v, string family)
    {
        if (v.Handled || v.Outcome == VisitOutcome.Bought) s.Flags.Add(v.Chest.Stolen ? $"dowry.cold:{family}" : $"dowry.warm:{family}"); // loc: keys
        s.Log("dowry", Loc.T($"{Name(family)} offered you the dowry chest."));
    }

    /// <summary>Naming a family for the court: right, the ward agrees and the matchmaker owes you; wrong, a family's wronged.</summary>
    static Choice Rule(GameState s, string family, string label)
    {
        string other = family == Ashby ? Corlew : Ashby;
        bool known = s.Flagged($"dowry.warm:{family}") || s.Flagged($"dowry.cold:{other}"); // loc: keys
        bool wrongKnown = s.Flagged($"dowry.cold:{family}") || s.Flagged($"dowry.warm:{other}"); // loc: keys
        return C(label, () =>
        {
            if (s.Secret("dowry") == family)
            {
                s.Sway(Faction.Commons, 1);
                s.Owe(Hesper, "dowry");
                s.TaleGold(s.Purses(0.3f));
                s.Flags.Add("dowry.right");
                s.Log("dowry", family == Ashby ? Loc.T("You told the court the dowry was the Ashbys', and the ward agreed. Aunt Hesper owes you a favour.")
                    : Loc.T("You told the court the dowry was the Corlews', and the ward agreed. Aunt Hesper owes you a favour."));
                return Loc.T("The ward agrees with you, and so does the court. Here's the court's fee, dear, and I owe you one.");
            }
            s.Sway(Faction.Commons, -1);
            s.Flags.Add("dowry.wrong");
            s.Log("dowry", family == Ashby ? Loc.T("You told the court the dowry was the Ashbys'. It wasn't, and the Corlews won't forget it.")
                : Loc.T("You told the court the dowry was the Corlews'. It wasn't, and the Ashbys won't forget it."));
            return Loc.T("Hm. The other family swears blind it opened for their girl last night. Well. The court has your word.");
        }, known ? W(s, s.Purses(0.3f) + Strategy.FavourWorth(s), (Faction.Commons, 1)) : wrongKnown ? W(s, 0, (Faction.Commons, -1)) : -s.Purses(0.1f));
    }

    static Line ReeveEnding(GameState s)
    {
        if (s.Flagged(MarketDays))
            return new(Dunning, Loc.T("Reeve of Thornwick, with a loom in the toll-house. The market's open till the lamps go out, and the roads are free."));
        if (s.Flagged(Tolls))
            return new(Caskett, Loc.T("Reeve Caskett, if you please. The tolls are paying for the new road to the Old Wood. The Guild's very pleased."));
        return null;
    }

    static Line GoblinsEnding(GameState s)
    {
        if (s.Flagged(GoblinFriend))
            return new(Nettle, Loc.T("Our burrow has a door now! A real door, with a lock you can open. The clan says you're an honorary goblin. It's a big honour."));
        if (s.Flagged("goblins.told"))
            return new(Bram, Loc.T("Nettle's clan went over the hills after Rook's raid. The crypt thefts didn't stop. They were never the goblins."));
        return s.Did("goblins.plea") ? new(Nettle, Loc.T("We came up again after a while. Rook still sniffs about. Goblins get the blame for everything.")) : null;
    }

    static Line UnopenedEnding(GameState s)
    {
        if (s.Flagged("unopened.proof"))
            return new(Agathe, Loc.T("I preach on the abbey steps now, about who pays for whose bread. The Guild's very cross. The Unopened are fewer, and better."));
        if (s.Flagged("unopened.alms") || s.Flagged("unopened.wardens"))
            return new(Cadoc, Loc.T("The Unopened stand at other doors now. Sister Agathe left them. She wouldn't tell me why."));
        return s.Did("unopened.picket") ? new(Agathe, Loc.T("We stood at your door all season, and you kept opening chests. We are very patient. So, it seems, are you.")) : null;
    }

    static Line DowryEnding(GameState s)
    {
        string family = s.Secret("dowry");
        if (s.Flagged("dowry.right"))
            return new(Hesper, family == Ashby
                ? Loc.T("The Ashby girl's married now, dowry and all, and the Corlews have stopped suing. I'm never doing another one of those.")
                : Loc.T("The Corlew girl's married now, dowry and all, and the Ashbys have stopped suing. I'm never doing another one of those."));
        if (s.Flagged("dowry.wrong"))
            return new(Hesper, Loc.T("The court gave the dowry to the wrong family on your word, and it won't open for them. They're suing you now, dear."));
        return s.Did("dowry.court") ? new(Hesper, Loc.T("The Ashbys and the Corlews are still at law over that dowry. The chest will outlive them all.")) : null;
    }

    static Line TollEnding(GameState s)
    {
        if (s.Flagged("toll.told"))
            return new(Hollis, Loc.T("Three stolen chests home from one night at the toll. I've asked the Wardens' captain for a Keywright on the road for good."));
        if (s.Flagged("toll.quiet"))
            return new(Bram, Loc.T("Hollis knows those chests at the toll were cold. He hasn't said so. He doesn't have to."));
        return s.Did("toll.ask") ? new(Hollis, Loc.T("The smugglers had a good season. I hope your shop did too.")) : null;
    }
}
