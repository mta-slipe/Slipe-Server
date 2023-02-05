using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class TakeWeaponRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public byte WeaponType { get; }
    public ushort? Ammo { get; }

    public TakeWeaponRpcPacket(ElementId elementId, byte weaponType, ushort? ammo)
    {
        this.ElementId = elementId;
        this.WeaponType = weaponType;
        this.Ammo = ammo;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.TAKE_WEAPON);
        builder.Write(this.ElementId);
        builder.WriteWeaponType(this.WeaponType);
        if (this.Ammo != null)
            builder.WriteAmmo(this.Ammo.Value);

        return builder.Build();
    }
}
