using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>How an entry came to you: something you saw, something someone said, something you tried, or something you
/// now have reason to do.</summary>
public enum EntryWay { Seen, Said, Tried, Heard, Found, Lead }

/// <summary>
/// One line of the Inquiry (0.34, docs/narrative.md): a direct observation, a statement with who said it, an experiment
/// and what came of it, or a lead. Knowledge carries across seasons (the profile's <see cref="Profile.Known"/>); an
/// entry that's <see cref="Held"/> is something you hold this season (a paper, a copy, Bram's tuning fork) and goes at the
/// rewind. A lead shows while <see cref="Open"/> says so: something you've reason to do and haven't done this season.
/// </summary>
public sealed class Entry
{
    public string Key { get; init; }
    public string Thread { get; init; }
    public EntryWay Way { get; init; }
    /// <summary>Who said it (a person's key), for a statement.</summary>
    public string Who { get; init; }
    public Func<GameState, string> Text { get; init; }
    /// <summary>Held this season only: it doesn't carry over.</summary>
    public bool Held { get; init; }
    /// <summary>A lead's condition: shown while it's true.</summary>
    public Func<GameState, bool> Open { get; init; }
}

/// <summary>A question the Inquiry follows, and when it shows.</summary>
public sealed record Thread(string Key, string Ask, Func<GameState, bool> Shows);

public static class Inquiry
{
    public const string Bram = "bram", Breath = "breath", Morning = "morning", Plan = "plan"; // loc: keys

    /// <summary>The threads, in the order they open. The breathing chest and the morning are only asked once you remember.</summary>
    public static readonly Thread[] Threads =
    {
        new(Bram, Loc.T("Where is Bram?"), s => s.BramAway || s.Remembers),
        new(Breath, Loc.T("What was the breathing chest?"), s => s.Remembers),
        new(Morning, Loc.T("Why did the first morning come again?"), s => s.Remembers),
        new(Plan, Loc.T("How do we bring them out?"), s => s.Remembers && s.Knows("seal") && s.KnowsAlive),
    };

    static Entry E(string key, string thread, EntryWay way, string text, string who = null, bool held = false) =>
        new() { Key = key, Thread = thread, Way = way, Who = who, Text = _ => text, Held = held };

    static Entry Lead(string key, string thread, string text, Func<GameState, bool> open) =>
        new() { Key = key, Thread = thread, Way = EntryWay.Lead, Text = _ => text, Open = open };

    public static readonly Entry[] Entries =
    {
        // ---- Where is Bram? ----
        E("summons", Bram, EntryWay.Said, Loc.T("\"The Guild's treasury has asked for Bram. An old consignment needs his attention. Temporary work.\""), Story.Vane),
        E("commission", Bram, EntryWay.Said, Loc.T("\"Keep the shop running until he returns, and his account paid. Earn the treasury key, and bring it to the treasury on the season's last night.\""), Story.Vane),
        E("noletter", Bram, EntryWay.Seen, Loc.T("Bram said he'd write every week. No letter has come from him. The Guild's letters come.")),
        E("haddon.note", Bram, EntryWay.Said, Loc.T("\"The Guild has father's note, due on its first night. If it isn't paid they'll take the yard, and him, to work it off.\""), TaleBook.Nell),
        E("haddon.taken", Bram, EntryWay.Seen, Loc.T("The Guild took Old Haddon on its first night, to work off his note. Nell is minding the yard alone.")),
        E("haddon.paid", Bram, EntryWay.Tried, Loc.T("You paid Old Haddon's note before the Guild's night. He stayed home, and swore at the Guild from his yard.")),
        E("tool", Bram, EntryWay.Found, Loc.T("Bram's tuning fork, in a lot of the Guild's repossessed goods. Its tag: from the weighhouse, palace road. Not the treasury: the weighhouse."), held: true),
        E("carts", Bram, EntryWay.Said, Loc.T("\"Guild carts pass my toll every fifth night, under sacking, down the palace road. A sack slipped once. It was a man. A gold man.\""), TaleBook.Hollis),
        Lead("lead.follow", Bram, Loc.T("Follow a Guild cart from Thornwick's toll, on a Guild night. (Or buy a night at the weighhouse at the Lantern Market.)"),
            s => s.Knows("carts") && !s.KnowsInventory),
        E("weighhouse", Bram, EntryWay.Seen, Loc.T("Under the palace road, the Guild's weighhouse: crates in rows, each chalked with a name and a weight. \"H. Haddon, 14 stone.\" \"Bram, keywright, 13 stone.\"")),
        E("ledger", Bram, EntryWay.Found, Loc.T("A page of the Guild's ledger: \"H. Haddon, fourteen stone, to the weighhouse.\" People, written down by weight.")),
        E("register", Bram, EntryWay.Found, Loc.T("A copy of the weighhouse register: every crate by name and weight, and the night it goes to the treasury."), held: true),
        Lead("lead.listen", Bram, Loc.T("Listen to a Guild crate at the counter: let it sit still, and knock back if it knocks."),
            s => s.KnowsInventory && !s.KnowsAlive),
        E("alive", Bram, EntryWay.Tried, Loc.T("A Guild crate knocked when it sat still. You knocked back, and it answered. The gilded are alive.")),
        E("tap", Bram, EntryWay.Heard, Loc.T("A crate tapped twice, then once: the way Bram taps the counter. He's in there.")),
        Lead("lead.crate", Bram, Loc.T("Get Bram's crate off the palace road: Corvin buys what falls off a Guild cart, and the Lantern Market's bargemen will divert one."),
            s => s.KnowsInventory && s.BramAway && !s.Guests.ContainsKey(Campaign.Bram) && !s.BramHome && s.Week <= Guild.Weeks),
        Lead("lead.free", Bram, Loc.T("Free whoever's in the back room: a full mimic eats the gold off them, and their note must be redeemed or they'll be taken again."),
            s => s.Guests.Count > 0),
        new()
        {
            Key = "pledge", Thread = Bram, Way = EntryWay.Said, Who = Story.Bram,
            Text = s => s.Is(Characters.Kit)
                ? Loc.T("\"Twenty years ago the Mark-Finders came for you: a marked baby. I pledged them my hand, mark and all. That was my account.\"")
                : Loc.T("\"Twenty years ago the Mark-Finders came for a marked baby, my niece Kit. I pledged them my hand, mark and all. That was my account.\""),
        },
        E("home", Bram, EntryWay.Seen, Loc.T("Bram came home, cold to the bone, and sat by the stove, and laughed at nothing for an hour.")),

        // ---- What was the breathing chest? ----
        E("swallowed", Breath, EntryWay.Seen, Loc.T("The treasury's chest didn't sound. It didn't ring, not even the way an empty chest rings. It slipped, and it breathed. Then it opened.")),
        E("mimic", Breath, EntryWay.Heard, Loc.T("A chest at the counter breathed while it sat still. Opened, it was a mimic: a chest that had eaten what was in it.")),
        Lead("lead.mimic", Breath, Loc.T("Listen for a chest that breathes while it sits still. From Thornwick, one turns up now and then."),
            s => s.Knows("swallowed") && !s.Knows("mimic")),
        Lead("lead.feed", Breath, Loc.T("Keep a mimic in the back room and feed it on Guild nights, or leave it with the Arcanum's menagerie."),
            s => s.Knows("mimic") && !s.Knows("full")),
        E("full", Breath, EntryWay.Tried, Loc.T("Fed twice, the mimic ate its fill and fell asleep on the gold. Wanting something isn't wanting everything.")),
        E("shell", Breath, EntryWay.Tried, Loc.T("A full mimic ate the gold off someone the Guild had taken, and they stepped out of it, cold. Their note still stood: the Guild could take them again.")),
        E("first", Breath, EntryWay.Said, Loc.T("\"Before there were Locking chests there was one chest. It was empty, and it wanted. We didn't invent the Locking, Keywright. We copied it.\""), Story.Mara),

        // ---- Why did the first morning come again? ----
        E("again", Morning, EntryWay.Seen, Loc.T("You woke on the first morning. Bram tapped the counter, twice and once. He doesn't remember any of it. You do.")),
        E("vane.again", Morning, EntryWay.Said, Loc.T("\"We've met before, I think. You always look at my ring like that.\""), Story.Vane),
        E("seal", Morning, EntryWay.Said, Loc.T("\"The Keyhold gave the seal a last clause: if the Maw breaks out without leave, the year turns back to its first morning. Whoever touched the seal remembers. So does he.\""), Story.Mara),
        Lead("lead.seal", Morning, Loc.T("Someone at Crownhold knows what the Keyhold put in the seal. Find the hooded woman."),
            s => s.KnowsInventory && !s.Knows("seal")),
        E("owner", Morning, EntryWay.Tried, Loc.T("A stolen chest you sent home opened for its owner the moment she touched it. Paying for a chest doesn't make it yours: the ward decides.")),
        Lead("lead.charter", Morning, Loc.T("Find out what the Crown actually pledged: Master Quire keeps the Chest Roll at Crownhold, and the King keeps the charter."),
            s => s.Knows("seal") && !s.Knows("charter")),
        E("charter", Morning, EntryWay.Found, Loc.T("The treasury's charter: the Crown pledged its keys and its takings to the Guild. Not its people. Not a keywright's word.")),
    };

    public static Entry Get(string key) => Entries.FirstOrDefault(e => e.Key == key);

    public static string WayName(EntryWay way) => way switch
    {
        EntryWay.Said => Loc.T("said"),
        EntryWay.Tried => Loc.T("tried"),
        EntryWay.Heard => Loc.T("heard"),
        EntryWay.Found => Loc.T("found"),
        EntryWay.Lead => Loc.T("lead"),
        _ => Loc.T("seen"),
    };

    /// <summary>
    /// The plan's four lines (docs/narrative.md, "The true ending"), each with whether it's true this season and what it
    /// still needs.
    /// </summary>
    public static IEnumerable<(string Line, bool Done, string Needs)> PlanLines(GameState s)
    {
        yield return (Loc.T("Find them, and know them."), s.PlanFind,
            !s.KnowsAlive ? Loc.T("You need to hear the gilded for yourself.")
            : Loc.T("This season you need the weighhouse register: see it yourself, buy a copy at the Lantern Market, or copy it from Quire's Roll."));
        yield return (Loc.T("Free them."), s.PlanFree,
            !s.DebtCleared ? Loc.T("Bram's pledge is the Guild's account: pay it.") : Loc.T("A full mimic, in the back room or at the menagerie."));
        yield return (Loc.T("A way out, and people to take them in."), s.PlanWayOut,
            Loc.T("Two friends who'll stand with you, or the Wet Hen hired for the night (the Lantern Market)."));
        yield return (Loc.T("Open the door without giving it away."), s.PlanDoor,
            !s.KeyWhole ? Loc.T("The treasury key, whole.") : Loc.T("You need to know both the seal's clause and the charter."));
    }
}

/// <summary>The Inquiry's state in a season: what's been noted, and what was known when it began.</summary>
public partial class GameState
{
    /// <summary>What you knew when this season began (the profile's), carried over from earlier seasons.</summary>
    public HashSet<string> KnownBefore { get; } = new();

    /// <summary>What's been noted this season (held things included).</summary>
    public HashSet<string> Noted { get; } = new();

    /// <summary>Whether you know it: noted this season, or known from an earlier one (and not something you'd have to hold).</summary>
    public bool Knows(string key) => Noted.Contains(key) || KnownBefore.Contains(key);

    /// <summary>People are carted to the weighhouse as stock: seen there, or read in the Guild's ledger.</summary>
    public bool KnowsInventory => Knows("weighhouse") || Knows("ledger");

    /// <summary>The gilded are alive: you've heard one knock back (or Bram's tap).</summary>
    public bool KnowsAlive => Knows("alive") || Knows("tap");

    /// <summary>Entries just noted, for the screen to show one at a time.</summary>
    public Queue<Entry> NewEntries { get; } = new();

    /// <summary>Notes an entry. True the first time this season; it's news (shown) unless it was known already.</summary>
    public bool Note(string key)
    {
        var entry = Inquiry.Get(key);
        if (entry == null || !Noted.Add(key)) return false;
        bool shown = Inquiry.Threads.FirstOrDefault(t => t.Key == entry.Thread)?.Shows(this) ?? false;
        if (shown && (entry.Held || !KnownBefore.Contains(key))) NewEntries.Enqueue(entry);
        return true;
    }

    /// <summary>Old tales' clue keys, from before the Inquiry: the ones that still mean something are noted.</summary>
    public void Find(string key)
    {
        switch (key)
        {
            case "ledger":
                Note("ledger");
                break;
            case "roll.crown" or "quire.law":
                if (Remembers) Note("charter");
                break;
        }
    }

    /// <summary>What's in the Inquiry now: the entries noted or known, shown thread by thread (leads while they're open).</summary>
    public IEnumerable<Entry> Entries(string thread) => Inquiry.Entries.Where(e => e.Thread == thread &&
        (e.Way == EntryWay.Lead ? e.Open?.Invoke(this) == true : e.Held ? Noted.Contains(e.Key) : Knows(e.Key)));

    void SaveInquiry(SaveData data)
    {
        data.Noted = Noted.ToList();
        data.KnownBefore = KnownBefore.ToList();
    }

    void LoadInquiry(SaveData data)
    {
        Noted.UnionWith((data.Noted ?? new List<string>()).Where(k => Inquiry.Get(k) != null));
        KnownBefore.UnionWith((data.KnownBefore ?? new List<string>()).Where(k => Inquiry.Get(k) != null));
    }
}
