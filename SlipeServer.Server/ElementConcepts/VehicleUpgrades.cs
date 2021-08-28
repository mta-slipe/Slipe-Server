using SlipeServer.Packets.Enums;
using SlipeServer.Server.Constants;
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

        public byte[] Bytes { get; set; } = new byte[] { };
        private VehicleUpgradeHood hood = VehicleUpgradeHood.None;
        public VehicleUpgradeHood Hood
        {
            get => this.hood;
            set
            {
                if (this.Hood != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Hood, (ushort)this.Hood, (ushort)value);
                    this.hood = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        private VehicleUpgradeVent vent = VehicleUpgradeVent.None;
        public VehicleUpgradeVent Vent
        {
            get => this.vent;
            set
            {
                if (this.Vent != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Vent, (ushort)this.Vent, (ushort)value);
                    this.vent = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        private VehicleUpgradeSpoiler spoiler = VehicleUpgradeSpoiler.None;
        public VehicleUpgradeSpoiler Spoiler
        {
            get => this.spoiler;
            set
            {
                if (this.Spoiler != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Spoiler, (ushort)this.Spoiler, (ushort)value);
                    this.spoiler = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        private VehicleUpgradeSideskirt sideskirt = VehicleUpgradeSideskirt.None;
        public VehicleUpgradeSideskirt Sideskirt
        {
            get => this.sideskirt;
            set
            {
                if (this.Sideskirt != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Sideskirt, (ushort)this.Sideskirt, (ushort)value);
                    this.sideskirt = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        private VehicleUpgradeBullbar frontBullbar = VehicleUpgradeBullbar.None;
        public VehicleUpgradeBullbar FrontBullbar
        {
            get => this.frontBullbar;
            set
            {
                if (this.FrontBullbar != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.FrontBullbars, (ushort)this.FrontBullbar, (ushort)value);
                    this.frontBullbar = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        private VehicleUpgradeBullbar rearBullbar = VehicleUpgradeBullbar.None;
        public VehicleUpgradeBullbar RearBullbar
        {
            get => this.rearBullbar;
            set
            {
                if (this.RearBullbar != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.RearBullbars, (ushort)this.RearBullbar, (ushort)value);
                    this.rearBullbar = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        //public VehicleUpgradeHood Headlights { get; set; }

        private VehicleUpgradeRoof roof = VehicleUpgradeRoof.None;
        public VehicleUpgradeRoof Roof
        {
            get => this.roof;
            set
            {
                if (this.Roof != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Roof, (ushort)this.Roof, (ushort)value);
                    this.roof = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        private VehicleUpgradeNitro nitro = VehicleUpgradeNitro.None;
        public VehicleUpgradeNitro Nitro
        {
            get => this.nitro;
            set
            {
                if (this.Nitro != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Nitro, (ushort)this.Nitro, (ushort)value);
                    this.nitro = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }


        private bool hasHydraulics = false;
        public bool HasHydraulics
        {
            get => this.hasHydraulics;
            set
            {
                if (this.HasHydraulics != value && CanHaveHydralics())
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Nitro, (ushort)(this.HasHydraulics ? 0 : 1087), (ushort)(value ? 1087 : 0));
                    this.hasHydraulics = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        private bool hasStereo = false;
        public bool HasStereo
        {
            get => this.hasStereo;
            set
            {
                if (this.HasStereo != value && CanHaveStereo())
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Nitro, (ushort)(this.HasStereo ? 0 : 1086), (ushort)(value ? 1086 : 0));
                    this.hasStereo = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        //public VehicleUpgradeHood Unknown { get; set; }

        private VehicleUpgradeWheel wheel = VehicleUpgradeWheel.None;
        public VehicleUpgradeWheel Wheels
        {
            get => this.wheel;
            set
            {
                if (this.Wheels != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Wheels, (ushort)this.Wheels, (ushort)value);
                    this.wheel = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        private VehicleUpgradeExhaust exhaust = VehicleUpgradeExhaust.None;
        public VehicleUpgradeExhaust Exhaust
        {
            get => this.exhaust;
            set
            {
                if (this.Exhaust != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Wheels, (ushort)this.Exhaust, (ushort)value);
                    this.exhaust = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        private VehicleUpgradeFrontBumper frontBumper = VehicleUpgradeFrontBumper.None;
        public VehicleUpgradeFrontBumper FrontBumper
        {
            get => this.frontBumper;
            set
            {
                if (this.FrontBumper != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Wheels, (ushort)this.FrontBumper, (ushort)value);
                    this.frontBumper = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        private VehicleUpgradeRearBumper rearBumper = VehicleUpgradeRearBumper.None;
        public VehicleUpgradeRearBumper RearBumper
        {
            get => this.rearBumper;
            set
            {
                if (this.RearBumper != value && CanHave(value))
                {
                    var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.RearBumper, (ushort)this.RearBumper, (ushort)value);
                    this.rearBumper = value;
                    UpgradeChanged?.Invoke(this.vehicle, args);
                }
            }
        }

        //public VehicleUpgradeHood Misc { get; set; }

        public bool CanHave(VehicleUpgradeHood hood)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeHood), this.vehicle.Model, (ushort)hood) != null;
        }

        public bool CanHave(VehicleUpgradeSpoiler spoiler)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeSpoiler), this.vehicle.Model, (ushort)spoiler) != null;
        }

        public bool CanHave(VehicleUpgradeVent vent)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeVent), this.vehicle.Model, (ushort)vent) != null;
        }

        public bool CanHave(VehicleUpgradeSideskirt sideskirt)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeSideskirt), this.vehicle.Model, (ushort)sideskirt) != null;
        }

        public bool CanHave(VehicleUpgradeBullbar bullbar)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeBullbar), this.vehicle.Model, (ushort)bullbar) != null;
        }

        public bool CanHave(VehicleUpgradeRoof bullbar)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeRoof), this.vehicle.Model, (ushort)bullbar) != null;
        }

        public bool CanHave(VehicleUpgradeNitro nitro)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeRoof), this.vehicle.Model, (ushort)nitro) != null;
        }

        public bool CanHave(VehicleUpgradeWheel wheel)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeWheel), this.vehicle.Model, (ushort)wheel) != null;
        }

        public bool CanHave(VehicleUpgradeExhaust exhaust)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeExhaust), this.vehicle.Model, (ushort)exhaust) != null;
        }

        public bool CanHave(VehicleUpgradeFrontBumper frontBumper)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeFrontBumper), this.vehicle.Model, (ushort)frontBumper) != null;
        }

        public bool CanHave(VehicleUpgradeRearBumper rearBumper)
        {
            return VehicleConstants.UpgradeVehicleUpgradeId(typeof(VehicleUpgradeRearBumper), this.vehicle.Model, (ushort)rearBumper) != null;
        }

        public bool CanHaveStereo()
        {
            return VehicleConstants.AvailiableUpgrades[this.vehicle.Model].Contains(1086);
        }

        public bool CanHaveHydralics()
        {
            return VehicleConstants.AvailiableUpgrades[this.vehicle.Model].Contains(1087);
        }

        public event ElementEventHandler<Vehicle, VehicleUpgradeChanged>? UpgradeChanged;
    }
}
