using System;

namespace SlipeServer.Packets.Reader
{
    public static class WeaponReaderExtensions
    {
        public static byte GetWeaponType(this PacketReader reader) => reader.GetByteCapped(6);
        public static byte GetWeaponSlot(this PacketReader reader) => reader.GetByteCapped(4);

        public static Tuple<ushort, ushort> GetAmmoTuple(this PacketReader reader, bool ammoInClip = false)
        {
            if (ammoInClip)
            {
                return new Tuple<ushort, ushort>(reader.GetCompressedUint16(), reader.GetCompressedUint16());
            }
            return new Tuple<ushort, ushort>(reader.GetCompressedUint16(), 0);
        }

        public static ushort GetAmmo(this PacketReader reader) => reader.GetCompressedUint16();
    }
}
