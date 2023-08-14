using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class VehicleBlownRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte TimeContext { get; set; }
    public bool CreateExplosion { get; set; }

    public VehicleBlownRpcPacket(ElementId elementId, byte timeContext, bool createExplosion)
    {
        this.ElementId = elementId;
        this.TimeContext = timeContext;
        this.CreateExplosion = createExplosion;
    }

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
