using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class SetAircraftMaxVelocityPacket(float maxVelocity) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public float MaxVelocity { get; set; } = maxVelocity;

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_AIRCRAFT_MAXVELOCITY);
        builder.Write(this.MaxVelocity);

        return builder.Build();
    }
}
