using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Keywright.Core;

namespace Keywright.Debug;

/// <summary>
/// The experience playtest (`--sim=experience [--n=30]`): not what a run earns but what it's like to play, for a player
/// who starts knowing nothing, learns, and comes back for a second and a third run. Each player is a learner (the sim's
/// careful player with mistakes that shrink run by run, <see cref="Careers"/>), with Bram's tips on as a first-timer
/// would have them. Every moment of play is a <see cref="Beat"/> with a cost in time (a human's pace, not the sim's) and
/// what it brought: things met for the first time and what learning them asks (load), novelty (new things, and old
/// ones wearing thin), peaks (a surprise climbing, a jackpot, a Heartsong, a close night with the Guild, a new shop),
/// and choices that were really choices (a chest that could go either way, a curio, an upgrade). From those: where the
/// run repeats itself with nothing new (stale stretches), where too much arrives at once (overload), the high points,
/// the dull ones, and what would make the curve better.
/// <para>
/// The weights are judgement calls, written down here so they can be argued with: the report is for comparing one
/// version of the game with the next, and for finding where in a run something happens, not for its absolute numbers.
/// </para>
/// </summary>
public static partial class Simulation
{
    /// <summary>What a player has learned and seen, carried from run to run.</summary>
    sealed class Knowledge
    {
        readonly Dictionary<string, int> _seen = new();

        public int Seen(string key) => _seen.GetValueOrDefault(key);

        /// <summary>Notes one more time this was met; returns how many times it had been before.</summary>
        public int Note(string key)
        {
            int n = Seen(key);
            _seen[key] = n + 1;
            return n;
        }

        public int Count => _seen.Count;
    }

    enum BeatKind { Customer, Morning, Peddler, Evening, Guild, Scene }

    /// <summary>One moment of play: a customer, a morning, Nim, an evening, the Guild's night, a scene.</summary>
    sealed class Beat
    {
        public int Day, Week, Tier;
        public BeatKind Kind;
        public string Label;
        public float Seconds, Novelty, Load, Peak, Choice;

        /// <summary>Things met for the very first time in this beat.</summary>
        public readonly List<string> New = new();

        /// <summary>The peaks in it, by name, for the report.</summary>
        public readonly List<(string Name, float Size)> Peaks = new();

        /// <summary>A lesson the player has already had in an earlier run: Bram teaching what they know.</summary>
        public bool Replay;

        /// <summary>Nothing new, nothing high, nothing to decide: the same again (a lesson replayed always is).</summary>
        public bool Routine => Replay || (Novelty < RoutineNovelty && Peak < RoutinePeak && Choice < RoutineChoice);

        /// <summary>Things met for the first time that mattered: something to learn, or something properly new.</summary>
        public int Significant;
    }

    // What makes a beat routine, and how fast familiar things wear thin.
    const float RoutineNovelty = 0.25f, RoutinePeak = 1f, RoutineChoice = 0.5f, Habituation = 0.6f;

    /// <summary>How much less of a decision each visit from the same seller is, this run.</summary>
    const float ChoiceHabituation = 0.9f;

    /// <summary>A peak counts as one of a run's high points from this size (after it's worn thin with repetition).</summary>
    const float BigPeak = 1.5f;

    /// <summary>What a lesson does to learning something: it's guided, so it weighs this much of what it would.</summary>
    const float GuidedLoad = 0.6f;

    /// <summary>A day's learning load past this, for someone new to the game, is too much at once.</summary>
    const float LoadCapacity = 6f;

    /// <summary>A mostly routine stretch at least this long (minutes) is a stale stretch.</summary>
    const float StaleMinutes = 8f;

    /// <summary>How a bought chest went, for the recorder.</summary>
    sealed class Opened
    {
        public int Value, Paid, Items, LuckChanged, Kept, Delivered;
        public bool Perfect, Locked, Forced, Song, SongFirst, WardNamed, Fooled, Broke;
        public readonly List<(Rarity Quality, int Steps)> Surprises = new();
        public readonly List<string> FirstFinds = new();
    }

    /// <summary>Records one run's beats, against what the player already knew.</summary>
    sealed class Recorder
    {
        public readonly Knowledge Know;
        public readonly List<Beat> Beats = new();
        readonly List<int> _profits = new();
        readonly List<string> _recentPeaks = new();
        readonly Dictionary<string, int> _visits = new();
        Beat _beat;
        int _streak, _bestStreak, _tier = 1;
        bool _guided;

        public Recorder(Knowledge know) => Know = know;

        Beat Begin(GameState s, BeatKind kind, string label, float seconds)
        {
            _beat = new Beat { Day = s.Day, Week = s.Week, Tier = s.Tier, Kind = kind, Label = label, Seconds = seconds };
            Beats.Add(_beat);
            return _beat;
        }

        /// <summary>
        /// Something met: the first time, it asks <paramref name="load"/> to learn and brings its novelty; after that it
        /// brings less each time (<see cref="Habituation"/>), and nothing to learn.
        /// </summary>
        void Meet(string key, float load, float novelty, string label = null)
        {
            int before = Know.Note(key);
            if (before == 0)
            {
                _beat.Load += load * (_guided ? GuidedLoad : 1);
                _beat.Novelty += novelty;
                _beat.New.Add(label ?? key);
                if (load > 0 || novelty >= 1) _beat.Significant++;
            }
            else _beat.Novelty += novelty * 0.5f * MathF.Pow(Habituation, before);
        }

        /// <summary>
        /// A high point, worn thin by how many of the same kind there have been lately (the fifth surprise in a row is
        /// less of a moment than the first).
        /// </summary>
        void Peak(string name, float size)
        {
            int recent = _recentPeaks.Count(p => p == name);
            size /= 1 + 0.35f * recent;
            _recentPeaks.Add(name);
            if (_recentPeaks.Count > 30) _recentPeaks.RemoveAt(0);
            _beat.Peak += size;
            _beat.Peaks.Add((name, size));
        }

        /// <summary>How much there is to learn about a seller: their rule and what they say, beyond listening and paying.</summary>
        static float Complexity(SellerDef s) =>
            (s.States is Clue.Articles or Clue.Pounds ? 1f : s.States != Clue.None ? 1.5f : 0) + (s.Packs ? 1 : 0) + (s.ShakeLimit > 0 ? 0.5f : 0) +
            (s.ShakeFee > 0 ? 0.5f : 0) + (s.HatesDrop || s.HatesMagnet || s.HatesHardShake ? 0.5f : 0) + (s.Wager > 0 ? 0.5f : 0) +
            (s.Heirloom != null ? 0.5f : 0) + (s.TakesDearest ? 0.5f : 0) + (s.SlipPrice ? 1 : 0) + (s.Gaze > 0 ? 0.5f : 0) +
            (s.Fakes.Length > 0 ? 0.5f : 0) + (s.Packhorse ? 0.5f : 0) + (s.Patience > 0 ? 0.5f : 0) +
            (s.Judge is Judgment.Miner or Judgment.Jeweller or Judgment.Crook or Judgment.Dreamer ? 0.5f : 0);

        public void Morning(GameState s)
        {
            Begin(s, BeatKind.Morning, $"day {s.Day}", 3);
            if (s.Day == 1) Meet("place:1", 1, 2, "Bram's stall");
            if (s.Tier != _tier)
            {
                Meet($"place:{s.Tier}", 1, 3, ShopTiers.Name(s.Tier));
                Peak("a new shop", 4);
                _tier = s.Tier;
            }
            if (s.TodayEvent is EventDef e)
            {
                _beat.Seconds += 6;
                Meet("sys:events", 0.5f, 0.5f, "events");
                Meet($"event:{e.Key}", e.Key is "dreaming" or "wardens" or "census" or "mourning" ? 1 : 0.4f, 1.5f, e.Name);
                Peak("an event", 0.6f);
            }
            if (s.Today.Stash > 0) Meet("sys:stash", 0.3f, 0.3f, "Bram's stash");
            if (s.Commissions.Count > 0 && s.DayOfWeek == 1) Meet("sys:commissions", 1, 0.8f, "commissions");
            if (s.Commissions.Any(c => c.Story)) Meet("story:hood", 0.5f, 1, "the Hooded Woman's asks");
        }

        public void Scene(GameState s, int lines)
        {
            if (lines == 0) return;
            Begin(s, BeatKind.Scene, $"week {s.Week}", 5 * lines);
            Meet($"story:week{s.Week}", 0.3f, 1.5f, $"week {s.Week}'s scene");
            // The reveals: the Locking and the Hooded Woman (week 2); Mara, the King and the Crown Chest (4).
            if (s.Week is 2 or Story.OfferWeek) Peak("a story reveal", 2.5f);
        }

        public void Peddler(GameState s, IReadOnlyList<CurioDef> stock, IReadOnlyList<CurioDef> bought, int affordable)
        {
            Begin(s, BeatKind.Peddler, "Nim", 25 + 10 * bought.Count);
            Meet("sys:curios", 1.5f, 1.5f, "Nim and curios");
            foreach (var curio in stock) Meet($"curio:{curio.Key}", 0, 0.3f, curio.Name);
            foreach (var curio in bought)
            {
                // A curio changes how you play: the first time you own one is something new to learn and use, and the
                // first time two go together (a way of trading) is a build taking shape.
                Meet($"own:{curio.Key}", 0.5f, 1f, $"owning the {curio.Name}");
                foreach (var tag in curio.Tags.Where(t => s.OwnedCurios.Any(o => o.Key != curio.Key && o.Has(t))))
                    Meet($"build:{tag}", 0.3f, 1f, tag == CurioTag.Trade ? "a trade's build" : $"a {Curios.TagName(tag)} build");
                Peak("a curio", 0.8f);
            }
            if (affordable >= 2) _beat.Choice += 1;
            else if (affordable == 1) _beat.Choice += 0.5f;
        }

        public void Customer(GameState s, Visit v, Hearing hearing, int worth, int? paid, Opened o, bool lesson)
        {
            var seller = v.Seller;
            float seconds = 12 + (lesson ? 35 : 0);
            if (hearing != null)
            {
                seconds += 4 * new[] { Shake.Gentle, Shake.Firm, Shake.Hard }.Count(l => hearing.HasTried(l, false));
                seconds += 3 * new[] { Shake.Gentle, Shake.Firm, Shake.Hard }.Count(l => hearing.HasTried(l, true));
                if (hearing.Dropped) seconds += 3;
            }
            seconds += v.Trade switch { Trade.Haggle => 6 + 4 * v.OffersMade, Trade.Offer => 8, _ => 3 };
            Begin(s, BeatKind.Customer, seller.Name, seconds);
            _guided = lesson;
            if (lesson) _beat.Replay = Know.Note($"lesson:{seller.Key}") > 0;

            Meet($"seller:{seller.Key}", 0.5f + Complexity(seller), 1.2f, seller.Name);
            if (v.PointsAtWard) Meet("mech:warded", 2, 1.5f, "warded things");
            if (v.Says is Clue.Articles or Clue.Tally) Meet("mech:counted", 0.5f, 0.5f, "a count of articles");
            if (v.Chest.Packed) Meet("mech:straw", 1, 0.8f, "straw");
            if (v.Chest.Sealed) Meet("mech:sealed", 0.5f, 1.2f, "a chained chest");
            if (v.Chest.Gilded) Meet("mech:gilded", 0.3f, 1, "a royal chest");
            if (v.Chest.Items.Any(i => i.Fake && i.Category == Category.Scrolls)) Meet("mech:sand", 1, 0.8f, "sand");
            if (v.Chest.Items.Any(i => i.Fake && i.Category is Category.Coins or Category.Gems)) Meet("mech:demongold", 1, 0.8f, "demon gold");
            if (v.Chest.Items.GroupBy(i => i.Category).Any(g => g.Count() > 1)) Meet("mech:pairs", 1, 0.6f, "two of a kind");
            if (s.Tier >= 2 && s.CanDrop) Meet("mech:drop", 1, 0.8f, "the drop");
            if (hearing != null && hearing.Hollow) Meet("mech:falsebottom", 1, 1, "false bottoms");
            if (s.Witchlight && v.Chest.Items.Any(i => i.Magic)) Meet("mech:glow", 1, 1, "enchantment");
            if (v.Chest.Song != null) Meet("mech:song", 1, 2, "a Heartsong's song");
            if (s.Day == 1) foreach (var basic in new[] { "shake", "slip", "clank", "gut" }) Meet($"basic:{basic}", 0.8f, 0.6f, basic);

            // Was it a real choice? A chest that could go either way (the odds bar near the middle), a price to name, a
            // haggle; not a clear pass or a clear buy.
            float ratio = v.Ask > 0 && worth > 0 ? (float)worth / v.Ask : 0;
            float choice = v.Trade == Trade.Offer ? 0.5f : ratio is >= 0.85f and <= 1.2f ? 1f : ratio is >= 0.7f and <= 1.45f ? 0.4f : 0.1f;
            if (v.Trade == Trade.Haggle) choice += 0.2f;
            // The same seller's chest for the umpteenth time is less of a decision than the first few: the patterns are learned.
            int visits = _visits[seller.Key] = _visits.GetValueOrDefault(seller.Key) + 1;
            _beat.Choice += choice * MathF.Max(0.25f, MathF.Pow(ChoiceHabituation, visits - 1));

            if (paid is not int price || o == null)
            {
                _beat.Seconds += 4;
                return;
            }
            // The opening, at a human's pace: the lock, every thing out of the chest, a climb for each surprise.
            _beat.Seconds += 6 + 2.5f * o.Items + 4 * o.Surprises.Sum(x => x.Steps) + (o.WardNamed ? 5 : 0) + (o.Song ? 12 : 0) + 3 * (o.Kept + o.Delivered);
            if (o.Locked)
            {
                _beat.Seconds += o.Forced ? 2 : 8;
                Meet("mech:lockpick", 1.5f, 1, "lockpicking");
            }
            // A jackpot: far more than chests have been making lately (not since the stall: the shop grows).
            int profit = o.Value - price;
            var recent = _profits.TakeLast(12).OrderBy(x => x).ToList();
            int median = recent.Count == 0 ? 0 : recent[recent.Count / 2];
            if (recent.Count >= 6 && profit > 0 && median > 0 && profit >= 3 * median) Peak("a jackpot", MathF.Min(4, MathF.Log2((float)profit / median)));
            _profits.Add(profit);
            foreach (var (quality, steps) in o.Surprises) Peak(quality == Rarity.Legendary ? "a legendary surprise" : "a surprise", steps * (quality >= Rarity.Epic ? 1.5f : 1));
            if (o.LuckChanged > 0) Peak("a curio's luck", 0.6f * o.LuckChanged);
            if (o.WardNamed) Peak("a warded thing given up", 2);
            if (o.Song) Peak("a Heartsong", o.SongFirst ? 8 : 5);
            foreach (var find in o.FirstFinds) Meet($"item:{find}", 0, 0.25f, find);
            if (o.Fooled) Meet("mech:fooled", 0.5f, 0.3f, "being fooled by a fake");
            if (o.Broke) Meet("mech:broke", 0.3f, 0.3f, "broken glass");
            _streak = o.Perfect ? _streak + 1 : 0;
            if (_streak > _bestStreak && _streak is 5 or 10 or 15)
            {
                _bestStreak = _streak;
                Peak("a run of perfect reads", 1.5f);
            }
            if (o.Delivered > 0) Peak("a commission done", 1);
        }

        /// <summary>A chest you couldn't afford burst as you passed, and let its Heartsong out to you.</summary>
        public void Burst(GameState s, Item song)
        {
            Begin(s, BeatKind.Customer, "a Heartsong bursts free", 20);
            Meet("mech:burst", 0.5f, 2, "a Heartsong bursting free");
            Peak("a Heartsong", s.Songbook.Contains(song.Song) ? 5 : 8);
            _beat.Choice += 1;
        }

        public void Evening(GameState s, int bought, IReadOnlyList<Upgrade> upgrades, bool synergyChoice, int affordable)
        {
            Begin(s, BeatKind.Evening, $"day {s.Day} ends", 18 + 8 * upgrades.Count + (synergyChoice ? 10 : 0));
            if (s.UpgradesOpen) Meet("sys:upgrades", 1, 1, "spending on the shop");
            if (s.DisplayOpen) Meet("sys:display", 1, 0.8f, "the display");
            if (s.SynergiesOpen) Meet("sys:synergies", 2, 1.2f, "synergies");
            foreach (var u in upgrades)
            {
                Meet($"upgrade:{u.Key}", u.Key is "lamp" or "magnet" ? 1.5f : u.IsRenovation ? 0.5f : 0.3f, 1, u.Name);
                Peak(u.IsRenovation ? "moving inward" : "an upgrade", u.IsRenovation ? 1 : 0.6f);
            }
            if (affordable >= 2) _beat.Choice += 1;
            if (synergyChoice) _beat.Choice += 0.5f;
            if (s.Day == GameState.IntroDay) Meet("sys:dues", 1, 1.5f, "the Guild's dues");
        }

        public void Guild(GameState s, int gold, int due, string outcome)
        {
            Begin(s, BeatKind.Guild, $"the Guild, week {s.Week}", 15 + (outcome is "pawned" or "grace" ? 20 : 0));
            Meet("sys:guildnight", 0.5f, 1, "the Guild's night");
            float cushion = due <= 0 ? 9 : (float)gold / due;
            switch (outcome)
            {
                case "paid" when cushion < 1.5f:
                    Peak("a close night with the Guild", 2.5f);
                    break;
                case "paid" when cushion > 4:
                    break;
                case "paid":
                    Peak("the Guild paid", 0.8f);
                    break;
                case "pawned":
                    Meet("sys:pawn", 1, 1, "pawning");
                    Peak("pawning to pay the Guild", 3);
                    break;
                case "grace":
                    Meet("sys:grace", 0.5f, 1, "Bram's word");
                    Peak("Bram vouches for you", 3);
                    break;
                case "lost":
                    Peak("the Guild takes the shop", 3);
                    break;
            }
            // Tension: how much it mattered tonight.
            _beat.Choice += cushion < 1.5f ? 1 : cushion < 2.5f ? 0.5f : 0;
        }

        public void Cleared(GameState s, bool early)
        {
            Begin(s, BeatKind.Scene, "the debt cleared", 20);
            Peak("the debt cleared", early ? 6 : 5);
        }

        public void Vault(GameState s)
        {
            Begin(s, BeatKind.Scene, "the Crown Chest", 40);
            Meet("story:vault", 0.5f, 3, "the Crown Chest");
            Peak("the Crown Chest", 8);
        }
    }

    /// <summary>A day's sums, for one run.</summary>
    sealed record DayTally(int Day, int Tier, float Minutes, int NewThings, float Load, float Novelty, float Peak, float Choice, float StaleMinutes, bool Overload,
        string[] New, int Customers, int BigPeaks);

    /// <summary>One run as played: its beats, and the run itself.</summary>
    sealed record Played(int Run, float Skill, string Character, GameState State, List<Beat> Beats)
    {
        public float Minutes => Beats.Sum(b => b.Seconds) / 60;
    }

    /// <summary>
    /// The stale stretches in a run: where customers have mostly been routine lately (a moving average of how routine
    /// each was, <see cref="Beat.Routine"/>, at or over <see cref="StaleShare"/>: one surprise doesn't rescue ten dull
    /// customers), for at least <see cref="StaleMinutes"/>. Mornings and evenings don't count either way.
    /// </summary>
    static List<(int FromDay, int ToDay, int Tier, float Minutes, int Beats)> Stale(List<Beat> beats)
    {
        var stretches = new List<(int, int, int, float, int)>();
        float minutes = 0, average = 0;
        int count = 0, from = 0, tier = 0, to = 0;
        foreach (var b in beats.Where(b => b.Kind == BeatKind.Customer))
        {
            average = average * (1 - StaleSmoothing) + (b.Routine ? 1 : 0) * StaleSmoothing;
            if (average >= StaleShare)
            {
                if (count == 0)
                {
                    from = b.Day;
                    tier = b.Tier;
                }
                minutes += b.Seconds / 60;
                count++;
                to = b.Day;
            }
            else
            {
                if (minutes >= StaleMinutes) stretches.Add((from, to, tier, minutes, count));
                minutes = 0;
                count = 0;
            }
        }
        if (minutes >= StaleMinutes) stretches.Add((from, to, tier, minutes, count));
        return stretches;
    }

    /// <summary>How quickly the moving average follows each customer, and how routine it has to be to call it stale.</summary>
    const float StaleSmoothing = 0.2f, StaleShare = 0.8f;

    static List<DayTally> Tally(List<Beat> beats)
    {
        var stale = Stale(beats);
        return beats.GroupBy(b => b.Day).OrderBy(g => g.Key).Select(g =>
        {
            float minutes = g.Sum(b => b.Seconds) / 60;
            float load = g.Sum(b => b.Load);
            var news = g.SelectMany(b => b.New).ToArray();
            float staleHere = stale.Where(st => st.FromDay <= g.Key && st.ToDay >= g.Key).Sum(st => st.Minutes / Math.Max(1, st.ToDay - st.FromDay + 1));
            return new DayTally(g.Key, g.Max(b => b.Tier), minutes, g.Sum(b => b.Significant), load, g.Sum(b => b.Novelty), g.Sum(b => b.Peak), g.Sum(b => b.Choice),
                staleHere, load > LoadCapacity, news, g.Count(b => b.Kind == BeatKind.Customer), g.Sum(b => b.Peaks.Count(x => x.Size >= BigPeak)));
        }).ToList();
    }

    /// <summary>
    /// The experience playtest: <paramref name="players"/> players, each starting with no knowledge and playing
    /// <paramref name="runs"/> runs (Kit, tips on, improving as <see cref="Careers"/> does; nothing else carries over
    /// but the journal and what Bram has taught). Prints run by run: how long it takes, where it goes stale, where it overloads, its peaks, a
    /// day-by-day curve, and what would improve it.
    /// </summary>
    public static void ExperienceReport(int players, int runs = 3)
    {
        var all = new List<Played>();
        for (int p = 0; p < players; p++)
        {
            var who = new Random(77 + p * 31);
            float start = who.Between(0f, 0.3f), best = who.Between(0.75f, 1f), pace = who.Between(0.1f, 0.3f);
            var profile = new Profile();
            var know = new Knowledge();
            for (int r = 0; r < runs; r++)
            {
                float skill = best - (best - start) * MathF.Exp(-pace * r);
                var rec = new Recorder(know);
                var style = Learner(skill, Characters.Kit, profile.MaxChallenge(Characters.Kit)) with { Guided = true };
                var (_, state) = Play(style, 700000 + p * 1000 + r, Guild.Weeks * Guild.DaysPerWeek, profile.Songs, rec, profile.Taught, profile.OpenCurios);
                profile.Runs++;
                profile.Note(state);
                if (state.DebtCleared)
                {
                    profile.Cleared++;
                    profile.ClearedAt(state);
                }
                if (state.VaultOpened) profile.Vaults++;
                profile.Bank(state);
                all.Add(new Played(r + 1, skill, Characters.Kit, state, rec.Beats));
            }
        }

        GD.Print($"=== Experience playtest: {players} players, {runs} runs each (Kit, tips on, a learner improving run by run) ===");
        GD.Print("Minutes are a human's pace (shaking, reading, the opening's show), not the sim's. Load: what there is to learn; a day");
        GD.Print($"over {LoadCapacity:0} is too much at once. Stale: {StaleMinutes:0}+ minutes of customers with nothing new, nothing high and nothing to decide.");
        for (int r = 1; r <= runs; r++)
        {
            var played = all.Where(x => x.Run == r).ToList();
            PrintRun(r, played);
        }
        Recommend(all, runs);
    }

    static float Median(IEnumerable<float> xs)
    {
        var list = xs.OrderBy(x => x).ToList();
        return list.Count == 0 ? 0 : list[list.Count / 2];
    }

    static void PrintRun(int run, List<Played> played)
    {
        int n = played.Count;
        var tallies = played.Select(p => Tally(p.Beats)).ToList();
        var stale = played.Select(p => Stale(p.Beats)).ToList();
        float minutes = Median(played.Select(p => p.Minutes));
        float customers = Median(played.Select(p => (float)p.Beats.Count(b => b.Kind == BeatKind.Customer)));
        float days = Median(played.Select(p => (float)p.State.Day));
        GD.Print($"\n--- Run {run} (skill {Median(played.Select(p => p.Skill)):0.00}) ---");
        GD.Print($"length: {minutes:0} minutes ({minutes / 60:0.0} h) over {days:0} days and {customers:0} customers (medians); " +
                 $"debt cleared {100f * played.Count(p => p.State.DebtCleared) / n:0}%, lost {100f * played.Count(p => p.State.Foreclosed) / n:0}% " +
                 $"(median week {Median(played.Where(p => p.State.Foreclosed).Select(p => (float)p.State.Week)):0})");
        float newPerHour = Median(played.Select(p => p.Beats.Sum(b => b.Significant) / Math.Max(0.1f, p.Minutes / 60)));
        float peaksPerHour = Median(played.Select(p => p.Beats.Sum(b => b.Peaks.Count(x => x.Size >= BigPeak)) / Math.Max(0.1f, p.Minutes / 60)));
        float staleShare = Median(played.Select(p => Stale(p.Beats).Sum(s => s.Minutes) / Math.Max(1, p.Minutes)));
        float longest = Median(stale.Select(s => s.Count == 0 ? 0 : s.Max(x => x.Minutes)));
        float replayed = Median(played.Select(p => p.Beats.Where(b => b.Replay).Sum(b => b.Seconds) / 60));
        GD.Print($"freshness: {newPerHour:0.0} new things an hour; {peaksPerHour:0.0} high points an hour; {100 * staleShare:0}% of the time in stale stretches " +
                 $"(longest {longest:0} min, median player){(replayed > 0 ? $"; {replayed:0} min of lessons already learned" : "")}");
        // Days in a row with nothing new that matters.
        float flat = Median(tallies.Select(t =>
        {
            int best = 0, run = 0;
            foreach (var d in t)
            {
                run = d.NewThings == 0 ? run + 1 : 0;
                best = Math.Max(best, run);
            }
            return (float)best;
        }));
        GD.Print($"the longest run of days with nothing new: {flat:0} (median player)");
        var byTier = stale.SelectMany(s => s).GroupBy(s => s.Tier).OrderBy(g => g.Key)
            .Select(g => $"{ShopTiers.Name(g.Key)} {g.Sum(x => x.Minutes) / n:0} min");
        GD.Print($"stale minutes per run, by shop: {string.Join(", ", byTier)}");
        var seen = played.SelectMany(p => p.Beats.Where(b => b.Kind == BeatKind.Customer)).ToList();
        GD.Print("routine customers, by shop: " + string.Join(", ", seen.GroupBy(b => b.Tier).OrderBy(g => g.Key)
            .Select(g => $"{ShopTiers.Name(g.Key)} {100f * g.Count(b => b.Routine) / g.Count():0}% of {(float)g.Count() / n:0} " +
                         $"({g.Sum(b => b.Seconds) / 60 / n:0} min)")));
        var overloads = tallies.SelectMany(t => t.Where(d => d.Overload)).ToList();
        GD.Print($"overload: {100f * tallies.Count(t => t.Any(d => d.Overload)) / n:0}% of players have a day over capacity; " +
                 $"heaviest day median load {Median(tallies.Select(t => t.Count == 0 ? 0 : t.Max(d => d.Load))):0.0} " +
                 $"(on day {Median(tallies.Select(t => t.Count == 0 ? 0f : t.OrderByDescending(d => d.Load).First().Day)):0})");
        if (overloads.Count > 0)
        {
            var what = overloads.SelectMany(d => d.New).Where(x => !Categories.Kinds.Any(k => k.Names.Contains(x))).GroupBy(x => x)
                .OrderByDescending(g => g.Count()).Take(12)
                .Select(g => $"{g.Key} ({100 * g.Count() / Math.Max(1, overloads.Count)}%)");
            GD.Print($"  what arrives on overloaded days: {string.Join(", ", what)}");
        }
        var peaks = played.SelectMany(p => p.Beats.SelectMany(b => b.Peaks)).GroupBy(x => x.Name)
            .OrderByDescending(g => g.Sum(x => x.Size)).Take(8).Select(g => $"{g.Key} {(float)g.Count() / n:0.0}x");
        GD.Print($"peaks a run: {string.Join(", ", peaks)}");

        GD.Print("day  shop  minutes  new  load  novelty  peaks  highs  choices  engagement  stale min  overload  (medians over players still playing)");
        int maxDay = played.Max(p => p.State.Day);
        for (int d = 1; d <= maxDay; d++)
        {
            var today = tallies.Select(t => t.FirstOrDefault(x => x.Day == d)).Where(x => x != null).ToList();
            if (today.Count < n / 4) break;
            float m = Median(today.Select(t => t.Minutes));
            float engagement = Median(today.Select(Engagement));
            GD.Print($"{d,3}  {Median(today.Select(t => (float)t.Tier)),4:0}  {m,7:0.0}  {Median(today.Select(t => (float)t.NewThings)),3:0}  " +
                     $"{Median(today.Select(t => t.Load)),4:0.0}  {Median(today.Select(t => t.Novelty)),7:0.0}  {Median(today.Select(t => t.Peak)),5:0.0}  " +
                     $"{Median(today.Select(t => (float)t.BigPeaks)),5:0}  " +
                     $"{Median(today.Select(t => t.Choice)),7:0.0}  {engagement,10:0.0}  {Median(today.Select(t => t.StaleMinutes)),9:0.0}  " +
                     $"{100f * today.Count(t => t.Overload) / today.Count,7:0}%");
        }
    }

    /// <summary>
    /// A day's engagement, per ten minutes of play: novelty, peaks and real choices (a choice counts a little less than a
    /// new thing or a peak). The stretch the player is in counts, not just the totals.
    /// </summary>
    static float Engagement(DayTally d) => d.Minutes <= 0 ? 0 : 10 * (d.Novelty + d.Peak + 0.6f * d.Choice) / d.Minutes;

    /// <summary>What the numbers say to change, worked out from the report's own measures.</summary>
    static void Recommend(List<Played> all, int runs)
    {
        GD.Print("\n--- What to change ---");
        var first = all.Where(p => p.Run == 1).ToList();
        float minutes = Median(first.Select(p => p.Minutes));
        if (minutes > 120)
            GD.Print($"* A first run takes about {minutes / 60:0.0} hours: too long to want another. Aim for 60-90 minutes (fewer days, fewer customers a day).");
        foreach (int r in Enumerable.Range(1, runs))
        {
            var played = all.Where(p => p.Run == r).ToList();
            var stale = played.SelectMany(p => Stale(p.Beats)).ToList();
            foreach (var tier in stale.GroupBy(s => s.Tier).Where(g => g.Sum(s => s.Minutes) / played.Count >= 15).OrderBy(g => g.Key))
                GD.Print($"* Run {r}: at {ShopTiers.Name(tier.Key)}, {tier.Sum(s => s.Minutes) / played.Count:0} minutes a run of routine customers " +
                         "(nothing new, nothing high, nothing to decide). Shorten the stay there, or give it something to do: a choice, a twist, a payoff.");
            var heavy = played.Select(p => Tally(p.Beats)).SelectMany(t => t.Where(d => d.Load > LoadCapacity)).ToList();
            if (heavy.Count > played.Count / 3)
            {
                var days = heavy.GroupBy(d => d.Day).OrderByDescending(g => g.Count()).Take(3).Select(g => $"day {g.Key}");
                var what = heavy.SelectMany(d => d.New).GroupBy(x => x).OrderByDescending(g => g.Count()).Take(6).Select(g => g.Key);
                GD.Print($"* Run {r}: too much at once on {string.Join(", ", days)} ({string.Join(", ", what)}). Spread these over more days.");
            }
        }
        if (runs >= 2)
        {
            float Fresh(int r) => Median(all.Where(p => p.Run == r).Select(p => p.Beats.Sum(b => b.Significant) / Math.Max(0.1f, p.Minutes / 60)));
            for (int r = 2; r <= runs; r++)
                if (Fresh(r) < Fresh(1) * 0.35f)
                    GD.Print($"* Run {r} brings {Fresh(r):0.0} new things an hour against run 1's {Fresh(1):0.0}: repeat runs need fresh content " +
                             "(builds that play differently, things only later runs meet).");
            float replays = Median(all.Where(p => p.Run >= 2).Select(p => (float)p.Beats.Count(b => b.Replay)));
            if (replays > 0)
                GD.Print($"* Runs 2+ replay {replays:0} of Bram's lessons a run: keep what's been taught in the profile, not the run.");
        }
    }
}
