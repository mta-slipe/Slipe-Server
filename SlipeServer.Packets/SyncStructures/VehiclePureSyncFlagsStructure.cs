using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Structures
{
    public class VehiclePureSyncFlagsStructure : ISyncStructure
    {
        public bool IsWearingGoggles { get; set; }
        public bool IsDoingGangDriveby { get; set; }
        public bool IsSirenOrAlarmActive { get; set; }
        public bool IsSmokeTrailEnabled { get; set; }
        public bool IsLandingGearDown { get; set; }
        public bool IsOnGround { get; set; }
        public bool IsInWater { get; set; }
        public bool IsDerailed { get; set; }
        public bool IsAircraft { get; set; }
        public bool HasAWeapon { get; set; }
        public bool IsHeliSearchLightVisible { get; set; }


        public VehiclePureSyncFlagsStructure()
        {

        }

        public VehiclePureSyncFlagsStructure(
            bool isWearingGoggles,
            bool isDoingGangDriveby,
            bool isSirenOrAlarmActive,
            bool isSmokeTrailEnabled,
            bool isLandingGearDown,
            bool isOnGround,
            bool isInWater,
            bool isDreailed,
            bool isAircraft,
            bool hasAWeapon,
            bool isHeliSearchLightVisible
        )
        {
            IsWearingGoggles = isWearingGoggles;
            IsDoingGangDriveby = isDoingGangDriveby;
            IsSirenOrAlarmActive = isSirenOrAlarmActive;
            IsSmokeTrailEnabled = isSmokeTrailEnabled;
            IsLandingGearDown = isLandingGearDown;
            IsOnGround = isOnGround;
            IsInWater = isInWater;
            IsDerailed = isDreailed;
            IsAircraft = isAircraft;
            HasAWeapon = hasAWeapon;
            IsHeliSearchLightVisible = isHeliSearchLightVisible;
        }

        public void Read(PacketReader reader)
        {
            IsDerailed = reader.GetBit();
            IsInWater = reader.GetBit();
            IsOnGround = reader.GetBit();
            IsLandingGearDown = reader.GetBit();

            IsSmokeTrailEnabled = reader.GetBit();
            IsSirenOrAlarmActive = reader.GetBit();
            IsDoingGangDriveby = reader.GetBit();
            IsWearingGoggles = reader.GetBit();

            IsHeliSearchLightVisible = reader.GetBit();
            HasAWeapon = reader.GetBit();
            IsAircraft = reader.GetBit();
        }

        public void Write(PacketBuilder builder)
        {
            builder.Write(IsDerailed);
            builder.Write(IsInWater);
            builder.Write(IsOnGround);
            builder.Write(IsLandingGearDown);

            builder.Write(IsSmokeTrailEnabled);
            builder.Write(IsSirenOrAlarmActive);
            builder.Write(IsDoingGangDriveby);
            builder.Write(IsWearingGoggles);

            builder.Write(IsHeliSearchLightVisible);
            builder.Write(HasAWeapon);
            builder.Write(IsAircraft);
        }
    }
}
