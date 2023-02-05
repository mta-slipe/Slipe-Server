using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class WarpIntoVehicleRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public ElementId VehicleId { get; }
    public byte Seat { get; }
    public byte TimeContext { get; }

    public WarpIntoVehicleRpcPacket(ElementId elementId, ElementId vehicleId, byte seat, byte timeContext)
    {
        this.ElementId = elementId;
        this.VehicleId = vehicleId;
        this.Seat = seat;
        this.TimeContext = timeContext;
    }

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
