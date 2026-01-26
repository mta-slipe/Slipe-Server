using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Builder;

namespace SlipeServer.Packets.Definitions.Sync;

public sealed class SetSyncIntervalPacket(
    int pureSync,
    int lightSync,
    int camSync,
    int pedSync,
    int unoccupiedVehicle,
    int objectSync,
    int keySyncRotation,
    int keySyncAnalogMove
    ) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.Unreliable;
    public override PacketPriority Priority => PacketPriority.Low;

    public int PureSync { get; set; } = pureSync;
    public int LightSync { get; set; } = lightSync;
    public int CamSync { get; set; } = camSync;
    public int PedSync { get; set; } = pedSync;
    public int UnoccupiedVehicle { get; set; } = unoccupiedVehicle;
    public int ObjectSync { get; set; } = objectSync;
    public int KeySyncRotation { get; set; } = keySyncRotation;
    public int KeySyncAnalogMove { get; set; } = keySyncAnalogMove;

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRPCFunction.SET_SYNC_INTERVALS);
        builder.Write(this.PureSync);
        builder.Write(this.LightSync);
        builder.Write(this.CamSync);
        builder.Write(this.PedSync);
        builder.Write(this.UnoccupiedVehicle);
        builder.Write(this.ObjectSync);
        builder.Write(this.KeySyncRotation);
        builder.Write(this.KeySyncAnalogMove);

        return builder.Build();
    }
}
