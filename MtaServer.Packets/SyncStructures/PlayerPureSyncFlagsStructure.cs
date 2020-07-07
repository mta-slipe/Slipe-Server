using MtaServer.Packets.Builder;
using MtaServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Structures
{
    public class PlayerPureSyncFlagsStructure : ISyncStructure
    {
        public bool IsInWater { get; set; }
        public bool IsOnGround { get; set; }
        public bool HasJetpack { get; set; }
        public bool IsDucked { get; set; }
        public bool WearsGoggles { get; set; }
        public bool HasContact { get; set; }
        public bool IsChoking { get; set; }
        public bool AkimboTargetUp { get; set; }
        public bool IsOnFire { get; set; }
        public bool HasAWeapon { get; set; }
        public bool IsSyncingVelocity { get; set; }
        public bool IsStealthAiming { get; set; }


        public PlayerPureSyncFlagsStructure()
        {

        }

        public PlayerPureSyncFlagsStructure(
            bool isInWater,
            bool isOnGround,
            bool hasJetpack,
            bool isDucked,
            bool wearsGoggles,
            bool hasContact,
            bool isChoking,
            bool akimboTargetUp,
            bool isOnFire,
            bool hasAWeapon,
            bool isSyncingVelocity,
            bool isStealthAiming
        )
        {
            IsInWater = isInWater;
            IsOnGround = isOnGround;
            HasJetpack = hasJetpack;
            IsDucked = isDucked;
            WearsGoggles = wearsGoggles;
            HasContact = hasContact;
            IsChoking = isChoking;
            AkimboTargetUp = akimboTargetUp;
            IsOnFire = isOnFire;
            HasAWeapon = hasAWeapon;
            IsSyncingVelocity = isSyncingVelocity;
            IsStealthAiming = isStealthAiming;
        }

        public void Read(PacketReader reader)
        {
            AkimboTargetUp = reader.GetBit();
            IsChoking = reader.GetBit();
            HasContact = reader.GetBit();
            WearsGoggles = reader.GetBit();

            IsDucked = reader.GetBit();
            HasJetpack = reader.GetBit();
            IsOnGround = reader.GetBit();
            IsInWater = reader.GetBit();

            IsStealthAiming = reader.GetBit();
            IsSyncingVelocity = reader.GetBit();
            HasAWeapon = reader.GetBit();
            IsOnFire = reader.GetBit();
        }

        public void Write(PacketBuilder builder)
        {
            builder.Write(AkimboTargetUp);
            builder.Write(IsChoking);
            builder.Write(HasContact);
            builder.Write(WearsGoggles);

            builder.Write(IsDucked);
            builder.Write(HasJetpack);
            builder.Write(IsOnGround);
            builder.Write(IsInWater);

            builder.Write(IsStealthAiming);
            builder.Write(IsSyncingVelocity);
            builder.Write(HasAWeapon);
            builder.Write(IsOnFire);
        }
    }
}
