using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Marker;

public class SetMarkerTargetPositionRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public Vector3? Target { get; set; }
    public SetMarkerTargetPositionRpcPacket(uint elementId, Vector3? target = null)
    {
        this.ElementId = elementId;
        this.Target = target;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_MARKER_TARGET);
        builder.WriteElementId(this.ElementId);
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
