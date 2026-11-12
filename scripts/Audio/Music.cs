using System.Collections.Generic;
using Godot;
using Keywright.Core;
using Keywright.Phases;

namespace Keywright.Audio;

/// <summary>
/// The music and the ambience under it (0.36; until then the game had neither): a theme for the title and the
/// crossroads, one for each market further in, the night, the Guild's night and the treasury, and a bed of ambience for
/// each place (res://assets/audio/music and ambience, written and rendered by tools/music/make.py). One of each plays
/// at a time, and a change fades across. <see cref="For"/> chooses by the screen and the shop; both play on the Music
/// bus, which has its own volume in Settings. A Heartsong's song has the room to itself: the music steps back for it.
/// </summary>
public partial class Music : Node
{
    /// <summary>The ambience sits this far under the music.</summary>
    const float BedDb = -5f;

    readonly Dictionary<string, AudioStream> _streams = new();
    AudioStreamPlayer[] _music, _beds;
    int _musicAt, _bedAt;
    string _track, _bed;
    float _duck;
    double _ducked;
    bool _released;

    public override void _Ready()
    {
        _music = new[] { Voice(), Voice() };
        _beds = new[] { Voice(), Voice() };
    }

    AudioStreamPlayer Voice()
    {
        var player = new AudioStreamPlayer { Bus = Settings.MusicBus, VolumeDb = -80 };
        AddChild(player);
        return player;
    }

    /// <summary>The theme and the bed for a screen: by what it is, the shop's market, and whether it's night.</summary>
    public void For(Phase phase, GameState state)
    {
        int tier = state?.Tier ?? 1;
        string market = tier switch { 1 => "crossroads", 2 => "thornwick", 3 => "crownhold", _ => "emporium" };
        string outside = tier switch { 1 => "day", 4 => "hall", _ => "market" };
        switch (phase)
        {
            case TitlePhase or SetupPhase:
                Play("crossroads");
                Bed("day");
                break;
            case TreasuryPhase:
                Play("treasury");
                Bed(null);
                break;
            case DuesPhase:
                Play("guild");
                Bed("night");
                break;
            case DayEndPhase or PeddlerPhase or RunEndPhase:
                Play("night");
                Bed("night");
                break;
            case TalkPhase talk when talk.AtNight:
                Play("night");
                Bed("night");
                break;
            // A scene keeps the music it came in on, unless it's the Guild's counting-house or the treasury.
            case ScenePhase scene:
                if (scene.Backdrop == "guild") Play("guild");
                else if (scene.Backdrop is "vault" or "keyhold") Play("treasury");
                else if (_track == null) Play(market);
                break;
            default:
                Play(market);
                Bed(outside);
                break;
        }
    }

    /// <summary>Fades to a theme (null: to silence). The same one again carries on where it was.</summary>
    public void Play(string track, float seconds = 1.6f)
    {
        if (_released || track == _track) return;
        _track = track;
        Cross(_music, ref _musicAt, track == null ? null : Load($"res://assets/audio/music/{track}.ogg"), seconds, 0);
    }

    /// <summary>Fades to a bed of ambience (null: none).</summary>
    public void Bed(string bed, float seconds = 2.2f)
    {
        if (_released || bed == _bed) return;
        _bed = bed;
        Cross(_beds, ref _bedAt, bed == null ? null : Load($"res://assets/audio/ambience/{bed}.ogg"), seconds, BedDb);
    }

    void Cross(AudioStreamPlayer[] pair, ref int at, AudioStream next, float seconds, float db)
    {
        var old = pair[at];
        at = 1 - at;
        var fresh = pair[at];
        if (old.Playing)
        {
            var fade = old.CreateTween();
            fade.TweenProperty(old, "volume_db", -60f, seconds).SetTrans(Tween.TransitionType.Sine);
            fade.TweenCallback(Callable.From(old.Stop));
        }
        if (next == null) return;
        fresh.Stream = next;
        fresh.VolumeDb = -50;
        fresh.Play();
        fresh.CreateTween().TweenProperty(fresh, "volume_db", db, seconds).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
    }

    AudioStream Load(string path)
    {
        if (_streams.TryGetValue(path, out var known)) return known;
        if (!ResourceLoader.Exists(path))
        {
            GD.PushWarning($"No music at {path}");
            return _streams[path] = null;
        }
        var stream = GD.Load<AudioStream>(path);
        if (stream is AudioStreamOggVorbis ogg) ogg.Loop = true;
        return _streams[path] = stream;
    }

    /// <summary>Steps back for a while (a Heartsong singing, the hush before a chest bursts): down, held, and back up.</summary>
    public void Duck(double seconds, float db = -14f)
    {
        _duck = db;
        _ducked = Mathf.Max((float)_ducked, (float)seconds);
    }

    public override void _Process(double delta)
    {
        if (_released) return;
        int bus = AudioServer.GetBusIndex(Settings.MusicBus);
        if (bus < 0) return;
        _ducked -= delta;
        float target = _ducked > 0 ? _duck : 0f, now = Settings.MusicDuckDb;
        // Down quickly, back up slowly.
        float next = Mathf.MoveToward(now, target, (float)delta * (target < now ? 40f : 9f));
        if (!Mathf.IsEqualApprox(next, now))
        {
            Settings.MusicDuckDb = next;
            Settings.ApplyAudio(Settings.Muted);
        }
    }

    /// <summary>Stops everything and lets go of the streams, so quitting leaves nothing playing behind.</summary>
    public void Release()
    {
        _released = true;
        foreach (var player in _music) Stop(player);
        foreach (var player in _beds) Stop(player);
        _streams.Clear();
    }

    static void Stop(AudioStreamPlayer player)
    {
        player.Stop();
        player.Stream = null;
    }
}
