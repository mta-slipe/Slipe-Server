using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle.Sirens;

public class GiveVehicleSirensPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public bool IsOverride { get; }
    public byte SirenType { get; }
    public byte SirenCount { get; }
    public bool Is360Degrees { get; }
    public bool DoLineOfSightCheck { get; }
    public bool UseRandomiser { get; }
    public bool EnableSilent { get; }

    public GiveVehicleSirensPacket(
        uint elementId,
        bool isOverride,
        VehicleSirenType sirenType,
        byte sirenCount,
        bool is360Degrees,
        bool doLineOfSightCheck,
        bool useRandomiser,
        bool enableSilent
    )
    {
        this.ElementId = elementId;
        this.IsOverride = isOverride;
        this.SirenType = (byte)sirenType;
        this.SirenCount = sirenCount;
        this.Is360Degrees = is360Degrees;
        this.DoLineOfSightCheck = doLineOfSightCheck;
        this.UseRandomiser = useRandomiser;
        this.EnableSilent = enableSilent;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.GIVE_VEHICLE_SIRENS);
        builder.WriteElementId(this.ElementId);

        builder.Write(this.IsOverride);
        if (this.IsOverride)
        {
            builder.Write(this.SirenType);
            builder.Write(this.SirenCount);
            builder.Write(this.Is360Degrees);
            builder.Write(this.DoLineOfSightCheck);
            builder.Write(this.UseRandomiser);
            builder.Write(this.EnableSilent);
        }

        return builder.Build();
    }
}
