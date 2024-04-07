using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;

public class SetControlStatePacket(string control, bool enabled) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public string Control { get; set; } = control;
    public bool Enabled { get; set; } = enabled;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_CONTROL_STATE);
        builder.WriteStringWithByteAsLength(this.Control);
        builder.Write((byte)(this.Enabled ? 1 : 0));
        return builder.Build();
    }
}
