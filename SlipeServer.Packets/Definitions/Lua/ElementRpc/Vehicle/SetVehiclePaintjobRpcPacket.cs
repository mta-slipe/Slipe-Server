using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public sealed class SetVehiclePaintjobRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte Paintjob { get; set; }

    public SetVehiclePaintjobRpcPacket(ElementId elementId, byte paintjob)
    {
        this.ElementId = elementId;
        this.Paintjob = paintjob;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_PAINTJOB);
        builder.Write(this.ElementId);
        builder.Write(this.Paintjob);
        return builder.Build();
    }
}
