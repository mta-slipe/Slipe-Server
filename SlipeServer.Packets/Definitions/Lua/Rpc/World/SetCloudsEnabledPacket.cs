using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public class SetCloudsEnabledPacket : Packet

{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public bool CloudsEnabled { get; set; }

    public SetCloudsEnabledPacket(bool cloudsEnabled)
    {
        this.CloudsEnabled = cloudsEnabled;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_CLOUDS_ENABLED);
        builder.Write(this.CloudsEnabled);

        return builder.Build();
    }
}
