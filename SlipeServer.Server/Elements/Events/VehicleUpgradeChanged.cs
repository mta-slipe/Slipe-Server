using SlipeServer.Packets.Enums.VehicleUpgrades;
using SlipeServer.Server.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Events;

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
                if (previousUpgrade != 0)
                    this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeSideskirt), vehicle.Model, previousUpgrade);
                if (newUpgrade != 0)
                    this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeSideskirt), vehicle.Model, newUpgrade);
                break;
            case VehicleUpgradeSlot.FrontBullbars:
                if (previousUpgrade != 0)
                    this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeBullbar), vehicle.Model, previousUpgrade);
                if (newUpgrade != 0)
                    this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeBullbar), vehicle.Model, newUpgrade);
                break;
            case VehicleUpgradeSlot.RearBullbars:
                if (previousUpgrade != 0)
                    this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeBullbar), vehicle.Model, previousUpgrade);
                if (newUpgrade != 0)
                    this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeBullbar), vehicle.Model, newUpgrade);
                break;
            case VehicleUpgradeSlot.Headlights:
                break;
            case VehicleUpgradeSlot.Roof:
                if (previousUpgrade != 0)
                    this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeRoof), vehicle.Model, previousUpgrade);
                if (newUpgrade != 0)
                    this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeRoof), vehicle.Model, newUpgrade);
                break;
            case VehicleUpgradeSlot.Nitro:
                if (previousUpgrade != 0)
                    this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeNitro), vehicle.Model, previousUpgrade);
                if (newUpgrade != 0)
                    this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeNitro), vehicle.Model, newUpgrade);
                break;
            case VehicleUpgradeSlot.Wheels:
                if (previousUpgrade != 0)
                    this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeWheel), vehicle.Model, previousUpgrade);
                if (newUpgrade != 0)
                    this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeWheel), vehicle.Model, newUpgrade);
                break;
            case VehicleUpgradeSlot.Exhaust:
                if (previousUpgrade != 0)
                    this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeExhaust), vehicle.Model, previousUpgrade);
                if (newUpgrade != 0)
                    this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeExhaust), vehicle.Model, newUpgrade);
                break;
            case VehicleUpgradeSlot.FrontBumper:
                if (previousUpgrade != 0)
                    this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeFrontBumper), vehicle.Model, previousUpgrade);
                if (newUpgrade != 0)
                    this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeFrontBumper), vehicle.Model, newUpgrade);
                break;
            case VehicleUpgradeSlot.RearBumper:
                if (previousUpgrade != 0)
                    this.PreviousUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeRearBumper), vehicle.Model, previousUpgrade);
                if (newUpgrade != 0)
                    this.NewUpgradeId = VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeRearBumper), vehicle.Model, newUpgrade);
                break;
            case VehicleUpgradeSlot.Hydraulics:
            case VehicleUpgradeSlot.Stereo:
            case VehicleUpgradeSlot.Unknown:
            case VehicleUpgradeSlot.Misc:
                throw new NotImplementedException();
                break;
        }
    }
}
