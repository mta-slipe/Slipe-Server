using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class RemoveFromVehiclePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public byte TimeContext { get; }

    public RemoveFromVehiclePacket(ElementId elementId, byte timeContext)
    {
        this.ElementId = elementId;
        this.TimeContext = timeContext;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.REMOVE_PED_FROM_VEHICLE);
        builder.Write(this.ElementId);
        builder.Write(this.TimeContext);

        return builder.Build();
    }
}
