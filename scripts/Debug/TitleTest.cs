using System;
using System.Threading.Tasks;
using Godot;
using Keywright.Phases;

namespace Keywright;

public partial class Main
{
    /// <summary>
    /// --title-test: the title's burst (0.32) against input sent the way the real thing sends it. A click on a button that
    /// has landed while the rest are still coming out is a click, not a skip; a key, a controller button or a click
    /// anywhere else while the chest is still straining puts everything in place at once, every button clickable; and
    /// that click doesn't press the button that lands under the pointer. Prints each check and quits with 1 if any failed.
    /// </summary>
    async void TitleTest()
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
        async Task<TitlePhase> Title()
        {
            var before = _phase;
            ToTitle();
            for (int i = 0; i < 300 && (_phase == before || _phase is not TitlePhase); i++) await Frame();
            return (TitlePhase)_phase;
        }
        // A click on a spot in the 1280x720 scene: the pointer's events are in the window's pixels, wherever it's drawn.
        async Task Click(Vector2 spot)
        {
            var at = GetViewport().GetScreenTransform() * spot;
            Input.WarpMouse(at);
            await Frame();
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = at, GlobalPosition = at });
            await Frame();
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = at, GlobalPosition = at });
            await Frame();
        }

        try
        {
            // The whole burst, as a session's first title plays it: the Collection's button, clicked as soon as it lands
            // (with the Credits still to come), opens the collection, and the rest keep coming out.
            var title = await Title();
            var collection = title.CollectionButton;
            for (int i = 0; i < 600 && !title.HasLanded(collection); i++) await Frame();
            await Click(title.HomeOf(collection));
            Check(title.CollectionOpen, "a click on a button that has landed presses it");
            // (Hurried, not Finished: on a slow machine the last button can land by the clock during the click's frames.)
            Check(!title.Hurried, "...and doesn't cut the rest short");
            // Esc with the collection open over the rest still coming out: it all lands, and the collection still closes.
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, PhysicalKeycode = Key.Escape, Pressed = true });
            await Frame();
            await Frame();
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, PhysicalKeycode = Key.Escape, Pressed = false });
            await Wait(0.4);
            Check(title.Finished && title.Settled, "a key with the collection open puts everything in place");
            Check(!title.CollectionOpen, "...and still closes the collection");

            // Back to the title (quicker now): a key while the chest's still straining puts everything in its place.
            title = await Title();
            await Wait(0.1);
            Check(!title.Finished, "the quicker burst is still building at 0.1 s");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Space, PhysicalKeycode = Key.Space, Pressed = true });
            await Frame();
            await Frame();
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Space, PhysicalKeycode = Key.Space, Pressed = false });
            Check(title.Finished && title.Settled, "a key finishes it at once, every button in place and clickable");

            // A click where Settings will land, before it has: everything's out, and Settings isn't pressed.
            title = await Title();
            await Wait(0.1);
            var settings = title.SettingsButton;
            await Click(title.HomeOf(settings));
            await Wait(0.3);
            Check(title.Finished && title.Settled, "a click finishes it at once");
            Check(!title.SettingsOpen, "...without pressing the button that lands under the pointer");
            await Click(title.HomeOf(settings));
            await Wait(0.3);
            Check(title.SettingsOpen, "a click after that is a click");

            // A controller's button does it too.
            title = await Title();
            await Wait(0.1);
            Input.ParseInputEvent(new InputEventJoypadButton { ButtonIndex = JoyButton.B, Pressed = true });
            await Frame();
            await Frame();
            Input.ParseInputEvent(new InputEventJoypadButton { ButtonIndex = JoyButton.B, Pressed = false });
            Check(title.Finished && title.Settled, "a controller button finishes it at once");
            await Wait(0.4);
        }
        catch (Exception e)
        {
            Check(false, $"the test ran to the end ({e.Message})");
        }
        GD.Print(failures == 0 ? "Title test: all passed" : $"Title test: {failures} failed");
        QuitGame(failures == 0 ? 0 : 1);
    }
}
