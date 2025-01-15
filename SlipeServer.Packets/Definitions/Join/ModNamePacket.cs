﻿using SlipeServer.Packets;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace MTAServerWrapper.Packets.Outgoing.Connection;

public sealed class ModNamePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_MOD_NAME;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ushort NetVersion { get; }
    public string Name { get; }

    public ModNamePacket(ushort netVersion, string name)
    {
        this.NetVersion = netVersion;
        this.Name = name;
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.NetVersion);
        builder.Write(this.Name);

        return builder.Build();
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }
}
