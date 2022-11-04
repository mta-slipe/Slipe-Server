using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Builder;

namespace SlipeServer.Packets.Definitions.Sync;

public class SetSyncSettingsPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_SYNC_SETTINGS;
    public override PacketReliability Reliability => PacketReliability.Unreliable;
    public override PacketPriority Priority => PacketPriority.Low;

    public byte[] BulletSyncWeaponIds { get; }
    public bool ExtrapolationEnabled { get; }
    public short ExtrapolationBaseMilliSeconds { get; }
    public short ExtrapolationPercentage { get; }
    public short ExtrapolationMaxMilliseconds { get; }
    public bool UseAlternativePulseOrder { get; }
    public bool AllowFastSprintFix { get; }
    public bool AllowDriveByAnimationFix { get; }
    public bool AllowShotgunDamageFix { get; }

    public SetSyncSettingsPacket(
        byte[] bulletSyncWeaponIds, bool extrapolationEnabled, short extrapolationBaseMilliSeconds,
        short extrapolationPercentage, short extrapolationMaxMilliseconds,
        bool useAlternativePulseOrder, bool allowFastSprintFix,
        bool allowDriveByAnimationFix, bool allowShotgunDamageFix
    )
    {
        this.BulletSyncWeaponIds = bulletSyncWeaponIds;
        this.ExtrapolationEnabled = extrapolationEnabled;
        this.ExtrapolationBaseMilliSeconds = extrapolationBaseMilliSeconds;
        this.ExtrapolationPercentage = extrapolationPercentage;
        this.ExtrapolationMaxMilliseconds = extrapolationMaxMilliseconds;
        this.UseAlternativePulseOrder = useAlternativePulseOrder;
        this.AllowFastSprintFix = allowFastSprintFix;
        this.AllowDriveByAnimationFix = allowDriveByAnimationFix;
        this.AllowShotgunDamageFix = allowShotgunDamageFix;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)this.BulletSyncWeaponIds.Length);
        builder.Write(this.BulletSyncWeaponIds);

        builder.Write((byte)(this.ExtrapolationEnabled ? 1 : 0));
        builder.Write(this.ExtrapolationBaseMilliSeconds);
        builder.Write(this.ExtrapolationPercentage);
        builder.Write(this.ExtrapolationMaxMilliseconds);

        builder.Write((byte)(this.UseAlternativePulseOrder ? 1 : 0));
        builder.Write((byte)(this.AllowFastSprintFix ? 1 : 0));
        builder.Write((byte)(this.AllowDriveByAnimationFix ? 1 : 0));
        builder.Write((byte)(this.AllowShotgunDamageFix ? 1 : 0));

        return builder.Build();
    }
}
