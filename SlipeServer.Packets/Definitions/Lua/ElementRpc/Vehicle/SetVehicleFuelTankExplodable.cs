using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetVehicleFuelTankExplodable : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public bool Enabled { get; set; }

    public SetVehicleFuelTankExplodable(uint elementId, bool enabled)
    {
        this.ElementId = elementId;
        this.Enabled = enabled;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_FUEL_TANK_EXPLODABLE);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.Enabled);
        return builder.Build();
    }
}
