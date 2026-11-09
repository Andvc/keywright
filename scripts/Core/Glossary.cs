using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>The trade's words that curios and tips lean on, each with exactly how it works in the game.</summary>
public enum Term { SightUnseen, BramsWord, Stash, ShortNight, PerfectRead, StepBetter, Surprise, DrySpell, Exhibition, Ward, FalseBottom, Haggling, Mended }

/// <summary>
/// The glossary (0.32): what "sight unseen", "Bram's word", "a perfect read" and the rest mean, said once and exactly,
/// so a curio's card can stay short and still be understood. A curio lists the words it uses (<see cref="For"/>); Nim's
/// cards, the curios at the counter and the day's end and the collection show their definitions.
/// </summary>
public static class Glossary
{
    public static string Name(Term term) => term switch
    {
        Term.SightUnseen => Loc.T("Sight unseen"),
        Term.BramsWord => Loc.T("Bram's word"),
        Term.Stash => Loc.T("Bram's stash"),
        Term.ShortNight => Loc.T("A short night"),
        Term.PerfectRead => Loc.T("A perfect read"),
        Term.StepBetter => Loc.T("A step better"),
        Term.Surprise => Loc.T("A surprise"),
        Term.DrySpell => Loc.T("A dry spell"),
        Term.Exhibition => Loc.T("Exhibition points"),
        Term.Ward => Loc.T("A warded thing"),
        Term.FalseBottom => Loc.T("A false bottom"),
        Term.Haggling => Loc.T("Haggling"),
        _ => Loc.T("Mended"),
    };

    public static string Define(Term term)
    {
        var (below, gift) = GameState.Stash(1);
        return term switch
        {
            Term.SightUnseen => Loc.T("A chest bought without shaking it, dropping it or putting the lodestone to it. The witchlight doesn't " +
                                      "count, and nor does what the seller tells you. Once you've handled it, it's no longer sight unseen."),
            Term.BramsWord => Loc.T("On a Guild night you can't pay, Bram can vouch for you, once a run: the Guild takes all your gold, " +
                                    "and what you still owe, half as much again, is added to next week's due. Not in the debt's last week."),
            Term.Stash => Loc.T($"{GameState.StashTimes} times a run, when a morning starts with under {below}g in your purse at the stall, " +
                                $"Bram slips you {gift}g from his secret stash (more in a grander shop)."),
            Term.ShortNight => Loc.T($"A Guild night when your purse can't cover the due. You can pawn what's on display (for " +
                                     $"{Synergies.Pct(Guild.PawnRate)} of its price), sell curios back (for half), call in a friend's favour, " +
                                     $"or take Bram's word."),
            Term.PerfectRead => Loc.T("Your slip named exactly what came out: how many of each kind, which were enchanted, and the false " +
                                      "bottom or warded thing if there was one. Nothing missed, nothing extra."),
            Term.StepBetter => Loc.T("One colour finer than its chest promised: common, uncommon, rare, epic. Luck never makes a thing " +
                                     "gold: a purple thing it would lift comes out enchanted instead."),
            Term.Surprise => Loc.T("A thing that comes out a step better than its chest's colour promised. A bad surprise is a step worse."),
            Term.DrySpell => Loc.T($"Things opened one after another that came out no better than their chest. After {GameState.PityAfter}, " +
                                   $"each chest you buy gets a growing chance ({Synergies.Pct(GameState.PityStep)} more for every thing " +
                                   $"since, up to {Synergies.Pct(GameState.PityMax)}) that something in it comes out a step better."),
            Term.Exhibition => Loc.T("What's on your display shelf scores points: 1 for a common thing, 2 uncommon, 4 rare, 8 epic, 16 " +
                                     "gold, twice that if enchanted. Points draw customers and finer chests."),
            Term.Ward => Loc.T("One thing a chest's last owner hid with a ward: it never sounds, weighs nothing and isn't counted. It " +
                               "comes out only if your slip names it: mark one more of its kind than you heard."),
            Term.FalseBottom => Loc.T("A hidden compartment under the chest's floor. Dropped from high up, the chest lands with a hollow " +
                                      "boom. What's under it always comes out; mark the false bottom on your slip for a perfect read."),
            Term.Haggling => Loc.T("A seller who haggles takes an offer: pick your price on the slider. They take it, come part of the " +
                                   "way down, or walk out if you go too low."),
            _ => Loc.T("Nothing of that kind comes out worse than its chest promised."),
        };
    }

    /// <summary>"Sight unseen: A chest bought without...", for a tooltip.</summary>
    public static string Line(Term term) => Words.Wrap(Loc.T($"{Name(term)}: {Define(term)}"));

    /// <summary>The words a curio's card uses, in the order they matter (none for a card that says it all).</summary>
    public static IReadOnlyList<Term> For(CurioDef curio) => CurioTerms.TryGetValue(curio.Key, out var terms) ? terms : None;

    static readonly Term[] None = System.Array.Empty<Term>();

    static readonly Dictionary<string, Term[]> CurioTerms = new() // loc: keys
    {
        ["sealed"] = new[] { Term.SightUnseen }, ["dip"] = new[] { Term.SightUnseen, Term.StepBetter }, ["bond"] = new[] { Term.SightUnseen },
        ["blindfold"] = new[] { Term.SightUnseen }, ["token"] = new[] { Term.Surprise }, ["crescent"] = new[] { Term.Surprise },
        ["signet"] = new[] { Term.Haggling }, ["tongue"] = new[] { Term.Haggling }, ["knot"] = new[] { Term.Haggling },
        ["coffer"] = new[] { Term.Haggling }, ["goldentongue"] = new[] { Term.Haggling },
        ["beads"] = new[] { Term.FalseBottom }, ["hook"] = new[] { Term.FalseBottom }, ["rod"] = new[] { Term.FalseBottom, Term.StepBetter },
        ["log"] = new[] { Term.FalseBottom }, ["pass"] = new[] { Term.FalseBottom }, ["compass"] = new[] { Term.FalseBottom },
        ["seal"] = new[] { Term.PerfectRead }, ["register"] = new[] { Term.PerfectRead }, ["fork"] = new[] { Term.PerfectRead },
        ["ledger"] = new[] { Term.PerfectRead }, ["truthglass"] = new[] { Term.PerfectRead },
        ["wardlamp"] = new[] { Term.Ward }, ["pillow"] = new[] { Term.Ward }, ["wardtally"] = new[] { Term.Ward },
        ["wardkey"] = new[] { Term.Ward, Term.StepBetter }, ["aldricseal"] = new[] { Term.Ward },
        ["shell"] = new[] { Term.DrySpell }, ["foot"] = new[] { Term.StepBetter }, ["wishbone"] = new[] { Term.Surprise },
        ["horseshoe"] = new[] { Term.Surprise }, ["clover"] = new[] { Term.StepBetter }, ["star"] = new[] { Term.Surprise, Term.StepBetter },
        ["spindle"] = new[] { Term.Surprise, Term.StepBetter }, ["wheel"] = new[] { Term.StepBetter },
        ["book"] = new[] { Term.Exhibition }, ["rope"] = new[] { Term.Exhibition }, ["lamps"] = new[] { Term.Exhibition },
        ["dummy"] = new[] { Term.Exhibition }, ["gauntlet"] = new[] { Term.Mended, Term.StepBetter }, ["cap"] = new[] { Term.Mended, Term.StepBetter },
        ["ticket"] = new[] { Term.ShortNight }, ["marker"] = new[] { Term.BramsWord }, ["note"] = new[] { Term.ShortNight },
        ["purse"] = new[] { Term.Stash },
    };

    /// <summary>A curio's words and what they mean, a line each, for under its description (empty if it uses none).</summary>
    public static string Explain(CurioDef curio) => string.Join("\n", For(curio).Select(Line));

    /// <summary>A curio as a tooltip says it: its name and description, then the words it uses, defined.</summary>
    public static string Tooltip(CurioDef curio)
    {
        string head = Words.Wrap(Loc.T($"{curio.Name}: {curio.Blurb}"));
        string terms = Explain(curio);
        return terms.Length == 0 ? head : head + "\n\n" + terms;
    }
}
