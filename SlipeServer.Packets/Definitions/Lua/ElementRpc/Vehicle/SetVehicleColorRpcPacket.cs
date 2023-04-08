using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Drawing;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetVehicleColorRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public Color?[] Colors { get; set; }

    public SetVehicleColorRpcPacket(ElementId elementId, Color?[] colors)
    {
        this.ElementId = elementId;
        this.Colors = colors;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_VEHICLE_COLOR);
        builder.Write(this.ElementId);
        var number = Math.Min((byte)Array.IndexOf(this.Colors, null), this.Colors.Length);
        builder.WriteCapped(number, 2);
        foreach (var color in this.Colors)
            builder.Write(color ?? Color.Black);

        return builder.Build();
    }
}
