using SlipeServer.Net.Wrappers.Enums;
using SlipeServer.Server.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace SlipeServer.Server;

/// <summary>
/// Server configuration
/// </summary>
public class Configuration
{
    /// <summary>
    /// Server name, as it will appear in the server browser.
    /// </summary>
    [MaxLength(128)]
    public string ServerName { get; set; } = "Default Slipe server";

    /// <summary>
    /// Server host IP address, usually left 0.0.0.0
    /// </summary>
    [MaxLength(15)]
    public string Host { get; set; } = "0.0.0.0";

    /// <summary>
    /// Server UDP port players connect to
    /// Note: A server's ASE port will be this value + 123
    /// </summary>
    public ushort Port { get; set; } = 50666;

    /// <summary>
    /// Server UDP port for players to connect to using a debug build of MTA
    /// Leaving this `null` will not allow debug clients to connect
    /// </summary>
    public ushort? DebugPort { get; set; } = null;

    /// <summary>
    /// Maximum player count
    /// </summary>
    public ushort MaxPlayerCount { get; set; } = 64;

    /// <summary>
    /// Server password
    /// </summary>
    [MaxLength(128)]
    public string? Password { get; set; } = null;


    /// <summary>
    /// HTTP host to listen to for built-in HTTP server for serving resource files
    /// </summary>
    public string HttpHost { get; set; } = "*";

    /// <summary>
    /// HTTP port to listen to for built-in HTTP server for serving resource files
    /// </summary>
    public ushort HttpPort { get; set; } = 22005;

    /// <summary>
    /// HTTP URL for an external HTTP server for serving resource files
    /// </summary>
    public string? HttpUrl { get; set; }

    /// <summary>
    /// Maximum HTTP connections allowed per client
    /// </summary>
    public int HttpConnectionsPerClient { get; set; } = 1;

    /// <summary>
    /// Directory to server client-side Lua resources from
    /// </summary>
    public string ResourceDirectory { get; set; } = "./Resources";

    /// <summary>
    /// In-game distance to sync explosions, any player within this distance from an explosion will be informed of the explosion
    /// </summary>
    public float ExplosionSyncDistance { get; set; } = 400;

    /// <summary>
    /// Distance used to determine what client should sync a ped
    /// </summary>
    public int PedSyncerDistance { get; set; } = 250;

    /// <summary>
    /// Distance used to determine what client should syc an unoccupied vehicle
    /// </summary>
    public int UnoccupiedVehicleSyncerDistance { get; set; } = 250;

    /// <summary>
    /// Distance after which a player should be light-synced. Meaning it receives less frequent updates, and is not visible on the client.
    /// </summary>
    public float LightSyncRange { get; set; } = 800;

    /// <summary>
    /// Bitstream version used by the server.
    /// Only needs editing if you implement additional packet handling / packet changes that were implemented in a newer version of MTA
    /// </summary>
    public ushort BitStreamVersion { get; set; } = 119;

    /// <summary>
    /// Sets whether voice chat is enabled
    /// </summary>
    public bool IsVoiceEnabled { get; set; }

    /// <summary>
    /// Sets whether fake lag is enabled
    /// </summary>
    public bool IsFakeLagCommandEnabled { get; set; } = true;

    /// <summary>
    /// Minimum version for connecting clients
    /// </summary>
    public Version MinVersion { get; set; } = new Version(1, 5, 8);

    /// <summary>
    /// Weapon ids that will triggered bullet sync packets to be sent to the server.
    /// </summary>
    public WeaponId[] BulletSyncEnabledWeapons { get; set; } = new WeaponId[] {
            WeaponId.Colt,
            WeaponId.Silenced,
            WeaponId.Deagle,
            WeaponId.Shotgun,
            WeaponId.CombatShotgun,
            WeaponId.Sawnoff,
            WeaponId.Tec9,
            WeaponId.Uzi,
            WeaponId.Mp5,
            WeaponId.Ak47,
            WeaponId.M4,
            WeaponId.Rifle,
            WeaponId.Sniper
        };

    public short VehicleExtrapolationBaseMilliseconds { get; set; } = 5;
    public short VehicleExtrapolationPercentage { get; set; } = 0;
    public short VehicleExtrapolationMaxMilliseconds { get; set; } = 150;

    public bool UseAlternativePulseOrder { get; set; } = false;
    public bool AllowFastSprintFix { get; set; } = true;
    public bool AllowDriveByAnimationFix { get; set; } = true;
    public bool AllowShotgunDamageFix { get; set; } = true;

    /// <summary>
    /// Amount of bytes worth of latent packets to send per second 
    /// </summary>
    public uint LatentBandwidthLimit { get; set; } = 50 * 1000 * 1000;
    
    /// <summary>
    /// Interval in milliseconds between latent packets to be sent
    /// </summary>
    public ushort LatentSendInterval { get; set; } = 100;

    /// <summary>
    /// Anticheat configuration
    /// </summary>
    public AntiCheatConfiguration AntiCheat { get; set; } = new();

    /// <summary>
    /// Intervals for player sync
    /// </summary>
    public SyncIntervals SyncIntervals { get; set; } = new();
}

/// <summary>
/// Contains the anti-cheat configuration for the server
/// </summary>
public class AntiCheatConfiguration
{
    public AllowGta3ImgMods AllowGta3ImgMods { get; set; } = AllowGta3ImgMods.None;
    public SpecialDetection[] EnableSpecialDetections { get; set; } = Array.Empty<SpecialDetection>();
    public AntiCheat[] DisabledAntiCheat { get; set; } = Array.Empty<AntiCheat>();
    public DataFile FileChecks { get; set; } = DataFile.None;
    public bool HideAntiCheat { get; set; } = false;
    public int VerifyClientSettings { get; set; } = -1;
}

/// <summary>
/// Contains the synchronisation intervals in milliseconds at which clients send synchronisation data
/// </summary>
public class SyncIntervals
{
    public int PureSync { get; set; } = 100;
    public int LightSync { get; set; } = 1500;
    public int CamSync { get; set; } = 500;
    public int PedSync { get; set; } = 400;
    public int UnoccupiedVehicle { get; set; } = 400;
    public int ObjectSync { get; set; } = 500;
    public int KeySyncRotation { get; set; } = 25;
    public int KeySyncAnalogMove { get; set; } = 25;
}
