using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Definitions.Ped
{
    public class PedSyncPacket : Packet
    {
        public override PacketId PacketId { get; } = PacketId.PACKET_ID_PED_SYNC;
        public override PacketReliability Reliability { get; } = PacketReliability.ReliableSequenced;
        public override PacketPriority Priority { get; } = PacketPriority.Medium;

        public struct SyncData
        {
            public bool Send { get; set; }
            public uint Model { get; set; }
            public byte Flags { get; set; }
            public byte TimeSyncContext { get; set; }
            public Vector3 Position { get; set; }
            public float Rotation { get; set; }
            public Vector3 Velocity { get; set; }
            public float Health { get; set; }
            public float Armor { get; set; }
            public bool IsOnFire { get; set; }
            public bool IsInWater { get; set; }
        }

        private List<SyncData> Syncs;

        public PedSyncPacket()
        {
            this.Syncs = new List<SyncData>();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            foreach (var data in this.Syncs)
            {
                if (data.Send)
                {
                    // Vehicle ID
                    builder.Write(data.Model);

                    // Sync time context
                    builder.Write(data.TimeSyncContext);

                    // Flags
                    builder.Write(data.Flags);

                    // Position
                    if ((data.Flags & 0x01) != 0)
                    {
                        builder.Write(data.Position.X);
                        builder.Write(data.Position.Y);
                        builder.Write(data.Position.Z);
                    }

                    // Rotation
                    if ((data.Flags & 0x02) != 0)
                        builder.Write(data.Rotation);
                    

                    // Velocity
                    if ((data.Flags & 0x04) != 0)
                    {
                        builder.Write(data.Velocity.X);
                        builder.Write(data.Velocity.Y);
                        builder.Write(data.Velocity.Z);
                    }

                    // Health, armor, on fire, is in water
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
                SyncData data = new SyncData();
                data.Send = false;

                data.Model = reader.GetByte();

                data.TimeSyncContext = reader.GetByte();

                byte flags = 0;

                flags = reader.GetByte();

                data.Flags = flags;

                // Position
                if ((flags & 0x01) != 0)
                {
                    Vector3 position = reader.GetVector3WithZAsFloat();
                    data.Position = position;
                }

                // Rotation
                if ((flags & 0x02) != 0)
                {
                    data.Rotation = reader.GetByte();
                }
                
                // Velocity
                if ((flags & 0x04) != 0)
                {
                    data.Velocity = reader.GetVector3WithZAsFloat();
                }
                // Health
                if ((flags & 0x08) != 0)
                {
                    data.Health = reader.GetByte();
                }

                // Armor
                if ((flags & 0x10) != 0)
                {
                    data.Armor = reader.GetByte();
                }

                // On fire
                if ((flags & 0x20) != 0)
                {
                    data.IsOnFire = reader.GetBit();
                }

                // In water
                if ((flags & 0x40) != 0)
                {
                    data.IsInWater = reader.GetBit();
                }

                this.Syncs.Add(data);
            }
        }
    }
}
