﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public sealed class GiveWeaponRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public byte WeaponType { get; }
    public ushort Ammo { get; }
    public bool SetAsCurrent { get; }

    public GiveWeaponRpcPacket(ElementId elementId, byte weaponType, ushort ammo, bool setAsCurrent)
    {
        this.ElementId = elementId;
        this.WeaponType = weaponType;
        this.Ammo = ammo;
        this.SetAsCurrent = setAsCurrent;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.GIVE_WEAPON);
        builder.Write(this.ElementId);
        builder.WriteWeaponType(this.WeaponType);
        builder.WriteAmmo(this.Ammo);
        builder.Write(this.SetAsCurrent);

        return builder.Build();
    }
}
