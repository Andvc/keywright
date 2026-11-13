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

    /// <summary>The line at the counter (or on the page) cut into sentences, and how far it's been said (0.35).</summary>
    System.Collections.Generic.List<string> _sentences = new();
    int _at;
    int _goldBefore, _swayedBefore, _logBefore, _keepsakesBefore;

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
        // A tale's first beat says so: a new story is starting, and the notebook will follow it.
        if (Tale != null && TaleLines == 0 && _talk.Offer == null)
            Add(UiKit.MakeLabel(Loc.T("A NEW TALE BEGINS"), 16, Palette.Muted, HorizontalAlignment.Center, outline: 4).Place(PanelX, 96, PanelW, 20));
        // The echo horn at the counter's end, for what's been said so far (0.35).
        shop.Light(Add(new EchoHorn(State) { Position = EchoHorn.AtCounter }));
        _mentor = Add(new MentorView { Position = new Vector2(322, 588), ZIndex = 40 });
        _next = Add(UiKit.MakeButton(Loc.T("Go on"), Palette.Text, 16, Advance).Place(PanelX + PanelW - 180, 620, 180, 48));
        _goldBefore = State.Gold;
        _swayedBefore = State.Swayed.Count;
        _logBefore = TaleLines;
        _keepsakesBefore = State.Keepsakes.Count;
        if (!_talk.Night) BuildQueue();
        // Bram's word on what these are, the first time: people with troubles, and people with tools.
        if (_talk.Offer != null) _mentor.Teach(State, "offer");
        else if (!_talk.Night) _mentor.Teach(State, "tales");
        Game.Hud.SetGold(State.Gold, animate: false);
        // Their first line, once Bram's word (if he has one, the first time) has been said: until 0.36 a click read him
        // out a sentence at a time and then them; now that a speech says itself, theirs has to start itself too (the
        // fresh-eyes playtest sat at a blank bubble until it pressed Go on).
        _opening = true;
    }

    bool _opening;

    /// <summary>A knock at the back door or a letter after dark: the night's music, not the market's.</summary>
    public bool AtNight => _talk?.Night ?? false;

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

    /// <summary>
    /// The next sentence of the line being said (Bram finishes his in his corner first), then the next line; the choices
    /// once every line is said (with the last sentence of the last).
    /// </summary>
    public void Next()
    {
        if (_panel != null || _leaving) return;
        bool opening = _opening;
        _opening = false;
        // (Not on the way in: Bram's word says itself, and their first line follows it.)
        if (_mentor.MidText && !opening) _mentor.Next();
        // Still saying it: the rest of the line at once.
        else if (_letterText == null && _person.MidSpeech) _person.FinishSpeech();
        else if (_letterText != null && _line >= 0 && _line < _talk.Lines.Count && _at < _sentences.Count - 1)
        {
            _at = _sentences.Count - 1;
            ShowSentence();
        }
        else
        {
            _line++;
            if (_line >= _talk.Lines.Count)
            {
                Ask();
                return;
            }
            var line = _talk.Lines[_line];
            ScenePhase.Trace?.Invoke($"{Story.Name(line.Speaker)}: {line.Text}");
            State.Hear(Story.Name(line.Speaker), line.Text, Heard.Dialogue);
            _at = 0;
            if (line.Speaker == Story.Bram && !_talk.Letter)
            {
                _sentences = new() { line.Text };
                _mentor.Say(line.Text);
            }
            else
            {
                _sentences = Words.Sentences(line.Text);
                if (_sentences.Count == 0) _sentences.Add(line.Text);
                ShowSentence();
            }
            Sfx.Play("tick", 1f, -10);
        }
        // The last line, said to its end: the choices come with it.
        if (_line == _talk.Lines.Count - 1 && _at >= _sentences.Count - 1 && !_mentor.MidText) Ask();
    }

    /// <summary>The sentence the line's got to: in the bubble, one at a time; on a letter's page, the page so far.</summary>
    void ShowSentence()
    {
        if (_letterText != null)
        {
            _letterText.Text = string.Join(" ", _sentences.Take(_at + 1));
            _nextIn = _at < _sentences.Count - 1 ? Words.ReadTime(_sentences[_at]) : 0;
        }
        // In the bubble the whole line, which says itself a sentence after another (0.36: no click a sentence).
        else
        {
            _person.Say(string.Join(" ", _sentences), record: false);
            _at = _sentences.Count - 1;
        }
    }

    double _nextIn;

    /// <summary>A letter's page fills in by itself, a sentence after another.</summary>
    public override void _Process(double delta)
    {
        if (_opening && !_mentor.MidText && !_leaving) Next();
        if (_nextIn <= 0 || _letterText == null || _leaving) return;
        _nextIn -= delta;
        if (_nextIn > 0 || _at >= _sentences.Count - 1) return;
        _at++;
        ShowSentence();
        if (_line == _talk.Lines.Count - 1 && _at >= _sentences.Count - 1 && _panel == null && !_answered) Ask();
    }

    void Say(string text, Color? color = null)
    {
        if (_letterText != null) _letterText.Text = text;
        else _person.Say(text, color, record: false);
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
        // What you said, and their answer, into the run's history.
        State.Hear(Characters.Get(State.Character)?.Name ?? "", choice.Label, Heard.Dialogue);
        if (reply != null)
        {
            State.Hear(Story.Name(Person), reply, Heard.Dialogue);
            Say(reply, Palette.Good);
        }
        if (_panel != null) Sfx.Play("chime", 1f, -8);
        _panel?.QueueFree();
        ShowKept(ShowNoted(ShowWhatChanged()));
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

    /// <summary>What the notebook's Town page now says of the tale, under what changed: it's written down, and it'll come back. Returns where the next thing can go.</summary>
    float ShowNoted(float y)
    {
        if (Tale == null || TaleLines <= _logBefore) return y;
        string line = State.TaleLog[Tale][^1];
        var note = Add(UiKit.MakePanel(Palette.Gold.Darkened(0.45f)).Place(PanelX, y, PanelW, 124));
        note.AddChild(UiKit.MakeLabel(Loc.T($"IN YOUR NOTEBOOK {Controls.Tag(Controls.Notebook)}"), 16, Palette.Gold).Place(16, 12, PanelW - 32, 20));
        var text = UiKit.MakeLabel(line, 16, Palette.Text).Wrapped();
        text.MaxLinesVisible = 4;
        text.ClipText = true;
        note.AddChild(text.Place(16, 36, PanelW - 32, 84));
        return y + 124 + 12;
    }

    /// <summary>
    /// A keepsake given (0.32): its own card, over the scene where the slip would be, with its picture and what it does,
    /// so the gift lands as a gift, not a line of text.
    /// </summary>
    public static string KeptHeading(KeepsakeDef keepsake) => Loc.T($"A KEEPSAKE: {keepsake.Name.ToUpperInvariant()}");

    /// <summary>A keepsake card's description box, right of its picture (four lines; the fit test).</summary>
    public static (float W, float H) KeptBox => (PanelW - 104, 98);
    /// <summary>Its heading's width: across the top, over the picture too, for a long name in a longer language.</summary>
    public static float KeptHeadingW => PanelW - 24;

    void ShowKept(float y)
    {
        var given = State.Keepsakes.Skip(_keepsakesBefore).Select(Keepsakes.Get).ToList();
        foreach (var keepsake in given)
        {
            var card = Add(UiKit.MakePanel(Palette.Keepsake.Darkened(0.35f)).Place(PanelX, Math.Min(y, 560 - 136), PanelW, 136));
            card.TooltipText = UiKit.KeepsakeTooltip(keepsake);
            card.MouseFilter = Control.MouseFilterEnum.Stop;
            card.AddChild(UiKit.MakeKeepsakeIcon(keepsake, new Vector2(48, 68), 64));
            card.AddChild(UiKit.MakeLabel(KeptHeading(keepsake), 16, Palette.Keepsake).Place(12, 8, KeptHeadingW, 20));
            var blurb = UiKit.MakeLabel(keepsake.Blurb, 16, Palette.Text).Wrapped();
            blurb.MaxLinesVisible = 4;
            blurb.ClipText = true;
            blurb.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            card.AddChild(blurb.Place(92, 34, KeptBox.W, KeptBox.H));
            card.PivotOffset = new Vector2(PanelW / 2, 68);
            card.Scale = Vector2.One * 0.6f;
            card.CreateTween().TweenProperty(card, "scale", Vector2.One, 0.3f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            Fx.Burst(this, new Vector2(PanelX + 48, Math.Min(y, 560 - 136) + 68), Palette.Keepsake, 40, 200f);
            Sfx.Play("fanfare", 1.2f, -6);
            y += 148;
        }
        if (given.Count > 0) Game.Moment($"32_keepsake_{given[0].Key}", 0.5);
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
