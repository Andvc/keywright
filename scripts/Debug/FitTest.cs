using System.Collections.Generic;
using Godot;
using Keywright.Core;
using Keywright.Phases;
using Keywright.Ui;

namespace Keywright;

public partial class Main
{
    /// <summary>
    /// --fit-test [--lang=CODE]: the texts that sit in fixed boxes and come and go with a run, measured in the language
    /// being played with its own font: every curio's name and description on Nim's cards (wide, and narrow when he lays
    /// out four) and every keepsake's card at a talk and in the collection. A run only shows the few it deals, so a
    /// description too long for its card would otherwise wait for the run that deals it. Exits 1 if any doesn't fit.
    /// </summary>
    void FitTest()
    {
        _demo = true;
        _saving = false;
        var failures = new List<string>();
        int checkedCount = 0;
        void Fits(string what, string text, float width, float height, int size = 16)
        {
            checkedCount++;
            float needs = UiKit.TextHeight(text, width, size);
            if (needs > height + 0.5f) failures.Add($"{what}: needs {needs:0} of {height:0} at {width:0} wide: \"{text}\"");
        }
        void OneLine(string what, string text, float width, int size = 16)
        {
            checkedCount++;
            float needs = UiKit.TextWidth(text, size);
            if (needs > width) failures.Add($"{what}: needs {needs:0} of {width:0} wide: \"{text}\"");
        }

        foreach (bool narrow in new[] { false, true })
        {
            string card = narrow ? "narrow card" : "card";
            foreach (var curio in Curios.All)
            {
                // The name takes what it needs (two lines of the small size at most), and the description the rest.
                var (nameW, nameH, nameSize) = PeddlerPhase.NameBox(curio.Name, narrow);
                Fits($"{curio.Key}'s name, {card}", curio.Name, nameW, 47, nameSize);
                var (blurbW, blurbH) = PeddlerPhase.BlurbBox(narrow, nameH);
                Fits($"{curio.Key}, {card}", curio.Blurb, blurbW, blurbH);
            }
        }
        foreach (var keepsake in Keepsakes.All)
        {
            OneLine($"{keepsake.Key}'s heading, talk", TalkPhase.KeptHeading(keepsake), TalkPhase.KeptHeadingW);
            Fits($"{keepsake.Key}, talk", keepsake.Blurb, TalkPhase.KeptBox.W, TalkPhase.KeptBox.H);
            Fits($"{keepsake.Key}, collection", keepsake.Blurb, CollectionView.KeepsakeBlurbBox.W, CollectionView.KeepsakeBlurbBox.H);
        }

        foreach (string failure in failures) GD.Print($"[fit] {failure}");
        GD.Print(failures.Count == 0
            ? $"Fit test: all {checkedCount} fit ({Localization.Current})" // loc: keys
            : $"Fit test: {failures.Count} of {checkedCount} don't fit ({Localization.Current})"); // loc: keys
        QuitGame(failures.Count == 0 ? 0 : 1);
    }
}
