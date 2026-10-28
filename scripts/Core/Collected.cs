using System.Linq;

namespace Keywright.Core;

/// <summary>
/// What the collection shows (opened from the title screen or the pause menu): every thing Bram's kin have found (and
/// found enchanted), every curio they've seen and every Heartsong they've heard, across all their runs (the profile),
/// with the run being played, when there is one. The notebook keeps what's for trading; this is what's been gathered.
/// </summary>
public sealed class Collected
{
    readonly Profile _profile;
    readonly GameState _run;

    public Collected(Profile profile, GameState run = null)
    {
        _profile = profile ?? new Profile();
        _run = run;
    }

    /// <summary>A run is being played (the pause menu's collection), so it can say what's yours and what's singing now.</summary>
    public bool InRun => _run != null;

    public bool Found(string entry) => _profile.Found.Contains(entry) || _run?.InJournal(entry) == true;

    public bool FoundEnchanted(string entry) => _profile.FoundEnchanted.Contains(entry) || _run?.InJournalEnchanted(entry) == true;

    /// <summary>Things found, every run together.</summary>
    public int FoundCount => Collection.Entries.Count(e => Found(e.Entry));

    /// <summary>Things found this run (0 from the title screen).</summary>
    public int FoundThisRun => _run?.Found.Count ?? 0;

    /// <summary>Nim's laid it out, or you've had it, in any run.</summary>
    public bool SawCurio(string key) => _profile.CuriosSeen.Contains(key) || _run?.SawCurio(key) == true;

    /// <summary>It's yours this run.</summary>
    public bool Owns(string key) => _run?.HasCurio(key) == true;

    /// <summary>Nim carries it: this run's stock, or (from the title screen) the next run's.</summary>
    public bool Carried(string key) => _run?.CurioOpen(key) ?? _profile.Opens(Curios.Get(key));

    /// <summary>How many times Bram's kin have done what opens a legendary curio, this run included.</summary>
    public int FeatDone(Feat feat) => _profile.Done(feat) + (_run == null ? 0 : Curios.Progress(_run, feat));

    public bool KnowsSong(string key) => _profile.Songs.Contains(key) || _run?.Songbook.Contains(key) == true;

    /// <summary>On display in the shop now, singing.</summary>
    public bool Singing(string key) => _run?.Shows(key) == true;

    public int SongsKnown => Heartsongs.All.Count(h => KnowsSong(h.Key));
}
