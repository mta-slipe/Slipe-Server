using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class SetElementPositionRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte TimeContext { get; set; }
    public Vector3 Position { get; set; }
    public bool IsWarp { get; }

    public SetElementPositionRpcPacket()
    {

    }

    public SetElementPositionRpcPacket(ElementId elementId, byte timeContext, Vector3 position, bool isWarp = false)
    {
        this.ElementId = elementId;
        this.TimeContext = timeContext;
        this.Position = position;
        this.IsWarp = isWarp;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_POSITION);
        builder.Write(this.ElementId);

        builder.Write(this.Position);

        builder.Write(this.TimeContext);

        if (!this.IsWarp)
        {
            builder.Write((byte)0);
        }

        return builder.Build();
    }
}
