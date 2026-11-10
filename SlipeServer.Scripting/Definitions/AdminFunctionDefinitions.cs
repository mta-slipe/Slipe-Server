using SlipeServer.Packets.Enums;
using SlipeServer.Server;
using SlipeServer.Server.Bans;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.Net;

namespace SlipeServer.Scripting.Definitions;

public class AdminFunctionDefinitions(IMtaServer server, IBanService banService)
{
    [ScriptFunctionDefinition("addBan")]
    public Ban AddBan(string? serial, IPAddress? ipAddress, DateTime? expiryTimeUtc, string? reason, string? bannedPlayerName, string? bannerName)
    {
        Ban ban = banService.AddBan(serial, ipAddress, expiryTimeUtc, reason, bannedPlayerName, bannerName);
        return ban;
    }

    [ScriptFunctionDefinition("banPlayer")]
    public Ban BanPlayer(Player bannedPlayer, Player? responsibleElement,
        string? reason, DateTime? expiryDate, bool banByIp = true, bool banBySerial = false, string? bannerName = "Console")
    {
        return this.AddBan(bannedPlayer.Client.Serial, bannedPlayer.Client.IPAddress, expiryDate, reason,
            bannedPlayer.Name, bannerName);
    }

    [ScriptFunctionDefinition("getBanAdmin")]
    public string? GetBanAdmin(Ban ban)
    {
        return ban.BannerName;
    }

    [ScriptFunctionDefinition("getBanIP")]
    public IPAddress? GetBanIp(Ban ban)
    {
        return ban.IPAddress;
    }

    [ScriptFunctionDefinition("GetBanNick")]
    public string? GetBanNick(Ban ban)
    {
        return ban.BannedPlayerName;
    }

    [ScriptFunctionDefinition("getBanSerial")]
    public string? GetBanSerial(Ban ban)
    {
        return ban.Serial;
    }

    [ScriptFunctionDefinition("getBanReason")]
    public string? GetBanReason(Ban ban)
    {
        return ban.Reason;
    }

    [ScriptFunctionDefinition("getBanTime")]
    public DateTime? GetBanTime(Ban ban)
    {
        return ban.BannedDateUtc;
    }

    [ScriptFunctionDefinition("getBans")]
    public IEnumerable<Ban> GetBans()
    {
        return banService.GetBans();
    }

    [ScriptFunctionDefinition("getUnbanTime")]
    public DateTime? GetUnbanTime(Ban ban)
    {
        return ban.ExpiryDateUtc;
    }
    
    [ScriptFunctionDefinition("isBan")]
    // I think this will be needed for lua scripts compatability. i'm not sure for now
    public bool IsBan(Ban ban)
    {
        return true; // ??
    }

    [ScriptFunctionDefinition("kickPlayer")]
    public bool KickPlayer(Player player, string? reason, Player? responsiblePlayer = null)
    {
        player.Kick(reason ?? "Unknown", PlayerDisconnectType.KICK, responsiblePlayer);
        return true;
    }

    [ScriptFunctionDefinition("setBanAdmin")]
    public bool SetBanAdmin(Ban ban, string bannerName)
    {
        ban.BannerName = bannerName;
        return true;
    }

    [ScriptFunctionDefinition("setBanNick")]
    public bool SetBanNick(Ban ban, string nick)
    {
        ban.BannedPlayerName = nick;
        return true;
    }

    [ScriptFunctionDefinition("setBanReason")]
    public bool SetBanReason(Ban ban, string reason)
    {
        ban.Reason = reason;
        return true;
    }

    [ScriptFunctionDefinition("setUnbanTime")]
    public bool SetBanTime(Ban ban, DateTime newUnbanTime)
    {
        // Do we need to check for the right datetime?
        ban.ExpiryDateUtc = newUnbanTime;
        return true;
    }

    // Do we need this too?
    [ScriptFunctionDefinition("reloadBans")]
    public bool ReloadBans()
    {
        // ??
        return true;
    }

    [ScriptFunctionDefinition("removeBan")]
    public bool RemoveBan(Ban ban)
    {
        banService.RemoveBan(ban);
        return true;
    }
    
}
