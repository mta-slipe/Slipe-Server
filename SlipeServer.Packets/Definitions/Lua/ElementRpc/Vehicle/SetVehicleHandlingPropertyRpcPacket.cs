using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public sealed class SetVehicleHandlingPropertyRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte Property { get; set; }

    public float? FloatValue { get; set; }
    public Vector3? VectorValue { get; set; }
    public uint? UIntValue { get; set; }
    public string? StringValue { get; set; }
    public byte? ByteValue { get; set; }
    public bool? BoolValue { get; set; }


    public SetVehicleHandlingPropertyRpcPacket(ElementId elementId, byte property, float value)
    {
        this.ElementId = elementId;
        this.Property = property;
        this.FloatValue = value;
    }

    public SetVehicleHandlingPropertyRpcPacket(ElementId elementId, byte property, Vector3 value)
    {
        this.ElementId = elementId;
        this.Property = property;
        this.VectorValue = value;
    }


    public SetVehicleHandlingPropertyRpcPacket(ElementId elementId, byte property, uint value)
    {
        this.ElementId = elementId;
        this.Property = property;
        this.UIntValue = value;
    }


    public SetVehicleHandlingPropertyRpcPacket(ElementId elementId, byte property, string value)
    {
        this.ElementId = elementId;
        this.Property = property;
        this.StringValue = value;
    }


    public SetVehicleHandlingPropertyRpcPacket(ElementId elementId, byte property, byte value)
    {
        this.ElementId = elementId;
        this.Property = property;
        this.ByteValue = value;
    }


    public SetVehicleHandlingPropertyRpcPacket(ElementId elementId, byte property, bool value)
    {
        this.ElementId = elementId;
        this.Property = property;
        this.BoolValue = value;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_HANDLING_PROPERTY);
        builder.Write(this.ElementId);
        builder.Write(this.Property);

        if (this.FloatValue.HasValue)
            builder.Write(this.FloatValue.Value);
        if (this.VectorValue.HasValue)
            builder.Write(this.VectorValue.Value);
        if (this.UIntValue.HasValue)
            builder.Write(this.UIntValue.Value);
        if (this.StringValue != null)
            builder.Write(this.StringValue);
        if (this.ByteValue.HasValue)
            builder.Write(this.ByteValue.Value);
        if (this.BoolValue.HasValue)
            builder.Write(this.BoolValue.Value ? 1 : 0);

        return builder.Build();
    }
}
