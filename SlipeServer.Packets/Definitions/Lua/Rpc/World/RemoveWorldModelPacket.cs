using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class RemoveWorldModelPacket(ushort model, float radius, Vector3 position, byte interior) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ushort ModelID { get; set; } = model;
    public float Radius { get; set; } = radius;
    public Vector3 Position { get; set; } = position;
    public byte Interior { get; set; } = interior;

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.REMOVE_WORLD_MODEL);
        builder.Write(this.ModelID);
        builder.Write(this.Radius);
        builder.Write(this.Position);
        builder.Write(this.Interior);

        return builder.Build();
    }
}
