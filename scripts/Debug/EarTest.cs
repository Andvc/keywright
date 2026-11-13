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
    /// --ear-test --no-tutorial [--seed=N]: the ear (0.36), through the counter and a real pointer. A hand shakes the chest
    /// the ways hands do (a smooth firm shake, a hard one with its wind-up and its wind-down, a mouse that reports in
    /// jumps, a slow frame rate), lifts it, carries it, and shakes with no listens left; after each, exactly the listens
    /// that way of shaking should cost have been spent. Prints each check and quits with 1 if any failed.
    /// <para>
    /// The bot shakes through <see cref="Ui.ShakeBox.AutoShake"/>, at exact strengths, so it never showed what this
    /// does: 0.36's first ear counted a strength only on frames where the hand's speed, sampled once a frame, was over
    /// its threshold, and at twelve frames a second a firm shake lit the meter and never got the ear.
    /// </para>
    /// </summary>
    async void EarTest()
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
                             here.Visit.ShakeFee <= 0 && !State.ListensFree(here.Visit);
                if (plain)
                {
                    counter = here;
                    break;
                }
                here.Pass();
                await Wait(2.5);
            }
            Check(counter != null, "a plain chest at the counter, with listens to count");
            if (counter == null) throw new InvalidOperationException("no chest to test");
            await Wait(1.2);
            var visit = counter.Visit;
            var view = counter.Shake.View;
            var chest = view.GlobalPosition + new Vector2(0, (view.Top + view.Bottom) / 2);
            GD.Print($"{visit.Seller.Name}'s chest at {chest:0}, {State.Listens} listens a day");

            // One hold of the chest: the pointer follows a path (reported every frame, or in jumps), then lets go.
            // The frames a second it got go into `rate`: a hand's shake is only seen as often as the game draws.
            double rate = 0;
            async Task Hold(Func<double, Vector2> path, double seconds, double every = 0)
            {
                MoveTo(chest);
                await Frame();
                await Frame();
                Press(true);
                await Frame();
                double start = Now(), last = -1;
                int frames = 0;
                for (double t = 0; t < seconds; t = Now() - start)
                {
                    if (every <= 0 || t - last >= every)
                    {
                        MoveTo(chest + path(t));
                        last = t;
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

            // Each way of shaking, on a fresh ear and a chest not yet listened to.
            async Task<string> Try(Func<Task> shake, params Check[] already)
            {
                State.RestEar();
                visit.Checks.Clear();
                foreach (var check in already) State.Listen(visit, check);
                int before = State.ListensUsed;
                var known = visit.Checks.ToHashSet();
                await shake();
                string spent = string.Join("+", visit.Checks.Where(k => !known.Contains(k)).OrderBy(k => k, StringComparer.Ordinal).Select(k => k switch
                {
                    "1" => "gentle", "2" => "firm", "3" => "hard", "drop" => "drop", _ => k, // loc: keys
                }));
                return $"{State.ListensUsed - before}:{spent}";
            }
            // A shake three times a second can't be seen at all below ten frames or so: if the machine didn't give the
            // test its frames (it's busy with something else), the shake is tried again before it's called a failure.
            double needs = 20;
            async Task Expect(string name, string want, Func<Task> shake, params Check[] already)
            {
                string got = "";
                for (int attempt = 0; attempt < 4; attempt++)
                {
                    got = await Try(shake, already);
                    if (got == want || rate >= needs) break;
                    GD.Print($"      ({name}: only {rate:0} frames a second, trying again)");
                    await Wait(2.0);
                }
                string slow = got != want && rate < needs ? $" at {rate:0} frames a second: run this test on its own" : "";
                Check(got == want, $"{name}: spends {want.Replace(":", " (")}){(got == want ? "" : $", but spent {got.Replace(":", " (")}){slow}")}");
            }
            var heardHard = Core.Check.Shaking(Core.Shake.Hard);
            var heardGentle = Core.Check.Shaking(Core.Shake.Gentle);

            await Expect("a gentle shake", "1:gentle", () => Hold(gentle, 1.6));
            await Expect("a firm shake", "1:firm", () => Hold(firm, 1.4));
            await Expect("a hard shake, wound up to and wound down from", "1:hard", () => Hold(t =>
                t < 0.25 ? Swing(110 * (float)(t / 0.25), 3.5f)(t) : t < 1.4 ? hard(t) : Vector2.Zero, 2.1));
            await Expect("a firm shake once the chest's been shaken hard (the playtest's stuck lesson)", "1:firm", () => Hold(firm, 1.4), heardGentle, heardHard);
            await Expect("the same shake again", "0:", () => Hold(firm, 1.2), heardGentle, Core.Check.Shaking(Core.Shake.Firm), heardHard);
            await Expect("a firm shake from a mouse that reports in jumps", "1:firm", () => Hold(Flip(18), 2.0, 0.09));
            await Expect("a hard shake from a mouse that reports in jumps", "1:hard", () => Hold(Flip(60), 1.6, 0.08));
            await Expect("a gentle shake from a mouse that reports in jumps", "1:gentle", () => Hold(Flip(6), 2.0, 0.1));
            await Expect("carrying the chest across the counter", "0:", () => Hold(t => new Vector2(110 * (float)Math.Min(t, 1.2), 0), 1.6));
            await Expect("lifting it and setting it down again", "0:", () => Hold(t =>
                new Vector2(0, -90 * (float)(t < 0.5 ? t / 0.5 : t < 0.9 ? 1 : Math.Max(0, 1 - (t - 0.9) / 0.5))), 1.6));
            await Expect("a hard shake, then a gentle one in the same hold", "2:gentle+hard", () => Hold(t => t < 1.2 ? hard(t) : t < 1.9 ? Vector2.Zero : gentle(t), 3.6));

            // A slow machine: the same firm shakes at twelve frames a second.
            int fps = Engine.MaxFps;
            Engine.MaxFps = 12;
            needs = 9;
            await Wait(0.5);
            await Expect("a firm shake at twelve frames a second", "1:firm", () => Hold(firm, 2.0), heardGentle, heardHard);
            await Expect("a firm shake in jumps at twelve frames a second", "1:firm", () => Hold(Flip(18), 2.4, 0.09), heardGentle, heardHard);
            await Expect("a hard shake at twelve frames a second", "1:hard", () => Hold(hard, 1.8));
            Engine.MaxFps = fps;
            needs = 20;
            await Wait(0.5);

            // No listens left: a strength not yet tried tells nothing, and one already tried still sounds.
            State.RestEar();
            visit.Checks.Clear();
            State.Listen(visit, heardGentle);
            foreach (var waiting in State.Queue.Where(v => v.Talk == null).Take(3))
                foreach (var level in new[] { Core.Shake.Gentle, Core.Shake.Firm, Core.Shake.Hard })
                    State.Listen(waiting, Core.Check.Shaking(level));
            // (Whatever's left after the queue's been listened to: spend it on this chest's own drop and lodestone checks.)
            foreach (var check in new[] { Core.Check.Dropping, Core.Check.Shaking(Core.Shake.Gentle, true), Core.Check.Shaking(Core.Shake.Firm, true),
                         Core.Check.Shaking(Core.Shake.Hard, true) })
                if (State.ListensLeft > 0) State.Listen(visit, check);
            Check(State.ListensLeft == 0, $"the day's listens can all be spent ({State.ListensUsed} of {State.Listens})");
            int used = State.ListensUsed;
            var had = visit.Checks.ToHashSet();
            await Hold(hard, 1.4);
            Check(State.ListensUsed == used && visit.Checks.SetEquals(had), "with none left, a hard shake spends nothing and opens nothing");
        }
        catch (Exception e)
        {
            GD.PrintErr(e.ToString());
            failures++;
        }
        GD.Print(failures == 0 ? "Ear test: a hand's shaking spends the listens it should." : $"Ear test: {failures} FAILED");
        await ToSignal(GetTree().CreateTimer(0.3, true), SceneTreeTimer.SignalName.Timeout);
        QuitGame(failures == 0 ? 0 : 1);
    }
}
