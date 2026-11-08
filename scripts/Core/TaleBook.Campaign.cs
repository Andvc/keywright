using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// The campaign's beats at the counter and the back door (0.34, docs/narrative.md), drawn every season. Before you
/// remember: Garrick's grey chest on the second morning (the same every spring), and Nell Haddon's word about her father's
/// note. Once you remember: Bram's tuning fork in a Guild sale, Sergeant Hollis and the carts, the weighhouse, the crates
/// (Haddon's, Bram's), the first mimic, Mara and the seal, the charter (Master Quire's Roll, or the King), and Bram's
/// homecoming. Each comes once a season, at the shop where it belongs, and none of it is a roll.
/// </summary>
public static partial class TaleBook
{
    public const string Nell = "nell", Haddon = Campaign.Haddon;

    /// <summary>The first-morning chest: the same every spring, whatever the season's seed (Garrick's granddad's grey chest).</summary>
    public static Chest FirstChest() => new(new[]
    {
        new Item(Category.Coins, Rarity.Uncommon), new Item(Category.Garb, Rarity.Common), new Item(Category.Coins, Rarity.Common),
    }, Rarity.Uncommon) { Tale = Story.Tale };

    /// <summary>A Guild crate off the palace road, with someone gilded inside (they knock when it sits still).</summary>
    static Chest Crate(string person) => new(Array.Empty<Item>(), Rarity.Uncommon, lockTier: LockTiers.All[2]) { Crate = person, Tale = Story.Tale };

    /// <summary>A crate at a set price (nobody knows what's in it; the seller wants rid of it).</summary>
    static Visit CrateVisit(GameState s, Random rng, string seller, string person, string pitch, float purses) =>
        Market.Price(rng, Sellers.Get(seller), Crate(person), s.Tier, ask: s.Purses(purses), trade: Trade.Fixed, pitch: pitch, origin: Origins.Get("seized"));

    static IEnumerable<TaleDef> CampaignTales() => new[]
    {
        new TaleDef
        {
            Key = Story.Tale, Title = Loc.T("Bram's Account"), Place = TalePlace.Always,
            Blurb = Loc.T("Bram's twenty-year account with the Gilded Guild falls due this season. Keep it paid, and mind what the Guild wants " +
                          "with him: the notebook's Inquiry page keeps what you learn, and how the treasury key is coming."),
            Beats = new Beat[]
            {
                // The second morning, every spring: Garrick's grey chest, the same chest with the same things in it.
                new()
                {
                    Key = "first.chest", Who = Garrick, Gap = 2, If = s => s.Week == 1 && !s.CampaignDone,
                    Brings = (s, rng) => Market.Price(rng, Sellers.Get(Garrick), FirstChest(), s.Tier,
                        pitch: Loc.T("My granddad's grey chest, from the attic. I've meant to sell it for years. {price}, and it's yours."), origin: Origins.Get("attic")),
                    Bought = (s, v) => s.RecordFirstChest(v.Chest),
                },
                // Week 1: the Guild has Old Haddon's note, due on its first night.
                new()
                {
                    Key = "haddon.note", Who = Nell, Gap = 3, If = s => s.Week == 1 && !s.CampaignDone && s.Claims.ContainsKey(Haddon),
                    Says = s =>
                    {
                        var lines = Say(Nell,
                            Loc.T("The Guild's men were at the yard. Father signed their paper for a new lathe, and it falls due on their first night: forty gold."),
                            Loc.T("If it isn't paid they'll take the yard, and him with it, to work it off. He says it's only work. I don't like how they said it."));
                        // What an earlier spring showed you (the Inquiry keeps it).
                        if (s.KnownBefore.Contains("haddon.taken"))
                            lines.Add(new(Story.Self, Loc.T("I've seen this before: the Guild's carriage took him with Bram, and he never came home.")));
                        else if (s.KnownBefore.Contains("haddon.paid"))
                            lines.Add(new(Story.Self, Loc.T("I've paid it before. He made me a wheel I didn't need, and he was home all season.")));
                        return lines;
                    },
                    Asks = s => new()
                    {
                        C(Loc.T($"Pay his note ({Campaign.HaddonNote}g)"), () =>
                        {
                            s.Redeem(Haddon);
                            s.Flags.Add("haddon.redeemed");
                            s.Note("haddon.paid");
                            s.Sway(Faction.Commons, 1);
                            s.Log(Story.Tale, Loc.T("You paid Old Haddon's note to the Guild before its first night. He's staying home."));
                            return Loc.T("\"You'd do that? He'll pay you back in wheels. Whether you want them or not.\"");
                        }, W(s, -Campaign.HaddonNote, (Faction.Commons, 1)) + (s.Remembers ? s.Purses(0.3f) : 0), cost: Campaign.HaddonNote, kind: true),
                        C(Loc.T("I can't, not this week"), () =>
                        {
                            s.Log(Story.Tale, Loc.T("Nell Haddon told you the Guild has her father's note, due on its first night. You couldn't help."));
                            return Loc.T("\"No. Nobody can. I'll find it somewhere.\"");
                        }),
                    },
                    Then = s => s.Note("haddon.note"),
                },
                // Once you remember: Bram's tuning fork in a lot of the Guild's repossessed goods.
                new()
                {
                    Key = "bram.tool", Who = Marrow, Gap = 6, If = s => s.Remembers && s.BramNotes && !s.CampaignDone && s.Week <= 4,
                    Brings = (s, rng) =>
                    {
                        var chest = Fill(rng, "seized", ShopTiers.Typical(s.Tier), s.Tier, 2);
                        chest.Tale = Story.Tale;
                        return Brought(s, rng, Marrow, chest, Loc.T("A lot of the Guild's repossessed goods: tools, mostly, and a tuning fork with a tag on it. " +
                                                                    "Nobody's claimed them. {price}."), "seized");
                    },
                    Bought = (s, v) =>
                    {
                        s.Note("tool");
                        s.Log(Story.Tale, Loc.T("In a lot of the Guild's repossessed goods: Bram's tuning fork, tagged from the weighhouse on the palace road. Not the treasury."));
                    },
                },
                // At Thornwick: the Wardens' sergeant counts the Guild's carts, and will take you along.
                new()
                {
                    Key = "bram.carts", Who = Hollis, Gap = 6, Tier = 2,
                    If = s => s.Remembers && s.BramNotes && !s.CampaignDone && !s.Flagged("bribed") && !s.Noted.Contains("weighhouse"),
                    Says = s => s.KnownBefore.Contains("carts")
                        ? Say(Hollis, Loc.T("Sergeant Hollis, off duty. \"You've that look again, like you know what I'm going to say. The Guild's carts? " +
                                            "Every fifth night, down the palace road. Come to the toll, if you mean it.\""))
                        : Say(Hollis,
                            Loc.T("Sergeant Hollis, off duty and uneasy. \"Guild carts pass my toll every fifth night, under sacking. They don't stop at the " +
                                  "Guild Hall: they go on down the palace road.\""),
                            Loc.T("\"Last week a sack slipped. It was a man, Keywright. A gold man. Next Guild night, I'm following one. Come with me, if you've the stomach.\"")),
                    Asks = s => new()
                    {
                        C(Loc.T("Meet him at the toll on the Guild's night"), () =>
                        {
                            s.Flags.Add("follow.yes");
                            return Loc.T("\"Bring a dark coat. And don't rattle anything.\"");
                        }, s.Purses(0.5f), kind: true),
                        C(Loc.T("Not this season"), () => Loc.T("\"Suit yourself. The carts'll keep coming.\"")),
                    },
                    Then = s => s.Note("carts"),
                },
                // A crate that fell off a Guild cart: Old Haddon's, if the Guild took him.
                new()
                {
                    Key = "crate.haddon", Who = Corvin, Gap = 11, Tier = 2,
                    If = s => s.Remembers && s.IsTaken(Haddon) && !s.Guests.ContainsKey(Haddon) && (s.Knows("carts") || s.KnowsInventory) && !s.CampaignDone,
                    Brings = (s, rng) => CrateVisit(s, rng, Corvin, Haddon,
                        Loc.T("Fell off a Guild cart on the palace road. Heavy as sin, Guild seal and all, and nobody's asking after it. {price}."), 0.5f),
                    Bought = (s, v) => s.Log(Story.Tale, Loc.T("You bought a Guild crate that fell off a cart on the palace road.")),
                    Passed = (s, v) => s.Log(Story.Tale, Loc.T("Corvin had a Guild crate off the palace road. You let it go on to the treasury.")),
                },
                // Bram's crate: the Wardens stop a cart for a friend; otherwise Corvin, if the bargemen were paid, or late in the season.
                new()
                {
                    Key = "crate.bram.wardens", Who = Hollis, Gap = 16, Tier = 2,
                    If = s => s.Remembers && s.KnowsInventory && s.Friend(Faction.Wardens) && WaitingForBram(s),
                    Brings = (s, rng) =>
                    {
                        var visit = Given(s, rng, Hollis, Crate(Campaign.Bram),
                            Loc.T("The Wardens stopped a Guild cart at my toll. This crate's chalked with a name you'd know. It's yours, Keywright. One coin, for luck."), "seized");
                        return visit;
                    },
                },
                new()
                {
                    Key = "crate.bram", Who = Corvin, Gap = 16, Tier = 2,
                    If = s => s.Remembers && s.KnowsInventory && WaitingForBram(s) && (s.Flagged("crate.diverted") || s.Week >= 5),
                    Brings = (s, rng) => CrateVisit(s, rng, Corvin, Campaign.Bram,
                        Loc.T("A Guild crate off the palace road, chalked with a name. The bargemen say you were asking. {price}, and I never saw it."), 1f),
                },
                // A mimic, every spring once you remember (it's the same spring: the crypt gives up the same warm chest), and
                // more come on their own, from Thornwick.
                new()
                {
                    Key = "breath.first", Who = Cadoc, Gap = 11, Tier = 2, If = s => s.Remembers && s.Mimic == null && !s.CampaignDone,
                    Brings = (s, rng) =>
                    {
                        var chest = new Chest(Array.Empty<Item>(), ShopTiers.Typical(s.Tier), lockTier: LockTiers.For(ShopTiers.Typical(s.Tier))) { Breathing = true, Tale = Story.Tale };
                        return Brought(s, rng, Cadoc, chest, Loc.T("From the crypt. It's warm, Keywright. Warm like bread. The brothers want it gone. {price}."), "crypt");
                    },
                },
                // At Crownhold: Mara knows what the Keyhold put in the seal.
                new()
                {
                    Key = "seal.mara", Who = Story.Mara, Gap = 11, Tier = 3, If = s => s.Remembers && s.KnowsInventory && !s.Knows("seal") && !s.CampaignDone,
                    Says = s => Say(Story.Mara,
                        Loc.T("A woman in a grey hood, who looks at you a long time. \"You remember a spring that hasn't happened yet. I can see it on you.\""),
                        Loc.T("\"I'm Mara, the last of the Keyhold. We gave the seal a last clause: if the Maw ever breaks out without leave, the year turns back " +
                              "to its first morning.\""),
                        Loc.T("\"Whoever has touched the seal's core remembers. So does he. Nobody else.\""),
                        Loc.T("\"The Guild doesn't own that chest, Keywright. Find out what the Crown actually pledged. Master Quire keeps the Roll.\"")),
                    Then = s =>
                    {
                        s.Note("seal");
                        s.Met.Add(Story.Mara);
                        s.Log(Story.Tale, Loc.T("Mara of the Keyhold told you the seal's last clause: when the Maw breaks out without leave, the year turns back."));
                    },
                },
                // The charter: Master Quire's Roll, or the King himself.
                new()
                {
                    Key = "charter.quire", Who = Quire, Gap = 11, Tier = 3, If = s => s.Remembers && s.Knows("seal") && !s.Knows("charter") && !s.CampaignDone,
                    Says = s => Say(Quire,
                        Loc.T("Master Quire, the Archivist, with the Roll open on your counter. \"The treasury. Here: pledged by the Crown to the Gilded Guild, " +
                              "ten years since. Its keys, and its takings.\""),
                        Loc.T("\"Not its people. Not a keywright's word. The Crown never owned those to pledge. The Guild can let you in. It can't make you give.\"")),
                    Then = s =>
                    {
                        s.Note("charter");
                        s.Log(Story.Tale, Loc.T("Master Quire showed you the treasury's charter: the Crown pledged its keys and takings to the Guild, not its people, nor a keywright's word."));
                    },
                },
                new()
                {
                    Key = "charter.aldo", Who = Aldo, Gap = 16, Tier = 4, If = s => s.Remembers && s.Knows("seal") && !s.Knows("charter") && s.Day > s.TierSince && !s.CampaignDone,
                    Says = s => Say(Aldo,
                        Loc.T("Ten years ago there was a famine, and the Crown had nothing it could spend. So I pledged my treasury to the Guild: its keys, and its takings."),
                        Loc.T("Not my people, Keywright. Not your word. A king can't pledge what was never his.")),
                    Then = s =>
                    {
                        s.Note("charter");
                        s.Log(Story.Tale, Loc.T("Old Aldo told you what the Crown pledged to the Guild: the treasury's keys and takings, not its people, nor your word."));
                    },
                },
                // A copy of the weighhouse register, from the Roll (the Guild registers its crates like any chest).
                new()
                {
                    Key = "register.quire", Who = Quire, Gap = 16, Tier = 3,
                    If = s => s.Remembers && s.Knows("charter") && s.KnowsInventory && !s.Noted.Contains("register") && !s.CampaignDone,
                    Says = s => Say(Quire, Loc.T("\"The Guild registers its crates in the Roll like any chest. I could copy the weighhouse pages for you. The ink's dear.\"")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Have him copy them ({s.Purses(0.5f)}g)"), () =>
                        {
                            s.Note("register");
                            return Loc.T("\"Every crate, by name and weight, and the night it goes down. Burn it after.\"");
                        }, s.Purses(0.5f), cost: s.Purses(0.5f), kind: true),
                        C(Loc.T("Not now"), () => Loc.T("\"The Roll keeps.\"")),
                    },
                },
                // Once Bram's been brought home: Mara, on what the Maw is.
                new()
                {
                    Key = "mara.first", Who = Story.Mara, Night = true, Letter = true, Gap = 1, Tier = 2,
                    If = s => s.Remembers && (s.SavedBramBefore || s.BramHome) && s.Knows("seal") && !s.Knows("first") && !s.CampaignDone,
                    Says = s => Say(Story.Mara,
                        Loc.T("A note under the door, in Mara's hand: \"You asked what the Maw is. Before there were Locking chests there was one chest. It was empty, and it wanted.\""),
                        Loc.T("\"We didn't invent the Locking, Keywright. We copied it, and shut it in its own chest. It can't have enough. That's all it is.\"")),
                    Then = s => s.Note("first"),
                },
                // That Guild night, at the toll: Hollis and you follow the cart.
                new()
                {
                    Key = "bram.follow", Who = Hollis, Night = true, After = "bram.carts", Gap = 0, Tier = 2, If = s => s.Flagged("follow.yes") && s.DuesTonight,
                    Says = s => Say(Hollis,
                        Loc.T("The cart went down the palace road to a long, low hall that smelled of hot metal. Hollis called it the weighhouse."),
                        Loc.T("Crates in rows, each chalked with a name and a weight. \"H. Haddon, 14 stone.\" \"Bram, keywright, 13 stone.\" A clerk read them off a register."),
                        Loc.T("\"People,\" Hollis whispered. \"They're weighing people, Keywright.\" You copied the register's page before the lamps went out.")),
                    Then = s =>
                    {
                        s.GoOutTonight();
                        s.Note("weighhouse");
                        s.Note("register");
                        s.Log(Story.Tale, Loc.T("You followed a Guild cart with Sergeant Hollis to the weighhouse: crates, each with a name and a weight. Bram's among them."));
                    },
                },
                // Bram, freed and his pledge redeemed, comes home.
                new()
                {
                    Key = "bram.home", Who = Story.Bram, Night = true, Gap = 0, If = s => s.BramComesHome,
                    Says = s => Story.Homecoming(s),
                    Then = s =>
                    {
                        s.WelcomeBram();
                        s.Log(Story.Tale, Loc.T("Bram is home: freed from a Guild crate, his pledge redeemed. He told you what he pledged, twenty years ago."));
                    },
                },
            },
        },
    };

    /// <summary>Bram's still to be found this season: away, not home, his crate not yet bought, and the treasury not yet opened.</summary>
    static bool WaitingForBram(GameState s) => !s.CampaignDone && s.BramNotes && !s.Guests.ContainsKey(Campaign.Bram) && !s.Rescued.Contains(Campaign.Bram) &&
                                               !s.TreasuryDone && s.Week <= Guild.Weeks;
}
