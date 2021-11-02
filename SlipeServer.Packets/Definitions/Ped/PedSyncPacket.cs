using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using System.Collections.Generic;

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

                    builder.Write((byte)data.Flags);

                    if ((data.Flags & PedSyncFlags.Position) != 0)
                    {
                        builder.Write(data.Position?.X ?? 0);
                        builder.Write(data.Position?.Y ?? 0);
                        builder.Write(data.Position?.Z ?? 0);
                    }

                    if ((data.Flags & PedSyncFlags.Rotation) != 0)
                        builder.Write(data.Rotation ?? 0);

                    if ((data.Flags & PedSyncFlags.Velocity) != 0)
                    {
                        builder.Write(data.Velocity?.X ?? 0);
                        builder.Write(data.Velocity?.Y ?? 0);
                        builder.Write(data.Velocity?.Z ?? 0);
                    }

                    if ((data.Flags & PedSyncFlags.Health) != 0)
                        builder.Write(data.Health ?? 0);
                    if ((data.Flags & PedSyncFlags.Armor) != 0)
                        builder.Write(data.Armor ?? 0);
                    if ((data.Flags & PedSyncFlags.IsOnFire) != 0)
                        builder.Write(data.IsOnFire ?? false);
                    if ((data.Flags & PedSyncFlags.IsInWater) != 0)
                        builder.Write(data.IsInWater ?? false);
                }
            }

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
            while (reader.Size - reader.Counter > 32)
            {
                PedSyncData data = new()
                {
                    Send = false
                };

                data.SourceElementId = reader.GetElementId();
                data.TimeSyncContext = reader.GetByte();
                data.Flags = (PedSyncFlags)reader.GetByte();

                if ((data.Flags & PedSyncFlags.Position) != 0)
                    data.Position = reader.GetVector3();

                if ((data.Flags & PedSyncFlags.Rotation) != 0)
                    data.Rotation = reader.GetFloat();

                if ((data.Flags & PedSyncFlags.Velocity) != 0)
                    data.Velocity = reader.GetVector3();

                if ((data.Flags & PedSyncFlags.Health) != 0)
                    data.Health = reader.GetFloat();

                if ((data.Flags & PedSyncFlags.Armor) != 0)
                    data.Armor = reader.GetFloat();

                if ((data.Flags & PedSyncFlags.IsOnFire) != 0)
                    data.IsOnFire = reader.GetBit();

                if ((data.Flags & PedSyncFlags.IsInWater) != 0)
                    data.IsInWater = reader.GetBit();

                this.Syncs.Add(data);
            }
        }
    }
}
