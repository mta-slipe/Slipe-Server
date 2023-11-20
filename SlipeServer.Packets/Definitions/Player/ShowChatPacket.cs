using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Player;

public class ShowChatPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public bool Show { get; }
    public bool InputBlocked { get; }

    public ShowChatPacket(bool show, bool inputBlocked)
    {
        this.Show = show;
        this.InputBlocked = inputBlocked;
    }

    public override void Read(byte[] bytes)
    {
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SHOW_CHAT);
        builder.Write((byte)(Show ? 1 : 0));
        builder.Write((byte)(InputBlocked ? 1 : 0));

        return builder.Build();
    }
}
