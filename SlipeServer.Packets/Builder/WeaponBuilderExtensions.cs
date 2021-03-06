using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Builder
{
    public static class WeaponBuilderExtensions
    {
        public static void WriteWeaponType(this PacketBuilder builder, byte weaponType)
        {
            builder.WriteCapped(weaponType, 6);
        }

        public static void WriteWeaponSlot(this PacketBuilder builder, byte weaponSlot)
        {
            builder.WriteCapped(weaponSlot, 4);
        }

        public static void WriteAmmo(this PacketBuilder builder, ushort? total, ushort? inClip = null)
        {
            if (total.HasValue)
                builder.WriteCompressed(total.Value);
            if (inClip.HasValue)
                builder.WriteCompressed(inClip.Value);
        }

    }
}
