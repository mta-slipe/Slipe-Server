using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;

namespace SlipeServer.Packets.Rpc;

public class RpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public RpcFunctions FunctionId { get; set; }

    public PacketReader Reader { get; private set; }

    public RpcPacket()
    {
        this.Reader = new PacketReader(Array.Empty<byte>());
    }

    public override void Read(byte[] bytes)
    {
        this.Reader = new PacketReader(bytes);

        this.FunctionId = (RpcFunctions)this.Reader.GetByte();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)this.FunctionId);

        return builder.Build();
    }
}
