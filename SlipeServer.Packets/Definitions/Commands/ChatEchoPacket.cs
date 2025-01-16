﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Drawing;

namespace SlipeServer.Packets.Definitions.Commands;

public sealed class ChatEchoPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_CHAT_ECHO;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.Low;

    public ElementId SourceId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Color Color { get; set; }
    public byte MessageType { get; set; }
    public bool IsColorCoded { get; set; }

    public ChatEchoPacket()
    {

    }

    public ChatEchoPacket(ElementId sourceId, string message, Color color, ChatEchoType messageType, bool isColorCoded = false)
    {
        this.SourceId = sourceId;
        this.Message = message;
        this.Color = color;
        this.MessageType = (byte)messageType;
        this.IsColorCoded = isColorCoded;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.Color);
        builder.Write(this.IsColorCoded);
        builder.Write(this.SourceId);
        builder.Write(this.MessageType);
        builder.WriteStringWithoutLength(this.Message);

        return builder.Build();
    }
}
