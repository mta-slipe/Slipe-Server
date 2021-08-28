using SlipeServer.Packets.Enums;
using SlipeServer.Server.Constants;
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
        public ushort? PreviousUpgradeId { get; set; } = null;
        public ushort? NewUpgradeId { get; set; } = null;

        public VehicleUpgradeChanged(Vehicle vehicle, VehicleUpgradeSlot slot, ushort previousUpgrade, ushort newUpgrade)
        {
            this.Vehicle = vehicle;
            this.Slot = slot;
            this.PreviousUpgrade = previousUpgrade;
            this.NewUpgrade = newUpgrade;
            switch (slot)
            {
                case VehicleUpgradeSlot.Hood:
                    if(previousUpgrade != 0)
                        this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeHood), vehicle.Model, previousUpgrade);
                    if(newUpgrade != 0)
                        this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeHood), vehicle.Model, newUpgrade);
                    break;
                case VehicleUpgradeSlot.Vent:
                    if (previousUpgrade != 0)
                        this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeVent), vehicle.Model, previousUpgrade);
                    if (newUpgrade != 0)
                        this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeVent), vehicle.Model, newUpgrade);
                    break;
                case VehicleUpgradeSlot.Spoiler:
                    if (previousUpgrade != 0)
                        this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeSpoiler), vehicle.Model, previousUpgrade);
                    if (newUpgrade != 0)
                        this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeSpoiler), vehicle.Model, newUpgrade);
                    break;
                case VehicleUpgradeSlot.Sideskirt:
                    break;
                case VehicleUpgradeSlot.FrontBullbars:
                    break;
                case VehicleUpgradeSlot.RearBullbars:
                    break;
                case VehicleUpgradeSlot.Headlights:
                    break;
                case VehicleUpgradeSlot.Roof:
                    break;
                case VehicleUpgradeSlot.Nitro:
                    break;
                case VehicleUpgradeSlot.Hydraulics:
                    break;
                case VehicleUpgradeSlot.Stereo:
                    break;
                case VehicleUpgradeSlot.Unknown:
                    break;
                case VehicleUpgradeSlot.Wheels:
                    break;
                case VehicleUpgradeSlot.Exhaust:
                    break;
                case VehicleUpgradeSlot.FrontBumper:
                    break;
                case VehicleUpgradeSlot.RearBumper:
                    break;
                case VehicleUpgradeSlot.Misc:
                    break;
            }
        }
    }
}
