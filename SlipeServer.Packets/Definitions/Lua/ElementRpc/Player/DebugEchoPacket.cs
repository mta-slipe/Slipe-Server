using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;

public class DebugEchoPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_DEBUG_ECHO;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public string Message { get; set; }
    public byte Level { get; set; }
    public Color Color { get; set; }

    public DebugEchoPacket(string message, byte level, Color color)
    {
        this.Message = message;
        this.Level = level;
        this.Color = color;
    }

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
