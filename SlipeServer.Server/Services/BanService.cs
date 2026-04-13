using SlipeServer.Server.Bans;
using System.Collections.Generic;
using System.Net;
using System;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System.Linq;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.Services;

/// <summary>
/// Allows you to ban IPs or serials
/// </summary>
public class BanService(IBanRepository banRepository, IElementCollection elementCollection) : IBanService
{
    public IEnumerable<Ban> GetBans() => banRepository.GetBans();
    public Ban AddBan(string? serial, IPAddress? ipAddress, DateTime? expiryTimeUtc = null, string? reason = null, string? bannedPlayerName = null, string? bannerName = null)
    {
        var ban = banRepository.AddBan(serial, ipAddress, expiryTimeUtc, reason, bannedPlayerName, bannerName);

        var playersToDisconnect = elementCollection
            .GetByType<Player>()
            .Select(x =>
            {
                var isBanned = IsIpOrSerialBanned(x.Client.Serial, x.Client.IPAddress, out var ban);
                return new {
                    player = x,
                    isBanned = isBanned,
                    ban = ban
                };
            })
            .Where(x => x.isBanned);

        foreach (var player in playersToDisconnect)
        {
            var disconnectType = player.ban?.Serial != null ? PlayerDisconnectType.BANNED_SERIAL : PlayerDisconnectType.BANNED_IP;
            player.player.Kick(ban.Reason ?? "Unknown", disconnectType);
        }

        this.BanAdded?.Invoke(this, new BanEventArgs(ban));

        return ban;

    }

    public void RemoveBans(string? serial, IPAddress? ipAddress)
    {
        var bansToRemove = GetBans()
            .Where(x => (serial != null && x.Serial == serial) || (ipAddress != null && x.IPAddress?.Equals(ipAddress) == true))
            .ToArray();
        banRepository.RemoveBans(serial, ipAddress);
        foreach (var ban in bansToRemove)
            this.BanRemoved?.Invoke(this, new BanEventArgs(ban));
    }

    public void RemoveBan(Ban ban)
    {
        banRepository.RemoveBan(ban);
        this.BanRemoved?.Invoke(this, new BanEventArgs(ban));
    }

    public void RemoveBan(Guid id)
    {
        var ban = GetBans().FirstOrDefault(x => x.Id == id);
        banRepository.RemoveBan(id);
        if (ban != null)
            this.BanRemoved?.Invoke(this, new BanEventArgs(ban));
    }

    public bool IsIpOrSerialBanned(string? serial, IPAddress? ipAddress, out Ban? ban) => banRepository.IsIpOrSerialBanned(serial, ipAddress, out ban);

    public event EventHandler<BanEventArgs>? BanAdded;
    public event EventHandler<BanEventArgs>? BanRemoved;
}
