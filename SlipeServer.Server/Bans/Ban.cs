using System;
using System.Net;

namespace SlipeServer.Server.Bans;

public class Ban
{
    public Guid Id { get; set; }
    public string? Serial { get; set; }
    public IPAddress? IPAddress { get; set; }
    public DateTime? ExpiryDateUtc { get; set; }
    public string? Reason { get; set; }
    public string? BannedPlayerName { get; set; }
    public string? BannerName { get; set; }

    public static Ban CreateAndValidate(string? serial, IPAddress? ipAddress, DateTime? expiryTimeUtc, string? reason, string? bannedPlayerName, string? bannerName)
    {
        if (serial == null && ipAddress == null)
            throw new InvalidBanException("A ban requires at least a serial or an IP address");

        if (expiryTimeUtc < DateTime.UtcNow)
            throw new Exception("A ban can not expire in the past");

        return new Ban()
        {
            Id = Guid.NewGuid(),
            Serial = serial,
            IPAddress = ipAddress,
            ExpiryDateUtc = expiryTimeUtc,
            Reason = reason,
            BannedPlayerName = bannedPlayerName,
            BannerName = bannerName
        };
    }

    public bool IsActive
        => this.ExpiryDateUtc == null || this.ExpiryDateUtc > DateTime.UtcNow;
}
