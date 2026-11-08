using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>
/// The end of a run: Bram's debt paid (keep trading against the Guild's endless interest, or retire), or the shop
/// foreclosed. Either way, how the run went, and your best across every run.
/// </summary>
public partial class RunEndPhase : Phase
{
    readonly bool _cleared;

    public RunEndPhase(bool cleared)
    {
        _cleared = cleared;
    }

    public RunEndPhase() { }

    public bool Cleared => _cleared;

    public override void _Ready()
    {
        Add(UiKit.MakeShop(State.Tier, _cleared ? 0.65f : 0.4f, ShopHour.Evening, State));
        var color = _cleared ? Palette.Gold : Palette.Bad;
        string title = _cleared
            ? State.TreasuryEnd == Campaign.Morning ? Loc.T("THE NEXT DAY")
            : State.TreasuryEnd == Campaign.Stayed ? Loc.T("THE TREASURY OPENS WITHOUT YOU")
            : Loc.T("FREE AND CLEAR")
            : Loc.T("FORECLOSED");
        var logo = Add(UiKit.MakeLabel(title, 64, color, HorizontalAlignment.Center, outline: 8).Place(0, 80, 1280, 80));
        logo.PivotOffset = new Vector2(640, 40);
        logo.Scale = Vector2.One * 1.3f;
        logo.CreateTween().TweenProperty(logo, "scale", Vector2.One, 0.4f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        int early = State.ClearedWeek > 0 ? Guild.Weeks - State.ClearedWeek : 0;
        string paid = early > 0 ? Loc.Plural(early, $"Bram's debt is paid, {early} week early", $"Bram's debt is paid, {early} weeks early")
            : Loc.T("Bram's debt is paid");
        string sub = _cleared
            ? State.TreasuryEnd == Campaign.Morning ? Loc.T($"{paid}, everyone brought out of the treasury, and a morning the season has never reached.")
            : State.TreasuryEnd == Campaign.Stayed ? Loc.T($"{paid}. You stayed away, and the Guild opened the treasury with Bram.")
            : State.CampaignDone ? Loc.T($"{paid}, another year.")
            : State.Week < Guild.Weeks && !State.KeyWhole ? Loc.T($"{paid}. Earn the rest of the treasury key before the season's last night.")
            : State.BramHome ? Loc.T($"{paid}, and Bram is home.")
            : Loc.T($"{paid}, and Bram never came home.")
            : State.DebtCleared ? Loc.T($"Bram's debt was paid, but the Guild's interest caught up with you in week {State.Week}.")
            : Loc.T($"The Guild took the shop in week {State.Week}.");
        // One line under the title: smaller when the words (a paid-early debt and what came of the Key) won't fit at 24.
        Add(UiKit.MakeLabel(sub, UiKit.TextWidth(sub, 24) <= 1240 ? 24 : 16, Palette.Text, HorizontalAlignment.Center, outline: 4).Place(0, 168, 1280, 30));

        var panel = Add(UiKit.MakePanel(color.Darkened(0.3f)).Place(90, 220, 540, 300));
        var rows = new List<(string, string)>
        {
            (Loc.T("Debt cleared"), State.ClearedDay > 0 ? Loc.T($"day {State.ClearedDay}, week {State.ClearedWeek}") : Loc.T("no")),
            (Loc.T("Days of trading"), $"{State.Day}"),
            (Loc.T("The shop"), ShopTiers.Name(State.Tier)),
            (Loc.T("The chests brought in"), Money.Short(State.TotalEarned)),
            (Loc.T("Gold in hand"), Money.Short(State.Gold)),
            (Loc.T("Collection"), Loc.T($"{State.Found.Count} of {Collection.Size}")),
            (Loc.T("The treasury key"), Loc.T($"{State.Pieces.Count} of {Campaign.Pieces.Length} pieces")),
            (Loc.T("Curios"), $"{State.CurioKeys.Count}"),
            // The season's people: how many of its tales got going, and who counts you a friend.
            (Loc.T("Tales begun"), Loc.T($"{State.TaleLog.Count} of {State.TalesDrawn.Count}")),
            (Loc.T("Friends"), Words.List(Factions.All.Where(State.Friend).Select(Factions.Name).DefaultIfEmpty(Loc.T("none")))),
        };
        // The labels as wide as the longest needs (a language can run long), the values right-aligned in what's left.
        float labelW = Mathf.Clamp(rows.Max(r => UiKit.TextWidth(r.Item1)) + 8, 200, 300);
        for (int i = 0; i < rows.Count; i++)
        {
            var (label, value) = rows[i];
            panel.AddChild(UiKit.MakeLabel(label, 16, Palette.Muted).Place(32, 18 + i * 28, labelW, 20));
            var text = UiKit.MakeLabel(value, 16, Palette.Text, HorizontalAlignment.Right).Place(32 + labelW, 18 + i * 28, 476 - labelW, 20);
            text.ClipText = true;
            text.TooltipText = value;
            text.MouseFilter = Control.MouseFilterEnum.Pass;
            panel.AddChild(text);
        }
        BuildScore(color);
        Add(UiKit.MakeLabel(News(), 16, Palette.Good, HorizontalAlignment.Center, outline: 4).Place(0, 532, 1280, 20));

        // Once the treasury's been opened the season's story is over: no trading on into the Guild's interest.
        if (_cleared && State.TreasuryDone)
        {
            Add(UiKit.MakeButton(Loc.T("Retire"), Palette.Gold, 24, Retire).Place(495, 580, 290, 64)).TooltipText = Loc.T("Hang up your apron: back to the title.");
            Sfx.Play("fanfare", 1f, -2);
            Fx.Confetti(this, new Vector2(640, 120), 160);
        }
        else if (_cleared)
        {
            Add(UiKit.MakeButton(Loc.T("Keep trading"), Palette.Gold, 24, KeepTrading).Place(340, 580, 290, 64)).TooltipText =
                State.Week < Guild.Weeks
                    ? Loc.T($"Nobody's coming for anything until week {Guild.Weeks + 1}: then the Guild wants interest, half as much again each week.")
                    : Loc.T("The Guild keeps coming, for half as much again each week. How long can you last?");
            Add(UiKit.MakeButton(Loc.T("Retire"), Palette.Text, 24, Retire).Place(650, 580, 290, 64)).TooltipText = Loc.T("Hang up your apron: back to the title.");
            Sfx.Play("fanfare", 1f, -2);
            Fx.Confetti(this, new Vector2(640, 120), 160);
        }
        else
        {
            Add(UiKit.MakeButton(Loc.T("Back to the title"), Palette.Gold, 24, Retire).Place(495, 580, 290, 64));
            Sfx.Play("lose", 0.8f, -2);
        }
        Game.Moment(_cleared ? "23_cleared" : "23_foreclosed", 0.8);
    }

    /// <summary>
    /// The run's score, line by line, and times this challenge level's multiplier: noted as the character's best when the
    /// run ends (a cleared run that keeps trading adds to it every week it keeps the Guild paid).
    /// </summary>
    void BuildScore(Color color)
    {
        var panel = Add(UiKit.MakePanel(Palette.Magic.Darkened(0.3f)).Place(650, 220, 540, 300));
        panel.AddChild(UiKit.MakeLabel(Loc.T($"{State.Kin.Name.ToUpperInvariant()}'S SCORE{(State.Challenge > 0 ? Loc.T($", LEVEL {State.Challenge}") : "")}"), 16,
            Palette.Magic).Place(32, 20, 480, 20));
        var lines = Score.Lines(State);
        for (int i = 0; i < lines.Count; i++)
        {
            var (label, points) = lines[i];
            panel.AddChild(UiKit.MakeLabel(label, 16, Palette.Muted).Place(32, 50 + i * 26, 380, 20));
            panel.AddChild(UiKit.MakeLabel($"{points}", 16, Palette.Text, HorizontalAlignment.Right).Place(400, 50 + i * 26, 108, 20));
        }
        int total = Score.Total(State), final = Score.Final(State);
        string times = State.Challenge > 0 ? Loc.T($"{total} x{Challenges.Multiplier(State.Challenge):0.##} = ") : "";
        int best = Game.Profile?.BestScore.GetValueOrDefault(State.Character) ?? 0;
        string scored = final > best ? Loc.T("Score: a new best!") : Loc.T("Score");
        panel.AddChild(UiKit.MakeLabel(scored, UiKit.TextWidth(scored, 24) <= 256 ? 24 : 16, Palette.Gold, outline: 4).Place(32, 254, 260, 30));
        panel.AddChild(UiKit.MakeLabel($"{times}{final}", 24, Palette.Gold, HorizontalAlignment.Right, outline: 4).Place(200, 254, 308, 30));
    }

    /// <summary>What this run opened up: another of Bram's kin, the next challenge level; or the record across runs.</summary>
    string News()
    {
        var profile = Game.Profile;
        var news = new List<string>();
        foreach (var kin in Characters.All.Where(c => profile.Unlocked(c) && !Game.KinAtStart.Contains(c.Key)))
            news.Add(Loc.T($"{kin.Name} can mind the stall now!"));
        int levels = profile.MaxChallenge(State.Character);
        if (levels > Game.LevelsAtStart) news.Add(Loc.T($"Challenge level {levels} is open for {State.Kin.Name}."));
        // Nim's stock grows as Bram's kin get further: what he'll bring from the next run on.
        var curios = Curios.All.Where(c => profile.OpensAfter(c, State) && !Game.CuriosAtStart.Contains(c.Key)).ToList();
        if (curios.Count > 0)
            news.Add(curios.Count <= 3 ? Loc.T($"Nim has new curios for your next run: {Words.List(curios.Select(c => c.Name))}.")
                : Loc.T($"Nim has {curios.Count} new curios for your next run, the {curios[0].Name} among them."));
        if (_cleared && !State.TreasuryDone && State.Week <= Guild.Weeks) news.Add(Loc.T("Keep trading and every week you keep the Guild paid adds to the score."));
        if (news.Count > 0) return string.Join("  ", news);
        return profile is { Runs: > 0 }
            ? Loc.T($"All runs: {profile.Runs} started, {profile.Cleared} debts cleared, best week {profile.BestWeek}.")
            : "";
    }

    public void KeepTrading()
    {
        if (!_cleared || State.TreasuryDone) return;
        Sfx.Play("bid");
        Game.StartDay();
    }

    public void Retire()
    {
        Sfx.Play("bid");
        Game.EndRun();
    }
}
