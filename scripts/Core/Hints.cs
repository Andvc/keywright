using System.Collections.Generic;

namespace Keywright.Core;

/// <summary>
/// Uncle Bram's tips. Each one shows the first time its moment comes up (a sound, a customer, a tool, a renovation),
/// so the game teaches one thing at a time, when it's in front of you.
/// </summary>
public static class Hints
{
    public const string Mentor = "Uncle Bram";

    static readonly Dictionary<string, string> Text = new()
    {
        // The first day: one lesson per customer.
        ["intro"] = "Morning! Here's the trick of it: a chest opens for you once you've paid for it, and nobody can see inside, so " +
                    "everyone shakes it and guesses. Tam never even shook his. Drag the chest slowly from side to side: that's a " +
                    "gentle shake.",
        ["kin_hesk"] = "Your back won't take a hard shake or a drop, Hesk, but your scale doesn't lie. Weigh what you heard against " +
                       "what's on the scale: whatever's left over is something you couldn't hear, or something under a false bottom.",
        ["kin_rosa"] = "Everybody haggles with you, Rosa, though they open a bit higher, knowing you. Make your offer with the slider. " +
                       "Too low and they'll let you try again, but walk one out of the door and they won't be back this week.",
        ["kin_isolde"] = "Your shelf's up from the first day, Isolde. Keep what you love: visitors pay at the door every night, a coin " +
                         "for every point of exhibition. Just remember the Guild won't take your collection in pawn on a short night.",
        ["heard_Coins"] = "Hear that jingle? Coin. Light things rattle at the gentlest shake. Tick Coin on your slip, there on the right.",
        ["slip"] = "Your slip adds up what you've ticked, valued as good as the chest's colour promises. The bar underneath is your " +
                   "odds that it's worth more than the price: the greener, the safer.",
        ["first_penn"] = "Penn only shook it gently. Nothing heavy or soft stirs for a gentle shake, so she's priced only what she " +
                         "heard. Try every strength yourself: gentle, firm and hard.",
        ["heard_Garb"] = "That flump is a cloak. Soft things only make a sound when you shake hard.",
        ["first_hob"] = "Hob shook it hard, and when something clanks, it drowns out anything delicate. Shake it yourself, at every strength.",
        ["heard_Arms"] = "Clank: a weapon, an axe or a blade. It only moves when you shake firmly, and while it's clanking you can't " +
                         "hear anything delicate. Shake gently to hear past it.",
        ["first_mags"] = "Mags counts the rattles at a firm shake and charges the same for each. If you can't hear anything she " +
                         "didn't, pass: a seller always asks a bit more than what they've heard is worth.",
        ["passed"] = "When you pass, they take it home unopened, and you'll never know for sure. But your gut usually has an opinion.",
        ["opened"] = "A tick is a good read. A cross is something to listen for next time.",
        ["dayend"] = $"That's a day's trading. Every coin goes toward the stall: once it's earned {GameState.UpgradesAt}g, all told, " +
                     "we'll talk about Thornwick.",

        // From the second day: chest colours, the ways people trade, and their stories.
        ["colours"] = "Look at the chest's colour: plain wood, green, blue, purple. The Wardens painted them by what went in, so the " +
                      "colour tells you how good the things inside tend to be. Only tend to: you'll get surprises both ways.",
        ["quality"] = "See? Not everything matches its chest. Mostly it does; sometimes it's a little worse; now and then it's much better.",
        ["fixed"] = "This one names a price. Take it or leave it.",
        ["haggle"] = "This one haggles, so they'll start high. Use the slider to make one offer: they'll take it, name a final " +
                     "price, or storm off if you insult them.",
        ["offer"] = "This one wants you to name a price. Offer what it's worth to you, less a little. You get two tries.",
        ["try_again"] = "Too low. One more try, then they'll go.",
        ["lookup"] = "Everyone you've traded with has a page in your notebook: how they judge a chest, how they trade, what they're " +
                     "like. Click whoever's at the counter to read theirs.",
        ["story"] = "Listen to where they say it came from. A barrow holds grave goods, a goblin warren mostly coin, a dwarf-hold " +
                    "weapons and gems.",
        ["keep"] = "Something grand? You can keep it for display instead of selling it. A good exhibition brings more customers, " +
                   "and better ones. Enchanted things count double.",
        ["kept"] = "On display. You'll see it on the shelf behind the counter.",
        ["exhibition"] = "Your exhibition. Click anything in it to sell it, if you need the money.",

        // The shop moves inward, a step at a time.
        ["upgrades"] = $"{GameState.UpgradesAt}g earned: the stall pays its way! Thornwick Market has more customers, richer chests " +
                       "and room for a shelf. But Thornwick chests cost more, so keep enough for tomorrow's. A price in red means it " +
                       "would leave you short: better to wait a day.",
        ["stash"] = "Nearly broke? I keep a little put by for days like this. Don't tell the Guild. It won't last forever: the " +
                    "day card says how much is left.",
        ["display"] = "A shelf behind the counter, with room for one thing! Keep something grand on display instead of selling " +
                      "it: an exhibition draws more customers, and better chests. Each cabinet you buy makes room for one more.",
        ["synergy"] = $"Crownhold folk know a shop by what it collects. Your notebook has tallied everything you've acquired: " +
                      $"{Sellers.Number(SynergyDef.PerKind)} of a kind earns a synergy. Open Synergies, bottom right, to put one to work " +
                      "or see how close you are.",
        ["slots"] = "Another synergy slot: open Synergies and put one more to work.",
        ["emporium"] = "The Royal Emporium, in the palace's shadow. The Gate's just down the street: you can feel it hum. There are " +
                       "bigger things to grow into now: a night market, a patrons' book, a velvet case for curios. Keep enough back for Vane.",
        ["ledger"] = "Forgotten a sound, or someone's habits? Press N for your notebook: the sounds, the regulars you've met, and " +
                     "a calendar of what's coming.",
        ["debt"] = "That's the Guild's next due, up top. Click it any time for the whole debt in your notebook: what's paid, " +
                   "what's left, and what each week will want.",
        ["lockpick"] = "The ward's let go: the chest is yours. Now for the old lock underneath. The pick sweeps round: press Space, " +
                       "or click, while it's in the green to set a pin. Set every pin cleanly and nothing inside gets scratched.",

        // The run: the Guild's dues, curios, commissions, events.
        ["dues"] = "Paid. Next week the Guild wants more, and more again the week after. Whatever you spend on the shop has to " +
                   "earn its keep before Vane comes back.",
        ["peddler"] = "Nim sells curios: oddments from the Locking, each bending one rule of the trade. You've only so much room " +
                      "for them, so pick ones that work together. Nim notices what you buy and brings more like it, and keeps his " +
                      "rarest for a real collector. Don't ask what Nim is.",
        ["commissions"] = "The regulars want things now: see the board on the left. When something they want comes out of a chest, " +
                          "hand it over instead of selling it. A commission always pays more than a sale, and a lot pays piece by piece.",
        ["deliver"] = "Someone wants this! Deliver it: a commission always pays more than selling it would.",
        ["event"] = "Something's happening in town today: two days every week are different. It's up in the top bar (click it to " +
                    "read it again), and your notebook's calendar shows what's coming this week and next.",
        ["stranger"] = "The Stranger. That chest's wrapped in iron chains: nothing inside can move, so there's nothing to hear. " +
                       "Drop it to feel how heavy it is. Heavy ones hold more.",
        ["gilded"] = "A royal seal! That's one of King Aldric's own chests, gilded. The seller thinks the gold is paint, and priced " +
                     "it as the colour underneath.",
        ["payoff"] = "Look at that purse: enough to pay off the whole back-tithe, every week of it, right now! Do it, and the stall " +
                     "is ours early. Or keep trading first: the Guild only ever asks for this week's.",
        ["pawn"] = "Short? Pawn what's on display, or your curios, at the Guild's price. If even that isn't enough, the Guild seals the shop.",
        ["hood"] = "The Hooded Woman pays in shards of the Aldric Key. If you can't find what she wants this week, she'll ask again next week.",

        // Each move brings one new thing.
        ["tier2"] = "Thornwick folk bring two new sounds. Potions slosh at a gentle shake, like coin. Spells (scrolls and " +
                    "spellbooks) rustle only at a hard shake, like cloaks. And some chests are locked, or have a false bottom. " +
                    "It'll be a quiet first day, while word gets round.",
        ["locked"] = "Locked. Buy it and you'll pick the lock before you open it. A crowbar's quicker, but it breaks glass.",
        ["dropped"] = "Dropped! A solid thud means a plain chest. A hollow boom means a false bottom: something hidden that nobody's priced.",
        ["hollow"] = "Hear that boom? A false bottom. Tick it on your slip: whatever's down there, nobody's priced it.",
        ["broke"] = "Glass breaks when you drop it. If you heard a slosh, think twice before dropping.",
        ["heard_Potions"] = "That slosh is a potion. It's delicate, like coin: a clank drowns it out.",
        ["heard_Scrolls"] = "A rustle: a spell, a scroll or a spellbook. Soft, like a cloak: it only sounds at a hard shake.",
        ["tier3"] = "Crownhold brings gems, which tinkle at a gentle shake; archmages, whose things are often enchanted (a " +
                    "witchlight would show it); and lots with two or three of a kind. Count the rattles: click a row again for each one.",
        ["heard_Gems"] = "A tinkle: a gem. Delicate, and worth the most.",
        ["lamp"] = "The witchlight's lit, and it stays lit: anything enchanted glows when it makes a sound. Click the star on a " +
                   "row to mark one enchanted.",
        ["counted"] = "Two of a kind: each one adds its value. Sellers who don't count only price one.",
        ["glow"] = "It glowed! That one's enchanted, and worth twice as much. Click the star on its row to mark it.",
        ["tier4"] = "The Royal Emporium. Court folk, the Maw's creatures up through the cracks in the Gate, and Silas, who sells " +
                    "demon gold: gilded iron that jingles just like the real thing. A lodestone would catch it.",
        ["magnet"] = "The lodestone's on, so iron can't move. Weapons go quiet, and nothing drowns out the delicate things. Demon " +
                     "gold, which is iron under the gilt, goes quiet too.",
        ["shake_limit"] = "That's all the shaking this one will stand: it's coming apart. Go by what you heard.",

        // Heartsongs (Heartsongs.cs): about a third of one a run, in any chest.
        ["heard_Heartsong"] = "That sound... I've heard it once in forty years. A Heartsong! Only a Keywright hears one sing, so the " +
                              "seller has no idea it's there. It's worth nothing in gold and everything to the shop: buy that chest. " +
                              "And if you can't afford it, pass: a Heartsong won't leave with them.",
        ["song_found"] = "A Heartsong. Nobody would pay a penny for one, but each does something for the shop that no gold can, " +
                         "for as long as it sings here. Keep it or let it go: every one you keep makes the next harder to find.",
        ["song_shown"] = "Hear that? The whole stall's different with it singing. It'll sing as long as it's ours.",

        // A regular with nothing new to teach gets a tip the first time they come in after the first day; the rest have
        // a lesson (Lessons.cs). Bram's kin who come knowing the trade still get his word on each seller's terms.
        ["seller_penn"] = "Penn only shakes gently, so heavy things and soft things make no sound for her.",
        ["seller_dorrin"] = "Dorrin's Barrow chests are rotten: about two seconds of shaking, all told. One strength, then: pick it well.",
        ["seller_snikkit"] = "Snikkit puts his price up a tenth for every strength you try after the first. Pick two: gentle and hard.",
        ["seller_bramble"] = "Mother Bramble always dreams true, in signs. If you can't hear what she dreamt of, it's warded: name it.",
        ["seller_gerda"] = "A Thornwick dowry always holds coin, a cloak and a potion. Whichever of the three you can't hear is warded.",
        ["seller_wren"] = "Wren's tag lists every kind of thing packed. A kind on the tag that never makes a sound is warded: name it.",
        ["seller_quill"] = "Quill's ward-roll names the warded thing, faded. Make out the name, and tick its kind.",
        ["seller_oriel"] = "Madame Oriel's cards: a suit drawn twice is what the ward holds. Three different suits, and nothing's warded.",
        ["warded"] = "A warded thing makes no sound and weighs nothing. Name it on your slip, and nothing that isn't there, and the " +
                     "ward will give it up.",
        ["engine"] = "You could pay off the Guild now. Or buy its Bookkeeping Engine for the same sum: it reads every chest for you, " +
                     "to finish the run quickly.",
        ["seller_wenna"] = "Wenna's scale says what her chest weighs. Take away the weight of what you heard: what's left never " +
                           "stirred (a cloak is 3 lb, a spell 1).",
        ["seller_pell"] = "Pell's ticket counts every article, false bottom and all. Count what you hear: one short means one under the floor.",
        ["seller_ansel"] = "Brother Ansel won't have his chests shaken hard: half again if you do. Crypt chests are full of holy cloaks.",
        ["seller_fennick"] = "Fennick packs his chests in straw: everything needs a strength harder to stir, and cloaks and spells won't " +
                             "stir at all. Shake firmly, then hard, then drop it.",
        ["seller_orrin"] = "Orrin likes a wager: read his chest exactly, every kind and every count, and he gives back a third of the price.",
        ["seller_corisande"] = "Solve grandmother's riddle. If you can't hear what it's about, it's warded: name it.",
        ["seller_aldous"] = "Sir Aldous takes the best weapon in his chest, for the fee he names. Value what's left.",
        ["seller_vexis"] = "Vexis gives you twelve seconds. Gently, then hard, and decide.",
        ["seller_malbranche"] = "Malbranche is cheap, but the dearest thing inside is his. A chest of many middling things suits his bargain.",
        ["seller_skrae"] = "Skrae's echoes come back off everything, warded or not. An echo with nothing to hear is warded.",
        ["seller_ermine"] = "Lady Ermine prices only her gems; the rest comes free. Some are paste, and she won't have a lodestone near them.",
        ["seller_aldo"] = "Old Aldo's price is your slip. Get it exactly right and he takes a quarter off; get it wrong and you pay what " +
                          "it holds, and a quarter more.",
        ["seller_maw"] = "The Maw sells for one coin, but everyone who comes after him today asks twice as much. Count who's still waiting.",
    };

    public static string Get(string key) => Text.TryGetValue(key, out var text) ? text : null;
}
