using System;
using System.Collections.Generic;

namespace SlipeServer.Packets.Constants
{
    public static class VehicleConstants
    {
        public static HashSet<int> VehiclesWithTurrets = new HashSet<int>()
        {
            432, 407, 601, 
        };
        public static HashSet<int> VehiclesWithAdjustableProperties = new HashSet<int>() 
        { 
            406, 443, 486, 520, 525, 530, 531, 592, 
        };
    }
}
