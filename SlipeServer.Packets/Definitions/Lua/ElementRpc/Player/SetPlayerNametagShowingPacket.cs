using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;

public sealed class SetPlayerNametagShowingPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public bool IsShowing { get; set; }

    public SetPlayerNametagShowingPacket(ElementId elementId, bool isShowing)
    {
        this.ElementId = elementId;
        this.IsShowing = isShowing;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_PLAYER_NAMETAG_SHOWING);
        builder.Write(this.ElementId);
        builder.Write(this.IsShowing ? (byte)1 : (byte)0);
        return builder.Build();
    }
}
