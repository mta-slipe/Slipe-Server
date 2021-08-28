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
        public ushort? PreviousUpgrade { get; set; } = null;
        public ushort? NewUpgrade { get; set; } = null;
        public ushort? PreviousUpgradeId { get; set; } = null;
        public ushort? NewUpgradeId { get; set; } = null;

        public VehicleUpgradeChanged(Vehicle vehicle, VehicleUpgradeSlot slot, ushort? previousUpgrade = null, ushort? newUpgrade = null)
        {
            this.Vehicle = vehicle;
            this.Slot = slot;
            this.PreviousUpgrade = previousUpgrade;
            this.NewUpgrade = newUpgrade;
            switch (slot)
            {
                case VehicleUpgradeSlot.Hood:
                    if(previousUpgrade.HasValue)
                        this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeHood), vehicle.Model, previousUpgrade.Value);
                    if(newUpgrade.HasValue)
                        this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeHood), vehicle.Model, newUpgrade.Value);
                    break;
                case VehicleUpgradeSlot.Vent:
                    if (previousUpgrade.HasValue)
                        this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeVent), vehicle.Model, previousUpgrade.Value);
                    if (newUpgrade.HasValue)
                        this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeVent), vehicle.Model, newUpgrade.Value);
                    break;
                case VehicleUpgradeSlot.Spoiler:
                    if (previousUpgrade.HasValue)
                        this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeSpoiler), vehicle.Model, previousUpgrade.Value);
                    if (newUpgrade.HasValue)
                        this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeSpoiler), vehicle.Model, newUpgrade.Value);
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
