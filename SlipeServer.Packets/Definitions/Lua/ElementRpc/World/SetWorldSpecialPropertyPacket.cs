using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.World;

public sealed class SetWorldSpecialPropertyPacket(byte property, bool isEnabled) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public byte Property { get; set; } = property;
    public bool IsEnabled { get; set; } = isEnabled;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_WORLD_SPECIAL_PROPERTY);
        builder.Write(this.Property);
        builder.Write(this.IsEnabled);

        return builder.Build();
    }
}
