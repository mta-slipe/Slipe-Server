using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetVehiclePlateTextRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public string PlateText { get; set; }

    public SetVehiclePlateTextRpcPacket(uint elementId, string plateText)
    {
        this.ElementId = elementId;
        this.PlateText = plateText;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_PLATE_TEXT);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.PlateText);
        return builder.Build();
    }
}
