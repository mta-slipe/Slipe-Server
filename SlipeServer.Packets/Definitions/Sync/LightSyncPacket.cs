using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Definitions.Sync
{

    public class LightSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LIGHTSYNC;
        public override PacketReliability Reliability => PacketReliability.Unreliable;
        public override PacketPriority Priority => PacketPriority.Low;

        public uint ElementId { get; set; }
        public byte TimeContext { get; set; }
        public ushort Latency { get; set; }
        public float? Health { get; set; }
        public float? Armor { get; set; }
        public Vector3? Position{ get; set; }
        public float? VehicleHealth { get; set; }

        public LightSyncPacket()
        {

        }

        public override void Read(byte[] bytes)
        {

        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.ElementId);
            builder.Write(this.TimeContext);
            builder.WriteCompressed(this.Latency);

            builder.Write(this.Health != null && this.Armor != null);

            if (this.Health != null && this.Armor != null)
            {
                builder.WritePlayerHealth(this.Health.Value);
                builder.WritePlayerArmor(this.Armor.Value);
            }

            builder.Write(this.Position != null);
            if (this.Position != null)
            {
                builder.WriteLowPrecisionVector3(this.Position.Value);

                builder.Write(this.VehicleHealth != null);
                if (this.VehicleHealth != null)
                    builder.WriteLowPrecisionVehicleHealth(this.VehicleHealth.Value);
            }

            return builder.Build();
        }
    }
}
