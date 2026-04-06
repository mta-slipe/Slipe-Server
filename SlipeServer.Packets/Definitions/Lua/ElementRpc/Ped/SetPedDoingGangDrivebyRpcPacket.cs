using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public sealed class SetPedDoingGangDrivebyRpcPacket(ElementId elementId, bool isDriveby, byte timeContext) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; } = elementId;
    public bool IsDriveby { get; } = isDriveby;
    public byte TimeContext { get; } = timeContext;

    public override void Read(byte[] bytes) { }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_PED_DOING_GANG_DRIVEBY);
        builder.Write(this.ElementId);
        builder.Write(this.IsDriveby);
        builder.Write(this.TimeContext);

        return builder.Build();
    }
}
