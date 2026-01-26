using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle.Sirens;

public sealed class SetVehicleSirensPacket(
    ElementId elementId,
    bool isOverride,
    byte sirenId,
    Vector3 position,
    Color color,
    uint minAlpha,
    bool is360Degrees,
    bool doLineOfSightCheck,
    bool useRandomiser,
    bool enableSilent
    ) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; } = elementId;
    public bool IsOverride { get; } = isOverride;
    public byte SirenId { get; } = sirenId;
    public Vector3 Position { get; } = position;
    public Color Color { get; } = color;
    public uint MinAlpha { get; } = minAlpha;
    public bool Is360Degrees { get; } = is360Degrees;
    public bool DoLineOfSightCheck { get; } = doLineOfSightCheck;
    public bool UseRandomiser { get; } = useRandomiser;
    public bool EnableSilent { get; } = enableSilent;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_SIRENS);
        builder.Write(this.ElementId);

        builder.Write(this.IsOverride);
        if (this.IsOverride)
        {
            builder.Write(this.SirenId);
            builder.Write(this.Position);
            builder.Write(this.Color, true, true);
            builder.Write(this.MinAlpha);
            builder.Write(this.Is360Degrees);
            builder.Write(this.DoLineOfSightCheck);
            builder.Write(this.UseRandomiser);
            builder.Write(this.EnableSilent);
        }

        return builder.Build();
    }
}
