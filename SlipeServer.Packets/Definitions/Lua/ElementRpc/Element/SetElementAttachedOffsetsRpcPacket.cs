using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class SetElementAttachedOffsetsRpcPacket(ElementId elementId, Vector3 position, Vector3 rotation) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; } = elementId;
    public Vector3 Position { get; set; } = position;
    public Vector3 Rotation { get; set; } = rotation;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_ATTACHED_OFFSETS);
        builder.Write(this.ElementId);

        builder.Write(this.Position);
        builder.Write(this.Rotation);

        return builder.Build();
    }
}
