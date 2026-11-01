using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// A tool of the trade, brought to the counter by someone who makes it (docs/stories.md): who, what they say, and what
/// they say to a yes and to a no. Turned down, they come back with it in a few days (<see cref="GameState.OfferAgain"/>).
/// </summary>
public sealed record OfferDef(string Upgrade, string Who, string Pitch, string Thanks, string Later);

public static class Offers
{
    public static readonly OfferDef[] All =
    {
        new("picks", "nettle",
            Loc.T("Goblin picks! Nettle's clan makes them, best in the hills: any lock opens easy as cheese. {price}, and you never forget Nettle."),
            Loc.T("Hehe! Shiny picks for shiny chests."), Loc.T("Pah. Nettle comes back. Nettle always comes back.")),
        new("scales", "hollis",
            Loc.T("The Wardens stamp honest weights on this market. Buyers pay more at a shop with Warden-stamped scales: {price}, stamp and all."),
            Loc.T("Stamped and sealed. Keep them honest, Keywright."), Loc.T("Suit yourself. The Wardens' stamp will keep.")),
        new("bench", "cadoc",
            Loc.T("The abbey's restorers mend relics three hundred years old. We'd lend you a bench and teach you the craft, for {price}: for the poor, you understand."),
            Loc.T("Bless you. Brother Anselm will come by to show you."), Loc.T("Another time, then. The bench will wait.")),
        new("lamp", "ilse",
            Loc.T("My mistress says the Arcanum has a witchlight going spare. Anything enchanted glows by it, and nobody else can see. {price}... she says."),
            Loc.T("Oh good! Don't let it go out: it's older than the Guild."), Loc.T("That's all right. I'll ask again when she sends me.")),
        new("block", "brass",
            Loc.T("Lantern Lane's auctions would take a Keywright's goods. An auction block in your shop, and buyers bid against each other: {price}."),
            Loc.T("Done. My cut is my own business."), Loc.T("As you like. Pledges keep, and so do offers.")),
        new("workshop", "durgan",
            Loc.T("My nephew's a journeyman restorer, and he's idle. A dwarf workshop at the back of your shop: {price}, and he works for his keep."),
            Loc.T("Ha! He'll mend things you didn't know were broken."), Loc.T("Hmph. Ask me again in a few days.")),
        new("magnet", "durgan",
            Loc.T("A lodestone from Ironroot's deep seam. Iron can't stir near it, and the Maw's demon gold is iron under the gilt. {price}. A dwarf knows iron."),
            Loc.T("Keep it away from your purse. Ha!"), Loc.T("It's been in the ground a thousand years. It'll wait.")),
        new("night", "gilt",
            Loc.T("My customers come after dark, friend. Keep your door open at night and they'll come to you too: {price}, and I'll send them round."),
            Loc.T("Wonderful. You'll never know they were mine."), Loc.T("Your loss, friend. The night's long.")),
        new("warrant", "aldo",
            Loc.T("A royal warrant. The King... owes me a favour. Hang it over your door and the court will buy from you: {price}, for the Crown's fee."),
            Loc.T("By appointment to the Crown. Don't let him down."), Loc.T("Another day, then. The King's patient.")),
        new("patrons", "zell",
            Loc.T("My clients ask the cards what they want, dear, and then they want it. For {price} I'll send them to you, two more commissions a week."),
            Loc.T("The cards said you would."), Loc.T("The cards said you might not. They'll say otherwise soon.")),
        new("master", "vey",
            Loc.T("My own restorer, from the palace workshops: the finest hands in Aldmark. I'll lend him to you for {price}. Appearances, you understand."),
            Loc.T("Charming. Do look after him."), Loc.T("How disappointing. I may ask again.")),
    };

    public static OfferDef For(string upgrade) => All.FirstOrDefault(o => o.Upgrade == upgrade);

    /// <summary>Whether this tool could be bought now (its tier, what it needs, not owned).</summary>
    public static bool Open(GameState state, Upgrade upgrade) => Upgrades.Open(state).Contains(upgrade);

    /// <summary>The tools whose makers would come to the counter today: open, not turned down lately, their maker still about.</summary>
    public static IEnumerable<Upgrade> Due(GameState state) =>
        Upgrades.Open(state).Where(u => u.Offered && For(u.Key) is OfferDef o && state.OfferDue(u.Key) && !state.Gone.Contains(o.Who) &&
                                        !state.Flagged($"away:{o.Who}"));

    /// <summary>Someone at the counter with a tool to sell: buy it, or not now.</summary>
    public static Talk Talk(GameState state, Upgrade upgrade)
    {
        var offer = For(upgrade.Key);
        int price = state.OfferPrice(upgrade);
        return new Talk
        {
            Who = offer.Who, Key = Key(upgrade.Key), Offer = upgrade.Key,
            Lines = { new Line(offer.Who, offer.Pitch.Replace("{price}", Money.Say(price))), new Line(Story.Bram, $"{upgrade.Name}: {upgrade.Blurb}") },
            Choices =
            {
                new Choice
                {
                    Label = Loc.T($"Buy it ({price}g)"), Cost = price,
                    Do = () =>
                    {
                        state.Grant(upgrade.Key);
                        return offer.Thanks;
                    },
                },
                new Choice
                {
                    Label = Loc.T("Not now"), Kind = true,
                    Do = () =>
                    {
                        state.Declined(upgrade.Key);
                        return offer.Later;
                    },
                },
            },
        };
    }

    public static string Key(string upgrade) => "offer:" + upgrade;
}
