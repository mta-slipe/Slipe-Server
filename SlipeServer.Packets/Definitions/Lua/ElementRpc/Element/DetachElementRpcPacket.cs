using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class DetachElementRpcPacket(ElementId elementId, Vector3 position, Vector3 rotation, byte timeContext) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; } = elementId;
    public Vector3 Position { get; set; } = position;
    public Vector3 Rotation { get; set; } = rotation;
    public byte TimeContext { get; set; } = timeContext;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.DETACH_ELEMENTS);
        builder.Write(this.ElementId);
        builder.Write(this.TimeContext);
        builder.Write(this.Position);
        builder.Write(this.Rotation);

        return builder.Build();
    }
}
