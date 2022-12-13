using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public class SetTrafficLightStatePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte State { get; set; }
    public bool Forced { get; set; }

    public SetTrafficLightStatePacket(byte state, bool forced)
    {
        this.State = state;
        this.Forced = forced;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_TRAFFIC_LIGHT_STATE);
        builder.WriteCapped(this.State, 4);
        builder.Write(this.Forced);

        return builder.Build();
    }
}
