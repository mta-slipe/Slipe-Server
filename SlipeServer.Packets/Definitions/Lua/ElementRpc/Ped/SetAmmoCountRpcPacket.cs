using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public sealed class SetAmmoCountRpcPacket(ElementId elementId, byte weaponType, ushort ammo, ushort? ammoInClip) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; } = elementId;
    public byte WeaponType { get; } = weaponType;
    public ushort Ammo { get; } = ammo;
    public ushort? AmmoInClip { get; } = ammoInClip;

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_WEAPON_AMMO);
        builder.Write(this.ElementId);
        builder.WriteWeaponType(this.WeaponType);
        builder.WriteAmmo(this.Ammo, this.AmmoInClip);

        return builder.Build();
    }
}
