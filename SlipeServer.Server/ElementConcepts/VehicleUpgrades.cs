using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.ElementConcepts
{
    public class VehicleUpgrades
    {
        private readonly Vehicle vehicle;

        public VehicleUpgrades(Vehicle vehicle)
        {
            this.vehicle = vehicle;
        }
        private VehicleUpgradeHood hood = VehicleUpgradeHood.None;
        public VehicleUpgradeHood Hood
        {
            get => this.hood;
            set
            {
                if(CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, 0, (ushort)this.Hood, (ushort)value);
                    this.hood = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }
        public VehicleUpgradeVent Vent { get; set; }
        public VehicleUpgradeSpoiler Spoiler { get; set; }
        public VehicleUpgradeSideskirt Sideskirt { get; set; }
        public VehicleUpgradeBullbar FrontBullbar { get; set; }
        public VehicleUpgradeBullbar RearBullbar { get; set; }
        //public VehicleUpgradeHood Headlights { get; set; }
        public VehicleUpgradeRoof Roof { get; set; }
        public VehicleUpgradeNitro Nitro { get; set; }
        public bool HasHydraulics { get; set; }
        public bool HasStereo { get; set; }
        //public VehicleUpgradeHood Unknown { get; set; }
        public VehicleUpgradeWheel Wheels { get; set; }
        public VehicleUpgradeHood Exhaust { get; set; }
        public VehicleUpgradeHood FrontBumper { get; set; }
        public VehicleUpgradeHood RearBumper { get; set; }
        public VehicleUpgradeHood Misc { get; set; }

        public bool CanHave(VehicleUpgradeHood hood)
        {
            return true;
        }

        public event ElementEventHandler<Vehicle, VehicleUpgradeChanged>? UpgradeChanged;
    }
}
