using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetVehicleVariantPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte Variant1 { get; set; }
    public byte Variant2 { get; set; }

    public SetVehicleVariantPacket(ElementId elementId, byte variant1, byte variant2)
    {
        this.ElementId = elementId;
        this.Variant1 = variant1;
        this.Variant2 = variant2;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_VARIANT);
        builder.Write(this.ElementId);
        builder.Write(this.Variant1);
        builder.Write(this.Variant2);
        return builder.Build();
    }
}
