using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;

namespace SlipeServer.Packets.Definitions.Vehicles
{
    public class VehicleDamageSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_VEHICLE_DAMAGE_SYNC;

        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

        public override PacketPriority Priority => PacketPriority.High;

        public uint VehicleId { get; set; }
        public byte?[] DoorStates { get; set; } = Array.Empty<byte?>();
        public byte?[] WheelStates { get; set; } = Array.Empty<byte?>();
        public byte?[] PanelStates { get; set; } = Array.Empty<byte?>();
        public byte?[] LightStates { get; set; } = Array.Empty<byte?>();

        public VehicleDamageSyncPacket()
        {

        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.VehicleId = reader.GetElementId();

            this.DoorStates = ReadComponentStates(reader, 6, 3);
            this.WheelStates = ReadComponentStates(reader, 4, 2);
            this.PanelStates = ReadComponentStates(reader, 7, 2);
            this.LightStates = ReadComponentStates(reader, 4, 2);
        }

        private byte?[] ReadComponentStates(PacketReader reader, int count, int bits)
        {
            var result = new byte?[count];
            for (int i = 0; i < count; i++)
                if (reader.GetBit())
                    result[i] = reader.GetByteCapped(bits);
                else
                    result[i] = null;

            return result;
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.VehicleId);

            WriteComponentStates(builder, this.DoorStates, 3);
            WriteComponentStates(builder, this.WheelStates, 2);
            WriteComponentStates(builder, this.PanelStates, 2);
            WriteComponentStates(builder, this.LightStates, 2);

            return builder.Build();
        }

        public void WriteComponentStates(PacketBuilder builder, byte?[] data, int bits)
        {
            foreach (var d in data)
            {
                builder.Write(d != null);
                if (d != null)
                    builder.WriteCapped(d.Value, bits);
            }
        }

        public override void Reset()
        {

        }
    }
}
