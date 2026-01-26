using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Builder;

namespace SlipeServer.Packets.Definitions.Sync;

public sealed class SetSyncSettingsPacket(
    byte[] bulletSyncWeaponIds, bool extrapolationEnabled, short extrapolationBaseMilliSeconds,
    short extrapolationPercentage, short extrapolationMaxMilliseconds,
    bool useAlternativePulseOrder, bool allowFastSprintFix,
    bool allowDriveByAnimationFix, bool allowShotgunDamageFix
    ) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_SYNC_SETTINGS;
    public override PacketReliability Reliability => PacketReliability.Unreliable;
    public override PacketPriority Priority => PacketPriority.Low;

    public byte[] BulletSyncWeaponIds { get; } = bulletSyncWeaponIds;
    public bool ExtrapolationEnabled { get; } = extrapolationEnabled;
    public short ExtrapolationBaseMilliSeconds { get; } = extrapolationBaseMilliSeconds;
    public short ExtrapolationPercentage { get; } = extrapolationPercentage;
    public short ExtrapolationMaxMilliseconds { get; } = extrapolationMaxMilliseconds;
    public bool UseAlternativePulseOrder { get; } = useAlternativePulseOrder;
    public bool AllowFastSprintFix { get; } = allowFastSprintFix;
    public bool AllowDriveByAnimationFix { get; } = allowDriveByAnimationFix;
    public bool AllowShotgunDamageFix { get; } = allowShotgunDamageFix;

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
