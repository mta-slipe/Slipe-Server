using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public sealed class VehicleSpawnPacket(IEnumerable<VehicleSpawnInfo> vehiclesToRespawn) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_VEHICLE_SPAWN;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public IEnumerable<VehicleSpawnInfo> VehiclesToSpawn { get; set; } = vehiclesToRespawn;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        foreach (var vehicleToSpawn in this.VehiclesToSpawn)
        {
            builder.Write(vehicleToSpawn.ElementId);
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
