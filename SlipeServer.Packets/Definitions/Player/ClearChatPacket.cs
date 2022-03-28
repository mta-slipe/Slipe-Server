using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Player;

public class ClearChatPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_CHAT_CLEAR;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ClearChatPacket()
    {

    }

    public override void Read(byte[] bytes)
    {
    }

    public override byte[] Write() => Array.Empty<byte>();
}
