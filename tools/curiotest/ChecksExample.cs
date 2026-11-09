using System.Collections.Generic;
using Keywright.Core;

namespace CurioTest;

/// <summary>An example of the pattern: a twin with and without the curio, the same chest, the difference the card names.</summary>
public static class ChecksExample
{
    public static IEnumerable<Check> Checks()
    {
        yield return new("sealed", "a chest bought sight unseen costs 15% less; handled, the full price", () =>
        {
            var (without, with) = T.Twins("sealed");
            var chest = T.Chest(Rarity.Common, T.I(Category.Coins), T.I(Category.Arms));
            var plain = T.Counter(without, T.Visit(without, "garrick", chest, ask: 40, trade: Trade.Fixed));
            var sealedVisit = T.Counter(with, T.Visit(with, "garrick", chest, ask: 40, trade: Trade.Fixed));
            T.Near(plain.Price * Curios.SealedPrice, sealedVisit.Price, "sight unseen, the sealed bid's price");
            sealedVisit.Handle();
            T.Near(plain.Price, sealedVisit.Price, "handled, the price goes back up");
        });
    }
}
