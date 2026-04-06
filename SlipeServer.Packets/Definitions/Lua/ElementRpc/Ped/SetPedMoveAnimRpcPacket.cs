using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public sealed class SetPedMoveAnimRpcPacket(ElementId elementId, uint moveAnim) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; } = elementId;
    public uint MoveAnim { get; } = moveAnim;

    public override void Read(byte[] bytes) { }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_PED_MOVE_ANIM);
        builder.Write(this.ElementId);
        builder.WriteCompressed(this.MoveAnim);

        return builder.Build();
    }
}
