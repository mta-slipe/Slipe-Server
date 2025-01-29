using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public sealed class SetVehicleLandingGearDownRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public bool Down { get; set; }

    public SetVehicleLandingGearDownRpcPacket(ElementId elementId, bool down)
    {
        this.ElementId = elementId;
        this.Down = down;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_LANDING_GEAR_DOWN);
        builder.Write(this.ElementId);
        builder.Write((byte)(this.Down ? 1 : 0));
        return builder.Build();
    }
}
