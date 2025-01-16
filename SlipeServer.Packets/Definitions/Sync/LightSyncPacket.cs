using SlipeServer.Packets.Enums;
using System.Numerics;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Sync;

public sealed class LightSyncPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LIGHTSYNC;
    public override PacketReliability Reliability => PacketReliability.Unreliable;
    public override PacketPriority Priority => PacketPriority.Low;

    private readonly PacketBuilder builder;


    public LightSyncPacket()
    {
        this.builder = new();
    }

    public LightSyncPacket(
        ElementId elementId,
        byte timeContext,
        ushort latency,
        float? health,
        float? armor,
        Vector3? position,
        float? vehicleHealth
    ) : this()
    {
        this.AddPlayer(elementId, timeContext, latency, health, armor, position, vehicleHealth);
    }

    public void AddPlayer(
        ElementId elementId,
        byte timeContext,
        ushort latency,
        float? health,
        float? armor,
        Vector3? position,
        float? vehicleHealth
    )
    {

        this.builder.Write(elementId);
        this.builder.Write(timeContext);
        this.builder.WriteCompressed(latency);

        this.builder.Write(health != null && armor != null);

        if (health != null && armor != null)
        {
            this.builder.WritePlayerHealth(health.Value);
            this.builder.WritePlayerArmor(armor.Value);
        }

        this.builder.Write(position != null);
        if (position != null)
        {
            this.builder.WriteLowPrecisionVector3(position.Value);

            this.builder.Write(vehicleHealth != null);
            if (vehicleHealth != null)
                this.builder.WriteLowPrecisionVehicleHealth(vehicleHealth.Value);
        }
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        return this.builder.Build();
    }
}
