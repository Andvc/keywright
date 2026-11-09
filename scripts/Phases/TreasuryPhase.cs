using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>Which treasury it is: the first time, again once you remember, or the final opening with the plan whole.</summary>
public enum TreasuryMode { First, Again, Final }

/// <summary>What came of it.</summary>
public enum TreasuryResult { Swallowed, Stayed, DoorLeft, DoorClosed }

/// <summary>
/// The Guild's treasury (0.34, docs/narrative.md): its great chest, which is the Maw, at a counter, with the counter's own
/// tools. The first time: you shake it and nothing moves; harder, and not even the ring an empty chest gives; you drop
/// it, and nothing booms; it slips out of your hands; the lid rises and falls; it's breathing; and it opens towards you
/// and swallows the counter, the slip and the screen. Then darkness, and two taps on the counter, and one. Once you
/// remember, you can stay away instead. With the plan whole you don't shake it: you hold still and listen, the people
/// inside knock, you knock back, you turn the key to look and not to own, they come out, and the Maw makes its last
/// offer. Shaking is dragging the chest with the mouse (or a controller's pointer), as at the counter.
/// </summary>
public partial class TreasuryPhase : Phase
{
    public override bool HoldsNotes => true;

    readonly TreasuryMode _mode;
    ChestView _chest;
    Label _caption, _prompt, _aside;
    Control _buttons;
    ColorRect _dark;
    Vector2 _home;
    bool _dragging, _busy, _knocked, _listening;
    Vector2 _grab, _lastMouse;
    float _energy, _still;
    int _step;

    /// <summary>For the bot: where the scene is, and whether it's waiting for a choice.</summary>
    public int Step => _step;
    public bool Choosing { get; private set; }
    public bool Finished { get; private set; }
    public TreasuryMode Mode => _mode;

    // How much shaking it takes (the sum of the chest's speed, pixels a second, over the frames it's dragged).
    const float Light = 9000, Hard = 26000;

    public TreasuryPhase() { }

    public TreasuryPhase(TreasuryMode mode) => _mode = mode;

    public override void _Ready()
    {
        Add(UiKit.MakeBackdrop("res://assets/backgrounds/scene_vault.png", 0.5f));
        Add(UiKit.MakeLabel(Loc.T("THE TREASURY"), 32, Palette.Gold, HorizontalAlignment.Center, outline: 6).Place(0, 72, 1280, 40));
        _home = new Vector2(640, 360);
        _chest = new ChestView { PixelSize = 3, Skin = "crown", Position = _home, Lock = 3 };
        Add(_chest);
        _caption = Add(UiKit.MakeLabel("", 24, Palette.Text, HorizontalAlignment.Center, outline: 6).Wrapped().Place(140, 500, 1000, 64));
        _prompt = Add(UiKit.MakeLabel("", 16, Palette.Gold, HorizontalAlignment.Center, outline: 4).Wrapped().Place(140, 570, 1000, 44));
        _aside = Add(UiKit.MakeLabel("", 16, Palette.Muted, HorizontalAlignment.Center, outline: 4).Wrapped().Place(140, 470, 1000, 24));
        _buttons = Add(new Control { MouseFilter = Control.MouseFilterEnum.Ignore, ZIndex = 60 }.Place(0, 0, 1280, 720));
        _dark = new ColorRect { Color = new Color(0, 0, 0, 0), MouseFilter = Control.MouseFilterEnum.Ignore, ZIndex = 50 };
        Add(_dark.Place(0, 0, 1280, 720));
        Game.Hud.Visible = false;
        switch (_mode)
        {
            case TreasuryMode.First:
                Say(Loc.T("The treasury's great chest: Bram's commission."), Loc.T("Shake it: hold the mouse button on the chest, and move."));
                break;
            case TreasuryMode.Again:
                Say(Loc.T("The great chest. You know what's in it."), null);
                Choose((Loc.T("Open it again"), () => OpenAgain()), (Loc.T("Stay away tonight"), () => StayAway()));
                break;
            default:
                Say(Loc.T("The great chest. Don't shake it this time."), Loc.T("Hold still, and listen."));
                _listening = true;
                break;
        }
        Game.Moment($"25_treasury_{_mode.ToString().ToLowerInvariant()}", 0.6);
    }

    void Say(string caption, string prompt)
    {
        _caption.Text = caption ?? "";
        _prompt.Text = prompt ?? "";
        ScenePhase.Trace?.Invoke($"[treasury] {caption}");
    }

    (string Label, System.Action Do)[] _options = System.Array.Empty<(string, System.Action)>();

    /// <summary>Buttons for what you can do now (one, or a choice), in a row near the bottom (above the dark).</summary>
    void Choose(params (string Label, System.Action Do)[] options)
    {
        foreach (var child in _buttons.GetChildren()) child.QueueFree();
        _options = options;
        Choosing = options.Length > 0;
        float w = 320, gap = 24, x = 640 - (options.Length * w + (options.Length - 1) * gap) / 2;
        for (int i = 0; i < options.Length; i++)
        {
            int pick = i;
            _buttons.AddChild(UiKit.MakeButton(options[i].Label, Palette.Gold, 16, () => Pick(pick)).Place(x, 626, w, 52));
            x += w + gap;
        }
    }

    /// <summary>Takes one of the buttons up now (a click, or the bot).</summary>
    public void Pick(int i)
    {
        if (!Choosing || i < 0 || i >= _options.Length) return;
        Choosing = false;
        var act = _options[i].Do;
        _options = System.Array.Empty<(string, System.Action)>();
        foreach (var child in _buttons.GetChildren()) child.QueueFree();
        act();
    }

    // ---- Shaking (the first time) ----

    public override void _Input(InputEvent e)
    {
        if (_busy || Finished) return;
        var mouse = GetGlobalMousePosition();
        if (e is InputEventMouseButton { ButtonIndex: MouseButton.Left } button)
        {
            bool onChest = mouse.DistanceTo(_chest.Position) < 170;
            if (button.Pressed && onChest)
            {
                if (_listening)
                {
                    // Not this time: you came to listen.
                    _still = 0;
                    _aside.Text = Loc.T("Still. Don't shake it. Listen.");
                    return;
                }
                if (_mode != TreasuryMode.First || _step >= 2) return;
                _dragging = true;
                _grab = mouse - _chest.Position;
                _lastMouse = mouse;
            }
            else if (!button.Pressed) _dragging = false;
        }
        if (_listening && e.IsActionPressed(Controls.Act) && _knocked && Choosing) Pick(0);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        if (_dragging && !_busy)
        {
            var mouse = GetGlobalMousePosition();
            var target = mouse - _grab;
            // The chest is heavy: it follows the hand within reach of where it sits.
            var offset = (target - _home).LimitLength(90);
            var before = _chest.Position;
            _chest.Position = _chest.Position.Lerp(_home + offset, Mathf.Min(1, dt * 14));
            float speed = (_chest.Position - before).Length() / Mathf.Max(dt, 0.001f);
            Shake(speed * dt * 60f);
            _lastMouse = mouse;
        }
        else if (!_busy && _mode == TreasuryMode.First && _step < 2)
            _chest.Position = _chest.Position.Lerp(_home, Mathf.Min(1, dt * 6));
        if (_listening && !_busy)
        {
            _still += dt;
            if (!_knocked && _still >= 3f) HearKnocks();
        }
    }

    /// <summary>Some shaking (by the hand or the bot): nothing moves inside. Ever.</summary>
    public void Shake(float amount)
    {
        if (_mode != TreasuryMode.First || _step >= 2 || amount <= 0) return;
        _energy += amount;
        _chest.Wobble(Mathf.Min(1, amount / 400f));
        if (_step == 0 && _energy > Light)
        {
            _step = 1;
            Say(Loc.T("Nothing moves inside."), Loc.T("Harder."));
        }
        else if (_step == 1 && _energy > Hard)
        {
            _step = 2;
            _dragging = false;
            Say(Loc.T("Nothing. Not even the wooden ring every empty chest gives."), null);
            Choose((Loc.T("Drop it"), Drop));
        }
    }

    /// <summary>Dropped: no boom, no ring. Then it slips, and breathes, and opens.</summary>
    public async void Drop()
    {
        if (_step != 2 || _busy) return;
        _busy = true;
        _step = 3;
        var tween = CreateTween();
        tween.TweenProperty(_chest, "position:y", _home.Y + 26, 0.18f).SetEase(Tween.EaseType.In);
        tween.TweenProperty(_chest, "position:y", _home.Y + 20, 0.12f);
        await Wait(0.4);
        if (!Alive) return;
        Say(Loc.T("No boom. No ring. Nothing at all."), null);
        await Wait(1.8);
        if (!Alive) return;
        // It slips out of the hands, out of where a chest sits to be judged.
        var slip = CreateTween().SetParallel();
        slip.TweenProperty(_chest, "position", _home + new Vector2(70, 46), 0.5f).SetEase(Tween.EaseType.In);
        slip.TweenProperty(_chest, "rotation", 0.09f, 0.5f);
        Say(Loc.T("It slips out of your hands."), null);
        await Wait(1.6);
        if (!Alive) return;
        await Breathing();
    }

    async System.Threading.Tasks.Task Breathing()
    {
        _step = 4;
        _chest.Breathe();
        Sfx.Play("breath", 0.85f, -2);
        Say(Loc.T("The lid rises. And falls."), null);
        await Wait(2.4);
        if (!Alive) return;
        _chest.Breathe();
        Sfx.Play("breath", 0.8f, 0);
        Say(Loc.T("It's breathing."), null);
        Game.Moment("25_treasury_breathing", 0.5);
        await Wait(3.0);
        if (!Alive) return;
        Swallow();
    }

    /// <summary>It opens towards you, and swallows the slip, the counter, the screen. Darkness, then two taps and one.</summary>
    async void Swallow()
    {
        _step = 5;
        _busy = true;
        Say("", null);
        Sfx.Play("growl", 0.6f, 2);
        Game.Shake(0.9f);
        var tween = CreateTween().SetParallel();
        tween.TweenProperty(_chest, "scale", Vector2.One * 7f, 1.5f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.In);
        tween.TweenProperty(_chest, "position", new Vector2(640, 520), 1.5f).SetEase(Tween.EaseType.In);
        tween.TweenMethod(Callable.From<float>(g => _chest.LidGap = g), 0f, 160f, 1.2f).SetDelay(0.3f);
        tween.TweenProperty(_dark, "color:a", 1f, 1.1f).SetDelay(0.6f);
        Game.Moment("25_treasury_swallow", 0.9);
        await Wait(2.4);
        if (!Alive) return;
        _chest.Visible = false;
        await Wait(1.2);
        if (!Alive) return;
        // In the dark, a sound you know.
        foreach (float gap in new[] { 0f, 0.22f, 0.55f })
        {
            await Wait(gap + 0.05);
            if (!Alive) return;
            Sfx.Play("tap", 1f, -2);
        }
        var line = UiKit.MakeLabel(Loc.T("Two taps on the counter. And one."), 24, Palette.Text, HorizontalAlignment.Center, outline: 6)
            .Place(140, 330, 1000, 40);
        line.ZIndex = 60;
        Add(line);
        Game.Moment("25_treasury_dark", 0.6);
        await Wait(1.2);
        if (!Alive) return;
        Choose((Loc.T("Open your eyes"), () => End(TreasuryResult.Swallowed)));
    }

    // ---- Again ----

    async void OpenAgain()
    {
        _busy = true;
        Say(Loc.T("You don't need to shake it. It's breathing already."), null);
        _chest.Breathe();
        Sfx.Play("breath", 0.8f, 0);
        await Wait(2.4);
        if (!Alive) return;
        Swallow();
    }

    async void StayAway()
    {
        _busy = true;
        Say(Loc.T("You leave the key on the Guild's table and walk home. Behind you, the treasury breathes."),
            Loc.T("They'll open it with Bram. And then it'll be spring."));
        _chest.Breathe();
        Sfx.Play("breath", 0.7f, -6);
        await Wait(3.0);
        if (!Alive) return;
        End(TreasuryResult.Stayed);
    }

    // ---- The final opening ----

    void HearKnocks()
    {
        _knocked = true;
        _aside.Text = "";
        Knocks(5, 0.95f);
        Say(Loc.T("A knock. Then another, and another. People."), Loc.T($"Knock back {Controls.Tag(Controls.Act)}"));
        Choose((Loc.T("Knock back"), KnockBack));
        Game.Moment("25_treasury_knocks", 0.8);
    }

    async void Knocks(int count, float pitch)
    {
        for (int i = 0; i < count; i++)
        {
            Sfx.Play("knock", pitch + (i % 3) * 0.06f, -4);
            await Wait(0.32 + (i % 2) * 0.25);
            if (!Alive) return;
        }
    }

    /// <summary>You knock back, and they answer.</summary>
    public async void KnockBack()
    {
        if (!_knocked || _busy) return;
        _busy = true;
        foreach (float gap in new[] { 0f, 0.22f, 0.55f })
        {
            await Wait(gap + 0.05);
            if (!Alive) return;
            Sfx.Play("tap", 1f, -2);
        }
        await Wait(0.7);
        if (!Alive) return;
        Knocks(7, 1.05f);
        Say(Loc.T("They answer. Everyone the Guild weighed and crated is in there, and they heard you."), null);
        await Wait(2.4);
        if (!Alive) return;
        _busy = false;
        Choose((Loc.T("Turn the key: to look, not to own"), TurnKey));
    }

    /// <summary>The key turns as one who's come to look: the lid lifts, and they come out.</summary>
    public async void TurnKey()
    {
        _busy = true;
        Sfx.Play("pin", 0.7f, -2);
        var tween = CreateTween().SetParallel();
        tween.TweenMethod(Callable.From<float>(g => _chest.LidGap = g), 0f, 40f, 1.2f).SetEase(Tween.EaseType.Out);
        tween.TweenMethod(Callable.From<float>(g => _chest.Glow = g), 0f, 0.8f, 1.2f);
        Say(Loc.T("They come out into the lamplight, gold flaking off them like old paint: cold, and blinking, and alive."),
            State.PlanWayOut ? Loc.T("Your friends at the door wrap them in coats and take them home.") : null);
        Game.Moment("25_treasury_out", 1.4);
        await Wait(3.2);
        if (!Alive) return;
        Offer();
    }

    /// <summary>The Maw's last offer: leave the door to it, or close it.</summary>
    async void Offer()
    {
        _busy = false;
        Sfx.Play("growl", 0.5f, -6);
        _aside.Text = "";
        Say(Story.FinalOffer(State), null);
        _caption.Modulate = Palette.Gold.Lightened(0.3f);
        Choose((Loc.T("Leave the door to it"), () => End(TreasuryResult.DoorLeft)), (Loc.T("Close the door"), CloseDoor));
        Game.Moment("25_treasury_offer", 0.6);
        await Wait(7.0);
        if (!Alive || !Choosing) return;
        // The Maw's whisper, in its own gold: quiet, but not lost (0.34's playtest found the muted grey hard to read).
        _aside.AddThemeColorOverride("font_color", Palette.Gold);
        _aside.Text = Story.Hesitation;
    }

    async void CloseDoor()
    {
        _busy = true;
        _caption.Modulate = Colors.White;
        _aside.Text = "";
        var tween = CreateTween().SetParallel();
        tween.TweenMethod(Callable.From<float>(g => _chest.LidGap = g), _chest.LidGap, 0f, 0.35f).SetEase(Tween.EaseType.In);
        tween.TweenMethod(Callable.From<float>(g => _chest.Glow = g), _chest.Glow, 0f, 0.6f);
        await Wait(0.35);
        if (!Alive) return;
        Sfx.Play("thud", 0.7f, 0);
        Game.Shake(0.4f);
        Say(Loc.T("You close the door on it, and the seal holds. Nothing turns. Nothing breathes."), null);
        await Wait(2.4);
        if (!Alive) return;
        _busy = false;
        Choose((Loc.T("Go home"), () => End(TreasuryResult.DoorClosed)));
    }

    void End(TreasuryResult result)
    {
        if (Finished) return;
        Finished = true;
        Choosing = false;
        Game.Hud.Visible = true;
        Game.AfterTreasury(result);
    }

    /// <summary>For the bot: plays the scene as a careful player would (with <paramref name="stay"/>, staying away when it can).</summary>
    public void BotStep(bool stay)
    {
        if (Finished) return;
        if (_mode == TreasuryMode.First && _step < 2 && !Choosing)
        {
            Shake(4000);
            return;
        }
        if (!Choosing) return;
        if (_mode == TreasuryMode.Again && _options.Length == 2) Pick(stay ? 1 : 0);
        else if (_mode == TreasuryMode.Final && _options.Length == 2) Pick(1);
        else Pick(0);
    }
}
