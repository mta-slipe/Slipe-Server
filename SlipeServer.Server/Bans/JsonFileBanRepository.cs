using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace SlipeServer.Server.Bans;

public class JsonFileBanRepository : IBanRepository
{
    private readonly string filepath;

    private readonly List<Ban> bans = [];

    public JsonFileBanRepository(string filepath = "bans.json")
    {
        this.filepath = filepath;
        LoadBans();
    }

    private void LoadBans()
    {
        if (File.Exists(this.filepath))
        {
            var fileContents = File.ReadAllText(this.filepath);
            var bans = JsonSerializer.Deserialize<IEnumerable<JsonBan>>(fileContents)
                ?.Select(x => (Ban)x);

            if (bans != null)
                foreach (var ban in bans.Where(x => x.IsActive))
                    this.bans.Add(ban);
        }
    }

    public Ban AddBan(string? serial, IPAddress? ipAddress, DateTime? expiryTimeUtc = null, string? reason = null, string? bannedPlayerName = null, string? bannerName = null)
    {
        var ban = Ban.CreateAndValidate(serial, ipAddress, expiryTimeUtc, reason, bannedPlayerName, bannerName);
        this.bans.Add(ban);
        SaveBans();
        return ban;
    }

    public IEnumerable<Ban> GetBans()
    {
        return this.bans.ToArray();
    }

    public bool IsIpOrSerialBanned(string? serial, IPAddress? ipAddress, out Ban? ban)
    {
        ban = this.bans
            .Where(x => x.ExpiryDateUtc == null || x.ExpiryDateUtc > DateTime.UtcNow)
            .FirstOrDefault(x =>                
                (x.Serial != null && x.Serial == serial) ||
                (x.IPAddress != null && x.IPAddress == ipAddress)
            );

        return ban != null;
    }

    public void RemoveBan(Guid id)
    {
        var ban = this.bans.SingleOrDefault(x => x.Id == id);
        if (ban == null)
            return;

        this.bans.Remove(ban);
        SaveBans();
    }

    public void RemoveBan(Ban ban)
    {
        this.bans.Remove(ban);
        SaveBans();
    }

    public void RemoveBans(string? serial, IPAddress? ipAddress)
    {
        var banCopy = this.bans.ToArray();
        foreach (var ban in banCopy)
        {
            if (
                (ban.Serial != null && ban.Serial == serial) ||
                (ban.IPAddress != null && ban.IPAddress == ipAddress)
            )
                this.bans.Remove(ban);
        }
        SaveBans();
    }

    private void SaveBans()
    {
        var json = JsonSerializer.Serialize(this.bans.Where(x => x.IsActive).Select(x => (JsonBan)x));
        File.WriteAllText(this.filepath, json);
    }

    public record JsonBan(Guid Id, string? Serial, string? IPAddress, DateTime? ExpiryDateUtc, string? Reason, string? BannedPlayerName, string? BannerName)
    {
        public static implicit operator Ban(JsonBan ban) => new()
        {
            Id = ban.Id,
            Serial = ban.Serial,
            IPAddress = ban.IPAddress == null ? null : System.Net.IPAddress.Parse(ban.IPAddress),
            ExpiryDateUtc = ban.ExpiryDateUtc,
            Reason = ban.Reason,
            BannedPlayerName = ban.BannedPlayerName,
            BannerName = ban.BannerName,
        };

        public static explicit operator JsonBan(Ban ban)
            => new(ban.Id, ban.Serial, ban.IPAddress?.ToString(), ban.ExpiryDateUtc, ban.Reason, ban.BannedPlayerName, ban.BannerName);
    }
}
