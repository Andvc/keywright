using System;
using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>
/// 0.30's Emporium tales (docs/stories.md): the King's court, where the papers the season turned up put the Guild on
/// trial; and the Maw's bargain, the debt paid for the Key.
/// </summary>
public static partial class TaleBook
{
    /// <summary>Papers the court needs to try the Guild; with fewer (but two at least), it decrees a fifth off.</summary>
    public const int PapersForTrial = 4, PapersForCourt = 2;

    static IEnumerable<TaleDef> EmporiumTales() => new[]
    {
        // The King's Court: every season, at the Emporium, if you've papers to bring it.
        new TaleDef
        {
            Key = "court", Title = Loc.T("The King's Court"), Place = TalePlace.Always,
            Blurb = Loc.T("The King has heard that the Emporium's Keywright has been collecting papers about his Guild. Bring him enough of them, " +
                          "and his court will try the Guild itself."),
            Ending = CourtEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "court.summons", Who = Aldo, Tier = 4, Gap = 0, If = s => s.Day > s.TierSince && s.Evidence.Count >= PapersForCourt,
                    Says = s => Say(Aldo,
                        Loc.T("They tell me the Emporium's Keywright has been collecting papers about my Guild. Ledgers. Registers. Old men's testimony."),
                        Loc.T("Bring all of it to my court, three days from now. If it's what I think it is, the Guild will answer for it.")),
                    Then = s =>
                    {
                        s.Flags.Add("court.summoned");
                        s.Promise(s.Day + 3, Aldo, Loc.T("The King's court"));
                        s.Log("court", Loc.T($"The King asked for your papers against the Guild, at his court in three days: {Words.List(s.Evidence.Select(Paper))}."));
                    },
                },
                new()
                {
                    Key = "court.bribe", Who = Story.Vane, Night = true, After = "court.summons", Gap = 1, Tier = 4,
                    Says = s => Say(Story.Vane,
                        Loc.T("Mister Vane at the back door, alone, which he never is. \"The Guild hears you've papers for the King. The Guild would " +
                              "much rather have them itself.\""),
                        Loc.T($"\"{Money.Say(s.Purses(2))}, tonight, and the Guild's friendship. Or keep them, and see how patient the Guild can be.\"")),
                    Asks = s => new()
                    {
                        C(Loc.T($"Sell them to the Guild ({s.Purses(2)}g)"), () =>
                        {
                            s.TaleGold(s.Purses(2));
                            s.Evidence.Clear();
                            s.Sway(Faction.Guild, 3);
                            s.Flags.Add("court.sold");
                            s.Kept(Aldo);
                            s.Log("court", Loc.T("You sold your papers against the Guild to Mister Vane, the night before the King's court."));
                            return Loc.T("\"A sensible Keywright. The Guild remembers its friends.\" He burns them in your grate before he goes.");
                        }, W(s, s.Purses(2), (Faction.Guild, 3)) - CourtWorth(s)),
                        C(Loc.T("They're for the King"), () =>
                        {
                            s.Sway(Faction.Guild, -2);
                            s.Log("court", Loc.T("Mister Vane offered to buy your papers against the Guild. You kept them for the King."));
                            return Loc.T("\"Then the Guild will see you in court, Keywright.\"");
                        }, W(s, CourtWorth(s), (Faction.Guild, -2)), kind: true),
                    },
                },
                new()
                {
                    Key = "court.trial", Who = Aldo, After = "court.bribe", Gap = 2, Tier = 4, If = s => !s.Flagged("court.sold"),
                    Expect = Loc.T("The King's court"),
                    Says = s => s.Evidence.Count >= PapersForTrial
                        ? Say(Aldo,
                            Loc.T($"My court has seen {Words.List(s.Evidence.Select(Paper))}. The Gilded Guild has robbed my subjects in my name, and fed " +
                                  $"the Maw with it."),
                            Loc.T("The Guild is on trial from today, and while it's tried, it takes half its tithe. Mister Vane is to be my guest, " +
                                  "in the palace cellars."))
                        : Say(Aldo,
                            Loc.T($"My court has seen {Words.List(s.Evidence.Select(Paper))}. It's not enough to try the Guild: they'd call it a " +
                                  $"Keywright's grudge, and my advisers would agree."),
                            s.Flagged(Decree)
                                ? Loc.T("I've cut their tithe once already. Bring me more, and I'll do more.")
                                : Loc.T("But it's enough for a decree: the Guild's tithe on Keywrights is cut by a fifth, from tonight.")),
                    Then = s =>
                    {
                        s.Kept(Aldo);
                        if (s.Evidence.Count >= PapersForTrial)
                        {
                            s.Flags.Add(Verdict);
                            s.Sway(Faction.Crown, 3);
                            s.Sway(Faction.Guild, -3);
                            s.Post("court", Loc.T("The Guild on trial"), Loc.T("By the King's court: the Gilded Guild is on trial, and takes half its tithe while it's tried. Aldric IX, Rex."));
                            s.Log("court", Loc.T("The King's court put the Gilded Guild on trial. It takes half its tithe while it's tried."));
                        }
                        else if (!s.Flagged(Decree))
                        {
                            s.Flags.Add(Decree);
                            s.Sway(Faction.Crown, 2);
                            s.Sway(Faction.Guild, -2);
                            s.Post("decree", Loc.T("By the King's decree"), Loc.T("The Gilded Guild's tithe on Keywrights is cut by a fifth. Aldric IX, Rex."));
                            s.Log("court", Loc.T("Your papers weren't enough for a trial, but the King cut the Guild's tithe by a fifth."));
                        }
                        else s.Log("court", Loc.T("Your papers weren't enough for a trial, and the King had already cut the Guild's tithe."));
                    },
                },
            },
        },

        // The Maw's Bargain: in the debt's last week, the Maw offers to pay it all, for the Key.
        new TaleDef
        {
            Key = "bargain", Title = Loc.T("The Maw's Bargain"), Place = TalePlace.Emporium,
            Blurb = Loc.T("In the debt's last week, the Maw himself comes to the counter. He'll pay everything you owe the Guild, tonight, for " +
                          "the shards of the Aldric Key."),
            Ending = BargainEnding,
            Beats = new Beat[]
            {
                new()
                {
                    Key = "bargain.offer", Who = Maw, Tier = 4, Gap = 0,
                    If = s => s.Week == Guild.Weeks && s.Day > s.TierSince && s.MapPieces > 0 && !s.MapGiven && !s.DebtCleared && !s.Gone.Contains(Maw),
                    Says = s => Say(Maw,
                        Loc.T("The air goes thick and sweet. A man in a borrowed body, dripping with rings, leans on your counter. \"Avarax. You know " +
                              "the name. Every coin you've ever touched has been my eye.\""),
                        Loc.T($"\"You owe my Guild {Money.Say(s.DebtLeft)}. I'll pay every coin of it tonight. All I want is the Key: the shards in " +
                              $"your strongbox. Such a small thing.\"")),
                    Asks = s => new()
                    {
                        C(Loc.T("Take his bargain"), () =>
                        {
                            s.TakeMawBargain();
                            s.Log("bargain", Loc.T("You gave the Maw the Aldric Key's shards, and he paid Bram's debt. Every coin."));
                            return Loc.T("\"Done.\" He doesn't take the shards. They're simply gone, and the rings on his fingers are warmer.");
                        }, BargainWorth(s)),
                        C(Loc.T("Never"), () =>
                        {
                            s.Flags.Add("bargain.refused");
                            s.Sway(Faction.Crown, 1);
                            s.Log("bargain", Loc.T("The Maw offered to pay the whole debt for the Key. You told him no, to his face."));
                            return Loc.T("\"Never is such a short word, for a mortal.\" The air clears. The sweetness stays a while.");
                        }, W(s, s.Purses(0.5f), (Faction.Crown, 1)), kind: true),
                    },
                },
                new()
                {
                    Key = "bargain.mara", Who = Story.Mara, Night = true, After = "bargain.offer", Gap = 0, Tier = 4, If = s => s.Flagged("bargain.refused"),
                    Says = s => Say(Story.Mara,
                        Loc.T("Mara at the back door, white to the lips. \"He came to you himself. And you told him no.\""),
                        Loc.T("\"Take this. It was the Keyhold's, and it's waited three hundred years for someone who'd say no to him.\"")),
                    Then = s =>
                    {
                        string gift = GiftCurio(s, new Random(unchecked(s.Seed * 61 + s.Day)), CurioRarity.Rare);
                        s.Log("bargain", Loc.T($"Mara gave you {gift}, for refusing the Maw."));
                    },
                },
            },
        },
    };

    /// <summary>
    /// What the court's worth to the careful player, if it sits: with four papers, half of what's left of the debt; with
    /// two or three, a fifth (unless the tithe's been cut already).
    /// </summary>
    static float CourtWorth(GameState s) => s.Evidence.Count >= PapersForTrial ? s.DebtLeft * GameState.Tried
        : s.Flagged(Decree) ? 0 : s.DebtLeft * (1 - GameState.Decreed);

    /// <summary>
    /// What the Maw's bargain is worth to the careful player: less than nothing, always. It pays the debt, but the run it
    /// saves is lost another way (the Key, the Crown Chest, Aldmark: no clear, and none of the debt's points). A learner
    /// may still take it, guessing.
    /// </summary>
    static float BargainWorth(GameState s) => -s.Purses(4) - s.DebtLeft;

    static Line CourtEnding(GameState s)
    {
        if (s.OnTrial)
            return new(Aldo, Loc.T("The Gilded Guild is on trial in my court, and Mister Vane is my guest in the palace cellars. It will be a long trial. I intend to enjoy it."));
        if (s.Flagged("court.sold"))
            return new(Bram, Loc.T("You sold the papers to Vane, the night before the King's court. He burned them in our grate. I can still smell it."));
        if (s.Flagged("court.summoned"))
            return new(Aldo, Loc.T("Your papers weren't enough to try the Guild this time. Keep collecting, Keywright. So will I."));
        return null;
    }

    static Line BargainEnding(GameState s)
    {
        if (s.GateOpens)
            return new(Story.Mara, Loc.T("You sold him the Key. The Gilded Gate stands open, and the Maw walks the palace halls in his borrowed body. The Locking won't hold another year."));
        return s.Flagged("bargain.refused")
            ? new(Story.Mara, Loc.T("He offered you everything you owed, and you said never. I've waited three hundred years to hear someone say that to him."))
            : null;
    }
}
