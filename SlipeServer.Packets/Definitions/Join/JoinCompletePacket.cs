using SlipeServer.Packets;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace MTAServerWrapper.Packets.Outgoing.Connection;

public sealed class JoinCompletePacket(string welcomeMessage, string version) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_SERVER_JOIN_COMPLETE;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public string WelcomeMessage { get; } = welcomeMessage;
    public string Version { get; } = version;

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.WelcomeMessage.Substring(0, Math.Min(128, this.WelcomeMessage.Length)));
        builder.Write(this.Version);

        return builder.Build();
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }
}
