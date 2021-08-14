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
            this.IsWearingGoggles = isWearingGoggles;
            this.IsDoingGangDriveby = isDoingGangDriveby;
            this.IsSirenOrAlarmActive = isSirenOrAlarmActive;
            this.IsSmokeTrailEnabled = isSmokeTrailEnabled;
            this.IsLandingGearDown = isLandingGearDown;
            this.IsOnGround = isOnGround;
            this.IsInWater = isInWater;
            this.IsDerailed = isDreailed;
            this.IsAircraft = isAircraft;
            this.HasAWeapon = hasAWeapon;
            this.IsHeliSearchLightVisible = isHeliSearchLightVisible;
        }

        public void Read(PacketReader reader)
        {
            this.IsDerailed = reader.GetBit();
            this.IsInWater = reader.GetBit();
            this.IsOnGround = reader.GetBit();
            this.IsLandingGearDown = reader.GetBit();

            this.IsSmokeTrailEnabled = reader.GetBit();
            this.IsSirenOrAlarmActive = reader.GetBit();
            this.IsDoingGangDriveby = reader.GetBit();
            this.IsWearingGoggles = reader.GetBit();

            this.IsHeliSearchLightVisible = reader.GetBit();
            this.HasAWeapon = reader.GetBit();
            this.IsAircraft = reader.GetBit();
        }

        public void Write(PacketBuilder builder)
        {
            builder.Write(this.IsDerailed);
            builder.Write(this.IsInWater);
            builder.Write(this.IsOnGround);
            builder.Write(this.IsLandingGearDown);

            builder.Write(this.IsSmokeTrailEnabled);
            builder.Write(this.IsSirenOrAlarmActive);
            builder.Write(this.IsDoingGangDriveby);
            builder.Write(this.IsWearingGoggles);

            builder.Write(this.IsHeliSearchLightVisible);
            builder.Write(this.HasAWeapon);
            builder.Write(this.IsAircraft);
        }
    }
}
