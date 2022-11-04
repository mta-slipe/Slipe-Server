using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Builder;

namespace SlipeServer.Packets.Definitions.Sync;

public class SetSyncIntervalPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.Unreliable;
    public override PacketPriority Priority => PacketPriority.Low;

    public int PureSync { get; set; }
    public int LightSync { get; set; }
    public int CamSync { get; set; }
    public int PedSync { get; set; }
    public int UnoccupiedVehicle { get; set; }
    public int ObjectSync { get; set; }
    public int KeySyncRotation { get; set; }
    public int KeySyncAnalogMove { get; set; }

    public SetSyncIntervalPacket(
        int pureSync,
        int lightSync,
        int camSync,
        int pedSync,
        int unoccupiedVehicle,
        int objectSync,
        int keySyncRotation,
        int keySyncAnalogMove
    )
    {
        this.PureSync = pureSync;
        this.LightSync = lightSync;
        this.CamSync = camSync;
        this.PedSync = pedSync;
        this.UnoccupiedVehicle = unoccupiedVehicle;
        this.ObjectSync = objectSync;
        this.KeySyncRotation = keySyncRotation;
        this.KeySyncAnalogMove = keySyncAnalogMove;
    }

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
