using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public class ResetMoonSizePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ResetMoonSizePacket() { }

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.RESET_MOON_SIZE);

        return builder.Build();
    }
}
