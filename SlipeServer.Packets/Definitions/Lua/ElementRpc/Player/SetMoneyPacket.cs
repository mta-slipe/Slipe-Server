using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;

public class SetMoneyPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public int Money { get; set; }
    public bool Instant { get; set; }

    public SetMoneyPacket(int money, bool instant)
    {
        this.Money = money;
        this.Instant = instant;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_PLAYER_MONEY);
        builder.Write(this.Money);
        builder.Write(this.Instant);
        return builder.Build();
    }
}
