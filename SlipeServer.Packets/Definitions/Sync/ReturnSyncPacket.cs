using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Sync;

public sealed class ReturnSyncPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_RETURN_SYNC;
    public override PacketReliability Reliability => PacketReliability.Reliable;
    public override PacketPriority Priority => PacketPriority.High;

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public bool IsInVechicle { get; set; }


    public ReturnSyncPacket()
    {

    }

    public ReturnSyncPacket(bool isInVehicle, Vector3 position, Vector3 rotation)
    {
        this.IsInVechicle = isInVehicle;
        this.Position = position;
        this.Rotation = rotation;
    }

    public ReturnSyncPacket(Vector3 position, Vector3 rotation)
    {
        this.IsInVechicle = true;
        this.Position = position;
        this.Rotation = rotation;
    }

    public ReturnSyncPacket(Vector3 position)
    {
        this.IsInVechicle = false;
        this.Position = position;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.IsInVechicle);
        if (this.IsInVechicle)
        {
            builder.WriteVector3WithZAsFloat(this.Position);
            builder.WriteVehicleRotation(this.Rotation);
        } else
        {
            builder.WriteVector3WithZAsFloat(this.Position);
        }

        return builder.Build();
    }
}
