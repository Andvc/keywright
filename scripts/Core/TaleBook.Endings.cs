using System.Linq;

namespace Keywright.Core;

/// <summary>
/// What became of everyone in 0.28's tales, for the season's end (<see cref="Story.SeasonEnd"/>): one line each, by what
/// you chose. Someone still around says it themselves; Bram tells the rest. Null for a tale that never got going.
/// </summary>
public static partial class TaleBook
{
    static Line MagpieEnding(GameState s)
    {
        string magpie = s.Secret("magpie");
        if (s.Flagged("magpie.caught"))
            return new(Hollis, Loc.T($"{Name(magpie)} was the Magpie, and she's in my cells. Every chest the Guild foreclosed on this year has gone home. Not a bad season, Keywright."));
        if (s.Flagged("magpie.wrong"))
        {
            string innocent = Suspects.FirstOrDefault(p => p != magpie && s.Gone.Contains(p)) ?? magpie;
            return new(Bram, Loc.T($"{Name(innocent)} spent the season in the Wardens' cells for the Magpie's thefts. The Magpie sends you a black feather now and then. I wouldn't keep them."));
        }
        if (s.Flagged("magpie.silent"))
            return new(Bram, Loc.T("The Magpie still steals from the Guild, and you know her face. She left a black feather on the counter last night. Friends in low places, eh?"));
        return s.Did("magpie.notice") ? new(Hollis, Loc.T("The Magpie's still out there, and the Guild's foreclosed chests still won't open. I'll have her one day.")) : null;
    }

    static Line PipEnding(GameState s)
    {
        if (s.Flagged(Apprentice) && !s.Gone.Contains(Pip))
            return new(Pip, Loc.T("I swept your counter all season, master, and kept my hand in my pocket. I can hear a coin through an inch of oak now. One day I'll have a stall like yours!"));
        if (s.Flagged("pip.named"))
            return new(Bram, Loc.T("Pip Hollow opens the Guild's chests in a counting-house in Crownhold now. Wynn left the mill. Nobody at Brindle says your name the way they used to."));
        if (s.Gone.Contains(Pip))
            return new(Bram, Loc.T("The Guild found Pip Hollow at the fair. Wynn went after him to Crownhold, and nobody's heard from either of them since."));
        return s.Did("pip.itch") ? new(Pip, Loc.T("I still fish the millpond, with my hand in my pocket. Nobody's looked at it yet. Mum says nobody will.")) : null;
    }

    static Line KettleEnding(GameState s)
    {
        // The Guild took him, to work off his note.
        if (s.IsTaken(Jory))
            return new(Bram, Loc.T("Jory Kettle went with the Guild's cart to work off his note. Ada takes bread to the Guild Hall every morning, and brings it home again."));
        if (s.Flagged(Porter))
            return new(Ada, s.Owed.ContainsKey(Ada) || s.CalledIn.Contains(Ada)
                ? Loc.T("Jory's home, and Rafe's sword is over the fire. The Kettles eat at their own table again, dear, and we owe it all to you.")
                : Loc.T("Jory's home, dear, and working the Barrow road with a cart. We eat at our own table again, and we owe that to you."));
        if (s.Did("kettle.debt"))
            return new(Bram, Loc.T("Jory Kettle's still in the Guild's debt-house. Ada goes up to Crownhold every week with bread, and comes back without it."));
        return s.Did("kettle.jory") ? new(Bram, Loc.T("Ada Kettle never found Rafe's sword, and Jory's still in the debt-house. Some seasons nobody wins.")) : null;
    }

    static Line DoorEnding(GameState s)
    {
        if (s.Did("door.hoard"))
            return new(Tobin, Loc.T("Half a hoard from behind the deep door, and I've never been down since. I've a farm now. Cows don't fall on you."));
        if (s.Flagged("door.rescued"))
            return new(Tobin, Loc.T("I limp now, and I laugh about it at the Crossed Keys. You paid to dig me out. I don't forget that, Keywright."));
        if (s.Flagged("door.staked"))
            return new(Bram, Loc.T("Nobody's been down to the Barrow's deep door since Tobin. Bettany keeps a candle lit in the window for him."));
        return s.Did("door.ask") ? new(Bram, Loc.T("Tobin found somebody else to stake him for the deep door. Or didn't. Nobody at the Crossed Keys will say.")) : null;
    }

    static Line WoodEnding(GameState s)
    {
        if (s.Flagged("wood.kept"))
            return new(Hawthorn, Loc.T("The oaks are standing, and will stand. Twice you refused them. The Wood remembers its friends, Keywright, for a very long time."));
        if (s.Flagged("wood.felled"))
            return new(Bram, Loc.T("The Guild's axes went into the Old Wood at the full moon. The druids don't come to market now, and the fair folk don't come at all."));
        if (s.Flagged("wood.sealed"))
            return new(Bram, Loc.T("Your mark's on the Guild's deed for the Old Wood. They've felled the edge of it. Mother Sorrel looks through you at market."));
        return s.Flagged("wood.refused") ? new(Bram, Loc.T("The Guild found another Keywright for the Wood's deed, or hasn't yet. The oaks are still standing, for now.")) : null;
    }

    static Line ParcelEnding(GameState s)
    {
        if (s.Flagged("parcel.given"))
            return new(Bram, Loc.T("Corvin's in the Wardens' cells, for a parcel you handed them. The Lockless won't forget that, and neither will he."));
        if (s.Did("parcel.collect"))
            return new(Corvin, Loc.T("I bought my boat, Keywright. If you ever need anything carried quietly down the Aldwater, ask for the Wet Hen."));
        if (s.Flagged("parcel.taken"))
            return new(Bram, Loc.T("The Wardens found Corvin's parcel in our back room and fined us for it. Corvin says it wasn't our fault. It was, a bit."));
        return null;
    }

    static Line ForeEnding(GameState s)
    {
        if (s.Flagged(Decree))
            return new(Aldo, Loc.T("By my decree, the Guild takes a fifth less from every Keywright in Aldmark. Master Quire says the Roll has never been so busy."));
        if (s.Flagged("fore.sold"))
            return new(Bram, Loc.T("You sold the Roll's copy to the Guild. Tobias Fenn never came back to the counting-house. Nobody's looked for him."));
        if (s.Flagged("fore.king"))
            return new(Quire, Loc.T("The Roll's copy is still waiting for the King's eyes. The Guild's seized chests still won't open. One day someone will listen."));
        return s.Did("fore.clerk") ? new(Bram, Loc.T("Half the chests the Guild forecloses on still won't open for anyone. Tobias Fenn still wonders why. So do I.")) : null;
    }

    static Line UnlockEnding(GameState s)
    {
        if (s.Flagged("unlock.broken"))
            return new(Ilse, Loc.T("Orme never rebuilt her engine. I'm an archmage's apprentice still, and the Locking holds. I can live with both."));
        if (s.Flagged(GateCracked))
            return new(Bram, Loc.T("Orme's engine opens one chest in three for anybody, and the Maw's gilt leaks into every purse. That's on all of us."));
        if (s.Gone.Contains(Orme))
            return new(Bram, Loc.T("The Guild took Archmage Orme away, engine and all. Ilse runs the Arcanum's stores alone, and won't look at you."));
        return s.Did("unlock.plate") ? new(Bram, Loc.T("Archmage Orme's still building something in the Arcanum's cellar. I'd keep my blood to myself.")) : null;
    }

    static Line CrownEnding(GameState s)
    {
        if (s.Flagged(CrownHome))
            return new(Durgan, Loc.T("The Ironroot Crown is home, Keywright, under the mountain where it was forged. Every dwarf in Aldmark knows your name now."));
        return s.Did("crown.ask") ? new(Durgan, Loc.T("The crown's still in the King's treasury, and the Guild still calls our gold contraband. One day.")) : null;
    }

    static Line VellEnding(GameState s)
    {
        if (s.Flagged("vell.given"))
            return new(Isaura, Loc.T("Cassian has stopped playing cards, mostly, and grandmother's boxes are mine. House Vell owes you, Keywright, and we pay our debts."));
        if (s.Flagged("vell.wardens"))
            return new(Isaura, Loc.T("A night in the Wardens' cells cured my brother of cards, for a month. That's a month more than anyone else managed."));
        return s.Did("vell.box") ? new(Bram, Loc.T("Lord Cassian still sells his sister's chests for stakes at cards. They still won't open for anyone who buys them.")) : null;
    }

    static Line DefectorEnding(GameState s)
    {
        if (s.Flagged(GazeKnown))
            return new(Bram, Loc.T("Skrae's somewhere quiet, it said, where nobody's heard of the Maw. I hope it's true. It was a good sort, for a demon."));
        if (s.Flagged("skrae.sold"))
            return new(Bram, Loc.T("Tick took Skrae back through the Gate. You were paid for it. I don't like to think about what the Maw does to runaways."));
        return s.Did("skrae.ask") ? new(Bram, Loc.T("Skrae still listens at the Gate, and still hates every word of it.")) : null;
    }
}
