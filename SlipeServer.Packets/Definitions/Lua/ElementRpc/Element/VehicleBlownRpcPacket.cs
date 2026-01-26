using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class VehicleBlownRpcPacket(ElementId elementId, byte timeContext, bool createExplosion) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; } = elementId;
    public byte TimeContext { get; set; } = timeContext;
    public bool CreateExplosion { get; set; } = createExplosion;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.BLOW_VEHICLE);
        builder.Write(this.ElementId);

        builder.Write(this.TimeContext);
        builder.Write(this.CreateExplosion);

        return builder.Build();
    }
}
