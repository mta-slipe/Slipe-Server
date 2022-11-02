using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle.Sirens;

public class SetVehicleSirensOnPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public bool IsOn { get; }

    public SetVehicleSirensOnPacket(uint elementId, bool isOn)
    {
        this.ElementId = elementId;
        this.IsOn = isOn;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_SIRENE_ON);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.IsOn ? (byte)1 : (byte)0);
        return builder.Build();
    }
}
