using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class SetPedGravityRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; }
    public float Gravity { get; }

    public SetPedGravityRpcPacket(uint elementId, float gravity)
    {
        this.ElementId = elementId;
        this.Gravity = gravity;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_PED_GRAVITY);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.Gravity);

        return builder.Build();
    }
}
