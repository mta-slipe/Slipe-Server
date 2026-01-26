using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class SetGlitchEnabledPacket(byte glitchType, bool enabled) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte GlitchType { get; set; } = glitchType;
    public byte Enabled { get; set; } = (byte)(enabled ? 1 : 0);

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_GLITCH_ENABLED);
        builder.Write(this.GlitchType);
        builder.Write(this.Enabled);

        return builder.Build();
    }
}
