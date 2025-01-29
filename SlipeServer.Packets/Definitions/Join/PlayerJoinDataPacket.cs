﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;

namespace SlipeServer.Packets.Definitions.Join;

public sealed class PlayerJoinDataPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_JOINDATA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ushort NetVersion { get; private set; }
    public ushort MtaVersion { get; private set; }
    public ushort BitStreamVersion { get; private set; }
    public string PlayerVersion { get; private set; } = string.Empty; // CMtaVersion type??
    public bool OptionalUpdateInfoRequired { get; private set; }
    public byte GameVersion { get; private set; } // unssigned
    public string Nickname { get; private set; } = string.Empty;
    public byte[] Password { get; private set; } = Array.Empty<byte>();
    public string Serial { get; private set; } = string.Empty;
    public string DiscordSecret { get; private set; } = string.Empty;

    public PlayerJoinDataPacket()
    {

    }

    public override void Read(byte[] bytes)
    {
        PacketReader reader = new PacketReader(bytes);
        this.NetVersion = reader.GetUint16();
        this.MtaVersion = reader.GetUint16();
        this.BitStreamVersion = reader.GetUint16();
        this.PlayerVersion = reader.GetString();
        this.OptionalUpdateInfoRequired = reader.GetBit();
        this.GameVersion = reader.GetByte();
        this.Nickname = reader.GetStringCharacters(PacketConstants.MaxPlayerNickLength).TrimEnd('\0');
        this.Password = reader.GetBytes(16);
        this.Serial = reader.GetStringCharacters(PacketConstants.MaxSerialLength);
        //this.DiscordSecret = reader.GetString();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();

        builder.Write(this.NetVersion);
        builder.Write(this.MtaVersion);
        builder.Write(this.BitStreamVersion);
        builder.Write(this.PlayerVersion);
        builder.Write(this.OptionalUpdateInfoRequired);
        builder.Write(this.GameVersion);
        builder.WriteStringWithoutLength((this.Nickname).PadRight(PacketConstants.MaxPlayerNickLength));
        builder.Write(this.Password);
        builder.WriteStringWithoutLength((this.Serial).PadRight(PacketConstants.MaxSerialLength));

        return builder.Build();
    }
}
