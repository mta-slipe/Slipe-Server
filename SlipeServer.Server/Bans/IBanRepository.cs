using System;
using System.Collections.Generic;
using System.Net;

namespace SlipeServer.Server.Bans;

public interface IBanRepository
{
    public IEnumerable<Ban> GetBans();
    public Ban AddBan(string? serial, IPAddress? ipAddress, DateTime? expiryTimeUtc = null, string? reason = null, string? bannedPlayerName = null, string? bannerName = null);
    public void RemoveBans(string? serial, IPAddress? ipAddress);
    public void RemoveBan(Ban ban) => RemoveBan(ban.Id);
    public void RemoveBan(Guid id);

    public bool IsIpOrSerialBanned(string? serial, IPAddress? ipAddress, out Ban? ban);
}
