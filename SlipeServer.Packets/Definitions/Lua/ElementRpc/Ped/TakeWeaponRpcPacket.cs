using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped
{
    public class TakeWeaponRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; }
        public byte WeaponType { get; }
        public ushort? Ammo { get; }

        public TakeWeaponRpcPacket(uint elementId, byte weaponType, ushort? ammo)
        {
            ElementId = elementId;
            WeaponType = weaponType;
            Ammo = ammo;
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.TAKE_WEAPON);
            builder.WriteElementId(this.ElementId);
            builder.WriteWeaponType(this.WeaponType);
            if (this.Ammo != null)
                builder.WriteAmmo(this.Ammo.Value);

            return builder.Build();
        }
    }
}
