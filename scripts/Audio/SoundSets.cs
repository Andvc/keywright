using System;
using System.Collections.Generic;
using Keywright.Core;

namespace Keywright.Audio;

/// <summary>
/// The sound each kind of thing makes, with alternatives to choose from in Settings > Item sounds. Each option is a
/// sound group: recordings under res://assets/audio/{group}/ (from <c>tools/import_art.py</c>) or one synthesized in
/// <see cref="Sfx"/>. The first option of each kind is the default.
/// </summary>
public static class SoundSets
{
    public sealed record Option(string Name, string Group);

    static readonly Dictionary<Category, Option[]> All = new()
    {
        [Category.Coins] = new Option[]
        {
            new(Loc.T("A: a purse of coins"), "jingle"), new(Loc.T("B: clinking chips"), "jingle_chips"), new(Loc.T("C: a handful of chips"), "jingle_handle"),
            new(Loc.T("D: bright coins (synth)"), "jingle_bright"),
        },
        [Category.Arms] = new Option[]
        {
            new(Loc.T("A: tin"), "clank"), new(Loc.T("B: light metal"), "clank_light"), new(Loc.T("C: pots and pans"), "clank_pot"),
            new(Loc.T("D: metal plate"), "clank_plate"),
        },
        [Category.Garb] = new Option[]
        {
            new(Loc.T("A: cloth"), "flump"), new(Loc.T("B: soft thump"), "flump_soft"), new(Loc.T("C: heavy thump"), "flump_heavy"),
            new(Loc.T("D: leather"), "flump_leather"),
        },
        [Category.Potions] = new Option[]
        {
            new(Loc.T("A: slosh (synth)"), "slosh"), new(Loc.T("B: glug (synth)"), "slosh_glug"), new(Loc.T("C: glass clink"), "slosh_clink"),
        },
        [Category.Scrolls] = new Option[]
        {
            new(Loc.T("A: shoved cards"), "rustle"), new(Loc.T("B: book pages"), "rustle_book"), new(Loc.T("C: sliding cards"), "rustle_slide"),
            new(Loc.T("D: shuffled cards"), "rustle_shuffle"),
        },
        [Category.Gems] = new Option[]
        {
            new(Loc.T("A: glass tinkle"), "tinkle"), new(Loc.T("B: other glass"), "tinkle_glass"), new(Loc.T("C: little bells (synth)"), "tinkle_bell"),
            new(Loc.T("D: light plate"), "tinkle_plate"),
        },
    };

    public static IReadOnlyList<Option> For(Category kind) => All[kind];

    /// <summary>The chosen option for a kind.</summary>
    public static Option Chosen(Category kind)
    {
        var options = All[kind];
        return options[Math.Clamp(Settings.SoundSet(kind), 0, options.Length - 1)];
    }

    /// <summary>The sound group to play for a thing of this kind.</summary>
    public static string Group(Category kind) => Chosen(kind).Group;
}
