using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class SetPedFightingStyleRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public byte FightingStyle { get; }

    public SetPedFightingStyleRpcPacket(ElementId elementId, byte fightingStyle)
    {
        this.ElementId = elementId;
        this.FightingStyle = fightingStyle;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_PED_FIGHTING_STYLE);
        builder.Write(this.ElementId);
        builder.Write(this.FightingStyle);

        return builder.Build();
    }
}
