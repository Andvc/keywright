using System;
using System.Linq;
using Godot;
using Keywright.Core;
using Keywright.Ui;

namespace Keywright.Phases;

/// <summary>
/// Someone at the counter (or the back door after dark) to talk, not to sell: a tale's beat, a tool offered, the night's
/// news (docs/stories.md). They say their piece a line at a time (click Go on, or the act key), with Bram's word in his
/// corner; a letter is read off the page. Then the choices, each with what it costs, where the slip would be. What you
/// choose is answered, and what it changed (gold, and where you stand with whom) is shown under it; then they go.
/// </summary>
public partial class TalkPhase : Phase
{
    const float PanelX = 820, PanelY = 120, PanelW = 436;

    readonly Talk _talk;
    SellerView _person;
    MentorView _mentor;
    Label _letterText;
    Control _panel;
    Button _next;
    int _line = -1;
    bool _answered, _leaving;
    int _goldBefore, _swayedBefore, _logBefore;

    /// <summary>The tale this talk belongs to, if it's a beat.</summary>
    string Tale => TaleBook.Find(_talk.Key)?.Tale;

    /// <summary>How many lines the tale's story has in the notebook so far.</summary>
    int TaleLines => Tale != null && State.TaleLog.TryGetValue(Tale, out var log) ? log.Count : 0;

    public TalkPhase(Talk talk) => _talk = talk;

    public TalkPhase() { }

    public Talk Talk => _talk;

    /// <summary>The choices are up and waiting (the bot answers them).</summary>
    public bool Asking => _panel != null && !_answered;

    /// <summary>Answered: only the leaving is left.</summary>
    public bool Answered => _answered;

    /// <summary>Who's speaking a line: Bram speaks from his corner; anyone else stands at the counter.</summary>
    string Speaker(int line) => line < _talk.Lines.Count ? _talk.Lines[line].Speaker : _talk.Who;

    /// <summary>Who stands at the counter: whoever speaks first that isn't Bram (the Magpie at the back door is someone you know).</summary>
    string Person => _talk.Lines.Select(l => l.Speaker).FirstOrDefault(s => s != Story.Bram) ?? _talk.Who;

    public override void _Ready()
    {
        var hour = _talk.Night ? ShopHour.Night : ShopView.CustomerHour(State);
        var shop = Add(UiKit.MakeShop(State.Tier, _talk.Night ? 0.8f : 1f, hour, State, transition: true));
        if (_talk.Letter)
        {
            // A letter, on the counter: read off the page in ink.
            var page = Add(UiKit.MakePanel(Palette.Gold.Darkened(0.4f)).Place(150, 150, 560, 300));
            page.AddChild(new ColorRect { Color = Palette.Paper, MouseFilter = Control.MouseFilterEnum.Ignore }.Place(10, 10, 540, 280));
            page.AddChild(UiKit.MakeLabel(_talk.Night ? Loc.T("A LETTER, AFTER DARK") : Loc.T("A LETTER"), 16, Palette.Ink, HorizontalAlignment.Center)
                .Place(10, 20, 540, 20));
            _letterText = UiKit.MakeLabel("", 16, Palette.Ink).Wrapped();
            page.AddChild(_letterText.Place(34, 52, 492, 230));
        }
        else
        {
            _person = Add(new SellerView(Sellers.Person(Person)));
            shop.Light(_person.Figure);
            _person.Enter();
            Sfx.PlayGroup(_talk.Night ? "door_close" : "door_open", 1f, -6);
        }
        string title = _talk.Offer != null ? Loc.T("AN OFFER") : TaleBook.Find(_talk.Key) is Beat beat && TaleBook.Get(beat.Tale) is TaleDef tale
            ? tale.Title.ToUpperInvariant() : _talk.Night ? Loc.T("AFTER DARK") : null;
        if (title != null)
            Add(UiKit.MakeLabel(title, 24, _talk.Night ? Palette.Magic : Palette.Gold, HorizontalAlignment.Center, outline: 6).Place(PanelX, 64, PanelW, 32));
        _mentor = Add(new MentorView { Position = new Vector2(322, 588), ZIndex = 40 });
        _next = Add(UiKit.MakeButton(Loc.T("Go on"), Palette.Text, 16, Advance).Place(PanelX + PanelW - 180, 620, 180, 48));
        _goldBefore = State.Gold;
        _swayedBefore = State.Swayed.Count;
        _logBefore = TaleLines;
        if (!_talk.Night) BuildQueue();
        // Bram's word on what these are, the first time: people with troubles, and people with tools.
        if (_talk.Offer != null) _mentor.Teach(State, "offer");
        else if (!_talk.Night) _mentor.Teach(State, "tales");
        Game.Hud.SetGold(State.Gold, animate: false);
        Next();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed(Controls.Act) || _leaving) return;
        GetViewport().SetInputAsHandled();
        Advance();
    }

    /// <summary>Go on: the next line, or (once answered) goodbye.</summary>
    void Advance()
    {
        if (_answered) Leave();
        else if (_panel == null) Next();
    }

    /// <summary>The next line, or the choices once every line is said.</summary>
    public void Next()
    {
        if (_panel != null || _leaving) return;
        _line++;
        if (_line >= _talk.Lines.Count)
        {
            Ask();
            return;
        }
        var line = _talk.Lines[_line];
        ScenePhase.Trace?.Invoke($"{Story.Name(line.Speaker)}: {line.Text}");
        if (line.Speaker == Story.Bram && !_talk.Letter) _mentor.Say(line.Text);
        else Say(line.Text);
        Sfx.Play("tick", 1f, -10);
        // The last line: the choices come with it.
        if (_line == _talk.Lines.Count - 1) Ask();
    }

    void Say(string text, Color? color = null)
    {
        if (_letterText != null) _letterText.Text = text;
        else _person.Say(text, color);
    }

    /// <summary>
    /// What you can say: a button each, with what it costs, where the slip would be. News with nothing to answer happens
    /// as its last line is said, and Go on (or Good night) sees them off.
    /// </summary>
    void Ask()
    {
        if (_panel != null || _answered) return;
        var options = _talk.Options;
        if (_talk.Choices.Count == 0)
        {
            Pick(options[0]);
            return;
        }
        _next.Visible = false;
        float h = 60 + options.Count * 60;
        _panel = Add(UiKit.MakePanel(_talk.Night ? Palette.Magic.Darkened(0.2f) : Palette.Gold.Darkened(0.2f)).Place(PanelX, PanelY, PanelW, h));
        _panel.AddChild(UiKit.MakeLabel(Loc.T("WHAT DO YOU SAY?"), 16, Palette.Gold, HorizontalAlignment.Center).Place(0, 14, PanelW, 20));
        for (int i = 0; i < options.Count; i++)
        {
            var choice = options[i];
            bool short_ = State.Gold < choice.Cost;
            var button = UiKit.MakeButton(choice.Label, choice.Cost > 0 ? Palette.Gold : Palette.Text, 16, () => Pick(choice)).Place(20, 46 + i * 60, PanelW - 40, 50);
            button.Disabled = short_;
            if (short_) button.TooltipText = Loc.T($"You need {choice.Cost}g.");
            _panel.AddChild(button);
        }
        Game.Moment($"30_talk_{_talk.Key.Replace(':', '_')}", 0.5);
    }

    /// <summary>Says it: the person answers, and what it changed is shown. Returns false if it couldn't be chosen.</summary>
    public bool Pick(Choice choice)
    {
        if (_answered || _leaving || choice == null || State.Gold < choice.Cost) return false;
        string reply = State.Choose(_talk, choice);
        if (!_talk.Done) return false;
        _answered = true;
        ScenePhase.Trace?.Invoke($"> {choice.Label}" + (reply != null ? $" / {reply}" : ""));
        if (reply != null) Say(reply, Palette.Good);
        if (_panel != null) Sfx.Play("chime", 1f, -8);
        _panel?.QueueFree();
        ShowNoted(ShowWhatChanged());
        Game.Hud.SetGold(State.Gold);
        Game.Hud.SetDue(State);
        _next.Visible = true;
        _next.Text = _talk.Night ? Loc.T("Good night") : Loc.T("Go on");
        Game.Moment($"31_answered_{_talk.Key.Replace(':', '_')}", 0.6);
        return true;
    }

    /// <summary>Gold, and every account that moved, where the choices were. Returns where the next thing can go.</summary>
    float ShowWhatChanged()
    {
        int gold = State.Gold - _goldBefore;
        var lines = State.Swayed.Skip(_swayedBefore).GroupBy(s => s.Faction)
            .Select(g => (g.Key, By: g.Sum(s => s.By))).Where(g => g.By != 0)
            .Select(g => (Text: Loc.T($"{Factions.Name(g.Key)}: {(g.By > 0 ? "+" : "")}{g.By}, {Factions.Word(State.Standing(g.Key))}"),
                          Color: g.By > 0 ? Palette.Good : Palette.Warn)).ToList();
        if (gold != 0) lines.Insert(0, (gold > 0 ? Loc.T($"+{gold}g") : Loc.T($"{gold}g"), gold > 0 ? Palette.Gold : Palette.Bad));
        if (_talk.Offer != null && State.Has(_talk.Offer)) lines.Insert(0, (Loc.T($"Bought: {Upgrades.Get(_talk.Offer).Name}"), Palette.Good));
        if (lines.Count == 0) return PanelY;
        float h = 24 + lines.Count * 28;
        var box = Add(UiKit.MakePanel(Palette.PanelBorder).Place(PanelX, PanelY, PanelW, h));
        for (int i = 0; i < lines.Count; i++)
            box.AddChild(UiKit.MakeLabel(lines[i].Text, 16, lines[i].Color).Place(16, 12 + i * 28, PanelW - 32, 24));
        if (gold != 0) FloatText(gold > 0 ? Loc.T($"+{gold}g") : Loc.T($"{gold}g"), new Vector2(PanelX + PanelW / 2, PanelY - 20), gold > 0 ? Palette.Gold : Palette.Bad, 24);
        return PanelY + h + 12;
    }

    /// <summary>What the notebook's Town page now says of the tale, under what changed: it's written down, and it'll come back.</summary>
    void ShowNoted(float y)
    {
        if (Tale == null || TaleLines <= _logBefore) return;
        string line = State.TaleLog[Tale][^1];
        var note = Add(UiKit.MakePanel(Palette.Gold.Darkened(0.45f)).Place(PanelX, y, PanelW, 124));
        note.AddChild(UiKit.MakeLabel(Loc.T($"IN YOUR NOTEBOOK {Controls.Tag(Controls.Notebook)}"), 16, Palette.Gold).Place(16, 12, PanelW - 32, 20));
        var text = UiKit.MakeLabel(line, 16, Palette.Text).Wrapped();
        text.MaxLinesVisible = 4;
        text.ClipText = true;
        note.AddChild(text.Place(16, 36, PanelW - 32, 84));
    }

    /// <summary>Who's still waiting today, along the counter's edge (as at the counter).</summary>
    void BuildQueue()
    {
        if (State.Queue.Count == 0) return;
        Add(UiKit.MakeLabel(Loc.T("Waiting"), 16, Palette.Muted, outline: 4).Place(24, 616, 120, 20));
        for (int i = 0; i < Math.Min(7, State.Queue.Count); i++)
        {
            var face = Add(UiKit.MakeSprite($"res://assets/sprites/sellers/faces/{State.Queue[i].Seller.Look}.png", new Vector2(44 + i * 38, 668), 1, outlined: true));
            face.Modulate = new Color(0.8f, 0.8f, 0.85f);
        }
    }

    /// <summary>They go: the next customer, or (at night) the rest of the night.</summary>
    public async void Leave()
    {
        if (_leaving || !_answered) return;
        _leaving = true;
        if (_person != null)
        {
            Sfx.PlayGroup("door_close", 1.1f, -8);
            await ToSignal(_person.Leave(), Tween.SignalName.Finished);
            if (!Alive) return;
        }
        if (_talk.Night) Game.NextNight();
        else Game.NextCustomer();
    }
}
