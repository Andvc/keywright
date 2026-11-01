using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Keywright.Core;
using Keywright.Phases;

namespace Keywright;

public partial class Main
{
    /// <summary>
    /// --pad-test [--shots=DIR]: plays the counter with a simulated controller, through the same events a real one sends:
    /// the left stick takes the pointer to the chest, A grabs it and flicking the stick shakes it (it must reach a hard
    /// shake), Y opens the notebook and B closes it, Start pauses and B resumes, and RB forces nothing (no lock here).
    /// Prints each check and quits with 1 if any failed.
    /// </summary>
    async void PadTest()
    {
        _demo = true;
        _saving = false;
        int failures = 0;
        void Check(bool ok, string what)
        {
            GD.Print($"{(ok ? "ok  " : "FAIL")}  {what}");
            if (!ok) failures++;
        }
        async Task Wait(double seconds) => await ToSignal(GetTree().CreateTimer(seconds, true), SceneTreeTimer.SignalName.Timeout);
        static void Stick(float x, float y)
        {
            Input.ParseInputEvent(new InputEventJoypadMotion { Axis = JoyAxis.LeftX, AxisValue = x });
            Input.ParseInputEvent(new InputEventJoypadMotion { Axis = JoyAxis.LeftY, AxisValue = y });
        }
        static void Button(JoyButton button, bool pressed) =>
            Input.ParseInputEvent(new InputEventJoypadButton { ButtonIndex = button, Pressed = pressed });
        async Task Tap(JoyButton button)
        {
            Button(button, true);
            await Wait(0.05);
            Button(button, false);
            await Wait(0.4);
        }

        try
        {
            // The day's story scene first: X (the act button) reads it line by line, as Space would.
            for (int i = 0; i < 80 && _phase is not CounterPhase; i++)
            {
                await Wait(0.3);
                if (_phase is ScenePhase) await Tap(JoyButton.X);
            }
            var screen = GetViewport().GetScreenTransform();
            GD.Print($"In a {DisplayServer.WindowGetSize()} window, the game drawn at {screen.Scale.X:0.##}x from {screen.Origin:0}");
            Check(_phase is CounterPhase, "X reads the story scene through to the counter");
            if (_phase is not CounterPhase counter) throw new InvalidOperationException("no counter to test");
            await Wait(1.5);
            // To the chest: steer the pointer at it until it's there.
            var view = counter.Shake.View;
            var chest = view.GlobalPosition + new Vector2(0, (view.Top + view.Bottom) / 2);
            for (int i = 0; i < 300 && GetViewport().GetMousePosition().DistanceTo(chest) > 12; i++)
            {
                // As a player does: full tilt while it's far, easing off as it gets close.
                var off = chest - GetViewport().GetMousePosition();
                var to = off.Normalized() * Mathf.Clamp(off.Length() / 200f, 0.35f, 1f);
                Stick(to.X, to.Y);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            Stick(0, 0);
            await Wait(0.2);
            Check(counter.Shake.Hover, $"the stick takes the pointer to the chest ({GetViewport().GetMousePosition():0}, the chest at {chest:0})");
            Moment("pad_01_at_chest");

            // Grab it and flick the stick from side to side.
            Button(JoyButton.A, true);
            var hardest = Core.Shake.Still;
            for (int flick = 0; flick < 16; flick++)
            {
                Stick(flick % 2 == 0 ? 1 : -1, 0);
                for (int f = 0; f < 6; f++)
                {
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                    if (counter.Shake.Level > hardest) hardest = counter.Shake.Level;
                }
            }
            Moment("pad_02_shaking");
            Stick(0, 0);
            Button(JoyButton.A, false);
            Check(hardest == Core.Shake.Hard, $"A and a flicked stick shake the chest hard (reached {hardest})");
            await Wait(1.5);

            await Tap(JoyButton.Y);
            Check(counter.Busy, "Y opens the notebook");
            Moment("pad_03_notebook");
            await Tap(JoyButton.B);
            Check(!counter.Busy, "B closes it");

            await Tap(JoyButton.Start);
            Check(GetTree().Paused, "Start pauses");
            Moment("pad_04_paused");
            await Tap(JoyButton.B);
            Check(!GetTree().Paused, "B resumes");
            Check(Controls.UsingPad && Controls.Hint(Controls.Notebook) == "Y", "prompts name the controller's buttons (the notebook's is Y)");

            // Rebinding: a key or button another action had moves, and an action left with nothing takes the old one.
            Controls.Bind(Controls.Act, Key.N);
            Check(Controls.Keys(Controls.Notebook).SequenceEqual(new[] { Key.L }), "binding N to act leaves the notebook L");
            Controls.Bind(Controls.Force, Key.L);
            Check(Controls.Keys(Controls.Notebook).SequenceEqual(new[] { Key.F }), "binding L to force swaps: the notebook takes F");
            Controls.Bind(Controls.Back, Key.Escape);
            Check(Controls.Keys(Controls.Pause).Contains(Key.Escape), "the menu and Back still share Esc");
            Controls.Bind(Controls.Notebook, JoyButton.X);
            Check(Controls.Buttons(Controls.Act).SequenceEqual(new[] { JoyButton.Y }), "binding X to the notebook swaps: act takes Y");
            await Tap(JoyButton.X);
            Check(counter.Busy, "the rebound X opens the notebook");
            await Tap(JoyButton.B);
            Controls.Reset();
            Check(Controls.Buttons(Controls.Notebook).SequenceEqual(new[] { JoyButton.Y }) && Controls.Keys(Controls.Act)[0] == Key.Space,
                "Defaults puts them all back");
        }
        catch (Exception e)
        {
            GD.PrintErr(e.ToString());
            failures++;
        }
        GD.Print(failures == 0 ? "Pad test: a controller plays the counter." : $"Pad test: {failures} FAILED");
        await Wait(0.3);
        QuitGame(failures == 0 ? 0 : 1);
    }
}
