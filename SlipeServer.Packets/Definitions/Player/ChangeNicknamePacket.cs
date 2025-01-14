﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Player;

public class ChangeNicknamePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_CHANGE_NICK;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId PlayerId { get; set; }
    public string Name { get; set; }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);
        this.PlayerId = reader.GetElementId();
        this.Name = reader.GetStringCharacters(reader.RemainingBytes);
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.PlayerId);
        builder.WriteStringWithoutLength(this.Name);

        return builder.Build();
    }
}
