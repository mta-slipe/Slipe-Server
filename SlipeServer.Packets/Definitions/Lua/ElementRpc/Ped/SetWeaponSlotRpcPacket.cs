using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class SetWeaponSlotRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public byte WeaponSlot { get; }

    public SetWeaponSlotRpcPacket(ElementId elementId, byte weaponSlot)
    {
        this.ElementId = elementId;
        this.WeaponSlot = weaponSlot;
    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_WEAPON_SLOT);
        builder.Write(this.ElementId);
        builder.WriteWeaponSlot(this.WeaponSlot);

        return builder.Build();
    }
}
