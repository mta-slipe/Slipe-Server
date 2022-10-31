using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetVehiclePaintjobRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public byte Paintjob { get; set; }

    public SetVehiclePaintjobRpcPacket(uint elementId, byte paintjob)
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
        builder.WriteElementId(this.ElementId);
        builder.Write(this.Paintjob);
        return builder.Build();
    }
}
