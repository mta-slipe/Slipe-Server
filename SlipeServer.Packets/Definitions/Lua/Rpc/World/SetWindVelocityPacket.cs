using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class SetWindVelocityPacket(Vector3 velocity) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public Vector3 WindVelocity { get; set; } = velocity;

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_WIND_VELOCITY);
        builder.WriteVelocityVector(this.WindVelocity);

        return builder.Build();
    }
}
