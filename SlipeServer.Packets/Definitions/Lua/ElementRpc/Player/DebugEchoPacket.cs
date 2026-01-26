using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;

public sealed class DebugEchoPacket(string message, byte level, Color color) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_DEBUG_ECHO;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public string Message { get; set; } = message;
    public byte Level { get; set; } = level;
    public Color Color { get; set; } = color;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.Level);
        if (this.Level == 0)
        {
            builder.Write(this.Color);
        }
        builder.WriteStringWithoutLength(this.Message);

        return builder.Build();
    }
}
