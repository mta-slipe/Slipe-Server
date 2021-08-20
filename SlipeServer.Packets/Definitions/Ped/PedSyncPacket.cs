using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Ped
{
    public class PedSyncPacket : Packet
    {
        public override PacketId PacketId { get; } = PacketId.PACKET_ID_PED_SYNC;
        public override PacketReliability Reliability { get; } = PacketReliability.UnreliableSequenced;
        public override PacketPriority Priority { get; } = PacketPriority.Medium;

        

        public List<PedSyncData> Syncs;

        public PedSyncPacket()
        {
            this.Syncs = new List<PedSyncData>();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            foreach (var data in this.Syncs)
            {
                if (data.Send)
                {
                    builder.Write(data.SourceElementId);

                    builder.Write(data.TimeSyncContext);

                    builder.Write(data.Flags);

                    if ((data.Flags & 0x01) != 0)
                    {
                        builder.Write(data.Position.X);
                        builder.Write(data.Position.Y);
                        builder.Write(data.Position.Z);
                    }

                    if ((data.Flags & 0x02) != 0)
                        builder.Write(data.Rotation);
                    

                    if ((data.Flags & 0x04) != 0)
                    {
                        builder.Write(data.Velocity.X);
                        builder.Write(data.Velocity.Y);
                        builder.Write(data.Velocity.Z);
                    }

                    if ((data.Flags & 0x08) != 0)
                        builder.Write(data.Health);
                    if ((data.Flags & 0x10) != 0)
                        builder.Write(data.Armor);
                    if ((data.Flags & 0x20) != 0)
                        builder.Write(data.IsOnFire);
                    if ((data.Flags & 0x40) != 0)
                        builder.Write(data.IsInWater);
                }
            }

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
            while ((reader.Size / 8) > 32)
            {
                PedSyncData data = new PedSyncData();
                data.Send = false;

                data.SourceElementId = reader.GetElementId();

                data.TimeSyncContext = reader.GetByte();

                byte flags = 0;

                flags = reader.GetByte();

                data.Flags = flags;

                if ((flags & 0x01) != 0)
                {
                    Vector3 position = reader.GetVector3WithZAsFloat();
                    data.Position = position;
                }

                if ((flags & 0x02) != 0)
                {
                    data.Rotation = reader.GetFloat();
                }
                
                if ((flags & 0x04) != 0)
                {
                    data.Velocity = reader.GetVector3WithZAsFloat();
                }

                if ((flags & 0x08) != 0)
                {
                    data.Health = reader.GetFloat();
                }

                if ((flags & 0x10) != 0)
                {
                    data.Armor = reader.GetFloat();
                }

                if ((flags & 0x20) != 0)
                {
                    data.IsOnFire = reader.GetBit();
                }

                if ((flags & 0x40) != 0)
                {
                    data.IsInWater = reader.GetBit();
                }

                this.Syncs.Add(data);
            }
        }
    }
}
