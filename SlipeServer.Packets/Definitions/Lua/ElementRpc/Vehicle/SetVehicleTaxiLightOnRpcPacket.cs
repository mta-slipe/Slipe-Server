using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetVehicleTaxiLightOnRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public bool State { get; set; }

    public SetVehicleTaxiLightOnRpcPacket(uint elementId, bool state)
    {
        this.ElementId = elementId;
        this.State = state;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_TAXI_LIGHT_ON);
        builder.WriteElementId(this.ElementId);
        builder.Write((byte)(this.State ? 1 : 0));
        return builder.Build();
    }
}
