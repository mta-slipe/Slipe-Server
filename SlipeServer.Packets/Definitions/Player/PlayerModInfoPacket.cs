using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Player
{
    public class PlayerModInfoPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_MODINFO;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public string InfoType { get; set; }
        public uint Count { get; set; }
        public List<ModInfoItem> ModInfoItems { get; set; }
        public PlayerModInfoPacket(string infoType, uint count, List<ModInfoItem> modInfoItems)
        {
            this.InfoType = infoType;
            this.Count = count;
            this.ModInfoItems = modInfoItems;
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
            this.InfoType = reader.GetString();
            this.Count = reader.GetUint32();

            this.ModInfoItems = new List<ModInfoItem>((int)Count);
            for (int i = 0; i < Count;i++)
            {
                ModInfoItem infoItem = new ModInfoItem();
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
                this.ModInfoItems.Add(infoItem);
            }
        }
    }
}
