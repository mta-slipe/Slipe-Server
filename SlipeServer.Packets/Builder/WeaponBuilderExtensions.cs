using SlipeServer.Packets.Structs;

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

        public static void Write(this PacketBuilder builder, MapInfoWeaponConfiguration weaponConfiguration)
        {
            builder.Write(weaponConfiguration.WeaponType);
            builder.Write(weaponConfiguration.TargetRange);
            builder.Write(weaponConfiguration.WeaponRange);
            builder.Write(weaponConfiguration.Flags);
            builder.Write(weaponConfiguration.MaximumClipAmmo);
            builder.Write(weaponConfiguration.Damage);
            builder.Write(weaponConfiguration.Accuracy);
            builder.Write(weaponConfiguration.MoveSpeed);
            builder.Write(weaponConfiguration.AnimationLoopStart);
            builder.Write(weaponConfiguration.AnimationLoopStop);
            builder.Write(weaponConfiguration.AnimationLoopBulletFire);
            builder.Write(weaponConfiguration.Animation2LoopStart);
            builder.Write(weaponConfiguration.Animation2LoopStop);
            builder.Write(weaponConfiguration.Animation2LoopBulletFire);
            builder.Write(weaponConfiguration.AnimationBreakoutTime);
        }
    }
}
