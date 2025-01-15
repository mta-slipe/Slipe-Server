﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.CollisionShape;

public sealed class SetCollisionPolygonHeightRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public Vector2 Height { get; set; }

    public SetCollisionPolygonHeightRpcPacket(ElementId elementId, Vector2 height)
    {
        this.ElementId = elementId;
        this.Height = height;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_COLPOLYGON_HEIGHT);
        builder.Write(this.ElementId);

        builder.Write(this.Height);

        return builder.Build();
    }
}
