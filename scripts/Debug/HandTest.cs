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
    /// --hand-test --no-tutorial [--seed=N]: a hand's shaking, through the counter and a real pointer. A hand shakes the
    /// chest the ways hands do (a smooth firm shake, a hard one with its wind-up and its wind-down, a mouse that reports
    /// in jumps, a slow frame rate), lifts it and carries it; after each, the counter has counted the strengths that way
    /// of shaking is (<see cref="Visit.Checks"/>: what Bram's lessons wait for, what the hint under the chest says, and
    /// what the opening calls unheard). Prints each check and quits with 1 if any failed.
    /// <para>
    /// The bot shakes through <see cref="Ui.ShakeBox.AutoShake"/>, at exact strengths, so it can't show this. (It was
    /// 0.36's `--ear-test`, written when a strength that didn't register cost a listen for nothing; 0.37 took the listens
    /// out, and a shake that doesn't count still strands a lesson.)
    /// </para>
    /// </summary>
    async void HandTest()
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
        async Task Frame() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        static double Now() => Time.GetTicksUsec() / 1_000_000.0;

        var pointer = Vector2.Zero;
        void MoveTo(Vector2 spot)
        {
            var at = GetViewport().GetScreenTransform() * spot;
            Input.WarpMouse(at);
            Input.ParseInputEvent(new InputEventMouseMotion { Position = at, GlobalPosition = at, Relative = at - pointer, ButtonMask = MouseButtonMask.Left });
            pointer = at;
        }
        void Press(bool down) =>
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = down, Position = pointer, GlobalPosition = pointer });

        try
        {
            // To the counter, and to a chest with no rule of its own about shaking (not Tobin's rotten one).
            CounterPhase counter = null;
            for (int i = 0; i < 400; i++)
            {
                await Wait(0.25);
                if (_phase is ScenePhase)
                {
                    Input.ParseInputEvent(new InputEventJoypadButton { ButtonIndex = JoyButton.X, Pressed = true });
                    await Wait(0.05);
                    Input.ParseInputEvent(new InputEventJoypadButton { ButtonIndex = JoyButton.X, Pressed = false });
                    continue;
                }
                if (_phase is not CounterPhase here || here.Busy || here.Concluded) continue;
                var seller = here.Visit.Seller;
                bool plain = here.Visit.Talk == null && State.ShakeLimit(seller) <= 0 && !State.MindsHardShake(seller) && !State.Impatient(seller) &&
                             here.Visit.ShakeFee <= 0 && State.CanShakeHardAt(seller);
                if (plain)
                {
                    counter = here;
                    break;
                }
                here.Pass();
                await Wait(2.5);
            }
            Check(counter != null, "a plain chest at the counter");
            if (counter == null) throw new InvalidOperationException("no chest to test");
            await Wait(1.2);
            var visit = counter.Visit;
            var view = counter.Shake.View;
            var chest = view.GlobalPosition + new Vector2(0, (view.Top + view.Bottom) / 2);
            GD.Print($"{visit.Seller.Name}'s chest at {chest:0}");

            // One hold of the chest: the pointer follows a path (reported every frame, or in jumps), then lets go.
            // The frames a second it got go into `rate`: a hand's shake is only seen as often as the game draws.
            double rate = 0;
            async Task Hold(Func<double, Vector2> path, double seconds, double every = 0, double[] pace = null)
            {
                int paced = 0;
                MoveTo(chest);
                await Frame();
                await Frame();
                Press(true);
                await Frame();
                double start = Now(), last = -1;
                int frames = 0;
                for (double t = 0; t < seconds; t = Now() - start)
                {
                    // (An uneven pace: the pointer's seen after each of these gaps in turn, as on a machine whose
                    // frames come a short one, then a long one.)
                    if (pace != null ? t - last >= pace[paced % pace.Length] : every <= 0 || t - last >= every)
                    {
                        MoveTo(chest + path(t));
                        last = t;
                        paced++;
                    }
                    await Frame();
                    frames++;
                }
                rate = frames / Math.Max(0.001, Now() - start);
                Press(false);
                await Wait(1.1);
            }
            static Func<double, Vector2> Swing(float amp, float hz) => t => new Vector2(amp, amp / 3f) * MathF.Sin((float)(Math.Tau * hz * t));
            // A mouse that reports in jumps: the other side each time it's asked (as xdotool does, a step at a time).
            static Func<double, Vector2> Flip(float amp)
            {
                int side = -1;
                return _ => new Vector2(amp, amp / 3f) * (side = -side);
            }
            var gentle = Swing(12, 2f);
            var firm = Swing(45, 3f);
            var hard = Swing(110, 3.5f);

            // Each way of shaking, on a chest nothing's been tried on: the strengths the counter counted.
            async Task<string> Try(Func<Task> shake)
            {
                counter.ForgetTried();
                await shake();
                return string.Join("+", visit.Checks.OrderBy(k => k, StringComparer.Ordinal).Select(k => k switch
                {
                    "1" => "gentle", "2" => "firm", "3" => "hard", "drop" => "drop", _ => k, // loc: keys
                }));
            }
            // A shake three times a second can't be seen at all below ten frames or so: if the machine didn't give the
            // test its frames (it's busy with something else), the shake is tried again before it's called a failure.
            double needs = 20;
            async Task Expect(string name, Func<Task> shake, string must, string never = "")
            {
                string got = "";
                bool Right() => must.Split('+', StringSplitOptions.RemoveEmptyEntries).All(got.Split('+').Contains) &&
                                !never.Split('+', StringSplitOptions.RemoveEmptyEntries).Any(got.Split('+').Contains);
                for (int attempt = 0; attempt < 4; attempt++)
                {
                    got = await Try(shake);
                    if (Right() || rate >= needs) break;
                    GD.Print($"      ({name}: only {rate:0} frames a second, trying again)");
                    await Wait(2.0);
                }
                string slow = !Right() && rate < needs ? $" at {rate:0} frames a second: run this test on its own" : "";
                string want = (must == "" ? "nothing" : must) + (never == "" ? "" : $", never {never.Replace("+", " or ")}");
                Check(Right(), $"{name}: counts {want}{(Right() ? "" : $", but counted {(got == "" ? "nothing" : got)}{slow}")}");
            }

            await Expect("a gentle shake", () => Hold(gentle, 1.6), "gentle", "firm+hard");
            await Expect("a firm shake", () => Hold(firm, 1.4), "firm", "hard");
            await Expect("a hard shake, wound up to and wound down from", () => Hold(t =>
                t < 0.25 ? Swing(110 * (float)(t / 0.25), 3.5f)(t) : t < 1.4 ? hard(t) : Vector2.Zero, 2.1), "hard");
            // (The playtest tool's own amplitudes, tools/playtest/pt: the chest glides between reports, so a flip reads
            // as the speed of the move it is.)
            await Expect("a firm shake from a mouse that reports in jumps", () => Hold(Flip(38), 2.0, 0.09), "firm", "hard");
            await Expect("a hard shake from a mouse that reports in jumps", () => Hold(Flip(95), 1.6, 0.08), "hard");
            await Expect("a gentle shake from a mouse that reports in jumps", () => Hold(Flip(6), 2.0, 0.1), "gentle", "firm+hard");
            await Expect("a firm shake seen at an uneven pace (short frames and long ones)", () => Hold(firm, 2.4, pace: new[] { 0.03, 0.13, 0.05, 0.12 }), "firm", "hard");
            await Expect("carrying the chest across the counter", () => Hold(t => new Vector2(110 * (float)Math.Min(t, 1.2), 0), 1.6), "", "firm+hard");
            await Expect("lifting it and setting it down again", () => Hold(t =>
                new Vector2(0, -90 * (float)(t < 0.5 ? t / 0.5 : t < 0.9 ? 1 : Math.Max(0, 1 - (t - 0.9) / 0.5))), 1.6), "", "firm+hard");
            await Expect("a hard shake, then a gentle one in the same hold", () => Hold(t => t < 1.2 ? hard(t) : t < 1.9 ? Vector2.Zero : gentle(t), 3.6), "gentle+hard");

            // A slow machine: the same shakes at twelve frames a second.
            int fps = Engine.MaxFps;
            Engine.MaxFps = 12;
            needs = 9;
            await Wait(0.5);
            await Expect("a firm shake at twelve frames a second", () => Hold(firm, 2.0), "firm", "hard");
            await Expect("a firm shake in jumps at twelve frames a second", () => Hold(Flip(38), 2.4, 0.09), "firm", "hard");
            await Expect("a hard shake at twelve frames a second", () => Hold(hard, 1.8), "hard");
            await Expect("a gentle shake at twelve frames a second", () => Hold(gentle, 2.0), "gentle", "firm+hard");
            Engine.MaxFps = fps;
            needs = 20;
            await Wait(0.5);
        }
        catch (Exception e)
        {
            GD.PrintErr(e.ToString());
            failures++;
        }
        GD.Print(failures == 0 ? "Hand test: a hand's shaking counts the strengths it should." : $"Hand test: {failures} FAILED");
        await ToSignal(GetTree().CreateTimer(0.3, true), SceneTreeTimer.SignalName.Timeout);
        QuitGame(failures == 0 ? 0 : 1);
    }
}
