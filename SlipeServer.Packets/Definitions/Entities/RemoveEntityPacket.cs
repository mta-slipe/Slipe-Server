using System;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class RemoveEntityPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_ENTITY_REMOVE;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    private readonly PacketBuilder builder;

    public RemoveEntityPacket()
    {
        this.builder = new PacketBuilder();
    }

    public void AddEntity(uint elementId)
    {
        this.builder.WriteElementId(elementId);
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        this.builder.Write(this.builder.Build());

        return this.builder.Build();
    }
}
