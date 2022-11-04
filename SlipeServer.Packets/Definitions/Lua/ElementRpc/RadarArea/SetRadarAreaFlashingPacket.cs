using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class SetRadarAreaFlashingPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; }
    public bool Flashing { get; }

    public SetRadarAreaFlashingPacket(uint elementId, bool flashing)
    {
        this.ElementId = elementId;
        this.Flashing = flashing;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_RADAR_AREA_FLASHING);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.Flashing);

        return builder.Build();
    }
}
