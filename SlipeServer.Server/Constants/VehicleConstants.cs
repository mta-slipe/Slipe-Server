using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Constants
{
    public class VehicleConstants
    {
        public static HashSet<ushort> TrailerModels = new HashSet<ushort>()
        {
            435, 450, 591, 606, 607, 608, 610, 584, 611
        };

        public static HashSet<ushort> WaterEntryVehicles = new HashSet<ushort>()
        {
            417, 539, 460, 447
        };

        public static Dictionary<ushort, byte> SeatsPerVehicle = new Dictionary<ushort, byte>()
        {
            [602] = 4
        };
    }
}
