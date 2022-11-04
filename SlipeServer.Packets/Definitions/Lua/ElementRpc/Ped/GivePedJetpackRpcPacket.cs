using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class GivePedJetpackRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; }

    public GivePedJetpackRpcPacket(uint elementId)
    {
        this.ElementId = elementId;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.GIVE_PED_JETPACK);
        builder.WriteElementId(this.ElementId);

        return builder.Build();
    }
}
