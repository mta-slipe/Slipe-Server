using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class PlayerScriptDefinitions(IElementCollection elementCollection, DebugLog debugLog)
{

    [ScriptFunctionDefinition("getAlivePlayers")]
    public IEnumerable<Player> GetAlivePlayers()
    {
        return elementCollection
            .GetByType<Player>()
            .Where(x => x.IsAlive);
    }

    [ScriptFunctionDefinition("getDeadPlayers")]
    public IEnumerable<Player> GetDeadPlayers()
    {
        return elementCollection
            .GetByType<Player>()
            .Where(x => !x.IsAlive);
    }

    [ScriptFunctionDefinition("getPlayersInTeam")]
    public IEnumerable<Player> GetPlayersInTeam(Team team)
    {
        return elementCollection
            .GetByType<Player>()
            .Where(x => x.Team == team);
    }

    [ScriptFunctionDefinition("countPlayersInTeam")]
    public int CountPlayersInTeam(Team team)
    {
        return elementCollection
            .GetByType<Player>()
            .Count(x => x.Team == team);
    }


    [ScriptFunctionDefinition("getPlayerCount")]
    public int GetPlayerCount()
    {
        return elementCollection
            .GetByType<Player>()
            .Count();
    }

    [ScriptFunctionDefinition("getPlayerIdleTime")]
    public int GetPlayerIdleTime(Player player)
    {
        return (int)(DateTime.UtcNow - player.LastMovedUtc).TotalMilliseconds;
    }

    [ScriptFunctionDefinition("getPlayerIP")]
    public string GetPlayerIp(Player player)
    {
        return player.Client.IPAddress?.ToString() ?? "UNKNOWN";
    }

    [ScriptFunctionDefinition("getPlayerVersion")]
    public string GetPlayerVersion(Player player)
    {
        return player.Client.Version ?? "UNKNOWN";
    }

    [ScriptFunctionDefinition("getRandomPlayer")]
    public Player? GetRandomPlayer()
    {
        var players = elementCollection
            .GetByType<Player>();

        if (!players.Any())
            return null;

        return players.ElementAt(Random.Shared.Next(0, players.Count()));
    }

    [ScriptFunctionDefinition("redirectPlayer")]
    public void RedirectPlayer(Player player, string ip = "", int port = 0, string? password = "")
    {
        player.RedirectTo(IPAddress.Parse(ip), (ushort)port, password);
    }

    [ScriptFunctionDefinition("resendPlayerACInfo")]
    public void ResendPlayerACInfo(Player player)
    {
        player.ResendPlayerACInfo();
    }

    [ScriptFunctionDefinition("resendPlayerModInfo")]
    public void ResendPlayerModInfo(Player player)
    {
        player.ResendModPackets();
    }

    [ScriptFunctionDefinition("getPlayerFromName")]
    public Player? GetPlayerFromName(string name)
    {
        return elementCollection
            .GetByType<Player>()
            .SingleOrDefault(x => x.Name == name);
    }


    [ScriptFunctionDefinition("spawnPlayer")]
    public void SpawnPlayer(Player player, Vector3 position, int rotation, int model, int interior, int dimension, Team team)
    {
        player.Spawn(position, rotation, (ushort)model, (byte)interior, (ushort)dimension);
        player.Team = team;
    }

    [ScriptFunctionDefinition("takePlayerScreenShot")]
    public void TakePlayerScreenShot(
        Player player,
        int width,
        int height,
        string tag = "",
        int quality = 30,
        int maxBandwidth = 5000,
        int maxPacketSize = 500)
    {
        player.TakeScreenshot((ushort)width, (ushort)height, tag, (byte)quality, (uint)maxBandwidth, (ushort)maxPacketSize);
    }


    [ScriptFunctionDefinition("getPlayerTeam")]
    public Team? GetPlayerTeam(Player player)
    {
        return player.Team;
    }

    [ScriptFunctionDefinition("setPlayerTeam")]
    public void SetPlayerTeam(Player player, Team team)
    {
        player.Team = team;
    }


    [ScriptFunctionDefinition("getPlayerBlurLevel")]
    public int GetPlayerBlurLevel(Player player)
    {
        return player.BlurLevel;
    }

    [ScriptFunctionDefinition("setPlayerBlurLevel")]
    public int SetPlayerBlurLevel(Player player, int level)
    {
        return player.BlurLevel = (byte)level;
    }


    [ScriptFunctionDefinition("getPlayerSerial")]
    public string GetPlayerSerial(Player player)
    {
        if (player.Client.Serial == null)
            throw new Exception($"Player {player.Name} has null serial. This shouldn't happen");

        return player.Client.Serial;
    }


    [ScriptFunctionDefinition("setPlayerScriptDebugLevel")]
    public void SetPlayerScriptDebugLevel(Player player, int level)
    {
        debugLog.SetVisibleTo(player, level > 0);
        player.DebugLogLevel = level;
    }

    [ScriptFunctionDefinition("getPlayerScriptDebugLevel")]
    public int GetPlayerScriptDebugLevel(Player player)
    {
        return player.DebugLogLevel;
    }


    [ScriptFunctionDefinition("setPlayerWantedLevel")]
    public void SetPlayerWantedLevel(Player player, int wantedLevel)
    {
        player.WantedLevel = (byte)wantedLevel;
    }

    [ScriptFunctionDefinition("getPlayerWantedLevel")]
    public int GetPlayerWantedLevel(Player player)
    {
        return player.WantedLevel;
    }


    [ScriptFunctionDefinition("isPlayerMuted")]
    public bool IsPlayerMuted(Player player)
    {
        return player.IsChatMuted;
    }

    [ScriptFunctionDefinition("setPlayerMuted")]
    public void SetPlayerMuted(Player player, bool isMuted)
    {
        player.IsChatMuted = isMuted;
    }


    [ScriptFunctionDefinition("getPlayerMoney")]
    public int GetPlayerMoney(Player player)
    {
        lock (player.ExternalMoneyLock)
        {
            return player.Money;
        }
    }

    [ScriptFunctionDefinition("setPlayerMoney")]
    public void SetPlayerMoney(Player player, int money, bool instant = false)
    {
        lock (player.ExternalMoneyLock)
        {
            player.SetMoney(money, instant);
        }
    }

    [ScriptFunctionDefinition("takePlayerMoney")]
    public void TakePlayerMoney(Player player, int money)
    {
        lock (player.ExternalMoneyLock)
        {
            player.SetMoney(player.Money - money);
        }
    }

    [ScriptFunctionDefinition("giveMoney")]
    public bool GiveMoney(Player player, int money)
    {
        lock (player.ExternalMoneyLock)
        {
            player.SetMoney(player.Money + money);
            return true;
        }
    }

    [ScriptFunctionDefinition("tryTakePlayerMoney")]
    public bool TryTakePlayerMoney(Player player, int money)
    {
        lock (player.ExternalMoneyLock)
        {
            if (player.Money < money)
                return false;

            player.SetMoney(player.Money - money);
            return true;
        }
    }



    [ScriptFunctionDefinition("getPlayerName")]
    public string GetPlayerName(Player player)
    {
        return player.Name;
    }

    [ScriptFunctionDefinition("getPlayerNametagText")]
    public string GetPlayerNametagText(Player player)
    {
        return player.NametagText;
    }

    [ScriptFunctionDefinition("setPlayerNametagText")]
    public void SetPlayerNametagText(Player player, string value)
    {
        player.NametagText = value;
    }

    [ScriptFunctionDefinition("getPlayerNametagColor")]
    public Color GetPlayerNametagColor(Player player)
    {
        return player.NametagColor ?? Color.White;
    }

    [ScriptFunctionDefinition("setPlayerNametagColor")]
    public void SetPlayerNametagColor(Player player, Color value)
    {
        player.NametagColor = value;
    }


    [ScriptFunctionDefinition("getPlayerPing")]
    public int GetPlayerPing(Player player)
    {
        return (int)player.Client.Ping;
    }


    [ScriptFunctionDefinition("isPlayerMapForced")]
    public bool ForcePlayerMap(Player player)
    {
        return player.IsMapForced;
    }

    [ScriptFunctionDefinition("forcePlayerMap")]
    public void ForcePlayerMap(Player player, bool visible)
    {
        player.IsMapForced = visible;
    }


    [ScriptFunctionDefinition("setControlState")]
    public void SetControlState(Player player, string control, bool state)
    {
        player.Controls.SetControlState(control, state);
    }

    [ScriptFunctionDefinition("getControlState")]
    public bool GetControlState(Player player, string control)
    {
        return player.Controls.IsControlStateSet(control);
    }
}
