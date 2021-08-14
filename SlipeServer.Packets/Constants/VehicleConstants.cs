using System;
using System.Collections.Generic;

namespace SlipeServer.Packets.Constants
{
    public static class VehicleConstants
    {
        public static HashSet<int> VehiclesWithTurrets { get; } = new()
        {
            407,
            432,
            601,
        };

        public static HashSet<int> VehiclesWithAdjustableProperties { get; } = new()
        {
            406, 443, 486, 520, 524, 525, 530, 531, 592,

        };

        public static HashSet<int> VehiclesWithDoors { get; } = new()
        {

        };
    }
}
