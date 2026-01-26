using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public sealed class WarpIntoVehicleRpcPacket(ElementId elementId, ElementId vehicleId, byte seat, byte timeContext) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; } = elementId;
    public ElementId VehicleId { get; } = vehicleId;
    public byte Seat { get; } = seat;
    public byte TimeContext { get; } = timeContext;

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.WARP_PED_INTO_VEHICLE);
        builder.Write(this.ElementId);
        builder.Write(this.VehicleId);
        builder.Write(this.Seat);
        builder.Write(this.TimeContext);

        return builder.Build();
    }
}
