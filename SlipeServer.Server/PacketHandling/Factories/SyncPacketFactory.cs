using System;
using System.Linq;
using SlipeServer.Packets.Definitions.Sync;

namespace SlipeServer.Server.PacketHandling.Factories
{
    public static class SyncPacketFactory
    {
        public static SetSyncSettingsPacket CreateSyncSettingsPacket(Configuration configuration)
        {
            return new SetSyncSettingsPacket(
                configuration.BulletSyncEnabledWeapons.Select(w => (byte)w).ToArray(), configuration.VehicleExtrapolationPercentage != 0,
                configuration.VehicleExtrapolationBaseMilliseconds, configuration.VehicleExtrapolationPercentage,
                configuration.VehicleExtrapolationMaxMilliseconds, configuration.UseAlternativePulseOrder,
                configuration.AllowFastSprintFix, configuration.AllowDriveByAnimationFix,
                configuration.AllowShotgunDamageFix
            );
        }
    }
}
