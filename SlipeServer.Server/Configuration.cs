using SlipeServer.Net.Enums;
using SlipeServer.Server.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace SlipeServer.Server
{
    public class Configuration
    {
        [MaxLength(128)]
        public string ServerName { get; set; } = "Default Slipe server";

        [MaxLength(15)]
        public string Host { get; set; } = "0.0.0.0";

        public ushort Port { get; set; } = 50666;

        public ushort MaxPlayerCount { get; set; } = 64;

        [MaxLength(128)]
        public string Password { get; set; } = "";


        public string HttpHost { get; set; } = "*";

        public ushort HttpPort { get; set; } = 40680;

        public string? HttpUrl { get; set; }

        public int HttpConnectionsPerClient { get; set; } = 1;

        public string ResourceDirectory { get; set; } = "./resources";

        public float ExplosionSyncDistance { get; set; } = 400;

        public bool IsVoiceEnabled { get; set; }

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


        public AntiCheatConfiguration AntiCheat { get; set; } = new AntiCheatConfiguration();
    }

    public class AntiCheatConfiguration
    {
        public AllowGta3ImgMods AllowGta3ImgMods { get; set; } = AllowGta3ImgMods.None;
        public SpecialDetection[] EnableSpecialDetections { get; set; } = new SpecialDetection[] { };
        public AntiCheat[] DisabledAntiCheat { get; set; } = new AntiCheat[] { };
        public DataFile FileChecks { get; set; } = DataFile.None;
        public bool HideAntiCheat { get; set; } = false;
        public int VerifyClientSettings { get; set; } = -1;
    }
}
