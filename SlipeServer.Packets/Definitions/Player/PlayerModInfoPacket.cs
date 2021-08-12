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
        public PlayerModInfoPacket()
        {

        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
            InfoType = reader.GetString();
            Count = reader.GetUint32();

            ModInfoItems = new List<ModInfoItem>((int)Count);
            for (int i = 0; i < Count;i++)
            {
                ModInfoItem infoItem = new ModInfoItem();
                infoItem.Id = reader.GetUint16();
                infoItem.Hash = reader.GetUint32();
                infoItem.Name = reader.GetString();
                infoItem.HasSize = reader.GetInt32() != 0;

                Vector3 size = new Vector3();
                size.X = reader.GetFloat();
                size.Y = reader.GetFloat();
                size.Z = reader.GetFloat();
                Vector3 originalSize = new Vector3();
                originalSize.X = reader.GetFloat();
                originalSize.Y = reader.GetFloat();
                originalSize.Z = reader.GetFloat();
                infoItem.HasHashInfo = reader.GetInt32() != 0;
                infoItem.ShortBytes = reader.GetUint32();
                infoItem.ShortMd5 = reader.GetString();
                infoItem.ShortSha256 = reader.GetString();
                infoItem.LongBytes = reader.GetUint32();
                infoItem.LongMd5 = reader.GetString();
                infoItem.LongSha256 = reader.GetString();
                ModInfoItems.Add(infoItem);
            }
            int x = 5;
        }
    }
}
