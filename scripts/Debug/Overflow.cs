using System.Collections.Generic;
using Godot;
using Keywright.Ui;

namespace Keywright.Debug;

/// <summary>
/// With --shots, at every capture point (<see cref="Main.Moment"/>): each label and button whose text no longer fits
/// the box it was placed in (<see cref="UiKit.Place{T}"/> remembers it while <see cref="UiKit.TrackPlacement"/> is on),
/// printed once as "[overflow] moment: text". A translation that runs long shows up in the log, in every language,
/// before anyone has to spot it in a screenshot. Text that's clipped or trimmed on purpose is left alone.
/// </summary>
public static class Overflow
{
    static readonly HashSet<string> Reported = new();

    public static void Scan(Node root, string moment)
    {
        foreach (Node node in root.FindChildren("*", "Control", true, false))
        {
            if (node is not Control control || !control.IsVisibleInTree() || !control.HasMeta(UiKit.PlacedMeta)) continue;
            var placed = (Vector2)control.GetMeta(UiKit.PlacedMeta);
            if (placed.X < 8 || placed.Y < 8) continue;
            string text;
            Vector2 needs;
            switch (control)
            {
                case Label label when !label.ClipText && label.TextOverrunBehavior == TextServer.OverrunBehavior.NoTrimming:
                {
                    text = label.Text;
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    needs = label.GetMinimumSize();
                    int lines = label.GetLineCount();
                    if (label.AutowrapMode != TextServer.AutowrapMode.Off)
                    {
                        // Wrapped to the width it has: what matters is how tall that makes it.
                        int spacing = label.GetThemeConstant("line_spacing");
                        needs.Y = lines * label.GetLineHeight() + (lines - 1) * spacing;
                    }
                    // One line hangs from the top of its box, and a line is taller than its letters (the 12px-grid
                    // fonts' line at 48 is 64): only a second line can spill out of the bottom.
                    if (lines <= 1) needs.Y = 0;
                    break;
                }
                case Button button when !button.ClipText && button.TextOverrunBehavior == TextServer.OverrunBehavior.NoTrimming:
                    text = button.Text;
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    needs = button.GetMinimumSize();
                    needs.Y = 0; // a button's frame sets its least height, in every language alike
                    break;
                default:
                    continue;
            }
            // A pixel or two is rounding; a few more down is lines a little taller than the 20px a box allows each.
            if (needs.X <= placed.X + 2 && needs.Y <= placed.Y + 8) continue;
            string key = control.GetType().Name + "|" + text;
            if (!Reported.Add(key)) continue;
            GD.Print($"[overflow] {moment}: {control.GetType().Name} needs {needs.X:0}x{needs.Y:0}, placed {placed.X:0}x{placed.Y:0}: " +
                     $"\"{text.Replace("\n", "\\n")}\"");
        }
    }
}
