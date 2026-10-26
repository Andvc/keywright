using System;
using System.Linq;

namespace Keywright.Core;

public static class RandomExtensions
{
    public static float NextFloat(this Random rng) => (float)rng.NextDouble();
    public static float Between(this Random rng, float min, float max) => min + (max - min) * (float)rng.NextDouble();
    public static T PickOne<T>(this Random rng, T[] items) => items[rng.Next(items.Length)];

    /// <summary>Picks by weight. Returns default if nothing has weight.</summary>
    public static T PickWeighted<T>(this Random rng, T[] items, Func<T, float> weight)
    {
        float total = items.Sum(weight);
        if (total <= 0) return default;
        float roll = rng.NextFloat() * total;
        foreach (var item in items)
        {
            roll -= weight(item);
            if (roll <= 0) return item;
        }
        return items[^1];
    }
}

public static class Money
{
    /// <summary>Small prices to the coin, bigger ones to the nearest 5.</summary>
    public static int Round(float value) => value <= 30 ? Math.Max(1, (int)MathF.Round(value)) : (int)MathF.Round(value / 5f) * 5;

    /// <summary>A sum as people say it ("12 gold", "45,000 gold"): the interface writes "45000g", but nobody talks like that.</summary>
    public static string Say(int amount) => amount == 1 ? "one coin" : $"{amount.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)} gold";
}
