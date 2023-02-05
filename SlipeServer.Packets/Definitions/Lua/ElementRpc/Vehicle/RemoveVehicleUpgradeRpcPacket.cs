using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class RemoveVehicleUpgradeRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte UpgradeId { get; set; }

    public RemoveVehicleUpgradeRpcPacket(ElementId elementId, ushort upgradeId)
    {
        this.ElementId = elementId;
        this.UpgradeId = (byte)(upgradeId - 1000);
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.REMOVE_VEHICLE_UPGRADE);
        builder.Write(this.ElementId);
        builder.Write(this.UpgradeId);
        return builder.Build();
    }
}
