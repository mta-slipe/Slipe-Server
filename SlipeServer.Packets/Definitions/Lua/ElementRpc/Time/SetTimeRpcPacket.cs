using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Time;

public sealed class SetTimeRpcPacket(byte hour, byte minute) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte Hour { get; } = hour;
    public byte Minute { get; } = minute;

    public override void Read(byte[] bytes) => throw new System.NotImplementedException();

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_TIME);
        builder.Write(this.Hour);
        builder.Write(this.Minute);
        
        return builder.Build();
    }
}
