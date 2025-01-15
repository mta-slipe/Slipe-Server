using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public sealed class SetPedAnimationProgressRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public string Animation { get; }
    public float Progress { get; }

    public SetPedAnimationProgressRpcPacket(ElementId elementId, string animation, float progress)
    {
        this.ElementId = elementId;
        this.Animation = animation;
        this.Progress = progress;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_PED_ANIMATION_PROGRESS);
        builder.Write(this.ElementId);
        builder.WriteStringWithByteAsLength(this.Animation);
        builder.Write(this.Progress);

        return builder.Build();
    }
}
