using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The Gilding (0.33, docs/narrative.md): the main story's beats at the counter and the back door, drawn every season.
/// The story mornings and Vane's nights tell the season's episodes (<see cref="Story"/>); these bring the case its
/// clues from the people who'd know (Nell Haddon, Bettany, Sister Agathe, Sergeant Hollis, Mara's letter, Old Aldo) and
/// from chests (Haddon's own, a Guild strongroom's, and from the second season one marked by your own hand). Each comes
/// once, at the shop and week it belongs to, and none of it is a roll.
/// </summary>
public static partial class TaleBook
{
    public const string Nell = "nell";

    static IEnumerable<TaleDef> GildingTales() => new[]
    {
        new TaleDef
        {
            Key = Story.Tale, Title = Loc.T("The Gilding"), Place = TalePlace.Always,
            Blurb = Loc.T("Old Haddon the wheelwright turned to gold at the crossroads, the night after the Guild foreclosed on him, and Bram " +
                          "has started wearing a glove. The notebook's Case page keeps the questions, and what you've found."),
            Beats = new Beat[]
            {
                // Week 1: Haddon's daughter brings his last chest, the one the Guild's men couldn't open.
                new()
                {
                    Key = "gild.nell", Who = Nell, Gap = 3, If = s => s.Week <= 2,
                    Brings = (s, rng) => Given(s, rng, Nell, Fill(rng, "seized", ShopTiers.Typical(s.Tier), s.Tier, 2, Category.Arms),
                        Loc.T("My father's last chest. Haddon, the wheelwright. The Guild's men took the rest, but this wouldn't open for them. " +
                              "He signed their paper for a new lathe, against his own hand. He laughed at that. One coin, for luck."), "seized"),
                    Bought = (s, v) =>
                    {
                        s.Find("nell");
                        s.Find("haddon.paper");
                        s.Log(Story.Tale, Loc.T("Nell Haddon gave you her father's last chest. Under his tools was the Guild's paper, in his hand."));
                    },
                    Passed = (s, v) =>
                    {
                        s.Find("nell");
                        s.Log(Story.Tale, Loc.T("Nell Haddon brought her father's last chest, and you let her take it home."));
                    },
                },
                // Week 2: Bettany has seen that glove before.
                new()
                {
                    Key = "gild.bettany", Who = Bettany, Gap = 7, If = s => s.BramGloved,
                    Says = s => Say(Bettany,
                        Loc.T("That glove of Bram's. I've seen it before, love. Twenty years back, the winter he took a marked baby out of the " +
                              "Guild's care."),
                        Loc.T("The Mark-Finders had her in a basket, and then they didn't. Nobody ever knew what Bram paid them.")),
                    Then = s =>
                    {
                        s.Find("bettany.baby");
                        s.Log(Story.Tale, Loc.T("Bettany says Bram took a marked baby out of the Guild's care, twenty years ago. Nobody knows what he paid."));
                    },
                },
                // Week 3, at Thornwick: the Unopened say what the gold is.
                new()
                {
                    Key = "gild.agathe", Who = Agathe, Gap = 11, Tier = 2,
                    Says = s => Say(Agathe,
                        Loc.T("Sister Agathe of the Unopened, with a bowl for alms. \"What's pledged to the Guild is pledged to gold, Keywright, " +
                              "and gold has one master. Look at Thornwick's square.\""),
                        Loc.T("\"And every coin you let out of a chest is an eye for him. You open them. He sees.\"")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Put something in her bowl ({ShopTiers.Scale(s.Tier) * 3}g)"), () =>
                        {
                            s.Sway(Faction.Abbey, 1);
                            s.Log(Story.Tale, Loc.T("Sister Agathe says what's pledged to the Guild is pledged to gold, and every coin out of a chest is the Maw's eye. You gave to her bowl."));
                            return Loc.T("\"Saint Ide sees it too. Keep your chests shut, if you can.\"");
                        }, W(s, -ShopTiers.Scale(s.Tier) * 3, (Faction.Abbey, 1)), cost: ShopTiers.Scale(s.Tier) * 3, kind: true),
                        C(Loc.T("I've a living to make"), () =>
                        {
                            s.Log(Story.Tale, Loc.T("Sister Agathe says what's pledged to the Guild is pledged to gold, and every coin out of a chest is the Maw's eye."));
                            return Loc.T("\"So did they, in the square.\"");
                        }),
                    },
                    Then = s =>
                    {
                        s.Find("unopened.gold");
                        s.Find("unopened.eye");
                    },
                },
                // Week 3: the Wardens' sergeant counts the Guild's carts, and one night follows one.
                new()
                {
                    Key = "gild.hollis", Who = Hollis, Gap = 13, Tier = 2,
                    Says = s => Say(Hollis,
                        Loc.T("Sergeant Hollis, off duty and uneasy. \"Guild carts pass my toll every fifth night, under sacking. They don't stop " +
                              "at the Guild Hall: they go on down the palace road.\""),
                        Loc.T("\"Last week a sack slipped. It was a man, Keywright. A gold man. Next Guild night, I'm going to follow one.\"")),
                    Then = s =>
                    {
                        s.Find("hollis.carts");
                        s.Log(Story.Tale, Loc.T("Sergeant Hollis says the Guild's carts take the gilded down the palace road every fifth night. He means to follow one."));
                    },
                },
                new()
                {
                    Key = "gild.hollis.gate", Who = Hollis, Night = true, After = "gild.hollis", Gap = 1, Tier = 2,
                    If = s => s.DayOfWeek == 1 || s.DuesTonight,
                    Says = s => Say(Hollis,
                        Loc.T("Hollis at the back door, grey in the face. \"I followed the cart. Under the palace there's a door with a seam of " +
                              "gold down the middle. They weighed him on a great scale, by the stone.\""),
                        Loc.T("\"Then the seam opened a crack, and something on the other side took him. I heard it say thank you.\"")),
                    Then = s =>
                    {
                        s.Find("hollis.gate");
                        s.Log(Story.Tale, Loc.T("Hollis followed a Guild cart to a door under the palace with a seam of gold. The gilded are weighed, and something behind it takes them."));
                    },
                },
                // Week 4, at Crownhold: a chest from the Guild's strongroom, sold off at its sale.
                new()
                {
                    Key = "gild.strongroom", Who = Brass, Gap = 16, Tier = 3,
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "seized", ShopTiers.Typical(s.Tier), s.Tier, 2, Category.Coins);
                        chest = new Chest(chest.Everything.Append(new Item(Category.Coins, chest.Quality, Fake: true)), chest.Quality, lockTier: chest.Lock);
                        chest.Tale = Story.Tale;
                        return Brought(s, rng, Brass, chest, Loc.T("From the Guild's own strongroom: its sale's leavings, the clerk said. Coin, mostly. " +
                                                                    "{price}."), "seized");
                    },
                    Bought = (s, v) =>
                    {
                        if (v.Chest.Everything.Any(i => i.Fake && i.Category == Category.Coins)) s.Find("guild.demongold");
                        s.Log(Story.Tale, Loc.T("A chest from the Guild's own strongroom held demon gold. The Guild deals in the Maw's gilded iron."));
                    },
                },
                // Week 5: Mara's letter, after Vane's word about kings.
                new()
                {
                    Key = "gild.note", Who = Story.Mara, Night = true, Letter = true, Gap = 21, If = s => s.Clues.Contains("vane.kings") && !s.MapGiven,
                    Says = s => Say(Story.Mara,
                        Loc.T("A note under the door, in Mara's hand: \"Even kings pledge, he said. So I read the Chest Roll. The Crown Chest: " +
                              "pledged to the Gilded Guild, against the Crown's note.\""),
                        Loc.T("\"The Locking is a chest like any other. It opens for its rightful owner. If the Crown can't pay on the season's " +
                              "last night, that's the Maw.\"")),
                    Then = s =>
                    {
                        s.Find("roll.crown");
                        s.Find("mara.owner");
                        s.Log(Story.Tale, Loc.T("Mara read the Chest Roll: the King pledged the Crown Chest to the Guild, and the note falls due on the season's last night."));
                    },
                },
                // Weeks 5 to 7, at the Emporium: the King himself, in a plain cloak.
                new()
                {
                    Key = "gild.aldo", Who = Aldo, Gap = 21, Tier = 4, If = s => s.Day > s.TierSince,
                    Says = s => Say(Aldo,
                        Loc.T("Ten years ago there was a famine, and the Crown had nothing it could spend. So I pledged the one thing it owned " +
                              "outright. You know which, I think."),
                        Loc.T("A gift is rightful, Keywright. Give a thing away before the note falls due, to someone who can hold it, and " +
                              "there's nothing left to collect.")),
                    Then = s =>
                    {
                        s.Find("aldo.note");
                        s.Find("aldo.gift");
                        s.Kept(Aldo);
                        s.Log(Story.Tale, Loc.T("Old Aldo pledged the Crown Chest to feed the realm through a famine. A gift is rightful, he says: there'd be nothing left to collect."));
                    },
                },
                // From the second season, when the year has turned back: a chest at a Guild sale, marked by your own hand.
                new()
                {
                    Key = "gild.ownchest", Who = Marrow, Gap = 11, Tier = 2, If = s => s.SeasonsBefore > 0 && Story.TurnedBack(s.LastEnd),
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "seized", ShopTiers.Typical(s.Tier), s.Tier, rng.Next(2, 4));
                        chest.Tale = Story.Tale;
                        return Brought(s, rng, Marrow, chest, Loc.T("A lot from the Guild's foreclosure sale. Odd one: the lid's marked inside, in a " +
                                                                     "Keywright's hand. {price}."), "seized");
                    },
                    Bought = (s, v) =>
                    {
                        s.Find("own.chest");
                        s.Log(Story.Tale, Loc.T("A chest from a Guild sale was marked inside the lid in your own hand, dated this spring. You don't remember marking it."));
                    },
                },
            },
        },
    };
}
