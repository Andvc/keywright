using System.Linq;

namespace Keywright.Core;

/// <summary>
/// Where a chest came from: a millpond, the Barrow, a goblin warren, the Maw's hoard. Three hundred years after the
/// Locking, chests are everywhere, and where one was found says what it tends to hold (see docs/world.md). Sellers say
/// where theirs came from.
/// </summary>
/// <param name="Name">"the Barrow", as a seller would say it.</param>
/// <param name="Holds">What it tends to hold, for the ledger's Places page.</param>
/// <param name="Kinds">How likely each kind of thing is in a chest from here, by <see cref="Category"/>.</param>
public sealed record OriginDef(string Key, string Name, string Holds, float[] Kinds);

public static class Origins
{
    //                                                                                                coin weapon cloak potion spell gem
    public static readonly OriginDef[] All =
    {
        new("millpond", Loc.T("the millpond"), Loc.T("whatever fell in; often nothing at all"), new[] { 3f, 2, 2, 2, 1, 0.5f }),
        new("attic", Loc.T("a farmhouse attic"), Loc.T("savings and Sunday clothes"), new[] { 3f, 1, 4, 1, 2, 0.5f }),
        new("kitbag", Loc.T("a soldier's kitbag"), Loc.T("weapons and a cloak"), new[] { 1f, 4, 4, 1, 1, 0.3f }),
        new("field", Loc.T("the old battlefield"), Loc.T("weapons, and the coin the dead carried"), new[] { 3f, 5, 1, 1, 0.5f, 0.3f }),
        new("tab", Loc.T("an unpaid tab"), Loc.T("a bit of everything"), new[] { 3f, 3, 2, 3, 1, 0.5f }),
        new("barrow", Loc.T("the Barrow"), Loc.T("grave goods: coin, weapons and gems"), new[] { 3f, 3, 2, 1, 1, 2 }),
        new("warren", Loc.T("a goblin burrow"), Loc.T("shinies a clan has dug up: coin, mostly"), new[] { 6f, 2, 1, 1, 0.5f, 2 }),
        new("camp", Loc.T("an outlaws' camp"), Loc.T("weapons, purses and potions"), new[] { 3f, 4, 1, 3, 1, 0.5f }),
        new("den", Loc.T("a wolf's den"), Loc.T("what the wolves' dinner wore"), new[] { 1f, 2, 5, 0.5f, 0.5f, 0.5f }),
        new("wood", Loc.T("the Old Wood"), Loc.T("brews, elven cloaks and spells on birch bark"), new[] { 0.5f, 0.5f, 4, 4, 3, 0.5f }),
        new("crypt", Loc.T("the abbey crypt"), Loc.T("holy cloaks, prayer-spells and holy water"), new[] { 1f, 1, 5, 2, 3, 1 }),
        new("road", Loc.T("the smugglers' road"), Loc.T("brandy and coin, and false bottoms"), new[] { 3f, 1, 1, 4, 1, 2 }),
        new("arcanum", Loc.T("the Arcanum stores"), Loc.T("mantles, potions and spellbooks, often enchanted"), new[] { 0.5f, 1, 3, 3, 4, 2 }),
        new("hold", Loc.T("a dwarf-hold"), Loc.T("weapons and gems"), new[] { 2f, 5, 0.5f, 0.5f, 1, 4 }),
        new("strongroom", Loc.T("a noble's strongroom"), Loc.T("coin, gems and fine clothes"), new[] { 4f, 1, 3, 1, 1, 5 }),
        new("pawn", Loc.T("a forgotten pawn lot"), Loc.T("anything, in twos and threes"), new[] { 4f, 3, 2, 2, 1, 3 }),
        new("river", Loc.T("the Aldwater mud"), Loc.T("drowned wizards' potions and spellbooks (and sand)"), new[] { 2f, 1, 1, 4, 5, 2 }),
        new("warchest", Loc.T("a knight's war chest"), Loc.T("weapons and a cloak"), new[] { 2f, 5, 3, 1, 1, 1 }),
        new("gate", Loc.T("the Maw's hoard"), Loc.T("gold and gems (some of it demon gold)"), new[] { 5f, 2, 1, 1, 1, 5 }),
        new("treasury", Loc.T("the Royal Treasury"), Loc.T("the best of everything"), new[] { 3f, 3, 2, 2, 2, 3 }),
        new("dowry", Loc.T("a bride's dowry"), Loc.T("coin for luck, a cloak for warmth, a potion for health"), new[] { 3f, 0.3f, 3, 3, 0.5f, 1 }),
        new("archive", Loc.T("the Archive vaults"), Loc.T("deeds, spellbooks, coin and seals"), new[] { 2f, 1, 1, 1, 4, 2 }),
        new("court", Loc.T("a courtier's chest"), Loc.T("jewels, fine clothes and purses"), new[] { 3f, 1, 3, 2, 1, 4 }),
        new("pocket", Loc.T("a stranger's pocket"), Loc.T("whatever someone was carrying: purses and trinkets"), new[] { 5f, 1, 1, 2, 1, 3 }),
    };

    public static OriginDef Get(string key) => All.First(o => o.Key == key);
}
