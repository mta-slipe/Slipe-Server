﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class SetElementInteriorRpcPacket : Packet
{

    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte Interior { get; set; }

    public Vector3? Position { get; set; }

    public SetElementInteriorRpcPacket()
    {

    }

    public SetElementInteriorRpcPacket(ElementId elementId, byte interior, Vector3? position = null)
    {
        this.ElementId = elementId;
        this.Interior = interior;
        this.Position = position;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_INTERIOR);
        builder.Write(this.ElementId);

        builder.Write(this.Interior);
        builder.Write((byte)(this.Position.HasValue ? 1 : 0));
        if (this.Position.HasValue)
            builder.Write(this.Position.Value);

        return builder.Build();
    }
}
