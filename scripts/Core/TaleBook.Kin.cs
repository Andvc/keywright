using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// 0.32: the tales of whoever minds the stall (docs/stories.md, "Their own tales"). Kit is Bram's niece and her story is
/// the Key's; Hesk, Rosa and Isolde aren't family at all: each was born with the Key-mark somewhere else and came to
/// Bram's stall for reasons of their own, and each has a tale that's drawn every season they play, ending in a keepsake.
/// </summary>
public static partial class TaleBook
{
    // The people of the partners' own tales.
    public const string Mabry = "mabry", Vash = "vash", Marta = "marta", Pemberton = "pemberton";

    /// <summary>Whose own tale a season holds, by who's minding the stall (null for Kit: hers is the Key).</summary>
    public static string Own(string character) => character switch
    {
        Characters.Hesk => "hesk",
        Characters.Rosa => "rosa",
        Characters.Isolde => "isolde",
        _ => null,
    };

    static IEnumerable<TaleDef> KinTales() => new[]
    {
        // Hesk's: the drowned town of Fallowford, and its common chest.
        new TaleDef
        {
            Key = "hesk", Title = Loc.T("The Drowned Town"), Place = TalePlace.Kin,
            Blurb = Loc.T("Hesk's town, Fallowford, went under the Aldwater when the Guild opened the weir. Its common chest went down with " +
                          "the town hall: every family's savings, in the town's name. Now the Guild's salvage barges are dredging for it."),
            Ending = s => !s.Did("hesk.letter") ? null
                : s.Did("hesk.weights") ? new(Mabry, Loc.T("Fallowford's building again, up on the hill where the water can't reach. Every family had its share. They named the new well after Hesk."))
                : s.Flagged("hesk.lost") ? new(Mabry, Loc.T("Fallowford's chest went down the Gate on the Guild's last barge. Nobody will ever know what was in it. I still dream about the hall."))
                : new(Mabry, Loc.T("Fallowford's scattered from here to the coast. I write to Hesk every week. He doesn't always write back.")),
            Beats = new Beat[]
            {
                new()
                {
                    Key = "hesk.letter", Who = Mabry, Gap = 3, Night = true, Letter = true,
                    Says = s => Say(Mabry,
                        Loc.T("Hesk, it's Mabry. The Guild's salvage barges are dredging Fallowford, and they've found the town hall under " +
                              "the mud. The common chest was in the hall: every family's savings, warded in the town's name. If the Guild " +
                              "raises it, they'll call it salvage and sell it, and it won't open for whoever buys it. It'll go down the Gate " +
                              "with the rest. You're the only one of us with the mark. Watch for it. Your sister.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Write back: I'll watch for it"), () =>
                        {
                            s.Flags.Add("hesk.watch");
                            s.Post("fallowford", Loc.T("Fallowford's common chest"),
                                Loc.T("Raised from the Aldwater by the Guild's salvage barges. It belongs to the people of Fallowford: it will open for no one else."));
                            s.Log("hesk", Loc.T("Mabry wrote: the Guild is dredging drowned Fallowford for the town's common chest. You promised to watch for it."));
                            return Loc.T("(The letter goes north on the night coach.)");
                        }, s.Purses(1f), kind: true),
                        C(Loc.T("Fallowford's gone, Mabry"), () =>
                        {
                            s.Log("hesk", Loc.T("You wrote to Mabry that drowned Fallowford is gone, and should be let go."));
                            return Loc.T("(You burn her letter. It doesn't help.)");
                        }),
                    },
                },
                new()
                {
                    Key = "hesk.mabry", Who = Mabry, After = "hesk.letter", Gap = 3, Tier = 2, If = s => s.Flagged("hesk.watch"),
                    Says = s => Say(Mabry,
                        Loc.T("Brother. You look like Father did, behind a counter. Mind your back."),
                        Loc.T("They've raised it. The chest's at the Guild's yard, for the salvage sale. Bailiff Marrow's selling it to a Keywright " +
                              "off the Guild's own list."),
                        Loc.T("It's ours, Hesk. Stand up at the sale and say so, out loud, with the mark on your hand, and the Wardens will have to hear it.")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll stand witness for Fallowford"), () =>
                        {
                            s.Flags.Add("hesk.witness");
                            s.Sway(Faction.Guild, -1);
                            s.Log("hesk", Loc.T("Mabry came: Fallowford's chest goes to the Guild's salvage sale, and Bailiff Marrow will bring it to your " +
                                                "counter. You'll stand witness that it's the town's."));
                            return Loc.T("Then I'll tell them you're coming. All of them.");
                        }, W(s, s.Purses(1.2f), (Faction.Guild, -1)), kind: true),
                        C(Loc.T("I can't fight the Guild"), () =>
                        {
                            s.Flags.Add("hesk.lost");
                            s.TakeDown("fallowford");
                            s.Log("hesk", Loc.T("You told Mabry you couldn't fight the Guild for Fallowford's chest."));
                            return Loc.T("Then it goes down the Gate, and nobody will even know whose it was.");
                        }),
                    },
                },
                new()
                {
                    Key = "hesk.sale", Who = Marrow, After = "hesk.mabry", Gap = 2, Tier = 3, If = s => s.Flagged("hesk.witness"),
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "river", Finer(s.Tier, 1), s, 4);
                        chest.Owner = Loc.T("the people of Fallowford");
                        chest.Tale = "fallowford";
                        return Brought(s, rng, Marrow, chest,
                            Loc.T("Salvage from Fallowford, by the Guild's right. The town's common chest, they tell me. {price}, and the Guild thanks you."), "seized");
                    },
                    Bought = (s, v) => s.Log("hesk", Loc.T("You bought Fallowford's chest at the Guild's sale. It's cold to your mark: the Guild seized it, " +
                                                          "it didn't own it. It waits in your back room for its people.")),
                    Passed = (s, v) =>
                    {
                        if (v.Outcome == VisitOutcome.Reported)
                        {
                            s.Flags.Add("hesk.home");
                            s.Sway(Faction.Guild, -1);
                            s.TakeDown("fallowford");
                            s.Log("hesk", Loc.T("Marrow brought Fallowford's chest, cold to your mark, and you sent for the Wardens: it goes home to its people."));
                        }
                        else
                        {
                            s.Flags.Add("hesk.lost");
                            s.TakeDown("fallowford");
                            s.Log("hesk", Loc.T("Fallowford's chest went to the Guild's buyer. It won't open for him: it'll go down the Gate."));
                        }
                    },
                },
                new()
                {
                    // Home by the Wardens, or given back from the back room at Nim's table ("returned:fallowford").
                    Key = "hesk.home", Who = Mabry, After = "hesk.sale", Gap = 1, Tier = 3,
                    If = s => s.Flagged("hesk.home") || s.Flagged("returned:fallowford"),
                    Brings = (s, rng) => Given(s, rng, Mabry, Fill(rng, "river", Finer(s.Tier, 1), s, 4),
                        Loc.T("The town voted, Hesk: you open it for us, and every family gets its share. This is yours, freely given. One coin, for luck."),
                        "river"),
                    Bought = (s, v) =>
                    {
                        s.Sway(Faction.Commons, 1);
                        s.Log("hesk", Loc.T("Fallowford's people gave you their common chest to open. Every family will have its share."));
                    },
                    Passed = (s, v) => s.Log("hesk", Loc.T("Fallowford's people offered you their common chest to open, and you let them take it home shut.")),
                },
                new()
                {
                    Key = "hesk.weights", Who = Mabry, After = "hesk.home", Gap = 0, Night = true,
                    Says = s => Say(Mabry,
                        Loc.T("Every family's had its share. Old Tamsin cried. So did I."),
                        Loc.T("They want you to have the town's assayer's weights. Father kept them, and they're true to the grain. Weigh honest, brother.")),
                    Then = s =>
                    {
                        s.GiveKeepsake(Keepsakes.Weights);
                        s.Owe(Mabry, "hesk");
                        s.Log("hesk", Loc.T("Mabry gave you Fallowford's weights, and the town owes you a favour."));
                    },
                },
                new()
                {
                    Key = "hesk.gate", Who = Mabry, After = "hesk.sale", Gap = 2, Night = true, Letter = true,
                    If = s => s.Flagged("hesk.lost") && !s.Flagged("returned:fallowford"),
                    Says = s => Say(Mabry,
                        Loc.T("Hesk. The Guild's buyer couldn't open it, so it went down the Gate on the last barge. I watched it go. I'm going to " +
                              "the coast, where there's work. Mind your back. Mabry.")),
                    Then = s => s.Log("hesk", Loc.T("Fallowford's chest went down the Gate. Mabry has gone to the coast.")),
                },
            },
        },

        // Rosa's: her mother's stall on Lantern Lane, which the Guild gave to Orrin Vash.
        new TaleDef
        {
            Key = "rosa", Title = Loc.T("Lantern Lane"), Place = TalePlace.Kin,
            Blurb = Loc.T("Rosa's mother kept the best stall on Crownhold's Lantern Lane until the Guild took it for tithe and gave it to its " +
                          "own man, Orrin Vash. By the Lane's old custom, a family wins its stall back if one of its blood reads a chest exactly."),
            Ending = s => !s.Did("rosa.letter") ? null
                : s.Did("rosa.key") ? new(Marta, Loc.T("The Brightwater stall's open again on Lantern Lane, and Orrin Vash sells from a barrow. Rosa's grandmother's sign is back up."))
                : s.Flagged("rosa.sold") ? new(Marta, Loc.T("Orrin Vash still has our stall. Rosa took his purse. I'm not angry. I'm only old."))
                : new(Marta, Loc.T("Orrin Vash still has our stall. Next season, Rosa says. She always says next season.")),
            Beats = new Beat[]
            {
                new()
                {
                    Key = "rosa.letter", Who = Marta, Gap = 3, Night = true, Letter = true,
                    Says = s => Say(Marta,
                        Loc.T("Rosa, love. Orrin Vash has hung his sign over our stall, the Guild's own man. But the Lane keeps its old " +
                              "custom: a family gets its stall back if one of its blood, with the mark, reads a chest in front of the traders, " +
                              "exactly, every last thing in it. You're the only one of us with the mark. Your mother, Marta.")),
                    Asks = s => new()
                    {
                        C(Loc.T("I'll come and read it"), () =>
                        {
                            s.Flags.Add("rosa.come");
                            s.Log("rosa", Loc.T("Your mother wrote: by the Lane's custom, you can win back the Brightwater stall by reading a chest " +
                                                "exactly, in front of the traders of Lantern Lane."));
                            return Loc.T("(You write back: when the shop's in Crownhold, you'll be there.)");
                        }, s.Purses(1f), kind: true),
                        C(Loc.T("Let Vash have it"), () =>
                        {
                            s.Log("rosa", Loc.T("You told your mother to let Orrin Vash keep the stall."));
                            return Loc.T("(You don't sleep much, that night.)");
                        }),
                    },
                },
                new()
                {
                    Key = "rosa.vash", Who = Vash, After = "rosa.letter", Gap = 4, Tier = 2, If = s => s.Flagged("rosa.come"),
                    Says = s => Say(Vash,
                        Loc.T("You'll be the Brightwater girl. You have your mother's chin. And her habit of haggling over everything."),
                        Loc.T($"The Lane's best pitch is mine now, by the Guild's grant. Here's {Money.Say(s.Purses(1.5f))} to stay in the " +
                              $"country and leave old customs alone.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Take his purse (+{s.Purses(1.5f)}g)"), () =>
                        {
                            s.Flags.Add("rosa.sold");
                            s.TaleGold(s.Purses(1.5f));
                            s.Sway(Faction.Guild, 1);
                            s.Log("rosa", Loc.T("You took Orrin Vash's purse to stay off Lantern Lane."));
                            return Loc.T("Sensible girl. Your mother never was.");
                        }, W(s, s.Purses(1.5f), (Faction.Guild, 1))),
                        C(Loc.T("Keep it. I'll see you on the Lane"), () =>
                        {
                            s.Flags.Add("rosa.defied");
                            s.Log("rosa", Loc.T("Orrin Vash offered you a purse to stay away. You'll see him on Lantern Lane, when the shop's in Crownhold."));
                            return Loc.T("Then bring your best ears. The traders will put up something hard.");
                        }, s.Purses(1.6f), kind: true),
                    },
                },
                new()
                {
                    Key = "rosa.reading", Who = Marta, After = "rosa.vash", Gap = 1, Tier = 3, If = s => s.Flagged("rosa.defied"),
                    Brings = (s, rng) =>
                    {
                        var visit = Contested(s, rng, Marta, Fill(rng, "pawn", Finer(s.Tier, 1), s, 4),
                            Loc.T("The traders put this up, love: read it exactly, every last thing in it, and the stall's ours again. One coin, for luck, the old way."),
                            "pawn", s.Purses(0.5f));
                        visit.ContestFor = Loc.T("The Lane's custom: read it exactly right, and the Brightwater stall is your family's again.");
                        return visit;
                    },
                    Bought = (s, v) =>
                    {
                        if (v.ReadRight)
                        {
                            s.Flags.Add("rosa.won");
                            s.Log("rosa", Loc.T("You read the traders' chest exactly, in front of all Lantern Lane. The Brightwater stall is your family's again."));
                        }
                        else
                        {
                            s.Flags.Add("rosa.missed");
                            s.Log("rosa", Loc.T("You read the traders' chest wrong, in front of all Lantern Lane. Vash keeps the stall, for now."));
                        }
                    },
                    Passed = (s, v) =>
                    {
                        s.Flags.Add("rosa.missed");
                        s.Log("rosa", Loc.T("You wouldn't read the traders' chest. Vash keeps the stall, for now."));
                    },
                },
                new()
                {
                    Key = "rosa.again", Who = Vash, After = "rosa.reading", Gap = 2, Tier = 3, If = s => s.Flagged("rosa.missed"),
                    Says = s => Say(Vash,
                        Loc.T("A Brightwater, beaten by a box. Your mother wept, I hear."),
                        Loc.T($"The custom allows a second reading, if your family stands the traders their supper: {Money.Say(s.Purses(1))}. " +
                              $"Lose again and the Lane never hears the name Brightwater.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Stand them supper ({s.Purses(1)}g)"), () =>
                        {
                            s.Flags.Add("rosa.second");
                            s.Log("rosa", Loc.T("You stood the traders of Lantern Lane their supper, for a second reading."));
                            return Loc.T("Tomorrow, then. Don't make a fool of your mother twice.");
                        }, s.Purses(0.2f), cost: s.Purses(1)),
                        C(Loc.T("Keep your stall"), () =>
                        {
                            s.Log("rosa", Loc.T("You let the Lane's second reading go. Vash keeps the Brightwater stall."));
                            return Loc.T("Wise. It suits me, that stall.");
                        }, kind: true),
                    },
                },
                new()
                {
                    Key = "rosa.second", Who = Marta, After = "rosa.again", Gap = 1, Tier = 3, If = s => s.Flagged("rosa.second"),
                    Brings = (s, rng) =>
                    {
                        var visit = Contested(s, rng, Marta, Fill(rng, "pawn", Finer(s.Tier, 1), s, 4),
                            Loc.T("The second reading, love. The whole Lane's watching. One coin, for luck. Breathe."), "pawn", s.Purses(0.5f));
                        visit.ContestFor = Loc.T("The Lane's second reading: exactly right, and the Brightwater stall is your family's again.");
                        return visit;
                    },
                    Bought = (s, v) =>
                    {
                        if (v.ReadRight) s.Flags.Add("rosa.won");
                        s.Log("rosa", v.ReadRight ? Loc.T("You read the second chest exactly. The Brightwater stall is your family's again.")
                            : Loc.T("You read the second chest wrong too. The Lane will never hear the name Brightwater again."));
                    },
                    Passed = (s, v) => s.Log("rosa", Loc.T("You never read the second chest. The Brightwater stall is Vash's for good.")),
                },
                // The stall won, after either reading.
                new()
                {
                    Key = "rosa.key", Who = Marta, After = "rosa.reading", Gap = 0, Night = true, If = s => s.Flagged("rosa.won"),
                    Says = KeySays,
                    Then = KeyThen,
                },
                new()
                {
                    Key = "rosa.keyagain", Who = Marta, After = "rosa.second", Gap = 0, Night = true, If = s => s.Flagged("rosa.won") && !s.Did("rosa.key"),
                    Says = KeySays,
                    Then = KeyThen,
                },
            },
        },

        // Isolde's: House Marchmont's three chests, sold at the Guild's sale.
        new TaleDef
        {
            Key = "isolde", Title = Loc.T("The Marchmont Pieces"), Place = TalePlace.Kin,
            Blurb = Loc.T("When the Guild foreclosed on House Marchmont it sold the family's chests to whoever bid. Isolde's old steward has " +
                          "traced the three that matter: her grandmother's jewel case, her father's map chest, and the long box with the seal."),
            Ending = s => !s.Did("isolde.letter") ? null
                : s.Did("isolde.whole") ? new(Pemberton, Loc.T("House Marchmont's seal is back on Lady Isolde's hand, and the jewel case and the maps beside it. Her grandmother would have wept."))
                : s.Did("isolde.seal") ? new(Pemberton, Loc.T("The Marchmont seal is home. The rest of the house's pieces are scattered, but the seal is what matters."))
                : new(Pemberton, Loc.T("The Marchmont pieces are scattered across Aldmark still. My lady says she has her collection, and that's house enough.")),
            Beats = new Beat[]
            {
                new()
                {
                    Key = "isolde.letter", Who = Pemberton, Gap = 3, Night = true, Letter = true,
                    Says = s => Say(Pemberton,
                        Loc.T("My lady. Since the Guild's sale I have traced three of House Marchmont's chests: your grandmother's jewel case, " +
                              "your father's map chest, and the long box with the seal. Their buyers will sell, for a price. They're the " +
                              "house's still, whoever paid the Guild: they'll be warm to your hand. Shall I tell the buyers where to find you? " +
                              "Your servant, Pemberton.")),
                    Asks = s => new()
                    {
                        C(Loc.T("Tell them to come"), () =>
                        {
                            s.Flags.Add("isolde.seek");
                            s.Log("isolde", Loc.T("Pemberton traced three of House Marchmont's chests: the jewel case, the map chest and the long box with " +
                                                  "the seal. Their buyers will bring them to your counter."));
                            return Loc.T("(You write to Pemberton: send them.)");
                        }, s.Purses(1f), kind: true),
                        C(Loc.T("Let the past lie"), () =>
                        {
                            s.Log("isolde", Loc.T("You told Pemberton to let House Marchmont's chests go."));
                            return Loc.T("(You put the letter in your collection's drawer, unanswered.)");
                        }),
                    },
                },
                new()
                {
                    Key = "isolde.case", Who = Hesper, After = "isolde.letter", Gap = 3, Tier = 2, If = s => s.Flagged("isolde.seek"),
                    Brings = (s, rng) => Brought(s, rng, Hesper, Fill(rng, "dowry", Finer(s.Tier, 1), s, 3, Category.Gems),
                        Loc.T("The Marchmont jewel case, my lady. I bought it at the Guild's sale for a dowry, and the match fell through. {price}. Warm to your hand, is it? Well, it would be."),
                        "dowry"),
                    Bought = (s, v) =>
                    {
                        s.Flags.Add("isolde.case");
                        s.Log("isolde", Loc.T("You bought back your grandmother's jewel case from Aunt Hesper."));
                    },
                    Passed = (s, v) => s.Log("isolde", Loc.T("You let your grandmother's jewel case go to another buyer.")),
                },
                new()
                {
                    Key = "isolde.maps", Who = Quire, After = "isolde.letter", Gap = 6, Tier = 3, If = s => s.Flagged("isolde.seek"),
                    Brings = (s, rng) => Brought(s, rng, Quire, Fill(rng, "archive", Finer(s.Tier, 1), s, 3, Category.Scrolls),
                        Loc.T("Lord Marchmont's map chest, bought for the Archive at the Guild's sale. The Roll still lists it as your house's, my lady. {price}."),
                        "archive"),
                    Bought = (s, v) =>
                    {
                        s.Flags.Add("isolde.maps");
                        s.Log("isolde", Loc.T("You bought back your father's map chest from Master Quire."));
                    },
                    Passed = (s, v) => s.Log("isolde", Loc.T("You let your father's map chest stay in the Archive.")),
                },
                new()
                {
                    Key = "isolde.vane", Who = Story.Vane, After = "isolde.letter", Gap = 8, Tier = 3, If = s => s.Flagged("isolde.seek") && (s.Did("isolde.maps") || s.Gone.Contains(Quire)),
                    Says = s => Say(Story.Vane,
                        Loc.T("Lady Marchmont. The Guild kept one piece of your house for itself: the long box, with your family's seal inside."),
                        Loc.T($"Seals are dangerous things in the wrong hands. In the right ones, {Money.Say(s.Purses(2))}. The Guild is nothing if not reasonable.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Buy back the seal ({s.Purses(2)}g)"), () =>
                        {
                            s.Flags.Add("isolde.sealbox");
                            s.Log("isolde", Loc.T("You paid Mister Vane for the long box with House Marchmont's seal."));
                            return Loc.T("A pleasure. The Guild will have it sent round.");
                        }, s.Purses(2.2f), cost: s.Purses(2)),
                        C(Loc.T("Not at that price"), () =>
                        {
                            s.Flags.Add("isolde.declined");
                            s.Log("isolde", Loc.T("Mister Vane offered you the Marchmont seal, for a price you wouldn't pay. He'll ask again, he says."));
                            return Loc.T("A pity. The Guild is patient, my lady. I'll ask again, once.");
                        }, kind: true),
                    },
                },
                // Once more, a few days on: the Guild is patient, and it wants the gold.
                new()
                {
                    Key = "isolde.vane2", Who = Story.Vane, After = "isolde.vane", Gap = 5, Tier = 3, If = s => s.Flagged("isolde.declined"),
                    Says = s => Say(Story.Vane,
                        Loc.T($"Lady Marchmont. The long box is still in the Guild's vault, and the price is still {Money.Say(s.Purses(2))}. The last time I'll ask.")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Buy back the seal ({s.Purses(2)}g)"), () =>
                        {
                            s.Flags.Add("isolde.sealbox");
                            s.Log("isolde", Loc.T("You paid Mister Vane for the long box with House Marchmont's seal."));
                            return Loc.T("A pleasure. The Guild will have it sent round.");
                        }, s.Purses(2.2f), cost: s.Purses(2)),
                        C(Loc.T("The Guild can keep it"), () =>
                        {
                            s.Log("isolde", Loc.T("You let the Guild keep House Marchmont's seal."));
                            return Loc.T("As you wish. It will look very well in the counting-house.");
                        }, kind: true),
                    },
                },
                // The seal home: the keepsake, and a word on whatever else came back.
                new()
                {
                    Key = "isolde.seal", Who = Pemberton, After = "isolde.vane", Gap = 1, Night = true,
                    If = s => s.Flagged("isolde.sealbox") && !(s.Flagged("isolde.case") && s.Flagged("isolde.maps")),
                    Says = s => Say(Pemberton,
                        Loc.T("The seal, my lady. Your grandmother pressed it into every letter she ever sent. Wear it, and let them see it at the door.")),
                    Then = s =>
                    {
                        s.GiveKeepsake(Keepsakes.Seal);
                        s.Log("isolde", Loc.T("House Marchmont's seal is back on your hand."));
                    },
                },
                new()
                {
                    Key = "isolde.whole", Who = Pemberton, After = "isolde.vane", Gap = 1, Night = true,
                    If = s => s.Flagged("isolde.sealbox") && s.Flagged("isolde.case") && s.Flagged("isolde.maps"),
                    Says = s => Say(Pemberton,
                        Loc.T("The jewel case, the maps and the seal, all three under one roof. House Marchmont is a house again, my lady."),
                        Loc.T("The King's court has heard. A Marchmont in trade, they say, and a good one. Wear the seal, and let them see it at the door.")),
                    Then = s =>
                    {
                        s.GiveKeepsake(Keepsakes.Seal);
                        s.Sway(Faction.Crown, 1);
                        s.Log("isolde", Loc.T("All three Marchmont pieces are home, and the seal is back on your hand. The Crown has heard of it."));
                    },
                },
            },
        },
    };

    static List<Line> KeySays(GameState s) => Say(Marta,
        Loc.T("The whole Lane's talking of nothing else. Vash is selling from a barrow, and I've never been happier in my life."),
        Loc.T("Here: your grandmother's stall key. Every trader on the Lane knows it. Show it, and they'll know a Brightwater deals fair."));

    static void KeyThen(GameState s)
    {
        s.GiveKeepsake(Keepsakes.StallKey);
        s.Owe(Marta, "rosa");
        s.Sway(Faction.Commons, 1);
        s.Log("rosa", Loc.T("Your mother gave you your grandmother's stall key. The Brightwater stall is open again on Lantern Lane."));
    }
}
