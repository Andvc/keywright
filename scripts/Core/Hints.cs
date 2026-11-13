using System.Collections.Generic;

namespace Keywright.Core;

/// <summary>
/// Uncle Bram's tips. Each one shows the first time its moment comes up (a sound, a customer, a tool, a renovation),
/// so the game teaches one thing at a time, when it's in front of you.
/// </summary>
public static class Hints
{
    /// <summary>Bram as the one minding the stall knows him: Kit's uncle, everyone else's partner (0.32).</summary>
    public static string Mentor => Kin == Characters.Kit ? Loc.T("Uncle Bram") : Loc.T("Bram");

    /// <summary>Who's minding the stall (set when a run starts or is continued), for how Bram is named.</summary>
    public static string Kin { get; set; } = Characters.Kit;

    static readonly Dictionary<string, string> Text = new()
    {
        // The first day: one lesson per customer.
        ["intro"] = Loc.T("Morning! Here's the trick of it: a chest opens for you once you've paid for it, and nobody can see inside, so " +
                    "everyone shakes it and guesses. Pip never even shook his. Drag the chest slowly from side to side: that's a " +
                    "gentle shake."),
        ["kin_hesk"] = Loc.T("Your back won't take a hard shake or a drop, Hesk, but your scale doesn't lie. Weigh what you heard against " +
                       "what's on the scale: whatever's left over is something you couldn't hear, or something under a false bottom."),
        ["kin_rosa"] = Loc.T("Everybody haggles with you, Rosa, though they open a bit higher, knowing you. Make your offer with the slider. " +
                       "Too low and they'll let you try again, but walk one out of the door and they won't be back this week."),
        ["kin_isolde"] = Loc.T($"Your shelf's up from the first day, Isolde. Keep what you love: visitors pay at the door every night, " +
                         $"{Characters.DoorCoins}g for every {Characters.DoorPoints} points of exhibition. Just remember the Guild won't take your collection in pawn on a short night."),
        ["heard_Coins"] = Loc.T("Hear that jingle? Coin. Light things rattle at the gentlest shake. Tick Coin on your slip, there on the right."),
        ["slip"] = Loc.T("Your slip adds up what you've ticked, valued as good as the chest's colour promises. The bar underneath is your " +
                   "odds that it's worth more than the price: the greener, the safer."),
        ["first_ada"] = Loc.T("Ada only shook it gently. Nothing heavy or soft stirs for a gentle shake, so she's priced only what she " +
                         "heard. Try every strength yourself: gentle, firm and hard."),
        ["heard_Garb"] = Loc.T("That flump is a cloak. Soft things only make a sound when you shake hard."),
        ["first_garrick"] = Loc.T("Garrick shook it hard, and when something clanks, it drowns out anything delicate. Shake it yourself, at every strength."),
        ["heard_Arms"] = Loc.T("Clank: a weapon, an axe or a blade. It only moves when you shake firmly, and while it's clanking you can't " +
                         "hear anything delicate. Shake gently to hear past it."),
        ["first_bettany"] = Loc.T("Bettany counts the rattles at a firm shake and charges the same for each. If you can't hear anything she " +
                         "didn't, pass: a seller always asks a bit more than what they've heard is worth."),
        ["passed"] = Loc.T("When you pass, they take it home unopened, and you'll never know for sure. But your gut usually has an opinion."),
        ["stolen"] = Loc.T("Your mark went cold: that chest isn't theirs to sell, and it won't open for whoever buys it. Pass on it, or send for " +
                     "the Wardens: they pay a finder's share."),
        ["tales"] = Loc.T("Not everyone comes to sell. People have their own troubles, and what you say to them comes back: who comes, what " +
                    "they bring, and who's on your side. The notebook's Town page keeps track."),
        ["offer"] = Loc.T("The best tools aren't sold in shops: the people who make them bring them to the counter. Say no, and they'll ask " +
                    "again in a few days."),
        ["opened"] = Loc.T("A tick is a good read. A cross is something to listen for next time."),
        ["unseen"] = Loc.T("You bought that one sight unseen, so there was nothing to read: a closed eye is a thing you never marked, " +
                           "not a mistake. Handle a chest before you buy it and your slip is checked as usual."),
        ["unheard"] = Loc.T("A closed eye is a thing you never listened for: you never shook the chest at the strength that stirs it, or never dropped it for what's under the floor. " +
                            "A cross is for what you listened to and got wrong. Gently, firmly, hard: every strength tells you something the others can't."),
        ["dayend"] = Loc.T("That's a day's trading. A week of them here, then Thornwick Market, a step further in: richer chests, and dearer ones. So let the purse grow."),

        // From the second day: chest colours, the ways people trade, and their stories.
        ["colours"] = Loc.T("Look at the chest's colour: plain wood, green, blue, purple. The Wardens painted them by what went in, so the " +
                      "colour tells you how good the things inside tend to be. Only tend to: you'll get surprises both ways."),
        ["quality"] = Loc.T("See? Not everything matches its chest. Mostly it does; sometimes it's a little worse; now and then it's much better."),
        ["fixed"] = Loc.T("This one names a price. Take it or leave it."),
        ["haggle"] = Loc.T("This one haggles, so they'll start high. Use the slider to make one offer: they'll take it, name a final " +
                     "price, or storm off if you insult them."),
        ["offer"] = Loc.T("This one wants you to name a price. Offer what it's worth to you, less a little. You get two tries."),
        ["try_again"] = Loc.T("Too low. One more try, then they'll go."),
        ["lookup"] = Loc.T("Everyone you've traded with has a page in your notebook: how they judge a chest, how they trade, what they're " +
                     "like. Click whoever's at the counter to read theirs."),
        ["horn"] = Loc.T("Missed what someone said? My old echo horn, at the end of the counter, keeps every word said across it. " +
                   "Click it to hear the lot again."),
        ["story"] = Loc.T("Listen to where they say it came from. A barrow holds grave goods, a goblin warren mostly coin, a dwarf-hold " +
                    "weapons and gems."),
        ["keep"] = Loc.T("Something grand? You can keep it for display instead of selling it. A good exhibition brings more customers, " +
                   "and better ones. Enchanted things count double."),
        ["kept"] = Loc.T("On display. You'll see it on the shelf behind the counter."),
        ["exhibition"] = Loc.T("Your exhibition. Click anything in it to sell it, if you need the money."),

        // The shop moves inward, a step at a time.
        ["upgrades"] = Loc.T("A proper shop, with room to grow: this is where you spend on it, and the people who make the best tools will bring them to the counter. Chests cost more here, so keep enough for tomorrow's. A price in red means it would leave you short: better to wait a day."),
        ["stash"] = Loc.T("Nearly broke? I keep a little put by for days like this. Don't tell the Guild. It won't last forever: the " +
                    "day card says how much is left."),
        ["display"] = Loc.T("A shelf behind the counter, with room for one thing! Keep something grand on display instead of selling " +
                      "it: an exhibition draws more customers, and better chests. Each cabinet you buy makes room for one more."),
        ["synergy"] = Loc.T($"Crownhold folk know a shop by what it collects. Your notebook has tallied everything you've acquired: {Sellers.Number(SynergyDef.PerKind)} of a kind earns a synergy. Open Synergies, bottom right, to put one to work or see how close you are."),
        ["slots"] = Loc.T("Another synergy slot: open Synergies and put one more to work."),
        ["emporium"] = Loc.T("The Royal Emporium, in the palace's shadow. The Gate's just down the street: you can feel it hum. There are " +
                       "bigger things to grow into now: a night market, a patrons' book, a velvet case for curios. Keep enough back for Vane."),
        ["ledger"] = Loc.T("Forgotten a sound, or someone's habits? Press N for your notebook: the sounds, the regulars you've met, and " +
                     "a calendar of what's coming."),
        ["debt"] = Loc.T("That's the Guild's next due, up top. Click it any time for the whole debt in your notebook: what's paid, " +
                   "what's left, and what each week will want."),
        ["lockpick"] = Loc.T("The chest is yours. Now for its old lock. The pick sweeps round: press {act}, or click, while it's in the " +
                       "green to set a pin; each pin you set sends it back the other way. Set every pin cleanly and nothing inside gets scratched."),

        // The run: the Guild's dues, curios, commissions, events.
        ["dues"] = Loc.T("Paid. Next week the Guild wants more, and more again the week after. Whatever you spend on the shop has to " +
                   "earn its keep before Vane comes back."),
        ["term_sightunseen"] = Loc.T("That curio pays on chests bought sight unseen: bought without shaking, dropping or putting the lodestone " +
                               "to them. The witchlight's fine. A '?' on a card means it uses a word like that: hover over it to read it."),
        ["term_bramsword"] = Loc.T("Bram's word is me vouching for you on a Guild night you can't pay: the Guild takes all your gold, and " +
                             "what's still owed, half as much again, goes on next week's due. Once a run, and never in the last week."),
        ["peddler"] = Loc.T("Nim comes every night after closing with curios: oddments from the Locking, each bending one rule of the " +
                      "trade. One a night at first; his lantern brings more. Pick ones that work together: Nim notices what you buy and " +
                      "brings more like it. Don't ask what Nim is."),
        ["commissions"] = Loc.T("The regulars want things now: see the board on the left. When something they want comes out of a chest, " +
                          "hand it over instead of selling it. A commission always pays more than a sale, and a lot pays piece by piece."),
        ["deliver"] = Loc.T("Someone wants this! Deliver it: a commission always pays more than selling it would."),
        ["event"] = Loc.T("Something's happening in town today: two days every week are different. It's up in the top bar (click it to " +
                    "read it again), and your notebook's calendar shows what's coming this week and next."),
        ["stranger"] = Loc.T("The Stranger. That chest's wrapped in iron chains: nothing inside can move, so there's nothing to hear. " +
                       "Drop it to feel how heavy it is. Heavy ones hold more."),
        ["gilded"] = Loc.T("A royal seal! That's one of King Aldric's own chests, gilded. The seller thinks the gold is paint, and priced " +
                     "it as the colour underneath."),
        ["payoff"] = Loc.T("Look at that purse: enough to pay off the whole back-tithe, every week of it, right now! Do it, and the stall " +
                     "is ours early. Or keep trading first: the Guild only ever asks for this week's."),
        ["pawn"] = Loc.T("Short? Pawn what's on display, or your curios, at the Guild's price. If even that isn't enough, the Guild seals the shop."),
        ["favour"] = Loc.T("Short, and folk owe you a favour? Call on them first: each brings an eighth of the due, once a season. That's what friends are for."),
        ["gold"] = Loc.T("Gold! A legend, that. They're made, not mended: no restorer or lucky charm will ever turn a thing gold. It went into that chest gold."),

        // Each move brings one new thing.
        ["tier2"] = Loc.T("Thornwick folk bring two new sounds. Potions slosh at a gentle shake, like coin. Spells (scrolls and " +
                    "spellbooks) rustle only at a hard shake, like cloaks. And some chests are locked, or have a false bottom. " +
                    "It'll be a quiet first day, while word gets round."),
        ["locked"] = Loc.T("Locked. Buy it and you'll pick the lock before you open it. A crowbar's quicker, but it breaks glass."),
        ["dropped"] = Loc.T("Dropped! A solid thud means a plain chest. A hollow boom means a false bottom: something hidden that nobody's priced."),
        ["hollow"] = Loc.T("Hear that boom? A false bottom. Tick it on your slip: whatever's down there, nobody's priced it."),
        ["broke"] = Loc.T("Glass breaks when you drop it. If you heard a slosh, think twice before dropping."),
        ["heard_Potions"] = Loc.T("That slosh is a potion. It's delicate, like coin: a clank drowns it out."),
        ["heard_Scrolls"] = Loc.T("A rustle: a spell, a scroll or a spellbook. Soft, like a cloak: it only sounds at a hard shake."),
        ["tier3"] = Loc.T("Crownhold brings gems, which tinkle at a gentle shake; archmages, whose things are often enchanted (a " +
                    "witchlight would show it); and lots with two or three of a kind. Count the rattles: click a row again for each one."),
        ["heard_Gems"] = Loc.T("A tinkle: a gem. Delicate, and worth the most."),
        ["lamp"] = Loc.T("The witchlight's lit, and it stays lit: anything enchanted glows when it makes a sound. Click the star on a " +
                   "row to mark one enchanted."),
        ["counted"] = Loc.T("Two of a kind: each one adds its value. Sellers who don't count only price one. A click counts one more, a right-click one fewer."),
        ["glow"] = Loc.T("It glowed! That one's enchanted, and worth twice as much. Click the star on its row to mark it."),
        // 0.33: the Lantern Market (0.34: it sells a way in, a way out, a crate off a cart; never an answer).
        ["market"] = Loc.T("The Lantern Market's open under the Barrow tonight, and Nim will take you down. His people sell what can't be " +
                           "bought by daylight, and buy what nobody else will. Mind who you sell gold to down there."),
        // 0.33: a mimic (0.34: a full one sleeps on the gold, and will eat the gold off someone gilded).
        ["mimic"] = Loc.T("A mimic! It's eaten whatever was in there. The Arcanum's menagerie pays well for one. Or keep it: it eats gold on " +
                          "Guild nights, no thief tries a door with a mimic behind it, and once it's eaten its fill it sleeps on the gold."),
        ["breathing"] = Loc.T("Hush. Hear that? Something in there is breathing. Where the Locking frays, an old ward goes hungry and eats what's in " +
                              "its chest: a mimic. Nothing rattles, so the seller thinks it's empty. The Arcanum's menagerie pays well for one."),
        ["tier4"] = Loc.T("The Royal Emporium. Court folk, the Maw's creatures up through the cracks in the Gate, and Gilt, who sells " +
                    "demon gold: gilded iron that jingles just like the real thing. A lodestone would catch it."),
        ["magnet"] = Loc.T("The lodestone's on, so iron can't move. Weapons go quiet, and nothing drowns out the delicate things. Demon " +
                     "gold, which is iron under the gilt, goes quiet too."),
        ["shake_limit"] = Loc.T("That's all the shaking this one will stand: it's coming apart. Go by what you heard."),
        // A season after the first (0.37): the trade's ways as far as any season has met them, from the first morning.
        ["ways"] = Loc.T("You've traded further in than this, and it shows in what comes to the counter: locks, false bottoms, two of a kind, " +
                         "enchantment. Whatever an earlier season met, this one has from its first morning."),
        // Bram's cradle (0.37): the shaking done for you, once the stall's week has taught what the strengths tell.
        ["cradle"] = Loc.T("My old cradle's on the counter. Press \"Rock it\" (or {act}) and it shakes the chest for you, gently, firmly and hard: you just listen. " +
                           "It won't rock where shaking costs something, and a drop is still your own doing."),

        // Heartsongs (Heartsongs.cs): about a third of one a run, in any chest.
        ["heard_Heartsong"] = Loc.T("That sound... I've heard it once in forty years. A Heartsong! Only a Keywright hears one sing, so the " +
                              "seller has no idea it's there. It's worth nothing in gold and everything to the shop: buy that chest. " +
                              "And if you can't afford it, pass: a Heartsong won't leave with them."),
        ["song_found"] = Loc.T("A Heartsong. Nobody would pay a penny for one, but each does something for the shop that no gold can, " +
                         "for as long as it sings here. Keep it or let it go: every one you keep makes the next harder to find."),
        ["song_shown"] = Loc.T("Hear that? The whole stall's different with it singing. It'll sing as long as it's ours."),
        ["songs_intro"] = Loc.T("One more thing. Somewhere among all these chests are nine Heartsongs: things loved so much they sing. Only " +
                          "a Keywright hears them, at the gentlest shake. Most seasons never hear one. If you ever do, buy that chest."),

        // A regular with nothing new to teach gets a tip the first time they come in after the first day; the rest have
        // a lesson (Lessons.cs). Bram's kin who come knowing the trade still get his word on each seller's terms.
        ["seller_ada"] = Loc.T("Ada only shakes gently, so heavy things and soft things make no sound for her."),
        ["seller_tobin"] = Loc.T("Tobin's Barrow chests are rotten: about two seconds of shaking, all told. One strength, then: pick it well."),
        ["seller_nettle"] = Loc.T("Nettle puts her price up a tenth for every strength you try after the first. Pick two: gentle and hard."),
        ["seller_sorrel"] = Loc.T("Mother Sorrel always dreams true, in signs. If you can't hear what she dreamt of, it's warded: name it."),
        ["seller_hesper"] = Loc.T("A Thornwick dowry always holds coin, a cloak and a potion. Whichever of the three you can't hear is warded."),
        ["seller_ilse"] = Loc.T("Ilse's tag lists every kind of thing packed. A kind on the tag that never makes a sound is warded: name it."),
        ["seller_quire"] = Loc.T("Quire's ward-roll names the warded thing, faded. Make out the name, and tick its kind."),
        ["seller_zell"] = Loc.T("Madame Zell's cards: a suit drawn twice is what the ward holds. Three different suits, and nothing's warded."),
        ["warded"] = Loc.T("A warded thing makes no sound and weighs nothing. Name it on your slip, and nothing that isn't there, and the " +
                     "ward will give it up."),
        ["engine"] = Loc.T("You could pay off the Guild now. Or buy its Bookkeeping Engine for the same sum: it reads every chest for you, " +
                     "to finish the run quickly."),
        ["seller_rook"] = Loc.T("Rook's scale says what her chest weighs. Take away the weight of what you heard: what's left never " +
                           "stirred (a cloak is 3 lb, a spell 1)."),
        ["seller_brass"] = Loc.T("Brass's ticket counts every article, false bottom and all. Count what you hear: one short means one under the floor."),
        ["seller_flick"] = Loc.T("Flick's chests are mostly stolen. Lay a hand on each one: if your mark goes cold, send for the Wardens, or pass."),
        // 0.30's officials, who come while their masters count you a friend.
        ["seller_marrow"] = Loc.T("The Guild's bailiff, selling what it took for tithe to its friends: cheap, by the colour alone. But a lot of what the " +
                             "Guild takes was never the Guild's to sell. Lay a hand on every one."),
        ["seller_brandt"] = Loc.T("The Wardens' quartermaster sells what nobody claimed, by the pound. A heavy chest comes dear and a light one cheap: " +
                             "take the weight of what you hear from his scale's."),
        ["seller_cadoc"] = Loc.T("Brother Cadoc won't have his chests shaken hard: half again if you do. Crypt chests are full of holy cloaks."),
        ["seller_corvin"] = Loc.T("Corvin packs his chests in straw: everything needs a strength harder to stir, and cloaks and spells won't " +
                             "stir at all. Shake firmly, then hard, then drop it."),
        ["seller_orme"] = Loc.T("Orme likes a wager: read her chest exactly, every kind and every count, and she gives back a third of the price."),
        ["seller_isaura"] = Loc.T("Solve grandmother's riddle. If you can't hear what it's about, it's warded: name it."),
        ["seller_rowan"] = Loc.T("Sir Rowan takes the best weapon in his chest, for the fee he names. Value what's left."),
        ["seller_tick"] = Loc.T("Tick gives you twelve seconds. Gently, then hard, and decide."),
        ["seller_sallow"] = Loc.T("Sallow is cheap, but the dearest thing inside is his. A chest of many middling things suits his bargain."),
        ["seller_skrae"] = Loc.T("Skrae's echoes come back off everything, warded or not. An echo with nothing to hear is warded."),
        ["seller_vey"] = Loc.T("Countess Vey prices only her gems; the rest comes free. Some are paste, and she won't have a lodestone near them."),
        ["seller_aldo"] = Loc.T("Old Aldo's price is your slip. Get it exactly right and he takes a quarter off; get it wrong and you pay what " +
                          "it holds, and a quarter more."),
        ["seller_maw"] = Loc.T("The Maw sells for one coin, but everyone who comes after him today asks twice as much. Count who's still waiting."),
    };

    public static string Get(string key) => Text.TryGetValue(key, out var text) ? text : null;
}
