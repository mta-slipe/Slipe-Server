using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Structures
{
    public class KeySyncFlagsStructure : ISyncStructure
    {
        public bool IsDucked { get; set; }
        public bool IsChoking { get; set; }
        public bool AkimboTargetUp { get; set; }
        public bool IsSyncingVehicle { get; set; }


        public KeySyncFlagsStructure()
        {

        }

        public KeySyncFlagsStructure(
            bool isDucked,
            bool isChoking,
            bool akimboTargetUp,
            bool isSyncingVehicle
        )
        {
            IsDucked = isDucked;
            IsChoking = isChoking;
            AkimboTargetUp = akimboTargetUp;
            IsSyncingVehicle = isSyncingVehicle;
        }

        public void Read(PacketReader reader)
        {
            IsSyncingVehicle = reader.GetBit();
            AkimboTargetUp = reader.GetBit();
            IsChoking = reader.GetBit();
            IsDucked = reader.GetBit();
        }

        public void Write(PacketBuilder builder)
        {
            builder.Write(IsSyncingVehicle);
            builder.Write(AkimboTargetUp);
            builder.Write(IsChoking);
            builder.Write(IsDucked);
        }
    }
}
