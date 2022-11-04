using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public class SetTimePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte Hour { get; set; }
    public byte Minute { get; set; }

    public SetTimePacket(byte hour, byte minute)
    {
        this.Hour = hour;
        this.Minute = minute;
    }
    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_TIME);
        builder.Write(this.Hour);
        builder.Write(this.Minute);

        return builder.Build();
    }
}
