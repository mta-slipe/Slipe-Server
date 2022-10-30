using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class RemoveVehicleUpgradeRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public byte UpgradeId { get; set; }

    public RemoveVehicleUpgradeRpcPacket(uint elementId, ushort upgradeId)
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
        builder.Write((byte)ElementRpcFunction.ADD_VEHICLE_UPGRADE);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.UpgradeId);
        return builder.Build();
    }
}
