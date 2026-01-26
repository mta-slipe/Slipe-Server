using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Marker;

public sealed class SetMarkerTargetPositionRpcPacket(ElementId elementId, Vector3? target = null) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; } = elementId;
    public Vector3? Target { get; set; } = target;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_MARKER_TARGET);
        builder.Write(this.ElementId);
        if (this.Target.HasValue)
        {
            builder.Write((byte)1);
            builder.Write(this.Target.Value);
        } else
        {
            builder.Write((byte)0);

        }

        return builder.Build();
    }
}
