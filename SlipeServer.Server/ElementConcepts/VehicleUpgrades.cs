using SlipeServer.Packets.Enums.VehicleUpgrades;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;

namespace SlipeServer.Server.ElementConcepts;

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

    private VehicleUpgradeFrontBullbar frontBullbar = VehicleUpgradeFrontBullbar.None;
    public VehicleUpgradeFrontBullbar FrontBullbar
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

    private VehicleUpgradeRearBullbar rearBullbar = VehicleUpgradeRearBullbar.None;
    public VehicleUpgradeRearBullbar RearBullbar
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


    private VehicleUpgradeLamp lamps = VehicleUpgradeLamp.None;
    public VehicleUpgradeLamp Lamps
    {
        get => this.lamps;
        set
        {
            if (this.Lamps != value && CanHave(value))
            {
                var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Lamps, (ushort)this.Lamps, (ushort)value);
                this.lamps = value;
                UpgradeChanged?.Invoke(this.vehicle, args);
            }
        }
    }

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
            if (this.HasHydraulics != value && CanHaveHydraulics())
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


    private VehicleUpgradeMisc misc = VehicleUpgradeMisc.None;
    public VehicleUpgradeMisc Misc
    {
        get => this.misc;
        set
        {
            if (this.Misc != value && CanHave(value))
            {
                var args = new VehicleUpgradeChanged(this.vehicle, VehicleUpgradeSlot.Misc, (ushort)this.Misc, (ushort)value);
                this.misc = value;
                UpgradeChanged?.Invoke(this.vehicle, args);
            }
        }
    }

    public bool CanHave(VehicleUpgradeHood hood)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeHood), this.vehicle.Model, (ushort)hood) != null;
    }

    public bool CanHave(VehicleUpgradeSpoiler spoiler)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeSpoiler), this.vehicle.Model, (ushort)spoiler) != null;
    }

    public bool CanHave(VehicleUpgradeVent vent)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeVent), this.vehicle.Model, (ushort)vent) != null;
    }

    public bool CanHave(VehicleUpgradeSideskirt sideskirt)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeSideskirt), this.vehicle.Model, (ushort)sideskirt) != null;
    }

    public bool CanHave(VehicleUpgradeFrontBullbar bullbar)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeFrontBullbar), this.vehicle.Model, (ushort)bullbar) != null;
    }

    public bool CanHave(VehicleUpgradeRearBullbar bullbar)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeRearBullbar), this.vehicle.Model, (ushort)bullbar) != null;
    }

    public bool CanHave(VehicleUpgradeLamp lamps)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeLamp), this.vehicle.Model, (ushort)lamps) != null;
    }

    public bool CanHave(VehicleUpgradeRoof bullbar)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeRoof), this.vehicle.Model, (ushort)bullbar) != null;
    }

    public bool CanHave(VehicleUpgradeNitro nitro)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeRoof), this.vehicle.Model, (ushort)nitro) != null;
    }

    public bool CanHave(VehicleUpgradeWheel wheel)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeWheel), this.vehicle.Model, (ushort)wheel) != null;
    }

    public bool CanHave(VehicleUpgradeExhaust exhaust)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeExhaust), this.vehicle.Model, (ushort)exhaust) != null;
    }

    public bool CanHave(VehicleUpgradeFrontBumper frontBumper)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeFrontBumper), this.vehicle.Model, (ushort)frontBumper) != null;
    }

    public bool CanHave(VehicleUpgradeRearBumper rearBumper)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeRearBumper), this.vehicle.Model, (ushort)rearBumper) != null;
    }

    public bool CanHaveStereo()
    {
        return VehicleUpgradesPerModel.AvailiableUpgradesPerVehicleModel[this.vehicle.Model].Contains(1086);
    }

    public bool CanHaveHydraulics()
    {
        return VehicleUpgradesPerModel.AvailiableUpgradesPerVehicleModel[this.vehicle.Model].Contains(1087);
    }

    public bool CanHave(VehicleUpgradeMisc misc)
    {
        return VehicleUpgradeConstants.GetUpgradeIdForVehicle(typeof(VehicleUpgradeMisc), this.vehicle.Model, (ushort)misc) != null;
    }

    public event ElementEventHandler<Vehicle, VehicleUpgradeChanged>? UpgradeChanged;
}
