using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Pickups;

public sealed class SetPickupTypeRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public ushort? Model { get; }
    public byte? WeaponId { get; set; }
    public ushort? Ammo { get; set; }
    public float? Armor { get; set; }
    public float? Health { get; set; }

    public SetPickupTypeRpcPacket(ElementId elementId, ushort model)
    {
        this.ElementId = elementId;
        this.Model = model;
    }

    public SetPickupTypeRpcPacket(ElementId elementId, byte weaponId, ushort ammo)
    {
        this.ElementId = elementId;
        this.WeaponId = weaponId;
        this.Ammo = ammo;
    }

    private SetPickupTypeRpcPacket(ElementId elementId)
    {
        this.ElementId = elementId;
    }

    public static SetPickupTypeRpcPacket CreateHealth(ElementId elementId, float health)
    {
        return new(elementId)
        {
            Health = health
        };
    }

    public static SetPickupTypeRpcPacket CreateArmor(ElementId elementId, float armor)
    {
        return new(elementId)
        {
            Armor = armor
        };
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_PICKUP_TYPE);
        builder.Write(this.ElementId);

        if (this.Health != null)
        {
            builder.Write((byte)0);
            builder.Write(this.Health.Value);
        } else if (this.Armor != null)
        {
            builder.Write((byte)1);
            builder.Write(this.Armor.Value);
        } else if (this.Ammo != null && this.WeaponId != null)
        {
            builder.Write((byte)2);
            builder.Write(this.WeaponId.Value);
            builder.Write(this.Ammo.Value);
        } else if (this.Model != null)
        {
            builder.Write((byte)3);
            builder.Write(this.Model.Value);
        }

        return builder.Build();
    }
}
