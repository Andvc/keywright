using System;
using System.Collections.Generic;
using Godot;
using Keywright.Core;
using Steamworks;

namespace Keywright;

/// <summary>
/// Steam, when the game's running on it: achievements (<see cref="Achievements"/>, kept in the profile either way),
/// what the player's doing for their friends to see (rich presence, worded by steam/rich_presence.vdf), and the
/// overlay, which pauses the game. Without Steam (a DRM-free build, the bot, the sim, a computer without the client, or
/// steam_api64.dll missing) every call does nothing and the game plays the same. steam/README.md has the Steamworks
/// side.
/// </summary>
public static class Platform
{
    /// <summary>Keywright's Steam app ID: 0 until the store page exists. Set, a release build started outside Steam
    /// asks Steam to start it instead.</summary>
    public const uint AppId = 0;

    public static bool OnSteam { get; private set; }

    static bool _statsReady;
    static readonly List<string> _pending = new();
    static Callback<GameOverlayActivated_t> _overlay;
    static Callback<UserStatsReceived_t> _stats;

    /// <summary>
    /// Connects to Steam if it's there. False if the game should quit at once: started outside Steam, which is
    /// starting it again (only with an <see cref="AppId"/>, and never in a debug build).
    /// </summary>
    public static bool Start(Action<bool> overlayShown)
    {
        try
        {
            if (AppId != 0 && !OS.IsDebugBuild() && SteamAPI.RestartAppIfNecessary(new AppId_t(AppId))) return false;
            OnSteam = SteamAPI.Init();
        }
        catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
        {
            GD.Print("Steam: no steam_api library here, so playing without Steam.");
            OnSteam = false;
            return true;
        }
        catch (Exception e)
        {
            GD.PushWarning($"Steam: {e.Message}");
            OnSteam = false;
            return true;
        }
        GD.Print(OnSteam ? "Steam: connected." : "Steam: not running, so playing without it.");
        if (!OnSteam) return true;
        _overlay = Callback<GameOverlayActivated_t>.Create(shown => overlayShown?.Invoke(shown.m_bActive != 0));
        _stats = Callback<UserStatsReceived_t>.Create(received =>
        {
            if (received.m_eResult != EResult.k_EResultOK) return;
            _statsReady = true;
            foreach (string key in _pending) SteamUserStats.SetAchievement(key);
            if (_pending.Count > 0) SteamUserStats.StoreStats();
            _pending.Clear();
        });
        SteamUserStats.RequestCurrentStats();
        return true;
    }

    /// <summary>Steam's callbacks, every frame.</summary>
    public static void Tick()
    {
        if (OnSteam) SteamAPI.RunCallbacks();
    }

    public static void Stop()
    {
        if (!OnSteam) return;
        SteamAPI.Shutdown();
        OnSteam = false;
    }

    /// <summary>Marks achievements earned on Steam (again, harmlessly, for ones it already has: one earned off Steam
    /// gets there the next time the game runs on it).</summary>
    public static void Unlock(IEnumerable<string> keys)
    {
        if (!OnSteam) return;
        bool any = false;
        foreach (string key in keys)
        {
            if (_statsReady) SteamUserStats.SetAchievement(key);
            else _pending.Add(key);
            any = true;
        }
        if (any && _statsReady) SteamUserStats.StoreStats();
    }

    /// <summary>
    /// What the player's doing, for their friends list: "At the title screen", or "Day 3 at Thornwick Market"
    /// (tokens in steam/rich_presence.vdf, worded by Steam in each friend's language).
    /// </summary>
    public static void Presence(GameState state)
    {
        if (!OnSteam) return;
        if (state == null)
        {
            SteamFriends.SetRichPresence("steam_display", "#Status_Title");
            return;
        }
        SteamFriends.SetRichPresence("tier", state.Tier.ToString());
        SteamFriends.SetRichPresence("day", state.Day.ToString());
        SteamFriends.SetRichPresence("week", state.Week.ToString());
        SteamFriends.SetRichPresence("steam_display", state.DebtCleared ? "#Status_Cleared" : "#Status_Day");
    }
}
