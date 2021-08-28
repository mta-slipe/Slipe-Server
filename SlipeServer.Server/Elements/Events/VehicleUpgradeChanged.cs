using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Events
{
    public class VehicleUpgradeChanged : EventArgs
    {
        public Vehicle Vehicle { get; set; }
        public VehicleUpgradeSlot Slot { get; set; }
        public ushort PreviousUpgrade { get; set; }
        public ushort NewUpgrade { get; set; }

        public VehicleUpgradeChanged(Vehicle vehicle, VehicleUpgradeSlot slot, ushort previousUpgrade, ushort newUpgrade)
        {
            this.Vehicle = vehicle;
            this.Slot = slot;
            this.PreviousUpgrade = previousUpgrade;
            this.NewUpgrade = newUpgrade;
        }
    }
}
