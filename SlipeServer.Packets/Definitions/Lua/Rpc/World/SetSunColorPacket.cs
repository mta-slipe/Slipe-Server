using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public class SetSunColorPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public Color CoreSun { get; set; }
    public Color CoronaSun { get; set; }

    public SetSunColorPacket(Color coreColor, Color coronaColor)
    {
        this.CoreSun = coreColor;
        this.CoronaSun = coronaColor;
    }

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
