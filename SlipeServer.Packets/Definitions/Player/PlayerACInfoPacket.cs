using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Player;

public class PlayerACInfoPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_ACINFO;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public List<byte> DetectedACList { get; set; } = new();
    public uint D3d9Size { get; set; }
    public string D3d9MD5 { get; set; } = string.Empty;
    public string D3d9SHA256 { get; set; } = string.Empty;
    public PlayerACInfoPacket()
    {
    }

    public override byte[] Write()
    {
        throw new NotSupportedException();
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        this.DetectedACList = new List<byte>();
        byte numItems = reader.GetByte();
        for (byte i = 0; i < numItems; i++)
        {
            this.DetectedACList.Add(reader.GetByte());
        }
        this.D3d9Size = reader.GetUint32();
        this.D3d9MD5 = reader.GetString();
        this.D3d9SHA256 = reader.GetString();
    }
}
