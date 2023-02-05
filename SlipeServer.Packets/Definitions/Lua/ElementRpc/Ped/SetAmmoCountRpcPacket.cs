using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class SetAmmoCountRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public byte WeaponType { get; }
    public ushort Ammo { get; }
    public ushort? AmmoInClip { get; }

    public SetAmmoCountRpcPacket(ElementId elementId, byte weaponType, ushort ammo, ushort? ammoInClip)
    {
        this.ElementId = elementId;
        this.WeaponType = weaponType;
        this.Ammo = ammo;
        this.AmmoInClip = ammoInClip;
    }

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
