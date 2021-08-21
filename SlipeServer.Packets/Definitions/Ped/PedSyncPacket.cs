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

                    if ((data.Flags & (int)PedSyncFlags.Position) != 0)
                    {
                        builder.Write(data.Position.X);
                        builder.Write(data.Position.Y);
                        builder.Write(data.Position.Z);
                    }

                    if ((data.Flags & (int)PedSyncFlags.Rotation) != 0)
                        builder.Write(data.Rotation);
                    

                    if ((data.Flags & (int)PedSyncFlags.Velocity) != 0)
                    {
                        builder.Write(data.Velocity.X);
                        builder.Write(data.Velocity.Y);
                        builder.Write(data.Velocity.Z);
                    }

                    if ((data.Flags & (int)PedSyncFlags.Health) != 0)
                        builder.Write(data.Health);
                    if ((data.Flags & (int)PedSyncFlags.Armor) != 0)
                        builder.Write(data.Armor);
                    if ((data.Flags & (int)PedSyncFlags.IsOnFire) != 0)
                        builder.Write(data.IsOnFire);
                    if ((data.Flags & (int)PedSyncFlags.IsInWater) != 0)
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

                if ((flags & (int)PedSyncFlags.Position) != 0)
                {
                    Vector3 position = reader.GetVector3WithZAsFloat();
                    data.Position = position;
                }

                if ((flags & (int)PedSyncFlags.Rotation) != 0)
                {
                    data.Rotation = reader.GetFloat();
                }
                
                if ((flags & (int)PedSyncFlags.Velocity) != 0)
                {
                    data.Velocity = reader.GetVector3WithZAsFloat();
                }

                if ((flags & (int)PedSyncFlags.Health) != 0)
                {
                    data.Health = reader.GetFloat();
                }

                if ((flags & (int)PedSyncFlags.Armor) != 0)
                {
                    data.Armor = reader.GetFloat();
                }

                if ((flags & (int)PedSyncFlags.IsOnFire) != 0)
                {
                    data.IsOnFire = reader.GetBit();
                }

                if ((flags & (int)PedSyncFlags.IsInWater) != 0)
                {
                    data.IsInWater = reader.GetBit();
                }

                this.Syncs.Add(data);
            }
        }
    }
}
