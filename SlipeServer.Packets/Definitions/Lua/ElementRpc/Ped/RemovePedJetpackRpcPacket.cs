using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class RemovePedJetpackRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; }

    public RemovePedJetpackRpcPacket(uint elementId)
    {
        this.ElementId = elementId;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.REMOVE_PED_JETPACK);
        builder.WriteElementId(this.ElementId);

        return builder.Build();
    }
}
