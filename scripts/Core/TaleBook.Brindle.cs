using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// 0.30's Brindle tales (docs/stories.md): a rival hedge-wright across the road, a dead knight who wants his blade sent
/// home, the Crossed Keys' tithe, and the harvest fair's reading contest. Built by methods, not fields, so the order the
/// partial files are read in never matters.
/// </summary>
public static partial class TaleBook
{
    static IEnumerable<TaleDef> BrindleTales() => new[]
    {
        // The Hedge-wright: Jasper Wick sets up across the road, unlicensed, as Bram was for twenty years.
        new TaleDef
        {
            Key = "hedge", Title = Loc.T("The Hedge-wright"), Place = TalePlace.Brindle,
            Blurb = Loc.T("Jasper Wick has the mark and no licence, and he's set up his cart across the road from the stall."),
            Ending = HedgeEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "hedge.cart", Who = Jasper, Gap = 4, If = s => s.Tier == 1,
                    Says = s => Say(Jasper,
                        Loc.T("Jasper Wick, hedge-wright, at your service and everyone else's. That's my cart across the road. No licence, no tithe, " +
                              "no Guild: just a good ear and a low price."),
                        Loc.T("There's room at the Crossroads for two of us, isn't there? I'll take the folk you're too slow for.")),
                    Then = s =>
                    {
                        s.Until[Rival] = s.Day + 14;
                        s.Log("hedge", Loc.T("Jasper Wick, a hedge-wright, set up his cart across the road. He takes one of your customers a day."));
                    },
                },
                new()
                {
                    Key = "hedge.aubel", Who = Aubel, After = "hedge.cart", Gap = 2,
                    Says = s => Say(Aubel,
                        Loc.T("Mistress Aubel, for the Gilded Guild. A hedge-wright is trading at this crossroads without the Guild's licence. You'll " +
                              "have seen him."),
                        Loc.T($"The Guild takes hedge-wrights into its counting-houses, as it takes marked children. A name is worth {Money.Say(s.Purses(0.5f))}.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Jasper Wick, across the road"), () =>
                        {
                            s.TaleGold(s.Purses(0.5f));
                            s.Sway(Faction.Guild, 2);
                            s.Sway(Faction.Commons, -1);
                            s.Goes(Jasper);
                            s.Until.Remove(Rival);
                            s.Flags.Add("hedge.named");
                            s.Log("hedge", Loc.T("You gave the Mark-Finder Jasper Wick's name. The Guild took him, cart and all."));
                            return Loc.T("The Guild thanks you, Keywright. A licensed trade is a safe trade.");
                        }, W(s, s.Purses(0.5f) + Strategy.CustomerDays(s, Math.Max(0, s.Until.GetValueOrDefault(Rival) - s.Day)),
                            (Faction.Guild, 2), (Faction.Commons, -1))),
                        C(Loc.T("I've seen no hedge-wright"), () =>
                        {
                            s.Flags.Add("hedge.shielded");
                            s.Log("hedge", Loc.T("You told the Mark-Finder you'd seen no hedge-wright. Bram was one for twenty years."));
                            return Loc.T("No? Then he's a quick one. The Guild will find him.");
                        }, W(s, 0), kind: true),
                    },
                },
                new()
                {
                    Key = "hedge.contest", Who = Jasper, After = "hedge.aubel", Gap = 1, If = s => s.Flagged("hedge.shielded"),
                    Brings = (s, rng) => Contested(s, rng, Jasper, Fill(rng, "millpond", ShopTiers.Typical(s.Tier), s.Tier, rng.Next(2, 4)),
                        Loc.T("Let's settle it like Keywrights: one chest, read in front of the Crossed Keys. Read it right and I'll take my cart to " +
                              "Thornwick. Wrong, and I stay. One coin, and it's yours either way."),
                        "millpond", s.Purses(0.3f)),
                    Bought = (s, v) =>
                    {
                        if (v.ReadRight)
                        {
                            s.Until.Remove(Rival);
                            s.Until[Crowd] = s.Day + Guild.DaysPerWeek;
                            s.Sway(Faction.Commons, 1);
                            s.Flags.Add("hedge.won");
                            s.Log("hedge", Loc.T("You read Jasper's chest exactly right in front of the whole Crossed Keys. He took his cart to Thornwick, " +
                                                 "and his customers came to you."));
                        }
                        else
                        {
                            s.Flags.Add("hedge.lost");
                            s.Log("hedge", Loc.T("You read Jasper's chest wrong in front of the whole Crossed Keys. His cart stays across the road."));
                        }
                    },
                    Passed = (s, v) =>
                    {
                        s.Flags.Add("hedge.lost");
                        s.Log("hedge", Loc.T("You wouldn't read against Jasper. His cart stays across the road."));
                    },
                },
                new()
                {
                    Key = "hedge.farewell", Who = Jasper, Night = true, After = "hedge.contest", Gap = 0, If = s => s.Flagged("hedge.won"),
                    Says = s => Say(Jasper, Loc.T("Jasper at the back door, his cart packed. \"You've a better ear than me, and you kept the Guild off " +
                                                  "me besides. If you ever need a hedge-wright's help, send to the Lockless in Thornwick. They'll find me.\"")),
                    Then = s =>
                    {
                        s.Owe(Jasper, "hedge");
                        s.Log("hedge", Loc.T("Jasper Wick owes you a favour: you kept the Guild off him."));
                    },
                },
            },
        },

        // The Field's Dead: a knight's chest from Brindle Field, and the knight, who wants his blade sent home to his blood.
        new TaleDef
        {
            Key = "field", Title = Loc.T("The Field's Dead"), Place = TalePlace.Brindle,
            Blurb = Loc.T("Garrick's plough turned up a knight's chest on Brindle Field, and the knight wants his blade back."),
            Ending = FieldEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "field.plough", Who = Garrick, Gap = 5,
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "field", ShopTiers.Typical(s.Tier), s.Tier, rng.Next(2, 4), Category.Arms);
                        chest.Tale = "field";
                        return Brought(s, rng, Garrick, chest, Loc.T("My plough turned this up on the Field, deep, under a knight's shield. Finder's " +
                                                                    "right: it's mine to sell, and I'd sooner not keep it in the house. {price}."), "field");
                    },
                    Bought = (s, v) =>
                    {
                        SetAside(s, v, "field");
                        s.Flags.Add("field.bought");
                        s.Log("field", Loc.T("You bought the knight's chest Garrick ploughed up. Bram set the knight's blade aside: it has a name on it."));
                    },
                    Passed = (s, v) => s.Log("field", Loc.T("You passed on the knight's chest Garrick ploughed up.")),
                },
                new()
                {
                    Key = "field.ghost", Who = Aldwin, Night = true, After = "field.plough", Gap = 0, If = s => s.Flagged("field.bought"),
                    Says = s => Say(Aldwin,
                        Loc.T("A cold knock, and an old knight in rusted mail at the back door, with the moon through him. \"That blade is mine. " +
                              "Aldwin Gale. I fell on that field before the Locking.\""),
                        Loc.T("\"Give it to my blood. There were Gales in Crownhold, once. I have waited three hundred years: I can wait a little more.\"")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll find your family"), () =>
                        {
                            s.Flags.Add("field.promised");
                            s.Log("field", Loc.T("Sir Aldwin Gale's ghost asked you to send his blade to his blood, the Gales of Crownhold. You promised."));
                            return Loc.T("\"Then I can rest a while.\" And he's gone, and the night's warm again.");
                        }, W(s, s.Purses(0.5f) + Strategy.FavourWorth(s)), kind: true),
                        C(Loc.T($"Finder's right: it's mine to sell ({s.Keepsakes.GetValueOrDefault("field")}g)"), () =>
                        {
                            s.TaleGold(s.Keepsakes.GetValueOrDefault("field"));
                            s.Flags.Add("field.sold");
                            s.Until[Haunt] = s.Day + Guild.DaysPerWeek;
                            s.Log("field", Loc.T("You sold Sir Aldwin's blade. His ghost walks the Crossroads at dusk now, and folk stay home."));
                            return Loc.T("\"Then I'll walk.\" The lamps gutter. Out on the Field, something starts to march.");
                        }, s.Keepsakes.GetValueOrDefault("field") - Strategy.CustomerDays(s, Guild.DaysPerWeek)),
                    },
                },
                new()
                {
                    Key = "field.rowan", Who = Rowan, After = "field.ghost", Gap = 1, Tier = 3, If = s => s.Flagged("field.promised"),
                    Says = s => Say(Rowan,
                        Loc.T("Bram says you've a blade for me? \"Aldwin Gale\" on the tang... my great-great-grandfather's. The Guild sold our hall, " +
                              "but it never had this. Where did you find it?"),
                        Loc.T("I can't pay what it's worth. But my house has a claim on the best blade in any chest I sell, and from today it's yours, " +
                              "not mine. And the Gales owe you.")),
                    Then = s =>
                    {
                        s.Keepsakes.Remove("field");
                        s.Flags.Add(AldwinHome);
                        s.Owe(Rowan, "field");
                        s.Sway(Faction.Crown, 1);
                        s.Log("field", Loc.T("You gave Sir Rowan his ancestor's blade. The best weapon in his chests is yours now, and the Gales owe you a favour."));
                    },
                },
                new()
                {
                    Key = "field.rest", Who = Aldwin, Night = true, After = "field.rowan", Gap = 0, Tier = 3,
                    Says = s => Say(Aldwin, Loc.T("One last cold knock. \"My blood has my blade. I can lie down now. Take this: it was in my purse when I " +
                                                  "fell, and I've no use for it.\" A coin, three hundred years old, warm as a hand.")),
                    Then = s =>
                    {
                        string gift = GiftCurio(s, new Random(unchecked(s.Seed * 59 + s.Day)), CurioRarity.Uncommon, CurioTag.Luck);
                        s.Log("field", Loc.T($"Sir Aldwin Gale lay down at last, and left you {gift}."));
                    },
                },
            },
        },

        // The Crossed Keys: the Guild has doubled the inn's tithe, and Bettany's a purse short.
        new TaleDef
        {
            Key = "inn", Title = Loc.T("The Crossed Keys"), Place = TalePlace.Brindle,
            Blurb = Loc.T("The Guild has doubled the Crossed Keys' tithe, and Bettany Crane is a purse short."),
            Ending = InnEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "inn.tab", Who = Bettany, Gap = 3,
                    Says = s => Say(Bettany,
                        Loc.T("The Guild's doubled the inn's tithe, love. \"The Crossed Keys does a good trade\", Vane says. It did, till he came."),
                        Loc.T($"I'm {Money.Say(s.Purses(1))} short, and they'll seal the doors if I can't pay. Lend it me, and you'll have it back and " +
                              $"half again by the next Guild night. On my mother's grave.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Lend her the money ({s.Purses(1)}g)"), () =>
                        {
                            s.Flags.Add("inn.lent");
                            s.Promise(s.Day + 4, Bettany, Loc.T("Bettany, to repay you"));
                            s.Log("inn", Loc.T("You lent Bettany what she was short for the Guild. She'll pay you back, and half again."));
                            return Loc.T("You're a love. Half again, by the next Guild night. On my mother's grave.");
                        }, W(s, s.Purses(0.5f) + Strategy.FavourWorth(s), (Faction.Commons, 1)), cost: s.Purses(1), kind: true),
                        C(Loc.T("I can't spare it"), () =>
                        {
                            s.Flags.Add("inn.refused");
                            s.Log("inn", Loc.T("You couldn't lend Bettany what she was short for the Guild."));
                            return Loc.T("No. No, of course. Who has, these days?");
                        }),
                    },
                },
                new()
                {
                    Key = "inn.repaid", Who = Bettany, After = "inn.tab", Gap = 4, If = s => s.Flagged("inn.lent"),
                    Says = s => Say(Bettany, Loc.T($"{Money.Say(s.Purses(1.5f))}, love: what I owed and half again. The Crossed Keys is still mine. " +
                                                   $"If you ever need a room, a meal or a word in the right ear, it's yours.")),
                    Then = s =>
                    {
                        s.Kept(Bettany);
                        s.TaleGold(s.Purses(1.5f));
                        s.Sway(Faction.Commons, 1);
                        s.Owe(Bettany, "inn");
                        s.Log("inn", Loc.T("Bettany paid you back, and half again. The Crossed Keys stays open, and she owes you a favour."));
                    },
                },
                new()
                {
                    Key = "inn.sealed", Who = Marrow, Night = true, Letter = true, After = "inn.tab", Gap = 2, If = s => s.Flagged("inn.refused"),
                    Says = s => Say(Marrow, Loc.T("A Guild notice, nailed to the Crossed Keys' door and copied to every shop at the crossroads: \"SEALED " +
                                                  "for tithe unpaid, by order of the Gilded Guild. Marrow, Bailiff.\" Bettany's gone north to her sister's.")),
                    Then = s =>
                    {
                        s.Goes(Bettany);
                        s.Flags.Add("inn.sealed");
                        s.Log("inn", Loc.T("The Guild sealed the Crossed Keys for its tithe. Bettany has gone north, and won't be back this season."));
                    },
                },
            },
        },

        // The Harvest Fair: the county comes to Brindle, and the reading contest wants a prize.
        new TaleDef
        {
            Key = "fair", Title = Loc.T("The Harvest Fair"), Place = TalePlace.Brindle,
            Blurb = Loc.T("The harvest fair comes to Brindle, and Garrick wants the new Keywright to put up the reading contest's prize."),
            Ending = FairEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "fair.prize", Who = Garrick, Gap = 6,
                    Says = s => Say(Garrick,
                        Loc.T("Harvest fair's the day after tomorrow, and the whole county comes. There's a reading contest: a chest nobody's opened, " +
                              "and a prize for whoever reads it right."),
                        Loc.T($"Bram always put up the prize. Will you? {Money.Say(s.Purses(0.5f))}, and your name on the banner.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Put up the prize ({s.Purses(0.5f)}g)"), () =>
                        {
                            s.Flags.Add("fair.sponsor");
                            s.Until[FairDay] = s.Day + 2;
                            s.Log("fair", Loc.T("You put up the harvest fair's prize. Your name's on the banner, and the county will come past the stall."));
                            return Loc.T("Bram's kin, on the banner! Half the county'll come past your stall on the day.");
                        }, W(s, Strategy.CustomerDays(s, 1) * GameState.FairCrowd), cost: s.Purses(0.5f), kind: true),
                        C(Loc.T("Not this year"), () =>
                        {
                            s.Log("fair", Loc.T("You didn't put up the harvest fair's prize. The miller did."));
                            return Loc.T("The miller, then. He'll want his name bigger than Bram's ever was.");
                        }),
                    },
                },
                new()
                {
                    Key = "fair.contest", Who = Garrick, After = "fair.prize", Gap = 2,
                    Brings = (s, rng) => Contested(s, rng, Garrick, Fill(rng, "attic", ShopTiers.Typical(s.Tier), s.Tier, rng.Next(2, 4)),
                        s.Flagged("fair.sponsor")
                            ? Loc.T("The fair's chest! Read it right in front of the whole county, and the prize is yours: your own money back, and the " +
                                    "miller's on top. One coin to enter.")
                            : Loc.T("The fair's chest, and the miller's prize. Read it right in front of the whole county and it's yours. One coin to enter."),
                        "attic", s.Purses(s.Flagged("fair.sponsor") ? 1 : 0.5f)),
                    Bought = (s, v) =>
                    {
                        if (v.ReadRight)
                        {
                            s.Sway(Faction.Commons, 1);
                            s.Owe(Garrick, "fair");
                            s.Flags.Add("fair.won");
                            s.Log("fair", Loc.T("You read the fair's chest exactly right, and won the prize. The county cheered; Garrick says he owes you a pint and a favour."));
                        }
                        else
                        {
                            s.Flags.Add("fair.lost");
                            s.Log("fair", Loc.T("You read the fair's chest wrong, in front of the whole county. The miller's boy won."));
                        }
                    },
                    Passed = (s, v) => s.Log("fair", Loc.T("You didn't enter the fair's reading contest.")),
                },
            },
        },
    };

    static Line HedgeEnding(GameState s)
    {
        if (s.Flagged("hedge.named"))
            return new(Bram, Loc.T("The Guild took Jasper Wick to a counting-house, and sold his cart. He was a hedge-wright, like I was. I keep thinking that."));
        if (s.Flagged("hedge.won"))
            return new(Jasper, Loc.T("I've a cellar in Crownhold now, and the Lockless bring me every chest their grandmothers ever locked. Best ear at the Crossroads? Yours, Keywright."));
        if (s.Flagged("hedge.lost"))
            return new(Jasper, Loc.T("Still across the road, still cheaper than you, and still no licence. Come and have a drink with the competition some time."));
        return s.Did("hedge.cart") ? new(Bram, Loc.T("Jasper Wick traded across the road all season. The Guild never caught him. I hope it never does.")) : null;
    }

    static Line FieldEnding(GameState s)
    {
        if (s.Flagged(AldwinHome))
            return new(Rowan, Loc.T("Great-great-grandfather's blade hangs in the Gales' hall again, and he walks no more. The Gales don't forget a debt, Keywright."));
        if (s.Flagged("field.sold"))
            return new(Bram, Loc.T("Sir Aldwin still walks Brindle Field at dusk, looking for his blade. Garrick won't plough the north end. Can't blame him."));
        if (s.Flagged("field.promised"))
            return new(Bram, Loc.T("Sir Aldwin's blade is still in our back room, waiting for a Gale. He's patient. He's had the practice."));
        return s.Did("field.plough") ? new(Garrick, Loc.T("Sold the knight's chest to a tinker in the end. He came back the next week, white as milk, and gave it back.")) : null;
    }

    static Line InnEnding(GameState s)
    {
        if (s.Flagged("inn.lent"))
            return new(Bettany, Loc.T("The Crossed Keys is still mine, love, and there's always a pint behind the bar with your name on it. Bram's too, the old soak."));
        return s.Flagged("inn.sealed")
            ? new(Bram, Loc.T("The Guild sealed the Crossed Keys. Bettany's up north with her sister. The crossroads is quieter than I've ever known it."))
            : s.Did("inn.tab") ? new(Bettany, Loc.T("I found the money in the end. Don't ask where, love.")) : null;
    }

    static Line FairEnding(GameState s)
    {
        if (s.Flagged("fair.won"))
            return new(Garrick, s.Flagged("fair.sponsor")
                ? Loc.T("Put up the prize and won it back! Folk are still talking about it. Best harvest fair since Bram's day.")
                : Loc.T("The miller's still sore you won his prize. Best harvest fair since Bram's day, and that's saying something."));
        if (s.Flagged("fair.lost"))
            return new(Garrick, Loc.T("The miller's boy read the fair's chest better than Bram's kin. He'll never let you forget it, and neither will I."));
        return s.Flagged("fair.sponsor") ? new(Garrick, Loc.T("Your name was on the banner, and the county came. Next year, enter the contest too.")) : null;
    }
}
