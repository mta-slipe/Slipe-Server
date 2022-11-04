using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public class SetSkyGradientPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public Color Top { get; set; }
    public Color Bottom { get; set; }

    public SetSkyGradientPacket(Color? topColor = null, Color? bottomColor = null)
    {
        this.Top = topColor ?? Color.FromArgb(0, 0, 0);
        this.Bottom = bottomColor ?? Color.FromArgb(0, 0, 0);
    }

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_SKY_GRADIENT);
        builder.Write(this.Top);
        builder.Write(this.Bottom);

        return builder.Build();
    }
}
