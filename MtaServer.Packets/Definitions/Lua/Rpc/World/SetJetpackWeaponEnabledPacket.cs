using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetJetpackWeaponEnabledPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;
        public byte WeaponType { get; set; }
        public bool Enabled { get; set; }

        public SetJetpackWeaponEnabledPacket(byte weaponType,bool enabled)
        {
            WeaponType = weaponType;
            Enabled = enabled;
        }
        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_JETPACK_WEAPON_ENABLED);
            builder.Write(this.WeaponType);
            builder.Write(this.Enabled);

            return builder.Build();
        }
    }
}
