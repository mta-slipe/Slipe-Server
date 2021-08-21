using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Player
{
    public class VehicleSpawnPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_VEHICLE_SPAWN;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public IEnumerable<VehicleSpawnInfo> VehiclesToSpawn { get; set; }

        public VehicleSpawnPacket(IEnumerable<VehicleSpawnInfo> vehiclesToRespawn)
        {
            this.VehiclesToSpawn = vehiclesToRespawn;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            foreach (var vehicleToSpawn in this.VehiclesToSpawn)
            {
                builder.WriteElementId(vehicleToSpawn.ElementId);
                builder.Write(vehicleToSpawn.TimeContext);
                builder.Write(vehicleToSpawn.Position);
                builder.Write(vehicleToSpawn.Rotation);
                builder.WriteCapped(vehicleToSpawn.Colors.Length, 2);
                foreach (var color in vehicleToSpawn.Colors)
                    builder.Write(color);
            }
            return builder.Build();
        }
    }
}
