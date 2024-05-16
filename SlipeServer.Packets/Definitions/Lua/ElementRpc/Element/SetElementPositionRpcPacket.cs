using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class SetElementPositionRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte TimeContext { get; set; }
    public Vector3 Position { get; set; }
    public bool IsWarp { get; set; }

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
        var reader = new PacketReader(bytes);

        var rpcFunction = (ElementRpcFunction)reader.GetByte();
        if (rpcFunction != ElementRpcFunction.SET_ELEMENT_POSITION)
            throw new InvalidOperationException($"Invalid rpcFunction, expected SET_ELEMENT_POSITION, got: {rpcFunction}");

        this.ElementId = reader.GetElementId();
        this.Position = reader.GetVector3();
        this.TimeContext = reader.GetByte();
        this.IsWarp = reader.IsFinishedReading ? true : false;
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
