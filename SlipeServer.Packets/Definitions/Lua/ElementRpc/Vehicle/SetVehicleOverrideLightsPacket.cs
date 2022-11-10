using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetVehicleOverrideLightsPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public byte OverrideLights { get; set; }

    public SetVehicleOverrideLightsPacket(uint elementId, byte overrideLights)
    {
        this.ElementId = elementId;
        this.OverrideLights = overrideLights;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_OVERRIDE_LIGHTS);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.OverrideLights);
        return builder.Build();
    }
}
