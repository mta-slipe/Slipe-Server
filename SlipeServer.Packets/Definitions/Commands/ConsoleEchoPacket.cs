﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Commands;

public sealed class ConsoleEchoPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_CONSOLE_ECHO;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.Low;

    public string Message { get; set; } = string.Empty;

    public ConsoleEchoPacket()
    {

    }

    public ConsoleEchoPacket(string message)
    {
        this.Message = message;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.WriteStringWithoutLength(this.Message);

        return builder.Build();
    }
}
