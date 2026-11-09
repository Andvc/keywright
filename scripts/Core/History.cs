using System.Collections.Generic;
using System.Linq;

namespace Keywright.Core;

/// <summary>What kind of thing was said: someone's line in a scene or a talk, a seller at the counter, Bram's tip or lesson.</summary>
public enum Heard { Dialogue, Seller, Tip, Lesson }

/// <summary>A line said in the run: the day, who said it (as the screen named them), the whole of it, and what kind.</summary>
public sealed record Said(int Day, string Who, string Text, Heard Kind);

/// <summary>
/// The run's history (0.35): everything said to you, oldest first, kept for the listening horn on the counter to say
/// back (people's lines in scenes and talks, sellers' words at the counter, Bram's tips and lessons, his notes).
/// </summary>
public sealed partial class GameState
{
    /// <summary>The most the history keeps; the oldest go first.</summary>
    public const int HistoryLimit = 400;

    public List<Said> History { get; } = new();

    /// <summary>Notes a line in the history (said again straight after, by the same voice, it's kept once).</summary>
    public void Hear(string who, string text, Heard kind)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        if (History.Count > 0 && History[^1].Text == text && History[^1].Who == who) return;
        History.Add(new Said(Day, who ?? "", text, kind));
        if (History.Count > HistoryLimit) History.RemoveRange(0, History.Count - HistoryLimit);
    }

    void SaveHistory(SaveData data) => data.History = History.ToList();

    void LoadHistory(SaveData data)
    {
        History.Clear();
        History.AddRange((data.History ?? new List<Said>()).Where(s => s != null && !string.IsNullOrEmpty(s.Text)).TakeLast(HistoryLimit));
    }
}
