using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;

namespace SlipeServer.Packets.Definitions.Player;

public class PlayerModInfoPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_MODINFO;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public string InfoType { get; set; } = string.Empty;
    public uint Count { get; set; }
    public ModInfoItem[] ModInfoItems { get; set; } = Array.Empty<ModInfoItem>();
    public PlayerModInfoPacket()
    {
    }

    public override byte[] Write()
    {
        throw new NotSupportedException();
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);
        this.InfoType = reader.GetString();
        this.Count = reader.GetUint32();

        this.ModInfoItems = new ModInfoItem[(int)this.Count];
        for (int i = 0; i < this.Count; i++)
        {
            ModInfoItem infoItem = new();
            infoItem.Id = reader.GetUint16();
            infoItem.Hash = reader.GetUint32();
            infoItem.Name = reader.GetString();
            infoItem.HasSize = reader.GetInt32() != 0;

            infoItem.Size = reader.GetVector3();
            infoItem.OriginalSize = reader.GetVector3();
            infoItem.HasHashInfo = reader.GetInt32() != 0;
            infoItem.ShortBytes = reader.GetUint32();
            infoItem.ShortMd5 = reader.GetString();
            infoItem.ShortSha256 = reader.GetString();
            infoItem.LongBytes = reader.GetUint32();
            infoItem.LongMd5 = reader.GetString();
            infoItem.LongSha256 = reader.GetString();
            this.ModInfoItems[i] = infoItem;
        }
    }
}
