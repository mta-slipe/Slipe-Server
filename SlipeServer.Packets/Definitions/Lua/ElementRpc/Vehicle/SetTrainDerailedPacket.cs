using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetTrainDerailedPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public bool IsDerailed { get; set; }

    public SetTrainDerailedPacket(ElementId elementId, bool isDerailed)
    {
        this.ElementId = elementId;
        this.IsDerailed = isDerailed;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_TRAIN_DERAILED);
        builder.Write(this.ElementId);
        builder.Write(this.IsDerailed);
        return builder.Build();
    }
}
