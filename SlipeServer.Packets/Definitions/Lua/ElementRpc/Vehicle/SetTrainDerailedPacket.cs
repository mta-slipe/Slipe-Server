using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetTrainDerailedPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public bool IsDerailed { get; set; }

    public SetTrainDerailedPacket(uint elementId, bool isDerailed)
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
        builder.WriteElementId(this.ElementId);
        builder.Write(this.IsDerailed);
        return builder.Build();
    }
}
