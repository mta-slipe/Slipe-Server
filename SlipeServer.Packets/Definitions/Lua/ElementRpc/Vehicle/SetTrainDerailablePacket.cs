using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetTrainDerailablePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public bool isDerailable { get; set; }

    public SetTrainDerailablePacket(ElementId elementId, bool isDerailable)
    {
        this.ElementId = elementId;
        this.isDerailable = isDerailable;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_TRAIN_DERAILABLE);
        builder.Write(this.ElementId);
        builder.Write(this.isDerailable);
        return builder.Build();
    }
}
