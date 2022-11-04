using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class SetPedFightingStyleRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; }
    public byte FightingStyle { get; }

    public SetPedFightingStyleRpcPacket(uint elementId, byte fightingStyle)
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
        builder.WriteElementId(this.ElementId);
        builder.Write(this.FightingStyle);

        return builder.Build();
    }
}
