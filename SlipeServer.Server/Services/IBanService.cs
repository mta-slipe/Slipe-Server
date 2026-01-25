using SlipeServer.Server.Bans;
using System;
using System.Collections.Generic;
using System.Net;

namespace SlipeServer.Server.Services;

public interface IBanService
{
    Ban AddBan(string? serial, IPAddress? ipAddress, DateTime? expiryTimeUtc = null, string? reason = null, string? bannedPlayerName = null, string? bannerName = null);
    IEnumerable<Ban> GetBans();
    bool IsIpOrSerialBanned(string? serial, IPAddress? ipAddress, out Ban? ban);
    void RemoveBan(Ban ban);
    void RemoveBan(Guid id);
    void RemoveBans(string? serial, IPAddress? ipAddress);
}