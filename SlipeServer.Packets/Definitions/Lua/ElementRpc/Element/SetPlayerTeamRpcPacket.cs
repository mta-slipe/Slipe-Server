using System;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class SetPlayerTeamRpcPacket : Packet
{
    public override PacketId PacketId { get; } = PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability { get; } = PacketReliability.ReliableSequenced;
    public override PacketPriority Priority { get; } = PacketPriority.High;

    public ElementId SourceElementId { get; set; }
    public ElementId TeamId { get; set; }


    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRPCFunction.SET_PLAYER_TEAM);
        builder.Write(this.SourceElementId);
        builder.Write(this.TeamId);

        return builder.Build();
    }

    public override void Read(byte[] bytes) => throw new NotImplementedException();
}
