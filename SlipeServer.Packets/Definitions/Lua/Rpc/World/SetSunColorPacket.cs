using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class SetSunColorPacket(Color coreColor, Color coronaColor) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public Color CoreSun { get; set; } = coreColor;
    public Color CoronaSun { get; set; } = coronaColor;

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_SUN_COLOR);
        builder.Write(this.CoreSun);
        builder.Write(this.CoronaSun);

        return builder.Build();
    }
}
