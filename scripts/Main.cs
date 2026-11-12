using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Audio;
using Keywright.Core;
using Keywright.Debug;
using Keywright.Phases;
using Keywright.Ui;

namespace Keywright;

/// <summary>
/// Root of the game: owns the game state, audio, HUD and screen effects, and swaps between phases. It opens on the title
/// screen; the shop is saved to user://save.json at the start of each day, and Esc pauses. A day runs: the week's scene
/// (from the second week), Nim's curios on his day, the counter, the day's end, and on the third day the Guild's due.
/// What you've done across runs is kept in user://profile.json.
/// Command-line flags (after `--`): --seed=N, --sim [--n=N], --shake-test, --save-test, --loc-test, --fit-test, --pseudo, --lang=CODE, --font-sheet, --export-sounds=DIR, --autoplay,
/// --shots=DIR, --quit-at-end, --days=N, --no-tutorial (no tips, no scripted first day),
/// --start-tier=N and --gold=N (jump ahead with the tools you'd have by then), --opening-demo (opens a blue chest
/// holding one thing of each quality, two of them better than the chest, and a Heartsong, to preview the opening's
/// effects; quits after with --quit-at-end), --shell-demo (with --shots: the title screen, a new game, the pause menu and the
/// settings, one screenshot each, then quits), --treasury-demo (the treasury on the last night: the first swallowing;
/// --treasury-demo=final, the final opening with the plan whole; quits after with --quit-at-end),
/// --start-week=N (a shop as a careful player would have it by then, played
/// through by the balance simulation; --gold=N then sets its purse). Autoplay sets --days=N to stop after that many days.
/// --character=KEY and --challenge=N play one of Bram's kin, at a challenge level (--terms=a,b: with just those boons and banes).
/// --engine starts with the Bookkeeping Engine, to see it read every chest. --curios=a,b,c owns those curios from the start.
/// --tales=a,b puts those tales in the season, whatever it draws.
/// --heartsongs=P makes a Heartsong turn up in that share of chests; --songs=all plays as if every one had been found
/// --mimics=N makes one chest in N breathe (from Thornwick, once you remember the treasury).
/// --season=N plays a later season (the last one lost, so the year turned back); --knew=all as if every question of the
/// case had been worked out in an earlier season.
/// </summary>
public partial class Main : Node2D
{
    public static Main Instance { get; private set; }

    /// <summary>The game's version: project.godot's application/config/version, which the exports are stamped with.</summary>
    public static string Version => ProjectSettings.GetSetting("application/config/version", "0.27").AsString();
    const string SavePath = "user://save.json", ProfilePath = "user://profile.json";

    public GameState State { get; private set; }

    /// <summary>Every run so far: for the title screen and the end of a run.</summary>
    public Profile Profile { get; private set; }
    public Sfx Sfx { get; private set; }
    public Hud Hud { get; private set; }

    readonly Random _rng = new();
    Camera2D _camera;
    ColorRect _flash, _fade;
    Phase _phase;
    Autoplay _bot;
    string _shotsDir;
    float _trauma;
    int _seed;

    /// <summary>A real game (not the bot or a debugging shortcut), so the shop is saved each day.</summary>
    bool _saving;

    /// <summary>A debugging walk-through (--shell-demo): nothing is saved, banked or learned for real.</summary>
    bool _demo;

    /// <summary>Who could mind the stall, and the levels open to this run's character, when the run began: the run's end says what's new.</summary>
    public string[] KinAtStart { get; private set; } = System.Array.Empty<string>();
    public int LevelsAtStart { get; private set; }

    /// <summary>What Nim carried when the run began: his stock grows as Bram's kin get further.</summary>
    public string[] CuriosAtStart { get; private set; } = System.Array.Empty<string>();

    public override void _Ready()
    {
        Instance = this;
        var args = OS.GetCmdlineUserArgs();
        // --pseudo: every translated text pseudo-translated, the sim's too (its numbers must come out the same).
        Loc.Pseudo = args.Contains("--pseudo");
        if (args.Contains("--sim") || Arg(args, "--sim") != null)
        {
            // A report that throws still quits (a headless run would otherwise wait forever).
            try
            {
                int N(int fallback) => int.Parse(Arg(args, "--n") ?? fallback.ToString());
                // --heartsongs=P: try another chance a chest holds one (for tuning it).
                if (float.TryParse(Arg(args, "--heartsongs"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture,
                        out var songOdds))
                    Heartsongs.Chance = songOdds;
                // --mimics=N: one chest in N breathes (0.33; a large N all but turns them off, to measure what they're worth).
                if (int.TryParse(Arg(args, "--mimics"), out var simMimics) && simMimics > 0) Mimics.OneIn = simMimics;
                // --dues=a,b,c,d: try other dues (for tuning them), and --listens=N another ear.
                if (Arg(args, "--dues") is string dues) Guild.TryDues(dues.Split(',').Select(int.Parse).ToArray());
                if (int.TryParse(Arg(args, "--listens"), out var listens)) GameState.ListensTried = listens;
                // --debtstep=S: try another rise in the Guild's dues a challenge level (for tuning it).
                if (float.TryParse(Arg(args, "--debtstep"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture,
                        out var debtStep))
                    Challenges.DebtStep = debtStep;
                switch (Arg(args, "--sim"))
                {
                    case "songs":
                        Simulation.Songs(N(150));
                        break;
                    case "songcount":
                        Simulation.SongCount(N(300));
                        break;
                    case "sellers":
                        Simulation.PrintSellers();
                        Simulation.LessonTest();
                        break;
                    case "seller":
                        Simulation.ShowSeller(Arg(args, "--who") ?? "pip");
                        break;
                    case "kin":
                        Simulation.Meta(N(100), Arg(args, "--who"));
                        break;
                    case "terms":
                        Simulation.TermsReport(N(200), Arg(args, "--who"));
                        break;
                    case "levels":
                        Simulation.TermLevels(N(200));
                        break;
                    case "careers":
                        Simulation.Careers(N(40), 30, int.TryParse(Arg(args, "--level"), out var careerLevel) ? careerLevel : null);
                        break;
                    case "builds":
                        Simulation.Builds(N(60));
                        break;
                    case "focus":
                        Simulation.FocusReport(Arg(args, "--who") ?? "compartments", N(100), args.Contains("--trace"));
                        break;
                    case "curio":
                        Simulation.CurioCheck((Arg(args, "--who") ?? "seal").Split(','), N(100));
                        break;
                    case "index":
                        Simulation.IndexReport();
                        break;
                    case "curiolist":
                        Simulation.CurioTable();
                        break;
                    case "trace":
                        Simulation.Trace(int.Parse(Arg(args, "--seed") ?? "1000"), Arg(args, "--character"));
                        break;
                    case "experience":
                        Simulation.ExperienceReport(N(30), int.Parse(Arg(args, "--runs") ?? "3"));
                        break;
                    case "quick":
                        Simulation.Run(N(120), quick: true);
                        break;
                    case "learners":
                        Simulation.Learners(N(120));
                        break;
                    default:
                        Simulation.Run(N(300));
                        break;
                }
            }
            catch (System.Exception e)
            {
                GD.PrintErr(e.ToString());
            }
            GetTree().Quit();
            return;
        }
        // --achievements: the table to set them up with in Steamworks (API name, name, description, hidden); with
        // --lang=CODE, in that language, for Steamworks' translations of them.
        if (args.Contains("--achievements"))
        {
            Localization.Apply(lang: Arg(args, "--lang"));
            GD.Print("| API name | Name | Description | Hidden |\n|---|---|---|---|");
            foreach (var a in Achievements.All) GD.Print($"| {a.Key} | {a.Name} | {a.Text} | {(a.Hidden ? "yes" : "")} |");
            GetTree().Quit();
            return;
        }
        if (args.Contains("--save-test") || args.Contains("--loc-test"))
        {
            bool passed = false;
            try
            {
                passed = args.Contains("--loc-test") ? LocTest.Run() : SaveTest.Run();
            }
            catch (System.Exception e)
            {
                GD.PrintErr(e.ToString());
            }
            GetTree().Quit(passed ? 0 : 1);
            return;
        }
        if (args.Contains("--shake-test"))
        {
            Sfx = new Sfx();
            AddChild(Sfx);
            AddChild(new ShakeTest { Count = int.Parse(Arg(args, "--n") ?? "40") });
            return;
        }

        // The language first, before anything with words in it is built; Godot's own translation of what controls
        // show is off, since everything shown is translated already (Loc).
        Settings.Load();
        Localization.Apply(pseudo: args.Contains("--pseudo"), lang: Arg(args, "--lang"));
        GetTree().Root.AutoTranslateMode = AutoTranslateModeEnum.Disabled;
        // Files from an older folder, then the settings again in case they came with them.
        CarryOverOldFiles();
        Settings.Load();
        if (args.Contains("--gm")) Settings.Gm = true;
        Profile = ReadProfile();
        // Steam, if it's there (opening its overlay pauses the game); started outside it, it starts the game itself.
        if (!Platform.Start(shown => { if (shown) OpenPause(); }))
        {
            QuitGame();
            return;
        }
        Platform.Unlock(Profile.Achievements);
        // Steam's callbacks every frame, paused or not (the overlay that paused the game says when it's closed).
        GetTree().ProcessFrame += Platform.Tick;
        // A controller moves and clicks the pointer (the mouse's own, so everything it can do, a controller can).
        AddChild(new GamepadCursor());
        GetTree().Root.Theme = MakeTheme();
        _seed = int.TryParse(Arg(args, "--seed"), out var s) ? s : (int)(Time.GetUnixTimeFromSystem() % 1_000_000);
        // Straight into a day for the bot and the debugging shortcuts; everyone else gets the title screen.
        bool direct = args.Contains("--autoplay") || args.Contains("--opening-demo") || args.Contains("--treasury-demo") || Arg(args, "--treasury-demo") != null ||
                      args.Contains("--pad-test") ||
                      Arg(args, "--start-tier") != null || Arg(args, "--start-week") != null || Arg(args, "--season") != null;
        if (direct)
        {
            // --terms=gifts,thin: just these boons and banes (to see a term's screens), whatever the level deals.
            var terms = Arg(args, "--terms") is string named ? Challenges.Named(named.Split(',')) : ((List<string>, List<string>)?)null;
            State = GameState.NewRun(_seed, Arg(args, "--character") ?? Characters.Kit, int.TryParse(Arg(args, "--challenge"), out var level) ? level : 0,
                !args.Contains("--no-tutorial"), terms: terms);
            if (State.Boons.Count + State.Banes.Count > 0)
                GD.Print($"Terms: for you {(State.Boons.Count > 0 ? string.Join(", ", State.Boons) : "none")}; " +
                         $"against you {(State.Banes.Count > 0 ? string.Join(", ", State.Banes) : "none")}");
            // --songs=all: as if Bram's kin had opened every Heartsong (the Crown Chest's other ending). --heartsongs=0.5:
            // half of all chests hold one, to see them turn up.
            if (Arg(args, "--songs") == "all") State.Songbook.UnionWith(Heartsongs.All.Select(h => h.Key));
            if (float.TryParse(Arg(args, "--heartsongs"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture,
                    out var songChance))
                Heartsongs.Chance = Math.Clamp(songChance, 0f, 1f);
            // --mimics=2: one chest in two breathes (from Thornwick), to see them at the counter and the opening.
            if (int.TryParse(Arg(args, "--mimics"), out var mimics) && mimics > 0) Mimics.OneIn = mimics;
            // --season=2: a later season, the last one lost (the year turned back: Bram's dream, Vane's "we've met before",
            // a chest marked in your own hand). --knew=all: every question of the case worked out in an earlier season
            // (filled in on the Case page, Bram's glove off, and the people who'd have told you notice that you know).
            if (int.TryParse(Arg(args, "--season"), out var season) && season > 1)
            {
                State.SeasonsBefore = season - 1;
                State.LastEnd = Campaign.Swallowed;
                State.Remembers = true;
                State.Note("again");
            }
            if (Arg(args, "--knew") == "all")
                State.KnownBefore.UnionWith(Inquiry.Entries.Where(e => !e.Held && e.Way != EntryWay.Lead).Select(e => e.Key));
            if (int.TryParse(Arg(args, "--start-tier"), out var startTier))
                State.SkipTo(Math.Clamp(startTier, 1, ShopTiers.Max), int.TryParse(Arg(args, "--gold"), out var g) ? g : 500);
            // --tales=a,b: those tales are in the season, whatever it draws (to check one on demand).
            TaleBook.Forced.AddRange((Arg(args, "--tales") ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Where(TaleBook.Exists));
            // A shop as a careful player would have it by the start of that week (tips are taken as read).
            if (int.TryParse(Arg(args, "--start-week"), out var startWeek) && startWeek > 1)
            {
                State.Guided = false;
                if (!Simulation.FastForward(State, (startWeek - 1) * Guild.DaysPerWeek))
                    GD.Print($"--start-week: this seed's shop was foreclosed in week {State.Week}; starting from there anyway");
                State.Guided = !args.Contains("--no-tutorial");
                // What the skipped weeks noted is in the notebook: no notes over the top bar for it.
                State.NewPieces.Clear();
                State.NewEntries.Clear();
                if (int.TryParse(Arg(args, "--gold"), out var gold)) State.Gold = gold;
                foreach (var key in new[] { "intro", "dayend", "colours", "slip", "ledger", "upgrades", "display", "synergy", "tier2", "tier3", "tier4" })
                    State.HintsSeen.Add(key);
            }
            if (args.Contains("--engine")) State.Grant(Upgrades.Engine);
            foreach (var key in (Arg(args, "--curios") ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Where(Curios.Exists))
                State.GiveCurio(key);
            if (Arg(args, "--event") is string forced && Events.Find(forced) != null) State.ForcedEvent = forced;
            NoteStart();
        }
        GD.Print($"Keywright seed {_seed}");

        AddChild(new Painter(DrawBackground) { ZIndex = -200 });
        _camera = new Camera2D { Position = new Vector2(640, 360) };
        AddChild(_camera);

        Sfx = new Sfx();
        AddChild(Sfx);
        if (Arg(args, "--export-sounds") is string soundDir)
        {
            Sfx.Export(soundDir);
            GD.Print($"Exported synthesized sounds to {soundDir}");
            QuitGame();
            return;
        }
        Hud = new Hud();
        AddChild(Hud);

        var overlay = new CanvasLayer { Layer = 50 };
        AddChild(overlay);
        _flash = new ColorRect { Color = Colors.Transparent, MouseFilter = Control.MouseFilterEnum.Ignore };
        _flash.Size = new Vector2(1280, 720);
        _fade = new ColorRect { Color = new Color(Palette.Bg, 0f), MouseFilter = Control.MouseFilterEnum.Ignore };
        _fade.Size = new Vector2(1280, 720);
        overlay.AddChild(_flash);
        overlay.AddChild(_fade);

        if (args.Contains("--autoplay"))
        {
            MentorView.Trace = text => GD.Print($"[Bram d{State?.Day} {State?.Current?.Seller.Key ?? "-"}] {text}");
            ScenePhase.Trace = text => GD.Print($"[Scene d{State?.Day}] {text}");
            _bot = new Autoplay
            {
                QuitAtEnd = args.Contains("--quit-at-end"),
                MaxDays = int.TryParse(Arg(args, "--days"), out var days) ? days : 0,
                SightUnseen = args.Contains("--sight-unseen"),
            };
            AddChild(_bot);
        }
        _shotsDir = Arg(args, "--shots");
        if (_shotsDir != null) DirAccess.MakeDirRecursiveAbsolute(_shotsDir);
        UiKit.TrackPlacement = _shotsDir != null;

        if (args.Contains("--font-sheet")) FontSheet();
        else if (args.Contains("--density-review") || Arg(args, "--density-review") != null) DensityReview();
        else if (args.Contains("--art-review")) ArtReview();
        else if (args.Contains("--shell-demo")) ShellDemo();
        else if (args.Contains("--title-test")) TitleTest();
        else if (args.Contains("--fit-test")) FitTest();
        else if (!direct) ToTitle();
        else if (args.Contains("--opening-demo")) OpeningDemo(args.Contains("--quit-at-end"));
        else if (args.Contains("--treasury-demo") || Arg(args, "--treasury-demo") != null)
            TreasuryDemo(Arg(args, "--treasury-demo") == "final", args.Contains("--quit-at-end"));
        else BeginPlaying();
        if (args.Contains("--pad-test")) PadTest();
    }

    /// <summary>Walks through the title screen, a new game's first moments, the pause menu and the settings, for screenshots.</summary>
    async void ShellDemo()
    {
        SignalAwaiter Wait(double s) => ToSignal(GetTree().CreateTimer(s, true), SceneTreeTimer.SignalName.Timeout);
        _demo = true;
        // A profile with some runs behind it, so the character screen has something to show.
        Profile = new Profile { Runs = 9, Cleared = 2, BestWeek = 8, BestTier = 3, LastCharacter = Characters.Hesk };
        Profile.Challenge[Characters.Kit] = 2;
        // Hesk at the top level: the most boons and banes the setup screen and the pause menu ever list.
        Profile.Challenge[Characters.Hesk] = Challenges.Max;
        Profile.BestScore[Characters.Kit] = 452;
        // ...and a little gathered, for the collection's pages.
        Profile.Found.AddRange(Collection.Entries.Where((e, i) => i % 3 != 2 && e.Quality <= Rarity.Rare).Select(e => e.Entry));
        Profile.FoundEnchanted.AddRange(Collection.Entries.Where(e => !e.Fake && e.Quality == Rarity.Uncommon).Take(3).Select(e => e.Entry));
        Profile.CuriosSeen.AddRange(Curios.All.Where((c, i) => i % 2 == 0).Select(c => c.Key));
        Profile.Songs.AddRange(Heartsongs.All.Take(2).Select(h => h.Key));
        Profile.Achievements.AddRange(new[] { "FIRST_CHEST", "THORNWICK", "FIRST_TITHE", "PERFECT_READ", "ENCHANTED" }); // loc: keys
        // Keepsakes held in earlier runs: the longest descriptions, for the cards' room.
        Profile.KeepsakesSeen.AddRange(new[] { Keepsakes.Weights, Keepsakes.Lamp, Keepsakes.Oath, Keepsakes.StallKey, Keepsakes.Aldwin });
        ToTitle();
        // The chest bursts and the menu comes out of it (a little over two seconds), then a look round.
        await Wait(3.0);
        var title = (TitlePhase)_phase;
        // A page's first frame can take a while (every curio's picture): each picture waits for its own.
        var gathered = title.OpenCollection();
        await Wait(0.8);
        Moment("05_collection_finds");
        await Wait(1.5);
        gathered.ShowPage(CollectionView.CuriosPage);
        await Wait(0.5);
        Moment("06_collection_curios");
        await Wait(1.5);
        gathered.ShowPage(CollectionView.SongsPage);
        await Wait(0.5);
        Moment("07_collection_songs");
        await Wait(1.5);
        gathered.ShowPage(CollectionView.AchievementsPage);
        await Wait(0.5);
        Moment("07_collection_achievements");
        await Wait(1.0);
        gathered.ShowPage(CollectionView.KeepsakesPage);
        await Wait(0.5);
        Moment("07_collection_keepsakes");
        await Wait(1.0);
        gathered.Close();
        await Wait(0.5);
        // The credits, at the top and further down (the engine's notices).
        var credits = title.OpenCredits();
        await Wait(0.8);
        Moment("08_credits");
        await Wait(0.5);
        if (credits.GetChildren().OfType<ScrollContainer>().FirstOrDefault() is { } scroll)
        {
            scroll.ScrollVertical = 1400;
            await Wait(0.4);
            Moment("08_credits_licences");
            await Wait(0.5);
        }
        credits.Close();
        await Wait(0.5);
        OpenSetup();
        await Wait(1.2);
        var setup = (SetupPhase)_phase;
        setup.SetLevel(Challenges.Max);
        await Wait(0.6);
        Moment("09_setup");
        await Wait(0.5);
        StartRun(Characters.Hesk, Challenges.Max);
        await Wait(3.5);
        Moment("10_newgame");
        await Wait(0.5);
        OpenPause();
        await Wait(0.6);
        Moment("11_pause");
        await Wait(0.5);
        var pause = GetChildren().OfType<PauseMenu>().First();
        var during = pause.OpenCollection();
        await Wait(0.8);
        Moment("11_pause_collection");
        await Wait(1.5);
        during.Close();
        await Wait(0.4);
        var settings = new SettingsView();
        pause.AddChild(settings);
        await Wait(0.6);
        Moment("12_settings");
        await Wait(0.5);
        foreach (var (page, name) in new[] { (SettingsView.SoundPage, "sound"), (SettingsView.ControlsPage, "controls"), (SettingsView.GamePage, "game") })
        {
            settings.ShowPage(page);
            await Wait(0.4);
            Moment($"12_settings_{name}");
            await Wait(0.5);
        }
        settings.ShowPage(SettingsView.SoundPage);
        settings.OpenSounds();
        await Wait(0.6);
        Moment("13_item_sounds");
        await Wait(0.8);
        // The Outlines setting at work, when the art is drawn to be outlined: off, a look at the scene with the pause
        // menu out of the way, and back on.
        if (ArtTexture.OutlinesAllowed)
        {
            settings.ToggleOutlines();
            pause.Visible = false;
            await Wait(0.4);
            Moment("13_outlines_off");
            await Wait(0.5);
            pause.Visible = true;
            settings.ToggleOutlines();
        }
        // With GM mode on: its buttons at work (the day can't be skipped during a scene), and the journal it filled.
        if (Settings.Gm)
        {
            settings.QueueFree();
            await Wait(0.3);
            GD.Print($"GM: {GmGold()} {GmUnlockJournal()} Skip: {CanSkipDay ?? "allowed"}");
            await Wait(0.5);
            Moment("14_gm");
            var collection = pause.OpenCollection(CollectionView.CuriosPage);
            await Wait(0.8);
            Moment("15_gm_curios");
            collection.ShowPage(CollectionView.FindsPage);
            await Wait(0.5);
            Moment("16_gm_collection");
            await Wait(0.3);
        }
        // Back to the title from the pause menu: the quicker burst.
        if (IsInstanceValid(pause)) pause.Resume();
        ToTitle();
        await Wait(0.75);
        Moment("17_title_return_flight");
        await Wait(1.2);
        Moment("17_title_return");
        await Wait(0.3);
        QuitGame();
    }

    bool _quitting;

    /// <summary>
    /// Quits cleanly: lets go of the screen and the bot first, gives anything still waiting on them a frame or two to
    /// notice, and collects what the managed side still holds, so the engine doesn't shut down around live objects (after
    /// a long session that could crash on the way out).
    /// </summary>
    public async void QuitGame(int exitCode = 0)
    {
        if (_quitting) return;
        _quitting = true;
        GetTree().Paused = false;
        _phase?.QueueFree();
        _phase = null;
        _bot?.QueueFree();
        _bot = null;
        Sfx?.Release();
        GetTree().ProcessFrame -= Platform.Tick;
        Platform.Stop();
        for (int i = 0; i < 3; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GetTree().Quit(exitCode);
    }

    /// <summary>The saved shop (or its backup), or null if there isn't one this version can read (one it can't is set aside).</summary>
    public static SaveData ReadSave() => Storage.Read(SavePath, SaveData.FromJson);

    void WriteSave() => Storage.Write(SavePath, State.Save().ToJson());

    /// <summary>
    /// Brings the profile and the run in progress over from where earlier versions kept them: Godot's own app_userdata
    /// folder until 0.27, and LootFall's until 0.18 (see <see cref="Storage.CarryOver"/>).
    /// </summary>
    static void CarryOverOldFiles() =>
        Storage.CarryOver(json => SaveData.FromJson(json) != null, json => Profile.FromJson(json) != null);

    /// <summary>The profile (or its backup); a new one if there's none this version can read (one it can't is set aside).</summary>
    static Profile ReadProfile() => Storage.Read(ProfilePath, Profile.FromJson) ?? new Profile();

    /// <summary>Notes the run's progress in the profile (a real game only) and writes it.</summary>
    void WriteProfile(System.Action<Profile> change = null)
    {
        if (!_saving || _demo)
        {
            NoteAchievements();
            return;
        }
        change?.Invoke(Profile);
        Profile.Note(State);
        NoteAchievements();
        Storage.Write(ProfilePath, Profile.ToJson());
    }

    /// <summary>
    /// Achievements done since the last look: marked in the profile (written with it), sent to Steam, and announced
    /// here when the game isn't on Steam, which announces its own.
    /// </summary>
    void NoteAchievements()
    {
        if (Profile == null) return;
        var earned = Achievements.Earned(State, Profile);
        if (earned.Count == 0) return;
        foreach (var achievement in earned)
        {
            Profile.Achievements.Add(achievement.Key);
            GD.Print($"[Achievement] {achievement.Key}: {achievement.Name}");
            if (!Platform.OnSteam) Announce(achievement);
        }
        Platform.Unlock(earned.Select(a => a.Key));
    }

    /// <summary>
    /// An Inquiry entry just noted (0.34): a note over the top bar (how you came by it, and the question it's for), which
    /// opens the notebook's Inquiry page where it can.
    /// </summary>
    void NoteEntry(Entry entry, List<EntryWay> ways)
    {
        var thread = Inquiry.Threads.FirstOrDefault(t => t.Key == entry.Thread);
        if (thread == null) return;
        string how = Words.List(ways.Select(Inquiry.WayName));
        string head = Loc.T($"In the notebook ({how})"), ask = thread.Ask;
        Toast(head, ask, Palette.Magic, Loc.T("Open the notebook's Inquiry page"), () => _phase?.ShowInquiry());
    }

    /// <summary>A piece of the treasury key just earned: the Guild's courier, over the top bar.</summary>
    void NotePiece(string piece, int fee, int count)
    {
        Hud.SetGold(State.Gold);
        Toast(Loc.T($"{Campaign.PieceName(piece)} of the treasury key"), Loc.T($"{count} of {Campaign.Pieces.Length}, and a master's fee: +{fee}g"),
            Palette.Gold, Loc.T("Open the notebook's Inquiry page"), () => _phase?.ShowInquiry());
    }

    /// <summary>The note over the top bar now showing, if any (the next waits for it to go).</summary>
    CanvasLayer _toast;

    /// <summary>Takes the note over the top bar away at once (the notebook it points to has just been opened).</summary>
    public void HideNote()
    {
        if (_toast != null && IsInstanceValid(_toast)) _toast.QueueFree();
        _toast = null;
    }

    void Toast(string head, string text, Color color, string tooltip, System.Action click)
    {
        var layer = new CanvasLayer { Layer = 85, ProcessMode = ProcessModeEnum.Always };
        AddChild(layer);
        _toast = layer;
        float w = Mathf.Clamp(Mathf.Max(UiKit.TextWidth(head), UiKit.TextWidth(text)) + 72, 360, 760);
        var panel = UiKit.MakePanel(color.Darkened(0.35f)).Place(640 - w / 2, 58, w, 62);
        panel.MouseFilter = Control.MouseFilterEnum.Stop;
        panel.MouseDefaultCursorShape = Control.CursorShape.PointingHand;
        panel.TooltipText = tooltip;
        panel.GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } && _phase != null && !GetTree().Paused) click();
        };
        layer.AddChild(panel);
        panel.AddChild(UiKit.MakeLabel(head, 16, color, HorizontalAlignment.Center).Place(0, 8, w, 20));
        panel.AddChild(UiKit.MakeLabel(text, 16, Palette.Text, HorizontalAlignment.Center).Place(0, 32, w, 20));
        panel.Modulate = new Color(1, 1, 1, 0);
        Sfx.Play("sting", 1.15f, -12);
        var tween = layer.CreateTween();
        tween.TweenProperty(panel, "modulate:a", 1f, 0.25f);
        tween.TweenCallback(Callable.From(() => Moment("clue")));
        tween.TweenInterval(4.0);
        tween.TweenProperty(panel, "modulate:a", 0f, 0.6f);
        tween.TweenCallback(Callable.From(layer.QueueFree));
    }

    /// <summary>An achievement's name over the top bar's title for a few seconds (the one place no screen uses).</summary>
    void Announce(AchievementDef achievement)
    {
        var layer = new CanvasLayer { Layer = 90, ProcessMode = ProcessModeEnum.Always };
        AddChild(layer);
        var panel = UiKit.MakePanel(Palette.Gold.Darkened(0.55f)).Place(400, 6, 480, 44);
        layer.AddChild(panel);
        panel.AddChild(UiKit.MakeLabel(Loc.T($"Achievement: {achievement.Name}"), 16, Palette.Gold, HorizontalAlignment.Center)
            .Place(0, 12, 480, 20));
        panel.TooltipText = achievement.Text;
        panel.Modulate = new Color(1, 1, 1, 0);
        var tween = layer.CreateTween();
        tween.TweenProperty(panel, "modulate:a", 1f, 0.3f);
        tween.TweenCallback(Callable.From(() => Moment("achievement")));
        tween.TweenInterval(3.0);
        tween.TweenProperty(panel, "modulate:a", 0f, 0.6f);
        tween.TweenCallback(Callable.From(layer.QueueFree));
    }

    /// <summary>The run is over (foreclosed, or retired): its feats and score are banked, the saved shop goes, and it's back to the title.</summary>
    public void EndRun()
    {
        WriteProfile(p => p.Bank(State));
        // A season that ended without a Heartsong ever heard: Bram's word on them, once, before the title.
        bool unheard = Profile.Songs.Count == 0 && State.SongsFound.Count == 0 && !Profile.Taught.Contains(SongsTold);
        if (unheard)
        {
            Profile.Taught.Add(SongsTold);
            WriteProfile();
        }
        if (_saving) Storage.Delete(SavePath);
        _saving = false;
        if (unheard) SwitchTo(new ScenePhase(Story.SongsUnheard(State), AfterRun, Loc.T("THE NINE HEARTSONGS"), "bram"));
        else AfterRun();
    }

    /// <summary>Bram's word on Heartsongs, said once a profile (between runs).</summary>
    const string SongsTold = "between.songs"; // loc: keys

    void AfterRun()
    {
        if (_bot != null)
        {
            QuitGame();
            return;
        }
        ToTitle();
    }

    /// <summary>Back to the title screen (from the pause menu).</summary>
    public void ToTitle()
    {
        Platform.Presence(null);
        Hud.Visible = false;
        SwitchTo(new TitlePhase(ReadSave()));
    }

    /// <summary>New game: who minds the stall, and on what terms.</summary>
    public void OpenSetup()
    {
        Hud.Visible = false;
        SwitchTo(new SetupPhase());
    }

    /// <summary>
    /// A new shop, minded by one of Bram's kin at a challenge level; saved over any old one when its first day starts (an
    /// old run still banks its feats and score).
    /// </summary>
    public void StartRun(string character, int challenge)
    {
        if (!_demo && ReadSave() is SaveData old)
        {
            Profile.Bank(GameState.Load(old));
            Storage.Delete(SavePath);
        }
        State = GameState.NewRun(_seed, character, challenge, Settings.Tips, Profile.Taught, Profile.OpenCurios,
            Challenges.Draw(Profile.TermsSeed, challenge));
        State.Songbook.UnionWith(Profile.Songs);
        State.Remember(Profile);
        _seed++;
        _saving = !_demo;
        Profile.LastCharacter = character;
        Profile.LastChallenge = challenge;
        NoteStart();
        WriteProfile(p => p.Runs++);
        BeginPlaying();
    }

    /// <summary>Who could mind the stall, and what's open to this run's character, as the run begins.</summary>
    void NoteStart()
    {
        KinAtStart = Characters.All.Where(Profile.Unlocked).Select(c => c.Key).ToArray();
        LevelsAtStart = Profile.MaxChallenge(State.Character);
        CuriosAtStart = Profile.OpenCurios.ToArray();
    }

    /// <summary>A Heartsong's just been opened: into the profile's songbook straight away.</summary>
    public void NoteSongs() => WriteProfile();

    /// <summary>Bram's just taught something (a tip, a lesson): into the profile straight away, so it's never taught again.</summary>
    public void NoteTaught() => WriteProfile();

    public void ContinueGame(SaveData save)
    {
        State = GameState.Load(save);
        State.Songbook.UnionWith(Profile.Songs);
        State.Remember(Profile);
        _saving = true;
        NoteStart();
        BeginPlaying();
    }

    void BeginPlaying()
    {
        Hud.Visible = true;
        Hud.SetGold(State.Gold, animate: false);
        // A jump (--start-week) stopped on the night the treasury's due: that night, not another day first.
        if (State.TreasuryDue)
        {
            Hud.SetDay(State.Day, State.Tier);
            Hud.SetDue(State);
            Treasury();
            return;
        }
        StartDay();
    }

    // ---- GM mode (Settings.Gm, or --gm): the pause menu's testing buttons ----

    /// <summary>GM: 99,999 gold in the purse. Returns what the pause menu says.</summary>
    public string GmGold()
    {
        State.Gold += 99999;
        // The game's paused under the GM panel: no counting up, the purse shows it at once.
        Hud.SetGold(State.Gold, animate: false);
        Hud.SetDue(State);
        return Loc.T($"In the purse: {State.Gold}g.");
    }

    /// <summary>GM: every page of the notebook and the collection filled, this run's and the profile's.</summary>
    public string GmUnlockJournal()
    {
        State.KnowAll();
        WriteProfile(p => p.KnowAll());
        return Loc.T("The notebook and the collection are full: every regular, every thing, every curio, and all nine Heartsongs.");
    }

    /// <summary>Why the day can't be skipped now, or null: only from Nim's counter, the shop's counter or the day's end.</summary>
    public string CanSkipDay => _phase is CounterPhase or PeddlerPhase or DayEndPhase or TalkPhase { Answered: false, Talk.Night: false } ? null
        : Loc.T("Not now: finish what's in hand first.");

    /// <summary>
    /// GM: the rest of today skipped. Everyone still waiting goes home and it's the evening (from the day's end: the next
    /// morning, the Guild coming first if it's due).
    /// </summary>
    public void GmSkipDay()
    {
        if (CanSkipDay != null) return;
        if (_phase is DayEndPhase)
        {
            CloseDay();
            return;
        }
        // Nim's table is the night's: skipping it is the next morning.
        if (_phase is PeddlerPhase)
        {
            StartDay();
            return;
        }
        State.SkipRestOfDay();
        SwitchTo(new DayEndPhase());
    }

    /// <summary>
    /// The top bar's due was clicked: the screen open now shows the Guild's debt in the notebook, if it can (not in the
    /// middle of picking a lock or an opening). Returns whether it did.
    /// </summary>
    public bool ShowDebt() => State != null && !GetTree().Paused && _phase != null && _phase.ShowDebt();

    /// <summary>Esc during a day, or the HUD's Menu button: pause.</summary>
    public void OpenPause()
    {
        if (State == null || _phase is TitlePhase || GetTree().Paused) return;
        AddChild(new PauseMenu());
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed(Controls.Pause))
        {
            GetViewport().SetInputAsHandled();
            OpenPause();
        }
    }

    /// <summary>Another window has the focus: the game pauses (a waiting seller's patience with it) and goes quiet, as the
    /// player has chosen (Settings); it comes back when they return.</summary>
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut)
        {
            if (Settings.MuteInBackground) Settings.ApplyAudio(muted: true);
            if (Settings.PauseInBackground && _bot == null && !_demo) OpenPause();
        }
        else if (what == NotificationApplicationFocusIn && Settings.MuteInBackground)
        {
            Settings.ApplyAudio();
        }
    }

    /// <summary>The look of things Godot draws itself, like tooltips: a framed panel and the pixel font.</summary>
    static Theme MakeTheme()
    {
        var theme = new Theme { DefaultFont = UiKit.Font, DefaultFontSize = UiKit.SnapFont(16) };
        theme.SetStylebox("panel", "TooltipPanel", UiKit.PanelBox(10));
        theme.SetColor("font_color", "TooltipLabel", Palette.Text);
        return theme;
    }

    /// <summary>
    /// Opens a blue chest with one thing of each quality (the epic one a step better than the chest, the legendary
    /// one two steps) and a false bottom, so the opening's effects can be seen and recorded together.
    /// </summary>
    async void OpeningDemo(bool quit)
    {
        State.SkipTo(ShopTiers.Max, 1000);
        Hud.SetGold(State.Gold, animate: false);
        var chest = new Chest(new[]
        {
            new Item(Category.Arms, Rarity.Common), new Item(Category.Garb, Rarity.Uncommon), new Item(Category.Potions, Rarity.Rare),
            new Item(Category.Coins, Rarity.Epic), new Item(Category.Gems, Rarity.Legendary),
            Heartsongs.Make(Heartsongs.Get(Heartsongs.Egg), Rarity.Rare),
        }, Rarity.Rare, new Item(Category.Scrolls, Rarity.Rare, Magic: true));
        var visit = Market.Price(State.Rng, Sellers.Get("tick"), chest, ShopTiers.Max, ask: 300);
        visit.Buy(State.Rng);
        State.Opening = State.Buy(visit, new Appraisal(chest.Quality, ShopTiers.Max, chest.Items.Select(i => i.Category)));
        var opening = new Phases.OpeningPhase();
        SwitchTo(opening);
        // The Heartsong waits for a choice: keep it, once it's had its moment.
        while (IsInstanceValid(opening) && !opening.Finished)
        {
            if (opening.SongChoice is SongReveal song)
            {
                await ToSignal(GetTree().CreateTimer(3.0), SceneTreeTimer.SignalName.Timeout);
                if (IsInstanceValid(song)) song.Keep();
            }
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        await ToSignal(GetTree().CreateTimer(3.0), SceneTreeTimer.SignalName.Timeout);
        if (quit) QuitGame();
    }

    /// <summary>
    /// The treasury straight away, on the season's last night with the account paid and the key whole: the first
    /// swallowing, or (final) the final opening with the plan whole.
    /// </summary>
    void TreasuryDemo(bool final, bool quit)
    {
        _demo = true;
        _quitAfterTreasury = quit;
        State.SkipTo(ShopTiers.Max, 1000);
        Hud.SetGold(State.Gold, animate: false);
        State.DemoTreasury(final);
        Treasury();
    }

    bool _quitAfterTreasury;

    /// <summary>Saves the shop (a real game only), starts a new day, and plays its morning.</summary>
    public void StartDay()
    {
        if (_saving)
        {
            WriteSave();
            WriteProfile();
        }
        else NoteAchievements();
        Platform.Presence(State);
        State.StartDay();
        Hud.SetDay(State.Day, State.Tier);
        Hud.SetDue(State);
        Hud.SetEvent(State);
        if (_bot != null && !_bot.OnDayStarted(State.Day)) return;
        // A story morning: who comes by, and what they want.
        if (State.StoryToday)
        {
            var lines = Story.WeekStart(State);
            if (lines.Count > 0)
            {
                SwitchTo(new ScenePhase(lines, OpenShop, State.Week == 1 ? Loc.T("BRAM'S STALL") : Loc.T($"WEEK {State.Week}"), Story.Setting(State.Week)));
                return;
            }
        }
        OpenShop();
    }

    /// <summary>Opens the shop: the day's card, and the first customer.</summary>
    public void OpenShop()
    {
        DayCard();
        NextCustomer();
    }

    /// <summary>
    /// The run's over, won or lost: the season's end first (what became of everyone whose tale began, once a run: not
    /// again if you keep trading and the Guild's interest catches up), then the score.
    /// </summary>
    void EndRun(bool cleared)
    {
        var lines = State.Flagged(SeasonEnded) ? new List<Line>() : Story.SeasonEnd(State);
        State.Flags.Add(SeasonEnded);
        if (lines.Count == 0) SwitchTo(new RunEndPhase(cleared));
        else SwitchTo(new ScenePhase(lines, () => SwitchTo(new RunEndPhase(cleared)), Loc.T("THE SEASON'S END"), "bram"));
    }

    const string SeasonEnded = "season.ended";

    /// <summary>After the day's end: the Guild's introduction (the second evening), its due (the fifth), or the night.</summary>
    public void CloseDay()
    {
        if (!State.GuildIntroduced && State.Day >= GameState.IntroDay) SwitchTo(new ScenePhase(Story.Intro(State), Night, Loc.T("THE GILDED GUILD"), "guild"));
        else if (State.DuesTonight && !State.PaidThisWeek) SwitchTo(new DuesPhase());
        // The debt was paid off early and the key's last piece has just come: the treasury, tonight.
        else if (State.TreasuryDue) Treasury();
        else Night();
    }

    /// <summary>
    /// The night (docs/stories.md): a tale's knock at the back door or a letter, the Wardens or the Lockless if they've
    /// a quarrel with you, then Nim's table, and the next morning.
    /// </summary>
    void Night()
    {
        State.StartNight();
        NextNight();
    }

    /// <summary>The night's next talk, or Nim's table once they're done.</summary>
    public void NextNight()
    {
        if (State.Tonight.FirstOrDefault(t => !t.Done) is Talk talk)
        {
            SwitchTo(new TalkPhase(talk));
            return;
        }
        // Nim's table, from his first night on (the third); before that, straight to the morning.
        if (State.PeddlerToday) SwitchTo(new PeddlerPhase());
        else StartDay();
    }

    /// <summary>After the Guild's visit: the shop lost, Bram's debt cleared (and maybe the vault), or the next morning.</summary>
    public void AfterDues()
    {
        if (State.Foreclosed)
        {
            SwitchTo(new ScenePhase(Story.Foreclosed(State), () => EndRun(cleared: false), backdrop: "guild"));
            return;
        }
        if (State.DebtCleared && State.ClearedDay == State.Day)
        {
            Cleared();
            return;
        }
        // The first Guild night: the treasury's summons, and Bram goes (0.34), with whoever's note the Guild collects tonight.
        State.GuildCollects();
        if (State.SummonsTonight)
        {
            State.Summon();
            SwitchTo(new ScenePhase(Story.Summons(State), Night, Loc.T("THE TREASURY'S SUMMONS"), "guild"));
            return;
        }
        Night();
    }

    /// <summary>
    /// Pays off the rest of Bram's debt at the end of a day, however early: the scene, then the vault (with the whole
    /// map) or the end of the run.
    /// </summary>
    public void PayOffDebt()
    {
        if (State.PayOff() <= 0) return;
        Sfx.PlayGroup("pay");
        Hud.SetGold(State.Gold);
        Hud.SetDue(State);
        Cleared();
    }

    /// <summary>The debt's just been cleared: Vane's last visit and Bram's joy, then the vault or the end of the run.</summary>
    void Cleared()
    {
        WriteProfile(p =>
        {
            p.Cleared++;
            p.ClearedAt(State);
        });
        SwitchTo(new ScenePhase(Story.Cleared(State), AfterCleared, Loc.T("FREE AND CLEAR"), State.BramHere ? "bram" : "guild"));
    }

    /// <summary>The debt's paid: Bram's homecoming if he's freed, then the treasury (with the key whole) or the season's end.</summary>
    void AfterCleared()
    {
        if (State.BramComesHome)
        {
            SwitchTo(new ScenePhase(Story.Homecoming(State), () =>
            {
                State.WelcomeBram();
                AfterCleared();
            }, Loc.T("TWO TAPS AND ONE"), "bram"));
            return;
        }
        if (State.TreasuryDue) Treasury();
        else EndRun(cleared: true);
    }

    /// <summary>
    /// The treasury (0.34, docs/narrative.md): the Guild's carriage, then the great chest at the counter. The first time
    /// it swallows you; once you remember you can stay away; with the plan whole, it's the final opening.
    /// </summary>
    void Treasury()
    {
        var mode = State.PlanWhole ? TreasuryMode.Final : State.Remembers ? TreasuryMode.Again : TreasuryMode.First;
        WriteProfile();
        SwitchTo(new ScenePhase(Story.TreasuryInvite(State), () => SwitchTo(new TreasuryPhase(mode)), Loc.T("THE TREASURY"), "vault"));
    }

    /// <summary>What came of the treasury: swallowed (the year turns, and you remember), stayed away (the season ends), the door left to the Maw (it turns anyway), or the door closed (the next day).</summary>
    public void AfterTreasury(TreasuryResult result)
    {
        switch (result)
        {
            case TreasuryResult.Swallowed:
                State.Swallow();
                Rewind();
                break;
            case TreasuryResult.Stayed:
                State.StayAway();
                EndRun(cleared: true);
                break;
            case TreasuryResult.DoorLeft:
                State.LeaveTheDoor();
                SwitchTo(new ScenePhase(Story.DoorLeft(State), Rewind, Loc.T("THE LAST CHEST"), "vault"));
                break;
            default:
                State.CloseTheDoor();
                WriteProfile();
                SwitchTo(new ScenePhase(Story.NextDay(State), () =>
                {
                    State.BeginNextDay();
                    Hud.SetDay(State.Day, State.Tier);
                    Hud.SetDue(State);
                    NextCustomer();
                }, Loc.T("THE NEXT DAY"), "bram"));
                break;
        }
    }

    /// <summary>
    /// The year turns: the season is banked, and (a real game) the next one starts at once with the same keywright at the
    /// same level, on the first morning. The bot and the demos stop here.
    /// </summary>
    void Rewind()
    {
        // A jump's season (--start-week doesn't save) is banked in memory all the same, so the next season remembers it.
        if (!_saving && !_demo && _bot == null)
        {
            Profile.Bank(State);
            Profile.Note(State);
        }
        WriteProfile(p => p.Bank(State));
        if (_saving) Storage.Delete(SavePath);
        if (_bot != null || _demo)
        {
            if (_bot != null || _quitAfterTreasury) QuitGame();
            else ToTitle();
            return;
        }
        _saving = false;
        StartRun(State.Character, State.Challenge);
    }

    /// <summary>After a chest's been opened: the next customer, or (after the vault) the epilogue and the end of the debt.</summary>
    public void AfterOpening()
    {
        // The next day's one chest, the waterwheel's tools: the shop goes on (0.34).
        if (State.Opening?.Visit.Beat == GameState.WaterwheelBeat)
        {
            SwitchTo(new ScenePhase(Story.ShopGoesOn(State), () => EndRun(cleared: true), Loc.T("THE NEXT DAY"), "bram"));
            return;
        }
        NextCustomer();
    }

    /// <summary>
    /// The day's title card, over the first customer: "Day 5", the week and the shop's name, the Guild's due, and today's
    /// event if there is one.
    /// </summary>
    /// <summary>
    /// How tall a day's event's description is on the morning card: as tall as its words at the card's width (three or
    /// four lines in French), never under two and a half; the fit test keeps every one to four lines.
    /// </summary>
    public static float MorningEventHeight(string text) => Mathf.Max(66, UiKit.TextHeight(text, 460));

    void DayCard()
    {
        var layer = new CanvasLayer { Layer = 40 };
        AddChild(layer);
        var e = State.TodayEvent;
        int stash = State.Today.Stash;
        // The week's note, on its first day (a week without a story morning): the Hooded Woman's word, the debt's last week.
        string note = State.DayOfWeek == 1 ? Story.WeekNote(State) : null;
        // The Guild's due shows once Vane has introduced it (the first evening).
        bool guild = State.GuildIntroduced;
        // The note as tall as its words (the Hooded Woman's ask runs to five lines, and longer in French).
        float noteH = note == null ? 0 : Mathf.Max(70, UiKit.TextHeight(note, 460));
        string eventText = e == null ? null : Events.Describe(e, State.BuyerKind);
        float eventH = e == null ? 0 : MorningEventHeight(eventText);
        float h = (e == null ? 136 : 172 + eventH) + (stash > 0 ? 28 : 0) - (guild ? 0 : 28) + (note == null ? 0 : noteH + 6);
        var card = UiKit.MakePanel(Palette.Gold.Darkened(0.2f)).Place(306, 130, 500, h);
        card.MouseFilter = Control.MouseFilterEnum.Ignore;
        card.AddChild(UiKit.MakeLabel(Loc.T($"DAY {State.Day}"), 48, Palette.Gold, HorizontalAlignment.Center, outline: 6).Place(0, 12, 500, 56));
        // Who's minding the stall, and at what level, unless it's Kit on the usual terms.
        string who = State.Is(Characters.Kit) && State.Challenge == 0 ? ""
            : $"{State.Kin.Name}{(State.Challenge > 0 ? Loc.T($", level {State.Challenge}") : "")}  -  ";
        card.AddChild(UiKit.MakeLabel(Loc.T($"{who}Week {State.Week}, day {State.DayOfWeek}{(who == "" ? Loc.T($" of {Guild.DaysPerWeek}") : "")}  -  {ShopTiers.Name(State.Tier)}"), 16, Palette.Text, HorizontalAlignment.Center).Place(0, 72, 500, 20));
        string due = State.DuesTonight ? Loc.T($"The Guild comes tonight for {State.DueNow}g.")
            : Loc.T($"The Guild comes for {State.DueNow}g at the end of day {State.Day + State.DaysToDue}.");
        if (guild)
            card.AddChild(UiKit.MakeLabel(due, 16, State.Gold >= State.DueNow ? Palette.Muted : Palette.Warn, HorizontalAlignment.Center)
                .Place(0, 100, 500, 20));
        float y = guild ? 132 : 104;
        if (stash > 0)
        {
            card.AddChild(UiKit.MakeLabel(Loc.T($"{Hints.Mentor} slipped you {stash}g from his secret stash ({State.StashLeft} left)."), 16, Palette.Good,
                HorizontalAlignment.Center).Place(0, y - 4, 500, 20));
            Hud.SetGold(State.Gold);
            y += 28;
        }
        if (note != null)
        {
            card.AddChild(UiKit.MakeLabel(note, 16, Palette.Gold, HorizontalAlignment.Center).Wrapped().Place(20, y - 4, 460, noteH));
            y += noteH + 6;
        }
        if (e != null)
        {
            card.AddChild(UiKit.MakeLabel(e.Name.ToUpperInvariant(), 24, Palette.Magic, HorizontalAlignment.Center, outline: 4).Place(0, y, 500, 30));
            card.AddChild(UiKit.MakeLabel(eventText, 16, Palette.Text, HorizontalAlignment.Center).Wrapped()
                .Place(20, y + 32, 460, eventH));
            Sfx.Play("chime", 1f, -6);
        }
        layer.AddChild(card);
        card.PivotOffset = new Vector2(250, h / 2);
        card.Scale = new Vector2(0.9f, 0.9f);
        card.Modulate = Colors.Transparent;
        var tween = card.CreateTween();
        tween.TweenProperty(card, "modulate", Colors.White, 0.25f).SetDelay(0.2f);
        tween.Parallel().TweenProperty(card, "scale", Vector2.One, 0.25f).SetDelay(0.2f).SetTrans(Tween.TransitionType.Back);
        tween.TweenInterval(e == null && stash == 0 && note == null ? 1.0 : note == null ? 2.6 : 4.0);
        tween.TweenProperty(card, "modulate", Colors.Transparent, 0.35f);
        tween.TweenCallback(Callable.From(layer.QueueFree));
        Moment(e != null ? $"00_event_{e.Key}" : stash > 0 ? "00_stash" : "00_daycard", 0.6);
    }

    /// <summary>Calls the next customer to the counter, or closes up for the day.</summary>
    public void NextCustomer()
    {
        Hud.SetDay(State.Day, State.Tier);
        Hud.SetDue(State);
        Hud.SetEvent(State);
        if (State.NextCustomer()) SwitchTo(State.Current.Talk is Talk talk ? new TalkPhase(talk) : new CounterPhase());
        else SwitchTo(new DayEndPhase());
    }

    public async void SwitchTo(Phase next)
    {
        if (_phase != null)
        {
            var fadeOut = CreateTween();
            fadeOut.TweenProperty(_fade, "color:a", 1f, 0.18f);
            await ToSignal(fadeOut, Tween.SignalName.Finished);
            // The game quit while the screen faded: there's nothing to switch to.
            if (_quitting)
            {
                next.Free();
                return;
            }
            _phase.QueueFree();
            Zoom(1f, Center);
        }
        _phase = next;
        AddChild(next);
        CreateTween().TweenProperty(_fade, "color:a", 0f, 0.22f);
        _bot?.OnPhase(next);
    }

    static readonly Vector2 Center = new(640, 360);

    /// <summary>Zooms the camera in (above 1) or out, keeping <paramref name="focus"/> where it is on screen.</summary>
    public void Zoom(float zoom, Vector2 focus)
    {
        _camera.Zoom = Vector2.One * zoom;
        _camera.Position = focus - (focus - Center) / zoom;
    }

    /// <summary>Adds screen-shake "trauma" (0..1). Shake strength is trauma squared, so small hits stay subtle.</summary>
    public void Shake(float amount) => _trauma = Mathf.Min(1f, _trauma + amount * Settings.Shake);

    public void Flash(Color color, float alpha, float duration = 0.35f)
    {
        _flash.Color = color with { A = alpha * Settings.Shake };
        CreateTween().TweenProperty(_flash, "color:a", 0f, duration).SetEase(Tween.EaseType.Out);
    }

    /// <summary>Freezes time for a moment on impact, which sells the weight of a hit.</summary>
    public async void HitStop(double seconds)
    {
        Engine.TimeScale = 0.05;
        await ToSignal(GetTree().CreateTimer(seconds, true, false, true), SceneTreeTimer.SignalName.Timeout);
        Engine.TimeScale = 1.0;
    }

    /// <summary>Saves a screenshot the first time a moment happens, when running with --shots. No-op otherwise.</summary>
    public async void Moment(string name, double delay = 0)
    {
        if (_shotsDir == null) return;
        if (delay > 0) await ToSignal(GetTree().CreateTimer(delay, true, false, true), SceneTreeTimer.SignalName.Timeout);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Debug.Overflow.Scan(GetTree().Root, name);
        string path = $"{_shotsDir}/{name}.png";
        if (FileAccess.FileExists(path)) return; // keep the first occurrence of each moment
        GetViewport().GetTexture().GetImage().SavePng(path);
    }

    public override void _Process(double delta)
    {
        // An Inquiry entry just noted, or a piece of the treasury key earned (0.34): a note over the top bar, one at a time.
        if (State != null && (_toast == null || !IsInstanceValid(_toast)) && _phase is not { HoldsNotes: true } && NotebookView.Open == 0 && HistoryView.Open == 0)
        {
            if (State.NewPieces.TryDequeue(out var piece)) NotePiece(piece.Piece, piece.Fee, piece.Count);
            else if (State.NewEntries.TryDequeue(out var entry))
            {
                // Several for one question at once (a scene's lines): one note, with each way they came.
                var ways = new List<EntryWay> { entry.Way };
                while (State.NewEntries.TryPeek(out var next) && next.Thread == entry.Thread)
                {
                    State.NewEntries.Dequeue();
                    if (!ways.Contains(next.Way)) ways.Add(next.Way);
                }
                NoteEntry(entry, ways);
            }
        }
        if (_camera == null) return;
        _trauma = Mathf.Max(0f, _trauma - (float)delta * 1.6f);
        float strength = _trauma * _trauma;
        _camera.Offset = new Vector2(Rand(), Rand()) * 24f * strength;
    }

    float Rand() => (float)(_rng.NextDouble() * 2 - 1);

    static void DrawBackground(Painter p)
    {
        p.DrawRect(new Rect2(-100, -100, 1480, 920), Palette.Bg);
        for (int i = 0; i < 6; i++)
            p.DrawRect(new Rect2(-100, 560 + i * 30, 1480, 30), new Color(0, 0, 0, 0.06f * i));
    }

    static string Arg(string[] args, string name) =>
        args.FirstOrDefault(a => a.StartsWith(name + "="))?[(name.Length + 1)..];
}
