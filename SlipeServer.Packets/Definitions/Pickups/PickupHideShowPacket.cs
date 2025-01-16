using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Collections.Generic;

namespace SlipeServer.Packets.Definitions.Pickups;

public struct PickupIdAndModel
{
    public ElementId Id { get; set; }
    public ushort Model { get; set; }

    public PickupIdAndModel(ElementId id, ushort model)
    {
        this.Id = id;
        this.Model = model;
    }
}

public sealed class PickupHideShowPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PICKUP_HIDESHOW;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public bool IsShowing { get; }
    public IEnumerable<PickupIdAndModel> Pickups { get; } = Array.Empty<PickupIdAndModel>();

    public PickupHideShowPacket(bool isShowing, IEnumerable<PickupIdAndModel> pickups)
    {
        this.IsShowing = isShowing;
        this.Pickups = pickups;
    }

    public PickupHideShowPacket()
    {

    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.IsShowing);
        foreach (var pickup in this.Pickups)
        {
            builder.Write(pickup.Id);
            builder.WriteCompressed(pickup.Model);
        }

        return builder.Build();
    }
}
