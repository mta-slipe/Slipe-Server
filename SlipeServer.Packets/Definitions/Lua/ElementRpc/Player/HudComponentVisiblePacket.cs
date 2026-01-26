using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;

public sealed class HudComponentVisiblePacket(byte hudComponent, bool show) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte HudComponent { get; set; } = hudComponent;
    public bool Show { get; set; } = show;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SHOW_PLAYER_HUD_COMPONENT);
        builder.Write((byte)this.HudComponent);
        builder.Write((byte)(this.Show ? 1 : 0));
        return builder.Build();
    }
}
