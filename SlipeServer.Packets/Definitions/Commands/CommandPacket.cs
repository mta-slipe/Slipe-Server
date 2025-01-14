using SlipeServer.Packets.Enums;
using System;
using System.Linq;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Definitions.Commands;

public class CommandPacket : Packet
{
    public const int MinCommandLength = 1;
    public const int MaxCommandLength = 255;

    public override PacketId PacketId => PacketId.PACKET_ID_COMMAND;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public string Command { get; private set; } = string.Empty;
    public string[] Arguments { get; private set; } = [];

    public CommandPacket()
    {

    }

    public override void Read(byte[] bytes)
    {
        if (bytes.Length < MinCommandLength || bytes.Length > MaxCommandLength * 4)
            return;

        var reader = new PacketReader(bytes);
        string[] commandArgs = reader.GetStringCharacters(bytes.Length).Split(' ');
        this.Command = commandArgs[0];
        this.Arguments = commandArgs.Skip(1).ToArray();
    }

    public override byte[] Write()
    {
        throw new NotSupportedException();
    }
}
